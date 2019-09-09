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
        //internal static ElkKeypad kp;



        //Zone -----------------------------------------------------------
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
