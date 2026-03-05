// =========================================
// AUTHOR: Marek Karaba
// DATE:   29.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;
using ServerData.Hits;
using UnityEngine;

namespace TycoonBuilder
{
	public class CDecadePass : CBaseUserComponent, IInitializable
	{
		private readonly CDesignProgressConfig _progressConfig;
		private readonly CDesignRegionConfigs _regionConfigs;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;

		private CBaseBattlePassData _data;

		public EBattlePassPremiumStatus PremiumStatus => _data.PremiumStatus;

		public CDecadePass(
			CDesignProgressConfig progressConfig,
			CDesignRegionConfigs regionConfigs,
			CHitBuilder hitBuilder,
			IEventBus eventBus, 
			IMapper mapper
			)
		{
			_progressConfig = progressConfig;
			_regionConfigs = regionConfigs;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_mapper = mapper;
		}

		public void InitialSync(CBattlePassDataDto dto)
		{
			_data = _mapper.Map<CBattlePassDataDto, CBaseBattlePassData>(dto);
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CDecadePassPremiumPurchasedSignal>(OnPremiumPurchased);
			_eventBus.Subscribe<CRegionIncreasedSignal>(OnRegionIncreased);
		}

		private void OnRegionIncreased(CRegionIncreasedSignal signal)
		{
			_data = new CBaseBattlePassData(EBattlePassPremiumStatus.Free, Array.Empty<int>(), Array.Empty<int>());
		}

		private void OnPremiumPurchased(CDecadePassPremiumPurchasedSignal signal)
		{
			_data.PremiumStatus = signal.PremiumStatus;
			_eventBus.Send(new CDecadePassActivatedSignal());
		}

		public int GetMaxClaimableIndex()
		{
			return User.Progress.Year == EYearMilestone.None ? 0 : GetMaxClaimableIndex(User.Progress.Year, User.Progress.XpInCurrentYear);
		}
		
		public int GetMaxClaimableIndex(EYearMilestone currentYear, int ownedXp)
		{
			CDecadePassConfigData passConfig = GetActivePassConfig();
			EYearMilestone nextYear = _progressConfig.GetNextYear(currentYear);
		
			int xpInCurrentYear = _progressConfig.GetXpInYear(currentYear);
			int tiersInYear;
			
			if (currentYear > passConfig.YearMilestones[^1].Milestone)
			{
				int lastRewardIndex = passConfig.FreeRewards.Count;
				return lastRewardIndex;
			}
		
			int yearStartIndex = passConfig.GetYearStartIndex(currentYear);
			int yearEndIndex = passConfig.GetYearStartIndex(nextYear);
			if (yearEndIndex == -1)
			{
				int lastRewardIndex = passConfig.FreeRewards.Count;
				tiersInYear = lastRewardIndex - yearStartIndex;
			}
			else
			{
				tiersInYear = yearEndIndex - yearStartIndex;
			}
			int xpPerTier = xpInCurrentYear / tiersInYear;
			int currentTier = yearStartIndex + ownedXp / xpPerTier;
			return currentTier;
		}

		public int GetMaxRewardIndex()
		{
			CDecadePassConfigData passConfig = GetActivePassConfig();
			return passConfig.FreeRewards.Count;
		}

		public int GetMaxXpInCurrentTier(EYearMilestone currentYear)
		{
			CDecadePassConfigData passConfig = GetActivePassConfig();	
			EYearMilestone nextYear = _progressConfig.GetNextYear(currentYear);
		
			int xpInCurrentYear = _progressConfig.GetXpInYear(currentYear);
			int yearStartIndex = passConfig.GetYearStartIndex(currentYear);
			int yearEndIndex = GetYearEndIndex(passConfig, nextYear);
			int tiersInYear = yearEndIndex - yearStartIndex;
			return xpInCurrentYear / tiersInYear;
		}

		public int GetCurrentXpInCurrentTier()
		{
			CDecadePassConfigData passConfig = GetActivePassConfig();
			EYearMilestone currentYear = User.Progress.Year;
			EYearMilestone nextYear = _progressConfig.GetNextYear(currentYear);
			int yearStartIndex = passConfig.GetYearStartIndex(currentYear);
			int yearEndIndex = GetYearEndIndex(passConfig, nextYear);
			int tiersInYear = yearEndIndex - yearStartIndex;
		
			int xpInCurrentYear = _progressConfig.GetXpInYear(currentYear);
			int xpPerTier = xpInCurrentYear / tiersInYear;
		
			int ownedXp = User.Progress.XpInCurrentYear;
			return ownedXp % xpPerTier;
		}

		private int GetYearEndIndex(CDecadePassConfigData passConfig, EYearMilestone nextYear)
		{
			int yearEndIndex = passConfig.GetYearStartIndex(nextYear);
			if (yearEndIndex != -1) 
				return yearEndIndex;
			
			int lastRewardIndex = passConfig.FreeRewards.Count;
			yearEndIndex = lastRewardIndex;
			return yearEndIndex;
		}

		public bool CanFreeRewardBeDoubled(int index)
		{
			CDecadePassConfigData passConfig = GetActivePassConfig();
			CBattlePassReward reward = passConfig.FreeRewards[index];
			return reward.CanBeDoubled;
		}

		public bool IsMilestoneIndex(int index)
		{
			CDecadePassConfigData config = GetActivePassConfig();
			return config.YearMilestones.Any(decadePassYear => decadePassYear.Index == index);
		}

		private CDecadePassConfigData GetActivePassConfig()
		{
			CRegionConfig regionConfig = _regionConfigs.GetRegionConfig(User.Progress.Region);
			return regionConfig.GetDecadePass();
		}

		public bool CanClaimIndex(int index, bool isPremium, EYearMilestone currentYear, int ownedXp)
		{
			bool isClaimed = IsIndexClaimed(index, isPremium);
			if (isClaimed)
				return false;

			bool canClaimPremium = PremiumStatus is EBattlePassPremiumStatus.Premium or EBattlePassPremiumStatus.ExtraPremium;
			if (isPremium && !canClaimPremium)
				return false;
			
			int maxIndex = GetMaxClaimableIndex(currentYear, ownedXp);
			return index <= maxIndex;
		}

		public IValuable GetRewardForIndex(int index, bool isPremium)
		{
			CDecadePassConfigData passConfig = GetActivePassConfig();
			bool isIndexOutOfRange = index < 0 || index >= passConfig.FreeRewards.Count;
			if (isIndexOutOfRange)
				return new CNullValuable();
			
			return isPremium ? passConfig.PremiumRewards[index].Reward : passConfig.FreeRewards[index].Reward;
		}

		public bool IsIndexClaimed(int index, bool isPremium)
		{
			return isPremium ? _data.IsPremiumIndexClaimed(index) : _data.IsFreeIndexClaimed(index);
		}

		public int GetRewardIndex(int index)
		{
			int rewardIndex = 0;
			for (int i = 0; i <= index; i++)
			{
				bool isMilestoneIndex = IsMilestoneIndex(i);
				if (!isMilestoneIndex)
				{
					rewardIndex++;
				}
			}
			return rewardIndex;
		}

		public void ClaimReward(int index, bool isPremium, bool isDouble)
		{
			MarkRewardAsClaimed(index, isPremium);
			SendClaimRequest(index, isPremium, isDouble);
			
			IValuable reward = GetRewardForIndex(index, isPremium);
			_eventBus.Send(new CDecadePassRewardClaimedSignal(index, isPremium, isDouble, reward));
		}

		private void MarkRewardAsClaimed(int index, bool isPremium)
		{
			if (isPremium)
			{
				_data.AddClaimedPremiumIndex(index);
			}
			else
			{
				_data.AddClaimedFreeIndex(index);
			}
		}

		private void SendClaimRequest(int index, bool isPremium, bool isDouble)
		{
			CClaimDecadePassTierRequest request = new(index, isPremium, isDouble);
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(request);
			hit.BuildAndSend();
		}

		public bool AreAllRewardsClaimed(EYearMilestone currentYear, int ownedXp)
		{
			int maxIndex = GetMaxClaimableIndex(currentYear, ownedXp);

			for (int i = 0; i <= maxIndex; i++)
			{
				IValuable reward = GetRewardForIndex(i, false);
				if (reward is CNullValuable)
					continue;
				
				bool isClaimed = IsIndexClaimed(i, false);
				if (!isClaimed)
					return false;
				
				if (PremiumStatus == EBattlePassPremiumStatus.Free)
					continue;
				
				isClaimed = IsIndexClaimed(i, true);
				if (!isClaimed)
					return false;
			}
			return true;
		}
		
		public bool AllFreeRewardsClaimed()
		{
			CDecadePassConfigData config = GetActivePassConfig();
			bool allFreeRewardsClaimed = _data.AllFreeRewardsClaimed(config.FreeRewards.Count);
			return allFreeRewardsClaimed;
		}

		public float CalculateRewardProgress(int currentIndex, EYearMilestone currentYear, int ownedXp)
		{
			const float offset = 0.5f;
			
			int maxClaimableIndex = GetMaxClaimableIndex(currentYear, ownedXp);
			if (currentIndex < maxClaimableIndex)
				return 1f;
			
			float maxXp = GetMaxXpInCurrentTier(User.Progress.Year);
			float currentXp = ownedXp % maxXp;
			float progress = currentXp / maxXp;
			
			if (currentIndex == maxClaimableIndex)
			{
				return CMath.Min(progress + offset, 1f);
			}
			
			if (currentIndex == maxClaimableIndex + 1)
			{
				return CMath.Min(progress - offset, 0.5f);
			}
			return 0f;
		}

		public bool IsIndexUnlocked(int currentIndex, EYearMilestone currentYear, int ownedXp)
		{
			int maxClaimableIndex = GetMaxClaimableIndex(currentYear, ownedXp);
			return currentIndex <= maxClaimableIndex;
		}

		public bool IsLastUnlockedReward(int currentIndex, EYearMilestone currentYear, int ownedXp)
		{
			int maxClaimableIndex = GetMaxClaimableIndex(currentYear, ownedXp);
			return currentIndex == maxClaimableIndex;
		}

		public float CalculateNextDecadeProgress(EYearMilestone yearMilestone, int ownedXp, ERegion region)
		{
			CDecadePassConfigData decadePassConfig = _regionConfigs.GetRegionConfig(region).GetDecadePass();
			EYearMilestone lastYearMilestone = decadePassConfig.YearMilestones.Last().Milestone;
		
			if (yearMilestone < lastYearMilestone)
				return 0f;
		
			if (yearMilestone > lastYearMilestone)
				return 1f;
			
			int nextDecadeIndex = GetYearEndIndex(decadePassConfig, _progressConfig.GetNextYear(yearMilestone));
			int maxClaimableIndex = GetMaxClaimableIndex(yearMilestone, ownedXp);
			
			if (maxClaimableIndex < nextDecadeIndex - 1)
				return 0f;

			int maxXp = GetMaxXpInCurrentTier(lastYearMilestone);
			float currentXp = ownedXp % maxXp;
			float progress = currentXp / maxXp;
			return progress;
		}

		public bool IsNextReward(int currentIndex, EYearMilestone currentYear, int ownedXp)
		{
			int maxClaimableIndex = GetMaxClaimableIndex(currentYear, ownedXp);
			return currentIndex == maxClaimableIndex + 1;
		}
	}
}