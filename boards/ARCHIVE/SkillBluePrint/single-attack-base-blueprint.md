# SingleAttack Base Skill Blueprint

## Core Rule

Create one new `SingleAttack` base skill with the existing SingleAttack row contract and, only for an explicit secondary effect, an existing Skill-owned Effect graph. The exact user-provided skill Reference MD is the only semantic input.

## Required Reference Facts

Use the `monster_id`, slot, and `skill_id` derived from the Reference path by `GAMEBULIDER_SKILL.md`. Extract the display name, description, damage attribute, base damage and coefficients, target selection or battlefield coverage, radius or target count, cooldown, critical rule, and every status/condition/consume rule stated by the Base behavior.

If the same cast also creates a secondary effect such as an allied shield, extract its operation, exact payload, target, duration, and timing. Do not use Trait, Master, or Awakening sections during the Base pass.

Qualitative payloads or missing target/timing values cause a stop.

## Allowed Files

Read and edit only:

- this blueprint;
- the exact user-provided Reference MD;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/single_attack/skills_single_attack.csv`;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/single_attack/skill_graph_nodes_single_attack.csv` only when the Base needs a secondary Skill-owned Effect graph;
- the uniquely matching status row in `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv` when a label needs an id;
- matching rows from `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv` and `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv` only for the selected Skill graph nodes;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/single_attack/single_attack_skill_triger.csv` and Trigger-owned graph rows only for an explicit event trigger.

Do not inspect another Reference, another family, unrelated rows, runtime code, prefabs, scenes, or assets.

## Authoring Rules

- add exactly one `runtime_kind=SingleAttack` Base row;
- map an explicit battlefield-wide one-shot target to the existing `hit_target_count=global` contract; do not infer global targeting from visual wording alone;
- use `owner_kind=Skill`, the derived skill id as `owner_id`, and `graph_kind=Effect` for a secondary Base effect;
- every Effect graph has exactly one operation node and unique `node_order` values;
- resolve status ids by one exact label match;
- do not invent asset paths or copy another skill's graph;
- if existing columns and registered nodes cannot represent the Reference, stop.

## Verification

Confirm identity, damage formula, targeting, cooldown, status rules, Skill graph ownership, operation-node count, argument order, CSV column counts, unique ids/orders, UTF-8 encoding, and targeted CSV validation. The diff must contain only the new Base row and its explicitly required Skill/Trigger graph bundle.
