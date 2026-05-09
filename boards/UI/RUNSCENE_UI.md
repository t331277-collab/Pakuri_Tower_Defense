# RUNSCENE_UI

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Task: 2026-05-08 Manifested HP Bar Runtime Sprite Repair

### Task title

Repair 2P-5P Monster HP bar fill sprites during HP label refresh.

### Goals

- Make authored `2PMonster` through `5PMonster` HP bars visible even if their `Fill` renderer is already bound with `sprite=null`.
- Keep existing scene-authored `MonsterHpBar/Fill` children instead of replacing UI objects.

### Constraints

- Role Owner is Code Builder.
- Play Mode visual verification remains user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies 2P-5P manifested HP bars in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:2026` updates manifested HP bars through the new runtime repair helper.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:2029` normalizes a live `HpBarFill` when its `sprite` is null, then immediately uses the normalized renderer for the bar fill update.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User found that the live scene instances `2PMonster/MonsterHpBar/Fill` through `5PMonster/MonsterHpBar/Fill` still had null sprites after the earlier party-rebuild fix.

## Task: 2026-05-08 Manifested Slot Status View Reuse

### Task title

Reuse existing RunScene 2P-5P monster status children for manifested monsters.

### Goals

- Avoid generated duplicate status labels when scene-authored status children already exist.
- Keep manifested monsters aligned with Eve-style one name label, one HP text, and one HP bar presentation.
- Support observed scene names and the user's reported `HPLable`, `HPBar`, and `Name Label` variants.

### Constraints

- Role Owner is Code Builder.
- Scene hierarchy was inspected through Unity-MCP; Play Mode was not run.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies visual overlap in RunScene Play Mode with manifested 2P-5P monsters.
- If any slot uses a different child name, add that exact scene name to the resolver after inspection.

### Evidence

- Unity-MCP scene hierarchy inspection found `MonsterHpLabel`, `MonsterHpBar/Fill`, `MonsterHpBar/Shield`, and `MonsterNameLabel` under inspected RunScene monster slot objects.
- Unity-MCP `find_gameobjects` did not find literal `HPLable`, `HPLabel`, `HPBar`, `Name Label`, or `NameLabel` objects, so the implementation supports both observed and reported names.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:272` resolves `MonsterNameLabel`, `Name Label`, `NameLabel`, `MonsterHpLabel`, `HPLabel`, `HPLable`, `HP Label`, and HP/shield bar fill paths.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1870` updates separate status views and only falls back to the generated combined label when no HP label exists.
- C# diff whitespace check over the touched combat files completed with exit code 0.

### History

- 2026-05-08: User asked that RunScene 2P-5P monsters use already existing child status objects and not recreate overlapping HP/name UI when manifested.

## Task: 2026-05-08 RunCombatCanvas Prisoner UI Wiring Inspection

### Task title

Inspect current RunScene prisoner Manifest UI wiring after manual `RunCombatCanvas` cleanup.

### Goals

- Confirm whether the current scene has the exact panel names that `RunCombatUiController` binds.
- Confirm whether required child buttons have `Button` and label text objects.
- Identify suspicious hierarchy issues without modifying the scene.

### Constraints

- Role Owner is Designer for inspection/reporting.
- Do not modify scene hierarchy in this task.
- Do not run Unity Play Mode.

### Role Owner

Designer

### Status

Completed with one scene-structure warning.

### Next Actions

- If requested, clean `PrisonerOfferingPanel` so it contains only intended Offering UI children and does not contain a nested `DefeatPanel` or duplicate `Title`.
- User verifies click flow in Play Mode.

### Evidence

- Unity-MCP loaded `Assets/Scenes/RunScene.unity` and inspected `RunCombatCanvas`.
- Found exact panels: `RewardPanel`, `PrisonerChoicePanel`, `PrisonerSummonerPanel`, `PrisonerOfferingPanel`, `PrisonerManifestFailurePopup`, and root `DefeatPanel`.
- Found `RewardPanel/RewardButtons/Prisoner`, `Artifact`, and `Material` with `Button=True` and `LabelText=True`.
- Found `PrisonerChoicePanel/ManifestButton`, `AssimilateButton`, `OfferingButton`, and `CorruptButton` with `Button=True` and `LabelText=True`.
- Found `PrisonerSummonerPanel/MonsterImage`, `Summary`, `SummonButton`, `ContinueButton`, and `BackButton`.
- Found `PrisonerOfferingPanel/Choice1`, `Choice2`, and `Choice3` with `Button=True` and `LabelText=True`.
- Found `PrisonerManifestFailurePopup/Summary` and `CloseButton`; `CloseButton` has `Button=True` and `LabelText=True`.
- Did not find misspelled names `PrisoneChoicePanel`, `PrisonerOffringPanel`, or `ProsonerManifextFaailurePopUP`.
- Warning: Unity-MCP found `PrisonerOfferingPanel` contains child `DefeatPanel` and two `Title` children.
- Report saved as `Pakuri/reference/Report/2026-05-08-runscene-manifest-ui-and-runtime-status.html`.

### History

- 2026-05-08: User manually reorganized `RunCombatCanvas` and asked for panel/button connection inspection.

## Migrated Task Blocks

## Task: 2026-05-05 RunScene MonsterPanel 1P Skill Status UI

### Task title

Bind RunScene MonsterPanel to 1P selected Monster active skill state.

### Goals

- Use the scene-authored RunScene `MonsterPanel` instead of replacing it with generated UI.
- Keep `1PMonster` active and future 2P-5P Monster groups inactive for now.
- Show learned active skills in `Active1` through `Active3`, with current-ammo-only TMP magazine count for magazine skills and a vertical cooldown/reload overlay.

### Constraints

- Role Owner is Code Builder.
- Preserve user-authored RunScene UI layout and images.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies RunScene Play Mode behavior as rewards add active skills, including current-ammo-only text such as `10`, `9`, `8` and per-skill cooldown overlays on Active1-3.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scenes/RunScene.unity` contains `MonsterPanel`, `1PMonster`, and multiple `Active1` / `Active2` / `Active3` names.
- Unity-MCP loaded-scene inspection found `RunCombatCanvas/MonsterPanel/1PMonster/Active1`, `Active2`, and `Active3`, plus `2PMonster` through `5PMonster`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now binds `CombatMonsterPanelUiController` from `RunCombatUiController.InitializeUi()` and runtime `Update()`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now binds existing slot `Text (TMP)` descendants through `TMP_Text` and no longer creates `CountText`.
- `Select-String -LiteralPath Pakuri\Assets\Scenes\RunScene.unity,Pakuri\Assets\Scenes\DebugScene.unity -Pattern "CountText"` found no saved `CountText` after the follow-up cleanup.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now provides the skill state snapshot used by the panel.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Follow-up builds for both projects completed with 0 errors after the TMP ammo binding change.
- 2026-05-05 follow-up: Rin B/C/D/E are no longer stored as `MagazineProjectile` in `rin.asset` or `monster_skills.csv`; Active slots now classify Rin A as the only Rin magazine skill.
- 2026-05-05 follow-up: `CombatRuntimeController.CreateMonsterPanelSkillView(...)` now requires `MagazineCapacity > 0` before using ammo/reload state, so Active2/3 non-magazine skills use their own slot cooldowns.
- 2026-05-05 follow-up: `RunCombatUiController.cs` disables TMP ammo text for non-magazine slots and assigns `DebugUiSolid` to `CooldownOverlay` so the black filled image can visibly drain as cooldown completes.
- 2026-05-05 follow-up validation: runtime and Editor builds completed with 0 errors after rerunning the Editor build sequentially; Unity-MCP read-only asset check reported `rin-b:Buff`, `rin-c:LineAttack`, `rin-d:Execute`, `rin-e:AreaAttack`, and console errors were only MCP client handler logs.

## Task: 2026-05-08 Manifested HP Slide Bar Fallback

### Task title

Ensure manifested monsters have a visible HP slide bar even when scene children are missing.

### Goals

- Keep using existing RunScene status children when present.
- Generate one fallback `MonsterHpBar` with `Background`, `Fill`, and `Shield` children only when no HP bar fill renderer is found.
- Keep manifested monsters to one name label, one HP text, and one HP bar set.

### Constraints

- Role Owner is Code Builder.
- UI verification in Play Mode is user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies 2P-5P manifested monster HP slide bars in RunScene Play Mode.
- If a specific scene child has a different name, inspect that exact child name and add it to the resolver.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:285` falls back to `EnsureManifestedHpBar(...)` only when no HP bar fill renderer is resolved.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:298` creates a fallback `MonsterHpBar`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1946` updates manifested monster name text, HP text, and HP/shield bar fill values.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- `git diff --check` over touched combat files completed with exit code 0 and CRLF warnings only.

### History

- 2026-05-08: User reported manifested monsters did not have an HP SlideBar.

### History

- 2026-05-05: User requested RunScene MonsterPanel behavior for current 1PMonster active skills, magazine text, and cooldown/reload overlay.
- 2026-05-05: User requested replacing `CountText` / `10/10` ammo display with a single `Text (TMP)` current-count display and reported copied cooldown behavior; Builder changed the shared UI binder so each slot uses its own snapshot and TMP text.
- 2026-05-05: User reported Howling/Shockwave Active2/3 still had ammo and Active1-like cooldown; Builder fixed Rin data and the shared MonsterPanel display guard.

## Task: 2026-04-26 Run UI Implementation Status Report

### Task title

HTML report for completed and incomplete Run / UI implementation work on 2026-04-26

### Goals

- Compare today's implementation against `run-systems-integration-summary-report.html` and `monster-select-run-ui-expansion-plan.html`.
- Document completed work, incomplete work, UI editability issues, and chosen UI editing direction.

### Constraints

- All claims must be based on actual files, scene state, command output, or `BLACKBOARD.md` history.
- Do not include work-time estimates in the report.
- Reflect the user's decision that game data is made inside Unity and consumed from Unity assets, not from runtime CSV loading.
- Reflect the user's decision that UI will use editable scene UI: Codex may create a base UI, and user-authored UI should be modified/bound rather than replaced.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User can open `Pakuri/reference/2026-04-26-run-ui-implementation-status-report.html` to review the report.

### Evidence

- Created `Pakuri/reference/2026-04-26-run-ui-implementation-status-report.html`.
- The report references actual implementation files including `MainMenuFlowController.cs`, `RunCombatUiController.cs`, `RunSceneBootstrap.cs`, `RunStartContext.cs`, `RunSession.cs`, `MonsterDefinition.cs`, and `GameDataCatalog.cs`.
- File timestamp check confirmed the report exists under `Pakuri/reference`.
- Updated the report to remove work-time content, UI Toolkit incomplete-scope content, and user Play Mode verification from the incomplete-scope table.
- Updated the report to state that CSV is not the runtime data path; Unity-created assets such as `MonsterDefinition` and `GameDataCatalog` are the chosen data source.

### History

- 2026-04-26: User requested an HTML work report based on `run-systems-integration-summary-report.html` and `monster-select-run-ui-expansion-plan.html`.
- 2026-04-26: Read both source HTML files, implementation file lists, data asset lists, scene file timestamps, manifest TextMeshPro evidence, and generated the report.
- 2026-04-26: User requested removal of Play Mode verification, work-time content, and UI Toolkit incomplete-scope content; user also fixed the direction to Unity-created data assets and editable scene Canvas UI. Updated the report accordingly.

## Task: RunScene Reward Button Visibility Fix

### Task title

RunScene stage-clear reward buttons are fixed editable slots and visible when rewards exist

### Goals

- Fix the RunScene issue where stage-clear reward buttons did not appear.
- Keep reward UI objects editable in Edit Mode instead of relying on delete/recreate behavior.
- Preserve authored button labels where possible, while runtime reward labels are still assigned from actual reward data.

### Constraints

- No external reviewer for this task; perform simple self-review only.
- Do not run Unity Play Mode; user performs gameplay verification.
- All claims must be based on actual files, scene state, or command output.

### Role Owner

Code Builder

### Status

Builder fix applied and self-reviewed. Waiting for user Play Mode verification.

### Next Actions

- User verifies RunScene stage clear: reward panel appears with reward buttons, selecting a reward enables the continue flow.
- If reward panel appears but a button is blocked or misplaced, inspect the saved RectTransform values of `RewardPanel`, `RewardButtons`, and `RewardButton_0..2`.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now uses fixed `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` slots under `RewardButtons`.
- `RebuildRewardButtons()` clears only the tracked button list, calls `EnsureRewardButtonSlots(false)`, then activates slots based on `combatController.GetRewardChoiceCount()`.
- `EnsureRewardButtonSlots()` repairs zero-height `RewardButtons`, ensures the three named button slots, and hides non-slot legacy buttons such as `RewardPreviewButton`.
- Existing nonzero reward button slot RectTransforms keep their authored positions/sizes; default positions are applied only when a slot is newly created or has a broken zero size.
- `EnsureButton()` now preserves existing non-empty labels unless an overwrite is explicitly requested or a label is newly created/empty.
- Unity MCP RunScene inspection after `OnEnable` reported `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` active in Edit Mode, and `RewardPreviewButton` inactive.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity console error check after clearing showed MCP-FOR-UNITY client handler exit logs only, not project script compile errors.

### History

- 2026-04-26: User reported RunScene reward buttons do not appear.
- 2026-04-26: Scene inspection found `RewardButtons` previously had zero height and fixed reward slots were missing, while monster assets contained reward choice data.
- 2026-04-26: Added persistent reward slots, repaired reward root sizing, hid legacy preview buttons, and made existing RunScene reward UI visible in Edit Mode.

## Task: Preserve Authored UI Layouts

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `MainMenuFlowController`, `RunCombatUiController`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Next Actions

- Legacy non-English note retained these code references: `MainMenuScene`, `RunScene`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`, `MainMenuPanel`, `BuildUiScaffold()`, `CacheUiReferences()`, `Title`, `Summary`, `Buttons`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`, `HudPanel`, `RewardPanel`, `DefeatPanel`, `BuildUiScaffold()`, `CacheUiReferences()`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `BuildUiScaffold()`, `EnsurePanel()`, `EnsureText()`, `EnsureButton()`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

## Task: RunScene Combat UI Restoration And Edit Mode Visibility

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `RunScene`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `MainMenuScene`, `RunScene`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Next Actions

- Legacy non-English note retained these code references: `MainMenuScene -> RunScene`.
- Legacy non-English note retained these code references: `MainMenuCanvas`, `RunCombatCanvas`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`, `[ExecuteAlways]`, `Touch To Start`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`, `RunScene`.
- Legacy non-English note retained these code references: `RunCombatUiController`.
- Legacy non-English note retained these code references: `RunCombatUiController`, `EveVerticalSliceController`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs`, `ActiveMonster`, `ActiveSession`, `FallbackMonsterId`.
- Legacy non-English note retained these code references: `RunScene`, `RunCombatCanvas`, `RunCombatUiController`, `CombatRoot`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `MainMenuScene`, `MainMenuCanvas`.
- Legacy non-English note retained these code references: `RunScene`, `RunCombatCanvas`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note retained these code references: `RunScene`.
- Legacy non-English note retained these code references: `RunScene`, `RunFlowController`.
- Legacy non-English note retained these code references: `RunCombatUiController`, `RunScene`, `RunCombatCanvas`.
- Legacy non-English note retained these code references: `MainMenuFlowController`, `RunCombatUiController`, `[ExecuteAlways]`.

## Task: Main Menu To RunScene Flow Separation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `RunScene`, `MainMenuScene`.
- Legacy non-English note retained these ASCII code references: `MainMenuScene`.
- Legacy non-English note retained these code references: `RunScene`, `RunSession`.
- Legacy non-English note retained these code references: `DontDestroyOnLoad`, `RunStartContext`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Next Actions

- Legacy non-English note retained these ASCII code references: `MainMenuScene`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunStartContext.cs`, `RunSession`, `DontDestroyOnLoad`.
- Legacy non-English note retained these ASCII code references: `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`, `Touch To Start`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs`, `RunScene`, `RunStartContext`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `using System;`, `Serializable`, `StringComparison`, `Math`.
- Legacy non-English note retained these code references: `RunScene`, `RunUICanvas`, `RunSceneBootstrap`.
- Legacy non-English note retained these code references: `MainMenuScene`, `MainMenuCanvas`, `MainMenuFlowController`, `EventSystem`.
- Legacy non-English note retained these code references: `Pakuri/ProjectSettings/EditorBuildSettings.asset`, `Assets/Scenes/MainMenuScene.unity`, `Assets/Scenes/RunScene.unity`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `SampleScene.unity`, `MainMenuScene.unity`, `RunScene.unity`.
- Legacy non-English note retained these code references: `RunScene.unity`, `RunUICanvas`, `RunFlowController`.
- Legacy non-English note retained these code references: `RunStartContext`, `MainMenuFlowController`, `RunSceneBootstrap`, `RunScene`, `MainMenuScene`.
- Legacy non-English note retained these code references: `dotnet build`.

## Task: Run Flow UICanvas Prototype Implementation

### Task title

Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.

### Goals

- Legacy non-English note retained these code references: `RunSession`, `RunFlowController`, `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

### Next Actions

- Legacy non-English note retained these code references: `RunUICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `B/G, C/H, D/I, E/J`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Seed Default Game Data`, `Assets/Data/GameData/GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `RunUICanvas`, `EventSystem`.
- Legacy non-English note retained these code references: `Assets/Data/GameData/GameDataCatalog.asset`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`, `EventSystem`, `InputSystemUIInputModule`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `stageIndex`.
- Legacy non-English note retained these code references: `RunFlowController.ClearButtons(...)`, `QueuedForDestroy`.
- Legacy non-English note retained these code references: `RunFlowController.ResolveReferences()`, `Arial.ttf`, `ArgumentException`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`, `Arial.ttf`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `EventSystem`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Resources.GetBuiltinResource<Font>("Arial.ttf")`, `RunFlowController`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

# Task: 2026-05-07 RunSession Learned Skill ID Refactor

### Task title

RunScene offering UI uses skill/passive IDs for learned checks.

### Goals

- Keep RunScene UI display text on `DisplayName` while learned-state logic uses stable IDs.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code and build/Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify active/passive offering buttons, enhancement offerings, and master skill offerings.

### Evidence

- Changed `RunCombatUiController.cs` offering view fields from `ActiveSkillName`/`PassiveSkillName` to `ActiveSkillId`/`PassiveSkillId`.
- `RunCombatUiController.cs` still renders titles with `skill.DisplayName` and `passive.DisplayName`, while learned checks now call `currentSession.HasLearnedActive(skill.SkillId)` and `currentSession.HasLearnedPassive(passive.PassiveId)`.
- Search evidence after edits found no remaining display-name learned checks under run scripts.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity-MCP console warning/error check after compile returned only MCP client handler logs.

### History

- 2026-05-07: Code Builder updated RunScene UI learned-state logic to follow the ID-based `RunSession` refactor.

# Task: 2026-05-08 RunScene Prisoner Manifest UI

### Task title

Add prisoner choice and Manifest result panels to RunScene UI generation.

### Goals

- Generate `PrisonerChoicePanel` and `PrisonerSummonerPanel` from `RunCombatUiController` using the existing runtime UI scaffold helpers.
- Show Manifest result information: monster image, name, A skill description, and basic stats.
- Expand MonsterPanel refresh so 1P remains selected monster and 2P+ groups represent Manifested party members.

### Constraints

- Role Owner is Code Builder.
- UI changes use existing `EnsurePanel`, `EnsureText`, `EnsureButton`, and layout helper patterns in `RunCombatUiController.cs`.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should inspect RunScene in Play Mode and confirm generated panels are positioned/readable in the target resolution.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now calls `EnsurePrisonerChoicePanels()` during UI cache/scaffold setup.
- `RunCombatUiController.cs` creates `PrisonerChoicePanel`, `PrisonerSummonerPanel`, `MonsterImage`, `Summary`, `SummonButton`, and `ContinueButton`.
- `CombatMonsterPanelUiController` now binds `1PMonster` through `5PMonster` groups and refreshes active groups from `combatController.PartyMonsterCount`.
- `CombatMonsterPanelUiController` asks `GetPartyMonsterPanelSkillViews(...)` so 1P shows the selected monster skills and 2P+ shows the Manifested monster A skill only.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested RunScene UI changes for prisoner choice and Manifest result display.

# Task: 2026-05-08 RunScene Runtime UI Activation Gate

### Task title

Ensure Play-mode RunScene entry shows only `HudPanel` and `MonsterPanel` at runtime start.

### Goals

- Support an editor workflow where all RunScene UI panels may be visible before pressing Play.
- On Play/RunScene entry, immediately hide every non-runtime panel except `HudPanel` and `MonsterPanel`.
- Keep reward, prisoner choice, Manifest, Offering, and defeat panels activated only by their game-logic states.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected scene hierarchy, script lines, build output, and Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that RunScene entry hides Reward/Prisoner/Manifest/Defeat UI and later state transitions still open the right panel.

### Evidence

- Unity-MCP scene hierarchy showed `RunCombatCanvas` has `HudPanel`, `RewardPanel`, `PrisonerOfferingPanel`, `DefeatPanel`, `MonsterPanel`, `PrisonerChoicePanel`, `PrisonerSummonerPanel`, and `PrisonerPanel`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:111` now applies `ShowRuntimeHudOnly()` during Play-mode `OnEnable`, before `Start()`.
- `RunCombatUiController.cs:438` through `:447` keeps `HudPanel` active, hides reward/prisoner/choice/summoner/defeat/offering panels, and explicitly activates `MonsterPanel`.
- `RunCombatUiController.cs:52`, `:209`, and `:335` now track the legacy/existing `PrisonerOfferingPanel`.
- `RunCombatUiController.cs:473`, `:596`, `:629`, `:836`, `:1183`, and `:1203` keep `PrisonerOfferingPanel` hidden through reward, prisoner choice, Manifest, Offering, continue, and defeat transitions unless editor preview mode is active.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh completed to idle, and console error query returned only MCP client-handler logs.

### History

- 2026-05-08: User clarified that Play should be tested with all UI visible before entry, and RunScene entry must leave only `HudPanel` and `MonsterPanel` visible until game logic activates other panels.
# Task: 2026-05-08 PrisonerChoicePanel Reward Click Bugfix

### Task title

Ensure `PrisonerChoicePanel` opens from prisoner reward clicks.

### Goals

- Prevent `RewardPanel` from remaining visible with only a claimed prisoner reward label after the click.
- Open `PrisonerChoicePanel` immediately for prisoner reward choices.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code and build/Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that `RewardPanel` hides and `PrisonerChoicePanel` appears when clicking a prisoner reward.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now branches to `OpenPrisonerChoicePanel(...)` before `RebuildRewardButtons()` when `IsPrisonerReward(...)` is true.
- `OpenPrisonerChoicePanel(...)` still hides `RewardPanel`, `PrisonerPanel`, `PrisonerOfferingPanel`, and `PrisonerSummonerPanel`, then activates `PrisonerChoicePanel`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP console error query after script refresh returned only MCP client-handler logs.

### History

- 2026-05-08: User reported `RewardPanel` prisoner click did not show a window.

# Task: 2026-05-08 PrisonerChoicePanel Runtime Persistence Fix

### Task title

Prevent RunScene victory UI refresh from closing prisoner choice UI.

### Goals

- Keep `PrisonerChoicePanel`, `PrisonerSummonerPanel`, `PrisonerPanel`, and `PrisonerOfferingPanel` visible while their reward modal flow is active.
- Keep initial RunScene runtime gate and normal reward UI transitions unchanged.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected RunScene UI script lines and build/Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that `RewardPanel` hides and `PrisonerChoicePanel` remains visible after clicking a prisoner reward.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:595` through `:603` hides `RewardPanel` and activates `PrisonerChoicePanel`.
- `RunCombatUiController.cs:458` through `:480` hides `PrisonerChoicePanel`, `PrisonerSummonerPanel`, `PrisonerPanel`, and `PrisonerOfferingPanel` when `EnterRewardState()` runs.
- `RunCombatUiController.cs:157` through `:164` now prevents `Update()` from calling `EnterRewardState()` while a reward modal is open.
- `RunCombatUiController.cs:1606` through `:1617` defines the active modal check used by the victory UI gate.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP script refresh was requested; console read returned existing missing-script/MCP logs, not C# compile errors.

### History

- 2026-05-08: User reported the prisoner choice panel still did not appear when pressing a reward button.
- 2026-05-08: Builder found the panel activation was being undone by the per-frame victory reward-state refresh and added a modal guard.

# Task: 2026-05-08 PrisonerOfferingPanel Runtime Routing

### Task title

Use `PrisonerOfferingPanel` as the actual RunScene Offering UI.

### Goals

- Bind Offering choice buttons from `PrisonerOfferingPanel`.
- Keep `PrisonerPanel` hidden so the duplicate/legacy panel does not appear during the real Offering flow.
- Preserve `PrisonerChoicePanel` and `PrisonerSummonerPanel` modal behavior.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from Unity-MCP scene hierarchy, inspected script lines, and build/console output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify `PrisonerChoicePanel` -> Offering opens the authored `PrisonerOfferingPanel`.

### Evidence

- Unity-MCP active `RunScene` inspection found `RunCombatCanvas/PrisonerOfferingPanel` active with child buttons `Choice1`, `Choice2`, `Choice3`, and `Title`.
- Unity-MCP also found separate `RunCombatCanvas/PrisonerPanel`, confirming the duplicate/legacy panel exists independently.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now checks existing UI by `PrisonerOfferingPanel`, binds offering buttons from that panel, and no longer creates `PrisonerPanel` as the generated offering scaffold.
- `RunCombatUiController.cs` editor preview/editing state now keeps `PrisonerPanel` hidden and `PrisonerOfferingPanel` visible for editing.
- `RunCombatUiController.cs` runtime transitions hide `PrisonerPanel` and activate `PrisonerOfferingPanel` only for the Offering choice flow.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings; Unity-MCP console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User clarified `PrisonerOfferingPanel` is the real Offering UI and `PrisonerPanel` appears to be a bad duplicate.
- 2026-05-08: Code Builder changed RunScene UI routing to use `PrisonerOfferingPanel` and keep `PrisonerPanel` hidden.

# Task: 2026-05-08 Offering UI Carries Party Member Target

### Task title

Attach Offering UI choices to the target monster state.

### Goals

- Keep the existing `PrisonerOfferingPanel` UI route.
- Make each generated Offering choice carry the party member monster ID it will modify.
- Support selected and Manifested monsters in the same Offering modal flow.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected `RunCombatUiController` code and build output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that Offering choices for Manifested monsters modify the intended monster and that the real `PrisonerOfferingPanel` still opens.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:907` resolves Offering targets from selected and Manifested monsters.
- `RunCombatUiController.cs:943`, `:977`, `:1016`, `:1054`, `:1074`, and `:1094` build choices using `RunSession.RunMonsterState`.
- `RunCombatUiController.cs:968`, `:1007`, `:1040`, and `:1127` assign `MonsterId = memberState.MonsterId` to Offering choice views.
- `RunCombatUiController.cs:1206` commits the Offering choice through `choice.MonsterId`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User said the UI issue seemed fixed, then clarified that Manifested monster state and Offering growth remained wrong.
- 2026-05-08: Code Builder kept the panel route and added target-monster identity to Offering choices.

# Task: 2026-05-08 PrisonerSummonerPanel Back Button

### Task title

Add a no-Manifest return button to `PrisonerSummonerPanel`.

### Goals

- Add a visible button that returns to `RewardPanel` without calling Manifest.
- Keep the existing result `ContinueButton` for after a Manifest attempt.
- Preserve MonsterPanel 1P-5P binding for Manifested party display.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected UI script, saved scene YAML, Unity-MCP output, and build output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify `PrisonerSummonerPanel/BackButton` closes the panel and returns to the reward list.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:390` creates/binds `BackButton`.
- `RunCombatUiController.cs:649` makes the back button visible when the summoner panel opens.
- `RunCombatUiController.cs:731` implements the no-Manifest return handler.
- `Pakuri/Assets/Scenes/RunScene.unity:5233` contains `m_Name: BackButton`.
- `Pakuri/Assets/Scenes/RunScene.unity:8429` contains `m_Text: Back to Reward`.
- Unity-MCP found `BackButton` after script refresh.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested a `PrisonerSummonerPanel` button that returns to `RewardPanel` without summoning.
- 2026-05-08: Code Builder added the button and saved `RunScene`.
# Task: 2026-05-08 Manifested MonsterPanel Ammo State

### Task title

Show Manifested monster A-skill ammo/reload state through the existing 2P+ MonsterPanel binding.

### Goals

- Keep existing RunScene MonsterPanel group binding.
- Feed 2P+ skill views with Manifested magazine current ammo and cooldown/reload data.
- Avoid UI-specific script changes when combat snapshot data is sufficient.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Do not run Unity Play Mode from Codex.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated through combat snapshot changes.

### Next Actions

- User verifies 2P+ MonsterPanel ammo count and reload overlay while Manifested A skills fire/reload.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:106` now passes `isMagazine`, `ShotsRemaining`, `MagazineCapacity`, and cooldown/reload values into `MonsterPanelSkillView` for Manifested party members.
- No `RunCombatUiController.cs` change was required for this task because the existing 1P-5P binder already consumes `GetPartyMonsterPanelSkillViews(...)`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported Manifested monsters had no magazine behavior.
- 2026-05-08: Code Builder added Manifested magazine state to the combat snapshot used by MonsterPanel.

# Task: 2026-05-08 PrisonerSummonerPanel Continue Sync Follow-up

### Task title

Synchronize RunScene Manifested party UI after summoner and Offering actions.

### Goals

- Prevent the first `SummonButton` -> summoner `ContinueButton` flow from leaving the 2P+ MonsterPanel stale.
- Keep Offering-acquired Manifested skills reflected in the 2P+ MonsterPanel skill snapshot.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that 2P+ MonsterPanel updates after the first successful Manifest and after Manifested Offering choices.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:702` refreshes combat party state after Manifest success.
- `RunCombatUiController.cs:1246` refreshes combat party state after Offering choice commit.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:149` provides the refresh method used by RunScene UI.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported first Manifested application appeared delayed until a later Manifest.
- 2026-05-08: Code Builder connected summoner and Offering UI actions to immediate Manifested party refresh.
# Task: 2026-05-08 Manifested Skill Effect Display Follow-up

### Task title

Display Manifested learned skill effects by skill kind instead of generic beam.

### Goals

- Preserve RunScene prisoner/Offering UI flow.
- Ensure the combat visuals shown after Offering-acquired Manifested skills match skill runtime kind better than the old beam-only display.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated through combat script changes.

### Next Actions

- User verifies RunScene UI flow plus in-combat visual result for Manifested Offering skills.

### Evidence

- No `RunCombatUiController.cs` edit was required in this follow-up; existing `RunCombatUiController.cs:1246` refreshes the combat party after Offering commit.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:512` now uses `CreateManifestedSkillVisual(...)`.
- `CombatRuntimeParty.cs:896` dispatches Manifested visual shape by `SkillRuntimeKind`.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported the skill acquisition UI and cooldown were working, but the Manifested monster effect display was wrong.

# Task: 2026-05-08 RunScene Manifested Sustained Visual Follow-up

### Task title

Record RunScene UI context for Manifested sustained skill duration correction.

### Goals

- Keep RunScene UI state aware that no UI script change was required for this follow-up.
- Point verification at in-combat Manifested visuals after Summon/Offering flows.

### Constraints

- Role Owner is Code Builder.
- No RunScene UI file was changed in this pass.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated through combat runtime changes.

### Next Actions

- User verifies RunScene Summon/Offering flow plus the in-combat duration of Manifested sustained effects.

### Evidence

- Existing RunScene UI paths still refresh Manifested party state after Summon and Offering.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now controls the sustained visual durations and Manifested Eve drone behavior.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported the issue through RunScene Manifested/Offering gameplay.

# Task: 2026-05-08 Manifest Failure Popup UI

### Task title

Add RunScene prisoner Manifest failure popup and update Manifest button routing.

### Goals

- Add a failure popup panel for failed Manifest attempts.
- Keep the popup as a reward modal so the per-frame reward state does not immediately hide it.
- Ensure `ManifestButton` owns the roll action while `SummonButton` no longer performs it.

### Constraints

- Role Owner is Code Builder.
- UI remains generated/bound through existing `RunCombatUiController` uGUI helpers.
- Do not run Unity Play Mode; user verifies the visual flow.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies `PrisonerManifestFailurePopup` visibility and return-to-reward behavior in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:65` adds failure popup fields.
- `RunCombatUiController.cs:396` creates `PrisonerManifestFailurePopup` with title, summary, and close button.
- `RunCombatUiController.cs:466`, `:497`, `:620`, and `:738` hide the popup during normal runtime transitions.
- `RunCombatUiController.cs:1715` includes the failure popup in `IsRewardModalOpen()`.
- Runtime and Editor builds completed with 0 errors and existing warnings; Unity-MCP console read returned only MCP client handler logs.

### History

- 2026-05-08: User requested a summon-failure popup when Manifest fails.
