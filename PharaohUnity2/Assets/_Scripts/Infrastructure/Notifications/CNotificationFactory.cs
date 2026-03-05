// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.01.2025
// =========================================

using AldaEngine;
using ServiceEngine;

namespace TycoonBuilder
{
	public class CNotificationFactory
	{
		private readonly ITranslation _translation;

		public CNotificationFactory(ITranslation translation)
		{
			_translation = translation;
		}

		public CUnityNotification GetNotification(
			ENotificationId notificationId, 
			long triggerInSeconds,
			string titleLangKey,
			string bodyLangKey
			)
		{
			string title = _translation.GetText(titleLangKey);
			string body = _translation.GetText(bodyLangKey);
			
			CUnityNotificationBuilder builder = new((int)notificationId, triggerInSeconds);
			builder.SetLargeIconName("icon_large")
				.SetSmallIconName("icon_small")
				.SetBody(body)
				.SetTitle(title);
			return builder.Build();
		}
		
		public CUnityNotificationBuilder GetStandardBuilder(int notificationId, long triggerInSeconds)
		{
			CUnityNotificationBuilder builder = new(notificationId, triggerInSeconds);
			builder.SetLargeIconName("icon_large")
				.SetSmallIconName("icon_small");
			return builder;
		}
	}
}