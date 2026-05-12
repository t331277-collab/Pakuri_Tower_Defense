## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-10` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/COMBAT/COMBAT_BLACKBOARD.md`.

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
