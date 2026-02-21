// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.10.2023
// =========================================

using ServerData;

namespace ServerData
{
	public class CInAppPrices : CBaseInAppPrices
	{
		public CInAppPrices()
		{
			InitPrices();
		}

		private void InitPrices()
		{
			AddPrice(EInAppPrice.Usd1, "pharaoh.specialpack1");
			AddPrice(EInAppPrice.Usd2, "pharaoh.specialpack2");
			AddPrice(EInAppPrice.Usd3, "pharaoh.specialpack3");
			AddPrice(EInAppPrice.Usd4, "pharaoh.specialpack4");
			AddPrice(EInAppPrice.Usd5, "pharaoh.specialpack5");
			AddPrice(EInAppPrice.Usd6, "pharaoh.specialpack6");
			AddPrice(EInAppPrice.Usd7, "pharaoh.specialpack7");
			AddPrice(EInAppPrice.Usd8, "pharaoh.specialpack8");
			AddPrice(EInAppPrice.Usd9, "pharaoh.specialpack9");
			AddPrice(EInAppPrice.Usd10, "pharaoh.specialpack10");
			AddPrice(EInAppPrice.Usd12, "pharaoh.specialpack11");
			AddPrice(EInAppPrice.Usd13, "pharaoh.specialpack12");
			AddPrice(EInAppPrice.Usd14, "pharaoh.specialpack13");
			AddPrice(EInAppPrice.Usd15, "pharaoh.specialpack14");
			AddPrice(EInAppPrice.Usd17, "pharaoh.specialpack15");
			AddPrice(EInAppPrice.Usd20, "pharaoh.specialpack16");
			AddPrice(EInAppPrice.Usd25, "pharaoh.specialpack17");
			AddPrice(EInAppPrice.Usd30, "pharaoh.specialpack18");

			AddPrice(EInAppPrice.Usd35, "pharaoh.specialpack19");
			AddPrice(EInAppPrice.Usd40, "pharaoh.specialpack20");
			AddPrice(EInAppPrice.Usd45, "pharaoh.specialpack21");
			AddPrice(EInAppPrice.Usd50, "pharaoh.specialpack22");
			AddPrice(EInAppPrice.Usd60, "pharaoh.specialpack23");
			AddPrice(EInAppPrice.Usd70, "pharaoh.specialpack24");
			AddPrice(EInAppPrice.Usd80, "pharaoh.specialpack25");
			AddPrice(EInAppPrice.Usd100, "pharaoh.specialpack26");
			AddPrice(EInAppPrice.Usd120, "pharaoh.specialpack27");
			AddPrice(EInAppPrice.Usd140, "pharaoh.specialpack28");
			AddPrice(EInAppPrice.Usd160, "pharaoh.specialpack29");
			AddPrice(EInAppPrice.Usd200, "pharaoh.specialpack30");

			AddPrice(EInAppPrice.Usd55, "pharaoh.specialpack31");
		}
	}
}