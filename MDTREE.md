# MDTREE.md

## Purpose

`MDTREE.md` is the routing index for persistent work state.

Start every session by reading `AGENTS.md` and this file. Read `BLACKBOARD.md` only when the request is global, ambiguous, or specifically about board policy/status.

## Minimal Read Set Rule

Treat routing as a reduction step, not as permission to read broadly.

Apply this order:
1. Read the mandatory startup files.
2. Pick the one primary domain that matches the user request.
3. Add conditional boards only when the request or the inspected failure path explicitly touches that axis.
4. Skip every other markdown file.

Conditional reads must be justified by one of these:
- the user explicitly asked for that domain;
- the inspected error message names that domain;
- the inspected code path being edited directly crosses into that domain.

Do not add a board only because it "might be related."

Examples of exclusions:
- Do not read UI boards for a projectile-runtime task unless the user or the inspected failure names UI objects, buttons, canvases, TMP, UXML, or USS.
- Do not read DATA or asset boards for a runtime logic task unless the user or the inspected failure names CSV, prefab, scene serialization, catalog, or asset wiring.
- Do not read RUN boards for a monster implementation task unless the request or inspected path explicitly touches `RunSession`, Offering, Menifest, or `NewRunScene` flow ownership.

## Root Files

- `AGENTS.md`: startup, evidence, routing, and role entry-point rules.
- `MDTREE.md`: this routing tree.
- `BLACKBOARD.md`: root index and current global status only.
- `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`: full pre-hierarchy archive.

Role entry points and track files:

- Designer: `AGENTS_ROLE/GAMEDESIGNER.md`
- Designer structure: `AGENTS_ROLE/GAMEDESIGNER_STRUCTURE.md`
- Designer implementation handoff: `AGENTS_ROLE/GAMEDESIGNER_IMPLEMENTATION.md`
- Designer refactoring: `AGENTS_ROLE/GAMEDESIGNER_REFACT.md`
- Designer gameplay: `AGENTS_ROLE/GAMEDESIGNER_GAMEPLAY.md`
- Designer handoff/evidence: `AGENTS_ROLE/GAMEDESIGNER_HANDOFF.md`
- Code Builder: `AGENTS_ROLE/GAMEBULIDER.md`
- Skill Builder: `AGENTS_ROLE/GAMEBULIDER.md` then `AGENTS_ROLE/GAMEBULIDER_SKILL.md`
- Code Builder structure: `AGENTS_ROLE/GAMEBULIDER_STRUCTURE.md`
- Code Builder implementation: `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md`
- Code Builder Skill Builder: `AGENTS_ROLE/GAMEBULIDER_SKILL.md`
- Code Builder refactoring: `AGENTS_ROLE/GAMEBULIDER_REFACT.md`
- Code Builder quality: `AGENTS_ROLE/GAMEBULIDER_QUALITY.md`
- Code Builder UI: `AGENTS_ROLE/GAMEBULIDER_UI.md`
- Code Builder verification: `AGENTS_ROLE/GAMEBULIDER_VERIFICATION.md`
- Code Reviewer: `AGENTS_ROLE/GAMEREVIWER.md`
- SimpelWorker: `AGENTS_ROLE/SIMPELWORKER.md`

## Routing Rules

### Global Or Ambiguous Work

Read:
- `BLACKBOARD.md`

Use when:
- The request is unclear.
- The user asks about overall status.
- The user asks about board structure, routing, or global policy.

When the task edits root routing or role-policy markdown such as `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/*.md`, or `boards/SkillBluePrint/*.md`, also read:
- `boards/OPS/AUTOMATION_GUIDE.md`

Do not pull MON, RUN, UI, or DATA boards for that policy task unless the policy change is specifically about their routing.

### Monster Work

Read the relevant active monster board:
- Eve: `boards/MON/EVE_MONSTER.md`
- Ariel: `boards/MON/ARIEL_MONSTER.md`
- Rin: `boards/MON/RIN_MONSTER.md`
- Sein: `boards/MON/SEIN_MONSTER.md`
- Vega: `boards/MON/VEGA_MONSTER.md`

Use related boards only when needed:
- Status/runtime effects: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- Data/assets: `boards/DATA/DATA_BLACKBOARD.md`, `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`
- NewRunScene UI/Offering: `boards/UI/RUNSCENE_UI.md`
- General UI/new-scene flow: `boards/UI/UI_BLACKBOARD.md`

Do not read those related boards unless the user request or the inspected failing code/error explicitly crosses into that domain.

Use archives only when older history is actually needed:
- `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`
- `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md`
- Per-board snapshots such as `boards/ARCHIVE/EVE_MONSTER_ARCHIVE_2026-05-18.md`

### Combat Runtime Work

Read the narrow active combat board first:
- Enemy runtime/spawn/skill cadence: `boards/COMBAT/ENEMY_BLACKBOARD.md`
- Status/shield/freeze/vulnerability/slow/runtime labels: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`

Also read:
- Relevant monster board when the work is monster-specific.
- `boards/RUN/RUN_BLACKBOARD.md` when combat ownership touches `NewRunScene` flow.

Do not add RUN boards for isolated combat logic when `NewRunScene` ownership is not part of the active request or inspected failure.

Use archives only when older implementation history is needed:
- `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`
- `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`

### Run / Reward / Save Work

Read:
- `boards/RUN/RUN_BLACKBOARD.md`

Then narrow only if needed:
- Rewards and skill choices: `boards/RUN/REWARD_BLACKBOARD.md`
- Save/load/checkpoint: `boards/RUN/SAVELOAD_BLACKBOARD.md`
- NewRunScene UI behavior: `boards/UI/RUNSCENE_UI.md`
- Menu/new-scene flow: `boards/UI/UI_BLACKBOARD.md`
- Monster-specific run behavior: relevant `boards/MON/{NAME}_MONSTER.md`

Do not add monster, UI, or save/load boards unless the request or inspected path explicitly requires that slice.

### UI Work

Read:
- `boards/UI/UI_BLACKBOARD.md` for surviving menu/new-scene flow.
- `boards/UI/RUNSCENE_UI.md` for active `NewRunScene` UI behavior.

Also read the relevant monster board when the UI work is monster-specific.

Do not read UI boards for non-UI runtime work just because the affected feature eventually appears on screen.

Use archives only when older deleted-scene or older RunScene history is needed:
- `boards/ARCHIVE/UI_BLACKBOARD_ARCHIVE_2026-05-18.md`
- `boards/ARCHIVE/RUNSCENE_UI_ARCHIVE_2026-05-18.md`
- `boards/ARCHIVE/DEBUGSCENE_UI_ARCHIVE_2026-05-14.md`
- `boards/ARCHIVE/MAINMENU_UI_ARCHIVE_2026-05-14.md`

### Data / Asset / CSV Work

Read:
- `boards/DATA/DATA_BLACKBOARD.md` for active runtime CSV authority and archive destinations.
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md` for active prefab/catalog/scene asset wiring.

Also read:
- Relevant monster board when the work is monster-specific.
- `boards/RUN/RUN_BLACKBOARD.md` when the data change affects `NewRunScene` runtime ownership.

Do not add RUN boards for isolated CSV/schema changes unless the change directly affects `NewRunScene` ownership or flow.

Use archives only when older transition history is needed:
- `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`
- `boards/ARCHIVE/DATA_BLACKBOARD_ARCHIVE_2026-05-18.md`
- `boards/ARCHIVE/GAMEDATA_ASSET_BLACKBOARD_ARCHIVE_2026-05-18.md`

### Reviewer / Codex / Unity-MCP / Automation Work

Read:
- `boards/OPS/REVIEWER_BLACKBOARD.md`
- `boards/OPS/CODEX_CLI_BLACKBOARD.md`
- `boards/OPS/UNITY_MCP_BLACKBOARD.md`
- `boards/OPS/AUTOMATION_GUIDE.md`

Reminder:
- Code Reviewer execution requires explicit user permission.
- Unity-MCP Play Mode gameplay verification is user-owned; Codex stops at build/compile/console/editor-state evidence.

### Report / Documentation Work

Read:
- The related active domain board only.

Rules:
- There is no active report board anymore.
- Do not route report or HTML work through a dedicated report board.
- When a report is about a specific system, read the matching active domain board and the referenced source files that the report must summarize.

## Update Rules

When a task changes facts in more than one domain, update every related active board in the same turn.

Examples:
- Eve runtime/status change: update `boards/MON/EVE_MONSTER.md` and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- Enemy CSV/runtime authority change: update `boards/COMBAT/ENEMY_BLACKBOARD.md`, `boards/DATA/DATA_BLACKBOARD.md`, and `boards/RUN/RUN_BLACKBOARD.md`.
- NewRunScene UI/Offering change: update `boards/UI/RUNSCENE_UI.md`, `boards/RUN/RUN_BLACKBOARD.md`, and any affected monster board.
- Menu/new-scene flow change: update `boards/UI/UI_BLACKBOARD.md` and `boards/RUN/RUN_BLACKBOARD.md`.
