// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CCityLevelConfigBuilder
	{
		private readonly List<IUpgradeRequirement> _upgradeRequirements = new(2);
		private readonly List<CCityStat> _stats = new(3);
		private readonly int _upgradeTimeInSeconds;
		private readonly ERegion _region;

		public CCityLevelConfigBuilder(ERegion region, int upgradeTimeInSeconds)
		{
			_upgradeTimeInSeconds = upgradeTimeInSeconds;
			_region = region;
		}

		public CCityLevelConfigBuilder AddUpgradeRequirement(EYearMilestone year)
		{
			_upgradeRequirements.Add(IUpgradeRequirement.Year(year));
			return this;
		}
		
		public CCityLevelConfigBuilder AddUpgradeRequirement(IValuable valuable)
		{
			_upgradeRequirements.Add(IUpgradeRequirement.Valuable(valuable));
			return this;
		}
		
		public CCityLevelConfigBuilder AddStat(ECityStat stat, int value)
		{
			_stats.Add(new CCityStat(stat, value));
			return this;
		}

		public CMainCityLevelConfig Build()
		{
			return new CMainCityLevelConfig(
				_upgradeRequirements.ToArray(),
				_stats.ToArray(),
				_upgradeTimeInSeconds,
				_region
				);
		}
	}
}