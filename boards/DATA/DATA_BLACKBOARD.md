## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/DATA_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older CSV-transition history remains in `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current runtime CSV authority, cleanup decisions, and archive destinations still needed for ongoing work.

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
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` now shows only Eve's supported runtime statuses with positive `status_chance`: `eve-a shock 0.15`, `eve-b slow 0.2`, `eve-c chill 1`, `eve-d shock 1`, and `eve-e vulnerable 1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain. A first parallel runtime/editor build hit only an `obj\Debug\Assembly-CSharp.dll` file lock, then runtime passed when rerun alone.

### History

- 2026-05-18: EffectManager scene authority, reward/choice separation, enemy dual-skill CSV authority, and inactive root CSV archiving were recorded as the current active data baseline.
- 2026-05-18: Code Builder consolidated monster projectile/status tuning into `monster_skills.csv`, removed duplicate/visual projectile columns from `monsters.csv`, and removed Ariel-A/Eve-A hardcoded projectile/status values from the shared mapper/executor path.
- 2026-05-18: Code Builder added `SyncCsvRuntimeCatalogs.bat`, exposed `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` for Unity batchmode, normalized unsupported design-only monster status labels to `status_chance=0`, and verified sync/validation through the open Unity Editor.
- 2026-05-19: Code Builder first added shared-projectile-compatible `rin-a` modifier coverage, then unified monster choice runtime data into `monster_skill_choices.csv` / `monster_modifier_skill_choice.csv` and kept crit-only / every-third-hit chain behavior explicitly unsupported where current Scripts2 runtime still has no matching contract.

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
