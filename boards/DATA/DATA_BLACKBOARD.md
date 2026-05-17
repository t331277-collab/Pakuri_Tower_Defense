## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-09` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/DATA/DATA_BLACKBOARD.md`.

## Task: 2026-05-17 EnemySkillData CSV Runtime Split

### Task title

Split Stage 1 enemy active skill tuning into `EnemySkillData.csv`.

### Goals

- Create an enemy skill CSV with the current seven active Stage 1 enemy skills plus source-only Archer `AimedShot`.
- Keep `EnemySkillData.csv` close to the existing monster skill CSV shape while adding only enemy-specific runtime fields needed by the current loader.
- Change the runtime CSV loader so `stage_one_enemies.csv` carries the enemy row and skill ID, while skill name, coefficient, cooldown, duration, radius, and flat value come from `EnemySkillData.csv`.

### Constraints

- Role Owner is Code Builder.
- Active `EnemyStat.csv` still has seven enemy rows; Archer was not added there because it is only present in runtime source data.
- Active `EnemyStat.csv` now keeps `active_skill_id` references but no longer keeps enemy active skill tuning columns such as `active_skill_coefficient`.
- `ChargeCommand` duration/radius/cooldown moved into CSV, but its current speed and outgoing damage multipliers remain hardcoded in `EnemyCombatSimulationSystem.ExecuteChargeCommand(...)`.
- No Play Mode verification was run by Codex.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies in Play Mode that Stage 1 enemies still execute their skills with the same behavior.
- If enemy skill behavior grows beyond the current fields, add explicit CSV columns before moving more hardcoded runtime constants.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- Added `Pakuri/Assets/CSVdata/EnemySkillData.csv` with 8 rows: `Slash`, `ShieldUp`, `AimedShot`, `ShurikenThrow`, `Heal`, `GuardianFlag`, `ChargeCommand`, and `SacredSwordWave`.
- `Import-Csv Pakuri\Assets\CSVdata\EnemySkillData.csv | Select-Object -Skip 1` returned `EnemySkillRows=8`.
- `Import-Csv Pakuri\Assets\CSVdata\EnemyStat.csv` returned `ActiveEnemyRows=7`, `ActiveHasSkillCoefficientColumn=False`, and skill IDs `Slash`, `ShieldUp`, `ShurikenThrow`, `Heal`, `GuardianFlag`, `ChargeCommand`, and `SacredSwordWave`; no Archer row was added to active `EnemyStat.csv`.
- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` now has 8 enemy rows with `stage_one_skill` references only and no active skill tuning columns.
- CSV consistency check returned `MissingStageSkillRefs=` empty and all 8 `EnemySkillData.csv` prefab paths existed under `Pakuri/Assets/Prefab/Enemy/Skill`.
- `PakuriCsvRuntimeData.Loader.cs` now loads `EnemySkillData.csv` through `PakuriCsvRuntimeSourceCatalog.EnemySkills` and applies the matching skill row while parsing enemy rows.
- `PakuriCsvRuntimeData.EnemyDataset.cs` now parses `EnemySkillRow` and copies skill name, coefficient, cooldown, duration, radius, and flat value into `EnemyRow`.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` regenerated `PakuriCsvRuntimeAssetCatalog.asset` with the 8 enemy skill prefab paths.
- Runtime build `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Editor build `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity menu `Pakuri/Validate CSV Source Data` produced no CSV validation errors in the warning/error console read; only MCP client handler logs remained.

### History

- 2026-05-17: User asked Code Builder to create `EnemySkillData.csv`, migrate the seven active enemy skills plus source-only `AimedShot`, and change the loader without adding absent enemy rows.

## Task: 2026-05-17 Projectile Blueprint Numeric Evidence Priority

### Task title

Record fallback order for projectile and enemy numeric evidence.

### Goals

- Update the projectile blueprint so future projectile implementation does not invent missing tuning numbers.
- Require active CSV checks before reference-document fallback when the user does not provide exact values.
- Record current monster skill CSV coverage.

### Constraints

- Role Owner is Designer.
- Documentation and evidence check only; no C# script, prefab, scene, or CSV data values were changed in this task.
- Active `SkillData.csv` coverage and runtime source `monster_skills.csv` coverage are different and must not be conflated.

### Role Owner

Designer

### Status

Blueprint update completed and file checks passed.

### Next Actions

- Code Builder should follow `boards/SkillBluePrint/projectile-blueprint.md` numeric evidence priority before future projectile edits.
- Add missing active `SkillData.csv` rows later when broad monster skill data entry resumes.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` now says to check `Pakuri/Assets/CSVdata/SkillData.csv` for skill values and `Pakuri/Assets/CSVdata/EnemyStat.csv` for enemy values first, then runtime source CSV files, then `Pakuri/reference/2.Monster` or `Pakuri/reference/5.enemy`.
- Reference monster skill file scan found `ReferenceMonsterSkillFiles=50`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` check returned `SourceMonsterSkillRows=50`, with `ariel:10`, `eve:10`, `rin:10`, `sein:10`, and `vega:10`, and no missing IDs from the 50 reference skill files.
- `Pakuri/Assets/CSVdata/SkillData.csv` and `Pakuri/Assets/CSVData/SkillData.csv` each currently contain only `eve-a`, `ariel-a`, and `ariel-b`, so 47 of the 50 monster skill IDs are not present in the active SkillData tables.
- `Pakuri/Assets/CSVdata/EnemyStat.csv` and `Pakuri/Assets/CSVData/EnemyStat.csv` each contain 7 Stage 1 enemy rows with active skill IDs `Slash`, `ShieldUp`, `ShurikenThrow`, `Heal`, `GuardianFlag`, `ChargeCommand`, and `SacredSwordWave`.

### History

- 2026-05-17: User asked to update the projectile blueprint with CSV-first numeric evidence lookup and asked whether current monster skills lack CSV data.

## Task: 2026-05-17 Ariel-A Projectile Data Alignment

### Task title

Record Ariel-A active skill data and runtime source prefab path.

### Goals

- Add Ariel-A to the active `SkillData.csv` skill table.
- Connect the runtime source `monster_skills.csv` row to the authored Ariel-A prefab path.
- Keep the data record clear about unsupported Ariel-A special/master behavior.

### Constraints

- Role Owner is Code Builder.
- Current runtime still resolves skill definitions through `PakuriCsvRuntimeData` / `monster_skills.csv`; the active `SkillData.csv` row is alignment and future-source data, not the only current runtime source.
- Current source schema has no base pierce-count or per-skill projectile-speed columns, so the runtime mapper uses an Ariel-A-specific mapping for pierce `1` and speed `17`.
- No Play Mode verification was run by Codex.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally checked.

### Next Actions

- Add first-class source schema fields for base pierce count and per-skill projectile speed before broad projectile data entry depends on those values.
- Add modifier/runtime support before treating Ariel-A White Judgement explosions, holy exposure, or shielded-ally damage scaling as implemented.

### Evidence

- `Pakuri/Assets/CSVData/SkillData.csv` now includes `ariel-a` with `ProjectileSkillData`, `MagazineProjectile`, Holy damage, base damage `18`, spell coefficient `1`, magazine `7`, reload `4.6`, shot interval `0.36`, pierce `1`, projectile speed `17`, and source notes from `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `ariel-a` `skill_effect_prefab_path=Assets/Prefab/Skill/Ariel/Airel_A.prefab`.
- CSV check returned `SkillDataUpperRows=3`, `UpperA=ariel-a`, `Pierce=1`, `Speed=17`, `SourcePrefab=Assets/Prefab/Skill/Ariel/Airel_A.prefab`, `SourceMagazine=7`, `SourceReload=4.6`, and `SourceShot=0.36`.
- Runtime/editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-17: User asked Code Builder to implement Ariel-A and supplied the `Airel_A.prefab` path.

## Task: 2026-05-16 NewRunScene Stage CSV Design Check

### Task title

Decide whether new Stage CSV files are needed for Stage Flow implementation.

### Goals

- Inspect existing `Pakuri/Assets/CSVdata` Stage-like CSV files before adding new data files.
- Avoid hardcoding Stage/Day/Encounter/Reward rules into the future Stage Flow manager.
- Separate active runtime data needs from reference/prototype wave CSV files.

### Constraints

- Role Owner is Designer.
- No CSV, C# script, scene, prefab, or loader implementation was changed in this task.
- Current `Scripts2/InGame` runtime still uses hardcoded `NewRunSceneEntryManager` enemy spawn sequencing and legacy catalog resolution, not a Stage CSV loader.

### Role Owner

Designer

### Status

Design decision recorded; ready for Code Builder handoff when the user asks for implementation.

### Next Actions

- Add a small active Stage Flow CSV set before implementing non-hardcoded NewRunScene Stage Flow.
- Treat existing `waves*.csv` files as non-authoritative prototype/reference data unless a loader is explicitly connected.
- If Code Builder implements Stage Flow, update Run, Data, Enemy, and UI boards together.

### Evidence

- `Get-ChildItem Pakuri\Assets\CSVdata -Recurse -File -Filter 'Stage*.csv'` returned no files.
- `Pakuri/Assets/CSVdata` currently contains active-looking CSV files `EnemyStat.csv`, `MonsterStat.csv`, `SkillData.csv`, `SkillChoiceData.csv`, `SkillChoiceModifierData.csv`, plus `waves.csv`, `waves_chapter1.csv`, `waves_chapter2.csv`, `waves_chapter3.csv`, and `waves_runtime.csv`.
- `Import-Csv Pakuri\Assets\CSVdata\waves.csv` parsed rows with old placeholder enemy IDs such as `ENEMY_001`, while active enemy data uses IDs such as `stage1-swordsman` in `EnemyStat.csv`.
- Repository text search for `waves.csv`, `waves_chapter1`, `waves_chapter2`, `waves_chapter3`, `waves_runtime`, `wave_id`, and `encounter_id` found CSV headers and legacy duplicate CSV files, but no C# runtime loader or manager consuming those wave files.
- C# search found the existing legacy CSV runtime path expects `Assets/CSVdata/source/catalog_monsters.csv`, `catalog_stage_one_enemies.csv`, and `stage_one_enemies.csv`, not the current `waves*.csv` files.

### History

- 2026-05-16: User asked whether new Stage-related CSV files should be created under `Assets/CSVdata` to avoid hardcoding Stage Flow, and asked to inspect existing Stage-like CSV files first.

## Task: 2026-05-16 NewRunScene Active Stage CSV Seeds

### Task title

Create active Stage Flow CSV seeds for day progression, encounter composition, and reward rules.

### Goals

- Add non-hardcoded CSV seeds for NewRunScene Stage 1 Day 1 through Day 11.
- Keep day flow, encounter composition, and reward payout separated into distinct active CSV files.
- Reference the current active enemy IDs from `EnemyStat.csv`.

### Constraints

- Role Owner is Code Builder.
- CSV assets only; no C# loader, scene wiring, UI wiring, prefab change, or Play Mode verification was done.
- Active scope is Stage 1 only because the current inspected `EnemyStat.csv` contains Stage 1 enemy IDs and no Stage 2~4 enemy rows.
- Event and shop are intentionally disabled in the active `StageDay.csv` rows per current user scope.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV consistency verified.

### Next Actions

- Implement a Stage Flow CSV parser/loader that consumes `StageDay.csv`, `StageEncounter.csv`, and `StageReward.csv`.
- Replace the fixed `NewRunSceneEntryManager.SpawnInitialEnemySequence()` path with data-driven Stage encounter spawning.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- Added `Pakuri/Assets/CSVdata/StageDay.csv` with 11 Stage 1 day rows and columns for `combat_type`, `encounter_id`, `reward_rule_id`, elite/shop/event flags, and notes.
- Added `Pakuri/Assets/CSVdata/StageEncounter.csv` with 30 encounter rows referencing `stage1-swordsman`, `stage1-shieldbearer`, `stage1-rogue`, `stage1-priest`, `stage1-guardian-captain`, `stage1-attack-captain`, and `stage1-hero-karin`.
- Added `Pakuri/Assets/CSVdata/StageReward.csv` with Stage 1 normal, elite, day 5 midboss, day 10 midboss, and boss reward rule rows.
- Added `.meta` files for all three CSV assets.
- PowerShell `Import-Csv` consistency check returned `StageDayRows=11`, `EncounterRows=30`, `RewardRows=5`, `MissingEncounterRefs=0`, `MissingRewardRefs=0`, and `MissingEnemyRefs=0`.
- `git diff --check -- Pakuri\Assets\CSVdata\StageDay.csv Pakuri\Assets\CSVdata\StageEncounter.csv Pakuri\Assets\CSVdata\StageReward.csv Pakuri\Assets\CSVdata\StageDay.csv.meta Pakuri\Assets\CSVdata\StageEncounter.csv.meta Pakuri\Assets\CSVdata\StageReward.csv.meta` completed with no output.
- Unity-MCP `refresh_unity` completed with `resulting_state=idle`; console warning/error read still showed an existing missing `Assets/CSVdata/source/catalog_monsters.csv` auto-sync warning and a `NullReferenceException` entry without stack detail.
- Follow-up after StageManager implementation: `StageEncounter.csv` day 11 guaranteed-prisoner flags were corrected so only one boss prisoner is guaranteed, matching the "at least one boss from the boss pool" rule.
- Follow-up consistency check returned `StageDayRows=11`, `EncounterRows=30`, `RewardRows=5`, `MissingEncounterRefs=0`, `MissingRewardRefs=0`, `MissingEnemyRefs=0`, and `Day11GuaranteedPrisoners=1`.

### History

- 2026-05-16: User approved creating active CSV files for "날짜 진행", "전투 구성", and "보상 규칙".

## Task: 2026-05-16 Stage-One Remaining Enemy CSV Rows

### Task title

Assign remaining requested stage-one enemy data into `Assets/CSVData/EnemyStat.csv`.

### Goals

- Add Shield, Guardian Captain, Attack Captain, and Hero Karin rows from `reference/5.enemy/stage-1-enemies.md`.
- Preserve Rogue's existing row and skill assignment.
- Keep CSV values parseable by PowerShell `Import-Csv`.

### Constraints

- Role Owner is Code Builder.
- Current `Scripts2/InGame` enemy definitions still resolve through the existing legacy catalog/data path; this task records CSVData assignment and scene/runtime wiring but does not replace the loader.
- No Unity Play Mode verification was run by Codex.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Later CSVData loader work should make `Assets/CSVData/EnemyStat.csv` authoritative for these rows before claiming runtime data comes directly from the new CSVData file.
- User verifies NewRunScene Play Mode behavior using the scene's assigned legacy catalog assets and prefabs.

### Evidence

- `Pakuri/reference/5.enemy/stage-1-enemies.md` was inspected for the requested enemy stats, defenses, passives, and skills.
- `Pakuri/Assets/CSVData/EnemyStat.csv` now includes `stage1-shieldbearer`, `stage1-guardian-captain`, `stage1-attack-captain`, and `stage1-hero-karin`.
- `Import-Csv Pakuri\Assets\CSVData\EnemyStat.csv` returned the requested rows with skill IDs `ShieldUp`, `ShurikenThrow`, `GuardianFlag`, `ChargeCommand`, and `SacredSwordWave`.
- Runtime/editor builds passed with 0 errors and existing assembly reference warnings after the related runtime skill implementation.

### History

- 2026-05-16: User asked Code Builder to assign remaining enemy data from the stage-one enemy reference into CSV and connect their skills/prefabs through the existing structure.

## Task: 2026-05-16 SkillData Range Removal

### Task title

Remove skill range as an InGame CSV/runtime concept.

### Goals

- Delete the `range` column from `Pakuri/Assets/CSVData/SkillData.csv`.
- Keep InGame skill targeting map-wide by ignoring source `SkillDefinition.Range`.
- Ignore future range modifier columns in skill choice modifier data.
- Keep local compile and CSV parsing checks clean.

### Constraints

- Role Owner is Code Builder.
- This task changes the InGame/Scripts2 skill execution path and CSVData seed file only.
- Legacy `Assets/Legacy` combat range logic is not claimed changed by this task.
- No Unity Play Mode gameplay verification was run by Codex.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies in NewRunScene Play Mode that Auto targeting now selects enemies across the whole map.
- If later CSVData loader work adds a direct `SkillData.csv` parser, keep `range` unsupported or ignored.

### Evidence

- `Pakuri/Assets/CSVData/SkillData.csv` now has no `range` header/property; `Import-Csv` returned `eve-a` and `ariel-b` with `target_shape`, `radius`, `cover_all`, and `projectile_speed` but no `range` property.
- `InGameSkillDefinitionMapper.cs` maps `source.Range` to ignored `Targeting.Range = 0f` and no longer copies range into `BeamLength`.
- `InGameSkillDataValidator.cs` no longer validates negative/missing source range and no longer requires positive projectile range.
- `SkillChoiceModifierRecord.cs` and `SkillExecutionSnapshot.cs` no longer parse/apply `range_multiplier` or `range_bonus`.
- Runtime/editor builds passed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP refresh reached idle and console warning/error read returned only MCP client handler logs.

### History

- 2026-05-16: User requested that all skills have no range concept, `SkillData.csv` remove range, future skill range info be ignored, and Auto target the whole map.

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
## Task: 2026-05-16 Stage Reward CSV Manifest Chance

### Task title

Record active StageReward Manifest probability data.

### Goals

- Keep Manifest success/failure probability out of hardcoded UI logic.
- Add active CSV data for the 70% success / 30% failure prisoner Manifest rule.

### Constraints

- Role Owner is Code Builder.
- The active CSV set remains under `Pakuri/Assets/CSVdata`.
- User owns Play Mode validation of random outcome feel.

### Role Owner

Code Builder

### Status

Implemented and locally checked.

### Next Actions

- Tune `manifest_success_chance` in `Pakuri/Assets/CSVdata/StageReward.csv` if later design changes require different rates by combat type or stage.

### Evidence

- Changed `Pakuri/Assets/CSVdata/StageReward.csv` to include `manifest_success_chance`.
- Changed `Pakuri/Assets/Scripts2/InGame/Core/NewRunStageManager.cs` to parse `manifest_success_chance` with `0.7f` fallback.
- CSV check returned `RewardRows=5; ManifestChanceColumn=True; BadManifestChanceRows=0; MissingRewardRefs=0; EncounterRows=30`.

### History

- 2026-05-16: User requested the 70% Manifest success probability be recorded in CSV.
- 2026-05-16: Builder added the CSV column and parser exposure for UI use.

## Task: 2026-05-17 Eve A-J CSV Choice Expansion

### Task title

Expand active CSV data for Eve A-J skills and Offering choices.

### Goals

- Enter Eve A-J skill rows from `Pakuri/reference/2.Monster/eve/skill`.
- Keep runtime source choices, metadata choices, modifier rows, and Offering reward rows ID-consistent.
- Record unsupported modifier semantics explicitly instead of silently inventing runtime fields.

### Constraints

- Role Owner is Code Builder.
- The current modifier schema supports damage, magazine, projectiles, pierce, reload/shot interval, radius, duration, branch, and status fields; passive conditional damage, resistance debuffs, cooldown modifiers, freeze duration, and vulnerable-stack conditions are recorded as unsupported notes.
- No Play Mode verification was run by Codex.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Add explicit runtime/data fields for cooldown modifiers, resistance debuffs, passive conditional damage, freeze duration, vulnerable-stack conditions, and shield/action-speed effects before claiming those effects are fully executable.
- User verifies Offering choice flow in Play Mode.

### Evidence

- Changed `Pakuri/Assets/CSVdata/source/monster_skills.csv`.
- Changed `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`.
- Changed `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv`.
- Changed `Pakuri/Assets/CSVdata/SkillChoiceData.csv`.
- Changed `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv`.
- CSV consistency check returned `EveSkillRows=10; Active=5; Passive=5; ChoiceData=50; SourceChoices=50; Modifiers=50; EveRewards=50; MissingChoiceMods=0; MissingRewardChoices=0; MissingSourceChoices=0; BadEveRewards=0; BadNumeric=0`.
- Runtime/editor builds completed with 0 errors and existing assembly reference warnings.
- 2026-05-17 follow-up: Fixed malformed Eve A-J `monster_skills.csv` rows that had shifted columns and caused Unity CSV enum errors such as row 43 `attribute='?꾩갹 6'`.
- Follow-up CSV validation returned `Headers=26; Rows=50; EveRows=10; Bad=0; EveAAttribute=Lightning; EveABaseDamage=24; EveDImplementation=RuntimeImplemented; EveDRequiredSlot=A`.
- Follow-up runtime/editor builds completed with 0 errors and existing assembly reference warnings; Unity refresh reached idle and console showed only MCP client logs.
- 2026-05-17 follow-up: Fixed Eve default skill name validation by changing `monster_skills.csv` Eve slot A `display_name` to `아크 볼트` and slot F `display_name` to `전압 보정`, matching `monsters.csv` `active_skill_name` and `passive_skill_name`.
- Follow-up exact-name check returned `ANameMatch=True`, `FNameMatch=True`; quote-aware CSV parsing returned `ExpectedColumns=26`, `TotalRows=52`, `BadRows=0`.
- Follow-up runtime/editor builds completed with 0 errors and existing assembly reference warnings; Unity refresh reached idle and console showed no `Pakuri CSV source validation failed` errors.

### History

- 2026-05-17: User asked Code Builder to fill Eve A-J data first so skill acquisition and enhancement can be mapped through Offering.
- 2026-05-17: User reported Unity CSV enum errors from malformed Eve rows; Builder replaced Eve A-J rows with fresh 26-column records.
- 2026-05-17: User reported Eve active/passive default skill display-name validation errors; Builder aligned Eve A/F display names with `monsters.csv`.

## Task: 2026-05-16 NewRunScene CSV Spawn And Runtime Source Fix

### Task title

Fix NewRunScene spawn data coordinates and missing runtime CSV source imports.

### Goals

- Keep NewRunScene enemy spawn positions data-driven through `StageEncounter.csv`.
- Align StageEncounter spawn coordinates with the authored `NewRunScene` camera/spawn point coordinate space.
- Restore the imported CSV source folder expected by `PakuriCsvRuntimeData`.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Treat Unity console claims as evidence only when the stack/file path points to project code or Unity internals explicitly.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that entering `NewRunScene` spawns stage 1-1 enemies on-screen.
- If another runtime exception appears, inspect the new stack trace and prefer project-code frames over old UnityEditor.Graphs console entries.

### Evidence

- Unity-MCP scene inspection showed `NewRunScene` has `GameManager` with `NewRunSceneEntryManager`, `InGameCombatManager`, and `NewRunStageManager`.
- Unity-MCP scene inspection showed `SpawnPoint` at world position `x=9.02, y=0, z=0`; current `StageEncounter.csv` previously used `spawn_x=31` and `spawn_y_min/max=0..17`.
- Changed `Pakuri/Assets/CSVdata/StageEncounter.csv` so all 30 encounter rows use `spawn_x=9.02`, normal rows use `spawn_y_min=-5` and `spawn_y_max=5`, and guaranteed boss rows use `0..0`.
- CSV check returned `Rows=30; SpawnX=9.02; MinY=-5; MaxY=5`.
- Unity console contained a project-code CSV error: `Required imported CSV TextAsset is missing at 'Assets/CSVdata/source/catalog_monsters.csv'` from `PakuriCsvRuntimeData.Editor.cs:89` and `PakuriCsvRuntimeCatalogPostprocessor.cs:84`.
- Copied required source CSVs from existing `Pakuri/Assets/Legacy/CSVdata/source` to the code-expected `Pakuri/Assets/CSVdata/source`.
- After clearing the Unity console and forcing asset refresh, no `Pakuri CSV runtime catalog auto-sync failed` error reappeared; only MCP client handler logs remained.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.

### History

- 2026-05-16: User reported that entering `NewRunScene` did not visibly spawn enemies and pasted a UnityEditor.Graphs NullReferenceException.
- 2026-05-16: Builder found StageEncounter spawn coordinates were off-screen for the current scene and fixed the active CSV data.
- 2026-05-16: Builder found and fixed the missing active `Assets/CSVdata/source` CSV imports required by the runtime CSV catalog auto-sync.
