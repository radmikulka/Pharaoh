// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.01.2025
// =========================================

using AldaEngine;
using ServiceEngine;

namespace Pharaoh
{
	public class CSaviourNotifications
	{
		private readonly INotifications<CUnityNotification> _notifications;
		private readonly CNotificationFactory _notificationFactory;

		public CSaviourNotifications(
			INotifications<CUnityNotification> notifications, 
			CNotificationFactory notificationFactory
			)
		{
			_notificationFactory = notificationFactory;
			_notifications = notifications;
		}

		public void Set()
		{
			SetSaviourNotification22Hours();
			SetSaviourNotification2Days();
			SetSaviourNotification6Days();
			SetSaviourNotification10Days();
		}
		
		private void SetSaviourNotification22Hours()
		{
			long triggerTime = CTimeConst.Hour.InSeconds * 22;
			CUnityNotification notification = _notificationFactory.GetNotification(
				ENotificationId.Saviour22Hours, 
				triggerTime,
				"Notification.Savior.22Hours.Title",
				"Notification.Savior.22Hours.Content");
			_notifications.SetNotification(notification);
		}
		
		private void SetSaviourNotification2Days()
		{
			long triggerTime = CTimeConst.Day.InSeconds * 2;
			CUnityNotification notification = _notificationFactory.GetNotification(
				ENotificationId.Saviour2Days, 
				triggerTime,
				"Notification.Savior.2Days.Title",
				"Notification.Savior.2Days.Content");
			_notifications.SetNotification(notification);
		}
		
		private void SetSaviourNotification6Days()
		{
			long triggerTime = CTimeConst.Day.InSeconds * 6;
			CUnityNotification notification = _notificationFactory.GetNotification(
				ENotificationId.Saviour6Days, 
				triggerTime,
				"Notification.Savior.6Days.Title",
				"Notification.Savior.6Days.Content");
			_notifications.SetNotification(notification);
		}
		
		private void SetSaviourNotification10Days()
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