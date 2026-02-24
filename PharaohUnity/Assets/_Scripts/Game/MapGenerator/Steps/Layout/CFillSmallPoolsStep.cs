using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Two-pass cleanup step:
    ///   Pass 1 — fills isolated water pools (not touching map border, smaller than _minPoolSize) → Land.
    ///   Pass 2 — fills isolated land islands (not touching map border, smaller than _minIslandSize) → Water.
    ///
    /// Run after all water-carving steps (BasicLayout, Rivers, Lakes)
    /// and before biome/obstacle placement.
    /// </summary>
    public class CFillSmallPoolsStep : CMapGenerationStepBase
    {
        [Header("Fill Settings")]
        [Tooltip("Water components with fewer tiles than this are filled to Land, " +
                 "provided they do not touch the map border.")]
        [SerializeField] [Min(1)] private int _minPoolSize = 10;

        [Tooltip("Land components with fewer tiles than this are filled to Water, " +
                 "provided they do not touch the map border (i.e. isolated islands).")]
        [SerializeField] [Min(1)] private int _minIslandSize = 10;

        private static readonly Vector2Int[] CardinalOffsets =
        {
            new(0,  1),
            new(0, -1),
            new(1,  0),
            new(-1, 0),
        };

        public override string StepName => "Fill Small Pools";
        public override string StepDescription => "Vyplňuje izolované malé vodní plochy na pevninu a malé ostrůvky na vodu.";

        public override void Execute(CMapData mapData, int seed)
        {
            FillIsolatedComponents(mapData, isBuildable: false, ETileType.Land,  _minPoolSize,   "pool(s)");
            FillIsolatedComponents(mapData, isBuildable: true,  ETileType.Water, _minIslandSize, "island(s)");
        }

        /// <param name="isBuildable">
        /// When true, scans buildable tiles (Land + Sand) as a single group.
        /// When false, scans Water tiles.
        /// </param>
        private void FillIsolatedComponents(
            CMapData mapData,
            bool isBuildable,
            ETileType fillType,
            int minSize,
            string label)
        {
            var visited   = new bool[mapData.Width, mapData.Height];
            var queue     = new Queue<Vector2Int>();
            var component = new List<Vector2Int>();

            int fillCount      = 0;
            int componentCount = 0;

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    if (visited[x, y]) continue;
                    if (MatchesScanGroup(mapData.Get(x, y).Type, isBuildable) == false) continue;

                    component.Clear();
                    queue.Clear();
                    bool isBorderConnected = false;

                    visited[x, y] = true;
                    queue.Enqueue(new Vector2Int(x, y));

                    while (queue.Count > 0)
                    {
                        Vector2Int current = queue.Dequeue();
                        component.Add(current);

                        if (current.x == 0 || current.x == mapData.Width  - 1 ||
                            current.y == 0 || current.y == mapData.Height - 1)
                            isBorderConnected = true;

                        foreach (var offset in CardinalOffsets)
                        {
                            int nx = current.x + offset.x;
                            int ny = current.y + offset.y;

                            if (!mapData.IsValid(nx, ny)) continue;
                            if (visited[nx, ny])          continue;
                            if (MatchesScanGroup(mapData.Get(nx, ny).Type, isBuildable) == false) continue;

                            visited[nx, ny] = true;
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }

                    componentCount++;

                    if (isBorderConnected || component.Count >= minSize) continue;

                    foreach (var pos in component)
                    {
                        STile tile = mapData.Get(pos.x, pos.y);
                        tile.Type  = fillType;
                        mapData.Set(pos.x, pos.y, tile);
                    }

                    fillCount++;
                }
            }

            string scanLabel = isBuildable ? "buildable (Land+Sand)" : "Water";
            Debug.Log($"[{StepName}] Filled {fillCount} isolated {label} (< {minSize} tiles) " +
                      $"out of {componentCount} {scanLabel} component(s) found.");
        }

        private static bool MatchesScanGroup(ETileType type, bool isBuildable)
        {
            return isBuildable ? type.IsBuildable() : type == ETileType.Water;
        }
    }
}
