## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-09` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/RUN/RUN_BLACKBOARD.md`.

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
