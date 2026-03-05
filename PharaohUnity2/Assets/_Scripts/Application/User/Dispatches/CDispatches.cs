// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.08.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;
using ServerData.Dto;
using ServerData.Hits;
using UnityEngine;

namespace TycoonBuilder
{
	public class CDispatches : CBaseUserComponent
	{
		private readonly List<CDispatch> _dispatches = new();
		private readonly HashSet<string> _tripCompletedFired = new();
		public IReadOnlyList<CDispatch> Dispatches => _dispatches;

		private readonly CDispatchTimeProvider _dispatchTimeProvider;
		private readonly CDispatchPathFactory _dispatchPathFactory;
		private readonly CDesignIndustryConfigs _industryConfigs;
		private readonly CDesignCityConfigs _cityConfigs;
		private readonly CDispatchDepo _dispatchDepo;
		private readonly IRewardQueue _rewardQueue;
		private readonly IServerTime _serverTime;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;

		public CDispatches(
			CDispatchTimeProvider dispatchTimeProvider,
			CDispatchPathFactory dispatchPathFactory,
			CDesignIndustryConfigs industryConfigs,
			CDesignVehicleConfigs vehicleConfigs,
			CDesignCityConfigs cityConfigs,
			IRewardQueue rewardQueue,
			IServerTime serverTime,
			CHitBuilder hitBuilder,
			IEventBus eventBus
			)
		{
			_dispatchDepo = new CDispatchDepo(serverTime, vehicleConfigs);
			_dispatchTimeProvider = dispatchTimeProvider;
			_dispatchPathFactory = dispatchPathFactory;
			_industryConfigs = industryConfigs;
			_rewardQueue = rewardQueue;
			_cityConfigs = cityConfigs;
			_serverTime = serverTime;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
		}

		public void InitialSync(CDispatchesDto dto)
		{
			foreach (CDispatchDto dispatchDto in dto.Dispatches)
			{
				CDispatch dispatch = new(dispatchDto);
				LoadDispatchPath(dispatch);
				_dispatches.Add(dispatch);
			}
		}

		public List<CDispatch> GetBusyDispatches()
		{
			long time = _serverTime.GetTimestampInMs();
			return _dispatches.Where(dispatch => dispatch.TripCompletionTime > time || dispatch.Type == EDispatchType.Resource).ToList();
		}

		public void Tick()
		{
			FireTripCompletedSignals();
			CompleteContractDispatches();
		}

		private void FireTripCompletedSignals()
		{
			long currentTime = _serverTime.GetTimestampInMs();
			foreach (CDispatch dispatch in _dispatches)
			{
				if (_tripCompletedFired.Contains(dispatch.Uid))
					continue;

				if (dispatch.TripCompletionTime > currentTime)
					continue;

				_tripCompletedFired.Add(dispatch.Uid);
				_eventBus.Send(new CDispatchTripCompletedSignal(dispatch));
			}
		}

		private void CompleteContractDispatches()
		{
			for (int i = 0; i < _dispatches.Count; i++)
			{
				CDispatch dispatch = _dispatches[i];

				bool completed = IsDispatchCompleted(dispatch);
				if (!completed)
					continue;

				bool shouldBeCompletedAutomatically = ShouldBeCompletedAutomatically(dispatch);
				if (!shouldBeCompletedAutomatically)
				{
					if (dispatch.Type == EDispatchType.Resource)
					{
						_eventBus.Send(new CResourceDispatchCompletionStateChangedSignal());
					}
					continue;
				}

				_dispatches.RemoveAt(i);
				_tripCompletedFired.Remove(dispatch.Uid);
				_eventBus.Send(new CDispatchCompletedSignal(dispatch.Uid));
			}
		}

		private bool ShouldBeCompletedAutomatically(CDispatch dispatch)
		{
			if (dispatch.Type != EDispatchType.Resource)
				return true;

			bool isPassengerDispatch = dispatch.ResourceData.ResourceToCollect.Id == EResource.Passenger;
			return isPassengerDispatch;
		}

		public bool IsDispatchCompleted(CDispatch dispatch)
		{
			bool completed = dispatch.IsCompleted(_serverTime.GetTimestampInMs());
			return completed;
		}

		public bool IsDispatchTripCompleted(CDispatch dispatch)
		{
			bool tripCompleted = dispatch.IsTripCompleted(_serverTime.GetTimestampInMs());
			return tripCompleted;
		}

		public bool IsVehicleDispatched(EVehicle vehicle)
		{
			foreach (CDispatch dispatch in _dispatches)
			{
				if (dispatch.Type == EDispatchType.TransportFleet)
				{
					foreach (EVehicle fleetVehicle in dispatch.FleetData.Vehicles)
					{
						if (fleetVehicle == vehicle) return true;
					}
				}
				else
				{
					if (dispatch.VehicleId == vehicle) return true;
				}
			}
			return false;
		}

		public bool IsVehicleBusy(EVehicle vehicle)
		{
			long currentTime = _serverTime.GetTimestampInMs();
			foreach (CDispatch dispatch in _dispatches)
			{
				bool tripCompleted = dispatch.TripCompletionTime <= currentTime;
				bool waitingForClaim = tripCompleted && dispatch.Type == EDispatchType.Resource;
				if (tripCompleted && !waitingForClaim)
					continue;
				
				if (dispatch.Type == EDispatchType.TransportFleet)
				{
					foreach (EVehicle fleetVehicle in dispatch.FleetData.Vehicles)
					{
						if (fleetVehicle == vehicle) return true;
					}
				}
				else
				{
					if (dispatch.VehicleId == vehicle) return true;
				}
			}
			return false;
		}

		public bool IsAnyVehicleDispatched()
		{
			return _dispatches.Count > 0;
		}

		public bool AnyResourceDispatchExists()
		{
			return _dispatches.Any(dispatch => dispatch.Type == EDispatchType.Resource);
		}

		public CDispatch[] GetCompletedResourceDispatches()
		{
			CDispatch[] completedDispatches = _dispatches
				.Where(dispatch => dispatch.Type == EDispatchType.Resource)
				.Where(IsDispatchCompleted)
				.ToArray();
			return completedDispatches;
		}

		public void ClaimResourceDispatch(EVehicle vehicle)
		{
			CDispatch dispatch = GetDispatchForVehicle(vehicle);
			if(dispatch.Type != EDispatchType.Resource)
				return;

			ClaimDispatch(dispatch);

			_eventBus.Send(new CResourceDispatchCollectedSignal(dispatch));

			_hitBuilder.GetBuilder(new CCollectDispatchedVehicleRequest(dispatch.Uid)).BuildAndSend();
		}

		public void SpeedupDispatch(string dispatchUid)
		{
			IValuable price = GetDispatchSpeedUpPrice(dispatchUid);
			_rewardQueue.ChargeValuable(EModificationSource.VehicleSpeedupContract, new []{price});

			CDispatch dispatch = GetDispatch(dispatchUid);
			ClaimDispatch(dispatch);

			_eventBus.Send(new CDispatchSpeedUpSignal(dispatch));

			_hitBuilder.GetBuilder(new CSpeedupDispatchRequest(dispatchUid)).BuildAndSend();
		}

		private void ClaimDispatch(CDispatch dispatch)
		{
			_dispatches.Remove(dispatch);
			_tripCompletedFired.Remove(dispatch.Uid);
			_eventBus.Send(new CDispatchCompletedSignal(dispatch.Uid));

			if (dispatch.Type != EDispatchType.Resource)
				return;

			SResource reward = dispatch.ResourceData.ResourceToCollect;
			IValuable rewardValuable = CValuableFactory.Resource(reward.Id, reward.Amount);
			_rewardQueue.AddRewards(EModificationSource.ClaimDispatch, new[] { rewardValuable });
			_eventBus.Send(new CResourceDispatchCompletedSignal(dispatch.Uid, reward));
		}

		public IValuable GetDispatchSpeedUpPrice(string dispatchUid)
		{
			CDispatch dispatch = GetDispatch(dispatchUid);
			long remainingTime = dispatch.TripCompletionTime - _serverTime.GetTimestampInMs();
			return CDesignVehicleConfigs.GetSpeedupPriceVehicle(remainingTime);
		}

		public long GetRemainingDispatchTimeInMs(string dispatchUid)
		{
			CDispatch dispatch = GetDispatch(dispatchUid);
			long currentTime = _serverTime.GetTimestampInMs();
			long remainingTime = dispatch.TripCompletionTime - currentTime;
			return Math.Max(0, remainingTime);
		}

		public int GetActiveDispatchesCount()
		{
			long time = _serverTime.GetTimestampInMs();
			return _dispatches.Count(dispatch => dispatch.TripCompletionTime > time || dispatch.Type == EDispatchType.Resource);
		}

		private CDispatch GetDispatch(string uid)
		{
			return _dispatches.First(dispatch => dispatch.Uid == uid);
		}

		public CDispatch GetDispatchOrDefault(string uid)
		{
			return _dispatches.FirstOrDefault(dispatch => dispatch.Uid == uid);
		}

		public CDispatch GetDispatchForVehicle(EVehicle vehicleId)
		{
			return _dispatches.First(dispatch => dispatch.VehicleId == vehicleId);
		}

		public CDispatch GetDispatchForVehicleOrDefault(EVehicle vehicleId)
		{
			return _dispatches.FirstOrDefault(dispatch => dispatch.VehicleId == vehicleId);
		}

		public bool AnyDispatchGoingToContract(EStaticContractId id)
		{
			long currentTime = _serverTime.GetTimestampInMs();
			foreach (CDispatch dispatch in _dispatches)
			{
				bool matchesContract = dispatch.ContractData!= null && dispatch.ContractData.Contract == id;
				if (!matchesContract) 
					continue;
				if (dispatch.TripCompletionTime > currentTime) 
					return true;
			}
			return false;
		}

		public bool AnyDispatchGoingToCity(ECity cityId)
		{
			long currentTime = _serverTime.GetTimestampInMs();
			foreach (CDispatch dispatch in _dispatches)
			{
				if (dispatch.Type != EDispatchType.Passenger) continue;
				if (dispatch.PassengerData.City != cityId) continue;
				if (dispatch.TripCompletionTime > currentTime) return true;
			}
			return false;
		}

		public bool ExistsForContract(EStaticContractId id)
		{
			foreach (CDispatch dispatch in _dispatches)
			{
				if (dispatch.Type == EDispatchType.Contract && dispatch.ContractData.Contract == id)
					return true;
				if (dispatch.Type == EDispatchType.TransportFleet && dispatch.FleetData.ContractId == id)
					return true;
			}
			return false;
		}

		public bool ExistsForCity(ECity id)
		{
			foreach (CDispatch dispatch in _dispatches)
			{
				if (dispatch.Type != EDispatchType.Passenger)
					continue;

				if (dispatch.PassengerData.City != id)
					continue;

				return true;
			}
			return false;
		}

		public IEnumerable<CDispatch> GetDispatchesInRegion(ERegion region)
		{
			foreach (CDispatch dispatch in _dispatches)
			{
				yield return dispatch;
			}
		}

		public CDispatch[] GetDispatchesForContract(EStaticContractId id)
		{
			return _dispatches
				.Where(dispatch =>
					(dispatch.Type == EDispatchType.Contract && dispatch.ContractData.Contract == id)
					|| (dispatch.Type == EDispatchType.TransportFleet && dispatch.FleetData.ContractId == id))
				.ToArray();
		}

		public CDispatch GetFirstDispatchForContract(EStaticContractId id)
		{
			return _dispatches.First(dispatch => dispatch.Type == EDispatchType.Contract && dispatch.ContractData.Contract == id);
		}

		public CDispatch[] GetDispatchesForResource(EResource id)
		{
			return _dispatches.Where(dispatch => dispatch.Type == EDispatchType.Resource && dispatch.ResourceData.ResourceToCollect.Id == id).ToArray();
		}

		public EVehicle GetLastSentVehicleForContract(EStaticContractId contract)
		{
			CDispatch dispatchForContract = GetFirstDispatchForContract(contract);
			return dispatchForContract?.VehicleId ?? EVehicle.None;
		}

		public CDispatch[] GetDispatchesForCity(ECity cityId)
		{
			CDispatch[] dispatches = _dispatches
				.Where(dispatch => dispatch.Type == EDispatchType.Passenger && dispatch.PassengerData.City == cityId)
				.ToArray();
			return dispatches;
		}

		public void AddContractDispatch(EStaticContractId contractId, EVehicle vehicleId, int resourceAmount)
		{
			CContract contract = User.Contracts.GetStaticContract(contractId);

			bool canAffordDispatch = CanAffordDispatch(contractId, vehicleId);
			if (!canAffordDispatch)
			{
				throw new Exception($"Cannot afford dispatch for contract {contractId} with vehicle {vehicleId}");
			}

			string uid = GetNewDispatchUid();
			COwnedVehicle vehicle = User.Vehicles.GetVehicle(vehicleId);
			long startTime = _dispatchDepo.GetAndProcessNextDispatchTimeInMs(vehicle.MovementType, User);

			CTripPrice tripPrice = GetTripPrice(contractId);
			ChargeVehicleDispatchPrice(vehicleId, tripPrice);

			SDispatchTime travelTime = _dispatchTimeProvider.GetTravelTime(contractId, vehicleId, User.Progress.Region, contract.Type);
			SUnixTime targetArrivalTime = startTime + travelTime.TravelToTime;
			SUnixTime completionTime = startTime + travelTime.TotalTravelTime;

			CDispatch dispatch = new(
				EDispatchType.Contract,
				uid,
				vehicleId,
				startTime,
				targetArrivalTime,
				travelTime.WaitAtDestinationTime,
				completionTime,
				contractData: new CContractDispatchData(contractId, resourceAmount)
			);

			LoadDispatchPath(dispatch);
			TryCancelDispatch(vehicleId);

			_dispatches.Add(dispatch);
			_eventBus.Send(new CVehicleDispatchedSignal(dispatch));

			_hitBuilder.GetBuilder(new CDispatchToStoryContractRequest(uid, vehicleId, contract.Uid, startTime, resourceAmount))
				.BuildAndSend();
		}

		private void TryCancelDispatch(EVehicle vehicle)
		{
			CDispatch dispatch = _dispatches.FirstOrDefault(dispatch => dispatch.VehicleId == vehicle);
			if(dispatch == null)
				return;
			_dispatches.Remove(dispatch);
			_tripCompletedFired.Remove(dispatch.Uid);
			_eventBus.Send(new CDispatchCancelledSignal(dispatch));
		}

		private void LoadDispatchPath(CDispatch dispatch)
		{
			if (dispatch.Type == EDispatchType.TransportFleet)
			{
				foreach (EVehicle vehicleId in dispatch.FleetData.Vehicles)
				{
					CContract c = User.Contracts.GetStaticContract(dispatch.FleetData.ContractId);
					CTrafficPath path = _dispatchPathFactory.GetPath(
						dispatch.FleetData.ContractId, vehicleId, ERegion.Region1,
						dispatch.DispatchStartTime, c.Type);
					dispatch.FleetData.LoadVehiclePath(vehicleId, path);
				}
				return;
			}

			CTrafficPath singlePath = dispatch.Type switch
			{
				EDispatchType.Contract => _dispatchPathFactory.GetPath(dispatch.ContractData.Contract, dispatch.VehicleId, ERegion.Region1, dispatch.DispatchStartTime,
				User.Contracts.GetStaticContract(dispatch.ContractData.Contract).Type),
				EDispatchType.Passenger => _dispatchPathFactory.GetPath(dispatch.PassengerData.City, dispatch.VehicleId, ERegion.Region1, dispatch.DispatchStartTime),
				EDispatchType.Resource => _dispatchPathFactory.GetPath(dispatch.ResourceData.ResourceToCollect.Id, dispatch.VehicleId, ERegion.Region1, dispatch.DispatchStartTime),
				_ => throw new ArgumentOutOfRangeException()
			};
			dispatch.LoadPath(singlePath);
		}

		public SDispatchTime GetDispatchTravelTimeForResource(EResource resourceId, EMovementType movementType)
		{
			SDispatchTime travelTime = _dispatchTimeProvider.GetTravelTime(resourceId, movementType, User.Progress.Region);

			if (User.Tutorials.IsTutorialCompleted(EDispatchCenterTutorialStep.Completed))
				return travelTime;

			SDispatchTime tutorialDispatchTime = new(7600, 13000, 0);
			return tutorialDispatchTime;
		}

		public CTripPrice GetTripPrice(EStaticContractId contractId)
		{
			CContract activeContract = User.Contracts.GetStaticContract(contractId);
			return activeContract.TripPrice;
		}

		public CTripPrice GetTripPrice(EResource resourceId)
		{
			CResourceIndustryConfig industryConfig = _industryConfigs.GetConfig(resourceId);
			return industryConfig.TripPrice;
		}

		public CTripPrice GetTripPrice(ECity cityId)
		{
			CCityConfig cityConfig = _cityConfigs.GetCityConfig(cityId);
			return cityConfig.TripPrice;
		}

		public void AddFleetDispatch(EStaticContractId contractId, EVehicle[] vehicles)
		{
			CContract contract = User.Contracts.GetStaticContract(contractId);
			string uid = GetNewDispatchUid();
			long startTime = _serverTime.GetTimestampInMs();

			long[] travelToTimes = new long[vehicles.Length];
			long[] travelFromTimes = new long[vehicles.Length];
			long maxTravelToTime = 0;
			long maxWaitAtDestTime = 0;
			long maxTravelFromTime = 0;

			for (int i = 0; i < vehicles.Length; i++)
			{
				SDispatchTime t = _dispatchTimeProvider.GetTravelTime(contractId, vehicles[i], User.Progress.Region, contract.Type);
				travelToTimes[i] = t.TravelToTime;
				travelFromTimes[i] = t.TravelFromTime;
				if (t.TravelToTime > maxTravelToTime) maxTravelToTime = t.TravelToTime;
				if (t.WaitAtDestinationTime > maxWaitAtDestTime) maxWaitAtDestTime = t.WaitAtDestinationTime;
				if (t.TravelFromTime > maxTravelFromTime) maxTravelFromTime = t.TravelFromTime;
			}

			foreach (EVehicle vehicleId in vehicles)
			{
				CTripPrice tripPrice = GetTripPrice(contractId);
				ChargeVehicleDispatchPrice(vehicleId, tripPrice);
			}

			long targetArrivalTime = startTime + maxTravelToTime + maxWaitAtDestTime;
			long completionTime = targetArrivalTime + maxTravelFromTime;

			CFleetDispatchData fleetData = new(vehicles, contractId, contract.Uid);
			CDispatch dispatch = new(
				EDispatchType.TransportFleet, uid, EVehicle.None,
				startTime, targetArrivalTime, 0, completionTime,
				fleetData: fleetData
			);

			LoadDispatchPath(dispatch);
			
			_dispatches.Add(dispatch);
			_eventBus.Send(new CVehicleDispatchedSignal(dispatch));

			_hitBuilder.GetBuilder(new CDispatchTransportFleetRequest(
				uid, contract.Uid, vehicles, startTime, travelToTimes, travelFromTimes
			)).BuildAndSend();
		}

		public void AddPassengerDispatch(ECity cityId, EVehicle vehicleId, int resourceAmount)
		{
			bool canAffordDispatch = CanAffordDispatch(cityId, vehicleId);
			if (!canAffordDispatch)
			{
				throw new Exception($"Cannot afford dispatch for contract {cityId} with vehicle {vehicleId}");
			}

			string uid = GetNewDispatchUid();

			COwnedVehicle vehicle = User.Vehicles.GetVehicle(vehicleId);
			long startTime = _dispatchDepo.GetAndProcessNextDispatchTimeInMs(vehicle.MovementType, User);
			int vehicleCapacity = User.Vehicles.GetCapacity(vehicleId);
			SDispatchTime travelTime = _dispatchTimeProvider.GetTravelTime(cityId, vehicle.MovementType, User.Progress.Region);

			SResource resource = new(EResource.Passenger, vehicleCapacity);

			CCityConfig cityConfig = _cityConfigs.GetCityConfig(cityId);
			ChargeVehicleDispatchPrice(vehicleId, cityConfig.TripPrice);

			long targetArrivalTime = startTime + travelTime.TravelToTime;
			long completionTime = startTime + travelTime.TotalTravelTime;

			CContract contract = User.Contracts.GetPassengerContract(cityId);

			CDispatch dispatch = new(
				EDispatchType.Passenger,
				uid,
				vehicleId,
				startTime,
				targetArrivalTime,
				travelTime.WaitAtDestinationTime,
				completionTime,
				passengerData: new CPassengerDispatchData(cityId, resource.Amount)
			);

			LoadDispatchPath(dispatch);
			TryCancelDispatch(vehicleId);

			_dispatches.Add(dispatch);
			_eventBus.Send(new CVehicleDispatchedSignal(dispatch));

			_hitBuilder.GetBuilder(new CDispatchToPassengerContractRequest(uid, vehicleId, contract.Uid, startTime, resourceAmount))
				.BuildAndSend();
		}

		public void AddResourceDispatch(EResource resourceId, EVehicle vehicleId)
		{
			bool canAffordDispatch = CanAffordDispatch(resourceId, vehicleId);
			if (!canAffordDispatch)
			{
				throw new Exception($"Cannot afford dispatch for resource {resourceId} with vehicle {vehicleId}");
			}

			string uid = GetNewDispatchUid();

			COwnedVehicle vehicle = User.Vehicles.GetVehicle(vehicleId);
			long startTime = _dispatchDepo.GetAndProcessNextDispatchTimeInMs(vehicle.MovementType, User);

			int vehicleCapacity = User.Vehicles.GetCapacity(vehicleId);

			SResource resource = new(resourceId, vehicleCapacity);

			CResourceIndustryConfig industryConfig = _industryConfigs.GetConfig(resourceId);
			ChargeVehicleDispatchPrice(vehicleId, industryConfig.TripPrice);

			SDispatchTime travelTime = GetDispatchTravelTimeForResource(resourceId, vehicle.MovementType);

			long targetArrivalTime = startTime + travelTime.TravelToTime;
			long completionTime = startTime + travelTime.TotalTravelTime;

			CDispatch dispatch = new(
				EDispatchType.Resource,
				uid,
				vehicleId,
				startTime,
				targetArrivalTime,
				travelTime.WaitAtDestinationTime,
				completionTime,
				resourceData: new CResourceDispatchData(resource)
			);

			LoadDispatchPath(dispatch);
			TryCancelDispatch(vehicleId);

			_dispatches.Add(dispatch);
			_eventBus.Send(new CVehicleDispatchedSignal(dispatch));

			_hitBuilder.GetBuilder(new CDispatchForResourceRequest(
					uid, vehicleId, resourceId, startTime, travelTime.TravelToTime, travelTime.WaitAtDestinationTime, travelTime.TravelFromTime))
				.BuildAndSend();
		}

		public bool CanAffordDispatch(ECity cityId, EVehicle vehicleId)
		{
			CCityConfig cityConfig = _cityConfigs.GetCityConfig(cityId);
			int fuelPrice = User.Vehicles.GetFuelEfficiency(vehicleId, cityConfig.TripPrice.FuelPriceValue);
			bool haveEnoughFuel = User.OwnedValuables.HaveValuable(CValuableFactory.Fuel(fuelPrice));
			return haveEnoughFuel;
		}

		public bool CanAffordDispatch(EResource resourceId, EVehicle vehicleId)
		{
			CResourceIndustryConfig industryConfig = _industryConfigs.GetConfig(resourceId);
			int fuelPrice = User.Vehicles.GetFuelEfficiency(vehicleId, industryConfig.TripPrice.FuelPriceValue);
			bool haveEnoughFuel = User.OwnedValuables.HaveValuable(CValuableFactory.Fuel(fuelPrice));
			return haveEnoughFuel;
		}

		public bool CanAffordDispatch(EStaticContractId contractId, EVehicle vehicleId)
		{
			CTripPrice tripPrice = GetTripPrice(contractId);
			int fuelPrice = User.Vehicles.GetFuelEfficiency(vehicleId, tripPrice.FuelPriceValue);
			bool haveEnoughFuel = User.OwnedValuables.HaveValuable(CValuableFactory.Fuel(fuelPrice));
			return haveEnoughFuel;
		}

		public CResourceValuable GetDispatchedResource(EVehicle vehicleId)
		{
			CDispatch dispatch = GetDispatchForVehicle(vehicleId);
			switch (dispatch.Type)
			{
				case EDispatchType.Resource:
				{
					CResourceValuable resource = CValuableFactory.Resource(dispatch.ResourceData.ResourceToCollect.Id, dispatch.ResourceData.ResourceToCollect.Amount);
					return resource;
				}
				case EDispatchType.Passenger:
					return CValuableFactory.Resource(EResource.Passenger, dispatch.PassengerData.ResourceAmount);
				case EDispatchType.Contract:
				{
					CContract activeContract = User.Contracts.GetStaticContract(dispatch.ContractData.Contract);
					EResource resourceId = activeContract.Requirement.Id;
					int amount = dispatch.ContractData.ResourceAmount;

					CResourceValuable resource = CValuableFactory.Resource(resourceId, amount);
					return resource;
				}
			}
			throw new Exception("Unsupported dispatch type");
		}

		private void ChargeVehicleDispatchPrice(EVehicle vehicleId, CTripPrice tripPrice)
		{
			COwnedVehicle vehicle = User.Vehicles.GetVehicle(vehicleId);
			int fuelPrice = vehicle.GetFuelEfficiency(vehicleId, tripPrice);
			_rewardQueue.ChargeValuable(EModificationSource.StoryContractDispatch, new IValuable[]{CValuableFactory.Fuel(fuelPrice)});
			vehicle.DecreaseDurability(tripPrice.DurabilityPrice, _serverTime.GetTimestampInMs());
		}

		private string GetNewDispatchUid()
		{
			return Guid.NewGuid().ToString();
		}

		public EVehicle[] GetBusyDispatchedVehicles()
		{
			long currentTime = _serverTime.GetTimestampInMs();
			List<EVehicle> vehicles = new();
			foreach (CDispatch dispatch in _dispatches)
			{
				bool isCompleted = dispatch.TripCompletionTime <= currentTime;
				bool waitingForClaim = isCompleted && dispatch.Type == EDispatchType.Resource;
				
				if (isCompleted && !waitingForClaim)
					continue;
				
				if (dispatch.Type == EDispatchType.TransportFleet)
				{
					foreach (EVehicle v in dispatch.FleetData.Vehicles)
					{
						vehicles.Add(v);
					}
				}
				else
				{
					vehicles.Add(dispatch.VehicleId);
				}
			}
			return vehicles.ToArray();
		}
	}
}
