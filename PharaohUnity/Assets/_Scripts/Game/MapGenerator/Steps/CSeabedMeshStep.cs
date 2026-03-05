using System.Collections.Generic;
using Pharaoh.Map;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Generates a procedural seabed mesh over all water tiles.
    ///
    /// Algorithm:
    ///   1. BFS from land-adjacent water tiles (coast cells) outward — assigns a distance
    ///      (in tile steps) to every water tile.
    ///   2. Maps distance → depth via CSeabedConfig.DepthCurve * MaxDepth.
    ///   3. For each water tile, builds a quad whose four corner Y values are averaged
    ///      from the depths of the (up to 4) adjacent tiles sharing that corner.
    ///   4. Combines all quads into a single MeshFilter + MeshRenderer.
    ///
    /// The resulting mesh sits below the water plane. A URP water shader can sample the
    /// Unity depth buffer to compute (waterY - seabedY) and render a depth-based color gradient.
    ///
    /// Must run after CDualGridCellSetupStep (needs CMapInstance + water tile data).
    /// </summary>
    public class CSeabedMeshStep : CMapGenerationStepBase
    {
        [SerializeField] private CSeabedConfig _seabed;

        public override string StepName        => "Seabed Mesh";
        public override string StepDescription => "Generuje procedurální mesh dna moře s interpolovanou hloubkou — vzdálenější voda je hlubší.";

        public override void Execute(CMapData mapData, int seed)
        {
            if (_seabed == null)
            {
                Debug.LogWarning($"[{StepName}] Seabed config is not assigned — skipping.");
                return;
            }

            var _config = _seabed;

            var mapInstance = GetMapInstance();
            if (mapInstance == null) return;

            // ── 1. BFS: compute distance-from-shore for every water tile ──────
            var dist    = new int[mapData.Width, mapData.Height];
            for (int x = 0; x < mapData.Width; x++)
                for (int y = 0; y < mapData.Height; y++)
                    dist[x, y] = int.MaxValue;

            var queue = new Queue<(int x, int y)>();

            // Seed: water tiles that are adjacent to at least one land tile
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    if (mapData.Get(x, y).Type != ETileType.Water) continue;
                    if (HasLandNeighbor(mapData, x, y))
                    {
                        dist[x, y] = 1;
                        queue.Enqueue((x, y));
                    }
                }
            }

            // BFS
            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                int nextDist = dist[cx, cy] + 1;

                foreach (var (dx, dy) in CardinalDirs)
                {
                    int nx = cx + dx, ny = cy + dy;
                    if (!mapData.IsValid(nx, ny)) continue;
                    if (mapData.Get(nx, ny).Type != ETileType.Water) continue;
                    if (dist[nx, ny] <= nextDist) continue;
                    dist[nx, ny] = nextDist;
                    queue.Enqueue((nx, ny));
                }
            }

            // Find the maximum finite distance for normalization
            int maxDist = 1;
            for (int x = 0; x < mapData.Width; x++)
                for (int y = 0; y < mapData.Height; y++)
                    if (dist[x, y] != int.MaxValue && dist[x, y] > maxDist)
                        maxDist = dist[x, y];

            // ── 2. Convert distance to per-tile depth ─────────────────────────
            var depth = new float[mapData.Width, mapData.Height];
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    if (mapData.Get(x, y).Type != ETileType.Water) continue;
                    float t      = dist[x, y] == int.MaxValue ? 1f : dist[x, y] / (float)maxDist;
                    depth[x, y]  = _config.DepthCurve.Evaluate(t) * _config.MaxDepth;
                }
            }

            // ── 3. Build per-corner averaged depth ───────────────────────────
            // Corner (cx, cy) is shared by tiles (cx-1,cy-1), (cx,cy-1), (cx-1,cy), (cx,cy).
            var cornerDepth = new float[mapData.Width + 1, mapData.Height + 1];
            for (int cx = 0; cx <= mapData.Width; cx++)
            {
                for (int cy = 0; cy <= mapData.Height; cy++)
                {
                    float sum = 0f;
                    int   cnt = 0;

                    foreach (var (ox, oy) in CornerNeighborOffsets)
                    {
                        int tx = cx + ox, ty = cy + oy;
                        if (!mapData.IsValid(tx, ty)) continue;
                        if (mapData.Get(tx, ty).Type != ETileType.Water) continue;
                        sum += depth[tx, ty];
                        cnt++;
                    }

                    cornerDepth[cx, cy] = cnt > 0 ? sum / cnt : 0f;
                }
            }

            // ── 4. Build mesh quads for every water tile ──────────────────────
            var vertices  = new List<Vector3>();
            var triangles = new List<int>();
            var uvs       = new List<Vector2>();

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    if (mapData.Get(x, y).Type != ETileType.Water) continue;

                    // Four corners of this tile (world positions, Y = -depth)
                    // SW=(x,y), SE=(x+1,y), NW=(x,y+1), NE=(x+1,y+1)
                    float ySW = -cornerDepth[x,     y];
                    float ySE = -cornerDepth[x + 1, y];
                    float yNW = -cornerDepth[x,     y + 1];
                    float yNE = -cornerDepth[x + 1, y + 1];

                    int base_ = vertices.Count;

                    vertices.Add(new Vector3(x - 0.5f,       ySW, y - 0.5f));       // SW
                    vertices.Add(new Vector3(x + 0.5f,       ySE, y - 0.5f));       // SE
                    vertices.Add(new Vector3(x - 0.5f,       yNW, y + 0.5f));       // NW
                    vertices.Add(new Vector3(x + 0.5f,       yNE, y + 0.5f));       // NE

                    uvs.Add(new Vector2(0f, 0f));
                    uvs.Add(new Vector2(1f, 0f));
                    uvs.Add(new Vector2(0f, 1f));
                    uvs.Add(new Vector2(1f, 1f));

                    // Two triangles (CCW winding, Y-up)
                    triangles.Add(base_);
                    triangles.Add(base_ + 2);
                    triangles.Add(base_ + 1);

                    triangles.Add(base_ + 1);
                    triangles.Add(base_ + 2);
                    triangles.Add(base_ + 3);
                }
            }

            if (vertices.Count == 0)
            {
                Debug.Log($"[{StepName}] No water tiles found — mesh not created.");
                return;
            }

            // ── 5. Create the mesh GameObject ─────────────────────────────────
            var mesh = new Mesh { name = "SeabedMesh" };

            if (vertices.Count > 65535)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject("SeabedMesh");
            go.transform.SetParent(mapInstance.transform, false);

            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            if (_config.SeabedMaterial != null)
                mr.sharedMaterial = _config.SeabedMaterial;

            Debug.Log($"[{StepName}] Seabed mesh built — {vertices.Count / 4} water tiles, {vertices.Count} vertices.");
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private static bool HasLandNeighbor(CMapData map, int x, int y)
        {
            foreach (var (dx, dy) in CardinalDirs)
            {
                int nx = x + dx, ny = y + dy;
                if (!map.IsValid(nx, ny)) continue;
                if (map.Get(nx, ny).Type == ETileType.Land) return true;
            }
            return false;
        }

        private CMapInstance GetMapInstance()
        {
            Transform root = transform.parent;
            var mi = root != null ? root.GetComponentInChildren<CMapInstance>() : null;
            if (mi == null)
                Debug.LogError($"[{StepName}] CMapInstance not found — run CDualGridCellSetupStep first.");
            return mi;
        }

        private static readonly (int dx, int dy)[] CardinalDirs =
        {
            (0, 1), (0, -1), (1, 0), (-1, 0)
        };

        // Tiles that share a corner at (cx, cy): offsets from corner position
        private static readonly (int ox, int oy)[] CornerNeighborOffsets =
        {
            (-1, -1), (0, -1), (-1, 0), (0, 0)
        };
    }
}
