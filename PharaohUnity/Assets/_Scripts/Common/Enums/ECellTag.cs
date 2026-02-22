using System;

namespace Pharaoh
{
	[Flags]
	public enum ECellTag
	{
		None       = 0,
		NearForest = 1 << 0,
		NearWater  = 1 << 1,
		NearRock   = 1 << 2,
		NearDesert = 1 << 3,
		Coastal    = 1 << 4,
	}
}
