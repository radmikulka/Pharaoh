// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using ServerData.Design;

namespace ServerData
{
	public abstract class CBaseFactoryConfigs
	{
		private readonly Dictionary<EFactory, CFactoryConfig> _factoryConfigs = new();
		private readonly CDesignEventFactoryConfigs _eventFactoryConfigs = new();
		private EFactory[] _factoryIdsCache;

		protected CBaseFactoryConfigs()
		{
			_eventFactoryConfigs.CreateConfigs(this);
		}

		public IValuable GetSlotPrice(int unlockedSlotsCount, EFactory factory)
		{
			CFactoryConfig factoryConfig = GetFactoryConfig(factory);
			if(factoryConfig.IsEventFactory)
			{
				return _eventFactoryConfigs.GetSlotPrice(unlockedSlotsCount);
			}
			return GetSlotPrice(unlockedSlotsCount);
		}

		protected abstract IValuable GetSlotPrice(int unlockedSlotsCount);

		public void BuildFactory(CFactoryConfigBuilder builder)
		{
			CFactoryConfig newConfig = builder.Build();
			_factoryConfigs[newConfig.Id] = newConfig;
		}

		public long GetUpgradeDuration(EFactory factoryId, int levelIndex)
		{
			return GetFactoryConfig(factoryId).GetUpgradeDuration(levelIndex);
		}
		
		public SRechargerLevelConfig GetDurabilityConfig(EFactory factoryId, int level)
		{
			CFactoryConfig factory = GetFactoryConfig(factoryId);
			return factory.GetRechargerConfig(level);
		}

		public CFactoryConfig GetFactoryConfig(EFactory factoryId)
		{
			return _factoryConfigs[factoryId];
		}

		public EFactory GetEventFactory(ELiveEvent liveEvent)
		{
			return _factoryConfigs.First(pair => pair.Value.LiveEvent == liveEvent).Key;
		}

		public EFactory[] GetAllFactoryIds()
		{
			return _factoryIdsCache ??= _factoryConfigs.Keys.ToArray();
		}

		public EFactory GetFactoryByProduct(EResource product)
		{
			if(TryGetFactoryByProduct(product, out EFactory factoryId))
				return factoryId;
			
			throw new Exception("No factory produces resource " + product);	
		}
		
		public bool TryGetFactoryByProduct(EResource product, out EFactory factoryId)
		{
			foreach (EFactory factory in GetAllFactoryIds())
			{
				CFactoryConfig factoryConfig = GetFactoryConfig(factory);
				if (factoryConfig.GetAllProducts().Contains(product))
				{
					factoryId = factory;
					return true;
				}
			}

			factoryId = default;
			return false;
		}

		public IReadOnlyList<IUpgradeRequirement> GetUpgradeRequirements(EFactory factory, int level)
		{
			CFactoryLevelConfig levelConfig = GetFactoryConfig(factory).GetLevelConfig(level);
			return levelConfig.UpgradeRequirements;
		}

		public long GetUpgradeTimeInMs(EFactory factory, int level)
		{
			CFactoryLevelConfig levelConfig = GetFactoryConfig(factory).GetLevelConfig(level);
			return levelConfig.UpgradeDurationInMs;
		}
		
		public CFactoryConfigBuilder GetFactoryBuilder(EFactory factoryId, EYearMilestone unlockYear)
		{
			return new CFactoryConfigBuilder(factoryId, unlockYear);
		}
		
		public CFactoryConfigBuilder GetFactoryBuilder(EFactory factoryId)
		{
			return new CFactoryConfigBuilder(factoryId);
		}
	}
}