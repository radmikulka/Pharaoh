# Map Generation Step — "Generate" Button Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a "Generate" Inspector button to every `IMapGenerationStep` that runs the full pipeline up to and including that step.

**Architecture:** Create abstract base class `CMapGenerationStepBase` (replaces `MonoBehaviour` in all step classes) with a `[Button]` method that calls a new `CMapGenerator.GenerateUpTo(IMapGenerationStep)` overload. NaughtyAttributes reflection picks up `[Button]` from base classes automatically, so all existing custom editors get the button for free.

**Tech Stack:** Unity C# (URP), NaughtyAttributes `[Button]`, no automated tests (Unity editor-only feature).

---

### Task 1: Add `GenerateUpTo` to `CMapGenerator`

**Files:**
- Modify: `Assets/_Scripts/Game/MapGenerator/CMapGenerator.cs`

**Step 1: Insert after `Clear()` (after line 79), before `// ─── Helpers`**

```csharp
public void GenerateUpTo(IMapGenerationStep stopAfterStep)
{
    var steps = CollectSteps();

    if (steps.Count == 0)
    {
        Debug.LogWarning("[CMapGenerator] No IMapGenerationStep components found on child GameObjects.");
        return;
    }

    _mapData = new CMapData(_width, _height);

    foreach (var step in steps)
    {
        Debug.Log($"[CMapGenerator] Running: {step.StepName}");
        step.Execute(_mapData, _seed);

        if (ReferenceEquals(step, stopAfterStep))
            break;
    }

    Debug.Log($"[CMapGenerator] Done (up to '{stopAfterStep.StepName}') — {_width}×{_height} tiles.");

#if UNITY_EDITOR
    UnityEditor.SceneView.RepaintAll();
#endif
}
```

**Step 2: Commit**

```bash
git add Assets/_Scripts/Game/MapGenerator/CMapGenerator.cs
git commit -m "feat: add CMapGenerator.GenerateUpTo(IMapGenerationStep)"
```

---

### Task 2: Create `CMapGenerationStepBase`

**Files:**
- Create: `Assets/_Scripts/Game/MapGenerator/CMapGenerationStepBase.cs`

```csharp
using NaughtyAttributes;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Base class for all map generation pipeline steps.
    /// Provides a shared "Generate Up To This Step" Inspector button.
    /// </summary>
    public abstract class CMapGenerationStepBase : MonoBehaviour, IMapGenerationStep
    {
        public abstract string StepName { get; }

        public abstract void Execute(CMapData mapData, int seed);

        [Button("Generate Up To This Step")]
        private void GenerateUpToThisStep()
        {
            var generator = GetComponentInParent<CMapGenerator>();
            if (generator == null)
            {
                Debug.LogError($"[{StepName}] CMapGenerator not found in parent.", this);
                return;
            }

            generator.GenerateUpTo(this);
        }
    }
}
```

**Step 2: Commit**

```bash
git add "Assets/_Scripts/Game/MapGenerator/CMapGenerationStepBase.cs" "Assets/_Scripts/Game/MapGenerator/CMapGenerationStepBase.cs.meta"
git commit -m "feat: add CMapGenerationStepBase with GenerateUpToThisStep button"
```

---

### Task 3: Migrate all step classes to `CMapGenerationStepBase`

**Files:**
- `Steps/CBasicLayoutStep.cs` line 12
- `Steps/CVoronoiRegionStep.cs` line 19
- `Steps/CObstaclePlacementStep.cs` line 21
- `Steps/CSpawnMapStep.cs` line 21
- `Steps/CRiverGenerationStep.cs` line 12
- `Steps/CLakeGenerationStep.cs` line 11

In each file change:
```csharp
// before
public class CFooStep : MonoBehaviour, IMapGenerationStep
// after
public class CFooStep : CMapGenerationStepBase
```

**Step 2: Commit**

```bash
git add Assets/_Scripts/Game/MapGenerator/Steps/
git commit -m "refactor: migrate all IMapGenerationStep classes to CMapGenerationStepBase"
```

---

## Testing Checklist (manual, in Unity Editor)

- [ ] No compile errors in Console
- [ ] Select `BasicLayout` child → Inspector shows "Generate Up To This Step" button
- [ ] Click it → only BasicLayout runs (one log line in Console)
- [ ] Select `VoronoiRegions` → button present alongside existing "Spawn Points" / "Recalculate Regions" buttons; clicking runs BasicLayout + Voronoi
- [ ] Steps with custom noise editors (`CBasicLayoutStep`, `CObstaclePlacementStep`) → button visible
- [ ] `CMapGenerator.Generate()` on parent still runs all steps as before
