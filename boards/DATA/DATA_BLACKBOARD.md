## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/DATA_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older CSV-transition history remains in `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current runtime CSV authority, cleanup decisions, and archive destinations still needed for ongoing work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-17 Enemy CSV Contract Simplification

### Task title

Collapse Enemy skill assignments into `enemies.csv` and remove unused Enemy CSV columns.

### Goals

- Delete `enemy_skill_loadouts.csv` and its source-catalog contract.
- Add direct `skill_slot_a_id` and `skill_slot_b_id` columns to `enemies.csv`.
- Remove `unit_sprite_path`, `projectile_sprite_path`, and Enemy base `description_text`/`summary`.

### Constraints

- Role Owner is Code Builder.
- Preserve all existing Enemy stats, passives, skill IDs, skill values, runtime visuals, and Trigger rows.
- Do not remove the shared optional description/summary parser used by other runtime datasets.
- Do not remove `EnemyDefinition` sprite API fields in this CSV-contract task; they are no longer populated by Enemy CSV.

### Role Owner

Code Builder

### Status

Implemented and validated.

### Next Actions

- User runs Unity Play Mode parity for Enemy spawn, A/B skill selection, and CombatStart behavior.
- Future Enemy assignment edits are made in the two slot columns of `runtime/enemy/enemies.csv`.

### Evidence

- `enemy_skill_loadouts.csv` and `.meta` are deleted; the runtime source catalog asset and C# catalog/loader/editor/source model contain no loadout field.
- `enemies.csv` has 16 data rows, Stage 1/2 counts of 8/8, 16 unique base IDs, and 0 missing A/B references.
- Ten active Enemy runtime CSV files passed `TextFieldParser` width validation with 0 malformed rows.
- Enemy CSV search found 0 `unit_sprite_path`, `projectile_sprite_path`, `description_text`, or `summary` headers; `passive_summary` intentionally remains.
- Active code/resource search found 0 removed loadout and Enemy sprite-path contract symbols.
- Solution build passed with 0 errors and the existing 2 `MSB3277` warnings.

### History

- 2026-07-17: Code Builder simplified the active Enemy CSV contract and updated the migration report to the direct A/B authority.

## Task: 2026-07-17 OpeningCharge Buff CSV Authority

### Task title

Make the Enemy Buff base/Trigger tables the sole CSV authority for OpeningCharge.

### Goals

- Remove OpeningCharge from the SingleAttack base and Trigger tables.
- Author its movement increase, target-max-health damage ratio, and freeze values in the existing Buff schema.
- Keep exactly one active base row and one active Trigger row for the skill.

### Constraints

- Role Owner is Code Builder.
- Keep the current 42-column Buff base schema and 7-column Trigger schema.
- Add no CSV file or column.
- Keep `ChargeDamageStatus` as the specialized execution profile.

### Role Owner

Code Builder

### Status

Implemented and structurally validated.

### Next Actions

- User verifies the authored `2.5` movement multiplier and `1.0` target maximum-health damage ratio in Play Mode.
- Keep future OpeningCharge tuning in the Buff base row rather than reintroducing a SingleAttack row.

### Evidence

- CSV parsing found `skills_buff.csv` at 42 columns with 0 malformed rows and `buff_skill_triger.csv` at 7 columns with 0 malformed rows.
- CSV parsing found `skills_single_attack.csv` at 44 columns with 0 malformed rows and `single_attack_skill_triger.csv` at 7 columns with 0 malformed rows after removal.
- An aggregate exact-ID check found 1 OpeningCharge base row (`Buff`, `ChargeDamageStatus`, movement `2.5`, ratio `1`) and 1 Trigger row (`Buff`, `CombatStart`).
- `EnemyMigrationDataset` now validates OpeningCharge as `SkillRuntimeKind.Buff`.
- Runtime and Editor C# builds passed with 0 errors; only the pre-existing 2 `MSB3277` warnings remained.

### History

- 2026-07-17: Code Builder transferred the existing OpeningCharge values from SingleAttack authoring to Buff authoring without changing CSV schemas.

## Task: 2026-07-17 BranchDamage Node And Eve-A Graph

### Task title

Replace the projectile-spawning branch node with a non-recursive instant-damage branch node.

### Goals

- Make `BranchDamage` the active node identifier and handler.
- Keep the existing positional parameters: chance bonus, count, damage multiplier, and search radius.
- Update Eve-A trait 5 and master 1 graph rows without changing their tuning values.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Keep the current 21-column graph schema.
- Do not add a CSV file or column.
- Runtime behavior must not require a projectile visual or prefab.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and statically validated; Unity Play Mode verification remains user-owned.

### Next Actions

- User verifies both Eve-A `BranchDamage` choice paths in Play Mode.
- If the temporary line presentation needs tuning, change runtime line presentation only; keep the node data contract unchanged.

### Evidence

- `skill_node_definitions.csv` contains one `BranchDamage` Action definition.
- `skill_node_definition_params.csv` contains four `BranchDamage` params in order: `chance_bonus`, `count`, `damage_multiplier`, `search_radius`.
- `skill_graph_nodes_projectile.csv` contains two Eve `BranchDamage` rows and no `BranchProjectile` row.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSnapshot.cs` now route `BranchDamage`.
- PowerShell `Import-Csv` checks returned 2 Eve graph rows, 1 node definition, 4 parameter definitions, and 0 old node rows.
- Runtime and Editor C# builds passed with 0 errors; only the pre-existing `MSB3277` warnings remained.

### History

- 2026-07-17: User approved the branch semantic change and requested both runtime and node application.
- 2026-07-17: Code Builder renamed the node contract and updated Eve-A graph authoring while preserving existing values.

## Task: 2026-07-16 Enemy Phase 9 CSV Authority Cutover

### Task title

Make merged Enemy CSV, typed base rows, and Trigger rows the only active Enemy data authority.

### Goals

- Remove legacy stage, skill, node, node-param, and catalog inputs.
- Preserve Stage 1/2 output ordering from one `enemies.csv`.
- Validate current data intrinsically without comparing to deleted tables.

### Constraints

- Role Owner is Code Builder.
- Current Enemy Choice/graph CSV remains absent.
- Enemy runtime visual fields remain on base rows with no offset columns.
- Legacy copies under `Assets/Legacy` are archives, not active runtime inputs.

### Role Owner

Code Builder

### Status

Phase 9 data cutover implemented and validated.

### Next Actions

- User verifies gameplay parity in Play Mode.
- Future Enemy tuning edits use only `runtime/enemy/enemies.csv`, typed base, and Trigger files.

### Evidence

- `enemies.csv` has 16 rows, `stage_id` and `sort_order`; Stage 1 and Stage 2 each contain 8 unique sort orders.
- The former 32 loadout rows were folded into `enemies.csv` direct A/B columns on 2026-07-17; typed base files have 16 skill rows and Trigger files have exactly OpeningCharge and Intimidation.
- `PakuriCsvRuntimeData` loader/source/build/validation no longer requires `stage_one_enemies.csv`, `stage_two_enemies.csv`, `EnemySkillData.csv`, `EnemySkillNodes.csv`, `EnemySkillNodeParams.csv`, or the two Enemy catalog CSVs.
- Active runtime search found none of the seven removed legacy CSV filenames.
- Enemy runtime CSV width validation passed across 10 active files after loadout removal.
- Unity Editor source sync/validation logged `[EnemyPhase9Validation] PASS`.

### History

- 2026-07-16: Code Builder completed Phase 9A and 9D data cleanup and made the merged dataset the active authority.

## Task: 2026-07-16 Enemy Phase 9 Data Deletion Gate

### Task title

Verify whether legacy Enemy CSV inputs can be deleted after Phase 7-8.

### Goals

- Confirm current Enemy AI no longer needs legacy execution data during active casts.
- Identify remaining loader and validation dependencies that block CSV deletion.
- Preserve current base-only Enemy skill authoring and future optional Choice boundary.

### Constraints

- Role Owner is Code Builder.
- No legacy CSV is deleted in this task.
- `enemies.csv` cannot become sole authority until loader/build/validation stops requiring the stage and legacy skill tables.

### Role Owner

Code Builder

### Status

Phase 9 data deletion is blocked.

### Next Actions

- Pass Unity Editor CSV validation and 16-skill Play Mode parity.
- Refactor `PakuriCsvRuntimeData` so `enemies.csv`, loadouts, typed base, and Trigger files are sufficient.
- Remove legacy parity validation only in the approved Phase 9 cleanup.

### Evidence

- `PakuriCsvRuntimeData.cs` still defines direct filenames for `stage_one_enemies.csv`, `stage_two_enemies.csv`, `EnemySkillData.csv`, `EnemySkillNodes.csv`, and `EnemySkillNodeParams.csv`.
- `PakuriCsvRuntimeData.Validation.cs` still reports missing legacy stage/skill rows as errors.
- `PakuriCsvRuntimeData.EnemyMigrationDataset.cs` still compares migrated rows against legacy Enemy rows and `EnemySkillData.csv`.
- Current Enemy Choice/graph input remains absent; `SkillChoiceResolver` generalization required no new CSV.

### History

- 2026-07-16: Code Builder verified that Phase 7-8 does not make legacy Enemy CSV deletable yet.

## Task: 2026-07-16 Enemy CSV Runtime Consumption Phase 4-6

### Task title

Consume typed Enemy base/loadout/Trigger CSV as shared runtime skill definitions for all current Enemy skills.

### Goals

- Materialize assigned `SkillDefinition[]` and `SkillTriggerDefinition[]` per Enemy.
- Carry combined coefficients, support multipliers, chain values, charge values, target scope, target selection, projectile lifetime, and runtime hitbox size into typed runtime data.
- Keep legacy CSV parity validation and fallback intact.

### Constraints

- Role Owner is Code Builder.
- `enemies.csv` is not yet sole Enemy definition authority.
- No Enemy Choice/master graph rows.
- No runtime hitbox offset columns.

### Role Owner

Code Builder

### Status

Phase 4-6 runtime consumption implemented and compile/static validated. Unity Editor CSV validation remains.

### Next Actions

- Run Unity CSV sync/validation in the open Editor.
- After Play Mode parity, decide Phase 7/9 authority switch and legacy CSV removal separately.

### Evidence

- `PakuriCsvRuntimeData.Build.cs` builds assigned Enemy base definitions and CombatStart triggers from `enemy_skill_loadouts.csv` and kind-specific base/Trigger files.
- Builder normalizes `FarthestHostile`, `RandomHostile`, `LowestHealthFriendly`, and current/all target labels into shared runtime selection values.
- Builder emits `passive-buff` for FrostPressure, ShieldUp, ChargeCommand, and Intimidation status materialization.
- `SkillDamageSpec` combined AP/SP fields and explicit projectile lifetime are consumed by shared execution utilities.
- Static CSV width output: area 43 columns, single_attack 44 columns, other active base files 42 columns, with no mismatched rows.
- Runtime/Editor solution build passed with 0 errors.

### History

- 2026-07-16: Code Builder connected Phase 0-3 Enemy migration data to Phase 4-6 shared typed runtime execution.

## Task: 2026-07-16 Enemy CSV Migration Phase 0-3

### Task title

Add merged Enemy definitions, separated loadouts, typed base skills, and CombatStart Trigger inputs as a parallel validated migration dataset.

### Goals

- Merge 8 Stage 1 and 8 Stage 2 definitions into `enemies.csv` with `stage_id`.
- Separate Basic/Special assignment into 32 loadout rows.
- Mechanically move 16 skill values and 21 node params into typed base fields.
- Keep Choice/graph inputs absent until a real Enemy enhancement feature exists.

### Constraints

- Role Owner is Code Builder.
- Legacy stage and Enemy skill CSV remain runtime fallback during parity.
- Base rows directly own visual paths; no `visual_override_id`.
- No `runtime_hitbox_offset_x/y` columns.

### Role Owner

Code Builder

### Status

Phase 0-3 data authored, cataloged, and compile-verified. Unity CSV menu validation remains.

### Next Actions

- Run `Pakuri/Validate CSV Source Data` in Unity after script refresh.
- Compare the representative behavior scenarios recorded in the Phase 0 baseline.
- Do not make `enemies.csv` the sole build authority before Phase 4+ parity.

### Evidence

- `Pakuri/Assets/CSVdata/runtime/enemy/enemies.csv`: 16 data rows.
- `Pakuri/Assets/CSVdata/runtime/enemy/enemy_skill_loadouts.csv`: 32 data rows.
- `Pakuri/Assets/CSVdata/runtime/enemy/skills/base/`: 16 active skill rows across projectile, area_attack, single_attack, buff, heal, and shield; passive is header-only.
- `Pakuri/Assets/CSVdata/runtime/enemy/skills/triggers/`: OpeningCharge and Intimidation CombatStart rows.
- CSV width check returned one consistent field count per file.
- All authored runtime visual Sprite/Controller paths exist on disk.
- Independent checks returned `enemy_and_loadout_parity=PASS` for 16 Enemy rows and 32 loadout rows, and `legacy_to_base_parity=PASS` for 16 base rows, 16 legacy action nodes, and 21 legacy params.
- Runtime and Editor C# builds passed with 0 errors; Unity menu CSV validation remains pending.

### History

- 2026-07-16: Code Builder implemented Phase 2 and Phase 3 as a parallel migration dataset with legacy parity validation.

## Task: 2026-07-16 Enemy Shared Skill CSV Migration Design

### Task title

Design a Monster-style typed base CSV structure for current Enemy skills, with optional future Choice and graph inputs.

### Goals

- Keep Enemy definition CSV responsible for unit stats, initial side, brain, passive, and skill loadout identity.
- Move the current cooldown, range, cast, visual, hitbox, damage, status, trigger, and complex action parameters into kind-specific base inputs.
- Preserve current values by mechanical migration rather than inventing new tuning.
- Do not require Choice or graph CSV until Enemy enhancement/master behavior actually exists.

### Constraints

- Role Owner is Designer.
- Proposed files such as `enemies.csv`, `enemy_skill_loadouts.csv`, and the `enemy/skills/` directory do not currently exist.
- Choice examples containing `<designer_value>` are schema illustrations, not approved balance data.
- Future node definitions should use shared authority, but node-definition work is outside the initial base migration.
- Ally conversion is future scope and is not part of the current CSV contract.
- Enemy runtime hitbox authoring uses size only when gameplay requires a collider; no offset columns are proposed.
- Final Enemy definition authority is one `enemies.csv` with `stage_id`; the existing Stage 1/2 files are migration inputs only.
- Runtime visual columns belong directly to typed base skill rows; no visual override table or ID is proposed.

### Role Owner

Designer

### Status

CSV contract and phased migration design documented; no data files changed.

### Next Actions

- Code Builder creates the loader/model path only after confirming the proposed contract against the current runtime parser.
- Move the current 16 skills and 21 parameters into kind-specific base fields without changing values.
- Merge current Stage 1/2 Enemy rows into `enemies.csv` and replace embedded skill columns with `skill_loadout_id`.
- Author Intimidation and OpeningCharge CombatStart relations in kind-specific Trigger CSV files.
- Extend the shared Trigger event contract with `CombatStart`; use the existing `TriggeredSkill` action value rather than introducing a new action name.
- Use shared Hostile/Friendly target scopes instead of duplicating Monster and Enemy executor logic.
- Add initial validation for missing skill IDs, bad loadout references, and missing asset paths.
- Add node/Choice/graph-position validation only when the optional enhancement inputs are introduced.

### Evidence

- Design report: `Pakuri/reference/Report/2026-07-16-enemy-shared-skill-runtime-csv-migration-plan.md`.
- Current active Enemy inputs are `stage_one_enemies.csv`, `stage_two_enemies.csv`, `EnemySkillData.csv`, `EnemySkillNodes.csv`, and `EnemySkillNodeParams.csv`.
- The current skill set contains 2 AreaAttack, 2 Buff, 4 CooldownProjectile, 2 Heal, 2 Shield, and 4 SingleAttack definitions.
- Current target selectors include CurrentTarget, EnemyAlliesInRadius, LowestHealthEnemyAlly, Self, FarthestTower, RandomTower, and AllTowers; Farthest and Random need shared targeting support.
- Current `ChainLightning` stores delayed-chain behavior in one action with inspected values `0.5` damage multiplier, `0.5` delay, radius `7`, and primary-target exclusion; the revised design keeps those current behavior values in a typed base execution profile rather than mandatory graph rows.
- Current `Intimidation` stores `trigger=CombatStart`, target `AllTowers`, action `ApplyOutgoingDamageMultiplierStatus`, and multiplier `0.7`; current `OpeningCharge` also stores `trigger=CombatStart`.
- Current shared `SkillTriggerEvent` lacks CombatStart, while `SkillTriggerActionKind.TriggeredSkill` already exists.
- Inspected Monster projectile, area-attack, single-attack, and buff base headers directly contain damage, coefficients, cooldown, targeting, status, and runtime visual/hitbox values.
- The revised report proposes unit-only Enemy data, a separate loadout table, and kind-specific base tables as the initial required structure; Choice and positional graph tables are future optional inputs.
- The Vega migration plan keeps runtime collider offsets at `(0,0)`, creates no offset columns, and carries only gameplay-required hitbox sizes; the Enemy CSV proposal now follows the same boundary.

### History

- 2026-07-16: Inspected active Enemy CSV contracts and shared Monster CSV/runtime composition; documented the proposed target schema and migration examples.
- 2026-07-16: Revised the CSV contract to keep all current Enemy base behavior in kind-specific base tables and defer Choice/graph files until enhancement or master effects exist.
- 2026-07-16: Removed ally-conversion data/API assumptions and removed Enemy hitbox offset columns from the proposed schema.
- 2026-07-16: Fixed final authority to merged `enemies.csv`, removed visual overrides, and added buff/single-attack Trigger CSV paths for current CombatStart skills.

## Task: 2026-07-14 Choice Definition And Graph Value Authority Cleanup

### Task title

Make `skill_choices_*.csv` definition-only and remove the empty legacy Effect CSV input.

### Goals

- Keep Choice CSV ownership on identity, display text, ordering, and the passive `target_skill_id` relation.
- Keep gameplay values, target routing, source-status gates, and choice prefab metadata in `skill_graph_nodes_*.csv`.
- Remove the zero-row `runtime/monster/skills/effects` input and its catalog/loader dependency.

### Constraints

- Role Owner is Code Builder.
- No Blueprint, monster proposal, UI, scene, or prefab content was read or edited.
- Runtime `SkillEffectDefinition` execution remains; only the empty legacy Effect CSV source path is removed.
- Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented; runtime/editor builds and Unity source/InGame validation passed.

### Next Actions

- User verifies cross-skill Choice application and Rin A master-2 visual behavior in Play Mode.
- Future Choice behavior values and target routing are authored in graph rows, not by restoring Choice wide columns.

### Evidence

- Six Choice files retain all 252 data rows; five files are 7 columns and passive is 8 columns because `target_skill_id` remains before `description_text`. In every file `description_text` is the final column.
- Vega C trait 4/5/master 1 source gate moved to `RequiredSourceStatus` Plan nodes; trait 5 now has explicit A/B/D/E target nodes.
- Rin A master-2 prefab path moved to a Choice/Plan `EffectVisual` node.
- Runtime Choice application filters normalized nodes by `SkillNodeDefinition.TargetSkillId`; Choice source gate/prefab fields are derived from normalized Plan nodes during catalog build.
- Deleted the six zero-data legacy Effect CSVs, their metadata/directories, source-catalog fields, loader/editor discovery, and the obsolete prefab exporter that depended on missing `Assets/Data/GameData/Monsters`.
- `dotnet build Assembly-CSharp.csproj --no-restore` and `dotnet build Assembly-CSharp-Editor.csproj --no-restore`: 0 errors; existing assembly-version warnings only.
- Unity-MCP catalog sync and source validation loaded 5 monsters and 8+8 enemies; InGame skill validation passed with 0 warnings.

### History

- 2026-07-14: Code inspection found all six legacy Effect files had zero data rows while loader/catalog still required them, and found 14 remaining post-description Choice metadata cells.
- 2026-07-14: Code Builder migrated the remaining metadata, reduced Choice schemas, removed the legacy Effect input path, and completed static plus Unity validation.

## Task: 2026-07-13 Vega Positional Graph Migration

### Task title

Vega Active Runtime CSV Positional Graph Migration

### Goals

- Vega A-J active runtime Choice/Effect/direct node를 기존 positional graph CSV로 이전한다.
- Trigger envelope를 유지하고 Trigger-owned Effect를 graph reference로 연결한다.
- 전환 후 중복 wide/legacy authoring이 0인지 검증한다.

### Constraints

- Role Owner는 Code Builder다.
- Blueprint/reference/archive는 범위 밖이다.
- 새 graph 파일과 offset은 추가하지 않는다.
- 이 전환 시점에는 cross-skill `runtime_target_skill_ids`와 source-status gate metadata를 유지했으며, 2026-07-14 definition-only 정리에서 graph node로 이전했다.

### Role Owner

- Code Builder

### Status

- **CSV migration implemented and Unity source validation passed / Play Mode remains**

### Next Actions

1. 사용자 Play Mode에서 Vega A-J graph 조합과 Trigger parity를 확인한다.
2. 전환된 graph가 권위이며 legacy Effect/direct node authoring을 복원하지 않는다.
3. 향후 Vega 변경은 기존 graph/definition schema에서 작성한다.

### Evidence

- 전환 후 Vega 집계: graph 154행/58 graph, legacy Effect 0, Trigger 15, direct node 0, direct param 0, 중복 wide 행동 값 0.
- graph 분포: Plan 45행/35 graph, Effect 109행/23 graph. 기존 projectile/line_attack/buff/single_attack/passive 21열 파일만 사용했다.
- Trigger 11행이 graph reference를 사용하며 모든 참조 대상 graph가 존재한다.
- A trait 4는 `BurstDamageRule(0, 1.5)`, E trait 4는 `TargetStatusCritBonus(..., min_stacks=20)`으로 작성됐다.
- CSV graph schema/required arg/Effect operation/Trigger reference 검사 오류 0, 변경 runtime CSV 30개 shape 오류 0.
- Unity-MCP source catalog validation과 InGame skill validation(`0 warning(s)`)이 통과했다.
- 상세 구현/검증: `boards/MON/VEGA_NODE_MIGRATION_PROPOSAL.md`.

### History

- 2026-07-13: active runtime CSV와 loader/mapper/validation code를 근거로 Vega 데이터 전환안을 기록했다.
- 2026-07-13: Code Builder가 승인된 A/E 수치를 적용하고 positional graph 이전, legacy 제거, Unity 자동 검증을 완료했다.

## Task: 2026-07-13 Sein Positional Graph Migration

### Task title

Move Sein A-J Choice and legacy Effect authoring into normalized positional graphs.

### Goals

- Reuse the existing projectile, area, single-attack, and passive graph files.
- Expose only the missing runtime meanings needed by the approved Sein proposal.
- Remove duplicate wide Choice and legacy Effect authoring after graph parity.

### Constraints

- Role Owner is Code Builder.
- No new graph CSV file or CSV column is introduced.
- The existing 17 Trigger rows remain event envelopes.
- Sein prefabs and scenes remain unchanged.

### Role Owner

Code Builder

### Status

Implemented and source/build validated; Play Mode parity verification remains.

### Next Actions

- Verify A-J graph composition and the retained Trigger behavior in Play Mode.
- Keep legacy Sein Effect rows removed after parity is confirmed.

### Evidence

- Sein totals after migration: positional graph 121, legacy Effect 0, Trigger 17, direct node 0, direct param 0.
- Sein Choice rows remain 51; non-routing wide behavior values remaining after migration count 0.
- Added `DamageDelayMultiplier`, `ConsecutiveHitDamageBonus`, and `AttachStatusPayload` definitions/params and extended positional `EffectDamage` params for values already consumed by runtime code.
- CSV shape and normalized graph validation returned 0 errors; both runtime and editor C# projects build with 0 errors.
- Unity-MCP source validation and runtime catalog sync completed without validation errors.

### History

- 2026-07-13: Designer documented the Sein migration in `boards/MON/SEIN_NODE_MIGRATION_PROPOSAL.md`.
- 2026-07-13: Code Builder implemented 121 graph rows, shared node exposure, legacy-authoring cleanup, and validation without prefab or scene changes.

## Task: 2026-07-12 Rin Positional Graph Migration Design

### Task title

Define the normalized CSV and node-definition handoff for Rin A-J.

### Goals

- Reuse the existing projectile, buff, line, single-attack, and passive graph files.
- Expose current wide/direct Effect meanings through positional node definitions and mapper/composer connections.
- Remove Rin legacy direct nodes and legacy Effects only after equivalent graph rows exist.

### Constraints

- Role Owner is Code Builder for the approved implementation phase.
- No new graph CSV file or gameplay meaning was introduced.
- Trigger rows remain event envelopes where graph Plan cannot currently modify trigger cadence or damage.

### Role Owner

Code Builder

### Status

Rin normalized graph migration implemented and validated; Play Mode behavior verification remains.

### Next Actions

- Verify the materialized Rin definitions and Trigger behavior in Play Mode.
- Keep legacy Rin authoring removed after the Play Mode pass confirms behavioral parity.

### Evidence

- `skill_graph_nodes_projectile/buff/line_attack/single_attack/passive.csv` all exist with the current 21-column schema.
- Rin currently has no graph rows but has 11 direct nodes and 22 direct params; `MaterializeSkillGraphRows(...)` rejects monster-level graph/direct-node mixing.
- The proposal identifies graph exposure for crit, beam width, knockback, reload reduction, core hitbox, hit-count refund, status payload, and Effect conditions using existing runtime fields.
- Added approved definitions/params and mapper/composer connections for the reused wide/direct/Effect meanings.
- Rin data totals after migration: graph 138, legacy Effect 0, Trigger 17, direct node 0, direct param 0, non-routing Choice behavior value 0.
- Every inspected runtime monster skill CSV row matches its header's 21-column graph schema or its own source header shape; `git diff --check` reports no whitespace error.
- Unity `Pakuri/Validate CSV Source Data` loaded the runtime catalog with 5 monsters and reported no validation error.

### History

- 2026-07-12: Designer documented the Rin normalized graph migration in `boards/MON/RIN_NODE_MIGRATION_PROPOSAL.md`.
- 2026-07-12: Code Builder implemented the positional graph rows, shared node exposure, passive gate inference, Trigger graph references, and legacy-authoring cleanup.

## Task: 2026-07-12 Eve Runtime Visual CSV Migration

### Task title

Represent Eve A-E and Eve-C master-2 visuals/hitboxes through normalized runtime CSV data.

### Goals

- Add runtime visual columns to the existing line-attack and area-attack base tables.
- Populate Eve A-E runtime Sprite/Animator/scale/sorting/hitbox values without adding offset columns.
- Add a reusable `RuntimeEffectVisual` node type using the existing graph `arg_1` through `arg_12` columns.

### Constraints

- Role Owner is Code Builder refactoring track.
- No new CSV file and no offset CSV column are introduced.
- Existing prefab-path node behavior must remain compatible for all unconverted skills.

### Role Owner

Code Builder

### Status

Implemented and Unity CSV validation passed. Runtime visual status-target ownership is explicit, and Eve-C master-2's existing `RuntimeEffectVisual` args now own its Collider footprint.

### Next Actions

- Preserve the runtime visual columns and `RuntimeEffectVisual` node definition during future normalized CSV maintenance.
- Use `runtime_visual_anchor=StatusTarget` only for visuals intentionally attached to a status target; blank/default values remain skill-owned.
- User verifies gameplay/visual parity in Play Mode.

### Evidence

- Updated `skills_projectile.csv`, `skills_line_attack.csv`, `skills_single_attack.csv`, and `skills_area_attack.csv` with Eve runtime visual values.
- CSV parser checks returned uniform field counts per edited table: projectile 38, line attack 29, single attack 42, area attack 32, and area graph 21.
- Added `RuntimeEffectVisual` to `skill_node_definitions.csv` and its Sprite, AnimatorController, scale, sorting, and optional hitbox params to `skill_node_definition_params.csv`.
- Eve-C master-2 graph now keeps `EffectDamage` and `EffectTarget(OnExpire)` and replaces only its visual row with `RuntimeEffectVisual`.
- Eve-C master-2's `RuntimeEffectVisual` row uses existing args 5/6 for hitbox size `6.52 x 6.11`; no new CSV file, column, or prefab-path flag was added.
- Unity-MCP source validation loaded the 5-monster, 8+8-enemy runtime catalog successfully.
- `skills_single_attack.csv` now has optional `runtime_visual_anchor`; Ariel-D is explicitly `StatusTarget`, while Eve-D and all other rows remain blank/default `Skill`.

### History

- 2026-07-12: Migrated Eve visual/hitbox authority to existing normalized CSV tables without creating offset columns or deleting prefabs.
- 2026-07-12: Added explicit runtime visual status-target ownership to stop Eve-B/E base visuals from being copied into their applied statuses.
- 2026-07-13: Added Eve-C master-2 runtime hitbox values to the existing 21-column graph row and validated the regenerated runtime catalog through Unity-MCP.

## Task: 2026-07-11 Eve A-J Skill Graph Migration Proposal

### Task title

Define the data/schema handoff for migrating all Eve skills to current skill graph authoring.

### Goals

- Inventory current Eve base, Choice, legacy effect, trigger, and graph ownership.
- Separate existing graph reuse, wide-runtime graph exposure, owner-kind extensions, and new shared semantics.
- Define the new area/line graph-file requirement and legacy-row deletion gates.
- Keep the redesigned Eve-E reference authoritative instead of migrating its obsolete magazine columns.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Implementation follows the approved Eve proposal and matching per-kind Skill Blueprints.
- Current 21-column graph schema and positional node definition model remain the target contract.
- New files, node definitions, params, or shared runtime connections require user approval before Builder implementation.
- No MSW-MCP was used.

### Role Owner

Code Builder / Skill Builder

### Status

Approved Eve data/schema migration implemented and validated; user Play Mode verification remains.

### Next Actions

- User verifies the migrated Eve graph combinations in Play Mode.
- Keep future Eve behavior changes graph-authored; do not restore removed wide/legacy ownership.

### Evidence

- Current Eve data count is base 10, Choice 50, graph 0, legacy effect 34, trigger 3, and legacy direct node 0.
- Legacy effects are Eve-B 2, Eve-C 1, Eve-F 5, Eve-G 4, Eve-H 4, Eve-I 4, and Eve-J 14.
- Existing graph files do not cover line-attack or area-attack, both required by Eve A-J.
- `SkillChoiceModifierRecord`, `SkillExecutionSnapshot`, and runtime executors already consume additional projectiles, shot interval, duration multiplier, status stack changes, conditional damage, status max stacks, branch data, and trigger proc chance through wide data; the proposal routes these as graph exposure rather than new gameplay behavior.
- `StatusElementDamageTakenBonus` exists as a node type but lacks Effect composer consumption, so Eve-I requires owner-kind extension rather than a duplicate node.
- Eve-D uses the existing full-roster status filter and target-position deployment runtime, so `StatusFilteredDeployment` is graph exposure rather than a new runtime meaning; the spawned prefab Collider now owns the spatial footprint.
- Eve-D still needs additive stack-rate bonuses, and Eve-E still needs a guarded snapshot-preserving zone recast.
- Eve-E's stack-scaled critical-damage-taken master can reuse the existing `StatusCriticalDamageTakenBonus` node because `StatusEffectRuntime.SumStacked` multiplies it by runtime status stacks; only the wide `StatusMaxStacksBonus` feature needs graph exposure.
- Added `skill_graph_nodes_line_attack.csv` and `skill_graph_nodes_area_attack.csv`; Unity generated their `.meta` files and the source catalog now references them.
- Authored 229 Eve graph rows in total: A 18, B 21, C/E 30, D 13, F-J 147.
- Deleted all 34 planned Eve legacy effect rows, consolidated Eve-G triggers, and converted Eve-H to triggered graph reference.
- Runtime catalog source validation passed in Unity-MCP; both runtime and Editor C# projects build with 0 errors.
- Eve-D base row now stores `radius=0` and `hit_target_count=global`: the CSV no longer owns an absolute radius, while each collider-backed deployment may hit every overlapping enemy. Radius multiplier graph nodes remain unchanged.

### History

- 2026-07-11: User requested a complete Eve A-J graph migration proposal with existing-feature reuse and evidence-based new-node rationale.
- 2026-07-12: Eve-D changed to a full-map shocked-target scan; removed the search-radius node proposal and reduced genuinely new common meanings from four to two.
- 2026-07-12: Code Builder completed the approved CSV/node/runtime migration and Unity catalog synchronization; Play Mode validation remains user-owned.
- 2026-07-12: Updated Eve-D base targeting authority to prefab Collider size plus `global` per-deployment hits; Unity source validation passed.

## Task: 2026-07-11 Ariel Skill Graph Nodes Migration Design

### Task title

Define the first compatibility migration from Ariel node instances and params to choice-owned skill graph nodes.

### Goals

- Make node CSVs definition-only in the final structure.
- Move actual node composition and values into `choices/{kind}/skill_graph_nodes_{kind}.csv`.
- Use `choice_id` as the owner id for Choice graphs instead of authored behavior names such as `ariel-a-master-2-holy-exposure-on-hit`.
- Preserve legacy effects and non-Ariel node paths until their own migrations complete.

### Constraints

- Role Owner is Designer / Code Builder.
- Ariel-only runtime CSV and compatibility loader implementation is complete; user Play Mode verification remains pending.
- First implementation scope is the 124 current Ariel nodes and 179 Ariel params.
- Legacy effects remain at 96 rows because Eve/Rin/Sein/Vega are not fully node-migrated.
- Rin/Vega legacy normalized nodes remain supported during transition.
- No MSW-MCP was used.

### Role Owner

Designer / Code Builder

### Status

Code Builder implementation and Unity source-data validation completed; user Play Mode regression pending.

### Next Actions

- 사용자 Play Mode에서 Ariel A-J 기본/특성/마스터 조합을 검증한다.
- 다음 몬스터를 명시적으로 전환하기 전까지 Rin/Vega legacy node와 legacy effects를 유지한다.
- Do not delete `skills/effects` until every legacy effect and trigger reference has graph coverage.

### Evidence

- Created `boards/MON/ARIEL_SKILL_GRAPH_NODES_MIGRATION_PLAN.md`.
- Current aggregation returned Ariel 124 nodes, 179 params, 32 used handlers, and 20 Effect groups.
- The target migration produces 39 Choice/Plan rows, 45 Choice/Effect rows, 36 Skill/Effect rows, and 4 Trigger/Effect rows.
- Current non-Ariel compatibility data is 15 legacy normalized nodes and 33 params.
- Current effects remain 96 rows and contain no Ariel rows.
- The plan defines `owner_kind + owner_id + graph_kind + graph_index` identity, generated runtime NodeId/EffectId, trigger graph references, validation, migration order, and deletion gates.
- Runtime authoring now has 32 node definitions, 53 definition-param rows, and 124 Ariel graph rows; legacy Ariel node/param rows are 0 while Rin/Vega 15/33 and shared effects 96 remain.
- Graph distribution is Choice/Plan 39, Choice/Effect 45, Skill/Effect 36, Trigger/Effect 4; all 20 Effect graphs contain exactly one operation node.
- Loader materialization preserves the existing `SkillNodeDefinition`/`SkillEffectDefinition` build path; two Ariel effect triggers use graph reference columns and the internal E-effect source resolves to `ariel-e@effect1`.
- Runtime and Editor dotnet builds completed with 0 errors. Unity-MCP catalog sync and source validation loaded 5 monsters and 8+8 stage enemies with 0 console errors.
- Graph schema follow-up removed `requires_active_choice_id`, `requires_passive_skill_id`, `excludes_passive_skill_id`, `runtime_support_state`, and `runtime_support_notes` from all four Ariel graph CSVs; each graph file now has 21 columns.
- `excludes_active_choice_id` remains as the only graph gate because `Skill/ariel-c/Effect/0` requires the single `ariel-c-master-1` exclusion; Choice Effect required-choice identity is inferred from its owner.
- The follow-up retained all 124 graph rows and one exclusion value, then passed Runtime/Editor builds and Unity-MCP catalog sync/source validation with 0 errors.

### History

- 2026-07-11: User approved a `skill_graph_nodes` single-value-owner direction, requested `choice_id`-based graph identity, required keeping legacy effects until all monsters migrate, and limited first implementation to Ariel.
- 2026-07-11: Code Builder completed the Ariel graph CSV/runtime compatibility migration and Unity source-data validation; only user-owned Play Mode regression remains.
- 2026-07-11: Code Builder reduced the graph instance schema to owner/node/args plus `excludes_active_choice_id`, preserving the only active exclusion while removing five redundant graph columns.

## Task: 2026-07-11 Legacy Effect To Semantic Node Migration Guide

### Task title

Define the current CSV and handler rules for migrating legacy monster effect rows to semantic nodes.

### Goals

- Record current runtime authority boundaries between base, choices, graph instances, node definitions, legacy nodes/effects, and triggers.
- Document positional `arg_N` interpretation and current owner/graph/generated-ID rules.
- Map only currently representable legacy effect fields to semantic operation, target, condition, lifetime, visual, and modifier nodes.
- Require a per-row non-empty-field audit instead of preserving a stale coverage classification from the previous CSV schema.

### Constraints

- Role Owner is Designer.
- This is a documentation handoff; no runtime CSV rows, schemas, catalogs, or code were changed.
- Current skill-kind consolidated CSV files and the graph/definition CSV files remain the active inspected authority.
- Existing handler reuse is mandatory before proposing a new handler.
- Current runtime supports Trigger-owned Effect graphs; current Ariel contains one such graph.
- No MSW-MCP was used.

### Role Owner

Designer

### Status

Guide rewritten from the current graph CSV and loader/composer evidence.

### Next Actions

- Route one selected monster and the minimum kind-specific CSV set before implementation.
- Audit all non-empty legacy effect fields; remove a legacy row only when every behavior field is represented or proven to equal a runtime default.
- Resolve legacy direct-node overlap before adding graph rows for Rin or Vega.
- Obtain user approval before adding missing area/line graph files, node definitions, params, CSV columns, or shared runtime connections.
- Preserve kind-consolidated effect CSV files while any other legacy row remains.

### Evidence

- Rewrote `boards/MON/ARIEL_NODE_DECOMPOSITION_GUIDE.md` for the current 21-column graph instance schema and removed the obsolete direct node/param authoring examples.
- Current CSV aggregation returned 124 Ariel graph rows in 56 graphs: 36 Plan graphs and 20 Effect graphs.
- Current legacy compatibility data is separate: Rin/Vega retain 15 direct node rows plus 33 params, and Eve/Rin/Sein/Vega retain 96 legacy effect rows.
- Current graph files exist for buff, passive, projectile, and single-attack; area-attack and line-attack graph files do not exist.
- Node definitions currently contain 32 types and 53 param definitions; `arg_N` is materialized by `param_order`, undefined args fail validation, and `arg_9~arg_12` have no current definition.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` rejects graph/direct-node mixing per monster, requires exactly one operation per Effect graph, derives Choice/Skill/Trigger target skills, and generates effect IDs from owner kind/id/index.
- `PakuriCsvRuntimeData.Build.cs` supports only a subset of legacy effect fields through the current graph param definitions and Effect composer, so the old 58/16/22 classification was removed.
- Current trigger evidence includes `Choice/ariel-a-master-2/Effect/0` and `Trigger/ariel-j-after-e-action-speed-trigger/Effect/0` graph references.

### History

- 2026-07-11: User requested a current-code-based transition MD for converting the remaining effect CSV behavior into Ariel-style semantic node composition.
- 2026-07-11: User requested revising the guide because the CSV structure changed; Designer replaced the old node/param path and stale coverage audit with the current graph schema, positional args, compatibility rules, and per-row conversion gates.

## Task: 2026-07-10 Ariel Runtime Visual CSV Columns

### Task title

Add base/trigger runtime visual and hitbox CSV columns for Ariel.

### Goals

- Store Ariel runtime visual sprite/controller/scale/sorting data on base skill and trigger CSV rows.
- Store Ariel runtime hitbox size data on collider-backed base skill and trigger CSV rows.
- Keep hitbox shape and trigger-state policy in code because the current runtime path uses one common BoxCollider2D convention.
- Keep node CSV schema unchanged for this pass.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime/monster/skills`.
- Existing node-owned `skill_effect_prefab_path` params are outside this base/trigger/status CSV migration.
- No MSW-MCP was used.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If Unity Editor validation is needed, sync and validate runtime CSV catalogs through Unity-MCP/editor menu after Unity reloads code.
- Treat future deliberate non-zero hitbox offsets as a separate schema extension; Ariel E uses offset `0,0`.

### Evidence

- Added runtime visual columns to `base/projectile/skills_projectile.csv`, `base/buff/skills_buff.csv`, `base/single_attack/skills_single_attack.csv`, `triggers/buff/buff_skill_triger.csv`, and `triggers/projectile/projectile_skill_triger.csv`.
- Ariel rows now carry `runtime_visual_sprite_path`, `runtime_visual_animator_controller_path`, `runtime_visual_scale`, `runtime_visual_sorting_order`, `runtime_hitbox_size_x`, and `runtime_hitbox_size_y` where applicable.
- Runtime code decides hitbox shape and trigger state: positive hitbox size creates a `BoxCollider2D`; projectile runtime visuals pass trigger mode, while non-projectile runtime visual paths default to non-trigger mode.
- Cleared converted Ariel trigger `skill_effect_prefab_path` values and Ariel D `status_effect_prefab_path`.
- CSV field-count verification passed for all five edited CSV files.
- Follow-up after Unity auto-sync reported `skills_projectile.csv` row 4 as 37 columns: the current disk file was verified with `PakuriCsvLineCodec` as 38 columns for row 4, and full runtime skill CSV shape verification returned no bad rows.
- `PakuriCsvRuntimeData.Editor.cs` now refreshes the AssetDatabase with `ForceSynchronousImport` at the start of runtime catalog sync so existing TextAsset references are not read from stale import cache after external CSV edits.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync from `Assets/CSVdata/runtime`, and `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary without CSV fatal errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.

### History

- 2026-07-10: User requested Code Builder implementation from the Ariel runtime visual migration plan, with Ariel prefabs retained but not used as runtime fallback for converted skill paths.
- 2026-07-10: User requested removing the common hitbox shape/trigger CSV columns and letting code own those policies; Code Builder removed those columns and verified all five edited CSV files keep matching row widths.
- 2026-07-10: User reported Unity auto-sync `skills_projectile.csv` row-width failure; Code Builder verified the final CSV shape, forced Unity reimport/sync successfully, and added a sync-time AssetDatabase refresh to prevent stale TextAsset reads.

## Task: 2026-07-09 Ariel F-J Passive EffectTarget Param Cleanup

### Task title

Remove copied Ariel F-J passive EffectTarget defaults from node params.

### Goals

- Implement `boards/TRAIT_MASTER/ARIEL_FJ_PASSIVE_NODE_CONVERSION_PLAN.md`.
- Keep Ariel F-J passive behavior on functional node params instead of copied old effect defaults.
- Remove status/shield passive target defaults that do not carry current behavior: `target_selection=Owner`, `target_shape=Battlefield`, `center_mode=Caster`, and no-visual `visual_anchor_mode=AppliedTargets`.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime/monster/skills`.
- The change is CSV-only; no runtime code expansion was required.
- No MSW-MCP was used.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, CSV-checked, and compile-verified.

### Next Actions

- User verifies Ariel F-J passive combinations in Play Mode if gameplay parity confirmation is needed.
- Keep future passive effect-owned node params minimal: target side, real conditions, lifetime, one-shot flags, source-skill filters, and actual modifier values.

### Evidence

- Edited `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/passive/passive_skill_node_params.csv`.
- Removed copied target defaults from Ariel F-J passive EffectTarget rows, including `target_selection=Owner`, `target_shape=Battlefield`, `center_mode=Caster`, and no-visual `visual_anchor_mode=AppliedTargets`.
- Kept functional params such as `target_side=AllAllies`, `target_side=Enemy`, `apply_once=true`, `duration_seconds`, `bonus`, `multiplier`, `attribute`, `status_id`, `min_stacks`, and `source_skill_id`.
- Made Ariel I EffectTarget rows explicit with `target_side=Enemy` after removing `target_shape=Battlefield` and `center_mode=Caster`.
- Removed-param scan returned `REMOVED_PASSIVE_TARGET_DEFAULTS_OK`.
- Passive node-param reference check returned `PASSIVE_NODE_PARAM_REFS_OK nodes=58 params=68`.
- Full runtime skill CSV shape check returned `CSV_SHAPE_OK files=31`.
- Full node-param reference check returned `NODE_PARAM_REFS_OK nodes=139 params=212 paramFiles=4`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors and 2 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors and 2 warnings.
- `git diff --check` returned no whitespace errors; Git printed line-ending normalization warnings only.

### History

- 2026-07-09: User requested Code Builder to implement the Ariel F-J passive node conversion plan from `boards/TRAIT_MASTER/ARIEL_FJ_PASSIVE_NODE_CONVERSION_PLAN.md`.

## Task: 2026-07-09 Ariel Node-Based Choice/Effect Cleanup

### Task title

Remove wrongly migrated Ariel choice/master effect clones and keep behavior on functional nodes.

### Goals

- Remove copied `MigratedToEffectBinding` effect-owned node groups that duplicated Ariel enhancement/master combinations.
- Keep numeric choice behavior on functional Choice nodes such as `StatusActionSpeedBonus`, `StatusShieldReceivedBonus`, `ShieldAmountMultiplier`, `StatusDamageTakenBonus`, and `StatusDurationBonus`.
- Preserve base effect-owned node groups for actual effects such as Ariel C blessing, Ariel E shield, and Ariel J post-E action speed.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime/monster/skills`.
- Work is based on inspected CSV/runtime code evidence only.
- No MSW-MCP was used.

### Role Owner

Code Builder

### Status

Implemented, CSV-checked, and compile-verified.

### Next Actions

- If editor-side import validation is required, run Unity-MCP/editor menu validation after Unity reloads the changed CSV assets.
- Keep future numeric skill enhancements and master modifiers as functional Choice nodes instead of precombined copied Effect groups.

### Evidence

- Removed the passive copied Effect groups `ariel-j-after-e-action-speed-trait1`, `ariel-g-shield-received-trait1`, `ariel-g-start-shield-trait2`, and `ariel-i-holy-exposure-damage-taken-trait1` from `nodes/passive/passive_skill_nodes.csv` and matching params from `nodes/passive/passive_skill_node_params.csv`.
- Removed the single-attack copied Effect groups with `MigratedToEffectBinding`, including Ariel C blessing trait/master combinations and Ariel E shield trait/master combinations, from `nodes/single_attack/single_attack_skill_nodes.csv` and matching params from `nodes/single_attack/single_attack_skill_node_params.csv`.
- Converted `ariel-c-trait-3-duration-bonus` and `ariel-h-trait-3-duration-bonus` from `DurationBonus` to `StatusDurationBonus` with `status_id=blessing` and `bonus_seconds=2`.
- Verified removed ids and `MigratedToEffectBinding` no longer appear under `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes`.
- Verified retained Choice nodes include `ariel-c-trait-2-blessing-action-speed`, `ariel-e-trait-2-shield-amount-multiplier`, `ariel-e-master-2-shield-amount-multiplier`, `ariel-g-trait-2-start-shield-amount-multiplier`, and `ariel-j-trait-1-after-e-action-speed-bonus`.
- Runtime code evidence: `SkillExecutionSnapshot.ApplyNodeBackedChoiceDefinition` applies mapped plan action nodes, and `SkillExecutionSystem.AppliesToSkill` accepts `TargetSkillId` / `RuntimeTargetSkillIds`.
- Runtime code evidence: `SkillMultiEffectExecutor.ResolveStatusSpec` applies targeted `StatusDurationBonus`, and `ResolveStatusEffectShieldAmount` applies snapshot `ShieldAmountMultiplier`.
- CSV parser shape check returned `CSV_SHAPE_OK files=31`.
- Node-param reference check returned `NODE_PARAM_REFS_OK nodes=139 paramFiles=4`.
- Node count summary returned `NODE_COUNT=139`, `PARAM_COUNT=239`, and `MIGRATED_TO_EFFECT_BINDING_COUNT=0`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors and 2 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors and 2 warnings.
- `git diff --check` returned no whitespace errors; Git printed line-ending normalization warnings only.

### History

- 2026-07-09: User requested Code Builder to fix wrongly converted skill enhancement/master skill rows back to node-based behavior and verify.

## Task: 2026-07-09 Monster Runtime Skill CSV Kind Folder Consolidation

### Task title

Consolidate monster-owned runtime skill CSV files into skill-kind folders.

### Goals

- Replace monster-folder split CSV files under `Pakuri/Assets/CSVdata/runtime/monster/skills` with skill-kind-owned folders.
- Add explicit `monster_id` to consolidated runtime skill CSV rows so loader ownership no longer depends on monster-prefixed filenames.
- Preserve current loaded row ids and row counts across base, choice, effect, trigger, node, and node-param data.
- Keep runtime/editor catalog loading compatible with the new file names and folders.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime/monster/skills`.
- No MSW-MCP was used.
- Unity-MCP Play Mode validation remains user-owned; Codex attempted editor menu sync only.

### Role Owner

Code Builder

### Status

Implemented, CSV-checked, source-catalog-guid checked, and compile-verified. Unity-MCP menu sync could not complete because the open editor still executed the pre-change compiled menu code and searched for `monster_skills_projectile.csv`; `execute_code` refresh also failed with a local Mono path-length error.

### Next Actions

- After the Unity Editor reloads/recompiles, run `Pakuri/Sync CSV Runtime Catalog Assets` and `Pakuri/Validate CSV Source Data` through Unity-MCP or the editor menu.
- Author future runtime monster skill CSV rows under skill-kind folders instead of monster folders.
- Keep `monster_id` present in consolidated CSV rows whenever filenames no longer carry monster ownership.

### Evidence

- Created kind-folder base files: `base/projectile/skills_projectile.csv`, `base/line_attack/skills_line_attack.csv`, `base/area_attack/skills_area_attack.csv`, `base/single_attack/skills_single_attack.csv`, `base/buff/skills_buff.csv`, and `base/passive/skills_passive.csv`.
- Created kind-folder choice files under `choices/{kind}/skill_choices_{kind}.csv`.
- Created kind-folder effect files under `effects/{kind}/{kind}_skill_effects.csv`, trigger files under `triggers/{kind}/{kind}_skill_triger.csv`, and node files under `nodes/{kind}/{kind}_skill_nodes.csv` / `{kind}_skill_node_params.csv`.
- Removed old monster folders and files under `skills/base`, `skills/choices`, `skills/effects`, `skills/nodes`, and `skills/triggers`.
- Row-count verification returned 31 active CSV files with base 50 rows, choices 252 rows, effects 96 rows, triggers 58 rows, nodes 208 rows, and node params 363 rows.
- CSV parser shape check returned `CSV_SHAPE_AND_MONSTER_ID_OK files=31`.
- ID/reference check returned `CSV_ID_CHECK_OK skills=50 choices=252 effects=96 triggers=58 nodes=208 node_params=363`.
- `PakuriCsvRuntimeSourceCatalog.asset` now references the 31 new skill-kind CSV GUIDs, and deleted skill CSV GUID scan returned `SOURCE_CATALOG_OLD_SKILL_GUIDS_REMOVED_OK`.
- `PakuriCsvRuntimeData.cs` fallback file names now use `skills_*` and `skill_choices_*` names.
- `PakuriCsvRuntimeData.Editor.cs` suffix collection now matches `skills_*` and `skill_choices_*` without requiring monster prefixes.
- `PakuriSkillEffectPrefabCsvExporter.cs` now writes to the new `choices/{kind}/skill_choices_{kind}.csv` paths.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors and 2 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors and 2 warnings.
- `git diff --check` returned no whitespace errors; Git printed line-ending normalization warnings only.

### History

- 2026-07-09: User requested Code Builder to stop organizing runtime monster skill CSV files by monster name and consolidate them into skill-kind folders/files with explicit `monster_id`.

## Task: 2026-07-08 Ariel Node CSV Kind Folder Split

### Task title

Split Ariel runtime node CSV authority into node and node-param folders by skill kind.

### Goals

- Keep active runtime CSV authority under `Pakuri/Assets/CSVdata/runtime`.
- Make Ariel normalized node files mirror the choice/base kind split while separating node rows from node-param rows.
- Keep existing recursive suffix-based catalog collection compatible.

### Constraints

- Role Owner is Code Builder.
- Only Ariel node CSV files were split; Rin and Vega node CSV files remain in their existing per-monster aggregate files.
- No MSW-MCP was used.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Future Ariel node rows should be authored in `skills/nodes/ariel/node/`.
- Future Ariel node params should be authored in `skills/nodes/ariel/nodes_param/`.
- If Unity Editor validation is needed, sync and validate runtime CSV catalogs through Unity-MCP.

### Evidence

- `PakuriCsvRuntimeData.Editor.cs` recursively collects node CSV TextAssets by `_skill_nodes.csv` and param CSV TextAssets by `_skill_node_params.csv`.
- Ariel node files are now `ariel_buff_skill_nodes.csv`, `ariel_passive_skill_nodes.csv`, `ariel_projectile_skill_nodes.csv`, and `ariel_single_attack_skill_nodes.csv` under `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/ariel/node/`.
- Ariel param files are now the matching `ariel_*_skill_node_params.csv` files under `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/ariel/nodes_param/`.
- Old aggregate Ariel node CSV files and their `.meta` files were deleted.
- Row preservation check returned `nodes_total=193`, `params_total=330`, `duplicate_node_ids=0`, and `missing_param_node_refs=0`.
- Classification check against Ariel base skill kind returned `classification_bad=0`.
- `PakuriCsvRuntimeSourceCatalog.asset` references the 8 new Ariel split CSV GUIDs and no longer references the old aggregate node GUIDs.
- Unity auto-sync initially failed before CSV load because `ApplyStatus` was registered twice in `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`; the duplicate schema registration was removed so only the current effect-owned `ApplyStatus` handler remains.
- Follow-up source validation errors for node-owned Ariel effect ids were fixed by making `PakuriCsvRuntimeData.Validation.cs` recognize all current effect-owned operation handlers through `IsEffectOperationHandler(...)`.
- The `source_skill_id=ariel-e-shield-base` node param is now accepted as a node-owned effect source in `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` instead of being validated only as a base skill id.
- Unity-MCP menu validation returned no entries for the reported failed source-validation strings and logged `InGame skill data validation passed with 0 warning(s).`
- Runtime and editor `dotnet build` commands passed with 0 errors; existing warnings remained.

### History

- 2026-07-08: User requested that `skills/nodes/ariel` be split into `node` and `nodes_param` folders, and then split by the kind of skill being strengthened like `skills/choices/ariel`.

## Task: 2026-07-08 Ariel Effect CSV Node Authority Removal

### Task title

Remove Ariel's per-monster effect CSV after moving its effect definitions into normalized node CSVs.

### Goals

- Keep current runtime monster CSV split authority intact.
- Allow a monster to omit `{monster}_skill_effects.csv` when its effects are represented by `owner_kind=Effect` nodes.
- Preserve trigger `triggered_effect_id` validation against node-owned effect ids.
- Keep moved effect prefab paths available to the runtime asset catalog through node params.

### Constraints

- Role Owner is Code Builder.
- Active runtime authority remains under `Pakuri/Assets/CSVdata/runtime`.
- No MSW-MCP was used.

### Role Owner

Code Builder

### Status

Implemented as semantic effect-owned nodes and compile-verified.

### Next Actions

- For future per-monster effect CSV removals, first verify effect id parity against effect-owned operation nodes and trigger references.
- Keep effect-owned node params semantic-only; do not recreate deleted effect CSV rows as carrier nodes such as `EffectStatus(status_id=passive-buff)`.
- Run Unity-MCP CSV/source validation if editor-side asset import validation is needed.

### Evidence

- Deleted `Pakuri/Assets/CSVdata/runtime/monster/skills/effects/ariel/ariel_skill_effects.csv` and `.meta`.
- Removed deleted GUID `de95dfd09fa14fd5bffaf64855a35d25` from `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset`.
- Post-delete `Test-Path` returned `False` for both deleted Ariel effect files.
- Post-delete search under `Pakuri/Assets/Resources` and `Pakuri/Assets/CSVdata` for `de95dfd09fa14fd5bffaf64855a35d25|ariel_skill_effects` returned no matches.
- `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`, `PakuriCsvRuntimeData.Validation.cs`, and `PakuriCsvRuntimeData.AssetReferences.cs` now support effect-owned node definitions as data authority.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now registers semantic operation handlers `ApplyStatus`, `ApplyShield`, and `StatusModifier`, plus target, visual, condition, lifetime, and status modifier handlers.
- Ariel node verification returned handler counts: `ApplyShield=6`, `ApplyStatus=13`, `StatusModifier=14`, `EffectDamage=2`, `EffectExtendStatusDuration=1`, `ConditionStatus=10`, `EffectLifetime=33`, `EffectTarget=36`, and `EffectVisual=11`.
- Ariel node verification returned `effects=36`, `nodes=193`, `params=330`, `passive_buff_param_count=0`, `effect_status_nodes=0`, and required-param `missing_count=0`.
- Effect parity returned `effect_csv_count=36 node_owner_count=36 missing=0 extra=0`; Ariel trigger references `ariel-a-master-2-holy-exposure-on-hit` and `ariel-j-after-e-action-speed` both resolved to node-owned effects.
- `git diff --check` returned `DIFF_CHECK_OK`; Git also printed line-ending normalization warnings.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.

### History

- 2026-07-08: Ariel became the first monster whose split effect CSV was removed after full node migration; other monster effect CSV files remain present.
- 2026-07-08: After migration, Code Builder first compacted Ariel effect-owned params, then corrected the data model after the user clarified that carrier status rows were still not semantic nodes. The final Ariel node data uses `ApplyStatus`, `ApplyShield`, and `StatusModifier` operations instead of `EffectStatus(status_id=passive-buff|shield|blessing)` carrier rows.

## Task: 2026-07-07 Monster Runtime CSV Inferred Metadata Cleanup

### Task title

Remove split monster runtime CSV columns that are now inferred by file ownership or slot rules.

### Goals

- Remove monster ownership, implementation-state, skill-kind, default-learned, passive-unlock, and support-note columns where the current split CSV layout makes them redundant.
- Keep runtime loading compatible by inferring removed values from split file names, slot rules, or existing fallback behavior.
- Keep `runtime_kind` in base/trigger CSVs because runtime execution still depends on it.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime/monster`.
- `monster_modifier_skill_choice.csv` keeps `monster_id` because it is still a mixed root CSV, not a per-monster split file.
- `skills/effects/*/*_skill_effects.csv` keeps `runtime_support_state` because `MigratedToEffectBinding` is still used by validation.
- No MSW-MCP was used; Unity-MCP remains the only MCP validation path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented, CSV-checked, and compile-verified.

### Next Actions

- If Unity Editor validation is needed, run `Pakuri/Sync CSV Runtime Catalog Assets` and `Pakuri/Validate CSV Source Data` through Unity-MCP.
- Keep future split skill CSVs free of columns inferred by monster folder/file ownership and slot conventions.

### Evidence

- Removed `monster_id`, `skill_kind`, and `implementation_state` from all `skills/base/{monster}/*.csv` files.
- Removed `is_default_learned` from current projectile base files; loader now treats slot A active skills as default learned.
- Removed `is_available_without_active_requirement` and `required_active_slot` from passive base files; loader now treats slot F as available without active requirement and maps G/H/I/J to B/C/D/E.
- Removed `runtime_kind` from all `*_skills_passive.csv` files; loader now infers `SkillRuntimeKind.Passive` when a base skill row has an F-J passive slot and no `runtime_kind` column.
- Removed `monster_id`, `runtime_support_state`, and `runtime_support_notes` from all split choice CSV files.
- Removed `target_skill_id` from choice CSV files where every data row targeted the owning `skill_id`; passive choice files that target other skills kept the column.
- Removed `enabled_by_default`, node gate columns, `runtime_support_state`, and `runtime_support_notes` from normalized node CSV files.
- Removed `monster_id`, `runtime_support_state`, and `runtime_support_notes` from split trigger CSV files.
- Removed `runtime_support_notes` from split effect CSV files while retaining `runtime_support_state` for `MigratedToEffectBinding` validation.
- Removed `active_skill_name` and `passive_skill_name` from `monsters.csv`; runtime catalog build now derives them from slot A active and slot F passive display names.
- `PakuriCsvRuntimeData.MonsterDataset.cs` now infers split CSV monster ownership from `{monster}_skills_*` / `{monster}_skill_*` file names and defaults removed base-skill metadata from slot rules.
- `PakuriCsvRuntimeData.MonsterDataset.cs` still requires `runtime_kind` for active rows when the column is missing, because active split files can contain multiple execution types such as `MagazineProjectile`/`CooldownProjectile`, `AreaAttack`/`Field`, or `Buff`/`Shield`.
- `PakuriCsvRuntimeData.Build.cs` now resolves monster active/passive display names from skill rows when building `MonsterDefinition`.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now defaults missing node metadata/gate columns to enabled/no gates.
- CSV shape check across `Pakuri/Assets/CSVdata/runtime/monster/**/*.csv` returned `CSV_SHAPE_OK`.
- Removed-column scans returned `BASE_REMOVED_COLUMNS_OK`, `CHOICE_REMOVED_COLUMNS_OK`, `NODE_REMOVED_COLUMNS_OK`, and `TRIGGER_EFFECT_MONSTER_REMOVED_COLUMNS_OK`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `git diff --check` returned no whitespace errors; only Git line-ending normalization warnings were printed.

### History

- 2026-07-07: User requested Code Builder to remove strong and conditional CSV metadata cleanup candidates and replace them with code-flow defaults/inference, with a short explanation of why each removed column was deleted and how it is replaced.
- 2026-07-07: User pointed out passive skill CSV files do not need `runtime_kind=Passive`; Builder removed that column from all passive base CSVs and made F-J slots infer passive runtime kind.

## Task: 2026-07-06 Monster Skill Choice Effect Trigger CSV Character Folder Split

### Task title

Split runtime monster skill choice, effect, and trigger CSV files into per-character folders and remove default-only columns.

### Goals

- Create `ariel`, `eve`, `rin`, `sein`, and `vega` folders under `skills/choices`, `skills/effects`, and `skills/triggers`.
- Replace root choice split files with `{monster}_skill_choices_{kind}.csv` files under each monster folder.
- Replace root `monster_skill_effects.csv` with `{monster}_skill_effects.csv` files under each monster folder.
- Replace root `monster_skill_triger.csv` with `{monster}_skill_triger.csv` files under each monster folder.
- Keep runtime/editor catalog loading compatible with split files.
- Remove columns that are only parser defaults in each split file.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime`.
- Legacy single-file catalog fields remain only as fallback when split arrays are empty.
- No MSW-MCP was used; Unity-MCP remains the only MCP validation path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented, CSV-checked, and compile-verified.

### Next Actions

- Author future choice rows in `skills/choices/{monster}/{monster}_skill_choices_{kind}.csv`.
- Author future effect rows in `skills/effects/{monster}/{monster}_skill_effects.csv`.
- Author future trigger rows in `skills/triggers/{monster}/{monster}_skill_triger.csv`.
- If Unity Editor validation is needed, run `Pakuri/Sync CSV Runtime Catalog Assets` and `Pakuri/Validate CSV Source Data` through Unity-MCP.

### Evidence

- Created monster folders under `Pakuri/Assets/CSVdata/runtime/monster/skills/choices`, `effects`, and `triggers`.
- Deleted the six root `monster_skill_choices_*.csv` files and replaced them with 23 monster-owned choice files.
- Deleted root `monster_skill_effects.csv` and replaced it with 5 monster-owned effect files.
- Deleted root `monster_skill_triger.csv` and replaced it with 5 monster-owned trigger files.
- Split generation created 33 CSV files with 442 total data rows and dropped 586 default-only columns.
- Choice verification returned `choice_files=23`, `choice_rows=252`, `choice_dupes=0`, `choice_missing_skills=0`, and `choice_by_monster=ariel:50,eve:50,rin:50,sein:51,vega:51`.
- Effect verification returned `effect_files=5`, `effect_rows=132`, `effect_dupes=0`, `effect_missing_skills=0`, and `effect_by_monster=ariel:36,eve:34,rin:20,sein:19,vega:23`.
- Trigger verification returned `trigger_files=5`, `trigger_rows=58`, `trigger_dupes=0`, `trigger_missing_source_skills=0`, and `trigger_by_monster=ariel:6,eve:3,rin:17,sein:17,vega:15`.
- Root legacy checks returned `root_legacy_choice_files=0`, `root_legacy_effect_files=0`, and `root_legacy_trigger_files=0`.
- `PakuriCsvRuntimeSourceCatalog.cs` now exposes split arrays for monster skill choices, effects, and triggers.
- `PakuriCsvRuntimeData.Editor.cs` now recursively collects split choice/effect/trigger CSV TextAssets by suffix.
- `PakuriCsvRuntimeData.Loader.cs` now loads choice/effect/trigger split arrays first, falling back to legacy single TextAsset fields only when arrays are empty.
- `PakuriCsvRuntimeData.MonsterDataset.cs` and `PakuriCsvRuntimeData.StatusPayload.cs` now allow default-backed effect/trigger/status columns to be missing from split CSV files.
- `PakuriCsvRuntimeSourceCatalog.asset` now clears legacy choice/effect/trigger aggregate references and stores split CSV TextAsset arrays.
- Search in `PakuriCsvRuntimeSourceCatalog.asset` for the old aggregate choice/effect/trigger GUIDs returned no matches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `git diff --check` returned no whitespace errors; only Git line-ending normalization warnings were printed.

### History

- 2026-07-06: User requested Code Builder to split `skills/choices`, `skills/effects`, and `skills/triggers/monster_skill_triger.csv` by monster type and clean garbage/default columns like the previous base/node split.

## Task: 2026-07-06 Monster Skill Base And Node CSV Character Folder Split

### Task title

Split runtime monster skill base and node CSV files into per-character folders and make runtime loading use those split assets.

### Goals

- Create `ariel`, `eve`, `rin`, `sein`, and `vega` folders under `Pakuri/Assets/CSVdata/runtime/monster/skills/base`.
- Rename base CSV files from `monster_skills_*` to `{monster}_skills_*` inside the owning monster folder.
- Remove columns that are empty for every data row in each split base CSV.
- Create the same five monster folders under `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes` and move current node CSV files into their owner folders.
- Keep in-game runtime loading compatible with the split folder structure.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime`.
- Legacy single-file catalog fields remain only as fallback when split arrays are empty.
- No MSW-MCP was used; Unity-MCP remains the only MCP validation path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented, CSV-checked, and compile-verified.

### Next Actions

- Author future base skill rows in `skills/base/{monster}/{monster}_skills_{kind}.csv`.
- Author future node rows in `skills/nodes/{monster}/{monster}_skill_nodes.csv` and params in `{monster}_skill_node_params.csv`.
- If Unity Editor validation is needed, run `Pakuri/Sync CSV Runtime Catalog Assets` and `Pakuri/Validate CSV Source Data` through Unity-MCP.

### Evidence

- `Pakuri/Assets/CSVdata/runtime/monster/skills/base` now has `ariel`, `eve`, `rin`, `sein`, and `vega` folders.
- The six root `monster_skills_*.csv` base files were replaced by 23 monster-owned files named `{monster}_skills_projectile.csv`, `{monster}_skills_line_attack.csv`, `{monster}_skills_area_attack.csv`, `{monster}_skills_single_attack.csv`, `{monster}_skills_buff.csv`, and `{monster}_skills_passive.csv` as applicable.
- Base CSV verification returned `base_file_count=23`, `base_row_count=50`, `base_by_monster=ariel:10,eve:10,rin:10,sein:10,vega:10`, `duplicate_skill_ids=0`, `blank_columns_after_split=0`, and `root_legacy_base_files=0`.
- 2026-07-06 follow-up cleanup removed 88 optional base CSV columns whose values were only parser defaults, including numeric `0`, bool `false`, blank strings, `status_effect_label=없음`, default `DamageAttribute.Physical`, default `required_active_slot=A`, and default multiplier `1` columns.
- Follow-up sample checks showed `Pakuri/Assets/CSVdata/runtime/monster/skills/base/ariel/ariel_skills_buff.csv` no longer contains `status_max_stacks`, `status_stack_amount`, `status_action_speed_bonus`, or `status_attack_power_bonus`.
- Follow-up sample checks showed `Pakuri/Assets/CSVdata/runtime/monster/skills/base/vega/vega_skills_line_attack.csv` no longer contains `active_duration_seconds`, `shot_interval_seconds`, or `spell_power_coefficient`.
- Follow-up base CSV verification returned `row_count=50`, `duplicate_skill_ids=0`, `by_monster=ariel:10,eve:10,rin:10,sein:10,vega:10`, and `default_only_optional_columns_remaining=0`.
- 2026-07-06 bool cleanup inspected `InGameSkillDefinitionMapper.MapDamage`, `BuffSkillData` mapping, `ShieldSkillData` mapping, and `SupportSkillExecutors`; `critical_allowed` is used through `MapDamage` for damage specs, but Shield mapping does not call `MapDamage`, and current Buff executor does not apply attached damage.
- Removed non-applicable `critical_allowed` from `ariel/ariel_skills_buff.csv`, `rin/rin_skills_buff.csv`, and `vega/vega_skills_buff.csv`.
- Bool follow-up verification returned `support_critical_allowed_remaining=0`, `row_count=50`, `duplicate_skill_ids=0`, and `by_monster=ariel:10,eve:10,rin:10,sein:10,vega:10`.
- Remaining bool `true` columns are code-referenced: `is_default_learned` is used by `RunSession` and validation, `is_available_without_active_requirement` is used by `RunSession`/UI/validation, `require_execute_threshold_to_cast` is used by single-attack execution, and remaining `critical_allowed` appears only on damage runtime kinds.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes` now has `ariel`, `eve`, `rin`, `sein`, and `vega` folders.
- Current node data was moved to `nodes/ariel/ariel_skill_nodes.csv`, `nodes/ariel/ariel_skill_node_params.csv`, `nodes/rin/rin_skill_nodes.csv`, `nodes/rin/rin_skill_node_params.csv`, `nodes/vega/vega_skill_nodes.csv`, and `nodes/vega/vega_skill_node_params.csv`.
- Node CSV verification returned `node_file_count=3`, `node_row_count=55`, `param_file_count=3`, `param_row_count=77`, `missing_param_node_refs=0`, and `root_legacy_node_files=0`.
- `PakuriCsvRuntimeSourceCatalog.cs` now exposes split TextAsset arrays for each base skill kind.
- `PakuriCsvRuntimeData.Editor.cs` now recursively collects split base and node CSV TextAssets by suffix under the base/nodes folders.
- `PakuriCsvRuntimeData.Loader.cs` now loads base skill split arrays first and falls back to legacy single TextAsset fields only when arrays are empty.
- `PakuriCsvRuntimeSourceCatalog.asset` now clears legacy base/node aggregate references and stores split CSV TextAsset arrays.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `git diff --check` returned no whitespace errors; only Git line-ending normalization warnings were printed.

### History

- 2026-07-06: User requested Code Builder to split `skills/base` and `skills/nodes` runtime CSVs into five monster folders, rename base files away from `monster_skills_*`, remove all-null columns, and keep in-game operation intact.
- 2026-07-06: User pointed out that all-null cleanup still left default-value garbage columns such as `0` status stack fields and unused line-attack timing/coefficient fields; Code Builder removed default-only optional columns from the split base CSV files.
- 2026-07-06: User pointed out `critical_allowed=true` on Ariel's Shield row; Code Builder verified bool references and removed non-applicable support-skill `critical_allowed` columns.

## Task: 2026-07-06 Monster Skill Node CSV Character Split

### Task title

Split monster skill node and node-param runtime CSVs by character while keeping runtime loading compatible.

### Goals

- Replace monolithic monster skill node CSV sources with character-prefixed split files.
- Make runtime/editor catalog loading use split node CSV arrays when present.
- Keep legacy single-file node fields as fallback only when no split files exist.
- Preserve existing node and node-param row content and in-game runtime behavior.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime`.
- No MSW-MCP is used; Unity-MCP remains the only MCP validation path if editor validation is needed.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, CSV-checked, and compile-verified.

### Next Actions

- Author future character-specific monster skill node rows in `skills/nodes/monster_skill_nodes_{character}.csv`.
- Author future character-specific node params in `skills/nodes/monster_skill_node_params_{character}.csv`.
- If Unity Editor validation is needed, run `Pakuri/Sync CSV Runtime Catalog Assets` and `Pakuri/Validate CSV Source Data` through Unity-MCP.

### Evidence

- Deleted aggregate `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/monster_skill_nodes.csv` and `monster_skill_node_params.csv` plus their `.meta` files.
- Added `monster_skill_nodes_ariel.csv` with 40 data rows, `monster_skill_nodes_rin.csv` with 11 data rows, and `monster_skill_nodes_vega.csv` with 4 data rows.
- Added `monster_skill_node_params_ariel.csv` with 44 data rows, `monster_skill_node_params_rin.csv` with 22 data rows, and `monster_skill_node_params_vega.csv` with 11 data rows.
- Split verification returned `node_total=55`, `node_dupes=0`, `param_total=77`, and `missing_param_nodes=0`.
- Row-width checks returned empty `bad=` for all six split node/node-param CSV files.
- `PakuriCsvRuntimeSourceCatalog.cs` now exposes `MonsterSkillNodeFiles` and `MonsterSkillNodeParamFiles`.
- `PakuriCsvRuntimeData.Editor.cs` now auto-collects `monster_skill_nodes_*.csv` and `monster_skill_node_params_*.csv` from `skills/nodes`, and uses legacy single files only when no split files exist.
- `PakuriCsvRuntimeData.Loader.cs` now loads split node/node-param TextAsset arrays first, falling back to the legacy single TextAsset fields only when the arrays are empty.
- `PakuriCsvRuntimeSourceCatalog.asset` now clears legacy `MonsterSkillNodes` / `MonsterSkillNodeParams` references and stores split CSV TextAsset arrays.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- Initial parallel runtime build hit only an `obj/Debug/Assembly-CSharp.dll` file lock; rerunning `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` alone passed with 0 errors.
- `git diff --check` returned no whitespace errors; only Git line-ending normalization warnings were printed.

### History

- 2026-07-06: User requested Code Builder to split `monster_skill_nodes.csv` and `monster_skill_node_params.csv` by character and keep in-game runtime operation intact.

## Task: 2026-07-05 Monster Skill Choice CSV Split And Skill Folder Reorg

### Task title

Split runtime monster skill choice rows by owner skill `runtime_kind` and organize monster skill runtime CSVs into purpose folders.

### Goals

- Replace the monolithic `monster_skill_choices.csv` runtime source with owner-runtime-kind split choice CSV files.
- Keep choice split ownership based on the row's owner `skill_id`, matching runtime choice lookup, not `target_skill_id`.
- Move active monster skill body/effect/trigger/node CSV files under purpose folders below `Pakuri/Assets/CSVdata/runtime/monster/skills`.
- Keep split choice columns narrow by omitting columns with no non-empty values in that split.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime`.
- Preserve moved CSV Unity GUIDs by moving existing `.meta` files with their CSV files.
- MSW-MCP is not used; Unity-MCP remains the only MCP path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Author future monster skill body rows under `skills/base`.
- Author future monster skill choice rows under `skills/choices` in the file matching the owner skill's `runtime_kind`.
- If Unity Editor validation is needed, run the existing Pakuri CSV runtime sync/validate menu through Unity-MCP.

### Evidence

- Created `skills/base`, `skills/choices`, `skills/effects`, `skills/triggers`, and `skills/nodes` folders under `Pakuri/Assets/CSVdata/runtime/monster/skills`.
- Moved `monster_skills_projectile.csv`, `monster_skills_line_attack.csv`, `monster_skills_area_attack.csv`, `monster_skills_single_attack.csv`, `monster_skills_buff.csv`, and `monster_skills_passive.csv` into `skills/base` with their `.meta` files.
- Moved `monster_skill_effects.csv` into `skills/effects`, `monster_skill_triger.csv` into `skills/triggers`, and `monster_skill_nodes.csv` / `monster_skill_node_params.csv` into `skills/nodes` with their `.meta` files.
- Deleted `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_choices.csv` and its `.meta`; verification returned `OLD_CHOICE_EXISTS=False` and `ROOT_CSV_COUNT=0`.
- Added `monster_skill_choices_projectile.csv` with 49 rows / 39 columns.
- Added `monster_skill_choices_line_attack.csv` with 21 rows / 28 columns.
- Added `monster_skill_choices_area_attack.csv` with 21 rows / 31 columns.
- Added `monster_skill_choices_single_attack.csv` with 63 rows / 43 columns.
- Added `monster_skill_choices_buff.csv` with 21 rows / 23 columns.
- Added `monster_skill_choices_passive.csv` with 77 rows / 26 columns.
- PowerShell verification returned `TOTAL_CHOICE_ROWS=252`, `DUPLICATE_CHOICE_IDS=0`, empty `BAD_SKILL_SPLITS`, and empty `BAD_CHOICE_SPLITS`.
- TextFieldParser width checks returned `bad=` empty for all moved skill body, split choice, effect, trigger, node, and node-param CSV files.
- PowerShell search under `Pakuri/Assets` returned no remaining `monster_skill_choices.csv`, `MonsterSkillChoicesFileName`, `MonsterSkillChoices:`, old choice GUID `5b5f094e9fbfaef4593518ad6d855917`, `monster_skills.csv`, `MonsterSkillsFileName`, or `MonsterSkills:` matches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `git diff --check` returned no whitespace errors; only Git line-ending normalization warnings were printed.

### History

- 2026-07-05: User approved splitting `monster_skill_choices.csv` using the original skill's `runtime_kind` and organizing the files into folders.

## Task: 2026-07-05 Monster Skills Runtime Kind CSV Split

### Task title

Split the runtime monster skill body table by `runtime_kind` ownership and remove the monolithic `monster_skills.csv` runtime source.

### Goals

- Replace `monster_skills.csv` with runtime-kind split CSV files under `Pakuri/Assets/CSVdata/runtime/monster/skills/`.
- Merge `CooldownProjectile` into the projectile split file, `Field` into the area-attack split file, and `Shield` into the buff split file.
- Keep split CSV columns narrow so each file contains only columns used by its owned runtime kinds.
- Make runtime loading, editor sync, and validation use the split files instead of `monster_skills.csv`.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime`.
- MSW-MCP is not used; Unity-MCP remains the only MCP path if Unity Editor validation is needed.
- Historical board references to old `monster_skills.csv` rows remain historical unless a current task block says otherwise.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Author future monster skill body rows in the split file that owns the row's `runtime_kind`.
- Keep `CooldownProjectile` in `monster_skills_projectile.csv`, `Field` in `monster_skills_area_attack.csv`, and `Shield` in `monster_skills_buff.csv`.
- If Unity Editor validation is needed, run the existing Pakuri CSV runtime sync/validate menu through Unity-MCP.

### Evidence

- Added `monster_skills_projectile.csv` with 7 rows / 35 columns and allowed kinds `MagazineProjectile|CooldownProjectile`.
- Added `monster_skills_line_attack.csv` with 3 rows / 25 columns and allowed kind `LineAttack`.
- Added `monster_skills_area_attack.csv` with 3 rows / 28 columns and allowed kinds `AreaAttack|Field`.
- Added `monster_skills_single_attack.csv` with 9 rows / 39 columns and allowed kind `SingleAttack`.
- Added `monster_skills_buff.csv` with 3 rows / 26 columns and allowed kinds `Buff|Shield`.
- Added `monster_skills_passive.csv` with 25 rows / 11 columns and allowed kind `Passive`.
- PowerShell CSV verification returned `TOTAL_ROWS=50`, `DUPLICATE_IDS=0`, and empty `BadKinds` for every split file.
- Comparison against the deleted monolithic `monster_skills.csv` returned `DROPPED_NON_DEFAULT_VALUES=0` after allowing blank/default values and the explicit `없음` status label.
- Deleted `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skills.csv` and `monster_skills.csv.meta`; `Test-Path` returned `False` for both paths.
- `PakuriCsvRuntimeData.Loader.cs` now loads the six split TextAssets and rejects rows whose `runtime_kind` does not belong to that split file.
- `PakuriCsvRuntimeData.MonsterDataset.cs` and `PakuriCsvRuntimeData.StatusPayload.cs` now allow missing non-owned skill/status columns when parsing split skill rows.
- `PakuriCsvRuntimeData.cs`, `PakuriCsvRuntimeData.Editor.cs`, `PakuriCsvRuntimeSourceCatalog.cs`, and `PakuriCsvRuntimeSourceCatalog.asset` now use the six split source names instead of `MonsterSkills`.
- `Select-String` under `Pakuri/Assets/Scripts2`, `Pakuri/Assets/Resources`, and `Pakuri/Assets/CSVdata` for `MonsterSkillsFileName`, `MonsterSkills:`, `monster_skills.csv`, and the old monolithic GUID returned no matches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `git diff --check` returned no whitespace errors; only Git line-ending normalization warnings were printed.

### History

- 2026-07-05: User asked Code Builder to merge `cooldown_projectile` into `MagazineProjectile`, include `Field` in `AreaAttack`, include `Shield` in `Buff`, and implement the split.

## Task: 2026-07-05 Monster Skill Base CSV Removal

### Task title

Remove unused monster skill base CSV tables from the active runtime skill data path.

### Goals

- Delete `monster_skill_base.csv` and `monster_skill_choice_base.csv` plus their Unity `.meta` files.
- Remove runtime/editor references to `monster_skill_base.csv`.
- Make choice source validation and build logic use `monster_skill_choices.csv` only.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/runtime`.
- MSW-MCP is not used; Unity-MCP remains the only MCP path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Keep future monster choice rows in `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_choices.csv`.
- If Unity Editor validation is needed, run the existing Pakuri CSV runtime sync/validate menu through Unity-MCP.

### Evidence

- Deleted `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_base.csv` and `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_base.csv.meta`.
- Deleted `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_choice_base.csv` and `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_choice_base.csv.meta`.
- `PakuriCsvRuntimeSourceCatalog.cs` and `PakuriCsvRuntimeSourceCatalog.asset` no longer contain `MonsterSkillBase` or `MonsterSkillChoiceBase`.
- `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.Loader.cs`, `PakuriCsvRuntimeData.SourceModel.cs`, `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`, and `PakuriCsvRuntimeData.Validation.cs` no longer use `SkillBaseRows` or `SkillChoiceBaseRows`.
- `Test-Path` for the four deleted CSV/meta paths returned `False`, `False`, `False`, and `False`.
- `Select-String` under `Pakuri/Assets/Scripts2/InGame/Data/Runtime` for removed base-table symbols and filenames returned no matches.
- `Select-String` on `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` for removed base-table symbols and filenames returned no matches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.

### History

- 2026-07-05: User asked Code Builder to delete `monster_skill_base.csv` and `monster_skill_choice_base.csv`, and to stop referencing `monster_skill_choice_base.csv` by unifying choice references onto `monster_skill_choices.csv`.

## Task: 2026-06-19 Enemy Skill Node Runtime Implementation 1-7

### Task title

Implement the data side of enemy skill node runtime handoff steps 1-7.

### Goals

- Add enemy skill node and node-param runtime CSV files.
- Remove `enemy_scope` from `EnemySkillData.csv` and keep `radius` as the enemy skill range authority.
- Add Stage2 active skill rows and bind Stage2 units to those skills.
- Keep Stage1 skills on the same node data path with old executor fallback still present.

### Constraints

- Role Owner is Code Builder.
- User deferred handoff step 8; old direct execution fallback remains.
- MSW-MCP is not used; Unity-MCP is the only MCP validation path.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, CSV-shape checked, and Unity-MCP CSV sync/validate checked. 2026-06-19 follow-up Code Builder pass added runtime validation coverage for enemy node `action_op` and `target_selector` values.

### Next Actions

- User verifies Stage2 skill behavior in Play Mode before step 8 removes old direct paths.
- Keep future enemy skill tuning in `EnemySkillData.csv` and behavior composition in `EnemySkillNodes.csv` / `EnemySkillNodeParams.csv`.

### Evidence

- Added `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillNodes.csv` and `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillNodeParams.csv`.
- `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillData.csv` no longer contains `enemy_scope` or `range`.
- Stage2 skill rows in `EnemySkillData.csv` use requested radii: FireDragonSlash 2, ChainLightning 7, FrostPressure 2, DarkStab 1.4, HolyDragonHeal 5, HolySpearThrow 14, OpeningCharge 40, Intimidation 40.
- `Pakuri/Assets/CSVdata/runtime/enemy/stage_two_enemies.csv` binds Stage2 enemies to those Stage2 active skill ids.
- CSV row-width check returned `bad=` empty for `EnemySkillData.csv`, `stage_two_enemies.csv`, `EnemySkillNodes.csv`, and `EnemySkillNodeParams.csv`.
- `PakuriCsvRuntimeData.Validation.cs` now rejects unsupported `EnemySkillNodes.csv` `action_op` values and unsupported `target_selector` values.
- PowerShell validation of `EnemySkillNodes.csv`, excluding the second schema row, returned `badOps=` and `badSelectors=` empty.
- `EnemySkillNodeParams.csv` contains the requested Stage2 values including `ChainLightning delay=0.5`, `ChainLightning chain_radius=7`, `FrostPressure action_speed_bonus=-0.2`, and `Intimidation multiplier=0.7`.
- Runtime/editor builds passed with 0 errors; only existing `MSB3277` assembly-version warnings remained.
- Unity-MCP sync logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/runtime' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP validate logged runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- 2026-06-19 follow-up Unity-MCP validation could not run because no Unity Editor instance was found by the MCP bridge.

### History

- 2026-06-19: User asked Code Builder to implement handoff steps 1-7, create the two enemy node CSV files, make Lightning Scout chain again after 0.5 seconds on another target, and make Arsen reduce target outgoing damage to x0.7.

## Task: 2026-06-19 EnemySkillData Range Column Removal

### Task title

Remove the unused `range` column from enemy skill runtime CSV data.

### Goals

- Keep enemy skill distance data on the currently used `radius` column.
- Remove the unused `range` column from `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillData.csv`.
- Preserve runtime CSV sync and validation.

### Constraints

- Role Owner is Code Builder.
- No enemy combat code was changed; inspected runtime code already ignored `range`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- Keep future Stage1 enemy skill distance authoring on `radius` unless runtime code adds a separate range contract.

### Evidence

- `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillData.csv` no longer contains the `range` header/type column.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\runtime\enemy\EnemySkillData.csv` showed `hasRange=False`, `headerCount=33`, and data rows loaded.
- TextFieldParser row-width check returned `expected=33` and `bad=` empty.
- Search under `Pakuri/Assets/Scripts2/InGame` found no `ReadFloat("range")`, `ReadOptionalFloat(record, "range")`, `ActiveSkillRange`, or `BasicSkillRange` references.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged sync from `Assets/CSVdata/runtime`.
- Unity-MCP `Pakuri/Validate CSV Source Data` logged runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity-MCP warning/error console read returned 0 entries.

### History

- 2026-06-19: User asked Code Builder to delete the unused `range` column after code inspection showed Stage1 enemy attack distance uses `radius` or attack-type fallback.

## Task: 2026-06-19 Enemy Skill Node Data Handoff

### Task title

Record the data-facing handoff for future enemy active skill node authoring.

### Goals

- Keep current `EnemySkillData.csv` as the enemy skill body source.
- Plan future enemy behavior rows as node and node-param data instead of extending hardcoded Stage1 skill switches.
- Include Stage1 enemy skills in the migration so Stage1 and Stage2 do not diverge into separate execution/data models.

### Constraints

- Role Owner is Designer.
- This task produced a handoff only; no CSV file, column, row, parser, prefab, or runtime catalog asset was changed.
- Proposed enemy node CSV files do not exist yet and must be created by Code Builder only after the implementation route is chosen.
- Do not infer unsupported dual-attribute damage or tower status support without inspecting/adding runtime support.

### Role Owner

Designer

### Status

Handoff created; implementation not started.

### Next Actions

- Code Builder decides exact enemy node CSV schema and adds parser/runtime support.
- Candidate files from the handoff are `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillNodes.csv` and `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillNodeParams.csv`.
- Code Builder keeps old enemy execution fallback until Stage1 node parity is verified.
- Code Builder removes `enemy_scope` from `EnemySkillData.csv`, adds Stage2 active skill body rows there, and treats each row's `radius` as the source of truth for enemy skill range.

### Evidence

- Created `Pakuri/reference/Report/2026-06-19-enemy-skill-node-runtime-handoff.md`.
- `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillData.csv` exists and currently holds enemy skill body/tuning fields.
- Updated `Pakuri/reference/Report/2026-06-19-enemy-skill-node-runtime-handoff.md` to require `EnemySkillData.csv` Stage2 rows, no `enemy_scope` gate, and requested Stage2 radius values: Fire Dragon Soldier 2, Lightning Scout 7, Ice Guard 2, Dark Assassin 1.4, Holy Priest 5, Ethan 14, Drake 40, Arsen 40.
- The proposed `EnemySkillNodes.csv` and `EnemySkillNodeParams.csv` files are handoff candidates only; they were not created in this task.
- `Pakuri/Assets/Scripts2/InGame/Data/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` contains normalized monster skill authoring support, but enemy skills are not currently compiled through that node path.
- `Pakuri/Assets/Scripts2/InGame/Enemy/EnemyCombatSystem.cs` currently executes enemy skills through resolved skill data and a direct skill-kind switch, not through enemy node CSV rows.

### History

- 2026-06-19: User requested a Code Builder handoff that judges Stage1 prefab skill applicability together with Stage2 enemy skill implementation planning.
- 2026-06-19: User revised the handoff so Stage2 enemy skills are managed through `EnemySkillData.csv`, enemy skill range comes from `radius`, `enemy_scope` is removed, and combat-start skills use high `radius` values for immediate execution.

## Task: 2026-06-19 Generic Node-Backed Choice Routing

### Task title

Record the data-facing behavior of replacing Ariel-only choice routing with generic node-backed choice routing.

### Goals

- Make normalized choice nodes apply generically for any monster instead of only Ariel.
- Keep legacy wide choice mapping as fallback for choices without normalized plan nodes.
- Preserve current runtime CSV schema and validation behavior.

### Constraints

- Role Owner is Code Builder.
- No CSV file, column, or row value was changed in this pass.
- Active CSV authority remains `Pakuri/Assets/CSVdata/runtime`.
- Play Mode gameplay parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, and Unity-MCP validated.

### Next Actions

- Future migrated choices for Eve, Vega, Sein, Rin, and Ariel can use normalized choice nodes without needing monster-specific runtime gates.
- Keep wide choice columns as compatibility fallback until old rows are fully migrated.

### Evidence

- `monster_skill_nodes.csv` currently contains normalized choice nodes for Ariel, Rin, and Vega, so routing had to become monster-agnostic rather than Ariel-only.
- `SkillExecutionSnapshot.cs` now routes any `SkillChoiceDefinition` with non-empty `NormalizedPlanNodes` through the node-backed choice path.
- `InGameSkillDefinitionMapper.cs` now skips old `ApplyNormalizedChoiceNodes(...)` folding when a choice already has normalized plan nodes, preventing node-backed choices from double-applying through legacy wide specs.
- `SkillExecutionSystem.cs` now reads `CountStatusDamageMultiplier` nodes without an Ariel-only gate.
- Search under `Pakuri/Assets/Scripts2/InGame/Skills` found no remaining `IsArielChoice` or `ApplyArielChoiceDefinition`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/InGame/Validate Skill Data` logged `InGame skill data validation passed with 0 warning(s)` after a clean console run, with 0 warning/error entries.

### History

- 2026-06-19: User requested Code Builder to fix Code Reviewer findings and make the structure better for future skill objectification and additions.

## Task: 2026-06-19 Shared Plan Projection For Existing Effect And Trigger CSV Rows

### Task title

Record the data-runtime projection path that lets existing effect/trigger CSV rows enter `SkillExecutionPlan`.

### Goals

- Keep current CSV schemas unchanged while making `monster_skill_effects.csv` and `monster_skill_triger.csv` rows visible as plan node payloads.
- Avoid adding Ariel-only data behavior; the projection applies to source-owned skill triggers for all monsters.
- Preserve existing runtime catalog sync and InGame skill validation.

### Constraints

- Role Owner is Code Builder.
- Active CSV authority remains `Pakuri/Assets/CSVdata/runtime`.
- No new CSV file, CSV column, or runtime catalog schema was added in this pass.
- Existing trigger/effect row execution remains compatible through fallback paths.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- Future CSV migration can author trigger/effect action semantics as normalized plan nodes once handler coverage is complete.
- Keep existing CSV effect/trigger rows valid until Play Mode parity proves the handler replacement path.

### Evidence

- `SkillData.cs` now includes `SkillTriggerDefinition[] SkillTriggers`.
- `InGameSkillDefinitionMapper.cs` filters monster-level `SkillTriggerDefinition` rows by `SourceSkillId` and attaches them to each active/passive skill data object.
- `SkillExecutionPlan.cs` converts `SkillData.MultiEffects` and `SkillData.SkillTriggers` into `SkillExecutionPlanNode.FromEffect(...)` and `SkillExecutionPlanNode.FromTrigger(...)`.
- `SkillPlanActionDispatcher.cs` resolves plan-projected effects/triggers with legacy fallback.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP CSV sync logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/runtime' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP validation logged runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity-MCP InGame skill validation logged `InGame skill data validation passed with 0 warning(s)`, and warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested the target structure be made reusable for Eve, Vega, Sein, Rin, and Ariel.
- 2026-06-19: Code Builder added plan projection for existing effect/trigger CSV runtime objects without changing CSV shape.

## Task: 2026-06-19 Ariel Plan-Action CSV Migration

### Task title

Record Ariel runtime CSV movement from old choice-wide behavior fields to normalized plan action nodes.

### Goals

- Keep Ariel choice metadata in `monster_skill_choices.csv`, but remove active Ariel modifier payload reliance on old behavior columns.
- Store Ariel D trait4/master1 remaining modifiers in `monster_skill_nodes.csv` and `monster_skill_node_params.csv`.
- Preserve Ariel A master2 status application as explicit trigger/effect CSV rows.

### Constraints

- Role Owner is Code Builder / Code Reviewer.
- Active CSV authority remains `Pakuri/Assets/CSVdata/runtime`.
- `monster_skill_triger.csv` and `monster_skill_effects.csv` remain explicit runtime object tables in this pass.

### Role Owner

Code Builder / Code Reviewer

### Status

Implemented and reviewed for Ariel-first migration scope.

### Next Actions

- Keep future Ariel modifier additions on `monster_skill_nodes.csv` plus `monster_skill_node_params.csv`.
- Do not add new Ariel behavior columns to `monster_skill_choices.csv` without a recorded exception.

### Evidence

- `monster_skill_choices.csv` Ariel old behavior-field scan returned `arielWideNonDefault=0`.
- `monster_skill_nodes.csv` has `ariel-d-trait-4-hit-target-count-bonus` / `HitTargetCountBonus` and `ariel-d-master-1-status-critical-damage-taken` / `StatusCriticalDamageTakenBonus`.
- `monster_skill_node_params.csv` stores `bonus=1` for D trait4 hit target count and `bonus=0.25` for D master1 critical damage taken.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` registers `HitTargetCountBonus` and validates overlap for `hit_target_count_bonus` and `status_critical_damage_taken_bonus`.
- Unity-MCP CSV sync and InGame skill validation passed with `InGame skill data validation passed with 0 warning(s)` and 0 warning/error console entries.

### History

- 2026-06-19: User requested Ariel-first target migration after prior Reviewer found old wide choice residues.

## Task: 2026-06-19 Ariel A Master2 Runtime CSV Binding Fix

### Task title

Record Ariel A master2 CSV migration from choice-wide status fields to trigger/effect/node rows.

### Goals

- Keep active Ariel A master2 behavior out of old `monster_skill_choices.csv` status-wide fields.
- Author the status application through `monster_skill_triger.csv` and `monster_skill_effects.csv`.
- Author the +15% Holy damage taken modifier through `monster_skill_nodes.csv` and `monster_skill_node_params.csv`.
- Add CSV validation coverage for migrated effect rows that still have executable gates.

### Constraints

- Role Owner is Code Builder.
- Active CSV authority remains `Pakuri/Assets/CSVdata/runtime`.
- Current runtime trigger enum uses `OnOutgoingDamage` for hit-success trigger binding; no unsupported `OnHit` enum value was authored.

### Role Owner

Code Builder

### Status

Implemented, compiled, and Unity-MCP validated.

### Next Actions

- Keep future migrated effect rows free of executable `requires_active_choice_id` / `requires_passive_skill_id` gates when `runtime_support_state=MigratedToEffectBinding`.
- Keep future on-hit status applications on trigger/effect rows before adding new wide choice columns.

### Evidence

- `monster_skill_choices.csv` now has `ariel-a-master-2` with old status-wide payload fields blank and `runtime_support_state=RuntimeImplemented`.
- `monster_skill_triger.csv` now has `ariel-a-master2-holy-exposure-on-hit` with `trigger_event=OnOutgoingDamage`, `target_selection=EventTarget`, and `trigger_action=Effect`.
- `monster_skill_effects.csv` now has `ariel-a-master-2-holy-exposure-on-hit` with `status_effect_id=holy-exposure`, `status_chance=1`, and `status_stack_amount=1`.
- `monster_skill_nodes.csv` and `monster_skill_node_params.csv` now carry the `StatusElementDamageTakenBonus` node and `bonus=0.15` param for `ariel-a-master-2`.
- `PakuriCsvRuntimeData.Validation.cs` now errors when `MigratedToEffectBinding` effect rows still carry executable choice/passive gates.
- CSV property-count check returned no bad rows for `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, `monster_skill_nodes.csv`, and `monster_skill_node_params.csv`.
- Unity-MCP sync/validate logs showed sync from `Assets/CSVdata/runtime`, runtime catalog load, and `InGame skill data validation passed with 0 warning(s)`.

### History

- 2026-06-19: Code Reviewer found `ariel-a-master-2` still depended on old choice-wide status columns and Ariel E migrated shield variants could still run through choice gates.
- 2026-06-19: Code Builder moved the behavior to trigger/effect/node rows, cleared migrated shield gates, and added validation coverage.

## Task: 2026-06-19 Ariel Passive Node Decomposition Follow-up

### Task title

Record Ariel passive modifier CSV decomposition on the normalized node path.

### Goals

- Keep Ariel passive numeric add-ons in `monster_skill_nodes.csv` and `monster_skill_node_params.csv` instead of duplicate choice-gated effect rows.
- Preserve runtime CSV validation and catalog sync after adding new status modifier handler ids.

### Constraints

- Role Owner is Code Builder.
- Active CSV authority remains under `Pakuri/Assets/CSVdata/runtime`.
- No new specialized effect binding CSV tables were added.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- Reuse the status modifier normalized handlers for future passive aura numeric add-ons.
- Keep conceptually new conditional passive effects in `monster_skill_effects.csv` when they add a new condition/effect object rather than modifying an existing base effect.

### Evidence

- Added normalized handler schemas for `StatusDamageBonusRate`, `StatusShieldReceivedBonus`, `StatusCriticalChanceBonus`, `StatusDamageTakenBonus`, and `StatusFlatElementResistReduction` in `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`.
- `monster_skill_nodes.csv` now carries the Ariel F/G/H/I/J passive status modifier nodes; the old generic passive damage node ids for F/H/I/J are absent (`oldGenericPassiveDamageNodes=0`).
- `monster_skill_effects.csv` marks G trait1, G trait2, I trait1, and J trait1 duplicate rows as `MigratedToEffectBinding`.
- `monster_skill_triger.csv` no longer contains `ariel-j-after-e-action-speed-trait1-trigger`.
- CSV shape check returned no bad rows for `monster_skill_choices.csv`, `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- Unity-MCP sync/validate logs showed sync from `Assets/CSVdata/runtime`, runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies, and `InGame skill data validation passed with 0 warning(s)`.

### History

- 2026-06-19: User requested Code Builder to finish Ariel A-J decomposition using the report's lines 373-962 as the node/effect/binding standard.

## Task: 2026-06-19 CSVdata Folder Reorganization

### Task title

Move active CSV authoring files into purpose-specific `Assets/CSVdata` folders and remove unused Codex backup CSV files.

### Goals

- Replace the old flat `Assets/CSVdata/source` runtime CSV folder with purpose-based runtime folders.
- Keep runtime catalog sync, validation, and editor auto-sync functional after the move.
- Preserve Unity GUID references by moving CSV `.meta` files with the CSV files.
- Delete unused `.bak_codex` CSV backup files from the active CSV folder.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV files remain UTF-8 and retain their row shape.
- `StageDay.csv`, `StageEncounter.csv`, and `StageReward.csv` remain active `NewRunScene` stage-flow inputs and were moved, not deleted.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, CSV-shape checked, compiled, and Unity-MCP sync/validate checked.

### Next Actions

- Future runtime CSV files should be added under `Assets/CSVdata/runtime/{catalog,enemy,monster,status}` by ownership.
- Future NewRunScene stage-flow CSV files should stay under `Assets/CSVdata/stage_flow`.
- Do not restore `Assets/CSVdata/source`; update `PakuriCsvRuntimeData.GetImportedSourceAssetPath(...)` when adding a new runtime CSV table.

### Evidence

- Active catalog CSV files now live under `Pakuri/Assets/CSVdata/runtime/catalog/`.
- Active enemy CSV files now live under `Pakuri/Assets/CSVdata/runtime/enemy/`, including `EnemySkillData.csv`.
- Active monster base/choice catalog CSV files now live under `Pakuri/Assets/CSVdata/runtime/monster/`.
- Active monster skill CSV files now live under `Pakuri/Assets/CSVdata/runtime/monster/skills/`.
- Active status CSV files now live under `Pakuri/Assets/CSVdata/runtime/status/`.
- Active stage-flow CSV files now live under `Pakuri/Assets/CSVdata/stage_flow/`.
- Deleted unused backups: `monster_skill_choices.csv.bak_codex`, `monster_skill_effects.csv.bak_codex`, `monster_skill_triger.csv.bak_codex`, and their `.meta` files.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.cs` now maps each runtime CSV filename to its purpose-specific folder through `GetImportedSourceAssetPath(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` now loads imported runtime CSVs through `GetImportedSourceAssetPath(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Editor/PakuriCsvRuntimeCatalogPostprocessor.cs` now watches `Assets/CSVdata/runtime/**/*.csv`.
- `Pakuri/Assets/Scripts2/InGame/Data/Editor/PakuriSkillEffectPrefabCsvExporter.cs` now writes `monster_skill_choices.csv` at `Assets/CSVdata/runtime/monster/skills/monster_skill_choices.csv`.
- PowerShell TextFieldParser check returned `bad=` empty for all active CSV files after the move, including runtime and stage-flow CSVs.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/runtime' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.`
- Unity-MCP warning/error console read after sync/validate returned 0 entries.

### History

- 2026-06-19: User requested Code Builder to reorganize `Pakuri/Assets/CSVdata` files by purpose, update hard paths, delete `.bak_codex` backups, run dotnet builds, and verify Unity-MCP CSV sync/validate.
- 2026-06-19: Code Builder moved active CSV files into `runtime/` and `stage_flow/`, removed the old empty `source` folder, updated runtime/editor hard paths, deleted unused `.bak_codex` backup files, and verified CSV shape, builds, and Unity-MCP sync/validate.

## Task: 2026-06-19 Ariel Normalized Choice Node Implementation

### Task title

Move Ariel numeric choice behavior toward generic normalized node authoring and reduce Ariel C pre-combined effect rows.

### Goals

- Use `monster_skill_nodes.csv` and `monster_skill_node_params.csv` as the user-selected generic effect-object storage path.
- Add reusable node handlers for common choice modifiers instead of adding new wide CSV columns.
- Migrate Ariel numeric choice modifiers out of `monster_skill_choices.csv` behavior fields into normalized choice nodes.
- Keep old effect rows only where still needed for compatibility, and disable Ariel C rows made redundant by composition.

### Constraints

- Role Owner is Code Builder.
- No specialized `skill_effect_bindings.csv`, `skill_effect_defs.csv`, or `skill_effect_modifiers.csv` files were added.
- Existing wide columns remain parser-compatible for old rows.
- Unity Play Mode parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, synced, and Unity-MCP validated.

### Next Actions

- User verifies Ariel C, Ariel B shield amount/duration, Ariel E shield trait/master composition, and Ariel J post-E / E-shield-only behavior in Play Mode.
- Future new exception behavior should prefer normalized node rows over new wide `monster_skill_choices.csv` columns.
- Code Reviewer pass is pending after the Phase 2-5 implementation.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now registers reusable node handlers including `CountStatusDamageMultiplier`, `MagazineBonus`, `ReloadTimeMultiplier`, `PierceBonus`, `DurationBonus`, `StatusActionSpeedBonus`, `StatusAilmentResistanceBonus`, `StatusConditionalDamageTakenBonus`, and `StatusElementDamageTakenBonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now keeps `PassiveDefinition.NormalizedPlanNodes`, and `PakuriCsvRuntimeData.Build.cs` builds passive-owned normalized nodes.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps the new node handlers into `SkillChoiceEffectSpec`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now applies normalized choice nodes during combat snapshot creation.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_nodes.csv` now has 47 imported rows after the Ariel migration, including `ariel-c-trait-2-blessing-action-speed`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_node_params.csv` now has 69 imported rows after the Ariel migration and Ariel C trait2 targeted action-speed node addition.
- Initial PowerShell migration output returned `migrated=28 nodes=47 params=68`; the final Ariel C trait2 node addition brought the parsed param row count to 69.
- TextFieldParser CSV shape check returned `monster_skill_choices.csv header=114 rows=252 bad=`, `monster_skill_nodes.csv header=14 rows=47 bad=`, `monster_skill_node_params.csv header=4 rows=69 bad=`, and `monster_skill_effects.csv header=70 rows=131 bad=`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_effects.csv` has 9 Ariel C pre-combined blessing rows disabled as `MigratedToEffectBinding`.
- Follow-up Phase 2-5 cleanup added the `ShieldAmountMultiplier` node handler and four Ariel shield amount nodes for B trait1, B master1, E trait2, and E master2.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_effects.csv` now has one active `ariel-e-shield*` row and three disabled E shield variants marked `MigratedToEffectBinding`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_effects.csv` no longer keeps J post-E action-speed behavior under `ariel-e`; those effects are now `ariel-j-after-e-action-speed*`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_triger.csv` now has two J-owned `OnSkillCast` trigger rows for `event_skill_id=ariel-e`.
- `condition_status_source_skill_id` was added to `monster_skill_effects.csv` and runtime parsing/build code so `ariel-j-shielded-holy-damage` can require the shield source `ariel-e-shield-base`.
- Phase 2-5 CSV shape check returned no bad rows for active Ariel-related skill CSV files, with `monster_skill_effects.csv header=71 rows=133 bad=`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP sync/validate logs showed CSV runtime catalog sync from `Assets/CSVdata/runtime`, runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies, and `InGame skill data validation passed with 0 warning(s).`

### History

- 2026-06-19: User requested Code Builder to execute the Ariel effect-object trigger-binding handoff and answered all six ambiguous design questions, including use of generic node CSVs.
- 2026-06-19: User then requested remaining Phase 2-5 implementation; Code Builder added shield amount nodes, reduced E shield variants, moved J post-E behavior to J-owned trigger/effect rows, and added effect source-skill conditions.

## Task: 2026-06-17 Normalized Skill Authoring Row Table Handoff

### Task title

Design the next CSV-authoring refactor so new exception skills add behavior nodes instead of new wide CSV columns.

### Goals

- Convert the 2026-05-29 skill runtime refactor feedback into a DATA-scoped authoring schema handoff.
- Keep current `monster_skills.csv` and `monster_skill_choices.csv` compatible during migration.
- Define a normalized row-table path where future behavior is authored through `monster_skill_nodes.csv` and `monster_skill_node_params.csv` instead of new CSV headers.
- Preserve existing `monster_skill_effects.csv` and `monster_skill_triger.csv` in the first pass because they already have row-like runtime support.
- Give Code Builder phases for parser skeleton, node compiler integration, first sample migration, choice-family migration, and future wide-column freeze.

### Constraints

- Role Owner is Designer for this handoff.
- Phase A changed CSV schema skeleton files, runtime CSV parser/model/validation code, and runtime source catalog references only.
- The handoff is grounded in the inspected source feedback html/md, active CSV headers, current parser/build code, and Phase 6 `SkillExecutionPlan` surface.
- Old wide columns must not be deleted in the first implementation.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Phase E wide-column freeze policy applied; Phase D choice-family migration remains locally validated.

### Next Actions

- Code Builder keeps legacy wide CSV behavior active until a later migration phase explicitly moves behavior into normalized nodes; `rin-d` is the first Phase C exception sample now migrated to normalized nodes.
- Code Builder updates `boards/COMBAT/ENEMY_BLACKBOARD.md` if normalized nodes change `SkillExecutionPlan`, executor routing, or runtime skill behavior.
- Code Reviewer should review before real skill authoring starts on the new node path.
- Phase C sample migration now has a duplicate guard for the currently supported execute/boss/kill normalized handlers.
- Phase D now has representative choice-owned normalized rows for damage/cooldown/radius modifiers, execute/boss/kill choice actions, on-hit additional damage, repeat per target, conditional crit, and redistribute-on-kill behavior; keep new exception choice behavior on node rows/params instead of adding new `monster_skill_choices.csv` behavior columns.
- Phase D reviewer follow-up now keeps representative choice metadata in `monster_skill_choice_base.csv`; duplicate legacy rows keep their behavior compatibility values, but `BuildSkillChoices(...)` prefers base-row metadata and can build future base-only choice rows with normalized nodes.
- Phase E board rule: new exception skill behavior must use `monster_skill_nodes.csv` plus `monster_skill_node_params.csv` by default.
- Phase E exception rule: adding new behavior columns to `monster_skills.csv` or `monster_skill_choices.csv` requires explicit Designer or Code Builder approval recorded in the active handoff or DATA board task.
- Existing wide behavior columns in `monster_skills.csv` and `monster_skill_choices.csv` are compatibility/deprecated inputs; keep them readable until enough migrated rows are proven through Play Mode.

### Evidence

- Created `Pakuri/reference/Report/2026-06-17-normalized-skill-authoring-row-table-handoff.md`.
- Source feedback inspected: `Pakuri/reference/Report/2026-05-29-skill-runtime-refactor-feedback-handoff.md` and `.html`.
- `monster_skills.csv` currently has 72 columns and 51 imported data rows.
- `monster_skill_choices.csv` currently has 114 columns and 253 imported data rows.
- `monster_skill_effects.csv` currently has 70 columns and 132 imported data rows.
- `monster_skill_triger.csv` currently has 47 columns and 57 imported data rows.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:417` to `:421` parses execute/boss/kill base skill columns directly.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:505` to `:533` parses execute/boss/kill choice columns directly.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:418` to `:490` shows `SkillDefinition` still owns many wide behavior fields.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:264` to `:350` shows `SkillChoiceDefinition` still owns many wide behavior fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs:6` to `:24` defines authoring source and node kind enums.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs:213` to `:226` already accepts normalized node rows through the compiler overload.
- Phase A added optional source catalog TextAsset fields for `monster_skill_base.csv`, `monster_skill_choice_base.csv`, `monster_skill_nodes.csv`, and `monster_skill_node_params.csv`.
- Phase A added empty header/type skeleton CSV files under `Pakuri/Assets/CSVdata/source/`.
- Phase A added `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` with row models, optional parsers, handler schema registry, and node/param validation.
- Phase A follow-up added handler-schema enum param value validation for normalized node params such as `predicate`, `attribute`, and `target_side`.
- Phase B added `SkillNodeDefinition` and `SkillNodeParamDefinition` to `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`.
- Phase B routes skill-owned normalized rows from `BuildActiveSkills(...)` into `SkillDefinition.NormalizedPlanNodes` and choice-owned normalized rows from `BuildSkillChoices(...)` into `SkillChoiceDefinition.NormalizedPlanNodes`.
- Phase B maps `SkillNodeDefinition[]` through `InGameSkillDefinitionMapper.MapSkillNodeDefinitions(...)` into `SkillExecutionPlanNode[]`.
- Phase B currently converts supported normalized handlers `TargetHealthRatioThresholdBonus`, `TargetPredicateDamageMultiplier` with `predicate=is_boss`, `BossDamageMultiplier`, `ExecuteCritChanceBonus`, `CooldownReset`, `CooldownResetOnKill`, `CooldownRefund`, and `CooldownRefundBonus` into typed plan ops; unsupported handlers still enter `SkillExecutionPlan.Nodes` as normalized row metadata without executable op payload.
- Phase B stores mapped normalized nodes on `SkillData.NormalizedPlanNodes` and `SkillChoiceEffectSpec.NormalizedPlanNodes`, and `SkillExecutionSnapshot` feeds them into `SkillExecutionPlanCompiler.Compile(source, snapshot, normalizedRows)`.
- Phase B reviewer follow-up preserves node `runtime_support_state` / `runtime_support_notes` and nested param `node_id` on `SkillNodeDefinition` / `SkillNodeParamDefinition` so runtime definitions no longer drop the normalized authoring support metadata.
- Phase B reviewer follow-up now validation-fails `owner_kind=Passive`, `owner_kind=Effect`, and `owner_kind=Trigger` until those owner paths are actually wired into runtime plans, preventing valid-looking normalized rows from being silently ignored.
- Phase A reviewer follow-up now enforces schema-declared enum params such as `predicate`, `attribute`, and `target_side` to use `value_type=Enum` and validates the authored value against the handler schema even when the row tries a different value type.
- Phase C migrated the `rin-d` base execute/kill sample by setting the legacy numeric wide fields `execute_health_ratio_threshold=0`, `execute_damage_multiplier=1`, and `kill_cooldown_refund_ratio=0` while keeping the old columns present and readable.
- Phase C added `rin-d-execute-condition`, `rin-d-execute-multiplier`, `rin-d-boss-multiplier`, and `rin-d-kill-cooldown-refund` rows to `monster_skill_nodes.csv`, with seven matching rows in `monster_skill_node_params.csv`.
- Phase C added duplicate validation guards so enabled normalized nodes for `TargetHealthRatioCondition`, `ExecuteDamageMultiplier`, boss multiplier handlers, `CooldownRefund`, `TargetHealthRatioThresholdBonus`, `ExecuteCritChanceBonus`, `CooldownRefundBonus`, and cooldown reset handlers fail when the matching legacy wide field is still active on the same owner.
- `dotnet build Pakuri.sln --no-restore` succeeded with 0 errors and existing `System.Net.Http` / `System.IO.Compression` conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after the enum validation follow-up; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after the enum validation follow-up; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Validate CSV Source Data` logged runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies after the empty normalized CSV files were imported.
- Phase B `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after implementation; existing `MSB3277` warnings remained.
- Phase B `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after implementation; existing `MSB3277` warnings remained.
- Unity-MCP Phase B smoke returned `nodes=1, damageModifiers=1, firstRow=phase_b_test_node, multiplier=1.25` for an in-memory normalized `BossDamageMultiplier` node.
- Phase B reviewer follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP direct `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` after the reviewer follow-up logged `Pakuri CSV runtime catalogs synced and validated from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP Phase B reviewer follow-up smoke returned `nodes=1, damageModifiers=1, row=phase_b_review_node, multiplier=1.25, support=RuntimeImplemented:phase_b_review_node`.
- Unity-MCP Phase B current-catalog check returned `catalog=True, activeSkills=25, skillNodes=0, choiceNodes=0`, confirming current empty normalized CSV rows do not add plan nodes to existing skills.
- Unity-MCP console after Phase B catalog load logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.`
- Phase C CSV field-count check returned `monster_skills.csv header=72 rows=52 bad=`, `monster_skill_nodes.csv header=14 rows=6 bad=`, and `monster_skill_node_params.csv header=4 rows=9 bad=`.
- Phase C `Import-Csv` check returned `rin-d` legacy values `threshold=0`, `require=true`, `execute=1`, `refund=0`, `boss=1`, with `nodeCount=4`, `paramCount=7`, and handlers `TargetHealthRatioCondition,ExecuteDamageMultiplier,TargetPredicateDamageMultiplier,CooldownRefund`.
- Phase C `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase C Unity-MCP `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` returned `SyncAndValidateCsvRuntimeCatalogsForEditor completed`.
- Phase C Unity-MCP runtime catalog inspection returned `legacy=threshold:0,require:True,execute:1,refund:0,boss:1|defNodes=4|planNodes=4|casts=1:0.3|damage=2:ExecuteMultiplier:1.8,BossMultiplier:1|kills=1:CooldownRefundBonus:0.35`.
- Phase D added 14 `Choice` owner rows to `monster_skill_nodes.csv` and 29 matching param rows to `monster_skill_node_params.csv`; handlers are `DamageMultiplier`, `CooldownMultiplier`, `RadiusMultiplier`, `TargetHealthRatioThresholdBonus`, `ExecuteCritChanceBonus`, `CooldownReset`, `TargetPredicateDamageMultiplier`, `CooldownRefundBonus`, `AdditionalDamage`, `EveryNthHitChainDamage`, `RepeatPerTarget`, `TargetStatusCritBonus`, and `RedistributeConsumedStatus`.
- Phase D migrated representative legacy values out of `monster_skill_choices.csv` for `ariel-a-trait-1`, `ariel-b-trait-3`, `ariel-c-trait-4`, `rin-d-trait-2`, `rin-d-master-1`, `rin-d-trait-5`, `rin-d-trait-3`, `rin-a-master-2`, `vega-d-master-1`, `vega-e-trait-4`, and `vega-e-trait-5` while keeping the old columns present.
- Phase D CSV field-count check using `Microsoft.VisualBasic.FileIO.TextFieldParser` returned `monster_skill_choices.csv: header=114 rows=253 bad=`, `monster_skill_nodes.csv: header=14 rows=19 bad=`, and `monster_skill_node_params.csv: header=4 rows=37 bad=`.
- Phase D `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase D Unity-MCP `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` returned `sync-ok`, and Unity-MCP `read_console` returned 0 warning/error entries afterward.
- Phase D Unity-MCP choice-family smoke returned `ariel-a-trait-1=True:1.25:nodes1`, `ariel-b-trait-3=True:0.8:nodes1`, `ariel-c-trait-4=True:1.25:nodes1`, `rin-a-master-2=extraTrue:1:0.4:Lightning:HitTarget|chain3:2:4.5:0.4:Lightning:nodes2`, `vega-d-master-1=damageTrue:0.65|repeat2:0.15:0.6:nodes2`, `vega-e-trait-4=crit0.35:name-mark:1:nodes1`, and `vega-e-trait-5=redistribute0.25:name-mark:5:3:nodes1`.
- Phase D fixed the duplicate-overlap guard so blank legacy chain/repeat multiplier cells are treated like the existing Build fallback (`>0 ? value : 1`) instead of being falsely considered active legacy wide behavior.
- Phase D reviewer follow-up filled `Pakuri/Assets/CSVdata/source/monster_skill_choice_base.csv` with 11 representative metadata rows for `ariel-a-trait-1`, `ariel-b-trait-3`, `ariel-c-trait-4`, `rin-a-master-2`, `rin-d-trait-2`, `rin-d-trait-3`, `rin-d-trait-5`, `rin-d-master-1`, `vega-d-master-1`, `vega-e-trait-4`, and `vega-e-trait-5`.
- Phase D reviewer follow-up updated `BuildSkillChoices(...)` so legacy duplicate choice rows preserve existing behavior fields while metadata fields come from `SkillChoiceBaseRows`, and base-only rows are merged by `sort_order` through `BuildBaseOnlySkillChoiceDefinition(...)`.
- Phase D reviewer follow-up updated normalized validation so choice-owned nodes and choice gates accept `monster_skill_choice_base.csv` rows, duplicate base/legacy rows must match `monster_id`, `skill_id`, and `choice_group`, and runtime asset validation accepts either legacy or base choice source rows.
- Phase D reviewer follow-up CSV field-count check returned `monster_skill_choice_base.csv header=13 lines=13 bad=`, `monster_skill_choices.csv header=114 lines=254 bad=`, `monster_skill_nodes.csv header=14 lines=20 bad=`, and `monster_skill_node_params.csv header=4 lines=38 bad=`.
- Phase D reviewer follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase D reviewer follow-up Unity-MCP `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` returned `sync-ok`; representative choice smoke returned each of the 11 migrated choice ids with `count=1` and expected node counts (`nodes=1` or `nodes=2`) plus base metadata descriptions.
- Phase D reviewer follow-up Unity-MCP console warning/error read showed only MCP transport `Client handler error: Cannot access a disposed object`, not a Pakuri CSV validation or C# compile error.
- Phase E updated `Pakuri/reference/Report/2026-06-17-normalized-skill-authoring-row-table-handoff.md` and `boards/SkillBluePrint/skill-csv-exception-guide.md` so future exception behavior routes to normalized nodes by default.
- Phase E marks existing wide behavior columns in `monster_skills.csv` and `monster_skill_choices.csv` as compatibility/deprecated authoring surfaces while preserving old CSV rows and old columns.
- Phase E TextFieldParser CSV field-count check returned `monster_skills.csv header=72 rows=51 bad=`, `monster_skill_choices.csv header=114 rows=253 bad=`, `monster_skill_base.csv header=13 rows=1 bad=`, `monster_skill_choice_base.csv header=13 rows=12 bad=`, `monster_skill_nodes.csv header=14 rows=19 bad=`, `monster_skill_node_params.csv header=4 rows=37 bad=`, `monster_skill_effects.csv header=70 rows=132 bad=`, and `monster_skill_triger.csv header=47 rows=57 bad=`.
- Phase E `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- Phase E Unity-MCP `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` returned `sync-ok`, and Unity-MCP warning/error console read returned 0 entries.

### History

- 2026-06-17: User noted that Phase 6 had not actually split or structured the CSV authoring layer yet, then requested a design and handoff for splitting `monster_skills.csv` and `monster_skill_choices.csv` so new exception skills can add nodes instead of columns.
- 2026-06-17: Code Builder implemented Phase A parser skeleton: optional normalized CSV files, row models, handler schema validation, empty schema CSVs, Unity import, and validation.
- 2026-06-17: Code Builder fixed the Code Reviewer finding that enum node params were only partly validated by adding handler-schema allowed enum values and re-running build plus Unity CSV validation.
- 2026-06-17: Code Builder implemented Phase B by preserving normalized CSV rows as `SkillNodeDefinition`, mapping supported handlers into `SkillExecutionPlanNode` operation payloads, and feeding base skill plus choice nodes into `SkillExecutionSnapshot` without removing or disabling legacy wide-column bridges.
- 2026-06-17: Code Builder fixed the Phase B Code Reviewer findings by preserving node support metadata, preserving nested param node ids, blocking unsupported passive/effect/trigger node owner kinds until runtime adapters exist, and tightening schema enum param validation.
- 2026-06-17: Code Builder implemented Phase C first sample migration for `rin-d`, added execute multiplier plan-op support, added duplicate guard validation for supported legacy+normalized behavior overlap, and verified the migrated sample through CSV checks, dotnet builds, Unity CSV validation, and runtime catalog plan inspection.
- 2026-06-17: Code Builder implemented Phase D representative choice-family migration, moved selected `monster_skill_choices.csv` behavior values into normalized choice-owned nodes/params, mapped generic choice nodes back into `SkillChoiceEffectSpec`, extended duplicate guards, and verified CSV shape, builds, Unity CSV sync, and runtime choice/node smoke checks.
- 2026-06-18: Code Builder fixed the Phase D Reviewer finding by populating choice base metadata rows, making `BuildSkillChoices(...)` use base metadata for duplicate rows and support base-only normalized choice rows, extending validation, and re-running CSV shape checks, dotnet builds, Unity CSV sync, and representative choice smoke checks.
- 2026-06-18: Code Builder started Phase E by applying the DATA board rule and Skill Builder exception-guide rule that new exception skill behavior defaults to normalized nodes, with old wide behavior columns treated as compatibility/deprecated inputs until Play Mode-proven migration coverage is sufficient; then verified CSV field counts, dotnet builds, Unity CSV sync, and console warning/error state.

## Task: 2026-05-31 Enemy Nexus Damage CSV Column

### Task title

Add `nexus_damage` to active stage enemy source CSVs and route it into enemy runtime data.

### Goals

- Add an authored Nexus damage value for Stage 1 and Stage 2 enemies.
- Keep current enemies at 1 Nexus damage by default.
- Keep CSV validation and runtime catalog sync passing after the schema extension.

### Constraints

- Role Owner is Code Builder.
- The authored header uses existing snake_case CSV style: `nexus_damage`.
- Parser lookup is case-insensitive, but active source authority records the snake_case column name.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, shape-checked, validated, and synced.

### Next Actions

- Change `nexus_damage` values in `stage_one_enemies.csv` or `stage_two_enemies.csv` when enemy-specific Nexus damage is designed.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` and `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv` now include `nexus_damage` with value `1` on current enemy rows.
- PowerShell field-count verification returned 27 header fields and no bad rows for both enemy CSV files.
- PowerShell `Import-Csv` verification found no blank/invalid `nexus_damage` values in either enemy CSV.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.EnemyDataset.cs` reads optional `nexus_damage` with default `1`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `NexusDamage` into `EnemyDefinition`.
- Unity menu `Pakuri/Validate CSV Source Data` logged a runtime catalog load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged a sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-31: User requested a Nexus damage column for Stage 1 and Stage 2 enemy CSVs.
- 2026-05-31: Code Builder added `nexus_damage`, mapped it through source CSV parsing/build, and synced runtime catalog assets.

## Task: 2026-05-31 Stage2 Enemy Runtime Catalog And Stage Flow Connection

### Task title

Connect Stage 2 enemy source data to the runtime CSV catalog and active stage-flow CSVs.

### Goals

- Add Stage 2 enemy catalog/source loading beside the existing Stage 1 enemy runtime path.
- Author Stage 2 day, encounter, and reward rows so `RunSession` stage advance can find Stage 2 data.
- Keep Stage 1 enemy sprite paths valid after the old `Assets/Image/Stage1/Enemy` path was no longer present.

### Constraints

- Role Owner is Code Builder.
- Stage 2 reward numbers currently copy the Stage 1 reward pattern because no separate Stage 2 reward-balance source was provided.
- Stage 2 unit sprite paths remain blank; prefab visuals are connected through `NewRunScene` enemy prefab bindings.

### Role Owner

Code Builder

### Status

Implemented and Unity CSV validation passed.

### Next Actions

- User verifies Stage 2 progression and spawn feel in Play Mode.
- Replace copied Stage 2 reward values when Stage 2-specific economy balance is authored.

### Evidence

- `Pakuri/Assets/CSVdata/source/catalog_stage_two_enemies.csv` now contains 8 Stage 2 enemy catalog entries.
- `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv` now uses runtime-supported Stage 2 passive IDs such as `FireDefenseUp`, `LightningDamageUp`, `IceDefenseUp`, and `HolyDefenseUp`.
- `Pakuri/Assets/CSVdata/StageDay.csv` now contains 11 `stage=2` rows.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` now contains 30 `stage2-*` encounter rows.
- `Pakuri/Assets/CSVdata/StageReward.csv` now contains `reward-stage2-normal`, `reward-stage2-midboss`, `reward-stage2-day10-midboss`, and `reward-stage2-boss`.
- CSV field-count check returned no bad rows for `stage_one_enemies.csv`, `stage_two_enemies.csv`, `catalog_stage_two_enemies.csv`, `StageDay.csv`, `StageEncounter.csv`, and `StageReward.csv`.
- Reference check returned `missingDayEncounter=`, `missingDayReward=`, `missingEncounterEnemy=`, `stage2Days=11`, and `stage2Encounters=30`.
- Unity `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog ... with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.`

### History

- 2026-05-31: User requested implementation of the Stage 2 passive/runtime/prefab/stage-flow connection after confirming `stage2-holy-priest.prefab` had the required actor and collider components.
- 2026-05-31: Code Builder connected Stage 2 source CSVs to the runtime catalog, added Stage 2 stage-flow rows, and corrected moved Stage 1 sprite paths to the existing `Assets/Enemy/Stage1/Enemy/Stage1/*.png` assets so CSV validation could complete.

## Task: 2026-05-31 Stage2 Enemy Data-Only Source CSV

### Task title

Create a data-only `stage_two_enemies.csv` source file using the current `stage_one_enemies.csv` shape.

### Goals

- Add the Stage 2 enemy reference data without connecting it to the runtime catalog yet.
- Keep the column layout identical to `stage_one_enemies.csv`.
- Copy `stage_one_skill`, `basic_skill`, `passive_skill_name`, `passive_skill_id`, and `passive_skill_value` from `stage_one_enemies.csv` by row order.
- Fill `passive_summary` from `Pakuri/reference/5.enemy/stage-2-enemies.md`.

### Constraints

- Role Owner is Code Builder.
- The new CSV is intentionally runtime-unconnected data only.
- No runtime catalog, source catalog asset, enum, skill, prefab, scene, or encounter wiring was changed.
- Stage 2 sprite paths remain blank because no Stage 2 sprite asset paths were provided or inspected for this task.

### Role Owner

Code Builder

### Status

Implemented and local CSV shape verified.

### Next Actions

- If Stage 2 should become runtime-loaded later, add explicit runtime catalog/source-catalog support and a Stage 2 encounter/spawn path instead of assuming this data-only CSV is loaded.
- Later Stage 2 runtime work should decide whether enemy skills stay on `StageOneEnemySkillKind` placeholders or move to a stage-neutral enemy skill id path.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv` was absent before creation; `Test-Path` returned `False` for both `stage_two_enemies.csv` and the typo path `stage_two_enemiese.csv`.
- `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv:1` now matches the `stage_one_enemies.csv` header.
- `Pakuri/Assets/CSVdata/source/stage_two_enemies.csv:3` through `:10` contain the eight Stage 2 enemy rows from `Pakuri/reference/5.enemy/stage-2-enemies.md`.
- PowerShell field-count verification returned `header=26 rows=10 bad=`.
- PowerShell comparison against `stage_one_enemies.csv` returned `copied=True` for all eight Stage 2 rows for `stage_one_skill`, `basic_skill`, `passive_skill_name`, `passive_skill_id`, and `passive_skill_value`.

### History

- 2026-05-31: User requested a data-only Stage 2 enemy CSV, same shape as `stage_one_enemies.csv`, with selected Stage 1 skill/passive columns copied and only `passive_summary` adapted from the Stage 2 reference.
- 2026-05-31: Code Builder created `stage_two_enemies.csv` without runtime hookup.

## Task: 2026-05-31 Vega F-J Passive Shared CSV Authoring And Effect-Header Normalization

### Task title

Author Vega F-J passive rows on the active CSV authority and normalize the passive-effect CSV schema so the new shared columns validate in Unity.

### Goals

- Keep Vega F-J passive implementation inside the active source CSV files instead of adding a Vega-only companion table.
- Author the required passive base/effect/trigger rows for Vega F-J.
- Keep the new generic effect schema aligned between authored rows, header/type rows, and Unity runtime import.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV scope stayed limited to `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, and the already-active `monster_skills.csv`.
- No new CSV file was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and validation passed.

### Next Actions

- Reuse `required_source_status_id` / `required_source_status_min_stacks` on passive effect rows before adding new aura-specific CSV tables.
- Reuse `status_conditional_incoming_skill_runtime_kinds` and `status_conditional_outgoing_skill_runtime_kinds` for future runtime-kind-specific damage modifiers before adding skill-specific hardcoding.
- When bulk-editing imported CSV outside Unity, force an editor asset refresh before trusting `TextAsset`-backed validation results.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header/type rows now contain 70 columns and include the new generic fields `required_source_status_id`, `required_source_status_min_stacks`, `status_conditional_incoming_skill_runtime_kinds`, and `status_conditional_outgoing_skill_runtime_kinds`.
- The same effect CSV now contains the Vega passive rows that were previously absent:
  - Vega-F at lines 114-117.
  - Vega-G at lines 118-120.
  - Vega-H at lines 122-125.
  - Vega-I at lines 126-131.
  - Vega-J at lines 132-133.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains the Vega passive trigger rows at lines 46-58, including the `event_skill_runtime_kinds=Area` filter used by `vega-i-area-cooldown-base`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains the Vega F-J `RuntimeImplemented` enhancement rows at lines 189-203 and the missing Vega-H `PassiveBase` row `vega-h-base-duration` at line 254.
- The first Unity validation pass failed with `CsvFatalException: CSV file 'monster_skill_effects.csv' row 114 has 70 columns but expected 66.`
- After normalizing the effect CSV header/type rows and forcing a Unity asset refresh, Unity menu `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` then logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Vega F-J passive implementation added new generic effect fields and the final Vega passive rows in the active CSV set.
- 2026-05-31: The first Unity validation attempt exposed that `monster_skill_effects.csv` data rows had already widened to 70 columns while the header/type rows were still 66 columns.
- 2026-05-31: Code Builder normalized the effect CSV schema, forced Unity asset refresh, and re-ran validation/sync successfully.

## Task: 2026-05-31 Vega-D Active Row Re-authoring For Overlap And Delayed Repeats

### Task title

Re-author the active Vega-D skill and master-1 choice rows so overlapping local AoE hits and delayed per-target repeats are expressed entirely in the existing CSV authority.

### Goals

- Keep Vega-D on `monster_skills.csv` and `monster_skill_choices.csv` without adding a new CSV column or a new companion table.
- Express overlap-enabled local fanout through `hit_target_count=global`.
- Express base plus two delayed extra hits through `repeat_count_per_target=2` and `repeat_interval_seconds=0.5`.

### Constraints

- Role Owner is Code Builder.
- Active CSV authority stayed limited to `monster_skills.csv` and `monster_skill_choices.csv` for this task.
- No CSV schema change was needed.

### Role Owner

Code Builder

### Status

Implemented, synced, and validation passed.

### Next Actions

- Reuse the same row pattern when another shared `SingleAttack` fanout skill needs overlap stacking at each resolved center.
- Reuse repeat-per-target authoring before adding a parallel trigger row when the desired pattern is still “immediate base hit plus delayed extra repeats at the same center.”

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` line `vega-d` now sets `hit_target_count=global` while preserving the existing marked-target fanout fields and prefab path.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` line `vega-d-master-1` now sets `description_text=각 표식 대상 위치에 범위 참격 2회 추가 발생, 각 참격 위력 -35%`, `repeat_count_per_target=2`, and `repeat_interval_seconds=0.5`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` confirmed `vega-d.hit_target_count=global`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` confirmed `vega-d-master-1.repeat_count_per_target=2`, `repeat_interval_seconds=0.5`, and `repeat_damage_multiplier=1`.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Earlier same-day Vega-D row authoring had left `hit_target_count` blank and `repeat_count_per_target=1`, which matched the temporary single-target local-hit interpretation.
- 2026-05-31: User then requested overlap-enabled area damage and two delayed extra slashes, so Code Builder updated the active rows without widening schema or runtime scope.

## Task: 2026-05-31 Vega-E Shared Choice/Skill CSV Extension And Active Row Authoring

### Task title

Extend the active monster skill CSV schema for reusable marked-target execution data, then author Vega E on that shared path.

### Goals

- Keep Vega E on the active `monster_skills.csv` and `monster_skill_choices.csv` authority instead of introducing a Vega-only companion table.
- Add only the shared columns needed for marked-target selection, target-status-stack damage, partial target-status consumption, conditional crit, and consumed-status redistribution.
- Keep unsupported row state explicit when reference authority is still incomplete.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV authority stayed limited to `monster_skills.csv` and `monster_skill_choices.csv` for this task.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and validation passed. Vega-E trait-5 row is now fully authored with the user-provided nearby-search values.

### Next Actions

- Reuse the new target-selection, target-stack-damage, consume, conditional-crit, and redistribution columns for future shared marked-target finishers before adding another skill CSV.
- Reuse the same `redistribute_consumed_status_search_radius` plus `redistribute_consumed_status_target_count` pair when future skills need bounded redistribution instead of inventing a new spread schema.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` header now includes `target_selection_status_id`, `target_selection_status_min_stacks`, `target_status_stack_status_id`, `target_status_stack_max_stacks`, `target_status_stack_base_damage`, `target_status_stack_attack_power_coefficient`, `target_status_stack_spell_power_coefficient`, `consume_target_status_id`, `consume_target_status_ratio`, and `consume_target_status_stacks`.
- The same skill CSV now authors `vega-e` with `target_selection=HighestStacks`, `target_selection_status_id=name-mark`, `target_selection_status_min_stacks=1`, `target_status_stack_status_id=name-mark`, `target_status_stack_base_damage=6`, `target_status_stack_attack_power_coefficient=0.18`, `consume_target_status_id=name-mark`, `consume_target_status_ratio=0.5`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header now includes `target_status_stack_damage_multiplier`, `consume_target_status_ratio_override`, `consume_target_status_stacks_override`, `conditional_crit_chance_bonus`, `conditional_crit_target_status_id`, `conditional_crit_target_status_min_stacks`, `redistribute_consumed_status_ratio_on_kill`, `redistribute_consumed_status_id`, `redistribute_consumed_status_search_radius`, and `redistribute_consumed_status_target_count`.
- The same choice CSV now authors `vega-e-trait-1`, `trait-2`, `trait-3`, `trait-4`, `trait-5`, `master-1`, and `master-2` as `RuntimeImplemented`; `vega-e-trait-5` now includes `redistribute_consumed_status_ratio_on_kill=0.25`, `redistribute_consumed_status_id=name-mark`, `redistribute_consumed_status_search_radius=100`, and `redistribute_consumed_status_target_count=1`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` confirmed the corrected Vega E row alignment after the first failed validation pass.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Code Builder first extended the shared active CSV schema for Vega E and authored the active rows.
- 2026-05-31: Initial Unity validation exposed a malformed Vega E row alignment in `monster_skill_choices.csv`; Builder corrected the row shape and re-ran validation/sync successfully.
- 2026-05-31: User then supplied the remaining trait-5 nearby-search authority and final Vega-E prefab path, so Skill Builder finished the active row authoring without another schema change.

## Task: 2026-05-30 Vega C/D Shared CSV Schema Extension And Active Row Authoring

### Task title

Extend the active monster skill CSV schema for reusable buff-active and marked-target fanout behavior, then author Vega C and Vega D on that shared data path.

### Goals

- Keep the new Vega C and Vega D behavior owned by the existing active CSV authority instead of adding a Vega-only file.
- Add only the shared columns needed for attached buff scalar overrides, source-status-gated modifiers, repeat-per-target fanout, and marked-target deployment filtering.
- Connect the user-provided Vega C/D prefab paths in the active skill rows.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV authority stayed limited to `monster_skills.csv` and `monster_skill_choices.csv` for this task.
- No new CSV file was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and validation passed.

### Next Actions

- Reuse `deployment_required_target_status_id` plus `deployment_required_target_status_min_stacks` for future shared marked-target fanout rows before inventing another deployment table.
- Reuse `runtime_target_skill_ids`, `required_source_status_id`, `status_action_speed_bonus`, `status_attack_power_bonus`, and repeat-per-target choice columns for future buff-active follow-up rules before adding another companion CSV.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` header now includes `deployment_required_target_status_id` and `deployment_required_target_status_min_stacks`.
- The same skill CSV now authors `vega-c` with `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_C.prefab`.
- The same skill CSV now authors `vega-d` with `runtime_kind=SingleAttack`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_D.prefab`, `deployment_required_target_status_id=name-mark`, and `deployment_required_target_status_min_stacks=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header now includes `runtime_target_skill_ids`, `required_source_status_id`, `required_source_status_min_stacks`, `status_action_speed_bonus`, `status_attack_power_bonus`, `repeat_count_per_target`, `repeat_interval_seconds`, and `repeat_damage_multiplier`.
- The same choice CSV now marks `vega-c-trait-2`, `vega-c-trait-3`, `vega-c-trait-4`, `vega-c-trait-5`, `vega-c-master-1`, `vega-c-master-2`, `vega-d-trait-5`, and `vega-d-master-1` as shared-runtime-backed rows instead of the prior unsupported/partial state, and it remaps `vega-d-trait-4` to conditional `name-mark >= 10` damage instead of plain unconditional `1.3x`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate the new shared columns.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-30: Code Builder first implemented the shared runtime contract requested by the Vega handoff, then Skill Builder authored the active Vega C and Vega D rows on the new shared CSV fields and synced the runtime catalog.

## Task: 2026-05-28 Vega-B Follow-up Trigger Row Re-authored To LineAttack

### Task title

Re-author the active Vega-B master-1 delayed follow-up row from trigger `SingleAttack` to explicit trigger `LineAttack` so the CSV authority matches the intended aimed-slash runtime path.

### Goals

- Keep the active source CSV explicit about the follow-up slash runtime kind and trigger action.
- Preserve the existing authored payload, delay, prefab path, and linked silence effect.
- Keep source validation aligned with the new explicit trigger action path.

### Constraints

- Role Owner is Code Builder.
- Edited source authority is limited to `monster_skill_triger.csv` plus the shared CSV validator/runtime definitions needed to accept explicit trigger `LineAttack`.
- No new CSV file or new CSV column was added.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Future delayed aimed slashes should prefer explicit `trigger_action=LineAttack` when they are authored as direct trigger payloads, not as helper-skill re-casts.
- Keep `triggered_skill_id` non-empty on trigger rows because the current CSV parser still requires that field even when the direct trigger action does not use it at runtime.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `runtime_kind=LineAttack`, `trigger_action=LineAttack`, `base_damage=30`, `attack_power_coefficient=1.4`, `damage_multiplier=0.45`, `radius=1.8`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`, and `triggered_effect_id=vega-b-master1-second-silence`.
- The same trigger row still keeps a non-empty `triggered_skill_id=vega-b`, which matches the current parser contract in `PakuriCsvRuntimeData.MonsterDataset.cs`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now treats explicit trigger `LineAttack` rows like trigger `SingleAttack` rows for positive payload checks, which keeps source validation aligned with the shared direct trigger line path.
- Unity menu `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary after the row update.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-28: Base `vega-b` had already been returned to `LineAttack`, but the delayed master-1 follow-up row still remained on the old trigger `SingleAttack` authoring pattern until the user requested parity.

## Task: 2026-05-28 Vega-B Base Runtime Kind Reverted To LineAttack

### Task title

Re-author the active Vega-B base row as `LineAttack` after user-facing validation showed the `SingleAttack` path produced a self-centered slash presentation.

### Goals

- Keep the active source CSV aligned with the intended aimed-slash presentation.
- Reuse the current `LineAttack` data contract without adding a new column or helper row.
- Sync the runtime catalog after the row change.

### Constraints

- Role Owner is Code Builder.
- Edited source authority is limited to `monster_skills.csv`.
- No new CSV column or new CSV file was introduced.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- If future Vega-B work also needs the master-1 second slash to rotate as a beam, handle that as a separate trigger-path decision instead of assuming the base-row revert solves the follow-up trigger row too.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b` with `runtime_kind=LineAttack`, `radius=1.8`, `cooldown_seconds=8`, `active_duration_seconds=0`, `shot_interval_seconds=0`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- PowerShell CSV readback confirmed the current active row values for `vega-b` exactly as `runtime_kind=LineAttack`, `active_duration_seconds=0`, `shot_interval_seconds=0`, and empty `hit_target_count`.
- Unity menu `Pakuri/Validate CSV Source Data` completed after the row change, and the console logged the runtime catalog load summary instead of a CSV failure.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` completed and the console logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-28: The prior SingleAttack row had solved path-contact damage behavior but still presented as a self-cast slash because the shared SingleAttack prefab path does not rotate toward the target.

## Task: 2026-05-28 Trigger SingleAttack Fixed Payload Validation Alignment

### Task title

Align trigger `SingleAttack` source validation with the real runtime damage contract and correct the Vega-B follow-up trigger row.

### Goals

- Prevent false assumptions that `damage_multiplier` alone is enough for trigger-routed `SingleAttack` damage rows.
- Keep source validation aligned with runtime damage resolution, which accepts base damage or positive stat coefficients.
- Correct `vega-b-master1-second-slash` so it both validates and deals the intended nonzero damage.

### Constraints

- Role Owner is Code Builder.
- Edited source authority is limited to `monster_skill_triger.csv` and the shared CSV validator.
- No new CSV column or new CSV file was added.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Future trigger-routed `SingleAttack` rows should include explicit payload evidence in the handoff: `base_damage`, coefficients, `damage_multiplier`, and `damage_source`.
- Do not rely on `damage_multiplier` as an implicit source-skill damage reuse rule for trigger rows.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `base_damage=30`, `attack_power_coefficient=1.4`, and `damage_multiplier=0.45`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now validates `Fixed` trigger `SingleAttack` rows with the same positive payload rule already used by shared damage effect rows: positive `base_damage` or positive `attack/spell` coefficient.
- Unity menu `Pakuri/Validate CSV Source Data` completed after the fix, and the console returned the runtime catalog load summary instead of the previous Vega-B validation failure.

### History

- 2026-05-28: Vega-B master-1 follow-up was first authored with `damage_multiplier=0.45` but zero base/coefficient payload, which both failed source validation and would have resolved to zero runtime damage.

## Task: 2026-05-28 Vega-B Shared Trigger Status Data Authoring

### Task title

Author the active CSV rows required for Vega-B silence slash follow-ups on the shared triggered `SingleAttack` path.

### Goals

- Keep Vega-B fully authored in the active CSV source without a hidden follow-up skill slot.
- Reuse existing active CSV tables for the second slash trigger row and linked silence/name-mark effect rows.
- Keep master-2 silence extension authored through existing threshold and status-duration fields.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV authority stayed limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `status_effects.csv`.
- The shared runtime/common-logic extension was user-approved before implementation.
- No new CSV file or new CSV column was introduced.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- Reuse the same row pattern when a follow-up slash needs delayed trigger damage plus a linked OnHit status effect: trigger row in `monster_skill_triger.csv` plus a linked `Status` `OnHit` effect row in `monster_skill_effects.csv`.
- Keep `silence` default duration at `4s` unless another inspected skill now needs a different shared base.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b` with `hit_target_count=global`, `status_effect_id=silence`, `status_duration_seconds=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `vega-b-trait-2` through `status_duration_bonus_status_id=silence` / `status_duration_bonus=1`, and `vega-b-master-2` through `threshold_status_id=name-mark`, `threshold_status_min_stacks=10`, and `threshold_apply_status_id=silence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `vega-b-trait5-name-mark` and `vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `vega-b-master1-second-slash` with `runtime_kind=SingleAttack`, `trigger_action=SingleAttack`, `damage_multiplier=0.45`, `trigger_delay_seconds=0.4`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`, and `triggered_effect_id=vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now sets shared `silence` default duration to `4`, which lets the master-2 silence refresh land at `4s` and trait-2 plus master-2 combine to `5s` without a new status id.

### History

- 2026-05-28: The user first proposed reusing a separate helper skill row for Vega-B second slash, but current active-slot validation and learned-runtime loading made the shared trigger/effect row approach smaller and more aligned with existing active CSV authority.

## Task: 2026-05-28 Vega-A Projectile Shared Runtime Extension

### Task title

Add the active CSV schema and shared runtime support required to author Vega-A burst cadence, burst-index damage, and follow-up shadow projectiles.

### Goals

- Keep Vega-A authorable in the active CSV source without adding a Vega-only table.
- Extend the shared projectile path so burst-internal timing, per-burst-hit modifiers, and follow-up projectiles are data-driven.
- Keep master-2 authored on the shared trigger/effect path using the later user-provided slash coefficient and prefab path.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV authority stayed limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- The shared runtime/common-logic extension was user-approved before implementation.
- No new CSV file was introduced.
- The missing Vega-A master-2 slash value was later provided by the user as `attack coefficient 0.5`, so the active effect row could be completed without widening scope.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and Unity editor-validated.

### Next Actions

- Reuse `burst_interval_seconds`, `burst_damage_projectile_index`, and `burst_damage_multiplier` for future projectile-burst skills before adding another per-projectile schema.
- Reuse `follow_up_projectile_count`, `follow_up_projectile_delay_seconds`, and `follow_up_projectile_damage_multiplier` for future delayed shadow/follow-up projectile choices.
- Reuse the existing shared `Damage` effect row path when a triggered effect must deal damage and apply status together; a separate Vega-only hybrid effect type was not needed.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` header now includes `burst_interval_seconds`, `burst_damage_projectile_index`, and `burst_damage_multiplier`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header now includes `burst_damage_projectile_index`, `burst_damage_multiplier`, `follow_up_projectile_count`, `follow_up_projectile_delay_seconds`, and `follow_up_projectile_damage_multiplier`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now authors `vega-a-master2-transfer-mark` as `effect_kind=Damage`, `attack_power_coefficient=0.5`, `status_stack_amount=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` already applies `ResolveStatusSpec(...)` from `SkillMultiEffectKind.Damage`, so the same shared row can deal damage and apply `name-mark` without a new effect kind.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now treats positive `base_damage` or positive `attack/spell` coefficient as valid payload for shared `Damage` effect rows, fixing the false failure on coeff-only effect rows such as `vega-a-master2-transfer-mark`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` now parses those new columns from active skill and choice rows.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `.../Skills/Data/SkillData.cs`, `.../Skills/Data/SkillChoiceEffectSpec.cs`, `.../Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, and `.../Skills/Data/InGameSkillDefinitionMapper.cs` now carry the new data into runtime definitions and snapshots.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now resolves burst-internal cadence separately from outer cast cadence.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` and `.../Execution/Executors/ProjectileSkillExecutor.cs` now resolve shared burst-index damage rules and execute follow-up projectiles after the triggering burst hit.
- Unity refresh completed after the new CSV schema and rows, and the filtered Unity console returned no Vega CSV/runtime errors after correcting the `triggered_skill_id` contract on `vega-a-master2-kill-transfer` and later filling the master-2 slash payload.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the pre-existing `MSB3277` warnings remained.

### History

- 2026-05-28: Shared runtime work started after user approval to implement the three extension points first under Code Builder, then continue Vega-A under Skill Builder.
- 2026-05-28: User later completed the missing Vega-A master-2 authority with `attack coefficient 0.5` and `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`, so the existing shared triggered-effect data path was finalized without more code changes.
- 2026-05-28: Unity source validation exposed that coeff-only `Damage` effect rows were blocked by a stale `base_damage > 0` rule even though runtime damage already resolves from coefficients; Builder aligned the shared validator to the actual runtime contract and revalidated successfully.

## Task: 2026-05-28 Sein Passive Shared Runtime Data Completion

### Task title

Finish the remaining Sein passive data that depended on new shared passive-base and triggered-cast runtime support.

### Goals

- Author the shared-runtime-backed CSV rows for Sein-I base and Sein-G trait-3.
- Keep the active CSV authority aligned with the new shared runtime behavior without adding a new CSV file.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Edited data files are `monster_skill_choices.csv` and `monster_skill_triger.csv`; Unity sync updates the generated runtime catalog asset.
- Shared runtime code was extended, but no new CSV file was introduced.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and Unity editor-validated.

### Next Actions

- Reuse `PassiveBase` choice rows for future learned-passive base modifiers before adding a new passive-base schema.
- Reuse the triggered-cast origin marker path when a passive must react only to a triggered child skill cast.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains `sein-i-base-shot-interval` with `choice_group=PassiveBase`, `target_skill_id=sein-d`, and `shot_interval_multiplier=0.8`.
- The same choice CSV now marks `sein-g-trait-3` `RuntimeImplemented` and removes the prior blocker note.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `sein-g-auto-barrage-reload-trait3`, which reduces `sein-a` reload by `0.10` on `OnSkillCast` of `sein-b` gated by Sein-G origin.
- CSV field-width parsing succeeded after the new rows: choices `columns=89 lines=253`, trigger `columns=44 lines=43`.
- Unity validation and runtime catalog sync both succeeded after the shared runtime and data changes.

### History

- 2026-05-28: Added the shared-runtime-backed Sein-I base and Sein-G trait-3 CSV rows and validated them through the Unity editor.

## Task: 2026-05-27 Sein Passive CSV-Only Runtime Data Authoring

### Task title

Author and sync the existing-runtime CSV data required for the CSV-solvable portion of Sein passives F, H, I, and J.

### Goals

- Add the status-effect and trigger data already supported by current runtime paths.
- Record active-skill choice routing needed for choice-gated Sein passive effects.
- Keep shared-runtime-only behavior out of this data pass.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Edited source authority is limited to `monster_skill_effects.csv`, `monster_skill_choices.csv`, and `monster_skill_triger.csv`.
- Unity sync writes the generated runtime catalog asset; no new CSV schema or shared runtime logic is added here.
- Excluded behavior is `sein-i` base tick-speed `+20%` and exact `sein-g-trait-3` auto-trigger source identification.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and Unity editor-validated for the routed CSV-only data.

### Next Actions

- After the approved shared runtime work exists, add only the data required for `sein-i` base tick speed and `sein-g-trait-3`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains 12 new Sein passive status effect rows for F/H/I/J using existing `passive-buff`, `fire-resist-down`, and `fire-exposure` runtime kinds.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks the authored F/H/I/J trait rows `RuntimeImplemented` and supplies target active skill routing where active snapshots require it.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains 12 Sein-J `OnKill` action rows using existing `CooldownRefund` and `ReloadReduce` behavior.
- CSV field-width parsing succeeded after edits: effects `columns=66 lines=110`, trigger `columns=44 lines=42`, choices `columns=89 lines=252`.
- Unity `Pakuri/Validate CSV Source Data` loaded the runtime catalog successfully, and `Pakuri/Sync CSV Runtime Catalog Assets` reported successful synchronization to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-27: Added and validated the CSV-only Sein passive F/H/I/J data pass; left the two shared-runtime behaviors excluded by scope.

## Task: 2026-05-27 Zero-Damage Persistent Zone CSV Validation Rule

### Task title

Adjust active CSV validation rules so status-only persistent `monster_skill_effects.csv` damage rows can remain zero-damage.

### Goals

- Keep active effect CSV authoring free of fake `base_damage` values for presence-only persistent zones.
- Preserve positive-damage requirements for normal damage rows.
- Validate the new rule through the actual Unity CSV validation menu.

### Constraints

- Role Owner is Code Builder.
- The change is a shared validation-rule adjustment, not a new CSV schema.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Future effect rows that are authored as zero-damage persistent status zones should match the shared rule exactly: persistent timing, status payload, and zero coefficients.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now exempts only status-only persistent zones from the unconditional positive-`base_damage` rule for `SkillMultiEffectKind.Damage`.
- Active CSV evidence remains `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` rows `sein-d-zone-presence` and `sein-e-master2-zone-presence`, both authored with `base_damage=0`.
- Unity menu `Pakuri/Validate CSV Source Data` succeeded after the fix and logged the runtime catalog load summary instead of the earlier validation failure.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 code errors; only the existing `MSB3277` warnings remained.

### History

- 2026-05-27: Sein-E / Sein-D presence zones exposed that the active validation rule was too strict for zero-damage persistent status-refresh rows.

## Task: 2026-05-27 Sein-C/D Delayed Projectile And Residual Zone CSV Authoring

### Task title

Extend the active skill/effect/status CSV authority required for Sein-C delayed projectile behavior and Sein-D residual zone behavior.

### Goals

- Keep Sein-C delayed impact, projectile delay tuning, and follow-up effects authored in the active CSV files.
- Keep Sein-D residual ember zone authored in the active effect CSV instead of a helper skill row.
- Keep new schema additions reusable for future skills.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- User explicitly approved widening scope to shared runtime/common-logic extension and new CSV columns when required.
- `monster_skill_choices.csv damage_delay_multiplier` and `monster_skill_effects.csv active_duration_seconds / tick_interval_seconds` are now part of the active authoring authority for this runtime path.
- Some effect values remain explicit inferences until a stronger authority is provided:
  - `sein-c-master-1` residual zone radius `1.2`, tick `0.5s`
  - `sein-d-master-2` residual zone radius `3.2`, tick `0.5s`
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- Reuse `damage_delay_multiplier` for future projectile delay tuning before adding another choice field.
- Reuse `active_duration_seconds` and `tick_interval_seconds` in effect rows for future persistent follow-up zones before creating helper active-skill rows.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header now includes `damage_delay_multiplier`; `sein-c-trait-4` uses `0.6`.
- The same choice CSV now authors Sein-C trait/master and Sein-D trait/master rows on shared fields, including conditional status damage for `sein-c-trait-5` and `sein-d-trait-5`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header now includes `active_duration_seconds` and `tick_interval_seconds`.
- The same effect CSV now contains `sein-c-master2-contact`, `sein-c-master1-zone`, and `sein-d-master2-zone`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `sein-c` as `CooldownProjectile` with `damage_delay_seconds=0.8` and authors `sein-d` with active duration, tick interval, and status payload values used by the shared runtime.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains the shared Sein status rows required by those choices.
- Unity menu execution for `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` produced filtered console logs `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-05-27: Sein-C and Sein-D required active CSV authoring support for projectile-delay tuning and effect-authored residual zones; the user approved the necessary schema widening.

## Task: 2026-05-26 Rin-E SingleAttack Core Hitbox CSV Schema

### Task title

Extend active skill CSV authority for SingleAttack prefab core-hitbox effects and Rin-E authoring.

### Goals

- Add a base active-skill prefab path column so active skill rows can provide `SkillEffectPrefab`.
- Add shared choice columns for prefab core-hitbox damage, core-hitbox additional damage, and hit-count cooldown refund.
- Author Rin-E enhancement and master rows as `RuntimeImplemented`.
- Add Rin-E master-2 slow as a choice-gated OnHit status row in `monster_skill_effects.csv`.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- CSV source remains the active authority; no Rin-only companion table was added.
- CSV files were exported as UTF-8.
- Unity CSV runtime catalog sync is pending because batchmode reported another Unity instance has this project open.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and synced through the open Unity Editor menu after the follow-up CSV validation fix.

### Next Actions

- Reuse `core_hitbox_name`, `core_damage_multiplier`, `core_on_hit_additional_damage_*`, and `hit_count_cooldown_refund_*` for future SingleAttack prefab-center effects before adding another schema.
- User verifies Rin-E master 2 slow behavior in Play Mode.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has 57 columns and `rin-e.skill_effect_prefab_path=Assets/Prefab/Skill/Rin/Rin_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now has 86 columns including the shared core-hitbox and hit-count cooldown refund fields.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now has 77 parsed rows and contains `rin-e-master2-slow`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `PakuriCsvRuntimeData.AssetReferences.cs` now parse, map, and collect the base `skill_effect_prefab_path` and new choice fields.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `SkillChoiceEffectSpec.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSnapshot.cs` now carry the new shared choice fields into runtime snapshots.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed only because Unity batchmode reported another Unity instance has `C:/TowerDefence_Pakuri/Test/Pakuri` open.
- Follow-up enum validation found the `DamageAttribute` enum defines `Darkness`, not `Dark`; `monster_skill_choices.csv` and `monster_skill_effects.csv` Rin-E rows were corrected to `Darkness`, and a CSV enum scan returned `ENUM_VALIDATION_OK`.
- Follow-up status-scope validation found `StatusEffectRuntime.TryParseStatusTargetScope(...)` only accepts `self` and `all_allies`; `rin-e-master2-slow` now leaves `status_target_scope` blank like other enemy OnHit status rows, while `target_side=Enemy` remains the target authority.
- `.NET TextFieldParser` scans returned `FIELD_COUNT_OK` for `monster_skill_effects.csv` 61 columns / 78 lines, `monster_skill_choices.csv` 86 columns / 252 lines, `monster_skills.csv` 57 columns / 52 lines, and `monster_skill_triger.csv` 34 columns / 10 lines.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the fix.

### History

- 2026-05-26: User requested full Rin-E Skill Builder implementation with the SingleAttack blueprint and `Assets/Prefab/Skill/Rin/Rin_E.prefab`.
- 2026-05-26: User reported Unity auto-sync failing on `monster_skill_effects.csv` row 78 because `attribute=Dark` was not a valid enum value; Builder corrected the CSV enum values and checked for remaining enum mismatches.
- 2026-05-26: User reported Unity CSV validation still failing on `rin-e-master2-slow status_target_scope=enemy`; Builder cleared that unsupported scope, verified the relevant CSV schemas and enum/status-scope scans, and synced the runtime catalog through the open Unity Editor menu.

## Task: 2026-05-26 SingleAttack Damage Delay CSV Schema

### Task title

Add `damage_delay_seconds` to active monster skill CSV and carry it into SingleAttack runtime data.

### Goals

- Let `Pakuri/Assets/CSVdata/source/monster_skills.csv` author per-skill SingleAttack hit delay.
- Default every existing monster skill row to `0` so current immediate-hit behavior remains unchanged until rows are tuned.
- Carry the field through `SkillRow`, `SkillDefinition`, `SingleAttackData`, validation, and mapper code.

### Constraints

- Role Owner is Code Builder.
- CSV source remains the active authority; no companion table was added.
- Existing row count and quoted CSV structure must remain parseable.
- Unity batchmode catalog sync could not complete while another Unity instance had the same project open.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Runtime catalog asset sync is pending through the open Unity Editor menu or a later batch sync after closing Unity.

### Next Actions

- Tune `damage_delay_seconds` values in `monster_skills.csv` for specific SingleAttack rows.
- Sync runtime catalog assets once Unity project locking allows it.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `damage_delay_seconds` after `knockback_distance`; every existing data row is `0`.
- CSV parser verification returned `records=52`, `fields=56 records=52`, `damage_delay_index=50`, `type=float`, and `nonzero_defaults=0`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` parses optional `damage_delay_seconds`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `DamageDelaySeconds` into `SkillDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Skills/Data/SingleAttackData.cs`, `Skills/Data/InGameSkillDefinitionMapper.cs`, and `Skills/Data/InGameSkillDataValidator.cs` now carry and validate the value.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed with Unity's duplicate-project-open guard for `C:/TowerDefence_Pakuri/Test/Pakuri`.

### History

- 2026-05-26: User requested Code Builder implementation of Designer's N-second delayed SingleAttack hit timing plan with default CSV value `0`.

## Task: 2026-05-26 Rin-B/C Shared Beam Buff And Status CSV/Runtime Extension

### Task title

Extend the shared CSV/runtime contracts required to finish Rin-B and Rin-C on the active Scripts2 skill path.

### Goals

- Add shared beam knockback and per-hit reload-reduction choice data for Rin-C.
- Add shared effect/status payload fields for Rin-B master-2 style outgoing additional damage without passive-trigger ownership hacks.
- Keep Rin-B trait/master extra buffs and Rin-C master slow authored in the active CSV tables.

### Constraints

- Role Owner is Skill Builder.
- User explicitly approved current Rin CSV/reference files as the parsed source for this task.
- No Rin-only companion CSV table was added; the work stays inside `monster_skills.csv`, `monster_skill_choices.csv`, and `monster_skill_effects.csv`.
- CSV/runtime claims are grounded in inspected source rows and runtime mapper/executor code.

### Role Owner

Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- Reuse `knockback_distance`, `knockback_distance_multiplier`, `reload_reduce_target_skill_id`, and `reload_reduce_seconds_per_hit` for future beam/line skills before adding another schema.
- Reuse `status_outgoing_additional_damage_*` for future buff/status-authored extra-hit behavior before adding a trigger-only side table.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now includes `knockback_distance`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now includes `knockback_distance_multiplier`, `reload_reduce_target_skill_id`, and `reload_reduce_seconds_per_hit`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now includes `status_outgoing_additional_damage_multiplier`, `status_outgoing_additional_damage_trigger_attribute`, and `status_outgoing_additional_damage_attribute`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.StatusPayload.cs`, and `PakuriCsvRuntimeData.Build.cs` now parse and map those new columns.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Skills/Data/BeamSkillData.cs`, `Skills/Data/SkillChoiceEffectSpec.cs`, `Skills/Execution/Modifiers/SkillChoiceModifierRecord.cs`, and `Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carry the new shared Rin-B/C data through runtime snapshots.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` and `StatusEffectRuntime.cs` now carry status-authored outgoing additional damage fields keyed by `DamageAttribute`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.skill_id -in @('rin-b','rin-c') }` returned all Rin-B/C choice rows with `runtime_support_state=RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after the schema/runtime changes; existing MSB3277 warnings remain.

### History

- 2026-05-26: User approved the wider Rin CSV/reference inspection exception required by the Skill Builder boundary and requested full Rin-C then Rin-B implementation.

## Task: 2026-05-24 Skill On-Hit Additional Damage CSV Schema

### Task title

Add shared choice CSV fields for direct on-hit extra damage and every-nth-hit chain damage.

### Goals

- Keep on-hit extra damage authored in `monster_skill_choices.csv`.
- Keep Rin-A master-2 off the projectile `branch_*` launch override fields.
- Carry the new CSV fields through runtime source rows, `SkillChoiceDefinition`, `SkillChoiceEffectSpec`, and `SkillExecutionSnapshot`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- User provided the parsed Rin-A master-2 values in the request.
- CSV source stayed UTF-8 and imported successfully through Unity.
- No new companion CSV table was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and synced into runtime catalog assets.

### Next Actions

- Future skills needing direct hit-target extra damage should reuse `on_hit_additional_damage_*`.
- Future skills needing deterministic nth-hit nearby chain damage should reuse `on_hit_chain_*`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now includes `on_hit_additional_damage_chance`, `on_hit_additional_damage_multiplier`, `on_hit_additional_damage_attribute`, `on_hit_additional_damage_target`, `on_hit_chain_hit_period`, `on_hit_chain_target_count`, `on_hit_chain_search_radius`, `on_hit_chain_damage_multiplier`, `on_hit_chain_damage_attribute`, and `on_hit_additional_damage_visual`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` showed `rin-a-master-2` with `on_hit_additional_damage_chance=1`, `on_hit_additional_damage_multiplier=0.4`, `on_hit_chain_hit_period=3`, `on_hit_chain_target_count=2`, and blank branch chance/count/launch fields.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` parses the new optional columns.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, `SkillDefinition.cs`, `SkillChoiceEffectSpec.cs`, `SkillChoiceModifierRecord.cs`, and `SkillExecutionSnapshot.cs` carry the new fields into runtime choice snapshots.
- Unity-MCP editor execution returned `rin-a-master-2|extra=True:1:0.4:Lightning:HitTarget|chain=3:2:4.5:0.4:Lightning|branch=False:False:0:False`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged a successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-24: User requested the additional damage behavior as a common skill on-hit option rather than a projectile-only branch extension.

## Task: 2026-05-24 Rin-A Choice CSV Authoring

### Task title

Author Rin-A remaining choice behavior on the active `monster_skill_choices.csv` runtime authority.

### Goals

- Add reusable nth-projectile-launch branch override columns to the active choice CSV.
- Move Rin-A trait 5 from unsupported critical prose to shared critical bonus fields.
- Move Rin-A master 2 from unsupported prose to shared branch fields plus launch-period override fields.
- Preserve Rin-A master 1 on the already-supported damage, magazine, and shot-interval fields.

### Constraints

- Role Owner is Skill Builder.
- User explicitly approved current CSV/code as the parsed source.
- No new monster-specific companion table was added.
- CSV stayed UTF-8 and all rows now have the same 59-column shape.

### Role Owner

Skill Builder

### Status

Implemented and synced into runtime catalog assets.

### Next Actions

- Reuse `branch_launch_period` and `branch_launch_chance_set` for future projectile skills that need "every Nth projectile launch" branch chance overrides.
- Keep future critical projectile choices on `crit_chance_bonus` and `crit_damage_bonus` before adding new critical schema.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` header/type rows now include `branch_launch_period` and `branch_launch_chance_set`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` showed `rin-a-trait-5` as `crit_chance_bonus=0.1`, `crit_damage_bonus=0.25`, and `RuntimeImplemented`.
- The same import showed `rin-a-master-2` as `branch_chance_set=0.4`, `branch_count=2`, `branch_damage_multiplier=0.4`, `branch_search_radius=4.5`, `branch_launch_period=3`, `branch_launch_chance_set=1`, and `RuntimeImplemented`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged a successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-24: User requested Skill Builder implementation for Rin-A master-2, remaining enhancements, and master-1 using current CSV/code as parsed source.

## Task: 2026-05-24 Eve F-J Passive Effect/Trigger CSV Schema And Authoring

### Task title

Extend shared passive effect/trigger CSV data so Eve F-J can stay fully data-authored on the current runtime catalog path.

### Goals

- Add shared effect columns for target-status-conditional status chance and status-id-specific applied-duration bonuses.
- Add shared trigger columns for condition status, attribute gating, proc chance, and internal cooldown.
- Re-author Eve F-J passive rows so the remaining `DataOnlyUnsupported` / `ReferenceDirect` Eve passive rows move onto shared runtime support.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Current CSV files were explicitly treated as the parsed source for this task.
- No new Eve-only CSV file was added; the work stayed inside `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `monster_skill_choices.csv`.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and Unity CSV validation passed.

### Next Actions

- Reuse `status_conditional_target_status_id` plus `status_conditional_status_chance_bonus` for future passive rows that say "extra status chance only against targets already carrying X".
- Reuse `status_applied_status_duration_bonus_status_id` plus `status_applied_status_duration_bonus` for future rows that extend only one applied status without editing global status defaults.
- Reuse `condition_status_id`, `trigger_attribute`, `proc_chance`, and `internal_cooldown_seconds` before adding another trigger companion table.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header/type rows now include `status_conditional_target_status_id`, `status_conditional_status_chance_bonus`, `status_applied_status_duration_bonus_status_id`, and `status_applied_status_duration_bonus`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` header/type rows now include `condition_status_id`, `trigger_attribute`, `proc_chance`, and `internal_cooldown_seconds`.
- Eve F-J rows in `monster_skill_choices.csv` are now all `RuntimeImplemented`; `eve-g-trait-3`, `eve-i-trait-3`, and `eve-j-trait-3` target the active skills they modify instead of staying passive-note-only.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.StatusPayload.cs`, `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate the new effect/trigger columns.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the schema and row-authoring change; only the existing `MSB3277` warnings remained.
- Unity `Pakuri/Validate CSV Source Data` succeeded after the follow-up validation fix, which confirmed the new headers, rows, and shared trigger semantics were accepted by the runtime catalog loader.

### History

- 2026-05-24: User asked Skill Builder to resume the interrupted Eve F-J passive implementation, which required shared passive effect and trigger schema expansion plus Eve row authoring.

## Task: 2026-05-18 Active Runtime CSV Authority

### Task title

Keep the current Scripts2 runtime CSV authority explicit and compact.

### Goals

- Keep active runtime authority on `Assets/CSVdata/source/*.csv` plus `Assets/CSVdata/EnemySkillData.csv`, with monster choice runtime data unified into `monster_skill_choices.csv` and `monster_modifier_skill_choice.csv`.
- Keep reward IDs, runtime choice IDs, and stage/enemy/monster CSV responsibilities separated.
- Keep base monster/enemy skill visual prefab authority out of active skill CSV rows now that `EffectManager` owns those scene mappings.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed intermediate migration steps remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active CSV authority summarized and retained for future work. 2026-05-18 Code Builder moved monster projectile/status tuning out of `monsters.csv` and into per-skill rows in `monster_skills.csv`. 2026-05-18 Code Builder added a one-command CSV runtime sync batch path and status-column validation/fallback for supported status labels. 2026-05-19 Code Builder superseded the old reward/modifier split by unifying monster choice runtime data into `monster_skill_choices.csv` plus the slim `monster_modifier_skill_choice.csv` gate file.

### Next Actions

- If future cleanup resumes, continue from this active runtime-authority split instead of reviving archived duplicate CSV tables.
- When CSV ownership changes, update this file together with `boards/RUN/RUN_BLACKBOARD.md` and `boards/COMBAT/ENEMY_BLACKBOARD.md`.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` carries the active enemy authored rows, including the current `basic_skill` plus `stage_one_skill` split.
- `Pakuri/Assets/CSVdata/EnemySkillData.csv` carries active enemy skill tuning rows.
- `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv` now carries the active monster choice gate rows, while `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now carries the unified choice display plus runtime modifier rows.
- `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` and `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` were deleted in the 2026-05-19 unification pass because active Scripts2 runtime code no longer reads them.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now keeps rows such as `rin-a-trait-5`, `rin-a-master-2`, and `ariel-a-master-1` explicitly marked `DataOnlyUnsupported` when current Scripts2 runtime still lacks the required special-case logic.
- After the 2026-05-26 execute-related choice-schema extension, `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` was normalized so all active rows now match the 78-column header again; post-fix field-count scans returned `UTF8_ALL_ROWS_OK` and `ALL_ROWS_OK_AFTER_BOM`, and the file was rewritten as UTF-8 BOM for cross-tool readability.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.*` files remain the active runtime load/build/validation path.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` plus `Assets/Scenes/NewScene/NewRunScene.unity` now own base monster/enemy skill effect prefab authority instead of `monster_skills.csv` / `EnemySkillData.csv`.
- `Pakuri/reference/Archive/InactiveRootCsv/` now stores archived inactive root CSV files that are no longer part of the active runtime path.
- `Pakuri/Assets/CSVdata/source/monsters.csv` no longer contains monster-level `projectile_speed`, `magazine_capacity`, `reload_duration`, `shot_interval`, `status_effect_label`, unit/projectile color, unit/projectile sprite path, projectile lifetime, or projectile hit radius columns.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now owns per-skill `projectile_speed`, `pierce_count`, `status_chance`, and `status_effect_label`; its deleted `range` column is no longer read by `PakuriCsvRuntimeData.MonsterDataset.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now maps projectile speed, base pierce, and status chance from `SkillDefinition` instead of hardcoded Ariel-A/Eve-A branches.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now falls back from blank `status_effect_id` to a parseable `status_effect_label`, so supported labels such as `媛먯쟾`, `?뷀솕`, `異붿쐞`, `鍮숆껐`, `痍⑥빟`, and `諛⑹뼱留? can resolve through `StatusEffectKind`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now fails validation when `status_chance > 0` points at an unsupported runtime status label/id.
- `SyncCsvRuntimeCatalogs.bat` calls Unity batchmode with `-executeMethod Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor`; when the project was already open in Unity, batchmode correctly failed with Unity's duplicate-project-open guard, and the same method was then invoked through Unity-MCP.
- Unity console after the MCP invocation logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced and validated from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` also logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the 2026-05-26 `monster_skill_choices.csv` row-width normalization follow-up.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` now shows only Eve's supported runtime statuses with positive `status_chance`: `eve-a shock 0.15`, `eve-b slow 0.2`, `eve-c chill 1`, `eve-d shock 1`, and `eve-e vulnerable 1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain. A first parallel runtime/editor build hit only an `obj\Debug\Assembly-CSharp.dll` file lock, then runtime passed when rerun alone.

### History

- 2026-05-18: EffectManager scene authority, reward/choice separation, enemy dual-skill CSV authority, and inactive root CSV archiving were recorded as the current active data baseline.
- 2026-05-18: Code Builder consolidated monster projectile/status tuning into `monster_skills.csv`, removed duplicate/visual projectile columns from `monsters.csv`, and removed Ariel-A/Eve-A hardcoded projectile/status values from the shared mapper/executor path.
- 2026-05-18: Code Builder added `SyncCsvRuntimeCatalogs.bat`, exposed `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` for Unity batchmode, normalized unsupported design-only monster status labels to `status_chance=0`, and verified sync/validation through the open Unity Editor.
- 2026-05-19: Code Builder first added shared-projectile-compatible `rin-a` modifier coverage, then unified monster choice runtime data into `monster_skill_choices.csv` / `monster_modifier_skill_choice.csv` and kept crit-only / every-third-hit chain behavior explicitly unsupported where current Scripts2 runtime still has no matching contract.
- 2026-05-26: Follow-up maintenance after the Rin-D execute schema extension normalized legacy `monster_skill_choices.csv` rows to the 78-column header, rewrote the file as UTF-8 BOM, and re-synced the runtime catalog without CSV fatal errors.

## Task: 2026-05-26 Rin F-J Passive CSV Trigger/Effect Schema

### Task title

Extend active monster skill CSV schema for reusable trigger actions, count gates, and conditional passive effects.

### Goals

- Add reusable CSV columns for delayed trigger actions, event skill filtering, event source scope, count gates, effect triggers, cooldown refunds, reload reduction, and status-source conditions.
- Add reusable effect columns for health-ratio conditions, hit-count conditions, and critical-damage status bonuses.
- Keep Rin F-J passive authoring in the active `Assets/CSVdata/source` CSV authority path.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Active CSV scope stayed limited to routed Rin skill-authoring files: `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- No new CSV file was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and validation passed.

### Next Actions

- Reuse `trigger_action`, `event_skill_id`, `target_skill_id`, `triggered_effect_id`, `trigger_delay_seconds`, `trigger_every_count`, and `event_source_scope` for future passive trigger work before adding another trigger table.
- Reuse `condition_health_ratio_max`, `condition_hit_count_min`, and `status_critical_damage_bonus` for future passive effects before adding specialized columns.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` header/type rows now include `trigger_action`, `event_skill_id`, `target_skill_id`, `triggered_effect_id`, `condition_status_source_skill_id`, `trigger_delay_seconds`, `trigger_every_count`, `event_source_scope`, `cooldown_refund_ratio`, and `reload_reduce_ratio`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header/type rows now include `condition_health_ratio_max`, `condition_hit_count_min`, and `status_critical_damage_bonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.StatusPayload.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate the new CSV fields.
- CSV field-count scan passed after authoring: `monster_skill_effects.csv` 64 columns / 91 lines, `monster_skill_triger.csv` 44 columns / 26 lines, `monster_skill_choices.csv` 86 columns / 252 lines, and `monster_skills.csv` 57 columns / 52 lines.
- Unity `Pakuri/Validate CSV Source Data` completed with the runtime catalog load summary and no Pakuri CSV validation failure.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-26: Rin F-J passive implementation required reusable trigger/action/count/effect schema instead of one-off runtime branches, and the user approved that extension.

## Task: 2026-05-29 Damage Meter Monster Icon CSV Handoff

### Task title

Prepare the CSV/data portion of the damage meter UI handoff.

### Goals

- Add `MonsterIconImage` to `Pakuri/Assets/CSVdata/source/monsters.csv` during Code Builder implementation.
- Route the new sprite path through the existing runtime CSV asset catalog path.
- Keep blank icon values non-fatal.

### Constraints

- Role Owner is Code Builder.
- Designer created the handoff only; no CSV or code changes were performed.
- Active CSV authority remains `Pakuri/Assets/CSVdata/source/*.csv` plus `PakuriCsvRuntimeData.*`.

### Role Owner

Code Builder

### Status

Implemented by Code Builder.

### Next Actions

- Fill `MonsterIconImage` asset paths later when final monster representative sprites are selected.
- User verifies in Play Mode that blank icon values hide the panel image without blocking the meter.

### Evidence

- `Pakuri/Assets/CSVdata/source/monsters.csv` inspected header currently has no `MonsterIconImage` column.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/MonsterDefinition.cs` currently has `DisplayName`, `UnitSprite`, and `ProjectileSprite`, but no dedicated monster icon field.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` already maps `monsters.csv display_name` into `MonsterDefinition.DisplayName` and uses `LoadSprite(...)` for existing sprite-backed CSV paths.
- `Pakuri/Assets/CSVdata/source/monsters.csv` now has `MonsterIconImage` plus `asset_path` type entry; all current monster rows keep the value blank.
- PowerShell CSV field-count check returned `header=24 rows=6 bad=`, confirming the edited `monsters.csv` row shape.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/MonsterDefinition.cs` now exposes `Sprite MonsterIconImage`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.AssetReferences.cs`, and `PakuriCsvRuntimeData.Build.cs` parse, collect, and map `MonsterIconImage`.
- Unity menu `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-29: User requested a Code Builder handoff that includes monster icon data ownership for the damage meter UI.
- 2026-05-29: Code Builder added the blank-safe `MonsterIconImage` CSV/catalog path for damage meter panel icons.

## Task: 2026-07-12 Rin Runtime Visual Data Feasibility

### Task title

Define the active CSV boundary for Rin prefab-to-runtime visual migration.

### Goals

- Reuse existing base runtime visual columns for Rin A-C while keeping Rin D base prefab-backed.
- Expose existing parsed Trigger runtime visual fields on the passive Trigger CSV for Rin F.
- Extend the shared runtime hitbox spec and single-attack Trigger schema with optional offset fields for Rin D master 1.
- Preserve prefab-owned data that the current runtime visual schema cannot represent.

### Constraints

- Role Owner is Code Builder for the approved implementation phase.
- Prefab deletion and scene fallback cleanup are not included.
- Non-zero collider offsets and named child colliders cannot be discarded during parity migration.

### Role Owner

Code Builder

### Status

CSV/runtime implementation and source/build validation completed; user Play Mode parity remains.

### Next Actions

- User verifies runtime visual/hitbox parity for Rin A/B/C/F and D master 1.
- User verifies Rin E hits every overlapping prefab collider target and preserves named `CoreHitBox` effects.
- Remove converted fallback prefab references only after parity confirmation.

### Evidence

- Projectile, buff, line-attack, and single-attack base CSV headers already expose runtime sprite/controller/scale/sorting/hitbox columns.
- `PakuriCsvRuntimeData.MonsterDataset.cs` already reads optional Trigger runtime visual fields, and `SkillTriggerRuntime` already consumes them.
- `passive_skill_triger.csv` does not currently include those runtime visual columns.
- `single_attack_skill_triger.csv` also lacks runtime visual fields, and `RuntimeSkillHitboxSpec` currently stores only `Size`; `RuntimeSkillVisualFactory` currently forces collider offset to zero.
- Rin D master 1 requires runtime box size `(3.9373517, 3.788869)` and offset `(0.53632426, -0.41973162)` to preserve current Trigger hitbox behavior.
- Active `rin-e` has radius `2.4` and blank `hit_target_count`; `InGameSkillDefinitionMapper` consequently leaves `UsePrefabHitbox=false`.
- `SingleAttackSkillExecutor.ResolveCoreHitboxColliders(...)` is called only inside the `UsePrefabHitbox` branch, so active data does not currently reach named `CoreHitBox` resolution.
- `RuntimeSkillHitboxSpec` now stores `Offset`; both skill and Trigger readers/builders accept optional `runtime_hitbox_offset_x/y`.
- `single_attack_skill_triger.csv` and `passive_skill_triger.csv` expose runtime visual/hitbox columns; D master 1 and two F follow-up rows carry the approved values.
- `skills_single_attack.csv` exposes `use_prefab_hitbox`; only Rin E sets it to `true` in this task.
- Explicit prefab-hitbox skills without an authored target limit now resolve `int.MaxValue` overlapping targets, avoiding Rin E's previous one-target cap while keeping `HitAllTargets=false` target-centered placement.
- TextFieldParser checks found no row/header count mismatch in all six edited CSVs. Unity-MCP source validation completed without errors.

### History

- 2026-07-12: Designer recorded the CSV exposure boundary and Rin-E pre-migration blocker while validating Rin runtime visual feasibility.
- 2026-07-13: User kept Rin D base prefab-backed and approved D master 1 runtime conversion; Designer added optional hitbox-offset and single-attack Trigger schema work to the handoff.
- 2026-07-13: Code Builder implemented runtime visual data, optional offset parsing/building, D master 1/F Trigger fields, and Rin-E explicit prefab-hitbox routing.

## Task: 2026-07-13 Sein Runtime Visual CSV Contract

### Task title

Author Sein runtime visual data and add a separate optional projectile impact visual contract.

### Goals

- Author existing runtime sprite/controller/scale/sorting/hitbox fields for Sein A-E and relevant Trigger/choice effects.
- Add optional impact sprite/controller/scale/sorting fields for delayed projectile impacts without reusing the flying projectile visual.
- Keep Sein runtime object and collider offsets at zero without adding Sein offset columns or graph params.

### Constraints

- Role Owner is Code Builder.
- Active data authority is `Pakuri/Assets/CSVdata/runtime`.
- Existing prefab paths stay as fallback pending Play Mode parity.

### Role Owner

Code Builder

### Status

Implemented, synchronized, and validated.

### Next Actions

- User performs Play Mode parity checks before fallback-path cleanup.
- Reuse the optional impact visual fields only for projectile skills that require a distinct impact presentation.

### Evidence

- `skills_projectile.csv` adds only `runtime_impact_visual_sprite_path`, `runtime_impact_visual_animator_controller_path`, `runtime_impact_visual_scale`, and `runtime_impact_visual_sorting_order`; no Sein offset fields were added.
- `PakuriCsvRuntimeData.MonsterDataset.cs`, `.Build.cs`, and `.AssetReferences.cs` parse, build, and catalog those optional impact assets.
- Sein A/B/C/D/E base rows and four choice graph visual targets plus A master-2 Trigger carry the inspected sprite/controller/scale/hitbox sizes.
- TextFieldParser reported no row/header mismatch across all 7 edited CSV files.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync from `Assets/CSVdata/runtime`; post-sync `Pakuri/Validate CSV Source Data` loaded the runtime catalog with no error entries.

### History

- 2026-07-13: User rejected offset authoring for Sein because both object and collider offsets are fixed at `(0,0)`.
- 2026-07-13: Code Builder added the distinct impact visual contract and authored Sein runtime visual rows/nodes.

## Task: 2026-07-14 Vega Runtime Visual CSV Design

### Task title

Define the active CSV and graph changes for Vega prefab-to-runtime skill visuals.

### Goals

- Populate existing base runtime visual fields for Vega A-E.
- Reuse Vega B base runtime visual for B master 1 without changing the line-attack Trigger CSV schema.
- Replace Vega A master 2 `EffectVisual` with a static, hitbox-free `RuntimeEffectVisual`.

### Constraints

- Role Owner is Designer.
- CSV and C# implementation was later completed by Code Builder.
- No offset fields are required: A/D gameplay colliders are centered, and B's non-zero prefab collider offset is not part of its mathematical line-hit authority.

### Role Owner

Designer

### Status

Implemented and validated by the related Code Builder task below.

### Next Actions

- User verifies runtime visual and collider parity in Play Mode.

### Evidence

- The five active Vega base rows already expose base runtime visual fields but leave them blank.
- `line_attack_skill_triger.csv` remains unchanged at 37 columns; `SkillTriggerRuntime` now resolves B master 1 visual from base `vega-b`.
- `RuntimeEffectVisual` currently requires both sprite and controller in `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs:490-502` and `skill_node_definition_params.csv:57-62`; `Vega_A_Master_2.prefab` has no Animator.
- Exact values and behavior constraints are recorded in `boards/MON/VEGA_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`.

### History

- 2026-07-14: Designer isolated the two authoring gaps and avoided adding unused offset or Vega-specific fields.

## Task: 2026-07-14 Vega Runtime Visual CSV Implementation

### Task title

Author Vega runtime visuals without new offset or Trigger columns.

### Goals

- Move Vega A-E and A master 2 visual authority into active runtime CSV/graph data.
- Remove B master 1 prefab authority while reusing base B runtime visual.
- Preserve centered runtime colliders and existing gameplay values.

### Constraints

- Role Owner is Code Builder.
- No CSV columns were added; `line_attack_skill_triger.csv` remains 37 columns.
- Object and collider offsets are fixed to `(0,0)` by existing default materialization.

### Role Owner

Code Builder

### Status

Implemented, synchronized, and validated.

### Next Actions

- User verifies visual and hitbox parity in Play Mode.

### Evidence

- Vega A-E base prefab paths, B master 1 prefab path, and A master 2 `EffectVisual` prefab reference were removed from active runtime CSV/graph files.
- Vega A/D contain runtime hitbox sizes; B uses shared runtime line query; A master 2 and E remain hitbox-free.
- `RuntimeEffectVisual` animator controller is optional in code schema and `skill_node_definition_params.csv`.
- Seven edited CSV files have zero header/row field-count mismatches.
- Active runtime skill CSV search has zero `Assets/Prefab/Skill/Vega` references.

### History

- 2026-07-14: Code Builder implemented the user's no-new-column, zero-offset data boundary.

## Task: Runtime skill prefab dependency decommission

### Goals

- Keep runtime skill numeric/visual authority in runtime CSV and graph nodes while deleting migrated skill prefabs.

### Constraints

- No new CSV columns; runtime object and collider offsets remain `(0, 0)`; retain Rin-D and Rin-E prefab exceptions.

### Role Owner

- Code Builder

### Status

- Code complete; user Play Mode verification pending.

### Next Actions

- User verifies base, enhancement, and master skill visuals and hit detection in Play Mode.

### Evidence

- `boards/MON/MONSTER_SKILL_RUNTIME_PREFAB_DECOMMISSION_PLAN.md`
- Runtime CSV shape check passed for 33 files; active runtime prefab path is Rin-E only.
- Deleted prefab GUID reference check passed for 33 prefab GUIDs.

### History

- 2026-07-14: Code Builder removed migrated prefab paths, normalized runtime collider offsets, and deleted migrated prefab assets.

## Task: 2026-07-17 Enemy Passive CSV Normalization

### Task title

Normalize Enemy passive ownership into assignment and definition tables.

### Goals

- Replace `passive_skill_name`, `passive_skill_id`, `passive_skill_value`, and `passive_summary` in `enemies.csv` with `passive_id`.
- Make `skills_passive.csv` the value/function authority for Enemy passives.
- Validate active A/B references separately from passive references.

### Constraints

- Role Owner is Code Builder.
- No new Enemy Choice or graph CSV is introduced.
- Enemy passive authoring contract remains limited to ID, display name, target, modifier kind, and value.
- Shared runtime classification values are fixed by the dedicated Enemy passive parser.

### Role Owner

Code Builder

### Status

Implemented. Solution compile, static CSV validation, and Unity Editor validation passed.

### Next Actions

- Keep future Enemy passive tuning in `skills_passive.csv`; change `enemies.csv` only when assignment changes.

### Evidence

- `enemies.csv` contract now ends with `skill_slot_a_id,skill_slot_b_id,passive_id,nexus_damage`.
- `skills_passive.csv` contract is `skill_id,display_name,apply_target,modifier_kind,modifier_value`.
- `PakuriCsvRuntimeData.EnemyMigrationDataset.cs` routes `skills_passive.csv` to a dedicated parser, reads the five authored columns, and internally materializes `skill_kind=Passive`, `slot=F`, `runtime_kind=Passive`.
- The same parser rejects passive rows authored outside `skills_passive.csv`; catalog validation rejects unknown references, unsupported targets, `None`, and non-positive values.
- `PakuriCsvRuntimeData.Build.cs` resolves `passive_id` into `EnemyPassiveDefinition`.
- CSV reference check found 16 assignments, 16 definitions, 0 missing, and 0 unused.
- CSV shape check found 5 columns, 16 rows, 0 width errors, and `enemy-sword-mastery.modifier_value=0.10`.
- `dotnet build Pakuri/Pakuri.sln --no-restore` passed with 0 errors and the existing 2 assembly-version warnings.
- `git diff --check` passed for the implementation files.
- `SyncAndValidateCsvRuntimeCatalogsForEditor()` logged `[EnemyPassiveParserValidation] PASS` for the five-column contract; the temporary hook and generated `.meta` leave no residue.

### History

- 2026-07-17: Code Builder established `enemies.csv` as passive assignment authority and `skills_passive.csv` as passive definition authority.
- 2026-07-17: Code Builder removed redundant `skill_kind`, `slot`, and `runtime_kind` columns and moved those constants into the dedicated Enemy passive parser.
