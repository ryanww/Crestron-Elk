using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkPanelSimpl
    {
        private bool isRegistered;
        private bool debug;

        ElkPanel myPanel;

        public delegate void IsRegistered(ushort value);
        public delegate void IsConnected(ushort value);

        public IsRegistered onIsRegistered { get; set; }

        public IsConnected onIsConnected { get; set; }

        public void PropertyToggle(string userDevice, int area, string property)
        {
            this.myPanel.NotificationManager.PropertyToggle(userDevice, area, property);
        }

        public void Initialize(ushort _panelId, SimplSharpString _host, ushort _port)
        {
            myPanel = ElkCore.AddOrGetCoreObject(_panelId);
            if (myPanel.getPanelIp.Length == 0)
            {
                myPanel.SetDebug(debug);
                myPanel.InitialzeConnection(_host.ToString(), _port);
                myPanel.RegisterSimplClient(Convert.ToString(_panelId));
                myPanel.SimplClients[Convert.ToString(_panelId)].OnNewEvent += new EventHandler<SimplEventArgs>(ElkPanel_SimplEvent);
                this.isRegistered = true;
            }
            else
            {
                ErrorLog.Error("Elk Panel with ID {0} already exists - please only use one panel instance per panel within project.", _panelId);
            }
        }

        public void SetDebug(ushort _value)
        {
            this.debug = Convert.ToBoolean(_value);
            if (isRegistered)
                this.myPanel.SetDebug(debug);
        }

        private void ElkPanel_SimplEvent(object sender, SimplEventArgs e)
        {
            switch (e.ID)
            {
                case eElkSimplEventIds.IsRegistered:
                    if (onIsRegistered != null)
                        onIsRegistered(e.IntData);
                    break;
                case eElkSimplEventIds.IsConnected:
                    if (onIsConnected != null)
                        onIsConnected(e.IntData);
                    break;
                default:
                    break;
            }
        }
    }
}