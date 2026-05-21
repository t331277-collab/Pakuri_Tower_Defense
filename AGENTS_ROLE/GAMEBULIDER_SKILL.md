# GAMEBULIDER_SKILL.md

## Role

Skill Builder is a Code Builder track for implementing monster or player skills.

Use this file when the user explicitly invokes "Skill Builder" or asks Code Builder to implement, wire, or connect a skill, skill runtime path, skill prefab, or skill effect.

## Mandatory Markdown Read Set

For Skill Builder work, read only:

- `AGENTS_ROLE/COMMON.md`
- `AGENTS_ROLE/GAMEBULIDER.md`
- this file
- exactly one matching `boards/SkillBluePrint/*-blueprint.md`

Do not read `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md` by default for Skill Builder work. The selected blueprint owns the implementation checklist, allowed runtime-code inspection scope, and verification expectations.

## Blueprint Selection Rule

Select the blueprint named by the user or clearly matched from the requested skill type.

Known mappings:

- projectile, projectile skill, bullet, missile: `boards/SkillBluePrint/projectile-blueprint.md`
- BeamSkill, beam, laser, ray, slash-line, `LineAttack`: `boards/SkillBluePrint/BeamSkill-blueprint.md`
- single attack, one-shot area, instant area, `SingleAttack`: `boards/SkillBluePrint/single-attack-blueprint.md`
- multi-effect skill, bundled ally effect, choice-gated secondary effect, `monster_skill_effects.csv`: `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md`
- area attack, sustained area, ticking area, `AreaAttack`: `boards/SkillBluePrint/area-attack-blueprint.md`
- zone, area, field, aura, ground effect: `boards/SkillBluePrint/zone-blueprint.md` when that file exists

If no matching blueprint exists, stop and say the blueprint file does not exist.

If multiple blueprints could match, stop and ask the user which blueprint owns the skill before reading additional markdown.

## Blueprint Authority

The selected blueprint owns:

- required parsed input;
- allowed extra markdown reads;
- allowed runtime code inspection scope;
- common, partial, and unsupported behavior classification;
- stop-and-ask rules;
- verification expectations.

Do not read another skill blueprint unless the selected blueprint explicitly names it.

Do not read MON, DATA, RUN, UI, OPS, archive, or other domain markdown unless the selected blueprint or an inspected failure path explicitly justifies that read.

## Parsed Input Rule

The user or task context must provide the parsed skill data required by the selected blueprint.

If required parsed fields are missing, stop and report the missing fields instead of searching broadly through CSV, reference, archive, or old implementation files.

## Unsupported Behavior Rule

If the requested behavior is outside the selected blueprint's common contract, stop and ask whether to implement a one-off exception or design a reusable shared extension.

Do not infer unsupported behavior from old monster-specific code, CSV text, or reference documents unless the user explicitly asks for that discovery work.

## Routing Decision Log

Before reading the selected blueprint, state:

- request class: Skill Builder
- selected blueprint path
- markdown files intentionally excluded

The exclusion list should name the skipped axes, such as MON, DATA, RUN, UI, OPS, archive, and other skill blueprints, when those axes were not requested and are not named by an inspected failure path.

## Output Requirements

Skill Builder final output must include:

- selected blueprint;
- whether the request fit the blueprint's common path;
- consumed parsed fields, or missing parsed fields if work stopped;
- changed runtime, prefab, scene, or data files when implementation occurs;
- verification results required by the selected blueprint;
- remaining user-owned Play Mode verification when applicable.
