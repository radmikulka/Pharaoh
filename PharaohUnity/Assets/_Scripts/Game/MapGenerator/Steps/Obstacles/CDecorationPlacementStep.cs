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
    ///   Pass 1 — noise threshold filter produces candidate tiles.
    ///   Pass 2 — rejection sampling enforces minimum spacing.
    ///
    /// Add multiple instances for different decoration types (grass, flowers, etc.).
    /// </summary>
    public class CDecorationPlacementStep : CMapGenerationStepBase
    {
        [Header("Decoration")]
        [SerializeField] private EDecorationType _decorationType = EDecorationType.Grass;

        [Header("Density Noise")]
        [Tooltip("Noise config that controls WHERE decorations can appear.")]
        [SerializeField] private CNoiseConfig _densityNoise;
        [Tooltip("Normalized noise value above which a tile becomes a candidate (0=full coverage, 1=nothing).")]
        [SerializeField] [Range(0f, 1f)] private float _densityThreshold = 0.50f;

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

            // ── Pass 1: collect candidate tiles via noise threshold ──────────
            var candidates = new List<Vector2Int>(mapData.Width * mapData.Height / 4);

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

                    float raw = noise.GetNoise(nx, ny);
                    float normalized = (raw + 1f) / 2f;

                    if (normalized > _densityThreshold)
                        candidates.Add(new Vector2Int(x, y));
                }
            }

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
