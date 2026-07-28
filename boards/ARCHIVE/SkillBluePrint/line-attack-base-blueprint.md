# LineAttack Base Skill Blueprint

## Core Rule

Create one new `LineAttack` base skill only with the existing LineAttack CSV contract. The exact user-provided skill Reference MD is the only semantic input.

## Required Reference Facts

Use the `monster_id`, slot, and `skill_id` derived from the Reference path by `GAMEBULIDER_SKILL.md`. Extract the display name, description, damage attribute, per-tick base damage and coefficients, beam width, active duration, tick interval, cooldown, critical rule, and any status chance, stacks, duration, or knockback.

The Reference must distinguish total damage from per-tick damage and duration from tick interval. Do not use Trait, Master, or Awakening sections during the Base pass.

## Allowed Files

Read and edit only:

- this blueprint;
- the exact user-provided Reference MD;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/line_attack/skills_line_attack.csv`;
- the uniquely matching status row in `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv` when a label needs an id;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/line_attack/line_attack_skill_triger.csv` and Trigger-owned rows in `choices/line_attack/skill_graph_nodes_line_attack.csv` only for an explicit event trigger;
- matching rows from `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv` and `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv` only when that trigger graph needs metadata.

Do not inspect another Reference, another family, unrelated rows, runtime code, prefabs, scenes, or assets.

## Authoring Rules

- add exactly one `runtime_kind=LineAttack` row;
- map beam width to `radius`, duration to `active_duration_seconds`, and tick period to `shot_interval_seconds` only when those meanings are explicit;
- resolve a status id only by one exact label match;
- do not invent asset paths; leave optional asset fields empty only when validation accepts them;
- do not author a Skill-owned Base graph on this route; stop if the Base behavior cannot fit the row plus an explicit trigger.

## Verification

Confirm identity, per-tick formula, width, duration, tick interval, cooldown, status values, CSV column count, unique skill id, conditional trigger ownership, UTF-8 encoding, and targeted CSV validation. The diff must contain only the new Base row and an explicitly required trigger bundle.
