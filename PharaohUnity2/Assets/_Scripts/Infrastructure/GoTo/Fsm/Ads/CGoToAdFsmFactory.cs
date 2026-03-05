// =========================================
// AUTHOR: Juraj Joscak
// DATE:   20.02.2026
// =========================================

using System;
using System.Linq;
using System.Threading;
using AldaEngine;
using ServerData;
using ServerData.Design;
using TycoonBuilder.GoToStates;
using UnityEngine;

namespace TycoonBuilder
{
	public class CGoToAdFsmFactory
	{
		private readonly CFsmStateFactory _fsmFactory;
		private readonly CUser _user;
		private readonly IServerTime _serverTime;
		private readonly CDesignRegionConfigs _regionConfigs;
		
		public CGoToAdFsmFactory(CFsmStateFactory fsmFactory, CUser user, IServerTime serverTime, CDesignRegionConfigs regionConfigs)
		{
			_fsmFactory = fsmFactory;
			_user = user;
			_serverTime = serverTime;
			_regionConfigs = regionConfigs;
		}

		public CGoToFsm TryCreateGoToPlacement(EAdPlacement placement, CancellationToken ct)
		{
			switch (placement)
			{
				case EAdPlacement.EventPassFree:
					return TryCreateGoToEventPassFree(ct);
				case EAdPlacement.DecadePassFree:
					return TryCreateGoToDecadePassFree(ct);
				case EAdPlacement.FuelSmallOffer:
					return TryCreateGoToFuelSmallOffer(ct);
				case EAdPlacement.SoftCurrencySmallOffer:
					return TryCreateGoToSoftCurrencySmallOffer(ct);
				case EAdPlacement.PassengerSmallOffer:
					return TryCreateGoToPassengerSmallOffer(ct);
				case EAdPlacement.DailyDealsRandomOffer:
					return TryCreateGoToDailyDealsRandomOffer(ct);
				case EAdPlacement.DispatcherAgnes:
					return TryCreateGoToDispatcherAgnes(ct);
				case EAdPlacement.WrenchSmallOffer:
					return TryCreateGoToWrenchSmallOffer(ct);
				case EAdPlacement.EventCoinsSmallOffer:
					return TryCreateGoToEventCoinsSmallOffer(ct);
				case EAdPlacement.MachineOilSmallOffer:
					return TryCreateGoToMachineOilSmallOffer(ct);
				default:
					throw new ArgumentOutOfRangeException(nameof(placement), placement, null);
			}
		}

		private CGoToFsm TryCreateGoToFuelSmallOffer(CancellationToken ct)
		{
			if (!OfferWithPlacementValid(EAdPlacement.FuelSmallOffer, out _))
				return null;
			
			COpenMenuState openMenuState = _fsmFactory.OpenMenuState;
			openMenuState.SetData(EScreenId.FuelStation);

			CGoToFsm fsm = new CGoToFsm(ct)
				.AddState(_fsmFactory.CloseAllMenus)
				.AddState(openMenuState)
				;
			return fsm;
		}
		
		private CGoToFsm TryCreateGoToSoftCurrencySmallOffer(CancellationToken ct)
		{
			if (!OfferWithPlacementValid(EAdPlacement.SoftCurrencySmallOffer, out _))
				return null;
			
			CGoToFsm fsm = new CGoToFsm(ct)
					.SetContextEntry(EGoToContextKey.ShopTab, EShopTab.HardAndSoftCurrency)
					.SetContextEntry(EGoToContextKey.ValuableType, EValuable.SoftCurrency)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenShopMenu)
				;

			return fsm;
		}
		
		private CGoToFsm TryCreateGoToPassengerSmallOffer(CancellationToken ct)
		{
			if (!OfferWithPlacementValid(EAdPlacement.SoftCurrencySmallOffer, out _))
				return null;
			
			CGoToFsm fsm = new CGoToFsm(ct)
				.SetContextEntry(EGoToContextKey.CityMenuTab, ECityMenuTab.Passengers)
				.AddState(_fsmFactory.CloseAllMenus)
				.AddState(_fsmFactory.OpenCityMenu)
				;
			
			return fsm;
		}
		
		private CGoToFsm TryCreateGoToDailyDealsRandomOffer(CancellationToken ct)
		{
			if (!OfferWithPlacementValid(EAdPlacement.DailyDealsRandomOffer, out COffer offer))
				return null;
			
			CGoToFsm fsm = new CGoToFsm(ct)
					.SetContextEntry(EGoToContextKey.ShopTab, EShopTab.DailyOffers)
					.SetContextEntry(EGoToContextKey.ValuableType, offer.Rewards.First().Id)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenShopMenu)
				;

			return fsm;
		}
		
		private CGoToFsm TryCreateGoToDispatcherAgnes(CancellationToken ct)
		{
			if (!OfferWithPlacementValid(EAdPlacement.DispatcherAgnes, out COffer offer))
				return null;
			
			CGoToFsm fsm = new CGoToFsm(ct)
					.SetContextEntry(EGoToContextKey.ShopTab, EShopTab.Dispatchers)
					.SetContextEntry(EGoToContextKey.ValuableType, offer.Rewards.First().Id)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenShopMenu)
				;

			return fsm;
		}
		
		private CGoToFsm TryCreateGoToWrenchSmallOffer(CancellationToken ct)
		{
			if (!OfferWithPlacementValid(EAdPlacement.WrenchSmallOffer, out COffer offer))
				return null;
			
			CGoToFsm fsm = new CGoToFsm(ct)
					.SetContextEntry(EGoToContextKey.ShopTab, EShopTab.Maintenance)
					.SetContextEntry(EGoToContextKey.ValuableType, offer.Rewards.First().Id)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenShopMenu)
				;

			return fsm;
		}
		
		private CGoToFsm TryCreateGoToMachineOilSmallOffer(CancellationToken ct)
		{
			if (!OfferWithPlacementValid(EAdPlacement.MachineOilSmallOffer, out COffer offer))
				return null;
			
			CGoToFsm fsm = new CGoToFsm(ct)
					.SetContextEntry(EGoToContextKey.ShopTab, EShopTab.Maintenance)
					.SetContextEntry(EGoToContextKey.ValuableType, offer.Rewards.First().Id)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenShopMenu)
				;

			return fsm;
		}
		
		private CGoToFsm TryCreateGoToEventCoinsSmallOffer(CancellationToken ct)
		{
			if (!OfferWithPlacementValid(EAdPlacement.EventCoinsSmallOffer, out COffer offer))
				return null;
			
			ILiveEvent[] runningEvents = _user.LiveEvents.GetRunningEventsOrDefault();
			ELiveEvent offerEvent = offer.GetParamValue<ELiveEvent>(EOfferParam.LiveEvent);
			if(runningEvents.All(e => e.Id != offerEvent))
				return null;
			
			CGoToFsm fsm = new CGoToFsm(ct)
					.SetContextEntry(EGoToContextKey.LiveEventId, offerEvent)
					.SetContextEntry(EGoToContextKey.LiveEventTab, EEventOverviewTab.Shop)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenLiveEventMenu)
				;
			
			return fsm;
		}
		
		private CGoToFsm TryCreateGoToDecadePassFree(CancellationToken ct)
		{
			EYearMilestone currentYear = _user.Progress.Year;
			int ownedXp = _user.Progress.XpInCurrentYear;
			
			CRegionConfig config = _regionConfigs.GetRegionConfig(_user.Progress.Region);
			CDecadePassConfigData decadePassConfig = config.GetDecadePass();

			int numberOfItems = decadePassConfig.FreeRewards.Count;
			int validRewardIndex = -1;

			for (int i = 0; i < numberOfItems; i++)
			{
				if (!IsRewardValid(i))
					continue;
				
				validRewardIndex = i;
				break;
			}

			if (validRewardIndex == -1)
				return null;

			COpenMenuState openMenuState = _fsmFactory.OpenMenuState;
			openMenuState.SetData(EScreenId.DecadePass);
			
			CGoToFsm fsm = new CGoToFsm(ct)
					.SetContextEntry(EGoToContextKey.PassRewardIndex, validRewardIndex)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(openMenuState)
					.AddState(_fsmFactory.ScrollToPassReward)
				;
			
			return fsm;

			bool IsRewardValid(int index)
			{
				bool canBeDoubled = _user.DecadePass.CanFreeRewardBeDoubled(index);
				bool isClaimable = _user.DecadePass.CanClaimIndex(index, false, currentYear, ownedXp);
				return canBeDoubled && isClaimable;
			}
		}
		
		private CGoToFsm TryCreateGoToEventPassFree(CancellationToken ct)
		{
			int validRewardIndex = -1;
			ELiveEvent eventId = ELiveEvent.None;
			
			ILiveEvent[] runningEvents = _user.LiveEvents.GetRunningEventsOrDefault();
			foreach (ILiveEvent liveEvent in runningEvents)
			{
				if(liveEvent.BaseContent is not IEventWithBattlePass contentWithPass)
					continue;

				validRewardIndex = GetValidRewardIndex(contentWithPass);
				if (validRewardIndex == -1)
					continue;
				
				eventId = liveEvent.Id;
				break;
			}

			if (validRewardIndex == -1)
				return null;

			CGoToFsm fsm = new CGoToFsm(ct)
					.SetContextEntry(EGoToContextKey.LiveEventId, eventId)
					.SetContextEntry(EGoToContextKey.LiveEventId, ELiveEvent.EarthAndFire)
					.SetContextEntry(EGoToContextKey.LiveEventTab, EEventOverviewTab.Pass)
					.SetContextEntry(EGoToContextKey.PassRewardIndex, validRewardIndex)
					.SetContextEntry(EGoToContextKey.PassRewardIndex, 6)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenLiveEventMenu)
					.AddState(_fsmFactory.ScrollToPassReward)
				;
			
			return fsm;

			int GetValidRewardIndex(IEventWithBattlePass content)
			{
				int rewardsCount = content.GetBattlePassRewardsCount();
				for (int i = 0; i < rewardsCount; i++)
				{
					bool claimable = content.CanClaimIndex(i, false, content.EventPoints);
					bool claimed = content.IsRewardClaimed(i, false);
					bool canBeDoubled = content.CanFreeRewardBeDoubled(i);

					if (canBeDoubled && claimable && !claimed)
						return i;
				}

				return -1;
			}
		}

		private bool OfferWithPlacementValid(EAdPlacement placement, out COffer offer)
		{
			offer = GetOfferByPlacement(placement);
			if(offer == null)
				return false;
			
			if(!_user.Offers.IsOfferValid(offer))
				return false;
			
			if(offer.MaxPurchasesReached())
				return false;
			
			if(OfferIsBlocked(offer))
				return false;

			return true;
		}
		
		private COffer GetOfferByPlacement(EAdPlacement placement)
		{
			return _user.Offers.Offers.Values.FirstOrDefault(o => o.Price is CAdvertisementValuable adPrice && adPrice.Placement == placement);
		}
		
		private bool OfferIsBlocked(COffer offer)
		{
			return DispatcherActiveBlock() || CooldownBlock() || FuelCapacityBlock();
			
			bool DispatcherActiveBlock()
			{
				CDispatcherValuable dispatcherReward = offer.Rewards.FirstOrDefault(reward => reward is CDispatcherValuable) as CDispatcherValuable;
				if(dispatcherReward == null)
					return false;

				bool? dispatcherActive = _user.Dispatchers.HaveDispatcher(dispatcherReward.Dispatcher, _serverTime.GetTimestampInMs());
				return dispatcherActive.HasValue && dispatcherActive.Value;
			}

			bool CooldownBlock()
			{
				if(offer.Params.All(p => p.Id != EOfferParam.CoolDownInSecs) || offer.Params.All(p => p.Id != EOfferParam.LastPurchaseTime))
					return false;
				
				long cooldown = offer.GetParamValue<int>(EOfferParam.CoolDownInSecs) * CTimeConst.Second.InMilliseconds;
				long lastPurchaseTime = offer.GetParamValue<long>(EOfferParam.LastPurchaseTime);
				long cooldownEndTime = lastPurchaseTime + cooldown;
			
				return cooldownEndTime > _serverTime.GetTimestampInMs();
			}

			bool FuelCapacityBlock()
			{
				if (offer.Rewards.First().Id != EValuable.Fuel)
					return false;
			
				if(offer.Price is not CAdvertisementValuable)
					return false;
			
				int maxFuel = _user.FuelStation.GetFuelCapacity();
				
				return _user.OwnedValuables.HaveValuable(new CConsumableValuable(EValuable.Fuel, maxFuel));
			}
		}
	}
}