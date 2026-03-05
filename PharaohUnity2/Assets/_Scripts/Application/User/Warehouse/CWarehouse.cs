// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;
using ServerData.Hits;
using UnityEngine;

namespace TycoonBuilder
{
	[ValidatableData]
	public class CWarehouse : CBaseUserComponent
	{
		[ValidatableData] private readonly Dictionary<EResource, CWarehouseResource> _resources = new();
		public CLevelData LevelData { get; private set; }

		private readonly CDesignSpecialBonusRewards _bonusRewards;
		private readonly CDesignResourceConfigs _resourceConfigs;
		private readonly IRewardQueue _rewardQueue;
		private readonly CHitBuilder _hitBuilder;
		private readonly IServerTime _serverTime;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;

		public CWarehouse(
			CDesignSpecialBonusRewards bonusRewards,
			CDesignResourceConfigs resourceConfigs,
			IRewardQueue rewardQueue,
			CHitBuilder hitBuilder,
			IServerTime serverTime,
			IEventBus eventBus, 
			IMapper mapper
			)
		{
			_resourceConfigs = resourceConfigs;
			_bonusRewards = bonusRewards;
			_rewardQueue = rewardQueue;
			_hitBuilder = hitBuilder;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_mapper = mapper;
		}

		public void InitialSync(CWarehouseDto dto)
		{
			LevelData = _mapper.Map<CLevelDataDto, CLevelData>(dto.LevelData);

			foreach (SResource resource in dto.Resources)
			{
				CWarehouseResource warehouseResource = CreateResource(resource.Id, resource.Amount);
				_resources.Add(resource.Id, warehouseResource);
			}
		}

		public void Sync(SResource resource)
		{
			if(!_resources.TryGetValue(resource.Id, out CWarehouseResource currentAmount))
			{
				currentAmount = CreateResource(resource.Id, 0);
				_resources.Add(resource.Id, currentAmount);
			}
			
			currentAmount.Amount.ServerValue = resource.Amount;
		}
		
		private CWarehouseResource CreateResource(EResource resourceId, int amount)
		{
			CResourceConfig config = _resourceConfigs.GetResourceConfig(resourceId);
			return new CWarehouseResource(config, amount);
		}

		public int GetTotalStoredResources()
		{
			int sum = 0;
			foreach (CWarehouseResource amount in _resources.Values)
			{
				if(!amount.CountToWarehouseCapacity)
					continue;
				sum += amount;
			}
			return sum;
		}

		public SResource[] GetAllResources()
		{
			return _resources.Select(item => new SResource(item.Key, item.Value)).ToArray();
		}

		public int GetResourceAmount(EResource resourceId)
		{
			return _resources.GetValueOrDefault(resourceId, null) ?? 0;
		}

		public SResource GetResource(EResource resourceId)
		{
			int amount = _resources.GetValueOrDefault(resourceId, null) ?? 0;
			return new SResource(resourceId, amount);
		}

		public void AddResource(SResource resource)
		{
			if(_resources.TryGetValue(resource.Id, out CWarehouseResource currentAmount))
			{
				_resources[resource.Id].Amount.LocalValue = currentAmount.Amount.LocalValue + resource.Amount;
			}
			else
			{
				CWarehouseResource newResource = CreateResource(resource.Id, resource.Amount);
				_resources.Add(resource.Id, newResource);
			}
			
			SendResourceChangedSignal(resource);
		}
		
		public void RemoveResource(SResource resource)
		{
			if (!_resources.TryGetValue(resource.Id, out CWarehouseResource currentAmount)) 
				return;
			_resources[resource.Id].Amount.LocalValue = currentAmount - resource.Amount;

			SendResourceChangedSignal(resource);
		}
		
		private void SendResourceChangedSignal(SResource resource)
		{
			SResource newResourceValue = GetResource(resource.Id);
			_eventBus.Send(new CWarehouseResourceChangedSignal(newResourceValue));
		}

		public void RemoveResource(IReadOnlyList<SResource> resources)
		{
			foreach (SResource resource in resources)
			{
				RemoveResource(resource);
			}
		}

		public bool HaveResource(SResource resource)
		{
			if(_resources.TryGetValue(resource.Id, out CWarehouseResource currentAmount))
			{
				return currentAmount >= resource.Amount;
			}

			return false;
		}

		public void BuyMoreMaterial(SResource resource)
		{
			IValuable price = _resourceConfigs.GetGetMoreMaterialPrice(resource);
			PayPrice(price);
			
			_rewardQueue.AddRewards(EModificationSource.GetMoreMaterial, new IValuable[] {new CResourceValuable(resource)} );
			_hitBuilder.GetBuilder(new CGetMoreMaterialRequest(resource))
				.BuildAndSend();
		}

		private void PayPrice(IValuable valuable)
		{
			switch (valuable)
			{
				case CConsumableValuable consumable:
					_rewardQueue.ChargeValuable(EModificationSource.GetMoreMaterial, new IValuable[]{consumable});
					break;
				default:
					throw new NotImplementedException("Non consumable prices are not implemented");
			}
		}

		public int GetCurrentLevel()
		{
			return LevelData.Level;
		}

		public int GetBonusCapacity()
		{
			int bonus = 0;
			if (User.DecadePass.PremiumStatus == EBattlePassPremiumStatus.ExtraPremium)
			{
				bonus += DecadePassBonus();
			}

			bool haveEventPremium = User.LiveEvents.HavePremiumPass(EBattlePassPremiumStatus.ExtraPremium);
			if (haveEventPremium)
			{
				bonus += EventPassBonus();
			}
			
			return bonus;
		}
		
		public int GetTotalBonusCapacity()
		{
			int bonus = DecadePassBonus();
			
			bool anyEventRunning = User.LiveEvents.IsAnyEventRunning(_serverTime.GetTimestampInMs());
			if (anyEventRunning)
			{
				bonus += EventPassBonus();
			}
			return bonus;
		}

		private int EventPassBonus()
		{
			return _bonusRewards.GetBonuses(ESpecialBonusRewardSource.EventPass, User.Progress.Region)
				.GetBonus(ESpecialBonusRewardType.WarehouseCapacity);
		}

		private int DecadePassBonus()
		{
			return _bonusRewards.GetBonuses(ESpecialBonusRewardSource.DecadePass, User.Progress.Region)
				.GetBonus(ESpecialBonusRewardType.WarehouseCapacity);
		}
	}
}