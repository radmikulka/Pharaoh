// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.UnityObjectPool;
using KBCore.Refs;
using ServiceEngine.Purchasing;
using TycoonBuilder.Ui;
using UnityEngine;

namespace TycoonBuilder
{
    public class CUiInstaller : CSceneDiInstaller
    {
        [SerializeField, Child] private CScreenManager _screenManager;
        [SerializeField, Child] private CUiMarkerProvider _markerProvider;
        [SerializeField, Child] private CUiParticlesManager _particlesManager;
        [SerializeField, Child] private CParticlesSourceProvider _particlesSourceProvider;
        [SerializeField, Child] private CFloatingWindowHandler _floatingWindowHandler;
        [SerializeField, Child] private CTopBarItemsUpperHolder _topBarItemsUpperHolder;

        private void OnValidate()
        {
            this.ValidateRefs();
        }

        public override void InstallBindings()
        {
            InstallCoreLogic();
            InstallUI();
            InstallParticles();
            base.InstallBindings();
        }

        private void InstallCoreLogic()
        {
            Container.AddSingleton<CAldaInstantiator>();
            Container.AddSingletonFromInstance<IScreenManager>(_screenManager);
            Container.AddSingleton<CUiRectsProvider>(true);
            Container.AddSingleton<CUiScrollRectsProvider>(true);
            Container.AddSingleton<CUiPurchasingOverlay>(true);
            Container.AddSingleton<CDialogQueue>(true);
        }

        private void InstallUI()
        {
            Container.AddSingletonFromInstance(_markerProvider);
            Container.AddSingletonFromInstance(_floatingWindowHandler, true);
            Container.AddSingleton<CAnimationProvider>();
            Container.AddSingletonFromInstance(_topBarItemsUpperHolder, true);
            Container.AddSingleton<CScreenHider>(true);
        }

        private void InstallParticles()
        {
            Container.AddSingletonFromInstance(_particlesSourceProvider);
            Container.AddSingletonFromInstance(_particlesManager);
            Container.AddSingleton<CParticlesHandler>(true);
            Container.AddSingleton<CUiCurrencyParticles>();
            Container.AddSingleton<CUiParticleCounts>();
        }
    }
}