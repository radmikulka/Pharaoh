// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

namespace ServerData
{
	public class CAdvertisementValuable : IValuable
	{
		public EValuable Id => EValuable.Advertisement;
		public EAdPlacement Placement { get; set; }

		public CAdvertisementValuable()
		{
		}

		public CAdvertisementValuable(EAdPlacement placement)
		{
			Placement = placement;
		}

		public EValuablePrice GetPriceType()
		{
			return EValuablePrice.Advertisement;
		}
	}
}