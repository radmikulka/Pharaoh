// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using ServerData;
using TycoonBuilder;

namespace Server
{
	public class CStaticOffers
	{
		private readonly CStaticOffersStorage _staticOffersStorage;
		private readonly Dictionary<EStaticOfferId, COffer> _offersCache = new();

		public CStaticOffers(CStaticOffersStorage staticOffersStorage)
		{
			_staticOffersStorage = staticOffersStorage;
		}
	
		public COffer GetOffer(EStaticOfferId id)
		{
			if (_offersCache.TryGetValue(id, out var offer))
				return offer;
		
			CClientOfferBuilder builder = _staticOffersStorage.GetOffer(id);
			string guild = CStaticOfferParser.GetOfferGuid(id);
			COffer newOffer = new(
				guild, 
				builder.Price, 
				builder.Rewards.ToArray(),
				builder.Params.AllParams,
				builder.Tags.ToArray(),
				false
			);
			_offersCache.Add(id, newOffer);
			return newOffer;
		}
		
		public COffer GetOffer(string id)
		{
			EStaticOfferId offerId = CStaticOfferParser.GetOfferId(id);
			return GetOffer(offerId);
		}
	}
}