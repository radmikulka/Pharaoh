// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.2.2024
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace ServerData
{
	public class CStaticOffersStorage
	{
		private readonly Dictionary<EStaticOfferId, CClientOfferBuilder> _offers = new();
		private readonly ILogger _logger;

		public CStaticOffersStorage(ILogger logger)
		{
			_logger = logger;
		}

		public CClientOfferBuilder GetOffer(EStaticOfferId offerId)
		{
			return _offers[offerId];
		}
	
		public void AddOffer(EStaticOfferId offerId, CClientOfferBuilder builder)
		{
			_offers.Add(offerId, builder);
			ValidateOffer(offerId, builder);
			BuildAnalyticsId(builder);
		}

		private void ValidateOffer(EStaticOfferId offerId, CClientOfferBuilder builder)
		{
			if (!builder.Params.HaveParam(EOfferParam.OriginalAnalyticsId))
			{
				_logger.LogError($"Offer {offerId} does not have AnalyticsId param set: {builder}");
			}
		}

		private void BuildAnalyticsId(CClientOfferBuilder builder)
		{
			string originalId = builder.Params.GetParamValue<string>(EOfferParam.OriginalAnalyticsId);
			string analyticsId = $"__{EOfferType.SimpleOffer}_{originalId}_";
			IOfferParam result = COfferParams.AnalyticsId(analyticsId);
			builder.Params.SetParam(result);
		}
	}
}