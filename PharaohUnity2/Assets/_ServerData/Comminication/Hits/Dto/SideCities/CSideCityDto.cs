// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.11.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CSideCityDto
	{
		[JsonProperty] public ECity City { get; set; }
		[JsonProperty] public int CompletedContractsCount { get; set; }

		public CSideCityDto()
		{
		}

		public CSideCityDto(ECity city, int completedContractsCount)
		{
			City = city;
			CompletedContractsCount = completedContractsCount;
		}
	}
}