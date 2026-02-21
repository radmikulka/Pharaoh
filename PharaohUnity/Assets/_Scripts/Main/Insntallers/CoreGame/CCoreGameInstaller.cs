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
using Pharaoh.GoToStates;
using Pharaoh.Infrastructure;
using UnityEngine;

namespace Pharaoh
{
    public class CCoreGameInstaller : CSceneDiInstaller
    {
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
            Container.AddSingletonFromInstance(_cameraMover);
            Container.AddSingleton<CCameraBorders>();
        }

        private void InstallCoreLogic()
        {
            Container.AddSingleton<CStartupQueue>(true);
            Container.AddSingletonFromInstance(_cullingGroupApi);
            Container.AddSingleton<CAldaInstantiator>();
            Container.AddSingleton<CCinematicModeHandler>();
        }
    }
}