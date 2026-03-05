// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.01.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CFutureLifeEventDto
	{
		[JsonProperty] public ELiveEvent Id { get; set; }
		[JsonProperty] public long StartTimeInMs { get; set; }

		public CFutureLifeEventDto()
		{
		}

		public CFutureLifeEventDto(ELiveEvent id, long startTimeInMs)
		{
			Id = id;
			StartTimeInMs = startTimeInMs;
		}
	}
}