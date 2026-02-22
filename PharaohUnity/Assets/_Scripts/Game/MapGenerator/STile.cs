using System;

namespace Pharaoh.MapGenerator
{
    [Serializable]
    public struct STile
    {
        public int X;
        public int Y;
        public ETileType Type;
        public int RegionId;           // -1 = unassigned
        public EBiomeType BiomeType;   // assigned by CVoronoiRegionStep
        public EObstacleType ObstacleType; // assigned by CObstaclePlacementStep
    }
}