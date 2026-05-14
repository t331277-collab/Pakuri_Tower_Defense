## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-07` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/DATA/CSV_BLACKBOARD.md`.

## Task: 2026-05-07 Vega Active Skill CSV Runtime State

### Task title

Mark Vega active skills A-E runtime state in source CSV.

### Goals

- Keep `monster_skills.csv` aligned with Vega A-E runtime implementation.
- Classify B/C/D/E with their concrete runtime kinds instead of generic projectile placeholders.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Future Vega data work should compare `monster_skills.csv`, `vega.asset`, and Unity runtime catalog output.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now marks `vega-a` through `vega-e` as `RuntimeImplemented`.
- The same CSV rows classify `vega-b` as `LineAttack`, `vega-c` as `Buff`, `vega-d` as `AreaAttack`, and `vega-e` as `Execute`.
- Unity-MCP runtime catalog inspection confirmed Vega A-E resolve with those runtime kinds and `RuntimeImplemented` state.

### History

- 2026-05-07: Code Builder updated Vega A-E CSV runtime state during active skill implementation.

## Task: 2026-05-07 Vega Passive Skill CSV Runtime State

### Task title

Mark Vega passive skills F-J runtime state in source CSV.

### Goals

- Keep `monster_skills.csv` aligned with Vega F-J runtime implementation.
- Ensure Unity CSV runtime catalog resolves Vega A-J as runtime implemented.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Future Vega data work should compare `monster_skills.csv`, `vega.asset`, and Unity runtime catalog output.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:48` through `:52` now mark `vega-f`, `vega-g`, `vega-h`, `vega-i`, and `vega-j` as `RuntimeImplemented`.
- Unity-MCP `execute_code` synced CSV runtime catalogs and confirmed Vega F-J resolve as `RuntimeImplemented`.
- `git diff --check -- Pakuri\Assets\CSVdata\source\monster_skills.csv` completed with CRLF warnings only.

### History

- 2026-05-07: Code Builder updated Vega F-J CSV runtime state during passive skill implementation.

## Task: 2026-05-07 Vega Projectile Sprite CSV Runtime State

### Task title

Update Vega `monsters.csv` projectile sprite path to the new assigned projectile sprite.

### Goals

- Replace the old Vega projectile sprite path in the active CSV source.
- Regenerate the runtime asset catalog from the corrected CSV source.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Use `Pakuri/Assets/CSVdata/source/monsters.csv` as the normal edit point for future monster unit/projectile sprite changes.

### Evidence

- Before the fix, `Pakuri/Assets/CSVdata/source/monsters.csv:7` contained `Assets/Image/Monster/Vega/Vega_Shoot_Temp.png`.
- `Pakuri/Assets/CSVdata/source/monsters.csv:7` now contains `Assets/Image/Monster/Vega/Vega_Shoot2.png`.
- Unity-MCP `execute_code` ran `PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor()` and reported `new=True` for `Assets/Image/Monster/Vega/Vega_Shoot2.png` and `old=False` for `Assets/Image/Monster/Vega/Vega_Shoot_Temp.png`.
- `git diff --check -- Pakuri\Assets\CSVdata\source\monsters.csv Pakuri\Assets\Resources\Pakuri\CSVRuntime\PakuriCsvRuntimeAssetCatalog.asset` completed with CRLF warnings only.

### History

- 2026-05-07: User reported Vega still used the old projectile sprite after assigning a new sprite on the SO.

## Task: 2026-05-07 Ariel A CSV Row Repair

### Task title

Repair only the `ariel-a` row in `monster_skills.csv`.

### Goals

- Fix the row that caused `runtime_kind` to read as `RuntimeImplemented`.
- Restore Ariel A display text from the Ariel skill reference without changing other skill rows.

### Constraints

- Role Owner is Code Builder.
- Edit scope is only `ariel-a` in `Pakuri/Assets/CSVdata/source/monster_skills.csv`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented for `ariel-a`; Unity CSV sync now advances past row 3 and stops on another broken row 16.

### Next Actions

- If requested separately, repair row 16 (`eve-d`) or other mojibake-damaged rows using their reference documents.
- Prefer editing CSV with UTF-8 capable tools and preserving the two-row header/type schema.

### Evidence

- Before the fix, `Pakuri/Assets/CSVdata/source/monster_skills.csv` row 3 had mojibake text merged into the `display_name` field, causing `runtime_kind` to parse as `RuntimeImplemented`.
- `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md` identifies Ariel A as `심판의 빛` and describes it as `아리엘의 기본 신성 투사체. 적에게 빛의 탄환을 날려 신성 피해를 준다.`
- PowerShell `Import-Csv -Encoding UTF8` verified `ariel-a` now has `display_name=심판의 빛`, `runtime_kind=MagazineProjectile`, `implementation_state=RuntimeImplemented`, `required_active_slot=A`, `attribute=Holy`, and `base_damage=18`.
- Unity-MCP `refresh_unity` completed successfully after the file edit.
- Unity-MCP `PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor()` no longer reports row 3; it now reports row 16 with `runtime_kind=RuntimeImplemented`, confirming `ariel-a` was no longer the blocking row.

### History

- 2026-05-07: User asked to fix only the erroring `ariel-a` row after a CSV enum error and mojibake discussion.

## Task: 2026-05-07 Eve D CSV Row Repair

### Task title

Repair row 16, `eve-d`, in `monster_skills.csv`.

### Goals

- Fix the row that caused `runtime_kind` to read as `RuntimeImplemented`.
- Restore Eve D's display name and runtime-kind column placement from the Eve skill reference.

### Constraints

- Role Owner is Code Builder.
- Edit scope is only `eve-d` in `Pakuri/Assets/CSVdata/source/monster_skills.csv`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented for `eve-d`; Unity CSV sync now advances past row 16 and stops on another damaged row 23.

### Next Actions

- If requested separately, repair row 23 (`rin-a`) or other mojibake-damaged rows using their reference documents.
- Keep CSV edits in a UTF-8 capable editor and preserve the two-row header/type schema.

### Evidence

- Before the fix, `Pakuri/Assets/CSVdata/source/monster_skills.csv` row 16 had mojibake text merged with `AreaAttack` in the `display_name` field, causing `runtime_kind` to parse as `RuntimeImplemented`.
- `Pakuri/reference/2.Monster/eve/skill/d-static-override.md` identifies Eve D as `스태틱 오버라이드`, with `기본 번개 피해=10`, `주문력 계수=0.7`, `범위=3.5`, and `쿨다운=7.0초`.
- `Pakuri/Assets/Scripts/Data/SkillDefinition.cs` defines `AreaAttack` under `SkillRuntimeKind` and `RuntimeImplemented` under `SkillImplementationState`, so the failure was column drift, not a missing enum value.
- PowerShell `Import-Csv -Encoding UTF8` verified `eve-d` now has `display_name=스태틱 오버라이드`, `runtime_kind=AreaAttack`, `implementation_state=RuntimeImplemented`, `attribute=Lightning`, `base_damage=10`, `spell_power_coefficient=0.7`, `radius=3.5`, `cooldown_seconds=7`, and `status_effect_id=감전`.
- Unity-MCP `refresh_unity` completed successfully after the file edit.
- Unity-MCP `AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/CSVdata/source/monster_skills.csv")` returned the corrected row 16 text.
- Unity-MCP `PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor()` no longer reports row 16; it now reports row 23 with `runtime_kind=RuntimeImplemented`, confirming `eve-d` was no longer the blocking row.

### History

- 2026-05-07: User reported the row 16 CSV enum error and asked why it happened and to fix it.

## Task: 2026-05-07 Monster Skills CSV Damaged Row Repair

### Task title

Repair all remaining enum/column-drift errors in `monster_skills.csv`.

### Goals

- Remove all remaining `runtime_kind`, `implementation_state`, and `attribute` enum parse failures from the active monster skill CSV.
- Restore damaged display names and safe one-line descriptions from the matching monster skill reference documents.
- Preserve existing numeric tuning, slot, learned, and implementation-state values unless the row was shifted.

### Constraints

- Role Owner is Code Builder.
- Edit scope is damaged rows in `Pakuri/Assets/CSVdata/source/monster_skills.csv`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated through Unity CSV sync.

### Next Actions

- If more mojibake text should be cleaned for readability, handle it as a separate content cleanup because current parser-blocking enum errors are resolved.
- Continue editing CSV with UTF-8 capable tools and avoid unescaped commas/quotes inside fields unless they are correctly quoted.

### Evidence

- A PowerShell scan using the same quote/comma behavior as `PakuriCsvRuntimeData.CsvSupport.cs::SplitCsvLine(...)` found parser-blocking issues in `rin-a`, `rin-b`, `rin-c`, `rin-e`, `rin-j`, `vega-g`, `vega-h`, `vega-i`, and `vega-j`.
- The runtime-kind drift rows were `rin-a`, `rin-b`, `rin-c`, `rin-e`, `vega-g`, `vega-h`, and `vega-j`; their display-name cells had text merged with `MagazineProjectile`, `Buff`, `LineAttack`, `AreaAttack`, or `Passive`.
- The attribute drift rows were `rin-j` and `vega-i`; their description/summary fields had broken quote/comma structure that shifted later cells.
- Reference documents used for names and descriptions were `rin/skill/a-shattering-fist.md`, `b-howling.md`, `c-shockwave.md`, `e-collapse-strike.md`, `j-collapse-aftermath.md`, `vega/skill/g-sealing-sword-form.md`, `h-execution-prep.md`, `i-chain-cleaving.md`, and `j-executioner.md`.
- Post-edit Unity-like CSV scan reported no invalid row structure, `runtime_kind`, `implementation_state`, or `attribute` issues.
- PowerShell `Import-Csv -Encoding UTF8` verified the repaired rows now resolve expected display names and enum columns.
- Unity-MCP `refresh_unity` completed successfully.
- Unity-MCP `PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor()` returned `CSV sync ok`.
- Unity-MCP console read after clearing showed only MCP client handler logs, not Pakuri CSV parse errors.

### History

- 2026-05-07: User asked to fix every row that still caused CSV errors.

## Task: 2026-05-07 Skill Effect Prefab Inspector Export

### Task title

Add an editor tool that exports inspector-assigned `SkillEffectPrefab` objects to CSV prefab-path columns.

### Goals

- Let designers assign effect prefabs on `MonsterDefinition` assets through the Inspector.
- Export assigned prefab object references to `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `monster_skill_choices.csv`.
- Run the existing CSV runtime catalog sync after export so prefab paths are included in the runtime asset catalog.

### Constraints

- Role Owner is Code Builder.
- The export tool only writes non-null prefab assignments; it does not clear existing CSV paths when a SO field is null.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Assign a prefab such as `Assets/Image/Monster/Eve/Effect_Prefab/Eve_Skill_B.prefab` to `eve.asset` / `eve-b` `SkillEffectPrefab`, then click the Inspector button or run `Pakuri/Export Skill Effect Prefabs To CSV`.
- After export, verify `skill_effect_prefab_path` in the active CSV and `Prefabs` in `PakuriCsvRuntimeAssetCatalog.asset`.

### Evidence

- Added `Pakuri/Assets/Scripts/Data/Editor/PakuriSkillEffectPrefabCsvExporter.cs`.
- The new menu item is `Pakuri/Export Skill Effect Prefabs To CSV`.
- The new `MonsterDefinition` custom inspector draws the default inspector and adds an `Export Skill Effect Prefabs To CSV` button.
- The exporter scans `Assets/Data/GameData/Monsters` for `MonsterDefinition` assets and collects non-null `SkillDefinition.SkillEffectPrefab`, `PassiveDefinition.SkillEffectPrefab`, and `SkillChoiceDefinition.SkillEffectPrefab` assignments.
- The exporter writes only the `skill_effect_prefab_path` column for matching `skill_id` rows in `monster_skills.csv` and matching `choice_id` rows in `monster_skill_choices.csv`.
- Unity-MCP `manage_asset import` imported the new script and generated `.meta` guid `6bae28997ffa62349952a464f0ec97c3`.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP `execute_menu_item` successfully executed `Pakuri/Export Skill Effect Prefabs To CSV`.
- Unity console logged `Pakuri skill effect prefab export completed. skills=0, choices=0, assignedSkills=0, assignedChoices=0`, which matches the currently unassigned SO prefab fields.

### History

- 2026-05-07: User requested an editor workflow so Inspector-assigned `SkillEffectPrefab` values can be exported to CSV paths and then used by runtime effects.

# Task: 2026-05-08 Eve A Skill Effect Prefab CSV Correction

### Task title

Remove the incorrect Eve B effect prefab path from the Eve A CSV row.

### Goals

- Stop runtime CSV data from assigning `Eve_Skill_B.prefab` to `eve-a`.
- Preserve the Eve B Prism Ray prefab assignment on `eve-b`.
- Keep A projectile visuals sourced from monster projectile sprite data instead of skill effect prefab data.

### Constraints

- Role Owner is Code Builder.
- Edit scope is `Pakuri/Assets/CSVdata/source/monster_skills.csv`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Eve no longer shows the Prism Ray prefab while using A.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:13` now has an empty `skill_effect_prefab_path` cell for `eve-a`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:14` still assigns `Assets/Image/Monster/Eve/Effect_Prefab/Eve_Skill_B.prefab` to `eve-b`.
- `Select-String` confirmed the post-edit rows for `eve-a` and `eve-b`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: Investigation found that the CSV runtime source assigned the Eve B effect prefab to `eve-a`.
- 2026-05-08: Code Builder cleared the `eve-a` prefab path while leaving `eve-b` unchanged.
