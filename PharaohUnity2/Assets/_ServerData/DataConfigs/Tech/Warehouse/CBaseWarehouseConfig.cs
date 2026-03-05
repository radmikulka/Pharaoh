// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.08.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using ServerData.Design;

namespace ServerData
{
	public class CBaseWarehouseConfig
	{
		private readonly List<CWarehouseLevel> _warehouseLevels = new();
		
		public CWarehouseLevel GetLevel(int level)
		{
			return _warehouseLevels[level - 1];
		}
		
		public int GetWarehouseCapacity(int level)
		{
			int result = 0;
			for (int i = 0; i < level; i++)
			{
				result += _warehouseLevels[i].CapacityIncrease;
			}

			return result;
		}
		
		public long GetUpgradeDuration(int level)
		{
			return _warehouseLevels[level - 1].UpgradeTimeInMs;
		}
		
		protected CWarehouseLevel AddWarehouseLevel(int capacityIncrease, long upgradeTime)
		{
			CWarehouseLevel newLevel = new (capacityIncrease, upgradeTime);
			_warehouseLevels.Add(newLevel);
			return newLevel;
		}
		
		public int GetMaxLevel()
		{
			return _warehouseLevels.Count;
		}
	}
}