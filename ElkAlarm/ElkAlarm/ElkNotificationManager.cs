using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using CrestronPushover;
using ElkAlarm.Annotations;
using Newtonsoft.Json;

namespace ElkAlarm
{
    public class ElkNotificationManager
    {
        private ElkPanel myPanel;
        private ElkNotificationMessageHandler myMessageHandler;
        private bool panelInitialized;
        private bool pushoverInitialized;
        public bool managerReady;

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
            configFileName = String.Format("\\NVRAM\\ElkNotificationCfg-PanelID-{0}.json", myPanel.getPanelId);
            myMessageHandler = new ElkNotificationMessageHandler(myPanel, this);
        }

        public void LoadNotificationConfig()
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
                    BuildNotificationConfig();
                }
                else
                {
                    configExists = false;
                    myPanel.SendDebug("Notification Config Does not exist, building new config");
                    BuildNotificationConfig();
                }
            }
            catch (Exception ex)
            {
                myPanel.SendDebug(String.Format("Exception Reading Notification Config: {0}", ex.ToString()));
            }
        }

        public void SaveNotificationConfig()
        {
            if (string.IsNullOrEmpty(configFileName)) return;
            try
            {
                string serializedConfig = string.Empty;
                if (notificationDevices != null)
                {
                    serializedConfig = JsonConvert.SerializeObject(notificationDevices, Formatting.Indented);
                }

                _writer = new StreamWriter(configFileName, false);
                _writer.Write(serializedConfig);
                _writer.Dispose();
                myPanel.SendDebug("Writing Notification Config to Disk");
            }
            catch (Exception ex)
            {
                myPanel.SendDebug(String.Format("Exception Writing Notification Config: {0}", ex.ToString()));
            }
        }

        public void BuildNotificationConfig()
        {
            try
            {
                if (configExists)
                {
                    myPanel.SendDebug(
                        "*****Appending Notification Config. Config Was Already Loaded From Disk Comparing to devices from Pushover.net*****");
                }
                else
                {
                    myPanel.SendDebug("*****No Config was found Building Notification Config*****");
                }

                //Add the devices if they don't exist in the dictionary (first run or loaded from config file)
                foreach (var pushoverDevice in PushoverManager.Instance.UserDevices)
                {
                    if (!notificationDevices.ContainsKey(pushoverDevice))
                    {
                        myPanel.SendDebug(String.Format("*****Config Adding Device from Pushover {0}*****", pushoverDevice));
                        notificationDevices.Add(pushoverDevice, new NotificationDevice(pushoverDevice));
                    }
                }

                //Build list of devices that no longer exist in Pushover
                List<string> devicesToRemove = new List<string>();
                foreach (var deviceToRemove in notificationDevices)
                {
                    if (!PushoverManager.Instance.UserDevices.Contains(notificationDevices[deviceToRemove.Key].DeviceName))
                    {
                        string deviceToRemoveString = notificationDevices[deviceToRemove.Key].DeviceName;

                        if (notificationDevices.ContainsKey(deviceToRemoveString))
                        {
                            devicesToRemove.Add(deviceToRemoveString);
                        }
                    }
                }
                //Remove the devices from the collection that no longer exist
                foreach (var item in devicesToRemove)
                {
                    myPanel.SendDebug(String.Format("*****Removing device {0} from config. Device was not found in Pushover devices.*****", item));
                    notificationDevices.Remove(item);
                }

                foreach (var device in notificationDevices)
                {
                    foreach (var area in myPanel.Areas)
                    {
                        //Don't write generic area names
                        if (!myPanel.Areas[area.Key].GetAreaName.Contains("Area"))
                        {
                            //add the area to the user device if it doesnt exist
                            int currentArea = myPanel.Areas[area.Key].GetAreaNumber;
                            if (

                                !notificationDevices[device.Key].NotificationAreas.ContainsKey(
                                    currentArea))
                            {
                                NotificationArea newArea = new NotificationArea();
                                newArea.Initialize(currentArea,
                                    myPanel.Areas[area.Key].GetAreaName);
                                notificationDevices[device.Key].NotificationAreas.Add(
                                    currentArea, newArea);
                            }
                        }
                    }

                    foreach (var zone in myPanel.Zones)
                    {
                        int currentZone = myPanel.Zones[zone.Key].GetZoneNumber;
                        string currentZoneName = myPanel.Zones[zone.Key].GetZoneName;
                        if (
                            notificationDevices[device.Key].NotificationZones[currentZone] == null)
                        {
                            NotificationZone newZone = new NotificationZone();
                            newZone.Initialize(currentZone, currentZoneName);
                            notificationDevices[device.Key].NotificationZones[currentZone] = newZone;
                        }
                    }
                }
                managerReady = true;

                myPanel.SendDebug("*****Serializing Notification Config*****");
                string json = JsonConvert.SerializeObject(notificationDevices);
                this.SaveNotificationConfig();
            }
            catch (Exception ex)
            {
                managerReady = false;
                myPanel.SendDebug("*****Error On Loading Notification Config*****");
            }
        }

        private void OnPushoverInitializedChange(bool status)
        {
            myPanel.SendDebug("*****Pushover Init Event*****");
            pushoverInitialized = status;
            if (pushoverInitialized && panelInitialized) LoadNotificationConfig();
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

        public Dictionary<int, NotificationArea> NotificationAreas = new Dictionary<int, NotificationArea>();

        //public List<NotificationZone> NotificationZones = new List<NotificationZone>();
        public NotificationZone[] NotificationZones = new NotificationZone[209];
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
        public ushort AreaNotifications { get; set; }

        public ushort ArmedStateChange { get; set; }

        public ushort AlarmStateChange { get; set; }

        public ushort ZoneChangeWhenArmed { get; set; }

        public ushort ZoneChangeWhenDisarmed { get; set; }
    }

    public class NotificationZone
    {
        public NotificationZone()
        {
        }

        public void Initialize(int _zone, string _name)
        {
            ZoneNumber = _zone;
            ZoneName = _name;
        }

        public int ZoneNumber;
        public string ZoneName;

        public ushort ArmedNotifications { get; set; }

        public ushort DisarmedNotifications { get; set; }
    }
}