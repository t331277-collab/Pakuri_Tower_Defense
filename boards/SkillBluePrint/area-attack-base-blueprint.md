# AreaAttack Base Skill Blueprint

## Core Rule

Create one new AreaAttack-family base skill with the existing AreaAttack CSV contract and, only for an explicit secondary effect, an existing Skill-owned Effect graph. The exact user-provided skill Reference MD is the only semantic input.

Use `AreaAttack` for the Reference format represented by sustained range/field damage with duration and tick interval. Use `Field` only when the Reference explicitly identifies that existing runtime contract; do not infer `Field` from the word "field" alone.

## Required Reference Facts

Use the `monster_id`, slot, and `skill_id` derived from the Reference path by `GAMEBULIDER_SKILL.md`. Extract the display name, description, damage attribute, per-hit or per-tick base damage and coefficients, radius, target count when limited, cooldown, active duration, tick interval, critical rule, and status chance/stacks/duration.

Do not use Trait, Master, or Awakening sections during the Base pass.

## Allowed Files

Read and edit only:

- this blueprint;
- the exact user-provided Reference MD;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/area_attack/skills_area_attack.csv`;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/area_attack/skill_graph_nodes_area_attack.csv` only when the Base needs a secondary Skill-owned Effect graph;
- the uniquely matching status row in `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv` when a label needs an id;
- matching rows from `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv` and `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv` only for selected Skill graph nodes.

Do not inspect another Reference, another family, unrelated rows, runtime code, prefabs, scenes, or assets.

## Authoring Rules

- add exactly one Base row using `AreaAttack`, or `Field` only when explicit;
- use `owner_kind=Skill` and the derived skill id only for an explicit secondary Effect graph;
- every Effect graph has exactly one operation node and unique `node_order` values;
- resolve status ids by one exact label match;
- do not invent asset paths or copy another skill's values;
- stop when the Base requires an event trigger because no AreaAttack trigger CSV exists;
- stop if existing columns and registered nodes cannot express the Base behavior.

## Verification

Confirm identity, runtime kind, damage formula, radius, duration, tick interval, status rules, Skill ownership, operation-node count, CSV column counts, unique ids/orders, UTF-8 encoding, and targeted CSV validation. The diff must contain only the new Base row and its explicitly required Skill graph.
