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

# Task: 2026-05-08 Prisoner Choice Reward Flow

### Task title

Route prisoner rewards through choice UI before Offering or Manifest.

### Goals

- Make prisoner reward selection open `PrisonerChoicePanel` first.
- Add Manifest, Assimilate, Offering, and Torture/Corrupt choice buttons.
- Preserve existing Offering behavior through `PrisonerPanel`.
- Make Assimilate and Torture/Corrupt clickable but non-functional for now.
- Make Manifest show result data and return to the normal reward-continue flow.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected files and build output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that each prisoner choice button activates the expected panel or placeholder behavior.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` changed prisoner reward handling from direct `OpenPrisonerPanel(...)` to `OpenPrisonerChoicePanel(...)`.
- `RunCombatUiController.cs` now creates `PrisonerChoicePanel` with Manifest, Assimilate, Offering, and Torture/Corrupt buttons.
- `RunCombatUiController.cs` keeps Offering routed to the existing `OpenPrisonerPanel(...)` path.
- `RunCombatUiController.cs` creates `PrisonerSummonerPanel` and displays monster image, name/title, A skill description, and basic stats on Manifest candidate/result.
- `RunCombatUiController.cs` adds a Manifest result `ContinueButton` so success/failure returns to `RewardPanel` and the existing continue flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested prisoner rewards no longer jump directly into Offering and instead open a prisoner choice UI with Manifest/Assimilate/Offering/Torture-Corrupt options.

# Task: 2026-05-08 Reward Panel Runtime Visibility Gate

### Task title

Keep reward and prisoner panels hidden until reward logic activates them.

### Goals

- Hide Reward/Prisoner/Manifest/Offering/Defeat UI on RunScene runtime entry.
- Preserve reward victory flow as the only path that activates `RewardPanel`.
- Preserve prisoner reward flow as the only path that activates prisoner choice, Manifest, and Offering panels.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code and build output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that reward/prisoner panels do not appear before victory and reward interaction.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:438` through `:447` hides reward, prisoner, prisoner choice, prisoner summoner, defeat, and `PrisonerOfferingPanel` on runtime HUD-only state.
- `RunCombatUiController.cs:453` activates `RewardPanel` only in `EnterRewardState()`.
- `RunCombatUiController.cs:590`, `:624`, and `:823` activate prisoner choice, Manifest, and Offering panels only from prisoner reward interactions.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User asked to check that all non-HUD/Monster UI is hidden at RunScene entry and only opens according to game logic.

# Task: 2026-05-08 Prisoner Reward Click Opens Choice Panel

### Task title

Fix prisoner reward click showing only claimed state without opening prisoner choice UI.

### Goals

- Ensure a prisoner reward click opens `PrisonerChoicePanel`.
- Avoid rebuilding the reward list into the claimed/completed visual before the prisoner choice panel opens.
- Make prisoner reward detection robust if one reward view field is inconsistent.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code, build output, and Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that clicking a prisoner reward opens `PrisonerChoicePanel` immediately instead of leaving only the claimed label on `RewardPanel`.

### Evidence

- Before the fix, `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` called `RebuildRewardButtons()` before checking whether the selected reward was prisoner, so the visible reward list could be rebuilt into the claimed state before panel transition.
- `CombatRuntimeRewards.cs` creates prisoner reward options with `RewardId = "prisoner:..."`, `RewardKind = "Prisoner"`, and `PrisonerName`.
- `RunCombatUiController.cs` now checks `IsPrisonerReward(rewardView, rewardId)` before rebuilding reward buttons.
- `RunCombatUiController.cs` now treats a reward as prisoner when `RewardKind == "Prisoner"` or when `PrisonerName` is present and `rewardId` starts with `prisoner:`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh completed to idle, and console error query returned only MCP client-handler logs.

### History

- 2026-05-08: User reported that clicking a prisoner reward in `RewardPanel` only showed the acquired/completed state and did not open any prisoner choice window.

# Task: 2026-05-08 PrisonerChoicePanel Per-Frame Hide Fix

### Task title

Keep prisoner choice/reward modals open after a prisoner reward click.

### Goals

- Fix the remaining prisoner reward click bug where `PrisonerChoicePanel` was opened and then immediately hidden.
- Keep non-prisoner reward clicks on the existing claimed-list rebuild path.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code, build output, and Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify prisoner reward click, Manifest panel open, Offering panel open, and return-to-reward behavior.

### Evidence

- `CombatRuntimeRewards.cs:177` through `:181` creates prisoner rewards with `RewardId = "prisoner:..."`, `RewardKind = "Prisoner"`, and `PrisonerName`.
- `CombatRuntimeController.cs:178` through `:204` exposes `RewardChoiceView.RewardKind` and `RewardChoiceView.PrisonerName`.
- `RunCombatUiController.cs:566` through `:568` checks prisoner reward status before reward-button rebuild and opens `OpenPrisonerChoicePanel(...)`.
- `RunCombatUiController.cs:599` through `:603` activates `PrisonerChoicePanel`.
- `RunCombatUiController.cs:458` through `:480` shows why the bug persisted: `EnterRewardState()` hides prisoner panels.
- `RunCombatUiController.cs:157` through `:164` now skips `EnterRewardState()` while a prisoner reward modal is active.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing Unity/MCPForUnity reference warnings.
- Unity-MCP console read after script refresh showed existing missing-script/MCP entries, not C# compile errors.

### History

- 2026-05-08: User reported that the reward button still did not show `PrisonerChoicePanel`.
- 2026-05-08: Builder identified the first fix was incomplete because the victory `Update()` loop re-entered reward state every frame and hid the newly opened panel.

# Task: 2026-05-08 Prisoner Offering And Reward Used State Follow-up

### Task title

Route Offering to `PrisonerOfferingPanel` and rebuild reward buttons after prisoner modal returns.

### Goals

- Use the scene-authored `PrisonerOfferingPanel` as the actual Offering UI.
- Prevent the legacy/generated `PrisonerPanel` from appearing when Offering is clicked.
- Show the prisoner reward button as used/claimed after returning from Manifest or Offering.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected scene hierarchy, changed code, and build/Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify Offering and Manifest return behavior from the reward panel.

### Evidence

- Unity-MCP scene inspection found `RunCombatCanvas/PrisonerOfferingPanel` with `Choice1`, `Choice2`, `Choice3`, and `Title`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now binds offering title/buttons from `PrisonerOfferingPanel` first, falling back to `PrisonerPanel` only if needed.
- `RunCombatUiController.cs` now hides `PrisonerPanel` and activates `PrisonerOfferingPanel` in the Offering flow.
- `RunCombatUiController.cs` resets `rewardPanelEntered = false` after Manifest result close and after committed Offering, so `EnterRewardState()` rebuilds reward buttons and reflects claimed state.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported Offering opened `PrisonerPanel`, and returning from Manifest did not visibly mark the prisoner reward button as used.
- 2026-05-08: Code Builder routed Offering to `PrisonerOfferingPanel` and forced reward-button refresh after prisoner modal flows.

# Task: 2026-05-08 Offering Rewards Target Party Members

### Task title

Make Offering rewards apply to selected and Manifested monster states.

### Goals

- Generate Offering choices for every current run party member, including Manifested monsters.
- Track chosen rewards and learned skills per monster ID so one monster's Offering state does not block or overwrite another's.
- Preserve selected-monster legacy learned lists for existing 1P combat code while adding party-member scoped state.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected Run reward/UI code and build output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that Offering can give a Manifested monster a new skill/modifier and that the monster uses that learned state in later combat.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:884` builds Offering choices from `ResolveOfferingTargetMonsters()`.
- `RunCombatUiController.cs:943`, `:977`, `:1016`, `:1054`, `:1074`, and `:1094` take `RunSession.RunMonsterState` for active, passive, enhancement, and master Offering choices.
- `RunCombatUiController.cs:968`, `:1007`, `:1040`, and `:1127` store `MonsterId = memberState.MonsterId` on generated choices.
- `RunCombatUiController.cs:1206` records the selected Offering choice against `choice.MonsterId`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs:166` records monster-ID scoped Offering choices.
- `RunSession.cs:218` and `:229` check learned active/passive skills by monster ID.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing Unity reference warnings.
- `git diff --check` on the changed Run/Combat scripts completed with no whitespace errors, aside from Git LF-to-CRLF normalization warnings.

### History

- 2026-05-08: User clarified Manifested monsters must also grow through Offering after joining the run.
- 2026-05-08: Code Builder changed Offering generation and commit paths to carry a party-member `MonsterId`.

# Task: 2026-05-08 Summoner Return Without Manifest

### Task title

Add a `PrisonerSummonerPanel` return button.

### Goals

- Let the player leave `PrisonerSummonerPanel` and return to `RewardPanel` without attempting Manifest.
- Keep the existing result `ContinueButton` for success/failure result close.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected UI code, saved scene YAML, build output, and Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that `Back to Reward` leaves the summoner panel without adding a Manifested monster.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:64` declares `prisonerSummonerBackButton`.
- `RunCombatUiController.cs:390` creates/binds `BackButton` with label `Back to Reward`.
- `RunCombatUiController.cs:731` clears the pending Manifest candidate and returns to the reward panel.
- `Pakuri/Assets/Scenes/RunScene.unity:5233` contains `m_Name: BackButton`.
- `Pakuri/Assets/Scenes/RunScene.unity:8429` contains `m_Text: Back to Reward`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested a button on `PrisonerSummonerPanel` that returns to `RewardPanel` without summoning.
- 2026-05-08: Code Builder added `BackButton` and wired it to the no-Manifest return path.

# Task: 2026-05-08 Manifested Offering Skill Refresh Follow-up

### Task title

Refresh Manifested party state after Manifest and Offering results.

### Goals

- Ensure the first successful Manifest result is visible to the run/combat party state immediately.
- Ensure Offering choices that target Manifested monsters update the Manifested skill runtime snapshot.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that a Manifested monster receiving an Offering skill uses that learned skill in later combat.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:702` refreshes Manifested runtime after Manifest success.
- `RunCombatUiController.cs:1246` refreshes Manifested runtime after `CommitOfferingChoice(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:149` reconfigures Manifested party members from `RunSession`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User asked to check whether Offering-acquired skills on Manifested monsters actually fire.
- 2026-05-08: Code Builder verified the monster-ID Offering path and added immediate Manifested party refresh after Offering.
# Task: 2026-05-08 Offering-Acquired Manifested Skill Visual Follow-up

### Task title

Record Offering-acquired Manifested skills using skill-kind combat visuals.

### Goals

- Keep Offering target identity and learned-skill commit behavior unchanged.
- Fix the combat-side visual result of Offering-acquired Manifested skills.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Offering-acquired Manifested active skills in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:1206` records Offering choices against `choice.MonsterId`.
- `RunCombatUiController.cs:1246` refreshes the Manifested combat runtime after Offering commit.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:896` now creates Manifested non-projectile visuals from `SkillRuntimeKind` and `SkillEffectPrefab`.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported the reward/Offering side worked but the resulting Manifested skill visual was still wrong.

# Task: 2026-05-08 Offering-Acquired Manifested Sustained Duration Follow-up

### Task title

Record that Offering-acquired Manifested sustained skills now use longer visual durations.

### Goals

- Preserve the Offering path that grants skills to Manifested monster state.
- Keep the combat-side sustained visual duration fix tied to Offering-acquired skills.

### Constraints

- Role Owner is Code Builder.
- No reward UI code changed in this pass.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated through combat runtime changes.

### Next Actions

- User verifies Offering-acquired Manifested sustained skills in later combat.

### Evidence

- Existing `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:1246` refreshes Manifested combat state after Offering commit.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now uses `ResolveManifestedSkillVisualDuration(...)` for sustained learned skill visuals.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported Offering acquisition and cooldown worked, then reported sustained effects were too short.
