// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.01.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using TycoonBuilder.Offers;
using UnityEngine;

namespace TycoonBuilder
{
	public class CStandardEventContent : CLiveEventContent, 
		IEventWithStore, IEventWithBattlePass, IEventWithLeaderboard, IEventWithContracts
	{
		private readonly Dictionary<EStaticContractId, CContract> _activeStoryContracts = new();
		private readonly HashSet<EStaticContractId> _completedContracts = new();
		private readonly IRewardQueue _rewardQueue;
		private readonly IServerTime _serverTime;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;
		
		public readonly ELiveEvent LiveEventId;

		public CBattlePassContent BattlePassContent { get; private set; }
		public CBaseBattlePassData BattlePassData { get; private set; }
		public CLeaderboard Leaderboard { get; private set; }
		public CLeaderboardComplement LeaderboardComplement { get; private set; }
		public int EventCoins { get; private set; }
		public int EventPoints { get; private set; }
		public int LastSeenEventPoints { get; private set; }
		public int LastSeenRank { get; private set; }
		public int TotalContractsCount { get; private set; }
		public bool IntroSeen { get; private set; }
		public int CompletedContractsCount => _completedContracts.Count;
		public COffer PremiumEventPassOffer => BattlePassContent.PremiumOffer;
		public COffer ExtraPremiumEventPassOffer => BattlePassContent.ExtraPremiumOffer;
		public int LeaderboardPoints => Leaderboard?.GetPointsForUid(User.Account.EncryptedUid) ?? 0;
		public ERegion Region { get; protected set; }

		protected CStandardEventContent(
			IRewardQueue rewardQueue,
			IServerTime serverTime,
			CHitBuilder hitBuilder,
			ELiveEvent liveEventId,
			IEventBus eventBus,
			IMapper mapper,
			CUser user
			) : base(user, liveEventId)
		{
			_rewardQueue = rewardQueue;
			_serverTime = serverTime;
			_hitBuilder = hitBuilder;
			LiveEventId = liveEventId;
			_eventBus = eventBus;
			_mapper = mapper;
		}
		
		public virtual void InitialSync(CStandardEventContentDto dto)
		{
			_completedContracts.UnionWith(dto.CompletedContracts);
			
			foreach (CContractDto contractDto in dto.Contracts)
			{
				CContract contract = _mapper.Map<CContractDto, CContract>(contractDto);
				_activeStoryContracts[contract.StaticData.ContractId] = contract;
			}

			BattlePassContent = _mapper.Map<CBattlePassContentDto, CBattlePassContent>(dto.BattlePassContent);
			BattlePassData = _mapper.Map<CBattlePassDataDto, CBaseBattlePassData>(dto.BattlePassData);
			Leaderboard = _mapper.Map<ILeaderboardDto, CLeaderboard>(dto.Leaderboard);
			TotalContractsCount = dto.TotalContractsCount;
			EventPoints = dto.EventPoints;
			LastSeenEventPoints = dto.LastSeenPoints;
			LastSeenRank = dto.LastSeenRank;
			EventCoins = dto.EventCoins;
			IntroSeen = dto.IntroSeen;
		}

		public void Sync(CLiveEventLeaderboardDto dto)
		{
			Leaderboard = _mapper.Map<ILeaderboardDto, CLeaderboard>(dto);
		}

		public void SyncComplement(CLeaderboardComplementDto dto)
		{
			LeaderboardComplement = _mapper.Map<CLeaderboardComplementDto, CLeaderboardComplement>(dto);

			Leaderboard.UpdateCompetitorsWithComplement(LeaderboardComplement);
			int newUserRank = Leaderboard.GetRankForUid(User.Account.EncryptedUid);
			
			_eventBus.Send(new CLeaderboardComplementSyncedSignal(dto.LeaderboardUid, LastSeenRank, newUserRank));
		}

		public void ClaimLeaderboardRewards(int rank, int pointsOnRank, IValuable[] rewards, IParticleSource[] particleSources)
		{
			EModificationSource source = EModificationSource.ClaimContract;
			_rewardQueue.AddRewardsWithSources(source, rewards, particleSources);
			
			_hitBuilder.GetBuilder(new CClaimEventLeaderboardRequest(LiveEventId, rank, pointsOnRank))
				.SetExecuteImmediately()
				.BuildAndSend();

			Leaderboard = null;
			LeaderboardComplement = null;
			_eventBus.Send(new CLeaderboardRewardClaimedSignal(LiveEventId));
		}

		public void Sync(CContract contract)
		{
			_activeStoryContracts[contract.StaticData.ContractId] = contract;
		}

		private CContract GetContract(EStaticContractId contractId)
		{
			return _activeStoryContracts[contractId];
		}

		public void ModifyEventCoins(int value)
		{
			EventCoins += value;
		}

		public bool HaveCoins(int valueToTest)
		{
			return EventCoins >= valueToTest;
		}

		public void ModifyEventPoints(int value)
		{
			EventPoints += value;
		}

		public bool HavePoints(int valueToTest)
		{
			return EventPoints >= valueToTest;
		}

		public CBattlePassReward[] GetPropagatedRewards()
		{
			return BattlePassContent.GetPropagatedRewards();
		}

		public bool IsAnyRewardClaimable()
		{
			return BattlePassContent.IsAnyRewardClaimable(EventPoints, BattlePassData);
		}

		public IEnumerable<CContract> AllActiveContracts()
		{
			foreach (var contract in _activeStoryContracts)
			{
				yield return contract.Value;
			}
		}

		public CContract GetContractOrDefault(EStaticContractId contractId)
		{
			_activeStoryContracts.TryGetValue(contractId, out CContract contract);
			return contract;
		}

		public bool IsContractCompleted(EStaticContractId contractId)
		{
			return _completedContracts.Contains(contractId);
		}

		public bool ContractExists(EStaticContractId id)
		{
			return _activeStoryContracts.ContainsKey(id);
		}

		public bool IsContractActivated(EStaticContractId id)
		{
			return _activeStoryContracts.TryGetValue(id, out var contract) && contract.StaticData.IsActivated;
		}

		public int GetContractTask(EStaticContractId id)
		{
			return _activeStoryContracts[id].StaticData.Task;
		}

		public float GetDeliveredContractProgress(EStaticContractId id)
		{
			CContract contract = GetContract(id);
			long currentTime = _serverTime.GetTimestampInMs();
			int deliveredAmount = contract.DeliveredAmount;
			foreach (CDispatch dispatch in User.Dispatches.GetDispatchesForContract(id))
			{
				float allowedTimeDiff = 2 * CTimeConst.Minute.InMilliseconds;
				bool arrivedToDestination = dispatch.TripCompletionTime - allowedTimeDiff <= currentTime;
				if (!arrivedToDestination)
				{
					deliveredAmount -= dispatch.ContractData.ResourceAmount;
				}
			}
			float progress = (float)deliveredAmount / contract.Requirement.Amount;
			return progress;
		}

		public void AddCompletedContract(CContract contract)
		{
			if(contract.EventData?.IsInfinity ?? false)
				return;
			_completedContracts.Add(contract.StaticData.ContractId);
		}

		public EResource GetContractRequirement(EStaticContractId id)
		{
			CContract contract = GetContract(id);
			return contract.Requirement.Id;
		}

		public bool RemoveContract(EStaticContractId contractId)
		{
			return _activeStoryContracts.Remove(contractId);
		}

		public void MarkLiveEventIntroAsSeen()
		{
			IntroSeen = true;
		}

		public bool CanClaimIndex(int currentIndex, bool isPremium, int ownedPoints)
		{
			int maxClaimableIndex = BattlePassContent.GetMaxClaimableIndex(ownedPoints);
			bool canBeClaimed = BattlePassData.CanClaimIndex(isPremium);
			return currentIndex <= maxClaimableIndex && canBeClaimed;
		}

		public bool IsRewardClaimed(int index, bool isPremium)
		{
			return BattlePassData.IsRewardClaimed(index, isPremium);
		}

		public bool CanFreeRewardBeDoubled(int currentIndex)
		{
			bool canBeDoubled = BattlePassContent.CanFreeRewardBeDoubled(currentIndex);
			return canBeDoubled;
		}

		public IValuable GetRewardForIndex(int currentIndex, bool isPremium)
		{
			IValuable reward = BattlePassContent.GetRewardForIndex(currentIndex, isPremium);
			return reward;
		}

		public int GetMaxClaimableBattlePassIndex(int ownedXp)
		{
			return BattlePassContent.GetMaxClaimableIndex(ownedXp);
		}

		public int GetTotalRequiredPointsForBattlePassReward(int currentIndex)
		{
			return BattlePassContent.GetTotalRequiredPointsForReward(currentIndex);
		}

		public int GetRequiredPointsForBattlePassReward(int index)
		{
			return BattlePassContent.GetRequiredPointsForReward(index);
		}

		public void ClaimBattlePassReward(int index, bool isPremium, bool isDouble)
		{
			MarkRewardAsClaimed(index, isPremium);
			SendClaimRequest(index, isPremium, isDouble);
			IValuable reward = GetRewardForIndex(index, isPremium);
			_eventBus.Send(new CEventPassRewardClaimedSignal(EventId, index, isPremium, isDouble, reward, BattlePassData.PremiumStatus));
		}

		private void MarkRewardAsClaimed(int index, bool isPremium)
		{
			if (isPremium)
			{
				BattlePassData.AddClaimedPremiumIndex(index);
			}
			else
			{
				BattlePassData.AddClaimedFreeIndex(index);
			}
		}

		private void SendClaimRequest(int index, bool isPremium, bool isDouble)
		{
			CClaimEventPassTierRequest request = new(LiveEventId, index, isPremium, isDouble);
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(request);
			hit.BuildAndSend();
		}
		
		public bool IsLastBattlePassReward(int rewardIndex)
		{
			bool isLastReward = rewardIndex == BattlePassContent.FreeRewards.Length - 1;
			return isLastReward;
		}

		public void SetLastSeenEventPoints(int points)
		{
			LastSeenEventPoints = points;
			
			CSetEventPointsSeenRequest request = new(LiveEventId, points);
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(request);
			hit.BuildAndSend();
		}
		
		public void SetLastSeenRank()
		{
			int newUserRank = Leaderboard.GetRankForUid(User.Account.EncryptedUid);
			LastSeenRank = newUserRank;
			
			CSetEventLastSeenRankRequest request = new(LiveEventId, LastSeenRank);
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(request);
			hit.BuildAndSend();
		}

		public float CalculateBattlePassRewardProgress(int currentIndex, int ownedPoints)
		{
			const float offset = 0.5f;
			
			int maxClaimableIndex = GetMaxClaimableBattlePassIndex(ownedPoints);
			if (currentIndex < maxClaimableIndex)
				return 1f;
			
			if (currentIndex > maxClaimableIndex + 1)
				return 0f;
			
			if (currentIndex == maxClaimableIndex)
			{
				float progress = GetProgress(currentIndex + 1, ownedPoints);
				float finalProgress = offset + progress;
				finalProgress = CMath.Clamp01(finalProgress);
				return finalProgress;
			}

			if (currentIndex == maxClaimableIndex + 1)
			{
				float progress = GetProgress(currentIndex, ownedPoints);
				float finalProgress = progress - offset;
				finalProgress = CMath.Clamp01(finalProgress);
				return finalProgress;
			}
			return 0f;
		}

		private float GetProgress(int currentIndex, int ownedPoints)
		{
			int totalRequiredPoints = GetTotalRequiredPointsForBattlePassReward(currentIndex - 1);
			int neededPointsForNextReward = GetRequiredPointsForBattlePassReward(currentIndex);
			int pointsIntoCurrentTier = ownedPoints - totalRequiredPoints;
			
			float progress = (float)pointsIntoCurrentTier / neededPointsForNextReward;
			progress = CMath.Clamp01(progress);
			return progress;
		}

		public int GetBattlePassRewardsCount()
		{
			return BattlePassContent.FreeRewards.Length;
		}

		public bool IsEventPassCompleted(int ownedPoints)
		{
			int maxIndex = GetMaxClaimableBattlePassIndex(ownedPoints);
			int rewardsCount = GetBattlePassRewardsCount();
			bool isCompleted = maxIndex >= rewardsCount - 1;
			return isCompleted;
		}

		public void UpdateBattlePassPremiumPassStatus(EBattlePassPremiumStatus status)
		{
			BattlePassData.UpdatePremiumPassStatus(status);
			_eventBus.Send(new CEventPassActivatedSignal(LiveEventId));
		}

		public int GetTotalRequiredPointsForLastBattlePassReward()
		{
			int rewardsCount = GetBattlePassRewardsCount();
			int totalRequiredPoints = GetTotalRequiredPointsForBattlePassReward(rewardsCount - 1);
			return totalRequiredPoints;
		}

		public void ClaimBonusBankRewards(IParticleSource source, int rewardCount)
		{
			BattlePassContent.IncreaseClaimedRewardsCount(rewardCount);

			List<IValuable> rewards = new();
			if (BattlePassContent.BonusBankReward is ICountableValuable countableValuable)
			{
				countableValuable = countableValuable.Multiply(rewardCount);
				rewards.Add(countableValuable);
			}
			else
			{
				Debug.LogError("Bonus bank reward is not ICountableValuable");
				return;
			}
			
			_rewardQueue.AddRewardsWithSources(EModificationSource.EventBonusBank, rewards.ToArray(), new []{source});
			
			CClaimEventPassBonusBankRequest request = new(LiveEventId);
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(request);
			hit.BuildAndSend();
		}
	}
}