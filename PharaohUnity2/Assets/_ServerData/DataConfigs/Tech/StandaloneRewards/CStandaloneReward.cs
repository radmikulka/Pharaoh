// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.04.2024
// =========================================

namespace ServerData
{
	public class CStandaloneReward
	{
		public readonly EStandaloneRewardId RewardId;
		public readonly IValuable Reward;

		public CStandaloneReward(EStandaloneRewardId rewardId, IValuable reward)
		{
			RewardId = rewardId;
			Reward = reward;
		}
	}
}