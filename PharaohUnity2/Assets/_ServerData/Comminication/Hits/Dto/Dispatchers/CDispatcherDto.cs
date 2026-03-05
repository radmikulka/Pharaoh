// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CDispatcherDto
	{
		[JsonProperty] public EDispatcher Id { get; set; }
		[JsonProperty] public long? ExpirationTime { get; set; }

		public CDispatcherDto()
		{
		}

		public CDispatcherDto(EDispatcher id, long? expirationTime)
		{
			Id = id;
			ExpirationTime = expirationTime;
		}
	}
}