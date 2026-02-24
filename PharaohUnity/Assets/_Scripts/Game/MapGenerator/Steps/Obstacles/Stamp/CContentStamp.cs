using System.Collections.Generic;
using AldaEngine;
using NaughtyAttributes;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    public enum EContentStampMode
    {
        Decoration,
        Obstacle,
    }

    /// <summary>
    /// Manually positioned content stamp. Place this as a child of CContentStep,
    /// position it in the Scene view, and it will bake decoration or obstacle content
    /// at that location during generation.
    /// </summary>
    public class CContentStamp : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private EContentStampMode _mode;

        [ShowIf("IsDecoration")]
        [SerializeField] private EDecorationType _decorationType;

        [ShowIf("IsObstacle")]
        [SerializeField] private GameObject _obstaclePrefab;

        [ShowIf("IsObstacle")]
        [SerializeField] private EObstacleType _obstacleType;

        [Header("Shape")]
        [SerializeField] private SIntMinMaxRange _sizeRange = new(10, 40);
        [SerializeField] [Range(0f, 1f)] private float _irregularity = 0.5f;
        [SerializeField] [Min(0.001f)] private float _noiseFrequency = 0.1f;

        [Header("Local Seed")]
        [SerializeField] private int _localSeed;

        private bool IsDecoration => _mode == EContentStampMode.Decoration;
        private bool IsObstacle => _mode == EContentStampMode.Obstacle;

        private List<Vector2Int> _previewTiles = new();

        [Button("Generate New Seed")]
        private void GenerateNewSeed()
        {
            _localSeed = new System.Random(System.Environment.TickCount).Next();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            RefreshPreview();
        }

        private void OnValidate()
        {
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            _previewTiles = new List<Vector2Int>();
            int targetSize = (_sizeRange.Min + _sizeRange.Max) / 2;
            GrowVirtual(Vector2Int.zero, targetSize, _localSeed, _previewTiles);
        }

        private void GrowVirtual(Vector2Int seed, int targetSize, int noiseSeed, List<Vector2Int> result)
        {
            var noise = new FastNoiseLite(noiseSeed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(_noiseFrequency);

            var filled = new HashSet<Vector2Int>();
            var frontier = new List<Vector2Int> { seed };
            var rng = new System.Random(noiseSeed);

            filled.Add(seed);
            result.Add(seed);

            Vector2Int[] offsets = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };

            while (filled.Count < targetSize && frontier.Count > 0)
            {
                int idx = rng.Next(frontier.Count);
                Vector2Int current = frontier[idx];
                frontier.RemoveAt(idx);

                foreach (var offset in offsets)
                {
                    Vector2Int neighbor = current + offset;
                    if (filled.Contains(neighbor)) continue;

                    float raw = noise.GetNoise(neighbor.x, neighbor.y);
                    float normalized = (raw + 1f) / 2f;
                    float threshold = _irregularity * 0.6f;

                    if (normalized < threshold) continue;

                    filled.Add(neighbor);
                    result.Add(neighbor);
                    frontier.Add(neighbor);

                    if (filled.Count >= targetSize) break;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_previewTiles == null || _previewTiles.Count == 0)
                RefreshPreview();

            Gizmos.color = IsDecoration
                ? new Color(0.85f, 0.70f, 0.20f, 0.70f)   // gold = decoration
                : new Color(0.75f, 0.20f, 0.20f, 0.70f);  // red = obstacle
            foreach (var t in _previewTiles)
                Gizmos.DrawCube(transform.position + new Vector3(t.x, 0f, t.y), new Vector3(0.9f, 0.05f, 0.9f));
        }

        private void SetContent(CMapData mapData, int x, int y)
        {
            STile tile = mapData.Get(x, y);
            if (!tile.Type.IsBuildable()) return;

            if (IsDecoration)
                tile.DecorationType = _decorationType;
            else
            {
                tile.ObstaclePrefab = _obstaclePrefab;
                tile.ObstacleType   = _obstacleType;
            }

            mapData.Set(x, y, tile);
        }

        /// <summary>
        /// Bakes this content stamp into mapData centered at gridCenter.
        /// Returns the number of tiles affected.
        /// </summary>
        public int Bake(CMapData mapData, Vector2Int gridCenter, int globalSeed)
        {
            var rng = new System.Random(globalSeed ^ _localSeed);
            int targetSize = rng.Next(_sizeRange.Min, _sizeRange.Max + 1);
            return Grow(mapData, gridCenter, targetSize, globalSeed ^ _localSeed);
        }

        private int Grow(CMapData mapData, Vector2Int seed, int targetSize, int noiseSeed)
        {
            var noise = new FastNoiseLite(noiseSeed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(_noiseFrequency);

            var filled = new HashSet<Vector2Int>();
            var frontier = new List<Vector2Int> { seed };
            var rng = new System.Random(noiseSeed);

            filled.Add(seed);
            SetContent(mapData, seed.x, seed.y);

            Vector2Int[] offsets = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };

            while (filled.Count < targetSize && frontier.Count > 0)
            {
                int idx = rng.Next(frontier.Count);
                Vector2Int current = frontier[idx];
                frontier.RemoveAt(idx);

                foreach (var offset in offsets)
                {
                    Vector2Int neighbor = current + offset;

                    if (filled.Contains(neighbor)) continue;
                    if (!mapData.IsValid(neighbor.x, neighbor.y)) continue;
                    if (!mapData.Get(neighbor.x, neighbor.y).Type.IsBuildable()) continue;

                    float raw = noise.GetNoise(neighbor.x, neighbor.y);
                    float normalized = (raw + 1f) / 2f;
                    float threshold = _irregularity * 0.6f;

                    if (normalized < threshold) continue;

                    filled.Add(neighbor);
                    SetContent(mapData, neighbor.x, neighbor.y);
                    frontier.Add(neighbor);

                    if (filled.Count >= targetSize) break;
                }
            }

            return filled.Count;
        }
    }
}
