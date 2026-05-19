# AUTOMATION_GUIDE

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

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

## Task: 2026-05-18 CSV Runtime Catalog Sync Batch

### Task title

Provide a one-command batch path for syncing and validating CSV runtime catalogs.

### Goals

- Let CSV authors edit `Pakuri/Assets/CSVdata/source/*.csv`, then regenerate the runtime CSV catalogs without navigating Unity menus.
- Reuse the existing Unity editor sync path instead of duplicating catalog generation logic in a script.
- Keep batchmode behavior explicit when the project is already open in Unity.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Claims are based on inspected `PakuriCsvRuntimeData.Editor.cs`, `PakuriCsvRuntimeCatalogPostprocessor.cs`, command output, and Unity console output.

### Role Owner

Code Builder

### Status

Implemented and locally verified up to Unity editor sync/validation. Batchmode launch itself was blocked by Unity's duplicate-project-open guard because the project was already open.

### Next Actions

- If Unity is closed, run `SyncCsvRuntimeCatalogs.bat` from the repository root to execute Unity batchmode sync/validation.
- If Unity is already open, use the menu `Pakuri/Sync CSV Runtime Catalog Assets` or invoke `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` through Unity-MCP/editor tooling.

### Evidence

- `SyncCsvRuntimeCatalogs.bat` exists at the repository root and calls `Unity.exe -batchmode -quit -projectPath "%REPO_DIR%Pakuri" -executeMethod Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` exposes the public static `SyncAndValidateCsvRuntimeCatalogsForEditor()` method used by the batch file.
- Running `cmd /c SyncCsvRuntimeCatalogs.bat` found `C:\Program Files\Unity\Hub\Editor\6000.3.14f1\Editor\Unity.exe` and failed only because another Unity instance already had `C:/TowerDefence_Pakuri/Test/Pakuri` open.
- Unity-MCP invocation of `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` logged successful runtime catalog load with 5 monsters and 8 stage-one enemies, then logged successful sync/validation from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-18: User requested a `.bat` to make CSV runtime catalog/source sync/regeneration easy after editing `monster_skills.csv`.

## Task: 2026-05-17 Projectile Skill Builder Flow

### Task title

Require Code Builder to read the projectile blueprint before projectile skill implementation.

### Goals

- Make future projectile-related implementation commands start from `boards/SkillBluePrint/projectile-blueprint.md`.
- Ensure Code Builder classifies requested projectile behavior as supported, partial, or unsupported before editing code/data/assets.
- Prevent unsupported special behavior such as Vega-A timed three-projectile sequences, branch variants, bounce, homing, installed projectiles, multi-hitbox projectiles, mark payloads, or impact-area projectiles from being treated as already supported common behavior.

### Constraints

- Role Owner is Designer because this is workflow and documentation policy, not runtime implementation.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims are based on inspected `AGENTS_ROLE/GAMEBULIDER.md`, `boards/SkillBluePrint/projectile-blueprint.md`, and routing files.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- On future projectile skill implementation commands, Code Builder reads `AGENTS.md`, `MDTREE.md`, routed boards, `AGENTS_ROLE/GAMEBULIDER.md`, the matching Builder track files, and then `boards/SkillBluePrint/projectile-blueprint.md` before editing.
- Code Builder records whether the requested projectile behavior is common, partial, or exceptional in the implementation summary and related board update.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` exists and contains the current InGame projectile path, supported/partial/unsupported matrix, special behavior rule, new projectile skill checklist, and Code Builder verification expectations.
- Updated `AGENTS_ROLE/GAMEBULIDER.md` with `Projectile Skill Blueprint Rule`.
- The new rule requires reading `boards/SkillBluePrint/projectile-blueprint.md` before projectile-related skill implementation edits.
- The new rule names exceptional behaviors that must not be assumed to be supported by the common projectile path.

### History

- 2026-05-17: User requested that future projectile skill implementation commands make Code Builder refer to `boards/SkillBluePrint/projectile-blueprint.md` and asked how the work flow should be modified.

## Task: 2026-05-18 BeamSkill Builder Flow

### Task title

Require Code Builder to read the BeamSkill blueprint before BeamSkill / LineAttack implementation.

### Goals

- Make future BeamSkill, beam, laser, ray, slash-line, or `LineAttack` implementation commands start from `boards/SkillBluePrint/BeamSkill-blueprint.md`.
- Ensure Code Builder classifies requested BeamSkill behavior as supported, partial, or unsupported before editing code/data/assets.
- Prevent unsupported special behavior such as width/duration choice modifiers, stop-at-first-target, knockback, resistance reduction, chained/curved/sweeping beams, telegraph delay, or custom per-target tick rules from being treated as already supported common behavior.

### Constraints

- Role Owner is Code Builder because the user explicitly requested markdown creation and Code Builder rule wiring.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims are based on inspected `AGENTS_ROLE/GAMEBULIDER.md`, `boards/SkillBluePrint/projectile-blueprint.md`, current BeamSkill runtime scripts, current CSV rows, and the newly added `boards/SkillBluePrint/BeamSkill-blueprint.md`.

### Role Owner

Code Builder

### Status

Implemented and locally verified by targeted file checks.

### Next Actions

- On future BeamSkill / LineAttack implementation commands, Code Builder reads `AGENTS.md`, `MDTREE.md`, routed boards, `AGENTS_ROLE/GAMEBULIDER.md`, the matching Builder track files, and then `boards/SkillBluePrint/BeamSkill-blueprint.md` before editing.
- Code Builder records whether the requested BeamSkill behavior is common, partial, or exceptional in the implementation summary and related board update.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` existed before this task and established the prior projectile blueprint pattern.
- `boards/SkillBluePrint/BeamSkill-blueprint.md` now exists and contains the current InGame BeamSkill / LineAttack path, supported/partial/unsupported matrix, special behavior rule, new BeamSkill checklist, extension points, Eve-B evidence summary, and verification expectations.
- Updated `AGENTS_ROLE/GAMEBULIDER.md` with `BeamSkill Blueprint Rule`.
- The new rule requires reading `boards/SkillBluePrint/BeamSkill-blueprint.md` before BeamSkill, beam, laser, ray, slash-line, or `LineAttack` implementation edits.
- Inspected `Pakuri/Assets/Scripts2/InGame/Skills/Data/BeamSkillData.cs`, `InGameSkillDefinitionMapper.cs`, `SkillExecutors.cs`, `InGameLineAttackActor.cs`, `SkillExecutionSnapshot.cs`, `SkillRuntimeInstance.cs`, `SkillExecutorRegistry.cs`, `Pakuri/Assets/CSVdata/source/monster_skills.csv`, `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv`, and `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` for blueprint evidence.

### History

- 2026-05-18: User requested a BeamSkill blueprint like the existing projectile blueprint and asked to make Code Builder refer to it.

## Task: 2026-05-14 Board Archive Expansion

### Task title

Archive user-specified common combat, projectile, CSV, monster, and refactoring board files.

### Goals

- Move user-specified board files and the full refactoring board folder under `boards/ARCHIVE`.
- Preserve all board history instead of deleting it.
- Update active routing so future work does not read moved files as active boards.

### Constraints

- Role Owner is Designer.
- Preserve files by moving them into `boards/ARCHIVE`.
- Verify resolved move targets stay under the workspace archive directory before moving.
- Do not change runtime code, Unity scenes, or gameplay assets.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Use `MDTREE.md` for active routing after the archive expansion.
- Consult the newly archived files only when older history is needed.

### Evidence

- Moved `boards/COMBAT/COMBAT_BLACKBOARD.md` to `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- Moved `boards/COMBAT/PROJECTILE_BLACKBOARD.md` to `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- Moved `boards/DATA/CSV_BLACKBOARD.md` to `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- Moved `boards/MON/MON_BLACKBOARD.md` to `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- Moved `boards/REFACTORING` to `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14`.
- Updated `MDTREE.md` and `BLACKBOARD.md` active routing/index references.
- Updated active monster board references that previously pointed to `boards/MON/MON_BLACKBOARD.md`.

### History

- 2026-05-14: User explicitly requested judging and archiving `COMBAT_BLACKBOARD.md`, `PROJECTILE_BLACKBOARD.md`, `CSV_BLACKBOARD.md`, `MON_BLACKBOARD.md`, and moving the whole `boards/REFACTORING` folder into `boards/ARCHIVE`.

## Task: 2026-05-12 MON Detail Board Compaction

### Task title

Compact `boards/MON/*.md` files under the active-board cleanup rule.

### Goals

- Apply the active board cleanup pattern to every markdown file under `boards/MON/`.
- Keep only each MON file's latest dated task blocks in the active file.
- Preserve older or undated MON task blocks under `boards/ARCHIVE/`.
- Fix the previously observed MON task block structure problem by removing malformed or older blocks from active files while preserving them in archive.

### Constraints

- Role Owner is Code Builder because the user explicitly requested file restructuring.
- Preserve all moved task history under `boards/ARCHIVE/`.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- Future MON work should read only the relevant active monster file selected by `MDTREE.md`; common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- Older MON task history is available in `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md`.

### Evidence

- Before compaction, `Get-ChildItem -Force -File -LiteralPath boards\MON` listed `ARIEL_MONSTER.md`, `EVE_MONSTER.md`, `MON_BLACKBOARD.md`, `RIN_MONSTER.md`, `SEIN_MONSTER.md`, and `VEGA_MONSTER.md`.
- Before compaction, line/task counts were `ARIEL_MONSTER.md` 292 lines / 8 task blocks, `EVE_MONSTER.md` 631 lines / 13 task headings, `MON_BLACKBOARD.md` 111 lines / 4 task blocks, `RIN_MONSTER.md` 360 lines / 11 task blocks, `SEIN_MONSTER.md` 254 lines / 8 task blocks, and `VEGA_MONSTER.md` 248 lines / 8 task blocks.
- Compaction kept latest dated task blocks by file: `ARIEL_MONSTER.md` kept two `2026-05-10` blocks, `EVE_MONSTER.md` kept one `2026-05-10` block, `MON_BLACKBOARD.md` kept four `2026-05-10` blocks, `RIN_MONSTER.md` kept four `2026-05-08` blocks, `SEIN_MONSTER.md` kept one `2026-05-09` block, and `VEGA_MONSTER.md` kept one `2026-05-10` block.
- Moved 39 older or undated MON task blocks to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md`.
- Added archive notes to every active `boards/MON/*.md` file pointing to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md`.

### History

- 2026-05-12: User asked to clean `C:\TowerDefence_Pakuri\Test\boards\MON` markdown files so they follow the `BLACKBOARD.md` cleanup rules.

## Task: 2026-05-12 Role Folder Move

### Task title

Move role-related markdown files under `AGENTS_ROLE/` and update role-routing paths.

### Goals

- Reduce root `Test/` markdown clutter by moving role-related `GAME*.md` files under `AGENTS_ROLE/`.
- Update `AGENTS.md` role entry points to use `AGENTS_ROLE/...` paths.
- Update `MDTREE.md` root file references to use `AGENTS_ROLE/...` paths.
- Update role entry files so track-specific reads point to `AGENTS_ROLE/...` paths.
- Verify which files are read for refactoring, implementation, and structure-design commands.
- Measure the fixed minimum startup/role-route text as line counts.

### Constraints

- Role Owner is Code Builder because the user explicitly requested file movement and routing updates.
- Preserve the highest absolute evidence rule.
- Preserve the default Designer role behavior.
- Preserve the rule that Code Reviewer execution requires explicit user permission.
- Preserve the Unity Play Mode boundary: user owns gameplay verification, Codex records build/compile/console/editor-state evidence only.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- Future role work should read `AGENTS.md`, `MDTREE.md`, then the role entry file under `AGENTS_ROLE/`, then only the matching track file under `AGENTS_ROLE/`.

### Evidence

- Before the move, `Get-ChildItem -Force -Name GAME*.md` listed root role files including `GAMEBULIDER.md`, `GAMEBULIDER_IMPLEMENTATION.md`, `GAMEBULIDER_REFACT.md`, `GAMEBULIDER_STRUCTURE.md`, `GAMEDESIGNER.md`, `GAMEDESIGNER_REFACT.md`, and `GAMEREVIWER.md`.
- Moved root `GAME*.md` files into `AGENTS_ROLE/`.
- Updated `AGENTS.md` so Designer reads `AGENTS_ROLE/GAMEDESIGNER.md`, Code Builder reads `AGENTS_ROLE/GAMEBULIDER.md`, and Code Reviewer reads `AGENTS_ROLE/GAMEREVIWER.md`.
- Updated `MDTREE.md` root file descriptions to list `AGENTS_ROLE/GAMEDESIGNER_*`, `AGENTS_ROLE/GAMEBULIDER_*`, and `AGENTS_ROLE/GAMEREVIWER.md`.
- Updated `AGENTS_ROLE/GAMEDESIGNER.md` and `AGENTS_ROLE/GAMEBULIDER.md` track routing entries to point to `AGENTS_ROLE/...` files.
- After the move, `Get-ChildItem -Force -Name GAME*.md` at the repository root returned no role markdown files.
- `Get-ChildItem -Force -LiteralPath AGENTS_ROLE` listed the moved role markdown files under `AGENTS_ROLE/`.
- `Test-Path` confirmed `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/GAMEDESIGNER.md`, `AGENTS_ROLE/GAMEDESIGNER_REFACT.md`, `AGENTS_ROLE/GAMEDESIGNER_STRUCTURE.md`, `AGENTS_ROLE/GAMEBULIDER.md`, `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md`, `AGENTS_ROLE/GAMEBULIDER_REFACT.md`, and `AGENTS_ROLE/GAMEBULIDER_STRUCTURE.md` exist.
- `Select-String` confirmed `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/GAMEDESIGNER.md`, and `AGENTS_ROLE/GAMEBULIDER.md` now route role reads through `AGENTS_ROLE/...` paths.
- Fixed minimum line-count checks returned 183 lines for refactor-design routing, 183 lines for implementation routing, 182 lines for structure-design routing, 180 lines for Builder refactor-implementation routing, and 183 lines for Builder structure-support routing, excluding domain boards and target code files.

### History

- 2026-05-12: User requested moving role-related markdown files out of the root `Test/` folder into `AGENTS_ROLE/`, updating `AGENTS.md` paths, verifying task-command routing, and reporting which paths and minimum fixed line counts are used for refactoring, implementation, and structure-design commands.

## Task: 2026-05-12 Role Track File Split

### Task title

Split Designer and Code Builder role rules into lightweight entry files and track-specific files.

### Goals

- Keep `GAMEDESIGNER.md` and `GAMEBULIDER.md` light.
- Move detailed Designer structure, implementation handoff, refactoring, gameplay, and handoff rules into separate `GAMEDESIGNER_*` files.
- Move detailed Code Builder structure, implementation, refactoring, quality, UI, and verification rules into separate `GAMEBULIDER_*` files.
- Make each role entry file explain which detailed file to read for each work type.
- Update routing/global status references so future sessions know the track files exist.

### Constraints

- Role Owner is Code Builder because the user explicitly requested markdown file restructuring.
- Preserve the highest absolute evidence rule.
- Preserve the default Designer role behavior.
- Preserve the rule that Code Reviewer execution requires explicit user permission.
- Preserve the Unity Play Mode boundary: user owns gameplay verification, Codex records build/compile/console/editor-state evidence only.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- Future Designer work should read `AGENTS_ROLE/GAMEDESIGNER.md`, then only the needed `AGENTS_ROLE/GAMEDESIGNER_*` track files.
- Future Code Builder work should read `AGENTS_ROLE/GAMEBULIDER.md`, then only the needed `AGENTS_ROLE/GAMEBULIDER_*` track files.

### Evidence

- Before the split, `Get-ChildItem -Force -Name GAME*.md` listed only `GAMEBULIDER.md`, `GAMEDESIGNER.md`, and `GAMEREVIWER.md`.
- Replaced `GAMEDESIGNER.md` with a lightweight entry point routing to `GAMEDESIGNER_STRUCTURE.md`, `GAMEDESIGNER_IMPLEMENTATION.md`, `GAMEDESIGNER_REFACT.md`, `GAMEDESIGNER_GAMEPLAY.md`, and `GAMEDESIGNER_HANDOFF.md`.
- Replaced `GAMEBULIDER.md` with a lightweight entry point routing to `GAMEBULIDER_STRUCTURE.md`, `GAMEBULIDER_IMPLEMENTATION.md`, `GAMEBULIDER_REFACT.md`, `GAMEBULIDER_QUALITY.md`, `GAMEBULIDER_UI.md`, and `GAMEBULIDER_VERIFICATION.md`.
- Updated `MDTREE.md` root file descriptions to list the new track files.
- Updated root `BLACKBOARD.md` current global status with the role-track split note.

### History

- 2026-05-12: User requested subdividing the refactoring, implementation, and structure-design content in `GAMEBULIDER.md` and `GAMEDESIGNER.md` into files such as `GAMEDESIGNER_REFACT.md` and `GAMEBULIDER_REFACT.md`, while leaving the original role files as light routing entry points.

## Task: 2026-05-12 Role File Split

### Task title

Split `AGENTS.md` role rules into dedicated role files.

### Goals

- Move Designer-specific instructions from `AGENTS.md` into `GAMEDESIGNER.md`.
- Move Code Builder-specific instructions from `AGENTS.md` into `GAMEBULIDER.md`.
- Move Code Reviewer-specific instructions from `AGENTS.md` into `GAMEREVIWER.md`.
- Leave `AGENTS.md` focused on startup, evidence, routing, persistent-state, and role entry-point rules.
- Update routing/global status references so future sessions know the role files exist.

### Constraints

- Role Owner is Code Builder because the user explicitly requested file restructuring.
- Preserve the highest absolute evidence rule.
- Preserve the default Designer role behavior.
- Preserve the rule that Code Reviewer execution requires explicit user permission.
- Preserve the Unity Play Mode boundary: user owns gameplay verification, Codex records build/compile/console/editor-state evidence only.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- Future sessions should read `AGENTS.md` and `MDTREE.md`, then read the active role file named by `AGENTS.md`.
- If the user wants corrected English spellings, create a separate migration from `AGENTS_ROLE/GAMEBULIDER.md` / `AGENTS_ROLE/GAMEREVIWER.md` to corrected filenames and update every reference together.

### Evidence

- Before the split, `Get-ChildItem -Force -Name GAMEDESIGNER.md,GAMEBULIDER.md,GAMEREVIWER.md,GAMEBUILDER.md,GAMEREVIEWER.md` reported that none of those files existed.
- Added `GAMEDESIGNER.md`, `GAMEBULIDER.md`, and `GAMEREVIWER.md`.
- Replaced `AGENTS.md` so it now points Designer to `GAMEDESIGNER.md`, Code Builder to `GAMEBULIDER.md`, and Code Reviewer to `GAMEREVIWER.md`.
- Updated `MDTREE.md` root file descriptions to list the new role files.
- Updated root `BLACKBOARD.md` current global status with the role-file split note.

### History

- 2026-05-12: User requested separating the current `AGENTS.md` role functions into `GAMEDESIGNER.md`, `GAMEBULIDER.md`, and `GAMEREVIWER.md`, with `AGENTS.md` only pointing to the role files when each role is performed.

## Task: 2026-05-12 Blackboard Seven-Day Archive Pass

### Task title

Compact `boards/**/*BLACKBOARD.md` files and archive older task blocks by seven-day ranges.

### Goals

- Keep each active `*BLACKBOARD.md` file to only the newest dated day of task blocks.
- Move older or undated task blocks into `boards/ARCHIVE/`.
- Group dated archived task blocks into seven-day archive files.
- Add a Code Builder rule asking whether completed or no-longer-needed active task blocks should be archived.

### Constraints

- Role Owner is Code Builder because the user explicitly requested file restructuring.
- Preserve task block content instead of deleting history.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- Future board maintenance can use the archive files under `boards/ARCHIVE/` for older task history.
- When a user says a task is done, or Builder determines a task no longer needs active context, Builder should ask whether to archive it.

### Evidence

- `Get-ChildItem -Path boards -Recurse -File -Filter *BLACKBOARD.md` found 16 active `*BLACKBOARD.md` files outside `boards/ARCHIVE/`.
- Reparse summary after deduplication showed active files retain only their latest dated day: for example `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md` keeps four `2026-05-10` blocks, the then-active report board kept one `2026-05-12` block, and undated-only files kept no task blocks.
- Created or rewrote `boards/ARCHIVE/BLACKBOARD_2026-04-20_to_2026-04-26_ARCHIVE_2026-05-12.md`.
- Created or rewrote `boards/ARCHIVE/BLACKBOARD_2026-04-27_to_2026-05-03_ARCHIVE_2026-05-12.md`.
- Created or rewrote `boards/ARCHIVE/BLACKBOARD_2026-05-04_to_2026-05-10_ARCHIVE_2026-05-12.md`.
- Created or rewrote `boards/ARCHIVE/BLACKBOARD_UNDATED_ARCHIVE_2026-05-12.md`.
- Updated `GAMEBULIDER.md` with the rule to ask before moving completed or no-longer-needed active task blocks to `boards/ARCHIVE/`.

### History

- 2026-05-12: User said category `*BLACKBOARD.md` files under `boards/` were too large and requested keeping only one latest day in each file, moving the rest under `boards/ARCHIVE/` in seven-day units, and adding a simple Builder rule to ask about archiving completed or unnecessary task blocks.

## Task: Hierarchical Board Migration And Routing Rule Update

### Task title

Move persistent-state routing from always reading `BLACKBOARD.md` to `AGENTS.md` + `MDTREE.md` + domain boards.

### Goals

- Reduce token use by routing to relevant board files instead of reading the full root board.
- Preserve the previous full `BLACKBOARD.md` history in an archive.
- Require simultaneous updates to every related board file when a task crosses domains.
- Record that Code Reviewer execution needs user permission and Unity-MCP Play Mode gameplay verification is user-owned.

### Constraints

- Role Owner is Code Builder for this migration.
- Preserve old detailed task history.
- Do not run Code Reviewer without user permission.
- Do not run Unity-MCP Play Mode gameplay verification.

### Role Owner

Code Builder

### Status

Implemented pending validation.

### Next Actions

- Validate file existence and routing references.
- Use this rule set for future task routing.

### Evidence

- `AGENTS.md` now says to read `AGENTS.md` and `MDTREE.md` first.
- `MDTREE.md` defines MON, COMBAT, RUN, UI, DATA, OPS, and REPORT routing.
- `BLACKBOARD.md` is now a root index.
- The previous full root board was archived at `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`.

### History

- 2026-04-30: User requested hierarchical board routing and simultaneous related board updates to avoid drift.

## Task: 2026-05-18 Report Board Removal And CSV UTF-8 Policy

### Task title

Remove the active REPORT board from routing and add explicit report/CSV authoring rules.

### Goals

- Delete the active report board so agents no longer route report work through it.
- Add a short English Designer rule for HTML report structure.
- Add an explicit Code Builder rule that CSV creation/editing must use UTF-8.
- Check current CSV files and convert them only if they are not already valid UTF-8.

### Constraints

- Role Owner is Code Builder because the user explicitly requested markdown and policy file changes.
- Keep report history in archive files instead of reviving an active report board.
- Base the CSV encoding conclusion on inspected file bytes, not editor assumptions.
- Do not rewrite already valid UTF-8 CSV files only to change BOM style.

### Role Owner

Code Builder

### Status

Implemented and locally verified by file inspection.

### Next Actions

- Future report or HTML tasks should route through the relevant active domain board plus the referenced source files, not through an active report board.
- Future CSV edits should preserve the current schema/content and stay UTF-8.

### Evidence

- The active report board existed before this task as the only file under the removed report-board folder.
- `MDTREE.md` now states there is no active report board and that report/documentation work reads the related active domain board only.
- `AGENTS_ROLE/GAMEDESIGNER.md` now includes a short English HTML report structure rule.
- `AGENTS_ROLE/GAMEBULIDER.md` now includes a `CSV Encoding Rule` that requires UTF-8 and forbids BOM-only rewrites unless the user asks.
- Active cross-board references in `boards/DATA/DATA_BLACKBOARD.md`, `boards/MON/EVE_MONSTER.md`, and `BLACKBOARD.md` were updated so they no longer point at an active report board.
- Byte validation over every current `Pakuri/Assets/**/*.csv` file reported `Utf8Valid=True`; no inspected CSV required data-preserving UTF-8 conversion in this task.

### History

- 2026-05-18: User requested deleting the active REPORT board, stopping future routing through it, adding report-format guidance to the Designer role file, and adding a UTF-8 CSV rule to the Code Builder role file.

## Task: 2026-05-18 SimpelWorker Role

### Task title

Add a minimal SimpelWorker role for very simple path-based work.

### Goals

- Add a `SimpelWorker` role entry to `AGENTS.md`.
- Create a dedicated role file for simple file-rename or information-extraction tasks.
- Keep SimpelWorker from reading extra markdown files after the required startup reads.
- Make SimpelWorker fall back to Designer when no exact work path is provided.

### Constraints

- Role Owner is Code Builder because the user explicitly requested markdown policy changes.
- Preserve the existing startup rule that `AGENTS.md` and `MDTREE.md` must be read first.
- Keep the new role lightweight and path-bounded.

### Role Owner

Code Builder

### Status

Implemented and locally verified by file inspection.

### Next Actions

- Use `SimpelWorker` only for clearly bounded simple tasks with an exact work path.
- Use Designer when the task path is missing or the scope expands beyond a trivial operation.

### Evidence

- `AGENTS.md` now lists `SimpelWorker: read AGENTS_ROLE/SIMPELWORKER.md`.
- `AGENTS.md` now states that SimpelWorker is for very simple work, reads no additional markdown after `AGENTS.md` and `MDTREE.md`, and falls back to Designer when no exact work path is provided.
- `AGENTS_ROLE/SIMPELWORKER.md` now exists and defines the role scope, markdown read rule, fallback rule, and scope limit.
- `MDTREE.md` now lists `AGENTS_ROLE/SIMPELWORKER.md` in the role entry-point section.

### History

- 2026-05-18: User requested adding a SimpelWorker role for very simple tasks and making it auto-fall back to Designer when no exact work path is given.

## Migrated Task Blocks

## Task: Token Optimized Board Routing Report

### Task title

Document the current token-optimized board routing workflow.

### Goals

- Record that the routing/report explanation was created for the AGENTS/MDTREE/boards workflow.
- Keep automation guidance aligned with the new method: read `AGENTS.md`, route through `MDTREE.md`, then read only relevant boards.
- Preserve the rule that Code Reviewer execution requires explicit user permission.

### Constraints

- Role Owner is Code Builder for this saved report task.
- Do not claim gameplay validation.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Completed pending user review.

### Next Actions

- Continue using `MDTREE.md` as the routing entry point for future automation and documentation tasks.

### Evidence

- `AGENTS.md` now defines `BLACKBOARD.md` as a root index and sends detailed state to `boards/`.
- `MDTREE.md` provides the routing table for domain board reads.
- `BLACKBOARD.md` points to `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`.
- Added report: `Pakuri/reference/Report/2026-04-30-token-optimized-board-routing.html`.

### History

- 2026-04-30: User requested a saved HTML explanation of token optimization changes to `AGENTS.md`, `BLACKBOARD.md`, and the work method.

## Task: Combat Automation Responsibility Guide

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `reference/current-architecture-plan.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/current-architecture-plan.html`.
- Legacy non-English note retained these code references: `manage_asset search`, `Assets`, `Scenes`, `Settings`, `Assets/Scripts`.
- Legacy non-English note retained these code references: `Get-ChildItem Pakuri\\Assets`, `Scenes`, `Settings`.
- Legacy non-English note retained these code references: `manage_scene get_hierarchy`, `SampleScene`, `Main Camera`, `Global Light 2D`.
- Legacy non-English note retained these code references: `debug_request_context`, `Pakuri@c88ab184`.
- Legacy non-English note retained these code references: `manage_scene get_active`, `manage_scene get_hierarchy`, `run_tests EditMode`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `reference/current-architecture-plan.html`.
- Legacy non-English note retained these code references: `manage_asset search`, `Get-ChildItem Pakuri\\Assets`, `manage_scene get_hierarchy`.
- Legacy non-English note retained these code references: `Pakuri/reference`.

## Task: Token Efficient Reviewer Wrapper

### Task title

Reduce unnecessary token use in the external Builder -> Reviewer wrapper while preserving evidence-based review.

### Goals

- Stop wrapper prompts from encouraging full `BLACKBOARD.md` dumps.
- Keep `AGENTS.md` full-read behavior and preserve related `BLACKBOARD.md` block checks.
- Provide Reviewer with direct changed-file evidence so it can review changed lines without broad repeated exploration.
- Create an HTML report explaining the before/after problem and solution.

### Constraints

- Role Owner is Code Builder.
- All claims must be grounded in actual files and command output.
- Because this modifies the external reviewer wrapper logic, Code Reviewer review is required after Builder implementation.

### Role Owner

Code Builder

### Status

Builder implementation, local validation, Reviewer feedback fixes, and external Code Reviewer PASS completed.

### Next Actions

- On the next actual wrapper run, compare new `*.console.txt` `tokens used` values against the prior 59k-83k token smoke-test logs.

### Evidence

- `codex_builder_reviewer.ps1` now adds `Get-BlackboardIndexText`, `Limit-Text`, `Get-ChangedPathList`, `Get-GitDiffText`, and `Get-AddedFileEvidenceText`.
- The wrapper now writes `blackboard_index.txt`, `loop_XX_git_diff.patch`, and `loop_XX_changed_file_evidence.txt` for each loop.
- `loop_XX_git_diff.patch` is git diff evidence for tracked changes; `loop_XX_changed_file_evidence.txt` is the fallback content evidence for existing changed files including untracked additions.
- Builder and Reviewer prompts now instruct agents to read `AGENTS.md` in full but use `BLACKBOARD.md` through the generated index and related task blocks instead of printing the full file.
- Reviewer prompts now include git diff evidence and changed file content evidence excerpts.
- Added `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- PowerShell parser validation for `codex_builder_reviewer.ps1` returned `PARSE_OK`.
- `git status --short` after Builder implementation showed `M codex_builder_reviewer.ps1`, `M BLACKBOARD.md`, and untracked `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- External Code Reviewer final rerun returned `REVIEW_RESULT: PASS` in `codex_loop_logs/token_wrapper_reviewer_20260428_rerun2.md`.
- `AGENTS.md` now says Reviewer runs once only, then reports issues to the user instead of continuing an automatic fix loop.
- `AGENTS.md` now says Codex does not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification, while Codex records build/compile/console/editor-state evidence only.

### History

- 2026-04-28: User asked to change the workflow so token use is reduced without weakening evidence-based hallucination prevention, and to create an HTML before/after report.
- 2026-04-28: Code Builder changed the wrapper to create targeted BLACKBOARD and changed-file evidence, then created the HTML report.
- 2026-04-28: External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES` because the HTML report overstated `loop_XX_git_diff.patch` as full changed diff evidence for untracked added files.
- 2026-04-28: Code Builder corrected the HTML report and BLACKBOARD wording to distinguish tracked git diff evidence from changed file content evidence.
- 2026-04-28: External Code Reviewer rerun still found one remaining HTML sentence that overstated full diff patch evidence; Code Builder corrected that sentence.
- 2026-04-28: External Code Reviewer final rerun returned `REVIEW_RESULT: PASS`.
- 2026-04-28: User requested a simple `AGENTS.md` policy update for one Reviewer run only and user-owned Unity-MCP Play Mode verification; Code Builder added the wording to `AGENTS.md` and the HTML report.

