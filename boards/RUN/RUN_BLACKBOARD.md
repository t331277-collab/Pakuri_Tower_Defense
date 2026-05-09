# RUN_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Task: 2026-05-08 Manifested HP Bar Runtime Sprite Repair

### Task title

Record RunScene impact of manifested HP bar live sprite repair.

### Goals

- Keep Run board aligned with the 2P-5P manifested HP bar visibility fix.
- Record that the fix targets already-bound runtime instances, not only party reconstruction.

### Constraints

- Role Owner is Code Builder.
- User performs RunScene Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested party slots show HP bars during RunScene combat.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:2029` repairs `HpBarFill.sprite == null` during manifested HP/status refresh.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:2130` uses the same repair-aware path when deactivating scene slots.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle and console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User asked to resume after an interrupted fix for invisible 2P-5P HP bars.

## Task: 2026-05-08 RunScene Manifested Rin Runtime Resume

### Task title

Record RunScene impact of Rin-first manifested runtime parity.

### Goals

- Keep Run board aligned with the Rin-first combat runtime change.
- Record that 2P-5P scene slots reuse existing status children when configured as manifested monster runtimes.
- Record verification boundaries for RunScene Play Mode.

### Constraints

- Role Owner is Code Builder.
- No Play Mode verification was run by Codex.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build and Unity-MCP editor checks.

### Next Actions

- User verifies manifested Rin in 2P-5P slots during RunScene Play Mode.
- User checks that one name label, one HP label, and one HP bar are visible per manifested slot.

### Evidence

- Unity-MCP scene hierarchy inspection found `CombatRoot/2PMonster`, `3PMonster`, `4PMonster`, `5PMonster`, and `EveUnit`.
- Unity-MCP scene hierarchy inspection found slot status children named `MonsterHpLabel`, `MonsterHpBar`, and `MonsterNameLabel`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:195` resolves existing slot status views during manifested runtime creation.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1870` writes monster name, HP text, HP fill, and shield fill to the resolved views.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported 2P-5P monsters already have child HP/name objects and asked that manifestation reuse them rather than spawning overlapping duplicates.

## Task: 2026-05-08 RunScene Manifest UI And Party Runtime Report

### Task title

Record the Run-level status of Manifest UI wiring and manifested party runtime documentation.

### Goals

- Keep Run board aligned with the current HTML report.
- Record that the selected monster duplicate guard applies at run-session and combat-party boundaries.
- Record the current RunScene UI inspection warning.

### Constraints

- Role Owner is Designer for documentation.
- No gameplay code or scene edits were made in this task.
- User performs Play Mode verification.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User verifies Manifest success/failure UI and manifested party behavior in Play Mode.
- If requested, clean `RunCombatCanvas/PrisonerOfferingPanel` hierarchy in a separate edit task.

### Evidence

- `RunCombatUiController.cs:791` excludes `currentSession.SelectedMonsterId` from Manifest candidates.
- `RunSession.cs:321` and `:334` reject selected-monster IDs when recording Manifested monsters.
- `CombatRuntimeParty.cs:156` skips selected-monster IDs even if bad session state exists.
- Unity-MCP scene inspection found all required RunScene prisoner/reward panels and buttons present.
- Unity-MCP scene inspection found `PrisonerOfferingPanel` has an unexpected child `DefeatPanel` and duplicate `Title`.
- Report saved as `Pakuri/reference/Report/2026-05-08-runscene-manifest-ui-and-runtime-status.html`.

### History

- 2026-05-08: User requested RunScene panel/button wiring inspection and current manifested monster structure explanation as HTML.

## Migrated Task Blocks

## Task: 2026-05-02 Run Skill And Reward Query Expansion

### Task title

Expand run-scene gameplay queries so monster skill and reward sub-data also flow through `PakuriDataManager`.

### Goals

- Stop run UI/debug consumers from reading `monster.ActiveSkills`, `monster.PassiveSkills`, and `monster.InitialRewardChoices` directly as their primary query path.
- Reuse the new collection-level query helpers for offering, debug toggles, and fallback skill lookups.
- Keep current run UI behavior unchanged while moving the lookup contract.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual script edits and actual Unity/editor output.
- Do not run Unity Play Mode verification.
- Code Reviewer has not run yet for this follow-up phase.

### Role Owner

Code Builder

### Status

Implemented, locally validated, and later reviewed with no discrete actionable bug reported.

### Next Actions

- User can verify in Play Mode that prisoner offerings, debug skill toggles, and fallback monster skill resolution still behave the same with CSV-backed data.
- If later requested, move `RunSession` learned-state checks away from `DisplayName` strings to stable ids.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now resolves active skills, passive skills, and initial reward choices through local helpers that call `PakuriDataManager.Instance.GetActiveSkills(...)`, `GetPassiveSkills(...)`, and `GetRewardChoices(...)`.
- `RunCombatUiController.cs` still resolves the fallback monster through `PakuriDataManager.Instance.ResolveMonster(...)`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` now rebuilds active/passive toggle lists through local helpers that call `PakuriDataManager.Instance.GetActiveSkills(...)` and `GetPassiveSkills(...)`.
- `MainMenuFlowController.cs`, `RunFlowController.cs`, and `RunSceneBootstrap.cs` still use the previously unified `PakuriDataManager` roster/fallback contract.
- `Select-String` over `Pakuri/Assets/Scripts/Run/*.cs` found the new `GetActiveSkills`, `GetPassiveSkills`, and `GetRewardChoices` helper calls in `RunCombatUiController.cs` and `DebugSceneController.cs`.
- After one compile-fix pass, Unity refresh completed without C# compile errors, and the CSV validation menu still loaded the 5-monster / 8-enemy runtime catalog.
- External `codex review --uncommitted` later covered the modified run-side files (`DebugSceneController`, `RunCombatUiController`) and reported no discrete actionable bug introduced by this patch.

### History

- 2026-05-02: User asked to finish the still-partial query-contract expansion after the earlier roster-level unification.
- 2026-05-02: Builder moved run combat offering/debug sub-data lookup onto `PakuriDataManager` collection queries and revalidated the scripts in Unity.
- 2026-05-02: The later reviewer pass inspected the modified run-side query-expansion files and did not raise an actionable follow-up bug.

## Task: 2026-05-02 Run Query Contract Unification

### Task title

Route run-entry and run-UI monster queries through `PakuriDataManager`.

### Goals

- Remove direct `GameDataCatalog.Monsters` reads from MainMenu, RunFlow, and DebugScene gameplay/UI paths.
- Keep fallback monster resolution in run-scene entry/UI behind one manager contract.
- Update missing-data messaging to the current CSV runtime source instead of the legacy seeder flow.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual script edits and actual Unity/editor output.
- Do not run Unity Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User can verify in Play Mode that MainMenu, RunScene fallback entry, and DebugScene still show/select monsters correctly with CSV-backed data.
- If later requested, move more run-scene setup to stable-id/context contracts so scene scripts no longer carry serialized catalog references at all.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs:165` now builds character-select buttons from `PakuriDataManager.Instance.GetMonsters(gameDataCatalog)`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs:70` and `:217` now read the monster roster through `PakuriDataManager` and no longer iterate `gameDataCatalog.Monsters` directly.
- `Pakuri/Assets/Scripts/Run/RunFlowController.cs:188` now resolves the monster list through `PakuriDataManager`, and `RunFlowController.cs:192` changed the missing-data hint to `Assets/CSVdata/source` and `Pakuri/CSVRuntime`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:898` now resolves its fallback monster through `PakuriDataManager.Instance.ResolveMonster(...)`.
- `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs:62` now resolves its fallback monster through the same `ResolveMonster(...)` contract.
- After the change, the script-tree `Select-String` query for `gameDataCatalog.Monsters|gameDataCatalog.StageOneEnemies|fallbackCatalog.Monsters` no longer found run-scene consumer usage.
- Unity `read_console` after script refresh showed the runtime catalog load log and existing missing-script warnings, but no C# compile error entries.

### History

- 2026-05-02: User requested implementation of the previously identified query-contract unification.
- 2026-05-02: Builder replaced remaining run-scene monster roster and fallback-monster reads with `PakuriDataManager` helpers.
- 2026-05-02: Builder also updated the RunFlow missing-data text so it no longer points to the legacy `Pakuri/Seed Default Game Data` menu.

## Task: 2026-05-02 Run Catalog Source Resolution To CSV Runtime Data

### Task title

Switch run-scene catalog resolution to the new CSV runtime loader.

### Goals

- Make run-entry and run-UI scripts consume the typed CSV runtime catalog on startup.
- Keep serialized `GameDataCatalog` fields only as scene references, not as the hidden runtime source after CSV failure.
- Use the new query contract for monster-id lookup in run flow entry points.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual script edits and actual build/console output.
- Do not run Unity Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally revalidated after the CSV Reviewer follow-up.

### Next Actions

- User can verify in Play Mode that MainMenu -> RunScene and DebugScene still enter combat with CSV-backed data.
- If later requested, replace display-name based learned-skill checks in `RunSession` with stable ids from the CSV source.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`, `RunFlowController.cs`, `DebugSceneController.cs`, `RunCombatUiController.cs`, and `RunSceneBootstrap.cs` still resolve their catalog through `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)`.
- `Pakuri/Assets/Scripts/Run/RunFlowController.cs` now uses `PakuriDataManager.Instance.GetData<MonsterDefinition>(currentSession.SelectedMonsterId)` when starting or retrying combat.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now resolves the fallback monster through `PakuriDataManager.Instance.GetData<MonsterDefinition>(RunSceneBootstrap.FallbackMonsterId)`.
- `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs` now resolves its fallback monster through `PakuriDataManager.Instance.GetData<MonsterDefinition>(fallbackMonsterId)`.
- `PakuriCsvRuntimeData` initializes from `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`, and on failure now returns `null` instead of silently falling back to the serialized scene catalog.
- Unity refresh after the follow-up created the new data-script `.meta` files and later console reads showed no C# compile errors.
- `Pakuri/Validate CSV Source Data` previously logged a successful in-memory load from resource source `Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog`.

### History

- 2026-05-02: Initial builder pass updated run-scene consumers to prefer the runtime CSV catalog while preserving their serialized field.
- 2026-05-02: Reviewer follow-up request led Builder to add `PakuriDataManager` monster lookup and to block serialized fallback use after CSV initialization failure.
- 2026-05-02: Unity refresh after the follow-up completed without C# compile errors.

## Task: 2026-05-01 Run Structure Expansion Risk Review

### Task title

Review `RunSession` and run flow scripts for future content expansion risks.

### Goals

- Inspect the actual run-state and run-UI scripts under `Pakuri/Assets/Scripts/Run`.
- Identify structural issues for adding elite/shop branches, richer reward types, or persistent progression.

### Constraints

- Role Owner is Designer.
- Base all findings on actual script content and command output.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- If the user requests follow-up design, separate the work into `RunSession` identity cleanup, branchable day-flow design, and UI prefab/template strategy.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunSession.cs` stores learned actives/passives by display name strings, not stable IDs, and `RunCombatUiController.cs` / `DebugSceneController.cs` also check learned state by `skill.DisplayName` / `passive.DisplayName`.
- `Pakuri/Assets/Scripts/Run/RunDayModel.cs` clamps stages to `1..4` and days to `1..11`; `RunCombatType.Shop` exists in the enum, but `RunDayModel.Resolve(...)` never returns `Shop` or `Elite`.
- `Select-String` over `Pakuri/Assets/Scripts/**/*.cs` found `HasEliteOption` / `HasShopOption` only in `RunDayModel.cs`, so those branch flags are currently unused.
- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs` moves state to `RunScene` through `RunStartContext` and `DontDestroyOnLoad`; there is no disk-backed save or reload path in `Pakuri/Assets/Scripts`.
- `Pakuri/Assets/Scripts/Run/RunFlowController.cs` and `RunCombatUiController.cs` both implement reward-state UI and button generation against `CombatRuntimeController`, creating duplicate flow logic.
- Legacy non-English note retained these ASCII code references: `RunFlowController.cs`.
- `Get-ChildItem Pakuri/Assets -Recurse -Filter *.prefab`, `*.uxml`, and `*.uss` returned `0`, so run UI is still fully code-built rather than asset-templated.

### History

- 2026-05-01: Reviewed `RunSession.cs`, `RunDayModel.cs`, `RunStartContext.cs`, `MainMenuFlowController.cs`, `RunFlowController.cs`, `RunCombatUiController.cs`, and `DebugSceneController.cs` for content expansion risk.

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

## Task: SaveAndLoad Direction Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `reference/4.run`, `reference/6.meta`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
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

- Legacy non-English note retained these code references: `RunSession`, `MetaSaveData`, `RunSnapshot`, `SaveLoadService`.
- Legacy non-English note retained these code references: `GameDataCatalog`, `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/4.run/dungeon-squad-run-structure.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/shop-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/event-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-index.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/active-skill-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/dark-trace-currency-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`, `Assets/Resources`, `Assets/StreamingAssets`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/save-and-load-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `monster-select-run-ui-expansion-plan.html`, `monster-select-run-ui-builder-handoff.html`, `reference/4.run`, `reference/6.meta`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `MetaSaveData`, `RunSnapshot`, `EphemeralRuntime`, `Pakuri/reference/save-and-load-plan.html`.
- Legacy non-English note retained these code references: `Pakuri/data`, `save-and-load-plan.html`.

## Task: Run Systems Integration Summary Report

### Task title

Legacy non-English note retained these code references: `monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan`.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.

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
- Legacy non-English note retained these code references: `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`, `MetaSaveData`, `RunSnapshot`, `GameDataCatalog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `Scenes`, `Screenshots`, `Scripts`, `Settings`, `Resources`, `StreamingAssets`, `DataGenerated`.
- Legacy non-English note retained these code references: `.uxml`, `.uss`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/rin/rin-tower.md`, `rin/skill/g~j`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/4.run/combat-reward-system.md`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `RunSession`, `run-systems-integration-summary-report.html`.

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

# Task: 2026-05-07 Character Skill Effect Pipeline Review

### Task title

Run character selection and session structure review summary

### Goals

- Preserve run-side conclusions from the structure review.

### Constraints

- Evidence must come from inspected scripts and Unity-MCP output.
- Designer review only; no run code implementation was performed.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- See `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.
- Future run work should move learned active/passive state from display-name strings to stable skill/passive IDs.

### Evidence

- `MainMenuFlowController.StartRun` calls `RunStartContext.Ensure().PrepareNewRun(selectedMonster)`.
- `RunStartContext.cs` stores `SelectedMonster` and `RunSession`, and keeps the context with `DontDestroyOnLoad`.
- `RunSceneBootstrap.cs` starts combat from pending context or fallback monster.
- `RunSession.cs` stores `LearnedActives` and `LearnedPassives` as `List<string>`, and `RunCombatUiController.cs` checks learned actives with `skill.DisplayName`.
- Report created at `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.

### History

- 2026-05-07: User requested current character creation, skill, and effect pipeline review. Designer documented the run selection/session flow and the display-name learned-skill risk.
# Task: 2026-05-07 RunSession Learned Skill ID Refactor

### Task title

Refactor RunSession learned active/passive state to store stable skill IDs.

### Goals

- Store learned active skills as `SkillDefinition.SkillId` values in `RunSession.LearnedActives`.
- Store learned passive skills as `PassiveDefinition.PassiveId` values in `RunSession.LearnedPassives`.
- Keep display text sourced from definitions such as `SkillDefinition.DisplayName` instead of using display names for learned-state logic.

### Constraints

- Role Owner is Code Builder because the user explicitly requested refactoring implementation.
- Evidence must come from inspected code, build output, and Unity-MCP output.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should verify in Play Mode that newly offered active/passive choices unlock and enhance the expected skills.
- Code Reviewer execution remains deferred until explicit user permission.

### Evidence

- Changed `Pakuri/Assets/Scripts/Run/RunSession.cs` to add `ActiveSkillId`/`PassiveSkillId`, resolve default active ID from `IsDefaultLearned` or slot A, and use `AddLearnedActive`/`AddLearnedPassive` with IDs.
- Changed `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` so offering choices store `ActiveSkillId`/`PassiveSkillId`, and `HasLearnedActive`/`HasLearnedPassive` checks use `skill.SkillId`/`passive.PassiveId`.
- Changed `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` so debug sessions add selected active/passive IDs instead of display names.
- Changed `Pakuri/Assets/Scripts/Run/RunFlowController.cs` so passive reward unlock passes `SelectedMonsterPassiveId`.
- Search evidence after edits found no remaining `HasLearnedActive(skill.DisplayName)`, `HasLearnedPassive(passive.DisplayName)`, `session.LearnedActives.Add(skill.DisplayName)`, or `session.LearnedPassives.Add(passive.DisplayName)` matches under `Pakuri/Assets/Scripts`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity-MCP `execute_code` result: `monster=ariel, activeSkillId=ariel-a, firstLearnedActive=ariel-a, hasSkillId=True, hasDisplayName=False`.
- Unity-MCP console warning/error check after compile returned only MCP client handler logs.

### History

- 2026-05-07: User asked to begin refactoring from the report's first priority: make `RunSession.LearnedActives` and `LearnedPassives` ID based.
- 2026-05-07: Code Builder implemented the ID-based learned-state path and validated build/editor behavior without Play Mode.

# Task: 2026-05-08 RunScene Prisoner Manifest Party Implementation

### Task title

Record prisoner Manifest results in `RunSession` and feed the next combat party.

### Goals

- Store Manifested monster IDs in the active run session.
- Keep 1P as the selected monster and add Manifested monsters from 2P onward on the next combat start.
- Keep initial Manifest combat participation limited to automatic A/basic attack behavior.

### Constraints

- Role Owner is Code Builder because the user explicitly requested implementation.
- Evidence must come from inspected files and build/Unity-MCP output.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify prisoner reward -> choice -> Manifest success/failure -> next combat party behavior.
- Code Reviewer execution remains deferred until explicit user permission.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunSession.cs` now stores `ManifestedMonsterIds`, `HasManifestedMonster(...)`, and `RecordManifestedMonster(...)`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` records successful Manifest results through `currentSession.RecordManifestedMonster(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` calls `ConfigureManifestedMonsterParty(session)` when a configured day begins.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` resolves `RunSession.ManifestedMonsterIds` through `PakuriDataManager`, skips the selected monster, and exposes party panel data with 1P at index 0 and Manifested monsters from index 1.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP imported `Assets/Scripts/Combat/CombatRuntimeParty.cs`; Unity console error query returned only MCP client-handler logs, not project compile errors.

### History

- 2026-05-08: User requested the recommended RunScene order: Rin CSV/SO cleanup, Manifest result storage, next-combat party read, 2P+ display, and limited A/basic auto-combat.

# Task: 2026-05-08 RunScene Runtime UI State Gate

### Task title

Start RunScene runtime with combat HUD only and let game logic own later panel transitions.

### Goals

- Prevent editor-visible panels from leaking into the initial RunScene runtime state.
- Keep `HudPanel` and `MonsterPanel` available during combat.
- Keep reward/prisoner/Manifest/defeat panels controlled by victory/reward/prisoner/defeat logic.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code, scene hierarchy, build output, and Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that the initial combat state shows only HUD/Monster UI and that victory/defeat/reward actions still reveal the correct panels.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:111` calls `ShowRuntimeHudOnly()` from Play-mode `OnEnable`, so runtime state is applied before the existing `Start()` call at `:138`.
- `RunCombatUiController.cs:438` through `:447` hides reward, prisoner, prisoner choice, prisoner summoner, defeat, and legacy `PrisonerOfferingPanel`, while keeping `HudPanel` and `MonsterPanel` active.
- `RunCombatUiController.cs:453`, `:590`, `:624`, `:823`, `:1170`, and `:1192` remain the game-logic activation points for reward, prisoner choice, Manifest, Offering, continue, and defeat states.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP console error query after script refresh returned only MCP client-handler logs.

### History

- 2026-05-08: User requested checking and enforcing the Play 기준 where all UI may be active before RunScene entry but only `HudPanel` and `MonsterPanel` remain active on entry.
# Task: 2026-05-08 Prisoner Reward Transition Bugfix

### Task title

Keep prisoner reward selection on the prisoner-choice transition path.

### Goals

- Make prisoner reward selection leave the reward list and enter prisoner choice UI.
- Preserve normal reward-list rebuild behavior for non-prisoner rewards.

### Constraints

- Role Owner is Code Builder.
- Detailed reward/UI evidence is recorded in `boards/RUN/REWARD_BLACKBOARD.md` and `boards/UI/RUNSCENE_UI.md`.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify the prisoner reward transition from victory reward state.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now evaluates prisoner rewards with `IsPrisonerReward(rewardView, rewardId)` before reward-button rebuild.
- Non-prisoner rewards still call `RebuildRewardButtons()` and then `EnterRewardState()`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported the prisoner reward click did not open a panel and only changed the button to acquired.

## Task: 2026-05-08 PrisonerChoicePanel Per-Frame Reward State Overwrite Fix

### Task title

Stop victory reward-state refresh from immediately hiding prisoner reward modals.

### Goals

- Fix the remaining case where clicking a prisoner reward opened no visible `PrisonerChoicePanel`.
- Preserve normal reward-state refresh when no prisoner modal is active.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code, build output, and Unity-MCP output.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that a prisoner reward click keeps `PrisonerChoicePanel` visible instead of returning to the claimed reward list.
- Code Reviewer execution remains deferred until explicit user permission.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:157` enters the victory UI path from `Update()`.
- `RunCombatUiController.cs:159` through `:164` now returns early when `IsRewardModalOpen()` is true, preventing per-frame `EnterRewardState()` from overwriting prisoner modals.
- `RunCombatUiController.cs:458` through `:480` shows that `EnterRewardState()` activates `RewardPanel` and hides `PrisonerChoicePanel`, `PrisonerSummonerPanel`, `PrisonerPanel`, and `PrisonerOfferingPanel`.
- `RunCombatUiController.cs:566` through `:568` still routes prisoner rewards to `OpenPrisonerChoicePanel(...)`, and `:599` through `:603` hides `RewardPanel` then activates `PrisonerChoicePanel`.
- `RunCombatUiController.cs:1606` through `:1617` now treats active prisoner choice, Manifest, Offering, or legacy offering panels as modal reward UI.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh was requested; console warning/error read returned existing missing-script entries and MCP client logs, not C# compile errors.

### History

- 2026-05-08: User reported the issue persisted after the first prisoner reward click fix.
- 2026-05-08: Builder found the remaining cause in the per-frame victory `Update()` path and added the modal guard.

# Task: 2026-05-08 Prisoner Offering Panel And Manifested Party Follow-up

### Task title

Fix RunScene prisoner Offering panel routing, Manifest reward return state, and Manifested party baseline state.

### Goals

- Make the prisoner choice `Offering` button open `PrisonerOfferingPanel`, not the legacy/generated `PrisonerPanel`.
- Rebuild reward buttons after Manifest/Offering modal return so the already-used prisoner reward button shows claimed/used state.
- Keep Manifested party monsters based on their own monster stats, HP, and A skill.

### Constraints

- Role Owner is Code Builder because the user explicitly requested a fix.
- Evidence must come from inspected scene hierarchy, scripts, build output, and Unity-MCP output.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify prisoner reward -> Offering opens `PrisonerOfferingPanel`, Manifest result -> reward return shows the prisoner reward as used, and next combat Manifested monsters auto-attack nearest enemies with A/basic behavior.
- Code Reviewer execution remains deferred until explicit user permission.

### Evidence

- Unity-MCP scene inspection found `RunCombatCanvas/PrisonerOfferingPanel` with `Choice1`, `Choice2`, `Choice3`, and `Title`; `RunCombatCanvas/PrisonerPanel` also exists as a separate inactive object.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now treats `PrisonerOfferingPanel` as the offering button root and only falls back to `PrisonerPanel` if the offering panel is absent.
- `RunCombatUiController.cs` now hides `PrisonerPanel` during Offering and activates `PrisonerOfferingPanel` for the actual offering choices.
- `RunCombatUiController.cs` now resets `rewardPanelEntered = false` when returning from Manifest result or committed Offering, forcing reward buttons to rebuild and show claimed state.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now stores Manifested monster `MaxHealth`, `CurrentHealth`, `BaseDamage`, and `PowerStat` from that monster's definition and displays HP in the Manifested party label.
- `CombatRuntimeParty.cs` already resolved the Manifested monster A skill from `SkillSlot.A` and selected the nearest living enemy by distance; this follow-up preserved that behavior.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh returned editor state `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported Offering opened `PrisonerPanel` instead of the real `PrisonerOfferingPanel`, Manifested monsters behaved oddly, and returning from Manifest did not mark the prisoner reward button as used.
- 2026-05-08: Code Builder fixed panel routing, reward-button rebuild on modal return, and explicit Manifested party HP/stat baseline storage.

# Task: 2026-05-08 Manifested Party Member Growth State

### Task title

Make Manifested monsters join the run as growable party-member monster states.

### Goals

- Treat a Manifested monster as the same kind of baseline monster state as a MainMenu-selected starting monster, but added during the run.
- Keep each party member's learned actives, learned passives, chosen rewards, and reward modifiers separate by monster ID.
- Let prisoner Offering choices target selected and Manifested party members so Manifested monsters can gain skills and become stronger.

### Constraints

- Role Owner is Code Builder because the user explicitly requested a fix.
- Evidence must come from inspected code, build output, and Unity-MCP output.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that a Manifested monster starts with its own baseline skill state, then receives Offering skills/modifiers as that same party member in later combat.
- Code Reviewer execution remains deferred until explicit user permission.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunSession.cs:11` defines `RunMonsterState`.
- `RunSession.cs:50` stores `PartyMembers`.
- `RunSession.cs:333` records a Manifested monster from `MonsterDefinition`.
- `RunSession.cs:389` ensures a party-member state and copies the monster's default active skills into that state.
- `RunSession.cs:166`, `:218`, and `:229` provide monster-ID scoped Offering choice and learned-skill checks.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:907` resolves Offering target monsters from selected plus Manifested party members.
- `RunCombatUiController.cs:1206` records Offering choices against `choice.MonsterId`.
- `RunCombatUiController.cs:1209` applies reward modifiers through the monster-ID scoped `AccumulateReward` flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.
- `git diff --check -- Pakuri\Assets\Scripts\Run\RunSession.cs Pakuri\Assets\Scripts\Run\RunCombatUiController.cs Pakuri\Assets\Scripts\Combat\CombatRuntimeParty.cs` completed with no whitespace errors; Git only reported LF-to-CRLF normalization warnings.

### History

- 2026-05-08: User clarified that Manifested monsters should be equivalent to MainMenu-starting monsters added during gameplay, not unregistered weird-skill users.
- 2026-05-08: Code Builder added per-party-member run state and made Offering choices apply to selected and Manifested monsters by monster ID.

# Task: 2026-05-08 Manifested Scene Slot Runtime

### Task title

Use `CombatRoot/2PMonster` through `5PMonster` as Manifested monster runtime slots.

### Goals

- Stop creating separate Manifested monster GameObjects when scene slots exist.
- Activate Manifested monsters in order through `2PMonster`, `3PMonster`, `4PMonster`, and `5PMonster`.
- Add a `PrisonerSummonerPanel` button that returns to `RewardPanel` without attempting Manifest.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected scene hierarchy, code, build output, and Unity-MCP output.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify prisoner Manifest opens the summoner panel, Back to Reward returns without summoning, and successful Manifest activates `2PMonster` then `3PMonster` etc.

### Evidence

- Unity-MCP found `CombatRoot/EveUnit`, `CombatRoot/2PMonster`, `CombatRoot/3PMonster`, `CombatRoot/4PMonster`, and `CombatRoot/5PMonster`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:41` defines the Manifested slot names as `2PMonster` through `5PMonster`.
- `CombatRuntimeParty.cs:139` resolves the scene slot before falling back to a generated object.
- `CombatRuntimeParty.cs:170` marks whether a Manifested runtime uses a scene slot.
- `CombatRuntimeParty.cs:562` clears Manifested state by deactivating scene slots instead of destroying them.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:390` creates/binds `BackButton` on `PrisonerSummonerPanel`.
- `RunCombatUiController.cs:731` returns from the summoner panel without Manifest.
- `Pakuri/Assets/Scenes/RunScene.unity:5233` contains `m_Name: BackButton`, and `:8429` contains `m_Text: Back to Reward`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.
- `git diff --check` on the changed scripts completed with no whitespace errors; full scene diff check reports Unity YAML trailing whitespace lines after scene save.

### History

- 2026-05-08: User clarified that `2PMonster` through `5PMonster` are already placed under `CombatRoot` and should be used as Manifested monster slots.
- 2026-05-08: Code Builder changed Manifested combat to use those scene slots and added the summoner back-to-reward button.

# Task: 2026-05-08 Manifested Summon Immediate Sync And Vega A Follow-up

### Task title

Fix first Manifested summon synchronization and Manifested Vega A projectile behavior.

### Goals

- Make a successful `PrisonerSummonerPanel/SummonButton` update the active combat party snapshot immediately instead of waiting for a later rebuild.
- Keep `ContinueButton` into the next day using the same `RunSession` Manifested party state.
- Ensure Offering choices for Manifested monsters refresh the party snapshot after learned-skill or modifier state changes.

### Constraints

- Role Owner is Code Builder because the user explicitly requested inspection and correction.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for Summon -> summoner Continue -> reward Continue, first Manifested party appearance, and Offering-acquired Manifested skills firing in later combat.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:702` calls `combatController.RefreshManifestedMonsterParty(currentSession)` immediately after successful `RecordManifestedMonster(...)`.
- `RunCombatUiController.cs:1246` calls the same refresh after `CommitOfferingChoice(...)`, so Manifested party skill snapshots are updated after Offering changes.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:149` exposes `RefreshManifestedMonsterParty(RunSession session)` as a public sync path that reconfigures and resets Manifested party runtime from the session.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported that the first SummonButton + ContinueButton flow appeared not to apply Manifested party state until the next Manifest.
- 2026-05-08: Code Builder added immediate session-to-party refresh after Manifest success and after Offering choice commit.
# Task: 2026-05-08 Manifested Offering Skill Effect Follow-up

### Task title

Make Offering-acquired Manifested skills show skill-kind visuals in combat.

### Goals

- Preserve the existing Offering path that grants skills to selected and Manifested party-member states.
- Ensure Manifested learned skills that fire through combat do not always draw as a generic beam.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that a Manifested monster can receive a skill through Offering, cooldown it, and show the proper non-beam effect when it fires.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:1246` already refreshes Manifested runtime after Offering choice commit.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:512` now routes non-projectile Manifested casts through `CreateManifestedSkillVisual(...)`.
- `CombatRuntimeParty.cs:896` uses `SkillRuntimeKind` to choose area/self/execute/line visuals.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported Offering acquisition and cooldown worked, but the Manifested monster's own effect did not appear.

# Task: 2026-05-08 RunScene Selected Monster Anchor Confirmation

### Task title

Confirm RunScene applies the selected monster to `EveUnit` and record Manifested duration follow-up.

### Goals

- Answer whether RunScene currently applies the selected monster to `EveUnit`.
- Keep run-state context aligned with the Manifested sustained skill correction.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies selected 1P monster visual and Manifested sustained skill duration in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs` calls `combatController.BeginConfiguredDay(monster, session, fallbackCatalog)` from `BeginCombat(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` calls `ConfigureMonster(monster)` during `BeginConfiguredDay(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` creates or resolves `eveAnchor` as `"EveUnit"`, sets `selectedUnitSprite = monster.UnitSprite`, and applies it through `EnsureSpriteRenderer(eveAnchor, ..., selectedUnitSprite)`.
- Runtime and Editor builds completed with 0 errors after the Manifested duration fix.

### History

- 2026-05-08: User asked whether the current structure applies the selected monster to `EveUnit` when entering RunScene.

# Task: 2026-05-08 Manifest Candidate Duplicate And Failure Popup

### Task title

Prevent selected-monster duplicate Manifest and move Manifest chance to `ManifestButton`.

### Goals

- Stop the current 1P selected monster, including Eve, from being added again through Manifest.
- Roll Manifest success/failure from `PrisonerChoicePanel/ManifestButton`, not `PrisonerSummonerPanel/SummonButton`.
- Show a Manifest failure popup when the roll fails.

### Constraints

- Role Owner is Code Builder because the user explicitly requested behavior changes.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that the selected monster is excluded from Manifest candidates, Manifest rolls immediately on `ManifestButton`, success still adds a new monster, and failure opens the popup.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:367` binds `ManifestButton` to `TryManifestPrisonerMonster`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:391` no longer binds `SummonButton` to the Manifest roll path.
- `RunCombatUiController.cs:396` creates `PrisonerManifestFailurePopup`, and `:657` / `:665` show it for no-candidate or failed rolls.
- `RunCombatUiController.cs:791` excludes `currentSession.SelectedMonsterId` from Manifest candidates.
- `Pakuri/Assets/Scripts/Run/RunSession.cs:321` and `:334` reject direct attempts to record the selected monster as Manifested.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh reached idle; console warning/error read returned only MCP client handler logs.
- `git diff --check` on the changed Run/Combat scripts completed with no whitespace errors, aside from Git LF-to-CRLF normalization warnings.

### History

- 2026-05-08: User reported Eve could be Manifested again while already present and requested the Manifest success chance move from `SummonButton` to `ManifestButton` with a failure popup.
