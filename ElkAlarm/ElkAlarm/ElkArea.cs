using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkArea
    {
        private int areaNumber;
        private bool isRegistered;

        private ElkPanel myPanel;

        private eAreaArmedStatus _armedStatus;
        private eAreaArmUpState _armUpState;
        private eAreaAlarmState _alarmState;
        private string _name;

        //Init -------------------------------------------------------
        public void Initialize(ElkPanel _panel, int _area)
        {
            areaNumber = _area;
            myPanel = _panel;
        }


        //Public Functions -------------------------------------------------------
        public void SetArmLevel(eAreaArmSet arm, string pass)
        {
            string armstr = string.Format("a{0}{1}{2}00", arm, areaNumber, pass);
            myPanel.SendDebug(string.Format("Area {0} - SetArmLevel = {1}", areaNumber, armstr));
            myPanel.Enqueue(armstr);
        }

        public eAreaArmedStatus getAreaArmedStatus { get { return _armedStatus; } }
        public string getAreaArmedStatusString()
        {
            switch (_armedStatus)
            {
                case eAreaArmedStatus.Disarmed:
                    return "Disarmed";
                    break;
                case eAreaArmedStatus.ArmedAway:
                    return "Armed Away";
                    break;
                case eAreaArmedStatus.ArmedStay:
                    return "Armed Stay";
                    break;
                case eAreaArmedStatus.ArmedStayInstant:
                    return "Armed Stay Instant";
                    break;
                case eAreaArmedStatus.ArmedNight:
                    return "Armed Night";
                    break;
                case eAreaArmedStatus.ArmedNightInstant:
                    return "Armed Night Instant";
                    break;
                case eAreaArmedStatus.ArmedVacation:
                    return "Armed Vacation";
                    break;
                default:
                    return "";
                    break;
            }
        }

        public eAreaArmUpState getAreaArmUpState { get { return _armUpState; } }
        public string getAreaArmUpStateString()
        {
            switch (_armUpState)
            {
                case eAreaArmUpState.NotReadyToArm:
                    return "Not Ready To Arm";
                    break;
                case eAreaArmUpState.ReadyToArm:
                    return "Ready To Arm";
                    break;
                case eAreaArmUpState.ReadyToArmIfForced:
                    return "Ready To Arm If Forced";
                    break;
                case eAreaArmUpState.ArmedWithExitTimer:
                    return "Armed With Exit Timer";
                    break;
                case eAreaArmUpState.Armed:
                    return "Armed";
                    break;
                case eAreaArmUpState.ForceArmed:
                    return "Force Armed";
                    break;
                case eAreaArmUpState.ArmedWithBypass:
                    return "Armed With Bypass";
                    break;
                default:
                    return "";
                    break;
            }
        }

        public eAreaAlarmState getAlarmStatus { get { return _alarmState; } }
        public string getAlarmStatusString()
        {
            switch (_alarmState)
            {
                case eAreaAlarmState.NoAlarmActive:
                    return "No Alarm Active";
                    break;
                case eAreaAlarmState.EntranceDelayActive:
                    return "Entrance Delay Active";
                    break;
                case eAreaAlarmState.AlarmAbortDelayActive:
                    return "Alarm Abort Delay Active";
                    break;
                case eAreaAlarmState.FireAlarm:
                    return "Fire Alarm";
                    break;
                case eAreaAlarmState.MedicalAlarm:
                    return "Medical Alarm";
                    break;
                case eAreaAlarmState.PoliceAlarm:
                    return "Police Alarm";
                    break;
                case eAreaAlarmState.BurglarAlarm:
                    return "Burglar Alarm";
                    break;
                case eAreaAlarmState.Aux1Alarm:
                    return "Aux 1 Alarm";
                    break;
                case eAreaAlarmState.Aux2Alarm:
                    return "Aux 2 Alarm";
                    break;
                case eAreaAlarmState.Aux3Alarm:
                    return "Aux 3 Alarm";
                    break;
                case eAreaAlarmState.Aux4Alarm:
                    return "Aux 4 Alarm";
                    break;
                case eAreaAlarmState.CarbonMonoxideAlarm:
                    return "Carbon Monoxide Alarm";
                    break;
               case eAreaAlarmState. EmergencyAlarm:
                    return "Emergency Alarm";
                    break;
                case eAreaAlarmState.FreezeAlarm:
                    return "Freeze Alarm";
                    break;
                case eAreaAlarmState.GasAlarm:
                    return "Gas Alarm";
                    break;
                case eAreaAlarmState.HeatAlarm:
                    return "Heat Alarm";
                    break;
                case eAreaAlarmState.WaterAlarm:
                    return "Water Alarm";
                    break;
                case eAreaAlarmState.FireSupervisory:
                    return "Fire Supervisory";
                    break;
                case eAreaAlarmState.VerifyFire:
                    return "Verify Fire";
                    break;
                default:
                    return "";
                    break;
            }
        }

        public string getAreaDescription { get { return _name; } }



        //Internal Functions -------------------------------------------------------
        public void internalSetAreaArmedStatus(int s)
        {
            eAreaArmedStatus te = (eAreaArmedStatus)Enum.Parse(typeof(eAreaArmedStatus), Convert.ToString(s), true);
            if (te != _armedStatus)
            {
                _armedStatus = te;
                myPanel.SendDebug(string.Format("Area {0} - internalSetAreaArmedStatus = {1}", areaNumber, _armedStatus.ToString()));
                OnElkAreaEvent(eElkAreaEventUpdateType.ArmedStatusChange);
            }
        }

        public void internalSetAreaArmUpState(int s)
        {
            eAreaArmUpState te = (eAreaArmUpState)Enum.Parse(typeof(eAreaArmUpState), Convert.ToString(s), true);
            if (te != _armUpState)
            {
                _armUpState = te;
                myPanel.SendDebug(string.Format("Area {0} - internalSetAreaArmUpState = {1}", areaNumber, _armUpState.ToString()));
                OnElkAreaEvent(eElkAreaEventUpdateType.ArmUpStatChange);
            }
        }

        public void internalSetAreaAlarmState(int s)
        {
            eAreaAlarmState te = (eAreaAlarmState)Enum.Parse(typeof(eAreaAlarmState), Convert.ToString(s), true);
            if (te != _alarmState)
            {
                _alarmState = te;
                myPanel.SendDebug(string.Format("Area {0} - internalSetAreaAlarmState = {1}", areaNumber, _alarmState.ToString()));
                OnElkAreaEvent(eElkAreaEventUpdateType.AlarmStateChange);
            }
        }

        public void internalSetAreaName(string name)
        {
            if (_name != name)
            {
                _name = name;
                myPanel.SendDebug(string.Format("Area {0} - internalSetAreaName = {1}", areaNumber, _name));
                OnElkAreaEvent(eElkAreaEventUpdateType.NameChange);
            }
        }
        

        //Events -------------------------------------------------------
        public event EventHandler<ElkAreaEventArgs> ElkAreaEvent;
        protected virtual void OnElkAreaEvent(eElkAreaEventUpdateType updateType)
        {
            if (ElkAreaEvent != null)
                ElkAreaEvent(this, new ElkAreaEventArgs() { Area = areaNumber, EventUpdateType = updateType });
        }


    }


    //Enum's -------------------------------------------------------
    public enum eAreaArmSet
    {
        Disarm = 0,
        ArmedAway = 1,
        ArmedStay = 2,
        ArmedStayInstant = 3,
        ArmedNight = 4,
        ArmedNightInstant = 5,
        ArmedVacation = 6,
        ArmToNextAwayMode = 7,
        ArmToNextStayMode = 8,
        ForceArmToAwayMode = 9,
        ForceArmToStayMode = 10
    }
    public enum eAreaArmedStatus
    {
        Disarmed = 0,
        ArmedAway = 1,
        ArmedStay = 2,
        ArmedStayInstant = 3,
        ArmedNight = 4,
        ArmedNightInstant = 5,
        ArmedVacation = 6
    }
    public enum eAreaArmUpState
    {
        NotReadyToArm = 0,
        ReadyToArm = 1,
        ReadyToArmIfForced = 2,
        ArmedWithExitTimer = 3,
        Armed = 4,
        ForceArmed = 5,
        ArmedWithBypass = 6
    }
    public enum eAreaAlarmState
    {
        NoAlarmActive = 0,
        EntranceDelayActive = 1,
        AlarmAbortDelayActive = 2,
        FireAlarm = 3,
        MedicalAlarm = 4,
        PoliceAlarm = 5,
        BurglarAlarm = 6,
        Aux1Alarm = 7,
        Aux2Alarm = 8,
        Aux3Alarm = 9, //not used
        Aux4Alarm = 10, //not used
        CarbonMonoxideAlarm = 11,
        EmergencyAlarm = 12,
        FreezeAlarm= 13,
        GasAlarm = 14,
        HeatAlarm = 15,
        WaterAlarm = 16,
        FireSupervisory = 17,
        VerifyFire = 18
    }


    //Events -------------------------------------------------------
    public class ElkAreaEventArgs : EventArgs
    {
        public int Area { get; set; }
        public eElkAreaEventUpdateType EventUpdateType { get; set; }
    }
    public enum eElkAreaEventUpdateType
    {
        ArmedStatusChange = 0,
        ArmUpStatChange = 1,
        AlarmStateChange = 2,
        NameChange = 3
    }
}