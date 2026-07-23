# AUTOMATION_GUIDE

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Archived History

Older OPS automation and role-policy task blocks were archived to `boards/ARCHIVE/OPS_AUTOMATION_ARCHIVE_2026-05-19.md`.

- Non-July task blocks from `boards\OPS\AUTOMATION_GUIDE.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Task: 2026-07-23 Naive Code Filter Role

### Task title

Add an evidence-based inspection role for removing or consolidating naive code.

### Goals

- Audit every declaration in one exact user-provided C# script or folder.
- Detect unnecessary indirection, multiple authorities, repeated internal fallback, dead code, and garbage variables.
- Expand into reference sites only when required to settle a finding.
- Separate inspection findings from Code Builder implementation.

### Constraints

- Role Owner is Code Builder for documentation implementation.
- Naive Code Filter does not edit source code, scenes, prefabs, assets, or data.
- The role must not create new features, state owners, fields, fallbacks, schemas, or assets.
- Unity dynamic references must be checked before dead-code classification.

### Role Owner

Code Builder

### Status

Implemented and documentation-validated.

### Next Actions

- Invoke the role by explicitly naming `Naive Code Filter` and providing one exact script or folder path.
- After reviewing its evidence-backed handoff, explicitly invoke Code Builder for approved deletions or consolidation.

### Evidence

- Added `AGENTS_ROLE/NAIVE_CODE_FILTER.md` with exact-path input, complete declaration coverage, bounded reference expansion, six finding criteria, four decisions, and a fixed output contract.
- `AGENTS.md` now exposes the role entry point and states its inspection-only boundary.
- `MDTREE.md` routes the role to the exact target and limits cross-file expansion to evidence needed for original-target findings.
- `AGENTS_ROLE/COMMON.md` now includes Naive Code Filter under shared evidence, Unity, Git, and board rules.
- UTF-8 reads succeeded for all five policy files; the new role contains 6 criteria, 4 decisions, and 9 required output items.
- Targeted trailing-whitespace search returned 0 matches, and `git diff --check` passed for the tracked policy files.
- No gameplay code, CSV, scene, prefab, or asset file was changed by this role-policy task.

### History

- 2026-07-23: User defined the Naive criteria and approved Code Builder creation of the inspection role.

## Task: 2026-07-18 July-Only Active Board Compaction

### Task title

Move every non-July task record from active COMBAT, DATA, MON, OPS, RUN, and UI boards into one source-grouped archive.

### Goals

- Keep active domain boards limited to task blocks explicitly dated in July 2026.
- Preserve all earlier and undated task history under `boards/ARCHIVE/`.
- Keep board routing shells and archive links in active files.
- Make the migration reproducible and verifiable.

### Constraints

- Role Owner is Code Builder.
- Gameplay code, CSV, prefabs, scenes, and unrelated worktree changes remain untouched.
- Existing task content must stay grouped by its original source board.
- Undated task blocks must be archived rather than assigned an invented date.

### Role Owner

Code Builder

### Status

Implemented and structurally validated.

### Next Actions

- Future board cleanup can run `tools/archive_non_july_board_tasks.ps1` with an updated keep month and archive date after inspecting the new cutoff.
- Read `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` only when pre-July or undated history is required.

### Evidence

- The pre-migration audit found 116 July-dated task blocks, 172 pre-July task blocks, 7 undated task blocks, and one standalone undated damage-meter handoff.
- `tools/archive_non_july_board_tasks.ps1` dry-run and apply runs both reported 116 retained July tasks and 180 archived tasks across 18 source files.
- Seventeen surviving active board files received a direct archive-history link.
- `boards/UI/DAMAGE_METER_UI_HANDOFF.md` was normalized into a required-field task block inside the archive and removed from the active UI folder.
- The first post-migration audit found 116 original July task blocks, 0 non-July active task blocks, 180 archive task blocks, 18 archive source sections, and 0 missing required archive sections.

### History

- 2026-07-18: User explicitly requested that active domain boards retain only July work and that every other record move under `boards/ARCHIVE/`.

## Task: 2026-07-17 Reference-Only Family Base Blueprint Routing

### Task title

Add six family-specific Base blueprints and route Skill Builder from one exact Reference MD.

### Goals

- Accept the exact user-provided skill Reference MD as the only semantic input.
- Route new Base authoring to one of Projectile, Buff, SingleAttack, LineAttack, AreaAttack, or Passive blueprints.
- Keep Enhancement/Master authoring on the shared node blueprint while deriving skill and Choice ids from the same Reference format.
- Read only the selected blueprint and its explicitly routed CSV rows.

### Constraints

- Role Owner is Designer for structure and Code Builder for documentation implementation.
- Base authoring is limited to existing runtime kinds, columns, registered nodes, and trigger files.
- New runtime behavior, schema, node types, code, prefabs, scenes, and asset creation remain outside Skill Builder.
- The provided Reference examples do not contain runtime visual asset paths, so Builder must not invent them.
- No Passive Reference example was provided; Passive semantic-input simulation remains pending a future exact user path.

### Role Owner

Designer / Code Builder

### Status

Implemented and locally verified.

### Next Actions

- Future Base requests provide one exact Reference MD path; Builder reads it first and selects exactly one Base blueprint.
- Combined Base plus Enhancement/Master work may read that Base blueprint and `enhancement-master-node-blueprint.md`, but no other family blueprint.
- Stop before editing when Reference values are qualitative, identity/family is ambiguous, or existing CSV/node contracts cannot express the behavior.
- A future Passive request must supply its exact Reference MD before Passive values can be simulated or authored.

### Evidence

- Added `projectile-base-blueprint.md`, `buff-base-blueprint.md`, `single-attack-base-blueprint.md`, `line-attack-base-blueprint.md`, `area-attack-base-blueprint.md`, and `passive-base-node-blueprint.md` under `boards/SkillBluePrint/`.
- The inspected Base CSV schemas differ by family. Existing runtime kinds are Projectile=`CooldownProjectile,MagazineProjectile`, Buff=`Buff,Shield`, SingleAttack=`SingleAttack`, LineAttack=`LineAttack`, and AreaAttack=`AreaAttack,Field`; Passive Base CSV contains only identity/description fields.
- Graph inspection found Skill-owned Base nodes in SingleAttack, AreaAttack, and Passive; Passive contains 163 Skill-owned graph rows. Projectile, Buff, and LineAttack have no current Skill-owned Base graph rows.
- `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv` uniquely maps inspected Reference labels including `추위`, `둔화`, `방어막`, `신성 노출`, `이름표식`, and `행동속도 증가` to current ids.
- `Test-Path` confirmed every routed Base/graph/trigger/node-definition/status path except an AreaAttack trigger CSV. The nonexistent AreaAttack trigger route was removed, and its blueprint now stops when an event trigger is required.
- A read-only simulation used the seven exact user-provided Reference paths. It derived `ariel-a`, `vega-a`, `ariel-e`, `ariel-b`, `rin-b`, `eve-b`, and `eve-c`, selected the expected family blueprint, resolved exactly one Base row each, and matched five Trait plus two Master Choice ids each with `simulationErrors=0`.
- `enhancement-master-node-blueprint.md` now derives skill and Choice ids from the Reference path/table positions, excludes Awakening rows, resolves status ids only by one exact label match, and stops on missing numeric payload or timing.
- `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/GAMEBULIDER.md`, `AGENTS_ROLE/GAMEBULIDER_SKILL.md`, and `BLACKBOARD.md` now expose the family Base route and retain the new-runtime/code/asset stop boundary.
- `area-attack-base-blueprint.md` no longer asks for magazine/reload values because the selected AreaAttack authoring contract does not use them.
- All six Base blueprints now spell out the Reference-derived identity as `monster_id`, slot, and `skill_id` instead of the opaque phrase "skill identity defined by GAMEBULIDER_SKILL.md".
- `GAMEBULIDER_SKILL.md` now centralizes the lifecycle boundary: runtime code owns state transitions and cleanup, while blueprints still require CSV-backed tuning values such as cooldown, duration, interval, reload, delay, and trigger internal cooldown.
- `GAMEBULIDER_SKILL.md` is now the sole final-output contract for Skill Builder work. Blueprints retain implementation and family-specific verification rules only; the remaining output instruction was removed from `enhancement-master-node-blueprint.md`.
- Inspected code evidence: `InGameSkillDefinitionMapper.cs` maps CSV cooldown/active duration/tick interval/magazine/reload into `SkillData`; `SkillRuntimeInstance.cs` owns countdown and recovery transitions; Beam/Zone/Support executors consume mapped duration/interval/status duration; projectile and SingleAttack visual lifetimes use runtime resolvers; Passive effects refresh through `InGameCombatManager` and `InGamePassiveEffectRuntime`.
- Final checks reported seven active blueprints, `utf8Errors=0`, `trailingWhitespace=0`, 55 concrete documented paths with `missing=0`, no removed oversized blueprint sections, and no stale single-blueprint routing phrase.
- `git diff --check` passed for the routed policy/blueprint files; Git emitted only LF-to-CRLF working-copy warnings.
- No gameplay CSV, runtime code, prefab, scene, or asset was edited.

### History

- 2026-07-17: User chose separate Base blueprints because family CSV columns differ and Passive uses Skill-owned nodes.
- 2026-07-17: User declared the exact skill Reference MD to be the entire user input and supplied seven active-skill examples for routing and identity simulation.
- 2026-07-17: User removed the irrelevant AreaAttack magazine/reload instruction and requested clarification of identity and lifecycle ownership; the documentation was made explicit and lifecycle rules were centralized instead of duplicated per blueprint.
- 2026-07-17: User requested one owner for Skill Builder final-output requirements; output ownership was centralized in `GAMEBULIDER_SKILL.md` and removed from the selected blueprints.

## Task: 2026-07-17 Single Enhancement/Master Node Blueprint Routing

### Task title

Replace runtime-kind skill blueprints with one Enhancement/Master node-authoring contract.

### Goals

- Route all Enhancement and Master Skill Builder work through one node blueprint.
- Limit default edits to one requested Choice and its graph rows on an existing base skill.
- Prevent default reads of unrelated markdown, CSV, runtime code, prefabs, scenes, history, and old implementations.
- Remove the obsolete runtime-kind and one-off skill blueprint files.
- Remove superseded Trait/Master plans and auxiliary Skill Builder workflow documents from active routing.
- Keep the single blueprint short and drive implementation from one exact user-provided skill Reference MD.

### Constraints

- Role Owner is Designer for the structure handoff and Code Builder for the documentation refactor.
- Base skill creation, runtime wiring, prefab/scene changes, new node types/handlers/parameters, and shared runtime extensions remain outside Skill Builder authority.
- Existing unrelated user worktree changes must remain untouched.
- This task changes workflow documentation only; it does not change gameplay code or CSV content.

### Role Owner

Designer / Code Builder

### Status

Implemented and locally verified. Its Base-skill exclusion boundary was superseded later on 2026-07-17 by the family Base blueprint task above; the shared Enhancement/Master route remains active.

### Next Actions

- Future Enhancement and Master Skill Builder tasks read `boards/SkillBluePrint/enhancement-master-node-blueprint.md` as the only default blueprint.
- The user provides one exact skill Reference MD; Builder stops with a missing-value list instead of searching other references or history.
- Route exactly one matching Choice CSV and graph CSV under `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/<family>/`.
- Read node-definition CSV files only when parsed node metadata is missing.
- Route one matching trigger CSV only when the Reference explicitly defines event-triggered behavior.
- Stop and return base-skill or runtime-extension work to Designer or the normal Code Builder implementation track.

### Evidence

- `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/GAMEBULIDER.md`, and `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now route Enhancement/Master node authoring to the single blueprint and exclude unrelated axes by default.
- `boards/SkillBluePrint/enhancement-master-node-blueprint.md` records only the Reference input/stop contract, minimal read/edit scope, Plan/Effect/Trigger composition rules, implementation steps, and verification.
- The blueprint was then reduced to 80 lines and six operational sections; `Purpose`, runtime evidence, full node-kind/handler catalogs, and duplicate final-output sections were removed.
- `AGENTS.md` and `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now allow only the exact user-provided Reference MD and stop when implementation-critical values are missing or ambiguous.
- The initial full handler catalog matched all 96 registered handlers, then was intentionally removed from the final blueprint so node metadata stays CSV-owned and the blueprint stays short.
- `Get-ChildItem` confirmed the current authoring layout contains matching Choice/graph files under `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/` and node metadata under `monster/skills/nodes/definitions/`.
- `rg --files boards/SkillBluePrint` reported exactly one active file: `enhancement-master-node-blueprint.md`.
- `Test-Path` returned `False` for all seven removed blueprints: AreaAttack, BeamSkill, multi-effect, passive-stat, projectile, shield/buff unification, and SingleAttack.
- Removed the completed `boards/TRAIT_MASTER` plan/guide and both superseded Skill Builder auxiliary documents; active Skill Builder routing no longer references them.
- Targeted active-routing search reported `0 active references` to the seven removed blueprint names.
- `git diff --check -- AGENTS.md MDTREE.md AGENTS_ROLE/GAMEBULIDER.md AGENTS_ROLE/GAMEBULIDER_SKILL.md BLACKBOARD.md boards/SkillBluePrint` passed; Git emitted only existing LF-to-CRLF conversion warnings.
- No runtime code, CSV content, prefab, scene, MON/DATA/RUN/UI board, or archive file was edited by this task.
- Vega A dry-run used only `a-three-sword-flurry.md`, the projectile Choice/graph CSVs, the projectile trigger CSV, and node-definition CSVs. It resolved seven Choices, one trigger, and ten graph nodes with `schemaErrors=0`.
- The dry-run returned `PROCEED` for the five Enhancement rows and correctly stopped Master 1 for missing `delay_seconds` and Master 2 for missing slash damage payload. No CSV row was edited by the simulation.

### History

- 2026-07-17: User approved replacing per-skill blueprints with one Enhancement/Master node blueprint, narrowing Code Builder routing, and deleting the other skill blueprints.
- 2026-07-17: User confirmed the completed Trait/Master plans and old handoff/exception guides were unnecessary and requested their deletion so Skill Builder references only the single node blueprint.
- 2026-07-17: User requested a shorter reference-driven blueprint and a real dry-run proving whether Enhancement/Master effects can be authored or safely stopped on missing source values.
