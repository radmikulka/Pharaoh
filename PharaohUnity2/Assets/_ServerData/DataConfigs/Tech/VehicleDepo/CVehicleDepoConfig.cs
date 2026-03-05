// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CVehicleDepoConfig
	{
		private readonly List<CUpgradeLevelConfig> _orderedLevels = new();

		public CUpgradeLevelConfig GetLevel(int level)
		{
			return _orderedLevels[level - 1];
		}
		
		public int GetDurabilityRepairAmount(int level)
		{
			int result = 0;
			for (int i = 0; i < level; i++)
			{
				CUpgradeLevelConfig levelData = _orderedLevels[i];
				int capacityIncrease = levelData.GetDurabilityRepairAmountIncrease();
				result += capacityIncrease;
			}
			return result;
		}

		protected void AddLevel(CUpgradeLevelConfig level)
		{
			_orderedLevels.Add(level);
		}

		public long GetUpgradeDuration(int level)
		{
			return _orderedLevels[level - 1].UpgradeDurationInMilliseconds;
		}
		
		public int GetMaxLevel()
		{
			return _orderedLevels.Count;
		}
	}
}