// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.01.2025
// =========================================

using AldaEngine;
using ServiceEngine;

namespace Pharaoh
{
	public class CSaviorNotifications
	{
		private readonly INotifications<CUnityNotification> _notifications;
		private readonly CNotificationFactory _notificationFactory;

		public CSaviorNotifications(
			INotifications<CUnityNotification> notifications, 
			CNotificationFactory notificationFactory
			)
		{
			_notificationFactory = notificationFactory;
			_notifications = notifications;
		}

		public void Set()
		{
			SetSaviorNotification22Hours();
			SetSaviorNotification2Days();
			SetSaviorNotification6Days();
			SetSaviorNotification10Days();
		}
		
		private void SetSaviorNotification22Hours()
		{
			long triggerTime = CTimeConst.Hour.InSeconds * 22;
			CUnityNotification notification = _notificationFactory.GetNotification(
				ENotificationId.Saviour22Hours, 
				triggerTime,
				"Notification.Savior.22Hours.Title",
				"Notification.Savior.22Hours.Content");
			_notifications.SetNotification(notification);
		}
		
		private void SetSaviorNotification2Days()
		{
			long triggerTime = CTimeConst.Day.InSeconds * 2;
			CUnityNotification notification = _notificationFactory.GetNotification(
				ENotificationId.Saviour2Days, 
				triggerTime,
				"Notification.Savior.2Days.Title",
				"Notification.Savior.2Days.Content");
			_notifications.SetNotification(notification);
		}
		
		private void SetSaviorNotification6Days()
		{
			long triggerTime = CTimeConst.Day.InSeconds * 6;
			CUnityNotification notification = _notificationFactory.GetNotification(
				ENotificationId.Saviour6Days, 
				triggerTime,
				"Notification.Savior.6Days.Title",
				"Notification.Savior.6Days.Content");
			_notifications.SetNotification(notification);
		}
		
		private void SetSaviorNotification10Days()
		{
			long triggerTime = CTimeConst.Day.InSeconds * 10;
			CUnityNotification notification = _notificationFactory.GetNotification(
				ENotificationId.Saviour10Days, 
				triggerTime,
				"Notification.Savior.10Days.Title",
				"Notification.Savior.10Days.Content");
			_notifications.SetNotification(notification);
		}
	}
}