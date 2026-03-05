// =========================================
// AUTHOR: Marek Karaba
// DATE:   27.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CCityUpgradeAnimationCompletedSignal : IEventBusSignal
	{
		public readonly ERegion Region;
		
		public CCityUpgradeAnimationCompletedSignal(ERegion region)
		{
			Region = region;
		}
	}
}