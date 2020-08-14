using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using CrestronPushover;

namespace ElkAlarm
{
    public class ElkNotificationMessageHandler
    {
        private ElkNotificationManager myElkNotificationManager;

        private ElkPanel myPanel;

        private Dictionary<string, NotificationDevice> myNotificationDevices =
            new Dictionary<string, NotificationDevice>();

        public NotificationZone[] myNotificationZoneArray = new NotificationZone[209];

        public ElkNotificationMessageHandler(ElkPanel _panel, ElkNotificationManager _manager)
        {
            myElkNotificationManager = _manager;
            myPanel = _panel;
            Initialize();
        }

        private void Initialize()
        {
            if (myPanel != null && myElkNotificationManager != null)
            {
                foreach (var area in myPanel.Areas)
                {
                    myPanel.Areas[area.Key].ElkAreaEvent += ElkNotificationMessageHandler_ElkAreaEvent;
                }

                foreach (var zone in myPanel.Zones)
                {
                    myPanel.Zones[zone.Key].ElkZoneEvent += ElkNotificationMessageHandler_ElkZoneEvent;
                }
            }
        }

        private void ElkNotificationMessageHandler_ElkAreaEvent(object sender, ElkAreaEventArgs e)
        {
            if (!myElkNotificationManager.managerReady) return;
            ElkArea currentArea = myPanel.GetAreaObject(e.Area);

            switch (e.EventUpdateType)
            {
                case eElkAreaEventUpdateType.ArmedStatusChange:
                    eAreaArmedStatus status = currentArea.GetAreaArmedStatus;
                    string areaName = currentArea.GetAreaName.TrimEnd();
                    string devicesToSend = string.Join(",", CheckAreaNotificationProperty(currentArea, e.EventUpdateType));
                    PushoverManager.Instance.SendMessage(devicesToSend, String.Format("{0} - {1}", areaName, status), String.Format("Area {0}", status));
                    myPanel.SendDebug(String.Format("NotificationMessageHandler: Building Message *{0} - {1} to Devices {2}", areaName, status, devicesToSend));

                    break;
            }
        }

        private void ElkNotificationMessageHandler_ElkZoneEvent(object sender, ElkZoneEventArgs e)
        {
            if (!myElkNotificationManager.managerReady) return;
            if (e.EventUpdateType == eElkZoneEventUpdateType.StatusChange)
            {
                ElkZone currentZone = myPanel.GetZoneObject(e.Zone);
                ElkArea currentArea = myPanel.GetAreaObject(currentZone.GetZoneAreaAssignment);
                eAreaArmedStatus areaArmedStatus =
                    currentArea.GetAreaArmedStatus;
                string areaName = currentArea.GetAreaName.TrimEnd();
                string zoneName = currentZone.GetZoneName.TrimEnd();
                eZoneStatus zoneStatus = currentZone.GetZoneStatus;
                string devicesToSend = string.Join(",", CheckZoneNotificationProperty(e.Zone, areaArmedStatus));
                myPanel.SendDebug(String.Format("NotificationMessageHandler: Building Message *{0} - {1}:{2} to Devices {3}", areaName, zoneName, zoneStatus, devicesToSend));
                PushoverManager.Instance.SendMessage(devicesToSend, String.Format("{0} - {1}", areaName, zoneName), String.Format("{0}", zoneStatus));
            }
        }

        private string[] CheckAreaNotificationProperty(ElkArea area, eElkAreaEventUpdateType update)
        {
            myNotificationDevices = myElkNotificationManager.notificationDevices;
            List<string> devicesToSend = new List<string>();
            foreach (var userDevice in myNotificationDevices)
            {
                if (userDevice.Value.NotificationAreas.ContainsKey(area.GetAreaNumber))
                {
                    try
                    {
                        if (update == eElkAreaEventUpdateType.ArmedStatusChange && userDevice.Value.NotificationAreas[area.GetAreaNumber].ArmedStateChange == 1)
                        {
                            myPanel.SendDebug(String.Format("NotificationMessageHandler: OK To send message for {0} {1}", userDevice.Value.DeviceName, area));
                            devicesToSend.Add(userDevice.Value.DeviceName);
                        }
                    }
                    catch (Exception ex)
                    {
                        myPanel.SendDebug(String.Format("NotificationMessageHandler: Error checking area property {0} {1} {2} \r\n{3}", userDevice, update, ex.ToString()));
                    }
                }
            }
            return devicesToSend.ToArray();
        }

        private string[] CheckZoneNotificationProperty(int zone, eAreaArmedStatus armedStatus)
        {
            myNotificationDevices = myElkNotificationManager.notificationDevices;
            List<string> devicesToSend = new List<string>();
            foreach (var userDevice in myNotificationDevices)
            {
                try
                {
                    if (userDevice.Value.NotificationZones[zone].DisarmedNotifications == 1 &&
                        armedStatus == eAreaArmedStatus.Disarmed)
                    {
                        devicesToSend.Add(userDevice.Value.DeviceName);
                    }

                    if (userDevice.Value.NotificationZones[zone].DisarmedNotifications == 1 &&
                        armedStatus > eAreaArmedStatus.Disarmed)
                    {
                        devicesToSend.Add(userDevice.Value.DeviceName);
                    }
                }
                catch (Exception ex)
                {
                    myPanel.SendDebug(String.Format("NotificationMessageHandler: Error checking zone property {0} {1} {2} \r\n{3}", userDevice, zone, ex.ToString()));
                }
            }
            return devicesToSend.ToArray();
        }
    }
}