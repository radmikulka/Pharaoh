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
using Pharaoh.Building;
using Pharaoh.GoToStates;
using Pharaoh.Infrastructure;
using UnityEngine;

namespace Pharaoh
{
    public class CCoreGameInstaller : CSceneDiInstaller
    {
        [SerializeField, Child] private CCameraMover _cameraMover;
        [SerializeField, Child] private CCullingGroupApi _cullingGroupApi;
        [SerializeField, Child] private CBuildingMenuPanel _buildingMenuPanel;

        private void OnValidate()
        {
            this.ValidateRefs();
        }

        public override void InstallBindings()
        {
            InstallCamera();
            InstallCoreLogic();
            InstallBuilding();
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

        private void InstallBuilding()
        {
            Container.AddSingleton<CBuildingPlacementValidator>();
            Container.AddSingleton<CBuildingManager>(true);
            Container.AddSingleton<CBuildingTickSystem>(true);
            Container.AddSingletonFromInstance(_buildingMenuPanel);
        }
    }
}