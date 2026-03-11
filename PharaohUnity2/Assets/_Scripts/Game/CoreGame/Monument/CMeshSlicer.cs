using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh
{
    /// <summary>
    /// Pure geometry utility for slicing meshes with box colliders.
    /// Stateless — all methods are static. No Unity lifecycle dependencies.
    /// </summary>
    public static class CMeshSlicer
    {
        // ───────── Data types ─────────

        public struct Tri
        {
            public Vector3 A, B, C;

            public Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                A = a;
                B = b;
                C = c;
            }
        }

        public struct BoxPlanes
        {
            public Vector3[] Normals;
            public float[] Dists;
        }

        // ───────── Vertex clustering ─────────

        /// <summary>
        /// Tolerance-based vertex clustering. Maps each vertex to an integer index.
        /// Two vertices closer than tolerance are considered the same vertex.
        /// O(n²) but reliable — no grid boundary issues.
        /// </summary>
        public class VertexMap
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

        // ───────── Triangle extraction ─────────

        public static List<Tri> ExtractTriangles(MeshFilter filter)
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

        public static BoxPlanes GetBoxPlanes(BoxCollider box)
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

        // ───────── Epsilon ─────────

        public static float ComputeSnapEpsilon(List<Tri> tris)
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

        // ───────── Clip to box (intersection) ─────────

        public static List<Tri> ClipToBox(List<Tri> tris, BoxPlanes planes)
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

        // ───────── Subtract box (boolean difference) ─────────

        public static List<Tri> SubtractBox(List<Tri> tris, BoxPlanes planes)
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

        // ───────── Cap generation ─────────

        public static List<Tri> BuildCaps(
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

            Debug.Log($"[MeshSlicer]   BuildCaps '{partName}': {boundaryHalfEdges.Count} boundary half-edges, " +
                      $"{vmap.Count} unique vertices, checking {(previousPlanes.Count + 1) * 6} planes");

            if (boundaryHalfEdges.Count == 0)
            {
                Debug.Log($"[MeshSlicer]   BuildCaps '{partName}': mesh is closed → no caps needed");
                return caps;
            }

            // Collect unique boundary vertex indices
            var boundaryVertIndices = new HashSet<int>();
            foreach (var he in boundaryHalfEdges)
            {
                boundaryVertIndices.Add(he.from);
                boundaryVertIndices.Add(he.to);
            }

            float planeTol = snapEps * 2f;

            // Current part's planes: cap faces outward (+normal)
            GenerateCapFaces(caps, boundaryVertIndices, vmap, currentPlanes, planeTol, false, partName);

            // Previous parts' planes: cap faces inward (-normal)
            foreach (var bp in previousPlanes)
            {
                GenerateCapFaces(caps, boundaryVertIndices, vmap, bp, planeTol, true, partName);
            }

            return caps;
        }

        // ───────── Mesh building ─────────

        public static Mesh BuildMesh(List<Tri> tris, Transform partTransform)
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

        // ───────── Bounds utility ─────────

        public static (Vector3 min, Vector3 max) ComputeBounds(List<Tri> tris)
        {
            var min = tris[0].A;
            var max = tris[0].A;

            foreach (var tri in tris)
            {
                min = Vector3.Min(min, Vector3.Min(tri.A, Vector3.Min(tri.B, tri.C)));
                max = Vector3.Max(max, Vector3.Max(tri.A, Vector3.Max(tri.B, tri.C)));
            }

            return (min, max);
        }

        // ───────── Internal: cap face generation ─────────

        /// <summary>
        /// For each face of the given box, collect boundary vertices on that face,
        /// sort by angle around centroid, and fan-triangulate.
        /// </summary>
        private static void GenerateCapFaces(
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

                Debug.Log($"[MeshSlicer]     Plane (n={faceNormal:F3}, d={dist:F2}): " +
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

        // ───────── Internal: half-edge cancellation ─────────

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

        // ───────── Internal: polygon operations ─────────

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

        private static void EmitPolygonAsTris(List<Vector3> polygon, List<Tri> output)
        {
            for (int i = 1; i < polygon.Count - 1; i++)
            {
                output.Add(new Tri(polygon[0], polygon[i], polygon[i + 1]));
            }
        }
    }
}
