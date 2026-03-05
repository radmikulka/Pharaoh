// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System;
using AldaEngine;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CCityFactory
	{
		private readonly CDesignSpecialBuildingConfigs _specialBuildingConfigs;
		private readonly CDesignMainCityConfigs _cityConfigs;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly IServerTime _serverTime;

		public CCityFactory(
			CDesignSpecialBuildingConfigs specialBuildingConfigs, 
			CDesignMainCityConfigs cityConfigs,
			CHitBuilder hitBuilder,
			IEventBus eventBus,
			IServerTime serverTime
		)
		{
			_specialBuildingConfigs = specialBuildingConfigs;
			_cityConfigs = cityConfigs;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_serverTime = serverTime;
		}

		public CCityData NewCity()
		{
			CRecharger resourceGenerator = CreateNewPassengersGenerator();
			return new CCityData(
				_specialBuildingConfigs, 
				_cityConfigs, 
				_hitBuilder,
				_eventBus,
				CLevelData.New(), 
				resourceGenerator, 
				Array.Empty<CBuildingPlot>()
				);
		}
	
		public CCityData ExistingCity(
			CLevelData levelData, 
			CRechargerDto dto,
			CBuildingPlot[] plots
		)
		{
			CRecharger resourceGenerator = CreateExistingPassengersGenerator(
				levelData.Level, 
				plots,
				dto
			);
			return new CCityData(
				_specialBuildingConfigs, 
				_cityConfigs, 
				_hitBuilder,
				_eventBus,
				levelData, 
				resourceGenerator, 
				plots
				);
		}
	
		private CRecharger CreateNewPassengersGenerator()
		{
			SRechargerLevelConfig generatorConfig = _cityConfigs.GetPassengersGeneratorConfig(1, 0);
			return CRecharger.New(generatorConfig);
		}

		private CRecharger CreateExistingPassengersGenerator(
			int cityLevel, 
			CBuildingPlot[] plots,
			CRechargerDto dto
		)
		{
			int passengersBonus = GetPassengersBonusFromBuildings(plots);
			SRechargerLevelConfig generatorConfig = _cityConfigs.GetPassengersGeneratorConfig(cityLevel, passengersBonus);
			return CRecharger.Existing(dto.LastTickTime, dto.CurrentAmount, generatorConfig);
		}
		
		private int GetPassengersBonusFromBuildings(CBuildingPlot[] plots)
		{
			int bonus = 0;
			foreach (CBuildingPlot plot in plots)
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
	}
}