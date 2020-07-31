using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkZone
    {
        private int zoneNumber = 0;
        private bool isRegistered = false;

        private ElkPanel myPanel;

        private eZoneStatus zoneStatus = eZoneStatus.Uninitialized;
        private eZoneType zoneType = eZoneType.Uninitialized;
        private string zoneName = "";
        private eZoneDefinition zoneDefinition;
        private double zoneVoltage = 0.0;
        private int areaAssignment = 0;

        //Init -------------------------------------------------------
        public void Initialize(ElkPanel _panel, int _zone)
        {
            zoneNumber = _zone;
            myPanel = _panel;
        }

        //Public Functions -------------------------------------------------------
        public void BypassRequest()
        {
            if (myPanel.Areas.ContainsKey(areaAssignment) && areaAssignment > 0)
            {
                if (myPanel.Areas[areaAssignment].bypassPassword.Length > 3)
                {
                    string cmdStr = string.Format("zb{0:000}{1}{2}00", zoneNumber, areaAssignment, myPanel.Areas[areaAssignment].bypassPassword);
                    myPanel.SendDebug(string.Format("Zone {0} - BypassRequest-UsingBypassCode = {1}", zoneNumber, cmdStr));
                    myPanel.Enqueue(cmdStr);
                }

                else if (myPanel.Areas[areaAssignment].myPw.IsValidCodeEntered())
                {
                    string cmdStr = string.Format("zb{0:000}{1}{2}00", zoneNumber, areaAssignment, myPanel.Areas[areaAssignment].myPw.getPassword());
                    myPanel.SendDebug(string.Format("Zone {0} - BypassRequest-UsingUserEnteredCode = {1}", zoneNumber, cmdStr));
                    myPanel.Enqueue(cmdStr);
                }
            }
            else
                ErrorLog.Error("ElkPanel {0} - Zone {1} doesnt have internal area assignment set.", myPanel.getPanelId, zoneNumber);
        }

        public void ZoneTripTrigger()
        {
            string cmdStr = string.Format("zt{0:000}00", zoneNumber);
            myPanel.SendDebug(string.Format("Zone {0} - ZoneTripTrigger = {1}", zoneNumber, cmdStr));
            myPanel.Enqueue(cmdStr);
        }

        public eZoneStatus GetZoneStatus { get { return zoneStatus; } }

        public eZoneType GetZoneType { get { return zoneType; } }

        public string GetZoneName { get { return zoneName; } }

        public int GetZoneAreaAssignment { get { return areaAssignment; } }

        public eZoneDefinition GetZoneDefinition { get { return zoneDefinition; } }

        public string GetZoneDefinitionString()
        {
            string ret = "";
            switch (zoneDefinition)
            {
                case eZoneDefinition.Disabled:
                    ret = "Disabled";
                    break;
                case eZoneDefinition.BurglarEntryExit1:
                    ret = "Burglar Entry/Exit 1";
                    break;
                case eZoneDefinition.BurglarEntryExit2:
                    ret = "Burglar Entry/Exit 2";
                    break;
                case eZoneDefinition.BurglarPerimeterInstant:
                    ret = "Burglar Perimeter Instant";
                    break;
                case eZoneDefinition.BurglarInterior:
                    ret = "Burglar Interior";
                    break;
                case eZoneDefinition.BurglarInteriorFollower:
                    ret = "Burglar Interior Follower";
                    break;
                case eZoneDefinition.BurglarInteriorNight:
                    ret = "Burglar Interior Night";
                    break;
                case eZoneDefinition.BurglarInteriorNightDelay:
                    ret = "Burglar Interior Night Delay";
                    break;
                case eZoneDefinition.Burglar24Hour:
                    ret = "Burglar 24 Hour";
                    break;
                case eZoneDefinition.BurglarBoxTamper:
                    ret = "Burglar Box Tamper";
                    break;
                case eZoneDefinition.FireAlarm:
                    ret = "Fire Alarm";
                    break;
                case eZoneDefinition.FireVerified:
                    ret = "Fire Verified";
                    break;
                case eZoneDefinition.FireSupervisory:
                    ret = "Fire Supervisory";
                    break;
                case eZoneDefinition.AuxAlarm1:
                    ret = "Aux Alarm 1";
                    break;
                case eZoneDefinition.AuxAlarm2:
                    ret = "Aux Alarm 2";
                    break;
                case eZoneDefinition.Keyfob:
                    ret = "Keyfob";
                    break;
                case eZoneDefinition.NonAlarm:
                    ret = "Non Alarm";
                    break;
                case eZoneDefinition.CarbonMonoxide:
                    ret = "Carbon Monoxide";
                    break;
                case eZoneDefinition.EmergencyAlarm:
                    ret = "Emergency Alarm";
                    break;
                case eZoneDefinition.FreezeAlarm:
                    ret = "Freeze Alarm";
                    break;
                case eZoneDefinition.GasAlarm:
                    ret = "Gas Alarm";
                    break;
                case eZoneDefinition.HeatAlarm:
                    ret = "Heat Alarm";
                    break;
                case eZoneDefinition.MedicalAlarm:
                    ret = "Medical Alarm";
                    break;
                case eZoneDefinition.PoliceAlarm:
                    ret = "Police Alarm";
                    break;
                case eZoneDefinition.PoliceNoIndication:
                    ret = "Police No Indication";
                    break;
                case eZoneDefinition.WaterAlarm:
                    ret = "WaterAlarm";
                    break;
                case eZoneDefinition.KeyMomentaryArmDisarm:
                    ret = "Key Momentary Arm/Disarm";
                    break;
                case eZoneDefinition.KeyMomentaryArmAway:
                    ret = "Key Momentary Arm Away";
                    break;
                case eZoneDefinition.KeyMomentaryArmStay:
                    ret = "Key Momentary Arm Stay";
                    break;
                case eZoneDefinition.KeyOnOff:
                    ret = "Key On/Off";
                    break;
                case eZoneDefinition.MuteAudibles:
                    ret = "Mute Audibles";
                    break;
                case eZoneDefinition.PowerSupervisory:
                    ret = "Power Supervisory";
                    break;
                case eZoneDefinition.Temperature:
                    ret = "Temperature";
                    break;
                case eZoneDefinition.AnalogZone:
                    ret = "Analog Zone";
                    break;
                case eZoneDefinition.PhoneKey:
                    ret = "Phone Key";
                    break;
                case eZoneDefinition.IntercomKey:
                    ret = "Intercom Key";
                    break;
                default:
                    ret = "";
                    break;
            }
            return ret;
        }

        public double GetZoneVoltage()
        {
            return zoneVoltage;
        }

        //Internal Functions -------------------------------------------------------

        internal void internalSetZoneStatus(int s)
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
            checkRegistered();
        }

        internal void internalSetZoneName(string name)
        {
            if (zoneName != name)
            {
                zoneName = name;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneName = {1}", zoneNumber, zoneName));
                OnElkZoneEvent(eElkZoneEventUpdateType.NameChange);
            }
            checkRegistered();
        }

        internal void internalSetZoneDefinition(int defNum)
        {
            eZoneDefinition te = (eZoneDefinition)Enum.Parse(typeof(eZoneDefinition), Convert.ToString(defNum), true);
            if (te != zoneDefinition)
            {
                zoneDefinition = te;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneDefinition = {1}", zoneNumber, zoneDefinition.ToString()));
                OnElkZoneEvent(eElkZoneEventUpdateType.DefinitionChange);
            }
            checkRegistered();
        }

        internal void internalSetZoneVoltage(double volt)
        {
            if (zoneVoltage != volt)
            {
                zoneVoltage = volt;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneVoltage = {1}", zoneNumber, zoneVoltage));
                OnElkZoneEvent(eElkZoneEventUpdateType.VoltageChange);
            }
            checkRegistered();
        }

        internal void internalSetBypass(bool state)
        {
            if (zoneStatus != eZoneStatus.Bypassed)
            {
                zoneStatus = eZoneStatus.Bypassed;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetBypass = {1}", zoneNumber, zoneStatus));
                OnElkZoneEvent(eElkZoneEventUpdateType.StatusChange);
            }
            checkRegistered();
        }

        internal void internalSetZoneArea(int area)
        {
            if (areaAssignment != area)
            {
                areaAssignment = area;
                myPanel.SendDebug(string.Format("Zone {0} - internalSetZoneArea = {1}", zoneNumber, areaAssignment));
                OnElkZoneEvent(eElkZoneEventUpdateType.AreaAssignmentChange);
            }
            checkRegistered();
        }

        //Private Functions -------------------------------------------------------
        private void checkRegistered()
        {
            if (!isRegistered)
            {
                string cmdStr = string.Format("sd00{0:000}00", zoneNumber);
                myPanel.SendDebug(string.Format("Zone {0} - checkRegistered = {1}", zoneNumber, cmdStr));
                myPanel.Enqueue(cmdStr);
                isRegistered = true;
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
        Uninitialized = -1,
        Normal = 0,
        Trouble = 1,
        Violated = 2,
        Bypassed = 3,
        SoftBypassed = 4
    }

    public enum eZoneType
    {
        Uninitialized = -1,
        Unconfigured = 0,
        Open = 1,
        EOL = 2,
        Short = 3
    }

    public enum eZoneDefinition
    {
        Uninitialized = -1,
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
        AreaAssignmentChange = 5
    }
}