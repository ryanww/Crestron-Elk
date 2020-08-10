using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
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
        private bool configExists;
        public string configFileName;

        internal Dictionary<string, NotificationDevice> notificationDevices = new Dictionary<string, NotificationDevice>();

        private FileStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;

        public ElkNotificationManager(ElkPanel _panel)
        {
            myPanel = _panel;
            myPanel.OnElkPanelInitializedChanged += myPanel_OnElkPanelInitializedChanged;
            PushoverManager.Instance.OnPushoverInitializedChange += OnPushoverInitializedChange;
            PushoverManager.Instance.PushoverUpdateEvent += Instance_PushoverUpdateEvent;
            configFileName = "\\NVRAM\\ElkNotificationCfg-PanelID-{0}.json";
        }

        private void LoadNotificationConfig(string path)
        {
            if (string.IsNullOrEmpty(configFileName)) return;
            try
            {
                FileInfo info = new FileInfo(configFileName);
                if (info.Exists)
                {
                    _stream = new FileStream(configFileName, FileMode.Open);
                    _reader = new StreamReader(_stream);

                    string text = _reader.ReadToEnd();
                    _reader.Dispose();
                    _stream.Dispose();

                    notificationDevices = JsonConvert.DeserializeObject<Dictionary<string, NotificationDevice>>(text);
                    configExists = true;
                }
                else
                {
                    BuildNotificationConfig();
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Exception Reading Config: {0}", e);
            }
        }

        public void SaveNotificationConfig()
        {
            if (string.IsNullOrEmpty(configFileName)) return;
            try
            {
                if (notificationDevices != null)
                {
                    string serializedConfig = JsonConvert.SerializeObject(notificationDevices);
                }

                _writer = new StreamWriter(configFileName, false);
                _writer.Write(notificationDevices);
                _writer.Dispose();
                _stream.Dispose();
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Exception Writing Config: {0}", e);
            }
        }

        private void BuildNotificationConfig()
        {
            try
            {
                if (configExists) myPanel.SendDebug("*****Appending Notification Config. Config Was Already Loaded From Disk*****");
                myPanel.SendDebug("*****Building Notification Config*****");

                //Add the devices if they don't exist in the dictionary (first run or loaded from config file)
                foreach (var device in PushoverManager.Instance.UserDevices)
                {
                    if (!notificationDevices.ContainsKey(device)) notificationDevices.Add(device, new NotificationDevice(device));
                }

                foreach (var device in notificationDevices)
                {
                    foreach (var area in myPanel.Areas)
                    {
                        //Don't write generic area names
                        if (!myPanel.Areas[area.Key].GetAreaName.Contains("Area"))
                        {
                            //add the area to the user device if it doesnt exist
                            if (
                                !notificationDevices[device.Key].NotificationAreas.ContainsKey(
                                    myPanel.Areas[area.Key].GetAreaNumber))
                            {
                                NotificationArea newArea = new NotificationArea();
                                newArea.Initialize(myPanel.Areas[area.Key].GetAreaNumber,
                                    myPanel.Areas[area.Key].GetAreaName);
                                notificationDevices[device.Key].NotificationAreas.Add(
                                    myPanel.Areas[area.Key].GetAreaNumber, newArea);
                            }
                        }
                    }
                }

                myPanel.SendDebug("*****Serializing Notification Config*****");
                string json = JsonConvert.SerializeObject(notificationDevices);
                //this.SaveNotificationConfig();

                CrestronConsole.PrintLine(json);
            }
            catch (Exception ex)
            {
                myPanel.SendDebug("*****Error On Loading Notification Config*****");
            }
        }

        private void OnPushoverInitializedChange(bool status)
        {
            myPanel.SendDebug("*****Pushover Init Event*****");
            pushoverInitialized = status;
            if (pushoverInitialized && panelInitialized) BuildNotificationConfig();
        }

        private void Instance_PushoverUpdateEvent(object sender, PushoverUpdateEventArgs e)
        {
        }

        private void myPanel_OnElkPanelInitializedChanged(bool status)
        {
            myPanel.SendDebug("*****Pushover Panel Init Event*****");
            panelInitialized = status;
            if (pushoverInitialized && panelInitialized) BuildNotificationConfig();
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

        public ushort VerifyFire { get; set; }
    }

    public class NotificationZone
    {
        public NotificationZone(int _zone, string _name)
        {
        }
    }
}