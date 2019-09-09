using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkAreaSimpl
    {
        public delegate void ArmStatusChange(ushort armStatus);
        public ArmStatusChange newArmStatusChange { get; set; }
        public delegate void StateStatusChange(ushort statusState);
        public StateStatusChange newStateStatusChange { get; set; }
        public delegate void AlarmStatusChange(ushort statusState);
        public AlarmStatusChange newAlarmStatusChange { get; set; }
        public delegate void DescriptionChange(SimplSharpString description);
        public DescriptionChange newDescriptionChange { get; set; }

        private ElkArea area;

        public void Initialize(ushort areaNum)
        {
            area = new ElkArea(areaNum);
            area.ElkAreaEvent += new EventHandler<ElkAreaEventArgs>(area_ElkAreaEvent);
        }

        public void SetArmState(ushort armState)
        {
            eAreaArmSet _as = (eAreaArmSet)Enum.Parse(typeof(eAreaArmSet), Convert.ToString(armState), true);
            area.SetArm(_as);
        }


        private void area_ElkAreaEvent(object sender, ElkAreaEventArgs e)
        {
            switch (e.EventID)
            {
                case eElkAreaEventID.ArmStatusChange:
                    if (newArmStatusChange != null)
                        newArmStatusChange((ushort)area.getArmStatus);
                    break;
                case eElkAreaEventID.StateStatusChange:
                    if (newStateStatusChange != null)
                        newStateStatusChange((ushort)area.getStateStatus);
                    break;
                case eElkAreaEventID.AlarmStatusChange:
                    if (newAlarmStatusChange != null)
                        newAlarmStatusChange((ushort)area.getAlarmStatus);
                    break;
                case eElkAreaEventID.DescriptionChange:
                    if (newDescriptionChange != null)
                        newDescriptionChange((SimplSharpString)area.getAreaDescription);
                    break;
               
            }
        }


    }
}