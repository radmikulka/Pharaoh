// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.04.2024
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CStandaloneRewardsConfigBase
	{
		private readonly Dictionary<EStandaloneRewardId, CStandaloneReward> _rewardsDb = new();
	
		public IValuable GetReward(EStandaloneRewardId rewardId)
		{
			return _rewardsDb[rewardId].Reward;
		}
	
		protected void AddReward(EStandaloneRewardId rewardId, IValuable reward)
		{
			_rewardsDb.Add(rewardId, new CStandaloneReward(rewardId, reward));
		}
	}
}