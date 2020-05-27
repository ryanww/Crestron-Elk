using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkKeypad
    {
        private int keypadNumber;
        private string keypadName;

        private bool isRegistered;

        private ElkPanel myPanel;
        public ElkPassword myPw = new ElkPassword();

        private eAreaArmedStatus armedStatus;
        private eAreaArmUpState armUpState;
        private eAreaAlarmState alarmState;
        private int countdownClock;
        private bool showTimer;

        //Init -------------------------------------------------------
        public void Initialize(ElkPanel _panel, int _kp)
        {
            keypadNumber = _kp;
            myPanel = _panel;
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
        
        public int GetKeypadNumber { get { return keypadNumber]; } }

        //Core internal -------------------------------------------------------
        internal void internalSetAreaArmedStatus(int s)
        {
            eAreaArmedStatus te = (eAreaArmedStatus)Enum.Parse(typeof(eAreaArmedStatus), Convert.ToString(s), true);
            if (te != armedStatus)
            {
                armedStatus = te;
                myPanel.SendDebug(string.Format("Area {0} - internalSetAreaArmedStatus = {1}", areaNumber, armedStatus.ToString()));
                OnElkAreaEvent(eElkAreaEventUpdateType.ArmedStatusChange);
            }
            checkRegistered();
        }

        

        //Private Functions -------------------------------------------------------
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