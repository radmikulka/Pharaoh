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
using Pharaoh.Ui;
using UnityEngine;

namespace Pharaoh
{
    public class CUiInstaller : CSceneDiInstaller
    {
        [SerializeField, Child] private CScreenManager _screenManager;
        [SerializeField, Child] private CUiParticlesManager _particlesManager;

        private void OnValidate()
        {
            this.ValidateRefs();
        }

        public override void InstallBindings()
        {
            InstallCoreLogic();
            InstallParticles();
            base.InstallBindings();
        }

        private void InstallCoreLogic()
        {
            Container.AddSingleton<CAldaInstantiator>();
            Container.AddSingleton<CUiParticleCounts>();
            Container.AddSingletonFromInstance<IScreenManager>(_screenManager);
            Container.AddSingleton<CUiPurchasingOverlay>(true);
            Container.AddSingleton<CDialogTaskHandler>(true);
        }

        private void InstallParticles()
        {
            Container.AddSingletonFromInstance(_particlesManager);
            Container.AddSingleton<CUiCurrencyParticles>();
        }
    }
}