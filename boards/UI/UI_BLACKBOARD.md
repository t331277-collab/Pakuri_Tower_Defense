# UI_BLACKBOARD

## Current State

The field-unit Registry ownership migration is implemented. UI consumers now query `UnitSpawnManager` instead of `InGameCombatManager.UnitRegistry`.

The previous UI and RunScene UI boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/UI/`.

For new UI work, inspect the exact current scripts, scenes, prefabs, UXML, USS, or assets first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-07-29 Field Unit Query Migration

### Task title

Move UI field-unit reads to `UnitSpawnManager`.

### Goals

- Keep UI read-only with respect to field-unit registration.
- Query selected and manifested player models from the shared `UnitSpawnManager` roster.
- Remove the `SpawnedPlayerModel` fallback source.

### Constraints

- Preserve current party panel, damage meter, debug UI, Offering refresh, and auto-skill UI behavior.
- Do not change UGUI objects, scene hierarchy, prefabs, player-facing text, navigation, or input.
- Unity Play Mode UI verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies party portraits, selected-monster damage-meter identity, learned-skill refresh, debug refresh, and auto-skill toggle in Play Mode.

### Evidence

- `MonsterPanelUI` reads `unitSpawnManager.Players` and no longer keeps a CombatManager Registry path or selected-player fallback.
- `DamageMeterUIController` resolves slot zero through `FindPlayerMonsterBySlot(0)`.
- `InGameUIManager`, `DebugUI`, and `InGameUtilityPanelController` receive `UnitSpawnManager` query access instead of `CombatUnitRegistry`.
- Active C# search finds zero `SpawnedPlayerModel` and `InGameCombatManager.UnitRegistry` references.
- Runtime and Editor project builds completed with zero errors.
- Unity Console contained zero errors after script refresh.

### History

- 2026-07-29: User approved one-owner field-unit management and read-only access for all other systems.
- 2026-07-29: Code Builder migrated affected UI consumers without changing UI assets or player-facing behavior.

## Task: 2026-07-29 Learned Skill UI Copy Removal

### Task title

Read learned skills from the shared `UnitSkills` source and rebuild only execution state.

### Goals

- Remove Offering and Debug UI copies from `RunMonsterState` lists into runtime models.
- Query learned active/passive state through `UnitSkills`.
- Preserve current button state, Offering completion, runtime rebuild, and display refresh behavior.

### Constraints

- Do not change UGUI objects, scenes, prefabs, labels, navigation, or player input.
- Keep reward commits routed through `RunSession.RecordOfferingChoice`.
- Unity Play Mode UI verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies Offering buttons, Debug skill labels, modifier panels, learned-skill display, and the next combat's skill list in Play Mode.

### Evidence

- `InGameUIManager` and `DebugUI` no longer define or call `SyncModelStateFromSession`.
- Debug learned-state checks call `state.Skills.HasActiveSkill` and `HasPassiveSkill`.
- Offering and Debug refresh paths retain `SkillExecution.RebuildLearnedSkillState` and display refresh calls.
- Removed copy-symbol search returned zero active production references.
- Runtime and Editor builds completed with zero errors; Unity EditMode tests passed 5/5.
- Unity script compilation returned ready and the post-compile Console contained zero errors or warnings.

### History

- 2026-07-29: User approved `UnitSkills` as the single learned-skill source with post-combat full execution-state rebuilds.
- 2026-07-29: Code Builder removed UI copy helpers and converted UI reads to the shared source.
