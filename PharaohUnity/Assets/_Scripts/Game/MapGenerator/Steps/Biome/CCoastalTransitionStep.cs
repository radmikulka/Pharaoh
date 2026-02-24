using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Tags Land tiles adjacent to Water with a configurable EContentTag, creating a coastal transition zone.
    /// Run after CFillSmallPoolsStep and before CObstaclePlacementStep.
    /// </summary>
    public class CCoastalTransitionStep : CMapGenerationStepBase
    {
        [Header("Coastal Transition")]
        [Tooltip("Content tag applied to coastal tiles.")]
        [SerializeField] private EContentTag _coastTag = EContentTag.Coast;

        [Tooltip("How many rings of tagged tiles to generate around water edges.")]
        [SerializeField] [Range(1, 10)] private int _sandDepth = 1;

        private static readonly Vector2Int[] CardinalOffsets =
        {
            new(0,  1),
            new(0, -1),
            new(1,  0),
            new(-1, 0),
        };

        public override string StepName => "Coastal Transition";
        public override string StepDescription => "Taguje land políčka sousedící s vodou EContentTag — vytváří pobřežní přechodovou zónu.";

        public override void Execute(CMapData mapData, int seed)
        {
            int tagged = 0;

            // Ring 1: Land adjacent to Water → _coastTag
            tagged += TagAdjacentToWater(mapData);

            // Rings 2..N: Land adjacent to already-tagged tiles → _coastTag
            for (int ring = 2; ring <= _sandDepth; ring++)
                tagged += TagAdjacentToTagged(mapData);

            Debug.Log($"[{StepName}] Tagged {tagged} land tiles (tag={_coastTag}, depth={_sandDepth}).");
        }

        private int TagAdjacentToWater(CMapData mapData)
        {
            var toTag = new List<Vector2Int>();

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);
                    if (tile.Type != ETileType.Land) continue;
                    if (tile.ContentTag != EContentTag.None) continue;
                    if (HasCardinalNeighborOfTileType(mapData, x, y, ETileType.Water))
                        toTag.Add(new Vector2Int(x, y));
                }
            }

            foreach (var pos in toTag)
            {
                STile tile = mapData.Get(pos.x, pos.y);
                tile.ContentTag = _coastTag;
                mapData.Set(pos.x, pos.y, tile);
            }

            return toTag.Count;
        }

        private int TagAdjacentToTagged(CMapData mapData)
        {
            var toTag = new List<Vector2Int>();

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);
                    if (tile.Type != ETileType.Land) continue;
                    if (tile.ContentTag != EContentTag.None) continue;
                    if (HasCardinalNeighborWithTag(mapData, x, y, _coastTag))
                        toTag.Add(new Vector2Int(x, y));
                }
            }

            foreach (var pos in toTag)
            {
                STile tile = mapData.Get(pos.x, pos.y);
                tile.ContentTag = _coastTag;
                mapData.Set(pos.x, pos.y, tile);
            }

            return toTag.Count;
        }

        private bool HasCardinalNeighborOfTileType(CMapData mapData, int x, int y, ETileType type)
        {
            foreach (var offset in CardinalOffsets)
            {
                int nx = x + offset.x;
                int ny = y + offset.y;
                if (mapData.IsValid(nx, ny) && mapData.Get(nx, ny).Type == type)
                    return true;
            }
            return false;
        }

        private bool HasCardinalNeighborWithTag(CMapData mapData, int x, int y, EContentTag tag)
        {
            foreach (var offset in CardinalOffsets)
            {
                int nx = x + offset.x;
                int ny = y + offset.y;
                if (mapData.IsValid(nx, ny) && mapData.Get(nx, ny).ContentTag == tag)
                    return true;
            }
            return false;
        }
    }
}
