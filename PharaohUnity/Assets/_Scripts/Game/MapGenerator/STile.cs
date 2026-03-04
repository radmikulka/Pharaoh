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
        public EContentTag ContentTag;         // assigned by CObstacleProximityTagStep
        public int VoronoiRegionId; // 0..N-1; 0 is default for unassigned tiles

        /// <summary>Deprecated — cliff geometry is now handled by CDualGridTerrainStep prefab configs.</summary>
        [Obsolete("Cliff geometry is handled by the dual grid terrain step. CliffType is no longer used by the spawn pipeline.")]
        public ECliffType CliffType;

        /// <summary>Deprecated — cliff rotation is now handled by CDualGridTerrainStep prefab configs.</summary>
        [Obsolete("Cliff rotation is handled by the dual grid terrain step. CliffRotationDeg is no longer used by the spawn pipeline.")]
        public int CliffRotationDeg;
    }
}
