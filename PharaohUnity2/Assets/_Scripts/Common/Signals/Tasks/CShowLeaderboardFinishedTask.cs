// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CShowLeaderboardFinishedTask
	{
		public ELiveEvent LiveEventId { get; }
		public int Rank { get; }
		public int PointsOnRank { get; }
		public IValuable[] Rewards { get; }

		public CShowLeaderboardFinishedTask(ELiveEvent liveEventId, int rank, int pointsOnRank, IValuable[] rewards)
		{
			LiveEventId = liveEventId;
			Rank = rank;
			PointsOnRank = pointsOnRank;
			Rewards = rewards;
		}
	}
}