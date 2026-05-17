## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-05` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/UI/UI_BLACKBOARD.md`.

## Task: 2026-05-17 NewMainMenu Legacy Flow Removal Phase3-4 Progress

### Task title

Record UI-side impact after the Run session/state migration and the Phase 4 Legacy/Data closure check.

### Goals

- Preserve `NewMainMenu.unity` + `UIManager.cs` as the surviving menu entry flow during the Run-session migration.
- Record that the new menu flow still does not depend on old `RunStartContext` or old Legacy menu/run controllers.
- Keep old UI/controller deletion deferred until the old-scene cleanup phase.

### Constraints

- Role Owner is Code Builder.
- No UI scene hierarchy, button binding, popup logic, or scene YAML was changed in this task.
- `RunStartContext.cs`, `MainMenuFlowController.cs`, `RunCombatUiController.cs`, and `DebugSceneController.cs` still exist under Legacy for the old scene path.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

New UI flow remains Scripts2-owned after the Run-session migration; old UI/controller cleanup is still pending.

### Next Actions

- Keep `UIManager.cs` and `NewRunStartContext` as the only new menu-to-new-run handoff path.
- Remove old menu/run UI controllers only in the final old-scene cleanup slice.

### Evidence

- `Pakuri/Assets/Scripts2/UI/UIManager.cs` still uses `NewRunStartContext.Prepare(monsterId)` and does not reference `RunStartContext`.
- Repository `Select-String` results showed `RunStartContext` references only in Legacy `MainMenuFlowController.cs` and `RunSceneBootstrap.cs`, not in Scripts2 UI flow code.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` and `RunDayModel.cs` now live under Scripts2-owned folders, reducing the new flow's remaining dependence on Legacy.
- Runtime and editor builds passed after the move, so the new menu-to-new-run compile path remained valid.

### History

- 2026-05-17: Code Builder completed the Run-session migration and recorded that the new UI flow still bypasses the old Legacy menu/run handoff path.

## Task: 2026-05-17 NewMainMenu Legacy Flow Removal Phase1-2 Progress

### Task title

Record UI-side migration progress after the shared Legacy combat/data foundation moved under Scripts2.

### Goals

- Preserve `NewMainMenu.unity` and `UIManager.cs` as the surviving menu entry flow while shared non-UI Legacy files move away from `Legacy/Scripts`.
- Confirm that the Phase 1-2 move did not reintroduce any dependency from the new menu flow back to old Legacy menu/run controllers.
- Keep old Legacy UI/controller deletion deferred until the remaining Run migration is finished.

### Constraints

- Role Owner is Code Builder.
- No UI hierarchy, button binding, or scene YAML was changed in this task.
- Old Legacy menu/run UI controller files still exist and were not deleted in this phase.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Shared combat/data migration completed; new menu flow remains Scripts2-owned, while old UI/controller deletion stays pending.

### Next Actions

- Keep `UIManager.cs` on the new scene path while Phase 3 migrates the remaining `Pakuri.Run` files.
- After Phase 3, remove old menu/run UI controllers only when the old scenes no longer need to be preserved.

### Evidence

- `Pakuri/Assets/Scripts2/UI/UIManager.cs` still calls `NewRunStartContext.Prepare(monsterId)` and keeps `newRunScenePath = "Assets/Scenes/NewScene/NewRunScene.unity"` as the new flow owner.
- Phase 1-2 moved shared non-UI dependencies used by the new flow into `Pakuri/Assets/Scripts2/InGame/Combat` and `Pakuri/Assets/Scripts2/InGame/Data`.
- `Pakuri/Assets/Legacy/Scripts/Run/Flow/MainMenuFlowController.cs`, `Pakuri/Assets/Legacy/Scripts/Run/UI/RunCombatUiController.cs`, and `Pakuri/Assets/Legacy/Scripts/Run/UI/DebugSceneController.cs` still exist, so deletion of the old UI/controller path has not happened yet.
- Runtime and editor builds passed after the move, so the new menu-to-new-run compile path remained valid.

### History

- 2026-05-17: Code Builder completed Phase 1-2 and recorded that the new menu flow stayed on Scripts2-owned UI code while old UI/controller cleanup remains pending.

## Task: 2026-05-17 NewMainMenu Legacy Flow Removal Constraint

### Task title

Record the UI-side constraint for deleting old Legacy menu/run controllers.

### Goals

- Preserve `NewMainMenu.unity` + `UIManager.cs` as the surviving menu entry flow.
- Prevent accidental dependency on old `MainMenuScene` / `RunScene` UI controllers during Legacy cleanup.
- Keep the UI-side migration boundary explicit for Code Builder.

### Constraints

- Role Owner is Designer.
- No UI object, scene hierarchy, or script was changed.
- `UIManager.cs` already owns the new menu flow, but old Legacy scene controllers still exist and still have serialized references in old scenes.

### Role Owner

Designer

### Status

Constraint recorded; ready for Code Builder handoff.

### Next Actions

- Do not route new menu/run behavior back into `MainMenuFlowController` or `RunCombatUiController`.
- During Legacy cleanup, validate that `NewMainMenu.unity` keeps only `Scripts2` menu flow ownership and does not gain new Legacy controller dependencies.
- Remove old menu/run UI controllers only after the shared Legacy base/runtime types have been migrated out.

### Evidence

- `Pakuri/Assets/Scripts2/UI/UIManager.cs` stores `newRunScenePath = "Assets/Scenes/NewScene/NewRunScene.unity"` and loads the new run scene.
- `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity` is the new scene file present under `Assets/Scenes/NewScene`.
- Legacy UI/flow controller files still exist for the old path: `MainMenuFlowController.cs`, `RunCombatUiController.cs`, and `DebugSceneController.cs`.

### History

- 2026-05-17: User asked how to retire Legacy scripts and controllers while keeping the new Scripts2 menu/run scene path.

## Task: 2026-05-14 NewMainMenu UIManager Flow Implementation

### Task title

Implement logic-only `NewMainMenu` UI flow binding.

### Goals

- Keep user-authored `NewMainMenu` UI layout untouched.
- Attach `UIManager` to `NewMainMenu/Manager`.
- Connect `Intro` -> `MainMenuUI` -> `MosterSelectUI`.
- Store the selected monster ID and load `Assets/Scenes/NewScene/NewRunScene.unity`.

### Constraints

- Role Owner is Code Builder.
- Do not restyle or regenerate UI objects.
- Code Reviewer was explicitly skipped by the user for this task.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies Play Mode click flow: Intro `GameStart`, `RunBtn`, monster select, monster-select `GameStart`.
- Next Code Builder task should expand prefab mapping beyond Eve when Ariel/Sein/Vega/Rin unit prefabs exist.

### Evidence

- Updated `Pakuri/Assets/Scripts2/UI/UIManager.cs`.
- `UIManager` resolves inactive scene UI by name and binds `Intro`, `MainMenuUI`, `MosterSelectUI`, `RunBtn`, character buttons, and `GameStart`.
- Unity-MCP loaded `Assets/Scenes/NewScene/NewMainMenu.unity` and read-only code returned `manager=True|uiManager=True|scene=Assets/Scenes/NewScene/NewMainMenu.unity`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: User explicitly assigned Code Builder to connect selected monster data from NewMainMenu to NewRunScene and skip Code Reviewer.

## Task: 2026-05-14 NewMainMenu UIManager Flow Handoff

### Task title

Plan logic-only binding for user-authored `NewMainMenu` UI.

### Goals

- Preserve the user-authored UI objects in `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity`.
- Implement logic in `Pakuri/Assets/Scripts2/UI/UIManager.cs` instead of generating or restyling UI.
- Bind Intro -> MainMenuUI -> MosterSelectUI flow and monster selection buttons to Run scene loading.

### Constraints

- Role Owner is Designer.
- Do not change UI layout, styling, or object hierarchy for this design handoff.
- Actual implementation should be a Code Builder task.
- Scene evidence currently uses `MosterSelectUI`, not `MonsterSelectUI`; Code Builder should bind the inspected object name unless the user renames it.

### Role Owner

Designer

### Status

Ready for Code Builder handoff.

### Next Actions

- Code Builder should replace the empty `UIManager.cs` shell with binding logic and attach it to `Manager` in `NewMainMenu`.
- Code Builder should keep all panel references serialized and also support name-based fallback for `Intro`, `MainMenuUI`, `MosterSelectUI`, `RunBtn`, character buttons, and `GameStart`.
- User should verify the final click flow in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts2/UI/UIManager.cs` exists and is currently an empty `MonoBehaviour` shell.
- `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity` contains `Manager`, `Intro`, `MainMenuUI`, `MosterSelectUI`, `RunBtn`, two `GameStart` objects, and character buttons `Ariel`, `Eve`, `Sein`, `Vega`, and `Rin`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains `BG`, `1PSpawnPoint` through `5PSpawnPoint`, `GameManager`, and `Nexus`.
- Existing legacy `MainMenuFlowController.StartRun(...)` prepares `RunStartContext` and calls `SceneManager.LoadScene(...)`.

### History

- 2026-05-14: User described the desired NewMainMenu flow and asked when to apply the UIManager logic connection relative to the next work.

## Task: 2026-05-14 Pre-2026-05-12 UI Board Archive Cleanup

### Task title

Archive UI detail board files whose latest internal date is before 2026-05-12.

### Goals

- Move UI detail board files that only contain pre-2026-05-12 history into `boards/ARCHIVE`.
- Keep active UI routing on `boards/UI/UI_BLACKBOARD.md`.
- Preserve older DebugScene and MainMenu history in archive files instead of deleting it.

### Constraints

- Role Owner is Designer.
- Do not delete board history.
- Move only files whose latest discovered date is earlier than 2026-05-12.
- Update routing references that previously pointed at the moved active files.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- For new DebugScene or MainMenu UI work, update `boards/UI/UI_BLACKBOARD.md`.
- Consult archived files only when older DebugScene or MainMenu UI history is needed.

### Evidence

- Date scan over non-archive `boards/**/*.md` found `boards/UI/MAINMENU_UI.md` latest date `2026-04-26`.
- Date scan over non-archive `boards/**/*.md` found `boards/UI/DEBUGSCENE_UI.md` latest date `2026-05-05`.
- Moved `boards/UI/DEBUGSCENE_UI.md` to `boards/ARCHIVE/DEBUGSCENE_UI_ARCHIVE_2026-05-14.md`.
- Moved `boards/UI/MAINMENU_UI.md` to `boards/ARCHIVE/MAINMENU_UI_ARCHIVE_2026-05-14.md`.
- Updated `MDTREE.md`, `BLACKBOARD.md`, and `boards/MON/EVE_MONSTER.md` references to use `boards/UI/UI_BLACKBOARD.md` for active UI routing and archived files for older history.

### History

- 2026-05-14: User asked to move board Markdown files that seemed unnecessary because they were from before 2026-05-12 into `boards/ARCHIVE`.

## Task: 2026-05-05 MonsterPanel 1P Skill Status UI

### Task title

Bind scene-authored MonsterPanel skill slots to the current 1P Monster runtime skill state.

### Goals

- Preserve the user-authored `MonsterPanel` hierarchy in RunScene and DebugScene.
- Activate only the `1PMonster` group for now while leaving future 2P-5P/NP Monster groups available for later party expansion.
- Show up to three learned active skills in `Active1`, `Active2`, and `Active3`.
- Show magazine counts under magazine skills and dark cooldown/reload overlay that brightens from top to bottom as time passes.

### Constraints

- Role Owner is Code Builder.
- Scene-authored uGUI remains the source of truth; runtime code binds existing `MonsterPanel/1PMonster/Active1..3`, uses existing `Text (TMP)` descendants for magazine text, and creates only missing `CooldownOverlay` helper objects.
- Skill icons fall back to the existing slot image when `SkillDefinition.SkillIcon` is not assigned.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that RunScene and DebugScene show only 1PMonster and update Active1-3 as active skills are learned/toggled.
- User verifies magazine text shows current ammo only, such as `10`, `9`, `8`, and that each Active slot's cooldown/reload overlay follows its assigned skill state in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/7.UI/8. combat-screen-layout.md` says the character skill group belongs at the lower-left, shows selected active skills next to the character icon, and displays reload/cooldown state plus bullet count.
- `Pakuri/Assets/Scenes/RunScene.unity` and `Pakuri/Assets/Scenes/DebugScene.unity` contain `MonsterPanel`, `1PMonster`, and `Active1` / `Active2` / `Active3` object names.
- Unity-MCP inspection of the loaded RunScene found `RunCombatCanvas/MonsterPanel/1PMonster/Active1`, `Active2`, and `Active3`, plus future `2PMonster` through `5PMonster` groups.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now exposes `GetMonsterPanelSkillViews(...)` for selected active skill icon, magazine, and cooldown/reload state.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now contains `CombatMonsterPanelUiController`, binds it in RunScene and DebugScene controllers, and controls only the existing scene-authored `MonsterPanel` hierarchy.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now binds slot ammo text through `TMP_Text` from existing `Text (TMP)` / `AmmoText` descendants and writes only the current ammo value instead of `current/max`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` no longer creates or binds `CountText`; `Select-String` found no `CountText` in saved RunScene or DebugScene after removing the three prior DebugScene objects.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` excludes `CooldownOverlay` when resolving a fallback icon image so overlay images cannot be mistaken for slot icons on later refreshes.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity refresh reached `resulting_state=idle`; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- `git diff --check` on changed controller files completed with no whitespace errors and CRLF conversion warnings only.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings after the TMP ammo binding fix.
- Unity-MCP inspection of the loaded DebugScene confirmed `CountTextInLoadedScene=0`; Unity console error query returned only MCP-FOR-UNITY client handler logs.
- Follow-up `git diff --check -- Pakuri\Assets\Scripts\Run\RunCombatUiController.cs Pakuri\Assets\Scenes\DebugScene.unity` completed with no whitespace errors and CRLF conversion warnings only.
- 2026-05-05 follow-up: `Pakuri/Assets/Data/GameData/Monsters/rin.asset` and `Pakuri/Assets/CSVdata/source/monster_skills.csv` now classify Rin B as `Buff`, Rin C as `LineAttack`, Rin D as `Execute`, and Rin E as `AreaAttack`, leaving only Rin A as `MagazineProjectile`.
- 2026-05-05 follow-up: `CombatRuntimeController.CreateMonsterPanelSkillView(...)` now treats a skill as magazine only when `RuntimeKind == MagazineProjectile` and `MagazineCapacity > 0`, so zero-magazine skills cannot inherit Active1 ammo/reload state.
- 2026-05-05 follow-up: `CombatMonsterPanelUiController.ApplySlot(...)` now disables the TMP ammo text GameObject for non-magazine skills, and `EnsureCooldownOverlay(...)` assigns the project-owned `DebugUiSolid` sprite so filled cooldown overlays can visibly drain from black to the normal white icon.
- 2026-05-05 follow-up validation: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and the sequential `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings; the first parallel Editor build failed only because the shared `obj\Debug\Assembly-CSharp.dll` was locked by the concurrent runtime build.
- 2026-05-05 follow-up Unity-MCP validation: forced asset refresh recovered to ready state; read-only Editor code reported `rin-a:MagazineProjectile`, `rin-b:Buff`, `rin-c:LineAttack`, `rin-d:Execute`, `rin-e:AreaAttack`, and `DebugUiSolid=DebugUiSolid`; console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-05: User reported adding `MonsterPanel` to RunScene and DebugScene and requested 1PMonster active-skill status binding with default icons, magazine count, and cooldown/reload overlay.
- 2026-05-05: User reported `CountText` duplication, requested `Text (TMP)` as the single ammo text, requested ammo display as current count only, and reported Active2/Active3 behaving like Active1 copies; Builder switched ammo binding to TMP, removed saved DebugScene `CountText` objects, and kept cooldown state sourced from each assigned skill snapshot.
- 2026-05-05: User reported that adding Howling and Shockwave made Active2/3 appear, but non-magazine skills still showed ammo, followed Active1 cooldown, and skipped the black-to-white cooldown fill; Builder corrected Rin skill runtime kinds, added a magazine-capacity guard, hid non-magazine ammo text, and ensured cooldown overlays use a real project sprite.
