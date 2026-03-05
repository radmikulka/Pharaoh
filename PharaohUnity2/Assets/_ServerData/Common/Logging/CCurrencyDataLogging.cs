// =========================================
// AUTHOR: Juraj Joscak
// DATE:   20.11.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AldaEngine;
using Server;
using ServerData.Design;

namespace ServerData.Logging
{
	public class CCurrencyDataLogging : ICurrencyDataLogging
	{
		private readonly ILogger _logger;
		private readonly CDesignStoryContractConfigs _storyContractConfigs;
		private readonly CDesignRegionConfigs _regionConfigs;
		private readonly CDesignFactoryConfigs _factoryConfigs;
		private readonly CDesignVehicleDepoConfig _vehicleDepoConfig;
		private readonly CDesignFuelStationConfig _fuelStationConfig;
		private readonly CDesignWarehouseConfig _warehouseConfig;
		private readonly CDesignMainCityConfigs _mainCityConfigs;
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly CDesignInitialUserConfig _initialUserConfig;
		
		private StringBuilder _stringBuilder;
		private readonly EYearMilestone _startYear = EYearMilestone._1930;
		private readonly ERegion _region = ERegion.Region1;
		
		public CCurrencyDataLogging(
			ILogger logger,
			CDesignStoryContractConfigs storyContractConfigs,
			CDesignRegionConfigs regionConfigs,
			CDesignFactoryConfigs factoryConfigs,
			CDesignVehicleDepoConfig vehicleDepoConfig,
			CDesignFuelStationConfig fuelStationConfig,
			CDesignWarehouseConfig warehouseConfig,
			CDesignMainCityConfigs mainCityConfigs,
			CDesignVehicleConfigs vehicleConfigs,
			CDesignInitialUserConfig initialUserConfig
			)
		{
			_logger = logger;
			_storyContractConfigs = storyContractConfigs;
			_regionConfigs = regionConfigs;
			_factoryConfigs = factoryConfigs;
			_vehicleDepoConfig = vehicleDepoConfig;
			_fuelStationConfig = fuelStationConfig;
			_warehouseConfig = warehouseConfig;
			_mainCityConfigs = mainCityConfigs;
			_vehicleConfigs = vehicleConfigs;
			_initialUserConfig = initialUserConfig;
		}
		
		public void LogCurrencySourcesAndSinks(EValuable currencyType)
		{
			_stringBuilder = new StringBuilder();
			_stringBuilder.AppendLine("");
			
			for (EYearMilestone i = _startYear; i <= CDesignProgressConfig.MaxYear; i++)
			{
				LogYear(i, currencyType);
			}
			
			_logger.LogInfo(_stringBuilder.ToString());
		}
		
		private void LogYear(EYearMilestone year, EValuable currencyType)
		{
			LogSources(year, currencyType);
			_stringBuilder.AppendLine("");
			LogSinks(year, currencyType);
			_stringBuilder.AppendLine("");
		}

		private void LogSources(EYearMilestone year, EValuable currencyType)
		{
			_stringBuilder.AppendLine($"Year {(int)year} Sources");
			int total = 0;
			total += LogStoryContractSources(year, currencyType);
			total += LogDecadePassFreeRewards(year, currencyType);
			_stringBuilder.AppendLine($"Total: {total}");
		}
		
		private int LogStoryContractSources(EYearMilestone year, EValuable currencyType)
		{
			int total = 0;

			foreach (CStoryContractConfig cfg in _storyContractConfigs.AllConfigs)
			{
				if(!IsContractUnlockedInYear(cfg, year, false))
					continue;

				foreach (CContractTask task in cfg.Tasks)
				{
					total += (task.Rewards.FirstOrDefault(reward => reward.Id == currencyType) as CConsumableValuable)?.Value ?? 0;
				}
			}
			
			_stringBuilder.AppendLine($"Story Contracts: {total}");
			return total;
		}
		
		private int LogDecadePassFreeRewards(EYearMilestone year, EValuable currencyType)
		{
			int total = 0;

			CDecadePassConfigData data = _regionConfigs.GetRegionConfig(_region).GetDecadePass(); 
			int startIndex = data.GetYearStartIndex(year);
			int endIndex = data.GetYearStartIndex(year+1);
			if(endIndex != -1)
				endIndex -= 1;

			for (int i = startIndex; i < data.FreeRewards.Count; i++)
			{
				if(i == -1)
					break;
				
				if(endIndex != -1 && i > endIndex)
					break;
				
				if(data.FreeRewards[i].Reward is CConsumableValuable consumableValuable && consumableValuable.Id == currencyType)
				{
					total += consumableValuable.Value;
				}
			}
			
			_stringBuilder.AppendLine($"Decade Pass Free Rewards: {total}");
			return total;
		}
		
		private void LogSinks(EYearMilestone year, EValuable currencyType)
		{
			_stringBuilder.AppendLine($"Year {(int)year} Sinks");
			int total = 0;
			total += LogFactoryUpgradeSinks(year, currencyType);
			total += LogVehicleDepotUpgradeSinks(year, currencyType);
			total += LogFuelStationUpgradeSinks(year, currencyType);
			total += LogWarehouseUpgradeSinks(year, currencyType);
			total += LogCityUpgradeSinks(year, currencyType);
			total += LogBuyVehicleSinks(year, currencyType);
			total += LogUpgradeVehicleSinks(year, currencyType);
			_stringBuilder.AppendLine($"Total: {total}");
		}
		
		private int LogFactoryUpgradeSinks(EYearMilestone year, EValuable currencyType)
		{
			int total = 0;

			IEnumerable<EFactory> factoryIds = _factoryConfigs.GetAllFactoryIds();
			foreach (EFactory factory in factoryIds)
			{
				CFactoryConfig cfg = _factoryConfigs.GetFactoryConfig(factory);
				if(!IsUnlockRequirementSatisfiedInYear(cfg.UnlockRequirement, year, true))
					continue;

				int i = 1;
				if (year != _startYear && IsUnlockRequirementSatisfiedInYear(cfg.UnlockRequirement, year-1, true))
				{
					for(; i < cfg.GetMaxLevel(); i++)
					{
						CFactoryLevelConfig levelCfg = cfg.GetLevelConfig(i);
						IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.UpgradeRequirements;
						
						if(!IsUpgradePossibleInYear(upgradeRequirements, year-1, out _, currencyType))
							break;
					}
				}
				
				for(; i < cfg.GetMaxLevel(); i++)
				{
					CFactoryLevelConfig levelCfg = cfg.GetLevelConfig(i);
					IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.UpgradeRequirements;
					
					if(!IsUpgradePossibleInYear(upgradeRequirements, year, out int sink, currencyType))
						break;
					
					total += sink;
				}
			}
			
			_stringBuilder.AppendLine($"Factory Upgrade: {total}");
			return total;
		}
		
		private int LogVehicleDepotUpgradeSinks(EYearMilestone year, EValuable currencyType)
		{
			int total = 0;
			
			int i = 1;
			if (year != _startYear)
			{
				for(; i < _vehicleDepoConfig.GetMaxLevel()-1; i++)
				{
					CUpgradeLevelConfig levelCfg = _vehicleDepoConfig.GetLevel(i);
					IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.UpgradeRequirements;
						
					if(!IsUpgradePossibleInYear(upgradeRequirements, year-1, out _, currencyType))
						break;
				}
			}
				
			for(; i < _vehicleDepoConfig.GetMaxLevel()-1; i++)
			{
				CUpgradeLevelConfig levelCfg = _vehicleDepoConfig.GetLevel(i);
				IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.UpgradeRequirements;
				if(!IsUpgradePossibleInYear(upgradeRequirements, year, out int sink, currencyType))
					break;
					
				total += sink;
			}
			
			_stringBuilder.AppendLine($"Vehicle Depot Upgrade: {total}");
			return total;
		}
		
		private int LogFuelStationUpgradeSinks(EYearMilestone year, EValuable currencyType)
		{
			int total = 0;
			
			int i = 1;
			if (year != _startYear)
			{
				for(; i < _fuelStationConfig.MaxLevel-1; i++)
				{
					CUpgradeLevelConfig levelCfg = _fuelStationConfig.GetLevel(i);
					IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.UpgradeRequirements;
						
					if(!IsUpgradePossibleInYear(upgradeRequirements, year-1, out _, currencyType))
						break;
				}
			}
				
			for(; i < _fuelStationConfig.MaxLevel-1; i++)
			{
				CUpgradeLevelConfig levelCfg = _fuelStationConfig.GetLevel(i);
				IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.UpgradeRequirements;
				if(!IsUpgradePossibleInYear(upgradeRequirements, year, out int sink, currencyType))
					break;
					
				total += sink;
			}
			
			_stringBuilder.AppendLine($"Fuel Station Upgrade: {total}");
			return total;
		}
		
		private int LogWarehouseUpgradeSinks(EYearMilestone year, EValuable currencyType)
		{
			int total = 0;
			
			int i = 1;
			if (year != _startYear)
			{
				for(; i < _warehouseConfig.GetMaxLevel()-1; i++)
				{
					CWarehouseLevel levelCfg = _warehouseConfig.GetLevel(i);
					IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.Requirements;
						
					if(!IsUpgradePossibleInYear(upgradeRequirements, year-1, out _, currencyType))
						break;
				}
			}
				
			for(; i < _warehouseConfig.GetMaxLevel()-1; i++)
			{
				CWarehouseLevel levelCfg = _warehouseConfig.GetLevel(i);
				IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.Requirements;
				if(!IsUpgradePossibleInYear(upgradeRequirements, year, out int sink, currencyType))
					break;
					
				total += sink;
			}
			
			_stringBuilder.AppendLine($"Warehouse Upgrade: {total}");
			return total;
		}
		
		private int LogCityUpgradeSinks(EYearMilestone year, EValuable currencyType)
		{
			int total = 0;

			if (!IsUnlockRequirementSatisfiedInYear(CDesignMainCityConfigs.UnlockRequirement, year, true))
			{
				_stringBuilder.AppendLine($"City Upgrade: {total}");
				return total;
			}
			
			int i = 1;
			if (year != _startYear)
			{
				for(; i < _mainCityConfigs.GetMaxLevel()-1; i++)
				{
					CMainCityLevelConfig levelCfg = _mainCityConfigs.GetLevelConfig(i);
					IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.UpgradeRequirements;
						
					if(!IsUpgradePossibleInYear(upgradeRequirements, year-1, out _, currencyType))
						break;
				}
			}
				
			for(; i < _mainCityConfigs.GetMaxLevel()-1; i++)
			{
				CMainCityLevelConfig levelCfg = _mainCityConfigs.GetLevelConfig(i);
				IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelCfg.UpgradeRequirements;
				if(!IsUpgradePossibleInYear(upgradeRequirements, year, out int sink, currencyType))
					break;
					
				total += sink;
			}
			
			_stringBuilder.AppendLine($"City Upgrade: {total}");
			return total;
		}
		
		private int LogBuyVehicleSinks(EYearMilestone year, EValuable currencyType)
		{
			int total = 0;

			Dictionary<EVehicle, CVehicleConfig> configs = _vehicleConfigs.GetConfigs();

			foreach (CVehicleConfig cfg in configs.Values)
			{
				if(_initialUserConfig.RegularUserValuables.Any(val => val is CVehicleValuable vehicle && vehicle.Vehicle == cfg.Id))
					continue;
				
				if(!IsUnlockRequirementSatisfiedInYear(cfg.UnlockRequirement, year, false))
					continue;

				if (cfg.Price is CConsumableValuable consumable && consumable.Id == currencyType)
				{
					total += consumable.Value;
				} 
			}
			
			_stringBuilder.AppendLine($"Buy Vehicles: {total}");
			return total;
		}
		
		private int LogUpgradeVehicleSinks(EYearMilestone year, EValuable currencyType)
		{
			int total = 0;
			
			Dictionary<EVehicle, CVehicleConfig> configs = _vehicleConfigs.GetConfigs();
			foreach (CVehicleConfig cfg in configs.Values)
			{
				if(cfg.UnlockRequirement is not CYearUnlockRequirement)
					continue;

				if(!IsOnUpgradeYear(cfg))
					continue;
				
				foreach (KeyValuePair<EVehicleStat, int[]> stat in cfg.Stats)
				{
					int maxLevel = stat.Key is EVehicleStat.AdvancedCapacity ? stat.Value.Length : stat.Value.Length - 1;
					for (int i = 0; i < maxLevel; i++)
					{
						IEnumerable<IValuable> upgradePrice = _vehicleConfigs.GetUpgradePrice(i, stat.Key, cfg.Region);
						int price = upgradePrice.Sum(val => val is CConsumableValuable consumable && consumable.Id == currencyType ? consumable.Value : 0);
						total += price;
					}
				}
			}
			
			_stringBuilder.AppendLine($"Upgrade Vehicles: {total}");
			return total;

			bool IsOnUpgradeYear(CVehicleConfig cfg)
			{
				if (year == _startYear)
				{
					return (IsUnlockRequirementSatisfiedInYear(cfg.UnlockRequirement, year, false) && IsUnlockRequirementSatisfiedInYear(CDesignVehicleConfigs.UpgradeUnlockRequirement, year, false));
				}
				
				return (IsUnlockRequirementSatisfiedInYear(cfg.UnlockRequirement, year, true) && IsUnlockRequirementSatisfiedInYear(CDesignVehicleConfigs.UpgradeUnlockRequirement, year, true))
				       && !(IsUnlockRequirementSatisfiedInYear(cfg.UnlockRequirement, year - 1, true) && IsUnlockRequirementSatisfiedInYear(CDesignVehicleConfigs.UpgradeUnlockRequirement, year - 1, true));
			}
		}
		
		private bool IsUpgradePossibleInYear(IReadOnlyList<IUpgradeRequirement> upgradeRequirements, EYearMilestone year, out int sink, EValuable currencyType)
		{
			sink = 0;
			foreach (IUpgradeRequirement requirement in upgradeRequirements)
			{
				switch (requirement)
				{
					case CValuableRequirement { Valuable: CConsumableValuable consumable }:
						sink += (consumable.Id == currencyType) ? consumable.Value : 0;
						break;
					case CYearMilestoneRequirement yearMilestoneRequirement:
						if (yearMilestoneRequirement.Year > year)
						{
							sink = 0;
							return false;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(requirement));
				}
			}

			return true;
		}
		
		private bool IsUnlockRequirementSatisfiedInYear(IUnlockRequirement unlockRequirement, EYearMilestone year, bool includePastYears)
		{
			switch (unlockRequirement)
			{
				case CCompositeUnlockRequirement compositeUnlockRequirement:
					return compositeUnlockRequirement.Requirements.All(req => IsUnlockRequirementSatisfiedInYear(req, year, includePastYears));
				case CContractUnlockRequirement contractUnlockRequirement:
					CStoryContractConfig blockerCfg = _storyContractConfigs.GetConfigOrDefault(contractUnlockRequirement.ContractId);
					return blockerCfg != null && IsContractUnlockedInYear(blockerCfg, year, true);
				case CYearUnlockRequirement yearUnlockRequirement:
					if(includePastYears)
						return yearUnlockRequirement.Year <= year;
					else
						return yearUnlockRequirement.Year == year;
				default:
					return true;
			}
		}
		
		private bool IsContractUnlockedInYear(CStoryContractConfig cfg, EYearMilestone year, bool includePastYears)
		{
			foreach (IUnlockRequirement unlockRequirement in cfg.UnlockRequirements)
			{
				if (!IsUnlockRequirementSatisfiedInYear(unlockRequirement, year, includePastYears))
					return false;
			}
				
			return true;
		}
	}
}