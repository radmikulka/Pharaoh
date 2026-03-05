// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.01.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.Offers
{
	public class CLeaderboardRewardClaimedSignal : IEventBusSignal
	{
		public readonly ELiveEvent LiveEventId;

		public CLeaderboardRewardClaimedSignal(ELiveEvent liveEventId)
		{
			LiveEventId = liveEventId;
		}
	}
}