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

        public void SelectUserDevice(string userDevice)
        {
            if (myNotificationManager.notificationDevices.ContainsKey(userDevice))
            {
                myNotificationDevice = myNotificationManager.notificationDevices[userDevice];
                if (newUserDeviceChange != null) newUserDeviceChange(myNotificationDevice.DeviceName);
            }
        }

        public void SelectNotificationArea(uint area)
        {
            myNotificationArea = myNotificationDevice.NotificationAreas[(int)area];
            if (OnUserDeviceAreaChange != null) OnUserDeviceAreaChange(myNotificationArea, new EventArgs());
        }

        public void PropertyToggle(string userDevice, ushort area, string property)
        {
            myNotificationManager.PropertyToggle(userDevice, (int)area, property);
        }
    }
}