namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// The 5 distinct visual tile shapes in the dual-grid system.
    /// CDualGridShapeResolver maps any EDualGridMask to one of these shapes plus a Y rotation.
    /// </summary>
    public enum EDualGridShape
    {
        OuterCorner,     // 1 active neighbour  — 4 rotations
        StraightEdge,    // 2 adjacent active    — 4 rotations
        OppositeCorners, // 2 diagonal active    — 2 rotations
        InnerCorner,     // 3 active neighbours  — 4 rotations
        Full,            // all 4 active         — 1 rotation
    }
}
