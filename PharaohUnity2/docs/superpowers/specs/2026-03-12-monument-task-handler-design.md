# Monument Task Handler System

## Context

Workers currently loop mindlessly: walk to monument, deliver (0.5s), walk back, repeat. There is no connection between worker trips and actual monument construction progress. The monument has pre-generated cells (grid subdivisions) that represent buildable pieces, but nothing assigns them to workers.

This design introduces a **task handler** — an intermediary between workers and the monument — that assigns individual cell-building tasks to workers, tracks completion, and stops workers when the monument is fully built.

## Design Decisions

| Decision | Choice |
|----------|--------|
| Task granularity | 1 cell = 1 task |
| Task ordering | Strict sequential (global queue across all parts) |
| Routing | Shared route + final target position (cell world pos) |
| Visual on completion | None yet (internal tracking only) |
| When all done | Workers stop (handler returns no task) |
| Logic ownership | CWorkerManager (not on CWorker or states) |
| Transition detection | Check state type after Tick() |

## Architecture

### New: `SMonumentTask` (struct)

```
Assets/_Scripts/Game/CoreGame/Monument/SMonumentTask.cs
```

- `int Index` — position in the global cell queue
- `Vector3 Target` — cell's world position (last-leg destination)

### New: `CMonumentTaskHandler` (plain class, NOT IInitializable)

```
Assets/_Scripts/Game/CoreGame/Monument/CMonumentTaskHandler.cs
```

Responsibilities:
- `void Initialize(CMonumentInstance monument)` — called explicitly by `CWorkerManager.OnCoreGameUnlocked()` after verifying monument is ready. Flattens cells from all parts (skipping `CutoutOnly` parts and parts with null/empty Cells) into a single ordered list. Reads cell world positions via `cell.transform.position`.
- `bool TryAssignTask(out SMonumentTask task)` — returns next unassigned cell, advances index. Returns false when all assigned.
- `void CompleteTask(int index)` — increments completed counter.
- `bool IsComplete` — true when all cells completed.

Internal state:
- `Vector3[] _cellPositions` — flattened cell positions (from `cell.transform.position`)
- `int _nextIndex` — next cell to assign
- `int _completedCount` — cells completed so far

### New: `CWorkerIdleState`

```
Assets/_Scripts/Game/CoreGame/Workers/States/CWorkerIdleState.cs
```

- `Tick()` returns `this` (does nothing). Manager handles task assignment.

### New: `CWorkerWalkingToTargetState`

```
Assets/_Scripts/Game/CoreGame/Workers/States/CWorkerWalkingToTargetState.cs
```

- Walks from current position toward `worker.CurrentTask.Target`.
- Reuse `CWorkerMovement.MoveAlongPath` with a single-element waypoint array `[task.Target]` for consistency.
- On arrival → transitions to `CWorkerDeliveringState`.

### Modified: `CWorker`

```
Assets/_Scripts/Game/CoreGame/Workers/CWorker.cs
```

- Add field: `SMonumentTask? CurrentTask`

### Modified: `CWorkerManager.Update()`

After ticking all states, manager checks transitions:

```
for each worker:
  previousState = worker.State
  worker.State = previousState.Tick(worker, dt, speed)

  if worker.State is CWorkerIdleState:
    // Worker is idle (just returned from storage, or still waiting for a task)
    TryAssignNextTask(worker)

  if worker.State is CWorkerDeliveringState && previousState is CWorkerWalkingToTargetState:
    // Worker just arrived at cell — will complete task when delivering finishes

  if worker.State is CWorkerWalkingToStorageState && previousState is CWorkerDeliveringState:
    // Delivering just ended → complete the task
    _handler.CompleteTask(worker.CurrentTask.Value.Index)
    worker.CurrentTask = null
```

New method:
```
TryAssignNextTask(CWorker worker):
  if _handler.TryAssignTask(out var task):
    worker.CurrentTask = task
    worker.State = new CWorkerWalkingToMonumentState()
    worker.WaypointIndex = 0
  // else: worker stays idle
```

### Modified: `CWorkerManager.SpawnWorker()`

- Initial state: `CWorkerIdleState` (not WalkingToMonument)
- Immediately call `TryAssignNextTask()` after spawn

### Modified: `CWorkerManager.OnCoreGameUnlocked()`

- Call `_handler.Initialize(_monumentSpawner.Monument)` explicitly (handler is a plain class, not IInitializable — avoids init ordering issues with CMonumentSpawner)
- Rest stays the same

### Modified: `CWorkerWalkingToMonumentState`

- On reaching end of shared route → transition to `CWorkerWalkingToTargetState` (not Delivering)

### Modified: `CWorkerDeliveringState`

- After 0.5s → transition to `CWorkerWalkingToStorageState` (unchanged)

### Modified: `CWorkerWalkingToStorageState`

- On reaching end of return route → transition to `CWorkerIdleState` (not WalkingToMonument)

### DI Wiring: `CMissionInstaller`

- `Container.AddSingleton<CMonumentTaskHandler>()` (no `IInitializable` — initialized explicitly by CWorkerManager)

## State Machine Flow

```
CWorkerIdleState
  ↓ (manager assigns task)
CWorkerWalkingToMonumentState (shared route)
  ↓ (reached end of route)
CWorkerWalkingToTargetState (walk to cell position)
  ↓ (reached cell)
CWorkerDeliveringState (0.5s wait)
  ↓ (manager calls CompleteTask)
CWorkerWalkingToStorageState (shared route back)
  ↓ (reached storage)
CWorkerIdleState (loop or stop if no tasks)
```

## Files to Create

1. `Assets/_Scripts/Game/CoreGame/Monument/SMonumentTask.cs`
2. `Assets/_Scripts/Game/CoreGame/Monument/CMonumentTaskHandler.cs`
3. `Assets/_Scripts/Game/CoreGame/Workers/States/CWorkerIdleState.cs`
4. `Assets/_Scripts/Game/CoreGame/Workers/States/CWorkerWalkingToTargetState.cs`

## Files to Modify

1. `Assets/_Scripts/Game/CoreGame/Workers/CWorker.cs` — add CurrentTask field
2. `Assets/_Scripts/Game/CoreGame/Workers/CWorkerManager.cs` — inject handler, rewrite Update with transition checks, change SpawnWorker initial state
3. `Assets/_Scripts/Game/CoreGame/Workers/States/CWorkerWalkingToMonumentState.cs` — transition to WalkingToTarget instead of Delivering
4. `Assets/_Scripts/Game/CoreGame/Workers/States/CWorkerWalkingToStorageState.cs` — transition to Idle instead of WalkingToMonument
5. `Assets/_Scripts/Main/Installers/CoreGame/CMissionInstaller.cs` — bind CMonumentTaskHandler

## Verification

1. Open Unity, load a scene with a monument that has parts with cells
2. Enter play mode, trigger CCoreGameUnlockedSignal
3. Verify workers spawn idle, get assigned tasks, walk shared route then to cell position
4. Verify cells are completed in sequential order (check handler's completed count in debugger)
5. Verify workers stop when all cells are exhausted
6. Add Debug.Log in handler to trace task assignment and completion
