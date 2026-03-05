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
using ServerData.Hits;
using ServiceEngine.Ads;
using ServiceEngine.Purchasing;
using Zenject;
using UnityEngine;

namespace TycoonBuilder.Infrastructure
{
	public class CRewardHandler : IRewardHandler, IAldaFrameworkComponent
	{
		private readonly CDispatchersRewardHandler _dispatcherRewardHandler;
		private readonly CValuableRegionModifier _valuableRegionModifier;
		private readonly CResourceRewardHandler _resourceRewardHandler;
		private readonly CVehicleRewardHandler _vehicleRewardHandler;
		private readonly CBuildingRewardHandler _buildingRewardHandler;
		private readonly CValuableRewardHandler _valuableRewardHandler;
		private readonly IUserValidator _userValidator;
		private readonly CEventSystem _eventSystem;
		private readonly CInAppPrices _inAppPrices;
		private readonly CHitBuilder _hitBuilder;
		private readonly IPurchasing _purchasing;
		private readonly IServerTime _serverTime;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;
		private readonly IAdsManager _adsManager;
		
		private readonly CInputLock _inputLock = new("RewardHandler", EInputLockLayer.RewardHandler);
		private readonly CInputLock _purchasingInputLock = new("Purchasing", EInputLockLayer.RewardHandler);

		public CRewardHandler(
			CDispatchersRewardHandler dispatcherRewardHandler,
			CValuableRegionModifier valuableRegionModifier,
			CResourceRewardHandler resourceRewardHandler,
			CValuableRewardHandler valuableRewardHandler,
			CVehicleRewardHandler vehicleRewardHandler,
			CBuildingRewardHandler buildingRewardHandler,
			IUserValidator userValidator,
			CEventSystem eventSystem,
			CInAppPrices inAppPrices,
			CHitBuilder hitBuilder,
			IPurchasing purchasing,
			IServerTime serverTime,
			IEventBus eventBus, 
			CUser user,
			IAdsManager adsManager
			)
		{
			_dispatcherRewardHandler = dispatcherRewardHandler;
			_valuableRegionModifier = valuableRegionModifier;
			_resourceRewardHandler = resourceRewardHandler;
			_valuableRewardHandler = valuableRewardHandler;
			_vehicleRewardHandler = vehicleRewardHandler;
			_buildingRewardHandler = buildingRewardHandler;
			_userValidator = userValidator;
			_eventSystem = eventSystem;
			_inAppPrices = inAppPrices;
			_hitBuilder = hitBuilder;
			_purchasing = purchasing;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_user = user;
			_adsManager = adsManager;
		}

		public async UniTask ClaimOffer(
			COfferWithResourceSources offerWithSources, 
			EModificationSource priceSource, 
			EModificationSource rewardSource, 
			bool shouldBlockQueue,
			CPurchasePayloads payloads,
			CancellationToken ct
			)
		{
			if (!IsOfferAvailable(offerWithSources.OfferGuid))
				return;
			
			COffer offer = _user.Offers.GetOffer(offerWithSources.OfferGuid);
			if (offer == null)
			{
				Debug.LogError($"Offer with guid {offerWithSources.OfferGuid} not found.");
				return;
			}
			
			if (shouldBlockQueue)
			{
				_eventSystem.AddInputLocker(_inputLock);
			}

			bool success;
			switch (offer.Price)
			{
				case CFreeNoHitValuable:
					await ClaimRewards(offer.Rewards.ToArray(), offerWithSources.ParticleSources, rewardSource, shouldBlockQueue, ct, offerWithSources.ModifyParams);
					success = true;
					break;
				case CFreeValuable:
					await ClaimRewards(offer.Rewards.ToArray(), offerWithSources.ParticleSources, rewardSource, shouldBlockQueue, ct, offerWithSources.ModifyParams);
					SendServerHit(offer);
					success = true;
					break;
				case CAdvertisementValuable adValuable:
					success = await _adsManager.PlayAd(new CAdData(adValuable.Placement), EAdUnit.Rewarded, ct);
					if (success)
					{
						await ClaimRewards(offer.Rewards.ToArray(), offerWithSources.ParticleSources, rewardSource, shouldBlockQueue, ct, offerWithSources.ModifyParams);
						SendServerHit(offer);
					}
					break;
				case CConsumableValuable consumable:
					success = await TryPurchaseForConsumable(consumable, offerWithSources, offerWithSources.PriceSource, offerWithSources.RewardSource, shouldBlockQueue, ct);
					SendServerHit(offer);
					break;
				case CResourceValuable resource:
					success = await TryPurchaseForResource(resource, offerWithSources, offerWithSources.RewardSource, shouldBlockQueue, ct);
					SendServerHit(offer);
					break;
				case CRealMoneyValuable realMoney:
					success = await TryPurchaseForRealMoney(realMoney, offerWithSources, offerWithSources.RewardSource, payloads, shouldBlockQueue, ct);
					break;
				case CEventCoinValuable eventCoin:
					success = await TryPurchaseForEventCoins(eventCoin, offerWithSources, offerWithSources.PriceSource, offerWithSources.RewardSource, shouldBlockQueue, ct);
					SendServerHit(offer);
					break;
				default:
					Debug.LogError($"Offer with price {offer.Price.GetType()} is not implemented.");
					success = false;
					break;
			}

			if (success)
			{
				offer.Claim(_serverTime.GetTimestampInMs());
				_eventBus.Send(new COfferClaimedSignal(offer.Guid));
			}
			
			_eventSystem.RemoveInputLocker(_inputLock);
		}

		public void ChargeValuables(IValuable[] rewards, EModificationSource source, CValueModifyParams modifyParams)
		{
			foreach (IValuable reward in rewards)
			{
				if (reward is CFreeValuable or CFreeNoHitValuable)
					continue;
				
				switch (reward)
				{
					case CConsumableValuable consumable:
						RemovePrice(consumable, source, modifyParams);
						break;
					case CResourceValuable resource:
						RemovePrice(resource, modifyParams);
						break;
					default:
						Debug.LogError($"Reward of type {reward.GetType()} is not implemented.");
						break;
				}
			}
		}

		private void SendServerHit(COffer offer)
		{
			CHitRecordBuilder hitBuilder = _hitBuilder.GetBuilder(new CValidatePurchaseRequest(null, offer.Guid, EModificationSource.None));
			hitBuilder.BuildAndSend();
		}

		private bool IsOfferAvailable(string offerGuid)
		{
			COffer offer = _user.Offers.GetOffer(offerGuid);
			if (offer == null)
				return false;

			int buyCount = offer.GetParamValueOrDefault<int>(EOfferParam.PurchasesCount);
			int maxBuyCount = offer.GetParamValueOrDefault<int>(EOfferParam.MaxPurchasesCount);

			if (buyCount < maxBuyCount || maxBuyCount <= 0)
				return true;
			
			Debug.LogError($"Offer {offer.Guid} has reached maximum purchase count of {maxBuyCount}. Cannot claim.");
			return false;
		}

		public async UniTask ClaimRewards(
			IValuable[] rewards,
			IParticleSource[] sources,
			EModificationSource source,
			bool shouldBlockQueue,
			CancellationToken ct,
			CValueModifyParams modifyParams)
		{
			rewards = _valuableRegionModifier.ModifyValuables(rewards, _user.Progress.Region, source);
			
			if (sources.Length > 0)
			{
				_eventBus.ProcessTask(new CAddModifiedSourcesTask(sources));
			}
			
			for (int i = 0; i < rewards.Length; i++)
			{
				IValuable reward = rewards[i];
				switch (reward)
				{
					case CConsumableValuable:
					case CEventPointValuable:
					case CXpValuable:
					case CEventCoinValuable:
					case CFrameValuable:
					{
						UniTask task = _valuableRewardHandler.Claim(rewards, i, ct, modifyParams);
						if (shouldBlockQueue)
						{
							await task;
						}
						break;
					}
					case CResourceValuable resource:
					{
						UniTask task = _resourceRewardHandler.Claim(rewards, i, resource, ct, modifyParams);
						if (shouldBlockQueue)
						{
							await task;
						}
						break;
					}
					case CVehicleValuable vehicle:
						await _vehicleRewardHandler.Claim(vehicle, ct, modifyParams);
						break;
					case CDispatcherValuable dispatcher:
						await _dispatcherRewardHandler.Claim(dispatcher, ct, modifyParams);
						break;
					case CBuildingValuable building:
						_buildingRewardHandler.Claim(building, modifyParams);
						break;
					default:
						Debug.LogError($"Reward of type {reward.GetType()} is not implemented.");
						break;
				}
			}
			
			_eventBus.ProcessTask<CClearModifiedSourcesTask>();
		}

		private void RemovePrice(CConsumableValuable consumable, EModificationSource source, CValueModifyParams modifyParams)
		{
			_valuableRewardHandler.Remove(consumable, source, modifyParams);
		}
		
		private void RemovePrice(CResourceValuable resource, CValueModifyParams modifyParams)
		{
			_resourceRewardHandler.Remove(resource, modifyParams);
		}

		private async UniTask<bool> TryPurchaseForConsumable(
			CConsumableValuable price, 
			COfferWithResourceSources offerWithSources, 
			EModificationSource priceSource, 
			EModificationSource rewardSource,
			bool shouldBlockQueue,
			CancellationToken ct
			)
		{
			bool canAfford = _user.OwnedValuables.HaveValuable(price);
			if (!canAfford)
				return false;
			
			RemovePrice(price, priceSource, offerWithSources.ModifyParams);
			COffer offer = _user.Offers.GetOffer(offerWithSources.OfferGuid);
			await ClaimRewards(offer.Rewards.ToArray(), offerWithSources.ParticleSources, rewardSource, shouldBlockQueue, ct, offerWithSources.ModifyParams);
			return true;
		}
		
		private async UniTask<bool> TryPurchaseForEventCoins(
			CEventCoinValuable price, 
			COfferWithResourceSources offerWithSources, 
			EModificationSource priceSource, 
			EModificationSource rewardSource,
			bool shouldBlockQueue,
			CancellationToken ct
		)
		{
			bool canAfford = _user.LiveEvents.HaveEventCoin(price);
			if (!canAfford)
				return false;
			
			_user.LiveEvents.AddEventCoins(price.Inverse());
			COffer offer = _user.Offers.GetOffer(offerWithSources.OfferGuid);
			await ClaimRewards(offer.Rewards.ToArray(), offerWithSources.ParticleSources, rewardSource, shouldBlockQueue, ct, offerWithSources.ModifyParams);
			return true;
		}

		private async UniTask<bool> TryPurchaseForResource(
			CResourceValuable price, 
			COfferWithResourceSources offerWithSources, 
			EModificationSource source, 
			bool shouldBlockQueue, 
			CancellationToken ct
			)
		{
			bool canAfford = _user.OwnedValuables.HaveValuable(price);
			if (!canAfford)
				return false;
			
			RemovePrice(price, offerWithSources.ModifyParams);
			
			COffer offer = _user.Offers.GetOffer(offerWithSources.OfferGuid);
			await ClaimRewards(offer.Rewards.ToArray(), offerWithSources.ParticleSources, source, shouldBlockQueue, ct, offerWithSources.ModifyParams);
			return true;
		}

		private async UniTask<bool> TryPurchaseForRealMoney(
			CRealMoneyValuable price, 
			COfferWithResourceSources offerWithSources, 
			EModificationSource source, 
			CPurchasePayloads payloads,
			bool shouldBlockQueue, 
			CancellationToken ct
			)
		{
			CLockObject lockObject = new("RewardHandler_TryPurchaseForRealMoney");
			_userValidator.PauseValidation(lockObject);
			
			long timestampInMs = _serverTime.GetTimestampInMs();
			string productId = _inAppPrices.GetStoreId(price.Price);

			COffer offer = _user.Offers.GetOffer(offerWithSources.OfferGuid);

			CPurchaseMetadata purchaseMetadata = new(
				offerWithSources.OfferGuid, 
				productId, 
				offer.GetAnalyticsId(),
				timestampInMs, 
				EModificationSource.OfferReward,
				payloads
				);
			_eventSystem.AddInputLocker(_purchasingInputLock);

			long requestClientTime = _serverTime.GetTimestampInMs();
			EPurchaseState purchaseState = await _purchasing.PurchaseProduct(purchaseMetadata);
			
			_eventSystem.RemoveInputLocker(_purchasingInputLock);
		
			if (purchaseState == EPurchaseState.Failed)
			{
				_userValidator.ResumeValidation(lockObject);
				return false;
			}

			await ClaimRewards(offer.Rewards.ToArray(), offerWithSources.ParticleSources, source, shouldBlockQueue, ct, offerWithSources.ModifyParams);
			// wait for sync - server response can be delayed because of delayed processing on main thread
			
			await UniTask.Yield(ct);
			
			TrySendBattlePassPurchasedSignal(offer.Guid, requestClientTime);
			TrySendEventPassPurchasedSignal(offer.Guid, requestClientTime);
		
			_userValidator.ResumeValidation(lockObject);
			return true;
		}

		private void TrySendBattlePassPurchasedSignal(string offerGuid, long timestamp)
		{
			string premiumStaticOfferGuid = $"StaticOffer_{(int)EStaticOfferId.DecadePassPremium}";
			string extraPremiumStaticOfferGuid = $"StaticOffer_{(int)EStaticOfferId.DecadePassExtraPremium}";

			bool isPremiumOffer = offerGuid == premiumStaticOfferGuid;
			bool isExtraPremiumOffer = offerGuid == extraPremiumStaticOfferGuid;
			
			if (!isPremiumOffer && !isExtraPremiumOffer)
				return;
			
			EBattlePassPremiumStatus status = isExtraPremiumOffer
				? EBattlePassPremiumStatus.ExtraPremium
				: EBattlePassPremiumStatus.Premium;
			
			_eventBus.Send(new CDecadePassPremiumPurchasedSignal(status));
			_user.FuelStation.Recharger.Update(timestamp);
		}

		private void TrySendEventPassPurchasedSignal(string offerGuid, long timestamp)
		{
			IEventWithBattlePass eventWithBattlePass = _user.LiveEvents.GetEventWithPremiumOfferOrDefault(offerGuid);
			if (eventWithBattlePass == null)
				return;

			bool isPremiumOffer = offerGuid == eventWithBattlePass.PremiumEventPassOffer.Guid;
			bool isExtraPremiumOffer = offerGuid == eventWithBattlePass.ExtraPremiumEventPassOffer.Guid;
			
			if (!isPremiumOffer && !isExtraPremiumOffer)
				return;
			
			EBattlePassPremiumStatus status = isExtraPremiumOffer
				? EBattlePassPremiumStatus.ExtraPremium
				: EBattlePassPremiumStatus.Premium;

			ELiveEvent liveEventId = _user.LiveEvents.GetEventId(eventWithBattlePass);
			IEventWithBattlePass eventContent = _user.LiveEvents.GetEventContent<IEventWithBattlePass>(liveEventId);
			eventContent.UpdateBattlePassPremiumPassStatus(status);
			_eventBus.Send(new CEventPassPremiumPurchasedSignal(liveEventId, status));
		}
	}
}