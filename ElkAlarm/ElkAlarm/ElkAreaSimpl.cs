using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkAreaSimpl
    {
        public delegate void ArmedStatusChange(ushort _armedStatus);

        public ArmedStatusChange newArmedStatusChange { get; set; }

        public delegate void ArmUpStateChange(ushort _armUpState);

        public ArmUpStateChange newArmUpStateChange { get; set; }

        public delegate void AlarmStateChange(ushort _alarmState);

        public AlarmStateChange newAlarmStatusChange { get; set; }

        public delegate void NameChange(SimplSharpString _name);

        public NameChange newNameChange { get; set; }

        public delegate void FunctionKeyNameChange(CrestronStringArray names);

        public FunctionKeyNameChange newFunctionKeyNameChange { get; set; }

        public delegate void ClockChange(SimplSharpString _clock, ushort _timerRunning);

        public ClockChange newClockChange { get; set; }

        public delegate void PwChange(SimplSharpString _pw);

        public PwChange newPwChange { get; set; }

        public delegate void ZoneAssignmentChange();

        public ZoneAssignmentChange newZoneAssignmentChange { get; set; }

        private ElkPanel myPanel;
        private ElkArea myArea;

        //Init -------------------------------------------------------
        public void Initialize(ushort _panel, ushort _areaNum)
        {
            myPanel = ElkCore.AddOrGetCoreObject(_panel);
            if (myPanel == null)
                return;

            if (myPanel.Areas.ContainsKey((int)_areaNum))
            {
                myArea = myPanel.Areas[(int)_areaNum];
                myArea.ElkAreaEvent += new EventHandler<ElkAreaEventArgs>(myArea_ElkAreaEvent);
                myArea.myPw.ElkPasswordEvent += new EventHandler<ElkPasswordEventArgs>(myPw_ElkPasswordEvent);
            }
        }

        //Public Functions -------------------------------------------------------

        public void SetBypassPassword(SimplSharpString _pw)
        {
            myArea.bypassPassword = _pw.ToString();
        }

        public void SetArmState(ushort _armState)
        {
            eAreaArmSet tas = (eAreaArmSet)Enum.Parse(typeof(eAreaArmSet), Convert.ToString(_armState), true);
            myArea.SetArmLevel(tas);
        }

        public void KeypadNumber(ushort _b)
        {
            myArea.myPw.AddKeyToPassword((int)_b);
        }

        public void KeypadBackspace()
        {
            myArea.myPw.Backspace();
        }

        public void KeypadClear()
        {
            myArea.myPw.ClearPassword();
        }

        public void KeypadFunctionPress(string _k)
        {
            myArea.KeypadFunctionPress(_k);
        }

        public ushort GetAlarmCountdownClockShow()
        {
            return myArea.GetAlarmCountdownClockShow ? (ushort)1 : (ushort)0;
        }

        public SimplSharpString GetAreaArmedStatusString()
        {
            return (SimplSharpString)myArea.GetAreaArmedStatusString;
        }

        public SimplSharpString GetAreaArmUpStateString()
        {
            SimplSharpString AreaArmUpStateText = (SimplSharpString)myArea.GetAreaArmUpStateString;

            return AreaArmUpStateText;
        }

        public SimplSharpString GetAlarmStatusString()
        {
            return (SimplSharpString)myArea.GetAlarmStatusString;
        }

        private CrestronStringArray GetFunctionKeyNames()
        {
            var functionKeyNames = new CrestronStringArray(myArea.functionKeyNames);
            return functionKeyNames;
        }

        public ushort GetZoneAssignment(ushort _zone)
        {
            if (myPanel.Zones.ContainsKey(_zone))
                return (ushort)myPanel.Zones[_zone].GetZoneAreaAssignment == myArea.GetAreaNumber ? (ushort)1 : (ushort)0;
            else
                return (ushort)0;
        }

        //Events -------------------------------------------------------
        private void myArea_ElkAreaEvent(object sender, ElkAreaEventArgs e)
        {
            switch (e.EventUpdateType)
            {
                case eElkAreaEventUpdateType.ArmedStatusChange:
                    if (newArmedStatusChange != null)
                        newArmedStatusChange((ushort)myArea.GetAreaArmedStatus);
                    break;
                case eElkAreaEventUpdateType.ArmUpStatChange:
                    if (newArmUpStateChange != null)
                        newArmUpStateChange((ushort)myArea.GetAreaArmUpState);
                    break;
                case eElkAreaEventUpdateType.AlarmStateChange:
                    if (newAlarmStatusChange != null)
                        newAlarmStatusChange((ushort)myArea.GetAlarmStatus);
                    break;
                case eElkAreaEventUpdateType.NameChange:
                    if (newNameChange != null)
                        newNameChange((SimplSharpString)myArea.GetAreaName);
                    break;
                case eElkAreaEventUpdateType.ClockChange:
                    if (newClockChange != null)
                        newClockChange((SimplSharpString)myArea.GetAlarmCountdownClockString, (ushort)GetAlarmCountdownClockShow());
                    break;
                case eElkAreaEventUpdateType.ZoneAssignmentChange:
                    if (newZoneAssignmentChange != null)
                        newZoneAssignmentChange();
                    break;
                case eElkAreaEventUpdateType.FunctionKeyNameChange:
                    if (newFunctionKeyNameChange != null)
                        newFunctionKeyNameChange(GetFunctionKeyNames());
                    break;
            }
        }

        private void myPw_ElkPasswordEvent(object sender, ElkPasswordEventArgs e)
        {
            if (newPwChange != null)
                newPwChange((SimplSharpString)e.Password);
        }
    }
}