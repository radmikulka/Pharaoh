using System;
using UnityEngine;

namespace Pharaoh
{
    [RequireComponent(typeof(BoxCollider))]
    public class CMonumentPart : MonoBehaviour
    {
        [HideInInspector] [SerializeField] private Mesh _generatedMesh;

        public Mesh GeneratedMesh
        {
            get => _generatedMesh;
            set => _generatedMesh = value;
        }

        private void OnDrawGizmosSelected()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            
            Vector3 size = box.size;
            size.x *= transform.localScale.x;
            size.y *= transform.localScale.y;
            size.z *= transform.localScale.z;
            Gizmos.DrawCube(box.center + transform.position, size);
        }
    }
}
