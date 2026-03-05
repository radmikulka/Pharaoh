using System;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// 4-bit flags mask for the dual-grid rendering system.
    /// Each flag indicates whether the adjacent logical cell satisfies the layer predicate.
    ///
    /// Corner (cx, cy) neighbours:
    ///   NW = cell (cx-1, cy)     NE = cell (cx, cy)
    ///   SW = cell (cx-1, cy-1)   SE = cell (cx, cy-1)
    ///
    /// CDualGridShapeResolver maps the resulting int (0-15) to an EDualGridShape + rotationY.
    /// </summary>
    [Flags]
    public enum EDualGridMask
    {
        None = 0,
        SE   = 1 << 0,
        SW   = 1 << 1,
        NE   = 1 << 2,
        NW   = 1 << 3,
    }
}
