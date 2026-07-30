# Skill Cast Finalization / Actor Application Handoff

## Task title

Finalize cast-fixed skill values before delivery and reduce Actors to runtime application.

## Goals

- Make `SkillExecution` the only family dispatch and cast-finalization entry.
- Make `SkillExecutionData` carry resolved cast values and deployment plans.
- Make spatial Executors consume prepared plans without interpreting Definitions.
- Make Actors consume prepared values, perform hit-time targeting/collision, apply gameplay, publish lifecycle events, and request deletion.
- Move misplaced shared logic into existing `SkillTargeting`, `SkillStatus`, `SkillExecutionRuleResolver`, and `EffectVisualBuilder` paths.
- Delete copied calculations, cross-family Actor dependencies, and empty execution-coordinator behavior.

## Constraints

- Preserve skill IDs, CSV values, Definition fields, assets, target policy, damage, status, critical, Trigger, repeat, follow-up, recast, visual, prefab, and timing behavior.
- Preserve hit-time decisions: target survival, collision, health/status conditions, resistance, execute result, random on-hit/branch chances, death, recovery, consumption, and redistribution.
- Preserve Actor-owned normal lifetime and `EffectManager.RemoveEffect` request.
- Keep Buff as the no-spatial-gameplay-Actor exception. `BuffSkillActor` remains visual/status lifetime only.
- Keep Charge on `SkillUseState`, ordinary enemy AI/movement, and `UnitCollisionResolver`.
- Add no production script, family snapshot class, base Actor, Actor interface, factory, or standalone contract file.
- Reuse existing algorithms by moving them; do not copy them into new paths.
- Unity Play Mode verification remains user-owned.

## Role Owner

Code Builder for implementation. Code Reviewer for one final review pass explicitly requested by the user.

## Status

Code Builder implementation and non-Play-Mode verification complete. Code Reviewer inspection pending.

## Next Actions

1. Run the user-authorized Code Reviewer pass.
2. Fix concrete findings, if any, without adding parallel abstractions.
3. Repeat static, build, and focused verification after any reviewer fix.
4. Leave Play Mode gameplay verification to the user.

## Inspected evidence

- `SkillExecutionState.BuildExecutionData` currently applies Definition Nodes, passives, enhancements, masters, and dynamic choices but does not finalize family values.
- `SkillExecutionData` currently stores modifiers such as `DamageMultiplier`, `RadiusMultiplier`, and `DurationMultiplier`.
- `SingleSkillActor`, `ProjectileSkillActor`, `LineSkillActor`, and `ZoneSkillActor` currently receive concrete Definitions and calculate cast-fixed values.
- `SkillTrigger.ExecuteCommand` currently creates Zone snapshots and calls `ZoneSkillExecutor.Execute` directly.
- `ProjectileStatusHitSpec` is declared in `ProjectileSkillActor.cs` but used by Single, Line, Zone, `SkillStatus`, and Status execution.
- Projectile impact currently calls `ZoneSkillActor.ApplyAreaTick`.
- Line, Zone, and Projectile repeat the same conditional-damage, `ApplyDamage`, status, and hit-enhancement sequence.
- Buff Executor currently calculates Heal and Shield values instead of receiving finalized amounts.
- Runtime and Editor baseline builds pass with zero errors and the two existing assembly-reference warnings.
- Baseline checkpoint commit: `c9303c4`.

## Responsibility boundaries

### Definitions

- Own authoring contracts only.
- Must not calculate runtime values, select targets, mutate combat state, or manage objects.

### SkillExecution

- Validate cast and construct context.
- Apply Definition, Nodes, passive, choice, Trigger override, and family formulas once.
- Prepare centers, directions, launch indices, delays, counts, geometry, damage/status, prefab, visual, and lifecycle policy.
- Dispatch only through one family path.
- Must not perform hit-time collision or gameplay application.

### SkillExecutionData

- Store modifier inputs until finalization.
- Store finalized values and prepared deployment collections after finalization.
- Must not select or mutate targets, start coroutines, create objects, or own lifetime.

### Executors

- Consume prepared deployment entries.
- Create visual, prefab, or empty Actor object through `EffectManager`.
- Attach the matching Actor and pass ready values.
- Must not inspect a concrete Definition, calculate values, choose targets, apply gameplay, publish hit events, or own coroutines.

### Actors

- Wait for prepared delays and own movement/tick/lifetime.
- Resolve actual hit-time targets through `SkillTargeting`.
- Resolve collider hits through `UnitCollisionResolver`.
- Call shared hit/status rules and family-only post-hit rules.
- Publish actual gameplay lifecycle events.
- Request `EffectManager` removal.
- Must not receive or interpret concrete Definitions or calculate cast-fixed plans.

### Shared helpers

- `SkillTargeting`: all deterministic target filtering, ordering, centers, directions, radii, and Buff radius filtering.
- `SkillStatus`: resolved generic status application data.
- `SkillExecutionRuleResolver`: shared conditional hit calculation and shared hit-application sequence.
- `SingleSkillRules`: Single-only hit-time execute, boss, status-stack, recovery, consumption, and redistribution rules.
- `EffectVisualBuilder`: visual rotation, scaling, renderer, hitbox, line, zone, and branch-line configuration.
- `EffectManager`: create, track, delete on Actor request, and forced cleanup only.

## Consolidation and deletion targets

- Delete Actor `DamageCalculator.CalculateRawDamage` and `SkillStatus.StatusSpec` calls after moving them to cast finalization.
- Delete Actor radius, duration, tick, width, length, count, delay, speed, lifetime, direction, and center formula helpers.
- Delete Trigger direct `ZoneSkillExecutor.Execute` call.
- Move and rename `ProjectileStatusHitSpec` under `SkillStatus.cs`; delete its Projectile declaration.
- Flatten `ProjectileBranchDamageSpec` into finalized `SkillExecutionData`; delete the class.
- Move shared area-hit behavior out of `ZoneSkillActor`; delete Projectile-to-Zone Actor dependency.
- Move Buff configured-radius filtering into `SkillTargeting`; delete local filtering algorithm.
- Merge `SkillRequirement.HasSourceStatus` into its only caller and remove dead `HasLearnedPassive`.
- Move Actor presentation configuration into `EffectVisualBuilder`.
- Delete one-line duplicated status wrappers.
- Remove same-file `partial` declarations when Actor planning sections disappear.
- Remove empty execution coordinator state when prepared entries can initialize concrete Actors directly.

## Family prepared data

### Projectile

- Origin, directions, launch delays, projectile indices, speed, lifetime, boundary, pierce, damage, status, critical values, impact plan, branch values, and follow-up values.

### Line

- Origins, directions, repeat delays, width, length, duration, tick interval, damage, status, critical values, knockback, visual, and prefab.

### Single

- Centers, repeat delays, radius, hit cap, prefab-hitbox mode, damage delay, damage, status, critical values, visual/prefab, and follow-up values.

### Zone

- Centers, radius, cover-all mode, duration, tick interval, target cap, damage, status, critical values, visual/prefab, recast generation, and expiry policy.

### Buff

- Resolved status, Heal amount, Shield amount/duration/status, Charge contact ratio/status, target policy, and visual lifetime.

## Compatibility risks

- Delayed Single target selection must remain at hit time while its center plan remains at the current cast-time calculation point.
- Projectile burst order, last-projectile Trigger, branch chance timing, impact delay, and pierce count must remain unchanged.
- Line base status must remain once per target while damage may tick repeatedly.
- Zone status may apply on each valid tick as before.
- Trigger-generated skills must retain source skill identity and lifecycle suppression.
- No-visual skills must retain the same gameplay path through empty Actor objects.
- Recast snapshot inheritance and generation limits must remain unchanged.

## Acceptance criteria

- Spatial Actor files contain zero concrete family Definition references.
- Spatial Actor files contain zero `DamageCalculator.CalculateRawDamage`, `SkillStatus.StatusSpec`, and cast-geometry calculation helpers.
- Spatial Executors contain zero concrete Definition interpretation, targeting, damage/status application, Trigger publication, or coroutine code.
- Direct family Executor calls exist only in `SkillExecution`.
- Projectile contains zero `ZoneSkillActor` calls.
- Generic status runtime data is not declared in a family Actor.
- One shared conditional hit/application path remains.
- Buff targeting uses `SkillTargeting`; no local radius scan remains.
- Every normal Actor completion requests deletion from `EffectManager`.
- No production script, base Actor, interface, or factory is added.
- `git diff --check` passes.
- Runtime and Editor builds complete with zero errors.
- Unity compilation and Console contain zero project errors.
- Focused and full available EditMode tests pass.
- User retains Play Mode gameplay verification.

## Verification

- Static forbidden-symbol searches by family.
- Direct Executor call and cross-family Actor dependency searches.
- Actor and shared helper line/method count comparison against commit `c9303c4`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false`.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false`.
- Unity script refresh, Console inspection, focused EditMode tests, and full EditMode tests through Unity-MCP.
- One Code Reviewer pass after Builder verification.

## Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/SkillExecutionData.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRuleResolver.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillStatus.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTargeting.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Reactions/SkillTrigger.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/`
- `Pakuri/Assets/Scripts/Combat/Effects/EffectManager.cs`
- `Pakuri/Assets/Scripts/Combat/Effects/EffectVisualBuilder.cs`
- `Pakuri/Assets/Scripts/Units/Collision/UnitCollisionResolver.cs`
- Family commits: `8698375`, `4db7aef`, `fac4762`, `f6a9e33`, `95dbf2a`.
- Shared-consolidation commits: `eae8a74`, `a59a415`, `befb722`.
- Spatial Actor/Executor forbidden-symbol searches returned zero Definition, cast-calculation, and cross-family Actor matches.
- `ProjectileBranchDamageSpec` and the Projectile-local generic status declaration are removed.
- `SkillExecutionRuleResolver` now owns the shared area/resolved hit application and Projectile launch-time branch/boundary rule paths.
- `SkillTargeting` owns Buff target and configured-radius filtering; `SkillRequirement` and dead `HasLearnedPassive` are removed.
- Runtime and Editor builds complete with zero errors and the two existing MCP assembly-reference warnings.
- Unity script compilation is idle and contains no project compile error.
- Focused Charge EditMode test passes 1/1; full EditMode tests pass 11/11.
- Unity Play Mode was not entered.

## History

- 2026-07-31: User rejected Actor-side cast-fixed calculation and required `SkillExecution`/`SkillExecutionData` finalization.
- 2026-07-31: User required consolidation by moving and deleting existing logic rather than copying classes or adding parallel algorithms.
- 2026-07-31: User explicitly assigned Code Builder implementation, intermediate Git commits, and one final Code Reviewer pass.
- 2026-07-31: Code Builder verified and committed baseline state as `c9303c4`.
- 2026-07-31: Code Builder moved cast-fixed values and plans for Line, Zone, Projectile, Single, and Buff into `SkillExecution`/`SkillExecutionData`.
- 2026-07-31: Code Builder consolidated shared hit application, targeting, status contracts, visual configuration, Charge active snapshots, and Projectile branch rules in existing paths.
- 2026-07-31: Runtime/Editor builds passed with zero errors; Unity focused and full EditMode tests passed.
