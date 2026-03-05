// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.12.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public interface IEventWithBattlePass : ILiveEventContent
	{
		int EventPoints { get; }
		void ModifyEventPoints(int value);
		bool HavePoints(int valueToTest);
		COffer PremiumEventPassOffer { get; }
		COffer ExtraPremiumEventPassOffer { get; }
		CBaseBattlePassData BattlePassData { get; }
		int LastSeenEventPoints { get; }
		void UpdateBattlePassPremiumPassStatus(EBattlePassPremiumStatus status);
		void ClaimBattlePassReward(int index, bool isPremium, bool isDouble);
		bool IsEventPassCompleted(int ownedPoints);
		float CalculateBattlePassRewardProgress(int i, int ownedPoints);
		int GetMaxClaimableBattlePassIndex(int ownedPoints);
		int GetBattlePassRewardsCount();
		int GetRequiredPointsForBattlePassReward(int nextRewardIndex);
		bool IsLastBattlePassReward(int maxClaimableIndex);
		int GetTotalRequiredPointsForBattlePassReward(int maxClaimableIndex);
		IValuable GetRewardForIndex(int currentIndex, bool isPremium);
		bool CanClaimIndex(int currentIndex, bool isPremium, int ownedPoints);
		bool IsRewardClaimed(int currentIndex, bool isPremium);
		bool CanFreeRewardBeDoubled(int currentIndex);
		void SetLastSeenEventPoints(int points);
		CBattlePassReward[] GetPropagatedRewards();
		bool IsAnyRewardClaimable();
	}
}