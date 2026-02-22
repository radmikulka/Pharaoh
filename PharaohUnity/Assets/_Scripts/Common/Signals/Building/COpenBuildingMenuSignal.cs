using AldaEngine;

namespace Pharaoh
{
	public class COpenBuildingMenuSignal : IEventBusSignal
	{
		public readonly SCellCoord Cell;

		public COpenBuildingMenuSignal(SCellCoord cell)
		{
			Cell = cell;
		}
	}
}
