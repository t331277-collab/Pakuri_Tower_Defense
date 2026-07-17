# Passive Base Skill Node Blueprint

## Core Rule

Create one new Passive base skill with the existing Passive metadata row and Skill-owned node graph. The exact user-provided skill Reference MD is the only semantic input.

Passive behavior is node-owned: the Base row identifies the skill, while `owner_kind=Skill` graph rows express its effect. Event-driven behavior additionally uses the Passive trigger CSV.

## Required Reference Facts

Use the `monster_id`, slot, and `skill_id` derived from the Reference path by `GAMEBULIDER_SKILL.md`. Extract the display name, description, every passive operation, exact numeric values, affected targets, conditions, status/attribute filters, effect duration when required, timing, and trigger event when present.

Do not use Trait, Master, or Awakening sections during the Base pass. A qualitative effect or missing condition/target/payload causes a stop.

## Allowed Files

Read and edit only:

- this blueprint;
- the exact user-provided Reference MD;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/passive/skills_passive.csv`;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/passive/skill_graph_nodes_passive.csv` for rows owned by the new Skill or its explicit Trigger;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/passive/passive_skill_triger.csv` only for event-driven Base behavior;
- matching rows from `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv` and `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv` only for selected nodes;
- the uniquely matching status row in `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv` when a label needs an id.

Do not inspect another Reference, another family, unrelated rows, runtime code, prefabs, scenes, or assets.

## Authoring Rules

- add exactly one metadata row to `skills_passive.csv`;
- use `owner_kind=Skill`, the derived skill id as `owner_id`, and `graph_kind=Effect` for the Base passive graph;
- use a Trigger row plus `owner_kind=Trigger` graph only when the Reference explicitly defines an event;
- each Effect graph has exactly one operation node and unique `node_order` values;
- resolve status ids by one exact label match;
- do not create node types, parameters, schema columns, asset paths, or runtime code;
- stop if registered nodes cannot express the Reference.

## Verification

Confirm identity, every Reference value, Skill/Trigger ownership, graph links, operation-node count, node argument order, unique graph orders, CSV column counts, UTF-8 encoding, and targeted CSV validation. The diff must contain only the new Passive row and its Skill/Trigger-owned graph bundle.
