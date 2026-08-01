# MON_BLACKBOARD

## Current State

There is no active monster-specific task block after the 2026-07-28 cleanup.

The previous Ariel, Eve, Rin, Sein, and Vega boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/MON/`.

For new monster work, inspect the exact current code and data first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-08-01 Monster Prefab Binding Migration

### Task title

Replace hardcoded playable-monster prefab selection with `MonsterPrefabBinding[]`.

### Goals

- Remove the five monster ID constants and five individual monster prefab fields from `UnitSpawnManager`.
- Resolve playable monster prefabs through serialized ID-to-prefab bindings.
- Preserve the existing Ariel, Eve, Rin, Sein, and Vega prefab references.

### Constraints

- Preserve `ResolveMonsterPrefab(string)` callers and spawn behavior.
- Keep monster data ownership in `MonsterDefinition`; keep scene prefab references in `UnitSpawnManager`.
- Do not modify unrelated user changes.

### Role Owner

Code Builder

### Status

Implemented and verified.

### Next Actions

- User verifies playable monster spawning in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs:25` now owns `MonsterPrefabBinding[]`.
- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs:435` resolves by binding ID.
- Unity component inspection reported five bindings: `ariel`, `eve`, `rin`, `sein`, `vega`, each with a prefab path.
- Unity script validation returned 0 warnings and 0 errors.
- Core and Editor builds completed with 0 errors; only the existing two assembly-reference warnings remain.

### History

- 2026-08-01: Code Builder replaced hardcoded playable-monster prefab routing with serialized bindings and preserved the existing prefab assets.
