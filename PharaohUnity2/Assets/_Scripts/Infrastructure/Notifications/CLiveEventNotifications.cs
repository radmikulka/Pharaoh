// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.01.2025
// =========================================

using AldaEngine;
using ServerData;
using ServiceEngine;

namespace TycoonBuilder
{
	public class CLiveEventNotifications
	{
		private readonly INotifications<CUnityNotification> _notifications;
		private readonly CNotificationFactory _notificationFactory;
		private readonly ITranslation _translation;
		private readonly IServerTime _serverTime;
		private readonly CUser _user;

		public CLiveEventNotifications(
			INotifications<CUnityNotification> notifications,
			CNotificationFactory factory, 
			ITranslation translation, 
			IServerTime serverTime, 
			CUser user
			)
		{
			_notifications = notifications;
			_notificationFactory = factory;
			_translation = translation;
			_serverTime = serverTime;
			_user = user;
		}

		public void Set()
		{
			TryEnqueueEventEndedNotification();
		}

		private void TryEnqueueEventEndedNotification()
		{
			foreach (ELiveEvent liveEvent in _user.LiveEvents.GetActiveEvents())
			{
				ILiveEvent eventData = _user.LiveEvents.GetActiveEvent(liveEvent);
				long triggerTime = GetTriggerInSecs(eventData.EndTimeInMs);
				if(triggerTime <= 0)
					continue;
				
				string eventName = _translation.GetText($"LiveEvent.{(int)liveEvent}.Title");
				string title = _translation.GetText("Notification.EventEnd.Title", eventName);
				string body = _translation.GetText("Notification.EventEnd.Content");
				CUnityNotificationBuilder builder = _notificationFactory.GetStandardBuilder(
					(int)ENotificationId.EventEnded + (int)liveEvent,
					triggerTime
					)
					.SetTitle(title)
					.SetBody(body)
					;

				_notifications.SetNotification(builder.Build());
			}
		}

		private long GetTriggerInSecs(long timeInMs)
		{
			long timeDiffInMs = timeInMs - _serverTime.GetTimestampInMs();
			return timeDiffInMs / CTimeConst.Second.InMilliseconds;
		}
	}
}