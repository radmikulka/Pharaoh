// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.07.2025
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using ServerData;
using ServerData.Design;
using ServerData.Dto;
using ServerData.Hits;
using UnityEngine;

namespace TycoonBuilder
{
	public class COwnedVehicle
	{
		private readonly Dictionary<EVehicleStat, int> _statLevels = new();
		private readonly CVehicleDurabilityRecharger _durability;
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;

		public readonly CVehicleConfig Config;
		public readonly ERegion Region;
		public readonly EVehicle Id;
		
		public bool IsOwned { get; private set; }
		public bool Seen { get; private set;}
		
		public EMovementType MovementType => Config.MovementType;
		
		public COwnedVehicle(
			CDesignVehicleConfigs vehicleConfigs, 
			CVehicleConfig config, 
			SVehicleStat[] stats, 
			CVehicleDurabilityRecharger durability, 
			CHitBuilder hitBuilder,
			IEventBus eventBus,
			bool seen,
			bool isOwned,
			ERegion region
			)
		{
			_vehicleConfigs = vehicleConfigs;
			_durability = durability;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			Config = config;
			Region = region;
			Id = config.Id;
			Seen = seen;
			IsOwned = isOwned;

			foreach (SVehicleStat stat in stats)
			{
				_statLevels.Add(stat.Stat, stat.Level - 1);
			}
		}
		
		public int GetStatIndex(EVehicleStat id)
		{
			return _statLevels.GetValueOrDefault(id, 0);
		}

		public int GetStatValue(EVehicleStat id)
		{
			int level = _statLevels.GetValueOrDefault(id, 0);
			if (id == EVehicleStat.AdvancedCapacity)
			{
				level--;
				level = CMath.Max(0, level);
			}
			int value = Config.GetStat(id, level);
			return value;
		}
		
		public void SetSeen()
		{
			Seen = true;
			
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CMarkVehicleAsSeenRequest(Id));
			hit.BuildAndSend();
			
			_eventBus.Send(new CVehicleSeenSignal(Id));
		}

		public void DecreaseDurability(int durabilityLoss, long time)
		{
			_durability.RemoveDurability(durabilityLoss, time);
		}

		public void RepairDurability(int repairAmount, long time)
		{
			_durability.AddDurability(repairAmount, time);
		}

		public void IncreaseStatLevel(EVehicleStat statId, long time)
		{
			IncrementStatLevel();
			
			if (statId == EVehicleStat.Durability)
			{
				UpgradeDurability(time);
			}
			
			_eventBus.Send(new CVehicleUpgradedSignal(Id));

			CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CUpgradeVehicleRequest(Id, statId));
			hit.BuildAndSend();
			return;
			
			void IncrementStatLevel()
			{
				if (_statLevels.TryGetValue(statId, out int level))
				{
					_statLevels[statId] = level + 1;
					return;
				}

				_statLevels[statId] = 1;
			}
		}

		private void UpgradeDurability(long time)
		{
			int statIndex = GetStatIndex(EVehicleStat.Durability);
			_durability.UpgradeDurability(time, statIndex + 1);
		}

		public int GetAndRefreshDurability(long time)
		{
			_durability.UpdateDurability(time);
			return _durability.CurrentAmount;
		}

		public int GetCurrentLevel()
		{
			int totalLevel = 1;
			foreach (KeyValuePair<EVehicleStat, int> stat in _statLevels)
			{
				_statLevels.TryGetValue(stat.Key, out int level);
				totalLevel += level;
			}
			return totalLevel;
		}

		public int GetMaxLevel()
		{
			int maxLevel = 1;
			foreach (EVehicleStat stat in Enum.GetValues(typeof(EVehicleStat)))
			{
				if (stat == EVehicleStat.None)
					continue;

				int maxStatLevel = Config.GetMaxStatLevel(stat);
				if (stat == EVehicleStat.AdvancedCapacity)
				{
					maxLevel += maxStatLevel + 1;
				}
				else
				{
					maxLevel += maxStatLevel - 1;
				}
			}
			return maxLevel;
		}
		
		public int GetFuelEfficiency(EVehicle vehicleId, CTripPrice tripPrice)
		{
			CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(vehicleId);
			int statLevel = GetStatIndex(EVehicleStat.FuelEfficiency);
			int statValue = vehicleConfig.GetStat(EVehicleStat.FuelEfficiency, statLevel);
			int fuelEfficiency = _vehicleConfigs.GetFuelEfficiency(tripPrice.FuelPriceValue, statValue);
			return fuelEfficiency;
		}

		public int GetMissingDurability(long timestampInMs)
		{
			int durability = GetAndRefreshDurability(timestampInMs);
			int statValue = GetStatValue(EVehicleStat.Durability);
			int missingDurability = statValue - durability;
			return missingDurability;
		}

		public long GetNextDurabilityRechargeRemainingTime(long timestampInMs)
		{
			return _durability.GetNextRechargeRemainingTime(timestampInMs);
		}

		public long GetFullDurabilityRechargeRemainingTime(long timestampInMs)
		{
			return _durability.GetFullRechargeRemainingTime(timestampInMs);
		}

		public bool CompareDurability(CModifiedVehicleRechargerDto dto)
		{
			return _durability.CompareWithDto(dto);
		}

		public void SetIsOwned(bool isOwned)
		{
			IsOwned = isOwned;
		}

		public bool IsLocked(CUser user)
		{
			return !IsOwned && !user.IsUnlockRequirementMet(Config.UnlockRequirement);
		}

		public override string ToString()
		{
			return
				$"{nameof(_durability)}: {_durability}, {nameof(Region)}: {Region}, {nameof(Id)}: {Id}, {nameof(IsOwned)}: {IsOwned}, {nameof(Seen)}: {Seen}";
		}
	}
}