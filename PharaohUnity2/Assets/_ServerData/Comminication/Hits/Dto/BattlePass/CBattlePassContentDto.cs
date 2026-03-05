// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.12.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData;
using ServerData.Dto;

namespace ServerData
{
	public class CBattlePassContentDto : IMapAble
	{
		[JsonProperty] public CBattlePassRewardTemplateDto[] FreeRewards { get; private set; }
		[JsonProperty] public CBattlePassRewardTemplateDto[] PremiumRewards { get; private set; }
		[JsonProperty] public int[] PointsRequiredForTier { get; private set; }
		[JsonProperty] public COfferDto ExtraPremiumOffer { get; private set; }
		[JsonProperty] public COfferDto ExtraPremiumUpgradeOffer { get; private set; }
		[JsonProperty] public COfferDto PremiumOffer { get; private set; }
		[JsonProperty] public int ClaimedBonusBanksCount { get; private set; }
		[JsonProperty] public CValuableDto BonusBankReward { get; private set; }
		[JsonProperty] public int PointsNeededForBonusBankTier { get; private set; }
		[JsonProperty] public int BonusBankRewardsCount { get; private set; }

		public CBattlePassContentDto(
			CBattlePassRewardTemplateDto[] freeRewards,
			CBattlePassRewardTemplateDto[] premiumRewards,
			int[] pointsRequiredForTier,
			COfferDto premiumOffer,
			COfferDto extraPremiumOffer,
			COfferDto extraPremiumUpgradeOffer,
			int claimedBonusBanksCount,
			CValuableDto bonusBankReward,
			int pointsNeededForBonusBankTier,
			int bonusBankRewardsCount
			)
		{
			PointsNeededForBonusBankTier = pointsNeededForBonusBankTier;
			ExtraPremiumUpgradeOffer = extraPremiumUpgradeOffer;
			ClaimedBonusBanksCount = claimedBonusBanksCount;
			PointsRequiredForTier = pointsRequiredForTier;
			BonusBankRewardsCount = bonusBankRewardsCount;
			ExtraPremiumOffer = extraPremiumOffer;
			BonusBankReward = bonusBankReward;
			PremiumRewards = premiumRewards;
			PremiumOffer = premiumOffer;
			FreeRewards = freeRewards;
		}
	}
}