Archived snapshot created during 2026-05-18 board cleanup.

# ENEMY_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Task: 2026-05-18 Stage1 Enemy Dual Skill Runtime

### Task title

Make Stage 1 enemies execute CSV-authored basic and special skills through the current enemy combat simulation.

### Goals

- Support one basic skill plus one special `stage_one_skill` per Stage 1 enemy.
- Keep cooldown-driven support skills (`Heal`, `ShieldUp`, `GuardianFlag`, `ChargeCommand`) working while basic attacks still exist.
- Let offensive specials such as `ShurikenThrow` and `SacredSwordWave` coexist with a separate basic attack.
- Keep duplicate basic/special pairs as one effective runtime skill.

### Constraints

- Role Owner is Code Builder.
- No Play Mode verification was run by Codex.
- Code Reviewer execution was explicitly disallowed by the user.
- Scene effect authority stayed in `EffectManager`; this task did not move skill effect ownership back into CSV.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- User verifies actual in-game cadence and feel for dual-skill enemies in NewRunScene Play Mode.
- If special/basic priority needs different pacing later, adjust `EnemyCombatSimulationSystem.ResolvePreferredOffensiveSkill(...)` with fresh gameplay evidence instead of hardcoding per-enemy exceptions.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs` no longer assumes one `StageOneSkill` cooldown path only; it now tracks separate basic/special cooldowns, resolves support vs offensive skills, and executes effect prefabs by the explicit skill kind being fired.
- `EnemyCombatState` now stores `BasicSkillCooldownRemaining` and `SpecialSkillCooldownRemaining`.
- `EnemyCombatSimulationSystem` now uses `EffectManager.ResolveEnemySkillEffectPrefab(enemyModel, skillKind)` so one enemy can map different prefabs for different fired skills.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` was updated through Unity-MCP so `EffectManager` enemy groups now include the extra basic-skill entries:
  `stage1-shieldbearer=>[ShieldUp:Shield_Skill,Slash:Warrior_Skill]`
  `stage1-rogue=>[ShurikenThrow:Rogue_Skill,AimedShot:Achor_Skill]`
  `stage1-priest=>[Heal:Preist_Skill,AimedShot:Achor_Skill]`
  `stage1-guardian-captain=>[GuardianFlag:Shield_King_Skill,Slash:Warrior_Skill]`
  `stage1-attack-captain=>[ChargeCommand:Warrior_King_Skill 1,Slash:Warrior_Skill]`
  `stage1-hero-karin=>[SacredSwordWave:Karin_Skill 1,Slash:Warrior_Skill]`
- `dotnet build Pakuri\\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors and only the existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP `manage_scene validate` for `Assets/Scenes/NewScene/NewRunScene.unity` returned `totalIssues=0`, `missingScripts=0`, and `brokenPrefabs=0`.

### History

- 2026-05-18: User directed Code Builder to make Stage 1 enemies use a CSV-authored basic-skill slot plus the existing special skill, while keeping effect prefabs scene-authored through `EffectManager`.

## Task: 2026-05-17 Enemy Skill CSV Runtime Split

### Task title

Move Stage 1 enemy active skill tuning out of enemy rows.

### Goals

- Keep Stage 1 enemy skill execution behavior unchanged while moving per-skill tuning to `EnemySkillData.csv`.
- Preserve current source-only Archer `AimedShot` support without adding an Archer row to active `EnemyStat.csv`.
- Keep enemy runtime model fields populated through the existing `EnemyDefinition -> UnitFactory -> EnemyUnitRuntimeModel` path.

### Constraints

- Role Owner is Code Builder.
- Enemy skill execution code in `EnemyCombatSimulationSystem.cs` was not behavior-refactored in this task.
- `InGameCombatManager.ResolveEnemySkillPrefab(...)` still resolves prefabs by `StageOneEnemySkillKind`; CSV prefab paths are now included in the runtime asset catalog but are not yet the execution-time prefab resolver.
- No Play Mode verification was run by Codex.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies Stage 1 enemy skill behavior in NewRunScene Play Mode.
- Later enemy skill data work should decide whether prefab resolution also moves from `InGameCombatManager` serialized fields into CSV/runtime catalog lookup.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/Assets/CSVdata/EnemySkillData.csv` contains the 8 current Stage 1 skill IDs, including source-only `AimedShot`.
- `Pakuri/Assets/CSVdata/EnemyStat.csv` now keeps the 7 active enemy rows with `active_skill_id` references and no `active_skill_coefficient` column.
- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` keeps 8 enemy rows and now references skill behavior only through `stage_one_skill`.
- `Pakuri/Assets/Legacy/Scripts/Data/Runtime/Csv/PakuriCsvRuntimeData.EnemyDataset.cs` applies `EnemySkillRow` data to `EnemyRow.ActiveSkillName`, `ActiveSkillCoefficient`, `ActiveSkillCooldown`, `ActiveSkillDuration`, `ActiveSkillRadius`, and `ActiveSkillFlatValue`.
- `Pakuri/Assets/Legacy/Scripts/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` still builds `EnemyDefinition` from those populated `EnemyRow` fields, preserving the existing downstream runtime path.
- CSV check returned `EnemySkillRows=8`, `StageEnemyRows=8`, and no missing `stage_one_skill` references.
- Runtime/editor `dotnet build` commands completed with 0 errors and existing assembly reference warnings.
- Unity CSV validation menu produced no CSV validation errors in the warning/error console read.

### History

- 2026-05-17: User requested Enemy skill CSV separation after discussing that Enemy skills should be managed like Monster skills.

## Task: 2026-05-16 Stage Encounter CSV Seed

### Task title

Create active Stage encounter composition rows using current Stage 1 enemy IDs.

### Goals

- Store Stage 1 day encounter enemy composition outside the current fixed spawn coroutine.
- Reference only enemy IDs that exist in the current active `EnemyStat.csv`.
- Include spawn order, count, interval, right-edge spawn coordinates, boss candidate flags, guaranteed boss flags, and guaranteed prisoner flags.

### Constraints

- Role Owner is Code Builder.
- CSV data only; no enemy spawn code, prefab assignment, or Play Mode verification was changed.
- Stage 2~4 encounters were not seeded because their active `EnemyStat.csv` rows do not exist yet.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV consistency verified.

### Next Actions

- Future Stage Flow implementation should spawn encounters from `StageEncounter.csv`.
- Replace the existing fixed Stage 1 spawn sequence only after the parser/loader is implemented and verified.

### Evidence

- Added `Pakuri/Assets/CSVdata/StageEncounter.csv` with 30 rows.
- `StageEncounter.csv` references current enemy IDs `stage1-swordsman`, `stage1-shieldbearer`, `stage1-rogue`, `stage1-priest`, `stage1-guardian-captain`, `stage1-attack-captain`, and `stage1-hero-karin`.
- `StageEncounter.csv` stores right-edge spawn X as `31`, normal Y range `0~17`, and boss Y fixed at `8`, matching the inspected Stage basic rules.
- Cross-file consistency check returned `MissingEnemyRefs=0` against `Pakuri/Assets/CSVdata/EnemyStat.csv`.
- `NewRunSceneEntryManager.SpawnEnemyById(...)` now lets `NewRunStageManager` spawn these enemy IDs from CSV rows through the existing prefab/model/roster path.
- `NewRunSceneEntryManager.spawnInitialEnemySequenceOnStart` is false in `NewRunScene.unity`, so the old fixed enemy sequence no longer competes with StageManager-driven encounter spawning.

### History

- 2026-05-16: User requested active CSV files for combat composition as part of the StageManager preparation.
- 2026-05-16: Code Builder connected Stage encounter rows to a new StageManager spawn path without changing enemy skill behavior.

## Task: 2026-05-16 Stage-One Remaining Enemy Skills And Spawn Expansion

### Task title

Implement Shield, Guardian Captain, Attack Captain, and Hero Karin enemy skill behavior through the existing InGame combat structure.

### Goals

- Add the remaining requested stage-one enemy rows to `Assets/CSVData/EnemyStat.csv`.
- Reuse `EnemyCombatSimulationSystem`, `InGameCombatManager`, projectile/hitbox actors, and resource mutation services for the new enemy skills.
- Connect `ShieldUp`, `GuardianFlag`, `ChargeCommand`, and `SacredSwordWave` to authored skill prefabs under `Assets/Prefab/Enemy/Skill`.
- Keep Rogue `ShurikenThrow` connected to the authored `Rogue_Skill` prefab.
- Spawn the requested enemies in `NewRunScene` at one-second intervals through the existing entry manager sequence.
- Make Karin's `SacredSwordWave` use the enemy projectile actor path instead of the short stationary hitbox path.
- Make self/ally enemy skills such as `GuardianFlag` and `ChargeCommand` execute from cooldown rather than waiting for melee attack range.
- Add simple stage-one enemy passive stat application to the new InGame runtime.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode gameplay verification was run by Codex.
- Skill prefabs remain visual/trigger relays; HP/shield/damage/buff logic stays in runtime code.
- `ActiveSkillRadius` is used for area effects such as Guardian Captain shield and Attack Captain command, not as global targeting range.
- Stage-one enemy passives in the new InGame runtime are numeric stat adjustments only; no event-driven passive system was introduced.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified by code search, CSV parsing, scene serialization checks, Unity refresh, and builds.

### Next Actions

- User verifies in NewRunScene Play Mode that Shield, Rogue, Priest, Guardian Captain, Attack Captain, and Karin spawn in the expected one-second cadence after the swordsman.
- User verifies `ShieldUp`, cooldown-based `GuardianFlag`, cooldown-based `ChargeCommand`, projectile-based `SacredSwordWave`, and Rogue skill prefab behavior in Play Mode.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/reference/5.enemy/stage-1-enemies.md` was inspected for Shield, Rogue, Guardian Captain, Attack Captain, and Hero Karin stats and skill values.
- `Pakuri/Assets/CSVData/EnemyStat.csv` now contains `stage1-shieldbearer`, `stage1-guardian-captain`, `stage1-attack-captain`, and `stage1-hero-karin`; `stage1-rogue` already existed and remains `ShurikenThrow`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs` has switch cases for `ShieldUp`, `AimedShot`, `GuardianFlag`, `ChargeCommand`, and `SacredSwordWave`.
- `EnemyCombatSimulationSystem.cs` implements `ExecuteShieldUp`, `ExecuteAimedShot`, `ExecuteGuardianFlag`, `ExecuteChargeCommand`, and `ExecuteSacredSwordWave`.
- `EnemyCombatSimulationSystem.cs` now routes `SacredSwordWave` through `ExecuteEnemyProjectile(...)`, which uses `InGameProjectileActor`.
- `EnemyCombatSimulationSystem.cs` now treats `Heal`, `ShieldUp`, `GuardianFlag`, and `ChargeCommand` as cooldown-driven self/ally skills before the melee distance gate.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` maps `ShieldUp`, `AimedShot`, `ShurikenThrow`, `GuardianFlag`, `ChargeCommand`, and `SacredSwordWave` to serialized enemy skill prefab fields.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitRuntimeModel.cs` now stores temporary incoming damage, outgoing damage, and move-speed multipliers plus active skill duration.
- `EnemyUnitRuntimeModel.cs` now separates passive outgoing damage, incoming damage, and healing multipliers from temporary skill buff multipliers.
- Added `Pakuri/Assets/Scripts2/InGame/Units/StageOneEnemyPassiveStatApplier.cs`, which applies stage-one enemy numeric passives when `UnitFactory.CreateEnemy(...)` creates the enemy model.
- `StageOneEnemyPassiveStatApplier.cs` mirrors the inspected legacy `ApplyStageOnePassive(...)` numeric effects: Slash damage, ShieldUp defenses, AimedShot crit chance, ShurikenThrow crit damage, Heal healing, GuardianFlag incoming damage, ChargeCommand damage, and SacredSwordWave damage.
- `Pakuri/Assets/Scripts2/InGame/Core/UnitResourceMutationService.cs` applies the enemy incoming-damage multiplier before rounded damage is committed.
- `UnitResourceMutationService.cs` now multiplies passive incoming damage and temporary incoming damage separately.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes skill prefab references for Shield, Archer, Rogue, Shield King, Warrior King, and Karin skill prefabs.
- Runtime build `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Editor build `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and the same existing warnings.
- Follow-up runtime/editor builds passed with 0 errors after the Karin projectile, cooldown-driven support skill, and passive stat applier changes.
- Unity-MCP refresh reached idle after the follow-up script changes.

### History

- 2026-05-16: User confirmed remaining enemy unit and skill prefabs exist, identified `Shield`, `Shield_King`, `Warrior_King`, and `Karin`, and requested Code Builder to assign data, implement skills, and spawn each enemy one second apart in `NewRunScene`.
- 2026-05-16: User reported Karin's skill looked wrong, Guardian Captain and Attack Captain were not attacking/using skills as expected, and asked Code Builder to switch Karin to projectile behavior, make Guardian/Attack Captain use cooldown-based self/ally skills, and add simple stage-one enemy passive stat application.

## Task: 2026-05-15 InGame Stage-One Enemy Skill MVP

### Task title

Implement Warrior, Rogue, and Priest enemy skill execution through existing combat services.

### Goals

- Reuse `EnemyCombatSimulationSystem` movement, targeting, range, and cooldown flow.
- Make Warrior `Slash` spawn the authored `Warrior_Skill` prefab and deal physical damage through a trigger/fallback hitbox relay.
- Make Rogue `ShurikenThrow` spawn the authored `Achor_Skill` prefab and deal physical projectile damage through `InGameProjectileActor`.
- Make Priest `Heal` restore the lowest-health enemy ally and spawn the authored `Preist_Skill` visual prefab.
- Destroy the Priest heal visual after one short playback instead of leaving the prefab looping in the scene.
- Parent runtime enemy, skill, and monster objects under the scene's runtime hierarchy roots.
- Keep actual damage/heal mutation inside `InGameCombatManager` and `UnitResourceMutationService`.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode gameplay verification was run by Codex.
- Prefab Trigger/Collider objects relay contact only; damage, heal, side filtering, duplicate-hit blocking, and resource mutation stay in runtime code.
- Runtime hierarchy routing uses existing scene roots when available: `RunTimeObject`, `RunTimeEnemy`, `RunTimeSkill`, and `RunTimeMonster`.
- This MVP covers the three currently spawned stage-one enemy prefabs only: `stage1-swordsman`, `stage1-rogue`, and `stage1-priest`.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified by builds, Unity-MCP import/scene evidence, console read, and file checks.

### Next Actions

- User verifies in NewRunScene Play Mode that Warrior slash damages the player monster, Rogue shuriken damages the player monster, Priest heals injured enemy allies, and `Preist_Skill` is destroyed after one short playback.
- User verifies spawned enemies appear under `RunTimeObject/RunTimeEnemy`, skill instances under `RunTimeSkill`, and player/party monsters under `RunTimeObject/RunTimeMonster`.
- Later enemy work can add ShieldUp, AimedShot, GuardianFlag, ChargeCommand, and SacredSwordWave after their prefabs/data/spawn roles exist.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/Assets/Prefab/Enemy/Skill/Warrior_Skill.prefab`, `Achor_Skill.prefab`, and `Preist_Skill.prefab` exist.
- Prefab inspection found `Warrior_Skill` and `Achor_Skill` have `BoxCollider2D`; runtime relay code sets the collider to trigger on the instantiated object.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitRuntimeModel.cs` now stores `StageOneSkill`, `ActiveSkillCoefficient`, `ActiveSkillRadius`, and `ActiveSkillFlatValue`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` copies those fields from `EnemyDefinition` into `EnemyUnitRuntimeModel`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs` now executes `Slash`, `ShurikenThrow`, and `Heal` at the existing enemy cooldown/range execution point.
- `EnemyCombatSimulationSystem.cs` spawns `InGameEnemySkillHitboxActor` for slash, `InGameProjectileActor` for shuriken, and the priest visual prefab after calling heal.
- `EnemyCombatSimulationSystem.cs` now attaches `InGameAttachedSkillEffectActor` to `Preist_Skill` instances with a `0.8f` lifetime, so the heal visual follows the target briefly and then destroys itself.
- `EnemyCombatSimulationSystem.cs` Priest healing now calls `FindLowestHealthEnemyAlly(roster)` without an `ActiveSkillRadius` distance filter, so the heal target search covers the full enemy roster.
- `InGameCombatManager.cs` now exposes `InstantiateSkillPrefab(...)`, which parents skill instances under `RunTimeSkill`.
- `SkillExecutors.cs` now uses `InGameCombatManager.InstantiateSkillPrefab(...)` for player skill projectile/shield visuals as well.
- `NewRunSceneEntryManager.cs` now instantiates the selected player monster under `RunTimeMonster` and stage-one enemies under `RunTimeEnemy`, creating/finding those roots under `RunTimeObject` when needed.
- `NewRunScene.unity` serializes `runtimeObjectRoot`, `runtimeEnemyRoot`, `runtimeMonsterRoot`, and `runtimeSkillRoot` references on `GameManager` components.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameEnemySkillHitboxActor.cs` was added for short-lived melee/trigger relay hits with same-side filtering and duplicate-hit blocking.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` now supports left-moving projectile X-boundary destruction as well as the existing right-moving path.
- `Pakuri/Assets/Scripts2/InGame/Core/UnitResourceMutationService.cs` and `InGameCombatManager.cs` now expose `Heal(...)`, clamped to `Stats.MaxHealth` and routed through existing actor refresh.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` assigns `warriorSkillPrefab` to GUID `86d2cf796a0668f48bf01d312cceb7dc`, `rogueSkillPrefab` to GUID `c68a14297d96473499a2c4d10658a55f`, and `priestSkillPrefab` to GUID `8d0e9d69f614e534ca717c247f2f7c9b`.
- Runtime build `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Editor build passed after rerunning alone; the first parallel Editor build failed only with the recurring `obj\Debug\Assembly-CSharp.dll` file lock.
- Unity-MCP script refresh imported the new script; console warning/error read showed MCP client messages and no C# compile errors.
- `git diff --check` on the changed scripts and scene passed with only LF-to-CRLF normalization warnings after trimming Unity scene trailing whitespace.
- Follow-up runtime build passed with 0 errors and existing warnings; the first parallel Editor build hit the recurring output DLL file lock and the standalone Editor build passed.
- Unity-MCP console warning/error read showed MCP client handler messages and no C# compile errors after the hierarchy/lifetime follow-up.
- Follow-up runtime/editor builds passed with 0 errors after removing the Priest heal target distance filter; Unity-MCP refresh reached idle and console warning/error read returned only MCP client handler logs.

### History

- 2026-05-15: User approved implementing the three current enemy skills through the existing structure and confirmed skill prefabs are under `Assets/Prefab/Enemy/Skill`.
- 2026-05-15: Code Builder implemented the three-skill MVP while keeping damage/heal resource mutation outside the prefabs.
- 2026-05-15: User reported `Preist_Skill` continued replaying and requested runtime hierarchy routing; Code Builder added short lifetime destruction for the priest visual and routed runtime enemies/skills/monsters to the requested roots.
- 2026-05-16: User clarified all skills should have no range concept; Code Builder removed the Priest heal target distance filter while leaving melee/projectile movement and prefab hit behavior intact.

## Task: 2026-05-15 InGame Projectile Damage / Enemy Removal Fix

### Task title

Fix NewRunScene projectile HP mutation visibility and dead enemy removal.

### Goals

- Make projectile-applied damage mutate HP as rounded whole-number values after defense calculation.
- Remove dead units from the InGame roster and destroy their Actor GameObject when HP reaches zero.
- Make enemy HP `Fill` decrease like a left-anchored slide, with the left edge fixed and the right edge shrinking.
- Show damage feedback through the prefab-authored `Damage` TextMesh, moving upward by about 1 local Y unit while fading out.
- Keep `Fill` inside the actual rendered `Background` sprite bounds during the left-anchored slide.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode verification was run by Codex; user verifies projectile hit, HP bar behavior, and enemy deletion in Play Mode.
- Keep the existing Phase4-C projectile actor hit route through `InGameCombatManager.ApplyDamage(...)`.
- Do not alter user-authored prefab HP bar transforms.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified by code inspection, builds, Unity refresh/console, and file checks.

### Next Actions

- User verifies in NewRunScene Play Mode that Eve-A projectile hits decrease enemy HP in rounded whole numbers, `Fill` stays aligned with `Background`, and enemies disappear when HP reaches zero.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` already routes hits through `combatManager.ApplyDamage(target.Model, damage, damageAttribute)`.
- `Pakuri/Assets/Scripts2/InGame/Core/UnitResourceMutationService.cs` now rounds defense-adjusted damage with `Mathf.Round(...)` and stores rounded HP/Shield resources through `RoundResource(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls `RemoveUnitIfDead(result)` after damage refresh; the helper finds the roster entry, unregisters the model, and destroys the Actor GameObject.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls `ShowDamageIfChanged(result)` after damage refresh, before death cleanup.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` unregisters dead units immediately but delays Actor GameObject destruction by `0.95f` seconds so the killing hit's Damage text can appear.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` now resolves the prefab-authored `Damage` TextMesh and routes `ShowDamage(...)` into an `InGameDamageTextPopup` component added to that child object at runtime.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` now updates HP `Fill` with left-anchored segment positioning: `backgroundCenterX - backgroundWidth * 0.5f + segmentWidth * 0.5f`.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` now computes segment positions from actual SpriteRenderer local rendered width: `sprite.bounds.size.x * localScale.x`, instead of treating `localScale.x` itself as displayed width.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` now converts desired rendered segment width back into target sprite scale via `ResolveScaleXForRenderedWidth(...)`, so `Fill` remains inside `Background` even when sprite bounds are not 1 world unit wide.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` contains `InGameDamageTextPopup`, which displays `N(Damage)`, moves from the authored local position to `+1` local Y over `0.9` seconds, and fades alpha to 0.
- Prefab inspection with `Select-String` found enemy `Background` local position `{x: 0, y: 0, z: 0}` and `Fill` local position `{x: 0, y: 0, z: -0.01}` with X scale `20` in all three stage-one enemy prefabs before this code change.
- Prefab inspection with `Select-String` found `Damage` TextMesh children in all three stage-one enemy prefabs, with authored local position `{x: 0, y: 3.52, z: 0}` and text `00`.
- Runtime build `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings after rerunning alone because the first parallel build hit an `obj\Debug\Assembly-CSharp.dll` file lock.
- Editor build `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and the same existing warnings.
- Follow-up runtime/editor builds passed with 0 errors after integrating the Damage Text popup and left-anchored HP Fill change.
- Follow-up runtime/editor builds passed with 0 errors after switching Fill math to actual SpriteRenderer rendered width and changing the damage text format to `N(Damage)`. The first parallel runtime build failed only with the recurring `obj\Debug\Assembly-CSharp.dll` file lock; standalone runtime build passed.
- Unity-MCP script refresh reached idle; console warning/error read showed only MCP client handler logs after this follow-up.
- Unity-MCP script refresh reached idle; console warning/error read no longer showed the temporary `InGameDamageTextPopup` type-missing compile errors after the helper class was moved into the already-compiled Actor file.
- Unity-MCP `validate_script` reported known duplicate-method false positives; direct `Select-String` found one `RoundResource(...)` definition, one `RemoveUnitIfDead(...)` definition, and one `ResolveDebugViewReferences(...)` definition per Actor file while builds passed.
- `git diff --check` on the changed scripts passed with only LF-to-CRLF normalization warnings.

### History

- 2026-05-15: User reported projectile firing worked, but HP decrease and monster deletion did not proceed, and enemy HPBar `Fill` moved away from `Background` after the first hit.
- 2026-05-15: Code Builder inspected the current projectile hit, resource mutation, manager refresh, Actor HPBar, and prefab HPBar transform evidence, then implemented rounded damage, death removal, and HP Fill position stabilization.
- 2026-05-15: User clarified HP should decrease from left to right like a slide and requested prefab `Damage` Text feedback that rises by about 1 Y and fades out; Code Builder implemented left-anchored Fill shrink and runtime Damage Text popup behavior.
- 2026-05-15: User reported `Fill` still escaped `Background` and requested damage text as `number(Damage)` rather than `Damage(number)`; Code Builder changed Fill width/position calculations to SpriteRenderer rendered-width units and changed popup format to `N(Damage)`.

## Task: 2026-05-15 Phase3-C Resource Mutation Pipeline

### Task title

Implement the InGame damage/shield/HP mutation pipeline without connecting it to enemy attack attempts yet.

### Goals

- Add a single runtime service for future enemy/monster skill code to mutate HP and Shield.
- Keep Phase3-B enemy attack attempts from applying real damage until monster and enemy skill execution exists.
- Refresh only the changed unit actor when HP or Shield changes.
- Represent HP and Shield as adjacent segments inside the same HP bar background.

### Constraints

- Role Owner is Code Builder.
- Do not connect Phase3-B attack attempts to actual HP loss in this slice.
- Do not implement monster skills, enemy skills, projectiles, support behavior, death animation, reward logic, or Play Mode gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified up to C# build, Unity script refresh, script validation for the new service, and file checks.

### Next Actions

- User verifies in Play Mode only that existing Phase3-B movement/attack-attempt behavior remains intact if needed.
- Later monster/enemy skill execution should call `InGameCombatManager.ApplyDamage(...)`, `GrantShield(...)`, or `SetShield(...)`.
- Later skill implementation should verify real HP/Shield decrease and death behavior in Play Mode.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Core/UnitResourceMutationService.cs` and `.meta`.
- `UnitResourceMutationService.ApplyDamage(...)` applies defense, consumes `CurrentShield` first, then reduces `CurrentHealth`.
- `UnitResourceMutationService.GrantShield(...)` and `SetShield(...)` mutate `UnitResourceRuntime.CurrentShield`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now exposes `ApplyDamage(...)`, `GrantShield(...)`, `SetShield(...)`, and `RefreshUnitActor(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs` was not connected to damage application in this slice.
- `Pakuri/Assembly-CSharp.csproj` includes `Assets\Scripts2\InGame\Core\UnitResourceMutationService.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing System.Net.Http/System.IO.Compression warnings after rerunning alone because a prior parallel build hit an output DLL file lock.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing warnings.
- Unity-MCP `validate_script` passed with 0 diagnostics for `UnitResourceMutationService.cs`; validation reported duplicate-method diagnostics for actor/manager files, but direct `Select-String` found only one definition each for the reported methods and both builds passed.
- Unity-MCP `execute_code` could not run because MCP's mono command failed with the existing Windows path-length error.
- `git diff --check` over the changed scripts and csproj passed with only LF-to-CRLF normalization warnings.

### History

- 2026-05-15: User completed Phase3-B Play Mode verification and asked whether Phase3-C should avoid real damage until monster/enemy skills exist.
- 2026-05-15: User directed Code Builder to implement only the Phase3-C damage/shield/HP pipeline and same-bar HP/Shield representation.

## Task: 2026-05-15 Phase3-B Enemy Movement Targeting Basic Attack Attempt

### Task title

Implement the first roster-driven enemy movement, targeting, and basic attack attempt loop.

### Goals

- Keep `InGameCombatManager` as the orchestrator and avoid putting all enemy behavior directly inside it.
- Use `UnitRosterService` player/enemy lists for target selection instead of scene searches.
- Move alive enemy entries toward the nearest alive player entry when outside attack-attempt range.
- When in range, record a basic attack attempt with cooldown, without applying damage or reducing HP.
- Keep the implementation compatible with `Melee`, `Ranged`, `MeleeAndRanged`, and `Buffer` attack types.

### Constraints

- Role Owner is Code Builder.
- This slice does not implement projectiles, melee hit timing, damage application, shield/heal/buff support behavior, skill execution, or live HP mutation.
- Basic attack attempt ranges/cooldowns are temporary runtime model fields derived from current enemy definition values until a dedicated basic-attack data schema exists.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified up to compile, Unity refresh, script validation, and file checks. Play Mode behavior verification remains user-owned.

### Next Actions

- User verifies in Play Mode that spawned enemies move toward the selected player monster and stop/attempt attacks when in range.
- Later Phase3-C should replace attack attempts with a damage/shield/status service and Actor refresh.
- Later support behavior should make `Buffer` target allies for heal/shield/buff instead of using the current player-targeting attack-attempt placeholder.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs`, which ticks `roster.Enemies`, finds nearest alive player targets from `roster.Players`, moves enemies with `Vector3.MoveTowards`, and increments attack-attempt state when in range.
- Updated `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` so `Update()` calls `enemyCombatSimulation.Tick(roster, Time.deltaTime, logEnemyAttackAttempts)`.
- Updated `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitRuntimeModel.cs` with `AttackAttemptRange` and `AttackAttemptCooldownSeconds`.
- Updated `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` so enemy model creation derives attempt range/cooldown from `EnemyDefinition.AttackType`, `ActiveSkillRadius`, and `ActiveSkillCooldown`.
- `UnitFactory.cs` maps `Ranged` to `Math.Max(5f, ActiveSkillRadius)`, `MeleeAndRanged` to `Math.Max(4f, ActiveSkillRadius)`, `Buffer` to `Math.Max(5f, ActiveSkillRadius)`, and default melee to `1.4f`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes `enemyCombatSimulationEnabled: 1` and `logEnemyAttackAttempts: 0` on `InGameCombatManager`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and the same existing warnings.
- Unity-MCP `refresh_unity` force refresh cleared the initial `EnemyCombatSimulationSystem` not found compile error after the new file was imported.
- Unity-MCP `validate_script` passed with 0 diagnostics for `InGameCombatManager.cs` and `EnemyCombatSimulationSystem.cs`.
- Unity-MCP `validate_script` reported a duplicate-method diagnostic for `UnitFactory.cs`, but direct `Select-String` found only one `CreateMonster(...)` definition and both runtime/editor builds passed.
- Unity-MCP `execute_code` verification could not run because MCP's mono invocation failed with `파일 이름이나 확장명이 너무 깁니다`; no Play Mode verification was attempted.
- `git diff --check` over the changed combat scripts and scene passed with only LF-to-CRLF normalization warnings.

### History

- 2026-05-15: User explicitly requested Code Builder to implement enemy movement, targeting, and basic attack "attempt".

## Task: 2026-05-15 Stage1 Enemy Type CSV And Triple Spawn Entry

### Task title

Record stage-one Melee/Ranged/Buffer enemy data and NewRunScene triple enemy entry spawning.

### Goals

- Keep using the existing `attack_type` column for enemy behavior grouping.
- Standardize current stage-one enemy types as `Melee`, `Ranged`, and `Buffer`.
- Record stage-one swordsman, rogue, and priest rows from the inspected enemy reference.
- Spawn the three stage-one enemy prefabs in NewRunScene at one-second intervals.

### Constraints

- Role Owner is Code Builder.
- User explicitly confirmed Rogue is `Ranged` and Priest is `Buffer`.
- Do not implement movement, attack, targeting, shield/heal/buff behavior, damage, or live HP mutation in this slice.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified up to build, scene serialization, prefab inspection, and console checks.

### Next Actions

- User verifies in Play Mode that `NewRunScene` spawns Warrior, Rogue, and Priest in order, one second apart.
- Phase3-B movement/targeting/basic attack work should consume `InGameCombatManager.Roster` and read `EnemyDefinition.AttackType`.
- Later shield/heal/buff logic should treat `Buffer` as behavior data, not as a separate hardcoded prefab path.

### Evidence

- `Pakuri/reference/5.enemy/stage-1-enemies.md` was inspected for swordsman, rogue, and priest stats, defenses, active skills, and passives.
- `Pakuri/Assets/CSVData/EnemyStat.csv` now contains `stage1-swordsman` as `Melee`, `stage1-rogue` as `Ranged`, and `stage1-priest` as `Buffer`.
- `Import-Csv Pakuri\Assets\CSVData\EnemyStat.csv` returned the three rows with HP `100`, `70`, and `90`, and skill radii `1.4`, `6`, and `5`.
- `Pakuri/Assets/Legacy/Scripts/Data/Definition/EnemyDefinition.cs` now defines `EnemyAttackType.Buffer`.
- `Pakuri/Assets/Legacy/CSVdata/source/stage_one_enemies.csv` now stores `stage1-priest` with `attack_type` `Buffer`.
- `Pakuri/Assets/Legacy/Data/GameData/Enemies/stage1-priest.asset` now stores `AttackType: 3`, matching the new `Buffer` enum position.
- `Pakuri/Assets/Legacy/Scripts/Combat/Manager/CombatRuntimeEnemies.cs` fallback stage-one priest data now creates priest with `EnemyAttackType.Buffer`.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` starts `SpawnInitialEnemySequence()` and calls swordsman, rogue, and priest spawn methods separated by `WaitForSeconds(enemySpawnIntervalSeconds)`.
- `NewRunSceneEntryManager.cs` spawns each enemy at `enemySpawnPoint.position.x` and `UnityEngine.Random.Range(enemySpawnMinY, enemySpawnMaxY)` for Y.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now serializes Warrior, Rogue, and Priest enemy prefab GUIDs and IDs: `stage1-swordsman`, `stage1-rogue`, and `stage1-priest`, with `enemySpawnIntervalSeconds: 1`.
- Unity-MCP `manage_prefabs get_hierarchy` confirmed `Stage1_Rogue_Unit.prefab` and `Stage1_Priest_Unit.prefab` have root `Pakuri.InGame.EnemyUnitActor`, `MonsterHpBar`, `Fill`, `Shield`, `MonsterHpLabel`, and `MonsterNameLabel`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and the same existing warnings.
- Unity-MCP console warning/error read showed only MCP client handler logs after the scene save.
- Unity-MCP `validate_script` passed for `EnemyDefinition.cs`; it reported duplicate-method diagnostics for `NewRunSceneEntryManager.cs`, but direct `Select-String` found only one relevant method definition each and both runtime/editor builds passed.

### History

- 2026-05-15: User rejected adding a new behavior column and directed Code Builder to keep existing `attack_type`, using `Melee`, `Ranged`, and `Buffer`; user also said Priest is `Buffer` and Rogue is `Ranged`.
- 2026-05-15: User said the three enemy-type prefabs were added under `Assets/Prefab/Enemy` and requested CSV data entry plus one-second triple spawn on NewRunScene entry.

## Task: 2026-05-15 InGame Phase3-A Combat Roster Ownership

### Task title

Implement the Phase3-A combat runtime ownership boundary for spawned monsters and enemies.

### Goals

- Make `InGameCombatManager` the owner of a runtime unit roster rather than a direct movement/targeting/attack/damage implementation class.
- Make `UnitRosterService` store active player and enemy registrations for later movement, targeting, and attack systems.
- Register the selected player monster and first spawned enemy from the current `NewRunSceneEntryManager` spawn path.
- Keep this slice behavior-preserving for Phase2-B spawn; do not implement movement, attacks, targeting, wave cadence, damage, or HP mutation yet.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Avoid per-frame scene searches and per-Actor combat `Update()` ownership in this slice.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Phase3-B should add enemy movement, target selection, and basic attack systems using the roster instead of scene searches.
- Phase3-C should add damage/shield/status services and dirty Actor refresh.
- User verifies in Play Mode that Phase2-B spawning still works with the new manager component on `GameManager`.

### Evidence

- Updated `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` so it owns a `UnitRosterService` and exposes `RegisterPlayerMonster(...)`, `RegisterEnemy(...)`, and active unit counts.
- Updated `Pakuri/Assets/Scripts2/InGame/Units/UnitRosterService.cs` with active unit registration lists for all entries, players, and enemies, plus duplicate-safe register/unregister/clear behavior.
- Updated `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` with a serialized `combatManager`, `[RequireComponent(typeof(InGameCombatManager))]`, and registration calls after spawned player/enemy Actor binding.
- Updated `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` so `GameManager` has `Pakuri.InGame.InGameCombatManager`, and `NewRunSceneEntryManager.combatManager` references it.
- Unity-MCP component read showed `GameManager` has `Transform`, `Pakuri.InGame.NewRunSceneEntryManager`, and `Pakuri.InGame.InGameCombatManager`; the entry manager's `combatManager` field references the manager component.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after rerunning outside the sandbox because the sandbox blocked `C:\Users\t3312\AppData\Local\Microsoft SDKs`; existing `System.Net.Http` / `System.IO.Compression` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and the same existing warnings.
- Unity-MCP `validate_script` passed with 0 diagnostics for `InGameCombatManager.cs` and `UnitRosterService.cs`.
- Unity-MCP `validate_script` reported duplicate-method diagnostics for `NewRunSceneEntryManager.cs`, but direct `Select-String` found only one definition each for `SpawnSelectedPlayerUnit`, `ResolveCatalog`, `ResolveSpawnPoint`, and `ResolveEnemySpawnPoint`; the runtime/editor builds passed.
- Unity-MCP console read showed the existing CSV auto-sync warning for missing legacy `Assets/CSVdata/source/catalog_monsters.csv` and an MCP object-converter warning while inspecting `InGameCombatManager`; no compile error was reported.
- `git diff --check` over the changed scripts and scene passed with only LF-to-CRLF normalization warnings after cleaning Unity YAML trailing whitespace.

### History

- 2026-05-15: User asked Code Builder to start Phase3-A after the roadmap was updated to make `InGameCombatManager` a loop orchestrator rather than the direct owner of all combat details.

## Task: 2026-05-15 NewRunScene Phase2-B Enemy Spawn Handoff

### Task title

Design the next Phase2-B slice for spawning the first stage-one enemy prefab in NewRunScene.

### Goals

- Spawn `Assets/Prefab/Enemy/Stage1_Warrior_Unit.prefab` from the authored `NewRunScene` enemy `SpawnPoint`.
- Create an `EnemyUnitRuntimeModel` from `stage1-swordsman` data through the existing `UnitFactory.CreateEnemy(...)` path.
- Bind that model into `EnemyUnitActor` so HP/name/debug children can refresh like the monster actor path.
- Keep this slice to spawn and actor/model binding only; movement, attacks, targeting, wave cadence, and damage exchange remain later combat-loop work.

### Constraints

- Role Owner is Designer for this handoff; no runtime C# or scene changes were made in this task.
- Current user-authored spawn rule is: X comes from the `SpawnPoint` transform X, and Y is randomized from -5 to +5.
- Do not overwrite user-authored prefab HP bar transform/visual layout.
- Do not run Unity Play Mode; user owns gameplay verification.

### Role Owner

Designer -> Code Builder

### Status

Builder implementation completed and locally verified. Phase2-B enemy spawn/model/actor-binding scope is complete; movement, attacks, targeting, wave cadence, and damage exchange remain later combat-loop work.

### Next Actions

- User verifies in Play Mode that one `Stage1_Warrior_Unit` appears at `SpawnPoint.x` with randomized Y in the -5 to +5 range.
- Later Code Builder work should implement enemy movement, attacks, targeting, wave cadence, damage exchange, and live HP mutation.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/Assets/Prefab/Enemy/Stage1_Warrior_Unit.prefab` exists; Unity-MCP `manage_asset get_info` returned asset type `UnityEngine.GameObject`, name `Stage1_Warrior_Unit`, and GUID `f2892daa44e860e49b1ea2b17f8682dc`.
- Unity-MCP `manage_prefabs get_hierarchy` found root `Stage1_Warrior_Unit` with components `Transform`, `SpriteRenderer`, and `Pakuri.InGame.EnemyUnitActor`, plus `MonsterHpBar/Background`, `Fill`, `Shield`, `MonsterHpLabel`, and `MonsterNameLabel` children.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs:5` currently defines `EnemyUnitActor` as an empty `MonoBehaviour`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs:29` defines `CreateEnemy(EnemyDefinition definition, int slotIndex = 0)`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:924` contains `m_Name: SpawnPoint`, and the inspected transform block has `m_LocalPosition: {x: 9.43, y: 0, z: 0}`.
- Updated `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` with `SpawnInitialEnemyUnit()`, `TryCreateEnemyModel(...)`, `ResolveEnemyDefinition(...)`, `BindSpawnedEnemyActor(...)`, and `UnityEngine.Random.Range(enemySpawnMinY, enemySpawnMaxY)` for the Y range.
- Updated `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` with `Initialize(EnemyUnitRuntimeModel)`, `RefreshDebugView()`, model storage, label lookup, and HP/shield fill scaling.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:679` now assigns `enemySpawnPoint`, `:686` assigns `stageOneEnemyPrefab` to GUID `f2892daa44e860e49b1ea2b17f8682dc`, `:687` sets `initialEnemyId: stage1-swordsman`, and `:688` through `:689` set Y range `-5` to `5`.
- 2026-05-15 build evidence: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after rerunning with approval because the sandbox blocked `C:\Users\t3312\AppData\Local\Microsoft SDKs`; existing `System.Net.Http` / `System.IO.Compression` warnings remained.
- 2026-05-15 build evidence: `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and the same existing warnings.
- Unity-MCP `manage_components set_property` succeeded for `enemySpawnPoint`, `stageOneEnemyPrefab`, `initialEnemyId`, `enemySpawnMinY`, and `enemySpawnMaxY`; `manage_scene save` saved `Assets/Scenes/NewScene/NewRunScene.unity`.
- Unity-MCP console warning/error read showed MCP client handler logs and an existing CSV auto-sync warning for missing legacy `Assets/CSVdata/source/catalog_monsters.csv`; no new compile error was reported.
- `git diff --check` on the changed scripts, scene, and boards completed with exit code 0, aside from LF-to-CRLF normalization warnings.

### History

- 2026-05-15: User stated that `Stage1_Warrior_Unit.prefab` now has HP and `EnemyUnitActor.cs` assigned, `NewRunScene` has the enemy `SpawnPoint` assigned, and the spawn range should use the spawn point X with Y from -5 to +5.
- 2026-05-15: Code Builder implemented the one-enemy spawn/model/actor-binding slice and verified it with builds, Unity-MCP field assignment/save evidence, console read, and diff check.

## Migrated Task Blocks

## Archive Note

- This file had no dated `## Task:` / `## Recent Task:` headings.
- Existing task blocks were moved to `boards/ARCHIVE/BLACKBOARD_UNDATED_ARCHIVE_2026-05-12.md` on 2026-05-12.
- Source file: `boards/COMBAT/ENEMY_BLACKBOARD.md`.

## Task: 2026-05-14 Stage1 Swordsman CSVData Phase0-2 Seed Row

### Task title

Record `stage1-swordsman` row added to the new CSVData enemy file.

### Goals

- Seed the first enemy row in `EnemyStat.csv`.
- Preserve stage-one swordsman stats, defenses, active skill, and passive summary from the inspected enemy reference.

### Constraints

- Role Owner is Code Builder.
- No enemy runtime behavior, prefab, scene, or Play Mode changes.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Later CSVData mapping should read `stage1-swordsman` from `EnemyStat.csv`.
- Enemy prefab binding remains a later InGame actor/scene task.

### Evidence

- `Pakuri/Assets/CSVData/EnemyStat.csv` now contains `stage1-swordsman`.
- `Pakuri/reference/5.enemy/stage-1-enemies.md` provides the inspected HP 100, attack 12, spell 0, move speed 1.00, physical defense 5, elemental defenses 2, active skill `베기`, cooldown 2.0, coefficient 100%, and passive `검술 숙련`.
- `Import-Csv Pakuri\Assets\CSVData\EnemyStat.csv` returned `stage1-swordsman` with HP `100`, attack `12`, physical defense `5`, and active skill `베기`.

### History

- 2026-05-14: Code Builder added the stage-one swordsman seed row as part of CSVData Phase0~2.

## Task: 2026-05-13 Phase 4 Enemy Simulation Handoff

### Task title

Record enemy-side handoff after Phase 3 closeout.

### Goals

- Confirm Phase 3 closeout did not implement enemy simulation.
- Identify current enemy owner locations for the next refactoring phase.
- Preserve the existing Phase 4 sequencing.

### Constraints

- Role Owner is Code Builder.
- Do not change enemy runtime C# behavior in Phase 3-H.
- Do not run Unity Play Mode.

### Role Owner

Code Builder

### Status

Ready for Phase 4 planning/implementation. No enemy code was changed in Phase 3-H.

### Next Actions

- Start Phase 4 `Enemy Simulation Split` by inspecting and separating enemy spawn, update, movement/attack, status, and cleanup ownership.
- Keep common target model migration for the later planned target/effect phase.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` still defines `EnemyRuntime` as a private nested class.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:306` owns `UpdateSpawning()`.
- `CombatRuntimeEnemies.cs:336` owns `SpawnEnemy(...)`.
- `CombatRuntimeEnemies.cs:706` owns `UpdateEnemies()`.
- Phase 3-H build and Unity-MCP verification passed without changing enemy simulation code.

### History

- 2026-05-13: Builder closed Phase 3 and recorded that Phase 4 enemy simulation is the next default implementation phase.

## Task: 2026-05-13 Enemy Common Target And Prefab Timing Note

### Task title

Record when enemy common target modeling and prefab-based actor authoring should be considered.

### Goals

- Keep enemy simulation split before common target ownership migration.
- Treat enemy prefab authoring as a later scene-facing view/component decision.
- Avoid replacing common combat-state modeling with prefab inheritance.

### Constraints

- Role Owner is Designer.
- Do not change runtime C# behavior.
- Do not claim prefab enemy creation exists unless code evidence is found.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Phase 4 should separate enemy simulation before common target ownership is decided.
- Phase 7 should introduce enemy target read adapter / model connection.
- Phase 8 may evaluate enemy prefab/view component authoring after target/effect APIs stabilize.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` to state that prefab-based commonization is a Phase 8 view/component question.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` defines `EnemyRuntime` as a private nested class.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:354`, `:419`, and `:517` create enemy renderer/label/parts with `AddComponent`.
- Searched enemy creation evidence did not show an `enemyPrefab` or `Instantiate` path in the inspected `CombatRuntimeEnemies.cs` matches.

### History

- 2026-05-13: User proposed prefab-based Monster / Enemy creation and asked where it belongs in the amended roadmap.
## Task: 2026-05-16 NewRunStage Boss Encounter Runtime

### Task title

Apply active StageEncounter boss selection and health multipliers.

### Goals

- Make normal encounters select one boss candidate from active encounter rows.
- Make guaranteed boss rows always spawn as boss rows.
- Apply `boss_health_multiplier_min/max` to spawned enemy runtime health.

### Constraints

- Role Owner is Code Builder.
- Do not alter enemy prefab assets in this task.
- User owns Play Mode validation of actual combat pacing.

### Role Owner

Code Builder

### Status

Implemented after Reviewer fix request.

### Next Actions

- User verifies in Play Mode that normal combat has one tougher boss enemy and midboss/boss days keep their guaranteed boss enemies.

### Evidence

- `Pakuri/reference/5.enemy/stage-basic-rules.md` states normal combat randomly chooses one normal enemy as the boss and Stage 1 normal boss health is 10~20x.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` has `is_boss_candidate`, `is_guaranteed_boss`, and `boss_health_multiplier_min/max` columns.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunStageManager.cs` now selects boss rows and passes a health multiplier to enemy spawning.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` now applies the health multiplier to `EnemyUnitRuntimeModel.Stats.MaxHealth` and `Resources.CurrentHealth`.
- Reviewer reported missing boss modifier application; Builder fixed it and reran runtime/editor builds with 0 errors.

### History

- 2026-05-16: Code Reviewer found StageEncounter boss health columns were parsed but not applied at spawn time.
- 2026-05-16: Builder connected boss row selection and enemy health multiplier application.

## Task: 2026-05-16 NewRunScene Enemy Spawn Visibility Fix

### Task title

Align active StageEncounter enemy spawn coordinates to NewRunScene.

### Goals

- Make 1-1 stage enemies spawn at the authored NewRunScene enemy spawn point area.
- Keep encounter positioning controlled by CSV data instead of hardcoded spawn overrides.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies that `NewRunScene` stage 1-1 enemies are visible after entering the scene.

### Evidence

- Unity-MCP scene inspection found `SpawnPoint` at `x=9.02, y=0, z=0`.
- Unity-MCP scene inspection found `NewRunStageManager` on `GameManager` references `Assets/CSVdata/StageEncounter.csv`.
- Unity-MCP prefab inspection confirmed stage 1 enemy prefabs have `Pakuri.InGame.EnemyUnitActor`.
- Active `Pakuri/Assets/CSVdata/StageEncounter.csv` now uses `spawn_x=9.02` for 30 rows, normal `spawn_y=-5..5`, and guaranteed boss `spawn_y=0..0`.
- CSV check returned `Rows=30; SpawnX=9.02; MinY=-5; MaxY=5`.
- Runtime and editor `dotnet build` commands completed with 0 errors.

### History

- 2026-05-16: User reported enemies were not spawning after entering `NewRunScene`.
- 2026-05-16: Builder verified scene references and prefabs, then corrected off-screen active encounter coordinates.

## Task: 2026-05-17 NewRunScene Enemy Spawn Boundary Refactor

### Task title

Move enemy prefab/model/Actor spawn binding behind `NewRunUnitSpawnManager`.

### Goals

- Keep `NewRunStageManager` responsible for encounter row selection, boss row selection, spawn timing, enemy-clear waiting, and reward preparation.
- Move enemy prefab lookup, enemy model creation, boss health multiplier application, Instantiate, `EnemyUnitActor.Initialize(...)`, runtime root parenting, and combat roster registration out of `NewRunSceneEntryManager`.
- Preserve the existing `NewRunStageManager -> NewRunSceneEntryManager.SpawnEnemyById(...)` call surface for compatibility.

### Constraints

- Role Owner is Code Builder.
- No enemy CSV rows, skill execution logic, passive logic, or combat simulation behavior was intentionally changed.
- User owns Play Mode validation of enemy spawn and combat pacing.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- User verifies NewRunScene Play Mode enemy spawn visibility, StageManager encounter progression, boss health multiplier behavior, and combat clear detection.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Core/NewRunUnitSpawnManager.cs`.
- `NewRunUnitSpawnManager.SpawnEnemyById(...)` now resolves stage-one enemy prefabs, creates `EnemyUnitRuntimeModel` through `UnitFactory.CreateEnemy(...)`, applies health multipliers, parents spawned enemies under `RunTimeEnemy`, initializes `EnemyUnitActor`, and registers enemies with `InGameCombatManager`.
- `NewRunSceneEntryManager.SpawnEnemyById(...)` remains as a compatibility wrapper and delegates to `NewRunUnitSpawnManager`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes `NewRunUnitSpawnManager` on `GameManager` with the existing stage-one enemy prefab references.
- Runtime build `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing warnings.
- Editor build `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing warnings.
- Unity-MCP console warning/error read after clearing showed only MCP client handler logs.
- `git diff --check` passed for the changed scripts and scene, with only LF-to-CRLF normalization warnings.

### History

- 2026-05-17: User asked whether `NewRunSceneEntryManager` had a god-class problem and then requested Code Builder implementation of the SpawnManager refactor while keeping `NewRunStageManager`.
