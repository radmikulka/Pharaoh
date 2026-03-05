// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

namespace ServerData
{
	public class CFuelStationCapacityReward : IUpgradeReward
	{
		public readonly int CapacityIncrease;

		public CFuelStationCapacityReward(int capacityIncrease)
		{
			CapacityIncrease = capacityIncrease;
		}
	}
}