// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CRegionActivatedSignal : IEventBusSignal
	{
		public readonly ERegion Region;

		public CRegionActivatedSignal(ERegion region)
		{
			Region = region;
		}
	}
}