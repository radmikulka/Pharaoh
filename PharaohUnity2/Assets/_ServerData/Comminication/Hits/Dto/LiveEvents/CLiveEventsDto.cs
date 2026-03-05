// =========================================
// AUTHOR: Radek Mikulka
// DATE:   4.3.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CLiveEventsDto
	{
		[JsonProperty] public CLiveEventDto[] LiveEvents { get; set; }
		[JsonProperty] public CFutureLifeEventDto[] FutureEvents { get; set; }

		public CLiveEventsDto()
		{
		}

		public CLiveEventsDto(CLiveEventDto[] liveEvents, CFutureLifeEventDto[] futureEvents)
		{
			LiveEvents = liveEvents;
			FutureEvents = futureEvents;
		}
	}
}

