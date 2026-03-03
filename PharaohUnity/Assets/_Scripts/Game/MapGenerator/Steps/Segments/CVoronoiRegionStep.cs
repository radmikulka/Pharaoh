using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Rozdělí mapu na N Voronoiových regionů pomocí stratifikovaného vzorkování.
    /// Každé políčko obdrží STile.VoronoiRegionId (0..N-1).
    /// Persistent seed pointy (CVoronoiSeedPoint.IsPersistent = true) přežijí regeneraci.
    /// </summary>
    public class CVoronoiRegionStep : CMapGenerationStepBase
    {
        [SerializeField] [Min(1)] private int _regionCount = 30;
        [SerializeField] [Range(0f, 1f)] private float _jitterAmount = 0.5f;

        [Header("Local Seed")]
        [SerializeField] private int _localSeed;

        [Button("Generate New Seed")]
        private void GenerateNewSeed()
        {
            _localSeed = new System.Random(Environment.TickCount).Next();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public override string StepName => "Voronoi Region";
        public override string StepDescription => "Rozdělí mapu na N Voronoiových regionů. Každé políčko dostane VoronoiRegionId (0..N-1).";

        // Gizmo cache
        private int[] _cachedRegionIds;
        private int _cachedWidth;
        private int _cachedHeight;

        public override void Execute(CMapData mapData, int seed)
        {
            // --- 1. Sesbírat a vyčistit child seed pointy ---
            var all = GetComponentsInChildren<CVoronoiSeedPoint>();
            var persistent = new List<CVoronoiSeedPoint>();

            foreach (var sp in all)
            {
                if (sp.IsPersistent)
                    persistent.Add(sp);
                else
                    DestroyImmediate(sp.gameObject);
            }

            // --- 2. Edge cases ---
            int newCount = Mathf.Max(0, _regionCount - persistent.Count);
            int totalRegions = persistent.Count + newCount;

            if (totalRegions == 0)
            {
                Debug.LogWarning("[CVoronoiRegionStep] totalRegions == 0, skipping.", this);
                return;
            }

            if (persistent.Count > _regionCount)
                Debug.LogWarning($"[CVoronoiRegionStep] Persistent seeds ({persistent.Count}) exceed _regionCount ({_regionCount}). All persistent seeds will be used.", this);

            // --- 3. Přiřadit RegionId persistent seedům (setřídit podle SiblingIndex pro stabilitu) ---
            persistent.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            for (int i = 0; i < persistent.Count; i++)
            {
                persistent[i].RegionId = i;
                persistent[i].TotalRegions = totalRegions;
            }

            // --- 4. Stratifikované vzorkování pro nové body ---
            var rng = new System.Random(seed ^ _localSeed);
            int cols = Mathf.CeilToInt(Mathf.Sqrt(newCount));
            int rows = Mathf.CeilToInt(newCount / (float)Mathf.Max(cols, 1));
            float cellW = mapData.Width  / (float)Mathf.Max(cols, 1);
            float cellH = mapData.Height / (float)Mathf.Max(rows, 1);

            int created = 0;
            for (int row = 0; row < rows && created < newCount; row++)
            {
                for (int col = 0; col < cols && created < newCount; col++)
                {
                    float cx = col * cellW + cellW * 0.5f;
                    float cy = row * cellH + cellH * 0.5f;

                    Vector2 jitter = RandomInsideUnitCircle(rng) * _jitterAmount * Mathf.Min(cellW, cellH) * 0.5f;
                    float px = Mathf.Clamp(cx + jitter.x, 0f, mapData.Width  - 1f);
                    float py = Mathf.Clamp(cy + jitter.y, 0f, mapData.Height - 1f);

                    // --- 5. Vytvořit GameObject pro nový seed point ---
                    int regionId = persistent.Count + created;
                    var go = new GameObject($"VoronoiSeed_{regionId}");
                    go.transform.SetParent(transform, worldPositionStays: false);
                    go.transform.position = new Vector3(px, 0f, py);

                    var sp = go.AddComponent<CVoronoiSeedPoint>();
                    sp.IsPersistent = false;
                    sp.RegionId = regionId;
                    sp.TotalRegions = totalRegions;

                    created++;
                }
            }

            // --- 6. Sestavit pole seed pozic (world XZ → map XY) ---
            var allSeeds = GetComponentsInChildren<CVoronoiSeedPoint>();
            // Sort by RegionId so index == RegionId
            Array.Sort(allSeeds, (a, b) => a.RegionId.CompareTo(b.RegionId));

            var seedPositions = new Vector2[allSeeds.Length];
            for (int i = 0; i < allSeeds.Length; i++)
            {
                var pos = allSeeds[i].transform.position;
                seedPositions[i] = new Vector2(pos.x, pos.z);
            }

            // --- 7. Voronoi assignment ---
            int w = mapData.Width;
            int h = mapData.Height;
            _cachedRegionIds = new int[w * h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    STile tile = mapData.Get(x, y);
                    int regionId = FindNearestRegion(x, y, seedPositions);
                    tile.VoronoiRegionId = regionId;
                    mapData.Set(x, y, tile);
                    _cachedRegionIds[x + y * w] = regionId;
                }
            }

            // --- 8. Uložit gizmo cache ---
            _cachedWidth  = w;
            _cachedHeight = h;
        }

        // --- Fast reassignment (no destroy/create, used when only seed positions change) ---

        /// <summary>
        /// Re-assigns VoronoiRegionId for every tile based on current seed point positions.
        /// Does NOT destroy or create any GameObjects — only reads existing CVoronoiSeedPoint children.
        /// Uses grid bucketing for O(1) average nearest-seed lookup.
        /// </summary>
        public void ReAssignVoronoi(CMapData mapData)
        {
            var allSeeds = GetComponentsInChildren<CVoronoiSeedPoint>();
            if (allSeeds.Length == 0) return;
            Array.Sort(allSeeds, (a, b) => a.RegionId.CompareTo(b.RegionId));

            var seedPositions = new Vector2[allSeeds.Length];
            for (int i = 0; i < allSeeds.Length; i++)
            {
                var pos = allSeeds[i].transform.position;
                seedPositions[i] = new Vector2(pos.x, pos.z);
            }

            int w = mapData.Width, h = mapData.Height;
            if (_cachedRegionIds == null || _cachedRegionIds.Length != w * h)
                _cachedRegionIds = new int[w * h];

            // Build spatial grid once for O(1) average nearest-seed lookup per tile.
            float cellSize = Mathf.Max(1f, Mathf.Sqrt(w * h / (float)allSeeds.Length));
            int gridCols   = Mathf.Max(1, Mathf.CeilToInt(w / cellSize));
            int gridRows   = Mathf.Max(1, Mathf.CeilToInt(h / cellSize));
            var buckets    = new List<int>[gridCols * gridRows];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = new List<int>();
            for (int i = 0; i < seedPositions.Length; i++)
            {
                int col = Mathf.Clamp((int)(seedPositions[i].x / cellSize), 0, gridCols - 1);
                int row = Mathf.Clamp((int)(seedPositions[i].y / cellSize), 0, gridRows - 1);
                buckets[col + row * gridCols].Add(i);
            }

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                STile tile     = mapData.Get(x, y);
                int   regionId = FindNearestRegionFast(x, y, seedPositions, cellSize, gridCols, gridRows, buckets);
                tile.VoronoiRegionId = regionId;
                mapData.Set(x, y, tile);
                _cachedRegionIds[x + y * w] = regionId;
            }

            _cachedWidth  = w;
            _cachedHeight = h;
        }

        // --- Helper methods ---

        private static Vector2 RandomInsideUnitCircle(System.Random rng)
        {
            while (true)
            {
                float x = (float)(rng.NextDouble() * 2.0 - 1.0);
                float y = (float)(rng.NextDouble() * 2.0 - 1.0);
                if (x * x + y * y <= 1f) return new Vector2(x, y);
            }
        }

        private static int FindNearestRegion(int tx, int ty, Vector2[] seeds)
        {
            float best = float.MaxValue;
            int id = 0;
            for (int i = 0; i < seeds.Length; i++)
            {
                float dx = tx - seeds[i].x;
                float dy = ty - seeds[i].y;
                float d  = dx * dx + dy * dy;
                if (d < best) { best = d; id = i; }
            }
            return id;
        }

        /// <summary>
        /// Grid-bucket nearest-seed lookup. Searches the tile's cell and its 8 neighbours (3×3 window).
        /// Falls back to brute-force when the 3×3 window contains no seeds (sparse edge case).
        /// </summary>
        private static int FindNearestRegionFast(int tx, int ty, Vector2[] seeds,
            float cellSize, int gridCols, int gridRows, List<int>[] buckets)
        {
            int tileCol = Mathf.Clamp((int)(tx / cellSize), 0, gridCols - 1);
            int tileRow = Mathf.Clamp((int)(ty / cellSize), 0, gridRows - 1);

            float best  = float.MaxValue;
            int   bestId = 0;
            bool  found  = false;

            for (int dc = -1; dc <= 1; dc++)
            for (int dr = -1; dr <= 1; dr++)
            {
                int nc = tileCol + dc;
                int nr = tileRow + dr;
                if (nc < 0 || nc >= gridCols || nr < 0 || nr >= gridRows) continue;
                foreach (int si in buckets[nc + nr * gridCols])
                {
                    float dx = tx - seeds[si].x;
                    float dy = ty - seeds[si].y;
                    float d  = dx * dx + dy * dy;
                    if (d < best) { best = d; bestId = si; found = true; }
                }
            }

            return found ? bestId : FindNearestRegion(tx, ty, seeds);
        }

        // --- Gizmo visualization ---

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() => DrawRegionGizmos();

        public void DrawRegionGizmos()
        {
            if(Selection.activeGameObject != gameObject)
                return;
            
            if (_cachedRegionIds == null || _cachedWidth == 0) return;

            var seedPoints = GetComponentsInChildren<CVoronoiSeedPoint>();
            int total = seedPoints.Length;
            if (total == 0) return;

            // --- Edge visualization ---
            Gizmos.color = Color.black;

            // Horizontal boundaries (between (x,y) and (x+1,y))
            for (int x = 0; x < _cachedWidth - 1; x++)
            {
                for (int y = 0; y < _cachedHeight; y++)
                {
                    int r1 = _cachedRegionIds[x + y * _cachedWidth];
                    int r2 = _cachedRegionIds[(x + 1) + y * _cachedWidth];
                    if (r1 != r2)
                        Gizmos.DrawLine(new Vector3(x + 0.5f, 1f, y - 0.5f),
                                        new Vector3(x + 0.5f, 1f, y + 0.5f));
                }
            }

            // Vertical boundaries (between (x,y) and (x,y+1))
            for (int x = 0; x < _cachedWidth; x++)
            {
                for (int y = 0; y < _cachedHeight - 1; y++)
                {
                    int r1 = _cachedRegionIds[x + y * _cachedWidth];
                    int r2 = _cachedRegionIds[x + (y + 1) * _cachedWidth];
                    if (r1 != r2)
                        Gizmos.DrawLine(new Vector3(x - 0.5f, 1f, y + 0.5f),
                                        new Vector3(x + 0.5f, 1f, y + 0.5f));
                }
            }
        }
#endif
    }
}
