// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.10.2025
// =========================================

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CModifiedRechargersDto
	{
		[JsonProperty] public CRechargerDto Fuel { get; set; }
		[JsonProperty] public CRechargerDto City { get; set; }
		[JsonProperty] public CModifiedFactoryRechargerDto[] Factories { get; set; }
		[JsonProperty] public CModifiedVehicleRechargerDto[] Vehicles { get; set; }

		public CModifiedRechargersDto(
			CRechargerDto fuel, 
			CRechargerDto city, 
			CModifiedFactoryRechargerDto[] factories,
			CModifiedVehicleRechargerDto[] vehicles
			)
		{
			Factories = factories;
			Vehicles = vehicles;
			Fuel = fuel;
			City = city;
		}

		[OnDeserialized]
		private void ReplaceNulls(StreamingContext context)
		{
			Factories ??= Array.Empty<CModifiedFactoryRechargerDto>();
			Vehicles ??= Array.Empty<CModifiedVehicleRechargerDto>();
		}
	}
}