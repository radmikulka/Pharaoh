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
        public bool IsObstacleBlocked;    // true for all tiles in a multi-tile formation's footprint
        public EDecorationType DecorationType; // assigned by CDecorationPlacementStep
    }
}
