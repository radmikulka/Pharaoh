// =========================================
// AUTHOR: Marek Karaba
// DATE:   23.01.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CEventPassRewardClaimedSignal : IEventBusSignal
	{
		public ELiveEvent LiveEventId { get; }
		public int RewardIndex { get; }
		public bool IsPremium { get; }
		public bool IsDoubled { get; }
		public IValuable Reward { get; }
		public EBattlePassPremiumStatus Status { get; }

		public CEventPassRewardClaimedSignal(
			ELiveEvent liveEventId,
			int rewardIndex,
			bool isPremium,
			bool isDoubled,
			IValuable reward,
			EBattlePassPremiumStatus status)
		{
			LiveEventId = liveEventId;
			RewardIndex = rewardIndex;
			IsPremium = isPremium;
			IsDoubled = isDoubled;
			Reward = reward;
			Status = status;
		}
	}
}