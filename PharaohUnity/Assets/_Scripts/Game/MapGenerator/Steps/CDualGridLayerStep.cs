using Pharaoh;
using Pharaoh.Map;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Universal dual-grid layer step — spawns corner tiles for one logical layer.
    ///
    /// Add one child GameObject with this component per layer you need:
    ///   • Land terrain  → Predicate = Land
    ///   • Coast biome   → Predicate = ContentTag, ContentTag = Coast
    ///   • Forest edges  → Predicate = ObstacleType, ObstacleType = Tree
    ///
    /// The layer config is resolved from CMapGenConfig based on the predicate + filter values.
    /// Must run after CDualGridCellSetupStep.
    /// </summary>
    public class CDualGridLayerStep : CMapGenerationStepBase
    {
        [SerializeField] private CMapGenConfig _mapGenConfig;
        [SerializeField] private ELayerPredicate _predicate;

        [Tooltip("Used when Predicate = ContentTag.")]
        [SerializeField] private EContentTag _contentTagFilter;

        [Tooltip("Used when Predicate = ObstacleType.")]
        [SerializeField] private EObstacleType _obstacleTypeFilter;

        [SerializeField] private float _yOffset;

        public override string StepName        => $"Dual Grid Layer ({_predicate})";
        public override string StepDescription => "Spawní rohové dlaždice jedné vrstvy duální mřížky na základě zvoleného predikátu.";

        public override void Execute(CMapData mapData, int seed)
        {
            var config = ResolveConfig();
            if (config == null) return;

            var mapInstance = GetMapInstance();
            if (mapInstance == null) return;

            Transform parent = mapInstance.transform;
            int spawnCount   = 0;

            for (int cx = 0; cx <= mapData.Width; cx++)
            {
                for (int cy = 0; cy <= mapData.Height; cy++)
                {
                    EDualGridMask mask = CDualGridUtils.ComputeMask(mapData, cx, cy, Matches);

                    if (mask == EDualGridMask.None) continue;

                    if (!config.Tiles.TryGetValue(mask, out var variant) || variant == null) continue;

                    var go = CDualGridUtils.SpawnCornerTile(variant, cx, cy, _yOffset, parent);
                    if (go != null)
                    {
                        go.name = $"{config.LayerName}_{cx}_{cy}";
                        spawnCount++;
                    }
                }
            }

            Debug.Log($"[{StepName}] Spawned {spawnCount} corner tiles.");
        }

        private CDualGridLayerConfig ResolveConfig()
        {
            if (_mapGenConfig == null)
            {
                Debug.LogWarning($"[{StepName}] MapGenConfig is not assigned — skipping.");
                return null;
            }

            CDualGridLayerConfig config = _predicate switch
            {
                ELayerPredicate.Land         => _mapGenConfig.LandLayer,
                ELayerPredicate.ContentTag   => _mapGenConfig.BiomeLayers?.GetOrDefault(_contentTagFilter),
                ELayerPredicate.ObstacleType => _mapGenConfig.ObstacleLayers?.GetOrDefault(_obstacleTypeFilter),
                _                            => null,
            };

            if (config == null)
                Debug.LogWarning($"[{StepName}] No config found in MapGenConfig for {_predicate}" +
                                 (_predicate == ELayerPredicate.ContentTag   ? $" / {_contentTagFilter}"   : "") +
                                 (_predicate == ELayerPredicate.ObstacleType ? $" / {_obstacleTypeFilter}" : "") +
                                 " — skipping.");
            return config;
        }

        private bool Matches(STile tile) => _predicate switch
        {
            ELayerPredicate.Land         => tile.Type == ETileType.Land,
            ELayerPredicate.ContentTag   => tile.ContentTag == _contentTagFilter,
            ELayerPredicate.ObstacleType => tile.ObstacleType == _obstacleTypeFilter,
            _                            => false,
        };

        private CMapInstance GetMapInstance()
        {
            Transform root = transform.parent;
            var mi = root != null ? root.GetComponentInChildren<CMapInstance>() : null;
            if (mi == null)
                Debug.LogError($"[{StepName}] CMapInstance not found — run CDualGridCellSetupStep first.");
            return mi;
        }
    }
}
