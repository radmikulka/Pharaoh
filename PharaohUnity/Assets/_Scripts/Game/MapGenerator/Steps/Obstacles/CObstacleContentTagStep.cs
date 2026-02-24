using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Scans obstacle tiles and tags nearby free buildable tiles with a content type.
    /// E.g. tiles near Tree obstacles get tagged HuntersLodge, enabling placement of a Hunter's Lodge there.
    /// Place after CObstaclePlacementStep / CObstacleClusterStep so ObstacleType is populated.
    /// </summary>
    public class CObstacleContentTagStep : CMapGenerationStepBase
    {
        [SerializeField] EObstacleType _obstacleType = EObstacleType.Tree;
        [SerializeField] EContentTag _contentTag = EContentTag.HuntersLodge;
        [SerializeField] int _radius = 3;

        public override string StepName => "Obstacle Content Tags";
        public override string StepDescription => "Tags buildable tiles near matching obstacles with a content type (e.g. tiles near Trees → HuntersLodge).";

        public override void Execute(CMapData mapData, int seed)
        {
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
                            if (dx * dx + dy * dy > radiusSq)
                                continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (!mapData.IsValid(nx, ny))
                                continue;

                            STile neighbor = mapData.Get(nx, ny);

                            if (!neighbor.Type.IsBuildable())
                                continue;

                            if (neighbor.IsObstacleBlocked)
                                continue;

                            if (neighbor.ContentTag != EContentTag.None)
                                continue;

                            neighbor.ContentTag = _contentTag;
                            mapData.Set(nx, ny, neighbor);
                        }
                    }
                }
            }
        }
    }
}
