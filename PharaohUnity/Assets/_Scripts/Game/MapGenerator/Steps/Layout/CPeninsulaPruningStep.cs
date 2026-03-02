using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Removes degenerate Land tiles (peninsulas / spikes) that have ≥N cardinal Water neighbors
    /// via iterative erosion. These tiles have no valid cliff mesh and would cause warnings in
    /// CCliffDetectionStep.
    ///
    /// Phase 1: iterative erosion (≥N water cardinal neighbors) until stable.
    /// Phase 2: bridge detection — finds any land tile that is a graph articulation vertex
    ///          (removing it disconnects the land graph). The non-border-connected component(s)
    ///          are the peninsula and get converted to Water along with the bridge tile.
    ///          Phase 2 is followed by another Phase 1 pass until no more bridges are found.
    ///
    /// Run after CFillSmallPoolsStep and before CCoastalTransitionStep.
    /// </summary>
    public class CPeninsulaPruningStep : CMapGenerationStepBase
    {
        [Header("Peninsula Pruning")]
        [Tooltip("Land tiles with this many or more cardinal Water neighbors are converted to Water.")]
        [SerializeField, Range(2, 4)] private int _minCardinalWaterNeighbors = 3;

        private static readonly Vector2Int[] CardinalOffsets =
        {
            new(0,  1),   // N
            new(0, -1),   // S
            new(1,  0),   // E
            new(-1, 0),   // W
        };

#if UNITY_EDITOR
        private List<Vector2Int> _cachedPrunedTiles = new();
#endif

        public override string StepName => "Peninsula Pruning";
        public override string StepDescription =>
            "Odstraňuje Land dlaždice s ≥N kardinálními vodními sousedy (výběžky/hroty) " +
            "iterativní erozí. Spouštět po CFillSmallPoolsStep, před CCoastalTransitionStep.";

        public override void Execute(CMapData mapData, int seed)
        {
#if UNITY_EDITOR
            _cachedPrunedTiles.Clear();
#endif
            int totalPruned = 0;
            int bridgePasses = 0;

            while (true)
            {
                totalPruned += RunErosionUntilStable(mapData);   // Phase 1
                int bridgeCount = RunBridgeDetectionPass(mapData);  // Phase 2
                if (bridgeCount == 0) break;
                totalPruned += bridgeCount;
                bridgePasses++;
            }

            Debug.Log($"[{StepName}] Pruned {totalPruned} tiles " +
                      $"({bridgePasses} bridge pass(es)).");
        }

        // ── Phase 1 ──────────────────────────────────────────────────────────────────

        private int RunErosionUntilStable(CMapData mapData)
        {
            int total = 0;
            while (true)
            {
                var toConvert = new List<Vector2Int>();
                for (int x = 0; x < mapData.Width; x++)
                    for (int y = 0; y < mapData.Height; y++)
                    {
                        if (mapData.Get(x, y).Type != ETileType.Land) continue;
                        if (CountCardinalWaterNeighbors(mapData, x, y) >= _minCardinalWaterNeighbors)
                            toConvert.Add(new Vector2Int(x, y));
                    }

                if (toConvert.Count == 0) break;

                foreach (var pos in toConvert)
                {
                    STile tile = mapData.Get(pos.x, pos.y);
                    tile.Type = ETileType.Water;
                    mapData.Set(pos.x, pos.y, tile);
#if UNITY_EDITOR
                    _cachedPrunedTiles.Add(pos);
#endif
                }
                total += toConvert.Count;
            }
            return total;
        }

        // ── Phase 2 ──────────────────────────────────────────────────────────────────

        // Single pass over all Land tiles. Any tile whose removal disconnects the land
        // graph is an articulation vertex / bridge. Non-border-connected components on
        // the far side of such a bridge are peninsulas and get converted to Water.
        // Returns the number of tiles converted.
        private int RunBridgeDetectionPass(CMapData mapData)
        {
            var toConvert = new HashSet<Vector2Int>();

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (mapData.Get(x, y).Type != ETileType.Land) continue;
                    if (toConvert.Contains(pos)) continue;

                    // Collect all in-bounds Land cardinal neighbors.
                    var landNeighbors = new List<Vector2Int>();
                    foreach (var off in CardinalOffsets)
                    {
                        var nb = new Vector2Int(x + off.x, y + off.y);
                        if (mapData.IsValid(nb.x, nb.y) && mapData.Get(nb.x, nb.y).Type == ETileType.Land)
                            landNeighbors.Add(nb);
                    }
                    if (landNeighbors.Count < 2) continue; // can't be a bridge

                    var fromFirst = FloodFillLand(mapData, landNeighbors[0], pos);

                    // Quick rejection: all other land neighbors reachable → not a bridge.
                    bool allReachable = true;
                    for (int i = 1; i < landNeighbors.Count; i++)
                    {
                        if (!fromFirst.Contains(landNeighbors[i])) { allReachable = false; break; }
                    }
                    if (allReachable) continue;

                    // Collect remaining components from each isolated neighbor.
                    var components = new List<HashSet<Vector2Int>> { fromFirst };
                    var covered = new HashSet<Vector2Int>(fromFirst);
                    for (int i = 1; i < landNeighbors.Count; i++)
                    {
                        if (covered.Contains(landNeighbors[i])) continue;
                        var comp = FloodFillLand(mapData, landNeighbors[i], pos);
                        components.Add(comp);
                        covered.UnionWith(comp);
                    }

                    // Keep border-connected components; collect peninsulas.
                    var peninsulas = new List<HashSet<Vector2Int>>();
                    foreach (var comp in components)
                        if (!IsBorderConnected(mapData, comp))
                            peninsulas.Add(comp);

                    if (peninsulas.Count == 0) continue; // genuine bridge, keep all

                    // Edge case: nothing is border-connected → keep largest, remove rest.
                    if (peninsulas.Count == components.Count)
                    {
                        var largest = components[0];
                        foreach (var comp in components)
                            if (comp.Count > largest.Count) largest = comp;
                        peninsulas.Clear();
                        foreach (var comp in components)
                            if (comp != largest) peninsulas.Add(comp);
                        if (peninsulas.Count == 0) continue;
                    }

                    toConvert.Add(pos);
                    foreach (var peninsula in peninsulas)
                        foreach (var tile in peninsula)
                            toConvert.Add(tile);
                }
            }

            foreach (var pos in toConvert)
            {
                STile tile = mapData.Get(pos.x, pos.y);
                tile.Type = ETileType.Water;
                mapData.Set(pos.x, pos.y, tile);
#if UNITY_EDITOR
                _cachedPrunedTiles.Add(pos);
#endif
            }
            return toConvert.Count;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        // BFS over Land tiles starting at 'start', forbidden from visiting 'exclude'.
        private HashSet<Vector2Int> FloodFillLand(CMapData mapData, Vector2Int start, Vector2Int exclude)
        {
            var visited = new HashSet<Vector2Int>();
            if (!mapData.IsValid(start.x, start.y) || mapData.Get(start.x, start.y).Type != ETileType.Land)
                return visited;

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var off in CardinalOffsets)
                {
                    var next = new Vector2Int(cur.x + off.x, cur.y + off.y);
                    if (next == exclude) continue;
                    if (visited.Contains(next)) continue;
                    if (!mapData.IsValid(next.x, next.y)) continue;
                    if (mapData.Get(next.x, next.y).Type != ETileType.Land) continue;
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
            return visited;
        }

        private bool IsBorderConnected(CMapData mapData, HashSet<Vector2Int> component)
        {
            foreach (var p in component)
                if (p.x == 0 || p.x == mapData.Width - 1 || p.y == 0 || p.y == mapData.Height - 1)
                    return true;
            return false;
        }

        // Returns how many cardinal neighbors are Water (or out of bounds).
        private int CountCardinalWaterNeighbors(CMapData mapData, int x, int y)
        {
            int count = 0;
            foreach (var offset in CardinalOffsets)
            {
                int nx = x + offset.x;
                int ny = y + offset.y;
                if (!mapData.IsValid(nx, ny) || mapData.Get(nx, ny).Type == ETileType.Water)
                    count++;
            }
            return count;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_cachedPrunedTiles == null || _cachedPrunedTiles.Count == 0) return;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.7f); // red
            foreach (var p in _cachedPrunedTiles)
                Gizmos.DrawCube(new Vector3(p.x, 0.2f, p.y), new Vector3(0.9f, 0.05f, 0.9f));
        }
#endif
    }
}
