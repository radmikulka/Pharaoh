# Naming Audit — Pharaoh Codebase

> **Scope:** All scripts under `Assets/_Scripts/` excluding AldaEngine and ServerData layers.
> **Format:** `Old name → New name | Reason`
> **Status:** Proposal only — no code has been changed.

---

## 1. Folders (typos, inconsistency)

| Old path | New path | Reason |
|---|---|---|
| `Main/Insntallers/` | `Main/Installers/` | typo |
| `UI/Valut/` | `UI/Vault/` | typo |

---

## 2. Classes / files — critical (developer name, structural issue)

| Old name | New name | Reason |
|---|---|---|
| `CRadekTest` | *(delete)* | developer first name in production code |
| `IIHaveCullingGroup` | `ICullingGroupOwner` | double `II` prefix; verb "Have" in interface name; new name describes the role |

---

## 3. Classes / files — redundant words in name

| Old name | New name | Reason |
|---|---|---|
| `CCoreGameGameMode` | `CCoreGameMode` | "CoreGame" + "Game" = redundant repetition |
| `CCoreGameGameModeData` | `CCoreGameModeData` | same as above |
| `CMenuManagerStateRequest` | `CMenuStateRequest` | "Manager" is an implementation detail, not part of the request's identity |
| `CMenuManagerStateResponse` | `CMenuStateResponse` | same as above |
| `CRequiredBundlesDownloader` | `CMissionBundleDownloader` | "Required" says nothing; this downloads bundles for a mission |
| `CDefaultBlitMaterial` | `CBlitMaterial` | "Default" is redundant — it is the only implementation |

---

## 4. Classes / files — `CUi` prefix

> **Decision (2026-03-07):** `CUi` prefixes are intentionally kept. UI scripts carry this prefix as part of the UI layer naming convention.

*(no changes)*

---

## 5. Classes / files — `CScreen` prefix

> **Decision (2026-03-07):** `CScreen` prefixes are intentionally kept. Screen scripts carry this prefix as part of the UI layer naming convention.

*(no changes)*

---

## 6. Classes / files — vague terms

| Old name | New name | Reason |
|---|---|---|
| `CLazyActionQueue` | `CDeferredActionQueue` | "Lazy" is ambiguous; "Deferred" = execution postponed until later |
| `ILazyAction` | `IDeferredAction` | same as above |
| `CPlayUniTaskLazyAction` | `CUniTaskDeferredAction` | same as above |
| `CValidServerConnectionPostprocess` | `CServerConnectionInitializer` | "Postprocess" is a vague process term; the class initialises data after a successful connection |
| `CPingPongFullScreenOverlayTask` | `CFadeFullScreenOverlayTask` | "PingPong" implies looping; this is a single fade-in → action → fade-out sequence |
| `CFreezeRenderer` | `CFrameCaptureRenderer` | "Freeze" does not describe what happens to the frame; this captures + blurs the background |
| `CFreezeRendererCamera` | `CFrameCaptureCamera` | same as above |
| `CFreezeRendererFeature` | `CFrameCaptureRendererFeature` | same as above |
| `CActivateFreezeRendererTask` | `CActivateFrameCaptureTask` | same as above |
| `CDeactivateFreezeRendererTask` | `CDeactivateFrameCaptureTask` | same as above |
| `CMenuBackgroundBlurTrigger` | `CBackgroundBlurTrigger` | "Menu" is an unnecessary qualifier |
| `CEditorTimeHacks` | `CEditorTimeScaleDebugger` | "Hacks" has negative connotations; inappropriate for permanent code |

---

## 7. Classes / files — analytics naming

| Old name | New name | Reason |
|---|---|---|
| `CLoadingTechFlow` | `CLoadingFunnelTracker` | "TechFlow" is not intuitive; this tracks a loading funnel |
| `CServicesTechFlow` | `CServiceFunnelTracker` | same as above |
| `ELoadingTechFlow` | `ELoadingFunnelStep` | enum values = funnel steps, not flow types |
| `EServiceTechnicalFlow` | `EServiceFunnelStep` | same as above; also inconsistent with `ELoadingTechFlow` (Technical vs Tech) |
| `SLoadingTechFunnelStep` | `SLoadingFunnelStep` | "Tech" is an unnecessary prefix in the struct name |
| `SServiceTechFunnelStep` | `SServiceFunnelStep` | same as above |

---

## 8. Classes / files — spelling / consistency

| Old name | New name | Reason |
|---|---|---|
| `CSaviourNotifications` | `CSaviorNotifications` | British spelling "Saviour" → American "Savior" (consistent with rest of codebase) |

---

## 9. Interfaces

| Old name | New name | Reason |
|---|---|---|
| `IAnimatedCurrency` | `IAnimatedValuable` | The implementation is named `CAnimatedValuable`; the interface must match |

---

## 10. Methods / Properties

| Class | Old signature | New signature | Reason |
|---|---|---|---|
| `COwnedValuables` | `GetOrCrateValuable(EValuable)` | `GetOrCreateValuable(EValuable)` | typo — "Crate" instead of "Create" (confirmed in file, used internally 4×) |
| `CAnimatedCurrency` | `bool BidingLocked` | `bool IsAnimationLocked` | "Biding" is a typo / archaism; bool properties use `Is…` convention; intent is now clear |
| `CAnimatedCurrency` | `AddBidingLock(CLockObject)` | `AddAnimationLock(CLockObject)` | same as above |
| `CAnimatedCurrency` | `RemoveBidingLock(CLockObject)` | `RemoveAnimationLock(CLockObject)` | same as above |
| `CFullScreenOverlay` | `PingPong(...)` | `FadeInAndOut(...)` | "PingPong" metaphor; method does fade-in → callback → fade-out |
| `CValidServerConnectionPostprocess` | `PostprocessServerConnection(...)` | `InitializeServerConnection(...)` | name follows the renamed class |

---

## 11. ServerHitter — terminology

> **Note:** `CRequestHit`, `CResponseHit`, `IHit` are AldaEngine types — they are NOT renamed. Only project-level wrapper classes are renamed.

| Old name | New name | Reason |
|---|---|---|
| `ServerHitter/` (folder) | `ServerApi/` | "Hitter" is jargon; "Api" describes what the system is |
| `CHitRecord` | `CRequestRecord` | "Hit" = jargon; this is a record of a pending request |
| `CHitBuilder` | `CRequestSender` | builds and sends requests; "Builder" is misleading (the class also sends) |
| `CHitRecordBuilder` | `CRequestBuilder` | fluent builder for configuring a request |
| `CHitsDispatcher` | `CRequestDispatcher` | dispatches batches of requests over TCP |
| `CHitsQueue` | `CRequestQueue` | queue of pending requests |
| `CHitRecordsGroup` | `CRequestBatch` | a group of requests sent together |
| `CAsyncHitResponse<T>` | `CServerResponse<T>` | async wrapper for a typed server response |

---

## What is NOT proposed for change

| Item | Reason |
|---|---|
| `CRequestHit`, `CResponseHit`, `IHit` | AldaEngine TCP layer types; changing them would break framework compatibility |
| `Blit` in renderers | Correct graphics term (Unity API) |
| `FreezeRender()` on `IMainCamera` | **Verified: this is project code** (`Assets/_Scripts/Common/Interfaces/IMainCamera.cs:15`), not AldaEngine. Can be renamed if the `CFreezeRenderer` → `CFrameCaptureRenderer` rename is adopted. |
| Signal/Task naming (`CCoreGameLoadedSignal`, `CLoadGameModeTask`) | Consistent and clear throughout the codebase |
| `CUserProgress`, `CAccount`, `CUser` | Good, simple names |
