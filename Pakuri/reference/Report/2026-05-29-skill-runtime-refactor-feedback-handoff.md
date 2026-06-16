# 2026-05-29 Skill Runtime Refactor Feedback Handoff

Role Owner: Designer

Status: Conversation-aligned handoff updated on 2026-05-30

Linked HTML: `Pakuri/reference/Report/2026-05-29-skill-runtime-refactor-feedback-handoff.html`

## Goal

Give concise, code-evidence-based feedback to the original worker who wrote the two supplied skill reports.

The subject is the skill runtime structure: how skills are owned, executed, extended, and authored. This is not a general character-animation critique.

## Source Reports To Respond To

- `D:\ChromeDownLoad\2026-05-28-skill-structure-problem-and-improvement-report.html`
- `D:\ChromeDownLoad\2026-05-28-skill-csv-runtime-architecture-explained.html`

Those two files are treated as the original worker's explanation and critique. The feedback below responds to that work using current repository evidence.

## Executive Feedback

I agree with the core critique: the current skill system is CSV-driven at the authoring surface, but many new behaviors are being absorbed as flat CSV fields, flat snapshot properties, and growing central runtime classes.

The target direction should be:

- a unit-scoped skill owner so the character is the runtime subject that chooses and uses skills;
- a behavior-preserving transition that keeps current CSV values compatible;
- skill behavior represented as small condition, targeting, damage modifier, hit action, kill action, projectile behavior, status/buff, and visual handlers;
- no more default expansion by adding one-off fields to `SkillExecutionSnapshot`, `SingleAttackData`, or giant executor switches unless the behavior is genuinely common and belongs there.

## Current Code Evidence

### Character skill ownership is not unit-scoped yet

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:93` calls `skillExecution.Tick(...)` every frame.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs:21` defines the central tick entry point.
- `SkillExecutionSystem.cs:35` iterates the whole roster.
- `SkillExecutionSystem.cs:120` iterates each unit's active skills.
- `SkillExecutionSystem.cs:156` resolves a global executor from `SkillExecutorRegistry`.
- Search found no current `UnitSkillController`, `MonsterUnitRuntimeController`, or `UnitCombatController` type under `Pakuri/Assets/Scripts2/InGame`.

Conclusion:

The character has per-unit skill state, but not a per-unit skill execution owner. Current execution is manager/system-driven.

### The runtime state is per unit, but behavior is distributed

- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:64` to `:69` stores identity, stats, defenses, resources, `SkillRuntime`, and `Statuses`.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitRuntimeModel.cs:3` adds `State`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeFactory.cs:67` creates `new SkillRuntimeInstance(owner, skillData)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs:15` stores the skill owner.

Conclusion:

The data already supports unit-owned skill state. The missing piece is a unit-scoped object that owns cast decisions and execution requests.

### `MonsterUnitActor` is not a gameplay owner

- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs:19` stores `public MonsterUnitRuntimeModel Model`.
- `MonsterUnitActor.cs:21` initializes the model reference and refreshes the debug view.
- `MonsterUnitActor.cs:36`, `:44`, and `:52` play active skill, hit, and death animations.
- `MonsterUnitActor.cs:60` refreshes the unit view through `UnitActorView`.

Correct wording:

`MonsterUnitActor` is not "animation only", because it also owns model binding, debug HP/name display, and damage popup presentation. But it is still not the owner of HP mutation, skill execution, status application, or damage rules.

### Damage, death, status, and view refresh are manager/service-driven

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:137` applies damage.
- `InGameCombatManager.cs:162` delegates resource mutation to `UnitResourceMutationService.ApplyDamage(...)`.
- `InGameCombatManager.cs:164` to `:170` refreshes actor view, shows damage, dispatches triggers, and removes dead units.
- `InGameCombatManager.cs:195` and `:236` apply statuses.
- `InGameCombatManager.cs:268` applies shield statuses.
- `InGameCombatManager.cs:1063` to `:1087` unregisters and destroys dead unit actors.

Conclusion:

Gameplay behavior is split across model, actor, combat manager, skill execution system, executor files, status runtime, and resource mutation service. That is the architectural issue to report.

## Why The Current Skill Field Pattern Is The Problem

The following active CSV fields exist in `Pakuri/Assets/CSVdata/source/monster_skills.csv`:

- `execute_health_ratio_threshold`
- `require_execute_threshold_to_cast`
- `execute_damage_multiplier`
- `kill_cooldown_refund_ratio`
- `boss_damage_multiplier`

The following related choice fields exist in `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`:

- `execute_health_ratio_bonus`
- `execute_crit_chance_bonus`
- `boss_damage_multiplier`
- `kill_cooldown_refund_ratio_bonus`
- `kill_resets_cooldown`
- `kill_resets_cooldown_requires_execute`

Current code path:

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs:17` to `:21` stores execute, kill refund, and boss multiplier fields directly on `SingleAttackData`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs:32` to `:78` stores many direct choice fields, including execute, boss, kill refund, and kill reset fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs:48`, `:68`, and `:69` stores those values as flat snapshot properties.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:123` checks `RequireExecuteThresholdToCast`.
- `SingleAttackSkillExecutor.cs:1314` to `:1319` applies execute threshold damage and crit logic.
- `SingleAttackSkillExecutor.cs:1322` to `:1328` applies boss damage multiplier.
- `SingleAttackSkillExecutor.cs:1356` to `:1382` handles kill cooldown reset/refund.

Conclusion:

These fields are real examples of the issue described in the source reports. They are skill behavior rules encoded as ever-growing columns and flat properties, then interpreted inside a concrete executor.

## Recommended Conceptual Split

Do not solve every new behavior by adding more `SingleAttackData` fields or more `SkillExecutionSnapshot` properties.

Use these primitives instead:

| Current field or behavior | Better runtime concept |
|---|---|
| `require_execute_threshold_to_cast` | `CastCondition: target health ratio <= threshold` |
| `execute_health_ratio_threshold` | `TargetHealthRatioCondition` |
| `execute_damage_multiplier` | `ConditionalDamageModifier` |
| `execute_crit_chance_bonus` | `ConditionalCritModifier` |
| `boss_damage_multiplier` | `TargetPredicateDamageModifier` where `target.IsBoss` |
| `kill_cooldown_refund_ratio` | `OnKillAction: ReduceCooldown` |
| `kill_resets_cooldown` | `OnKillAction: ResetCooldown` |
| status duration or stat changes over time | `Status/BuffRuntimeHandler` |
| projectile movement, collision, branch, impact | `ProjectileBehavior` or projectile request/handler |

## Buff Versus Skill Rule

Use buff/status handlers for effects that persist on a unit and change unit state over time:

- action speed;
- attack or spell power;
- shield received;
- outgoing damage;
- incoming damage;
- resistance;
- critical chance or damage;
- move speed;
- status duration or stack behavior.

Do not force one-hit rules into buffs when they are only part of a hit calculation:

- boss-only damage;
- execute-only damage;
- execute-only crit chance;
- kill-triggered cooldown refund;
- target-health conditional cast rejection.

Those should be damage/cast/kill handlers, not status buffs.

## Projectile Subclass Boundary

Create a specialized projectile behavior only when the projectile itself behaves differently:

- homing;
- arc movement;
- delayed impact;
- pierce/contact lifetime rules;
- child or branch projectile spawning;
- projectile-owned collision filtering.

Do not put generic combat rules such as boss damage or execute damage inside projectile subclasses. Those rules belong to damage modifiers or hit actions so projectile, beam, zone, and single-hit skills can share them.

## Target Architecture

Recommended target shape:

```text
MonsterUnitActor
  presentation, animation, damage popup, debug labels

MonsterUnitRuntimeModel
  identity, stats, resources, statuses, skill runtime state

UnitSkillController
  per-character skill tick, cast decision, manual/auto routing, execution request creation

SkillExecutionPlan
  ordered condition/action/modifier nodes compiled from current CSV definitions

Action and modifier handlers
  damage, status, projectile spawn, zone spawn, cooldown change, reload change, visual spawn

InGameCombatManager and services
  scene roster, damage/status application, shared combat service coordination
```

This keeps the character as the subject using the skill while keeping low-level combat mutation in shared services.

## Behavior-Preserving Implementation Order

### Phase 1: Extract current SingleAttack special rules into handlers

Keep current CSV and `SingleAttackData` fields readable.

Extract the current execute, boss, and kill-recovery code into local runtime helpers or handler classes:

- `ISkillCastCondition`
- `TargetHealthRatioCastCondition`
- `ISkillDamageModifier`
- `ExecuteDamageModifier`
- `BossDamageModifier`
- `ISkillPostHitAction`
- `KillCooldownRefundAction`
- `KillCooldownResetAction`

Acceptance:

- Current SingleAttack behavior stays unchanged.
- `SingleAttackSkillExecutor` stops directly owning every execute/boss/kill rule inline.
- Existing fields still load and map.

### Phase 2: Add a unit-scoped skill owner

Introduce a unit-level controller such as `UnitSkillController`.

It should own:

- ticking that unit's `SkillRuntime.ActiveSkills`;
- deciding whether auto/manual skill use is allowed;
- resolving the execution request for the unit;
- notifying the actor about skill animation.

`InGameCombatManager` can still coordinate roster and services, but it should not be the place where all character skill decisions live.

Acceptance:

- A character/unit can be identified as the owner that uses its skills.
- Per-unit cast decision logic can be tested without routing through all of `InGameCombatManager.Update()`.
- `SkillRuntimeInstance` remains compatible as the state object.

### Phase 3: Convert flat snapshot fields into grouped modifier operations

Start by grouping existing flat fields into records:

- `DamageModifierOp`
- `CooldownModifierOp`
- `ProjectileModifierOp`
- `StatusModifierOp`
- `HitActionOp`
- `KillActionOp`

Acceptance:

- Adding a new damage condition should not require a new top-level `SkillExecutionSnapshot` property every time.
- Existing choice IDs and passive IDs still resolve.

### Phase 4: Compile current CSV into an execution plan

Keep current CSV headers for compatibility, but compile them into a plan:

```text
SkillExecutionPlan
  CastConditions[]
  Targeting
  Actions[]
  OnHitActions[]
  OnKillActions[]
  OnExpireActions[]
  Visuals[]
```

Acceptance:

- Current `Projectile`, `Beam`, `SingleAttack`, `Zone`, `Buff`, `Shield`, and `Passive` executors can remain as compatibility handlers.
- New behavior is added as a handler or node instead of another executor-specific field.

### Phase 5: Normalize authoring only after runtime is stable

Do not physically split the CSV first.

After the plan compiler works, consider moving from wide columns to row sets:

- skill base rows;
- skill action rows;
- condition rows;
- modifier rows;
- trigger rows;
- visual rows.

Acceptance:

- Current CSVs still import.
- New complex behavior can be expressed as rows instead of additional columns.

## Feedback To Give The Original Worker

The original reports are directionally right, but the next recommendation should be more specific:

1. The issue is not simply "CSV versus code".
2. The issue is that new skill behavior currently becomes wide CSV fields plus wide snapshot fields plus central executor logic.
3. The character already owns skill state through `BaseUnitRuntimeModel.SkillRuntime`, but does not own skill decision/execution through a unit-scoped controller.
4. Adding direct projectile subclasses is useful only for projectile behavior, not for general damage or kill rules.
5. Execute, boss, and kill-refund examples should become cast conditions, damage modifiers, and post-hit/post-kill actions.
6. Buff/status handlers should be used for persistent unit-state changes, not for every conditional hit rule.
7. The first implementation should be behavior-preserving and should keep all current CSV values compatible.

## Code Builder Acceptance Criteria

- Existing gameplay behavior is preserved.
- Current CSV files and generated runtime catalog remain compatible.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passes.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passes.
- Unity `Pakuri/Validate CSV Source Data` passes.
- If CSV mapping or runtime catalog assets change, Unity `Pakuri/Sync CSV Runtime Catalog Assets` is run.
- At least one skill using execute, boss damage, and kill cooldown behavior is checked against current behavior before and after the refactor.

## Related Boards

Update these if implementation proceeds:

- `boards/COMBAT/ENEMY_BLACKBOARD.md` for shared skill execution, triggers, projectile/single/line execution, or unit skill controller changes.
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md` for buff/status/shield/status-modifier behavior.
- `boards/DATA/DATA_BLACKBOARD.md` for CSV schema, catalog build, validation, or runtime data mapping.
- `boards/RUN/RUN_BLACKBOARD.md` only if NewRunScene runtime ownership or run flow changes directly.
