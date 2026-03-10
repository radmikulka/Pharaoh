using System.Collections.Generic;
using UnityEngine;

namespace CMonumentBuilder
{
    public struct SliceVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public static SliceVertex Lerp(SliceVertex a, SliceVertex b, float t)
        {
            return new SliceVertex
            {
                position = Vector3.Lerp(a.position, b.position, t),
                normal = Vector3.Lerp(a.normal, b.normal, t).normalized,
                uv = Vector2.Lerp(a.uv, b.uv, t)
            };
        }
    }

    public struct SliceTriangle
    {
        public Vector3 v0, v1, v2;
        public Vector3 n0, n1, n2;
        public Vector2 uv0, uv1, uv2;
        public Material material;

        public SliceVertex GetVertex(int index)
        {
            switch (index)
            {
                case 0: return new SliceVertex { position = v0, normal = n0, uv = uv0 };
                case 1: return new SliceVertex { position = v1, normal = n1, uv = uv1 };
                case 2: return new SliceVertex { position = v2, normal = n2, uv = uv2 };
                default: throw new System.ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static SliceTriangle FromVertices(SliceVertex a, SliceVertex b, SliceVertex c, Material mat)
        {
            return new SliceTriangle
            {
                v0 = a.position, v1 = b.position, v2 = c.position,
                n0 = a.normal, n1 = b.normal, n2 = c.normal,
                uv0 = a.uv, uv1 = b.uv, uv2 = c.uv,
                material = mat
            };
        }

        public List<SliceVertex> ToVertexList()
        {
            return new List<SliceVertex> { GetVertex(0), GetVertex(1), GetVertex(2) };
        }
    }

    public struct ClipResult
    {
        public List<SliceTriangle> inside;
        public List<SliceTriangle> outside;
        public List<Vector3> boundaryPoints;
    }

    public static class MeshSlicerUtils
    {
        private const float kEpsilon = 1e-6f;

        /// <summary>
        /// Clips a polygon (list of 3D points) against a plane, keeping only the inside part.
        /// "Inside" = positive side of the plane (where plane.GetDistanceToPoint > 0).
        /// </summary>
        public static List<Vector3> ClipPolygonByPlane(List<Vector3> polygon, Plane plane)
        {
            if (polygon.Count < 3) return new List<Vector3>();

            var distances = new float[polygon.Count];
            int positiveCount = 0, negativeCount = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                distances[i] = plane.GetDistanceToPoint(polygon[i]);
                if (distances[i] > kEpsilon) positiveCount++;
                else if (distances[i] < -kEpsilon) negativeCount++;
            }

            if (negativeCount == 0) return new List<Vector3>(polygon);
            if (positiveCount == 0) return new List<Vector3>();

            var result = new List<Vector3>();
            for (int i = 0; i < polygon.Count; i++)
            {
                int j = (i + 1) % polygon.Count;
                float dI = distances[i];
                float dJ = distances[j];
                bool iInside = dI >= -kEpsilon;
                bool jInside = dJ >= -kEpsilon;

                if (iInside) result.Add(polygon[i]);

                if (iInside != jInside)
                {
                    float t = dI / (dI - dJ);
                    t = Mathf.Clamp01(t);
                    result.Add(Vector3.Lerp(polygon[i], polygon[j], t));
                }
            }

            return result;
        }

        /// <summary>
        /// Clips a convex polygon against a plane using Sutherland-Hodgman.
        /// Returns (insidePolygon, outsidePolygon) with interpolated attributes.
        /// "Inside" = positive side of the plane (where plane.GetDistanceToPoint > 0).
        /// </summary>
        public static (List<SliceVertex> inside, List<SliceVertex> outside) SutherlandHodgmanClip(
            List<SliceVertex> polygon, Plane plane)
        {
            var inside = new List<SliceVertex>();
            var outside = new List<SliceVertex>();

            if (polygon.Count < 3)
                return (inside, outside);

            // Classify all vertices
            var distances = new float[polygon.Count];
            int positiveCount = 0, negativeCount = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                distances[i] = plane.GetDistanceToPoint(polygon[i].position);
                if (distances[i] > kEpsilon) positiveCount++;
                else if (distances[i] < -kEpsilon) negativeCount++;
            }

            // All on one side
            if (negativeCount == 0)
            {
                inside.AddRange(polygon);
                return (inside, outside);
            }
            if (positiveCount == 0)
            {
                outside.AddRange(polygon);
                return (inside, outside);
            }

            // Clip — build both inside and outside polygons
            for (int i = 0; i < polygon.Count; i++)
            {
                int j = (i + 1) % polygon.Count;
                SliceVertex current = polygon[i];
                SliceVertex next = polygon[j];
                float dCurrent = distances[i];
                float dNext = distances[j];

                bool currentInside = dCurrent >= -kEpsilon;
                bool nextInside = dNext >= -kEpsilon;

                if (currentInside)
                {
                    inside.Add(current);
                    if (!nextInside)
                    {
                        // Crossing from inside to outside
                        SliceVertex intersection = IntersectEdge(current, next, dCurrent, dNext);
                        inside.Add(intersection);
                        outside.Add(intersection);
                    }
                }
                else
                {
                    outside.Add(current);
                    if (nextInside)
                    {
                        // Crossing from outside to inside
                        SliceVertex intersection = IntersectEdge(current, next, dCurrent, dNext);
                        outside.Add(intersection);
                        inside.Add(intersection);
                    }
                }
            }

            return (inside, outside);
        }

        /// <summary>
        /// Returns interpolated SliceVertex at the plane intersection of edge a→b.
        /// </summary>
        public static SliceVertex IntersectEdge(SliceVertex a, SliceVertex b, float distA, float distB)
        {
            float t = distA / (distA - distB);
            t = Mathf.Clamp01(t);
            return SliceVertex.Lerp(a, b, t);
        }

        /// <summary>
        /// Fan triangulation from first vertex for convex polygons.
        /// </summary>
        public static List<SliceTriangle> TriangulateConvexPolygon(List<SliceVertex> polygon, Material material)
        {
            var triangles = new List<SliceTriangle>();
            if (polygon.Count < 3) return triangles;

            for (int i = 1; i < polygon.Count - 1; i++)
            {
                triangles.Add(SliceTriangle.FromVertices(polygon[0], polygon[i], polygon[i + 1], material));
            }
            return triangles;
        }

        /// <summary>
        /// Clips a single triangle against a plane. Returns inside/outside triangle lists
        /// and boundary points on the cut plane.
        /// </summary>
        public static ClipResult ClipTriangleByPlane(SliceTriangle tri, Plane plane)
        {
            var polygon = tri.ToVertexList();
            var (inside, outside) = SutherlandHodgmanClip(polygon, plane);

            var result = new ClipResult
            {
                inside = TriangulateConvexPolygon(inside, tri.material),
                outside = TriangulateConvexPolygon(outside, tri.material),
                boundaryPoints = new List<Vector3>()
            };

            // Collect boundary points: vertices that lie on the plane
            CollectPlanePoints(inside, plane, result.boundaryPoints);

            return result;
        }

        private static void CollectPlanePoints(List<SliceVertex> polygon, Plane plane, List<Vector3> points)
        {
            foreach (var v in polygon)
            {
                float d = Mathf.Abs(plane.GetDistanceToPoint(v.position));
                if (d < kEpsilon)
                {
                    points.Add(v.position);
                }
            }
        }

        /// <summary>
        /// Generates tangent planes for a 32-plane icosphere approximation of a sphere.
        /// Uses icosahedron vertices (12) + face centers (20) = 32 directions.
        /// </summary>
        public static Plane[] GenerateIcospherePlanes(Vector3 center, float radius)
        {
            var normals = new List<Vector3>();

            // Golden ratio
            float phi = (1f + Mathf.Sqrt(5f)) / 2f;
            float invLen = 1f / Mathf.Sqrt(1f + phi * phi);

            // 12 icosahedron vertices (normalized)
            var icoVerts = new Vector3[]
            {
                new Vector3(-1, phi, 0) * invLen,
                new Vector3( 1, phi, 0) * invLen,
                new Vector3(-1,-phi, 0) * invLen,
                new Vector3( 1,-phi, 0) * invLen,
                new Vector3(0,-1, phi) * invLen,
                new Vector3(0, 1, phi) * invLen,
                new Vector3(0,-1,-phi) * invLen,
                new Vector3(0, 1,-phi) * invLen,
                new Vector3( phi, 0,-1) * invLen,
                new Vector3( phi, 0, 1) * invLen,
                new Vector3(-phi, 0,-1) * invLen,
                new Vector3(-phi, 0, 1) * invLen,
            };

            // 20 icosahedron faces (vertex indices)
            int[,] icoFaces = new int[,]
            {
                {0,11,5}, {0,5,1}, {0,1,7}, {0,7,10}, {0,10,11},
                {1,5,9}, {5,11,4}, {11,10,2}, {10,7,6}, {7,1,8},
                {3,9,4}, {3,4,2}, {3,2,6}, {3,6,8}, {3,8,9},
                {4,9,5}, {2,4,11}, {6,2,10}, {8,6,7}, {9,8,1},
            };

            // Add 12 vertex normals
            foreach (var v in icoVerts)
            {
                normals.Add(v.normalized);
            }

            // Add 20 face center normals
            for (int i = 0; i < 20; i++)
            {
                Vector3 faceCenter = (icoVerts[icoFaces[i, 0]] + icoVerts[icoFaces[i, 1]] + icoVerts[icoFaces[i, 2]]) / 3f;
                normals.Add(faceCenter.normalized);
            }

            // Build planes — normals point inward so "inside sphere" = positive half-space
            var planes = new Plane[normals.Count];
            for (int i = 0; i < normals.Count; i++)
            {
                Vector3 n = normals[i];
                Vector3 pointOnSurface = center + n * radius;
                planes[i] = new Plane(-n, pointOnSurface);
            }

            return planes;
        }

        /// <summary>
        /// Returns clipping planes for a collider in world space.
        /// </summary>
        public static Plane[] GetColliderPlanes(Collider collider)
        {
            if (collider is BoxCollider box)
                return GetBoxPlanes(box);
            if (collider is SphereCollider sphere)
                return GetSpherePlanes(sphere);
            if (collider is MeshCollider meshCol)
                return GetMeshColliderPlanes(meshCol);

            Debug.LogWarning($"Unsupported collider type: {collider.GetType().Name}");
            return new Plane[0];
        }

        private static Plane[] GetBoxPlanes(BoxCollider box)
        {
            Transform t = box.transform;
            Vector3 center = t.TransformPoint(box.center);
            Vector3 halfSize = box.size * 0.5f;

            // Local axes in world space
            Vector3 right = t.right;
            Vector3 up = t.up;
            Vector3 forward = t.forward;

            // Account for lossy scale
            Vector3 scale = t.lossyScale;
            float sx = halfSize.x * Mathf.Abs(scale.x);
            float sy = halfSize.y * Mathf.Abs(scale.y);
            float sz = halfSize.z * Mathf.Abs(scale.z);

            // Normals point inward so that "inside the box" = positive half-space
            return new Plane[]
            {
                new Plane(-right,   center + right * sx),
                new Plane( right,   center - right * sx),
                new Plane(-up,      center + up * sy),
                new Plane( up,      center - up * sy),
                new Plane(-forward, center + forward * sz),
                new Plane( forward, center - forward * sz),
            };
        }

        private static Plane[] GetSpherePlanes(SphereCollider sphere)
        {
            Transform t = sphere.transform;
            Vector3 center = t.TransformPoint(sphere.center);
            // Use max scale axis for radius
            Vector3 scale = t.lossyScale;
            float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            float worldRadius = sphere.radius * maxScale;

            return GenerateIcospherePlanes(center, worldRadius);
        }

        private static Plane[] GetMeshColliderPlanes(MeshCollider meshCol)
        {
            Mesh mesh = meshCol.sharedMesh;
            if (mesh == null)
            {
                Debug.LogWarning("MeshCollider has no sharedMesh assigned.");
                return new Plane[0];
            }

            Transform t = meshCol.transform;
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;
            int triCount = tris.Length / 3;

            var planes = new Plane[triCount];
            for (int i = 0; i < triCount; i++)
            {
                Vector3 a = t.TransformPoint(verts[tris[i * 3]]);
                Vector3 b = t.TransformPoint(verts[tris[i * 3 + 1]]);
                Vector3 c = t.TransformPoint(verts[tris[i * 3 + 2]]);

                Vector3 cross = Vector3.Cross(b - a, c - a);
                if (cross.sqrMagnitude < 1e-10f)
                {
                    // Degenerate triangle — use a plane that keeps everything inside
                    planes[i] = new Plane(Vector3.up, new Vector3(0, 999999f, 0));
                    continue;
                }
                Vector3 normal = cross.normalized;
                // Negate so "inside the mesh" = positive half-space
                planes[i] = new Plane(-normal, a);
            }
            return planes;
        }

        /// <summary>
        /// Tests if a point is inside a collider using ClosestPoint.
        /// </summary>
        public static bool PointInCollider(Vector3 point, Collider collider)
        {
            Vector3 closest = collider.ClosestPoint(point);
            return (closest - point).sqrMagnitude < kEpsilon * kEpsilon;
        }

        /// <summary>
        /// Collects boundary edge segments from intersection points on a cut plane,
        /// then orders them into connected loops.
        /// </summary>
        public static List<List<Vector3>> CollectBoundaryEdges(List<Vector3> boundaryPoints, Plane cutPlane, float epsilon = 0.001f)
        {
            if (boundaryPoints.Count < 2)
                return new List<List<Vector3>>();

            // Deduplicate points
            var unique = new List<Vector3>();
            foreach (var p in boundaryPoints)
            {
                bool found = false;
                foreach (var u in unique)
                {
                    if ((u - p).sqrMagnitude < epsilon * epsilon)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) unique.Add(p);
            }

            if (unique.Count < 3)
                return new List<List<Vector3>>();

            // Project onto 2D for ordering (use plane's local coordinate system)
            Vector3 planeNormal = cutPlane.normal;
            Vector3 tangent, bitangent;
            ComputePlaneBasis(planeNormal, out tangent, out bitangent);

            Vector3 centroid = Vector3.zero;
            foreach (var p in unique) centroid += p;
            centroid /= unique.Count;

            // Sort by angle around centroid in the plane
            unique.Sort((a, b) =>
            {
                Vector3 da = a - centroid;
                Vector3 db = b - centroid;
                float angleA = Mathf.Atan2(Vector3.Dot(da, bitangent), Vector3.Dot(da, tangent));
                float angleB = Mathf.Atan2(Vector3.Dot(db, bitangent), Vector3.Dot(db, tangent));
                return angleA.CompareTo(angleB);
            });

            var loops = new List<List<Vector3>>();
            loops.Add(unique);
            return loops;
        }

        private static void ComputePlaneBasis(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
        {
            Vector3 up = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.99f ? Vector3.up : Vector3.right;
            tangent = Vector3.Cross(normal, up).normalized;
            bitangent = Vector3.Cross(normal, tangent).normalized;
        }

        /// <summary>
        /// Ear-clipping triangulation for a boundary loop. Normal determines winding.
        /// </summary>
        public static List<SliceTriangle> TriangulateBoundaryLoop(List<Vector3> loop, Vector3 normal, Material capMaterial)
        {
            var result = new List<SliceTriangle>();
            if (loop.Count < 3) return result;

            // Project to 2D for ear clipping
            Vector3 tangent, bitangent;
            ComputePlaneBasis(normal, out tangent, out bitangent);

            var points2D = new List<Vector2>();
            foreach (var p in loop)
            {
                points2D.Add(new Vector2(Vector3.Dot(p, tangent), Vector3.Dot(p, bitangent)));
            }

            // Ensure correct winding (counter-clockwise when viewed from normal direction)
            float signedArea = 0;
            for (int i = 0; i < points2D.Count; i++)
            {
                int j = (i + 1) % points2D.Count;
                signedArea += (points2D[j].x - points2D[i].x) * (points2D[j].y + points2D[i].y);
            }
            bool needReverse = signedArea > 0;

            var indices = new List<int>();
            for (int i = 0; i < loop.Count; i++) indices.Add(i);
            if (needReverse) indices.Reverse();

            // Ear clipping
            int safety = indices.Count * indices.Count;
            while (indices.Count > 2 && safety-- > 0)
            {
                bool earFound = false;
                for (int i = 0; i < indices.Count; i++)
                {
                    int prev = (i - 1 + indices.Count) % indices.Count;
                    int next = (i + 1) % indices.Count;

                    int iPrev = indices[prev];
                    int iCurr = indices[i];
                    int iNext = indices[next];

                    Vector2 a = points2D[iPrev];
                    Vector2 b = points2D[iCurr];
                    Vector2 c = points2D[iNext];

                    // Check if ear (convex)
                    float cross = Cross2D(b - a, c - a);
                    if (cross <= 0) continue; // reflex

                    // Check no other vertex inside this triangle
                    bool containsOther = false;
                    for (int k = 0; k < indices.Count; k++)
                    {
                        if (k == prev || k == i || k == next) continue;
                        if (PointInTriangle2D(points2D[indices[k]], a, b, c))
                        {
                            containsOther = true;
                            break;
                        }
                    }

                    if (!containsOther)
                    {
                        // Emit triangle
                        result.Add(new SliceTriangle
                        {
                            v0 = loop[iPrev], v1 = loop[iCurr], v2 = loop[iNext],
                            n0 = normal, n1 = normal, n2 = normal,
                            uv0 = new Vector2(points2D[iPrev].x, points2D[iPrev].y),
                            uv1 = new Vector2(points2D[iCurr].x, points2D[iCurr].y),
                            uv2 = new Vector2(points2D[iNext].x, points2D[iNext].y),
                            material = capMaterial
                        });

                        indices.RemoveAt(i);
                        earFound = true;
                        break;
                    }
                }

                if (!earFound) break; // no more ears (degenerate polygon)
            }

            return result;
        }

        private static float Cross2D(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private static bool PointInTriangle2D(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross2D(b - a, p - a);
            float d2 = Cross2D(c - b, p - b);
            float d3 = Cross2D(a - c, p - c);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        /// <summary>
        /// Extracts SliceTriangles from a MeshFilter, transforming to world space.
        /// </summary>
        public static List<SliceTriangle> ExtractTriangles(MeshFilter mf, MeshRenderer mr)
        {
            var result = new List<SliceTriangle>();
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) return result;

            if (!mesh.isReadable)
            {
                Debug.LogError($"Mesh '{mesh.name}' is not readable. Enable Read/Write in import settings.");
                return result;
            }

            Transform t = mf.transform;
            Vector3[] verts = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uvs = mesh.uv;
            Material[] mats = mr != null ? mr.sharedMaterials : new Material[0];

            // Ensure arrays exist
            if (normals == null || normals.Length != verts.Length)
            {
                mesh.RecalculateNormals();
                normals = mesh.normals;
            }
            if (uvs == null || uvs.Length != verts.Length)
                uvs = new Vector2[verts.Length];

            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                int[] tris = mesh.GetTriangles(sub);
                Material mat = (sub < mats.Length) ? mats[sub] : null;

                for (int i = 0; i < tris.Length; i += 3)
                {
                    int i0 = tris[i], i1 = tris[i + 1], i2 = tris[i + 2];

                    result.Add(new SliceTriangle
                    {
                        v0 = t.TransformPoint(verts[i0]),
                        v1 = t.TransformPoint(verts[i1]),
                        v2 = t.TransformPoint(verts[i2]),
                        n0 = t.TransformDirection(normals[i0]).normalized,
                        n1 = t.TransformDirection(normals[i1]).normalized,
                        n2 = t.TransformDirection(normals[i2]).normalized,
                        uv0 = uvs[i0],
                        uv1 = uvs[i1],
                        uv2 = uvs[i2],
                        material = mat
                    });
                }
            }

            return result;
        }
    }
}
