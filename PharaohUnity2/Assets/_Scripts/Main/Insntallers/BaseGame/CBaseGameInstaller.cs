// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.Localization;
using AldaEngine.UnityObjectPool;
using KBCore.Refs;
using Server;
using ServerData;
using ServerData.Design;
using TycoonBuilder;
using TycoonBuilder.Configs;
using TycoonBuilder.GoToStates;
using TycoonBuilder.Infrastructure;
using TycoonBuilder.MenuTriggers;
using TycoonBuilder.Ui;
using TycoonBuilder.Ui.DispatchMenu;
using UnityEngine;

namespace TycoonBuilder
{
    public class CBaseGameInstaller : CSceneDiInstaller
    {
        [SerializeField, Child] private CGoToHandler _goToHandler;
        [SerializeField] private CGameCtsProvider _gameCtsProvider;
        [SerializeField, Child] private CLazyActionQueue _lazyActionQueue;
        [SerializeField, Child] private CVehicleRenderer _vehicleRenderer;
        [SerializeField] private CDialogueQueue _dialogueQueue;
        [SerializeField] private CDialogueHandler _dialogueHandler;
        [SerializeField] private CRewardQueue _rewardQueue;
        [SerializeField] private CImageDownloader _imageDownloader;
        [SerializeField] private CInfoScreenContentsConfig _infoScreenContentsConfig;
        [SerializeField] private CUiGlobalsConfig _uiGlobalsConfig;
        [SerializeField] private CShopDetailRenderer _shopDetailRenderer;
        [SerializeField] private CShopOfferLocalBackgroundsConfig _shopOfferLocalBackgroundsConfig;
        [SerializeField] private CFpsProvider _fpsProvider;
        [SerializeField, Child] private CRenderer _renderer;

        private void OnValidate()
        {
            this.ValidateRefs();
        }

        public override void Start()
        {
            base.Start();
            ValidateNonLazyInjection();
        }

        public override void InstallBindings()
        {
            base.InstallBindings();

            InstallCoreLogic();
            InstallGameMode();
            InstallUser();
            InstallCommonComponents();
            InstallDialogues();
            InstallRewardHandler();
            InstallMenuTriggers();
            InstallUi();
            InstallGoToStates();
            InstallAnalytics();
        }

        private void InstallGoToStates()
        {
            Container.AddSingletonFromInstance<IGoToHandler>(_goToHandler);
            Container.AddSingleton<COpenDispatchMenuHandler>();
            
            Container.AddSingleton<CCloseAllMenusState>();
            Container.AddSingleton<CCloseMenuState>();
            Container.AddSingleton<COpenMenuTabState>();
            Container.AddSingleton<CSceneLightingHandler>();
            Container.AddSingleton<IVehicleShowcase, CVehicleShowcase>();
            Container.Bind(typeof(CTutorialCommentatorProxy), typeof(ITutorialCommentator)).To<CTutorialCommentatorProxy>().AsSingle();
            Container.Bind(typeof(CCameraZoomProxy), typeof(ICameraZoom)).To<CCameraZoomProxy>().AsSingle();
            Container.AddTransient<CMoveCameraToRegionPointState>();
            Container.AddTransient<CMoveToRegionState>();
            Container.AddTransient<CMoveCameraToContractState>();
            Container.AddSingleton<CMoveCameraToRegionPointInstantState>();
            Container.AddTransient<CMoveCameraToResourceMineState>();
            Container.AddTransient<CMoveCameraToSideCityState>();
            Container.AddTransient<CUserPropertyAnalytics>(true);
            Container.AddSingleton<CFocusOnContractDetailState>();
            Container.AddSingleton<CFocusOnIndustryDetailState>();
            Container.AddSingleton<CFocusOnCityDetailState>();
            Container.AddTransient<COpenContractDispatchMenuState>();
            Container.AddTransient<CClaimContractState>();
            Container.AddTransient<COpenResourceDispatchMenuState>();
            Container.AddTransient<COpenCityDispatchMenuState>();
            Container.AddTransient<COpenFactoryMenuState>();
            Container.AddTransient<COpenFactoriesMenuState>();
            Container.AddTransient<COpenShopMenuState>();
            Container.AddTransient<COpenLiveEventMenuState>();
            Container.AddTransient<COpenContractsMenuState>();
            Container.AddTransient<CPulseRectState>();
            Container.AddTransient<COpenCityMenuState>();
            Container.AddTransient<COpenSpecialBuildingMenuState>();
            Container.AddTransient<COpenBuildingParcelMenuState>();
            Container.AddTransient<COpenVehicleDetailState>();
            Container.AddTransient<COpenGetMoreResourcesMenuState>();
            Container.AddTransient<CFocusOnRegionalOfficeState>();
            Container.AddTransient<CUpgradeCityState>();
            Container.AddTransient<CShowNameTagsState>();
            Container.AddTransient<CHideNameTagsState>();
            Container.AddTransient<COpenMenuState>();
            Container.AddTransient<CKillMenuState>();
            Container.AddTransient<COpenCityUpgradeMenuState>();
            Container.AddTransient<CLoadLiveEventState>();
            Container.AddTransient<CLoadCoreGameState>();
            Container.AddTransient<CScrollToPassRewardState>();
        }
        
        private void InstallCoreLogic()
        {
            ITranslation resetableTranslation = GetResetableTranslations();
            Container.Bind<ITranslation>().FromInstance(resetableTranslation);
            
            Container.AddSingletonFromInstance<ICtsProvider>(_gameCtsProvider);
            Container.AddSingleton<CAldaInstantiator>();
            Container.AddSingleton<CLoginService>();
            Container.AddSingleton<CVehicleRotatorInput>();
            Container.AddSingletonFromInstance(_renderer);
            Container.AddSingletonFromInstance(_vehicleRenderer);
            Container.AddSingletonFromInstance(_fpsProvider);
        }
        
        private CResetableTranslation GetResetableTranslations()
        {
            CAldaFramework aldaFramework = ResolveFromParent<CAldaFramework>();
            ITranslation parentTranslations = ResolveFromParent<ITranslation>();
            IEventBus eventBus = ResolveFromParent<IEventBus>();
            CResetableTranslation result = new(parentTranslations, eventBus);
            aldaFramework.TryProcessAldaComponent(result, Container);
            return result;
        }

        private void InstallCommonComponents()
        {
            Container.AddSingletonFromInstance(_lazyActionQueue);
            Container.AddSingletonFromInstance(_shopDetailRenderer);
            Container.AddSingleton<IMainCameraProvider, CMainCameraProvider>();
            Container.AddSingleton<IRequiredBundlesProvider, CRequiredBundlesProvider>();
            Container.AddSingleton<CWorldMap>();
            Container.AddSingleton<CStaticOffers>();
            Container.AddSingleton<CDispatchTimeProvider>();
            Container.AddSingleton<CValuableAnalytics>(true);
            Container.AddSingleton<CEscapeHandler>();
            Container.AddSingleton<IGraphicsQualityProvider, CGraphicsQualityProvider>();
            Container.AddSingleton<CFailedPurchaseResolver>(true);
            Container.AddSingleton<IDecadeProvider, CDecadeProvider>(true);
            Container.AddSingleton<IYearProvider, CYearProvider>(true);
            Container.AddSingletonFromInstance(_infoScreenContentsConfig);
            Container.AddSingleton<CGlobalVariablesHandler>(true);
            Container.AddSingleton<CGlobalVariablesInfoScreenHandler>(true);
            Container.AddSingleton<CYearProgressHandler>(true);
            Container.AddSingleton<CPopUpOffersHandler>(true);
            Container.AddSingleton<CVehiclesProvider>();
            Container.AddSingleton<CClaimedRegionsProvider>();
            Container.AddSingleton<CWarehouseHandler>(true);
            Container.AddSingleton<IRateUsHandler, CRateUsHandler>();
            Container.AddSingleton<IVehicleNameProvider, CVehicleNameProvider>();
            Container.AddSingleton<CContractAnimationHandler>(true);
            Container.AddSingleton<CDoubleTapHandler>(true);
            Container.AddSingleton<IDispatcherOffersProvider, CDispatcherOffersProvider>();
            Container.AddSingleton<CSideCityGraphicProvider>(true);
            Container.AddSingleton<CVehicleComparer>();
            Container.AddSingleton<CIndustryRegionsProvider>();
            Container.AddSingleton<CDecadePassChecklistProvider>();
            Container.AddSingleton<IContractTaskProvider, CContractTaskProvider >();
            //Container.AddSingleton<IContractTaskProvider, CDummyContractTaskProvider>();
        }

        private void InstallUser()
        {
           Container.AddSingleton<CUser>(true);
           Container.AddSingleton<IUserValidator, CUserValidator>();
           Container.AddSingleton<CAnimatedCurrencies>();
           Container.AddSingleton<CUserProgress>();
           Container.AddSingleton<CSpecialValuables>();
           Container.AddSingleton<COwnedValuables>();
           Container.AddSingleton<COwnedVehicles>();
           Container.AddSingleton<COwnedFrames>();
           Container.AddSingleton<CDispatchers>();
           Container.AddSingleton<CFuelStation>();
           Container.AddSingleton<CVehicleDepo>();
           Container.AddSingleton<CRechargerValidator>();
           Container.AddSingleton<CUserTutorials>();
           Container.AddSingleton<CCity>();
           Container.AddSingleton<CTasks>();
           Container.AddSingleton<CProjects>();
           Container.AddSingleton<COffers>();
           Container.AddSingleton<CAccount>();
           Container.AddSingleton<CContracts>();
           Container.AddSingleton<CFactories>();
           Container.AddSingleton<CDebugInfo>();
           Container.AddSingleton<CWarehouse>();
           Container.AddSingleton<CDecadePass>();
           Container.AddSingleton<CSideCities>();
           Container.AddSingleton<CDispatches>();
           Container.AddSingleton<CLiveEvents>();
           Container.AddTransient<CGlobalVariables>();
           
           Container.AddSingleton<CDispatchPathFactory>();
           Container.AddSingleton<CCityFactory>();
           Container.AddSingleton<CFactoriesFactory>();
        }

        private void InstallGameMode()
        {
            Container.AddSingleton<CGameModeManager>(true);
            Container.AddSingleton<CRequiredBundlesDownloader>();
            Container.AddSingleton<CGameModeFactory>();
            Container.AddSingleton<CCoreGameGameMode>();
            Container.AddSingleton<CRegionLiveEventGameGameMode>();
            
            Container.Bind(typeof(CMissionsController), typeof(ICameraPlaneProvider), typeof(IMissionController))
                .To<CMissionsController>()
                .AsSingle();
        }

        private void InstallDialogues()
        {
            Container.AddSingletonFromInstance<IDialogueQueue>(_dialogueQueue);
            Container.AddSingletonFromInstance<IDialogueHandler>(_dialogueHandler);
        }

        private void InstallRewardHandler()
        {
            Container.AddSingletonFromInstance<IRewardQueue>(_rewardQueue);
            Container.AddSingleton<IRewardHandler, CRewardHandler>();
            Container.AddSingleton<CResourceRewardHandler>();
            Container.AddSingleton<CVehicleRewardHandler>();
            Container.AddSingleton<CBuildingRewardHandler>();
            Container.AddSingleton<CValuableRewardHandler>();
            Container.AddSingleton<CDispatchersRewardHandler>();
            
            if (CPlatform.IsEditor)
            {
                Container.AddSingleton<CNativeRateUsHandlerEditor>(true);
                return;
            }

            Container.AddSingleton<CNativeRateUsHandler>(true);
        }

        private void InstallMenuTriggers()
        {
            Container.AddSingleton<CMenuTriggersHandler>(true);
            Container.AddSingleton<CMenuTriggersConfig>();
        }

        private void InstallUi()
        {
            Container.AddSingletonFromInstance(_imageDownloader, true);
            Container.AddSingletonFromInstance(_uiGlobalsConfig, true);
            Container.AddSingletonFromInstance(_shopOfferLocalBackgroundsConfig, true);
            Container.Bind<IMenuHorizontalMaximumSizeProvider>().To<CMenuHorizontalMaximumSizeProvider>().AsSingle().NonLazy();
        }
        
        private void InstallAnalytics()
        {
            Container.AddSingleton<CDialogueAnalytics>(true);
            Container.AddSingleton<CBattlePassAnalytics>(true);
            Container.AddSingleton<CRateUsAnalytics>(true);
            Container.AddSingleton<CContractsAnalytics>(true);
            Container.AddSingleton<CShopAnalytics>(true);
            Container.AddSingleton<CCompanyGrowthAnalytics>(true);
            Container.AddSingleton<CGetMoreMaterialAnalytics>(true);
            Container.AddSingleton<CProfileAnalytics>(true);
            Container.AddSingleton<CTutorialAnalytics>(true);
            Container.AddSingleton<CGoToAnalytics>(true);
            Container.AddSingleton<CEventLeaderboardAnalytics>(true);
            Container.AddSingleton<CShowcaseAnalytics>(true);
        }

        private void ValidateNonLazyInjection()
        {
            if(!CPlatform.IsEditor)
                return;
			
            IEnumerable<Type> nonLazyTypes = CAssemblyScanner.GetTypesWithAttribute(typeof(NonLazyAttribute));
            foreach (Type nonLazyType in nonLazyTypes)
            {
                object nonLazyInstance = Container.TryResolve(nonLazyType);
                if (nonLazyInstance == null)
                {
                    Debug.LogError($"NonLazy type {nonLazyType.Name} is not resolved");
                }
            }
        }
    }
}