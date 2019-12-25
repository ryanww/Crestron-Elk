using System;
using System.Text;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using TCP_Client;


namespace ElkAlarm
{
    public class ElkPanel
    {
        private CrestronQueue<string> commandQueue;
        private CrestronQueue<string> responseQueue;
        private CTimer commandQueueTimer;
        private CTimer responseQueueTimer;
        private TCPClientDevice client;
        
        public bool debug;
        private int panelId;
        public int getPanelId { get { return this.panelId; } }
        private string panelIp = "";
        public string getPanelIp { get { return this.panelIp; } }
        private int panelPort = 0;
        private bool isInitialized;
        private bool isConnected = false;
        public bool IsConnected { get { return this.isConnected; } }
        private bool initRun = false;

        private string pwCode;

        internal static Dictionary<int, ElkArea> Areas = new Dictionary<int, ElkArea>();
        internal static Dictionary<int, ElkZone> Zones = new Dictionary<int, ElkZone>();

        //Initialize
        public bool Initialize(int _panelId, string _host, ushort _port)
        {
            if (this.initRun)
                return false;

            panelId = _panelId;

            this.SendDebug(string.Format("Added Panel {0} @ {1}:{2} & initialize", _panelId, _host, _port));

            for (int i = 0; i < 8; i++)
            {
                if (!Areas.ContainsKey(i))
                {
                    ElkArea a = new ElkArea();
                    a.Initialize(this, i);
                    Areas.Add(i, a);
                }
            }
            for (int i = 0; i < 208; i++)
            {
                if (!Zones.ContainsKey(i))
                {
                    ElkZone z = new ElkZone();
                    z.Initialize(this, i);
                    Zones.Add(i, z);
                }
            }
            commandQueue = new CrestronQueue<string>();
            responseQueue = new CrestronQueue<string>();

            this.panelId = _panelId;
            this.panelIp = _host;
            this.panelPort = _port;
            
            
            if (this.commandQueueTimer == null)
                this.commandQueueTimer = new CTimer(CommandQueueDequeue, null, 0, 50);

            if (this.responseQueueTimer == null)
                this.responseQueueTimer = new CTimer(ResponseQueueDequeue, null, 0, 50);

            if (this.isConnected == false)
                this.InitialzeConnection();
            this.initRun = true;

            return true;
        }

        public void InitialzeConnection()
        {
            if (this.panelIp.Length > 0)
            {
                this.client = new TCPClientDevice();
                this.client.ID = 1;
                this.client.ConnectionStatus += new StatusEventHandler(client_ConnectionStatus);
                this.client.ResponseString += new ResponseEventHandler(client_ResponseString);
                this.client.Connect(this.panelIp, (ushort)this.panelPort);
            }
        }

        public void InitializePanelParameters()
        {
            this.Enqueue("zs00");//Zone status request
            this.isInitialized = true;
        }


        //Comms --------------------------------------------------------------
        private void CommandQueueDequeue(object o)
        {
            if (!commandQueue.IsEmpty)
            {
                var data = commandQueue.Dequeue();
                data = generateChecksumString(data);
                SendDebug(string.Format("Elk - Sending from queue: {0}", data));
                client.SendCommand(data + "\x0D\x0A");
            }
        }
        StringBuilder RxData = new StringBuilder();
        bool busy = false;
        int Pos = -1;
        private void ResponseQueueDequeue(object o)
        {
            if (!responseQueue.IsEmpty)
            {
                try
                {
                    // removes string from queue, blocks until an item is queued
                    string tmpString = responseQueue.Dequeue();

                    RxData.Append(tmpString); //Append received data to the COM buffer

                    if (!busy)
                    {
                        busy = true;
                        while (RxData.ToString().Contains("\x0D\x0A"))
                        {
                            Pos = RxData.ToString().IndexOf("\x0D\x0A");
                            var data = RxData.ToString().Substring(0, Pos);
                            var garbage = RxData.Remove(0, Pos + 2); // remove data from COM buffer
                            CrestronConsole.PrintLine("send to parse " + data);
                            ParseInternalResponse(data);
                        }

                        busy = false;
                    }
                }
                catch (Exception e)
                {
                    busy = false;
                    ErrorLog.Exception(e.Message, e);
                }
            }
        }
        internal void Enqueue(string data)
        {
            commandQueue.Enqueue(data);
        }
        private void ParseResponse(string data)
        {
            try
            {
                SendDebug(string.Format("Elk - Received and adding to queue: {0}", data));
                responseQueue.Enqueue(data);
            }
            catch (Exception e)
            {
            }
        }
        void client_ResponseString(string response, int id)
        {
            ParseResponse(response);
        }
        void client_ConnectionStatus(int status, int id)
        {
            if (status == 2 && !isConnected)
            {
                this.isConnected = true;
                foreach (var item in SimplClients)
                {
                    item.Value.Fire(new SimplEventArgs(eElkSimplEventIds.IsConnected, "true", 1));
                }
                CrestronEnvironment.Sleep(1500);

                foreach (var item in SimplClients)
                {
                    item.Value.Fire(new SimplEventArgs(eElkSimplEventIds.IsRegistered, "true", 1));
                }
                this.InitializePanelParameters();
            }
            else if (isConnected && status != 2)
            {
                this.SendDebug("Elk Disconnected");
                this.isConnected = false;
                //foreach (var item in SimplClients)
                //{
                //    //item.Value.Fire(new SimplEventArgs(eQscSimplEventIds.IsRegistered, "false", 0));
                //    //item.Value.Fire(new SimplEventArgs(eQscSimplEventIds.IsConnected, "false", 0));
                //}
            }
        }
        private string generateChecksumString(string s)
        {
            s = String.Format("{0:X2}{1}", s.Length + 2, s);
            int sum = 0;
            foreach (char c in s)
                sum += (int)c;
            sum = -(sum % 256);
            s = String.Format("{0}{1}",s,((byte)sum).ToString("X2"));
            return s;
        }





        //Parsing
        private void ParseInternalResponse(string returnString)
        {
            if (returnString.Length > 0)
            {
                string ps = returnString.Substring(2);
                int zoneIndex;
                try
                {
                    switch (ps.Substring(0, 2))
                    {
                        case "ZS": //All Zone Status
                            SendDebug("Got ZS");
                            char[] zsArray;
                            zsArray = ps.Substring(2, 208).ToCharArray();
                            for (int i = 0; i < zsArray.Length; i++)
                                Zones[i].internalSetZoneStatus(zsArray[i] - 48);
                            break;

                        case "ZC": //Single Zone Change
                            SendDebug("Got ZS");
                            zoneIndex = int.Parse(ps.Substring(2, 3)) - 1;
                            int zoneStatus = ps.ToCharArray()[5]-48;
                            Zones[zoneIndex].internalSetZoneStatus(zoneStatus);
                            break;
                        
                        case "ZD": //Zone Definitions
                            SendDebug("Got ZD");
                            char[] zdArray;
                            zdArray = ps.Substring(4, 208).ToCharArray();
                            for (int i = 0; i < zdArray.Length; i++)
                                Zones[i].internalSetZoneDefinition(zdArray[i] - 48);
                            break;

                        case "ZB":
                            SendDebug("Got ZB");
                            zoneIndex = int.Parse(ps.Substring(2, 3)) - 1;
                            if (
                            bool zoneBypass = Convert.ToBoolean(ps.Substring(5,1));
                            Zones[zoneIndex].internalSetBypass(zoneBypass);
                            break;

                        case "ZV":
                            SendDebug("Got ZV");
                            zoneIndex = int.Parse(ps.Substring(2, 3))-1;
                            double zoneVoltage = int.Parse(ps.Substring(5, 3));
                            zoneVoltage /= 10;
                            if (zoneIndex < Zones.Count)
                                Zones[zoneIndex].internalSetZoneVoltage(zoneVoltage);
                            break;

                        case "ZP": //Zone Partition Request 06zp0050(CR-LF)
                            SendDebug("Got ZP");
                            char[] zpArray;
                            zpArray = ps.Substring(2, 208).ToCharArray();
                            for (int i = 0; i < zpArray.Length; i++)
                                Zones[i].internalSetZoneArea(zpArray[i] - 48);
                            break;

                        case "AS": //Area Status
                            SendDebug("Got AS");
                            string armedStatusString = ps.Substring(2, 8);
                            string armUpStateString = ps.Substring(10, 8);
                            string alarmStateString = ps.Substring(18, 8);
                            for (int i = 0; i < 8; i++)
                            {
                                Areas[i].internalSetAreaArmedStatus(armedStatusString.ToCharArray()[i-48]);
                                Areas[i].internalSetAreaArmUpState(armUpStateString.ToCharArray()[i-48]);
                                Areas[i].internalSetAreaAlarmState(alarmStateString.ToCharArray()[i-48]);
                            }
                            break;

                        case "SD":
                            int itemType = int.Parse(ps.Substring(2, 2));
                            int itemIndex = int.Parse(ps.Substring(4, 3));
                            int itemIndexZero = itemIndex - 1;
                            string itemText = ps.Substring(7, 16); //may need to mask out high bit

                            switch (itemType)
                            {
                                case 0:// = Zone Name
                                    if (itemIndexZero < Zones.Count)
                                        Zones[itemIndexZero].internalSetZoneName(itemText);
                                    break;
                                case 1://1 = Area Name
                                    if (itemIndexZero < Areas.Count)
                                        Areas[itemIndexZero].internalSetAreaName(itemText);
                                    break;
                                case 2:// = User Name
                                    break;
                                case 3:// = Keypad Name
                                    break;
                                case 4:// = Output Name
                                    break;
                                case 5:// = Task Name
                                    break;
                                case 6:// = Telephone Name
                                    break;
                                case 7:// = Light Name
                                    break;
                                case 8:// = Alarm Duration Name
                                    break;
                                case 9:// = Custom Settings
                                    break;
                                case 10:// = Counters Names
                                    break;
                                case 11:// = Thermostat Names
                                    break;
                                case 12:// = Function Key 1 Name
                                    break;
                                case 13:// = Function Key 2 Name
                                    break;
                                case 14:// = Function Key 3 Name
                                    break;
                                case 15:// = Function Key 4 Name
                                    break;
                                case 16:// = Function Key 5 Name
                                    break;
                                case 17:// = Function Key 6 Name
                                    break;
                                case 18:// = Audio Zone Name
                                    break;
                                case 19:// = Audio Source Name
                                    break;
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    SendDebug(String.Format("Error is Elk: {0}:\r\n{1}", e.Message, returnString));
                }
            }
        }

        //Simpl
        internal Dictionary<string, SimplEvents> SimplClients = new Dictionary<string, SimplEvents>();
        public bool RegisterSimplClient(string _id)
        {
            try
            {
                lock (SimplClients)
                {
                    if (!SimplClients.ContainsKey(_id))
                    {
                        this.SimplClients.Add(_id, new SimplEvents());
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }




        //Utility
        public void SetDebug(bool _value)
        {
            this.debug = _value;
            SendDebug("Enabing debug");
        }
        public void SendDebug(string msg)
        {
            if (debug)
                CrestronConsole.PrintLine(String.Format("Elk Panel {0}: {1}", panelId, msg));
        }

    }
}
