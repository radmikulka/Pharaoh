// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData.Design;

namespace ServerData
{
	public class CFactoryConfig
	{
		private readonly CFactoryLevelConfig _baseLevel;
		private readonly CFactoryLevelConfig[] _levels;
		private readonly EResource[] _allProducts;
		
		public readonly EFactory Id;
		public readonly ELiveEvent LiveEvent;
		public readonly IUnlockRequirement UnlockRequirement;
		
		public bool IsEventFactory => LiveEvent != ELiveEvent.None;

		public CFactoryConfig(EFactory id, ELiveEvent liveEvent, IUnlockRequirement unlockRequirement, CFactoryLevelConfig baseLevel, CFactoryLevelConfig[] levels)
		{
			UnlockRequirement = unlockRequirement;
			_baseLevel = baseLevel;
			LiveEvent = liveEvent;
			_levels = levels;
			Id = id;
			_allProducts = GenerateProductsCache();
		}

		public SRechargerLevelConfig GetRechargerConfig(int level)
		{
			int max = _baseLevel.GetMaxDurabilityBonus();
			int production = _baseLevel.GetRepairAmountBonus();
			long duration = _baseLevel.GetRepairTimeBonus();

			for (int i = 0; i < level - 1; i++)
			{
				CFactoryLevelConfig levelConfig = GetLevelConfig(i + 1);
				max += levelConfig.GetMaxDurabilityBonus();
				production += levelConfig.GetRepairAmountBonus();
				duration -= levelConfig.GetRepairTimeBonus();
			}
			
			return new SRechargerLevelConfig(max, production, duration);
		}
		
		public CFactoryLevelConfig GetLevelConfig(int level)
		{
			return _levels[level - 1];
		}
		
		public EYearMilestone GetUnlockYear()
		{
			if(UnlockRequirement.GetRequirementOfType<CYearUnlockRequirement>() is { } factoryYearRequirement)
			{
				return factoryYearRequirement.Year;
			}
			
			return EYearMilestone._1930;
		}
		
		public CFactoryLevelConfig GetBaseLevelConfig()
		{
			return _baseLevel;
		}
		
		public int GetMaxLevel()
		{
			return _levels.Length + 1;
		}

		public CFactoryProduct GetProduct(EResource resource, int factoryLevel)
		{
			CFactoryProduct result = new(resource);
			TryAddLevel(_baseLevel.GetProductOrDefault(resource));
			
			for (int i = factoryLevel - 2; i >= 0; i--)
			{
				CFactoryLevelConfig levelConfig = GetLevelConfig(i + 1);
				CFactoryProductConfig product = levelConfig.GetProductOrDefault(resource);
				TryAddLevel(product);
			}
			return result;

			void TryAddLevel(CFactoryProductConfig productLevel)
			{
				if(productLevel == null)
					return;
				
				result.AddProducedAmount(productLevel.Resource.Amount);
				result.AddProductionTime(productLevel.ProductionTimeInMs);

				foreach (SResource requirement in productLevel.Requirements)
				{
					result.AddRequirement(requirement);
				}
			}
		}

		public int GetProductUnlockLevel(EResource resource)
		{
			if(_baseLevel.GetProductOrDefault(resource) != null)
				return 1;
			
			for(int i = 0; i < _levels.Length; i++)
			{
				CFactoryLevelConfig levelConfig = GetLevelConfig(i + 1);
				CFactoryProductConfig product = levelConfig.GetProductOrDefault(resource);
				if(product != null)
					return i + 2;
			}
			
			throw new Exception($"Factory {Id} does not produce resource {resource}");
		}
		
		public EYearMilestone GetProductUnlockYear(EResource resource)
		{
			int unlockYear = (int)EYearMilestone._1930;

			if(UnlockRequirement.GetRequirementOfType<CYearUnlockRequirement>() is { } factoryYearRequirement)
			{
				unlockYear = CMath.Max(unlockYear, (int)factoryYearRequirement.Year);
			}
			
			for(int i = 0; i < _levels.Length; i++)
			{
				CFactoryLevelConfig levelConfig = GetLevelConfig(i + 1);
				CFactoryProductConfig product = levelConfig.GetProductOrDefault(resource);
				if(product != null)
					return (EYearMilestone)unlockYear;

				foreach (IUpgradeRequirement req in levelConfig.UpgradeRequirements)
				{
					if(req is CYearMilestoneRequirement yearReq)
					{
						unlockYear = CMath.Max(unlockYear, (int)yearReq.Year);
					}
				}
				
			}
			
			throw new Exception($"Factory {Id} does not produce resource {resource}");
		}

		public EResource[] GetAllProducts()
		{
			return _allProducts;
		}
		
		private EResource[] GenerateProductsCache()
		{
			List<EResource> result = new();
			result.AddRange(_baseLevel.GetProducts());
			
			for (int i = 0; i < _levels.Length; i++)
			{
				CFactoryLevelConfig levelConfig = GetLevelConfig(i + 1);
				result.AddRange(levelConfig.GetProducts());
			}

			return result.Distinct().ToArray();
		}
		
		public EResource[] GetAllProductsOnLevel(int level)
		{
			List<EResource> result = new();
			result.AddRange(_baseLevel.GetProducts());
			for (int i = level -2; i >= 0; i--)
			{
				CFactoryLevelConfig levelConfig = GetLevelConfig(i + 1);
				result.AddRange(levelConfig.GetProducts());
			}
			
			return result.Distinct().ToArray();
		}

		public long GetUpgradeDuration(int level)
		{
			return GetLevelConfig(level).UpgradeDurationInMs;
		}
	}
}