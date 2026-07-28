## Archived History

- Non-July task blocks from `boards\MON\SEIN_MONSTER.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-09 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/SEIN_MONSTER.md`.

# SEIN_MONSTER

## Scope

Sein dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Sein file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Active Sein task history is recorded below.

## Task: 2026-07-13 Sein Skill Runtime Visual Migration Design

### Task title

Design the safe migration of Sein prefab-backed skill visuals to runtime-created sprite, animator, and collider objects.

### Goals

- Create `boards/MON/SEIN_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md` using the inspected Rin plan format as structural reference.
- Classify every active Sein prefab visual by easy conversion, schema/runtime extension, retained fallback, or no-target status.
- Preserve current projectile, delayed-impact, zone-collider, Trigger, and Sein-E multi-deployment behavior.

### Constraints

- Role Owner is Designer; no code, CSV, scene, or prefab implementation is part of this task.
- Every conclusion uses inspected current code, active runtime CSV rows, prefab serialization, and Unity-MCP asset/hierarchy output.
- Active skill authoring is currently under `Pakuri/Assets/CSVdata/authoring/`; older `source/` board paths are not treated as current authority.
- Do not add runtime object/collider offset columns or graph params; user fixed both offsets at `(0,0)`.
- Remaining shared runtime/common-logic extensions require explicit user approval before Skill Builder / Code Builder implementation.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Designer

### Status

Design completed. Implementation has not started.

### Next Actions

- User reviews and approves or narrows the proposed Sein-E runtime multi-deployment and optional Sein-C impact-runtime extensions.
- After approval, assign Skill Builder / Code Builder to implement the phased migration from `boards/MON/SEIN_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`.
- Update DATA/asset boards only when implementation actually changes CSV schema/rows, runtime catalog wiring, or scene `EffectManager` mappings.

### Evidence

- Unity-MCP found nine Sein skill prefabs and reported each as a single-root hierarchy with one `SpriteRenderer` and one `BoxCollider2D`; seven also have one `Animator`.
- Serialized prefab inspection resolved exact sprite/controller paths, root scales, collider sizes, and current legacy offsets for all targets.
- User set runtime object/collider offsets to `(0,0)`; the corrected plan removes all proposed offset CSV/graph additions and preserves collider sizes only.
- `RuntimeSkillVisualFactory.cs` already supports one runtime sprite/animator/box; absent offset authoring leaves `RuntimeSkillHitboxSpec.Offset` at `(0,0)`.
- `InGameSkillDefinitionMapper.cs` and `SingleAttackSkillExecutor.cs` currently tie Sein-E multi-deployment/line-style behavior to the prefab path.
- `InGameProjectileActor.cs` currently accepts a prefab-only impact visual, so Sein C's separate projectile and delayed-impact roles cannot both use the one base skill `RuntimeVisual`.
- The created plan records per-target decisions, exact runtime values, required shared changes, migration order, risks, acceptance criteria, and Builder verification.

### History

- 2026-07-13: User asked Designer to create a Sein runtime skill migration document modeled on the Rin runtime visual migration plan.
- 2026-07-13: Designer inspected current Sein prefab assets/hierarchies, runtime CSV references, scene mappings, runtime visual factory, mappers, executors, actor collision paths, and normalized graph visual support, then created the migration plan without implementation changes.
- 2026-07-13: User clarified runtime object and collider offsets are fixed at `(0,0)` and must not receive new authoring columns. Designer removed the offset schema proposal and reclassified current non-zero prefab offsets as intentionally normalized legacy data.

## Task: 2026-07-13 Sein-D Prefab Collider Hit Detection

### Task title

Make Sein-D damage ticks use the authored prefab collider instead of implicit battlefield-wide `Field` targeting.

### Goals

- Let Sein-D use the same `InGameZoneSkillActor` prefab-collider path as Sein-C master 1 and Sein-D master 2.
- Preserve radius fallback when a zone instance has no collider.
- Preserve explicit all-target behavior authored through hit-target-count semantics.

### Constraints

- Role Owner is Code Builder.
- User-authored collider changes in `Sein_C_Master_1.prefab`, `Sein_D.prefab`, and `Sein_D_Master_2.prefab` are preserved without Builder edits.
- No CSV, scene, prefab, public API, or serialized field is changed by the runtime fix.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor-validated; Play Mode collider-boundary verification remains.

### Next Actions

- Verify in Play Mode that Sein-D damages and applies its tick status only to enemies overlapping `Sein_D.prefab`'s collider.
- Verify enemies outside the collider no longer receive Sein-D ticks.
- Reconfirm Sein-C master 1 and Sein-D master 2 continue using their prefab colliders.

### Evidence

- Current area-skill data contains one `Field` skill: `sein-d`, with prefab path `Assets/Prefab/Skill/Sein/Sein_D.prefab`.
- `InGameSkillDefinitionMapper.cs` no longer maps `SkillRuntimeKind.Field` to `Targeting.CoverAll` or `ZoneSkillData.Area.CoverAll`; explicit `hitAllTargets` remains the zone-wide override.
- `InGameZoneSkillActor.Initialize(...)` selects prefab hitbox evaluation when `coverAll` is false and an instantiated collider exists, otherwise retaining its radius fallback.
- Unity-MCP prefab hierarchy inspection found an active root `BoxCollider2D` on `Sein_C_Master_1`, `Sein_D`, and `Sein_D_Master_2`.
- Runtime and Editor C# builds completed with 0 errors; only the existing two MSB3277 assembly-conflict warnings remained on the final sequential build.
- Unity-MCP refresh/compile completed and the cleared console contained 0 errors.

### History

- 2026-07-13: User required Sein-D to follow prefab collider boundaries and added colliders to the three Sein zone prefabs.
- 2026-07-13: Code Builder removed the implicit `Field => CoverAll` mapping, preserved explicit all-target routing, and validated the existing collider-first/fallback zone actor path.

## Task: 2026-07-13 Sein A-J Node Migration

### Task title

Implement the approved Sein A-J migration from wide Choice and legacy Effect authoring to positional skill graph nodes.

### Goals

- Preserve `boards/MON/SEIN_NODE_MIGRATION_PROPOSAL.md` as the implementation contract and result record.
- Map Sein A-J to existing node/runtime functions wherever current code supports the behavior.
- Separate graph exposure/composer extensions from Trigger rows that must remain event envelopes.
- Remove migrated wide Choice behavior and legacy Effect rows after graph parity is authored.

### Constraints

- Role Owner is Code Builder for the implementation phase.
- Every conclusion is based on inspected current runtime CSV, node definitions, materializer, mapper, Effect composer, Trigger runtime, Executor code, and Sein reference files.
- Preserve current gameplay, IDs, prefab/collider contracts, and current authored values during migration.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and source/build validation completed. Prefabs and scenes were not changed. User Play Mode parity verification remains.

### Next Actions

- Verify Sein A-J combinations in Play Mode, especially B consecutive-hit scaling, C delayed/contact follow-ups, D/E persistent status ticks, E multi-deployment, G proc/reload gates, and J target-specific refunds.
- Keep the 17 Trigger rows as event envelopes unless a separate trigger-runtime migration is explicitly designed.

### Evidence

- `boards/MON/SEIN_NODE_MIGRATION_PROPOSAL.md` records the complete A-J mapping, migration sequence, risks, and acceptance criteria.
- Post-migration inspection counted Sein Choice 51, positional graph 121, legacy Effect 0, Trigger 17, and legacy direct node/param 0 rows.
- All 51 Sein Choice rows contain routing/metadata only; graph-migrated wide behavior values remaining outside routing columns count 0.
- `skill_node_definitions.csv` and `skill_node_definition_params.csv` expose `DamageDelayMultiplier`, `ConsecutiveHitDamageBonus`, and `AttachStatusPayload`, plus the existing runtime-consumed `EffectDamage` attack coefficient/tick interval parameters.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` proves the 21-column graph materialization, exact-one-operation Effect rule, monster-level direct-node mixing guard, and passive-owner generated Effect gate inference.
- `PakuriCsvRuntimeData.Build.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSnapshot.cs` materialize the new graph operations into the already-consumed runtime fields.
- Static validation found 0 CSV shape, unknown-node, required-argument, duplicate-order, and Effect-operation-count errors across the inspected runtime skill CSV set.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` each build with 0 errors and 2 pre-existing assembly-conflict warnings.
- Unity-MCP `Pakuri/Validate CSV Source Data` loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies without validation errors; `Pakuri/Sync CSV Runtime Catalog Assets` completed successfully.
- `git diff --name-only` for `Pakuri/Assets/Prefab/Skill/Sein` and `NewRunScene.unity` is empty.

### History

- 2026-07-13: User asked Designer to create a Sein node-migration proposal using the Rin proposal format and existing functions as much as possible.
- 2026-07-13: Designer inspected current Sein data/runtime support, identified two wide-to-graph exposure nodes and one hybrid damage/status Effect composer gap, then created the proposal without implementation changes.
- 2026-07-13: User explicitly assigned Code Builder. Code Builder added the shared node/composer exposure, authored 121 Sein graph rows, removed 19 migrated legacy Effects and all migrated Choice behavior values, retained 17 Trigger rows, and completed build plus Unity source validation.

## Task: 2026-07-13 Sein Skill Runtime Visual Refactor

### Task title

Move Sein A-E skill visuals from prefab-first execution to runtime-composed visual/hitbox execution.

### Goals

- Runtime-compose all currently prefab-backed Sein A-E skill visual targets identified in `SEIN_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`.
- Keep Sein C projectile and impact visuals separate.
- Keep Sein E multi-deployment and line-style collider behavior.
- Use centered runtime roots and colliders with no Sein offset authoring.

### Constraints

- Role Owner is Code Builder; no Skill Blueprint is used.
- Prefab and scene references stay as fallback until user Play Mode verification.
- Play Mode remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and non-Play-Mode verified.

### Next Actions

- User verifies A-E runtime visuals, animation, damage boundaries, C delayed impact, and E deployment count/orientation in Play Mode.
- After parity confirmation, remove retained fallback prefab paths and A/B/C scene mappings in a separate cleanup.

### Evidence

- Base CSV rows now define runtime visuals/hitboxes for `sein-a`, `sein-b`, `sein-c`, `sein-d`, and `sein-e` using inspected prefab values.
- Trigger/choice graph rows now define runtime visuals for A master 2, C master 1/master 2, D master 2, and E master 2.
- Sein C uses `ImpactRuntimeVisual` for its delayed B-1 impact while its projectile uses `Sein_Shoot.png`.
- Runtime visual selection precedes prefab fallback in `ProjectileSkillExecutor`, `InGameProjectileActor.ResolveImpact`, `SkillMultiEffectExecutor`, and `SingleAttackSkillExecutor`.
- All 7 edited CSV files passed row/header column-count checks; both C# projects built with 0 errors; Unity-MCP sync and post-sync validation produced 0 error logs.

### History

- 2026-07-13: User authorized Code Builder to start the refactor and fixed the zero-offset contract.
- 2026-07-13: Runtime composition code/data/catalog wiring was implemented; manual gameplay parity was intentionally left to the user.
