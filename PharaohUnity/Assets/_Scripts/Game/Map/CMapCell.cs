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
        public EDecorationType DecorationType { get; }
        public EContentTag ContentTag { get; }

        public GameObject TileObject { get; internal set; }     // spawned land tile
        public GameObject ObstacleObject { get; internal set; } // spawned obstacle (null if none)
        public GameObject DecorationObject { get; internal set; } // spawned decoration (null if none)

        public ECellTag Tags { get; internal set; }
        public EBuildingId BuildingId { get; internal set; } = EBuildingId.None;
        public bool HasBuilding => BuildingId != EBuildingId.None;

        public CMapCell(int x, int y, ETileType tileType, EDecorationType decorationType, EContentTag contentTag = EContentTag.None)
        {
            X = x;
            Y = y;
            TileType = tileType;
            DecorationType = decorationType;
            ContentTag = contentTag;
        }
    }
}
