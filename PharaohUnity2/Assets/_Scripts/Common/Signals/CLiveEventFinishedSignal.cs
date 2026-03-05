// =========================================
// AUTHOR: Marek Karaba
// DATE:   11.12.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CLiveEventFinishedSignal : IEventBusSignal
	{
		public readonly ELiveEvent LiveEventId;

		public CLiveEventFinishedSignal(ELiveEvent liveEventId)
		{
			LiveEventId = liveEventId;
		}
	}
}