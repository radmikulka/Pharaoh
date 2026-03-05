// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CDecadePassRewardClaimedSignal : IEventBusSignal
	{
		public int RewardIndex { get; }
		public bool IsPremium { get; }
		public bool IsDoubled { get; }
		public IValuable Reward { get; }

		public CDecadePassRewardClaimedSignal(int rewardIndex, bool isPremium, bool isDoubled, IValuable reward)
		{
			RewardIndex = rewardIndex;
			IsPremium = isPremium;
			IsDoubled = isDoubled;
			Reward = reward;
		}
	}
}