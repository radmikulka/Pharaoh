using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CBuildingUpgradedSignal : IEventBusSignal
	{
		public readonly EBuildingId BuildingId;
		public readonly SCellCoord Cell;
		public readonly int NewLevel;

		public CBuildingUpgradedSignal(EBuildingId buildingId, SCellCoord cell, int newLevel)
		{
			BuildingId = buildingId;
			Cell = cell;
			NewLevel = newLevel;
		}
	}
}
