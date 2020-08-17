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

        public event EventHandler OnUserDeviceZoneChange;

        private ElkPanel myPanel;
        private NotificationDevice myNotificationDevice;
        private NotificationArea myNotificationArea;
        private ElkNotificationManager myNotificationManager;

        public NotificationZone[] myNotificationZoneArray = new NotificationZone[209];

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

        public void Build()
        {
            myNotificationManager.BuildNotificationConfig();
        }

        public void Load()
        {
            myNotificationManager.LoadNotificationConfig();
        }

        public void SelectUserDevice(ushort userDevice)
        {
            userDevice = (ushort)(userDevice - 1);
            try
            {
                if (userDevice >= 0 && userDevice <= myNotificationManager.notificationDevices.Count)
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
            if (!myNotificationDevice.NotificationAreas.ContainsKey((int)area)) return;
            try
            {
                myNotificationArea = myNotificationDevice.NotificationAreas[(int)area];
                myNotificationZoneArray = myNotificationDevice.NotificationZones;

                if (OnUserDeviceAreaChange != null) OnUserDeviceAreaChange(myNotificationArea, new EventArgs());
            }
            catch (Exception ex)
            {
                myPanel.SendDebug(String.Format("Pushover Notification Manager Error Selecting User Device: {0}", ex.ToString()));
            }
        }

        public ushort ToggleZoneProperty(string userDevice, ushort zone, string property)
        {
            if (String.IsNullOrEmpty(userDevice)) return 0;
            ushort newValue = 0;
            if (myNotificationManager.notificationDevices.ContainsKey(userDevice) && zone <= 209)
            {
                switch (property)
                {
                    case "ArmedNotifications":
                        newValue = myNotificationZoneArray[zone].ArmedNotifications = (ushort)(myNotificationZoneArray[zone].ArmedNotifications == 1 ? 0 : 1);
                        myNotificationManager.notificationDevices[userDevice].NotificationZones[zone].ArmedNotifications
                            = newValue;

                        break;
                    case "DisarmedNotifications":
                        newValue = myNotificationZoneArray[zone].DisarmedNotifications = (ushort)(myNotificationZoneArray[zone].DisarmedNotifications == 1 ? 0 : 1);
                        myNotificationManager.notificationDevices[userDevice].NotificationZones[zone].DisarmedNotifications
                            = newValue;

                        break;
                }
            }

            return newValue;
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
            if (String.IsNullOrEmpty(userDevice)) return 0;
            return myNotificationManager.PropertyToggle(userDevice, (int)area, property);
        }
    }
}