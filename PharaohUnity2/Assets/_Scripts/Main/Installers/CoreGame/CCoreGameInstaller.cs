// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.UnityObjectPool;
using Pharaoh;
using KBCore.Refs;
using Pharaoh.Infrastructure;
using UnityEngine;

namespace Pharaoh
{
    public class CCoreGameInstaller : CSceneDiInstaller
    {
        [SerializeField, Child] private CCameraRotator _cameraRotator;
        [SerializeField, Child] private CCameraMover _cameraMover;
        [SerializeField, Child] private CCullingGroupApi _cullingGroupApi;

        private void OnValidate()
        {
            this.ValidateRefs();
        }

        public override void InstallBindings()
        {
            InstallCamera();
            InstallCoreLogic();
            base.InstallBindings();
        }

        private void InstallCamera()
        {
            Container.AddSingletonFromInstance(_cameraRotator);
            Container.AddSingletonFromInstance(_cameraMover);
        }

        private void InstallCoreLogic()
        {
            Container.AddSingleton<CBackgroundBundleDownloader>(true);
            Container.AddSingleton<CStartupQueue>(true);
            Container.AddSingletonFromInstance(_cullingGroupApi);
            Container.AddSingleton<CAldaInstantiator>();
        }
    }
}