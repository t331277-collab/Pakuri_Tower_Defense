## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/MON_BLACKBOARD.md`.

## Task: 2026-05-13 Manifested Party Runtime Boundary

### Task title

Track common monster impact of the Phase 2 manifested party runtime boundary.

### Goals

- Preserve Manifested monster `CombatUnitRuntime` skill/state behavior while moving party collection ownership behind a runtime service.
- Keep monster-specific unit dispatch for Eve, Rin, Sein, Vega, and Ariel on the existing controller paths for this slice.
- Keep Offering-learned active skill synchronization behavior unchanged.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Future Phase 2 slices may move monster unit dispatch behind the service after a separate verification pass.
- User verifies Manifested monsters still cast learned skills and maintain HP/shield state in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:8` through `:12` now stores manifested party list access behind the `manifestedParty` service.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:58` through `:60` calls separate skill sync, combat tick, and view refresh helpers for each manifested unit.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:566` through `:583` keeps the existing `SyncManifestedLearnedSkills(...)`, `CombatUnitRuntime.TickManifestedCombat(...)`, and label refresh calls intact behind separate helper methods.
- `CombatUnitRuntime.cs:145` through `:193` still owns per-unit timer ticking and still calls `Owner.TickManifestedUnitSkill(...)`; this slice did not rewrite monster-specific dispatch.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder started Phase 2 with a service boundary before moving monster-specific unit dispatch or shared target/effect logic.

## Task: 2026-05-13 Monster Skill Battlefield Facade Registration

### Task title

Route monster skill battlefield object registration through the Phase 1 facade.

### Goals

- Keep monster skill behavior unchanged while replacing direct projectile/effect/drone list registration writes.
- Prepare later monster runtime adapter narrowing by giving skill files a single battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Detailed monster-specific notes are recorded in each monster board.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build and Unity-MCP console checks.

### Next Actions

- User verifies selected and manifested monster skill behavior in Play Mode if needed.
- Future Phase 6 should narrow monster skill adapters after battlefield, party, enemy, and selected-unit boundaries stabilize.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:22` through `:39` adds facade methods for enemy, projectile, skill-effect, and drone registration.
- `Select-String` after implementation found 52 `AddBattlefield*` call sites across manager and monster skill files.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Code Builder implemented Phase 1 battlefield facade registration for Eve, Ariel, Rin, Sein, Vega, party, enemy, and selected projectile paths.

## Task: 2026-05-10 Ariel Manifested Shield Expiry And Archangel Effect Fix

### Task title

Track common monster impact of Ariel party shield expiry and E visual correction.

### Goals

- Ensure selected 1P monster shield state granted by a 2P-5P Ariel is no longer tied to the selected monster being Ariel.
- Keep Manifested Ariel E visual behavior aligned with the selected Ariel E battlefield effect path.

### Constraints

- Role Owner is Code Builder.
- Detailed Ariel behavior is recorded in `boards/MON/ARIEL_MONSTER.md`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies selected 1P shield expiry after Manifested Ariel B/E and Ariel E visual output in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:518` ticks selected-unit shield duration from common selected combat update.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:86` clears selected shield and mirrored selected unit shield fields on expiry.
- `CombatRuntimeArielSkills.cs:438`, `:693`, and `:700` route selected and Manifested Ariel E through a battlefield-wide Archangel visual helper.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:35` and `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:28` store shield-applied frame state so selected and manifested shield timers start decaying on the same next frame.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: Fixed a selected-unit shield timer ownership bug found after Manifested Ariel team shield migration.
- 2026-05-10: Follow-up aligned shield timer first-tick timing after user reported 1P shield duration appeared shorter than 2P-5P.

## Task: 2026-05-10 Ariel Unit Executor Migration And Team Shield

### Task title

Track common monster impact of Ariel unit executor migration and team shield state.

### Goals

- Continue monster unit-runtime parity by adding Ariel-specific unit execution after Vega.
- Store shield and Ariel timed state on `CombatUnitRuntime` so 2P-5P party units can receive and absorb Ariel shields.

### Constraints

- Role Owner is Code Builder.
- Detailed Ariel behavior is recorded in `boards/MON/ARIEL_MONSTER.md`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP checks.

### Next Actions

- User verifies Manifested Ariel skill parity and selected Ariel party shield behavior in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:42` now stores per-unit shield and Ariel timed state.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:637` calls `TryTickArielUnitSkill(...)` before generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:2024` and `:2043` now display/pass manifested shield state instead of hardcoded `0f`.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:808` applies team shield state to selected and manifested party units.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: User requested Ariel unit executor migration and teammate shield verification after the Vega migration.

## Task: 2026-05-10 Vega Unit Executor Migration

### Task title

Track common monster impact of the Vega unit executor migration.

### Goals

- Continue the monster OOP/unit-runtime parity work after Eve, Rin, and Sein by adding Vega-specific unit execution.
- Keep Manifested Vega in `CombatUnitRuntime` / `CombatSkillRuntime` for A-E rather than relying on the generic manifested fallback.

### Constraints

- Role Owner is Code Builder.
- Detailed Vega behavior is recorded in `boards/MON/VEGA_MONSTER.md`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP compile/console checks.

### Next Actions

- User verifies Manifested Vega skill parity in RunScene Play Mode.
- Continue Ariel unit executor migration only after Vega behavior is accepted.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:630` now calls `TryTickVegaUnitSkill(...)` before the generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:139` implements the Vega unit tick dispatcher.
- `CombatRuntimeVegaSkills.cs:445`, `:507`, `:548`, and `:616` implement unit-owned B/C/D/E paths.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:36` stores Vega unit state for Extermination Permit and Black Ledger cooldown charge.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-10: User requested the Vega unit executor migration from the remaining-work report.

## Task: 2026-05-10 Monster Shield Skill Review

### Task title

Review and correct monster shield skill runtime coverage.

### Goals

- Identify shield-bearing monster skills from `Pakuri/reference/2.Monster`.
- Confirm Ariel and Eve shield runtime paths are aligned with the inspected references.
- Fix Eve F shield application and timing where code did not match the reference.

### Constraints

- Role Owner is Code Builder.
- Detailed Eve evidence is recorded in `boards/MON/EVE_MONSTER.md`.
- Detailed status evidence is recorded in `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel B/E and Eve F shield behavior in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Shield reference search found concrete implemented shield skills for Ariel B/E and Eve F; generic pattern files mention shield concepts but are not concrete monster skill implementations.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs` contains the shared selected shield timer, Ariel team shield application, and Archangel effect creation paths inspected in this pass.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:93` through `:108` removes Eve's duplicate selected shield timer decrement.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1682` through `:1732` applies Eve F shields to lightning-skill selected and manifested allies.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console error read returned only MCP client handler logs.

### History

- 2026-05-10: User asked to review all shield logic among monsters in `Pakuri/reference/2.Monster` and fix Eve if needed.
