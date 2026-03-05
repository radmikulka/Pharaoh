// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.12.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CBattlePassContent
	{
		public readonly CBattlePassReward[] FreeRewards;
		public readonly CBattlePassReward[] PremiumRewards;
		public readonly int[] PointsRequiredForTier;
		public readonly COffer ExtraPremiumOffer;
		public readonly COffer PremiumOffer;
		public readonly IValuable BonusBankReward;
		public readonly int PointsNeededForBonusBankTier;
		public readonly int BonusBankRewardsCount;
		public int ClaimedBonusBanksCount { get; private set; }

		public CBattlePassContent(
			CBattlePassReward[] freeRewards, 
			CBattlePassReward[] premiumRewards, 
			int[] pointsRequiredForTier, 
			COffer extraPremiumOffer, 
			COffer premiumOffer,
			int claimedBonusBanksCount,
			IValuable bonusBankReward,
			int pointsNeededForBonusBankTier,
			int bonusBankRewardsCount
			)
		{
			FreeRewards = freeRewards;
			PremiumRewards = premiumRewards;
			PointsRequiredForTier = pointsRequiredForTier;
			ExtraPremiumOffer = extraPremiumOffer;
			PremiumOffer = premiumOffer;
			ClaimedBonusBanksCount = claimedBonusBanksCount;
			BonusBankReward = bonusBankReward;
			PointsNeededForBonusBankTier = pointsNeededForBonusBankTier;
			BonusBankRewardsCount = bonusBankRewardsCount;
		}

		internal int GetMaxClaimableIndex(int ownedPoints)
		{
			int tierIndex = -1;
			for (int i = 0; i < PointsRequiredForTier.Length; i++)
			{
				if (ownedPoints >= PointsRequiredForTier[i])
				{
					tierIndex = i;
					ownedPoints -= PointsRequiredForTier[i];
				}
				else
				{
					break;
				}
			}
			return tierIndex;
		}

		internal void IncreaseClaimedRewardsCount(int count)
		{
			ClaimedBonusBanksCount += count;
		}

		internal bool CanFreeRewardBeDoubled(int currentIndex)
		{
			return FreeRewards[currentIndex].CanBeDoubled;
		}

		internal IValuable GetRewardForIndex(int currentIndex, bool isPremium)
		{
			return isPremium ? PremiumRewards[currentIndex].Reward : FreeRewards[currentIndex].Reward;
		}

		public int GetTotalRequiredPointsForReward(int currentIndex)
		{
			int totalPoints = 0;
			for (int i = 0; i <= currentIndex; i++)
			{
				totalPoints += PointsRequiredForTier[i];
			}
			return totalPoints;
		}

		public int GetRequiredPointsForReward(int index)
		{
			if (index >= PointsRequiredForTier.Length)
				return -1;
			
			return PointsRequiredForTier[index];
		}

		public CBattlePassReward[] GetPropagatedRewards()
		{
			List<CBattlePassReward> propagatedRewards = FreeRewards.Where(reward => reward.IsPropagated).ToList();
			propagatedRewards.AddRange(PremiumRewards.Where(reward => reward.IsPropagated));
			return propagatedRewards.ToArray();
		}

		public bool IsAnyRewardClaimable(int eventPoints, CBaseBattlePassData data)
		{
			for (int i = 0; i < FreeRewards.Length; i++)
			{
				bool rewardClaimed = data.IsRewardClaimed(i, false);
				bool isClaimable = eventPoints >= GetTotalRequiredPointsForReward(i);
				if (isClaimable && !rewardClaimed)
					return true;
			}

			bool premiumActive = data.PremiumStatus != EBattlePassPremiumStatus.Free;
			if (!premiumActive) 
				return false;
			
			for (int i = 0; i < PremiumRewards.Length; i++)
			{
				bool rewardClaimed = data.IsRewardClaimed(i, true);
				bool isClaimable = eventPoints >= GetTotalRequiredPointsForReward(i);
				if (isClaimable && !rewardClaimed)
					return true;
			}
			return false;
		}

		public bool AllBonusBankRewardsClaimed()
		{
			return ClaimedBonusBanksCount >= BonusBankRewardsCount;
		}
	}
}