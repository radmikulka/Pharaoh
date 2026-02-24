using System;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Flags enum — combine any subset of cardinal directions to define which map edges
    /// are pulled toward water via a falloff gradient.
    ///
    /// Examples:
    ///   AllSides            — continent surrounded by sea on all four sides
    ///   North | East        — sea to the north and east, land to the south and west
    ///   East | West         — corridor / peninsula (same as the old EastWest)
    ///   None                — raw noise only, no edge falloff
    /// </summary>
    [Flags]
    public enum EBorderType
    {
        None  = 0,
        North = 1 << 0,  //  1
        South = 1 << 1,  //  2
        East  = 1 << 2,  //  4
        West  = 1 << 3,  //  8

        // Convenience combinations
        AllSides   = North | South | East | West,
        EastWest   = East  | West,
        NorthSouth = North | South,
    }
}
