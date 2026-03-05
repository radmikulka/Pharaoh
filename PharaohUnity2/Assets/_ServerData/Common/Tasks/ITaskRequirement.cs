// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

using AldaEngine;

namespace ServerData
{
	public interface ITaskRequirement
	{
		public ETaskRequirement Id { get; }
		public ETaskRequirementType TaskRequirementType { get; }
	
		public static CCountableTaskRequirementConfig CreateProducts(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.CreateProducts, ETaskRequirementType.Common, count);
		}
	
		public static CCountableTaskRequirementConfig WatchAd(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.WatchAd, ETaskRequirementType.Common, count);
		}

		public static CCountableTaskRequirementConfig DispatchCargoVehicle(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.DispatchCargoVehicle, ETaskRequirementType.Common, count);
		}

		public static CCountableTaskRequirementConfig DispatchPassengerVehicle(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.DispatchPassengerVehicle, ETaskRequirementType.Common, count);
		}

		public static CCountableTaskRequirementConfig SpendGold(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.SpendGold, ETaskRequirementType.Common, count);
		}

		public static CCountableTaskRequirementConfig CompleteAnyContract(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.CompleteAnyContract, ETaskRequirementType.Common, count);
		}

		public static ITaskRequirement CompleteEventContract(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.CompleteEventContract, ETaskRequirementType.Common, count);
		}
		
		public static ITaskRequirement SpendTycoonCash(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.SpendTycoonCash, ETaskRequirementType.Common, count);
		}
		
		public static ITaskRequirement UpgradeVehicle(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.UpgradeVehicle, ETaskRequirementType.Common, count);
		}

		public static ITaskRequirement ClaimAllFreeDecadePassRewards()
		{
			return new CTaskRequirementConfig(ETaskRequirement.ClaimAllFreeDecadePassRewards, ETaskRequirementType.Retroactive);
		}
		
		public static ITaskRequirement HaveFactoryLevelsSum(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.HaveFactoryLevelsSum, ETaskRequirementType.Retroactive, count);
		}
		
		public static ITaskRequirement HaveWarehouseLevel(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.HaveWarehouseLevel, ETaskRequirementType.Retroactive, count);
		}
		
		public static ITaskRequirement HaveVehicleDepotLevel(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.HaveVehicleDepotLevel, ETaskRequirementType.Retroactive, count);
		}
		
		public static ITaskRequirement HaveCityLevel(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.HaveCityLevel, ETaskRequirementType.Retroactive, count);
		}
		
		public static ITaskRequirement OwnVehiclesCount(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.OwnVehiclesCount, ETaskRequirementType.Retroactive, count);
		}
		
		public static ITaskRequirement OwnVehicleUpgrades(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.OwnVehicleUpgrades, ETaskRequirementType.Retroactive, count);
		}

		public static ITaskRequirement OwnedBuildingPlotsCount(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.OwnedBuildingPlotsCount, ETaskRequirementType.Retroactive, count);
		}
		
		public static ITaskRequirement HaveFullyUpgradedVehiclesCount(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.HaveFullyUpgradedVehiclesCount, ETaskRequirementType.Retroactive, count);
		}
		
		public static ITaskRequirement HavePlacedBuildings(int count)
		{
			return new CCountableTaskRequirementConfig(ETaskRequirement.HavePlacedBuildings, ETaskRequirementType.Retroactive, count);
		}
		
		public static ITaskRequirement UpgradeFactoryToLevel(EFactory factory, int level)
		{
			return new CUpgradeFactoryToLevelRequirementConfig(factory, level);
		}
	}
}