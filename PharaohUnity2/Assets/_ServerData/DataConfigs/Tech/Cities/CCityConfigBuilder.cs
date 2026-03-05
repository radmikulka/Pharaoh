// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace ServerData
{
	public class CCityConfigBuilder
	{
		public readonly CTripPriceBuilder TripPriceBuilder = new();
		private readonly List<CPassengerContractGeneratorPhase> _generatorPhases = new();
		private readonly ECity _cityId;

		private ERegion _region;
		private ELiveEvent _liveEvent;
		private EStaticContractId _unlockContract;

		public CCityConfigBuilder(ECity cityId)
		{
			_cityId = cityId;
		}
		
		public CCityConfigBuilder SetUnlockContract(EStaticContractId unlockContract)
		{
			_unlockContract = unlockContract;
			return this;
		}
		
		public CCityConfigBuilder SetRegion(ERegion region)
		{
			_region = region;
			return this;
		}
		
		public CCityConfigBuilder SetLiveEvent(ELiveEvent liveEvent)
		{
			_liveEvent = liveEvent;
			return this;
		}

		public CCityConfigBuilder AddLinearContractGeneratorPhase(SIntMinMaxRange requiredPassengersRange, int phaseLenght, SIntMinMaxRange rewardRange)
		{
			_generatorPhases.Add(new CLinearPassengerContractGeneratorPhase(requiredPassengersRange, rewardRange, phaseLenght));
			return this;
		}
		
		public CCityConfigBuilder AddCyclicContractGeneratorPhase(SIntMinMaxRange requiredPassengersRange, int phaseLenght, int cycleFrequency, SIntMinMaxRange rewardRange)
		{
			_generatorPhases.Add(new CCyclicPassengerContractGeneratorPhase(requiredPassengersRange, rewardRange, phaseLenght, cycleFrequency));
			return this;
		}
		
		public CCityConfig Build()
		{
			return new CCityConfig(
				_cityId, 
				_region, 
				_liveEvent, 
				_unlockContract,
				_generatorPhases.ToArray(), 
				TripPriceBuilder.Build()
				);
		}
	}
}