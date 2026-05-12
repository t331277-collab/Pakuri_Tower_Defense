## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-05` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/UI/UI_BLACKBOARD.md`.

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
