// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using Server;
using ServerData;
using ServerData.Design;
using ServerData.Hits;
using TycoonBuilder.Offers;
using UnityEngine;

namespace TycoonBuilder
{
	public class COffers : CBaseUserComponent, IInitializable
	{
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly CStaticOffers _staticOffers;
		private readonly IServerTime _serverTime;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;
		private readonly CImageDownloader _imageDownloader;
		private readonly ICtsProvider _ctsProvider;
		private readonly CDesignDispatcherConfigs _designDispatcherConfigs;

		public IReadOnlyDictionary<string, COffer> Offers => _offers;
		private readonly Dictionary<string, COffer> _offers = new();
		private readonly Dictionary<string, COfferGroup> _offerGroups = new();

		public COffers(
			CDesignVehicleConfigs vehicleConfigs,
			CDesignDispatcherConfigs designDispatcherConfigs,
			CStaticOffers staticOffers, 
			IServerTime serverTime, 
			CHitBuilder hitBuilder, 
			IEventBus eventBus, 
			IMapper mapper,
			CImageDownloader imageDownloader,
			ICtsProvider ctsProvider
			)
		{
			_vehicleConfigs = vehicleConfigs;
			_staticOffers = staticOffers;
			_serverTime = serverTime;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_mapper = mapper;
			_imageDownloader = imageDownloader;
			_ctsProvider = ctsProvider;
			_designDispatcherConfigs = designDispatcherConfigs;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CPassengerContractRewardsClaimedSignal>(OnPassengerContractRewardsClaimed);
			_eventBus.Subscribe<CStoryContractRewardsClaimedSignal>(OnStoryContractRewardsClaimed);
		}

		public void InitialSync(COffersDto dto)
		{
			AddOffers(dto.Offers);
			AddOfferGroups(dto.Groups);
		}
		
		public void Sync(COffersDto newOffersDto)
		{
			if(newOffersDto == null)
				return;
			
			COffer[] newOffers = AddOffers(newOffersDto.Offers);
			AddOfferGroups(newOffersDto.Groups);
			_eventBus.Send(new CNewOffersSyncedSignal(newOffers));
		}
		
		public COffer GetOffer(string guid)
		{
			bool isOffer =  _offers.ContainsKey(guid);
			if (isOffer)
			{
				return _offers[guid];
			}

			COffer offer = User.LiveEvents.GetOfferOrDefault(guid);
			if (offer != null)
				return offer;

			offer = _staticOffers.GetOffer(guid);
			return offer;
		}
		
		public COffer GetStaticOffer(EStaticOfferId id)
		{
			return _staticOffers.GetOffer(id);
		}
		
		private void OnPassengerContractRewardsClaimed(CPassengerContractRewardsClaimedSignal signal)
		{
			IncreaseCompletedPiggyContracts();
		}

		private void OnStoryContractRewardsClaimed(CStoryContractRewardsClaimedSignal signal)
		{
			IncreaseCompletedPiggyContracts();
		}

		private void IncreaseCompletedPiggyContracts()
		{
			foreach (KeyValuePair<string, COffer> offer in _offers)
			{
				bool isPiggy = offer.Value.HavePiggyContractMilestone();
				if(!isPiggy)
					continue;
				offer.Value.IncreaseCompletedPiggyContracts();
			}
		}

		private COffer[] AddOffers(COfferDto[] offers)
		{
			List<COffer> newOffers = new();
			foreach (COfferDto offerDto in offers)
			{
				COffer offer = _mapper.Map<COfferDto, COffer>(offerDto);
				newOffers.Add(offer);
				PreloadImages(offer.Params);
				if (_offers.TryAdd(offer.Guid, offer))
					continue;
				
				_offers[offer.Guid] = offer;
			}
			return newOffers.ToArray();
		}

		private void AddOfferGroups(COfferGroupDto[] groups)
		{
			foreach (COfferGroupDto groupDto in groups)
			{
				COfferGroup group = _mapper.Map<COfferGroupDto, COfferGroup>(groupDto);
				PreloadImages(group.Params);
				if (_offerGroups.TryAdd(group.GroupId, group))
					continue;
				
				_offerGroups[group.GroupId] = group;
			}
		}

		private void PreloadImages(IOfferParam[] offerParams)
		{
			string trayIconUrl = offerParams.FirstOrDefault(p => p.Id == EOfferParam.TrayIcon)?.GetValueOrDefault<string>();
			if (trayIconUrl != null)
			{
				_imageDownloader.GetImage(trayIconUrl, _ctsProvider.Token).Forget();
			}
			
			string backgroundUrl = offerParams.FirstOrDefault(p => p.Id == EOfferParam.BackgroundImage)?.GetValueOrDefault<string>();
			if (backgroundUrl != null)
			{
				_imageDownloader.GetImage(backgroundUrl, _ctsProvider.Token).Forget();
			}
		}

		public COffer[] GetOffersByParam<T>(EOfferParam paramId, T paramValue)
		{
			return _offers.Values.Where(
					offer => offer.Params.Any(param => param is COfferParam typedParam 
					                                   && typedParam.Id == paramId 
					                                   && typedParam.GetValueOrDefault<T>().Equals(paramValue)))
				.ToArray();
		}
		
		public COffer[] GetOffersWithParam(EOfferParam paramId)
		{
			return _offers.Values.Where(offer => offer.Params.Any(param => param.Id == paramId)).ToArray();
		}
		
		public IEnumerable<COfferGroup> GetGroupsWithParam(EOfferParam paramId)
		{
			return _offerGroups.Values.Where(group => group.Params.Any(param => param.Id == paramId));
		}
		
		public COfferGroup GetOfferGroup(string groupId)
		{
			if (_offerGroups.TryGetValue(groupId, out COfferGroup group))
				return group;
			
			throw new Exception($"Offer group with id {groupId} not found.");
		}
		
		public bool IsOfferValid(COffer offer)
		{
			if(offer.Rewards.Any(reward => reward is CVehicleValuable vehicleReward && User.Vehicles.IsVehicleOwned(vehicleReward.Vehicle)) && !offer.MaxPurchasesReached())
				return false;
			
			if(offer.Rewards.Any(reward => reward is CBuildingValuable buildingReward && (User.City.HaveBuilding(buildingReward.Building) ?? false)) && !offer.MaxPurchasesReached())
				return false;
			
			if(offer.Rewards.Count == 1 && offer.Rewards[0] is CDispatcherValuable dispatcherReward && !DispatcherOfferIsValid(dispatcherReward))
				return false;
			
			long expirationTime = offer.GetParamValueOrDefault<long>(EOfferParam.ExpirationTime); 
			return expirationTime == 0 || expirationTime >= _serverTime.GetTimestampInMs();
		}
		
		private bool DispatcherOfferIsValid(CDispatcherValuable dispatcherReward)
		{
			if(dispatcherReward.ExpirationDurationIsSecs.HasValue)
				return true;
			
			if(User.Dispatchers.HaveDispatcher(dispatcherReward.Dispatcher, _serverTime.GetTimestampInMs()) ?? false)
				return false;
			
			CStaticDispatcherConfig cfg = _designDispatcherConfigs.GetConfig(dispatcherReward.Dispatcher);
			CYearUnlockRequirement yearRequirement = GetRequirementOfType<CYearUnlockRequirement>(cfg.UnlockRequirement);
			if(yearRequirement != null && yearRequirement.Year > User.Progress.Year + 4)
				return false;
			
			COwnedValuableUnlockRequirement ownedValuableRequirement = GetRequirementOfType<COwnedValuableUnlockRequirement>(cfg.UnlockRequirement);
			if(ownedValuableRequirement is { Valuable: CDispatcherValuable dispatcherValuable } && !(User.Dispatchers.HaveDispatcher(dispatcherValuable.Dispatcher, _serverTime.GetTimestampInMs()) ?? false))
				return false;
				
			return true;

			T GetRequirementOfType<T>(IUnlockRequirement requirement) where T : class, IUnlockRequirement
			{
				if(requirement is T typedRequirement)
					return typedRequirement;

				if(requirement is not CCompositeUnlockRequirement compositeRequirement)
					return null;
				
				foreach (IUnlockRequirement partial in compositeRequirement.Requirements)
				{
					T found = GetRequirementOfType<T>(partial);
					if (found != null)
						return found;
				}

				return null;
			}
		}

		public bool IsGroupValid(string groupId)
		{
			COffer[] offers = GetOffersByParam(EOfferParam.GroupId, groupId);
			if (offers.Any(offer => !IsOfferValid(offer)))
				return false;
			
			COfferGroup group = GetOfferGroup(groupId);
			EOfferType type = group.GetParamValueOrDefault<EOfferType>(EOfferParam.OfferType);
			if (type == EOfferType.PiggyBank)
			{
				return IsPiggyBankValid(groupId);
			}
			
			if (offers.All(offer => offer.MaxPurchasesReached()))
				return false;
			
			return true;
		}

		private bool IsPiggyBankValid(string groupId)
		{
			COffer[] offers = GetOffersByParam(EOfferParam.GroupId, groupId).OrderBy(Order).ToArray();
			return !offers.Last().MaxPurchasesReached();

			int Order(COffer offer)
			{
				return offer.GetParamValue<int>(EOfferParam.PiggyMilestone);
			}
		}

		public bool OfferShowsMarker(COffer offer)
		{
			if(!IsOfferValid(offer))
				return false;
				
			if(offer.IsExpired(_serverTime.GetTimestampInMs()))
				return false;

			if (offer.MaxPurchasesReached())
				return false;
				
			if(offer.Price is CFreeValuable)
				return true;
			
			if(!offer.IsSeen)
				return true;

			return false;
		}
		
		public void MarkOfferAsSeen(string offerGuid)
		{
			if (!_offers.TryGetValue(offerGuid, out COffer offer))
				return;
			
			if(offer.IsSeen)
				return;
			
			offer.MarkAsSeen();
			_hitBuilder.GetBuilder(new CMarkOfferAsSeenRequest(offerGuid))
				.BuildAndSend();
			
			_eventBus.Send(new COfferSeenSignal());
		}
		
		public void MarkOffersAsSeen(IEnumerable<string> offerGuids)
		{
			foreach (string guid in offerGuids)
			{
				if (!_offers.TryGetValue(guid, out COffer offer))
					return;

				if (offer.IsSeen)
					continue;
			
				offer.MarkAsSeen();
				_hitBuilder.GetBuilder(new CMarkOfferAsSeenRequest(guid))
					.BuildAndSend();
			}
			
			_eventBus.Send(new COfferSeenSignal());
		}

		public IEnumerable<EBundleId> GetSpecialOfferBundles()
		{
			foreach (var offer in _offers)
			{
				foreach (IValuable reward in offer.Value.Rewards)
				{
					if (reward is CVehicleValuable vehicle)
					{
						CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(vehicle.Vehicle);
						if (vehicleConfig.OverrideBundleId != EBundleId.None)
						{
							yield return vehicleConfig.OverrideBundleId;
						}
					}
				}
			}
		}
	}
}