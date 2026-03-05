// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CFactoryLevelConfigBuilder
	{
		private readonly List<IUpgradeRequirement> _upgradeRequirements = new(2);
		private readonly List<IUpgradeReward> _rewards = new(2);
		private readonly long _upgradeDurationInSeconds;

		public CFactoryLevelConfigBuilder(long upgradeDurationInSeconds)
		{
			_upgradeDurationInSeconds = upgradeDurationInSeconds;
		}
		
		public CFactoryLevelConfigBuilder AddUpgradeRequirement(IUpgradeRequirement requirement)
		{
			_upgradeRequirements.Add(requirement);
			return this;
		}
		
		public CFactoryLevelConfigBuilder AddReward(IUpgradeReward reward)
		{
			_rewards.Add(reward);
			return this;
		}
		
		public CFactoryLevelConfig Build()
		{
			return new CFactoryLevelConfig(
				_rewards.ToArray(), 
				_upgradeRequirements.ToArray(), 
				_upgradeDurationInSeconds
			);
		}
	}
}