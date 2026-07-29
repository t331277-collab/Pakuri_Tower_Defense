# DATA_BLACKBOARD

## Archived History

The pre-cleanup file, including completed and superseded data tasks, is preserved at `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/DATA/DATA_BLACKBOARD.md`.

## Task: 2026-07-28 Skill Trigger / Node Data Contract Design

### Task title

Replace kind-branched graph authoring with Trigger-owned and owner-keyed Nodes.

### Goals

- Remove `graph_kind` from all six `skill_graph_nodes_*.csv`.
- Add no replacement grouping column or intermediate grouping type.
- Move Trigger payload fields into Trigger-owned Node data while Trigger rows retain activation rules.

### Constraints

- Role Owner is Designer for the handoff and Code Builder refactoring track for later implementation.
- Preserve all current IDs, values, asset paths, ordering, gates, and generated catalog behavior during migration.
- Keep the legacy graph reader until converted CSV and runtime parity pass.
- No active CSV was changed in this design task.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Superseded by the 2026-07-29 Trigger Final Outcome Generation Design.

### Next Actions

- No action. The preserved historical handoff is under `boards/ARCHIVE/`.
- Current work follows `boards/COMBAT/SKILL_TRIGGER_EXECUTOR_REUSE_HANDOFF.md`.

### Evidence

- `SkillGraphParser` currently branches graph kinds and rewrites Effect rows to generated Effect ownership.
- `GameDataCatalogBuilder` separately materializes ordinary Nodes and Effect definitions.
- Active graph authoring contains 508 Effect rows and 256 ordinary modifier rows.
- The handoff removes `graph_kind`, rejects replacement grouping IDs, defines owner-keyed Nodes, expands Trigger events, and specifies parser/validator/catalog/compiler migration.

### History

- 2026-07-28: User requested removal of `graph_kind`, rejection of the intermediate grouping term, and Trigger-based Node activation.
- 2026-07-28: Designer recorded the replacement data contract without editing CSV or runtime catalogs.
- 2026-07-28: Code Builder archived older DATA task history and retained this as the only active DATA task.
- 2026-07-29: User superseded runtime Trigger Node dispatch with final outcome generation and existing Executor reuse.

## Task: 2026-07-29 Trigger Visual Duration Data Repair

### Task title

Restore explicit one-second lifetime Nodes for standalone Trigger visuals.

### Goals

- Repair ten Trigger-owned Node collections whose `ShowVisual` rows had no `SetDuration`.
- Keep visual lifetime explicit in authoring data.

### Constraints

- No runtime fallback and no validator change.
- Preserve the 19-column Node CSV contract and contiguous owner-local `node_order`.
- Preserve all existing values and add only the missing duration rows.

### Role Owner

Code Builder

### Status

Complete except user-owned Play Mode verification.

### Next Actions

- User verifies one-second visual removal for representative OnExpire, OnHit, OnKill, OnOutgoingDamage, OnShieldExpire, and last-projectile events.

### Evidence

- Five graph CSV files received ten total `SetDuration=1` rows; the line-attack graph required no change.
- All six graph files retain a 19-column width for every header and row.
- Each repaired owner has exactly one positive duration Node, and the standalone non-positive Trigger visual count is zero.
- Unity CSV source validation completed without errors and the runtime catalog loaded 5/8/8 definitions.

### History

- 2026-07-29: User required explicit data duration and prohibited a runtime zero-duration fallback.
- 2026-07-29: Code Builder restored the ten missing lifetime Nodes from reference intent and pre-migration one-second behavior.

## Task: 2026-07-29 CSV Loading Pipeline Responsibility Refactor

### Task title

Reorganize CSV loading into one ordered pipeline with four responsibility folders.

### Goals

- Implement the approved Parsing, Validation, Generation, and RuntimeCatalog structure.
- Keep one parsed `SourceModel`, one semantic validation pass, one catalog build, and one lookup rebuild.
- Remove duplicate ownership and implicit static builder dependencies.

### Constraints

- Preserve current CSV, serialized asset, runtime catalog, public API, ordering, and gameplay behavior.
- Preserve existing `.meta` GUIDs and the runtime Resources path.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and non-Play-Mode verification complete.

### Next Actions

- User verifies representative gameplay flows in Unity Play Mode.

### Evidence

- The approved handoff records current file ownership, target paths, stage contracts, the single-validation rule, and compatibility gates.
- Baseline runtime and editor C# builds completed with zero errors before implementation.
- Loading now has explicit Parsing, Validation, Generation, and RuntimeCatalog folders; combat skill compilation moved to `Combat/Skills/Compilation`.
- Static search found one semantic-validation call, one catalog-build call, and one lookup-rebuild call in the ordered loader path.
- Static search found zero references to the removed `runtimeCsvCatalog` loader state.
- All moved scripts retain their original GUIDs, and all new scripts have `.meta` files.
- `Assembly-CSharp.csproj` built with zero errors; Unity compiled without project errors.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.

### History

- 2026-07-29: User selected Code Builder, required the handoff MD first, and authorized implementation from that MD.
- 2026-07-29: User prohibited unnecessary duplicate structure and repeat validation of an already validated source model.
- 2026-07-29: Code Builder completed the handoff implementation and all available non-Play-Mode checks.

## Task: 2026-07-29 Ponytail Loading Pipeline Simplification

### Task title

Delete dead CSV-loading code and merge duplicate lookup and handler ownership.

### Goals

- Keep the Parsing -> Validation -> Generation -> RuntimeCatalog pipeline behavior.
- Delete unused parser, DTO, validator, builder, and skill-handler metadata.
- Merge runtime lookup storage into `GameDataCatalog`.

### Constraints

- Ponytail leads the implementation; existing markdown is reference material only.
- Preserve active CSV contracts, serialized fields, public lookup APIs, and gameplay behavior.
- Preserve unrelated pre-existing working-tree changes.

### Role Owner

Code Builder, ponytail-led

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative CSV loading and skill execution in Unity Play Mode.

### Evidence

- `Loading` changed from 13 C# files and 7,084 lines to 12 C# files and 5,718 lines: net reduction 1,366 lines.
- `GameDataLookup.cs` and its `.meta` were removed; lookup registration and queries now live in `GameDataCatalog.cs`.
- Static search found zero remaining removed-symbol or block-comment matches and retained the single ordered validation, build, and lookup-rebuild calls.
- Every remaining Loading C# file has a `.meta` file.
- Runtime and Editor `dotnet build` checks completed with zero errors; the Unity EditMode test passed 1/1.
- Unity finished script compilation idle and ready with zero `Assets/Scripts/Loading` console errors; one separate MCP package transport error was present.

### History

- 2026-07-29: User assigned Code Builder and required ponytail-led deletion, consolidation, and a final net-line-reduction report.
- 2026-07-29: Code Builder removed dead data and helpers, deleted duplicate handler metadata, merged lookup ownership, and completed static, build, EditMode, and Unity console checks.

## Task: 2026-07-29 Final Skill Catalog Generation Design

### Task title

Make Loading Generation produce final typed skill data once.

### Goals

- Make `GameDataCatalogBuilder` directly create final active, passive, Choice, Trigger, and Node data.
- Parse Node and Trigger enum/list/condition authoring strings into final typed values exactly once in Generation.
- Make `GameDataCatalog` index final data instead of Source Definition wrappers.
- Prevent repeated validation, Definition compilation, Trigger compilation, and Choice Node parsing.

### Constraints

- Keep the existing Parsing -> Validation -> Generation -> RuntimeCatalog order.
- Keep exactly one semantic validation, one build, and one lookup rebuild.
- Preserve CSV schemas, values, IDs, ordering, asset paths, and runtime behavior.
- Avoid duplicate handler-support lists between Validator and Builder.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Code Builder implementation and available non-Play-Mode verification complete. Phases 1-6 complete.

### Next Actions

- User verifies representative CSV-loaded active, passive, enhancement, master, Trigger, and enemy skill behavior in Unity Play Mode.

### Evidence

- `GameDataLoader.BuildValidatedRuntimeCatalog` currently calls validation, catalog build, and lookup rebuild once each.
- `GameDataCatalogBuilder` currently stops at Source Definition and string-param Node Definition creation.
- Combat compiler scripts perform a second static conversion during unit state rebuild or first Choice use.
- `SkillNodeExecutor` and `SkillTrigger` still parse authored scope, policy, condition, status, runtime-kind, Choice, attribute, event-skill, and event-source values during execution.
- Final Loading and Combat contracts are specified in `boards/COMBAT/SKILL_DIRECT_CATALOG_RUNTIME_HANDOFF.md`.
- Phase 1 baseline `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` builds completed with zero errors.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies; the EditMode test job succeeded.
- Phase 2 added the final typed contracts that Generation will populate directly: final Choice Nodes, Node target IDs, typed status conditions, typed Trigger lists, and event source scope.
- Phase 3 Generation now produces and indexes final active, passive, enemy, Choice, Trigger, and Node data once.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies through the final catalog path.
- Phase 4 runtime consumers use final Choice Nodes, typed Trigger arrays/scope, and final SkillDefinition lookup values directly.
- Runtime Execution/Trigger/StatusRules search found zero authored `Split`, `Enum.Parse`, or `TryParse` calls.
- Runtime and Editor builds completed with zero errors; Unity CSV validation retained 5 monsters and 8/8 enemies.
- Phase 5 integrated final skill/Choice/Node creation into `GameDataCatalogBuilder` partials and removed all compiler/mapper symbols.
- `UnitSkills` owns learned-ID/Choice application; `StatusRuntimeCompiler.CompileTriggers` was deleted as dead code.
- All 18 moved script GUID pairs matched; Runtime/Editor builds and Unity CSV validation passed.
- Phase 6 confined temporary build contracts to Loading/Generation and removed duplicate public Source/Definition contracts from Combat.
- Removed-symbol, runtime parsing, and Generation-outside Definition-mutation searches all returned zero.
- EditMode target-filter/reference-reuse tests passed 2/2; solution build and Unity script compilation completed with zero errors; CSV validation retained 5/8/8.
- Whole-task C# diff is 909 additions and 1,069 deletions: net reduction 160 lines.

### History

- 2026-07-29: User approved direct use of final authored skill data and requested a Code Builder-ready design.
- 2026-07-29: Designer recorded the cross-domain Loading/Combat handoff without changing runtime code or CSV.
- 2026-07-29: Designer updated the Generation contract so encoded authoring strings are converted once and final runtime consumers receive enum/array values.
- 2026-07-29: Code Builder completed Phase 1 baseline protection before changing the final data contracts.
- 2026-07-29: Code Builder completed Phase 2 final typed contracts with the current compiler retained only as an intermediate compatibility path.
- 2026-07-29: Code Builder completed Phase 3 final catalog generation and final-type RuntimeCatalog indexing.
- 2026-07-29: Code Builder completed Phase 4 final catalog direct runtime consumption.
- 2026-07-29: Code Builder completed Phase 5 Generation ownership integration and Combat skill folder migration.
- 2026-07-29: Code Builder completed Phase 6 temporary-contract cleanup and full non-Play-Mode verification.

## Task: 2026-07-29 Trigger Final Outcome Generation Design

### Task title

Generate final triggered skill Definitions or typed state commands once.

### Goals

- Convert Trigger-owned authored Nodes into final concrete `SkillDefinition` references or typed non-skill commands in Generation.
- Stop building runtime `SkillNode[]` payloads for Trigger execution.
- Reuse existing catalog Definitions for the four current `ExecuteSkill` mappings.
- Preserve Choice/base modifier Node generation.

### Constraints

- Keep the current Parsing -> Validation -> Generation -> RuntimeCatalog order.
- Do not add a new CSV schema or C# script unless inspected code proves it necessary.
- Preserve IDs, values, ordering, asset paths, dynamic event-value semantics, and one validation/build/lookup flow.
- Do not silently activate the 81 current modifier-only owners without owner-level evidence.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Phase 3 final Definition runtime consumption complete. Phase 4 Trigger Node payload removal pending.

### Next Actions

- Execute the 22 generated typed commands through existing runtime APIs.
- Remove legacy Trigger Node payload generation and consumption.

### Evidence

- No Trigger owner currently combines two delivery kinds.
- No Trigger owner currently combines delivery and a non-skill state command.
- Current delivery shapes map to existing Single, Zone, Buff, and Shield Definition families.
- Event-derived damage uses shield-applied, shield-absorbed, or event-applied damage snapshots.
- The full Generation mapping and validation rules are recorded in the handoff.
- Phase 1 confirmed 158 Trigger rows and 606 Trigger-owned Node rows with runtime/editor build error 0.
- Phase 2 generated 55 final Definitions and 22 typed commands while retaining 81 current no-action owners.
- Unity EditMode catalog verification passed 1/1 and both C# builds completed with error 0.
- Phase 3 consumes 55 final Definitions through the existing family dispatch without catalog registration of hidden Definitions.
- Runtime/editor builds completed with error 0; `SkillCatalogRuntimeTests` passed 3/3.

### History

- 2026-07-29: User selected existing Executor reuse instead of runtime Trigger Node effect dispatch.
- 2026-07-29: Designer recorded the corresponding final Generation outcome contract.
- 2026-07-29: User approved Code Builder implementation; Phase 1 fixed the current owner and build baseline.
- 2026-07-29: Code Builder completed Phase 2 final outcome Generation and focused catalog verification.
- 2026-07-29: Code Builder completed Phase 3 runtime consumption with source snapshot, lifecycle, target, and dynamic-value policies.
