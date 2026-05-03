# CSV_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

## Task: 2026-05-02 Typed CSV Source Runtime Pipeline

### Task title

Introduce a typed CSV source-of-truth pipeline under `Pakuri/Assets/CSVdata/source`.

### Goals

- Add CSV tables that can represent the current monster, reward-choice, skill, passive-choice, and stage-one enemy runtime data.
- Preserve the user's rule that legacy `Pakuri/data/*.csv` files are not rewritten in-place.
- Add parser-side and built-catalog validation so invalid source data becomes a fatal startup error.

### Constraints

- Role Owner is Code Builder.
- Use the actual runtime model shape, not the incomplete legacy CSV headers.
- Only row edits should be needed for normal content iteration inside the new typed source files.
- Do not claim Play Mode verification.

### Role Owner

Code Builder

### Status

Builder follow-up implemented and locally revalidated after the Reviewer findings. A later builder pass also split the `PakuriCsvRuntimeData` monolith into partial files without changing the typed CSV contract. The original Reviewer verdict remains FAIL until the user explicitly asks for another review.

### Next Actions

- Keep content edits inside `Pakuri/Assets/CSVdata/source/*.csv`.
- If a new content type needs new fields, extend the typed source schema and the runtime catalog sync together.
- If later requested, add a separate editor report that lists missing asset references and invalid enum values before startup.

### Evidence

- `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.cs` defines the typed CSV source contract and now loads source text through `PakuriCsvRuntimeSourceCatalog` instead of direct filesystem reads.
- The runtime loader is now physically split across `PakuriCsvRuntimeData.cs`, `PakuriCsvRuntimeData.Loader.cs`, `PakuriCsvRuntimeData.Validation.cs`, `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.Editor.cs`, and `PakuriCsvRuntimeData.Types.cs`, so the CSV pipeline is no longer concentrated in one 2000+ line file.
- The parser still expects a header row and a required second-row type declaration; `CsvTable.Load(...)` throws when a CSV has fewer than 2 rows.
- The active imported source files are:
- `Pakuri/Assets/CSVdata/source/catalog_monsters.csv`
- `Pakuri/Assets/CSVdata/source/catalog_stage_one_enemies.csv`
- `Pakuri/Assets/CSVdata/source/monsters.csv`
- `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv`
- `Pakuri/Assets/CSVdata/source/monster_skills.csv`
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`
- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` serializes references to those 7 imported CSV TextAssets.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` serializes the runtime-safe sprite/prefab dependency map that corresponds to the current CSV rows.
- `PakuriDataManager.Instance.GetData<T>(id)` now exists in `Pakuri/Assets/Scripts/Data/PakuriDataManager.cs` and is used by `RunFlowController.cs`, `RunCombatUiController.cs`, and `RunSceneBootstrap.cs` for monster lookup.
- The split kept the singleton boundary narrow: `PakuriDataManager` remains the only singleton-style query registry, while `PakuriCsvRuntimeData` stays a static bootstrap/service entry point instead of turning scene/runtime controllers into global singletons.
- `ValidateSourceModelOrThrow(...)` now checks duplicate ids, missing catalog references, missing monster references, active/passive slot rules, skill-choice linkage, and runtime asset-catalog coverage.
- `ValidateRuntimeCatalogOrThrow(...)` now checks the built in-memory catalog plus non-null bound assets for non-empty sprite/prefab paths.
- `Pakuri/Validate CSV Source Data` previously logged a successful 5-monster / 8-enemy load from resource source `Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog`.
- `Get-Content -Encoding UTF8 Pakuri/Assets/CSVdata/source/monsters.csv` confirmed the second-row type schema and readable Korean payload text.
- After the split, a full Unity asset refresh generated `.meta` files for each new `PakuriCsvRuntimeData.*.cs` file, and a later console read showed no C# compile errors before `Pakuri/Validate CSV Source Data` was re-run successfully.

### History

- 2026-05-02: User approved a typed CSV direction instead of forcing the runtime shape into the legacy CSV headers.
- 2026-05-02: Initial builder pass implemented parser, validator, exporter, UTF-8 writing, and validation menu support, but still used `Pakuri/data/source` and lacked a unified query contract.
- 2026-05-02: User imported the typed CSV into `Pakuri/Assets/CSVdata` and asked Builder to address the Reviewer findings.
- 2026-05-02: Builder switched the active source root to `Assets/CSVdata/source`, added runtime source/asset catalog assets under `Assets/Resources/Pakuri/CSVRuntime`, and added `PakuriDataManager`.
- 2026-05-02: Added `Pakuri/reference/Report/2026-05-02-data-structure-refactor-implementation-report.html` to summarize the current CSV runtime pipeline against the original review direction.
- 2026-05-02: Builder split the CSV runtime code into runtime-entry, loader, validation, build, editor, and type-support partial files, then revalidated compile/import and the CSV startup path through the Unity validation menu.

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

## Task: SaveAndLoad Direction Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `reference/4.run`, `reference/6.meta`.
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

- Legacy non-English note retained these code references: `RunSession`, `MetaSaveData`, `RunSnapshot`, `SaveLoadService`.
- Legacy non-English note retained these code references: `GameDataCatalog`, `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/4.run/dungeon-squad-run-structure.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/shop-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/event-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-index.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/active-skill-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/dark-trace-currency-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`, `Assets/Resources`, `Assets/StreamingAssets`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/save-and-load-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `monster-select-run-ui-expansion-plan.html`, `monster-select-run-ui-builder-handoff.html`, `reference/4.run`, `reference/6.meta`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `MetaSaveData`, `RunSnapshot`, `EphemeralRuntime`, `Pakuri/reference/save-and-load-plan.html`.
- Legacy non-English note retained these code references: `Pakuri/data`, `save-and-load-plan.html`.

