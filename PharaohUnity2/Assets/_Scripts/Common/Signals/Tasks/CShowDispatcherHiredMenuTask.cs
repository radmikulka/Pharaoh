// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.12.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CShowDispatcherHiredMenuTask
	{
		public readonly EDispatcher DispatcherId;
		public readonly long? ExpirationTime;

		public CShowDispatcherHiredMenuTask(EDispatcher dispatcherId, long? expirationTime)
		{
			DispatcherId = dispatcherId;
			ExpirationTime = expirationTime;
		}
	}
}