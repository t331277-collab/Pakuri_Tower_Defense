# MDTREE.md

## Purpose

`MDTREE.md` routes persistent work to the smallest current markdown set.

Start every session by reading `AGENTS.md` and this file. Read `BLACKBOARD.md` only for global, ambiguous, board-policy, or overall-status work.

## Minimal Read Set

1. Read `AGENTS.md` and `MDTREE.md`.
2. Select one primary role and one primary active board.
3. Add a second board only when the user request or an inspected code/error path directly crosses that domain.
4. Do not read archives unless historical evidence is required.

## Root And Role Files

- Root status: `BLACKBOARD.md`
- Shared role rules: `AGENTS_ROLE/COMMON.md`
- Designer: `AGENTS_ROLE/GAMEDESIGNER.md`
- Code Builder: `AGENTS_ROLE/GAMEBULIDER.md`
- Code Reviewer: `AGENTS_ROLE/GAMEREVIWER.md`
- Naive Code Filter: `AGENTS_ROLE/NAIVE_CODE_FILTER.md`
- SimpelWorker: `AGENTS_ROLE/SIMPELWORKER.md`
- Skill Builder: `AGENTS_ROLE/GAMEBULIDER_SKILL.md` — preserved but inactive

Code Builder tracks:

- Structure: `AGENTS_ROLE/GAMEBULIDER_STRUCTURE.md`
- Implementation: `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md`
- Refactoring: `AGENTS_ROLE/GAMEBULIDER_REFACT.md`
- Quality: `AGENTS_ROLE/GAMEBULIDER_QUALITY.md`
- UI: `AGENTS_ROLE/GAMEBULIDER_UI.md`
- Verification: `AGENTS_ROLE/GAMEBULIDER_VERIFICATION.md`

## Active Boards

### Global / Policy / Automation

- Root current status: `BLACKBOARD.md`
- Role, routing, archive, and automation work: `boards/OPS/AUTOMATION_GUIDE.md`

### Skill Trigger Executor Reuse

- Current consolidation design: `boards/COMBAT/SKILL_TRIGGER_REACTION_LOGIC_CONSOLIDATION_HANDOFF.md`
- Implemented baseline handoff: `boards/COMBAT/SKILL_TRIGGER_EXECUTOR_REUSE_HANDOFF.md`
- Combat-side current task: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- Data-contract current task: `boards/DATA/DATA_BLACKBOARD.md`

These are the only active detailed task records after the 2026-07-28 cleanup.

### Domain Indexes With No Active Task

- Monster work: `boards/MON/MON_BLACKBOARD.md`
- Run/reward/save work: `boards/RUN/RUN_BLACKBOARD.md`
- UI work: `boards/UI/UI_BLACKBOARD.md`

Read these only when the named domain is requested. Add a required-field task block there when new persistent work starts.

## Archive Routes

- 2026-07-28 pre-cleanup domain snapshots: `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/`
- Skill Builder blueprints: `boards/ARCHIVE/SkillBluePrint/` — historical only; never active implementation authority
- Pre-compaction root index: `boards/ARCHIVE/BLACKBOARD_2026-07-28_PRE_COMPACTION.md`
- Pre-July board history: `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md`
- Pre-hierarchy root history: `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`

Do not read an archive by default. Read one only when the user requests historical context or current source evidence is insufficient.

## Routing Rules

### Global Or Ambiguous Work

Read `BLACKBOARD.md`. For root policy, role, routing, archive, or board maintenance also read `boards/OPS/AUTOMATION_GUIDE.md`.

### Combat / Skill Runtime Work

For the current Trigger executor-reuse migration, read the handoff and the narrow COMBAT or DATA task board required by the requested change. Inspect the exact C# and CSV files before making claims.

For unrelated new combat work, start from the exact code path. Create a new required-field task block in the narrowest applicable active board only when persistent state is needed.

### Data / CSV / Asset Work

Read `boards/DATA/DATA_BLACKBOARD.md` only when the request or inspected path touches CSV, parsing, validation, catalogs, schemas, or asset wiring.

### Monster Work

Read `boards/MON/MON_BLACKBOARD.md` and the exact monster code/data requested. Historical per-monster boards are archived and must not be read unless older evidence is required.

### Run / Reward / Save Work

Read `boards/RUN/RUN_BLACKBOARD.md` and the exact Run code/data requested.

### UI Work

Read `boards/UI/UI_BLACKBOARD.md` and the exact UI code, scene, prefab, UXML, USS, or asset requested.

### Reviewer / Codex / Unity-MCP / Automation Work

Read `boards/OPS/AUTOMATION_GUIDE.md`. Code Reviewer execution still requires explicit user permission. Unity Play Mode gameplay verification remains user-owned.

### Skill Builder

Skill Builder is inactive. Read only `AGENTS_ROLE/GAMEBULIDER_SKILL.md` to report the inactive boundary. Do not route work through archived blueprints.

## Update Rules

Every new task block must include:

- Task title
- Goals
- Constraints
- Role Owner
- Status
- Next Actions
- Evidence
- History

When work changes facts in multiple active domains, update every directly affected active board in the same turn. Preserve completed or superseded detailed history under `boards/ARCHIVE/`.
