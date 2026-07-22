## Archived History

- Non-July task blocks from `boards\DATA\DATA_BLACKBOARD.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/DATA_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older CSV-transition history remains in `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current runtime CSV authority, cleanup decisions, and archive destinations still needed for ongoing work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-22 Damage And Defense Type Simplification

### Task title

Keep one defense data type and reduce DamageCalculator to raw and final damage calculation.

### Goals

- Use `UnitDefenseStats` for definition and runtime defense values.
- Copy definition defenses only once when creating a mutable runtime unit.
- Remove per-hit defense conversion and unused damage calculation layers.

### Constraints

- Role Owner is Code Builder.
- CSV columns and authored defense values remain unchanged.
- Existing defense reduction, critical, incoming-damage, shield, and healing formulas remain unchanged.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and solution-build verified.

### Next Actions

- User verifies physical/elemental defense, critical hits, shield amounts, healing, and conditional projectile damage in Play Mode.

### Evidence

- `DamageCalculator.cs` now contains only `CalculateRawDamage(...)` and `CalculateFinalDamage(...)` as calculation methods.
- `AttributeDefenseSet`, `CopyDefenses`, `ResolveDefense`, `ResolveDamageAgainstTarget`, shield calculation, and healing power calculation were removed from `DamageCalculator`.
- `MonsterDefinition`, `EnemyDefinition`, and `GameDataCatalogBuilder` now use `UnitDefenseStats` directly without changing CSV fields.
- `UnitCombatStateFactory.CreateRuntimeDefenses(...)` retains the one required copy before runtime passives mutate unit defenses.
- `ProjectileSkillActor.ResolveHitDamage(...)` now applies the same snapshot conditional damage multiplier as the other projectile hit path.
- Search under `Pakuri/Assets/Scripts` found zero references to the removed defense and damage helper symbols.
- `git diff --check` passed for the edited scripts.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-version warnings.

### History

- 2026-07-22: Code Builder unified defense representation, removed per-hit defense allocation, and reduced DamageCalculator to the two requested damage stages.

## Task: 2026-07-19 Stage Parser Dead Value Removal

### Task title

Remove unused Stage row values while preserving active CSV parsing behavior.

### Goals

- Stop storing `StageDayRow.CombatType`, which had no reader.
- Stop storing `StageRewardRow.PrisonerCount3Chance`, whose runtime result already uses the implicit remainder branch.
- Preserve numeric validation of `prisoner_count_3_chance` and leave CSV files unchanged.

### Constraints

- Role Owner is Code Builder.
- Stage encounter composition, reward values, probability branch behavior, CSV schema, and UTF-8 data remain unchanged.
- No CSV or runtime catalog asset is edited by this task.

### Role Owner

Code Builder

### Status

Implemented, solution-build verified, and Unity Editor compile-verified.

### Next Actions

- User verifies representative Stage reward rolls and day progression in Play Mode.

### Evidence

- `StageManager.cs` removes zero-reference `CurrentEncounterId`, `CurrentRewardRuleId`, `StageDayRow.CombatType`, and stored `PrisonerCount3Chance`.
- `LoadRewards(...)` still calls `ParseFloat(row, "prisoner_count_3_chance")` through a discard, preserving malformed-number failure behavior.
- The 1/2/3-prisoner branch remains `chance1`, `chance1 + chance2`, then remainder; no CSV content changed.
- Solution build passed with 0 errors and the existing 2 `MSB3277` warnings.
- Unity refresh/compile returned to idle with no C# compiler or `Assets/Scripts` error entries.

### History

- 2026-07-19: Code Builder removed dead Stage values while retaining the third-chance parse validation side effect.

## Task: 2026-07-17 Stage 1 Boss Health Multiplier Retuning

### Task title

Retune Stage 1 normal boss candidates and fixed midboss health multipliers by day range.

### Goals

- Set Stage 1 Day 1-5 applicable boss rows to a `3-5` health multiplier range.
- Set Stage 1 Day 6-10 applicable boss rows to a `6-10` health multiplier range.
- Preserve Stage 1 non-boss escorts, Day 11, and all Stage 2 rows.

### Constraints

- Role Owner is Code Builder.
- Change only `boss_health_multiplier_min` and `boss_health_multiplier_max` on Stage 1 rows selected as a boss candidate or guaranteed boss for Day 1-10.
- Do not change CSV schema, encounter composition, prisoner flags, or notes.

### Role Owner

Code Builder

### Status

Implemented and structurally validated.

### Next Actions

- User verifies the revised Stage 1 boss durability curve in Unity Play Mode.

### Evidence

- `Pakuri/Assets/CSVdata/stage_flow/StageEncounter.csv` contains 24 applicable Stage 1 Day 1-10 boss rows: Day 1-5 validate as `3-5`, and Day 6-10 validate as `6-10`.
- CSV validation reports 14 header fields, 60 data rows, and zero malformed-width rows.
- Day 5 and Day 10 non-boss escort rows remain `1-1`; Stage 1 Day 11 and all Stage 2 rows are outside the diff.

### History

- 2026-07-17: Code Builder applied the user-approved Stage 1 boss health multiplier ranges without changing unrelated encounters.

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

- `Pakuri/Assets/CSVdata/authoring/enemy/enemies.csv`: 16 data rows.
- `Pakuri/Assets/CSVdata/authoring/enemy/enemy_skill_loadouts.csv`: 32 data rows.
- `Pakuri/Assets/CSVdata/authoring/enemy/skills/base/`: 16 active skill rows across projectile, area_attack, single_attack, buff, heal, and shield; passive is header-only.
- `Pakuri/Assets/CSVdata/authoring/enemy/skills/triggers/`: OpeningCharge and Intimidation CombatStart rows.
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
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring/monster/skills`.
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
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync from `Assets/CSVdata/authoring`, and `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary without CSV fatal errors.
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

- Implement the Ariel F-J passive node conversion and EffectTarget cleanup.
- Keep Ariel F-J passive behavior on functional node params instead of copied old effect defaults.
- Remove status/shield passive target defaults that do not carry current behavior: `target_selection=Owner`, `target_shape=Battlefield`, `center_mode=Caster`, and no-visual `visual_anchor_mode=AppliedTargets`.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring/monster/skills`.
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

- Edited `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/passive/passive_skill_node_params.csv`.
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

- 2026-07-09: User requested Code Builder to implement the Ariel F-J passive node conversion and EffectTarget cleanup.

## Task: 2026-07-09 Ariel Node-Based Choice/Effect Cleanup

### Task title

Remove wrongly migrated Ariel choice/master effect clones and keep behavior on functional nodes.

### Goals

- Remove copied `MigratedToEffectBinding` effect-owned node groups that duplicated Ariel enhancement/master combinations.
- Keep numeric choice behavior on functional Choice nodes such as `StatusActionSpeedBonus`, `StatusShieldReceivedBonus`, `ShieldAmountMultiplier`, `StatusDamageTakenBonus`, and `StatusDurationBonus`.
- Preserve base effect-owned node groups for actual effects such as Ariel C blessing, Ariel E shield, and Ariel J post-E action speed.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring/monster/skills`.
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
- Verified removed ids and `MigratedToEffectBinding` no longer appear under `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes`.
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

- Replace monster-folder split CSV files under `Pakuri/Assets/CSVdata/authoring/monster/skills` with skill-kind-owned folders.
- Add explicit `monster_id` to consolidated runtime skill CSV rows so loader ownership no longer depends on monster-prefixed filenames.
- Preserve current loaded row ids and row counts across base, choice, effect, trigger, node, and node-param data.
- Keep runtime/editor catalog loading compatible with the new file names and folders.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring/monster/skills`.
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

- Keep active runtime CSV authority under `Pakuri/Assets/CSVdata/authoring`.
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
- Ariel node files are now `ariel_buff_skill_nodes.csv`, `ariel_passive_skill_nodes.csv`, `ariel_projectile_skill_nodes.csv`, and `ariel_single_attack_skill_nodes.csv` under `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/ariel/node/`.
- Ariel param files are now the matching `ariel_*_skill_node_params.csv` files under `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/ariel/nodes_param/`.
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
- Active runtime authority remains under `Pakuri/Assets/CSVdata/authoring`.
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

- Deleted `Pakuri/Assets/CSVdata/authoring/monster/skills/effects/ariel/ariel_skill_effects.csv` and `.meta`.
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
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring/monster`.
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
- CSV shape check across `Pakuri/Assets/CSVdata/authoring/monster/**/*.csv` returned `CSV_SHAPE_OK`.
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
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring`.
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

- Created monster folders under `Pakuri/Assets/CSVdata/authoring/monster/skills/choices`, `effects`, and `triggers`.
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

- Create `ariel`, `eve`, `rin`, `sein`, and `vega` folders under `Pakuri/Assets/CSVdata/authoring/monster/skills/base`.
- Rename base CSV files from `monster_skills_*` to `{monster}_skills_*` inside the owning monster folder.
- Remove columns that are empty for every data row in each split base CSV.
- Create the same five monster folders under `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes` and move current node CSV files into their owner folders.
- Keep in-game runtime loading compatible with the split folder structure.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring`.
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

- `Pakuri/Assets/CSVdata/authoring/monster/skills/base` now has `ariel`, `eve`, `rin`, `sein`, and `vega` folders.
- The six root `monster_skills_*.csv` base files were replaced by 23 monster-owned files named `{monster}_skills_projectile.csv`, `{monster}_skills_line_attack.csv`, `{monster}_skills_area_attack.csv`, `{monster}_skills_single_attack.csv`, `{monster}_skills_buff.csv`, and `{monster}_skills_passive.csv` as applicable.
- Base CSV verification returned `base_file_count=23`, `base_row_count=50`, `base_by_monster=ariel:10,eve:10,rin:10,sein:10,vega:10`, `duplicate_skill_ids=0`, `blank_columns_after_split=0`, and `root_legacy_base_files=0`.
- 2026-07-06 follow-up cleanup removed 88 optional base CSV columns whose values were only parser defaults, including numeric `0`, bool `false`, blank strings, `status_effect_label=없음`, default `DamageAttribute.Physical`, default `required_active_slot=A`, and default multiplier `1` columns.
- Follow-up sample checks showed `Pakuri/Assets/CSVdata/authoring/monster/skills/base/ariel/ariel_skills_buff.csv` no longer contains `status_max_stacks`, `status_stack_amount`, `status_action_speed_bonus`, or `status_attack_power_bonus`.
- Follow-up sample checks showed `Pakuri/Assets/CSVdata/authoring/monster/skills/base/vega/vega_skills_line_attack.csv` no longer contains `active_duration_seconds`, `shot_interval_seconds`, or `spell_power_coefficient`.
- Follow-up base CSV verification returned `row_count=50`, `duplicate_skill_ids=0`, `by_monster=ariel:10,eve:10,rin:10,sein:10,vega:10`, and `default_only_optional_columns_remaining=0`.
- 2026-07-06 bool cleanup inspected `InGameSkillDefinitionMapper.MapDamage`, `BuffSkillData` mapping, `ShieldSkillData` mapping, and `SupportSkillExecutors`; `critical_allowed` is used through `MapDamage` for damage specs, but Shield mapping does not call `MapDamage`, and current Buff executor does not apply attached damage.
- Removed non-applicable `critical_allowed` from `ariel/ariel_skills_buff.csv`, `rin/rin_skills_buff.csv`, and `vega/vega_skills_buff.csv`.
- Bool follow-up verification returned `support_critical_allowed_remaining=0`, `row_count=50`, `duplicate_skill_ids=0`, and `by_monster=ariel:10,eve:10,rin:10,sein:10,vega:10`.
- Remaining bool `true` columns are code-referenced: `is_default_learned` is used by `RunSession` and validation, `is_available_without_active_requirement` is used by `RunSession`/UI/validation, `require_execute_threshold_to_cast` is used by single-attack execution, and remaining `critical_allowed` appears only on damage runtime kinds.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes` now has `ariel`, `eve`, `rin`, `sein`, and `vega` folders.
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
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring`.
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

- Deleted aggregate `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/monster_skill_nodes.csv` and `monster_skill_node_params.csv` plus their `.meta` files.
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
- Move active monster skill body/effect/trigger/node CSV files under purpose folders below `Pakuri/Assets/CSVdata/authoring/monster/skills`.
- Keep split choice columns narrow by omitting columns with no non-empty values in that split.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring`.
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

- Created `skills/base`, `skills/choices`, `skills/effects`, `skills/triggers`, and `skills/nodes` folders under `Pakuri/Assets/CSVdata/authoring/monster/skills`.
- Moved `monster_skills_projectile.csv`, `monster_skills_line_attack.csv`, `monster_skills_area_attack.csv`, `monster_skills_single_attack.csv`, `monster_skills_buff.csv`, and `monster_skills_passive.csv` into `skills/base` with their `.meta` files.
- Moved `monster_skill_effects.csv` into `skills/effects`, `monster_skill_triger.csv` into `skills/triggers`, and `monster_skill_nodes.csv` / `monster_skill_node_params.csv` into `skills/nodes` with their `.meta` files.
- Deleted `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_choices.csv` and its `.meta`; verification returned `OLD_CHOICE_EXISTS=False` and `ROOT_CSV_COUNT=0`.
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

- Replace `monster_skills.csv` with runtime-kind split CSV files under `Pakuri/Assets/CSVdata/authoring/monster/skills/`.
- Merge `CooldownProjectile` into the projectile split file, `Field` into the area-attack split file, and `Shield` into the buff split file.
- Keep split CSV columns narrow so each file contains only columns used by its owned runtime kinds.
- Make runtime loading, editor sync, and validation use the split files instead of `monster_skills.csv`.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring`.
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
- Deleted `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skills.csv` and `monster_skills.csv.meta`; `Test-Path` returned `False` for both paths.
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
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring`.
- MSW-MCP is not used; Unity-MCP remains the only MCP path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Keep future monster choice rows in `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_choices.csv`.
- If Unity Editor validation is needed, run the existing Pakuri CSV runtime sync/validate menu through Unity-MCP.

### Evidence

- Deleted `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_base.csv` and `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_base.csv.meta`.
- Deleted `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_choice_base.csv` and `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_choice_base.csv.meta`.
- `PakuriCsvRuntimeSourceCatalog.cs` and `PakuriCsvRuntimeSourceCatalog.asset` no longer contain `MonsterSkillBase` or `MonsterSkillChoiceBase`.
- `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.Loader.cs`, `PakuriCsvRuntimeData.SourceModel.cs`, `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`, and `PakuriCsvRuntimeData.Validation.cs` no longer use `SkillBaseRows` or `SkillChoiceBaseRows`.
- `Test-Path` for the four deleted CSV/meta paths returned `False`, `False`, `False`, and `False`.
- `Select-String` under `Pakuri/Assets/Scripts2/InGame/Data/Runtime` for removed base-table symbols and filenames returned no matches.
- `Select-String` on `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` for removed base-table symbols and filenames returned no matches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.

### History

- 2026-07-05: User asked Code Builder to delete `monster_skill_base.csv` and `monster_skill_choice_base.csv`, and to stop referencing `monster_skill_choice_base.csv` by unifying choice references onto `monster_skill_choices.csv`.

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
- Active data authority is `Pakuri/Assets/CSVdata/authoring`.
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
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync from `Assets/CSVdata/authoring`; post-sync `Pakuri/Validate CSV Source Data` loaded the runtime catalog with no error entries.

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

## Task: 2026-07-17 CSV Authoring Root Rename And Empty Node Cleanup

### Task title

Rename the active CSV authoring root and remove zero-row direct node inputs.

### Goals

- Rename the former `runtime` CSV root to `Pakuri/Assets/CSVdata/authoring`.
- Keep `monster/skills/nodes/definitions` as the node schema authority.
- Delete the zero-data-row `buff`, `passive`, `projectile`, and `single_attack` direct node folders.

### Constraints

- Role Owner is Code Builder.
- Preserve every CSV and `.meta` GUID outside the explicitly empty node inputs.
- Preserve runtime catalog behavior, graph-node generation, and `Assets/Resources/Pakuri/CSVRuntime` output paths.
- Do not edit archive boards during current-path migration.

### Role Owner

Code Builder

### Status

Implemented and validated.

### Next Actions

- Future authored CSV files use `Pakuri/Assets/CSVdata/authoring`.
- Add direct node CSV folders again only if an actual authored direct-node row requires them.

### Evidence

- `authoring.meta` retains former root GUID `764a31a743b22f8468ef8ce3e253f371`.
- `runtime` and `runtime.meta` no longer exist; `authoring/{catalog,enemy,monster,status}` exists.
- Deleted 8 direct-node CSV files each had exactly 2 schema rows and 0 data rows.
- `authoring/monster/skills/nodes` now contains only `definitions`; its two CSV files contain 89 and 196 data rows.
- `PakuriCsvRuntimeData` and its postprocessor now resolve/watch `Assets/CSVdata/authoring`.
- Unity source sync left `MonsterSkillNodeFiles` and `MonsterSkillNodeParamFiles` empty while preserving both definition references.
- `dotnet build Pakuri/Pakuri.sln --no-restore` passed with 0 errors and the existing 2 `MSB3277` warnings.
- Unity batch sync loaded 5 monsters, 8 Stage 1 enemies, and 8 Stage 2 enemies and logged successful validation from `Assets/CSVdata/authoring`.
- The sync wrapper now selects the exact Unity version from `ProjectSettings/ProjectVersion.txt`; the current open Editor is 6000.3.14f1.
- The open 6000.3.14f1 Editor logged `[AuthoringCsvMigrationValidation] PASS`; the temporary validation hook files leave no residue.

### History

- 2026-07-17: Code Builder performed the GUID-preserving root move, deleted verified zero-row node inputs, migrated active path references, and validated the generated runtime catalogs.
- 2026-07-17: Code Builder restored unrelated project/package/URP changes caused by an initial older-Editor validation launch; those files have zero content diff.

## Task: 2026-07-17 Remove Legacy Skill Choice Modifier Runtime

### Task title

Remove the disconnected `SkillChoiceModifierLibrary` runtime path and its empty compatibility API.

### Goals

- Delete the unused `Skills/Execution/Modifiers` scripts and Unity metadata.
- Remove the empty modifier-library setter and hardcoded zero-count properties.
- Preserve the active `SkillChoiceDefinition -> SkillExecutionSnapshot` choice application path.

### Constraints

- Role Owner is Code Builder.
- Do not change authoring CSV, runtime catalog, prefab, scene, or live choice behavior.
- Remove only symbols proven disconnected by repository-wide source search.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that learned Enhancement/Master choices still affect their target skills.
- Future choice modifiers continue through `SkillChoiceDefinition` and `SkillExecutionSnapshot`; do not restore the deleted standalone modifier CSV parser/library path.

### Evidence

- Deleted `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Modifiers/SkillChoiceModifierLibrary.cs`, `SkillChoiceModifierRecord.cs`, their `.meta` files, and the folder `.meta`; the `Modifiers` directory no longer exists.
- Removed `SkillExecutionSystem.ModifierRecordCount`, the empty `SetChoiceModifierLibrary(...)`, and `InGameCombatManager.SkillChoiceModifierRecordCount`.
- Search for `SkillChoiceModifierLibrary|SkillChoiceModifierRecord|SetChoiceModifierLibrary|ModifierRecordCount|SkillChoiceModifierRecordCount` under `Pakuri/Assets/Scripts2` returned zero references.
- The live resolver still loads chosen IDs through `PakuriDataManager.TryGetData(..., out SkillChoiceDefinition)` and applies them with `SkillExecutionSnapshot.ApplyChoiceDefinition(...)`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and the existing 2 `MSB3277` warnings.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and the existing 2 `MSB3277` warnings.
- Unity-MCP Console query returned 0 error entries, and `SkillExecutionSystem.cs` validation returned 0 errors and 0 warnings.
- Unity-MCP validation reported a duplicate `ResolveEffectManager()` in `InGameCombatManager.cs`, but source search found one declaration at line 1319 and both C# builds passed; this is a validator false positive unrelated to the removed property.

### History

- 2026-07-17: User approved deletion of the legacy modifier scripts, folder, metadata, and remaining empty APIs; Code Builder completed removal and compile verification.
