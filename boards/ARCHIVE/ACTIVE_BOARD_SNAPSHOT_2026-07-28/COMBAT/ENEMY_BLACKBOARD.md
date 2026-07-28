# ENEMY_BLACKBOARD

This is the active enemy-domain persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archived History

- Non-July task blocks from `boards\COMBAT\ENEMY_BLACKBOARD.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-19 Enemy Core Dead Code Removal

### Task title

Remove repository-dead Enemy AI state, targeting helpers, counters, and spawn wrappers.

### Goals

- Remove write-only Enemy combat state and unconsumed execution counters.
- Keep the active nearest-target, support selection, movement, A/B cast, and Nexus assault paths unchanged.
- Keep only the full encounter-driven Enemy spawn overload.

### Constraints

- Role Owner is Code Builder.
- Existing Enemy skill values, cooldowns, target policy, prefabs, and Stage encounter behavior remain unchanged.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, solution-build verified, and Unity Editor compile-verified.

### Next Actions

- User verifies representative Enemy movement, A/B casts, support casts, and Nexus contact in Play Mode.

### Evidence

- `EnemyCombatSystem.cs` no longer contains `EnemyCombatState`, its dictionary, write-only target/attempt fields, the unused short `Tick(...)`, or unconsumed attack counters.
- `EnemyTargeting.cs` no longer contains the four zero-reference farthest/random/all/radius helpers; active nearest/Nexus/lowest-health helpers remain.
- `EnemySpawnManger.cs` retains one full `SpawnEnemyById(...)` path and removes two unused wrappers plus unused default Y-range accessors.
- Repository search returned no removed-symbol matches; `dotnet build Pakuri/Pakuri.sln --no-restore /p:UseSharedCompilation=false -v:minimal` passed with 0 errors and the existing 2 `MSB3277` warnings.
- Unity refresh/compile returned to idle with no C# compiler or `Assets/Scripts` error entries; the sole Error entry was the MCP package transport `Cannot access a disposed object`.

### History

- 2026-07-19: User switched to Code Builder and requested removal of the code-proven dead paths.

## Task: 2026-07-17 Enemy Direct A/B Skill Assignment

### Task title

Remove the redundant Enemy loadout table and assemble A/B runtime skills directly from `enemies.csv`.

### Goals

- Make each Enemy row own `skill_slot_a_id` and `skill_slot_b_id`.
- Preserve the existing 16-Enemy A/B assignments exactly.
- Keep base skill implementation and shared runtime execution unchanged.

### Constraints

- Role Owner is Code Builder.
- Enemy acquisition/AI selection remains separate from Monster selection.
- Current Enemy base skills still require no Choice/graph rows.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, structurally validated, and compile-verified.

### Next Actions

- User verifies all Enemy A/B casts and CombatStart skills in Play Mode.
- Add Enemy Choice/graph data only when a real enhancement or Master feature is authored.

### Evidence

- `enemies.csv` contains 16 rows with required `skill_slot_a_id` and `skill_slot_b_id`; both slot reference checks returned 0 missing IDs and all 16 base skills remain referenced.
- `PakuriCsvRuntimeData.EnemyMigrationDataset.cs` parses and validates the two direct slot IDs.
- `PakuriCsvRuntimeData.Build.cs` builds `SkillSlot.A` and `SkillSlot.B` definitions directly without a loadout lookup.
- Searches under active Enemy code/resources found 0 references to `EnemySkillLoadouts`, `SkillLoadoutId`, `skill_loadout_id`, or `enemy_skill_loadouts.csv`.
- `dotnet build Pakuri/Pakuri.sln --no-restore /p:UseSharedCompilation=false -v:minimal` passed with 0 errors and the existing 2 `MSB3277` warnings.

### History

- 2026-07-17: Code Builder merged the exact former 32 loadout assignments into 16 Enemy rows and removed the separate loadout authority.

## Task: 2026-07-17 OpeningCharge Buff Runtime And Contact Damage Fix

### Task title

Move OpeningCharge out of SingleAttack and restore its CombatStart charge/contact behavior through the Buff route.

### Goals

- Classify OpeningCharge and its CombatStart Trigger as `Buff`.
- Give the caster the existing charge movement increase, ramping to the authored `2.5` multiplier.
- Deal physical base damage equal to `100%` of the contacted target's maximum health.
- Preserve the existing 5-second freeze contact effect.

### Constraints

- Role Owner is Code Builder.
- Keep the specialized `ChargeSkillData` and `ChargeSkillExecutor`; do not route the charge through normal attack execution.
- Add no CSV file or column.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and C# compile-verified. User Play Mode verification remains.

### Next Actions

- Verify OpeningCharge starts at CombatStart and visibly accelerates the caster.
- Verify the first contacted hostile receives damage equal to its maximum health before mitigation and the freeze effect.
- Verify the 46 normal-attack hit is no longer the only damage observed during the opening charge.

### Evidence

- The previous Trigger declared `runtime_kind=SingleAttack`, while `InGameSkillDefinitionMapper` mapped `execution_profile=ChargeDamageStatus` to `ChargeSkillData`; `SkillTriggerRuntime.MatchesRuntimeKind` accepted only `SingleAttackData` for that Trigger route, so execution stopped before `ChargeSkillExecutor`.
- `skills_buff.csv` now owns the single OpeningCharge base row with `runtime_kind=Buff`, `move_speed_multiplier=2.5`, `target_max_health_ratio=1`, and `freeze` for 5 seconds.
- `buff_skill_triger.csv` now owns the single OpeningCharge `CombatStart` Trigger with `runtime_kind=Buff`; the old SingleAttack base and Trigger rows were removed.
- `SkillTriggerRuntime` accepts `ChargeSkillData` for the Buff Trigger route, and the mapper uses the Buff row's movement multiplier for the charge maximum.
- `SupportSkillExecutors.cs` keeps damage as `target.MaxHealth * DamageTargetMaxHealthRatio` and now recognizes center contact within `0.05` when collider overlap data is unavailable.
- Runtime and Editor C# builds passed with 0 errors; only the pre-existing 2 `MSB3277` warnings remained. `git diff --check` passed.

### History

- 2026-07-17: Code Builder traced the missing damage to the SingleAttack Trigger/runtime-data type mismatch.
- 2026-07-17: Code Builder moved OpeningCharge authoring to Buff, restored Trigger compatibility, and added a collider-independent close-contact fallback.

## Task: 2026-07-17 Shared Projectile Directional Destroy Boundary

### Task title

Make shared Monster/Enemy projectiles derive their destroy boundary from each launch origin and direction.

### Goals

- Prevent right-to-left Enemy projectiles from being destroyed on their first movement frame.
- Preserve left-to-right projectile behavior through the same side-neutral runtime path.
- Give spread and follow-up projectiles their own direction-correct destroy boundary.

### Constraints

- Role Owner is Code Builder.
- Projectile lifetime, collision, damage, CSV values, runtime visuals, prefabs, and scene serialization remain unchanged.
- Boundary calculation must not branch on Monster/Enemy identity or unit side.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and C# compile-verified. User Play Mode verification remains.

### Next Actions

- Verify AimedShot, ShurikenThrow, SacredSwordWave, and HolySpearThrow travel left from Enemy casters instead of disappearing immediately.
- Verify existing Monster projectiles still travel right and expire by collision, directional boundary, or projectile lifetime.
- Verify spread and follow-up projectiles use their actual launch direction.

### Evidence

- `InGameProjectileActor.ResolveDestroyBoundaryX(...)` now calculates `origin.x + normalizedDirection.x * maxTravelDistance` from launch origin, direction, speed, and lifetime.
- `ProjectileSkillExecutor` calculates a boundary for each `spreadDirection` and separately for delayed follow-up projectiles.
- The later Eve-A `BranchDamage` conversion removed child branch projectiles entirely, so branch damage no longer participates in projectile-boundary handling.
- Searches found no remaining parameterless `ResolveProjectileDestroyBoundaryX()` call in `ProjectileSkillExecutor` or `InGameProjectileActor`.
- `dotnet build Pakuri/Pakuri.sln --no-restore /p:UseSharedCompilation=false -v:minimal` passed with 0 errors and the existing 2 `MSB3277` warnings.

### History

- 2026-07-17: Code Builder replaced the shared projectile's scene-right-boundary dependency with side-neutral per-launch directional boundaries.
- 2026-07-17: The Eve-A branch conversion removed the temporary branch-projectile boundary path while retaining the shared base/spread/follow-up fix.

## Task: 2026-07-16 Enemy Shared Skill Migration Phase 9A-9D

### Task title

Retire the legacy Enemy skill execution/data path after the shared runtime authority switch.

### Goals

- Make merged Enemy/base/Trigger CSV the only active Enemy skill input; the former loadout table was later folded into direct A/B columns.
- Remove Enemy-only Plan/Executor, duplicate scalar state, and scene visual fallback.
- Preserve old Enemy skill prefabs under Legacy without changing their GUIDs.
- Pass the final non-Play-Mode deletion gates.

### Constraints

- Role Owner is Code Builder.
- Enemy skill prefabs are moved, not deleted.
- Runtime hitboxes use authored size only; Enemy offset columns and consumers remain absent.
- `OpeningCharge` remains intentionally visual-less because no prior scene/prefab visual mapping exists.
- Unity Play Mode gameplay parity remains user-owned.

### Role Owner

Code Builder

### Status

Phase 9A-9D implemented. Code/data/scene/prefab deletion gates pass; the 2026-07-17 direct A/B task subsequently removed the intermediate loadout table. User Play Mode parity remains.

### Next Actions

- User verifies all 16 Enemy skills in Stage 1/2 Play Mode scenarios.
- Fix any gameplay issue in shared runtime/base CSV authority; do not restore the legacy Plan/Executor path.

### Evidence

- `EnemyCombatSystem.cs` now contains shared-runtime AI selection only; searches found no `EnemySkillPlanRuntime`, `EnemySkillExecutor`, or `EnemyResolvedSkillData`.
- `EnemyDefinition.cs`, `EnemyUnitRuntimeModel.cs`, and `UnitFactory.cs` no longer carry legacy Basic/Active skill copies or `StageOneEnemySkillKind`.
- `EnemyPassiveModifierKind` plus `EnemyPassiveRuntime` replace the removed string-switch passive application.
- `EffectManager.cs` and `NewRunScene.unity` no longer contain the Enemy enum visual registry or `enemySkillEffects`.
- Unity Editor CSV validation logged `[EnemyPhase9Validation] PASS`.
- `dotnet build Pakuri/Pakuri.sln --no-restore /p:UseSharedCompilation=false` passed with 0 errors and the existing 2 warnings.

### History

- 2026-07-16: Code Builder completed Phase 9A-9D, removed the active legacy Enemy execution path, and passed the final non-gameplay deletion gates.

## Task: 2026-07-16 Enemy Shared Skill Migration Phase 7-8 And Phase 9 Gate

### Task title

Move Enemy AI cast/cooldown ownership to shared runtime, generalize future Choice input, and verify whether Phase 9 deletion may begin.

### Goals

- Let Enemy AI select A/B `SkillRuntimeInstance` without executing effects directly.
- Keep shared `SkillExecutionSystem` and `SkillRuntimeInstance` as the only cast/cooldown owner.
- Allow future Enemy Choice IDs to use the shared snapshot/graph resolver without adding current Choice data.
- Decide Phase 9 from inspected code, CSV, scene-fallback, prefab, and verification evidence.

### Constraints

- Role Owner is Code Builder.
- Current Enemy Choice/graph CSV remains absent.
- Unity Play Mode verification remains user-owned.
- Phase 9 deletion and prefab movement are not performed in this task.
- Enemy skill prefabs must not be deleted.

### Role Owner

Code Builder

### Status

Phase 7-8 code implemented and C# compile-verified. Phase 9 deletion gate failed; cleanup must not begin yet.

### Next Actions

- Run Unity Editor CSV validation in the already-open project.
- Play Mode parity-check all 16 Enemy skills through Stage 1/2 representatives.
- Remove legacy loader/model/EffectManager dependencies only after all Phase 9 gates pass.
- After scene and serialized prefab references reach 0, move `Pakuri/Assets/Prefab/Enemy/Skill` to `Pakuri/Assets/Legacy/Enemy/Skill` with all `.meta` files preserved.

### Evidence

- `EnemyCombatSystem.TickEnemy(...)` no longer calls legacy cooldown tick, pending Plan actions, legacy active charge, or Plan/Executor fallback.
- Enemy AI selects slot B then A and calls `InGameCombatManager.TryExecuteSelectedSkill(...)`.
- `SkillExecutionSystem.TryRouteSkill(...)` calls `SkillRuntimeInstance.TryBeginCast(...)`; Enemy AI no longer writes its own cooldown.
- Enemy entries remain excluded by `InGameCombatManager.ShouldAutoRouteSkill(...)`.
- `SkillChoiceResolver` now reads `BaseUnitRuntimeModel.State.ChosenChoiceIds`.
- `StageOneEnemySkillKind` remains in 5 C# files; legacy Enemy CSV filenames remain direct loader/validation dependencies; `EffectManager` still owns Enemy enum mappings.
- `Pakuri/Assets/Prefab/Enemy/Skill` contains 15 prefabs, 15 prefab `.meta` files, and folder metadata; `Pakuri/Assets/Legacy/Enemy/Skill` does not yet exist.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` passed with 0 errors and the existing 2 MSB3277 warnings.

### History

- 2026-07-16: Code Builder completed Phase 7-8, rejected Phase 9 deletion on unmet gates, and fixed the prefab policy to Legacy move instead of deletion.

## Task: 2026-07-16 Enemy Shared Skill Migration Phase 4-6

### Task title

Route all 16 current Enemy base skills through shared typed runtime executors while retaining legacy AI selection and fallback.

### Goals

- Execute Phase 4 damage/projectile skills through shared SingleAttack/Projectile executors.
- Execute Phase 5 support skills through shared Buff/Shield/Heal executors and side-aware target scope.
- Execute Phase 6 chain, compound debuff, and CombatStart charge through shared typed runtime.
- Prevent Monster auto routing from duplicating Enemy AI casts.

### Constraints

- Role Owner is Code Builder.
- Enemy AI selection and Basic/Special cooldown ownership remain legacy until Phase 7.
- Legacy plan/executor remains fallback; no deletion in this phase.
- Scene Enemy prefab mapping remains fallback; no prefab or scene removal.
- Enemy Choice/master graph remains future scope.

### Role Owner

Code Builder

### Status

Phase 4-6 code implemented and C# compile-verified. Unity CSV menu and Play Mode parity remain.

### Next Actions

- Run `Pakuri/Validate CSV Source Data` from the already-open Unity Editor.
- Play Mode parity-check Stage 1/2 representatives, especially HolySpearThrow farthest target, support radius, ChainLightning delay, FrostPressure status, and OpeningCharge.
- Begin Phase 7 only after parity: move Enemy AI to runtime slot/cooldown authority and remove compatibility fallback.

### Evidence

- `EnemySpawnManger.TryCreateEnemyModel(...)` builds assigned Enemy runtime skills before registration.
- `EnemyCombatSystem.ExecuteSkillWithPlanFallback(...)` routes selected skill IDs through `InGameCombatManager.TryExecuteTriggeredSkill(...)` before legacy fallback.
- `InGameCombatManager.ShouldAutoRouteSkill(...)` rejects Enemy entries, preventing duplicate automatic casts.
- `InGameSkillDefinitionMapper` maps DamageArea, Heal, delayed chain, charge, combined AP/SP, explicit projectile lifetime, Farthest/Random target selection, and configured support scopes.
- `SupportSkillExecutors.cs` contains shared Heal, ChainAttack, and Charge typed executors plus shared charge ticking.
- `StatusEffectRuntime` now maps Enemy-authored spell/damage modifiers and permanent status flags; builder emits parser-valid `passive-buff`.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` passed with 0 errors; existing MSB3277 warnings remained.
- Static width check found 0 header/type/data mismatches across 7 Enemy base CSV files.
- Unity batchmode validation was blocked only by the open-project duplicate-instance guard.

### History

- 2026-07-16: Code Builder implemented Phase 4-6 with a shared-runtime-first compatibility adapter and retained Phase 7/legacy cleanup boundaries.

## Task: 2026-07-16 Enemy Shared Skill Migration Phase 0-3

### Task title

Implement the Phase 0 baseline, shared runtime generalization, merged Enemy/loadout CSV input, and 16 typed Enemy base rows without switching Enemy execution authority.

### Goals

- Preserve current Enemy AI and legacy executor behavior during the parity period.
- Allow shared skill runtime state, factory, Trigger, and Passive paths to accept `BaseUnitRuntimeModel`.
- Add one-time shared `CombatStart` dispatch without removing current Enemy CombatStart behavior.
- Load and validate new Enemy migration CSV in parallel with legacy data.

### Constraints

- Role Owner is Code Builder.
- Phase 4 executor transfer, Enemy AI slot routing, and legacy removal are excluded.
- New Enemy base CSV has no graph/Choice rows and no runtime hitbox offset columns.
- Current scene Enemy prefab mapping remains fallback authority.

### Role Owner

Code Builder

### Status

Phase 0-3 implementation complete and C# compile-verified. Unity Play Mode parity remains.

### Next Actions

- Run Unity CSV source validation and representative Stage 1/2 Play Mode scenarios.
- Begin Phase 4 only after the new parallel CSV validation is clean.
- Keep legacy Enemy execution enabled until each migrated skill passes behavior parity.

### Evidence

- `Pakuri/reference/Report/2026-07-16-enemy-shared-skill-phase-0-baseline.md` records 16 skills, 21 params, 15 prefab snapshots, and collider authority.
- `BaseUnitRuntimeModel.State`, generalized `SkillRuntimeFactory`, side-aware Trigger/Passive iteration, `SkillTriggerEvent.CombatStart`, and one-time registration dispatch are implemented.
- `enemies.csv` has 16 rows; `enemy_skill_loadouts.csv` has 32 rows; typed base files have 16 active rows; Trigger files have 2 CombatStart rows.
- `PakuriCsvRuntimeData.EnemyMigrationDataset.cs` validates full legacy Enemy fields, A/B loadout parity, 16 node action profiles, 21 node params, stage IDs, and exactly one CombatStart Trigger for OpeningCharge and Intimidation.
- Runtime and Editor `dotnet build` passed with 0 errors; existing MSB3277 warnings remained.
- Independent PowerShell parity checks returned `enemy_and_loadout_parity=PASS` and `legacy_to_base_parity=PASS`.

### History

- 2026-07-16: Code Builder completed migration plan Phase 0-3 while retaining the legacy Enemy cast owner and executor.

## Task: 2026-07-16 Enemy Shared Skill Runtime And CSV Migration Design

### Task title

Design the migration from the Enemy-only skill plan/executor path to shared Monster-style typed base skill executors, with optional future Choice/graph enhancement.

### Goals

- Separate Enemy unit description and skill loadout from skill execution values.
- Route the current 16 Enemy base skills through shared SkillDefinition, SkillRuntimeInstance, SkillExecutionPlan, and typed executors without requiring Choice/graph rows.
- Keep Enemy AI as a separate brain that selects a skill without owning its effect implementation.

### Constraints

- Role Owner is Designer.
- This task creates a design/handoff document only; no code, CSV, prefab, or scene behavior is changed.
- Proposed file names and APIs in the report do not exist until a Code Builder implements them.
- Current Monster behavior must remain stable while Enemy skills migrate incrementally behind legacy fallback.
- Current Enemy skills have no inspected enhancement or master rows, so the initial migration must not create mandatory Choice/graph inputs.
- Ally conversion is a future feature and is excluded from this migration's APIs, steps, verification, and acceptance criteria.
- Runtime visual ownership stays directly on each typed base skill row; no visual override layer is used.
- CombatStart timing is stored in kind-specific Trigger CSV while the triggered skill's effect remains in its typed base row.

### Role Owner

Designer

### Status

Design handoff created; implementation has not started.

### Next Actions

- Code Builder generalizes the current Monster-specific SkillRuntimeFactory to BaseUnitRuntimeModel-compatible input.
- Add shared Hostile/Friendly target scopes, shared Heal execution, and Enemy AI-to-shared-runtime cast handoff.
- Add `SkillTriggerEvent.CombatStart` and one-shot combat-start dispatch because the current shared trigger enum has no CombatStart value.
- Migrate simple Damage/Area/Projectile Enemy base rows into shared typed executors first, then support and complex execution profiles.
- Move Intimidation and OpeningCharge CombatStart conditions into buff and single-attack Trigger CSV paths.
- Generalize Choice state only when Enemy enhancement/master acquisition is actually introduced.
- Retire Enemy-only Plan/Executor and StageOneEnemySkillKind only after all 16 skills pass parity checks.

### Evidence

- Design report: `Pakuri/reference/Report/2026-07-16-enemy-shared-skill-runtime-csv-migration-plan.md`.
- Current Enemy data has 16 skills, 16 nodes, and 21 node params across `EnemySkillData.csv`, `EnemySkillNodes.csv`, and `EnemySkillNodeParams.csv`.
- `EnemyDefinition.cs`, `PakuriCsvRuntimeData.EnemyDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `EnemyUnitRuntimeModel.cs`, and `UnitFactory.cs` copy Basic/Active skill fields across multiple layers.
- `EnemyCombatSystem.cs` owns both Enemy AI selection and Enemy-only skill execution; `SkillExecutionSystem` also ticks roster entries, so a direct dual connection would risk duplicate casts.
- `Slash` is assigned 11 times. Under the revised contract one skill ID has one base-row visual; different visuals require distinct skill IDs/base rows while still sharing AreaAttack executor code.
- Inspected Monster base CSV headers directly carry base damage, coefficients, cooldown, radius, target selection, status, and runtime visual/hitbox data; graph rows are not required merely to represent those base values.
- `Intimidation` and `OpeningCharge` both have current `CombatStart` rows in `EnemySkillNodes.csv`; Intimidation stores outgoing multiplier `0.7` in both current skill/param data.
- `EnemyCombatSystem.ExecuteOutgoingDamageMultiplierStatus(...)` converts Intimidation multiplier `0.7` to `DamageBonusRate=-0.3` through `multiplier - 1f`.
- Current `SkillTriggerActionKind` contains `TriggeredSkill`, while current `SkillTriggerEvent` contains no `CombatStart`, proving the required shared trigger extension boundary.

### History

- 2026-07-16: Inspected the current Enemy CSV, Enemy runtime copies, shared Monster skill path, targeting, and prefab/runtime visual paths; created the migration report.
- 2026-07-16: Revised the handoff so current Enemy skills migrate as typed base skills; Choice/graph is optional and deferred until an actual enhancement or master feature exists.
- 2026-07-16: Removed ally-conversion implementation content because it is future scope, and aligned Enemy runtime hitboxes with the Vega zero-offset/size-only migration boundary.
- 2026-07-16: Removed visual override design, assigned runtime visuals directly to base rows, and separated CombatStart Trigger timing from base effect data.

## Task: 2026-07-05 Monster Choice Base Runtime Removal

### Task title

Record that monster skill choice runtime no longer accepts the removed base CSV tables.

### Goals

- Supersede older Phase D notes that mentioned `monster_skill_choice_base.csv` as an active choice metadata source.
- Keep current monster choice runtime authority on `monster_skill_choices.csv`.
- Keep historical notes intact while making the current state explicit.

### Constraints

- Role Owner is Code Builder.
- This board update records the data/runtime cleanup only; enemy skill behavior was not changed.
- MSW-MCP is not used; Unity-MCP remains the only MCP path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Treat older board/reference mentions of `monster_skill_choice_base.csv` and `SkillChoiceBaseRows` as historical unless a future task explicitly reintroduces a base table.

### Evidence

- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_choice_base.csv` and its `.meta` file were deleted.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_base.csv` and its `.meta` file were deleted.
- `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.Loader.cs`, `PakuriCsvRuntimeData.SourceModel.cs`, `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`, and `PakuriCsvRuntimeData.Validation.cs` no longer use `SkillChoiceBaseRows` or `SkillBaseRows`.
- `Select-String` under `Pakuri/Assets/Scripts2/InGame/Data/Runtime` for removed base-table symbols and filenames returned no matches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.

### History

- 2026-07-05: User asked Code Builder to delete `monster_skill_base.csv` and `monster_skill_choice_base.csv`, and to unify choice references onto `monster_skill_choices.csv`.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/ENEMY_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/enemy history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current Stage 1 enemy runtime authority and verification baseline.

## Task: 2026-07-17 Enemy Passive Base CSV Authority

### Task title

Move fixed Enemy self-passives from `enemies.csv` scalar fields into passive base skill definitions.

### Goals

- Keep `enemies.csv` responsible only for assigning one `passive_id` per Enemy.
- Keep passive display name, self target, modifier kind, and value in `skills_passive.csv`.
- Preserve all 16 existing Enemy passive effects and values at spawn time.

### Constraints

- Role Owner is Code Builder.
- Enemy passive acquisition, Choice, graph, enhancement, and master paths are not added.
- Current runtime supports only `EnemyPassiveTarget.Self`.
- Legacy Enemy assets are not edited.

### Role Owner

Code Builder

### Status

Code complete. Solution compile, static CSV checks, and Unity Editor CSV validation passed. Play Mode verification remains.

### Next Actions

- Verify representative damage, defense, critical, healing, and incoming-damage passives in Play Mode.

### Evidence

- `Pakuri/Assets/CSVdata/authoring/enemy/enemies.csv` has 16 Enemy rows and now stores only `passive_id` for passive assignment.
- `Pakuri/Assets/CSVdata/authoring/enemy/skills/base/passive/skills_passive.csv` has 16 five-column definitions with `apply_target`, `modifier_kind`, and `modifier_value`.
- `PakuriCsvRuntimeData.EnemyMigrationDataset.cs` detects `skills_passive.csv` and internally constructs the shared `Passive/F/Passive` classification without authoring those three columns.
- Static CSV reference verification returned Enemy 16, passive 16, missing references 0, unused passives 0.
- CSV shape verification returned 5 columns, 16 rows, 0 width errors, and `enemy-sword-mastery.modifier_value=0.10`.
- `EnemyDefinition.cs` now builds `EnemyPassiveDefinition`; `UnitFactory.cs` passes it to `EnemyPassiveRuntime.Apply`.
- `EnemyUnitRuntimeModel.cs` retains the existing modifier math for the 10 currently used modifier kinds.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-version warnings.
- Unity Editor logged `[EnemyPassiveParserValidation] PASS` after syncing the five-column source; the temporary hook and `.meta` were removed.

### History

- 2026-07-17: Code Builder separated Enemy passive assignment from passive definition while preserving current spawn-time behavior.
- 2026-07-17: Code Builder simplified Enemy passive authoring to five columns and added the dedicated parser path; runtime behavior and assignment model remain unchanged.

## Task: 2026-07-18 Unified Enemy Prefab Bindings

### Task title

Unify Stage 1 and Stage 2 Enemy prefab resolution through one ID binding array.

### Goals

- Remove the Stage 1-only Enemy ID, prefab, spawn-method, and fallback branches.
- Resolve every authored Stage 1/2 Enemy through the existing `enemyPrefabBindings` path.
- Preserve encounter-driven spawning through `StageManager -> SceneEntryManager.SpawnEnemyById(...) -> EnemySpawnManger.SpawnEnemyById(...)`.

### Constraints

- Role Owner is Code Builder.
- Enemy definitions, encounter composition, stats, skills, AI, spawn coordinates, and prefab GUIDs remain unchanged.
- Unity Play Mode gameplay verification remains user-owned.
- Existing unrelated worktree changes remain untouched.

### Role Owner

Code Builder

### Status

Implemented, structurally validated, compile-verified, and Unity Editor refresh-verified. Play Mode verification remains.

### Next Actions

- User verifies representative Stage 1 and Stage 2 encounters in `NewRunScene`, including `stage1-archer`.

### Evidence

- `EnemySpawnManger.cs` now resolves Enemy prefabs only from `enemyPrefabBindings`; seven Stage 1 constants, seven prefab fields, seven ID fields, seven specialized spawn methods, and seven fallback branches were removed.
- `SceneEntryManager.cs` retains the generic `SpawnEnemyById(...)` route and removes the disabled Stage 1-only startup sequence plus its cached objects and delegate.
- `NewRunScene.unity` now contains all 16 `enemies.csv` IDs in one binding array: Stage 1 count 8 and Stage 2 count 8.
- Static comparison returned `csv_count=16`, `binding_count=16`, with no missing, extra, or duplicate IDs.
- The previously unbound `stage1-archer` now points to existing `Stage1_Achor.prefab` GUID `bffcd0db2ede5a34a9297596966f6697`; the prefab contains `EnemyUnitActor`.
- Legacy Stage 1 spawn declaration/reference search returned `legacy_match_count=0`.
- `dotnet build Pakuri/Pakuri.sln --no-restore /p:UseSharedCompilation=false -v:minimal` passed with 0 errors and the existing 2 `MSB3277` warnings.
- Unity refresh/compile completed with `ready_for_tools=true`; the only error entry was the existing MCP package client-handler `Cannot access a disposed object`, not project code.

### History

- 2026-07-18: User identified the Stage 1-only prefab/spawn declarations as inconsistent with Stage 2 binding ownership and explicitly requested Code Builder unification.
- 2026-07-18: Code Builder migrated all 16 Enemy prefab bindings to one array and removed the disabled Stage 1-only spawn path.

## Task: 2026-07-19 InGameCombatManager Responsibility Split Phase 1

### Task title

Split damage, player input, Actor display, and Nexus checks from `InGameCombatManager`.

### Goals

- Keep `InGameCombatManager` as the combat coordinator while moving independent rules into direct, narrowly named classes.
- Preserve damage order, shield handling, manual/auto skill input, Actor refresh, and Nexus exclusion behavior.
- Mark every touched script with a `Code Builder` comment.

### Constraints

- Role Owner is Code Builder.
- Public manager APIs, serialized fields, and `Update()` execution order remain unchanged.
- Existing status, skill trigger, stage flow, UI, CSV, prefab, and scene behavior remain unchanged.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Phase 1 implemented, solution compile-verified, and Unity Editor refresh-verified. Play Mode verification remains.

### Next Actions

- User verifies shield-to-health damage order, healing, status shield refresh, manual burst aiming, auto skill routing, Actor defeat display, and Nexus defeat flow in Play Mode.
- Treat further status/trigger extraction as a separate phase only after this behavior check.

### Evidence

- `InGameCombatManager.cs` is 1,331 lines after extraction; it delegates damage to `CombatDamageService`, player input to `PlayerCombatControl`, Actor display to `CombatUnitView`, and Nexus identity to `CombatTargetRules`.
- `CombatDamageService.cs`, `PlayerCombatControl.cs`, `CombatUnitView.cs`, and `CombatTargetRules.cs` exist with Unity `.meta` files.
- Removed-symbol search in `InGameCombatManager.cs` returned no `UnitResourceMutationService`, direct Input System/EventSystem usage, direct mouse access, or moved damage-resolution implementation.
- `dotnet build Pakuri/Pakuri.sln --no-restore /p:UseSharedCompilation=false -v:minimal` passed with 0 errors and the existing 2 `MSB3277` warnings.
- Unity forced refresh completed at idle with Play Mode off; console filters for `error CS` and `Assets/Scripts` returned 0 entries.
- `git diff --check` passed.

### History

- 2026-07-19: User requested Code Builder responsibility separation with direct naming modeled after `Animation_Controller.cs` and explicit comments on touched scripts.
- 2026-07-19: Code Builder completed Phase 1 without moving Nexus defeat state/UI from `StageManager`.

## Task: 2026-07-19 Combat Responsibility Merge

### Task title

Merge over-separated damage, Nexus, and Actor display helpers into their existing owners.

### Goals

- Return damage, healing, and shield mutation to `InGameCombatManager`.
- Make `BaseUnitRuntimeModel` own the shared Nexus-role check.
- Make `UnitRosterEntry` connect its registered Actor to damage, refresh, and defeat behavior.
- Delete the three temporary helper scripts and their Unity metadata.

### Constraints

- Role Owner is Code Builder.
- Public combat APIs, damage order, Trigger order, Actor behavior, and serialized fields remain unchanged.
- `PlayerCombatControl` remains separated.
- Root `BLACKBOARD.md` is not read or updated per user instruction.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, solution-build verified, and Unity Editor compile-verified. Play Mode verification remains.

### Next Actions

- User verifies shield-to-health damage, healing, Actor hit/death display, Nexus defeat notification, and next-day Actor refresh in Play Mode.

### Evidence

- `CombatDamageService.cs`, `CombatTargetRules.cs`, `CombatUnitView.cs`, and their `.meta` files no longer exist.
- `InGameCombatManager` now owns resource damage, healing, shield synchronization, status/passive damage modifier preparation, Trigger dispatch, and death coordination.
- `BaseUnitRuntimeModel.IsNexus` is the common role check; `EnemyTargeting.IsNexus(...)` now delegates to it.
- `UnitRosterEntry` owns `ShowDamage(...)`, `RefreshActor()`, and `ShowDefeated()` for its registered Actor.
- Repository search returned no references to the three deleted helper types.
- Unity forced refresh removed the deleted source paths from the generated C# project and returned to idle with 0 `error CS` and 0 `Assets/Scripts` error entries.
- `dotnet build Pakuri/Pakuri.sln --no-restore /p:UseSharedCompilation=false -v:minimal` passed with 0 errors and the existing 2 `MSB3277` warnings.

### History

- 2026-07-19: User rejected the three helper boundaries as unnecessary and explicitly requested Code Builder reintegration.
- 2026-07-19: Code Builder merged each responsibility into the existing manager, model, and roster owners without changing public combat behavior.
