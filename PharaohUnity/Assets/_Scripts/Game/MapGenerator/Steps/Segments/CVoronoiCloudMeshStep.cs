using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    public class CVoronoiCloudMeshStep : CMapGenerationStepBase
    {
        private const string RaycastLayerName = "RaycastTarget";

        [SerializeField] private float _cloudHeight = 2f;
        [SerializeField] private Material _cloudMaterial;

        public override string StepName => "Voronoi Cloud Mesh";
        public override string StepDescription =>
            "Generates clickable cloud meshes covering each Voronoi region. Click = reveal.";

        public override void Execute(CMapData mapData, int seed)
        {
            // 2a. Destroy existing cloud children
            foreach (var existing in GetComponentsInChildren<CVoronoiCloudRegion>())
                DestroyImmediate(existing.gameObject);

            // 2b. Group tiles by VoronoiRegionId
            var tilesByRegion = new Dictionary<int, List<Vector2Int>>();
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    int rid = mapData.Get(x, y).VoronoiRegionId;
                    if (!tilesByRegion.TryGetValue(rid, out var list))
                        tilesByRegion[rid] = list = new List<Vector2Int>();
                    list.Add(new Vector2Int(x, y));
                }
            }

            foreach (var kvp in tilesByRegion)
            {
                int regionId = kvp.Key;
                var tiles = kvp.Value;

                // 2c. Collect directed boundary edges (region interior always on the left)
                var directedEdges = CollectDirectedEdges(regionId, tiles, mapData);

                // 2d. Trace directed edges into closed polygon loops
                var polygons = TracePolygons(directedEdges);

                // 2e + 2f. Triangulate each loop and build mesh + GameObject
                for (int compIdx = 0; compIdx < polygons.Count; compIdx++)
                {
                    var polygon = polygons[compIdx];
                    if (polygon.Count < 3) continue;

                    int[] triangles = TriangulateEarClipping(polygon);
                    if (triangles == null || triangles.Length < 3) continue;

                    var vertices3D = new Vector3[polygon.Count];
                    for (int i = 0; i < polygon.Count; i++)
                        vertices3D[i] = new Vector3(polygon[i].x, _cloudHeight, polygon[i].y);

                    var mesh = new Mesh();
                    mesh.name = $"CloudRegion_{regionId}_{compIdx}";
                    mesh.vertices = vertices3D;
                    mesh.triangles = triangles;
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();

                    string goName = polygons.Count > 1
                        ? $"CloudRegion_{regionId}_{compIdx}"
                        : $"CloudRegion_{regionId}";

                    var go = new GameObject(goName);
                    go.transform.SetParent(transform, worldPositionStays: false);
                    go.layer = LayerMask.NameToLayer(RaycastLayerName);

                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    go.AddComponent<MeshRenderer>().sharedMaterial = _cloudMaterial;

                    var col = go.AddComponent<MeshCollider>();
                    col.sharedMesh = mesh;
                    col.convex = false;

                    var region = go.AddComponent<CVoronoiCloudRegion>();
                    region.RegionId = regionId;
                }
            }
        }

        // --- Edge collection ---

        // Directed edges with the region interior always on the left side.
        // Right boundary  → edge goes north (+Z): (x+0.5, y-0.5) → (x+0.5, y+0.5)
        // Left  boundary  → edge goes south (-Z): (x-0.5, y+0.5) → (x-0.5, y-0.5)
        // Top   boundary  → edge goes west  (-X): (x+0.5, y+0.5) → (x-0.5, y+0.5)
        // Bottom boundary → edge goes east  (+X): (x-0.5, y-0.5) → (x+0.5, y-0.5)
        private static List<(Vector2 from, Vector2 to)> CollectDirectedEdges(
            int regionId, List<Vector2Int> tiles, CMapData mapData)
        {
            var edges = new List<(Vector2, Vector2)>();
            foreach (var tile in tiles)
            {
                int tx = tile.x, ty = tile.y;
                float fx = tx, fy = ty;

                if (!mapData.IsValid(tx + 1, ty) || mapData.Get(tx + 1, ty).VoronoiRegionId != regionId)
                    edges.Add((new Vector2(fx + 0.5f, fy - 0.5f), new Vector2(fx + 0.5f, fy + 0.5f)));

                if (!mapData.IsValid(tx - 1, ty) || mapData.Get(tx - 1, ty).VoronoiRegionId != regionId)
                    edges.Add((new Vector2(fx - 0.5f, fy + 0.5f), new Vector2(fx - 0.5f, fy - 0.5f)));

                if (!mapData.IsValid(tx, ty + 1) || mapData.Get(tx, ty + 1).VoronoiRegionId != regionId)
                    edges.Add((new Vector2(fx + 0.5f, fy + 0.5f), new Vector2(fx - 0.5f, fy + 0.5f)));

                if (!mapData.IsValid(tx, ty - 1) || mapData.Get(tx, ty - 1).VoronoiRegionId != regionId)
                    edges.Add((new Vector2(fx - 0.5f, fy - 0.5f), new Vector2(fx + 0.5f, fy - 0.5f)));
            }
            return edges;
        }

        // --- Polygon tracing ---

        // Follows directed edge chains into closed loops.
        // Handles X-junctions (4 edges at one vertex) by tracing separate loops.
        private static List<List<Vector2>> TracePolygons(List<(Vector2 from, Vector2 to)> directedEdges)
        {
            var adjacency = new Dictionary<Vector2, List<Vector2>>();
            foreach (var (from, to) in directedEdges)
            {
                if (!adjacency.TryGetValue(from, out var list))
                    adjacency[from] = list = new List<Vector2>();
                list.Add(to);
            }

            var visited = new HashSet<(Vector2, Vector2)>();
            var polygons = new List<List<Vector2>>();

            foreach (var startVertex in new List<Vector2>(adjacency.Keys))
            {
                foreach (var startTo in adjacency[startVertex])
                {
                    if (visited.Contains((startVertex, startTo))) continue;

                    var polygon = new List<Vector2>();
                    Vector2 current = startVertex;
                    Vector2 to = startTo;

                    while (!visited.Contains((current, to)))
                    {
                        visited.Add((current, to));
                        polygon.Add(current);
                        current = to;

                        if (!adjacency.TryGetValue(current, out var neighbors)) break;

                        bool found = false;
                        to = default;
                        foreach (var n in neighbors)
                        {
                            if (!visited.Contains((current, n)))
                            {
                                to = n;
                                found = true;
                                break;
                            }
                        }
                        if (!found) break;
                    }

                    if (polygon.Count >= 3)
                        polygons.Add(polygon);
                }
            }

            return polygons;
        }

        // --- Triangulation ---

        // Ear-clipping triangulation. Polygon must have at least 3 vertices.
        // Returns triangle indices (groups of 3) into the polygon vertex list.
        private static int[] TriangulateEarClipping(List<Vector2> polygon)
        {
            int n = polygon.Count;
            if (n < 3) return null;
            if (n == 3) return new[] { 0, 1, 2 };

            // Ensure CCW winding (signed area > 0)
            if (ComputeSignedArea(polygon) < 0f)
                polygon.Reverse();

            var idx = new List<int>(n);
            for (int i = 0; i < n; i++) idx.Add(i);

            var triangles = new List<int>();
            int maxIter = n * n + n;

            for (int iter = 0; idx.Count > 3 && iter < maxIter; iter++)
            {
                int count = idx.Count;
                bool clipped = false;

                for (int i = 0; i < count; i++)
                {
                    int iPrev = (i - 1 + count) % count;
                    int iNext = (i + 1) % count;

                    Vector2 a = polygon[idx[iPrev]];
                    Vector2 b = polygon[idx[i]];
                    Vector2 c = polygon[idx[iNext]];

                    if (!IsEar(a, b, c, polygon, idx, iPrev, i, iNext)) continue;

                    triangles.Add(idx[iPrev]);
                    triangles.Add(idx[i]);
                    triangles.Add(idx[iNext]);
                    idx.RemoveAt(i);
                    clipped = true;
                    break;
                }

                if (!clipped) break;
            }

            if (idx.Count >= 3)
            {
                triangles.Add(idx[0]);
                triangles.Add(idx[1]);
                triangles.Add(idx[2]);
            }

            return triangles.ToArray();
        }

        private static float ComputeSignedArea(List<Vector2> polygon)
        {
            float sum = 0f;
            int n = polygon.Count;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[(i + 1) % n];
                sum += a.x * b.y - b.x * a.y;
            }
            return sum * 0.5f;
        }

        private static bool IsEar(Vector2 a, Vector2 b, Vector2 c,
            List<Vector2> polygon, List<int> idx, int iPrev, int iCur, int iNext)
        {
            // b must be a convex (left) turn for a CCW polygon
            float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
            if (cross <= 0f) return false;

            // No other polygon vertex may lie inside triangle (a, b, c)
            int count = idx.Count;
            for (int j = 0; j < count; j++)
            {
                if (j == iPrev || j == iCur || j == iNext) continue;
                if (PointInTriangle(polygon[idx[j]], a, b, c)) return false;
            }
            return true;
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = TriSign(p, a, b);
            float d2 = TriSign(p, b, c);
            float d3 = TriSign(p, c, a);
            bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNeg && hasPos);
        }

        private static float TriSign(Vector2 p1, Vector2 p2, Vector2 p3)
            => (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}
