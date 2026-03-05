// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.02.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CEventLeaderboardFinishedSignal : IEventBusSignal
	{
		public ELiveEvent LiveEventId { get; }

		public CEventLeaderboardFinishedSignal(ELiveEvent liveEventId)
		{
			LiveEventId = liveEventId;
		}
	}
}