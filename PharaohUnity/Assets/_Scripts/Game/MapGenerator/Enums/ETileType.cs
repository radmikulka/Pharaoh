namespace Pharaoh.MapGenerator
{
    public enum ETileType
    {
        Water,
        Land,
    }

    public static class ETileTypeExtensions
    {
        public static bool IsBuildable(this ETileType type) =>
            type == ETileType.Land;
    }
}
