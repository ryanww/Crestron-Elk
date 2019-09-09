using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    //Internal ------------------------------------------------
    internal class ElkInternalEventsArgs : EventArgs
    {
        public string Name;
        public double Data;
        public string SData;

        public ElkInternalEventsArgs(string name, double data, string sData)
        {
            this.Name = name;
            this.Data = data;
            this.SData = sData;
        }
    }

    internal class InternalEvents
    {
        private event EventHandler<ElkInternalEventsArgs> onNewEvent = delegate { };

        public event EventHandler<ElkInternalEventsArgs> OnNewEvent
        {
            add
            {
                if (!onNewEvent.GetInvocationList().Contains(value))
                {
                    onNewEvent += value;
                }
            }
            remove
            {
                onNewEvent -= value;
            }
        }

        internal void Fire(ElkInternalEventsArgs e)
        {
            onNewEvent(null, e);
        }
    }


    //Zone -------------------------------------------------------
    public class ElkZoneEventArgs : EventArgs
    {
        public int Zone;
        public eElkZoneEventID EventID;
        public string Data_String;
        public bool Data_Bool;
        public int Data_Int;

        /// <summary>
        /// Default constructor for Elk Zone Events
        /// </summary>
        /// <param name="zone">Zone ID</param>
        /// <param name="eventID">Event Type</param>
        /// <param name="sData">String Data</param>
        /// <param name="bData">Bool Data</param>
        /// <param name="iData">Integer Data</param>
        public ElkZoneEventArgs(int zone, eElkZoneEventID eventID, string sData, bool bData, int iData)
        {
            this.Zone = zone;
            this.EventID = eventID;
            this.Data_String = sData;
            this.Data_Bool = bData;
            this.Data_Int = iData;
        }
        
    }
    public enum eElkZoneEventID
    {
        StatusChange = 0,
        TypeChange = 1,
        DescriptionChange = 2
    }

    //Area -------------------------------------------------------
    public class ElkAreaEventArgs : EventArgs
    {
        public int Area;
        public eElkAreaEventID EventID;
        public string Data_String;
        public bool Data_Bool;
        public int Data_Int;

        /// <summary>
        /// Default constructor for Elk Area Events
        /// </summary>
        /// <param name="zone">Area ID</param>
        /// <param name="eventID">Event Type</param>
        /// <param name="sData">String Data</param>
        /// <param name="bData">Bool Data</param>
        /// <param name="iData">Integer Data</param>
        public ElkAreaEventArgs(int zone, eElkAreaEventID eventID, string sData, bool bData, int iData)
        {
            this.Area = zone;
            this.EventID = eventID;
            this.Data_String = sData;
            this.Data_Bool = bData;
            this.Data_Int = iData;
        }

    }
    public enum eElkAreaEventID
    {
        ArmStatusChange = 0,
        StateStatusChange = 1,
        AlarmStatusChange = 2,
        DescriptionChange = 3
    }
    

    //Password ---------------------------------------------------------
    public class ElkPasswordEventArgs : EventArgs
    {
        public string Data_String;

        public ElkPasswordEventArgs(string newString)
        {
            this.Data_String = newString;
        }
    }

}