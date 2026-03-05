# Save Data Migration to ServerData Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Move `CSaveData` and `CMissionResearchSaveData` from `Assets/_Scripts/Common/Save/` (namespace `Pharaoh`) to `Assets/_ServerData/Common/Save/` (namespace `ServerData`).

**Architecture:** Pure file move + namespace rename. No logic changes. Four consumer files need a `using ServerData;` added. Old files and empty folder deleted.

**Tech Stack:** Unity C#, MessagePack

---

### Task 1: Create `CMissionResearchSaveData` in ServerData

**Files:**
- Create: `Assets/_ServerData/Common/Save/CMissionResearchSaveData.cs`

**Step 1: Create the file**

```csharp
// =========================================
// DATE:   02.03.2026
// =========================================

using System;
using MessagePack;

namespace ServerData
{
	[MessagePackObject]
	public class CMissionResearchSaveData
	{
		[Key(0)] public int CurrentKP { get; set; }
		[Key(1)] public long NextKpRegenTimestamp { get; set; }
		[Key(2)] public int[] PurchasedResearchIds { get; set; } = Array.Empty<int>();
	}
}
```

**Step 2: Commit**

```bash
git add "Assets/_ServerData/Common/Save/CMissionResearchSaveData.cs"
git commit -m "feat: add CMissionResearchSaveData to ServerData namespace"
```

---

### Task 2: Create `CSaveData` in ServerData

**Files:**
- Create: `Assets/_ServerData/Common/Save/CSaveData.cs`

**Step 1: Create the file**

```csharp
// =========================================
// DATE:   01.03.2026
// =========================================

using System.Collections.Generic;
using MessagePack;

namespace ServerData
{
    [MessagePackObject]
    public class CSaveData
    {
        /// <summary>
        /// Revealed Voronoi cloud regions per mission.
        /// Key = (int)EMissionId, Value = set of revealed VoronoiRegionIds.
        /// </summary>
        [Key(0)]
        public Dictionary<int, HashSet<int>> RevealedCloudRegions = new();

        /// <summary>
        /// Research save data per mission.
        /// Key = (int)EMissionId.
        /// </summary>
        [Key(1)]
        public Dictionary<int, CMissionResearchSaveData> MissionResearch = new();
    }
}
```

**Step 2: Commit**

```bash
git add "Assets/_ServerData/Common/Save/CSaveData.cs"
git commit -m "feat: add CSaveData to ServerData namespace"
```

---

### Task 3: Update `ISaveManager` reference

**Files:**
- Modify: `Assets/_Scripts/Common/Interfaces/ISaveManager.cs`

**Step 1: Add `using ServerData;` to the file**

Current file uses `CSaveData` in the `Pharaoh` namespace. Add the using directive after existing usings:

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;

namespace Pharaoh
{
    public interface ISaveManager
    {
        CSaveData Data { get; }
        bool HasSave { get; }
        UniTask SaveAsync(CancellationToken ct = default);
        UniTask LoadAsync(CancellationToken ct = default);
    }
}
```

**Step 2: Commit**

```bash
git add "Assets/_Scripts/Common/Interfaces/ISaveManager.cs"
git commit -m "refactor: update ISaveManager to use ServerData.CSaveData"
```

---

### Task 4: Update `CSaveManager` reference

**Files:**
- Modify: `Assets/_Scripts/Application/Save/CSaveManager.cs`

**Step 1: Add `using ServerData;`**

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;

namespace Pharaoh
{
    public class CSaveManager : ISaveManager
    {
        private CSaveData _data = new();

        public CSaveData Data => _data;
        public bool HasSave => false; // TODO: implement when server I/O is added

        public async UniTask SaveAsync(CancellationToken ct = default)
        {
            // TODO: serialize _data to binary (MessagePack) and send to server
            await UniTask.CompletedTask;
        }

        public async UniTask LoadAsync(CancellationToken ct = default)
        {
            // TODO: fetch from server and deserialize into _data
            await UniTask.CompletedTask;
        }
    }
}
```

**Step 2: Commit**

```bash
git add "Assets/_Scripts/Application/Save/CSaveManager.cs"
git commit -m "refactor: update CSaveManager to use ServerData.CSaveData"
```

---

### Task 5: Update `COwnedResearches` reference

**Files:**
- Modify: `Assets/_Scripts/Application/User/OwnedResearches/COwnedResearches.cs`

**Step 1: Read the file first, then add `using ServerData;`**

Open the file, locate the `using` block at the top, add `using ServerData;` alongside existing usings. No other changes needed — class references `CSaveData` and `CMissionResearchSaveData` which are now in `ServerData`.

**Step 2: Commit**

```bash
git add "Assets/_Scripts/Application/User/OwnedResearches/COwnedResearches.cs"
git commit -m "refactor: update COwnedResearches to use ServerData save types"
```

---

### Task 6: Update `CKnowledgePointsRegenService` reference

**Files:**
- Modify: `Assets/_Scripts/Game/Research/CKnowledgePointsRegenService.cs`

**Step 1: Read the file first, then add `using ServerData;`**

Open the file, locate the `using` block at the top, add `using ServerData;`. No other changes — class uses `CSaveData` and `CMissionResearchSaveData`.

**Step 2: Commit**

```bash
git add "Assets/_Scripts/Game/Research/CKnowledgePointsRegenService.cs"
git commit -m "refactor: update CKnowledgePointsRegenService to use ServerData save types"
```

---

### Task 7: Delete old files and empty folder

**Files:**
- Delete: `Assets/_Scripts/Common/Save/CSaveData.cs`
- Delete: `Assets/_Scripts/Common/Save/CSaveData.cs.meta`
- Delete: `Assets/_Scripts/Common/Save/CMissionResearchSaveData.cs`
- Delete: `Assets/_Scripts/Common/Save/CMissionResearchSaveData.cs.meta`
- Delete: `Assets/_Scripts/Common/Save/` (folder + its `.meta`)

**Step 1: Delete the files**

```bash
git rm "Assets/_Scripts/Common/Save/CSaveData.cs"
git rm "Assets/_Scripts/Common/Save/CSaveData.cs.meta"
git rm "Assets/_Scripts/Common/Save/CMissionResearchSaveData.cs"
git rm "Assets/_Scripts/Common/Save/CMissionResearchSaveData.cs.meta"
git rm "Assets/_Scripts/Common/Save.meta"
```

**Step 2: Commit**

```bash
git commit -m "refactor: remove old save data files from Pharaoh namespace"
```

---

### Task 8: Verify Unity compiles cleanly

**Step 1:** Open Unity Editor and check the Console for any compile errors.

Expected: No errors referencing `CSaveData` or `CMissionResearchSaveData`.

If errors appear: they will point to a file that still has `using Pharaoh` resolving the old types — add `using ServerData;` to that file.
