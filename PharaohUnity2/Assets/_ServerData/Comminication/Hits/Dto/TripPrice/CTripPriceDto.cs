// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.09.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CTripPriceDto : IMapAble
	{
		[JsonProperty] public int FuelPrice { get; set; }
		[JsonProperty] public int DurabilityPrice { get; set; }

		public CTripPriceDto()
		{
		}

		public CTripPriceDto(int fuelPrice, int durabilityPrice)
		{
			DurabilityPrice = durabilityPrice;
			FuelPrice = fuelPrice;
		}
	}
}