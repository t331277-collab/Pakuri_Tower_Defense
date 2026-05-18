# BeamSkill Blueprint For InGame LineAttack Skills

## Purpose

This document is the first-read implementation contract for InGame BeamSkill / LineAttack skills.

It is not a replacement for code inspection. Its role is to reduce the amount of code a Code Builder must inspect before implementing another beam, laser, ray, slash-line, or straight line tick skill. New BeamSkill work should start from this document, then verify the listed files and the specific skill data being changed.

## Numeric Evidence Priority

Do not invent BeamSkill numbers when the user does not provide exact numeric evidence.

When BeamSkill implementation needs numeric values, inspect evidence in this order:

1. Active CSV data first.
   - Monster skill runtime source: `Pakuri/Assets/CSVdata/source/monster_skills.csv`
   - Monster skill choice runtime source: `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`
   - Monster reward choice runtime source: `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv`
   - Shared runtime modifier source: `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv`

2. Runtime script mapping next, when the CSV field meaning is unclear.
   - `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`
   - `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`
   - `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`

3. Original reference documents last, when CSV data does not contain the needed value.
   - Monster original data: `Pakuri/reference/2.Monster`

If none of these files contains the value, state that the value is missing and ask for a design decision. Record which file supplied each non-obvious tuning value, especially when a mapper or executor needs a skill-ID-specific exception because the current CSV schema has no matching field.

## Current Common Beam / LineAttack Path

The current InGame BeamSkill path is the `Scripts2/InGame` path.

1. Data defines the skill.
   - `Pakuri/Assets/CSVdata/source/monster_skills.csv`
   - `runtime_kind=LineAttack` maps to BeamSkill runtime data.
   - Current relevant fields include `base_damage`, coefficients, `radius`, `cooldown_seconds`, `shot_interval_seconds`, status fields, and `active_duration_seconds`.

2. Skill data is mapped into runtime data.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`
   - `SkillRuntimeKind.LineAttack` maps to `BeamSkillData`.
   - `source.Radius` maps to `BeamSkillData.BeamWidth`.
   - `source.ActiveDurationSeconds` maps to `skill.Timing.ActiveDuration`.
   - `source.ShotIntervalSeconds` maps to `skill.Timing.TickInterval`.
   - `BeamSkillData.BeamLength` is currently mapped to `0f`, so runtime resolves length from battlefield boundary or default length.
   - Damage and on-hit status map to `BeamSkillData.DamagePerTick` and `BeamSkillData.OnHitStatus`.

3. Learned active skills become runtime instances.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeFactory.cs`
   - `SkillRuntimeFactory.RebuildLearnedActiveSet(...)` adds learned active skills to `UnitSkillRuntimeSet`.

4. Runtime gating checks whether the skill can cast.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs`
   - `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs`
   - `CanCastWithSnapshot(...)` checks cooldown, cast state, reload state, magazine, and cast interval.
   - For non-magazine BeamSkill rows, `shot_interval_seconds` currently affects the cast interval through `SkillRuntimeInstance` and the line tick interval through `BeamSkillExecutor`.

5. The Beam executor resolves target direction and creates or routes the line attack.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutorRegistry.cs`
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs`
   - `BeamSkillExecutor` resolves origin, manual or nearest-target direction, damage, status, line length, width, duration, tick interval, and prefab.
   - If no prefab or `EffectManager` is available, it immediately routes one line tick without a persistent visual actor.

6. LineAttack actor ticks damage while alive.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs`
   - `Initialize(...)` configures visual scale, then applies an immediate line tick.
   - `Update()` repeats line ticks on interval while duration remains.
   - Each tick hits each candidate unit at most once for that tick, applies damage, then applies status by chance.
   - The actor destroys itself when duration ends or when its combat manager is missing.

7. Combat manager owns shared combat APIs.
   - `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`
   - Relevant APIs: `ApplyDamage(...)`, `ApplyStatus(...)`, `ResolveProjectileDestroyBoundaryX()`, and skill effect instantiation through `EffectManager`.

## Supported / Partial / Unsupported Matrix

| Behavior | Current status | Evidence / implementation note |
|---|---|---|
| `LineAttack` runtime kind mapping to BeamSkillData | Supported | `InGameSkillDefinitionMapper.CreateConcreteActiveSkill(...)` returns `BeamSkillData` for `SkillRuntimeKind.LineAttack`. |
| CSV-driven damage per tick | Supported | `MapDamage(beam.DamagePerTick, source)` maps base damage, attribute, and coefficients. |
| CSV-driven width | Supported | `beam.BeamWidth = source.Radius`; executor clamps width to at least `0.1f`. |
| CSV-driven active duration | Supported | `active_duration_seconds` maps to `Timing.ActiveDuration`; executor uses it when > 0. |
| CSV-driven tick interval | Supported | `shot_interval_seconds` maps to `Timing.TickInterval`; executor falls back to `0.1f`. |
| Immediate first tick | Supported | `InGameLineAttackActor.Initialize(...)` calls `ApplyLineTick(...)` before the first `Update()`. |
| Repeated tick damage | Supported | `InGameLineAttackActor.Update()` repeats `ApplyLineTick(...)` while duration remains. |
| On-hit status chance | Supported | `BeamSkillData.OnHitStatus` maps from CSV status fields and `TryApplyStatus(...)` calls `InGameCombatManager.ApplyStatus(...)`. |
| Manual 1P A-skill direction | Partial | `BeamSkillExecutor` accepts `context.HasManualAimDirection`; broader per-skill manual input is still owned by `SkillExecutionSystem` / combat input flow. |
| Auto target nearest enemy | Partial | Executor uses `SkillExecutionUtility.FindNearestTarget(...)`; no priority rules beyond current utility behavior are guaranteed. |
| Length to battlefield boundary | Partial | `ResolveBeamLength(...)` uses `ResolveProjectileDestroyBoundaryX()` only when direction has a meaningful X component; otherwise it falls back to `31f`. |
| Prefab visual scaling | Partial | `InGameLineAttackActor.ConfigureVisual()` scales a `SpriteRenderer` by line length and width. Complex child effects are not proven to scale correctly. |
| Fallback no-prefab behavior | Partial | Without prefab or `EffectManager`, executor applies a single tick and returns; no persistent visual or repeated ticking occurs. |
| Choice damage multiplier | Supported | `SkillExecutionUtility.ResolveDamage(...)` consumes snapshot damage/base-damage modifiers. |
| Choice shot interval multiplier | Partial | Runtime gating and executor tick interval can receive multiplier effects through snapshot/runtime, but confirm the current execution path before claiming a specific trait is fully supported. |
| Choice duration multiplier / duration bonus | Partial | `SkillExecutionSnapshot` stores duration modifiers, but inspected `BeamSkillExecutor.ResolveDuration(...)` currently reads `skill.Timing.ActiveDuration` directly. Do not claim Beam duration modifiers work until this is verified or implemented. |
| Choice radius/width multiplier | Unsupported in current Beam executor | `SkillExecutionSnapshot` stores radius modifiers, but inspected `BeamSkillExecutor` uses `skill.BeamWidth` directly. |
| Stop at first target | Unsupported in current executor | `BeamSkillData.StopAtFirstTarget` exists, but `InGameLineAttackActor.ApplyLineTick(...)` does not read it. |
| Pushback / knockback | Unsupported in current line actor | Damage/status are applied, but no displacement API is invoked. |
| Resistance reduction from a Beam hit | Unsupported as common behavior | Status can apply, but direct resistance reduction like Eve-B master wording needs an implemented status/damage-layer contract. |
| Multi-segment, forked, reflected, or chained beams | Unsupported | Current actor owns one origin, direction, length, and width. |
| Curved or sweeping beams | Unsupported | Current hit test is a static rectangular line projection per tick. |
| Per-target persistent hit cooldown across ticks | Unsupported | Duplicate prevention is per tick only through a local `HashSet`; the same target can be hit again on the next tick. |
| Ground warning / charge delay / telegraph | Unsupported | `BeamSkillExecutor` instantiates immediately and `InGameLineAttackActor.Initialize(...)` immediately applies damage. |

## Special Behavior Rule

Do not assume special BeamSkill behavior is supported just because it is described in a monster reference file or CSV row.

The following behavior must be implemented as an explicit exception or as a deliberate reusable extension before a skill can rely on it:

- Beam width or duration modifiers from choice data.
- Stop at first target.
- Knockback or pushback line attacks.
- Resistance reduction or other non-status debuffs on line hit.
- Multi-segment, forked, reflected, chained, curved, or sweeping beams.
- Charge-up, warning zone, delayed damage, or telegraph phases.
- Persistent per-target hit cooldown independent of tick interval.
- Beam visuals that require child hitboxes or non-sprite scaling.

If a special behavior will be reused by several skills, prefer a new shared BeamSkill extension point rather than a monster-only hardcoded branch. If the behavior is unique and urgent, record it as a deliberate exception with the owning skill ID, affected files, and Play Mode acceptance criteria.

## New BeamSkill Checklist

Before implementing a new BeamSkill / LineAttack skill:

1. Confirm the skill row exists in data.
   - Check `Pakuri/Assets/CSVdata/source/monster_skills.csv`.
   - Confirm `runtime_kind=LineAttack`.
   - Confirm width source (`radius`), duration source (`active_duration_seconds`), tick source (`shot_interval_seconds`), damage fields, and status fields.

2. Confirm the mapped runtime type.
   - `LineAttack` should map to `BeamSkillData`.
   - If it maps to `ProjectileSkillData`, `ZoneSkillData`, `ShieldSkillData`, or `BuffSkillData`, do not implement it through `InGameLineAttackActor` without a design decision.

3. Confirm prefab ownership.
   - Check whether the skill has `SkillEffectPrefab` or `EffectManager` mapping.
   - Current scene evidence: `NewRunScene.unity` maps monster `eve-b` to `Assets/Prefab/Skill/Eve/Eve_B.prefab`.
   - `BeamSkillExecutor` can route one no-prefab tick, but persistent visuals and repeated ticks require an instantiated actor.

4. Confirm status behavior.
   - Supported statuses are centralized in `StatusEffectKind` and applied through `InGameCombatManager.ApplyStatus(...)`.
   - Add new status kinds before relying on new runtime status names.
   - Do not claim resistance reduction, knockback, or damage amplification exists unless the relevant runtime service implements it.

5. Confirm modifier behavior.
   - `SkillChoiceModifierData.csv` may contain damage, shot interval, radius, and duration rows.
   - Before claiming a modifier works for BeamSkill, inspect whether `BeamSkillExecutor` consumes the corresponding `SkillExecutionSnapshot` field.
   - At the time this document was written, damage modifiers are supported, shot interval is partial, and radius/duration modifiers are not consumed directly by `BeamSkillExecutor`.

6. Decide whether the skill is common or exceptional.
   - Common: static straight line, nearest/manual direction, immediate first tick, repeated tick damage/status, width from CSV radius, duration from CSV active duration.
   - Exceptional: anything involving charge delay, knockback, stop-first-target, resistance reduction, width/duration modifiers, sweeping/curved/forking line shapes, or custom per-target tick rules.

## Recommended Extension Points

| Need | Recommended direction |
|---|---|
| Width modifiers | Apply `snapshot.RadiusMultiplier` / `RadiusBonus` when resolving Beam width, and document accepted CSV fields. |
| Duration modifiers | Apply `snapshot.DurationMultiplier` / `DurationBonus` when resolving Beam duration, and verify runtime `ActiveDurationRemaining` display if used. |
| Stop at first target | Make `InGameLineAttackActor.ApplyLineTick(...)` honor `BeamSkillData.StopAtFirstTarget` or a new hit policy enum. |
| Knockback / pushback | Add a shared movement/displacement service call after damage/status instead of embedding monster-specific movement in the actor. |
| Resistance reduction | Add a status/effect type that the damage layer can read, or a clear stat-modifier runtime service. |
| Charge delay / telegraph | Add a delayed activation state to the actor, separating visual warning from damage ticking. |
| Curved/sweeping beam | Add a new actor or shape mode; do not overload the current rectangular line projection silently. |

## Eve-B Current Evidence Summary

Eve-B is the current reference implementation for the BeamSkill path.

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` defines `eve-b` as `LineAttack`, `RuntimeImplemented`, display name `프리즘 레이`, base damage `12`, spell coefficient `1.6`, width/radius `3.2`, cooldown `6.5`, tick interval `0.15`, status `slow`, status chance `0.2`, and active duration `1.2`.
- `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` defines Eve-B trait/master rows with damage, shot interval, and duration wording.
- `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` contains Eve-B modifier rows; some are marked `PartialRuntimeSupport` or `DataOnlyUnsupported`.
- `InGameSkillDefinitionMapper` maps `LineAttack` to `BeamSkillData`.
- `SkillExecutorRegistry` registers `BeamSkillExecutor`.
- `BeamSkillExecutor` creates or routes a line attack and attaches `InGameLineAttackActor` when a prefab is instantiated.
- `InGameLineAttackActor` applies immediate and interval line ticks.
- `NewRunScene.unity` maps `eve-b` to `Assets/Prefab/Skill/Eve/Eve_B.prefab`.

## Verification Expected From Code Builder

For documentation-only changes:

- Run a targeted markdown/file existence check.
- Do not run Play Mode.

For code changes implementing BeamSkill behavior:

- Run `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`.
- Run `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` when editor scripts or Unity serialization may be affected.
- Refresh Unity scripts if Unity is available, then check console errors/warnings.
- Record whether the requested BeamSkill behavior is common, partial, or exceptional.
- Leave Play Mode gameplay verification to the user.
