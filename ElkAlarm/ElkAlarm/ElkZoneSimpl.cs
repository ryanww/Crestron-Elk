using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkZoneSimpl
    {
        public delegate void StatusChange(ushort status);
        public StatusChange newStatusChange { get; set; }
        public delegate void TypeChange(ushort type);
        public TypeChange newTypeChange { get; set; }

        private ElkZone zone;

        public void Initialize(ushort zoneNum)
        {
            zone = new ElkZone(zoneNum);
            zone.ElkZoneEvent += new EventHandler<ElkZoneEventArgs>(zone_ElkZoneEvent);
        }

        public void SetBypassState(ushort state)
        {
            zone.BypassStateSet(Convert.ToBoolean(state));
        }
        public void SetBypassToggle()
        {
            zone.BypassStateToggle();
        }

        private void zone_ElkZoneEvent(object sender, ElkZoneEventArgs e)
        {
            switch (e.EventID)
            {
                case eElkZoneEventID.StatusChange:
                    if (newStatusChange != null)
                        newStatusChange((ushort)zone.getZoneStatus);
                    break;
                case eElkZoneEventID.TypeChange:
                    if (newTypeChange != null)
                        newTypeChange((ushort)zone.getZoneType);
                    break;
            }
        }
    }
}