// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;
using ServerData.Hits;
using UnityEngine;

namespace TycoonBuilder
{
	public class COwnedVehicles : CBaseUserComponent
	{
		private CRepairAmountPerTickProvider _repairAmountPerTickProvider;
		
		private readonly CDesignResourceConfigs _designResourceConfigs;
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly CResourceConfigs _resourceConfigs;
		private readonly CVehiclesFactory _vehiclesFactory;
		private readonly IBundleManager _bundleManager;
		private readonly IRewardQueue _rewardQueue;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly CWorldMap _worldMap;

		private readonly Dictionary<EVehicle, COwnedVehicle> _vehicles = new();

		public int Count => _vehicles.Count;

		public COwnedVehicles(
			CDesignResourceConfigs designResourceConfigs,
			CDesignVehicleConfigs vehicleConfigs,
			CResourceConfigs resourceConfigs,
			CVehiclesFactory vehiclesFactory,
			IBundleManager bundleManager,
			IRewardQueue rewardQueue,
			CHitBuilder hitBuilder,
			IEventBus eventBus,
			CWorldMap worldMap)
		{
			_designResourceConfigs = designResourceConfigs;
			_vehicleConfigs = vehicleConfigs;
			_resourceConfigs = resourceConfigs;
			_vehiclesFactory = vehiclesFactory;
			_bundleManager = bundleManager;
			_rewardQueue = rewardQueue;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_worldMap = worldMap;
		}

		public override void Initialize(CUser user)
		{
			base.Initialize(user);
			
			_repairAmountPerTickProvider = new CRepairAmountPerTickProvider(user);
		}

		public void InitialSync(CVehiclesDto dto)
		{
			foreach (CVehicleDto vehicleDto in dto.Vehicles)
			{
				AddExistingVehicle(vehicleDto);
			}
		}
		
		public COwnedVehicle GetOrCreateVehicle(EVehicle vehicleId)
		{
			if (!_vehicles.TryGetValue(vehicleId, out COwnedVehicle vehicle))
			{
				vehicle = _vehiclesFactory.NewVehicle(vehicleId, User.Progress.Region, _repairAmountPerTickProvider, false);
				_vehicles.Add(vehicleId, vehicle);
			}
			return vehicle;
		}

		public COwnedVehicle GetVehicle(EVehicle vehicleId)
		{
			return _vehicles[vehicleId];
		}
		
		public COwnedVehicle GetVehicleOrDefault(EVehicle vehicleId)
		{
			_vehicles.TryGetValue(vehicleId, out COwnedVehicle vehicle);
			return vehicle;
		}

		public COwnedVehicle[] GetAllOwnedVehicles()
		{
			return _vehicles.Values.Where(vehicle => vehicle.IsOwned).ToArray();
		}
		
		private void AddExistingVehicle(CVehicleDto dto)
		{
			COwnedVehicle newVehicle = _vehiclesFactory.ExistingVehicle(dto, _repairAmountPerTickProvider);
			_vehicles.Add(newVehicle.Id, newVehicle);
		}

		public void AddNewVehicle(EVehicle vehicle, ERegion region, EVehicleObtainSource obtainSource)
		{
			COwnedVehicle alreadyOwnedVehicle = GetVehicleOrDefault(vehicle);
			if (alreadyOwnedVehicle != null)
			{
				alreadyOwnedVehicle.SetIsOwned(true);
			}
			else
			{
				COwnedVehicle newVehicle = _vehiclesFactory.NewVehicle(vehicle, region, _repairAmountPerTickProvider, true);
				_vehicles.Add(vehicle, newVehicle);
			}
			_eventBus.Send(new CVehicleAddedSignal(vehicle, obtainSource));
		}
		
		public bool GetSeen(EVehicle vehicleId)
		{
			COwnedVehicle vehicle = GetVehicleOrDefault(vehicleId);
			return vehicle is { Seen: true };
		}

		public int GetDurability(EVehicle vehicleId)
		{
			int statValue = GetStat(vehicleId, EVehicleStat.Durability);
			return statValue;
		}

		public int GetCapacity(EVehicle vehicleId)
		{
			int statValue = GetCurrentCapacity(vehicleId);
			return statValue;
		}

		public int GetFuelEfficiency(EVehicle vehicleId, int tripPrice)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			int statValue = vehicle.GetStatValue(EVehicleStat.FuelEfficiency);
			int fuelEfficiency = _vehicleConfigs.GetFuelEfficiency(tripPrice, statValue);
			return fuelEfficiency;
		}
		
		public int GetMaximumCapacity(EVehicle vehicleId)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			int maxCapacity = vehicle.Config.GetStat(EVehicleStat.AdvancedCapacity, vehicle.Config.GetMaxStatLevel(EVehicleStat.AdvancedCapacity));
			return maxCapacity;
		}
		
		public bool AllBasicUpgradesDone(EVehicle vehicleId)
		{
			bool capacityFullyUpgraded = IsFullyUpgradedStat(vehicleId, EVehicleStat.Capacity);
			bool fuelEfficiencyFullyUpgraded = IsFullyUpgradedStat(vehicleId, EVehicleStat.FuelEfficiency);
			bool durabilityFullyUpgraded = IsFullyUpgradedStat(vehicleId, EVehicleStat.Durability);
			bool allUpgraded = capacityFullyUpgraded && fuelEfficiencyFullyUpgraded && durabilityFullyUpgraded;
			return allUpgraded;
		}
		
		public bool IsFullyUpgraded(EVehicle vehicleId)
		{
			bool allBasicUpgradesDone = AllBasicUpgradesDone(vehicleId);
			bool advancedCapacityFullyUpgraded = IsFullyUpgradedStat(vehicleId, EVehicleStat.AdvancedCapacity);
			bool allUpgraded = allBasicUpgradesDone && advancedCapacityFullyUpgraded;
			return allUpgraded;
		}
		
		public bool IsFullyUpgradedStat(EVehicle vehicleId, EVehicleStat stat)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			int statIndex = vehicle.GetStatIndex(stat);
			int maxStatLevel = vehicle.Config.GetMaxStatLevel(stat) - 1;
			if (stat == EVehicleStat.AdvancedCapacity)
			{
				maxStatLevel += 2;
			}
			return statIndex == maxStatLevel;
		}

		private int GetCurrentCapacity(EVehicle vehicleId)
		{
			int capacity = GetStat(vehicleId, EVehicleStat.Capacity);
			int advancedCapacityLevel = GetStatLevel(vehicleId, EVehicleStat.AdvancedCapacity);
			if (advancedCapacityLevel <= 0) 
				return capacity;
			
			int advancedCapacity = GetStat(vehicleId, EVehicleStat.AdvancedCapacity);
			return advancedCapacity;
		}

		private int GetStatLevel(EVehicle vehicleId, EVehicleStat advancedCapacity)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			int statLevel = vehicle.GetStatIndex(advancedCapacity);
			return statLevel;
		}

		private int GetStat(EVehicle vehicleId, EVehicleStat stat)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			int statValue = vehicle.GetStatValue(stat);
			return statValue;
		}

		public bool IsVehicleOwned(EVehicle vehicle)
		{
			COwnedVehicle ownedVehicle = GetVehicleOrDefault(vehicle);
			return ownedVehicle is { IsOwned: true };
		}

		public int GetUpgradeGain(EVehicle vehicleId, EVehicleStat stat)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			int currentLevel = vehicle.GetStatIndex(stat);
			int nextLevel = currentLevel + 1;
			int currentValue = vehicle.GetStatValue(stat);
			if (stat == EVehicleStat.AdvancedCapacity)
			{
				currentValue = GetCurrentCapacity(vehicleId);
				nextLevel--;
			}
			int nextValue = vehicle.Config.GetStat(stat, nextLevel);
			int gain = nextValue - currentValue;
			return gain;
		}

		public int GetCurrentLevel(EVehicle vehicleId)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			int level = vehicle.GetCurrentLevel();
			return level;
		}
		
		public int GetMaxLevel(EVehicle vehicleId)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			int maxLevel = vehicle.GetMaxLevel();
			return maxLevel;
		}
		
		public int MissingValuablesForUpgrade(EVehicle vehicleId, ERegion region)
		{
			bool upgradesLocked = VehicleUpgradesLocked();
			if (upgradesLocked)
				return 0;

			bool allBasicUpgradesDone = AllBasicUpgradesDone(vehicleId);

			EVehicleStat[] statsToCheck = allBasicUpgradesDone
				? new[] { EVehicleStat.Durability, EVehicleStat.FuelEfficiency, EVehicleStat.Capacity, EVehicleStat.AdvancedCapacity }
				: new[] { EVehicleStat.Durability, EVehicleStat.FuelEfficiency, EVehicleStat.Capacity };

			int bestTotalMissing = int.MaxValue;

			foreach (EVehicleStat stat in statsToCheck)
			{
				if (IsFullyUpgradedStat(vehicleId, stat))
					continue;

				int currentStatLevel = GetStatLevel(vehicleId, stat);
				IValuable[] prices = _vehicleConfigs.GetUpgradePrice(currentStatLevel, stat, region).ToArray();

				int totalMissing = 0;
				bool canAfford = true;

				foreach (IValuable price in prices)
				{
					if (price is not CConsumableValuable consumablePrice)
						continue;

					if (User.OwnedValuables.HaveValuable(consumablePrice))
						continue;

					canAfford = false;
					int owned = User.OwnedValuables.GetConsumable(consumablePrice.Id).Amount;
					int missingAmount = consumablePrice.Value - owned;
					totalMissing += missingAmount;
				}

				if (canAfford)
					return 0;

				if (totalMissing < bestTotalMissing)
				{
					bestTotalMissing = totalMissing;
				}
			}

			return bestTotalMissing;
		}
		
		public bool CanAffordAnyUpgrade(EVehicle vehicleId, ERegion region)
		{
			bool upgradesLocked = VehicleUpgradesLocked();
			if (upgradesLocked)
				return false;
			
			bool canAffordDurability = CanAffordToUpgradeStat(vehicleId, EVehicleStat.Durability, region);
			bool canAffordFuelEfficiency = CanAffordToUpgradeStat(vehicleId, EVehicleStat.FuelEfficiency, region);
			bool canAffordCapacity = CanAffordToUpgradeStat(vehicleId, EVehicleStat.Capacity, region);
			bool canAffordAdvancedCapacity = CanAffordToUpgradeStat(vehicleId, EVehicleStat.AdvancedCapacity, region);
			bool allBasicUpgradesDone = AllBasicUpgradesDone(vehicleId);
			bool canUpgradeAdvancedCapacity = allBasicUpgradesDone && canAffordAdvancedCapacity;
			return canAffordDurability || canAffordFuelEfficiency || canAffordCapacity || canUpgradeAdvancedCapacity;
		}

		public IEnumerable<COwnedVehicle> GetVehicles(EMovementType movementType, ETransportType transportType)
		{
			foreach (var vehicle in _vehicles)
			{
				bool isValidMovementType = movementType.HasFlag(vehicle.Value.MovementType);
				if(!isValidMovementType)
					continue;
				
				bool isValidTransportType = transportType.HasFlag(vehicle.Value.Config.TransportType);
				if(!isValidTransportType)
					continue;
				
				yield return vehicle.Value;
			}
		}

		public IEnumerable<ELiveEvent> GetRequiredEvents()
		{
			foreach (KeyValuePair<EVehicle, COwnedVehicle> vehicle in _vehicles)
			{
				if(vehicle.Value.Config.LiveEvent == ELiveEvent.None)
					continue;
				yield return vehicle.Value.Config.LiveEvent;
			}
		}

		public void RepairVehicle(EVehicle vehicleId, int repairAmount, ERegion regionId, long time)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			vehicle.RepairDurability(repairAmount, time);
			
			_eventBus.Send(new CVehicleRepairedSignal(vehicleId));
			_hitBuilder.GetBuilder(new CRepairVehicleRequest(vehicleId, repairAmount, regionId)).BuildAndSend();
		}

		private bool CanAffordToUpgradeStat(EVehicle vehicleId, EVehicleStat stat, ERegion region)
		{
			bool isFullyUpgradedStat = IsFullyUpgradedStat(vehicleId, stat);
			if (isFullyUpgradedStat)
				return false;

			int currentStatLevel = GetStatLevel(vehicleId, stat);
			IValuable[] prices = _vehicleConfigs.GetUpgradePrice(currentStatLevel, stat, region).ToArray();
			bool canAfford = prices.Select(price => User.OwnedValuables.HaveValuable(price)).All(canAfford => canAfford);
			return canAfford;
		}

		public int GetAvailableVehiclesCount(EMovementType movementType, ETransportType transportType)
		{
			int count = _vehicles.Values.Count(vehicle => movementType.HasFlag(vehicle.Config.MovementType)
			                                              && vehicle.Config.TransportType == transportType);
			return count;
		}
		
		public EVehicle[] GetAvailableVehicles(EMovementType movementType, ETransportType transportType)
		{
			EVehicle[] vehicles = _vehicles.Values
				.Where(vehicle => movementType.HasFlag(vehicle.Config.MovementType) && vehicle.Config.TransportType == transportType)
				.Select(vehicle => vehicle.Id).ToArray();
			return vehicles;
		}
		
		public int GetAvailableVehiclesCount(IContract contract, EResource resourceId)
		{
			ETransportType transportType = _designResourceConfigs.GetResourceConfig(resourceId).TransportType;
			int count = GetAvailableVehiclesCount(contract.MovementType, transportType);
			return count;
		}
		
		public Sprite GetVehicleSprite(EVehicle vehicleId)
		{
			CVehicleResourceConfig[] vehicleResourceConfigs = _resourceConfigs.Vehicles.GetConfigs().ToArray();
			CVehicleResourceConfig vehicleResourceConfig = vehicleResourceConfigs.FirstOrDefault(config => config.Id == vehicleId);
			Sprite icon = null;
			if (vehicleResourceConfig != null)
			{
				icon = _bundleManager.LoadItem<Sprite>(vehicleResourceConfig.Sprite);
			}
			else
			{
				Debug.LogError("Vehicle resource config not found for vehicle: " + vehicleId);
			}
			return icon;
		}

		public void BuyVehicle(EVehicle vehicleId, EVehicleObtainSource obtainSource)
		{
			IValuable vehiclePrice = GetVehiclePrice(vehicleId);

			_rewardQueue.ChargeValuable(EModificationSource.BuyVehicle, new []{vehiclePrice});
			CVehicleValuable vehicle = CValuableFactory.Vehicle(vehicleId);
			
			CValueModifyParams modifyParams= new CValueModifyParams().Add(new CVehicleChangeParam(obtainSource));
			_rewardQueue.AddRewards(EModificationSource.BuyVehicle, new IValuable[] { vehicle }, modifyParams);

			CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CBuyVehicleRequest(vehicleId));
			hit.BuildAndSend();
			
			_eventBus.Send(new CVehicleBoughtSignal(vehicleId));
		}

		private IValuable GetVehiclePrice(EVehicle vehicleId)
		{
			CVehicleConfig config = _vehicleConfigs.GetConfig(vehicleId);
			IValuable price = config.Price;
			return price;
		}

		public bool VehicleUpgradesLocked()
		{
			CContractUnlockRequirement upgradeRequirement = (CContractUnlockRequirement) CDesignVehicleConfigs.UpgradeUnlockRequirement;
			bool completed = User.Contracts.IsContractCompleted(upgradeRequirement.ContractId);
			return !completed;
		}

		public EVehicle[] GetVehiclesToUnlockAtYear(EYearMilestone yearMilestone)
		{
			Dictionary<EVehicle, CVehicleConfig> configs = _vehicleConfigs.GetConfigs();
			List<EVehicle> vehicles = new List<EVehicle>();

			foreach (KeyValuePair<EVehicle, CVehicleConfig> vehicleConfig in configs)
			{
				if (vehicleConfig.Value.UnlockRequirement is not CYearUnlockRequirement yearUnlockRequirement)
					continue;

				if (yearUnlockRequirement.Year != yearMilestone)
					continue;

				vehicles.Add(vehicleConfig.Key);
			}
			return vehicles.ToArray();
		}

		public bool IsVehicleUnlockedThisYear(EVehicle vehicleId, EYearMilestone yearMilestone)
		{
			CVehicleConfig config = _vehicleConfigs.GetConfig(vehicleId);
			if (config.UnlockRequirement is not CYearUnlockRequirement yearUnlockRequirement)
				return false;

			return yearUnlockRequirement.Year == yearMilestone;
		}

		public bool IsLocked(EVehicle vehicleId)
		{
			COwnedVehicle vehicle = GetVehicleOrDefault(vehicleId);
			if (vehicle == null)
			{
				CVehicleConfig config = _vehicleConfigs.GetConfig(vehicleId);
				bool isUnlocked = User.IsUnlockRequirementMet(config.UnlockRequirement);
				return !isUnlocked;
			}
			return vehicle.IsLocked(User);
		}
		
		public void UpgradeVehicle(EVehicle vehicleId, EVehicleStat stat, long time)
		{
			COwnedVehicle vehicle = GetVehicle(vehicleId);
			vehicle.IncreaseStatLevel(stat, time);
			User.Tasks.TryIncreaseProgress(ETaskRequirement.UpgradeVehicle);
		}

		public int GetFullyUpgradedVehiclesCount()
		{
			int count = _vehicles.Values.Count(vehicle => IsFullyUpgraded(vehicle.Id));
			return count;
		}

		public int GetUpgradesCount()
		{
			int result = 0;
			foreach (COwnedVehicle vehicle in GetAllOwnedVehicles())
			{
				foreach (EVehicleStat stat in new[] { EVehicleStat.Durability, EVehicleStat.Capacity, EVehicleStat.FuelEfficiency, EVehicleStat.AdvancedCapacity })
				{
					result += vehicle.GetStatIndex(stat) + 1;
				}
			}
			return result;
		}
	}
}
