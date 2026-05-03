# MAINMENU_UI

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

## Task: MainMenu Persistent Editable Panels

### Task title

MainMenuScene stage-transition UI panels are persistent scene objects

### Goals

- MainMenuScene UI transitions must not create/delete runtime screen UI.
- Touch To Start, Run menu, and Character Select must all exist in the scene so the user can edit them together in Edit Mode.
- Future UI direction: authored scene UI is the source of truth; scripts bind callbacks, toggle visibility, and only create missing named anchors.

### Constraints

- No external reviewer for this task; perform simple self-review only.
- Do not run Unity Play Mode; user performs gameplay verification.
- All claims must be based on actual files, scene state, or command output.

### Role Owner

Code Builder

### Status

Builder changes applied and self-reviewed. Waiting for user Play Mode verification.

### Next Actions

- User verifies MainMenuScene flow: Touch To Start -> Run -> Character Select -> RunScene.
- If user edits any of `TouchToStartPanel`, `RunMenuPanel`, `CharacterSelectPanel`, or their child labels/buttons, verify those edits persist after entering Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs` now has separate persistent fields for `touchToStartPanel`, `runMenuPanel`, `characterSelectPanel`, and `monsterButtonRoot`.
- `MainMenuFlowController.OnEnable()` calls `ShowAllPanelsForEditing()` only when `Application.isPlaying` is false, so all panels are visible in Edit Mode.
- Runtime methods `ShowTouchToStart()`, `ShowRunMenu()`, and `ShowCharacterSelect()` call `SetPanelVisibility(...)` and no longer call `Destroy`, `DestroyImmediate`, or `ClearButtons`.
- `EnsureText()` and `EnsureButton()` set default text/style only when a component is newly created, preserving existing authored UI text and styling.
- Unity MCP scene check reported `MainMenuCanvas` child count 3 after cleanup.
- Unity MCP code execution reported `TouchToStartPanel active=True children=3`, `RunMenuPanel active=True children=3`, and `CharacterSelectPanel active=True children=4`.
- Unity MCP code execution reported five persistent character buttons under `MonsterButtons`: `MonsterButton_ariel`, `MonsterButton_eve`, `MonsterButton_rin`, `MonsterButton_sein`, and `MonsterButton_vega`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity console error check showed only MCP-FOR-UNITY client handler exit log entries, not project script compile errors.
- `Pakuri/Packages/manifest.json` search found `com.unity.ugui` and no `com.unity.textmeshpro` line.
- Asset search under `Pakuri/Assets` found no `TextMeshPro`, `TMP_Text`, `TMPro`, or `LiberationSans` usage/assets.
- Generated `Pakuri/Assembly-CSharp.csproj` contains `Unity.TextMeshPro` references, but current project UI scripts and scene-generated UI are still based on `UnityEngine.UI.Text`.

### History

- 2026-04-26: User requested MainMenuScene click-transition screens to be editable at once instead of created/deleted at runtime, and asked why UI text is not TextMeshPro text.
- 2026-04-26: Replaced the single dynamic `MainMenuPanel` flow with persistent `TouchToStartPanel`, `RunMenuPanel`, and `CharacterSelectPanel` scene objects.
- 2026-04-26: Removed the obsolete generated `MainMenuPanel` from `MainMenuScene` and saved the scene.
- 2026-04-26: Verified build, scene hierarchy, persistent character buttons, and console state.
- 2026-04-26: User reported UI Pos X / Pos Y could not be edited. Actual code and scene checks found `VerticalLayoutGroup` and `ContentSizeFitter` on generated UI containers.
- 2026-04-26: Updated `MainMenuFlowController` and `RunCombatUiController` so generated UI containers remove `VerticalLayoutGroup` / `ContentSizeFitter` instead of adding them.
- 2026-04-26: Verified MainMenuScene `TouchToStartPanel`, `RunMenuPanel`, `CharacterSelectPanel`, and `MonsterButtons` report `VLG=False, CSF=False`; also removed and saved those components from RunScene reward/defeat UI containers.

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

## Task: Monster Select Run UI Expansion Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `2.Monster`, `skill-choice-pool-rule.md`, `combat-reward-system.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `g~j`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `Main Camera`, `Global Light 2D`, `CombatRoot`.
- Legacy non-English note retained these code references: `CombatRoot`, `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`, `ProjectileRoot`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `NO_UI_TOOLKIT_ASSETS`, `NO_UI_NAMED_ASSETS`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/monster-basic-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `eve/eve-tower.md`, `rin/rin-tower.md`, `sein/sein-tower.md`, `vega/vega-tower.md`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `f~j`, `f-ambidextrous.md`, `g~j`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `2.Monster`, `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `combat-reward-system.md`, `dungeon-squad-run-structure.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note retained these code references: `g~j`.
- Legacy non-English note retained these ASCII code references: `, `.

