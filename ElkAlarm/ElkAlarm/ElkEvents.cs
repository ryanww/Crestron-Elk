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





    

    //Password ---------------------------------------------------------
    public class ElkPasswordEventArgs : EventArgs
    {
        public string Data_String;

        public ElkPasswordEventArgs(string newString)
        {
            this.Data_String = newString;
        }
    }


    //Simpl
    public class SimplEvents
    {
        private event EventHandler<SimplEventArgs> onNewEvent = delegate { };

        public event EventHandler<SimplEventArgs> OnNewEvent
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

        internal void Fire(SimplEventArgs e)
        {
            onNewEvent(null, e);
        }
    }

    public class SimplEventArgs : EventArgs
    {
        public SimplSharpString StringData;
        public ushort IntData;
        public eElkSimplEventIds ID;

        public SimplEventArgs(eElkSimplEventIds id, SimplSharpString stringData, ushort intData)
        {
            this.StringData = stringData;
            this.IntData = intData;
            this.ID = id;
        }
    }

    public enum eElkSimplEventIds
    {
        IsRegistered = 1,
        IsConnected = 2
    }
}