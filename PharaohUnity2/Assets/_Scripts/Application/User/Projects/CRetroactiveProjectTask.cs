// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.02.2026
// =========================================

using System;
using ServerData;

namespace TycoonBuilder
{
	public class CRetroactiveProjectTask
	{
		public readonly ETaskId TaskId;
		public readonly int TargetCount;
		public readonly EFactory Factory;

		public CRetroactiveProjectTask(ETaskId taskId, int targetCount, EFactory factory = EFactory.None)
		{
			TaskId = taskId;
			TargetCount = targetCount;
			Factory = factory;
		}

		public bool IsCompleted(CUser user) => TaskId switch
		{
			ETaskId.FactoryLevel          => user.Factories.HaveFactoryLevelSum(TargetCount),
			ETaskId.UpgradeWarehouse      => user.Warehouse.LevelData.Level >= TargetCount,
			ETaskId.UpgradeCity           => user.City.GetOrCreateCityData().LevelData.Level >= TargetCount,
			ETaskId.VehicleFleet          => user.Vehicles.GetAllOwnedVehicles().Length >= TargetCount,
			ETaskId.BuiltCityProperty     => user.City.GetOrCreateCityData().GetUnlockedBuildingPlotsCount() >= TargetCount,
			ETaskId.FullyUpgradedVehicles => user.Vehicles.GetFullyUpgradedVehiclesCount() >= TargetCount,
			ETaskId.UpgradeVehicleDepot   => user.VehicleDepo.GetCurrentLevel() >= TargetCount,
			ETaskId.OwnVehicleUpgrades    => user.Vehicles.GetUpgradesCount() >= TargetCount,
			ETaskId.HavePlacedBuildings   => user.City.GetOrCreateCityData().GetPlotsWithBuildingCount() >= TargetCount,
			ETaskId.UpgradeFactoryToLevel => EvaluateUpgradeFactoryToLevel(user),
			_ => throw new ArgumentOutOfRangeException(nameof(TaskId), TaskId.ToString())
		};

		private bool EvaluateUpgradeFactoryToLevel(CUser user)
			=> user.Factories.GetOrCreateFactory(Factory).LevelData.Level >= TargetCount;
	}
}
