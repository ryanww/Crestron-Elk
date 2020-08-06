using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using CrestronPushover;
using Newtonsoft.Json;

namespace ElkAlarm
{
    public class ElkNotificationManager
    {
        private ElkPanel myPanel;
        private bool panelInitialized;
        private bool pushoverInitialized;
        private Dictionary<string, NotificationDevice> notificationDevices = new Dictionary<string, NotificationDevice>();

        public ElkNotificationManager(ElkPanel _panel)
        {
            myPanel = _panel;
            myPanel.OnElkPanelInitializedChanged += myPanel_OnElkPanelInitializedChanged;
            PushoverManager.Instance.OnPushoverInitializedChange += OnPushoverInitializedChange;
            PushoverManager.Instance.PushoverUpdateEvent += Instance_PushoverUpdateEvent;
        }

        private void OnPushoverInitializedChange(bool status)
        {
            myPanel.SendDebug("*****Pushover Init Event*****");
            pushoverInitialized = status;
            if (pushoverInitialized && panelInitialized) LoadNotificationConfig();
        }

        private void LoadNotificationConfig()
        {
            myPanel.SendDebug("*****Loading Notification Config*****");
            foreach (var device in PushoverManager.Instance.UserDevices)
            {
                if (!notificationDevices.ContainsKey(device)) notificationDevices.Add(device, new NotificationDevice(device));
            }

            foreach (var device in notificationDevices)
            {
                foreach (var area in myPanel.Areas)
                {
                    notificationDevices[device.Key].NotificationAreas.Add(myPanel.Areas[area.Key].GetAreaNumber, new NotificationArea(myPanel.Areas[area.Key].GetAreaNumber, myPanel.Areas[area.Key].GetAreaName));
                }
            }

            try
            {
                myPanel.SendDebug("*****Serializing Notification Config*****");
                string json = JsonConvert.SerializeObject(notificationDevices);
                CrestronConsole.PrintLine(json);
            }
            catch (Exception ex)
            {
                myPanel.SendDebug("*****Error On Loading Notification Config*****");
            }
        }

        private void Instance_PushoverUpdateEvent(object sender, PushoverUpdateEventArgs e)
        {
        }

        private void myPanel_OnElkPanelInitializedChanged(bool status)
        {
            myPanel.SendDebug("*****Pushover Panel Init Event*****");
            panelInitialized = status;
            if (pushoverInitialized && panelInitialized) LoadNotificationConfig();
        }
    }

    public class NotificationDevice
    {
        public NotificationDevice(string _name)
        {
            DeviceName = _name;
        }

        public string DeviceName;
        public bool EnableNotifications;
        public Dictionary<int, NotificationArea> NotificationAreas = new Dictionary<int, NotificationArea>();
    }

    public class NotificationArea
    {
        public NotificationArea(int _number, string _name)
        {
            AreaNumber = _number;
            AreaName = _name;
        }

        public int AreaNumber;
        public string AreaName;
        public bool AreaNotifications;
        public bool ArmedStatus;
        public bool ArmUpStats;
        public bool ZoneViolated;
        public bool ZoneBypassed;
        public bool ZoneSoftBypassed;
    }
}