# Buff Base Skill Blueprint

## Core Rule

Create one new Buff base skill only with the existing Buff CSV contract. The exact user-provided skill Reference MD is the only semantic input.

Use `Shield` when the Base effect grants a calculated shield. Use `Buff` when it applies an existing status or stat bonus without standalone enemy damage. If the Reference mixes behavior outside this contract, stop.

## Required Reference Facts

Use the `monster_id`, slot, and `skill_id` derived from the Reference path by `GAMEBULIDER_SKILL.md`. Extract the display name, description, runtime kind, cooldown, status or shield label, duration, target scope, merge behavior, and exact shield/damage coefficients or stat bonuses used by the effect.

For `Shield`, require the shield formula and refresh rule when the Reference defines one. For `Buff`, require each bonus, chance, stack amount, and max stacks that the described behavior needs. Do not use Trait, Master, or Awakening sections during the Base pass.

The existing family contract permits these narrow defaults when the Reference describes a guaranteed, single-stack, finite-duration application: `status_merge_policy=same_source_refresh`, `status_chance=1`, `status_max_stacks=1`, and `status_stack_amount=1`. Map all-allies and self targets to the existing `all_allies` and `self` tokens. Any other omitted policy or value causes a stop.

## Allowed Files

Read and edit only:

- this blueprint;
- the exact user-provided Reference MD;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/buff/skills_buff.csv`;
- the uniquely matching row of `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv` when a status label needs an id;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/buff/buff_skill_triger.csv` only when the Base description explicitly requires an event trigger.

Do not inspect another Reference, another family, graph rows, runtime code, prefabs, scenes, or assets.

## Authoring Rules

- add exactly one row to `skills_buff.csv`;
- use only `Buff` or `Shield` and existing columns;
- resolve a status id only by one exact `status_effect_label` match;
- use only the documented narrow defaults above; do not invent another merge, stack, target, visual, or prefab value;
- leave optional asset fields empty only when the schema and targeted validation accept them;
- if the Base requires damage, multiple effects, a Skill-owned graph, or a new status definition, stop.

## Verification

Confirm the derived identity, runtime kind, target scope, status/shield values, duration, cooldown, CSV column count, unique skill id, UTF-8 encoding, and targeted CSV validation. The diff must contain only the new Base row and an explicitly required direct trigger row.
