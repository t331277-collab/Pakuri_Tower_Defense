# AUTOMATION_GUIDE

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Archived History

Older OPS automation and role-policy task blocks were archived to `boards/ARCHIVE/OPS_AUTOMATION_ARCHIVE_2026-05-19.md`.

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

## Task: 2026-05-30 UTF-8 Documentation Read Default

### Task title

Make UTF-8 raw text reads the default documentation inspection path for repository policy work.

### Goals

- Keep markdown and other text-document evidence readable without mojibake during Codex inspection.
- Record the repository-level expectation in `AGENTS.md`.
- Align future document reads on `Get-Content -Raw -Encoding UTF8` where PowerShell is used.

### Constraints

- Role Owner is Designer.
- This task changes workflow policy, not runtime gameplay code.
- Command approval behavior is controlled by the CLI/runtime, not by markdown policy alone.

### Role Owner

Designer

### Status

Implemented.

### Next Actions

- Future markdown and text-document inspection should prefer `Get-Content -Raw -Encoding UTF8`.
- If command approval prompts still appear, reuse the saved approved prefix instead of widening to unrelated command families.

### Evidence

- `AGENTS.md` startup rules now state that markdown and other text documentation files should be read with `Get-Content -Raw -Encoding UTF8` by default.
- A broad approved command prefix for `Get-Content -Raw -Encoding UTF8` was saved in the current CLI session.

### History

- 2026-05-30: User asked to default future document reads to UTF-8 and to record that policy in `AGENTS.md`.

## Task: 2026-05-30 File Read/Write Command Allowance For This Project

### Task title

Record that file read/write shell commands are expected workflow commands in this project.

### Goals

- Make file read/write inspection commands explicit as normal repository workflow.
- Avoid repeated policy churn around enumerating individual command examples.
- Keep UTF-8-safe text reads as the preferred documentation-read pattern.

### Constraints

- Role Owner is Designer.
- This task records project-local workflow policy only.
- CLI approval storage is still controlled by the runtime; markdown policy does not override runtime security.

### Role Owner

Designer

### Status

Implemented.

### Next Actions

- Prefer UTF-8-safe text reads for documentation where practical.
- Reuse saved CLI approvals where possible, while treating file read/write commands as the normal intended workflow in project policy.

### Evidence

- `AGENTS.md` now states that file read/write shell commands inside the intended workspace are normal and expected workflow commands for this project.
- `AGENTS.md` no longer enumerates a short fixed list of command examples and instead records the broader project-level allowance.

### History

- 2026-05-30: User asked to replace the explicit command list with a broader statement that file read/write commands are allowed for this project before continuing implementation work.

## Task: 2026-05-28 Skill Builder Trigger Payload Documentation Guard

### Task title

Document the trigger-row payload guard so Skill Builder handoffs do not produce source-validity errors for trigger-routed `SingleAttack` follow-ups.

### Goals

- Make the Skill Builder documentation explicitly state that trigger `SingleAttack` rows own their own concrete damage payload.
- Prevent future handoffs from treating `damage_multiplier` as an implicit source-skill payload reuse mechanism.
- Keep the guard close to the active Skill Builder blueprint and exception docs.

### Constraints

- Role Owner is Code Builder / Designer.
- This task changes workflow documentation, not runtime gameplay code by itself.
- Claims must stay grounded in the inspected validator, trigger CSV row contract, and edited markdown files.

### Role Owner

Code Builder / Designer

### Status

Implemented and locally verified by targeted markdown inspection plus Unity CSV validation.

### Next Actions

- Future Skill Builder row-bundle handoffs should always restate trigger damage payload fields when `monster_skill_triger.csv` participates in the implementation.

### Evidence

- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now tells Builder that routed `monster_skill_triger.csv` `SingleAttack` follow-up rows must carry positive fixed payload through `base_damage` or `attack/spell` coefficient rather than `damage_multiplier` alone.
- The then-active SingleAttack blueprint included `TriggerSingleAttackRows` in optional parsed input and required a `Fixed` trigger row to pass source CSV validation with its own positive damage payload; that runtime-kind blueprint was later removed.
- The then-active exception guidance recorded that trigger rows owned their own damage payload and linked effect rows did not satisfy trigger-row damage validation; this guidance was later superseded by the single node blueprint.
- The then-active handoff format required concrete trigger damage payload fields; that auxiliary format was later removed from active routing.

### History

- 2026-05-28: Vega-B master-1 follow-up validation failure showed that the existing Skill Builder docs did not explicitly guard against zero-payload trigger rows that only carried a multiplier.

## Task: 2026-05-24 Projectile Blueprint Nth Launch Branch Extension

### Task title

Promote nth-launch branch chance override into the projectile blueprint common path.

### Goals

- Keep Skill Builder from stopping on approved reusable projectile branch behavior that triggers every nth base projectile launch.
- Document `BranchLaunchPeriod` and `BranchLaunchChanceSet` as optional parsed projectile fields.
- Preserve the stop-and-ask rule for sequence-state behavior not covered by the shared nth-launch branch override.

### Constraints

- Role Owner is Code Builder / Skill Builder because this policy update follows an implemented runtime extension.
- No skill CSV row values, prefab assets, or scene objects were changed by this task.
- Skill Builder still requires parsed input or explicit current CSV/code discovery authorization before editing specific skill rows.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and locally verified by build, Unity refresh, and targeted source inspection.

### Next Actions

- Future projectile Skill Builder requests may use `BranchLaunchPeriod` plus `BranchLaunchChanceSet` for reusable nth-launch branch behavior.
- If a future request needs another nth-launch effect family, add a deliberate shared extension instead of hardcoding a monster-specific branch.

### Evidence

- The then-active projectile blueprint listed `BranchLaunchPeriod` and `BranchLaunchChanceSet` as optional common parsed fields.
- That blueprint included nth-launch branch chance override in its common projectile contract and kept other sequence-state effects outside the shared pattern.
- The runtime-kind blueprint was later removed when Skill Builder moved to the single Enhancement/Master node blueprint.
- Runtime support was added through `SkillRuntimeInstance.ProjectileLaunchCount`, `ProjectileSkillExecutor` per-launch branch resolution, and snapshot/choice fields for branch launch period/chance.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remained.

### History

- 2026-05-24: User approved using a reusable launch-count function so `Rin-a` master-2 can make every 3rd launched projectile branch at 100%, and future skills can reuse nth-launch projectile behavior.

## Task: 2026-05-24 Skill Blueprint Execution Path Refresh

### Task title

Refresh Skill Builder blueprint references after the skill execution folder refactor.

### Goals

- Remove stale `SkillExecutors.cs` references from the multi-effect and shield/buff unification blueprints.
- Point future Skill Builder work at the current `Execution/Executors`, `Execution/Runtime`, and `Execution/Actors` files.
- Keep blueprint claims grounded in the currently inspected refactored code structure.

### Constraints

- Role Owner is Designer because this changes workflow/blueprint documentation, not runtime gameplay code.
- No C# script, scene, prefab, or gameplay CSV values were changed.
- Claims must stay grounded in inspected blueprint text and current runtime source paths.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown/code-path inspection.

### Next Actions

- Future Skill Builder work that uses these blueprints should follow the current role-folder paths instead of looking for `SkillExecutors.cs`.
- If more blueprints still mention deleted execution files, update them only after inspecting the current matching runtime type path.

### Evidence

- The then-active multi-effect and shield/buff blueprint documents were refreshed to the refactored executor paths; both one-off documents were later removed from active Skill Builder routing.
- Targeted search in the two edited blueprint files found no remaining deleted `Execution/SkillExecutors.cs` path references; current `SupportSkillExecutors.cs` references are intentional.
- `Test-Path Pakuri\Assets\Scripts2\InGame\Skills\Execution\SkillExecutors.cs` returned `False`.

### History

- 2026-05-24: User asked to update the stale `SkillExecutors.cs` references in the multi-effect and shield/buff status unification blueprints after the execution folder role split.

## Task: 2026-05-24 Skill Builder Blueprint And Explicit-Path Only Default

### Task title

Make Skill Builder default to blueprint plus explicit parsed input/path authority, with CSV/code discovery allowed only by explicit user instruction.

### Goals

- Raise the default Skill Builder boundary to a top-level repository rule.
- Make `Skill Builder` stop instead of inspecting unrelated CSV/code when parsed fields or work paths are missing.
- Reserve current CSV/code inspection as an explicit exception path that the user must request.

### Constraints

- Role Owner is Designer because this task changes workflow policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims must stay grounded in the inspected text of `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/COMMON.md`, `AGENTS_ROLE/GAMEDESIGNER.md`, `AGENTS_ROLE/GAMEBULIDER_SKILL.md`, and this board.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Future `Skill Builder` requests should provide either parsed fields or explicit work paths if code inspection inside a scoped area is expected.
- If the user wants Builder to derive missing values from current CSV/code, the instruction should say so explicitly instead of leaving that as the default.
- Future blueprint updates should preserve this default boundary unless the user explicitly requests a policy change.

### Evidence

- `AGENTS.md` now has `Skill Builder Absolute Boundary`, which limits default Skill Builder authority to the selected blueprint, explicit parsed input, and explicit code/prefab/scene/asset paths, and says CSV/code discovery requires explicit user instruction.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now says default Skill Builder authority is limited to the selected blueprint, explicit parsed skill data, explicit work paths, and files inside those paths only when blueprint-required inspection is needed.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now says reading current CSV/code as a parsed-source discovery step is forbidden by default and allowed only when the user explicitly instructs Builder to do so.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now requires Builder to stop and report missing parsed fields, scoped row bundles, or explicit work paths instead of broad repository discovery.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` final output requirements now include whether explicit CSV/code discovery was used.

### History

- 2026-05-24: User asked whether Skill Builder could be made to implement from blueprint and user-given paths only, with related CSV/code inspection treated as an exception path that runs only on explicit user command.

## Task: 2026-05-24 Passive Stat Blueprint And Skill Builder Route

### Task title

Add a blueprint-only passive-stat contract and route Skill Builder passive requests to it.

### Goals

- Create a dedicated Skill Builder blueprint for always-on passive number-adjustment skills.
- Keep ordinary passive implementation on blueprint plus parsed input only, without CSV/code rediscovery.
- Force triggered, damage-dealing, target-search, or proc-based passive behavior into stop-and-ask instead of implicit implementation.
- Add a clear routing mapping so passive requests no longer fail on missing blueprint selection.

### Constraints

- Role Owner is Designer because this task changes workflow/design policy, not runtime gameplay code.
- No C# script, scene, prefab, or gameplay CSV values were changed by this task.
- Claims must stay grounded in inspected Skill Builder routing markdown and existing blueprint patterns.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown checks.

### Next Actions

- The passive-stat blueprint route recorded by this historical task was later removed; current Enhancement/Master work uses the single node blueprint.
- If a passive creates gameplay actions instead of always-on numeric modifications, Builder should stop and ask instead of weakening the passive-stat contract.
- If a reusable event-driven passive contract is needed later, create a separate passive-trigger blueprint rather than broadening the passive-stat blueprint.

### Evidence

- Added the then-active passive-stat blueprint, which was later removed.
- `passive-stat-blueprint.md` now defines a blueprint-only contract for `RuntimeKind == Passive` work, including required parsed input, allowed modifier families, common passive-stat contract, and stop-and-ask rules.
- `passive-stat-blueprint.md` explicitly forbids CSV/reference rediscovery by default and blocks event-driven, damage-dealing, target-search, proc-based, and spawn-creating passives behind stop-and-ask.
- The then-active Skill Builder mapping routed passive-stat requests to that blueprint; the mapping was later replaced by the single Enhancement/Master node route.

### History

- 2026-05-24: User asked to create a passive blueprint that lets Skill Builder implement ordinary passives from the blueprint alone and to wire the routing path for future passive requests.
