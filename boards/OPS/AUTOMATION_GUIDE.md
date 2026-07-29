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
