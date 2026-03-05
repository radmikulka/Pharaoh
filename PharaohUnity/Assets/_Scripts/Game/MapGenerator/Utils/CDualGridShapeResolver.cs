namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Maps all 16 EDualGridMask values to a (EDualGridShape, rotationY) pair.
    ///
    /// Bit layout:  bit3=NW  bit2=NE  bit1=SW  bit0=SE
    ///
    /// Canonical orientations (0°):
    ///   OuterCorner     — SE active            (terrain in bottom-right quadrant)
    ///   StraightEdge    — SW+SE active          (terrain fills bottom half)
    ///   OppositeCorners — NE+SW active          (terrain in top-right and bottom-left)
    ///   InnerCorner     — NW missing (0111)     (concavity faces NW)
    ///   Full            — all active
    ///
    /// Rotation is CW when viewed from above (Unity Y-up).
    /// </summary>
    public static class CDualGridShapeResolver
    {
        private readonly struct SEntry
        {
            public readonly EDualGridShape Shape;
            public readonly float RotationY;
            public SEntry(EDualGridShape shape, float rotationY) { Shape = shape; RotationY = rotationY; }
        }

        // Index = (int)EDualGridMask, null = None (skip tile)
        private static readonly SEntry?[] _table = new SEntry?[16]
        {
            null,                                          // 0000  None
            new(EDualGridShape.OuterCorner,      0f),      // 0001  SE
            new(EDualGridShape.OuterCorner,     90f),      // 0010  SW
            new(EDualGridShape.StraightEdge,     180f),    // 0011  SW+SE
            new(EDualGridShape.OuterCorner,    270f),      // 0100  NE
            new(EDualGridShape.StraightEdge,    90f),      // 0101  NE+SE
            new(EDualGridShape.OppositeCorners,  0f),      // 0110  NE+SW
            new(EDualGridShape.InnerCorner,      0f),      // 0111  NW missing
            new(EDualGridShape.OuterCorner,    180f),      // 1000  NW
            new(EDualGridShape.OppositeCorners, 90f),      // 1001  NW+SE
            new(EDualGridShape.StraightEdge,   270f),      // 1010  NW+SW
            new(EDualGridShape.InnerCorner,     90f),      // 1011  NE missing
            new(EDualGridShape.StraightEdge,   0f),        // 1100  NW+NE
            new(EDualGridShape.InnerCorner,    270f),      // 1101  SW missing
            new(EDualGridShape.InnerCorner,    180f),      // 1110  SE missing
            new(EDualGridShape.Full,             0f),      // 1111  All
        };

        /// <summary>
        /// Returns false for mask == None (no tile should be spawned).
        /// </summary>
        public static bool TryResolve(EDualGridMask mask, out EDualGridShape shape, out float rotationY)
        {
            var entry = _table[(int)mask];
            if (entry == null) { shape = default; rotationY = 0f; return false; }
            shape     = entry.Value.Shape;
            rotationY = entry.Value.RotationY;
            return true;
        }
    }
}
