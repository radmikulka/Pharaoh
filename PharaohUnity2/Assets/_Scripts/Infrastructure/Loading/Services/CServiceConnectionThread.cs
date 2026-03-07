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
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Pharaoh.Loading
{
	public class CServiceConnectionThread
	{
		private readonly INotificationPermissionHandler _notificationPermissionHandler;
		private readonly CServiceFunnelTracker _servicesTechFlow;
		private readonly IFacebookService _facebookService;
		private readonly CCrashlyticsKeys _crashlyticsKeys;
		private readonly IRemoteDatabase _remoteDatabase;
		private readonly IRemoteConfig _remoteConfig;
		private readonly ICrashlytics _crashlytics;
		private readonly IPurchasing _purchasing;
		private readonly CAttHandler _attHandler;
		private readonly IMessaging _messaging;
		private readonly IAnalytics _analytics;

		public CServiceConnectionThread(
			INotificationPermissionHandler notificationPermissionHandler,
			CServiceFunnelTracker servicesTechFlow,
			CCrashlyticsKeys crashlyticsKeys,
			IFacebookService facebookService,
			IRemoteDatabase remoteDatabase,
			IRemoteConfig remoteConfig,
			ICrashlytics crashlytics,
			IPurchasing purchasing,
			CAttHandler attHandler, 
			IMessaging messaging, 
			IAnalytics analytics
			)
		{
			_notificationPermissionHandler = notificationPermissionHandler;
			_servicesTechFlow = servicesTechFlow;
			_crashlyticsKeys = crashlyticsKeys;
			_facebookService = facebookService;
			_remoteDatabase = remoteDatabase;
			_remoteConfig = remoteConfig;
			_crashlytics = crashlytics;
			_purchasing = purchasing;
			_attHandler = attHandler;
			_messaging = messaging;
			_analytics = analytics;
		}

		public void InitOfflineServices()
		{
			_crashlytics.InitializeAsync().Forget();
			_analytics.InitializeAsync().Forget();
			_crashlyticsKeys.SetDeviceInfo();
		}

		public async UniTask InitOnlineServicesAsync()
		{
			_servicesTechFlow.Send(EServiceFunnelStep.OnlineStart);
			UniTask remoteConfigFetch = _remoteConfig.TryFetchAsync(1f);
			
			_remoteDatabase.InitializeAsync().Forget();
			_messaging.Initialize().Forget();
			_facebookService.Initialize();
			_purchasing.InitializeAsync().Forget();
			
			_servicesTechFlow.Send(EServiceFunnelStep.MainThreadServicesPassed);
			
			await _notificationPermissionHandler.RequestPermission();
			_servicesTechFlow.Send(EServiceFunnelStep.NotificationsPassed);
			await _attHandler.TryShowRequest();
			_servicesTechFlow.Send(EServiceFunnelStep.AttPassed);
			
			await remoteConfigFetch;
			_servicesTechFlow.Send(EServiceFunnelStep.RemoteConfigFetched);
			await _remoteConfig.TryActivateFetchedDataAsync();
			_servicesTechFlow.Send(EServiceFunnelStep.RemoteConfigActivated);
		}
	}
}