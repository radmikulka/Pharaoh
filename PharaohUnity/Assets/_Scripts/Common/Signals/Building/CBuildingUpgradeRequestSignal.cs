using AldaEngine;

namespace Pharaoh
{
	public class CBuildingUpgradeRequestSignal : IEventBusSignal
	{
		public readonly SCellCoord Cell;

		public CBuildingUpgradeRequestSignal(SCellCoord cell)
		{
			Cell = cell;
		}
	}
}
