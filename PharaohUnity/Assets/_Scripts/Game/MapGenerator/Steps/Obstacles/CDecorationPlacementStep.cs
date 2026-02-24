using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using AldaEngine;
using UnityEditor;
#endif

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Places decorative vegetation on land tiles using noise + rejection sampling.
    /// Writes to DecorationType (not ObstacleType) — decorations don't block building.
    ///
    /// Mirrors CObstaclePlacementStep pattern:
    ///   Pass 1 — score all valid tiles by noise, take top N (oversampled) as candidates.
    ///   Pass 2 — rejection sampling enforces minimum spacing; stops at _targetCount.
    ///
    /// Add multiple instances for different decoration types (grass, flowers, etc.).
    /// </summary>
    public class CDecorationPlacementStep : CMapGenerationStepBase
    {
        [Header("Decoration")]
        [SerializeField] private EDecorationType _decorationType = EDecorationType.Grass;

        [Header("Density Noise")]
        [Tooltip("Noise config that controls WHERE decorations can appear (high-noise tiles get priority).")]
        [SerializeField] private CNoiseConfig _densityNoise;

        [Header("Placement")]
        [Tooltip("Target number of decorations to place. Actual count may be slightly lower if the map is too small.")]
        [SerializeField] [Min(1)] private int _targetCount = 80;

        [Header("Spacing")]
        [Tooltip("Minimum distance in tiles between any two decorations of this type.")]
        [SerializeField] [Min(0)] private int _minSpacing = 1;

        public override string StepName => $"Decorations ({_decorationType})";
        public override string StepDescription => "Rozmísťuje dekorativní objekty na land políčkách pomocí noise hustoty a rejection samplingu.";

        public override void Execute(CMapData mapData, int seed)
        {
            if (_densityNoise == null)
            {
                Debug.LogError($"[{StepName}] DensityNoise config is not assigned.", this);
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

                    if (tile.Type == ETileType.Water)                    continue;
                    if (tile.ObstaclePrefab != null)                     continue;
                    if (tile.IsObstacleBlocked)                          continue;
                    if (tile.DecorationType != EDecorationType.None)     continue;

                    float nx = x, ny = y;
                    if (_densityNoise.UseDomainWarp)
                        noise.DomainWarp(ref nx, ref ny);

                    float raw        = noise.GetNoise(nx, ny);
                    float normalized = (raw + 1f) / 2f;
                    noiseScores.Add((new Vector2Int(x, y), normalized));
                }
            }

            // Sort descending by noise score
            noiseScores.Sort((a, b) => b.score.CompareTo(a.score));

            // Oversample to account for spacing rejection
            float oversamplingFactor = Mathf.Max(2f, _minSpacing * 2f + 1f);
            int candidateCount = Mathf.Min((int)(_targetCount * oversamplingFactor), noiseScores.Count);

            var candidates = new List<Vector2Int>(candidateCount);
            for (int i = 0; i < candidateCount; i++)
                candidates.Add(noiseScores[i].pos);

            // ── Shuffle candidates (Fisher-Yates, seeded) ──────────────────
            var rng = new System.Random(seed);
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            // ── Pass 2: rejection sampling — enforce spacing ────────────────
            var exclusion = new bool[mapData.Width, mapData.Height];
            int placed = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (placed >= _targetCount) break;
                var pos = candidates[i];
                if (exclusion[pos.x, pos.y]) continue;

                // Place decoration
                STile tile = mapData.Get(pos.x, pos.y);
                tile.DecorationType = _decorationType;
                mapData.Set(pos.x, pos.y, tile);
                placed++;

                // Mark exclusion zone
                if (_minSpacing > 0)
                {
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
            }

            Debug.Log($"[{StepName}] Placed {placed} decorations from {candidates.Count} candidates.");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CDecorationPlacementStep))]
    public class CDecorationPlacementStepEditor : CMapStepEditor
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
