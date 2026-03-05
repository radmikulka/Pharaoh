// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace ServerData
{
	public class CUpgradeLevelConfig
	{
		private readonly IUpgradeReward[] _rewards;
		private readonly IUpgradeRequirement[] _upgradeRequirements;
		
		public readonly long UpgradeDurationInSeconds;
		public IReadOnlyList<IUpgradeReward> Rewards => _rewards;
		public IReadOnlyList<IUpgradeRequirement> UpgradeRequirements => _upgradeRequirements;
		public long UpgradeDurationInMilliseconds => UpgradeDurationInSeconds * CTimeConst.Second.InMilliseconds;

		public CUpgradeLevelConfig(IUpgradeReward[] rewards, IUpgradeRequirement[] upgradeRequirements, long upgradeDurationInSeconds)
		{
			UpgradeDurationInSeconds = upgradeDurationInSeconds;
			_upgradeRequirements = upgradeRequirements;
			_rewards = rewards;
		}

		public int GetFuelCapacityIncrease()
		{
			return _rewards.GetUpgradeRewardOrDefault<CFuelStationCapacityReward>()?.CapacityIncrease ?? 0;
		}

		public int GetFuelProductionIncrease()
		{
			return _rewards.GetUpgradeRewardOrDefault<CFuelStationProductionReward>()?.ProductionIncrease ?? 0;
		}
		
		public int GetDurabilityRepairAmountIncrease()
		{
			return _rewards.GetUpgradeRewardOrDefault<CVehicleDurabilityRepairAmountReward>()?.AmountIncrease ?? 0;
		}
	}
}