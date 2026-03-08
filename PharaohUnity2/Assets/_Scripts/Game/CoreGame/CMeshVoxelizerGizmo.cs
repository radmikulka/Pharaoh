using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Scripts.Game.CoreGame
{
    [RequireComponent(typeof(MeshFilter))]
    public class CMeshVoxelizerGizmo : MonoBehaviour
    {
        [SerializeField] Vector3 _cellSize = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField] Color _voxelColor = new Color(0f, 1f, 0.4f, 1f);
        [SerializeField] bool _drawBoundary = true;
        [SerializeField] Color _boundaryColor = new Color(1f, 1f, 0f, 1f);
        [SerializeField] bool _drawBounds = true;
        [SerializeField] Color _boundsColor = new Color(1f, 0.6f, 0f, 1f);

#if UNITY_EDITOR
        internal bool[] _cachedVoxels;
        internal bool[] _cachedBoundary;
        internal bool[] _cachedExcluded;
        internal int    _nx, _ny, _nz;
        internal Bounds _cachedBounds;
        internal Vector3 _cachedCellSize;
        internal Mesh    _cachedMesh;

        CMonumentPart[] _cachedParts;
        Collider[]      _cachedPartColliders;
        Bounds[]        _cachedPartBounds;
        Vector3[]       _cachedPartCellSizes;
        Matrix4x4[]     _cachedPartMatrices;

        private void OnDrawGizmosSelected()
        {
            var mf   = GetComponent<MeshFilter>();
            var mesh = mf != null ? mf.sharedMesh : null;
            if (mesh == null || _cellSize.x <= 0f || _cellSize.y <= 0f || _cellSize.z <= 0f)
                return;

            if (mesh != _cachedMesh || _cellSize != _cachedCellSize || PartsChanged())
                Recalculate();

            if (_cachedVoxels == null)
                return;

            if (_drawBounds)
            {
                Gizmos.color = _boundsColor;
                var worldCenter = transform.TransformPoint(_cachedBounds.center);
                var worldSize   = Vector3.Scale(_cachedBounds.size, transform.lossyScale);
                Gizmos.DrawWireCube(worldCenter, worldSize);
            }

            var scaledCell = Vector3.Scale(_cellSize, transform.lossyScale);

            for (int z = 0; z < _nz; z++)
            for (int y = 0; y < _ny; y++)
            for (int x = 0; x < _nx; x++)
            {
                int idx      = x + y * _nx + z * _nx * _ny;
                bool inside   = _cachedVoxels[idx];
                bool boundary = _drawBoundary && _cachedBoundary != null && _cachedBoundary[idx];

                if (!inside && !boundary) continue;
                if (_cachedExcluded != null && _cachedExcluded[idx]) continue;

                Gizmos.color = inside ? _voxelColor : _boundaryColor;

                var localCenter = _cachedBounds.min + new Vector3(
                    (x + 0.5f) * _cellSize.x,
                    (y + 0.5f) * _cellSize.y,
                    (z + 0.5f) * _cellSize.z);
                Gizmos.DrawWireCube(transform.TransformPoint(localCenter), scaledCell);
            }

            if (_cachedParts != null)
            {
                foreach (var part in _cachedParts)
                {
                    if (part == null) continue;
                    part.EnsureRecalculated();
                    part.DrawGizmos();
                }
            }
        }

        private bool PartsChanged()
        {
            var currentParts = GetComponentsInChildren<CMonumentPart>();

            if (_cachedParts == null || currentParts.Length != _cachedParts.Length)
                return true;

            for (int i = 0; i < currentParts.Length; i++)
            {
                if (currentParts[i] != _cachedParts[i])
                    return true;

                var col = currentParts[i].GetComponent<Collider>();
                if (col != _cachedPartColliders[i])
                    return true;
                if (col != null && col.bounds != _cachedPartBounds[i])
                    return true;
                if (currentParts[i]._cellSize != _cachedPartCellSizes[i])
                    return true;
                if (currentParts[i].transform.localToWorldMatrix != _cachedPartMatrices[i])
                    return true;
            }

            return false;
        }

        internal void Recalculate()
        {
            var mf   = GetComponent<MeshFilter>();
            var mesh = mf != null ? mf.sharedMesh : null;
            if (mesh == null || _cellSize.x <= 0f || _cellSize.y <= 0f || _cellSize.z <= 0f)
            {
                _cachedVoxels = null;
                _cachedBoundary = null;
                _cachedExcluded = null;
                return;
            }

            _cachedMesh     = mesh;
            _cachedCellSize = _cellSize;
            _cachedBounds   = mesh.bounds;

            _nx = Mathf.Max(1, Mathf.CeilToInt(_cachedBounds.size.x / _cellSize.x));
            _ny = Mathf.Max(1, Mathf.CeilToInt(_cachedBounds.size.y / _cellSize.y));
            _nz = Mathf.Max(1, Mathf.CeilToInt(_cachedBounds.size.z / _cellSize.z));

            int total = _nx * _ny * _nz;
            if (total > 200_000)
            {
                Debug.LogWarning($"[CMeshVoxelizerGizmo] Grid too large ({_nx}×{_ny}×{_nz} = {total} cells). Increase Cell Size. Skipping recalculation.", this);
                _cachedVoxels = null;
                _cachedBoundary = null;
                _cachedExcluded = null;
                return;
            }

            var vertices  = mesh.vertices;
            var triangles = mesh.triangles;
            var dir       = Vector3.right;

            _cachedVoxels = new bool[total];

            for (int z = 0; z < _nz; z++)
            for (int y = 0; y < _ny; y++)
            for (int x = 0; x < _nx; x++)
            {
                var center = _cachedBounds.min + new Vector3(
                    (x + 0.5f) * _cellSize.x,
                    (y + 0.5f) * _cellSize.y,
                    (z + 0.5f) * _cellSize.z);
                _cachedVoxels[x + y * _nx + z * _nx * _ny] =
                    CVoxelizerUtils.IsInsideMesh(center, dir, vertices, triangles);
            }

            // boundary pass
            _cachedBoundary = new bool[total];
            for (int z = 0; z < _nz; z++)
            for (int y = 0; y < _ny; y++)
            for (int x = 0; x < _nx; x++)
            {
                int idx = x + y * _nx + z * _nx * _ny;
                if (_cachedVoxels[idx]) continue;

                if ((x > 0       && _cachedVoxels[(x - 1) + y * _nx + z * _nx * _ny]) ||
                    (x < _nx - 1 && _cachedVoxels[(x + 1) + y * _nx + z * _nx * _ny]) ||
                    (y > 0       && _cachedVoxels[x + (y - 1) * _nx + z * _nx * _ny]) ||
                    (y < _ny - 1 && _cachedVoxels[x + (y + 1) * _nx + z * _nx * _ny]) ||
                    (z > 0       && _cachedVoxels[x + y * _nx + (z - 1) * _nx * _ny]) ||
                    (z < _nz - 1 && _cachedVoxels[x + y * _nx + (z + 1) * _nx * _ny]))
                {
                    _cachedBoundary[idx] = true;
                }
            }

            // exclusion pass: cells whose world centre falls inside a child CMonumentPart
            var parts = GetComponentsInChildren<CMonumentPart>();
            _cachedExcluded = new bool[total];

            foreach (var part in parts)
            {
                part.EnsureRecalculated();

                for (int z = 0; z < _nz; z++)
                for (int y = 0; y < _ny; y++)
                for (int x = 0; x < _nx; x++)
                {
                    int idx = x + y * _nx + z * _nx * _ny;
                    if (_cachedExcluded[idx]) continue;

                    var localCenter = _cachedBounds.min + new Vector3(
                        (x + 0.5f) * _cellSize.x,
                        (y + 0.5f) * _cellSize.y,
                        (z + 0.5f) * _cellSize.z);
                    if (part.IsWorldPointInside(transform.TransformPoint(localCenter)))
                        _cachedExcluded[idx] = true;
                }
            }

            // cache parts state for change detection
            _cachedParts         = parts;
            _cachedPartColliders = new Collider[parts.Length];
            _cachedPartBounds    = new Bounds[parts.Length];
            _cachedPartCellSizes = new Vector3[parts.Length];
            _cachedPartMatrices  = new Matrix4x4[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                var col = parts[i].GetComponent<Collider>();
                _cachedPartColliders[i] = col;
                _cachedPartBounds[i]    = col != null ? col.bounds : default;
                _cachedPartCellSizes[i] = parts[i]._cellSize;
                _cachedPartMatrices[i]  = parts[i].transform.localToWorldMatrix;
            }
        }

        [CustomEditor(typeof(CMeshVoxelizerGizmo))]
        public class CMeshVoxelizerGizmoEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                EditorGUILayout.Space();

                var gizmo = (CMeshVoxelizerGizmo)target;

                int nx = gizmo._nx, ny = gizmo._ny, nz = gizmo._nz;
                int total = nx * ny * nz;

                int insideCount = 0, boundaryCount = 0, excludedCount = 0;
                if (gizmo._cachedVoxels != null)
                    foreach (var v in gizmo._cachedVoxels)
                        if (v) insideCount++;
                if (gizmo._cachedBoundary != null)
                    foreach (var v in gizmo._cachedBoundary)
                        if (v) boundaryCount++;
                if (gizmo._cachedExcluded != null)
                    foreach (var v in gizmo._cachedExcluded)
                        if (v) excludedCount++;

                string statsText = gizmo._cachedVoxels != null
                    ? $"Grid: {nx} × {ny} × {nz} = {total} cells\nInside: {insideCount}   Boundary: {boundaryCount}   Excluded: {excludedCount}"
                    : "No voxel data. Press Recalculate.";

                EditorGUILayout.HelpBox(statsText, MessageType.Info);

                if (total > 200_000)
                    EditorGUILayout.HelpBox($"Grid has {total} cells — exceeds 200k limit. Increase Cell Size.", MessageType.Warning);

                if (GUILayout.Button("Recalculate"))
                {
                    gizmo.Recalculate();
                    SceneView.RepaintAll();
                }
            }
        }
#endif
    }
}
