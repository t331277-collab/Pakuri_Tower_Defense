# MON_BLACKBOARD

## Current State

There is no active monster-specific task block after the 2026-07-28 cleanup.

The previous Ariel, Eve, Rin, Sein, and Vega boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/MON/`.

For new monster work, inspect the exact current code and data first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-08-03 Boss HP Priority Display

### Task title

Show one highest-maximum-HP active boss through Canvas `BossHP` while lower-priority bosses retain their prefab HP displays.

### Goals

- Select active `IsBoss` enemies by `Stats.MaxHealth`.
- Hide only the selected boss's `MonsterHpBar` and show `Canvas/BossHP`.
- Move to the next highest-maximum-HP boss after the selected boss is defeated.

### Constraints

- Preserve existing boss designation and user-edited enemy prefab transforms.
- Do not mass-edit enemy prefab assets; hide the runtime `MonsterHpBar` root only.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies multi-boss spawn, damage, shield, defeat, and next-boss handoff in `NewRunScene` Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/UI/InGame/Info/BossHpUI.cs` selects live `Model.IsBoss` entries by descending `Model.Stats.MaxHealth` priority and updates the selected entry each frame.
- `Pakuri/Assets/Scripts/Units/Display/UnitHpBar.cs` exposes runtime visibility for the prefab `MonsterHpBar` root; `EnemyActor` forwards the call.
- Enemy prefab scan found 16 prefabs and 0 missing `MonsterHpBar` roots.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors; existing Unity reference-conflict warnings remain.

### History

- 2026-08-03: Code Builder added highest-maximum-HP boss selection, runtime world-bar handoff, and Canvas BossHP synchronization without overwriting the existing user prefab edits.

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
