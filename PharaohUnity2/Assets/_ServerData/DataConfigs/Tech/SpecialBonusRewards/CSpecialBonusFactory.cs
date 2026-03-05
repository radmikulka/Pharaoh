// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.02.2026
// =========================================

namespace ServerData
{
	public static class CSpecialBonusFactory
	{
		public static ISpecialBonus WarehouseCapacity(int bonus)
		{
			return new CIntSpecialBonus(ESpecialBonusRewardType.WarehouseCapacity, bonus);
		}

		public static ISpecialBonus FuelCapacity(int bonus)
		{
			return  new CIntSpecialBonus(ESpecialBonusRewardType.FuelCapacity, bonus);
		}
	}
}