## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-05` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/UI/UI_BLACKBOARD.md`.

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
