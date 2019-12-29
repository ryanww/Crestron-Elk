using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkOutputSimpl
    {

        public delegate void StateChange(ushort _state);
        public StateChange newStateChange { get; set; }
        public delegate void NameChange(SimplSharpString _name);
        public NameChange newNameChange { get; set; }

        private ElkPanel myPanel;
        private ElkOutput myOutput;

        //Init -------------------------------------------------------
        public void Initialize(ushort _panel, ushort _outputNumber)
        {
            myPanel = ElkCore.AddOrGetCoreObject(_panel);
            if (myPanel == null)
                return;

            if (myPanel.Outputs.ContainsKey((int)_outputNumber))
            {
                myOutput = myPanel.Outputs[(int)_outputNumber];
                myOutput.ElkOutputEvent +=new EventHandler<ElkOutputEventArgs>(myOutput_ElkOutputEvent);
            }
        }



        //Public Functions -------------------------------------------------------
        public void SetOutputOn(ushort _seconds)
        {
            myOutput.SetOutputOn((int)_seconds);
        }
        public void SetOutputOff()
        {
            myOutput.SetOutputOff();
        }
        public void SetOutputToggle()
        {
            myOutput.SetOutputToggle();
        }
        

        //Events -------------------------------------------------------
        void  myOutput_ElkOutputEvent(object sender, ElkOutputEventArgs e)
        {
            switch (e.EventUpdateType)
            {
                case eElkOutputEventUpdateType.StateChange:
                    if (myOutput.GetOutputState == true)
                        newStateChange((ushort)1);
                    else
                        newStateChange((ushort)0);
                    break;
                case eElkOutputEventUpdateType.NameChange:
                    newNameChange((SimplSharpString)myOutput.GetOutputName);
                    break;
            }
        }
    }
}