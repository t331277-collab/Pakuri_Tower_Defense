# REWARD_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

## Task: Run Day Combat Type And Material Rewards

### Task title

Implement run day combat type model, actual prisoner/gold/dark trace rewards, and prisoner offering choices.

### Goals

- Add a run day model for day index and combat type.
- Implement document-based rewards for prisoner, gold, and dark trace.
- Do not implement artifact effects yet.
- Show reward buttons by cloning editable templates under `RewardPanel/RewardButtons`.
- Show prisoner reward types and open the pre-made `PrisonerPanel` for offering choices when a prisoner reward is clicked.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run Code Reviewer once only after implementation.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation and local validation for editable templates, click-to-claim material rewards, always-available ContinueButton, and prisoner offering choice panel completed. User Play Mode verification is complete. User chose to defer the external Code Reviewer run for later.

### Next Actions

- User will run or request the deferred external Code Reviewer review later if needed.

### Evidence

- `Pakuri/reference/4.run/combat-reward-system.md` defines prisoner count chance, boss prisoner guarantee, gold, and dark trace rewards.
- `Pakuri/reference/4.run/dungeon-squad-run-structure.md` defines day-based combat types for normal, midboss, and boss days.
- `RunSession.cs` currently stores stage/day/gold/dark trace/prisoner count but has no explicit combat type model.
- `RunCombatUiController.cs` currently uses fixed `RewardButton_0` to `RewardButton_2` slots under `RewardButtons`.
- Added `Pakuri/Assets/Scripts/Run/RunDayModel.cs` with `RunCombatType` and day-based combat type resolution.
- `RunSession.cs` now tracks `CurrentDayModel`, `CurrentCombatType`, and collected prisoner names.
- `CombatRuntimeController` now builds reward items for prisoners, gold, and dark trace only; artifact rewards and prisoner offering are not implemented.
- `RunCombatUiController.cs` now clones editable `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` templates for prisoner, artifact, and material/other reward display categories.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the 2 existing Unity/MCPForUnity reference warnings.
- Unity refresh requested script compilation; console error query returned only MCP-FOR-UNITY client handler exit logs, not project script compile errors.
- `git diff --check` for changed Run/Combat files returned exit code 0 with CRLF warnings only.
- Unity generated `Pakuri/Assets/Scripts/Run/RunDayModel.cs.meta`.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES` in `codex_loop_logs/run_day_rewards_reviewer_20260428.md`.
- Reviewer finding: `CombatRuntimeRewards.cs` can duplicate prisoner rewards because `BuildRewardPrisoners()` adds guaranteed boss prisoners and then samples `currentNormalEnemyPool`, which can include the same normal enemy used as `currentNormalBossDefinition`.
- User accepted the duplicate prisoner finding as acceptable for now and reported Play Mode test completed.
- `CombatRuntimeController.RewardChoiceView` now carries `PrisonerName`, `GoldAmount`, `DarkTraceAmount`, and `Claimed`.
- `CombatRuntimeRewards.ApplyRewardChoice()` now marks one reward option as claimed and keeps `IsWaitingForRewardChoice` true until all reward options are claimed.
- `RunSession.cs` now exposes `ClaimMaterialReward()` and `ClaimPrisonerReward()` for click-to-claim updates.
- `RunCombatUiController.cs` no longer calls `ApplyPostCombatSummary()` when entering the reward panel; it applies prisoner/material rewards only from clicked reward buttons.
- `RunCombatUiController.cs` now resolves editable templates named `Prisoner`, `Artifact`, and `Material`.
- Unity editor check on loaded `RunScene` found `RewardButtons` children: `RewardPreviewButton`, `Prisoner`, `Artifact`, and `Material`; missing component scan returned `missing=0`.
- Saved `RunScene` after template rename.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the 2 existing Unity/MCPForUnity reference warnings.
- Unity console was cleared and rechecked; error query returned 0 entries.
- User reported Play Mode verification completed for the click-to-claim reward flow and clarified that `ContinueButton` staying active before all rewards are selected is intentional.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/prisoner-choice-system.md`.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md` defines the skill choice pool as unlearned active skills, unlearned passive skills, learned active enhancements, and master skills when conditions exist; candidates under 3 are shown only by remaining count.
- `Pakuri/reference/2.Monster/monster-basic-rule.md` defines run-time acquisition limits as active skills 3 and passive skills 3.
- `RunScene.unity` contains a pre-made inactive `PrisonerPanel` with `Choice1`, `Choice2`, and `Choice3`.
- `MonsterDefinition.cs` contains current data fields available for this prototype: `ActiveSkills`, `PassiveSkills`, and `InitialRewardChoices`; no separate master-skill data model exists yet.
- `RunSession.cs` now records offering choices and learned active/passive skills through `RecordOfferingChoice()`, `HasLearnedActive()`, and `HasLearnedPassive()`.
- `RunCombatUiController.cs` now caches `PrisonerPanel`, opens it from prisoner reward buttons, builds up to 3 shuffled offering choices from actual monster data while respecting the current active/passive acquisition limits, hides unused choice buttons, and returns to `RewardPanel` after a choice.
- `RunCombatUiController.cs` now keeps `ContinueButton` active in reward state so rewards can be skipped.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the 2 existing Unity/MCPForUnity reference warnings after the prisoner offering implementation.
- Unity script refresh completed; console error query returned only MCP-FOR-UNITY client handler exit logs, not project script compile errors.
- User reported Play Mode verification completed for the prisoner offering choice flow.
- User reported no notable Play Mode issues and chose not to run Code Reviewer now; user may run Code Reviewer later.

### History

- 2026-04-28: User requested roadmap steps 2 and 3 together, excluding artifact implementation, and requested reward buttons cloned from one editable template per reward category.
- 2026-04-28: Code Builder implemented the run day combat type model, material reward construction, prisoner display reward items, and template-cloned reward buttons.
- 2026-04-28: External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES`; Code Builder is waiting for user instruction instead of auto-fixing.
- 2026-04-28: User accepted the duplicate prisoner finding, reported Play Mode test completed, and requested editable `Prisoner`, `Material`, `Artifact` templates plus click-to-claim material rewards.
- 2026-04-28: Code Builder changed reward acquisition from reward-panel entry to clicked reward buttons, kept artifact as an editable template only, and saved `RunScene` with editable template names.
- 2026-04-28: User reported Play Mode verification completed and clarified that ContinueButton should remain active even when rewards remain unselected.
- Legacy non-English note retained these code references: `PrisonerPanel`.

- 2026-04-28: User reported Play Mode verification completed for the prisoner offering choice flow.

- 2026-04-28: User reported no notable Play Mode issues and chose to defer the Code Reviewer run until later.

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

