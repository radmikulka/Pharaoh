// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.09.2025
// =========================================

namespace ServerData
{
	internal static class CCurrencyRounder
	{
		internal static int RoundCurrency(this int amount)
		{
			int roundingUnit = amount switch
			{
				< 100 => 10,
				< 1000 => 50,
				< 10000 => 100,
				_ => 200
			};

			int result = Calculate();
			return result;
			
			int Calculate()
			{
				int remainder = amount % roundingUnit;
				int baseAmount = amount - remainder;

				if (remainder >= roundingUnit / 2)
				{
					baseAmount += roundingUnit;
				}
				return baseAmount;
			}
		}
		
		internal static int RoundCurrency(this float amount)
		{
			return RoundCurrency((int)amount);
		}
	}
}