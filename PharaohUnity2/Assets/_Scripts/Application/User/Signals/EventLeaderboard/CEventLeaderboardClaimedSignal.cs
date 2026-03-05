// =========================================
// AUTHOR: Marek Karaba
// DATE:   31.01.2026
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CEventLeaderboardClaimedSignal : IEventBusSignal
	{
		public CLeaderboard Leaderboard { get; }

		public CEventLeaderboardClaimedSignal(CLeaderboard leaderboard)
		{
			Leaderboard = leaderboard;
		}
	}
}