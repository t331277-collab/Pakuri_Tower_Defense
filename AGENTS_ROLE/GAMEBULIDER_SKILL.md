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

Default path:

- blueprint only

Exception-only companion docs are allowed only when the selected blueprint cannot proceed on its own because the task is driven by a scoped row bundle or because row-combination interpretation is itself the blocking issue:

- `boards/SkillBluePrint/skill-csv-exception-guide.md`
- `boards/SkillBluePrint/skill-builder-handoff-format.md`

Do not read `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md` by default for Skill Builder work. The selected blueprint owns the implementation checklist, allowed runtime-code inspection scope, and verification expectations.

## Blueprint Selection Rule

Select the blueprint named by the user or clearly matched from the requested skill type.

Known mappings:

- passive, passive skill, always-on passive, stat passive: `boards/SkillBluePrint/passive-stat-blueprint.md`
- projectile, projectile skill, bullet, missile: `boards/SkillBluePrint/projectile-blueprint.md`
- BeamSkill, beam, laser, ray, slash-line, `LineAttack`: `boards/SkillBluePrint/BeamSkill-blueprint.md`
- single attack, one-shot area, instant area, `SingleAttack`: `boards/SkillBluePrint/single-attack-blueprint.md`
- multi-effect skill, bundled ally effect, choice-gated secondary effect, `monster_skill_effects.csv`: `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md`
- area attack, sustained area, ticking area, `AreaAttack`: `boards/SkillBluePrint/area-attack-blueprint.md`
- zone, area, field, aura, ground effect: `boards/SkillBluePrint/zone-blueprint.md` when that file exists

Exception-doc usage does not replace blueprint selection.
Choose the primary blueprint from the base runtime behavior first, then use the exception docs only when the blueprint alone cannot safely continue.

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

## Shared Skill Runtime Policy

For skill blueprints that are fundamentally contact-based attack skills:

- projectile
- `SingleAttack`
- `AreaAttack`

Builder should prefer prefab-authored hitbox behavior when the runtime path and prefab actually provide collider-based contact.

This means:

- projectile contact should stay on the shared projectile path
- `SingleAttack` contact should stay on the shared prefab-hitbox / shared area-hit path
- `AreaAttack` / zone contact should stay on the shared prefab-hitbox zone path when the zone prefab provides colliders

Do not redesign those skills back into monster-only fixed fake radii when the shared prefab hitbox path already exists.

Do not force collider-contact structure onto skills that are not fundamentally contact attacks.
Keep the existing non-contact structure for cases such as:

- explicit target-designated debuffs or marks
- battlefield-wide or global aura effects
- other skills whose common path is status/selection/radius logic instead of prefab contact

If a request mixes contact-hitbox behavior and explicit-target or global-effect behavior in one skill, stop and ask whether the user wants:

- one shared reusable extension, or
- a one-off exception

## Parsed Input Rule

The user or task context may provide the parsed skill data required by the selected blueprint directly.

When parsed data is not provided directly, Skill Builder may derive it only from the minimum active skill-authoring CSV set under `Pakuri/Assets/CSVdata/source/` that is required by the selected blueprint and the requested skill.

Default Skill Builder authority is limited to:

- the selected blueprint
- parsed skill data explicitly provided by the user or task context when it exists
- self-routed active skill-authoring CSV files under `Pakuri/Assets/CSVdata/source/` that directly participate in the selected skill's base/choice/effect/trigger contract
- explicit work paths named by the user
- files inside those explicit work paths only when the blueprint requires inspection to complete the implementation

Before reading any CSV, Builder must name that CSV in the routing decision and keep the CSV read set minimal.

Do not inspect unrelated CSV, code, references, boards, archives, or old monster implementations to infer missing values or behavior intent.

Allowed alternative:

- a normalized scoped row bundle that follows `boards/SkillBluePrint/skill-builder-handoff-format.md`

Only in that row-bundle exception path, Skill Builder may use:

- `boards/SkillBluePrint/skill-csv-exception-guide.md`
- `boards/SkillBluePrint/skill-builder-handoff-format.md`

to interpret the provided row bundle.

Reading broad current CSV/code outside the self-routed active skill-authoring CSV set is still a separate exception and is forbidden by default.
Use it only when the user explicitly instructs Builder to do so.

If the selected blueprint's common contract is insufficient, or the work requires a new CSV file, a new CSV column, reference-driven value discovery, old monster implementation inspection, or a new shared runtime/common-logic extension, stop and ask the user before widening scope.

If required parsed fields, the required scoped row bundle, or the explicit work path set cannot be completed from the explicit input plus the routed active CSV set, stop and report the missing items instead of searching broadly through CSV, reference, archive, or old implementation files.

## Cooldown Data Policy

When a skill request includes cooldown reduction such as "cooldown -n%" or "cooldown reduction n%", Builder must reuse the existing cooldown CSV authority instead of inventing a new ad hoc field.

Use the already existing cooldown-owned fields that match the requested layer:

- base skill cooldown -> `cooldown_seconds`
- choice or enhancement cooldown scaling -> `cooldown_multiplier`

Do not add a separate new percentage-only cooldown field when the requested behavior is just scaling or editing existing cooldown authority.

## Unsupported Behavior Rule

If the requested behavior is outside the selected blueprint's common contract, stop and ask whether to implement a one-off exception or design a reusable shared extension.

Do not infer unsupported behavior from old monster-specific code, CSV text, or reference documents unless the user explicitly asks for that discovery work.

## Routing Decision Log

Before reading the selected blueprint, state:

- request class: Skill Builder
- selected blueprint path
- exception docs to read next, only when the blueprint alone cannot safely continue
- markdown files intentionally excluded

The exclusion list should name the skipped axes, such as MON, DATA, RUN, UI, OPS, archive, and other skill blueprints, when those axes were not requested and are not named by an inspected failure path.

## Output Requirements

Skill Builder final output must include:

- selected blueprint;
- whether the request fit the blueprint's common path;
- consumed parsed fields, or missing parsed fields if work stopped;
- whether explicit CSV/code discovery was used, or whether the work stayed inside blueprint plus explicit parsed input/path authority;
- changed runtime, prefab, scene, or data files when implementation occurs;
- verification results required by the selected blueprint;
- remaining user-owned Play Mode verification when applicable.
