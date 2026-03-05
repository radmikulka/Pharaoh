// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;
using ServerData.Dto;
using ServerData.Hits;
using UnityEngine;

namespace TycoonBuilder
{
	public class CFactories : CBaseUserComponent
	{
		private readonly Dictionary<EFactory, CFactory> _factories = new();
		private readonly CDesignFactoryConfigs _factoryConfigs;
		private readonly CFactoriesFactory _factoriesFactory;
		private readonly IRewardQueue _rewardQueue;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly IServerTime _serverTime;

		public CFactories(
			CFactoriesFactory factoriesFactory, 
			CDesignFactoryConfigs factoryConfigs, 
			IRewardQueue rewardQueue, 
			CHitBuilder hitBuilder,
			IEventBus eventBus,
			IServerTime serverTime
			)
		{
			_factoriesFactory = factoriesFactory;
			_factoryConfigs = factoryConfigs;
			_rewardQueue = rewardQueue;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_serverTime = serverTime;
		}
		
		public void InitialSync(CFactoriesDto dto)
		{
			foreach (CFactoryDto factoryDb in dto.Factores)
			{
				CFactory factory = _factoriesFactory.GetExisting(factoryDb);
				_factories.Add(factory.Id, factory);
			}
		}

		public void CraftInFactory(EFactory factoryId, EResource resource, int slotIndex, long time)
		{
			CFactory factory = GetOrCreateFactory(factoryId);
			CFactoryConfig config = _factoryConfigs.GetFactoryConfig(factoryId);
			CFactoryProduct product = config.GetProduct(resource, factory.LevelData.Level);

			User.Warehouse.RemoveResource(product.Requirements);
			factory.StartProduction(slotIndex, product.Resource, product.ProductionTimeInMs, time);
			
			_hitBuilder.GetBuilder(new CCraftInFactoryRequest(factoryId, resource, slotIndex))
				.BuildAndSend();
			
			_eventBus.Send(new CFactoryProductionStartedSignal());
		}

		public void CollectCraftedFactoryProduct(EFactory factoryId, int slotIndex, EModificationSource overrideSource = EModificationSource.None)
		{
			CFactory factory = GetOrCreateFactory(factoryId);
			CFactorySlot slot = factory.GetOrCreateSlot(slotIndex);
			
			slot.Claim();
			
			_hitBuilder.GetBuilder(new CCollectCraftedFactoryProductRequest(factoryId, slotIndex, overrideSource))
				.BuildAndSend();
			
			_eventBus.Send(new CFactoryProductClaimedSignal());
		}

		public void UpgradeFactory(EFactory factoryId, long time)
		{
			CFactory factory = GetOrCreateFactory(factoryId);

			IReadOnlyList<IUpgradeRequirement> upgradeRequirements = _factoryConfigs.GetUpgradeRequirements(
				factoryId,
				factory.LevelData.Level
			);

			IValuable[] valuables = upgradeRequirements.ExtractValuables().ToArray();
			_rewardQueue.ChargeValuable(EModificationSource.UpgradeFactory, valuables);

			factory.LevelData.StartUpgrade(time);
			
			_hitBuilder.GetBuilder(new CUpgradeFactoryRequest(factoryId)).BuildAndSend();
			_eventBus.Send(new CFactoryUpgradeStateChangedSignal());
		}

		public void SpeedUpUpgradeFactory(EFactory factoryId, long time)
		{
			_hitBuilder.GetBuilder(new CSpeedupFactoryUpgradeRequest(factoryId))
				.BuildAndSend();

			long? upgradeFinishTime = UpgradeFinishTime(factoryId);
			if (!upgradeFinishTime.HasValue)
				throw new Exception("Upgrade finish time is null");
			
			IValuable price = CDesignGlobalConfig.GetSpeedupPrice(upgradeFinishTime.Value - time);
			_rewardQueue.ChargeValuable(EModificationSource.FactoryUpgradeSpeedup, new []{price});
			
			CFactory factory = GetOrCreateFactory(factoryId);
			factory.LevelData.FinishUpgrade();
			factory.Durability.Upgrade(_factoryConfigs.GetDurabilityConfig(factoryId, factory.LevelData.Level), _serverTime.GetTimestampInMs());
			_eventBus.Send(new CFactoryUpgradeStateChangedSignal());
		}
		
		public long? UpgradeFinishTime(EFactory factoryId)
		{
			CFactoryConfig factoryCfg = _factoryConfigs.GetFactoryConfig(factoryId);
			CFactory factoryData = GetOrCreateFactory(factoryId);
			CFactoryLevelConfig level = factoryCfg.GetLevelConfig(factoryData.LevelData.Level);
			return factoryData.LevelData.UpgradeStartTime + level.UpgradeDurationInMs;
		}

		public void BuyFactorySlot(EFactory factoryId, int slotIndex)
		{
			CFactory factory = GetOrCreateFactory(factoryId);
			CFactorySlot slot = factory.GetOrCreateSlot(slotIndex);

			int unlockedSlotsCount = factory.GetUnlockedSlotsCount();
			IValuable price = _factoryConfigs.GetSlotPrice(unlockedSlotsCount, factoryId);
			_rewardQueue.ChargeValuable(EModificationSource.BuyFactorySlot, new []{price});

			slot.Unlock();
			
			_hitBuilder.GetBuilder(new CBuyFactorySlotRequest(factoryId, slotIndex)).BuildAndSend();
			_eventBus.Send(new CFactorySlotBoughtSignal());
		}

		public void RepairFactory(EFactory factoryId, long time)
		{
			CFactory factory = GetOrCreateFactory(factoryId);
			IValuable[] price = _factoryConfigs.GetRepairPrice(User.Progress.SeenYear);

			_rewardQueue.ChargeValuable(EModificationSource.RepairFactory, price);
			_hitBuilder.GetBuilder(new CRepairFactoryRequest(factoryId)).BuildAndSend();
			factory.Repair(time);
			
			_eventBus.Send(new CFactoryRepairedSignal());
		}

		public void ClaimFactoryUpgrade(EFactory factoryId)
		{
			CFactory factory = GetOrCreateFactory(factoryId);
			factory.LevelData.FinishUpgrade();
			factory.Durability.Upgrade(_factoryConfigs.GetDurabilityConfig(factoryId, factory.LevelData.Level), _serverTime.GetTimestampInMs());
			
			_hitBuilder.GetBuilder(new CClaimFactoryUpgradeRequest(factoryId)).BuildAndSend();
			_eventBus.Send(new CFactoryUpgradeStateChangedSignal());
		}

		public CFactory GetOrCreateFactory(EFactory id)
		{
			if (_factories.TryGetValue(id, out var factory))
				return factory;

			factory = _factoriesFactory.GetNew(id);
			_factories[id] = factory;
			return factory;
		}
		
		public int GetTotalFinishedProductsCount()
		{
			int result = 0;
			foreach (CFactory factory in _factories.Values)
			{
				CFactoryConfig cfg = _factoryConfigs.GetFactoryConfig(factory.Id);
				if(cfg.LiveEvent != ELiveEvent.None && User.LiveEvents.GetActiveEventOrDefault(cfg.LiveEvent) == null)
					continue;
				
				result += factory.GetFinishedProductsCount(EResource.None, _serverTime.GetTimestampInMs());
			}

			return result;
		}

		public bool IsUpgradeRunning(EFactory factoryId)
		{
			CFactory factory = GetOrCreateFactory(factoryId);
			return factory.LevelData.IsUpgradeRunning();
		}

		public void SetFactoryAsSeen(EFactory factoryId)
		{
			_hitBuilder.GetBuilder(new CFactorySeenRequest(factoryId)).BuildAndSend();
			_factories[factoryId].MarkAsSeen();
		}

		public bool IsAnyFactoryUpgradeCompleted()
		{
			foreach (CFactory factory in _factories.Values)
			{
				bool isCompleted = IsUpgradeCompleted(factory.Id);
				if (isCompleted)
					return true;
			}
			return false;
		}

		public bool IsUpgradeCompleted(EFactory factoryId)
		{
			CFactory factory = GetOrCreateFactory(factoryId);
			CFactoryConfig factoryCfg = _factoryConfigs.GetFactoryConfig(factoryId);
			
			if(factory.LevelData.Level >= factoryCfg.GetMaxLevel())
				return false;
			
			CFactoryLevelConfig level = factoryCfg.GetLevelConfig(factory.LevelData.Level);
			bool isCompleted = factory.LevelData.IsCompleted(_serverTime.GetTimestampInMs(), level.UpgradeDurationInMs);
			return isCompleted;
		}

		public bool AreAllFactoryUpgradesRunning()
		{
			if(_factories.Values.Count == 0)
				return false;
			
			return _factories.Values.All(factory => factory.LevelData.IsUpgradeRunning());
		}
		
		public EFactory GetFactoryForLiveEventOrDefault(ELiveEvent liveEventId)
		{
			switch (liveEventId)
			{
				case ELiveEvent.EarthAndFire:
					return EFactory.EarthFireFactory;
				case ELiveEvent.BankingTycoon:
					return EFactory.BankingTycoonFactory;
				
				default: return EFactory.None;
			}
		}

		public bool HaveFactoryLevelSum(int targetLevelSum)
		{
			return FactoryLevelSum() >= targetLevelSum;
		}

		public int FactoryLevelSum()
		{
			return _factories.Values.Sum(factory => factory.LevelData.Level);
		}
	}
}