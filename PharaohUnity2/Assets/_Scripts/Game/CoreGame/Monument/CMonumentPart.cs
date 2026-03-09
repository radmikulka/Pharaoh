using UnityEngine;

namespace Pharaoh
{
    public class CMonumentPart : MonoBehaviour
    {
        [HideInInspector] [SerializeField] private Mesh _generatedMesh;

        public Mesh GeneratedMesh
        {
            get => _generatedMesh;
            set => _generatedMesh = value;
        }
    }
}
