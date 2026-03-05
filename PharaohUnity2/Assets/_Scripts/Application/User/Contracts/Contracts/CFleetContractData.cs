// =========================================
// AUTHOR: Claude
// DATE:   04.03.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CFleetContractData
	{
		public readonly int TotalPower;
		public readonly CTransportFleetSlotConfig[] Slots;

		public CFleetContractData(int totalPower, CTransportFleetSlotConfig[] slots)
		{
			TotalPower = totalPower;
			Slots = slots;
		}
	}
}
