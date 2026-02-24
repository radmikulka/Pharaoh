namespace Pharaoh.MapGenerator
{
    public enum ETileType
    {
        Water,
        Land,
        Coast = 2,
    }

    public static class ETileTypeExtensions
    {
        public static bool IsBuildable(this ETileType type) =>
            type == ETileType.Land || type == ETileType.Coast;
    }
}