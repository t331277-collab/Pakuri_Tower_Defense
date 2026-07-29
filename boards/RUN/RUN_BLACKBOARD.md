# RUN_BLACKBOARD

## Current State

There is no active Run, reward, or save/load task block after the 2026-07-28 cleanup.

The previous Run, reward, and save/load boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/RUN/`.

For new Run work, inspect the exact current code and data first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-07-29 Unit Skill Ownership Consolidation

### Task title

Keep each run monster's learned skills in one shared `UnitSkills` instance.

### Goals

- Remove duplicate learned-active, learned-passive, and chosen-Choice collections from `RunMonsterState`.
- Keep `RunSession` responsible for Offering transactions, learning limits, party state, and reward-consumption history.
- Share the same `UnitSkills` instance with each player monster runtime model.

### Constraints

- Preserve current learning limits, default skill selection, Offering behavior, day restoration, and skill execution.
- Keep `ChosenRewardIds` in `RunMonsterState`.
- Keep full `SkillExecutionState` rebuilds because learning occurs after combat.
- Do not add or delete production scripts.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies default skill, active/passive learning, Choice application, and next-day party restoration in Play Mode.

### Evidence

- `RunMonsterState` now contains `MonsterId`, one `UnitSkills Skills`, and `ChosenRewardIds`.
- Production skill mutations now occur only in `RunSession`.
- Active C# search returns zero `LearnedActives`, `LearnedPassives`, `ChosenChoiceIds`, `ApplyLearnedSkills`, and `SyncModelStateFromSession` references.
- Runtime and Editor project builds completed with zero errors and the two existing assembly-reference warnings.
- `SkillCatalogRuntimeTests` passed 5/5; `MonsterRuntimeSharesRunSessionSkills` proves the run state and runtime model share one instance.
- Unity script compilation returned ready and the post-compile Console contained zero errors or warnings.

### History

- 2026-07-29: Designer and user agreed that `UnitSkills` owns learned skill and Choice state while `RunSession` owns run rules and reward transactions.
- 2026-07-29: Code Builder removed duplicate run collections and converted spawn, restoration, Offering UI, and debug paths to the shared instance.
