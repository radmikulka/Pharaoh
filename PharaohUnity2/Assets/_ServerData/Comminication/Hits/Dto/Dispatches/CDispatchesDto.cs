// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.07.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CDispatchesDto
	{
		[JsonProperty] public CDispatchDto[] Dispatches { get; set; }

		public CDispatchesDto()
		{
		}

		public CDispatchesDto(CDispatchDto[] dispatches)
		{
			Dispatches = dispatches;
		}
	}
}
