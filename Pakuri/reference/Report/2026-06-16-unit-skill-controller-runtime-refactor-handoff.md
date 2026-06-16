# 2026-06-16 UnitSkillController Runtime Refactor Handoff

Role Owner: Designer

Status: Code-evidence-based implementation handoff

Source handoff: `Pakuri/reference/Report/2026-05-29-skill-runtime-refactor-feedback-handoff.md`

## Goal

Create a behavior-preserving implementation plan for introducing `UnitSkillController` and restructuring current skill runtime ownership according to the feedback section in the source handoff.

The refactor target is not "CSV versus code". The target is to stop routing every new skill behavior through wider CSV columns, wider snapshot fields, and larger executor-specific logic, while keeping all current gameplay and current CSV rows compatible.

## Inspected Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:54` owns `private readonly SkillExecutionSystem skillExecution = new SkillExecutionSystem();`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:95` to `:105` ticks learned passives, then calls `skillExecution.Tick(...)`, then handles selected-player manual skill input.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs:21` defines the central tick entry point.
- `SkillExecutionSystem.cs:35` to `:38` iterates all roster entries.
- `SkillExecutionSystem.cs:107` to `:120` resolves the unit model, ticks `model.SkillRuntime`, and reads `skillRuntime.ActiveSkills`.
- `SkillExecutionSystem.cs:123` to `:129` checks `canAutoRoute(entry, runtime)` and routes each active skill.
- `SkillExecutionSystem.cs:156` and `:240` resolve executors through `SkillExecutorRegistry`.
- `SkillExecutionSystem.cs:271` to `:320` registers default executors: projectile, beam, single attack, zone, buff, shield, passive.
- Search under `Pakuri/Assets/Scripts2/InGame` found no current `UnitSkillController` type.
- Search under `Pakuri/Assets/Scripts2/InGame/Skills/Execution` found no current `ISkillCastCondition`, `ISkillDamageModifier`, `ISkillPostHitAction`, `SkillExecutionPlan`, `DamageModifierOp`, or `KillActionOp` type.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:63` to `:74` shows every unit model already owns `SkillRuntime`, `Statuses`, and auto flags.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeFactory.cs:35` to `:36` rebuilds `owner.SkillRuntime`.
- `SkillRuntimeFactory.cs:67` creates `new SkillRuntimeInstance(owner, skillData)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs:8` to `:15` stores the owner on each skill runtime.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs:19` to `:26` binds a `MonsterUnitRuntimeModel` and refreshes view/debug state.
- `MonsterUnitActor.cs:37` to `:47` owns active-skill and hit animation calls; `:84` to `:87` refreshes the actor view.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1` still contains direct execute/boss/kill columns such as `execute_health_ratio_threshold`, `require_execute_threshold_to_cast`, `execute_damage_multiplier`, `kill_cooldown_refund_ratio`, and `boss_damage_multiplier`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:1` still contains choice columns such as `execute_health_ratio_bonus`, `execute_crit_chance_bonus`, `boss_damage_multiplier`, `kill_cooldown_refund_ratio_bonus`, `kill_resets_cooldown`, and `kill_resets_cooldown_requires_execute`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs:24` to `:28` stores execute, kill cooldown refund, and boss multiplier fields directly on `SingleAttackData`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs:65` to `:71` stores execute crit, boss multiplier, kill refund, and kill reset values as top-level snapshot properties.
- `SkillExecutionSnapshot.cs:306` to `:326` applies those choice values into the snapshot.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs:75` to `:81` stores the same execute/boss/kill fields on choice specs.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:132` checks `RequireExecuteThresholdToCast`.
- `SingleAttackSkillExecutor.cs:1461` to `:1475` applies execute and boss damage directly inside the executor.
- `SingleAttackSkillExecutor.cs:1745` to `:1759` applies kill reset/refund directly inside the executor.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:165` to `:198` owns damage application coordination, actor refresh, damage popup, trigger dispatch, and death removal.
- `InGameCombatManager.cs:238` to `:311` owns status and shield-status application entry points.

## Current Problem

The repository already has unit-owned skill state, but not a unit-scoped skill decision owner.

Current flow:

```text
InGameCombatManager.Update()
  -> SkillExecutionSystem.Tick(roster, combatManager, ...)
     -> iterate all roster entries
     -> tick each model.SkillRuntime
     -> iterate each active skill
     -> resolve executor from global registry
     -> executor performs concrete behavior
```

That means each character has `SkillRuntime`, but the runtime subject that decides "this unit may cast this skill now" is still the global skill execution system plus `InGameCombatManager` predicate logic.

The second issue remains visible in current execute/boss/kill handling: behavior-specific fields flow from CSV to data classes, snapshot properties, and direct `SingleAttackSkillExecutor` branches. This is the pattern to break incrementally.

## Behavior That Must Stay Unchanged

- Current learned active skill loading must remain compatible with `SkillRuntimeFactory.RebuildLearnedActiveSet(...)`.
- Existing `monster_skills.csv` and `monster_skill_choices.csv` headers and row values must remain readable during the refactor.
- Existing executor routing through projectile, beam, single attack, zone, buff, shield, and passive handlers must keep working while ownership changes are introduced.
- Current execute-threshold cast rejection, execute damage, execute crit bonus, boss damage multiplier, kill cooldown refund, and kill cooldown reset behavior must remain identical before and after the first implementation phases.
- `MonsterUnitActor` should remain presentation/model-binding oriented; it should not become the owner of HP mutation, status rules, target selection, or damage formula.
- `InGameCombatManager` should remain the shared combat mutation/service coordinator during early phases.

## Target Responsibility Split

```text
MonsterUnitActor
  presentation, animation, debug labels, damage popup/view refresh hooks

MonsterUnitRuntimeModel / BaseUnitRuntimeModel
  identity, stats, resources, statuses, skill runtime state, auto flags

UnitSkillController
  one unit's skill-runtime ticking, auto/manual cast decision, cast request creation,
  owner-can-act checks, and actor animation notification request

SkillExecutionSystem / registry layer
  shared executor registry, snapshot/plan resolution, execution dispatch service

Skill behavior handlers / plan nodes
  cast conditions, targeting, damage modifiers, hit actions, kill actions,
  projectile behavior, status/buff handlers, visual requests

InGameCombatManager and services
  roster, damage/status/shield mutation, trigger dispatch, death cleanup,
  camera/global combat gates, shared scene service coordination
```

Dependency direction should remain from unit controller to shared execution services, not from executors back into actors or UI.

## Work Phases

### Phase 0: Lock Baseline And Select Regression Samples

Goal:

- Establish behavior and verification baseline before moving ownership.

Code Builder tasks:

- Confirm current `UnitSkillController` absence with source search.
- Identify at least one current skill/choice combination that exercises each behavior: execute threshold, boss damage multiplier, kill cooldown refund/reset.
- Record current data path from CSV to `SingleAttackData`, `SkillChoiceEffectSpec`, `SkillExecutionSnapshot`, and `SingleAttackSkillExecutor`.
- Run compile and CSV validation before refactor.

Acceptance:

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passes.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passes.
- Unity `Pakuri/Validate CSV Source Data` passes when Unity Editor is available.
- If Unity Editor is unavailable, record that limitation and provide build/code evidence.

### Phase 1: Add `UnitSkillController` As A Behavior-Preserving Shell

Goal:

- Introduce the per-unit owner without changing runtime behavior.

Suggested implementation surface:

- Add a new source file under a narrow runtime/ownership path, for example `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/UnitSkillController.cs`.
- The controller should wrap one `UnitRosterEntry`, its `BaseUnitRuntimeModel`, `UnitSkillRuntimeSet`, and required shared execution dependencies.
- First version may delegate execution to existing `SkillExecutionSystem` helpers or a small extracted internal route method.

Controller should own:

- ticking `model.SkillRuntime`;
- checking `entry.IsAlive`;
- checking `StatusEffectRuntime.CanAct(model)`;
- checking `model.AutoSkillEnabled`;
- iterating that unit's active skills;
- calling a supplied auto-route predicate;
- creating the execution request for one runtime.

Controller should not own:

- damage mutation;
- status mutation;
- CSV parsing;
- executor registry construction;
- actor prefab spawning;
- scene object lookup.

Acceptance:

- `SkillExecutionSystem.Tick(...)` still exists as the public global entry point.
- Internally it creates/uses `UnitSkillController` per roster entry or delegates each entry to one.
- No player-facing behavior changes.
- Existing manual skill execution still routes through the old public API until Phase 2.

### Phase 2: Move Auto And Manual Skill Decision Boundaries Into The Unit Controller

Goal:

- Make "this unit uses this skill" explicit.

Code Builder tasks:

- Move per-unit auto-route checks out of the central loop into `UnitSkillController`.
- Add a controller method for manual selected skill execution, while preserving `InGameCombatManager.HandleSelectedPlayerManualSkillInput()` as the input owner.
- Keep global camera/combat predicates in `InGameCombatManager` as injected policy, not hardcoded inside the controller.
- Ensure actor animation notification is requested from unit execution success, not from deep executor internals.

Acceptance:

- A single unit's skill tick can be inspected/tested without iterating the entire roster.
- `InGameCombatManager.Update()` no longer directly owns the full per-skill iteration details.
- Selected-player manual skill behavior remains unchanged.
- Auto mode behavior remains unchanged, including visible-enemy gating.

### Phase 3: Extract Current `SingleAttack` Execute/Boss/Kill Rules Into Handler Classes

Goal:

- Stop growing `SingleAttackSkillExecutor` with inline special rules while keeping current CSV fields.

Suggested new concepts:

- `ISkillCastCondition`
- `TargetHealthRatioCastCondition`
- `ISkillDamageModifier`
- `ExecuteDamageModifier`
- `BossDamageModifier`
- `ISkillPostHitAction`
- `KillCooldownRefundAction`
- `KillCooldownResetAction`

Important constraint:

- Do not change CSV headers in this phase.
- Do not remove current fields from `SingleAttackData`, `SkillChoiceEffectSpec`, or `SkillExecutionSnapshot` yet.
- Map current fields into handlers as an adapter.

Acceptance:

- `SingleAttackSkillExecutor` delegates execute-threshold cast rejection, execute damage/crit, boss damage, and kill cooldown recovery to local handlers.
- Current behavior remains unchanged.
- Handler tests or focused inspection show old values still flow from existing CSV columns.

### Phase 4: Group Flat Snapshot Behavior Into Operation Records

Goal:

- Reduce top-level snapshot growth.

Suggested grouped operations:

- `CastConditionOp`
- `DamageModifierOp`
- `CritModifierOp`
- `HitActionOp`
- `KillActionOp`
- `CooldownModifierOp`
- `StatusModifierOp`
- `ProjectileModifierOp`

Migration rule:

- Keep existing public snapshot properties as compatibility bridges until all current call sites are converted.
- New behavior should be added as operation records, not as another top-level `SkillExecutionSnapshot` property, unless it is truly global.

Acceptance:

- Existing execute/boss/kill behavior can be represented through grouped operations.
- Adding another conditional damage rule should not require a new direct property beside `BossDamageMultiplier` or `KillCooldownRefundRatioBonus`.

### Phase 5: Compile Current Data Into `SkillExecutionPlan`

Goal:

- Make skill behavior a plan compiled from current data, while keeping existing executors as compatibility handlers.

Suggested plan shape:

```text
SkillExecutionPlan
  CastConditions[]
  Targeting
  Actions[]
  DamageModifiers[]
  OnHitActions[]
  OnKillActions[]
  OnExpireActions[]
  Visuals[]
```

Code Builder tasks:

- Add a plan compiler that consumes current `SkillData`, choice specs, and snapshot data.
- Feed the plan to current executors gradually.
- Keep projectile, beam, single attack, zone, buff, shield, and passive executor routing stable.

Acceptance:

- Current CSV still imports without authoring changes.
- Current executor files still compile and run.
- New behavior can be added as a plan node or handler before adding a new wide CSV field.

### Phase 6: Normalize Authoring Only After Runtime Stabilizes

Goal:

- Avoid schema churn before the runtime can consume behavior nodes.

Do not do this first:

- Do not split current CSV files into new condition/action/modifier tables before Phase 5 works.
- Do not delete old columns until compatibility migration is proven.

Future normalized authoring may use:

- skill base rows;
- skill condition rows;
- skill action rows;
- skill modifier rows;
- skill trigger rows;
- skill visual rows.

Acceptance:

- Old CSV rows still load.
- New row-set authoring can coexist with old wide-column authoring.
- Runtime catalog sync and validation still pass.

### Phase 7: Cleanup, Board Updates, And Reviewer Gate

Goal:

- Close the refactor slice without leaving half-migrated ownership.

Code Builder tasks:

- Remove only adapter code that is no longer referenced.
- Update related board files.
- Run compile/build checks.
- Run Unity CSV validation/sync if CSV mapping or runtime catalog changes.
- Run Code Reviewer only with explicit user permission.

Acceptance:

- No untracked behavior migration remains undocumented.
- Related boards describe what changed and what still remains.
- User-owned Play Mode verification items are listed separately.

## Feedback To Give The Original Worker, Converted To Implementation Guidance

1. The report direction is correct, but implementation must start with behavior-preserving ownership extraction, not a CSV rewrite.
2. `UnitSkillController` should be introduced because unit skill state already exists on `BaseUnitRuntimeModel.SkillRuntime`, but unit skill decision ownership does not.
3. Current execute, boss, and kill-refund logic should be the first extraction sample because inspected code proves those rules are still wide CSV fields plus flat snapshot fields plus direct executor logic.
4. Projectile subclasses are not the right place for boss/execute/kill rules. Those are cast conditions, damage modifiers, and kill actions.
5. Buff/status handlers should remain for persistent unit-state changes. One-hit damage conditions should not be forced into statuses.
6. Existing CSV values must remain compatible until runtime plan/handler support is stable.

## Risk Areas

- Manual selected-skill input can regress if Phase 2 changes input ownership instead of only execution ownership.
- Auto mode can regress if visible-enemy gating or `AutoSkillEnabled` checks move without preserving current order.
- Snapshot migration can regress choices because many existing choice fields are already accumulated in `SkillExecutionSnapshot.Apply(...)`.
- Kill cooldown reset/refund must preserve exact precedence: reset currently returns before refund when `KillResetsCooldown` is valid.
- Boss and execute multipliers must preserve multiplication order and default `1f` fallback.
- CSV schema changes before Phase 5 would widen risk and should be avoided.

## Expected Verification From Code Builder

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`
- Unity `Pakuri/Validate CSV Source Data` if Unity Editor is connected.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` only if CSV mapping or runtime catalog assets change.
- Code inspection proving current execute/boss/kill sample behavior still maps from the same CSV rows after each phase.
- No Unity Play Mode gameplay verification by Codex; user owns gameplay feel verification.

## Related Boards To Update If Implementation Proceeds

- `boards/COMBAT/ENEMY_BLACKBOARD.md`: primary board for shared skill execution, executor routing, and `UnitSkillController` ownership changes.
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`: update only when buff/status handler behavior or status modifier runtime changes.
- `boards/DATA/DATA_BLACKBOARD.md`: update only when CSV schema, parser, validation, source CSV, or runtime catalog sync behavior changes.
- `boards/RUN/RUN_BLACKBOARD.md`: update only if NewRunScene runtime flow, Offering, or run ownership changes directly.

## Non-Goals

- No code implementation in this Designer handoff.
- No CSV schema change in the first implementation step.
- No Play Mode claim from Codex.
- No Code Reviewer execution without explicit user permission.
