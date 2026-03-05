// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.10.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using ServerData.Dto;
using UnityEngine;
using UnityEngine.UIElements;

namespace TycoonBuilder
{
	[ValidatableData]
	public class CRechargerValidator : CBaseUserComponent, IIsConsistent
	{
		private readonly Dictionary<EFactory, CModifiedFactoryRechargerDto> _factories = new();
		private readonly Dictionary<EVehicle, CModifiedVehicleRechargerDto> _vehicles = new();
		private readonly IServerTime _serverTime;
		private CRechargerDto _fuel;
		private CRechargerDto _city;

		public CRechargerValidator(IServerTime serverTime)
		{
			_serverTime = serverTime;
		}

		public void Sync(CModifiedUserDataDto modifiedData)
		{
			CModifiedRechargersDto data = modifiedData.Rechargers;
			if(data == null)
				return;
			
			AddData(data);
		}

		private void AddData(CModifiedRechargersDto data)
		{
			if(data.Fuel != null)
			{
				_fuel = data.Fuel;
			}
			
			if(data.City != null)
			{
				_city = data.City;
			}

			foreach (CModifiedFactoryRechargerDto factory in data.Factories)
			{
				_factories[factory.FactoryId] = factory;
			}
			
			foreach (CModifiedVehicleRechargerDto vehicle in data.Vehicles)
			{
				_vehicles[vehicle.VehicleId] = vehicle;
			}
		}

		private void Validate()
		{
			if (_fuel != null && !User.FuelStation.Recharger.CompareWithDto(_fuel))
			{
				Debug.LogError($"Inconsistent Fuel Recharger Data {_fuel} | {User.FuelStation.Recharger}");
			}

			CCityData cityData = User.City.GetOrCreateCityData();
			if(_city != null && !cityData.PassengersGenerator.CompareWithDto(_city))
			{
				Debug.LogError($"Inconsistent City Recharger Data dto - {_city} | {cityData.PassengersGenerator} - " +
				               $"diff {(_city.LastTickTime - cityData.PassengersGenerator.LastTickTime) / CTimeConst.Second.InMilliseconds}sec " +
				               $"now is {CUnixTime.GetDate(_serverTime.GetTimestampInMs())}");
			}

			foreach (var factory in _factories)
			{
				CFactory userFactory = User.Factories.GetOrCreateFactory(factory.Key);
				if (!userFactory.Durability.CompareWithDto(factory.Value))
				{
					Debug.LogError($"Inconsistent Factory Recharger Data {factory.Value} | {userFactory.Durability}");
				}
			}

			foreach (var vehicle in _vehicles)
			{
				COwnedVehicle userVehicle = User.Vehicles.GetVehicle(vehicle.Key);

				if (!userVehicle.CompareDurability(vehicle.Value))
				{
					Debug.LogError($"Inconsistent Vehicle Recharger Data dto - {vehicle.Value} vs data - {userVehicle}");
				}
			}
		}

		private void Clear()
		{
			_fuel = null;
			_city = null;
			_factories.Clear();
			_vehicles.Clear();
		}

		public bool IsConsistent()
		{
			Validate();
			Clear();
			return true;
		}
	}
}