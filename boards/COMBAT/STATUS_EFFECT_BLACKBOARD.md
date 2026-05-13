## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-10` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.

## Task: 2026-05-13 Phase 3 Skill Effect Slice Plan

### Task title

Define skill-effect Phase 3 slices before implementation.

### Goals

- Move skill-effect lifecycle ownership behind a simulation boundary after projectile slices.
- Preserve current effect tick interval, expiry, residual spawn, shape hit checks, and damage routing.
- Keep reusable temporary-effect migration reserved for Phase 7.

### Constraints

- Role Owner is Designer.
- Do not change runtime C# behavior.
- Do not introduce `TemporaryEffectInstance` in Phase 3.
- Do not migrate shield/status modifiers into a common layer during this phase.

### Role Owner

Designer

### Status

Completed. Skill-effect work should occupy Phase 3-D and Phase 3-E.

### Next Actions

- Phase 3-D: move the shared skill-effect lifetime/tick loop behind a simulation boundary.
- Phase 3-E: separate shape checks, effect damage dispatch, and expiry-spawn routing while preserving Eve, Sein, Vega, and manifested behavior.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1230` owns `UpdateEveSkillEffects()` and `UpdatePersistentSkillEffects()` over the shared `skillEffects` list.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1233` through `:1283` owns `TickSkillEffect(...)`, which dispatches to Sein, Vega, manifested, and Eve effect damage/status behavior.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1428` owns beam shape checks for effects.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeSeinSkills.cs:1026` through `:1114` owns Sein effect detection, damage, and residual effect spawning on expiry.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:772` through `:813` owns Vega effect damage/name-mark behavior, and `:1523` through `:1533` owns Vega effect classification helpers.
- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:32` through `:35` already routes skill-effect registration through `AddBattlefieldSkillEffect(...)`.

### History

- 2026-05-13: Designer split Phase 3 skill-effect lifecycle work into 3-D and 3-E before Code Builder implementation.

## Task: 2026-05-13 Temporary Effect Reuse Roadmap Timing

### Task title

Record the timing for moving status, shield, and modifier effects into a common temporary-effect layer.

### Goals

- Clarify that temporary-effect reuse is a Phase 7 migration, not a Phase 3 implementation task.
- Start common temporary effects with a simple modifier such as action speed.
- Move shield next, then move/damage/status-chance modifiers, and leave complex statuses for later.

### Constraints

- Role Owner is Designer.
- Do not change runtime C# behavior.
- Preserve existing shield and status behavior until Code Builder receives a specific implementation task.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Phase 7-C: verify one common modifier effect.
- Phase 7-D: split shield grant / absorb APIs.
- Phase 7-E/F: migrate broader modifiers and complex statuses only after the simple effect path is proven.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` to order temporary-effect migration as action speed, shield, general modifiers, then complex status effects.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:330` through `:346` proposes `ApplyTemporaryEffect(...)`, `GrantShield(...)`, and shield subsystem separation.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:145` through `:186` currently ticks manifested shield and timed buffs locally.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:738` through `:748` currently ticks enemy move buff, freeze, slow, shock, and chill state directly.

### History

- 2026-05-13: User asked to amend the roadmap with temporary-effect reuse timing, including reusable shield/effect direction from the 2026-05-10 proposal.

## Task: 2026-05-13 Battlefield Facade Skill Effect Registration

### Task title

Route skill-effect registration through the Phase 1 battlefield facade.

### Goals

- Replace direct battlefield `skillEffects.Add(...)` registration writes with facade calls.
- Preserve existing effect timers, status application, visual duration, and tick behavior.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build and Unity-MCP console checks.

### Next Actions

- Future Phase 3 can move effect ticking/lifetime ownership behind the facade.
- Future Phase 7 can migrate transferable status/shield effects into common temporary-effect APIs.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:32` through `:35` adds `AddBattlefieldSkillEffect(...)`.
- `Select-String` after implementation found skill-effect registration calls routed through `AddBattlefieldSkillEffect(...)` in party and monster skill files.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.
- Unity-MCP console warning/error read after script import/refresh returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-13: Code Builder implemented Phase 1 battlefield facade boundary and routed skill-effect registration writes through it.

## Task: 2026-05-10 Ariel Selected Shield Expiry Status Fix

### Task title

Make selected-unit Ariel shield status expire outside selected-Ariel-only runtime.

### Goals

- Ensure shields on selected 1P from Manifested Ariel B/E are cleared when their timer reaches zero.
- Keep Archangel shield state and selected unit mirror fields synchronized with the cleared shield.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode status verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies selected 1P shield text/bar disappears after Manifested Ariel shield duration ends.

### Evidence

- Before the fix, `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:83` through `:88` tied `unitShieldTimer` decay to `UpdateArielSkillCooldowns()`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:518` now ticks selected-unit shield state from common selected combat.
- `CombatRuntimeArielSkills.cs:86` clears `unitShieldValue`, `arielArchangelShieldValue`, `arielArchangelShieldTimer`, and mirrored `selectedUnitRuntime` shield/Ariel fields on expiry.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:35` adds `ShieldAppliedFrame`; `:160` through `:163` skip shield timer decay on the frame the shield was applied.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:28` adds `unitShieldAppliedFrame`; `:95` through `:98` apply the same first-frame skip to selected 1P shields.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: User reported selected 1P kept Ariel shield after the Manifested Ariel shield duration should have ended.
- 2026-05-10: User then reported selected 1P shield duration appeared shorter than 2P-5P; Builder aligned first-frame timer decay semantics for selected and manifested shield statuses.

## Task: 2026-05-10 Ariel Unit Shield And Holy Exposure Runtime

### Task title

Carry Ariel shield, sanctuary, Archangel, and Holy Exposure behavior through unit source logic.

### Goals

- Store Ariel shield and timed buff state per `CombatUnitRuntime`.
- Make Ariel B/E shields protect selected plus manifested party units.
- Keep Ariel Holy Exposure and shield-dependent passive bonuses source-aware for manifested Ariel.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode status verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies selected Ariel shield skills protect teammates and Manifested Ariel applies Holy Exposure/status interactions in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:42` stores per-unit shield, shield source, blessing, sanctuary, Archangel shield, burst, and reflect state.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:808` applies Ariel team shields to selected and manifested units.
- `CombatRuntimeArielSkills.cs:869` handles manifested shield absorption, Archangel shield share reduction, reflect, and burst.
- `CombatRuntimeArielSkills.cs:1300` applies Ariel A master Holy Exposure from manifested projectile hits.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:464` through `:473` lets manifested units absorb shield before HP damage.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: Ariel unit executor migration corrected selected-only shield storage so Ariel shields can protect 2P-5P teammates.

## Task: 2026-05-10 Vega Unit Status And Passive Runtime

### Task title

Carry Manifested Vega mark, silence, vulnerability, and passive state through unit source logic.

### Goals

- Make Manifested Vega B apply silence/name marks from the source unit choices/passives.
- Make Manifested Vega C maintain unit-owned Extermination Permit buff state.
- Make Manifested Vega D/E apply I/J vulnerability and cooldown-charge behavior from the source unit.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode status verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP checks.

### Next Actions

- User verifies Manifested Vega name marks, silence, area vulnerability, survivor vulnerability, and cooldown-charge behavior in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:36` stores Manifested Vega C buff and D cooldown-charge state on the unit.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:445` implements unit B rectangle slash with silence/name-mark state.
- `CombatRuntimeVegaSkills.cs:507` implements unit C Extermination Permit timer and action/attack buff state.
- `CombatRuntimeVegaSkills.cs:548` and `:616` implement unit D/E status interactions.
- `CombatRuntimeVegaSkills.cs:1634` and `:1651` implement unit I/J vulnerability application.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-10: Vega unit executor migration moved the remaining Manifested Vega status/passive behavior from generic manifested approximations into Vega unit-owned logic.

## Task: 2026-05-10 Shield Status Timer And Eve F Application

### Task title

Centralize selected shield timer ownership and apply Eve F to lightning allies.

### Goals

- Keep selected-unit shield duration decremented by the shared shield timer path only.
- Preserve first-frame shield duration by using `ShieldAppliedFrame`.
- Apply Eve F battle-start shield to manifested allies that have lightning skills.

### Constraints

- Role Owner is Code Builder.
- This is status/runtime validation only; Play Mode verification remains user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies that selected Eve's Eve F shield lasts the intended 12 seconds and that manifested lightning allies receive it.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:518` calls `UpdateSelectedUnitShieldTimer(Time.deltaTime)` once per combat update for selected-unit shield timers.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:93` through `:108` no longer decrements `unitShieldTimer` inside Eve cooldown ticking.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1682` through `:1732` stamps `unitShieldAppliedFrame` / `ShieldAppliedFrame` and applies Eve F to manifested lightning-skill allies.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:160` through `:168` skips timer decrement on the frame a shield was applied.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console error read returned only MCP client handler logs.

### History

- 2026-05-10: User reported selected Eve shield duration seemed too short and asked for a broader shield skill review.
