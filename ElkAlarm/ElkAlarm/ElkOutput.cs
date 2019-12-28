using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkOutput
    {
        private int outputNumber;
        private bool isRegistered;

        private ElkPanel myPanel;

        private string outputName;
        private bool outputState;

        //Init -------------------------------------------------------
        public void Initialize(ElkPanel _panel, int _out)
        {
            outputNumber = _out;
            myPanel = _panel;
        }


        //Public Functions -------------------------------------------------------
        public void SetOutputOn(int _sec)
        {
            myPanel.SendDebug(string.Format("Output {0} - SetOutputOn, seconds: {1}", outputNumber, _sec));
            myPanel.Enqueue(string.Format("cn{0:000}{1:00000}00", outputNumber, _sec));
        }
        public void SetOutputOff()
        {
            myPanel.SendDebug(string.Format("Output {0} - SetOutputOff", outputNumber));
            myPanel.Enqueue(string.Format("cf{0:000}00", outputNumber));
        }
        public void SetOutputToggle()
        {
            myPanel.SendDebug(string.Format("Output {0} - SetOutputToggle"));
            myPanel.Enqueue(string.Format("ct{0:000}00", outputNumber));
        }

        public bool getOutputState { get { return outputState; } }
        public string getOutputName { get { return outputName; } }
        public int getOutputNumber { get { return outputNumber; } }


        //Internal Functions -------------------------------------------------------
        internal void internalSetOutputName(string name)
        {
            if (outputName != name)
            {
                outputName = name;
                myPanel.SendDebug(string.Format("Output {0} - internalSetOutputName = {1}", outputNumber, outputName));
                OnElkOutputEvent(eElkOutputEventUpdateType.NameChange);
            }
            checkRegistered();
        }

        internal void internalOutputStateSet(bool state)
        {
            if (outputState != state)
            {
                outputState = state;
                myPanel.SendDebug(string.Format("Output {0} - internalOutputStateSet = {1}", outputNumber, outputState));
                OnElkOutputEvent(eElkOutputEventUpdateType.StateChange);
            }
            checkRegistered();
        }

        //Private Functions -------------------------------------------------------
        private void checkRegistered()
        {
            if (!isRegistered)
            {
                string cmdStr = string.Format("sd04{0:000}00", outputNumber);
                myPanel.SendDebug(string.Format("Output {0} - checkRegistered = {1}", outputNumber, cmdStr));
                myPanel.Enqueue(cmdStr);
                isRegistered = true;
            }
        }


        //Events -------------------------------------------------------
        public event EventHandler<ElkOutputEventArgs> ElkOutputEvent;
        protected virtual void OnElkOutputEvent(eElkOutputEventUpdateType updateType)
        {
            if (ElkOutputEvent != null)
                ElkOutputEvent(this, new ElkOutputEventArgs() { Output = outputNumber, EventUpdateType = updateType });
        }
    }


    //Events -------------------------------------------------------
    public class ElkOutputEventArgs : EventArgs
    {
        public int Output { get; set; }
        public eElkOutputEventUpdateType EventUpdateType { get; set; }
    }
    public enum eElkOutputEventUpdateType
    {
        StateChange = 0,
        NameChange = 1
    }
}