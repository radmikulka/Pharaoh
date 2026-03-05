// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData.Design;

namespace ServerData
{
	public class CFuelStationConfig
	{
		private readonly List<CUpgradeLevelConfig> _orderedLevels = new();
		public int MaxLevel => _orderedLevels.Count;

		public CUpgradeLevelConfig GetLevel(int level)
		{
			return _orderedLevels[level - 1];
		}
		
		public int GetCapacity(int level)
		{
			int result = 0;
			for (int i = 0; i < level; i++)
			{
				CUpgradeLevelConfig levelConfigData = _orderedLevels[i];
				int capacityIncrease = levelConfigData.GetFuelCapacityIncrease();
				result += capacityIncrease;
			}
			return result;
		}
		
		public int GetProduction(int level)
		{
			int result = 0;
			for (int i = 0; i < level; i++)
			{
				CUpgradeLevelConfig levelConfigData = _orderedLevels[i];
				int capacityIncrease = levelConfigData.GetFuelProductionIncrease();
				result += capacityIncrease;
			}
			return result;
		}

		public long GetUpgradeDuration(int level)
		{
			return _orderedLevels[level - 1].UpgradeDurationInMilliseconds;
		}
		
		public SRechargerLevelConfig GetRechargerConfig(int level)
		{
			int production = GetProduction(level);
			int capacity = GetCapacity(level);
			return new SRechargerLevelConfig(capacity, production, CDesignFuelStationConfig.ProductionIntervalInSeconds);
		}

		protected void AddLevel(CUpgradeLevelConfig levelConfig)
		{
			_orderedLevels.Add(levelConfig);
		}
	}
}