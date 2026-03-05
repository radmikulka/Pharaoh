// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

namespace ServerData
{
	public class CVehicleDurabilityRepairAmountReward : IUpgradeReward
	{
		public readonly int AmountIncrease;

		public CVehicleDurabilityRepairAmountReward(int amountIncrease)
		{
			AmountIncrease = amountIncrease;
		}
	}
}