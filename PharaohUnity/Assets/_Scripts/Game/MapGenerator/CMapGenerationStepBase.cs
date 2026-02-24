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
    /// </summary>
    [CustomEditor(typeof(CMapGenerationStepBase), editorForChildClasses: true)]
    public class CMapStepEditor : CBaseEditor<CMapGenerationStepBase>
    {
        public override void OnInspectorGUI()
        {
            string desc = ((CMapGenerationStepBase)target).StepDescription;
            if (!string.IsNullOrEmpty(desc))
            {
                EditorGUILayout.HelpBox(desc, MessageType.None);
                EditorGUILayout.Space(4);
            }

            base.OnInspectorGUI();
        }
    }
#endif
}
