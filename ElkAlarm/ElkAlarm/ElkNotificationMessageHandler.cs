using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkNotificationMessageHandler
    {
        private ElkNotificationManager myElkNotificationManager;
        private ElkPanel myPanel;

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
        }
    }
}