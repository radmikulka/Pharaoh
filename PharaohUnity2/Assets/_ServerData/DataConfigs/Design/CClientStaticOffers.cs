// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using AldaEngine;

namespace ServerData
{
	[NonLazy]
	public class CClientStaticOffers : CBaseClientStaticOffers
	{
		public CClientStaticOffers(CStaticOffersStorage offersStorage) : base(offersStorage)
		{
			CreateOffers();
		}

		private void CreateOffers()
		{
			AddDecadePassOffers();
		}

		private void AddDecadePassOffers()
		{
			CClientOfferBuilder premiumOffer = GetOfferBuilder()
				.SetPrice(CValuableFactory.RealMoney(EInAppPrice.Usd10))
				.AddTag(EOfferTag.DecadePassPremium)
				;
			premiumOffer.Params.SetAnalyticsId("DecadePassPremium");
			AddOffer(EStaticOfferId.DecadePassPremium, premiumOffer);
			
			CClientOfferBuilder extraPremiumOffer = GetOfferBuilder()
					.SetPrice(CValuableFactory.RealMoney(EInAppPrice.Usd20))
					.AddTag(EOfferTag.DecadePassExtraPremium)
				;
			extraPremiumOffer.Params.SetAnalyticsId("DecadePassExtraPremium");
			AddOffer(EStaticOfferId.DecadePassExtraPremium, extraPremiumOffer);

			CClientOfferBuilder extraPremiumUpgradeOffer = GetOfferBuilder()
					.SetPrice(CValuableFactory.RealMoney(EInAppPrice.Usd10))
					.AddTag(EOfferTag.DecadePassExtraPremiumUpgrade)
				;
			extraPremiumUpgradeOffer.Params.SetAnalyticsId("DecadePassExtraPremiumUpgrade");
			AddOffer(EStaticOfferId.DecadePassExtraPremiumUpgrade, extraPremiumUpgradeOffer);
		}
	}
}