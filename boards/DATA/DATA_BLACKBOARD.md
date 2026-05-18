## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/DATA_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older CSV-transition history remains in `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current runtime CSV authority, cleanup decisions, and archive destinations still needed for ongoing work.

## Task: 2026-05-18 Active Runtime CSV Authority

### Task title

Keep the current Scripts2 runtime CSV authority explicit and compact.

### Goals

- Keep active runtime authority on `Assets/CSVdata/source/*.csv`, `Assets/CSVdata/EnemySkillData.csv`, and `Assets/CSVdata/SkillChoiceModifierData.csv`.
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

Current active CSV authority summarized and retained for future work. 2026-05-18 Code Builder moved monster projectile/status tuning out of `monsters.csv` and into per-skill rows in `monster_skills.csv`. 2026-05-18 Code Builder added a one-command CSV runtime sync batch path and status-column validation/fallback for supported status labels.

### Next Actions

- If future cleanup resumes, continue from this active runtime-authority split instead of reviving archived duplicate CSV tables.
- When CSV ownership changes, update this file together with `boards/RUN/RUN_BLACKBOARD.md`, `boards/COMBAT/ENEMY_BLACKBOARD.md`, and `boards/REPORT/REPORT_BLACKBOARD.md`.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` carries the active enemy authored rows, including the current `basic_skill` plus `stage_one_skill` split.
- `Pakuri/Assets/CSVdata/EnemySkillData.csv` carries active enemy skill tuning rows.
- `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` explicitly carries reward target skill fields plus `linked_choice_id`.
- `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` remains the active runtime modifier table after dead range columns were removed.
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
- `Pakuri/Assets/Scripts2/InGame/UI/OfferingUI.cs` now uses CSV-backed `DisplayName`, `Title`, `DescriptionText`, `Summary`, and IDs for Offering choice text instead of broken hardcoded fragments.

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
