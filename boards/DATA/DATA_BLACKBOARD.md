# DATA_BLACKBOARD

## Archived History

The pre-cleanup file, including completed and superseded data tasks, is preserved at `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/DATA/DATA_BLACKBOARD.md`.

## Task: 2026-07-28 Skill Trigger / Node Data Contract Design

### Task title

Replace kind-branched graph authoring with Trigger-owned and owner-keyed Nodes.

### Goals

- Remove `graph_kind` from all six `skill_graph_nodes_*.csv`.
- Add no replacement grouping column or intermediate grouping type.
- Move Trigger payload fields into Trigger-owned Node data while Trigger rows retain activation rules.

### Constraints

- Role Owner is Designer for the handoff and Code Builder refactoring track for later implementation.
- Preserve all current IDs, values, asset paths, ordering, gates, and generated catalog behavior during migration.
- Keep the legacy graph reader until converted CSV and runtime parity pass.
- No active CSV was changed in this design task.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Data-contract design complete. Implementation not started.

### Next Actions

- User approves or revises the Node-only schema in `boards/COMBAT/SKILL_TRIGGER_NODE_UNIFICATION_HANDOFF.md`.
- Code Builder adds dual-read/migration support before converting the 508 current Effect rows.

### Evidence

- `SkillGraphParser` currently branches graph kinds and rewrites Effect rows to generated Effect ownership.
- `GameDataCatalogBuilder` separately materializes ordinary Nodes and Effect definitions.
- Active graph authoring contains 508 Effect rows and 256 ordinary modifier rows.
- The handoff removes `graph_kind`, rejects replacement grouping IDs, defines owner-keyed Nodes, expands Trigger events, and specifies parser/validator/catalog/compiler migration.

### History

- 2026-07-28: User requested removal of `graph_kind`, rejection of the intermediate grouping term, and Trigger-based Node activation.
- 2026-07-28: Designer recorded the replacement data contract without editing CSV or runtime catalogs.
- 2026-07-28: Code Builder archived older DATA task history and retained this as the only active DATA task.

## Task: 2026-07-29 Trigger Visual Duration Data Repair

### Task title

Restore explicit one-second lifetime Nodes for standalone Trigger visuals.

### Goals

- Repair ten Trigger-owned Node collections whose `ShowVisual` rows had no `SetDuration`.
- Keep visual lifetime explicit in authoring data.

### Constraints

- No runtime fallback and no validator change.
- Preserve the 19-column Node CSV contract and contiguous owner-local `node_order`.
- Preserve all existing values and add only the missing duration rows.

### Role Owner

Code Builder

### Status

Complete except user-owned Play Mode verification.

### Next Actions

- User verifies one-second visual removal for representative OnExpire, OnHit, OnKill, OnOutgoingDamage, OnShieldExpire, and last-projectile events.

### Evidence

- Five graph CSV files received ten total `SetDuration=1` rows; the line-attack graph required no change.
- All six graph files retain a 19-column width for every header and row.
- Each repaired owner has exactly one positive duration Node, and the standalone non-positive Trigger visual count is zero.
- Unity CSV source validation completed without errors and the runtime catalog loaded 5/8/8 definitions.

### History

- 2026-07-29: User required explicit data duration and prohibited a runtime zero-duration fallback.
- 2026-07-29: Code Builder restored the ten missing lifetime Nodes from reference intent and pre-migration one-second behavior.
