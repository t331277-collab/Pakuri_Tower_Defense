# STATUS_EFFECT_BLACKBOARD

## Archived History

The pre-cleanup file, including all completed July tasks, is preserved at `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.

## Task: 2026-07-28 Skill Trigger / Node Unification Design

### Task title

Design Trigger as the sole activation authority and ordered Nodes as the sole payload authority.

### Goals

- Remove `graph_kind`, Effect runtime ownership, and the removed intermediate terminology after a behavior-preserving migration.
- Route former Effect timing and payload through Trigger-owned Nodes.
- Keep `SkillNode` as one compiled operation container.

### Constraints

- Role Owner is Designer for the handoff and Code Builder refactoring track for later implementation.
- Preserve current damage, status, shield, timing, targeting, delay, repeat, visual, recast, Choice, Passive, and Trigger behavior.
- Do not delete the legacy Effect path before migrated family parity exists.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Superseded by the 2026-07-29 Trigger Executor Reuse Design.

### Next Actions

- No action. The preserved historical handoff is under `boards/ARCHIVE/`.
- Current work follows `boards/COMBAT/SKILL_TRIGGER_EXECUTOR_REUSE_HANDOFF.md`.

### Evidence

- `SkillEffectDefinition` and `SkillTriggerDefinition` both own timing/conditions/target/payload axes.
- Current Trigger events do not cover all current Effect timings.
- Active authoring contains 508 Effect graph rows and 256 ordinary modifier graph rows.
- Fifteen current C# files reference `SkillEffectDefinition`.
- The superseded full design is preserved at `boards/ARCHIVE/SKILL_TRIGGER_NODE_UNIFICATION_HANDOFF_2026-07-28.md`.

### History

- 2026-07-28: User selected Trigger-to-Node as the unified execution direction and removed `graph_kind` plus the rejected intermediate terminology.
- 2026-07-28: Designer created the implementation handoff without changing runtime code or CSV.
- 2026-07-28: Code Builder archived older COMBAT task history and retained this as the only active COMBAT task.
- 2026-07-29: User superseded direct Trigger Node dispatch with existing family Executor reuse.

## Task: 2026-07-29 Trigger Visual Duration Restoration

### Task title

Restore the pre-migration lifetime of standalone Trigger visuals.

### Goals

- Add one positive `SetDuration` Node to each of the ten standalone Trigger visual owners that lost its lifetime during Node migration.
- Preserve the prior one-second visual lifetime without adding a runtime fallback.

### Constraints

- Runtime and validator code remain unchanged.
- Damage, targeting, status payload, visual assets, Trigger gates, and existing Node order remain unchanged.
- `Pakuri/reference/2.Monster` remains the gameplay-intent source; the pre-migration runtime is the exact lifetime source.

### Role Owner

Code Builder

### Status

CSV implementation and non-Play-Mode verification complete. User Play Mode verification remains.

### Next Actions

- User verifies that representative Trigger visuals disappear after one second and that damage still occurs exactly once.

### Evidence

- Pre-migration `SkillTrigger`, `ZoneSkillExecutor`, and `SingleSkillExecutor` assigned transient additional-damage visuals a `1f` lifetime.
- The relevant monster references define these ten payloads as instantaneous explosions, reflections, follow-ups, or slashes and do not define a persistent visual duration.
- Ten Trigger owners now each contain exactly one `SetDuration=1` Node.
- All sixteen Trigger owners with `ShowVisual` now have a positive duration or status-owned lifetime; standalone non-positive duration count is zero.
- Unity CSV source validation passed and loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity Console reported zero errors; `dotnet build Pakuri/Pakuri.sln --no-restore` completed with zero errors.

### History

- 2026-07-29: User rejected a runtime zero-duration fallback and directed data-only restoration from the monster references and prior behavior.
- 2026-07-29: Code Builder restored `SetDuration=1` for ten affected Trigger owners.

## Task: 2026-07-29 Eve-E Recast Generation Regression Diagnosis

### Task title

Diagnose the Eve E zone that continues displaying and dealing damage after its intended lifetime.

### Goals

- Distinguish the actual `eve-e-master-1` OnExpire Trigger from the non-Trigger `eve-e-master-2` Choice modifiers.
- Identify why an Eve E recast zone can repeat without reaching its authored generation limit.

### Constraints

- Preserve all authored duration, generation, Trigger, Choice, damage, status, visual, prefab, scene, and generated-catalog values.
- Keep non-lifecycle Trigger events at their existing zero recast generation.
- Every conclusion must follow from current authoring and runtime code.
- Unity Play Mode reproduction remains user-owned.

### Role Owner

Code Builder

### Status

Runtime correction and local non-Play-Mode verification complete. User Play Mode verification remains.

### Next Actions

- User verifies that `eve-e-master-1` creates only one three-second recast and that `eve-e-master-2` creates no recast.

### Evidence

- `eve-e-master-2` owns only `StatusMaxStacksBonus` and `StatusCriticalDamageTakenBonus` Choice Nodes; no Trigger or visual Node with that ID exists.
- `eve-e-master-1` owns the `OnExpire` Trigger and `RecastZone` Node with `max_generation=1`.
- `ZoneSkillActor` publishes OnExpire with its current `recastGeneration`.
- Before the correction, `SkillTrigger.PublishLifecycleEvent` converted the lifecycle context to `TriggerExecutionContext`, which had no recast-generation field.
- Before the correction, `SkillTrigger.TryExecuteOwnedNodes` constructed a new `SkillExecutionContext` without a recast-generation argument, resetting it to zero.
- Before the correction, `ZoneSkillExecutor.ExecuteRecast` therefore saw zero on every expiration and passed the generation guard repeatedly.
- The pre-legacy-deletion path passed `context.RecastGeneration` directly into the next recast actor.
- `TriggerExecutionContext` now stores a non-negative `RecastGeneration`; lifecycle publication copies it from `SkillExecutionContext`, and Trigger-owned Node execution copies it into the next `SkillExecutionContext`.
- Repository-wide authoring inspection found one `RecastZone`: `eve-e-master-1`, with one matching `OnExpire` Trigger, duration 3, and `max_generation=1`; static validation reported zero errors.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with zero errors and the two pre-existing assembly-version warnings.
- Unity script validation, forced compilation, domain reload, and Console inspection completed with zero diagnostics.

### History

- 2026-07-29: User reported that the Eve E master effect remained indefinitely and continued applying damage.
- 2026-07-29: Designer found an ID mismatch in current authoring and a recast-generation loss introduced in the Trigger-to-Node execution path.
- 2026-07-29: User confirmed `eve-e-master-1` and authorized Code Builder correction plus cross-skill verification.
- 2026-07-29: Code Builder restored recast-generation propagation in the common Trigger execution path and verified every authored `RecastZone`.

## Task: 2026-07-29 Skill Runtime Responsibility Comments

### Task title

Document method-level behavior and core responsibility boundaries in the shared skill runtime.

### Goals

- Add Korean method-level comments to previously undocumented constructors, snapshot helpers, conditional rule resolvers, and runtime Node execution helpers.
- Clarify the top-level responsibility of the Definition, execution routing, execution snapshot, targeting, and Node dispatch files.
- Correct the stale `SkillExecutionRuleResolver` header so it describes its actual conditional runtime-rule responsibility.

### Constraints

- Limit changes to comments in the eight user-specified C# files.
- Preserve every type, method signature, field, operation, condition, execution order, and player-facing behavior.
- Do not change CSV, catalog, prefab, scene, asset, or runtime data contracts.

### Role Owner

Code Builder

### Status

Comment implementation and non-Play-Mode verification complete.

### Next Actions

- User may review the new responsibility comments while implementing future Base, Enhancement, Master, Passive, and Trigger skills.

### Evidence

- Method-comment coverage scan reported zero undocumented method or constructor declarations in all eight target files.
- Removing comments and whitespace from each changed file produced code text identical to its `HEAD` version (`ALL_CODE_EQUAL=True`).
- `git diff --check` completed without whitespace errors.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with zero errors and the two existing assembly-version conflict warnings.
- Unity script refresh and domain reload completed; editor state returned idle/ready and Console contained zero errors.

### History

- 2026-07-29: User requested method-level and core-responsibility comments across the shared skill runtime.
- 2026-07-29: Code Builder added comment-only documentation to the eight specified files and verified that executable code remained unchanged.

## Task: 2026-07-29 Skill Compilation Placement Refactor

### Task title

Move post-catalog skill compilation out of the Loading pipeline.

### Goals

- Move `SkillDefinitionCompiler` to `Combat/Skills/Compilation`.
- Split `SkillNodeMapper` and `SkillChoiceCompiler` into their own files.
- Preserve every compilation and runtime Node behavior.

### Constraints

- File organization and ownership only; no skill, Trigger, Node, status, damage, timing, or gameplay behavior changes.
- Preserve `Pakuri.InGame` namespaces and all public method signatures.
- Preserve the existing compiler script `.meta` GUID.
- Do not modify the user-owned comment changes already present in the eight combat runtime files.

### Role Owner

Code Builder

### Status

Implementation and non-Play-Mode verification complete.

### Next Actions

- User verifies representative compiled skills in Unity Play Mode.

### Evidence

- `SkillDefinitionCompiler` is not called by `GameDataLoader.LoadAndValidateRuntimeCatalog`.
- Inspected compiler consumers are combat `SkillExecution`, spawn state construction, and UI/run learned-skill application.
- The current source file contains three separate classes: `SkillDefinitionCompiler`, `SkillNodeMapper`, and `SkillChoiceCompiler`.
- `SkillDefinitionCompiler`, `SkillNodeMapper`, and `SkillChoiceCompiler` now reside in separate files under `Combat/Skills/Compilation`.
- The compiler script retains its original Unity GUID and the extracted files have generated `.meta` files.
- Existing namespaces and method signatures compile through `Assembly-CSharp.csproj` and Unity with zero errors.

### History

- 2026-07-29: User approved the four-stage Loading structure and Code Builder implementation.
- 2026-07-29: Code Builder recorded this separate combat-boundary task before moving compilation code.
- 2026-07-29: Code Builder moved and split the compilation classes without modifying the eight pre-existing user-owned combat runtime files.

## Task: 2026-07-29 Final Skill Catalog Direct-Use Design

### Task title

Generate final typed skill data once and use it directly from `GameDataCatalog`.

### Goals

- Remove runtime Source-to-Definition and Node compilation.
- Remove authored Node and Trigger string parsing from runtime execution.
- Remove the three `Combat/Skills/Compilation` scripts.
- Reorganize all current `Combat/Skills` scripts into Definition, Runtime, Execution, Delivery, and Reaction responsibilities.

### Constraints

- Preserve every current combat behavior, CSV value, ID, order, and asset reference.
- Keep one semantic validation call and one final catalog build.
- Keep per-cast `SkillExecutionData` and per-unit `SkillUseState`.
- Preserve moved script `.meta` GUIDs.
- Designer changes documentation only.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Code Builder implementation and available non-Play-Mode verification complete. Phases 1-6 complete.

### Next Actions

- User verifies representative active, passive, enhancement, master, Trigger, and enemy skill behavior in Unity Play Mode.

### Evidence

- Current `Combat/Skills` contains 27 C# scripts and 15,387 lines.
- `SkillExecution.RebuildLearnedSkillState` calls `CompileActive` and `CompilePassive`.
- `SkillNodeMapper.GetChoiceRuntimeNodes` performs first-use Choice Node mapping and caching.
- `SkillNodeExecutor` reparses authored scope, merge policy, condition, status-list, and runtime-kind strings during execution.
- `SkillTrigger` splits authored Choice, attribute, and event-skill lists during trigger checks and compares event source scope as a string.
- Current graph authoring has two Choice owners targeting more than one skill.
- Full current tree, all script responsibilities, final 24-script tree, data contracts, migration, risks, and verification are recorded in the handoff.
- Phase 1 started from clean commit `565eed5`; current `Combat/Skills` contains 27 C# files and 15,387 lines.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` baseline builds completed with zero errors.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies; the EditMode test job succeeded.
- Phase 2 final types now store typed status scope/policy, status conditions, status lists, RuntimeKind filters, Trigger lists/scope, Choice Nodes, and Node target skill IDs.
- `SkillNodeExecutor` contains zero authored `StatusRuntimeCompiler.Parse*` calls; the runtime/editor assembly builds with zero errors.
- Phase 3 Monster, Enemy, and RuntimeCatalog storage uses final Definition/Choice types; Combat state rebuild stores those same references without compiling.
- Generation builds status definitions first and creates status runtime payloads without re-entering `GameDataLoader.CurrentCatalog`.
- Unity CSV validation loaded 5/8/8 definitions after final catalog generation.
- Phase 4 removed `SkillChoice.Source`, Choice lazy Node mapping/cache, and runtime Trigger authored-string parsing.
- Runtime Execution/Trigger/StatusRules search found zero `Split`, `Enum.Parse`, or `TryParse` calls.
- Runtime and Editor builds completed with zero errors; Unity CSV validation retained 5 monsters and 8/8 enemies.
- Phase 5 removed all three compiler/mapper symbols and `CompileTriggers`; Generation owns the integrated Builder logic.
- `Combat/Skills` now contains the specified 24 scripts under Definitions, Runtime, Execution, Delivery, and Reactions.
- All 18 moved script GUID pairs matched; runtime/editor builds and Unity CSV validation passed.
- Phase 6 removed all Source/Definition duplicate contracts, `NormalizedNodes`, and raw final Node/Trigger/status authored-string fields.
- Removed-symbol, runtime parsing, and Generation-outside Definition-mutation searches all returned zero.
- EditMode target-filter/reference-reuse tests passed 2/2; solution build and Unity script compilation completed with zero errors; CSV validation retained 5/8/8.
- `Combat/Skills` changed from 27 scripts/15,387 lines to 24 scripts/12,102 lines: net reduction 3 scripts and 3,285 lines.

### History

- 2026-07-29: User chose final authored-data direct use and requested a Code Builder-ready markdown plus the complete `Combat/Skills` structure.
- 2026-07-29: Designer created the implementation handoff from inspected current code and data.
- 2026-07-29: Designer extended the handoff so Generation produces final typed Node and Trigger conditions and runtime code only compares or executes them.
- 2026-07-29: Code Builder completed Phase 1 baseline protection and recorded the live code, GUID, build, Unity, and CSV evidence.
- 2026-07-29: Code Builder completed Phase 2 final typed contracts while retaining the old compiler path as a buildable bridge.
- 2026-07-29: Code Builder completed Phase 3 final catalog generation and direct final-type indexing.
- 2026-07-29: Code Builder completed Phase 4 final Choice/Trigger/Status direct consumption and removed 239 net C# lines.
- 2026-07-29: Code Builder completed Phase 5 compiler deletion and responsibility-folder migration.
- 2026-07-29: Code Builder completed Phase 6 dead-contract deletion and full non-Play-Mode regression verification.

## Task: 2026-07-29 Trigger Executor Reuse Design

### Task title

Keep Trigger as the activation gate and route skill outcomes through existing family Executors.

### Goals

- Reduce `SkillTrigger` to event, condition, cooldown/count, delay/repeat, and delegation.
- Route Trigger delivery through `SkillExecution.TryExecuteTriggered` and existing family Executors.
- Delete `SkillNodeExecutor.cs` without adding a replacement script.
- Keep non-skill cooldown, reload, and status-duration commands on existing runtime APIs.

### Constraints

- Preserve current Trigger IDs, conditions, ordering, event snapshots, dynamic values, recursion limits, and player-facing behavior.
- Do not duplicate Trigger condition checks in `SkillExecution` or family Executors.
- Designer changes documentation only.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Phase 3 shared Trigger execution complete. Phase 4 typed command migration pending.

### Next Actions

- Route the 22 typed commands through existing cooldown, reload, status-duration, and Zone recast APIs.
- Remove Trigger runtime Node payload consumption.

### Evidence

- `SkillNodeExecutor.Execute` and `HasRuntimeActions` are called only by `SkillTrigger.cs`.
- Current Trigger authoring contains 158 owners and 606 Nodes: 4 existing-skill calls, 51 direct delivery results, 1 recast, 21 state commands, and 81 modifier-only owners.
- `SkillExecution.ExecuteSkill` already dispatches every concrete skill family to the existing Executor.
- `InGameCombatManager` publishes shield, status, kill, damage, and combat-start events outside the family Executor boundary.
- Full contracts, migration phases, edge cases, acceptance criteria, and verification are recorded in the handoff.
- Phase 1 confirmed 158 Triggers, 606 owned Nodes, 77 action owners, 81 no-action owners, 24 Combat/Skills scripts, and 12,102 lines.
- Runtime and Editor builds completed with zero errors before implementation.
- Phase 2 generated 55 final Definitions, 22 typed commands, and 81 inactive owners.
- Focused Unity EditMode catalog test passed 1/1; runtime/editor builds remained error 0.
- Phase 3 routes all 55 final Definitions through existing family Executors.
- `BuffSkillExecutor` now uses `StatusCombatRules.ApplyStatus`; lifecycle and source snapshot policies are explicit.
- Runtime/editor builds completed with error 0 and `SkillCatalogRuntimeTests` passed 3/3.

### History

- 2026-07-29: User selected condition-only Trigger orchestration, existing Executor reuse, and `SkillNodeExecutor` deletion.
- 2026-07-29: Designer replaced the obsolete direct Node-dispatch design with the executor-reuse handoff.
- 2026-07-29: User approved implementation; Code Builder completed the Phase 1 behavior and build baseline.
- 2026-07-29: Code Builder completed Phase 2 final Trigger outcome Generation and focused catalog verification.
- 2026-07-29: Code Builder completed Phase 3 shared family execution, status-rule parity, EventTarget filtering, and dynamic event-value snapshots.
