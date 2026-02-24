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
    /// horizontal flow (E-W): |forward.x| >= |forward.z|  →  cross-axis = Z (stampGrid.y)
    /// vertical   flow (N-S): |forward.z|  > |forward.x|  →  cross-axis = X (stampGrid.x)
    /// </summary>
    public class CRiverStamp : MonoBehaviour
    {
        [Header("Path Shape")]
        [SerializeField] [Range(0f, 1f)] private float _windiness = 0.5f;
        [SerializeField] [Min(0.001f)] private float _pathNoiseFrequency = 0.05f;

        [Header("Width")]
        [SerializeField] private SIntMinMaxRange _widthRange = new(1, 3);
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

        private void OnDrawGizmos()
        {
            if (_previewTiles == null || _previewTiles.Count == 0)
                RefreshPreview();

            bool horizontal = Mathf.Abs(transform.forward.x) >= Mathf.Abs(transform.forward.z);
            Vector3 mainAxis  = horizontal ? Vector3.right   : Vector3.forward;
            Vector3 crossAxis = horizontal ? Vector3.forward : Vector3.right;

            Gizmos.color = new Color(0.15f, 0.35f, 0.75f, 0.65f);
            foreach (var t in _previewTiles)
                Gizmos.DrawCube(
                    transform.position + mainAxis * t.x + crossAxis * t.y,
                    new Vector3(0.9f, 0.05f, 0.9f));

            // Arrow showing flow direction
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * 5f);
        }

        /// <summary>
        /// Bakes this river stamp into mapData.
        /// stampGrid.y drives the starting cross-position for horizontal (E-W) flow;
        /// stampGrid.x drives it for vertical (N-S) flow.
        /// Returns the number of water tiles carved.
        /// </summary>
        public int Bake(CMapData mapData, Vector2Int stampGrid, int globalSeed)
        {
            bool horizontal = Mathf.Abs(transform.forward.x) >= Mathf.Abs(transform.forward.z);
            int  length     = horizontal ? mapData.Width : mapData.Height;
            int  noiseSeed  = globalSeed ^ _localSeed;

            var pathNoise = new FastNoiseLite(noiseSeed);
            pathNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            pathNoise.SetFrequency(_pathNoiseFrequency);

            var widthNoise = new FastNoiseLite(noiseSeed + 1);
            widthNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            widthNoise.SetFrequency(_widthNoiseFrequency);

            // Starting cross-position comes from stamp grid placement
            float crossPos   = horizontal ? stampGrid.y : stampGrid.x;
            int prevCrossInt = Mathf.RoundToInt(crossPos);
            int placed       = 0;

            for (int step = 0; step < length; step++)
            {
                float raw  = pathNoise.GetNoise(step, 0f);
                float drift = raw * _windiness * 2f;
                crossPos  += drift;

                int breadth = horizontal ? mapData.Height : mapData.Width;
                crossPos = Mathf.Clamp(crossPos, 0, breadth - 1);

                int crossInt = Mathf.RoundToInt(crossPos);

                float widthT     = (widthNoise.GetNoise(step, 0f) + 1f) / 2f;
                int currentWidth = Mathf.RoundToInt(Mathf.Lerp(_widthRange.Min, _widthRange.Max, widthT));
                int halfWidth    = currentWidth / 2;

                int crossMin = Mathf.Min(prevCrossInt, crossInt);
                int crossMax = Mathf.Max(prevCrossInt, crossInt);

                for (int c = crossMin; c <= crossMax; c++)
                {
                    int cx = horizontal ? step : c;
                    int cy = horizontal ? c    : step;

                    if (mapData.IsValid(cx, cy) && CarveTile(mapData, cx, cy))
                        placed++;

                    for (int w = 1; w <= halfWidth; w++)
                    {
                        int wx1 = horizontal ? cx     : cx - w;
                        int wy1 = horizontal ? cy - w : cy;
                        int wx2 = horizontal ? cx     : cx + w;
                        int wy2 = horizontal ? cy + w : cy;

                        if (mapData.IsValid(wx1, wy1)) CarveTile(mapData, wx1, wy1);
                        if (mapData.IsValid(wx2, wy2)) CarveTile(mapData, wx2, wy2);
                    }
                }

                prevCrossInt = crossInt;
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
