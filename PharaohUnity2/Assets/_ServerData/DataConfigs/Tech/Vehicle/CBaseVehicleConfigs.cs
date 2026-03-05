// =========================================
// AUTHOR: Radek Mikulka
// DATE:   27.06.2025
// =========================================

using System;
using System.Collections.Generic;
using ServerData.Design;

namespace ServerData
{
	public class CBaseVehicleConfigs
	{
		private readonly Dictionary<EVehicle, CVehicleConfig> _configs = new();
		private readonly Dictionary<EMovementType, SVehicleSpeed> _movementParams = new();
		public IReadOnlyList<SMovementTypeSpeed> MovementSpeeds => _allMovementSpeeds;
		private SMovementTypeSpeed[] _allMovementSpeeds;

		protected IVehicleBuilderFactory _vehicleBuilderFactory;
		protected CDesignRegionConfigs RegionConfigs;

		public CBaseVehicleConfigs(CDesignRegionConfigs regionConfigs, IVehicleBuilderFactory vehicleBuilderFactory)
		{
			_vehicleBuilderFactory = vehicleBuilderFactory;
			RegionConfigs = regionConfigs;
		}

		public CVehicleConfig GetConfig(EVehicle id)
		{
			return _configs[id];
		}

		public CVehicleBuilder GetNewVehicleBuilder(EVehicle id)
		{
			CVehicleBuilder vehicleBuilder = _vehicleBuilderFactory.GetNewVehicleBuilder();
			vehicleBuilder.SetStandardOrigin();
			return vehicleBuilder.SetVehicle(id);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="speed">rychlost vozidla, která může být ovlivněna ještě parametrem na křivce</param>
		/// <param name="accelerationDistance">Vzdálenost, po které vozidlo zrychlí z 0 na maximální rychlost.</param>
		/// <param name="decelerationDistance">Vzdálenost, po které vozidlo zpomalí z maximální rychlosti na 0</param>
		protected void SetMovementInMeterPerSec(EMovementType type, float speed, float accelerationDistance)
		{
			SVehicleSpeed speedConfig = new(type, speed, accelerationDistance, accelerationDistance);
			_movementParams[type] = speedConfig;
		}

		public int GetCapacity(int statValue)
		{
			return statValue;
		}

		public SVehicleSpeed GetMovementSpeed(EMovementType type)
		{
			return _movementParams[type];
		}

		internal void AddConfig(CVehicleConfig config)
		{
			_configs.Add(config.Id, config);
		}

		public Dictionary<EVehicle, CVehicleConfig> GetConfigs()
		{
			return _configs;
		}

		protected void FinalizeConfiguration()
		{
			_allMovementSpeeds = new SMovementTypeSpeed[_movementParams.Count];
			int iterator = 0;
			foreach (var valuePair in _movementParams)
			{
				_allMovementSpeeds[iterator++] = new SMovementTypeSpeed(valuePair.Key, valuePair.Value);
			}
		}
		
		public IValuable[] GetRepairPrice(int repairAmount, ERegion regionId)
		{
			CRegionConfig regionConfig = RegionConfigs.GetRegionConfig(regionId);
			IValuable softCurrencyPrice = GetRepairPriceSoftCurrency(repairAmount, regionConfig);
			IValuable oilPrice = GetRepairPriceMachineOil(repairAmount);
			return new [] { softCurrencyPrice, oilPrice };
		}
		
		protected virtual IValuable GetRepairPriceSoftCurrency(int repairAmount, CRegionConfig regionConfig)
		{
			return CNullValuable.Instance;
		}

		protected virtual IValuable GetRepairPriceMachineOil(int repairAmount)
		{
			return CNullValuable.Instance;
		}
		
		public IEnumerable<IValuable> GetUpgradePrice(int statIndex, EVehicleStat stat, ERegion regionId)
		{
			int statLevel = statIndex + 1;
			CRegionConfig regionConfig = RegionConfigs.GetRegionConfig(regionId);
			yield return GetUpgradePriceSoftCurrency(statLevel, stat, regionConfig);

			IValuable valuable = GetUpgradePriceVehicleStat(statLevel, stat, regionConfig);
			yield return valuable;
		}
		
		protected virtual IValuable GetUpgradePriceSoftCurrency(int statLevel, EVehicleStat stat, CRegionConfig regionConfig)
		{
			return CNullValuable.Instance;
		}

		protected virtual IValuable GetUpgradePriceVehicleStat(int statLevel, EVehicleStat stat, CRegionConfig regionConfig)
		{
			return CNullValuable.Instance;
		}
		
		protected IValuable UpgradePriceVehicleStat(EVehicleStat stat, float parts)
		{
			switch (stat)
			{
				case EVehicleStat.Capacity:
					return CValuableFactory.CapacityPart((int)parts);
				case EVehicleStat.Durability:
					return CValuableFactory.DurabilityPart((int)parts);
				case EVehicleStat.FuelEfficiency:
					return CValuableFactory.FuelPart((int)parts);
				case EVehicleStat.AdvancedCapacity:
					return CValuableFactory.AdvancedCapacityPart((int)parts);
				default:
					throw new NotImplementedException("Not implemented vehicle stat: " + stat);
			}
		}

	}
}