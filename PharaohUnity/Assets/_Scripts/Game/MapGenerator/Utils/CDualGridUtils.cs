using System;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Shared utilities for dual-grid map rendering steps.
    ///
    /// Dual grid principle: visual tiles are placed at corners (intersections) of the logical grid.
    /// A corner (cx, cy) reads from four adjacent logical cells and produces an EDualGridMask that
    /// selects which prefab variant to spawn.
    ///
    ///   NW = cell (cx-1, cy)    NE = cell (cx, cy)
    ///   SW = cell (cx-1, cy-1)  SE = cell (cx, cy-1)
    ///
    /// Corner world position: (cx - 0.5, yOffset, cy - 0.5)
    /// Corner range: cx in [0..W], cy in [0..H]  →  (W+1)×(H+1) corners total.
    /// </summary>
    public static class CDualGridUtils
    {
        /// <summary>
        /// Compute the EDualGridMask for corner (cx, cy).
        /// Out-of-bounds cells return false for the predicate (treated as "not active").
        /// </summary>
        public static EDualGridMask ComputeMask(CMapData map, int cx, int cy, Func<STile, bool> predicate)
        {
            var mask = EDualGridMask.None;
            if (SamplePredicate(map, cx - 1, cy,     predicate)) mask |= EDualGridMask.NW;
            if (SamplePredicate(map, cx,     cy,     predicate)) mask |= EDualGridMask.NE;
            if (SamplePredicate(map, cx - 1, cy - 1, predicate)) mask |= EDualGridMask.SW;
            if (SamplePredicate(map, cx,     cy - 1, predicate)) mask |= EDualGridMask.SE;
            return mask;
        }

        /// <summary>
        /// Returns a deterministic index in [0, count) for corner (cx, cy).
        /// Same position always gives the same result regardless of generation order.
        /// </summary>
        public static int GetVariantIndex(int cx, int cy, int count)
        {
            if (count <= 1) return 0;
            int hash = cx * 73856093 ^ cy * 19349663;
            hash = (hash ^ (hash >> 13)) * 1597334677;
            hash ^= hash >> 16;
            return (hash & 0x7FFFFFFF) % count;
        }

        /// <summary>
        /// Spawn a tile prefab at the world position of corner (cx, cy).
        /// Returns null if the variant is null, has no prefabs, or the selected prefab is null.
        /// </summary>
        public static GameObject SpawnCornerTile(CDualGridTileVariant variant, int cx, int cy,
                                                  float yOffset, Transform parent)
        {
            if (variant?.Prefabs == null || variant.Prefabs.Length == 0)
                return null;

            int idx    = GetVariantIndex(cx, cy, variant.Prefabs.Length);
            var prefab = variant.Prefabs[idx];
            if (prefab == null) return null;

            var worldPos = new Vector3(cx - 0.5f, yOffset, cy - 0.5f);

#if UNITY_EDITOR
            var go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
            return go;
#else
            return UnityEngine.Object.Instantiate(prefab, worldPos, Quaternion.identity, parent);
#endif
        }

        // ─── Private ─────────────────────────────────────────────────────────

        private static bool SamplePredicate(CMapData map, int x, int y, Func<STile, bool> predicate)
        {
            if (!map.IsValid(x, y)) return false;
            return predicate(map.Get(x, y));
        }
    }
}
