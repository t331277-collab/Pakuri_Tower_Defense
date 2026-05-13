# ENEMY_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

## Archive Note

- This file had no dated `## Task:` / `## Recent Task:` headings.
- Existing task blocks were moved to `boards/ARCHIVE/BLACKBOARD_UNDATED_ARCHIVE_2026-05-12.md` on 2026-05-12.
- Source file: `boards/COMBAT/ENEMY_BLACKBOARD.md`.

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
