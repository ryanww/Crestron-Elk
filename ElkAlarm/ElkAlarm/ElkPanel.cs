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
        private bool isInitialized = false;
        private bool isConnected = false;
        public bool IsConnected { get { return this.isConnected; } }
        private bool initRun = false;

        internal Dictionary<int, ElkArea> Areas = new Dictionary<int, ElkArea>();
        internal Dictionary<int, ElkZone> Zones = new Dictionary<int, ElkZone>();
        internal Dictionary<int, ElkOutput> Outputs = new Dictionary<int, ElkOutput>();
        internal Dictionary<int, ElkKeypad> Keypads = new Dictionary<int, ElkKeypad>();

        //Initialize
        public void Initialize(int _panelId)
        {
            if (this.initRun)
                return;

            panelId = _panelId;

            this.SendDebug(string.Format("Added and initialized Panel ", _panelId));

            for (int i = 1; i <= 8; i++)
            {
                if (!Areas.ContainsKey(i))
                {
                    ElkArea a = new ElkArea();
                    a.Initialize(this, i);
                    Areas.Add(i, a);
                }
            }
            for (int i = 1; i <= 208; i++)
            {
                if (!Zones.ContainsKey(i))
                {
                    ElkZone z = new ElkZone();
                    z.Initialize(this, i);
                    Zones.Add(i, z);
                }
            }
            for (int i = 1; i <= 208; i++)
            {
                if (!Outputs.ContainsKey(i))
                {
                    ElkOutput o = new ElkOutput();
                    o.Initialize(this, i);
                    Outputs.Add(i, o);
                }
            }

            commandQueue = new CrestronQueue<string>();
            responseQueue = new CrestronQueue<string>();

            this.initRun = true;
        }

        public void InitialzeConnection(string _host, ushort _port)
        {   
            this.panelIp = _host;
            this.panelPort = _port;
            
            if (this.panelIp.Length > 0)
            {
                this.SendDebug(string.Format("Initializing Panel {0} connection @ {1}:{2} & initialize", panelId, panelIp, panelPort));

                if (this.commandQueueTimer == null)
                    this.commandQueueTimer = new CTimer(CommandQueueDequeue, null, 0, 50);

                if (this.responseQueueTimer == null)
                    this.responseQueueTimer = new CTimer(ResponseQueueDequeue, null, 0, 50);

                this.client = new TCPClientDevice();
                this.client.ID = 1;
                this.client.ConnectionStatus += new StatusEventHandler(client_ConnectionStatus);
                this.client.ResponseString += new ResponseEventHandler(client_ResponseString);
                this.client.Connect(this.panelIp, (ushort)this.panelPort);
            }
        }

        public void InitializePanelParameters()
        {
            this.Enqueue("as00"); //Arming status request
            this.Enqueue("zs00"); //Zone status request
            this.Enqueue("zp00"); //Zone partition request
            this.Enqueue("zd00"); //Zone definition request
            this.Enqueue("cs00"); //Output status request
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
                ErrorLog.Error("ElkPanel {0} - Parse error: {1}", panelId, e.Message);
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
            if (returnString.Length <=2)
                return;

            string repType = returnString.Substring(0, 6);
            string data = "";
            CrestronConsole.PrintLine("parse {0}", repType);
            int index = 0;
            
            //All Zone Status (tested)
            if (repType.Contains("ZS"))
            {
                data = returnString.Substring(repType.IndexOf("ZS"));
                SendDebug("Got ZS");
                char[] zsArray;
                zsArray = data.Substring(2, 208).ToCharArray();
                for (int i = 0; i < zsArray.Length; i++)
                    if (Zones.ContainsKey(i + 1))
                        Zones[i + 1].internalSetZoneStatus(hexToInt(zsArray[i]));
            }

            //Single Zone Change (tested)
            if (repType.Contains("ZC"))
            {
                data = returnString.Substring(repType.IndexOf("ZC"));
                SendDebug("Got ZC");
                index = int.Parse(data.Substring(2, 3));
                if (Zones.ContainsKey(index))
                    Zones[index].internalSetZoneStatus(hexToInt(data.ToCharArray()[5]));
            }


            //Zone Definitions (tested)
            if (repType.Contains("ZD"))
            {
                data = returnString.Substring(repType.IndexOf("ZD"));
                SendDebug("Got ZD");
                char[] zdArray;
                zdArray = data.Substring(2, 208).ToCharArray();
                for (int i = 0; i < zdArray.Length; i++)
                    if (Zones.ContainsKey(i + 1))
                        Zones[i + 1].internalSetZoneDefinition((int)zdArray[i] - 48);
            }

            //Zone Bypass (tested)
            if (repType.Contains("ZB"))
            {
                data = returnString.Substring(repType.IndexOf("ZB"));
                SendDebug("Got ZB");
                index = int.Parse(data.Substring(2, 3));
                if (Zones.ContainsKey(index))
                    if (data.ToCharArray()[5] == '1')
                        Zones[index].internalSetBypass(true);
                    else
                        Zones[index].internalSetBypass(false);
            }

            //Zone Voltage (tested)
            if (repType.Contains("ZV"))
            {
                data = returnString.Substring(repType.IndexOf("ZV"));
                SendDebug("Got ZV");
                index = int.Parse(data.Substring(2, 3));
                double zoneVoltage = int.Parse(data.Substring(5, 3));
                zoneVoltage /= 10;
                if (Zones.ContainsKey(index))
                    Zones[index].internalSetZoneVoltage(zoneVoltage);
            }

            //Alarm By Zone Report (tested)
            if (repType.Contains("AZ"))
            {
                data = returnString.Substring(repType.IndexOf("AZ"));
                SendDebug("Got AZ");
                char[] zdArray;
                zdArray = data.Substring(2, 208).ToCharArray();
                for (int i = 0; i < zdArray.Length; i++)
                    if (Zones.ContainsKey(i + 1))
                        Zones[i + 1].internalSetZoneDefinition((int)zdArray[i] - 48);
            }

            //Zone Partition (tested)
            if (repType.Contains("ZP"))
            {
                data = returnString.Substring(repType.IndexOf("ZP"));
                SendDebug("Got ZP");
                char[] zpArray;
                zpArray = data.Substring(2, 208).ToCharArray();
                for (int i = 0; i < zpArray.Length; i++)
                    if (Zones.ContainsKey(i+1))
                        Zones[i+1].internalSetZoneArea(hexToInt(zpArray[i]));
                foreach (var a in Areas)
                    a.Value.internalZoneAssignmentChanged();
            }

            //Area Status (tested)
            if (repType.Contains("AS"))
            {
                data = returnString.Substring(repType.IndexOf("AS"));
                SendDebug("Got AS");
                string armedStatusString = data.Substring(2, 8);
                string armUpStateString = data.Substring(10, 8);
                string alarmStateString = data.Substring(18, 8);
                int alarmCountdownTime = int.Parse(data.Substring(26, 2));
                for (int i = 0; i < 8; i++)
                    if (Areas.ContainsKey(i + 1))
                    {
                        Areas[i + 1].internalSetAreaArmedStatus(hexToInt(armedStatusString.ToCharArray()[i]));
                        Areas[i + 1].internalSetAreaArmUpState(hexToInt(armUpStateString.ToCharArray()[i]));
                        Areas[i + 1].internalSetAreaAlarmState(hexToInt(alarmStateString.ToCharArray()[i]));
                        Areas[i + 1].internalSetCountdownClock(alarmCountdownTime);
                    }
            }

            //Output Status (tested)
            if (repType.Contains("CS"))
            {
                data = returnString.Substring(repType.IndexOf("CS"));
                SendDebug("Got CS");
                char[] csArray;
                csArray = data.Substring(2, 208).ToCharArray();
                for (int i = 0; i < csArray.Length; i++)
                    if (Outputs.ContainsKey(i + 1))
                        if (csArray[i] == '1')
                            Outputs[i + 1].internalOutputStateSet(true);
                        else
                            Outputs[i + 1].internalOutputStateSet(false);
            }

            //Output Status (tested)
            if (repType.Contains("CC"))
            {
                data = returnString.Substring(repType.IndexOf("CC"));
                SendDebug("Got CC");
                index = int.Parse(data.Substring(2, 3));
                if (Outputs.ContainsKey(index))
                    if (data.ToCharArray()[5] == '1')
                        Outputs[index].internalOutputStateSet(true);
                    else
                        Outputs[index].internalOutputStateSet(false); ;
            }


            //Descriptions (tested)
            if (repType.Contains("SD"))
            {
                data = returnString.Substring(repType.IndexOf("SD"));
                int itemType = int.Parse(data.Substring(2, 2));
                int itemIndex = int.Parse(data.Substring(4, 3));
                string itemText = data.Substring(7, 16); //may need to mask out high bit

                switch (itemType)
                {
                    case 0:// = Zone Name
                        if (Zones.ContainsKey(itemIndex))
                            Zones[itemIndex].internalSetZoneName(itemText);
                        break;
                    case 1://1 = Area Name
                        if (Areas.ContainsKey(itemIndex))
                            Areas[itemIndex].internalSetAreaName(itemText);
                        break;
                    case 2:// = User Name
                        break;
                    case 3:// = Keypad Name
                        break;
                    case 4:// = Output Name
                        if (Outputs.ContainsKey(itemIndex))
                            Outputs[itemIndex].internalSetOutputName(itemText);
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
                ErrorLog.Error("ElkPanel {0} - Simple client registration error: {1}", panelId, e.Message);
                return false;
            }
        }


        //Interface
        public ElkArea GetAreaObject(int _area)
        {
            if (Areas.ContainsKey(_area))
                return Areas[_area];
            else
                return null;
        }
        public ElkZone GetZoneObject(int _zone)
        {
            if (Zones.ContainsKey(_zone))
                return Zones[_zone];
            else
                return null;
        }
        public ElkOutput GetOutputObject(int _output)
        {
            if (Outputs.ContainsKey(_output))
                return Outputs[_output];
            else
                return null;
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
        public static int hexToInt(char hexChar)
        {
            hexChar = char.ToUpper(hexChar);
            return (int)hexChar < (int)'A' ? ((int)hexChar - (int)'0') : 10 + ((int)hexChar - (int)'A');
        }
    }
}
