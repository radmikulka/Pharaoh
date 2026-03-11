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

        // ───────── Cell planes (grid subdivision) ─────────

        public static BoxPlanes GetCellPlanes(Transform t, Vector3 localMin, Vector3 localMax)
        {
            var normals = new Vector3[6];
            var dists = new float[6];

            for (int axis = 0; axis < 3; axis++)
            {
                var localN = Vector3.zero;
                localN[axis] = 1f;
                var worldN = t.TransformDirection(localN).normalized;

                var posLocal = localMax;
                var negLocal = localMin;

                normals[axis * 2] = worldN;
                dists[axis * 2] = Vector3.Dot(worldN, t.TransformPoint(posLocal));
                normals[axis * 2 + 1] = -worldN;
                dists[axis * 2 + 1] = Vector3.Dot(-worldN, t.TransformPoint(negLocal));
            }

            return new BoxPlanes { Normals = normals, Dists = dists };
        }

        // ───────── Point-in-mesh test ─────────

        /// <summary>
        /// Ray-cast test: fires rays from point, counts triangle intersections.
        /// Odd count = inside closed mesh. Uses 3 non-axis-aligned directions
        /// to avoid edge/vertex precision issues, takes majority vote.
        /// </summary>
        public static bool IsPointInsideMesh(Vector3 point, List<Tri> tris)
        {
            // Non-axis-aligned directions to avoid hitting edges/vertices of regular meshes
            Vector3[] dirs =
            {
                new Vector3(0.31f, 0.95f, 0.07f).normalized,
                new Vector3(-0.17f, 0.43f, 0.89f).normalized,
                new Vector3(0.73f, -0.21f, 0.65f).normalized,
            };

            int insideVotes = 0;

            for (int d = 0; d < dirs.Length; d++)
            {
                int intersections = 0;

                foreach (var tri in tris)
                {
                    if (RayHitsTriangle(point, dirs[d], tri))
                    {
                        intersections++;
                    }
                }

                bool inside = (intersections % 2) == 1;
                Debug.Log($"[MeshSlicer]   Ray #{d} dir={dirs[d]:F3}: {intersections} hits → {(inside ? "INSIDE" : "OUTSIDE")}");

                if (inside)
                {
                    insideVotes++;
                }
            }

            return insideVotes >= 2;
        }

        private static bool RayHitsTriangle(Vector3 origin, Vector3 dir, Tri tri)
        {
            // Möller–Trumbore
            Vector3 e1 = tri.B - tri.A;
            Vector3 e2 = tri.C - tri.A;
            Vector3 h = Vector3.Cross(dir, e2);
            float a = Vector3.Dot(e1, h);

            if (Mathf.Abs(a) < 1e-7f)
                return false;

            float f = 1f / a;
            Vector3 s = origin - tri.A;
            float u = f * Vector3.Dot(s, h);

            if (u < 0f || u > 1f)
                return false;

            Vector3 q = Vector3.Cross(s, e1);
            float v = f * Vector3.Dot(dir, q);

            if (v < 0f || u + v > 1f)
                return false;

            float t = f * Vector3.Dot(e2, q);
            return t > 1e-7f;
        }

        // ───────── Box face generation ─────────

        /// <summary>
        /// Generates 12 world-space triangles forming the 6 faces of a BoxCollider.
        /// flipNormals=false → outward normals; flipNormals=true → inward normals.
        /// </summary>
        public static List<Tri> GenerateBoxTris(BoxCollider box, bool flipNormals)
        {
            Transform t = box.transform;
            Vector3 c = box.center;
            Vector3 e = box.size * 0.5f;

            // 8 corners in world space
            // Bit layout: bit0=X sign, bit1=Y sign, bit2=Z sign
            // 0:(-,-,-) 1:(+,-,-) 2:(-,+,-) 3:(+,+,-) 4:(-,-,+) 5:(+,-,+) 6:(-,+,+) 7:(+,+,+)
            var corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                corners[i] = t.TransformPoint(new Vector3(
                    c.x + ((i & 1) == 0 ? -e.x : e.x),
                    c.y + ((i & 2) == 0 ? -e.y : e.y),
                    c.z + ((i & 4) == 0 ? -e.z : e.z)));
            }

            // 6 face quads, CCW winding → outward normals
            int[,] faces =
            {
                { 1, 3, 7, 5 }, // +X
                { 0, 4, 6, 2 }, // -X
                { 2, 6, 7, 3 }, // +Y
                { 0, 1, 5, 4 }, // -Y
                { 4, 5, 7, 6 }, // +Z
                { 0, 2, 3, 1 }, // -Z
            };

            var tris = new List<Tri>(12);

            for (int f = 0; f < 6; f++)
            {
                Vector3 a = corners[faces[f, 0]];
                Vector3 b = corners[faces[f, 1]];
                Vector3 cc = corners[faces[f, 2]];
                Vector3 d = corners[faces[f, 3]];

                if (flipNormals)
                {
                    tris.Add(new Tri(a, cc, b));
                    tris.Add(new Tri(a, d, cc));
                }
                else
                {
                    tris.Add(new Tri(a, b, cc));
                    tris.Add(new Tri(a, cc, d));
                }
            }

            return tris;
        }

        /// <summary>
        /// Generates 12 world-space triangles forming the 6 faces of an axis-aligned box
        /// defined by localMin/localMax in the given Transform's local space.
        /// </summary>
        public static List<Tri> GenerateBoxTris(Transform t, Vector3 localMin, Vector3 localMax, bool flipNormals)
        {
            // 8 corners in world space
            // Bit layout: bit0=X sign, bit1=Y sign, bit2=Z sign
            // 0:(min,min,min) 1:(max,min,min) 2:(min,max,min) 3:(max,max,min)
            // 4:(min,min,max) 5:(max,min,max) 6:(min,max,max) 7:(max,max,max)
            var corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                corners[i] = t.TransformPoint(new Vector3(
                    (i & 1) == 0 ? localMin.x : localMax.x,
                    (i & 2) == 0 ? localMin.y : localMax.y,
                    (i & 4) == 0 ? localMin.z : localMax.z));
            }

            // 6 face quads, CCW winding → outward normals
            int[,] faces =
            {
                { 1, 3, 7, 5 }, // +X
                { 0, 4, 6, 2 }, // -X
                { 2, 6, 7, 3 }, // +Y
                { 0, 1, 5, 4 }, // -Y
                { 4, 5, 7, 6 }, // +Z
                { 0, 2, 3, 1 }, // -Z
            };

            var tris = new List<Tri>(12);

            for (int f = 0; f < 6; f++)
            {
                Vector3 a = corners[faces[f, 0]];
                Vector3 b = corners[faces[f, 1]];
                Vector3 cc = corners[faces[f, 2]];
                Vector3 d = corners[faces[f, 3]];

                if (flipNormals)
                {
                    tris.Add(new Tri(a, cc, b));
                    tris.Add(new Tri(a, d, cc));
                }
                else
                {
                    tris.Add(new Tri(a, b, cc));
                    tris.Add(new Tri(a, cc, d));
                }
            }

            return tris;
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

        // ───────── Vertex snapping ─────────

        /// <summary>
        /// Snaps vertices that are within tolerance of a box plane to lie exactly on that plane.
        /// Eliminates floating-point gaps between clipped surface tris and cap tris.
        /// </summary>
        public static void SnapVerticesToPlanes(List<Tri> tris, BoxPlanes planes, float tolerance)
        {
            for (int i = 0; i < tris.Count; i++)
            {
                tris[i] = new Tri(
                    SnapVertex(tris[i].A, planes, tolerance),
                    SnapVertex(tris[i].B, planes, tolerance),
                    SnapVertex(tris[i].C, planes, tolerance));
            }
        }

        private static Vector3 SnapVertex(Vector3 v, BoxPlanes planes, float tolerance)
        {
            for (int p = 0; p < 6; p++)
            {
                float d = Vector3.Dot(planes.Normals[p], v) - planes.Dists[p];
                if (Mathf.Abs(d) < tolerance)
                {
                    v -= d * planes.Normals[p];
                }
            }

            return v;
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

        // ───────── Cell cap generation (grid subdivision) ─────────

        /// <summary>
        /// Builds caps for a grid cell. Unlike BuildCaps, also adds cell face corners
        /// that are inside the part mesh — producing solid "brick" walls, not just
        /// boundary-loop patches.
        /// </summary>
        public static List<Tri> BuildCellCaps(
            List<Tri> clippedTris, float snapEps,
            BoxPlanes cellPlanes, Transform t, Vector3 cellMin, Vector3 cellMax,
            List<Tri> partTris, string cellName)
        {
            var caps = new List<Tri>();
            var vmap = new VertexMap(snapEps);

            // Find boundary half-edges via cancellation
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

            Debug.Log($"[MeshSlicer]   BuildCellCaps '{cellName}': {boundaryHalfEdges.Count} boundary half-edges");

            if (boundaryHalfEdges.Count == 0)
            {
                Debug.Log($"[MeshSlicer]   BuildCellCaps '{cellName}': mesh is closed → no caps needed");
                return caps;
            }

            var boundaryVertIndices = new HashSet<int>();
            foreach (var he in boundaryHalfEdges)
            {
                boundaryVertIndices.Add(he.from);
                boundaryVertIndices.Add(he.to);
            }

            float planeTol = snapEps * 2f;

            // 8 cell corners in world space (bit layout: bit0=X, bit1=Y, bit2=Z)
            var worldCorners = new Vector3[8];
            var cornerInside = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                worldCorners[i] = t.TransformPoint(new Vector3(
                    (i & 1) == 0 ? cellMin.x : cellMax.x,
                    (i & 2) == 0 ? cellMin.y : cellMax.y,
                    (i & 4) == 0 ? cellMin.z : cellMax.z));
                cornerInside[i] = IsPointInsideMesh(worldCorners[i], partTris);
            }

            // Which corners belong to which face (same layout as GenerateBoxTris)
            int[][] faceCornerIndices =
            {
                new[] { 1, 3, 5, 7 }, // +X
                new[] { 0, 2, 4, 6 }, // -X
                new[] { 2, 3, 6, 7 }, // +Y
                new[] { 0, 1, 4, 5 }, // -Y
                new[] { 4, 5, 6, 7 }, // +Z
                new[] { 0, 1, 2, 3 }, // -Z
            };

            for (int p = 0; p < 6; p++)
            {
                var faceNormal = cellPlanes.Normals[p];
                var dist = cellPlanes.Dists[p];

                // Collect boundary vertices on this face
                var faceVerts = new List<Vector3>();
                foreach (int vi in boundaryVertIndices)
                {
                    var pos = vmap.GetPosition(vi);
                    if (Mathf.Abs(Vector3.Dot(faceNormal, pos) - dist) < planeTol)
                    {
                        float d = Vector3.Dot(faceNormal, pos) - dist;
                        faceVerts.Add(pos - d * faceNormal);
                    }
                }

                // Add cell face corners that are inside the part mesh
                int cornersInsideCount = 0;
                for (int c = 0; c < 4; c++)
                {
                    int ci = faceCornerIndices[p][c];
                    if (cornerInside[ci])
                    {
                        Vector3 corner = worldCorners[ci];
                        float d = Vector3.Dot(faceNormal, corner) - dist;
                        faceVerts.Add(corner - d * faceNormal);
                        cornersInsideCount++;
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

                Debug.Log($"[MeshSlicer]     CellCap plane {p} (n={faceNormal:F3}): " +
                          $"{faceVerts.Count} verts ({cornersInsideCount} corners inside)");

                // Fan triangulate — CCW from +faceNormal → outward facing
                for (int i = 1; i < faceVerts.Count - 1; i++)
                {
                    caps.Add(new Tri(faceVerts[0], faceVerts[i], faceVerts[i + 1]));
                }
            }

            return caps;
        }

        // ───────── Mesh building ─────────

        /// <summary>
        /// Builds a Unity Mesh from world-space triangles.
        /// smoothingAngleDeg = 0 → per-face normals (RecalculateNormals).
        /// smoothingAngleDeg > 0 → smooth normals: face normals are averaged at shared
        /// vertex positions when the angle between them is below the threshold.
        /// </summary>
        public static Mesh BuildMesh(List<Tri> tris, Transform partTransform, float smoothingAngleDeg = 0f)
        {
            int vertCount = tris.Count * 3;
            var verts = new Vector3[vertCount];
            var indices = new int[vertCount];

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

            if (smoothingAngleDeg > 0f)
            {
                ComputeSmoothNormals(mesh, verts, tris.Count, smoothingAngleDeg);
            }
            else
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static void ComputeSmoothNormals(Mesh mesh, Vector3[] verts, int triCount, float smoothingAngleDeg)
        {
            float cosThreshold = Mathf.Cos(smoothingAngleDeg * Mathf.Deg2Rad);

            // Compute face normals
            var faceNormals = new Vector3[triCount];
            for (int i = 0; i < triCount; i++)
            {
                int idx = i * 3;
                Vector3 e1 = verts[idx + 1] - verts[idx];
                Vector3 e2 = verts[idx + 2] - verts[idx];
                faceNormals[i] = Vector3.Cross(e1, e2).normalized;
            }

            // Group vertices by position using VertexMap
            var vmap = new VertexMap(1e-5f);
            int vertCount = verts.Length;
            var vertGroup = new int[vertCount]; // maps vertex index → group id

            var groups = new Dictionary<int, List<int>>();

            for (int i = 0; i < vertCount; i++)
            {
                int groupId = vmap.GetIndex(verts[i]);
                vertGroup[i] = groupId;

                if (!groups.TryGetValue(groupId, out var list))
                {
                    list = new List<int>();
                    groups[groupId] = list;
                }

                list.Add(i);
            }

            // For each vertex, average face normals of co-located vertices within angle threshold
            var normals = new Vector3[vertCount];

            foreach (var group in groups.Values)
            {
                foreach (int vi in group)
                {
                    Vector3 myNormal = faceNormals[vi / 3];
                    Vector3 smooth = myNormal;

                    foreach (int vj in group)
                    {
                        if (vj / 3 == vi / 3)
                            continue;

                        Vector3 otherNormal = faceNormals[vj / 3];
                        if (Vector3.Dot(myNormal, otherNormal) >= cosThreshold)
                        {
                            smooth += otherNormal;
                        }
                    }

                    normals[vi] = smooth.normalized;
                }
            }

            mesh.normals = normals;
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
