// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.12.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;
using UnityEngine;

namespace TycoonBuilder.Infrastructure
{
	public class CDispatcherOffersProvider : IDispatcherOffersProvider
	{
		private readonly IUnlockRequirement _unlockRequirement = IUnlockRequirement.Year(EYearMilestone._1931);
		
		private readonly CDesignDispatcherConfigs _designDispatcherConfigs;
		private readonly IServerTime _serverTime;
		private readonly CUser _user;

		public CDispatcherOffersProvider(
			CDesignDispatcherConfigs designDispatcherConfigs,
			IServerTime serverTime,
			CUser user)
		{
			_designDispatcherConfigs = designDispatcherConfigs;
			_serverTime = serverTime;
			_user = user;
		}
		
		private COffer[] GetAllOffers()
		{
			COffer[] offers = _user.Offers.GetOffersByParam(EOfferParam.ShopTab, EOfferShopTab.DispatchersShop)
				.Where(offer => offer.GetPlacement().HasFlag(EOfferPlacement.Shop))
				.ToArray();	
			return offers;
		}

		public bool AnyOfferAvailable()
		{
			bool isUnlockRequirementMet = _user.IsUnlockRequirementMet(_unlockRequirement);
			if (!isUnlockRequirementMet)
				return false;
			
			COffer offer = GetOfferWithHighestPriorityOrDefault();
			return offer != null;
		}

		public COffer GetOfferWithHighestPriorityOrDefault()
		{
			COffer permanentOffer = GetPermanentDispatcherOfferOrDefault();
			if (permanentOffer != null)
				return permanentOffer;
			
			COffer adOffer = GetAdDispatcherOfferOrDefault();
			return adOffer;
		}

		private COffer GetPermanentDispatcherOfferOrDefault()
		{
			COffer[] offers = GetAllOffers();
			foreach (COffer currentOffer in offers)
			{
				CDispatcherValuable dispatcherReward = GetDispatcherValuableFromOffer(currentOffer);
				if (dispatcherReward == null)
					continue;

				bool isForAd = currentOffer.Price is CAdvertisementValuable;
				if (isForAd)
					continue;
				
				CDispatcherValuable dispatcherValuable = GetDispatcherValuableFromOffer(currentOffer);
				bool isDispatcherActive = IsDispatcherActive(dispatcherValuable.Dispatcher);
				if (isDispatcherActive)
					continue;

				bool isPermanent = !dispatcherReward.ExpirationDurationIsSecs.HasValue;
				if (!isPermanent)
					continue;
				
				CStaticDispatcherConfig cfg = _designDispatcherConfigs.GetConfig(dispatcherReward.Dispatcher);
				bool isUnlockRequirementMet = _user.IsUnlockRequirementMet(cfg.UnlockRequirement);
				if (!isUnlockRequirementMet)
					continue;
				
				return currentOffer;
			}
			return null;
		}

		public CDispatcherValuable GetDispatcherValuableFromOffer(COffer offer)
		{
			CDispatcherValuable valuable = null;
			foreach (IValuable reward in offer.Rewards)
			{
				if (reward is not CDispatcherValuable dispatcherValuable)
					continue;

				if (valuable != null)
				{
					Debug.LogWarning("Multiple dispatcher valuables found in offer rewards.");
					continue;
				}
				valuable = dispatcherValuable;
			}

			if (valuable != null) 
				return valuable;
			
			Debug.LogWarning("No dispatcher valuable found in offer rewards.");
			return null;
		}

		private COffer GetAdDispatcherOfferOrDefault()
		{
			COffer[] offers = GetAllOffers();
			foreach (COffer currentOffer in offers)
			{
				CDispatcherValuable dispatcherReward = GetDispatcherValuableFromOffer(currentOffer);
				if (dispatcherReward == null)
					continue;

				bool isForAd = currentOffer.Price is CAdvertisementValuable;
				if (!isForAd)
					continue;
			
				bool isOnCooldown = IsOnCooldown(currentOffer);
				if (isOnCooldown)
					continue;
			
				CDispatcherValuable dispatcherValuable = GetDispatcherValuableFromOffer(currentOffer);
				bool isDispatcherActive = IsDispatcherActive(dispatcherValuable.Dispatcher);
				if (isDispatcherActive)
					continue;
				
				CStaticDispatcherConfig cfg = _designDispatcherConfigs.GetConfig(dispatcherReward.Dispatcher);
				bool isUnlockRequirementMet = _user.IsUnlockRequirementMet(cfg.UnlockRequirement);
				if (!isUnlockRequirementMet)
					continue;
				
				return currentOffer;
			}
			return null;
		}

		private bool IsDispatcherActive(EDispatcher dispatcherId)
		{
			bool isDispatcherActive = _user.Dispatchers.IsDispatcherActive(dispatcherId, _serverTime.GetTimestampInMs());
			return isDispatcherActive;
		}

		private bool IsOnCooldown(COffer offer)
		{
			if (!HasParam(EOfferParam.CoolDownInSecs, offer.Params) || !HasParam(EOfferParam.LastPurchaseTime, offer.Params))
			{
				return false;
			}

			long cooldown = GetParamValue<int>(EOfferParam.CoolDownInSecs, offer.Params) * CTimeConst.Second.InMilliseconds;
			long lastPurchaseTime = GetParamValue<long>(EOfferParam.LastPurchaseTime, offer.Params);
			long cooldownEndTime = lastPurchaseTime + cooldown;

			bool isOnCooldown = cooldownEndTime > _serverTime.GetTimestampInMs();
			return isOnCooldown;

			bool HasParam(EOfferParam paramType, IOfferParam[] offerParams)
			{
				return offerParams.Any(param => param.Id == paramType);
			}
			
			T GetParamValue<T>(EOfferParam paramType, IOfferParam[] offerParams)
			{
				IOfferParam foundParam = offerParams.FirstOrDefault(param => param.Id == paramType);
				return foundParam == null ? throw new KeyNotFoundException($"Parameter {paramType} not found.") : foundParam.GetValue<T>();
			}
		}
	}
}