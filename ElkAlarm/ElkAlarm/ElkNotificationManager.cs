using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using CrestronPushover;
using Newtonsoft.Json;

namespace ElkAlarm
{
    public class ElkNotificationManager
    {
        private ElkPanel myPanel;
        private bool panelInitialized;
        private bool pushoverInitialized;
        internal Dictionary<string, NotificationDevice> notificationDevices = new Dictionary<string, NotificationDevice>();

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
                    if (!myPanel.Areas[area.Key].GetAreaName.Contains("Area"))
                    {
                        NotificationArea newArea = new NotificationArea();
                        newArea.Initialize(myPanel.Areas[area.Key].GetAreaNumber, myPanel.Areas[area.Key].GetAreaName);
                        notificationDevices[device.Key].NotificationAreas.Add(myPanel.Areas[area.Key].GetAreaNumber, newArea);
                    }
                }
            }

            foreach (var userDevice in this.notificationDevices)
            {
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

        public ushort PropertyToggle(string userDevice, int area, string property)
        {
            ushort value = 0;
            try
            {
                PropertyInfo propertyInfo = notificationDevices[userDevice].NotificationAreas[area].GetType().GetCType().GetProperty(property);
                value = (ushort)propertyInfo.GetValue(notificationDevices[userDevice].NotificationAreas[area], null);

                if (value == 0) propertyInfo.SetValue(notificationDevices[userDevice].NotificationAreas[area], (ushort)1, null);
                if (value == 1) propertyInfo.SetValue(notificationDevices[userDevice].NotificationAreas[area], (ushort)0, null);

                value = (ushort)propertyInfo.GetValue(notificationDevices[userDevice].NotificationAreas[area], null);
                myPanel.SendDebug(String.Format("Notification Manager Toggling Property: {0} to {1}", property, value));
                string json = JsonConvert.SerializeObject(notificationDevices[userDevice].NotificationAreas[area]);
                CrestronConsole.PrintLine(json);
            }
            catch (Exception ex)
            {
                myPanel.SendDebug(String.Format("Notification Manager - Error Toggling Property {0}", ex.ToString()));
            }
            return value;
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
        public NotificationArea()
        {
        }

        public void Initialize(int _number, string _name)
        {
            AreaNumber = _number;
            AreaName = _name;
        }

        public int AreaNumber;
        public string AreaName;

        //Global Notifications Per Device
        public uint AreaNotifications { get; set; }

        public uint ArmedStatus { get; set; }

        public uint ArmUpStats { get; set; }

        public uint ZoneViolated { get; set; }

        public uint ZoneBypassed { get; set; }

        public uint ZoneSoftBypassed { get; set; }

        //Armed Status
        public ushort Disarmed { get; set; }

        public ushort ArmedAway { get; set; }

        public ushort ArmedStay { get; set; }

        public ushort ArmedStayInstant { get; set; }

        public ushort ArmedToNight { get; set; }

        public ushort ArmedToNightInstant { get; set; }

        public ushort ArmedVacation { get; set; }

        //Alarm Status
        public ushort EntranceDelayActive { get; set; }

        public ushort AlarmAbortDelayActive { get; set; }

        public ushort FireAlarm { get; set; }

        public ushort MedicalAlarm { get; set; }

        public ushort PoliceAlarm { get; set; }

        public ushort BurglarAlarm { get; set; }

        public ushort Aux1Alarm { get; set; }

        public ushort Aux2Alarm { get; set; }

        public ushort CarbonMonoxideAlarm { get; set; }

        public ushort EmergencyAlarm { get; set; }

        public ushort FreezeAlarm { get; set; }

        public ushort GasAlarm { get; set; }

        public ushort HeatAlarm { get; set; }

        public ushort WaterAlarm { get; set; }

        public ushort FireSupervisory { get; set; }

        public ushort VerifyFire;
    }

    public class NotificationZone
    {
        public NotificationZone(int _zone, string _name)
        {
        }
    }
}