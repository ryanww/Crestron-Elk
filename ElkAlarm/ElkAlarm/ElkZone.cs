using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkZone
    {
        private int zoneNumber;
        private bool isRegistered;

        private ElkPanel myPanel;

        private eZoneStatus zoneStatus;
        private eZoneType zoneType;
        private string zoneName;
        private eZoneDefinition zoneDefinition;
        private double zoneVoltage;
        private bool zoneBypassed;
        private int areaAssignment;

        //Init -------------------------------------------------------
        public void Initialize(ElkPanel _panel, int _zone)
        {
            zoneNumber = _zone;
            myPanel = _panel;
        }


        //Public Functions -------------------------------------------------------
        public void BypassStateSet(bool state)
        {
            switch (zoneStatus)
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
            switch (zoneStatus)
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

        public void ZoneTrigger()
        {
            myPanel.Enqueue(string.Format("zt{0}00", zoneNumber.ToString("000")));
        }

        public eZoneStatus getZoneStatus { get { return zoneStatus; } }
        public eZoneType getZoneType { get { return zoneType; } }
        public string getZoneName { get { return zoneName; } }

        public eZoneDefinition getZoneDefinition { get { return zoneDefinition; } }
        public string getZoneDefinitionString()
        {
            switch (zoneDefinition)
            {
                case eZoneDefinition.Disabled:
                    return "Disabled";
                    break;
                case eZoneDefinition.BurglarEntryExit1:
                    return "Burglar Entry/Exit 1";
                    break;
                case eZoneDefinition.BurglarEntryExit2:
                    return "Burglar Entry/Exit 2";
                    break;
                case eZoneDefinition.BurglarPerimeterInstant:
                    return "Burglar Perimeter Instant";
                    break;
                case eZoneDefinition.BurglarInterior:
                    return "Burglar Interior";
                    break;
                case eZoneDefinition.BurglarInteriorFollower:
                    return "Burglar Interior Follower";
                    break;
                case eZoneDefinition.BurglarInteriorNight:
                    return "Burglar Interior Night";
                    break;
                case eZoneDefinition.BurglarInteriorNightDelay:
                    return "Burglar Interior Night Delay";
                    break;
                case eZoneDefinition.Burglar24Hour:
                    return "Burglar 24 Hour";
                    break;
                case eZoneDefinition.BurglarBoxTamper:
                    return "Burglar Box Tamper";
                    break;
                case eZoneDefinition.FireAlarm:
                    return "Fire Alarm";
                    break;
                case eZoneDefinition.FireVerified:
                    return "Fire Verified";
                    break;
                case eZoneDefinition.FireSupervisory:
                    return "Fire Supervisory";
                    break;
                case eZoneDefinition.AuxAlarm1:
                    return "Aux Alarm 1";
                    break;
                case eZoneDefinition.AuxAlarm2:
                    return "Aux Alarm 2";
                    break;
                case eZoneDefinition.Keyfob:
                    return "Keyfob";
                    break;
                case eZoneDefinition.NonAlarm:
                    return "Non Alarm";
                    break;
                case eZoneDefinition.CarbonMonoxide:
                    return "Carbon Monoxide";
                    break;
                case eZoneDefinition.EmergencyAlarm:
                    return "Emergency Alarm";
                    break;
                case eZoneDefinition.FreezeAlarm:
                    return "Freeze Alarm";
                    break;
                case eZoneDefinition.GasAlarm:
                    return "Gas Alarm";
                    break;
                case eZoneDefinition.HeatAlarm:
                    return "Heat Alarm";
                    break;
                case eZoneDefinition.MedicalAlarm:
                    return "Medical Alarm";
                    break;
                case eZoneDefinition.PoliceAlarm:
                    return "Police Alarm";
                    break;
                case eZoneDefinition.PoliceNoIndication:
                    return "Police No Indication";
                    break;
                case eZoneDefinition.WaterAlarm:
                    return "WaterAlarm";
                    break;
                case eZoneDefinition.KeyMomentaryArmDisarm:
                    return "Key Momentary Arm/Disarm";
                    break;
                case eZoneDefinition.KeyMomentaryArmAway:
                    return "Key Momentary Arm Away";
                    break;
                case eZoneDefinition.KeyMomentaryArmStay:
                    return "Key Momentary Arm Stay";
                    break;
                case eZoneDefinition.KeyOnOff:
                    return "Key On/Off";
                    break;
                case eZoneDefinition.MuteAudibles:
                    return "Mute Audibles";
                    break;
                case eZoneDefinition.PowerSupervisory:
                    return "Power Supervisory";
                    break;
                case eZoneDefinition.Temperature:
                    return "Temperature";
                    break;
                case eZoneDefinition.AnalogZone:
                    return "Analog Zone";
                    break;
                case eZoneDefinition.PhoneKey:
                    return "Phone Key";
                    break;
                case eZoneDefinition.IntercomKey:
                    return "Intercom Key";
                    break;
                default:
                    return "";
                    break;
            }
        }

        //Internal Functions -------------------------------------------------------

        //06zs004D(CR-LF)
        public void internalSetZoneStatus(int s)
        {
            eZoneStatus tzs = zoneStatus;
            eZoneType tzt = zoneType;

            switch (s)
            {
                case 0:
                    tzs = eZoneStatus.Normal;
                    tzt = eZoneType.Unconfigured;
                    break;
                case 1:
                    tzs = eZoneStatus.Normal;
                    tzt = eZoneType.Open;
                    break;
                case 2:
                    tzs = eZoneStatus.Normal;
                    tzt = eZoneType.EOL;
                    break;
                case 3:
                    tzs = eZoneStatus.Normal;
                    tzt = eZoneType.Short;
                    break;
                case 5:
                    tzs = eZoneStatus.Trouble;
                    tzt = eZoneType.Open;
                    break;
                case 6:
                    tzs = eZoneStatus.Trouble;
                    tzt = eZoneType.EOL;
                    break;
                case 7:
                    tzs = eZoneStatus.Trouble;
                    tzt = eZoneType.Short;
                    break;
                case 9:
                    tzs = eZoneStatus.Violated;
                    tzt = eZoneType.Open;
                    break;
                case 10:
                    tzs = eZoneStatus.Violated;
                    tzt = eZoneType.EOL;
                    break;
                case 11:
                    tzs = eZoneStatus.Violated;
                    tzt = eZoneType.Short;
                    break;
                case 12:
                    tzs = eZoneStatus.SoftBypassed;
                    break;
                case 13:
                    tzs = eZoneStatus.Bypassed;
                    tzt = eZoneType.Open;
                    break;
                case 14:
                    tzs = eZoneStatus.Bypassed;
                    tzt = eZoneType.EOL;
                    break;
                case 15:
                    tzs = eZoneStatus.Bypassed;
                    tzt = eZoneType.Short;
                    break;
            }
            if (tzs != zoneStatus)
            {
                zoneStatus = tzs;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneStatus - zoneStatus = {1}", zoneNumber, zoneStatus.ToString()));
                OnElkZoneEvent(eElkZoneEventUpdateType.StatusChange);
            }
            if (tzt != zoneType)
            {
                zoneType = tzt;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneStatus - zoneType = {1}", zoneNumber, zoneType.ToString()));
                OnElkZoneEvent(eElkZoneEventUpdateType.TypeChange);
            }
        }

        public void internalSetZoneName(string name)
        {
            if (zoneName != name)
            {
                zoneName = name;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneName = {1}", zoneNumber, zoneName));
                OnElkZoneEvent(eElkZoneEventUpdateType.NameChange);
            }
        }

        //06zd005C Zone Definition, request data
        public void internalSetZoneDefinition(int defNum)
        {
            eZoneDefinition te = (eZoneDefinition)Enum.Parse(typeof(eZoneDefinition), Convert.ToString(defNum), true);
            if (te != zoneDefinition)
            {
                zoneDefinition = te;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneDefinition = {1}", zoneNumber, zoneDefinition.ToString()));
                OnElkZoneEvent(eElkZoneEventUpdateType.DefinitionChange);
            }
        }

        //09zv12300B1 Zone 123 analog voltage request data
        public void internalSetZoneVoltage(double volt)
        {
            if (zoneVoltage != volt)
            {
                zoneVoltage = volt;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneVoltage = {1}", zoneNumber, zoneVoltage));
                OnElkZoneEvent(eElkZoneEventUpdateType.VoltageChange);
            }
        }

        public void internalSetBypass(bool state)
        {
            if (zoneBypassed != state)
            {
                zoneBypassed = state;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetBypass = {1}", zoneNumber, zoneBypassed));
                OnElkZoneEvent(eElkZoneEventUpdateType.BypassChange);
            }
        }

        public void internalSetZoneArea(int area)
        {
            if (areaAssignment != area)
            {
                areaAssignment = area;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneArea = {1}", zoneNumber, areaAssignment));
                OnElkZoneEvent(eElkZoneEventUpdateType.AreaAssignmentChange);
            }
        }

        //Events -------------------------------------------------------
        public event EventHandler<ElkZoneEventArgs> ElkZoneEvent;
        protected virtual void OnElkZoneEvent(eElkZoneEventUpdateType updateType)
        {
            if (ElkZoneEvent != null)
                ElkZoneEvent(this, new ElkZoneEventArgs() { Zone = zoneNumber, EventUpdateType = updateType });
        }
    }


    //Enum's -------------------------------------------------------
    public enum eZoneStatus
    {
        Normal = 0,
        Trouble = 1,
        Violated = 2,
        Bypassed = 3,
        SoftBypassed = 4
    }
    public enum eZoneType
    {
        Unconfigured = 0,
        Open = 1,
        EOL = 2,
        Short = 3
    }
    public enum eZoneDefinition
    {
        Disabled = 0,
        BurglarEntryExit1 = 1,
        BurglarEntryExit2 = 2,
        BurglarPerimeterInstant = 3,
        BurglarInterior = 4,
        BurglarInteriorFollower = 5,
        BurglarInteriorNight = 6,
        BurglarInteriorNightDelay = 7,
        Burglar24Hour = 8,
        BurglarBoxTamper = 9,
        FireAlarm = 10,
        FireVerified = 11,
        FireSupervisory = 12,
        AuxAlarm1 = 13,
        AuxAlarm2 = 14,
        Keyfob = 15,
        NonAlarm = 16,
        CarbonMonoxide = 17,
        EmergencyAlarm = 18,
        FreezeAlarm = 19,
        GasAlarm = 20,
        HeatAlarm = 21,
        MedicalAlarm = 22,
        PoliceAlarm = 23,
        PoliceNoIndication = 24,
        WaterAlarm = 25,
        KeyMomentaryArmDisarm = 26,
        KeyMomentaryArmAway = 27,
        KeyMomentaryArmStay = 28,
        KeyMomentaryDisarm = 29,
        KeyOnOff = 30,
        MuteAudibles = 31,
        PowerSupervisory = 32,
        Temperature = 33,
        AnalogZone = 34,
        PhoneKey = 35,
        IntercomKey = 36
    }


    //Events -------------------------------------------------------
    public class ElkZoneEventArgs : EventArgs
    {
        public int Zone { get; set; }
        public eElkZoneEventUpdateType EventUpdateType { get; set; }
    }
    public enum eElkZoneEventUpdateType
    {
        StatusChange = 0,
        TypeChange = 1,
        NameChange = 2,
        DefinitionChange = 3,
        VoltageChange = 4,
        BypassChange = 5,
        AreaAssignmentChange = 6
    }
}