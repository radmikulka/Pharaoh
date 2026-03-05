using NaughtyAttributes;
using Pharaoh.Map;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Universal dual-grid layer step — spawns corner tiles for one logical layer.
    ///
    /// Add one child GameObject with this component per layer you need:
    ///   • Land terrain  → Mode = Land
    ///   • Coast biome   → Mode = ContentTag, ContentTag = Coast
    ///   • Forest edges  → Mode = ObstacleType, ObstacleType = Tree
    ///
    /// Must run after CDualGridCellSetupStep.
    /// </summary>
    public class CDualGridLayerStep : CMapGenerationStepBase
    {
        private enum EMode  { Land, ContentTag, ObstacleType }
        private enum ELayer { L0, L1 }
        private static float LayerY(ELayer l) => l switch { ELayer.L1 => -0.1f, _ => 0f };

        [SerializeField] private CDualGridLayerConfig _config;
        [SerializeField] private EMode _mode;

        [Tooltip("Used when Mode = ContentTag.")]
        [SerializeField] [ShowIf("_mode", EMode.ContentTag)] private EContentTag _contentTagFilter;

        [Tooltip("Used when Mode = ObstacleType.")]
        [SerializeField] [ShowIf("_mode", EMode.ObstacleType)] private EObstacleType _obstacleTypeFilter;

        [SerializeField] private ELayer _layer;

        public override string StepName        => $"Dual Grid Layer ({_mode})";
        public override string StepDescription => "Spawní rohové dlaždice jedné vrstvy duální mřížky na základě zvoleného módu.";

        public override void Execute(CMapData mapData, int seed)
        {
            if (_config == null)
            {
                Debug.LogWarning($"[{StepName}] Config is not assigned — skipping.");
                return;
            }

            var mapInstance = GetMapInstance();
            if (mapInstance == null) return;

            Transform parent = mapInstance.transform;
            int spawnCount   = 0;

            for (int cx = 0; cx <= mapData.Width; cx++)
            {
                for (int cy = 0; cy <= mapData.Height; cy++)
                {
                    EDualGridMask mask = CDualGridUtils.ComputeMask(mapData, cx, cy, Matches);

                    if (!CDualGridShapeResolver.TryResolve(mask, out var shape, out float rotationY)) continue;

                    if (shape == EDualGridShape.Full)
                        rotationY = CDualGridUtils.GetVariantIndex(cx, cy, 4) * 90f;

                    if (!_config.Tiles.TryGetValue(shape, out var variant) || variant == null) continue;

                    var go = CDualGridUtils.SpawnCornerTile(variant, cx, cy, LayerY(_layer), rotationY, parent);
                    if (go != null)
                    {
                        go.name = $"{_config.LayerName}_{cx}_{cy}";
                        spawnCount++;
                    }
                }
            }

            Debug.Log($"[{StepName}] Spawned {spawnCount} corner tiles.");
        }

        private bool Matches(STile tile) => _mode switch
        {
            EMode.Land         => tile.Type == ETileType.Land,
            EMode.ContentTag   => tile.ContentTag == _contentTagFilter,
            EMode.ObstacleType => tile.ObstacleType == _obstacleTypeFilter,
            _                  => false,
        };

        private CMapInstance GetMapInstance()
        {
            return FindFirstObjectByType<CMapInstance>();
        }
    }
}
