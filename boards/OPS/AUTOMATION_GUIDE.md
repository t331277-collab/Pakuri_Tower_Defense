# AUTOMATION_GUIDE

## Archived History

The pre-cleanup OPS automation, Codex CLI, Reviewer, and Unity-MCP boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/OPS/`.

## Task: 2026-07-28 Active Board And Skill Builder Archive Cleanup

### Task title

Deactivate Skill Builder and archive obsolete active-board content.

### Goals

- Preserve Skill Builder as an inactive named role.
- Remove all active routing to its former blueprints.
- Move `boards/SkillBluePrint` intact under `boards/ARCHIVE/`.
- Keep only the current Trigger/Node design task active.
- Preserve completed, superseded, empty-shell, and old COMBAT, DATA, MON, OPS, RUN, and UI records under a dated archive snapshot.
- Align `AGENTS.md`, `MDTREE.md`, role files, and root status with the resulting paths.

### Constraints

- Role Owner is Code Builder.
- No C#, CSV, scene, prefab, Unity asset, or gameplay behavior changes are part of this task.
- No historical record is deleted.
- The 2026-07-28 Trigger/Node handoff and its COMBAT/DATA task blocks remain active.
- Archived Skill Builder blueprints are historical evidence only and cannot authorize implementation.

### Role Owner

Code Builder

### Status

Implemented and structurally validated.

### Next Actions

- Use only the active routes in `MDTREE.md` for future work.
- Read the dated snapshots only when historical evidence is explicitly required.
- Reactivate Skill Builder only through a new explicit user policy request.

### Evidence

- Pre-move audit found 22 Markdown files in the six requested domain folders excluding the current Trigger/Node handoff.
- Task-status inspection found the retained 2026-07-28 COMBAT/DATA design task as the only current unimplemented cross-domain work.
- `Pakuri/Assets/Scripts` contains 70 C# files: Combat 31, Data 5, GameFlow 11, InGame 1, UI 8, and Units 14.
- The move command preserved the 22 domain files under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/`.
- The seven former blueprint files moved intact to `boards/ARCHIVE/SkillBluePrint/`.
- The former root index moved intact to `boards/ARCHIVE/BLACKBOARD_2026-07-28_PRE_COMPACTION.md`.
- Hash comparison checked 26 unchanged moved files against their Git `HEAD` blobs with 0 missing files and 0 mismatches.
- The archived root `BLACKBOARD.md` hash exactly matches its Git `HEAD` blob.
- All 29 concrete paths parsed from `MDTREE.md` exist.
- Active policy and board files contain 0 references to the removed active blueprint path and 0 references to the removed active domain-board paths.
- Three active `## Task:` blocks and the standalone Trigger/Node handoff contain all eight required sections.
- Strict UTF-8 decoding passed for 44 policy, active-board, and moved archive files with 0 errors.
- `git diff --check` passed; Git emitted only working-copy LF-to-CRLF notices.
- No C#, CSV, scene, prefab, or Unity asset file changed.

### History

- 2026-07-28: User explicitly selected Code Builder, deactivated Skill Builder while preserving the role, requested the whole SkillBluePrint archive move, and requested old or structure-incompatible content removal from the six domain folders.
- 2026-07-28: Code Builder inspected role policy, all target-board task statuses, standalone design documents, current script inventory, and exact move targets before changing files.
- 2026-07-28: Code Builder completed the archive move, rebuilt the minimal active routes, and passed path, hash, task-schema, UTF-8, and Git diff validation.

## Task: 2026-07-29 Global Ponytail Codex Plugin Installation

### Task title

Install and activate the upstream Ponytail plugin for global Codex use.

### Goals

- Register the upstream Ponytail marketplace globally.
- Install and enable `ponytail@ponytail`.
- Confirm the installed version, Node runtime, bundled skills, and lifecycle hooks.
- Give the user the exact activation and usage commands.

### Constraints

- Use the inspected upstream repository and installed plugin files as evidence.
- Do not modify project C#, CSV, scene, prefab, or Unity assets.
- Lifecycle hooks require user review and trust through `/hooks`.
- A newly installed plugin requires a new Codex thread or application restart before its skills and hooks are loaded.

### Role Owner

Designer

### Status

Plugin installed and enabled. User-owned restart and hook trust remain.

### Next Actions

- User restarts Codex or opens a new thread.
- User opens `/hooks`, reviews the Ponytail lifecycle hooks, and trusts them.
- User invokes `@ponytail-help` or uses the automatically active default `full` mode.

### Evidence

- `codex plugin marketplace add DietrichGebert/ponytail` added marketplace `ponytail` from the upstream Git repository.
- `codex plugin add ponytail@ponytail` installed the plugin under `C:\Users\t3312\.codex\plugins\cache\ponytail\ponytail\4.8.4`.
- `codex plugin list` reports `ponytail@ponytail` as `installed, enabled`, version `4.8.4`.
- `node --version` reports `v24.14.0`.
- The installed manifest points to `./skills/` and `./hooks/claude-codex-hooks.json`.
- The installed help skill lists Codex invocations `@ponytail`, `@ponytail-review`, and `@ponytail-help`; the skills directory also contains audit, debt, and gain skills.
- The inspected hook manifest registers `SessionStart`, `SubagentStart`, and `UserPromptSubmit` Node commands.

### History

- 2026-07-29: User approved global Ponytail installation and requested usage instructions.
- 2026-07-29: Designer registered the marketplace, installed and enabled Ponytail 4.8.4, and verified the local runtime and installed files.

## Task: 2026-07-30 Global Script Comment Normalization

### Task title

Replace stale comments under `Pakuri/Assets/Scripts` with complete Korean responsibility summaries.

### Goals

- Remove every pre-existing source comment from the 73 current C# scripts.
- Add a concise Korean role and responsibility header at the top of every script.
- Add a concise Korean responsibility summary to every type, method, constructor, and local function declaration.
- Preserve all non-comment code and runtime behavior.

### Constraints

- Role Owner is Code Builder.
- Scope is limited to `Pakuri/Assets/Scripts/**/*.cs` plus this persistent-state record.
- Comments must describe responsibilities found in the inspected current source.
- Public APIs, serialized fields, source tokens, preprocessor directives, scenes, prefabs, CSV data, and Unity assets must not change.
- Unity Play Mode remains user-owned.

### Role Owner

Code Builder

### Status

Completed and verified.

### Next Actions

- Use the new file and declaration summaries as the current structural documentation.
- Update the matching summary whenever a script or declaration responsibility changes.
- Run user-owned Play Mode gameplay verification only if behavioral work is later added.

### Evidence

- Initial inventory found 73 scripts and 3,773 lines containing legacy comment syntax under `Pakuri/Assets/Scripts`.
- Roslyn inspection with `UNITY_EDITOR` enabled found 244 type declarations, 1,195 method or constructor declarations, and 1 local function.
- Final structural audit found 73/73 file headers and 1,440/1,440 documented declarations, with 0 missing declarations and 0 unexpected legacy or inline comments.
- Korean localization audit found Korean text in 73/73 file headers and 1,440/1,440 declaration summaries, with 0 missing Korean summaries.
- A Roslyn token and preprocessor comparison against Git `HEAD` found 0 non-comment code mismatches across all 73 scripts.
- The Korean comment conversion compared every file before and after localization and found 0 non-comment code-token or preprocessor mismatches.
- `dotnet build Assembly-CSharp.csproj --no-restore --verbosity quiet` passed with 0 errors and the existing 2 assembly-reference warnings.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity quiet` passed with 0 errors and the existing 2 assembly-reference warnings.
- Unity script refresh and compilation completed with the editor idle and 0 error console entries.
- Unity EditMode test job `f129a3b2a83947de9be624252c6666b1` passed 9/9 tests with 0 failures and 0 skips.
- After Korean localization, Unity EditMode test job `0c77612c4dc643bcaf92b7722801eafb` passed 9/9 tests with 0 failures and 0 skips.
- The temporary Roslyn audit utility and its generated build output were removed after verification.

### History

- 2026-07-30: User explicitly selected Code Builder and requested complete replacement of all comments under `Pakuri/Assets/Scripts`.
- 2026-07-30: Code Builder inspected the current file, type, method, constructor, local-function, comment, and build baseline before editing.
- 2026-07-30: Code Builder replaced legacy comments with file-level role/responsibility headers and declaration-level summaries without changing non-comment source tokens.
- 2026-07-30: Code Builder completed static coverage, Git token comparison, runtime/editor builds, Unity compilation, console inspection, and the full EditMode test suite.
- 2026-07-30: User requested Korean comments without damaging their meaning; Code Builder localized all file and declaration summaries while preserving code identifiers and responsibility scope.
- 2026-07-30: Code Builder revalidated Korean coverage, non-comment token identity, runtime/editor builds, Unity compilation, the error console, and the full EditMode test suite.
