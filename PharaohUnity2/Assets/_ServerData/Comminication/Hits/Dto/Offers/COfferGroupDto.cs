// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Runtime.Serialization;
using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class COfferGroupDto : IMapAble
	{
		[JsonProperty] public EOfferTag[] Tags { get; set; }
		[JsonProperty] public COfferParamDto[] Params { get; set; }

		public COfferGroupDto()
		{
		}

		public COfferGroupDto(EOfferTag[] tags, COfferParamDto[] groupParams)
		{
			Params = groupParams;
			Tags = tags;
		}
    
		[OnDeserialized]
		private void OnDeserialize(StreamingContext context)
		{
			Tags ??= Array.Empty<EOfferTag>();
			Params ??= Array.Empty<COfferParamDto>();
		}
	}
}