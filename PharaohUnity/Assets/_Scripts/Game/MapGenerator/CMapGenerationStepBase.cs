using NaughtyAttributes;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Base class for all map generation pipeline steps.
    /// Provides a shared "Generate Up To This Step" Inspector button.
    /// </summary>
    public abstract class CMapGenerationStepBase : MonoBehaviour, IMapGenerationStep
    {
        public abstract string StepName { get; }

        public abstract void Execute(CMapData mapData, int seed);

        [Button("Generate Up To This Step")]
        private void GenerateUpToThisStep()
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
}
