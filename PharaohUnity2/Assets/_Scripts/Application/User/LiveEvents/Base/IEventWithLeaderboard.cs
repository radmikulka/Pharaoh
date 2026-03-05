// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.01.2026
// =========================================

using ServerData;
using ServerData.Dto;

namespace TycoonBuilder
{
	public interface IEventWithLeaderboard : ILiveEventContent
	{
		CLeaderboard Leaderboard { get; }
		int LeaderboardPoints { get; }
		void Sync(CLiveEventLeaderboardDto dto);
		void SyncComplement(CLeaderboardComplementDto dto);
		void ClaimLeaderboardRewards(int rank, int pointsOnRank, IValuable[] rewards, IParticleSource[] particleSources);
	}
}