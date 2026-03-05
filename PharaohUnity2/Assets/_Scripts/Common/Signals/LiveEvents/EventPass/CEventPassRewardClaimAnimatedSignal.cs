// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.12.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CEventPassRewardClaimAnimatedSignal : IEventBusSignal
	{
		public ELiveEvent LiveEventId { get; }
		public int RewardIndex { get; }
		public bool IsPremium { get; }

		public CEventPassRewardClaimAnimatedSignal(ELiveEvent liveEventId, int rewardIndex, bool isPremium)
		{
			RewardIndex = rewardIndex;
			IsPremium = isPremium;
		}
	}
}