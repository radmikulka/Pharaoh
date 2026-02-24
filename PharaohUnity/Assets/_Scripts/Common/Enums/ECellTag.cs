using System;

namespace Pharaoh
{
	[Flags]
	public enum ECellTag
	{
		None      = 0,
		NearWater = 1 << 0,
		NearRock  = 1 << 1,
		Coastal   = 1 << 2,
		NearSand  = 1 << 3,
	}
}
