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
