using System;
using System.Collections.Generic;
using NaughtyAttributes;
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

        // --- Gizmo visualization ---

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_cachedRegionIds == null || _cachedWidth == 0) return;

            var seedPoints = GetComponentsInChildren<CVoronoiSeedPoint>();
            int total = seedPoints.Length;
            if (total == 0) return;

            // Pre-compute colors (1× HSVToRGB per region, not per tile)
            var colors = new Color[total];
            for (int r = 0; r < total; r++)
                colors[r] = Color.HSVToRGB(r / (float)total, 0.7f, 0.9f);

            for (int x = 0; x < _cachedWidth; x++)
            {
                for (int y = 0; y < _cachedHeight; y++)
                {
                    int regionId = _cachedRegionIds[x + y * _cachedWidth];
                    if (regionId >= 0 && regionId < total)
                        Gizmos.color = colors[regionId];
                    Gizmos.DrawCube(new Vector3(x, 0f, y), new Vector3(0.9f, 0.05f, 0.9f));
                }
            }
        }
#endif
    }
}
