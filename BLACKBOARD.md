# BLACKBOARD.md

## Role

This file is now the root persistent-state index.

Do not use this file as the default detailed task log. Start with `AGENTS.md` and `MDTREE.md`, then read the relevant `boards/` files selected by the routing rules.

The full pre-hierarchy task history is preserved at:

- `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`

## Current Global Status

- Board hierarchy was created on 2026-04-30.
- Detailed task blocks were copied into domain-specific files under `boards/`.
- `BLACKBOARD.md` should stay small and contain only routing, global status, and cross-domain notes.
- Code Reviewer execution requires explicit user permission.
- Unity-MCP Play Mode gameplay verification remains user-owned; Codex records build/compile/console/editor-state evidence only.

## Board Tree

### Monster

- `boards/MON/MON_BLACKBOARD.md`: common monster/player-monster creation rules, terms, skill-slot rules, and monster data history.
- `boards/MON/EVE_MONSTER.md`: Eve-specific skill/runtime implementation history.
- Future character files: `boards/MON/VEGA_MONSTER.md`, `boards/MON/ARIEL_MONSTER.md`, `boards/MON/SEIN_MONSTER.md`, `boards/MON/RIN_MONSTER.md`.

### Combat

- `boards/COMBAT/COMBAT_BLACKBOARD.md`: common combat runtime.
- `boards/COMBAT/ENEMY_BLACKBOARD.md`: enemy spawn, target priority, enemy HP/projectiles.
- `boards/COMBAT/PROJECTILE_BLACKBOARD.md`: player/enemy projectile behavior.
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`: shock, freeze, chill, slow, vulnerability, shield, and other status effects.

### Run

- `boards/RUN/RUN_BLACKBOARD.md`: run flow, day progression, combat type, RunSession.
- `boards/RUN/REWARD_BLACKBOARD.md`: reward buttons, material rewards, skill-choice rewards.
- `boards/RUN/SAVELOAD_BLACKBOARD.md`: save/load and checkpoint planning.

### UI

- `boards/UI/UI_BLACKBOARD.md`: shared UI layout/edit-mode policy.
- `boards/UI/DEBUGSCENE_UI.md`: DebugScene UI canvas, skill panel, enhancement modal, editable scene UI.
- `boards/UI/MAINMENU_UI.md`: main menu and monster selection UI.
- `boards/UI/RUNSCENE_UI.md`: RunScene combat/reward UI.

### Data

- `boards/DATA/DATA_BLACKBOARD.md`: data pipeline overview.
- `boards/DATA/CSV_BLACKBOARD.md`: CSV source data role and limitations.
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`: Unity static assets, `GameDataCatalog`, `MonsterDefinition`.

### Ops

- `boards/OPS/REVIEWER_BLACKBOARD.md`: reviewer wrapper and review flow.
- `boards/OPS/CODEX_CLI_BLACKBOARD.md`: Codex CLI setup and command findings.
- `boards/OPS/UNITY_MCP_BLACKBOARD.md`: Unity MCP bridge and usage notes.
- `boards/OPS/AUTOMATION_GUIDE.md`: automation responsibility rules.

### Reports

- `boards/REPORT/REPORT_BLACKBOARD.md`: HTML/report work history.

## Current Task Block

### Task title

Hierarchical board migration and routing rule update.

### Goals

- Replace the previous always-read `BLACKBOARD.md` workflow with `AGENTS.md` + `MDTREE.md` routing.
- Preserve the old detailed `BLACKBOARD.md` history.
- Split task history into domain-specific board files.
- Add rules requiring related board files to be updated together.

### Constraints

- Preserve evidence and old task history.
- Do not run Unity Play Mode gameplay verification.
- Do not run Code Reviewer without user permission.

### Role Owner

Code Builder

### Status

Completed.

### Next Actions

- Use `AGENTS.md` + `MDTREE.md` routing for future sessions and read `BLACKBOARD.md` only when the routed scope needs global status.
- Run build only when later tasks change code; this migration itself changed markdown only.

### Evidence

- Original detailed `BLACKBOARD.md` was archived to `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`.
- `MDTREE.md` defines routing rules for MON, COMBAT, RUN, UI, DATA, OPS, and REPORT boards.
- `AGENTS.md` now says to read `AGENTS.md` and `MDTREE.md` first, then route to related boards.
- `AGENTS.md` now states Code Reviewer execution requires explicit user permission.
- `AGENTS.md` keeps Unity-MCP Play Mode gameplay verification assigned to the user.
- 2026-05-02 validation: `Test-Path` confirmed `AGENTS.md`, `MDTREE.md`, root `BLACKBOARD.md`, the archive file, and all routed board files exist.
- 2026-05-02 validation: `Get-ChildItem boards -Recurse -File` listed MON, COMBAT, RUN, UI, DATA, OPS, REPORT, and `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`.
- 2026-05-02 validation: `Select-String` over `boards/` for `## Migrated Task Blocks`, `## Task:`, `### Task title`, and `### Status` confirmed migrated task-block sections exist in the domain boards.
- 2026-05-02 validation: `run_codex.bat`, `codex_builder_reviewer.ps1`, and `codex_prompt.txt` all exist at the repository root.

### History

- 2026-04-30: User requested a hierarchical board structure to reduce token use and speed up task routing.
- 2026-04-30: Created domain board hierarchy under `boards/`, added `MDTREE.md`, and changed `BLACKBOARD.md` into a root index.
- 2026-05-02: Validated the root files, archived pre-hierarchy log, domain board hierarchy, migrated task-block structure, and reviewer-wrapper artifacts; the task is now complete.
