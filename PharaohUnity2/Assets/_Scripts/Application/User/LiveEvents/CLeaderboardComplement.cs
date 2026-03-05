// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.01.2026
// =========================================

namespace TycoonBuilder
{
	public class CLeaderboardComplement
	{
		public string LeaderboardUid { get; set; }
		public CLeaderboardUser[] ValuableModifications { get; set; }

		public CLeaderboardComplement(string leaderboardUid, CLeaderboardUser[] valuableModifications)
		{
			LeaderboardUid = leaderboardUid;
			ValuableModifications = valuableModifications;
		}
	}
}