// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

namespace ServerData
{
	public class CFactoryRepairSpeedReward : IUpgradeReward
	{
		public readonly int RepairAmountIncrease;
		public readonly long RepairTimeDecreaseSeconds;

		public CFactoryRepairSpeedReward(int repairAmountIncrease, long repairTimeDecrease)
		{
			RepairAmountIncrease = repairAmountIncrease;
			RepairTimeDecreaseSeconds = repairTimeDecrease;
		}
	}
}