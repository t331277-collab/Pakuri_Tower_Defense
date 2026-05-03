# DATA_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

## Task: 2026-05-02 Legacy Seeder Removal And Dataset-Level Split

### Task title

Remove the remaining legacy seeder path and split CSV runtime support by dataset responsibility.

### Goals

- Delete the obsolete `PakuriGameDataSeeder` editor-only path now that runtime source-of-truth is CSV.
- Remove the old bootstrap menu flow tied to `GameDataCatalog.asset`.
- Split CSV runtime support into dataset-oriented files instead of keeping row/parser support bundled together.
- Extend `PakuriDataManager` so monster sub-data queries also route through one contract.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual file edits and actual Unity/editor output.
- Do not run Unity Play Mode verification.
- Code Reviewer has not run yet for this follow-up phase.

### Role Owner

Code Builder

### Status

Implemented, locally validated, and later reviewed with no discrete actionable bug reported.

### Next Actions

- If later requested, continue by removing dormant legacy `Assets/Data/GameData/*.asset` authoring dependencies outside runtime.
- If a stricter verification pass is needed later, use Unity compile/console evidence again because reviewer-side `dotnet build` remained blocked by sandboxed SDK path access.

### Evidence

- `Pakuri/Assets/Scripts/Data/Editor/Legacy/PakuriGameDataSeeder.cs` and its `.meta` file were deleted from the worktree.
- `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.Editor.cs` now exposes only `Pakuri/Sync CSV Runtime Catalog Assets` and `Pakuri/Validate CSV Source Data`; the old bootstrap menu is no longer present.
- `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.cs` still points runtime source loading at `ImportedSourceAssetRoot = "Assets/CSVdata/source"` and resource catalogs at `Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog` and `Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog`.
- Added dataset-split files: `PakuriCsvRuntimeData.CsvSupport.cs`, `PakuriCsvRuntimeData.SourceModel.cs`, `PakuriCsvRuntimeData.CatalogDataset.cs`, `PakuriCsvRuntimeData.MonsterDataset.cs`, and `PakuriCsvRuntimeData.EnemyDataset.cs`.
- `Pakuri/Assets/Scripts/Data/PakuriDataManager.cs` now exposes `GetActiveSkills(...)`, `GetPassiveSkills(...)`, `GetRewardChoices(...)`, `ResolveActiveSkill(...)`, and `ResolvePassiveSkill(...)` in addition to the earlier roster queries.
- `Select-String` over `Pakuri/Assets/Scripts/Data/*.cs` found the new dataset files and menu strings, and no remaining `PakuriGameDataSeeder` hit under `Pakuri/Assets/Scripts/Data`.
- After fixing one `System.Random` ambiguity and one missing `using System;` import, Unity refresh completed without C# compile errors, and `Pakuri/Validate CSV Source Data` again logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- External `codex review --uncommitted` later reviewed the modified runtime-data refactor files and reported no discrete actionable bug introduced by this patch; the reviewer also stated it could not complete a local `dotnet build` because SDK/user sentinel paths were blocked in that environment.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- 2026-05-02: Builder deleted `PakuriGameDataSeeder`, removed the old bootstrap menu path, split row/parser support into dataset files, expanded `PakuriDataManager`, fixed the resulting compile errors, and revalidated the CSV runtime load path in Unity.
- 2026-05-02: User explicitly requested Code Reviewer execution for the current follow-up, and the external reviewer returned no discrete actionable bug for this patch set.

## Task: 2026-05-02 Query Contract Unification Around PakuriDataManager

### Task title

Unify monster/enemy runtime query paths behind `PakuriDataManager`.

### Goals

- Add collection-level query helpers to `PakuriDataManager`.
- Remove direct gameplay reads of `gameDataCatalog.Monsters`, `gameDataCatalog.StageOneEnemies`, and `fallbackCatalog.Monsters`.
- Keep serialized catalog references as fallback inputs to the data manager rather than the consumer-side source of truth.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual file edits and actual Unity/editor output.
- Do not run Unity Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- If the user wants a stricter second pass, convert remaining catalog-dependent method signatures to pass stable ids or data-manager-backed context objects instead of `GameDataCatalog`.
- If later requested, rerun Code Reviewer once against this query-contract follow-up.

### Evidence

- `Pakuri/Assets/Scripts/Data/PakuriDataManager.cs` now exposes `GetMonsters(...)`, `GetStageOneEnemies(...)`, and `ResolveMonster(...)`.
- After the change, `Get-ChildItem -Path 'Pakuri/Assets/Scripts' -Recurse -Filter *.cs | Select-String -Pattern 'gameDataCatalog\.Monsters|gameDataCatalog\.StageOneEnemies|fallbackCatalog\.Monsters'` returned only `PakuriDataManager.cs:114`, which is the internal fallback access inside the manager itself.
- `refresh_unity` requested a script compile, and later `read_console` output showed the runtime catalog load log but no C# compile error entries.
- `Pakuri/Validate CSV Source Data` again logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- The latest Unity console reads still include `The referenced script (Unknown) on this Behaviour is missing!`, so a pre-existing scene/reference issue remains outside this query-contract change.

### History

- 2026-05-02: User asked to implement the previously identified high-priority query-contract unification.
- 2026-05-02: Builder added collection lookup helpers to `PakuriDataManager` and rewired remaining gameplay consumers away from direct catalog array reads.
- 2026-05-02: Unity script refresh completed, console reads showed no compile errors, and the CSV validation menu still loaded the 5-monster / 8-enemy runtime catalog.

## Task: 2026-05-02 CSV Source Runtime Migration

### Task title

Move runtime game-data loading to typed CSV source files under `Pakuri/Assets/CSVdata/source`.

### Goals

- Keep existing `Pakuri/data/*.csv` files as legacy reference data.
- Add a typed CSV source set that can represent the current `GameDataCatalog`, `MonsterDefinition`, and `EnemyDefinition` runtime shape.
- Parse the typed CSV source on game startup and stop the game on fatal data-validation errors.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and actual command output.
- Do not run Unity Play Mode verification from Codex.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Builder follow-up implemented and locally revalidated after the Reviewer findings. A later builder pass also split the `PakuriCsvRuntimeData` monolith into multiple partial files while preserving the same CSV-first runtime path. The original Reviewer verdict remains FAIL until the user explicitly requests another review.

### Next Actions

- User can continue editing row data in `Pakuri/Assets/CSVdata/source/*.csv`.
- If the runtime model grows beyond the current 7 tables, extend the typed CSV schema and runtime catalog sync together.
- If the user later requests another Code Reviewer run, re-check this builder follow-up against `2026-05-01-data-structure-review.html`.
- If the user wants the HTML implementation report refreshed, update it so the old `PakuriCsvRuntimeData.cs` monolith note no longer describes the current repository state.

### Evidence

- `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.cs` now uses `ImportedSourceAssetRoot = "Assets/CSVdata/source"`, `SourceCatalogResourcesPath = "Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog"`, and `AssetCatalogResourcesPath = "Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog"`.
- `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.cs` is now a 116-line runtime entry file, and the previous monolith has been split into `PakuriCsvRuntimeData.Loader.cs` (312 lines), `PakuriCsvRuntimeData.Validation.cs` (506 lines), `PakuriCsvRuntimeData.Build.cs` (304 lines), `PakuriCsvRuntimeData.Editor.cs` (794 lines), and `PakuriCsvRuntimeData.Types.cs` (479 lines).
- Added `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeSourceCatalog.cs`, `PakuriCsvRuntimeAssetCatalog.cs`, `PakuriDataManager.cs`, and `Pakuri/Assets/Scripts/Data/Editor/PakuriCsvRuntimeCatalogPostprocessor.cs`.
- `LoadAndValidateRuntimeCatalog()` now loads imported CSV through `PakuriCsvRuntimeSourceCatalog`, validates CSV id/link rules plus runtime asset catalog coverage, builds an in-memory `GameDataCatalog`, validates bound runtime assets, registers `PakuriDataManager`, and still calls `Application.Quit()` on fatal startup errors.
- `ResolveCatalogOrFallback(...)` now returns `null` when CSV initialization failed, so the serialized `GameDataCatalog` field no longer acts as a hidden second runtime source after failure.
- `ValidateSourceModelOrThrow(...)` now checks sprite/prefab asset-path coverage against `PakuriCsvRuntimeAssetCatalog`.
- `ValidateRuntimeCatalogOrThrow(...)` now checks that non-empty CSV sprite/prefab paths became non-null runtime object references.
- The split preserved the existing singleton boundary: `PakuriDataManager.Instance` remains the only data-registry singleton, and the refactor did not introduce new scene-controller singletons.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` exists and its YAML references the 7 imported CSV TextAssets from `Assets/CSVdata/source/*.csv`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` exists and its YAML contains 11 `AssetPath:` sprite entries and an empty prefab list for the current source data set.
- Unity `refresh_unity` imported the new scripts, created `.meta` files for `PakuriCsvRuntimeSourceCatalog.cs`, `PakuriCsvRuntimeAssetCatalog.cs`, and `PakuriDataManager.cs`, and a later console read showed no C# compile errors.
- `Pakuri/Validate CSV Source Data` previously logged: `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- A later full Unity asset refresh created `.meta` files for every new `PakuriCsvRuntimeData.*.cs` partial file, and a subsequent console read showed no project compile errors before the validation menu was re-run successfully.
- `dotnet build` could not be used as the final verification path in this sandbox because it failed on denied access to `C:\\Users\\t3312\\.dotnet\\...toolpath.sentinel` and `C:\\Users\\t3312\\AppData\\Local\\Microsoft SDKs`.

### History

- 2026-05-02: User approved a new typed CSV source set while keeping the legacy `Pakuri/data/*.csv` files as reference only.
- 2026-05-02: Initial builder pass loaded CSV from `Pakuri/data/source`, then Code Reviewer marked the direction FAIL for hidden asset fallback, missing query contract, and non-runtime-safe asset paths.
- 2026-05-02: User later imported the typed CSV into `Pakuri/Assets/CSVdata` and asked Builder to fix the Reviewer findings.
- 2026-05-02: Builder moved the runtime source root to `Assets/CSVdata/source`, added runtime source/asset catalogs in `Assets/Resources/Pakuri/CSVRuntime`, added `PakuriDataManager`, and changed validation to cover asset-path binding.
- 2026-05-02: Unity refresh created the new script `.meta` files; `Pakuri/Sync CSV Runtime Catalog Assets` generated the two runtime catalog assets; Unity console compile finished without C# errors.
- 2026-05-02: Added `Pakuri/reference/Report/2026-05-02-data-structure-refactor-implementation-report.html` to document how the original review findings map to the current implementation state.
- 2026-05-02: Builder split `PakuriCsvRuntimeData.cs` into runtime entry, loader, validation, build, editor, and type-support partial files; after a full asset refresh Unity generated all new `.meta` files, compile errors stayed clear, and CSV validation still loaded the 5-monster / 8-enemy runtime catalog.

## Task: 2026-05-01 Assets Script And SO Structure Review

### Task title

Review `Pakuri/Assets` script and ScriptableObject structure for future content expansion risks.

### Goals

- Inspect the actual `Pakuri/Assets/Scripts` and `Pakuri/Assets/Data/GameData` files.
- Identify structural issues that will make adding monsters, stages, rewards, or meta content expensive.
- Record only evidence-based findings from the current code and assets.

### Constraints

- Role Owner is Designer.
- Base every conclusion on actual files and command output.
- Do not claim gameplay verification or unimplemented systems as existing.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- If the user requests implementation, split the follow-up into data-model cleanup, run-state cleanup, and combat runtime decoupling.

### Evidence

- `Pakuri/Assets/Scripts/Data/GameDataCatalog.cs` stores only `Monsters` and `StageOneEnemies`; there is no stage/reward/shop catalog model.
- `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs` keeps duplicated top-level combat tuning fields (`MaxHealth`, `PowerStat`, `BaseDamage`, etc.) alongside `BaseStats`, which creates two stat sources.
- `Pakuri/Assets/Scripts/Data/SkillDefinition.cs` defines `RuntimeKind`, `ImplementationState`, `SkillEffectPrefab`, and `StatusEffectId`, but `Select-String` over `Pakuri/Assets/Scripts/**/*.cs` found no runtime consumers for `RuntimeKind`, `ImplementationState`, `SkillEffectPrefab`, or `StatusEffectId`.
- `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs` seeds a fixed 5-monster roster and fixed Stage 1 enemy list, and reads monster skill source documents from `Application.dataPath/../reference/2.Monster/...`, so the content source of truth is partly outside `Assets`.
- `Pakuri/Assets/Data/GameData/GameDataCatalog.asset` references 5 monster assets and 8 `StageOneEnemies` assets only.
- Monster asset inspection found `SkillIcon: {fileID: 0}` and `SkillEffectPrefab: {fileID: 0}` 60 times each per monster asset, so the current skill/effect authoring pipeline is structurally present but still unpopulated.
- Monster asset inspection found `eve.asset` marks only `eve-a` as `ImplementationState: 2` while `CombatRuntimeEveSkills.cs` already contains Eve B-E/F-J runtime code, showing data/runtime state drift.

### History

- 2026-05-01: Read `Pakuri/Assets/Scripts/Data/*.cs`, `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Assets/Data/GameData/*.asset`, and supporting run/combat scripts to review content expansion risk.

## Task: Monster A-J Skill Data Cleanup

### Task title

Prepare the 5 monster A-J skill data cleanup from reference documents.

### Goals

- Use `Pakuri/reference/Report/2026-04-28-reference-implementation-roadmap.html` step 5 as the implementation direction.
- Compare the 5 monster A-J skill documents under `Pakuri/reference/2.Monster` against current `Assets/Data/GameData/Monsters/*.asset`.
- Represent A as the default active skill, B-E as selectable actives, F as a selectable base passive, and G-J as passives unlocked by their matching active skills.
- Keep this pass focused on data/selection/unlock structure before full runtime effects.

### Constraints

- Role Owner is Designer until explicit Builder handoff.
- Ground all claims in actual files and command output.
- Current `SkillDefinition`/`PassiveDefinition` can store base skill/passive fields but has no structured fields for active enhancements, passive enhancements, or master skill branches.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation completed, and the user reported Play Mode verification completed. The required one-shot external Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; the user chose not to fix that reviewer finding for now. The finding is limited to trailing whitespace in `Pakuri/Assets/Data/GameData/Monsters/eve.asset`.

### Next Actions

- Continue to the next requested design or implementation task.
- If the user later wants the reviewer finding cleaned, remove the trailing whitespace in `eve.asset`, rerun `git diff --check`, rebuild, and update this block.

### Evidence

- Roadmap report step 5 says to organize monster A-J skill data first, completing selection/unlock structure before all complex effects.
- `Pakuri/reference/2.Monster` contains `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `monster-skill-patterns.md`, 5 monster tower documents, and 50 A-J skill documents.
- `SkillDefinition.cs` currently contains `SkillId`, `DisplayName`, `Slot`, `RuntimeKind`, `ImplementationState`, damage/range/cooldown/magazine fields, `StatusEffectId`, and `Summary`.
- `PassiveDefinition` currently contains `PassiveId`, `DisplayName`, `Slot`, `RequiredActiveSlot`, `ImplementationState`, and `Summary`.
- `MonsterDefinition.cs` currently stores `InitialRewardChoices`, `ActiveSkills`, and `PassiveSkills`, but no active-enhancement, passive-enhancement, or master-skill structured data.
- Current monster assets already contain A-E active entries and F-J passive entries; all A entries are `RuntimeImplemented`, B-E and F-J are `DataOnly`.
- `monster-basic-rule.md` states each monster starts with active A learned, starts with no passives learned, F is selectable without a specific active unlock, and G-J unlock after the matching B-E active is learned.
- `skill-choice-pool-rule.md` defines active enhancements, passive enhancements, and master skill candidates, but the current SO model has no dedicated structures for these candidates.
- `SkillDefinition.cs` now adds `SkillChoiceDefinition`, `SkillIcon`, `SkillEffectPrefab`, `DescriptionText`, active `EnhancementChoices`, active `MasterSkillChoices`, passive `EnhancementChoices`, `IsDefaultLearned`, and `IsAvailableWithoutActiveRequirement`.
- `PakuriGameDataSeeder.cs` now reads `Pakuri/reference/2.Monster/{monster}/skill/*.md` and populates A-E active and F-J passive data from those documents.
- `RunCombatUiController.cs` now adds structured active enhancements, passive enhancements, and master skill choices to the prisoner offering pool; it bypasses the active requirement only when `PassiveDefinition.IsAvailableWithoutActiveRequirement` is true.
- After running `Pakuri/Seed Default Game Data`, each monster asset has 5 `SkillId` entries, 5 `PassiveId` entries, 10 `EnhancementChoices` blocks, and 5 `MasterSkillChoices` blocks.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing 2 Unity/MCP reference warnings.
- Unity console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; verified with `git diff --check -- Pakuri\Assets\Data\GameData\Monsters\eve.asset`, which reports trailing whitespace at lines 225, 238, 288, 301, 352, and 365.
- Added `Pakuri/reference/Report/2026-04-29-roadmap-implementation-result.html` comparing today's implementation result against `2026-04-28-reference-implementation-roadmap.html`.
- Added `Pakuri/reference/Report/2026-04-29-token-optimization-savings.html` estimating token savings from document parsing/token reduction based on measured file sizes.

### History

- 2026-04-29: User requested starting roadmap step 5, monster A-J skill data cleanup, and asked for questions if needed.
- 2026-04-29: User selected the data-structure expansion path, requested per-skill icon/effect/description fields, confirmed reference documents are the conflict source of truth, and confirmed F passive should be selectable from prisoner offering instead of default-granted.
- 2026-04-29: Code Builder expanded skill data structures, connected structured choices to prisoner offering, seeded monster A-J data from reference documents, and ran build/Unity validation.
- 2026-04-29: External Code Reviewer one-shot review returned `NEEDS_CHANGES` for trailing whitespace in `eve.asset`; Builder paused for user instruction per AGENTS.md.
- 2026-04-29: User reported Play Mode verification completed and chose not to fix the reviewer-raised whitespace issue for now.
- 2026-04-29: Designer added roadmap comparison and token optimization savings HTML reports under `Pakuri/reference/Report`.

## Task: CSV Data Role And Loading Review

### Task title

Legacy non-English note retained these code references: `Pakuri/data`.

### Goals

- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these code references: `ally_units.csv`, `ally_runtime.csv`, `enemies.csv`, `enemy_runtime.csv`.
- Legacy non-English note retained these code references: `skills.csv`, `skill_runtime.csv`, `skill_branches.csv`, `levelup_choices.csv`, `levelup_rules.csv`.
- Legacy non-English note retained these code references: `waves_chapter1.csv`, `waves_chapter2.csv`, `waves_chapter3.csv`, `waves_runtime.csv`, `boss_patterns.csv`.
- Legacy non-English note retained these code references: `items.csv`, `status_effects.csv`, `formations.csv`, `balance_targets.csv`.
- Legacy non-English note retained these ASCII code references: `spawn_points.csv`.
- Legacy non-English note retained these code references: `towers.csv`, `tower_skills.csv`, `TOWER_001`.
- Legacy non-English note retained these code references: `ally_units.csv`, `ALLY_*`, `skills.csv`, `TOWER_001`.
- Legacy non-English note retained these code references: `ally_units.csv`, `levelup_choices.csv`, `skill_branches.csv`, `SKILL_004`, `skills.csv`.
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`, `Assets/Resources`, `Assets/StreamingAssets`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts`, `TextAsset`, `Resources.Load`, `StreamingAssets`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `Pakuri/data`.
- Legacy non-English note retained these code references: `ALLY_*`, `TOWER_*`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`.

## Task: Run Systems Integration Summary Report

### Task title

Legacy non-English note retained these code references: `monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan`.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`, `MetaSaveData`, `RunSnapshot`, `GameDataCatalog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `Scenes`, `Screenshots`, `Scripts`, `Settings`, `Resources`, `StreamingAssets`, `DataGenerated`.
- Legacy non-English note retained these code references: `.uxml`, `.uss`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/rin/rin-tower.md`, `rin/skill/g~j`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/4.run/combat-reward-system.md`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `RunSession`, `run-systems-integration-summary-report.html`.

## Task: Run Flow UICanvas Prototype Implementation

### Task title

Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.

### Goals

- Legacy non-English note retained these code references: `RunSession`, `RunFlowController`, `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

### Next Actions

- Legacy non-English note retained these code references: `RunUICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `B/G, C/H, D/I, E/J`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Seed Default Game Data`, `Assets/Data/GameData/GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `RunUICanvas`, `EventSystem`.
- Legacy non-English note retained these code references: `Assets/Data/GameData/GameDataCatalog.asset`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`, `EventSystem`, `InputSystemUIInputModule`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `stageIndex`.
- Legacy non-English note retained these code references: `RunFlowController.ClearButtons(...)`, `QueuedForDestroy`.
- Legacy non-English note retained these code references: `RunFlowController.ResolveReferences()`, `Arial.ttf`, `ArgumentException`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`, `Arial.ttf`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `EventSystem`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Resources.GetBuiltinResource<Font>("Arial.ttf")`, `RunFlowController`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

