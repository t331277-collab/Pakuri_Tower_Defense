## Archived History

- Non-July task blocks from `boards\MON\RIN_MONSTER.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-08 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/RIN_MONSTER.md`.

## Task: 2026-07-12 Rin Skill Runtime Visual Migration Feasibility

### Task title

Classify Rin skill prefab visuals for Ariel-style runtime composition.

### Goals

- Identify Rin visuals that fit the existing shared runtime sprite/animator/box model.
- Retain prefabs where collider offsets or named child hitboxes carry gameplay meaning.
- Define a behavior-preserving Code Builder migration order without deleting prefabs.

### Constraints

- Role Owner is Code Builder for the approved implementation phase.
- Runtime/CSV implementation is included; prefab deletion and scene-mapping cleanup remain outside this pass.
- Prefab assets remain on disk until all converted paths pass user Play Mode verification.

### Role Owner

Code Builder

### Status

Rin A/B/C/F/D-master1 runtime visual migration implemented and source/build validated. User Play Mode parity verification remains.

### Next Actions

- User verifies Rin A/B/C/F and D master 1 visual/collision parity in Play Mode.
- User verifies Rin E base-area and `CoreHitBox` center effects after explicit prefab-hitbox routing.
- Keep Rin D base and Rin E prefab-backed; retain converted prefab/scene fallback references until parity is confirmed.

### Evidence

- Unity-MCP inspected all seven Rin skill prefab hierarchies: A/B/C/D/D-master1/F are single-root; E alone contains `Rin_E/CoreHitBox`.
- Current factory supports one sprite, one animator, uniform scale, and one zero-offset root box collider.
- A/B/C fit the existing runtime path; F fits after passive Trigger CSV columns are exposed.
- D master 1 has collider offset `(0.53632426, -0.41973162)`; user approved preserving it through a shared runtime hitbox-offset extension.
- D base remains prefab-backed by user decision. E remains prefab-backed because it has two differently transformed colliders including named child `CoreHitBox`.
- Active G-J rows contain no prefab or runtime visual path and therefore have no parity-migration target.
- Active Rin-E data maps to `UsePrefabHitbox=false`, while named core lookup only runs inside the prefab-hitbox branch; this is a verified pre-migration blocker.
- Shared runtime hitbox specs now preserve optional offset; D master 1 CSV carries exact size `(3.9373517, 3.788869)` and offset `(0.53632426, -0.41973162)`.
- Rin A/B/C base and Rin F follow-up rows now carry runtime visual data; runtime execution paths prefer those specs over prefab fallback.
- Rin E now carries `use_prefab_hitbox=true`; explicit prefab hitbox with no target count resolves all overlapping targets while retaining target-centered placement.
- CSV shape checks passed for all six edited files. Runtime and Editor builds passed with 0 errors.
- Unity-MCP source validation loaded 5 monsters without validation errors. No Rin prefab or `NewRunScene` diff exists.

### History

- 2026-07-12: User requested Rin A-J prefab-to-runtime feasibility verification using the Ariel migration approach.
- 2026-07-12: Designer classified A/B/C/D as easy, F as a small schema-exposure conversion, D master 1 as conditional, E as prefab-retained, and G-J as having no current visual prefab target.
- 2026-07-13: User selected Rin D base for prefab retention and Rin D master 1 for runtime conversion; Designer revised the handoff to preserve D master 1's non-zero collider offset through a shared optional offset extension.
- 2026-07-13: Code Builder implemented the approved runtime visual rows, shared offset support, and Rin-E explicit prefab-hitbox routing; prefab deletion/scene cleanup deferred until user Play Mode parity.

## Task: 2026-07-12 Rin A-J Node Migration Proposal

### Task title

Design Rin A-J migration from wide/legacy skill authoring to positional skill graphs.

### Goals

- Move Rin base/Choice/Effect behavior to existing graph kinds while preserving Trigger event envelopes.
- Reuse current wide/direct/runtime meanings and introduce no new gameplay semantics.
- Preserve Rin-E `CoreHitBox` and existing skill prefab contracts during the node migration.

### Constraints

- Role Owner is Code Builder for the approved implementation phase.
- Existing prefab, scene, and Rin-E `CoreHitBox` contracts remain unchanged.
- Rin graph rows and Rin legacy direct nodes cannot coexist in one materialized dataset.

### Role Owner

Code Builder

### Status

Rin A-J positional graph migration implemented and source/build validation completed. Play Mode behavior verification remains.

### Next Actions

- Verify Rin A-E damage, targeting, reload, slow, execute, `CoreHitBox`, and hit-count refund behavior in Play Mode.
- Verify Rin F-J Trigger cadence and passive Effect gates in Play Mode.

### Evidence

- Inspected Rin reference A-J files, normalized base/Choice/Effect/Trigger/direct-node CSV rows, node definitions, materializer, mapper, executors, status runtime, and Rin prefabs.
- Current Rin data contains base 10, Choice 50, graph 0, legacy Effect 20, Trigger 17, direct node 11, and direct param 22 rows.
- All needed graph kind files already exist; no new graph CSV file is proposed.
- Every requested Rin behavior already has a current wide/direct/Effect/Trigger runtime meaning, so the proposal requires zero new gameplay semantics.
- Rin now materializes from 138 positional graph rows; Rin legacy Effect rows, legacy direct nodes/params, and non-routing Choice behavior values are all zero.
- All 17 Rin Trigger rows remain; the two Rin-I kill triggers now reference Trigger-owned Effect graphs.
- Runtime and Editor C# builds completed with 0 errors; Unity `Pakuri/Validate CSV Source Data` completed without validation errors.
- `git diff --name-only -- Pakuri/Assets/Prefab Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` returned no changed prefab or scene path.

### History

- 2026-07-12: User requested an Eve-format Rin node migration proposal that maximizes reuse of existing features.
- 2026-07-12: Designer created `RIN_NODE_MIGRATION_PROPOSAL.md` and retained prefab/Trigger compatibility boundaries.
- 2026-07-12: Code Builder exposed the approved shared node meanings, migrated Rin A-J to positional graphs, removed overlapping Rin legacy authoring, and completed source/build validation.

## Scope

Rin dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Rin file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Not populated yet.
