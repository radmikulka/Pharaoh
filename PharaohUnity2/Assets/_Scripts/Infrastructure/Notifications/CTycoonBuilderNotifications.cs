// =========================================
// AUTHOR: Radek Mikulka
// DATE:   1.1.2024
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServiceEngine;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CTycoonBuilderNotifications : MonoBehaviour, IInitializable
	{
		private INotifications<CUnityNotification> _notifications;
		private CSaviourNotifications _saviourNotifications;
		private CNotificationFactory _notificationFactory;

		[Inject]
		private void Inject(
			INotifications<CUnityNotification> notifications,
			CNotificationFactory notificationFactory,
			ITranslation translation,
			IServerTime serverTime,
			IEventBus eventBus,
			CUser user
			)
		{
			_saviourNotifications = new CSaviourNotifications(notifications, notificationFactory);
			_notificationFactory = notificationFactory;
			_notifications = notifications;
		}

		public void Initialize()
		{
			SetTestNotification();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if(hasFocus)
				return;

			_saviourNotifications.Set();
		}

		private void SetTestNotification()
		{
			if(!CPlatform.IsDebug)
				return;
			
			CUnityNotification notification = _notificationFactory.GetNotification(
				ENotificationId.Test, 
				30,
				"Notification.Savior.22Hours.Title",
				"Notification.Savior.22Hours.Content");
			_notifications.SetNotification(notification);
		}
	}
}