# Projectile Blueprint For InGame Skills

## Purpose

This document is the first-read implementation contract for InGame projectile skills.

It is not a replacement for code inspection. Its role is to reduce the amount of code an AI or Code Builder must inspect before implementing another projectile skill. New projectile work should start from this document, then verify the listed files and the specific skill data being changed.

## Numeric Evidence Priority

Do not invent skill or enemy numbers when the user does not provide exact numeric evidence.

When a projectile or enemy-related implementation needs numeric values and the user has not supplied the exact source, inspect evidence in this order:

1. Active CSV data first.
   - Skill values: `Pakuri/Assets/CSVdata/SkillData.csv`
   - Enemy values: `Pakuri/Assets/CSVdata/EnemyStat.csv`

2. Runtime source CSV data next, when the active CSV file does not contain the needed row or field.
   - Monster skill runtime source: `Pakuri/Assets/CSVdata/source/monster_skills.csv`
   - Monster skill choice runtime source: `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`
   - Monster reward choice runtime source: `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv`

3. Original reference documents last, when CSV data does not contain the needed numeric value.
   - Monster original data: `Pakuri/reference/2.Monster`
   - Enemy original data: `Pakuri/reference/5.enemy`

If none of these files contains the value, state that the value is missing and ask for a design decision or add an explicit placeholder only when the user approves it. Record which file supplied each non-obvious tuning value, especially when a runtime mapper needs a skill-ID-specific exception because the current CSV schema has no matching field.

## Current Common Projectile Path

The current InGame projectile path is the `Scripts2/InGame` path.

1. Data defines the skill.
   - `Pakuri/Assets/CSVData/SkillData.csv`
   - `Pakuri/Assets/CSVdata/source/monster_skills.csv`
   - `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv`

2. Skill data is mapped into runtime data.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`
   - `SkillRuntimeKind.MagazineProjectile` and `SkillRuntimeKind.CooldownProjectile` map to `ProjectileSkillData`.
   - Projectile fields currently mapped include magazine size, reload time, projectile speed, damage, and on-hit status.

3. Learned active skills become runtime instances.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeFactory.cs`
   - `SkillRuntimeFactory.RebuildLearnedActiveSet(...)` adds learned active skills to `UnitSkillRuntimeSet`.

4. Runtime gating checks whether the projectile can fire.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs`
   - `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs`
   - `CanCastWithSnapshot(...)` checks cooldown, cast state, reload state, magazine, and shot interval.
   - `TryBeginCast(snapshot)` consumes one magazine shot and starts cooldown / interval / reload state.

5. The projectile executor creates projectile objects.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutorRegistry.cs`
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs`
   - `ProjectileSkillExecutor` resolves target direction, damage, prefab, status spec, branch spec, projectile count, pierce, lifetime, and destroy boundary.

6. Projectile GameObjects move and hit targets.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs`
   - The actor moves every `Update()`.
   - It checks trigger collision and roster-distance fallback hits.
   - On hit it applies damage, then status, then branch projectiles.
   - It decrements remaining hits and destroys itself when pierce capacity is consumed.

7. Combat manager owns shared combat APIs.
   - `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`
   - Relevant APIs: `ApplyDamage(...)`, `ApplyStatus(...)`, `ResolveSkillEffectPrefab(...)`, `ResolveProjectileDestroyBoundaryX()`, `InstantiateSkillPrefab(...)`.

## Supported / Partial / Unsupported Matrix

| Behavior | Current status | Evidence / implementation note |
|---|---|---|
| Straight projectile movement | Supported | `InGameProjectileActor.Update()` moves by direction and speed. |
| Projectile prefab instantiation | Supported | `ProjectileSkillExecutor` calls `InGameCombatManager.InstantiateSkillPrefab(...)`. |
| Skill-id prefab fallback | Supported | `InGameCombatManager.ResolveSkillEffectPrefab("eve-a")` returns `eveAProjectilePrefab`. |
| Magazine | Supported | `SkillRuntimeInstance` owns magazine size, magazine remaining, and reload start. |
| Reload | Supported | `SkillRuntimeInstance` ticks `ReloadRemaining` and refills magazine. |
| Shot interval / fire interval | Supported | `SkillRuntimeInstance` uses `TickRemaining` as cast interval. |
| Base damage + stat coefficient | Supported | `SkillExecutionUtility.ResolveDamage(...)` uses base damage, stat source, coefficient, and snapshot multiplier. |
| Element / damage attribute | Supported | `SkillExecutionUtility.MapAttribute(...)` maps skill element to `DamageAttribute`. |
| Pierce count | Supported | `ProjectileSkillExecutor` adds snapshot pierce bonus; `InGameProjectileActor` stores `remainingHits = pierce + 1`. |
| Additional projectile count | Supported | `ProjectileSkillExecutor` adds `snapshot.AdditionalProjectileBonus` and spreads shots by angle. |
| Basic fan spread | Supported | Multi-projectile shots use a fixed 10 degree step in `ResolveProjectileSpreadDirection(...)`. |
| On-hit status application | Supported | `ProjectileStatusHitSpec` and `InGameProjectileActor.TryApplyStatus(...)` call `InGameCombatManager.ApplyStatus(...)`. |
| Status stacks / max stacks / duration | Supported | `StatusEffectKind` and `UnitStatusRuntimeSet` own normalized status kind, duration, and stack behavior. |
| Status label display | Supported | `MonsterUnitActor` and `EnemyUnitActor` append `StatusEffectUtility.BuildDisplaySuffix(...)`. |
| Choice modifier damage / magazine / reload / shot interval / pierce / additional projectiles | Supported | `SkillChoiceResolver` and `SkillExecutionSnapshot` apply matching modifier records. |
| Eve-A style branch projectile | Partial | `ProjectileBranchHitSpec` can spawn branch projectiles on hit, but only as the current on-hit branch pattern. Branch projectiles currently do not inherit status or branch specs. |
| Manual 1P A-skill aim | Partial | `InGameCombatManager.HandleSelectedPlayerPrimarySkillInput()` routes selected player slot A by mouse direction when auto is off. This is currently a selected-primary path, not a general per-skill input system. |
| Auto target nearest enemy | Partial | `SkillExecutionUtility.FindNearestTarget(...)` finds nearest target from roster. It does not implement priority rules beyond nearest. |
| Trigger hit detection | Partial | `InGameProjectileActor` supports trigger collision, but also uses roster-distance fallback because not all targets may have final collider contracts. |
| Projectile lifetime / destroy boundary | Partial | Lifetime is derived from a fixed battlefield travel distance of `31f`; boundary uses scene transform or fallback X. Non-horizontal or special path projectiles may need a different contract. |
| Critical hit | Unsupported in current projectile executor | Data can say critical allowed, but inspected current `ResolveDamage(...)` does not roll critical damage. |
| Bounce / ricochet | Unsupported | No current bounce target selection or bounce counter exists in `InGameProjectileActor`. |
| Homing / guided projectile | Unsupported | Current movement is fixed direction * speed. No target tracking is present. |
| Installed / trap / placed projectile | Unsupported | Current projectile starts at caster or hit origin and moves. It does not support stationary traps or delayed armed projectiles. |
| Multi-hitbox projectile | Unsupported | Current actor owns one hit radius / collider path. Complex multiple hitboxes need an explicit new actor or child relay contract. |
| Timed 3-shot sequence like Vega-A | Unsupported as common behavior | Additional projectiles are simultaneous fan shots. Timed or queued burst sequences need explicit exception or a new reusable burst scheduler. |
| Per-projectile mark systems | Unsupported as common behavior | No generic projectile mark payload exists in the current InGame projectile actor. |
| Impact area / explosion after projectile hit | Unsupported in current executor | `ProjectileSkillData` has impact fields, but inspected `ProjectileSkillExecutor` does not execute impact area damage/status yet. |
| Status-driven passive damage bonuses | Unsupported in current damage service | Status can be applied and queried, but `UnitResourceMutationService.ApplyDamage(...)` does not currently read status stacks for damage amplification. |

## Special Behavior Rule

Do not assume special projectile behavior is supported just because it is described in a monster reference file or CSV row.

The following behavior must be implemented as an explicit exception or as a deliberate reusable extension before a skill can rely on it:

- Vega-A style timed three-projectile sequence.
- Branch lightning variants beyond the current Eve-A style on-hit branch spec.
- Bounce / ricochet projectiles.
- Homing or target-following projectiles.
- Installed, trap, delayed, or stationary projectile objects.
- Multi-hitbox projectile prefabs.
- Per-projectile mark payloads, such as a projectile carrying a monster-specific mark stack.
- Projectile impact area / explosion behavior.

If a special behavior will be reused by several skills, prefer a new shared extension point rather than a monster-only hardcoded branch. If the behavior is unique and urgent, record it as a deliberate exception with the owning skill ID, affected files, and Play Mode acceptance criteria.

## New Projectile Skill Checklist

Before implementing a new projectile skill:

1. Confirm the skill row exists in data.
   - Check `Pakuri/Assets/CSVdata/SkillData.csv` first for active skill data.
   - If the active CSV has no row or no needed field, check `Pakuri/Assets/CSVdata/source/monster_skills.csv`, which is still the current runtime source bridge for monster skill definitions.
   - If CSV data still lacks the needed value, inspect the matching original file under `Pakuri/reference/2.Monster/{monster}/skill`.
   - Confirm runtime kind is projectile-compatible.

2. Confirm the mapped runtime type.
   - `MagazineProjectile` and `CooldownProjectile` should map to `ProjectileSkillData`.
   - If it maps to `ZoneSkillData`, `BeamSkillData`, `ShieldSkillData`, or `BuffSkillData`, do not implement it through the projectile actor without a design decision.

3. Confirm prefab ownership.
   - Check whether the skill has `SkillEffectPrefab` / projectile prefab data.
   - If data does not provide the prefab, check `InGameCombatManager.ResolveSkillEffectPrefab(...)`.
   - If neither path provides a prefab, current executor falls back to direct target damage only when a target exists.

4. Confirm status behavior.
   - Supported statuses are centralized in `StatusEffectKind`.
   - Add new status kinds there before relying on new runtime status names.
   - Current status application supports apply, stack, duration, expiry, and label display.
   - Do not claim status-driven damage bonuses exist unless `UnitResourceMutationService` or an equivalent damage layer implements them.

5. Confirm modifier behavior.
   - Use `SkillChoiceModifierData.csv` for common numeric behavior.
   - Supported common modifier fields include damage multiplier, base damage bonus, magazine bonus, pierce bonus, additional projectile bonus, reload multiplier, shot interval multiplier, branch fields, and status stack fields.
   - Modifier rows apply only when the chosen choice ID belongs to the current skill's enhancement or master choices.

6. Decide whether the skill is common or exceptional.
   - Common: straight/fan projectile with optional pierce, status, branch, magazine, reload, and numeric modifiers.
   - Exceptional: timed burst, homing, bounce, trap/install, multi-hitbox, mark payload, impact area, or custom target behavior.

## Recommended Extension Points

Use these extension points when common behavior is not enough:

| Need | Recommended direction |
|---|---|
| Timed burst sequence | Add a reusable burst scheduler before implementing Vega-A-like behavior in more than one skill. |
| Homing projectile | Add target retention and steering to a new projectile behavior mode, not to every skill executor. |
| Bounce / ricochet | Add bounce count, target exclusion, and next-target selection as a separate projectile hit behavior. |
| Installed/trap projectile | Add a stationary actor or mode with arming delay, duration, trigger radius, and duplicate-hit policy. |
| Multi-hitbox | Add a child hitbox relay contract that reports to one owning projectile runtime. |
| Impact area | Implement `ProjectileSkillData.ImpactArea`, `ImpactDamage`, and `ImpactStatus` in `ProjectileSkillExecutor` / hit handling. |
| Mark payload | Add an explicit projectile payload structure instead of encoding monster-specific marks in unrelated status fields. |

## Eve-A Current Evidence Summary

Eve-A is the current reference implementation for the common projectile path.

- `SkillData.csv` defines `eve-a` as `ProjectileSkillData` / `MagazineProjectile`, with lightning damage, base damage 24, spell coefficient 0.95, shot interval 0.35, magazine 6, reload 4, status `감전`, chance 0.15, and stacks 1.
- `monster_skills.csv` defines `eve-a` as `MagazineProjectile`, `RuntimeImplemented`, and `status_effect_id=shock`.
- `InGameCombatManager.ResolveSkillEffectPrefab("eve-a")` returns `eveAProjectilePrefab`.
- `NewRunScene.unity` assigns `eveAProjectilePrefab` to `Assets/Prefab/Skill/Eve/Eve_A.prefab`.
- `Eve_A.prefab` has a trigger `BoxCollider2D` and `Pakuri.InGame.InGameProjectileActor`.
- On hit, `InGameProjectileActor` applies damage, then status, then branch projectiles.
- Eve-A shock is currently applied through the shared status path.

## Verification Expected From Code Builder

For documentation-only changes:

- Run a targeted markdown/file existence check.
- Do not run Play Mode.

For code changes implementing projectile behavior:

- Run `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`.
- Run `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` when editor scripts or Unity serialization may be affected.
- Refresh Unity scripts if Unity is available, then check console errors/warnings.
- Record which projectile behavior is common, partial, or exceptional.
- Leave Play Mode gameplay verification to the user.
