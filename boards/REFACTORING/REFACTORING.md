# REFACTORING

This board records broad refactoring plans that cut across combat, monster runtime, data flow, UI, and reports.

When doing related work, follow `MDTREE.md` routing and update this file together with the affected domain boards.

## Task: 2026-05-13 CombatRuntimeController Structure Split Plan

### Task title

Plan the `CombatRuntimeController` structure split refactor from the 2026-05-10 proposal.

### Goals

- Turn the 2026-05-10 `CombatRuntimeController` structure proposal into an executable work order.
- Reduce the God Class / shared-state pressure around `CombatRuntimeController`.
- Split work in a sequence that preserves current combat behavior.
- Make later skill reuse, common target modeling, temporary effects, and Monster/Enemy objectification possible without creating two sources of truth.
- Define which board files must be updated as each implementation slice lands.

### Constraints

- Role Owner is Designer for this planning task.
- Code Builder must not start implementation from this board alone; each slice still needs explicit implementation work.
- Preserve current player-facing combat behavior unless a later task explicitly approves behavior change.
- Do not run Unity Play Mode as Codex verification. User owns gameplay verification.
- Code Reviewer execution requires explicit user permission.
- Do not introduce runtime combat singletons for battle state. The inspected proposal says singletons are valid for global definitions/context, not mutable battle state.

### Role Owner

Designer

### Status

Phase 1 battlefield facade boundary implemented. Phase 2 implementation has started with a manifested party runtime service boundary.

### Next Actions

- Continue Phase 2 by moving more manifested party view binding and skill dispatch code behind `ManifestedPartyRuntime` in reviewable slices.
- Do not start by extracting every monster skill executor or by moving all HP/shield/status state into `CombatTargetModel`.
- After each implementation slice, run runtime/editor builds and update this board plus the affected combat/projectile/status/monster/report board files.

### Evidence

- `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html:543` through `:550` proposes the order: state ownership document, Manifested Party, Projectile / Effect / Drone, Enemy Simulation, Selected Unit Combat, then narrower monster runtime adapter interfaces.
- `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html:734` states that state ownership separation is more important than file splitting.
- `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html:736` states the first boundaries to handle are `Manifested Party`, `Projectile / Effect / Drone`, and `Enemy Simulation`.
- Current inspected `CombatRuntimeController` partial files total 14 files, 14,022 lines, and 668,782 characters.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:307` through `:310` owns `enemies`, `projectiles`, `skillEffects`, and `drones`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:357` and `:369` own selected-monster and selected-unit runtime state.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:481` through `:505` runs the main update orchestration.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:557` owns the current manifested party combat update entry point.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` owns projectile update and `:516` owns selected monster combat update in the current partial layout.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:706` owns enemy update in the current partial layout.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:29` shows monster skill runtime adapters still hold a full `CombatRuntimeController` reference.
- `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html` adds the later validation that common base-class inheritance for Monster/Enemy should come after `ICombatTarget` / adapter and effect-layer stabilization.
- `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md` records the Phase 0 state ownership table and declares proposed owners for Phase 1 through Phase 7.
- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:7` through `:79` adds `CombatBattlefieldState` and facade add methods for enemies, projectiles, skill effects, and drones.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:8` through `:12` adds the `manifestedParty` service and routes existing `manifestedMonsters`, `manifestedDrones`, and `manifestedMonsterSlots` accessors through it.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:42` through `:60` owns the manifested party combat tick loop and separates unit skill sync, combat ticking, and view refresh calls.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:553` through `:583` now delegates `UpdateManifestedMonsterPartyCombat()` to `manifestedParty.TickCombat(...)` and splits unit validity, skill sync, combat tick, and view refresh into separate helper methods.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2 service slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2 service slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs` as a `UnityEditor.MonoScript`; after forced script refresh, console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `Select-String` after implementation found 52 `AddBattlefield*` call sites and no remaining raw `enemies.Add`, `projectiles.Add`, `skillEffects.Add`, or `drones.Add` calls for battlefield lists; remaining `.Add` matches were internal hit sets and `manifestedDrones`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after a first parallel run hit an `obj\Debug\Assembly-CSharp.dll` file lock.
- Unity-MCP imported `Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs` as a `UnityEditor.MonoScript`; after forced script refresh, console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.

### History

- 2026-05-13: User asked to write how the `CombatRuntimeController` structure split refactor should proceed, based on `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`, into `boards/REFACTORING/REFACTORING.md`.
- 2026-05-13: User asked to start Phase 0, `State Ownership Map`; Designer created `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md` without runtime C# changes.
- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 1 `Battlefield Facade Boundary`; Builder added the battlefield facade partial under `Assets/Scripts/Combat/Battlefield` and routed battlefield list registration through facade methods.
- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 2 `Manifested Party Runtime Split`; Builder added a manifested party runtime service boundary while preserving existing selected/manifested combat behavior paths.

## Current Refactor Order

### Phase 0. State Ownership Map

Status:

- Completed as `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md`.

Purpose:

- Document which system owns each mutable combat state before moving code.
- Prevent implementation from only moving methods while keeping hidden shared state.

Work:

- Map controller-owned battlefield lists: `enemies`, `projectiles`, `skillEffects`, `drones`.
- Map selected unit state: selected monster definition, selected unit runtime, HP/shield, magazine/reload, projectile config, status UI state.
- Map manifested party state: party slots, `CombatUnitRuntime` list, HP/shield bars, learned active skills, manifested skill runtimes.
- Map enemy simulation state: spawn state, movement, targeting, enemy HP/shield/status/buff fields.
- Map skill executor dependencies: monster skill files that directly access battlefield lists, enemy state, selected unit state, and manifested party state.

Acceptance criteria:

- A state ownership table exists before code extraction starts.
- Every field moved in later phases has a declared old owner and new owner.
- No behavior changes are made in this phase.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/REPORT/REPORT_BLACKBOARD.md`

### Phase 1. Battlefield Facade Boundary

Status:

- Implemented as `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs`.
- Battlefield registration writes now route through `AddBattlefieldEnemy`, `AddBattlefieldProjectile`, `AddBattlefieldSkillEffect`, and `AddBattlefieldDrone`.
- Update loops, cleanup loops, target queries, runtime nested classes, and list storage remain in their existing files for behavior preservation.

Purpose:

- Create the first safe boundary around shared battlefield lists before splitting larger systems.
- Make skill executors request battlefield actions rather than directly mutating lists.

Work:

- Introduce a small battlefield facade or internal service around `enemies`, `projectiles`, `skillEffects`, and `drones`.
- Start with one narrow path, such as manifested visual/projectile spawn or one Eve projectile/effect spawn path.
- Keep the underlying lists in place during the first slice; change access path, not behavior.
- Add request-style methods for spawn/find operations before moving the full simulation loop.

Acceptance criteria:

- One selected spawn path no longer calls the raw list directly.
- Existing update order in `CombatRuntimeController.Update()` remains unchanged.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/COMBAT/PROJECTILE_BLACKBOARD.md`
- Relevant monster board if a monster skill file changes.

### Phase 2. Manifested Party Runtime Split

Status:

- Started as `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs`.
- The first slice moves manifested party list/slot storage and top-level party tick orchestration behind a service boundary.
- Skill dispatch, damage formulas, view binding helpers, and scene object creation remain in existing controller partials for behavior preservation.

Purpose:

- Split the largest manager partial boundary after the battlefield access path is available.
- Remove party combat/state/UI responsibility from the controller partial surface.

Work:

- Separate manifested party state from manifested party view updates.
- Extract party skill ticking and unit dispatch into a party runtime service.
- Keep scene slot binding and Unity object references stable.
- Keep existing selected/manifested behavior parity until Play Mode verification by the user.

Acceptance criteria:

- `CombatRuntimeController` delegates manifested party tick to a dedicated object or narrowed service.
- Party label/HP/shield view update is isolated from damage/cooldown logic.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/MON/MON_BLACKBOARD.md`
- Relevant `boards/MON/{NAME}_MONSTER.md` files if monster-specific unit dispatch changes.
- `boards/UI/RUNSCENE_UI.md` if party UI display behavior changes.

### Phase 3. Projectile / Effect / Drone Simulation Split

Purpose:

- Move battlefield object lifecycle out of controller partials after facade access is established.
- Make projectile/effect/drone simulation independently readable and testable.

Work:

- Extract projectile ticking, collision, lifetime, and cleanup into a simulation service.
- Extract skill effect and drone ticking into the same battlefield simulation layer or separate narrow services.
- Keep damage application API stable until common target/effect migration starts.

Acceptance criteria:

- `CombatRuntimeController.Update()` calls a narrowed simulation tick method.
- Projectile/effect/drone lists are owned by the simulation boundary or facade, not directly scattered through skill files.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/COMBAT/PROJECTILE_BLACKBOARD.md`
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md` if status application changes.

### Phase 4. Enemy Simulation Split

Purpose:

- Separate spawning, movement, targeting, enemy active skills, and enemy status/buff timers from selected-unit and controller logic.

Work:

- Move enemy update and spawn policy into an enemy simulation boundary.
- Prepare `EnemyRuntime` to stop being only a private nested implementation detail.
- Keep enemy target priority and damage behavior unchanged.

Acceptance criteria:

- Enemy update can be read without loading selected-unit combat and manifested party implementation details.
- Enemy simulation exposes only narrow query/application APIs to the battlefield and skill layers.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/COMBAT/ENEMY_BLACKBOARD.md`
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`

### Phase 5. Selected Unit Combat Split

Purpose:

- Move 1P reload, automatic skill trigger, shield mirror, and selected-unit combat state out of the God Class center after shared battlefield/enemy boundaries exist.

Work:

- Extract selected unit combat tick from `CombatRuntimeProjectiles.cs` / controller partial surface.
- Keep selected unit state synchronized with existing `CombatUnitRuntime` until `CombatTargetModel` migration.
- Avoid changing current skill behavior in the same slice.

Acceptance criteria:

- Selected unit tick is owned by a selected-unit combat boundary.
- Existing `Update()` order still produces the same combat phases.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- Relevant monster board if selected monster skill behavior changes.

### Phase 6. Monster Runtime Adapter Interface Narrowing

Purpose:

- Stop monster skill runtime adapters from seeing the whole `CombatRuntimeController`.
- Enable skill reuse by making skills request target lookup, projectile/effect spawn, damage, and temporary effects through narrow contracts.

Work:

- Define small interfaces for target query, damage application, battlefield spawn, and temporary effect application.
- Migrate `CombatMonsterSkillRuntime` and monster-specific runtime adapters to those interfaces incrementally.
- Keep monster-specific formulas in existing files until common effect/target layers are stable.

Acceptance criteria:

- At least one monster runtime path no longer needs full controller access.
- Skill executor dependencies are narrower than `CombatRuntimeController`.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/MON/MON_BLACKBOARD.md`
- Relevant `boards/MON/{NAME}_MONSTER.md`

### Phase 7. CombatTarget / TemporaryEffect Migration

Purpose:

- Satisfy the shared target and temporary effect design after structural boundaries reduce duplication risk.
- Move toward reusable skill effects and common Monster/Enemy object handling.

Work:

- Introduce `ICombatTarget` / adapter first for selected unit, manifested unit, and enemy.
- Add `CombatTargetModel` only after read adapters prove stable.
- Introduce `TemporaryEffectInstance` for action speed, shield, then status effects.
- Decide whether a common `CombatActorBase` or pure target model base class is worthwhile after adapter/effect behavior stabilizes.

Acceptance criteria:

- Selected unit, manifested units, and enemies can be read through a common target contract.
- At least one temporary effect applies through a common effect API to more than one target type.
- No direct common base-class inheritance is introduced before adapter behavior is verified.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/MON/MON_BLACKBOARD.md`
- Relevant monster files.

## Implementation Gate

Each implementation phase must provide:

- Changed file list.
- Build evidence: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`.
- Build evidence: `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`.
- `git diff --check` evidence.
- Unity-MCP refresh / console evidence where relevant.
- Explicit note that Unity Play Mode gameplay verification remains user-owned.
- Board updates for every affected domain.
