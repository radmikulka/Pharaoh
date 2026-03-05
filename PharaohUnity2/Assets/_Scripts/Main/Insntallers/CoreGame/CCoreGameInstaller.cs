// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.UnityObjectPool;
using TycoonBuilder;
using KBCore.Refs;
using TycoonBuilder.GoToStates;
using TycoonBuilder.Infrastructure;
using UnityEngine;

namespace TycoonBuilder
{
    public class CCoreGameInstaller : CSceneDiInstaller
    {
        [SerializeField, Child] private CCargoPool _cargoPool;
        [SerializeField, Child] private CCameraMover _cameraMover;
        [SerializeField, Child] private CCargoLoadMaterialFactory _cargoLoadMaterialFactory;
        [SerializeField, Child] private CCargoLoadAnim _cargoLoadAnim;
        [SerializeField, Child] private CDepoResourceFactory _depoResourceFactory;
        [SerializeField, Child] private CTrafficController _trafficController;
        [SerializeField, Child] private CVehicleFactory _vehiclesFactory;
        [SerializeField, Child] private CCullingGroupApi _cullingGroupApi;
        
        private void OnValidate()
        {
            this.ValidateRefs();
        }

        public override void InstallBindings()
        {
            InstallCamera();
            InstallCoreLogic();
            InstallTrafficDestinations();
            InstallTutorialFunnels();
            base.InstallBindings();
        }

        private void InstallCamera()
        {
            Container.AddSingletonFromInstance(_cameraMover);
            Container.AddSingleton<CCameraBorders>();
        }

        private void InstallTutorialFunnels()
        {
            Container.AddSingleton<CGetMoreMaterialTutorialFunnel>();
            Container.AddSingleton<CIntroTutorialAnalytics>();
            Container.AddSingleton<CUpgradeVehicleTutorialFunnel>();
            Container.AddSingleton<CVehicleDepotTutorialFunnel>();
            Container.AddSingleton<CDispatchCenterTutorialFunnel>();
            Container.AddSingleton<CBrokenVehicleTutorialFunnel>();
            Container.AddSingleton<CWarehouseTutorialFunnel>();
            Container.AddSingleton<CPlayerCityTutorialFunnel>();
            Container.AddSingleton<COpenCityPlotTutorialFunnel>();
            Container.AddSingleton<CFactoryTutorialFunnel>();
        }

        private void InstallCoreLogic()
        {
            Container.AddSingleton<CBackgroundBundleDownloader>(true);
            Container.AddSingleton<CStartupQueue>(true);
            Container.AddSingleton<CStaticContractInstances>();
            Container.AddSingleton<CResourceMineInstances>();
            Container.AddSingleton<CSideCityInstances>();
            Container.AddSingleton<CTrafficTaskFactory>();
            Container.AddSingletonFromInstance(_cullingGroupApi);
            Container.AddSingletonFromInstance(_cargoLoadAnim);
            Container.AddSingletonFromInstance(_depoResourceFactory);
            Container.AddSingletonFromInstance(_trafficController);
            Container.AddSingletonFromInstance(_vehiclesFactory);
            Container.AddSingleton<CAldaInstantiator>();
            Container.AddSingleton<CCinematicModeHandler>();
            Container.AddSingletonFromInstance(_cargoLoadMaterialFactory);
            Container.AddSingleton<CCargoFactory>();
            Container.AddSingletonFromInstance(_cargoPool);
            Container.AddSingleton<CRegionPoints>();
            Container.AddSingleton<CIntroTutorial>();
            Container.AddSingleton<ISmartArrowLocker, CSmartArrowLocker>();
            Container.AddSingleton<CTrafficVehicleFollower>();
            Container.AddSingleton<CSimpleVehicleFollower>();
            Container.AddSingleton<CDispatchCenterTutorial>(true);
            Container.AddSingleton<CBrokenVehicleTutorial>(true);
            Container.AddSingleton<CVehicleUpgradeTutorial>(true);
            Container.AddSingleton<COpenCityPlotTutorial>();
            Container.AddSingleton<CContractsMenuTutorial>(true);
            Container.AddSingleton<CPlayerCityTutorial>(true);
            Container.AddSingleton<CVehicleDepotTutorial>(true);
            Container.AddSingleton<CFactoryTutorial>(true);
            Container.AddSingleton<CGetMoreMaterialTutorial>(true);
        }
        
        private void InstallTrafficDestinations()
        {
            Container.AddSingleton<CTrafficDestinationFactory>();
            Container.AddTransient<CContractTrafficDestination>();
        }
    }
}