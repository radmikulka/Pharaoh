using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Places obstacles in clusters: a dense core surrounded by a sparse fringe.
    /// Uses a single noise field with two thresholds:
    ///   noise > DensityThreshold              → core candidate  (tight spacing)
    ///   noise > DensityThreshold * EdgeFactor → fringe candidate (normal spacing)
    ///
    /// Add multiple instances of this step for different obstacle types,
    /// each with their own noise config.
    /// </summary>
    public class CObstacleClusterStep : CMapGenerationStepBase, IHaveNoise
    {
        [SerializeField] private EObstacleType _obstacleType;

        [Header("Density Noise")]
        [Tooltip("Noise config that controls cluster positions and shapes.")]
        [SerializeField] private CNoiseConfig _densityNoise;
        [Tooltip("Noise value above which a tile is a cluster core candidate.")]
        [SerializeField] [Range(0f, 1f)] private float _densityThreshold = 0.60f;

        [Header("Spacing")]
        [Tooltip("Minimum spacing between obstacles in the cluster fringe.")]
        [SerializeField] [Min(0)] private int _minSpacing = 2;
        [Tooltip("Minimum spacing between obstacles in the cluster core (tighter than MinSpacing).")]
        [SerializeField] [Min(0)] private int _coreSpacing = 0;
        [Tooltip("Multiplier on DensityThreshold for fringe candidates. Lower = wider fringe. At 1.0 only core tiles are placed.")]
        [SerializeField] [Range(0.5f, 1f)] private float _edgeFactor = 0.7f;

        [Header("Range")]
        [Tooltip("Radius in cells around this object's position. 0 = no restriction (full map).")]
        [SerializeField] [Min(0)] private int _range = 0;

        public CNoiseConfig NoiseConfig => _densityNoise;

        public override string StepName => "Obstacle Clusters";
        public override string StepDescription => "Rozmísťuje překážky ve shlucích — husté jádro s řidším okrajem, řízeným dvěma prahovými hodnotami noise.";

#if UNITY_EDITOR
        private List<Vector2Int> _placedTiles = new();
#endif

        public override void Execute(CMapData mapData, int seed)
        {
            if (_densityNoise == null)
            {
                Debug.LogError($"[{StepName}] DensityNoise config is not assigned.", this);
                return;
            }

            var noise = _densityNoise.CreateNoise(seed);
            float fringeThreshold = _densityThreshold * _edgeFactor;
            if (fringeThreshold >= 1f) return; // nic nemůže projít — normalized je max 1

            int cx      = Mathf.RoundToInt(transform.position.x);
            int cy      = Mathf.RoundToInt(transform.position.z);
            int rangeSq = _range * _range;

            // ── Pass 1: collect candidates with their noise values ───────────
            var candidates      = new List<Vector2Int>(mapData.Width * mapData.Height / 4);
            var candidateNoise  = new List<float>();

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);

                    if (!tile.Type.IsBuildable())         continue;
                    if (tile.CliffType != ECliffType.None) continue;
                    if (tile.ObstaclePrefab != null)      continue;
                    if (tile.IsObstacleBlocked)           continue;

                    if (_range > 0)
                    {
                        int dx = x - cx, dy = y - cy;
                        if (dx * dx + dy * dy > rangeSq) continue;
                    }

                    float nx = x, ny = y;
                    if (_densityNoise.UseDomainWarp)
                        noise.DomainWarp(ref nx, ref ny);

                    float raw        = _densityNoise.SampleNoise(noise, nx, ny);
                    float normalized = Mathf.Clamp01((raw + 1f) / 2f);

                    if (normalized > fringeThreshold)
                    {
                        candidates.Add(new Vector2Int(x, y));
                        candidateNoise.Add(normalized);
                    }
                }
            }

            // ── Shuffle candidates (Fisher-Yates, keep noise in sync) ────────
            var rng = new System.Random(seed);
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (candidates[i],     candidates[j])     = (candidates[j],     candidates[i]);
                (candidateNoise[i], candidateNoise[j]) = (candidateNoise[j], candidateNoise[i]);
            }

            // ── Pass 2: rejection sampling — spacing depends on core vs fringe
            var exclusion = new int[mapData.Width, mapData.Height];
            int placed = 0;
#if UNITY_EDITOR
            _placedTiles.Clear();
#endif

            for (int i = 0; i < candidates.Count; i++)
            {
                var pos = candidates[i];
                if (exclusion[pos.x, pos.y] > 0) continue;

                STile tile = mapData.Get(pos.x, pos.y);
                tile.ObstacleType = _obstacleType;
                mapData.Set(pos.x, pos.y, tile);
                placed++;
#if UNITY_EDITOR
                _placedTiles.Add(pos);
#endif

                // Core tiles get tight spacing; fringe tiles get normal spacing
                int spacing = candidateNoise[i] > _densityThreshold ? _coreSpacing : _minSpacing;

                if (spacing > 0)
                {
                    int rSq = spacing * spacing;
                    for (int dx = -spacing; dx <= spacing; dx++)
                    {
                        for (int dy = -spacing; dy <= spacing; dy++)
                        {
                            if (dx * dx + dy * dy > rSq) continue;
                            int nx2 = pos.x + dx, ny2 = pos.y + dy;
                            if (mapData.IsValid(nx2, ny2) && exclusion[nx2, ny2] < spacing)
                                exclusion[nx2, ny2] = spacing;
                        }
                    }
                }
            }

            Debug.Log($"[{StepName}] Placed {placed} obstacles from {candidates.Count} candidates.");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if(Selection.activeGameObject != gameObject) 
                return;
            
            if (_placedTiles == null || _placedTiles.Count == 0) return;
            Gizmos.color = new Color(0.8f, 0.3f, 0.1f, 0.7f);
            foreach (var p in _placedTiles)
                Gizmos.DrawCube(new Vector3(p.x, 0f, p.y), new Vector3(0.8f, 0.8f, 0.8f));

            // Draw range circle
            if (_range > 0)
            {
                int cx = Mathf.RoundToInt(transform.position.x);
                int cy = Mathf.RoundToInt(transform.position.z);
                var center = new Vector3(cx, 0f, cy);

                UnityEditor.Handles.color = new Color(1f, 0.8f, 0.1f, 0.8f);
                UnityEditor.Handles.DrawWireDisc(center, Vector3.up, _range);
            }
        }
#endif
    }
}
