# COMBAT STATE OWNERSHIP MAP

This file is the Phase 0 output for the `CombatRuntimeController` structure split.

It records current mutable combat-state owners before code extraction starts. No runtime C# behavior is changed by this phase.

## Task: 2026-05-13 CombatRuntimeController State Ownership Map

### Task title

Create the state ownership map required before splitting `CombatRuntimeController`.

### Goals

- Identify the current owner of mutable combat state with inspected code evidence.
- Declare the intended next owner for each state group before Phase 1 extraction starts.
- Prevent later refactors from moving methods while leaving hidden shared state in `CombatRuntimeController`.
- Preserve current combat behavior during Phase 0.

### Constraints

- Role Owner is Designer for this ownership mapping task.
- This phase must not edit runtime C# files.
- Code Builder must treat proposed owners as migration targets, not as completed implementation.
- Preserve serialized fields, scene bindings, public properties, and current update order unless a later Code Builder task explicitly changes them.
- Unity Play Mode verification is user-owned.
- Code Reviewer execution requires explicit user permission.

### Role Owner

Designer

### Status

Phase 0 planning completed. Phase 1 battlefield facade implementation completed. Phase 2 manifested party runtime split has started.

### Next Actions

- Continue Phase 2 `ManifestedPartyRuntime` extraction in small slices, keeping scene slots and selected/manifested behavior stable.
- Keep `CombatRuntimeController.Update()` order unchanged until a later phase explicitly moves orchestration.
- Update this file if implementation discovers a field owner or dependency not listed below.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` through `:94` defines `EnemyRuntime` as a private nested class with enemy transform, health, shield, status, buff, and monster-specific effect fields.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:96` through `:174` defines private nested `ProjectileRuntime`, `SkillEffectRuntime`, and `DroneRuntime` classes.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:307` through `:310` stores `enemies`, `projectiles`, `skillEffects`, and `drones` directly on `CombatRuntimeController`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:326` through `:338` stores selected-unit HP, shield, ammo, shot cooldown, and reload state directly on `CombatRuntimeController`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:357` through `:386` stores selected monster definition, labels, selected runtime reference, configured stats, projectile config, and status chance directly on `CombatRuntimeController`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:481` through `:505` orchestrates the current update order: input/marker/popups, spawning, enemies, projectiles, monster runtime effects, manifested party combat, selected monster combat, selected status visuals, battle resolution.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:27` through `:29` stores manifested monsters, manifested drones, and scene slots in the controller partial.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:65` through `:80` mirrors selected-unit stats and shield into `selectedUnitRuntime`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:557` through `:583` updates manifested party combat and calls `CombatUnitRuntime.TickManifestedCombat`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:2066` through `:2104` syncs learned active skills into `CombatSkillRuntime` entries.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:306` through `:334` owns enemy spawn counters and spawn cooldown behavior.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:336` through `:398` creates `EnemyRuntime` instances and adds them to `enemies`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:706` through `:805` mutates enemy status timers, buffs, movement, and lifecycle cleanup.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:945` through `:980` chooses enemy priority targets from selected unit, manifested units, and nexus.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:399` through `:470` applies damage to enemy, selected unit, and manifested unit state.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:516` through `:645` updates selected-unit shield, cooldown, reload, ammo, and default projectile firing.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:8` through `:50` stores manifested/selected combat-unit references, stats, skills, shield fields, and monster-specific timers.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:145` through `:193` ticks manifested combat timers and calls back into the controller for each skill.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatSkillRuntime.cs:6` through `:68` stores per-skill cooldown, magazine, reload, and pending Vega projectile state.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:21` through `:38` keeps monster runtime adapters holding a full `CombatRuntimeController` reference.
- `Select-String` over `CombatRuntime*Skills.cs` found direct skill-file access to `enemies`, `projectiles.Add`, `skillEffects.Add`, `drones.Add`, `manifestedMonsters`, `selectedUnitRuntime`, `unitShieldValue`, and selected ammo/reload fields.
- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:7` through `:79` now owns a small `CombatBattlefieldState` facade over the existing `enemies`, `projectiles`, `skillEffects`, and `drones` lists.
- `CombatRuntimeEnemies.cs:398`, `CombatRuntimeProjectiles.cs:633`, `CombatRuntimeParty.cs:850`, and monster skill files now call `AddBattlefield*` methods for battlefield object registration.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:8` through `:12` now owns the manifested party runtime service field and preserves existing lower-case accessors for manifested monsters, drones, and slots.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:42` through `:60` now owns the top-level manifested party combat tick loop and separates per-unit skill sync, combat tick, and view refresh calls.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:35`, `:160`, `:553` through `:583`, and `:2153` route party count, add, tick, view refresh, and clear behavior through the manifested party service boundary.

### History

- 2026-05-13: User asked to start refactoring from Phase 0, `State Ownership Map`, based on the previous refactor plan.
- 2026-05-13: Designer inspected the current combat runtime files and created this ownership map without changing runtime C# code.
- 2026-05-13: Code Builder implemented Phase 1 by adding a battlefield facade partial and routing battlefield list registration writes through it.
- 2026-05-13: Code Builder started Phase 2 by adding `CombatRuntimeManifestedPartyRuntime.cs`, moving manifested party collection/slot ownership behind the service while leaving skill dispatch and view binding behavior intact.

## Current Update Order Boundary

`CombatRuntimeController.Update()` currently owns the top-level execution order:

1. Input and marker update.
2. Damage popup update.
3. Battle-resolved early visual update.
4. Enemy spawning.
5. Enemy simulation.
6. Projectile simulation.
7. Selected monster runtime effects.
8. Manifested party combat.
9. Selected monster combat.
10. Selected monster status visuals.
11. Battle resolution.

Phase 1 must preserve this order. A facade may sit behind a call, but the call sequence should remain unchanged until a later phase explicitly changes execution ownership.

## Ownership Table

| State group | Current owner and evidence | Current writers / readers | Proposed next owner | Move phase | Compatibility rule |
| --- | --- | --- | --- | --- | --- |
| `enemies` list | `CombatRuntimeController.cs:307`; enemy creation adds runtime at `CombatRuntimeEnemies.cs:398` | Enemy spawn/update, projectile hits, monster skill files, target queries | `CombatBattlefieldState` facade first, then `EnemySimulation` owns mutation | Phase 1 facade, Phase 4 owner split | Keep list order and cleanup timing stable. Do not expose mutable list directly after facade migration. |
| `projectiles` list | `CombatRuntimeController.cs:308`; default selected shot adds at `CombatRuntimeProjectiles.cs:633` | Projectile update/cleanup, selected and monster skill files | `CombatBattlefieldState` facade, then projectile simulation service | Phase 1 facade, Phase 3 owner split | Preserve projectile lifetime, hit order, sequence names, and cleanup behavior. |
| `skillEffects` list | `CombatRuntimeController.cs:309`; skill files add effects by direct `skillEffects.Add` calls found by `Select-String` | Monster skill files and effect update loop | `CombatBattlefieldState` facade, then effect simulation service | Phase 1 facade, Phase 3 owner split | Keep tick interval and visual duration semantics unchanged. |
| `drones` list | `CombatRuntimeController.cs:310`; Eve skill file directly adds drone runtime by `drones.Add` | Eve skill effects and drone update | `CombatBattlefieldState` facade, then drone simulation service | Phase 1 facade, Phase 3 owner split | Preserve drone attack cadence, range, duration, and spawned projectile behavior. |
| `damagePopups` list | `CombatRuntimeController.cs:311`; update order calls `UpdateDamagePopups()` before combat simulation at `CombatRuntimeController.cs:490` | Damage application paths and popup updater | Keep in visual feedback layer until combat-state split is stable | Later UI/visual cleanup, not Phase 1 | Do not block battlefield facade on popup ownership. |
| Enemy runtime model | Private nested `EnemyRuntime` at `CombatRuntimeController.cs:28-94` | Spawn, enemy update, projectile damage, skill files, labels | `EnemyRuntime` moved out of controller or wrapped by `ICombatTarget` adapter; mutation owned by `EnemySimulation` | Phase 4, then Phase 7 adapter | Do not introduce common base inheritance before adapter behavior is verified. |
| Enemy spawn counters | `pendingNormalSpawnCount`, `spawnedNormalCount`, `pendingBossSpawn`, `pendingBossSpawnCount`, `spawnCooldown` at `CombatRuntimeController.cs:330-335` | `UpdateSpawning()` at `CombatRuntimeEnemies.cs:306-334` | `EnemySpawnRuntime` inside `EnemySimulation` | Phase 4 | Preserve spawn interval, boss count, and `pendingBossSpawn` behavior. |
| Enemy status and buff timers | `EnemyRuntime` fields at `CombatRuntimeController.cs:49-91`; decremented at `CombatRuntimeEnemies.cs:724-778` | Enemy update and monster skill files | `EnemySimulation` owns enemy timers first; later common temporary effects may own transferable timers | Phase 4, then Phase 7 | Timers stay on enemy runtime until common effect migration has target adapters. |
| Enemy target priority | `GetEnemyPriorityTarget` at `CombatRuntimeEnemies.cs:945-980` reads selected unit, manifested units, and nexus | Enemy movement, enemy damage, projectile target setup | `EnemyTargetingService` or enemy simulation query over combat targets | Phase 4 or Phase 7 | Preserve selected-unit-first nearest behavior and nexus fallback. |
| Selected unit HP | `unitCurrentHealth` at `CombatRuntimeController.cs:326`; selected damage mutates it at `CombatRuntimeProjectiles.cs:446-447` | Enemy/projectile damage, HUD, selected runtime sync | `SelectedUnitCombatState`; later exposed through `ICombatTarget` | Phase 5, then Phase 7 | Keep `UnitCurrentHealth` public read behavior stable until UI bindings migrate. |
| Selected unit shield | `unitShieldValue`, `unitShieldTimer` at `CombatRuntimeController.cs:327-328`; `unitShieldAppliedFrame` in `CombatRuntimeArielSkills.cs:28`; selected shield timer at `CombatRuntimeArielSkills.cs:88-131` | Ariel/Eve shield logic, selected runtime mirror, damage absorption | `SelectedUnitCombatState` first; later common `TemporaryEffectInstance` shield effect | Phase 5, then Phase 7 | Preserve next-frame shield decay behavior and mirror into `selectedUnitRuntime` until UI/skill dependencies are moved. |
| Selected ammo/reload | `currentShotsRemaining`, `shotCooldown`, `reloadRemaining` at `CombatRuntimeController.cs:336-338`; selected combat mutates at `CombatRuntimeProjectiles.cs:516-645` | Selected skill files and HUD | `SelectedUnitCombatState` | Phase 5 | Preserve current magazine/reload UI and selected monster skill behavior. |
| Selected monster definition/config | `selectedMonster`, selected ids/names/attribute/defenses at `CombatRuntimeController.cs:357-366`; stat/projectile config at `:376-386` | Selected fire logic, skill adapters, HUD/panel | `SelectedUnitLoadout` plus `SelectedUnitCombatState` | Phase 5 | Keep existing serialized/public access paths until UI and skill files migrate. |
| `selectedUnitRuntime` mirror | `selectedUnitRuntime` at `CombatRuntimeController.cs:369`; configured at `CombatRuntimeParty.cs:41-63`; synced at `:65-80` | Selected unit skills, shield mirror, target adapter candidate | Keep as selected visual/runtime bridge until `ICombatTarget` adapter exists | Phase 5, then Phase 7 | Do not make `CombatUnitRuntime` the only source of selected HP/shield until duplicated controller fields are removed together. |
| Manifested unit list | `manifestedMonsters` at `CombatRuntimeParty.cs:27` | Party config, enemy target priority, party combat, skill files | `ManifestedPartyRuntime` | Phase 2 | Preserve 2P-5P slot order and `PartyMonsterCount` behavior. |
| Manifested drone list | `manifestedDrones` at `CombatRuntimeParty.cs:28` | Party combat and clear logic | `ManifestedPartyRuntime` or battlefield drone service depending on effect type | Phase 2 or Phase 3 | Keep clear/despawn behavior tied to party clear until service boundary is explicit. |
| Manifested scene slots | `manifestedMonsterSlots` at `CombatRuntimeParty.cs:29` | Party create/clear/status view setup | `ManifestedPartyViewBinder`, not combat simulation | Phase 2 | Preserve scene child names `2PMonster` through `5PMonster`. |
| Manifested combat-unit state | `CombatUnitRuntime.cs:8-50` stores owner, monster/state refs, visuals, HP, shield, skills, and monster-specific timers | Party combat tick, skill files, damage absorption, labels | `CombatUnitRuntime` remains view/runtime bridge; combat state may later move behind target model | Phase 2, then Phase 7 | Do not move all HP/shield/status into `CombatTargetModel` before adapter proof. |
| Manifested skill runtime | `CombatSkillRuntime.cs:6-68`; populated in `CombatRuntimeParty.cs:2066-2104` | Party tick and monster skill files | Remain per-unit skill runtime, owned by `ManifestedPartyRuntime` after Phase 2 | Phase 2 | Keep cooldown/magazine fields unchanged during party split. |
| Monster runtime adapters | `CombatMonsterSkillRuntime.cs:21-38` stores full controller reference | Selected skill runtime dispatch and monster-specific files | Narrow interfaces for target query, damage, battlefield spawn, and temporary effects | Phase 6 | Do not start Phase 1 by rewriting all monster skill executors. |
| `nextProjectileSequence` | `CombatRuntimeController.cs:350`; selected and skill files increment before naming projectiles | Selected/default shots, monster projectile factories | Battlefield facade sequence service or projectile simulation | Phase 1 or Phase 3 | Preserve current object-name monotonic sequence if tests/logs rely on it. |
| Battle result flags | `battleResolved`, `victory`, reward flags at `CombatRuntimeController.cs:341-349` | Update early return, HUD, rewards, battle resolution | Stay in battle/session controller during combat simulation split | Later run-flow split, not Phase 1 | Do not mix battle-resolution migration into battlefield facade work. |

## Dependency Direction Target

Current dependency direction:

- `CombatRuntimeController` partials own state and call each other directly.
- Monster skill files access controller fields and battlefield lists directly.
- `CombatUnitRuntime.TickManifestedCombat()` calls back into controller through `Owner.TickManifestedUnitSkill`.

Target dependency direction after the full planned refactor:

- Controller owns top-level lifecycle and delegates to runtime services.
- Battlefield facade owns list access methods before list ownership moves.
- Enemy simulation owns enemy spawn/update and exposes target queries through narrow APIs.
- Manifested party runtime owns party units and skill ticking.
- Selected unit combat owns selected HP/shield/ammo/reload.
- Monster skill adapters depend on narrow interfaces, not full `CombatRuntimeController`.
- Common target/effect APIs are introduced after selected, enemy, and party state owners are stable.

## Phase 1 Handoff

Recommended first Code Builder slice:

1. Add a minimal internal battlefield facade/service owned by `CombatRuntimeController`.
2. Start with one write path only, preferably one `projectiles.Add(...)`, `skillEffects.Add(...)`, or `drones.Add(...)` call.
3. Keep underlying lists physically on the controller for the first slice if that minimizes risk.
4. Replace direct write with a request method such as `AddProjectileRuntime(...)` or `SpawnSkillEffectRuntime(...)`.
5. Do not change `Update()` order.
6. Do not move `EnemyRuntime`, `ProjectileRuntime`, `SkillEffectRuntime`, or `DroneRuntime` classes yet unless needed for compilation.
7. Run runtime/editor builds and `git diff --check`.

Phase 1 acceptance evidence should show:

- The changed file list.
- The exact direct list-write path that was migrated.
- Build evidence for `Pakuri\Assembly-CSharp.csproj`.
- Build evidence for `Pakuri\Assembly-CSharp-Editor.csproj`.
- `git diff --check` result.
- Board updates in `boards/REFACTORING/REFACTORING.md`, this file, and affected combat/projectile/monster boards.

Phase 1 completion evidence:

- Changed code files: `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs`, `CombatRuntimeEnemies.cs`, `CombatRuntimeParty.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeArielSkills.cs`, `CombatRuntimeEveSkills.cs`, `CombatRuntimeRinSkills.cs`, `CombatRuntimeSeinSkills.cs`, and `CombatRuntimeVegaSkills.cs`.
- Imported Unity asset: `Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs` with guid `101a3caea2a123d40ad484c072ede7b4`.
- `git diff --check` over changed code files completed with only LF-to-CRLF warnings.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP console warning/error read after import and script refresh returned only MCP-FOR-UNITY client handler logs.
