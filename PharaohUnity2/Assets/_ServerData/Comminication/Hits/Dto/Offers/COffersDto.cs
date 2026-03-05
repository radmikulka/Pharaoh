// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class COffersDto : IMapAble
	{
		[JsonProperty] public COfferDto[] Offers { get; set; }
		[JsonProperty] public COfferGroupDto[] Groups { get; set; }

		public COffersDto()
		{
		}

		public COffersDto(COfferDto[] offers, COfferGroupDto[] groups)
		{
			Offers = offers;
			Groups = groups;
		}
	}
}