// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using ServerData.Design;

namespace ServerData
{
	public class CMainCityConfigs
	{
		private readonly List<CMainCityLevelConfig> _levelConfigs = new();
		
		public CMainCityLevelConfig GetLevelConfig(int level)
		{
			return _levelConfigs[level - 1];
		}
		
		public int GetMaxLevel()
		{
			return _levelConfigs.Count;
		}

		public int GetMaxLevel(ERegion region)
		{
			return _levelConfigs.Count(config => config.Region <= region);
		}

		public int GetStatValueAtLevel(ECityStat stat, int level)
		{
			int result = 0;
			for (int i = 0; i < level; i++)
			{
				CMainCityLevelConfig levelConfig = _levelConfigs[i];
				result += levelConfig.GetStatUpgradeValue(stat);
			}

			return result;
		}

		public int GetMaxStatValue(ECityStat stat, ERegion region)
		{
			int maxLevel = GetMaxLevel(region);
			return GetStatValueAtLevel(stat, maxLevel);
		}

		public long GetUpgradeDuration(int level)
		{
			return GetLevelConfig(level).UpgradeTimeInMs;
		}
		
		public SRechargerLevelConfig GetPassengersGeneratorConfig(int level, int passengersFromBuildings)
		{
			int max = GetStatValueAtLevel(ECityStat.MaxPassengers, level) + passengersFromBuildings;
			int productionPerTick = CDesignMainCityConfigs.CalculatePassengerRatePerTick(max, passengersFromBuildings);
			return new SRechargerLevelConfig(
				max, 
				productionPerTick, 
				CDesignMainCityConfigs.PassengersReplacementTickTimeInSeconds
			);
		}

		protected void BuildLevel(CCityLevelConfigBuilder builder)
		{
			CMainCityLevelConfig config = builder.Build();
			_levelConfigs.Add(config);
		}

		protected CCityLevelConfigBuilder GetNewBuilder(ERegion region, int upgradeTimeInSeconds)
		{
			return new CCityLevelConfigBuilder(region, upgradeTimeInSeconds);
		}
	}
}