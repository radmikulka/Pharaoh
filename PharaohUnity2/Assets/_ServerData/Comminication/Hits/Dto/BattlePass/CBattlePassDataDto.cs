// =========================================
// AUTHOR:
// DATE:   29.07.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CBattlePassDataDto : IMapAble
	{
		[JsonProperty] public int[] ClaimedFreeIndexes { get; set; }
		[JsonProperty] public int[] ClaimedPremiumIndexes { get; set; }
		[JsonProperty] public EBattlePassPremiumStatus PremiumStatus { get; set; }

		public CBattlePassDataDto()
		{
		}

		public CBattlePassDataDto(int[] claimedFreeIndexes, int[] claimedPremiumIndexes, EBattlePassPremiumStatus premiumStatus)
		{
			ClaimedFreeIndexes = claimedFreeIndexes;
			ClaimedPremiumIndexes = claimedPremiumIndexes;
			PremiumStatus = premiumStatus;
		}
	}
}

