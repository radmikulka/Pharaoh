// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.01.2026
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CLeaderboardComplementDto : IMapAble
	{
		[JsonProperty] public string LeaderboardUid { get; set; }
		[JsonProperty] public CLeaderboardUserDto[] ValuableModifications { get; set; }

		public CLeaderboardComplementDto()
		{
		}

		public CLeaderboardComplementDto(string leaderboardUid, CLeaderboardUserDto[] valuableModifications)
		{
			LeaderboardUid = leaderboardUid;
			ValuableModifications = valuableModifications;
		}
	}
}