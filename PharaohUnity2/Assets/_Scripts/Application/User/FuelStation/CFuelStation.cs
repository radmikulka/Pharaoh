// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;
using ServerData.Hits;

namespace TycoonBuilder
{
	public class CFuelStation : CBaseUserComponent
	{
		public CLevelData LevelData { get; private set; }

		private readonly CDesignSpecialBonusRewards _bonusRewards;
		private readonly CDesignFuelStationConfig _config;
		private readonly IRewardQueue _rewardQueue;
		private readonly IServerTime _serverTime;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;

		public readonly CObservableRecharger Recharger = new();

		public CFuelStation(
			CDesignSpecialBonusRewards bonusRewards, 
			CDesignFuelStationConfig config, 
			IRewardQueue rewardQueue, 
			CHitBuilder hitBuilder, 
			IServerTime serverTime, 
			IEventBus eventBus, 
			IMapper mapper
			)
		{
			_bonusRewards = bonusRewards;
			_rewardQueue = rewardQueue;
			_hitBuilder = hitBuilder;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_config = config;
			_mapper = mapper;
		}

		public void InitialSync(CFuelStationDto dto)
		{
			SRechargerLevelConfig rechargerLevelConfig = _config.GetRechargerConfig(dto.LevelData.Level);
			CRechargerWithBonusCapacity recharger = CRechargerWithBonusCapacity.Existing(dto.Recharger.LastTickTime, dto.Recharger.CurrentAmount, rechargerLevelConfig, GetBonusCapacity);
			CLevelData levelData = _mapper.Map<CLevelDataDto, CLevelData>(dto.LevelData);
			
			long time = _serverTime.GetDayRefreshTimeInMs();
			Recharger.SetRecharger(recharger, time);
			LevelData = levelData;
		}
		
		public void StartUpgrade(long time)
		{
			IReadOnlyList<IUpgradeRequirement> upgradeRequirements = GetUpgradeRequirements();

			IValuable[] valuables = upgradeRequirements.ExtractValuables().ToArray();
			_rewardQueue.ChargeValuable(EModificationSource.UpgradeFactory, valuables);

			LevelData.StartUpgrade(time);
			
			_hitBuilder.GetBuilder(new CStartFuelStationUpgradeRequest()).BuildAndSend();
			_eventBus.Send(new CFuelStationUpgradeStateChangedSignal());
		}

		public IReadOnlyList<IUpgradeRequirement> GetUpgradeRequirements()
		{
			return _config.GetLevel(LevelData.Level + 1).UpgradeRequirements;
		}

		public void SpeedUpUpgrade(long time)
		{
			_hitBuilder.GetBuilder(new CSpeedupFuelStationUpgradeRequest())
				.BuildAndSend();

			long? upgradeFinishTime = UpgradeFinishTime();
			if (!upgradeFinishTime.HasValue)
				throw new Exception("Upgrade finish time is null");
			
			IValuable price = CDesignGlobalConfig.GetSpeedupPrice(upgradeFinishTime.Value - time);
			_rewardQueue.ChargeValuable(EModificationSource.FactoryUpgradeSpeedup, new []{price});
			
			FinishUpgrade(time);
			_eventBus.Send(new CFuelStationUpgradeStateChangedSignal());
		}

		public long? UpgradeFinishTime()
		{
			if (!LevelData.IsUpgradeRunning())
				return null;
			
			return LevelData.UpgradeStartTime + _config.GetLevel(LevelData.Level + 1).UpgradeDurationInSeconds * CTimeConst.Second.InMilliseconds;
		}
		
		public void ClaimUpgrade(long time)
		{
			FinishUpgrade(time);
			
			_hitBuilder.GetBuilder(new CClaimFuelStationUpgradeRequest()).BuildAndSend();
			_eventBus.Send(new CFuelStationUpgradeStateChangedSignal());
		}
	
		private void FinishUpgrade(long time)
		{
			bool isRunning = LevelData.IsUpgradeRunning();
			if(!isRunning)
				throw new Exception("Upgrade is not running.");
		
			LevelData.FinishUpgrade();
			SRechargerLevelConfig rechargerConfig = _config.GetRechargerConfig(LevelData.Level);
			Recharger.Upgrade(rechargerConfig, time);
			_eventBus.Send(new CFuelStationUpgradeStateChangedSignal());
		}

		public void ModifyOverCapacity(int amount, long time, CValueModifyParams modifyParams)
		{
			Recharger.ModifyOverCapacity(amount, time, modifyParams);
		}

		public void Remove(int amount, long timestampInMs)
		{
			Recharger.Remove(amount, timestampInMs);
		}

		public bool HaveFuel(int consumableValue)
		{
			return Recharger.CurrentAmount >= consumableValue;
		}

		public int GetFuelCapacity()
		{
			long time = _serverTime.GetTimestampInMs();
			return Recharger.MaxCapacity(time);
		}

		public bool CanAffordUpgrade()
		{
			bool isFullyUpgraded = IsFullyUpgraded();
			if (isFullyUpgraded)
				return false;
			
			IReadOnlyList<IUpgradeRequirement> requirements = GetUpgradeRequirements();
			bool canAfford = CLevelData.CanAffordRequirements(requirements, User);
			return canAfford;
		}

		public bool IsFullyUpgraded()
		{
			return LevelData.Level >= _config.MaxLevel;
		}

		public bool IsUpgradeRunning()
		{
			bool isUpgradeRunning = LevelData.IsUpgradeRunning();
			return isUpgradeRunning;
		}
		
		public bool IsCompleted(long timestampInMs)
		{
			bool isFullyUpgraded = IsFullyUpgraded();
			if (isFullyUpgraded)
				return true;
			
			long duration = _config.GetLevel(LevelData.Level + 1).UpgradeDurationInSeconds * CTimeConst.Second.InMilliseconds;
			bool isCompleted = LevelData.IsCompleted(timestampInMs, duration);
			return isCompleted;
		}

		public int GetBonusCapacity(long time)
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
				.GetBonus(ESpecialBonusRewardType.FuelCapacity);
		}

		private int DecadePassBonus()
		{
			return _bonusRewards.GetBonuses(ESpecialBonusRewardSource.DecadePass, User.Progress.Region)
				.GetBonus(ESpecialBonusRewardType.FuelCapacity);
		}
	}
}