// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using System;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CVehiclesFactory
	{
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;

		public CVehiclesFactory(CDesignVehicleConfigs vehicleConfigs, CHitBuilder hitBuilder, IEventBus eventBus)
		{
			_vehicleConfigs = vehicleConfigs;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
		}

		public COwnedVehicle NewVehicle(EVehicle id, ERegion region, CRepairAmountPerTickProvider repairAmountPerTickProvider, bool isOwned)
		{
			CVehicleDurabilityRecharger recharger = CVehicleDurabilityRecharger.New(id, _vehicleConfigs, repairAmountPerTickProvider);
			CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(id);
			return new COwnedVehicle(
				_vehicleConfigs, 
				vehicleConfig, 
				Array.Empty<SVehicleStat>(), 
				recharger, 
				_hitBuilder,
				_eventBus,
				false,
				isOwned,
				region
				);
		}

		public COwnedVehicle ExistingVehicle(CVehicleDto dto, CRepairAmountPerTickProvider repairAmountPerTickProvider)
		{
			int durabilityLevel = GetDurabilityLevel();
			CVehicleDurabilityRecharger recharger = CVehicleDurabilityRecharger.Existing(
				dto.Id, 
				_vehicleConfigs, 
				repairAmountPerTickProvider, 
				dto.Durability.CurrentAmount,
				dto.Durability.LastTickTime,
				durabilityLevel);
			
			CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(dto.Id);
			return new COwnedVehicle(
				_vehicleConfigs,
				vehicleConfig, 
				dto.Stats, 
				recharger, 
				_hitBuilder,
				_eventBus,
				dto.Seen,
				dto.IsOwned,
				dto.Region
				);

			int GetDurabilityLevel()
			{
				foreach (SVehicleStat stat in dto.Stats)
				{
					if(stat.Stat != EVehicleStat.Durability)
						continue;
					return stat.Level;
				}

				return 1;
			}
		}
	}
}