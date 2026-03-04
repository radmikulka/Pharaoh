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
    /// All 16 combinations are named so CSerializableDictionary's SearchableEnum
    /// dropdown shows readable names in the Inspector.
    /// Integer value = index 0-15.
    /// </summary>
    [Flags]
    public enum EDualGridMask
    {
        None        = 0,                    // 0000 — no active neighbours
        SE          = 1 << 0,              // 0001
        SW          = 1 << 1,              // 0010
        SW_SE       = SW | SE,             // 0011
        NE          = 1 << 2,              // 0100
        NE_SE       = NE | SE,             // 0101
        NE_SW       = NE | SW,             // 0110
        NE_SW_SE    = NE | SW | SE,        // 0111
        NW          = 1 << 3,              // 1000
        NW_SE       = NW | SE,             // 1001
        NW_SW       = NW | SW,             // 1010
        NW_SW_SE    = NW | SW | SE,        // 1011
        NW_NE       = NW | NE,             // 1100
        NW_NE_SE    = NW | NE | SE,        // 1101
        NW_NE_SW    = NW | NE | SW,        // 1110
        All         = NW | NE | SW | SE,   // 1111
    }
}
