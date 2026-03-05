// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.11.2025
// =========================================

using AldaEngine;

namespace ServerData
{
	public abstract class CPassengerContractGeneratorPhase
	{
		public readonly SIntMinMaxRange RequiredPassengersRange;
		public readonly SIntMinMaxRange RewardRange;
		public readonly int PhaseLength;

		protected CPassengerContractGeneratorPhase(
			SIntMinMaxRange requiredPassengersRange, 
			SIntMinMaxRange rewardRange, 
			int phaseLength
			)
		{
			RequiredPassengersRange = requiredPassengersRange;
			RewardRange = rewardRange;
			PhaseLength = phaseLength;
		}
	}
}