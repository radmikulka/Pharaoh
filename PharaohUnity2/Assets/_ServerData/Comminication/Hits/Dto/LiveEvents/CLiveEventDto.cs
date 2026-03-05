// =========================================
// AUTHOR: Radek Mikulka
// DATE:   4.3.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CLiveEventDto
	{
		[JsonProperty] public ELiveEvent Id { get; set; }
		[JsonProperty] public long EndTimeInMs { get; set; }
		[JsonProperty] public long CancellationTimeInMs { get; set; }
		[JsonProperty] public long StartTimeInMs { get; set; }
		[JsonProperty] public bool IsSeen { get; set; }
		[JsonProperty] public ILiveEventContentDto Content { get; set; }

		public CLiveEventDto()
		{
		}

		public CLiveEventDto(
			ELiveEvent id, 
			bool isSeen,
			long endTimeInMs, 
			long startTimeInMs, 
			long cancellationTimeInMs, 
			ILiveEventContentDto content
			)
		{
			CancellationTimeInMs = cancellationTimeInMs;
			StartTimeInMs = startTimeInMs;
			EndTimeInMs = endTimeInMs;
			Content = content;
			IsSeen = isSeen;
			Id = id;
		}
	}
}
