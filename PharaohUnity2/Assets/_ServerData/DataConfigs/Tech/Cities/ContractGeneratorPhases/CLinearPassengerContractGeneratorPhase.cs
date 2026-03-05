// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.11.2025
// =========================================

using AldaEngine;

namespace ServerData
{
	public class CLinearPassengerContractGeneratorPhase : CPassengerContractGeneratorPhase
	{
		public CLinearPassengerContractGeneratorPhase(
			SIntMinMaxRange requiredPassengersRange, 
			SIntMinMaxRange rewardRange, 
			int phaseLength
		) 
			: base(requiredPassengersRange, rewardRange, phaseLength)
		{
		}
	}
}