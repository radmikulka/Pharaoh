// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.11.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CSideCitiesDto
	{
		[JsonProperty] public CSideCityDto[] Cities { get; set; }

		public CSideCitiesDto()
		{
		}

		public CSideCitiesDto(CSideCityDto[] cities)
		{
			Cities = cities;
		}
	}
}