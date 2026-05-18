# ENEMY_BLACKBOARD

This is the active enemy-domain persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/ENEMY_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/enemy history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current Stage 1 enemy runtime authority and verification baseline.

## Task: 2026-05-18 NewRun Prefix Removal Follow-up

### Task title

Keep enemy spawn references aligned after removing `NewRun` from runtime script names.

### Goals

- Keep enemy spawn manager records aligned with the renamed scene entry type.
- Preserve the existing `EnemySpawnManger` script GUID and scene component reference.

### Constraints

- Role Owner is Code Builder.
- Behavior must remain unchanged.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode only if they want runtime spawn behavior confirmation.

### Evidence

- The previous `NewRunSceneEntryManager.cs` script is now `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs`.
- `SceneEntryManager.cs` references `EnemySpawnManger` for configured enemy spawns.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` records `Pakuri.InGame.SceneEntryManager`, `Pakuri.InGame.StageManager`, and `Pakuri.InGame.EnemySpawnManger`.
- Search found no remaining `NewRunSceneEntryManager`, `NewRunStageManager`, `NewRunStartContext`, or `NewRunStageState` references in scripts, scene assets, prefab assets, asset files, or `Assembly-CSharp.csproj`.
- Runtime/editor builds passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-18: Code Builder removed the `NewRun` prefix from the remaining `NewRun*.cs` runtime scripts after the earlier `EnemySpawnManger` rename.

## Task: 2026-05-18 Enemy Spawn Manager Rename

### Task title

Rename the current NewRunScene spawn manager script to `EnemySpawnManger`.

### Goals

- Rename the existing spawn manager script and class used by `NewRunScene`.
- Preserve Unity scene/component compatibility by keeping the existing script `.meta` GUID.
- Keep `SceneEntryManager` references compiling against the renamed type.

### Constraints

- Role Owner is Code Builder.
- Requested source file `NewRunStageSpawnManager.cs` was not present in `Pakuri/Assets`; the existing wired spawn manager was the former `Pakuri/Assets/Scripts2/InGame/Core/NewRunUnitSpawnManager.cs`, now `EnemySpawnManger.cs`.
- This was a behavior-preserving rename only.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode only if they want to confirm scene component behavior after the Unity script rename.

### Evidence

- The former `Pakuri/Assets/Scripts2/InGame/Core/NewRunUnitSpawnManager.cs` and `.meta` were moved to `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` and `.meta`.
- `EnemySpawnManger.cs.meta` keeps GUID `fa013f8b8851bec4882efe505f98b801`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` now declares `public sealed class EnemySpawnManger : MonoBehaviour`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now references `EnemySpawnManger`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` still references GUID `fa013f8b8851bec4882efe505f98b801` and now has `m_EditorClassIdentifier: Assembly-CSharp::Pakuri.InGame.EnemySpawnManger`.
- `Pakuri/Assembly-CSharp.csproj` now compiles `Assets\Scripts2\InGame\Core\EnemySpawnManger.cs`.
- Search after the rename found no remaining `NewRunUnitSpawnManager` or `NewRunStageSpawnManager` references in scripts, scene assets, or `Assembly-CSharp.csproj`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings when rerun alone after a parallel-build file-lock attempt.
- Unity-MCP force refresh cleared the stale missing-file compile error; console warning/error read showed MCP client handler logs and a UnityEditor.Graphs `NullReferenceException`, not a C# compile error.

### History

- 2026-05-18: User requested Code Builder rename `NewRunStageSpawnManager.cs` to `EnemySpawnManger.cs`; Code Builder inspected the project, found no `NewRunStageSpawnManager.cs`, and applied the rename to the actual wired spawn manager `NewRunUnitSpawnManager.cs`.

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

Current active enemy runtime state summarized and retained for future work. 2026-05-18 Code Builder refactor keeps behavior in the same runtime path while now co-locating enemy skill execution with the enemy combat loop and keeping cooldown helpers in adjacent files.

### Next Actions

- User verifies real in-game cadence, priority, and feel for dual-skill enemies in `NewRunScene`.
- If enemy behavior changes again, update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/RUN/RUN_BLACKBOARD.md`.
- Use the archive snapshot when older MVP or intermediate spawn-sequence history is needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs` now owns enemy tick orchestration and also contains the integrated `EnemySkillExecutor` helper for enemy skill execution and visual/effect dispatch.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySkillCooldown.cs` owns basic/special skill resolution, attack range, support-skill readiness, cooldown ticking, temporary enemy modifier ticking, and the integrated resolved-skill contract types.
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

## Task: 2026-05-18 CSV-Backed Stage1 Enemy Passives

### Task title

Apply Stage 1 enemy passives from CSV passive ID/value fields.

### Goals

- Remove Stage 1 enemy passive stat changes from `StageOneSkill` hardcoded branches.
- Apply `PhysicalDamageUp` only to Physical damage.
- Keep existing defense, crit, healing, and incoming-damage passive behavior through reusable passive IDs.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-verified.

### Next Actions

- User verifies in Play Mode that Physical enemy passive buffs affect only Physical attacks and do not affect non-Physical enemy damage.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` now contains the integrated Stage 1 enemy passive application helper and switches on `EnemyUnitRuntimeModel.PassiveSkillId` instead of `StageOneSkill`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` maps `PhysicalDamageUp` to `PassivePhysicalDamageMultiplier`, `DefenseUp` to all defense stats, `CritChanceUp`, `CritDamageUp`, `HealingUp`, and `IncomingDamageDown`.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitRuntimeModel.cs` now stores `PassiveSkillId`, `PassiveSkillValue`, and `PassivePhysicalDamageMultiplier`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs` multiplies `PassivePhysicalDamageMultiplier` only when the resolved damage attribute is `DamageAttribute.Physical`.
- Unity-MCP editor execution after CSV sync returned `sword=PhysicalDamageUp:0.1:phys=1.1:out=1`, confirming the old generic outgoing multiplier stays at `1` for the swordsman passive.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: Code Builder changed Stage 1 enemy passive application from skill-kind hardcoding to CSV passive ID/value application.
