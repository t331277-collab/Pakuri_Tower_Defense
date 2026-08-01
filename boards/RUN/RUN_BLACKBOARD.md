# RUN_BLACKBOARD

## Current State

There is no active Run, reward, or save/load task block after the 2026-07-28 cleanup.

The previous Run, reward, and save/load boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/RUN/`.

For new Run work, inspect the exact current code and data first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-08-01 NewRunScene Monster Prefab Serialization Migration

### Task title

Move `NewRunScene` monster prefab references into `MonsterPrefabBinding[]`.

### Goals

- Replace the five `UnitSpawnManager` scene fields with one serialized binding array.
- Preserve the five existing prefab GUID references in `NewRunScene`.
- Keep selected-monster and manifested-party spawn call sites unchanged.

### Constraints

- Preserve scene references and runtime spawn behavior.
- Do not change RunSession or learned-skill ownership.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and verified.

### Next Actions

- User verifies selected and manifested monster spawning in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:23616` now contains five `monsterPrefabBindings` entries.
- Unity loaded `NewRunScene` successfully and scene validation reported 0 issues, 0 missing scripts, and 0 broken prefabs.
- Unity component inspection reported the five expected monster IDs and prefab asset paths.
- Core and Editor builds completed with 0 errors; only the existing two assembly-reference warnings remain.

### History

- 2026-08-01: Code Builder migrated the existing NewRunScene monster prefab references from individual fields to serialized binding entries without changing spawn callers.

## Task: 2026-08-01 Player Party Restore Consolidation

### Task title

Consolidate selected-player and additional-player session restoration into one traversal.

### Goals

- Keep one `RestorePlayerPartyFromSession` entry point for every party slot.
- Preserve registry checks and revival of existing runtime monsters.
- Preserve selected-player creation for slot 0 and manifested-monster creation for later slots.

### Constraints

- Keep the public `RestorePlayerPartyFromSession` API and existing creation methods.
- Preserve `RunSession` ownership and next-day restoration behavior.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and verified.

### Next Actions

- User verifies next-day party revival and restoration in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs:144` now loops from slot 0 through `PartyMembers` in one method.
- Repository search found zero `RestoreSelectedPlayerFromSession` and `RestoreAdditionalPlayersFromSession` references.
- Core and Editor builds completed with 0 errors; only the existing two assembly-reference warnings remain.
- Unity script validation reported 0 warnings and 0 errors; Unity Console reported 0 error/warning entries.

### History

- 2026-08-01: Code Builder merged the two private restoration traversals while retaining their slot-specific creation branches.

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
