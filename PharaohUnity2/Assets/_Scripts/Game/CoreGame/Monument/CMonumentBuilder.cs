using System.Collections.Generic;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

namespace Pharaoh
{
    public class CMonumentBuilder : MonoBehaviour
    {
        private const string Tag = "[MonumentBuilder]";

        private static readonly Color[] Palette =
        {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.cyan, Color.magenta
        };

        [SerializeField] private GameObject _sourceAsset;
        [SerializeField] private bool _drawGizmos = true;

        // ───────── Tri struct ─────────

        private struct Tri
        {
            public Vector3 A, B, C;

            public Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                A = a;
                B = b;
                C = c;
            }
        }

        private struct BoxPlanes
        {
            public Vector3[] Normals;
            public float[] Dists;
        }

        // ───────── Entry points ─────────

        [Button]
        private void Generate()
        {
            if (_sourceAsset == null)
            {
                Debug.LogWarning($"{Tag} No source asset assigned.");
                return;
            }

            var filter = _sourceAsset.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                Debug.LogWarning($"{Tag} Source asset '{_sourceAsset.name}' has no MeshFilter or mesh is null.");
                return;
            }

            var parts = CollectParts();
            if (parts.Count == 0)
            {
                Debug.LogWarning($"{Tag} No CMonumentPart children found.");
                return;
            }

            var sourceMesh = filter.sharedMesh;
            Debug.Log($"{Tag} ═══ GENERATE START ═══");
            Debug.Log($"{Tag} Source: '{_sourceAsset.name}', mesh='{sourceMesh.name}', " +
                      $"verts={sourceMesh.vertexCount}, tris={sourceMesh.triangles.Length / 3}");
            Debug.Log($"{Tag} Parts count: {parts.Count}");

            var remaining = ExtractTriangles(filter);
            Debug.Log($"{Tag} Extracted {remaining.Count} triangles (world space)");
            LogBounds(remaining, "Source mesh");

            float snapEps = ComputeSnapEpsilon(remaining);
            Debug.Log($"{Tag} Snap epsilon: {snapEps:E3}");

            // Accumulate all cutting planes so later parts can cap cuts from earlier parts
            var allPlanes = new List<BoxPlanes>();

            // Process from last to first
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                var partName = parts[i].name;
                Debug.Log($"{Tag} ─── Part [{i}] '{partName}' ───");

                var box = parts[i].GetComponent<BoxCollider>();
                if (box == null)
                {
                    Debug.LogWarning($"{Tag}   Part '{partName}' has no BoxCollider → skipping");
                    parts[i].GeneratedMesh = null;
                    continue;
                }

                LogBoxCollider(box, partName);

                var planes = GetBoxPlanes(box);
                LogBoxPlanes(planes, partName);

                int remainingBefore = remaining.Count;
                var clipped = ClipToBox(remaining, planes);
                Debug.Log($"{Tag}   ClipToBox: {remainingBefore} input tris → {clipped.Count} clipped tris");

                remaining = SubtractBox(remaining, planes);
                Debug.Log($"{Tag}   SubtractBox: {remainingBefore} input tris → {remaining.Count} remaining tris");

                if (clipped.Count == 0)
                {
                    Debug.LogWarning($"{Tag}   Part '{partName}' got 0 clipped tris → no mesh generated");
                    parts[i].GeneratedMesh = null;
                    allPlanes.Add(planes);
                    continue;
                }

                LogBounds(clipped, $"Part '{partName}' clipped");

                var caps = BuildCaps(clipped, snapEps, allPlanes, planes, partName);
                allPlanes.Add(planes);
                Debug.Log($"{Tag}   Caps: {caps.Count} cap tris generated");
                clipped.AddRange(caps);

                Debug.Log($"{Tag}   Total tris for '{partName}': {clipped.Count} " +
                          $"(geometry={clipped.Count - caps.Count} + caps={caps.Count})");

                var mesh = BuildMesh(clipped, parts[i].transform);
                Debug.Log($"{Tag}   Mesh built: verts={mesh.vertexCount}, " +
                          $"tris={mesh.triangles.Length / 3}, bounds={mesh.bounds}");
                ApplyMesh(parts[i], mesh);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(parts[i]);
#endif
            }

            Debug.Log($"{Tag} Remaining tris after all parts: {remaining.Count} (discarded)");
            Debug.Log($"{Tag} ═══ GENERATE COMPLETE ═══");

            _sourceAsset.SetActive(false);
        }

        [Button]
        private void Clean()
        {
            var parts = CollectParts();
            foreach (var part in parts)
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

        // ───────── Vertex clustering ─────────

        /// <summary>
        /// Tolerance-based vertex clustering. Maps each vertex to an integer index.
        /// Two vertices closer than tolerance are considered the same vertex.
        /// O(n²) but reliable — no grid boundary issues.
        /// </summary>
        private class VertexMap
        {
            private readonly List<Vector3> _positions = new List<Vector3>();
            private readonly float _toleranceSq;

            public VertexMap(float tolerance)
            {
                _toleranceSq = tolerance * tolerance;
            }

            public int GetIndex(Vector3 v)
            {
                for (int i = 0; i < _positions.Count; i++)
                {
                    if ((_positions[i] - v).sqrMagnitude < _toleranceSq)
                    {
                        return i;
                    }
                }

                _positions.Add(v);
                return _positions.Count - 1;
            }

            public Vector3 GetPosition(int index)
            {
                return _positions[index];
            }

            public int Count => _positions.Count;
        }

        private static float ComputeSnapEpsilon(List<Tri> tris)
        {
            float maxAbs = 1f;
            foreach (var tri in tris)
            {
                maxAbs = Mathf.Max(maxAbs, MaxAbs(tri.A));
                maxAbs = Mathf.Max(maxAbs, MaxAbs(tri.B));
                maxAbs = Mathf.Max(maxAbs, MaxAbs(tri.C));
            }

            // Float32 has ~7 decimal digits precision.
            // After clipping arithmetic, errors scale with coordinate magnitude.
            return Mathf.Max(maxAbs * 5e-6f, 1e-5f);
        }

        private static float MaxAbs(Vector3 v)
        {
            return Mathf.Max(Mathf.Abs(v.x), Mathf.Max(Mathf.Abs(v.y), Mathf.Abs(v.z)));
        }

        // ───────── Logging helpers ─────────

        private static void LogBounds(List<Tri> tris, string label)
        {
            if (tris.Count == 0)
                return;

            var min = tris[0].A;
            var max = tris[0].A;

            foreach (var tri in tris)
            {
                min = Vector3.Min(min, Vector3.Min(tri.A, Vector3.Min(tri.B, tri.C)));
                max = Vector3.Max(max, Vector3.Max(tri.A, Vector3.Max(tri.B, tri.C)));
            }

            Debug.Log($"{Tag}   {label} bounds: min={min}, max={max}, size={max - min}");
        }

        private static void LogBoxCollider(BoxCollider box, string partName)
        {
            var t = box.transform;
            var worldCenter = t.TransformPoint(box.center);
            var worldSize = Vector3.Scale(box.size, t.lossyScale);

            Debug.Log($"{Tag}   BoxCollider '{partName}': " +
                      $"localCenter={box.center}, localSize={box.size}, " +
                      $"worldCenter={worldCenter}, worldSize≈{worldSize}, " +
                      $"pos={t.position}, rot={t.rotation.eulerAngles}, scale={t.lossyScale}");
        }

        private static void LogBoxPlanes(BoxPlanes planes, string partName)
        {
            var sb = new StringBuilder();
            sb.Append($"{Tag}   Box planes for '{partName}':");
            string[] faceNames = { "+X", "-X", "+Y", "-Y", "+Z", "-Z" };

            for (int p = 0; p < 6; p++)
            {
                sb.Append($"\n{Tag}     [{faceNames[p]}] normal={planes.Normals[p]:F4}, dist={planes.Dists[p]:F4}");
            }

            Debug.Log(sb.ToString());
        }

        // ───────── Part collection ─────────

        private List<CMonumentPart> CollectParts()
        {
            var parts = new List<CMonumentPart>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var part = transform.GetChild(i).GetComponent<CMonumentPart>();
                if (part != null)
                {
                    parts.Add(part);
                }
            }
            return parts;
        }

        // ───────── Triangle extraction ─────────

        private static List<Tri> ExtractTriangles(MeshFilter filter)
        {
            var mesh = filter.sharedMesh;
            var verts = mesh.vertices;
            var tris = mesh.triangles;
            var t = filter.transform;
            var result = new List<Tri>(tris.Length / 3);

            for (int i = 0; i < tris.Length; i += 3)
            {
                result.Add(new Tri(
                    t.TransformPoint(verts[tris[i]]),
                    t.TransformPoint(verts[tris[i + 1]]),
                    t.TransformPoint(verts[tris[i + 2]])));
            }

            return result;
        }

        // ───────── Box planes ─────────

        private static BoxPlanes GetBoxPlanes(BoxCollider box)
        {
            var normals = new Vector3[6];
            var dists = new float[6];
            var t = box.transform;
            var c = box.center;
            var e = box.size * 0.5f;

            for (int axis = 0; axis < 3; axis++)
            {
                var localN = Vector3.zero;
                localN[axis] = 1f;
                var worldN = t.TransformDirection(localN).normalized;

                var posLocal = c;
                posLocal[axis] += e[axis];
                var negLocal = c;
                negLocal[axis] -= e[axis];

                normals[axis * 2] = worldN;
                dists[axis * 2] = Vector3.Dot(worldN, t.TransformPoint(posLocal));
                normals[axis * 2 + 1] = -worldN;
                dists[axis * 2 + 1] = Vector3.Dot(-worldN, t.TransformPoint(negLocal));
            }

            return new BoxPlanes { Normals = normals, Dists = dists };
        }

        // ───────── Clip to box (intersection) ─────────

        private static List<Tri> ClipToBox(List<Tri> tris, BoxPlanes planes)
        {
            var result = new List<Tri>();
            var polygon = new List<Vector3>(9);

            for (int t = 0; t < tris.Count; t++)
            {
                polygon.Clear();
                polygon.Add(tris[t].A);
                polygon.Add(tris[t].B);
                polygon.Add(tris[t].C);

                for (int p = 0; p < 6 && polygon.Count >= 3; p++)
                {
                    ClipPolygonByPlane(polygon, planes.Normals[p], planes.Dists[p]);
                }

                if (polygon.Count < 3)
                    continue;

                EmitPolygonAsTris(polygon, result);
            }

            return result;
        }

        // ───────── Subtract box (boolean minus) ─────────

        private static List<Tri> SubtractBox(List<Tri> tris, BoxPlanes planes)
        {
            var result = new List<Tri>();
            var remainder = new List<Vector3>(9);
            var inside = new List<Vector3>(9);
            var outside = new List<Vector3>(9);

            for (int t = 0; t < tris.Count; t++)
            {
                remainder.Clear();
                remainder.Add(tris[t].A);
                remainder.Add(tris[t].B);
                remainder.Add(tris[t].C);

                for (int p = 0; p < 6; p++)
                {
                    if (remainder.Count < 3)
                        break;

                    SplitPolygonByPlane(remainder, planes.Normals[p], planes.Dists[p],
                        inside, outside);

                    if (outside.Count >= 3)
                    {
                        EmitPolygonAsTris(outside, result);
                    }

                    remainder.Clear();
                    remainder.AddRange(inside);
                }

                // remainder is fully inside the box → discard
            }

            return result;
        }

        // ───────── Cap generation (vertex-based angle-sort) ─────────

        private static List<Tri> BuildCaps(
            List<Tri> clippedTris, float snapEps,
            List<BoxPlanes> previousPlanes, BoxPlanes currentPlanes, string partName)
        {
            var caps = new List<Tri>();
            var vmap = new VertexMap(snapEps);

            // Find boundary half-edges via cancellation.
            // Interior edges appear in both directions and cancel; boundary edges remain.
            var boundaryHalfEdges = new HashSet<(int from, int to)>();

            foreach (var tri in clippedTris)
            {
                int ia = vmap.GetIndex(tri.A);
                int ib = vmap.GetIndex(tri.B);
                int ic = vmap.GetIndex(tri.C);

                ProcessHalfEdge(boundaryHalfEdges, ia, ib);
                ProcessHalfEdge(boundaryHalfEdges, ib, ic);
                ProcessHalfEdge(boundaryHalfEdges, ic, ia);
            }

            int totalPlaneCount = (previousPlanes.Count + 1) * 6;
            Debug.Log($"{Tag}   BuildCaps '{partName}': {boundaryHalfEdges.Count} boundary half-edges, " +
                      $"{vmap.Count} unique vertices, checking {totalPlaneCount} planes");

            if (boundaryHalfEdges.Count == 0)
            {
                Debug.Log($"{Tag}   BuildCaps '{partName}': mesh is closed → no caps needed");
                return caps;
            }

            // Collect unique boundary vertex indices
            var boundaryVertIndices = new HashSet<int>();
            foreach (var he in boundaryHalfEdges)
            {
                boundaryVertIndices.Add(he.from);
                boundaryVertIndices.Add(he.to);
            }

            // Tight tolerance: clipped vertices should be within ~1e-3 of their plane
            float planeTol = snapEps * 2f;

            // Current part's planes: cap faces outward from the clipped piece (+normal)
            CapFacesForPlanes(caps, boundaryVertIndices, vmap, currentPlanes, planeTol, false, partName);

            // Previous parts' planes: cap faces outward from subtract remainder (-normal)
            foreach (var bp in previousPlanes)
            {
                CapFacesForPlanes(caps, boundaryVertIndices, vmap, bp, planeTol, true, partName);
            }

            return caps;
        }

        /// <summary>
        /// For each face of the given box, collect boundary vertices on that face,
        /// sort by angle around centroid, and fan-triangulate.
        /// </summary>
        private static void CapFacesForPlanes(
            List<Tri> caps, HashSet<int> boundaryVertIndices, VertexMap vmap,
            BoxPlanes planes, float planeTol, bool flipNormal, string partName)
        {
            for (int p = 0; p < 6; p++)
            {
                var faceNormal = planes.Normals[p];
                var dist = planes.Dists[p];

                // Collect boundary vertices on this plane
                var faceVerts = new List<Vector3>();
                foreach (int vi in boundaryVertIndices)
                {
                    var pos = vmap.GetPosition(vi);
                    if (Mathf.Abs(Vector3.Dot(faceNormal, pos) - dist) < planeTol)
                    {
                        // Project exactly onto plane
                        float d = Vector3.Dot(faceNormal, pos) - dist;
                        faceVerts.Add(pos - d * faceNormal);
                    }
                }

                if (faceVerts.Count < 3)
                    continue;

                // Build 2D tangent basis for the plane
                var tangent = Vector3.Cross(faceNormal,
                    Mathf.Abs(faceNormal.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
                var bitangent = Vector3.Cross(faceNormal, tangent);

                // Compute centroid
                var centroid = Vector3.zero;
                foreach (var v in faceVerts)
                {
                    centroid += v;
                }
                centroid /= faceVerts.Count;

                // Sort vertices by angle around centroid (2D projection on face plane)
                faceVerts.Sort((a, b) =>
                {
                    float angleA = Mathf.Atan2(
                        Vector3.Dot(a - centroid, bitangent),
                        Vector3.Dot(a - centroid, tangent));
                    float angleB = Mathf.Atan2(
                        Vector3.Dot(b - centroid, bitangent),
                        Vector3.Dot(b - centroid, tangent));
                    return angleA.CompareTo(angleB);
                });

                // Angle sort always produces CCW from +faceNormal perspective
                // (tangent, bitangent, faceNormal) is always right-handed.
                // CCW fan triangulation → normal = +faceNormal.
                // For currentPlanes: cap should face +faceNormal → no flip.
                // For previousPlanes: cap should face -faceNormal → flip.
                bool needsFlip = flipNormal;

                Debug.Log($"{Tag}     Plane (n={faceNormal:F3}, d={dist:F2}): " +
                          $"{faceVerts.Count} cap verts, flip={needsFlip}");

                // Fan triangulate
                for (int i = 1; i < faceVerts.Count - 1; i++)
                {
                    if (needsFlip)
                    {
                        caps.Add(new Tri(faceVerts[0], faceVerts[i + 1], faceVerts[i]));
                    }
                    else
                    {
                        caps.Add(new Tri(faceVerts[0], faceVerts[i], faceVerts[i + 1]));
                    }
                }
            }
        }

        private static void ProcessHalfEdge(
            HashSet<(int, int)> halfEdges, int a, int b)
        {
            var reverse = (b, a);
            if (halfEdges.Contains(reverse))
            {
                halfEdges.Remove(reverse);
            }
            else
            {
                halfEdges.Add((a, b));
            }
        }

        // ───────── Polygon operations ─────────

        private static void ClipPolygonByPlane(List<Vector3> polygon, Vector3 normal, float dist)
        {
            var buffer = new List<Vector3>(polygon.Count + 1);

            for (int i = 0; i < polygon.Count; i++)
            {
                var current = polygon[i];
                var next = polygon[(i + 1) % polygon.Count];
                float dCurrent = Vector3.Dot(normal, current) - dist;
                float dNext = Vector3.Dot(normal, next) - dist;

                if (dCurrent <= 0f)
                {
                    buffer.Add(current);
                }

                if ((dCurrent <= 0f) != (dNext <= 0f))
                {
                    float k = dCurrent / (dCurrent - dNext);
                    buffer.Add(current + k * (next - current));
                }
            }

            polygon.Clear();
            polygon.AddRange(buffer);
        }

        private static void SplitPolygonByPlane(
            List<Vector3> polygon, Vector3 normal, float dist,
            List<Vector3> inside, List<Vector3> outside)
        {
            inside.Clear();
            outside.Clear();

            for (int i = 0; i < polygon.Count; i++)
            {
                var current = polygon[i];
                var next = polygon[(i + 1) % polygon.Count];
                float dCurrent = Vector3.Dot(normal, current) - dist;
                float dNext = Vector3.Dot(normal, next) - dist;

                if (dCurrent <= 0f)
                {
                    inside.Add(current);
                }
                else
                {
                    outside.Add(current);
                }

                if ((dCurrent <= 0f) != (dNext <= 0f))
                {
                    float k = dCurrent / (dCurrent - dNext);
                    var intersection = current + k * (next - current);
                    inside.Add(intersection);
                    outside.Add(intersection);
                }
            }
        }

        // ───────── Emit helpers ─────────

        private static void EmitPolygonAsTris(List<Vector3> polygon, List<Tri> output)
        {
            for (int i = 1; i < polygon.Count - 1; i++)
            {
                output.Add(new Tri(polygon[0], polygon[i], polygon[i + 1]));
            }
        }

        // ───────── Mesh building ─────────

        private static Mesh BuildMesh(List<Tri> tris, Transform partTransform)
        {
            var verts = new Vector3[tris.Count * 3];
            var indices = new int[tris.Count * 3];

            for (int i = 0; i < tris.Count; i++)
            {
                int idx = i * 3;
                verts[idx] = partTransform.InverseTransformPoint(tris[i].A);
                verts[idx + 1] = partTransform.InverseTransformPoint(tris[i].B);
                verts[idx + 2] = partTransform.InverseTransformPoint(tris[i].C);
                indices[idx] = idx;
                indices[idx + 1] = idx + 1;
                indices[idx + 2] = idx + 2;
            }

            var mesh = new Mesh { name = partTransform.name + "_generated" };
            mesh.vertices = verts;
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ───────── Part child cleanup ─────────

        private static void CleanPartChild(CMonumentPart part)
        {
            for (int i = part.transform.childCount - 1; i >= 0; i--)
            {
                var child = part.transform.GetChild(i);
                if (child.name == "_Generated")
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        // ───────── Mesh application ─────────

        private static void ApplyMesh(CMonumentPart part, Mesh mesh)
        {
            part.GeneratedMesh = mesh;

            // Clean previous generated child if any
            CleanPartChild(part);

            var child = new GameObject("_Generated");
            child.transform.SetParent(part.transform, false);

            var filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        // ───────── Gizmos ─────────

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

            var parts = CollectParts();

            for (int p = 0; p < parts.Count; p++)
            {
                var mesh = parts[p].GeneratedMesh;
                if (mesh == null)
                    continue;

                Gizmos.color = Palette[p % Palette.Length];
                DrawWireframe(mesh, parts[p].transform);
            }
        }

        private static void DrawWireframe(Mesh mesh, Transform t)
        {
            var verts = mesh.vertices;
            var tris = mesh.triangles;

            for (int i = 0; i < tris.Length; i += 3)
            {
                var v0 = t.TransformPoint(verts[tris[i]]);
                var v1 = t.TransformPoint(verts[tris[i + 1]]);
                var v2 = t.TransformPoint(verts[tris[i + 2]]);
                Gizmos.DrawLine(v0, v1);
                Gizmos.DrawLine(v1, v2);
                Gizmos.DrawLine(v2, v0);
            }
        }
    }
}
