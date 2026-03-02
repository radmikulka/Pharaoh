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
using Pharaoh.GoToStates;
using Pharaoh.Infrastructure;
using Pharaoh.Ui;
using ServerData;
using Pharaoh;
using UnityEngine;

namespace Pharaoh
{
    public class CBaseGameInstaller : CSceneDiInstaller
    {
        [SerializeField, Child] private CGoToHandler _goToHandler;
        [SerializeField] private CGameCtsProvider _gameCtsProvider;
        [SerializeField, Child] private CLazyActionQueue _lazyActionQueue;
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
            InstallSave();
            InstallCommonComponents();
        }
        
        private void InstallCoreLogic()
        {
            ITranslation resetableTranslation = GetResetableTranslations();
            Container.Bind<ITranslation>().FromInstance(resetableTranslation);
            
            Container.AddSingletonFromInstance<ICtsProvider>(_gameCtsProvider);
            Container.AddSingleton<CAldaInstantiator>();
            Container.AddSingletonFromInstance(_renderer);
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

        private void InstallSave()
        {
            Container.AddSingleton<ISaveManager, CSaveManager>(true);
        }

        private void InstallCommonComponents()
        {
            Container.AddSingletonFromInstance(_lazyActionQueue);
            Container.AddSingleton<IMainCameraProvider, CMainCameraProvider>();
            Container.AddSingleton<IRequiredBundlesProvider, CRequiredBundlesProvider>();
            Container.AddSingleton<CEscapeHandler>();
            Container.AddSingleton<IGraphicsQualityProvider, CGraphicsQualityProvider>();
            Container.AddSingleton<CDoubleTapHandler>(true);
        }

        private void InstallUser()
        {
           Container.AddSingleton<CUser>(true);
           Container.AddSingleton<CAnimatedCurrencies>();
           Container.AddSingleton<COwnedValuables>();
           Container.AddSingleton<COwnedResources>();
           Container.AddSingleton<CActiveMission>();
           Container.AddSingleton<COwnedResearches>();
        }

        private void InstallGameMode()
        {
            Container.AddSingleton<CGameModeManager>(true);
            Container.AddSingleton<CRequiredBundlesDownloader>();
            Container.AddSingleton<CGameModeFactory>();
            Container.AddSingleton<CCoreGameGameMode>();
            
            Container.Bind(typeof(CMissionController), typeof(ICameraPlaneProvider), typeof(IMissionController))
                .To<CMissionController>()
                .AsSingle();
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