# Save Data Migration to ServerData

**Date:** 2026-03-02

## Goal

Move `CSaveData` and `CMissionResearchSaveData` from `Assets/_Scripts/Common/Save/` (namespace `Pharaoh`) to `Assets/_ServerData/Common/Save/` (namespace `ServerData`), because save data is a pure DTO layer that will be synchronized with the server.

## What Moves

| From | To |
|------|-----|
| `Assets/_Scripts/Common/Save/CSaveData.cs` | `Assets/_ServerData/Common/Save/CSaveData.cs` |
| `Assets/_Scripts/Common/Save/CMissionResearchSaveData.cs` | `Assets/_ServerData/Common/Save/CMissionResearchSaveData.cs` |

- Namespace changes: `Pharaoh` → `ServerData`
- Old files and empty folder deleted

## What Stays

- `ISaveManager` remains in `Assets/_Scripts/Common/Interfaces/` — it is an application-layer interface
- `CSaveManager` remains in `Assets/_Scripts/Application/Save/`

## Reference Updates (add `using ServerData;`)

| File | Path |
|------|------|
| `ISaveManager.cs` | `Assets/_Scripts/Common/Interfaces/` |
| `CSaveManager.cs` | `Assets/_Scripts/Application/Save/` |
| `COwnedResearches.cs` | `Assets/_Scripts/Application/User/OwnedResearches/` |
| `CKnowledgePointsRegenService.cs` | `Assets/_Scripts/Game/Research/` |

## Constraints

- No logic changes — only namespace and file location
- Classes remain MessagePack-serialized DTOs with primitive types only
