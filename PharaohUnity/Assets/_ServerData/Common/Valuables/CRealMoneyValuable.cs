// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

namespace ServerData
{
	public class CRealMoneyValuable : IValuable
	{
		public EValuable Id => EValuable.RealMoney;
		public EInAppPrice Price { get; set; }

		public CRealMoneyValuable()
		{
		}

		public CRealMoneyValuable(EInAppPrice price)
		{
			Price = price;
		}
	}
}