# ENEMY_BLACKBOARD

This is the active enemy-domain persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/ENEMY_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/enemy history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current Stage 1 enemy runtime authority and verification baseline.

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

Current active enemy runtime state summarized and retained for future work. 2026-05-18 Code Builder refactor keeps behavior in the same runtime path while splitting enemy skill targeting, cooldown, and execution into dedicated helper classes.

### Next Actions

- User verifies real in-game cadence, priority, and feel for dual-skill enemies in `NewRunScene`.
- If enemy behavior changes again, update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/RUN/RUN_BLACKBOARD.md`.
- Use the archive snapshot when older MVP or intermediate spawn-sequence history is needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs` now owns enemy tick orchestration and delegates skill details to `EnemySkillCooldown`, `EnemyTargeting`, and `EnemySkillExecutor`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySkillCooldown.cs` owns basic/special skill resolution, attack range, support-skill readiness, cooldown ticking, and temporary enemy modifier ticking.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyTargeting.cs` owns nearest-player target lookup and enemy-ally support target lookup.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySkillExecutor.cs` owns enemy skill execution and visual/effect dispatch for the current enemy skill set.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySkillRuntime.cs` owns shared enemy skill slot and resolved-skill data types.
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
