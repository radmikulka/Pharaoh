// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using Pharaoh.Infrastructure;
using ServerData;
using ServiceEngine;
using ServiceEngine.Ads;
using ServiceEngine.Firebase;
using ServiceEngine.ServiceMaster;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Pharaoh.Loading
{
	public class CServiceConnectionThread
	{
		private readonly INotificationPermissionHandler _notificationPermissionHandler;
		private readonly IFacebookService _facebookService;
		private readonly CCrashlyticsKeys _crashlyticsKeys;
		private readonly CServiceMaster _serviceMaster;
		private readonly IRemoteConfig _remoteConfig;
		private readonly ITranslation _translation;
		private readonly ICrashlytics _crashlytics;
		private readonly IPurchasing _purchasing;
		private readonly CAttHandler _attHandler;
		private readonly IAdsManager _adsManager;
		private readonly IMessaging _messaging;
		private readonly IAnalytics _analytics;
		private readonly ISingular _singular;

		public CServiceConnectionThread(
			INotificationPermissionHandler notificationPermissionHandler,
			CCrashlyticsKeys crashlyticsKeys,
			IFacebookService facebookService,
			CServiceMaster serviceMaster,
			IRemoteConfig remoteConfig,
			ICrashlytics crashlytics,
			ITranslation translation, 
			IPurchasing purchasing, 
			IAdsManager adsManager, 
			CAttHandler attHandler, 
			IMessaging messaging, 
			IAnalytics analytics, 
			ISingular singular
			)
		{
			_notificationPermissionHandler = notificationPermissionHandler;
			_crashlyticsKeys = crashlyticsKeys;
			_facebookService = facebookService;
			_serviceMaster = serviceMaster;
			_remoteConfig = remoteConfig;
			_crashlytics = crashlytics;
			_translation = translation;
			_purchasing = purchasing;
			_attHandler = attHandler;
			_adsManager = adsManager;
			_messaging = messaging;
			_analytics = analytics;
			_singular = singular;
		}

		public void InitOfflineServices()
		{
			_crashlytics.InitializeAsync().Forget();
			_analytics.InitializeAsync().Forget();
			_crashlyticsKeys.SetDeviceInfo();
		}

		public async UniTask InitOnlineServicesAsync()
		{
			UniTask remoteConfigFetch = _remoteConfig.TryFetchAsync(1f);
			
			_messaging.Initialize().Forget();
			_facebookService.Initialize();
			_purchasing.InitializeAsync().Forget();
			
			await _notificationPermissionHandler.RequestPermission();
			await _attHandler.TryShowRequest();
			
			await remoteConfigFetch;
			await _remoteConfig.TryActivateFetchedDataAsync();
			
			
			
			InitAds();
			
			_crashlyticsKeys.SetLanguage(_translation.CurrentLanguage);

			string userId = SystemInfo.deviceUniqueIdentifier;
			_crashlytics.SetUserId(userId);
			_analytics.SetUserId(userId);
			
			_singular.Initialize(userId, CPlatform.IsDebug);
		}
		
		private void InitAds()
		{
			CApplovinAdapterConfig initData = _serviceMaster.Applovin.GetAdapterConfig();
			_adsManager.InitializeProviderAsync(initData, CancellationToken.None);
		}
	}
}