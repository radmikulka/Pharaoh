namespace Pharaoh.Building
{
	// Bitmask: bit3 = North, bit2 = South, bit1 = East, bit0 = West
	// Matches CMapInstance.CardinalOffsets order: N(0,1) S(0,-1) E(1,0) W(-1,0)
	public static class CRoadVariantResolver
	{
		private static readonly SRoadVariant[] Lookup = new SRoadVariant[16]
		{
			// 0000 — isolated: use Cross as fallback
			new SRoadVariant(ERoadShape.Cross,      0f),
			// 0001 — W only
			new SRoadVariant(ERoadShape.DeadEnd,    270f),
			// 0010 — E only
			new SRoadVariant(ERoadShape.DeadEnd,    90f),
			// 0011 — E+W
			new SRoadVariant(ERoadShape.Straight,   90f),
			// 0100 — S only
			new SRoadVariant(ERoadShape.DeadEnd,    180f),
			// 0101 — S+W
			new SRoadVariant(ERoadShape.Corner,     180f),
			// 0110 — S+E
			new SRoadVariant(ERoadShape.Corner,     90f),
			// 0111 — S+E+W
			new SRoadVariant(ERoadShape.TJunction,  180f),
			// 1000 — N only
			new SRoadVariant(ERoadShape.DeadEnd,    0f),
			// 1001 — N+W
			new SRoadVariant(ERoadShape.Corner,     270f),
			// 1010 — N+E
			new SRoadVariant(ERoadShape.Corner,     0f),
			// 1011 — N+E+W
			new SRoadVariant(ERoadShape.TJunction,  0f),
			// 1100 — N+S
			new SRoadVariant(ERoadShape.Straight,   0f),
			// 1101 — N+S+W
			new SRoadVariant(ERoadShape.TJunction,  270f),
			// 1110 — N+S+E
			new SRoadVariant(ERoadShape.TJunction,  90f),
			// 1111 — all four
			new SRoadVariant(ERoadShape.Cross,      0f),
		};

		public static SRoadVariant Resolve(int mask) => Lookup[mask & 0xF];
	}
}
