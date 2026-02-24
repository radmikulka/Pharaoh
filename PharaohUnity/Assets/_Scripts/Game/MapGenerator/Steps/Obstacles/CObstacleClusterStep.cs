using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using AldaEngine;
using UnityEditor;
#endif

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Places obstacles in clusters: a dense core surrounded by a sparse fringe.
    /// Uses a single noise field with two thresholds:
    ///   noise > DensityThreshold              → core candidate  (tight spacing)
    ///   noise > DensityThreshold * EdgeFactor → fringe candidate (normal spacing)
    ///
    /// Add multiple instances of this step for different obstacle types,
    /// each with their own noise config and prefab array.
    /// </summary>
    public class CObstacleClusterStep : CMapGenerationStepBase
    {
        [Header("Prefabs")]
        [Tooltip("One or more obstacle prefabs. A random one is chosen per tile (seeded).")]
        [SerializeField] private GameObject[] _prefabs;

        [Header("Density Noise")]
        [Tooltip("Noise config that controls cluster positions and shapes.")]
        [SerializeField] private CNoiseConfig _densityNoise;
        [Tooltip("Noise value above which a tile is a cluster core candidate.")]
        [SerializeField] [Range(0f, 1f)] private float _densityThreshold = 0.60f;

        [Header("Spacing")]
        [Tooltip("Minimum spacing between obstacles in the cluster fringe.")]
        [SerializeField] [Min(1)] private int _minSpacing = 2;
        [Tooltip("Minimum spacing between obstacles in the cluster core (tighter than MinSpacing).")]
        [SerializeField] [Min(0)] private int _coreSpacing = 0;
        [Tooltip("Multiplier on DensityThreshold for fringe candidates. Lower = wider fringe.")]
        [SerializeField] [Range(0.5f, 0.95f)] private float _edgeFactor = 0.7f;

        public override string StepName => "Obstacle Clusters";
        public override string StepDescription => "Rozmísťuje překážky ve shlucích — husté jádro s řidším okrajem, řízeným dvěma prahovými hodnotami noise.";

        public override void Execute(CMapData mapData, int seed)
        {
            if (_densityNoise == null)
            {
                Debug.LogError($"[{StepName}] DensityNoise config is not assigned.", this);
                return;
            }

            if (_prefabs == null || _prefabs.Length == 0)
            {
                Debug.LogError($"[{StepName}] Prefabs array is empty.", this);
                return;
            }

            var noise = _densityNoise.CreateNoise(seed);
            float fringeThreshold = _densityThreshold * _edgeFactor;

            // ── Pass 1: collect candidates with their noise values ───────────
            var candidates      = new List<Vector2Int>(mapData.Width * mapData.Height / 4);
            var candidateNoise  = new List<float>();

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);

                    if (!tile.Type.IsBuildable())    continue;
                    if (tile.ObstaclePrefab != null) continue;
                    if (tile.IsObstacleBlocked)      continue;

                    float nx = x, ny = y;
                    if (_densityNoise.UseDomainWarp)
                        noise.DomainWarp(ref nx, ref ny);

                    float raw        = noise.GetNoise(nx, ny);
                    float normalized = (raw + 1f) / 2f; // −1..1 → 0..1

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

            for (int i = 0; i < candidates.Count; i++)
            {
                var pos = candidates[i];
                if (exclusion[pos.x, pos.y] > 0) continue;

                STile tile = mapData.Get(pos.x, pos.y);
                tile.ObstaclePrefab = _prefabs[rng.Next(_prefabs.Length)];
                mapData.Set(pos.x, pos.y, tile);
                placed++;

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
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CObstacleClusterStep))]
    public class CObstacleClusterStepEditor : CMapStepEditor
    {
        private Editor _noiseEditor;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var densityNoise = serializedObject
                .FindProperty("_densityNoise").objectReferenceValue as CNoiseConfig;

            if (densityNoise == null) { _noiseEditor = null; return; }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("— Density Noise Config —", EditorStyles.boldLabel);

            Editor.CreateCachedEditor(densityNoise, null, ref _noiseEditor);
            _noiseEditor.OnInspectorGUI();
        }

        private void OnDisable()
        {
            if (_noiseEditor != null) DestroyImmediate(_noiseEditor);
        }
    }
#endif
}
