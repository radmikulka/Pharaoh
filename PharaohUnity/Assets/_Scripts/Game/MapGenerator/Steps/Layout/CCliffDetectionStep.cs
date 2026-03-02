using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Analyzes Land/Water neighbors for every Land tile and writes CliffType + CliffRotationDeg
    /// into STile. Must run after all steps that finalize ETileType (including CFillSmallPoolsStep
    /// and CCoastalTransitionStep) and before CSpawnMapStep.
    ///
    /// Rotation convention: Y-rotation applied as Quaternion.Euler(0, CliffRotationDeg, 0).
    /// Default orientation (0°) = cliff face pointing North (+Z). Artist must verify with actual meshes.
    ///
    /// Out-of-bounds neighbors are treated as Water.
    /// </summary>
    public class CCliffDetectionStep : CMapGenerationStepBase
    {
        public override string StepName => "Cliff Detection";

#if UNITY_EDITOR
        private struct CGizmoCliffTile
        {
            public Vector2Int Pos;
            public ECliffType Type;
            public float      RotationDeg;
        }

        private List<CGizmoCliffTile> _cachedCliffTiles = new();
#endif
        public override string StepDescription => "Analyzuje sousedy každé Land dlaždice a zapisuje CliffType + CliffRotationDeg do STile pro následný spawn cliff meshů.";

        public override void Execute(CMapData mapData, int seed)
        {
            int cliffCount = 0;
#if UNITY_EDITOR
            _cachedCliffTiles.Clear();
#endif

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);
                    if (tile.Type != ETileType.Land) continue;

                    bool north = IsWater(mapData, x, y + 1);
                    bool south = IsWater(mapData, x, y - 1);
                    bool east  = IsWater(mapData, x + 1, y);
                    bool west  = IsWater(mapData, x - 1, y);

                    int cardinalCount = (north ? 1 : 0) + (south ? 1 : 0) + (east ? 1 : 0) + (west ? 1 : 0);

                    if (cardinalCount == 1)
                    {
                        tile.CliffType = ECliffType.Straight;
                        if (north)
                            tile.CliffRotationDeg = 90;
                        else if (east)
                            tile.CliffRotationDeg = 180;
                        else if (south)
                            tile.CliffRotationDeg = 270;
                        else
                            tile.CliffRotationDeg = 0;
                        cliffCount++;
                    }
                    else if (cardinalCount == 2)
                    {
                        bool opposite = (north && south) || (east && west);
                        if (opposite)
                        {
                            tile.CliffType = ECliffType.Strait;
                            tile.CliffRotationDeg = (north && south) ? 0 : 90;
                        }
                        else
                        {
                            tile.CliffType = ECliffType.OuterCorner;
                            if (north && east)       tile.CliffRotationDeg = 90;
                            else if (south && east)  tile.CliffRotationDeg = 180;
                            else if (south && west)  tile.CliffRotationDeg = 270;
                            else                     tile.CliffRotationDeg = 0; // north && west
                        }
                        cliffCount++;
                    }
                    else if (cardinalCount == 0)
                    {
                        bool ne = IsWater(mapData, x + 1, y + 1);
                        bool se = IsWater(mapData, x + 1, y - 1);
                        bool sw = IsWater(mapData, x - 1, y - 1);
                        bool nw = IsWater(mapData, x - 1, y + 1);

                        if (ne || se || sw || nw)
                        {
                            tile.CliffType = ECliffType.InnerCorner;
                            if (ne)
                                tile.CliffRotationDeg = 90;
                            else if (se)
                                tile.CliffRotationDeg = 180;
                            else if (sw)
                                tile.CliffRotationDeg = 270;
                            else
                                tile.CliffRotationDeg = 0;
                            cliffCount++;
                        }
                        // else: fully interior — CliffType stays None
                    }
                    else // cardinalCount >= 3: peninsula / fully surrounded
                    {
                        Debug.LogWarning($"[{StepName}] Tile ({x},{y}) has {cardinalCount} cardinal water neighbors (peninsula). Falling back to CliffType.None.");
                        // CliffType stays None — flat land fallback
                    }


#if UNITY_EDITOR
                    if (tile.CliffType != ECliffType.None)
                        _cachedCliffTiles.Add(new CGizmoCliffTile { Pos = new Vector2Int(x, y), Type = tile.CliffType, RotationDeg = tile.CliffRotationDeg });
#endif
                    mapData.Set(x, y, tile);
                }
            }

            Debug.Log($"[{StepName}] Detected {cliffCount} cliff tiles.");
        }

        // Returns true if (nx, ny) is out of bounds or a Water tile.
        private static bool IsWater(CMapData mapData, int nx, int ny)
        {
            if (!mapData.IsValid(nx, ny)) return true;
            return mapData.Get(nx, ny).Type == ETileType.Water;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_cachedCliffTiles == null || _cachedCliffTiles.Count == 0) return;

            foreach (var c in _cachedCliffTiles)
            {
                Gizmos.color = c.Type switch
                {
                    ECliffType.Straight    => new Color(1f,   0.5f, 0f,   0.8f), // orange
                    ECliffType.OuterCorner => new Color(1f,   0.2f, 0.2f, 0.8f), // red
                    ECliffType.InnerCorner => new Color(0.2f, 0.8f, 1f,   0.8f), // cyan
                    ECliffType.Strait      => new Color(0.7f, 0.2f, 1f,   0.8f), // purple
                    _                      => Color.white,
                };

                var center = new Vector3(c.Pos.x, 0.25f, c.Pos.y);
                Gizmos.DrawCube(center, new Vector3(0.85f, 0.05f, 0.85f));

                // Arrow pointing in the cliff-face direction (0° = North = +Z)
                var dir = Quaternion.Euler(0f, c.RotationDeg, 0f) * Vector3.forward * 0.4f;
                Gizmos.DrawLine(center, center + dir);
            }
        }
#endif
    }
}
