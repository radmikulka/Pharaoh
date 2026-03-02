using AldaEngine;

namespace Pharaoh
{
	public class COpenBuildingDetailSignal : IEventBusSignal
	{
		public readonly SCellCoord Cell;

		public COpenBuildingDetailSignal(SCellCoord cell)
		{
			Cell = cell;
		}
	}
}
