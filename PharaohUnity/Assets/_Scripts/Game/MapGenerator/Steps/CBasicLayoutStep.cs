using UnityEngine;
#if UNITY_EDITOR
using AldaEngine;
using UnityEditor;
#endif

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Step 0 — generates the basic land/water layout using noise + an edge falloff.
    /// </summary>
    public class CBasicLayoutStep : MonoBehaviour, IMapGenerationStep
    {
        [SerializeField] private CNoiseConfig _noiseConfig;
        [SerializeField] private EBorderType _borderType = EBorderType.AllSides;
        [SerializeField] [Range(0f, 1f)] private float _waterThreshold = 0.4f;
        [SerializeField] [Range(0.5f, 6f)] private float _falloffStrength = 3f;

        public string StepName => "Basic Layout (Land / Water)";

        public void Execute(CMapData mapData, int seed)
        {
            if (_noiseConfig == null)
            {
                Debug.LogError("[CBasicLayoutStep] NoiseConfig is not assigned!", this);
                return;
            }

            var noise = _noiseConfig.CreateNoise(seed);

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    float nx = x;
                    float ny = y;

                    if (_noiseConfig.UseDomainWarp)
                        noise.DomainWarp(ref nx, ref ny);

                    float raw = noise.GetNoise(nx, ny);
                    float normalized = (raw + 1f) / 2f; // −1..1 → 0..1

                    float falloff = ComputeFalloff(x, y, mapData.Width, mapData.Height);
                    float value = Mathf.Clamp01(normalized - falloff);

                    ETileType type = value > _waterThreshold ? ETileType.Land : ETileType.Water;

                    mapData.Set(x, y, new STile { X = x, Y = y, Type = type, RegionId = -1 });
                }
            }
        }

        private float ComputeFalloff(int x, int y, int width, int height)
        {
            if (_borderType == EBorderType.None) return 0f;

            // Normalize tile position to −1..1 range
            float nx = (x / (float)(width  - 1)) * 2f - 1f;
            float ny = (y / (float)(height - 1)) * 2f - 1f;

            // Accumulate the maximum falloff contribution from each active direction.
            // Taking the max is mathematically equivalent to the original combined formulas:
            //   East | West   → max(nx⁺, (-nx)⁺) = |nx|   (same as old EastWest)
            //   All four      → max(|nx|, |ny|)           (same as old AllSides)
            float falloff = 0f;

            if ((_borderType & EBorderType.East)  != 0)
                falloff = Mathf.Max(falloff, Mathf.Pow(Mathf.Max(0f,  nx), _falloffStrength));
            if ((_borderType & EBorderType.West)  != 0)
                falloff = Mathf.Max(falloff, Mathf.Pow(Mathf.Max(0f, -nx), _falloffStrength));
            if ((_borderType & EBorderType.North) != 0)
                falloff = Mathf.Max(falloff, Mathf.Pow(Mathf.Max(0f,  ny), _falloffStrength));
            if ((_borderType & EBorderType.South) != 0)
                falloff = Mathf.Max(falloff, Mathf.Pow(Mathf.Max(0f, -ny), _falloffStrength));

            return falloff;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CBasicLayoutStep))]
    public class CBasicLayoutStepEditor : CBaseEditor<CBasicLayoutStep>
    {
        private Editor _noiseEditor;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var noiseConfig = serializedObject
                .FindProperty("_noiseConfig").objectReferenceValue as CNoiseConfig;

            if (noiseConfig == null) { _noiseEditor = null; return; }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("— Noise Config —", EditorStyles.boldLabel);

            Editor.CreateCachedEditor(noiseConfig, null, ref _noiseEditor);
            _noiseEditor.OnInspectorGUI();
        }

        private void OnDisable()
        {
            if (_noiseEditor != null) DestroyImmediate(_noiseEditor);
        }
    }
#endif
}
