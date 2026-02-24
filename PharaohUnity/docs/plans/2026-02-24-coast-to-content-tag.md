# Coast → EContentTag Migration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Remove `ETileType.Coast` so tile type is only Water/Land; move the coastal-zone concept into `EContentTag`; update `CCoastalTransitionStep` to write `ContentTag`; replace `CObstacleContentTagStep` with a new `CObstacleProximityTagStep`.

**Architecture:** Pure data and step changes — no new abstractions. `ETileType` shrinks to two values; `EContentTag` gains `Coast`. Two step files change, one is deleted, one is created. `IsBuildable()` simplifies automatically; all callers that use it need no manual edits.

**Tech Stack:** Unity (URP), C#, no automated test runner — correctness verified by Unity Editor compile check and in-editor generation ("Generate Up To This Step" button on each step component).

---

### Task 1: Remove `ETileType.Coast` and simplify `IsBuildable()`

**Files:**
- Modify: `Assets/_Scripts/Game/MapGenerator/Enums/ETileType.cs`

**Step 1: Edit the file**

Replace the entire file content with:

```csharp
namespace Pharaoh.MapGenerator
{
    public enum ETileType
    {
        Water,
        Land,
    }

    public static class ETileTypeExtensions
    {
        public static bool IsBuildable(this ETileType type) =>
            type == ETileType.Land;
    }
}
```

**Step 2: Verify no other file references `ETileType.Coast`**

Search the codebase for any remaining references:
- Search pattern: `ETileType.Coast`
- Expected: zero results (only `CCoastalTransitionStep.cs` used it — that file is rewritten in Task 3)

**Step 3: Commit**

```
git add Assets/_Scripts/Game/MapGenerator/Enums/ETileType.cs
git commit -m "feat: remove ETileType.Coast — tile type is now Water or Land only"
```

---

### Task 2: Add `Coast` to `EContentTag`

**Files:**
- Modify: `Assets/_Scripts/Game/MapGenerator/Enums/EContentTag.cs`

**Step 1: Edit the file**

Replace the entire file content with:

```csharp
namespace Pharaoh.MapGenerator
{
    public enum EContentTag
    {
        None = 0,
        Coast,
        HuntersLodge,
    }
}
```

Note: `Coast` is inserted before `HuntersLodge`. Explicit integer values are not used (other than `None = 0`), so insertion order is safe as long as no serialized integer values were persisted in ScriptableObjects or scenes — if they were, append `Coast` at the end instead. Check Unity scene/asset files for serialized `ContentTag` integer values before deciding. If in doubt, append.

**Step 2: Commit**

```
git add Assets/_Scripts/Game/MapGenerator/Enums/EContentTag.cs
git commit -m "feat: add EContentTag.Coast (moved from ETileType)"
```

---

### Task 3: Rewrite `CCoastalTransitionStep`

**Files:**
- Modify: `Assets/_Scripts/Game/MapGenerator/Steps/Biome/CCoastalTransitionStep.cs`

**Step 1: Replace the file content**

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Tags Land tiles adjacent to Water with a configurable EContentTag, creating a coastal transition zone.
    /// Run after CFillSmallPoolsStep and before CObstaclePlacementStep.
    /// </summary>
    public class CCoastalTransitionStep : CMapGenerationStepBase
    {
        [Header("Coastal Transition")]
        [Tooltip("Content tag applied to coastal tiles.")]
        [SerializeField] private EContentTag _coastTag = EContentTag.Coast;

        [Tooltip("How many rings of tagged tiles to generate around water edges.")]
        [SerializeField] [Range(1, 10)] private int _sandDepth = 1;

        private static readonly Vector2Int[] CardinalOffsets =
        {
            new(0,  1),
            new(0, -1),
            new(1,  0),
            new(-1, 0),
        };

        public override string StepName => "Coastal Transition";
        public override string StepDescription => "Taguje land políčka sousedící s vodou EContentTag — vytváří pobřežní přechodovou zónu.";

        public override void Execute(CMapData mapData, int seed)
        {
            int tagged = 0;

            // Ring 1: Land adjacent to Water → _coastTag
            tagged += TagAdjacentToWater(mapData);

            // Rings 2..N: Land adjacent to already-tagged tiles → _coastTag
            for (int ring = 2; ring <= _sandDepth; ring++)
                tagged += TagAdjacentToTagged(mapData);

            Debug.Log($"[{StepName}] Tagged {tagged} land tiles (tag={_coastTag}, depth={_sandDepth}).");
        }

        private int TagAdjacentToWater(CMapData mapData)
        {
            var toTag = new List<Vector2Int>();

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);
                    if (tile.Type != ETileType.Land) continue;
                    if (tile.ContentTag != EContentTag.None) continue;
                    if (HasCardinalNeighborOfTileType(mapData, x, y, ETileType.Water))
                        toTag.Add(new Vector2Int(x, y));
                }
            }

            foreach (var pos in toTag)
            {
                STile tile = mapData.Get(pos.x, pos.y);
                tile.ContentTag = _coastTag;
                mapData.Set(pos.x, pos.y, tile);
            }

            return toTag.Count;
        }

        private int TagAdjacentToTagged(CMapData mapData)
        {
            var toTag = new List<Vector2Int>();

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);
                    if (tile.Type != ETileType.Land) continue;
                    if (tile.ContentTag != EContentTag.None) continue;
                    if (HasCardinalNeighborWithTag(mapData, x, y, _coastTag))
                        toTag.Add(new Vector2Int(x, y));
                }
            }

            foreach (var pos in toTag)
            {
                STile tile = mapData.Get(pos.x, pos.y);
                tile.ContentTag = _coastTag;
                mapData.Set(pos.x, pos.y, tile);
            }

            return toTag.Count;
        }

        private bool HasCardinalNeighborOfTileType(CMapData mapData, int x, int y, ETileType type)
        {
            foreach (var offset in CardinalOffsets)
            {
                int nx = x + offset.x;
                int ny = y + offset.y;
                if (mapData.IsValid(nx, ny) && mapData.Get(nx, ny).Type == type)
                    return true;
            }
            return false;
        }

        private bool HasCardinalNeighborWithTag(CMapData mapData, int x, int y, EContentTag tag)
        {
            foreach (var offset in CardinalOffsets)
            {
                int nx = x + offset.x;
                int ny = y + offset.y;
                if (mapData.IsValid(nx, ny) && mapData.Get(nx, ny).ContentTag == tag)
                    return true;
            }
            return false;
        }
    }
}
```

**Step 2: Verify in Unity Editor**

Open the Unity Editor. Confirm:
- No compile errors in the Console.
- Select the `CCoastalTransitionStep` component in the scene — Inspector shows `Coast Tag` (EContentTag dropdown, default `Coast`) and `Sand Depth` (int, default 1).
- Click "Generate Up To This Step" — map generates, Console logs `Tagged N land tiles (tag=Coast, depth=1)`.

**Step 3: Commit**

```
git add Assets/_Scripts/Game/MapGenerator/Steps/Biome/CCoastalTransitionStep.cs
git commit -m "feat: CCoastalTransitionStep writes EContentTag instead of ETileType.Coast"
```

---

### Task 4: Delete `CObstacleContentTagStep`

**Files:**
- Delete: `Assets/_Scripts/Game/MapGenerator/Steps/Obstacles/CObstacleContentTagStep.cs`
- Delete: `Assets/_Scripts/Game/MapGenerator/Steps/Obstacles/CObstacleContentTagStep.cs.meta`

**Step 1: Delete the files**

Use the OS file system or Unity Editor (right-click → Delete in Project window). Deleting through Unity is preferred — it removes the `.meta` file automatically and updates the asset database.

If deleting via CLI:
```
git rm "Assets/_Scripts/Game/MapGenerator/Steps/Obstacles/CObstacleContentTagStep.cs"
git rm "Assets/_Scripts/Game/MapGenerator/Steps/Obstacles/CObstacleContentTagStep.cs.meta"
```

**Step 2: Check for scene references**

If any scene or prefab had a `CObstacleContentTagStep` component attached, Unity will report a missing script warning. Search Unity's Console for "missing script" after deletion. If found, remove the missing component from the affected GameObjects — the new `CObstacleProximityTagStep` (Task 5) will replace it.

**Step 3: Commit**

```
git add -u
git commit -m "feat: delete CObstacleContentTagStep (replaced by CObstacleProximityTagStep)"
```

---

### Task 5: Create `CObstacleProximityTagStep`

**Files:**
- Create: `Assets/_Scripts/Game/MapGenerator/Steps/Obstacles/CObstacleProximityTagStep.cs`

**Step 1: Create the file**

```csharp
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Tags buildable Land tiles near obstacles of a given type with a configurable EContentTag.
    /// E.g. tiles near Tree obstacles get tagged HuntersLodge, enabling placement of a Hunter's Lodge.
    /// Place after CObstaclePlacementStep / CObstacleClusterStep so ObstacleType is populated.
    /// </summary>
    public class CObstacleProximityTagStep : CMapGenerationStepBase
    {
        [Header("Source")]
        [Tooltip("Only obstacles of this type are used as tag origins.")]
        [SerializeField] private EObstacleType _obstacleType = EObstacleType.Tree;

        [Header("Tag")]
        [Tooltip("Content tag to apply to nearby tiles.")]
        [SerializeField] private EContentTag _contentTag = EContentTag.HuntersLodge;

        [Tooltip("Radius in tiles around each matching obstacle within which tiles are tagged.")]
        [SerializeField] [Min(1)] private int _radius = 3;

        public override string StepName => "Obstacle Proximity Tag";
        public override string StepDescription => "Taguje land políčka v okolí překážek daného typu zadaným EContentTag.";

        public override void Execute(CMapData mapData, int seed)
        {
            int tagged = 0;
            int radiusSq = _radius * _radius;

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    if (mapData.Get(x, y).ObstacleType != _obstacleType)
                        continue;

                    for (int dx = -_radius; dx <= _radius; dx++)
                    {
                        for (int dy = -_radius; dy <= _radius; dy++)
                        {
                            if (dx * dx + dy * dy > radiusSq) continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (!mapData.IsValid(nx, ny)) continue;

                            STile neighbor = mapData.Get(nx, ny);
                            if (neighbor.Type != ETileType.Land) continue;
                            if (neighbor.IsObstacleBlocked) continue;
                            if (neighbor.ContentTag != EContentTag.None) continue;

                            neighbor.ContentTag = _contentTag;
                            mapData.Set(nx, ny, neighbor);
                            tagged++;
                        }
                    }
                }
            }

            Debug.Log($"[{StepName}] Tagged {tagged} tiles (obstacle={_obstacleType}, tag={_contentTag}, radius={_radius}).");
        }
    }
}
```

**Step 2: Verify in Unity Editor**

- No compile errors in Console.
- Add a `CObstacleProximityTagStep` component to the map generator GameObject (after the obstacle placement steps in the pipeline).
- Inspector shows `Obstacle Type`, `Content Tag`, and `Radius` fields.
- Click "Generate Up To This Step" — Console logs `Tagged N tiles (obstacle=Tree, tag=HuntersLodge, radius=3)`.
- If the deleted `CObstacleContentTagStep` was used in any scene, re-add it as `CObstacleProximityTagStep` with matching settings.

**Step 3: Commit**

```
git add "Assets/_Scripts/Game/MapGenerator/Steps/Obstacles/CObstacleProximityTagStep.cs"
git add "Assets/_Scripts/Game/MapGenerator/Steps/Obstacles/CObstacleProximityTagStep.cs.meta"
git commit -m "feat: add CObstacleProximityTagStep replacing CObstacleContentTagStep"
```

---

## Completion Checklist

- [ ] `ETileType` has only `Water` and `Land`; `IsBuildable()` returns `type == ETileType.Land`
- [ ] `EContentTag` contains `None`, `Coast`, `HuntersLodge`
- [ ] `CCoastalTransitionStep` writes `ContentTag` (not `Type`) and inspector shows `_coastTag` dropdown
- [ ] `CObstacleContentTagStep` is deleted (no missing-script warnings in Editor)
- [ ] `CObstacleProximityTagStep` exists, compiles, and tags tiles correctly in-editor
- [ ] Zero references to `ETileType.Coast` remain in the codebase
