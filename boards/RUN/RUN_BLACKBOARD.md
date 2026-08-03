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

## Task: 2026-08-03 Offering Skill Popup Text

### Task title

Update NewRunScene Offering popup text by learned-skill category.

### Goals

- Preserve `RunSession` Offering selection and learned-skill ownership.
- Display `신규 획득!` for new A~E active skills.
- Display `패시브 스킬` for new F~J passive skills.
- Display `마스터 스킬` for master choices.

### Constraints

- Keep the existing `OpenOfferingPanel → BuildOfferingChoices → BindChoiceButton` flow.
- Do not modify `RunSession`, `UnitSkills`, CSV data, or scene hierarchy.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally compiled.

### Next Actions

- User verifies the popup text after opening each Offering choice category in Play Mode.

### Evidence

- `InGameUIManager` assigns category-specific `OfferingKind` values before shared button binding.
- `RunSession` remains the existing source for learnability and master-choice eligibility.
- Runtime and Editor project builds completed with zero errors and the existing two assembly-reference warnings.

### History

- 2026-08-03: Code Builder added category-specific `NewSkillPopUText` updates through the shared Offering UI path.

## Task: 2026-08-03 NewRunScene CSV Image Binding

### Task title

Remove obsolete scene Sprite ownership for PrisonPanel and use runtime catalog Images.

### Goals

- Remove five serialized monster portrait fields from `InGameUIManager` and `NewRunScene`.
- Clear the direct Karin Sprite from `PrisonPanel/Prisonal/Image`.
- Keep the scene hierarchy and UI object paths unchanged.

### Constraints

- Do not change `RunSession`, `UnitSkills`, Offering flow or scene hierarchy.
- Keep Play Mode verification user-owned.
- Preserve unrelated existing scene changes.

### Role Owner

Code Builder

### Status

Implemented and scene-validated.

### Next Actions

- User verifies prisoner selection and monster party image refresh in `NewRunScene` Play Mode.

### Evidence

- `NewRunScene.unity` no longer serializes `arielPrisonPortrait`, `evePrisonPortrait`, `rinPrisonPortrait`, `seinPrisonPortrait` or `vegaPrisonPortrait`.
- The direct `Karin.png` Sprite on `PrisonPanel/Prisonal/Image` was cleared to `fileID: 0`.
- Unity scene validation reported 0 issues, 0 missing scripts and 0 broken prefabs.

### History

- 2026-08-03: Code Builder removed obsolete scene Sprite references; UI now assigns catalog-backed Images at refresh time.

## Task: 2026-08-03 Run UI Inspector Reference Wiring

### Task title

Wire NewRunScene reward, Offering, and Menifest UI through serialized Inspector references.

### Goals

- Preserve the existing Offering and manifest flow while removing runtime scene-name lookup from the extracted UI modules.
- Keep Choice1~3 popup activation/text/color behavior and Prison party slot behavior unchanged.
- Store all current NewRunScene module references on `Canvas/InGameUIManager`.

### Constraints

- Preserve RunSession ownership, scene hierarchy, and user-facing flow.
- Do not change CSV or runtime skill/manifest rules.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and scene-validated.

### Next Actions

- User verifies Offering selection, master/passive popup labels, manifest success/failure, and next-day flow in Play Mode.

### Evidence

- `OfferingUI`, `MenifestUI`, `PrisonPanelUI`, `RewardPanelUI`, and `InGameInfoUI` constructors now consume typed serializable reference groups.
- `NewRunScene` Canvas inspection reports assigned Offering choice buttons/popups/texts, Menifest controls/images, Prison slots, reward controls, and resource labels.
- Scene validation reported 0 issues, 0 missing scripts, and 0 broken prefabs; Unity Console contained 0 error/warning entries after refresh.
- Runtime and Editor project build completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder saved the NewRunScene Inspector reference graph and removed the obsolete scene resolver used by the former UI modules.

## Task: 2026-08-03 NewRunScene Spawn and UI Hierarchy Normalization

### Task title

Use direct Inspector spawn/UI references and organize `NewRunScene` without changing layer-sensitive layout.

### Goals

- Store `UnitSpawnManager` party spawn points as direct serialized scene references.
- Keep scene-owned UI modules as `MonoBehaviour` components with their own Inspector references.
- Keep `InGameUIManager` as the coordinator and remove the shared `InGameUIReferences` object.
- Group spawn, runtime, and UI objects while preserving their previous order, world transforms, and layers.

### Constraints

- Preserve the existing NewRunScene runtime flow and authored UI positions.
- Preserve Default layer 0 for Grid/Runtime objects and UI layer 5 for Canvas/UI objects.
- Do not modify unrelated Combat changes already present in the worktree.
- Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, serialized, compiled, and scene-validated.

### Next Actions

- User verifies player/enemy spawn positions, reward flow, Offering flow, manifest flow, and next-day transition in Play Mode.

### Evidence

- `GameFlow/Spawn/UnitSpawnManager.cs:20-25,448-457` uses serialized `partySpawnPoints` and resolves by party-slot index; the former `GameObject.Find` path is gone.
- `NewRunScene.unity` stores five non-zero `partySpawnPoints` fileIDs plus direct player/enemy/runtime-root references.
- Live hierarchy reports `Grid/SpawnPoint`, `Runtime/Enemies`, `Runtime/Skills`, `Runtime/Monsters`, and the six UI category roots.
- Live hierarchy reports layer 0 for Grid/Runtime categories and layer 5 for UI and its category roots; scene validation reports 0 issues, 0 missing scripts, and 0 broken prefabs.
- `InGameUIReferences.cs` and its `.meta` are absent, and no deleted setup type remains in project files or source search.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and 2 existing assembly-reference warnings; Unity editor state reports no active compilation.

### History

- 2026-08-03: Code Builder replaced spawn/UI lookup with direct Inspector references, converted the scene UI modules to MonoBehaviours, removed the shared reference script, and grouped NewRunScene objects without changing their world transforms or layer assignments.
