using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Places organic-shaped inland lakes using noise-driven flood fill.
    /// Lakes are grown from seed points on land tiles, producing irregular shapes.
    /// </summary>
    public class CLakeGenerationStep : MonoBehaviour, IMapGenerationStep
    {
        [Header("Lakes")]
        [SerializeField] [Range(0, 3)] private int _lakeCount = 1;
        [SerializeField] [Min(1)] private int _minSize = 3;
        [SerializeField] [Min(1)] private int _maxSize = 8;

        [Header("Placement")]
        [SerializeField] [Min(2)] private int _minDistanceFromEdge = 3;
        [SerializeField] [Min(2)] private int _minDistanceBetweenLakes = 5;

        [Header("Shape")]
        [SerializeField] [Range(0f, 1f)] private float _irregularity = 0.5f;
        [SerializeField] [Min(0.001f)] private float _noiseFrequency = 0.1f;

        public string StepName => "Lakes";

        public void Execute(CMapData mapData, int seed)
        {
            if (_lakeCount <= 0) return;

            var rng = new System.Random(seed);
            var lakeCenters = new List<Vector2Int>();

            for (int i = 0; i < _lakeCount; i++)
            {
                int lakeSeed = rng.Next();
                Vector2Int? center = FindLakeSeed(mapData, rng, lakeCenters);
                if (!center.HasValue)
                {
                    Debug.LogWarning($"[{StepName}] Could not find valid seed for lake {i + 1}.");
                    continue;
                }

                int targetSize = rng.Next(_minSize, _maxSize + 1);
                int placed = GrowLake(mapData, center.Value, targetSize, lakeSeed);
                lakeCenters.Add(center.Value);

                Debug.Log($"[{StepName}] Lake at ({center.Value.x},{center.Value.y}): {placed} tiles.");
            }
        }

        private Vector2Int? FindLakeSeed(CMapData mapData, System.Random rng, List<Vector2Int> existing)
        {
            const int maxAttempts = 100;

            int minX = _minDistanceFromEdge;
            int minY = _minDistanceFromEdge;
            int maxX = mapData.Width - _minDistanceFromEdge - 1;
            int maxY = mapData.Height - _minDistanceFromEdge - 1;

            if (minX > maxX || minY > maxY) return null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int x = rng.Next(minX, maxX + 1);
                int y = rng.Next(minY, maxY + 1);

                if (!mapData.IsValid(x, y)) continue;
                if (mapData.Get(x, y).Type != ETileType.Land) continue;

                // Check distance from other lakes
                bool tooClose = false;
                foreach (var other in existing)
                {
                    int dx = x - other.x;
                    int dy = y - other.y;
                    if (dx * dx + dy * dy < _minDistanceBetweenLakes * _minDistanceBetweenLakes)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                return new Vector2Int(x, y);
            }

            return null;
        }

        private int GrowLake(CMapData mapData, Vector2Int seed, int targetSize, int noiseSeed)
        {
            var noise = new FastNoiseLite(noiseSeed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(_noiseFrequency);

            var filled = new HashSet<Vector2Int>();
            var frontier = new List<Vector2Int> { seed };
            var rng = new System.Random(noiseSeed);

            filled.Add(seed);
            CarveTile(mapData, seed.x, seed.y);

            // Cardinal neighbor offsets
            Vector2Int[] offsets = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };

            while (filled.Count < targetSize && frontier.Count > 0)
            {
                // Pick a random frontier tile to expand from
                int idx = rng.Next(frontier.Count);
                Vector2Int current = frontier[idx];
                frontier.RemoveAt(idx);

                foreach (var offset in offsets)
                {
                    Vector2Int neighbor = current + offset;

                    if (filled.Contains(neighbor)) continue;
                    if (!mapData.IsValid(neighbor.x, neighbor.y)) continue;
                    if (mapData.Get(neighbor.x, neighbor.y).Type != ETileType.Land) continue;

                    // Noise-based acceptance — higher irregularity = more rejection = jagged shapes
                    float raw = noise.GetNoise(neighbor.x, neighbor.y);
                    float normalized = (raw + 1f) / 2f;
                    float threshold = _irregularity * 0.6f;

                    if (normalized < threshold) continue;

                    filled.Add(neighbor);
                    CarveTile(mapData, neighbor.x, neighbor.y);
                    frontier.Add(neighbor);

                    if (filled.Count >= targetSize) break;
                }
            }

            return filled.Count;
        }

        private static bool CarveTile(CMapData mapData, int x, int y)
        {
            STile tile = mapData.Get(x, y);
            if (tile.Type != ETileType.Land) return false;
            tile.Type = ETileType.Water;
            mapData.Set(x, y, tile);
            return true;
        }
    }
}
