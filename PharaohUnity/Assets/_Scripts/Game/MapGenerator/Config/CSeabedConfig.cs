using UnityEngine;

namespace Pharaoh.MapGenerator
{
    [CreateAssetMenu(fileName = "cfg_seabed", menuName = "Pharaoh/MapGen/Seabed")]
    public class CSeabedConfig : ScriptableObject
    {
        [Tooltip("Maximum depth (Y offset below 0) of the seabed mesh.")]
        public float MaxDepth = 3f;

        [Tooltip("Maps normalized distance from shore (0 = coast, 1 = deep water) to depth fraction.")]
        public AnimationCurve DepthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Material applied to the generated seabed MeshRenderer.")]
        public Material SeabedMaterial;
    }
}
