// =========================================
// AUTHOR: Marek Karaba
// DATE:   20.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class COpenLeaderboardRewardsTask
	{
		public ELiveEvent LiveEventId { get; }

		public COpenLeaderboardRewardsTask(ELiveEvent liveEventId)
		{
			LiveEventId = liveEventId;
		}
	}
}