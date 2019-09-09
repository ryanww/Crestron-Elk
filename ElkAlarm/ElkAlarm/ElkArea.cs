using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkArea
    {
        private int _aNum;
        private bool _registered;
        private eAreaArmStatus _armStatus;
        private eAreaStateStatus _stateStatus;
        private eAreaAlarmStatus _alarmStatus;
        private string _description;

        public event EventHandler<ElkAreaEventArgs> ElkAreaEvent;

        public eAreaArmStatus getArmStatus { get { return _armStatus; } }
        public eAreaStateStatus getStateStatus { get { return _stateStatus; } }
        public eAreaAlarmStatus getAlarmStatus { get { return _alarmStatus; } }
        public string getAreaDescription { get { return _description; } }
        public bool getIsRegistered { get { return _registered; } }

        public ElkArea(int area)
        {
            _aNum = area;
            if (ElkProcessor.RegisterZone(_aNum))
            {
                ElkProcessor.Areas[_aNum].OnNewEvent += new EventHandler<ElkInternalEventsArgs>(ElkArea_OnNewEvent);
                _registered = true;
            }
        }

        public void SetArm(eAreaArmSet arm)
        {
            //TODO: set area arm state
        }
        


        void ElkArea_OnNewEvent(object sender, ElkInternalEventsArgs e)
        {
            switch (e.Name)
            {
                case "STATUS":
                    switch ((int)e.Data)
                    {
                        case 0: { _armStatus = eAreaArmStatus.Disarmed; } break;
                        case 1: { _armStatus = eAreaArmStatus.ArmedAway; } break;
                        case 2: { _armStatus = eAreaArmStatus.ArmedStay; } break;
                        case 3: { _armStatus = eAreaArmStatus.ArmedStayInstant; } break;
                        case 4: { _armStatus = eAreaArmStatus.ArmedNight; } break;
                        case 5: { _armStatus = eAreaArmStatus.ArmedNightInstant; } break;
                        case 6: { _armStatus = eAreaArmStatus.ArmedVacation; } break;
                    }
                    break;
                case "STATE":
                    switch ((int)e.Data)
                    {
                        case 0: { _stateStatus = eAreaStateStatus.NotReadyToArm; } break;
                        case 1: { _stateStatus = eAreaStateStatus.ReadyToArm; } break;
                        case 2: { _stateStatus = eAreaStateStatus.ForceArmZoneFault; } break;
                        //case 3: { _stateStatus = eAreaStateStatus.Armed; } break;
                        case 4: { _stateStatus = eAreaStateStatus.Armed; } break;
                        case 5: { _stateStatus = eAreaStateStatus.ForceArmed; } break;
                        case 6: { _stateStatus = eAreaStateStatus.ArmedWithBypass; } break;
                    }
                    break;
                case "ALARM":
                    _alarmStatus = (eAreaAlarmStatus)Enum.Parse(typeof(eAreaStateStatus), Convert.ToString(e.Data), true);
                    break;

            }
        }

    }

    public class Area
    {
        public int AreaNum;
    }

    public enum eAreaArmSet
    {
        ArmAway = 0,
        ArmStay = 1,
        ArmNight = 2,
        ArmStayInstant = 3,
        ArmNightInstant = 4,
        ArmVacation = 5,
        Disarm = 6,
        ArmExitButton = 7,
        ArmStayButton = 8
    }

    public enum eAreaArmStatus
    {
        ArmedAway = 0,
        ArmedStay = 1,
        ArmedNight = 2,
        ArmedStayInstant = 3,
        ArmedNightInstant = 4,
        ArmedVacation = 5,
        Disarmed = 6
    }
    public enum eAreaStateStatus
    {
        NotReadyToArm = 0,
        ReadyToArm = 1,
        ForceArmZoneFault = 2,
        Armed = 3,
        ForceArmed = 4,
        ArmedWithBypass = 5
    }
    public enum eAreaAlarmStatus
    {
        NoAlarmActive = 0,
        EntranceDelayActive = 1,
        AlarmAbortDelayActive = 2,
        FireAlarmActive = 3,
        MedicalAlarmActive = 4,
        PoliceAlarmActive = 5,
        BurglarAlarmActive = 6,
        Aux1AlarmActive = 7,
        Aux2AlarmActive = 8,
        CarbonMonoxideAlarmActive = 9,
        EmergencyAlarmActive = 10,
        FreezeAlarmActive = 11,
        GasAlarmActive = 12,
        HeatAlarmActive = 13,
        WaterAlarmActive = 14,
        FireSupervisoryAlarmActive = 15,
        VerifyFireAlarmActive = 16
    }
}