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

        public void PropertyToggle(string userDevice, int area, string property)
        {
            try
            {
                PropertyInfo propertyInfo = notificationDevices[userDevice].NotificationAreas[area].GetType().GetCType().GetProperty(property);
                uint value = (uint)propertyInfo.GetValue(notificationDevices[userDevice].NotificationAreas[area], null);

                if (value == 0) propertyInfo.SetValue(notificationDevices[userDevice].NotificationAreas[area], (uint)1, null);
                if (value == 1) propertyInfo.SetValue(notificationDevices[userDevice].NotificationAreas[area], (uint)0, null);

                value = (uint)propertyInfo.GetValue(notificationDevices[userDevice].NotificationAreas[area], null);
                myPanel.SendDebug(String.Format("Notification Manager Toggling Property: {0} to {1}", property, Convert.ToBoolean(value)));
            }
            catch (Exception ex)
            {
                myPanel.SendDebug("Notification Manager - Error Toggling Property");
            }
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
        public bool AreaNotifications { get; set; }

        public bool ArmedStatus { get; set; }

        public bool ArmUpStats { get; set; }

        public bool ZoneViolated { get; set; }

        public bool ZoneBypassed { get; set; }

        public bool ZoneSoftBypassed { get; set; }

        //Armed Status
        public uint Disarmed { get; set; }

        public uint ArmedAway { get; set; }

        public uint ArmedStay { get; set; }

        public uint ArmedStayInstant { get; set; }

        public uint ArmedToNight { get; set; }

        public uint ArmedToNightInstant { get; set; }

        public uint ArmedVacation { get; set; }

        //Alarm Status
        public uint EntranceDelayActive { get; set; }

        public uint AlarmAbortDelayActive { get; set; }

        public uint FireAlarm { get; set; }

        public uint MedicalAlarm { get; set; }

        public uint PoliceAlarm { get; set; }

        public uint BurglarAlarm { get; set; }

        public uint Aux1Alarm { get; set; }

        public uint Aux2Alarm { get; set; }

        public uint CarbonMonoxideAlarm { get; set; }

        public uint EmergencyAlarm { get; set; }

        public uint FreezeAlarm { get; set; }

        public uint GasAlarm { get; set; }

        public uint HeatAlarm { get; set; }

        public uint WaterAlarm { get; set; }

        public uint FireSupervisory { get; set; }

        public uint VerifyFire;
    }

    public class NotificationZone
    {
        public NotificationZone(int _zone, string _name)
        {
        }
    }
}