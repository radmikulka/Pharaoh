// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.01.2026
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CLiveEventLeaderboardDto : CLeaderboardDto
	{
		[JsonProperty] public ELiveEvent LiveEvent { get; set; }
		[JsonProperty] public int PhaseIndex { get; set; }

		public CLiveEventLeaderboardDto()
		{
		}

		public CLiveEventLeaderboardDto(
			CLeaderboardUserDto[] competitors, 
			CLeaderboardRewardDto[] rewards, 
			string leaderboardUid, 
			ELiveEvent liveEvent,
			long endTime,
			int phaseIndex
		) : base(competitors, rewards, leaderboardUid, endTime)
		{
			LiveEvent = liveEvent;
			PhaseIndex = phaseIndex;
		}
	}
}