// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData.Hits;
using ServiceEngine;
using ServiceEngine.Ads;
using ServiceEngine.ServiceMaster;
using UnityEngine;

namespace Pharaoh
{
	public class CConnectionEventsHandler
	{
		private readonly CInitialUserDataProvider _initialUserDataProvider;
		private readonly CCrashlyticsKeys _crashlyticsKeys;
		private readonly CServiceMaster _serviceMaster;
		private readonly ICrashlytics _crashlytics;
		private readonly ITranslation _translation;
		private readonly IAdsManager _adsManager;
		private readonly IGameTime _gameTime;
		private readonly IAnalytics _analytics;
		private readonly ISingular _singular;

		public CConnectionEventsHandler(
			CInitialUserDataProvider initialUserDataProvider, 
			CCrashlyticsKeys crashlyticsKeys,
			CServiceMaster serviceMaster, 
			ITranslation translation, 
			ICrashlytics crashlytics, 
			IGameTime gameTime, 
			IAdsManager adsManager, 
			IAnalytics analytics, 
			ISingular singular
			)
		{
			_initialUserDataProvider = initialUserDataProvider;
			_crashlyticsKeys = crashlyticsKeys;
			_serviceMaster = serviceMaster;
			_translation = translation;
			_crashlytics = crashlytics;
			_gameTime = gameTime;
			_adsManager = adsManager;
			_analytics = analytics;
			_singular = singular;
		}

		public void PreprocessServerConnection()
		{
			InitAds();
			
			_crashlyticsKeys.SetLanguage(_translation.CurrentLanguage);

			string userId = SystemInfo.deviceUniqueIdentifier;
			_crashlytics.SetUserId(userId);
			_analytics.SetUserId(userId);
			
			_singular.Initialize(userId, CPlatform.IsDebug);
			
			_gameTime.Init(CUnixTime.TimestampInMs());
		}
		
		public void PostprocessServerConnection(CConnectResponse response)
		{
			_initialUserDataProvider.Dto = response.User;
		}

		private void InitAds()
		{
			CApplovinAdapterConfig initData = _serviceMaster.Applovin.GetAdapterConfig();
			_adsManager.InitializeProviderAsync(initData, CancellationToken.None);
		}
	}
}