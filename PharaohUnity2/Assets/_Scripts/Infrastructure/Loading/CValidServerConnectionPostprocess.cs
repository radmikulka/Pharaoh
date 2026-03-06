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

namespace Pharaoh
{
	public class CValidServerConnectionPostprocess
	{
		private readonly CCommunicationTokenProvider _communicationTokenProvider;
		private readonly CInitialUserDtoProvider _initialUserDtoProvider;
		private readonly CCrashlyticsKeys _crashlyticsKeys;
		private readonly CServiceMaster _serviceMaster;
		private readonly ICrashlytics _crashlytics;
		private readonly ITranslation _translation;
		private readonly IAuthService _authService;
		private readonly IAdsManager _adsManager;
		private readonly IServerTime _serverTime;
		private readonly IAnalytics _analytics;
		private readonly ISingular _singular;

		public CValidServerConnectionPostprocess(
			CCommunicationTokenProvider communicationTokenProvider,
			CInitialUserDtoProvider initialUserDtoProvider, 
			CCrashlyticsKeys crashlyticsKeys,
			CServiceMaster serviceMaster, 
			ITranslation translation, 
			ICrashlytics crashlytics, 
			IAuthService authService, 
			IServerTime serverTime, 
			IAdsManager adsManager, 
			IAnalytics analytics, 
			ISingular singular
			)
		{
			_communicationTokenProvider = communicationTokenProvider;
			_initialUserDtoProvider = initialUserDtoProvider;
			_crashlyticsKeys = crashlyticsKeys;
			_serviceMaster = serviceMaster;
			_translation = translation;
			_crashlytics = crashlytics;
			_authService = authService;
			_serverTime = serverTime;
			_adsManager = adsManager;
			_analytics = analytics;
			_singular = singular;
		}
		
		public async UniTask PostprocessServerConnection(CConnectResponse response, CancellationToken ct)
		{
			_initialUserDtoProvider.Dto = response.User;

			await InitAds(ct);
			
			_crashlyticsKeys.SetLanguage(_translation.CurrentLanguage.ToString());
			
			_crashlytics.SetUserId(response.User.Account.PublicId);

			if (response.User.Account.IsTestUser)
			{
				_analytics.SetActiveAnalytics(false);
				_singular.MarkAsTester();
			}
			
			_analytics.SetUserId(response.User.Account.PublicId);
			
			_singular.Initialize(response.User.Account.PublicId, response.User.Account.IsTestUser);
			_serverTime.Init(response.ServerTimeInMs, response.DayRefreshTimeInMs);
			
			_communicationTokenProvider.SetCommunicationToken(response.CommunicationToken);
		}

		private async UniTask InitAds(CancellationToken ct)
		{
			CApplovinAdapterConfig initData = _serviceMaster.Applovin.GetAdapterConfig();
			await _adsManager.InitializeProviderAsync(initData, ct);
		}
	}
}