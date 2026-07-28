# Projectile Base Skill Blueprint

## Core Rule

Create one new Projectile base skill only with the existing Projectile CSV contract. The exact user-provided skill Reference MD is the only semantic input.

Use `MagazineProjectile` when the Reference defines a magazine and reload cycle. Use `CooldownProjectile` when it defines a cooldown-driven projectile without a magazine. If neither mapping is exact, stop.

## Required Reference Facts

Use the `monster_id`, slot, and `skill_id` derived from the Reference path by `GAMEBULIDER_SKILL.md`. Extract the display name, description, damage attribute, base damage and coefficients, projectile speed, projectile count and interval, pierce count, firing cadence, critical rule, and either magazine/reload values or cooldown.

Extract status label, chance, stacks, duration, target, radius, or delayed damage only when the Reference defines them. Do not use the Trait, Master, or Awakening sections during the Base pass.

Missing or qualitative values such as "small damage", "fast", or "after a delay" are not authoring values. Stop and list them.

## Allowed Files

Read and edit only:

- this blueprint;
- the exact user-provided Reference MD;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/projectile/skills_projectile.csv`;
- the uniquely matching row of `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv` only when a status label needs an id;
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/projectile/projectile_skill_triger.csv` and Trigger-owned rows in `choices/projectile/skill_graph_nodes_projectile.csv` only when the Base description explicitly requires an event trigger;
- matching rows from `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv` and `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv` only when that trigger graph requires node metadata.

Do not inspect another Reference, another family, unrelated rows, runtime code, prefabs, scenes, or assets.

## Authoring Rules

- add exactly one row to `skills_projectile.csv`;
- map exact Reference facts to the existing header; do not add columns;
- map an explicit infinite-pierce rule to the existing `pierce_count=999` sentinel;
- resolve a status id only by one exact `status_effect_label` match; zero or multiple matches means stop;
- do not invent asset paths; leave optional asset fields empty only when the schema and targeted validation accept them;
- do not author Skill-owned base graph rows on this route; if the Base behavior needs them, stop because the current Projectile base contract does not provide that path.

## Verification

Confirm the derived identity, runtime kind, every authored numeric value, CSV column count, unique skill id, conditional trigger ownership, UTF-8 encoding, and targeted CSV validation. The diff must contain only the new Base row and an explicitly required trigger bundle.
