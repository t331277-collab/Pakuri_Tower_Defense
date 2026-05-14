# MDTREE.md

## Purpose

`MDTREE.md` is the routing index for persistent work state.

Start every session by reading `AGENTS.md` and this file. Do not read the full `BLACKBOARD.md` by default. Use the routing rules below to open only the board files relevant to the user request.

## Root Files

- `AGENTS.md`: global startup, evidence, routing, and role entry-point rules.
- `AGENTS_ROLE/GAMEDESIGNER.md`: Designer role entry point and track routing.
- `AGENTS_ROLE/GAMEDESIGNER_STRUCTURE.md`: Designer structure-design rules.
- `AGENTS_ROLE/GAMEDESIGNER_IMPLEMENTATION.md`: Designer implementation-handoff rules.
- `AGENTS_ROLE/GAMEDESIGNER_REFACT.md`: Designer refactoring-design rules.
- `AGENTS_ROLE/GAMEDESIGNER_GAMEPLAY.md`: Designer gameplay-facing design rules.
- `AGENTS_ROLE/GAMEDESIGNER_HANDOFF.md`: Designer Unity evidence and Code Builder handoff rules.
- `AGENTS_ROLE/GAMEBULIDER.md`: Code Builder role entry point and track routing.
- `AGENTS_ROLE/GAMEBULIDER_STRUCTURE.md`: Code Builder structure-support rules.
- `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md`: Code Builder direct implementation rules.
- `AGENTS_ROLE/GAMEBULIDER_REFACT.md`: Code Builder refactoring rules.
- `AGENTS_ROLE/GAMEBULIDER_QUALITY.md`: Code Builder code-quality rules.
- `AGENTS_ROLE/GAMEBULIDER_UI.md`: Code Builder Unity UI implementation rules.
- `AGENTS_ROLE/GAMEBULIDER_VERIFICATION.md`: Code Builder verification, Reviewer transition, and board-update rules.
- `AGENTS_ROLE/GAMEREVIWER.md`: Code Reviewer role rules.
- `MDTREE.md`: this routing tree.
- `BLACKBOARD.md`: root index and current global status only.
- `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`: full pre-hierarchy detailed task archive.

## Routing Rules

### Global Or Ambiguous Work

Read:
- `BLACKBOARD.md`

Use when:
- The request is unclear.
- The user asks about current overall status.
- The user asks about board structure, routing, or global policy.

### Monster / Player Monster / Skill Work

Read the relevant active monster file when it exists:
- Eve: `boards/MON/EVE_MONSTER.md`
- Ariel: `boards/MON/ARIEL_MONSTER.md`
- Rin: `boards/MON/RIN_MONSTER.md`
- Sein: `boards/MON/SEIN_MONSTER.md`
- Vega: `boards/MON/VEGA_MONSTER.md`

Common monster history from before the 2026-05-14 board cleanup is archived at:
- `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`

If creating a new monster such as Vega, Ariel, Sein, or Rin:
- Read that monster's file if it exists.
- If it does not exist, create `boards/MON/{NAME}_MONSTER.md`.
- Use `boards/MON/EVE_MONSTER.md` only as an implementation reference when a concrete example is needed.

Related boards:
- Status effects: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- Projectiles: older projectile history is archived at `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`; create a new active projectile board only if new projectile work resumes.
- DebugScene monster testing: use `boards/UI/UI_BLACKBOARD.md`; older DebugScene UI history is archived at `boards/ARCHIVE/DEBUGSCENE_UI_ARCHIVE_2026-05-14.md`.
- Game data assets: `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`

### Combat Runtime Work

Common combat history from before the 2026-05-14 board cleanup is archived at:
- `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`

For new active combat work, read only the relevant active narrow board below, or create a new active board if the topic no longer has one.

Then narrow by topic:
- Enemy spawn, target priority, enemy stats: `boards/COMBAT/ENEMY_BLACKBOARD.md`
- Projectile behavior: older history is archived at `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`
- Shock, chill, freeze, shield, vulnerability, slow: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- Monster-specific combat: relevant `boards/MON/{NAME}_MONSTER.md`

### Refactoring / Architecture Work

The former refactoring board folder was archived on 2026-05-14:
- `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`
- `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/COMBAT_STATE_OWNERSHIP_MAP.md`

For new architecture work, route by the affected domain board and create a new active board only when the work needs persistent state beyond the existing domain files.

Then also read the affected domain board:
- Combat state ownership map: archived at `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/COMBAT_STATE_OWNERSHIP_MAP.md`
- Combat runtime refactor: archived common history at `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`
- Projectile/effect/drone refactor: archived projectile history at `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`
- Enemy simulation refactor: `boards/COMBAT/ENEMY_BLACKBOARD.md`
- Status/effect refactor: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- Monster skill executor refactor: relevant `boards/MON/{NAME}_MONSTER.md`; archived common history at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`
- UI-impacting refactor: related `boards/UI/*.md`

### Run / Reward / Save Work

Read:
- `boards/RUN/RUN_BLACKBOARD.md`

Then narrow by topic:
- Rewards and skill choices: `boards/RUN/REWARD_BLACKBOARD.md`
- Save/load or checkpoint design: `boards/RUN/SAVELOAD_BLACKBOARD.md`
- Run UI: `boards/UI/RUNSCENE_UI.md`
- Monster selection: relevant `boards/MON/{NAME}_MONSTER.md` and `boards/UI/UI_BLACKBOARD.md`; older MainMenu UI history is archived at `boards/ARCHIVE/MAINMENU_UI_ARCHIVE_2026-05-14.md`.

### UI Work

Read:
- `boards/UI/UI_BLACKBOARD.md`

Then narrow by topic:
- DebugScene UI: `boards/UI/UI_BLACKBOARD.md`; older DebugScene UI history is archived at `boards/ARCHIVE/DEBUGSCENE_UI_ARCHIVE_2026-05-14.md`.
- Main menu UI: `boards/UI/UI_BLACKBOARD.md`; older MainMenu UI history is archived at `boards/ARCHIVE/MAINMENU_UI_ARCHIVE_2026-05-14.md`.
- RunScene UI: `boards/UI/RUNSCENE_UI.md`

If UI is tied to monster testing, also read:
- Relevant `boards/MON/{NAME}_MONSTER.md`

### Data / Asset / CSV Work

Read:
- `boards/DATA/DATA_BLACKBOARD.md`

Then narrow by topic:
- CSV source data: older CSV board history is archived at `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`; use `boards/DATA/DATA_BLACKBOARD.md` for active CSVData transition state.
- Unity ScriptableObject/static assets: `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`
- Monster data: relevant `boards/MON/{NAME}_MONSTER.md`; archived common history at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`

### Reviewer / Codex / Unity-MCP / Automation Work

Read:
- Reviewer workflow: `boards/OPS/REVIEWER_BLACKBOARD.md`
- Codex CLI setup or commands: `boards/OPS/CODEX_CLI_BLACKBOARD.md`
- Unity MCP connection or tool usage: `boards/OPS/UNITY_MCP_BLACKBOARD.md`
- Automation responsibility: `boards/OPS/AUTOMATION_GUIDE.md`

Reminder:
- Code Reviewer execution requires explicit user permission.
- Unity-MCP Play Mode gameplay verification is user-owned; Codex stops at build/compile/console/editor-state evidence.

### Report / HTML Documentation Work

Read:
- `boards/REPORT/REPORT_BLACKBOARD.md`

If the report is about a specific domain, also read that domain board.

## Update Rules

When a task changes facts in more than one domain, update every related board in the same turn.

Examples:
- Eve projectile change: update `boards/MON/EVE_MONSTER.md`; consult `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md` and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md` only when older projectile/common monster history is needed.
- DebugScene Eve skill toggle change: update `boards/UI/UI_BLACKBOARD.md` and `boards/MON/EVE_MONSTER.md`; consult `boards/ARCHIVE/DEBUGSCENE_UI_ARCHIVE_2026-05-14.md` only when older UI history is needed.
- Run reward UI change: update `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, and `boards/RUN/RUN_BLACKBOARD.md`.
- New character creation: create/update `boards/MON/{NAME}_MONSTER.md` and update data/combat/UI boards if implementation touches those domains.
