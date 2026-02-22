using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CBuildingPlacedSignal : IEventBusSignal
	{
		public readonly EBuildingId BuildingId;
		public readonly SCellCoord Cell;

		public CBuildingPlacedSignal(EBuildingId buildingId, SCellCoord cell)
		{
			BuildingId = buildingId;
			Cell = cell;
		}
	}
}
