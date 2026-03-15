using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pharaoh
{
    [RequireComponent(typeof(BoxCollider))]
    public class CMonumentPart : MonoBehaviour
    {
        public enum EPartMode
        {
            Default,
            CutoutOnly
        }
        
        [HideInInspector] [SerializeField] private Mesh _generatedMesh;
        [HideInInspector] [SerializeField] private List<GameObject> _cells;
        [SerializeField] private EPartMode _mode;
        [SerializeField] private Vector3 _cellSize;
        [SerializeField] private CWorkerPath _pathToMonument;
        [SerializeField] private CWorkerPath _pathFromMonument;
        [SerializeField] private GameObject[] _visuals;
        [SerializeField] private float _cameraZoom;

        public Mesh GeneratedMesh
        {
            get => _generatedMesh;
            set => _generatedMesh = value;
        }

        public List<GameObject> Cells
        {
            get => _cells;
            set => _cells = value;
        }

        public EPartMode Mode => _mode;
        public Vector3 CellSize => _cellSize;
        public CWorkerPath PathToMonument => _pathToMonument;
        public CWorkerPath PathFromMonument => _pathFromMonument;
        public float CameraZoom => _cameraZoom;

        public void SetVisualsActive(bool active)
        {
            for (int i = 0; i < _visuals.Length; i++)
            {
                _visuals[i].SetActive(active);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (Selection.activeGameObject != gameObject)
                return;

            BoxCollider box = GetComponent<BoxCollider>();

            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);

            Vector3 size = box.size;
            size.x *= transform.localScale.x;
            size.y *= transform.localScale.y;
            size.z *= transform.localScale.z;
            Gizmos.DrawCube(box.center + transform.position, size);

            DrawGridGizmo(box);
        }

        private void DrawGridGizmo(BoxCollider box)
        {
            if (_cellSize == Vector3.zero)
                return;

            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

            Vector3 localMin = box.center - box.size * 0.5f;
            Vector3 localMax = box.center + box.size * 0.5f;
            Transform t = transform;

            for (int axis = 0; axis < 3; axis++)
            {
                if (_cellSize[axis] <= 0f)
                    continue;

                int count = Mathf.CeilToInt((localMax[axis] - localMin[axis]) / _cellSize[axis]);

                for (int i = 1; i < count; i++)
                {
                    float offset = localMin[axis] + i * _cellSize[axis];
                    if (offset >= localMax[axis])
                        break;

                    int axisA = (axis + 1) % 3;
                    int axisB = (axis + 2) % 3;

                    Vector3 c00 = Vector3.zero, c01 = Vector3.zero;
                    Vector3 c10 = Vector3.zero, c11 = Vector3.zero;

                    c00[axis] = offset;
                    c01[axis] = offset;
                    c10[axis] = offset;
                    c11[axis] = offset;

                    c00[axisA] = localMin[axisA]; c00[axisB] = localMin[axisB];
                    c01[axisA] = localMax[axisA]; c01[axisB] = localMin[axisB];
                    c10[axisA] = localMin[axisA]; c10[axisB] = localMax[axisB];
                    c11[axisA] = localMax[axisA]; c11[axisB] = localMax[axisB];

                    Gizmos.DrawLine(t.TransformPoint(c00), t.TransformPoint(c01));
                    Gizmos.DrawLine(t.TransformPoint(c01), t.TransformPoint(c11));
                    Gizmos.DrawLine(t.TransformPoint(c11), t.TransformPoint(c10));
                    Gizmos.DrawLine(t.TransformPoint(c10), t.TransformPoint(c00));
                }
            }
        }
    }
}
