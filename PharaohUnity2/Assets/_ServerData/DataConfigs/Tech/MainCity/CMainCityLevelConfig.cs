// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace ServerData
{
	public class CMainCityLevelConfig
	{
		private readonly IUpgradeRequirement[] _upgradeRequirements;
		private readonly CCityStat[] _statIncreaseValues;

		public IReadOnlyList<IUpgradeRequirement> UpgradeRequirements => _upgradeRequirements;
		public readonly int UpgradeTimeInSeconds;
		public readonly ERegion Region;
		
		public long UpgradeTimeInMs => UpgradeTimeInSeconds * CTimeConst.Second.InMilliseconds;

		public CMainCityLevelConfig(
			IUpgradeRequirement[] upgradeRequirements, 
			CCityStat[] statIncreaseValues, 
			int upgradeTimeInSeconds,
			ERegion region
		)
		{
			_upgradeRequirements = upgradeRequirements;
			UpgradeTimeInSeconds = upgradeTimeInSeconds;
			_statIncreaseValues = statIncreaseValues;
			Region = region;
		}

		public int GetStatUpgradeValue(ECityStat stat)
		{
			foreach (CCityStat cityStat in _statIncreaseValues)
			{
				if (cityStat.Stat == stat)
				{
					return cityStat.Value;
				}
			}

			return 0;
		}

		public bool HaveNewBuildingPlot()
		{
			foreach (CCityStat increaseValue in _statIncreaseValues)
			{
				if (increaseValue.Stat == ECityStat.BuildingPlots)
					return true;
			}

			return false;
		}
	}
}