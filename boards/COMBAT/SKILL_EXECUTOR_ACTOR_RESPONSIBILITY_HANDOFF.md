# Skill Executor / Actor Responsibility Handoff

## Task title

Unify skill execution as `SkillExecution -> Executor -> Actor -> EffectManager`.

## Goals

- Make `SkillExecution` the only cast validation, context construction, snapshot preparation, and family dispatch entry.
- Make `SkillExecutionData` contain typed, cast-fixed values before an Executor receives it.
- Make each spatial family Executor launch only its family Actor.
- Make each spatial family Actor own targeting at the required timing, collision, hit judgment, effect application, Trigger publication, and lifetime completion.
- Move duplicated cross-family hit enhancement logic to the existing `SkillExecutionRuleResolver`.
- Keep Buff as the explicit no-spatial-gameplay-Actor exception while retaining `BuffSkillActor` for Buff visual lifetime.
- Remove `EffectManager`'s automatic family-Actor selection. Each family explicitly attaches its own Actor.
- Add no base Actor, interface, factory, or standalone contract script.

## Constraints

- Preserve current skill IDs, CSV values, Definition fields, targeting policy, damage, status, critical, Trigger, repeat, follow-up, recast, visual, prefab, and timing behavior.
- Executors do not validate casts, select targets, calculate combat values, interpret modifiers, apply gameplay effects, publish hit events, or own coroutines.
- Actors do not commit cast cooldown, magazine, reload, or active-duration cost and do not decide whether a cast may begin. Hit-result refunds/reductions remain valid Actor-side effects through existing rules.
- A target's current health, status, collision, death, and resistance remain hit-time Actor judgments; they cannot be frozen as cast-fixed snapshot values.
- Trigger delay and repeat remain owned by `SkillTrigger`; delivery delay and repeat after family launch belong to the family Actor.
- Every family Actor owns its normal visual/gameplay lifetime and requests deletion from `EffectManager` when complete.
- `EffectManager` owns object creation, attachment, tracking, deletion, and forced combat-reset cleanup. It does not count down normal lifetimes or decide which family Actor to attach.
- `Charge` remains the current `SkillUseState` plus `EnemyCombatDecision`/`EnemyActionController` path. Do not recreate a Charge Actor or Charge State.
- Do not add `SkillSpecs.cs`, `StatusContracts.cs`, `RuntimeSkillVisualSpec.cs`, `SkillTriggerContracts.cs`, an Actor base class, an Actor interface, or a delivery factory.
- Preserve Unity `.meta` GUIDs for moved or renamed scripts.
- Unity Play Mode gameplay verification remains user-owned.

## Role Owner

Designer for this handoff. Code Builder for implementation after explicit role assignment.

## Status

Code implementation and available non-Play-Mode verification complete.

## Implementation result

- `SingleSkillExecutor`, `ProjectileSkillExecutor`, `LineSkillExecutor`, and `ZoneSkillExecutor` now only create an empty execution Actor, attach the matching family Actor, pass `SkillExecutionContext` plus `SkillExecutionData`, and return launch success.
- Family targeting, Definition/snapshot value resolution, delivery delay/repeat, collision, damage/status application, Trigger publication, and completion now reside in the matching `*SkillActor.cs`.
- Executors receive typed `SkillExecutionData` and do not inspect its fields. Actors combine that typed snapshot with the family Definition during initialization because precomputing every family scalar inside the shared snapshot would duplicate family rules in `SkillExecutionData`.
- All family target selection continues through the existing `SkillTargeting`; Collider judgment continues through `UnitCollisionResolver`.
- The four copied hit-enhancement implementations were deleted. `SkillExecutionRuleResolver.ApplyHitEnhancements` is the only implementation.
- `EffectManager` no longer selects or times family Actors. Buff/status and spatial Actors request deletion when their owned lifetime ends.
- Zone recast validation and inherited-snapshot choice moved to `SkillTrigger`; recast reaches the normal `ZoneSkillExecutor.Execute` entry.
- Projectile no-visual direct-hit bypass and timer-only `LineSkillActor` reuse were removed.
- `BuffSkillExecutors.cs` was renamed to `BuffSkillExecutor.cs`; GUID `210c9a9da090fa545801a1d1fb30c1ed` was preserved.

## Baseline inspected evidence

- `SkillExecution.ExecutePrepared` currently performs cast checks, creates `SkillExecutionContext`, publishes cast lifecycle events, and dispatches one concrete Definition to a family Executor.
- `SkillExecutionState.BuildExecutionData` creates `SkillExecutionData`, applies base Definition Nodes, passives, enhancements, masters, and dynamic choice rules.
- `SkillExecutionData` is currently a typed modifier snapshot, but several final cast values are still calculated inside family Executors.
- `ProjectileSkillExecutor`, `LineSkillExecutor`, `SingleSkillExecutor`, and `ZoneSkillExecutor` each contain an `ApplyHitEnhancements` implementation. Projectile, Line, and Zone copies are line-for-line equal in the inspected range; Single differs only in lifecycle-context policy and formatting.
- `SingleSkillExecutor` currently owns area-center calculation, target selection, prefab collision, delayed hit coroutines, damage, status, hit count, follow-up, kill recovery, and Trigger publication.
- `SingleSkillActor` currently owns only target following and visual lifetime removal.
- `ProjectileSkillActor`, `LineSkillActor`, and `ZoneSkillActor` already own collision/tick damage, status application, and effect removal, so they are closer to the target boundary.
- `ProjectileSkillExecutor` still contains a no-Actor direct-hit fallback and follow-up coroutines.
- `LineSkillExecutor` still resolves targets/directions, calculates line values, and owns repeated-cast coroutines.
- `ZoneSkillExecutor` still calculates centers/radius/duration/ticks/damage and owns a separate recast path.
- `BuffSkillExecutor` directly applies Status, Heal, Shield, and Charge contact behavior. Buff has no spatial collision lifecycle.
- `EffectManager.CreateEffect` currently attaches `BuffSkillActor` to temporary target visuals and `SingleSkillActor` to other timed temporary visuals.
- `EffectCreateRequest.DurationSeconds` is currently consumed only by those two automatic Actor-attachment branches.
- `EffectManager.CreateObject` already supports a no-visual Actor through `EffectCreateRequest.CreateEmptyActor`.
- Persistent status visuals currently bypass `BuffSkillActor`: `InGameCombatManager` calls `EffectManager.RemoveEffect(null, status)` when a status expires, is consumed, or is removed.
- `ProjectileSkillActor.SpawnBranchDamageLine` currently attaches `LineSkillActor` only to time a Projectile-owned branch visual even though `ProjectileSkillActor` already has `InitializeVisualLifetime`.

## Corrections and problems found

1. The first handoff revision incorrectly moved normal visual countdown into `EffectManager`. This violated the requested Actor lifetime boundary and has been removed.
2. The first revision incorrectly deleted `BuffSkillActor`. Temporary Buff visuals and persistent status visuals still need an Actor completion owner, so the script remains.
3. Current `EffectManager.CreateEffect` chooses `BuffSkillActor` or `SingleSkillActor` from request shape. Object creation should not decide delivery family; explicit caller attachment replaces this.
4. Current persistent status expiration directly calls `EffectManager.RemoveEffect`. Normal status completion must signal `BuffSkillActor`, which then requests deletion.
5. Current Projectile branch-line visual uses `LineSkillActor` as a generic timer. It should use `ProjectileSkillActor.InitializeVisualLifetime` because the visual belongs to Projectile behavior.
6. Buff has no gameplay Actor, so it is the targeting exception: `BuffSkillExecutor` calls the existing `SkillTargeting` immediately. Moving Buff family targeting into `SkillExecution` would mix family behavior into the common dispatcher.

## Core decision

```text
SkillExecution
  1. validate cast
  2. build context
  3. build/finalize SkillExecutionData
  4. dispatch family
        |
        v
Family Executor
  5. create effect object or empty object
  6. attach and initialize family Actor
        |
        v
Family Actor
  7. wait/move/tick
  8. call SkillTargeting or UnitCollisionResolver at required timing
  9. judge target-dependent rules
 10. apply damage/status/knockback
 11. publish OnHit/OnHitCount/OnExpire
 12. request EffectManager removal
```

Buff has no spatial Actor:

```text
SkillExecution
  -> finalized Buff snapshot and immediate targets
  -> BuffSkillExecutor
     -> delegate Status/Heal/Shield application to existing combat APIs
     -> ask EffectManager for optional visual object
     -> attach BuffSkillActor
        -> own temporary visual lifetime
        -> request EffectManager deletion
```

Persistent Buff/status visual:

```text
StatusRuntimeInstance ends
  -> InGameCombatManager reports status end
  -> EffectManager resolves the tracked visual and signals BuffSkillActor
  -> BuffSkillActor completes
  -> BuffSkillActor requests EffectManager.RemoveEffect
```

`EffectManager.ClearEffects` remains a forced combat-reset/scene-cleanup path and may remove tracked objects directly. It is not a normal skill lifetime path.

Charge remains outside Delivery Actor lifetime:

```text
BuffSkillExecutor starts active SkillUseState
  -> EnemyCombatDecision chooses ordinary nearest player/Nexus
  -> EnemyActionController moves
  -> UnitCollisionResolver detects contact
  -> BuffSkillExecutor.ApplyChargeContact applies final contact effect
```

## Snapshot boundary

### Cast-fixed data

`SkillExecutionData` must contain these values before family dispatch:

- Definition Nodes, passive, enhancement, master, and Trigger dynamic overrides already converted to typed values.
- Final base damage input and damage multiplier.
- Final radius, duration, tick interval, width, length, knockback, speed, pierce, projectile count, deployment count, repeat count, and delay values.
- Final critical permission and cast-fixed critical bonuses.
- Final status application data.
- Final visual/prefab selection.
- Source skill identity and lifecycle-publication policy.

The implementation extends the existing `SkillExecutionData`; it does not add family snapshot scripts.

Targeting algorithms remain in the existing `SkillTargeting.cs`. Spatial Actors call it at their required hit/tick timing. `BuffSkillExecutor` calls it synchronously because Buff has no gameplay Actor. No family implements a second target scan/order algorithm.

### Hit-time data

These values stay unresolved until Actor judgment:

- Which targets are alive and selectable at the actual hit or tick time.
- Collider overlap or swept collision result.
- Target health threshold, status stacks, boss condition, resistance, and execute state.
- Random hit/status/branch chances that are defined to occur on hit.
- Final applied damage result, death, kill recovery, status consumption, and redistribution.

Reading these values is gameplay judgment, not Executor-side Snapshot interpretation.

## Final folder structure

```text
Pakuri/Assets/Scripts/Combat/
├─ Effects/
│  ├─ EffectManager.cs
│  └─ EffectVisualBuilder.cs
└─ Skills/
   ├─ Definitions/
   │  ├─ SkillDefinition.cs
   │  ├─ Buff/BuffSkillDefinition.cs
   │  ├─ Choice/SkillChoice.cs
   │  ├─ Line/LineSkillDefinition.cs
   │  ├─ Nodes/
   │  │  ├─ SkillNode.cs
   │  │  ├─ SkillNodeActions.cs
   │  │  ├─ SkillNodeConditions.cs
   │  │  └─ SkillNodeModifiers.cs
   │  ├─ Passive/PassiveSkillDefinition.cs
   │  ├─ Projectile/ProjectileSkillDefinition.cs
   │  ├─ Single/SingleSkillDefinition.cs
   │  ├─ Trigger/SkillTriggerDefinition.cs
   │  └─ Zone/ZoneSkillDefinition.cs
   ├─ Delivery/
   │  ├─ Buff/
   │  │  ├─ BuffSkillExecutor.cs
   │  │  └─ BuffSkillActor.cs
   │  ├─ Line/
   │  │  ├─ LineSkillExecutor.cs
   │  │  └─ LineSkillActor.cs
   │  ├─ Projectile/
   │  │  ├─ ProjectileSkillExecutor.cs
   │  │  └─ ProjectileSkillActor.cs
   │  ├─ Single/
   │  │  ├─ SingleSkillExecutor.cs
   │  │  ├─ SingleSkillActor.cs
   │  │  └─ SingleSkillRules.cs
   │  └─ Zone/
   │     ├─ ZoneSkillExecutor.cs
   │     └─ ZoneSkillActor.cs
   ├─ Execution/
   │  ├─ SkillActionContext.cs
   │  ├─ SkillExecution.cs
   │  ├─ SkillExecutionRuleResolver.cs
   │  ├─ SkillStatus.cs
   │  └─ SkillTargeting.cs
   ├─ Reactions/
   │  └─ SkillTrigger.cs
   └─ Runtime/
      ├─ SkillExecutionData.cs
      └─ UnitSkills.cs
```

Changes from the current tree:

- Rename `BuffSkillExecutors.cs` to `BuffSkillExecutor.cs` because it now contains one Executor class.
- Keep `BuffSkillActor.cs` as the Buff/status visual lifetime owner.
- Delete `EffectCreateRequest.DurationSeconds` after removing `EffectManager`'s automatic Actor attachment; the creating Buff/Single/Projectile/Line/Zone path passes lifetime directly to its Actor initializer.
- Keep all other listed scripts. Add no production script.

## Script responsibilities

### Definitions

| Script | Final responsibility | Must not own |
|---|---|---|
| `SkillDefinition.cs` | Shared immutable skill contracts: timing, targeting, damage, status, area, visual reference, Nodes | Runtime targeting, damage application, Actor lifetime |
| `ProjectileSkillDefinition.cs` | Projectile-only launch, burst, pierce, movement, contact, and impact contracts | Launch execution or collision |
| `LineSkillDefinition.cs` | Line width, length, repeats, knockback, tick damage/status contracts | Direction selection or tick execution |
| `SingleSkillDefinition.cs` | Direct, delayed, area, multi-deployment, prefab-hitbox, execute, status-consumption contracts | Target selection, delay coroutine, damage |
| `ZoneSkillDefinition.cs` | Zone area, duration, tick, target-count, damage/status contracts | Zone instance lifetime or recast execution |
| `BuffSkillDefinition.cs` | Status, Heal, Shield, Charge value contracts through `BuffEffectKind` | Runtime application or AI movement |
| `PassiveSkillDefinition.cs` | Passive modifier contracts | Per-cast execution |
| `SkillChoice.cs` | Choice identity and typed Node modifications | Runtime effect application |
| `SkillTriggerDefinition.cs` | Trigger condition, schedule, command, and triggered Definition contracts | Delivery implementation |
| `SkillNode*.cs` | Typed generated operation wrapper, conditions, modifiers, and actions | Runtime string parsing or family execution |

### Shared execution and runtime

| Script | Final responsibility | Must not own |
|---|---|---|
| `SkillExecution.cs` | Validate cast, build context, finalize snapshot, dispatch family, commit cooldown/magazine, publish cast lifecycle | Family movement, collision, hit application |
| `SkillActionContext.cs` | Immutable event data passed to Trigger handling | Decisions or mutation |
| `SkillExecutionData.cs` | One cast's typed, finalized static values plus typed hit-time rule inputs | Target state mutation, coroutines, GameObject lifetime |
| `SkillExecutionRuleResolver.cs` | Shared target-dependent damage/critical rule evaluation and one common `ApplyHitEnhancements` implementation | Target search or Actor lifetime |
| `SkillStatus.cs` | Convert final status Definition plus snapshot modifiers to runtime status application data | Selecting targets or applying status |
| `SkillTargeting.cs` | Shared deterministic target lists, nearest target, chain target, center, radius, and direction helpers | Damage/status application |
| `UnitSkills.cs` | Learned skill and Choice ownership | Per-cast execution |
| `SkillTrigger.cs` | Event gates, chance, internal cooldown, delay/repeat, recursion protection, then common `SkillExecution` call | Family targeting, damage, visual, or Actor behavior |

### Effects

| Script | Final responsibility | Must not own |
|---|---|---|
| `EffectManager.cs` | Create visual/prefab/empty objects, attach, track, delete on Actor request, route tracked-status end signals, force-clear combat effects | Normal lifetime countdown, family Actor selection, hit judgment, damage, status, skill Trigger publication |
| `EffectVisualBuilder.cs` | Configure renderer, animator, scale, rotation, and hitbox presentation | Gameplay collision result or lifetime |

`EffectManager` does not attach `SingleSkillActor` or `BuffSkillActor` from `EffectCreateRequest` shape. The family caller attaches the correct existing Actor. No generic lifetime component is added.

## Family Executor / Actor responsibilities

### Projectile

`ProjectileSkillExecutor`:

- Receive validated `SkillExecutionContext` and finalized `SkillExecutionData`.
- Create initial projectile effect or empty object through `EffectManager`.
- Attach `ProjectileSkillActor`.
- Pass prepared launch values to `Initialize`.
- Return whether launch started.

It does not select targets, calculate damage/speed/pierce/count, directly hit a target, schedule follow-ups, publish hit events, or apply status.

`ProjectileSkillActor`:

- Own movement, lifetime, pierce, stop-on-first-hit, impact delay, and follow-up launch timing.
- Use `UnitCollisionResolver` for contact.
- Judge target-dependent damage/critical/branch rules.
- Apply contact and impact damage/status.
- Call shared `SkillExecutionRuleResolver.ApplyHitEnhancements`.
- Publish hit/expire lifecycle events.
- Request removal from `EffectManager`.
- Own Projectile-created impact and branch visual lifetime; do not borrow another family's Actor as a timer.

The current no-visual direct-hit fallback is removed. No visual still creates an empty projectile Actor, preserving one execution path.

### Line

`LineSkillExecutor`:

- Create initial line effect or empty object.
- Attach `LineSkillActor`.
- Pass prepared line launch values.
- Return whether launch started.

It does not find a target, calculate directions/length/width/duration/ticks, own repeat coroutines, apply damage, or publish deployment/hit events.

`LineSkillActor`:

- Own line placement, repeat timing, duration, tick schedule, and runtime hitbox.
- Resolve targets through `UnitCollisionResolver`.
- Apply damage, status, and knockback.
- Call shared hit enhancements.
- Publish deployment/hit lifecycle events.
- Remove finished line effects through `EffectManager`.

### Single

`SingleSkillExecutor`:

- Always create a visual, prefab, or empty Single object.
- Attach `SingleSkillActor`.
- Pass validated context and finalized snapshot.
- Return whether the Actor started.

It does not reject execute-threshold casts, select targets, calculate radius/damage/status, start delay/repeat/follow-up coroutines, inspect prefab collisions, apply effects, or publish hit events.

`SingleSkillActor`:

- Own damage delay, animation lifetime, deployment repeats, and conditional follow-up timing.
- Resolve direct, limited, area, event-locked, and prefab-hitbox targets at the same timing as current behavior.
- Use `SkillTargeting` for logical targeting and `UnitCollisionResolver` for prefab hitboxes.
- Apply damage/status and target-status consumption.
- Run execute, boss, kill recovery, cooldown refund, redistribution, and follow-up rules through `SingleSkillRules` and shared resolvers.
- Publish `OnDeploymentCast`, `OnHit`, and `OnHitCount` only after the corresponding gameplay event.
- Remove itself through `EffectManager` after gameplay and animation lifetime finish.

`SingleSkillRules`:

- Keep Single-only target-dependent execute, boss, consumed-status, and kill-recovery calculations.
- Expose the cast threshold precheck to `SkillExecution`.
- Do not create objects, select targets, apply damage, or own time.

### Zone

`ZoneSkillExecutor`:

- Create the initial zone effect or empty object for each already-prepared deployment center.
- Attach `ZoneSkillActor`.
- Pass prepared zone values.
- Return whether at least one Actor started.

It does not calculate centers/radius/duration/ticks/damage, select tick targets, apply effects, or own recast validation.

`ZoneSkillActor`:

- Own zone duration, ticks, prefab collision, hit limits, and expiry.
- Select targets at each tick through `SkillTargeting` or `UnitCollisionResolver`.
- Apply damage/status and shared hit enhancements.
- Publish hit and expire lifecycle events.
- Request removal through `EffectManager`.

Recast preparation, generation limit, inherited snapshot, duration/radius override, and center selection move to `SkillExecution`/`SkillTrigger`. Recast then uses the same `ZoneSkillExecutor.Execute` entry; separate `ExecuteRecast` delivery logic is deleted.

### Buff

`BuffSkillExecutor`:

- Explicit no-spatial-gameplay-Actor exception.
- Receive finalized effect amount/duration/status.
- Resolve immediate Buff targets through the existing `SkillTargeting`.
- Delegate Status to `StatusCombatRules.ApplyStatus`.
- Delegate Heal, Shield, and Charge contact to existing `InGameCombatManager` APIs.
- Ask `EffectManager` for an optional visual object, then explicitly attach and initialize `BuffSkillActor`.
- Return whether immediate execution committed.

It does not calculate heal/shield values, interpret snapshot modifiers, implement a separate targeting algorithm, run spatial collision, count down visual lifetime, or delete the object.

`BuffSkillActor`:

- Exists as the Buff/status visual lifetime owner, not a Buff gameplay-application Actor.
- Temporary Buff visual: count down the prepared duration.
- Persistent status visual: retain the associated `StatusRuntimeInstance` identity and complete when the existing status-removal path signals that the status ended.
- On temporary timeout or persistent status end, call `EffectManager.RemoveEffect`.
- Do not apply Status, Heal, Shield, Charge, targeting, or combat calculations.
- Charge cross-frame lifetime remains owned by `SkillUseState` and enemy AI.

## Dependency direction

```text
Definitions
    ↓
SkillExecutionData ← SkillExecutionRuleResolver / SkillStatus
    ↓
SkillExecution → Family Executor
                    ↓
                Family Actor
                    ↓
 SkillTargeting / UnitCollisionResolver / Combat APIs
                    ↓
              EffectManager removal
```

Forbidden reverse dependencies:

- `SkillExecution` must not call Actor hit methods.
- Actor must not call `SkillExecution.TryExecuteSkill` except through an explicit Trigger/follow-up request already owned by `SkillTrigger`.
- `EffectManager` must not inspect Definitions or apply gameplay.
- `SkillTargeting` must not know Executor or Actor types.

## Migration plan

1. Record current static references, file counts, builds, Unity console, and focused EditMode tests.
2. Extend `SkillExecutionData` finalization and move Executor-side cast-fixed calculations to `SkillExecutionState.BuildExecutionData` or finalization called by `SkillExecution`.
3. Move family cast rejection and preflight validation to `SkillExecution`.
4. Consolidate the four `ApplyHitEnhancements` copies into `SkillExecutionRuleResolver`, preserving Single lifecycle policy.
5. Convert `SingleSkillActor` from visual timer to full Single judgment/application Actor; reduce `SingleSkillExecutor` to creation and initialization.
6. Remove Projectile direct-hit fallback and Executor follow-up coroutine; make `ProjectileSkillActor` own every projectile path.
7. Move Line repeat schedule and remaining calculations into prepared snapshot/`LineSkillActor`.
8. Move Zone recast preparation to shared execution and reduce Zone Executor to Actor launch.
9. Remove `EffectManager`'s automatic family-Actor attachment and normal lifetime timing. Make every family creation path explicitly attach and initialize its own existing Actor.
10. Delete the now-unused `EffectCreateRequest.DurationSeconds` parameter/property and pass lifetime directly to Actor initialization.
11. Keep `BuffSkillActor` for temporary Buff/status visual lifetime. Route persistent status-end notifications to it so normal status visual deletion is requested by the Actor.
12. Replace the Projectile branch visual's timer-only `LineSkillActor` with `ProjectileSkillActor.InitializeVisualLifetime`.
13. Trim Buff Executor to prepared immediate application and rename `BuffSkillExecutors.cs` with `.meta` GUID preserved.
14. Run static, build, Unity compilation/console, and EditMode verification after each family migration.

Each family is a containment point. If one family fails verification, do not migrate the next family until the current behavior is restored.

## Behavior compatibility checklist

- Manual aim and automatic nearest-target direction remain unchanged.
- Event-target locking does not fall back to another target.
- Single delayed hits select targets at the current delayed-hit timing.
- Projectile contact, pierce, stop-on-first-hit, impact, branch, burst, and follow-up behavior remain unchanged.
- Line repeats, tick interval, hitbox, critical, status, and knockback remain unchanged.
- Zone deployment count, center, radius, tick, target cap, recast generation, and expire behavior remain unchanged.
- `OnHit` publishes only after a successful hit result, never when an Actor is merely created.
- `OnHitCount` uses actual successful target count.
- Trigger-created skills use the same family Actor path and retain source skill identity/lifecycle suppression.
- No-visual Projectile, Line, Single, and Zone still execute through empty Actors.
- Cooldown, magazine, reload, and active-duration commit remain in `SkillExecution`/`SkillUseState`.
- Charge continues to use ordinary enemy targeting and movement without a Charge Actor or state.

## Acceptance criteria

- Spatial Executor searches return zero `DamageCalculator`, `SkillStatus.StatusSpec`, `SkillTargeting`, `UnitCollisionResolver`, `ApplyDamage`, `ApplyStatus`, `StartCoroutine`, `WaitForSeconds`, and hit lifecycle publication.
- `SingleSkillExecutor` only creates/attaches/initializes `SingleSkillActor` and reports launch success.
- `SingleSkillActor` owns Single target selection, prefab collision, delay/repeat/follow-up timing, damage/status, Trigger publication, and removal.
- Projectile has no direct-hit path that bypasses `ProjectileSkillActor`.
- Line and Zone cross-frame behavior exists only in their Actors.
- One shared `ApplyHitEnhancements` implementation remains; four Executor copies are gone.
- `EffectManager` contains zero `SingleSkillActor.Attach` and `BuffSkillActor.Attach` calls.
- `EffectManager` contains no normal lifetime countdown or timed-effect collection.
- `EffectCreateRequest` contains no `DurationSeconds`; lifetime is passed directly to the family Actor.
- Every created temporary skill visual has an explicitly initialized family Actor.
- Spatial Actors and `BuffSkillExecutor` use `SkillTargeting`; no second targeting implementation exists.
- Projectile-owned impact/branch visuals use `ProjectileSkillActor`, not timer-only `LineSkillActor`.
- `BuffSkillActor` requests `EffectManager.RemoveEffect` on temporary timeout or persistent status-end signal.
- Normal status expiry/consume/remove paths do not directly delete the mapped visual before `BuffSkillActor` completes.
- `EffectManager.ClearEffects` remains the allowed forced-cleanup exception.
- `BuffSkillExecutors.cs` becomes `BuffSkillExecutor.cs` with its `.meta` GUID preserved.
- `ZoneSkillExecutor.ExecuteRecast` is removed and recast reaches normal Zone execution.
- No new base Actor, interface, factory, or standalone contract script exists.
- Active Definition/CSV/asset values remain unchanged.
- `git diff --check` passes.
- Runtime and Editor builds complete with zero errors.
- Unity script compilation and console contain no project errors.
- Focused and full available EditMode tests pass.
- User verifies representative Projectile, Line, Single, Zone, Buff, Trigger, no-visual, and Charge behavior in Play Mode.

## Verification expected from Code Builder

- Static symbol and forbidden-call searches for every Executor and Actor.
- Current-versus-final file tree comparison.
- `.meta` GUID comparison for the Buff Executor rename.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false`.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false`.
- Unity script refresh/compilation and console inspection through Unity-MCP.
- Focused Actor-boundary tests plus full available EditMode tests.
- No CSV comparison is required unless implementation unexpectedly changes a CSV.

## Related boards

- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/COMBAT/SKILL_DEFINITION_FAMILY_CONSOLIDATION_HANDOFF.md`
- `boards/COMBAT/SKILL_TRIGGER_EXECUTOR_REUSE_HANDOFF.md`

## Next Actions

- User verifies representative Projectile, Line, Single, Zone, Buff, Trigger, no-visual, and Charge behavior in Play Mode.

## Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/SkillExecutionData.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRuleResolver.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTargeting.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Projectile/ProjectileSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Projectile/ProjectileSkillActor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Line/LineSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Line/LineSkillActor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Single/SingleSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Single/SingleSkillActor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Single/SingleSkillRules.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Zone/ZoneSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Zone/ZoneSkillActor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Buff/BuffSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Delivery/Buff/BuffSkillActor.cs`
- `Pakuri/Assets/Scripts/Combat/Effects/EffectManager.cs`
- `Pakuri/Assets/Scripts/Combat/Effects/EffectVisualBuilder.cs`
- `Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`

## History

- 2026-07-31: User defined Executor as a no-validation, no-Snapshot-interpretation family launcher.
- 2026-07-31: User defined Actor as the spatial judgment, gameplay application, Trigger publication, and completion owner.
- 2026-07-31: User required Single to follow the same high-level `execute -> judge` flow as Projectile, Line, and Zone.
- 2026-07-31: Designer inspected current family delivery, shared execution, runtime snapshot, Trigger, and EffectManager paths and created this handoff.
- 2026-07-31: User corrected visual lifetime ownership: Actor owns completion and signals `EffectManager` to delete.
- 2026-07-31: Designer removed the proposed `EffectManager` timer, retained `BuffSkillActor`, and added the persistent-status visual end-signal boundary.
- 2026-07-31: User confirmed that target selection must reuse `SkillTargeting.cs`; Buff remains the immediate Executor-side targeting exception because it has no gameplay Actor.
- 2026-07-31: Code Builder reduced all four spatial Executor files to Actor creation/initialization only and moved family logic into the matching Actor files.
- 2026-07-31: Code Builder consolidated hit enhancements, removed Projectile and Zone bypass paths, corrected EffectManager/Buff Actor lifetime signaling, renamed the Buff Executor with its GUID preserved, and completed static/build/Unity/EditMode verification.
