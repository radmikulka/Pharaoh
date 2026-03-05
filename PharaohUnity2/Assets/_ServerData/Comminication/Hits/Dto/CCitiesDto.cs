// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CCitiesDto
	{
		[JsonProperty] public CCityDataDto Data { get; set; }
		[JsonProperty] public ESpecialBuilding[] OwnedBuildings { get; set; }

		public CCitiesDto()
		{
		}

		public CCitiesDto(CCityDataDto data, ESpecialBuilding[] ownedBuildings)
		{
			OwnedBuildings = ownedBuildings;
			Data = data;
		}
	}
}