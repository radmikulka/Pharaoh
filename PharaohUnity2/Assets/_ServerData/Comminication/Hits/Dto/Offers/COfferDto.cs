// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.07.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData
{
	public class COfferDto : IMapAble
	{
		[JsonProperty] public string Guid { get; set; }
		[JsonProperty] public CValuableDto Price { get; set; }
		[JsonProperty] public CValuableDto[] Rewards { get; set; }
		[JsonProperty] public COfferParamDto[] Params { get; set; }
		[JsonProperty] public EOfferTag[] Tags { get; set; }
		[JsonProperty] public bool IsSeen { get; set; }

		public COfferDto()
		{
		}

		public COfferDto(
			string guid, 
			CValuableDto price, 
			CValuableDto[] rewards, 
			COfferParamDto[] @params, 
			EOfferTag[] tags,
			bool isSeen
		)
		{
			Guid = guid;
			Price = price;
			IsSeen = isSeen;
			Rewards = rewards;
			Params = @params;
			Tags = tags;
		}
	}
}