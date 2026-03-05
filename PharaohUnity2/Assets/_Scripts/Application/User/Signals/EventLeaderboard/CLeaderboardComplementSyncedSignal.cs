// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.01.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.Offers
{
	public class CLeaderboardComplementSyncedSignal : IEventBusSignal
	{
		public readonly string LeaderboardUid;
		public readonly int LastSeenRank;
		public readonly int NewUserRank;

		public CLeaderboardComplementSyncedSignal(string leaderboardUid, int lastSeenRank, int newUserRank)
		{
			LeaderboardUid = leaderboardUid;
			LastSeenRank = lastSeenRank;
			NewUserRank = newUserRank;
		}
	}
}