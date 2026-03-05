// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData.Design;

namespace ServerData.Logging
{
	public class CVehicleDataLogging : IVehicleDataLogging
	{
		private const string VehicleDataDirectory = "LoggingData";
		private const string VehicleDataFileName = "VehicleData.csv";
		
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly ILogger _logger;

		private IFileWriter _fileWriter;

		public CVehicleDataLogging(CDesignVehicleConfigs vehicleConfigs, ILogger logger)
		{
			_vehicleConfigs = vehicleConfigs;
			_logger = logger;
		}

		public void LogFullUpgradeCost(EVehicle vehicleId)
		{
			CVehicleConfig config = _vehicleConfigs.GetConfig(vehicleId);
			if (config == null)
			{
				_logger.LogError($"Vehicle config not found for vehicleId: {vehicleId}");
				return;
			}

			List<CConsumableValuable> totalCost = GetTotalUpgradeCost(config.Region, config);

			CConsumableValuable softCurrency =
				CValuableFactory.SoftCurrency(totalCost.Where(consumable => consumable.Id == EValuable.SoftCurrency)
					.Sum(consumable => consumable.Value));
			CConsumableValuable partsCapacity =
				CValuableFactory.Consumable(EValuable.CapacityPart,
					totalCost.Where(consumable => consumable.Id == EValuable.CapacityPart)
						.Sum(consumable => consumable.Value));
			CConsumableValuable partsDurability =
				CValuableFactory.Consumable(EValuable.DurabilityPart,
					totalCost.Where(consumable => consumable.Id == EValuable.DurabilityPart)
						.Sum(consumable => consumable.Value));
			CConsumableValuable partsConsumption =
				CValuableFactory.Consumable(EValuable.FuelPart,
					totalCost.Where(consumable => consumable.Id == EValuable.FuelPart)
						.Sum(consumable => consumable.Value));
			CConsumableValuable partsAdvancedCapacity =
				CValuableFactory.Consumable(EValuable.AdvancedCapacityPart,
					totalCost.Where(consumable => consumable.Id == EValuable.AdvancedCapacityPart)
						.Sum(consumable => consumable.Value));

			_logger.LogInfo(
				$"{config.Region}: Vehicle cost for {vehicleId}:\n" +
				$"Soft Currency: {softCurrency.Value}\n" +
				$"Parts Capacity: {partsCapacity.Value}\n" +
				$"Parts Durability: {partsDurability.Value}\n" +
				$"Parts Consumption: {partsConsumption.Value}\n" +
				$"Parts Advanced Capacity: {partsAdvancedCapacity.Value}");
		}

		// ReSharper disable once UseCollectionExpression
		private List<CConsumableValuable> GetTotalUpgradeCost(ERegion regionId, CVehicleConfig config)
		{
			EVehicleStat[] allStats = new []
			{
				EVehicleStat.Capacity, EVehicleStat.Durability, EVehicleStat.FuelEfficiency, EVehicleStat.AdvancedCapacity
			};
			List<CConsumableValuable> totalCost = new();
			foreach (EVehicleStat stat in allStats)
			{
				int maxStatLevel = config.GetMaxStatLevel(stat);
				int startingIndex = stat == EVehicleStat.AdvancedCapacity ? 0 : -1;

				for (int i = 0; i <= maxStatLevel; i++)
				{
					if (stat != EVehicleStat.AdvancedCapacity && i == maxStatLevel)
						continue;

					IValuable[] upgradePrice =
						_vehicleConfigs.GetUpgradePrice(startingIndex + i, stat, regionId).ToArray();
					foreach (IValuable valuable in upgradePrice)
					{
						if (valuable is not CConsumableValuable consumableValuable)
							throw new Exception("Only consumable valuables are supported in upgrade cost logging.");

						totalCost.Add(consumableValuable);
					}
				}
			}

			return totalCost;
		}

		public void LogAllVehiclesUpgradeCost(ERegion regionId)
		{
			Dictionary<EVehicle, CVehicleConfig> configs = _vehicleConfigs.GetConfigs();
			foreach (KeyValuePair<EVehicle, CVehicleConfig> pair in configs)
			{
				if (pair.Value.Region != regionId)
					continue;
				
				LogFullUpgradeCost(pair.Key);
			}
		}

		public void ExportAllToCsv()
		{
			_fileWriter = new CCsvWriter();
			AddHeader();
			
			Dictionary<EVehicle, CVehicleConfig> configs = _vehicleConfigs.GetConfigs();
			foreach (KeyValuePair<EVehicle, CVehicleConfig> vehicleConfig in configs)
			{
				WriteLineForVehicle(vehicleConfig);
			}
			
			_fileWriter.WriteToFile(VehicleDataDirectory, VehicleDataFileName);
		}

		private void AddHeader()
		{
			_fileWriter.AddItem("index");
			_fileWriter.AddItem("name");
			_fileWriter.AddItem("unlock_year");
			_fileWriter.AddItem("transport_type");
			_fileWriter.AddItem("movement_type");
			_fileWriter.AddItem("base_transport_capacity");
			_fileWriter.AddItem("maximum_transport_capacity");
			_fileWriter.AddItem("base_fuel_efficiency");
			_fileWriter.AddItem("maximum_fuel_efficiency");
			_fileWriter.AddItem("base_durability");
			_fileWriter.AddItem("maximum_durability");
			_fileWriter.AddItem("base_advanced_transport_capacity");
			_fileWriter.AddItem("maximum_advanced_transport_capacity");
			_fileWriter.AddItem("origin_flag");
			_fileWriter.AppendLine();
		}

		private void WriteLineForVehicle(KeyValuePair<EVehicle, CVehicleConfig> vehicleConfig)
		{
			int index = (int)vehicleConfig.Key;
			string vehicleName = vehicleConfig.Key.ToString();
			int unlockYear = vehicleConfig.Value.UnlockRequirement is CYearUnlockRequirement yearUnlockRequirement
				? (int)yearUnlockRequirement.Year
				: -1;
			ETransportType transportType = vehicleConfig.Value.TransportType;
			EMovementType movementType = vehicleConfig.Value.MovementType;
			int baseTransportCapacity = vehicleConfig.Value.GetStat(EVehicleStat.Capacity, 0);
			int maximumTransportCapacity = vehicleConfig.Value.GetStat(EVehicleStat.Capacity, vehicleConfig.Value.GetMaxStatLevel(EVehicleStat.Capacity) - 1);
			int baseFuelEfficiency = vehicleConfig.Value.GetStat(EVehicleStat.FuelEfficiency, 0);
			int maximumFuelEfficiency = vehicleConfig.Value.GetStat(EVehicleStat.FuelEfficiency, vehicleConfig.Value.GetMaxStatLevel(EVehicleStat.FuelEfficiency) - 1);
			int baseDurability = vehicleConfig.Value.GetStat(EVehicleStat.Durability, 0);
			int maximumDurability = vehicleConfig.Value.GetStat(EVehicleStat.Durability, vehicleConfig.Value.GetMaxStatLevel(EVehicleStat.Durability) - 1);
			int baseAdvancedCapacity = vehicleConfig.Value.GetStat(EVehicleStat.AdvancedCapacity, 0);
			int maximumAdvancedCapacity = vehicleConfig.Value.GetStat(EVehicleStat.AdvancedCapacity, vehicleConfig.Value.GetMaxStatLevel(EVehicleStat.AdvancedCapacity));
			EOriginFlag originFlag = vehicleConfig.Value.Origin;
			
			_fileWriter.AddItem(index);
			_fileWriter.AddItem(vehicleName);
			_fileWriter.AddItem(unlockYear);
			_fileWriter.AddItem(transportType);
			_fileWriter.AddItem(movementType);
			_fileWriter.AddItem(baseTransportCapacity);
			_fileWriter.AddItem(maximumTransportCapacity);
			_fileWriter.AddItem(baseFuelEfficiency);
			_fileWriter.AddItem(maximumFuelEfficiency);
			_fileWriter.AddItem(baseDurability);
			_fileWriter.AddItem(maximumDurability);
			_fileWriter.AddItem(baseAdvancedCapacity);
			_fileWriter.AddItem(maximumAdvancedCapacity);
			_fileWriter.AddItem(originFlag);
			_fileWriter.AppendLine();
		}
	}
}

