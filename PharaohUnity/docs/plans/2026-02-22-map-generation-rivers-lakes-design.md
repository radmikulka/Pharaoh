# Map Generation: Rivers & Lakes

## Goal

Add river and lake generation to the existing editor-driven map pipeline. Rivers are noise-carved paths of water tiles; lakes are flood-fill grown organic shapes. Both integrate as new `IMapGenerationStep` components with no changes to existing code.

## Pipeline Order

```
Step 0: CBasicLayoutStep          (existing - land/water noise layout)
Step 1: CRiverGenerationStep      (NEW)
Step 2: CLakeGenerationStep       (NEW)
Step 3: CVoronoiRegionStep        (existing - biome assignment)
Step 4: CObstaclePlacementStep    (existing - obstacle placement)
Step 5: CSpawnMapStep             (existing - instantiation + tag computation)
```

Rivers and lakes run before biome assignment so Voronoi correctly skips water tiles. No existing steps require modification.

## CRiverGenerationStep

Carves 1-3 natural-looking river paths through land tiles.

### Algorithm

1. Pick entry/exit points on opposite map edges (must be land tiles).
2. Generate a noise field using FastNoiseLite with a unique seed.
3. Walk from entry toward exit. At each step, sample noise to add lateral drift perpendicular to the general direction, creating a winding path.
4. Convert walked tiles to `ETileType.Water`. Optionally widen by converting 1 adjacent neighbor.

### Parameters

| Field | Type | Description |
|-------|------|-------------|
| `_riverCount` | int (1-3) | Number of rivers |
| `_windiness` | float (0-1) | Lateral noise drift strength |
| `_maxWidth` | int (1-2) | River width in tiles |
| `_noiseSeed` | int | Seed for reproducibility |
| `_noiseFrequency` | float | Drift frequency |

### Edge Cases

- If entry/exit tile is water, resample.
- If path walks off-map, clamp to valid coordinates.
- Rivers can cross each other (both are water).

## CLakeGenerationStep

Places 1-2 organic-shaped inland lakes.

### Algorithm

1. Pick random land tiles as seeds, at least `_minDistanceFromEdge` from map edges.
2. Flood-fill outward from seed. At each expansion step, use noise threshold to decide inclusion, producing irregular shapes.
3. Stop when lake reaches target size (random in `_minSize` to `_maxSize`).
4. Convert all filled tiles to `ETileType.Water`.

### Parameters

| Field | Type | Description |
|-------|------|-------------|
| `_lakeCount` | int (0-3) | Number of lakes |
| `_minSize` / `_maxSize` | int | Lake size range in tiles (e.g., 3-8) |
| `_minDistanceFromEdge` | int | Minimum tiles from map edge |
| `_minDistanceBetweenLakes` | int | Prevents merging |
| `_irregularity` | float (0-1) | Shape jaggedness (0=circular, 1=very irregular) |
| `_noiseSeed` | int | Seed for reproducibility |

### Edge Cases

- If seed lands on water (ocean or river), resample.
- If growth blocked by edges or other lakes, stop early.
- Lakes touching rivers merge naturally.

## Impact on Existing Code

No changes to existing code. River/lake tiles are standard water tiles:

- `CMapCell.ComputeCellTags()` — already detects water neighbors for NearWater/Coastal tags.
- `CBuildingPlacementValidator` — already prevents building on water.
- `CVoronoiRegionStep` — already skips water via `_skipWaterTiles`.
- `CObstaclePlacementStep` — only places on land.
- `CSpawnMapStep` — handles water tiles normally.

## New Files

1. `Assets/_Scripts/Game/MapGenerator/Steps/CRiverGenerationStep.cs`
2. `Assets/_Scripts/Game/MapGenerator/Steps/CLakeGenerationStep.cs`

## Map Scale

Target: 40-60 cell maps with 1-3 rivers and 1-2 lakes.
