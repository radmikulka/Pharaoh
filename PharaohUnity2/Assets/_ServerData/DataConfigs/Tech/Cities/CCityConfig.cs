// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.09.2025
// =========================================

using AldaEngine;

namespace ServerData
{
	public class CCityConfig
	{
		public readonly ECity CityId;
		public readonly ERegion Region;
		public readonly ELiveEvent LiveEvent;
		public readonly EStaticContractId UnlockContract;
		public readonly CPassengerContractGeneratorPhase[] ContractsGeneratorPhases;
		public readonly CTripPrice TripPrice;

		public CCityConfig(
			ECity cityId, 
			ERegion region, 
			ELiveEvent liveEvent,
			EStaticContractId unlockContract, 
			CPassengerContractGeneratorPhase[] contractsGeneratorPhases, 
			CTripPrice tripPrice
			)
		{
			ContractsGeneratorPhases = contractsGeneratorPhases;
			UnlockContract = unlockContract;
			LiveEvent = liveEvent;
			TripPrice = tripPrice;
			Region = region;
			CityId = cityId;
		}

		public SPassengerGeneratorPhase GetPassengerGeneratorPhase(int completedContracts)
		{
			int cumulativeLenght = 0;
			for (int i = 0; i < ContractsGeneratorPhases.Length; i++)
			{
				CPassengerContractGeneratorPhase phase = ContractsGeneratorPhases[i];
				if(cumulativeLenght + phase.PhaseLength >= completedContracts)
				{
					return new(phase, completedContracts - cumulativeLenght);
				}

				cumulativeLenght += phase.PhaseLength;
			}
			return new(ContractsGeneratorPhases[^1], completedContracts - cumulativeLenght);
		}
	}
}