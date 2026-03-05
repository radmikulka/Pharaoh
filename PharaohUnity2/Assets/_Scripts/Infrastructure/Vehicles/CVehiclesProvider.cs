// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.08.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CVehiclesProvider
	{
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly CUser _user;
		
		public readonly ETransportType[] TransportTypesInOrder = { ETransportType.Bulk, ETransportType.Flat, ETransportType.Passenger, ETransportType.Container, ETransportType.Liquid };

		public CVehiclesProvider(CDesignVehicleConfigs vehicleConfigs, CUser user)
		{
			_vehicleConfigs = vehicleConfigs;
			_user = user;
		}
		
		public bool AnyVehicleExistsWithMovementType(ETransportType transportType, EMovementType movementType, ERegion[] validRegions)
		{
			bool anyVehicleExists = validRegions.Any(claimedRegion => AnyVehicleExistsWithMovementTypeInRegion(transportType, claimedRegion, movementType));
			return anyVehicleExists;
		}
		
		public bool AnyVehicleExistsInRegion(ETransportType transportType, ERegion region)
		{
			Dictionary<EVehicle, CVehicleConfig> configs = _vehicleConfigs.GetConfigs();
			EVehicle[] validVehiclesInRegion = GetValidVehiclesInRegion(region);
			return configs.Any(kvp => validVehiclesInRegion.Contains(kvp.Key) && kvp.Value.TransportType == transportType);
		}

		private EVehicle[] GetValidVehiclesInRegion(ERegion region)
		{
			Dictionary<EVehicle, CVehicleConfig> configs = _vehicleConfigs.GetConfigs();
			List<EVehicle> validVehicles = new();
			foreach (KeyValuePair<EVehicle, CVehicleConfig> kvp in configs)
			{
				CVehicleConfig vehicleConfig = kvp.Value;
				ERegion vehicleRegion = vehicleConfig.Region;
				if (vehicleRegion == ERegion.None)
				{
					COwnedVehicle ownedVehicle = _user.Vehicles.GetVehicleOrDefault(vehicleConfig.Id);
					if (ownedVehicle != null)
					{
						vehicleRegion = ownedVehicle.Region;
					}
				}
				
				if (vehicleRegion == region)
				{
					validVehicles.Add(vehicleConfig.Id);
				}
			}
			return validVehicles.ToArray();
		}
		
		private bool AnyVehicleExistsWithMovementTypeInRegion(ETransportType transportType, ERegion region, EMovementType movementType)
		{
			Dictionary<EVehicle, CVehicleConfig> configs = _vehicleConfigs.GetConfigs();
			EVehicle[] validVehiclesInRegion = GetValidVehiclesInRegion(region);
			return configs.Any(kvp => validVehiclesInRegion.Contains(kvp.Key) && kvp.Value.TransportType == transportType && movementType.HasFlag(kvp.Value.MovementType));
		}

		public EVehicle[] GetOrderedVehicleList(List<EVehicle> vehicles)
		{
			vehicles.Sort(GetVehicleOrder);
			return vehicles.ToArray();
		}
		
		private int GetVehicleOrder(EVehicle vehicleIdA, EVehicle vehicleIdB)
		{
			// by owned status
			bool ownedA = _user.Vehicles.IsVehicleOwned(vehicleIdA);
			bool ownedB = _user.Vehicles.IsVehicleOwned(vehicleIdB);
			if (ownedA != ownedB)
				return ownedA ? -1 : 1;

			// by capacity
			if (ownedA)
			{
				int capacityA = _user.Vehicles.GetCapacity(vehicleIdA);
				int capacityB = _user.Vehicles.GetCapacity(vehicleIdB);
				return capacityB.CompareTo(capacityA);
			}
				
			CVehicleConfig configA = _vehicleConfigs.GetConfig(vehicleIdA);
			CVehicleConfig configB = _vehicleConfigs.GetConfig(vehicleIdB);
			
			// by unlock level
			int unlockLevelA = (int)((configA.UnlockRequirement as CYearUnlockRequirement)?.Year ?? 0);
			int unlockLevelB = (int)((configB.UnlockRequirement as CYearUnlockRequirement)?.Year ?? 0);
			if (unlockLevelA != unlockLevelB)
				return unlockLevelA.CompareTo(unlockLevelB);
				
			// by price
			CConsumableValuable priceA = (CConsumableValuable)configA.Price;
			CConsumableValuable priceB = (CConsumableValuable)configB.Price;

			if (priceA.Id == priceB.Id)
			{
				return priceA.Value.CompareTo(priceB.Value);
			}

			return priceA.Id.CompareTo(priceB.Id);
		}
	}
}