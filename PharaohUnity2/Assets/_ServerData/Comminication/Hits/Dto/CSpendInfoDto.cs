// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.1.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CSpendInfoDto
	{
		[JsonProperty] public float Total { get; set; }
		[JsonProperty] public float Past15Days { get; set; }
		[JsonProperty] public float Past30Days { get; set; }
		[JsonProperty] public float Past60Days { get; set; }
		[JsonProperty] public float Past90Days { get; set; }
		[JsonProperty] public float Past7Days { get; set; }

		public CSpendInfoDto()
		{
		}

		public CSpendInfoDto(
			float total, 
			float past60Days, 
			float past30Days, 
			float past15Days,
			float past90Days,
			float past7Days)
		{
			Total = total;
			Past60Days = past60Days;
			Past30Days = past30Days;
			Past15Days = past15Days;
			Past90Days = past90Days;
			Past7Days = past7Days;
		}
	}
}