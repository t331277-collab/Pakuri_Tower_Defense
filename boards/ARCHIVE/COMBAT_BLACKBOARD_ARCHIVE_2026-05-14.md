## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps active combat task blocks after the 2026-05-12 archive pass; newer combat tasks may be appended above older retained context.
- Source file: `boards/COMBAT/COMBAT_BLACKBOARD.md`.

## Task: 2026-05-14 Eve-E Field Data Implementation

### Task title

Record combat-side data impact of Eve-E Field classification.

### Goals

- Ensure Eve-E is no longer treated as a projectile data skill for InGame validation/mapping.
- Confirm the current mapper resolves Eve-E to `ZoneSkillData`.
- Keep combat execution unchanged until later skill executor phases.

### Constraints

- Role Owner is Code Builder.
- No combat loop, projectile runtime, target service, scene, prefab, or Play Mode changes.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Later skill executor work should implement Eve-E as zone/field behavior and define any missing radius/placement tuning.

### Evidence

- Updated Eve-E source and asset data from projectile classification to `Field`.
- Unity-MCP Editor code execution returned `mapped=ZoneSkillData|zone=True|errors=0|warnings=0` for Eve-E.
- Runtime and editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: Code Builder changed Eve-E classification to support ZoneSkillData.

## Task: 2026-05-14 InGame Phase2-A Eve Unit Model Mapping

### Task title

Record combat-side Phase2-A base unit model creation for Eve.

### Goals

- Map `MonsterDefinition` and `EnemyDefinition` into `BaseUnitRuntimeModel` family instances.
- Use Eve as the selected Phase2-A monster sample.
- Keep only unit-common combat stats/resources/defenses in the base model.
- Keep scene object creation, actor binding, target services, damage services, and skill execution out of this slice.

### Constraints

- Role Owner is Code Builder.
- No scene edit, code-generated prefab creation, combat loop, target registry, damage resolver, or Play Mode verification.
- Existing monster skill/projectile tuning stays in definitions until later SkillData mapper work.
- Reuse existing `PakuriCsvRuntimeData`, `GameDataCatalog`, `PakuriDataManager`, and `RunSession` flow.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Phase2-B should bind `MonsterUnitActor` / `EnemyUnitActor` to user-authored prefabs and existing runtime model values without adding combat execution.
- User owns Play Mode gameplay verification when actor or scene binding begins.

### Evidence

- `UnitFactory` now creates selected monster, manifested monster, enemy, and Phase2-A test models from current data definitions.
- `BaseUnitRuntimeModel` now initializes identity, stats, defenses, resources, and auto flags by default.
- `MonsterUnitRuntimeModel` inherits `BaseUnitRuntimeModel` and owns monster learned state.
- `EnemyUnitRuntimeModel` inherits `BaseUnitRuntimeModel` and owns enemy encounter/attack/attribute classification.
- Added `UnitDefenseRuntime` so defense data from `AttributeDefenseSet` is not dropped during model creation.
- `InGameTestDataManager` now defaults to Eve and `stage1-swordsman` and stores loaded sample monster/enemy models.
- Unity-MCP Editor code execution returned `ok=True|monster=eve|monsterHp=220|learnedA=1|enemy=stage1-swordsman|enemyHp=100`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP refresh reached idle; console warning/error read returned only MCP client handler logs, not C# compile errors.

### History

- 2026-05-14: User explicitly assigned Code Builder and requested Phase2-A implementation with Eve as the monster sample.

## Task: 2026-05-14 InGame Phase1-D Skill Data Validation

### Task title

Record combat-side Phase1-D InGame skill data validation.

### Goals

- Validate mapped InGame skill data before target, damage, status, and executor work starts.
- Catch duplicate IDs, empty IDs, slot kind mismatches, owner/character ID mismatches, unsupported runtime kinds, and missing timing fields.
- Keep combat execution behavior unchanged.

### Constraints

- Role Owner is Code Builder.
- No combat loop, damage resolver, target registry, executor registry, scene wiring, CSV edit, or Play Mode verification.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Use the validator before Phase2-A/Phase4 data execution work.
- Run `Pakuri/InGame/Validate Skill Data` in Unity Editor when an editor instance is available.

### Evidence

- Added data-only validator `InGameSkillDataValidator.cs`.
- Added editor menu `InGameSkillDataValidationMenu.cs`.
- `InGameSkillDataValidator` checks duplicate `SkillId` / `PassiveId`, empty IDs/names, monster ID prefix mismatches, active slot A-E and passive slot F-J mismatches, unsupported active `SkillRuntimeKind`, negative timing values, projectile range/magazine/reload/shot interval, non-passive cooldown, and mapped `SkillData` timing/targeting presence.
- Runtime and editor `dotnet build` checks completed with 0 errors; existing assembly reference warnings remain.
- Unity-MCP refresh was unavailable because no Unity Editor instance was found.

### History

- 2026-05-14: Code Builder implemented Phase1-D as a validation-only combat/data safety step before later execution phases.

## Task: 2026-05-14 InGame Final Combat Runtime Target

### Task title

Record the combat-side final runtime responsibility map for InGame.

### Goals

- Define how `InGameCombatManager` should orchestrate combat after the roadmap is complete.
- Separate combat responsibilities into unit roster, target query, damage, shield/status, skill runtime/executor, projectile/effect/spawn simulation, and actor presentation.
- Keep current empty shells and future proposed classes clearly separated.

### Constraints

- Role Owner is Designer.
- No C# combat implementation or scene wiring in this task.
- Current `CombatRuntimeController` remains the active old runtime until a later Code Builder task wires InGame.

### Role Owner

Designer

### Status

Completed as combat architecture context.

### Next Actions

- Phase1-D should validate mapped InGame skill data before combat execution code is added.
- Later Code Builder work should keep `InGameCombatManager` as orchestration only and put target/damage/status/skill logic into narrowed services.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` currently exists as an empty `MonoBehaviour`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitRuntimeModel.cs` currently owns runtime identity/stat/resource/state references and auto flags.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` and `SkillBlueprintSpecs.cs` currently own static Blueprint skill data fields.
- `InGameSkillCatalog.cs` currently reuses `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)` and `PakuriDataManager` rather than adding a second CSV parser.
- `Select-String` found no current `TargetQueryService`, `DamageService`, `SkillExecutorRegistry`, `SkillRuntimeInstance`, `ProjectileSimulation`, or `StatusEffectService` class under `Pakuri/Assets/Scripts2/InGame`.
- Code Builder renamed the runtime script namespace from `Pakuri.CombatV2` to `Pakuri.InGame`, renamed the root folder to `Pakuri/Assets/Scripts2/InGame`, and updated the compile project references in `Pakuri/Assembly-CSharp.csproj`.
- Core combat entry/result/context/test-data shells now use `*Manager.cs` names: `InGameCombatManager.cs`, `InGameContextManager.cs`, `InGameResultManager.cs`, and `InGameTestDataManager.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; Unity-MCP reflection confirmed `Pakuri.InGame.InGameCombatManager` exists in the project assembly.

### History

- 2026-05-14: Designer documented the completed InGame combat runtime target structure, with proposed future services separated from currently existing shells.
- 2026-05-14: Code Builder performed the CombatV2-to-InGame rename and kept combat-side responsibilities unchanged.

## Task: 2026-05-14 InGame Prefab Storage Contract

### Task title

Record prefab storage roots for InGame units, enemies, and skills.

### Goals

- Store future player/monster unit prefabs under `Pakuri/Assets/Prefab/Monster`.
- Store future enemy prefabs under `Pakuri/Assets/Prefab/Enemy`.
- Store future skill or skill-effect prefabs under `Pakuri/Assets/Prefab/Skill`.

### Constraints

- Role Owner is Designer.
- No prefab authoring or asset moves in this task.

### Role Owner

Designer

### Status

Recorded as combat asset context.

### Next Actions

- Future Code Builder prefab work should use these folders as default destinations.
- Skill logic must remain in code/data; prefabs stored here should be visual/scene objects unless a later task explicitly defines a component responsibility.

### Evidence

- `Pakuri/Assets/Prefab` exists.
- `Pakuri/Assets/Prefab` contains `Enemy`, `Monster`, and `Skill` subfolders.
- User stated that future monster/player-unit, enemy, and skill prefabs will be stored under `Assets/Prefab`.

### History

- 2026-05-14: User clarified InGame prefab storage location.

## Task: 2026-05-14 NewRunScene InGame Scene Contract

### Task title

Record `NewRunScene` as the InGame test and final in-game scene contract.

### Goals

- Treat `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` as the scene used for InGame testing and the intended final in-game scene.
- Record the user-defined responsibilities for key scene objects.
- Use the named spawn points as the future player and manifested monster placement contract.

### Constraints

- Role Owner is Designer.
- No scene edits or runtime script wiring in this task.
- Object responsibilities are recorded as the user's design intent unless a component was directly confirmed in scene YAML.

### Role Owner

Designer

### Status

Recorded as scene contract context.

### Next Actions

- Code Builder scene-wiring work should target `NewRunScene.unity`.
- Before attaching scripts, verify current scene components through Unity-MCP or scene YAML and preserve existing object names.

### Evidence

- `Test-Path .\Pakuri\Assets\Scenes\NewScene\NewRunScene.unity` returned `True`.
- Scene YAML contains `m_Name: BG`, `1PSpawnPoint`, `2PSpawnPoint`, `3PSpawnPoint`, `4PSpawnPoint`, `5PSpawnPoint`, `GameManager`, and `Nexus`.
- Scene YAML shows `BG` has a `SpriteRenderer`.
- Scene YAML shows `Nexus` has a `SpriteRenderer`.
- Scene YAML currently shows `GameManager` as a GameObject with a `Transform`.
- User stated: `BG` is the background sprite; `1P~5PSpawnPoint` are the player and manifested monster spawn points; `GameManager` is the place for core game logic; `Nexus` is the nexus.

### History

- 2026-05-14: User declared `Assets/Scenes/NewScene/NewRunScene.unity` as the InGame test scene and intended main in-game scene.

## Task: 2026-05-14 InGame Phase1-C Skill Data Bridge

### Task title

Record combat-side test skill data bridge implementation.

### Goals

- Allow InGame to read existing active skills through current data loading and map them to InGame `SkillData`.
- Keep this bridge data-only, with no skill execution or combat loop behavior.
- Mark test loading timing explicitly as `Awake` on a test-only bootstrap.

### Constraints

- Role Owner is Code Builder.
- No combat scene edits, target registry, executor, damage resolver, or Play Mode verification.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Phase1-D validation should run before broad data entry or full skill execution.
- Code Reviewer requires explicit user permission.

### Evidence

- Added `InGameSkillCatalog.cs`, `InGameSkillDefinitionMapper.cs`, and `InGameTestDataManager.cs`.
- `InGameSkillCatalog` calls existing `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)` and `PakuriDataManager` instead of parsing CSV directly.
- `InGameTestDataManager` defaults to sample monster `ariel` and active slots `A`, `B`.
- Unity-MCP editor code execution returned `ariel-a:ProjectileSkillData|ariel-b:ShieldSkillData|sourceCatalog=True`.

### History

- 2026-05-14: Code Builder implemented Phase1-C after the user confirmed existing `Assets/Scripts/Data` should be the data-loading reference.

## Task: 2026-05-14 InGame Roadmap After Blueprint Data

### Task title

Record combat-side sequencing after Phase1-B.

### Goals

- Keep combat runtime implementation behind data connection and unit binding milestones.
- Use sample Blueprint skills before full skill data entry.
- Preserve the old combat controller until InGame has an independent minimum loop.

### Constraints

- Role Owner is Designer.
- No combat code or scene changes in this task.

### Role Owner

Designer

### Status

Completed as roadmap/report work.

### Next Actions

- Phase1-C should add a InGame catalog/mapper that reads from existing `SkillDefinition` data provided by the current data flow.
- Combat execution work starts later at Phase4 after unit creation and target/damage services are defined.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` currently has no combat loop.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` and `SkillBlueprintSpecs.cs` currently provide data structures only.
- `Pakuri/Assets/Scripts/Data/Runtime/PakuriDataManager.cs` currently resolves active and passive skills from `MonsterDefinition` / `GameDataCatalog`.

### History

- 2026-05-14: Designer produced a roadmap that places sample data connection before full data expansion.
- 2026-05-14: Designer chose existing CSV/Data reuse plus a thin InGame mapper over a new InGame CSV loader.

## Task: 2026-05-14 InGame Phase1-B Skill Blueprint Data

### Task title

Record combat-side Blueprint skill data structure implementation.

### Goals

- Define InGame skill data shapes for projectile, beam, zone, buff, shield, passive, status, and choice effects.
- Keep combat runtime behavior unimplemented while preparing data that later executors can read.
- Keep visual prefab references as presentation data, not execution logic.

### Constraints

- Role Owner is Code Builder.
- No combat loop, executor registry, damage resolver, target registry, scene binding, or Play Mode verification in this task.
- Code Reviewer execution still requires explicit user permission.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally compile verified.

### Next Actions

- Reviewer permission is required before the Code Reviewer stage.
- Next implementation should remain narrow: either executor interface shells or a compatibility bridge from current `SkillDefinition`, not full skill behavior.

### Evidence

- Added shared combat skill data specs in `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillBlueprintSpecs.cs`.
- Updated all InGame skill data ScriptableObject files under `Pakuri/Assets/Scripts2/InGame/Skills/Data/`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors.
- Unity-MCP final console read after force refresh showed no InGame script compile errors.

### History

- 2026-05-14: User asked Code Builder to start Phase1-B for new Blueprint skill data files.

## Task: 2026-05-14 InGame Scripts2 Skeleton

### Task title

Create minimal compileable InGame script shells under `Assets/Scripts2`.

### Goals

- Start InGame implementation with new scripts under `Pakuri/Assets/Scripts2/InGame`.
- Create unit, runtime context, and skill-data blueprint shells without gameplay logic.
- Follow the user's latest direction to begin from new `SkillData` blueprint subclasses before implementing a legacy `SkillDefinition` adapter.

### Constraints

- Role Owner is Code Builder.
- No Unity scene edits and no Play Mode verification in this task.
- Scripts must remain minimal shells and must not implement combat behavior yet.
- Code Reviewer is required after Builder implementation by user workflow, but Reviewer execution still needs explicit permission under `AGENTS_ROLE/GAMEBULIDER.md`.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally compiled.

### Next Actions

- Ask for explicit Code Reviewer permission before running the reviewer stage.
- Next Builder slice should add only one narrow binding layer, such as `UnitFactory` data mapping or one skill executor contract, after review.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `InGameContextManager.cs`, and `InGameResultManager.cs`.
- Added unit shells under `Pakuri/Assets/Scripts2/InGame/Units/`, including `MonsterUnitActor.cs`, `EnemyUnitActor.cs`, `UnitRuntimeModel.cs`, `UnitIdentity.cs`, `UnitStatsRuntime.cs`, `UnitResourceRuntime.cs`, `UnitStateBucket.cs`, `UnitFactory.cs`, and `UnitRosterService.cs`.
- Added skill blueprint shells under `Pakuri/Assets/Scripts2/InGame/Skills/Data/`, including `SkillData.cs`, `ProjectileSkillData.cs`, `BeamSkillData.cs`, `ZoneSkillData.cs`, `BuffSkillData.cs`, `ShieldSkillData.cs`, `PassiveSkillData.cs`, `StatusEffectData.cs`, and `SkillChoiceEffectSpec.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings for `System.Net.Http` and `System.IO.Compression`.

### History

- 2026-05-14: User explicitly switched to Code Builder and requested only file/minimal compileable script shells with no code logic.
- 2026-05-14: Code Builder created the `InGame` script skeleton under `Assets/Scripts2` and verified runtime assembly compilation.

## Task: 2026-05-14 InGame Unit And Skill Component Boundaries

### Task title

Record combat-domain component boundaries for InGame unit creation and skill execution.

### Goals

- Define proposed combat-side owners for unit factory, roster, target registry, skill adapter, skill runtime, executors, passive triggers, status, shield, damage, and presentation.
- Keep `SkillEffectPrefab` as presentation data only.
- Keep ordinary skills type-executor driven and reserve custom executors for explicit exceptions.

### Constraints

- Role Owner is Designer.
- No C# implementation or Unity scene edits in this task.
- Existing `CombatRuntimeController` remains current runtime until Code Builder implements InGame.

### Role Owner

Designer

### Status

Completed as design/report work.

### Next Actions

- First combat-side implementation should create only InGame contracts and model/unit factory skeletons.
- Later skill implementation should flow through `SkillSpecAdapter`, `SkillRuntimeInstance`, `SkillExecutorRegistry`, and type-specific executors.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-unit-skill-component-architecture.html`.
- `Pakuri/reference/skill-class-design.md:54`, `:81`, `:102`, `:130`, `:155`, and `:184` define the skill families used by the report.
- `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs:28` defines current `SkillRuntimeKind`, and `:63` defines current `SkillEffectPrefab`.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatEffectFactory.cs` already separates line/circle visual object creation from skill definitions, matching the report's visual/logic split.
- `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html:343` through `:344` already states skill executor logic and `SkillEffectPrefab` presentation ownership.

### History

- 2026-05-14: Designer created the component architecture report requested by the user.

## Task: 2026-05-13 InGame Foundation Architecture

### Task title

Record combat-domain foundation design for the new InGame runtime.

### Goals

- Define combat ownership for a new runtime without changing existing combat C#.
- Separate unit runtime state, Actor/View presentation, skill executors, status effects, projectile/effect/summon simulation, and spawn management.
- Keep 1P and manifested units on the same `MonsterUnitActor` path, with default-on auto attack that can be toggled per unit.

### Constraints

- Role Owner is Designer.
- No C# implementation or Unity scene edits in this task.
- Existing `CombatRuntimeController` remains the current runtime until a Code Builder task implements InGame.

### Role Owner

Designer

### Status

Completed as design/report work.

### Next Actions

- Code Builder should start with a minimal InGame vertical slice: model contracts, one monster actor, one enemy actor, and auto attack only.
- UI integration with `RunCombatUiController` remains deferred.

### Evidence

- Added `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` through the nested runtime definitions show current enemy/projectile/effect/drone runtime state is embedded in the old controller partial.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs` currently stores manifested unit HP, base damage, power stat, shield, Ariel/Vega/Rin/Sein timers, and skill runtimes.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatSkillRuntime.cs` currently stores skill cooldown, magazine, shot cooldown, reload, and queued Vega projectile state.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatEffectFactory.cs` already separates line/circle visual object creation from skill definitions, supporting InGame visual/logic separation.

### History

- 2026-05-13: User confirmed InGame should use model/view separation, shared 1P/manifested monster actor, auto attack toggles, and learned-choice lookup from unit state.

## Task: 2026-05-13 Phase 3-H Combat Simulation Closeout

### Task title

Close combat-side Phase 3 projectile/effect/drone refactoring.

### Goals

- Verify combat update order still reaches projectile, effect, manifested party, and selected unit logic.
- Confirm Phase 3 simulation boundaries are present for projectile, persistent skill-effect, selected drone, and manifested drone lifecycle.
- Mark enemy simulation as the next combat runtime split.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Completed and locally validated. Phase 3 combat-side refactoring is complete in Builder scope.

### Next Actions

- Begin Phase 4 `Enemy Simulation Split` when requested.
- User verifies RunScene combat behavior in Play Mode for projectiles, persistent effects, selected drones, and manifested drones.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:498` through `:505` still updates spawning, enemies, projectiles, monster skill runtime effects, manifested party combat, selected monster combat, selected status visuals, and battle resolution in order.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectileSimulation.cs:22` through `:25` now owns the projectile update boundary.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:22` through `:25` now owns the persistent skill-effect update boundary.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs:22` through `:35` now owns selected and manifested drone update/cleanup boundaries.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:49` still calls `owner.UpdateManifestedDrones()` before manifested unit combat ticking.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported all Phase 3 touched scripts, refreshed scripts to idle, and console read showed only MCP-FOR-UNITY connection/client-handler logs.

### History

- 2026-05-13: User asked Code Builder to start Phase 3-H and determine whether Phase 3 refactoring is finished.
- 2026-05-13: Builder verified combat-side boundaries and concluded Phase 3 is complete in Builder scope.

## Task: 2026-05-13 Phase 3-G Manifested Drone Simulation Alignment

### Task title

Move manifested drone combat ticking behind the drone simulation boundary.

### Goals

- Preserve manifested party combat update order while aligning manifested drone lifecycle ticking with `CombatRuntimeDroneSimulation`.
- Preserve manifested Eve drone target lookup, projectile firing, cooldown, duration, and cleanup behavior.
- Keep selected and manifested drone runtime classes separate.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not merge selected and manifested drone runtimes.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue to Phase 3-H as a separate closeout/ownership verification slice.
- User verifies manifested Eve Drone Beacon behavior in Play Mode if needed.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs:27` through `:35` keeps manifested drone update and cleanup compatibility entries.
- `CombatRuntimeDroneSimulation.cs:118` through `:160` preserves manifested drone reverse iteration, validity checks, duration, cooldown, target lookup, no-target retry, projectile fire, and cooldown reset.
- `CombatRuntimeDroneSimulation.cs:162` through `:183` preserves manifested drone cleanup and list removal.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:19` through `:49` still owns manifested Eve drone deployment and registration.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:49` still calls `owner.UpdateManifestedDrones()` before manifested unit combat ticking.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the touched scripts and final console read returned only MCP-FOR-UNITY client connection/client-handler logs.

### History

- 2026-05-13: User assigned Code Builder and requested Phase 3-G.
- 2026-05-13: Builder moved manifested drone ticking/projectile/cleanup behavior into `CombatRuntimeDroneSimulation.cs` while leaving deployment in the manifested party drone partial.

## Task: 2026-05-13 Phase 3-F Selected Drone Simulation Boundary

### Task title

Move selected Eve drone combat ticking behind a boundary.

### Goals

- Preserve combat update order while moving selected Eve drone lifecycle ticking into `CombatRuntimeDroneSimulation`.
- Preserve Drone Beacon firing cadence, projectile creation, and cleanup behavior.
- Keep manifested drone work for Phase 3-G.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not merge selected and manifested drone runtimes.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue to Phase 3-G as a separate manifested drone simulation alignment slice.
- User verifies selected Eve Drone Beacon behavior in Play Mode if needed.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs:22` through `:25` keeps `UpdateDrones()` as the selected drone update entry.
- `CombatRuntimeDroneSimulation.cs:36` through `:63` preserves reverse iteration, missing-drone removal, duration ticking, attack-period ticking, fire timing, destruction, and removal.
- `CombatRuntimeDroneSimulation.cs:65` through `:105` preserves selected drone projectile creation and `AddBattlefieldProjectile(...)` registration.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1200` still runs persistent effects before selected Eve drone ticking.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the touched scripts and final console read returned only MCP-FOR-UNITY client connection/client-handler logs.

### History

- 2026-05-13: User assigned Code Builder and requested Phase 3-F.
- 2026-05-13: Builder moved selected Eve drone ticking and projectile creation into `CombatRuntimeDroneSimulation.cs` while leaving manifested drones unchanged.

## Task: 2026-05-13 Phase 3-E Skill Effect Hit / Expiry Routing

### Task title

Split combat skill-effect hit and expiry routing helpers.

### Goals

- Make combat skill-effect hit routing readable without changing formulas or update order.
- Preserve Eve, Sein, Vega, and manifested effect behavior.
- Keep common temporary-effect migration out of this slice.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not move enemy simulation, selected-unit combat state, damage APIs, common targets, or common temporary effects.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue to Phase 3-F as a separate selected Eve drone simulation boundary slice.
- User verifies skill-effect behavior in Play Mode if needed.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:27` through `:39` now owns the skill-effect enemy tick dispatcher.
- `CombatRuntimeSkillEffectSimulation.cs:41` through `:56` separates enemy validity and shape checks.
- `CombatRuntimeSkillEffectSimulation.cs:58` through `:79` preserves hit routing order: Sein, Vega, manifested, Eve fallback.
- `CombatRuntimeSkillEffectSimulation.cs:81` through `:97` preserves Eve B slow and Eve C chill/freeze handling after Eve effect damage.
- `CombatRuntimeSkillEffectSimulation.cs:99` through `:102` separates expiry routing from the lifecycle loop.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the touched scripts and final console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-13: User assigned Code Builder and requested Phase 3-E.
- 2026-05-13: Builder split skill-effect hit/expiry routing helpers while preserving combat behavior.

## Task: 2026-05-13 Phase 3-D Skill Effect Simulation Boundary Shell

### Task title

Move combat skill-effect lifecycle ticking behind a boundary.

### Goals

- Preserve combat update order while moving skill-effect lifecycle ticking behind `CombatRuntimeSkillEffectSimulation`.
- Preserve effect duration, tick interval, hit-set clearing, expiry handling, destruction, and removal behavior.
- Keep skill-specific damage/status callbacks unchanged for this slice.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not move enemy simulation, selected-unit combat state, damage APIs, common targets, or common temporary effects.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue to Phase 3-E as a separate skill-effect hit/expiry routing split.
- User verifies skill-effect behavior in Play Mode if needed.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:500` through `:503` still calls projectiles, monster skill runtime effects, manifested party combat, and selected combat in the same order.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:22` through `:25` keeps the existing persistent effect update entry point and routes it through the new boundary.
- `CombatRuntimeSkillEffectSimulation.cs:36` through `:64` owns the moved `skillEffects` lifecycle loop and preserves `HitThisTick.Clear()`, callback, expiry, destroy, and remove order.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1200` still updates persistent effects before selected Eve drones.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the new script and final console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-13: User assigned Code Builder and requested Phase 3-D.
- 2026-05-13: Builder moved skill-effect lifecycle ticking behind a boundary while preserving combat update order.

## Task: 2026-05-13 Phase 3-C Projectile Hit Routing Helpers

### Task title

Split combat projectile hit routing by source type.

### Goals

- Make projectile hit routing readable by separating enemy, manifested, and selected/player projectile paths.
- Preserve combat update order, projectile cleanup/lifetime boundary, and all damage/status behavior.
- Keep damage application APIs and common target ownership unchanged.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not move enemy simulation, selected-unit combat state, damage APIs, or common target/effect state.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue to Phase 3-D as a separate skill-effect simulation boundary slice.
- User verifies projectile behavior in Play Mode if needed.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:31` through `:43` dispatches projectile processing by source type.
- `CombatRuntimeProjectiles.cs:47` through `:62` handles enemy projectile routing.
- `CombatRuntimeProjectiles.cs:64` through `:93` handles manifested projectile routing.
- `CombatRuntimeProjectiles.cs:95` through `:175` handles selected/player projectile routing and enemy-hit follow-up.
- `CombatRuntimeProjectiles.cs:394`, `:422`, and `:452` still own enemy, selected, and manifested damage application methods.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; final console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-13: User assigned Code Builder and requested Phase 3-C.
- 2026-05-13: Builder split projectile hit routing helpers while preserving combat update order and damage ownership.

## Task: 2026-05-13 Phase 3-B Projectile Cleanup Lifetime Ownership

### Task title

Move projectile cleanup, lifetime, and X-edge checks behind the projectile simulation boundary.

### Goals

- Keep combat update order stable while moving projectile cleanup/lifetime responsibility into the Phase 3 projectile boundary.
- Preserve current projectile hit routing, status application, pierce handling, and damage calls.
- Keep the slice limited to cleanup/lifetime/edge ownership.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not move enemy simulation, selected-unit combat, damage APIs, or common target/effect state.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue to Phase 3-C as a separate source-specific projectile hit routing split.
- User verifies projectile behavior in Play Mode if needed.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectileSimulation.cs:53` through `:66` owns missing projectile removal.
- `CombatRuntimeProjectileSimulation.cs:68` through `:80` owns lifetime ticking and remaining-lifetime checks.
- `CombatRuntimeProjectileSimulation.cs:83` through `:104` owns projectile X-edge checks.
- `CombatRuntimeProjectileSimulation.cs:106` through `:120` owns cleanup, `Object.Destroy`, and list removal.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:31` through `:154` still preserves enemy, manifested, and selected/player hit branch order.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; final console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-13: User assigned Code Builder and requested Phase 3-B after Phase 3-A.
- 2026-05-13: Builder moved projectile cleanup/lifetime/edge responsibilities behind the projectile boundary while preserving combat update and hit branch order.

## Task: 2026-05-13 Phase 3-A Projectile Simulation Boundary Shell

### Task title

Add a combat-runtime projectile simulation boundary shell.

### Goals

- Keep the combat update order stable while introducing the first Phase 3 projectile simulation boundary.
- Preserve current projectile movement, collision, hit routing, status application, pierce, and cleanup behavior.
- Keep Phase 3-A limited to a wrapper-level boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not move enemy simulation, selected-unit combat, damage APIs, or common target/effect state.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue to Phase 3-B only as a separate cleanup/lifetime ownership slice.
- User verifies projectile behavior in Play Mode if needed.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectileSimulation.cs`.
- `CombatRuntimeProjectileSimulation.cs:20` through `:22` keeps the `UpdateProjectiles()` call target and routes it through `ProjectileSimulationBoundary.Tick()`.
- `CombatRuntimeProjectileSimulation.cs:34` through `:36` calls `owner.UpdateProjectilesCore()`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` keeps the old projectile loop as `UpdateProjectilesCore()`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:500` still places projectile ticking after `UpdateEnemies()` and before `UpdateMonsterSkillRuntimeEffects()`.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; final console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-13: User assigned Code Builder and requested Phase 3-A start from the refactoring board.
- 2026-05-13: Builder added the projectile boundary shell and preserved the existing combat update order.

## Task: 2026-05-13 Phase 3 Combat Runtime Work Breakdown

### Task title

Record the combat-runtime execution plan for Phase 3 projectile/effect/drone simulation split.

### Goals

- Keep the top-level combat update order stable during Phase 3.
- Split projectile, skill-effect, and drone lifecycle ownership into small Code Builder slices.
- Preserve existing selected, manifested, enemy, and skill-specific damage/status behavior.
- Prevent Phase 3 from absorbing Phase 4 enemy simulation, Phase 5 selected-unit combat, Phase 6 skill reuse, or Phase 7 common target/effect migration.

### Constraints

- Role Owner is Designer.
- Planning only; no runtime C# behavior changes.
- Code Builder must start with Phase 3-A only if implementation begins.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer

### Status

Completed. Combat-domain Phase 3 plan is 3-A through 3-H.

### Next Actions

- Phase 3-A should introduce only a projectile simulation boundary shell around the existing projectile update path.
- Update this board after each Phase 3 implementation slice with build and Unity-MCP evidence.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:496` through `:506` shows the current top-level order: spawning, enemies, projectiles, monster skill runtime effects, manifested party combat, selected monster combat, visuals, and battle resolution.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` through `:154` mixes projectile movement, source-specific hit routing, status application, pierce, edge cleanup, and cleanup calls in one loop.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:177` through `:184` drives selected monster skill-effect updates through monster runtime adapters.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1230` owns the shared skill-effect lifetime loop behind Eve's selected skill update path.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1286` through `:1342` owns selected Eve drone ticking and drone projectile spawning.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:51` through `:92` owns manifested drone ticking separately from selected Eve `DroneRuntime`.

### Phase 3 Slice Summary

- 3-A: Projectile simulation boundary shell.
- 3-B: Projectile cleanup and lifetime ownership.
- 3-C: Projectile hit routing helpers by source type.
- 3-D: Skill-effect simulation boundary shell.
- 3-E: Skill-effect hit and expiry routing.
- 3-F: Selected Eve drone simulation boundary.
- 3-G: Manifested drone simulation alignment.
- 3-H: Phase 3 closeout verification and Phase 4 handoff.

### History

- 2026-05-13: User asked Designer to decide the Phase 3 `3-[A-Z]` work split and record it before implementation.

## Task: 2026-05-13 Shared Target And Temporary Effect Roadmap Amendment

### Task title

Record combat-domain timing for shared target, temporary effects, and skill reuse after Phase 2-E.

### Goals

- Keep Phase 3 as the next combat implementation step.
- Place reusable temporary effects in Phase 7 after common target read adapters.
- Place same-type skill reuse in Phase 6 after projectile/effect/drone and adapter boundaries are safer.
- Place Monster / Enemy common actor or prefab authoring after target/effect stabilization.

### Constraints

- Role Owner is Designer.
- This is a report/roadmap amendment only.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Phase 3 remains `Projectile / Effect / Drone Simulation Split`.
- Do not migrate shield/status/action speed into a common temporary-effect system until Phase 7.
- Do not use prefab or inheritance as the first common combat-state model.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` with explicit Phase 7 and Phase 8 timing.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:375` through `:376` defines success as selected ally, summoned ally, and enemy all reading common channels and effect apply/update/expire/stack being reproduced in a common effect layer.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:327` through `:328`, `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:45`, and `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:738` through `:748` show current selected, manifested, and enemy effect state still stored/ticked separately.

### History

- 2026-05-13: User requested the roadmap HTML be amended with explicit timing for the shared combat target / temporary effect proposal and the proposed skill reuse / common actor direction.

## Task: 2026-05-13 Phase 2 Manifested Party Closeout Verification

### Task title

Verify Phase 2 manifested party closeout and next combat-runtime phase.

### Goals

- Inspect remaining manifested party special-case methods after Phase 2-E.
- Confirm whether another Phase 2 helper split is necessary before Phase 3.
- Confirm runtime/editor compile status without changing combat code.

### Constraints

- Role Owner is Code Builder.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Completed. Phase 2 is closable and should hand off to Phase 3 by default.

### Next Actions

- Begin Phase 3 with a small projectile/effect/drone simulation boundary slice.
- Leave Rin shockwave, persistent/frost field, and Vega queued projectile special cases in place until a later owner boundary gives them a cleaner home.
- User still owns Play Mode verification for manifested special-skill behavior.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:351` through `:443` keeps `TryFireManifestedRinShockwave(...)` as a Rin-specific formula/effect/damage/knockback/reload-reduction path.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:469` through `:508` keeps generic field routing and persistent field dispatch that is still called from `CombatRuntimeManifestedPartyDamage.cs:22`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:512` through `:564` keeps `CreateManifestedEveFrostField(...)` as an Eve-specific field effect with chill/freeze choices and skill-effect registration.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:701` through `:744` keeps Vega queued projectile sequencing tied to `CombatSkillRuntime` pending projectile fields.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs:82` and `:115` call `UpdateManifestedQueuedProjectiles(...)` and `QueueManifestedVegaThreeSwordFlurry(...)`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs:9` through `:22` already owns the generic manifested skill fire entry point and delegates special cases first.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings.
- Unity-MCP refresh returned idle; console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to perform the Phase 2 closeout verification.

## Task: 2026-05-13 Manifested Party Damage Projectile Helper Split

### Task title

Separate generic manifested damage and projectile-fire helper methods from the party manager partial.

### Goals

- Move generic manifested skill fire, projectile fire, projectile hit/status, generic damage application, and damage/projectile resolver helpers out of `CombatRuntimeParty.cs`.
- Preserve existing `CombatUnitRuntime` skill dispatch, manifested projectile registration, hit resolution, status application, and damage calculation behavior.
- Keep monster-specific special formulas, including Rin shockwave and Eve frost field, in the existing party partial for this slice.

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
- User verifies manifested projectile fire, projectile hit/status behavior, generic Offering-learned skill damage, Rin shockwave, Eve frost field, and Vega queued projectile behavior in RunScene Play Mode.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs`.
- `CombatRuntimeManifestedPartyDamage.cs:9` owns generic manifested non-projectile skill fire.
- `CombatRuntimeManifestedPartyDamage.cs:63`, `:81`, `:112`, and `:124` own manifested projectile fire entry points, pierce resolution, and projectile object/runtime creation.
- `CombatRuntimeManifestedPartyDamage.cs:184`, `:236`, `:258`, and `:289` own manifested projectile hit resolution, source follow-up effects, area follow-up damage, and projectile status application.
- `CombatRuntimeManifestedPartyDamage.cs:311`, `:316`, `:335`, `:368`, `:451`, `:458`, `:465`, `:476`, and `:483` own generic manifested skill damage, effect damage, base damage, damage multiplier, projectile speed, projectile lifetime, projectile hit radius, and status chance helpers.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:351`, `:490`, `:512`, and `:739` retain monster-specific Rin shockwave, persistent field, Eve frost field, and queued Vega projectile call-site behavior.
- `Pakuri/Assembly-CSharp.csproj:81` includes the new script in the C# project.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the new script with guid `87b85aa0eb1d47849e4ae88329a740ef`; editor state returned `ready_for_tools=true`, and console warning/error read returned only MCP client handler logs after refresh.
- `git diff --check` completed with no whitespace errors and LF-to-CRLF warnings only.
- External Phase 2 Code Reviewer output was saved to `codex_loop_logs\phase2_manifested_party_reviewer_20260513.md` and ended with `REVIEW_RESULT: PASS`.
- Reviewer evidence found no missing referenced helper, duplicate method definition, new null-risk regression, or behavior-order regression in the moved Phase 2 code.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to perform Phase 2-E `Manifested Party Damage / Projectile Fire Helper` split and run Code Reviewer for Phase 2.
- 2026-05-13: External Code Reviewer returned `REVIEW_RESULT: PASS` for the Phase 2 manifested party refactor.

## Task: 2026-05-13 Manifested Party Skill Visual Helper Split

### Task title

Separate manifested non-drone skill visual and scene-object helper methods from the party manager partial.

### Goals

- Move manifested skill visual duration, circle visual, line visual, and shared visual configuration helpers out of `CombatRuntimeParty.cs`.
- Preserve existing skill-fire call sites, damage formulas, projectile behavior, and monster-specific visual durations.
- Keep the slice limited to non-drone visual helper responsibility after the Phase 2-C drone lifecycle split.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue Phase 2 by moving remaining manifested damage/formula or projectile/skill-fire helpers only in separate reviewable slices.
- User verifies Manifested Offering-learned and monster-specific skill visuals in RunScene Play Mode.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyVisuals.cs`.
- `CombatRuntimeManifestedPartyVisuals.cs:9` owns manifested non-drone skill visual dispatch.
- `CombatRuntimeManifestedPartyVisuals.cs:60` owns manifested skill visual duration resolution.
- `CombatRuntimeManifestedPartyVisuals.cs:120`, `:132`, and `:154` own circle visual creation, line visual creation, and shared visual configuration.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:371`, `:401`, and `:757` remain call sites for the moved helpers.
- `Pakuri/Assembly-CSharp.csproj:85` includes the new script in the C# project.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the new script with guid `6f30f996b52c4bb4ea62492d3b619c4c`; after script refresh, console warning/error read returned only MCP client handler logs.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 2-D `Manifested Party Skill Visual / Scene Object Helper` split.

## Task: 2026-05-13 Manifested Party Drone Lifecycle Split

### Task title

Separate manifested Eve drone runtime and lifecycle helpers from the party manager partial.

### Goals

- Move `ManifestedDroneRuntime` out of `CombatRuntimeParty.cs`.
- Move manifested Eve drone deployment, ticking, projectile firing, and cleanup into a dedicated partial.
- Preserve existing manifested drone list storage through `ManifestedPartyRuntime`.
- Keep non-drone manifested damage formulas and skill visuals unchanged in this slice.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue Phase 2 by moving remaining manifested damage/formula or non-drone scene-object helpers only in separate reviewable slices.
- User verifies Manifested Eve drone beacon lifetime, firing cadence, cleanup, and projectile behavior in RunScene Play Mode.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs`.
- `CombatRuntimeManifestedPartyDrones.cs:8` through `:16` owns `ManifestedDroneRuntime`.
- `CombatRuntimeManifestedPartyDrones.cs:19` through `:48` owns manifested Eve drone deployment and `manifestedDrones.Add(...)`.
- `CombatRuntimeManifestedPartyDrones.cs:51` through `:92` owns manifested drone ticking and projectile firing.
- `CombatRuntimeManifestedPartyDrones.cs:95` through `:115` owns manifested drone cleanup.
- `CombatRuntimeParty.cs:16` now starts at `ManifestedMonsterSlotNames`, so the drone runtime type is no longer in that file.
- `CombatRuntimeParty.cs:1586` still calls `RemoveManifestedDroneAt(i)` during party clear.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the new script and console warning/error read returned only MCP client handler logs after refresh.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to start the remaining Phase 2 work.

## Task: 2026-05-13 Manifested Party Skill Dispatcher Split

### Task title

Separate manifested party unit skill dispatch from the party manager partial.

### Goals

- Move manifested party unit skill dispatch and fallback cooldown/magazine ticking out of `CombatRuntimeParty.cs`.
- Keep `CombatUnitRuntime`'s existing `Owner.TickManifestedUnitSkill(...)` callback stable for this slice.
- Preserve Eve, Rin, Sein, Vega, Ariel, and generic manifested fallback dispatch order.
- Leave manifested damage formulas and scene object firing methods in the existing party partial for behavior preservation.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue Phase 2 by narrowing remaining manifested party callbacks or moving later formula/spawn responsibilities only in separate reviewable slices.
- User verifies Manifested Eve/Rin/Sein/Vega/Ariel skills and generic Offering-learned skills in RunScene Play Mode.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs`.
- `CombatRuntimeManifestedPartySkills.cs:5` preserves the existing `TickManifestedUnitSkill(...)` callback used by `CombatUnitRuntime`.
- `CombatRuntimeManifestedPartySkills.cs:10` through `:71` owns manifested unit skill dispatch order: Eve, Rin, Sein, Vega, Ariel, then generic fallback.
- `CombatRuntimeManifestedPartySkills.cs:74` through `:139` owns fallback ticking, queued projectile ticking, reload ticking, and magazine fire dispatch.
- `CombatRuntimeManifestedPartyRuntime.cs:64` through `:71` routes per-skill dispatch through `ManifestedPartyRuntime.TickUnitSkill(...)`.
- `CombatRuntimeParty.cs:362` now starts at `FireManifestedMonsterSkill(...)`, so damage/formula and object firing methods stayed in the existing party partial.
- `CombatUnitRuntime.cs:193` still calls `Owner.TickManifestedUnitSkill(this, Skills[i], elapsed)`.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the new script and console warning/error read returned only MCP client handler logs after refresh.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to perform Phase 2-B `Manifested Party Skill Dispatcher` split.

## Task: 2026-05-13 Manifested Party View Binder Split

### Task title

Separate manifested party view binding and HP/shield refresh helpers.

### Goals

- Move manifested party status-view binding out of `CombatRuntimeParty.cs`.
- Keep scene-authored 2P-5P slot child names and fallback HP/shield bar behavior unchanged.
- Preserve combat skill dispatch, damage formulas, and RunScene UI-facing APIs.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue Phase 2 with manifested party skill dispatch extraction in a separate slice.
- User verifies manifested name/HP/shield labels and bars in RunScene Play Mode.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs`.
- `CombatRuntimeManifestedPartyView.cs:23` through `:52` resolves scene-authored manifested status children.
- `CombatRuntimeManifestedPartyView.cs:55` through `:141` owns fallback and live HP/shield bar repair helpers.
- `CombatRuntimeManifestedPartyView.cs:256` through `:302` owns manifested name, HP text, fallback label, and HP/shield bar refresh.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:194`, `:197`, `:224`, `:300`, `:334`, and `:1851` still call the same helper names from the party flow.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP imported the new script and console warning/error read returned only MCP client handler logs after refresh.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to perform Phase 2-A `Manifested Party View Binder` split.

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
