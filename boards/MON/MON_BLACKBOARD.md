## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/MON_BLACKBOARD.md`.

## Task: 2026-05-13 Phase 3 Monster Skill And Drone Boundary Plan

### Task title

Record monster-skill impact of Phase 3 projectile/effect/drone simulation split.

### Goals

- Preserve monster-specific projectile, effect, and drone behavior while lifecycle ownership moves.
- Keep same-type skill reuse for Phase 6, not Phase 3.
- Identify that selected Eve drones and manifested Eve drones are separate runtime types today.

### Constraints

- Role Owner is Designer.
- Do not change monster runtime C# behavior.
- Do not merge selected and manifested drone runtime classes unless Code Builder later proves it is a safe behavior-preserving slice.

### Role Owner

Designer

### Status

Completed. Monster-domain Phase 3 impact is recorded.

### Next Actions

- Phase 3-F should handle selected Eve `DroneRuntime` lifecycle.
- Phase 3-G should align manifested `ManifestedDroneRuntime` lifecycle with the new simulation boundary without forcing class unification.
- Keep exact same-type skill executor reuse decisions until Phase 6.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:177` through `:184` calls each monster runtime adapter's `UpdateEffects()`.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1286` through `:1342` owns selected Eve drone ticking and projectile creation through `DroneRuntime`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:8` through `:16` defines separate `ManifestedDroneRuntime` fields for source unit, skill, GameObject, duration, and cooldown.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:51` through `:92` ticks manifested drones and fires manifested monster projectiles.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:49` calls `owner.UpdateManifestedDrones()` as part of manifested party combat ticking.

### History

- 2026-05-13: Designer recorded that Phase 3 should move lifecycle ownership while deferring broad monster skill reuse to Phase 6.

## Task: 2026-05-13 Monster Skill Reuse And Common Actor Timing Note

### Task title

Record monster-domain timing for same-type skill reuse and Monster / Enemy actor commonization.

### Goals

- Keep monster skill reuse as a Phase 6 adapter-narrowing topic.
- Preserve the user's proposed grouping direction: projectile, beam / line / area / field, and summon / drone skill families.
- Leave the exact same-type grouping scope undecided until Phase 6 starts.
- Place Monster / Enemy shared parent or prefab actor work after common target/effect stabilization.

### Constraints

- Role Owner is Designer.
- Do not change monster runtime C# behavior.
- Do not decide exact skill grouping before adapter and lifecycle ownership evidence exists.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Revisit monster skill reuse during Phase 6 with current code evidence and user-selected grouping scope.
- Keep Phase 3 focused on projectile/effect/drone lifecycle ownership, not a full monster skill executor reclassification.
- Evaluate Monster / Enemy common actor inheritance or prefab view component only after Phase 7 target/effect APIs stabilize.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` with explicit same-type skill reuse timing and scope deferral.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:53`, `:68`, `:83`, `:98`, and `:113` show monster-specific runtime adapters still calling controller action-speed helpers.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:8` is currently a `MonoBehaviour`, while `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` defines enemy runtime as a private nested class; this supports interface/model-first commonization before a common parent class.

### History

- 2026-05-13: User asked to amend the roadmap with the proposed same-type skill reuse approach and Monster / Enemy common parent or prefab direction.

## Task: 2026-05-13 Phase 2 Monster Special-Case Closeout Verification

### Task title

Verify common monster impact of Phase 2 manifested party closeout.

### Goals

- Check whether remaining manifested monster special cases should be split before Phase 3.
- Preserve monster-specific formulas and behavior during closeout verification.
- Record why broad skill reuse should wait for later adapter/simulation phases.

### Constraints

- Role Owner is Code Builder.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode; monster skill verification remains user-owned.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Completed. No monster-specific Phase 2-F split is recommended before Phase 3.

### Next Actions

- Keep Rin shockwave, Eve frost field, and Vega queued projectile special cases in place for now.
- Revisit monster skill reuse during Phase 6 adapter narrowing after Phase 3 projectile/effect/drone and Phase 4-5 state owners are more stable.

### Evidence

- `CombatRuntimeParty.cs:351` through `:443` contains Manifested Rin C shockwave behavior with Rin choice checks, Rin damage helpers, knockback, slow, and reload reduction.
- `CombatRuntimeParty.cs:512` through `:564` contains Manifested Eve C frost field behavior with Eve choice checks, damage multiplier, chill stacks, freeze duration, and skill-effect registration.
- `CombatRuntimeParty.cs:701` through `:744` contains Manifested Vega A queued projectile sequencing and source-specific damage/mark stack resolution.
- `CombatRuntimeManifestedPartyDamage.cs:9` through `:22` already centralizes generic manifested skill fire and delegates these special cases before generic damage.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle and console warning/error read showed only MCP client handler logs.

### History

- 2026-05-13: Code Builder inspected remaining manifested monster special cases and concluded they should not block Phase 2 closeout.

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
