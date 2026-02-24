using ModestTree;
using UnityEngine;
#if UNITY_EDITOR
using AldaEngine;
using UnityEditor;
#endif

namespace Pharaoh.MapGenerator
{
    public class CNoiseConfig : MonoBehaviour
    {
        [Header("Noise")]
        public FastNoiseLite.NoiseType NoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
        [Range(0.001f, 0.1f)] public float Frequency = 0.015f;

        [Header("Fractal")]
        public FastNoiseLite.FractalType FractalType = FastNoiseLite.FractalType.FBm;
        [Range(1, 4)] public int Octaves = 4;

        [Header("Domain Warp")]
        public bool UseDomainWarp = true;
        public FastNoiseLite.DomainWarpType DomainWarpType = FastNoiseLite.DomainWarpType.OpenSimplex2;
        [Range(1f, 100f)]public float DomainWarpAmplitude = 30f;

        [Header("Output")]
        [Range(0.01f, 3f)] public float Amplitude = 1f;

        public FastNoiseLite CreateNoise(int seed)
        {
            var noise = new FastNoiseLite(seed);
            noise.SetNoiseType(NoiseType);
            noise.SetFrequency(Frequency);
            noise.SetFractalType(FractalType);
            noise.SetFractalOctaves(Octaves);

            if (UseDomainWarp)
            {
                noise.SetDomainWarpType(DomainWarpType);
                noise.SetDomainWarpAmp(DomainWarpAmplitude);
            }

            return noise;
        }

        public float SampleNoise(FastNoiseLite noise, float nx, float ny)
            => noise.GetNoise(nx, ny) * Amplitude;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CNoiseConfig))]
    public class CNoiseConfigEditor : CBaseEditor<CNoiseConfig>
    {
        private Texture2D _preview;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
                _preview = null;

            DrawNoisePreview();
        }

        private void DrawNoisePreview()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Noise Preview (seed 42)", EditorStyles.boldLabel);

            if (_preview == null)
                _preview = GeneratePreview(MyTarget);

            var rect = GUILayoutUtility.GetRect(
                GUIContent.none, GUIStyle.none,
                GUILayout.Height(128), GUILayout.ExpandWidth(true));
            EditorGUI.DrawPreviewTexture(rect, _preview);
        }

        private static Texture2D GeneratePreview(CNoiseConfig config)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var noise = config.CreateNoise(42);

            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float nx = x, ny = y;
                if (config.UseDomainWarp) noise.DomainWarp(ref nx, ref ny);
                float v = Mathf.Clamp01((config.SampleNoise(noise, nx, ny) + 1f) / 2f);
                tex.SetPixel(x, y, new Color(v, v, v));
            }

            tex.Apply();
            return tex;
        }
    }
#endif
}
