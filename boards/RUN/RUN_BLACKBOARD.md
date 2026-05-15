## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-09` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/RUN/RUN_BLACKBOARD.md`.

## Task: 2026-05-15 NewRunScene Stage-One Enemy Skill MVP

### Task title

Connect NewRunScene's three spawned stage-one enemies to their first skill behavior.

### Goals

- Keep the existing NewRunScene triple enemy spawn and roster registration path.
- Use the three authored enemy skill prefabs assigned on `GameManager` / `InGameCombatManager`.
- Let Warrior and Rogue damage the player monster through runtime relay/resource services.
- Let Priest heal injured enemy allies through the same resource refresh path.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode gameplay verification was run by Codex.
- This task does not implement Stage Flow, rewards, prisoner rewards, later enemy waves, or boss/midboss skills.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies in NewRunScene Play Mode that the already-spawned Warrior, Rogue, and Priest use their skill prefabs and mutate HP/heal values correctly.
- Stage Flow and prisoner reward work should start only after this enemy-skill MVP is accepted in Play Mode.

### Evidence

- `NewRunScene.unity` now serializes `warriorSkillPrefab`, `rogueSkillPrefab`, and `priestSkillPrefab` on `InGameCombatManager`.
- `EnemyCombatSimulationSystem.cs` keeps the existing movement/targeting/cooldown loop and calls enemy skill execution from that point.
- `Warrior_Skill.prefab`, `Achor_Skill.prefab`, and `Preist_Skill.prefab` under `Assets/Prefab/Enemy/Skill` are the assigned visual/trigger prefabs for the three current enemies.
- `InGameEnemySkillHitboxActor.cs` relays Warrior slash hits and `InGameProjectileActor.cs` relays Rogue shuriken hits into `InGameCombatManager.ApplyDamage(...)`.
- `UnitResourceMutationService.cs` and `InGameCombatManager.cs` now expose `Heal(...)` for Priest healing and actor refresh.
- Runtime/editor builds passed with 0 errors after rerunning the Editor build alone because the first parallel attempt hit the known output DLL file lock.
- Unity-MCP console warning/error read showed no C# compile errors after the new script import.

### History

- 2026-05-15: User requested Code Builder implementation of the three current enemy skills using prefabs from `Assets/Prefab/Enemy/Skill`.

## Task: 2026-05-15 NewRunScene Phase4-C Damage Visibility Follow-up

### Task title

Record NewRunScene projectile hit HP display and deletion fix.

### Goals

- Fix the Phase4-C projectile-hit path so HP decrease is visible as rounded whole-number HP.
- Delete dead enemy/monster Actor GameObjects through the combat manager after resource mutation reports death.
- Make HPBar `Fill` decrease as a left-anchored slide rather than shrinking from both sides.
- Display hit damage through the prefab `Damage` TextMesh, rising about 1 local Y unit and fading out.
- Keep the left-anchored `Fill` calculation inside the actual rendered `Background` sprite bounds.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode gameplay verification was run by Codex.
- This follow-up does not implement new skills, wave logic, reward logic, or timed status expiry.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies NewRunScene Play Mode: projectile hit reduces enemy HP, HP Fill remains visually inside the Background, and enemies are removed at 0 HP.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` was inspected and already calls `combatManager.ApplyDamage(...)` on valid enemy hits.
- `Pakuri/Assets/Scripts2/InGame/Core/UnitResourceMutationService.cs` now rounds defense-adjusted damage and stored resource values.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now unregisters and destroys a unit Actor when `ApplyDamage(...)` returns `IsDead`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls `ShowDamageIfChanged(result)` and delays dead Actor destruction by `0.95f` seconds after immediate roster unregister.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` and `MonsterUnitActor.cs` now use left-anchored HP `Fill` positioning so the left edge stays fixed while the right edge shrinks.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` and `MonsterUnitActor.cs` now resolve prefab `Damage` TextMesh children and expose `ShowDamage(...)`.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` and `MonsterUnitActor.cs` now compute HP/Shield segment positions from SpriteRenderer rendered width, using `sprite.bounds.size.x * localScale.x`, so movement is based on the visible `Background` width rather than raw scale values.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` contains the `InGameDamageTextPopup` helper used by both Actor files; it displays `N(Damage)`, rises by `1f` local Y over `0.9f` seconds, and fades out.
- Prefab inspection found `Damage` TextMesh children in all current monster and enemy unit prefabs before the code change.
- Runtime/editor builds passed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP refresh/console evidence showed no remaining C# compile errors after moving the popup helper into the already-compiled Actor file.
- Follow-up runtime/editor builds passed with 0 errors after switching segment math to SpriteRenderer rendered-width units and changing damage text format to `N(Damage)`.
- Unity-MCP script refresh reached idle and console warning/error read showed only MCP client handler logs after the follow-up.
- `git diff --check` on the changed scripts passed with only LF-to-CRLF normalization warnings.

### History

- 2026-05-15: User reported projectile firing works, but HP decrease, monster deletion, and enemy HPBar Fill position were broken after hits.
- 2026-05-15: Code Builder fixed rounded HP mutation, death cleanup, and Fill coordinate stabilization.
- 2026-05-15: User clarified that HP should slide down from left to right and requested Damage Text feedback; Code Builder added prefab `Damage` Text popup animation and left-anchored HP Fill shrink.
- 2026-05-15: User reported Fill still escaped BG and requested `number(Damage)` text format; Code Builder changed segment math to actual rendered sprite width and changed popup text to `N(Damage)`.

## Task: 2026-05-15 NewRunScene Phase4-C-0 Skill Actor Minimum Execution

### Task title

Record the first NewRunScene Phase4-C skill effect execution slice.

### Goals

- Move Phase4 from no-effect executor contracts into minimum visible/effective execution for sample skills.
- Connect Eve-A projectile prefab spawning, movement, damage relay, and spawn-point X destruction.
- Connect Ariel-B shield grant and attached visual prefab spawning.
- Connect 1P A manual mouse firing and `Canvas/AutoBtn` auto-route toggle.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode gameplay verification was run by Codex.
- Full 2P-5P party spawning is not implemented by this slice; non-first roster entries will auto-route if present.
- Eve-A shock, branch lightning, broad skill data expansion, projectile fan-out, and Ariel-B timed shield expiry remain pending.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified by compile/editor checks.

### Next Actions

- User verifies NewRunScene Play Mode: Eve-A manual hold fire, AutoBtn auto toggle, projectile hit/destroy behavior, and Ariel-B when learned.
- Add a timed status/effect layer before claiming shield duration or shock duration behavior.
- Add reusable branch/multi-projectile behavior after the base projectile actor path is accepted.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- Added `InGameProjectileActor.cs`, `InGameAttachedSkillEffectActor.cs`, and `InGameAutoSkillButton.cs` under `Pakuri/Assets/Scripts2/InGame/Skills/Execution`.
- `SkillExecutionSystem.cs` now supports `TryExecuteManual(...)` and an auto-route predicate.
- `SkillExecutionContext.cs` now carries optional manual aim direction.
- `SkillExecutors.cs` now executes projectile and shield sample effects instead of returning no-effect results.
- `InGameCombatManager.cs` now resolves skill effect prefabs, handles 1P manual A input, exposes AutoBtn manual-to-auto switching, and resolves projectile destroy boundary X.
- `NewRunScene.unity` serializes `eveAProjectilePrefab`, `arielBShieldEffectPrefab`, `projectileDestroyBoundary`, and `Canvas/AutoBtn`'s `InGameAutoSkillButton` reference.
- Runtime/editor builds passed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle and console warning/error read showed no C# compile errors.
- 2026-05-15 follow-up: `InGameCombatManager.cs` no longer calls `UnityEngine.Input.GetMouseButton(0)` or `Input.mousePosition`; manual 1P A input now uses `UnityEngine.InputSystem.Mouse.current`.
- 2026-05-15 follow-up: Runtime/editor builds passed with 0 errors after the Input System API replacement, and Unity-MCP script refresh reached idle.

### History

- 2026-05-15: User requested Code Builder to implement Phase4-C-0 as a common actor component slice connected to Eve-A and Ariel-B minimum execution.
- 2026-05-15: User reported the NewRunScene Play Mode error caused by `UnityEngine.Input` while the project uses the Input System package; Builder replaced the manual A-skill mouse read with Input System API.

## Task: 2026-05-15 NewRunScene Phase4-B Skill Execution Contract

### Task title

Record Phase4-B skill execution contract, registry, choice snapshot, and NewRunScene wiring.

### Goals

- Add a skill execution system that routes learned active skill runtime instances through type-based executors.
- Build execution snapshots from unit chosen choice IDs without mutating source `SkillData`.
- Connect NewRunScene-selected monster models to learned active skill runtime instances.
- Keep actual damage, shield, status, projectile prefab creation, trigger relay, pierce, duplicate-hit, and tick-hit behavior out of Phase4-B.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode verification by Codex.
- Code Reviewer execution requires explicit user permission and was not run.
- Phase4-B executors are no-effect contract executors only.
- The Unity-MCP `execute_code` construction check remained blocked by the known Windows mono path-length issue.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Phase4-C should connect minimum sample effects such as `eve-a` damage and `ariel-b` shield through `InGameCombatManager.ApplyDamage(...)` and `GrantShield(...)`.
- Later projectile/beam/zone work should add trigger relay/runtime behavior without putting hit logic in prefabs.
- User performs Play Mode verification only after actual Phase4-C effects are connected.

### Evidence

- Added execution contract files under `Pakuri/Assets/Scripts2/InGame/Skills/Execution`.
- `InGameCombatManager.cs` now owns a `SkillExecutionSystem`, ticks it from `Update()`, exposes routed/rejected counts, and parses an optional `skillChoiceModifierCsv` TextAsset.
- `NewRunSceneEntryManager.cs` now calls `SkillRuntimeFactory.RebuildLearnedActiveSet(...)` after creating the selected monster model.
- `InGameTestDataManager.cs` also rebuilds learned active skill runtime for its loaded sample monster model.
- `SkillRuntimeInstance.CanCast` now respects positive `Timing.TickInterval` as a cast interval gate.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` stores `skillExecutionEnabled: 1`, `logSkillExecutionContracts: 0`, and `skillChoiceModifierCsv` referencing `SkillChoiceModifierData.csv` GUID `6c4e1bb3fa254e02a749fb55f6d685d7`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle after script import.
- Unity-MCP console warning/error read showed only the known existing CSV auto-sync warning and MCP client handler logs.

### History

- 2026-05-15: User directed Code Builder to start Phase4-B after Eve-A choice modifier CSV seed creation.

## Task: 2026-05-15 NewRunScene Phase4-A Skill Runtime State

### Task title

Record Phase4-A learned active skill runtime state creation.

### Goals

- Add unit-owned skill runtime storage for learned active skills.
- Keep `SkillData` as immutable blueprint data during combat.
- Track cooldown, cast, active duration, tick interval, magazine, and reload state per runtime instance.
- Do not execute projectiles, damage, shield grants, targeting, or skill effects in Phase4-A.

### Constraints

- Role Owner is Code Builder.
- No Play Mode verification by Codex.
- `SkillRuntimeFactory` activates only skills already present in `MonsterUnitRuntimeModel.State.LearnedActiveSkillIds`.
- Actual skill executor, target query, projectile creation, `ApplyDamage(...)`, and `GrantShield(...)` remain Phase4-B/C work.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Phase4-B should add executor interfaces/registry and choice-resolution snapshots.
- Phase4-C should connect minimum sample effects such as `eve-a` damage and `ariel-b` shield through the Phase3-C resource mutation APIs.
- User performs Play Mode verification only after skill execution is connected.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` for cooldown, cast, active duration, tick interval, magazine, and reload state.
- Added `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/UnitSkillRuntimeSet.cs` for unit-owned active skill runtime storage, lookup, and ticking.
- Added `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeFactory.cs` to build learned active skill runtime instances from `InGameSkillCatalog` and `UnitStateBucket.LearnedActiveSkillIds`.
- Updated `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` with `SkillRuntime`.
- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html` to mark Phase4-A complete.
- Runtime and editor builds passed with 0 errors and existing assembly reference warnings.
- Unity-MCP force refresh cleared the initial missing new-script import error; console read then showed only MCP client handler logs.
- Unity-MCP `execute_code` runtime construction check failed with the known Windows mono path-length error, not a C# compile error.

### History

- 2026-05-15: User explicitly requested Code Builder to perform Phase4-A work.

## Task: 2026-05-15 NewRunScene Phase3-C Resource Pipeline

### Task title

Record NewRunScene Phase3-C resource mutation and actor refresh pipeline.

### Goals

- Keep the current NewRunScene Phase3-B enemy movement and attack-attempt loop intact.
- Add a manager-owned route for future skill systems to apply damage or shields to registered units.
- Refresh HP/Shield actor visuals only when a registered unit resource changes.
- Avoid declaring Play Mode HP loss or death behavior complete before monster and enemy skills exist.

### Constraints

- Role Owner is Code Builder.
- No Play Mode verification by Codex.
- No monster skill, enemy skill, actual attack damage connection, death handling, or reward handling was implemented.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User can continue using Play Mode to verify Phase3-B movement/attack attempt behavior.
- Later skill implementation should call `InGameCombatManager` resource APIs and then verify HP/Shield decrease and death behavior in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` owns a `UnitResourceMutationService` and exposes `ApplyDamage(...)`, `GrantShield(...)`, `SetShield(...)`, and `RefreshUnitActor(...)`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitRosterService.cs` now exposes `Find(BaseUnitRuntimeModel model)` so the manager can refresh the changed registered actor.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `EnemyUnitActor.cs` update HP and Shield segments from the same `Background` width.
- `EnemyCombatSimulationSystem.cs` remains attack-attempt only and does not call the new damage API.
- Runtime and editor builds passed with 0 errors and existing warnings.
- Unity-MCP script refresh reached idle after the script changes.
- Unity-MCP console read showed only MCP client handler logs and the expected MCP `execute_code` path-length failure from the attempted non-PlayMode calculation check, not C# compile errors.

### History

- 2026-05-15: User stated Phase3-B was implemented and Play Mode verified, then directed Code Builder to implement Phase3-C only as a resource pipeline because skills are not implemented yet.

## Task: 2026-05-15 NewRunScene Enemy Combat Attempt Loop

### Task title

Record NewRunScene activation of the first enemy movement/target/attack-attempt loop.

### Goals

- Keep NewRunScene entry spawning intact.
- Enable the `InGameCombatManager` enemy combat simulation loop on `GameManager`.
- Leave attack-attempt logs disabled by default so 100+ enemy tests do not spam the console.

### Constraints

- Role Owner is Code Builder.
- No Play Mode verification from Codex.
- This is not yet damage, HP reduction, ranged projectile, support heal/shield/buff, or skill execution.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies in Play Mode that NewRunScene enemies move toward the selected monster and stop/attempt attacks in range.
- If visual debugging is needed, set `logEnemyAttackAttempts` on `InGameCombatManager` to true temporarily.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now owns an `EnemyCombatSimulationSystem` instance and ticks it from `Update()`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now stores `enemyCombatSimulationEnabled: 1` and `logEnemyAttackAttempts: 0` on `InGameCombatManager`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.
- Unity-MCP console initially showed a new-script import error for `EnemyCombatSimulationSystem`, then a force refresh completed and later console reads no longer showed that compile error.
- Unity-MCP `execute_code` edit-mode simulation verification failed because MCP's mono command hit a Windows path-length error, so gameplay behavior still needs user Play Mode verification.

### History

- 2026-05-15: User requested Code Builder to implement enemy movement, targeting, and basic attack "attempt".

## Task: 2026-05-15 NewRunScene Triple Enemy Entry Spawn

### Task title

Spawn the three current stage-one enemy prefabs during NewRunScene entry.

### Goals

- Keep selected player monster spawning intact.
- Spawn stage-one Warrior, Rogue, and Priest enemy units after NewRunScene entry.
- Space the three enemy spawns by one second.
- Preserve the authored enemy spawn rule: X from `SpawnPoint`, Y from `-5` to `5`.

### Constraints

- Role Owner is Code Builder.
- This is entry spawning only, not wave cadence, movement, targeting, attacks, damage, or skill execution.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies NewRunScene in Play Mode: selected monster spawns, then Warrior, Rogue, and Priest spawn one second apart at `SpawnPoint.x` with randomized Y in `-5~5`.
- Later Phase3-B should move from entry-only spawning into roster-driven enemy simulation systems.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` now starts `SpawnInitialEnemySequence()` after `SpawnSelectedPlayerUnit()`.
- `SpawnInitialEnemySequence()` calls `SpawnInitialEnemyUnit()`, waits `enemySpawnIntervalSeconds`, calls `SpawnRangedEnemyUnit()`, waits again, and calls `SpawnBufferEnemyUnit()`.
- `TrySpawnEnemyUnit(...)` creates an `EnemyUnitRuntimeModel`, instantiates the configured prefab, binds `EnemyUnitActor`, and registers the model/actor with `InGameCombatManager`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` stores `initialEnemyId: stage1-swordsman`, `rangedEnemyId: stage1-rogue`, `bufferEnemyId: stage1-priest`, `enemySpawnMinY: -5`, `enemySpawnMaxY: 5`, and `enemySpawnIntervalSeconds: 1`.
- Unity-MCP scene save reported `Scene 'NewRunScene' saved successfully to 'Assets/Scenes/NewScene/NewRunScene.unity'`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.
- Unity-MCP console warning/error read showed only MCP client handler logs after the scene save.

### History

- 2026-05-15: User requested NewRunScene entry spawning for the three current enemy types after adding the prefabs under `Assets/Prefab/Enemy`.

## Task: 2026-05-15 NewRunScene Phase3-A Combat Manager Registration

### Task title

Record NewRunScene entry registration into the Phase3-A combat manager roster.

### Goals

- Keep the current NewRunScene selected monster and first enemy spawn path intact.
- Add an explicit `InGameCombatManager` component to `GameManager`.
- Register the spawned selected monster and spawned enemy with the combat manager roster after Actor/model binding.

### Constraints

- Role Owner is Code Builder.
- No Play Mode verification; user verifies actual spawned visuals and runtime gameplay.
- Do not change NewMainMenu selection flow or legacy `RunSceneBootstrap` production path in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies NewRunScene in Play Mode: selected monster and first enemy still spawn after `GameManager` gained `InGameCombatManager`.
- Later Phase3-B work should consume `InGameCombatManager.Roster` for movement/targeting/basic attack rather than searching the scene.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` now has a serialized `combatManager` field and `CombatManager` property.
- `NewRunSceneEntryManager.cs` now calls `RegisterSpawnedPlayer()` after selected monster Actor binding and `RegisterSpawnedEnemy()` after enemy Actor binding.
- `NewRunSceneEntryManager.cs` uses `ResolveCombatManager()` to get or add `InGameCombatManager` on the same `GameManager`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now contains an `InGameCombatManager` component on `GameManager`, and `NewRunSceneEntryManager.combatManager` references that component.
- Unity-MCP component read confirmed the scene component state and serialized reference.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-15: Code Builder implemented Phase3-A combat manager roster registration after the user requested Phase3-A work.

## Task: 2026-05-15 NewRunScene Enemy Spawn Entry Handoff

### Task title

Record the NewRunScene entry-side requirements for first enemy spawning.

### Goals

- Extend the current NewRunScene entry flow after selected 1P monster spawn so one `stage1-swordsman` enemy can be spawned from the authored enemy `SpawnPoint`.
- Preserve the current selected-monster flow and existing `NewRunSceneEntryManager` 1P prefab binding behavior.

### Constraints

- Role Owner is Designer for this handoff; no scene or code changes were made.
- `NewRunSceneEntryManager` currently owns 1P selected monster spawning; no active enemy spawn field or enemy spawn method exists in the inspected file.
- Do not change NewMainMenu selection flow or current RunSession handoff in this slice.

### Role Owner

Designer -> Code Builder

### Status

Builder implementation completed and locally verified. Phase2-B entry-side spawn/model/actor-binding scope is complete.

### Next Actions

- User verifies NewRunScene in Play Mode: selected 1P monster still spawns, one enemy spawns from the authored `SpawnPoint` X, and enemy Y is in the -5 to +5 range.
- Later combat work should move beyond entry spawning into enemy movement, target search, attack timing, and HP mutation.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs:39` defines `SpawnSelectedPlayerUnit()`.
- `NewRunSceneEntryManager.cs:70` through `:75` instantiates the selected monster prefab at `playerSpawnPoint` and binds the spawned actor.
- `NewRunSceneEntryManager.cs:194` through `:204` resolves `1PSpawnPoint` by name only when the serialized field is missing.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:5` currently defines an empty `InGameCombatManager`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:677` through `:685` shows `GameManager` has `NewRunSceneEntryManager` with `playerSpawnPoint`, `fallbackCatalog`, and five monster prefab fields, but no enemy spawn/prefab fields in the current component.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:924` contains the authored `SpawnPoint`.
- Code Builder kept the temporary Phase2-B enemy entry spawn in `NewRunSceneEntryManager` rather than activating the currently empty `InGameCombatManager`.
- `NewRunSceneEntryManager.cs:46` calls `SpawnInitialEnemyUnit()` after `SpawnSelectedPlayerUnit()`, and `:90` defines the one-enemy spawn method.
- `NewRunSceneEntryManager.cs:113` uses `UnityEngine.Random.Range(enemySpawnMinY, enemySpawnMaxY)` while preserving X from `enemySpawnPoint.position`.
- `NewRunSceneEntryManager.cs:316` through `:325` falls back to `GameObject.Find("SpawnPoint")` if the serialized spawn point is missing.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:679`, `:686`, and `:687` through `:689` show the saved enemy spawn point, enemy prefab, enemy ID, and Y-range fields.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.
- Unity-MCP `refresh_unity` reached idle after external file sync; final editor state reported `ready_for_tools=true`, `is_playing=false`, and no asset update in progress.

### History

- 2026-05-15: Designer inspected the current NewRunScene entry path and recorded the enemy spawn handoff scope after the user finished assigning the enemy prefab and spawn point.
- 2026-05-15: Code Builder implemented the enemy entry spawn in `NewRunSceneEntryManager`, connected scene fields, and verified build/editor state without running Play Mode.

## Task: 2026-05-14 Five Monster NewRunScene Prefab Binding Fix

### Task title

Finish five-monster prefab binding and HP bar visibility for NewRunScene entry.

### Goals

- Remove the trailing whitespace that caused the previous Code Reviewer failure.
- Bind Ariel, Eve, Sein, Vega, and Rin prefabs on `NewRunSceneEntryManager`.
- Make the prefab `MonsterHpBar` render by assigning a real sprite to HP bar renderers.
- Verify all five selected monster IDs can create a model and initialize `MonsterUnitActor`.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode gameplay verification; user owns click-flow and visual Play Mode checks.
- Keep current UI flow unchanged.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies the five monster selection flow in Play Mode.
- Later combat work should update model resources and call `MonsterUnitActor.RefreshDebugView()` after HP/shield changes.

### Evidence

- Updated `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` to assign `fallbackCatalog` and all five prefab fields: `arielUnitPrefab`, `eveUnitPrefab`, `rinUnitPrefab`, `seinUnitPrefab`, and `vegaUnitPrefab`.
- Added `Pakuri/Assets/Prefab/Monster/MonsterHpBarPixel.png` and assigned it to `Background`, `Fill`, and `Shield` SpriteRenderers in all five monster unit prefabs.
- Updated `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` so fallback catalog data can be used before CSV runtime initialization and monster resolution requires exact ID matches.
- Unity-MCP verification returned `modelOk=True`, matching `model=ariel/eve/sein/vega/rin`, `actorModel=True`, `bgSprite=True`, `fillSprite=True`, and `shieldSprite=True` for all five IDs.
- `git diff --check` over the changed scene, prefabs, and entry scripts completed with exit code 0 and only LF-to-CRLF warnings.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.
- 2026-05-14 follow-up: User manually adjusted the five monster prefab `MonsterHpBar` Scale values. `Select-String` evidence found `MonsterHpBar` root scales around `{x: 3.3, y: 1.7, z: 1.35}` and `Background` / `Fill` scales of `{x: 20, y: 2.5, z: 1}` in the monster prefabs.
- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html` to state that `MonsterHpBar` Scale and visible size are user-authored prefab responsibility, and Code Builder must not overwrite them in runtime code.
- 2026-05-14 follow-up: Updated `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` so HP and shield fill X scale use `Background.localScale.x * normalizedValue` instead of writing the normalized value directly.
- Unity-MCP editor code verified all five prefabs: `Ariel_Unit`, `Eve_Unit`, `Sein_Unit`, `Vega_Unit`, and `Rin_Unit` returned `bgX=20`, `fullFillX=20`, and `halfFillX=10`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and a follow-up single `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: User asked Code Builder to remove trailing whitespace, verify five prefab Actor/Model binding, and fix invisible `MonsterHpBar`.
- 2026-05-14: User said they directly modified `MonsterHpBar` Scale and asked to update the InGame build roadmap report.
- 2026-05-14: User reported that `HpFill` was forced to `1` when entering from NewMainScene to NewGameScene and asked Code Builder to make it match the `Background` Scale.

## Task: 2026-05-14 NewRunScene Phase2-B Actor Model Binding

### Task title

Bind the selected 1P monster prefab actor to a runtime unit model on NewRunScene entry.

### Goals

- Create the selected monster `MonsterUnitRuntimeModel` from the current CSV/Data runtime catalog during `NewRunScene` entry.
- Spawn the selected monster prefab at `1PSpawnPoint`.
- Inject the created model into the spawned prefab's `MonsterUnitActor`.

### Constraints

- Role Owner is Code Builder.
- The user already added Eve prefab HP/name debug children and `MonsterUnitActor`; this task does not redesign UI.
- `MonsterUnitRuntimeModel` is a plain runtime C# model and is not a prefab-inspector component.
- Code Reviewer was not run because the user did not explicitly request Reviewer execution for this slice.
- Do not run Unity Play Mode; user verifies gameplay flow.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User manually assigns Ariel/Sein/Vega/Rin prefab fields on `NewRunSceneEntryManager` when ready.
- Connect runtime HP/resource mutation from the combat loop so `MonsterUnitActor.RefreshDebugView()` reflects live damage and shield changes.
- User verifies NewMainMenu monster selection into NewRunScene in Play Mode.

### Evidence

- Updated `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` so selected monster ID resolves through `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)`, `PakuriDataManager.Instance.ResolveMonster(...)`, `RunSession.Begin(...)`, and `UnitFactory.CreateSelectedMonster(...)`.
- Updated `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` with `Initialize(MonsterUnitRuntimeModel)`, model storage, child reference resolution, and debug HP/name refresh.
- Unity-MCP editor code execution returned `manager=True|spawn=1PSpawnPoint|eveScenePrefab=Eve_Unit|actor=True|initialize=True|refresh=True|nameLabel=True|hpLabel=True|hpFill=True|shieldFill=True|modelMonster=eve|modelHp=220|learnedA=1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-14: User said Eve prefab now has HP SlideBar, debug HP/name labels, and `MonsterUnitActor`, then requested Code Builder to start Phase2-B.

## Task: 2026-05-14 NewMainMenu To NewRunScene Entry Implementation

### Task title

Implement selected monster handoff and first NewRunScene 1P prefab spawn.

### Goals

- Carry selected monster ID from `NewMainMenu` to `NewRunScene`.
- Include `NewMainMenu` and `NewRunScene` in Build Settings.
- Spawn `Assets/Prefab/Monster/Eve_Unit.prefab` at `1PSpawnPoint` when Eve is selected or when fallback is allowed.

### Constraints

- Role Owner is Code Builder.
- Only Eve prefab spawning is implemented because `Assets/Prefab/Monster` currently contains only `Eve_Unit.prefab`.
- Non-Eve selections are stored but currently log that no prefab is configured.
- Code Reviewer was explicitly skipped by the user for this task.
- Do not run Unity Play Mode; user verifies gameplay flow.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Add Ariel/Sein/Vega/Rin prefab bindings when their unit prefabs exist.
- Continue Phase2-B by binding spawned 1P prefab/model to the InGame unit actor/runtime model instead of only spawning the visual shell.
- User verifies the NewMainMenu -> NewRunScene click flow in Play Mode.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Core/NewRunStartContext.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs`.
- Updated `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` so `GameManager` has `NewRunSceneEntryManager`, `playerSpawnPoint` references `1PSpawnPoint`, and `eveUnitPrefab` references `Assets/Prefab/Monster/Eve_Unit.prefab`.
- Updated `Pakuri/ProjectSettings/EditorBuildSettings.asset` through Unity-MCP so `Assets/Scenes/NewScene/NewMainMenu.unity` and `Assets/Scenes/NewScene/NewRunScene.unity` are enabled build scenes.
- Unity-MCP read-only code returned `gameManager=True|entry=True|spawn=1PSpawnPoint|prefab=Eve_Unit|scene=Assets/Scenes/NewScene/NewRunScene.unity`.
- Runtime and editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: User explicitly requested Code Builder to pass selected monster entry data to NewRunScene and spawn the Eve shell prefab at 1PSpawnPoint.

## Task: 2026-05-14 NewMainMenu To NewRunScene Loading Timing

### Task title

Record when the NewMainMenu selection handoff should be applied.

### Goals

- Connect selected monster data before deeper `NewRunScene` actor/prefab binding.
- Preserve current `RunStartContext` / `RunSession` ownership pattern unless replaced by a later CSVData/InGame loader task.
- Keep `NewRunScene` entry ID-based so CSVData and prefab binding can evolve without rewriting UI.

### Constraints

- Role Owner is Designer.
- No Run flow code or scene change in this design note.
- UI implementation should not own combat spawning; it should only choose a monster, prepare run context, and load the scene.

### Role Owner

Designer

### Status

Ready for Code Builder handoff.

### Next Actions

- Implement `UIManager.cs` handoff before Phase2-B actor binding, because Phase2-B needs a real selected 1P monster input.
- After handoff, Phase2-B should consume the selected monster at `NewRunScene` entry and bind it to `1PSpawnPoint`.
- Later CSVData Phase3~5 can replace the data lookup source while keeping the same selected-monster ID handoff contract.

### Evidence

- `Pakuri/Assets/Legacy/Scripts/Run/Flow/RunStartContext.cs` stores `SelectedMonster`, `Session`, and `HasPendingRun`.
- `Pakuri/Assets/Legacy/Scripts/Run/Flow/RunSceneBootstrap.cs` reads `RunStartContext.Instance` and falls back only when no pending run exists.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` already has a selected monster creation path and currently defaults the Phase2-A sample to `eve`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains `1PSpawnPoint`, `GameManager`, and `Nexus`.

### History

- 2026-05-14: User asked where to place the NewMainMenu UI flow, monster selection, and NewRunScene loading work in the next implementation order.

## Task: 2026-05-14 Combat V2 Final Run Integration Target

### Task title

Record the completed Run-to-Combat V2 integration structure.

### Goals

- Preserve MainMenuScene / `RunStartContext` / `RunSession` data timing until Combat V2 is ready to replace the old combat entry.
- Define how final V2 ingame flow receives selected monster, session state, party state, learned choices, and catalog data.
- Keep Run UI integration deferred until a minimum V2 combat loop is stable.

### Constraints

- Role Owner is Designer.
- No Run flow implementation, UI wiring, scene edit, or Play Mode verification in this task.
- Existing `RunSceneBootstrap` still starts the old `CombatRuntimeController` path until a later Code Builder task changes it.

### Role Owner

Designer

### Status

Completed as Run integration architecture context.

### Next Actions

- Future Code Builder work should introduce a V2 scene/bootstrap handoff only after Phase1-D validation and Phase2-A unit mapping are implemented.
- When production flow is rewired, preserve `RunStartContext` and `RunSession` ownership of selected monster and learned-state data.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html`.
- `Pakuri/Assets/Scripts/Run/Flow/RunSceneBootstrap.cs` currently resolves the catalog and calls `CombatRuntimeController.BeginConfiguredDay(monster, session, fallbackCatalog)`.
- `Pakuri/Assets/Scripts/Run/Flow/RunStartContext.cs` stores `SelectedMonster`, `Session`, and `HasPendingRun`.
- `Pakuri/Assets/Scripts/Run/Session/RunSession.cs` stores selected monster ID/name, learned active/passive IDs, chosen rewards, manifested monster IDs, and `PartyMembers`.
- Scene YAML confirms `NewRunScene` contains `GameManager`, `1PSpawnPoint` through `5PSpawnPoint`, and `Nexus`.

### History

- 2026-05-14: Designer documented the final Run integration target for completed Combat V2 ingame flow.

## Task: 2026-05-14 NewRunScene Run And Combat Scene Contract

### Task title

Record `NewRunScene` as the intended in-game scene for future Run/Combat V2 flow.

### Goals

- Treat `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` as the scene used for Combat V2 tests and the future main in-game scene.
- Preserve the current MainMenuScene / RunStartContext production data handoff until explicitly rewired.
- Record scene object roles for future RunScene integration.

### Constraints

- Role Owner is Designer.
- No Run flow, scene, UI, or bootstrap code changes in this task.
- Existing `RunSceneBootstrap` and current runtime flow remain unchanged until a Code Builder task rewires them.

### Role Owner

Designer

### Status

Recorded as Run/scene context.

### Next Actions

- Future Code Builder work should connect Combat V2 to `NewRunScene` only after the current test-only data bootstrap and sample skill bridge are validated.
- When production flow is wired, preserve MainMenuScene selection and `RunStartContext` data timing unless the user explicitly changes it.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` exists.
- Scene YAML contains `BG`, `1PSpawnPoint`, `2PSpawnPoint`, `3PSpawnPoint`, `4PSpawnPoint`, `5PSpawnPoint`, `GameManager`, and `Nexus`.
- User stated that `NewRunScene` is both the test scene and intended final in-game scene.
- User stated that `1P~5PSpawnPoint` are the player and manifested monster spawn points.
- User stated that `GameManager` is for the game's core logic and `Nexus` is the nexus.

### History

- 2026-05-14: User clarified the intended role of existing objects in `NewRunScene`.

## Task: 2026-05-13 Combat V2 RunSession Compatibility Note

### Task title

Record Run-domain compatibility decisions for Combat V2.

### Goals

- Keep current Run UI Flow and `RunSession` data ownership while new combat runtime is built.
- Store skill enhancement/learned-choice state on unit state and let Combat V2 read it during skill execution.
- Defer `RunCombatUiController` integration until a minimal new combat loop exists.

### Constraints

- Role Owner is Designer.
- Do not implement UI or Run flow changes in this task.
- Existing RunScene remains current until a later Code Builder task wires Combat V2.

### Role Owner

Designer

### Status

Completed as design context.

### Next Actions

- Combat V2 implementation should consume `RunSession` and `RunSession.RunMonsterState` without changing Run flow first.
- UI integration should be designed only after Combat V2 can run a minimal battle independently.

### Evidence

- Added `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html`.
- `Pakuri/Assets/Scripts/Run/Flow/RunSceneBootstrap.cs:53` through `:57` shows the current combat entry receives `MonsterDefinition`, `RunSession`, and `GameDataCatalog`.
- `Pakuri/Assets/Scripts/Run/Session/RunSession.cs` stores `SelectedMonsterId`, learned actives/passives, party members, manifested monster records, and per-monster learned state.
- User confirmed that learned choices should remain stored on unit state and be queried at skill execution time.

### History

- 2026-05-13: User decided not to implement UI integration yet and to keep learned-choice storage on unit state for Combat V2.

## Task: 2026-05-09 Assets Scripts Folder Organization

### Task title

Organize Run scripts under Flow, Session, and UI subfolders.

### Goals

- Make the Run script structure easier to scan from the folder tree.
- Keep run behavior unchanged by moving files only, with `.cs.meta` files moved together.

### Constraints

- Role Owner is Designer -> Code Builder.
- Do not change C# class names, namespaces, serialized field names, or gameplay logic.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Designer -> Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Use `Pakuri/Assets/Scripts/Run/Flow`, `Session`, and `UI` as the current Run script map.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Added design document `Pakuri/reference/Report/2026-05-09-assets-scripts-folder-organization-design.md`.
- Moved `MainMenuFlowController.cs`, `RunFlowController.cs`, `RunFlowState.cs`, `RunSceneBootstrap.cs`, and `RunStartContext.cs` to `Pakuri/Assets/Scripts/Run/Flow`.
- Moved `RunDayModel.cs` and `RunSession.cs` to `Pakuri/Assets/Scripts/Run/Session`.
- Moved `DebugSceneController.cs` and `RunCombatUiController.cs` to `Pakuri/Assets/Scripts/Run/UI`.
- Moved `.cs.meta` files with their matching `.cs` files to preserve Unity script GUIDs.
- Unity-MCP `refresh_unity` reached idle after script refresh.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings after rerunning it alone; the earlier parallel editor build failed only because the runtime build held an `obj\Debug` cache file lock.
- Unity-MCP console warning/error read returned only MCP client handler logs.

### History

- 2026-05-09: User requested organizing `Assets/Scripts` so Run and other domains are clearer from the folder structure.
