# ENEMY_BLACKBOARD

This is the active enemy-domain persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archive Note

- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

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
