// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CUserTutorialsDto : IMapAble
	{
		[JsonProperty] public EIntroTutorialStep Intro { get; set; }
		[JsonProperty] public EWarehouseTutorialStep Warehouse { get; set; }
		[JsonProperty] public EVehicleDepotTutorialStep VehicleDepot { get; set; }
		[JsonProperty] public EDispatchCenterTutorialStep DispatchCenter { get; set; }
		[JsonProperty] public EGetMoreMaterialTutorialStep GetMoreMaterial { get; set; }
		[JsonProperty] public EPlayerCityTutorialStep City { get; set; }
		[JsonProperty] public EFactoryTutorialStep Factory { get; set; }
		[JsonProperty] public EOpenCityPlotStep OpenCityPlot { get; set; }
		[JsonProperty] public EContractsTutorialStep ContractsTutorialStep { get; set; }
		[JsonProperty] public EBrokenVehicleTutorialStep BrokenVehicle { get; set; }
		[JsonProperty] public EVehicleUpgradeTutorialStep VehicleUpgradeStep { get; set; }

		public CUserTutorialsDto()
		{
		}

		public CUserTutorialsDto(
			EIntroTutorialStep intro, 
			EWarehouseTutorialStep warehouse, 
			EDispatchCenterTutorialStep dispatchCenter,
			EVehicleDepotTutorialStep vehicleDepot,
			EFactoryTutorialStep factory,
			EGetMoreMaterialTutorialStep getMoreMaterial,
			EPlayerCityTutorialStep city,
			EContractsTutorialStep contractsTutorialStep,
			EOpenCityPlotStep openCityPlot,
			EBrokenVehicleTutorialStep brokenVehicle,
			EVehicleUpgradeTutorialStep vehicleUpgradeStep
			)
		{
			ContractsTutorialStep = contractsTutorialStep;
			VehicleUpgradeStep = vehicleUpgradeStep;
			GetMoreMaterial = getMoreMaterial;
			DispatchCenter = dispatchCenter;
			BrokenVehicle = brokenVehicle;
			VehicleDepot = vehicleDepot;
			OpenCityPlot = openCityPlot;
			Warehouse = warehouse;
			Factory = factory;
			Intro = intro;
			City = city;
		}
	}
}