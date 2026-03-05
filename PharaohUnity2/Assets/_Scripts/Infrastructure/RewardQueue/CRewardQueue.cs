// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Infrastructure
{
	public class CRewardQueue : MonoBehaviour, IAldaFrameworkComponent, IRewardQueue
	{
		private readonly Queue<CRewardsWithResourceSources> _rewardsWithSources = new();
		private readonly Queue<COfferWithResourceSources> _offersWithSources = new();
		
		private IRewardHandler _rewardHandler;
		private ICtsProvider _ctsProvider;
		private CUser _user;
		
		private bool _claimingInProgress;
		private bool _currentItemBlocksQueue;

		[Inject]
		private void Inject(IRewardHandler rewardHandler, ICtsProvider ctsProvider, CUser user)
		{
			_rewardHandler = rewardHandler;
			_ctsProvider = ctsProvider;
			_user = user;
		}
		
		private void Update()
		{
			ClaimRewards();
			ClaimOffers();
		}

		private void ClaimRewards()
		{
			if (_rewardsWithSources.Count == 0 || _claimingInProgress)
				return;

			ClaimFirstReward(_ctsProvider.Token).Forget();
		}

		private async UniTask ClaimFirstReward(CancellationToken ct)
		{
			_claimingInProgress = true;
			
			CRewardsWithResourceSources rewardsWithSources = _rewardsWithSources.Dequeue();
			bool shouldBlockQueue = ShouldBlockQueue(rewardsWithSources);
			_currentItemBlocksQueue = shouldBlockQueue;
			await _rewardHandler.ClaimRewards(rewardsWithSources.Rewards, rewardsWithSources.ParticleSources, rewardsWithSources.Source, false, ct, rewardsWithSources.ModifyParams);
			
			_claimingInProgress = false;
			_currentItemBlocksQueue = false;
		}

		private void ClaimOffers()
		{
			if (_offersWithSources.Count == 0 || _claimingInProgress)
				return;

			ClaimFirstOffer(_ctsProvider.Token).Forget();
		}

		private async UniTask ClaimFirstOffer(CancellationToken ct)
		{
			_claimingInProgress = true;
			
			COfferWithResourceSources offerWithSources = _offersWithSources.Dequeue();
			bool shouldBlockQueue = ShouldBlockQueue(offerWithSources);
			_currentItemBlocksQueue = shouldBlockQueue;
			
			await _rewardHandler.ClaimOffer(
				offerWithSources, 
				offerWithSources.PriceSource, 
				offerWithSources.RewardSource, 
				shouldBlockQueue, 
				offerWithSources.Payloads, 
				ct
				);
			
			_claimingInProgress = false;
			_currentItemBlocksQueue = false;
		}

		private bool ShouldBlockQueue(COfferWithResourceSources offerWithSources)
		{
			COffer offer = _user.Offers.GetOffer(offerWithSources.OfferGuid);
			return ContainsBlockingItem(offer.Rewards.ToArray());
		}
		
		private bool ShouldBlockQueue(CRewardsWithResourceSources rewardsWithSources)
		{
			return ContainsBlockingItem(rewardsWithSources.Rewards);
		}
		
		private bool ContainsBlockingItem(IValuable[] rewards)
		{
			bool nonBlockingRewards = rewards.All(valuable => valuable is CConsumableValuable or CResourceValuable or CDispatcherValuable or CEventCoinValuable or CEventPointValuable);
			return !nonBlockingRewards;
		}
		
		private bool IsEmpty()
		{
			return _rewardsWithSources.Count == 0 && _offersWithSources.Count == 0;
		}

		public void AddRewards(EModificationSource source, IValuable[] rewards, CValueModifyParams modifyParams)
		{
			AddRewardsToQueue(rewards, Array.Empty<IParticleSource>(), source, modifyParams);
		}

		public void AddRewardsWithSources(EModificationSource source, IValuable[] rewards, IParticleSource[] particleSources, CValueModifyParams modifyParams)
		{
			AddRewardsToQueue(rewards, particleSources, source, modifyParams);
		}

		public void AddOfferRewardsWithSources(
			EModificationSource priceSource,
			EModificationSource rewardSource,
			string offerGuid,
			CPurchasePayloads payloads,
			CValueModifyParams modifyParams
			)
		{
			COfferWithResourceSources offerWithSources = new (offerGuid, Array.Empty<IParticleSource>(), priceSource, rewardSource, modifyParams, payloads);
			_offersWithSources.Enqueue(offerWithSources);
		}

		public void AddOfferRewardsWithSources(
			EModificationSource priceSource,
			EModificationSource rewardSource,
			string offerGuid,
			IParticleSource[] particleSources,
			CPurchasePayloads payloads,
			CValueModifyParams modifyParams
			)
		{
			COfferWithResourceSources offerWithSources = new (offerGuid, particleSources, priceSource, rewardSource, modifyParams, payloads);
			_offersWithSources.Enqueue(offerWithSources);
		}
		
		public void ChargeValuable(EModificationSource source, IValuable[] rewards, CValueModifyParams modifyParams)
		{
			_rewardHandler.ChargeValuables(rewards, source, modifyParams);
		}

		private void AddRewardsToQueue(IValuable[] rewards, IParticleSource[] particleSources, EModificationSource source, CValueModifyParams modifyParams)
		{
			CRewardsWithResourceSources rewardsWithSources = new (rewards, particleSources, source, modifyParams);
			_rewardsWithSources.Enqueue(rewardsWithSources);
		}

		public async UniTask WaitUntilQueueUnblocked(CancellationToken ct)
		{
			bool isEmpty = IsEmpty();
			bool anyItemWillBlockQueue = AnyItemWillBlockQueue();
			
			while (!isEmpty && anyItemWillBlockQueue || _currentItemBlocksQueue)
			{
				await UniTask.Yield(ct);
				isEmpty = IsEmpty();
				anyItemWillBlockQueue = AnyItemWillBlockQueue();
			}
		}

		public bool IsRunning()
		{
			bool isEmpty = IsEmpty();
			return !isEmpty || _currentItemBlocksQueue;
		}

		private bool AnyItemWillBlockQueue()
		{
			if (_currentItemBlocksQueue)
				return true;

			foreach (COfferWithResourceSources offerSource in _offersWithSources)
			{
				COffer offer = _user.Offers.GetOffer(offerSource.OfferGuid);
				if (ContainsBlockingItem(offer.Rewards.ToArray()))
					return true;
			}
			
			foreach (CRewardsWithResourceSources rewardsSource in _rewardsWithSources)
			{
				if (ContainsBlockingItem(rewardsSource.Rewards))
					return true;
			}
			return false;
		}
	}
}