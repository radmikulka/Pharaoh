// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CModifiedUserDataDto
	{
		[JsonProperty] public CValuableModificationDto[] ValuableModifications { get; set; }
		[JsonProperty] public COffersDto NewOffers { get; set; }

		public CModifiedUserDataDto(CValuableModificationDto[] valuableModifications, COffersDto newOffers)
		{
			ValuableModifications = valuableModifications;
			NewOffers = newOffers;
		}

		[OnDeserialized]
		private void ReplaceNulls(StreamingContext context)
		{
			ValuableModifications ??= Array.Empty<CValuableModificationDto>();
		}
	}
}