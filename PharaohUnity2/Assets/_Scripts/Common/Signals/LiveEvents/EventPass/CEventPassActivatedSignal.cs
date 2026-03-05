// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.01.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CEventPassActivatedSignal : IEventBusSignal
	{
		public ELiveEvent LiveEventId { get; }

		public CEventPassActivatedSignal(ELiveEvent liveEventId)
		{
			LiveEventId = liveEventId;
		}
	}
}