// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using AldaEngine;
using ServerData;
using ServerData.Hits;

namespace TycoonBuilder
{
	public class CUserTutorials : CBaseUserComponent
	{
		private EVehicleUpgradeTutorialStep _vehicleUpgradeStep;
		private EDispatchCenterTutorialStep _dispatchCenterStep;
		private EBrokenVehicleTutorialStep _brokenVehicleStep;
		private EWarehouseTutorialStep _warehouseTutorialStep;
		private EGetMoreMaterialTutorialStep _getMoreMaterial;
		private EContractsTutorialStep _contractsTutorialStep;
		private EVehicleDepotTutorialStep _vehicleDepot;
		private EPlayerCityTutorialStep _playerCity;
		private EFactoryTutorialStep _factory;
		private EOpenCityPlotStep _cityPlot;
		private EIntroTutorialStep _intro;

		public bool IntroCompleted => GetIntroStep() == EIntroTutorialStep.Completed;

		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;

		public CUserTutorials(IEventBus eventBus, CHitBuilder hitBuilder)
		{
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
		}

		public void InitialSync(CUserTutorialsDto dto)
		{
			_contractsTutorialStep = dto.ContractsTutorialStep;
			_vehicleUpgradeStep = dto.VehicleUpgradeStep;
			_dispatchCenterStep = dto.DispatchCenter;
			_warehouseTutorialStep = dto.Warehouse;
			_getMoreMaterial = dto.GetMoreMaterial;
			_brokenVehicleStep = dto.BrokenVehicle;
			_vehicleDepot = dto.VehicleDepot;
			_cityPlot = dto.OpenCityPlot;
			_playerCity = dto.City;
			_factory = dto.Factory;
			_intro = dto.Intro;
		}
		
		public EIntroTutorialStep GetIntroStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.IntroTutorial))
				return EIntroTutorialStep.Completed;
			
			return _intro;
		}
		
		public EContractsTutorialStep GetContractsTutorialStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.ContractsMenu))
				return EContractsTutorialStep.Completed;
			
			return _contractsTutorialStep;
		}
		
		public EDispatchCenterTutorialStep GetDispatchCenterStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.DispatchCenter))
				return EDispatchCenterTutorialStep.Completed;
			
			return _dispatchCenterStep;
		}
		
		public EOpenCityPlotStep GetCityPlotStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.OpenCityPlot))
				return EOpenCityPlotStep.Completed;
			
			return _cityPlot;
		}
		
		public EVehicleUpgradeTutorialStep GetVehicleUpgradeTutorialStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.VehicleUpgrade))
				return EVehicleUpgradeTutorialStep.Completed;
			
			return _vehicleUpgradeStep;
		}
		
		public EWarehouseTutorialStep GetWarehouseTutorialStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.Warehouse))
				return EWarehouseTutorialStep.Completed;
			
			return _warehouseTutorialStep;
		}
		
		public EVehicleDepotTutorialStep GetVehicleDepotTutorialStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.VehicleDepot))
				return EVehicleDepotTutorialStep.Completed;
			
			return _vehicleDepot;
		}
		
		public EFactoryTutorialStep GetFactoryTutorialStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.OpenFirstFactory))
				return EFactoryTutorialStep.Completed;
			
			return _factory;
		}
		
		public EBrokenVehicleTutorialStep GetBrokenVehicleTutorialStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.BrokenVehicle))
				return EBrokenVehicleTutorialStep.Completed;
			
			return _brokenVehicleStep;
		}
		
		public EGetMoreMaterialTutorialStep GetGetMoreMaterialTutorialStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.GetMoreMaterial))
				return EGetMoreMaterialTutorialStep.Completed;
			
			return _getMoreMaterial;
		}
		
		public EPlayerCityTutorialStep GetPlayerCityTutorialStep()
		{
			if(CDebugConfig.Instance.TutorialSkip.HasFlag(ETutorialSkip.PlayerCity))
				return EPlayerCityTutorialStep.Completed;
			
			return _playerCity;
		}

		public bool IsTutorialCompleted(EIntroTutorialStep completed)
		{
			return GetIntroStep() >= completed;
		}
		
		public bool IsTutorialCompleted(EGetMoreMaterialTutorialStep completed)
		{
			return GetGetMoreMaterialTutorialStep() >= completed;
		}
		
		public bool IsTutorialCompleted(EOpenCityPlotStep completed)
		{
			return GetCityPlotStep() >= completed;
		}
		
		public bool IsTutorialCompleted(EDispatchCenterTutorialStep completed)
		{
			return GetDispatchCenterStep() >= completed;
		}
		
		public bool IsTutorialCompleted(EVehicleUpgradeTutorialStep completed)
		{
			return GetVehicleUpgradeTutorialStep() >= completed;
		}
		
		public bool IsTutorialCompleted(EPlayerCityTutorialStep completed)
		{
			return GetPlayerCityTutorialStep() >= completed;
		}
		
		public bool IsTutorialCompleted(EContractsTutorialStep completed)
		{
			return GetContractsTutorialStep() >= completed;
		}

		public bool IsTutorialCompleted(EWarehouseTutorialStep completed)
		{
			return GetWarehouseTutorialStep() >= completed;
		}
		
		public bool IsTutorialCompleted(EBrokenVehicleTutorialStep completed)
		{
			return GetBrokenVehicleTutorialStep() >= completed;
		}

		public bool IsTutorialCompleted(EVehicleDepotTutorialStep completed)
		{
			return GetVehicleDepotTutorialStep() >= completed;
		}

		public bool IsTutorialCompleted(EFactoryTutorialStep completed)
		{
			return GetFactoryTutorialStep() >= completed;
		}

		public void SetIntroStep(EIntroTutorialStep step)
		{
			if(_intro >= step)
				return;
			
			_eventBus.Send(new CIntroTutorialStepCompleted(_intro));
			_intro = step;

			_hitBuilder.GetBuilder(new CSetIntroTutorialStepRequest(step)).BuildAndSend();
		}

		public void SetDispatchCenterStep(EDispatchCenterTutorialStep step)
		{
			if(_dispatchCenterStep >= step)
				return;
			
			_dispatchCenterStep = step;
			_hitBuilder.GetBuilder(new CSetDispatchCenterTutorialStepRequest(step)).BuildAndSend();
		}
		
		public void SetFactoryStep(EFactoryTutorialStep step)
		{
			if(_factory >= step)
				return;
			
			_factory = step;
			_hitBuilder.GetBuilder(new CSetFactoryTutorialStepRequest(step)).BuildAndSend();
		}

		public void SetWarehouseTutorialStep(EWarehouseTutorialStep step)
		{
			if(_warehouseTutorialStep >= step)
				return;
			
			_warehouseTutorialStep = step;
			_hitBuilder.GetBuilder(new CSetWarehouseTutorialStepRequest(step)).BuildAndSend();
		}
		
		public void SetCityPlotStep(EOpenCityPlotStep step)
		{
			if(_cityPlot >= step)
				return;
			
			_cityPlot = step;
			_hitBuilder.GetBuilder(new CSetOpenCityPlotTutorialStepRequest(step)).BuildAndSend();
		}
		
		public void SetPlayerCityTutorialStep(EPlayerCityTutorialStep step)
		{
			if(_playerCity >= step)
				return;
			
			_playerCity = step;
			_hitBuilder.GetBuilder(new CSetPlayerCityTutorialStepRequest(step)).BuildAndSend();
		}
		
		public void SetVehicleDepotTutorialStep(EVehicleDepotTutorialStep step)
		{
			if(_vehicleDepot >= step)
				return;
			
			_vehicleDepot = step;
			_hitBuilder.GetBuilder(new CSetVehicleDepotTutorialStepRequest(step)).BuildAndSend();
		}
		
		public void SetContractsTutorialStep(EContractsTutorialStep step)
		{
			if(_contractsTutorialStep >= step)
				return;
			
			_contractsTutorialStep = step;
			_hitBuilder.GetBuilder(new CSetContractsTutorialStepRequest(step)).BuildAndSend();
		}
		
		public void SetBrokenVehicleTutorialStep(EBrokenVehicleTutorialStep step)
		{
			if(_brokenVehicleStep >= step)
				return;
			
			_brokenVehicleStep = step;
		}
		
		public void SetUpgradeVehicleTutorialStep(EVehicleUpgradeTutorialStep step)
		{
			if(_vehicleUpgradeStep >= step)
				return;
			
			_vehicleUpgradeStep = step;
		}

		public void SetGetMoreMaterialStep(EGetMoreMaterialTutorialStep step)
		{
			if(_getMoreMaterial >= step)
				return;
			
			_getMoreMaterial = step;
			_hitBuilder.GetBuilder(new CSetGetMoreMaterialTutorialStepRequest(step)).BuildAndSend();
		}
	}
}