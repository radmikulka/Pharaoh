// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CRegionLiveEventGameModeLoadedSignal : IEventBusSignal
	{
		public readonly ELiveEvent EventId;

		public CRegionLiveEventGameModeLoadedSignal(ELiveEvent eventId)
		{
			EventId = eventId;
		}
	}
}