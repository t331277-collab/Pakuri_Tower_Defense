# AUTOMATION_GUIDE

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Archived History

Older OPS automation and role-policy task blocks were archived to `boards/ARCHIVE/OPS_AUTOMATION_ARCHIVE_2026-05-19.md`.

## Task: 2026-05-22 Multi-Effect Skill CSV Blueprint

### Task title

Add a reusable multi-effect skill CSV blueprint and Skill Builder route.

### Goals

- Document the reusable CSV-owned route for skills that need secondary damage, ally buffs, delayed waves, or choice-gated effects.
- Keep Ariel-C style behavior out of monster-specific executor hardcoding.
- Add Skill Builder routing for `monster_skill_effects.csv` / multi-effect skill work.
- Extend the blueprint contract to cover separated effect centers and applied-target visual anchors.

### Constraints

- Role Owner is Designer for the blueprint and routing contract.
- Runtime implementation is recorded in DATA/COMBAT/Ariel boards.
- Claims are grounded in inspected `monster_skills.csv`, `SkillDefinition.cs`, `PakuriCsvRuntimeData.*`, and `SkillExecutors.cs`.

### Role Owner

Designer / Skill Builder

### Status

Implemented and locally verified.

### Next Actions

- Future bundled ally-effect or choice-gated secondary-effect skills should start from `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md`.
- If a future effect cannot fit the CSV columns, extend the blueprint/schema before adding executor branches.

### Evidence

- Added `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md`.
- `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md` now documents `center_mode` and `visual_anchor_mode`, including `PrimarySkillCenter` for delayed waves and `AppliedTargets` for unit-attached buff visuals.
- Updated `AGENTS_ROLE/GAMEBULIDER_SKILL.md` so multi-effect skill and `monster_skill_effects.csv` requests route to the new blueprint.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now invokes a shared `SkillMultiEffectExecutor` from the `SingleAttack` path instead of adding an Ariel-C branch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now keeps multi-effect application targets separate from visual centers/anchors through generic `SkillMultiEffectCenterMode` and `SkillMultiEffectVisualAnchorMode` fields.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-22: User approved the Designer blueprint first, then Skill Builder implementation for reusable multi-effect skill CSV support.
- 2026-05-22: Code Builder updated the multi-effect blueprint and implementation so future skills can express applied-target attached visuals and primary-skill-center secondary waves without monster-specific executor branches.

## Task: 2026-05-21 SingleAttack And AreaAttack Blueprint Contracts

### Task title

Add parsed-input Skill Builder blueprints for SingleAttack and AreaAttack.

### Goals

- Add a `SingleAttack` blueprint for one-shot area damage skills.
- Add an `AreaAttack` blueprint for sustained ticking area skills.
- Keep both new blueprints aligned with the existing projectile / BeamSkill parsed-input contract style.
- Update Skill Builder blueprint selection so `SingleAttack` and `AreaAttack` requests route to the new files.

### Constraints

- Role Owner is Designer because this task changes workflow/design policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims must stay grounded in inspected runtime scripts and existing blueprint text.
- `AreaAttack` must not silently absorb `Field`, `Mark`, or `Execute` behavior even though the current runtime maps those kinds to `ZoneSkillData`.

### Role Owner

Designer

### Status

Implemented and locally verified by markdown and targeted code-path inspection.

### Next Actions

- Future `SingleAttack` implementation requests should read `boards/SkillBluePrint/single-attack-blueprint.md` as the first-read contract.
- Future `AreaAttack` implementation requests should read `boards/SkillBluePrint/area-attack-blueprint.md` as the first-read contract.
- If future area-like work is actually `Field`, `Mark`, `Execute`, drone, trap, marked-target fanout, or ally-effect bundled behavior, create or select a more specific blueprint instead of forcing it through these contracts.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` and `boards/SkillBluePrint/BeamSkill-blueprint.md` define the parsed-input contract structure copied for the new blueprint style: `Purpose`, `Core Rule`, `Builder Working Mode`, `What Builder May Read`, `Required Parsed Input`, common contract, stop-and-ask rule, and Builder output.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:69-75` maps `SkillRuntimeKind.SingleAttack` to `SingleAttackData` and `SkillRuntimeKind.AreaAttack` to `ZoneSkillData`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:145-163` maps radius, duration, tick interval, cover-all, damage, and status into `ZoneSkillData` / `SingleAttackData`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:479` shows `ZoneSkillExecutor`; `:611` shows `SingleAttackSkillExecutor`; `:628` routes SingleAttack through `InGameZoneSkillActor.ApplyAreaTick(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs:24-66` initializes and applies an immediate area tick; `:140-160` repeats ticks until duration expires.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs:194-195` registers `SingleAttackSkillExecutor` and `ZoneSkillExecutor` by default.
- Added `boards/SkillBluePrint/single-attack-blueprint.md`.
- Added `boards/SkillBluePrint/area-attack-blueprint.md`.
- Updated `AGENTS_ROLE/GAMEBULIDER_SKILL.md` known mappings for `SingleAttack` and `AreaAttack`.

### History

- 2026-05-21: User asked whether `SingleAttack` and `AreaAttack` blueprints could be created before implementing new skills; Designer inspected existing shared runtime evidence and concluded the contracts can be created first.
- 2026-05-21: User approved creating two blueprint files following the existing blueprint format and similar routing path.

## Task: 2026-05-21 Beam Blueprint Contract Rewrite

### Task title

Rewrite the BeamSkill blueprint to match the parsed-input contract style of the projectile blueprint.

### Goals

- Make `boards/SkillBluePrint/BeamSkill-blueprint.md` a blueprint-first contract instead of a value-rediscovery guide.
- Align BeamSkill routing, read-set boundaries, stop-and-ask behavior, and Builder output expectations with `boards/SkillBluePrint/projectile-blueprint.md`.
- Keep the BeamSkill blueprint grounded in the currently inspected shared `LineAttack` runtime path.

### Constraints

- Role Owner is Designer because this task changes workflow/design policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims must stay grounded in inspected Beam runtime scripts and the current projectile blueprint text.

### Role Owner

Designer

### Status

Implemented and locally verified by markdown and targeted code-path inspection.

### Next Actions

- Future BeamSkill implementation requests should treat this blueprint as the first-read contract and should not reopen CSV/reference files for ordinary numeric rediscovery.
- If future Beam behavior requires charge phases, sweep arcs, stop-first-target, or other unsupported line behavior, extend the shared contract deliberately instead of weakening the stop-and-ask rule.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` is the active parsed-input contract reference that now defines the desired structure: `Purpose`, `Core Rule`, `Builder Working Mode`, `What Builder May Read`, `Required Parsed Input`, `Common ... Contract`, `Stop And Ask User Rule`, and `Required Builder Output`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:67-68` maps `SkillRuntimeKind.LineAttack` to `BeamSkillData`, and `:112-113` plus `:139` map active duration, tick interval, and beam width from runtime data.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:318-388` shows `BeamSkillExecutor` as the shared Beam path; `:428-469` confirms duration, width, and tick interval are resolved through shared helpers and snapshot modifiers.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs:25-56` shows immediate first tick on initialization, and `:119-149` shows repeated ticking until duration ends.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs:70-117` applies shared damage and status through `InGameCombatManager` but does not own special stop-first-target, knockback, or curved/sweeping behavior.
- `boards/SkillBluePrint/BeamSkill-blueprint.md` now follows the projectile-style contract and no longer tells Builder to rediscover numbers from CSV/reference files by default.

### History

- 2026-05-21: User explicitly asked to modify `BeamSkill-blueprint.md` like `projectile-blueprint.md`, verify routing, and perform the work in the Designer role.

## Task: 2026-05-20 Projectile Blueprint Burst Contract Update

### Task title

Promote uniform sequential projectile burst into the common projectile blueprint contract.

### Goals

- Keep `Skill Builder` from stopping on ordinary sequential burst projectile skills after the shared runtime extension.
- Document `BurstProjectileCount` as the common input for sequential projectile volleys.
- Keep special non-uniform delayed projectile behavior in the stop-and-ask path.

### Constraints

- Role Owner is Code Builder / Skill Builder because this policy update follows an implemented runtime extension.
- Do not broaden Skill Builder markdown reads.
- Keep unsupported special sequence behavior explicit.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented.

### Next Actions

- Future projectile blueprints should distinguish `BurstProjectileCount` sequential shots from simultaneous fan-spread projectile count.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` now lists `BurstProjectileCount` in required parsed input.
- `boards/SkillBluePrint/projectile-blueprint.md` now states that `BurstProjectileCount` is for sequential same-direction projectiles fired within one magazine cycle.
- `boards/SkillBluePrint/projectile-blueprint.md` now keeps non-uniform delayed repeated firing in the stop-and-ask examples.

### History

- 2026-05-20: Code Builder added shared burst projectile runtime support for Sein-B and updated the projectile blueprint so future Skill Builder work can use the new common contract.

## Task: 2026-05-19 OPS Board Active Compaction

### Task title

Compact OPS active boards and archive older operational task blocks.

### Goals

- Keep OPS active files focused on current operational state.
- Move older completed automation and reviewer task blocks to `boards/ARCHIVE/`.
- Preserve all moved task history instead of deleting it.
- Verify active and archive task counts after compaction.

### Constraints

- Role Owner is Designer because this task restructures persistent markdown state, not runtime code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Keep `CODEX_CLI_BLACKBOARD.md` and `UNITY_MCP_BLACKBOARD.md` active because each has only one task block.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown checks.

### Next Actions

- Use `boards/ARCHIVE/OPS_AUTOMATION_ARCHIVE_2026-05-19.md` for older automation/policy history.
- Use `boards/ARCHIVE/OPS_REVIEWER_ARCHIVE_2026-05-19.md` for older completed Reviewer history.
- Keep future OPS active files limited to current unresolved or recently relevant task blocks.

### Evidence

- Before compaction, `AUTOMATION_GUIDE.md` had 552 lines and 19 task blocks.
- Before compaction, `REVIEWER_BLACKBOARD.md` had 171 lines and 6 task blocks.
- After compaction, `AUTOMATION_GUIDE.md` had 123 lines and 4 task blocks before this new task block was added.
- After compaction, `REVIEWER_BLACKBOARD.md` had 57 lines and 2 task blocks.
- Added `boards/ARCHIVE/OPS_AUTOMATION_ARCHIVE_2026-05-19.md` with 436 lines and 15 archived task blocks.
- Added `boards/ARCHIVE/OPS_REVIEWER_ARCHIVE_2026-05-19.md` with 121 lines and 4 archived task blocks.

### History

- 2026-05-19: User asked whether OPS markdown files were necessary, then requested cleaning the files under `boards/OPS`.



## Task: 2026-05-19 Role Markdown Common Rule Compaction

### Task title

Move repeated role rules into a shared common role file.

### Goals

- Keep `AGENTS.md` as the startup and role-entry authority.
- Add a shared common role file instead of repeating evidence, Unity Play Mode, Git, Reviewer, and board-update boundaries across role files.
- Compact Designer, Code Builder, Skill Builder, Code Reviewer, and track files so they keep only role-specific or track-specific instructions.
- Preserve `SimpelWorker` as a minimal role that does not read additional markdown after `AGENTS.md` and `MDTREE.md`.

### Constraints

- Role Owner is Designer because this task changes workflow policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- `AGENTS.md` remains the required startup file; it was not renamed to `AGENTS_COMMON.md`.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown checks.

### Next Actions

- Future role files should add only role-specific instructions and avoid repeating rules already owned by `AGENTS.md`, `MDTREE.md`, or `AGENTS_ROLE/COMMON.md`.
- If more shared rules are needed, add them to `AGENTS_ROLE/COMMON.md` and keep downstream files as references.

### Evidence

- Added `AGENTS_ROLE/COMMON.md` with shared evidence/failure rules, Unity Play Mode boundary, Git and Reviewer boundary, and board update boundary.
- `AGENTS.md` now instructs Designer, Code Builder, Skill Builder, and Code Reviewer to read `AGENTS_ROLE/COMMON.md`; `SimpelWorker` remains excluded.
- `MDTREE.md` now lists `AGENTS_ROLE/COMMON.md` as shared role rules.
- Removed repeated highest-evidence-rule text from `AGENTS_ROLE/GAMEDESIGNER.md`, `AGENTS_ROLE/GAMEBULIDER.md`, and `AGENTS_ROLE/GAMEREVIWER.md`.
- Removed repeated Play Mode, Git, evidence, and board-update boundary text from lower role/track files where those rules are now covered by `AGENTS_ROLE/COMMON.md`.
- Updated Skill Builder and skill blueprint read sets to include `AGENTS_ROLE/COMMON.md`.
- `git diff --check -- AGENTS.md MDTREE.md AGENTS_ROLE\*.md boards\SkillBluePrint\projectile-blueprint.md boards\SkillBluePrint\BeamSkill-blueprint.md BLACKBOARD.md boards\OPS\AUTOMATION_GUIDE.md` completed with no whitespace errors, aside from Git LF-to-CRLF normalization warnings.

### History

- 2026-05-19: User observed that core role markdown files repeated the same requirements and asked to apply the recommended common-rule structure.

## Task: 2026-05-19 Skill Builder Track Routing

### Task title

Move skill implementation blueprint routing into a dedicated Skill Builder track.

### Goals

- Stop adding individual skill-type blueprint rules directly to the Code Builder entry file.
- Add a reusable `Skill Builder` track for projectile, BeamSkill, future zone, and future skill blueprints.
- Keep future skill implementation markdown reads to `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/GAMEBULIDER.md`, `AGENTS_ROLE/GAMEBULIDER_SKILL.md`, and exactly one matching blueprint unless the selected blueprint or inspected failure path justifies more.
- Verify by simulated routing that unrelated MON, DATA, RUN, UI, OPS, archive, and other skill blueprint markdown are excluded for a simple projectile Skill Builder request.

### Constraints

- Role Owner is Designer because this task changes workflow policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- The repository does not contain `CODEBUILDER.md`; the inspected Code Builder entry file is `AGENTS_ROLE/GAMEBULIDER.md`.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown checks.

### Next Actions

- Future skill implementation requests should invoke `Skill Builder` and name or imply exactly one skill blueprint.
- Add future skill types by creating a new `boards/SkillBluePrint/*-blueprint.md` file and, when helpful, adding only a short mapping line in `AGENTS_ROLE/GAMEBULIDER_SKILL.md`.
- If `zone-blueprint.md` is needed, create it before asking Skill Builder to implement zone/area/field skills through that blueprint.

### Evidence

- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now exists and defines the `Skill Builder` track, mandatory markdown read set, blueprint selection, blueprint authority, parsed-input rule, unsupported-behavior rule, routing decision log, and output requirements.
- `AGENTS.md` now recognizes `Skill Builder` as a Code Builder track and routes it through `AGENTS_ROLE/GAMEBULIDER.md` then `AGENTS_ROLE/GAMEBULIDER_SKILL.md`.
- `AGENTS_ROLE/GAMEBULIDER.md` now routes skill implementation, skill runtime wiring, skill prefab/effect connection, and user-invoked `Skill Builder` work to `AGENTS_ROLE/GAMEBULIDER_SKILL.md`.
- Removed the previous `Projectile Skill Blueprint Rule` and `BeamSkill Blueprint Rule` sections from `AGENTS_ROLE/GAMEBULIDER.md`.
- `MDTREE.md` now lists `AGENTS_ROLE/GAMEBULIDER_SKILL.md` under Code Builder track files.
- `boards/SkillBluePrint/projectile-blueprint.md` now uses `AGENTS_ROLE/GAMEBULIDER_SKILL.md` instead of `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md` in its mandatory/allowed markdown read set.
- `boards/SkillBluePrint/BeamSkill-blueprint.md` now has a `What Builder May Read` section using the Skill Builder mandatory read set and explicit conditional markdown rules.

### History

- 2026-05-19: User said the direct projectile/BeamSkill insertions in Code Builder felt messy and would not scale to future `zone_blueprint.md` and other skill blueprints.
- 2026-05-19: User requested a new role named `Skill Builder`, an explanation of deleted/added content, and a simulation proving the Skill Builder path reads only the intended markdown files.

## Task: 2026-05-19 Minimal Markdown Routing Tightening

### Task title

Tighten routing rules so Codex reads the smallest justified markdown set and skips unrelated boards by default.

### Goals

- Split routing guidance into mandatory reads versus conditional reads.
- Explicitly forbid reading unrelated domain markdown "just in case."
- Require a short routing decision log before broader work.
- Tighten the projectile blueprint so projectile implementation does not pull unrelated markdown by default.

### Constraints

- Role Owner is Designer because this task changes workflow policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims must stay grounded in the inspected text of `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/GAMEBULIDER.md`, and `boards/SkillBluePrint/projectile-blueprint.md`.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Future sessions should treat routing as a reduction step and justify every additional markdown read from the user request or the inspected failure path.
- Future projectile implementation tasks should start from the mandatory Builder set and add monster/DATA/RUN/UI boards only when the request or failure path explicitly requires them.

### Evidence

- `AGENTS.md` now says to decide the smallest markdown read set after reading `AGENTS.md` and `MDTREE.md`, to separate mandatory/conditional/excluded reads, and to avoid extra markdown reads "just in case."
- `AGENTS.md` now says that, when practical, the worker should state a short routing decision including request class, files to read next, and intentionally skipped markdown files.
- `MDTREE.md` now has `Minimal Read Set Rule`, explicit exclusion examples, and a policy-routing clause that sends root policy markdown edits to `boards/OPS/AUTOMATION_GUIDE.md` without automatically pulling MON/RUN/UI/DATA boards.
- `AGENTS_ROLE/GAMEBULIDER.md` now has `Minimal Builder Read Set` and `Routing Decision Log`, including explicit conditions for when monster, DATA, RUN, UI, and verification markdown may be added.
- `boards/SkillBluePrint/projectile-blueprint.md` now defines the default mandatory markdown set for projectile implementation and explicitly forbids unrelated UI/RUN/DATA/OPS/other-monster markdown reads unless the request or inspected failure path names those domains.

### History

- 2026-05-19: User noted that Codex could read unnecessary markdown under the existing routing wording and asked to apply the first four tightening ideas: mandatory/conditional split, explicit exclusions, routing decision log, and stronger projectile-blueprint bans.

## Task: 2026-05-19 Projectile Blueprint Parsed-Input And Stop-Ask Rewrite

### Task title

Rewrite the projectile blueprint around parsed input and stop-and-ask rules.

### Goals

- Change `boards/SkillBluePrint/projectile-blueprint.md` from a search-oriented guide into a blueprint-first contract for common projectile work.
- Make future projectile implementation tasks consume caller-provided parsed runtime inputs instead of rediscovering numbers from CSV or reference files.
- Make Builder stop and ask the user whenever a requested projectile behavior falls outside the current common projectile path.
- Remove overly heavy file-inventory style guidance when the real rule is "feed parsed data into the shared projectile runtime."

### Constraints

- Role Owner is Designer because this task changes implementation design policy, not runtime code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims are based on the inspected current shared projectile runtime explanation already grounded in `InGameSkillDefinitionMapper.cs`, `SkillExecutors.cs`, `InGameProjectileActor.cs`, `SkillExecutionSystem.cs`, and the previous projectile blueprint text.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Future projectile implementation tasks should treat `boards/SkillBluePrint/projectile-blueprint.md` as the primary contract and should not reopen CSV/reference sources unless the user explicitly instructs that.
- If a task request does not include the required parsed input fields, Code Builder should stop and report the missing fields instead of searching for them.
- If a task requests timed burst, homing, bounce, last-shot explosion, trap/install, impact-area, mark payload, or other special projectile behavior, Code Builder should stop and ask the user instead of guessing.

### Evidence

- The previous `boards/SkillBluePrint/projectile-blueprint.md` explicitly redirected Builder toward large CSV/reference rediscovery and then toward a heavy `Fixed Implementation Surface` file list.
- The rewritten `boards/SkillBluePrint/projectile-blueprint.md` now centers on `Core Rule`, `Builder Working Mode`, `Required Parsed Input`, `Common Projectile Contract`, `Stop And Ask User Rule`, and `Preferred Builder Response Pattern`.
- The rewritten blueprint now states that projectile numbers and behavior intent must come from caller-provided parsed input, that shared projectile runtime is the default path, and that unsupported special behavior must trigger a user question instead of an inferred implementation.
- 2026-05-19 follow-up: `Optional but common fields` was narrowed to `ChoiceModifierSpecs`, `OnHitStatusId`, `OnHitStatusChance`, `ProjectilePrefabSource`, and `SkillEffectPrefabOverride`; fields such as `ProjectileCount`, `LifetimeSeconds`, `MaxTravelDistance`, `DestroyBoundaryPolicy`, `HitRadius`, `OnHitStatusStacks`, and `OnHitStatusDurationSeconds` were moved out of the current common projectile input contract.
- 2026-05-19 follow-up header check showed the active `Pakuri/Assets/CSVdata/source/monster_skills.csv` does not currently contain those removed fields, while `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` still contains `skill_effect_prefab_path`, so no active CSV column deletion was required for this follow-up.

### History

- 2026-05-19: User said the current projectile blueprint relies too much on other CSV/C# sources and requested a redesign so Builder can implement projectile skills by reading the blueprint alone.
- 2026-05-19: User then clarified that the blueprint should be understandable to AI, should favor parsed-data-to-common-runtime flow, and should stop and ask when a projectile requires special behavior such as timed firing, homing, or last-shot explosion.
- 2026-05-19: User requested shrinking the optional parsed-field list further and asked to remove unsupported field expectations while keeping prefab path support.

