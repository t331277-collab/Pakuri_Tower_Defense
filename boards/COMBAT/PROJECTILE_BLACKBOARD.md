## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-10` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/COMBAT/PROJECTILE_BLACKBOARD.md`.

## Task: 2026-05-13 Phase 3 Projectile Slice Plan

### Task title

Define projectile-specific Phase 3 slices before implementation.

### Goals

- Start Phase 3 with projectile simulation ownership before skill effects and drones.
- Keep projectile hit behavior stable while making the loop readable in smaller boundaries.
- Preserve selected, manifested, enemy, and monster-specific projectile hooks.

### Constraints

- Role Owner is Designer.
- Do not change projectile runtime C# behavior.
- Do not move damage application APIs or common target model in Phase 3.

### Role Owner

Designer

### Status

Completed. Projectile work should occupy Phase 3-A through Phase 3-C.

### Next Actions

- Phase 3-A: introduce the projectile simulation boundary shell.
- Phase 3-B: move cleanup/lifetime/edge-removal responsibility behind the boundary.
- Phase 3-C: split hit routing into enemy projectile, manifested projectile, and selected/player projectile handlers without changing damage formulas.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` through `:154` currently combines projectile iteration, movement, enemy projectile target hits, manifested projectile hits, selected/player projectile hits, status application, pierce decrement, edge checks, and cleanup calls.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:182` through `:245` owns enemy projectile hit resolution against manifested units, selected monster, and nexus.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:255` through `:301` owns selected/player projectile enemy hit detection.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:649` through `:660` owns projectile cleanup and list removal.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs:184` owns manifested projectile hit resolution and must remain behaviorally compatible during Phase 3-C.
- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:27` through `:30` already routes projectile registration through `AddBattlefieldProjectile(...)`.

### History

- 2026-05-13: Designer split Phase 3 projectile work into 3-A through 3-C before Code Builder implementation.

## Task: 2026-05-13 Phase 2 Projectile Closeout Verification

### Task title

Confirm manifested projectile helper closeout before Phase 3 projectile simulation work.

### Goals

- Verify that Phase 2-E already moved generic manifested projectile fire and hit helpers.
- Confirm remaining Vega queued projectile behavior is not an independent Phase 2-F projectile helper split.
- Preserve projectile gameplay behavior before Phase 3 simulation ownership work.

### Constraints

- Role Owner is Code Builder.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode; projectile gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Completed. Phase 2 projectile helper closeout is verified; next projectile work belongs to Phase 3 simulation ownership.

### Next Actions

- Start Phase 3 by moving projectile ticking/lifetime/collision cleanup behind a simulation boundary.
- Preserve Vega queued projectile cadence and hit hook order during Phase 3.

### Evidence

- `CombatRuntimeManifestedPartyDamage.cs:63`, `:81`, `:112`, `:124`, and `:184` already own manifested projectile fire, pierce, object/runtime creation, and hit resolution.
- `CombatRuntimeParty.cs:701` through `:744` keeps Vega queued projectile setup and ticking because it depends on `CombatSkillRuntime.PendingVegaProjectileCount`, `PendingVegaProjectileIndex`, `PendingVegaProjectileDelay`, and `PendingVegaProjectileDirection`.
- `CombatRuntimeManifestedPartySkills.cs:82` ticks queued projectiles together with skill runtime ticking, and `:115` queues Vega flurry from magazine skill firing.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle and console warning/error read showed only MCP client handler logs.

### History

- 2026-05-13: Code Builder verified that no additional manifested projectile helper split should block Phase 3.

## Task: 2026-05-13 Manifested Party Projectile Fire Helper Split

### Task title

Track projectile impact of manifested party damage/projectile helper separation.

### Goals

- Preserve manifested projectile object creation, facade registration, speed/lifetime/hit-radius/status setup, hit resolution, pierce, and source follow-up behavior.
- Keep projectile simulation and cleanup in existing projectile runtime files for this Phase 2 slice.
- Preserve monster-specific projectile hook order for Rin, Sein, Vega, and Ariel.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns projectile gameplay verification.
- Code Reviewer execution was explicitly requested by the user for Phase 2 and will run once after Builder verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated. Phase 2 Code Reviewer completed with `REVIEW_RESULT: PASS`.

### Next Actions

- Do not run another Reviewer pass for Phase 2 unless the user explicitly requests it.
- User verifies manifested projectile fire, pierce, status application, and monster-specific projectile hit hooks in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs:63`, `:81`, `:112`, and `:124` own manifested projectile fire entry points, pierce resolution, and projectile object/runtime creation.
- `CombatRuntimeManifestedPartyDamage.cs:163` through `:175` sets speed, lifetime, hit radius, base damage, status chance, manifested source data, and status effect ID on `ProjectileRuntime`.
- `CombatRuntimeManifestedPartyDamage.cs:184` owns manifested projectile hit resolution and preserves Rin, Sein, Vega, and Ariel projectile hook order.
- `CombatRuntimeManifestedPartyDamage.cs:223` through `:229` preserves projectile status, branch, source effects, and Vega mark application after a manifested hit.
- `CombatRuntimeManifestedPartyDamage.cs:258` and `:289` own area follow-up damage and manifested projectile status application.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:739` still fires queued Vega projectiles through `FireManifestedMonsterProjectile(...)`.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.
- Unity-MCP imported the new script and console warning/error read returned only MCP client handler logs after refresh.
- External Phase 2 Code Reviewer output was saved to `codex_loop_logs\phase2_manifested_party_reviewer_20260513.md` and ended with `REVIEW_RESULT: PASS`.

### History

- 2026-05-13: Builder moved generic manifested projectile fire and hit helper methods into `CombatRuntimeManifestedPartyDamage.cs` as Phase 2-E.
- 2026-05-13: External Code Reviewer returned `REVIEW_RESULT: PASS` for the Phase 2 manifested party refactor.

## Task: 2026-05-13 Battlefield Facade Projectile Registration

### Task title

Route projectile registration through the Phase 1 battlefield facade.

### Goals

- Replace direct battlefield `projectiles.Add(...)` registration writes with facade calls.
- Preserve projectile update, hit, cleanup, lifetime, and damage behavior.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build and Unity-MCP console checks.

### Next Actions

- User verifies projectile behavior in Play Mode if needed.
- Future Phase 3 can move projectile ticking/lifetime ownership behind the facade.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:27` through `:30` adds `AddBattlefieldProjectile(...)`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:633` now registers the default selected projectile through `AddBattlefieldProjectile(projectile)`.
- `Select-String` found projectile facade calls in enemy, party, Ariel, Eve, Rin, Sein, and Vega projectile spawn paths.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Code Builder implemented Phase 1 battlefield facade boundary and routed projectile registration writes through it.

## Task: 2026-05-10 Ariel Unit Projectile Runtime

### Task title

Route Manifested Ariel projectile hits and White Judgement explosion through Ariel unit logic.

### Goals

- Resolve Manifested Ariel A projectile damage through Ariel unit passive/choice helpers.
- Apply Ariel A master Holy Exposure and White Judgement explosion using the manifested source unit.
- Preserve selected Ariel projectile behavior.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode projectile verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP checks.

### Next Actions

- User verifies Manifested Ariel A projectile damage, pierce, Holy Exposure, and White Judgement explosion in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:1062` checks `TryApplyArielUnitProjectileHit(...)` during Manifested projectile hit resolution.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:1300` implements Ariel unit projectile hit damage and A master Holy Exposure.
- `CombatRuntimeArielSkills.cs:991` and `:1026` route pending Ariel judgement explosions through manifested source-unit damage when the projectile is manifested.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:65` and `:77` trigger pending Ariel judgement explosions for manifested projectile hit cleanup and X-edge cleanup.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: Ariel unit executor migration added projectile-specific unit hooks for Manifested Ariel.

## Task: 2026-05-10 Vega Unit Projectile Runtime

### Task title

Route Manifested Vega projectile hits through Vega unit projectile logic.

### Goals

- Keep Manifested Vega A as projectile objects with three-sword cadence.
- Resolve Manifested Vega projectile damage through the source `CombatUnitRuntime` and F-J passive state.
- Preserve Vega name-mark source guarding while adding A master afterimage and unit buff mark bonuses.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode projectile verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP checks.

### Next Actions

- User verifies Manifested Vega A projectile damage, mark stacks, afterimage, and kill mark-transfer behavior in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:1054` checks `TryApplyVegaUnitProjectileHit(...)` during Manifested projectile hit resolution.
- `CombatRuntimeParty.cs:1547` queues a fourth Manifested Vega A shot when `vega-a-master-1` is present.
- `CombatRuntimeParty.cs:1569` uses `GetVegaUnitThreeSwordDamageMultiplier(...)` for unit-owned Vega A projectile damage.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:1068` implements unit-owned Vega projectile hit damage with passive final damage, flat defense reduction, and critical chance.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: Vega unit executor migration added projectile-specific unit hooks for Manifested Vega.
