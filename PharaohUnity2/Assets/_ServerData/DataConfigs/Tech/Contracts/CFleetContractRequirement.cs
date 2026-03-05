// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.02.2026
// =========================================

namespace ServerData
{
	public class CFleetContractRequirement : IContractRequirement
	{
		public readonly int TotalPower;
		public readonly CTransportFleetSlotConfig[] Slots;

		public CFleetContractRequirement(int totalPower, CTransportFleetSlotConfig[] slots)
		{
			TotalPower = totalPower;
			Slots = slots;
		}
	}
}
