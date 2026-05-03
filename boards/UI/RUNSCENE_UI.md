# RUNSCENE_UI

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

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

