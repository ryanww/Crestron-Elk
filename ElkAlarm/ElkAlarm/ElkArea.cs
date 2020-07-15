using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using WMS.Utilities;

namespace ElkAlarm
{
    public class ElkArea
    {
        private int areaNumber;
        private string areaName;

        private bool isRegistered;

        private ElkPanel myPanel;
        public ElkPassword myPw = new ElkPassword();

        private eAreaArmedStatus armedStatus = eAreaArmedStatus.Uninitialized;
        private eAreaArmUpState armUpState = eAreaArmUpState.Uninitialized;
        private eAreaAlarmState alarmState = eAreaAlarmState.Uninitialized;
        private int countdownClock = -1;
        private bool showTimer;
        private StopwatchTimer entryExitTimer;
        private bool entryExitTimerIsRunning = false;

        //Init -------------------------------------------------------
        public void Initialize(ElkPanel _panel, int _area)
        {
            areaNumber = _area;
            myPanel = _panel;
            entryExitTimer = new StopwatchTimer();
            entryExitTimer.CountdownFb += new TimerEventHandler(entryExitTimer_CountdownFb);
            entryExitTimer.CountdownRunningFb += new BoolEventHandler(entryExitTimer_CountdownRunning);
        }

        //Public Functions -------------------------------------------------------
        public void SetArmLevel(eAreaArmSet arm)
        {
            if (myPw.IsValidCodeEntered())
            {
                char al = '0';
                al += (char)(int)arm;

                string pw = myPw.getPassword();

                string cmdStr = string.Format("a{0}{1}{2}00", al, areaNumber, pw);
                myPanel.SendDebug(string.Format("Area {0} - SetArmLevel = {1} ({2})", areaNumber, arm, cmdStr));
                myPanel.Enqueue(cmdStr);
            }
        }

        public void UnbypassAllZones()
        {
            string cmdStr = string.Format("zb000{0}{1}00", areaNumber, myPw.getPassword());
            myPanel.SendDebug(string.Format("Area {0} - UnbypassAllZones = {1}", areaNumber, cmdStr));
            myPanel.Enqueue(cmdStr);
        }

        public void BypassAllZones()
        {
            string cmdStr = string.Format("zb999{0}{1}00", areaNumber, myPw.getPassword());
            myPanel.SendDebug(string.Format("Area {0} - BypassAllZones = {1}", areaNumber, cmdStr));
            myPanel.Enqueue(cmdStr);
        }

        public eAreaArmedStatus GetAreaArmedStatus { get { return armedStatus; } }

        public string GetAreaArmedStatusString
        {
            get
            {
                string ret = "";
                switch (armedStatus)
                {
                    case eAreaArmedStatus.Disarmed:
                        ret = "Disarmed";
                        break;
                    case eAreaArmedStatus.ArmedAway:
                        ret = "Armed Away";
                        break;
                    case eAreaArmedStatus.ArmedStay:
                        ret = "Armed Stay";
                        break;
                    case eAreaArmedStatus.ArmedStayInstant:
                        ret = "Armed Stay Instant";
                        break;
                    case eAreaArmedStatus.ArmedNight:
                        ret = "Armed Night";
                        break;
                    case eAreaArmedStatus.ArmedNightInstant:
                        ret = "Armed Night Instant";
                        break;
                    case eAreaArmedStatus.ArmedVacation:
                        ret = "Armed Vacation";
                        break;
                    default:
                        ret = "";
                        break;
                }
                return ret;
            }
        }

        public eAreaArmUpState GetAreaArmUpState { get { return armUpState; } }

        public string GetAreaArmUpStateString
        {
            get
            {
                string ret = "";
                switch (armUpState)
                {
                    case eAreaArmUpState.NotReadyToArm:
                        ret = "Not Ready To Arm";
                        break;
                    case eAreaArmUpState.ReadyToArm:
                        ret = "Ready To Arm";
                        break;
                    case eAreaArmUpState.ReadyToArmIfForced:
                        ret = "Ready To Arm If Forced";
                        break;
                    case eAreaArmUpState.ArmedWithExitTimer:
                        ret = "Armed With Exit Timer";
                        break;
                    case eAreaArmUpState.Armed:
                        ret = "Armed";
                        break;
                    case eAreaArmUpState.ForceArmed:
                        ret = "Force Armed";
                        break;
                    case eAreaArmUpState.ArmedWithBypass:
                        ret = "Armed With Bypass";
                        break;
                    default:
                        ret = "";
                        break;
                }
                return ret;
            }
        }

        public eAreaAlarmState GetAlarmStatus { get { return alarmState; } }

        public string GetAlarmStatusString
        {
            get
            {
                string ret = "";
                switch (alarmState)
                {
                    case eAreaAlarmState.NoAlarmActive:
                        ret = "No Alarm Active";
                        break;
                    case eAreaAlarmState.EntranceDelayActive:
                        ret = "Entrance Delay Active";
                        break;
                    case eAreaAlarmState.AlarmAbortDelayActive:
                        ret = "Alarm Abort Delay Active";
                        break;
                    case eAreaAlarmState.FireAlarm:
                        ret = "Fire Alarm";
                        break;
                    case eAreaAlarmState.MedicalAlarm:
                        ret = "Medical Alarm";
                        break;
                    case eAreaAlarmState.PoliceAlarm:
                        ret = "Police Alarm";
                        break;
                    case eAreaAlarmState.BurglarAlarm:
                        ret = "Burglar Alarm";
                        break;
                    case eAreaAlarmState.Aux1Alarm:
                        ret = "Aux 1 Alarm";
                        break;
                    case eAreaAlarmState.Aux2Alarm:
                        ret = "Aux 2 Alarm";
                        break;
                    case eAreaAlarmState.Aux3Alarm:
                        ret = "Aux 3 Alarm";
                        break;
                    case eAreaAlarmState.Aux4Alarm:
                        ret = "Aux 4 Alarm";
                        break;
                    case eAreaAlarmState.CarbonMonoxideAlarm:
                        ret = "Carbon Monoxide Alarm";
                        break;
                    case eAreaAlarmState.EmergencyAlarm:
                        ret = "Emergency Alarm";
                        break;
                    case eAreaAlarmState.FreezeAlarm:
                        ret = "Freeze Alarm";
                        break;
                    case eAreaAlarmState.GasAlarm:
                        ret = "Gas Alarm";
                        break;
                    case eAreaAlarmState.HeatAlarm:
                        ret = "Heat Alarm";
                        break;
                    case eAreaAlarmState.WaterAlarm:
                        ret = "Water Alarm";
                        break;
                    case eAreaAlarmState.FireSupervisory:
                        ret = "Fire Supervisory";
                        break;
                    case eAreaAlarmState.VerifyFire:
                        ret = "Verify Fire";
                        break;
                    default:
                        ret = "";
                        break;
                }
                return ret;
            }
        }

        public string GetAreaName { get { return areaName; } }

        public string GetAlarmCountdownClockString
        {
            get
            {
                string c = "";
                //Exit
                if (armUpState == eAreaArmUpState.ArmedWithExitTimer)
                    c = string.Format("Exit Timer: {0}", countdownClock);
                //Enter
                if (alarmState == eAreaAlarmState.EntranceDelayActive)
                    c = string.Format("Enter Timer: {0}", countdownClock);
                return c;
            }
        }

        public bool GetAlarmCountdownClockShow { get { return showTimer; } }

        public int GetAreaNumber { get { return areaNumber; } }

        //Core internal -------------------------------------------------------
        internal void internalSetAreaArmedStatus(int s)
        {
            eAreaArmedStatus te = (eAreaArmedStatus)Enum.Parse(typeof(eAreaArmedStatus), Convert.ToString(s), true);
            if (te != armedStatus)
            {
                armedStatus = te;
                if (armedStatus == eAreaArmedStatus.Disarmed) entryExitTimer.Reset();
                myPanel.SendDebug(string.Format("Area {0} - internalSetAreaArmedStatus = {1}", areaNumber, armedStatus.ToString()));
                OnElkAreaEvent(eElkAreaEventUpdateType.ArmedStatusChange);
            }
            checkRegistered();
        }

        internal void internalSetAreaArmUpState(int s)
        {
            eAreaArmUpState te = (eAreaArmUpState)Enum.Parse(typeof(eAreaArmUpState), Convert.ToString(s), true);
            if (te != armUpState)
            {
                armUpState = te;
                myPanel.SendDebug(string.Format("Area {0} - internalSetAreaArmUpState = {1}", areaNumber, armUpState.ToString()));
                OnElkAreaEvent(eElkAreaEventUpdateType.ArmUpStatChange);

                //checkTimer();
            }
            checkRegistered();
        }

        internal void internalSetAreaAlarmState(int s)
        {
            eAreaAlarmState te = (eAreaAlarmState)Enum.Parse(typeof(eAreaAlarmState), Convert.ToString(s), true);
            if (te != alarmState)
            {
                alarmState = te;
                myPanel.SendDebug(string.Format("Area {0} - internalSetAreaAlarmState = {1}", areaNumber, alarmState.ToString()));
                OnElkAreaEvent(eElkAreaEventUpdateType.AlarmStateChange);

                //checkTimer();
            }
            checkRegistered();
        }

        internal void internalSetAreaName(string name)
        {
            if (areaName != name)
            {
                areaName = name;
                myPanel.SendDebug(string.Format("Area {0} - internalSetAreaName = {1}", areaNumber, areaName));
                OnElkAreaEvent(eElkAreaEventUpdateType.NameChange);
            }
            checkRegistered();
        }

        internal void internalSetCountdownClock(int timerType, int timer1, int timer2, int armedState)
        {
            if (timer1 > 0 && armedState != 0)
            {
                myPanel.SendDebug(string.Format("Area {0} - internalSetCountdownClock = {1}", areaNumber, timer1));

                entryExitTimer = new StopwatchTimer();
                entryExitTimer.SetSeconds(timer1);
                entryExitTimer.StartCountdown();
            }

            checkRegistered();
        }

        private void entryExitTimer_CountdownFb(object sender, TimerEventArgs e)
        {
            if (countdownClock != e.seconds)
            {
                countdownClock = e.seconds;
                OnElkAreaEvent(eElkAreaEventUpdateType.ClockChange);
                CrestronConsole.PrintLine("{0} - Exit Timer: {1}", areaName, e.seconds);
            }
        }

        private void entryExitTimer_CountdownRunning(object sender, BoolEventArgs e)
        {
            entryExitTimerIsRunning = e.val == 1 ? true : false;

            showTimer = entryExitTimerIsRunning;

            if (!entryExitTimerIsRunning)
            {
                OnElkAreaEvent(eElkAreaEventUpdateType.ClockChange);
                myPanel.SendDebug(String.Format("{0} - EntryExit Timer - Running: {1}", areaName, entryExitTimerIsRunning));
                countdownClock = -1;
            }
        }

        internal void internalZoneAssignmentChanged()
        {
            OnElkAreaEvent(eElkAreaEventUpdateType.ZoneAssignmentChange);
        }

        //Private Functions -------------------------------------------------------
        private void checkTimer()
        {
            bool t;
            if (alarmState == eAreaAlarmState.EntranceDelayActive || armUpState == eAreaArmUpState.ArmedWithExitTimer)
                t = true;
            else
                t = false;

            if (showTimer != t)
            {
                myPanel.SendDebug(string.Format("Area {0} - checkTimer = {1}", areaNumber, showTimer));
                OnElkAreaEvent(eElkAreaEventUpdateType.ClockChange);
            }
            //TODO: Implement Timer
        }

        private void checkRegistered()
        {
            if (!isRegistered)
            {
                string cmdStr = string.Format("sd01{0:000}00", areaNumber);
                myPanel.SendDebug(string.Format("Area {0} - checkRegistered = {1}", areaNumber, cmdStr));
                myPanel.Enqueue(cmdStr);
                isRegistered = true;
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
        Uninitialized = -1,
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
        Uninitialized = -1,
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
        Uninitialized = -1,
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
        FreezeAlarm = 13,
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
        NameChange = 3,
        ClockChange = 4,
        ZoneAssignmentChange = 5
    }
}