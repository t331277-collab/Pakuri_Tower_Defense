## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-08` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/RUN/REWARD_BLACKBOARD.md`.

## Task: 2026-05-16 Stage Reward CSV Seed

### Task title

Create active Stage reward rule CSV rows for NewRunScene Stage Flow.

### Goals

- Store Stage 1 reward payout rules outside code.
- Include prisoner count probabilities, gold, dark trace, elite bonus prisoner count, and artifact choice count.
- Keep artifact UI implementation deferred while preserving data columns for later use.

### Constraints

- Role Owner is Code Builder.
- CSV data only; no reward UI or prisoner flow code was changed.
- Event, shop, and artifact UI behavior remain unimplemented in this slice.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV consistency verified.

### Next Actions

- Future Stage Flow implementation should read `StageReward.csv` and apply rewards to `RunSession`.
- User-authored reward UI should display the parsed prisoner/gold/dark trace result after enemy clear.

### Evidence

- Added `Pakuri/Assets/CSVdata/StageReward.csv`.
- `StageReward.csv` has rows for `reward-stage1-normal`, `reward-stage1-elite`, `reward-stage1-midboss`, `reward-stage1-day10-midboss`, and `reward-stage1-boss`.
- `StageReward.csv` stores prisoner count odds `0.05`, `0.80`, and `0.15`, matching `Pakuri/reference/4.run/combat-reward-system.md`.
- `StageReward.csv` stores Stage 1 gold/dark trace values normal `10/10`, midboss `30/20`, and boss `50/50`.
- `Import-Csv` and cross-file consistency checks reported no missing reward references from `StageDay.csv`.
- `NewRunStageManager` reads `StageReward.csv` and exposes pending gold, dark trace, prisoner count, and prisoner IDs when the flow reaches `RewardReady`.
- Reward UI, Manifest UI, Offering UI, and artifact reward UI were not implemented in this slice.

### History

- 2026-05-16: User requested active CSV files including reward rules for the next StageManager implementation.
- 2026-05-16: Code Builder added StageManager reward-ready state and pending reward properties for future UI wiring.

## Task: 2026-05-17 Eve Offering Skill Choice Reward Mapping

### Task title

Map Eve skill choice IDs into NewRunScene Offering rewards.

### Goals

- Let Offering rewards store the same Eve choice IDs used by `SkillChoiceModifierData.csv`.
- Avoid showing B-E active enhancements before the corresponding active skill is learned.
- Avoid showing F-J passive enhancements before the corresponding passive is learned.
- Prevent selected modifiers for one skill from mutating another skill's execution snapshot.

### Constraints

- Role Owner is Code Builder.
- NewRunScene UI layout was not redesigned.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Offering order and random choice feel in Play Mode.
- Later passive runtime work should consume the `DataOnlyUnsupported` passive trait rows.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` now contains 50 Eve reward rows matching the 50 Eve choice/modifier IDs.
- `InGameUIManager.cs` filters skill-choice reward IDs by learned active/passive ownership before adding enhancement choices to the Offering popup.
- `SkillChoiceResolver.cs` filters modifier records against the current skill's `EnhancementChoices` and `MasterChoices`, preventing cross-skill modifier leakage.
- CSV consistency check returned no missing reward, source choice, or modifier links for Eve choice IDs.
- Runtime/editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-17: User said data cleanup and Offering mapping should come first before Eve full skill implementation, then asked Code Builder to perform that work.

## Task: 2026-05-08 Manifested Runtime Resume Reward Context

### Task title

Record that this Rin-first runtime pass did not change reward selection code.

### Goals

- Keep reward board aligned because manifested monsters are acquired through prisoner Manifest flow.
- Record that this pass changed combat/runtime status binding, not reward candidate or Offering selection code.

### Constraints

- Role Owner is Code Builder.
- No Play Mode verification was run by Codex.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Context recorded; no reward code changes were made in this pass.

### Next Actions

- User verifies Manifest acquisition plus manifested Rin skill behavior in Play Mode.
- If reward candidate or Offering choice behavior is wrong, inspect `RunCombatUiController` and `RunSession` in a separate focused pass.

### Evidence

- `git status --short` showed modified reward/run files already existed in the worktree, but this pass changed combat runtime files and board records only.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:195` and `:1870` contain the manifested slot status-view reuse work.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:128` contains the Rin unit-runtime dispatch work.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User resumed a manifested monster runtime task whose acquisition context is RunScene Manifest flow, but the requested fix was combat/runtime parity and status UI reuse.

## Task: 2026-05-08 Manifest UI Wiring Status Report

### Task title

Record current prisoner Manifest reward UI wiring inspection.

### Goals

- Keep reward/prisoner modal state aligned with the current scene inspection.
- Record that `ManifestButton` owns the Manifest roll and failure popup path in current code.
- Record the UI hierarchy warning found in `PrisonerOfferingPanel`.

### Constraints

- Role Owner is Designer for documentation.
- No gameplay code or scene edits were made.
- User performs Play Mode verification.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User verifies reward -> prisoner choice -> Manifest success/failure flow in Play Mode.
- If UI is visually wrong, first inspect or clean the nested `PrisonerOfferingPanel/DefeatPanel` and duplicate `Title`.

### Evidence

- `RunCombatUiController.cs:367` binds `ManifestButton` to `TryManifestPrisonerMonster`.
- `RunCombatUiController.cs:391` binds `SummonButton` to result close rather than the Manifest roll.
- `RunCombatUiController.cs:396` through `:400` creates/binds `PrisonerManifestFailurePopup`.
- `RunCombatUiController.cs:1711` through `:1717` includes the failure popup and prisoner panels in `IsRewardModalOpen()`.
- Unity-MCP scene inspection found all required prisoner reward panels and buttons present with button components and label text.
- Unity-MCP scene inspection found `PrisonerOfferingPanel` has an unexpected child `DefeatPanel` and duplicate `Title`.
- Report saved as `Pakuri/reference/Report/2026-05-08-runscene-manifest-ui-and-runtime-status.html`.

### History

- 2026-05-08: User requested current panel/button connection inspection and an HTML summary of current Manifest runtime structure.

## Migrated Task Blocks

## Task: 2026-05-08 Manifest Candidate Selected Monster Fix

### Task title

Allow the MainMenu-selected monster to appear as a Manifest candidate.

### Goals

- Fix the Manifest candidate filter that hid Eve when Eve was the selected MainMenu monster.
- Allow selecting Eve through Manifest to record Eve in `RunSession.ManifestedMonsterIds`.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies in RunScene Play Mode that Eve appears as a Manifest candidate and is added after a successful Manifest choice.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` filters Manifest candidates through `currentSession.HasManifestedMonster(monster.MonsterId)`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs` now makes `HasManifestedMonster(...)` check only `ManifestedMonsterIds`, not `SelectedMonsterId`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs` keeps `RecordManifestedMonster(...)` guarded by `HasManifestedMonster(...)`, so Eve can now be recorded when it is not already in `ManifestedMonsterIds`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings; Unity refresh returned idle and console error query returned only MCP client handler exit logs.

### History

- 2026-05-08: User reported Eve did not appear in Manifest candidates and selecting Eve did not add Eve.

## Task: 2026-05-08 Eve Unit Offering Runtime Choices

### Task title

Use unit-owned Offering choices in shared Eve skill execution.

### Goals

- Record that Eve B-E shared unit execution reads per-unit Offering choices.
- Keep selected and manifested Eve skill enhancements aligned with each unit's `RunMonsterState`.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies Offering-enhanced Eve B-E on selected and manifested Eve in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` uses `HasManifestedChoice(runtime, ...)` in shared Eve B-E caster methods.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` configures selected and manifested unit runtimes with `RunSession.EnsurePartyMemberState(...)` / per-member `RunMonsterState`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested object-oriented skill ownership so Offering-enhanced skills behave the same for selected and manifested units.

## Task: 2026-05-08 Manifested Offering Skill State

### Task title

Use per-monster Offering state when manifested Eve casts Frost Field.

### Goals

- Ensure Offering choices recorded on a manifested Eve affect the manifested Eve skill runtime.
- Record that Eve C trait choices are read from the manifested unit's `RunMonsterState.ChosenRewardIds`.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated for manifested Eve C.

### Next Actions

- User verifies Offering-acquired Frost Field on manifested Eve in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `RunSession.RecordOfferingChoice(string monsterId, ...)` already records choices into per-member `RunMonsterState.ChosenRewardIds` for non-selected party members.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now reads manifested Eve C trait checks from `runtime.State.ChosenRewardIds` via `HasManifestedChoice(...)`.
- The manifested Eve C persistent effect now applies trait 1 radius/duration, trait 2 tick/chill stacks, trait 3 damage/cooldown, trait 4 radius/damage, and trait 5 damage/freeze duration.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User clarified that Offering-enhanced skills on manifested units should behave like the same skill on the MainMenu-selected unit.
## Task: 2026-05-16 NewRunScene Reward Buttons And Prisoner Flow

### Task title

Drive NewRunScene rewards through RewardPanel buttons.

### Goals

- Clone reward buttons from `RewardBtnContainer` according to pending gold, dark trace, and prisoner rewards.
- Apply gold/dark trace only when their buttons are clicked.
- Use prisoner buttons for Offering or Manifest, then disable the consumed prisoner button.
- Store Manifest success chance in active reward CSV data.

### Constraints

- Role Owner is Code Builder.
- User-authored UI hierarchy is used as-is: `RewardPanel`, `PrisonerChoicePopUp`, `OfferingPanel`, `MenifestedFailPopUp`, and `MenifestedSuccessPopUp`.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies reward button count/spacing, resource counts, prisoner button disabled state, Offering choice application, and Manifest success/failure popups in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` creates reward buttons from `PrisonerBtn`, `GoldBtn`, and `DarkBtn`, updates `Goldinfo`, `Darkinfo`, and `StageInfo`, and binds Offering/Manifest popup buttons.
- `Pakuri/Assets/CSVdata/StageReward.csv` now has `manifest_success_chance` with `0.70` in all active reward rows.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunStageManager.cs` exposes `PendingManifestSuccessChance`, `PendingGoldReward`, `PendingDarkTraceReward`, and `PendingPrisonerEnemyIds` for the UI.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.
- Reviewer reported duplicate prisoner sampling from one encounter row; Builder fixed candidate selection to sample without exceeding spawned row `Count`.

### History

- 2026-05-16: User specified the reward UI click flow and 70/30 Manifest probability from the prisoner-choice reference.
- 2026-05-16: Builder implemented the reward UI controller and CSV-backed Manifest probability.
