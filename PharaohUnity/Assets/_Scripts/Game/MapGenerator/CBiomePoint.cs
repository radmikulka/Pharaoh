using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// A physical seed point for Voronoi region generation.
    /// Place as a child of a CVoronoiRegionStep GameObject.
    /// Move freely in the Scene view, then press "Recalculate Regions".
    /// </summary>
    public class CBiomePoint : MonoBehaviour
    {
        public EBiomeType BiomeType = EBiomeType.None;

        // Assigned automatically by CVoronoiRegionStep.SpawnPoints — do not edit manually.
        [HideInInspector] public int RegionId;

        private void OnDrawGizmos()
        {
            Color c = CMapGenerator.GetBiomeColor(BiomeType);

            Gizmos.color = c;
            Gizmos.DrawSphere(transform.position, 2f);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 2.1f);

#if UNITY_EDITOR
            UnityEditor.Handles.color = c;
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 3f,
                $"{BiomeType} (r{RegionId})"
            );
#endif
        }
    }
}