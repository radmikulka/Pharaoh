// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CVehicleDto : IMapAble
	{
		[JsonProperty] public EVehicle Id { get; set; }
		[JsonProperty] public ERegion Region { get; set; }
		[JsonProperty] public SVehicleStat[] Stats { get; set; }
		[JsonProperty] public CRechargerDto Durability { get; set; }
		[JsonProperty] public bool IsOwned { get; set; }
		[JsonProperty] public bool Seen { get; set; }

		public CVehicleDto()
		{
		}

		public CVehicleDto(EVehicle id, ERegion region, SVehicleStat[] stats, CRechargerDto durability, bool seen, bool isOwned)
		{
			Id = id;
			Stats = stats;
			Region = region;
			Durability = durability;
			IsOwned = isOwned;
			Seen = seen;
		}
	}
}