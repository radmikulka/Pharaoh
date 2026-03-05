// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.02.2026
// =========================================

namespace ServerData
{
	public class CRetroactiveTasksFactory
	{
		public static CRetroactiveTaskConfig DecadePassFreeRewards()
		{
			return new CRetroactiveTaskConfig(ETaskId.DecadePassFreeRewards, ITaskRequirement.ClaimAllFreeDecadePassRewards());
		}

		public static CRetroactiveTaskConfig FactoryLevel(int levels)
		{
			return new CRetroactiveTaskConfig(ETaskId.FactoryLevel, ITaskRequirement.HaveFactoryLevelsSum(levels));
		}

		public static CRetroactiveTaskConfig UpgradeWarehouse(int level)
		{
			return new CRetroactiveTaskConfig(ETaskId.UpgradeWarehouse, ITaskRequirement.HaveWarehouseLevel(level));
		}

		public static CRetroactiveTaskConfig UpgradeCity(int level)
		{
			return new CRetroactiveTaskConfig(ETaskId.UpgradeCity, ITaskRequirement.HaveCityLevel(level));
		}

		public static CRetroactiveTaskConfig VehicleFleet(int vehiclesCount)
		{
			return new CRetroactiveTaskConfig(ETaskId.VehicleFleet, ITaskRequirement.OwnVehiclesCount(vehiclesCount));
		}

		public static CRetroactiveTaskConfig BuiltCityProperty(int plotsCount)
		{
			return new CRetroactiveTaskConfig(ETaskId.BuiltCityProperty, ITaskRequirement.OwnedBuildingPlotsCount(plotsCount));
		}

		public static CRetroactiveTaskConfig FullyUpgradedVehicles(int vehiclesCount)
		{
			return new CRetroactiveTaskConfig(ETaskId.FullyUpgradedVehicles, ITaskRequirement.HaveFullyUpgradedVehiclesCount(vehiclesCount));
		}
		
		public static CRetroactiveTaskConfig UpgradeVehicleDepot(int level)
		{
			return new CRetroactiveTaskConfig(ETaskId.UpgradeVehicleDepot, ITaskRequirement.HaveVehicleDepotLevel(level));
		}
		
		public static CRetroactiveTaskConfig UpgradeFactoryToLevel(EFactory factory, int level)
		{
			return new CRetroactiveTaskConfig(ETaskId.UpgradeFactoryToLevel, ITaskRequirement.UpgradeFactoryToLevel(factory, level));
		}
		
		public static CRetroactiveTaskConfig UpgradeVehicles(int count)
		{
			return new CRetroactiveTaskConfig(ETaskId.OwnVehicleUpgrades, ITaskRequirement.OwnVehicleUpgrades(count));
		}
		
		public static CRetroactiveTaskConfig PlaceBuildings(int count)
		{
			return new CRetroactiveTaskConfig(ETaskId.HavePlacedBuildings, ITaskRequirement.HavePlacedBuildings(count));
		}
	}
}