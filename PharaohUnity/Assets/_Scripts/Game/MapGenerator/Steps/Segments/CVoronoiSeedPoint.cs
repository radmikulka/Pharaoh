using UnityEngine;

namespace Pharaoh.MapGenerator
{
    public class CVoronoiSeedPoint : MonoBehaviour
    {
        [Tooltip("Přetrvá přes regeneraci. Non-persistent se smažou a vygenerují znovu.")]
        public bool IsPersistent = false;

        [HideInInspector] public int RegionId;
        [HideInInspector] public int TotalRegions;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (TotalRegions <= 0) return;
            float hue = RegionId / (float)TotalRegions;
            Gizmos.color = Color.HSVToRGB(hue, 0.7f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.6f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.2f, RegionId.ToString());
        }
#endif
    }
}
