using NaughtyAttributes;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Step 1 — assigns Voronoi-based region IDs and biome types to land tiles.
    ///
    /// Usage:
    ///   1. Press "Spawn Points" to create evenly distributed CBiomePoint child GameObjects.
    ///   2. On each BiomePoint, set the BiomeType in the Inspector.
    ///   3. Move points freely in the Scene view as needed.
    ///   4. Press "Generate" (on CMapGenerator) to run the full pipeline, OR
    ///      press "Recalculate Regions" to re-assign without re-running prior steps.
    ///
    /// Note: "Recalculate Regions" requires at least one prior "Generate" in this session.
    /// After a domain reload (recompile) you must run Generate again first.
    /// </summary>
    public class CVoronoiRegionStep : MonoBehaviour, IMapGenerationStep
    {
        [Header("Regions")]
        [SerializeField] private int _regionCount = 5;
        [SerializeField] private int _seed = 42;
        [SerializeField] private bool _skipWaterTiles = true;

        public string StepName => "Voronoi Regions";

        // Cached after Execute() — used by RecalculateRegions without re-running the full pipeline.
        // Not serialized: reset on domain reload.
        private CMapData _lastMapData;

        // ─── IMapGenerationStep ─────────────────────────────────────────────

        public void Execute(CMapData mapData, int seed)
        {
            _lastMapData = mapData;

            var points = GetBiomePoints();
            if (points.Length == 0)
            {
                Debug.LogWarning("[CVoronoiRegionStep] No CBiomePoint children found — run \"Spawn Points\" first.", this);
                return;
            }

            AssignVoronoi(mapData, points);
        }

        // ─── Editor Buttons ─────────────────────────────────────────────────

        /// <summary>
        /// Creates CBiomePoint child GameObjects using a jittered grid so points
        /// are distributed evenly across the map rather than clustering randomly.
        /// </summary>
        [Button]
        public void SpawnPoints()
        {
            var generator = GetComponentInParent<CMapGenerator>();
            if (generator == null)
            {
                Debug.LogError("[CVoronoiRegionStep] CMapGenerator not found in parent.", this);
                return;
            }

            // Remove previous points
            foreach (var p in GetBiomePoints())
                DestroyImmediate(p.gameObject);

            int mapW = generator.Width;
            int mapH = generator.Height;

            // Jittered grid: divide map into cols×rows cells, one point per cell
            int cols = Mathf.CeilToInt(Mathf.Sqrt(_regionCount));
            int rows = Mathf.CeilToInt((float)_regionCount / cols);

            float cellW = mapW / (float)cols;
            float cellH = mapH / (float)rows;

            var rng = new System.Random(_seed);
            int id = 0;
            Vector3 origin = generator.transform.position;

            for (int row = 0; row < rows && id < _regionCount; row++)
            {
                for (int col = 0; col < cols && id < _regionCount; col++)
                {
                    float px = (col + (float)rng.NextDouble()) * cellW;
                    float pz = (row + (float)rng.NextDouble()) * cellH;

                    var go = new GameObject($"BiomePoint_{id}");
                    go.transform.SetParent(transform);
                    go.transform.position = origin + new Vector3(px, 0f, pz);

                    var point = go.AddComponent<CBiomePoint>();
                    point.RegionId = id;

                    id++;
                }
            }

            Debug.Log($"[CVoronoiRegionStep] Spawned {id} biome points (grid {cols}×{rows}). Set BiomeType on each point in the Inspector.");

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        /// <summary>
        /// Re-assigns Voronoi regions based on current CBiomePoint positions and BiomeTypes
        /// without re-running earlier pipeline steps. Requires a prior Generate() call.
        /// </summary>
        [Button]
        public void RecalculateRegions()
        {
            if (_lastMapData == null)
            {
                Debug.LogWarning("[CVoronoiRegionStep] No map data in memory — run Generate first.", this);
                return;
            }

            var points = GetBiomePoints();
            if (points.Length == 0)
            {
                Debug.LogWarning("[CVoronoiRegionStep] No CBiomePoint children found.", this);
                return;
            }

            AssignVoronoi(_lastMapData, points);
            Debug.Log("[CVoronoiRegionStep] Regions recalculated.");

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        // ─── Private ────────────────────────────────────────────────────────

        private CBiomePoint[] GetBiomePoints() => GetComponentsInChildren<CBiomePoint>();

        private void AssignVoronoi(CMapData mapData, CBiomePoint[] points)
        {
            var generator = GetComponentInParent<CMapGenerator>();
            Vector3 origin = generator != null ? generator.transform.position : transform.position;

            // Build per-point tile-space positions, region IDs, and biome types
            var tilePoints = new UnityEngine.Vector2[points.Length];
            var regionIds  = new int[points.Length];
            var biomeTypes = new EBiomeType[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 local = points[i].transform.position - origin;
                tilePoints[i] = new UnityEngine.Vector2(local.x, local.z);
                regionIds[i]  = points[i].RegionId;
                biomeTypes[i] = points[i].BiomeType;
            }

            // Assign each tile to its nearest biome point
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);

                    if (_skipWaterTiles && tile.Type == ETileType.Water)
                    {
                        tile.RegionId  = -1;
                        tile.BiomeType = EBiomeType.None;
                        mapData.Set(x, y, tile);
                        continue;
                    }

                    var tilePos = new UnityEngine.Vector2(x, y);
                    float minSqrDist = float.MaxValue;
                    int closestIdx = 0;

                    for (int i = 0; i < tilePoints.Length; i++)
                    {
                        float sqrDist = (tilePos - tilePoints[i]).sqrMagnitude;
                        if (sqrDist < minSqrDist)
                        {
                            minSqrDist = sqrDist;
                            closestIdx = i;
                        }
                    }

                    tile.RegionId  = regionIds[closestIdx];
                    tile.BiomeType = biomeTypes[closestIdx];
                    mapData.Set(x, y, tile);
                }
            }
        }
    }
}