using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkZoneSimpl
    {

        public delegate void StatusChange(ushort _status);
        public StatusChange newStatusChange { get; set; }
        public delegate void TypeChange(ushort _type);
        public TypeChange newTypeChange { get; set; }
        public delegate void NameChange(SimplSharpString _name);
        public NameChange newNameChange { get; set; }
        public delegate void DefinitionChange(ushort _definition);
        public DefinitionChange newDefinitionChange { get; set; }
        public delegate void VoltageChange(ushort _voltage);
        public VoltageChange newVoltageChange { get; set; }
        public delegate void AreaAssignmentChange(ushort _area);
        public AreaAssignmentChange newAreaAssignmentChange { get; set; }


        private ElkPanel myPanel;
        private ElkZone myZone;

        //Init -------------------------------------------------------
        public void Initialize(ushort _panel, ushort _zoneNumber)
        {
            myPanel = ElkCore.AddOrGetCoreObject(_panel);
            if (myPanel == null)
                return;

            if (myPanel.Zones.ContainsKey((int)_zoneNumber))
            {
                myZone = myPanel.Zones[(int)_zoneNumber];
                myZone.ElkZoneEvent += new EventHandler<ElkZoneEventArgs>(myZone_ElkZoneEvent);
            }
        }


        //Public Functions -------------------------------------------------------
        public void BypassRequest()
        {
            myZone.BypassRequest();
        }
        public void ZoneTripTrigger()
        {
            myZone.ZoneTripTrigger();
        }
        public SimplSharpString GetZoneDefinition()
        {
            return (SimplSharpString)myZone.GetZoneDefinitionString();
        }


        //Events -------------------------------------------------------
        void myZone_ElkZoneEvent(object sender, ElkZoneEventArgs e)
        {
            switch (e.EventUpdateType)
            {
                case eElkZoneEventUpdateType.StatusChange:
                    newStatusChange((ushort)myZone.GetZoneStatus);
                    break;
                case eElkZoneEventUpdateType.TypeChange:
                    newTypeChange((ushort)myZone.GetZoneType);
                    break;
                case eElkZoneEventUpdateType.NameChange:
                    newNameChange((SimplSharpString)myZone.GetZoneName);
                    break;
                case eElkZoneEventUpdateType.DefinitionChange:
                    newDefinitionChange((ushort)myZone.GetZoneDefinition);
                    break;
                case eElkZoneEventUpdateType.VoltageChange:
                    double v = myZone.GetZoneVoltage() * 10;
                    newVoltageChange((ushort)v);
                    break;
                case eElkZoneEventUpdateType.AreaAssignmentChange:
                    newAreaAssignmentChange((ushort)myZone.GetZoneAreaAssignment);
                    break;
            }
        }
    }
}