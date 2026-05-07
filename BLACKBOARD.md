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

## Recent Task: 2026-05-08 RunScene Prisoner Manifest Party

### Task title

RunScene prisoner choice, Manifest result storage, and limited Manifested party combat.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster/Data-spanning Manifest implementation.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/COMBAT/COMBAT_BLACKBOARD.md`, `boards/MON/MON_BLACKBOARD.md`, and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for prisoner choice panels, Manifest result, next-day 2P+ party display, and limited A/basic auto combat.

### Evidence

- Changed `Pakuri/Assets/CSVdata/source/monster_skills.csv`, `Pakuri/Assets/Scripts/Run/RunSession.cs`, `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`, `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs`, `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs`, and added `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: Code Builder implemented the requested ordered RunScene Manifest flow and recorded domain-specific board evidence.

## Recent Task: 2026-05-08 Prisoner Offering Panel And Manifest Follow-up

### Task title

Fix RunScene prisoner Offering panel routing and Manifested party baseline state.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster-spanning follow-up.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/COMBAT/COMBAT_BLACKBOARD.md`, and `boards/MON/MON_BLACKBOARD.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for Offering opening `PrisonerOfferingPanel`, Manifest return marking the prisoner reward as used, and Manifested monsters using own HP/stat/A-skill nearest-enemy auto attack.

### Evidence

- Changed `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` and `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- Unity-MCP scene inspection confirmed `RunCombatCanvas/PrisonerOfferingPanel` and separate `RunCombatCanvas/PrisonerPanel` both exist.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported Offering opened the wrong panel, Manifested monster behavior needed correction, and Manifest return did not show the prisoner reward button as used.

## Recent Task: 2026-05-08 Manifested Party Member Growth State

### Task title

Make Manifested monsters behave as growable party-member monster states.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster-spanning Manifested monster growth fix.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/COMBAT/COMBAT_BLACKBOARD.md`, and `boards/MON/MON_BLACKBOARD.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification that Manifested monsters start from their own registered monster state, auto-cast registered learned skills at nearest enemies, and gain skills/modifiers through Offering.

### Evidence

- Changed `Pakuri/Assets/Scripts/Run/RunSession.cs`, `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`, and `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `RunSession.cs` now has `RunMonsterState`, `PartyMembers`, monster-ID scoped learned skill/reward methods, and `RecordManifestedMonster(MonsterDefinition monster)`.
- `RunCombatUiController.cs` now builds Offering choices for selected plus Manifested target monsters and commits choices by `choice.MonsterId`.
- `CombatRuntimeParty.cs` now syncs Manifested learned active IDs to registered `SkillDefinition` entries and auto-casts them at the nearest living enemy.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP script refresh returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.
- `git diff --check` on the changed Run/Combat scripts completed with no whitespace errors, aside from Git LF-to-CRLF normalization warnings.

### History

- 2026-05-08: User clarified Manifested monsters should be equivalent to MainMenu-starting monsters added during gameplay, not unregistered weird-skill users, and should grow through Offering.
- 2026-05-08: Code Builder added per-monster run party state, made Offering target that state, and made Manifested combat use registered learned skills.

## Recent Task: 2026-05-08 Manifested Scene Slots And Summoner Back Button

### Task title

Use authored NPMonster slots and add summoner return.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster-spanning correction to use `CombatRoot/2PMonster` through `5PMonster`.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/COMBAT/COMBAT_BLACKBOARD.md`, and `boards/MON/MON_BLACKBOARD.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for Manifested monster slot activation order, MonsterPanel display, A/default skill behavior, and summoner Back to Reward behavior.

### Evidence

- Unity-MCP found `CombatRoot/EveUnit`, `CombatRoot/2PMonster`, `CombatRoot/3PMonster`, `CombatRoot/4PMonster`, and `CombatRoot/5PMonster`.
- Changed `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` to bind Manifested monsters to those scene slots.
- Changed `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` to add `PrisonerSummonerPanel/BackButton`.
- Saved `Pakuri/Assets/Scenes/RunScene.unity`; `RunScene.unity:5233` has `m_Name: BackButton`, and `:8429` has `m_Text: Back to Reward`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.
- Unity-MCP script refresh returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User clarified that the scene already has `2PMonster` through `5PMonster` under `CombatRoot`, and requested using those slots plus adding a summoner return button.
- 2026-05-08: Code Builder changed Manifested runtime slot binding and added the `Back to Reward` path.

## Recent Task: 2026-05-08 Manifested Summon Sync And Vega A

### Task title

Fix first Manifested summon synchronization and Manifested Vega A three-projectile behavior.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster-spanning Manifest follow-up.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/COMBAT/COMBAT_BLACKBOARD.md`, `boards/COMBAT/PROJECTILE_BLACKBOARD.md`, `boards/MON/MON_BLACKBOARD.md`, and `boards/MON/VEGA_MONSTER.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for first Summon/Continue application, Manifested Vega A three-projectile behavior, and Offering-acquired Manifested skill firing.

### Evidence

- Changed `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` and `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `RunCombatUiController.cs:702` and `:1246` refresh Manifested party state after Manifest success and Offering commit.
- `CombatRuntimeParty.cs:149` exposes `RefreshManifestedMonsterParty(RunSession session)`.
- `CombatRuntimeParty.cs:747` through `:774` queues Manifested Vega A as three projectiles, with 0.12 second spacing and 2x third-hit damage.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported first Manifested application delay, Manifested Vega A missing its three-projectile baseline, and requested checking Offering-acquired Manifested skills.
- 2026-05-08: Code Builder added immediate Manifested party refresh and Manifested Vega A-specific projectile burst behavior.
