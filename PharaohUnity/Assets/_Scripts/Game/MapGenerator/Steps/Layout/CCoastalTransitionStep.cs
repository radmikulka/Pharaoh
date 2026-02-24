using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Converts Land tiles adjacent to Water into Sand, creating a visual transition zone.
    /// Run after CFillSmallPoolsStep and before CObstaclePlacementStep.
    /// </summary>
    public class CCoastalTransitionStep : CMapGenerationStepBase
    {
        [Header("Coastal Transition")]
        [Tooltip("How many rings of sand tiles to generate around water edges.")]
        [SerializeField] [Range(1, 10)] private int _sandDepth = 1;

        private static readonly Vector2Int[] CardinalOffsets =
        {
            new(0,  1),
            new(0, -1),
            new(1,  0),
            new(-1, 0),
        };

        public override string StepName => "Coastal Transition";
        public override string StepDescription => "Převádí land políčka sousedící s vodou na písek — vytváří pobřežní přechodovou zónu.";

        public override void Execute(CMapData mapData, int seed)
        {
            int converted = 0;

            // Ring 1: Land adjacent to Water → Sand
            converted += ConvertAdjacentLand(mapData, ETileType.Water);

            // Rings 2..N: Land adjacent to Sand → Sand (each pass expands by one ring)
            for (int ring = 2; ring <= _sandDepth; ring++)
                converted += ConvertAdjacentLand(mapData, ETileType.Sand);

            Debug.Log($"[{StepName}] Converted {converted} land tiles to sand (depth={_sandDepth}).");
        }

        /// <summary>
        /// Finds all Land tiles cardinally adjacent to tiles of <paramref name="adjacentTo"/> type
        /// and converts them to Sand. Returns the number of tiles converted.
        /// </summary>
        private int ConvertAdjacentLand(CMapData mapData, ETileType adjacentTo)
        {
            // Snapshot which tiles to convert before mutating, to avoid order-dependent results
            var toConvert = new System.Collections.Generic.List<Vector2Int>();

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    if (mapData.Get(x, y).Type != ETileType.Land) continue;

                    if (HasCardinalNeighborOfType(mapData, x, y, adjacentTo))
                        toConvert.Add(new Vector2Int(x, y));
                }
            }

            foreach (var pos in toConvert)
            {
                STile tile = mapData.Get(pos.x, pos.y);
                tile.Type = ETileType.Sand;
                mapData.Set(pos.x, pos.y, tile);
            }

            return toConvert.Count;
        }

        private bool HasCardinalNeighborOfType(CMapData mapData, int x, int y, ETileType type)
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
    }
}
