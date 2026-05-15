## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-09` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/DATA/DATA_BLACKBOARD.md`.

## Task: 2026-05-15 Eve-A Skill Choice Modifier CSV Seed

### Task title

Create first CSVData choice and modifier seed rows for Eve-A.

### Goals

- Add a structured `SkillChoiceData.csv` file for Eve-A enhancement and master choices.
- Add a structured `SkillChoiceModifierData.csv` file that uses explicit modifier columns instead of a generic `value` column.
- Keep this as data groundwork only; do not implement a loader, resolver, executor, projectile branch runtime, or Play Mode behavior.

### Constraints

- Role Owner is Code Builder.
- Scope is Eve-A Arc Bolt only: five enhancement choices and two master choices from the inspected Eve reference.
- Non-applicable modifier columns are represented as `null` strings for the future parser/validator to treat as not applied.
- Reload speed and fire speed modifiers that are expressed as speed changes in the reference are stored as derived time/interval multipliers with source notes.
- The current Unity auto-sync warning for missing `Assets/CSVdata/source/catalog_monsters.csv` remains outside this task.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Later CSVData loader work should parse `SkillChoiceData.csv` and `SkillChoiceModifierData.csv`.
- Phase4-B should use these rows through `SkillChoiceResolver` / `SkillExecutionSnapshot` without mutating source `SkillData`.
- Projectile branch runtime remains later work after minimum Phase4-C skill execution is proven.

### Evidence

- Added `Pakuri/Assets/CSVdata/SkillChoiceData.csv` with seven Eve-A rows: `eve-a-trait-1` through `eve-a-trait-5`, `eve-a-master-1`, and `eve-a-master-2`.
- Added `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` with explicit columns including `damage_multiplier`, `magazine_bonus`, `additional_projectile_bonus`, `pierce_bonus`, `reload_time_multiplier`, `shot_interval_multiplier`, `branch_chance_bonus`, `branch_chance_set`, `branch_count`, `branch_damage_multiplier`, `branch_search_radius`, `status_tag`, and `status_stacks_set`.
- Added `.meta` files for both CSV assets.
- `Import-Csv Pakuri\Assets\CSVdata\SkillChoiceData.csv` returned seven rows with five `ActiveEnhancement` choices and two `ActiveMaster` choices.
- `Import-Csv Pakuri\Assets\CSVdata\SkillChoiceModifierData.csv` returned seven rows and the choice/modifier ID check reported `ChoiceRows=7`, `ModifierRows=7`, and no missing modifiers.
- `git diff --check -- Pakuri\Assets\CSVdata\SkillChoiceData.csv Pakuri\Assets\CSVdata\SkillChoiceModifierData.csv Pakuri\Assets\CSVdata\SkillChoiceData.csv.meta Pakuri\Assets\CSVdata\SkillChoiceModifierData.csv.meta` completed with no output.
- Unity-MCP `refresh_unity` returned `success=true`, `resulting_state=idle`.
- Unity-MCP console warning/error read showed the existing `Pakuri CSV runtime catalog auto-sync failed` message because `Test-Path Pakuri\Assets\CSVdata\source\catalog_monsters.csv` returned `False`; this task did not change that source folder state.

### History

- 2026-05-15: User directed Code Builder to create `SkillChoiceData.csv` and `SkillChoiceModifierData.csv` for Eve first, using explicit modifier columns such as projectile, power, width/radius, reload time, fire speed, and pierce count instead of a generic `value` column.

## Task: 2026-05-15 Stage1 Enemy CSV Type Expansion

### Task title

Track the stage-one enemy CSV expansion for Melee, Ranged, and Buffer enemies.

### Goals

- Fill `Assets/CSVData/EnemyStat.csv` with the current three enemy rows needed by NewRunScene tests.
- Keep the existing `attack_type` column as the behavior grouping field.
- Preserve loader compatibility by also aligning the legacy stage-one source row and current priest asset.

### Constraints

- Role Owner is Code Builder.
- User explicitly confirmed Rogue is `Ranged` and Priest is `Buffer`.
- The new `Assets/CSVData/EnemyStat.csv` rows are data-entry groundwork; current `Scripts2/InGame` still resolves enemies through the existing legacy catalog/data manager path.
- No new CSV loader was implemented in this task.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Later CSVData loader work should map `EnemyStat.csv.attack_type` values `Melee`, `Ranged`, and `Buffer` into `EnemyDefinition.AttackType`.
- Do not claim the new CSVData path is authoritative until `Scripts2/InGame` no longer depends on legacy `Pakuri.Data` resolution.

### Evidence

- `Pakuri/Assets/CSVData/EnemyStat.csv` now contains `stage1-swordsman`, `stage1-rogue`, and `stage1-priest`.
- `Import-Csv Pakuri\Assets\CSVData\EnemyStat.csv` returned attack types `Melee`, `Ranged`, and `Buffer`.
- `Pakuri/Assets/Legacy/CSVdata/source/stage_one_enemies.csv` now stores `stage1-priest` as `Buffer`.
- `Pakuri/Assets/Legacy/Data/GameData/Enemies/stage1-priest.asset` now stores `AttackType: 3`.
- `Pakuri/Assets/Legacy/Scripts/Data/Definition/EnemyDefinition.cs` now defines `EnemyAttackType.Buffer`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-15: User directed Code Builder to keep the existing CSV column and standardize stage-one types as `Melee`, `Ranged`, and `Buffer`.

## Task: 2026-05-14 CSVData Source Transition Roadmap

### Task title

Track the planned data-source transition from legacy CSV/Data scripts to new `Assets/CSVData` files.

### Goals

- Treat `Assets/CSVData/MonsterStat.csv`, `EnemyStat.csv`, and `SkillData.csv` as the intended future source of monster, enemy, and skill numeric data.
- Keep `Assets/Legacy` as reference-only after the actual runtime compile/reference path is removed.
- Record that reference documents under `Pakuri/reference/2.Monster` and `Pakuri/reference/5.enemy` are the manual source for filling the new CSV rows.

### Constraints

- Role Owner is Designer.
- No CSV contents were added and no runtime C# was changed in this task.
- Legacy is not considered disabled until compile targets and runtime references are removed or isolated.

### Role Owner

Designer

### Status

Completed as a design roadmap.

### Next Actions

- CSVData Phase0~2 header and minimum sample rows are implemented in `Assets/CSVData`; continue with the new CSV loader and mapping work before skill execution depends on unit data.
- Implement the new CSV loader and unit model mapping around Phase2-B / Phase2-C before skill execution depends on unit data.
- Implement `SkillData.csv` to `SkillData` subclass mapping before InGame Phase4-A through Phase4-C skill execution.
- Remove `Scripts2/InGame` dependencies on legacy `Pakuri.Data` types before claiming the new CSV path is authoritative.

### Phase0~2 Implementation Update

- `Pakuri/Assets/CSVData/MonsterStat.csv`, `EnemyStat.csv`, and `SkillData.csv` now contain Phase0~2 headers and minimum rows for Eve, Ariel, `stage1-swordsman`, `eve-a`, and `ariel-b`.
- `Import-Csv` checks over all three CSVData files parsed the new rows and returned expected IDs and key values.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-csvdata-transition-roadmap.html`.
- `Assets/CSVData/EnemyStat.csv`, `MonsterStat.csv`, and `SkillData.csv` exist and are currently empty.
- `Assets/Legacy/CSVdata`, `Assets/Legacy/Data`, and `Assets/Legacy/Scripts` exist.
- `Assembly-CSharp.csproj` still includes `Assets\Legacy\Scripts\...` compile items.
- `Scripts2/InGame` still references legacy `Pakuri.Data`, `MonsterDefinition`, `SkillDefinition`, `PakuriCsvRuntimeData`, and `PakuriDataManager` in inspected search results.
- `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html` now includes section `2-1. CSVData 파이프라인 삽입 타이밍`, placing CSVData Phase0~2 before deep Phase2-B binding, CSVData Phase3~4 around Phase2-B/Phase2-C, CSVData Phase5 before Phase4 skill execution, and Legacy deactivation before Phase8-A Run integration.

### History

- 2026-05-14: User proposed making the new `Assets/CSVData` files the future runtime source and using legacy files only as reference.
- 2026-05-14: Designer amended the InGame build roadmap to show exactly when CSVData pipeline work should be inserted into the InGame implementation order.
- 2026-05-14: Code Builder implemented CSVData Phase0~2 headers and minimum seed rows.

## Task: 2026-05-14 Eve-E Field Data Implementation

### Task title

Track data-layer Eve-E field classification implementation.

### Goals

- Record that Eve-E now leaves projectile classification and enters field/zone classification.
- Keep detailed asset evidence in `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.

### Constraints

- Role Owner is Code Builder.
- No scene, prefab, combat executor, or Play Mode changes.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Use `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md` for the detailed changed-file evidence.

### Evidence

- `monster_skills.csv` and `eve.asset` now classify Eve-E as `Field`.
- Unity-MCP Editor code execution confirmed Eve-E maps to `ZoneSkillData` with validation `errors=0|warnings=0`.
- Runtime and editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: User explicitly assigned Code Builder to change Eve-E `RuntimeKind` from `MagazineProjectile` to `Field`.

## Task: 2026-05-14 InGame Phase2-A Definition To Unit Model Mapping

### Task title

Track data-layer Phase2-A definition to base unit model mapping.

### Goals

- Record that Phase2-A reads existing monster/enemy data and creates InGame `BaseUnitRuntimeModel` family models.
- Keep CSV/Data source unchanged.
- Keep skill/projectile tuning in the existing definitions until later SkillData mapper work.
- Keep detailed evidence in `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.

### Constraints

- Role Owner is Code Builder.
- No CSV edits, asset generation, code-generated prefab changes, scene edits, or Play Mode verification.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Use `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md` for data mapping details.
- Continue using existing data loading for later InGame phases.
- Build Phase2-B around user-authored prefabs rather than generated prefab assets.

### Evidence

- `UnitFactory` resolves Eve and stage-one enemy definitions through the existing catalog/data manager flow.
- `UnitFactory` creates Eve as `MonsterUnitRuntimeModel` and `stage1-swordsman` as `EnemyUnitRuntimeModel`.
- Added `BaseUnitRuntimeModel.cs`, `MonsterUnitRuntimeModel.cs`, and `EnemyUnitRuntimeModel.cs`.
- Unity-MCP Editor code execution returned `ok=True|monster=eve|monsterHp=220|learnedA=1|enemy=stage1-swordsman|enemyHp=100`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: Phase2-A mapped existing data definitions into InGame unit models.
- 2026-05-14: User confirmed prefabs are created manually and Definition skill/projectile tuning should be split later during skill implementation.

## Task: 2026-05-14 InGame Phase1-D Skill Data Validation

### Task title

Track data-layer Phase1-D validation for InGame skill mapping.

### Goals

- Ensure skill data validation exists before full skill data expansion.
- Keep existing CSV/Data loading as the source of truth.
- Record the validation-only implementation without changing CSV rows or assets.

### Constraints

- Role Owner is Code Builder.
- No CSV edits, ScriptableObject asset creation, prefab edits, scene edits, or Play Mode verification.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Use the detailed data/asset task in `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md` for validation evidence.
- Run the Unity Editor menu `Pakuri/InGame/Validate Skill Data` when Unity is available.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDataValidator.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Editor/InGameSkillDataValidationMenu.cs`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: Code Builder implemented Phase1-D validation for InGame skill data mapping.

## Task: 2026-05-09 Assets Scripts Folder Organization

### Task title

Organize Data scripts under Definition and Runtime subfolders.

### Goals

- Make the Data script structure easier to scan from the folder tree.
- Keep data loading behavior unchanged by moving files only, with `.cs.meta` files moved together.

### Constraints

- Role Owner is Designer -> Code Builder.
- Do not change C# class names, namespaces, serialized field names, or runtime data logic.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Designer -> Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Use `Pakuri/Assets/Scripts/Data/Definition`, `Runtime`, `Runtime/Csv`, and `Editor` as the current Data script map.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Added design document `Pakuri/reference/Report/2026-05-09-assets-scripts-folder-organization-design.md`.
- Moved `EnemyDefinition.cs`, `GameDataCatalog.cs`, `MonsterDefinition.cs`, and `SkillDefinition.cs` to `Pakuri/Assets/Scripts/Data/Definition`.
- Moved `PakuriDataManager.cs`, `PakuriCsvRuntimeAssetCatalog.cs`, and `PakuriCsvRuntimeSourceCatalog.cs` to `Pakuri/Assets/Scripts/Data/Runtime`.
- Moved `PakuriCsvRuntimeData*.cs` runtime/CSV partials to `Pakuri/Assets/Scripts/Data/Runtime/Csv`.
- Kept editor-only scripts under `Pakuri/Assets/Scripts/Data/Editor`.
- Moved `.cs.meta` files with their matching `.cs` files to preserve Unity script GUIDs.
- Unity-MCP `refresh_unity` reached idle after script refresh.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings after rerunning it alone; the earlier parallel editor build failed only because the runtime build held an `obj\Debug` cache file lock.
- Unity-MCP console warning/error read returned only MCP client handler logs.

### History

- 2026-05-09: User requested organizing `Assets/Scripts` so Data and other domains are clearer from the folder structure.

## Migrated Task Blocks
