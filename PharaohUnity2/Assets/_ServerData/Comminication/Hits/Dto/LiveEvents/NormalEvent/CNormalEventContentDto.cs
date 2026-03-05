// =========================================
// AUTHOR: Radek Mikulka
// DATE:   25.07.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CNormalEventContentDto : CStandardEventContentDto
	{
		[JsonProperty] public ERegion Region { get; set; }

		public CNormalEventContentDto()
		{
		}

		public CNormalEventContentDto(
			CContractDto[] contracts,
			EStaticContractId[] completedContracts,
			CBattlePassContentDto battlePassContent,
			CBattlePassDataDto battlePassData,
			CLeaderboardDto leaderboard,
			int eventCoins,
			int eventPoints,
			int lastSeenPoints,
			int lastSeenRank,
			int totalContractsCount,
			bool introSeen,
			ERegion region
			) : base(contracts, completedContracts, battlePassContent, battlePassData, leaderboard, eventCoins, eventPoints, lastSeenPoints, lastSeenRank, totalContractsCount, introSeen)
		{
			Region = region;
		}
	}
}