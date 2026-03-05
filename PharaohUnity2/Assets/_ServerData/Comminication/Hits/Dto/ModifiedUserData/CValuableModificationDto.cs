// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData;

namespace ServerData.Dto
{
	public class CValuableModificationDto
	{
		[JsonProperty] public EModificationSource Source { get; set; }
		[JsonProperty] public string SourceDetail { get; set; }
		[JsonProperty] public EValuablePrice Price { get; set; }
		[JsonProperty] public CValuableDto Valuable { get; set; }
		[JsonProperty] public CValuableDto OwnedValue { get; set; }

		public CValuableModificationDto()
		{
		}

		public CValuableModificationDto(EModificationSource source, EValuablePrice price, string sourceDetail, CValuableDto valuable, CValuableDto ownedValue)
		{
			Source = source;
			Price = price;
			Valuable = valuable;
			OwnedValue = ownedValue;
			SourceDetail = sourceDetail;
		}
	}
}