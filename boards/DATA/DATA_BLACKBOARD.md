## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/DATA_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older CSV-transition history remains in `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current runtime CSV authority, cleanup decisions, and archive destinations still needed for ongoing work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

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
- No CSV, parser, runtime catalog, code, prefab, or scene file was changed by this design task.
- The handoff is grounded in the inspected source feedback html/md, active CSV headers, current parser/build code, and Phase 6 `SkillExecutionPlan` surface.
- Old wide columns must not be deleted in the first implementation.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Designer

### Status

Handoff created.

### Next Actions

- Code Builder implements the schema/parser skeleton only after explicit user request.
- Code Builder updates this DATA board when actual CSV schema, parser, validation, or runtime catalog behavior changes.
- Code Builder updates `boards/COMBAT/ENEMY_BLACKBOARD.md` if normalized nodes change `SkillExecutionPlan`, executor routing, or runtime skill behavior.
- Code Reviewer should review before real skill authoring starts on the new node path.

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

### History

- 2026-06-17: User noted that Phase 6 had not actually split or structured the CSV authoring layer yet, then requested a design and handoff for splitting `monster_skills.csv` and `monster_skill_choices.csv` so new exception skills can add nodes instead of columns.

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
