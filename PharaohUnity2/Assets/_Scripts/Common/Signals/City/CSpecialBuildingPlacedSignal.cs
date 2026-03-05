// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CSpecialBuildingPlacedSignal : IEventBusSignal
	{
		public readonly ESpecialBuilding BuildingId;
		public readonly int Index;

		public CSpecialBuildingPlacedSignal(ESpecialBuilding buildingId, int index)
		{
			BuildingId = buildingId;
			Index = index;
		}
	}
}