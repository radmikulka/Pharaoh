// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
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
	public class CVehicleDepo : CBaseUserComponent
	{
		private readonly CDesignVehicleDepoConfig _config;
		private readonly IRewardQueue _rewardQueue;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;
		
		private CLevelData _levelData;

		public CVehicleDepo(
			CDesignVehicleDepoConfig config,
			IRewardQueue rewardQueue,
			CHitBuilder hitBuilder,
			IEventBus eventBus,
			IMapper mapper)
		{
			_rewardQueue = rewardQueue;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_config = config;
			_mapper = mapper;
		}

		public void InitialSync(CVehicleDepoDto dto)
		{
			CLevelData levelData = _mapper.Map<CLevelDataDto, CLevelData>(dto.LevelData);
			_levelData = levelData;
		}

		public int GetVehicleRepairAmount()
		{
			int result = _config.GetDurabilityRepairAmount(_levelData.Level);
			return result;
		}
		
		public int GetNextLevelVehicleRepairAmount()
		{
			int result = _config.GetDurabilityRepairAmount(_levelData.Level + 1);
			return result;
		}
		
		public bool IsFullyUpgraded()
		{
			int maxLevel = _config.GetMaxLevel();
			return _levelData.Level >= maxLevel;
		}

		public void StartUpgrade(long timestampInMs)
		{
			CUpgradeLevelConfig levelConfig = _config.GetLevel(_levelData.Level + 1);
			IUpgradeRequirement[] valuableRequirements = levelConfig.UpgradeRequirements.Where(requirement => requirement is CValuableRequirement).ToArray();
			IValuable[] valuables = valuableRequirements.Select(requirement => ((CValuableRequirement)requirement).Valuable).ToArray();
			
			_rewardQueue.ChargeValuable(EModificationSource.VehicleDepoUpgrade, valuables);
			
			_levelData.StartUpgrade(timestampInMs);
			
			_hitBuilder.GetBuilder(new CStartVehicleDepoUpgradeRequest()).BuildAndSend();
			_eventBus.Send(new CDepotUpgradeStartedSignal());
		}
		
		public bool IsUpgradeRunning()
		{
			return _levelData.IsUpgradeRunning();
		}

		public void ClaimUpgrade()
		{
			FinishUpgrade();
			_hitBuilder.GetBuilder(new CClaimVehicleDepoUpgradeRequest()).BuildAndSend();
		}
		
		private void FinishUpgrade()
		{
			_levelData.FinishUpgrade();
			_eventBus.Send(new CDepotUpgradedSignal());
		}

		public void SpeedupUpgrade(long timestampInMs)
		{
			long remainingTime = GetRemainingUpgradeTime(timestampInMs);
			IValuable speedUpPrice = CDesignGlobalConfig.GetSpeedupPrice(remainingTime);
			_rewardQueue.ChargeValuable(EModificationSource.VehicleDepoUpgrade, new []{speedUpPrice} );

			FinishUpgrade();
			_hitBuilder.GetBuilder(new CSpeedupVehicleDepoUpgradeRequest()).BuildAndSend();
		}

		public long GetRemainingUpgradeTime(long requestClientTime)
		{
			long duration = GetNextUpgradeTotalDurationInMs();
			return _levelData.GetRemainingUpgradeTime(requestClientTime, duration);
		}

		public int GetCurrentLevel()
		{
			return _levelData.Level;
		}
		
		public long GetNextUpgradeTotalDurationInMs()
		{
			CUpgradeLevelConfig levelConfig = _config.GetLevel(_levelData.Level + 1);
			return levelConfig.UpgradeDurationInSeconds * CTimeConst.Second.InMilliseconds;
		}
		
		public IReadOnlyList<IUpgradeRequirement> GetNextLevelUpgradeRequirements()
		{
			CUpgradeLevelConfig levelConfig = _config.GetLevel(_levelData.Level + 1);
			return levelConfig.UpgradeRequirements;
		}

		public bool IsCompleted(long timestampInMs)
		{
			long duration = GetNextUpgradeTotalDurationInMs();
			bool isCompleted = _levelData.IsCompleted(timestampInMs, duration);
			return isCompleted;
		}

		public bool CanAffordUpgrade()
		{
			bool isFullyUpgraded = IsFullyUpgraded();	
			if (isFullyUpgraded)
				return false;
			
			IReadOnlyList<IUpgradeRequirement> requirements = GetNextLevelUpgradeRequirements();
			bool canAfford = CLevelData.CanAffordRequirements(requirements, User);
			return canAfford;
		}
	}
}