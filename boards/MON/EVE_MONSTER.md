## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/EVE_MONSTER.md`.

# EVE_MONSTER

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Scope

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

Legacy non-English note retained these code references: `boards/MON/MON_BLACKBOARD.md`.

## Eve Runtime Summary

- Eve active skills A-E runtime work exists in the migrated task blocks below.
- Eve passive skills F-J runtime work exists in the migrated task blocks below.
- Arc Bolt has projectile, branch damage, magazine, reload, and enhancement/master behavior history.
- Eve status runtime includes shock, chill/freeze interactions, vulnerability, shield, action-speed, and passive damage modifiers.
- DebugScene testing for Eve skill toggles is tied to `boards/UI/DEBUGSCENE_UI.md`.

## Cross-Board Update Requirements

- Projectile changes: update this file and `boards/COMBAT/PROJECTILE_BLACKBOARD.md`.
- Status/shield/freeze/vulnerability changes: update this file and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- DebugScene Eve skill toggle changes: update this file, `boards/MON/MON_BLACKBOARD.md`, and `boards/UI/DEBUGSCENE_UI.md`.
- Eve data asset changes: update this file and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- Reports about Eve implementation: update this file and `boards/REPORT/REPORT_BLACKBOARD.md`.

## Task: 2026-05-13 Eve Battlefield Facade Registration

### Task title

Route Eve battlefield projectile, effect, and drone registration through the Phase 1 facade.

### Goals

- Preserve Eve skill behavior while replacing direct battlefield list registration writes.
- Keep Eve projectile/effect/drone creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve skills in Play Mode if needed.

### Evidence

- `CombatRuntimeEveSkills.cs:816`, `:877`, and `:1342` now call `AddBattlefieldProjectile(...)`.
- `CombatRuntimeEveSkills.cs:1171` now calls `AddBattlefieldDrone(...)`.
- Eve skill-effect registrations now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Eve battlefield object registration through facade methods.

## Migrated Task Blocks

## Task: 2026-05-10 Eve Voltage Calibration Shield Review

### Task title

Fix Eve F shield timing and ally application.

### Goals

- Review monster reference files under `Pakuri/reference/2.Monster` for shield-bearing skills.
- Make Eve F apply its battle-start shield to lightning-skill allies, not only the selected 1P unit.
- Prevent selected Eve's shield timer from being decremented by both Eve-specific and shared shield timer paths.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve F in RunScene Play Mode with selected Eve and manifested lightning-skill allies.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Reference search found concrete shield skills in `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `Pakuri/reference/2.Monster/eve/eve-tower.md`, and `Pakuri/reference/2.Monster/eve/skill/f-voltage-calibration.md`.
- `Pakuri/reference/2.Monster/eve/skill/f-voltage-calibration.md:18` defines the shield as Eve power 120% for 12 seconds on lightning-skill allies.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:93` through `:108` no longer decrements `unitShieldTimer`; selected shield duration is handled by the shared shield timer path.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1558` through `:1594` checks selected and manifested units for lightning skills.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1682` through `:1732` applies Eve F shield to the selected lightning unit and manifested lightning-skill allies, stamps `ShieldAppliedFrame`, and updates manifested labels.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh reached idle; console error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-10: User asked to review shield logic among monsters under `Pakuri/reference/2.Monster`, specifically noting Eve shield seemed not to apply correctly.
