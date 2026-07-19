## Archived History

- Non-July task blocks from `boards\RUN\RUN_BLACKBOARD.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUN_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older run/combat flow history remains in that snapshot and earlier archives.
- This active file now keeps only the current `NewRunScene` authority split and the surviving new-scene flow baseline.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-19 NewRunScene Core Dead Code Removal

### Task title

Remove unused NewRunScene entry, combat façade, Stage projection, and serialized spawn-range state.

### Goals

- Keep the active selected-player, manifested-party, encounter Enemy, combat, and reward flow unchanged.
- Remove zero-reference public projections, overloads, status façades, and write-only spawn results.
- Remove Scene YAML values whose backing fields had no runtime consumer.

### Constraints

- Role Owner is Code Builder.
- `SpawnedPlayerModel`, `UnitSpawnManager`, full Enemy spawn handoff, `ActiveEnemyCount`, active status APIs, and run progression remain intact.
- Existing unrelated `EffectVisualUtility` worktree changes are preserved.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, solution-build verified, and Unity Editor compile-verified.

### Next Actions

- User verifies selected/manifested party restore, Stage 1/2 Enemy spawning, day advance, rewards, and Auto toggle in Play Mode.

### Evidence

- `SceneEntryManager.cs` removes unused Actor/Enemy projections, last-Enemy capture, two short spawn wrappers, and `StartContext.HasPendingRun`; live UI-consumed model/spawn-manager properties remain.
- `InGameCombatManager.cs` removes zero-reference count projections, status/resource façades, registration wrapper, simple damage wrapper, and one-way Auto-enable wrapper while retaining active callers' overloads.
- `SkillExecutionSystem.cs` removes routed/rejected counters whose only consumers were deleted façade properties.
- `NewRunScene.unity` removes only `enemySpawnMinY` and `enemySpawnMaxY`; active encounter rows continue to pass Y ranges into the full spawn path.
- Solution build passed with 0 errors and the existing 2 `MSB3277` warnings; `git diff --check` reported no whitespace errors.
- Unity refresh/compile returned to idle with no C# compiler or `Assets/Scripts` error entries; Play Mode was not started.

### History

- 2026-07-19: Code Builder removed repository-dead NewRunScene core APIs and state without touching active flow ownership.

## Task: 2026-07-17 PrisonPanel Run-State Binding

### Task title

Bind PrisonPanel party progression to the existing RunSession manifestation order.

### Goals

- Keep 1P owned by `SelectedMonsterId` and 2P-5P owned by `ManifestedMonsterIds` append order.
- Permit manifestation only in the first empty party slot.
- Preserve the existing prisoner-consumption, success roll, and random unowned player-monster candidate logic.

### Constraints

- Role Owner is Code Builder.
- No RunSession schema or manifestation probability rule changed.
- Offering and manifestation still consume the active prisoner reward through the existing reward view.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated; user Play Mode verification pending.

### Next Actions

- User verifies party order and spawn slot order after multiple successful manifestations.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` derives PrisonPanel slots from `SelectedMonsterId` plus `ManifestedMonsterIds` and enables only index `partyCount` for Menifested.
- `MenifestUI.ResolveNextManifestCandidate(...)`, `PendingManifestSuccessChance`, `RecordManifestedMonster(...)`, and spawn slot computation remain the existing manifestation authorities.
- Failure-back, success-skip, and success-confirm now all return to RewardPanel through `CompletePrisonAction()`.
- Runtime/editor builds passed with 0 errors; Unity scene validation reported 0 issues.

### History

- 2026-07-17: User confirmed the prisoner remains a material and successful manifestation still yields a random unowned player unit.
- 2026-07-17: Code Builder connected that unchanged run logic to the sequential PrisonPanel slot UI.

## Task: 2026-07-17 Enemy Direct Slot Runtime Handoff

### Task title

Keep the run-time Enemy spawn/AI flow unchanged while sourcing A/B skills directly from `enemies.csv`.

### Goals

- Build each spawned Enemy's two assigned runtime skills without a loadout table.
- Preserve prefab-based Enemy spawning and Enemy AI A/B selection.
- Preserve CombatStart Trigger dispatch.

### Constraints

- Role Owner is Code Builder.
- Scene serialization, prefab bindings, encounter composition, skill values, and AI policy are unchanged.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Code complete and compile-verified.

### Next Actions

- User verifies Enemy prefab spawn and all A/B/CombatStart casts in `NewRunScene`.
- Runtime defects must be fixed in the direct slot/shared execution path; do not restore the loadout table.

### Evidence

- `PakuriCsvRuntimeData.Build.cs` assigns A/B definitions directly from the migrated Enemy row before runtime skill rebuilding.
- `EnemyCombatSystem` still selects the resulting A/B `SkillRuntimeInstance` entries; its selection policy was not changed.
- `EnemySpawnManger` prefab binding flow was not edited by this task.
- Solution build passed with 0 errors and the existing 2 warnings.

### History

- 2026-07-17: Code Builder replaced only the Enemy assignment data source, leaving run spawn and AI ownership intact.

## Task: 2026-07-16 Enemy Phase 9 Runtime Cutover

### Task title

Run Enemy AI and CombatStart skills solely through shared runtime execution after legacy retirement.

### Goals

- Keep Enemy AI as A/B runtime selection only.
- Keep cast/cooldown authority in `SkillRuntimeInstance`.
- Remove runtime fallback to Enemy-only skill execution and scene visual lookup.

### Constraints

- Role Owner is Code Builder.
- Enemy entries remain excluded from Monster automatic skill routing.
- CombatStart Trigger skills remain excluded from normal Enemy AI selection.
- Unity Play Mode cadence and behavior parity remain user-owned.

### Role Owner

Code Builder

### Status

Runtime cutover and compile/Editor validation complete; Play Mode parity remains.

### Next Actions

- User verifies A/B priority, cooldown cadence, no duplicate casts, OpeningCharge, and Intimidation in Play Mode.
- Any defect is fixed in shared runtime execution; legacy fallback is not restored.

### Evidence

- `EnemyCombatSystem` no longer contains legacy cooldown, pending-plan, charge, Plan, or Executor runtime branches.
- `SkillRuntimeInstance.TryBeginCast(...)` remains the cast/cooldown authority.
- `ShouldAutoRouteSkill(...)` still excludes Enemy entries, preventing shared auto-route plus Enemy AI duplicate execution.
- OpeningCharge and Intimidation remain one-shot `CombatStart` Trigger rows.
- Unity Editor CSV validation passed and script reload completed without compiler errors.
- Solution build passed with 0 errors and the existing 2 warnings.

### History

- 2026-07-16: Code Builder completed Phase 9 runtime cutover and final non-Play-Mode deletion verification.

## Task: 2026-07-16 Enemy Runtime Cast Authority Phase 7-8

### Task title

Make Enemy AI a shared-runtime selector while preserving separate Monster auto-routing ownership.

### Goals

- Tick Enemy cooldown through the shared runtime set.
- Route selected Enemy casts through the same execution request used by automatic shared skills.
- Keep CombatStart Trigger casts separate from normal Enemy AI selection.

### Constraints

- Role Owner is Code Builder.
- Enemy entries remain excluded from Monster automatic skill routing.
- Legacy code remains physically present until Phase 9 gates pass.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified; Play Mode cadence and parity pending.

### Next Actions

- Verify support priority, A/B fallback, cooldown cadence, OpeningCharge, and Intimidation in Play Mode.
- Confirm no duplicate cast occurs between `SkillExecutionSystem.Tick(...)` and `EnemyCombatSystem.Tick(...)`.
- Do not start Phase 9 deletion until the run-time parity gate passes.

### Evidence

- `InGameCombatManager.Update()` ticks shared skill runtime before Enemy AI; Enemy auto-route predicate returns false.
- `EnemyCombatSystem` selects runtime slots and calls `CanExecuteSelectedSkill(...)` / `TryExecuteSelectedSkill(...)`.
- `UnitSkillController.TryExecuteSelected(...)` creates the same non-manual request shape as auto routing.
- `SkillExecutionSystem.TryRouteSkill(...)` resolves Choice snapshot, executes typed executor, and commits cast/cooldown once.
- CombatStart-triggered runtimes are filtered out of normal Enemy AI slot selection.

### History

- 2026-07-16: Code Builder completed the Phase 7 ownership switch and Phase 8 Choice-state generalization without enabling current Enemy Choice acquisition.

## Task: 2026-07-16 Enemy Shared Runtime Spawn And Cast Bridge

### Task title

Build Enemy shared skill runtime before roster registration and bridge legacy Enemy AI choices to shared executors.

### Goals

- Ensure CombatStart triggers can execute from assigned Enemy runtime skills at registration.
- Keep one cast source by disabling Enemy entries in Monster automatic skill routing.
- Preserve current Enemy AI movement, skill priority, and cooldown cadence during Phase 4-6.

### Constraints

- Role Owner is Code Builder.
- Phase 7 runtime slot/cooldown ownership is not complete.
- Legacy executor remains failure fallback.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Spawn/cast bridge implemented and compile-verified. Runtime parity remains.

### Next Actions

- Verify OpeningCharge and Intimidation fire once on combat start.
- Verify normal Enemy skills do not double-cast from `SkillExecutionSystem.Tick` plus `EnemyCombatSystem.Tick`.
- After parity, perform Phase 7 runtime slot/cooldown authority transfer.

### Evidence

- `EnemySpawnManger` calls `SkillRuntimeFactory.RebuildAssignedActiveSet(...)` before `RegisterEnemy(...)`.
- Registration dispatch can therefore resolve OpeningCharge/Intimidation runtime instances.
- `ShouldAutoRouteSkill(...)` returns false for `EnemyUnitRuntimeModel`.
- `EnemyCombatSystem` routes its selected skill ID through shared triggered execution and uses legacy code only on rejection.
- `SharedChargeSkillRuntime.Tick(...)` owns active shared charge movement/hit resolution before normal Enemy AI actions.

### History

- 2026-07-16: Code Builder added the Phase 4-6 compatibility bridge without claiming Phase 7 completion.

## Task: 2026-07-15 Next-Day Transient Combat Reset

### Task title

Reset monster skill runtime and field-applied skill effects before advancing to the next day.

### Goals

- Reset every registered monster unit's active-skill cooldown/cast/reload/burst runtime at 1-1 → 1-2 style transitions.
- Remove field-resident runtime skill objects and cancel delayed skill actions from the completed day.
- Clear transient statuses, shields, status visuals, passive trigger counters, and passive trigger cooldowns.
- Preserve run metadata, learned skills, selected choices, and the existing optional next-day health-restore rule.

### Constraints

- Role Owner is Code Builder.
- Reset occurs only after `StageState.RewardReady` and active-session validation succeed.
- `RunSession`, monster state metadata, learned skill lists, and choice records are not cleared.
- Unity Play Mode progression verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that a monster skill on cooldown at day end is ready at the next day start.
- User verifies projectiles, zones, beams, delayed hits, statuses, and shields from the prior day do not remain.
- User verifies learned skills and Offering choices persist across the same transition.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` calls `ResetTransientCombatStateForNextDay()` after preserving Nexus health and before `RunSession.AdvanceDay()`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` cancels skill coroutines, clears enemy combat cache, field skill objects, status visual tracking, passive runtime tracking, and registered unit transient effects.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitRuntimeStateService.cs` now separates transient combat reset from `RestoreForNextDay()`, so cooldown/effect reset does not force health restoration.
- Existing `RestorePlayerHealthForNextDay()` remains controlled by `restorePlayerHealthOnDayAdvance`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; Unity console reported 0 errors after refresh.

### History

- 2026-07-15: User added next-round monster cooldown and skill-effect reset to the Code Builder request.
- 2026-07-15: Code Builder added a pre-advance transient reset boundary while preserving session metadata and health-restore configuration.

## Task: 2026-07-17 Enemy Passive Spawn Assembly

### Task title

Apply CSV-defined Enemy self-passives during Enemy runtime model creation.

### Goals

- Resolve each Enemy's `passive_id` during runtime catalog build.
- Apply the resolved passive exactly once when `UnitFactory.CreateEnemy` creates the runtime model.
- Preserve current outgoing damage, defense, critical, healing, and incoming-damage calculations.

### Constraints

- Role Owner is Code Builder.
- Enemy passives are fixed spawn-time unit modifiers, not learned Monster passives.
- No Enemy run-state acquisition or Choice state is created.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and Unity CSV-validated. Play Mode verification remains.

### Next Actions

- Verify one Enemy for each modifier family in Play Mode after Unity CSV validation.

### Evidence

- `PakuriCsvRuntimeData.Build.cs` resolves `EnemyMigrationRow.PassiveId` into `EnemyPassiveDefinition`.
- `PakuriCsvRuntimeData.EnemyMigrationDataset.cs` turns each five-column `skills_passive.csv` definition into the shared internal `Passive/F/Passive` runtime row.
- `UnitFactory.cs` invokes `EnemyPassiveRuntime.Apply(model, definition.PassiveSkill)` after base stats and defenses are materialized.
- `EnemyPassiveRuntime` supports only `Self` and retains existing multiplier/additive calculations.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors.
- Unity Editor source sync logged `[EnemyPassiveParserValidation] PASS` for the dedicated parser and five-column CSV.

### History

- 2026-07-17: Code Builder moved Enemy passive spawn assembly to the new passive-definition contract.
- 2026-07-17: Dedicated Enemy passive parsing removed three redundant authoring columns without changing spawn assembly.

## Task: 2026-07-18 Unified Stage Enemy Spawn Handoff

### Task title

Keep NewRunScene Stage 1/2 spawning on the encounter-driven generic Enemy path.

### Goals

- Preserve `StageManager` encounter-row spawning for both stages.
- Remove the disabled SceneEntryManager Stage 1-only startup sequence.
- Keep one `SpawnEnemyById(...)` handoff into Enemy model/prefab creation.

### Constraints

- Role Owner is Code Builder.
- Stage flow CSV, encounter order, spawn timing, coordinates, rewards, RunSession, and combat behavior remain unchanged.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated. Play Mode progression verification remains.

### Next Actions

- User verifies Stage 1 and Stage 2 encounter progression and Enemy clear/reward transitions in `NewRunScene`.

### Evidence

- `StageManager.SpawnEncounterRows(...)` continues to pass each `StageEncounterRow.EnemyId` through `SceneEntryManager.SpawnEnemyById(...)`.
- `SceneEntryManager` no longer owns seven Stage 1-specific spawn calls or the Scene-disabled `SpawnInitialEnemySequence()`; its generic spawn route remains.
- `EnemySpawnManger` resolves all 16 authored Enemy IDs from the Scene binding array before creating and registering the runtime Enemy model.
- Static comparison returned 16 authored IDs and 16 Scene bindings with no mismatches.
- Solution build passed with 0 errors and the existing 2 warnings; Unity refresh/compile reached `ready_for_tools=true`.

### History

- 2026-07-18: Code Builder removed the unused Stage 1 bootstrap route and retained encounter-driven Stage 1/2 spawning as the sole active run path.

## Task: 2026-07-19 Combat Coordinator Boundary Phase 1

### Task title

Reduce `InGameCombatManager` responsibility without changing run or Nexus lifecycle ownership.

### Goals

- Move combat damage, selected-player input, Actor display, and Nexus identity checks behind small direct helpers.
- Keep Nexus defeat state, defeat UI, day transition, and run progression in the existing `StageManager` path.
- Preserve the combat manager's public handoff used by run flow.

### Constraints

- Role Owner is Code Builder.
- `StageManager`, scenes, prefabs, CSV sources, and serialized manager fields are not changed by this phase.
- Unity Play Mode progression verification remains user-owned.

### Role Owner

Code Builder

### Status

Phase 1 implemented and compile/editor validated. Play Mode run verification remains.

### Next Actions

- User verifies Nexus damage and defeat UI, reward/day transitions, and next-day combat reset in Play Mode.
- Keep `StageManager` as Nexus/run lifecycle owner unless later code evidence shows a narrower run-specific boundary is needed.

### Evidence

- `InGameCombatManager.ApplyDamage(...)`, healing, shield sync, player input, Actor refresh, and Nexus filtering now delegate to `CombatDamageService`, `PlayerCombatControl`, `CombatUnitView`, and `CombatTargetRules`.
- `StageManager.cs` was not modified by this phase; existing Nexus defeat state/UI and run progression remain in place.
- Public `InGameCombatManager` methods and serialized fields remain available, and `Update()` keeps its existing execution order.
- Solution build passed with 0 errors and the existing 2 assembly-version warnings.
- Unity Editor forced refresh reached idle; script-related error filters returned 0 entries.

### History

- 2026-07-19: Code Builder completed the first combat-manager split while retaining run/Nexus lifecycle ownership in `StageManager`.
