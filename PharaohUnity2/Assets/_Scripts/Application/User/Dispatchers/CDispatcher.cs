// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CDispatcher
	{
		public EDispatcher Id { get; set; }
		public long? ExpirationTime { get; set; }

		public CDispatcher(EDispatcher id, long? expirationTime)
		{
			Id = id;
			ExpirationTime = expirationTime;
		}

		public bool IsExpired(long time)
		{
			return ExpirationTime < time;
		}

		public void SetExpirationTime(long? expirationTime)
		{
			ExpirationTime = expirationTime;
		}
	}
}