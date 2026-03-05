// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.07.2024
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CLeaderboardRewardDto : IMapAble
	{
		[JsonProperty] public int MinRank { get; set; }
		[JsonProperty] public CValuableDto[] Rewards { get; set; }

		public CLeaderboardRewardDto()
		{
		}

		public CLeaderboardRewardDto(int minRank, CValuableDto[] rewards)
		{
			MinRank = minRank;
			Rewards = rewards;
		}
	}
}