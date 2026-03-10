using UnityEngine;

namespace CMonumentBuilder
{
    /// <summary>
    /// Marks a GameObject as a cut volume for CMonumentBuilder.
    /// Requires a Collider (BoxCollider, SphereCollider, or MeshCollider)
    /// to define the volume's shape.
    /// </summary>
    public class CCutVolume : MonoBehaviour
    {
        public Color capColor = Color.white;
    }
}
