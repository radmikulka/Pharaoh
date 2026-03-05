// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;
using ServerData.Hits;

namespace TycoonBuilder
{
	public class CCityData
	{
		private readonly CDesignSpecialBuildingConfigs _specialBuildingConfigs;
		private readonly CDesignMainCityConfigs _cityConfigs;
		private readonly List<CBuildingPlot> _plots = new();
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;

		public readonly CLevelData LevelData;
		public readonly CRecharger PassengersGenerator;
		
		public IReadOnlyList<CBuildingPlot> BuildingPlots => _plots;

		public CCityData(
			CDesignSpecialBuildingConfigs specialBuildingConfigs,
			CDesignMainCityConfigs cityConfigs,
			CHitBuilder hitBuilder,
			IEventBus eventBus,
			CLevelData levelData, 
			CRecharger passengersGenerator, 
			CBuildingPlot[] plots
			)
		{
			_plots.AddRange(plots);
			_specialBuildingConfigs = specialBuildingConfigs;
			PassengersGenerator = passengersGenerator;
			_hitBuilder = hitBuilder;
			_cityConfigs = cityConfigs;
			LevelData = levelData;
			_eventBus = eventBus;
			
			PassengersGenerator.OnRecharged += OnPassengersRecharged;
		}

		public bool IsBuildingPlotUnlocked(int index)
		{
			CBuildingPlot plot = GetPlotOrDefault(index);
			return plot is { IsUnlocked: true };
		}

		public void UnlockBuildingPlot(int index)
		{
			CBuildingPlot plot = GetOrCreatePlot(index);
			plot.Unlock();
			
			_eventBus.Send(new CBuildingPlotUnlockedSignal(index));
			
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CBuyBuildingPlotRequest(index));
			hit.BuildAndSend();
		}

		public bool HasBuilding(int index)
		{
			CBuildingPlot plot = GetPlotOrDefault(index);
			return plot is { IsEmpty: false };
		}

		public void PlaceBuilding(int index, ESpecialBuilding buildingId, long time)
		{
			CBuildingPlot plot = GetPlot(index);
			plot.PlaceSpecialBuilding(buildingId);
			UpgradePopulationGenerator(time);
			
			_eventBus.Send(new CSpecialBuildingPlacedSignal(buildingId, index));
			
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CPlaceSpecialBuildingRequest(index, buildingId));
			hit.BuildAndSend();
		}

		public void StartUpgrade(long timestamp)
		{
			LevelData.StartUpgrade(timestamp);
			_eventBus.Send(new CCityUpgradeStartedSignal());
			
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CUpgradeCityRequest());
			hit.BuildAndSend();
		}

		public void FinishUpgrade(bool speedUp, long time)
		{
			LevelData.FinishUpgrade();
			UpgradePopulationGenerator(time);
			
			_eventBus.Send(new CCityUpgradedSignal());

			if (speedUp)
			{
				CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CSpeedupCityUpgradeRequest());
				hit.BuildAndSend();
			}
			else
			{
				CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CClaimCityUpgradeRequest());
				hit.BuildAndSend();
			}
		}

		public int GetLevel()
		{
			return LevelData.Level;
		}

		public long GetUpgradeStartTime()
		{
			if (!LevelData.UpgradeStartTime.HasValue)
				throw new SqlNullValueException("Upgrade is not running - UpgradeStartTime is null");
			
			return LevelData.UpgradeStartTime.Value;
		}

		public bool IsUpgradeRunning()
		{
			return LevelData.IsUpgradeRunning();
		}

		public bool IsCompleted(long currentTime, long upgradeDuration)
		{
			bool isCompleted = LevelData.IsCompleted(currentTime, upgradeDuration);
			return isCompleted;
		}

		public CBuildingPlot GetPlotOrDefault(int index)
		{
			return _plots.FirstOrDefault(p => p.Index == index);
		}

		private void UpgradePopulationGenerator(long time)
		{
			SRechargerLevelConfig generatorConfig = GetRechargerConfig();
			PassengersGenerator.Upgrade(generatorConfig, time);
		}

		private SRechargerLevelConfig GetRechargerConfig()
		{
			int passengersBonus = GetPassengersBonusFromBuildings();
			SRechargerLevelConfig generatorConfig = _cityConfigs.GetPassengersGeneratorConfig(LevelData.Level, passengersBonus);
			return generatorConfig;
		}

		public int GetPassengersBonusFromBuildings()
		{
			int bonus = 0;
			foreach (CBuildingPlot plot in _plots)
			{
				if (!plot.IsUnlocked) 
					continue;
				if(plot.IsEmpty)
					continue;

				CSpecialBuildingConfig buildingConfig = _specialBuildingConfigs.GetConfig(plot.Building);
				bonus += buildingConfig.MaxPassengersBonus;
			}
			return bonus;
		}

		private CBuildingPlot GetOrCreatePlot(int index)
		{
			CBuildingPlot plot = GetPlotOrDefault(index);
			if (plot != null) return plot;
			plot = new CBuildingPlot(index, false, ESpecialBuilding.None);
			_plots.Add(plot);
			return plot;
		}

		private CBuildingPlot GetPlot(int index)
		{
			return _plots.First(p => p.Index == index);
		}

		public int GetPlotsWithBuildingCount()
		{
			return _plots.Count(plot => !plot.IsEmpty);
		}
		
		public int GetEmptyPlotsCount()
		{
			return _plots.Count(plot => plot.IsEmpty && plot.IsUnlocked);
		}

		public bool IsBuildingPlaced(ESpecialBuilding buildingId)
		{
			return _plots.Any(plot => plot.Building == buildingId);
		}

		private void OnPassengersRecharged()
		{
			_eventBus.Send(new CCityRechargedSignal());
		}

		public bool IsAnyBuildingPlotUnlocked()
		{
			return _plots.Any(plot => plot.IsUnlocked);
		}

		public int GetUnlockedBuildingPlotsCount()
		{
			return _plots.Count(plot => plot.IsUnlocked);
		}

		public int GetEmptyPlotIndex()
		{
			return _plots.FirstOrDefault(plot => plot.IsEmpty && plot.IsUnlocked)?.Index ?? -1;
		}
	}
}