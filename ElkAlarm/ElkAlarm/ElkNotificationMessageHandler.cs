using System;
using System.Collections.Generic;
using Crestron.SimplSharp;

using CrestronPushover;

namespace ElkAlarm
{
	public class ElkNotificationMessageHandler
	{
		private ElkNotificationManager myElkNotificationManager;

		internal ElkPanel myPanel;

		private Dictionary<string, NotificationDevice> myNotificationDevices =
			new Dictionary<string, NotificationDevice>();

		private Dictionary<int, LastZoneStatus> lastZoneStatuses = new Dictionary<int, LastZoneStatus>();

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
			string areaName = currentArea.GetAreaName.TrimEnd();
			switch (e.EventUpdateType)
			{
				case eElkAreaEventUpdateType.ArmedStatusChange:
					eAreaArmedStatus status = currentArea.GetAreaArmedStatus;

					string devicesToSendArmed = string.Join(",", CheckAreaNotificationProperty(currentArea, e.EventUpdateType));
					if (devicesToSendArmed.Length > 0 && !String.IsNullOrEmpty(devicesToSendArmed))
					{
						PushoverManager.Instance.SendMessage(devicesToSendArmed, String.Format("{0} - {1}", areaName, status),
							String.Format("Area {0}", status));
						myPanel.SendDebug(
							String.Format("NotificationMessageHandler: Building Message *{0} - {1} to Devices {2}",
								areaName, status, devicesToSendArmed));
					}
					break;
				case eElkAreaEventUpdateType.AlarmStateChange:
					eAreaAlarmState alarmStatus = currentArea.GetAlarmStatus;
					string devicesToSendAlarm = string.Join(",", CheckAreaNotificationProperty(currentArea, e.EventUpdateType));
					if (devicesToSendAlarm.Length > 0 && !String.IsNullOrEmpty(devicesToSendAlarm))
					{
						PushoverManager.Instance.SendMessage(devicesToSendAlarm, String.Format("{0} - {1}", areaName, alarmStatus),
							String.Format("Area {0}", alarmStatus));
						myPanel.SendDebug(
							String.Format("NotificationMessageHandler: Building Message *{0} - {1} to Devices {2}",
								areaName, alarmStatus, devicesToSendAlarm));
					}
					break;
			}
		}

		private void ElkNotificationMessageHandler_ElkZoneEvent(object sender, ElkZoneEventArgs e)
		{
			if (!myElkNotificationManager.managerReady) return;
			ProcessZoneEvent(e);
		}

		private void ProcessZoneEvent(ElkZoneEventArgs e)
		{
			if (e.EventUpdateType == eElkZoneEventUpdateType.StatusChange)
			{
				ElkZone currentZone = myPanel.GetZoneObject(e.Zone);

				int zoneNumber = currentZone.GetZoneNumber;
				eZoneStatus zoneStatus = currentZone.GetZoneStatus;

				if (!lastZoneStatuses.ContainsKey(zoneNumber))
				{
					lastZoneStatuses.Add(zoneNumber, new LastZoneStatus(myPanel, this, currentZone, zoneStatus));
				}
				else
				{
					if (!lastZoneStatuses[zoneNumber].TimerRuning)
					{
						//lastZoneStatuses[zoneNumber].Zone = currentZone;
						lastZoneStatuses[zoneNumber].CheckMessage();
					}
				}
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
							myPanel.SendDebug(String.Format("NotificationMessageHandler: OK To send Armed State message for {0} {1}", userDevice.Value.DeviceName, area));
							devicesToSend.Add(userDevice.Value.DeviceName);
						}

						if (update == eElkAreaEventUpdateType.AlarmStateChange && userDevice.Value.NotificationAreas[area.GetAreaNumber].AlarmStateChange == 1)
						{
							myPanel.SendDebug(String.Format("NotificationMessageHandler: OK To send Alarm Status message for {0} {1}", userDevice.Value.DeviceName, area));
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

		internal string[] CheckZoneNotificationProperty(int zone, eAreaArmedStatus armedStatus)
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
						myPanel.SendDebug(String.Format("NotificationMessageHandler: Zone Check Disarmed {0} - {1}", zone, userDevice.Value.DeviceName));
					}

					if (userDevice.Value.NotificationZones[zone].ArmedNotifications == 1 &&
						armedStatus > eAreaArmedStatus.Disarmed)
					{
						devicesToSend.Add(userDevice.Value.DeviceName);
						myPanel.SendDebug(String.Format("NotificationMessageHandler: Zone Check Armed {0} - {1}", zone, userDevice.Value.DeviceName));
					}
				}
				catch (Exception ex)
				{
					myPanel.SendDebug(String.Format("NotificationMessageHandler: Error checking zone property {0} {1} {2} \r\n{3}", userDevice, zone, ex.ToString()));
				}
			}
			myPanel.SendDebug(String.Format("NotificationMessageHandler: Zone Devices to send count {0}", devicesToSend.Count));
			return devicesToSend.ToArray();
		}
	}

	internal class LastZoneStatus
	{
		private const int _interval = 600000;

		private CTimer TimeToNextMessage;
		private bool _timerRunning = false;
		private ElkPanel myPanel;
		private eZoneStatus lastStatus;
		public ElkZone Zone;

		private ElkArea currentArea;
		private string areaName;
		private string zoneName;
		private int zoneNumber;
		private ElkNotificationMessageHandler handler;

		public bool TimerRuning { get { return _timerRunning; } }

		public LastZoneStatus(ElkPanel _panel, ElkNotificationMessageHandler _handler, ElkZone _zone, eZoneStatus _zoneStatus)
		{
			TimeToNextMessage = new CTimer(OnNextMessage, Timeout.Infinite);
			myPanel = _panel;
			Zone = _zone;
			handler = _handler;
			lastStatus = eZoneStatus.Uninitialized;

			currentArea = myPanel.GetAreaObject(Zone.GetZoneAreaAssignment);
			areaName = currentArea.GetAreaName.TrimEnd();
			zoneName = Zone.GetZoneName.TrimEnd();
			zoneNumber = Zone.GetZoneNumber;

			CheckMessage();
		}

		public void CheckMessage()
		{
			if (Zone != null)
			{
				eZoneStatus currentStatus = myPanel.GetZoneObject(Zone.GetZoneNumber).GetZoneStatus;
				if (lastStatus != currentStatus)
				{
					CrestronConsole.PrintLine("Firing Next Update for Zone {0} status changed to {1}", Zone.GetZoneName, Zone.GetZoneStatus);

					eAreaArmedStatus areaArmedStatus =
					currentArea.GetAreaArmedStatus;

					string devicesToSend = string.Join(",", handler.CheckZoneNotificationProperty(zoneNumber, areaArmedStatus));
					myPanel.SendDebug(String.Format("Zone Devices to send count {0} - {1}", devicesToSend.Length, devicesToSend));
					if (devicesToSend.Length > 0 && !String.IsNullOrEmpty(devicesToSend))
					{
						myPanel.SendDebug(
							String.Format("NotificationMessageHandler: Building Message *{0} - {1}:{2} to Devices {3}",
								areaName, zoneName, currentStatus, devicesToSend));
						PushoverManager.Instance.SendMessage(devicesToSend, String.Format("{0} - {1}", areaName, zoneName), String.Format("{0}", currentStatus));
					}

					lastStatus = currentStatus;
					RestartTimeout();
				}
			}
		}

		public void RestartTimeout()
		{
			if (!_timerRunning)
			{
				TimeToNextMessage.Reset(_interval);

				_timerRunning = true;
			}
		}

		private void OnNextMessage(object userspecific)
		{
			try
			{
				CheckMessage();
				_timerRunning = false;
			}
			catch (Exception ex)
			{
				myPanel.SendDebug(String.Format("Error in OnNextMessage for Zone {0}: {1}", Zone.GetZoneName, ex.Message));
			}
		}
	}
}