# Enhancement And Master Skill Node Blueprint

## Core Rule

Use this blueprint only to create or change an Enhancement or Master Choice for an existing base skill.

Read the exact skill Reference MD path provided by the user, compose the requested behavior from existing node types, and edit only the target Choice, graph, and explicitly required trigger rows.

If the request needs missing values, a base-skill change, a new node/schema, runtime code, a prefab/scene edit, or unrelated data, stop and report the boundary.

## Reference Input And Stop Rule

The user provides one exact skill Reference MD path. That file is the only semantic input; do not require separately parsed ids, values, node data, or asset paths.

Read that file and extract:

- `monster_id` from the monster directory and the slot from the Reference filename prefix;
- `skill_id` as `<monster_id>-<lowercase slot>`, after confirming the path, title, and Basic Information do not conflict;
- the runtime family from the Base behavior described by the Reference;
- Enhancement rows only from the Trait table and Master rows only from the Master Skill table;
- `choice_id` as `<skill_id>-trait-<1-based row>` or `<skill_id>-master-<1-based row>`;
- Choice title, description, requested behavior, and exact numeric values;
- conditions, targets, and timing required by that behavior;
- referenced status labels and exact repository paths only when the behavior needs them.

Do not author the Awakening table under this blueprint. Do not require irrelevant fields. A numeric modifier does not need prefab data unless the Reference explicitly supplies a repository path for a visual or spawned object.

Use `ActiveEnhancement` and `ActiveMaster` only for an active Base skill. For another group, use only a group already supported by the selected family CSV; otherwise stop.

Before editing, confirm the derived base skill resolves exactly once in the selected family and that every requested new Choice id is absent. A conflicting identity, multiple family matches, or an existing row with different content means stop.

If any value required to author the requested effect is absent or ambiguous, stop before editing and list the missing values. Phrases such as "small damage" or "after a delay" are insufficient without a numeric payload or time. Do not search another Reference, linked Obsidian document, board, history, old implementation, or broad code.

## Allowed Reads And Edits

After mandatory startup and role documents, read only:

- this blueprint;
- the exact user-provided skill Reference MD;
- the derived skill's single row in `Pakuri/Assets/CSVdata/authoring/monster/skills/base/<family>/skills_<family>.csv`, read-only, to confirm the Base exists and the family is unique;
- one matching `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/<family>/skill_choices_<family>.csv`;
- one matching `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/<family>/skill_graph_nodes_<family>.csv`;
- one matching `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/<family>/<family>_skill_triger.csv` only when the Reference explicitly requires an event trigger;
- the uniquely matching row of `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv` only when a Reference status label needs an id;
- matching rows from `skill_node_definitions.csv` and `skill_node_definition_params.csv` only when node metadata was not provided.

The node-definition files are under:

- `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/`.

Edit only the requested Choice row, its graph rows, and an explicitly required trigger row with its Trigger-owned graph. Do not edit the Base row or read another Base row, another Choice, runtime code, prefabs, scenes, catalogs, or unrelated rows.

## Node Composition Rules

- use `owner_kind=Choice`, the requested Choice id as `owner_id`, and the existing base skill as `target_skill_id`;
- use `graph_kind=Plan` to modify existing skill values, conditions, or event behavior;
- use `graph_kind=Effect` only when the Choice creates a new effect bundle;
- use a matching trigger row plus `owner_kind=Trigger` graph only when the Reference explicitly defines an event such as OnHit, OnKill, or OnExpire;
- keep `node_order` unique inside each graph;
- reuse registered `node_type_id` values and their defined argument order;
- resolve a status id only when `status_effects.csv` contains one exact `status_effect_label` match; zero or multiple matches means stop;
- do not invent prefab, sprite, animator, or other asset paths when the Reference contains no exact repository path;
- do not edit node definitions or parameters under this blueprint.

An Effect graph must contain exactly one operation node:

- `ApplyStatus`, `ApplyShield`, `StatusModifier`, `EffectStatus`, `EffectDamage`, `RecastZone`, or `EffectExtendStatusDuration`.

Other Effect nodes may define target, condition, lifetime, payload, or visual. If existing node types cannot represent the reference behavior, stop.

## Implementation Steps

1. Read the Reference MD and check required values.
2. Select exactly one runtime family and route its Choice and graph CSV files; add its trigger CSV only when the Reference requires an event.
3. Select the minimum existing node types that express the requested behavior.
4. Add or update only the target Choice row and its owned graph rows.
5. Preserve all unrelated rows and file formatting.

## Verification

- confirm every authored value matches the Reference MD;
- confirm Choice ownership, target skill, graph kind, unique node order, node type, and argument order;
- confirm each Effect graph has exactly one operation node;
- confirm every trigger event, Choice gate, and Trigger-owned graph link matches the Reference;
- run the existing targeted CSV validation when available;
- confirm the diff contains only the routed Choice and graph rows.

Do not run runtime/editor builds for this data-only path. Report changed rows and validation results, or the exact missing values that caused a stop.
