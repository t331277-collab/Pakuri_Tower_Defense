# CSV Loading Pipeline Refactor Handoff

## Task title

Reorganize CSV loading into one ordered pipeline with four responsibility folders.

## Goals

- Move the CSV loading system under `Pakuri/Assets/Scripts/Loading`.
- Make Parsing, Validation, Generation, and RuntimeCatalog the four explicit stages.
- Keep `GameDataLoader` as the only pipeline orchestrator.
- Parse one source model, validate that instance once, build one runtime catalog, then create its lookup once.
- Remove duplicate ownership, duplicate validation, and implicit builder access to loader static state.
- Move combat-only skill compilation out of Loading.

## Constraints

- Preserve CSV schemas, IDs, values, ordering, asset paths, and generated runtime behavior.
- Preserve `GameDataLoader.CurrentCatalog`, `CsvRuntimeCatalog`, `GameDataCatalog`, and current namespaces.
- Preserve `RuntimeInitializeLoadType.BeforeSceneLoad` and the Resources path `Pakuri/CSVRuntime/CsvRuntimeCatalog`.
- Preserve serialized field names and move every existing `.meta` file with its script.
- Do not add asmdefs in this refactor.
- Editor synchronization may parse once to discover asset references, but full validation must run at most once for that parsed `SourceModel`.
- Runtime startup must call semantic validation exactly once before generation.
- Generation and lookup stages must not re-run source validation.
- Unity Play Mode gameplay verification remains user-owned.

## Role Owner

Code Builder

## Status

Implemented and locally verified. User-owned Play Mode gameplay verification remains.

## Next Actions

- User verifies representative gameplay flows in Unity Play Mode.

## Evidence

- Before the refactor, `GameDataLoader.LoadAndValidateRuntimeCatalog` performed load, parse, validate, build, and lookup registration in that order.
- Before the refactor, `CsvRowParser` owned both row conversion and whole-source loading.
- Before the refactor, `CsvSourceModel` owned a catalog validation helper despite being the intermediate model.
- Before the refactor, `CsvDataValidator.cs` contained both `CsvDataValidator` and `CsvAssetReferenceCollector`.
- Before the refactor, `GameDataCatalog.cs` contained both `GameDataCatalog` and `GameDataLookup`.
- Before the refactor, `GameDataCatalogBuilder` resolved Unity assets through `GameDataLoader.runtimeCsvCatalog`.
- Before the refactor, `SkillDefinitionCompiler.cs` contained `SkillDefinitionCompiler`, `SkillNodeMapper`, and `SkillChoiceCompiler`, while its inspected consumers were combat/spawn/UI paths after catalog loading.
- No asmdef file exists under `Pakuri/Assets`.
- Baseline runtime and editor C# builds completed with zero errors before implementation.
- The implemented tree places loading code under `Loading/Parsing`, `Loading/Validation`, `Loading/Generation`, and `Loading/RuntimeCatalog`; combat compilation is under `Combat/Skills/Compilation`.
- `rg` found two `ValidateSourceModelOrThrow(` references: one definition and one call in `GameDataLoader.BuildValidatedRuntimeCatalog`.
- `rg` found two `BuildRuntimeCatalog(` references: one definition and one call in the same loader method.
- `rg` found one `.RebuildLookup(` reference, immediately after the catalog build; `runtimeCsvCatalog` has zero references.
- The old `Assets/Scripts/Data` and `Assets/Scripts/GameFlow/Loading` directories no longer exist.
- All eleven moved scripts and the moved Loading folder retain their original Unity GUIDs; all newly extracted scripts have `.meta` files.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal /p:UseSharedCompilation=false` completed with zero errors and the two pre-existing assembly-version conflict warnings.
- Unity forced script compilation completed without project errors.
- Unity menu `Pakuri/Validate CSV Source Data` completed and loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- `git diff --check` completed without whitespace errors; its output contained only pre-existing LF-to-CRLF notices on user-owned files.

## History

- 2026-07-29: Designer inspected the eleven loading-related scripts and documented the current parsing, validation, generation, lookup, editor-sync, and combat-compilation boundaries.
- 2026-07-29: User approved a new Loading root with four responsibility folders and directed Code Builder to write this handoff before implementation.
- 2026-07-29: User required simple conventions, no unnecessary duplicate structure, and no repeated validation of an already validated source model.
- 2026-07-29: Code Builder implemented the responsibility split, removed the loader-static builder dependency, and made editor sync reuse its parsed `SourceModel`.
- 2026-07-29: Code Builder completed static checks, GUID checks, the runtime C# build, Unity compilation, and Unity CSV source validation.

## Target Structure

```text
Pakuri/Assets/Scripts/
├─ Loading/
│  ├─ GameDataLoader.cs
│  ├─ Parsing/
│  │  ├─ CsvRuntimeCatalog.cs
│  │  ├─ CsvSourceLoader.cs
│  │  ├─ CsvParser.cs
│  │  ├─ CsvRowParser.cs
│  │  ├─ CsvSourceModel.cs
│  │  ├─ SkillGraphParser.cs
│  │  └─ EditorSync/CsvCatalogEditor.cs
│  ├─ Validation/
│  │  ├─ CsvDataValidator.cs
│  │  └─ CsvAssetReferenceCollector.cs
│  ├─ Generation/
│  │  └─ GameDataCatalogBuilder.cs
│  └─ RuntimeCatalog/
│     ├─ GameDataCatalog.cs
│     └─ GameDataLookup.cs
└─ Combat/Skills/Compilation/
   ├─ SkillDefinitionCompiler.cs
   ├─ SkillNodeMapper.cs
   └─ SkillChoiceCompiler.cs
```

## Stage Contracts

```csharp
SourceModel CsvSourceLoader.LoadSourceModel(CsvRuntimeCatalog source);
void CsvDataValidator.ValidateSourceModelOrThrow(SourceModel source, CsvRuntimeCatalog assets);
GameDataCatalog GameDataCatalogBuilder.BuildRuntimeCatalog(SourceModel source, CsvRuntimeCatalog assets);
void GameDataCatalog.RebuildLookup();
```

`GameDataLoader` owns their order. Parsing owns structural requirements needed to materialize data. Validation owns cross-row and runtime-compatibility rules. Generation assumes validated input. RuntimeCatalog stores and indexes generated definitions.

## Single-Validation Rule

- `CsvSourceLoader.LoadSourceModel` may reject malformed CSV, duplicate IDs, wrong split-table runtime kinds, and invalid graph materialization because no usable source model can be produced.
- `CsvDataValidator.ValidateSourceModelOrThrow` owns semantic validation and runs once per produced `SourceModel`.
- `GameDataCatalogBuilder` resolves definitions and assets but does not validate source semantics again.
- `GameDataCatalog.RebuildLookup` indexes generated objects but does not validate source data.
- Editor sync-and-validate must reuse the `SourceModel` produced while synchronizing assets instead of parsing it again.

## Compatibility And Verification

- Preserve all public signatures used outside the pipeline.
- Keep `CsvCatalogEditor` in the runtime assembly behind `#if UNITY_EDITOR`; a Unity-special `Editor` folder would block its access to the pipeline's internal types.
- Preserve script GUIDs by moving existing `.meta` files.
- Build `Assembly-CSharp.csproj` and use Unity's compiler for the `#if UNITY_EDITOR` synchronization path.
- Confirm old `Scripts/Data` and `Scripts/GameFlow/Loading` C# paths are empty after moves.
- Confirm runtime startup has one call to `ValidateSourceModelOrThrow`.
- Confirm Editor sync-and-validate reuses one parsed source.
- Confirm no `runtimeCsvCatalog` static dependency remains.
- Use Unity-MCP for refresh, compilation, Console inspection, and CSV source validation.
