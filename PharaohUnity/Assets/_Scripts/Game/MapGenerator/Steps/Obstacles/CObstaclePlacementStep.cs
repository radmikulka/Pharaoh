using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using AldaEngine;
using UnityEditor;
#endif

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Places obstacles on land tiles using a two-pass algorithm:
    ///   Pass 1 — score all valid tiles by noise, take top N (oversampled) as candidates.
    ///   Pass 2 — rejection sampling enforces a uniform minimum spacing; stops at _targetCount.
    ///
    /// Add multiple instances of this step for different obstacle types,
    /// each with their own noise config and prefab array.
    /// </summary>
    public class CObstaclePlacementStep : CMapGenerationStepBase
    {
        [Header("Prefabs")]
        [Tooltip("One or more obstacle prefabs. A random one is chosen per tile (seeded).")]
        [SerializeField] private GameObject[] _prefabs;

        [Header("Density Noise")]
        [Tooltip("Noise config that controls WHERE obstacles can appear (high-noise tiles get priority).")]
        [SerializeField] private CNoiseConfig _densityNoise;

        [Header("Placement")]
        [Tooltip("Target number of obstacles to place. Actual count may be slightly lower if the map is too small.")]
        [SerializeField] [Min(1)] private int _targetCount = 50;

        [Header("Spacing")]
        [Tooltip("Minimum distance in tiles between any two obstacles of this type.")]
        [SerializeField] [Min(1)] private int _minSpacing = 2;

        public override string StepName => "Obstacle Placement";
        public override string StepDescription => "Rozmísťuje překážky pomocí noise hustoty s rovnoměrným rozestupem.";

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

            // ── Pass 1: score all valid tiles, take top N by noise ───────────
            // High-noise tiles are preferred; oversample to account for spacing rejects.
            var noiseScores = new List<(Vector2Int pos, float score)>(mapData.Width * mapData.Height);

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);

                    if (!tile.Type.IsBuildable())      continue;
                    if (tile.ObstaclePrefab != null)   continue;
                    if (tile.IsObstacleBlocked)        continue;

                    float nx = x, ny = y;
                    if (_densityNoise.UseDomainWarp)
                        noise.DomainWarp(ref nx, ref ny);

                    float raw        = noise.GetNoise(nx, ny);
                    float normalized = (raw + 1f) / 2f; // −1..1 → 0..1
                    noiseScores.Add((new Vector2Int(x, y), normalized));
                }
            }

            // Sort descending by noise score so highest-density tiles are first
            noiseScores.Sort((a, b) => b.score.CompareTo(a.score));

            // Oversample: each placed item blocks a circle of radius _minSpacing,
            // so we need more candidates than the target count.
            float oversamplingFactor = Mathf.Max(2f, _minSpacing * 2f + 1f);
            int candidateCount = Mathf.Min((int)(_targetCount * oversamplingFactor), noiseScores.Count);

            var candidates = new List<Vector2Int>(candidateCount);
            for (int i = 0; i < candidateCount; i++)
                candidates.Add(noiseScores[i].pos);

            // ── Shuffle candidates (Fisher-Yates) ────────────────────────────
            var rng = new System.Random(seed);
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            // ── Pass 2: rejection sampling — enforce uniform spacing ──────────
            var exclusion = new bool[mapData.Width, mapData.Height];
            int placed = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (placed >= _targetCount) break;
                var pos = candidates[i];
                if (exclusion[pos.x, pos.y]) continue;

                STile tile = mapData.Get(pos.x, pos.y);
                tile.ObstaclePrefab = _prefabs[rng.Next(_prefabs.Length)];
                mapData.Set(pos.x, pos.y, tile);
                placed++;

                int rSq = _minSpacing * _minSpacing;
                for (int dx = -_minSpacing; dx <= _minSpacing; dx++)
                {
                    for (int dy = -_minSpacing; dy <= _minSpacing; dy++)
                    {
                        if (dx * dx + dy * dy > rSq) continue;
                        int nx2 = pos.x + dx, ny2 = pos.y + dy;
                        if (mapData.IsValid(nx2, ny2))
                            exclusion[nx2, ny2] = true;
                    }
                }
            }

            Debug.Log($"[{StepName}] Placed {placed} obstacles from {candidates.Count} candidates.");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CObstaclePlacementStep))]
    public class CObstaclePlacementStepEditor : CMapStepEditor
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
