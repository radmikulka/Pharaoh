// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CSpecialBuildingsTabClosed : IEventBusSignal
	{
		public readonly int BuildingPlotIndex;

		public CSpecialBuildingsTabClosed(int buildingPlotIndex)
		{
			BuildingPlotIndex = buildingPlotIndex;
		}
	}
}