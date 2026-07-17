# GAMEBULIDER_SKILL.md

## Role

Skill Builder is the Code Builder track for:

- creating a new Base skill with an existing Projectile, Buff, SingleAttack, LineAttack, AreaAttack, or Passive runtime/schema;
- creating or changing Enhancement and Master Choices for an existing or newly authored Base skill.

New runtime behavior, schema columns, node types, shared code, prefab/scene changes, and asset creation remain outside this track.

## Mandatory Markdown Read Set

After `AGENTS.md` and `MDTREE.md`, read only:

- `AGENTS_ROLE/COMMON.md`;
- `AGENTS_ROLE/GAMEBULIDER.md`;
- this file;
- the exact skill Reference MD path provided by the user;
- exactly one routed Base blueprint for Base work;
- `boards/SkillBluePrint/enhancement-master-node-blueprint.md` for Enhancement/Master work.

A combined Base plus Enhancement/Master request may read the selected Base blueprint and the Enhancement/Master blueprint. Do not read the other Base blueprints.

## Reference-Only Input Rule

The exact user-provided skill Reference MD is the only semantic input. Do not require separate ids, parsed values, node bundles, or asset paths.

Derive identity only when all checks agree:

- `monster_id`: directory immediately before `skill` in the Reference path;
- slot: leading filename token before the first hyphen, normalized to the CSV slot token;
- `skill_id`: `<monster_id>-<lowercase slot>`;
- display name and behavior: Reference title, summary, Basic Information, Base Values, and calculation sections.

Trait rows, Master rows, and Awakening rows are not Base input. Enhancement/Master authoring uses only the Trait and Master Skill tables; Awakening is excluded.

If the path, title, classification, slot, formulas, or behavior conflict, stop. Do not open a linked Obsidian document or another Reference to repair the input.

## Runtime Lifecycle Boundary

Do not restate or author runtime lifecycle algorithms in a blueprint. Existing runtime code owns cast eligibility, cooldown/reload countdown, active/tick progression, projectile and visual cleanup, status expiry, and passive refresh/event dispatch.

Blueprints still require the exact data values consumed by that lifecycle when the selected CSV exposes them: cooldown, reload, firing/tick interval, active or status duration, delayed effect timing, and trigger internal cooldown. A code-owned transition is not permission to invent or omit its Reference-owned tuning value.

## Blueprint Routing

Read the Reference first, then select exactly one Base blueprint:

- projectile launch behavior: `boards/SkillBluePrint/projectile-base-blueprint.md`;
- shield or non-damaging status/stat application: `boards/SkillBluePrint/buff-base-blueprint.md`;
- one immediate damage execution, including a documented secondary effect: `boards/SkillBluePrint/single-attack-base-blueprint.md`;
- sustained line/beam damage with duration and tick interval: `boards/SkillBluePrint/line-attack-base-blueprint.md`;
- sustained area/field damage with duration and tick interval: `boards/SkillBluePrint/area-attack-base-blueprint.md`;
- passive behavior expressed by Skill-owned nodes: `boards/SkillBluePrint/passive-base-node-blueprint.md`.

All Enhancement and Master requests route to:

- `boards/SkillBluePrint/enhancement-master-node-blueprint.md`.

If more than one Base family matches or the Reference lacks the facts needed to distinguish them, stop and report the ambiguity.

## Minimal Authority

The selected blueprint owns the exact CSV read/edit set. Before reading a CSV, name its path and why it is required.

Status ids may be resolved only from the uniquely matching `status_effect_label` row in:

- `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv`.

Do not invent an id or asset path. Do not inspect unrelated rows, another family, another Reference, linked documentation, boards, archives, old implementations, broad runtime code, prefabs, scenes, or asset folders.

If the Reference lacks an exact required value, or the selected existing contract cannot express the behavior, stop before editing. Scope expansion requires explicit user authority.

## Routing Decision Log

Before CSV access, state:

- request class: Base, Enhancement/Master, or combined;
- exact Reference MD path;
- derived monster, slot, skill id, and runtime family;
- selected blueprint or two blueprints for a combined request;
- exact CSV files and row ownership to read/edit;
- excluded markdown, family, code, prefab, scene, asset, and unrelated CSV axes.

## Output Requirements

Report:

- consumed Reference and selected blueprint;
- derived identity and runtime family;
- routed and changed rows/files;
- node/trigger ownership when used;
- targeted CSV validation results;
- confirmation that no unrelated family, runtime code, prefab, scene, or asset was changed;
- exact missing value or boundary when stopped.
