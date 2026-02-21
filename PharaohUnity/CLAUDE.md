# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Pharaoh** is a mobile game (Android/iOS) built with Unity (URP). It uses the **AldaEngine** proprietary framework with **Zenject** for dependency injection.

## Build & Development

This is a Unity project — there are no CLI build commands. Open the project in **Unity Editor** (check `ProjectSettings/ProjectVersion.txt` for the required Unity version). Building is done via Unity's Build Settings (`File > Build Settings`).

Key asset bundle operations are performed through Unity's editor menus provided by the AldaEngine framework.

## Code Architecture

### Layer Structure (`Assets/_Scripts/`)

```
Main/           → Zenject installer setup (DI composition root)
Infrastructure/ → Platform services, scene loading, rendering, input
Application/    → Game modes, user state, server data
Game/           → Mission logic, camera, culling, game-specific systems
UI/             → Screen management, HUD, overlays
Common/         → Signals, configs, interfaces, shared utilities
```

### Dependency Injection (Zenject)

The DI tree is composed of scene-scoped installers. The root is `CProjectInstaller` (project-level prefab), which binds core services. Scene-specific installers extend per-scene:

- `CProjectInstaller` — services (Firebase, Ads, Purchasing), EventBus, SceneManager, BundleManager
- `CConnectionScreenInstaller` — connection/auth screen
- `CBaseGameInstaller` — base game environment
- `CCoreGameInstaller` — camera, culling groups, startup queue
- `CRegionInstaller` — mission/region-specific bindings
- `CUiInstaller` — screen manager, UI components

### Signal/Event System

Communication between systems uses a **publish-subscribe event bus** (`IEventBus` / `CRootEventBus`). Two signal types exist:

- **Signals** (fire-and-forget): e.g. `CCoreGameLoadedSignal`, `CTapSignal`, `CSceneLoadedSignal`
- **Tasks** (command pattern, can be async): e.g. `CLoadGameModeTask`, `CShowScreenTask`, `CCloseAllScreensTask`

```csharp
_eventBus.Subscribe<TSignal>(handler);
_eventBus.AddTaskHandler<TTask, TResponse>(handler);
_eventBus.AddAsyncTaskHandler<TTask>(asyncHandler);
```

Signals are defined in `Assets/_Scripts/Common/Signals/`.

### Scene Loading Sequence

Scenes are managed by `CSceneManager`. The startup flow is:

1. `_Blank` (entry) → installs project DI
2. `_ConnectingScreen` (additive) → auth/server connection
3. `_BaseGame` (additive, pre-loaded) → game environment
4. `_CoreGame` (additive) → core gameplay, triggers `CGameModeStartedSignal`
5. `_Ui` (additive) → UI overlay

`_PreventStripping` exists solely to prevent IL2CPP stripping of referenced types.

### Game Mode System

Game modes are created via `CGameModeFactory` and managed by `CGameModeManager`. To trigger a game mode load, publish `CLoadGameModeTask` on the event bus. The active game modes are `CoreGame` and `RegionLiveEvent` (from `EGameModeId`).

### Screen/UI Management

`CScreenManager` manages a stack of `IUiScreen` instances. Screens are shown/hidden via event bus tasks (`CShowScreenTask`, `CTryCloseActiveScreenTask`). Screens that implement `IEscapable` handle back button/escape key.

### GoTo FSM

`CGoToFsm` is a sequential state machine used for orchestrating multi-step user flows (e.g., "navigate to mission after closing menus"). States are pushed onto `CGoToFsmStack` and executed in order. States can block input via `IGoToHandler`.

### Config System

All game configuration flows through `CResourceConfigs` (a `ScriptableObject` injected at the project level). Sub-configs include:
- `CSceneResourceConfig` — scene bundle requirements and loading mode
- `CMissionConfig` — mission data
- `CValuableResourceConfig` — currencies/resources
- `CServerConfig`, `CBuildConfig`, `CDebugConfig` — runtime/build settings

## Naming Conventions

| Type | Prefix | Example |
|------|--------|---------|
| Classes | `C` | `CSceneManager` |
| Interfaces | `I` | `ISceneManager` |
| Enums | `E` | `EGameModeId` |
| Structs | `S` | `SValueChangeArgs` |
| Private fields | `_camelCase` | `_eventBus` |
| Public properties | `PascalCase` | `ActiveMission` |

## Key External Dependencies

- **AldaEngine** — proprietary framework (not in this repo); provides base classes for DI, screen management, TCP, localization
- **ServiceEngine** — proprietary services wrapper for Firebase, Ads, Analytics, Purchasing, Singular
- **Zenject** — dependency injection
- **UniTask** — async/await (prefer over coroutines)
- **DoTween** — tweening animations
- **Cinemachine 3.x** — camera system
- **Newtonsoft JSON** — serialization

## Pending Features (TODO.txt)

Settings system, localizations, rewards queue, rate-us, dialog system.