// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CSpecialBuildingBoughtSignal : IEventBusSignal
	{
		public readonly ESpecialBuilding BuildingId;

		public CSpecialBuildingBoughtSignal(ESpecialBuilding buildingId)
		{
			BuildingId = buildingId;
		}
	}
}