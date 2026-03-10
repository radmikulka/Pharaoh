using System.Collections.Generic;
using UnityEngine;

namespace CMonumentBuilder
{
    public class VolumeResult
    {
        public List<SliceTriangle> fragments = new List<SliceTriangle>();
        public List<SliceTriangle> caps = new List<SliceTriangle>();
    }

    public static class MeshSlicer
    {
        /// <summary>
        /// Entry point: slices all triangles through the volume hierarchy.
        /// Returns a dictionary mapping each CCutVolume to its geometry result.
        /// Unclaimed triangles (outside all root volumes) are discarded.
        /// </summary>
        public static Dictionary<CCutVolume, VolumeResult> SliceAll(
            List<SliceTriangle> triangles,
            List<CCutVolume> rootVolumes)
        {
            var results = new Dictionary<CCutVolume, VolumeResult>();
            var unclaimed = new List<SliceTriangle>(triangles);

            foreach (var rootVolume in rootVolumes)
            {
                unclaimed = ProcessVolume(unclaimed, rootVolume, results);
            }

            // unclaimed is discarded (geometry outside all volumes)
            return results;
        }

        /// <summary>
        /// Recursively processes a volume: clips triangles to this volume's collider,
        /// subtracts child volumes, generates caps, and assigns remaining fragments.
        /// Returns triangles that are outside this volume (unclaimed).
        /// </summary>
        private static List<SliceTriangle> ProcessVolume(
            List<SliceTriangle> triangles,
            CCutVolume volume,
            Dictionary<CCutVolume, VolumeResult> results)
        {
            Collider collider = volume.GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogError($"CCutVolume '{volume.gameObject.name}' has no Collider component! Add a BoxCollider, SphereCollider, or MeshCollider.");
                return triangles;
            }

            Plane[] planes = MeshSlicerUtils.GetColliderPlanes(collider);
            Debug.Log($"  ProcessVolume '{volume.gameObject.name}': {triangles.Count} input tris, {planes.Length} planes ({collider.GetType().Name})");

            var inside = new List<SliceTriangle>(triangles);
            var outside = new List<SliceTriangle>();
            var allBoundaryPoints = new Dictionary<int, List<Vector3>>();

            // Clip all triangles against each plane of the collider
            for (int p = 0; p < planes.Length; p++)
            {
                Plane plane = planes[p];
                var nextInside = new List<SliceTriangle>();
                var planeBoundary = new List<Vector3>();

                foreach (var tri in inside)
                {
                    var clip = MeshSlicerUtils.ClipTriangleByPlane(tri, plane);
                    nextInside.AddRange(clip.inside);
                    outside.AddRange(clip.outside);
                    planeBoundary.AddRange(clip.boundaryPoints);
                }

                inside = nextInside;
                if (planeBoundary.Count > 0)
                    allBoundaryPoints[p] = planeBoundary;
            }

            Debug.Log($"    After clipping: {inside.Count} inside, {outside.Count} outside, {allBoundaryPoints.Count} boundary planes");

            // Generate caps at this volume's boundary
            var capMaterial = CreateCapMaterial(volume.capColor);
            var caps = new List<SliceTriangle>();

            foreach (var kvp in allBoundaryPoints)
            {
                int planeIdx = kvp.Key;
                List<Vector3> points = kvp.Value;
                Plane plane = planes[planeIdx];

                var loops = MeshSlicerUtils.CollectBoundaryEdges(points, plane);
                foreach (var loop in loops)
                {
                    // Clip cap loop against all OTHER planes of this volume
                    var clipped = loop;
                    for (int otherP = 0; otherP < planes.Length; otherP++)
                    {
                        if (otherP == planeIdx) continue;
                        clipped = MeshSlicerUtils.ClipPolygonByPlane(clipped, planes[otherP]);
                        if (clipped.Count < 3) break;
                    }
                    if (clipped.Count < 3) continue;

                    // plane.normal points inward; -plane.normal points outward to close the piece
                    var capTris = MeshSlicerUtils.TriangulateBoundaryLoop(clipped, -plane.normal, capMaterial);
                    caps.AddRange(capTris);
                }
            }

            // Recurse into child volumes — they carve out from this volume's inside
            var remaining = inside;
            CCutVolume[] children = GetChildVolumes(volume);
            Debug.Log($"    Found {children.Length} child volume(s)");

            foreach (var child in children)
            {
                int beforeCount = remaining.Count;
                remaining = ProcessVolumeSubtract(remaining, child, results, capMaterial);
                Debug.Log($"    After subtracting '{child.gameObject.name}': {beforeCount} → {remaining.Count} remaining");

                // Also subtract child volume from this volume's boundary caps
                // so caps get holes where children punch through
                Collider childCol = child.GetComponent<Collider>();
                if (childCol != null)
                {
                    int capsBefore = caps.Count;
                    caps = SubtractVolumeFromTriangles(caps, childCol);
                    Debug.Log($"    Caps after subtracting '{child.gameObject.name}': {capsBefore} → {caps.Count}");
                }
            }

            // Assign result to this volume
            results[volume] = new VolumeResult
            {
                fragments = remaining,
                caps = caps
            };

            Debug.Log($"    Volume '{volume.gameObject.name}' result: {remaining.Count} fragments, {caps.Count} caps");

            return outside;
        }

        /// <summary>
        /// Processes a child volume: claims the inside for the child, returns the outside
        /// to the parent. Also generates caps at the child boundary facing outward (for parent).
        /// </summary>
        private static List<SliceTriangle> ProcessVolumeSubtract(
            List<SliceTriangle> triangles,
            CCutVolume childVolume,
            Dictionary<CCutVolume, VolumeResult> results,
            Material parentCapMaterial)
        {
            Collider collider = childVolume.GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogWarning($"CCutVolume '{childVolume.gameObject.name}' has no Collider. Skipping.");
                return triangles;
            }

            Plane[] planes = MeshSlicerUtils.GetColliderPlanes(collider);
            Debug.Log($"    ProcessVolumeSubtract '{childVolume.gameObject.name}': {triangles.Count} input tris, {planes.Length} planes");
            for (int dbg = 0; dbg < planes.Length; dbg++)
                Debug.Log($"      Plane[{dbg}]: normal={planes[dbg].normal} dist={planes[dbg].distance}");

            var inside = new List<SliceTriangle>(triangles);
            var outside = new List<SliceTriangle>();
            var allBoundaryPoints = new Dictionary<int, List<Vector3>>();
            var planeDidSplit = new HashSet<int>();

            for (int p = 0; p < planes.Length; p++)
            {
                Plane plane = planes[p];
                var nextInside = new List<SliceTriangle>();
                int beforeOutside = outside.Count;
                var planeBoundary = new List<Vector3>();

                foreach (var tri in inside)
                {
                    var clip = MeshSlicerUtils.ClipTriangleByPlane(tri, plane);
                    nextInside.AddRange(clip.inside);
                    outside.AddRange(clip.outside);
                    planeBoundary.AddRange(clip.boundaryPoints);
                }

                inside = nextInside;

                if (outside.Count > beforeOutside)
                {
                    planeDidSplit.Add(p);
                }

                if (planeBoundary.Count > 0)
                {
                    allBoundaryPoints[p] = planeBoundary;
                }

                Debug.Log($"      Plane[{p}] n={plane.normal:F3}: inside={inside.Count}, outside_total={outside.Count}, split={outside.Count > beforeOutside}, boundary={planeBoundary.Count}");
            }

            // Generate caps only for planes that actually split geometry.
            // Use boundary points when available (mesh-accurate caps), otherwise
            // fall back to GenerateVolumeFacePolygon (for planes where all triangles
            // were entirely on one side — no straddling, so no boundary points).
            var childCapMaterial = CreateCapMaterial(childVolume.capColor);
            var childCaps = new List<SliceTriangle>();
            var parentCaps = new List<SliceTriangle>();

            foreach (int planeIdx in planeDidSplit)
            {
                Plane plane = planes[planeIdx];

                if (allBoundaryPoints.TryGetValue(planeIdx, out var points) && points.Count > 0)
                {
                    // Boundary-based caps: accurate to the actual mesh intersection
                    var loops = MeshSlicerUtils.CollectBoundaryEdges(points, plane);
                    foreach (var loop in loops)
                    {
                        var clipped = loop;
                        for (int otherP = 0; otherP < planes.Length; otherP++)
                        {
                            if (otherP == planeIdx)
                            {
                                continue;
                            }
                            clipped = MeshSlicerUtils.ClipPolygonByPlane(clipped, planes[otherP]);
                            if (clipped.Count < 3)
                            {
                                break;
                            }
                        }
                        if (clipped.Count < 3)
                        {
                            continue;
                        }

                        childCaps.AddRange(MeshSlicerUtils.TriangulateBoundaryLoop(clipped, -plane.normal, childCapMaterial));
                        parentCaps.AddRange(MeshSlicerUtils.TriangulateBoundaryLoop(clipped, plane.normal, parentCapMaterial));
                    }
                }
                else
                {
                    // Fallback: no boundary points (triangles were entirely outside this plane,
                    // not straddling it). Use the full volume face clipped to the box.
                    var face = GenerateVolumeFacePolygon(planes, planeIdx);
                    if (face.Count < 3)
                    {
                        continue;
                    }

                    childCaps.AddRange(MeshSlicerUtils.TriangulateBoundaryLoop(face, -plane.normal, childCapMaterial));
                    parentCaps.AddRange(MeshSlicerUtils.TriangulateBoundaryLoop(face, plane.normal, parentCapMaterial));
                }
            }

            // Recurse into child's children
            var childRemaining = inside;
            CCutVolume[] grandchildren = GetChildVolumes(childVolume);
            foreach (var grandchild in grandchildren)
            {
                childRemaining = ProcessVolumeSubtract(childRemaining, grandchild, results, childCapMaterial);

                // Subtract grandchild from child's caps
                Collider gcCol = grandchild.GetComponent<Collider>();
                if (gcCol != null)
                    childCaps = SubtractVolumeFromTriangles(childCaps, gcCol);
            }

            // Log bounds of child fragments for debugging
            if (childRemaining.Count > 0)
            {
                Vector3 bmin = childRemaining[0].v0, bmax = childRemaining[0].v0;
                foreach (var tri in childRemaining)
                {
                    bmin = Vector3.Min(bmin, Vector3.Min(tri.v0, Vector3.Min(tri.v1, tri.v2)));
                    bmax = Vector3.Max(bmax, Vector3.Max(tri.v0, Vector3.Max(tri.v1, tri.v2)));
                }
                Debug.Log($"    Child '{childVolume.gameObject.name}' fragment bounds: min={bmin} max={bmax}");
                if (collider is BoxCollider box)
                {
                    Vector3 worldCenter = collider.transform.TransformPoint(box.center);
                    Vector3 s = collider.transform.lossyScale;
                    Vector3 he = Vector3.Scale(box.size * 0.5f, new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z)));
                    Debug.Log($"    Expected bounds: center={worldCenter} half-extent={he} → min={worldCenter - he} max={worldCenter + he}");
                }
            }

            // Assign child volume result
            results[childVolume] = new VolumeResult
            {
                fragments = childRemaining,
                caps = childCaps
            };

            // Parent-facing caps become part of parent's fragments
            outside.AddRange(parentCaps);

            return outside;
        }

        /// <summary>
        /// Generates a face polygon for one plane of a convex volume by creating a large
        /// quad on the plane and clipping it against all other planes.
        /// </summary>
        private static List<Vector3> GenerateVolumeFacePolygon(Plane[] planes, int planeIndex)
        {
            Plane plane = planes[planeIndex];
            Vector3 normal = plane.normal;
            // Point on plane closest to origin: -normal * distance
            Vector3 pointOnPlane = -normal * plane.distance;

            Vector3 up = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.99f ? Vector3.up : Vector3.right;
            Vector3 tangent = Vector3.Cross(normal, up).normalized;
            Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

            float size = 100f;
            var polygon = new List<Vector3>
            {
                pointOnPlane + tangent * size + bitangent * size,
                pointOnPlane - tangent * size + bitangent * size,
                pointOnPlane - tangent * size - bitangent * size,
                pointOnPlane + tangent * size - bitangent * size
            };

            // Clip against all other planes to get the exact face of the convex volume
            for (int j = 0; j < planes.Length; j++)
            {
                if (j == planeIndex) continue;
                polygon = MeshSlicerUtils.ClipPolygonByPlane(polygon, planes[j]);
                if (polygon.Count < 3) break;
            }

            return polygon;
        }

        /// <summary>
        /// Subtracts a collider's volume from a list of triangles.
        /// Returns only the triangles (or fragments) that are OUTSIDE the collider.
        /// Used to punch holes in caps where child volumes overlap.
        /// </summary>
        private static List<SliceTriangle> SubtractVolumeFromTriangles(List<SliceTriangle> triangles, Collider collider)
        {
            Plane[] planes = MeshSlicerUtils.GetColliderPlanes(collider);

            // Sequential clip: "inside" candidates shrink each step,
            // "outside" accumulates fragments definitely outside the volume.
            var insideCandidate = new List<SliceTriangle>(triangles);
            var outsideAccumulated = new List<SliceTriangle>();

            foreach (var plane in planes)
            {
                var nextInside = new List<SliceTriangle>();
                foreach (var tri in insideCandidate)
                {
                    var clip = MeshSlicerUtils.ClipTriangleByPlane(tri, plane);
                    nextInside.AddRange(clip.inside);
                    outsideAccumulated.AddRange(clip.outside);
                }
                insideCandidate = nextInside;
            }

            // insideCandidate = triangles inside the collider → discarded
            // outsideAccumulated = triangles outside the collider → kept
            return outsideAccumulated;
        }

        /// <summary>
        /// Gets direct child CCutVolume components.
        /// </summary>
        private static CCutVolume[] GetChildVolumes(CCutVolume parent)
        {
            var children = new List<CCutVolume>();
            foreach (Transform child in parent.transform)
            {
                var vol = child.GetComponent<CCutVolume>();
                if (vol != null)
                    children.Add(vol);
            }
            return children.ToArray();
        }

        /// <summary>
        /// Creates a simple unlit material with the given color for cap faces.
        /// </summary>
        private static Material CreateCapMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                Debug.LogError("CMonumentBuilder: Cannot find any shader for cap material. Caps will use error shader.");
                shader = Shader.Find("Hidden/InternalErrorShader");
            }

            var mat = new Material(shader);
            mat.color = color;
            mat.SetColor("_BaseColor", color); // URP uses _BaseColor
            mat.name = $"Cap_{ColorUtility.ToHtmlStringRGB(color)}";
            return mat;
        }

        /// <summary>
        /// Builds a Unity Mesh and Material[] from a VolumeResult.
        /// Groups fragments by material into submeshes, plus one submesh for caps.
        /// </summary>
        public static (Mesh mesh, Material[] materials) BuildMesh(VolumeResult result)
        {
            // Group all triangles (fragments + caps) by material
            var allTriangles = new List<SliceTriangle>(result.fragments);
            allTriangles.AddRange(result.caps);

            var groups = new Dictionary<Material, List<SliceTriangle>>();
            foreach (var tri in allTriangles)
            {
                Material key = tri.material;
                if (!groups.ContainsKey(key))
                    groups[key] = new List<SliceTriangle>();
                groups[key].Add(tri);
            }

            if (groups.Count == 0)
                return (null, null);

            // Build combined mesh
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var submeshIndices = new List<int[]>();
            var materials = new List<Material>();

            foreach (var kvp in groups)
            {
                Material mat = kvp.Key;
                List<SliceTriangle> tris = kvp.Value;
                materials.Add(mat);

                var indices = new List<int>();
                foreach (var tri in tris)
                {
                    int baseIdx = vertices.Count;

                    vertices.Add(tri.v0); vertices.Add(tri.v1); vertices.Add(tri.v2);
                    normals.Add(tri.n0); normals.Add(tri.n1); normals.Add(tri.n2);
                    uvs.Add(tri.uv0); uvs.Add(tri.uv1); uvs.Add(tri.uv2);

                    indices.Add(baseIdx);
                    indices.Add(baseIdx + 1);
                    indices.Add(baseIdx + 2);
                }

                submeshIndices.Add(indices.ToArray());
            }

            Mesh mesh = new Mesh();
            if (vertices.Count > 65535)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = submeshIndices.Count;

            for (int i = 0; i < submeshIndices.Count; i++)
            {
                mesh.SetTriangles(submeshIndices[i], i);
            }

            mesh.RecalculateBounds();

            return (mesh, materials.ToArray());
        }
    }
}
