using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkNotificationManagerSimpl
    {
        public delegate void OnUserDeviceChange(SimplSharpString deviceName);

        public OnUserDeviceChange newUserDeviceChange { get; set; }

        public event EventHandler OnUserDeviceAreaChange;

        private ElkPanel myPanel;
        private NotificationDevice myNotificationDevice;
        private NotificationArea myNotificationArea;
        private ElkNotificationManager myNotificationManager;

        public ElkNotificationManagerSimpl()
        {
        }

        public void Initialize(ushort _panel)
        {
            myPanel = ElkCore.AddOrGetCoreObject(_panel);
            myNotificationManager = myPanel.NotificationManager;
            if (myPanel == null)
                return;
        }

        public void SelectUserDevice(ushort userDevice)
        {
            try
            {
                if (userDevice <= myNotificationManager.notificationDevices.Count)
                {
                    myNotificationDevice = myNotificationManager.notificationDevices.ElementAt(userDevice).Value;
                    if (newUserDeviceChange != null) newUserDeviceChange(myNotificationDevice.DeviceName);
                }
            }
            catch (Exception ex)
            {
                myPanel.SendDebug(String.Format("Pushover Notification Manager Error Selecting User Device: {0}", ex.ToString()));
            }
        }

        public void SelectNotificationArea(uint area)
        {
            if (area == 0) return;
            try
            {
                myNotificationArea = myNotificationDevice.NotificationAreas[(int)area];
                if (OnUserDeviceAreaChange != null) OnUserDeviceAreaChange(myNotificationArea, new EventArgs());
            }
            catch (Exception ex)
            {
                myPanel.SendDebug(String.Format("Pushover Notification Manager Error Selecting User Device: {0}", ex.ToString()));
            }
        }

        public void SaveNotificationConfig()
        {
            myNotificationManager.SaveNotificationConfig();
        }

        public void SetConfigFileName(string path)
        {
            myNotificationManager.configFileName = path;
        }

        public ushort PropertyToggle(string userDevice, ushort area, string property)
        {
            return myNotificationManager.PropertyToggle(userDevice, (int)area, property);
        }
    }
}