// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CUpgradeLevelConfigBuilder
	{
		private readonly List<IUpgradeRequirement> _upgradeRequirements = new(2);
		private readonly List<IUpgradeReward> _rewards = new(2);
		private readonly long _upgradeDurationInSeconds;

		public CUpgradeLevelConfigBuilder(long upgradeDurationInSeconds)
		{
			_upgradeDurationInSeconds = upgradeDurationInSeconds;
		}
		
		public CUpgradeLevelConfigBuilder AddUpgradeRequirement(IUpgradeRequirement requirement)
		{
			_upgradeRequirements.Add(requirement);
			return this;
		}
		
		public CUpgradeLevelConfigBuilder AddReward(IUpgradeReward reward)
		{
			_rewards.Add(reward);
			return this;
		}
		
		public CUpgradeLevelConfig Build()
		{
			return new CUpgradeLevelConfig(
				_rewards.ToArray(), 
				_upgradeRequirements.ToArray(), 
				_upgradeDurationInSeconds
			);
		}
	}
}