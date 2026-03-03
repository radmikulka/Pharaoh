using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CBuildingRemovalRequestSignal : IEventBusSignal
	{
		public readonly SCellCoord Cell;

		public CBuildingRemovalRequestSignal(SCellCoord cell) => Cell = cell;
	}
}
