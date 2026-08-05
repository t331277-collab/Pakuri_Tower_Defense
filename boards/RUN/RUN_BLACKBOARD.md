# RUN_BLACKBOARD

## Current State

There is no active Run, reward, or save/load task block after the 2026-07-28 cleanup.

The previous Run, reward, and save/load boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/RUN/`.

For new Run work, inspect the exact current code and data first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-08-05 Spirit Contract Synergy and Spirit King Runtime

### Task title

Implement Spirit Contract stage effects and the temporary Spirit King ally.

### Goals

- At Stage start, derive Spirit Contract count from the existing artifact ownership and learn A/B/C/D at thresholds 2/4/6/8.
- Spawn one `SummonDefinition` Spirit King through `UnitSpawnManager` at `MiddleSpawnPoint` when count is at least 2.
- Reuse existing Single/Zone skill, graph-node, trigger, targeting, team-target and MonsterActor HP/popup paths.
- Move the Spirit King at speed 0.5 only while enemies exist, stop at `EnemySpawnPoint`, prevent same-Stage respawn after death, and recreate it next Stage when eligible.

### Constraints

- `UnitRole.Summon` and `UnitSide.Player` distinguish the temporary ally from party monsters without adding it to Manifest, Offering or Run party slots.
- `SummonDefinition` remains separate from playable `GameDataCatalog.Monsters`.
- Dimension Rift is a `ZoneSkillDefinition` with `PullToCenter=0.2 unit/tick`, zero damage and existing Zone `OnExpire` follow-up explosion.
- DamageMeter excludes `UnitRole.Summon`; HP and damage popup use the existing `MonsterActor` path.
- Every implementation phase is committed separately; Unity Play Mode remains user-owned.

### Role Owner

Code Builder

### Status

Phase 0 design update in progress. Runtime implementation not yet started.

### Next Actions

- Commit the corrected runtime design and data/runtime task state.
- Implement summon skill graph/trigger loading, then synergy state, spawn, movement and scene binding in separate commits.

### Evidence

- `artifact-synergy-runtime-design.md` records the confirmed 2/4/6/8 thresholds, `SummonDefinition`, `ZoneSkill` Rift, 0.2 pull tick, 0.5 movement and targeting rules.
- `ArtifactSynergyManager.PrepareStage` currently computes `SynergyState` but does not execute `ArtifactSynergyEffectDefinition` or spawn a summon.
- `UnitSpawnManager` already owns player/enemy registration and has the scene `EnemySpawnPoint` binding; `MiddleSpawnPoint` exists in `InGameScene` but is not yet bound to summon spawning.
- `CombatUnitRegistry` groups by `UnitSide`, `SkillTargeting` uses `roster.Players` for ally targeting, and `DamageMeterRuntimeTracker` filters `UnitRole.Monster`, establishing the reuse boundaries.

### History

- 2026-08-05: User confirmed Spirit King as a Monster-like `SummonDefinition` ally with existing MonsterActor UI, targetability, fixed movement and no same-Stage respawn.
- 2026-08-05: Code Builder updated the runtime design with the confirmed Zone pull, target selection, skill thresholds and stage lifecycle.

## Task: 2026-08-05 Artifact Debug Acquisition Flow

### Task title

Use the existing RunSession artifact ownership rules from DebugUI.

### Goals

- Allow a DebugUI-selected artifact to enter the existing ArtifactAchiveDebugUI 1P-5P receiving-unit flow without opening PrisonPanel.
- Keep `RunSession.TryAcquireArtifact` as the authority for run-wide duplicate prevention and per-unit capacity.
- Keep normal RewardPanel artifact completion behavior separate from debug acquisition; debug success closes ArtifactAchiveDebugUI after the grant.

### Constraints

- Do not create a second artifact grant or ownership collection.
- Debug acquisition still requires an occupied party member and rejects a member with three artifacts or an already-owned artifact.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally verified. Play Mode verification remains user-owned.

### Next Actions

- User verifies debug acquisition state mutation and duplicate/capacity rejection in Play Mode.

### Evidence

- `RunSession.CanAcquireArtifact` and `TryAcquireArtifact` remain unchanged and are still called by `PrisonPanelUI.AcquireArtifact`.
- `DebugUI` calls `RunSession.TryAcquireArtifact` from the existing ArtifactAchiveDebugUI 1P-5P buttons; it does not bypass `RunSession` validation.
- `RunSession.CanAcquireArtifact` disables invalid debug receiving-unit buttons for duplicate artifacts, missing party members and three-artifact capacity.
- The `RunMonsterState.Artifacts` ownership and `ArtifactState` three-item cap remain the only persisted grant state.
- The current sibling `ArtifactDebugUI` and `ArtifactAchiveDebugUI` scene paths reach the debug artifact acquisition flow without changing the RunSession ownership contract; live hierarchy inspection found zero `EmodifierBtn` descendants under `ArtifactAchiveDebugUI`.
- DebugUI modifier-button removal is now tolerated by optional binding; artifact acquisition now routes through the existing debug 1P-5P buttons and `RunSession` ownership checks.

### History

- 2026-08-05: Code Builder connected DebugUI artifact choices to the existing RunSession/PrisonPanel acquisition path without duplicating ownership rules.
- 2026-08-05: Code Builder corrected the user-reorganized ArtifactDebug hierarchy path and removed only the ArtifactAchiveDebugUI modifier children, preserving the existing RunSession acquisition rules.
- 2026-08-05: Code Builder updated the debug artifact panel binding after ArtifactDebugUI moved to a DebugPanel sibling and verified the existing acquisition return path still compiles and resolves.
- 2026-08-05: Code Builder removed modifier-button binding as a prerequisite for artifact debug initialization; the existing debug artifact acquisition ownership path remains unchanged.
- 2026-08-05: Code Builder updated the artifact acquisition panel to its new DebugPanel sibling path and preserved the existing RunSession/PrisonPanel ownership flow.
- 2026-08-05: Code Builder moved debug receiving-unit selection from PrisonPanel to ArtifactAchiveDebugUI 1P-5P, assigned party names, added successful-grant logs, and removed the obsolete debug completion context while preserving the normal RewardPanel/PrisonPanel path.
- 2026-08-05: User reported no debug grant after 1P-5P clicks; the cause was listener removal during debug-state reset. Code Builder restored only the button listener registration and left `RunSession.TryAcquireArtifact` and normal PrisonPanel acquisition unchanged.

## Task: 2026-08-05 Boss Artifact Reward Acquisition Design

### Task title

Grant one non-duplicate artifact after an eligible boss combat reward choice.

### Goals

- Prepare an artifact reward on configured Day5 Midboss, Day10 Midboss and Day11 Boss reward rows when `artifact_choice_count` requests choices.
- Draw up to three unowned artifacts uniformly without replacement.
- Keep the selected artifact pending while PrisonPanel selects an occupied 1P-5P receiving unit, then commit through `RunSession`, consume the RewardPanel artifact button, and reopen RewardPanel.
- Keep acquired IDs in the receiving `RunMonsterState.Artifacts` so existing next-Stage artifact composition can use them.

### Constraints

- `ArtifactState.TryAdd` enforces local capacity; run-wide duplicate prevention lives in `RunSession` rather than UI code.
- Ownership remains per monster. ArtifactPanel selects the artifact and PrisonPanel acquisition mode selects the receiving occupied party member.
- Selection must be generated once per pending reward and not rerolled by reopening the panel.
- Artifact rewards remain skippable through the existing RewardPanel `NextBtn`.
- PrisonPanel blocks clicks for a three-artifact recipient, but `RunSession.TryAcquireArtifact` remains the authoritative capacity check and must reject the same request independently.

### Role Owner

Code Builder

### Status

Implemented and corrected for Stage 1/2 Day5, Day10 and Day11. `artifact_choice_count` is now the single reward eligibility source; Play Mode verification remains user-owned.

### Next Actions

- User: verify the artifact reward button and acquisition/skip flow on Day5, Day10 and Day11 in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/GameFlow/Stage/StageManager.cs` calls `PrepareReward()` after combat and currently prepares only gold, dark trace, and prisoners.
- `Pakuri/Assets/Scripts/GameFlow/RunSession.cs` stores one `ArtifactState` per `RunMonsterState` but exposes no artifact acquisition API.
- `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactState.cs` has capacity three and no duplicate check.
- `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactSynergyManager.cs` reads owned artifact IDs at Stage preparation, so a reward committed before next day naturally becomes active next Stage.
- Current runtime design limits implemented individual effects to the ten Spirit Contract artifacts; the other forty remain deferred.
- `PrisonPanelUI.Refresh()` already maps `RunSession.PartyMembers` in 1P-5P order, providing the exact receiving-member index without a second party roster.
- `StageManager.PrepareReward()` now clamps and forwards `currentReward.ArtifactChoiceCount`; normal rows remain zero while configured Midboss/Boss rows provide three.
- `RunSession.TryAcquireArtifact()` validates party membership, run-wide duplicate ownership and the recipient's three-artifact capacity before committing.
- Focused Unity EditMode artifact tests passed 7/7, including duplicate/capacity rejection and reduced/all-full choice counts.
- `Pakuri.sln` builds with zero errors; the two reported warnings are pre-existing assembly-reference warnings.
- Focused EditMode verification passed 2/2 for the six Stage 1/2 Midboss/Boss reward counts and the existing remaining-pool/capacity behavior.

### History

- 2026-08-05: Defined boss gate, equal-probability no-replacement draw, centralized acquisition authority, and next-Stage activation boundary.
- 2026-08-05: User resolved artifact ownership selection: choose an artifact first, then choose its receiving occupied party slot through PrisonPanel acquisition mode.
- 2026-08-05: User specified that a full recipient slot stays visible/enabled and only its click is blocked; RunSession validation remains the final guard.
- 2026-08-05: Code Builder implemented Boss-only pending choices, uniform no-replacement Spirit Contract draws, centralized acquisition, skip behavior and final-boss reward ordering.
- 2026-08-05: Designer investigated the missing-button report and found no missing scene template or clone call; the outstanding discriminator is the tested Stage/Day.
- 2026-08-05: User confirmed Midboss inclusion; Code Builder made reward data the single gate and enabled Stage 1/2 Day5, Day10 and Day11.

## Task: 2026-08-05 Artifact and Synergy Stage Runtime Design

### Task title

Design per-monster artifact ownership, derived synergy state and Stage-start effect composition.

### Goals

- Persist at most three artifact IDs per `RunMonsterState`.
- Rebuild per-unit artifact effects and count-only synergy state once per Stage.
- Separate persistent owned artifact IDs from Stage-active effect IDs distributed by recipient scope.
- Route all combat outcomes through the existing skill trigger/execution pipeline.
- Defer Spirit King spawn and all synergy-effect execution until after the artifact-first runtime is verified.
- Classify every individual artifact effect as passive modifier/trigger application.
- Make two Effect CSVs and Spirit King unit/skill authoring Phase 1, then limit first runtime implementation to the ten Spirit Contract artifacts.

### Constraints

- Code Builder Phase 3 task; Unity Play Mode verification remains user-owned.
- Do not mutate persistent unit stats/defenses cumulatively between Days.
- Do not represent artifact effects as learned skills, hidden passives or enhancement/master Choices.
- `ArtifactState`, `SynergyState` and `ArtifactSynergyManager` consume generated artifact effect Definitions.
- `SkillExecutionRules` consumes active `SkillModifier` effects after learned skill composition; `SkillTrigger` collects active artifact Reactions without converting them to learned passives.
- Do not create artifact-only Node/Trigger CSVs; reuse the existing passive graph-node/trigger authoring path.
- Spirit King uses `UnitSide.Player` and a non-party `UnitRole.Summon`; no `SummonSkillDefinition` or `SummonSkillExecutor`.

### Role Owner

Code Builder.

### Status

Phase 3 is complete for all ten Spirit Contract artifacts. Stage preparation resolves party artifact count, Prism/Codex representative attributes and variable modifier repetition, then distributes existing Node/Trigger effects.

### Next Actions

- Keep synergy-level effect execution and Spirit King runtime deferred to Phase 4+.
- Carry the confirmed Spirit King runtime rules into Phase 4+: `base_move_speed=0.5`, move only while an enemy is registered, move toward `EnemySpawnPoint`, stop on arrival, no same-Stage respawn after death, and spawn a fresh unit next Stage when synergy is at least 2.
- Enforce the confirmed no-duplicate artifact rule when the acquisition system is implemented.
- User verifies Stage preparation, Rift battle duration and Compass follow-up visuals in Play Mode.

### Evidence

- `RunSession.RunMonsterState` and `UnitCombatState` now share one `ArtifactState`; `ArtifactState.TryAdd` enforces the three-item cap.
- `StageManager.RunCurrentDayFlow` calls `ArtifactSynergyManager.PrepareStage` before player restoration; the Manager clears and redistributes Stage-active effect IDs and only logs synergy counts.
- `ArtifactEffectDefinition` now owns generated `SkillNode[]` and `SkillReaction[]` without entering `LearnedPassiveSkillIds`.
- `SkillExecutionRules.BuildExecutionData` applies active artifact modifiers after passive, Enhancement and Master composition; `SkillTrigger` schedules active artifact Reactions through the existing gate/scheduler.
- Existing graph/trigger CSVs now accept artifact Effect ownership; no artifact-only Node/Trigger file was created.
- Current data fully authors all ten Spirit Contract artifacts: eight `SkillModifier` artifacts and two `PassiveTrigger` artifacts; synergy-level effects remain deferred.
- `ArtifactSynergyManager.PrepareStage` uses two passes: count all synergies first, then resolve and distribute dynamic Spirit Contract effects without changing skill Definitions.
- Prism counts learned active A-E attributes across the party, excludes Physical, and breaks ties by A-E then 1P-5P; Codex counts distinct per-member representatives with Physical excluded.
- Spirit Elixir repeats its existing +2% Effect once per Spirit Contract artifact, including itself; Codex repeats its existing +4% Effect once per distinct representative attribute.
- Rift Gem assigns six permanent `CombatStart` resistance-down reactions to its owner so the battlefield-wide effects fire once; Resonance Compass uses five elemental `OnOutgoingDamage` reactions at 8% and `EventAppliedDamage * 0.30`.
- The revised design identifies exact existing Node/Trigger/Executor routes and marks missing reload-complete, densest, temporary allied spawn/movement and conditional-crit paths as required extensions.
- Spirit Bombardment reuses `SingleSkillDefinition` plus `RepeatPerTarget` for three total casts; Dimensional Collapse is split into pull and follow-up explosion SingleAttack Definitions. Current `SingleSkillExecutor` publishes `OnDeploymentCast` and completes without timed `OnExpire`, so that lifecycle is an explicit minimal extension.
- `CombatUnitRegistry` groups by `Identity.Side`, and `SkillTargeting.TargetList` gives Player-side `Ally/AllAllies` skills the full `Players` list; a Player-side Spirit King therefore receives team effects cast after it spawns.
- `SkillExecution.TryExecuteAutomaticSkills` scans all registered entries, while current movement exists only in `EnemyActionController`; Spirit King can reuse automatic skills but needs a small allied movement controller.
- `artifact-synergy-runtime-design.md` now records fixed Spirit King movement speed `0.5`, `EnemySpawnPoint` destination, no-enemy movement pause, no same-Stage respawn, next-Stage re-summon at synergy `2+`, and DamageMeter exclusion with MonsterActor HP/damage popup reuse.
- Current targeting code confirms a Player-side Spirit King is targetable: `CombatUnitRegistry` groups Players by `Identity.Side`, `EnemyCombatDecision` searches all live `Players` except Nexus-only fallback filtering, and `SkillTargeting.IsSkillTargetable` excludes only `UnitRole.Nexus`.
- Full runtime design and acceptance criteria are recorded in `Pakuri/reference/4.run/artifact-synergy-runtime-design.md`.
- Phase 1 now has 27 synergy-effect rows for all 20 detailed non-Tracker levels; Spirit Contract rows reference the authored Spirit King and four granted skill Definitions without adding runtime integration.
- Phase 2-generated Artifact/Synergy/Summon Definitions are now consumed by the Phase 3 state, manager, snapshot and Trigger paths.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.
- Strict CSV parsing found no malformed rows in the four changed CSV files.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings.
- `artifact_effects.csv` now carries typed `repeat_rule` and `selection_rule` fields through SourceModel, Validation, Generation and `ArtifactEffectDefinition`; `ArtifactSynergyManager` has 0 references to the former artifact/synergy ID constants.
- `ArtifactDefinitions.cs` is organized at `Pakuri/Assets/Scripts/Combat/Artifact/Definition/ArtifactDefinitions.cs`; Loading retains the CSV parser, validator and generator, while the Definition namespace/API remains unchanged.
- Focused EditMode verification passed 4/4 for catalog Definition fields, prepared artifact state, snapshot application and dynamic Spirit Contract resolution. Full EditMode remains 19/21 with only the two recorded Trigger baseline failures.
- `ArtifactSynergyManager` no longer owns a duplicate `ActiveSlots` array; its three active-skill scans directly iterate `SkillSlot.A` through `SkillSlot.E`, preserving the required tie order. Focused EditMode verification passed 4/4 and the solution build completed with 0 errors.
- Code Builder moved the three artifact runtime ownership scripts and their Unity `.meta` files to `Pakuri/Assets/Scripts/Combat/Artifact`: `ArtifactState`, `SynergyState` and `ArtifactSynergyManager`; source references to the old `Units/Runtime` and `GameFlow` paths are 0, and the existing `.meta` GUIDs were preserved.
- Unity regenerated `Pakuri/Assembly-CSharp.csproj` with the three `Combat/Artifact` paths; editor compilation is idle with 0 console errors, and the solution build completed with 0 errors.
- Focused EditMode verification passed 3/3: catalog/Definition generation, dynamic Stage party resolution and Rift/Compass existing Trigger outcomes.
- Full `SkillCatalogRuntimeTests` ran 21 tests: 19 passed. The only failures are the same pre-existing Trigger baseline assertions at `SkillCatalogRuntimeTests.cs:665` and `:782`; all artifact tests pass in the full fixture.

### History

- 2026-08-05: Designer traced Run, Stage, spawn, skill rebuild, Trigger and Executor paths and produced the Stage composition design.
- 2026-08-05: User corrected effect identity; Designer removed hidden passive composition and specified generated Artifact effect Definitions managed by ArtifactState/SynergyState/ArtifactSynergyManager, with Spirit Contract first.
- 2026-08-05: Designer made all individual artifacts passive modifier/trigger effects, documented concrete Node paths, and moved two Effect CSVs to Phase 1 before runtime work.
- 2026-08-05: User removed the Summon-skill concept; Designer changed Spirit King to a temporary Player-side monster using existing Unit/Skill paths plus a movement-only extension.
- 2026-08-05: Designer moved Spirit King unit and five skill source rows into Phase 1 and recorded the exact SingleAttack/AreaAttack execution split.
- 2026-08-05: Code Builder authored and validated the four Phase 1 CSVs; RunSession, StageManager and runtime code remain unchanged.
- 2026-08-05: User restricted the next implementation target to the ten Spirit Contract artifacts; Designer made synergy state count/log-only and deferred Spirit King and all synergy-effect execution.
- 2026-08-05: User selected independent ArtifactState snapshot application instead of runtime passive composition; Designer removed artifact-only Node/Trigger CSVs and fixed Phase 3 to reuse existing graph-node/trigger execution paths.
- 2026-08-05: Code Builder implemented the shared ArtifactState, count-only SynergyState, Stage preparation hook, Effect Node/Reaction generation and existing snapshot/Trigger consumers; authored only gameplay values already fixed by source/design evidence.
- 2026-08-05: Code Builder completed all ten Spirit Contract artifact effects with the confirmed resolver, status, stacking and follow-up rules; synergy-level execution and duplicate acquisition enforcement remain deferred.
- 2026-08-05: Code Builder moved `ArtifactState`, `SynergyState` and `ArtifactSynergyManager` plus their `.meta` files under `Combat/Artifact`, then organized `ArtifactDefinitions.cs` under `Combat/Artifact/Definition`; Loading parser, validator and generator files remained in `Loading`.
- 2026-08-05: Code Builder replaced manager artifact/effect ID branching with Definition-owned `repeat_rule` and `selection_rule` metadata, preserving the existing Prism, Elixir and Codex behavior.
- 2026-08-05: Code Builder removed the manager-local `ActiveSlots` array and changed representative-attribute scans to direct `SkillSlot.A` through `SkillSlot.E` iteration.
- 2026-08-05: Designer applied the confirmed Spirit King movement/death/DamageMeter rules to the runtime design and rechecked enemy target selection against the current registry and targeting code.

## Task: 2026-08-05 Artifact Synergy Foundation CSVs

### Task title

Record the initial Run artifact synergy and artifact catalog data.

### Goals

- Represent the Run artifact synergy designs as foundation CSV data.
- Associate every currently authored artifact with its owning synergy.
- Keep the files ready for a later parser/runtime task without implementing that task now.

### Constraints

- Do not connect the catalogs to RunSession, rewards, UI or runtime behavior.
- Do not fabricate Tracker details missing from `artifact-synergy-list.md`.
- Preserve the source document's player-facing wording without Markdown backticks.

### Role Owner

Code Builder.

### Status

Complete. Run artifact foundation data exists without unused ordering metadata; no gameplay system consumes it yet.

### Next Actions

- Complete the Tracker design and artifact list.
- Define Run ownership and parsing in a separate explicit implementation task.

### Evidence

- `artifact_synergies.csv` records six synergies and their common 2/4/6/8 activation counts.
- `artifacts.csv` records the 50 artifacts present in the source's five detailed synergy sections.
- Import validation confirmed unique IDs and valid artifact-to-synergy references.
- Source inspection confirmed Tracker appears only in the six-synergy summary and has no detailed section or artifact table.
- Both catalogs omit `sort_order`; deterministic UI/runtime ordering remains deferred until a consumer defines that requirement.
- The Spirit Contract artifact row now uses `spirit-elixir`, `정령의 비약`, and the revised all-damage/resistance-down description.

### History

- 2026-08-05: Code Builder added the unparsed Run artifact catalogs and recorded the source's Tracker data gap.
- 2026-08-05: Code Builder removed unused `sort_order` fields from both unparsed catalogs and preserved the existing row order and content.
- 2026-08-05: Code Builder renamed the CSVs and preserved their Unity `.meta` GUIDs and file contents.
- 2026-08-05: Designer synchronized the requested `정령의 비약` change into source and the unparsed artifact catalog.

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

## Task: 2026-08-03 Manifest Failure Overlay and Scene Rename

### Task title

Keep `ManifestFailPopup` over `PrisonPanel`, restore PrisonPanel on failure Back, and rename the active menu/run scenes.

### Goals

- Keep `PrisonPanel` active when manifest fails so `ManifestFailPopup` renders above it.
- Make the failure popup Back action reopen `PrisonPanel`.
- Rename the existing run scene to `InGameScene` and the existing main menu scene to `MainMenuScene`.

### Constraints

- Preserve the existing success-manifest flow and popup hierarchy.
- Update all active serialized/code/build-settings scene paths.
- The requested `NewMainScene.unity` did not exist; use the actual `NewMainMenu.unity` as the main-menu source.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, statically verified, and compiled.

### Next Actions

- User verifies manifest failure overlay, failure Back return, success flow, and MainMenu ↔ InGame scene transitions in Play Mode.

### Evidence

- `MenifestUI.IsFailurePopupVisible` exposes the failure popup state; `CompleteAfterFailure` now calls `OpenPrisonPanel()`.
- `PrisonPanelUI` hides the panel only when the manifest result is not the failure popup, leaving the existing `Popup` sibling above `Panels` in the scene hierarchy.
- `NewRunScene` was renamed to `InGameScene`; `NewMainMenu` was renamed to `MainMenuScene`, with their `.meta` GUIDs preserved.
- `MainMenuUIManager`, `StageManager`, both serialized scene fields, and `EditorBuildSettings.asset` now use `InGameScene`/`MainMenuScene` paths.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings; `git diff --check` returned no whitespace errors.

### History

- 2026-08-03: Code Builder changed failure overlay/back behavior, renamed the two existing scenes, synchronized active references, and verified the build.

## Task: 2026-08-03 Manifest Back Binding and Debug Skill Reference Fix

### Task title

Bind `ManifestFailPopup` Back from an active UI owner and restore `DebugUI` StageManager access.

### Goals

- Ensure `ManifestFailPopup` Back closes the fail popup and returns to `RewardPanel`.
- Keep the failed-manifest popup over `PrisonPanel` until Back.
- Allow `DebugPanel` skill buttons to resolve the active `RunSession`.

### Constraints

- Preserve success manifestation flow.
- Keep direct Inspector references.
- Do not bypass existing `RunSession` skill-learning rules.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, statically verified, and compiled.

### Next Actions

- User verifies failure popup Back, `RewardPanel` return, and `DebugPanel` active/passive skill acquisition in Play Mode.

### Evidence

- `MenifestUI` now belongs to the active `Popup` GameObject (`InGameScene.unity` fileID `310674459`), so `Awake()` binds `manifestedFailBackButton`.
- `CompleteAfterFailure()` disables the failure popup and calls `InGameUIManager.CompletePrisonAction()`, which hides `PrisonPanel` and shows `RewardPanel`.
- `DebugUI.stageManager` now points to StageManager fileID `1427799829` instead of `{fileID: 0}`, allowing `ResolveSession()` to return the active session.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings; `git diff --check` passed for the modified code and scene.

### History

- 2026-08-03: Code Builder moved `MenifestUI` to the active `Popup` owner, restored `RewardPanel` return on failure Back, and restored the `DebugUI` StageManager Inspector reference.

## Task: 2026-08-03 Stage End UI Inspector Wiring and Nexus Persistence

### Task title

Move Stage end-flow UI references into UI components and keep one Nexus runtime model across days.

### Goals

- Remove `StageManager.ResolveEndFlowReferences` and bind end buttons through Inspector-owned UI components.
- Remove StageManager's direct CSV loading and health preserve/restore workaround.
- Prevent repeated Nexus registration from replacing the registry's persistent Nexus model.

### Constraints

- Preserve the existing win/defeat panel hierarchy, Button objects, and MainMenu return flow.
- Use the Loading runtime catalog for Stage data.
- Preserve Nexus current health during `ContinueToNextDay` without a second health-copy path.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, serialized, and compiled. Play Mode verification remains user-owned.

### Next Actions

- User verifies win/defeat Back buttons, multi-day Nexus health persistence, Stage transition, and enemy spawning in Play Mode.

### Evidence

- `StageManager` now stores `StageEndPanelUI winPanelUI` and `defeatPanelUI`; `InGameScene.unity` assigns both components and their Inspector Button references.
- Search across `Assets/Scripts` and `InGameScene.unity` returned `NO_OLD_STAGE_END_FLOW_REFERENCES` for the old CSV fields, `StageFlowTable`, `ResolveEndFlowReferences`, and health preserve/restore symbols.
- `UnitSpawnManager.RegisterNexus` returns when an existing registered player model has `IsNexus`, so Day transition does not create a second `nexus` model.
- `InGameCombatManager.ResetCombatState` clears Nexus transient status/shield state but does not reset `Resources.CurrentHealth`; the existing model therefore keeps current health.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder added `StageEndPanelUI`, moved end-button ownership to Inspector references, consumed the Loading Stage catalog, removed the Nexus health workaround, and made Nexus registration idempotent.
