using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh
{
    public class CMonumentBuilder : MonoBehaviour
    {
        private static readonly Color[] Palette =
        {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.cyan, Color.magenta
        };

        [SerializeField] private GameObject _sourceAsset;
        [SerializeField] private bool _drawGizmos = true;

        // ───────── Entry point ─────────

        [ContextMenu("Split Mesh")]
        private void SplitMesh()
        {
            if (_sourceAsset == null)
                return;

            // Collect all source triangles in world space
            var srcVerts = new List<Vector3>();
            var srcTris = new List<int>();

            foreach (var filter in _sourceAsset.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null)
                    continue;

                var mesh = filter.sharedMesh;
                var verts = mesh.vertices;
                var tris = mesh.triangles;
                int baseIdx = srcVerts.Count;

                for (int i = 0; i < verts.Length; i++)
                {
                    srcVerts.Add(filter.transform.TransformPoint(verts[i]));
                }

                for (int i = 0; i < tris.Length; i++)
                {
                    srcTris.Add(tris[i] + baseIdx);
                }
            }

            // Process root parts (direct children of the builder)
            for (int i = 0; i < transform.childCount; i++)
            {
                var part = transform.GetChild(i).GetComponent<CMonumentPart>();
                if (part != null)
                {
                    ProcessPart(part, srcVerts, srcTris);
                }
            }
        }

        // ───────── Recursive tree processing ─────────

        private void ProcessPart(CMonumentPart part, List<Vector3> inputVerts, List<int> inputTris)
        {
            var box = part.GetComponent<BoxCollider>();
            if (box == null)
                return;

            // 1. Clip input to own collider
            var ownPlanes = GetBoxPlanes(box);
            var ownVerts = new List<Vector3>();
            var ownTris = new List<int>();
            ClipTrianglesToBox(inputVerts, inputTris, ownPlanes, ownVerts, ownTris);

            if (ownVerts.Count == 0)
            {
                part.GeneratedMesh = null;
                return;
            }

            // 2. Collect direct child parts
            var children = new List<CMonumentPart>();
            for (int i = 0; i < part.transform.childCount; i++)
            {
                var child = part.transform.GetChild(i).GetComponent<CMonumentPart>();
                if (child != null)
                {
                    children.Add(child);
                }
            }

            // 3. Give each child its portion (clipped from unmodified ownMesh) and recurse
            foreach (var child in children)
            {
                var childBox = child.GetComponent<BoxCollider>();
                if (childBox == null)
                    continue;

                var childPlanes = GetBoxPlanes(childBox);
                var childVerts = new List<Vector3>();
                var childTris = new List<int>();
                ClipTrianglesToBox(ownVerts, ownTris, childPlanes, childVerts, childTris);

                ProcessPart(child, childVerts, childTris);
            }

            // 4. Subtract all children from own mesh
            var resultVerts = ownVerts;
            var resultTris = ownTris;

            foreach (var child in children)
            {
                var childBox = child.GetComponent<BoxCollider>();
                if (childBox == null)
                    continue;

                var childPlanes = GetBoxPlanes(childBox);
                var newVerts = new List<Vector3>();
                var newTris = new List<int>();
                SubtractBox(resultVerts, resultTris, childPlanes, newVerts, newTris);
                resultVerts = newVerts;
                resultTris = newTris;
            }

            // 5. TODO: generate cap faces at cut boundaries

            // 6. Transform to part local space → create mesh
            var localVerts = new Vector3[resultVerts.Count];
            for (int i = 0; i < resultVerts.Count; i++)
            {
                localVerts[i] = part.transform.InverseTransformPoint(resultVerts[i]);
            }

            var resultMesh = new Mesh { name = part.name + "_generated" };
            resultMesh.vertices = localVerts;
            resultMesh.SetTriangles(resultTris, 0);
            resultMesh.RecalculateNormals();
            resultMesh.RecalculateBounds();
            part.GeneratedMesh = resultMesh;
            ApplyMesh(part, resultMesh);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(part);
#endif
        }

        // ───────── Box planes (world space) ─────────

        private static (Vector3[] normals, float[] dists) GetBoxPlanes(BoxCollider box)
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

            return (normals, dists);
        }

        // ───────── Clipping (intersection with box) ─────────

        private static void ClipTrianglesToBox(
            List<Vector3> inVerts, List<int> inTris,
            (Vector3[] normals, float[] dists) planes,
            List<Vector3> outVerts, List<int> outTris)
        {
            var polygon = new List<Vector3>(9);

            for (int t = 0; t < inTris.Count; t += 3)
            {
                polygon.Clear();
                polygon.Add(inVerts[inTris[t]]);
                polygon.Add(inVerts[inTris[t + 1]]);
                polygon.Add(inVerts[inTris[t + 2]]);

                for (int p = 0; p < 6 && polygon.Count >= 3; p++)
                {
                    ClipPolygonByPlane(polygon, planes.normals[p], planes.dists[p]);
                }

                if (polygon.Count < 3)
                    continue;

                EmitPolygon(polygon, outVerts, outTris);
            }
        }

        // ───────── Subtraction (boolean minus box) ─────────

        private static void SubtractBox(
            List<Vector3> inVerts, List<int> inTris,
            (Vector3[] normals, float[] dists) planes,
            List<Vector3> outVerts, List<int> outTris)
        {
            var remainder = new List<Vector3>(9);
            var inside = new List<Vector3>(9);
            var outside = new List<Vector3>(9);

            for (int t = 0; t < inTris.Count; t += 3)
            {
                remainder.Clear();
                remainder.Add(inVerts[inTris[t]]);
                remainder.Add(inVerts[inTris[t + 1]]);
                remainder.Add(inVerts[inTris[t + 2]]);

                for (int p = 0; p < 6; p++)
                {
                    if (remainder.Count < 3)
                        break;

                    SplitPolygonByPlane(remainder,
                        planes.normals[p], planes.dists[p],
                        inside, outside);

                    // Outside part exits through this face → keep
                    if (outside.Count >= 3)
                    {
                        EmitPolygon(outside, outVerts, outTris);
                    }

                    // Inside part might still be inside the box → continue
                    remainder.Clear();
                    remainder.AddRange(inside);
                }

                // Whatever remains is fully inside the box → discard
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

        private static void EmitPolygon(List<Vector3> polygon, List<Vector3> outVerts, List<int> outTris)
        {
            for (int i = 1; i < polygon.Count - 1; i++)
            {
                int baseIdx = outVerts.Count;
                outVerts.Add(polygon[0]);
                outVerts.Add(polygon[i]);
                outVerts.Add(polygon[i + 1]);
                outTris.Add(baseIdx);
                outTris.Add(baseIdx + 1);
                outTris.Add(baseIdx + 2);
            }
        }

        // ───────── Mesh application ─────────

        private static void ApplyMesh(CMonumentPart part, Mesh mesh)
        {
            var filter = part.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = part.gameObject.AddComponent<MeshFilter>();
            }

            filter.sharedMesh = mesh;

            var renderer = part.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = part.gameObject.AddComponent<MeshRenderer>();
            }

            if (renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }
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

            var parts = GetComponentsInChildren<CMonumentPart>();

            for (int p = 0; p < parts.Length; p++)
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
