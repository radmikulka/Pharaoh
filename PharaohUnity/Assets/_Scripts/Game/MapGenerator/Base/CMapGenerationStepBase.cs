using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using AldaEngine;
using UnityEditor;
#endif

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Base class for all map generation pipeline steps.
    /// Provides a shared "Generate Up To This Step" Inspector button.
    /// </summary>
    public abstract class CMapGenerationStepBase : MonoBehaviour, IMapGenerationStep
    {
        public abstract string StepName { get; }

        /// <summary>Czech description shown at the top of the Inspector.</summary>
        public virtual string StepDescription => string.Empty;

        public abstract void Execute(CMapData mapData, int seed);

        [Button("Generate Up To This Step")]
        protected void GenerateUpToThisStep()
        {
#if UNITY_EDITOR
            var generator = GetComponentInParent<CMapGenerator>();
            if (generator == null)
            {
                Debug.LogError($"[{StepName}] CMapGenerator not found in parent.", this);
                return;
            }

            generator.GenerateUpTo(this);
#endif
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Base editor for all CMapGenerationStepBase subclasses.
    /// Displays StepDescription at the top of every step inspector.
    /// Steps with a custom editor should inherit from this class instead of CBaseEditor&lt;T&gt;.
    /// Automatically renders a foldout "— Noise Config —" for steps that implement IHaveNoise.
    /// </summary>
    [CustomEditor(typeof(CMapGenerationStepBase), editorForChildClasses: true)]
    public class CMapStepEditor : CBaseEditor<CMapGenerationStepBase>
    {
        private Editor _noiseEditor;
        private bool _noiseFoldout;
        private bool _noiseFoldoutLoaded;

        public override void OnInspectorGUI()
        {
            string desc = ((CMapGenerationStepBase)target).StepDescription;
            if (!string.IsNullOrEmpty(desc))
            {
                EditorGUILayout.HelpBox(desc, MessageType.None);
                EditorGUILayout.Space(4);
            }

            base.OnInspectorGUI();

            if (target is IHaveNoise hasNoise)
                DrawNoiseFoldout(hasNoise.NoiseConfig);
        }

        private void DrawNoiseFoldout(CNoiseConfig noiseConfig)
        {
            if (noiseConfig == null) { _noiseEditor = null; return; }

            if (!_noiseFoldoutLoaded)
            {
                _noiseFoldout = EditorPrefs.GetBool($"CMapStepEditor.NoiseFoldout.{target.GetType().Name}", true);
                _noiseFoldoutLoaded = true;
            }

            EditorGUILayout.Space(8);
            bool newFoldout = EditorGUILayout.Foldout(_noiseFoldout, "— Noise Config —", true, EditorStyles.foldoutHeader);
            if (newFoldout != _noiseFoldout)
            {
                _noiseFoldout = newFoldout;
                EditorPrefs.SetBool($"CMapStepEditor.NoiseFoldout.{target.GetType().Name}", _noiseFoldout);
            }

            if (_noiseFoldout)
            {
                Editor.CreateCachedEditor(noiseConfig, null, ref _noiseEditor);
                _noiseEditor.OnInspectorGUI();
            }
        }

        private void OnDisable()
        {
            if (_noiseEditor != null) DestroyImmediate(_noiseEditor);
        }
    }
#endif
}
