using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Scripts.Game.CoreGame
{
#if UNITY_EDITOR
    internal static class CVoxelizerUtils
    {
        internal static bool IsInsideMesh(Vector3 orig, Vector3 dir, Vector3[] vertices, int[] triangles)
        {
            int count    = 0;
            int triCount = triangles.Length;

            for (int i = 0; i < triCount; i += 3)
            {
                var v0 = vertices[triangles[i]];
                var v1 = vertices[triangles[i + 1]];
                var v2 = vertices[triangles[i + 2]];

                var   e1 = v1 - v0;
                var   e2 = v2 - v0;
                var   h  = Vector3.Cross(dir, e2);
                float a  = Vector3.Dot(e1, h);

                if (a > -1e-6f && a < 1e-6f) continue;

                float f = 1f / a;
                var   s = orig - v0;
                float u = f * Vector3.Dot(s, h);
                if (u < 0f || u > 1f) continue;

                var   q = Vector3.Cross(s, e1);
                float v = f * Vector3.Dot(dir, q);
                if (v < 0f || u + v > 1f) continue;

                float t = f * Vector3.Dot(e2, q);
                if (t > 1e-6f) count++;
            }

            return (count % 2) == 1;
        }
    }
#endif

    [RequireComponent(typeof(Collider))]
    public class CMonumentPart : MonoBehaviour
    {
        [SerializeField] internal Vector3 _cellSize    = new Vector3(0.25f, 0.25f, 0.25f);
        [SerializeField]          Color   _voxelColor  = new Color(0f, 0.6f, 1f, 1f);
        [SerializeField]          bool    _drawBoundary  = true;
        [SerializeField]          Color   _boundaryColor = new Color(0.5f, 0f, 1f, 1f);

#if UNITY_EDITOR
        internal bool[]    _cachedVoxels;
        internal bool[]    _cachedBoundary;
        internal bool[]    _cachedExcluded;
        internal int       _nx, _ny, _nz;
        internal Bounds    _cachedWorldBounds;
        internal Vector3   _cachedCellSize;
        internal Collider  _cachedCollider;
        internal Matrix4x4 _cachedMatrix;

        // direct children cache (for exclusion + change detection)
        CMonumentPart[] _cachedChildren;
        Collider[]      _cachedChildColliders;
        Bounds[]        _cachedChildBounds;
        Vector3[]       _cachedChildCellSizes;
        Matrix4x4[]     _cachedChildMatrices;

        // builder asset change detection
        GameObject _cachedBuilderAsset;
        Matrix4x4  _cachedBuilderAssetMatrix;

        // ── helpers ────────────────────────────────────────────────────────────

        CMonumentBuilder FindBuilder()
        {
            var t = transform.parent;
            while (t != null)
            {
                var b = t.GetComponent<CMonumentBuilder>();
                if (b != null) return b;
                t = t.parent;
            }
            return null;
        }

        internal CMonumentPart[] GetDirectChildParts()
        {
            var list = new List<CMonumentPart>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var p = transform.GetChild(i).GetComponent<CMonumentPart>();
                if (p != null) list.Add(p);
            }
            return list.ToArray();
        }

        internal void EnsureColliderCached()
        {
            if (_cachedCollider == null)
                _cachedCollider = GetComponent<Collider>();
        }

        bool ChildrenChanged()
        {
            var current = GetDirectChildParts();
            if (_cachedChildren == null || current.Length != _cachedChildren.Length)
                return true;

            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != _cachedChildren[i]) return true;

                var col = current[i].GetComponent<Collider>();
                if (col != _cachedChildColliders[i]) return true;
                if (col != null && col.bounds != _cachedChildBounds[i]) return true;
                if (current[i]._cellSize != _cachedChildCellSizes[i]) return true;
                if (current[i].transform.localToWorldMatrix != _cachedChildMatrices[i]) return true;
            }
            return false;
        }

        bool BuilderAssetChanged()
        {
            var builder = FindBuilder();
            if (builder == null) return _cachedBuilderAsset != null;
            return builder._asset != _cachedBuilderAsset ||
                   (builder._asset != null &&
                    builder._asset.transform.localToWorldMatrix != _cachedBuilderAssetMatrix);
        }

        // ── public API ─────────────────────────────────────────────────────────

        internal void EnsureRecalculated()
        {
            var col = GetComponent<Collider>();
            if (col == null) return;

            if (col != _cachedCollider ||
                _cellSize != _cachedCellSize ||
                transform.localToWorldMatrix != _cachedMatrix ||
                col.bounds != _cachedWorldBounds ||
                ChildrenChanged() ||
                BuilderAssetChanged())
                Recalculate();
        }

        internal void Recalculate()
        {
            var col = GetComponent<Collider>();
            if (col == null || _cellSize.x <= 0f || _cellSize.y <= 0f || _cellSize.z <= 0f)
            {
                _cachedVoxels = null; _cachedBoundary = null; _cachedExcluded = null;
                return;
            }

            _cachedCollider    = col;
            _cachedCellSize    = _cellSize;
            _cachedMatrix      = transform.localToWorldMatrix;
            _cachedWorldBounds = col.bounds;

            _nx = Mathf.Max(1, Mathf.CeilToInt(_cachedWorldBounds.size.x / _cellSize.x));
            _ny = Mathf.Max(1, Mathf.CeilToInt(_cachedWorldBounds.size.y / _cellSize.y));
            _nz = Mathf.Max(1, Mathf.CeilToInt(_cachedWorldBounds.size.z / _cellSize.z));

            int total = _nx * _ny * _nz;
            if (total > 200_000)
            {
                Debug.LogWarning(
                    $"[CMonumentPart] Grid too large ({_nx}×{_ny}×{_nz} = {total} cells). Increase Cell Size.", this);
                _cachedVoxels = null; _cachedBoundary = null; _cachedExcluded = null;
                return;
            }

            // 1. inside pass (clipped to builder asset if present)
            var builder = FindBuilder();
            builder?.EnsureAssetCached();

            _cachedVoxels = new bool[total];
            for (int z = 0; z < _nz; z++)
            for (int y = 0; y < _ny; y++)
            for (int x = 0; x < _nx; x++)
            {
                var worldCenter = WorldCellCenter(x, y, z);
                _cachedVoxels[x + y * _nx + z * _nx * _ny] =
                    IsWorldPointInside(worldCenter) &&
                    (builder == null || builder.IsInsideAsset(worldCenter));
            }

            // cache builder asset state
            _cachedBuilderAsset       = builder?._asset;
            _cachedBuilderAssetMatrix = builder?._asset != null
                ? builder._asset.transform.localToWorldMatrix
                : default;

            // 2. boundary pass
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

            // 3. exclusion pass — mask cells covered by direct child parts
            var children    = GetDirectChildParts();
            _cachedExcluded = new bool[total];

            foreach (var child in children)
            {
                child.EnsureRecalculated();

                for (int z = 0; z < _nz; z++)
                for (int y = 0; y < _ny; y++)
                for (int x = 0; x < _nx; x++)
                {
                    int idx = x + y * _nx + z * _nx * _ny;
                    if (!_cachedExcluded[idx] && child.IsWorldPointInside(WorldCellCenter(x, y, z)))
                        _cachedExcluded[idx] = true;
                }
            }

            // 4. cache children state for next change-detection pass
            _cachedChildren       = children;
            _cachedChildColliders = new Collider[children.Length];
            _cachedChildBounds    = new Bounds[children.Length];
            _cachedChildCellSizes = new Vector3[children.Length];
            _cachedChildMatrices  = new Matrix4x4[children.Length];

            for (int i = 0; i < children.Length; i++)
            {
                var cc = children[i].GetComponent<Collider>();
                _cachedChildColliders[i] = cc;
                _cachedChildBounds[i]    = cc != null ? cc.bounds : default;
                _cachedChildCellSizes[i] = children[i]._cellSize;
                _cachedChildMatrices[i]  = children[i].transform.localToWorldMatrix;
            }
        }

        internal bool IsWorldPointInside(Vector3 worldPoint)
        {
            var col = _cachedCollider != null ? _cachedCollider : GetComponent<Collider>();
            if (col == null) return false;

            if (col is BoxCollider box)
            {
                var local = box.transform.InverseTransformPoint(worldPoint) - box.center;
                var half  = box.size * 0.5f;
                return Mathf.Abs(local.x) <= half.x &&
                       Mathf.Abs(local.y) <= half.y &&
                       Mathf.Abs(local.z) <= half.z;
            }

            if (col is SphereCollider sphere)
            {
                var   worldCenter = sphere.transform.TransformPoint(sphere.center);
                var   scale       = sphere.transform.lossyScale;
                float worldRadius = sphere.radius * Mathf.Max(
                    Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                return (worldPoint - worldCenter).sqrMagnitude <= worldRadius * worldRadius;
            }

            // CapsuleCollider / MeshCollider (convex): ClosestPoint returns the
            // query point itself when it is inside the collider.
            return (col.ClosestPoint(worldPoint) - worldPoint).sqrMagnitude < 1e-6f;
        }

        /// <summary>Draw only this part's own cells (children already excluded).</summary>
        internal void DrawGizmos()
        {
            if (_cachedVoxels == null) return;

            for (int z = 0; z < _nz; z++)
            for (int y = 0; y < _ny; y++)
            for (int x = 0; x < _nx; x++)
            {
                int  idx      = x + y * _nx + z * _nx * _ny;
                bool inside   = _cachedVoxels[idx];
                bool boundary = _drawBoundary && _cachedBoundary != null && _cachedBoundary[idx];

                if (!inside && !boundary) continue;
                if (_cachedExcluded != null && _cachedExcluded[idx]) continue;

                Gizmos.color = inside ? _voxelColor : _boundaryColor;
                Gizmos.DrawWireCube(WorldCellCenter(x, y, z), _cellSize);
            }
        }

        /// <summary>Draw this part and all descendants recursively.</summary>
        internal void DrawTree()
        {
            EnsureRecalculated();
            DrawGizmos();

            if (_cachedChildren == null) return;
            foreach (var child in _cachedChildren)
                if (child != null) child.DrawTree();
        }

        // ── Unity callbacks ────────────────────────────────────────────────────

        private void OnDrawGizmosSelected() => DrawTree();

        // ── private helpers ────────────────────────────────────────────────────

        Vector3 WorldCellCenter(int x, int y, int z) =>
            _cachedWorldBounds.min + new Vector3(
                (x + 0.5f) * _cellSize.x,
                (y + 0.5f) * _cellSize.y,
                (z + 0.5f) * _cellSize.z);

        // ── nested editor ──────────────────────────────────────────────────────

        [CustomEditor(typeof(CMonumentPart))]
        public class CMonumentPartEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                EditorGUILayout.Space();

                var part  = (CMonumentPart)target;
                int total = part._nx * part._ny * part._nz;

                int insideCount = 0, boundaryCount = 0, excludedCount = 0;
                if (part._cachedVoxels   != null) foreach (var v in part._cachedVoxels)   if (v) insideCount++;
                if (part._cachedBoundary != null) foreach (var v in part._cachedBoundary) if (v) boundaryCount++;
                if (part._cachedExcluded != null) foreach (var v in part._cachedExcluded) if (v) excludedCount++;

                string statsText = part._cachedVoxels != null
                    ? $"Grid: {part._nx} × {part._ny} × {part._nz} = {total} cells\n" +
                      $"Inside: {insideCount}   Boundary: {boundaryCount}   Excluded: {excludedCount}"
                    : "No voxel data — select object in Scene or press Recalculate.";

                EditorGUILayout.HelpBox(statsText, MessageType.Info);

                if (total > 200_000)
                    EditorGUILayout.HelpBox(
                        $"Grid has {total} cells — exceeds 200k limit. Increase Cell Size.",
                        MessageType.Warning);

                if (GUILayout.Button("Recalculate"))
                {
                    part.Recalculate();
                    SceneView.RepaintAll();
                }
            }
        }
#endif
    }
}
