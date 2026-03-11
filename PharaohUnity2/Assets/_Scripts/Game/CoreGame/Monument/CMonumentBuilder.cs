using System.Collections.Generic;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

namespace Pharaoh
{
    public class CMonumentBuilder : MonoBehaviour
    {
        private const string Tag = "[MonumentBuilder]";

        [SerializeField] private GameObject _sourceAsset;
        [SerializeField] private bool _drawGizmos = true;

        // ───────── Entry points ─────────

        [Button]
        private void Generate()
        {
            if (!ValidateSetup(out MeshFilter filter, out List<CMonumentPart> parts))
                return;

            Mesh sourceMesh = filter.sharedMesh;
            LogGenerateStart(sourceMesh, parts.Count);

            List<CMeshSlicer.Tri> remaining = CMeshSlicer.ExtractTriangles(filter);
            Debug.Log($"{Tag} Extracted {remaining.Count} triangles (world space)");
            LogBounds(remaining, "Source mesh");

            float snapEps = CMeshSlicer.ComputeSnapEpsilon(remaining);
            Debug.Log($"{Tag} Snap epsilon: {snapEps:E3}");

            List<CMeshSlicer.BoxPlanes> allPlanes = new List<CMeshSlicer.BoxPlanes>();

            for (int i = parts.Count - 1; i >= 0; i--)
            {
                remaining = ProcessPart(parts[i], i, remaining, snapEps, allPlanes);
            }

            Debug.Log($"{Tag} Remaining tris after all parts: {remaining.Count} (discarded)");
            Debug.Log($"{Tag} ═══ GENERATE COMPLETE ═══");

            _sourceAsset.SetActive(false);
        }

        [Button]
        private void Clean()
        {
            List<CMonumentPart> parts = CollectParts();
            foreach (CMonumentPart part in parts)
            {
                part.GeneratedMesh = null;
                CleanPartChild(part);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(part);
#endif
            }

            if (_sourceAsset != null)
            {
                _sourceAsset.SetActive(true);
            }

            Debug.Log($"{Tag} Cleaned {parts.Count} parts, source asset re-enabled.");
        }

        // ───────── Per-part processing ─────────

        private List<CMeshSlicer.Tri> ProcessPart(
            CMonumentPart part, int index,
            List<CMeshSlicer.Tri> remaining, float snapEps,
            List<CMeshSlicer.BoxPlanes> allPlanes)
        {
            string partName = part.name;
            Debug.Log($"{Tag} ─── Part [{index}] '{partName}' ───");

            BoxCollider box = part.GetComponent<BoxCollider>();

            LogBoxCollider(box, partName);

            CMeshSlicer.BoxPlanes planes = CMeshSlicer.GetBoxPlanes(box);
            LogBoxPlanes(planes, partName);

            int remainingBefore = remaining.Count;
            List<CMeshSlicer.Tri> clipped = CMeshSlicer.ClipToBox(remaining, planes);
            Debug.Log($"{Tag}   ClipToBox: {remainingBefore} input tris → {clipped.Count} clipped tris");

            remaining = CMeshSlicer.SubtractBox(remaining, planes);
            Debug.Log($"{Tag}   SubtractBox: {remainingBefore} input tris → {remaining.Count} remaining tris");

            if (clipped.Count == 0)
            {
                Debug.LogWarning($"{Tag}   Part '{partName}' got 0 clipped tris → no mesh generated");
                part.GeneratedMesh = null;
                allPlanes.Add(planes);
                return remaining;
            }

            LogBounds(clipped, $"Part '{partName}' clipped");

            List<CMeshSlicer.Tri> caps = CMeshSlicer.BuildCaps(clipped, snapEps, allPlanes, planes, partName);
            allPlanes.Add(planes);
            Debug.Log($"{Tag}   Caps: {caps.Count} cap tris generated");
            clipped.AddRange(caps);

            int geometryCount = clipped.Count - caps.Count;
            Debug.Log($"{Tag}   Total tris for '{partName}': {clipped.Count} " +
                      $"(geometry={geometryCount} + caps={caps.Count})");

            Mesh mesh = CMeshSlicer.BuildMesh(clipped, part.transform);
            Debug.Log($"{Tag}   Mesh built: verts={mesh.vertexCount}, " +
                      $"tris={mesh.triangles.Length / 3}, bounds={mesh.bounds}");
            ApplyMesh(part, mesh);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(part);
#endif

            return remaining;
        }

        // ───────── Validation ─────────

        private bool ValidateSetup(out MeshFilter filter, out List<CMonumentPart> parts)
        {
            filter = null;
            parts = null;

            if (_sourceAsset == null)
            {
                Debug.LogWarning($"{Tag} No source asset assigned.");
                return false;
            }

            filter = _sourceAsset.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                Debug.LogWarning($"{Tag} Source asset '{_sourceAsset.name}' has no MeshFilter or mesh is null.");
                return false;
            }

            parts = CollectParts();
            if (parts.Count == 0)
            {
                Debug.LogWarning($"{Tag} No CMonumentPart children found.");
                return false;
            }

            return true;
        }

        // ───────── Part collection ─────────

        private List<CMonumentPart> CollectParts()
        {
            List<CMonumentPart> parts = new List<CMonumentPart>();
            for (int i = 0; i < transform.childCount; i++)
            {
                CMonumentPart part = transform.GetChild(i).GetComponent<CMonumentPart>();
                if (part != null)
                {
                    parts.Add(part);
                }
            }
            return parts;
        }

        // ───────── Mesh application ─────────

        private static void ApplyMesh(CMonumentPart part, Mesh mesh)
        {
            part.GeneratedMesh = mesh;
            CleanPartChild(part);

            GameObject child = new GameObject("_Generated");
            child.transform.SetParent(part.transform, false);

            MeshFilter filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        private static void CleanPartChild(CMonumentPart part)
        {
            for (int i = part.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = part.transform.GetChild(i);
                if (child.name == "_Generated")
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        // ───────── Logging ─────────

        private void LogGenerateStart(Mesh sourceMesh, int partCount)
        {
            Debug.Log($"{Tag} ═══ GENERATE START ═══");
            Debug.Log($"{Tag} Source: '{_sourceAsset.name}', mesh='{sourceMesh.name}', " +
                      $"verts={sourceMesh.vertexCount}, tris={sourceMesh.triangles.Length / 3}");
            Debug.Log($"{Tag} Parts count: {partCount}");
        }

        private static void LogBounds(List<CMeshSlicer.Tri> tris, string label)
        {
            if (tris.Count == 0)
                return;

            (Vector3 min, Vector3 max) = CMeshSlicer.ComputeBounds(tris);
            Debug.Log($"{Tag}   {label} bounds: min={min}, max={max}, size={max - min}");
        }

        private static void LogBoxCollider(BoxCollider box, string partName)
        {
            Transform t = box.transform;
            Vector3 worldCenter = t.TransformPoint(box.center);
            Vector3 worldSize = Vector3.Scale(box.size, t.lossyScale);

            Debug.Log($"{Tag}   BoxCollider '{partName}': " +
                      $"localCenter={box.center}, localSize={box.size}, " +
                      $"worldCenter={worldCenter}, worldSize≈{worldSize}, " +
                      $"pos={t.position}, rot={t.rotation.eulerAngles}, scale={t.lossyScale}");
        }

        private static void LogBoxPlanes(CMeshSlicer.BoxPlanes planes, string partName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Tag}   Box planes for '{partName}':");
            string[] faceNames = { "+X", "-X", "+Y", "-Y", "+Z", "-Z" };

            for (int p = 0; p < 6; p++)
            {
                sb.Append($"\n{Tag}     [{faceNames[p]}] normal={planes.Normals[p]:F4}, dist={planes.Dists[p]:F4}");
            }

            Debug.Log(sb.ToString());
        }

        // ───────── Gizmos ─────────

        private static readonly Color[] Palette =
        {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.cyan, Color.magenta
        };

        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
                return;

            OnDrawGizmosSelected();
        }

        private void OnDrawGizmosSelected()
        {
            if (_sourceAsset == null)
                return;

            MeshFilter filter = _sourceAsset.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
                return;

            List<CMonumentPart> parts = CollectParts();
            List<CMeshSlicer.Tri> sourceTris = CMeshSlicer.ExtractTriangles(filter);

            for (int p = 0; p < parts.Count; p++)
            {
                BoxCollider box = parts[p].GetComponent<BoxCollider>();
                if (box == null)
                    continue;

                Gizmos.color = Palette[p % Palette.Length];
                CMeshSlicer.BoxPlanes planes = CMeshSlicer.GetBoxPlanes(box);
                DrawCutEdges(sourceTris, planes);
            }
        }

        /// <summary>
        /// For each face of the box, finds where source triangles cross the plane
        /// and draws only the intersection segments that lie within the box face.
        /// </summary>
        private static void DrawCutEdges(List<CMeshSlicer.Tri> tris, CMeshSlicer.BoxPlanes planes)
        {
            for (int pi = 0; pi < 6; pi++)
            {
                Vector3 normal = planes.Normals[pi];
                float dist = planes.Dists[pi];

                foreach (CMeshSlicer.Tri tri in tris)
                {
                    float dA = Vector3.Dot(normal, tri.A) - dist;
                    float dB = Vector3.Dot(normal, tri.B) - dist;
                    float dC = Vector3.Dot(normal, tri.C) - dist;

                    bool aIn = dA <= 0f;
                    bool bIn = dB <= 0f;
                    bool cIn = dC <= 0f;

                    if (aIn == bIn && bIn == cIn)
                        continue;

                    // Find the two edge-plane intersection points
                    Vector3 i0 = Vector3.zero, i1 = Vector3.zero;
                    int count = 0;

                    if (aIn != bIn)
                    {
                        float k = dA / (dA - dB);
                        if (count == 0)
                            i0 = tri.A + k * (tri.B - tri.A);
                        else
                            i1 = tri.A + k * (tri.B - tri.A);
                        count++;
                    }

                    if (bIn != cIn)
                    {
                        float k = dB / (dB - dC);
                        if (count == 0)
                            i0 = tri.B + k * (tri.C - tri.B);
                        else
                            i1 = tri.B + k * (tri.C - tri.B);
                        count++;
                    }

                    if (count < 2 && cIn != aIn)
                    {
                        float k = dC / (dC - dA);
                        i1 = tri.C + k * (tri.A - tri.C);
                        count++;
                    }

                    if (count < 2)
                        continue;

                    // Only draw if both points are inside the box
                    if (IsInsideBox(i0, planes) && IsInsideBox(i1, planes))
                    {
                        Gizmos.DrawLine(i0, i1);
                    }
                }
            }
        }

        private static bool IsInsideBox(Vector3 point, CMeshSlicer.BoxPlanes planes)
        {
            const float tolerance = 0.001f;
            for (int i = 0; i < 6; i++)
            {
                if (Vector3.Dot(planes.Normals[i], point) - planes.Dists[i] > tolerance)
                    return false;
            }
            return true;
        }
    }
}
