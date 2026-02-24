using System;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    [Serializable]
    public struct STile
    {
        public int X;
        public int Y;
        public ETileType Type;
        public GameObject ObstaclePrefab;  // set on the anchor tile by obstacle placement steps
        public EObstacleType ObstacleType;     // logical type of the obstacle (e.g. Tree)
        public bool IsObstacleBlocked;    // true for all tiles in a multi-tile formation's footprint
        public EDecorationType DecorationType; // assigned by CDecorationPlacementStep
        public EContentTag ContentTag;         // assigned by CObstacleContentTagStep
        public int VoronoiRegionId; // 0..N-1; 0 is default for unassigned tiles
    }
}
