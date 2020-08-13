using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

namespace ElkAlarm
{
    public class ElkNotificationMessageHandler
    {
        private ElkNotificationManager myElkNotificationManager;
        private ElkPanel myPanel;

        private Dictionary<string, NotificationDevice> myNotificationDevices =
            new Dictionary<string, NotificationDevice>();

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
            }
        }

        private void ElkNotificationMessageHandler_ElkAreaEvent(object sender, ElkAreaEventArgs e)
        {
            myPanel.SendDebug(String.Format("NotificationMessageHandler: Got Area Event - {0} - {1}", e.Area, e.EventUpdateType));

            ElkArea currentArea = myPanel.GetAreaObject(e.Area);

            switch (e.EventUpdateType)
            {
                case eElkAreaEventUpdateType.ArmedStatusChange:
                    eAreaArmedStatus status = currentArea.GetAreaArmedStatus;
                    CheckNotificationProperty(e.Area, status);
                    break;
            }
        }

        private void CheckNotificationProperty(int area, eAreaArmedStatus status)
        {
            myNotificationDevices = myElkNotificationManager.notificationDevices;

            foreach (var userDevice in myNotificationDevices)
            {
                if (userDevice.Value.NotificationAreas.ContainsKey(area))
                {
                    try
                    {
                        PropertyInfo propertyInfo =
                            myNotificationDevices[userDevice.Key].NotificationAreas[area].GetType().GetCType().GetProperty(status.ToString());
                        ushort value = (ushort)propertyInfo.GetValue(myNotificationDevices[userDevice.Key].NotificationAreas[area], null);
                        bool isEnabled = Convert.ToBoolean(value);
                        if (isEnabled) myPanel.SendDebug(String.Format("NotificationMessageHandler: OK To send message for {0} {1} {2}", userDevice, area, status));
                    }
                    catch (Exception ex)
                    {
                        myPanel.SendDebug(String.Format("NotificationMessageHandler: Error getting property {0} {1} {2} \r\n{3}", userDevice, area, status, ex.ToString()));
                    }
                }
            }
        }
    }
}