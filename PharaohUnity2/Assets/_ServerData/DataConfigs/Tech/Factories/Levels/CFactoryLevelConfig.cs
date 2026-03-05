// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;

namespace ServerData
{
	public class CFactoryLevelConfig
	{
		private readonly IUpgradeReward[] _rewards;
		private readonly IUpgradeRequirement[] _upgradeRequirements;
		
		public readonly long UpgradeDurationInSeconds;
		public IReadOnlyList<IUpgradeReward> Rewards => _rewards;
		public IReadOnlyList<IUpgradeRequirement> UpgradeRequirements => _upgradeRequirements;
		
		public long UpgradeDurationInMs => UpgradeDurationInSeconds * CTimeConst.Second.InMilliseconds;

		public CFactoryLevelConfig(
			IUpgradeReward[] rewards, 
			IUpgradeRequirement[] upgradeRequirements, 
			long upgradeDurationInSeconds
		)
		{
			_rewards = rewards;
			_upgradeRequirements = upgradeRequirements;
			UpgradeDurationInSeconds = upgradeDurationInSeconds;
		}

		public int GetMaxDurabilityBonus()
		{
			return _rewards.GetUpgradeRewardOrDefault<CFactoryMaxDurabilityReward>()?.DurabilityIncrease ?? 0;
		}
		
		public long GetRepairTimeBonus()
		{
			return _rewards.GetUpgradeRewardOrDefault<CFactoryRepairSpeedReward>()?.RepairTimeDecreaseSeconds ?? 0;
		}
		
		public int GetRepairAmountBonus()
		{
			return _rewards.GetUpgradeRewardOrDefault<CFactoryRepairSpeedReward>()?.RepairAmountIncrease ?? 0;
		}
		
		public CFactoryProductConfig GetProductOrDefault(EResource resource)
		{
			foreach (IUpgradeReward reward in _rewards)
			{
				if (reward is not CNewFactoryProductReward matchedReward)
					continue;
				if (matchedReward.ProductConfig.Resource.Id == resource)
					return matchedReward.ProductConfig;
			}
			return null;
		}

		public IEnumerable<EResource> GetProducts()
		{
			return _rewards.Where(reward => reward is CNewFactoryProductReward).Select(reward => ((CNewFactoryProductReward)reward).ProductConfig.Resource.Id);
		}
	}
}