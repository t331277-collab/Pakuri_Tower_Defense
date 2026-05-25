## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/DATA_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older CSV-transition history remains in `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current runtime CSV authority, cleanup decisions, and archive destinations still needed for ongoing work.

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

## Task: 2026-05-23 Eve-E Choice CSV Schema And Row Authoring

### Task title

Extend the shared choice CSV schema so Eve-E can author vulnerable max-stack bonuses and target-status-gated damage without Eve-only hardcoded rows.

### Goals

- Keep Eve-E choice behavior data-owned in `monster_skill_choices.csv`.
- Add generic choice columns for targeted status max-stack bonuses and conditional target-status damage multipliers.
- Re-author Eve-E rows so no Eve-E trait/master row remains partial or unsupported after the shared runtime extension.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Current CSV files were explicitly treated as the parsed source for this task.
- No new Eve-only companion CSV table was added; the work stays inside `monster_skill_choices.csv`.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Reuse `status_max_stacks_bonus_status_id` plus `status_max_stacks_bonus` for future choice-driven status-cap increases before adding another schema.
- Reuse `conditional_damage_multiplier` plus `conditional_target_status_id` / `conditional_target_status_min_stacks` for future hit-time target-threshold damage bonuses before adding skill-id-specific columns.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `status_max_stacks_bonus_status_id`, `status_max_stacks_bonus`, `conditional_damage_multiplier`, `conditional_target_status_id`, and `conditional_target_status_min_stacks` to the active choice schema.
- Eve-E rows in `monster_skill_choices.csv` are now all `RuntimeImplemented`; trait 1 keeps `magazine_bonus=1`, trait 4 keeps `reload_time_multiplier=0.76923` plus `branch_count=1`, master 1 keeps `shot_interval_multiplier=0.76923` plus `branch_count=2`, trait 5 now authors `conditional_damage_multiplier=1.4` gated by `vulnerable >= 5`, and master 2 now authors `status_critical_damage_taken_bonus=0.01` plus `status_max_stacks_bonus_status_id=vulnerable` and `status_max_stacks_bonus=5`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate those new choice columns.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Skills/Data/SkillChoiceEffectSpec.cs`, `Skills/Execution/SkillChoiceModifierRecord.cs`, and `Skills/Execution/SkillExecutionSnapshot.cs` now carry the new fields into runtime choice snapshots.
- `Import-Csv -Encoding UTF8 Pakuri\\Assets\\CSVdata\\source\\monster_skill_choices.csv | Where-Object { $_.skill_id -eq 'eve-e' }` returned all seven Eve-E choice rows with `runtime_support_state=RuntimeImplemented`.
- `dotnet build Pakuri\\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the schema and row-authoring change; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User asked Skill Builder to implement Eve-E from the current CSV/code as parsed source, which required new shared choice columns for the last unsupported Eve-E behaviors.

## Task: 2026-05-23 Eve-D Choice Payload On Existing CSV Fields

### Task title

Author Eve-D on current CSV/runtime authority by reusing existing choice fields instead of adding new SingleAttack follow-up columns.

### Goals

- Keep Eve-D base tuning on the existing `monster_skills.csv` row.
- Keep cooldown reduction on the existing `cooldown_multiplier` field.
- Reuse existing choice fields for the scoped delayed follow-up payload needed by Eve-D master 1.
- Avoid introducing new CSV columns for a one-skill exception while the current parser requires strict row-width alignment.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Current CSV and code were explicitly treated as the parsed source for this task.
- No new columns were added to `monster_skill_choices.csv`, `monster_skill_effects.csv`, or `monster_skill_triger.csv`.
- `branch_search_radius` is reused here as delay seconds only on the new shared SingleAttack follow-up interpretation path; this was not generalized into a new schema name in this task.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If another SingleAttack later needs the same delayed status-gated follow-up, author it on the same existing fields and cite this task rather than adding duplicate schema.
- If a future design needs both real branch-search radius and delayed follow-up timing on the same SingleAttack contract, revisit the schema with fresh parsed-source evidence before overloading more fields.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` keeps Eve-D on one base row with `runtime_kind=SingleAttack`, `cooldown_seconds=7`, and `status_effect_id=shock`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `eve-d-trait-3` with `cooldown_multiplier=0.8`, which keeps cooldown reduction on the shared cooldown field instead of a new column.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `eve-d-master-1` with `status_tag=shock`, `branch_count=1`, `branch_damage_multiplier=0.5`, `branch_search_radius=0.5`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Eve/Eve_D.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` still has no Eve-D effect rows, and `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` still has no Eve-D trigger rows, so this implementation stayed on the existing base/choice tables.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now consumes that existing choice payload in `ResolveFollowUpSpec(...)` and `ExecuteConditionalFollowUpAfterDelay(...)` instead of requiring schema expansion.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the Eve-D CSV-authoring pass; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User told Skill Builder to implement Eve-D from the current CSV/code as parsed source and explicitly required cooldown reduction to keep using existing cooldown CSV fields rather than inventing a new cooldown-decrease schema.

## Task: 2026-05-23 Zone Prefab Radius Multiplier Interpretation

### Task title

Keep Eve-C zone scaling on `radius_multiplier` while moving hit detection to prefab colliders.

### Goals

- Preserve current CSV authority without adding new Eve-C schema.
- Keep `radius_multiplier` as the authored scaling input for collider-based zone prefabs.
- Avoid repurposing `radius_bonus` for the requested `1.3 => 30% larger` behavior.

### Constraints

- Role Owner is Code Builder.
- No CSV source row or header was changed in this task.
- `radius` remains in the schema because other shared paths and effect rows still use it as data.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented in runtime interpretation only; no CSV content change was required.

### Next Actions

- Future Eve-C-style prefab growth should use `radius_multiplier`, not `radius_bonus`.
- If the project later wants to remove `radius` from AreaAttack schema entirely, inspect effect rows and non-hitbox fallback paths first instead of deleting it from current CSV blindly.

### Evidence

- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.radius_bonus -and $_.radius_bonus.Trim() -ne '' }` returned no authored runtime rows, so the current active data does not require a `radius_bonus=1.3` reinterpretation.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now scales collider-backed AreaAttack prefabs through the existing snapshot scale-factor path, which is driven by current choice snapshot radius scaling.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` still contains rows such as `eve-c-master2-expire-burst` that use `radius`, so the shared schema field was not removed in this task.
- No CSV file under `Pakuri/Assets/CSVdata/source/` was edited for this change.

### History

- 2026-05-23: User approved the Eve-C prefab-collider implementation and explicitly said not to use `radius_bonus=1.3` for the scaling behavior.

## Task: 2026-05-23 Eve-C Choice And Effect CSV Schema Follow-Up

### Task title

Extend the shared skill CSV schema so Eve-C can author targeted status-duration bonuses, threshold-status promotions, and an OnExpire burst through data-owned rows.

### Goals

- Add choice columns for targeted status-duration bonuses and threshold-status promotions.
- Re-author Eve-C trait/master rows on those generic columns instead of changing global status defaults.
- Add the Eve-C master 2 OnExpire effect row to `monster_skill_effects.csv`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- CSV remains the source of truth for Eve-C tuning; runtime code only adds generic consumers for the new columns.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified. Unity-MCP sync/console calls timed out during this task, so runtime catalog evidence for the new prefab path was recorded from the serialized asset catalog file rather than a fresh sync log.

### Next Actions

- Keep future “bonus duration for one status only” skills on `status_duration_bonus_status_id` plus `status_duration_bonus`.
- Keep future “X stacks of A immediately applies B” skills on `threshold_status_id`, `threshold_status_min_stacks`, and `threshold_apply_status_id`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:1-2` now includes `status_duration_bonus_status_id`, `status_duration_bonus`, `threshold_status_id`, `threshold_status_min_stacks`, and `threshold_apply_status_id`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate those five columns.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `eve-c-trait-5` with `freeze +1.0s`, `eve-c-master-1` with `freeze +1.5s` plus `chill >= 4 -> freeze`, and `eve-c-master-2` as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now authors `eve-c-master2-expire-burst` with `OnExpire`, `Ice`, `24` base damage, `1.5` spell coefficient, `requires_active_choice_id=eve-c-master-2`, and `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.monster_id -eq 'eve' -and $_.skill_id -eq 'eve-c' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned no rows.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_effects.csv | Where-Object { $_.effect_id -eq 'eve-c-master2-expire-burst' }` returned the authored OnExpire damage row.

### History

- 2026-05-23: User rejected a global `freeze` duration edit and asked for a shared choice-snapshot extension plus a data-owned Eve-C master-2 expire burst row.

## Task: 2026-05-22 Ariel Dynamic Choice Count And Conditional Status CSV Schema

### Task title

Extend the shared CSV schema so Ariel's last two choice rows stay data-owned through generic count-based and source-conditional status fields.

### Goals

- Add choice fields for dynamic per-cast status counting and per-count damage scaling.
- Add choice/status fields for source-status-gated incoming-damage bonuses on applied statuses.
- Keep the supporting schema in `monster_skill_choices.csv` and `monster_skills.csv` aligned with the current parser contract.

### Constraints

- Role Owner is Code Builder.
- CSV remains the source of truth for authored Ariel choice behavior; runtime code only adds generic consumers for the new columns.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and CSV-sync-verified.

### Next Actions

- Future “count allies with X status” or “target status grants bonus only from attackers with Y status” designs should use these same fields before adding new skill-specific schema.
- Keep `monster_skills.csv` type/header rows aligned with parser-required status payload columns whenever shared status fields are added.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:1-2` now includes `count_status_id`, `count_target_side`, `damage_multiplier_per_count`, `count_max`, `status_conditional_source_status_id`, and `status_conditional_damage_taken_bonus`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:7` and `:28` now author `ariel-a-trait-5` and `ariel-d-trait-5` on those generic fields and mark both rows `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1-2` now matches the shared status payload parser by carrying `status_ailment_resistance_bonus` and `status_flat_element_resist_reduction` in both the header row and the type-description row.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate the new choice/status columns.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs`, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSnapshot.cs` now carry the new fields from CSV definitions into runtime snapshots.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.monster_id -eq 'ariel' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned no rows after this schema pass.
- Unity-MCP console after clear plus `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-22: User asked Code Builder to finish the remaining Ariel choice rows by adding whatever shared common logic was still missing.

## Task: 2026-05-22 Ariel Passive Choice/Resistance CSV Schema Follow-Up

### Task title

Extend the CSV/runtime schema so Ariel passive-choice follow-up rows and shield ailment-resistance rows stay data-owned.

### Goals

- Add CSV fields for `condition_skill_attribute`, `status_ailment_resistance_bonus`, `status_flat_element_resist_reduction`, and `status_critical_chance_bonus`.
- Carry passive choice `status_ailment_resistance_bonus` through choice parsing, build, mapping, and runtime snapshots.
- Re-author the Ariel rows that became supported through these shared fields and record the reduced unsupported set.

### Constraints

- Role Owner is Code Builder.
- CSV remains the source of truth for skill authoring; runtime code only adds generic consumers for the new columns.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly told Builder not to run Reviewer.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Keep future “has Holy active skill” or “status grants ailment resistance” designs on these same fields instead of introducing skill-ID-specific columns.
- Ariel choice rows still unsupported after this pass are only `ariel-a-trait-5` and `ariel-d-trait-5`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:15`, `:40-43`, and `:46-50` now mark the newly supported Ariel rows as `RuntimeImplemented`, and `ariel-b-master-1` stores `status_ailment_resistance_bonus=0.3`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:15`, `:25`, `:27`, `:29-30`, `:33-34`, and `:37` now author the Ariel follow-up rows that rely on the new condition/resistance/crit schema.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.StatusPayload.cs:25-64` now parses `status_ailment_resistance_bonus`, `status_flat_element_resist_reduction`, and `status_critical_chance_bonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:135-162`, `:350-378` now parse choice-level ailment-resistance overrides and effect-level `condition_skill_attribute`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs:265`, `:431-432`, `:481-517` now map those parsed fields into runtime definitions.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:198-203`, `:265-266`, `:314-317`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs:58-59`, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSnapshot.cs:59-60`, `:182-185`, `:252-253` now carry the new fields through definition and snapshot layers.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -like 'ariel-*' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned only `ariel-a-trait-5` and `ariel-d-trait-5`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement the Ariel rows previously classified as CSV-only or requiring only a small shared contract.

## Task: 2026-05-22 Ariel Choice/Trigger Schema Extension For Shared Runtime Effects

### Task title

Extend CSV/runtime schema so Ariel's remaining active/master effects stay data-owned on shared runtime contracts.

### Goals

- Add choice fields for target-count bonus, crit chance bonus, crit damage bonus, and status critical-damage-taken bonus.
- Add trigger support for tracked attribute payload and the new absorb/expire trigger contracts.
- Add multi-effect support for status-duration extension.
- Keep Ariel's new reflection, crit-mark, mark-expiry, and shield-duration rows authored in CSV rather than hardcoded by skill ID.

### Constraints

- Role Owner is Code Builder.
- CSV remains the source of truth for these skill-authoring changes.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly said not to run it.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and coverage-audited.

### Next Actions

- Future skills that need absorb reflection, expiry bursts, or duration extension should add CSV rows against these shared fields first.
- Ariel still has unsupported or partial choice rows outside this schema slice: `ariel-a-trait-5`, `ariel-b-master-1`, `ariel-d-trait-5`, `ariel-f-trait-3`, `ariel-g-trait-1`, `ariel-g-trait-2`, `ariel-g-trait-3`, `ariel-h-trait-3`, `ariel-i-trait-1`, `ariel-i-trait-2`, `ariel-i-trait-3`, and `ariel-j-trait-1`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:16`, `:27`, `:29-30`, and `:35` now encode Ariel rows that use `hit_target_count_bonus`, `status_critical_damage_taken_bonus`, and the new runtime support states.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:5-6` now encode `OnShieldAbsorb` and `OnStatusExpire` Ariel trigger rows with `damage_source` values `ShieldAbsorbedAmount` and `TrackedIncomingDamage`, plus `tracked_attribute=Holy`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:9` now encodes `ExtendStatusDuration` for `ariel-e-trait-5`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` now parses `hit_target_count_bonus`, `crit_chance_bonus`, `crit_damage_bonus`, `status_critical_damage_taken_bonus`, and `tracked_attribute`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now maps those parsed fields into runtime `SkillChoiceEffectSpec`, `SkillTriggerDefinition`, and `SkillEffectDefinition` data.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:55`, `:103-113`, and `:248-260` define the runtime enums/fields backing the new CSV schema.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -like 'ariel-*' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned only the remaining unsupported/partial Ariel rows listed above, confirming the new Ariel rows moved out of unsupported state.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement the previously proposed shared data/runtime changes for Ariel and then verify Ariel-wide implementation coverage.

## Task: 2026-05-22 Monster Skill Trigger CSV Source

### Task title

Add CSV authority for trigger-called hidden skill executions.

### Goals

- Add `monster_skill_triger.csv` to source CSV data and runtime catalog loading.
- Parse, build, validate, asset-reference, and catalog-sync trigger rows into `MonsterDefinition.SkillTriggers`.
- Keep Ariel trigger behavior data-owned where trigger event, choice gate, repeat timing, damage source, target shape, and prefab path are enough.

### Constraints

- Role Owner is Code Builder.
- The requested CSV spelling is `monster_skill_triger.csv`.
- CSV trigger runtime initially supports `SingleAttack` trigger rows only.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- When adding more event-driven rows, add them to `monster_skill_triger.csv` only after a matching generic trigger event exists.
- Keep unsupported trigger categories marked unsupported in choice CSV until their event payload and runtime contract are implemented.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:3-4` contains the new Ariel trigger rows for last projectile hit and shield expiry.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeData.cs` defines `MonsterSkillTriggersFileName = "monster_skill_triger.csv"`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeSourceCatalog.cs:14` adds the `MonsterSkillTriggers` source TextAsset slot.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeData.SourceModel.cs` adds the source-model trigger dictionary, while loader/editor/build/asset-reference/validation files load, build, reference, and validate trigger rows.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeData.MonsterDataset.cs` adds the `SkillTriggerRow` parser.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` was synced and now references the generated `monster_skill_triger.csv` TextAsset.
- Runtime and editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings; Unity-MCP CSV catalog sync logged successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-22: User requested `monster_skill_triger.csv` creation for Ariel `ariel-a-master-1` and `ariel-b-trait-4` trigger skills.

## Task: 2026-05-22 Ariel Choice And Multi-Effect CSV Cleanup

### Task title

Record Ariel CSV-owned choice support corrections and new multi-effect rows.

### Goals

- Keep supported Ariel choice behavior represented in `monster_skill_choices.csv` and `monster_skill_effects.csv`.
- Replace Ariel-E unconditional choice multipliers with conditional multi-effect rows where required.
- Preserve unsupported event-trigger behavior as unsupported instead of encoding it incorrectly.

### Constraints

- Role Owner is Code Builder.
- CSV remains the source of truth for these data-shaped changes.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- Future Ariel event-trigger work should extend runtime trigger support before adding CSV rows for last-shot, shield-expiry, shield-absorb, or mark-expiry effects.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:18-23` correct Ariel-C trait/master runtime support states to `ReferenceDirect`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:34` removes the unconditional `damage_multiplier=1.5` from `ariel-e-trait-4`; `monster_skill_effects.csv:7` adds a Holy Exposure-conditioned extra damage row.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:36` removes the unconditional `damage_multiplier=0.82` from `ariel-e-master-1`; `monster_skill_effects.csv:8` adds all-ally incoming damage `-0.18` for 5 seconds.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:3-6` encode Ariel-E base, trait 2, master 2, and combined trait 2 plus master 2 shield amount rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:9` adds Ariel-B trait 5 as a shield-conditioned all-ally Holy damage status.
- `Import-Csv -Encoding UTF8` checks over the edited choice/effect rows returned the expected fields.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-22: User asked to start implementation from items that can be solved by CSV, then proceed to small shared runtime extensions.

## Task: 2026-05-22 CSV Runtime Refactor Cleanup

### Task title

Reduce duplicate code in the CSV runtime load/build/validation path without changing CSV schema or runtime behavior.

### Goals

- Share CSV line split/join/escape logic between runtime CSV reading and the editor prefab exporter.
- Consolidate repeated skill/status-effect payload parsing and runtime assignment.
- Consolidate repeated build-time filter/sort patterns.
- Use one referenced-asset collection path for editor asset catalog creation and validation coverage.

### Constraints

- Role Owner is Code Builder.
- Keep current CSV column names and runtime definition fields compatible.
- Do not turn `PakuriCsvRuntimeData` into a larger God Class; new responsibilities stay in small helper files.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If Unity regenerates project files, confirm the new runtime CSV helper scripts remain included in generated project metadata.
- Future CSV asset-path additions should go through `CollectReferencedAssets(...)` so editor catalog generation and validation stay aligned.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvLineCodec.cs` now owns shared CSV line split/join/escape/unescape helpers.
- `Pakuri/Assets/Scripts2/InGame/Data/Editor/PakuriSkillEffectPrefabCsvExporter.cs` now uses `PakuriCsvLineCodec` instead of its duplicate local CSV helpers.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.StatusPayload.cs` now owns shared status payload parsing, and `PakuriCsvRuntimeData.Build.cs` applies it through `ApplyStatusPayload(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.AssetReferences.cs` now owns referenced sprite/prefab collection, including `Skill effect '{effect.Id}' status_effect_prefab_path`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` and `PakuriCsvRuntimeData.Validation.cs` now both use `CollectReferencedAssets(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now uses `FilterAndSort(...)` for reward, active skill, effect, passive skill, and skill choice row selection.
- `dotnet build 'Pakuri\Assembly-CSharp.csproj' --no-restore` passed with 0 errors; existing MSB3277 warnings for `System.Net.Http` and `System.IO.Compression` remained.
- `dotnet build 'Pakuri\Assembly-CSharp-Editor.csproj' --no-restore` passed with 0 errors; the same existing MSB3277 warnings remained.
- `git diff --check -- Pakuri\Assets\Scripts2\InGame\Data\Runtime\Csv Pakuri\Assets\Scripts2\InGame\Data\Editor\PakuriSkillEffectPrefabCsvExporter.cs Pakuri\Assembly-CSharp.csproj` reported only existing line-ending normalization warnings and no whitespace errors.

### History

- 2026-05-22: User asked Code Builder to implement the four previously identified duplicate-reduction targets under `InGame/Data/Runtime/Csv`.

## Task: 2026-05-22 Passive Skill Multi-Effect CSV Runtime

### Task title

Extend `monster_skill_effects.csv` so passive skills and passive-gated active effects can use the shared effect runtime.

### Goals

- Attach effect rows to passive skill definitions as runtime data.
- Add passive requirement/exclusion columns and one-shot effect support.
- Add shield-received status modifier data for CSV-authored shield scaling.
- Keep Ariel F-J implementation data-owned rather than hardcoded by skill ID.

### Constraints

- Role Owner is Code Builder.
- CSV remains UTF-8 without BOM and follows the header plus type-row convention.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- Use `requires_passive_skill_id` / `excludes_passive_skill_id` for future passive-gated multi-effect rows.
- Use `apply_once=true` only for effects that should fire once per passive owner/effect key.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:1` now includes `requires_passive_skill_id`, `excludes_passive_skill_id`, `apply_once`, and `status_shield_received_bonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` parses those new columns, and `PakuriCsvRuntimeData.Build.cs` copies them into `SkillEffectDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now sets `PassiveDefinition.PassiveEffects = BuildSkillEffects(model, skill.Id)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` validates `requires_passive_skill_id` and `excludes_passive_skill_id` against passive skill rows.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains the generic `passive-buff` status row.
- A byte check on `monster_skill_effects.csv` returned leading bytes `34 101 102`, confirming the edited CSV starts with `"` / `e` / `f` and not a UTF-8 BOM.
- Unity console logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: Code Builder extended the multi-effect schema after the user asked for Ariel F-J passive implementation through CSV runtime-read effects if possible.

## Task: 2026-05-22 Monster Skill Multi-Effect CSV Runtime

### Task title

Add `monster_skill_effects.csv` as the reusable CSV source for secondary skill effects.

### Goals

- Add a new source CSV table for choice-gated secondary effects.
- Parse/build/validate the table through `PakuriCsvRuntimeData`.
- Use the table to encode Ariel-C ally buffs, master effects, trait 5 shielded-ally Holy damage, and master 2 second wave without hardcoded skill IDs.
- Separate effect application target fields from visual center/anchor fields so ally buffs can apply to allies while visual effects can attach to affected units or stay at the primary attack center.

### Constraints

- Role Owner is Skill Builder.
- CSV remains UTF-8 and follows the existing header plus type-row convention.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder

### Status

Implemented, synced to Unity runtime catalog assets, and locally validated by build plus direct CSV reference checks.

### Next Actions

- Future similar skills should add rows to `monster_skill_effects.csv`.
- If future effects need unsupported targeting or projectile behavior, extend the multi-effect blueprint/schema first.

### Evidence

- Added `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` and Unity generated `monster_skill_effects.csv.meta` with GUID `4ddf6bb31440b41438f4a7b82bbd5a92`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_effects.csv` returned 9 Ariel-C rows with `effect_kind` values `Status` and `Damage`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.cs` now defines `MonsterSkillEffectsFileName = "monster_skill_effects.csv"`.
- `PakuriCsvRuntimeData.Loader.cs`, `.SourceModel.cs`, `.MonsterDataset.cs`, `.Build.cs`, `.Validation.cs`, and `.Editor.cs` now load, parse, build, validate, and catalog effect rows and effect prefab paths.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:83` defines `SkillMultiEffectCenterMode`; `:91` defines `SkillMultiEffectVisualAnchorMode`; `:107` and `:108` store the parsed values on `SkillEffectDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:359` and `:360` parse `center_mode` and `visual_anchor_mode`; `PakuriCsvRuntimeData.Build.cs:281` and `:282` copy them into runtime definitions.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:1` now includes `center_mode` and `visual_anchor_mode`; Ariel-C buff rows use `PrimarySkillCenter` plus `AppliedTargets`, while the master 2 damage row uses `PrimarySkillCenter` plus `Center`.
- Representative Ariel-C buff visual rows use `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab`; trait 2 / trait 5 supplemental numeric rows keep prefab paths blank to avoid duplicate buff visuals.
- A PowerShell CSV reference check over `monster_skills.csv`, `monster_skill_choices.csv`, and `monster_skill_effects.csv` returned `OK effects=9 ariel_c=9`, including skill ID, choice ID, prefab path, damage, and status-effect ID checks.
- 2026-05-22 follow-up CSV check returned all 9 Ariel-C effect rows with parsed `center_mode` / `visual_anchor_mode` values and the expected `Ariel_C-Buff` / `Ariel_C` prefab split.
- Unity-MCP `execute_menu_item` currently fails to find `Pakuri/Validate CSV Source Data` even though `PakuriCsvRuntimeData.Editor.cs` contains `[MenuItem("Pakuri/Validate CSV Source Data")]`; Unity-MCP `execute_code` remains blocked by the known Windows mono path-length error, so final validation did not rely on those tool paths.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- 2026-05-22 follow-up runtime/editor `dotnet build` commands again passed with 0 errors and existing MSB3277 warnings after the center/visual schema extension.

### History

- 2026-05-22: User requested a Designer multi-effect CSV blueprint followed by Skill Builder schema/parser/build/shared-executor implementation.
- 2026-05-22: User asked Code Builder to implement separated multi-effect centers and applied-target visuals so Ariel-C ally buffs can apply to allies but use the requested `Ariel_C-Buff.prefab` unit-attached effect.

## Task: 2026-05-21 Eve-A Branch Choice CSV Retune

### Task title

Retune Eve-A Arc Bolt branch choice rows so the new branch rule stays data-owned.

### Goals

- Remove the forced `branch_chance_set=1` behavior from Eve-A trait 5 and master 1.
- Keep the new branch chance values as additive choice bonuses.
- Set the recursive branch damage falloff to 70% on the choice rows that enable the mechanic.

### Constraints

- Role Owner is Code Builder.
- `monster_skill_choices.csv` remains the source of truth for these choice modifiers.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and rechecked from the edited CSV.

### Next Actions

- If future Arc Bolt tuning changes branch chance or damage falloff again, edit these same choice fields before considering code changes.
- If a future base Eve-A row needs always-on branching, add that through the shared projectile data path rather than overloading these two choice rows.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-a-trait-5` to `branch_chance_bonus=0.35`, blank `branch_chance_set`, `branch_count=2`, and `branch_damage_multiplier=0.7`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-a-master-1` to `branch_chance_bonus=0.6`, blank `branch_chance_set`, `branch_count=2`, `branch_damage_multiplier=0.7`, and `branch_search_radius=4.5`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -in @('eve-a-trait-5','eve-a-master-1','eve-a-master-2') }` returned the updated branch fields exactly, while `eve-a-master-2` remained the non-branch status choice row.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:266-288` is still the shared consumer that interprets these branch fields into runtime branch behavior.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after the edit; existing MSB3277 warnings remained.

### History

- 2026-05-21: User asked Code Builder to implement the new Arc Bolt branch rule with minimal code changes, so the choice CSV was retuned to additive chance plus 70% recursive branch falloff.

## Task: 2026-05-20 Shield And Buff Status Schema Implementation

### Task title

Implement the blueprint CSV/runtime schema for source-aware buff and shield status behavior.

### Goals

- Add explicit skill-row ownership for target scope, merge policy, and shield refresh policy.
- Normalize new shield CSV content onto canonical `status_effect_id=shield` while keeping legacy parse compatibility.
- Keep runtime validation strict enough to catch incomplete shield/buff rows during catalog sync.

### Constraints

- Role Owner is Code Builder.
- CSV files remain the authoritative source for skill-status tuning.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented, synced into the runtime catalog, and validation-backed in the editor.

### Next Actions

- Future timed ally buff/shield rows should populate `status_target_scope` and `status_merge_policy` instead of relying on code-only defaults.
- Future shield rows should continue using canonical `status_effect_id=shield`; keep `holy-shield` as parse compatibility only until old content is fully gone.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1` now includes `status_target_scope`, `status_merge_policy`, and `shield_amount_refresh_policy`; `:4` shows `ariel-b` populated as `shield / all_allies / same_source_refresh / take_highest`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv:10` now keeps the canonical shared shield row under `status_effect_id=shield`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:137-139` adds the three new schema fields to runtime skill definitions.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:80-82` and `:226-228` parse the three new CSV columns into `SkillRow`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs:234-236` copies the parsed values into `SkillDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs:308-328` now rejects buff/shield rows missing supported `status_target_scope`, `status_merge_policy`, or `shield_amount_refresh_policy`, and `:321` / `:351` enforce canonical shield id `shield`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:172-190` maps CSV-owned duration, target scope, refresh rule, and runtime status payload into buff/shield skill data.
- Unity menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` eventually logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the source CSV asset refresh and the `status_effects.csv` quote fix.

### History

- 2026-05-20: Code Builder implemented the schema proposed in the shield/buff blueprint across source CSV, runtime parse/build, mapper, and validation code.
- 2026-05-20: Editor sync first failed with `CSV table 'monster_skills.csv' is missing required column 'status_target_scope'`, which confirmed Unity had not yet reimported the edited CSV asset.
- 2026-05-20: After a forced asset refresh, editor sync failed again with `CSV file 'status_effects.csv' row 10 has 2 columns but expected 19`; Code Builder fixed the broken shield-row quote and reran sync successfully.

## Task: 2026-05-20 Shield And Buff CSV Schema Design Handoff

### Task title

Prepare the CSV/schema handoff for source-aware shield and buff runtime unification.

### Goals

- Record the requested new skill-row data ownership for target scope and merge policy.
- Record that shield duration must come from CSV/runtime data instead of code fallback.
- Give Code Builder one evidence-based schema contract before implementation begins.

### Constraints

- Role Owner is Designer.
- This task changes documentation only; no CSV source file changed yet.
- New field names remain proposal-level until Code Builder implements them.

### Role Owner

Designer

### Status

Schema handoff documented for Code Builder.

### Next Actions

- Code Builder implements the selected skill-row fields through the CSV runtime build path.
- If Builder renames any proposed field, update this file with the final adopted schema names in the implementation turn.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:4` shows `ariel-b` already owns `status_duration_seconds=5` but the current shield runtime does not honor that through timed gameplay state.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:14` and `:35` show buff rows already own duration/stack values.
- `Pakuri/Assets/CSVdata/source/status_effects.csv:10` already has a shield-like shared row under `holy-shield`.
- `boards/SkillBluePrint/shield-buff-status-unification-blueprint.md` proposes CSV-owned `status_target_scope`, merge-policy fields, and shield canonical-id normalization notes.

### History

- 2026-05-20: User requested a Builder-ready markdown handoff for the shield/buff redesign.
- 2026-05-20: Designer documented the required CSV schema direction and linked it to the new Builder blueprint.

## Task: 2026-05-20 Ariel-A Master 2 Status Choice CSV Activation

### Task title

Promote `ariel-a-master-2` from data-only to shared-status-supported choice data with a per-choice Holy damage taken bonus field.

### Goals

- Encode Ariel-A master 2 as a shared status choice in `monster_skill_choices.csv`.
- Let a projectile choice row carry its own `status_element_damage_taken_bonus` instead of forcing all users of the same status row to share one value.
- Keep the source of truth in the unified choice CSV rather than adding a second Ariel-specific data table.
- Sync the runtime catalog after the source CSV edit.

### Constraints

- Role Owner is Code Builder.
- The CSV file remains UTF-8.
- The unified choice CSV schema changed in this task by adding `status_element_damage_taken_bonus`.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented, schema-updated, and synced into the runtime catalog.

### Next Actions

- If later active choices need new debuff application, prefer `status_tag` plus optional stack/chance/override fields before introducing skill-specific runtime branches.
- Keep unsupported Ariel rows explicit until a matching shared runtime contract exists.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains the new header/type column `status_element_damage_taken_bonus`, and `ariel-a-master-2` sets `status_tag=holy-exposure`, `status_stacks_set=1`, `status_element_damage_taken_bonus=0.15`, `runtime_support_state=ReferenceDirect`, and `runtime_support_notes=Reference status effect mapped into unified choice CSV.`
- `git diff -- Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` shows the new shared choice column plus the `ariel-a-master-2` row changing from `DataOnlyUnsupported` to a `holy-exposure` shared-status row with `0.15`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -eq 'ariel-a-master-2' }` returned the new status fields exactly.
- Unity-MCP menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` returned `success:true` for this task; no new sync log line was captured afterward in the inspected console window.

### History

- 2026-05-20: User asked Code Builder to apply the `ariel-a-master-2` Holy Exposure data fix using the shared status choice path.
- 2026-05-20: User then required per-skill values, so Code Builder added `status_element_damage_taken_bonus` to `monster_skill_choices.csv` and set Ariel-A master 2 to `0.15`.

## Task: 2026-05-20 DebugModifiedUI Uses Unified Skill Choice CSV

### Task title

Reuse `monster_skill_choices.csv` runtime choice rows for debug active trait/master UI.

### Goals

- Keep `monster_skill_choices.csv` as the single source for active choice button text and debug-applied active choice IDs.
- Reuse the already built `SkillChoiceDefinition` runtime objects instead of adding a debug-only CSV or hardcoded label path.
- Keep current active choice grouping (`ActiveEnhancement`, `ActiveMaster`) authoritative for debug availability rules.

### Constraints

- Role Owner is Code Builder.
- No CSV file shape or content changed in this task.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented as a new consumer of existing runtime choice data. Debug modifier UI now reads existing `SkillChoiceDefinition` rows and does not add a parallel data source.

### Next Actions

- If future debug UI needs richer formatting than one `Text (TMP)` per button, keep sourcing `Title` and `DescriptionText` from `SkillChoiceDefinition` rather than duplicating the strings elsewhere.
- If passive enhancement debug support is later added, continue using the same `SkillChoiceDefinition` runtime catalog path.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` already contains active choice rows with `choice_id`, `choice_group`, `title`, and `description_text`, for example `ariel-a-trait-1` through `ariel-a-master-2`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:52-83` defines `SkillChoiceDefinition` with `ChoiceId`, `ChoiceGroup`, `Title`, and `DescriptionText`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs:287-315` already builds `SkillChoiceDefinition` rows from the unified choice CSV into active skill and passive skill choice arrays.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now reads `sourceSkill.EnhancementChoices` and `sourceSkill.MasterSkillChoices` directly and writes their `Title` plus `DescriptionText` into `DebugModifiedUI` button labels.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now records applied debug modifier picks with the exact `choice.ChoiceId`, which keeps debug-applied choices aligned with the same runtime IDs used by Offering and combat execution.

### History

- 2026-05-20: User requested `DebugModifiedUI` button text and application behavior sourced from `monster_skill_choices.csv`; Code Builder reused the existing runtime choice catalog instead of adding new data assets.

## Task: 2026-05-20 Projectile Burst Count CSV Field

### Task title

Add shared projectile burst count data to active monster skill CSV.

### Goals

- Add a reusable CSV field for sequential projectile burst count.
- Keep `monster_skills.csv` as the source of Sein-B numeric runtime behavior.
- Keep existing simultaneous projectile modifiers compatible by using the existing `additional_projectile_bonus` column for burst skills.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- CSV stays UTF-8.
- Existing non-burst projectile skills should keep burst count 1.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and validated through build plus Unity runtime mapping inspection.

### Next Actions

- Future projectile skills that need sequential volleys should set `projectile_burst_count` instead of adding monster-specific runtime branches.
- If a future skill needs both sequential burst count and simultaneous fan count independently modified by choices, add a separate choice column instead of overloading `additional_projectile_bonus`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now includes `projectile_burst_count`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` parses `projectile_burst_count` into `SkillRow.ProjectileBurstCount`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` copies `SkillRow.ProjectileBurstCount` into `SkillDefinition.ProjectileBurstCount`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps `SkillDefinition.ProjectileBurstCount` into `ProjectileSkillData.Projectile.BurstProjectileCount`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` returned `sein-b` with display name `작열 난사` and `projectile_burst_count=5`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` returned `sein-b-trait-1 additional_projectile_bonus=2`, `sein-b-master-1 additional_projectile_bonus=4`, and `sein-b-master-2 additional_projectile_bonus=-2`.

### History

- 2026-05-20: Code Builder added the field for Sein-B and kept it generic for future projectile skills.

## Task: 2026-05-19 Monster Choice CSV Unification

### Task title

Unify monster choice runtime data into one choice CSV plus one slim Offering gate CSV.

### Goals

- Replace `monster_reward_choices.csv` with a slim `monster_modifier_skill_choice.csv` gate table.
- Move runtime-applicable monster choice modifiers into `monster_skill_choices.csv`.
- Keep unsupported special-case choices explicitly marked for later logic work instead of hiding them behind missing rows.

### Constraints

- Role Owner is Code Builder.
- Every CSV conclusion must stay tied to inspected code or inspected reference markdown under `Pakuri/reference/2.Monster/`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Active monster choice data is now split into one slim Offering gate CSV and one unified choice/modifier CSV.

### Next Actions

- If future special-case logic is implemented, start from the rows currently marked `DataOnlyUnsupported` or `PartialRuntimeSupport` in `monster_skill_choices.csv`.
- If CSV ownership changes again, update this file together with `boards/RUN/RUN_BLACKBOARD.md` and `boards/UI/RUNSCENE_UI.md`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.cs` now sets `MonsterRewardChoicesFileName = "monster_modifier_skill_choice.csv"`.
- `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv` now contains only `choice_id, monster_id, active_skill_id, passive_skill_id, sort_order`, and `Import-Csv -Encoding UTF8` over the file returned 250 data rows after excluding the type row.
- `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv.meta` now uses GUID `2f9229f6de8506a4fae1fad9c093e347`, and `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` now references that same GUID for `MonsterRewardChoices`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` now parses the slim reward gate rows and the merged modifier/runtime-support columns from `monster_skill_choices.csv`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now builds `RewardChoiceDefinition` as a slim gate object and builds merged runtime modifier fields directly into `SkillChoiceDefinition`.
- Deleted `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` and `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` because active Scripts2 runtime code no longer reads them.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` no longer has the serialized `skillChoiceModifierCsv` field or the old modifier reload path.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` now returns 250 data rows after excluding the type row, with support-state counts `ReferenceDirect=104`, `PartialRuntimeSupport=24`, `DataOnlyUnsupported=115`, and `DerivedFromReference=7`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` keeps unsupported rows explicit, for example `ariel-a-master-1` remains `DataOnlyUnsupported` with a note that the final-shell double explosion still needs special-case logic.
- The applied numeric-choice values were rechecked against inspected reference markdown, including `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md`, `Pakuri/reference/2.Monster/ariel/skill/f-guiding-light.md`, `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`, `Pakuri/reference/2.Monster/eve/skill/b-prism-ray.md`, `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md`, `Pakuri/reference/2.Monster/sein/skill/e-doomsday-line.md`, and `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing MSB3277 `System.Net.Http` / `System.IO.Compression` warnings remained.

### History

- 2026-05-19: Code Builder merged monster choice runtime values into `monster_skill_choices.csv`, introduced the slim `monster_modifier_skill_choice.csv` gate file, deleted the old reward/modifier CSV pair, aligned `PakuriCsvRuntimeSourceCatalog.asset` and the new gate CSV `.meta` on GUID `2f9229f6de8506a4fae1fad9c093e347`, and reclassified several Eve beam/area rows from unsupported to direct or partial support after rechecking the reference markdown and the current runtime field support.

## Task: 2026-05-19 CSV Auto-Sync Missing TextAsset Recovery

### Task title

Make CSV runtime auto-sync recover when a source CSV exists on disk but is not yet imported as a Unity `TextAsset`.

### Goals

- Explain and fix the `Required imported CSV TextAsset is missing` auto-sync failure for `monster_modifier_skill_choice.csv`.
- Keep external CSV edits recoverable without requiring a manual Unity reimport first.
- Preserve the existing runtime source catalog sync ownership.

### Constraints

- Role Owner is Code Builder.
- The fix must stay grounded in inspected editor sync code, real file existence, and Unity console evidence.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and editor-verified. Auto-sync now retries a synchronous asset import before treating a source CSV as missing.

### Next Actions

- If another imported CSV path is renamed externally, use the same recovery path instead of assuming the AssetDatabase has already created the `TextAsset`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` previously called `AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath)` once inside `LoadTextAssetOrThrow(...)` and threw immediately when the load returned `null`.
- The inspected filesystem still contained `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv` and `monster_modifier_skill_choice.csv.meta` while the user-reported stack trace showed the exception was thrown before the asset became an imported `TextAsset`.
- `PakuriCsvRuntimeData.Editor.cs` now calls `TryImportTextAsset(assetPath)` before failing. That helper checks whether the file exists on disk, runs `AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport)`, then runs `AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport)`, and only throws if the `TextAsset` still cannot be loaded.
- Unity-MCP menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` after the fix logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity console after the fix no longer showed the previous `Required imported CSV TextAsset is missing at 'Assets/CSVdata/source/monster_modifier_skill_choice.csv'` exception.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after the fix; a parallel editor build attempt hit only a transient `Assembly-CSharp.dll` file lock, and a standalone `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` then passed with 0 errors. Existing MSB3277 warnings remained unchanged.

### History

- 2026-05-19: User reported Unity editor auto-sync failing on `monster_modifier_skill_choice.csv`; Code Builder traced the failure to a one-shot `LoadAssetAtPath<TextAsset>` check and added a synchronous refresh/import retry path before the fatal exception.

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
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now falls back from blank `status_effect_id` to a parseable `status_effect_label`, so supported labels such as `감전`, `둔화`, `추위`, `빙결`, `취약`, and `방어막` can resolve through `StatusEffectKind`.
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

## Task: 2026-05-18 Monster Skill Active Duration CSV Field

### Task title

Add structured active-duration data for CSV-driven LineAttack skills.

### Goals

- Avoid parsing duration out of description text for Eve-B.
- Keep LineAttack duration as runtime CSV data.
- Preserve the current `monster_skills.csv` authority for skill damage, timing, and status tuning.

### Constraints

- Role Owner is Code Builder.
- Data values must stay in CSV, not skill-ID-specific code branches.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and synced through Unity-MCP CSV runtime catalog validation.

### Next Actions

- Future sustained LineAttack rows should set `active_duration_seconds` instead of relying on prose descriptions.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now includes `active_duration_seconds`.
- Eve-B row sets `active_duration_seconds=1.2`, while its damage, coefficient, width, cooldown, tick interval, status ID, status chance, and status label remain in the same CSV row.
- `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` executed through Unity-MCP and returned `csv-runtime-sync-ok`.
- Unity-MCP mapping inspection confirmed Eve-B maps to `BeamSkillData` with active duration `1.2`, tick `0.15`, damage `12`, coefficient `1.6`, width `3.2`, status `slow`, and chance `0.2`.

### History

- 2026-05-18: Added `active_duration_seconds` to support Eve-B without hardcoded duration values.

## Task: 2026-05-18 Prisoner/Offering UI Data Source Check

### Task title

Confirm CSV-backed display fields used by reward and Offering UI cleanup.

### Goals

- Confirm `stage1-swordsman` is valid enemy ID data, not corrupted CSV text.
- Keep player-facing prisoner names sourced from `stage_one_enemies.csv` display names through the runtime catalog.
- Keep Offering choice labels sourced from current monster skill, passive, and reward definition display fields.

### Constraints

- Role Owner is Code Builder.
- No CSV file was changed in this task.
- No authoritative UI localization CSV was found for static UI labels such as Reward, Prisoner, Gold, Dark Trace, Active, Passive, or Enhancement.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Confirmed and code path updated.

### Next Actions

- If static UI labels need localization, create or identify a dedicated UI string CSV before replacing the remaining English placeholder labels.

### Evidence

- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\stage_one_enemies.csv | Where-Object { $_.enemy_id -eq 'stage1-swordsman' }` returned `display_name : 검사`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` assigns `enemy.DisplayName = sourceEnemy.DisplayName`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves prisoner display text through `GameDataCatalog.GetStageOneEnemyById(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now uses CSV-backed `DisplayName`, `Title`, `DescriptionText`, `Summary`, and IDs for Offering choice text instead of broken hardcoded fragments through its integrated Offering flow helper.

### History

- 2026-05-18: Code Builder inspected CSV and runtime data definitions after the user reported code-side mojibake, then removed the broken hardcoded UI string fragments without changing CSV source data.

## Task: 2026-05-18 Monster AreaAttack And SingleAttack Runtime Data

### Task title

Split sustained AreaAttack rows from one-shot SingleAttack rows in monster skill CSV data.

### Goals

- Keep Eve C/E as sustained `AreaAttack` skills backed by `ZoneSkillData`.
- Add `SingleAttack` for one-shot area damage skills listed by the user.
- Correct Eve C/D display names against the Eve reference skill files.

### Constraints

- Role Owner is Code Builder.
- Numeric skill values stay in `monster_skills.csv`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Eve C/E tick over their authored durations and that SingleAttack skills apply one immediate area hit.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md` names Eve C as `프로스트 필드`; `d-static-override.md` names Eve D as `스태틱 오버라이드`; `e-drone-beacon.md` names Eve E as `플라즈마 필드`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` rows for `ariel-c`, `ariel-e`, `rin-e`, `vega-b`, and `eve-d` now use `runtime_kind=SingleAttack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` rows for `eve-c` and `eve-e` now use `runtime_kind=AreaAttack`; Eve C has `active_duration_seconds=4`, and Eve E has `active_duration_seconds=5`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` defines `SkillRuntimeKind.SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` defines the new one-shot area SkillData type.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps `SingleAttack` to `SingleAttackData` and keeps `AreaAttack` mapped to `ZoneSkillData`.
- Unity-MCP `InGameSkillDataValidator.ValidateCatalog()` returned `valid=True; errors=0; warnings=0`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: User clarified that Eve C/E should be `AreaAttack`, that row 46 and rows 5/7/17/34 should be one-shot area attacks, and requested Code Builder implementation.

## Task: 2026-05-18 Stage1 Enemy Passive CSV Fields

### Task title

Move Stage 1 enemy passive effect values into CSV-backed fields.

### Goals

- Add reusable passive IDs and numeric values beside the existing passive display name.
- Keep same-effect passive variants reusable through one ID with different values.
- Keep Physical damage passives represented as `PhysicalDamageUp`.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-verified.

### Next Actions

- Future Stage 1 enemy passive rows should set `passive_skill_id` and `passive_skill_value` rather than adding skill-kind-specific branches.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` now has `passive_skill_id` and `passive_skill_value` columns.
- The supported passive IDs are validated in `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs`: `PhysicalDamageUp`, `DefenseUp`, `CritChanceUp`, `CritDamageUp`, `HealingUp`, and `IncomingDamageDown`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.EnemyDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `EnemyDefinition.cs` now carry the passive ID/value from CSV into runtime definitions.
- Unity-MCP editor execution of `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` followed by `UnitFactory.CreateEnemy(...)` returned `sword=PhysicalDamageUp:0.1:phys=1.1:out=1;priest=HealingUp:0.15:heal=1.15:phys=1;captain=PhysicalDamageUp:0.12:phys=1.12`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- `git diff --check --` on the changed passive-related files passed with only line-ending warnings.

### History

- 2026-05-18: Code Builder added CSV-backed enemy passive IDs/values and synced them into runtime enemy models.

## Task: 2026-05-21 Explicit Target Selection SingleAttack Data Mapping

### Task title

Respect `target_selection` when mapping zero-radius SingleAttack CSV rows.

### Goals

- Keep legacy zero-radius SingleAttack rows with blank `target_selection` able to cover all targets.
- Let explicit target-selection rows such as Ariel-D route as one-target SingleAttack skills.

### Constraints

- Role Owner is Code Builder.
- No CSV data values were changed in this follow-up.
- The behavior is grounded in the active runtime CSV row and mapper code.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- For future zero-radius SingleAttack rows, leave `target_selection` blank only when full coverage is intended.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` Ariel-D row contains `radius=0` and `target_selection=HighestHealth`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now sets `single.Area.CoverAll = source.Radius <= 0f && string.IsNullOrWhiteSpace(source.TargetSelection)`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-21: User reported Ariel-D looked like it hit all enemies; Builder fixed the data-to-runtime cover-all mapping so explicit `target_selection` wins over zero radius.

## Task: 2026-05-22 Skill Choice Beam Width Bonus

### Task title

Add CSV-backed `beam_width_bonus` for beam/line skill width upgrades.

### Goals

- Separate beam width upgrades from radius upgrades.
- Preserve Eve-B trait 2's damage +30% while moving 광선 폭 +30% to a dedicated field.
- Carry the new CSV field into runtime `SkillExecutionSnapshot`.

### Constraints

- Role Owner is Code Builder.
- Existing non-beam width notes that are still marked unsupported were not remapped.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve-B trait 2 in Play Mode: damage remains +30%, beam/line width increases by +30%.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now has `beam_width_bonus` after `max_health_bonus`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` row `eve-b-trait-2` now has `damage_multiplier=1.3`, blank `radius_multiplier`, and `beam_width_bonus=0.3`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` reads `beam_width_bonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, `SkillDefinition.cs`, `SkillChoiceEffectSpec.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSnapshot.cs` carry `BeamWidthBonus` into skill execution.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-22: User requested `Beam_Width_Bonus`-style enhancement support so 광선 폭 +30% scales beam effect width instead of using radius fields.
