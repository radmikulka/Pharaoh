// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.01.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CSmallEventContentDto : CStandardEventContentDto
	{
		[JsonProperty] public EMovementType ResourceIndustryMovementType { get; private set; }
		
		public CSmallEventContentDto()
		{
		}

		public CSmallEventContentDto(
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
			EMovementType resourceIndustryMovementType
		) : base(contracts, completedContracts, battlePassContent, battlePassData, leaderboard, eventCoins, eventPoints, lastSeenPoints, lastSeenRank, totalContractsCount, introSeen)
		{
			ResourceIndustryMovementType = resourceIndustryMovementType;
		}
	}
}