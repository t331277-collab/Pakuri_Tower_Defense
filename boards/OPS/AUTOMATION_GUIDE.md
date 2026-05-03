# AUTOMATION_GUIDE

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

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

