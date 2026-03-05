// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

namespace ServerData
{
	public class CStaticOfferValuable : IValuable
	{
		public EValuable Id => EValuable.StaticOffer;
		public EStaticOfferId OfferId { get; set; }
		
		public CStaticOfferValuable(EStaticOfferId offerId)
		{
			OfferId = offerId;
		}
	}
}