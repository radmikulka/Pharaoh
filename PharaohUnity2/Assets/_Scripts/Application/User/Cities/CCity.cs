// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
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
	public class CCity : CBaseUserComponent
	{
		private readonly CDesignSpecialBuildingConfigs _specialBuildingConfigs;
		private readonly CDesignMainCityConfigs _mainDesignCityConfigs;
		private readonly CCityFactory _cityFactory;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;
		private readonly IServerTime _serverTime;

		private readonly HashSet<ESpecialBuilding> _ownedBuildings = new();
		private CCityData _cityData;
		private readonly CObservableRecharger _passengersGenerator = new();

		public CCity(
			CDesignSpecialBuildingConfigs specialBuildingConfigs,
			CDesignMainCityConfigs mainDesignCityConfigs,
			CCityFactory cityFactory,
			CHitBuilder hitBuilder,
			IEventBus eventBus, 
			IMapper mapper,
			IServerTime serverTime
			)
		{
			_specialBuildingConfigs = specialBuildingConfigs;
			_mainDesignCityConfigs = mainDesignCityConfigs;
			_cityFactory = cityFactory;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_mapper = mapper;
			_serverTime = serverTime;
		}

		public void InitialSync(CCitiesDto dto)
		{
			CLevelData levelData = _mapper.Map<CLevelDataDto, CLevelData>(dto.Data.LevelData);
			CBuildingPlot[] buildingPlots = _mapper.Map<CBuildingPlotDto, CBuildingPlot>(dto.Data.BuildingPlots);
			_cityData = _cityFactory.ExistingCity(levelData, dto.Data.PassengersGenerator, buildingPlots);
			_passengersGenerator.SetRecharger(_cityData.PassengersGenerator, _serverTime.GetTimestampInMs());

			_ownedBuildings.UnionWith(dto.OwnedBuildings);
		}
		
		public void StartUpgrade(long timestampInMs)
		{
			CCityData cityData = GetCityData();
			cityData.StartUpgrade(timestampInMs);
		}
		
		public void SpeedUpUpgrade(long time)
		{
			FinishUpgrade(true, time);
		}

		public void ClaimUpgrade(long time)
		{
			FinishUpgrade(false, time);
		}
		
		public bool CanAffordUpgrade()
		{
			bool isFullyUpgraded = IsFullyUpgraded();
			if (isFullyUpgraded)
				return false;
			
			CCityData cityData = GetCityData();
			int nextLevel = cityData.GetLevel() + 1;
			CMainCityLevelConfig levelConfig = _mainDesignCityConfigs.GetLevelConfig(nextLevel);
			IReadOnlyList<IUpgradeRequirement> upgradeRequirements = levelConfig.UpgradeRequirements;
			bool canAfford = CLevelData.CanAffordRequirements(upgradeRequirements, User);
			return canAfford;
		}

		private bool IsFullyUpgraded()
		{
			CCityData cityData = GetCityData();
			int currentLevel = cityData.GetLevel();
			int maxLevel = _mainDesignCityConfigs.GetMaxLevel();
			return currentLevel >= maxLevel;
		}

		private void FinishUpgrade(bool speedUp, long time)
		{
			CCityData cityData = GetCityData();
			cityData.FinishUpgrade(speedUp, time);
		}

		public bool IsCompleted(long timestampInMs)
		{
			int maxLevel  = _mainDesignCityConfigs.GetMaxLevel();
			CCityData cityData = GetCityData();
			bool isMaxLevel = cityData.GetLevel() >= maxLevel;
			if (isMaxLevel)
				return true;
			
			long duration = GetNextUpgradeDuration();
			bool isCompleted = cityData.IsCompleted(timestampInMs, duration);
			return isCompleted;
		}

		public long GetNextUpgradeDuration()
		{
			CCityData cityData = GetCityData();
			int currentLevel = cityData.GetLevel();
			CMainCityLevelConfig levelConfig = _mainDesignCityConfigs.GetLevelConfig(currentLevel + 1);
			return levelConfig.UpgradeTimeInMs;
		}
		
		public bool IsUpgradeRunning()
		{
			CCityData cityData = GetCityData();
			return cityData.IsUpgradeRunning();
		}

		public int GetLevel()
		{
			CCityData cityData = GetCityData();
			return cityData.GetLevel();
		}

		public long GetUpgradeTimeRemaining(long timestampInMs)
		{
			CCityData cityData = GetCityData();
			long upgradeDuration = GetNextUpgradeDuration();
			long timeElapsed = timestampInMs - cityData.GetUpgradeStartTime();
			long timeRemaining = upgradeDuration - timeElapsed;
			return timeRemaining;
		}

		private CCityData GetCityData()
		{
			return _cityData;
		}
		
		public void AddOwnedBuilding(ESpecialBuilding buildingId, bool free, bool sendHit)
		{
			_ownedBuildings.Add(buildingId);

			if (sendHit)
			{
				CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CBuySpecialBuildingRequest(buildingId, free));
				hit.BuildAndSend();
			}

			_eventBus.Send(new CSpecialBuildingBoughtSignal(buildingId));
		}

		public bool IsBuildingOwned(ESpecialBuilding building)
		{
			return _ownedBuildings.Contains(building);
		}
		
		public bool IsBuildingPlotUnlocked(int index)
		{
			CCityData cityData = GetCityData();
			return cityData.IsBuildingPlotUnlocked(index);
		}
		
		public bool IsAnyBuildingPlotUnlocked()
		{
			CCityData cityData = GetCityData();
			return cityData.IsAnyBuildingPlotUnlocked();
		}
		
		public bool HasBuildingInPlot(int index)
		{
			CCityData cityData = GetCityData();
			return cityData.HasBuilding(index);
		}

		public IValuable GetBuildingPlotPrice()
		{
			int unlockedPlotsCount = GetUnlockedBuildingPlotsCount();
			IValuable price = CDesignMainCityConfigs.GetBuildingPlotUnlockPrice(unlockedPlotsCount);
			return price;
		}
		
		public int GetUnlockedBuildingPlotsCount()
		{
			int count = 0;
			foreach (CBuildingPlot plot in _cityData.BuildingPlots)
			{
				if (plot.IsUnlocked)
					count++;
			}
			return count;
		}

		public bool CanAffordBuildingPlot()
		{
			IValuable price = GetBuildingPlotPrice();
			bool canAfford = User.OwnedValuables.HaveValuable(price);
			return canAfford;
		}

		public void UnlockBuildingPlot(int index)
		{
			CCityData cityData = GetCityData();
			cityData.UnlockBuildingPlot(index);
		}

		public void PlaceSpecialBuilding(int index, ESpecialBuilding buildingId, long time)
		{
			CCityData cityData = GetCityData();
			cityData.PlaceBuilding(index, buildingId, time);
		}

		public ESpecialBuilding GetBuildingOnPlot(int plotIndex)
		{
			CCityData cityData = GetCityData();
			CBuildingPlot buildingPlot = cityData.GetPlotOrDefault(plotIndex);
			ESpecialBuilding buildingId = buildingPlot?.Building ?? ESpecialBuilding.None;
			if (buildingId == ESpecialBuilding.None)
			{
				throw new Exception($"No building on plot {plotIndex} ");
			}
			return buildingId;
		}

		public bool IsBuildingPlaced(ESpecialBuilding buildingId)
		{
			CCityData cityData = GetCityData();
			bool isPlaced = cityData.IsBuildingPlaced(buildingId);
			return isPlaced;
		}

		public int GetPassengersBonusFromBuildings()
		{
			CCityData cityData = GetCityData();
			int bonus = cityData.GetPassengersBonusFromBuildings();
			return bonus;
		}

		public CObservableRecharger GetPassengersGenerator(long timestampInMs)
		{
			CCityData cityData = GetOrCreateCityData();
			cityData.PassengersGenerator.Update(timestampInMs);
			return _passengersGenerator;
		}

		public int GetBuildingsInInventoryCount()
		{
			return _ownedBuildings.Count(b => !IsBuildingPlaced(b));
		}
		
		public int GetPlotsWithBuildingCount()
		{
			CCityData cityData = GetCityData();
			int count = cityData.GetPlotsWithBuildingCount();
			return count;
		}
		
		public int GetEmptyPlotsCount()
		{
			CCityData cityData = GetCityData();
			int count = cityData.GetEmptyPlotsCount();
			return count;
		}

		public int GetEmptyPlotIndex()
		{
			CCityData cityData = GetCityData();
			return cityData.GetEmptyPlotIndex();
		}

		public int GetFirstLockedPlotIndex()
		{
			CCityData cityData = GetCityData();
			int totalPlots = _mainDesignCityConfigs.GetStatValueAtLevel(ECityStat.BuildingPlots, cityData.LevelData.Level);
			
			for(int i = 0; i < totalPlots; i++)
			{
				if (!cityData.IsBuildingPlotUnlocked(i))
					return i;
			}
			
			return -1;
		}
		
		public long GetTimeRemainingForNextPassengersGain(long timestampInMs)
		{
			CCityData cityData = GetCityData();
			long timeRemaining = cityData.PassengersGenerator.GetNextRechargeRemainingTime(timestampInMs);
			return timeRemaining;
		}

		public long GetTimeRemainingForFullPassengersGain(long timestampInMs)
		{
			CCityData cityData = GetCityData();
			long timeRemaining = cityData.PassengersGenerator.GetFullRechargeRemainingTime(timestampInMs);
			return timeRemaining;
		}
		
		public IUpgradeRequirement[] GetUpgradeRequirements()
		{
			CCityData cityData = GetCityData();
			int nextLevel = cityData.GetLevel() + 1;
			CMainCityLevelConfig levelConfig = _mainDesignCityConfigs.GetLevelConfig(nextLevel);
			return levelConfig.UpgradeRequirements.ToArray();
		}

		public bool? HaveBuilding(ESpecialBuilding buildingId)
		{
			return _ownedBuildings.Contains(buildingId);
		}

		public CCityData GetOrCreateCityData()
		{
			if (_cityData == null)
			{
				_cityData ??= _cityFactory.NewCity();
				_passengersGenerator.SetRecharger(_cityData.PassengersGenerator, _serverTime.GetTimestampInMs());
			}
			
			return _cityData;
		}

		public IEnumerable<ELiveEvent> GetRequiredEvents()
		{
			foreach (ESpecialBuilding ownedBuilding in _ownedBuildings)
			{
				CSpecialBuildingConfig buildingConfig = _specialBuildingConfigs.GetConfig(ownedBuilding);
				if(buildingConfig.LiveEvent == ELiveEvent.None)
					continue;
				yield return buildingConfig.LiveEvent;
			}
		}
	}
}

