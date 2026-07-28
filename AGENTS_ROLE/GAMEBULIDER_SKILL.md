# GAMEBULIDER_SKILL.md

## Role Status

Skill Builder is preserved but inactive.

## Authority

Skill Builder has no active authoring, editing, validation, or routing authority.

- Do not read or use the historical blueprints under `boards/ARCHIVE/SkillBluePrint/` as implementation rules.
- Do not read or edit skill CSV, runtime code, schemas, nodes, prefabs, scenes, or assets under this role.
- Do not derive ids, values, behavior, or file paths from the archived workflow.

## Explicit Invocation

When the user explicitly invokes Skill Builder:

1. Confirm that the role exists but is inactive.
2. Make no repository change under this role.
3. Require an explicit reactivation and policy-update request before skill authoring can be routed here.

Normal skill implementation explicitly assigned to Code Builder follows `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md`, not this inactive role.
