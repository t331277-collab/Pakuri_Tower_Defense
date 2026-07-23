## Archived History

- Non-July task blocks from `boards\RUN\REWARD_BLACKBOARD.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-08` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/RUN/REWARD_BLACKBOARD.md`.

## Task: 2026-07-23 Manifestation Through Unified Party IDs

### Task title

Choose and register manifested Monsters through the deployed-party ID authority.

### Goals

- Build manifestation candidates from Monsters absent from `PartyMonsterIds`.
- Add a successful candidate once and use its returned party index as the spawn slot.
- Remove duplicate selected-versus-manifested checks from reward flow.

### Constraints

- Role Owner is Code Builder.
- Prisoner consumption, manifestation success roll, random candidate selection, popups, and completion callbacks remain unchanged.
- Maximum party size remains five.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, solution-build verified, and Unity console checked.

### Next Actions

- User verifies every successful manifestation selects only an ID absent from the current deployed party and occupies the next slot.

### Evidence

- `ResolveNextManifestCandidate(...)` now excludes IDs contained in `RunSession.PartyMonsterIds`.
- `CommitManifestChoice()` calls `TryAddPartyMonster(...)` and uses its returned slot index for `SpawnManifestedMonster(...)`.
- `HasManifestedMonster(...)`, `RecordManifestedMonster(...)`, and the selected-Monster helper are removed.
- Active source search found zero old split-party symbol references.
- `git diff --check` passed.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 `MSB3277` warning groups.
- Unity 6000.3.14f1 instance `Pakuri@0c8eeeb5` reported 0 console error entries.

### History

- 2026-07-23: Code Builder moved manifestation eligibility and slot assignment to the unified deployed-party list.

## Task: 2026-07-23 Offering Maximum-Health Bonus Removal

### Task title

Remove the inactive maximum-health modifier from Offering persistence.

### Goals

- Stop copying an unauthored maximum-health value into Offering views and run state.
- Keep reward consumption, selected Choice IDs, learned skills, and runtime skill rebuilding unchanged.

### Constraints

- Role Owner is Code Builder.
- Active Choice eligibility and duplicate prevention remain owned by `RunSession`.
- No active authoring CSV row or column changes are required.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, solution-build verified, and Unity console checked.

### Next Actions

- User verifies normal active/passive/Enhancement/Master Offering selection in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/UI/InGame/InGameUIManager.cs` removes the Offering view field, data copy, and `AddMaxHealthBonus(...)` commit branch.
- `Pakuri/Assets/Scripts/UI/InGame/DebugUI.cs` removes the matching debug commit branch.
- `RecordOfferingChoice(...)`, `CanLearnActive(...)`, `CanLearnPassive(...)`, and both `CanChooseSkillChoice(...)` paths remain active.
- Active source and authoring search found zero maximum-health bonus contract references.
- `git diff --check` passed.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 `MSB3277` warning groups.
- Unity 6000.3.14f1 instance `Pakuri@0c8eeeb5` reported 0 console error entries.

### History

- 2026-07-23: Code Builder removed the dormant Offering maximum-health value while preserving active Choice and skill acquisition state.

## Task: 2026-07-22 Offering Persistent Modifier Cleanup

### Task title

Store Offering combat upgrades through Choice IDs and persist only maximum-health growth separately.

### Goals

- Remove the redundant Offering-to-RunSession combat modifier copy.
- Keep the existing Choice selection and runtime SkillSnapshot calculation path.
- Keep maximum-health growth available when party combat models are rebuilt.

### Constraints

- Role Owner is Code Builder.
- Reward consumption, learned skill recording, and Offering presentation remain unchanged.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and solution-build verified.

### Next Actions

- User verifies normal Enhancement modifiers and maximum-health Enhancement after a party rebuild in Play Mode.

### Evidence

- `InGameUIManager.cs` and `DebugUI.cs` now call `RunSession.AddMaxHealthBonus(...)` only for maximum-health growth.
- `OfferingChoiceView` no longer copies damage, magazine, shot interval, reload, or status-chance fields that had no consumer after Choice ID recording.
- `AccumulateReward` and its repository references are removed; `RecordOfferingChoice(...)` still records the selected Choice ID.
- `dotnet build Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-version warnings.

### History

- 2026-07-22: Code Builder removed the duplicate Offering modifier accumulation path and preserved Choice-driven combat calculations.

## Task: 2026-07-17 PrisonPanel Reinforcement Reward Routing

### Task title

Route a selected prisoner reward through per-party-unit reinforcement or sequential manifestation.

### Goals

- Replace `PrisonerChoicePopUp` as the active prisoner reward entry with `PrisonPanel`.
- Build Offering choices only for the occupied slot selected by the player.
- Return to RewardPanel and preserve reward consumption after Offering or every terminal manifestation result.

### Constraints

- Role Owner is Code Builder.
- Existing Offering choice generation, caps, commit recording, modifiers, and runtime-model refresh remain unchanged except for target filtering.
- Existing manifestation success/candidate logic remains unchanged.
- No CSV row, column, or schema changed.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated; user Play Mode verification pending.

### Next Actions

- User verifies each Reinforcement button shows only that slot monster's choices and consumes only the selected prisoner reward.
- User verifies manifestation failure, skip, and confirm all return to the still-current RewardPanel.

### Evidence

- `OfferingUI.OpenOfferingPanel(string monsterId)` resolves one monster and calls the existing active/passive/enhancement candidate builders only for that state.
- `ResolvePrisonerDisplayName(...)` now uses `GameDataCatalog.GetEnemyById(...)`, covering both Stage 1 and Stage 2 Korean CSV display names.
- `CompletePrisonAction()` is shared by Offering commit and all Menifest terminal actions.
- C# builds and Unity script validation passed with 0 errors; Unity console error query returned 0 entries.

### History

- 2026-07-17: Code Builder implemented the approved per-unit reinforcement and reward-panel return routing.

## Task: 2026-07-22 Skill Acquisition Eligibility Ownership

### Task title

Move active, passive, Enhancement, and Master eligibility checks into RunSession.

### Goals

- Use one acquisition rule source for normal Offering UI and Debug UI.
- Keep selected rewards and Choice IDs as persistent run progression data.
- Remove duplicated UI-side Choice counting and target-skill resolution.

### Constraints

- Role Owner is Code Builder.
- Existing active/passive caps and Enhancement/Master limits remain unchanged.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and solution-build verified.

### Next Actions

- User verifies active/passive acquisition caps, three Enhancements before Master, duplicate prevention, and passive prerequisites in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/GameFlow/RunSession.cs` provides `CanLearnActive`, `CanLearnPassive`, and two `CanChooseSkillChoice` entry points.
- `InGameUIManager.cs` and `DebugUI.cs` use the same RunSession methods; Debug UI duplicate Choice counting helpers were removed.
- Choice counting recognizes both `SkillId` and `TargetSkillId` instead of silently choosing a UI-only path.
- `dotnet build Pakuri/Assembly-CSharp.csproj -v:minimal` completed with 0 errors and the existing 2 assembly-version warnings.

### History

- 2026-07-22: Code Builder centralized skill acquisition eligibility in RunSession and removed duplicate UI logic.

## Task: 2026-07-22 Learned Choice Execution Data Consolidation

### Task title

Keep learned Choice state and resolved skill values in UnitSkills without a separate SkillSnapshot file.

### Goals

- Preserve RunSession acquisition eligibility and selected Choice IDs.
- Build the current skill's resolved values from UnitSkills-owned state.
- Let Executors read selected SkillNode definitions directly.

### Constraints

- Role Owner is Code Builder.
- Reward choice limits and saved IDs remain unchanged.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and solution-build verified.

### Next Actions

- User verifies Enhancement and Master selections still affect the correct learned skill in Play Mode.

### Evidence

- `SkillSnapshot.cs` is deleted and its required resolved-value data is now the `UnitSkillData` declaration inside `UnitSkills.cs`.
- `UnitSkills.CreateExecutionData(...)` combines learned Choice IDs with the selected SkillNode definitions before `SkillExecution` calls an Executor.
- RunSession acquisition methods and stored Choice ID lists were not removed by this consolidation.
- `dotnet build Pakuri/Assembly-CSharp.csproj -v:minimal` completed with 0 errors and the existing 2 assembly-version warnings.

### History

- 2026-07-22: Code Builder removed the separate snapshot file while retaining Choice-driven execution values under UnitSkills.

## Task: 2026-07-23 RunSession Reward State Consolidation

### Task title

Remove duplicate reward-state lookups and unused prisoner bookkeeping from RunSession.

### Goals

- Pass the resolved `RunMonsterState` through acquisition and recording paths.
- Keep reward, Choice, active, and passive IDs on the same party-member state.
- Remove unused prisoner-history and repeated Choice target/default-active helpers.

### Constraints

- Role Owner is Code Builder.
- Existing acquisition caps and prerequisite rules remain unchanged.
- CSV/schema and visible reward flow remain unchanged.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated. Play Mode reward verification remains.

### Next Actions

- User verifies active/passive acquisition, duplicate blocking, three Active Enhancements before one Master, and one Passive Enhancement per skill.
- User verifies reinforcement and manifestation both consume and return through the existing reward UI flow.

### Evidence

- `RunSession.RecordOfferingChoice(...)`, `CanLearnActive(...)`, `CanLearnPassive(...)`, and both `CanChooseSkillChoice(...)` overloads now accept an existing `RunMonsterState`.
- `InGameUIManager.cs` and `DebugUI.cs` resolve that state once and reuse it instead of repeatedly searching by monster ID.
- `PrisonersSeen`, `PrisonerNames`, `ClaimPrisonerReward`, `HasChosenReward`, `HasLearnedActive`, and the RunSession-owned `HasLearnedPassive` helper were removed.
- Choice target resolution is centralized in `ResolveChoiceTargetSkillId(...)`; default-active comparison is performed from one resolved ID.
- Named constants replace the Enhancement/Master limit literals.
- Related removed-symbol search returned 0 matches; `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 `MSB3277` warning groups.
- Unity Editor console error query returned 0 entries after script compilation.

### History

- 2026-07-23: Code Builder consolidated reward acquisition around the party-member state and removed unused prisoner and wrapper APIs.
