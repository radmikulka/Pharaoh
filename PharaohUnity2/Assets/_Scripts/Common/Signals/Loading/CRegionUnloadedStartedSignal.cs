// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CRegionUnloadedStartedSignal : IEventBusSignal
	{
		public readonly ERegion Region;

		public CRegionUnloadedStartedSignal(ERegion region)
		{
			Region = region;
		}
	}
}