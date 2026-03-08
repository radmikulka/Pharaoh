using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Scripts.Game.CoreGame
{
    public class CMonumentBuilder : MonoBehaviour
    {
        [SerializeField] internal GameObject _asset;
        [SerializeField]         string      _generatedRootName = "GeneratedMeshes";

#if UNITY_EDITOR
        // ── asset mesh cache ───────────────────────────────────────────────────

        struct SMeshData
        {
            internal Vector3[]  Vertices;
            internal int[]      Triangles;
            internal Matrix4x4  WorldToLocal;
        }

        SMeshData[] _assetMeshData;
        GameObject  _cachedAssetRef;
        Matrix4x4   _cachedAssetMatrix;

        internal void EnsureAssetCached()
        {
            if (_asset == null)
            {
                _assetMeshData  = null;
                _cachedAssetRef = null;
                return;
            }

            bool assetChanged   = _asset != _cachedAssetRef;
            bool matrixChanged  = _asset.transform.localToWorldMatrix != _cachedAssetMatrix;
            if (!assetChanged && !matrixChanged) return;

            var mfs  = _asset.GetComponentsInChildren<MeshFilter>();
            var data = new List<SMeshData>(mfs.Length);
            foreach (var mf in mfs)
            {
                var mesh = mf.sharedMesh;
                if (mesh == null) continue;
                data.Add(new SMeshData
                {
                    Vertices     = mesh.vertices,
                    Triangles    = mesh.triangles,
                    WorldToLocal = mf.transform.worldToLocalMatrix,
                });
            }

            _assetMeshData  = data.ToArray();
            _cachedAssetRef = _asset;
            _cachedAssetMatrix = _asset.transform.localToWorldMatrix;
        }

        internal bool IsInsideAsset(Vector3 worldPoint)
        {
            if (_asset == null) return true;   // no asset → no clipping
            EnsureAssetCached();
            if (_assetMeshData == null || _assetMeshData.Length == 0) return false;

            foreach (var md in _assetMeshData)
            {
                var local = md.WorldToLocal.MultiplyPoint3x4(worldPoint);
                if (CVoxelizerUtils.IsInsideMesh(local, Vector3.right, md.Vertices, md.Triangles))
                    return true;
            }
            return false;
        }

        internal bool AssetChanged() =>
            _asset != _cachedAssetRef ||
            (_asset != null && _asset.transform.localToWorldMatrix != _cachedAssetMatrix);

        // ── gizmo / editor ─────────────────────────────────────────────────────

        CMonumentPart[] GetRootParts()
        {
            var list = new List<CMonumentPart>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var part = transform.GetChild(i).GetComponent<CMonumentPart>();
                if (part != null) list.Add(part);
            }
            return list.ToArray();
        }

        // ── mesh slicing ───────────────────────────────────────────────────────

        internal void GenerateMeshParts()
        {
            EnsureAssetCached();
            if (_asset == null)
            {
                Debug.LogWarning("[CMonumentBuilder] No asset assigned.", this);
                return;
            }

            var rootParts = GetRootParts();

            // Delete previous generated root
            var existing = transform.Find(_generatedRootName);
            if (existing != null)
                DestroyImmediate(existing.gameObject);

            var genRoot = new GameObject(_generatedRootName);
            genRoot.transform.SetParent(transform, false);

            // Assign world-space triangles to the deepest containing part
            var partTris = new Dictionary<CMonumentPart,
                Dictionary<Material, List<(Vector3 v0, Vector3 v1, Vector3 v2)>>>();

            foreach (var mf in _asset.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = mf.sharedMesh;
                if (mesh == null) continue;

                var rawVerts    = mesh.vertices;
                var ltw         = mf.transform.localToWorldMatrix;
                var worldVerts  = new Vector3[rawVerts.Length];
                for (int i = 0; i < rawVerts.Length; i++)
                    worldVerts[i] = ltw.MultiplyPoint3x4(rawVerts[i]);

                var mr = mf.GetComponent<MeshRenderer>();
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    // mat may be null (no renderer or missing slot) — null is a valid dict key
                    var mat = (mr != null && mr.sharedMaterials.Length > sub)
                        ? mr.sharedMaterials[sub] : null;

                    var tris = mesh.GetTriangles(sub);
                    for (int t = 0; t < tris.Length; t += 3)
                    {
                        var v0 = worldVerts[tris[t]];
                        var v1 = worldVerts[tris[t + 1]];
                        var v2 = worldVerts[tris[t + 2]];
                        var centroid = (v0 + v1 + v2) / 3f;

                        var part = FindDeepestPart(centroid, rootParts);
                        if (part == null) continue;

                        if (!partTris.TryGetValue(part, out var matDict))
                        {
                            matDict = new Dictionary<Material, List<(Vector3, Vector3, Vector3)>>();
                            partTris[part] = matDict;
                        }

                        if (!matDict.TryGetValue(mat!, out var triList))
                        {
                            triList = new List<(Vector3, Vector3, Vector3)>();
                            matDict[mat!] = triList;
                        }

                        triList.Add((v0, v1, v2));
                    }
                }
            }

            foreach (var root in rootParts)
                CreateGeneratedGO(root, genRoot.transform, partTris);

            SceneView.RepaintAll();
        }

        CMonumentPart FindDeepestPart(Vector3 worldPoint, IEnumerable<CMonumentPart> candidates)
        {
            foreach (var part in candidates)
            {
                if (!part.IsWorldPointInside(worldPoint)) continue;
                var deeper = FindDeepestPart(worldPoint, part.GetDirectChildParts());
                return deeper ?? part;
            }
            return null;
        }

        Mesh BuildMesh(Dictionary<Material, List<(Vector3 v0, Vector3 v1, Vector3 v2)>> matDict,
                       string meshName)
        {
            var vertices    = new List<Vector3>();
            var submeshTris = new List<List<int>>();

            foreach (var kvp in matDict)
            {
                var tris = new List<int>();
                foreach (var (v0, v1, v2) in kvp.Value)
                {
                    tris.Add(vertices.Count);
                    tris.Add(vertices.Count + 1);
                    tris.Add(vertices.Count + 2);
                    vertices.Add(transform.InverseTransformPoint(v0));
                    vertices.Add(transform.InverseTransformPoint(v1));
                    vertices.Add(transform.InverseTransformPoint(v2));
                }
                submeshTris.Add(tris);
            }

            var mesh = new Mesh { name = meshName };
            mesh.vertices     = vertices.ToArray();
            mesh.subMeshCount = submeshTris.Count;
            for (int i = 0; i < submeshTris.Count; i++)
                mesh.SetTriangles(submeshTris[i], i);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        void CreateGeneratedGO(
            CMonumentPart part,
            Transform parentTransform,
            Dictionary<CMonumentPart, Dictionary<Material, List<(Vector3, Vector3, Vector3)>>> partTris)
        {
            var go = new GameObject(part.name);
            go.transform.SetParent(parentTransform, false);

            if (partTris.TryGetValue(part, out var matDict) && matDict.Count > 0)
            {
                go.AddComponent<MeshFilter>().sharedMesh = BuildMesh(matDict, part.name);
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterials = new List<Material>(matDict.Keys).ToArray();
            }

            foreach (var child in part.GetDirectChildParts())
                CreateGeneratedGO(child, go.transform, partTris);
        }

        // ── gizmo / editor ─────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            EnsureAssetCached();
            foreach (var part in GetRootParts())
                part.DrawTree();
        }

        [CustomEditor(typeof(CMonumentBuilder))]
        public class CMonumentBuilderEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                EditorGUILayout.Space();

                var builder  = (CMonumentBuilder)target;
                var allParts = builder.GetComponentsInChildren<CMonumentPart>();

                EditorGUILayout.HelpBox(
                    $"CMonumentParts in tree: {allParts.Length}",
                    MessageType.Info);

                if (GUILayout.Button("Recalculate All"))
                {
                    builder.EnsureAssetCached();
                    foreach (var part in allParts)
                        part.Recalculate();
                    SceneView.RepaintAll();
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Generate Mesh Parts"))
                {
                    builder.GenerateMeshParts();
                    EditorUtility.SetDirty(builder.gameObject);
                }
            }
        }
#endif
    }
}
