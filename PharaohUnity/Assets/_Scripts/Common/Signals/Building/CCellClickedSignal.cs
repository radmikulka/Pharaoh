using AldaEngine;

namespace Pharaoh
{
	public class CCellClickedSignal : IEventBusSignal
	{
		public readonly SCellCoord Cell;

		public CCellClickedSignal(SCellCoord cell)
		{
			Cell = cell;
		}
	}
}
