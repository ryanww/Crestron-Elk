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



    }

    public class Area
    {
        public int AreaNum;
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