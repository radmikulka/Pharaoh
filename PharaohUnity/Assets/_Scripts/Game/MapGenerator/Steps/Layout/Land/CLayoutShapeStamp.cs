using System.Collections.Generic;
using AldaEngine;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Manually positioned terrain shape stamp. Place this as a child of CLayoutShapeStep,
    /// position it in the Scene view, and it will bake the shape at that location during generation.
    /// </summary>
    public class CLayoutShapeStamp : MonoBehaviour
    {
        [Header("Size")]
        [SerializeField] private SIntMinMaxRange _sizeRange = new(3, 80);

        [Header("Shape")]
        [SerializeField] [Range(0f, 1f)] private float _irregularity = 0.5f;
        [SerializeField] [Min(0.001f)] private float _noiseFrequency = 0.1f;

        [Header("Local Seed")]
        [SerializeField] private int _localSeed;

        [Header("Mode")]
        [Tooltip("If true, fills Land tiles into Water instead of carving a lake.")]
        [SerializeField] private bool _isIsland = false;

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
            var rng = new System.Random(_localSeed);
            int targetSize = rng.Next(_sizeRange.Min, _sizeRange.Max + 1);

            // Match noise coordinates used in GrowLake: sample at absolute grid position
            var generator = GetComponentInParent<CMapGenerator>();
            Vector3 localPos = generator != null
                ? generator.transform.InverseTransformPoint(transform.position)
                : transform.localPosition;
            var gridCenter = new Vector2Int(Mathf.RoundToInt(localPos.x), Mathf.RoundToInt(localPos.z));

            GrowLakeVirtual(gridCenter, targetSize, _localSeed, _previewTiles);
        }

        // Grows shape from 'origin' using absolute noise coordinates (matching GrowLake).
        // Tiles stored as local offsets from origin so gizmo can draw relative to transform.position.
        // Note: Preview ignores tile-type and map-bounds filtering that Bake applies,
        // so the actual baked shape may be smaller near edges or other terrain features.
        private void GrowLakeVirtual(Vector2Int origin, int targetSize, int noiseSeed, List<Vector2Int> result)
        {
            var noise = new FastNoiseLite(noiseSeed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(_noiseFrequency);

            var filled = new HashSet<Vector2Int>();
            var frontier = new List<Vector2Int> { origin };
            var rng = new System.Random(noiseSeed);

            filled.Add(origin);
            result.Add(Vector2Int.zero);

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
                    result.Add(neighbor - origin);
                    frontier.Add(neighbor);

                    if (filled.Count >= targetSize) break;
                }
            }
        }

        /// <summary>Returns true if the tile type at the stamp center is valid for this mode.</summary>
        public bool IsCenterValid(ETileType type) =>
            _isIsland ? type == ETileType.Water : type.IsBuildable();

        private void OnDrawGizmosSelected()
        {
            if(Selection.activeGameObject != gameObject)
                return;
            
            if (_previewTiles == null || _previewTiles.Count == 0)
                RefreshPreview();

            Gizmos.color = _isIsland
                ? new Color(0.35f, 0.70f, 0.25f, 1f)   // zelená = ostrov
                : new Color(0.20f, 0.45f, 0.85f, 1f);  // modrá = jezero
            foreach (var t in _previewTiles)
                Gizmos.DrawCube(transform.position + new Vector3(t.x, 0f, t.y), new Vector3(0.9f, 0.05f, 0.9f));
        }

        /// <summary>
        /// Bakes this lake stamp into mapData centered at gridCenter.
        /// Returns the number of water tiles placed.
        /// </summary>
        public int Bake(CMapData mapData, Vector2Int gridCenter)
        {
            var rng = new System.Random(_localSeed);
            int targetSize = rng.Next(_sizeRange.Min, _sizeRange.Max + 1);
            return GrowLake(mapData, gridCenter, targetSize, _localSeed);
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
                    bool neighborOk = _isIsland
                        ? mapData.Get(neighbor.x, neighbor.y).Type == ETileType.Water
                        : mapData.Get(neighbor.x, neighbor.y).Type.IsBuildable();
                    if (!neighborOk) continue;

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

        private bool CarveTile(CMapData mapData, int x, int y)
        {
            STile tile = mapData.Get(x, y);
            if (_isIsland)
            {
                if (tile.Type != ETileType.Water) return false;
                tile.Type = ETileType.Land;
            }
            else
            {
                if (!tile.Type.IsBuildable()) return false;
                tile.Type = ETileType.Water;
            }
            mapData.Set(x, y, tile);
            return true;
        }
    }
}
