// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace ServerData
{
	public class CWarehouseLevel
	{
		private readonly List<IUpgradeRequirement> _requirements = new(2);
		
		public int CapacityIncrease { get; }
		public long UpgradeTimeInMs => UpgradeTimeInSec * CTimeConst.Second.InMilliseconds;
		public long UpgradeTimeInSec { get; }
	
		public IReadOnlyList<IUpgradeRequirement> Requirements => _requirements;
		
		public CWarehouseLevel(int capacityIncrease, long upgradeTimeInSec)
		{
			CapacityIncrease = capacityIncrease;
			UpgradeTimeInSec = upgradeTimeInSec;
		}
		
		public CWarehouseLevel AddRequirement(EYearMilestone year)
		{
			_requirements.Add(IUpgradeRequirement.Year(year));
			return this;
		}
		
		public CWarehouseLevel AddRequirement(IValuable valuable)
		{
			_requirements.Add(IUpgradeRequirement.Valuable(valuable));
			return this;
		}
	}
}