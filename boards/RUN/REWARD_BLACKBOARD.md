## Archived History

- Non-July task blocks from `boards\RUN\REWARD_BLACKBOARD.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-08` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/RUN/REWARD_BLACKBOARD.md`.

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
