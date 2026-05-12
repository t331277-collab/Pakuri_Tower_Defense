## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/MON_BLACKBOARD.md`.

## Task: 2026-05-13 Manifested Party Damage Projectile Helper Split

### Task title

Track common monster impact of manifested damage/projectile helper separation.

### Goals

- Preserve manifested monster-specific projectile hook order for Rin, Sein, Vega, and Ariel.
- Preserve generic Offering-learned manifested skill damage and projectile behavior after moving helper methods out of the party partial.
- Keep monster-specific special formulas such as Rin shockwave and Eve frost field in place for this slice.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution was explicitly requested by the user for Phase 2 and will run once after Builder verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated. Phase 2 Code Reviewer completed with `REVIEW_RESULT: PASS`.

### Next Actions

- Do not run another Reviewer pass for Phase 2 unless the user explicitly requests it.
- User verifies Manifested Rin, Sein, Vega, Ariel projectile hooks and generic Offering-learned damage in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs:184` owns manifested projectile hit resolution and preserves Rin, Sein, Vega, and Ariel projectile hook order before the generic damage fallback.
- `CombatRuntimeManifestedPartyDamage.cs:311`, `:316`, `:335`, `:368`, and `:451` own generic manifested skill damage, effect damage, base damage, and damage multiplier helpers.
- `CombatRuntimeManifestedPartyDamage.cs:63`, `:81`, `:112`, and `:124` own manifested projectile fire and pierce helper methods.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:351`, `:490`, and `:512` retain Rin shockwave, persistent skill routing, and Eve frost field special behavior in the party partial.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.
- External Phase 2 Code Reviewer output was saved to `codex_loop_logs\phase2_manifested_party_reviewer_20260513.md` and ended with `REVIEW_RESULT: PASS`.

### History

- 2026-05-13: Builder separated generic manifested damage and projectile-fire helpers after the runtime, view binder, skill dispatcher, drone lifecycle, and visual helper Phase 2 slices.
- 2026-05-13: External Code Reviewer returned `REVIEW_RESULT: PASS` for the Phase 2 manifested party refactor.

## Task: 2026-05-13 Manifested Party Skill Visual Helper Split

### Task title

Track common monster impact of manifested skill visual helper separation.

### Goals

- Preserve Manifested monster-specific skill visual shape and duration behavior.
- Preserve generic Offering-learned manifested skill visuals after moving helper methods out of the party partial.
- Avoid changing monster damage formulas, skill dispatch order, or projectile firing in this slice.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Eve, Sein, Vega, Ariel, and generic Offering-learned skill visuals in RunScene Play Mode.
- Future Phase 2 work should move remaining formula or projectile-fire responsibilities in small slices with monster-specific evidence.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyVisuals.cs:60` keeps manifested visual duration resolution after the split.
- `CombatRuntimeManifestedPartyVisuals.cs:67`, `:82`, `:87`, and `:97` preserve existing `eve-b`, `sein-d`, `vega-c`, and `ariel-c` duration cases.
- `CombatRuntimeManifestedPartyVisuals.cs:120`, `:132`, and `:154` preserve circle, line, and shared visual configuration helpers.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:371`, `:401`, and `:757` remain call sites for the moved helpers, so monster skill dispatch and damage formulas were not changed by this slice.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder separated manifested non-drone skill visual helpers after the runtime, view binder, skill dispatcher, and drone lifecycle Phase 2 slices.

## Task: 2026-05-13 Manifested Party Drone Lifecycle Split

### Task title

Track common monster impact of manifested Eve drone lifecycle separation.

### Goals

- Preserve Manifested Eve drone beacon deployment, lifetime, target lookup, and firing cadence.
- Keep `manifestedDrones` owned through the manifested party runtime service-backed list.
- Avoid changing non-Eve monster unit dispatch or damage formulas in this slice.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Eve drone beacon behavior in RunScene Play Mode.
- Future Phase 2 work should keep remaining monster-specific formula moves in small slices.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:19` through `:48` preserves Manifested Eve drone object creation and registration.
- `CombatRuntimeManifestedPartyDrones.cs:51` through `:92` preserves drone duration ticking, nearest-target lookup, projectile fire, and `EveDroneAttackPeriod` cadence.
- `CombatRuntimeManifestedPartyDrones.cs:95` through `:115` preserves play/edit-mode cleanup behavior.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:1586` still clears manifested drones during party clear.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder separated manifested Eve drone lifecycle after the runtime, view binder, and skill dispatcher Phase 2 slices.

## Task: 2026-05-13 Manifested Party Skill Dispatcher Split

### Task title

Track common monster impact of manifested party skill dispatch separation.

### Goals

- Preserve manifested monster-specific unit dispatch for Eve, Rin, Sein, Vega, and Ariel.
- Preserve generic Offering-learned manifested skill fallback, cooldown, reload, and magazine behavior.
- Keep `CombatUnitRuntime` skill ticking callback stable while the dispatcher moves behind the manifested party runtime service.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Eve/Rin/Sein/Vega/Ariel A-E paths and generic Offering-learned skill firing in RunScene Play Mode.
- Future Phase 2 work should avoid moving monster-specific damage formulas together with unrelated state-owner changes.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs:17`, `:22`, `:27`, `:32`, and `:37` preserve the Eve, Rin, Sein, Vega, and Ariel unit dispatch order before generic fallback.
- `CombatRuntimeManifestedPartySkills.cs:42` through `:71` preserves fallback cooldown target selection and projectile/non-projectile dispatch.
- `CombatRuntimeManifestedPartySkills.cs:86` through `:139` preserves manifested magazine firing, Vega three-sword flurry, Eve drone beacon, reload, and shot cooldown behavior.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:193` still calls `Owner.TickManifestedUnitSkill(...)`, so this slice does not require monster runtime call-site migration.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder separated manifested party skill dispatch after the runtime and view binder Phase 2 slices.

## Task: 2026-05-13 Manifested Party View Binder Split

### Task title

Track common monster impact of manifested party view binding separation.

### Goals

- Preserve Manifested monster name, HP, shield, fallback label, and scene slot status display behavior.
- Keep monster-specific `CombatUnitRuntime` skill/state behavior unchanged.
- Avoid changing Offering-learned active skill synchronization or monster-specific unit dispatch in this slice.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested monster labels, HP/shield bars, and learned skill display in RunScene Play Mode.
- Future Phase 2 skill-dispatch extraction should preserve Eve/Rin/Sein/Vega/Ariel unit paths.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs:23` through `:52` keeps support for `MonsterNameLabel`, `Name Label`, `NameLabel`, `MonsterHpLabel`, `HPLabel`, `HPLable`, `HP Label`, and HP/shield bar paths.
- `CombatRuntimeManifestedPartyView.cs:256` through `:302` preserves manifested name, HP text, shield text, fallback combined label, and HP/shield bar refresh.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:194`, `:197`, `:224`, `:300`, and `:334` still call the view helpers during unit creation/reset/tick.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder separated manifested party view binding after the initial manifested party runtime service boundary.

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
