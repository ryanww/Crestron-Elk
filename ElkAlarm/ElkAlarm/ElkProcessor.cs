using System;
using System.Text;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using TCP_Client;


namespace ElkAlarm
{
    public class ElkProcessor
    {
        private static CrestronQueue<string> commandQueue;
        private static CrestronQueue<string> responseQueue;
        private static CTimer commandQueueTimer;
        private static CTimer responseQueueTimer;
        private static TCPClientDevice client;
        private static bool debug;

        private static bool isInitialized;
        private static bool isConnected;
        private static bool isDisposed;

        private static string pwCode;

        internal static List<Area> areas;
        internal static List<Zone> zones;

        //internal static Dictionary<int, InternalEvents> Zones = new Dictionary<int, InternalEvents>();
        //internal static Dictionary<int, InternalEvents> Areas = new Dictionary<int, InternalEvents>();
        internal static Dictionary<string, SimplEvents> SimplClients = new Dictionary<string, SimplEvents>();



        //Initialize -----------------------------------------------------------
        public static void Initialize(string host, ushort port)
        {
            if (!isInitialized)
            {
                sendDebug("Elk initializing");
                commandQueue = new CrestronQueue<string>();
                responseQueue = new CrestronQueue<string>();
                commandQueueTimer = new CTimer(CommandQueueDequeue, null, 0, 50);
                responseQueueTimer = new CTimer(ResponseQueueDequeue, null, 0, 50);

                for (int i=0; i < 8; i++)
                {
                    areas.Add(new Area());
                }
                for (int i = 0; i < 208; i++)
                {
                    zones.Add(new Zone());
                }

                client = new TCPClientDevice();
                client.ID = 1;
                client.ConnectionStatus += new StatusEventHandler(client_ConnectionStatus);
                client.ResponseString += new ResponseEventHandler(client_ResponseString);
                client.Connect(host, port);
            }
        }



        //Comms --------------------------------------------------------------
        private static void CommandQueueDequeue(object o)
        {
            if (!commandQueue.IsEmpty)
            {
                var data = commandQueue.Dequeue();
                data = generateChecksumString(data);
                sendDebug(string.Format("Elk - Sending from queue: {0}", data));
                client.SendCommand(data + "\x0D\x0A");
            }
        }
        static StringBuilder RxData = new StringBuilder();
        static bool busy = false;
        static int Pos = -1;
        private static void ResponseQueueDequeue(object o)
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
        private static void ParseInternalResponse(string returnString)
        {
            if (returnString.Length > 0)
            {
                try
                {
                    //Zones
                    if (returnString.Contains("D6ZS")) //All Zones
                    {
                        //char[] arr;
                        //arr = returnString.ToCharArray();
                        //for(int i=4; i<returnString.Length-1; i++)
                        //{
                        //    if (Zones.ContainsKey(i-3))
                        //    {
                        //        Zones[i].Fire(new ElkInternalEventsArgs("STATE",hexToInt(arr[i]),""));
                        //    }
                        //}
                    }
                    else if (returnString.Contains("0AZC")) //Single Zone
                    {
                        //int z = Convert.ToInt16(returnString.Substring(4, 3));
                        //char s = returnString.ToCharArray()[3];
                        //if (Zones.ContainsKey(z))
                        //{
                        //    Zones[z].Fire(new ElkInternalEventsArgs("STATE", hexToInt(s), ""));
                        //}
                    }

                    //Area
                    else if (returnString.Contains("1EAS")) //Area Status
                    {
                        //for (int i = 1; i <= 8; i++)
                        //{
                        //    int armedStatus = hexToInt(returnString.ToCharArray()[i + 4]);
                        //    int armUpState = hexToInt(returnString.ToCharArray()[i + 4 + 8]);
                        //    int alarmState = hexToInt(returnString.ToCharArray()[i + 4 + 16]);
                        //    if (Areas.ContainsKey(i))
                        //    {
                        //        Areas[i].Fire(new ElkInternalEventsArgs("STATUS", armedStatus, ""));
                        //        Areas[i].Fire(new ElkInternalEventsArgs("STATE", armUpState, ""));
                        //        Areas[i].Fire(new ElkInternalEventsArgs("ALARM", alarmState, ""));
                        //    }
                        //}
                    }

                }
                catch (Exception e)
                {
                    sendDebug(String.Format("Error is Elk: {0}:\r\n{1}", e.Message, returnString));
                }
            }
        }
        internal static void Enqueue(string data)
        {
            commandQueue.Enqueue(data);
        }
        public static void ParseResponse(string data)
        {
            try
            {
                sendDebug(string.Format("Elk - Received and adding to queue: {0}", data));
                responseQueue.Enqueue(data);
            }
            catch (Exception e)
            {
            }
        }
        static void client_ResponseString(string response, int id)
        {
            //sendDebug(string.Format("RX ID:{0} - {1}", id, response));
            ParseResponse(response);
        }
        static void client_ConnectionStatus(int status, int id)
        {
            if (status == 2 && !isConnected)
            {
                isConnected = true;
                foreach (var item in SimplClients)
                {
                    //item.Value.Fire(new SimplEventArgs(eQscSimplEventIds.IsConnected, "true", 1));
                }
                CrestronEnvironment.Sleep(1500);

                //CoreModuleInit();

                isInitialized = true;

                foreach (var item in SimplClients)
                {
                    //item.Value.Fire(new SimplEventArgs(eQscSimplEventIds.IsRegistered, "true", 1));
                }
            }
            else if (isConnected && status != 2)
            {
                sendDebug("Elk Disconnected");
                isConnected = false;
                isInitialized = false;
                foreach (var item in SimplClients)
                {
                    //item.Value.Fire(new SimplEventArgs(eQscSimplEventIds.IsRegistered, "false", 0));
                    //item.Value.Fire(new SimplEventArgs(eQscSimplEventIds.IsConnected, "false", 0));
                }
            }
        }
        static string generateChecksumString(string originalCommand)
        {
            int chksm = 0;
            char[] arr;
            arr = originalCommand.ToCharArray();
            for (int i = 0; i < originalCommand.Length; i++)
            {
                chksm = chksm + arr[i];
            }
            chksm = 256 - (chksm % 256);
            return String.Format("{0:X2}%s%02X", originalCommand.Length, originalCommand, chksm);
        }

        //Zones -----------------------------------------------------------
        //static internal bool RegisterZone(int zone)
        //{
        //    //try
        //    //{
        //    //    lock (Zones)
        //    //    {
        //    //        Zones.Add(zone, new InternalEvents());
        //    //        return true;
        //    //    }
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    ErrorLog.Error("Elk Error: Couldn't add zone {0} - {1}", zone, e.Message);
        //    //    return false;
        //    //}
        //}
        //Zone initialize
        //06zs004D\x0D\x0A


        //Areas -----------------------------------------------------------
        //static internal bool RegisterArea(int area)
        //{
        //    //try
        //    //{
        //    //    lock (Areas)
        //    //    {
        //    //        Areas.Add(area, new InternalEvents());
        //    //        return true;
        //    //    }
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    ErrorLog.Error("Elk Error: Couldn't add area {0} - {1}", area, e.Message);
        //    //    return false;
        //    //}
        //}


        //Password --------------------------------------------------------
        static internal void setPasswordText(string pw)
        {
            pwCode = pw;
        }


        //Simpl
        public static bool RegisterSimplClient(string id)
        {
            try
            {
                lock (SimplClients)
                {
                    if (!SimplClients.ContainsKey(id))
                    {
                        SimplClients.Add(id, new SimplEvents());
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
        public static int hexToInt(char hexChar)
        {
            hexChar = char.ToUpper(hexChar);  // may not be necessary

            return (int)hexChar < (int)'A' ?
                ((int)hexChar - (int)'0') :
                10 + ((int)hexChar - (int)'A');
        }

        static void sendDebug(string debugMsg)
        {
            if (debug)
                CrestronConsole.PrintLine("Elk Debug - {0}", debugMsg);
        }

    }
}
