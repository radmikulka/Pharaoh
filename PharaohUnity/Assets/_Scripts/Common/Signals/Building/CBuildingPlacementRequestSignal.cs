using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CBuildingPlacementRequestSignal : IEventBusSignal
	{
		public readonly EBuildingId BuildingId;
		public readonly SCellCoord Cell;

		public CBuildingPlacementRequestSignal(EBuildingId buildingId, SCellCoord cell)
		{
			BuildingId = buildingId;
			Cell = cell;
		}
	}
}
