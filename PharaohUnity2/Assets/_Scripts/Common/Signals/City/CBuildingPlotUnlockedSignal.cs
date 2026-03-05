// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CBuildingPlotUnlockedSignal : IEventBusSignal
	{
		public readonly int Index;

		public CBuildingPlotUnlockedSignal(int index)
		{
			Index = index;
		}
	}
}