using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CBuildingRemovedSignal : IEventBusSignal
	{
		public readonly EBuildingId BuildingId;
		public readonly SCellCoord Cell;

		public CBuildingRemovedSignal(EBuildingId buildingId, SCellCoord cell)
		{
			BuildingId = buildingId;
			Cell = cell;
		}
	}
}
