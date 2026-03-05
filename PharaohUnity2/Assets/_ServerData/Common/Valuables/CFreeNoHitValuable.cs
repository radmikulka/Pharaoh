// =========================================
// AUTHOR: Marek Karaba
// DATE:   31.07.2025
// =========================================

namespace ServerData
{
	public class CFreeNoHitValuable : IValuable
	{
		public EValuable Id => EValuable.FreeNoHit;
        
		public static readonly CFreeNoHitValuable Instance = new();

		private CFreeNoHitValuable()
		{
		}

		public EValuablePrice GetPriceType()
		{
			return EValuablePrice.Free;
		}
	}
}