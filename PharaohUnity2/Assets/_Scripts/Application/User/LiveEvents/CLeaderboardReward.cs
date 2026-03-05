// =========================================
// AUTHOR: Marek Karaba
// DATE:   16.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CLeaderboardReward
	{
		public int MinRank { get; set; }
		public IValuable[] Rewards { get; set; }

		public CLeaderboardReward(int minRank, IValuable[] rewards)
		{
			MinRank = minRank;
			Rewards = rewards;
		}
	}
}