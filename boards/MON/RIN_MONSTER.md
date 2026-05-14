## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-08 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/RIN_MONSTER.md`.

# RIN_MONSTER

## Scope

Rin dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Rin file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Not populated yet.

## Task: 2026-05-13 Rin Battlefield Facade Registration

### Task title

Route Rin battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Rin skill behavior while replacing direct battlefield list registration writes.
- Keep Rin projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin skills in Play Mode if needed.

### Evidence

- `CombatRuntimeRinSkills.cs:575` now calls `AddBattlefieldProjectile(...)`.
- Rin skill-effect registrations now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Rin battlefield object registration through facade methods.

## Task: 2026-05-08 Rin CombatUnitRuntime Parity Resume

### Task title

Route selected Rin and manifested Rin through shared unit skill runtime paths.

### Goals

- Make selected 1P Rin and manifested 2P-5P Rin call `CombatUnitRuntime` plus `CombatSkillRuntime` based execution for Rin B/C/D/E.
- Preserve Rin A magazine/projectile handling on the existing path.
- Keep manifested Rin Howling buff duration and Howling dark follow-up on the unit runtime, not on selected-only fields.
- Reuse existing RunScene slot status children for manifested monster name, HP text, and HP/shield bars.

### Constraints

- Role Owner is Code Builder.
- Claims are based on inspected files, Unity-MCP scene hierarchy output, and command output.
- Unity Play Mode verification is user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build, Unity refresh, and console checks.

### Next Actions

- User verifies selected Rin and manifested Rin B/C/D/E behavior in RunScene Play Mode.
- User verifies 2P-5P monster status UI does not duplicate labels or bars when manifested monsters appear.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:76` defines `TickSelectedRinUnitSkillRuntimes(...)` for selected Rin skill runtime ticking.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:128` routes Rin automatic skill execution through `TryTriggerRinUnitAutomaticSkills(CombatUnitRuntime runtime)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:240`, `:321`, and `:401` implement unit-runtime casts for Rin B, Rin D, and Rin E; Rin C is routed through the same unit skill tick and manifested shockwave path.
- `Pakuri/Assets/Scripts/Combat/CombatUnitRuntime.cs:15` through `:18` stores separate name label, HP label, HP bar fill, and shield bar fill references.
- `Pakuri/Assets/Scripts/Combat/CombatUnitRuntime.cs:25`, `:59`, `:104`, and `:128` store, tick, and reset manifested Rin Howling state on the unit runtime.
- Unity-MCP scene hierarchy inspection found `CombatRoot/2PMonster`, `3PMonster`, `4PMonster`, `5PMonster`, and `EveUnit`; 2P/3P/Eve children included `MonsterHpLabel`, `MonsterHpBar/Fill`, `MonsterHpBar/Shield`, and `MonsterNameLabel`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP script refresh reached idle; console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-08: User resumed an interrupted request to start from Rin and make selected 1P and manifested 2P-5P monsters use the same `CombatUnitRuntime` plus `CombatSkillRuntime` execution basis.

## Task: 2026-05-08 Manifested Rin C Shockwave Parity Fix

### Task title

Make manifested Rin C apply selected Rin C beam and knockback behavior.

### Goals

- Fix manifested Rin C so it does more than visual line damage.
- Apply selected Rin C's map-wide beam hit shape, knockback, width choices, master slow, master lightning follow-up, and reload reduction behavior where applicable.
- Keep damage multiplier sourced through existing manifested Rin C choice multiplier logic.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Rin C knockback in RunScene Play Mode.
- User verifies Rin C master/trait choices if those choices are learned on the manifested Rin.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:220` through `:310` shows selected Rin C uses map-wide range, `IsPointInsideBeam(...)`, `ApplyRinKnockback(...)`, master lightning follow-up, master slow, and trait reload reduction.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:499` routes manifested `rin-c` into `TryFireManifestedRinShockwave(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:545` implements the manifested Rin C beam path using selected-runtime helper methods and manifested Offering checks.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:627` reduces manifested Rin A reload when manifested Rin C trait 5 hits while Rin A is reloading.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported selected Rin C knockback works, but manifested Rin C only showed effect/beam without moving enemies.

## Task: 2026-05-08 Manifested Rin Common Runtime Parity

### Task title

Apply Rin Offering choices through manifested projectile and common skill runtime.

### Goals

- Keep manifested Rin skills sourced from `SkillDefinition` data.
- Apply Rin manifested Offering choices in shared damage, cooldown, magazine, reload, and shot interval paths.
- Preserve manifested projectile/status handling through the common combat service.

### Constraints

- Role Owner is Code Builder.
- This is common manifested runtime work, not a full line-by-line copy of selected Rin private skill code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Rin skills and Offering upgrades in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:866` includes Rin skill-specific damage multipliers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:991` includes Rin cooldown choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1250`, `:1278`, and `:1310` include Rin A magazine/reload/shot-interval choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:693` applies manifested projectile status from skill data.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: Manifested Rin common runtime parity was implemented and retained as the latest active Rin task block during MON board compaction.

## Required Sections For Future Work

- Source references
- Skill slots A-J
- Runtime implementation status
- Data asset status
- DebugScene test status
- Evidence
- History

## Task: 2026-05-08 Manifested Rin Passive And Targeting Continuation

### Task title

Make manifested Rin use Rin passive skill runtime effects and participate as an enemy target.

### Goals

- Apply Rin F-J passive effects to manifested Rin A/C/D/E runtime paths through `CombatUnitRuntime`.
- Keep manifested Rin cooldown ticking affected by Rin action-speed passives.
- Fix missing manifested HP slide bar fallback.
- Allow enemies to target and damage manifested Rin and other manifested monsters.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification is user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build, diff check, Unity refresh, and console read.

### Next Actions

- User verifies in RunScene Play Mode that manifested Rin gets passive effects from Offering, has one HP bar, and can be attacked by enemies.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:197` ticks manifested Rin unit skill cooldowns with `GetRinUnitActionSpeedMultiplier(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:1073` adds `TryApplyRinUnitProjectileHit(...)` for manifested Rin projectile damage with unit passive modifiers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:1269` tracks manifested Rin physical hit count for Rin H.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:1848` implements manifested Rin action-speed passive calculation.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:793` routes manifested Rin C damage through `ApplyRinUnitSkillDamage(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `git diff --check` over touched combat files completed with exit code 0 and CRLF warnings only.
- Unity-MCP script refresh requested compilation; console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-08: User requested resuming work so manifested Rin gains passive skills like selected Rin, manifested monsters have HP slide bars, and enemies attack manifested monsters too.
