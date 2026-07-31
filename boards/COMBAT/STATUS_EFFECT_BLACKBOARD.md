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

Code Builder Phase 1-6 complete. Non-Play-Mode verification passed; user Play Mode confirmation remains.

### Next Actions

- User verifies representative Trigger families, commands, delay/repeat/recast, and dynamic damage in Play Mode.
- Run Code Reviewer only after separate explicit user approval.

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
- Phase 4 routes 1 recast, 14 cooldown refunds, 6 reload reductions, and 1 status-duration extension through existing APIs.
- `SkillTriggerDefinition.Nodes` and `SkillTrigger` legacy Node execution are removed; `SkillCatalogRuntimeTests` remains 3/3 and Unity Console reports zero errors/warnings.
- Phase 5 deletes `SkillNodeExecutor.cs/.meta`, Trigger-only public operation types, and their legacy mapping; active C# search returns zero deleted symbols.
- Runtime/editor builds remain error 0 and Unity `SkillCatalogRuntimeTests` passes 3/3 after deletion.
- Final static search returns zero deleted symbols, Trigger runtime Node payloads, and authored-string parsing in runtime consumers.
- Solution build error 0, Unity Console error/warning 0, full EditMode 3/3, CSV catalog 5/8/8.
- `Combat/Skills` is 23 C# scripts; Git diff is 579 additions / 1,547 deletions, net -968 lines.
- Whole production `Assets/Scripts` Git diff is 1,299 additions / 1,742 deletions, net -443 lines.

### History

- 2026-07-29: User selected condition-only Trigger orchestration, existing Executor reuse, and `SkillNodeExecutor` deletion.
- 2026-07-29: Designer replaced the obsolete direct Node-dispatch design with the executor-reuse handoff.
- 2026-07-29: User approved implementation; Code Builder completed the Phase 1 behavior and build baseline.
- 2026-07-29: Code Builder completed Phase 2 final Trigger outcome Generation and focused catalog verification.
- 2026-07-29: Code Builder completed Phase 3 shared family execution, status-rule parity, EventTarget filtering, and dynamic event-value snapshots.
- 2026-07-29: Code Builder completed Phase 4 typed command execution and removed Trigger runtime Node payload consumption.
- 2026-07-29: Code Builder completed Phase 5 legacy executor, operation, mapping, and stale context cleanup.
- 2026-07-29: Code Builder completed Phase 6 static/build/Unity/CSV verification and recorded final net deletion.

## Task: 2026-07-29 Passive Trigger State Consolidation

### Task title

Delete `PassiveSkill.cs` and keep its live Trigger gate state with `SkillTrigger`.

### Goals

- Move internal cooldown and N-count state to the Trigger condition owner.
- Delete `PassiveSkill.cs` and its Unity `.meta`.
- Remove the empty passive change-notification pipeline from `InGameCombatManager`.

### Constraints

- Preserve Trigger cooldown timing, count reset behavior, case-insensitive keys, and per-combat-manager state isolation.
- Do not add a replacement script, interface, factory, or gameplay behavior.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies `eve-g`, `rin-h`, `sein-g`, and `vega-i` cooldown/count behavior in Play Mode.

### Evidence

- `PassiveSkill.cs` contained two dictionaries, `Reset`, `ConsumeTriggerCooldown`, `ConsumeTriggerCount`, and six empty notification/flush methods.
- Only `SkillTrigger.cs` called the two live consume methods.
- `SkillTrigger.cs` now owns the same case-insensitive cooldown/count dictionaries in manager-keyed `TriggerGateState`.
- `InGameCombatManager.Awake` and `ResetCombatState` reset that manager's Trigger state.
- Active Combat C# search returns zero `PassiveSkill`, `PassiveEffects`, empty notification, or pending-flush references.
- `PassiveSkill.cs` and `PassiveSkill.cs.meta` are deleted; no replacement script was added.
- `git diff --check` passed.
- `Pakuri/Pakuri.sln` builds with zero errors and the two existing assembly-reference warnings.
- Unity Console reports zero errors/warnings after script refresh.
- Unity EditMode `SkillCatalogRuntimeTests` passes 4/4 and loads the catalog at 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- The implementation diff is 69 additions and 122 deletions: net reduction 53 lines.

### History

- 2026-07-29: User approved Code Builder consolidation and deletion of `PassiveSkill.cs`.
- 2026-07-29: Code Builder moved live Trigger gate state to `SkillTrigger`, removed empty manager callbacks, deleted the script/meta, and completed local verification.

## Task: 2026-07-29 Status Runtime Responsibility Refactor

### Task title

Remove runtime status compilation and separate status definition, execution, runtime, and skill-integration responsibilities.

### Goals

- Delete the `StatusRuntimeCompiler` runtime/compiler responsibility.
- Generate final `StatusRuntimeData` once in Loading Generation.
- Make Combat retrieve final status data from `GameDataCatalog`.
- Move `SkillStatus` to `Combat/Skills/Execution`.
- Organize `Combat/Status` into `Definitions`, `Execution`, and `Runtime`.

### Constraints

- Preserve current status application, stack, duration, modifier, display, and skill behavior.
- Preserve moved Unity script `.meta` GUIDs.
- Keep authored status string parsing inside Loading.
- Do not add a replacement compiler, factory, or interface.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative buff, debuff, shield, stack, duration, and status display behavior in Unity Play Mode.
- Run Code Reviewer only after separate explicit user approval.

### Evidence

- Baseline worktree was clean at commit `772711c`; `Pakuri/Pakuri.sln` built with zero errors and two existing assembly-reference warnings.
- `StatusRuntimeCompiler.cs` and the `StatusRuntimeCompiler` symbol are removed.
- `Combat/Status` now contains `Definitions/StatusEffectDefinition.cs`, `Execution/StatusRules.cs`, and `Runtime/StatusState.cs`.
- `SkillStatus.cs` now resides under `Combat/Skills/Execution`; all inspected callers remain Skill Delivery executors.
- All four moved script `.meta` GUID comparisons returned `match=True`.
- Combat status and SkillStatus searches returned zero authored `Enum.Parse`, `Enum.TryParse`, or `Split` calls.
- Removed-symbol searches returned zero `StatusRuntimeCompiler` and `StatusEffectLookup` references.
- `Pakuri/Pakuri.sln` builds with zero errors; Unity Console reports zero errors/warnings after the final refresh.
- `SkillCatalogRuntimeTests` passes 4/4, including final status-data generation and RuntimeCatalog reference reuse.
- Production C# diff is 465 additions / 526 deletions: net reduction 61 lines. Tests add 18 lines, making the total C# diff net reduction 43 lines.

### History

- 2026-07-29: User approved Code Builder implementation of the status compiler removal and responsibility split.
- 2026-07-29: Code Builder moved parsing to Loading, final data generation to Generation, and lookup to RuntimeCatalog.
- 2026-07-29: Code Builder separated Status folders, moved SkillStatus to Skills/Execution, and completed static/build/Unity/EditMode verification.

## Task: 2026-07-29 Field Unit Registry Ownership Consolidation

### Task title

Make `UnitSpawnManager` the sole owner of field-unit registration and removal.

### Goals

- Keep `CombatUnitRegistry` as a hidden helper owned by `UnitSpawnManager`.
- Route selected monsters, manifested monsters, enemies, and Nexus through one field-unit manager.
- Replace external Registry access with read/query APIs on `UnitSpawnManager`.
- Remove the separate selected-player GameObject and model cache.

### Constraints

- Preserve spawning, restoration, targeting, collision lookup, AI, skills, UI display, death, and Nexus behavior.
- Keep `CombatUnitRegistry.cs` and `CombatUnitEntry`.
- Do not change CSV, scene, prefab, catalog, saved-run, or gameplay data.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies selected-monster spawn, manifestation, enemy waves, targeting, death, Nexus contact, and next-day party restoration in Play Mode.
- Run Code Reviewer only after separate explicit user approval.

### Evidence

- `CombatUnitRegistry` is now an internal sealed helper instantiated only by `UnitSpawnManager`.
- Active C# search finds `CombatUnitRegistry` only in its definition file and the private `UnitSpawnManager.unitRegistry` field.
- All Registry `Register` and `Unregister` calls now exist only inside `UnitSpawnManager`.
- External combat, skill, AI, GameFlow, and UI consumers receive or query `UnitSpawnManager`.
- `spawnedPlayerUnit`, `SpawnedPlayerModel`, `InGameCombatManager.UnitRegistry`, and the former CombatManager register/despawn APIs have zero active C# references.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` build with zero errors and the two existing assembly-reference warnings.
- Unity script refresh returned idle, Console contained zero errors, and focused script validation reported zero errors.
- `git diff --check` passed.

### History

- 2026-07-29: User clarified that all current field monsters, manifested monsters, and enemies should be managed through one owner.
- 2026-07-29: User selected Code Builder and approved `UnitSpawnManager` ownership with `CombatUnitRegistry` retained as a hidden helper.
- 2026-07-29: Code Builder moved Registry ownership and mutation to `UnitSpawnManager`, migrated external consumers, removed duplicate selected-player state, and completed local verification.

## Task: 2026-07-29 UnitSkills Single Runtime Source

### Task title

Use `UnitSkills` as the sole learned-skill and Choice ownership source.

### Goals

- Keep active, passive, enhancement, and master ownership in `UnitSkills`.
- Remove session-to-runtime ID copying.
- Keep `SkillExecutionState` as the separate transient cooldown, cast, magazine, reload, and execution-list state.

### Constraints

- Preserve full learned-skill execution-state rebuilds after post-combat learning and on spawn/restoration.
- Preserve existing skill, passive, Choice, Trigger, targeting, and delivery behavior.
- Do not introduce incremental synchronization, a replacement runtime class, or a new production script.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies newly learned active/passive skills and Choice effects in the next Play Mode combat.

### Evidence

- `UnitSkills.AddChoice` now classifies and stores confirmed Choice IDs.
- `UnitCombatStateFactory` and player restoration assign the exact `RunMonsterState.Skills` instance instead of copying collections.
- Production `AddActiveSkill`, `AddPassiveSkill`, and `AddChoice` calls exist only in `RunSession`.
- Removed copy-symbol search returned zero active production references.
- Runtime and Editor builds completed with zero errors; all five `SkillCatalogRuntimeTests` passed.
- Unity script compilation returned ready and the post-compile Console contained zero errors or warnings.

### History

- 2026-07-29: User confirmed that learning occurs only during the post-combat reward stage and selected full execution-state rebuilds over incremental synchronization.
- 2026-07-29: Code Builder retained `SkillExecutionState` behavior and consolidated persistent learned-skill ownership into `UnitSkills`.

## Task: 2026-07-30 Unified Collider Collision Resolution

### Task title

Route all Collider-based skill and Nexus contact hits through one resolver.

### Goals

- Use one collision-result path for Projectile, Line, Zone prefab hitboxes, Single prefab hitboxes, Charge, and enemy-to-Nexus contact.
- Map every raw physics Collider through `UnitSpawnManager.FindByCollider()`.
- Use actual target Colliders only, with no Transform-position collision fallback.
- Give Line a real runtime `BoxCollider2D`.

### Constraints

- Preserve caller-owned targeting filters, target order, damage, status, follow-up, pierce, and tick behavior.
- Keep direct-target, chain, and radius targeting separate because they are not Collider collision delivery.
- Preserve the moved Unity script `.meta` GUID.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies Projectile pierce/impact, visual and no-visual Line, Zone/Single prefab hitboxes, Charge contact, and enemy-to-Nexus contact in Play Mode.
- Confirm that every collision-participating unit prefab has an enabled `Collider2D`; missing Colliders now intentionally produce no hit.

### Evidence

- `UnitHitboxOverlap.cs` was renamed to `UnitCollisionResolver.cs`; GUID `4280db46ec0042e69ea67a44c8b10498` is preserved.
- `UnitCollisionResolver.CollectTargets()` is the only production caller of `UnitSpawnManager.FindByCollider()`.
- Projectile, Line, Zone prefab hitbox, Single prefab/core hitbox, Charge, and Nexus contact all call `UnitCollisionResolver.CollectTargets()`.
- Moving Projectile and Charge use the same resolver with `Collider2D.Cast`; static shapes use `Collider2D.Overlap`.
- `LineSkillActor` creates and sizes a real runtime `BoxCollider2D`; no-visual Line also creates an empty Actor through `EffectManager`.
- `CombatUnitEntry.GetHitboxColliders()`, `ResolveTargetPoint()`, Line point fallback, Charge distance fallback, Projectile `OnTriggerEnter2D`, Line `Physics2D.OverlapBox`, Nexus `OverlapPoint`, and Nexus distance fallback are removed.
- Active production C# search returns zero `UnitHitboxOverlap`, `OnTriggerEnter2D`, `Physics2D.OverlapBox`, `OverlapPoint`, or `Collider2D.Distance(...).isOverlapped` collision paths.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` build with zero errors and the two existing assembly-reference warnings.
- `git diff --check` passes.
- Unity EditMode tests pass 6/6; `CollisionResolverUsesOverlapAndMovementCast` verifies both overlap and swept movement detection.
- Final Unity script refresh is idle and Console reports zero errors.

### History

- 2026-07-29: User required all collision-required skills to use one route and prohibited Transform-position exceptions without approval.
- 2026-07-30: Code Builder introduced `UnitCollisionResolver`, migrated all inspected Collider delivery paths, removed Transform fallbacks, and completed build, static, Unity, and EditMode verification.

## Task: 2026-07-30 Enemy Passive Shared Learning Runtime

### Task title

Move Enemy passives into the shared learned-skill runtime and remove Enemy-only combat branches.

### Goals

- Make Enemy spawn initialize assigned active and passive IDs through `UnitSkills`.
- Build Enemy `SkillState` through the same learned-skill reconstruction used by Monster.
- Resolve passive damage, defense, critical, healing, and incoming-damage modifiers through `SkillExecutionState`.
- Delete `EnemyPassiveModifiers` and all Enemy-only passive state and combat calculation branches.

### Constraints

- Preserve the six existing Enemy passive modifier behaviors and authored values.
- Preserve Monster reward learning, Choice, and Trigger behavior.
- Preserve the user’s existing uncommitted roster and collision refactors in overlapping files.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative DamageUp, DefenseUp, HealingUp, and IncomingDamageDown enemies in Play Mode.

### Evidence

- `UnitCombatStateFactory.CreateEnemy` records assigned active and passive IDs in the common `UnitSkills` storage.
- `SkillExecution.RebuildLearnedSkillState` now accepts resolved active and passive definitions for both Monster and Enemy.
- `SkillExecutionState` supplies all six passive modifier results to common damage and healing calculations.
- Passive Trigger dispatch reads each learned passive’s runtime definition instead of loading `MonsterDefinition`.
- `EnemyPassiveModifiers.cs`, its `.meta`, the empty `Enemy/Passive` folder, and all eight Enemy-only passive fields were removed.
- Static searches return zero removed Enemy passive types, fields, `RebuildAssignedSkillState`, Monster-only passive role gates, or Enemy branches in damage/healing.
- Runtime and Editor C# builds complete with zero errors and the two existing assembly-reference warnings.
- Unity catalog loading retains 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Full Unity EditMode tests pass 9/9, including shared Enemy spawn, all six modifier kinds, and all 16 catalog Enemy passives.
- Final Unity script refresh is idle and Console contains no project compile errors.

### History

- 2026-07-30: User rejected the separate Enemy passive runtime and required Enemy spawn to use the Monster learned-skill path.
- 2026-07-30: User explicitly required deletion of `EnemyPassiveModifiers`, Enemy-only multiplier fields, and Enemy branches in damage and healing.
- 2026-07-30: Code Builder completed the shared learned-passive migration and non-Play-Mode verification.

## Task: 2026-07-30 Skill Definition Family Consolidation

### Task title

Consolidate skill Definitions and execution logic by real delivery family.

### Goals

- Keep one concrete Single Definition and one concrete Buff Definition.
- Convert Chain to Trigger-based common Single execution.
- Move Charge initiation to the Buff family.
- Split Definitions into family folders after behavior consolidation.
- Delete duplicate type dispatch, executor branches, and dead fields.

### Constraints

- Follow `boards/COMBAT/SKILL_DEFINITION_FAMILY_CONSOLIDATION_HANDOFF.md`.
- Preserve current CSV values, IDs, assets, Trigger behavior, and gameplay behavior.
- Do not create rejected standalone contract scripts or compatibility-shell subclasses.
- Preserve existing XML-comment-tag cleanup.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Code implementation and non-Play-Mode verification complete.

### Next Actions

- User Play Mode verification for the affected combat skills.

### Evidence

- Current Generation and runtime use four removable concrete subclasses: Chain, Charge, Heal, and Shield.
- Current Chain and Charge have separate Single executor overloads.
- Current Buff Status, Heal, and Shield use three executor classes.
- Full approved behavior and file contracts are recorded in the handoff.
- `BuffSkillDefinition` now owns Status, Heal, Shield, and Charge through `BuffEffectKind`.
- `BuffSkillExecutor.Execute` is the sole Buff family entry and shares target/visual paths.
- OpeningCharge now uses the Buff active-state path plus ordinary `EnemyCombatDecision`/`EnemyActionController` targeting, movement, and `UnitCollisionResolver` contact; separate Charge Actor/State scripts are removed.
- Runtime and Editor builds completed with zero errors; Unity C# console errors were zero.
- Full Unity EditMode tests passed 11/11.

### History

- 2026-07-30: User approved implementation after the handoff MD is created.
- 2026-07-30: Code Builder created the handoff and started implementation.
- 2026-07-30: Code Builder completed Buff family consolidation and non-Play-Mode verification.

## Task: 2026-07-31 Skill Executor / Actor Responsibility Unification

### Task title

Unify skill execution as `SkillExecution -> Executor -> Actor -> EffectManager`.

### Goals

- Make spatial family Executors launch-only.
- Make spatial family Actors own targeting at the required timing, collision, hit judgment, effect application, Trigger publication, and completion.
- Make `SkillExecutionData` finalized before Executor dispatch.
- Remove duplicated hit enhancement implementations and `EffectManager`'s automatic family-Actor selection.

### Constraints

- Preserve current gameplay, Trigger, CSV, Definition, asset, and visual behavior.
- Buff remains the no-spatial-gameplay-Actor exception; `BuffSkillActor` owns Buff/status visual lifetime.
- Charge remains on `SkillUseState` plus ordinary enemy AI/movement/collision.
- Normal visual lifetime belongs to the family Actor; `EffectManager` only creates, tracks, deletes on Actor request, and force-clears combat effects.
- Spatial Actors and the no-gameplay-Actor Buff Executor reuse `SkillTargeting.cs`; no separate targeting algorithm is added.
- Add no base Actor, interface, factory, or standalone contract script.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer for the handoff. Code Builder after explicit user assignment.

### Status

Code implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative Projectile, Line, Single, Zone, Buff, Trigger, no-visual, and Charge behavior in Play Mode.

### Evidence

- Four spatial Executor files contain only matching Actor creation, attachment, initialization, and launch-result return; forbidden targeting, calculation, coroutine, gameplay-application, and Trigger symbols return zero matches.
- Single, Projectile, Line, and Zone family calculations, targeting, delayed/repeated work, collision, gameplay application, and completion reside in their matching Actor files.
- Spatial Actors and the Buff Executor call the existing `SkillTargeting`; no second targeting implementation was added.
- `SkillExecutionRuleResolver.ApplyHitEnhancements` is the only shared hit-enhancement implementation.
- Projectile no-visual execution uses an empty Projectile Actor; its branch visual uses `ProjectileSkillActor`, not `LineSkillActor`.
- Zone recast calls normal `ZoneSkillExecutor.Execute`; `ZoneSkillExecutor.ExecuteRecast` is removed.
- `EffectCreateRequest.DurationSeconds` and `EffectManager` automatic Single/Buff Actor attachment are removed.
- Persistent status end paths call `EffectManager.SignalStatusEffectEnded`; `BuffSkillActor` then requests removal.
- `BuffSkillExecutor.cs.meta` retains GUID `210c9a9da090fa545801a1d1fb30c1ed`.
- Removed-symbol searches find no consolidated subclass, Charge Actor/State, XML `<summary>/<c>` tag, Projectile direct-hit fallback, or Zone recast executor.
- `git diff --check` passes.
- Runtime and Editor builds complete with zero errors and the two existing assembly-reference warnings.
- Unity script compilation completes with zero Console errors.
- Full Unity EditMode tests pass 11/11.
- Full boundaries and final responsibilities are recorded in `boards/COMBAT/SKILL_EXECUTOR_ACTOR_RESPONSIBILITY_HANDOFF.md`.

### History

- 2026-07-31: User required no validation or Snapshot interpretation in Executors and gameplay judgment/application in Actors.
- 2026-07-31: User required Single to use the same high-level execute-then-judge flow as other spatial families.
- 2026-07-31: Designer created the implementation handoff from inspected current code.
- 2026-07-31: User corrected visual lifetime ownership; Designer revised the handoff so family Actors own completion and request `EffectManager` deletion.
- 2026-07-31: User confirmed reuse of `SkillTargeting.cs` and explicitly assigned Code Builder implementation.
- 2026-07-31: Code Builder completed Executor/Actor responsibility migration, EffectManager lifetime correction, Buff Executor rename, static/build/Unity/EditMode verification, and preserved Play Mode verification for the user.

## Task: 2026-07-31 Skill Cast Finalization / Actor Application

### Task title

Finalize cast-fixed skill values before delivery and reduce Actors to runtime application.

### Goals

- Finalize family values and deployment plans in `SkillExecution`/`SkillExecutionData`.
- Remove concrete Definition interpretation and cast planning from Actors.
- Consolidate targeting, status, hit application, and visual setup into existing shared paths.
- Delete cross-family Actor dependencies and direct Trigger-to-Executor bypasses.

### Constraints

- Preserve current gameplay, CSV, Definition, Trigger, visual, prefab, asset, and timing behavior.
- Keep hit-time target, collision, health/status, resistance, chance, death, recovery, consumption, and redistribution decisions in Actor/shared hit rules.
- Add no production script, family snapshot class, base Actor, interface, or factory.
- Reuse and move existing logic; do not copy algorithms.
- Actor owns normal lifetime and requests `EffectManager` deletion.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder for implementation. Code Reviewer for one final review pass.

### Status

Code Reviewer ran once and returned concrete fixes. Code Builder applied them in `a9088f0`; static and Runtime/Editor build verification pass. Final EditMode rerun is pending because Unity is in user-owned Play Mode.

### Next Actions

- User exits Play Mode when ready, then rerun the full EditMode suite.
- User verifies representative Projectile, Line, Single, Zone, Buff, Trigger, no-visual, and Charge behavior in Play Mode.

### Evidence

- Baseline Runtime and Editor builds complete with zero errors and two existing assembly-reference warnings.
- Baseline checkpoint commit is `c9303c4`.
- Actor calculation, Trigger bypass, shared status type placement, and cross-family Actor dependencies are recorded in the handoff.
- Family and consolidation commits are `8698375`, `4db7aef`, `fac4762`, `f6a9e33`, `95dbf2a`, `eae8a74`, `a59a415`, and `befb722`.
- Spatial Actor/Executor forbidden-symbol searches return zero concrete Definition, cast-calculation, and cross-family Actor matches.
- Runtime and Editor builds complete with zero errors and the two existing assembly-reference warnings.
- Unity focused Charge test passes 1/1 and full EditMode tests pass 11/11.
- Code Reviewer ran once and found Executor/Actor placement, triggered Definition identity, and Projectile branch/visual responsibility gaps.
- Fix commit `a9088f0` moves prepared placement/scheduling into Executors, preserves hit-time Actor application, prepares branch values and execution identity, and moves branch-line presentation into `EffectVisualBuilder`.
- Post-fix static searches and Runtime/Editor builds pass; Unity refresh passes and Console error query is empty.
- Post-fix EditMode job `167d7ad311004124906e358775f87d61` ran zero tests because Unity reported Play Mode/transition; Code Builder did not stop user-owned Play Mode.
- Full implementation evidence is recorded in `boards/COMBAT/SKILL_CAST_FINALIZATION_ACTOR_APPLICATION_HANDOFF.md`.

### History

- 2026-07-31: User approved the finalized responsibility boundary and explicitly assigned Builder implementation, intermediate commits, and final Reviewer inspection.
- 2026-07-31: Code Builder created the implementation handoff and preserved the prior completed work in checkpoint `c9303c4`.
- 2026-07-31: Code Builder completed family finalization, common-path consolidation, static/build/Unity verification, and intermediate commits.
- 2026-07-31: Code Reviewer ran once and returned concrete fixes; Code Builder applied them in `a9088f0`.
- 2026-07-31: Post-fix builds and Unity compilation passed; the full EditMode rerun remains pending because Unity was in user-owned Play Mode.

## Task: 2026-07-31 Lowest Health Ratio Targeting

### Task title

Select the ally with the lowest current-health/max-health ratio.

### Goals

- Make `LowestHealth` compare health ratios instead of absolute current health.

### Constraints

- Change only the existing shared comparison expressions.
- Preserve `HighestHealth` behavior and all targeting contracts.

### Role Owner

Code Builder.

### Status

Complete.

### Next Actions

- User verifies the Stage1 priest heals the ally with the lowest health ratio in Play Mode.

### Evidence

- `SkillTargeting.FindNearestTarget` and `SkillTargeting.CompareTargets` now compare `CurrentHealth / MaxHealth` for `LowestHealth`.
- `git diff --check` passes.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` build with zero errors and the two existing assembly-reference warnings.

### History

- 2026-07-31: User requested a minimal formula-only Code Builder fix.
- 2026-07-31: Code Builder changed both shared `LowestHealth` comparison paths without adding a class or targeting mode.

## Task: 2026-07-31 Skill Trigger Reaction Logic Consolidation Design

### Task title

Remove the separate Trigger skill Definition and consolidate reactions through existing skill snapshots and Executors.

### Goals

- Keep `SkillTrigger.cs` as the single event and condition judge.
- Replace hidden TriggeredSkill Definitions with original or explicitly targeted skill snapshots.
- Record common-path conversions for cross-skill passives, event-derived damage, direct delivery, Actor-less passives, Zone recast, ChainLightning, and recursion.

### Constraints

- Add no C# script, Trigger Executor, Trigger Actor, event bus, or replacement Definition hierarchy.
- Do not copy `SkillTriggerDefinition` into another script.
- Preserve current working outcomes while restoring the approved 17 event effects and 64 normal cast effects.
- Keep event conditions in `SkillTrigger` and effect ownership in existing Skill/Choice/Passive execution.

### Role Owner

Code Builder.

### Status

User approved effect restoration and Code Builder implementation. Phases 1-8 complete. Code Reviewer corrections 1-2 are implemented and await re-review.

### Next Actions

- Commit Code Reviewer correction 2 separately.
- Re-run Code Reviewer and correct findings until approval.

### Evidence

- Current design: `boards/COMBAT/SKILL_TRIGGER_REACTION_LOGIC_CONSOLIDATION_HANDOFF.md`.
- Inspected CSV join found 158 Trigger rows, 606 Trigger-owned Nodes, 77 runtime outcomes, and 81 no-runtime-outcome owners.
- The 77 outcomes are 27 damage, 21 status, 3 shield, 4 existing-skill execution, and 22 typed commands.
- The 27 damage outcomes split into 7 event-value reactions and 20 independently targeted/ranged/visual reactions.
- Learned passive source skills own 40 runtime outcomes.
- Existing code retains depth 8 in `SkillExecution`, generation 1 for `eve-e-master-1`, and ChainLightning delay 0.5, multiplier 0.5, radius 7, and primary exclusion.
- Semantic re-audit split the 158 rows into 65 working Trigger reactions, 17 Trigger-intent rows with no runtime outcome, and 76 non-Trigger rows.
- The 76 non-Trigger rows are 75 OnCast rows and one same-source OnSkillCast follow-up.
- Ariel-B trait 1~3 are direct Choice modifiers, trait 4 is an OnShieldExpire damage Trigger, trait 5 is an OnCast status modifier with no current runtime outcome, and master 2 is an OnShieldAbsorb damage Trigger.
- `SkillCatalogRuntimeTests.TriggerSemanticClassificationBaselineIsStable` fixes the semantic `65/17/76` owner classification.
- Phase 1 solution build completed with error 0; Unity focused EditMode test passed 1/1 and loaded catalog 5/8/8.
- Phase 2 routes preparation and Executors by existing `SkillRuntimeKind`; solution build error 0 and Unity EditMode 13/13.
- Current `AreaAttack` data has both `SingleSkillDefinition` (`Slash`, `FireDragonSlash`) and Zone-family results, so Phase 2 validates and preserves that existing family split.
- Phase 3 replaces the common `TryExecuteTriggered` dependency on the full Trigger Definition with `TryExecuteReaction`, passing existing runtime, Definition, snapshot runtime, and primitive execution adjustments.
- Phase 3 solution build completed with error 0; Unity forced script compile and full EditMode tests passed 14/14.
- Phase 4 final runtime contains 82 semantic Trigger rows and zero OnCast/same-source follow-up rows.
- Phase 4 attaches 74 normal cast/passive payloads to existing Skill/Choice/Passive Nodes; `ariel-e-trait-4` uses the existing conditional-damage Choice operation and duplicate `eve-h-trait-3` is excluded.
- Phase 4 restores 64 `StatusModifier` payloads as normal `PassiveBuff` effects with authored target, status, source-skill, health-ratio, duration, and mutation conditions.
- Phase 4 solution build completed with error 0; Unity forced script compile and full EditMode tests passed 14/14.
- Phase 5 replaces 40 hidden direct-delivery Definitions with 57 common event effects: damage 24 and status 33, including the restored 17 incomplete status outcomes.
- Phase 5 leaves only 4 learned cross-skill references and 21 state commands; all 82 runtime reactions now have an outcome.
- Phase 5 solution build completed with error 0; Unity full EditMode tests passed 14/14.
- Phase 6 final catalog contains 48 passive source reactions: common effect 24, learned cross-skill reuse 4, and existing state command 20.
- The earlier design count 37 described a pre-refactor hidden-Definition subset, not final passive source ownership; the code-derived final count is 48.
- Phase 6 state commands remain on existing APIs: cooldown refund 14 and reload reduction 6; the remaining command is Zone recast.
- Phase 6 solution build completed with error 0; Unity full EditMode tests passed 15/15.
- Phase 7 ChainLightning uses the original skill Damage and runtime snapshot without a `__chain` Definition; targeting remains primary-excluding, radius 7, delay 0.5, multiplier 0.5.
- Phase 7 Zone recast uses the original snapshot with delay 0.5, radius multiplier 0.6, duration 3, and generation cap 1.
- Phase 7 solution build completed with error 0; Unity full EditMode tests passed 15/15.
- Phase 8 deletes `SkillTriggerDefinition.cs/.meta`, `SkillDefinition.SkillTriggers`, and Monster/Enemy Trigger arrays.
- Existing Skill/Choice/Passive Nodes now own `SkillReactionOp`; `SkillExecutionData` exposes the active reaction snapshot to `SkillTrigger`.
- C# search finds zero `SkillTriggerDefinition` references; solution build error 0, Unity Console error 0, and full EditMode tests passed 15/15.
- Reviewer correction 1 removes the duplicate `ariel-a-master-2` cast payload, restores Vega B's delayed 0.45 same-skill line follow-up with silence and prepared aim, preserves Ariel C's prepared center, and reapplies passive effects after runtime Choice rebuild.
- Final normal cast/passive payload count is 73; solution build error 0, Unity Console error 0, and full EditMode tests passed 15/15.
- Reviewer correction 2 prevents Vega B's delayed self-reuse from scheduling the same normal follow-up again; other common reaction executions retain normal cast effects.

### History

- 2026-07-31: User required an MD that integrates the existing blockers without adding scripts or relocating the old class contents.
- 2026-07-31: Designer wrote the common snapshot and existing-Executor consolidation handoff and preserved the earlier implemented handoff as baseline evidence.
- 2026-07-31: User clarified that ordinary modifiers and cast-time payloads are not Trigger reactions.
- 2026-07-31: Designer re-audited all 158 rows and corrected the handoff to separate working Triggers, incomplete Trigger intent, and non-Trigger authoring.
- 2026-07-31: User approved restoring the 17 event-driven effects and 64 normal cast effects, assigned Code Builder, and required per-phase commits plus Reviewer approval.
- 2026-07-31: Code Builder completed the Phase 1 classification baseline.
- 2026-07-31: Code Builder completed Phase 2 runtime-kind executor routing.
- 2026-07-31: Code Builder completed Phase 3 existing-skill runtime reuse.
- 2026-07-31: Code Builder completed Phase 4 non-Trigger extraction and normal effect ownership restoration.
- 2026-07-31: Code Builder completed Phase 5 direct-delivery consolidation and incomplete event-effect restoration.
- 2026-07-31: Code Builder completed Phase 6 Actor-less passive and state-command verification without adding runtime branches.
- 2026-07-31: Code Builder completed Phase 7 Zone and Chain snapshot reuse.
- 2026-07-31: Code Builder completed Phase 8 obsolete contract deletion and existing-node reaction ownership.
- 2026-07-31: Code Reviewer requested four behavior corrections; Code Builder implemented them through existing common paths.
- 2026-07-31: Code Reviewer found the Vega B asynchronous self-follow-up recursion; Code Builder disabled nested cast effects for that one reuse.
