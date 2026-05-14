# ENEMY_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

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
