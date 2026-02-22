using Pharaoh.MapGenerator;
using ServerData;
using UnityEngine;

namespace Pharaoh.Map
{
    public class CMapCell
    {
        public int X { get; }
        public int Y { get; }
        public ETileType TileType { get; }
        public EBiomeType BiomeType { get; }
        public EObstacleType ObstacleType { get; }

        public GameObject TileObject { get; internal set; }     // spawned land tile
        public GameObject ObstacleObject { get; internal set; } // spawned obstacle (null if None)

        public ECellTag Tags { get; internal set; }
        public EBuildingId BuildingId { get; internal set; } = EBuildingId.None;
        public bool HasBuilding => BuildingId != EBuildingId.None;

        public CMapCell(int x, int y, ETileType tileType, EBiomeType biomeType, EObstacleType obstacleType)
        {
            X = x;
            Y = y;
            TileType = tileType;
            BiomeType = biomeType;
            ObstacleType = obstacleType;
        }
    }
}
