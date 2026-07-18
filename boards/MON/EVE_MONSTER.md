## Archived History

- Non-July task blocks from `boards\MON\EVE_MONSTER.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/EVE_MONSTER_ARCHIVE_2026-05-18.md`.
- Older monster-wide history remains in `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md` and `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md`.
- This active file now keeps only the current Eve A-J runtime baseline still useful for ongoing work.

# EVE_MONSTER

This is the active Eve-domain persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Scope

- Active focus is the Scripts2 `NewRunScene` Eve A-J path.
- Older RunScene/Manifested/CombatRuntime detail is preserved in archive files and should be read only when older history is actually needed.

## Cross-Board Update Requirements

- Status work: update this file and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- Data/catalog/Offering work: update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- NewRunScene UI or Offering gating changes: update this file and `boards/UI/RUNSCENE_UI.md`.
- Eve reports: update this file when a report changes active Eve facts. There is no active report board.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-17 Eve-A Branch Damage Conversion

### Task title

Convert Eve-A branch enhancement from recursive child projectiles to non-recursive instant chain damage.

### Goals

- Damage nearby enemies immediately when the original Eve-A projectile hits.
- Show a temporary blue line from the hit target to each branch-damage target.
- Prevent branch damage from creating another branch.
- Rename the authored node from `BranchProjectile` to `BranchDamage`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Preserve the existing Eve-A branch chance, target count, damage multiplier, and search-radius values.
- Do not add a prefab, new CSV column, or new graph file for the temporary line.
- Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented. Runtime and Editor builds pass; user Play Mode verification remains.

### Next Actions

- User verifies Eve-A trait 5 and master 1 against clustered enemies in `NewRunScene`.
- Confirm one original hit creates at most the configured two branch-damage applications and no child projectile.
- Confirm the temporary blue lines are visible and branch damage does not recursively fan out.

### Evidence

- `InGameProjectileActor.TryApplyBranchDamage(...)` selects distinct nearby hostile targets, applies immediate damage, and passes `suppressOutgoingDamageTriggers=true`.
- The old branch projectile spawn, fallback projectile, `CloneForChild()`, runtime visual, and prefab fields were removed from the branch spec.
- `InGameProjectileActor` creates a 0.12-second blue `LineRenderer` between the primary hit position and each branch target.
- Eve-A graph rows now use `BranchDamage`: trait 5 remains `0.35 / 2 / 0.7`, and master 1 remains `0.6 / 2 / 0.7 / 4.5`.
- CSV parsing found 2 Eve `BranchDamage` graph rows, 1 node definition, 4 parameter definitions, and 0 remaining `BranchProjectile` CSV rows.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the existing two assembly-version warnings remained.

### History

- 2026-07-17: User changed Eve-A branch from bouncing projectiles to electrical-style nearby damage with temporary blue lines and prohibited branch recursion.
- 2026-07-17: Code Builder implemented the shared projectile runtime change, renamed the node and Eve graph rows, and completed build/CSV checks.

## Task: 2026-07-12 Eve Runtime Visual And Hitbox Migration

### Task title

Move Eve A-E and Eve-C master-2 visual/hitbox construction from prefab instantiation to shared runtime composition.

### Goals

- Build Eve A-E visuals from CSV-owned Sprite, AnimatorController, scale, sorting order, and optional BoxCollider2D data.
- Preserve Eve-D per-shocked-target overlapping collider deployments and Eve-C/E collider-backed zone behavior.
- Keep Eve-C master-2 damage/target/timing on its existing OnExpire Effect graph while replacing only its prefab visual with runtime composition.
- Retain all Eve skill prefab assets and current scene mappings until the later all-monster cleanup pass.

### Constraints

- Role Owner is Code Builder refactoring track; skill blueprints were intentionally not used.
- Eve-C master-2 keeps a zero Collider offset; this task adds no CSV file or column.
- Player-facing timing, damage, targeting, node composition, and prefab assets must remain unchanged.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, CSV/catalog validated, and both C# projects build with 0 errors. Eve-C master-2 now uses its runtime-created BoxCollider2D for OnExpire damage instead of a circle or prefab dependency; Play Mode parity verification remains.

### Next Actions

- User verifies Eve A projectile collision, Eve-B beam width/duration, Eve-C/E collider zones and radius traits, Eve-D global overlapping deployments, and Eve-C master-2 expiry visual in Play Mode.
- User verifies Eve-C master-2 damages every enemy overlapping its runtime `6.52 x 6.11` local hitbox once and does not damage enemies outside that collider.
- User verifies Eve-B slow and Eve-E vulnerable no longer create a separate `RuntimeStatusVisual`, while their legitimate skill visuals still appear.
- Keep `Assets/Prefab/Skill/Eve/*.prefab` until all monster skill visual migrations are complete.

### Evidence

- Eve A/B/C/D/E base rows now carry `runtime_visual_*` data; A/C/D/E also carry their prefab-authored BoxCollider2D sizes.
- `BeamSkillExecutor.cs` and `ZoneSkillExecutor.cs` now prefer shared `RuntimeSkillVisualFactory` composition while preserving their old prefab fallback for unconverted skills.
- `RuntimeEffectVisual` was added to the normalized node definitions; Eve-C master-2 uses it instead of `Eve_c-master-2.prefab` while retaining `EffectDamage` and `EffectTarget(OnExpire)` nodes.
- Eve-C master-2's `RuntimeEffectVisual` row now stores hitbox size `6.52 x 6.11`, scale `0.435692`, and implicit offset `0,0`, copied from the retained prefab's Collider/Transform values.
- `SkillMultiEffectExecutor` creates the one-shot runtime visual before damage when a runtime hitbox is authored, applies radius-trait scale, synchronizes transforms, and routes overlap hits through the shared `InGameZoneSkillActor.ApplyColliderAreaTick(...)`; it does not fall back to circle damage for that authored hitbox.
- `SkillMultiEffectExecutor.cs` now supports runtime visual creation for transient, attached, and zone effect visuals.
- No Eve prefab was deleted and `NewRunScene.unity` was not edited.
- Unity-MCP `Pakuri/Validate CSV Source Data` loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies without validation errors.
- Runtime and Editor `dotnet build --no-restore /p:UseSharedCompilation=false` completed with 0 errors and the existing two MSB3277 warnings.
- `StatusEffectRuntime.CreateStatusData(...)` now copies a skill runtime visual only when its anchor is explicitly `StatusTarget`; Eve-B/E remain at the default `Skill` anchor.
- `InGameCombatManager` creates status-attached runtime visuals with `includeHitbox: false`, preventing a status decoration from inheriting Eve-E's gameplay collider even if visual data is misrouted later.

### History

- 2026-07-12: User approved the work as a Code Builder refactor, required all offsets to stay implicit zero, and deferred prefab deletion until every skill migration is complete.
- 2026-07-12: Code Builder implemented shared Beam/Zone/Effect runtime visuals and migrated Eve A-E plus Eve-C master-2 data.
- 2026-07-12: Fixed Eve-B/E base visuals being reused as `RuntimeStatusVisual`; Eve-D and all Eve prefab assets were left unchanged.
- 2026-07-13: Code Builder made Eve-C master-2 OnExpire damage use the runtime-composed Collider copied from `Eve_c-master-2.prefab`; the prefab remains retained but is not a runtime dependency.

## Task: 2026-07-11 Eve A-J Skill Graph Migration Proposal

### Task title

Design the Eve A-J migration from wide legacy skill data to the current Ariel-style skill graph structure.

### Goals

- Decompose Eve A-J base behavior, 25 active traits, 10 active masters, and 15 passive traits into base, Plan, Effect, and Trigger ownership.
- Reuse current graph nodes and existing wide-runtime features before proposing new common semantics.
- Discard the old magazine Eve-E behavior and use the revised `e-drone-beacon.md` non-magazine zone as the migration authority.
- Identify missing graph files, graph exposure nodes, owner extensions, and genuinely new runtime meanings before implementation.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Implementation follows the approved `boards/MON/EVE_NODE_MIGRATION_PROPOSAL.md` and the matching per-kind Skill Blueprints.
- Evidence is limited to routed Eve A-J runtime CSV rows, current node definitions, graph/runtime consumers, build output, and Unity-MCP CSV validation.
- Active awakening level rows are not present in the current Eve Choice CSV and remain outside this migration proposal.
- No MSW-MCP was used.

### Role Owner

Code Builder / Skill Builder

### Status

Eve A-J graph migration implemented. Source CSV validation and both C# builds pass; user Play Mode verification remains.

### Next Actions

- User verifies Eve A-J base, enhancement, and master combinations in `NewRunScene` Play Mode.
- Pay particular attention to Eve-D overlapping full-field deployments, Eve-E one-generation recast, Eve-G 4%+3% proc composition, and Eve-H status-expire graph damage.

### Evidence

- Created `boards/MON/EVE_NODE_MIGRATION_PROPOSAL.md`.
- Current Eve runtime aggregation returned 10 base rows, 50 Choice rows, 0 graph rows, 34 legacy effect rows, 3 trigger rows, and 0 legacy direct nodes.
- Current graph files exist for projectile, single-attack, and passive, while Eve-B line-attack and Eve-C/E area-attack graph files do not exist.
- The proposal retains existing graph nodes for ordinary damage/cooldown/radius/status/effect composition, exposes existing wide runtime features through graph nodes, and isolates actual new common semantics.
- Eve-D scans the full enemy roster for shocked targets and creates one independent collider-authored deployment at every match; the prefab Collider owns the base footprint and overlapping deployments can damage the same enemy multiple times.
- `StatusFilteredDeployment` is reclassified as graph exposure of the existing wide base/runtime path, and the obsolete `DeploymentSearchRadiusMultiplier` proposal is removed.
- The remaining genuinely new meanings are additive target-status stack damage rate and zone recast.
- Eve-E `약점 고정` reuses `StatusCriticalDamageTakenBonus(0.01)` because `StatusEffectRuntime.SumStacked` already multiplies status data by the vulnerable runtime stack count; only `StatusMaxStacksBonus` needs graph exposure.
- Eve-E is re-authored from the revised reference as a radius 3.2, 5-second, 0.8-second tick, 10-second cooldown non-magazine zone; `플라즈마 붕괴` becomes a guarded one-generation zone recast.
- Current passive base names G-J are shifted relative to the references; the proposal corrects them to G 입자 분리, H 냉각 알고리즘, I 과전류 회로, J 약점 분석.
- Added line-attack and area-attack graph CSVs and authored Eve graphs: projectile 18 rows, line-attack 21 rows, area-attack 30 rows, single-attack 13 rows, and passive 147 rows.
- Removed all 34 replaced Eve legacy effect rows; Eve-G triggers were consolidated from two rows to one base trigger plus `TriggerProcChanceBonus`, and Eve-H now references a Choice Effect graph.
- Implemented shared graph exposure and runtime support for duration/projectile/tick/status/conditional modifiers, target-status stack-rate damage, trigger proc bonuses, status-filtered deployment, and Zone-only `RecastZone`.
- Eve-E is now base radius 3.2, duration 5, tick 0.8, cooldown 10, magazine/reload 0, vulnerable max stacks 10; `플라즈마 붕괴` recasts once after 0.5 seconds for 3 seconds at radius 60%.
- Unity-MCP `Pakuri/Validate CSV Source Data` completed with only the successful 5-monster/8+8-enemy catalog load log.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors; only the existing two assembly-version warnings remain.
- Eve-D base now uses `radius=0` so CSV does not impose an absolute footprint and `hit_target_count=global` so every enemy overlapping each spawned Collider can be hit. Existing `RadiusMultiplier` nodes continue scaling the prefab root, Sprite, and Collider together.

### History

- 2026-07-11: User requested an Eve A-J node migration proposal modeled on the Ariel guide, including every trait/master, Eve-E replacement behavior, existing-feature reuse, and reasons for each new node proposal.
- 2026-07-12: User changed Eve-D from a limited search radius to one full-map scan; the reference and proposal now remove search-range ownership, keep per-target radius-1.8 explosions, and allow overlapping explosion damage.
- 2026-07-12: Code Builder implemented the approved A-J migration, removed replaced legacy rows, regenerated runtime catalogs through Unity validation, and completed compile/data verification. Play Mode verification remains user-owned.
- 2026-07-12: After the user added an enabled `BoxCollider2D` to `Eve_D.prefab`, Code Builder set Eve-D `radius=0` and `hit_target_count=global`; Unity CSV validation passed and Play Mode hit verification remains user-owned.

## Task: 2026-07-11 Eve-E Non-Magazine Field Reference Redesign

### Task title

Redesign the Eve-E reference as a non-magazine area field using Eve-C's field structure.

### Goals

- Remove magazine, reload, single-target tick, and concurrent-field assumptions from the Eve-E design reference.
- Give Eve-E the same area/duration/tick/cooldown tuning axes as Eve-C while preserving Lightning and vulnerable identity.
- Replace the magazine-dependent master with a one-time delayed field recast suitable for later graph-node implementation.
- Keep this pass documentation-only so runtime CSV and code conversion can occur with the later Eve node migration.

### Constraints

- Role Owner is Designer.
- Only `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md` is changed as the skill-design authority in this pass.
- `skills_area_attack.csv`, choice/effect/trigger CSVs, runtime code, prefabs, and scenes remain unchanged.
- Values are grounded in the inspected Eve-C reference and the current Eve-C/E area-attack CSV rows.
- No MSW-MCP was used.

### Role Owner

Designer

### Status

Eve-E reference redesigned; runtime implementation intentionally deferred to node conversion.

### Next Actions

- During Eve graph migration, change Eve-E base data from magazine/single-target behavior to the documented radius 3.2 non-magazine field with a 10-second cooldown.
- Convert the five traits into field/Choice nodes and implement `플라즈마 붕괴` as a delayed one-time field-recast Effect/Trigger graph.
- The recast must inherit the ended field's final damage/tick/critical/vulnerable snapshot, use 60% of its final radius for 3 seconds, and suppress recursive `플라즈마 붕괴` activation.
- Preserve `약점 고정` vulnerable-stack identity and verify whether its current behavior requires a shared node definition/runtime extension.
- User verifies final field cadence, full-area targeting, vulnerable stacking, and master behavior in Play Mode after implementation.

### Evidence

- `c-frost-field.md` defines Eve-C as a non-ammunition area field with radius 3.2, duration 4 seconds, tick interval 0.5 seconds, and cooldown 8 seconds.
- The current `skills_area_attack.csv` disk row still defines Eve-E with radius 0, hit-target count 1, magazine 3, reload 6 seconds, duration 5 seconds, and tick interval 0.8 seconds.
- Updated `e-drone-beacon.md` to define Eve-E as radius 3.2, duration 5 seconds, tick interval 0.8 seconds, cooldown 10 seconds, and damage to every enemy in the field.
- Traits 1-4 now use the Eve-C field axes: radius/duration, tick/status, damage/cooldown, and radius-for-damage tradeoff; trait 5 retains the vulnerable-5 Lightning damage condition.
- Replaced `감시 드론망` with `플라즈마 붕괴`: 0.5 seconds after the original field ends, it recasts once at the ended position for 3 seconds with 60% of the original field's final radius.
- The reference explicitly carries the original final damage/tick/critical/vulnerable snapshot into the recast and forbids the recast from triggering another collapse, preventing an infinite loop.
- Updated the `플라즈마 붕괴` awakening progression to scale delay, recast duration, and recast radius instead of obsolete explosion damage.
- `약점 고정` remains the second master because it is independent of magazine ownership and preserves Eve-E's vulnerable-stack specialization.

### History

- 2026-07-11: User requested changing only the Eve-E reference now, using Eve-C as the field baseline, while deferring runtime changes until Eve's node-based conversion.
- 2026-07-11: User replaced the field-end explosion with a one-time recast at the ended location after 0.5 seconds for 3 seconds at 60% radius.

## Task: 2026-07-14 Eve B Runtime Line Collider Detection

### Task title

Move Eve B line-hit detection onto the shared runtime collider query.

### Goals

- Detect Eve B targets by their runtime hitbox colliders.
- Keep line query offset `(0,0)` and avoid new CSV columns.

### Constraints

- Role Owner is Code Builder.
- Eve B CSV visual data and prefab assets were not changed; only the shared LineAttack execution path changed.
- Original `Eve_B.prefab` collider offset `(0,0)` was used as evidence.

### Role Owner

Code Builder

### Status

Implemented and build-validated; user Play Mode boundary check remains.

### Next Actions

- User verifies Eve B hits targets whose colliders overlap the line edge and misses non-overlapping targets.

### Evidence

- `BeamSkillExecutor` routes Eve B `LineAttack` through `InGameLineAttackActor.ApplyLineTick`.
- `ApplyLineTick` now executes a centered, rotated `Physics2D.OverlapBox(length, width)` and matches results against `UnitRosterEntry.GetHitboxColliders()`.
- `Assembly-CSharp.csproj` builds with 0 errors; Unity-MCP InGame skill validation passed with 0 warnings.

### History

- 2026-07-14: User requested collider-based runtime detection for Eve B together with Vega B.
- 2026-07-14: Code Builder implemented it through the shared LineAttack actor.
