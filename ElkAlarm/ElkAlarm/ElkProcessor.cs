using System;
using System.Text;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
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
        private static bool isDisposed;

        private static string kpText;

        internal static Dictionary<int, InternalEvents> Zones = new Dictionary<int, InternalEvents>();
        internal static Dictionary<int, InternalEvents> Areas = new Dictionary<int, InternalEvents>();
        



        //Zones -----------------------------------------------------------
        static internal bool RegisterZone(int zone)
        {
            try
            {
                lock (Zones)
                {
                    Zones.Add(zone, new InternalEvents());
                    return true;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Elk Error: Couldn't add zone {0} - {1}", zone, e.Message);
                return false;
            }
        }


        //Areas -----------------------------------------------------------
        static internal bool RegisterArea(int area)
        {
            try
            {
                lock (Areas)
                {
                    Areas.Add(area, new InternalEvents());
                    return true;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Elk Error: Couldn't add area {0} - {1}", area, e.Message);
                return false;
            }
        }


        //Keypad --------------------------------------------------------
        static internal void setKeypadText(string text)
        {
            kpText = text;
        }



        //Zone initialize
        //06zs004D\x0D\x0A


        static void sendDebug(string debugMsg)
        {
            if (debug)
                CrestronConsole.PrintLine("Elk Debug - {0}", debugMsg);
        }

    }
}
