## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-09` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/RUN/RUN_BLACKBOARD.md`.

## Task: 2026-05-14 Five Monster NewRunScene Prefab Binding Fix

### Task title

Finish five-monster prefab binding and HP bar visibility for NewRunScene entry.

### Goals

- Remove the trailing whitespace that caused the previous Code Reviewer failure.
- Bind Ariel, Eve, Sein, Vega, and Rin prefabs on `NewRunSceneEntryManager`.
- Make the prefab `MonsterHpBar` render by assigning a real sprite to HP bar renderers.
- Verify all five selected monster IDs can create a model and initialize `MonsterUnitActor`.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode gameplay verification; user owns click-flow and visual Play Mode checks.
- Keep current UI flow unchanged.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies the five monster selection flow in Play Mode.
- Later combat work should update model resources and call `MonsterUnitActor.RefreshDebugView()` after HP/shield changes.

### Evidence

- Updated `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` to assign `fallbackCatalog` and all five prefab fields: `arielUnitPrefab`, `eveUnitPrefab`, `rinUnitPrefab`, `seinUnitPrefab`, and `vegaUnitPrefab`.
- Added `Pakuri/Assets/Prefab/Monster/MonsterHpBarPixel.png` and assigned it to `Background`, `Fill`, and `Shield` SpriteRenderers in all five monster unit prefabs.
- Updated `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` so fallback catalog data can be used before CSV runtime initialization and monster resolution requires exact ID matches.
- Unity-MCP verification returned `modelOk=True`, matching `model=ariel/eve/sein/vega/rin`, `actorModel=True`, `bgSprite=True`, `fillSprite=True`, and `shieldSprite=True` for all five IDs.
- `git diff --check` over the changed scene, prefabs, and entry scripts completed with exit code 0 and only LF-to-CRLF warnings.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.
- 2026-05-14 follow-up: User manually adjusted the five monster prefab `MonsterHpBar` Scale values. `Select-String` evidence found `MonsterHpBar` root scales around `{x: 3.3, y: 1.7, z: 1.35}` and `Background` / `Fill` scales of `{x: 20, y: 2.5, z: 1}` in the monster prefabs.
- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html` to state that `MonsterHpBar` Scale and visible size are user-authored prefab responsibility, and Code Builder must not overwrite them in runtime code.
- 2026-05-14 follow-up: Updated `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` so HP and shield fill X scale use `Background.localScale.x * normalizedValue` instead of writing the normalized value directly.
- Unity-MCP editor code verified all five prefabs: `Ariel_Unit`, `Eve_Unit`, `Sein_Unit`, `Vega_Unit`, and `Rin_Unit` returned `bgX=20`, `fullFillX=20`, and `halfFillX=10`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and a follow-up single `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: User asked Code Builder to remove trailing whitespace, verify five prefab Actor/Model binding, and fix invisible `MonsterHpBar`.
- 2026-05-14: User said they directly modified `MonsterHpBar` Scale and asked to update the InGame build roadmap report.
- 2026-05-14: User reported that `HpFill` was forced to `1` when entering from NewMainScene to NewGameScene and asked Code Builder to make it match the `Background` Scale.

## Task: 2026-05-14 NewRunScene Phase2-B Actor Model Binding

### Task title

Bind the selected 1P monster prefab actor to a runtime unit model on NewRunScene entry.

### Goals

- Create the selected monster `MonsterUnitRuntimeModel` from the current CSV/Data runtime catalog during `NewRunScene` entry.
- Spawn the selected monster prefab at `1PSpawnPoint`.
- Inject the created model into the spawned prefab's `MonsterUnitActor`.

### Constraints

- Role Owner is Code Builder.
- The user already added Eve prefab HP/name debug children and `MonsterUnitActor`; this task does not redesign UI.
- `MonsterUnitRuntimeModel` is a plain runtime C# model and is not a prefab-inspector component.
- Code Reviewer was not run because the user did not explicitly request Reviewer execution for this slice.
- Do not run Unity Play Mode; user verifies gameplay flow.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User manually assigns Ariel/Sein/Vega/Rin prefab fields on `NewRunSceneEntryManager` when ready.
- Connect runtime HP/resource mutation from the combat loop so `MonsterUnitActor.RefreshDebugView()` reflects live damage and shield changes.
- User verifies NewMainMenu monster selection into NewRunScene in Play Mode.

### Evidence

- Updated `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` so selected monster ID resolves through `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)`, `PakuriDataManager.Instance.ResolveMonster(...)`, `RunSession.Begin(...)`, and `UnitFactory.CreateSelectedMonster(...)`.
- Updated `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` with `Initialize(MonsterUnitRuntimeModel)`, model storage, child reference resolution, and debug HP/name refresh.
- Unity-MCP editor code execution returned `manager=True|spawn=1PSpawnPoint|eveScenePrefab=Eve_Unit|actor=True|initialize=True|refresh=True|nameLabel=True|hpLabel=True|hpFill=True|shieldFill=True|modelMonster=eve|modelHp=220|learnedA=1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-14: User said Eve prefab now has HP SlideBar, debug HP/name labels, and `MonsterUnitActor`, then requested Code Builder to start Phase2-B.

## Task: 2026-05-14 NewMainMenu To NewRunScene Entry Implementation

### Task title

Implement selected monster handoff and first NewRunScene 1P prefab spawn.

### Goals

- Carry selected monster ID from `NewMainMenu` to `NewRunScene`.
- Include `NewMainMenu` and `NewRunScene` in Build Settings.
- Spawn `Assets/Prefab/Monster/Eve_Unit.prefab` at `1PSpawnPoint` when Eve is selected or when fallback is allowed.

### Constraints

- Role Owner is Code Builder.
- Only Eve prefab spawning is implemented because `Assets/Prefab/Monster` currently contains only `Eve_Unit.prefab`.
- Non-Eve selections are stored but currently log that no prefab is configured.
- Code Reviewer was explicitly skipped by the user for this task.
- Do not run Unity Play Mode; user verifies gameplay flow.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Add Ariel/Sein/Vega/Rin prefab bindings when their unit prefabs exist.
- Continue Phase2-B by binding spawned 1P prefab/model to the InGame unit actor/runtime model instead of only spawning the visual shell.
- User verifies the NewMainMenu -> NewRunScene click flow in Play Mode.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Core/NewRunStartContext.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs`.
- Updated `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` so `GameManager` has `NewRunSceneEntryManager`, `playerSpawnPoint` references `1PSpawnPoint`, and `eveUnitPrefab` references `Assets/Prefab/Monster/Eve_Unit.prefab`.
- Updated `Pakuri/ProjectSettings/EditorBuildSettings.asset` through Unity-MCP so `Assets/Scenes/NewScene/NewMainMenu.unity` and `Assets/Scenes/NewScene/NewRunScene.unity` are enabled build scenes.
- Unity-MCP read-only code returned `gameManager=True|entry=True|spawn=1PSpawnPoint|prefab=Eve_Unit|scene=Assets/Scenes/NewScene/NewRunScene.unity`.
- Runtime and editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: User explicitly requested Code Builder to pass selected monster entry data to NewRunScene and spawn the Eve shell prefab at 1PSpawnPoint.

## Task: 2026-05-14 NewMainMenu To NewRunScene Loading Timing

### Task title

Record when the NewMainMenu selection handoff should be applied.

### Goals

- Connect selected monster data before deeper `NewRunScene` actor/prefab binding.
- Preserve current `RunStartContext` / `RunSession` ownership pattern unless replaced by a later CSVData/InGame loader task.
- Keep `NewRunScene` entry ID-based so CSVData and prefab binding can evolve without rewriting UI.

### Constraints

- Role Owner is Designer.
- No Run flow code or scene change in this design note.
- UI implementation should not own combat spawning; it should only choose a monster, prepare run context, and load the scene.

### Role Owner

Designer

### Status

Ready for Code Builder handoff.

### Next Actions

- Implement `UIManager.cs` handoff before Phase2-B actor binding, because Phase2-B needs a real selected 1P monster input.
- After handoff, Phase2-B should consume the selected monster at `NewRunScene` entry and bind it to `1PSpawnPoint`.
- Later CSVData Phase3~5 can replace the data lookup source while keeping the same selected-monster ID handoff contract.

### Evidence

- `Pakuri/Assets/Legacy/Scripts/Run/Flow/RunStartContext.cs` stores `SelectedMonster`, `Session`, and `HasPendingRun`.
- `Pakuri/Assets/Legacy/Scripts/Run/Flow/RunSceneBootstrap.cs` reads `RunStartContext.Instance` and falls back only when no pending run exists.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` already has a selected monster creation path and currently defaults the Phase2-A sample to `eve`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains `1PSpawnPoint`, `GameManager`, and `Nexus`.

### History

- 2026-05-14: User asked where to place the NewMainMenu UI flow, monster selection, and NewRunScene loading work in the next implementation order.

## Task: 2026-05-14 Combat V2 Final Run Integration Target

### Task title

Record the completed Run-to-Combat V2 integration structure.

### Goals

- Preserve MainMenuScene / `RunStartContext` / `RunSession` data timing until Combat V2 is ready to replace the old combat entry.
- Define how final V2 ingame flow receives selected monster, session state, party state, learned choices, and catalog data.
- Keep Run UI integration deferred until a minimum V2 combat loop is stable.

### Constraints

- Role Owner is Designer.
- No Run flow implementation, UI wiring, scene edit, or Play Mode verification in this task.
- Existing `RunSceneBootstrap` still starts the old `CombatRuntimeController` path until a later Code Builder task changes it.

### Role Owner

Designer

### Status

Completed as Run integration architecture context.

### Next Actions

- Future Code Builder work should introduce a V2 scene/bootstrap handoff only after Phase1-D validation and Phase2-A unit mapping are implemented.
- When production flow is rewired, preserve `RunStartContext` and `RunSession` ownership of selected monster and learned-state data.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html`.
- `Pakuri/Assets/Scripts/Run/Flow/RunSceneBootstrap.cs` currently resolves the catalog and calls `CombatRuntimeController.BeginConfiguredDay(monster, session, fallbackCatalog)`.
- `Pakuri/Assets/Scripts/Run/Flow/RunStartContext.cs` stores `SelectedMonster`, `Session`, and `HasPendingRun`.
- `Pakuri/Assets/Scripts/Run/Session/RunSession.cs` stores selected monster ID/name, learned active/passive IDs, chosen rewards, manifested monster IDs, and `PartyMembers`.
- Scene YAML confirms `NewRunScene` contains `GameManager`, `1PSpawnPoint` through `5PSpawnPoint`, and `Nexus`.

### History

- 2026-05-14: Designer documented the final Run integration target for completed Combat V2 ingame flow.

## Task: 2026-05-14 NewRunScene Run And Combat Scene Contract

### Task title

Record `NewRunScene` as the intended in-game scene for future Run/Combat V2 flow.

### Goals

- Treat `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` as the scene used for Combat V2 tests and the future main in-game scene.
- Preserve the current MainMenuScene / RunStartContext production data handoff until explicitly rewired.
- Record scene object roles for future RunScene integration.

### Constraints

- Role Owner is Designer.
- No Run flow, scene, UI, or bootstrap code changes in this task.
- Existing `RunSceneBootstrap` and current runtime flow remain unchanged until a Code Builder task rewires them.

### Role Owner

Designer

### Status

Recorded as Run/scene context.

### Next Actions

- Future Code Builder work should connect Combat V2 to `NewRunScene` only after the current test-only data bootstrap and sample skill bridge are validated.
- When production flow is wired, preserve MainMenuScene selection and `RunStartContext` data timing unless the user explicitly changes it.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` exists.
- Scene YAML contains `BG`, `1PSpawnPoint`, `2PSpawnPoint`, `3PSpawnPoint`, `4PSpawnPoint`, `5PSpawnPoint`, `GameManager`, and `Nexus`.
- User stated that `NewRunScene` is both the test scene and intended final in-game scene.
- User stated that `1P~5PSpawnPoint` are the player and manifested monster spawn points.
- User stated that `GameManager` is for the game's core logic and `Nexus` is the nexus.

### History

- 2026-05-14: User clarified the intended role of existing objects in `NewRunScene`.

## Task: 2026-05-13 Combat V2 RunSession Compatibility Note

### Task title

Record Run-domain compatibility decisions for Combat V2.

### Goals

- Keep current Run UI Flow and `RunSession` data ownership while new combat runtime is built.
- Store skill enhancement/learned-choice state on unit state and let Combat V2 read it during skill execution.
- Defer `RunCombatUiController` integration until a minimal new combat loop exists.

### Constraints

- Role Owner is Designer.
- Do not implement UI or Run flow changes in this task.
- Existing RunScene remains current until a later Code Builder task wires Combat V2.

### Role Owner

Designer

### Status

Completed as design context.

### Next Actions

- Combat V2 implementation should consume `RunSession` and `RunSession.RunMonsterState` without changing Run flow first.
- UI integration should be designed only after Combat V2 can run a minimal battle independently.

### Evidence

- Added `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html`.
- `Pakuri/Assets/Scripts/Run/Flow/RunSceneBootstrap.cs:53` through `:57` shows the current combat entry receives `MonsterDefinition`, `RunSession`, and `GameDataCatalog`.
- `Pakuri/Assets/Scripts/Run/Session/RunSession.cs` stores `SelectedMonsterId`, learned actives/passives, party members, manifested monster records, and per-monster learned state.
- User confirmed that learned choices should remain stored on unit state and be queried at skill execution time.

### History

- 2026-05-13: User decided not to implement UI integration yet and to keep learned-choice storage on unit state for Combat V2.

## Task: 2026-05-09 Assets Scripts Folder Organization

### Task title

Organize Run scripts under Flow, Session, and UI subfolders.

### Goals

- Make the Run script structure easier to scan from the folder tree.
- Keep run behavior unchanged by moving files only, with `.cs.meta` files moved together.

### Constraints

- Role Owner is Designer -> Code Builder.
- Do not change C# class names, namespaces, serialized field names, or gameplay logic.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Designer -> Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Use `Pakuri/Assets/Scripts/Run/Flow`, `Session`, and `UI` as the current Run script map.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Added design document `Pakuri/reference/Report/2026-05-09-assets-scripts-folder-organization-design.md`.
- Moved `MainMenuFlowController.cs`, `RunFlowController.cs`, `RunFlowState.cs`, `RunSceneBootstrap.cs`, and `RunStartContext.cs` to `Pakuri/Assets/Scripts/Run/Flow`.
- Moved `RunDayModel.cs` and `RunSession.cs` to `Pakuri/Assets/Scripts/Run/Session`.
- Moved `DebugSceneController.cs` and `RunCombatUiController.cs` to `Pakuri/Assets/Scripts/Run/UI`.
- Moved `.cs.meta` files with their matching `.cs` files to preserve Unity script GUIDs.
- Unity-MCP `refresh_unity` reached idle after script refresh.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings after rerunning it alone; the earlier parallel editor build failed only because the runtime build held an `obj\Debug` cache file lock.
- Unity-MCP console warning/error read returned only MCP client handler logs.

### History

- 2026-05-09: User requested organizing `Assets/Scripts` so Run and other domains are clearer from the folder structure.
