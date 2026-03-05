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
			AddPrice(EInAppPrice.Usd1, "tycoonbuilder.specialpack1");
			AddPrice(EInAppPrice.Usd2, "tycoonbuilder.specialpack2");
			AddPrice(EInAppPrice.Usd3, "tycoonbuilder.specialpack3");
			AddPrice(EInAppPrice.Usd4, "tycoonbuilder.specialpack4");
			AddPrice(EInAppPrice.Usd5, "tycoonbuilder.specialpack5");
			AddPrice(EInAppPrice.Usd6, "tycoonbuilder.specialpack6");
			AddPrice(EInAppPrice.Usd7, "tycoonbuilder.specialpack7");
			AddPrice(EInAppPrice.Usd8, "tycoonbuilder.specialpack8");
			AddPrice(EInAppPrice.Usd9, "tycoonbuilder.specialpack9");
			AddPrice(EInAppPrice.Usd10, "tycoonbuilder.specialpack10");
			AddPrice(EInAppPrice.Usd12, "tycoonbuilder.specialpack11");
			AddPrice(EInAppPrice.Usd13, "tycoonbuilder.specialpack12");
			AddPrice(EInAppPrice.Usd14, "tycoonbuilder.specialpack13");
			AddPrice(EInAppPrice.Usd15, "tycoonbuilder.specialpack14");
			AddPrice(EInAppPrice.Usd17, "tycoonbuilder.specialpack15");
			AddPrice(EInAppPrice.Usd20, "tycoonbuilder.specialpack16");
			AddPrice(EInAppPrice.Usd25, "tycoonbuilder.specialpack17");
			AddPrice(EInAppPrice.Usd30, "tycoonbuilder.specialpack18");

			AddPrice(EInAppPrice.Usd35, "tycoonbuilder.specialpack19");
			AddPrice(EInAppPrice.Usd40, "tycoonbuilder.specialpack20");
			AddPrice(EInAppPrice.Usd45, "tycoonbuilder.specialpack21");
			AddPrice(EInAppPrice.Usd50, "tycoonbuilder.specialpack22");
			AddPrice(EInAppPrice.Usd60, "tycoonbuilder.specialpack23");
			AddPrice(EInAppPrice.Usd70, "tycoonbuilder.specialpack24");
			AddPrice(EInAppPrice.Usd80, "tycoonbuilder.specialpack25");
			AddPrice(EInAppPrice.Usd100, "tycoonbuilder.specialpack26");
			AddPrice(EInAppPrice.Usd120, "tycoonbuilder.specialpack27");
			AddPrice(EInAppPrice.Usd140, "tycoonbuilder.specialpack28");
			AddPrice(EInAppPrice.Usd160, "tycoonbuilder.specialpack29");
			AddPrice(EInAppPrice.Usd200, "tycoonbuilder.specialpack30");

			AddPrice(EInAppPrice.Usd55, "tycoonbuilder.specialpack31");
		}
	}
}