# Skill Trigger / Node Unification Handoff

## Task title

Unify skill activation under Trigger and define every payload as ordered Nodes.

## Goals

- Delete the `graph_kind` column from every active `skill_graph_nodes_*.csv`.
- Remove the rejected intermediate grouping term from the authoring, parsing, validation, catalog, runtime type, cache, method, field, and documentation contracts changed by this migration.
- Define graph rows only as Nodes.
- Make Trigger the sole authority for when a triggered Node collection runs.
- Make ordered `SkillNode[]` the authority for what that activation does.
- Keep `SkillNode` as the compiled container for one strongly typed operation.
- Remove `SkillEffectDefinition` and family-specific additional-effect dispatch after behavior-preserving migration.

## Constraints

- Role Owner for implementation is Code Builder refactoring track.
- This handoff is design-only; no C#, CSV, prefab, scene, or generated catalog was changed.
- Preserve current player-facing damage, status, shield, targeting, timing, delay, repeat, visual, hitbox, recast, cooldown, reload, Choice, Passive, and Trigger behavior.
- Preserve current authored IDs, numeric values, ordering, and asset paths.
- Do not delete the current Effect runtime before converted Nodes and Trigger events have parity evidence.
- Do not introduce a replacement grouping ID, grouping column, grouping wrapper, or renamed form of the removed concept.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer execution requires explicit user permission.

## Current inspected evidence

- Active graph CSV headers currently contain `graph_kind`.
- `SkillGraphParser.SkillGraphKind` currently branches ordinary modifier rows and Effect rows.
- `SkillGraphParser` rewrites Effect rows into generated Effect ownership while leaving ordinary rows under Skill, Choice, Passive, or Trigger ownership.
- `SkillEffectDefinition` owns timing, Choice/Passive gates, source-status requirements, target selection, conditions, damage/status payload, lifetime, recast, and visual data.
- `SkillTriggerDefinition` separately owns event selection, Choice/status gates, proc chance, internal cooldown, delay/repeat, targeting, damage payload, triggered skill/effect IDs, cooldown refund, reload reduction, and visual data.
- Current Effect timing supports `OnCast`, `OnDeploymentCast`, `Delayed`, `OnHit`, `OnExpire`, and `OnHitCount`.
- Current Trigger events do not cover all those lifecycle points. `OnOutgoingDamage` is not an exact replacement for `OnHit`.
- The current `SkillExecutionData` node-application method applies compiled modifier operations but is a snapshot accumulator, not an event payload dispatcher.
- The current Choice-target runtime wrapper stores `SkillNode[]` for caching.
- Active authoring contains 508 Effect graph rows and 256 ordinary modifier graph rows under `Pakuri/Assets/CSVdata/authoring/monster/skills/choices`.
- Repository search found 15 current C# files referencing `SkillEffectDefinition`.
- No current `SkillEffect.cs`, `SkillNodeEffectExecutor.cs`, or `SkillHitExecutor.cs` script exists.

## Final responsibility rule

```text
Trigger = when
Node = what
SkillNode = one compiled operation
SkillActionContext = runtime event data supplied to Nodes
SkillNodeExecutor = ordered Node dispatcher
```

`Effect` and the rejected intermediate grouping concept are not retained as runtime definitions, graph kinds, grouping IDs, execution pipelines, or new type names.

## Target runtime flow

```text
Core skill cast / combat event / actor lifecycle event
    |
    v
SkillTrigger
  - event match
  - Choice/Passive/status requirements
  - proc and internal cooldown
  - delay and repeat
  - resolve Trigger-owned Nodes
    |
    v
ordered SkillNode[]
    |
    v
SkillNodeExecutor
  - modifier Node -> SkillExecutionData
  - damage Node -> combat damage API
  - status Node -> shared status API
  - target Node -> shared targeting API
  - visual Node -> EffectManager
  - execute-skill Node -> SkillExecution
  - recast Node -> family executor
  - cooldown/reload Node -> SkillUseState
```

## Target node ownership

Nodes are retrieved by existing ownership fields instead of a grouping ID.

```text
NodeOwnerKey
  = owner_kind
  + owner_id
  + target_skill_id
```

Ownership:

- `owner_kind=Skill`: base skill Nodes.
- `owner_kind=Choice`: Enhancement or Master Nodes.
- `owner_kind=Passive`: Passive-owned Nodes.
- `owner_kind=Trigger`: Nodes executed when the matching Trigger runs.

For Trigger ownership:

```text
skill trigger row.trigger_id
    ==
skill graph row.owner_id where owner_kind=Trigger
```

Former Skill/Choice/Passive Effect groups receive explicit Trigger rows. Their migrated payload Nodes use `owner_kind=Trigger` and that Trigger's ID as `owner_id`.

Ordinary always-applied Skill, Choice, and Passive modifier Nodes remain under their existing owners and are applied during execution-data construction. Runtime may create an internal `BuildExecutionData` activation event, but authoring does not need one manual Trigger row per ordinary modifier owner.

## Target CSV contract

The user-referenced files are CSV files, not C# scripts:

- `skill_graph_nodes_projectile.csv`
- `skill_graph_nodes_buff.csv`
- `skill_graph_nodes_single_attack.csv`
- `skill_graph_nodes_line_attack.csv`
- `skill_graph_nodes_area_attack.csv`
- `skill_graph_nodes_passive.csv`

Final header:

```text
monster_id
owner_kind
owner_id
target_skill_id
node_order
node_type_id
arg_1 ... arg_12
excludes_active_choice_id
```

Deleted column:

```text
graph_kind
```

No replacement grouping column is added.

Each row defines one Node. Multiple rows with the same owner key form one deterministic ordered Node collection through `node_order`.

## Former Effect node migration

Existing Effect node types become ordinary Node operations:

- `EffectDamage` -> `ApplyDamage`
- `ApplyStatus` remains `ApplyStatus`
- `EffectExtendStatusDuration` -> `ExtendStatusDuration`
- `RecastZone` remains `RecastZone`
- `EffectTarget` -> target specification Node
- `ConditionStatus` -> condition Node
- `EffectLifetime` -> duration/lifetime Node
- `RuntimeEffectVisual` -> visual specification Node

The old generated Effect ID and inferred Effect grouping logic are removed. If one old owner currently has multiple independently triggered effects, each becomes a separate explicit Trigger ID and its Nodes move to that Trigger owner.

## Trigger data contract

Trigger definitions retain activation concerns:

```text
trigger_id
source_skill_id
trigger_event
requirements
proc_chance
internal_cooldown_seconds
trigger_delay_seconds
repeat_count
repeat_interval_seconds
```

The Trigger's `trigger_id` is also its Node owner ID. No replacement grouping ID is added.

Move current Trigger payload fields such as damage, radius, status, target shape, triggered skill ID, cooldown refund, and reload reduction into Trigger-owned Nodes.

Add the missing lifecycle events before removing Effect timing:

- `BuildExecutionData`
- `OnCast`
- `OnDeploymentCast`
- `OnHit`
- `OnExpire`
- `OnHitCount`

Keep current global combat events:

- `OnMagazineLastProjectileHit`
- `OnShieldExpire`
- `OnShieldAbsorb`
- `OnStatusExpire`
- `OnOutgoingDamage`
- `OnKill`
- `OnSkillCast`
- `CombatStart`

`Delayed` is not an event. Delay remains Trigger scheduling data.

## Parser changes

### `SkillGraphParser.cs`

- Delete `SkillGraphKind`.
- Delete `SkillGraphNodeRow.GraphKind`.
- Stop reading `graph_kind`.
- Delete Effect-owner rewriting.
- Delete generated Effect IDs and inferred Effect grouping.
- Materialize every CSV row as one `SkillNodeRow` using the authored `owner_kind` and `owner_id`.
- Preserve `target_skill_id`, `node_order`, `node_type_id`, arguments, and exclusion fields.
- Resolve `node_type_id` to `handler_id`, `node_kind`, and parameter definitions exactly once.
- Rename any local variables, comments, errors, and helpers that retain the removed term.

## Validation changes

### Schema validation

- Reject a graph CSV that still contains `graph_kind` after migration completion.
- Require the final Node header and column count for every `skill_graph_nodes_*.csv`.
- Validate `node_type_id` against `skill_node_definitions.csv`.
- Validate every argument against `skill_node_definition_params.csv`.
- Validate unique `(owner_kind, owner_id, target_skill_id, node_order)` keys.
- Validate deterministic contiguous ordering according to the repository's chosen ordering rule.

### Ownership validation

- Validate Skill owner IDs against active skills.
- Validate Choice owner IDs against active choices.
- Validate Passive owner IDs against active passives.
- Validate Trigger owner IDs against active triggers.
- Reject Trigger-owned Nodes without one matching Trigger.
- Reject Trigger rows without at least one owned Node unless an explicitly supported no-payload Trigger remains.
- Validate owner monster and target skill monster consistency.

### Handler validation

- Delete ordinary-kind-versus-Effect handler validation.
- Delete Effect-only handler classification.
- Validate handler compatibility through `node_kind`, owner kind, required runtime context, and supported executor operation.
- A snapshot-only modifier Node must not be accepted under an event-only Trigger if the executor cannot apply it safely.
- A runtime action Node requiring target/center data must declare and validate those requirements.

### Reference validation

- Validate status IDs, skill IDs, Choice IDs, Passive IDs, asset paths, and Trigger IDs from Node parameters.
- Validate exclusions and requirements after former Effect gates move to Trigger or condition Nodes.
- Validate that migrated OnHit actions do not rely on `OnOutgoingDamage` as an alias.
- Validate bounded recursion for execute-skill and recast Nodes.

### Runtime support validation

- Rename the current processability helper -> `CanProcessNode`.
- Rename the current runtime-handler classifier -> `IsRuntimeNodeHandler`.
- Remove messages and comments containing the removed term.
- Require every active Node handler to have one compiled operation and one executor route.

## Catalog and compiler changes

### `GameDataCatalogBuilder.cs`

- Delete `BuildSkillEffects(...)`.
- Delete `BuildEffectOwnedSkillEffects(...)`.
- Delete Effect-owner grouping/materialization and generated Effect lookup.
- Build Skill, Choice, Passive, and Trigger-owned `SkillNodeDefinition[]`.
- Attach Trigger-owned Node definitions to `SkillTriggerDefinition` or an owner-keyed runtime lookup.
- Rename every normalized-node field to `NormalizedNodes`.
- Rename methods, comments, and local variables that retain the removed term.

### `SkillDefinitionCompiler.cs`

- Keep `SkillNodeMapper`.
- Rename the Choice runtime lookup to `GetChoiceRuntimeNodes`.
- Rename `MapSkillNodeDefinition` only if needed for naming consistency; the existing name already describes Nodes.
- Rename the processability helper to `CanProcessNode`.
- Rename the runtime-handler classifier to `IsRuntimeNodeHandler`.
- Compile all supported node handlers into strongly typed `SkillNode` operations.
- Cache `SkillNode[]` directly by owner/target rather than wrapping them in a type containing the removed term.

### `CsvDataValidator.cs`

- Remove `graph_kind` and ordinary-kind/Effect branch checks.
- Add final header, owner, Trigger-node, handler-context, reference, and executor-support checks.
- Reject stale legacy rows and orphan Trigger-owned Nodes.

## New scripts

### `Combat/Skills/Execution/SkillActionContext.cs`

- Immutable runtime context for one Node execution.
- Owns source unit, source skill ID, event target, event center, event damage, hit count, and current `SkillExecutionData`.
- Replaces duplicated event parameters and the current Trigger-only execution context.
- Does not choose targets or execute Nodes.

### `Combat/Skills/Execution/SkillNodeExecutor.cs`

- Sole dispatcher for ordered compiled Nodes.
- Routes modifier Nodes into `SkillExecutionData`.
- Routes runtime action Nodes into existing shared damage, status, targeting, visual, skill execution, cooldown, reload, and recast APIs.
- Does not decide when Nodes run.

No grouping-wrapper script is created.

## Existing scripts retained with final responsibility

### `Combat/Skills/Choices/SkillNode.cs`

- Retain.
- Store exactly one strongly typed operation.
- Add operation structs only when a migrated payload has no existing Node representation.
- No event matching, target search, scheduling, or direct combat mutation.

### `Combat/Skills/Execution/SkillExecutionData.cs`

- Retain.
- Own one cast's accumulated scalar modifiers and deferred rule data.
- Rename the node-application method to `ApplyNodes`.
- Remove direct Effect collection and execution.
- Accept modifier operations from `SkillNodeExecutor`.

### `Combat/Skills/Execution/SkillExecutionRuleResolver.cs`

- Retain.
- Resolve target/current-state-dependent Node rules such as conditional damage, conditional critical chance, burst rules, and source-status requirements.
- Do not make it a Trigger or general Node executor.

### `Combat/Skills/SkillType/Trigger/SkillTrigger.cs`

- Retain and reduce to activation authority.
- Receive lifecycle and combat events.
- Match requirements, proc, internal cooldown, delay, and repeat.
- Resolve Trigger-owned `SkillNode[]` and invoke `SkillNodeExecutor`.
- Remove direct damage/status/skill/cooldown/reload payload implementations after Node parity exists.

### `Combat/Skills/Execution/SkillExecution.cs`

- Retain.
- Execute core skills and publish cast lifecycle events.
- Build the initial execution snapshot from Skill/Choice/Passive-owned Nodes.
- Remain the entry point for manually, automatically, and Trigger-invoked core skills.

### Family Executors and Actors

Retain:

- `ProjectileSkillExecutor.cs`
- `ProjectileSkillActor.cs`
- `LineSkillExecutor.cs`
- `LineSkillActor.cs`
- `ZoneSkillExecutor.cs`
- `ZoneSkillActor.cs`
- `SingleSkillExecutor.cs`
- `SingleSkillActor.cs`
- `BuffSkillExecutors.cs`
- `PassiveSkill.cs`

Final responsibility:

- Execute family-specific core behavior.
- Publish lifecycle events with `SkillActionContext`.
- Stop selecting and directly executing `SkillEffectDefinition[]`.

### Shared systems

Retain:

- `SkillTargeting.cs`: shared target resolution.
- `StatusRules.cs`: shared status composition/application.
- `StatusRuntimeCompiler.cs`: compile status data used by Nodes.
- `EffectManager`: visual object creation and cleanup.
- `GameDataCatalogBuilder.cs`: build Trigger definitions and owner-keyed Nodes.
- `SkillGraphParser.cs`: parse one Node graph model without kind branching.

## Whole-script deletion list

No current whole C# script is approved for immediate deletion.

Reason:

- Current Effect behavior is embedded across retained Definition, Builder, Status, Targeting, Trigger, Executor, and Actor scripts.
- The previously separate `SkillEffect.cs`, `SkillNodeEffectExecutor.cs`, and `SkillHitExecutor.cs` scripts do not exist.
- `SkillExecutionRuleResolver.cs`, `SkillTrigger.cs`, `SkillNode.cs`, and every family Executor/Actor retain non-Effect responsibilities.

## Code sections and symbols deleted after parity

### `SkillDefinition.cs`

- `SkillEffectDefinition`
- `SkillMultiEffectKind`
- `SkillMultiEffectTiming`
- Effect-only fields and `MultiEffects` arrays
- Trigger payload fields moved into Trigger-owned Nodes
- `SkillTriggerActionKind` after every Trigger executes owned Nodes
- the Choice-only runtime Node wrapper
- every normalized-node field using the rejected term

Target-side/selection/shape/center enums may remain or receive Node-oriented names if Node operations still use them.

### `GameDataCatalogBuilder.cs`

- `BuildSkillEffects(...)`
- `BuildEffectOwnedSkillEffects(...)`
- Effect-owner grouping/materialization
- `ApplyEffectOwnedSkillEffectOperationNode(...)`
- generated Effect ID helpers
- stale term-bearing fields, methods, comments, and local variables

### `SkillGraphParser.cs`

- `SkillGraphKind`
- `GraphKind`
- `graph_kind` parsing
- Effect-owner rewriting
- inferred Effect grouping and IDs
- ordinary-kind-versus-Effect handler validation

### `SkillDefinitionCompiler.cs`

- the Choice-only runtime Node wrapper
- the current Choice runtime lookup
- the current processability helper
- the current runtime-handler classifier
- stale term-bearing comments and local variables

Replacement is direct owner/target `SkillNode[]` caching and Node-oriented method names.

### `StatusRuntimeCompiler.cs`

- `CompileSkillEffects(...)`
- Effect-specific compilation entry points

Keep shared status compilation used by Node operations.

### `StatusRules.cs`

- `EffectStatusSpec(...)`
- Effect-specific condition/application entry points

Move reusable status composition into Node action APIs instead of deleting shared status logic.

### `SkillTargeting.cs`

- Effect-specific overloads accepting `SkillEffectDefinition`

Keep generic target selection operating on Node target specifications.

### `SkillTrigger.cs`

- `ExecuteEffectAction(...)`
- `TriggeredEffect(...)`
- direct payload switch branches after Node replacements exist
- Trigger payload calculations moved into `SkillNodeExecutor`

### Family Executors and Actors

- `ExecuteAdditionalEffects(...)`
- `TimedEffects(...)`
- `OnHitEffects(...)`
- `OnExpireEffects(...)`
- stored `SkillEffectDefinition[]`
- family-local Effect condition, target, timing, and payload dispatch

Replace each removed path with lifecycle event publication.

## Final script structure

```text
Combat/Skills/
├─ Choices/
│  └─ SkillNode.cs
│     └─ compiled operation container
│
├─ Execution/
│  ├─ SkillActionContext.cs       [new]
│  │  └─ event data for Node execution
│  ├─ SkillNodeExecutor.cs        [new]
│  │  └─ sole ordered Node dispatcher
│  ├─ SkillExecutionData.cs
│  │  └─ one-cast modifier snapshot
│  ├─ SkillExecutionRuleResolver.cs
│  │  └─ dynamic Node rule resolution
│  └─ SkillExecution.cs
│     └─ core skill execution and lifecycle event publication
│
├─ SkillType/Trigger/
│  └─ SkillTrigger.cs
│     └─ event/condition/scheduling authority
│
├─ SkillType/{Projectile,Line,Zone,Single,Buff,Passive}/
│  └─ family Executors and Actors
│     └─ family behavior and lifecycle event publication
│
└─ Definitions/
   └─ SkillDefinition.cs
      └─ Skill, Choice, Trigger, Node definitions
```

## Migration phases

### Phase 1: Node execution foundation

- Add `SkillActionContext` and `SkillNodeExecutor`.
- Replace Choice-only runtime wrapper with direct owner/target `SkillNode[]` caching.
- Keep the current Effect pipeline active.

Rollback point: new code remains unused.

### Phase 2: Trigger event coverage

- Add missing lifecycle events.
- Make family Executors/Actors publish events while current Effect execution still runs.
- Prevent one event from executing both legacy Effect and migrated Nodes.

Rollback point: disable new Trigger-to-Node routing.

### Phase 3: Node operation coverage

- Add or reuse Node operations for damage, status, duration extension, target selection, visuals, recast, skill execution, cooldown refund, and reload reduction.
- Move Trigger payload ownership into Trigger-owned Nodes.

Rollback point: Trigger keeps one legacy action branch per unmigrated action.

### Phase 4: Parser and validator dual support

- Add final Node-only parsing and validation.
- Temporarily support legacy input only inside a contained migration adapter.
- Validate exact owner and Trigger references before converting active CSV.

Rollback point: active authoring remains on the old schema.

### Phase 5: CSV migration

- Remove `graph_kind` from all six `skill_graph_nodes_*.csv`.
- Convert 508 current Effect rows into Trigger-owned Nodes.
- Convert former Effect groups into explicit Trigger IDs.
- Preserve ordinary Skill/Choice/Passive Node ownership.

Rollback point: retain a verified copy of the pre-migration authoring state until runtime parity passes.

### Phase 6: Consumer migration

- Migrate Projectile, Line, Zone, Single, Buff, Passive, and Trigger consumers one family at a time.
- Compare each family against current behavior before moving to the next.

Rollback point: family-specific legacy Effect path remains until that family passes.

### Phase 7: Legacy deletion

- Remove `SkillEffectDefinition`, Effect materialization, Effect timing, and family Effect dispatch.
- Remove Trigger payload branches and fields represented by Nodes.
- Remove legacy migration adapter.
- Confirm repository contains no removed term in active changed contracts.

No rollback after this phase without restoring the prior data/runtime contract.

## Compatibility constraints

- Existing Choice IDs, Skill IDs, Passive IDs, Trigger IDs, status IDs, asset paths, and numeric values remain unchanged.
- Preserve execution ordering when multiple Nodes share one owner and event.
- Preserve `ApplyOnce`, proc chance, internal cooldown, delay, repeat, hit-count, source-status, Choice, Passive, and exclusion gates.
- Preserve event-target versus selected-target behavior.
- Preserve OnHit behavior when damage is zero, blocked, or status-only.
- Preserve projectile/zone actor lifetime snapshots and OnExpire execution after the source cast has ended.
- Prevent triggered skill or recast recursion using current source/generation guards.
- Preserve generated runtime catalog compatibility until final deletion.

## Edge cases

- Snapshot modifier Nodes and runtime action Nodes must execute only in valid runtime contexts.
- Multiple projectiles from one cast must retain per-projectile center, target, launch index, and last-projectile state.
- Delayed Trigger execution needs an immutable context copy.
- Destroyed targets before delayed execution need defined cancellation or fallback.
- Status-only OnHit actions must run without outgoing damage.
- OnExpire must execute exactly once.
- Triggered Node recursion must be bounded.
- Multiple matching Triggers and multiple owned Nodes must preserve deterministic order.
- One Trigger owning zero Nodes must fail validation unless explicitly allowed.
- Two Triggers must not accidentally share one owner ID.

## Acceptance criteria

- All six active `skill_graph_nodes_*.csv` files contain no `graph_kind` column.
- No replacement grouping ID column exists.
- Every graph row defines one valid Node.
- Every Trigger-owned Node has one matching Trigger.
- Every Trigger requiring a payload owns at least one Node.
- Repository search finds zero `SkillEffectDefinition`, `SkillMultiEffectTiming`, `SkillGraphKind`, and zero occurrences of the rejected intermediate terminology across active authoring, parser, validator, catalog, compiler, and runtime contracts.
- Repository search finds no removed term in active parser, validator, catalog, compiler, runtime, CSV headers, or new documentation produced for this architecture.
- `SkillTrigger` is the activation gate for event-driven Nodes.
- `SkillNodeExecutor` is the sole compiled runtime-action dispatcher.
- Base, Enhancement, Master, Passive, and Trigger behavior remains numerically and visually equivalent.
- No duplicate OnCast, OnHit, OnExpire, or OnHitCount action occurs.
- Runtime and Editor projects compile with zero errors.
- CSV source validation completes with zero errors.

## Verification expected from Code Builder

- Record repository search counts for all legacy and replacement symbols before and after each phase.
- Validate the exact final header and row width of all six graph CSV files.
- Validate owner, Trigger, target skill, node type, parameter, status, skill, Choice, Passive, asset, exclusion, and recursion references.
- Add edit-mode or pure C# tests for Trigger matching, Node order, delayed context snapshots, and recursion guards where the current test surface permits.
- Run `dotnet build Pakuri/Pakuri.sln --no-restore`.
- Run Unity script compilation and inspect Console errors.
- Run the project's CSV source validation.
- Produce representative data parity evidence for every skill family.
- User performs Play Mode verification for representative Base, Enhancement, Master, Passive, OnHit, OnExpire, OnKill, delayed, status-only, recast, and triggered-skill cases.

## Related board files

- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`

## Role Owner

Designer for this handoff. Code Builder refactoring track for implementation after explicit user request.

## Status

Design handoff revised to the Node-only contract. Implementation not started.

## Next Actions

- User approves or revises the Trigger/Node target contract.
- Code Builder implements Phase 1 only, retaining the legacy Effect path as rollback.
- Later phases proceed only after preceding compile/data evidence.

## Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Definitions/SkillDefinition.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Choices/SkillNode.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionData.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRuleResolver.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/SkillType/Trigger/SkillTrigger.cs`
- `Pakuri/Assets/Scripts/GameFlow/Loading/GameDataCatalogBuilder.cs`
- `Pakuri/Assets/Scripts/GameFlow/Loading/SkillDefinitionCompiler.cs`
- `Pakuri/Assets/Scripts/Data/SkillGraphParser.cs`
- `Pakuri/Assets/Scripts/Data/CsvDataValidator.cs`
- Current repository searches: 508 Effect graph rows, 256 ordinary modifier graph rows, and 15 C# files referencing `SkillEffectDefinition`.

## History

- 2026-07-28: User selected Trigger as activation authority and Nodes as payload authority.
- 2026-07-28: Designer created the first handoff using an intermediate grouping concept.
- 2026-07-28: User rejected `graph_kind`, the intermediate grouping term, and any definition of that intermediate concept.
- 2026-07-28: Designer revised the handoff to a Node-only CSV, parser, validator, catalog, compiler, cache, and runtime contract.
