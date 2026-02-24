using System.Collections.Generic;
using AldaEngine;
using NaughtyAttributes;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Manually positioned river stamp. Place this as a child of CRiverGenerationStep,
    /// orient transform.forward in the flow direction, and it will carve an edge-to-edge river
    /// during generation with noise-controlled width variation.
    ///
    /// Flow direction is the actual XZ projection of transform.forward (any angle supported).
    /// Cross-direction is 90° CCW perpendicular to flow in the XZ plane.
    /// </summary>
    public class CRiverStamp : MonoBehaviour
    {
        [Header("Path Shape")]
        [SerializeField] [Range(0f, 1f)] private float _windiness = 0.5f;
        [SerializeField] [Min(0.001f)] private float _pathNoiseFrequency = 0.05f;

        [Header("Width")]
        [SerializeField] [MinMaxRangeInt(1, 15)] private SIntMinMaxRange _widthRange = new(1, 3);
        [SerializeField] [Min(0.001f)] private float _widthNoiseFrequency = 0.05f;

        [Header("Local Seed")]
        [SerializeField] private int _localSeed;

        // Preview cache: t.x = offset along flow axis, t.y = cross-axis offset
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

            const int previewLength = 60;
            const int halfLength    = previewLength / 2;

            var pathNoise = new FastNoiseLite(_localSeed);
            pathNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            pathNoise.SetFrequency(_pathNoiseFrequency);

            var widthNoise = new FastNoiseLite(_localSeed + 1);
            widthNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            widthNoise.SetFrequency(_widthNoiseFrequency);

            float crossPos   = 0f;
            int prevCrossInt = 0;

            for (int step = 0; step < previewLength; step++)
            {
                float raw  = pathNoise.GetNoise(step, 0f);
                float drift = raw * _windiness * 2f;
                crossPos  += drift;
                crossPos   = Mathf.Clamp(crossPos, -30f, 30f);

                int crossInt = Mathf.RoundToInt(crossPos);

                float widthT     = (widthNoise.GetNoise(step, 0f) + 1f) / 2f;
                int currentWidth = Mathf.RoundToInt(Mathf.Lerp(_widthRange.Min, _widthRange.Max, widthT));
                int halfWidth    = currentWidth / 2;

                int crossMin = Mathf.Min(prevCrossInt, crossInt);
                int crossMax = Mathf.Max(prevCrossInt, crossInt);

                for (int c = crossMin; c <= crossMax; c++)
                {
                    _previewTiles.Add(new Vector2Int(step - halfLength, c));
                    for (int w = 1; w <= halfWidth; w++)
                    {
                        _previewTiles.Add(new Vector2Int(step - halfLength, c - w));
                        _previewTiles.Add(new Vector2Int(step - halfLength, c + w));
                    }
                }

                prevCrossInt = crossInt;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_previewTiles == null || _previewTiles.Count == 0)
                RefreshPreview();

            Vector2 flowDir2D  = new Vector2(transform.forward.x, transform.forward.z).normalized;
            Vector2 crossDir2D = new Vector2(-flowDir2D.y, flowDir2D.x);
            Vector3 flowDir3D  = new Vector3(flowDir2D.x, 0, flowDir2D.y);
            Vector3 crossDir3D = new Vector3(crossDir2D.x, 0, crossDir2D.y);

            Gizmos.color = new Color(0.15f, 0.35f, 0.75f, 0.65f);
            foreach (var t in _previewTiles)
                Gizmos.DrawCube(
                    transform.position + flowDir3D * t.x + crossDir3D * t.y,
                    new Vector3(0.9f, 0.05f, 0.9f));

            // Arrow showing flow direction
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * 5f);
        }

        /// <summary>
        /// Bakes this river stamp into mapData.
        /// Flow direction is the XZ projection of transform.forward (any angle).
        /// stampGrid determines the river's starting cross-position via dot product with crossDir.
        /// Returns the number of water tiles carved.
        /// </summary>
        public int Bake(CMapData mapData, Vector2Int stampGrid)
        {
            Vector3 rawFwd  = transform.forward;
            Vector2 flowDir  = new Vector2(rawFwd.x, rawFwd.z).normalized;
            Vector2 crossDir = new Vector2(-flowDir.y, flowDir.x); // 90° CCW perp

            int noiseSeed = _localSeed;

            var pathNoise = new FastNoiseLite(noiseSeed);
            pathNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            pathNoise.SetFrequency(_pathNoiseFrequency);

            var widthNoise = new FastNoiseLite(noiseSeed + 1);
            widthNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            widthNoise.SetFrequency(_widthNoiseFrequency);

            // Project all four map corners onto flow and cross directions to get valid ranges
            Vector2[] corners =
            {
                new Vector2(0,             0),
                new Vector2(mapData.Width, 0),
                new Vector2(mapData.Width, mapData.Height),
                new Vector2(0,             mapData.Height)
            };

            float flowMin = float.MaxValue, flowMax = float.MinValue;
            float crossMin = float.MaxValue, crossMax = float.MinValue;
            foreach (var corner in corners)
            {
                float fd = Vector2.Dot(corner, flowDir);
                float cd = Vector2.Dot(corner, crossDir);
                if (fd < flowMin)  flowMin  = fd;
                if (fd > flowMax)  flowMax  = fd;
                if (cd < crossMin) crossMin = cd;
                if (cd > crossMax) crossMax = cd;
            }

            // Starting cross-position from stamp grid placement
            float crossPos = Vector2.Dot(new Vector2(stampGrid.x, stampGrid.y), crossDir);

            const float stepSize = 0.5f;
            int steps = Mathf.CeilToInt((flowMax - flowMin) / stepSize);
            var painted = new HashSet<Vector2Int>();
            int placed  = 0;

            for (int s = 0; s < steps; s++)
            {
                float t = flowMin + s * stepSize;

                crossPos += pathNoise.GetNoise(t, 0f) * _windiness * 2f * stepSize;
                crossPos  = Mathf.Clamp(crossPos, crossMin, crossMax);

                float widthT  = (widthNoise.GetNoise(t, 0f) + 1f) / 2f;
                int halfWidth = Mathf.RoundToInt(Mathf.Lerp(_widthRange.Min, _widthRange.Max, widthT)) / 2;

                Vector2 center = t * flowDir + crossPos * crossDir;

                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    Vector2 pos = center + w * crossDir;
                    var key = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
                    if (painted.Add(key) && mapData.IsValid(key.x, key.y))
                    {
                        if (CarveTile(mapData, key.x, key.y))
                            placed++;
                    }
                }
            }

            return placed;
        }

        private static bool CarveTile(CMapData mapData, int x, int y)
        {
            STile tile = mapData.Get(x, y);
            if (!tile.Type.IsBuildable()) return false;
            tile.Type = ETileType.Water;
            mapData.Set(x, y, tile);
            return true;
        }
    }
}
