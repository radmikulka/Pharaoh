// =========================================
// AUTHOR: Marek Karaba
// DATE:   29.07.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CDecadePassRewardClaimAnimatedSignal : IEventBusSignal
	{
		public int RewardIndex { get; }
		public bool IsPremium { get; }

		public CDecadePassRewardClaimAnimatedSignal(int rewardIndex, bool isPremium)
		{
			RewardIndex = rewardIndex;
			IsPremium = isPremium;
		}
	}
}