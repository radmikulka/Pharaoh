// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CLevelData
	{
		public int Level { get; private set; }
		public long? UpgradeStartTime { get; private set; }

		public static CLevelData New()
		{
			return new CLevelData(1, null);
		}

		public CLevelData(int level, long? upgradeStartTime)
		{
			Level = level;
			UpgradeStartTime = upgradeStartTime;
		}

		public void StartUpgrade(long time)
		{
			UpgradeStartTime = time;
		}

		public bool IsCompleted(long currentTime, long upgradeDuration)
		{
			if (!UpgradeStartTime.HasValue)
				return false;
			long completionTime = UpgradeStartTime.Value + upgradeDuration;
			return currentTime >= completionTime;
		}

		public void FinishUpgrade()
		{
			UpgradeStartTime = null;
			++Level;
		}

		public bool IsUpgradeRunning()
		{
			return UpgradeStartTime.HasValue;
		}
		
		public static bool CanAffordRequirements(IReadOnlyList<IValuable> valuables, CUser user)
		{
			for(int i = 0; i < valuables.Count; i++)
			{
				if (!user.OwnedValuables.HaveValuable(valuables[i]))
					return false;
			}
			
			return true;
		}

		public static bool CanAffordRequirements(IReadOnlyList<IUpgradeRequirement> requirements, CUser user)
		{
			for (int i = 0; i < requirements.Count; i++)
			{
				if (!CanAffordRequirement(requirements[i], user))
					return false;
			}
			
			return true;
		}

		private static bool CanAffordRequirement(IUpgradeRequirement requirement, CUser user)
		{
			return requirement switch
			{
				CValuableRequirement valuableRequirement => user.OwnedValuables.HaveValuable(valuableRequirement.Valuable),
				CYearMilestoneRequirement cWarehouseYearRequirement => user.Progress.Year >= cWarehouseYearRequirement.Year,
				_ => throw new ArgumentOutOfRangeException(nameof(requirement))
			};
		}

		public long GetRemainingUpgradeTime(long timestampInMs, long upgradeDuration)
		{
			if (!UpgradeStartTime.HasValue)
				return 0;

			long timeElapsed = timestampInMs - UpgradeStartTime.Value;
			long timeRemaining = upgradeDuration - timeElapsed;
			
			return CMath.Max(0, timeRemaining);
		}
	}
}