// =========================================
// AUTHOR: Juraj Joscak
// DATE:   03.03.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CLiveEventCurrencyTypeChangedSignal : IEventBusSignal
	{
		public readonly ELiveEvent LiveEventId;

		public CLiveEventCurrencyTypeChangedSignal(ELiveEvent liveEventId)
		{
			LiveEventId = liveEventId;
		}
	}
}