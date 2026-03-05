// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CDispatchersDto
	{
		[JsonProperty] public CDispatcherDto[] Dispatchers { get; set; }

		public CDispatchersDto()
		{
		}

		public CDispatchersDto(CDispatcherDto[] dispatchers)
		{
			Dispatchers = dispatchers;
		}
	}
}