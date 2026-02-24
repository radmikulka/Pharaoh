using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Tags buildable Land tiles near obstacles of a given type with a configurable EContentTag.
    /// E.g. tiles near Tree obstacles get tagged HuntersLodge, enabling placement of a Hunter's Lodge.
    /// Place after CObstaclePlacementStep / CObstacleClusterStep so ObstacleType is populated.
    /// </summary>
    public class CObstacleProximityTagStep : CMapGenerationStepBase
    {
        [Header("Source")]
        [Tooltip("Only obstacles of this type are used as tag origins.")]
        [SerializeField] private EObstacleType _obstacleType = EObstacleType.Tree;

        [Header("Tag")]
        [Tooltip("Content tag to apply to nearby tiles.")]
        [SerializeField] private EContentTag _contentTag = EContentTag.HuntersLodge;

        [Tooltip("Radius in tiles around each matching obstacle within which tiles are tagged.")]
        [SerializeField] [Min(1)] private int _radius = 3;

        public override string StepName => "Obstacle Proximity Tag";
        public override string StepDescription => "Taguje land políčka v okolí překážek daného typu zadaným EContentTag.";

        public override void Execute(CMapData mapData, int seed)
        {
            int tagged = 0;
            int radiusSq = _radius * _radius;

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    if (mapData.Get(x, y).ObstacleType != _obstacleType)
                        continue;

                    for (int dx = -_radius; dx <= _radius; dx++)
                    {
                        for (int dy = -_radius; dy <= _radius; dy++)
                        {
                            if (dx * dx + dy * dy > radiusSq) continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (!mapData.IsValid(nx, ny)) continue;

                            STile neighbor = mapData.Get(nx, ny);
                            if (neighbor.Type != ETileType.Land) continue;
                            if (neighbor.IsObstacleBlocked) continue;
                            if (neighbor.ContentTag != EContentTag.None) continue;

                            neighbor.ContentTag = _contentTag;
                            mapData.Set(nx, ny, neighbor);
                            tagged++;
                        }
                    }
                }
            }

            Debug.Log($"[{StepName}] Tagged {tagged} tiles (obstacle={_obstacleType}, tag={_contentTag}, radius={_radius}).");
        }
    }
}
