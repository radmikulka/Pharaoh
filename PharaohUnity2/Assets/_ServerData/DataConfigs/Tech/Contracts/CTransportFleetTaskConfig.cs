// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.02.2026
// =========================================

namespace ServerData
{
	public class CTransportFleetTaskConfig : IContractTaskComponent
	{
		public readonly int TotalPower;
		public readonly CTransportFleetSlotConfig[] Slots;

		public CTransportFleetTaskConfig(int totalPower, CTransportFleetSlotConfig[] slots)
		{
			TotalPower = totalPower;
			Slots = slots;
		}
	}
}
