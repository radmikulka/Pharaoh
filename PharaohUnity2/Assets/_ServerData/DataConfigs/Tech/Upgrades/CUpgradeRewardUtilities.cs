// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public static class CUpgradeRewardUtilities
	{
		public static T GetUpgradeRewardOrDefault<T>(this IEnumerable<IUpgradeReward> rewards) where T : class, IUpgradeReward
		{
			foreach (IUpgradeReward reward in rewards)
			{
				if (reward is T matchedReward)
					return matchedReward;
			}
			return null;
		}
	}
}