# Naming Audit — Methods, Properties, Fields

**Scope:** Every `.cs` file under `Assets/_Scripts/` (excluding paths containing `AldaEngine`) and `Assets/_ServerData/`.
**Files read:** 271
**Date:** 2026-03-08

---

## 1. Typos

### 1.1 `CPharaohNotifications._saviourNotifications`
**File:** `Assets/_Scripts/Infrastructure/Notifications/CPharaohNotifications.cs`
**Problem:** The private field uses British spelling `_saviourNotifications`, while the class it holds is `CSaviorNotifications` (American spelling). The mismatch will cause confusion when searching the codebase.
**Suggestion:** Rename the field to `_saviorNotifications` to match the class name.

### 1.2 `CDummlyLocalizationProvider` (nested class in `CProjectInstaller`)
**File:** `Assets/_Scripts/Main/Installers/CProjectInstaller.cs`
**Problem:** `Dummly` is not a word; the intent is `Dummy`.
**Suggestion:** Rename to `CDummyLocalizationProvider`.

### 1.3 `CUserCacheTimoutConfig` (class name)
**File:** `Assets/_ServerData/Comminication/Configs/CUserCacheTimoutConfig.cs`
**Problem:** `Timout` is a misspelling of `Timeout`.
**Suggestion:** Rename the class to `CUserCacheTimeoutConfig`.

---

## 2. Vague / Unclear Names

### 2.1 `CBuildConfig.SetBundleEditorTesting()`
**File:** `Assets/_Scripts/Common/Configs/CBuildConfig.cs`
**Problem:** `Set` implies assigning a value, but the method body configures multiple build settings and flags to prepare for bundle editor testing. The name understates what is happening.
**Suggestion:** `PrepareBundleEditorTesting()` or `ConfigureBundleEditorTesting()`.

### 2.2 `CServerConfig.LikeABoss` (property)
**File:** `Assets/_Scripts/Common/Configs/CServerConfig.cs`
**Problem:** The property name is informal slang with no clear technical meaning. It appears to toggle a privileged/debug user preset, but the name gives no indication of that intent.
**Suggestion:** `UsePrivilegedUserPreset` or `UseAdminPreset`.

### 2.3 `CStartupQueue.RunAsync()`
**File:** `Assets/_Scripts/Game/GameController/CStartupQueue.cs`
**Problem:** `Run` alone is a vague verb (prohibited by audit rules). The method waits a moment then activates the deferred action queue and fires `CCoreGameUnlockedSignal`.
**Suggestion:** `ActivateQueueAsync()` or `UnlockCoreGameAsync()`.

---

## 3. Bool Convention Violations

### 3.1 `COwnedValuable.HaveValuable()`
**File:** `Assets/_Scripts/Application/User/OwnedValuables/COwnedValuable.cs`
**Problem:** Returns `bool`. The verb `Have` is grammatically wrong for a predicate; the convention requires `Is`, `Has`, `Can`, `Try`, or `Should`.
**Suggestion:** `HasValuable()`.

### 3.2 `CDebugUserDeletionHandler.WillDeleteUserInThisSession()`
**File:** `Assets/_Scripts/Infrastructure/Loading/CDebugUserDeletionHandler.cs`
**Problem:** Returns `bool`. `Will` is not in the allowed prefix list (`Is`, `Has`, `Can`, `Try`, `Should`).
**Suggestion:** `IsMarkedForDeletion()` or `ShouldDeleteUserThisSession()`.

---

## 4. Misleading Names

### 4.1 `CCameraBorder.OnMissionLoaded`
**File:** `Assets/_Scripts/Game/Camera/CameraBorders/CCameraBorder.cs`
**Problem:** This event handler responds to `CMissionActivatedSignal`, not a "loaded" signal. The signal is fired after the mission scene is activated, not at load completion. The name implies a different lifecycle event.
**Suggestion:** `OnMissionActivated`.

---

## 5. Redundant / Wrong Prefix in Member Names

### 5.1 `EScreenId.CCoreGameOverlay`
**File:** `Assets/_Scripts/Common/Enums/EScreenId.cs`
**Problem:** The enum member `CCoreGameOverlay` uses the `CC` class-naming prefix, which should never appear on an enum value. Enum members use PascalCase without type prefixes.
**Suggestion:** `CoreGameOverlay`.

---

## 6. Other

### 6.1 `CUiValuable.Valuable` (private field without underscore)
**File:** `Assets/_Scripts/UI/Vault/CUiValuable.cs`
**Problem:** The private field `Valuable` is declared with PascalCase instead of the required `_camelCase` convention for private fields (`_valuable`).
**Suggestion:** Rename to `_valuable`.

### 6.2 `CRegionInstaller.mission` (private serialized field without underscore)
**File:** `Assets/_Scripts/Main/Installers/CRegionInstaller.cs`
**Problem:** The `[SerializeField, Child] private CMissionController mission` field is missing the `_` prefix required by the project's naming convention for private fields.
**Suggestion:** Rename to `_mission`.

### 6.3 `CTouchesDb.GetFirstTwoPointers()` — tuple element names with underscore prefix
**File:** `Assets/_Scripts/Infrastructure/Input/CTouchesDb.cs`
**Problem:** The return type declares tuple element names `_newTouchA` and `_newTouchB` with underscore prefixes (e.g., `(Touch _newTouchA, Touch _newTouchB)`). Tuple element names are public-facing identifiers and should follow PascalCase or camelCase, not the private-field underscore convention.
**Suggestion:** `(Touch touchA, Touch touchB)`.

---

## Summary

| Category | Count |
|---|---|
| 1. Typos | 3 |
| 2. Vague / unclear names | 3 |
| 3. Bool convention violations | 2 |
| 4. Misleading names | 1 |
| 5. Redundant / wrong prefix | 1 |
| 6. Other (field naming, tuple names) | 3 |
| **Total** | **13** |
