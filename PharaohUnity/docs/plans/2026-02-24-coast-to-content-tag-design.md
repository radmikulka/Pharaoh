# Design: Move Coast from ETileType to EContentTag

**Date:** 2026-02-24

## Summary

Remove `ETileType.Coast` so that `ETileType` contains only `Water` and `Land`. The concept of "coastal zone" moves into the tag system (`EContentTag.Coast`). `CCoastalTransitionStep` is updated to write `ContentTag` instead of `Type`. `CObstacleContentTagStep` is replaced by a new `CObstacleProximityTagStep` that follows the same style as `CCoastalTransitionStep`.

## Motivation

- `ETileType` should represent only fundamental terrain classification (water vs. land), not visual/gameplay zone metadata.
- Coast is a zone concept, not a tile type — it belongs alongside other content tags like `HuntersLodge`.
- Consolidating zone metadata into `EContentTag` makes the tag system the single source of truth for buildable-zone restrictions.

## Data Type Changes

### `ETileType.cs`
- Remove `Coast = 2`.
- Simplify `IsBuildable()` to `type == ETileType.Land`.

```csharp
public enum ETileType { Water, Land }

public static bool IsBuildable(this ETileType type) => type == ETileType.Land;
```

### `EContentTag.cs`
- Add `Coast` value.

```csharp
public enum EContentTag
{
    None = 0,
    Coast,
    HuntersLodge,
}
```

`STile` requires no changes — `ContentTag` field already exists.

## Step Changes

### `CCoastalTransitionStep.cs`

Add `[SerializeField] EContentTag _coastTag = EContentTag.Coast;` (inspector-configurable).

Logic stays ring-based BFS:
- **Ring 1:** Land tiles cardinally adjacent to `ETileType.Water` → set `ContentTag = _coastTag`
- **Ring 2..N:** Land tiles cardinally adjacent to a tile with `ContentTag == _coastTag` → set `ContentTag = _coastTag`
- Skip tiles that already have `ContentTag != None`.

`HasCardinalNeighborOfType` is split into two helpers:
- `HasCardinalNeighborOfTileType(mapData, x, y, ETileType)` — used for ring 1
- `HasCardinalNeighborWithTag(mapData, x, y, EContentTag)` — used for rings 2..N

### `CObstacleContentTagStep.cs` → DELETE

Replaced by `CObstacleProximityTagStep`.

### New: `CObstacleProximityTagStep.cs`

Location: `Assets/_Scripts/Game/MapGenerator/Steps/Obstacles/`

Inspector fields:
```csharp
[Header("Source")]
[SerializeField] EObstacleType _obstacleType;

[Header("Tag")]
[SerializeField] EContentTag _contentTag;
[SerializeField] [Min(1)] int _radius = 3;
```

Logic:
- Scan all tiles for `ObstacleType == _obstacleType`.
- For each match, iterate all tiles within circular radius `_radius`.
- If neighbor is `Land`, not `IsObstacleBlocked`, and `ContentTag == None` → set `ContentTag = _contentTag`.

## Ripple Effects

| File | Change |
|------|--------|
| `ETileType.cs` | Remove `Coast`, simplify `IsBuildable()` |
| `EContentTag.cs` | Add `Coast` |
| `CCoastalTransitionStep.cs` | Rewrite to write `ContentTag` |
| `CObstacleContentTagStep.cs` | Delete |
| `CObstacleProximityTagStep.cs` | Create new |

Files using `IsBuildable()` (`CObstaclePlacementStep`, `CObstacleClusterStep`) automatically get the updated semantics — no manual changes needed.
