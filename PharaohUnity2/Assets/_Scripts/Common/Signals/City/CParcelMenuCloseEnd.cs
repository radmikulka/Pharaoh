// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.10.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CParcelMenuCloseEnd : IEventBusSignal
	{
		public readonly int BuildingPlotIndex;

		public CParcelMenuCloseEnd(int buildingPlotIndex)
		{
			BuildingPlotIndex = buildingPlotIndex;
		}
	}
}