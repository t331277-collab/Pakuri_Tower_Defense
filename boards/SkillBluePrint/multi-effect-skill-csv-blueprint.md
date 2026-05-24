# Multi-Effect Skill CSV Blueprint

## Purpose

This document is the implementation contract for skills that need more than one runtime effect from CSV data.

Use this blueprint when a base skill already fits a shared executor, but its full reference behavior also needs additional damage, buff, debuff, delayed wave, conditional target, or choice-gated effect rows.

This blueprint exists because the current `SingleAttack` blueprint explicitly treats bundled ally effects and repeated pulses as stop-and-ask behavior. The reusable answer is a CSV-owned effect table plus a shared executor helper, not a monster-specific branch.

## Core Rule

Do not hardcode monster IDs, skill IDs, choice IDs, prefab paths, or special effect values in executors.

All effect behavior must come from parsed CSV rows.

If a requested effect cannot be represented by the multi-effect CSV schema below, Builder must stop and ask whether to extend the schema.

## Inspected Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` owns the base skill row, while `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` exists as the reusable secondary-effect table.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:351` stores skill-owned `SkillEffectDefinition[] MultiEffects`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.cs:23` names `monster_skill_effects.csv` as the runtime multi-effect source file.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:65-66` executes the base `SingleAttack` hit, then calls `SkillMultiEffectExecutor.Execute(...)` with the resolved primary center.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs:11-13` owns the shared multi-effect executor helper.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SupportSkillExecutors.cs:8` and `:140` contain the shared `BuffSkillExecutor` / `ShieldSkillExecutor` support paths for ally effects.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` currently marks several Ariel-C rows as `DataOnlyUnsupported`, which proves the existing base skill plus choice modifier columns are not enough for full Ariel-C behavior.

## CSV Ownership

Use the source CSV:

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv`

The table owns secondary or replacement effects for active skills. It should not replace ordinary fields in `monster_skills.csv`; it only describes additional effect rows that a skill executor can play after the base effect.

Recommended columns:

- `effect_id`
- `skill_id`
- `sort_order`
- `effect_kind`
- `target_side`
- `target_selection`
- `target_shape`
- `center_mode`
- `visual_anchor_mode`
- `effect_timing`
- `delay_seconds`
- `enabled_by_default`
- `requires_active_choice_id`
- `excludes_active_choice_id`
- `condition_status_id`
- `condition_target_side`
- `attribute`
- `base_damage`
- `attack_power_coefficient`
- `spell_power_coefficient`
- `damage_multiplier`
- `radius`
- `cover_all`
- `status_effect_id`
- `status_chance`
- `status_effect_label`
- `status_duration_seconds`
- `status_max_stacks`
- `status_stack_amount`
- `status_target_scope`
- `status_merge_policy`
- `shield_amount_refresh_policy`
- `status_action_speed_bonus`
- `status_move_speed_bonus`
- `status_attack_power_bonus`
- `status_spell_power_bonus`
- `status_damage_bonus_rate`
- `status_damage_taken_bonus`
- `status_critical_damage_taken_bonus`
- `status_critical_resistance_bonus`
- `status_element_resist_reduction`
- `status_element_damage_taken_bonus`
- `skill_effect_prefab_path`
- `runtime_support_state`
- `runtime_support_notes`

The CSV must keep the project convention of a header row followed by a type row.

## Supported First Implementation Scope

The first implementation should support:

- effect kind `Damage`
- effect kind `Status`
- target side `Enemy`, `Self`, and `AllAllies`
- target shape `Circle` and `Battlefield`
- center mode `EffectTarget`, `PrimarySkillCenter`, `Caster`, and `NearestEnemy`
- visual anchor mode `Center` and `AppliedTargets`
- timing `OnCast` and delayed execution via `delay_seconds`
- choice gates through `requires_active_choice_id` and `excludes_active_choice_id`
- target filtering through `condition_status_id`
- effect-local prefab path through the runtime asset catalog
- center-based visuals and applied-target attached visuals through the same effect-local prefab path
- status spell power bonus through shared status runtime data
- outgoing element damage bonus through shared status runtime data

The first implementation may execute delayed effects through a runtime coroutine/helper owned by the shared executor layer. It should still be driven only by CSV rows.

## Ariel-C Data Shape

For Ariel-C, use effect rows instead of executor branches:

- base skill row keeps the common `SingleAttack` enemy damage.
- default ally blessing/action-speed row is enabled unless a replacing master choice excludes it.
- ally blessing rows keep `target_side=AllAllies` for effect application, while `visual_anchor_mode=AppliedTargets` can attach a buff prefab to each successfully affected ally.
- damage wave rows can use `center_mode=PrimarySkillCenter` so delayed or secondary waves stay centered on the base `SingleAttack` target point instead of reselecting a different unit.
- trait rows can be represented as gated replacement/additional status rows.
- master 1 replaces the default ally buff with a spell-power buff row.
- master 2 adds a delayed second enemy damage row at the authored damage multiplier.
- shielded-ally Holy damage support is represented as a conditional ally status row with `condition_status_id=shield`.

This keeps future similar skills reusable: add rows to `monster_skill_effects.csv`, not C# branches.

## Builder Implementation Surface

Expected Builder changes for future extensions:

- extend runtime data classes/enums for new skill effect definition fields;
- preserve `SkillDefinition.MultiEffects` as the skill-owned multi-effect container;
- extend parsing and validation for `monster_skill_effects.csv` when new columns are needed;
- include any new effect prefab paths in the CSV runtime asset catalog;
- map parsed multi-effects into transient `SkillData`;
- extend the shared `SkillMultiEffectExecutor` helper instead of adding monster-specific executor branches;
- call the helper from the relevant base executor after the base hit or cast, following the existing `SingleAttackSkillExecutor` pattern;
- keep the helper generic enough for other executors to call later.

Do not add Ariel-specific executor code.

## Stop And Ask Rule

Builder must stop if a requested effect needs behavior outside this contract, including:

- projectile spawning from a multi-effect row;
- target search by custom non-status predicates;
- stacking rules that cannot be expressed by existing status merge policies;
- per-target sequence state such as first/third/last target;
- a new stat bonus type not represented in status runtime data.

## Verification Expected From Builder

Builder must provide:

- targeted CSV parse evidence for the new table;
- runtime/editor `dotnet build` results;
- Unity CSV catalog sync or a clear reason it could not run;
- console/error inspection when Unity is available;
- a note that Play Mode gameplay verification remains user-owned.
