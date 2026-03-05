// =========================================
// AUTHOR: Juraj Joscak
// DATE:   10.11.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using AldaEngine;
using ServerData.Design;

namespace ServerData.Logging
{
	public class CFactoriesDataLogging : IFactoriesDataLogging
	{
		private readonly CDesignFactoryConfigs _factoryConfigs;
		private readonly ILogger _logger;

		private StringBuilder _stringBuilder;
	
		public CFactoriesDataLogging(ILogger logger, CDesignFactoryConfigs factoryConfigs)
		{
			_logger = logger;
			_factoryConfigs = factoryConfigs;
		}
	
		public void LogAllFactoryLevels()
		{
			_stringBuilder = new StringBuilder();
			_stringBuilder.AppendLine("");
		
			IEnumerable<EFactory> factoryIds = _factoryConfigs.GetAllFactoryIds();
			foreach (EFactory factoryId in factoryIds)
			{
				LogFactoryLevels(factoryId);
			}
		
			_logger.LogInfo(_stringBuilder.ToString());
		}
	
		private void LogFactoryLevels(EFactory factoryId)
		{
			_stringBuilder.AppendLine($"Factory {factoryId}");
			_stringBuilder.AppendLine("--------------------");

			CFactoryConfig cfg = _factoryConfigs.GetFactoryConfig(factoryId);

			LogBaseLevel(cfg);

			for (int i = 0; i < cfg.GetMaxLevel()-1; i++)
			{
				LogLevel(i, cfg);
			}
		
			_stringBuilder.AppendLine("");
		}
	
		private void LogBaseLevel(CFactoryConfig factoryCfg)
		{
			EYearMilestone unlockYear = ((CYearUnlockRequirement)factoryCfg.UnlockRequirement).Year;
			_stringBuilder.AppendLine($"Baselevel (Open Level {(int)unlockYear})");
			LogDurability(factoryCfg, 0);
			LogMaterials(factoryCfg, 0);
			_stringBuilder.AppendLine("");
		}

		private void LogLevel(int index, CFactoryConfig factoryCfg)
		{
			CFactoryLevelConfig previousCfg = index > 0 ? factoryCfg.GetLevelConfig(index) : null;
			_stringBuilder.Append($"Factory {factoryCfg.Id} Level {index+1}");

			if (previousCfg != null && previousCfg.UpgradeRequirements.FirstOrDefault(req => req is CYearMilestoneRequirement) is CYearMilestoneRequirement yearReq)
			{
				_stringBuilder.Append($" (Open Level {(int)yearReq.Year})");
			}
			_stringBuilder.Append('\n');
		
			LogDurability(factoryCfg, index);
			LogMaterials(factoryCfg, index+1);
			_stringBuilder.AppendLine("");
		}
	
		private void LogDurability(CFactoryConfig factoryCfg, int level)
		{
			SRechargerLevelConfig rechargerLevel = factoryCfg.GetRechargerConfig(level);
			_stringBuilder.AppendLine($"FactoryDurability {rechargerLevel.MaxCapacity}, FactoryRepairSpeed {rechargerLevel.ProductionPerTick}/{TimeString(rechargerLevel.ProductionTickDurationInSeconds)}");
		}
		
		private void LogMaterials(CFactoryConfig cfg, int level)
		{
			EResource[] products = cfg.GetAllProductsOnLevel(level);
			foreach (EResource product in products)
			{
				CFactoryProduct productLevel = cfg.GetProduct(product, level);
				_stringBuilder.AppendLine($"Material {product} - {productLevel.Resource.Amount} / {TimeString(productLevel.ProductionTimeInSeconds)}");
			}
		}

		private string TimeString(long time)
		{
			string hours = (time / 3600) > 0 ? $"{time / 3600}h " : "";
			string minutes = (time % 3600) / 60 > 0 ? $"{(time % 3600) / 60}m " : "";
			string seconds = (time % 60) > 0 ? $"{time % 60}s" : "";
			return $"{hours}{minutes}{seconds}";
		}
	}
}