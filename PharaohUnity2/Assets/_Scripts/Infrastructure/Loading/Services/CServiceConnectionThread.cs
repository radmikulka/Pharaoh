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
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Pharaoh.Loading
{
	public class CServiceConnectionThread
	{
		private readonly INotificationPermissionHandler _notificationPermissionHandler;
		private readonly CServiceFunnelTracker _servicesFunnelTracker;
		private readonly IFacebookService _facebookService;
		private readonly CCrashlyticsKeys _crashlyticsKeys;
		private readonly IRemoteDatabase _remoteDatabase;
		private readonly ICrashlytics _crashlytics;
		private readonly IPurchasing _purchasing;
		private readonly CAttHandler _attHandler;
		private readonly IMessaging _messaging;
		private readonly IAnalytics _analytics;

		public CServiceConnectionThread(
			INotificationPermissionHandler notificationPermissionHandler,
			CServiceFunnelTracker servicesFunnelTracker,
			CCrashlyticsKeys crashlyticsKeys,
			IFacebookService facebookService,
			IRemoteDatabase remoteDatabase,
			ICrashlytics crashlytics,
			IPurchasing purchasing,
			CAttHandler attHandler,
			IMessaging messaging,
			IAnalytics analytics
			)
		{
			_notificationPermissionHandler = notificationPermissionHandler;
			_servicesFunnelTracker = servicesFunnelTracker;
			_crashlyticsKeys = crashlyticsKeys;
			_facebookService = facebookService;
			_remoteDatabase = remoteDatabase;
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
			_servicesFunnelTracker.Send(EServiceFunnelStep.OnlineStart);

			_remoteDatabase.InitializeAsync().Forget();
			_messaging.Initialize().Forget();
			_facebookService.Initialize();
			_purchasing.InitializeAsync().Forget();

			_servicesFunnelTracker.Send(EServiceFunnelStep.MainThreadServicesPassed);

			await _notificationPermissionHandler.RequestPermission();
			_servicesFunnelTracker.Send(EServiceFunnelStep.NotificationsPassed);
			await _attHandler.TryShowRequest();
			_servicesFunnelTracker.Send(EServiceFunnelStep.AttPassed);
		}
	}
}