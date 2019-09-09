using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkZone
    {
        private int _zNum;
        private bool _registered;
        private eZoneStatus _status;
        private eZoneType _type;
        private string _description;

        public event EventHandler<ElkZoneEventArgs> ElkZoneEvent;

        public eZoneStatus getZoneStatus { get { return _status; } }
        public eZoneType getZoneType { get { return _type; } }
        public string getZoneDescription { get { return _description; } }
        public bool getIsRegistered { get { return _registered; } }


        public ElkZone(int zone)
        {
            _zNum = zone;
            if (ElkProcessor.RegisterZone(_zNum))
            {
                ElkProcessor.Zones[_zNum].OnNewEvent += new EventHandler<ElkInternalEventsArgs>(ElkZone_OnNewEvent);
                _registered = true;
            }
        }

        public void BypassStateSet (bool state)
        {
            switch (_status)
            {
                case eZoneStatus.Normal:
                    {
                        //If state == true then set to bypass
                    }
                    break;
                case eZoneStatus.Bypassed:
                    {
                        //If state == false then set to normal
                    }
                    break;
            }
        }
        public void BypassStateToggle()
        {
            switch (_status)
            {
                case eZoneStatus.Normal:
                    {
                        BypassStateSet(true);
                    }
                    break;
                case eZoneStatus.Bypassed:
                    {
                        BypassStateSet(false);
                    }
                    break;
            }
        }

        void ElkZone_OnNewEvent(object sender, ElkInternalEventsArgs e)
        {
            if (e.Name == "")
            {
                //TODO: Add event stuff from processor
            }
        }
    }
    
    public class Zone
    {
        public int ZoneNum;
    }

    public enum eZoneStatus
    {
        Normal = 0,
        Trouble = 1,
        Violated = 2,
        Bypassed = 3
    }
    public enum eZoneType
    {
        Unconfigured = 0,
        Open = 1,
        EOL = 2,
        Short = 3
    }

}