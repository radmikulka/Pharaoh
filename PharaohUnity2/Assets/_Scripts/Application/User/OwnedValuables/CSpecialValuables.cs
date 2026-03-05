// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using AldaEngine;
using ServerData;
using ServerData.Design;
using UnityEngine;

namespace TycoonBuilder
{
	public class CSpecialValuables
	{
		private readonly CDesignVehicleConfigs _vehicleConfigs;

		public CSpecialValuables(CDesignVehicleConfigs vehicleConfigs)
		{
			_vehicleConfigs = vehicleConfigs;
		}

		public bool TryHandleSpecialValuable(IValuable valuable, CUser user, long timestampInMs, CValueModifyParams modifyParams)
		{
			switch (valuable)
			{
				case CBuildingValuable building:
					user.City.AddOwnedBuilding(building.Building, true, false);
					return true;
				case CFrameValuable frame:
					user.OwnedFrames.Add(frame.Frame);
					return true;
				case CVehicleValuable vehicle:
					CVehicleChangeParam obtainSource = modifyParams?.GetParamOrDefault<CVehicleChangeParam>();
					CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(vehicle.Vehicle);
					ERegion region = vehicleConfig.Region != ERegion.None ? vehicleConfig.Region : user.Progress.Region;
					user.Vehicles.AddNewVehicle(vehicle.Vehicle, region, obtainSource?.Source ?? EVehicleObtainSource.None);
					return true;
				case CResourceValuable resource:
					if (resource.Resource.Id == EResource.Passenger)
					{
						CObservableRecharger passengerGenerator = user.City.GetPassengersGenerator(timestampInMs);
						passengerGenerator.Update(timestampInMs);
						passengerGenerator.ModifyOverCapacity(resource.Resource.Amount, timestampInMs, modifyParams);
						return true;
					}
					user.Warehouse.AddResource(resource.Resource);
					return true;
				case CDispatcherValuable dispatcher:
					long? expiration = timestampInMs + dispatcher.ExpirationDurationIsSecs * CTimeConst.Second.InMilliseconds;
					user.Dispatchers.AddDispatcher(dispatcher.Dispatcher, expiration);
					return true;
				case CConsumableValuable { Id: EValuable.Fuel } consumable:
					if (consumable.Value < 0)
					{
						consumable = consumable.Reverse();
						user.FuelStation.Remove(consumable.Value, timestampInMs);
					}
					else
					{
						user.FuelStation.ModifyOverCapacity(consumable.Value, timestampInMs, modifyParams);
					}
					return true;
				case CEventCoinValuable eventCoin:
					user.LiveEvents.AddEventCoins(eventCoin);
					return true;
				case CEventPointValuable eventPointValuable:
					user.LiveEvents.AddEventPoints(eventPointValuable);
					return true;
				default: return false;
			}
		}

		public bool? HaveValuable(IValuable valuable, CUser user, long timestamp)
		{
			switch (valuable)
			{
				case CEventPointValuable eventPoint:
					return user.LiveEvents.HaveEventPoint(eventPoint);
				case CEventCoinValuable eventCoin:
					return user.LiveEvents.HaveEventCoin(eventCoin);
				case CBuildingValuable building:
					return user.City.HaveBuilding(building.Building);
				case CFrameValuable frame:
					return user.OwnedFrames.Contains(frame.Frame);
				case CDispatcherValuable dispatcher:
					return user.Dispatchers.HaveDispatcher(dispatcher.Dispatcher, timestamp);
				case CFreeValuable:
				case CFreeNoHitValuable:
					return true;
				case CVehicleValuable vehicle:
					return user.Vehicles.IsVehicleOwned(vehicle.Vehicle);
				case CResourceValuable resource:
					return user.Warehouse.HaveResource(resource.Resource);
				case CConsumableValuable { Id: EValuable.Fuel } consumable:
					return user.FuelStation.HaveFuel(consumable.Value);
				default: return null;
			}
		}
	}
}