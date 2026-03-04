using UnityEditor;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    [ExecuteAlways]
    public class CVoronoiSeedPoint : MonoBehaviour
    {
        [Tooltip("Přetrvá přes regeneraci. Non-persistent se smažou a vygenerují znovu.")]
        public bool IsPersistent = false;

        [HideInInspector] public int RegionId;
        [HideInInspector] public int TotalRegions;

#if UNITY_EDITOR
        private static bool _isGenerating;

        private void Update()
        {
            if (Application.isPlaying) return;
            if (_isGenerating) return;
            if (!transform.hasChanged) return;
            transform.hasChanged = false;

            var step      = GetComponentInParent<CVoronoiRegionStep>();
            var generator = GetComponentInParent<CMapGenerator>();
            if (step == null || generator == null) return;

            _isGenerating = true;
            try
            {
                if (generator.MapData != null)
                {
                    // Fast path: only recompute Voronoi assignment, no GameObject destroy/create.
                    step.ReAssignVoronoi(generator.MapData);
                    UnityEditor.SceneView.RepaintAll();
                }
                else
                {
                    // Fallback: MapData not yet available — run the full pipeline up to this step.
                    generator.GenerateUpTo(step);
                }

                // Reset hasChanged on all seed points (fast path keeps positions,
                // fallback may have created new GameObjects).
                foreach (var sp in step.GetComponentsInChildren<CVoronoiSeedPoint>())
                    sp.transform.hasChanged = false;
            }
            finally
            {
                _isGenerating = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (TotalRegions <= 0) return;
            
            if(Selection.activeGameObject != gameObject && Selection.activeGameObject != transform.parent?.gameObject)
                return;
            
            float hue = RegionId / (float)TotalRegions;
            Gizmos.color = Color.HSVToRGB(hue, 0.7f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.6f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.2f, RegionId.ToString());
        }

        private void OnDrawGizmosSelected()
        {
            GetComponentInParent<CVoronoiRegionStep>()?.DrawRegionGizmos();
        }
#endif
    }
}
