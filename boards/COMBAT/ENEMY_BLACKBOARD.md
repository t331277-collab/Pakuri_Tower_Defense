# ENEMY_BLACKBOARD

This is the active enemy-domain persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archive Note

- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

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

- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_choice_base.csv` and its `.meta` file were deleted.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_base.csv` and its `.meta` file were deleted.
- `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.Loader.cs`, `PakuriCsvRuntimeData.SourceModel.cs`, `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`, and `PakuriCsvRuntimeData.Validation.cs` no longer use `SkillChoiceBaseRows` or `SkillBaseRows`.
- `Select-String` under `Pakuri/Assets/Scripts2/InGame/Data/Runtime` for removed base-table symbols and filenames returned no matches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.

### History

- 2026-07-05: User asked Code Builder to delete `monster_skill_base.csv` and `monster_skill_choice_base.csv`, and to unify choice references onto `monster_skill_choices.csv`.

## Task: 2026-06-19 Enemy Skill Node Runtime Implementation 1-7

### Task title

Implement enemy Stage1/Stage2 skills through runtime skill node plans.

### Goals

- Compile enemy skill body rows plus node rows into runtime skill plans.
- Preserve old direct executor fallback until step 8 is approved.
- Implement Stage2 behavior handlers requested for combat-start, chain, heal, charge, and outgoing-damage reduction skills.

### Constraints

- Role Owner is Code Builder.
- User explicitly postponed step 8 until after observing implemented skills.
- Arsen outgoing damage reduction uses existing status modifier runtime because inspected player runtime models do not expose a direct outgoing-damage multiplier field.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and non-Play-Mode validated. 2026-06-19 follow-up Code Builder pass fixed Reviewer findings and wired Stage2 skill prefabs where matching prefabs exist. 2026-06-19 later Code Builder pass fixed Stage2 Fire/Dark collider-prefab runtime use and changed Ethan HolySpearThrow projectile speed to current move speed x2. 2026-06-19 final Code Builder pass separated enemy skill hit detection from visual lifetime and excluded Nexus from enemy skill hitbox damage. 2026-06-19 later projectile follow-up set Ethan spear speed to Karin sword-wave speed and enemy projectile lifetime to 10 seconds. 2026-06-19 final Drake follow-up replaced instant charge damage/teleport behavior with an active collider-contact charge that ramps from normal move speed to x2.5 over 3 seconds, clears on hit, and applies 5 second Freeze to the hit non-Nexus player unit. 2026-06-19 Arsen/FireDragon follow-up made Arsen intimidation explicitly stage-persistent and made FireDragonSlash collider damage hit every contacted non-Nexus enemy target instead of stopping after the first hit.

### Next Actions

- User verifies Play Mode behavior for Stage2 enemies, especially Lightning Scout delayed second hit, Drake spawn-time charge, and Arsen outgoing damage x0.7.
- After behavior confirmation, step 8 can remove old direct execution paths if still desired.

### Evidence

- `EnemyDefinition.cs`, `EnemyUnitRuntimeModel.cs`, and `UnitFactory.cs` now carry attack/spell coefficients plus basic/active `EnemySkillPlanDefinition` objects.
- `PakuriCsvRuntimeData.*` now loads, builds, and validates `EnemySkillNodes.csv` and `EnemySkillNodeParams.csv`.
- `EnemyCombatSystem.cs` executes plan nodes before falling back to `EnemySkillExecutor.Execute(...)`.
- `EnemySkillPlanRuntime` handles `DamageArea`, `SpawnProjectile`, `Heal`, `Damage`, `DamageAndActionSpeedDebuff`, `DamageThenDelayedChain`, `ChargeDamageStatus`, and `ApplyOutgoingDamageMultiplierStatus`.
- Lightning Scout chain uses a pending action with `delay_seconds=0.5` and excludes the first target unit id.
- Arsen intimidation applies a `PassiveBuff` status modifier with `DamageBonusRate=-0.3`, which existing `StatusEffectRuntime.ResolveOutgoingDamageMultiplier(...)` multiplies into outgoing damage.
- Arsen intimidation now applies that `PassiveBuff` with the named `StagePersistentStatus=true` path; inspected `UnitStatusRuntime.Tick(...)` keeps permanent statuses from ticking down, so the effect persists until stage/runtime state reset removes statuses.
- `EnemySkillExecutor.ResolveAttackDamage(...)` now uses explicit attack/spell coefficients and only falls back to compatibility `Coefficient` when both explicit coefficients are zero, preventing spell-only skills such as Chain Lightning from double-counting attack damage.
- `EnemyCombatSystem.cs` now routes Dark Stab and Frost Pressure through `TrySpawnColliderDamageSkill(...)` when a Stage2 skill prefab has an enabled `Collider2D`; Frost Pressure configures status-on-hit through `InGameEnemySkillHitboxActor.ConfigureStatusOnHit(...)`.
- `EnemyCombatSystem.cs` now also routes `DamageArea` plan nodes through `TrySpawnColliderDamageSkill(...)` before falling back to `ExecuteSlash(...)`, so FireDragonSlash can use the collider-backed `fire-dragon-slayer.prefab` path.
- `EnemyCombatSystem.cs` now passes `HitAllColliderTargets=int.MaxValue` only for `StageOneEnemySkillKind.FireDragonSlash`; other `DamageArea` collider skills keep the default single-hit limit.
- `InGameEnemySkillHitboxActor.cs` now continues scanning all roster entries in a frame and does not decrement `remainingHits` when the max-hit sentinel is `int.MaxValue`, allowing FireDragonSlash to damage every contacted non-Nexus opposing unit once per visual lifetime.
- Enemy collider skill hitbox lifetime now uses `SkillVisualSpawnUtility.ResolveVisualLifetime(instance, minimum)` instead of the previous fixed `0.35f`, so animated Stage2 skill prefabs can remain for their authored animation length.
- `InGameEnemySkillHitboxActor.cs` now disables hit detection when `remainingHits` is exhausted instead of destroying the visual GameObject, so collider-backed enemy skill prefabs remain until the animation-length lifetime expires.
- `InGameEnemySkillHitboxActor.cs` now excludes `UnitRole.Nexus` targets from enemy skill hitbox damage, leaving Nexus damage to the enemy Nexus assault path.
- `EnemySkillExecutor.ResolveEnemyProjectileSpeed(enemyModel, skillData, fallbackSpeed)` previously used current move speed x2 for `HolySpearThrow`; the 2026-06-19 projectile follow-up changed it to fixed `12f` so Ethan spear matches Karin `SacredSwordWave` runtime projectile speed.
- `EnemyCombatSystem.cs` now stores Drake `ChargeDamageStatus` as `EnemyCombatState.ActiveCharge` instead of teleporting to the selected target or applying immediate damage.
- `EnemyCombatSystem.cs` now ticks active Drake charge before normal enemy targeting, ramps `enemyModel.MoveSpeedMultiplier` from x1 to x2.5 over 3 seconds, and uses the existing `MoveToward(...)` path for movement.
- `EnemyCombatSystem.cs` now resolves Drake charge hits through `UnitHitboxUtility.IsTargetInsideHitbox(enemyEntry.GetHitboxColliders(), candidate)` against active non-Nexus player units, so any colliding Monster unit can be hit even if it was not the original random charge target.
- `EnemyCombatSystem.cs` now clears Drake active charge on hit and applies damage plus `StatusEffectKind.Freeze` for the node `status_duration` value, defaulting to 5 seconds.
- `EnemyCombatSystem.cs` now resolves Ethan `HolySpearThrow` projectile speed as `12f`, matching the inspected Karin `SacredSwordWave` runtime speed.
- `EnemyCombatSystem.cs` now gives enemy projectile skills `AimedShot`, `ShurikenThrow`, `SacredSwordWave`, and `HolySpearThrow` a 10 second max lifetime and calculates their x-boundary from resolved speed x lifetime so they are not removed by the old short boundary before the 10 second timeout.
- Lightning Scout chain damage remains wired: `EnemySkillNodes.csv` maps `ChainLightning` to `DamageThenDelayedChain`, `EnemySkillNodeParams.csv` sets `chain_multiplier=0.5`, `delay=0.5`, and `chain_radius=7`, and `EnemyCombatSystem.cs` applies the pending chain damage with `pending.DamageMultiplier`.
- `InGameEnemySkillHitboxActor.cs` now supports optional status-on-hit and applies the configured status through `combatManager.ApplyStatus(...)` after a collider hit applies damage.
- Lightning Scout still uses direct target damage and now spawns `lightning-scout_1.prefab` as an attached visual on the directly selected targets; the inspected prefab has no 2D collider, matching the requested direct-target-only behavior.
- Arsen Intimidation still applies the x0.7 outgoing-damage status and now spawns `arsen_Skill.prefab` as an attached visual effect; the inspected prefab has no 2D collider, matching the requested debuff/effect-only behavior.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps Stage2 enemy skill effects for FireDragonSlash, ChainLightning, FrostPressure, DarkStab, HolyDragonHeal, HolySpearThrow, and Intimidation through the existing `EffectManager.enemySkillEffects` scene authority.
- No Drake skill prefab exists under `Pakuri/Assets/Prefab/Enemy/Skill/Stage2` in the inspected file listing, so OpeningCharge remains logic-only in this pass.
- `EnemyTargeting.cs` now exposes farthest/random/all player target helpers used by the enemy node runtime.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and existing `MSB3277` warnings.
- 2026-06-19 follow-up `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and existing `MSB3277` warnings.
- 2026-06-19 later follow-up `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained, and the Editor build also retried once after a transient `Assembly-CSharp.dll` file lock.
- 2026-06-19 later Unity-MCP `validate_script` for `Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` returned 0 warnings and 0 errors.
- 2026-06-19 final `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- 2026-06-19 final Unity-MCP `validate_script` for `Assets/Scripts2/InGame/Skills/Execution/Actors/InGameEnemySkillHitboxActor.cs` returned 0 errors and 1 warning: `Consider using FixedUpdate() for Rigidbody operations`.
- 2026-06-19 later projectile follow-up `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- 2026-06-19 later projectile follow-up Unity-MCP `validate_script` for `Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` returned 0 warnings and 0 errors.
- 2026-06-19 final Drake follow-up `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- 2026-06-19 final Drake follow-up Unity-MCP `validate_script` for `Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` returned 0 warnings and 0 errors.
- 2026-06-19 Arsen/FireDragon follow-up sandboxed `dotnet build` initially failed because access to `C:\Users\t3312\AppData\Local\Microsoft SDKs` was denied; the approved external rerun passed `Pakuri/Assembly-CSharp.csproj` and `Pakuri/Assembly-CSharp-Editor.csproj` with 0 errors and existing `MSB3277` warnings.
- 2026-06-19 Arsen/FireDragon follow-up Unity-MCP `validate_script` for `Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` returned 0 warnings and 0 errors; `Assets/Scripts2/InGame/Skills/Execution/Actors/InGameEnemySkillHitboxActor.cs` returned 0 errors and 1 existing-style warning: `Consider using FixedUpdate() for Rigidbody operations`.
- 2026-06-19 follow-up Unity-MCP validation was not available because `validate_script` returned `No Unity Editor instances found. Please ensure Unity is running with MCP for Unity bridge.`
- PowerShell CSV checks returned `bad=` empty for `EnemySkillData.csv`, `stage_two_enemies.csv`, `EnemySkillNodes.csv`, and `EnemySkillNodeParams.csv`; `Select-String` found no `enemy_scope` or `range` in `EnemySkillData.csv`.
- PowerShell node validation returned `badOps=` and `badSelectors=` empty after excluding the CSV schema row.

### History

- 2026-06-19: User asked Code Builder to start steps 1-7 from the enemy skill node runtime handoff and defer step 8.

## Task: 2026-06-19 EnemySkillData Range Column Removal

### Task title

Remove the unused `range` column from enemy skill runtime CSV data.

### Goals

- Keep current enemy attack-distance behavior unchanged.
- Remove the dead enemy skill `range` CSV column after confirming combat code uses `radius`.
- Preserve CSV catalog sync and validation.

### Constraints

- Role Owner is Code Builder.
- No enemy combat runtime code was changed.
- `EnemyCombatRules.ResolveAttackAttemptRange(...)` still uses positive `skillData.Radius` first, then attack-type fallback.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- Use `radius` for Stage1 enemy skill attack-attempt distance and ally-effect radius until a separate range runtime contract is added.

### Evidence

- `EnemyCombatSystem.cs` resolves attack attempt range from `skillData.Radius` and falls back to attack type values: Ranged 5, MeleeAndRanged 4, Buffer 5, default 1.4.
- `PakuriCsvRuntimeData.EnemyDataset.cs` reads `EnemySkillData.csv` `radius` and assigns it to active/basic skill radius fields.
- `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillData.csv` no longer contains `range`.
- CSV row-width check returned `expected=33` and `bad=` empty.
- Unity-MCP sync/validate completed with 0 warning/error console entries.

### History

- 2026-06-19: User asked whether `range` was unnecessary, then requested Code Builder remove it.

## Task: 2026-06-19 Enemy Skill Node Runtime Handoff

### Task title

Prepare a Code Builder handoff for migrating enemy Stage1 and Stage2 active skills to a node-based execution structure.

### Goals

- Decide whether existing `Assets/Prefab/Enemy/Skill/Stage1` skills should be included in the enemy node migration.
- Give Code Builder a concrete Stage1 parity-first and Stage2 extension plan.
- Keep enemy behavior grounded in inspected current runtime code, active enemy CSVs, existing prefabs, and the Stage2 enemy reference.

### Constraints

- Role Owner is Designer.
- This task produced a handoff only; no runtime code, CSV, prefab, or scene behavior was changed.
- Prefabs are treated as optional visual/projectile/hitbox payloads, not as the authoritative behavior definition.
- MSW-MCP remains excluded; Unity-MCP is the only project MCP path.

### Role Owner

Designer

### Status

Handoff created.

### Next Actions

- Code Builder implements the enemy node data model, compiler, plan/action dispatcher, and Stage1 fallback path.
- Code Builder migrates Stage1 first for behavior parity, then adds Stage2-specific handlers such as chain damage, combat-start triggers, charge movement/contact, and tower debuffs.
- Code Builder adds Stage2 active skill body rows to `EnemySkillData.csv`, removes `enemy_scope`, and uses `radius` as the skill range authority.
- User or Builder verifies Play Mode behavior after implementation.

### Evidence

- Created `Pakuri/reference/Report/2026-06-19-enemy-skill-node-runtime-handoff.md`.
- `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillData.csv` currently stores enemy skill body fields such as `runtime_kind`, coefficients, cooldown, radius, projectile speed/lifetime, duration, flat value, movement multiplier, and outgoing damage multiplier.
- Updated `Pakuri/reference/Report/2026-06-19-enemy-skill-node-runtime-handoff.md` to set Stage2 `EnemySkillData.csv` radius values: Fire Dragon Soldier 2, Lightning Scout 7, Ice Guard 2, Dark Assassin 1.4, Holy Priest 5, Ethan 14, Drake 40, Arsen 40.
- The same report now states `enemy_scope` should be removed and skill usability should come from unit skill assignment, not scope filtering.
- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` binds Stage1 enemies to `stage_one_skill`, `basic_skill`, passive names, passive ids, and passive values.
- `Pakuri/Assets/Scripts2/InGame/Enemy/EnemyDefinition.cs` currently defines Stage1-only `StageOneEnemySkillKind` values.
- `Pakuri/Assets/Scripts2/InGame/Enemy/EnemyCombatSystem.cs` currently resolves enemy skills into `EnemyResolvedSkillData` and executes them through a direct `EnemySkillExecutor.Execute(...)` switch.
- Stage1 prefab folder contains `Achor_Skill.prefab`, `Karin_Skill 1.prefab`, `Preist_Skill.prefab`, `Rogue_Skill.prefab`, `Shield_King_Skill.prefab`, `Shield_Skill.prefab`, `Warrior_King_Skill 1.prefab`, and `Warrior_Skill.prefab`.
- Stage2 prefab folder contains `arsen_Skill.prefab`, `dark-assassin_Skill.prefab`, `ethan_Skill.prefab`, `fire-dragon-slayer.prefab`, `holy-priest_Skill.prefab`, `ice-guard_Skill.prefab`, and `lightning-scout_1.prefab`.
- `Pakuri/reference/5.enemy/stage-2-enemies.md` section `## 2. 모든 적 액티브 스킬` defines Fire Dragon Soldier, Lightning Scout, Ice Guard, Dark Assassin, Holy Priest, Ethan, Drake, and Arsen active skill behavior.

### History

- 2026-06-19: User asked whether existing Stage1 enemy skill prefabs can be included while preparing Stage2 enemy skills, and requested a handoff markdown file for Code Builder.
- 2026-06-19: User revised the handoff so `EnemySkillData.csv` owns Stage2 skill body rows and radius values, with Drake/Arsen combat-start skills using `radius=40` for immediate spawn-time execution.

## Task: 2026-06-19 Target Skill Runtime Structure Fix

### Task title

Apply Code Reviewer findings for the target MonsterUnitActor / RuntimeModel / SkillExecutionPlan / handler structure.

### Goals

- Remove combat/runtime reset state mutation from `MonsterUnitActor`.
- Move existing effect and trigger execution through a shared plan action dispatcher path.
- Stop using Ariel-only choice routing and use node-backed choice routing for any monster with normalized choice nodes.
- Keep current gameplay data behavior compatible while improving the structure for Ariel, Rin, Vega, Eve, and Sein migration.

### Constraints

- Role Owner is Code Builder.
- This pass keeps existing CSV schemas and effect/trigger definition objects compatible.
- Core effect execution internals remain in the existing executor methods, but dispatch selection now enters through `SkillPlanActionDispatcher`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, Unity-MCP script-validated, and InGame skill data validated.

### Next Actions

- User verifies Play Mode behavior for Ariel and any Rin/Vega choices that already have normalized choice nodes.
- Later work can split individual effect action internals into separate handler classes once behavior parity is confirmed.

### Evidence

- `MonsterUnitActor.cs` no longer contains `AutoAttackEnabled`, `AutoSkillEnabled`, `Statuses`, `Resources`, `SkillRuntime`, or `CurrentHealth` state reset code.
- `MonsterUnitRuntimeStateService.cs` now owns next-day runtime reset for monster model auto flags, statuses, shields, health, and active skill runtime state.
- `SceneEntryManager.cs` calls `MonsterUnitRuntimeStateService.RestoreForNextDay(model)` before `actor.ReviveForNextDay()`.
- `SkillExecutionPlan.cs` now exposes `SkillEffectAction` / `SkillTriggerAction` wrappers and `EffectActions` / `TriggerActions`, replacing public raw `Plan.Effects` / `Plan.Triggers` exposure.
- `SkillPlanActionDispatcher.cs` now owns effect-kind dispatch and trigger-action dispatch.
- `SkillMultiEffectExecutor.cs` now routes direct, filtered, and delayed effect execution through `SkillPlanActionDispatcher.ExecuteEffect(...)`.
- `SkillTriggerRuntime.cs` now routes trigger action selection through `SkillPlanActionDispatcher.ExecuteTriggerAction(...)`.
- `SkillExecutionSnapshot.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSystem.cs` no longer contain `IsArielChoice` or `ApplyArielChoiceDefinition`; choice routing is based on whether `NormalizedPlanNodes` exist.
- Search for `IsArielChoice`, `ApplyArielChoiceDefinition`, `Plan.Effects`, and `Plan.Triggers` under `Pakuri/Assets/Scripts2/InGame/Skills` returned no matches.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `validate_script` returned 0 warnings and 0 errors for `MonsterUnitRuntimeStateService.cs`, `SceneEntryManager.cs`, `SkillExecutionPlan.cs`, `SkillPlanActionDispatcher.cs`, `SkillMultiEffectExecutor.cs`, `SkillTriggerRuntime.cs`, `SkillExecutionSnapshot.cs`, and `InGameSkillDefinitionMapper.cs`.
- After clearing the console, Unity-MCP `Pakuri/InGame/Validate Skill Data` logged `InGame skill data validation passed with 0 warning(s)`, and warning/error console read returned 0 entries.
- `git diff --check` exited successfully; only line-ending warnings were printed.

### History

- 2026-06-19: User asked Code Builder to fix the Code Reviewer findings on Ariel's target runtime structure.

## Task: 2026-06-19 Shared SkillExecutionPlan Effect/Trigger Foundation

### Task title

Move shared skill runtime toward a monster-agnostic plan node execution structure.

### Goals

- Let Eve, Ariel, Rin, Sein, and Vega use the same future `SkillExecutionPlan` path instead of Ariel-only plan-action routing.
- Project existing skill multi-effect rows and source-owned trigger rows into `SkillExecutionPlanNode` payloads without changing CSV row behavior.
- Route active skill executors through plan-resolved effect lists while preserving the existing `SkillMultiEffectExecutor` behavior.
- Make `UnitSkillController` create a typed `SkillExecutionRequest` for auto/manual casts before global dispatch.

### Constraints

- Role Owner is Code Builder.
- This pass is behavior-preserving: existing CSV schemas and old trigger/effect runtime execution remain compatible.
- Trigger/effect rows are now visible in plan nodes, but full handler replacement of `SkillTriggerRuntime` and `SkillMultiEffectExecutor` is still future work.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, Unity-MCP script-validated, CSV-synced, and InGame skill data validated.

### Next Actions

- Later migration can replace remaining direct `SkillTriggerRuntime` and `SkillMultiEffectExecutor` internals with explicit plan action handlers.
- Later migration can remove Ariel-only choice routing once all normalized choice nodes use the common plan action path.
- User verifies unchanged auto/manual skill behavior and trigger/effect behavior in Play Mode.

### Evidence

- `SkillExecutionPlan.cs` now allows `SkillExecutionPlanNode` to carry `SkillEffectDefinition` and `SkillTriggerDefinition` payloads and exposes compiled `Effects` and `Triggers`.
- `SkillData.cs` now stores `SkillTriggers`, and `InGameSkillDefinitionMapper.cs` maps monster trigger rows onto their source skill data for all monsters.
- `SkillRuntimeInstance.cs` now compiles a `BasePlan` so active source-owned trigger lookup can use plan-projected trigger rows.
- `SkillPlanActionDispatcher.cs` now resolves effect/trigger payloads from the compiled plan with legacy fallback.
- `BeamSkillExecutor.cs`, `ProjectileSkillExecutor.cs`, `SingleAttackSkillExecutor.cs`, `SupportSkillExecutors.cs`, and `ZoneSkillExecutor.cs` now read plan-resolved effects before calling existing multi-effect execution.
- `SkillTriggerRuntime.cs` now resolves source-owned active-skill triggers through the runtime base plan, falling back to monster-level trigger rows when needed.
- `SkillExecutionRequest.cs` was added, and `UnitSkillController.cs` now creates auto/manual request objects before `SkillExecutionSystem` routes execution.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `validate_script` returned 0 warnings and 0 errors for `SkillExecutionPlan.cs`, `SkillExecutionRequest.cs`, and `SkillPlanActionDispatcher.cs`.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` executed; console logged runtime catalog load and `InGame skill data validation passed with 0 warning(s)`.
- Unity-MCP warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested Code Builder implementation of the target structure for all monsters, not just Ariel.
- 2026-06-19: Code Builder implemented a shared behavior-preserving foundation: plan node effect/trigger payloads, plan-resolved executor effects, active trigger plan projection, and typed skill execution requests.

## Task: 2026-06-16 UnitSkillController Runtime Refactor Phase 1-6

### Task title

Add a behavior-preserving `UnitSkillController` shell, move manual decision routing into it, extract current `SingleAttack` execute/boss/kill rules into handlers, add operation-record bridges, compile those current operation records into an initial `SkillExecutionPlan`, add a normalized authoring node surface without changing current CSV schema, and close the refactor slice with cleanup and verification evidence.

### Goals

- Convert the 2026-05-29 skill runtime refactor feedback into an implementation-stage handoff.
- Keep the first implementation phases behavior-preserving and compatible with current CSV/data/runtime paths.
- Make `UnitSkillController` the planned unit-scoped skill decision owner while keeping shared combat mutation in `InGameCombatManager`.
- Implement Phase 1 by moving the existing per-entry skill tick/auto-route loop behind `UnitSkillController` without changing manual skill routing or executor behavior.
- Implement Phase 2 by routing manual selected-skill execution through the cached `UnitSkillController` while preserving `InGameCombatManager.HandleSelectedPlayerManualSkillInput()` as input owner.
- Keep active-skill animation request owned by `UnitSkillController` and supplied into the shared route method as a success callback.
- Implement Phase 3 by moving current `SingleAttack` execute-threshold, execute damage/crit, boss damage, and kill cooldown reset/refund rules into local handler classes while preserving current CSV/data/snapshot fields.
- Implement Phase 4 by representing current `SingleAttack` execute/boss/kill snapshot behavior through grouped operation records while retaining the old flat snapshot properties as compatibility bridges.
- Implement Phase 5 by compiling current `SkillData` identity and current snapshot operation records into `SkillExecutionPlan`, then feeding that plan to the existing `SingleAttack` rule handlers without changing CSV schema or executor routing.
- Implement Phase 6 by allowing normalized plan nodes to coexist with legacy wide-column operation bridges in the runtime plan surface, without creating new CSV files, deleting old columns, or changing parser/catalog behavior.
- Implement Phase 7 by removing only obsolete adapter wrappers, updating the combat board, and recording non-gameplay verification evidence.

### Constraints

- Role Owner is Code Builder after the initial Designer handoff.
- No CSV, prefab, scene, status, damage, or executor behavior changes were performed in Phase 1.
- `SkillExecutionSystem.Tick(...)` remains the public global entry point.
- Existing manual skill execution still exposes the old `SkillExecutionSystem.TryExecuteManual(...)` API, but internally delegates to the unit controller.
- Phase 3 does not change CSV headers, parser logic, `SingleAttackData`, `SkillChoiceEffectSpec`, or `SkillExecutionSnapshot` field compatibility.
- Phase 4 does not remove existing flat `SkillExecutionSnapshot` properties; it adds operation lists beside them and routes the Phase 3 handlers through those lists.
- Phase 5 does not change CSV headers, parser logic, source CSV rows, executor registry routing, or base skill data classes; the first plan only wraps current source identity plus the Phase 4 operation records.
- Phase 6 does not split current CSV files into condition/action/modifier tables and does not delete old columns; it only adds runtime plan node types and a compiler overload for future normalized row inputs.
- Phase 7 does not remove compatibility fields or old CSV/data paths; only unneeded local wrapper adapters are eligible for cleanup.
- User requested avoiding excessive fallback functions; Phase 1 adds one controller shell and delegates to the existing route method.
- Code Reviewer execution still requires explicit user permission.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Phase 1, Phase 2, Phase 3, Phase 4, initial Phase 5, Phase 6 normalized-node runtime surface, and Phase 7 cleanup/verification implemented. Reviewer allocation fix applied. Locally build/editor/Unity-MCP script-validated.

### Next Actions

- User performs Play Mode verification for unchanged auto skill routing, selected manual skill behavior, and `SingleAttack` execute/boss/kill behavior.
- User performs Play Mode verification for unchanged Phase 5 plan-fed `SingleAttack` execute/boss/kill behavior.
- Future normalized CSV/schema work must still be a separate DATA-scoped step, after user approval, because Phase 6 did not add or alter authoring tables.
- Phase B normalized CSV/data integration now feeds authored `SkillNodeDefinition` rows into `SkillExecutionPlan`; Phase C adds the first `rin-d` normalized execute/kill sample and an `ExecuteMultiplier` plan op.
- Run Code Reviewer only when the user explicitly requests the Phase 7/final refactor review.
- Update this board if shared skill execution, executor routing, or `UnitSkillController` ownership changes are implemented.
- Update `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md` only if status/buff handler behavior changes.
- Update `boards/DATA/DATA_BLACKBOARD.md` only if CSV schema, parser, validation, or runtime catalog behavior changes.

### Evidence

- Created `Pakuri/reference/Report/2026-06-16-unit-skill-controller-runtime-refactor-handoff.md`.
- Source handoff inspected: `Pakuri/reference/Report/2026-05-29-skill-runtime-refactor-feedback-handoff.md`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:54` owns `SkillExecutionSystem`; `:95` to `:105` calls `skillExecution.Tick(...)` from `Update()`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs:13` to `:15` stores reusable `UnitSkillController` instances by `UnitRosterEntry` and a reusable stale-entry list.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs:25` defines the central tick entry, `:39` to `:44` prunes the controller cache and iterates roster entries, and `:121` to `:122` reuses the cached controller for each unit tick.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs:48` to `:72` preserves the public manual execution API and delegates it to `UnitSkillController.TryExecuteManual(...)`.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:69` already stores `UnitSkillRuntimeSet SkillRuntime`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeFactory.cs:67` creates `new SkillRuntimeInstance(owner, skillData)`.
- Before Phase 3, `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:132`, `:1461` to `:1475`, and `:1745` to `:1759` directly handled execute threshold, boss multiplier, and kill cooldown reset/refund rules.
- Search under `Pakuri/Assets/Scripts2/InGame` found no `UnitSkillController`.
- Added `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/UnitSkillController.cs`; `:5` defines `public sealed class UnitSkillController`, `:7` defines `SkillRouteRequest`, and `:31` defines its `Tick(...)` method.
- `UnitSkillController.cs:45` ticks `skillRuntime`, `:46` preserves the existing auto-skill/alive/can-act guard, `:55` preserves the injected auto-route predicate, and `:60` to `:71` delegates auto execution to the existing route request.
- `UnitSkillController.cs:75` to `:96` adds `TryExecuteManual(...)`, so selected manual skill execution now enters the unit-scoped controller before shared route dispatch.
- `UnitSkillController.cs:98` to `:102` owns active-skill animation notification request and passes it to the shared route delegate for successful routed casts.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs:125` to `:133` creates `UnitSkillController(entry, TryRouteSkill)` only on cache miss; the per-frame `TickEntry(...)` path no longer allocates a controller every frame.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs:136` to `:170` prunes cached controllers whose `UnitRosterEntry` is no longer present in `roster.Entries`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs:173` to `:184` keeps shared execution dispatch centralized while accepting a controller-supplied `notifyActiveSkillAnimation` callback.
- `Pakuri/Assembly-CSharp.csproj:68` includes `Assets\Scripts2\InGame\Skills\Execution\Runtime\UnitSkillController.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillRuleHandlers.cs`; `:7` defines `ISkillCastCondition`, `:12` defines `ISkillDamageModifier`, `:17` defines `ISkillPostHitAction`, `:97` defines `TargetHealthRatioCastCondition`, `:137` defines `ExecuteDamageModifier`, `:155` defines `BossDamageModifier`, `:172` defines `KillCooldownResetAction`, and `:190` defines `KillCooldownRefundAction`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:125` to `:131` now delegates execute-threshold cast rejection to `SingleAttackSkillRuleHandlers`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:1442` to `:1448` now delegates execute and boss damage modifier application to `SingleAttackSkillRuleHandlers`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:1684` to `:1693` now delegates kill cooldown recovery to `SingleAttackSkillRuleHandlers`.
- `Pakuri/Assembly-CSharp.csproj:150` includes `Assets\Scripts2\InGame\Skills\Execution\Executors\SingleAttackSkillRuleHandlers.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs:8` to `:78` now defines `CastConditionOp`, `DamageModifierOp`, `CritModifierOp`, and `KillActionOp` record structs with matching op-kind enums.
- `SkillExecutionSnapshot.cs:206` to `:214` stores and exposes `CastConditionOps`, `DamageModifierOps`, `CritModifierOps`, and `KillActionOps` as operation-list bridges while keeping the old flat properties at `:119`, `:136`, and `:139` to `:142`.
- `SkillExecutionSnapshot.cs:278` to `:406` still accumulates `ExecuteHealthRatioBonus`, `ExecuteCritChanceBonus`, `BossDamageMultiplier`, `KillCooldownRefundRatioBonus`, `KillResetsCooldown`, and `KillResetsCooldownRequiresExecute` from existing choice specs.
- `SkillExecutionSnapshot.cs:935` to `:964` rebuilds the `SingleAttack` operation bridges from the current flat compatibility properties.
- `SingleAttackSkillRuleHandlers.cs:121`, `:165`, `:192`, `:221`, and `:253` now read snapshot operation lists for execute threshold bonus, execute crit bonus, boss multiplier, kill reset, and kill refund bonus.
- Added `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs`; `:6` defines `SkillExecutionPlan`, `:24` to `:29` expose the source skill identity and compiled operation lists, and `:48` to `:59` define `SkillExecutionPlanCompiler.Compile(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs:98` builds an initial plan, `:103` exposes `Plan`, `:622` to `:623` rebuilds operation bridges and the plan after each choice spec is applied, and `:971` to `:974` owns the plan rebuild bridge.
- `SingleAttackSkillRuleHandlers.cs:121`, `:165`, `:192`, `:222`, and `:254` now read cast, crit, damage, and kill operation lists through `snapshot.Plan` instead of directly through the snapshot operation lists.
- `Pakuri/Assembly-CSharp.csproj:130` includes `Assets\Scripts2\InGame\Skills\Execution\Runtime\SkillExecutionPlan.cs`.
- `SkillExecutionPlan.cs:6` to `:24` now defines normalized authoring-source and plan-node kind enums for legacy wide-column and future normalized-row coexistence.
- `SkillExecutionPlan.cs:26` to `:101` defines `SkillExecutionPlanNode` with optional cast-condition, damage-modifier, crit-modifier, and kill-action payloads plus row/source metadata.
- `SkillExecutionPlan.cs:103` to `:129` keeps `SkillExecutionPlan` source identity and executable op lists while also exposing copied normalized `Nodes`.
- `SkillExecutionPlan.cs:147` to `:203` merges node payload ops into the same typed operation lists read by current handlers, so future normalized rows can feed existing compatibility handlers.
- `SkillExecutionPlan.cs:213` to `:226` adds a compiler overload that accepts normalized plan nodes while the existing `Compile(source, snapshot)` path remains unchanged.
- Phase B added normalized node flow from `SkillDefinition.NormalizedPlanNodes` and `SkillChoiceDefinition.NormalizedPlanNodes` through `InGameSkillDefinitionMapper.MapSkillNodeDefinitions(...)` into `SkillData.NormalizedPlanNodes` and `SkillChoiceEffectSpec.NormalizedPlanNodes`.
- Phase B `SkillExecutionSnapshot` now stores `NormalizedPlanNodes`, adds source nodes on construction, adds choice nodes from `ApplyChoiceSpec(...)`, and calls `SkillExecutionPlanCompiler.Compile(Source, this, normalizedPlanNodes)`.
- Phase B supported handlers currently converted into executable plan ops are `TargetHealthRatioThresholdBonus`, boss-damage handlers, `ExecuteCritChanceBonus`, and kill cooldown reset/refund handlers; other normalized handlers remain metadata-only `SkillExecutionPlanNode` entries until their executor semantics are implemented.
- Phase B Unity-MCP smoke returned `nodes=1, damageModifiers=1, firstRow=phase_b_test_node, multiplier=1.25`, proving a normalized node can appear in `SkillExecutionPlan.Nodes` and feed an executable plan op.
- Phase B reviewer follow-up blocks passive/effect/trigger-owned normalized rows in CSV validation until those owner paths are actually wired into runtime plans, so runtime plan integration currently supports skill-owned and choice-owned node rows only.
- Phase B reviewer follow-up Unity-MCP smoke returned `nodes=1, damageModifiers=1, row=phase_b_review_node, multiplier=1.25, support=RuntimeImplemented:phase_b_review_node`, proving the skill-owned normalized boss modifier path still feeds the compiled plan after metadata preservation changes.
- Phase C adds `DamageModifierOpKind.ExecuteMultiplier` and maps `ExecuteDamageMultiplier` normalized nodes into the `SingleAttack` execute damage modifier path, so migrated execute multiplier values can come from plan nodes instead of `SingleAttackData.ExecuteDamageMultiplier`.
- Phase C migrates `rin-d` base execute/kill behavior into normalized plan nodes; Unity-MCP runtime catalog inspection returned `legacy=threshold:0,require:True,execute:1,refund:0,boss:1|defNodes=4|planNodes=4|casts=1:0.3|damage=2:ExecuteMultiplier:1.8,BossMultiplier:1|kills=1:CooldownRefundBonus:0.35`.
- Phase C duplicate guard validation prevents supported normalized execute/boss/kill rows from coexisting with active matching legacy wide values on the same skill or choice owner.
- Phase D adds representative choice-owned normalized nodes for damage/cooldown/radius modifiers, execute/boss/kill choice actions, on-hit additional damage, repeat per target, conditional crit, and redistribute-on-kill behavior while keeping the old wide columns readable for compatibility.
- Phase D maps generic choice nodes such as `DamageMultiplier`, `CooldownMultiplier`, `RadiusMultiplier`, `AdditionalDamage`, `EveryNthHitChainDamage`, `RepeatPerTarget`, `TargetStatusCritBonus`, and `RedistributeConsumedStatus` back into `SkillChoiceEffectSpec`; execute/boss/kill choice nodes continue through `SkillExecutionPlanNode` operation payloads.
- Phase D Unity-MCP choice smoke returned `rin-a-master-2=extraTrue:1:0.4:Lightning:HitTarget|chain3:2:4.5:0.4:Lightning:nodes2`, `vega-d-master-1=damageTrue:0.65|repeat2:0.15:0.6:nodes2`, `vega-e-trait-4=crit0.35:name-mark:1:nodes1`, and `vega-e-trait-5=redistribute0.25:name-mark:5:3:nodes1`.
- Phase D Unity-MCP plan smoke for `rin-d` after applying `rin-d-trait-2` and `rin-d-master-1` returned `nodes=7|planNodes=7|casts=2:0.3|crits=1:0.5|kills=2:CooldownRefundBonus:False:0.35,CooldownReset:True:0`, confirming base Phase C nodes and Phase D choice nodes coexist in the compiled plan without legacy duplicate application.
- Phase D reviewer follow-up keeps representative choice metadata in `monster_skill_choice_base.csv`, makes duplicate legacy choice rows use base metadata while preserving legacy behavior fields, and allows future base-only choice rows to enter runtime choices with normalized nodes.
- Phase D reviewer follow-up validation accepts `SkillChoiceBaseRows` as valid choice owners/gates and enforces duplicate base/legacy routing-field agreement for `monster_id`, `skill_id`, and `choice_group`.
- Phase D reviewer follow-up Unity-MCP smoke returned each of the 11 migrated representative choice ids with `count=1` and expected normalized node counts, so the base metadata integration did not duplicate runtime choices or drop existing normalized choice nodes.
- Phase B Unity-MCP current-catalog check returned `catalog=True, activeSkills=25, skillNodes=0, choiceNodes=0`, so the current empty normalized CSV rows do not alter existing skill plans.
- Phase B `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase B reviewer follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase C `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase 7 cleanup removed the now-unneeded private `SingleAttackSkillExecutor` wrappers for execute-threshold rejection and kill recovery; `SingleAttackSkillExecutor.cs:83`, `:763`, `:830`, `:900`, and `:945` now call `SingleAttackSkillRuleHandlers` directly.
- Phase 7 reference search under `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` found only direct `SingleAttackSkillRuleHandlers.ShouldRejectCastForExecuteThreshold(...)` and `SingleAttackSkillRuleHandlers.HandleKillRecovery(...)` calls after wrapper removal.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing `MSB3277` warnings; one earlier parallel build failed only with `CS2012` file lock on `obj\Debug\Assembly-CSharp.dll`.
- Phase 3 `git diff --check` reported no whitespace errors and only CRLF working-copy warnings.
- Phase 4 `git diff --check` reported no whitespace errors and only CRLF working-copy warnings.
- Phase 5 `git diff --check` reported no whitespace errors and only CRLF working-copy warnings.
- Phase 6 `git diff --check` reported no whitespace errors and only CRLF working-copy warnings.
- Unity-MCP `validate_script` reported 0 warnings and 0 errors for `Assets/Scripts2/InGame/Skills/Execution/Runtime/UnitSkillController.cs` and `Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs`.
- Unity-MCP `read_console` returned 0 error log entries after validation.
- Phase 3 Unity-MCP `validate_script` and `read_console` could not run because the tool returned `No Unity Editor instances found. Please ensure Unity is running with MCP for Unity bridge.`
- Phase 4 Unity-MCP `validate_script` reported 0 warnings and 0 errors for `Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` and `Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillRuleHandlers.cs` after retry; Unity-MCP console read returned UnityConnect token exchange and MCP transport errors, not C# compile diagnostics.
- Phase 5 Unity-MCP `validate_script` reported 0 warnings and 0 errors for `Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs`, `Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs`, and `Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillRuleHandlers.cs`; Unity-MCP `read_console` returned 0 warning/error entries.
- Phase 6 Unity-MCP `validate_script` reported 0 warnings and 0 errors for `Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs`; Unity-MCP `read_console` returned 0 warning/error entries.
- Phase 7 `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after a single-project rerun; a previous parallel build attempt failed only with `CS2012` file lock on `obj\Debug\Assembly-CSharp.dll`.
- Phase 7 `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing `MSB3277` warnings.
- Phase 7 `git diff --check` reported no whitespace errors and only CRLF working-copy warnings.
- Phase 7 Unity-MCP editor state reported idle, not compiling, no domain reload pending, and ready for tools.
- Phase 7 Unity-MCP `read_console` returned 0 warning/error entries before script validation.
- Phase 7 Unity-MCP `validate_script` reported 0 warnings and 0 errors for `Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs`, `Assets/Scripts2/InGame/Skills/Execution/Runtime/UnitSkillController.cs`, `Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs`, `Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs`, `Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillRuleHandlers.cs`, and `Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs`; two first parallel validation calls transiently returned `No Unity Editor instances found` and passed on retry.

### History

- 2026-06-16: User requested a handoff that divides work stages for restructuring current skill runtime according to the 2026-05-29 feedback and creating `UnitSkillController`.
- 2026-06-16: User switched to Code Builder and requested Phase 1 implementation while avoiding excessive fallback functions. Code Builder added the behavior-preserving controller shell and kept existing routing/executor behavior delegated through `SkillExecutionSystem`.
- 2026-06-16: User requested the Code Reviewer fix request to remove per-frame `UnitSkillController` allocation. Code Builder changed `SkillExecutionSystem` to cache controllers by `UnitRosterEntry` and prune stale entries against `roster.Entries`.
- 2026-06-16: User requested Code Builder Phase 2 implementation. Code Builder added `UnitSkillController.TryExecuteManual(...)`, preserved `SkillExecutionSystem.TryExecuteManual(...)` as a compatibility wrapper, and moved active-skill animation notification request into the unit controller callback path.
- 2026-06-16: User requested Code Builder Phase 3 implementation. Code Builder added local single-attack rule handlers and changed `SingleAttackSkillExecutor` to delegate current execute, boss, and kill cooldown rules without changing CSV/data/snapshot compatibility fields.
- 2026-06-16: User requested Code Builder Phase 4 implementation. Code Builder added snapshot operation-record bridges and changed the Phase 3 handlers to consume those operation lists while preserving existing flat snapshot properties.
- 2026-06-16: User requested Code Builder Phase 5 implementation. Code Builder added an initial `SkillExecutionPlan` compiler, made `SkillExecutionSnapshot` rebuild the plan from current operation records, and routed current `SingleAttack` rule handlers through the plan while preserving CSV/data/executor compatibility.
- 2026-06-16: User requested Code Builder Phase 6 implementation. Code Builder added runtime normalized plan-node types and a compiler overload so future normalized row-set authoring can coexist with legacy wide-column operation bridges without changing current CSV/parser/catalog behavior.
- 2026-06-17: User requested Code Builder Phase 7 implementation. Code Builder removed obsolete local `SingleAttackSkillExecutor` wrapper adapters, kept compatibility fields and CSV/data paths intact, updated this board, and verified the refactor slice with dotnet builds, diff check, Unity editor state, console read, and Unity-MCP script validation.
- 2026-06-17: User requested Code Builder Phase B for normalized skill authoring. Code Builder connected DATA-level `SkillNodeDefinition` rows into the existing `SkillExecutionPlan` path while keeping current legacy wide-column operation bridges active.
- 2026-06-17: Code Builder fixed Phase B Code Reviewer findings by preserving normalized node support metadata and preventing unsupported passive/effect/trigger owner rows from silently disappearing before runtime adapters exist.
- 2026-06-17: User requested Code Builder Phase C. Code Builder migrated the first `rin-d` execute/kill sample into normalized nodes, added execute multiplier node execution, and added duplicate guard validation for supported legacy+normalized behavior overlap.
- 2026-06-17: User requested Code Builder Phase D. Code Builder migrated representative choice behavior families into normalized choice-owned nodes and verified that they map into `SkillChoiceEffectSpec` or compiled `SkillExecutionPlan` operations as appropriate.

## Task: 2026-06-07 Collider-Only Skill Hit Validation

### Task title

Remove target-point and radius fallback hit checks now that monster unit prefabs have colliders.

### Goals

- Require skill hitbox/projectile overlap against target colliders instead of target transform points.
- Remove projectile and enemy hitbox actor radius fallback when the skill prefab lacks enabled colliders.
- Keep enemy offensive skill prefabs on collider-driven hit detection.

### Constraints

- Role Owner is Code Builder.
- User added `Collider2D` components to all `Assets/Prefab/Monster/*_Unit.prefab` files before this code change.
- Enemy offensive skill prefabs under `Assets/Prefab/Enemy/Skill` already have colliders for `Warrior_Skill.prefab`, `Achor_Skill.prefab`, `Rogue_Skill.prefab`, and `Karin_Skill 1.prefab`.
- Non-offensive enemy visual prefabs such as priest/shield/command visuals do not need hitbox colliders for damage routing.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that enemy projectile/slash attacks hit only through collider overlap against monster colliders.
- User verifies monster skills still hit enemies correctly through enemy colliders after the shared hitbox utility change.

### Evidence

- `Assets/Prefab/Monster/Ariel_Unit.prefab`, `Eve_Unit.prefab`, `Rin_Unit.prefab`, `Sein_Unit.prefab`, and `Vega_Unit.prefab` each have one detected 2D collider.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitRosterService.cs` no longer uses `hitbox.OverlapPoint(target.ResolveTargetPoint())`; `UnitHitboxUtility.IsTargetInsideHitbox(...)` now requires target colliders and checks collider distance overlap.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs` no longer uses the `hitRadius` roster fallback when the projectile has no enabled collider; it returns without a hit instead.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameEnemySkillHitboxActor.cs` no longer uses the `hitRadius` roster fallback when the hitbox prefab has no enabled collider; it returns without a hit instead.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameZoneSkillActor.cs` debug hitbox path no longer uses target-point overlap and now follows collider-only overlap checks.
- Search for `OverlapPoint(targetPoint)`, `ResolveTargetPoint().*sqrMagnitude`, `var radiusSq = hitRadius * hitRadius`, and `PointCheck hitbox` under `Pakuri/Assets/Scripts2/InGame` returned no matches after the change.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-06-07: User confirmed monster unit prefabs now have colliders and requested removal of fallback hit detection paths.

## Task: 2026-05-31 Monster Revive Targetability Restore

### Task title

Restore defeated monster targetability through actor revive on the next day.

### Goals

- Keep defeated monster bodies persistent during the cleared day.
- Restore HP, colliders, roster registration, and idle animation when the next day begins.
- Restore common combat state for revived selected and manifested monsters so they can attack again.
- Preserve the selected monster Auto mode from before death while manifested monsters return with AutoSkill enabled.
- Avoid creating replacement monster prefabs when a dead actor already exists for the party slot.

### Constraints

- Role Owner is Code Builder.
- The revive path is slot-based for selected slot 0 and manifested slots 1-4.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated. 2026-05-31 follow-up fixed revived monster attack/Auto state.

### Next Actions

- User verifies in Play Mode that revived selected and manifested monsters are targetable again, attack again, and enemies attack the revived actor instead of a duplicate prefab.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now searches existing scene `MonsterUnitActor` instances by `UnitIdentity.SlotIndex` before spawning selected or manifested monsters on day advance.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` re-registers revived actors through `combatManager.RegisterPlayerMonster(...)`.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now restores max HP, re-enables child colliders, and plays idle through `ReviveForNextDay()`.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now restores common revived combat state by setting `AutoAttackEnabled=true`, clearing statuses and shields, and resetting each learned active `SkillRuntimeInstance`.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` restores `AutoSkillEnabled=true` only for non-selected monsters, so manifested monsters attack while selected monster Auto remains controlled by `InGameCombatManager.PlayerAutoSkillEnabled`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now finds the selected monster by `UnitRole.Monster` and `SlotIndex == 0` instead of `roster.Players[0]`, preventing Nexus from receiving selected-player Auto state.
- `Pakuri/Assets/Scripts2/InGame/Animation/Animation_Controller.cs` now has `ReviveToIdle()` to undo death freeze and return to idle animation.
- Unity-MCP `validate_script` reported 0 errors for `SceneEntryManager.cs`, `MonsterUnitActor.cs`, and `Animation_Controller.cs`; only the existing `Animation_Controller.Update()` GC warning remained.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- 2026-05-31 follow-up validation: Unity-MCP `validate_script` reported 0 errors for `MonsterUnitActor.cs`; `InGameCombatManager.cs` validator reported a duplicate `ResolveEffectManager` signature, but PowerShell search found only one declaration at line 1202 and both dotnet builds passed with 0 errors.
- 2026-05-31 follow-up validation: Unity refresh reached idle and Unity warning/error console read returned 0 entries.

### History

- 2026-05-31: User requested stage/day advance to revive dead monsters instead of spawning a fresh monster prefab.
- 2026-05-31: Code Builder added revive and re-registration before existing prefab respawn paths.
- 2026-05-31: User reported revived monsters could not attack/Auto; Code Builder added common revived combat-state restore and selected-player lookup by monster slot.

## Task: 2026-05-31 Persistent Monster Death Body

### Task title

Keep defeated monsters visible while removing them from targetable combat runtime.

### Goals

- Stop destroying `MonsterUnitActor` GameObjects when monster HP reaches 0.
- Keep the existing roster unregister path so defeated monsters are no longer target candidates.
- Disable defeated monster colliders so collision-based lookup does not keep treating the body as an active unit.
- If `Animation_Controller` exists, play death animation and freeze on the final death frame.

### Constraints

- Role Owner is Code Builder.
- Enemy and other non-monster deaths still use the existing delayed `Destroy(...)` path.
- Unity Play Mode gameplay verification remains user-owned.
- Unity-MCP script validation could not run because no Unity Editor instance was connected.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that defeated monsters remain visible, are not targeted, and hold the final death sprite.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls `MonsterUnitActor.MarkDefeated()` and returns instead of destroying monster actors inside `RemoveUnitIfDead(...)`.
- The same method still unregisters the dead model from `UnitRosterService`, so `EnemyTargeting.IsActive(...)` and skill target searches no longer see that monster as alive/targetable.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now has `MarkDefeated()`, disables child `Collider2D` components, and plays death animation if `Animation_Controller` is present.
- `Pakuri/Assets/Scripts2/InGame/Animation/Animation_Controller.cs` now freezes death by replaying the dead state at normalized time `0.999f`, updating once, and setting `animator.speed = 0f`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after an initial parallel-build file lock.

### History

- 2026-05-31: User requested monsters not disappear after death, become untargetable, and hold the final death animation sprite when `Animation_Controller` exists.
- 2026-05-31: Code Builder changed monster death handling to persistent visible bodies with disabled colliders and final-frame death animation freeze.

## Task: 2026-05-31 Enemy Nexus Assault Runtime

### Task title

Let enemies target and damage the Nexus after all monster/player targets are gone.

### Goals

- Preserve the current monster-first enemy targeting behavior.
- Let Nexus become the fallback target when no non-Nexus player unit is alive.
- Apply CSV-authored Nexus damage on contact.
- Despawn the enemy that successfully damages the Nexus.

### Constraints

- Role Owner is Code Builder.
- Nexus assault uses a separate Nexus runtime actor, not `MonsterUnitActor`.
- Default Nexus damage is 1 when data is missing or non-positive.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that enemies only rush Nexus after monsters are cleared and that each damaging enemy disappears.
- Tune `nexus_damage` values in stage enemy CSVs if later enemies should deal more than 1 Nexus damage.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EnemyTargeting.cs` now treats `UnitRole.Nexus` as fallback by excluding Nexus during the first nearest-player scan.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` routes Nexus targets to `TickNexusAssault(...)` instead of normal enemy skill rotation.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` applies `Mathf.Max(1f, enemyModel.NexusDamage)` to the Nexus and calls `combatManager.DespawnUnit(enemyModel)`.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitRuntimeModel.cs`, `EnemyDefinition.cs`, `UnitFactory.cs`, and `PakuriCsvRuntimeData.Build.cs` carry the new `NexusDamage` value into runtime enemies.
- Unity-MCP `validate_script` on `Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` reported 0 warnings and 0 errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User asked for enemies to damage Nexus after monster targets are gone and disappear after dealing Nexus damage.
- 2026-05-31: Code Builder added Nexus fallback targeting plus contact-damage despawn behavior.

## Task: 2026-05-31 Stage2 Enemy Runtime And Passive Extension

### Task title

Extend enemy runtime lookup and passive application so Stage 2 enemies can spawn and apply their authored passives.

### Goals

- Let enemy lookup resolve both Stage 1 and Stage 2 enemy definitions.
- Add reusable enemy passive IDs for attribute-specific damage and defense increases.
- Keep existing Stage 1 passive IDs and behavior compatible.

### Constraints

- Role Owner is Code Builder.
- This work does not implement new enemy active skill kinds; Stage 2 rows still use the existing `StageOneEnemySkillKind` contract.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, CSV-validated, and Stage 2 spawn-coordinate checked.

### Next Actions

- User verifies Stage 2 enemy combat behavior in Play Mode.
- If Stage 2 enemies need unique active skills later, extend enemy skill data separately instead of overloading this passive-runtime task.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Definition/GameDataCatalog.cs` now exposes `StageTwoEnemies`, `GetStageTwoEnemyById(...)`, and `GetEnemyById(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriDataManager.cs` registers Stage 2 enemies and resolves `EnemyDefinition` from either stage dictionary.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` resolves enemy definitions with `catalog.GetEnemyById(...)` and resolves prefab overrides through `enemyPrefabBindings`.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` Stage 2 rows now use the same spawn coordinate pattern as Stage 1: normal and escort rows use `spawn_x=9.02`, `spawn_y_min=-5`, `spawn_y_max=5`; guaranteed boss rows use `spawn_x=9.02`, `spawn_y_min=0`, `spawn_y_max=0`.
- PowerShell verification returned `stage2Rows=30 badNormal=0 badBoss=0` for Stage 2 spawn coordinates and `missingEncounterDays=0 missingEnemyRefs=0 missingDayEncounterRefs=0 missingRewardRefs=0` for StageDay/StageEncounter/StageReward/enemy references.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitRuntimeModel.cs`, `UnitFactory.cs`, and `EnemyCombatSystem.cs` now support `FireDamageUp`, `LightningDamageUp`, `IceDamageUp`, `DarknessDamageUp`, `HolyDamageUp`, plus matching attribute-defense passive IDs.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity `Pakuri/Validate CSV Source Data` loaded 8 stage-one enemies and 8 stage-two enemies.

### History

- 2026-05-31: Earlier inspection showed only broad `DefenseUp` and `PhysicalDamageUp` worked for enemy passives.
- 2026-05-31: Code Builder added the attribute-specific enemy passive runtime path and Stage 2 enemy lookup path.
- 2026-05-31: Code Builder normalized Stage 2 `StageEncounter.csv` spawn coordinates to the Stage 1 coordinate pattern after the user reported abnormal Stage 2 enemy spawn positions.

## Task: 2026-05-31 Stage2 Enemy Data-Only Source CSV

### Task title

Record the Stage 2 enemy source rows as data-only enemy-domain groundwork.

### Goals

- Preserve the current Stage 1 runtime enemy path while adding a separate Stage 2 source CSV for future work.
- Keep Stage 2 rows aligned to the existing enemy row shape so later runtime connection can compare against the current Stage 1 contract.
- Avoid changing enemy combat, spawn, skill enum, or prefab behavior in this data-only step.

### Constraints

- Role Owner is Code Builder.
- The new file is not connected to `PakuriCsvRuntimeData`, `EnemySpawnManger`, `EffectManager`, `StageEncounter.csv`, or scene prefabs.
- `stage_one_skill`, `basic_skill`, `passive_skill_name`, `passive_skill_id`, and `passive_skill_value` are placeholders copied from Stage 1 by row order as requested.

### Role Owner

Code Builder

### Status

Data-only CSV created and shape-verified.

### Next Actions

- Future runtime Stage 2 work must add a real Stage 2 load/spawn/encounter path before these rows can affect gameplay.
- Future skill work must replace or generalize the Stage 1 skill placeholders if Stage 2 enemy skills should execute their authored reference behavior.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv:3` through `:10` contain the eight Stage 2 enemy rows: fire, lightning, ice, darkness, holy, Ethan, Drake, and Arsen.
- `Pakuri/reference/5.enemy/stage-2-enemies.md` supplied the Stage 2 names, roles, attack types, attributes, stats, defenses, and passive summaries.
- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` supplied the header/type rows and the requested copied Stage 1 skill/passive columns.
- PowerShell field-count verification returned `header=26 rows=10 bad=`.
- PowerShell comparison against `stage_one_enemies.csv` returned `copied=True` for all eight Stage 2 rows for the five requested copied columns.

### History

- 2026-05-31: User requested `stage_two_enemies.csv` as a runtime-unconnected source file using the same shape as `stage_one_enemies.csv`.
- 2026-05-31: Code Builder created the data-only CSV and left runtime enemy behavior unchanged.

## Task: 2026-05-28 Shared Trigger LineAttack Direct Execution

### Task title

Add an explicit shared trigger `LineAttack` execution path so delayed follow-up slashes can reuse beam/line aiming and linked OnHit status payloads without re-casting a helper skill.

### Goals

- Let trigger rows directly execute a shared `LineAttack` with runtime-authored damage, width, prefab, and status payload.
- Keep base aimed-slash presentation consistent between direct skills and delayed trigger follow-ups.
- Avoid recursive same-skill re-cast behavior on `OnSkillCast` trigger chains.

### Constraints

- Role Owner is Code Builder.
- The shared runtime was extended only for explicit `trigger_action=LineAttack`; existing trigger rows do not auto-switch behavior just because `runtime_kind=LineAttack`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Reuse explicit `trigger_action=LineAttack` only when a trigger must spawn a direct delayed line slash rather than re-cast a learned skill runtime.
- If a future skill needs original-cast target locking instead of delayed nearest-target resolution, extend trigger context separately instead of overloading this direct line path.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now includes `SkillTriggerActionKind.LineAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now switches on `SkillTriggerActionKind.LineAttack` and executes a shared `ExecuteLineAttack(...)` path instead of routing only through `TriggeredSkill` or `SingleAttack`.
- The same runtime file keeps `ResolveTriggerAction(...)` conservative, so only explicit `trigger_action=LineAttack` rows use the new direct path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` now applies linked OnHit effects through `TryApplyOnHitEffects(..., SkillExecutionSnapshot, ...)`, which preserves snapshot-resolved status specs on the shared line actor path.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now validates explicit trigger `LineAttack` damage rows with the same positive payload rule used for trigger `SingleAttack`: positive `base_damage` or positive `attack/spell` coefficient for `Fixed` damage source.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` completed and the console logged the runtime catalog load summary without a Pakuri CSV failure.

### History

- 2026-05-28: Vega-B master-1 follow-up originally lived on the shared triggered `SingleAttack` path; after the user asked for the same aimed slash behavior as base `vega-b`, Builder added a direct trigger `LineAttack` action instead of re-casting the base skill.

## Task: 2026-05-28 Triggered SingleAttack Damage Payload Contract Fix

### Task title

Correct the shared trigger `SingleAttack` damage-payload contract after Vega-B follow-up validation exposed a mismatch between authored rows and runtime expectations.

### Goals

- Keep triggered `SingleAttack` follow-ups on the shared combat runtime path.
- Ensure follow-up trigger rows carry concrete damage payload values instead of relying on `damage_multiplier` alone.
- Keep source validation and runtime behavior consistent for future trigger-routed slashes.

### Constraints

- Role Owner is Code Builder.
- This task updates shared validation behavior and one authored trigger row; no new trigger action kind was introduced.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Future triggered `SingleAttack` follow-ups should copy or derive a real payload into the trigger row before applying a scaling multiplier.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now accepts `Fixed` trigger `SingleAttack` rows when they have positive `base_damage` or positive `attack/spell` coefficient, matching the runtime `ResolveDamage(...)` contract.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now gives `vega-b-master1-second-slash` concrete payload values `base_damage=30`, `attack_power_coefficient=1.4`, and `damage_multiplier=0.45`.
- Unity menu `Pakuri/Validate CSV Source Data` completed successfully, and the console logged `PakuriCsvRuntimeData loaded runtime catalog ...` instead of the earlier `vega-b-master1-second-slash` validation error.

### History

- 2026-05-28: The first Vega-B follow-up trigger authoring incorrectly treated `damage_multiplier` as if it reused the source skill damage payload automatically.

## Task: 2026-05-28 Triggered SingleAttack OnHit Status Payload

### Task title

Let shared triggered `SingleAttack` actions apply linked `OnHit` status effects with source-skill choice modifiers.

### Goals

- Reuse `monster_skill_triger.csv` `SingleAttack` rows for delayed follow-up slashes that must damage and inflict status together.
- Keep the payload on the shared trigger/effect path instead of adding a hidden helper skill runtime.
- Preserve source-skill choice gates and status-duration bonuses on the triggered status application.

### Constraints

- Role Owner is Code Builder.
- This is a shared combat-runtime extension, not Vega-only hardcoding.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Reuse `triggered_effect_id` with `effect_kind=Status` and `effect_timing=OnHit` when future triggered `SingleAttack` follow-ups need status payloads.
- Keep source-skill duration bonuses on this path by resolving the triggered OnHit status through the source-skill active-choice snapshot.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now resolves a linked `OnHit` status effect for triggered `SingleAttack`, passes it through prefab-hitbox and area-hit routing, and applies it after each shared damage call.
- The same runtime file now builds a source-skill active-choice snapshot for triggered OnHit status resolution, so shared checks such as `requires_active_choice_id` and `status_duration_bonus_status_id` are honored on the follow-up hit.
- The same runtime file now applies the triggered OnHit status even on the radius-0 single-target fallback branch, closing the last direct-hit gap inside the shared trigger path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the pre-existing `MSB3277` warnings remained.

### History

- 2026-05-28: Vega-B master-1 required a delayed second slash that deals damage and also silences the hit targets; the previous shared trigger `SingleAttack` path could deal damage but could not carry a linked OnHit status payload.

## Task: 2026-05-26 SingleAttack CSV Damage Delay Runtime

### Task title

Add CSV-authored delayed damage timing and animation-length visual lifetime for SingleAttack.

### Goals

- Let each `SingleAttack` row tune hit timing with `damage_delay_seconds`, defaulting existing rows to `0`.
- Keep visual prefabs spawning immediately while delaying only damage/status/on-hit follow-up resolution.
- Destroy SingleAttack visual/hitbox prefabs by animation clip length instead of the previous fixed `1f` lifetime.

### Constraints

- Role Owner is Code Builder.
- This is a shared SingleAttack runtime extension, not monster-specific hardcoding.
- Existing rows keep `damage_delay_seconds=0` for immediate-hit compatibility.
- Unity Play Mode gameplay verification remains user-owned.
- Unity batchmode CSV runtime sync was attempted but blocked because the project is already open in another Unity instance.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. CSV runtime catalog asset sync remains pending in the open Unity Editor or by rerunning `SyncCsvRuntimeCatalogs.bat` after closing the project.

### Next Actions

- User sets nonzero `damage_delay_seconds` values on desired `SingleAttack` rows and verifies hit feel in Play Mode.
- Run `Pakuri/Sync CSV Runtime Catalog Assets` in the open Unity Editor, or close Unity and rerun `SyncCsvRuntimeCatalogs.bat`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` now exposes `DamageDelaySeconds` with default `0`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now reads `skill.DamageDelaySeconds`, schedules delayed hit resolution through coroutines, and keeps immediate behavior when the value is `0`.
- `SingleAttackSkillExecutor.cs` now delays `UsePrefabHitbox` damage until after `WaitForSeconds(...)`, then calls `Physics2D.SyncTransforms()` before `ApplyPrefabHitbox(...)`.
- `SingleAttackSkillExecutor.cs` now resolves visual lifetime from child `Animator` / legacy `Animation` clip lengths and falls back to `1f` only when no animation length exists.
- CSV parser verification returned `records=52`, `fields=56 records=52`, `damage_delay_index=50`, `type=float`, and `nonzero_defaults=0` for `monster_skills.csv`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed with Unity's duplicate-project-open guard: another Unity instance has `C:/TowerDefence_Pakuri/Test/Pakuri` open.

### History

- 2026-05-26: User chose the N-second delayed damage approach and requested Code Builder implementation with default value `0` plus animation-length prefab deletion.

## Task: 2026-05-24 Shared Skill On-Hit Additional Damage Runtime

### Task title

Add a reusable skill hit rider for direct extra damage and every-nth-hit chain damage.

### Goals

- Let skill choices add immediate extra damage to the actual hit target without using `SingleAttack` as a fake triggered skill.
- Let skill choices count primary hits and run deterministic chain damage every nth hit.
- Apply the shared option from projectile, beam, zone, and single-attack hit paths.

### Constraints

- Role Owner is Code Builder.
- This is a shared runtime extension, not Rin-only hardcoding.
- Added damage calls are guarded through the shared helper so additional damage does not recursively invoke itself.
- On-hit extra and chain damage must not dispatch outgoing-damage triggers again; they are rider damage attached to the primary hit, not new trigger roots.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that direct on-hit extra damage and every-third-hit chain damage feel correct under real combat timing.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now stores `SkillHitCount` and exposes `AdvanceSkillHitCount()`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillOnHitAdditionalDamageUtility.cs` owns the shared on-hit extra damage and chain target selection.
- `InGameProjectileActor`, `InGameLineAttackActor`, `InGameZoneSkillActor`, `ProjectileSkillExecutor`, and `SingleAttackSkillExecutor` now call the shared helper after primary hit damage/status resolution.
- `SkillOnHitAdditionalDamageUtility` uses an in-helper execution flag so additional damage calls do not recursively invoke the same on-hit additional damage path.
- `SkillOnHitAdditionalDamageUtility.cs:69` and `SkillOnHitAdditionalDamageUtility.cs:110` pass `suppressOutgoingDamageTriggers: true` through `InGameCombatManager.ApplyDamage`, so direct extra and chain rider damage do not fan out through `OnOutgoingDamage` triggers.
- `InGameCombatManager.cs:388` returns before `SkillTriggerRuntime.ExecuteOutgoingDamage` when `DamageApplicationOptions.SuppressOutgoingDamageTriggers` is set.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity console after sync/validation showed only MCP client handler connection exceptions as errors, not C# compile errors.

### History

- 2026-05-24: User asked to generalize the Rin-A master-2 idea into a skill on-hit option usable by projectile, zone, single-attack, and beam skills.
- 2026-05-24: User chose nth-hit as the chain criterion and reported direct 40% Lightning rider damage behaving like all-target damage. Code inspection showed hit-target damage was target-only, but it used the normal outgoing-damage trigger path; rider damage now suppresses outgoing-damage trigger dispatch.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/ENEMY_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/enemy history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current Stage 1 enemy runtime authority and verification baseline.

## Task: 2026-05-24 Projectile Nth Launch Branch Runtime Extension

### Task title

Add a reusable projectile launch counter path for nth-launch branch chance overrides.

### Goals

- Let projectile choices express branch chance overrides on every nth base projectile launch.
- Support `Rin-a` master-2 style "every 3rd launched projectile branches at 100%" without hardcoding monster or skill IDs.
- Preserve existing branch-on-hit behavior for choices that only use branch chance/count/damage/search fields.

### Constraints

- Role Owner is Code Builder for the shared runtime extension and Skill Builder for future skill row application.
- This task changed shared runtime/data definitions only; no skill CSV row values, prefab assets, or scene objects were edited.
- Skill Builder did not inspect current Rin CSV rows because the user did not explicitly authorize current CSV/code discovery as parsed source.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder / Skill Builder

### Status

Shared runtime extension implemented and compile-verified. Specific `Rin-a` enhancement/master rows still require parsed input or explicit CSV discovery authorization before Skill Builder can edit data.

### Next Actions

- To apply `Rin-a` master-2 through data, provide the parsed row bundle or explicitly authorize Skill Builder to use current CSV/code as the parsed source.
- Required master-2 parsed values include target skill/choice identity plus `branch_launch_period=3`, `branch_launch_chance_set=1`, and any intended branch count/damage/search overrides.
- Remaining `Rin-a` enhancements and master-1 need their parsed effect values before implementation.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now stores `ProjectileLaunchCount` and exposes `AdvanceProjectileLaunchCount()`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now advances the launch count for each instantiated base projectile and resolves branch chance per projectile launch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs`, `SkillChoiceEffectSpec.cs`, `SkillChoiceModifierRecord.cs`, and `SkillChoiceDefinition` now carry `BranchLaunchPeriod` plus `BranchLaunchChanceSet`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` can read optional `branch_launch_period` and `branch_launch_chance_set` columns when present.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.CsvSupport.cs` now exposes `HasColumn(...)` so optional new columns do not break older CSV tables that do not contain them.
- `boards/SkillBluePrint/projectile-blueprint.md` now treats nth-launch branch chance override as part of the current common projectile path when parsed fields are provided.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; the same existing `MSB3277` warnings remained.
- Unity-MCP script refresh recovered after domain reload and returned ready; console warning/error read showed MCP client handler connection logs, not C# compile errors.

### History

- 2026-05-24: User approved the reusable shared extension where projectile launch count, not hit count, drives every nth launch branch behavior.

## Task: 2026-05-24 Skill Executor File Boundary Split

### Task title

Split the monolithic skill executor source into responsibility-specific execution files.

### Goals

- Remove the oversized `SkillExecutors.cs` owner and keep each executor in its own source file.
- Preserve the existing `SkillExecutorRegistry` type-based routing and runtime behavior.
- Move shared status-spec construction out of `ProjectileSkillExecutor` so beam, zone, single-attack, buff, and shield executors do not depend on the projectile executor for common status work.

### Constraints

- Role Owner is Code Builder.
- This is a behavior-preserving refactor only; no CSV rows, prefab assets, scene objects, runtime tuning, or skill contracts were intentionally changed.
- Unity Play Mode gameplay verification remains user-owned.
- Existing unrelated workspace changes under Rin assets and CSVRuntime assets were not touched by this task.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode only if they want gameplay confirmation after the source-file split.
- Future skill executor changes should edit the narrow executor file or common utility file instead of reintroducing a combined `SkillExecutors.cs`.

### Evidence

- The previous `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `.meta` were removed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/TypedSkillExecutor.cs`, `ProjectileSkillExecutor.cs`, `BeamSkillExecutor.cs`, `ZoneSkillExecutor.cs`, `SingleAttackSkillExecutor.cs`, `BuffSkillExecutor.cs`, `ShieldSkillExecutor.cs`, `PassiveSkillExecutor.cs`, `SkillMultiEffectExecutor.cs`, `SkillExecutionUtility.cs`, and `SkillStatusSpecUtility.cs` now hold the split responsibilities.
- `Pakuri/Assembly-CSharp.csproj` now includes the new split skill execution files and no longer includes `SkillExecutors.cs`.
- Search after the split found no remaining `ProjectileSkillExecutor.ResolveStatusSpec` or `ProjectileSkillExecutor.ResolveStatusData` callers.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` `System.Net.Http` / `System.IO.Compression` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; the same existing `MSB3277` warnings remained.
- Unity-MCP force refresh and script compilation returned the editor to idle; console error read showed only MCP client handler logs, not C# compile errors.

### History

- 2026-05-24: Code Builder split the monolithic skill executor file into executor-specific files plus common status, multi-effect, and execution utility files while preserving registry routing.

## Task: 2026-05-24 Skill Execution Folder Role Organization

### Task title

Organize skill execution scripts into role-specific subfolders and consolidate tiny support files.

### Goals

- Move the current `Execution` root scripts into responsibility folders so the folder layout matches runtime ownership.
- Consolidate tiny contract/model/support executor files without changing public type names or namespaces.
- Preserve MonoBehaviour file names and moved `.meta` files for actor/UI scripts.

### Constraints

- Role Owner is Code Builder.
- This is a behavior-preserving structure refactor only; no CSV rows, prefabs, scenes, skill tuning, public type names, or namespaces were intentionally changed.
- Unity Play Mode gameplay verification remains user-owned.
- Existing unrelated workspace changes under Rin assets and CSVRuntime assets were not touched by this task.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode only if they want runtime gameplay confirmation after the folder move.
- Future skill execution code should be added under the matching folder: `Executors`, `Actors`, `Runtime`, `Utilities`, `Contracts`, `Modifiers`, or `UI`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution` now has no root `.cs` files; `.cs` files are under `Actors`, `Contracts`, `Executors`, `Modifiers`, `Runtime`, `UI`, and `Utilities`.
- `SkillExecutionContext`, `SkillExecutionResult`, and `SkillExecutionStatus` are now in `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Contracts/SkillExecutionModels.cs`.
- `IInGameSkillExecutor` and `TypedSkillExecutor<TSkillData>` are now in `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Contracts/SkillExecutorContracts.cs`.
- `BuffSkillExecutor`, `ShieldSkillExecutor`, and `PassiveSkillExecutor` are now in `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SupportSkillExecutors.cs`.
- `Pakuri/Assembly-CSharp.csproj` now includes the new role-folder paths and no longer includes the removed root-level contract/model/support executor files.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` `System.Net.Http` / `System.IO.Compression` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; the same existing `MSB3277` warnings remained.
- Unity-MCP script refresh returned to idle; console warning/error read showed only MCP client handler connection logs, not C# compile errors.

### History

- 2026-05-24: Code Builder moved skill execution source files into role folders and consolidated the tiny contract/model/support executor files while preserving type names and namespaces.

## Task: 2026-05-24 Shared Skill Executor Helper Consolidation

### Task title

Consolidate duplicated shared skill-execution helpers and remove scene-name hardcoding from prefab-hitbox center resolution.

### Goals

- Keep sibling skill executors using one shared implementation for rotation, ordered target resolution, and prefab hitbox scaling.
- Remove the `GameObject.Find("SkillPoint")` scene dependency from `SingleAttackSkillExecutor` so `HitAllTargets` prefab-hitbox skills resolve from runtime combat context instead of a hardcoded scene object name.
- Preserve current runtime behavior contracts and serialized data compatibility while reducing duplication.

### Constraints

- Role Owner is Code Builder.
- This task is a behavior-preserving refactor of shared combat runtime code; it does not change CSV rows, prefab assets, or scene serialization.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that `HitAllTargets` prefab-hitbox skills still center correctly during real combat after the `SkillPoint` fallback removal.
- If another executor needs target ordering or prefab scaling, route it through `SkillExecutionUtility` instead of reintroducing local duplicates.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:2526` now owns shared `ResolveRotation(...)`, `ApplyPrefabScale(...)`, `ResolveOrderedTargets(...)`, and `ResolveTargetGroupCenter(...)` helpers inside `SkillExecutionUtility`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:140`, `:533`, `:739`, `:827`, `:1097`, `:1212`, `:1266`, and `:1312` now route projectile, beam, zone, and single-attack executor paths through those shared helpers instead of local duplicates.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1053` and `:1057` now resolve `HitAllTargets` prefab-hitbox centers through `ResolveTargetGroupCenter(...)`; the previous `GameObject.Find("SkillPoint")` scene-name lookup is no longer present in the file.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` assembly-version warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; the same existing `MSB3277` warnings remained.

### History

- 2026-05-24: Code Builder consolidated duplicated helper logic from `ProjectileSkillExecutor`, `BeamSkillExecutor`, `ZoneSkillExecutor`, and `SingleAttackSkillExecutor` into `SkillExecutionUtility`, then replaced the hidden `SkillPoint` scene dependency with target-group-center resolution from runtime context.

## Task: 2026-05-18 Stage1 Enemy Runtime Authority

### Task title

Keep the current Stage 1 enemy runtime grounded in the active CSV-plus-scene authority split.

### Goals

- Keep Stage 1 enemy composition driven by `StageEncounter.csv`.
- Keep Stage 1 enemy skill tuning driven by `stage_one_enemies.csv`, `EnemySkillData.csv`, and the runtime enemy model path.
- Keep enemy skill visual prefabs scene-authored through `EffectManager` in `NewRunScene`.
- Keep one basic skill plus one special skill per enemy in the current combat simulation.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed implementation history before this cleanup is preserved in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active enemy runtime state summarized and retained for future work. 2026-05-18 Code Builder refactor keeps behavior in the same runtime path while now co-locating enemy skill execution with the enemy combat loop. 2026-05-18 follow-up then absorbed the former cooldown helper into the same owner and renamed that owner to `EnemyCombatSystem.cs`.

### Next Actions

- User verifies real in-game cadence, priority, and feel for dual-skill enemies in `NewRunScene`.
- If enemy behavior changes again, update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/RUN/RUN_BLACKBOARD.md`.
- Use the archive snapshot when older MVP or intermediate spawn-sequence history is needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` now owns enemy tick orchestration, basic/special skill resolution, attack range, support-skill readiness, cooldown ticking, temporary enemy modifier ticking, the integrated resolved-skill contract types, and the integrated `EnemySkillExecutor` helper for enemy skill execution and visual/effect dispatch.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyTargeting.cs` still owns nearest-player target lookup and enemy-ally support target lookup.
- `EnemyCombatState` stores separate `BasicSkillCooldownRemaining` and `SpecialSkillCooldownRemaining`.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` and `Assets/Scenes/NewScene/NewRunScene.unity` own enemy skill visual prefab mappings.
- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` carries the current `basic_skill` plus `stage_one_skill` authored split.
- `Pakuri/Assets/CSVdata/EnemySkillData.csv` carries active Stage 1 skill tuning rows.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` carries the current Stage 1 encounter composition rows used by the stage flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.

### History

- 2026-05-16: Stage encounter CSV seeding and StageManager-driven spawn wiring were recorded.
- 2026-05-17: Enemy skill tuning was split out of enemy rows into `EnemySkillData.csv`.
- 2026-05-18: Dual-skill enemy runtime and scene-owned effect authority became the active baseline.
- 2026-05-18: Code Builder split `EnemyCombatSimulationSystem` into orchestration, cooldown, targeting, execution, and shared runtime-data files.
- 2026-05-18: Code Builder later merged `EnemySkillExecutor.cs` into `EnemyCombatSimulationSystem.cs` and merged `EnemySkillRuntime.cs` into `EnemySkillCooldown.cs` during the repository-wide high-integration consolidation pass.
- 2026-05-18: Code Builder then absorbed `EnemySkillCooldown.cs` into the renamed `EnemyCombatSystem.cs` owner and also absorbed `CombatStatModels.cs` into `DamageCalculator.cs`.
