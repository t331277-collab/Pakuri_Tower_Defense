# AUTOMATION_GUIDE

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

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
- Reparse summary after deduplication showed active files retain only their latest dated day: for example `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md` keeps four `2026-05-10` blocks, `boards/REPORT/REPORT_BLACKBOARD.md` keeps one `2026-05-12` block, and undated-only files keep no task blocks.
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

