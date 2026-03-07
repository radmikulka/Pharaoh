// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.Localization;
using AldaEngine.Tcp;
using Pharaoh.Infrastructure;
using ServerData;
using Pharaoh;
using ServiceEngine;
using ServiceEngine.Ads;
using ServiceEngine.Analytics;
using ServiceEngine.Facebook;
using ServiceEngine.Firebase;
using ServiceEngine.GoogleSignIn;
using ServiceEngine.Purchasing;
using ServiceEngine.ServiceMaster;
using ServiceEngine.Singular;
using UnityEngine;
using ILogger = AldaEngine.ILogger;

namespace Pharaoh
{
    [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
    public class CProjectInstaller : CBaseDiInstaller
    {
        [Header("Services")]
        [SerializeField] private CUnityNotifications _unityNotifications;
        [SerializeField] private CFirebaseCrashlytics _crashlytics;
        [SerializeField] private CExitAppHandler _exitAppHandler;
        [SerializeField] private CServiceMaster _serviceMaster;
        [SerializeField] private CPurchasingPharaoh _purchasing;
        [SerializeField] private CAdsManager _adsManager;

        [Header("Systems")]
        [SerializeField] private CMainThreadActionsQueue _mainThreadActionsQueue;
        [SerializeField] private CApplicationCtsProvider _gameCtsProvider;
        [SerializeField] private CRequestDispatcher _hitsDispatcher;
        [SerializeField] private CBundleManager _bundleManager;
        [SerializeField] private CEventSystem _eventSystem;
        [SerializeField] private CResourceConfigs _configs;
        [SerializeField] private CAudioManager _audioManager;

        public override void InstallBindings()
        {
            base.InstallBindings();

            InstallCommonComponents();
            InstallCoreLogic();
            InstallServices();
            InstallConfigs();
            InstallServerClient();
            InstallEventBus();
            InstallMappers();
            InstallWaitingScreen();
            BindLoadingScreenProxy();
            InstallAuth();
            InstallAds();
            
            Container.AddSingleton<ITranslation, CDummlyLocalizationProvider>();
            Container.AddSingleton<ISettings, CSettings>();
        }
        
        private void InstallCoreLogic()
        {
            Container.AddSingletonFromInstance<CApplicationCtsProvider>(_gameCtsProvider);
            Container.AddSingletonFromInstance<ICtsProvider>(_gameCtsProvider);
            Container.AddSingletonFromInstance<IAudioManager>(_audioManager);
            Container.AddSingleton<CAldaInstantiator>();
            Container.AddSingleton<ISceneManager, CSceneManager>(true);
            Container.AddSingletonFromInstance<IBundleManager>(_bundleManager);
            Container.AddSingletonFromInstance<CEventSystem>(_eventSystem);
        }

        private void InstallMappers()
        {
            Container.AddSingleton<IMapper, CMapper>();
            Container.AddSingleton<IJsonMapper, CNewtonsoftJsonMapper>();
            Container.AddSingleton<CServerDataMapper>(true);
            Container.AddSingleton<CUserMapper>(true);
        }
        
        private void InstallCommonComponents()
        {
            Container.InstallVersionProvider();
            Container.AddSingleton<ILogger, CUnityLogger>();
            Container.AddSingleton<CDoTween>(true);
            Container.AddSingleton<IRestartGameHandler, CRestartGameHandler>();
            Container.AddSingletonFromInstance<CMainThreadActionsQueue>(_mainThreadActionsQueue);
        }

        private void BindLoadingScreenProxy()
        {
            CLoadingScreenProxy loadingScreenProxy = new();
            Container.Bind(typeof(CLoadingScreenProxy), typeof(ILoadingScreen)).FromInstance(loadingScreenProxy)
                .AsSingle();
        }
        
        private void InstallWaitingScreen()
        {
            Container.Bind(typeof(IWaitingScreen), typeof(CWaitingScreenProvider))
                .FromInstance(new CWaitingScreenProvider()).AsSingle();
        }

        private void InstallServerClient()
        {
            Container.AddSingleton<CErrorHandler>();
            Container.AddSingleton<CInitialUserDtoProvider>();
            Container.AddSingleton<ITcpCrypter, CTcpCrypter>();
            Container.AddSingleton<IServerTime, CServerTime>();
            Container.AddSingleton<CDebugUserDeletionHandler>();
            Container.AddSingleton<CServerEndpointProvider>();
            Container.AddSingleton<CCommunicationTokenProvider>();
            Container.AddSingleton<CRequestSender>();
            Container.AddSingleton<CClientEndPoints>(true);
            Container.AddSingleton<CTcpServerEndPointsCollection>();
            Container.AddSingleton<CTcpClientFactory>();
            Container.AddSingleton<IActiveAuth, CAuthUidStorage>();
            Container.AddSingletonFromInstance<CRequestDispatcher>(_hitsDispatcher);
        }

        private void InstallConfigs()
        {
            BindAndInjectNonComponent(_configs);
            Container.AddSingleton<CInAppPrices>();
        }

        private void InstallServices()
        {
            Container.InstallAdTooltips();
            Container.AddSingleton<CNotificationFactory>();
            Container.InstallUnityNotifications(_unityNotifications, new CNullNotificationNameParser(), new Color(0.1f, 0.58f, 1f));
            Container.AddSingletonFromInstance<IAnalytics>(GetAnalytics());
            Container.AddSingleton<IRemoteDatabase, CFirebaseDatabase>();
            Container.AddSingleton<IRemoteConfig, CFirebaseRemoteConfig>();
            Container.AddSingletonFromInstance<ICrashlytics>(_crashlytics);
            Container.AddSingletonFromInstance(_serviceMaster);
            Container.AddSingleton<CInternetReachabilityChecker>();
            Container.AddSingleton<CCrashlyticsKeys>();
            Container.AddSingleton<CServiceFunnelTracker>();
            Container.AddSingleton<CLoadingFunnelTracker>();
            Container.AddSingleton<CAttHandler>();
            Container.InstallInAppUpdate();
            Container.AddSingleton<ISingular, CSingular>();
            Container.AddSingleton<IDeviceIdProvider, CMobileDeviceIdProvider>();
            Container.AddSingleton<IMessaging, CFirebaseMessaging>();
            Container.AddSingletonFromInstance<IExitAppHandler>(_exitAppHandler);
            Container.InstallNativeRateUs();
            
            InstallPurchasing();
        }
        
        private IAnalytics GetAnalytics()
        {
            CFirebaseAnalytics firebase = new();
            if (CPlatform.IsDebug || CPlatform.IsEditor)
            {
                return new CAnalyticsToConsole(firebase);
            }

            return firebase;
        }

        private void InstallAuth()
        {
            Container.AddSingleton<IAuthService, CAuthService>();
            
            Container.AddSingleton<IFacebookService, CFacebookService>();
            Container.AddSingleton<IAppleSignIn, CAppleSignIn>();
            
            IGoogleSignIn googleSignIn = GetGoogleSignIn();
            Container.AddSingletonFromInstance<IGoogleSignIn>(googleSignIn);
        }

        private void InstallAds()
        {
            Container.AddSingletonFromInstance<IAdsManager>(_adsManager);
            Container.AddSingleton<CAdPlayingInputLocker>(true);
            Container.AddSingleton<CAdsAnalytics>(true);
            
            if (CPlatform.IsEditor)
            {
                Container.Bind<IAdsProvider>().To<CEditorAds>().AsSingle();
                return;
            }

            Container.Bind<IAdsProvider>().To<CApplovinAdapter>().AsSingle();
        }
        
        private IGoogleSignIn GetGoogleSignIn()
        {
            if (CPlatform.IsEditor || CPlatform.IsIosPlatform)
                return new CMockGoogleSignIn();
            
            CGoogleSignIn googleSignIn = new(_serviceMaster.GoogleLogin.WebClientId);
            return googleSignIn;
        }

        private void InstallPurchasing()
        {
            Container.AddSingleton<IPurchaseValidator, CPurchaseValidator>();
            Container.AddSingleton<CPurchasingAnalytics>();
            Container.AddSingleton<CPurchaseReceiptUnpacker>();
            Container.AddSingleton<IPurchasingProductsProvider, CPurchasingProductsProvider>();
            Container.AddSingletonFromInstance<IPurchasing>(_purchasing);

            Container.AddSingleton<IPricesParser, CPricesParser>();
            Container.AddSingleton<ITextFileReader, CTextResourceReader>();
        }

        private void InstallEventBus()
        {
            CRootEventBus rootBus = new();
            Container.AddSingletonFromInstance<CRootEventBus>(rootBus);
            Container.AddSingletonFromInstance<IEventBus>(rootBus);
        }
        
        private class CDummlyLocalizationProvider : ITranslation
        {
            public ELanguageCode SystemLanguage { get; }
            public ELanguageCode CurrentLanguage { get; }
            public CEvent<ITranslation> OnLanguageChanged { get; } = new("");
            
            public string GetText(string key)
            {
                return key;
            }

            public string GetText(string key, params object[] args)
            {
                return key;
            }

            public void SetLanguage(ELanguageCode language)
            {
               
            }

            public List<ELanguageCode> GetSupportedLanguages()
            {
                return new List<ELanguageCode>();
            }
        }
        
        private class CSettings : ISettings
        {
            public EGraphicsQuality Quality { get; }
            public ELanguageCode Language { get; }
        }
    }
}