namespace Pharaoh.MapGenerator
{
    public enum ECliffType
    {
        None,
        Straight,     // 1 cardinal water neighbor
        OuterCorner,  // 2 adjacent cardinal water neighbors
        InnerCorner,  // 0 cardinal water, ≥1 diagonal water
        Strait,       // 2 opposite cardinal water neighbors (N+S or E+W)
    }
}
