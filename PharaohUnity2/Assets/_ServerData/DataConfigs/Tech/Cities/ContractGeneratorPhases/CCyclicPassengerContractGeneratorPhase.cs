// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.11.2025
// =========================================

using AldaEngine;

namespace ServerData
{
	public class CCyclicPassengerContractGeneratorPhase : CPassengerContractGeneratorPhase
	{
		public readonly int CycleFrequency;
		
		public CCyclicPassengerContractGeneratorPhase(
			SIntMinMaxRange requiredPassengersRange, 
			SIntMinMaxRange rewardRange,
			int phaseLength, 
			int cycleFrequency
		) 
			: base(requiredPassengersRange, rewardRange, phaseLength)
		{
			CycleFrequency = cycleFrequency;
		}
	}
}