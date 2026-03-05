// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.11.2025
// =========================================

namespace ServerData
{
	public struct SPassengerGeneratorPhase
	{
		public readonly CPassengerContractGeneratorPhase Phase;
		public readonly int CycleOffset;

		public SPassengerGeneratorPhase(CPassengerContractGeneratorPhase phase, int cycleOffset)
		{
			Phase = phase;
			CycleOffset = cycleOffset;
		}
	}
}