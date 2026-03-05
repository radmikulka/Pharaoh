// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.12.2025
// =========================================

using System.Collections.Generic;
using ServerData;

namespace TycoonBuilder
{
	public class CBaseBattlePassData
	{
		private readonly HashSet<int> _claimedFreeIndexes = new();
		private readonly HashSet<int> _claimedPremiumIndexes = new();
	
		public EBattlePassPremiumStatus PremiumStatus { get; set; }
		public IReadOnlyCollection<int> ClaimedFreeIndexes => _claimedFreeIndexes;
		public IReadOnlyCollection<int> ClaimedPremiumIndexes => _claimedPremiumIndexes;

		public CBaseBattlePassData()
		{
		}

		public CBaseBattlePassData(EBattlePassPremiumStatus premiumStatus, 
			int[] claimedFreeIndexes, 
			int[] claimedPremiumIndexes
		)
		{
			PremiumStatus = premiumStatus;
			_claimedFreeIndexes.UnionWith(claimedFreeIndexes);
			_claimedPremiumIndexes.UnionWith(claimedPremiumIndexes);
		}

		public void AddClaimedFreeIndex(int index)
		{
			_claimedFreeIndexes.Add(index);
		}

		public void AddClaimedPremiumIndex(int index)
		{
			_claimedPremiumIndexes.Add(index);
		}

		public bool IsFreeIndexClaimed(int index)
		{
			return _claimedFreeIndexes.Contains(index);
		}

		public bool IsPremiumIndexClaimed(int index)
		{
			return _claimedPremiumIndexes.Contains(index);
		}

		public bool CanClaimIndex(bool isPremium)
		{
			return !isPremium || PremiumStatus != EBattlePassPremiumStatus.Free;
		}
		
		public bool IsRewardClaimed(int index, bool isPremium)
		{
			return isPremium ? _claimedPremiumIndexes.Contains(index) : _claimedFreeIndexes.Contains(index);
		}

		public void UpdatePremiumPassStatus(EBattlePassPremiumStatus status)
		{
			PremiumStatus = status;
		}

		public bool AllFreeRewardsClaimed(int rewardsCount)
		{
			for (int i = 0; i < rewardsCount; i++)
			{
				if (!_claimedFreeIndexes.Contains(i))
				{
					return false;
				}
			}
			return true;
		}
	}
}