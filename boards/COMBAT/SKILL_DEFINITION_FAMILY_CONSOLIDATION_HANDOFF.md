# Skill Definition Family Consolidation Handoff

## Task title

Consolidate skill Definitions and execution logic by real delivery family.

## Goals

- Split `Combat/Skills/Definitions` into family folders.
- Keep one concrete Definition class for each final family: Projectile, Line, Single, Zone, Buff, and Passive.
- Replace `SingleChainSkillDefinition` with a Trigger-generated follow-up `SingleSkillDefinition`.
- Move charge initiation from `SingleChargeSkillDefinition` into `BuffSkillDefinition`.
- Replace `BuffHealSkillDefinition` and `BuffShieldSkillDefinition` with one `BuffSkillDefinition`.
- Delete type-test dispatch, generation branches, executor overloads, and duplicated target/visual loops made unnecessary by the consolidation.
- Keep common value contracts with their real owners instead of adding standalone contract scripts.

## Constraints

- Role Owner is Code Builder.
- Preserve current player-facing behavior unless this handoff explicitly changes the implementation mechanism.
- Preserve all current CSV values, IDs, asset paths, cooldowns, targeting values, status values, Trigger order, and runtime visuals.
- Do not add `SkillSpecs.cs`, `StatusContracts.cs`, `RuntimeSkillVisualSpec.cs`, or `SkillTriggerContracts.cs`.
- Do not keep removed concrete classes as aliases, wrappers, empty subclasses, compatibility shells, or copied fields with separate executor branches.
- Do not create `ProjectileSkillNode`, `SingleSkillNode`, or other family Node inheritance.
- Keep authored parsing in Loading and final typed Definition generation in `GameDataCatalogBuilder`.
- Preserve the existing one validation, one catalog build, and one lookup rebuild pipeline.
- Preserve the existing XML-comment-tag cleanup already present in the worktree.
- Unity Play Mode gameplay verification remains user-owned.

## Role Owner

Code Builder

## Status

Code implementation and non-Play-Mode verification complete.

## Target structure

```text
Pakuri/Assets/Scripts/Combat/Skills/Definitions/
├─ SkillDefinition.cs
├─ Projectile/
│  └─ ProjectileSkillDefinition.cs
├─ Line/
│  └─ LineSkillDefinition.cs
├─ Single/
│  └─ SingleSkillDefinition.cs
├─ Zone/
│  └─ ZoneSkillDefinition.cs
├─ Buff/
│  └─ BuffSkillDefinition.cs
├─ Passive/
│  └─ PassiveSkillDefinition.cs
├─ Choice/
│  └─ SkillChoice.cs
├─ Trigger/
│  └─ SkillTriggerDefinition.cs
└─ Nodes/
   ├─ SkillNode.cs
   ├─ SkillNodeConditions.cs
   ├─ SkillNodeModifiers.cs
   └─ SkillNodeActions.cs
```

## Ownership boundaries

### `SkillDefinition.cs`

- Owns the base `SkillDefinition`.
- Owns only enums and value specs shared by multiple final skill families.
- Keeps `SkillTimingSpec`, `SkillTargetingSpec`, `SkillDamageSpec`, `StatusApplicationSpec`, and `AreaBlueprintSpec`.

### Family Definitions

- `ProjectileSkillDefinition.cs` owns `ProjectileSkillDefinition` and `ProjectileBlueprintSpec`.
- `LineSkillDefinition.cs` owns `LineSkillDefinition`.
- `SingleSkillDefinition.cs` owns the only Single concrete Definition.
- `ZoneSkillDefinition.cs` owns `ZoneSkillDefinition`.
- `BuffSkillDefinition.cs` owns the only Buff concrete Definition and `BuffEffectKind`.
- `PassiveSkillDefinition.cs` owns `PassiveSkillDefinition` and `PassiveModifierKind`.

### Non-family contracts

- `SkillChoice.cs` owns `SkillChoice` and `SkillChoiceGroup`.
- `SkillTriggerDefinition.cs` owns Trigger enums, `SkillTriggerCommand`, and `SkillTriggerDefinition`.
- Node files group the existing operation value types by responsibility while retaining one `SkillNode` wrapper.
- Status target/merge/shield enums and `BuffModifierSpec` move to `Combat/Status/Definitions/StatusEffectDefinition.cs`.
- Runtime visual enums/specs move to `Combat/Effects/EffectManager.cs`; `EffectManager` retains creation/lifetime ownership and `EffectVisualBuilder` retains renderer/animator/hitbox configuration.

## Single consolidation

### Base Single

- `SingleSkillDefinition` remains the only Single Definition.
- Existing direct, area, prefab-hitbox, mark, execute, delayed-damage, and status behavior stays on the existing common Single executor path.

### Chain

- `DamageThenDelayedChain` creates a normal single-target `SingleSkillDefinition`.
- Loading Generation creates a hidden `SkillTriggerDefinition` from the existing authored chain values.
- The Trigger listens to `OnHit`, uses the authored delay, and runs a hidden `SingleSkillDefinition` follow-up.
- The follow-up uses the authored damage multiplier and radius, excludes the event target, and selects the nearest other target.
- The existing `SkillTargeting.ChainTargets` algorithm remains the single targeting implementation.
- Follow-up skill lifecycle publication is disabled so the follow-up does not recursively trigger itself.
- Outgoing-damage combat events remain available as in the current direct `ApplyDamage` path.
- Delete the Chain executor overload, coroutine, target scan, hit method, and concrete class.

### Charge

- `OpeningCharge` remains authored as `SkillRuntimeKind.Buff` and remains activated by its existing CombatStart Trigger.
- Generation creates `BuffSkillDefinition` with `BuffEffectKind.Charge`.
- Buff execution activates the existing `SkillUseState` runtime.
- `EnemyCombatDecision` supplies the ordinary nearest-player/Nexus target; no Charge target state or retargeting path remains.
- `EnemyActionController` reuses ordinary movement and suppresses other skills while Charge is active.
- Preserve speed ramp, movement blocking rules, Collider collision, max-health damage, on-hit status, and charge cancellation.
- Delete the Single Charge executor overload and concrete Definition.

## Buff consolidation

`BuffSkillDefinition` owns one explicit `BuffEffectKind`:

```text
Status
Heal
Shield
Charge
```

- `Status` uses `StatusCombatRules.ApplyStatus`.
- `Heal` uses `InGameCombatManager.Heal` and preserves passive healing modifiers.
- `Shield` uses `InGameCombatManager.ApplyShieldStatus` and preserves shield amount, duration, refresh, absorb, and expire behavior.
- `Charge` activates the existing Buff skill runtime and applies its contact outcome through the shared Buff executor.
- One `BuffSkillExecutor.Execute` entry selects targets once and delegates only the irreducible effect application.
- Common target resolution and visual creation are shared.
- `SkillRuntimeKind` is not used as the effect discriminator because `ShieldUp` is authored as `Shield` but executes as a status buff, while `OpeningCharge` is authored as `Buff` but executes as charge.
- Delete `BuffHealSkillDefinition`, `BuffShieldSkillDefinition`, their executor classes, and their dispatch branches.

## Dead-field cleanup

Current static inspection shows these `BuffSkillDefinition` fields are written but not consumed by runtime:

- `BuffDuration`
- `Modifiers`
- `HasAttachedDamage`
- `AttachedDamage`
- `AttachedDamageRadius`

Delete them and their Generation writes only after the baseline catalog and build checks pass.

## Migration phases

1. Record baseline static references, catalog tests, runtime/editor builds, and Unity console state.
2. Convert Chain to generated Trigger + common Single execution.
3. Move Charge initiation and runtime state to Buff.
4. Merge Status/Heal/Shield/Charge generation and execution into one Buff type and executor.
5. Delete removed type symbols and dead fields.
6. Split Definitions into the approved family folders and move existing `.meta` GUIDs where applicable.
7. Run static, build, Unity compile/console, catalog, and EditMode verification.

## Compatibility constraints

- No current Definition class is Unity-serialized; final skill Definitions are generated in Loading.
- Keep namespaces and public field meanings stable unless the field belongs to a removed class.
- Hidden Trigger Definitions remain unregistered in learned-skill/UI lists.
- Existing Trigger source IDs and source snapshots remain unchanged.
- Existing skill CSV schemas remain unchanged for the Chain migration; existing chain columns remain the tuning source.
- New C# scripts under `Assets` require Unity `.meta` files.

## Acceptance criteria

- Active production and test searches return zero `SingleChainSkillDefinition`, `SingleChargeSkillDefinition`, `BuffHealSkillDefinition`, and `BuffShieldSkillDefinition`.
- `SkillExecution.ExecuteSkill` has one Single dispatch and one Buff dispatch.
- `SingleSkillExecutor` has one public family entry and no Chain/Charge overload.
- Buff Status, Heal, Shield, and Charge use one concrete Definition and one executor entry.
- Chain follow-up is represented by a `SkillTriggerDefinition` and a hidden `SingleSkillDefinition`.
- Chain keeps 0.5-second delay, 0.5 damage multiplier, radius 7, and primary-target exclusion for current `ChainLightning`.
- OpeningCharge keeps CombatStart activation, speed ramp, collision, max-health damage, and Freeze application while using the ordinary nearest-player/Nexus AI target.
- Heal, GuardianFlag shield, ShieldUp status buff, ChargeCommand status buff, and Trigger-generated shields retain current behavior.
- Definitions folder matches the approved family structure without the rejected helper scripts.
- `git diff --check` passes.
- Runtime and Editor builds complete with zero errors.
- Unity compilation and console contain no project errors.
- Relevant EditMode tests pass.

## Verification expected from Code Builder

- Static symbol and declaration searches.
- CSV width/value comparison for any touched CSV; no CSV change is expected.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false`.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false`.
- Unity script refresh/compilation and console inspection through Unity-MCP.
- Focused and full available EditMode tests.
- User Play Mode verification for ChainLightning, OpeningCharge, Heal, GuardianFlag, ShieldUp, and ChargeCommand.

## Related boards

- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`

## Next Actions

- User Play Mode verification for ChainLightning, OpeningCharge, Heal, GuardianFlag, ShieldUp, and ChargeCommand.

## Evidence

- `SkillDefinition.cs` currently contains the base Definition, ten concrete subclasses, shared specs, Choice, Trigger, and visual contracts.
- `SingleSkillExecutor` currently has separate overloads for Single, Chain, and Charge.
- `BuffSkillExecutors.cs` currently has separate Status, Shield, and Heal executor classes.
- `GameDataCatalogBuilder.Skills.cs` selects concrete subclasses by runtime kind and execution profile.
- `OpeningCharge` is authored as Buff and activated by a CombatStart Trigger, but still creates `SingleChargeSkillDefinition`.
- `ChainLightning` is the only inspected `DamageThenDelayedChain` row and carries all follow-up tuning in its existing base-skill CSV row.
- `EffectManager` creates, attaches, tracks, and removes runtime effects; `EffectVisualBuilder` configures renderers, animators, scale, and hitboxes.
- Status target, merge, shield-refresh, and modifier contracts are consumed by `Combat/Status`.
- Final Definitions contain one concrete class per Projectile, Line, Single, Zone, Buff, and Passive family.
- ChainLightning builds `ChainLightning__chain_on_hit`: OnHit, 0.5-second delay, 0.5 multiplier, radius 7, nearest target excluding the primary, hidden common Single Definition, and lifecycle suppression.
- OpeningCharge generates `BuffSkillDefinition` with `BuffEffectKind.Charge`; its lifetime now uses `SkillUseState` and has no Charge-specific actor/state.
- Status, Heal, Shield, and Charge share `BuffSkillExecutor.Execute`, common target resolution, and common visual creation.
- Static searches returned zero removed subclasses, old charge runtime names, XML tags, and rejected standalone contract files.
- `git diff --check` passed.
- Runtime and Editor builds completed with zero errors and the two existing assembly-version warnings.
- Unity script compilation completed with zero C# errors.
- Focused family regression passed 1/1; full Unity EditMode suite passed 10/10.

## History

- 2026-07-30: User requested family-folder organization and rejected file-only pseudo-consolidation.
- 2026-07-30: User approved Trigger-based Chain, Buff-family Charge, and unified Buff Status/Heal/Shield behavior.
- 2026-07-30: User rejected standalone Skill spec, Status contract, visual contract, and Trigger contract scripts.
- 2026-07-30: Code Builder created this handoff before implementation as explicitly requested.
- 2026-07-30: Code Builder completed the family consolidation, folder split, dead-field deletion, Unity compilation, builds, and EditMode verification.

---

## Task title

Remove Charge-specific actor/state by reusing the active Buff runtime and enemy movement path.

## Goals

- Delete `Delivery/Buff/ChargeActor.cs` and `Delivery/Buff/ChargeState.cs`.
- Remove `UnitCombatState.ActiveCharge`.
- Use the existing `SkillUseState` active duration for the cross-frame lifetime.
- Use the existing enemy movement, `SkillTargeting`, `UnitCollisionResolver`, damage, and status paths.
- Keep only the irreducible Charge contact-effect branch on `BuffSkillDefinition`.

## Constraints

- A file deletion must remove responsibility rather than copy the same actor/state logic elsewhere.
- Do not add a generic movement-effect interface or another runtime state class for the single current Charge skill.
- Preserve the authored 3-second speed ramp and 2.5 maximum movement multiplier.
- Reuse the target already resolved by `EnemyCombatDecision.FindNearestPlayerTarget`; do not run Charge-specific `SkillTargeting`.
- This changes `OpeningCharge` from its authored `RandomHostile` selection to the ordinary enemy target rule: nearest living non-Nexus player, then Nexus.
- `active_duration_seconds=5` must become the actual Charge lifetime; it is currently authored but not consumed by `ChargeActor`.
- CombatStart-triggered skill execution must start the existing runtime before `SkillUseState.IsActive` can own Charge lifetime.

## Role Owner

Code Builder

## Status

Code implementation and non-Play-Mode verification complete.

## Next Actions

- User Play Mode verification: OpeningCharge selects the ordinary nearest player, suppresses other skills for its active duration, accelerates over the authored ramp, applies contact damage and Freeze, then ends.

## Evidence

- `BuffSkillExecutor.ExecuteCharge` selects through `SkillTargeting.FindNearestTarget` and copies skill Definition values into `UnitCombatState.ActiveCharge`.
- `ChargeState` stores `SkillId`, `TargetUnitId`, elapsed time, ramp, speed multiplier, damage ratio, on-hit status, and attribute; all immutable effect values already exist on `BuffSkillDefinition`.
- `ChargeActor.Tick` is a separate pre-AI loop called by `EnemyActionController.Tick`; it duplicates movement orchestration but already delegates collision, damage, and status to shared systems.
- `EnemyActionController.MoveToward` is the existing ordinary enemy movement path and already applies `StatusCombatRules.MoveSpeedMultiplier`.
- `EnemyActionController.TickEnemy` already resolves one ordinary target through `EnemyCombatDecision.FindNearestPlayerTarget` before movement and attack decisions.
- `EnemyCombatDecision.FindNearestPlayerTarget` does not retain a target; it returns the current nearest living non-Nexus player and falls back to the Nexus.
- An active-Charge branch placed after that ordinary target resolution can move toward the same target, resolve contact, and return before support/offensive skill selection, which suppresses every other skill without a separate Charge state.
- `SkillUseState` already owns and ticks `ActiveDurationRemaining`, and `TryBeginCast` initializes it from `SkillTimingSpec.ActiveDuration`.
- Triggered execution calls `ExecutePrepared(..., false, ...)`, so the current CombatStart Trigger does not call `TryBeginCast`.
- The Trigger CSV schema contains only `trigger_id`, source/triggered skill IDs, event, sort order, and enabled; it has no begin-cast field.
- `OpeningCharge` is authored with `active_duration_seconds=5`, `move_speed_multiplier=2.5`, `target_max_health_ratio=1`, and Freeze duration 5.
- `ChargeActor.cs`, `ChargeState.cs`, their `.meta` files, and `UnitCombatState.ActiveCharge` were deleted.
- Triggered Charge execution now begins the existing runtime, and `SkillUseState.StopActive` ends it on contact.
- `EnemyCombatDecision.ResolveActiveCharge` reads the active skill collection without allocating a new target state.
- `EnemyActionController` resolves the ordinary target once, reuses `MoveToward`, returns before other skill decisions, and checks contact through `UnitCollisionResolver`.
- `BuffSkillExecutor.ApplyChargeContact` applies the authored max-health damage and attached status, then stops the active runtime.
- Exact production/test symbol searches returned zero `ChargeActor`, `ChargeState`, and `ActiveCharge` references.
- `git diff --check` passed.
- Runtime and Editor `dotnet build --no-restore /p:UseSharedCompilation=false` completed with zero errors and the existing two assembly-version warnings.
- Unity script compilation completed with zero project-script console errors; the console retained one unrelated MCP transport disposal error under its package path.
- Focused Charge-runtime and forward/backward collision tests passed 2/2.
- Full Unity EditMode suite passed 11/11.

## History

- 2026-07-31: User proposed deleting Charge-specific actor/state and expressing Charge as a Buff over common targeting, movement, collision, damage, and status paths.
- 2026-07-31: Designer confirmed that deletion is feasible only by moving lifetime to `SkillUseState`; target lock cannot be preserved without retaining some target-specific state.
- 2026-07-31: User directed Charge to reuse the ordinary `EnemyCombatDecision` target and to suppress other skill use while the Buff is active; Designer removed the proposed Charge-specific retargeting path.
- 2026-07-31: User accepted the ordinary nearest-player target and explicitly handed implementation to Code Builder.
- 2026-07-31: Code Builder deleted the Charge-specific actor/state, reused shared runtime/AI/movement/collision paths, added focused regression coverage, and completed non-Play-Mode verification.
