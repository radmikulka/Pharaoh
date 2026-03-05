// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

namespace ServerData
{
	public class CFactoryMaxDurabilityReward : IUpgradeReward
	{
		public readonly int DurabilityIncrease;

		public CFactoryMaxDurabilityReward(int durabilityIncrease)
		{
			DurabilityIncrease = durabilityIncrease;
		}
	}
}