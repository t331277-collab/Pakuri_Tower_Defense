# ENEMY_BLACKBOARD

This is the active enemy-domain persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archive Note

- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-05-26 SingleAttack CSV Damage Delay Runtime

### Task title

Add CSV-authored delayed damage timing and animation-length visual lifetime for SingleAttack.

### Goals

- Let each `SingleAttack` row tune hit timing with `damage_delay_seconds`, defaulting existing rows to `0`.
- Keep visual prefabs spawning immediately while delaying only damage/status/on-hit follow-up resolution.
- Destroy SingleAttack visual/hitbox prefabs by animation clip length instead of the previous fixed `1f` lifetime.

### Constraints

- Role Owner is Code Builder.
- This is a shared SingleAttack runtime extension, not monster-specific hardcoding.
- Existing rows keep `damage_delay_seconds=0` for immediate-hit compatibility.
- Unity Play Mode gameplay verification remains user-owned.
- Unity batchmode CSV runtime sync was attempted but blocked because the project is already open in another Unity instance.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. CSV runtime catalog asset sync remains pending in the open Unity Editor or by rerunning `SyncCsvRuntimeCatalogs.bat` after closing the project.

### Next Actions

- User sets nonzero `damage_delay_seconds` values on desired `SingleAttack` rows and verifies hit feel in Play Mode.
- Run `Pakuri/Sync CSV Runtime Catalog Assets` in the open Unity Editor, or close Unity and rerun `SyncCsvRuntimeCatalogs.bat`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` now exposes `DamageDelaySeconds` with default `0`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now reads `skill.DamageDelaySeconds`, schedules delayed hit resolution through coroutines, and keeps immediate behavior when the value is `0`.
- `SingleAttackSkillExecutor.cs` now delays `UsePrefabHitbox` damage until after `WaitForSeconds(...)`, then calls `Physics2D.SyncTransforms()` before `ApplyPrefabHitbox(...)`.
- `SingleAttackSkillExecutor.cs` now resolves visual lifetime from child `Animator` / legacy `Animation` clip lengths and falls back to `1f` only when no animation length exists.
- CSV parser verification returned `records=52`, `fields=56 records=52`, `damage_delay_index=50`, `type=float`, and `nonzero_defaults=0` for `monster_skills.csv`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed with Unity's duplicate-project-open guard: another Unity instance has `C:/TowerDefence_Pakuri/Test/Pakuri` open.

### History

- 2026-05-26: User chose the N-second delayed damage approach and requested Code Builder implementation with default value `0` plus animation-length prefab deletion.

## Task: 2026-05-24 Shared Skill On-Hit Additional Damage Runtime

### Task title

Add a reusable skill hit rider for direct extra damage and every-nth-hit chain damage.

### Goals

- Let skill choices add immediate extra damage to the actual hit target without using `SingleAttack` as a fake triggered skill.
- Let skill choices count primary hits and run deterministic chain damage every nth hit.
- Apply the shared option from projectile, beam, zone, and single-attack hit paths.

### Constraints

- Role Owner is Code Builder.
- This is a shared runtime extension, not Rin-only hardcoding.
- Added damage calls are guarded through the shared helper so additional damage does not recursively invoke itself.
- On-hit extra and chain damage must not dispatch outgoing-damage triggers again; they are rider damage attached to the primary hit, not new trigger roots.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that direct on-hit extra damage and every-third-hit chain damage feel correct under real combat timing.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now stores `SkillHitCount` and exposes `AdvanceSkillHitCount()`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillOnHitAdditionalDamageUtility.cs` owns the shared on-hit extra damage and chain target selection.
- `InGameProjectileActor`, `InGameLineAttackActor`, `InGameZoneSkillActor`, `ProjectileSkillExecutor`, and `SingleAttackSkillExecutor` now call the shared helper after primary hit damage/status resolution.
- `SkillOnHitAdditionalDamageUtility` uses an in-helper execution flag so additional damage calls do not recursively invoke the same on-hit additional damage path.
- `SkillOnHitAdditionalDamageUtility.cs:69` and `SkillOnHitAdditionalDamageUtility.cs:110` pass `suppressOutgoingDamageTriggers: true` through `InGameCombatManager.ApplyDamage`, so direct extra and chain rider damage do not fan out through `OnOutgoingDamage` triggers.
- `InGameCombatManager.cs:388` returns before `SkillTriggerRuntime.ExecuteOutgoingDamage` when `DamageApplicationOptions.SuppressOutgoingDamageTriggers` is set.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity console after sync/validation showed only MCP client handler connection exceptions as errors, not C# compile errors.

### History

- 2026-05-24: User asked to generalize the Rin-A master-2 idea into a skill on-hit option usable by projectile, zone, single-attack, and beam skills.
- 2026-05-24: User chose nth-hit as the chain criterion and reported direct 40% Lightning rider damage behaving like all-target damage. Code inspection showed hit-target damage was target-only, but it used the normal outgoing-damage trigger path; rider damage now suppresses outgoing-damage trigger dispatch.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/ENEMY_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/enemy history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current Stage 1 enemy runtime authority and verification baseline.

## Task: 2026-05-24 Projectile Nth Launch Branch Runtime Extension

### Task title

Add a reusable projectile launch counter path for nth-launch branch chance overrides.

### Goals

- Let projectile choices express branch chance overrides on every nth base projectile launch.
- Support `Rin-a` master-2 style "every 3rd launched projectile branches at 100%" without hardcoding monster or skill IDs.
- Preserve existing branch-on-hit behavior for choices that only use branch chance/count/damage/search fields.

### Constraints

- Role Owner is Code Builder for the shared runtime extension and Skill Builder for future skill row application.
- This task changed shared runtime/data definitions only; no skill CSV row values, prefab assets, or scene objects were edited.
- Skill Builder did not inspect current Rin CSV rows because the user did not explicitly authorize current CSV/code discovery as parsed source.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder / Skill Builder

### Status

Shared runtime extension implemented and compile-verified. Specific `Rin-a` enhancement/master rows still require parsed input or explicit CSV discovery authorization before Skill Builder can edit data.

### Next Actions

- To apply `Rin-a` master-2 through data, provide the parsed row bundle or explicitly authorize Skill Builder to use current CSV/code as the parsed source.
- Required master-2 parsed values include target skill/choice identity plus `branch_launch_period=3`, `branch_launch_chance_set=1`, and any intended branch count/damage/search overrides.
- Remaining `Rin-a` enhancements and master-1 need their parsed effect values before implementation.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now stores `ProjectileLaunchCount` and exposes `AdvanceProjectileLaunchCount()`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now advances the launch count for each instantiated base projectile and resolves branch chance per projectile launch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs`, `SkillChoiceEffectSpec.cs`, `SkillChoiceModifierRecord.cs`, and `SkillChoiceDefinition` now carry `BranchLaunchPeriod` plus `BranchLaunchChanceSet`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` can read optional `branch_launch_period` and `branch_launch_chance_set` columns when present.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.CsvSupport.cs` now exposes `HasColumn(...)` so optional new columns do not break older CSV tables that do not contain them.
- `boards/SkillBluePrint/projectile-blueprint.md` now treats nth-launch branch chance override as part of the current common projectile path when parsed fields are provided.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; the same existing `MSB3277` warnings remained.
- Unity-MCP script refresh recovered after domain reload and returned ready; console warning/error read showed MCP client handler connection logs, not C# compile errors.

### History

- 2026-05-24: User approved the reusable shared extension where projectile launch count, not hit count, drives every nth launch branch behavior.

## Task: 2026-05-24 Skill Executor File Boundary Split

### Task title

Split the monolithic skill executor source into responsibility-specific execution files.

### Goals

- Remove the oversized `SkillExecutors.cs` owner and keep each executor in its own source file.
- Preserve the existing `SkillExecutorRegistry` type-based routing and runtime behavior.
- Move shared status-spec construction out of `ProjectileSkillExecutor` so beam, zone, single-attack, buff, and shield executors do not depend on the projectile executor for common status work.

### Constraints

- Role Owner is Code Builder.
- This is a behavior-preserving refactor only; no CSV rows, prefab assets, scene objects, runtime tuning, or skill contracts were intentionally changed.
- Unity Play Mode gameplay verification remains user-owned.
- Existing unrelated workspace changes under Rin assets and CSVRuntime assets were not touched by this task.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode only if they want gameplay confirmation after the source-file split.
- Future skill executor changes should edit the narrow executor file or common utility file instead of reintroducing a combined `SkillExecutors.cs`.

### Evidence

- The previous `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `.meta` were removed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/TypedSkillExecutor.cs`, `ProjectileSkillExecutor.cs`, `BeamSkillExecutor.cs`, `ZoneSkillExecutor.cs`, `SingleAttackSkillExecutor.cs`, `BuffSkillExecutor.cs`, `ShieldSkillExecutor.cs`, `PassiveSkillExecutor.cs`, `SkillMultiEffectExecutor.cs`, `SkillExecutionUtility.cs`, and `SkillStatusSpecUtility.cs` now hold the split responsibilities.
- `Pakuri/Assembly-CSharp.csproj` now includes the new split skill execution files and no longer includes `SkillExecutors.cs`.
- Search after the split found no remaining `ProjectileSkillExecutor.ResolveStatusSpec` or `ProjectileSkillExecutor.ResolveStatusData` callers.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` `System.Net.Http` / `System.IO.Compression` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; the same existing `MSB3277` warnings remained.
- Unity-MCP force refresh and script compilation returned the editor to idle; console error read showed only MCP client handler logs, not C# compile errors.

### History

- 2026-05-24: Code Builder split the monolithic skill executor file into executor-specific files plus common status, multi-effect, and execution utility files while preserving registry routing.

## Task: 2026-05-24 Skill Execution Folder Role Organization

### Task title

Organize skill execution scripts into role-specific subfolders and consolidate tiny support files.

### Goals

- Move the current `Execution` root scripts into responsibility folders so the folder layout matches runtime ownership.
- Consolidate tiny contract/model/support executor files without changing public type names or namespaces.
- Preserve MonoBehaviour file names and moved `.meta` files for actor/UI scripts.

### Constraints

- Role Owner is Code Builder.
- This is a behavior-preserving structure refactor only; no CSV rows, prefabs, scenes, skill tuning, public type names, or namespaces were intentionally changed.
- Unity Play Mode gameplay verification remains user-owned.
- Existing unrelated workspace changes under Rin assets and CSVRuntime assets were not touched by this task.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode only if they want runtime gameplay confirmation after the folder move.
- Future skill execution code should be added under the matching folder: `Executors`, `Actors`, `Runtime`, `Utilities`, `Contracts`, `Modifiers`, or `UI`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution` now has no root `.cs` files; `.cs` files are under `Actors`, `Contracts`, `Executors`, `Modifiers`, `Runtime`, `UI`, and `Utilities`.
- `SkillExecutionContext`, `SkillExecutionResult`, and `SkillExecutionStatus` are now in `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Contracts/SkillExecutionModels.cs`.
- `IInGameSkillExecutor` and `TypedSkillExecutor<TSkillData>` are now in `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Contracts/SkillExecutorContracts.cs`.
- `BuffSkillExecutor`, `ShieldSkillExecutor`, and `PassiveSkillExecutor` are now in `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SupportSkillExecutors.cs`.
- `Pakuri/Assembly-CSharp.csproj` now includes the new role-folder paths and no longer includes the removed root-level contract/model/support executor files.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` `System.Net.Http` / `System.IO.Compression` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; the same existing `MSB3277` warnings remained.
- Unity-MCP script refresh returned to idle; console warning/error read showed only MCP client handler connection logs, not C# compile errors.

### History

- 2026-05-24: Code Builder moved skill execution source files into role folders and consolidated the tiny contract/model/support executor files while preserving type names and namespaces.

## Task: 2026-05-24 Shared Skill Executor Helper Consolidation

### Task title

Consolidate duplicated shared skill-execution helpers and remove scene-name hardcoding from prefab-hitbox center resolution.

### Goals

- Keep sibling skill executors using one shared implementation for rotation, ordered target resolution, and prefab hitbox scaling.
- Remove the `GameObject.Find("SkillPoint")` scene dependency from `SingleAttackSkillExecutor` so `HitAllTargets` prefab-hitbox skills resolve from runtime combat context instead of a hardcoded scene object name.
- Preserve current runtime behavior contracts and serialized data compatibility while reducing duplication.

### Constraints

- Role Owner is Code Builder.
- This task is a behavior-preserving refactor of shared combat runtime code; it does not change CSV rows, prefab assets, or scene serialization.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that `HitAllTargets` prefab-hitbox skills still center correctly during real combat after the `SkillPoint` fallback removal.
- If another executor needs target ordering or prefab scaling, route it through `SkillExecutionUtility` instead of reintroducing local duplicates.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:2526` now owns shared `ResolveRotation(...)`, `ApplyPrefabScale(...)`, `ResolveOrderedTargets(...)`, and `ResolveTargetGroupCenter(...)` helpers inside `SkillExecutionUtility`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:140`, `:533`, `:739`, `:827`, `:1097`, `:1212`, `:1266`, and `:1312` now route projectile, beam, zone, and single-attack executor paths through those shared helpers instead of local duplicates.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1053` and `:1057` now resolve `HitAllTargets` prefab-hitbox centers through `ResolveTargetGroupCenter(...)`; the previous `GameObject.Find("SkillPoint")` scene-name lookup is no longer present in the file.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` assembly-version warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; the same existing `MSB3277` warnings remained.

### History

- 2026-05-24: Code Builder consolidated duplicated helper logic from `ProjectileSkillExecutor`, `BeamSkillExecutor`, `ZoneSkillExecutor`, and `SingleAttackSkillExecutor` into `SkillExecutionUtility`, then replaced the hidden `SkillPoint` scene dependency with target-group-center resolution from runtime context.

## Task: 2026-05-18 Stage1 Enemy Runtime Authority

### Task title

Keep the current Stage 1 enemy runtime grounded in the active CSV-plus-scene authority split.

### Goals

- Keep Stage 1 enemy composition driven by `StageEncounter.csv`.
- Keep Stage 1 enemy skill tuning driven by `stage_one_enemies.csv`, `EnemySkillData.csv`, and the runtime enemy model path.
- Keep enemy skill visual prefabs scene-authored through `EffectManager` in `NewRunScene`.
- Keep one basic skill plus one special skill per enemy in the current combat simulation.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed implementation history before this cleanup is preserved in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active enemy runtime state summarized and retained for future work. 2026-05-18 Code Builder refactor keeps behavior in the same runtime path while now co-locating enemy skill execution with the enemy combat loop. 2026-05-18 follow-up then absorbed the former cooldown helper into the same owner and renamed that owner to `EnemyCombatSystem.cs`.

### Next Actions

- User verifies real in-game cadence, priority, and feel for dual-skill enemies in `NewRunScene`.
- If enemy behavior changes again, update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/RUN/RUN_BLACKBOARD.md`.
- Use the archive snapshot when older MVP or intermediate spawn-sequence history is needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` now owns enemy tick orchestration, basic/special skill resolution, attack range, support-skill readiness, cooldown ticking, temporary enemy modifier ticking, the integrated resolved-skill contract types, and the integrated `EnemySkillExecutor` helper for enemy skill execution and visual/effect dispatch.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyTargeting.cs` still owns nearest-player target lookup and enemy-ally support target lookup.
- `EnemyCombatState` stores separate `BasicSkillCooldownRemaining` and `SpecialSkillCooldownRemaining`.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` and `Assets/Scenes/NewScene/NewRunScene.unity` own enemy skill visual prefab mappings.
- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` carries the current `basic_skill` plus `stage_one_skill` authored split.
- `Pakuri/Assets/CSVdata/EnemySkillData.csv` carries active Stage 1 skill tuning rows.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` carries the current Stage 1 encounter composition rows used by the stage flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.

### History

- 2026-05-16: Stage encounter CSV seeding and StageManager-driven spawn wiring were recorded.
- 2026-05-17: Enemy skill tuning was split out of enemy rows into `EnemySkillData.csv`.
- 2026-05-18: Dual-skill enemy runtime and scene-owned effect authority became the active baseline.
- 2026-05-18: Code Builder split `EnemyCombatSimulationSystem` into orchestration, cooldown, targeting, execution, and shared runtime-data files.
- 2026-05-18: Code Builder later merged `EnemySkillExecutor.cs` into `EnemyCombatSimulationSystem.cs` and merged `EnemySkillRuntime.cs` into `EnemySkillCooldown.cs` during the repository-wide high-integration consolidation pass.
- 2026-05-18: Code Builder then absorbed `EnemySkillCooldown.cs` into the renamed `EnemyCombatSystem.cs` owner and also absorbed `CombatStatModels.cs` into `DamageCalculator.cs`.
