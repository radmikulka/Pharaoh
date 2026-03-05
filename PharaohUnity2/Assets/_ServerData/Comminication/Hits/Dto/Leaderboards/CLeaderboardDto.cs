// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.07.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CLeaderboardDto : ILeaderboardDto
	{
		[JsonProperty] public CLeaderboardUserDto[] Competitors { get; set; }
		[JsonProperty] public CLeaderboardRewardDto[] Rewards { get; set; }
		[JsonProperty] public long EndTime { get; set; }
		[JsonProperty] public string Uid { get; set; }

		public CLeaderboardDto()
		{
		}

		public CLeaderboardDto(CLeaderboardUserDto[] competitors, CLeaderboardRewardDto[] rewards, string leaderboardUid, long endTime)
		{
			Competitors = competitors;
			Uid = leaderboardUid;
			EndTime = endTime;
			Rewards = rewards;
		}
	}
}