// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

namespace ServerData
{
	public class CBaseClientStaticOffers
	{
		private readonly CStaticOffersStorage _offersStorage;

		public CBaseClientStaticOffers(CStaticOffersStorage offersStorage)
		{
			_offersStorage = offersStorage;
		}

		protected CClientOfferBuilder GetOfferBuilder()
		{
			return new CClientOfferBuilder();
		}

		protected void AddOffer(EStaticOfferId id, CClientOfferBuilder builder)
		{
			_offersStorage.AddOffer(id, builder);
		}
	}
}