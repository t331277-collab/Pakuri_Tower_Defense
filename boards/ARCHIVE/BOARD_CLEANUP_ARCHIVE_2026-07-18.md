# Board Cleanup Archive 2026-07-18

Moved from active boards under COMBAT, DATA, MON, OPS, RUN, and UI during the 2026-07-18 cleanup.
Criteria: keep only task blocks explicitly dated in 2026-07 in active boards. Archive every earlier dated task and every undated task.
The standalone undated damage meter handoff was normalized into the required task-block format and archived from its former UI path.

- Kept active task blocks: 116
- Archived task blocks: 180
- Source board files with archived task blocks: 18

## Source: boards\COMBAT\ENEMY_BLACKBOARD.md

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
- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillData.csv` no longer contains `range`.
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
- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillData.csv` currently stores enemy skill body fields such as `runtime_kind`, coefficients, cooldown, radius, projectile speed/lifetime, duration, flat value, movement multiplier, and outgoing damage multiplier.
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

## Source: boards\COMBAT\PROJECTILE_BLACKBOARD.md

## Task: 2026-05-27 Shared Projectile Delayed-Impact And Timed Follow-Up Runtime

### Task title

Extend the shared projectile runtime to support contact-stop delayed impacts plus on-hit and on-expire follow-up effects.

### Goals

- Let projectile skills stop on first contact, wait a configured delay, then resolve a delayed area impact.
- Let projectile skills run shared `monster_skill_effects.csv` `OnHit` and `OnExpire` rows without monster-specific executor branches.
- Preserve existing direct-hit fallback behavior for simple projectiles that do not need delayed impact or timed effects.

### Constraints

- Role Owner is Code Builder.
- This is a shared projectile/runtime extension, not a Sein-only branch.
- The new behavior is only exercised when projectile/effect data asks for delayed impact or timed effects.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- Reuse the delayed-impact path for future projectile skills before adding another projectile state machine.
- Keep projectile visuals scene-owned through `EffectManager` when a flying visual is distinct from the delayed impact visual.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs` now stores delayed-impact state (`stopAfterFirstHit`, `impactDelaySeconds`, `impactEffectPrefab`, `hasImpactArea`, `onHitEffects`, and `onExpireEffects`) and resolves contact-stop delayed impacts through the shared actor.
- The same file now defers destroy-boundary cleanup while armed, executes `OnHit` follow-up effects on contact, resolves delayed explosion damage through `InGameZoneSkillActor.ApplyAreaTick(...)`, and waits for spawned impact visual lifetime before `OnExpire` follow-up effects.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now resolves projectile timed effects from `SkillMultiEffectTiming.OnHit` and `OnExpire`, prefers explicit projectile prefab then scene `EffectManager` mapping for the flying visual, and creates a projectile actor instead of using direct-hit fallback when delayed-impact behavior exists.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillVisualSpawnUtility.cs` now exposes shared animation-length visual lifetime resolution used by projectile delayed-impact follow-up cleanup.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDataValidator.cs` now allows `CooldownProjectile` rows that rely on cooldown timing instead of magazine/reload fields while still validating projectile speed.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-05-27: Sein-C required a shared projectile path for “flying arrow -> first-contact stop -> delayed explosion -> optional residual zone,” so the projectile actor and executor were extended instead of adding a Sein-only implementation.

## Source: boards\COMBAT\STATUS_EFFECT_BLACKBOARD.md

## Task: 2026-06-19 Ariel Plan Action Status Modifier Routing

### Task title

Apply Ariel status-related choice modifiers through plan action handlers instead of old choice spec folding.

### Goals

- Route Ariel status modifier nodes such as Holy damage taken and critical damage taken through `SkillActionOp`.
- Keep Ariel A master2 holy exposure application as a trigger-bound status effect on the hit event target.
- Preserve non-Ariel old wide status behavior compatibility.

### Constraints

- Role Owner is Code Builder / Code Reviewer.
- No Ariel-only status storage was added.
- Trigger/effect status application remains explicit CSV runtime behavior in this pass.

### Role Owner

Code Builder / Code Reviewer

### Status

Implemented and reviewed for Ariel-first scope.

### Next Actions

- User verifies the live status effects in Play Mode.
- Future status modifier migrations should prefer normalized nodes before adding new wide status columns.

### Evidence

- `SkillExecutionSnapshot.cs` has `SkillActionOpKind.StatusElementDamageTakenBonus` and `SkillActionOpKind.StatusCriticalDamageTakenBonus` handling inside `ApplyPlanAction(...)`.
- `InGameSkillDefinitionMapper.cs` maps `StatusElementDamageTakenBonus` and `StatusCriticalDamageTakenBonus` handlers into `SkillActionOp`.
- `monster_skill_nodes.csv` keeps `ariel-a-master-2-holy-exposure-element-damage-taken` with handler `StatusElementDamageTakenBonus` and `bonus=0.15`.
- `monster_skill_nodes.csv` keeps `ariel-d-master-1-status-critical-damage-taken` with handler `StatusCriticalDamageTakenBonus` and `bonus=0.25`.
- `monster_skill_triger.csv` / `monster_skill_effects.csv` keep Ariel A master2 status application as `OnOutgoingDamage` + `EventTarget` + `Status` effect with `status_effect_id=holy-exposure`.
- Runtime/editor builds passed with 0 errors, and Unity-MCP InGame skill validation passed with 0 warning(s).

### History

- 2026-06-19: User requested target-structure migration and Reviewer verification after asking whether Ariel A master2 should be trigger/event-target/apply-status/holy-exposure.

## Task: 2026-06-19 Trigger-Bound EventTarget Status Application Fix

### Task title

Allow trigger-bound effect rows to apply statuses to the hit event target.

### Goals

- Support Ariel A master2 holy exposure as a trigger-bound status effect.
- Preserve shared trigger/effect runtime behavior without adding Ariel-only branches.
- Keep status modifier values data-authored through normalized nodes.

### Constraints

- Role Owner is Code Builder.
- Current trigger event used for hit-success routing is `OnOutgoingDamage`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, and Unity-MCP validated.

### Next Actions

- User verifies Ariel A master2 applies `holy-exposure` to the actual hit enemy in Play Mode.
- Reuse trigger-bound `Effect` + `EventTarget` for future on-hit status applications.

### Evidence

- `SkillTriggerRuntime.ExecuteEffect(...)` now constructs `SkillExecutionContext` with `triggerContext.EventTarget`, enabling `SkillMultiEffectExecutor` to resolve `target_selection=EventTarget`.
- `monster_skill_triger.csv` now binds `ariel-a-master2-holy-exposure-on-hit` to `trigger_event=OnOutgoingDamage`, `target_selection=EventTarget`, and `trigger_action=Effect`.
- `monster_skill_effects.csv` now applies `status_effect_id=holy-exposure` through `ariel-a-master-2-holy-exposure-on-hit`.
- `monster_skill_nodes.csv` / `monster_skill_node_params.csv` add `StatusElementDamageTakenBonus` `bonus=0.15` for the applied holy exposure status.
- Runtime and editor dotnet builds passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP validation logged `InGame skill data validation passed with 0 warning(s)` and warning/error console read returned 0 entries.

### History

- 2026-06-19: User confirmed the intended design as hit trigger, event target, apply status, and `status_id=holy-exposure`; Builder implemented the current runtime equivalent using `OnOutgoingDamage`.

## Task: 2026-06-19 Ariel Passive Status Modifier Node Decomposition

### Task title

Route Ariel passive status modifier add-ons through normalized choice nodes.

### Goals

- Let passive status effects receive additive modifier-node values for damage bonus, shield received, critical chance, damage taken, and flat element resist reduction.
- Ensure Ariel F/G/H/I/J passive numeric upgrades compose onto base status effect objects instead of relying on duplicate status effect rows.

### Constraints

- Role Owner is Code Builder.
- Shared status runtime behavior remains data-driven; no Ariel-only status branch was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, and Unity-MCP validated.

### Next Actions

- User verifies Ariel passive status modifier stacking in Play Mode.
- Future status modifier nodes should remain additive unless a handoff explicitly asks for replacement semantics.

### Evidence

- `SkillStatusSpecUtility.ResolveStatusData(...)` now applies normalized choice status modifiers additively to `StatusEffectData` fields and `BuffModifierSpec` fields.
- `SkillExecutionSnapshot` now carries `StatusDamageBonusRate`, `StatusShieldReceivedBonus`, `StatusCriticalChanceBonus`, `StatusDamageTakenBonus`, and `StatusFlatElementResistReduction`.
- Existing status element/critical/ailment modifier choice values now accumulate onto base status data instead of replacing base values.
- Ariel F trait1, G trait1/G trait2, H trait1/H trait2, I trait1, J trait1, and J trait2 are now represented by normalized status modifier nodes.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP validation logged `InGame skill data validation passed with 0 warning(s)` and the warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested Code Builder to decompose all Ariel skills like Ariel C using atomic effect object + modifier node + binding node.

## Task: 2026-06-19 Ariel Effect Object Trigger Binding Handoff

### Task title

Document how Ariel A-J can move from pre-combined CSV rows to skill body plus small effect objects, trigger bindings, and conditional modifiers.

### Goals

- Preserve the current runtime evidence that Ariel already has base skill rows, effect rows, trigger rows, and a small normalized node start.
- Define a migration handoff that reduces Ariel C-style row explosion.
- Explain how the old structure remains compatibility input until parity is verified.

### Constraints

- Role Owner started as Designer and continued as Code Builder after user explicitly requested implementation.
- The implementation uses generic `monster_skill_nodes.csv` and `monster_skill_node_params.csv`; no specialized effect object CSV files were added in this pass.
- User resolved the six ambiguous design questions before implementation.
- Unity Play Mode parity remains user-owned.

### Role Owner

Designer / Code Builder

### Status

Code Builder pass implemented normalized node handler expansion, Ariel numeric choice node migration, and Ariel C blessing row-explosion reduction.

### Next Actions

- User Play Mode verifies Ariel C combinations, Ariel B shield events, Ariel E shield composition, and Ariel J post-E / Ariel-E-shield-only behavior.
- Code Reviewer pass is pending after the Phase 2-5 implementation.

### Evidence

- `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md` was created.
- `Pakuri/reference/2.Monster/ariel/skill/` contains the inspected Ariel A-J reference markdown files.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skills.csv` contains Ariel base rows `ariel-a` through `ariel-j`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_effects.csv` contains Ariel effect rows including pre-combined Ariel C blessing rows.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_triger.csv` contains Ariel trigger rows for last projectile, shield expire, shield absorb, and status expire behavior.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` owns current multi-effect execution for damage, status, and status-duration extension.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` owns current combat trigger dispatch.
- User answers recorded in the handoff: D trait 5 requires the attacker itself to have shield; J shield condition requires Ariel-E-generated shield; I holy exposure damage taken applies to all incoming damage while exposure exists; passives are always active; durations stay seconds; generic node CSVs are the storage path.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now registers reusable handlers including `CountStatusDamageMultiplier`, `MagazineBonus`, `ReloadTimeMultiplier`, `PierceBonus`, `DurationBonus`, `StatusActionSpeedBonus`, `StatusAilmentResistanceBonus`, `StatusConditionalDamageTakenBonus`, and `StatusElementDamageTakenBonus`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now applies normalized choice nodes on the combat snapshot path and supports status-targeted action speed bonuses.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now resolves status snapshot overrides through `SkillStatusSpecUtility.ResolveStatusData(...)`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_nodes.csv` now contains Ariel choice-owned normalized node rows for migrated numeric modifiers and `ariel-c-trait-2-blessing-action-speed`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_effects.csv` has 9 Ariel C pre-combined blessing rows disabled as `MigratedToEffectBinding`; the base rows now compose with normalized choice nodes.
- Phase 2-5 added `ShieldAmountMultiplier` so shield amount choices can avoid reusing generic damage multipliers when damage and shield behavior diverge.
- `SkillMultiEffectExecutor.ResolveStatusEffectShieldAmount(...)` now receives the combat snapshot and applies the shield-specific multiplier to status-effect shield amounts.
- `StatusEffectRuntime.MatchesConditionStatus(...)` now supports an optional required source skill id for effect condition checks.
- `ariel-j-shielded-holy-damage` now uses `condition_status_source_skill_id=ariel-e-shield-base`, matching the source id stored by the Ariel E shield effect status.
- `monster_skill_triger.csv` now keeps A last-shot, B shield-expire, B shield-absorb, and D mark-expire trigger rows as explicit runtime trigger-binding compatibility rows.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings for `System.Net.Http` and `System.IO.Compression` remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` completed; console logged runtime catalog load and `InGame skill data validation passed with 0 warning(s).`

### History

- 2026-06-19: User asked how Ariel A-J would be decomposed into skill body, small effect objects, trigger bindings, and conditional modifiers, including runtime application and old-structure handling.
- 2026-06-19: User then requested Code Builder implementation and answered the ambiguity questions; Code Builder implemented the generic node handler expansion, migrated 28 Ariel numeric choice modifiers into normalized nodes, added the Ariel C trait2 targeted blessing node, and disabled 9 Ariel C pre-combined rows.
- 2026-06-19: User requested the remaining Phase 2-5 implementation; Code Builder added shield-specific multiplier support, E shield row reduction, J-owned post-E triggers, and source-specific effect conditions.

## Task: 2026-05-31 Nexus Exclusion From Skill And Status Targets

### Task title

Keep Nexus as a damageable enemy fallback target while excluding it from player skill, buff, shield, heal, and status target paths.

### Goals

- Preserve Nexus in the combat roster so enemies can attack it after monsters are gone.
- Prevent allied skills, buffs, shields, heals, status application, status-count targeting, and chained additional damage from selecting Nexus.
- Keep direct damage against Nexus allowed.

### Constraints

- Role Owner is Code Builder.
- Nexus remains registered as player-side `UnitRole.Nexus`; filtering happens in skill/status paths, not by removing it from the roster.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Unity-MCP validator could not run because no Unity Editor instance was found.

### Next Actions

- User verifies in Play Mode that Nexus HP can still be damaged by enemies, but Monster buffs/skills no longer apply to Nexus.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs` now filters `UnitRole.Nexus` from resolved skill target lists, including `Self`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now filters `UnitRole.Nexus` from status-count target resolution.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` already guards Nexus from `GrantShield`, `SetShield`, `Heal`, `ApplyStatus`, `ApplyShieldStatus`, and `ExtendStatusDuration`, while `ApplyDamage` remains allowed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` already filters Nexus from all-allies cooldown target entries.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillOnHitAdditionalDamageUtility.cs` already skips Nexus as a chain target.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- Unity-MCP `validate_script` returned `No Unity Editor instances found`, so Unity-side validation was not available in this session.

### History

- 2026-05-31: User reported Monster buffs appearing to apply to Nexus and clarified Nexus should only take damage, not be a skill/buff target.
- 2026-05-31: Code Builder verified Nexus is registered in the player roster for enemy fallback targeting, then tightened skill/status target filters instead of unregistering Nexus.

## Task: 2026-05-31 Shared Passive Aura, Runtime-Kind Filter, Burst Status Hook, And All-Allies Cooldown Refund For Vega F-J

### Task title

Extend the shared passive/status runtime so Vega F-J can stay on reusable common logic for burst-index mark bonus, owner-status-gated aura behavior, area-only passive modifiers/triggers, and teamwide cooldown refund.

### Goals

- Keep Vega passive work on shared data-driven runtime contracts instead of Vega-only branches.
- Let passive effects and passive triggers require a live owner status.
- Let status-based damage modifiers and trigger events filter by skill runtime kind such as `Area`.
- Let passive-triggered cooldown refund iterate allied skill runtimes, not only the owner's single target skill.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The extension stays on shared status/runtime/trigger paths.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile/CSV-validation verified.

### Next Actions

- Reuse the burst-status choice path before adding another projectile-only trigger event for “Nth projectile adds stacks” behavior.
- Reuse `required_source_status_id` and runtime-kind filters for future “while buff X is active” or “Area damage only” passives before adding monster-specific branches.
- Reuse `TargetSide=AllAllies` on cooldown/reload trigger actions when future support skills need teamwide refund behavior.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now exposes the shared fields `EventSkillRuntimeKinds`, `StatusConditionalIncomingSkillRuntimeKinds`, `StatusConditionalOutgoingSkillRuntimeKinds`, `HasBurstStatusProjectileIndex`, `BurstStatusProjectileIndex`, and `BurstStatusStacksBonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.StatusPayload.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse/map/validate the new burst-status, owner-status gate, and runtime-kind filter fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carries burst-status bonus data, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now applies `ResolveBurstStatusStacksBonus(...)` on projectile hit.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/InGamePassiveEffectRuntime.cs` and `SkillMultiEffectExecutor.cs` now honor owner live-status gates on passive effects.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now filters incoming/outgoing modifiers through `MatchesSkillRuntimeKinds(...)`, which is the shared status-side `Area` filter used by Vega-I debuffs.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now filters passive-trigger events through `trigger.EventSkillRuntimeKinds`, routes direct effect rows through `SkillMultiEffectExecutor.ExecuteDirect(...)`, and resolves multi-target cooldown/reload operations through `ResolveTargetRuntimes(...)`, including `TargetSide=AllAllies`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now uses the burst-status path on `vega-f-trait-3`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now uses owner-status-gated aura rows on `vega-h-*` and `Area`-only incoming-damage rows on `vega-d-i-area-vulnerability-*`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now uses `event_skill_runtime_kinds=Area` on `vega-i-area-cooldown-base` and `TargetSide=AllAllies` cooldown refund rows on `vega-j-cooldown-base` and `vega-j-cooldown-trait1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged a successful sync after the final effect-schema normalization and refresh.

### History

- 2026-05-31: Designer's Vega F-J handoff narrowed the remaining blockers to burst-index status bonus, source-status-gated aura, runtime-kind filter, and multi-unit cooldown refund.
- 2026-05-31: Code Builder implemented those shared runtime contracts and Skill Builder authored Vega F-J on that path.
- 2026-05-31: Final Unity validation passed after the effect CSV header/type rows were normalized to match the widened shared status schema.

## Task: 2026-05-31 Shared SingleAttack HitAllTargets Origin Fix For Status-Filtered Fanout

### Task title

Fix the shared `SingleAttack` prefab-hitbox origin rule so status-filtered fanout skills can stay target-centered even when they also hit all local targets.

### Goals

- Keep caster-anchored `HitAllTargets` behavior for skills that are intentionally self-origin slashes.
- Prevent `HitAllTargets` from overriding the resolved deployment center on status-filtered fanout skills such as Vega-D.
- Preserve the current shared deployment-center, overlap, and repeat logic without adding a new executor mode.

### Constraints

- Role Owner is Code Builder.
- This task modifies only the shared hitbox-origin guard in `SingleAttackSkillExecutor.cs`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor-validated.

### Next Actions

- Reuse the same `!UsesStatusFilteredDeployments(skill)` guard when another shared `SingleAttack` row combines `hit_target_count=global` with status-filtered multi-center deployment.
- If a future skill needs explicit caster-origin behavior even with status-filtered deployment, add a named shared flag instead of relying on the old implicit coupling.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now narrows `ResolvePrefabHitboxCenter(...)` so only non-status-filtered `HitAllTargets` skills snap the prefab origin back to the caster.
- The same executor still resolves status-filtered centers via `ResolveDeploymentCenters(...)`, still uses `UsePrefabHitbox`, and still applies overlap/repeat behavior on those centers.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` completed to the point that the console logged runtime catalog load plus sync without a new C# or CSV failure.

### History

- 2026-05-31: Vega-D exposed that the shared `HitAllTargets` origin rule was still assuming a caster-anchored slash even after Vega-D had been re-authored to use target-centered status-filtered fanout AoE.

## Task: 2026-05-31 Shared SingleAttack Overlapping Fanout Reuse For Vega-D

### Task title

Reuse the current shared status-filtered `SingleAttack` fanout path for overlapping local AoE hits and delayed repeats through data-only Vega-D row changes.

### Goals

- Keep one deployment center per status-matched target.
- Allow each deployment center to hit all enemies in its local area when the skill row authors `hit_target_count=global`.
- Reuse the existing shared per-target repeat scheduler for delayed extra slashes instead of adding a Vega-only coroutine path.

### Constraints

- Role Owner is Code Builder.
- No new runtime code was added for this task; the change was limited to authoring values already supported by the inspected shared executor and snapshot.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented through active CSV re-authoring and compile/editor validation.

### Next Actions

- Reuse `hit_target_count=global` on status-filtered `SingleAttack` rows when local overlap stacking is intended.
- Reuse `repeat_count_per_target` plus `repeat_interval_seconds` when a fanout slash should add delayed extra hits at each resolved deployment center.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` still resolves status-filtered deployment centers with `ResolveDeploymentCenters(...)`, computes local hit count with `ResolveEffectiveHitTargetCount(...)`, and schedules delayed repeats per center in `ScheduleRepeatedDeployments(...)`.
- The same executor computes repeat timing as `delaySeconds = snapshot.RepeatIntervalSeconds * repeatIndex`, which is why authored `repeat_count_per_target=2` plus `repeat_interval_seconds=0.5` yields `+0.5s` and `+1.0s` follow-up hits after the immediate base hit.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillAreaUtility.cs` and `SkillExecutionUtility.cs` still route radius multipliers into both collision radius and live prefab scale, so overlap and visual growth stay aligned on the shared path.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors Vega-D with `hit_target_count=global`, and `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `vega-d-master-1` with `repeat_count_per_target=2` and `repeat_interval_seconds=0.5`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` completed to the point that the console logged runtime catalog load plus sync without a new C# or CSV failure.

### History

- 2026-05-31: Earlier same-day Vega-D row authoring had constrained local hits to single-target behavior.
- 2026-05-31: User later requested overlap-stacking AoE plus two delayed extra hits, and Code Builder confirmed the existing shared executor already supported those semantics through current CSV fields alone.

## Task: 2026-05-31 Shared SingleAttack Status-Filtered Fanout Single-Target Fix

### Task title

Split status-filtered `SingleAttack` fanout from line-style multi-deployment presentation so per-target repeated casts can remain single-target.

### Goals

- Preserve the shared deployment resolution that fans out across enemies carrying a required runtime status.
- Stop status-filtered fanout from inheriting the long line visual transform used by non-status multi-deployment prefab slashes.
- Restore authored hit-target-count handling for status-filtered fanout deployments.

### Constraints

- Role Owner is Code Builder.
- The existing shared runtime already supported status-filtered deployment centers, so this task stayed within current common logic instead of introducing a new shared deployment system.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor-validated.

### Next Actions

- Reuse the same split whenever another shared `SingleAttack` skill needs one cast per status-matched target without the line-style stretched visual treatment.
- If a future skill truly needs status-filtered fanout plus line-style stretching, author or add an explicit shared flag instead of relying on the old implicit `UseMultiDeployment` coupling.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` still couples `DeploymentRequiredTargetStatusId` to `UseMultiDeployment`, so shared executor handling remains the right place to separate visual semantics from deployment semantics.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now adds `UsesStatusFilteredDeployments(...)`, `UsesLineStyleMultiDeploymentVisual(...)`, and `ResolveEffectiveHitTargetCount(...)` so status-filtered fanout no longer automatically means line-style stretched visuals or unlimited hit count.
- The same executor still resolves one deployment center per status-matched target through `ResolveDeploymentCenters(...)`, so existing shared marked-target fanout behavior remains intact.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` completed to the point that the console logged runtime catalog load plus sync without a new C# or CSV failure.

### History

- 2026-05-31: Vega D exposed that the old shared `UseMultiDeployment` branch conflated three concerns: repeated deployment centers, line-style prefab presentation, and unlimited hit count.
- 2026-05-31: Code Builder split the status-filtered fanout path from the line-style branch while keeping the same shared deployment-center resolution.

## Task: 2026-05-31 Shared Target-Status Consumption And Redistribution Support For Vega E

### Task title

Extend the shared combat/status runtime so `SingleAttack` can scale from target status stacks, consume part of those stacks, and optionally redistribute consumed stacks on kill.

### Goals

- Keep Vega E mark interaction on shared runtime contracts rather than a Vega-only status branch.
- Support partial stack consumption on runtime statuses through the existing shared unit status store.
- Let shared `SingleAttack` resolve conditional crit and consumed-status redistribution from snapshot data.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The implementation stays on shared status storage, combat-manager helpers, and `SingleAttack` runtime paths.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile/CSV-validation verified.

### Next Actions

- Reuse the shared consume-stack helper path if another skill later needs partial status consumption instead of whole-status removal.
- Reuse the existing redistribution snapshot fields when another inspected skill needs explicit search radius/count authority instead of adding a parallel status spread system.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` now exposes shared `ConsumeStacks(...)` helpers on `UnitStatusRuntimeSet` and `UnitStatusRuntime`, which lets status stacks be reduced without clearing the whole status entry.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now wraps shared status-stack consumption through `ConsumeStatusStacks(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carries shared target-status-stack damage multipliers, consume overrides, conditional crit rules, and consumed-status redistribution fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now resolves target-status-stack additive damage, consumes planned stacks on hit, and redistributes a portion of consumed stacks on kill when snapshot data requests it.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillStatusSpecUtility.cs` now creates a direct status spec for redistribution application without adding a Vega-specific status application path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Vega E required shared target-status-stack damage plus partial mark consumption, so Code Builder extended the shared status/combat path instead of hardcoding `name-mark` logic inside a Vega-only branch.
- 2026-05-31: Builder also added shared consumed-status redistribution support.
- 2026-05-31: User later supplied Vega-E trait-5 search radius `100` and target count `1`, and Skill Builder finished the active redistribution row on that existing shared path.

## Task: 2026-05-30 Shared Source-Status Modifier And Marked-Target Fanout Support For Vega C/D

### Task title

Extend shared status-aware combat runtime so buff-active choice modifiers and marked-target fanout can stay on reusable common paths.

### Goals

- Let choice rows require an active source status before they modify later outgoing skill behavior.
- Let attached buff status data receive shared choice-driven action-speed and attack-power scalar overrides.
- Let shared contact-target resolution filter targets by required runtime status and minimum stacks for marked-target fanout skills such as Vega D.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The runtime additions remain shared status/targeting behavior, not Vega-only executor branches.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile/CSV-validation verified.

### Next Actions

- Reuse `RequiredSourceStatusId` plus `RuntimeTargetSkillIds` when a future buff should change only specific later skills while the source buff is active.
- Reuse `DeploymentRequiredTargetStatusId` when a future `SingleAttack` or other resolved-deployment skill must fan out only across marked targets.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now checks `RequiredSourceStatusId` / minimum stacks before a choice spec is applied and now matches delimited `RuntimeTargetSkillIds`, which is the shared gate Vega C uses for buff-active trait/master behavior.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillStatusSpecUtility.cs` now clones attached status data with snapshot-provided `status_action_speed_bonus` and `status_attack_power_bonus` overrides, so buff status scalars no longer have to stay fixed on the base skill row only.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs` and `.../SkillExecutionUtility.cs` now expose shared target resolution filtered by required target status id and minimum stacks.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now resolves one deployment center per matched target carrying the required status and supports repeat-per-target fanout through shared snapshot fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carries shared repeat-per-target values and attached-buff scalar override flags through the execution snapshot used by both Vega C and Vega D.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.

### History

- 2026-05-30: Vega C and Vega D implementation required the shared runtime to understand buff-active source-status gates, attached buff scalar overrides, and marked-target deployment fanout before the routed Vega rows could move out of `DataOnlyUnsupported` / mismatched runtime states.

## Task: 2026-05-28 Shared Silence Default Duration For Vega-B Threshold Refresh

### Task title

Align the shared `silence` base duration with Vega-B threshold silence refresh so the extra second can be authored without a duplicate status id.

### Goals

- Let Vega-B base silence remain `3s` while the master-2 threshold refresh lands at `4s`.
- Let trait-2 `+1s` stack naturally on both the base silence and the threshold refresh.
- Avoid creating a second silence status id only for Vega-B.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The shared status id `silence` was changed only after inspecting the active Vega-B CSV usage in the current routed skill-authoring scope.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- If another inspected skill later requires a different shared silence base, revisit whether `silence` should stay shared or whether that skill needs a distinct status id.

### Evidence

- `Pakuri/Assets/CSVdata/source/status_effects.csv` now sets `silence` default duration to `4`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors Vega-B base silence explicitly at `status_duration_seconds=3`, so the base hit stays at `3s`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now uses the shared threshold reapply path for `vega-b-master-2`, so the reapplied silence reads the shared default `4s`, and the same choice CSV applies `vega-b-trait-2` as `status_duration_bonus_status_id=silence` / `status_duration_bonus=1`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` resolves status duration from explicit duration first and otherwise falls back to the shared status default, then adds snapshot duration bonuses for the matching status id.

### History

- 2026-05-28: Vega-B master-2 needed “Name Mark 10 stacks or more -> silence duration +1 second” on the shared threshold reapply path, which reads status defaults instead of the original base-skill explicit duration.

## Task: 2026-05-27 Zero-Damage Persistent Presence Zone Validation

### Task title

Keep shared presence-status zones valid when they intentionally deal no damage.

### Goals

- Preserve the `sein-d-superheated-presence` refresh path as a zero-damage persistent zone.
- Avoid adding fake damage to presence-only effect rows.
- Verify the shared CSV validator recognizes this status-only persistent-zone pattern.

### Constraints

- Role Owner is Code Builder.
- The runtime/status behavior stays shared; no Sein-only validator bypass was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Reuse the same shared validation allowance for future persistent zones that exist only to refresh a status and intentionally deal zero damage.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now treats zero-damage `Damage` effects as valid only when they are persistent zones with status payloads and zero stat coefficients.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` keeps `sein-d-zone-presence` and `sein-e-master2-zone-presence` at `base_damage=0` while continuing to apply `sein-d-superheated-presence`.
- Unity menu `Pakuri/Validate CSV Source Data` completed and logged the runtime catalog load summary without the previous `requires positive base_damage` errors.

### History

- 2026-05-27: Shared validation originally forced positive base damage on all `Damage` effect rows, which incorrectly rejected presence-only persistent zones.

## Task: 2026-05-27 Sein-D Superheated Presence Shared Status

### Task title

Add a shared zone-presence status so Sein-E conditional damage can query whether a target is currently inside a Sein-D-style superheated zone.

### Goals

- Keep `Sein-E trait-5` on the existing conditional-target-status damage path.
- Avoid overloading `sein-d-heat-stack`, which represents repeated zone hits rather than current zone occupancy.
- Reuse shared persistent-zone multi-effects so both base Sein-D and Sein-E master-2 can refresh the same presence status.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The status store remains the shared `StatusEffectKind` / combat-manager status runtime; no parallel zone-presence registry was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that enemies inside base Sein-D and Sein-E master-2 zones keep the short-lived `sein-d-superheated-presence` status refreshed while they remain inside the area.
- User verifies that leaving the zone drops the status quickly enough for `Sein-E trait-5` damage to stop applying.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now defines `SeinDSuperheatedPresence`, parses id `sein-d-superheated-presence`, and returns a shared runtime definition with default duration `0.75s` and max stacks `1`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains `sein-d-superheated-presence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `sein-d-zone-presence` so base Sein-D can refresh that shared presence status through an `OnCast` persistent-zone companion effect.
- The same effect CSV now contains `sein-e-master2-zone-presence` so each Sein-E master-2 deployment center spawns a matching persistent presence zone through shared `OnDeploymentCast` routing.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now gates `sein-e-trait-5` on `conditional_target_status_id=sein-d-superheated-presence` and `conditional_target_status_min_stacks=1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-05-27: Sein-E trait-5 required “currently inside superheated zone” semantics, so Builder added a new shared presence status instead of reusing the existing repeated-hit stack status.

## Task: 2026-05-27 Sein Projectile/Zone Conditional Status Additions

### Task title

Add shared runtime status identities for Sein-C trait-5 and Sein-D trait-5 conditional damage logic.

### Goals

- Keep Sein-C trait-5 on a shared conditional-target-status path instead of a hardcoded target-memory branch.
- Keep Sein-D trait-5 on a shared status-stack threshold path driven by the zone hit runtime.
- Route both statuses through the existing `StatusEffectKind` / shared combat-manager status store.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The status ids were added to shared status/runtime files and active CSV authority; no parallel status store was introduced.
- `sein-a-hit-mark` duration `5s` is inferred because the inspected request bundle did not provide an explicit duration.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-C trait-5 damage only increases against enemies recently hit by Sein-A.
- User verifies in Play Mode that Sein-D trait-5 only increases damage after the same target has accumulated at least 4 recent zone-hit stacks.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now defines `SeinAHitMark` and `SeinDHeatStack`, accepts ids `sein-a-hit-mark` and `sein-d-heat-stack` in `TryParse(...)`, and returns shared runtime definitions for both statuses.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains rows `sein-a-hit-mark` and `sein-d-heat-stack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now applies `sein-a-hit-mark` from Sein-A hits and `sein-d-heat-stack` from Sein-D zone ticks.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now gates `sein-c-trait-5` with `conditional_target_status_id=sein-a-hit-mark` and `conditional_target_status_min_stacks=1`, and gates `sein-d-trait-5` with `conditional_target_status_id=sein-d-heat-stack` and `conditional_target_status_min_stacks=4`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-05-27: Sein-C trait-5 and Sein-D trait-5 required reusable status-gated damage conditions, so new shared status identities were added instead of hardcoding those checks inside the skill runtime.

## Task: 2026-05-26 SingleAttack OnHit Status Effect Support

### Task title

Let shared SingleAttack hits apply choice-gated OnHit status effects for Rin-E master-2 slow.

### Goals

- Reuse `monster_skill_effects.csv` OnHit status rows for SingleAttack hit targets.
- Keep Rin-E master-2 slow on shared status application instead of a Rin-only branch.
- Preserve existing shared `SkillStatusApplyUtility` status application.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The change is scoped to SingleAttack hit targets and status-type OnHit effects.
- Unity Play Mode gameplay verification remains user-owned.
- Unity CSV runtime catalog sync is pending because batchmode reported another Unity instance has this project open.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-E master 2 applies the 2-second, -25% move speed slow to each hit enemy.
- Sync runtime catalog assets once Unity project locking allows it.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now resolves `SkillMultiEffectTiming.OnHit` status effects with `SkillMultiEffectExecutor.ShouldRun(...)` and applies them to each SingleAttack hit target through `SkillStatusApplyUtility.TryApplyStatus(...)`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `rin-e-master2-slow` with `effect_timing=OnHit`, `status_effect_id=slow`, `status_duration_seconds=2`, and `status_move_speed_bonus=-0.25`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed only because Unity batchmode reported another Unity instance has `C:/TowerDefence_Pakuri/Test/Pakuri` open.
- Follow-up enum validation found the `DamageAttribute` enum defines `Darkness`, not `Dark`; `rin-e-master2-slow.attribute` was corrected to `Darkness`, and a CSV enum scan returned `ENUM_VALIDATION_OK`.

### History

- 2026-05-26: Rin-E master-2 implementation required slow on each SingleAttack hit target, so SingleAttack adopted the existing shared OnHit status-effect pattern already used by beam/line runtime paths.
- 2026-05-26: User reported Unity auto-sync failing on `monster_skill_effects.csv` row 78 because `attribute=Dark` was not a valid enum value; Builder corrected the status-effect row to use `Darkness`.

## Task: 2026-05-24 Shared Passive Condition-Status And Trigger Expression Support

### Task title

Extend shared status/trigger runtime so passive effect rows and trigger rows can target expression-style condition statuses and shared proc-gated routed skills.

### Goals

- Let shared runtime parse condition-status expressions such as `chill;freeze` and `shock:5`.
- Let passive effect rows and trigger rows both consume the same condition-status matcher instead of duplicating string logic.
- Keep routed trigger validation aligned with actual runtime semantics so non-`SingleAttack` routed skills do not need fake damage payloads.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The implementation must stay on shared status parsing, trigger runtime, and CSV validation paths.
- Unity Play Mode gameplay verification remains user-owned.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and Unity CSV validation passed.

### Next Actions

- Reuse the shared condition-status expression format for future passive or trigger work that needs OR lists or minimum stack gates before inventing another status-condition schema.
- Keep trigger damage-field validation scoped to `SingleAttack` unless a future routed trigger runtime begins consuming its own damage payload.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now defines shared condition-status parsing and matching helpers used by both target status checks and status-expire trigger checks.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `SkillTriggerRuntime.cs` now delegate condition-status checks to `StatusEffectRuntime`, and `SkillTriggerRuntime.cs` now supports multi-attribute trigger filters such as `Lightning;Ice`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now validates expression-style `condition_status_id` values and shared trigger-attribute lists, and now limits trigger damage payload validation to `runtime_kind=SingleAttack`.
- The validation follow-up was grounded by the failing Eve-G trigger rows `eve-g-auto-prism-ray` and `eve-g-auto-prism-ray-trait1`, which route `LineAttack` `eve-b` and therefore should not require synthetic `base_damage` values on the trigger row.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the shared runtime/validation change; only the existing `MSB3277` warnings remained.
- Unity `Pakuri/Validate CSV Source Data` then completed successfully and logged the runtime catalog load summary instead of `CsvFatalException`.

### History

- 2026-05-24: Eve F-J passive completion exposed that shared trigger validation was over-constraining routed non-`SingleAttack` triggers and that passive condition-status rows needed shared expression parsing.

## Task: 2026-05-26 Rin F-J Passive Status/Trigger Runtime Extensions

### Task title

Support Rin F-J passive status bonuses, hit-count effects, and trigger actions on shared combat runtime paths.

### Goals

- Let statuses grant outgoing critical damage bonus in the same modifier path as action speed, attack power, and critical chance.
- Let multi-effect rows run on `OnHitCount` for hit-count-gated passive effects.
- Let passive triggers filter by event skill, event source scope, trigger count, and status source skill before running a `SingleAttack`, effect, cooldown refund, or reload reduction action.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The extension is shared runtime behavior, not Rin-only branches.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile/CSV-validation verified.

### Next Actions

- Use `StatusEffectRuntime.ResolveCriticalDamageBonus(...)` for future outgoing critical damage status bonuses.
- Use `SkillMultiEffectTiming.OnHitCount` plus `condition_hit_count_min` for future hit-count-gated passive effects.
- Keep count gate evaluation before proc/internal cooldown consumption for `trigger_every_count` trigger rows.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` now includes `BuffModifierSpec.CritDamageBonusRate`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now initializes, measures, and resolves critical-damage status bonuses through `ResolveCriticalDamageBonus(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` adds resolved status critical damage to outgoing critical damage calculation and stores passive trigger counts.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now supports `ExecuteOnHitCount(...)`, health-ratio target conditions, hit-count conditions, and status critical damage bonus mapping.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now supports `SkillTriggerActionKind` actions, event-applied damage source, delayed triggers, event skill filters, event source scope filters, condition status source skill filters, count gates, cooldown refund, and reload reduction.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary and did not log a Pakuri CSV validation failure.

### History

- 2026-05-26: User approved extending shared runtime support for Rin F-J, including all-allied physical damage counts and reusable trigger/effect structures.

## Task: 2026-05-17 InGame Shared Status Runtime Baseline

### Task title

Keep the current Scripts2 status runtime grounded in `StatusEffectKind` and the shared unit-status store.

### Goals

- Keep all new status work routed through `StatusEffectKind` instead of ad hoc strings.
- Keep status storage, ticking, apply/remove/query, and label refresh owned by shared runtime code.
- Keep Eve-A shock application on the shared projectile hit path.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older status-effect slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active status runtime baseline summarized and retained for future work. 2026-05-18 Code Builder refactor keeps status labels on the actor path while centralizing shared actor presentation in `UnitActorView`. 2026-05-18 projectile/status tuning now reads status chance and label from `monster_skills.csv`; supported runtime labels can now be used as a fallback when `status_effect_id` is blank.

### Next Actions

- Future skills should apply statuses only through `InGameCombatManager.ApplyStatus(...)`.
- Later passive/resistance/damage work should query `StatusEffectKind`-based runtime state rather than adding parallel status storage.
- Use the archive snapshot when older shield/freeze/temporary-effect details are needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` defines the shared enum and central status display helpers.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` owns the current unit status store and ticking behavior.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` owns status apply/remove/query plus actor refresh on state changes.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `EnemyUnitActor.cs` delegate active status label presentation to `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `InGameProjectileActor.cs` currently route Eve-A shock through the shared projectile hit path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now contains `status_chance` and `status_effect_label` per skill; Eve-A stores `status_effect_id=shock`, `status_chance=0.15`, and `status_effect_label=媛먯쟾`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` passes CSV `StatusChance` into `StatusApplicationSpec.Chance`; `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` no longer contains the Eve-A shock chance special case.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now parses supported Korean labels `媛먯쟾`, `異붿쐞`, `?됯린`, `鍮숆껐`, `?뷀솕`, `痍⑥빟`, and `諛⑹뼱留? in addition to the canonical ids.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now resolves blank `status_effect_id` from a parseable `status_effect_label` and stores the canonical status tag from `StatusEffectKind`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` rejects positive `status_chance` values on unsupported runtime status labels/ids.
- Unsupported design labels such as `移⑤У`, `?대쫫?쒖떇`, `?좎꽦 ?몄텧`, `?붿뿼 ???媛먯냼`, `?됰룞?띾룄 利앷?`, and `?됰갚` remain label-only in `monster_skills.csv` with `status_chance=0` unless a matching `StatusEffectKind` is added later.

### History

- 2026-05-17: Shared status runtime, enum centralization, label suffix display, and Eve-A shock application became the active baseline.
- 2026-05-18: Code Builder commonized `MonsterUnitActor`/`EnemyUnitActor` display refresh through `UnitActorView.cs`.
- 2026-05-18: Code Builder moved status chance/label authority from monster-level rows and hardcoded Eve-A executor logic into per-skill CSV rows.
- 2026-05-18: Code Builder made supported Korean status labels parseable from CSV, added validation for unsupported positive `status_chance`, and normalized design-only labels to chance 0.

## Source: boards\DATA\DATA_BLACKBOARD.md

## Task: 2026-06-19 Enemy Skill Node Runtime Implementation 1-7

### Task title

Implement the data side of enemy skill node runtime handoff steps 1-7.

### Goals

- Add enemy skill node and node-param runtime CSV files.
- Remove `enemy_scope` from `EnemySkillData.csv` and keep `radius` as the enemy skill range authority.
- Add Stage2 active skill rows and bind Stage2 units to those skills.
- Keep Stage1 skills on the same node data path with old executor fallback still present.

### Constraints

- Role Owner is Code Builder.
- User deferred handoff step 8; old direct execution fallback remains.
- MSW-MCP is not used; Unity-MCP is the only MCP validation path.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, CSV-shape checked, and Unity-MCP CSV sync/validate checked. 2026-06-19 follow-up Code Builder pass added runtime validation coverage for enemy node `action_op` and `target_selector` values.

### Next Actions

- User verifies Stage2 skill behavior in Play Mode before step 8 removes old direct paths.
- Keep future enemy skill tuning in `EnemySkillData.csv` and behavior composition in `EnemySkillNodes.csv` / `EnemySkillNodeParams.csv`.

### Evidence

- Added `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillNodes.csv` and `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillNodeParams.csv`.
- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillData.csv` no longer contains `enemy_scope` or `range`.
- Stage2 skill rows in `EnemySkillData.csv` use requested radii: FireDragonSlash 2, ChainLightning 7, FrostPressure 2, DarkStab 1.4, HolyDragonHeal 5, HolySpearThrow 14, OpeningCharge 40, Intimidation 40.
- `Pakuri/Assets/CSVdata/authoring/enemy/stage_two_enemies.csv` binds Stage2 enemies to those Stage2 active skill ids.
- CSV row-width check returned `bad=` empty for `EnemySkillData.csv`, `stage_two_enemies.csv`, `EnemySkillNodes.csv`, and `EnemySkillNodeParams.csv`.
- `PakuriCsvRuntimeData.Validation.cs` now rejects unsupported `EnemySkillNodes.csv` `action_op` values and unsupported `target_selector` values.
- PowerShell validation of `EnemySkillNodes.csv`, excluding the second schema row, returned `badOps=` and `badSelectors=` empty.
- `EnemySkillNodeParams.csv` contains the requested Stage2 values including `ChainLightning delay=0.5`, `ChainLightning chain_radius=7`, `FrostPressure action_speed_bonus=-0.2`, and `Intimidation multiplier=0.7`.
- Runtime/editor builds passed with 0 errors; only existing `MSB3277` assembly-version warnings remained.
- Unity-MCP sync logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/authoring' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP validate logged runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- 2026-06-19 follow-up Unity-MCP validation could not run because no Unity Editor instance was found by the MCP bridge.

### History

- 2026-06-19: User asked Code Builder to implement handoff steps 1-7, create the two enemy node CSV files, make Lightning Scout chain again after 0.5 seconds on another target, and make Arsen reduce target outgoing damage to x0.7.

## Task: 2026-06-19 EnemySkillData Range Column Removal

### Task title

Remove the unused `range` column from enemy skill runtime CSV data.

### Goals

- Keep enemy skill distance data on the currently used `radius` column.
- Remove the unused `range` column from `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillData.csv`.
- Preserve runtime CSV sync and validation.

### Constraints

- Role Owner is Code Builder.
- No enemy combat code was changed; inspected runtime code already ignored `range`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- Keep future Stage1 enemy skill distance authoring on `radius` unless runtime code adds a separate range contract.

### Evidence

- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillData.csv` no longer contains the `range` header/type column.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\authoring\enemy\EnemySkillData.csv` showed `hasRange=False`, `headerCount=33`, and data rows loaded.
- TextFieldParser row-width check returned `expected=33` and `bad=` empty.
- Search under `Pakuri/Assets/Scripts2/InGame` found no `ReadFloat("range")`, `ReadOptionalFloat(record, "range")`, `ActiveSkillRange`, or `BasicSkillRange` references.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged sync from `Assets/CSVdata/authoring`.
- Unity-MCP `Pakuri/Validate CSV Source Data` logged runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity-MCP warning/error console read returned 0 entries.

### History

- 2026-06-19: User asked Code Builder to delete the unused `range` column after code inspection showed Stage1 enemy attack distance uses `radius` or attack-type fallback.

## Task: 2026-06-19 Enemy Skill Node Data Handoff

### Task title

Record the data-facing handoff for future enemy active skill node authoring.

### Goals

- Keep current `EnemySkillData.csv` as the enemy skill body source.
- Plan future enemy behavior rows as node and node-param data instead of extending hardcoded Stage1 skill switches.
- Include Stage1 enemy skills in the migration so Stage1 and Stage2 do not diverge into separate execution/data models.

### Constraints

- Role Owner is Designer.
- This task produced a handoff only; no CSV file, column, row, parser, prefab, or runtime catalog asset was changed.
- Proposed enemy node CSV files do not exist yet and must be created by Code Builder only after the implementation route is chosen.
- Do not infer unsupported dual-attribute damage or tower status support without inspecting/adding runtime support.

### Role Owner

Designer

### Status

Handoff created; implementation not started.

### Next Actions

- Code Builder decides exact enemy node CSV schema and adds parser/runtime support.
- Candidate files from the handoff are `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillNodes.csv` and `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillNodeParams.csv`.
- Code Builder keeps old enemy execution fallback until Stage1 node parity is verified.
- Code Builder removes `enemy_scope` from `EnemySkillData.csv`, adds Stage2 active skill body rows there, and treats each row's `radius` as the source of truth for enemy skill range.

### Evidence

- Created `Pakuri/reference/Report/2026-06-19-enemy-skill-node-runtime-handoff.md`.
- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillData.csv` exists and currently holds enemy skill body/tuning fields.
- Updated `Pakuri/reference/Report/2026-06-19-enemy-skill-node-runtime-handoff.md` to require `EnemySkillData.csv` Stage2 rows, no `enemy_scope` gate, and requested Stage2 radius values: Fire Dragon Soldier 2, Lightning Scout 7, Ice Guard 2, Dark Assassin 1.4, Holy Priest 5, Ethan 14, Drake 40, Arsen 40.
- The proposed `EnemySkillNodes.csv` and `EnemySkillNodeParams.csv` files are handoff candidates only; they were not created in this task.
- `Pakuri/Assets/Scripts2/InGame/Data/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` contains normalized monster skill authoring support, but enemy skills are not currently compiled through that node path.
- `Pakuri/Assets/Scripts2/InGame/Enemy/EnemyCombatSystem.cs` currently executes enemy skills through resolved skill data and a direct skill-kind switch, not through enemy node CSV rows.

### History

- 2026-06-19: User requested a Code Builder handoff that judges Stage1 prefab skill applicability together with Stage2 enemy skill implementation planning.
- 2026-06-19: User revised the handoff so Stage2 enemy skills are managed through `EnemySkillData.csv`, enemy skill range comes from `radius`, `enemy_scope` is removed, and combat-start skills use high `radius` values for immediate execution.

## Task: 2026-06-19 Generic Node-Backed Choice Routing

### Task title

Record the data-facing behavior of replacing Ariel-only choice routing with generic node-backed choice routing.

### Goals

- Make normalized choice nodes apply generically for any monster instead of only Ariel.
- Keep legacy wide choice mapping as fallback for choices without normalized plan nodes.
- Preserve current runtime CSV schema and validation behavior.

### Constraints

- Role Owner is Code Builder.
- No CSV file, column, or row value was changed in this pass.
- Active CSV authority remains `Pakuri/Assets/CSVdata/authoring`.
- Play Mode gameplay parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, and Unity-MCP validated.

### Next Actions

- Future migrated choices for Eve, Vega, Sein, Rin, and Ariel can use normalized choice nodes without needing monster-specific runtime gates.
- Keep wide choice columns as compatibility fallback until old rows are fully migrated.

### Evidence

- `monster_skill_nodes.csv` currently contains normalized choice nodes for Ariel, Rin, and Vega, so routing had to become monster-agnostic rather than Ariel-only.
- `SkillExecutionSnapshot.cs` now routes any `SkillChoiceDefinition` with non-empty `NormalizedPlanNodes` through the node-backed choice path.
- `InGameSkillDefinitionMapper.cs` now skips old `ApplyNormalizedChoiceNodes(...)` folding when a choice already has normalized plan nodes, preventing node-backed choices from double-applying through legacy wide specs.
- `SkillExecutionSystem.cs` now reads `CountStatusDamageMultiplier` nodes without an Ariel-only gate.
- Search under `Pakuri/Assets/Scripts2/InGame/Skills` found no remaining `IsArielChoice` or `ApplyArielChoiceDefinition`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/InGame/Validate Skill Data` logged `InGame skill data validation passed with 0 warning(s)` after a clean console run, with 0 warning/error entries.

### History

- 2026-06-19: User requested Code Builder to fix Code Reviewer findings and make the structure better for future skill objectification and additions.

## Task: 2026-06-19 Shared Plan Projection For Existing Effect And Trigger CSV Rows

### Task title

Record the data-runtime projection path that lets existing effect/trigger CSV rows enter `SkillExecutionPlan`.

### Goals

- Keep current CSV schemas unchanged while making `monster_skill_effects.csv` and `monster_skill_triger.csv` rows visible as plan node payloads.
- Avoid adding Ariel-only data behavior; the projection applies to source-owned skill triggers for all monsters.
- Preserve existing runtime catalog sync and InGame skill validation.

### Constraints

- Role Owner is Code Builder.
- Active CSV authority remains `Pakuri/Assets/CSVdata/authoring`.
- No new CSV file, CSV column, or runtime catalog schema was added in this pass.
- Existing trigger/effect row execution remains compatible through fallback paths.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- Future CSV migration can author trigger/effect action semantics as normalized plan nodes once handler coverage is complete.
- Keep existing CSV effect/trigger rows valid until Play Mode parity proves the handler replacement path.

### Evidence

- `SkillData.cs` now includes `SkillTriggerDefinition[] SkillTriggers`.
- `InGameSkillDefinitionMapper.cs` filters monster-level `SkillTriggerDefinition` rows by `SourceSkillId` and attaches them to each active/passive skill data object.
- `SkillExecutionPlan.cs` converts `SkillData.MultiEffects` and `SkillData.SkillTriggers` into `SkillExecutionPlanNode.FromEffect(...)` and `SkillExecutionPlanNode.FromTrigger(...)`.
- `SkillPlanActionDispatcher.cs` resolves plan-projected effects/triggers with legacy fallback.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP CSV sync logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/authoring' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP validation logged runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity-MCP InGame skill validation logged `InGame skill data validation passed with 0 warning(s)`, and warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested the target structure be made reusable for Eve, Vega, Sein, Rin, and Ariel.
- 2026-06-19: Code Builder added plan projection for existing effect/trigger CSV runtime objects without changing CSV shape.

## Task: 2026-06-19 Ariel Plan-Action CSV Migration

### Task title

Record Ariel runtime CSV movement from old choice-wide behavior fields to normalized plan action nodes.

### Goals

- Keep Ariel choice metadata in `monster_skill_choices.csv`, but remove active Ariel modifier payload reliance on old behavior columns.
- Store Ariel D trait4/master1 remaining modifiers in `monster_skill_nodes.csv` and `monster_skill_node_params.csv`.
- Preserve Ariel A master2 status application as explicit trigger/effect CSV rows.

### Constraints

- Role Owner is Code Builder / Code Reviewer.
- Active CSV authority remains `Pakuri/Assets/CSVdata/authoring`.
- `monster_skill_triger.csv` and `monster_skill_effects.csv` remain explicit runtime object tables in this pass.

### Role Owner

Code Builder / Code Reviewer

### Status

Implemented and reviewed for Ariel-first migration scope.

### Next Actions

- Keep future Ariel modifier additions on `monster_skill_nodes.csv` plus `monster_skill_node_params.csv`.
- Do not add new Ariel behavior columns to `monster_skill_choices.csv` without a recorded exception.

### Evidence

- `monster_skill_choices.csv` Ariel old behavior-field scan returned `arielWideNonDefault=0`.
- `monster_skill_nodes.csv` has `ariel-d-trait-4-hit-target-count-bonus` / `HitTargetCountBonus` and `ariel-d-master-1-status-critical-damage-taken` / `StatusCriticalDamageTakenBonus`.
- `monster_skill_node_params.csv` stores `bonus=1` for D trait4 hit target count and `bonus=0.25` for D master1 critical damage taken.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` registers `HitTargetCountBonus` and validates overlap for `hit_target_count_bonus` and `status_critical_damage_taken_bonus`.
- Unity-MCP CSV sync and InGame skill validation passed with `InGame skill data validation passed with 0 warning(s)` and 0 warning/error console entries.

### History

- 2026-06-19: User requested Ariel-first target migration after prior Reviewer found old wide choice residues.

## Task: 2026-06-19 Ariel A Master2 Runtime CSV Binding Fix

### Task title

Record Ariel A master2 CSV migration from choice-wide status fields to trigger/effect/node rows.

### Goals

- Keep active Ariel A master2 behavior out of old `monster_skill_choices.csv` status-wide fields.
- Author the status application through `monster_skill_triger.csv` and `monster_skill_effects.csv`.
- Author the +15% Holy damage taken modifier through `monster_skill_nodes.csv` and `monster_skill_node_params.csv`.
- Add CSV validation coverage for migrated effect rows that still have executable gates.

### Constraints

- Role Owner is Code Builder.
- Active CSV authority remains `Pakuri/Assets/CSVdata/authoring`.
- Current runtime trigger enum uses `OnOutgoingDamage` for hit-success trigger binding; no unsupported `OnHit` enum value was authored.

### Role Owner

Code Builder

### Status

Implemented, compiled, and Unity-MCP validated.

### Next Actions

- Keep future migrated effect rows free of executable `requires_active_choice_id` / `requires_passive_skill_id` gates when `runtime_support_state=MigratedToEffectBinding`.
- Keep future on-hit status applications on trigger/effect rows before adding new wide choice columns.

### Evidence

- `monster_skill_choices.csv` now has `ariel-a-master-2` with old status-wide payload fields blank and `runtime_support_state=RuntimeImplemented`.
- `monster_skill_triger.csv` now has `ariel-a-master2-holy-exposure-on-hit` with `trigger_event=OnOutgoingDamage`, `target_selection=EventTarget`, and `trigger_action=Effect`.
- `monster_skill_effects.csv` now has `ariel-a-master-2-holy-exposure-on-hit` with `status_effect_id=holy-exposure`, `status_chance=1`, and `status_stack_amount=1`.
- `monster_skill_nodes.csv` and `monster_skill_node_params.csv` now carry the `StatusElementDamageTakenBonus` node and `bonus=0.15` param for `ariel-a-master-2`.
- `PakuriCsvRuntimeData.Validation.cs` now errors when `MigratedToEffectBinding` effect rows still carry executable choice/passive gates.
- CSV property-count check returned no bad rows for `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, `monster_skill_nodes.csv`, and `monster_skill_node_params.csv`.
- Unity-MCP sync/validate logs showed sync from `Assets/CSVdata/authoring`, runtime catalog load, and `InGame skill data validation passed with 0 warning(s)`.

### History

- 2026-06-19: Code Reviewer found `ariel-a-master-2` still depended on old choice-wide status columns and Ariel E migrated shield variants could still run through choice gates.
- 2026-06-19: Code Builder moved the behavior to trigger/effect/node rows, cleared migrated shield gates, and added validation coverage.

## Task: 2026-06-19 Ariel Passive Node Decomposition Follow-up

### Task title

Record Ariel passive modifier CSV decomposition on the normalized node path.

### Goals

- Keep Ariel passive numeric add-ons in `monster_skill_nodes.csv` and `monster_skill_node_params.csv` instead of duplicate choice-gated effect rows.
- Preserve runtime CSV validation and catalog sync after adding new status modifier handler ids.

### Constraints

- Role Owner is Code Builder.
- Active CSV authority remains under `Pakuri/Assets/CSVdata/authoring`.
- No new specialized effect binding CSV tables were added.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- Reuse the status modifier normalized handlers for future passive aura numeric add-ons.
- Keep conceptually new conditional passive effects in `monster_skill_effects.csv` when they add a new condition/effect object rather than modifying an existing base effect.

### Evidence

- Added normalized handler schemas for `StatusDamageBonusRate`, `StatusShieldReceivedBonus`, `StatusCriticalChanceBonus`, `StatusDamageTakenBonus`, and `StatusFlatElementResistReduction` in `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`.
- `monster_skill_nodes.csv` now carries the Ariel F/G/H/I/J passive status modifier nodes; the old generic passive damage node ids for F/H/I/J are absent (`oldGenericPassiveDamageNodes=0`).
- `monster_skill_effects.csv` marks G trait1, G trait2, I trait1, and J trait1 duplicate rows as `MigratedToEffectBinding`.
- `monster_skill_triger.csv` no longer contains `ariel-j-after-e-action-speed-trait1-trigger`.
- CSV shape check returned no bad rows for `monster_skill_choices.csv`, `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- Unity-MCP sync/validate logs showed sync from `Assets/CSVdata/authoring`, runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies, and `InGame skill data validation passed with 0 warning(s)`.

### History

- 2026-06-19: User requested Code Builder to finish Ariel A-J decomposition using the report's lines 373-962 as the node/effect/binding standard.

## Task: 2026-06-19 CSVdata Folder Reorganization

### Task title

Move active CSV authoring files into purpose-specific `Assets/CSVdata` folders and remove unused Codex backup CSV files.

### Goals

- Replace the old flat `Assets/CSVdata/source` runtime CSV folder with purpose-based runtime folders.
- Keep runtime catalog sync, validation, and editor auto-sync functional after the move.
- Preserve Unity GUID references by moving CSV `.meta` files with the CSV files.
- Delete unused `.bak_codex` CSV backup files from the active CSV folder.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV files remain UTF-8 and retain their row shape.
- `StageDay.csv`, `StageEncounter.csv`, and `StageReward.csv` remain active `NewRunScene` stage-flow inputs and were moved, not deleted.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, CSV-shape checked, compiled, and Unity-MCP sync/validate checked.

### Next Actions

- Future runtime CSV files should be added under `Assets/CSVdata/authoring/{catalog,enemy,monster,status}` by ownership.
- Future NewRunScene stage-flow CSV files should stay under `Assets/CSVdata/stage_flow`.
- Do not restore `Assets/CSVdata/source`; update `PakuriCsvRuntimeData.GetImportedSourceAssetPath(...)` when adding a new runtime CSV table.

### Evidence

- Active catalog CSV files now live under `Pakuri/Assets/CSVdata/authoring/catalog/`.
- Active enemy CSV files now live under `Pakuri/Assets/CSVdata/authoring/enemy/`, including `EnemySkillData.csv`.
- Active monster base/choice catalog CSV files now live under `Pakuri/Assets/CSVdata/authoring/monster/`.
- Active monster skill CSV files now live under `Pakuri/Assets/CSVdata/authoring/monster/skills/`.
- Active status CSV files now live under `Pakuri/Assets/CSVdata/authoring/status/`.
- Active stage-flow CSV files now live under `Pakuri/Assets/CSVdata/stage_flow/`.
- Deleted unused backups: `monster_skill_choices.csv.bak_codex`, `monster_skill_effects.csv.bak_codex`, `monster_skill_triger.csv.bak_codex`, and their `.meta` files.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.cs` now maps each runtime CSV filename to its purpose-specific folder through `GetImportedSourceAssetPath(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` now loads imported runtime CSVs through `GetImportedSourceAssetPath(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Editor/PakuriCsvRuntimeCatalogPostprocessor.cs` now watches `Assets/CSVdata/authoring/**/*.csv`.
- `Pakuri/Assets/Scripts2/InGame/Data/Editor/PakuriSkillEffectPrefabCsvExporter.cs` now writes `monster_skill_choices.csv` at `Assets/CSVdata/authoring/monster/skills/monster_skill_choices.csv`.
- PowerShell TextFieldParser check returned `bad=` empty for all active CSV files after the move, including runtime and stage-flow CSVs.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/authoring' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.`
- Unity-MCP warning/error console read after sync/validate returned 0 entries.

### History

- 2026-06-19: User requested Code Builder to reorganize `Pakuri/Assets/CSVdata` files by purpose, update hard paths, delete `.bak_codex` backups, run dotnet builds, and verify Unity-MCP CSV sync/validate.
- 2026-06-19: Code Builder moved active CSV files into `runtime/` and `stage_flow/`, removed the old empty `source` folder, updated runtime/editor hard paths, deleted unused `.bak_codex` backup files, and verified CSV shape, builds, and Unity-MCP sync/validate.

## Task: 2026-06-19 Ariel Normalized Choice Node Implementation

### Task title

Move Ariel numeric choice behavior toward generic normalized node authoring and reduce Ariel C pre-combined effect rows.

### Goals

- Use `monster_skill_nodes.csv` and `monster_skill_node_params.csv` as the user-selected generic effect-object storage path.
- Add reusable node handlers for common choice modifiers instead of adding new wide CSV columns.
- Migrate Ariel numeric choice modifiers out of `monster_skill_choices.csv` behavior fields into normalized choice nodes.
- Keep old effect rows only where still needed for compatibility, and disable Ariel C rows made redundant by composition.

### Constraints

- Role Owner is Code Builder.
- No specialized `skill_effect_bindings.csv`, `skill_effect_defs.csv`, or `skill_effect_modifiers.csv` files were added.
- Existing wide columns remain parser-compatible for old rows.
- Unity Play Mode parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, synced, and Unity-MCP validated.

### Next Actions

- User verifies Ariel C, Ariel B shield amount/duration, Ariel E shield trait/master composition, and Ariel J post-E / E-shield-only behavior in Play Mode.
- Future new exception behavior should prefer normalized node rows over new wide `monster_skill_choices.csv` columns.
- Code Reviewer pass is pending after the Phase 2-5 implementation.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now registers reusable node handlers including `CountStatusDamageMultiplier`, `MagazineBonus`, `ReloadTimeMultiplier`, `PierceBonus`, `DurationBonus`, `StatusActionSpeedBonus`, `StatusAilmentResistanceBonus`, `StatusConditionalDamageTakenBonus`, and `StatusElementDamageTakenBonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now keeps `PassiveDefinition.NormalizedPlanNodes`, and `PakuriCsvRuntimeData.Build.cs` builds passive-owned normalized nodes.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps the new node handlers into `SkillChoiceEffectSpec`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now applies normalized choice nodes during combat snapshot creation.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_nodes.csv` now has 47 imported rows after the Ariel migration, including `ariel-c-trait-2-blessing-action-speed`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_node_params.csv` now has 69 imported rows after the Ariel migration and Ariel C trait2 targeted action-speed node addition.
- Initial PowerShell migration output returned `migrated=28 nodes=47 params=68`; the final Ariel C trait2 node addition brought the parsed param row count to 69.
- TextFieldParser CSV shape check returned `monster_skill_choices.csv header=114 rows=252 bad=`, `monster_skill_nodes.csv header=14 rows=47 bad=`, `monster_skill_node_params.csv header=4 rows=69 bad=`, and `monster_skill_effects.csv header=70 rows=131 bad=`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_effects.csv` has 9 Ariel C pre-combined blessing rows disabled as `MigratedToEffectBinding`.
- Follow-up Phase 2-5 cleanup added the `ShieldAmountMultiplier` node handler and four Ariel shield amount nodes for B trait1, B master1, E trait2, and E master2.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_effects.csv` now has one active `ariel-e-shield*` row and three disabled E shield variants marked `MigratedToEffectBinding`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_effects.csv` no longer keeps J post-E action-speed behavior under `ariel-e`; those effects are now `ariel-j-after-e-action-speed*`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_triger.csv` now has two J-owned `OnSkillCast` trigger rows for `event_skill_id=ariel-e`.
- `condition_status_source_skill_id` was added to `monster_skill_effects.csv` and runtime parsing/build code so `ariel-j-shielded-holy-damage` can require the shield source `ariel-e-shield-base`.
- Phase 2-5 CSV shape check returned no bad rows for active Ariel-related skill CSV files, with `monster_skill_effects.csv header=71 rows=133 bad=`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP sync/validate logs showed CSV runtime catalog sync from `Assets/CSVdata/authoring`, runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies, and `InGame skill data validation passed with 0 warning(s).`

### History

- 2026-06-19: User requested Code Builder to execute the Ariel effect-object trigger-binding handoff and answered all six ambiguous design questions, including use of generic node CSVs.
- 2026-06-19: User then requested remaining Phase 2-5 implementation; Code Builder added shield amount nodes, reduced E shield variants, moved J post-E behavior to J-owned trigger/effect rows, and added effect source-skill conditions.

## Task: 2026-06-17 Normalized Skill Authoring Row Table Handoff

### Task title

Design the next CSV-authoring refactor so new exception skills add behavior nodes instead of new wide CSV columns.

### Goals

- Convert the 2026-05-29 skill runtime refactor feedback into a DATA-scoped authoring schema handoff.
- Keep current `monster_skills.csv` and `monster_skill_choices.csv` compatible during migration.
- Define a normalized row-table path where future behavior is authored through `monster_skill_nodes.csv` and `monster_skill_node_params.csv` instead of new CSV headers.
- Preserve existing `monster_skill_effects.csv` and `monster_skill_triger.csv` in the first pass because they already have row-like runtime support.
- Give Code Builder phases for parser skeleton, node compiler integration, first sample migration, choice-family migration, and future wide-column freeze.

### Constraints

- Role Owner is Designer for this handoff.
- Phase A changed CSV schema skeleton files, runtime CSV parser/model/validation code, and runtime source catalog references only.
- The handoff is grounded in the inspected source feedback html/md, active CSV headers, current parser/build code, and Phase 6 `SkillExecutionPlan` surface.
- Old wide columns must not be deleted in the first implementation.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Phase E wide-column freeze policy applied; Phase D choice-family migration remains locally validated.

### Next Actions

- Code Builder keeps legacy wide CSV behavior active until a later migration phase explicitly moves behavior into normalized nodes; `rin-d` is the first Phase C exception sample now migrated to normalized nodes.
- Code Builder updates `boards/COMBAT/ENEMY_BLACKBOARD.md` if normalized nodes change `SkillExecutionPlan`, executor routing, or runtime skill behavior.
- Code Reviewer should review before real skill authoring starts on the new node path.
- Phase C sample migration now has a duplicate guard for the currently supported execute/boss/kill normalized handlers.
- Phase D now has representative choice-owned normalized rows for damage/cooldown/radius modifiers, execute/boss/kill choice actions, on-hit additional damage, repeat per target, conditional crit, and redistribute-on-kill behavior; keep new exception choice behavior on node rows/params instead of adding new `monster_skill_choices.csv` behavior columns.
- Phase D reviewer follow-up now keeps representative choice metadata in `monster_skill_choice_base.csv`; duplicate legacy rows keep their behavior compatibility values, but `BuildSkillChoices(...)` prefers base-row metadata and can build future base-only choice rows with normalized nodes.
- Phase E board rule: new exception skill behavior must use `monster_skill_nodes.csv` plus `monster_skill_node_params.csv` by default.
- Phase E exception rule: adding new behavior columns to `monster_skills.csv` or `monster_skill_choices.csv` requires explicit Designer or Code Builder approval recorded in the active handoff or DATA board task.
- Existing wide behavior columns in `monster_skills.csv` and `monster_skill_choices.csv` are compatibility/deprecated inputs; keep them readable until enough migrated rows are proven through Play Mode.

### Evidence

- Created `Pakuri/reference/Report/2026-06-17-normalized-skill-authoring-row-table-handoff.md`.
- Source feedback inspected: `Pakuri/reference/Report/2026-05-29-skill-runtime-refactor-feedback-handoff.md` and `.html`.
- `monster_skills.csv` currently has 72 columns and 51 imported data rows.
- `monster_skill_choices.csv` currently has 114 columns and 253 imported data rows.
- `monster_skill_effects.csv` currently has 70 columns and 132 imported data rows.
- `monster_skill_triger.csv` currently has 47 columns and 57 imported data rows.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:417` to `:421` parses execute/boss/kill base skill columns directly.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:505` to `:533` parses execute/boss/kill choice columns directly.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:418` to `:490` shows `SkillDefinition` still owns many wide behavior fields.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:264` to `:350` shows `SkillChoiceDefinition` still owns many wide behavior fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs:6` to `:24` defines authoring source and node kind enums.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs:213` to `:226` already accepts normalized node rows through the compiler overload.
- Phase A added optional source catalog TextAsset fields for `monster_skill_base.csv`, `monster_skill_choice_base.csv`, `monster_skill_nodes.csv`, and `monster_skill_node_params.csv`.
- Phase A added empty header/type skeleton CSV files under `Pakuri/Assets/CSVdata/source/`.
- Phase A added `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` with row models, optional parsers, handler schema registry, and node/param validation.
- Phase A follow-up added handler-schema enum param value validation for normalized node params such as `predicate`, `attribute`, and `target_side`.
- Phase B added `SkillNodeDefinition` and `SkillNodeParamDefinition` to `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`.
- Phase B routes skill-owned normalized rows from `BuildActiveSkills(...)` into `SkillDefinition.NormalizedPlanNodes` and choice-owned normalized rows from `BuildSkillChoices(...)` into `SkillChoiceDefinition.NormalizedPlanNodes`.
- Phase B maps `SkillNodeDefinition[]` through `InGameSkillDefinitionMapper.MapSkillNodeDefinitions(...)` into `SkillExecutionPlanNode[]`.
- Phase B currently converts supported normalized handlers `TargetHealthRatioThresholdBonus`, `TargetPredicateDamageMultiplier` with `predicate=is_boss`, `BossDamageMultiplier`, `ExecuteCritChanceBonus`, `CooldownReset`, `CooldownResetOnKill`, `CooldownRefund`, and `CooldownRefundBonus` into typed plan ops; unsupported handlers still enter `SkillExecutionPlan.Nodes` as normalized row metadata without executable op payload.
- Phase B stores mapped normalized nodes on `SkillData.NormalizedPlanNodes` and `SkillChoiceEffectSpec.NormalizedPlanNodes`, and `SkillExecutionSnapshot` feeds them into `SkillExecutionPlanCompiler.Compile(source, snapshot, normalizedRows)`.
- Phase B reviewer follow-up preserves node `runtime_support_state` / `runtime_support_notes` and nested param `node_id` on `SkillNodeDefinition` / `SkillNodeParamDefinition` so runtime definitions no longer drop the normalized authoring support metadata.
- Phase B reviewer follow-up now validation-fails `owner_kind=Passive`, `owner_kind=Effect`, and `owner_kind=Trigger` until those owner paths are actually wired into runtime plans, preventing valid-looking normalized rows from being silently ignored.
- Phase A reviewer follow-up now enforces schema-declared enum params such as `predicate`, `attribute`, and `target_side` to use `value_type=Enum` and validates the authored value against the handler schema even when the row tries a different value type.
- Phase C migrated the `rin-d` base execute/kill sample by setting the legacy numeric wide fields `execute_health_ratio_threshold=0`, `execute_damage_multiplier=1`, and `kill_cooldown_refund_ratio=0` while keeping the old columns present and readable.
- Phase C added `rin-d-execute-condition`, `rin-d-execute-multiplier`, `rin-d-boss-multiplier`, and `rin-d-kill-cooldown-refund` rows to `monster_skill_nodes.csv`, with seven matching rows in `monster_skill_node_params.csv`.
- Phase C added duplicate validation guards so enabled normalized nodes for `TargetHealthRatioCondition`, `ExecuteDamageMultiplier`, boss multiplier handlers, `CooldownRefund`, `TargetHealthRatioThresholdBonus`, `ExecuteCritChanceBonus`, `CooldownRefundBonus`, and cooldown reset handlers fail when the matching legacy wide field is still active on the same owner.
- `dotnet build Pakuri.sln --no-restore` succeeded with 0 errors and existing `System.Net.Http` / `System.IO.Compression` conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after the enum validation follow-up; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after the enum validation follow-up; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Validate CSV Source Data` logged runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies after the empty normalized CSV files were imported.
- Phase B `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after implementation; existing `MSB3277` warnings remained.
- Phase B `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after implementation; existing `MSB3277` warnings remained.
- Unity-MCP Phase B smoke returned `nodes=1, damageModifiers=1, firstRow=phase_b_test_node, multiplier=1.25` for an in-memory normalized `BossDamageMultiplier` node.
- Phase B reviewer follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP direct `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` after the reviewer follow-up logged `Pakuri CSV runtime catalogs synced and validated from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP Phase B reviewer follow-up smoke returned `nodes=1, damageModifiers=1, row=phase_b_review_node, multiplier=1.25, support=RuntimeImplemented:phase_b_review_node`.
- Unity-MCP Phase B current-catalog check returned `catalog=True, activeSkills=25, skillNodes=0, choiceNodes=0`, confirming current empty normalized CSV rows do not add plan nodes to existing skills.
- Unity-MCP console after Phase B catalog load logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.`
- Phase C CSV field-count check returned `monster_skills.csv header=72 rows=52 bad=`, `monster_skill_nodes.csv header=14 rows=6 bad=`, and `monster_skill_node_params.csv header=4 rows=9 bad=`.
- Phase C `Import-Csv` check returned `rin-d` legacy values `threshold=0`, `require=true`, `execute=1`, `refund=0`, `boss=1`, with `nodeCount=4`, `paramCount=7`, and handlers `TargetHealthRatioCondition,ExecuteDamageMultiplier,TargetPredicateDamageMultiplier,CooldownRefund`.
- Phase C `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase C Unity-MCP `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` returned `SyncAndValidateCsvRuntimeCatalogsForEditor completed`.
- Phase C Unity-MCP runtime catalog inspection returned `legacy=threshold:0,require:True,execute:1,refund:0,boss:1|defNodes=4|planNodes=4|casts=1:0.3|damage=2:ExecuteMultiplier:1.8,BossMultiplier:1|kills=1:CooldownRefundBonus:0.35`.
- Phase D added 14 `Choice` owner rows to `monster_skill_nodes.csv` and 29 matching param rows to `monster_skill_node_params.csv`; handlers are `DamageMultiplier`, `CooldownMultiplier`, `RadiusMultiplier`, `TargetHealthRatioThresholdBonus`, `ExecuteCritChanceBonus`, `CooldownReset`, `TargetPredicateDamageMultiplier`, `CooldownRefundBonus`, `AdditionalDamage`, `EveryNthHitChainDamage`, `RepeatPerTarget`, `TargetStatusCritBonus`, and `RedistributeConsumedStatus`.
- Phase D migrated representative legacy values out of `monster_skill_choices.csv` for `ariel-a-trait-1`, `ariel-b-trait-3`, `ariel-c-trait-4`, `rin-d-trait-2`, `rin-d-master-1`, `rin-d-trait-5`, `rin-d-trait-3`, `rin-a-master-2`, `vega-d-master-1`, `vega-e-trait-4`, and `vega-e-trait-5` while keeping the old columns present.
- Phase D CSV field-count check using `Microsoft.VisualBasic.FileIO.TextFieldParser` returned `monster_skill_choices.csv: header=114 rows=253 bad=`, `monster_skill_nodes.csv: header=14 rows=19 bad=`, and `monster_skill_node_params.csv: header=4 rows=37 bad=`.
- Phase D `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase D Unity-MCP `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` returned `sync-ok`, and Unity-MCP `read_console` returned 0 warning/error entries afterward.
- Phase D Unity-MCP choice-family smoke returned `ariel-a-trait-1=True:1.25:nodes1`, `ariel-b-trait-3=True:0.8:nodes1`, `ariel-c-trait-4=True:1.25:nodes1`, `rin-a-master-2=extraTrue:1:0.4:Lightning:HitTarget|chain3:2:4.5:0.4:Lightning:nodes2`, `vega-d-master-1=damageTrue:0.65|repeat2:0.15:0.6:nodes2`, `vega-e-trait-4=crit0.35:name-mark:1:nodes1`, and `vega-e-trait-5=redistribute0.25:name-mark:5:3:nodes1`.
- Phase D fixed the duplicate-overlap guard so blank legacy chain/repeat multiplier cells are treated like the existing Build fallback (`>0 ? value : 1`) instead of being falsely considered active legacy wide behavior.
- Phase D reviewer follow-up filled `Pakuri/Assets/CSVdata/source/monster_skill_choice_base.csv` with 11 representative metadata rows for `ariel-a-trait-1`, `ariel-b-trait-3`, `ariel-c-trait-4`, `rin-a-master-2`, `rin-d-trait-2`, `rin-d-trait-3`, `rin-d-trait-5`, `rin-d-master-1`, `vega-d-master-1`, `vega-e-trait-4`, and `vega-e-trait-5`.
- Phase D reviewer follow-up updated `BuildSkillChoices(...)` so legacy duplicate choice rows preserve existing behavior fields while metadata fields come from `SkillChoiceBaseRows`, and base-only rows are merged by `sort_order` through `BuildBaseOnlySkillChoiceDefinition(...)`.
- Phase D reviewer follow-up updated normalized validation so choice-owned nodes and choice gates accept `monster_skill_choice_base.csv` rows, duplicate base/legacy rows must match `monster_id`, `skill_id`, and `choice_group`, and runtime asset validation accepts either legacy or base choice source rows.
- Phase D reviewer follow-up CSV field-count check returned `monster_skill_choice_base.csv header=13 lines=13 bad=`, `monster_skill_choices.csv header=114 lines=254 bad=`, `monster_skill_nodes.csv header=14 lines=20 bad=`, and `monster_skill_node_params.csv header=4 lines=38 bad=`.
- Phase D reviewer follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase D reviewer follow-up Unity-MCP `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` returned `sync-ok`; representative choice smoke returned each of the 11 migrated choice ids with `count=1` and expected node counts (`nodes=1` or `nodes=2`) plus base metadata descriptions.
- Phase D reviewer follow-up Unity-MCP console warning/error read showed only MCP transport `Client handler error: Cannot access a disposed object`, not a Pakuri CSV validation or C# compile error.
- Phase E updated the normalized skill-authoring handoff and the then-active exception guidance so future exception behavior routed to normalized nodes by default. That exception guide was later superseded by the single Enhancement/Master node blueprint.
- Phase E marks existing wide behavior columns in `monster_skills.csv` and `monster_skill_choices.csv` as compatibility/deprecated authoring surfaces while preserving old CSV rows and old columns.
- Phase E TextFieldParser CSV field-count check returned `monster_skills.csv header=72 rows=51 bad=`, `monster_skill_choices.csv header=114 rows=253 bad=`, `monster_skill_base.csv header=13 rows=1 bad=`, `monster_skill_choice_base.csv header=13 rows=12 bad=`, `monster_skill_nodes.csv header=14 rows=19 bad=`, `monster_skill_node_params.csv header=4 rows=37 bad=`, `monster_skill_effects.csv header=70 rows=132 bad=`, and `monster_skill_triger.csv header=47 rows=57 bad=`.
- Phase E `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase E Unity-MCP `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` returned `sync-ok`, and Unity-MCP warning/error console read returned 0 entries.

### History

- 2026-06-17: User noted that Phase 6 had not actually split or structured the CSV authoring layer yet, then requested a design and handoff for splitting `monster_skills.csv` and `monster_skill_choices.csv` so new exception skills can add nodes instead of columns.
- 2026-06-17: Code Builder implemented Phase A parser skeleton: optional normalized CSV files, row models, handler schema validation, empty schema CSVs, Unity import, and validation.
- 2026-06-17: Code Builder fixed the Code Reviewer finding that enum node params were only partly validated by adding handler-schema allowed enum values and re-running build plus Unity CSV validation.
- 2026-06-17: Code Builder implemented Phase B by preserving normalized CSV rows as `SkillNodeDefinition`, mapping supported handlers into `SkillExecutionPlanNode` operation payloads, and feeding base skill plus choice nodes into `SkillExecutionSnapshot` without removing or disabling legacy wide-column bridges.
- 2026-06-17: Code Builder fixed the Phase B Code Reviewer findings by preserving node support metadata, preserving nested param node ids, blocking unsupported passive/effect/trigger node owner kinds until runtime adapters exist, and tightening schema enum param validation.
- 2026-06-17: Code Builder implemented Phase C first sample migration for `rin-d`, added execute multiplier plan-op support, added duplicate guard validation for supported legacy+normalized behavior overlap, and verified the migrated sample through CSV checks, dotnet builds, Unity CSV validation, and runtime catalog plan inspection.
- 2026-06-17: Code Builder implemented Phase D representative choice-family migration, moved selected `monster_skill_choices.csv` behavior values into normalized choice-owned nodes/params, mapped generic choice nodes back into `SkillChoiceEffectSpec`, extended duplicate guards, and verified CSV shape, builds, Unity CSV sync, and runtime choice/node smoke checks.
- 2026-06-18: Code Builder fixed the Phase D Reviewer finding by populating choice base metadata rows, making `BuildSkillChoices(...)` use base metadata for duplicate rows and support base-only normalized choice rows, extending validation, and re-running CSV shape checks, dotnet builds, Unity CSV sync, and representative choice smoke checks.
- 2026-06-18: Code Builder started Phase E by applying the DATA board rule and Skill Builder exception-guide rule that new exception skill behavior defaults to normalized nodes, with old wide behavior columns treated as compatibility/deprecated inputs until Play Mode-proven migration coverage is sufficient; then verified CSV field counts, dotnet builds, Unity CSV sync, and console warning/error state.

## Task: 2026-05-31 Enemy Nexus Damage CSV Column

### Task title

Add `nexus_damage` to active stage enemy source CSVs and route it into enemy runtime data.

### Goals

- Add an authored Nexus damage value for Stage 1 and Stage 2 enemies.
- Keep current enemies at 1 Nexus damage by default.
- Keep CSV validation and runtime catalog sync passing after the schema extension.

### Constraints

- Role Owner is Code Builder.
- The authored header uses existing snake_case CSV style: `nexus_damage`.
- Parser lookup is case-insensitive, but active source authority records the snake_case column name.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, shape-checked, validated, and synced.

### Next Actions

- Change `nexus_damage` values in `stage_one_enemies.csv` or `stage_two_enemies.csv` when enemy-specific Nexus damage is designed.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` and `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv` now include `nexus_damage` with value `1` on current enemy rows.
- PowerShell field-count verification returned 27 header fields and no bad rows for both enemy CSV files.
- PowerShell `Import-Csv` verification found no blank/invalid `nexus_damage` values in either enemy CSV.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.EnemyDataset.cs` reads optional `nexus_damage` with default `1`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `NexusDamage` into `EnemyDefinition`.
- Unity menu `Pakuri/Validate CSV Source Data` logged a runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged a sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-31: User requested a Nexus damage column for Stage 1 and Stage 2 enemy CSVs.
- 2026-05-31: Code Builder added `nexus_damage`, mapped it through source CSV parsing/build, and synced runtime catalog assets.

## Task: 2026-05-31 Stage2 Enemy Runtime Catalog And Stage Flow Connection

### Task title

Connect Stage 2 enemy source data to the runtime CSV catalog and active stage-flow CSVs.

### Goals

- Add Stage 2 enemy catalog/source loading beside the existing Stage 1 enemy runtime path.
- Author Stage 2 day, encounter, and reward rows so `RunSession` stage advance can find Stage 2 data.
- Keep Stage 1 enemy sprite paths valid after the old `Assets/Image/Stage1/Enemy` path was no longer present.

### Constraints

- Role Owner is Code Builder.
- Stage 2 reward numbers currently copy the Stage 1 reward pattern because no separate Stage 2 reward-balance source was provided.
- Stage 2 unit sprite paths remain blank; prefab visuals are connected through `NewRunScene` enemy prefab bindings.

### Role Owner

Code Builder

### Status

Implemented and Unity CSV validation passed.

### Next Actions

- User verifies Stage 2 progression and spawn feel in Play Mode.
- Replace copied Stage 2 reward values when Stage 2-specific economy balance is authored.

### Evidence

- `Pakuri/Assets/CSVdata/source/catalog_stage_two_enemies.csv` now contains 8 Stage 2 enemy catalog entries.
- `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv` now uses runtime-supported Stage 2 passive IDs such as `FireDefenseUp`, `LightningDamageUp`, `IceDefenseUp`, and `HolyDefenseUp`.
- `Pakuri/Assets/CSVdata/StageDay.csv` now contains 11 `stage=2` rows.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` now contains 30 `stage2-*` encounter rows.
- `Pakuri/Assets/CSVdata/StageReward.csv` now contains `reward-stage2-normal`, `reward-stage2-midboss`, `reward-stage2-day10-midboss`, and `reward-stage2-boss`.
- CSV field-count check returned no bad rows for `stage_one_enemies.csv`, `stage_two_enemies.csv`, `catalog_stage_two_enemies.csv`, `StageDay.csv`, `StageEncounter.csv`, and `StageReward.csv`.
- Reference check returned `missingDayEncounter=`, `missingDayReward=`, `missingEncounterEnemy=`, `stage2Days=11`, and `stage2Encounters=30`.
- Unity `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog ... with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.`

### History

- 2026-05-31: User requested implementation of the Stage 2 passive/runtime/prefab/stage-flow connection after confirming `stage2-holy-priest.prefab` had the required actor and collider components.
- 2026-05-31: Code Builder connected Stage 2 source CSVs to the runtime catalog, added Stage 2 stage-flow rows, and corrected moved Stage 1 sprite paths to the existing `Assets/Enemy/Stage1/Enemy/Stage1/*.png` assets so CSV validation could complete.

## Task: 2026-05-31 Stage2 Enemy Data-Only Source CSV

### Task title

Create a data-only `stage_two_enemies.csv` source file using the current `stage_one_enemies.csv` shape.

### Goals

- Add the Stage 2 enemy reference data without connecting it to the runtime catalog yet.
- Keep the column layout identical to `stage_one_enemies.csv`.
- Copy `stage_one_skill`, `basic_skill`, `passive_skill_name`, `passive_skill_id`, and `passive_skill_value` from `stage_one_enemies.csv` by row order.
- Fill `passive_summary` from `Pakuri/reference/5.enemy/stage-2-enemies.md`.

### Constraints

- Role Owner is Code Builder.
- The new CSV is intentionally runtime-unconnected data only.
- No runtime catalog, source catalog asset, enum, skill, prefab, scene, or encounter wiring was changed.
- Stage 2 sprite paths remain blank because no Stage 2 sprite asset paths were provided or inspected for this task.

### Role Owner

Code Builder

### Status

Implemented and local CSV shape verified.

### Next Actions

- If Stage 2 should become runtime-loaded later, add explicit runtime catalog/source-catalog support and a Stage 2 encounter/spawn path instead of assuming this data-only CSV is loaded.
- Later Stage 2 runtime work should decide whether enemy skills stay on `StageOneEnemySkillKind` placeholders or move to a stage-neutral enemy skill id path.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv` was absent before creation; `Test-Path` returned `False` for both `stage_two_enemies.csv` and the typo path `stage_two_enemiese.csv`.
- `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv:1` now matches the `stage_one_enemies.csv` header.
- `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv:3` through `:10` contain the eight Stage 2 enemy rows from `Pakuri/reference/5.enemy/stage-2-enemies.md`.
- PowerShell field-count verification returned `header=26 rows=10 bad=`.
- PowerShell comparison against `stage_one_enemies.csv` returned `copied=True` for all eight Stage 2 rows for `stage_one_skill`, `basic_skill`, `passive_skill_name`, `passive_skill_id`, and `passive_skill_value`.

### History

- 2026-05-31: User requested a data-only Stage 2 enemy CSV, same shape as `stage_one_enemies.csv`, with selected Stage 1 skill/passive columns copied and only `passive_summary` adapted from the Stage 2 reference.
- 2026-05-31: Code Builder created `stage_two_enemies.csv` without runtime hookup.

## Task: 2026-05-31 Vega F-J Passive Shared CSV Authoring And Effect-Header Normalization

### Task title

Author Vega F-J passive rows on the active CSV authority and normalize the passive-effect CSV schema so the new shared columns validate in Unity.

### Goals

- Keep Vega F-J passive implementation inside the active source CSV files instead of adding a Vega-only companion table.
- Author the required passive base/effect/trigger rows for Vega F-J.
- Keep the new generic effect schema aligned between authored rows, header/type rows, and Unity runtime import.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV scope stayed limited to `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, and the already-active `monster_skills.csv`.
- No new CSV file was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and validation passed.

### Next Actions

- Reuse `required_source_status_id` / `required_source_status_min_stacks` on passive effect rows before adding new aura-specific CSV tables.
- Reuse `status_conditional_incoming_skill_runtime_kinds` and `status_conditional_outgoing_skill_runtime_kinds` for future runtime-kind-specific damage modifiers before adding skill-specific hardcoding.
- When bulk-editing imported CSV outside Unity, force an editor asset refresh before trusting `TextAsset`-backed validation results.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header/type rows now contain 70 columns and include the new generic fields `required_source_status_id`, `required_source_status_min_stacks`, `status_conditional_incoming_skill_runtime_kinds`, and `status_conditional_outgoing_skill_runtime_kinds`.
- The same effect CSV now contains the Vega passive rows that were previously absent:
  - Vega-F at lines 114-117.
  - Vega-G at lines 118-120.
  - Vega-H at lines 122-125.
  - Vega-I at lines 126-131.
  - Vega-J at lines 132-133.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains the Vega passive trigger rows at lines 46-58, including the `event_skill_runtime_kinds=Area` filter used by `vega-i-area-cooldown-base`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains the Vega F-J `RuntimeImplemented` enhancement rows at lines 189-203 and the missing Vega-H `PassiveBase` row `vega-h-base-duration` at line 254.
- The first Unity validation pass failed with `CsvFatalException: CSV file 'monster_skill_effects.csv' row 114 has 70 columns but expected 66.`
- After normalizing the effect CSV header/type rows and forcing a Unity asset refresh, Unity menu `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` then logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Vega F-J passive implementation added new generic effect fields and the final Vega passive rows in the active CSV set.
- 2026-05-31: The first Unity validation attempt exposed that `monster_skill_effects.csv` data rows had already widened to 70 columns while the header/type rows were still 66 columns.
- 2026-05-31: Code Builder normalized the effect CSV schema, forced Unity asset refresh, and re-ran validation/sync successfully.

## Task: 2026-05-31 Vega-D Active Row Re-authoring For Overlap And Delayed Repeats

### Task title

Re-author the active Vega-D skill and master-1 choice rows so overlapping local AoE hits and delayed per-target repeats are expressed entirely in the existing CSV authority.

### Goals

- Keep Vega-D on `monster_skills.csv` and `monster_skill_choices.csv` without adding a new CSV column or a new companion table.
- Express overlap-enabled local fanout through `hit_target_count=global`.
- Express base plus two delayed extra hits through `repeat_count_per_target=2` and `repeat_interval_seconds=0.5`.

### Constraints

- Role Owner is Code Builder.
- Active CSV authority stayed limited to `monster_skills.csv` and `monster_skill_choices.csv` for this task.
- No CSV schema change was needed.

### Role Owner

Code Builder

### Status

Implemented, synced, and validation passed.

### Next Actions

- Reuse the same row pattern when another shared `SingleAttack` fanout skill needs overlap stacking at each resolved center.
- Reuse repeat-per-target authoring before adding a parallel trigger row when the desired pattern is still “immediate base hit plus delayed extra repeats at the same center.”

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` line `vega-d` now sets `hit_target_count=global` while preserving the existing marked-target fanout fields and prefab path.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` line `vega-d-master-1` now sets `description_text=각 표식 대상 위치에 범위 참격 2회 추가 발생, 각 참격 위력 -35%`, `repeat_count_per_target=2`, and `repeat_interval_seconds=0.5`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` confirmed `vega-d.hit_target_count=global`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` confirmed `vega-d-master-1.repeat_count_per_target=2`, `repeat_interval_seconds=0.5`, and `repeat_damage_multiplier=1`.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Earlier same-day Vega-D row authoring had left `hit_target_count` blank and `repeat_count_per_target=1`, which matched the temporary single-target local-hit interpretation.
- 2026-05-31: User then requested overlap-enabled area damage and two delayed extra slashes, so Code Builder updated the active rows without widening schema or runtime scope.

## Task: 2026-05-31 Vega-E Shared Choice/Skill CSV Extension And Active Row Authoring

### Task title

Extend the active monster skill CSV schema for reusable marked-target execution data, then author Vega E on that shared path.

### Goals

- Keep Vega E on the active `monster_skills.csv` and `monster_skill_choices.csv` authority instead of introducing a Vega-only companion table.
- Add only the shared columns needed for marked-target selection, target-status-stack damage, partial target-status consumption, conditional crit, and consumed-status redistribution.
- Keep unsupported row state explicit when reference authority is still incomplete.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV authority stayed limited to `monster_skills.csv` and `monster_skill_choices.csv` for this task.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and validation passed. Vega-E trait-5 row is now fully authored with the user-provided nearby-search values.

### Next Actions

- Reuse the new target-selection, target-stack-damage, consume, conditional-crit, and redistribution columns for future shared marked-target finishers before adding another skill CSV.
- Reuse the same `redistribute_consumed_status_search_radius` plus `redistribute_consumed_status_target_count` pair when future skills need bounded redistribution instead of inventing a new spread schema.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` header now includes `target_selection_status_id`, `target_selection_status_min_stacks`, `target_status_stack_status_id`, `target_status_stack_max_stacks`, `target_status_stack_base_damage`, `target_status_stack_attack_power_coefficient`, `target_status_stack_spell_power_coefficient`, `consume_target_status_id`, `consume_target_status_ratio`, and `consume_target_status_stacks`.
- The same skill CSV now authors `vega-e` with `target_selection=HighestStacks`, `target_selection_status_id=name-mark`, `target_selection_status_min_stacks=1`, `target_status_stack_status_id=name-mark`, `target_status_stack_base_damage=6`, `target_status_stack_attack_power_coefficient=0.18`, `consume_target_status_id=name-mark`, `consume_target_status_ratio=0.5`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header now includes `target_status_stack_damage_multiplier`, `consume_target_status_ratio_override`, `consume_target_status_stacks_override`, `conditional_crit_chance_bonus`, `conditional_crit_target_status_id`, `conditional_crit_target_status_min_stacks`, `redistribute_consumed_status_ratio_on_kill`, `redistribute_consumed_status_id`, `redistribute_consumed_status_search_radius`, and `redistribute_consumed_status_target_count`.
- The same choice CSV now authors `vega-e-trait-1`, `trait-2`, `trait-3`, `trait-4`, `trait-5`, `master-1`, and `master-2` as `RuntimeImplemented`; `vega-e-trait-5` now includes `redistribute_consumed_status_ratio_on_kill=0.25`, `redistribute_consumed_status_id=name-mark`, `redistribute_consumed_status_search_radius=100`, and `redistribute_consumed_status_target_count=1`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` confirmed the corrected Vega E row alignment after the first failed validation pass.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Code Builder first extended the shared active CSV schema for Vega E and authored the active rows.
- 2026-05-31: Initial Unity validation exposed a malformed Vega E row alignment in `monster_skill_choices.csv`; Builder corrected the row shape and re-ran validation/sync successfully.
- 2026-05-31: User then supplied the remaining trait-5 nearby-search authority and final Vega-E prefab path, so Skill Builder finished the active row authoring without another schema change.

## Task: 2026-05-30 Vega C/D Shared CSV Schema Extension And Active Row Authoring

### Task title

Extend the active monster skill CSV schema for reusable buff-active and marked-target fanout behavior, then author Vega C and Vega D on that shared data path.

### Goals

- Keep the new Vega C and Vega D behavior owned by the existing active CSV authority instead of adding a Vega-only file.
- Add only the shared columns needed for attached buff scalar overrides, source-status-gated modifiers, repeat-per-target fanout, and marked-target deployment filtering.
- Connect the user-provided Vega C/D prefab paths in the active skill rows.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV authority stayed limited to `monster_skills.csv` and `monster_skill_choices.csv` for this task.
- No new CSV file was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and validation passed.

### Next Actions

- Reuse `deployment_required_target_status_id` plus `deployment_required_target_status_min_stacks` for future shared marked-target fanout rows before inventing another deployment table.
- Reuse `runtime_target_skill_ids`, `required_source_status_id`, `status_action_speed_bonus`, `status_attack_power_bonus`, and repeat-per-target choice columns for future buff-active follow-up rules before adding another companion CSV.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` header now includes `deployment_required_target_status_id` and `deployment_required_target_status_min_stacks`.
- The same skill CSV now authors `vega-c` with `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_C.prefab`.
- The same skill CSV now authors `vega-d` with `runtime_kind=SingleAttack`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_D.prefab`, `deployment_required_target_status_id=name-mark`, and `deployment_required_target_status_min_stacks=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header now includes `runtime_target_skill_ids`, `required_source_status_id`, `required_source_status_min_stacks`, `status_action_speed_bonus`, `status_attack_power_bonus`, `repeat_count_per_target`, `repeat_interval_seconds`, and `repeat_damage_multiplier`.
- The same choice CSV now marks `vega-c-trait-2`, `vega-c-trait-3`, `vega-c-trait-4`, `vega-c-trait-5`, `vega-c-master-1`, `vega-c-master-2`, `vega-d-trait-5`, and `vega-d-master-1` as shared-runtime-backed rows instead of the prior unsupported/partial state, and it remaps `vega-d-trait-4` to conditional `name-mark >= 10` damage instead of plain unconditional `1.3x`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate the new shared columns.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-30: Code Builder first implemented the shared runtime contract requested by the Vega handoff, then Skill Builder authored the active Vega C and Vega D rows on the new shared CSV fields and synced the runtime catalog.

## Task: 2026-05-28 Vega-B Follow-up Trigger Row Re-authored To LineAttack

### Task title

Re-author the active Vega-B master-1 delayed follow-up row from trigger `SingleAttack` to explicit trigger `LineAttack` so the CSV authority matches the intended aimed-slash runtime path.

### Goals

- Keep the active source CSV explicit about the follow-up slash runtime kind and trigger action.
- Preserve the existing authored payload, delay, prefab path, and linked silence effect.
- Keep source validation aligned with the new explicit trigger action path.

### Constraints

- Role Owner is Code Builder.
- Edited source authority is limited to `monster_skill_triger.csv` plus the shared CSV validator/runtime definitions needed to accept explicit trigger `LineAttack`.
- No new CSV file or new CSV column was added.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Future delayed aimed slashes should prefer explicit `trigger_action=LineAttack` when they are authored as direct trigger payloads, not as helper-skill re-casts.
- Keep `triggered_skill_id` non-empty on trigger rows because the current CSV parser still requires that field even when the direct trigger action does not use it at runtime.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `runtime_kind=LineAttack`, `trigger_action=LineAttack`, `base_damage=30`, `attack_power_coefficient=1.4`, `damage_multiplier=0.45`, `radius=1.8`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`, and `triggered_effect_id=vega-b-master1-second-silence`.
- The same trigger row still keeps a non-empty `triggered_skill_id=vega-b`, which matches the current parser contract in `PakuriCsvRuntimeData.MonsterDataset.cs`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now treats explicit trigger `LineAttack` rows like trigger `SingleAttack` rows for positive payload checks, which keeps source validation aligned with the shared direct trigger line path.
- Unity menu `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary after the row update.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-28: Base `vega-b` had already been returned to `LineAttack`, but the delayed master-1 follow-up row still remained on the old trigger `SingleAttack` authoring pattern until the user requested parity.

## Task: 2026-05-28 Vega-B Base Runtime Kind Reverted To LineAttack

### Task title

Re-author the active Vega-B base row as `LineAttack` after user-facing validation showed the `SingleAttack` path produced a self-centered slash presentation.

### Goals

- Keep the active source CSV aligned with the intended aimed-slash presentation.
- Reuse the current `LineAttack` data contract without adding a new column or helper row.
- Sync the runtime catalog after the row change.

### Constraints

- Role Owner is Code Builder.
- Edited source authority is limited to `monster_skills.csv`.
- No new CSV column or new CSV file was introduced.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- If future Vega-B work also needs the master-1 second slash to rotate as a beam, handle that as a separate trigger-path decision instead of assuming the base-row revert solves the follow-up trigger row too.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b` with `runtime_kind=LineAttack`, `radius=1.8`, `cooldown_seconds=8`, `active_duration_seconds=0`, `shot_interval_seconds=0`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- PowerShell CSV readback confirmed the current active row values for `vega-b` exactly as `runtime_kind=LineAttack`, `active_duration_seconds=0`, `shot_interval_seconds=0`, and empty `hit_target_count`.
- Unity menu `Pakuri/Validate CSV Source Data` completed after the row change, and the console logged the runtime catalog load summary instead of a CSV failure.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` completed and the console logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-28: The prior SingleAttack row had solved path-contact damage behavior but still presented as a self-cast slash because the shared SingleAttack prefab path does not rotate toward the target.

## Task: 2026-05-28 Trigger SingleAttack Fixed Payload Validation Alignment

### Task title

Align trigger `SingleAttack` source validation with the real runtime damage contract and correct the Vega-B follow-up trigger row.

### Goals

- Prevent false assumptions that `damage_multiplier` alone is enough for trigger-routed `SingleAttack` damage rows.
- Keep source validation aligned with runtime damage resolution, which accepts base damage or positive stat coefficients.
- Correct `vega-b-master1-second-slash` so it both validates and deals the intended nonzero damage.

### Constraints

- Role Owner is Code Builder.
- Edited source authority is limited to `monster_skill_triger.csv` and the shared CSV validator.
- No new CSV column or new CSV file was added.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Future trigger-routed `SingleAttack` rows should include explicit payload evidence in the handoff: `base_damage`, coefficients, `damage_multiplier`, and `damage_source`.
- Do not rely on `damage_multiplier` as an implicit source-skill damage reuse rule for trigger rows.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `base_damage=30`, `attack_power_coefficient=1.4`, and `damage_multiplier=0.45`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now validates `Fixed` trigger `SingleAttack` rows with the same positive payload rule already used by shared damage effect rows: positive `base_damage` or positive `attack/spell` coefficient.
- Unity menu `Pakuri/Validate CSV Source Data` completed after the fix, and the console returned the runtime catalog load summary instead of the previous Vega-B validation failure.

### History

- 2026-05-28: Vega-B master-1 follow-up was first authored with `damage_multiplier=0.45` but zero base/coefficient payload, which both failed source validation and would have resolved to zero runtime damage.

## Task: 2026-05-28 Vega-B Shared Trigger Status Data Authoring

### Task title

Author the active CSV rows required for Vega-B silence slash follow-ups on the shared triggered `SingleAttack` path.

### Goals

- Keep Vega-B fully authored in the active CSV source without a hidden follow-up skill slot.
- Reuse existing active CSV tables for the second slash trigger row and linked silence/name-mark effect rows.
- Keep master-2 silence extension authored through existing threshold and status-duration fields.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV authority stayed limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `status_effects.csv`.
- The shared runtime/common-logic extension was user-approved before implementation.
- No new CSV file or new CSV column was introduced.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- Reuse the same row pattern when a follow-up slash needs delayed trigger damage plus a linked OnHit status effect: trigger row in `monster_skill_triger.csv` plus a linked `Status` `OnHit` effect row in `monster_skill_effects.csv`.
- Keep `silence` default duration at `4s` unless another inspected skill now needs a different shared base.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b` with `hit_target_count=global`, `status_effect_id=silence`, `status_duration_seconds=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `vega-b-trait-2` through `status_duration_bonus_status_id=silence` / `status_duration_bonus=1`, and `vega-b-master-2` through `threshold_status_id=name-mark`, `threshold_status_min_stacks=10`, and `threshold_apply_status_id=silence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `vega-b-trait5-name-mark` and `vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `vega-b-master1-second-slash` with `runtime_kind=SingleAttack`, `trigger_action=SingleAttack`, `damage_multiplier=0.45`, `trigger_delay_seconds=0.4`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`, and `triggered_effect_id=vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now sets shared `silence` default duration to `4`, which lets the master-2 silence refresh land at `4s` and trait-2 plus master-2 combine to `5s` without a new status id.

### History

- 2026-05-28: The user first proposed reusing a separate helper skill row for Vega-B second slash, but current active-slot validation and learned-runtime loading made the shared trigger/effect row approach smaller and more aligned with existing active CSV authority.

## Task: 2026-05-28 Vega-A Projectile Shared Runtime Extension

### Task title

Add the active CSV schema and shared runtime support required to author Vega-A burst cadence, burst-index damage, and follow-up shadow projectiles.

### Goals

- Keep Vega-A authorable in the active CSV source without adding a Vega-only table.
- Extend the shared projectile path so burst-internal timing, per-burst-hit modifiers, and follow-up projectiles are data-driven.
- Keep master-2 authored on the shared trigger/effect path using the later user-provided slash coefficient and prefab path.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV authority stayed limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- The shared runtime/common-logic extension was user-approved before implementation.
- No new CSV file was introduced.
- The missing Vega-A master-2 slash value was later provided by the user as `attack coefficient 0.5`, so the active effect row could be completed without widening scope.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and Unity editor-validated.

### Next Actions

- Reuse `burst_interval_seconds`, `burst_damage_projectile_index`, and `burst_damage_multiplier` for future projectile-burst skills before adding another per-projectile schema.
- Reuse `follow_up_projectile_count`, `follow_up_projectile_delay_seconds`, and `follow_up_projectile_damage_multiplier` for future delayed shadow/follow-up projectile choices.
- Reuse the existing shared `Damage` effect row path when a triggered effect must deal damage and apply status together; a separate Vega-only hybrid effect type was not needed.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` header now includes `burst_interval_seconds`, `burst_damage_projectile_index`, and `burst_damage_multiplier`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header now includes `burst_damage_projectile_index`, `burst_damage_multiplier`, `follow_up_projectile_count`, `follow_up_projectile_delay_seconds`, and `follow_up_projectile_damage_multiplier`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now authors `vega-a-master2-transfer-mark` as `effect_kind=Damage`, `attack_power_coefficient=0.5`, `status_stack_amount=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` already applies `ResolveStatusSpec(...)` from `SkillMultiEffectKind.Damage`, so the same shared row can deal damage and apply `name-mark` without a new effect kind.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now treats positive `base_damage` or positive `attack/spell` coefficient as valid payload for shared `Damage` effect rows, fixing the false failure on coeff-only effect rows such as `vega-a-master2-transfer-mark`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` now parses those new columns from active skill and choice rows.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `.../Skills/Data/SkillData.cs`, `.../Skills/Data/SkillChoiceEffectSpec.cs`, `.../Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, and `.../Skills/Data/InGameSkillDefinitionMapper.cs` now carry the new data into runtime definitions and snapshots.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now resolves burst-internal cadence separately from outer cast cadence.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` and `.../Execution/Executors/ProjectileSkillExecutor.cs` now resolve shared burst-index damage rules and execute follow-up projectiles after the triggering burst hit.
- Unity refresh completed after the new CSV schema and rows, and the filtered Unity console returned no Vega CSV/runtime errors after correcting the `triggered_skill_id` contract on `vega-a-master2-kill-transfer` and later filling the master-2 slash payload.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the pre-existing `MSB3277` warnings remained.

### History

- 2026-05-28: Shared runtime work started after user approval to implement the three extension points first under Code Builder, then continue Vega-A under Skill Builder.
- 2026-05-28: User later completed the missing Vega-A master-2 authority with `attack coefficient 0.5` and `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`, so the existing shared triggered-effect data path was finalized without more code changes.
- 2026-05-28: Unity source validation exposed that coeff-only `Damage` effect rows were blocked by a stale `base_damage > 0` rule even though runtime damage already resolves from coefficients; Builder aligned the shared validator to the actual runtime contract and revalidated successfully.

## Task: 2026-05-28 Sein Passive Shared Runtime Data Completion

### Task title

Finish the remaining Sein passive data that depended on new shared passive-base and triggered-cast runtime support.

### Goals

- Author the shared-runtime-backed CSV rows for Sein-I base and Sein-G trait-3.
- Keep the active CSV authority aligned with the new shared runtime behavior without adding a new CSV file.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Edited data files are `monster_skill_choices.csv` and `monster_skill_triger.csv`; Unity sync updates the generated runtime catalog asset.
- Shared runtime code was extended, but no new CSV file was introduced.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and Unity editor-validated.

### Next Actions

- Reuse `PassiveBase` choice rows for future learned-passive base modifiers before adding a new passive-base schema.
- Reuse the triggered-cast origin marker path when a passive must react only to a triggered child skill cast.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains `sein-i-base-shot-interval` with `choice_group=PassiveBase`, `target_skill_id=sein-d`, and `shot_interval_multiplier=0.8`.
- The same choice CSV now marks `sein-g-trait-3` `RuntimeImplemented` and removes the prior blocker note.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `sein-g-auto-barrage-reload-trait3`, which reduces `sein-a` reload by `0.10` on `OnSkillCast` of `sein-b` gated by Sein-G origin.
- CSV field-width parsing succeeded after the new rows: choices `columns=89 lines=253`, trigger `columns=44 lines=43`.
- Unity validation and runtime catalog sync both succeeded after the shared runtime and data changes.

### History

- 2026-05-28: Added the shared-runtime-backed Sein-I base and Sein-G trait-3 CSV rows and validated them through the Unity editor.

## Task: 2026-05-27 Sein Passive CSV-Only Runtime Data Authoring

### Task title

Author and sync the existing-runtime CSV data required for the CSV-solvable portion of Sein passives F, H, I, and J.

### Goals

- Add the status-effect and trigger data already supported by current runtime paths.
- Record active-skill choice routing needed for choice-gated Sein passive effects.
- Keep shared-runtime-only behavior out of this data pass.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Edited source authority is limited to `monster_skill_effects.csv`, `monster_skill_choices.csv`, and `monster_skill_triger.csv`.
- Unity sync writes the generated runtime catalog asset; no new CSV schema or shared runtime logic is added here.
- Excluded behavior is `sein-i` base tick-speed `+20%` and exact `sein-g-trait-3` auto-trigger source identification.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and Unity editor-validated for the routed CSV-only data.

### Next Actions

- After the approved shared runtime work exists, add only the data required for `sein-i` base tick speed and `sein-g-trait-3`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains 12 new Sein passive status effect rows for F/H/I/J using existing `passive-buff`, `fire-resist-down`, and `fire-exposure` runtime kinds.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks the authored F/H/I/J trait rows `RuntimeImplemented` and supplies target active skill routing where active snapshots require it.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains 12 Sein-J `OnKill` action rows using existing `CooldownRefund` and `ReloadReduce` behavior.
- CSV field-width parsing succeeded after edits: effects `columns=66 lines=110`, trigger `columns=44 lines=42`, choices `columns=89 lines=252`.
- Unity `Pakuri/Validate CSV Source Data` loaded the runtime catalog successfully, and `Pakuri/Sync CSV Runtime Catalog Assets` reported successful synchronization to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-27: Added and validated the CSV-only Sein passive F/H/I/J data pass; left the two shared-runtime behaviors excluded by scope.

## Task: 2026-05-27 Zero-Damage Persistent Zone CSV Validation Rule

### Task title

Adjust active CSV validation rules so status-only persistent `monster_skill_effects.csv` damage rows can remain zero-damage.

### Goals

- Keep active effect CSV authoring free of fake `base_damage` values for presence-only persistent zones.
- Preserve positive-damage requirements for normal damage rows.
- Validate the new rule through the actual Unity CSV validation menu.

### Constraints

- Role Owner is Code Builder.
- The change is a shared validation-rule adjustment, not a new CSV schema.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Future effect rows that are authored as zero-damage persistent status zones should match the shared rule exactly: persistent timing, status payload, and zero coefficients.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now exempts only status-only persistent zones from the unconditional positive-`base_damage` rule for `SkillMultiEffectKind.Damage`.
- Active CSV evidence remains `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` rows `sein-d-zone-presence` and `sein-e-master2-zone-presence`, both authored with `base_damage=0`.
- Unity menu `Pakuri/Validate CSV Source Data` succeeded after the fix and logged the runtime catalog load summary instead of the earlier validation failure.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 code errors; only the existing `MSB3277` warnings remained.

### History

- 2026-05-27: Sein-E / Sein-D presence zones exposed that the active validation rule was too strict for zero-damage persistent status-refresh rows.

## Task: 2026-05-27 Sein-C/D Delayed Projectile And Residual Zone CSV Authoring

### Task title

Extend the active skill/effect/status CSV authority required for Sein-C delayed projectile behavior and Sein-D residual zone behavior.

### Goals

- Keep Sein-C delayed impact, projectile delay tuning, and follow-up effects authored in the active CSV files.
- Keep Sein-D residual ember zone authored in the active effect CSV instead of a helper skill row.
- Keep new schema additions reusable for future skills.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- User explicitly approved widening scope to shared runtime/common-logic extension and new CSV columns when required.
- `monster_skill_choices.csv damage_delay_multiplier` and `monster_skill_effects.csv active_duration_seconds / tick_interval_seconds` are now part of the active authoring authority for this runtime path.
- Some effect values remain explicit inferences until a stronger authority is provided:
  - `sein-c-master-1` residual zone radius `1.2`, tick `0.5s`
  - `sein-d-master-2` residual zone radius `3.2`, tick `0.5s`
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- Reuse `damage_delay_multiplier` for future projectile delay tuning before adding another choice field.
- Reuse `active_duration_seconds` and `tick_interval_seconds` in effect rows for future persistent follow-up zones before creating helper active-skill rows.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header now includes `damage_delay_multiplier`; `sein-c-trait-4` uses `0.6`.
- The same choice CSV now authors Sein-C trait/master and Sein-D trait/master rows on shared fields, including conditional status damage for `sein-c-trait-5` and `sein-d-trait-5`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header now includes `active_duration_seconds` and `tick_interval_seconds`.
- The same effect CSV now contains `sein-c-master2-contact`, `sein-c-master1-zone`, and `sein-d-master2-zone`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `sein-c` as `CooldownProjectile` with `damage_delay_seconds=0.8` and authors `sein-d` with active duration, tick interval, and status payload values used by the shared runtime.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains the shared Sein status rows required by those choices.
- Unity menu execution for `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` produced filtered console logs `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-05-27: Sein-C and Sein-D required active CSV authoring support for projectile-delay tuning and effect-authored residual zones; the user approved the necessary schema widening.

## Task: 2026-05-26 Rin-E SingleAttack Core Hitbox CSV Schema

### Task title

Extend active skill CSV authority for SingleAttack prefab core-hitbox effects and Rin-E authoring.

### Goals

- Add a base active-skill prefab path column so active skill rows can provide `SkillEffectPrefab`.
- Add shared choice columns for prefab core-hitbox damage, core-hitbox additional damage, and hit-count cooldown refund.
- Author Rin-E enhancement and master rows as `RuntimeImplemented`.
- Add Rin-E master-2 slow as a choice-gated OnHit status row in `monster_skill_effects.csv`.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- CSV source remains the active authority; no Rin-only companion table was added.
- CSV files were exported as UTF-8.
- Unity CSV runtime catalog sync is pending because batchmode reported another Unity instance has this project open.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and synced through the open Unity Editor menu after the follow-up CSV validation fix.

### Next Actions

- Reuse `core_hitbox_name`, `core_damage_multiplier`, `core_on_hit_additional_damage_*`, and `hit_count_cooldown_refund_*` for future SingleAttack prefab-center effects before adding another schema.
- User verifies Rin-E master 2 slow behavior in Play Mode.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has 57 columns and `rin-e.skill_effect_prefab_path=Assets/Prefab/Skill/Rin/Rin_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now has 86 columns including the shared core-hitbox and hit-count cooldown refund fields.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now has 77 parsed rows and contains `rin-e-master2-slow`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `PakuriCsvRuntimeData.AssetReferences.cs` now parse, map, and collect the base `skill_effect_prefab_path` and new choice fields.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `SkillChoiceEffectSpec.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSnapshot.cs` now carry the new shared choice fields into runtime snapshots.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed only because Unity batchmode reported another Unity instance has `C:/TowerDefence_Pakuri/Test/Pakuri` open.
- Follow-up enum validation found the `DamageAttribute` enum defines `Darkness`, not `Dark`; `monster_skill_choices.csv` and `monster_skill_effects.csv` Rin-E rows were corrected to `Darkness`, and a CSV enum scan returned `ENUM_VALIDATION_OK`.
- Follow-up status-scope validation found `StatusEffectRuntime.TryParseStatusTargetScope(...)` only accepts `self` and `all_allies`; `rin-e-master2-slow` now leaves `status_target_scope` blank like other enemy OnHit status rows, while `target_side=Enemy` remains the target authority.
- `.NET TextFieldParser` scans returned `FIELD_COUNT_OK` for `monster_skill_effects.csv` 61 columns / 78 lines, `monster_skill_choices.csv` 86 columns / 252 lines, `monster_skills.csv` 57 columns / 52 lines, and `monster_skill_triger.csv` 34 columns / 10 lines.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the fix.

### History

- 2026-05-26: User requested full Rin-E Skill Builder implementation with the SingleAttack blueprint and `Assets/Prefab/Skill/Rin/Rin_E.prefab`.
- 2026-05-26: User reported Unity auto-sync failing on `monster_skill_effects.csv` row 78 because `attribute=Dark` was not a valid enum value; Builder corrected the CSV enum values and checked for remaining enum mismatches.
- 2026-05-26: User reported Unity CSV validation still failing on `rin-e-master2-slow status_target_scope=enemy`; Builder cleared that unsupported scope, verified the relevant CSV schemas and enum/status-scope scans, and synced the runtime catalog through the open Unity Editor menu.

## Task: 2026-05-26 SingleAttack Damage Delay CSV Schema

### Task title

Add `damage_delay_seconds` to active monster skill CSV and carry it into SingleAttack runtime data.

### Goals

- Let `Pakuri/Assets/CSVdata/source/monster_skills.csv` author per-skill SingleAttack hit delay.
- Default every existing monster skill row to `0` so current immediate-hit behavior remains unchanged until rows are tuned.
- Carry the field through `SkillRow`, `SkillDefinition`, `SingleAttackData`, validation, and mapper code.

### Constraints

- Role Owner is Code Builder.
- CSV source remains the active authority; no companion table was added.
- Existing row count and quoted CSV structure must remain parseable.
- Unity batchmode catalog sync could not complete while another Unity instance had the same project open.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Runtime catalog asset sync is pending through the open Unity Editor menu or a later batch sync after closing Unity.

### Next Actions

- Tune `damage_delay_seconds` values in `monster_skills.csv` for specific SingleAttack rows.
- Sync runtime catalog assets once Unity project locking allows it.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `damage_delay_seconds` after `knockback_distance`; every existing data row is `0`.
- CSV parser verification returned `records=52`, `fields=56 records=52`, `damage_delay_index=50`, `type=float`, and `nonzero_defaults=0`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` parses optional `damage_delay_seconds`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `DamageDelaySeconds` into `SkillDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Skills/Data/SingleAttackData.cs`, `Skills/Data/InGameSkillDefinitionMapper.cs`, and `Skills/Data/InGameSkillDataValidator.cs` now carry and validate the value.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed with Unity's duplicate-project-open guard for `C:/TowerDefence_Pakuri/Test/Pakuri`.

### History

- 2026-05-26: User requested Code Builder implementation of Designer's N-second delayed SingleAttack hit timing plan with default CSV value `0`.

## Task: 2026-05-26 Rin-B/C Shared Beam Buff And Status CSV/Runtime Extension

### Task title

Extend the shared CSV/runtime contracts required to finish Rin-B and Rin-C on the active Scripts2 skill path.

### Goals

- Add shared beam knockback and per-hit reload-reduction choice data for Rin-C.
- Add shared effect/status payload fields for Rin-B master-2 style outgoing additional damage without passive-trigger ownership hacks.
- Keep Rin-B trait/master extra buffs and Rin-C master slow authored in the active CSV tables.

### Constraints

- Role Owner is Skill Builder.
- User explicitly approved current Rin CSV/reference files as the parsed source for this task.
- No Rin-only companion CSV table was added; the work stays inside `monster_skills.csv`, `monster_skill_choices.csv`, and `monster_skill_effects.csv`.
- CSV/runtime claims are grounded in inspected source rows and runtime mapper/executor code.

### Role Owner

Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- Reuse `knockback_distance`, `knockback_distance_multiplier`, `reload_reduce_target_skill_id`, and `reload_reduce_seconds_per_hit` for future beam/line skills before adding another schema.
- Reuse `status_outgoing_additional_damage_*` for future buff/status-authored extra-hit behavior before adding a trigger-only side table.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now includes `knockback_distance`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now includes `knockback_distance_multiplier`, `reload_reduce_target_skill_id`, and `reload_reduce_seconds_per_hit`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now includes `status_outgoing_additional_damage_multiplier`, `status_outgoing_additional_damage_trigger_attribute`, and `status_outgoing_additional_damage_attribute`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.StatusPayload.cs`, and `PakuriCsvRuntimeData.Build.cs` now parse and map those new columns.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Skills/Data/BeamSkillData.cs`, `Skills/Data/SkillChoiceEffectSpec.cs`, `Skills/Execution/Modifiers/SkillChoiceModifierRecord.cs`, and `Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carry the new shared Rin-B/C data through runtime snapshots.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` and `StatusEffectRuntime.cs` now carry status-authored outgoing additional damage fields keyed by `DamageAttribute`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.skill_id -in @('rin-b','rin-c') }` returned all Rin-B/C choice rows with `runtime_support_state=RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after the schema/runtime changes; existing MSB3277 warnings remain.

### History

- 2026-05-26: User approved the wider Rin CSV/reference inspection exception required by the Skill Builder boundary and requested full Rin-C then Rin-B implementation.

## Task: 2026-05-24 Skill On-Hit Additional Damage CSV Schema

### Task title

Add shared choice CSV fields for direct on-hit extra damage and every-nth-hit chain damage.

### Goals

- Keep on-hit extra damage authored in `monster_skill_choices.csv`.
- Keep Rin-A master-2 off the projectile `branch_*` launch override fields.
- Carry the new CSV fields through runtime source rows, `SkillChoiceDefinition`, `SkillChoiceEffectSpec`, and `SkillExecutionSnapshot`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- User provided the parsed Rin-A master-2 values in the request.
- CSV source stayed UTF-8 and imported successfully through Unity.
- No new companion CSV table was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and synced into runtime catalog assets.

### Next Actions

- Future skills needing direct hit-target extra damage should reuse `on_hit_additional_damage_*`.
- Future skills needing deterministic nth-hit nearby chain damage should reuse `on_hit_chain_*`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now includes `on_hit_additional_damage_chance`, `on_hit_additional_damage_multiplier`, `on_hit_additional_damage_attribute`, `on_hit_additional_damage_target`, `on_hit_chain_hit_period`, `on_hit_chain_target_count`, `on_hit_chain_search_radius`, `on_hit_chain_damage_multiplier`, `on_hit_chain_damage_attribute`, and `on_hit_additional_damage_visual`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` showed `rin-a-master-2` with `on_hit_additional_damage_chance=1`, `on_hit_additional_damage_multiplier=0.4`, `on_hit_chain_hit_period=3`, `on_hit_chain_target_count=2`, and blank branch chance/count/launch fields.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` parses the new optional columns.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, `SkillDefinition.cs`, `SkillChoiceEffectSpec.cs`, `SkillChoiceModifierRecord.cs`, and `SkillExecutionSnapshot.cs` carry the new fields into runtime choice snapshots.
- Unity-MCP editor execution returned `rin-a-master-2|extra=True:1:0.4:Lightning:HitTarget|chain=3:2:4.5:0.4:Lightning|branch=False:False:0:False`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged a successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-24: User requested the additional damage behavior as a common skill on-hit option rather than a projectile-only branch extension.

## Task: 2026-05-24 Rin-A Choice CSV Authoring

### Task title

Author Rin-A remaining choice behavior on the active `monster_skill_choices.csv` runtime authority.

### Goals

- Add reusable nth-projectile-launch branch override columns to the active choice CSV.
- Move Rin-A trait 5 from unsupported critical prose to shared critical bonus fields.
- Move Rin-A master 2 from unsupported prose to shared branch fields plus launch-period override fields.
- Preserve Rin-A master 1 on the already-supported damage, magazine, and shot-interval fields.

### Constraints

- Role Owner is Skill Builder.
- User explicitly approved current CSV/code as the parsed source.
- No new monster-specific companion table was added.
- CSV stayed UTF-8 and all rows now have the same 59-column shape.

### Role Owner

Skill Builder

### Status

Implemented and synced into runtime catalog assets.

### Next Actions

- Reuse `branch_launch_period` and `branch_launch_chance_set` for future projectile skills that need "every Nth projectile launch" branch chance overrides.
- Keep future critical projectile choices on `crit_chance_bonus` and `crit_damage_bonus` before adding new critical schema.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header/type rows now include `branch_launch_period` and `branch_launch_chance_set`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` showed `rin-a-trait-5` as `crit_chance_bonus=0.1`, `crit_damage_bonus=0.25`, and `RuntimeImplemented`.
- The same import showed `rin-a-master-2` as `branch_chance_set=0.4`, `branch_count=2`, `branch_damage_multiplier=0.4`, `branch_search_radius=4.5`, `branch_launch_period=3`, `branch_launch_chance_set=1`, and `RuntimeImplemented`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged a successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-24: User requested Skill Builder implementation for Rin-A master-2, remaining enhancements, and master-1 using current CSV/code as parsed source.

## Task: 2026-05-24 Eve F-J Passive Effect/Trigger CSV Schema And Authoring

### Task title

Extend shared passive effect/trigger CSV data so Eve F-J can stay fully data-authored on the current runtime catalog path.

### Goals

- Add shared effect columns for target-status-conditional status chance and status-id-specific applied-duration bonuses.
- Add shared trigger columns for condition status, attribute gating, proc chance, and internal cooldown.
- Re-author Eve F-J passive rows so the remaining `DataOnlyUnsupported` / `ReferenceDirect` Eve passive rows move onto shared runtime support.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Current CSV files were explicitly treated as the parsed source for this task.
- No new Eve-only CSV file was added; the work stayed inside `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `monster_skill_choices.csv`.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and Unity CSV validation passed.

### Next Actions

- Reuse `status_conditional_target_status_id` plus `status_conditional_status_chance_bonus` for future passive rows that say "extra status chance only against targets already carrying X".
- Reuse `status_applied_status_duration_bonus_status_id` plus `status_applied_status_duration_bonus` for future rows that extend only one applied status without editing global status defaults.
- Reuse `condition_status_id`, `trigger_attribute`, `proc_chance`, and `internal_cooldown_seconds` before adding another trigger companion table.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header/type rows now include `status_conditional_target_status_id`, `status_conditional_status_chance_bonus`, `status_applied_status_duration_bonus_status_id`, and `status_applied_status_duration_bonus`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` header/type rows now include `condition_status_id`, `trigger_attribute`, `proc_chance`, and `internal_cooldown_seconds`.
- Eve F-J rows in `monster_skill_choices.csv` are now all `RuntimeImplemented`; `eve-g-trait-3`, `eve-i-trait-3`, and `eve-j-trait-3` target the active skills they modify instead of staying passive-note-only.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.StatusPayload.cs`, `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate the new effect/trigger columns.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the schema and row-authoring change; only the existing `MSB3277` warnings remained.
- Unity `Pakuri/Validate CSV Source Data` succeeded after the follow-up validation fix, which confirmed the new headers, rows, and shared trigger semantics were accepted by the runtime catalog loader.

### History

- 2026-05-24: User asked Skill Builder to resume the interrupted Eve F-J passive implementation, which required shared passive effect and trigger schema expansion plus Eve row authoring.

## Task: 2026-05-18 Active Runtime CSV Authority

### Task title

Keep the current Scripts2 runtime CSV authority explicit and compact.

### Goals

- Keep active runtime authority on `Assets/CSVdata/source/*.csv` plus `Assets/CSVdata/EnemySkillData.csv`, with monster choice runtime data unified into `monster_skill_choices.csv` and `monster_modifier_skill_choice.csv`.
- Keep reward IDs, runtime choice IDs, and stage/enemy/monster CSV responsibilities separated.
- Keep base monster/enemy skill visual prefab authority out of active skill CSV rows now that `EffectManager` owns those scene mappings.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed intermediate migration steps remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active CSV authority summarized and retained for future work. 2026-05-18 Code Builder moved monster projectile/status tuning out of `monsters.csv` and into per-skill rows in `monster_skills.csv`. 2026-05-18 Code Builder added a one-command CSV runtime sync batch path and status-column validation/fallback for supported status labels. 2026-05-19 Code Builder superseded the old reward/modifier split by unifying monster choice runtime data into `monster_skill_choices.csv` plus the slim `monster_modifier_skill_choice.csv` gate file.

### Next Actions

- If future cleanup resumes, continue from this active runtime-authority split instead of reviving archived duplicate CSV tables.
- When CSV ownership changes, update this file together with `boards/RUN/RUN_BLACKBOARD.md` and `boards/COMBAT/ENEMY_BLACKBOARD.md`.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` carries the active enemy authored rows, including the current `basic_skill` plus `stage_one_skill` split.
- `Pakuri/Assets/CSVdata/EnemySkillData.csv` carries active enemy skill tuning rows.
- `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv` now carries the active monster choice gate rows, while `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now carries the unified choice display plus runtime modifier rows.
- `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` and `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` were deleted in the 2026-05-19 unification pass because active Scripts2 runtime code no longer reads them.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now keeps rows such as `rin-a-trait-5`, `rin-a-master-2`, and `ariel-a-master-1` explicitly marked `DataOnlyUnsupported` when current Scripts2 runtime still lacks the required special-case logic.
- After the 2026-05-26 execute-related choice-schema extension, `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` was normalized so all active rows now match the 78-column header again; post-fix field-count scans returned `UTF8_ALL_ROWS_OK` and `ALL_ROWS_OK_AFTER_BOM`, and the file was rewritten as UTF-8 BOM for cross-tool readability.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.*` files remain the active runtime load/build/validation path.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` plus `Assets/Scenes/NewScene/NewRunScene.unity` now own base monster/enemy skill effect prefab authority instead of `monster_skills.csv` / `EnemySkillData.csv`.
- `Pakuri/reference/Archive/InactiveRootCsv/` now stores archived inactive root CSV files that are no longer part of the active runtime path.
- `Pakuri/Assets/CSVdata/source/monsters.csv` no longer contains monster-level `projectile_speed`, `magazine_capacity`, `reload_duration`, `shot_interval`, `status_effect_label`, unit/projectile color, unit/projectile sprite path, projectile lifetime, or projectile hit radius columns.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now owns per-skill `projectile_speed`, `pierce_count`, `status_chance`, and `status_effect_label`; its deleted `range` column is no longer read by `PakuriCsvRuntimeData.MonsterDataset.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now maps projectile speed, base pierce, and status chance from `SkillDefinition` instead of hardcoded Ariel-A/Eve-A branches.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now falls back from blank `status_effect_id` to a parseable `status_effect_label`, so supported labels such as `媛먯쟾`, `?뷀솕`, `異붿쐞`, `鍮숆껐`, `痍⑥빟`, and `諛⑹뼱留? can resolve through `StatusEffectKind`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now fails validation when `status_chance > 0` points at an unsupported runtime status label/id.
- `SyncCsvRuntimeCatalogs.bat` calls Unity batchmode with `-executeMethod Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor`; when the project was already open in Unity, batchmode correctly failed with Unity's duplicate-project-open guard, and the same method was then invoked through Unity-MCP.
- Unity console after the MCP invocation logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced and validated from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` also logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the 2026-05-26 `monster_skill_choices.csv` row-width normalization follow-up.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` now shows only Eve's supported runtime statuses with positive `status_chance`: `eve-a shock 0.15`, `eve-b slow 0.2`, `eve-c chill 1`, `eve-d shock 1`, and `eve-e vulnerable 1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain. A first parallel runtime/editor build hit only an `obj\Debug\Assembly-CSharp.dll` file lock, then runtime passed when rerun alone.

### History

- 2026-05-18: EffectManager scene authority, reward/choice separation, enemy dual-skill CSV authority, and inactive root CSV archiving were recorded as the current active data baseline.
- 2026-05-18: Code Builder consolidated monster projectile/status tuning into `monster_skills.csv`, removed duplicate/visual projectile columns from `monsters.csv`, and removed Ariel-A/Eve-A hardcoded projectile/status values from the shared mapper/executor path.
- 2026-05-18: Code Builder added `SyncCsvRuntimeCatalogs.bat`, exposed `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` for Unity batchmode, normalized unsupported design-only monster status labels to `status_chance=0`, and verified sync/validation through the open Unity Editor.
- 2026-05-19: Code Builder first added shared-projectile-compatible `rin-a` modifier coverage, then unified monster choice runtime data into `monster_skill_choices.csv` / `monster_modifier_skill_choice.csv` and kept crit-only / every-third-hit chain behavior explicitly unsupported where current Scripts2 runtime still has no matching contract.
- 2026-05-26: Follow-up maintenance after the Rin-D execute schema extension normalized legacy `monster_skill_choices.csv` rows to the 78-column header, rewrote the file as UTF-8 BOM, and re-synced the runtime catalog without CSV fatal errors.

## Task: 2026-05-26 Rin F-J Passive CSV Trigger/Effect Schema

### Task title

Extend active monster skill CSV schema for reusable trigger actions, count gates, and conditional passive effects.

### Goals

- Add reusable CSV columns for delayed trigger actions, event skill filtering, event source scope, count gates, effect triggers, cooldown refunds, reload reduction, and status-source conditions.
- Add reusable effect columns for health-ratio conditions, hit-count conditions, and critical-damage status bonuses.
- Keep Rin F-J passive authoring in the active `Assets/CSVdata/source` CSV authority path.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV scope stayed limited to routed Rin skill-authoring files: `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- No new CSV file was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and validation passed.

### Next Actions

- Reuse `trigger_action`, `event_skill_id`, `target_skill_id`, `triggered_effect_id`, `trigger_delay_seconds`, `trigger_every_count`, and `event_source_scope` for future passive trigger work before adding another trigger table.
- Reuse `condition_health_ratio_max`, `condition_hit_count_min`, and `status_critical_damage_bonus` for future passive effects before adding specialized columns.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` header/type rows now include `trigger_action`, `event_skill_id`, `target_skill_id`, `triggered_effect_id`, `condition_status_source_skill_id`, `trigger_delay_seconds`, `trigger_every_count`, `event_source_scope`, `cooldown_refund_ratio`, and `reload_reduce_ratio`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header/type rows now include `condition_health_ratio_max`, `condition_hit_count_min`, and `status_critical_damage_bonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.StatusPayload.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate the new CSV fields.
- CSV field-count scan passed after authoring: `monster_skill_effects.csv` 64 columns / 91 lines, `monster_skill_triger.csv` 44 columns / 26 lines, `monster_skill_choices.csv` 86 columns / 252 lines, and `monster_skills.csv` 57 columns / 52 lines.
- Unity `Pakuri/Validate CSV Source Data` completed with the runtime catalog load summary and no Pakuri CSV validation failure.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-26: Rin F-J passive implementation required reusable trigger/action/count/effect schema instead of one-off runtime branches, and the user approved that extension.

## Task: 2026-05-29 Damage Meter Monster Icon CSV Handoff

### Task title

Prepare the CSV/data portion of the damage meter UI handoff.

### Goals

- Add `MonsterIconImage` to `Pakuri/Assets/CSVdata/source/monsters.csv` during Code Builder implementation.
- Route the new sprite path through the existing runtime CSV asset catalog path.
- Keep blank icon values non-fatal.

### Constraints

- Role Owner is Code Builder.
- Designer created the handoff only; no CSV or code changes were performed.
- Active CSV authority remains `Pakuri/Assets/CSVdata/source/*.csv` plus `PakuriCsvRuntimeData.*`.

### Role Owner

Code Builder

### Status

Implemented by Code Builder.

### Next Actions

- Fill `MonsterIconImage` asset paths later when final monster representative sprites are selected.
- User verifies in Play Mode that blank icon values hide the panel image without blocking the meter.

### Evidence

- `Pakuri/Assets/CSVdata/source/monsters.csv` inspected header currently has no `MonsterIconImage` column.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/MonsterDefinition.cs` currently has `DisplayName`, `UnitSprite`, and `ProjectileSprite`, but no dedicated monster icon field.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` already maps `monsters.csv display_name` into `MonsterDefinition.DisplayName` and uses `LoadSprite(...)` for existing sprite-backed CSV paths.
- `Pakuri/Assets/CSVdata/source/monsters.csv` now has `MonsterIconImage` plus `asset_path` type entry; all current monster rows keep the value blank.
- PowerShell CSV field-count check returned `header=24 rows=6 bad=`, confirming the edited `monsters.csv` row shape.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/MonsterDefinition.cs` now exposes `Sprite MonsterIconImage`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.AssetReferences.cs`, and `PakuriCsvRuntimeData.Build.cs` parse, collect, and map `MonsterIconImage`.
- Unity menu `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-29: User requested a Code Builder handoff that includes monster icon data ownership for the damage meter UI.
- 2026-05-29: Code Builder added the blank-safe `MonsterIconImage` CSV/catalog path for damage meter panel icons.

## Task: Runtime skill prefab dependency decommission

### Task title

Runtime skill prefab dependency decommission

### Goals

- Keep runtime skill numeric/visual authority in runtime CSV and graph nodes while deleting migrated skill prefabs.

### Constraints

- No new CSV columns; runtime object and collider offsets remain `(0, 0)`; retain Rin-D and Rin-E prefab exceptions.

### Role Owner

- Code Builder

### Status

- Code complete; user Play Mode verification pending.

### Next Actions

- User verifies base, enhancement, and master skill visuals and hit detection in Play Mode.

### Evidence

- `boards/MON/MONSTER_SKILL_RUNTIME_PREFAB_DECOMMISSION_PLAN.md`
- Runtime CSV shape check passed for 33 files; active runtime prefab path is Rin-E only.
- Deleted prefab GUID reference check passed for 33 prefab GUIDs.

### History

- 2026-07-14: Code Builder removed migrated prefab paths, normalized runtime collider offsets, and deleted migrated prefab assets.

## Source: boards\DATA\GAMEDATA_ASSET_BLACKBOARD.md

## Task: 2026-06-19 CSV Runtime Source Asset Path Reorganization

### Task title

Keep runtime CSV source asset references valid after moving CSV files into purpose-specific folders.

### Goals

- Preserve runtime source catalog and asset catalog sync after CSV file moves.
- Preserve Unity object references by moving CSV `.meta` files with their CSV files.
- Keep editor auto-sync watching the new runtime CSV folder tree.

### Constraints

- Role Owner is Code Builder.
- The runtime source catalog asset remains under `Assets/Resources/Pakuri/CSVRuntime`.
- Stage-flow scene references are preserved by GUID and remain user Play Mode verified.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP sync/validate checked.

### Next Actions

- Keep prefab-path authority in CSV rows unchanged; only the CSV table asset locations changed.
- When adding a new runtime CSV file, add its purpose folder mapping in `PakuriCsvRuntimeData.GetImportedSourceAssetPath(...)`.

### Evidence

- `PakuriCsvRuntimeData.Editor.cs` now syncs source TextAssets from `Assets/CSVdata/authoring/...` instead of `Assets/CSVdata/source`.
- `PakuriCsvRuntimeCatalogPostprocessor.cs` now detects changed `.csv` files under `Assets/CSVdata/authoring`.
- `PakuriSkillEffectPrefabCsvExporter.cs` now targets `Assets/CSVdata/authoring/monster/skills/monster_skill_choices.csv`.
- Unity-MCP sync logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/authoring' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP warning/error console read after sync/validate returned 0 entries.

### History

- 2026-06-19: Code Builder reorganized active CSV source assets and updated runtime/editor catalog paths without changing prefab-path strings inside the CSV rows.

## Task: 2026-06-19 Stage2 Enemy Skill Prefab Binding

### Task title

Wire Stage2 enemy skill prefabs through the active `EffectManager` enemy skill effect mapping.

### Goals

- Connect Stage2 enemy skill visuals under `Assets/Prefab/Enemy/Skill/Stage2` to the Stage2 skill ids.
- Use prefab colliders for offensive collider-backed skills when the prefab contains a 2D collider.
- Keep Lightning Scout and Arsen as direct-target/effect-only visuals because their inspected prefabs do not contain 2D colliders.

### Constraints

- Role Owner is Code Builder.
- `EffectManager.enemySkillEffects` in `NewRunScene` remains the scene authority for enemy skill visual prefab mapping.
- No Drake skill prefab exists under `Assets/Prefab/Enemy/Skill/Stage2` in the inspected file listing, so Drake OpeningCharge was not mapped to a skill prefab in this pass.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and file/build verified. 2026-06-19 later Code Builder pass fixed collider-backed runtime use for FireDragonSlash and kept Ethan prefab mapping while changing HolySpearThrow speed in runtime.

### Next Actions

- User verifies Stage2 skill visuals and collider contact behavior in Play Mode.
- Add a Drake skill prefab under `Assets/Prefab/Enemy/Skill/Stage2` before wiring OpeningCharge visual authority through the same mapping.

### Evidence

- `Pakuri/Assets/Prefab/Enemy/Skill/Stage2` contains `arsen_Skill.prefab`, `dark-assassin_Skill.prefab`, `ethan_Skill.prefab`, `fire-dragon-slayer.prefab`, `holy-priest_Skill.prefab`, `ice-guard_Skill.prefab`, and `lightning-scout_1.prefab`.
- PowerShell collider scan returned `collider=True` for `dark-assassin_Skill.prefab`, `ethan_Skill.prefab`, `fire-dragon-slayer.prefab`, and `ice-guard_Skill.prefab`.
- The same collider scan returned `collider=False` for `arsen_Skill.prefab`, `holy-priest_Skill.prefab`, and `lightning-scout_1.prefab`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `stage2-fire-dragon-slayer` / FireDragonSlash, `stage2-lightning-scout` / ChainLightning, `stage2-ice-guard` / FrostPressure, `stage2-dark-assassin` / DarkStab, `stage2-holy-priest` / HolyDragonHeal, `stage2-ethan` / HolySpearThrow, and `stage2-arsen` / Intimidation in `EffectManager.enemySkillEffects`.
- 2026-06-19 later inspection confirmed `NewRunScene.unity` still maps `stage2-fire-dragon-slayer` to `fire-dragon-slayer.prefab` GUID `ead3e9b7ab06e2f4287fbdf62d8aa4f1`, `stage2-dark-assassin` to `dark-assassin_Skill.prefab` GUID `36d272f072fbdbb4483b7e1b8cb8a5ed`, and `stage2-ethan` to `ethan_Skill.prefab` GUID `a41c14d516a697f4a803b6e60ec659e9`.
- `EnemyCombatSystem.cs` now routes `DamageArea` through the same collider-prefab path used by other collider-backed Stage2 enemy skills before falling back to old slash behavior, and hitbox lifetime now resolves from prefab animation length through `SkillVisualSpawnUtility.ResolveVisualLifetime(...)`.
- `EnemyCombatSystem.cs` now resolves HolySpearThrow projectile speed as current Ethan move speed x2 instead of the fixed CSV/projectile speed path.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and existing `MSB3277` warnings.
- 2026-06-19 later `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained, and the Editor build also retried once after a transient `Assembly-CSharp.dll` file lock.
- 2026-06-19 later Unity-MCP `validate_script` for `Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` returned 0 warnings and 0 errors.

### History

- 2026-06-19: User requested Stage2 skill prefab linkage and collider behavior by skill type after the enemy skill node runtime implementation.

## Task: 2026-05-31 Stage2 Enemy Prefab Binding

### Task title

Wire Stage 2 enemy prefabs into the active `NewRunScene` enemy spawn manager.

### Goals

- Connect every Stage 2 enemy id to its prefab under `Assets/Prefab/Enemy/Stage2`.
- Keep the existing Stage 1 hardcoded prefab fallback intact.
- Verify each Stage 2 prefab has the required runtime actor and collision component.

### Constraints

- Role Owner is Code Builder.
- The new binding uses the shared `EnemySpawnManger.enemyPrefabBindings` array instead of adding eight new Stage 2 serialized fields.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, Unity-MCP inspected, and debug-view child wiring checked.

### Next Actions

- User verifies Stage 2 prefab spawn positions and visual scale in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes 8 `enemyPrefabBindings` entries for `stage2-fire-dragon-slayer`, `stage2-lightning-scout`, `stage2-ice-guard`, `stage2-dark-assassin`, `stage2-holy-priest`, `stage2-ethan`, `stage2-drake`, and `stage2-arsen`.
- Unity-MCP scene inspection after reloading `Assets/Scenes/NewScene/NewRunScene.unity` showed all 8 Stage 2 bindings on `GameManager` / `Pakuri.InGame.EnemySpawnManger`.
- Unity-MCP `manage_asset get_components` found both `Pakuri.InGame.EnemyUnitActor` and `UnityEngine.BoxCollider2D` on all 8 Stage 2 prefabs, including `Assets/Prefab/Enemy/Stage2/stage2-holy-priest.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs` defines the auto-bound debug child names as `MonsterNameLabel`, `MonsterHpLabel`, `Damage`, `Background`, `Fill`, and `Shield`.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` calls `ResolveDebugViewReferences()` from `Initialize()` and resolves those children through `UnitActorView.FindTextMesh(...)` / `FindChildTransform(...)`.
- Unity-MCP `manage_prefabs get_hierarchy` found all 8 Stage 2 prefabs have `Damage` with `TextMesh`, `MonsterHpBar` with `Background`/`Fill`/`Shield` sprite children, `MonsterHpLabel` with `TextMesh`, and `MonsterNameLabel` with `TextMesh`.

### History

- 2026-05-31: User stated `stage2-holy-priest.prefab` now had `EnemyUnitActor` and `BoxCollider2D`, then requested Stage 2 prefab connection work.
- 2026-05-31: Code Builder added the shared prefab-binding array and connected all Stage 2 prefab assets in `NewRunScene`.
- 2026-05-31: Code Builder checked the newly added Stage 2 prefab debug-view children against `EnemyUnitActor` / `UnitActorView` and found the actual prefab names match the runtime auto-binding names.

## Task: 2026-05-28 Vega-A Skill Prefab Catalog Wiring

### Task title

Keep Vega-A base projectile visuals and shared follow-up projectiles wired through the active CSV runtime asset-catalog path.

### Goals

- Author Vega-A base skill visual authority on `Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- Reuse the same prefab path for shared follow-up projectile spawning instead of creating a second Vega-only visual route.
- Keep Vega-A master-2 slash visual authority on the user-provided `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` effect row.
- Keep the active runtime asset catalog as the resolver for the CSV-authored prefab path.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- No prefab content edit was performed in this task.
- Asset-path authority stayed on the active skill CSV and runtime asset catalog.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and Unity runtime-catalog path validated.

### Next Actions

- User verifies in Play Mode that both the base Vega-A burst projectiles and master-1 shadow follow-up projectile resolve the same requested prefab path.
- User verifies in Play Mode that Vega-A master-2 kill slashes resolve `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` through the triggered effect row.
- If later Vega-A branches require a different visual, add that as a CSV-authored choice/effect prefab path instead of a hardcoded asset lookup.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now sets `vega-a.skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- `Pakuri/Assets/Prefab/Skill/Vega/Vega_A.prefab` is the exact user-provided prefab path used for this implementation.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now sets `vega-a-master2-transfer-mark.skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.
- `Pakuri/Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` exists at the exact user-provided path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now reuses the resolved projectile prefab path when executing shared follow-up projectiles.
- Unity refresh completed after the CSV update, and the filtered Unity console returned no asset-catalog or CSV runtime errors.

### History

- 2026-05-28: User explicitly supplied `Assets/Prefab/Skill/Vega/Vega_A.prefab` as the Vega-A effect reference path, so the active wiring stayed on the existing CSV/runtime catalog route.
- 2026-05-28: User later supplied `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` as the Vega-A master-2 slash effect path, so that branch also stayed on the existing CSV/runtime catalog route.

## Task: 2026-05-27 Sein-C/D Skill Prefab And Catalog Wiring

### Task title

Keep Sein-C and Sein-D visuals wired through the active scene `EffectManager` and CSV runtime asset catalog paths.

### Goals

- Use `Assets/Prefab/Skill/Sein/Sein_B.prefab` as the flying projectile visual for Sein-C through the scene `EffectManager`.
- Keep Sein-C explosion / master effects and Sein-D zone / master effects resolvable through the runtime asset catalog from CSV-authored prefab paths.
- Avoid creating a new asset-routing path for delayed projectile impact visuals or residual zones.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Base flying projectile visual authority remains scene-owned through `EffectManager`.
- Follow-up explosion and zone visuals remain CSV-authored and runtime-catalog resolved.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and file-verified.

### Next Actions

- User verifies in Play Mode that Sein-C uses `Sein_B.prefab` while flying, then swaps to the requested impact / residual-zone visuals.
- Future delayed projectile skills should keep this split authority: scene mapping for the flying visual, CSV/runtime catalog for follow-up effect prefabs.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps monster `sein` skill `sein-c` to prefab GUID `2d30ba8904b73e2439b402f4782aefb3`, the requested `Assets/Prefab/Skill/Sein/Sein_B.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now points `sein-c.skill_effect_prefab_path` to `Assets/Prefab/Skill/Sein/Sein_C.prefab` and `sein-d.skill_effect_prefab_path` to `Assets/Prefab/Skill/Sein/Sein_D.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now points `sein-c-master2-contact` to `Assets/Prefab/Skill/Sein/Sein_C_Master-2.prefab`, `sein-c-master1-zone` to `Assets/Prefab/Skill/Sein/Sein_C_Master_1.prefab`, and `sein-d-master2-zone` to `Assets/Prefab/Skill/Sein/Sein_D_Master_2.prefab`.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` was updated by that sync and remains the runtime prefab-path resolver for the CSV-authored Sein effect prefabs.

### History

- 2026-05-27: User specified explicit Sein-C and Sein-D prefab paths for projectile, explosion, and zone visuals; the active wiring stayed on the existing scene `EffectManager` plus CSV runtime catalog split.

## Task: 2026-05-26 Rin Unit Animator Component Wiring

### Task title

Attach the new Rin animation controller component to the active `Rin_Unit` monster prefab.

### Goals

- Keep Rin unit animation wiring on `Assets/Prefab/Monster/Rin_Unit.prefab`.
- Reuse the already assigned `Rin_Animation_Cont.controller` Animator controller.
- Avoid scene-wide or CSV-owned animation wiring in this first implementation.

### Constraints

- Role Owner is Code Builder.
- The root prefab already carried `MonsterUnitActor` and `Animator`; this task only adds `Animation_Controller`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP inspected.

### Next Actions

- User verifies the animated Rin unit in Play Mode.

### Evidence

- Unity-MCP `manage_prefabs get_hierarchy` for `Assets/Prefab/Monster/Rin_Unit.prefab` showed the root `Rin_Unit` with `UnityEngine.Transform`, `UnityEngine.SpriteRenderer`, `Pakuri.InGame.MonsterUnitActor`, `UnityEngine.Animator`, and `Pakuri.InGame.Animation_Controller`.
- `Pakuri/Assets/Prefab/Monster/Rin_Unit.prefab` serializes `Pakuri.InGame.Animation_Controller` with script GUID `3ab96406b52c3454daa4c602c0b81989`.
- Unity editor code inspection returned `actor=True|animator=True|animationController=True|controllerName=Rin_Animation_Cont|clips=Anim_Rin_Idle,Anim_Rin_Attack_1,Anim_Rin_Attack_2,Anim_Rin_Attack_3,Anim_Rin_Dead_1,Anim_Rin_Hit`.

### History

- 2026-05-26: User requested the Rin animation implementation to be assigned only to `Assets/Prefab/Monster/Rin_Unit.prefab` for now.

## Task: 2026-05-26 Rin-B/Rin-C EffectManager Scene Wiring

### Task title

Keep Rin-B and Rin-C base skill visuals wired through the active `NewRunScene` `EffectManager` path.

### Goals

- Add the missing base `rin-b` scene visual mapping to `Assets/Prefab/Skill/Rin/Rin_B.prefab`.
- Keep `rin-c` grounded on the existing `Assets/Prefab/Skill/Rin/Rin_C.prefab` scene mapping.
- Avoid moving base monster skill prefab authority back into skill CSV rows.

### Constraints

- Role Owner is Skill Builder.
- No prefab content edit was required in this task.
- Base skill visuals remain scene-owned through `EffectManager`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder

### Status

Implemented and file-verified.

### Next Actions

- User verifies in Play Mode that Rin-B shows `Rin_B.prefab` and Rin-C continues to show `Rin_C.prefab`.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Rin/Rin_B.prefab.meta` stores GUID `1265e3a5e02b7f14cb94a3a818221ffa`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `SkillId: rin-b` to `Prefab: {fileID: 2447093715789092070, guid: 1265e3a5e02b7f14cb94a3a818221ffa, type: 3}`.
- `Pakuri/Assets/Prefab/Skill/Rin/Rin_C.prefab.meta` stores GUID `c17e18be6f4f31b49a083bf1ce120f0d`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` keeps `rin-c` mapped to `Prefab: {fileID: 8767310348598417902, guid: c17e18be6f4f31b49a083bf1ce120f0d, type: 3}`.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` remains the active base monster skill visual resolver through `ResolveMonsterSkillEffectPrefab(...)`.

### History

- 2026-05-26: User supplied `Assets/Prefab/Skill/Rin/Rin_B.prefab` and `Assets/Prefab/Skill/Rin/Rin_C.prefab` as the required Rin-B/Rin-C effect paths.

## Task: 2026-05-24 Rin-A Master-2 Choice Prefab Catalog Sync

### Task title

Sync Rin-A master-2 choice-level prefab path into the runtime asset catalog.

### Goals

- Keep base Rin-A visual authority scene-owned through `NewRunScene` `EffectManager`.
- Make the master-2 choice-level `skill_effect_prefab_path` resolvable through `PakuriCsvRuntimeAssetCatalog`.
- Reuse `Assets/Prefab/Skill/Rin/Rin_A.prefab` for the master-2 branch/effect path as requested.

### Constraints

- Role Owner is Skill Builder.
- No prefab content was edited.
- Base `rin-a` scene mapping remains unchanged.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder

### Status

Synced and file-verified.

### Next Actions

- User verifies in Play Mode that Rin-A master-2 uses the intended Rin_A visual on branch/effect projectiles.
- Future choice-level prefab paths should continue to sync through `Pakuri/Sync CSV Runtime Catalog Assets`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `rin-a-master-2` `skill_effect_prefab_path=Assets/Prefab/Skill/Rin/Rin_A.prefab`.
- `Pakuri/Assets/Prefab/Skill/Rin/Rin_A.prefab.meta` stores GUID `19bfba788239eba498a44cb67c2622c6`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` already maps monster `rin` skill `rin-a` to the same GUID through `EffectManager`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `AssetPath: Assets/Prefab/Skill/Rin/Rin_A.prefab` with GUID `19bfba788239eba498a44cb67c2622c6`.

### History

- 2026-05-24: User required Rin-A master-2 effect to use `Assets/Prefab/Skill/Rin/Rin_A.prefab`.

## Task: 2026-05-17 Active Runtime Skill Asset Wiring

### Task title

Keep the current skill prefab and runtime catalog wiring explicit for the active Scripts2 path.

### Goals

- Preserve the current runtime actor/prefab wiring for active skill prefabs already used by the kept new scene flow.
- Preserve the CSV runtime asset catalog as the asset-resolution bridge for active skill prefab paths.
- Keep choice-snapshot and Offering data alignment visible from the asset board point of view.

### Constraints

- Role Owner is Code Builder.
- User-authored prefab art/layout remains preserved as authored.
- Unity Play Mode verification remains user-owned.
- Detailed older asset wiring slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active asset-wiring baseline summarized and retained for future work. 2026-05-18 Code Builder removed monster unit/projectile sprite path authority from `monsters.csv`. 2026-05-18 CSV runtime sync can now be invoked by batchmode through a public editor method.

### Next Actions

- If more monster skills become active in runtime, wire them through the same prefab-actor plus catalog path instead of creating parallel asset routes.
- Update this file together with `boards/DATA/DATA_BLACKBOARD.md` when prefab-path authority changes again.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Ariel/Airel_A.prefab` is wired as a runtime projectile actor and serialized into `NewRunScene`.
- `Pakuri/Assets/Prefab/Skill/Rin/Rin_A.prefab` exists and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now serializes it as the `EffectManager` visual mapping for monster `rin` skill `rin-a`.
- `Pakuri/Assets/Prefab/Skill/Eve/Eve_A.prefab` and `Pakuri/Assets/Prefab/Skill/Ariel/Ariel_B.prefab` remain the retained baseline examples for shared projectile/attached-effect actor usage.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` remains the runtime asset catalog bridge for active prefab-path resolution.
- `PakuriCsvRuntimeData.Build.cs` was recorded as the source that builds active skills, passive skills, choice rows, and reward rows from the active CSV source files.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` remains the retained scene evidence for current prefab serialization and runtime references.
- `Pakuri/Assets/CSVdata/source/monsters.csv` no longer carries `unit_sprite_path`, `projectile_sprite_path`, `unit_color`, or `projectile_color`; `PakuriCsvRuntimeData.Editor.cs` no longer adds monster sprite paths to the CSV runtime asset catalog.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` no longer validates monster unit/projectile sprite asset coverage from `monsters.csv`; enemy sprite validation remains unchanged.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` exposes `SyncAndValidateCsvRuntimeCatalogsForEditor()` for batchmode and Unity-MCP execution.
- `SyncCsvRuntimeCatalogs.bat` invokes that editor method and writes Unity batch logs to `PakuriCsvRuntimeSync.log`.
- Unity-MCP execution of `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` logged successful catalog load and validation; `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` was touched by the sync.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.

### History

- 2026-05-15: Shared runtime skill prefab actor wiring became the retained baseline.
- 2026-05-17: Ariel-A prefab wiring and Eve A-J runtime catalog source alignment were added to that active baseline.
- 2026-05-18: Monster visual sprite/color source columns were removed from `monsters.csv`; current skill visual authority remains `EffectManager` plus scene/prefab wiring.
- 2026-05-18: CSV runtime catalog sync/validation was exposed as a public editor method and wrapped by `SyncCsvRuntimeCatalogs.bat`.
- 2026-05-19: Rin-A prefab wiring was added to the active `EffectManager` scene mapping using `Assets/Prefab/Skill/Rin/Rin_A.prefab`.

## Task: Runtime skill prefab asset decommission

### Task title

Runtime skill prefab asset decommission

### Goals

- Remove runtime-migrated assets from `Assets/Prefab/Skill` and keep only Rin-D/Rin-E exceptions.

### Constraints

- Delete prefab and matching meta together; remove serialized references before deletion; use Unity-MCP for asset refresh.

### Role Owner

- Code Builder

### Status

- Code complete; user Play Mode verification pending.

### Next Actions

- User verifies runtime visual parity in Play Mode.

### Evidence

- 33 prefabs and their metas deleted; remaining files are Rin-D, Rin-E, their metas, and `Rin.meta`.
- Unity-MCP refreshed assets; `PakuriCsvRuntimeAssetCatalog.asset` contains only Rin-E under `Assets/Prefab/Skill`.
- Deleted prefab GUID reference check returned zero current references.

### History

- 2026-07-14: Code Builder completed asset decommission and Unity catalog synchronization.

## Source: boards\MON\ARIEL_MONSTER.md

## Task: 2026-06-19 Ariel Plan-Action Runtime Migration

### Task title

Move Ariel choice modifiers from old wide choice folding to Ariel-only `SkillExecutionPlan` action handling.

### Goals

- Remove remaining Ariel old wide behavior payloads from `monster_skill_choices.csv`.
- Compile Ariel choice-owned normalized nodes into `SkillExecutionPlanNode.Action` payloads.
- Make Ariel snapshot mutation use plan action handlers instead of `ApplyNormalizedChoiceNodes(...)` folding into `SkillChoiceEffectSpec`.
- Keep Ariel A master2 status application on the explicit trigger/effect handler path while the +15% Holy damage-taken modifier stays a normalized node.

### Constraints

- Role Owner is Code Builder, then Code Reviewer.
- Compatibility gate is Ariel-only by `monster_id=ariel` or `choice_id` prefix `ariel-`.
- Full trigger/effect rows are not yet unified into `SkillExecutionPlan.Actions`; they remain explicit `monster_skill_triger.csv` / `monster_skill_effects.csv` runtime objects.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder / Code Reviewer

### Status

Implemented and reviewed. Passed for the Ariel-first target scope; full future goal of putting every trigger/effect action inside one `SkillExecutionPlan` is still not complete.

### Next Actions

- User verifies Ariel A master2, Ariel D trait4/master1, and dynamic shield-count damage behavior in Play Mode.
- Future migration can move trigger/effect CSV rows into plan action nodes if the target architecture requires a single plan-owned execution list.

### Evidence

- `SkillExecutionPlan.cs` now exposes `SkillExecutionPlanNode.Action`, `SkillExecutionPlanNode.FromAction(...)`, and `SkillExecutionPlan.Actions`.
- `SkillExecutionSnapshot.cs` now detects Ariel choices, maps `choice.NormalizedPlanNodes` through `InGameSkillDefinitionMapper.MapSkillNodeDefinitions(...)`, applies `ApplyPlanActionNodes(...)`, and skips the old `SkillChoiceEffectSpec` path for Ariel.
- `InGameSkillDefinitionMapper.cs` now skips `ApplyNormalizedChoiceNodes(...)` for Ariel choices and maps normalized node handlers such as `HitTargetCountBonus` and `StatusCriticalDamageTakenBonus` into `SkillActionOp`.
- `SkillExecutionSystem.cs` now resolves Ariel dynamic `CountStatusDamageMultiplier` through mapped plan action nodes while keeping the old wide dynamic path for non-Ariel compatibility.
- `monster_skill_choices.csv` Ariel old behavior-field scan returned `arielWideNonDefault=0`.
- `monster_skill_nodes.csv` has `ariel-d-trait-4-hit-target-count-bonus` with handler `HitTargetCountBonus`, and `monster_skill_node_params.csv` stores `bonus=1`.
- `monster_skill_nodes.csv` has `ariel-d-master-1-status-critical-damage-taken` with handler `StatusCriticalDamageTakenBonus`, and `monster_skill_node_params.csv` stores `bonus=0.25`.
- `monster_skill_triger.csv` has `ariel-a-master2-holy-exposure-on-hit` with `trigger_event=OnOutgoingDamage`, `requires_active_choice_id=ariel-a-master-2`, `target_selection=EventTarget`, `trigger_action=Effect`, and `triggered_effect_id=ariel-a-master-2-holy-exposure-on-hit`.
- `monster_skill_effects.csv` has `ariel-a-master-2-holy-exposure-on-hit` with `status_effect_id=holy-exposure`, `status_chance=1`, and `status_stack_amount=1`.
- Runtime and editor `dotnet build` commands passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP sync/validate menus logged runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`; warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested Code Builder to perform steps 1-6 for Ariel-first target-structure migration, then Code Reviewer review.
- 2026-06-19: Builder added plan action payloads and Ariel-only runtime routing; Reviewer found no blocking defects in the scoped Ariel-first migration, with the explicit caveat that trigger/effect rows are still not single-plan action nodes.

## Task: 2026-06-19 Ariel A Master2 Trigger Binding Fix

### Task title

Convert Ariel A master2 holy exposure from old choice wide columns to trigger/effect/node composition.

### Goals

- Replace `ariel-a-master-2` choice-wide status payload with a trigger-bound status effect object.
- Apply holy exposure to the hit event target through the current trigger runtime.
- Keep the +15% Holy damage taken value in a normalized status modifier node.
- Prevent migrated Ariel E shield variants from executing through leftover choice gates.

### Constraints

- Role Owner is Code Builder.
- Current trigger enum has no `OnHit`; the implemented runtime event is `OnOutgoingDamage`, which is the existing hit-success trigger path.
- Active CSV authority is under `Pakuri/Assets/CSVdata/authoring`.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, CSV-checked, and Unity-MCP validated. Code Reviewer was not rerun.

### Next Actions

- User verifies in Play Mode that Ariel A master2 applies `holy-exposure` to enemies hit by Ariel A.
- User verifies Ariel E shield amount variants no longer double-apply old precombined shield rows with the new shield amount nodes.

### Evidence

- `monster_skill_choices.csv` now keeps `ariel-a-master-2` as `RuntimeImplemented` with blank `status_tag`, `status_chance_bonus`, `status_stacks_set`, and `status_element_damage_taken_bonus`.
- `monster_skill_triger.csv` now has `ariel-a-master2-holy-exposure-on-hit` with `source_skill_id=ariel-a`, `trigger_event=OnOutgoingDamage`, `requires_active_choice_id=ariel-a-master-2`, `target_selection=EventTarget`, `trigger_action=Effect`, and `triggered_effect_id=ariel-a-master-2-holy-exposure-on-hit`.
- `monster_skill_effects.csv` now has `ariel-a-master-2-holy-exposure-on-hit` as a `Status` effect for `status_effect_id=holy-exposure`, `status_chance=1`, and `status_stack_amount=1`.
- `monster_skill_nodes.csv` and `monster_skill_node_params.csv` now add `ariel-a-master-2-holy-exposure-element-damage-taken` with handler `StatusElementDamageTakenBonus` and `bonus=0.15`.
- `monster_skill_effects.csv` no longer has executable `requires_active_choice_id` or `requires_passive_skill_id` gates on `MigratedToEffectBinding` rows, including the three Ariel E shield variants.
- `SkillTriggerRuntime.ExecuteEffect(...)` now forwards `triggerContext.EventTarget` into `SkillExecutionContext`, so `target_selection=EventTarget` works for trigger-bound effect rows.
- `PakuriCsvRuntimeData.Validation.cs` now rejects `MigratedToEffectBinding` skill effects that still carry executable choice/passive gates.
- `Import-Csv` property-count check returned `monster_skill_choices.csv props=114 dataRows=253 bad=`, `monster_skill_effects.csv props=71 dataRows=133 bad=`, `monster_skill_triger.csv props=47 dataRows=59 bad=`, `monster_skill_nodes.csv props=14 dataRows=54 bad=`, and `monster_skill_node_params.csv props=4 dataRows=76 bad=`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` executed; console logged runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`, with 0 warning/error entries.

### History

- 2026-06-19: Code Reviewer found `ariel-a-master-2` still used old choice-wide status columns and Ariel E migrated shield variants still had executable choice gates.
- 2026-06-19: User confirmed Ariel A master2 should be represented as trigger on hit, event target, apply status, and `status_id=holy-exposure`; Builder implemented the current-runtime equivalent using `OnOutgoingDamage` plus trigger-bound status effect and normalized node modifier.

## Task: 2026-06-19 Ariel Passive Node Decomposition Follow-up

### Task title

Convert remaining Ariel passive numeric modifiers to atomic normalized nodes.

### Goals

- Make Ariel F/G/H/I/J passive numeric upgrades compose like Ariel C: base effect objects plus modifier nodes and trigger bindings.
- Remove duplicate execution paths where old choice-gated effect or trigger rows would stack with the new nodes.
- Keep conceptually separate effects such as F trait3 crit, G trait3 shielded holy damage, and I trait3 holy resist reduction as effect objects.

### Constraints

- Role Owner is Code Builder.
- The implementation stays on `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- No MSW-MCP is used; Unity checks use Unity-MCP only.
- Unity Play Mode parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, CSV-checked, and Unity-MCP validated.

### Next Actions

- User verifies Ariel F/G/H/I/J passive combinations in Play Mode.
- Keep future passive numeric add-ons on normalized nodes before adding choice-gated duplicate effect rows.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now registers `StatusDamageBonusRate`, `StatusShieldReceivedBonus`, `StatusCriticalChanceBonus`, `StatusDamageTakenBonus`, and `StatusFlatElementResistReduction` normalized handlers.
- `SkillChoiceEffectSpec`, `SkillExecutionSnapshot`, `InGameSkillDefinitionMapper`, `SkillStatusSpecUtility`, and `SingleAttackSkillExecutor` now carry those status modifier nodes into status data; existing element/crit/ailment status bonuses now accumulate instead of replacing base values.
- `monster_skill_nodes.csv` now uses status modifier nodes for `ariel-f-trait-1-holy-damage-bonus`, `ariel-g-trait-1-shield-received-bonus`, `ariel-g-trait-2-start-shield-amount-multiplier`, `ariel-h-trait-1-blessed-holy-damage-bonus`, `ariel-h-trait-2-blessed-action-speed-bonus`, `ariel-i-trait-1-exposure-damage-taken-bonus`, `ariel-j-trait-1-after-e-action-speed-bonus`, and `ariel-j-trait-2-shielded-holy-damage-bonus`.
- `monster_skill_effects.csv` marks `ariel-g-shield-received-trait1`, `ariel-g-start-shield-trait2`, `ariel-i-holy-exposure-damage-taken-trait1`, and `ariel-j-after-e-action-speed-trait1` as `MigratedToEffectBinding`.
- `monster_skill_triger.csv` no longer contains `ariel-j-after-e-action-speed-trait1-trigger`; J trait1 now modifies the base J post-E trigger effect through a normalized node.
- CSV shape check returned `monster_skill_choices.csv header=114 rows=252 bad=`, `monster_skill_nodes.csv header=14 rows=52 bad=`, `monster_skill_node_params.csv header=4 rows=74 bad=`, `monster_skill_effects.csv header=71 rows=131 bad=`, and `monster_skill_triger.csv header=47 rows=57 bad=`.
- Ariel spot check returned `{"fNode":1,"gNodes":2,"hNodes":2,"iTrait1Migrated":1,"jTrait1TriggerRows":0,"oldGenericPassiveDamageNodes":0}`.
- Active old-support check returned `activeReferenceDirectEffects=0 referenceDirectTriggers=0` for Ariel rows.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP console after `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` logged runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`, with 0 warning/error console entries.

### History

- 2026-06-19: User requested Code Builder to decompose every Ariel skill like Ariel C using atomic effect object + modifier node + binding node, using lines 373-962 of the Ariel handoff report as the detailed node standard.

## Task: 2026-06-19 Ariel Effect Object Node Pilot

### Task title

Implement the first Ariel normalized-node pilot for numeric choice modifiers and Ariel C blessing composition.

### Goals

- Move Ariel numeric choice effects from wide `monster_skill_choices.csv` fields to reusable normalized nodes.
- Prove Ariel C can reduce pre-combined blessing rows by composing base effect rows with trait/passive nodes.
- Keep old effect rows only as compatibility rows when not yet replaced by the generic node path.

### Constraints

- Role Owner is Code Builder.
- User selected generic `monster_skill_nodes.csv` and `monster_skill_node_params.csv`; no new specialized effect tables were added.
- User answered D trait 5 condition is attacker-self shield, J requires Ariel-E-generated shield, I exposure damage taken applies to all incoming damage, passives are always active, and durations stay seconds.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- User verifies Ariel C base, trait2, trait3, H trait3, trait5, master1, and master2 combinations in Play Mode.
- Later pass should implement source-specific shield checks so Ariel J can require Ariel-E-generated shield instead of generic shield.
- Continue Ariel B/E/passive ownership cleanup only after Ariel C parity is confirmed.

### Evidence

- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_nodes.csv` now contains Ariel normalized choice nodes for damage, cooldown, magazine, reload, pierce, duration, shield-count damage, status-conditional damage-taken, and status modifier bonuses.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_node_params.csv` now carries the matching values; initial migration output reported `migrated=28 nodes=47 params=68`, and the final Ariel C trait2 targeted action-speed node addition brought the parsed param row count to 69.
- TextFieldParser CSV shape check returned `monster_skill_choices.csv header=114 rows=252 bad=`, `monster_skill_nodes.csv header=14 rows=47 bad=`, `monster_skill_node_params.csv header=4 rows=69 bad=`, and `monster_skill_effects.csv header=70 rows=131 bad=`.
- `ariel-c-trait-2-blessing-action-speed` is a `StatusActionSpeedBonus` node with `status_id=blessing` and `bonus=0.06`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_effects.csv` keeps Ariel C base rows but disables 9 pre-combined rows as `MigratedToEffectBinding`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now applies normalized choice nodes during combat snapshot creation and resolves status-targeted action speed bonuses.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now applies snapshot status overrides through `SkillStatusSpecUtility.ResolveStatusData(...)`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` completed; console logged `InGame skill data validation passed with 0 warning(s).`

### History

- 2026-06-19: User requested Code Builder implementation of `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md` and provided answers to all ambiguous design questions.

## Task: 2026-06-19 Ariel Phase 2-5 Effect Object Cleanup

### Task title

Continue Ariel B/E/A/D/F-J normalized node, trigger-binding, and passive ownership cleanup after the Ariel C pilot.

### Goals

- Move Ariel B shield amount modifiers onto a shield-specific normalized node handler.
- Reduce Ariel E shield variants to one active shield effect plus shield amount nodes.
- Move Ariel J post-E action-speed behavior out of Ariel E effect rows into J-owned trigger/effect rows.
- Keep Ariel A/B/D trigger rows as explicit specialized trigger-binding compatibility rows.
- Add a source-specific effect condition so Ariel J shielded holy damage requires the Ariel E shield effect, not any shield.

### Constraints

- Role Owner is Code Builder, followed by one Code Reviewer pass requested by the user.
- The implementation stays on generic `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- No MSW-MCP is used; Unity checks use Unity-MCP only.
- Unity Play Mode parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, CSV-checked, and Unity-MCP validated. Code Reviewer pass pending in the same user request.

### Next Actions

- Run the requested Code Reviewer pass against the current diff.
- User verifies Ariel B shield amount/duration, E shield trait/master combinations, J after-E action-speed, and J Ariel-E-shield-only holy damage in Play Mode.

### Evidence

- `SkillChoiceEffectSpec`, `SkillExecutionSnapshot`, `InGameSkillDefinitionMapper`, `SkillExecutionUtility`, and `SkillMultiEffectExecutor` now carry `ShieldAmountMultiplier`; active shield skill bodies and status-effect shield amounts can use shield-specific normalized choice nodes.
- `SkillEffectDefinition`, `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `StatusEffectRuntime`, and `SkillMultiEffectExecutor` now carry `condition_status_source_skill_id` for effect condition checks.
- `monster_skill_nodes.csv` contains four Ariel `ShieldAmountMultiplier` nodes: B trait1, B master1, E trait2, and E master2.
- `monster_skill_effects.csv` has exactly one active `ariel-e-shield*` row, while `ariel-e-shield-trait2`, `ariel-e-shield-master2`, and `ariel-e-shield-trait2-master2` are `MigratedToEffectBinding`.
- `monster_skill_effects.csv` no longer contains `ariel-e-passive-j-*`; the post-E action-speed effects now live as `ariel-j-after-e-action-speed` and `ariel-j-after-e-action-speed-trait1`.
- `monster_skill_triger.csv` contains J-owned `OnSkillCast` trigger rows for `event_skill_id=ariel-e`, including the trait1-gated trigger.
- `ariel-j-shielded-holy-damage` now has `condition_status_id=shield` and `condition_status_source_skill_id=ariel-e-shield-base`, matching the actual effect-created shield status source.
- CSV field-count check returned no bad rows for `monster_skill_choices.csv`, `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- Acceptance spot-check returned `eActiveShieldRows=1`, `eDisabledShieldVariants=3`, `shieldAmountNodes=4`, `oldEJRows=0`, `jTriggerRows=2`, and `jShieldSource=ariel-e-shield-base`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` completed; console logged catalog load and `InGame skill data validation passed with 0 warning(s)`, with 0 error/warning console entries.

### History

- 2026-06-19: User requested Code Builder to perform the remaining Phase 2-5 work from `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md`, then run Code Reviewer.

## Task: 2026-05-22 Ariel Final Shared Choice Runtime Completion

### Task title

Implement `ariel-a-trait-5` and `ariel-d-trait-5` through shared choice/status contracts and re-audit Ariel coverage.

### Goals

- Add a shared choice snapshot rule that counts shielded allies and converts the count into a per-cast damage multiplier.
- Add a shared status rule that increases incoming damage only when the attacker has a required status and the target carries the marked status.
- Confirm that no Ariel skill, choice, effect, or trigger row remains unsupported after this pass.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay reusable in shared runtime/data paths rather than adding Ariel-only execution branches.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and CSV-sync-verified.

### Next Actions

- User verifies in Play Mode that `ariel-a-trait-5` scales Ariel-A damage by `+6%` per currently shielded ally at cast time.
- User verifies in Play Mode that `ariel-d-trait-5` increases damage only when the attacker has `shield` and the target carries Ariel-D's `holy-exposure` mark.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:7` now marks `ariel-a-trait-5` as `RuntimeImplemented` with `count_status_id=shield`, `count_target_side=AllAllies`, and `damage_multiplier_per_count=0.06`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:28` now marks `ariel-d-trait-5` as `RuntimeImplemented` with `status_conditional_source_status_id=shield` and `status_conditional_damage_taken_bonus=0.1`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs:216-285` now resolves choices with roster context, counts matching status holders, and applies the dynamic damage multiplier to the cast snapshot.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:291-337`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:234-246`, `:366-374`, and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:965-1011` now carry source-conditional incoming-damage status data through status resolution and the live damage path.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1-2` now contains the current status payload schema columns, including `status_ailment_resistance_bonus` and `status_flat_element_resist_reduction`, so editor CSV sync matches the parser contract.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv | Where-Object { $_.monster_id -eq 'ariel' -and $_.implementation_state -notin @('RuntimeImplemented','ReferenceDirect') }`, the matching `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv` checks all returned no rows.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.
- Unity-MCP console after clear plus `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-22: User asked Code Builder to implement `ariel-a-trait-5` and `ariel-d-trait-5` and confirm whether every Ariel skill was now implemented.

## Task: 2026-06-07 Ariel Animation Clip Controller And Prefab Wiring

### Task title

Create Ariel's shared Rin-contract animation assets and wire the monster prefab animator.

### Goals

- Create Ariel's six animation clips: attack 1, attack 2, attack 3, idle, hit, and death.
- Create `Ariel_Animation_Cont.controller` with the same parameter contract as Rin: `Attack`, `AttackIndex`, `Hit`, and `Death`.
- Add Animator and `Animation_Controller` components to `Ariel_Unit.prefab` and connect `MonsterUnitActor.animationController`.

### Constraints

- Role Owner is Code Builder.
- The controller contract follows inspected `Rin_Animation_Cont.controller`.
- Unity Editor import and Play Mode animation verification were not available in this session.

### Role Owner

Code Builder

### Status

Implemented and locally YAML/build-verified.

### Next Actions

- User lets Unity import the new `.anim` and `.controller` assets.
- User verifies in Play Mode that Ariel plays idle, attack 1-3, hit, and death through the shared animation parameter contract.

### Evidence

- `Pakuri/Assets/Image/Monster/ariel/Animation/Animation_Ariel_Sprite` now contains 6 `Anim_Ariel_*.anim` files, 6 matching `.anim.meta` files, `Ariel_Animation_Cont.controller`, and `Ariel_Animation_Cont.controller.meta`.
- `Select-String` confirmed `Ariel_Animation_Cont.controller` contains `Attack`, `AttackIndex`, `Hit`, `Death`, and the states `Anim_Ariel_Attack_1`, `Anim_Ariel_Attack_2`, `Anim_Ariel_Attack_3`, `Anim_Ariel_Hit`, `Anim_Ariel_Idle`, and `Anim_Ariel_Dead_1`.
- `Pakuri/Assets/Prefab/Monster/Ariel_Unit.prefab` now has `animationController: {fileID: 900100000000002}`, an `Animator` with controller GUID `b2339c033d324ea8a1f138797de25ab8`, and an `Animation_Controller` with `idleState: Anim_Ariel_Idle`, `deadState: Anim_Ariel_Dead_1`, and `attackStateCount: 3`.
- The controller meta GUID check returned `Ariel controllerGuid=b2339c033d324ea8a1f138797de25ab8 linked=True`.
- The generated idle clip check returned `Ariel idleName=Anim_Ariel_Idle spriteRefs=16`.
- 2026-06-07 follow-up correction verified `Ariel root=4596420534878418281 rootRefs=true animatorOwner=4596420534878418281 controllerOwner=4596420534878418281 ok=true` after fixing the generated Animator and `Animation_Controller` component owner fileIDs to the root `Ariel_Unit` GameObject.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing `MSB3277` warnings remained.

### History

- 2026-06-07: User asked Code Builder to create each monster's six animation clips, create controllers with Rin's parameter contract, and wire each monster prefab Animator controller.
- 2026-06-07: User reported the non-Rin monster prefabs still did not show assigned Animator / `Animation_Controller`; Code Builder found the generated component blocks were owned by the wrong GameObject fileID and corrected them to the root Unit GameObject.

## Task: Ariel runtime skill prefab decommission

### Task title

Ariel runtime skill prefab decommission

### Goals

- Remove all seven Ariel skill prefabs after retaining their runtime visual data.

### Constraints

- No blueprint or new CSV columns; collider offset remains `(0, 0)`.

### Role Owner

- Code Builder

### Status

- Code complete; user Play Mode verification pending.

### Next Actions

- User verifies Ariel A-E, enhancement, and master visuals in Play Mode.

### Evidence

- Ariel graph prefab EffectVisual rows were replaced by `RuntimeEffectVisual` where required.
- `NewRunScene.unity` contains no Ariel monster skill prefab mappings.
- `Pakuri/Assets/Prefab/Skill/Ariel` assets and `Ariel.meta` were deleted.

### History

- 2026-07-14: Code Builder completed Ariel prefab dependency removal.

## Source: boards\MON\EVE_MONSTER.md

## Task: 2026-05-24 Eve F-J Passive Runtime Completion

### Task title

Implement Eve passive skills F-J on shared passive/effect/trigger runtime paths and finish the interrupted `SkillTriggerRuntime.cs` follow-up.

### Goals

- Keep Eve-F/J passive behavior data-owned through `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `monster_skill_choices.csv`.
- Support Eve-F combat-start shield plus shocked-target modifiers, Eve-G Lightning/Ice ally buffs plus auto Prism Ray trigger, Eve-H chill/freeze target modifiers plus freeze-expire burst, Eve-I shocked/shock-5 Lightning amplifiers, and Eve-J vulnerable multi-resistance debuffs.
- Keep all new behavior on shared runtime/status/trigger code paths instead of adding Eve-only executor branches.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The selected authority stayed on `boards/SkillBluePrint/passive-stat-blueprint.md`, the inspected Eve CSV rows, and the explicitly edited runtime/data files.
- Unity Play Mode gameplay verification remains user-owned.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, build-verified, and Unity CSV validation passed.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-F gives the combat-start shield only to allies with at least one Lightning active skill and that trait 3 grants action speed only while shielded.
- User verifies Eve-G auto-casts Eve-B from allied Lightning/Ice outgoing damage with the shared internal cooldown and that trait 3 only boosts Eve-B against shielded targets.
- User verifies Eve-H freeze-expire burst, Eve-I shock-5 Lightning resistance reduction, and Eve-J vulnerable damage/resistance amplification on live enemies.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `eve-f-trait-1` through `eve-j-trait-3` as `RuntimeImplemented`; `eve-g-trait-3` now targets `eve-b`, `eve-i-trait-3` now targets `eve-d`, and `eve-j-trait-3` now targets `eve-e`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now authors Eve F-J passive rows such as `eve-f-start-shield`, `eve-h-status-chance`, `eve-i-shock5-lightning-resist`, and the `eve-j-vulnerable-*-resist` family.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `eve-g-auto-prism-ray`, `eve-g-auto-prism-ray-trait1`, and `eve-h-freeze-expire-burst`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillTriggerRuntime.cs`, `Skills/Execution/SkillExecutors.cs`, and `Skills/Data/StatusEffectRuntime.cs` now share condition-status parsing, trigger-attribute matching, and runtime-kind checks needed by Eve G/H/I/J.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity `Pakuri/Validate CSV Source Data` completed successfully and logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`

### History

- 2026-05-24: User asked Skill Builder to resume the interrupted Eve F-J passive implementation that had stopped during the added `SkillTriggerRuntime.cs` work.

## Task: 2026-05-17 Eve A-J Active Runtime Baseline

### Task title

Keep the current Eve A-J Scripts2 runtime state compact and explicit.

### Goals

- Preserve the current Eve A-J data/Offering baseline from the active CSV source files.
- Preserve the shared status-runtime foundation and visible label output used by Eve-A shock.
- Preserve Eve-A projectile modifier execution through the shared InGame execution path.
- Keep the board explicit that Eve B-E executor depth and F-J passive effect depth still remain later work.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older Eve slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active Eve baseline summarized and retained for future work. 2026-05-18 Eve-A/Eve status values are now read from `monster_skills.csv`. 2026-05-18 supported Korean status labels can now resolve through the shared status parser.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-A shock, modifier choices, and Offering gating behave as recorded.
- Continue later Eve work from the shared status/runtime path instead of reintroducing Eve-only special-case state.
- Use the archive snapshot when older prefab-binding or CombatRuntime-era Eve history is needed.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv`, `monster_skill_choices.csv`, and `monster_modifier_skill_choice.csv` hold the retained Eve A-J source rows and active choice/modifier mappings.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs`, `SkillExecutionSystem.cs`, `SkillRuntimeInstance.cs`, and `InGameProjectileActor.cs` own the current Eve-A projectile modifier, branch, and shock execution path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectData.cs`, `StatusEffectKind.cs`, `InGameSkillDefinitionMapper.cs`, and `BaseUnitRuntimeModel.cs` own the retained shared status foundation used by Eve work.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` was recorded as the current Offering gating point for learned active/passive Eve reward choices.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores Eve-A `projectile_speed=15`, `pierce_count=0`, `status_effect_id=shock`, `status_chance=0.15`, and `status_effect_label=媛먯쟾`; Eve-B/C/D/E status rows are `slow`/`chill`/`shock`/`vulnerable` with labels.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` no longer contains an Eve-A-only shock chance override; `InGameSkillDefinitionMapper.cs` now maps status chance from CSV into `StatusApplicationSpec`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now parses supported Korean labels such as `媛먯쟾`, `?뷀솕`, `異붿쐞`, and `痍⑥빟`, and `InGameSkillDefinitionMapper.cs` can use a parseable `status_effect_label` when `status_effect_id` is blank.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` shows Eve's positive runtime statuses as `eve-a shock 0.15 媛먯쟾`, `eve-b slow 0.2 ?뷀솕`, `eve-c chill 1 異붿쐞`, `eve-d shock 1 媛먯쟾`, and `eve-e vulnerable 1 痍⑥빟`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-17: Eve A-J source data, Offering mapping, shared status foundation, Eve-A projectile modifier execution, and visible status label behavior became the current active baseline.
- 2026-05-18: Code Builder moved Eve-A shock chance and projectile speed from hardcoded/monster-level data into the Eve skill row.
- 2026-05-18: Code Builder added supported Korean status-label parsing/fallback and CSV runtime sync batch support.

## Task: 2026-06-07 Eve Animation Clip Controller And Prefab Wiring

### Task title

Create Eve's shared Rin-contract animation assets and wire the monster prefab animator.

### Goals

- Create Eve's six animation clips: attack 1, attack 2, attack 3, idle, hit, and death.
- Create `Eve_Animation_Cont.controller` with the same parameter contract as Rin: `Attack`, `AttackIndex`, `Hit`, and `Death`.
- Add Animator and `Animation_Controller` components to `Eve_Unit.prefab` and connect `MonsterUnitActor.animationController`.

### Constraints

- Role Owner is Code Builder.
- The controller contract follows inspected `Rin_Animation_Cont.controller`.
- Unity Editor import and Play Mode animation verification were not available in this session.

### Role Owner

Code Builder

### Status

Implemented and locally YAML/build-verified.

### Next Actions

- User lets Unity import the new `.anim` and `.controller` assets.
- User verifies in Play Mode that Eve plays idle, attack 1-3, hit, and death through the shared animation parameter contract.

### Evidence

- `Pakuri/Assets/Image/Monster/Eve/Animation/Animation_Eve_Sprite` now contains 6 `Anim_Eve_*.anim` files, 6 matching `.anim.meta` files, `Eve_Animation_Cont.controller`, and `Eve_Animation_Cont.controller.meta`.
- `Select-String` confirmed `Eve_Animation_Cont.controller` contains `Attack`, `AttackIndex`, `Hit`, `Death`, and the states `Anim_Eve_Attack_1`, `Anim_Eve_Attack_2`, `Anim_Eve_Attack_3`, `Anim_Eve_Hit`, `Anim_Eve_Idle`, and `Anim_Eve_Dead_1`.
- `Pakuri/Assets/Prefab/Monster/Eve_Unit.prefab` now has `animationController: {fileID: 900200000000002}`, an `Animator` with controller GUID `cc69556112bc45619ea4177c77ae95dc`, and an `Animation_Controller` with `idleState: Anim_Eve_Idle`, `deadState: Anim_Eve_Dead_1`, and `attackStateCount: 3`.
- The controller meta GUID check returned `Eve controllerGuid=cc69556112bc45619ea4177c77ae95dc linked=True`.
- The generated idle clip check returned `Eve idleName=Anim_Eve_Idle spriteRefs=16`.
- 2026-06-07 follow-up correction verified `Eve root=4596420534878418281 rootRefs=true animatorOwner=4596420534878418281 controllerOwner=4596420534878418281 ok=true` after fixing the generated Animator and `Animation_Controller` component owner fileIDs to the root `Eve_Unit` GameObject.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing `MSB3277` warnings remained.

### History

- 2026-06-07: User asked Code Builder to create each monster's six animation clips, create controllers with Rin's parameter contract, and wire each monster prefab Animator controller.
- 2026-06-07: User reported the non-Rin monster prefabs still did not show assigned Animator / `Animation_Controller`; Code Builder found the generated component blocks were owned by the wrong GameObject fileID and corrected them to the root Unit GameObject.

## Task: 2026-05-18 Eve C/D/E Runtime Kind And Names

### Task title

Correct Eve C/D/E names and AreaAttack/SingleAttack runtime kinds from reference files.

### Goals

- Keep Eve C named `프로스트 필드`, not translated as `서리 지대`.
- Keep Eve D named `스태틱 오버라이드`, not translated as `정전기 과부하`.
- Route Eve C/E as sustained `AreaAttack` and Eve D as one-shot `SingleAttack`.

### Constraints

- Role Owner is Code Builder.
- Eve C/D/E names are grounded in `Pakuri/reference/2.Monster/eve/skill`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Eve C ticks for 4 seconds, Eve E ticks for 5 seconds, and Eve D performs a one-shot area hit.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md` lists `스킬명 | 프로스트 필드`.
- `Pakuri/reference/2.Monster/eve/skill/d-static-override.md` lists `스킬명 | 스태틱 오버라이드`.
- `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md` lists `스킬명 | 플라즈마 필드`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `eve-c` display `프로스트 필드`, runtime `AreaAttack`, tick interval `0.5`, and active duration `4`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `eve-d` display `스태틱 오버라이드` and runtime `SingleAttack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `eve-e` runtime `AreaAttack`, tick interval `0.8`, and active duration `5`.
- Eve passive descriptions in `monster_skills.csv` now refer to `프로스트 필드` and `스태틱 오버라이드`.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User reported that the issue was not CSV corruption but wrong translated/hardcoded skill naming and requested Code Builder correction.

## Task: Eve runtime skill prefab decommission

### Task title

Eve runtime skill prefab decommission

### Goals

- Remove all six Eve skill prefabs while preserving runtime visuals and projectile branches.

### Constraints

- LineAttack remains collider-driven; no new CSV columns; collider offset remains `(0, 0)`.

### Role Owner

- Code Builder

### Status

- Code complete; user Play Mode verification pending.

### Next Actions

- User verifies Eve A-E, especially A branch projectiles and B line hit detection, in Play Mode.

### Evidence

- Projectile branch runtime visual support was added to `ProjectileSkillExecutor` and `InGameProjectileActor`.
- `NewRunScene.unity` contains no Eve monster skill prefab mappings.
- `Pakuri/Assets/Prefab/Skill/Eve` assets and `Eve.meta` were deleted.

### History

- 2026-07-14: Code Builder completed Eve prefab dependency removal.

## Source: boards\MON\RIN_MONSTER.md

## Task: 2026-06-07 Rin Animator Trigger Controller And Shared Actor Hook

### Task title

Move Rin unit animation routing from direct state-play attack/hit calls to Animator parameters, and make monster animation hooks reusable by other monster actors.

### Goals

- Add trigger/int parameter routing to `Rin_Animation_Cont.controller`.
- Change `Animation_Controller` attack and hit playback to use Animator parameters instead of hardcoded direct state names.
- Keep death final-frame freeze in script after the `Death` trigger.
- Remove the Rin-only `MonsterUnitActor` definition-id gate so other monster prefabs can opt in by adding `Animation_Controller` and a compatible Animator Controller.

### Constraints

- Role Owner is Code Builder.
- Existing Rin state names and animation clip references are preserved.
- Other characters still need their own compatible Animator Controller parameters and prefab component wiring before they can play animations.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin plays random `AttackIndex` 0-2 attacks, hit animation, death animation, and death final-frame freeze.
- For Ariel/Eve/Sein/Vega, add `Animator` plus `Animation_Controller` to each prefab and use an Animator Controller with `Attack`, `AttackIndex`, `Hit`, and `Death` parameters.

### Evidence

- `Pakuri/Assets/Image/Monster/Rin/Animation/Animation_Rin 1/Rin_Animation_Cont.controller` now has `Attack`, `AttackIndex`, `Hit`, and `Death` Animator parameters.
- The same controller now has Any State transitions for `Attack` plus `AttackIndex` 0, 1, and 2 into `Anim_Rin_Attack_1`, `Anim_Rin_Attack_2`, and `Anim_Rin_Attack_3`.
- The same controller now has Any State transitions for `Hit` into `Anim_Rin_Hit` and `Death` into `Anim_Rin_Dead_1`.
- Attack and hit states now transition back to `Anim_Rin_Idle` with exit time.
- `Pakuri/Assets/Scripts2/InGame/Animation/Animation_Controller.cs` now calls `SetInteger("AttackIndex", ...)`, `SetTrigger("Attack")`, `SetTrigger("Hit")`, and `SetTrigger("Death")` when those Animator parameters exist, and keeps direct `Animator.Play(deadState, 0, 0.999f)` only for the death final-frame freeze.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` no longer contains `RinMonsterId` or `ShouldUseRinAnimation()` and now calls the resolved `Animation_Controller` for any monster actor that has the component.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `git diff --check -- Pakuri\Assets\Scripts2\InGame\Animation\Animation_Controller.cs Pakuri\Assets\Scripts2\InGame\Units\MonsterUnitActor.cs "Pakuri\Assets\Image\Monster\Rin\Animation\Animation_Rin 1\Rin_Animation_Cont.controller"` passed with only line-ending conversion warnings.

### History

- 2026-06-07: User asked Code Builder to update `Rin_Animation_Cont.controller`, `Animation_Controller.cs`, and `MonsterUnitActor.cs` so Rin uses Animator parameters/transitions for normal animation routing while `MonsterUnitActor` becomes reusable by other characters.

## Task: 2026-05-26 Rin-E SingleAttack Core Hitbox Skill Completion

### Task title

Implement Rin-E base skill, enhancement traits, and master effects on the shared SingleAttack prefab-hitbox path.

### Goals

- Use `Assets/Prefab/Skill/Rin/Rin_E.prefab` as Rin-E's skill effect prefab.
- Let `CoreHitBox` child colliders drive center-only Rin-E effects.
- Implement Rin-E trait 1-5 and master 1-2 without Rin-only hardcoded branches.
- Keep center damage, center Fire bonus, hit-count cooldown refund, Dark extra damage, and master-2 slow on shared runtime/data extensions.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprint is `boards/SkillBluePrint/single-attack-blueprint.md`.
- User explicitly approved the reusable shared extension for behavior outside the original SingleAttack common contract.
- Unity Play Mode gameplay verification remains user-owned.
- Unity CSV runtime catalog sync is pending because batchmode reported another Unity instance has this project open.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and synced through the open Unity Editor menu after the follow-up CSV validation fix.

### Next Actions

- User verifies in Play Mode that Rin-E uses `Rin_E.prefab`, base hit timing follows current `damage_delay_seconds`, and `CoreHitBox` center hits apply center-only effects.
- User verifies in Play Mode that Rin-E master 2 applies the intended slow to hit enemies.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Rin/Rin_E.prefab` contains a child named `CoreHitBox` with an enabled `BoxCollider2D`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `skill_effect_prefab_path` and `rin-e` points to `Assets/Prefab/Skill/Rin/Rin_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `core_hitbox_name`, `core_damage_multiplier`, `core_on_hit_additional_damage_*`, and `hit_count_cooldown_refund_*` columns.
- Rin-E trait rows are now `RuntimeImplemented`: trait 1 damage `1.3`, trait 2 radius `1.25`, trait 3 cooldown `0.8`, trait 4 `CoreHitBox` damage `1.5`, and trait 5 `rin-b` cooldown refund ratio `0.2` when at least 3 targets are hit.
- Rin-E master rows are now `RuntimeImplemented`: master 1 damage `2`, radius `0.8`, and core Fire additional damage `1`; master 2 damage `1.35`, radius `1.5`, and Dark on-hit additional damage `0.45`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now includes `rin-e-master2-slow`, an OnHit status effect that applies `slow` for `2` seconds with `status_move_speed_bonus=-0.25`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now separates configured core hitbox colliders, applies core-only damage/extra damage, applies SingleAttack OnHit status effects, and applies hit-count cooldown refunds after a cast.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed only because Unity batchmode reported another Unity instance has `C:/TowerDefence_Pakuri/Test/Pakuri` open.
- Follow-up enum validation found the `DamageAttribute` enum defines `Darkness`, not `Dark`; `rin-e-master-2` and `rin-e-master2-slow` were corrected from `Dark` to `Darkness`, and a CSV enum scan returned `ENUM_VALIDATION_OK`.
- Follow-up status-scope validation found `StatusEffectRuntime.TryParseStatusTargetScope(...)` only accepts `self` and `all_allies`; `rin-e-master2-slow` now leaves `status_target_scope` blank like `rin-c-master2-slow`, while `target_side=Enemy` still makes the OnHit status enemy-targeted.
- CSV source scans returned `STATUS_TARGET_SCOPE_OK`, `STATUS_MERGE_POLICY_OK`, and `DAMAGE_ATTRIBUTE_ENUM_OK` for `monster_skill_effects.csv`.
- `.NET TextFieldParser` scans returned `FIELD_COUNT_OK` for `monster_skill_effects.csv` 61 columns / 78 lines, `monster_skill_choices.csv` 86 columns / 252 lines, `monster_skills.csv` 57 columns / 52 lines, and `monster_skill_triger.csv` 34 columns / 10 lines.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the fix.

### History

- 2026-05-26: User asked Code Builder and Skill Builder to apply the approved SingleAttack CoreHitBox extension and implement Rin-E with all enhancement and master effects using `Assets/Prefab/Skill/Rin/Rin_E.prefab`.
- 2026-05-26: User reported Unity auto-sync failing on `monster_skill_effects.csv` row 78 because `attribute=Dark` was not a valid `DamageAttribute`; Builder corrected the enum value to `Darkness` and checked CSV enum columns for the same class of error.
- 2026-05-26: User reported Unity CSV validation still failing on `rin-e-master2-slow status_target_scope=enemy`; Builder cleared that unsupported scope, verified the relevant CSV schemas and enum/status-scope scans, and synced the runtime catalog through the open Unity Editor menu.

## Task: 2026-05-26 Rin Unit Animator Runtime Hook

### Task title

Connect Rin's prefab Animator to the current Scripts2 active-skill, hit, and death runtime events.

### Goals

- Add an `Animation_Controller` runtime component that drives the existing `Rin_Animation_Cont` states directly.
- Play one random Rin attack state whenever a non-triggered active skill cast is successfully routed.
- Play Rin hit animation on non-lethal monster damage and Rin death animation before the dead unit is destroyed.
- Keep the first implementation scoped to `Assets/Prefab/Monster/Rin_Unit.prefab`.

### Constraints

- Role Owner is Code Builder.
- The existing animator controller has no parameters or transitions, so direct `Animator.Play(...)` calls are used.
- Runtime animation requests are gated by the unit model `DefinitionId == "rin"` in `MonsterUnitActor`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally verified through compile, prefab inspection, and Unity editor code inspection.

### Next Actions

- User verifies in Play Mode that Rin plays random attack animations on active skill casts, hit animation on non-lethal damage, and death animation at HP 0.
- If other monsters need the same behavior later, promote the Rin-only model gate into shared data or prefab configuration.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Animation/Animation_Controller.cs` now resolves the local `Animator`, plays `Anim_Rin_Attack_1`, `Anim_Rin_Attack_2`, or `Anim_Rin_Attack_3` randomly, plays `Anim_Rin_Hit`, locks on `Anim_Rin_Dead_1`, and returns transient attack/hit states to idle after the clip length.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now exposes `TryPlayActiveSkillAnimation()`, `TryPlayHitAnimation()`, and `TryPlayDeathAnimation()` and only routes them when the model definition id is `rin`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now calls the active-skill animation hook only after `executor.Execute(...)` routes and `runtime.TryBeginCast(snapshot)` succeeds.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls the hit animation for non-lethal monster damage and the death animation before `Destroy(actor.gameObject, 0.95f)`.
- `Pakuri/Assets/Prefab/Monster/Rin_Unit.prefab` now has `Pakuri.InGame.Animation_Controller` on the root beside `MonsterUnitActor` and `Animator`.
- Unity editor code inspection returned `actor=True|animator=True|animationController=True|controllerName=Rin_Animation_Cont|clips=Anim_Rin_Idle,Anim_Rin_Attack_1,Anim_Rin_Attack_2,Anim_Rin_Attack_3,Anim_Rin_Dead_1,Anim_Rin_Hit`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone; the first parallel attempt hit only an `Assembly-CSharp.dll` file lock.

### History

- 2026-05-26: User asked Code Builder to implement the Designer-approved Rin animation plan using `Animation_Controller.cs`, `Rin_Unit.prefab`, and the existing `Rin_Animation_Cont.controller`.

## Task: 2026-05-26 Rin-D Execute Gate And Execute-Only Kill Effects

### Task title

Implement Rin-D cast gating at the execute threshold and restrict execute-only kill rewards to the primary Rin-D hit on shared Scripts2 runtime paths.

### Goals

- Make `rin-d` reject casts unless its selected target is within the current execute threshold.
- Keep Rin-D target ordering on the existing `LowestHealth` raw-current-health selection.
- Make Rin-D master 1 cooldown reset and Holy burst require an execute kill from the primary `rin-d` hit.
- Keep the fix on shared `SingleAttack`, damage, and trigger runtime paths without Rin-only hardcoded branches.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested that target selection remain unchanged.
- New behavior is data-driven through shared CSV/runtime flags instead of monster-specific branches.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-D does not cast above the current execute threshold.
- User verifies that Rin-D master 1 Holy burst triggers only on execute kills from the primary Rin-D hit and does not chain its own kill reset behavior.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now adds shared `require_execute_threshold_to_cast` and sets it to `true` on `rin-d`.
- The same `monster_skills.csv` file had to be normalized so all active skill rows carry the new trailing column; a post-fix CSV field-count scan returned `ALL_ROWS_OK` for 55-column rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds shared `kill_resets_cooldown_requires_execute` and sets it to `true` on `rin-d-master-1`.
- The same `monster_skill_choices.csv` file had to be normalized so pre-existing choice rows also carry the new trailing column; post-fix field-count scans returned `UTF8_ALL_ROWS_OK` and `ALL_ROWS_OK_AFTER_BOM` for 78-column rows after the file was rewritten as UTF-8 BOM.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now adds shared `require_event_execute` and sets it to `true` on `rin-d-master1-kill-burst`.
- The same `monster_skill_triger.csv` file had to be normalized so pre-existing trigger rows also carry the new trailing column; a post-fix CSV field-count scan returned `ALL_ROWS_OK` for 34-column rows.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now rejects `SingleAttack` casts when `RequireExecuteThresholdToCast` is enabled and the selected target is above threshold, and it passes execute-hit state into shared kill recovery.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now carry shared execute-kill context through damage and kill trigger dispatch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now attributes triggered damage to `triggered_skill_id` first, so triggered Holy burst kills no longer report as primary `rin-d` kills.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after a rerun; the first parallel attempt failed only because `Assembly-CSharp.dll` was temporarily locked by the concurrent runtime build.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the `monster_skill_choices.csv` row-width normalization follow-up.

### History

- 2026-05-26: User requested Code Builder implementation so Rin-D casts only below threshold, keeps current target selection, and applies master-1 cooldown reset only to execute kills from the primary Rin-D hit.
- 2026-05-26: Follow-up fix normalized `monster_skills.csv` row widths after Unity CSV runtime sync reported row 3 had 54 columns while the updated header expected 55.
- 2026-05-26: Additional follow-up fix normalized `monster_skill_triger.csv` row widths after Unity CSV runtime sync reported row 3 had 33 columns while the updated header expected 34.
- 2026-05-26: Additional follow-up fix normalized `monster_skill_choices.csv` row widths after Unity CSV runtime sync reported row 3 had 77 columns while the updated header expected 78, then rewrote the file as UTF-8 BOM and re-synced the runtime catalog successfully.

## Task: 2026-05-26 Rin-D Execute Condition And Master Effect Audit

### Task title

Audit current `rin-d` SingleAttack runtime against the authored execute threshold, enhancement effects, and master effects.

### Goals

- Verify whether `rin-d` cast gating matches the authored 30% execute-health behavior.
- Verify that Rin-D trait and master choice fields map into the current Scripts2 runtime as intended.
- Record confirmed implementation mismatches before Builder follow-up.

### Constraints

- Role Owner is Designer.
- Claims are limited to inspected CSV rows and current Scripts2 runtime code.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` does not exist in the current workspace, so no legacy-side comparison was possible from that path.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer

### Status

Inspection completed. Confirmed code/data mismatches exist in the current Scripts2 Rin-D behavior.

### Next Actions

- Builder should decide whether Rin-D must refuse execution unless a target is within the execute threshold, or whether only the execute bonus should be gated while cast remains allowed.
- Builder should add an execute-only gate for Rin-D master-1 kill reset and kill burst if the authored text is meant to apply only to executed targets.
- Builder should review whether Rin-D target selection should prefer lowest health ratio instead of lowest raw current health.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` defines `rin-d` as `runtime_kind=SingleAttack`, `target_selection=LowestHealth`, `execute_health_ratio_threshold=0.3`, `execute_damage_multiplier=1.8`, and `kill_cooldown_refund_ratio=0.35`.
- `Pakuri/reference/2.Monster/rin/skill/d-finishing-blow.md` authors Rin-D around `泥섑삎 湲곗? 泥대젰 30% ?댄븯`, trait 2 `泥섑삎 湲곗? 泥대젰 +10%`, master 1 `泥섑삎 ??곸뿉寃?移섎챸? ?뺣쪧 +50%, 泥섏튂 ??荑⑤떎???꾩쟾 珥덇린??, and master 2 `泥섑삎 湲곗? 泥대젰 -10%`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:708` through `:714` only use the execute threshold to add execute damage and execute crit chance; the cast path itself does not stop when the target is above the threshold.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:359` through `:389` always damage the first ordered target from `ResolveOrderedTargets(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs:68` through `:97` and `SkillExecutionUtility.cs:219` through `:221` implement `LowestHealth` with raw `CurrentHealth`, not health ratio.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` gives `rin-d-master-1` `execute_crit_chance_bonus=0.5` and `kill_resets_cooldown=true`; `rin-d-master-2` gives `damage_multiplier=1.9`, `execute_health_ratio_bonus=-0.1`, `cooldown_multiplier=1.25`, and guaranteed Darkness on-hit additional damage.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:755` through `:767` reset or refund cooldown on any Rin-D kill, without checking whether that hit was an execute hit.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:10` plus `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs:181` through `:212` wire the Rin-D master-1 Holy burst to generic `OnKill`, and the trigger context does not carry an execute/non-execute flag.

### History

- 2026-05-26: User reported that Rin-D seemed to fire even when the opponent was not below 30% HP and asked for a full inspection of the skill, enhancement effects, and master effects.

## Task: 2026-05-26 Rin-B And Rin-C Skill Builder Completion

### Task title

Implement Rin-B and Rin-C active enhancement/master effects on the current Scripts2 shared skill runtime.

### Goals

- Keep `rin-c` on the shared `LineAttack` / beam runtime and finish all trait/master effects.
- Keep `rin-b` on the shared buff runtime and finish all trait/master effects, including the ally-wide master follow-up damage.
- Reuse shared CSV/runtime paths instead of adding Rin-only hardcoded branches.

### Constraints

- Role Owner is Skill Builder.
- User explicitly authorized current Rin CSV/reference files as the parsed source for this task.
- Base skill visuals remain scene-owned through `NewRunScene` `EffectManager`; choice/effect behavior remains data-owned through the active CSV/runtime pipeline.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder

### Status

Implemented, compile-verified, and Unity refresh-checked.

### Next Actions

- User verifies in Play Mode that Rin-B ally buffs and Rin-C knockback / reload reduction / lightning follow-up / slow behave as authored.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now keeps `rin-b` at `status_duration_seconds=5` and `status_action_speed_bonus=0.2`, and adds `knockback_distance=0.6` to `rin-c`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks all `rin-b-*` and `rin-c-*` choice rows as `RuntimeImplemented`; `rin-c-trait-2` uses `beam_width_bonus=0.25`, `rin-c-trait-3` uses `knockback_distance_multiplier=1.4`, `rin-c-trait-5` uses `reload_reduce_target_skill_id=rin-a` plus `reload_reduce_seconds_per_hit=0.25`, `rin-c-master-1` uses the shared on-hit lightning fields, and `rin-c-master-2` uses `beam_width_bonus=0.6`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `rin-b-trait2-action-speed`, `rin-b-trait4-self-attack`, `rin-b-trait5-crit`, `rin-b-master1-roar`, `rin-b-master2-abyss`, and `rin-c-master2-slow`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/BeamSkillExecutor.cs`, `InGameLineAttackActor.cs`, `SkillOnHitAdditionalDamageUtility.cs`, and `SkillRuntimeInstance.cs` now cover Rin-C width/knockback/reload-reduction/on-hit additional damage on the shared beam path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SupportSkillExecutors.cs`, `SkillMultiEffectExecutor.cs`, `StatusEffectRuntime.cs`, and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now cover Rin-B scaled buff multi-effects and status-driven outgoing additional damage on the shared buff/status path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` also passed with 0 errors after rerun outside the sandbox-denied path. Existing MSB3277 warnings remain.
- Unity `refresh_unity` returned `resulting_state":"idle"`, and warning/error console reads after the refresh showed only MCP-FOR-UNITY client-handler logs, not C# or CSV runtime errors.

### History

- 2026-05-26: User instructed Skill Builder to start with Rin-C, explicitly approved current Rin CSV/reference files as parsed source, and then requested the same treatment for Rin-B.

# RIN_MONSTER

## Task: 2026-05-24 Rin-A Master-2 On-Hit Lightning Revision

### Task title

Revise `rin-a-master-2` from projectile branch launch behavior to shared on-hit Lightning additional damage and every-third-hit chain damage.

### Goals

- Make every Rin-A primary hit apply Lightning additional damage equal to 40% of the resolved hit damage.
- Make every 3rd Rin-A primary hit chain Lightning damage equal to 40% of the resolved hit damage to up to 2 enemies near the hit target.
- Keep the behavior on a shared on-hit damage extension usable by projectile, beam, zone, and single-attack hit paths.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- User provided the parsed behavior values in the request.
- No Rin-only hardcoded runtime branch was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-A master-2 applies the direct Lightning extra hit on each hit and chains to 2 nearby enemies every 3rd primary hit.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now gives `rin-a-master-2` `on_hit_additional_damage_chance=1`, `on_hit_additional_damage_multiplier=0.4`, `on_hit_additional_damage_attribute=Lightning`, `on_hit_additional_damage_target=HitTarget`, `on_hit_chain_hit_period=3`, `on_hit_chain_target_count=2`, `on_hit_chain_search_radius=4.5`, `on_hit_chain_damage_multiplier=0.4`, and `on_hit_chain_damage_attribute=Lightning`.
- The same row now has blank `branch_chance_set`, `branch_count`, `branch_damage_multiplier`, `branch_launch_period`, and `branch_launch_chance_set`, so it no longer uses the projectile branch launch override path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillOnHitAdditionalDamageUtility.cs` applies shared direct on-hit extra damage and every-nth-hit chain damage.
- Unity-MCP editor execution returned `rin-a-master-2|extra=True:1:0.4:Lightning:HitTarget|chain=3:2:4.5:0.4:Lightning|branch=False:False:0:False`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-24: User clarified that master-2 should be on-hit Lightning additional damage plus every-third-hit chain damage, not projectile branch launch chance behavior.

## Task: 2026-05-24 Rin-A Choice Runtime Completion

### Task title

Implement Rin-A remaining enhancement and master choice data on the shared projectile path.

### Goals

- Move `rin-a-trait-5` from partial support to shared projectile critical bonus fields.
- Keep `rin-a-master-1` on existing shared damage, magazine, and shot-interval fields.
- Implement `rin-a-master-2` with shared projectile branch behavior plus every-third-projectile-launch chance override.
- Use `Assets/Prefab/Skill/Rin/Rin_A.prefab` for Rin-A master-2 effect prefab resolution.

### Constraints

- Role Owner is Skill Builder.
- User explicitly approved treating current CSV/code as the parsed source.
- Implementation stayed on the selected projectile blueprint common path and did not add Rin-only hardcoded runtime logic.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-A trait 5 applies critical chance/damage bonuses.
- User verifies in Play Mode that Rin-A master 2 branches with 40% chance normally and 100% chance every 3rd projectile launch.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now includes `branch_launch_period` and `branch_launch_chance_set`.
- `rin-a-trait-5` now has `crit_chance_bonus=0.1`, `crit_damage_bonus=0.25`, blank `damage_multiplier`, and `runtime_support_state=RuntimeImplemented`.
- `rin-a-master-1` remains data-authored with `damage_multiplier=1.12`, `magazine_bonus=6`, and `shot_interval_multiplier=0.8200000000000001`.
- `rin-a-master-2` now has `skill_effect_prefab_path=Assets/Prefab/Skill/Rin/Rin_A.prefab`, `branch_chance_set=0.4`, `branch_count=2`, `branch_damage_multiplier=0.4`, `branch_search_radius=4.5`, `branch_launch_period=3`, `branch_launch_chance_set=1`, and `runtime_support_state=RuntimeImplemented`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `Assets/Prefab/Skill/Rin/Rin_A.prefab` with GUID `19bfba788239eba498a44cb67c2622c6`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-24: User authorized current CSV/code as parsed source for Rin-A master-2, remaining enhancements, and master-1 implementation.

## Task: 2026-05-26 Rin F-J Passive Shared Trigger/Status Implementation

### Task title

Implement Rin passive F-J on shared status/effect/trigger runtime.

### Goals

- Implement Rin-F delayed follow-up attacks through `SingleAttack` trigger rows with `trigger_delay_seconds=0.3`.
- Implement Rin-H as all-allied physical-damage count tracking before triggering auto shockwave rows.
- Keep Rin-G, Rin-I, and Rin-J authored through common status/effect/trigger structures instead of Rin-only hardcoded runtime branches.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- User explicitly approved the shared extensions needed for all-allied physical count, trigger action, cooldown/reload reduction, trigger delay, and status/effect conditions.
- Skill Builder CSV reads stayed limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and CSV-validation/build verified.

### Next Actions

- User verifies in Play Mode that Rin-F follow-up uses `Assets/Prefab/Skill/Rin/Rin_F.prefab` after 0.3 seconds.
- User verifies in Play Mode that Rin-H counts all allied physical damage events and fires on the configured 10-hit / 8-hit trait cadence.
- User verifies in Play Mode that Rin-G/I/J passive effects and cooldown/reload reductions match the design sheet intent.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `rin-f-followup`, `rin-f-followup-trait2`, and `rin-f-followup-lightning-trait3` with `trigger_action=SingleAttack`, `event_skill_id=rin-a;rin-c;rin-d;rin-e`, `event_source_scope=owner`, and `trigger_delay_seconds=0.3`; the physical rows use `Assets/Prefab/Skill/Rin/Rin_F.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains Rin-H all-ally physical trigger rows with `trigger_attribute=Physical`, `event_source_scope=all_allies`, and `trigger_every_count=10` or `8` depending on trait 1.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains Rin F-J passive status/effect rows including `rin-i-finishing-kill-crit-damage-trait2`, `rin-j-physical-defense-down`, and `rin-j-hitcount-action-speed`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now supports `trigger_action`, `event_skill_id`, `event_source_scope`, `trigger_delay_seconds`, `trigger_every_count`, effect triggers, cooldown refund, and reload reduction.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now stores passive trigger counts and dispatches skill-cast triggers; `SkillExecutionSystem.cs` dispatches active skill-cast events after routed non-triggered casts.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` and `SingleAttackSkillExecutor.cs` now support `OnHitCount` multi-effects for hit-count-gated shared passive effects.
- CSV field-count scan passed: `monster_skill_effects.csv` 64 columns / 91 lines, `monster_skill_triger.csv` 44 columns / 26 lines, `monster_skill_choices.csv` 86 columns / 252 lines, and `monster_skills.csv` 57 columns / 52 lines.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and did not log the prior `unsupported status_target_scope` CSV error.

### History

- 2026-05-26: User approved extending the shared trigger/status runtime, then clarified Rin-H should count all allied physical-damage skill usage before triggering.

## Task: Rin partial runtime skill prefab decommission

### Task title

Rin partial runtime skill prefab decommission

### Goals

- Delete migrated Rin A/B/C/D Master 1/F prefabs and retain Rin-D/Rin-E exceptions.

### Constraints

- Rin-D scene mapping and Rin-E runtime CSV prefab path remain active; collider offset remains `(0, 0)`.

### Role Owner

- Code Builder

### Status

- Code complete; user Play Mode verification pending.

### Next Actions

- User verifies Rin A-C/F runtime visuals and Rin D/E retained prefab behavior in Play Mode.

### Evidence

- Rin A Master 2 and Rin D Master 1/F trigger prefab references were removed.
- Rin D Master 1 collider offsets were normalized to `0,0`.
- `Assets/Prefab/Skill/Rin` now retains only Rin-D and Rin-E prefabs.

### History

- 2026-07-14: Code Builder completed the approved partial Rin prefab decommission.

## Source: boards\MON\SEIN_MONSTER.md

## Task: 2026-05-28 Sein-I Base And Sein-G Trait-3 Shared Runtime Completion

### Task title

Extend the shared passive and trigger runtime so Sein-I base and Sein-G trait-3 can be fully implemented.

### Goals

- Add a shared passive-base modifier path that can modify an existing active skill while the passive is learned.
- Add a shared triggered-skill cast marker path so a passive can react only to an auto-triggered child skill cast.
- Finish the remaining Sein-I base and Sein-G trait-3 CSV implementation on top of those shared paths.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The work stays inside the Sein board, the DATA board, the passive-stat blueprint authority, the current shared runtime scripts, and the routed Sein CSV files.
- No Play Mode gameplay verification is performed by Codex.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and Unity editor-validated.

### Next Actions

- User verifies in Play Mode that learned `sein-i` speeds up `sein-d` tick cadence by 20%.
- User verifies that only Sein-G auto-triggered `sein-b` casts reduce `sein-a` reload, while manual `sein-b` casts do not.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now define and build shared `PassiveBase` modifier rows through `PassiveDefinition.BaseModifierChoices`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now applies learned passive base modifiers to active skill snapshots before chosen enhancement choices are added.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now forward a trigger-source skill marker through triggered skill cast execution, and triggered skills now dispatch shared `OnSkillCast` trigger events.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains `sein-i-base-shot-interval` and marks `sein-g-trait-3` as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `sein-g-auto-barrage-reload-trait3`, which uses the shared triggered-cast origin marker plus `condition_status_source_skill_id=sein-g` to gate the reload reduction to Sein-G-origin casts only.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed after one transient initial file-lock failure on `obj\Debug\Assembly-CSharp.dll`; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary, and `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-28: Code Builder added a shared passive-base modifier group and a shared triggered-skill cast origin marker path, then authored the remaining Sein-I base and Sein-G trait-3 rows on top of that runtime support.

## Task: 2026-05-27 Sein Passive CSV-Only F/H/I/J Authoring

### Task title

Implement the Sein passive portions that are already expressible through current CSV/runtime paths, without adding shared runtime logic.

### Goals

- Author `sein-f`, `sein-h`, the status portion of `sein-i`, and `sein-j` through existing status/effect/trigger CSV fields.
- Keep `sein-i` base tick-speed modification and `sein-g-trait-3` trigger-origin behavior outside this pass because their required shared paths are not present in the inspected runtime.
- Sync and validate the authored source CSV data through the Unity editor menus.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Routed edit set is limited to `monster_skill_effects.csv`, `monster_skill_choices.csv`, and `monster_skill_triger.csv`; Unity sync updates the generated runtime catalog asset.
- No shared runtime/common-logic extension is authored in this pass.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and editor-validated for the CSV-only scope. `sein-i` base tick-speed `+20%` and exact `sein-g-trait-3` remain pending shared runtime work.

### Next Actions

- Implement the approved shared passive-base to active-skill numeric modifier path for `sein-i` base tick speed.
- Implement a marker or composite trigger path so only `sein-g`-auto-triggered `sein-b` can reduce `sein-a` reload for `sein-g-trait-3`.
- Run gameplay verification in Unity Play Mode after the shared-runtime remainder is implemented.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` rows `sein-f-fire-damage` through `sein-e-passive-j-fire-resist-down-trait3` add 12 current-runtime status effects: F ally fire/crit effects, H C-hit fire-resist/exposure effects, I D-hit exposure effects, and J E-hit exposure/resist effects.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` rows `sein-f-trait-1` through `sein-j-trait-3` now route active-skill traits to `sein-a`, `sein-c`, `sein-d`, or `sein-e` where required and record the CSV-authored effects as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` rows `sein-j-kill-sein-a-reload` through `sein-j-kill-sein-e-cooldown-trait2` add 12 existing-action triggers for Sein-J: base 20% and trait-2 additional 10% cooldown/reload refunds after `sein-e` kills.
- `TextFieldParser` validation after authoring returned `FIELD_COUNT_OK` for the three routed CSV files: effects `columns=66 lines=110`, trigger `columns=44 lines=42`, choices `columns=89 lines=252`.
- Unity menu `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog ... with 5 monsters and 8 stage-one enemies.` and `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.
- The only exception entries emitted during the isolated Unity menu run were MCP transport connection-exit logs sourced from `Library/PackageCache/com.coplaydev.unity-mcp.../StdioBridgeHost.cs`, not CSV validation failures.

### History

- 2026-05-27: Skill Builder / Code Builder edited only CSV-solvable Sein passive behavior, field-validated the routed CSV set, then used Unity validation and catalog sync menus. Shared-runtime-only I base and G trait-3 behavior remained excluded.

## Task: 2026-05-27 Designer Review Of Sein Passive F-J Blockers

### Task title

Re-check whether Sein passive F-J requires new shared runtime work, or whether the existing CSV and runtime paths already cover the requested behavior.

### Goals

- Verify the reported F-J blockers against inspected CSV and runtime code instead of assuming the prior Skill Builder conclusion is complete.
- Separate behavior already expressible through current CSV paths from exact behavior that still lacks an execution path.
- Keep the review within the active Sein board, the selected passive blueprint, the routed F-J CSV rows, and the runtime files named or directly reached by the blocker.

### Constraints

- Role Owner is Designer; this task records design verification only and does not implement CSV or runtime changes.
- The inspected active CSV set was limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `status_effects.csv`.
- No reference markdown, archive markdown, unrelated monster board, UI board, RUN board, or DATA board was opened for this review.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Designer

### Status

Reviewed. The previous blocker list is over-broad: new passive-specific status kinds are not required for exact F/H/J status effects or the status portion of I. Two exact behaviors still require user-approved shared runtime work.

### Next Actions

- Return F/H/J and the status portion of I to Skill Builder as CSV-only work using existing `passive-buff`, `fire-resist-down`, and `fire-exposure` status ids plus existing `requires_passive_skill_id` / trigger paths.
- Before authoring trait-gated active effects, set the relevant passive choice `target_skill_id` to the affected active skill so the active skill snapshot can see that chosen passive enhancement.
- Ask the user whether to approve a shared runtime path for:
  - `sein-i` base applying `Sein-D` tick-speed `+20%` while the passive is learned;
  - `sein-g-trait-3` reducing `Sein-A` reload only after a `Sein-G`-triggered `Sein-B` execution.

### Evidence

- `boards/SkillBluePrint/passive-stat-blueprint.md` treats always-on skill-specific numeric modifiers and status-rule numeric modifiers as ordinary passive-stat behavior, while event-driven follow-ups remain stop-and-ask behavior.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` has no retained `sein-f`, `sein-h`, `sein-i`, or `sein-j` execution rows; their `monster_skills.csv` rows are marked `RuntimeImplemented` but currently only carry summary text for these effects.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` sets status `SourceSkillId` from each `effect_id`, defaults merge policy to `SameSourceRefresh`, supports `requires_passive_skill_id`, and copies flat fire-resist and element-damage-taken numeric payloads into runtime statuses.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` keeps source-aware statuses of the same `StatusEffectKind` separate by source id, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` sums their flat element resistance and element damage-taken values. Existing `fire-resist-down`, `fire-exposure`, and `passive-buff` ids therefore cover distinct additive F/H/I/J status effects without new enum ids.
- Existing CSV rows `ariel-e-passive-j-action-speed`, `rin-j-physical-defense-down`, and their trait variants already demonstrate active-skill effects gated by `requires_passive_skill_id` and passive enhancements routed to the affected active skill through `target_skill_id`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` already supports passive-owner `OnKill`, `CooldownRefund`, and `ReloadReduce` actions, so Sein-J kill-based cooldown refund can be authored as existing trigger rows, including separate target rows when multiple Sein active skills must be refunded.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` only adds choice modifiers to an active skill snapshot from chosen choices whose target resolves to that active skill. No inspected path applies a learned passive base row directly to `sein-d` tick interval, so `sein-i` base tick-speed `+20%` remains a real shared-runtime gap.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` executes one trigger action per trigger row and forwards a triggered skill as `sein-b`; no inspected CSV-visible marker identifies that B cast as specifically produced by `sein-g`. Therefore exact `sein-g-trait-3` remains blocked without a shared marker or composite-action path.

### History

- 2026-05-27: Designer inspected the passive-stat blueprint, routed Sein F-J CSV rows, status merge/runtime code, passive multi-effect gating, trigger actions, and active snapshot choice application. The inspection disproved the proposed new-status-kind blocker for F/H/I/J status payloads and retained only the I base tick-speed and G trait-3 trigger-origin gaps as shared-runtime decisions.

## Task: 2026-05-27 Sein Passive F-J Skill Builder Blocker Review

### Task title

Attempt Sein passive F-J Skill Builder authoring from the passive-stat blueprint, keep only validator-safe progress, and record the shared-runtime blockers that prevent exact completion.

### Goals

- Implement Sein passive `f-heated-aim`, `g-flame-barrage`, `h-burning-trajectory`, `i-thermal-spread`, and `j-doomsday-omen` through the routed passive skill CSV set when the existing shared runtime permits it.
- Keep repository data validator-clean while testing whether the current passive/stat/trigger paths are sufficient.
- Stop before widening into a new shared runtime/common-logic extension that was not yet user-approved for this passive task.

### Constraints

- Role Owner is Skill Builder.
- Routed CSV read/edit set was limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `status_effects.csv`.
- Reference inputs came only from `Pakuri/reference/2.Monster/sein/skill/f-heated-aim.md` through `j-doomsday-omen.md` plus `boards/SkillBluePrint/passive-stat-blueprint.md`.
- New shared runtime/common-logic extensions discovered during authoring must stop and be reported instead of being silently invented.

### Role Owner

Skill Builder

### Status

Partially advanced, then stopped at shared-runtime blockers. Repository restored to validator-clean state.

### Next Actions

- Ask the user whether to widen scope into shared runtime work for:
  - new supported runtime status kinds or a string-backed passive status path so F/H/I/J can apply distinct additive passive debuffs/buffs without enum rejection;
  - a passive-base skill-modifier path so Sein-I base can grant `Sein-D` tick-speed `+20%` without relying on a selectable enhancement row;
  - a trigger marker path so `Sein-G trait-3` can detect “auto-triggered Sein-B only” before reducing `Sein-A` reload.
- If the user approves that wider shared work, resume from this blocker list instead of rediscovering it.

### Evidence

- `boards/SkillBluePrint/passive-stat-blueprint.md` explicitly limits the common path to always-on stat/skill/status modifiers and says event-driven passives should stop and ask rather than be guessed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` shows only chosen passive-enhancement rows can modify an active skill snapshot via `SkillChoiceResolver`; there is no current passive-base path that directly applies `Sein-I` base `Sein-D` tick-speed `+20%`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` accepts only the enum-backed ids through `StatusEffectUtility.TryParse(...)`; attempted new ids for distinct Sein passive statuses were rejected by the CSV validator.
- Unity editor validation, after reverting the unsupported new passive status/effect rows, logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Valid Sein-G progress was retained:
  - `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `sein-g-auto-barrage-base`, `sein-g-auto-barrage-trait1`, and `sein-g-auto-barrage-trait2`.
  - `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `sein-g-trait-1` and `sein-g-trait-2` as `RuntimeImplemented`, while `sein-g-trait-3` remains `DataOnlyUnsupported`.

### History

- 2026-05-27: Skill Builder first attempted full F-J authoring with new passive-specific status ids and trigger/effect rows, then proved through Unity validation and direct runtime reflection that those ids are rejected because they are not supported by `StatusEffectKind`.
- 2026-05-27: Builder restored the invalid F/H/I/J passive status/effect rows, kept only validator-safe Sein-G trigger authoring, and recorded the remaining shared-runtime blockers for a later explicit user decision.

## Task: 2026-05-27 Sein-G Triggered Skill Damage Override Support

### Task title

Add the minimum shared runtime path needed for Sein-G so a passive trigger can launch a triggered active skill with a trigger-scoped damage multiplier.

### Goals

- Let passive or trigger runtime launch a triggered active skill with a damage multiplier that applies only to that triggered execution.
- Keep the change shared and minimal so existing triggered-skill callers remain behavior-compatible when they do not pass an override.
- Avoid adding broader trigger snapshot or custom Sein-only branches before Sein-G authoring begins.

### Constraints

- Role Owner is Code Builder.
- Only the shared triggered-skill execution path is extended in this step; no Sein-G CSV authoring is part of this change.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Skill Builder can now author Sein-G trigger rows that call `sein-b` with a trigger-row `damage_multiplier`, knowing that the multiplier will be applied only to that triggered B execution.
- User still needs Play Mode verification after Sein-G rows are authored.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now accepts an optional `triggeredDamageMultiplier` when forwarding triggered-skill execution to the skill system.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now accepts that override on the triggered execution path and applies it through `SkillExecutionSnapshot.ApplyDynamicDamageMultiplier(...)` before executor dispatch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now forwards trigger-row `damage_multiplier` into triggered-skill execution instead of dropping it for `TriggerActionKind.TriggeredSkill`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after the change; only the existing `MSB3277` warnings remained.

### History

- 2026-05-27: Designer review concluded that Sein-G could already express the ally-fire trigger, proc chance, cooldown gate, and reload reduction, but lacked a shared path for “triggered B only deals 60% damage.”

## Task: 2026-05-27 Sein-E Manual Parallel Deployment Visual Fix

### Task title

Fix Sein-E manual casting so multi-deployment lines no longer collapse onto one center and so the prefab visual is scaled/oriented like the old line attack presentation.

### Goals

- Make manual Sein-E cast multiple deployments appear simultaneously instead of overlapping at one center.
- Align manual Sein-E deployment centers in parallel to the clicked direction.
- Restore large line-like visual scaling for `Sein_E.prefab` under the new `SingleAttack` multi-deployment runtime.

### Constraints

- Role Owner is Code Builder.
- The fix stays in shared `SingleAttackSkillExecutor` runtime code; no CSV or prefab content rewrite is part of this step.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that manual Sein-E now shows 3 simultaneous parallel lines in the clicked direction.
- User verifies the line visual length/width now matches the intended old `LineAttack` presentation more closely instead of spawning as a very small effect.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now routes both auto and manual Sein-E multi-deployment casts through the same shared target-anchored center allocation path instead of keeping a manual-only parallel-center branch.
- The same executor still rotates and rescales multi-deployment prefabs using the old line-style presentation rule (`length=31`, width from resolved skill radius), but its facing now follows the resolved deployment center direction just like auto casting.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillDeploymentCenterUtility.cs` no longer disables nearest-distinct target-center allocation just because the cast came from manual aim input.
- `Pakuri/Assets/Prefab/Skill/Sein/Sein_E.prefab` still has local scale `0.22610311` and a `BoxCollider2D`, which explained why the unadjusted `SingleAttack` presentation looked much smaller than the previous line actor presentation.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 code errors after the fix; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 code errors when rerun sequentially; only the existing `MSB3277` warnings remained.

### History

- 2026-05-27: User reported that Sein-E only showed one small line when cast manually, which inspection traced to manual center duplication in the new multi-deployment path and to the loss of the old line actor's visual scaling rule.
- 2026-05-27: User then reported that manual Sein-E still behaved differently from auto; Builder removed the manual-only parallel deployment branch so manual casts now use the same nearest-distinct target-center allocation as auto casts.

## Task: 2026-05-27 Sein-E Presence Zone CSV Validation Fix

### Task title

Allow Sein-E / Sein-D status-only persistent zones to pass shared CSV validation without inventing fake damage values.

### Goals

- Keep `sein-d-zone-presence` and `sein-e-master2-zone-presence` authored as zero-damage status-refresh zones.
- Fix the shared CSV validator so these rows do not require fake positive `base_damage`.
- Verify the original Unity error no longer appears when `Pakuri/Validate CSV Source Data` runs.

### Constraints

- Role Owner is Code Builder.
- The fix must stay on shared validation logic instead of mutating Sein data into unintended damage rows.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- User verifies in Play Mode that Sein-D and Sein-E master-2 presence zones still refresh `sein-d-superheated-presence` without adding unintended damage.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now allows `SkillMultiEffectKind.Damage` rows with `base_damage <= 0`, zero coefficients, persistent-zone timing, and status payloads to pass validation as status-only persistent zones.
- The authored rows remain `sein-d-zone-presence` and `sein-e-master2-zone-presence` in `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv`; no fake damage numbers were added.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun sequentially after one file-lock failure caused by parallel compilation.
- Unity menu `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and did not log the earlier `requires positive base_damage` errors for `sein-d-zone-presence` or `sein-e-master2-zone-presence`.

### History

- 2026-05-27: User reported Unity startup validation errors because status-only presence zones were authored as `Damage` effects with `base_damage=0`, which the shared validator previously rejected unconditionally.

## Task: 2026-05-27 Sein-E Multi-Deployment SingleAttack And Master Zone Follow-Up

### Task title

Extend shared `SingleAttack` runtime for Sein-E multi-deployment casting and implement Sein-E enhancement/master rows on that shared path.

### Goals

- Convert `sein-e` from `LineAttack` authoring to `SingleAttack` authoring without losing the intended multi-line cast feel.
- Let one Sein-E cast execute multiple independent prefab-hitbox `SingleAttack` deployments using nearest-distinct target allocation with repeat fallback.
- Implement Sein-E trait-3, trait-4, trait-5, and master-2 through shared multi-effect/status/runtime paths instead of a Sein-only branch.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Selected blueprint is `boards/SkillBluePrint/single-attack-blueprint.md`.
- Routed CSV authority was limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_modifier_skill_choice.csv`, and `status_effects.csv`.
- User explicitly approved widening scope to shared runtime/common-logic extension for the required multi-deployment behavior.
- `Sein-E` must use `Assets/Prefab/Skill/Sein/Sein_E.prefab`.
- `Sein-E master-2` must reuse Sein-D-style zone behavior/effects instead of a separate bespoke prefab family.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that base `sein-e` fires 3 independent hitbox deployments at once, preferring distinct nearby enemies and only overlapping targets when fewer than 3 enemies exist.
- User verifies that `sein-e-trait-4` raises deployment count to 4 while reducing damage to 85%.
- User verifies that `sein-e-master-2` spawns a Sein-D-style zone at each Sein-E deployment center and that `sein-e-trait-5` only gains bonus damage against enemies currently inside those superheated zones.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillDeploymentCenterUtility.cs` now provides shared target-anchored deployment-center allocation with nearest-distinct selection plus repeat fallback.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs`, `InGameSkillDefinitionMapper.cs`, and `Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now support `SingleAttack` multi-deployment prefab-hitbox execution, using `hit_target_count` as base deployment count when the skill is authored as prefab-hitbox `SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` and `Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now add shared `OnDeploymentCast` timing so each Sein-E deployment center can run follow-up effects.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ZoneSkillExecutor.cs` now reuses the same deployment-center utility for existing multi-zone target anchoring and runs cast-timed multi-effects at each spawned zone center.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `sein-e` as `runtime_kind=SingleAttack`, `hit_target_count=3`, base `fire-resist-down` status application for `5s` with flat reduction `10`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `sein-e-trait-1` through `sein-e-master-2` as `RuntimeImplemented`, sets `sein-e-trait-4.hit_target_count_bonus=1` with `damage_multiplier=0.85`, and gates `sein-e-trait-5` on `conditional_target_status_id=sein-d-superheated-presence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now adds `sein-e-trait3-fire-resist-bonus` as a shared `OnHit` status effect, plus `sein-e-master2-zone-damage` and `sein-e-master2-zone-presence` as shared `OnDeploymentCast` persistent-zone follow-ups that reuse Sein-D values and visuals.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains `sein-d-superheated-presence`, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now recognizes that status id in shared runtime parsing/definition paths.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.

### History

- 2026-05-27: User clarified that Sein-E should not be a capped-3-target attack, but a simultaneous multi-cast skill whose independent hitboxes can each hit unlimited enemies.
- 2026-05-27: Builder reused the distinct-target-allocation concept from existing zone runtime behavior, promoted it into a shared deployment-center utility, and added a shared `OnDeploymentCast` follow-up timing so Sein-E master-2 could spawn Sein-D-style zones per deployment center.

## Task: 2026-05-27 Sein-B Manual Burst And Projectile Hold Input Fix

### Task title

Fix Sein-B manual burst continuation and add projectile-only manual hold firing in `NewRunScene`.

### Goals

- Make `sein-b` complete its full burst sequence from one manual click even when player auto-skill is off.
- Allow manual projectile skills to keep firing while the mouse button is held, using the current cursor direction at each shot.
- Keep beam, zone, and single-attack skills on their existing one-click manual behavior.

### Constraints

- Role Owner is Code Builder.
- No CSV, prefab, or scene serialization change is part of this fix; the issue is resolved in runtime input ownership only.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that `sein-b` fires all 5 burst shots from one click in manual mode.
- User verifies that holding the mouse continues firing projectile skills toward the current cursor direction, while non-projectile active skills still react only to click-start.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors `sein-b` with `magazine_capacity=4`, `shot_interval_seconds=0.18`, and `projectile_burst_count=5`; no Sein-B CSV row change was needed.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now stores latched manual projectile input, continues manual execution while a projectile runtime is bursting, and limits hold-repeat behavior to `ProjectileSkillData`.
- The same combat manager now refreshes projectile manual aim from the current cursor position while the mouse button is held, but leaves non-projectile manual skills on `wasPressedThisFrame` behavior.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock on `Assembly-CSharp.dll`.

### History

- 2026-05-27: User reported that Sein-B fired only one manual projectile after DebugUI learn, even though the skill CSV authored a 5-shot burst.
- 2026-05-27: Code Builder confirmed the burst CSV/runtime data were already correct and fixed the manual-input ownership path so projectile bursts can continue without enabling full auto-skill mode.

## Task: 2026-05-27 Sein-C And Sein-D Enhancement/Master Runtime Completion

### Task title

Implement Sein-C and Sein-D enhancement/master behavior on shared projectile, multi-effect, status, and zone runtime paths.

### Goals

- Convert `sein-c` from area-attack authoring to delayed-impact projectile authoring using the shared projectile runtime.
- Implement Sein-C trait/master rows and Sein-D trait/master rows through current shared choice/effect/status paths where possible.
- Reuse shared persistent-zone spawning for Sein-C master-1 and Sein-D master-2 instead of adding Sein-only runtime branches.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprints are `boards/SkillBluePrint/projectile-blueprint.md` for Sein-C and `boards/SkillBluePrint/area-attack-blueprint.md` for Sein-D.
- User explicitly approved widening scope to shared runtime/common-logic extension and CSV schema expansion where needed.
- The following values are user-provided or inferred from the nearest inspected authority and should stay explicit until the user replaces them:
  - `sein-c.projectile_speed=20` is inferred from the requested reuse of `Assets/Prefab/Skill/Sein/Sein_B.prefab`.
  - `sein-a-hit-mark` duration `5s` is inferred for Sein-C trait-5 gating.
  - `sein-c-master-1` residual zone radius `1.2` and tick interval `0.5s` are inferred; the user only specified `25%` damage for `1.5s`.
  - `sein-d-master-2` residual zone radius `3.2` and tick interval `0.5s` reuse inspected base Sein-D values as the nearest available authority.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-C now fires the requested projectile visual, stops on first contact, then explodes after the delay.
- User verifies Sein-C master-2 contact damage and visual, Sein-C master-1 residual flame-zone spawn, and Sein-D master-2 residual ember-zone spawn.
- If the inferred Sein-C/Sein-D zone radius or tick values should change, update the authored effect rows rather than adding new runtime branches.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/c-flame-trajectory.md` and `Pakuri/reference/2.Monster/sein/skill/d-superheated-zone.md` were the inspected skill references for the requested Sein-C and Sein-D behavior bundle.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now sets `sein-c.runtime_kind=CooldownProjectile`, `sein-c.projectile_speed=20`, `sein-c.damage_delay_seconds=0.8`, and `sein-c.skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_C.prefab`; it also sets `sein-d.skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_D.prefab`, `sein-d.active_duration_seconds=4`, `sein-d.shot_interval_seconds=0.5`, and `sein-d.status_effect_id=sein-d-heat-stack`.
- The same skill CSV now marks `sein-a` with `status_effect_id=sein-a-hit-mark`, `status_chance=1`, `status_duration_seconds=5`, `status_max_stacks=1`, and `status_stack_amount=1` so Sein-C trait-5 can stay on a shared conditional-status path.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `damage_delay_multiplier`, updates `sein-c-trait-4` to `0.6`, gates `sein-c-trait-5` on `sein-a-hit-mark`, and authors `sein-c-master-1`, `sein-c-master-2`, and `sein-d-master-2` as `SharedExtension`; it also updates `sein-d-trait-1`, `sein-d-trait-2`, `sein-d-trait-5`, and `sein-d-master-1` with the shared duration / interval / conditional-damage fields.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now includes `active_duration_seconds` and `tick_interval_seconds` columns plus `sein-c-master2-contact`, `sein-c-master1-zone`, and `sein-d-master2-zone` effect rows using `Assets/Prefab/Skill/Sein/Sein_C_Master-2.prefab`, `Assets/Prefab/Skill/Sein/Sein_C_Master_1.prefab`, and `Assets/Prefab/Skill/Sein/Sein_D_Master_2.prefab`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains `sein-a-hit-mark` and `sein-d-heat-stack`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps monster `sein` skill `sein-c` through `EffectManager` to the requested flying-arrow prefab `Assets/Prefab/Skill/Sein/Sein_B.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs` now supports projectile contact-stop, delayed impact, on-hit follow-up effects, on-expire follow-up effects, and delayed area resolution.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now resolves shared `OnHit` / `OnExpire` multi-effects for projectiles, uses scene `EffectManager` projectile visuals before effect prefabs, and creates a projectile actor even when only delayed-impact behavior is present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now supports `EventTarget` damage/status targeting and shared persistent damage-zone spawning from effect rows.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now recognizes `sein-a-hit-mark` and `sein-d-heat-stack` as shared runtime statuses.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.
- Unity console filtering after `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` showed `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` and did not show a `Pakuri` CSV failure in the retrieved entries.

### History

- 2026-05-27: User requested Code Builder / Skill Builder implementation for Sein-C and Sein-D enhancement/master behavior with explicit prefab paths for projectile, explosion, and zone visuals.
- 2026-05-27: User approved shared projectile delayed-impact expansion and shared residual-zone reuse instead of a helper active-skill row approach.

## Task: 2026-05-26 Sein-B Enhancement And Master Runtime Completion

### Task title

Implement Sein-B enhancement choices and master effects through the shared burst projectile and shared consecutive-hit extension paths.

### Goals

- Mark Sein-B trait 1-4 and master 1-2 as implemented through existing shared projectile choice modifiers.
- Add a reusable shared consecutive-hit damage extension for projectile skills.
- Implement Sein-B trait 5 on that shared consecutive-hit path with +8% same-target consecutive-hit damage up to +40%.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprint is `boards/SkillBluePrint/projectile-blueprint.md`.
- User explicitly approved widening scope to a new shared runtime/common-logic extension and new CSV columns for projectile consecutive-hit behavior.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-B trait 1-4 and master 1-2 modify burst count, damage, reload speed, shot interval, and crit chance as expected.
- User verifies in Play Mode that Sein-B trait 5 deals no bonus on the first hit to a target, then gains +8% per same-target consecutive hit up to +40%, and resets when the hit target changes.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md` defines Sein-B trait 1 burst count +2, trait 2 damage +25%, trait 3 reload speed +30%, trait 4 shot interval -25%, trait 5 same-target consecutive hit damage +8% up to +40%, master 1 burst count +4 with damage -20%, and master 2 burst count -2 with damage +90% and crit chance +20%.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `consecutive_hit_bonus_rate` and `consecutive_hit_max` columns, marks `sein-b-trait-1` through `sein-b-trait-5` and `sein-b-master-1` through `sein-b-master-2` as `RuntimeImplemented`, sets `sein-b-trait-5` to `consecutive_hit_bonus_rate=0.08` and `consecutive_hit_max=0.4`, and sets `sein-b-master-2` to `crit_chance_bonus=0.2`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now carry the new consecutive-hit choice fields through the CSV runtime definition/build path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carry those fields through the shared choice-to-snapshot path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now tracks the last projectile hit target and repeat count and resolves a shared same-target consecutive-hit damage multiplier from choice snapshot data, with fallback to `ProjectileSkillData` fields when present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs` and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now apply that shared consecutive-hit multiplier on both prefab projectile hits and direct-hit fallback projectile damage.
- CSV field-count validation returned `monster_skill_choices.csv HEADER_WIDTH=88 FIELD_COUNT_OK`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after a first parallel file-lock retry; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity editor validation logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity runtime catalog inspection returned `sein-b-trait-5|state=RuntimeImplemented|dmg=False:1|burst=0|crit=0|consec=0.08:0.4` and `sein-b-master-2|state=RuntimeImplemented|dmg=True:1.9|burst=-2|crit=0.2|consec=0:0`.

### History

- 2026-05-26: User asked Skill Builder to implement Sein-B enhancement and master effects using `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md`.
- 2026-05-26: Builder confirmed master 2 crit chance already fit the shared choice path, but trait 5 same-target consecutive-hit damage required a new shared projectile runtime extension and new choice CSV columns.
- 2026-05-26: User approved widening scope to a reusable shared consecutive-hit extension and new CSV columns, and Builder implemented the shared path plus Sein-B choice wiring.

## Task: 2026-05-26 Sein-A Enhancement And Master Runtime Completion

### Task title

Implement Sein-A enhancement choices and master effects on the shared projectile and hit-trigger SingleAttack paths.

### Goals

- Mark Sein-A trait 1-5 and master 1 as implemented through existing shared projectile choice modifier fields.
- Implement Sein-A master 2 as an OnOutgoingDamage hit trigger that runs a shared SingleAttack explosion.
- Use `Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab` for the master-2 small explosion effect.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprint is `boards/SkillBluePrint/projectile-blueprint.md`.
- User pointed out the existing hit-trigger SingleAttack common runtime, so master-2 stays on that shared trigger path instead of adding Sein-only logic.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-A trait 1-5 and master 1 modify damage, magazine, reload speed, pierce, and shot interval as expected.
- User verifies in Play Mode that Sein-A master 2 spawns `Sein_A_Master-2.prefab` and deals 50% Fire explosion damage on Sein-A hits.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md` defines Sein-A as a projectile / magazine basic attack with trait 1 damage +25%, trait 2 magazine +4, trait 3 reload speed +30%, trait 4 pierce +1 and damage +10%, trait 5 shot interval -20% and damage -10%, master 1 damage +55% and pierce +1, and master 2 50% Fire small explosion on hit.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `sein-a-trait-1` through `sein-a-trait-5`, `sein-a-master-1`, and `sein-a-master-2` as `RuntimeImplemented`.
- The same choice rows use existing shared projectile choice fields: `damage_multiplier`, `magazine_bonus`, `reload_time_multiplier`, `pierce_bonus`, and `shot_interval_multiplier`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now adds `sein-a-master2-hit-explosion` with `trigger_event=OnOutgoingDamage`, `requires_active_choice_id=sein-a-master-2`, `event_skill_id=sein-a`, `trigger_action=SingleAttack`, `damage_source=EventAppliedDamage`, `damage_source_multiplier=0.5`, `attribute=Fire`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab`.
- `Pakuri/Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab` exists and contains a `BoxCollider2D`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now dispatches source-owned `OnOutgoingDamage` triggers before the existing passive-owner trigger scan, enabling active-skill choice-gated hit triggers without Sein-only branches.
- CSV field-count validation returned `FIELD_COUNT_OK` for `monster_skill_choices.csv` 86 columns / 252 rows, `monster_skill_triger.csv` 44 columns / 27 rows, and `monster_skills.csv` 57 columns / 52 rows.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity editor code inspection returned all Sein-A trait/master rows as `RuntimeImplemented` and returned `trigger=sein-a-master2-hit-explosion|event=OnOutgoingDamage|action=SingleAttack|source=sein-a|choice=sein-a-master-2|eventSkill=sein-a|damage=EventAppliedDamage:0.5|prefab=Sein_A_Master-2`.

### History

- 2026-05-26: User asked Skill Builder to implement Sein-A enhancement and master effects using `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md`, with master-2 using `Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab`.
- 2026-05-26: Initial blueprint pass stopped on projectile impact explosion, then user pointed to the existing hit-trigger SingleAttack common logic; Builder reused that shared path and added a small source-owned `OnOutgoingDamage` dispatch extension.

## Task: 2026-05-20 Sein-B Shared Burst Projectile Implementation

### Task title

Implement Sein-B through the shared projectile burst extension.

### Goals

- Add a shared sequential burst count path instead of a Sein-only projectile branch.
- Make `sein-b` fire 5 projectiles per cycle at `shot_interval_seconds`, repeat that cycle `magazine_capacity` times, then wait on cooldown/reload.
- Wire `sein-b` to the requested `Assets/Prefab/Skill/Sein/Sein_A.prefab` visual through `EffectManager`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Unity Play Mode gameplay verification remains user-owned.
- Keep the implementation reusable for future projectile skills such as Vega.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and non-gameplay verified.

### Next Actions

- User verifies in Play Mode that Sein-B emits 5 sequential projectiles per cycle and repeats for 4 magazine cycles before the 6 second recovery.
- If Sein-B crit-chance master behavior is required, implement that as a separate choice-modifier extension because the current shared choice path still lacks crit chance modifiers.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md` defines `?꾪솚 ??5`, `?꾩갹 ??4`, `?ъ옣???쒓컙 6.0珥?, `諛쒖궗 媛꾧꺽 0.18珥?, base fire damage `14`, attack coefficient `0.65`, and projectile speed `20.0`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `projectile_burst_count`; the `sein-b` row maps to `projectile_burst_count=5`, `magazine_capacity=4`, `shot_interval_seconds=0.18`, `cooldown_seconds=6`, `reload_seconds=6`, `projectile_speed=20`, and `pierce_count=0`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now tracks queued burst shots and starts recovery only after the queued burst completes and the magazine is exhausted.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` keeps `AdditionalProjectileBonus` as simultaneous fan-spread only when `BurstProjectileCount <= 1`; burst skills use that bonus in runtime burst count instead.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `sein-b` to prefab GUID `256552cb82ec9c2499fc2e0e01d20dd2`, the existing `Assets/Prefab/Skill/Sein/Sein_A.prefab`.
- `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` followed by runtime mapping inspection returned `sein-b:burst=5;mag=4;interval=0.18;cooldown=6;reload=6;speed=20`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained. A first parallel runtime build hit only an `Assembly-CSharp.dll` file lock and passed when rerun alone.
- Unity-MCP console after refresh still contained MCP client-exit and `UnityEditor.Graphs` exceptions, but no `Pakuri` skill/CSV error was reported in the retrieved entries.

### History

- 2026-05-20: User approved an exact shared implementation for the Sein-B 5-shot burst cycle instead of the approximate existing magazine projectile behavior.

## Task: 2026-06-07 Sein Animation Clip Controller And Prefab Wiring

### Task title

Create Sein's shared Rin-contract animation assets and wire the monster prefab animator.

### Goals

- Create Sein's six animation clips: attack 1, attack 2, attack 3, idle, hit, and death.
- Create `Sein_Animation_Cont.controller` with the same parameter contract as Rin: `Attack`, `AttackIndex`, `Hit`, and `Death`.
- Add Animator and `Animation_Controller` components to `Sein_Unit.prefab` and connect `MonsterUnitActor.animationController`.

### Constraints

- Role Owner is Code Builder.
- The controller contract follows inspected `Rin_Animation_Cont.controller`.
- Unity Editor import and Play Mode animation verification were not available in this session.

### Role Owner

Code Builder

### Status

Implemented and locally YAML/build-verified.

### Next Actions

- User lets Unity import the new `.anim` and `.controller` assets.
- User verifies in Play Mode that Sein plays idle, attack 1-3, hit, and death through the shared animation parameter contract.

### Evidence

- `Pakuri/Assets/Image/Monster/Sein/Animation/Animation_Sein_Sprite` now contains 6 `Anim_Sein_*.anim` files, 6 matching `.anim.meta` files, `Sein_Animation_Cont.controller`, and `Sein_Animation_Cont.controller.meta`.
- `Select-String` confirmed `Sein_Animation_Cont.controller` contains `Attack`, `AttackIndex`, `Hit`, `Death`, and the states `Anim_Sein_Attack_1`, `Anim_Sein_Attack_2`, `Anim_Sein_Attack_3`, `Anim_Sein_Hit`, `Anim_Sein_Idle`, and `Anim_Sein_Dead_1`.
- `Pakuri/Assets/Prefab/Monster/Sein_Unit.prefab` now has `animationController: {fileID: 900300000000002}`, an `Animator` with controller GUID `ea44a003bbf345bbbccbfb750101f1ea`, and an `Animation_Controller` with `idleState: Anim_Sein_Idle`, `deadState: Anim_Sein_Dead_1`, and `attackStateCount: 3`.
- The controller meta GUID check returned `Sein controllerGuid=ea44a003bbf345bbbccbfb750101f1ea linked=True`.
- The generated idle clip check returned `Sein idleName=Anim_Sein_Idle spriteRefs=16`.
- 2026-06-07 follow-up correction verified `Sein root=4596420534878418281 rootRefs=true animatorOwner=4596420534878418281 controllerOwner=4596420534878418281 ok=true` after fixing the generated Animator and `Animation_Controller` component owner fileIDs to the root `Sein_Unit` GameObject.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing `MSB3277` warnings remained.

### History

- 2026-06-07: User asked Code Builder to create each monster's six animation clips, create controllers with Rin's parameter contract, and wire each monster prefab Animator controller.
- 2026-06-07: User reported the non-Rin monster prefabs still did not show assigned Animator / `Animation_Controller`; Code Builder found the generated component blocks were owned by the wrong GameObject fileID and corrected them to the root Unit GameObject.

## Task: Sein runtime skill prefab decommission

### Task title

Sein runtime skill prefab decommission

### Goals

- Remove all nine Sein skill prefabs after clearing base, trigger, and graph prefab references.

### Constraints

- Use existing runtime visual fields and `RuntimeEffectVisual`; no new columns; collider offset remains `(0, 0)`.

### Role Owner

- Code Builder

### Status

- Code complete; user Play Mode verification pending.

### Next Actions

- User verifies Sein A-E, enhancement, and master visuals in Play Mode.

### Evidence

- Sein C/D/E base prefab paths and A/C/D/E effect prefab nodes were removed while runtime visual values remain.
- `NewRunScene.unity` contains no Sein monster skill prefab mappings.
- `Pakuri/Assets/Prefab/Skill/Sein` assets and `Sein.meta` were deleted.

### History

- 2026-07-14: Code Builder completed Sein prefab dependency removal.

## Source: boards\MON\VEGA_MONSTER.md

## Task: 2026-05-31 Vega F-J Passive Shared Runtime And CSV Implementation

### Task title

Implement Vega passive skills F-J on shared runtime contracts, then author the passive base/effect/trigger rows in the active CSV set.

### Goals

- Keep Vega F-J on reusable shared runtime paths instead of adding Vega-only combat branches.
- Implement the missing common-runtime surfaces identified by the earlier handoff: burst-index status bonus, source-status-gated passive aura, runtime-kind-filtered passive damage modifiers/triggers, and all-allies cooldown refund.
- Author the final Vega F-J passive base/effect/trigger rows in the active CSV authority and clear stale unsupported metadata.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Runtime authority stayed on the current shared Scripts2 combat/runtime path.
- CSV authority stayed on `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv` plus the already-active `monster_skills.csv`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, build-verified, and Unity CSV-validated/synced.

### Next Actions

- User verifies in Play Mode that `vega-f` trait 3 adds the extra `name-mark` stack only on Vega-A's final burst projectile.
- User verifies in Play Mode that `vega-h` ally buffs/debuffs follow live `slaughter-permit` uptime and stop immediately when the owner loses that status.
- User verifies in Play Mode that `vega-i` applies and consumes only `Area`-kind damage interactions and that `vega-j` refunds cooldown to all allied active skills on Vega-E kills.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains the Vega F-J passive completion rows:
  - `vega-h-base-duration` as `PassiveBase` at line 254.
  - `vega-f-trait-1` through `vega-j-trait-3` as `RuntimeImplemented` rows at lines 189-203.
  - `vega-f-trait-3` now authors the burst hook through `runtime_target_skill_ids=vega-a`, `burst_status_projectile_index=0`, and `burst_status_stacks_bonus=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now includes the passive effect rows that were absent during the earlier re-audit:
  - Vega-F rows `vega-f-name-mark-damage-base` through `vega-f-name-mark-resist-trait2` at lines 114-117.
  - Vega-G rows `vega-g-silence-damage-base`, `vega-g-silence-damage-trait1`, and `vega-g-silence-mark-crit-trait3` at lines 118-120.
  - Vega-H source-status-gated aura rows `vega-h-slaughter-action-base` through `vega-h-slaughter-mark-damage-trait3` at lines 122-125.
  - Vega-I triggered area-vulnerability rows `vega-d-i-area-vulnerability-base` through `vega-d-i-area-vulnerability-trait3-trait2` at lines 126-131.
  - Vega-J survive-target rows `vega-e-j-survive-target-base` and `vega-e-j-survive-target-trait2` at lines 132-133.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now includes the passive trigger rows that were absent during the earlier re-audit:
  - `vega-g-mark-on-hit-base` at line 46.
  - `vega-i-area-vulnerability-base` through `vega-i-area-vulnerability-trait3-trait2` at lines 47-53.
  - `vega-i-area-cooldown-base` at line 49 with `event_source_scope=all_allies`, `target_skill_id=vega-d`, and `event_skill_runtime_kinds=Area`.
  - `vega-j-cooldown-base`, `vega-j-cooldown-trait1`, `vega-j-survive-target-base`, `vega-j-survive-target-trait2`, and `vega-j-vega-d-cooldown-trait3` at lines 54-58.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now exposes shared `EventSkillRuntimeKinds`, `StatusConditionalIncomingSkillRuntimeKinds`, `StatusConditionalOutgoingSkillRuntimeKinds`, `HasBurstStatusProjectileIndex`, `BurstStatusProjectileIndex`, and `BurstStatusStacksBonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.StatusPayload.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse/map/validate the Vega F-J shared fields including `required_source_status_id`, `event_skill_runtime_kinds`, the runtime-kind conditional status fields, and the burst-status choice fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now consumes `snapshot.ResolveBurstStatusStacksBonus(...)`, which is the shared runtime hook used by Vega-F trait 3.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now resolves conditional incoming/outgoing damage modifiers through `MatchesSkillRuntimeKinds(...)`, which is the shared `Area` damage filter used by Vega-I.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now checks `trigger.EventSkillRuntimeKinds`, can execute direct effect rows through `SkillMultiEffectExecutor.ExecuteDirect(...)`, and resolves cooldown/reload targets through `ResolveTargetRuntimes(...)`, including `TargetSide=AllAllies` for Vega-J.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header/type rows were normalized to 70 columns so the newly added generic effect fields are accepted by the Unity CSV loader; after a forced Unity asset refresh, `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` both completed successfully.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity console after the final validation pass logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity console after the final sync pass logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Designer first re-audited Vega F-J and produced `boards/MON/VEGA_FJ_COMMON_RUNTIME_HANDOFF.md` because the then-inspected active CSV set had no passive base/effect/trigger authoring for F-J.
- 2026-05-31: User then explicitly requested Code Builder runtime implementation and Skill Builder row authoring from that handoff, the Vega reference markdown, and `boards/SkillBluePrint/passive-stat-blueprint.md`.
- 2026-05-31: Initial Unity validation failed with `CsvFatalException: CSV file 'monster_skill_effects.csv' row 114 has 70 columns but expected 66.` because the new generic effect fields had been added to authored rows without matching header/type-row normalization.
- 2026-05-31: Code Builder normalized the effect CSV header/type rows to 70 columns, forced a Unity asset refresh, and re-ran validation/sync successfully.

## Task: 2026-05-31 Vega F-J Passive Runtime Re-audit And Code Builder Handoff

### Task title

Re-audit whether Vega passive skills F-J and their enhancement rows are actually implementable on the current CSV/common-runtime surface, then prepare a Code Builder handoff for the missing work.

### Goals

- Separate metadata-only passive rows from real gameplay-supported passive runtime behavior.
- Identify which Vega F-J pieces are CSV-authorable today and which still need shared runtime additions.
- Produce a concrete Code Builder handoff markdown for the missing common-runtime work.

### Constraints

- Role Owner is Designer.
- Conclusions must stay grounded in inspected active CSV/runtime files only.
- Designer does not implement runtime code or CSV behavior rows.

### Role Owner

Designer

### Status

Handoff markdown created. Re-audit completed.

### Next Actions

- Code Builder should start from `boards/MON/VEGA_FJ_COMMON_RUNTIME_HANDOFF.md`.
- Code Builder should first decide whether to implement exact shared contracts or ask the user to approve approximations for the currently unsupported semantics.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` contains `vega-f` through `vega-j` as passive rows with `runtime_kind=Passive`, but that file alone does not create runtime behavior.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` contains Vega F-J `PassiveEnhancement` rows, but there are no Vega F-J `PassiveBase` rows in the active choice CSV.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` contains no `vega-f` through `vega-j` rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` contains no `vega-f` through `vega-j` rows.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` shows that `PassiveDefinition` behavior is built only from `PassiveBase`, `PassiveEnhancement`, and passive effect rows.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/InGamePassiveEffectRuntime.cs` shows that learned passives only execute runtime behavior when `PassiveEffects` exist.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` shows that passive base modifiers only apply when `BaseModifierChoices` exist.
- Follow-up recheck then confirmed that some previously flagged blockers already have reusable support in current code, including `condition_status_id` stack-threshold expressions in `StatusEffectRuntime.TryParseConditionStatusExpression(...)`, `status_duration_bonus_status_id` passive-base duration overrides, and the two-stage effect-application gate formed by `condition_status_id` plus `status_conditional_target_status_id`.
- `boards/MON/VEGA_FJ_COMMON_RUNTIME_HANDOFF.md` now records the detailed Designer handoff for Code Builder.

### History

- 2026-05-31: User asked for a Designer opinion on whether Vega F-J passives and their enhancements fit the current CSV/common-runtime surface and requested a Code Builder handoff file if shared runtime was still needed.
- 2026-05-31: Designer re-audited the active CSV/runtime paths and found that the current board-level claim that Vega F-J were already implemented was broader than the inspected active data/runtime evidence.
- 2026-05-31: User then requested a second-pass search for already-existing generic contracts before keeping anything on the “new common logic required” list, and the handoff was narrowed accordingly.

## Task: 2026-05-31 Vega-D Deployment Center Spawn Fix

### Task title

Fix Vega-D so each marked-target AoE slash spawns at the resolved target center instead of snapping back to Monster Vega's own position.

### Goals

- Preserve the overlapping local AoE fanout behavior authored for Vega D.
- Keep `hit_target_count=global` from forcing the prefab hitbox origin back to the caster when the skill is also using status-filtered deployments.
- Avoid any new common runtime feature beyond the minimal executor bug fix.

### Constraints

- Role Owner is Code Builder.
- The user explicitly requested immediate implementation and instructed Builder to stop only if new common logic became necessary; inspected current executor already had the needed deployment-center path.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- User verifies in Play Mode that Vega-D now appears at each marked target position instead of at Vega's own position.
- User verifies in Play Mode that the overlapping local AoE and `즉시 / +0.5s / +1.0s` repeat timing still behave as previously authored after the center-origin fix.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors `vega-d` with both `hit_target_count=global` and `deployment_required_target_status_id=name-mark`, so the bug had to be in the shared executor's hitbox-origin decision rather than in the active row bundle.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` still maps `hit_target_count=global` to `single.HitAllTargets=true` while also mapping `deployment_required_target_status_id` to `single.UsePrefabHitbox=true` and `single.UseMultiDeployment=true`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` previously routed any `HitAllTargets` prefab hitbox through `ResolvePrefabHitboxCenter(...)` back to the caster position; it now keeps the resolved deployment center when `UsesStatusFilteredDeployments(skill)` is true.
- The same executor still resolves one center per marked target and still applies prefab scaling on that center, so this fix changes spawn origin only and does not alter the authored overlap/repeat semantics.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity console after `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` showed runtime catalog load plus sync. The console also still showed one non-blocking MCP bridge warning `Client handler error: Cannot access a disposed object.`

### History

- 2026-05-31: After the overlap/repeat re-authoring, the user observed that Vega-D was spawning on Vega's own position instead of each target center.
- 2026-05-31: Code Builder traced the bug to the shared `ResolvePrefabHitboxCenter(...)` branch that still treated all `HitAllTargets` skills like caster-anchored slashes and narrowed that branch to exclude status-filtered deployments.

## Task: 2026-05-31 Vega-D Overlapping Area Fanout Re-authoring

### Task title

Re-author Vega D back to overlapping local area hits per marked target and update master-1 to add two delayed extra slashes on the existing shared repeat path.

### Goals

- Keep Vega D on the shared `SingleAttack` status-filtered fanout path without adding a new runtime branch.
- Let each marked-target deployment center hit all enemies in its local radius so overlaps can stack.
- Author Vega-D master-1 as base hit plus two extra delayed repeats at `0.5s` and `1.0s`.

### Constraints

- Role Owner is Code Builder.
- The user explicitly required work to stop if a new common runtime was needed; inspected current runtime already supported local multi-hit count, prefab radius scaling, and delayed per-target repeats.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- User verifies in Play Mode that each marked-target slash now damages every enemy inside the local radius and that overlapped circles stack damage.
- User verifies in Play Mode that Vega-D master-1 now lands at `즉시 / +0.5s / +1.0s` per marked-target center and that each hit uses the authored `-35%` power adjustment.
- User verifies in Play Mode that Vega-D master-2 still enlarges the live slash prefab together with the effective hit radius.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-d` with `hit_target_count=global` while keeping `runtime_kind=SingleAttack`, `radius=1.25`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_D.prefab`, `deployment_required_target_status_id=name-mark`, and `deployment_required_target_status_min_stacks=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `vega-d-master-1` with description `각 표식 대상 위치에 범위 참격 2회 추가 발생, 각 참격 위력 -35%`, `damage_multiplier=0.65`, `repeat_count_per_target=2`, `repeat_interval_seconds=0.5`, and `repeat_damage_multiplier=1`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` already keeps status-filtered fanout centers through `ResolveDeploymentCenters(...)`, resolves unlimited local hits when `HitAllTargets` is authored through `ResolveEffectiveHitTargetCount(...)`, and schedules per-target repeats with `delaySeconds = snapshot.RepeatIntervalSeconds * repeatIndex`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` still uses `SkillExecutionUtility.ApplyPrefabScale(...)` for the current Vega-D status-filtered fanout path instead of the stretched line visual branch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillAreaUtility.cs` still maps radius modifiers into both effective radius and prefab scale through `ResolveRadius(...)` and `ResolvePrefabScaleFactor(...)`, so Vega-D master-2 radius growth remains data-driven.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity console after `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` showed `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: An earlier same-day Vega-D pass temporarily re-authored the row toward single-target local hits to remove unintended overlap behavior.
- 2026-05-31: User then explicitly requested overlapping local area damage plus base hit and two delayed extra slashes, so Code Builder re-authored the active Vega-D rows on the already-inspected shared runtime path without adding new common logic.

## Task: 2026-05-31 Vega-D Marked-Target Fanout Single-Target Fix

### Task title

Keep Vega D on marked-target fanout while restoring per-target single-hit behavior and removing the unintended beam-like prefab stretch.

### Goals

- Preserve the shared `SingleAttack` resolved-deployment path that fires once per enemy carrying `name-mark`.
- Stop status-filtered fanout casts from inheriting the line-style multi-deployment visual scaling.
- Restore authored single-target hit count per deployment instead of unlimited local hits.

### Constraints

- Role Owner is Code Builder.
- The user explicitly required that work stop only if firing separately at every marked enemy needed new shared common logic; inspected runtime already had that shared deployment path, so this task stayed inside the existing executor.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- User verifies in Play Mode that Vega D now spawns one slash per `name-mark` target without the stretched beam presentation.
- User verifies in Play Mode that each slash damages only the intended marked target instead of also clipping nearby enemies around the deployment center.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors `vega-d` as `runtime_kind=SingleAttack` with `deployment_required_target_status_id=name-mark`, `deployment_required_target_status_min_stacks=1`, `radius=1.25`, and empty `hit_target_count`, so the active row still requests one deployment per marked enemy rather than a separate runtime kind.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` still maps any `DeploymentRequiredTargetStatusId` row to `UsePrefabHitbox=true` and `UseMultiDeployment=true`, which is why the fix had to stay inside the shared `SingleAttackSkillExecutor` behavior split rather than in CSV alone.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now distinguishes status-filtered deployments from line-style multi-deployment visuals through `UsesStatusFilteredDeployments(...)`, `UsesLineStyleMultiDeploymentVisual(...)`, and `ResolveEffectiveHitTargetCount(...)`.
- In that executor, status-filtered fanout casts now keep the shared resolved-deployment center logic but no longer call `ConfigureMultiDeploymentPrefabVisual(...)`; they instead follow the normal prefab scaling path and use the authored hit-target count floor of `1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity console after `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` showed `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: User reported that Vega D should stay `SingleAttack` and fire at every marked enemy, but the current effect looked like a beam and the per-cast damage was not behaving as single-target.
- 2026-05-31: Code Builder verified that the beam-like look came from `ConfigureMultiDeploymentPrefabVisual(...)` and that unlimited local hits came from `effectiveHitTargetCount = int.MaxValue` on the generic `UseMultiDeployment` path, then split the status-filtered fanout behavior from the line-style multi-deployment branch.

## Task: 2026-05-31 Vega-E Shared Runtime Implementation And CSV Authoring

### Task title

Implement the shared runtime extensions and active CSV rows required to bring Vega E onto the current common `SingleAttack` path.

### Goals

- Keep Vega E on shared runtime rather than adding a Vega-only executor branch.
- Support marked-target selection, mark-stack-based extra damage, and partial mark consumption through reusable runtime/data contracts.
- Author the active Vega E CSV rows on those shared fields and keep unsupported data explicit where reference authority is still missing.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Implementation authority started from `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md`, then the final row-authoring pass used `boards/SkillBluePrint/single-attack-blueprint.md`, `Pakuri/reference/2.Monster/vega/skill/e-final-sentence.md`, and the routed active CSV files.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, build-verified, and Unity CSV-validated. Vega-E trait-5 is now fully authored on the shared redistribution path with user-provided search radius `100` and target count `1`.

### Next Actions

- User verifies in Play Mode that Vega E now targets the enemy with the highest `name-mark` stack count and refuses to cast only when no marked target exists.
- User verifies in Play Mode that base damage, per-stack bonus damage, and consumed-mark amount match the authored Vega E values across trait/master combinations.
- User verifies in Play Mode that trait-5 kill redistribution sends `25%` of consumed `name-mark` to one nearby enemy using search radius `100`.

### Evidence

- `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md` was used as the implementation contract for the shared Vega E runtime work.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs` and `.../SkillExecutionUtility.cs` now support `HighestStacks` targeting keyed by `target_selection_status_id` plus a minimum required stack count.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs`, `SkillDefinition.cs`, `InGameSkillDefinitionMapper.cs`, `SkillExecutionSnapshot.cs`, and the `PakuriCsvRuntimeData.*` CSV runtime files now carry shared target-status-stack damage, target-status consumption, conditional crit, and consumed-status redistribution fields.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now expose shared partial status-stack consumption helpers.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now resolves shared target-status-stack bonus damage, consumes target stacks on hit, and can redistribute a portion of consumed stacks on kill.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-e` with `target_selection=HighestStacks`, `target_selection_status_id=name-mark`, `target_selection_status_min_stacks=1`, `target_status_stack_status_id=name-mark`, `target_status_stack_base_damage=6`, `target_status_stack_attack_power_coefficient=0.18`, `consume_target_status_id=name-mark`, `consume_target_status_ratio=0.5`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `vega-e-trait-1`, `trait-2`, `trait-3`, `trait-4`, `trait-5`, `master-1`, and `master-2` as shared-runtime-backed rows; `vega-e-trait-5` now authors `redistribute_consumed_status_ratio_on_kill=0.25`, `redistribute_consumed_status_id=name-mark`, `redistribute_consumed_status_search_radius=100`, and `redistribute_consumed_status_target_count=1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- The current redistribution split behavior inside `SingleAttackSkillExecutor.cs` only matters when the authored target count exceeds `1`; current Vega-E trait-5 authors target count `1`, so no multi-target split inference is exercised for this skill row.

### History

- 2026-05-30: Designer produced `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md` after confirming Vega E still needed shared targeting/consumption/scaling extensions.
- 2026-05-31: Code Builder implemented the shared runtime fields and re-authored the active Vega E rows on that path.
- 2026-05-31: First Unity CSV validation exposed a temporary Vega E row-shape regression in `monster_skill_choices.csv`; Builder corrected the row alignment and revalidated successfully.
- 2026-05-31: User later provided trait-5 nearby-search authority (`radius 100`, `target count 1`) plus the final prefab path `Assets/Prefab/Skill/Vega/Vega_E.prefab`, and Skill Builder completed the remaining row authoring.

## Task: 2026-05-30 Vega-E Common Runtime Code Builder Handoff

### Task title

Prepare a Designer handoff for the remaining shared-runtime work needed to fully support Vega E on current CSV/runtime authority.

### Goals

- Separate Vega E behaviors that already fit current shared runtime from behaviors that still need shared extension.
- Hand off the minimum shared runtime surface needed for Vega E without proposing Vega-only hardcoded branches.
- Give Code Builder a concrete implementation order and acceptance target.

### Constraints

- Role Owner is Designer.
- Designer does not implement code or scene changes.
- Conclusions must stay grounded in inspected current code and active CSV rows only.

### Role Owner

Designer

### Status

Handoff markdown created. Implementation not started.

### Next Actions

- If user requests implementation, Code Builder should use `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md` as the starting contract.
- Code Builder should first re-audit which Vega E rows can move to shared runtime by CSV re-authoring alone before extending code.

### Evidence

- `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md` now records the Code Builder handoff for Vega E.
- `monster_skills.csv` currently authors `vega-e` as shared `SingleAttack`, but still with `target_selection=Nearest`.
- `monster_skill_choices.csv` currently marks all `vega-e-*` choice rows `DataOnlyUnsupported`.
- Inspected shared runtime already supports generic damage multiplier, cooldown multiplier, and kill cooldown refund paths, so not every Vega E row necessarily needs new code.
- Inspected shared runtime did not show a current generic contract for highest-mark target selection, mark-stack-based damage scaling, consumed-mark tracking, or consumed-mark redistribution.

### History

- 2026-05-30: User asked whether Vega E could be implemented on current common logic and CSV.
- 2026-05-30: Designer concluded Vega E base cast already routes through shared `SingleAttack`, but full intended behavior still needs shared targeting/consumption/scaling extensions.
- 2026-05-30: User then requested a Code Builder handoff markdown for Vega E.

## Task: 2026-05-30 Vega-C And Vega-D Shared Runtime Implementation And Skill Authoring

### Task title

Implement the shared runtime extensions and active CSV rows required to bring Vega C and Vega D onto reusable common paths.

### Goals

- Keep Vega C on shared `Buff` while adding reusable buff-active modifier support and attached-buff scalar choice overrides.
- Move Vega D from the mismatched `AreaAttack` row shape to shared `SingleAttack` marked-target fanout.
- Finish the routed active CSV authoring for Vega C and Vega D, including the user-provided prefab paths.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Shared runtime extensions were allowed because the user explicitly asked for Code Builder implementation from the handoff.
- User explicitly clarified that Vega D is `SingleAttack`-style repeated slashes at target positions, not a zone-style area attack.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, build-verified, and Unity CSV-validated.

### Next Actions

- User verifies in Play Mode that Vega C buff-active bonuses affect the intended follow-up skills only while `slaughter-permit` is active.
- User verifies in Play Mode that Vega D casts one slash per marked enemy position, allows overlap, and repeats one extra slash per marked target when master-1 is learned.

### Evidence

- `boards/MON/VEGA_CD_COMMON_RUNTIME_HANDOFF.md` was used as the explicit implementation contract for the shared Vega C/D extension work.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now exposes shared choice/runtime fields for `RuntimeTargetSkillIds`, attached buff action-speed / attack-power overrides, `RequiredSourceStatusId`, repeat-per-target fields, and `DeploymentRequiredTargetStatusId`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now applies choice rows only when the source status requirement is met and accepts delimited `RuntimeTargetSkillIds`, so Vega C buff-active modifiers stay on shared runtime routing instead of Vega-only branches.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillStatusSpecUtility.cs` now clones attached status data with snapshot-provided action-speed and attack-power overrides, which lets Vega C trait-2, trait-3, and master-2 modify the attached `slaughter-permit` buff through shared logic.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now treats `DeploymentRequiredTargetStatusId` as a shared resolved-deployment path and schedules shared repeat deployments per center, so Vega D stays on `SingleAttack` while fanning out across marked targets.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillExecutionUtility.cs` and `.../SkillTargetingUtility.cs` now expose shared ordered-target resolution filtered by required target status and minimum stacks.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-c` with `Assets/Prefab/Skill/Vega/Vega_C.prefab`, and authors `vega-d` as `runtime_kind=SingleAttack`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_D.prefab`, `deployment_required_target_status_id=name-mark`, and `deployment_required_target_status_min_stacks=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now re-authors Vega C rows through shared `status_action_speed_bonus`, `status_attack_power_bonus`, `runtime_target_skill_ids`, and `required_source_status_id` fields, and re-authors Vega D trait-4 / trait-5 / master-1 through shared conditional-damage, status-set, and repeat-per-target fields.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity console also logged one MCP bridge warning `Client handler error: Cannot access a disposed object.`, but no new Vega CSV parse failure or C# compile failure appeared in the inspected console output.

### History

- 2026-05-30: User asked Designer whether Vega C and Vega D could be implemented from the current repository state and then requested an English markdown handoff for Code Builder.
- 2026-05-30: User clarified that Vega D should be treated as `SingleAttack` semantics rather than area-zone semantics; Designer reflected that correction in the handoff.
- 2026-05-30: User then explicitly requested Code Builder implementation from `boards/MON/VEGA_CD_COMMON_RUNTIME_HANDOFF.md` followed by Skill Builder authoring for Vega C and Vega D with the provided prefab paths.

## Task: 2026-05-28 Vega-B Master-1 Follow-up Returned To LineAttack

### Task title

Convert the Vega-B master-1 delayed second slash from the shared triggered `SingleAttack` path to the shared triggered `LineAttack` path so it matches the aimed slash behavior of the Vega-B base skill.

### Goals

- Make the delayed second slash rotate and travel on the same shared line-attack presentation path as base `vega-b`.
- Keep the authored `0.4s` delay, `45%` scaled damage, prefab path, and linked `1s` silence effect.
- Preserve CSV validation and runtime-catalog sync after the trigger-path change.

### Constraints

- Role Owner is Code Builder.
- This change stays on the existing `vega-b-master1-second-slash` trigger row plus the shared trigger runtime; no hidden helper skill row was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- User verifies in Play Mode that Vega-B master-1 second slash now aims like base `vega-b` instead of appearing as the older self-centered `SingleAttack` follow-up.
- If design later requires the delayed slash to lock to the exact original cast target/path instead of re-resolving nearest target at `0.4s`, that would need a separate trigger-context extension.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `runtime_kind=LineAttack`, `trigger_action=LineAttack`, `target_selection=Nearest`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`, and linked effect `vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors base `vega-b` as `runtime_kind=LineAttack`, so the base and follow-up now share the same runtime kind and prefab path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now includes an explicit `SkillTriggerActionKind.LineAttack` branch and `ExecuteLineAttack(...)` shared trigger path for direct delayed line slashes.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` now resolves linked OnHit status effects through the passed `SkillExecutionSnapshot`, so the triggered line path keeps source-skill choice-gated status rules instead of losing them on the beam actor path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-28: After base `vega-b` was returned to `LineAttack`, the user reported that the master-1 delayed second slash still looked like the older `SingleAttack` follow-up and requested the same aimed slash path for the follow-up hit.

## Task: 2026-05-28 Vega-B Base Skill Returned To LineAttack

### Task title

Return Vega-B base skill to the shared `LineAttack` path so the slash aims toward the target instead of spawning as a self-centered `SingleAttack`.

### Goals

- Fix the current “cast on self” visual feel reported by the user.
- Keep Vega-B using the shared beam/line actor rotation path like other straight aimed slashes.
- Preserve base damage, silence payload, cooldown, width, and prefab path.

### Constraints

- Role Owner is Code Builder.
- This change is limited to the active Vega-B base skill row and runtime-catalog sync.
- The existing master-1 delayed second slash trigger row remains on the shared triggered `SingleAttack` path for now.

### Role Owner

Code Builder

### Status

Implemented and Unity CSV-validated.

### Next Actions

- User verifies in Play Mode that base Vega-B now rotates toward the current target like a straight aimed line attack.
- If master-1 must also rotate on the same path, that follow-up still needs a separate shared trigger-beam design decision.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b runtime_kind=LineAttack`, keeps `radius=1.8`, `cooldown_seconds=8`, `status_effect_id=silence`, and keeps `Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/BeamSkillExecutor.cs` resolves target direction from nearest target and spawns the prefab with `ResolveRotation(direction)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` rotates the live line actor from `lineDirection`, which is why the LineAttack path matches the user-requested aimed slash behavior.
- Unity menu `Pakuri/Validate CSV Source Data` completed and the console logged the runtime catalog load summary without new Vega-B CSV errors.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` completed and the console logged sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-28: After the earlier SingleAttack contact implementation, the user reported that Vega-B still looked like a self-cast slash even though damage landed; targeted inspection confirmed the visual issue was caused by the SingleAttack prefab spawn path using identity rotation.

## Task: 2026-05-28 Vega-B Follow-up Trigger Payload Correction

### Task title

Fix the authored Vega-B master-1 follow-up trigger row so CSV validation passes and the second slash deals the intended scaled damage.

### Goals

- Remove the current Vega-B source CSV validation failure.
- Keep the second slash at the intended `45%` scaling while giving the trigger row a real damage payload.
- Preserve the existing shared triggered `SingleAttack` plus linked OnHit silence path.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The correction stays inside the existing Vega-B row bundle and shared validator; no hidden helper skill row was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and editor-validated.

### Next Actions

- User verifies in Play Mode that the second slash now deals the scaled damage as expected, not just the linked `1s` silence.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `base_damage=30`, `attack_power_coefficient=1.4`, and `damage_multiplier=0.45`.
- Unity menu `Pakuri/Validate CSV Source Data` completed after the correction, and the console logged the runtime catalog load summary instead of the previous Vega-B trigger validation failure.

### History

- 2026-05-28: The first authored row kept only `damage_multiplier=0.45` and zeroed the real payload fields, which was both validator-invalid and runtime-zero-damage.

## Task: 2026-05-28 Vega-B Triggered Second Slash And Silence Authoring

### Task title

Author Vega-B on the shared SingleAttack path and extend triggered SingleAttack so the delayed second slash can carry OnHit silence.

### Goals

- Keep Vega-B on `SingleAttack` with the user-provided `Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- Implement base silence, trait-2 silence duration bonus, trait-5 Name Mark application, master-1 delayed second slash, and master-2 10-stack silence extension.
- Avoid a Vega-only helper runtime or hidden extra active-skill slot.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Authority stayed on `boards/SkillBluePrint/single-attack-blueprint.md`, `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md`, routed active CSV files, and the user-provided prefab path.
- The shared runtime/common-logic extension was user-approved before implementation.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Vega-B now emits the slash from the caster position, damages each enemy on the path once, and applies base `3s` silence.
- User verifies that trait-2 extends Vega-B silence by `+1s`, trait-5 adds `name-mark` `+2` on hit, master-1 fires the delayed `0.4s` second slash with `45%` damage and `1s` silence, and master-2 refreshes silence by `+1s` at `name-mark>=10`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b` as `SingleAttack` with `hit_target_count=global`, `status_effect_id=silence`, `status_duration_seconds=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `vega-b-trait-2` and `vega-b-master-2` `RuntimeImplemented` through shared silence-duration and threshold-status fields.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `vega-b-trait5-name-mark` and `vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `vega-b-master1-second-slash`, which routes a delayed `SingleAttack` slash at `0.4s`, `damage_multiplier=0.45`, and links `vega-b-master1-second-silence`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now lets triggered `SingleAttack` hits carry shared `OnHit` status effects with the source-skill active-choice snapshot, so Vega-B master-1 reuses shared status gating and silence-duration bonuses.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now anchors `HitAllTargets` prefab hitboxes at the caster position, which matches the Vega-B slash-path prefab behavior instead of centering the hitbox on the target group.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the pre-existing `MSB3277` warnings remained.

### History

- 2026-05-28: Initial inspection confirmed Vega-B was already authored as `SingleAttack`, so the work stayed on the shared SingleAttack blueprint path instead of the beam blueprint.
- 2026-05-28: The user considered a hidden follow-up skill row for the second slash, but current active-slot validation and learned-runtime loading made that path larger than a small shared triggered-SingleAttack extension.

## Task: 2026-05-28 Vega-A Shared Projectile Runtime Extension And Skill Authoring

### Task title

Extend the shared projectile runtime for Vega-A burst timing, per-burst damage rules, and follow-up shadow shots, then author the active Vega-A data on that path.

### Goals

- Keep Vega-A on the projectile blueprint path instead of adding a Vega-only runtime.
- Author the inspected reference values for 3-hit burst timing, third-hit bonus, Name Mark application, trait-4 last-hit bonus, trait-5 conditional damage, and master-1 shadow follow-up.
- Keep master-2 grounded on the user-provided slash coefficient and prefab path without adding a Vega-only runtime.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Authority stayed on `boards/SkillBluePrint/projectile-blueprint.md`, `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md`, the routed active CSV files, and the user-provided prefab path `Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- The base reference did not provide a numeric slash-damage value for master-2, but the user later provided `attack coefficient 0.5` and `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` as explicit authority.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, compile-verified, and Unity editor-validated.

### Next Actions

- User verifies in Play Mode that Vega-A fires 3-hit bursts with `0.12s` internal spacing and `0.55s` outer cadence.
- User verifies that trait-4 boosts only the last burst hit, trait-5 boosts only targets with at least 10 `name-mark` stacks, and master-1 spawns one next-frame shadow projectile at `45%` damage.
- User verifies in Play Mode that master-2 kill triggers now deal the small slash through the shared triggered-effect path and use `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.

### Evidence

- `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md` specifies 3 bullets, `3번째 탄환 200%`, shot interval `0.55`, bullet interval `0.12`, hit-applied `name-mark` 1 stack, trait-4 last-hit `+50%`, trait-5 `+25%` vs `name-mark` 10+, and master-1 shadow projectile `45%`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-a` with `shot_interval_seconds=0.55`, `burst_interval_seconds=0.12`, `projectile_burst_count=3`, `burst_damage_projectile_index=3`, `burst_damage_multiplier=2`, `status_effect_id=name-mark`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `vega-a-trait-4` through the shared last-burst-hit multiplier path, `vega-a-trait-5` through the shared conditional target-status multiplier path, and `vega-a-master-1` through the shared follow-up projectile path.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now authors `vega-a-master2-transfer-mark` as a shared `Damage` effect with `attack_power_coefficient=0.5`, `status_stack_amount=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` and `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now mark Vega-A master-2 `RuntimeImplemented` on the existing nearest-enemy OnKill trigger/effect path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs`, `.../Execution/Runtime/SkillExecutionSnapshot.cs`, and `.../Execution/Executors/ProjectileSkillExecutor.cs` now carry separate burst interval, burst-index damage rules, and follow-up projectile execution on the shared projectile runtime path.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now accepts shared `Damage` effect rows with positive `attack_power_coefficient` or `spell_power_coefficient` even when `base_damage=0`, matching the actual runtime formula used by `SkillExecutionUtility.ResolveDamage(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity refresh completed to `idle`, and the filtered Unity console returned no CSV/runtime errors after the trigger-row contract fix.

### History

- 2026-05-28: User first challenged whether burst-internal spacing already existed from Sein-B; re-inspection confirmed the existing shared burst path and narrowed the required extensions to shared burst-index damage rules and shared follow-up projectile support.
- 2026-05-28: The new Vega master-2 trigger row initially failed CSV parsing because `monster_skill_triger.csv` requires a non-empty `triggered_skill_id`; the row was corrected and Unity validation then completed without further errors.
- 2026-05-28: User later provided the missing master-2 slash authority as `attack coefficient 0.5` plus `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`, which completed the existing trigger/effect implementation path without further shared code changes.
- 2026-05-28: Unity source validation then exposed a shared mismatch: coeff-only `Damage` effect rows were runtime-valid but validator-invalid. Builder fixed the shared validator so Vega-A master-2 and future coeff-only damage effects no longer require fake positive `base_damage`.

## Task: 2026-05-18 Vega-B SingleAttack Runtime Kind

### Task title

Route Vega-B through the new SingleAttack runtime kind for one-shot area damage.

### Goals

- Move Vega-B out of `LineAttack` because the requested CSV row belongs to one-shot `SingleAttack`.
- Preserve existing CSV-authored damage, coefficient, radius, and cooldown.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Vega-B now behaves as a one-shot area hit in the current shared executor path.

### Evidence

- `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md` names Vega-B `移⑤У????쒕룄`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `vega-b runtime_kind=SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` and `SkillExecutors.cs` provide the data and executor path.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User listed CSV row 34 as a one-shot area attack skill for the new `SingleAttack` type.

## Task: 2026-06-07 Vega Animation Clip Controller And Prefab Wiring

### Task title

Create Vega's shared Rin-contract animation assets and wire the monster prefab animator.

### Goals

- Create Vega's six animation clips: attack 1, attack 2, attack 3, idle, hit, and death.
- Create `Vega_Animation_Cont.controller` with the same parameter contract as Rin: `Attack`, `AttackIndex`, `Hit`, and `Death`.
- Add Animator and `Animation_Controller` components to `Vega_Unit.prefab` and connect `MonsterUnitActor.animationController`.

### Constraints

- Role Owner is Code Builder.
- The controller contract follows inspected `Rin_Animation_Cont.controller`.
- Unity Editor import and Play Mode animation verification were not available in this session.

### Role Owner

Code Builder

### Status

Implemented and locally YAML/build-verified.

### Next Actions

- User lets Unity import the new `.anim` and `.controller` assets.
- User verifies in Play Mode that Vega plays idle, attack 1-3, hit, and death through the shared animation parameter contract.

### Evidence

- `Pakuri/Assets/Image/Monster/Vega/Animation/Animation_Vega_Sprite` now contains 6 `Anim_Vega_*.anim` files, 6 matching `.anim.meta` files, `Vega_Animation_Cont.controller`, and `Vega_Animation_Cont.controller.meta`.
- `Select-String` confirmed `Vega_Animation_Cont.controller` contains `Attack`, `AttackIndex`, `Hit`, `Death`, and the states `Anim_Vega_Attack_1`, `Anim_Vega_Attack_2`, `Anim_Vega_Attack_3`, `Anim_Vega_Hit`, `Anim_Vega_Idle`, and `Anim_Vega_Dead_1`.
- `Pakuri/Assets/Prefab/Monster/Vega_Unit.prefab` now has `animationController: {fileID: 900400000000002}`, an `Animator` with controller GUID `c923064a4af54d6f9e26058c1197e17d`, and an `Animation_Controller` with `idleState: Anim_Vega_Idle`, `deadState: Anim_Vega_Dead_1`, and `attackStateCount: 3`.
- The controller meta GUID check returned `Vega controllerGuid=c923064a4af54d6f9e26058c1197e17d linked=True`.
- The generated idle clip check returned `Vega idleName=Anim_Vega_Idle spriteRefs=16`.
- 2026-06-07 follow-up correction verified `Vega root=4596420534878418281 rootRefs=true animatorOwner=4596420534878418281 controllerOwner=4596420534878418281 ok=true` after fixing the generated Animator and `Animation_Controller` component owner fileIDs to the root `Vega_Unit` GameObject.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing `MSB3277` warnings remained.

### History

- 2026-06-07: User asked Code Builder to create each monster's six animation clips, create controllers with Rin's parameter contract, and wire each monster prefab Animator controller.
- 2026-06-07: User reported the non-Rin monster prefabs still did not show assigned Animator / `Animation_Controller`; Code Builder found the generated component blocks were owned by the wrong GameObject fileID and corrected them to the root Unit GameObject.

## Task: Vega runtime skill prefab decommission

### Task title

Vega runtime skill prefab decommission

### Goals

- Remove all six Vega skill prefabs while preserving runtime visuals, collider LineAttack, and follow-up projectiles.

### Constraints

- No new CSV columns; object and collider offsets remain `(0, 0)`.

### Role Owner

- Code Builder

### Status

- Code complete; user Play Mode verification pending.

### Next Actions

- User verifies Vega A-E, especially A Master 1 follow-up and B/Master line hit detection, in Play Mode.

### Evidence

- Runtime follow-up projectile creation was added to `ProjectileSkillExecutor`.
- Active runtime CSV search has zero `Assets/Prefab/Skill/Vega` references.
- `Pakuri/Assets/Prefab/Skill/Vega` assets and `Vega.meta` were deleted.

### History

- 2026-07-14: Code Builder completed Vega prefab dependency removal.

## Source: boards\OPS\AUTOMATION_GUIDE.md

## Task: 2026-05-30 UTF-8 Documentation Read Default

### Task title

Make UTF-8 raw text reads the default documentation inspection path for repository policy work.

### Goals

- Keep markdown and other text-document evidence readable without mojibake during Codex inspection.
- Record the repository-level expectation in `AGENTS.md`.
- Align future document reads on `Get-Content -Raw -Encoding UTF8` where PowerShell is used.

### Constraints

- Role Owner is Designer.
- This task changes workflow policy, not runtime gameplay code.
- Command approval behavior is controlled by the CLI/runtime, not by markdown policy alone.

### Role Owner

Designer

### Status

Implemented.

### Next Actions

- Future markdown and text-document inspection should prefer `Get-Content -Raw -Encoding UTF8`.
- If command approval prompts still appear, reuse the saved approved prefix instead of widening to unrelated command families.

### Evidence

- `AGENTS.md` startup rules now state that markdown and other text documentation files should be read with `Get-Content -Raw -Encoding UTF8` by default.
- A broad approved command prefix for `Get-Content -Raw -Encoding UTF8` was saved in the current CLI session.

### History

- 2026-05-30: User asked to default future document reads to UTF-8 and to record that policy in `AGENTS.md`.

## Task: 2026-05-30 File Read/Write Command Allowance For This Project

### Task title

Record that file read/write shell commands are expected workflow commands in this project.

### Goals

- Make file read/write inspection commands explicit as normal repository workflow.
- Avoid repeated policy churn around enumerating individual command examples.
- Keep UTF-8-safe text reads as the preferred documentation-read pattern.

### Constraints

- Role Owner is Designer.
- This task records project-local workflow policy only.
- CLI approval storage is still controlled by the runtime; markdown policy does not override runtime security.

### Role Owner

Designer

### Status

Implemented.

### Next Actions

- Prefer UTF-8-safe text reads for documentation where practical.
- Reuse saved CLI approvals where possible, while treating file read/write commands as the normal intended workflow in project policy.

### Evidence

- `AGENTS.md` now states that file read/write shell commands inside the intended workspace are normal and expected workflow commands for this project.
- `AGENTS.md` no longer enumerates a short fixed list of command examples and instead records the broader project-level allowance.

### History

- 2026-05-30: User asked to replace the explicit command list with a broader statement that file read/write commands are allowed for this project before continuing implementation work.

## Task: 2026-05-28 Skill Builder Trigger Payload Documentation Guard

### Task title

Document the trigger-row payload guard so Skill Builder handoffs do not produce source-validity errors for trigger-routed `SingleAttack` follow-ups.

### Goals

- Make the Skill Builder documentation explicitly state that trigger `SingleAttack` rows own their own concrete damage payload.
- Prevent future handoffs from treating `damage_multiplier` as an implicit source-skill payload reuse mechanism.
- Keep the guard close to the active Skill Builder blueprint and exception docs.

### Constraints

- Role Owner is Code Builder / Designer.
- This task changes workflow documentation, not runtime gameplay code by itself.
- Claims must stay grounded in the inspected validator, trigger CSV row contract, and edited markdown files.

### Role Owner

Code Builder / Designer

### Status

Implemented and locally verified by targeted markdown inspection plus Unity CSV validation.

### Next Actions

- Future Skill Builder row-bundle handoffs should always restate trigger damage payload fields when `monster_skill_triger.csv` participates in the implementation.

### Evidence

- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now tells Builder that routed `monster_skill_triger.csv` `SingleAttack` follow-up rows must carry positive fixed payload through `base_damage` or `attack/spell` coefficient rather than `damage_multiplier` alone.
- The then-active SingleAttack blueprint included `TriggerSingleAttackRows` in optional parsed input and required a `Fixed` trigger row to pass source CSV validation with its own positive damage payload; that runtime-kind blueprint was later removed.
- The then-active exception guidance recorded that trigger rows owned their own damage payload and linked effect rows did not satisfy trigger-row damage validation; this guidance was later superseded by the single node blueprint.
- The then-active handoff format required concrete trigger damage payload fields; that auxiliary format was later removed from active routing.

### History

- 2026-05-28: Vega-B master-1 follow-up validation failure showed that the existing Skill Builder docs did not explicitly guard against zero-payload trigger rows that only carried a multiplier.

## Task: 2026-05-24 Projectile Blueprint Nth Launch Branch Extension

### Task title

Promote nth-launch branch chance override into the projectile blueprint common path.

### Goals

- Keep Skill Builder from stopping on approved reusable projectile branch behavior that triggers every nth base projectile launch.
- Document `BranchLaunchPeriod` and `BranchLaunchChanceSet` as optional parsed projectile fields.
- Preserve the stop-and-ask rule for sequence-state behavior not covered by the shared nth-launch branch override.

### Constraints

- Role Owner is Code Builder / Skill Builder because this policy update follows an implemented runtime extension.
- No skill CSV row values, prefab assets, or scene objects were changed by this task.
- Skill Builder still requires parsed input or explicit current CSV/code discovery authorization before editing specific skill rows.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and locally verified by build, Unity refresh, and targeted source inspection.

### Next Actions

- Future projectile Skill Builder requests may use `BranchLaunchPeriod` plus `BranchLaunchChanceSet` for reusable nth-launch branch behavior.
- If a future request needs another nth-launch effect family, add a deliberate shared extension instead of hardcoding a monster-specific branch.

### Evidence

- The then-active projectile blueprint listed `BranchLaunchPeriod` and `BranchLaunchChanceSet` as optional common parsed fields.
- That blueprint included nth-launch branch chance override in its common projectile contract and kept other sequence-state effects outside the shared pattern.
- The runtime-kind blueprint was later removed when Skill Builder moved to the single Enhancement/Master node blueprint.
- Runtime support was added through `SkillRuntimeInstance.ProjectileLaunchCount`, `ProjectileSkillExecutor` per-launch branch resolution, and snapshot/choice fields for branch launch period/chance.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remained.

### History

- 2026-05-24: User approved using a reusable launch-count function so `Rin-a` master-2 can make every 3rd launched projectile branch at 100%, and future skills can reuse nth-launch projectile behavior.

## Task: 2026-05-24 Skill Blueprint Execution Path Refresh

### Task title

Refresh Skill Builder blueprint references after the skill execution folder refactor.

### Goals

- Remove stale `SkillExecutors.cs` references from the multi-effect and shield/buff unification blueprints.
- Point future Skill Builder work at the current `Execution/Executors`, `Execution/Runtime`, and `Execution/Actors` files.
- Keep blueprint claims grounded in the currently inspected refactored code structure.

### Constraints

- Role Owner is Designer because this changes workflow/blueprint documentation, not runtime gameplay code.
- No C# script, scene, prefab, or gameplay CSV values were changed.
- Claims must stay grounded in inspected blueprint text and current runtime source paths.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown/code-path inspection.

### Next Actions

- Future Skill Builder work that uses these blueprints should follow the current role-folder paths instead of looking for `SkillExecutors.cs`.
- If more blueprints still mention deleted execution files, update them only after inspecting the current matching runtime type path.

### Evidence

- The then-active multi-effect and shield/buff blueprint documents were refreshed to the refactored executor paths; both one-off documents were later removed from active Skill Builder routing.
- Targeted search in the two edited blueprint files found no remaining deleted `Execution/SkillExecutors.cs` path references; current `SupportSkillExecutors.cs` references are intentional.
- `Test-Path Pakuri\Assets\Scripts2\InGame\Skills\Execution\SkillExecutors.cs` returned `False`.

### History

- 2026-05-24: User asked to update the stale `SkillExecutors.cs` references in the multi-effect and shield/buff status unification blueprints after the execution folder role split.

## Task: 2026-05-24 Skill Builder Blueprint And Explicit-Path Only Default

### Task title

Make Skill Builder default to blueprint plus explicit parsed input/path authority, with CSV/code discovery allowed only by explicit user instruction.

### Goals

- Raise the default Skill Builder boundary to a top-level repository rule.
- Make `Skill Builder` stop instead of inspecting unrelated CSV/code when parsed fields or work paths are missing.
- Reserve current CSV/code inspection as an explicit exception path that the user must request.

### Constraints

- Role Owner is Designer because this task changes workflow policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims must stay grounded in the inspected text of `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/COMMON.md`, `AGENTS_ROLE/GAMEDESIGNER.md`, `AGENTS_ROLE/GAMEBULIDER_SKILL.md`, and this board.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Future `Skill Builder` requests should provide either parsed fields or explicit work paths if code inspection inside a scoped area is expected.
- If the user wants Builder to derive missing values from current CSV/code, the instruction should say so explicitly instead of leaving that as the default.
- Future blueprint updates should preserve this default boundary unless the user explicitly requests a policy change.

### Evidence

- `AGENTS.md` now has `Skill Builder Absolute Boundary`, which limits default Skill Builder authority to the selected blueprint, explicit parsed input, and explicit code/prefab/scene/asset paths, and says CSV/code discovery requires explicit user instruction.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now says default Skill Builder authority is limited to the selected blueprint, explicit parsed skill data, explicit work paths, and files inside those paths only when blueprint-required inspection is needed.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now says reading current CSV/code as a parsed-source discovery step is forbidden by default and allowed only when the user explicitly instructs Builder to do so.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now requires Builder to stop and report missing parsed fields, scoped row bundles, or explicit work paths instead of broad repository discovery.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` final output requirements now include whether explicit CSV/code discovery was used.

### History

- 2026-05-24: User asked whether Skill Builder could be made to implement from blueprint and user-given paths only, with related CSV/code inspection treated as an exception path that runs only on explicit user command.

## Task: 2026-05-24 Passive Stat Blueprint And Skill Builder Route

### Task title

Add a blueprint-only passive-stat contract and route Skill Builder passive requests to it.

### Goals

- Create a dedicated Skill Builder blueprint for always-on passive number-adjustment skills.
- Keep ordinary passive implementation on blueprint plus parsed input only, without CSV/code rediscovery.
- Force triggered, damage-dealing, target-search, or proc-based passive behavior into stop-and-ask instead of implicit implementation.
- Add a clear routing mapping so passive requests no longer fail on missing blueprint selection.

### Constraints

- Role Owner is Designer because this task changes workflow/design policy, not runtime gameplay code.
- No C# script, scene, prefab, or gameplay CSV values were changed by this task.
- Claims must stay grounded in inspected Skill Builder routing markdown and existing blueprint patterns.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown checks.

### Next Actions

- The passive-stat blueprint route recorded by this historical task was later removed; current Enhancement/Master work uses the single node blueprint.
- If a passive creates gameplay actions instead of always-on numeric modifications, Builder should stop and ask instead of weakening the passive-stat contract.
- If a reusable event-driven passive contract is needed later, create a separate passive-trigger blueprint rather than broadening the passive-stat blueprint.

### Evidence

- Added the then-active passive-stat blueprint, which was later removed.
- `passive-stat-blueprint.md` now defines a blueprint-only contract for `RuntimeKind == Passive` work, including required parsed input, allowed modifier families, common passive-stat contract, and stop-and-ask rules.
- `passive-stat-blueprint.md` explicitly forbids CSV/reference rediscovery by default and blocks event-driven, damage-dealing, target-search, proc-based, and spawn-creating passives behind stop-and-ask.
- The then-active Skill Builder mapping routed passive-stat requests to that blueprint; the mapping was later replaced by the single Enhancement/Master node route.

### History

- 2026-05-24: User asked to create a passive blueprint that lets Skill Builder implement ordinary passives from the blueprint alone and to wire the routing path for future passive requests.

## Source: boards\OPS\REVIEWER_BLACKBOARD.md

## Task: 2026-06-19 Enemy Skill Node Runtime 1-7 Review

### Task title

Review Code Builder enemy skill node runtime implementation steps 1-7 and step 8 retirement readiness.

### Goals

- Verify the node CSV/runtime implementation against the enemy skill handoff steps 1-7.
- Check correctness, build evidence, CSV coverage, and runtime fallback behavior.
- Decide whether old direct execution can be retired safely.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not implement code or CSV fixes.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Reviewer

### Status

Fix required before step 8. Steps 1-7 compile and load, but one damage-coefficient bug and one validation gap block safe retirement.

### Next Actions

- Code Builder fixes spell-only enemy damage coefficient fallback.
- Code Builder adds validation for supported enemy node `action_op` and `target_selector` values before removing direct fallback.
- Do not delete `EnemySkillExecutor` helper methods while node runtime still calls them.

### Evidence

- `EnemySkillData.csv` has `ChainLightning` with attack coefficient `0` and spell coefficient `1.2`.
- `PakuriCsvRuntimeData.EnemyDataset.cs` sets compatibility `coefficient` to spell coefficient when attack coefficient is zero.
- `EnemyCombatSystem.cs` `ResolveAttackDamage(...)` currently uses compatibility `Coefficient` as an attack coefficient fallback and then also adds `SpellPowerCoefficient`, so spell-only offensive skills can add attack damage incorrectly.
- `ValidateEnemySkillNodes(...)` checks unknown skill ids, empty `action_op`, and orphan params, but does not validate `action_op` or `target_selector` against supported runtime values.
- Current assigned Stage1/Stage2 basic and active skill ids all have node rows; reviewer command returned `missing=`.
- Current node CSV rows use supported action ops/selectors; reviewer command returned `unknownActionOps=0` and `unknownSelectors=0`.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing `MSB3277` warnings.
- Unity-MCP sync/load logs showed runtime catalog sync and load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies; warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested Code Reviewer verification of the just-finished Code Builder steps 1-7 and whether step 8 direct path retirement is safe.

## Task: 2026-06-19 Ariel Plan-Action Migration Review

### Task title

Review Code Builder's Ariel-first migration to plan action payloads and Ariel-only compatibility routing.

### Goals

- Verify the prior old wide-field findings are fixed.
- Verify Ariel choices now bypass `ApplyNormalizedChoiceNodes(...)` into `SkillChoiceEffectSpec`.
- Verify new plan action payloads exist and are executed for Ariel modifiers.
- Verify validation/build/Unity-MCP checks pass.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not edit files.
- Review is scoped to Ariel-first target migration; full single-plan ownership of trigger/effect rows is treated as a future architecture step unless explicitly required now.

### Role Owner

Code Reviewer

### Status

Passed with caveat. No blocking findings for the Ariel-first minimum target. Caveat: trigger/effect application rows are still explicit CSV runtime objects, not `SkillExecutionPlan.Actions`.

### Next Actions

- User verifies Ariel runtime behavior in Play Mode.
- If the next target is full single-plan ownership, migrate `monster_skill_triger.csv` / `monster_skill_effects.csv` event actions into plan action node handlers.

### Evidence

- `SkillExecutionPlan.cs` exposes `SkillExecutionPlanNode.Action`, `FromAction(...)`, and `SkillExecutionPlan.Actions`.
- `SkillExecutionSnapshot.cs` line matches show `IsArielChoice(choice)` routes to `ApplyArielChoiceDefinition(choice)`, which calls `ApplyPlanActionNodes(nodes)` and returns before old choice-spec folding.
- `InGameSkillDefinitionMapper.cs` line matches show `if (choice != null && !IsArielChoice(choice)) ApplyNormalizedChoiceNodes(...)`, plus `MapSkillActionOp(...)` mappings for `HitTargetCountBonus` and `StatusCriticalDamageTakenBonus`.
- `SkillExecutionSystem.cs` line matches show Ariel dynamic `CountStatusDamageMultiplier` is applied from mapped plan action nodes.
- CSV scan returned `arielWideNonDefault=0`.
- Spot checks found `ariel-d-trait-4-hit-target-count-bonus` / `bonus=1`, `ariel-d-master-1-status-critical-damage-taken` / `bonus=0.25`, and `ariel-a-master-2-holy-exposure-element-damage-taken` / `bonus=0.15`.
- Ariel A master2 trigger/effect CSV rows show `OnOutgoingDamage`, `EventTarget`, `trigger_action=Effect`, and `status_effect_id=holy-exposure`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP sync/validate menus completed; console showed runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`, with 0 warning/error entries.

### History

- 2026-06-19: User requested Code Builder to execute steps 1-6 toward the target structure, then asked Code Reviewer to verify whether the work was complete.

## Task: 2026-06-19 Ariel A-J Node Conversion Re-Review

### Task title

Re-review whether Ariel A-J are fully converted to skill-body plus atomic effect/modifier/binding nodes without old structure reliance.

### Goals

- Verify Ariel A-J against `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md`.
- Check that original values remain CSV-authored while runtime implementation is not wide-column driven.
- Identify any active old/precombined rows or direct choice-column paths that still affect runtime.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not implement code/CSV fixes during review.
- Findings must be based on inspected files and command/Unity-MCP output.
- Unity Play Mode gameplay parity remains user-owned.

### Role Owner

Code Reviewer

### Status

Fix required findings were addressed by Code Builder. Code Reviewer rerun is still pending explicit user request.

### Next Actions

- Run a fresh Code Reviewer pass only if the user explicitly requests it.
- User verifies Ariel A master2 holy exposure and Ariel E shield amount variants in Play Mode.

### Evidence

- `monster_skill_choices.csv` line 9 keeps `ariel-a-master-2` active as `runtime_support_state=ReferenceDirect` with `status_tag=holy-exposure`, `status_chance_bonus=1`, `status_stacks_set=1`, and `status_element_damage_taken_bonus=0.15`; no `monster_skill_nodes.csv` row targets `ariel-a-master-2`.
- `SkillExecutionSnapshot.cs` lines 787-788 copy `choice.HasStatusElementDamageTakenBonus` and `choice.StatusElementDamageTakenBonus` from choice data into the runtime snapshot; `SkillStatusSpecUtility.cs` lines 150 and 188-190 apply that snapshot value to status data.
- `monster_skill_effects.csv` lines 18-20 keep `ariel-e-shield-trait2`, `ariel-e-shield-master2`, and `ariel-e-shield-trait2-master2` with `runtime_support_state=MigratedToEffectBinding` but non-empty `requires_active_choice_id` values.
- `SkillMultiEffectExecutor.cs` lines 183-188 only suppress disabled effects when `requires_active_choice_id` is blank; the executor does not check `runtime_support_state`, so the migrated Ariel E shield variants can still run when their choices are active.
- `SkillMultiEffectExecutor.cs` lines 776-779 applies both effect `DamageMultiplier` and snapshot `ShieldAmountMultiplier`, so the old precombined Ariel E shield variants can stack with the new node multiplier path.
- Current counts: `monster_skills.csv` has 10 Ariel skill rows, `monster_skill_choices.csv` has 50 Ariel choice rows, `monster_skill_effects.csv` has 35 Ariel effect rows, `monster_skill_triger.csv` has 5 Ariel trigger rows, `monster_skill_nodes.csv` has 37 Ariel node rows, and `monster_skill_node_params.csv` has 41 Ariel node parameter rows.
- Unity-MCP menu runs succeeded for CSV runtime sync, CSV source validation, and InGame skill data validation; console showed `InGame skill data validation passed with 0 warning(s)`, but this validator did not catch the runtime execution issues above.
- Code Builder follow-up converted `ariel-a-master-2` to trigger/effect/node composition: `monster_skill_triger.csv` has `ariel-a-master2-holy-exposure-on-hit`, `monster_skill_effects.csv` has `ariel-a-master-2-holy-exposure-on-hit`, and `monster_skill_nodes.csv` / `monster_skill_node_params.csv` have `ariel-a-master-2-holy-exposure-element-damage-taken` with `bonus=0.15`.
- Code Builder follow-up blanked old `ariel-a-master-2` choice-wide status payload fields in `monster_skill_choices.csv`.
- Code Builder follow-up cleared executable choice gates from the three migrated Ariel E shield variant rows and added validation that rejects `MigratedToEffectBinding` skill effects with executable choice/passive gates.
- Code Builder follow-up changed `SkillTriggerRuntime.ExecuteEffect(...)` to pass `triggerContext.EventTarget` into the effect execution context, enabling trigger-bound `target_selection=EventTarget` status effects.
- Follow-up checks returned no migrated effect rows with executable choice/passive gates and no Ariel old `status_tag` / `status_element_damage_taken_bonus` choice rows for `ariel-a-master-2`.
- Follow-up `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Follow-up Unity-MCP sync/validation logged runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`, with 0 warning/error console entries.

### History

- 2026-06-19: User requested Code Reviewer verification that Ariel A-J are fully node-converted according to the handoff report and no longer depend on old structure, with CSV as source values but skill body plus atomic nodes as implementation shape.
- 2026-06-19: User requested Code Builder to fix the Code Reviewer findings and confirmed the intended Ariel A master2 shape as hit trigger, event target, apply status, and `holy-exposure`.

## Task: 2026-06-19 Ariel Phase 2-5 Code Review

### Task title

Review the Ariel Phase 2-5 normalized node, trigger-binding, and passive ownership cleanup.

### Goals

- Check changed runtime code, CSV rows, and validation evidence for correctness regressions.
- Confirm the new shield amount node path and source-specific status condition are wired to real runtime code.
- Confirm no old Ariel E-owned J passive action-speed rows remain active.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not edit files.
- Review evidence must come from inspected files and command/Unity-MCP output.
- Unity Play Mode gameplay parity remains user-owned.

### Role Owner

Code Reviewer

### Status

Passed with no blocking findings.

### Next Actions

- User verifies runtime behavior in Play Mode for Ariel B, E, and J combinations.

### Evidence

- `SkillChoiceEffectSpec.cs`, `SkillExecutionSnapshot.cs`, `InGameSkillDefinitionMapper.cs`, `SkillExecutionUtility.cs`, and `SkillMultiEffectExecutor.cs` show `ShieldAmountMultiplier` is parsed into choice specs, applied to snapshots, and consumed by shield amount resolution.
- `SkillEffectDefinition.cs`, `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `StatusEffectRuntime.cs`, and `SkillMultiEffectExecutor.cs` show `condition_status_source_skill_id` is parsed, built, and checked through `MatchesConditionStatus(..., requiredSourceSkillId)`.
- `SkillTriggerRuntime.cs` resolves triggered effects from both active and passive effect arrays, so J-owned passive effects referenced by `triggered_effect_id` are discoverable.
- `InGamePassiveEffectRuntime.cs` still routes learned passive effects through `SkillMultiEffectExecutor.Execute(...)`; the moved J post-E effect rows are `enabled_by_default=false` with no effect-level choice gate, so they do not run from the passive refresh path and instead run through J-owned triggers.
- CSV acceptance check returned `eActiveShieldRows=1`, `eDisabledShieldVariants=3`, `shieldAmountNodes=4`, `oldEJRows=0`, `jTriggerRows=2`, and `jShieldSource=ariel-e-shield-base`.
- CSV field-count check returned no bad rows for `monster_skill_choices.csv`, `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- Runtime and editor `dotnet build` commands passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP console logs showed CSV runtime sync from `Assets/CSVdata/authoring`, runtime catalog load, and `InGame skill data validation passed with 0 warning(s)`; Unity-MCP warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested Code Builder to implement remaining Phase 2-5 work and then perform Code Reviewer work.

## Task: 2026-06-19 Ariel Target Runtime Structure Review

### Task title

Review whether Ariel's current skill and execution runtime are fully arranged into the target MonsterUnitActor / RuntimeModel / UnitSkillController / SkillExecutionPlan / handler / combat-service structure.

### Goals

- Check Ariel's current execution architecture against the target responsibility split.
- Identify target-structure gaps based on inspected runtime code and validation output.
- Avoid implementation changes during Code Reviewer work.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not implement fixes.
- Review conclusions must be backed by inspected code or command/Unity-MCP output.

### Role Owner

Code Reviewer

### Status

Fix required. Ariel is closer to the target structure, but the current runtime is not fully in the final target structure.

### Next Actions

- Move actor-owned revive/combat reset state changes out of `MonsterUnitActor`.
- Convert legacy effect/trigger execution into real plan action handler dispatch, instead of carrying raw `SkillEffectDefinition` / `SkillTriggerDefinition` payloads through the plan.
- Remove Ariel-only special routing once the generic plan action path owns the same behavior.

### Evidence

- `MonsterUnitActor.cs` exposes display/animation/debug methods, but `ReviveForNextDay(...)` still mutates `Model.AutoAttackEnabled`, `Model.AutoSkillEnabled`, `Model.Statuses`, `Model.Resources`, `Model.SkillRuntime`, and health resources.
- `BaseUnitRuntimeModel.cs` owns `Identity`, `Stats`, `Resources`, `SkillRuntime`, `Statuses`, `AutoAttackEnabled`, and `AutoSkillEnabled`; `MonsterUnitRuntimeModel.cs` inherits this runtime state model.
- `UnitSkillController.cs` ticks `skillRuntime`, routes auto/manual execution, and creates `SkillExecutionRequest`; `SkillExecutionRequest.cs` carries runtime entry, owner, roster, combat manager, delta time, manual target, aim point, log flag, and animation callback.
- `SkillExecutionPlan.cs` compiles normalized plan nodes and now carries `Actions`, `Effects`, and `Triggers`, but `Effects` / `Triggers` are still raw legacy definitions.
- `SkillPlanActionDispatcher.cs` resolves effects/triggers from plan payloads, but `SkillMultiEffectExecutor.cs` and `SkillTriggerRuntime.cs` still directly execute damage/status/zone/visual/cooldown/reload behavior.
- `SkillExecutionSnapshot.cs` and `InGameSkillDefinitionMapper.cs` still contain Ariel-only choice routing (`IsArielChoice(...)`) to avoid old normalized choice folding.
- `InGameCombatManager.cs` owns roster/skill system coordination and applies damage/status, matching the shared combat coordination target at a broad level.
- Ariel CSV scan returned `arielChoices=50`, `arielEffects=36`, `arielTriggers=6`, `arielNodes=40`, `arielParams=44`, and `arielWideNonDefault=0`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and existing `MSB3277` warnings; parallel build produced one transient `MSB3026` copy retry warning.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and existing `MSB3277` warnings.
- Unity-MCP `Pakuri/InGame/Validate Skill Data` logged `InGame skill data validation passed with 0 warning(s)`.
- Unity console warning/error read returned one unrelated Animator Controller error: `[Worker4] The Animator Controller (lightning-scout_1) you have used is not valid. Animations will not play`.

### History

- 2026-06-19: User requested Code Reviewer to verify whether Ariel's full skill structure and execution structure are configured into the target architecture.

## Task: 2026-06-19 Target Structure Re-Review After Builder Fix

### Task title

Re-review whether the current runtime reaches the target architecture and whether Ariel needs more isolated refactor before moving to other monsters.

### Goals

- Check the target responsibility split after Code Builder's structure fixes.
- Decide whether Ariel is fully final or whether the current structure is good enough to proceed with other monsters.
- Ground the decision in inspected code and validation output.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not implement fixes.
- Findings must be based on inspected files and command/Unity-MCP output.

### Role Owner

Code Reviewer

### Status

Passed with residual architecture caveat. The target structure is now good enough to move to other monsters before doing final handler-class polish. Ariel does not need to remain blocked for more isolated refactor.

### Next Actions

- Proceed with Eve/Vega/Sein/Rin migration using the generic node-backed choice and plan action path.
- After several monsters share the path, finish the remaining handler split by moving `ExecuteDamageEffectAction`, `ExecuteStatusEffectAction`, zone/visual spawn, and trigger cooldown/reload routines out of the legacy executor/runtime files into dedicated action handler classes.
- User performs Play Mode verification for Ariel because current review did not run gameplay.

### Evidence

- `MonsterUnitActor.cs` now contains presentation/animation/damage-popup/debug/collider-facing methods only; search found no `AutoAttackEnabled`, `AutoSkillEnabled`, `Statuses`, `Resources`, `SkillRuntime`, or `CurrentHealth` mutation in that file.
- `MonsterUnitRuntimeStateService.cs` owns next-day runtime reset for auto flags, statuses, shields, health, and active skill runtime state.
- `BaseUnitRuntimeModel.cs` owns identity, stats, resources, statuses, and skill runtime state; `MonsterUnitRuntimeModel.cs` inherits that runtime model.
- `UnitSkillController.cs` still owns skill runtime tick, auto/manual routing, and `SkillExecutionRequest` creation.
- `SkillExecutionPlan.cs` exposes `EffectActions` and `TriggerActions` instead of public raw `Plan.Effects` / `Plan.Triggers`.
- Search under `Pakuri/Assets/Scripts2/InGame/Skills` found no remaining `IsArielChoice`, `ApplyArielChoiceDefinition`, `Plan.Effects`, or `Plan.Triggers`.
- `SkillPlanActionDispatcher.cs` now owns effect-kind dispatch and trigger-action dispatch.
- `SkillMultiEffectExecutor.cs` still owns internal methods named `ExecuteDamageEffectAction`, `ExecuteStatusEffectAction`, persistent zone spawn, and visual spawn; `SkillTriggerRuntime.cs` still owns internal cooldown/reload and triggered attack action methods.
- Ariel active CSV scan returned `arielChoices=50`, `arielEffects=36`, `arielTriggers=6`, and `arielNodes=40`; Ariel still uses explicit trigger/effect rows for event-bound behavior.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- After clearing the Unity console, Unity-MCP `Pakuri/InGame/Validate Skill Data` logged `InGame skill data validation passed with 0 warning(s)`, and warning/error console read returned 0 entries.

### History

- 2026-06-19: User asked Code Reviewer whether the current structure reached the target and whether Ariel should be fully finished before moving to other skills.

## Source: boards\OPS\UNITY_MCP_BLACKBOARD.md

## Task: 2026-05-18 CSV Runtime Sync Via Open Unity Editor

### Task title

Use Unity-MCP as the fallback CSV runtime sync path when Unity batchmode cannot open the project.

### Goals

- Record the open-editor fallback for CSV runtime catalog sync/validation.
- Preserve evidence that the sync method executed even though the direct MCP call timed out.
- Keep Play Mode verification out of Codex scope.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Claims are based on Unity-MCP command output and Unity console logs.

### Role Owner

Code Builder

### Status

Completed for the 2026-05-18 CSV runtime catalog sync task.

### Next Actions

- Prefer `SyncCsvRuntimeCatalogs.bat` when Unity is closed.
- Use Unity menu or Unity-MCP invocation when the project is already open.

### Evidence

- `cmd /c SyncCsvRuntimeCatalogs.bat` failed with Unity's duplicate-project-open guard because `C:/TowerDefence_Pakuri/Test/Pakuri` was already open.
- Unity-MCP `execute_code` for `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` timed out receiving the response, but the Unity console subsequently logged the method's success messages.
- Unity console logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity console logged `Pakuri CSV runtime catalogs synced and validated from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-18: Code Builder used Unity-MCP to complete CSV runtime sync/validation after batchmode could not open the already-open Unity project.

## Source: boards\RUN\REWARD_BLACKBOARD.md

## Task: 2026-05-31 Offering Choice Labels And Active Skill Cap

### Task title

Make Offering reward choices identify their source skill and enforce active skill reward limits.

### Goals

- Show the source monster in each Offering choice card `Summary`.
- Show the source skill and choice title in each Offering choice card `SkillName`.
- Keep Offering active skill rewards capped at two non-default active skills beyond the default A/default active skill.

### Constraints

- Role Owner is Code Builder.
- Reward choice commit still goes through `RunSession.RecordOfferingChoice(...)`.
- No CSV row, column, or schema change was introduced.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that Offering reward cards no longer show only `아리엘 · 특성 1` style labels and that active skill choices stop appearing after two additional active skills.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now uses separate `Summary` and `SkillName` fields in `OfferingChoiceView`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves enhancement display names through linked skill ids and formats examples like `심판의 빛·특성 1`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now gates active skill candidate generation with `MaxAdditionalActiveSkillCount = 2` and excludes `IsDefaultLearned` or slot `A` active skills from the count.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- Unity-MCP `validate_script` for `Assets/Scripts2/InGame/UI/InGameUIManager.cs` reported 0 errors and the existing `Update()` string-concatenation GC warning.

### History

- 2026-05-31: User reported Offering enhancement choices were hard to identify because labels appeared as monster name plus generic trait number.
- 2026-05-31: Code Builder changed Offering reward label binding and active skill candidate gating.

## Task: 2026-05-31 Offering Skill Acquisition Runtime Sync

### Task title

Fix Offering active/passive skill acquisition so the selected runtime model and revived party models receive the learned skill state.

### Goals

- Preserve exact Offering skill choice ids for active/passive skill rewards.
- Make Offering commit refresh every scene-valid monster actor model from `RunSession`.
- Make next-day revive sync learned skill state before the actor is re-registered.

### Constraints

- Role Owner is Code Builder.
- Offering still records through `RunSession.RecordOfferingChoice(...)`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Unity-MCP validator could not run because no Unity Editor instance was found.

### Next Actions

- User verifies active/passive skills obtained through Offering become usable and remain usable after day advance revive.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now sets `ChoiceId` on active skill Offering choices to `skill.SkillId` and on passive skill Offering choices to `passive.PassiveId`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now calls `RefreshSceneMonsterActorSkillModels(...)` after roster-player runtime refresh, syncing all scene-valid `MonsterUnitActor.Model` instances from `RunSession`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now calls `SyncExistingMonsterModelFromSession(model)` before `actor.ReviveForNextDay()`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User asked Code Builder to fix Offering skill acquisition together with the Nexus buff-target issue.
- 2026-05-31: Code Builder kept `RunSession` as the reward state authority and patched runtime synchronization paths that missed unregistered dead monster actors.

## Source: boards\RUN\RUN_BLACKBOARD.md

## Task: 2026-06-19 Stage Flow CSV Folder Move

### Task title

Move active `NewRunScene` stage-flow CSV files into `Assets/CSVdata/stage_flow`.

### Goals

- Separate stage-flow CSV files from runtime catalog CSV files.
- Preserve `NewRunScene` serialized `StageManager` TextAsset references by moving `.meta` files with the CSV files.
- Keep Stage 1 and Stage 2 day/encounter/reward data available to the existing `StageManager` flow.

### Constraints

- Role Owner is Code Builder.
- No StageManager gameplay behavior or CSV row content was changed.
- Unity Play Mode progression verification remains user-owned.

### Role Owner

Code Builder

### Status

Moved and CSV-shape checked.

### Next Actions

- Future stage-flow rows should be edited under `Pakuri/Assets/CSVdata/stage_flow/`.
- User verifies Play Mode day/stage progression as usual.

### Evidence

- `Pakuri/Assets/CSVdata/stage_flow/StageDay.csv` exists after the move with 10 columns, 22 data/type rows after the header, and no field-count mismatch.
- `Pakuri/Assets/CSVdata/stage_flow/StageEncounter.csv` exists after the move with 14 columns, 60 data/type rows after the header, and no field-count mismatch.
- `Pakuri/Assets/CSVdata/stage_flow/StageReward.csv` exists after the move with 13 columns, 9 data/type rows after the header, and no field-count mismatch.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` had serialized `stageDayCsv`, `stageEncounterCsv`, and `stageRewardCsv` TextAsset GUID references before the move; the CSV `.meta` files were moved with the CSV files to preserve those GUIDs.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after the move.

### History

- 2026-06-19: Code Builder moved `StageDay.csv`, `StageEncounter.csv`, and `StageReward.csv` from `Assets/CSVdata/` to `Assets/CSVdata/stage_flow/` while preserving `.meta` files.

## Task: 2026-05-31 Offering Choice Labels And Active Skill Cap

### Task title

Split Offering choice card labels into monster summary and skill name, and enforce the active skill acquisition cap.

### Goals

- Display the monster name in each Offering choice card `Summary` label.
- Display the source skill plus choice title in each Offering choice card `SkillName` label, such as `심판의 빛·특성 1`.
- Stop active skill Offering candidates after the monster has learned two non-default active skills beyond its default A/default active skill.

### Constraints

- Role Owner is Code Builder.
- `InGameUIManager.cs` remains the `NewRunScene` Offering UI owner.
- No CSV schema or runtime catalog ownership change was introduced.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that `Choice1` through `Choice3` show monster names in `Summary`, names like `심판의 빛·특성 1` in `SkillName`, and stop offering extra active skills after two non-default active acquisitions.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now binds Offering card `Summary` and `SkillName` labels separately, while keeping the previous `Text (TMP)` fallback path.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now builds enhancement skill names from `TargetSkillId`, `SkillId`, or reward active/passive ids and resolves display names from active/passive definitions.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` replaced the previous `MaxRunActiveSkillCount = 5` gate with `MaxAdditionalActiveSkillCount = 2`, counting learned active skills while excluding default/A active skills.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- Unity-MCP `validate_script` for `Assets/Scripts2/InGame/UI/InGameUIManager.cs` reported 0 errors and the existing `Update()` string-concatenation GC warning.

### History

- 2026-05-31: User asked Designer whether Offering labels could include the source skill and whether active skill acquisition was capped at A plus two extra active skills.
- 2026-05-31: User approved Code Builder implementation for `Summary`/`SkillName` card display and active skill cap enforcement.

## Task: 2026-05-31 Offering Learned Skill Sync For Revived Monsters

### Task title

Ensure Offering-acquired skills persist into runtime models, including monsters that died before the Offering choice and are revived on the next day.

### Goals

- Record active/passive Offering skill choices with stable choice ids.
- Refresh learned skill runtime models for scene monster actors even when they are not currently registered in the combat roster.
- Sync session learned active/passive/choice state back into existing dead monster models before `ReviveForNextDay()` re-registers them.

### Constraints

- Role Owner is Code Builder.
- Existing RunSession state remains the authority for learned skills.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Unity-MCP validator could not run because no Unity Editor instance was found.

### Next Actions

- User verifies in Play Mode that Offering-acquired skills appear immediately for living monsters and remain available after dead selected/manifested monsters revive on the next day.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now writes `ChoiceId = skill.SkillId` for active skill Offering choices and `ChoiceId = passive.PassiveId` for passive skill Offering choices.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now refreshes all scene-valid `MonsterUnitActor` models from `RunSession` and rebuilds learned active runtime sets after Offering commit, not only currently registered roster players.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now syncs `LearnedActives`, `LearnedPassives`, and `ChosenChoiceIds` from `ActiveSession` into an existing monster model before `ReviveForNextDay()` and `RegisterPlayerMonster(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now filters revived player-slot lookup to `UnitRole.Monster`, keeping Nexus out of player monster revive lookup.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User reported Offering skill acquisition did not result in skills being acquired.
- 2026-05-31: Code inspection showed `RunSession.RecordOfferingChoice(...)` records learned active/passive ids, but dead monsters are absent from `combatManager.Roster.Players`; Code Builder added scene-actor refresh and revive-time session sync.

## Task: 2026-05-31 Day Advance Monster Revive And Nexus HP Persistence

### Task title

Reuse defeated monster actors on day advance and preserve Nexus HP across rounds.

### Goals

- Revive existing defeated player monster actors instead of spawning replacement prefabs when the next day starts.
- Restore revived monster HP to max and return animation to idle.
- Re-register revived monsters into the combat roster so enemies can target them again.
- Preserve Nexus current HP across day transitions.

### Constraints

- Role Owner is Code Builder.
- Nexus HP persistence no longer relies only on `NexusUnitActor.Model` surviving; `StageManager` preserves and reapplies the current Nexus HP during day advance.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated. 2026-05-31 follow-up fixed Nexus HP reset during day advance and restored revived monster combat state.

### Next Actions

- User verifies in Play Mode that a dead selected or manifested monster revives on next day without a new prefab instance, attacks again, and that Nexus HP remains at the previous round value.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now tries `TryReviveExistingPlayerBySlot(...)` for selected and manifested party slots before calling prefab respawn/spawn paths.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` finds existing `MonsterUnitActor` instances by `Identity.SlotIndex`, calls `ReviveForNextDay()`, and re-registers them with `InGameCombatManager.RegisterPlayerMonster(...)`.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now exposes `ReviveForNextDay()`, restores HP to max, re-enables child `Collider2D` components, refreshes the view, and returns animation to idle.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now restores common revived monster combat state by enabling auto attacks, clearing statuses/shields, resetting active skill runtime state, and restoring AutoSkill only for non-selected monsters.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now resolves the selected player entry by `UnitRole.Monster` and `SlotIndex == 0` instead of taking `roster.Players[0]`, so Nexus cannot receive the selected monster Auto setting.
- `Pakuri/Assets/Scripts2/InGame/Animation/Animation_Controller.cs` now exposes `ReviveToIdle()` to stop death freeze, restore animator speed, clear dead state, and play idle.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` captures Nexus current HP before `RunSession.AdvanceDay()`, excludes non-monster player units from the day-advance HP restore loop, and reapplies preserved Nexus HP after `NexusUnitActor.Initialize()`.
- `Pakuri/Assets/Scripts2/InGame/Units/NexusUnitActor.cs` now exposes `TryGetCurrentHealth(...)` and `SetCurrentHealth(...)` for StageManager HP carryover.
- Unity-MCP `validate_script` reported 0 errors for `SceneEntryManager.cs`, `MonsterUnitActor.cs`, and `Animation_Controller.cs`; only the existing `Animation_Controller.Update()` GC warning remained.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- 2026-05-31 follow-up validation: Unity-MCP `validate_script` reported 0 errors for `MonsterUnitActor.cs`, `StageManager.cs`, and `NexusUnitActor.cs`; `InGameCombatManager.cs` validator reported a duplicate `ResolveEffectManager` signature, but PowerShell search found only one declaration at line 1202 and both dotnet builds passed with 0 errors.
- 2026-05-31 follow-up validation: Unity refresh reached idle and Unity warning/error console read returned 0 entries.

### History

- 2026-05-31: User requested Code Builder work so dead monsters revive on stage/day advance instead of spawning new prefabs, while Nexus HP persists across rounds.
- 2026-05-31: Code Builder added actor revive/re-register path and confirmed Nexus initialization already preserves current HP when the model exists.
- 2026-05-31: User reported revived monsters could not attack/Auto and Nexus HP still reset to max on next stage; Code Builder added common revived monster combat-state restore, selected-monster Auto lookup by monster slot, and explicit StageManager Nexus HP carryover.

## Task: 2026-05-31 Nexus Assault Win Defeat Flow

### Task title

Add a `NewRunScene` Nexus target, defeat flow, and configurable Stage 2-11 win flow.

### Goals

- Register the `Nexus` as a player-side runtime target after the selected player monster spawns.
- Make enemies attack the Nexus only after no non-Nexus player targets remain.
- Show `Canvas/DefeatPanel` when Nexus HP reaches 0.
- Show `Canvas/WinPanel` when the configured clear stage/day is reached.
- Route both Win and Defeat buttons through the same return-to-main-menu method.

### Constraints

- Role Owner is Code Builder.
- The actual build scene is `Assets/Scenes/NewScene/NewMainMenu.unity`; no `NewMainScene` file was found in the build scene list.
- `winStageIndex` and `winDayIndex` remain inspector-editable because Stage 2-11 is prototype authority.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, scene-bound, compile-verified, and CSV-synced.

### Next Actions

- User verifies in Play Mode that enemies damage the Nexus after monsters are gone, disappear after applying Nexus damage, and both end-flow buttons return to `NewMainMenu`.
- If the prototype win condition changes, update `StageManager.winStageIndex` and `StageManager.winDayIndex` in the inspector.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now registers `NexusUnitActor`, handles `OnNexusDefeated`, hides/shows Win/Defeat panels, and routes both end buttons to `ReturnToMainMenu()`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` exposes `winStageIndex=2`, `winDayIndex=11`, and `mainMenuScenePath=Assets/Scenes/NewScene/NewMainMenu.unity`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyTargeting.cs` searches non-Nexus player targets first, then falls back to Nexus.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` moves enemies toward Nexus, applies `enemyModel.NexusDamage`, then despawns the damaging enemy.
- Unity-MCP scene inspection found `Nexus` with `Pakuri.InGame.NexusUnitActor` and trigger `BoxCollider2D`; `StageManager` serialized `nexusActor`, `winPanel`, and `defeatPanel`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User asked Code Builder to implement the Nexus attack/defeat/win flow and make enemies disappear after damaging the Nexus.
- 2026-05-31: Code Builder added a Nexus runtime actor/model, enemy Nexus assault path, StageManager end-flow handling, and scene bindings.

## Task: 2026-05-31 Stage2 NewRunScene Stage Flow Rows

### Task title

Add Stage 2 day, encounter, reward, and prefab-binding data needed by the active `NewRunScene` run flow.

### Goals

- Ensure `RunSession.AdvanceDay()` can move from Stage 1 day 11 to Stage 2 day 1 without missing `StageDay` data.
- Ensure `StageManager` can resolve Stage 2 encounter and reward rule IDs.
- Ensure `EnemySpawnManger` can resolve Stage 2 enemy prefabs when `StageEncounter.csv` emits Stage 2 enemy ids.

### Constraints

- Role Owner is Code Builder.
- Stage 2 reward values are temporary copies of Stage 1 values until a Stage 2 reward-balance source is provided.
- Unity Play Mode progression verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and non-gameplay verified; Stage 2 spawn positions normalized to Stage 1 coordinates.

### Next Actions

- User verifies reaching Stage 2 after Stage 1 boss in Play Mode.
- If Stage 2 spawn density or reward economy feels wrong, update the CSV rows rather than adding code branches.

### Evidence

- `Pakuri/Assets/CSVdata/StageDay.csv` contains 11 Stage 2 day rows from day 1 through day 11.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` contains Stage 2 normal, midboss, day-10 midboss, and boss encounter rows.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` Stage 2 normal and escort rows use `spawn_x=9.02`, `spawn_y_min=-5`, `spawn_y_max=5`; Stage 2 guaranteed boss rows use `spawn_x=9.02`, `spawn_y_min=0`, `spawn_y_max=0`, matching the Stage 1 spawn-coordinate pattern.
- `Pakuri/Assets/CSVdata/StageReward.csv` contains the Stage 2 reward rule IDs referenced by `StageDay.csv`.
- PowerShell reference check returned no missing day encounter, day reward, or encounter enemy references.
- PowerShell spawn-coordinate check returned `stage2Rows=30 badNormal=0 badBoss=0`.
- Unity-MCP scene inspection showed `EnemySpawnManger.enemyPrefabBindings` populated with all 8 Stage 2 prefab references after reloading `Assets/Scenes/NewScene/NewRunScene.unity`.

### History

- 2026-05-31: User requested Stage 2 spawn-rule connection through `StageManager` after Stage 2 prefab/component setup.
- 2026-05-31: Code Builder added the data rows that the existing `StageManager` already consumes, avoiding a new StageManager code branch.
- 2026-05-31: Code Builder changed Stage 2 encounter spawn coordinates from the previous far/right Stage 2 values to the same Stage 1 coordinate pattern after the user reported abnormal Stage 2 enemy spawn positions.

## Task: 2026-05-31 DebugUI Offering-State Commit Path

### Task title

Keep DebugUI skill and enhancement acquisition synchronized with the Offering duplicate-filter state.

### Goals

- Route DebugUI active skill acquisition through the same learned-active state used by Offering.
- Route DebugUI passive skill acquisition through the same learned-passive state used by Offering.
- Route DebugUI active and passive enhancement acquisition through `RunSession.RecordOfferingChoice`.
- Preserve `ChosenChoiceIds` recording so later Offering candidates can suppress choices already taken from DebugUI.

### Constraints

- Role Owner is Code Builder.
- This task changes debug acquisition plumbing only; it does not change the Offering candidate builder or CSV reward schema.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that DebugUI-acquired active skills, passive skills, active enhancements, and passive enhancements do not reappear in Offering.
- If a specific reward row still reappears, inspect that row's `choiceId`/`rewardId` linkage before widening runtime logic.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` is the inspected Offering owner and commits choices with `RecordOfferingChoice(choice.MonsterId, choice.RewardId, choice.ChoiceId, choice.ActiveSkillId, choice.PassiveSkillId)`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` stores `ChosenRewardIds`, `ChosenChoiceIds`, `LearnedActives`, and `LearnedPassives`, and exposes `HasLearnedActive(...)` and `HasLearnedPassive(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:130` now routes active debug skill acquisition through `CommitDebugOfferingChoice(...)` instead of directly mutating only learned active state.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:165` routes passive debug skill acquisition through `CommitDebugOfferingChoice(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:673` routes active enhancement acquisition through `CommitDebugOfferingChoice(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:703` routes passive enhancement acquisition through `CommitDebugOfferingChoice(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:900` calls `RunSession.RecordOfferingChoice(...)`; `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:936` resolves exact reward id matches and otherwise records the exact choice id fallback.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only existing MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after an initial parallel-build file lock.
- Unity-MCP `validate_script` on `Assets/Scripts2/InGame/UI/DebugUI.cs` reported 0 errors and one pre-existing-style validator warning about string concatenation in `Update()`.
- Unity-MCP warning/error console read after script refresh returned 0 entries.

### History

- 2026-05-31: User required DebugUI, DebugModifiedUI, and DebugPassiveModifiedUI acquisition to use the Offering acquisition path so selected items stop appearing in Offering.
- 2026-05-31: Code Builder added `DebugUI.CommitDebugOfferingChoice(...)` and routed active/passive skills plus active/passive enhancement buttons through that helper.

## Task: 2026-05-27 NewRunScene Manual Projectile Hold Ownership

### Task title

Restrict manual hold-repeat input to projectile skills and preserve one-click behavior for other active skills in `NewRunScene`.

### Goals

- Keep manual input ownership in `InGameCombatManager` instead of moving projectile burst continuation into auto-skill routing.
- Let manual projectile skills re-sample the current cursor direction while the mouse button is held.
- Preserve beam, zone, and single-attack manual casts as one-click actions that do not retarget after activation.

### Constraints

- Role Owner is Code Builder.
- This is a runtime input/control fix only; no CSV authority or scene prefab registry change was introduced.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies that manual projectile skills continue firing while the button is held and that cursor movement affects subsequent projectile shots.
- User verifies that manual non-projectile skills still cast once per click and do not change direction or target after activation.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` previously gated all manual skill execution on `IsPrimaryMousePressedThisFrame()`, which prevented projectile burst follow-up shots from routing after the first click.
- The same file now distinguishes projectile runtimes from non-projectile runtimes during manual input handling, using held-button cursor sampling only for `ProjectileSkillData`.
- Manual projectile burst continuation now stays on the manual execution path by reusing latched manual aim/target data when the mouse button is no longer held but a projectile runtime is still bursting.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock on `Assembly-CSharp.dll`.

### History

- 2026-05-27: User requested that projectile skills gain manual hold-repeat behavior while beam, zone, and single-attack skills keep their current one-click activation model.

## Task: 2026-05-26 Rin-B/Rin-C NewRunScene Runtime Verification

### Task title

Verify the current `NewRunScene` runtime accepts the Rin-B/Rin-C shared-skill implementation without compile or refresh errors.

### Goals

- Confirm the shared beam/buff/status runtime changes compile on both runtime and editor assemblies.
- Confirm Unity refresh returns to idle after the Rin-B/Rin-C source changes.
- Confirm warning/error console reads do not show new C# or CSV runtime failures.

### Constraints

- Role Owner is Skill Builder.
- This task records runtime validation only; gameplay verification remains user-owned.
- Existing external assembly conflict warnings are preserved as-is.

### Role Owner

Skill Builder

### Status

Compile-verified and refresh-checked.

### Next Actions

- User verifies Rin-B/Rin-C behavior in Play Mode.
- If a later gameplay-only issue appears, start from the current compile/refresh-clean baseline instead of rechecking schema wiring first.

### Evidence

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after the Rin-B/Rin-C work; only the existing `System.Net.Http` / `System.IO.Compression` MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` first failed inside the sandbox with `Access to the path 'C:\Users\t3312\AppData\Local\Microsoft SDKs' is denied`, then passed with 0 errors when rerun unsandboxed; this was an environment permission issue, not a code error.
- Unity `refresh_unity` returned `resulting_state":"idle"` after the Rin-B/Rin-C source changes.
- Unity warning/error console reads after refresh returned only MCP-FOR-UNITY client connection/disposal logs and did not report C# compile errors or CSV runtime sync failures.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `Skills/Execution/Executors/BeamSkillExecutor.cs`, `Skills/Execution/Executors/SupportSkillExecutors.cs`, and `Skills/Execution/SkillMultiEffectExecutor.cs` are the inspected runtime owners for the new Rin-B/Rin-C execution paths validated by the builds and refresh.

### History

- 2026-05-26: Skill Builder completed Rin-B/C implementation and then verified the active `NewRunScene` runtime path through build plus Unity refresh/console checks.

## Task: 2026-05-18 NewRunScene Current Runtime Authority

### Task title

Keep the current `NewRunScene` runtime authority split explicit and compact.

### Goals

- Preserve `EffectManager` as the current monster/enemy skill visual authority in the kept new scene flow.
- Preserve the explicit separation between chosen reward IDs and chosen runtime choice IDs.
- Preserve the current CSV runtime catalog path without the serialized `fallbackCatalog` dependency on `NewRunScene`.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed intermediate migration history remains in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active run/runtime authority summarized and retained for future work. 2026-05-18 Code Builder refactor keeps the same runtime authority while retaining Offering and Menifest flow helpers inside `InGameUIManager.cs`. 2026-05-18 monster projectile/status runtime tuning is now skill-row based. 2026-05-18 follow-up renamed the enemy combat owner to `EnemyCombatSystem.cs` and absorbed the former cooldown helper into that file.

### Next Actions

- If runtime ownership changes again, update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/COMBAT/ENEMY_BLACKBOARD.md`.
- Use the archive snapshot when older step-by-step migration history is needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` plus `Assets/Scenes/NewScene/NewRunScene.unity` own the current monster/enemy skill visual registry path.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now owns `EnemyCombatSystem`, and `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` now holds both the enemy combat loop and the former cooldown-rule helper logic used during `NewRunScene` combat ticks.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` now keeps `ChosenRewardIds` and `ChosenChoiceIds` separately.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` keeps the top-level reward/UI binding and now contains the Offering and Menifest flow helper types directly in the same file.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now passes `rewardId` plus the exact enhancement `choiceId` into the session and owns active/passive/enhancement Offering choice construction through its integrated helper types.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still owns Menifest candidate, fail, success, commit, and skip popup flow while preserving the same scene-binding entry points.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` no longer keeps the retained `fallbackCatalog` scene dependency.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now owns per-skill projectile speed, pierce count, status chance, and status label; `monsters.csv` no longer owns those duplicate projectile/status columns.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps projectile speed, pierce, and status chance directly from `SkillDefinition`; `SkillExecutors.cs` no longer overrides Eve-A shock chance in code.

### History

- 2026-05-18: EffectManager scene authority, reward/choice separation, and removal of the serialized fallback catalog became the active run/runtime baseline.
- 2026-05-18: Code Builder split Offering and Menifest UI flows into separate helpers while keeping `InGameUIManager.cs` as the scene-binding facade.
- 2026-05-18: Code Builder later merged `OfferingUI.cs` and `MenifestUI.cs` back into `InGameUIManager.cs` during the repository-wide high-integration consolidation pass, keeping the same flow ownership in one file.
- 2026-05-18: Code Builder consolidated monster projectile/status tuning into skill rows and verified runtime/editor builds with 0 errors.
- 2026-05-18: Code Builder renamed `EnemyCombatSimulationSystem.cs` to `EnemyCombatSystem.cs` and absorbed `EnemySkillCooldown.cs` into that owner while preserving the same `NewRunScene` runtime authority path.

## Task: 2026-05-17 Surviving New Scene Flow Baseline

### Task title

Keep the surviving new scene flow and core Eve/status runtime handoff explicit.

### Goals

- Preserve `NewMainMenu.unity` and `NewRunScene.unity` as the surviving supported scene path.
- Preserve the current status-label refresh path and Eve-A choice-modifier execution path used by the kept new run flow.
- Keep the board clear that older Legacy controller retirement progress detail now lives in the archive snapshot.

### Constraints

- Role Owner is Code Builder.
- This retained baseline is kept because it still defines the active scene flow used by ongoing work.
- Detailed phase-by-phase migration history remains in the archive snapshot.

### Role Owner

Code Builder

### Status

Retained as the active new-scene flow baseline.

### Next Actions

- Future run work should assume only the `NewMainMenu` -> `NewRunScene` path survives.
- If scene ownership changes, update this file together with `boards/UI/UI_BLACKBOARD.md`.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity` and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` remain the surviving scene pair.
- `Pakuri/ProjectSettings/EditorBuildSettings.asset` was recorded as containing only those two kept scene paths.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `BaseUnitRuntimeModel.cs`, and `StatusEffectKind.cs` own the current status label refresh baseline used by `NewRunScene`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs`, `SkillRuntimeInstance.cs`, and `SkillExecutors.cs` own the current Eve-A chosen-choice execution path.

### History

- 2026-05-17: Legacy scene/controller cleanup, status label runtime, and Eve-A projectile modifier runtime were recorded against the surviving new-scene flow.

## Task: 2026-05-29 Damage Meter Runtime Handoff

### Task title

Prepare the runtime damage-source tracking portion of the damage meter UI handoff.

### Goals

- Track player monster damage at the `InGameCombatManager.ApplyDamage` boundary.
- Use actual applied health plus shield delta for current-round totals.
- Preserve `RunSession.ManifestedMonsterIds` order for 2P to 5P display.

### Constraints

- Role Owner is Code Builder.
- Designer created the handoff only; no runtime implementation was performed.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented by Code Builder.

### Next Actions

- User verifies live Play Mode damage totals and source segmentation during combat.
- If future damage executors need more granular source names, pass `damageMeterSourceId` / `damageMeterDisplayName` through those specific executor paths.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` exposes `ApplyDamage(... BaseUnitRuntimeModel source, ... string sourceSkillId ...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` returns `InGameResourceChangeResult` with `PreviousHealth`, `CurrentHealth`, `PreviousShield`, `CurrentShield`, and `AppliedDamage`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` appends manifested monster ids in `ManifestedMonsterIds`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` uses `session.ManifestedMonsterIds.Count` to compute manifested spawn slot index.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls `DamageMeterRuntimeTracker.RecordDamage(options, result)` immediately after `resourceMutations.ApplyDamage(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now resets `DamageMeterRuntimeTracker.Active` in `StartCurrentDay()` before the current day combat flow starts.
- `DamageApplicationOptions` now carries optional meter-only `DamageMeterSourceId` and `DamageMeterDisplayName`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` passes trigger ids as meter source ids for direct trigger damage where the runtime path exposes them.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` accepts optional `damageMeterSourceId` and forwards it to `ApplyDamage`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-29: User requested a Code Builder handoff for damage meter runtime tracking and source naming.
- 2026-05-29: Code Builder implemented the damage meter runtime hook and meter-only source metadata path.

## Source: boards\UI\RUNSCENE_UI.md

## Task: 2026-05-31 Offering Choice Card Summary And SkillName

### Task title

Bind `NewRunScene` Offering choice cards to monster summary and source skill names.

### Goals

- Fill `Choice1` through `Choice3` `Summary` labels with the monster display name.
- Fill `Choice1` through `Choice3` `SkillName` labels with the source skill and choice title.
- Preserve the existing `Desc` label as the effect description.
- Preserve fallback behavior for older card layouts that only have `Text (TMP)`.

### Constraints

- Role Owner is Code Builder.
- This is UGUI `Canvas/OfferingPanel` behavior in `NewRunScene`.
- No scene asset edit was required because the inspected scene already contains `Summary` and `SkillName` label names.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that Offering card text appears in the intended authored labels and does not overflow or bind to the wrong TMP child.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains `Choice1`, `Choice2`, and `Choice3` related `Summary` and `SkillName` child names.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves `Summary`, `SkillName`, `Desc`, `Icon`, and fallback `Text (TMP)` for each Offering button.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now sets `Summary` to the monster display name and `SkillName` to values such as `심판의 빛·특성 1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- Unity-MCP `validate_script` for `Assets/Scripts2/InGame/UI/InGameUIManager.cs` reported 0 errors and the existing `Update()` string-concatenation GC warning.

### History

- 2026-05-31: User requested Offering `Choice1` through `Choice3` UI labels to place the monster name in `Summary` and source skill plus trait title in `SkillName`.
- 2026-05-31: Code Builder implemented the UGUI label binding in `InGameUIManager.cs`.

## Task: 2026-05-31 Offering Skill Choice Commit Refresh

### Task title

Keep Offering active/passive skill choices visible to runtime UI/model state immediately after commit, including dead scene actors.

### Goals

- Give active/passive Offering choices non-empty `ChoiceId` values.
- Refresh registered roster monster models and dead/unregistered scene monster actor models after Offering commit.
- Rebuild learned active skill runtime sets after the session state is copied into each model.

### Constraints

- Role Owner is Code Builder.
- `InGameUIManager.cs` remains the active NewRunScene Offering owner.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Unity-MCP validator could not run because no Unity Editor instance was found.

### Next Actions

- User verifies in Play Mode that Offering skill choices update the monster's available skills and that dead monsters keep those choices after next-day revive.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` commits Offering choices through `session.RecordOfferingChoice(choice.MonsterId, choice.RewardId, choice.ChoiceId, choice.ActiveSkillId, choice.PassiveSkillId)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now assigns `ChoiceId = skill.SkillId` for active skill Offering choices and `ChoiceId = passive.PassiveId` for passive skill Offering choices.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now refreshes scene-valid `MonsterUnitActor` models from `RunSession` and rebuilds learned active runtime sets after Offering commit.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User reported Offering-acquired skills were not acquired.
- 2026-05-31: Code Builder patched the Offering UI commit refresh so it no longer depends only on `combatManager.Roster.Players`.

## Task: 2026-05-31 Nexus HP And End Panels

### Task title

Bind Nexus HP text and Win/Defeat panels for the `NewRunScene` end flow.

### Goals

- Display Nexus HP as `current / max` in `Canvas/Info/NexusHPinfo`.
- Show `Canvas/DefeatPanel` on Nexus defeat.
- Show `Canvas/WinPanel` on the configured Stage 2-11 prototype clear condition.
- Use the same return-to-main-menu method for both Win and Defeat buttons.

### Constraints

- Role Owner is Code Builder.
- The real main menu scene path is `Assets/Scenes/NewScene/NewMainMenu.unity`.
- `NexusUnitActor` auto-resolves `Canvas/Info/NexusHPinfo` when the serialized field is blank.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, scene-bound, and compile/editor validated.

### Next Actions

- User verifies in Play Mode that Nexus HP text updates, DefeatPanel/WinPanel activate at the right time, and both buttons load `NewMainMenu`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/NexusUnitActor.cs` writes Nexus HP as `current / max` and auto-resolves `Canvas/Info/NexusHPinfo`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` resolves `Canvas/WinPanel`, `Canvas/DefeatPanel`, and panel child `Button` components, then binds both to `ReturnToMainMenu()`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` hides Win/Defeat panels on startup and shows the matching panel on victory or defeat.
- Unity-MCP scene inspection found `Canvas/Info/NexusHPinfo`, `Canvas/WinPanel`, `Canvas/DefeatPanel`, and `Nexus` with `NexusUnitActor`.
- Unity-MCP `validate_script` on `Assets/Scripts2/InGame/Units/NexusUnitActor.cs` and `Assets/Scripts2/InGame/Core/StageManager.cs` reported 0 warnings and 0 errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User asked for Nexus HP display plus Win/Defeat buttons that return to `NewMainMenu`.
- 2026-05-31: Code Builder implemented `NexusUnitActor` HP text binding and StageManager end-panel/button flow.

## Task: 2026-05-31 DebugUI Passive Buttons And Offering-State Recording

### Task title

Extend `NewRunScene` DebugUI to learn passive F-J skills and passive enhancements through the same run-state path used by Offering.

### Goals

- Add F-J DebugUI skill buttons to the same A-J slot resolution path.
- Let F-J buttons learn passive skills for the currently selected player monster.
- Let each F-J button child `EmodifierBtn` open `DebugPassiveModifiedUI`.
- Let `DebugPassiveModifiedUI/Trait1` through `Trait3` acquire passive enhancement choices.
- Record DebugUI skill and enhancement acquisition through `RunSession.RecordOfferingChoice` so learned skills and chosen choice ids can be filtered out of later Offering choices.

### Constraints

- Role Owner is Code Builder.
- This is UGUI `NewRunScene` debug tooling work, not a gameplay balance or CSV schema change.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that F-J buttons learn passives for the selected monster.
- User verifies that F-J `EmodifierBtn` opens `DebugPassiveModifiedUI` and Trait1-Trait3 can be acquired.
- User verifies that DebugUI-acquired skills/enhancements do not appear again in Offering.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:17` expands `DebugSlots` from A-E to A-J.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:133` adds `TryLearnPassiveSlot`, resolving passives with `PakuriDataManager.Instance.ResolvePassiveSkill(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:324` resolves `DebugPassiveModifiedUI`, and `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:377` resolves `DebugPassiveModifiedUI/Trait1` through `Trait3`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:678` adds `ApplyPassiveModifierChoice` for passive enhancement acquisition.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:900` adds `CommitDebugOfferingChoice`, which calls `RunSession.RecordOfferingChoice(...)` and then refreshes runtime skill models, button labels, modifier buttons, and the monster panel.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:936` keeps enhancement reward lookup exact by returning a matching reward id only when `RewardId == choice.ChoiceId`; otherwise it records the exact choice id fallback.
- Unity-MCP scene inspection found `Canvas/DebugUI/FBtn` through `JBtn`, each with child `EmodifierBtn`, and found `Canvas/DebugPassiveModifiedUI/Trait1` through `Trait3`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only existing MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after an initial parallel-build file lock.
- Unity-MCP `validate_script` on `Assets/Scripts2/InGame/UI/DebugUI.cs` reported 0 errors and one pre-existing-style validator warning about string concatenation in `Update()`.
- Unity-MCP warning/error console read after script refresh returned 0 entries.

### History

- 2026-05-31: User asked Designer how to extend `DebugUI` for F-J passive acquisition and passive enhancement acquisition.
- 2026-05-31: User then asked Code Builder to implement the described DebugUI and Offering-state recording changes.
- 2026-05-31: Code Builder implemented A-J slot binding, passive acquisition, passive modifier panel binding, and shared run-state commit logic in `DebugUI.cs`.

## Task: 2026-05-17 NewRunScene Active UI Rules

### Task title

Keep the current `NewRunScene` UI behavior compact and explicit.

### Goals

- Preserve active status suffix display on unit name labels.
- Preserve the current `AutoBtn` route that switches 1P A between manual and automatic execution.
- Preserve the current Offering enhancement availability filter based on learned active/passive state.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older UI task history remains in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active `NewRunScene` UI rules summarized and retained for future work. 2026-05-18 Code Builder refactor centralizes shared unit actor display logic and now keeps Offering/Menifest behavior inside `InGameUIManager.cs` through integrated helper types.

### Next Actions

- User verifies in Play Mode that label suffixes, AutoBtn behavior, and Offering gating still match the retained baseline.
- Future UI work should update this file only when those active rules change.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `EnemyUnitActor.cs` now delegate shared name/status/HP/shield/damage-popup presentation to `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAutoSkillButton.cs` plus `InGameCombatManager.cs` own the current AutoBtn behavior.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` keeps `Canvas/AutoBtn` wired to `Pakuri.InGame.InGameAutoSkillButton`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` owns top-level `NewRunScene` UI lookup/binding and now contains the Offering/Menifest flow helper types directly in the same file.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still owns the learned-skill Offering enhancement filter, Offering choice commit path, Menifest popup state, candidate commit, and skip behavior through those integrated helper types.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.

### History

- 2026-05-15: AutoBtn manual/auto routing became part of the active baseline.
- 2026-05-17: Status suffix display and Offering enhancement availability filtering were added to that active baseline.
- 2026-05-18: Code Builder split `InGameUIManager` into Offering and Menifest helper flows, and commonized `MonsterUnitActor`/`EnemyUnitActor` presentation through `UnitActorView.cs`.
- 2026-05-18: Code Builder later re-merged the Offering and Menifest helper files into `InGameUIManager.cs` during the repository-wide high-integration consolidation pass.

## Task: 2026-05-29 Damage Meter UI Handoff

### Task title

Prepare the Code Builder handoff for the authored `NewRunScene` damage meter overlay.

### Goals

- Keep the damage meter UI work grounded in the existing authored `Canvas/DamageMeterUI` hierarchy.
- Route implementation to a separate damage meter UI/controller path instead of expanding Offering/Menifest ownership in `InGameUIManager.cs`.
- Preserve 1P to 5P panel order based on selected monster plus `RunSession.ManifestedMonsterIds`.
- Keep damage meter skill bars bounded by `MeterBG` width, with 1st-place total damage as the full-width reference.
- Apply repeated skill segment colors in red, blue, light green, sky blue, yellow, purple, and dark green order.
- Preserve the authored `Skill-Meter` RectTransform Y/anchor/pivot while resizing cloned skill segments.
- Resolve trigger-based damage meter labels back to the trigger source skill/passive display name when available.
- Prefer `monster_skills.csv` active/passive `display_name` over choice or trigger-derived names when the damage source id is a real skill id.

### Constraints

- Role Owner is Code Builder.
- Designer created the handoff only; no code or scene implementation was performed.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented by Code Builder.

### Next Actions

- User verifies live Play Mode numbers, button behavior, and visual fit for the authored meter layout.
- Future icon work can fill `MonsterIconImage` values in `monsters.csv`; blank values are currently supported.

### Evidence

- Unity-MCP found `Canvas/DamageMeterUIBtn` and `Canvas/DamageMeterUI` in `NewRunScene`.
- Unity-MCP found `Canvas/DamageMeterUI/1PDamagePanel` through `5PDamagePanel`; `1PDamagePanel` includes `Image`, `Monster_Name_Text`, `Total_Damage`, `Total_Damage_Persent`, `MeterBG`, and `Skill-Meter/SkillName`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` exposes `ApplyDamage(... source, ... sourceSkillId ...)` and returns `InGameResourceChangeResult`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` stores `ManifestedMonsterIds` in append order.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterRuntimeTracker.cs` records player monster damage by actual health plus shield delta.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` auto-resolves `Canvas/DamageMeterUIBtn`, `Canvas/DamageMeterUI`, `Close`, and `1P~5PDamagePanel` children by name.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` calculates each skill meter width from `source.Damage / leaderDamage`, clamps accumulated width to `MeterBG`, and applies a fixed seven-color segment palette.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` now caches the authored `Skill-Meter` anchor, pivot, and Y position so clones only change X/width.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` now resolves trigger ids such as `rin-f-followup` through `SkillTriggerDefinition.SourceSkillId`, so `rin-f` damage can display the passive name from `monster_skills.csv`.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` now resolves active/passive skill ids before choice titles or trigger source fallback, and trigger fallback no longer matches `TriggeredSkillId`, preventing `rin-a`/`sein-a` from being overwritten by related passive or trigger labels.
- Unity-MCP component inspection found `Pakuri.InGame.DamageMeterRuntimeTracker` and `Pakuri.InGame.DamageMeterUIController` attached to `Canvas`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity console after CSV validation/sync logged runtime catalog load and CSV runtime catalog sync without Pakuri CSV failure.

### History

- 2026-05-29: User requested a Code Builder implementation handoff for the damage meter UI design.
- 2026-05-29: Code Builder implemented the runtime tracker, UI controller, combat hook, and Canvas scene binding for the authored damage meter overlay.
- 2026-05-29: Code Builder changed skill meter widths to use the leader-damage scale and added the requested seven-color repeating segment palette.
- 2026-05-29: Code Builder preserved authored skill-meter Y/anchor/pivot on clones and routed trigger damage labels back to their source skill/passive display names.
- 2026-05-29: Code Builder changed damage meter label resolution so active/passive `monster_skills.csv` display names take priority over choice and trigger-derived labels.

## Source: boards\UI\UI_BLACKBOARD.md

## Task: 2026-05-17 Surviving NewScene UI Flow

### Task title

Keep the active UI flow grounded in `NewMainMenu` plus `NewRunScene` only.

### Goals

- Preserve `Assets/Scenes/NewScene/NewMainMenu.unity` -> `Assets/Scenes/NewScene/NewRunScene.unity` as the surviving menu/run path.
- Preserve `Pakuri/Assets/Scripts2/UI/UIManager.cs` as the active menu-side flow owner.
- Keep older Legacy UI/controller retirement detail out of the active board while preserving it in the archive snapshot.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.

### Role Owner

Code Builder

### Status

Current active UI flow summarized and retained for future work.

### Next Actions

- Future menu/run flow work should update this file together with `boards/RUN/RUN_BLACKBOARD.md`.
- Use the archive snapshot when the deleted Legacy scene/controller cleanup history is actually needed.

### Evidence

- `Pakuri/Assets/Scripts2/UI/UIManager.cs` still owns the active menu flow and loads `Assets/Scenes/NewScene/NewRunScene.unity`.
- `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity` remains the current menu scene used by that flow.
- `Pakuri/ProjectSettings/EditorBuildSettings.asset` was recorded as containing only the kept `NewMainMenu.unity` and `NewRunScene.unity` scene paths.

### History

- 2026-05-14: `UIManager`-owned `NewMainMenu` flow binding became the retained baseline.
- 2026-05-17: Legacy scene/controller cleanup left only the new scene pair as the supported UI flow.

## Task: 2026-05-26 Floating Damage Text Multi-Popup Retention

### Task title

Keep each floating damage text visible for its own 1 second instead of replacing the previous number on the next hit.

### Goals

- Change the shared world-space damage text path so repeated hits create separate popup text instances.
- Keep the existing per-unit `Damage` TextMesh child as the template and anchor.
- Avoid requiring scene or prefab edits for existing monster and enemy units.

### Constraints

- Role Owner is Code Builder.
- The implementation is scoped to the shared `UnitActorView` damage popup helper.
- Unity Play Mode gameplay/visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that rapid repeated damage keeps previous numbers visible for about 1 second while new numbers appear separately.
- If the popup stacking spacing is visually too tight or too tall, tune `stackVerticalSpacing` on the shared popup component behavior.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs` now keeps `InGameDamageTextPopup` as a template manager and spawns separate cloned `TextMesh` popup objects per `Show(...)` call.
- `InGameDamageTextPopup` default duration is now `1f`, keeps up to `12` active popup instances per unit, offsets concurrent popups by `0.18f`, and destroys each clone after its own timer.
- Existing actor flow is unchanged: `MonsterUnitActor.ShowDamage(...)` and `EnemyUnitActor.ShowDamage(...)` still call the shared popup object.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity `validate_script` for `Assets/Scripts2/InGame/Units/UnitActorView.cs` passed with 0 errors and one validator warning about string concatenation in Update.
- Unity `refresh_unity` recovered after reconnect and reported the editor ready; console error read showed existing UnityEditor.Graphs and MCP client handler exception logs, not a compile error from this file.

### History

- 2026-05-26: User asked whether current damage text could remain for 1 second when another hit arrives, then approved Code Builder implementation using the shared popup-clone approach.

## Source: boards\UI\DAMAGE_METER_UI_HANDOFF.md

## Task: Undated Damage Meter UI Code Builder Handoff

### Task title

Implement the `NewRunScene` damage meter overlay from the authored `Canvas/DamageMeterUI` hierarchy.

### Goals

- Open `Canvas/DamageMeterUI` from `Canvas/DamageMeterUIBtn`, hide the button while the overlay is open, and restore the button when the overlay closes.
- Show active party damage rows in fixed 1P to 5P order.
- Keep 1P as the initially selected monster, then map 2P to 5P from `RunSession.ManifestedMonsterIds` order.
- Track each monster's actual round damage by display source, including basic skill damage and master/trigger/additional damage when those sources are distinguishable.
- Render total damage, leader-relative percent, total meter width, and per-source skill meter segments according to `Pakuri/reference/7.UI/8-1. damage-meter-overlay-layout.md`.
- Add `MonsterIconImage` to `Pakuri/Assets/CSVdata/source/monsters.csv` and use it for the panel `Image` when present.

### Constraints

- Role Owner is Code Builder.
- Designer does not implement code or scene changes.
- Unity Play Mode gameplay verification remains user-owned.
- Use actual applied health plus shield delta for meter totals; do not use unresolved base damage or overkill-inclusive raw final damage.
- Keep `InGameUIManager.cs` focused on existing reward, Offering, and Menifest flow. Do not put the damage meter implementation there unless Code Builder finds a direct scene-binding constraint.
- Blank `MonsterIconImage` values must be accepted and must leave the panel image unchanged or hidden without failing CSV validation.
- `Skill-Meter` RectTransform position and size authored in the scene are the template authority; cloned source segments should preserve the template layout basis and only adjust segment width/position as needed.

### Role Owner

Code Builder

### Status

Designer handoff created. Implementation not started.

### Selected track

Designer implementation handoff plus gameplay-facing feedback clarity.

### Evidence

- `AGENTS.md` and `MDTREE.md` require evidence-based work and minimal markdown routing.
- `AGENTS_ROLE/GAMEDESIGNER.md` says Designer does not implement code or scene changes.
- `boards/UI/RUNSCENE_UI.md` records `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` as the current top-level `NewRunScene` UI lookup/binding owner and Offering/Menifest flow owner.
- `boards/RUN/RUN_BLACKBOARD.md` records `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` as current `NewRunScene` runtime authority and `EffectManager` as current skill visual registry.
- `boards/DATA/DATA_BLACKBOARD.md` records `Pakuri/Assets/CSVdata/source/*.csv` and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.*` as active runtime CSV authority.
- Unity-MCP found `Canvas/DamageMeterUIBtn` with `Button` and `Canvas/DamageMeterUI` with overlay `Image`.
- Unity-MCP found `Canvas/DamageMeterUI/1PDamagePanel` through `5PDamagePanel`, each with authored panel children; `1PDamagePanel` contains `Image`, `Monster_Name_Text`, `Total_Damage`, `Total_Damage_Persent`, `MeterBG`, and `Skill-Meter/SkillName`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` exposes `ApplyDamage(... BaseUnitRuntimeModel source, ... string sourceSkillId ...)` and builds `DamageApplicationOptions` with `Source` and `SourceSkillId`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` returns `InGameResourceChangeResult` with `PreviousHealth`, `CurrentHealth`, `PreviousShield`, `CurrentShield`, and `AppliedDamage`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` currently uses `result.AppliedDamage` for damage popups, but that value is final calculated damage and may not equal actual resource delta when shields, low remaining health, or overkill are involved.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` stores `ManifestedMonsterIds`, appends manifested monsters in `RecordManifestedMonster`, and stores per-monster `PartyMembers` state.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` records a manifested monster, computes `slotIndex = Mathf.Clamp(session.ManifestedMonsterIds.Count, 1, 4)`, then calls `entryManager.SpawnManifestedMonster(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `monsters.csv display_name` into `MonsterDefinition.DisplayName`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `monster_skills.csv display_name` into `SkillDefinition.DisplayName`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `monster_skill_choices.csv title` into `SkillChoiceDefinition.Title`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/MonsterDefinition.cs` currently has `DisplayName`, `UnitSprite`, and `ProjectileSprite`, but no dedicated monster icon field.
- `Pakuri/Assets/CSVdata/source/monsters.csv` currently has no `MonsterIconImage` column in its inspected header.

### Relevant files and Unity objects

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity`
- `Canvas/DamageMeterUIBtn`
- `Canvas/DamageMeterUI`
- `Canvas/DamageMeterUI/Close`
- `Canvas/DamageMeterUI/1PDamagePanel` through `Canvas/DamageMeterUI/5PDamagePanel`
- `Canvas/DamageMeterUI/*PDamagePanel/Image`
- `Canvas/DamageMeterUI/*PDamagePanel/Monster_Name_Text`
- `Canvas/DamageMeterUI/*PDamagePanel/Total_Damage`
- `Canvas/DamageMeterUI/*PDamagePanel/Total_Damage_Persent`
- `Canvas/DamageMeterUI/*PDamagePanel/MeterBG`
- `Canvas/DamageMeterUI/*PDamagePanel/Skill-Meter`
- `Canvas/DamageMeterUI/*PDamagePanel/Skill-Meter/SkillName`
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs`
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/MonsterDefinition.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.*.cs`
- `Pakuri/Assets/CSVdata/source/monsters.csv`
- `Pakuri/Assets/CSVdata/source/monster_skills.csv`
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`

### Expected implementation surface

- Create a new runtime tracker, recommended name `DamageMeterRuntimeTracker`.
- Create a new UI controller, recommended name `DamageMeterUIController`.
- Add a serialized or auto-resolved reference from `DamageMeterUIController` to `InGameCombatManager` and the current run session source.
- Add a narrow call in `InGameCombatManager.ApplyDamage` immediately after `resourceMutations.ApplyDamage(...)` returns.
- Extend CSV source, row parsing, runtime model, asset reference collection, runtime catalog build, and validation for `MonsterIconImage`.

### Damage calculation ownership

`InGameCombatManager.ApplyDamage` is the correct event boundary. It already receives source unit and source skill id and already owns the resolved `InGameResourceChangeResult`.

The meter must calculate actual applied damage as:

```csharp
actualDamage =
    Mathf.Max(0f, result.PreviousHealth - result.CurrentHealth)
  + Mathf.Max(0f, result.PreviousShield - result.CurrentShield);
```

Only record when:

- `actualDamage > 0`
- `options.Source != null`
- source side is player/monster
- source monster id can be resolved from `options.Source.Identity.DefinitionId`

Do not record:

- healing
- shield grants
- rejected or zero-damage hits
- damage from enemies to player monsters
- damage prevented by invulnerability or missed/no-target paths

### Damage source identity

Basic implementation can use `sourceSkillId` and resolve display name from `monster_skills.csv display_name`.

Required implementation for this task should add a display-source layer so master or trigger damage can be separated from the base skill when runtime can identify it:

- Keep `sourceSkillId` for combat semantics.
- Add a meter-only source id, recommended `DamageMeterSourceId`.
- Add a meter-only display name, recommended `DamageMeterDisplayName`.
- Basic skill damage uses `sourceSkillId` and `SkillDefinition.DisplayName`.
- Choice/master-authored damage uses the `monster_skill_choices.csv title` when the triggering runtime knows the choice id.
- Trigger/effect-authored follow-up damage should pass a separate meter source id if it represents a separate displayed source, such as `vega-b-master1-second-slash`.

If Code Builder finds that a current executor passes only `sourceSkillId=vega-b` for a master-1 follow-up, then the UI cannot honestly display `침묵의 대태도 - 두번째 봉인` separately without adding meter-source metadata to that executor/trigger path.

### UI behavior

- On startup, `DamageMeterUI` should be hidden unless the scene intentionally ships it open for debug. If hidden at startup, `DamageMeterUIBtn` should be visible.
- `DamageMeterUIBtn.onClick` opens the overlay and disables/hides the button.
- `DamageMeterUI/Close.onClick` closes the overlay and re-enables/shows the button.
- Overlay opening does not pause combat.
- Open overlay refreshes immediately; while open, refresh at about `0.2` seconds or on dirty tracker events.
- Closing overlay does not reset accumulated round damage.
- Round damage resets when the next combat round starts, not when the overlay closes.

### Panel activation and party order

- Build display party list from current session:
  - index 0: selected monster id.
  - index 1 to 4: `RunSession.ManifestedMonsterIds` in existing list order.
- Bind `1PDamagePanel` to party index 0, `2PDamagePanel` to party index 1, and so on.
- Set unused panels inactive.
- Keep panel positions fixed; do not reorder panels by damage rank.
- If a monster disappears mid-combat, keep its current row visible for that combat if it was part of the session party.

### Text and meter formatting

- `Monster_Name_Text`: `MonsterDefinition.DisplayName`.
- `Total_Damage`: compact format from the layout doc.
- `Total_Damage_Persent`: leader-relative percent where top total is `100%`.
- If all totals are zero, show all active rows as `0`, `0%`, empty meter.
- Use comma formatting only where there is enough space; otherwise use compact `K`/`M`.
- Suggested compact examples: `999`, `1K`, `12.4K`, `968K`, `1.82M`.

### Skill meter rules

- Use the authored `Skill-Meter` object as the template.
- Clone one segment per nonzero meter source.
- Preserve template height, vertical position, and visual style.
- Segment width equals `monsterSourceDamage / monsterTotalDamage`.
- Segment x position is cumulative from left to right.
- `SkillName` should show the resolved display source name and compact damage value.
- Do not create a visible segment for zero-damage sources.
- Keep a stable source order:
  - base active skill order A to E first.
  - then active master or trigger sources in the order they first dealt damage.
  - then passive/additional sources in the order they first dealt damage.

### Monster icon data ownership

Add `MonsterIconImage` to `monsters.csv`.

Implementation requirements:

- Add header and type row entry, likely `asset_path`.
- Add parser row property in the monster source row model.
- Add asset-reference collection entry so the runtime asset catalog includes the sprite.
- Add `Sprite MonsterIconImage` or equivalent to `MonsterDefinition`.
- Map loaded sprite into `MonsterDefinition` during runtime catalog build.
- UI assigns it to `*PDamagePanel/Image`.
- Blank or unresolved value should leave the image blank/hidden and not crash.

### Edge cases

- Overkill: count only actual HP/shield removed.
- Shield: count shield damage and health damage together under the same source segment.
- Zero damage: do not grow segment; optional text row may show 0 only if UI has room.
- Additional outgoing status damage: if it reuses the original `sourceSkillId`, it will aggregate under that source unless Code Builder passes a distinct meter source.
- Triggered line/single/zone follow-ups: must pass distinct meter metadata when they should be displayed as separate lines.
- Enemy damage: excluded from this player-facing meter.
- Missing monster icon path: pass.
- Missing display name lookup: fallback to source id.
- Missing source id: fallback to `Unknown` only for debug; avoid visible production ambiguity if possible.

### Acceptance criteria

- Clicking `Canvas/DamageMeterUIBtn` opens `Canvas/DamageMeterUI` and hides/disables `DamageMeterUIBtn`.
- Clicking `Canvas/DamageMeterUI/Close` closes the overlay and restores `DamageMeterUIBtn`.
- Active 1P to 5P panels match selected plus manifested monster order, not damage rank.
- Unused damage panels are inactive.
- `Monster_Name_Text` shows `monsters.csv display_name` via `MonsterDefinition.DisplayName`.
- `Image` uses `MonsterIconImage` when present and safely passes when blank.
- `Total_Damage` equals actual applied health plus shield damage for the current round.
- `Total_Damage_Persent` uses highest party total as `100%`.
- Skill segments sum to the monster total and visually fill the same template meter width.
- Base skill source names come from `monster_skills.csv display_name`.
- Master/choice source names come from `monster_skill_choices.csv title` when the runtime provides that source metadata.
- Vega-B style follow-up damage can be displayed separately only if the trigger/effect path passes distinct meter-source metadata.

### Verification expected from Code Builder

- Unity-MCP scene inspection confirms `DamageMeterUIController` is attached and references or resolves `DamageMeterUIBtn`, `DamageMeterUI`, `Close`, and 1P to 5P panels.
- CSV field-count validation passes after adding `MonsterIconImage`.
- Unity `Pakuri/Validate CSV Source Data` passes.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` passes after icon asset paths are added.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passes.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passes.
- Unity console read shows no new C# compile errors or CSV runtime failures.
- User performs Play Mode gameplay verification for live combat numbers and visual fit.

### Related board files that must be updated

- `boards/UI/RUNSCENE_UI.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- `boards/RUN/RUN_BLACKBOARD.md`

### Next Actions

- No active action remains in this archived handoff. The implementation result is recorded in `boards/UI/RUNSCENE_UI.md` under the 2026-05-29 damage meter task.

### History

- 2026-05-29: Designer inspected current scene objects, current damage application path, current run Menifest order, current CSV build mappings, and created this Code Builder handoff.
