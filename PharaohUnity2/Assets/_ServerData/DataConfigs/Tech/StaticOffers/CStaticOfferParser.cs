// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.10.2025
// =========================================

namespace ServerData
{
	public static class CStaticOfferParser
	{
		private const string Prefix = "StaticOffer_";
	
		public static string GetOfferGuid(EStaticOfferId offerId)
		{
			return $"{Prefix}{(int)offerId}";
		}
	
		public static EStaticOfferId GetOfferId(string offerGuid)
		{
			string idString = offerGuid.Replace(Prefix, "");
			int id = int.Parse(idString);
			return (EStaticOfferId)id;
		}
	}
}