// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.01.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CStandardEventContentDto : ILiveEventContentDto
	{
		[JsonProperty] public CContractDto[] Contracts { get; set; }
		[JsonProperty] public EStaticContractId[] CompletedContracts { get; set; }
		[JsonProperty] public CBattlePassContentDto BattlePassContent { get; set; }
		[JsonProperty] public CBattlePassDataDto BattlePassData { get; set; }
		[JsonProperty] public ILeaderboardDto Leaderboard { get; set; }
		[JsonProperty] public int TotalContractsCount { get; set; }
		[JsonProperty] public int EventCoins { get; set; }
		[JsonProperty] public int EventPoints { get; set; }
		[JsonProperty] public int LastSeenPoints { get; set; }
		[JsonProperty] public int LastSeenRank { get; set; }
		[JsonProperty] public bool IntroSeen { get; set; }

		public CStandardEventContentDto()
		{
		}

		public CStandardEventContentDto(
			CContractDto[] contracts, 
			EStaticContractId[] completedContracts, 
			CBattlePassContentDto battlePassContent,
			CBattlePassDataDto battlePassData,
			ILeaderboardDto leaderboard,
			int eventCoins,
			int eventPoints,
			int lastSeenPoints,
			int lastSeenRank,
			int totalContractsCount,
			bool introSeen
		)
		{
			TotalContractsCount = totalContractsCount;
			CompletedContracts = completedContracts;
			BattlePassContent = battlePassContent;
			BattlePassData = battlePassData;
			LastSeenPoints = lastSeenPoints;
			LastSeenRank = lastSeenRank;
			Leaderboard = leaderboard;
			EventPoints = eventPoints;
			EventCoins = eventCoins;
			Contracts = contracts;
			IntroSeen = introSeen;
		}
	}
}