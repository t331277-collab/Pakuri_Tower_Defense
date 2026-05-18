Archived snapshot created during 2026-05-18 board cleanup.

## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-09` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/DATA/DATA_BLACKBOARD.md`.

## Task: 2026-05-18 Stage1 Enemy Basic Skill CSV Slot

### Task title

Add a CSV-authored `basic_skill` slot for Stage 1 enemies and feed it into the new runtime enemy combat path.

### Goals

- Add `basic_skill` next to `stage_one_skill` in the active `stage_one_enemies.csv`.
- Let Stage 1 enemies carry one common/basic skill plus one existing special skill from CSV.
- Keep duplicate `basic_skill == stage_one_skill` authored rows valid, but execute them as one effective skill at runtime.
- Keep enemy skill effect authority in the scene `EffectManager`, while using the new `basic_skill` CSV as the runtime skill-selection authority.

### Constraints

- Role Owner is Code Builder.
- User explicitly denied Code Reviewer execution for this task.
- Play Mode verification was not run by Codex.
- `stage_one_skill` remained the existing special/passive anchor; duplicate authored basic skills are ignored at runtime instead of nulling the special slot in source CSV.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- User verifies in NewRunScene Play Mode that shieldbearer/rogue/priest/guardian/attack-captain/karin now use both their basic and special skills with the expected cadence.
- If more Stage 1 archetypes need different common skills later, edit only `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` and the matching `EffectManager` scene entries.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` now has a `basic_skill` column and eight rows with `Slash` / `AimedShot` authored per the current user instruction.
- The same CSV was rebuilt from inspected enemy asset values so every row is 24 columns wide again; Unity no longer reports the earlier `row 3 has 23 columns but expected 24` fatal error.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.EnemyDataset.cs` now parses optional `basic_skill`, validates it against `EnemySkillData.csv`, and applies a second skill payload bundle into `EnemyRow`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/EnemyDefinition.cs`, `EnemyUnitRuntimeModel.cs`, `UnitFactory.cs`, and `PakuriCsvRuntimeData.Build.cs` now carry authored `basic_skill` data end to end into runtime enemy models.
- Unity-MCP `PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor()` returned `sync ok: monsters=5, enemies=8`.
- Unity-MCP runtime inspection returned:
  `stage1-shieldbearer: basic=True/Slash, special=ShieldUp`
  `stage1-rogue: basic=True/AimedShot, special=ShurikenThrow`
  `stage1-priest: basic=True/AimedShot, special=Heal`
  `stage1-hero-karin: basic=True/Slash, special=SacredSwordWave`

### History

- 2026-05-18: User first requested a second Stage 1 enemy skill slot tied to `attack_type`, then clarified that the real authority must be the CSV itself so any enemy type can later be assigned any basic skill.

## Task: 2026-05-18 Skill Effect Prefab Scene Authority Migration

### Task title

Move monster/enemy skill effect prefab authority from runtime CSV fields into a scene-owned `EffectManager`.

### Goals

- Add a dedicated `EffectManager` under `NewRunScene` `GameManager` for skill-effect prefab lookup and instantiation.
- Remove `skill_effect_prefab_path` as a runtime-authority field from `monster_skills.csv` and `EnemySkillData.csv`.
- Keep `monster_skill_choices.csv` effect prefab paths intact because runtime choice effects still use that CSV-authored override path.
- Group inspector authoring by unit so monster/enemy effect entries are readable and clickable in one place.

### Constraints

- Role Owner is Code Builder.
- `skill_effect_prefab_path` remains valid only for `monster_skill_choices.csv`; this slice did not redesign choice effect overrides.
- Enemy unit body prefabs and player/monster unit body prefabs remain on their existing scene-authored spawn wiring.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- If the user wants projectile/body/effect authoring unified further, decide whether monster/enemy unit body prefabs should also move into a dedicated scene registry instead of staying on spawn-manager fields.
- If the user wants choice-effect authoring out of CSV too, plan a second slice that replaces `monster_skill_choices.csv` prefab paths with scene or asset-owned choice effect registries.
- If future editor tooling should export scene effect assignments, add a new `EffectManager`-aware exporter instead of writing base-skill prefab paths back into active skill CSVs.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now serializes `effectManager` and no longer owns `eveAProjectilePrefab`, `arielAProjectilePrefab`, `arielBShieldEffectPrefab`, or `runtimeSkillRoot`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs`, `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs`, and `InGameProjectileActor.cs` now resolve/instantiate skill effects through `EffectManager`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `EnemyDataset.cs`, `Build.cs`, `Editor.cs`, and `Validation.cs` no longer treat base monster/enemy `skill_effect_prefab_path` values as runtime-authority input.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/CSVdata/EnemySkillData.csv` no longer contain the `skill_effect_prefab_path` column, while `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` still does.
- `Pakuri/Assets/Scripts2/InGame/Data/Editor/PakuriSkillEffectPrefabCsvExporter.cs` now exports only choice effect prefab paths back into `monster_skill_choices.csv`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/EnemyDefinition.cs`, `EnemyUnitRuntimeModel.cs`, and `UnitFactory.cs` no longer carry `ActiveSkillEffectPrefab`.
- `Pakuri/Assets/CSVdata/EnemySkillData.csv` row 10 (`SacredSwordWave`) was repaired after the initial migration so `CooldownProjectile`, `outgoing_damage_multiplier`, `source_status`, and `source_notes` realign to the correct columns.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now serializes `effectManager: {fileID: 1427799831}`, `Assembly-CSharp::Pakuri.InGame.EffectManager`, `runtimeSkillRoot: {fileID: 502722560}`, `monsterSkillEffects`, and `enemySkillEffects`.
- Unity-MCP `execute_code` returned `Configured EffectManager on GameManager: monsterGroups=5, enemyGroups=8`, and a follow-up inspection returned `EffectManager runtimeRoot=RunTimeSkill, monsterGroups=5, enemyGroups=8, combatLink=True`.
- `dotnet build Pakuri\\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP `manage_scene validate` on `Assets/Scenes/NewScene/NewRunScene.unity` reported `totalIssues=0`, `missingScripts=0`, and `brokenPrefabs=0`; console readback contained only MCP bridge connection logs.
- After forcing `AssetDatabase.ImportAsset(\"Assets/CSVdata/EnemySkillData.csv\")` and `PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor()`, Unity-MCP returned `CSV sync ok: monsters=5, enemies=8`.

### History

- 2026-05-18: User explicitly assigned Code Builder to implement `EffectManager`, delete base-skill prefab-path runtime authority from `monster_skills.csv` / `EnemySkillData.csv`, and make the scene inspector the single source of truth for monster/enemy skill effect prefabs.
- 2026-05-18: After the first CSV column-removal pass, user reported `EnemySkillData.csv` row-10 parse failure; Code Builder repaired the `SacredSwordWave` row and reimported the CSV until runtime sync succeeded again.

## Task: 2026-05-18 Reward Choice CSV Structure Cleanup

### Task title

Separate offering reward IDs from runtime skill-choice IDs, remove dead modifier CSV columns, and archive inactive root CSV files.

### Goals

- Stop using `RunSession.ChosenRewardIds` as the implicit runtime skill-choice source.
- Make `monster_reward_choices.csv` explicitly declare reward target skills and optional linked runtime choice IDs.
- Remove dead `range_multiplier` / `range_bonus` columns from `SkillChoiceModifierData.csv`.
- Decide the role of root `MonsterStat.csv`, `SkillData.csv`, `EnemyStat.csv`, and `SkillChoiceData.csv` by moving them out of live `Assets/CSVdata`.

### Constraints

- Role Owner is Code Builder.
- Keep active runtime CSV authority on `Assets/CSVdata/source/*.csv`, `Assets/CSVdata/EnemySkillData.csv`, and `Assets/CSVdata/SkillChoiceModifierData.csv`.
- Do not reintroduce fallback reward-target inference by reward-id prefix matching.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- If the user wants full single-source choice data later, decide whether `monster_reward_choices.csv` and `monster_skill_choices.csv` should remain separate layers or be merged into one authored schema.
- If non-Eve monsters need runtime combat modifiers from offerings, add explicit linked modifier rows for their `linked_choice_id` values instead of relying on reward numbers alone.
- If archived root CSVs need to return as live runtime sources later, reintroduce them through one loader path instead of parallel tables.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Definition/MonsterDefinition.cs` now gives each reward choice `ActiveSkillId`, `PassiveSkillId`, and `LinkedChoiceId`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `...Build.cs`, and `...Validation.cs` now parse, build, and validate those reward-target/link fields from `monster_reward_choices.csv`.
- `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` now declares explicit `active_skill_id`, `passive_skill_id`, and `linked_choice_id` columns for every active reward row.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` no longer infers reward ownership from `rewardId.StartsWith(skillId + "-")`; enhancement availability now uses explicit reward target fields.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` now stores `ChosenRewardIds` and `ChosenChoiceIds` separately, and `RecordOfferingChoice(...)` now records reward IDs independently from optional linked runtime choice IDs.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` now copies `RunMonsterState.ChosenChoiceIds` into `UnitStateBucket.ChosenChoiceIds`, so runtime skill modifiers no longer piggyback on reward IDs.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs` no longer depends on `UnitFactory.TryCreatePhase2ATestModels(...)`; the helper was removed from `UnitFactory.cs`.
- `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` no longer contains `range_multiplier` or `range_bonus` columns.
- `Pakuri/reference/Archive/InactiveRootCsv/` now contains archived copies of `MonsterStat.csv`, `SkillData.csv`, `EnemyStat.csv`, and `SkillChoiceData.csv` plus their `.meta` files and a `README.md` explaining that they are inactive transition/reference data.
- GUID searches for `MonsterStat.csv`, `SkillData.csv`, `EnemyStat.csv`, and `SkillChoiceData.csv` returned no matches under `Pakuri/Assets` text assets (`*.unity`, `*.prefab`, `*.asset`, `*.cs`) before the archive move.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly-reference warnings.
- Unity-MCP loaded `Assets/Scenes/NewScene/NewRunScene.unity`, `manage_scene validate` reported `totalIssues=0`, `missingScripts=0`, `brokenPrefabs=0`, and warning/error console output contained only MCP client-handler logs.

### History

- 2026-05-18: User explicitly assigned Code Builder to implement the roadmap items for choice/reward/modifier CSV cleanup, root CSV role decision, and dead helper/column removal.

## Task: 2026-05-18 Enemy Skill CSV Runtime Authority Step 1

### Task title

Remove the NewRunScene fallback catalog dependency and route enemy skill prefab/projectile/buff values from CSV into runtime models.

### Goals

- Make `EnemySkillData.csv` the runtime authority for enemy skill effect prefab, projectile speed/lifetime, and ChargeCommand buff multipliers.
- Remove `NewRunUnitSpawnManager` dependence on the serialized `fallbackCatalog` asset in `NewRunScene`.
- Keep the current enemy skill effect asset paths validated through the runtime CSV asset catalog flow.

### Constraints

- Role Owner is Code Builder.
- This slice did not redesign the broader `PakuriDataManager` fallback-capable API surface; it removed actual new-scene runtime usage of the serialized fallback asset.
- Player skill prefab fallback wiring in `InGameCombatManager` was left unchanged because the current request targeted enemy runtime authority.
- `StageOneEnemyPassiveStatApplier` hardcoded passive multipliers were not moved in this slice because the request targeted active enemy skill prefab/speed/buff values.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- If the user wants full enemy CSV authority, move passive per-skill multipliers from `StageOneEnemyPassiveStatApplier.cs` into authored data next.
- If the user wants full de-legacy data cleanup, remove the now-unused `GameDataCatalog.asset` fallback asset path from the remaining project data inventory after confirming nothing else references it.
- If future enemy skills need visual timing values, add explicit CSV columns instead of introducing new hardcoded durations.

### Evidence

- `Pakuri/Assets/CSVdata/EnemySkillData.csv` now includes `projectile_speed`, `projectile_lifetime`, `move_speed_multiplier`, and `outgoing_damage_multiplier` columns, with authored values for `AimedShot`, `ShurikenThrow`, `ChargeCommand`, and `SacredSwordWave`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.EnemyDataset.cs` now parses those four columns plus `skill_effect_prefab_path`, and copies them into `EnemyRow`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now loads enemy skill effect prefabs from the runtime asset catalog and stores projectile/buff values into `EnemyDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/EnemyDefinition.cs`, `EnemyUnitRuntimeModel.cs`, and `UnitFactory.cs` now carry enemy active-skill prefab/projectile/buff values end to end into the runtime model.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` no longer resolves enemy skill prefabs through a `StageOneSkill` switch; it now returns `enemy.ActiveSkillEffectPrefab`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSimulationSystem.cs` now reads projectile speed/lifetime from the enemy runtime model and reads ChargeCommand move speed/outgoing damage multipliers from CSV-fed runtime fields.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunUnitSpawnManager.cs` no longer has a serialized `fallbackCatalog` field and now resolves its catalog only from `PakuriDataManager.Instance.CurrentCatalog` or `PakuriCsvRuntimeData.ResolveCatalogOrFallback(null)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs` no longer serializes a fallback catalog for isolated test bootstrap.
- `Pakuri/Assets/Scripts2/InGame/Data/Editor/PakuriCsvRuntimeCatalogPostprocessor.cs` now watches both `Assets/CSVdata/source/*.csv` and the root `Assets/CSVdata/EnemySkillData.csv` for runtime sync.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` no longer serializes the `fallbackCatalog` field.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings after rerunning it alone; the earlier parallel editor build failed only because the runtime build held `obj\Debug\Assembly-CSharp.dll` open.
- Unity-MCP loaded `Assets/Scenes/NewScene/NewRunScene.unity`, `manage_scene get_active` returned `name=NewRunScene`, `path=Assets/Scenes/NewScene/NewRunScene.unity`, and `manage_scene validate` reported `missingScripts=0`, `brokenPrefabs=0`.
- Unity-MCP warning/error console read after validation returned only MCP client handler logs.

### History

- 2026-05-18: User explicitly assigned Code Builder to implement the roadmap items for fallback catalog removal/replacement and full enemy active skill CSV connection for prefab/speed/buff values.

## Task: 2026-05-18 CSV Runtime Structure Review Roadmap

### Task title

Record the current CSV runtime structure review and cleanup categories for the NewRunScene data path.

### Goals

- Preserve the current evidence-backed conclusion about which CSV files are actual runtime sources and which are transition-only.
- Record the main structural issues in the current CSV bridge and runtime application path.
- Record the recommended categories for each affected file or issue: delete, keep, merge, or fix now.

### Constraints

- Role Owner is Designer.
- Documentation-only update; no CSV file, C# script, asset, or scene was changed.
- Current NewRunScene still has a `fallbackCatalog` scene reference, so this review must not claim the runtime is already pure-CSV.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Designer

### Status

Review documented as an HTML roadmap and preserved in board form.

### Next Actions

- First cleanup slice should remove or replace the `fallbackCatalog` asset reference in `NewRunScene`.
- After that, connect `EnemySkillData.csv` prefab and remaining hardcoded enemy numeric fields into the runtime path.
- Then decide whether root `MonsterStat.csv` / `SkillData.csv` / `EnemyStat.csv` / `SkillChoiceData.csv` will become the new source of truth or be retired after consolidation.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.cs` points runtime catalog import at `Assets/CSVdata/source` and also loads `Assets/CSVdata/EnemySkillData.csv`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Loader.cs` loads `monsters.csv`, `monster_skills.csv`, `monster_reward_choices.csv`, `monster_skill_choices.csv`, `stage_one_enemies.csv`, and `EnemySkillData.csv`.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunStageManager.cs` and `Assets/Scenes/NewScene/NewRunScene.unity` show direct serialized use of `StageDay.csv`, `StageEncounter.csv`, and `StageReward.csv`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` and `Assets/Scenes/NewScene/NewRunScene.unity` show direct serialized use of `SkillChoiceModifierData.csv`.
- Current Scripts2 runtime search found direct runtime references for `EnemySkillData.csv`, but no direct runtime references for `MonsterStat.csv`, `SkillData.csv`, `EnemyStat.csv`, or `SkillChoiceData.csv`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.EnemyDataset.cs` reads enemy `skill_effect_prefab_path` but does not copy it into `EnemyDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `EnemyCombatSimulationSystem.cs`, `NewRunUnitSpawnManager.cs`, and `InGameSkillDefinitionMapper.cs` still contain monster/enemy prefab or numeric hardcoding.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` still stores `fallbackCatalog` referencing `Pakuri/Assets/Legacy/Data/GameData/GameDataCatalog.asset`.

### History

- 2026-05-18: User asked for the full CSV structure review to be converted into an HTML working roadmap with delete/keep/merge/fix-now classification.

## Task: 2026-05-17 Scripts2 Legacy Core Migration Phase3-4

### Task title

Move the remaining Scripts2-used Run session/state files and verify that Legacy/Data code migration is complete.

### Goals

- Complete Phase 3 by relocating `RunSession` and `RunDayModel` into a Scripts2-owned folder.
- Keep Phase 3 scoped to the Run session/state files that the current Scripts2 flow actually uses.
- Complete Phase 4 by proving there are no remaining `Pakuri/Assets/Legacy/Scripts/Data/**/*.cs` files left to move.

### Constraints

- Role Owner is Code Builder.
- No CSV source, ScriptableObject asset, runtime catalog behavior, or new menu/run scene logic was intentionally changed.
- `RunStartContext.cs` and old controller files were left in Legacy because they belong to the old scene flow and are not part of the new Scripts2 Run session/state dependency.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Phase 3 implemented; Phase 4 closed with no remaining Legacy/Data code files.

### Next Actions

- Proceed to Phase 5 old-scene controller cleanup after scene-reference verification.
- Do not claim `Legacy/Scripts` is fully removable yet; `RunStartContext` and old scene/controller scripts still remain.

### Evidence

- New Scripts2-owned Run files now exist at `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` and `RunDayModel.cs`, with the old Legacy session paths removed.
- `Pakuri/Assembly-CSharp.csproj` now points to `Assets\Scripts2\InGame\Run\RunSession.cs` and `Assets\Scripts2\InGame\Run\RunDayModel.cs`.
- `Get-ChildItem Pakuri\Assets\Legacy\Scripts\Data -Recurse -Filter *.cs | Measure-Object` returned `Count=0`, confirming there are no remaining Legacy/Data `.cs` files.
- `Pakuri/Assembly-CSharp-Editor.csproj` now contains only Scripts2-owned data editor includes for `PakuriSkillEffectPrefabCsvExporter.cs` and `PakuriCsvRuntimeCatalogPostprocessor.cs`; no `Assets\Legacy\Scripts\Data\...` compile include remains.
- Runtime/editor builds completed with 0 errors and the existing assembly reference warnings.

### History

- 2026-05-17: Code Builder completed Phase 3 Run-session migration and verified that Phase 4 had no remaining Legacy/Data code to move.

## Task: 2026-05-17 Scripts2 Legacy Core Migration Phase1-2

### Task title

Move the shared Legacy combat/data foundation used by Scripts2 into Scripts2-owned folders.

### Goals

- Complete Phase 1 by moving the shared combat base files that define `DamageAttribute`, `DamageCalculator`, `CombatStatBlock`, and `AttributeDefenseSet`.
- Complete Phase 2 by moving the shared `Pakuri.Data` definitions, runtime catalog/manager files, and supporting editor CSV sync scripts into a Scripts2-owned structure.
- Preserve namespaces, serialized script GUIDs, and current New scene runtime behavior.

### Constraints

- Role Owner is Code Builder.
- This slice is behavior-preserving migration only; no gameplay logic, scene hierarchy, prefab binding, CSV values, or runtime API contract was intentionally changed.
- `.cs.meta` files had to move with their `.cs` files to keep Unity serialized references valid.
- `Pakuri.Run` files such as `RunSession`, `RunDayModel`, and old Run/MainMenu controllers were not moved in this phase.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Phase 1 and Phase 2 implemented and locally verified.

### Next Actions

- Phase 3: move `Pakuri.Run` session/state files still used by `NewRunSceneEntryManager`, `NewRunStageManager`, `NewRunUnitSpawnManager`, `UnitFactory`, and `InGameUIManager`.
- Phase 4: confirm whether any remaining Legacy-side editor/runtime support files still need relocation after the Run migration.
- Phase 5: after Run migration and Unity missing-script checks, remove old-scene-only controllers and then delete the remaining Legacy scripts.

### Evidence

- New Scripts2-owned combat files now exist at `Pakuri/Assets/Scripts2/InGame/Combat/CombatStatModels.cs` and `Pakuri/Assets/Scripts2/InGame/Combat/DamageCalculator.cs`; the old Legacy paths `Pakuri/Assets/Legacy/Scripts/Combat/Monster/CombatStatModels.cs` and `Pakuri/Assets/Legacy/Scripts/Combat/Skill/DamageCalculator.cs` no longer exist.
- New Scripts2-owned data definition files now exist at `Pakuri/Assets/Scripts2/InGame/Data/Definition/EnemyDefinition.cs`, `GameDataCatalog.cs`, `MonsterDefinition.cs`, and `SkillDefinition.cs`.
- New Scripts2-owned runtime files now exist at `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriDataManager.cs`, `PakuriCsvRuntimeAssetCatalog.cs`, `PakuriCsvRuntimeSourceCatalog.cs`, and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData*.cs`.
- New Scripts2-owned editor support files now exist at `Pakuri/Assets/Scripts2/InGame/Data/Editor/PakuriCsvRuntimeCatalogPostprocessor.cs` and `PakuriSkillEffectPrefabCsvExporter.cs`.
- `Pakuri/Assembly-CSharp.csproj` now includes Scripts2-owned compile paths such as `Assets\Scripts2\InGame\Combat\CombatStatModels.cs`, `Assets\Scripts2\InGame\Combat\DamageCalculator.cs`, `Assets\Scripts2\InGame\Data\Definition\GameDataCatalog.cs`, `Assets\Scripts2\InGame\Data\Runtime\PakuriDataManager.cs`, and `Assets\Scripts2\InGame\Data\Runtime\Csv\PakuriCsvRuntimeData.cs`.
- `Pakuri/Assembly-CSharp-Editor.csproj` now includes `Assets\Scripts2\InGame\Data\Editor\PakuriSkillEffectPrefabCsvExporter.cs` and `PakuriCsvRuntimeCatalogPostprocessor.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check -- Pakuri/Assembly-CSharp.csproj Pakuri/Assembly-CSharp-Editor.csproj Pakuri/Assets/Scripts2/InGame/Combat Pakuri/Assets/Scripts2/InGame/Data ...` completed with no output.

### History

- 2026-05-17: User explicitly assigned Code Builder to execute Phase 1 through Phase 2 of the Legacy-to-Scripts2 migration.

## Task: 2026-05-17 Scripts2 Legacy Core Migration Design

### Task title

Plan the safe migration of core Legacy data/run/combat base scripts into the Scripts2-owned structure.

### Goals

- Remove the current hard dependency from `Scripts2` onto `Pakuri/Assets/Legacy/Scripts` before deleting Legacy scripts.
- Preserve `NewMainMenu.unity` and `NewRunScene.unity` behavior while moving shared base types, catalog loading, and run state into a Scripts2-owned location.
- Keep Unity serialized references valid during the migration.

### Constraints

- Role Owner is Designer.
- This task records migration design only; no C# script, `.meta`, scene, prefab, or asset file was changed.
- Current `Scripts2` runtime still directly references Legacy-defined types such as `GameDataCatalog`, `MonsterDefinition`, `EnemyDefinition`, `SkillDefinition`, `PassiveDefinition`, `RunSession`, `PakuriDataManager`, `PakuriCsvRuntimeData`, `DamageAttribute`, `CombatStatBlock`, `AttributeDefenseSet`, and `DamageCalculator`.
- `Assembly-CSharp.csproj` still includes Legacy scripts, and Unity scenes/assets still serialize Legacy script GUID references.

### Role Owner

Designer

### Status

Migration order defined; ready for Code Builder handoff.

### Next Actions

- Phase 1: move shared base types from Legacy into a Scripts2-owned folder while preserving `.meta` GUIDs and keeping namespaces stable.
- Phase 2: move `Pakuri.Data` runtime catalog/definition/manager files into a Scripts2-owned data folder, again preserving `.meta` GUIDs.
- Phase 3: move `Pakuri.Run` session/state files needed by `NewRunSceneEntryManager`, `NewRunStageManager`, `NewRunUnitSpawnManager`, `UnitFactory`, and `InGameUIManager`.
- Phase 4: move Legacy editor CSV sync/validation scripts that support the runtime catalog assets.
- Phase 5: only after builds and Unity serialized-reference checks pass, remove old-scene-only controllers and then delete the remaining Legacy scripts.

### Evidence

- `Scripts2` dependency scan found 22 files referencing `Pakuri.Data`, `Pakuri.Combat`, or `Pakuri.Run`, with 258 reference matches across those files.
- `Assembly-CSharp.csproj` still includes Legacy compile items such as `Assets\Legacy\Scripts\Data\Runtime\Csv\PakuriCsvRuntimeData.cs`, `Assets\Legacy\Scripts\Data\Definition\MonsterDefinition.cs`, `Assets\Legacy\Scripts\Data\Definition\EnemyDefinition.cs`, and `Assets\Legacy\Scripts\Run\Session\RunSession.cs`.
- Legacy script definitions currently own the shared types used by `Scripts2`: `GameDataCatalog`, `MonsterDefinition`, `EnemyDefinition`, `SkillDefinition`, `PassiveDefinition`, `PakuriDataManager`, `PakuriCsvRuntimeData`, `DamageAttribute`, `DamageCalculator`, `AttributeDefenseSet`, and `CombatStatBlock`.
- Serialized-reference scan found 24 Legacy script GUID references remaining in Unity assets, including `MainMenuScene.unity`, `RunScene.unity`, `DebugScene.unity`, `Legacy/Data/GameData/*.asset`, and `Resources/Pakuri/CSVRuntime/*.asset`.

### History

- 2026-05-17: User asked how to migrate and delete Legacy scripts while keeping the Scripts2-owned New scene flow.

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
