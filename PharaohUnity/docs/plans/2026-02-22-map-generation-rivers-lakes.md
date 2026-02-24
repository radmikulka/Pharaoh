# Rivers & Lakes Map Generation — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add two new map generation pipeline steps — `CRiverGenerationStep` (noise-carved rivers) and `CLakeGenerationStep` (flood-fill lakes) — to the existing editor-driven map generator.

**Architecture:** Both steps implement `IMapGenerationStep` and are added as child GameObjects of `CMapGenerator` in the scene hierarchy. They slot between `CBasicLayoutStep` (step 0) and `CVoronoiRegionStep` (existing step 1). Each step modifies `STile.Type` from `Land` to `Water` on selected tiles. No existing code is modified.

**Tech Stack:** Unity C#, FastNoiseLite (already in project), `IMapGenerationStep` interface, `CMapData`/`STile` data structures.

---

## Reference: Existing Patterns

All steps follow this pattern:
- `MonoBehaviour` in namespace `Pharaoh.MapGenerator`
- Implements `IMapGenerationStep` (properties: `string StepName`, method: `void Execute(CMapData mapData, int seed)`)
- Uses `[SerializeField]` for inspector-tunable parameters
- Lives in `Assets/_Scripts/Game/MapGenerator/Steps/`
- Uses `CNoiseConfig.CreateNoise(seed)` for noise generation
- Noise values normalized from -1..1 to 0..1 via `(raw + 1f) / 2f`
- Uses `System.Random(seed)` for deterministic randomization
- Uses `mapData.IsValid(x, y)` for bounds checking
- Uses `mapData.Get(x, y)` / `mapData.Set(x, y, tile)` for tile access

---

### Task 1: Create CRiverGenerationStep

**Files:**
- Create: `Assets/_Scripts/Game/MapGenerator/Steps/CRiverGenerationStep.cs`

**Step 1: Create the file with class skeleton and parameters**

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Carves natural-looking river paths through land tiles using noise-based lateral drift.
    /// Rivers flow from one map edge to the opposite edge.
    /// </summary>
    public class CRiverGenerationStep : MonoBehaviour, IMapGenerationStep
    {
        [Header("Rivers")]
        [SerializeField] [Range(0, 3)] private int _riverCount = 1;
        [SerializeField] [Range(1, 2)] private int _maxWidth = 1;

        [Header("Path Shape")]
        [SerializeField] [Range(0f, 1f)] private float _windiness = 0.5f;
        [SerializeField] private float _noiseFrequency = 0.05f;

        public string StepName => "Rivers";

        public void Execute(CMapData mapData, int seed)
        {
            if (_riverCount <= 0) return;

            var rng = new System.Random(seed);

            for (int i = 0; i < _riverCount; i++)
            {
                int riverSeed = rng.Next();
                GenerateRiver(mapData, rng, riverSeed);
            }
        }

        private void GenerateRiver(CMapData mapData, System.Random rng, int riverSeed)
        {
            // Determine flow direction: 0 = north-south, 1 = east-west
            bool horizontal = rng.Next(2) == 0;

            int length = horizontal ? mapData.Width : mapData.Height;
            int breadth = horizontal ? mapData.Height : mapData.Width;

            // Pick start position along the entry edge
            int startCross = FindLandOnEdge(mapData, rng, horizontal, isEnd: false);
            if (startCross < 0) return;

            // Set up noise for lateral drift
            var noise = new FastNoiseLite(riverSeed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(_noiseFrequency);

            float crossPos = startCross;
            int placed = 0;

            for (int step = 0; step < length; step++)
            {
                // Sample noise for lateral drift
                float raw = noise.GetNoise(step, 0f);
                float drift = raw * _windiness * 2f; // drift in range [-windiness*2, +windiness*2]
                crossPos += drift;
                crossPos = Mathf.Clamp(crossPos, 0, breadth - 1);

                int crossInt = Mathf.RoundToInt(crossPos);

                // Determine tile coordinates based on flow direction
                int x = horizontal ? step : crossInt;
                int y = horizontal ? crossInt : step;

                if (!mapData.IsValid(x, y)) continue;

                // Carve main tile
                CarveTile(mapData, x, y);
                placed++;

                // Widen river
                if (_maxWidth > 1)
                {
                    int wx = horizontal ? x : x + 1;
                    int wy = horizontal ? y + 1 : y;
                    if (mapData.IsValid(wx, wy))
                        CarveTile(mapData, wx, wy);
                }
            }

            Debug.Log($"[{StepName}] River carved: {placed} tiles (direction: {(horizontal ? "E-W" : "N-S")}).");
        }

        private int FindLandOnEdge(CMapData mapData, System.Random rng, bool horizontal, bool isEnd)
        {
            int breadth = horizontal ? mapData.Height : mapData.Width;
            int edgePos = isEnd
                ? (horizontal ? mapData.Width - 1 : mapData.Height - 1)
                : 0;

            // Collect all land tiles on this edge
            var candidates = new List<int>();
            for (int i = 0; i < breadth; i++)
            {
                int x = horizontal ? edgePos : i;
                int y = horizontal ? i : edgePos;
                if (mapData.IsValid(x, y) && mapData.Get(x, y).Type == ETileType.Land)
                    candidates.Add(i);
            }

            if (candidates.Count == 0) return -1;
            return candidates[rng.Next(candidates.Count)];
        }

        private static void CarveTile(CMapData mapData, int x, int y)
        {
            STile tile = mapData.Get(x, y);
            if (tile.Type == ETileType.Land)
            {
                tile.Type = ETileType.Water;
                mapData.Set(x, y, tile);
            }
        }
    }
}
```

**Step 2: Verify the file compiles**

Open Unity Editor. Wait for script compilation. Check the Console for errors. The step should appear as a component you can add to a child GameObject of `CMapGenerator`.

**Step 3: Manual editor test**

1. In the scene hierarchy, find the `CMapGenerator` object.
2. Create a new child GameObject named "Step 1 - Rivers" and position it in hierarchy between "Step 0 - Basic Layout" and the Voronoi step.
3. Add the `CRiverGenerationStep` component.
4. Set `_riverCount = 1`, `_windiness = 0.5`, `_maxWidth = 1`.
5. Click "Generate" on `CMapGenerator`.
6. Verify in the Gizmos view that a river of water tiles carves through the land.
7. Try `_riverCount = 2` and regenerate — two rivers should appear.

**Step 4: Commit**

```bash
git add Assets/_Scripts/Game/MapGenerator/Steps/CRiverGenerationStep.cs
git add Assets/_Scripts/Game/MapGenerator/Steps/CRiverGenerationStep.cs.meta
git commit -m "feat: add CRiverGenerationStep to map generation pipeline"
```

---

### Task 2: Create CLakeGenerationStep

**Files:**
- Create: `Assets/_Scripts/Game/MapGenerator/Steps/CLakeGenerationStep.cs`

**Step 1: Create the file with full implementation**

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Places organic-shaped inland lakes using noise-driven flood fill.
    /// Lakes are grown from seed points on land tiles, producing irregular shapes.
    /// </summary>
    public class CLakeGenerationStep : MonoBehaviour, IMapGenerationStep
    {
        [Header("Lakes")]
        [SerializeField] [Range(0, 3)] private int _lakeCount = 1;
        [SerializeField] [Min(1)] private int _minSize = 3;
        [SerializeField] [Min(1)] private int _maxSize = 8;

        [Header("Placement")]
        [SerializeField] [Min(2)] private int _minDistanceFromEdge = 3;
        [SerializeField] [Min(2)] private int _minDistanceBetweenLakes = 5;

        [Header("Shape")]
        [SerializeField] [Range(0f, 1f)] private float _irregularity = 0.5f;
        [SerializeField] private float _noiseFrequency = 0.1f;

        public string StepName => "Lakes";

        public void Execute(CMapData mapData, int seed)
        {
            if (_lakeCount <= 0) return;

            var rng = new System.Random(seed);
            var lakeCenters = new List<Vector2Int>();

            for (int i = 0; i < _lakeCount; i++)
            {
                int lakeSeed = rng.Next();
                Vector2Int? center = FindLakeSeed(mapData, rng, lakeCenters);
                if (!center.HasValue)
                {
                    Debug.LogWarning($"[{StepName}] Could not find valid seed for lake {i + 1}.");
                    continue;
                }

                int targetSize = rng.Next(_minSize, _maxSize + 1);
                int placed = GrowLake(mapData, center.Value, targetSize, lakeSeed);
                lakeCenters.Add(center.Value);

                Debug.Log($"[{StepName}] Lake at ({center.Value.x},{center.Value.y}): {placed} tiles.");
            }
        }

        private Vector2Int? FindLakeSeed(CMapData mapData, System.Random rng, List<Vector2Int> existing)
        {
            const int maxAttempts = 100;

            int minX = _minDistanceFromEdge;
            int minY = _minDistanceFromEdge;
            int maxX = mapData.Width - _minDistanceFromEdge - 1;
            int maxY = mapData.Height - _minDistanceFromEdge - 1;

            if (minX > maxX || minY > maxY) return null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int x = rng.Next(minX, maxX + 1);
                int y = rng.Next(minY, maxY + 1);

                if (!mapData.IsValid(x, y)) continue;
                if (mapData.Get(x, y).Type != ETileType.Land) continue;

                // Check distance from other lakes
                bool tooClose = false;
                foreach (var other in existing)
                {
                    int dx = x - other.x;
                    int dy = y - other.y;
                    if (dx * dx + dy * dy < _minDistanceBetweenLakes * _minDistanceBetweenLakes)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                return new Vector2Int(x, y);
            }

            return null;
        }

        private int GrowLake(CMapData mapData, Vector2Int seed, int targetSize, int noiseSeed)
        {
            var noise = new FastNoiseLite(noiseSeed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(_noiseFrequency);

            var filled = new HashSet<Vector2Int>();
            var frontier = new List<Vector2Int> { seed };
            var rng = new System.Random(noiseSeed);

            filled.Add(seed);
            CarveTile(mapData, seed.x, seed.y);

            // Cardinal neighbor offsets
            Vector2Int[] offsets = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };

            while (filled.Count < targetSize && frontier.Count > 0)
            {
                // Pick a random frontier tile to expand from
                int idx = rng.Next(frontier.Count);
                Vector2Int current = frontier[idx];
                frontier.RemoveAt(idx);

                foreach (var offset in offsets)
                {
                    Vector2Int neighbor = current + offset;

                    if (filled.Contains(neighbor)) continue;
                    if (!mapData.IsValid(neighbor.x, neighbor.y)) continue;
                    if (mapData.Get(neighbor.x, neighbor.y).Type != ETileType.Land) continue;

                    // Noise-based acceptance — higher irregularity = more rejection = jagged shapes
                    float raw = noise.GetNoise(neighbor.x, neighbor.y);
                    float normalized = (raw + 1f) / 2f;
                    float threshold = _irregularity * 0.6f; // scale so 1.0 irregularity ≈ 60% rejection

                    if (normalized < threshold) continue;

                    filled.Add(neighbor);
                    CarveTile(mapData, neighbor.x, neighbor.y);
                    frontier.Add(neighbor);

                    if (filled.Count >= targetSize) break;
                }
            }

            return filled.Count;
        }

        private static void CarveTile(CMapData mapData, int x, int y)
        {
            STile tile = mapData.Get(x, y);
            if (tile.Type == ETileType.Land)
            {
                tile.Type = ETileType.Water;
                mapData.Set(x, y, tile);
            }
        }
    }
}
```

**Step 2: Verify the file compiles**

Open Unity Editor. Wait for script compilation. Check Console for errors.

**Step 3: Manual editor test**

1. In the scene hierarchy under `CMapGenerator`, create a child GameObject named "Step 2 - Lakes".
2. Position it after the Rivers step and before the Voronoi step.
3. Add the `CLakeGenerationStep` component.
4. Set `_lakeCount = 1`, `_minSize = 3`, `_maxSize = 6`, `_irregularity = 0.4`.
5. Click "Generate" on `CMapGenerator`.
6. Verify in Gizmos view that an organic lake shape appears inland (not at the edges).
7. Set `_lakeCount = 2`, regenerate — two separated lakes should appear.
8. Set `_irregularity = 0` — lakes should be more circular. Set to `1.0` — more jagged.

**Step 4: Commit**

```bash
git add Assets/_Scripts/Game/MapGenerator/Steps/CLakeGenerationStep.cs
git add Assets/_Scripts/Game/MapGenerator/Steps/CLakeGenerationStep.cs.meta
git commit -m "feat: add CLakeGenerationStep to map generation pipeline"
```

---

### Task 3: Integration Test — Full Pipeline

**Files:** None (scene setup only)

**Step 1: Set up the full pipeline in the scene**

Ensure the `CMapGenerator` hierarchy looks like:

```
CMapGenerator
├── Step 0 - Basic Layout      (CBasicLayoutStep)
├── Step 1 - Rivers            (CRiverGenerationStep)
├── Step 2 - Lakes             (CLakeGenerationStep)
├── Step 3 - Voronoi Regions   (CVoronoiRegionStep)
├── Step 4 - Obstacles (Rock)  (CObstaclePlacementStep)
├── Step 4 - Obstacles (Tree)  (CObstaclePlacementStep)
└── Step 5 - Spawn Map         (CSpawnMapStep)
```

**Step 2: Generate and verify full pipeline**

1. Click "Generate" on `CMapGenerator`.
2. Verify in order:
   - Land/water base layout appears (step 0).
   - Rivers carve through land (step 1).
   - Lakes appear inland (step 2).
   - Biomes are assigned only to remaining land tiles (step 3) — river/lake tiles should NOT have biomes.
   - Obstacles appear only on land tiles (step 4) — none on rivers or lakes.
   - Spawned GameObjects match the Gizmos preview (step 5).
3. Check Console for all step logs showing expected tile counts.

**Step 3: Test edge cases**

- Set `_riverCount = 0` and `_lakeCount = 0` — should produce same result as before (no rivers/lakes).
- Set `_riverCount = 3` — verify rivers don't break the map.
- Generate with multiple seeds — verify results are deterministic (same seed = same map).

**Step 4: Test building placement**

1. After generating, enter Play mode.
2. Click on a river tile — should NOT open the building menu.
3. Click on a lake tile — should NOT open the building menu.
4. Click on a land tile near a river — should show Coastal/NearWater tags if adjacent to river.

**Step 5: Commit scene changes**

```bash
git add -A
git commit -m "feat: integrate rivers and lakes into map generation pipeline"
```
