## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps active combat task blocks after the 2026-05-12 archive pass; newer combat tasks may be appended above older retained context.
- Source file: `boards/COMBAT/COMBAT_BLACKBOARD.md`.

## Task: 2026-05-13 Manifested Party Runtime Split Phase 2 Start

### Task title

Start Phase 2 by adding a manifested party runtime service boundary.

### Goals

- Begin separating manifested party state and combat tick orchestration from `CombatRuntimeParty.cs`.
- Preserve existing selected/manifested combat behavior, scene slot binding, monster skill dispatch, and RunScene MonsterPanel data flow.
- Keep the first Phase 2 slice small enough to build and review before moving more logic.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented as the first Phase 2 slice and locally validated.

### Next Actions

- Continue Phase 2 by moving view binding or unit skill dispatch behind the new runtime service in separate slices.
- User performs Play Mode verification for manifested party slot activation, skill firing, HP/shield labels, and MonsterPanel snapshots.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs`.
- `CombatRuntimeManifestedPartyRuntime.cs:8` through `:12` owns `manifestedParty` plus compatibility accessors for existing manifested monster, drone, and slot users.
- `CombatRuntimeManifestedPartyRuntime.cs:42` through `:60` owns the manifested party top-level tick loop and separates per-unit skill sync, combat tick, and view refresh calls.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:553` through `:583` delegates `UpdateManifestedMonsterPartyCombat()` into the service and keeps unit validity, skill sync, combat tick, and view refresh isolated in separate helpers.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the new script and console warning/error read returned only MCP client handler logs after refresh.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 2 from `boards/REFACTORING/REFACTORING.md`.
- 2026-05-13: Builder added a manifested party service boundary without changing the combat update order or selected/manifested skill dispatch behavior.

## Task: 2026-05-13 Combat Refactor Start Plan

### Task title

Design the starting order for a full combat runtime refactor.

### Goals

- Reconcile the shared target / temporary effect design with the CombatRuntimeController token/refactor proposal.
- Confirm the current combat code still has shared mutable state and target/effect ownership problems.
- Choose the safest first implementation step for a large refactor.

### Constraints

- Role Owner is Designer.
- Preserve current player-facing combat behavior until Code Builder receives an explicit implementation handoff.
- Do not run Unity Play Mode.

### Role Owner

Designer

### Status

Phase 1 battlefield facade boundary implemented.

### Next Actions

- First battlefield/state ownership facade is in place; next implementation should either extend facade read/query methods or begin manifested party runtime split.
- Use `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md` as the Phase 0 ownership source before Phase 1 implementation.
- Code Builder should verify runtime and editor builds after any implementation slice.
- Use `boards/REFACTORING/REFACTORING.md` as the phase-order source for the `CombatRuntimeController` structure split.

### Evidence

- `CombatRuntimeController.cs:307` through `:310` still owns `enemies`, `projectiles`, `skillEffects`, and `drones` lists.
- `CombatRuntimeController.cs:326` through `:378` still owns selected-unit HP, shield, stats, monster skill ids, and projectile configuration fields.
- `CombatRuntimeController.cs:481` through `:505` still orchestrates spawning, enemies, projectiles, skill effects, manifested party combat, selected unit combat, HUD, and battle resolution from one update loop.
- `CombatUnitRuntime.cs:21` through `:50` stores manifested unit combat state plus monster-specific timers and shield state.
- `CombatRuntimeEnemies.cs:724` through `:765` directly decrements enemy status/buff timers.
- `CombatRuntimeEveSkills.cs:1682` through `:1731` directly applies Eve F shield to selected controller fields and manifested runtime fields separately.
- Added design report `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html`.
- 2026-05-13 follow-up verification: current code search found `CombatRuntimeController.cs:28` defines `EnemyRuntime` as a private nested class, while `CombatUnitRuntime.cs:8` defines manifested units as a separate `MonoBehaviour`; therefore direct common base-class inheritance should come after `ICombatTarget` / adapter and effect-layer stabilization.
- 2026-05-13 follow-up verification: updated `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html` to state that the current plan enables God Class reduction, skill reuse, common target model, and temporary effects, but explicit common base-class inheritance needs a later migration phase.
- 2026-05-13 follow-up planning: added `boards/REFACTORING/REFACTORING.md` with the phase order from `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- 2026-05-13 Phase 0 start: added `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md`, mapping current mutable combat-state owners and proposed next owners before code extraction.
- 2026-05-13 Phase 1 implementation: added `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs`, routed enemy/projectile/skill-effect/drone battlefield list registration through `AddBattlefield*` methods, and preserved existing update order.
- `Select-String` after implementation found 52 `AddBattlefield*` call sites and no remaining raw battlefield list registration writes except non-battlefield hit-set additions and `manifestedDrones`.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings; Unity-MCP console read after import/refresh showed only MCP client handler logs.

### History

- 2026-05-13: User requested a structural refactor plan based on the two existing 2026-05-10 reports before starting a major combat rewrite.
- 2026-05-13: User asked to re-verify whether the plan satisfies the two proposal goals including skill reuse, Monster/Enemy objectification, common inheritance, and God Class cleanup.
- 2026-05-13: User asked to record the `CombatRuntimeController` structure split work order under `boards/REFACTORING/REFACTORING.md`.
- 2026-05-13: User asked to start the refactor from Phase 0, `State Ownership Map`.
- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 1 `Battlefield Facade Boundary`.

## Task: 2026-05-10 Ariel Selected Shield Timer And Archangel Visual Fix

### Task title

Move selected-unit shield expiry to common combat update and share Ariel E battlefield visual creation.

### Goals

- Decouple selected 1P shield duration from selected-Ariel-only cooldown ticking.
- Preserve Manifested Ariel team shield behavior while ensuring selected 1P shield UI/state expires.
- Make Ariel E effect creation independent of nearest-target visual fallback.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies shield expiry and Ariel E visual in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:83` through `:88` previously decremented selected shield duration only from Ariel cooldown updates.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:518` now calls `UpdateSelectedUnitShieldTimer(Time.deltaTime)` during every selected-unit combat update.
- `CombatRuntimeArielSkills.cs:86` clears `unitShieldValue`, Archangel shield tracking, and `selectedUnitRuntime` shield mirror fields when selected shield duration reaches zero.
- `CombatRuntimeArielSkills.cs:700` creates a battlefield-wide Archangel Descent effect used by selected and Manifested Ariel E.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:35` adds `ShieldAppliedFrame`, and `CombatUnitRuntime.cs:160` skips 2P-5P shield timer decay on the application frame.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:28` adds `unitShieldAppliedFrame`, and `CombatRuntimeArielSkills.cs:95` skips 1P shield timer decay on the application frame.
- Follow-up: `CombatRuntimeArielSkills.cs:831` and `:902` stamp shield application with `Time.frameCount`; `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:79` mirrors the selected unit frame state.
- Runtime and Editor builds completed with 0 errors and existing warnings; Unity-MCP refresh reached ready and console showed only MCP client handler logs.
- Follow-up runtime and Editor builds completed with 0 errors and existing warnings; Unity-MCP console showed only MCP client handler/timeout logs.

### History

- 2026-05-10: User reported 1P shields from Manifested Ariel did not expire and Ariel E effect was missing.
- 2026-05-10: User then reported 1P shield duration appeared shorter than 2P-5P; Builder made selected and manifested shield timers start decaying on the same next-frame basis.

## Task: 2026-05-10 Ariel Unit Executor And Party Shield Runtime

### Task title

Add Ariel-specific unit executor dispatch and manifested party shield absorption.

### Goals

- Dispatch Manifested Ariel A-E through Ariel unit runtime before generic fallback.
- Resolve Ariel unit damage/passives through the source `CombatUnitRuntime`.
- Let manifested units absorb Ariel shields before HP loss.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies RunScene Play Mode behavior for Manifested Ariel A-E and selected Ariel team shields.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:637` inserts `TryTickArielUnitSkill(...)` after Vega dispatch and before generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:422` through `:681` dispatches Ariel unit A-E by `SkillSlot`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:464` through `:473` absorbs manifested-unit shield damage and calls `HandleArielUnitShieldAbsorbed(...)`.
- `CombatRuntimeArielSkills.cs:1515` resolves Ariel sanctuary damage reduction for unit targets.
- Runtime and Editor builds completed with 0 errors and existing warnings; Unity-MCP refresh reached idle and console warning/error read returned only MCP client handler logs.

### History

- 2026-05-10: Added Ariel unit executor dispatch and party shield absorption from the report's remaining Ariel migration item.

## Task: 2026-05-10 Vega Unit Executor Migration

### Task title

Add Vega-specific unit executor dispatch to combat runtime.

### Goals

- Dispatch Manifested Vega skills through Vega unit executor code before generic manifested fallback.
- Resolve Manifested Vega projectile and skill damage through the source `CombatUnitRuntime` and F-J passive state.
- Preserve existing selected 1P Vega manual/automatic behavior.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP checks.

### Next Actions

- User verifies RunScene Play Mode behavior for Manifested Vega A-E and F-J passive interactions.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:630` inserts `TryTickVegaUnitSkill(...)` after Eve/Rin/Sein unit dispatch and before generic fallback.
- `CombatRuntimeParty.cs:1054` inserts `TryApplyVegaUnitProjectileHit(...)` into Manifested projectile damage resolution.
- `CombatRuntimeParty.cs:1547` and `:1569` now let Vega unit choices affect queued A projectile count and damage.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:139` through `:168` dispatches A-E by `SkillSlot`.
- `CombatRuntimeVegaSkills.cs:1334` applies Vega unit final-damage passive logic for physical projectile/skill damage.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- `git diff --check` over the changed scripts completed with exit code 0.
- Unity-MCP refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-10: Added Vega unit executor dispatch/damage hooks from the report's remaining Vega migration item.

## Task: 2026-05-10 Combat Shield Runtime Review

### Task title

Fix Eve F shield runtime and validate shield-bearing monster skills.

### Goals

- Confirm combat shield runtime paths for shield-bearing monster skills found under `Pakuri/reference/2.Monster`.
- Remove Eve's duplicate selected shield timer decrement.
- Apply Eve F shields to lightning-skill manifested allies using the same shield runtime fields.

### Constraints

- Role Owner is Code Builder.
- Detailed Eve status is recorded in `boards/MON/EVE_MONSTER.md`.
- Detailed status-effect timer evidence is recorded in `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies combat behavior in Play Mode for Ariel B/E and Eve F.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Reference search under `Pakuri/reference/2.Monster` found concrete shield implementations for Ariel B/E and Eve F.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs` has selected and team shield application paths with `ShieldAppliedFrame`.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:93` through `:108` removes the Eve-local selected shield timer decrement.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1558` through `:1594` identifies selected and manifested lightning skills.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1682` through `:1732` applies Eve F shield to selected and manifested lightning allies.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console error read returned only MCP client handler logs.

### History

- 2026-05-10: User asked for Eve and other shield skill application to be reviewed and fixed where needed.
