# REFACTORING

This board records broad refactoring plans that cut across combat, monster runtime, data flow, UI, and reports.

When doing related work, follow `MDTREE.md` routing and update this file together with the affected domain boards.

## Task: 2026-05-14 InGame Phase2-A Eve Unit Model Mapping

### Task title

Implement Phase2-A base unit runtime model split for InGame.

### Goals

- Move `UnitFactory` from an empty shell to a data-definition to `BaseUnitRuntimeModel` family mapper.
- Use Eve as the selected monster sample and one stage-one enemy as the enemy sample.
- Extract only unit-common stats/resources/defenses from existing definitions in this slice.
- Keep Phase2-A model-only so Phase2-B can handle actor binding separately.

### Constraints

- Role Owner is Code Builder.
- No combat loop, skill execution, target/damage/status services, code-generated prefab authoring, scene edit, or Play Mode verification.
- Keep existing `MonsterDefinition` skill/projectile tuning out of the unit model; split it later through the SkillData mapper when skill implementation starts.
- Do not replace existing CSV/Data loading.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Phase2-B can start from model-to-actor binding for user-authored `Monster` / `Enemy` prefabs with `MonsterUnitActor` and `EnemyUnitActor`.
- Do not add combat execution until Phase3/Phase4 service boundaries exist.

### Evidence

- Updated `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitRuntimeModel.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitRuntimeModel.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Units/UnitDefenseRuntime.cs`.
- Updated `Pakuri/Assets/Scripts2/InGame/Units/UnitRuntimeModel.cs`.
- Updated `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs`.
- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html` to mark Phase2-A complete, record no code-generated prefabs, and defer skill/projectile tuning to later SkillData mapper work.
- `UnitFactory` now creates Eve as `MonsterUnitRuntimeModel` and `stage1-swordsman` as `EnemyUnitRuntimeModel`.
- `BaseUnitRuntimeModel` now owns common identity, stats, defenses, resources, and auto flags.
- Unity-MCP Editor code execution returned `ok=True|monster=eve|monsterHp=220|learnedA=1|enemy=stage1-swordsman|enemyHp=100`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle; console warning/error read returned only MCP client handler logs, not C# compile errors.

### History

- 2026-05-14: User explicitly assigned Code Builder and requested Phase2-A implementation with Eve as the monster.
- 2026-05-14: User changed Phase2-A direction from direct `UnitRuntimeModel` mapping to a `BaseUnitRuntimeModel` / `MonsterUnitRuntimeModel` / `EnemyUnitRuntimeModel` split and confirmed prefabs are created manually in Unity Editor.

## Task: 2026-05-14 InGame Phase1-D Skill Data Validation

### Task title

Implement Phase1-D validation for mapped InGame skill data.

### Goals

- Detect duplicate active `SkillId` and passive `PassiveId` values before broad skill data expansion.
- Detect empty IDs, empty names, character/owner ID prefix mismatches, active/passive slot mismatches, unsupported active runtime kinds, and required timing gaps.
- Provide an editor entry point so validation can be run before later InGame phases.

### Constraints

- Role Owner is Code Builder.
- Do not edit CSV source data, ScriptableObject assets, scene objects, or skill execution logic in this phase.
- Keep validation data-only and reusable outside the editor menu.
- Do not run Unity Play Mode; user owns gameplay verification.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Phase2-A can start after Reviewer permission or after the user accepts the local build evidence.
- Run Unity Editor menu `Pakuri/InGame/Validate Skill Data` when Unity is available to view validation logs in the console.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDataValidator.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Editor/InGameSkillDataValidationMenu.cs`.
- Updated `Pakuri/Assembly-CSharp.csproj` and `Pakuri/Assembly-CSharp-Editor.csproj` with the new compile entries.
- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html` to mark Phase1-D complete and point to the new InGame validator/menu files.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings after rerunning it alone; the earlier parallel runtime build hit only an `obj\Debug\Assembly-CSharp.dll` file lock.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP `refresh_unity` could not run because no Unity Editor instance was found.

### History

- 2026-05-14: User asked Code Builder to start Phase1-D unless it was already implemented or unnecessary.
- 2026-05-14: Code Builder confirmed no InGame validator existed, then added runtime validation and an editor menu hook.

## Task: 2026-05-14 InGame Final Ingame Structure

### Task title

Record the final InGame ingame architecture target after the roadmap is completed.

### Goals

- Define the completed InGame class responsibility map across Run entry, data bridge, unit model/actor, skill data/runtime/executor, target/damage/status services, simulation, scene anchors, and assets.
- Mark currently existing shells separately from proposed future classes.
- Preserve the roadmap requirement that the old combat and Run flow remain available until InGame proves a minimum independent loop.

### Constraints

- Role Owner is Designer.
- No runtime C# implementation, scene wiring, prefab creation, or data migration in this task.
- Proposed classes must not be treated as existing implementation until Code Builder creates them.

### Role Owner

Designer

### Status

Completed as architecture/report work.

### Next Actions

- Continue with Phase1-D validation before broad data entry or skill execution work.
- Use `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html` as the target responsibility map for later Phase2-A through Phase8-A Code Builder tasks.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html`.
- Current `InGameCombatManager.cs`, `InGameContextManager.cs`, and `InGameResultManager.cs` are empty shells, so the report labels the final loop as target structure, not current behavior.
- Current `UnitRuntimeModel.cs` stores identity, stats, resources, state, and auto flags.
- Current `MonsterUnitActor.cs`, `EnemyUnitActor.cs`, `UnitFactory.cs`, and `UnitRosterService.cs` exist but have no behavior.
- Current `InGameSkillCatalog.cs` and `InGameSkillDefinitionMapper.cs` implement the Phase1-C data bridge from existing `SkillDefinition` / `PassiveDefinition` into InGame `SkillData`.
- Scene YAML evidence confirms `NewRunScene` anchors: `BG`, `1PSpawnPoint` through `5PSpawnPoint`, `GameManager`, and `Nexus`.
- Updated the final ingame structure report so its first section now describes the target refactoring outcome: common unit inheritance/target contracts for buffs, action speed, shields, and status effects, plus reusable Blueprint-driven `SkillData` implementation according to `C:\TowerDefence_Pakuri\towerdefense_pakuri_docs\docs\dev\skill-class-design.md`.
- Renamed the script root from `Pakuri/Assets/Scripts2/CombatV2` to `Pakuri/Assets/Scripts2/InGame`, preserved matching `.meta` files, and renamed core scripts to `InGameCombatManager.cs`, `InGameContextManager.cs`, `InGameResultManager.cs`, and `InGameTestDataManager.cs`.
- Renamed prefixed skill bridge scripts from `CombatV2SkillCatalog.cs` / `CombatV2SkillDefinitionMapper.cs` to `InGameSkillCatalog.cs` / `InGameSkillDefinitionMapper.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` conflict warnings.
- Unity-MCP `refresh_unity` completed after the rename; latest console read showed no `CS2001` missing-source error, and `unity_reflect` found `Pakuri.InGame.InGameCombatManager` in `Assembly-CSharp`.

### History

- 2026-05-14: User asked what the completed ingame structure and class interactions would look like if the InGame roadmap is fully implemented.
- 2026-05-14: User asked to replace the report’s current-state evidence section with wording that the common unit hierarchy solves the 2026-05-10 shared combat target/temporary effect issue and that Blueprint-style skill classes solve previous skill hardcoding.
- 2026-05-14: User asked Code Builder to rename the CombatV2 script tree and class references to InGame, with core role scripts using `*Manager.cs` naming.

## Task: 2026-05-14 InGame Asset Storage Contract

### Task title

Record asset storage destinations for future InGame implementation.

### Goals

- Use existing `Pakuri/Assets/Prefab` subfolders for future prefab creation.
- Use existing `Pakuri/Assets/SO` for future ScriptableObject data assets.
- Keep folder roles stable before Code Builder begins prefab or SO asset authoring.

### Constraints

- Role Owner is Designer.
- No asset authoring or folder mutation in this task.

### Role Owner

Designer

### Status

Recorded as refactoring asset context.

### Next Actions

- Future Code Builder work should place prefabs and SO data in these folders unless a later user instruction changes the contract.

### Evidence

- `Pakuri/Assets/Prefab` exists and contains `Enemy`, `Monster`, and `Skill`.
- `Pakuri/Assets/SO` exists.
- User stated `Assets/Prefab` will store monster/player-unit, enemy, and skill prefabs.
- User stated `Assets/SO` will store SO data.

### History

- 2026-05-14: User declared the future prefab and ScriptableObject storage layout.

## Task: 2026-05-14 NewRunScene InGame Integration Target

### Task title

Record the scene integration target for InGame refactoring.

### Goals

- Set `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` as the future integration target for InGame.
- Keep the current scene object names as stable anchors for future wiring.
- Avoid creating a parallel test-only scene unless the user explicitly asks for one.

### Constraints

- Role Owner is Designer.
- No scene edits or code changes in this task.
- Future integration must preserve existing names unless a later task explicitly renames objects.

### Role Owner

Designer

### Status

Recorded as integration target context.

### Next Actions

- Future Code Builder scene work should attach or bind InGame systems through `GameManager` in `NewRunScene`.
- Spawn placement should use `1PSpawnPoint` through `5PSpawnPoint`.
- The `Nexus` object should be treated as the nexus target/structure in later combat rules.

### Evidence

- Scene file exists at `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity`.
- Scene YAML names confirm `BG`, `1PSpawnPoint`, `2PSpawnPoint`, `3PSpawnPoint`, `4PSpawnPoint`, `5PSpawnPoint`, `GameManager`, and `Nexus`.
- Scene YAML shows `BG` and `Nexus` have `SpriteRenderer` components.
- User stated this scene is the testing scene and final in-game scene.

### History

- 2026-05-14: User defined `NewRunScene` object responsibilities for InGame.

## Task: 2026-05-14 InGame Phase1-C Data Bridge

### Task title

Implement the Phase1-C test data bridge from existing CSV/Data to InGame skill data.

### Goals

- Reuse existing `PakuriCsvRuntimeData`, `GameDataCatalog`, and `PakuriDataManager` data loading instead of creating a new InGame CSV loader.
- Convert 1-2 existing `SkillDefinition` samples into InGame `SkillData` runtime test objects.
- Explicitly mark the data load timing as test-only and separate from the production MainMenuScene / RunStartContext flow.

### Constraints

- Role Owner is Code Builder.
- No CSV edits, ScriptableObject asset creation, scene edits, skill execution logic, or Play Mode verification in this task.
- Existing unrelated dirty files must not be reverted.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Run Code Reviewer only after explicit user permission.
- Phase1-D should add validation rules around duplicate IDs, empty IDs, unsupported runtime kinds, and required field gaps.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillCatalog.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs`.
- `InGameTestDataManager` tooltip states that loading in `Awake` is test-only and that production should keep the `MainMenuScene` / `RunStartContext` handoff.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP script refresh completed with no InGame compile errors.
- Unity-MCP editor code execution returned `ariel-a:ProjectileSkillData|ariel-b:ShieldSkillData|sourceCatalog=True`.

### History

- 2026-05-14: User requested Phase1-C as Code Builder and asked to clarify data loading timing for testing.

## Task: 2026-05-14 InGame Build Roadmap

### Task title

Record the implementation order after Phase1-B Blueprint data.

### Goals

- Establish that testing needs sample data, but full 50-skill data entry should wait until the skeleton and execution path are stable.
- Define Phase1-C through Phase8-A from sample data connection to Run integration.
- Keep the existing combat runtime and Run flow untouched until InGame proves an independent minimum loop.

### Constraints

- Role Owner is Designer.
- No runtime C# implementation, Unity scene edit, ScriptableObject asset creation, or data migration in this task.

### Role Owner

Designer

### Status

Completed as roadmap/report work.

### Next Actions

- Code Builder next recommended slice is Phase1-C: reuse current `PakuriCsvRuntimeData` / `PakuriDataManager` / `GameDataCatalog` flow and add a thin InGame skill catalog/mapper for 1-2 sample skills.
- Do not enter full data production before sample data lookup, data validation, and minimum execution are proven.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Current `InGameCombatManager.cs` and `MonsterUnitActor.cs` are still empty shells.
- Current `SkillData.cs` and `SkillBlueprintSpecs.cs` define data-only Blueprint structures.
- Existing `MonsterDefinition.cs` still references legacy `SkillDefinition[] ActiveSkills` and `PassiveDefinition[] PassiveSkills`.
- Existing `RunSceneBootstrap.cs` still calls the old `CombatRuntimeController.BeginConfiguredDay(...)` path.
- Existing `PakuriCsvRuntimeData.Build.cs` builds current runtime catalogs from CSV, so InGame should not duplicate CSV parsing for Phase1-C.

### History

- 2026-05-14: User asked whether data should be connected before testing; Designer answered sample data first, full data later.
- 2026-05-14: User requested the work roadmap as HTML.
- 2026-05-14: User proposed using existing `Assets/Scripts/Data` as the reference for the first connected sample data; Designer agreed.

## Task: 2026-05-14 InGame Phase1-B Blueprint Skill Data

### Task title

Implement Phase1-B data-only Blueprint skill structures.

### Goals

- Move the new InGame skill skeleton from empty shells to reusable data-only Blueprint definitions.
- Centralize repeated blueprint fields in reusable serializable spec classes.
- Preserve existing runtime, UI flow, CSV loading, and scene behavior.

### Constraints

- Role Owner is Code Builder.
- This slice does not add executors, adapters, runtime behavior, Unity scene objects, or ScriptableObject asset instances.
- Existing unrelated dirty files must not be reverted.

### Role Owner

Code Builder

### Status

Builder implementation completed and verified with local compile checks.

### Next Actions

- Run Code Reviewer only after explicit user permission.
- Keep the next phase small: bridge current `SkillDefinition` to the Blueprint shape, or add executor contract shells.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillBlueprintSpecs.cs`.
- Updated the existing InGame skill data files under `Pakuri/Assets/Scripts2/InGame/Skills/Data`.
- `git diff --check -- Pakuri/Assets/Scripts2/InGame/Skills/Data Pakuri/Assembly-CSharp.csproj` completed with no output.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings.
- Unity-MCP `refresh_unity` with `mode=force` imported the new common spec file; final console read showed no skill-data compile errors.

### History

- 2026-05-14: User requested Phase1-B implementation as Code Builder.
- 2026-05-14: Code Builder wrote reusable Blueprint skill data structures without runtime skill logic.

## Task: 2026-05-14 InGame Scripts2 Skeleton Implementation

### Task title

Start InGame implementation with minimal compileable shells under `Assets/Scripts2`.

### Goals

- Add a new `InGame` script tree without touching the existing combat runtime.
- Create core context/result/controller shells, shared unit shells, and Blueprint-first skill data shells.
- Keep current Run UI flow, CSV/Data loading, and scene state untouched in this first Builder slice.

### Constraints

- Role Owner is Code Builder.
- No gameplay logic, UI logic, scene binding, data migration, or Play Mode verification in this task.
- Existing uncommitted scene/meta changes are not part of this Builder task and must not be reverted.

### Role Owner

Code Builder

### Status

Builder implementation completed and compile verified.

### Next Actions

- Obtain explicit permission before running Code Reviewer.
- After review, choose one small next slice: data-to-unit factory mapping, skill executor interface shells, or a new scene bootstrap shell.

### Evidence

- Added new scripts under `Pakuri/Assets/Scripts2/InGame/Core`, `Pakuri/Assets/Scripts2/InGame/Units`, and `Pakuri/Assets/Scripts2/InGame/Skills/Data`.
- `Get-ChildItem -Path .\Pakuri\Assets\Scripts2\InGame -Recurse -Filter *.cs` listed 21 new C# files.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings.
- `git status --short -- Pakuri/Assets/Scripts2 Pakuri/Assembly-CSharp.csproj` showed the new `Pakuri/Assets/Scripts2/InGame/` folder as untracked and pre-existing deleted `Scripts2` meta files unrelated to this task.

### History

- 2026-05-14: User asked Code Builder to create only the required scripts first under `Assets/Scripts2`, with no code logic beyond minimal compileable shells.
- 2026-05-14: Code Builder created and compiled the InGame skeleton.

## Task: 2026-05-14 InGame Unit Skill Component Architecture

### Task title

Refine InGame unit creation and skill implementation responsibilities using `skill-class-design.md`.

### Goals

- Keep the user's monster/enemy model-and-actor direction for InGame.
- Use `skill-class-design.md` as the target skill data schema reference.
- Preserve current `SkillDefinition` / `PassiveDefinition` compatibility through a proposed adapter before any final data-schema migration.
- Prevent triple skill implementation by assigning data, runtime state, executor logic, and presentation ownership explicitly.

### Constraints

- Role Owner is Designer.
- No runtime C# implementation or Unity scene edits in this task.
- Proposed files/classes in the report are not claimed to exist yet.
- Keep UI integration deferred until a minimal independent InGame loop exists.

### Role Owner

Designer

### Status

Completed as an HTML design report.

### Next Actions

- When Code Builder starts InGame, begin with contracts/model skeleton and then unit factory/actor binding before adding broad skill executors.
- Introduce `SkillSpecAdapter` or equivalent compatibility layer before replacing existing `SkillDefinition` usage.
- Keep ordinary skill behavior data-driven; reserve custom executors for documented exceptions such as Rin-D and Vega-E.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-unit-skill-component-architecture.html`.
- `Pakuri/reference/skill-class-design.md:11` through `:20` proposes `SkillData` with projectile, beam, zone, buff, shield, and passive subclasses.
- `Pakuri/reference/skill-class-design.md:177` through `:178` and `:306` identify Rin-D and Vega-E as hardcoded exception logic.
- `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs:54`, `:63`, and `:78` through `:79` show current skill data already carries effect prefab and choice arrays.
- `Pakuri/Assets/Scripts/Data/Definition/MonsterDefinition.cs:56` through `:57` shows monsters already reference active and passive skill definitions.
- `Pakuri/Assets/Scripts/Run/Flow/RunSceneBootstrap.cs:53` through `:57` shows the current Run-to-combat input contract that InGame must preserve.

### History

- 2026-05-14: User asked how the recommended approach would manage unit creation and implement skills, with file structure and component diagrams.

## Task: 2026-05-13 InGame Foundation Architecture

### Task title

Design a new InGame foundation instead of continuing only the existing combat partial split.

### Goals

- Preserve current Run UI Flow, `RunSession`, and CSV/Data loading while building a new combat runtime/scene in parallel.
- Use model/view separation: pure unit runtime models plus thin Unity Actor/View components.
- Make 1P and manifested units share the same `MonsterUnitActor`, skill execution, learned-choice lookup, and auto-attack capability.
- Keep `RunCombatUiController` integration deferred until the new combat loop exists.

### Constraints

- Role Owner is Designer.
- No runtime C# implementation in this design task.
- No Unity scene edits in this design task.
- Evidence must come from inspected code and the user's confirmed decisions.

### Role Owner

Designer

### Status

Completed as an HTML foundation report.

### Next Actions

- Start InGame implementation with contract/model skeleton only: `InGameCombatManager`, `InGameContextManager`, `UnitRuntimeModel`, and result contracts.
- Do not start UI integration until the new combat loop can spawn units and resolve a minimal battle independently.
- Keep existing `CombatRuntimeController` as the comparison/rollback path during InGame development.

### Evidence

- Added `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html`.
- `Pakuri/Assets/Scripts/Run/Flow/RunSceneBootstrap.cs:53` through `:57` shows the current combat entry uses `MonsterDefinition`, `RunSession`, and `GameDataCatalog`.
- `Pakuri/Assets/Scripts/Data/Definition/MonsterDefinition.cs` exposes monster stats, defenses, health/damage/projectile tuning, active skills, passive skills, and reward choices.
- `Pakuri/Assets/Scripts/Data/Definition/EnemyDefinition.cs` exposes enemy stats, defenses, attack type, Stage 1 skill kind, active skill values, and passive summary.
- `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs` exposes `SkillRuntimeKind`, `SkillEffectPrefab`, cooldown, magazine, status, and enhancement/master choice data.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` still defines current `EnemyRuntime` as a private nested class, supporting the decision to build InGame beside the old runtime rather than further entangling it.
- User confirmed model/view separation, data-based identity, reusable skill management, shared `MonsterUnitActor`, auto-attack toggles, deferred UI integration, and learned-choice lookup from unit state.

### History

- 2026-05-13: After Phase 3 refactor completion, user proposed replacing the old combat path with a new combat scene/runtime while retaining Run/Data flows.
- 2026-05-13: Designer recommended a parallel InGame approach.
- 2026-05-13: User provided rough unit/skill/prefab/animation structure and confirmed the final choices after Designer questions.

## Task: 2026-05-13 Phase 3-H Closeout / Ownership Verification

### Task title

Verify Phase 3 projectile/effect/drone simulation split completion.

### Goals

- Confirm Phase 3-A through Phase 3-G lifecycle-owner boundaries exist in code.
- Confirm projectiles, skill effects, selected drones, and manifested drones have readable simulation boundary owners.
- Decide whether Phase 3 is complete and whether Phase 4 Enemy Simulation can start.

### Constraints

- Role Owner is Code Builder.
- Do not implement Phase 4 enemy simulation in this slice.
- Do not run Unity Play Mode; gameplay verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Completed. Phase 3 is complete in Code Builder scope; only user-owned Play Mode verification and optional Code Reviewer remain outside this closeout.

### Next Actions

- Start Phase 4 `Enemy Simulation Split` as the next default Code Builder implementation phase when requested.
- User verifies selected/manifested projectile, sustained skill-effect, and drone behavior in Play Mode.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectileSimulation.cs:22` through `:25` routes `UpdateProjectiles()` through `ProjectileSimulationBoundary`, and `:41` through `:120` owns projectile lookup, lifetime, X-edge checks, cleanup, and removal.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` through `:43` keeps the projectile loop body in `UpdateProjectilesCore()` and routes enemy, manifested, and selected projectile handling into named helpers.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:22` through `:25` routes persistent skill effects through `SkillEffectSimulationBoundary`, `:58` through `:79` owns effect hit dispatch, and `:104` through `:142` owns the shared effect lifecycle loop.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs:22` through `:35` routes selected and manifested drone update/cleanup through `DroneSimulationBoundary`; `:46` through `:72` owns selected drone ticking, and `:118` through `:183` owns manifested drone ticking and cleanup.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:8` through `:17` still defines `ManifestedDroneRuntime`, and `:19` through `:49` still owns manifested Eve drone deployment and registration.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1199` still preserves persistent effect update before selected drone update.
- `Pakuri/Assembly-CSharp.csproj:70`, `:100`, and `:101` include `CombatRuntimeSkillEffectSimulation.cs`, `CombatRuntimeDroneSimulation.cs`, and `CombatRuntimeProjectileSimulation.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check` reported no whitespace errors; it printed only LF-to-CRLF working-copy warnings.
- Unity-MCP reimported all Phase 3 touched scripts, script refresh reached idle, and console read showed only MCP-FOR-UNITY client connection/client-handler logs rather than C# compile errors.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` still defines `EnemyRuntime` as a private nested class, while `CombatRuntimeEnemies.cs:306`, `:336`, and `:706` still own spawning, enemy creation, and enemy update, so Phase 4 is the next unimplemented split.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 3-H and judge whether Phase 3 refactoring is finished.
- 2026-05-13: Builder inspected Phase 3 boundary code, project inclusion, enemy handoff points, builds, diff check, Unity import/refresh, and console state; no additional Phase 3 implementation slice was found.

## Task: 2026-05-13 Phase 3-G Manifested Drone Simulation Alignment

### Task title

Align manifested drone ticking with the drone simulation boundary.

### Goals

- Move manifested drone ticking, target lookup, projectile firing cadence, and cleanup behind `CombatRuntimeDroneSimulation.cs`.
- Preserve `ManifestedDroneRuntime` type, deployment, source runtime, skill reference, visual setup, and manifested party list ownership.
- Keep selected and manifested drone runtime classes separate.

### Constraints

- Role Owner is Code Builder.
- Do not merge `DroneRuntime` and `ManifestedDroneRuntime`.
- Do not redesign manifested Eve drone behavior or projectile values.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue with Phase 3-H `Phase 3 Closeout / Ownership Verification` as a separate verification slice.
- User verifies manifested Eve Drone Beacon deployment, lifetime, target lookup, projectile fire, and cleanup in Play Mode if needed.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs:27` through `:35` now keeps `UpdateManifestedDrones()` and `RemoveManifestedDroneAt(...)` as compatibility entries that route through `DroneSimulationBoundary`.
- `CombatRuntimeDroneSimulation.cs:118` through `:160` now owns manifested drone reverse iteration, validity checks, duration ticking, cooldown ticking, no-target retry cooldown, nearest-target lookup, projectile firing, and attack cooldown reset.
- `CombatRuntimeDroneSimulation.cs:162` through `:183` now owns manifested drone cleanup and preserves play/edit-mode destruction behavior before removing from `manifestedDrones`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:8` through `:17` still defines `ManifestedDroneRuntime`.
- `CombatRuntimeManifestedPartyDrones.cs:19` through `:49` still owns manifested Eve drone deployment, visual setup, duration setup, list registration, and status label.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:49` still calls `owner.UpdateManifestedDrones()` before manifested unit combat ticking, preserving existing party tick order.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check` reported no whitespace errors; it printed only existing LF-to-CRLF working-copy warnings.
- Unity-MCP reimported `CombatRuntimeDroneSimulation.cs` and `CombatRuntimeManifestedPartyDrones.cs`; script refresh recovered after a Unity disconnect/retry and returned ready, and final console read returned only MCP-FOR-UNITY client connection/client-handler logs.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 3-G.
- 2026-05-13: Builder moved manifested drone ticking/projectile/cleanup behavior into `CombatRuntimeDroneSimulation.cs` while preserving manifested runtime type and deployment ownership.

## Task: 2026-05-13 Phase 3-F Selected Drone Simulation Boundary

### Task title

Move selected Eve drone ticking behind a simulation boundary.

### Goals

- Move selected Eve `DroneRuntime` lifecycle ticking and `FireDroneProjectile(...)` behavior out of `CombatRuntimeEveSkills.cs`.
- Preserve selected Eve drone duration, attack cadence, nearest-target lookup, projectile creation fields, and cleanup timing.
- Keep manifested drones separate for Phase 3-G.

### Constraints

- Role Owner is Code Builder.
- Do not merge `DroneRuntime` and `ManifestedDroneRuntime`.
- Do not redesign Eve drone skills or projectile values.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue with Phase 3-G `Manifested Drone Simulation Alignment` only as a separate slice.
- User verifies selected Eve Drone Beacon duration, firing cadence, target lookup, projectile behavior, and cleanup in Play Mode if needed.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs`.
- `CombatRuntimeDroneSimulation.cs:7` through `:25` defines the selected drone simulation boundary and keeps `UpdateDrones()` as the compatibility entry point.
- `CombatRuntimeDroneSimulation.cs:36` through `:63` owns reverse iteration over `drones`, missing-runtime removal, duration ticking, attack cooldown ticking, projectile fire timing, destruction, and list removal.
- `CombatRuntimeDroneSimulation.cs:65` through `:105` owns selected drone projectile creation and preserves nearest-target lookup, direction fallback, `DroneShot` naming, sprite/color/sorting, speed `12f`, lifetime `2f`, hit radius `0.28f`, `SkillId = "eve-e"`, and vulnerable stack transfer.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1171` through `:1184` still creates and registers selected Eve `DroneRuntime` values during `TryCastEveDroneBeacon()`.
- `CombatRuntimeEveSkills.cs:1196` through `:1200` still calls `UpdatePersistentSkillEffects()` before `UpdateDrones()`, preserving selected Eve effect-before-drone order.
- `Pakuri/Assembly-CSharp.csproj:100` includes `CombatRuntimeDroneSimulation.cs` for local builds.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check` reported no whitespace errors; it printed only existing LF-to-CRLF working-copy warnings.
- Unity-MCP reimported `CombatRuntimeDroneSimulation.cs` with guid `8f3c5bd7d2a044a4aad0f2e3c6b8d901` and `CombatRuntimeEveSkills.cs`; script refresh recovered to idle, and final console read returned only MCP-FOR-UNITY client connection/client-handler logs.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 3-F.
- 2026-05-13: Builder moved selected Eve drone ticking and drone projectile spawning into `CombatRuntimeDroneSimulation.cs` without merging manifested drone runtime.

## Task: 2026-05-13 Phase 3-E Skill Effect Hit / Expiry Routing

### Task title

Split skill-effect shape, hit, and expiry routing helpers.

### Goals

- Separate skill-effect enemy eligibility, shape checks, hit routing, Eve fallback effect handling, and expiry routing into readable helpers.
- Preserve Eve B/C, Sein effect damage/residual spawn, Vega line/area effects, and manifested effect damage formulas.
- Keep common temporary-effect migration out of Phase 3.

### Constraints

- Role Owner is Code Builder.
- Do not introduce `TemporaryEffectInstance`; that remains Phase 7.
- Do not migrate shield/status modifiers into a common effect layer.
- Do not start Phase 3-F selected drone boundary in this slice.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue with Phase 3-F `Selected Drone Simulation Boundary` only as a separate slice.
- User performs Play Mode skill-effect verification if needed.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:27` through `:39` now owns `TickSkillEffect(...)` as an enemy-loop dispatcher.
- `CombatRuntimeSkillEffectSimulation.cs:41` through `:56` separates enemy validity and shape checks, preserving the previous Eve B / Vega line beam check and circular radius check.
- `CombatRuntimeSkillEffectSimulation.cs:58` through `:79` separates hit routing in the previous order: Sein, Vega, manifested source, then Eve fallback.
- `CombatRuntimeSkillEffectSimulation.cs:81` through `:97` preserves Eve fallback damage plus Eve B slow and Eve C chill/freeze status handling.
- `CombatRuntimeSkillEffectSimulation.cs:99` through `:102` owns expiry routing and delegates current Sein residual behavior to `TryHandleSeinSkillEffectExpired(...)`.
- `CombatRuntimeSkillEffectSimulation.cs:113` through `:140` still preserves the Phase 3-D lifecycle order: duration/tick, `HitThisTick.Clear()`, tick callback, expiry callback, destroy, remove.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeSeinSkills.cs:1031` through `:1073` still owns Sein effect damage formulas; `:1075` through `:1114` still owns Sein residual effect spawn formulas.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:772` through `:783` still owns Vega effect damage/name-mark behavior; `:1523` through `:1536` still owns Vega effect classification helpers.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1200` still updates persistent effects before selected Eve drones.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check` completed with no whitespace errors and LF-to-CRLF warnings only.
- Unity-MCP reimported `CombatRuntimeSkillEffectSimulation.cs`, `CombatRuntimeEveSkills.cs`, and `CombatRuntimeSeinSkills.cs`; script refresh recovered to ready, and final console warning/error read returned only MCP client handler logs.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 3-E.
- 2026-05-13: Builder split skill-effect shape, hit, and expiry routing helpers while leaving formulas and common temporary effects unchanged.

## Task: 2026-05-13 Phase 3-D Skill Effect Simulation Boundary Shell

### Task title

Move shared skill-effect lifetime ticking behind a simulation boundary.

### Goals

- Move the `skillEffects` reverse-iteration lifetime/tick loop behind a named effect simulation boundary.
- Preserve effect duration ticking, tick interval cadence, `HitThisTick.Clear()`, expiry callbacks, object destruction, and list removal timing.
- Keep existing damage/status helper callbacks in their current skill files.

### Constraints

- Role Owner is Code Builder.
- Do not introduce `TemporaryEffectInstance`; that remains Phase 7.
- Do not start Phase 3-E hit/expiry routing separation in this slice.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue with Phase 3-E `Skill Effect Hit / Expiry Routing` only as a separate slice.
- User performs Play Mode skill-effect verification if needed.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs`.
- `CombatRuntimeSkillEffectSimulation.cs:7` through `:25` defines the effect simulation boundary and keeps `UpdatePersistentSkillEffects()` as the compatibility entry point.
- `CombatRuntimeSkillEffectSimulation.cs:36` through `:64` owns reverse iteration over `skillEffects`, missing-runtime removal, duration ticking, tick interval checks, `HitThisTick.Clear()`, `TickSkillEffect(...)`, expiry handling, `Object.Destroy(...)`, and list removal.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1200` still calls `UpdatePersistentSkillEffects()` before `UpdateDrones()`, preserving selected Eve effect-before-drone order.
- `CombatRuntimeEveSkills.cs:1202` still owns `TickSkillEffect(...)`; `:1222`, `:1228`, and `:1234` still dispatch Sein, Vega, and manifested effect damage callbacks.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeSeinSkills.cs:1075` still owns `TryHandleSkillEffectExpired(...)`, so expiry-spawn routing did not move in Phase 3-D.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check` completed with no whitespace errors and LF-to-CRLF warnings only.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs` with guid `f572c8fd58d31864c8a7db4a9f131701`; script refresh recovered to ready, and final console warning/error read returned only MCP client handler logs.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 3-D.
- 2026-05-13: Builder moved the shared skill-effect lifetime/tick loop behind a simulation boundary while leaving effect damage and expiry routing callbacks in place.

## Task: 2026-05-13 Phase 3-C Projectile Hit Routing Helpers

### Task title

Split projectile hit routing into source-specific helpers.

### Goals

- Split the projectile update loop into enemy projectile, manifested projectile, and selected/player projectile handlers.
- Preserve current hit order, status labels, debug logs, pierce consumption, Ariel explosion triggers, Sein/Vega/Rin hooks, and status application order.
- Keep damage application APIs and formulas in their current owner.

### Constraints

- Role Owner is Code Builder.
- Preserve player-facing projectile behavior.
- Do not move `ApplyDamageToEnemy`, `ApplyDamageToSelectedMonster`, `ApplyDamageToManifestedUnit`, or common target state.
- Do not change projectile creation sites, cleanup ownership, or lifetime ticking.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue with Phase 3-D `Skill Effect Simulation Boundary Shell` only as a separate slice.
- User performs Play Mode projectile verification if needed.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:31` through `:43` now routes each projectile by source type to `ProcessEnemyProjectile(...)`, `ProcessManifestedProjectile(...)`, or `ProcessSelectedProjectile(...)`.
- `CombatRuntimeProjectiles.cs:47` through `:62` owns enemy projectile hit/lifetime cleanup routing while preserving `TryHitEnemyProjectileTarget(...)`, status label text, and cleanup calls.
- `CombatRuntimeProjectiles.cs:64` through `:93` owns manifested projectile hit, debug log, pierce decrement, Ariel explosion trigger, X-edge, and cleanup routing.
- `CombatRuntimeProjectiles.cs:95` through `:111` owns selected/player projectile top-level hit versus X-edge routing.
- `CombatRuntimeProjectiles.cs:113` through `:175` owns selected/player enemy-hit follow-up order: Sein impact, damage application, flash, Ariel/Sein/Vega/Rin hooks, status application, debug log, branch/proc, Ariel explosion, hit set, pierce, and cleanup.
- `CombatRuntimeProjectiles.cs:394`, `:422`, and `:452` still own `ApplyDamageToEnemy`, `ApplyDamageToSelectedMonster`, and `ApplyDamageToManifestedUnit`, so damage APIs did not move in Phase 3-C.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check` completed with no whitespace errors and LF-to-CRLF warnings only.
- Unity-MCP reimported `Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs` with guid `f12ed5e4da1e7aa47836230d77c9c225`; editor state returned `ready_for_tools=true`, and final console warning/error read returned only MCP client handler logs.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 3-C after Phase 3-B.
- 2026-05-13: Builder split projectile hit routing into source-specific helpers while leaving damage APIs and formulas in place.

## Task: 2026-05-13 Phase 3-B Projectile Cleanup Lifetime Ownership

### Task title

Move projectile cleanup and lifetime ownership behind the projectile boundary.

### Goals

- Move projectile lookup, invalid runtime removal, lifetime ticking, battlefield X-edge checks, and cleanup/destruction into the projectile simulation boundary.
- Preserve reverse-iteration cleanup safety and `GameObject` destruction plus `projectiles.RemoveAt(i)` behavior.
- Keep enemy, manifested, and selected/player projectile hit and damage routing unchanged for Phase 3-C.

### Constraints

- Role Owner is Code Builder.
- Preserve player-facing projectile behavior.
- Do not split selected/manifested/enemy damage resolution in this slice.
- Do not move damage APIs, common target state, or projectile creation sites.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue with Phase 3-C `Projectile Hit Routing Helpers` only as a separate slice.
- User performs Play Mode projectile verification if needed.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectileSimulation.cs:41` exposes `LastProjectileIndex` for reverse projectile iteration.
- `CombatRuntimeProjectileSimulation.cs:43` through `:51` owns indexed projectile lookup.
- `CombatRuntimeProjectileSimulation.cs:53` through `:66` owns missing projectile removal with `owner.projectiles.RemoveAt(index)`.
- `CombatRuntimeProjectileSimulation.cs:68` through `:80` owns projectile lifetime ticking and remaining-lifetime checks.
- `CombatRuntimeProjectileSimulation.cs:83` through `:104` owns battlefield X-edge checks.
- `CombatRuntimeProjectileSimulation.cs:106` through `:120` owns cleanup, `Object.Destroy(projectile.GameObject)`, and `owner.projectiles.RemoveAt(index)`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:16` now starts reverse iteration from `ProjectileSimulationBoundary.LastProjectileIndex`.
- `CombatRuntimeProjectiles.cs:18` through `:29` gets projectiles through the boundary, removes missing entries through the boundary, and ticks lifetime through the boundary.
- `CombatRuntimeProjectiles.cs:31` through `:154` still preserves the existing enemy, manifested, and selected/player projectile hit branch order while calling boundary cleanup.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check` completed with no whitespace errors and LF-to-CRLF warnings only.
- Unity-MCP reimported `Assets/Scripts/Combat/Manager/CombatRuntimeProjectileSimulation.cs` with guid `57f2745c5878a53408874be4db5a95fc`; editor state returned `ready_for_tools=true`, and final console warning/error read returned only MCP client handler logs.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to implement Phase 3-B after Phase 3-A.
- 2026-05-13: Builder moved projectile cleanup/lifetime/edge responsibilities behind the Phase 3 projectile boundary without changing damage or hit routing formulas.

## Task: 2026-05-13 Phase 3-A Projectile Simulation Boundary Shell

### Task title

Implement the Phase 3-A projectile simulation boundary shell.

### Goals

- Add a narrow projectile simulation boundary around the existing projectile update entry point.
- Preserve `CombatRuntimeController.Update()` order and the current projectile loop behavior.
- Keep projectile damage formulas, hit routing, pierce handling, cleanup, and projectile creation sites unchanged for this slice.

### Constraints

- Role Owner is Code Builder.
- Preserve player-facing combat behavior.
- Do not start Phase 3-B cleanup ownership, Phase 3-C hit routing split, Phase 6 skill reuse, or Phase 7 common target/effect migration.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue with Phase 3-B `Projectile Cleanup / Lifetime Ownership` only after this slice is accepted.
- User performs Play Mode projectile verification if needed.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectileSimulation.cs`.
- `CombatRuntimeProjectileSimulation.cs:20` keeps the `UpdateProjectiles()` entry point that `CombatRuntimeController.Update()` calls.
- `CombatRuntimeProjectileSimulation.cs:22` routes the entry point through `ProjectileSimulationBoundary.Tick()`.
- `CombatRuntimeProjectileSimulation.cs:34` through `:36` delegates to `owner.UpdateProjectilesCore()`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` renames the existing loop body from `UpdateProjectiles()` to `UpdateProjectilesCore()`; the loop body, hit branches, damage calls, pierce handling, and cleanup calls were otherwise left in place.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:500` still calls `UpdateProjectiles()` between `UpdateEnemies()` and `UpdateMonsterSkillRuntimeEffects()`.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeProjectileSimulation.cs` as `UnityEditor.MonoScript` with guid `57f2745c5878a53408874be4db5a95fc`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check` completed with no whitespace errors and LF-to-CRLF warnings only.
- Unity-MCP editor state returned `ready_for_tools=true`, and console warning/error read returned only MCP-FOR-UNITY client handler logs after the new script import.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to read `boards/REFACTORING/REFACTORING.md` and start Phase 3-A.
- 2026-05-13: Builder added a projectile simulation boundary shell and kept the existing projectile update order and loop behavior intact.

## Task: 2026-05-13 Phase 3 Projectile Effect Drone Split Work Breakdown

### Task title

Define Phase 3-A through Phase 3-H before implementation.

### Goals

- Split Phase 3 `Projectile / Effect / Drone Simulation Split` into reviewable implementation slices.
- Preserve current projectile hit order, effect tick behavior, drone cadence, cleanup timing, and `CombatRuntimeController.Update()` order.
- Avoid starting Phase 6 skill reuse or Phase 7 common target / temporary effect migration during Phase 3.
- Provide Code Builder with a clear sequence before any runtime C# edit starts.

### Constraints

- Role Owner is Designer.
- Do not change runtime C# behavior in this planning task.
- Phase 3 must keep damage application APIs stable until Phase 7.
- Do not move selected-unit combat, enemy simulation, or common target ownership in Phase 3.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission.

### Role Owner

Designer

### Status

Completed as a pre-implementation work breakdown. Phase 3 should be split into 3-A through 3-H.

### Next Actions

- If implementation starts, begin with Phase 3-A only.
- After each slice, Code Builder should run runtime/editor builds, `git diff --check`, Unity-MCP refresh/console evidence where relevant, and update affected boards.
- Do not start Phase 3-D or later until Phase 3-A through 3-C have preserved projectile behavior.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:496` through `:506` still calls `UpdateProjectiles()`, `UpdateMonsterSkillRuntimeEffects()`, `UpdateManifestedMonsterPartyCombat()`, and `UpdateSelectedMonsterCombat()` in a fixed order.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` through `:154` currently owns projectile ticking, movement, selected/manifested/enemy hit routing, status application, pierce, edge cleanup, and projectile cleanup calls in one loop.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:649` through `:660` owns `CleanupProjectile(int index)`.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1230` owns `UpdateEveSkillEffects()` and the shared `skillEffects` lifetime/tick loop even though later checks dispatch to Sein, Vega, and manifested effect damage.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1233` through `:1283` owns `TickSkillEffect(...)`, including Eve, Sein, Vega, and manifested effect damage routing.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1286` through `:1342` owns selected Eve `drones` ticking and drone projectile fire.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:8` through `:115` owns a separate `ManifestedDroneRuntime` list and lifecycle, not the same `DroneRuntime` list used by selected Eve.
- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:7` through `:79` already provides Phase 1 registration facade methods for projectile, skill effect, and drone additions while the underlying lists still live on `CombatRuntimeController`.

### Phase 3 Slices

| Slice | Name | Scope | Keep out of scope | Acceptance criteria |
| --- | --- | --- | --- | --- |
| 3-A | Projectile Simulation Boundary Shell | Add a narrow projectile simulation boundary and route the existing `UpdateProjectiles()` entry through it while keeping current helper calls and update order. | Do not change damage formulas, hit logic, pierce logic, or projectile creation sites. | `CombatRuntimeController.Update()` still reaches projectile ticking at the same point, and behavior is a wrapper-level move only. |
| 3-B | Projectile Cleanup / Lifetime Ownership | Move projectile cleanup and lifetime/edge-removal responsibility behind the projectile boundary. | Do not split selected/manifested/enemy damage resolution yet. | Cleanup remains reverse-iteration safe; destroyed GameObjects and `projectiles.RemoveAt(i)` behavior are preserved. |
| 3-C | Projectile Hit Routing Helpers | Split the projectile loop into source-specific handlers for enemy projectile, manifested projectile, and selected/player projectile paths. | Do not move `ApplyDamageToEnemy`, `ApplyDamageToSelectedMonster`, `ApplyDamageToManifestedUnit`, or common target state. | Hit order, status labels, pierce consumption, Ariel explosion trigger, Sein/Vega/Rin hooks, and status application remain in the same observable order. |
| 3-D | Skill Effect Simulation Boundary Shell | Move the `skillEffects` lifetime/tick loop behind an effect simulation boundary while keeping existing damage/status helper callbacks. | Do not introduce `TemporaryEffectInstance`; that is Phase 7. | Effect duration, tick interval, `HitThisTick.Clear()`, expiry handling, and removal timing are preserved. |
| 3-E | Skill Effect Hit / Expiry Routing | Separate effect shape checks, hit routing, and expiry-spawn routing enough that Eve/Sein/Vega/Manifested effect handlers are readable boundaries. | Do not migrate shield/status modifiers into common temporary effects. | Eve B/C, Sein residual spawn, Vega line/area effects, and manifested effect damage still dispatch through existing formulas. |
| 3-F | Selected Drone Simulation Boundary | Move selected Eve `drones` ticking and `FireDroneProjectile(...)` behind a drone simulation boundary. | Do not merge manifested drones yet and do not redesign drone skills. | Drone duration, attack period, nearest-target lookup, projectile creation, and cleanup remain unchanged. |
| 3-G | Manifested Drone Simulation Alignment | Give `ManifestedDroneRuntime` ticking a comparable simulation boundary or adapter while preserving its party-source runtime and list ownership. | Do not force `DroneRuntime` and `ManifestedDroneRuntime` into one class unless implementation evidence shows it is a safe no-behavior slice. | Manifested Eve drone deployment, cooldown, target lookup, projectile fire, and cleanup stay behaviorally identical. |
| 3-H | Phase 3 Closeout / Ownership Verification | Verify projectile/effect/drone lifecycle owners are readable, board state is updated, and Phase 4 Enemy Simulation can start. | Do not perform Phase 4 enemy split in the same slice. | Builds pass, `git diff --check` passes, Unity-MCP console has no C# compile errors, and boards identify Phase 4 as the next default phase. |

### History

- 2026-05-13: User explicitly asked Designer to decide how far to split Phase 3 into `3-[A-Z]` before work starts and record it on the boards.

## Task: 2026-05-13 Roadmap Amendment For Shared Target, Temporary Effect, And Skill Reuse

### Task title

Add explicit timing for shared target, reusable temporary effects, same-type skill reuse, and Monster / Enemy actor commonization.

### Goals

- Reconcile the 2026-05-10 shared combat target / temporary effect proposal with the post-Phase-2-E roadmap.
- State that same-type skill reuse is a Phase 6 decision after Phase 3 lifecycle and adapter boundaries.
- State that common target / temporary effect migration belongs in Phase 7.
- State that Monster / Enemy inheritance or prefab-based common authoring belongs after Phase 7 stabilization, primarily as a Phase 8 view/component decision.

### Constraints

- Role Owner is Designer.
- Do not change runtime C# behavior.
- Preserve the current Phase 3 next-step recommendation.
- Keep prefab authoring separate from combat-state ownership until target/effect APIs are proven.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Start implementation with Phase 3 `Projectile / Effect / Drone Simulation Split` unless the user asks for another design-only pass.
- During Phase 6, decide the exact same-type grouping scope with the user: projectile, beam / line / area / field, summon / drone, and any exceptions.
- During Phase 7, introduce read adapters before moving state ownership into `CombatTargetModel`.
- During Phase 8, evaluate prefab/view commonization only after Phase 7 target/effect behavior is stable.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` with section `7. 2026-05-10 공통 대상 / 임시효과 제안 반영 여부`.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:401` through `:402` says selected ally, summoned ally, and enemy combat state must first be readable from one layer before common temporary effects can be safely reused.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:53`, `:68`, `:83`, `:98`, and `:113` still route monster action-speed reads through controller methods, so broad skill reuse should wait for adapter narrowing.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` defines `EnemyRuntime` as private nested class, while `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:8` is a `MonoBehaviour`; this supports adapter/model-first commonization before base-class inheritance.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:354`, `:419`, and `:517` show enemy scene object composition with `AddComponent`, so prefab authoring should be evaluated as a view/component phase rather than as the first combat model step.

### History

- 2026-05-13: User requested an amendment explaining whether the 2026-05-10 shared combat target / temporary effect proposal is included in the roadmap and where to place same-type skill reuse, common parent/prefab creation, and temporary-effect reuse.

## Task: 2026-05-13 Phase 2 Manifested Party Closeout Verification

### Task title

Verify whether Phase 2 `Manifested Party` should close after Phase 2-E.

### Goals

- Inspect the remaining Phase 2 candidate code in `CombatRuntimeParty.cs`.
- Decide whether a small independent Phase 2-F formula or field-effect slice exists.
- Confirm the current code still compiles after the Phase 2-E state.
- Record whether the next default implementation phase should be Phase 3.

### Constraints

- Role Owner is Code Builder.
- Do not change runtime C# behavior for this verification task.
- Preserve current player-facing combat behavior.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Completed. Phase 2 is closable; no small independent Phase 2-F slice was identified from inspected code.

### Next Actions

- Start Phase 3 `Projectile / Effect / Drone Simulation Split` as the next default implementation phase.
- Keep Rin shockwave, generic/persistent field routing, Eve frost field, and Vega queued projectile call-site behavior in place until Phase 3 or Phase 6 provides a better owner boundary.
- Run Code Reviewer only if the user explicitly requests another review pass.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:351` through `:443` contains `TryFireManifestedRinShockwave(...)`, which combines Rin-specific choice checks, line effect creation, direct enemy iteration, Rin damage hooks, knockback, slow, and reload reduction; it is not a small generic helper slice.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:469` through `:508` contains generic field routing and `TryFireManifestedPersistentSkill(...)`, which remains directly called by `CombatRuntimeManifestedPartyDamage.cs:22`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:512` through `:564` contains `CreateManifestedEveFrostField(...)`, which combines Eve-specific radius/duration/tick/chill/freeze choices and effect registration; it should wait for Phase 3 effect simulation or later skill-adapter work rather than becoming a standalone Phase 2-F slice.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:701` through `:744` contains Vega queued projectile setup/ticking, and `CombatRuntimeManifestedPartySkills.cs:82` and `:115` still call `UpdateManifestedQueuedProjectiles(...)` and `QueueManifestedVegaThreeSwordFlurry(...)`; this is tied to `CombatSkillRuntime` magazine ticking rather than an independent closeout slice.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs:9` through `:22` already owns the generic manifested skill fire entry point and delegates the remaining special cases before generic damage.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing `System.Net.Http` / `System.IO.Compression` assembly reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings.
- Unity-MCP `refresh_unity` returned `success=true`, `refresh_triggered=false`, `resulting_state=idle`.
- Unity-MCP console warning/error read returned only MCP-FOR-UNITY client handler disposed/exited logs, not C# compile errors.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to perform the Phase 2 closeout verification.

## Task: 2026-05-13 Combat Runtime Refactor Roadmap After Phase 2-E

### Task title

Create the post-Phase-2-E combat runtime refactor roadmap.

### Goals

- Confirm Phase 2 closeout criteria after the Phase 2-E manifested party damage/projectile helper split.
- Record the remaining full roadmap from Phase 3 through Phase 7 and the later common actor/base-class decision point.
- Place skill reuse refactoring at the correct timing relative to projectile/effect/drone simulation and monster runtime adapter narrowing.
- Place common combat target and Monster/Enemy common base proposals at the correct timing relative to `ICombatTarget`, `TemporaryEffect`, and state-owner stabilization.

### Constraints

- Role Owner is Designer for this roadmap/report task.
- Preserve the existing implementation order unless inspected code provides a stronger reason to change it.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode.
- Code Reviewer execution requires explicit user permission and is not part of this report task.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Treat Phase 2 as closable unless Code Builder identifies a small independent remaining manifested formula/field-effect slice in inspected code.
- Prepare Phase 3 `Projectile / Effect / Drone Simulation Split` as the next default implementation phase.
- Delay broad skill-executor reuse until Phase 6 adapter narrowing, using Phase 3 to prepare projectile/effect/drone lifecycle boundaries.
- Delay common combat target migration until Phase 7 and delay common base-class inheritance until after `ICombatTarget` / temporary-effect behavior is verified.

### Evidence

- Added `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html`.
- `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html:319` through `:321` says Phase 2 should only continue if a smaller formula/field-effect slice is identified.
- `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html:324` through `:345` records Phase 3 projectile/effect/drone, Phase 4 enemy, Phase 5 selected-unit, Phase 6 adapter narrowing, and Phase 7 combat target / temporary-effect migration.
- `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html:546` through `:550` proposes Manifested Party, Projectile / Effect / Drone, Enemy Simulation, Selected Unit Combat, then monster runtime adapter narrowing.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:401` through `:402` says common target and reusable temporary effects are needed, but first selected ally, summoned ally, and enemy combat state must be readable from one layer.
- `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html:451` through `:453` says common base-class inheritance should come after `ICombatTarget` / adapter and effect-layer stabilization.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:307` through `:310` still owns battlefield lists and `:498` through `:503` still owns top-level update calls.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` still owns `UpdateProjectiles()` and `:516` still owns selected-unit combat.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:306`, `:706`, and `:945` still own enemy spawn/update/target priority.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:29` still exposes a full `CombatRuntimeController` reference.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:8` is a `MonoBehaviour` and `:193` still calls back through `Owner.TickManifestedUnitSkill(...)`.

### History

- 2026-05-13: User requested a detailed evidence-based HTML roadmap from Phase 2 closure verification through the remaining refactor phases, including the timing for skill reuse and common combat model proposals.

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

Phase 1 battlefield facade boundary implemented. Phase 2 manifested party runtime, view binder, skill dispatcher, drone lifecycle, skill visual helper, and damage/projectile helper boundaries are implemented. User-requested Phase 2 Code Reviewer completed with `REVIEW_RESULT: PASS`.

### Next Actions

- Continue Phase 2 only if a smaller remaining manifested party formula or field-effect slice is identified by inspected code; otherwise prepare Phase 3 projectile/effect/drone simulation split.
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
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs:7` through `:19` owns `ManifestedMonsterStatusViews`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs:23` through `:52` resolves scene-authored manifested name, HP, HP-bar, and shield-bar children.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs:55` through `:141` owns fallback and live HP/shield bar repair helpers.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs:256` through `:302` owns manifested label and HP/shield bar refresh.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:194`, `:197`, `:224`, `:300`, `:334`, and `:1851` now call the view binder helpers from the party/runtime flow.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs:5` keeps the existing `CombatUnitRuntime` callback name as a compatibility wrapper.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs:10` through `:71` owns manifested unit skill dispatch order, preserving Eve, Rin, Sein, Vega, Ariel, then generic fallback.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs:74` through `:139` owns fallback manifested skill ticking, queued projectile ticking, reload ticking, and magazine fire dispatch.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:64` through `:71` routes per-skill dispatch through `ManifestedPartyRuntime.TickUnitSkill(...)`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:362` now starts at `FireManifestedMonsterSkill(...)`, so the dispatcher block was moved out while damage/formula and scene-object firing methods remain in the existing party partial.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:193` still calls `Owner.TickManifestedUnitSkill(...)`, preserving the current public callback path for this slice.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:8` through `:16` now owns `ManifestedDroneRuntime`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:19` through `:48` now owns manifested Eve drone deployment and registration into `manifestedDrones`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:51` through `:92` now owns manifested drone ticking, target lookup, and projectile firing.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:95` through `:115` now owns manifested drone cleanup.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:16` now starts at `ManifestedMonsterSlotNames`, so the drone runtime type moved out of the party partial.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:1586` still calls `RemoveManifestedDroneAt(i)` from party clear, preserving cleanup behavior.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyVisuals.cs:9` now owns manifested non-drone skill visual dispatch.
- `CombatRuntimeManifestedPartyVisuals.cs:60` now owns manifested skill visual duration resolution while preserving existing monster-specific skill ID durations.
- `CombatRuntimeManifestedPartyVisuals.cs:120`, `:132`, and `:154` now own manifested circle visual creation, line visual creation, and shared visual configuration.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:371`, `:401`, and `:757` remain call sites for the moved visual helpers, preserving skill-fire behavior while reducing the party partial surface.
- `Pakuri/Assembly-CSharp.csproj:85` includes `Assets\Scripts\Combat\Manager\CombatRuntimeManifestedPartyVisuals.cs`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs:9` now owns generic manifested non-projectile skill fire.
- `CombatRuntimeManifestedPartyDamage.cs:63`, `:81`, `:112`, and `:124` now own manifested projectile fire entry points, pierce resolution, and projectile object/runtime creation.
- `CombatRuntimeManifestedPartyDamage.cs:184`, `:236`, `:258`, and `:289` now own manifested projectile hit resolution, source follow-up effects, area follow-up damage, and projectile status application.
- `CombatRuntimeManifestedPartyDamage.cs:311`, `:316`, `:335`, `:368`, `:451`, `:458`, `:465`, `:476`, and `:483` now own generic manifested skill damage, effect damage, base damage, damage multiplier, projectile speed, projectile lifetime, projectile hit radius, and status chance helpers.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:351`, `:490`, `:512`, and `:739` now retain only the monster-specific Rin shockwave, persistent field, Eve frost field, and queued Vega projectile call sites in the party partial.
- `Pakuri/Assembly-CSharp.csproj:81` includes `Assets\Scripts\Combat\Manager\CombatRuntimeManifestedPartyDamage.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-E damage/projectile helper slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-E damage/projectile helper slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs` as a `UnityEditor.MonoScript` with guid `87b85aa0eb1d47849e4ae88329a740ef`; after forced script refresh, editor state returned `ready_for_tools=true`, and console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `git diff --check` completed with no whitespace errors and LF-to-CRLF warnings only after the Phase 2-E damage/projectile helper slice.
- External Phase 2 Code Reviewer output was saved to `codex_loop_logs\phase2_manifested_party_reviewer_20260513.md` and ended with `REVIEW_RESULT: PASS`.
- Reviewer evidence found no missing referenced helper, duplicate method definition, new null-risk regression, or behavior-order regression in the moved Phase 2 code.
- `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html` records the Phase 2-E alignment check against section 9 of `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- The Phase 2-E alignment report concludes that Phase 2 follows the proposal direction for state ownership, Manifested Party first, and no runtime-state singleton migration, while Phase 3 `Projectile / Effect / Drone Simulation Split`, Phase 4 `Enemy Simulation Split`, Phase 5 `Selected Unit Combat Split`, Phase 6 adapter narrowing, and Phase 7 common target/effect migration remain.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-D visual helper slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-D visual helper slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyVisuals.cs` as a `UnityEditor.MonoScript` with guid `6f30f996b52c4bb4ea62492d3b619c4c`; after forced script refresh, console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-C drone lifecycle slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-C drone lifecycle slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs` as a `UnityEditor.MonoScript`; after forced script refresh, editor state returned `ready_for_tools=true`, and console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-B skill dispatcher slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-B skill dispatcher slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs` as a `UnityEditor.MonoScript`; after forced script refresh, editor state returned `ready_for_tools=true`, and console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-A view binder slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-A view binder slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs` as a `UnityEditor.MonoScript`; after forced script refresh, console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
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
- 2026-05-13: User explicitly assigned Code Builder and asked to perform Phase 2-A `Manifested Party View Binder`; Builder moved manifested party status view binding and label/bar refresh helpers into a new partial script.
- 2026-05-13: User explicitly assigned Code Builder and asked to perform Phase 2-B `Manifested Party Skill Dispatcher`; Builder moved manifested skill dispatch and fallback cooldown/magazine ticking into a new partial script behind the manifested party runtime service.
- 2026-05-13: User explicitly assigned Code Builder and asked to start the remaining Phase 2 work; Builder moved manifested Eve drone runtime, deployment, ticking, and cleanup into a new partial script as Phase 2-C.
- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 2-D `Manifested Party Skill Visual / Scene Object Helper`; Builder moved manifested non-drone skill visual duration and visual object helper methods into a new partial script.
- 2026-05-13: User explicitly assigned Code Builder and asked to perform Phase 2-E `Manifested Party Damage / Projectile Fire Helper` and run Code Reviewer for Phase 2; Builder moved generic manifested damage/projectile helper methods into a new partial script before Reviewer execution.
- 2026-05-13: External Code Reviewer returned `REVIEW_RESULT: PASS` for the Phase 2 manifested party refactor.
- 2026-05-13: User asked for a Phase 2-E status report comparing the current refactor with the 2026-05-10 proposal's section 9 recommendations; Designer added `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html`.

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
- Phase 2-A moves manifested party status view binding and label/bar refresh helpers into `CombatRuntimeManifestedPartyView.cs`.
- Phase 2-B moves manifested party skill dispatch and fallback cooldown/magazine ticking into `CombatRuntimeManifestedPartySkills.cs`.
- Phase 2-C moves manifested Eve drone lifecycle into `CombatRuntimeManifestedPartyDrones.cs`.
- Phase 2-D moves manifested non-drone skill visual and scene-object helper methods into `CombatRuntimeManifestedPartyVisuals.cs`.
- Phase 2-E moves generic manifested damage and projectile-fire helper methods into `CombatRuntimeManifestedPartyDamage.cs`.
- Monster-specific special formulas such as Rin shockwave and Eve frost field remain in existing controller partials for behavior preservation.

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
