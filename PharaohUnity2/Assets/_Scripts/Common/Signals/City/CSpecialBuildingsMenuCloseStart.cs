// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.10.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CSpecialBuildingsMenuCloseStart : IEventBusSignal
	{
		public readonly int BuildingPlotIndex;

		public CSpecialBuildingsMenuCloseStart(int buildingPlotIndex)
		{
			BuildingPlotIndex = buildingPlotIndex;
		}
	}
}