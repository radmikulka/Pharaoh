// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CVehiclesDto
	{
		[JsonProperty] public CVehicleDto[] Vehicles { get; set; }

		public CVehiclesDto()
		{
		}

		public CVehiclesDto(CVehicleDto[] vehicles)
		{
			Vehicles = vehicles;
		}
	}
}