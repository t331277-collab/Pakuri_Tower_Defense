## Archived History

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- Completed or older Reviewer task blocks were archived to `boards/ARCHIVE/OPS_REVIEWER_ARCHIVE_2026-05-19.md` on 2026-05-19.

## Task: 2026-06-19 Enemy Skill Node Runtime 1-7 Review

### Task title

Review Code Builder enemy skill node runtime implementation steps 1-7 and step 8 retirement readiness.

### Goals

- Verify the node CSV/runtime implementation against the enemy skill handoff steps 1-7.
- Check correctness, build evidence, CSV coverage, and runtime fallback behavior.
- Decide whether old direct execution can be retired safely.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not implement code or CSV fixes.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Reviewer

### Status

Fix required before step 8. Steps 1-7 compile and load, but one damage-coefficient bug and one validation gap block safe retirement.

### Next Actions

- Code Builder fixes spell-only enemy damage coefficient fallback.
- Code Builder adds validation for supported enemy node `action_op` and `target_selector` values before removing direct fallback.
- Do not delete `EnemySkillExecutor` helper methods while node runtime still calls them.

### Evidence

- `EnemySkillData.csv` has `ChainLightning` with attack coefficient `0` and spell coefficient `1.2`.
- `PakuriCsvRuntimeData.EnemyDataset.cs` sets compatibility `coefficient` to spell coefficient when attack coefficient is zero.
- `EnemyCombatSystem.cs` `ResolveAttackDamage(...)` currently uses compatibility `Coefficient` as an attack coefficient fallback and then also adds `SpellPowerCoefficient`, so spell-only offensive skills can add attack damage incorrectly.
- `ValidateEnemySkillNodes(...)` checks unknown skill ids, empty `action_op`, and orphan params, but does not validate `action_op` or `target_selector` against supported runtime values.
- Current assigned Stage1/Stage2 basic and active skill ids all have node rows; reviewer command returned `missing=`.
- Current node CSV rows use supported action ops/selectors; reviewer command returned `unknownActionOps=0` and `unknownSelectors=0`.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing `MSB3277` warnings.
- Unity-MCP sync/load logs showed runtime catalog sync and load with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies; warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested Code Reviewer verification of the just-finished Code Builder steps 1-7 and whether step 8 direct path retirement is safe.

## Task: 2026-06-19 Ariel Plan-Action Migration Review

### Task title

Review Code Builder's Ariel-first migration to plan action payloads and Ariel-only compatibility routing.

### Goals

- Verify the prior old wide-field findings are fixed.
- Verify Ariel choices now bypass `ApplyNormalizedChoiceNodes(...)` into `SkillChoiceEffectSpec`.
- Verify new plan action payloads exist and are executed for Ariel modifiers.
- Verify validation/build/Unity-MCP checks pass.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not edit files.
- Review is scoped to Ariel-first target migration; full single-plan ownership of trigger/effect rows is treated as a future architecture step unless explicitly required now.

### Role Owner

Code Reviewer

### Status

Passed with caveat. No blocking findings for the Ariel-first minimum target. Caveat: trigger/effect application rows are still explicit CSV runtime objects, not `SkillExecutionPlan.Actions`.

### Next Actions

- User verifies Ariel runtime behavior in Play Mode.
- If the next target is full single-plan ownership, migrate `monster_skill_triger.csv` / `monster_skill_effects.csv` event actions into plan action node handlers.

### Evidence

- `SkillExecutionPlan.cs` exposes `SkillExecutionPlanNode.Action`, `FromAction(...)`, and `SkillExecutionPlan.Actions`.
- `SkillExecutionSnapshot.cs` line matches show `IsArielChoice(choice)` routes to `ApplyArielChoiceDefinition(choice)`, which calls `ApplyPlanActionNodes(nodes)` and returns before old choice-spec folding.
- `InGameSkillDefinitionMapper.cs` line matches show `if (choice != null && !IsArielChoice(choice)) ApplyNormalizedChoiceNodes(...)`, plus `MapSkillActionOp(...)` mappings for `HitTargetCountBonus` and `StatusCriticalDamageTakenBonus`.
- `SkillExecutionSystem.cs` line matches show Ariel dynamic `CountStatusDamageMultiplier` is applied from mapped plan action nodes.
- CSV scan returned `arielWideNonDefault=0`.
- Spot checks found `ariel-d-trait-4-hit-target-count-bonus` / `bonus=1`, `ariel-d-master-1-status-critical-damage-taken` / `bonus=0.25`, and `ariel-a-master-2-holy-exposure-element-damage-taken` / `bonus=0.15`.
- Ariel A master2 trigger/effect CSV rows show `OnOutgoingDamage`, `EventTarget`, `trigger_action=Effect`, and `status_effect_id=holy-exposure`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP sync/validate menus completed; console showed runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`, with 0 warning/error entries.

### History

- 2026-06-19: User requested Code Builder to execute steps 1-6 toward the target structure, then asked Code Reviewer to verify whether the work was complete.

## Task: 2026-06-19 Ariel A-J Node Conversion Re-Review

### Task title

Re-review whether Ariel A-J are fully converted to skill-body plus atomic effect/modifier/binding nodes without old structure reliance.

### Goals

- Verify Ariel A-J against `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md`.
- Check that original values remain CSV-authored while runtime implementation is not wide-column driven.
- Identify any active old/precombined rows or direct choice-column paths that still affect runtime.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not implement code/CSV fixes during review.
- Findings must be based on inspected files and command/Unity-MCP output.
- Unity Play Mode gameplay parity remains user-owned.

### Role Owner

Code Reviewer

### Status

Fix required findings were addressed by Code Builder. Code Reviewer rerun is still pending explicit user request.

### Next Actions

- Run a fresh Code Reviewer pass only if the user explicitly requests it.
- User verifies Ariel A master2 holy exposure and Ariel E shield amount variants in Play Mode.

### Evidence

- `monster_skill_choices.csv` line 9 keeps `ariel-a-master-2` active as `runtime_support_state=ReferenceDirect` with `status_tag=holy-exposure`, `status_chance_bonus=1`, `status_stacks_set=1`, and `status_element_damage_taken_bonus=0.15`; no `monster_skill_nodes.csv` row targets `ariel-a-master-2`.
- `SkillExecutionSnapshot.cs` lines 787-788 copy `choice.HasStatusElementDamageTakenBonus` and `choice.StatusElementDamageTakenBonus` from choice data into the runtime snapshot; `SkillStatusSpecUtility.cs` lines 150 and 188-190 apply that snapshot value to status data.
- `monster_skill_effects.csv` lines 18-20 keep `ariel-e-shield-trait2`, `ariel-e-shield-master2`, and `ariel-e-shield-trait2-master2` with `runtime_support_state=MigratedToEffectBinding` but non-empty `requires_active_choice_id` values.
- `SkillMultiEffectExecutor.cs` lines 183-188 only suppress disabled effects when `requires_active_choice_id` is blank; the executor does not check `runtime_support_state`, so the migrated Ariel E shield variants can still run when their choices are active.
- `SkillMultiEffectExecutor.cs` lines 776-779 applies both effect `DamageMultiplier` and snapshot `ShieldAmountMultiplier`, so the old precombined Ariel E shield variants can stack with the new node multiplier path.
- Current counts: `monster_skills.csv` has 10 Ariel skill rows, `monster_skill_choices.csv` has 50 Ariel choice rows, `monster_skill_effects.csv` has 35 Ariel effect rows, `monster_skill_triger.csv` has 5 Ariel trigger rows, `monster_skill_nodes.csv` has 37 Ariel node rows, and `monster_skill_node_params.csv` has 41 Ariel node parameter rows.
- Unity-MCP menu runs succeeded for CSV runtime sync, CSV source validation, and InGame skill data validation; console showed `InGame skill data validation passed with 0 warning(s)`, but this validator did not catch the runtime execution issues above.
- Code Builder follow-up converted `ariel-a-master-2` to trigger/effect/node composition: `monster_skill_triger.csv` has `ariel-a-master2-holy-exposure-on-hit`, `monster_skill_effects.csv` has `ariel-a-master-2-holy-exposure-on-hit`, and `monster_skill_nodes.csv` / `monster_skill_node_params.csv` have `ariel-a-master-2-holy-exposure-element-damage-taken` with `bonus=0.15`.
- Code Builder follow-up blanked old `ariel-a-master-2` choice-wide status payload fields in `monster_skill_choices.csv`.
- Code Builder follow-up cleared executable choice gates from the three migrated Ariel E shield variant rows and added validation that rejects `MigratedToEffectBinding` skill effects with executable choice/passive gates.
- Code Builder follow-up changed `SkillTriggerRuntime.ExecuteEffect(...)` to pass `triggerContext.EventTarget` into the effect execution context, enabling trigger-bound `target_selection=EventTarget` status effects.
- Follow-up checks returned no migrated effect rows with executable choice/passive gates and no Ariel old `status_tag` / `status_element_damage_taken_bonus` choice rows for `ariel-a-master-2`.
- Follow-up `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Follow-up Unity-MCP sync/validation logged runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`, with 0 warning/error console entries.

### History

- 2026-06-19: User requested Code Reviewer verification that Ariel A-J are fully node-converted according to the handoff report and no longer depend on old structure, with CSV as source values but skill body plus atomic nodes as implementation shape.
- 2026-06-19: User requested Code Builder to fix the Code Reviewer findings and confirmed the intended Ariel A master2 shape as hit trigger, event target, apply status, and `holy-exposure`.

## Task: 2026-06-19 Ariel Phase 2-5 Code Review

### Task title

Review the Ariel Phase 2-5 normalized node, trigger-binding, and passive ownership cleanup.

### Goals

- Check changed runtime code, CSV rows, and validation evidence for correctness regressions.
- Confirm the new shield amount node path and source-specific status condition are wired to real runtime code.
- Confirm no old Ariel E-owned J passive action-speed rows remain active.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not edit files.
- Review evidence must come from inspected files and command/Unity-MCP output.
- Unity Play Mode gameplay parity remains user-owned.

### Role Owner

Code Reviewer

### Status

Passed with no blocking findings.

### Next Actions

- User verifies runtime behavior in Play Mode for Ariel B, E, and J combinations.

### Evidence

- `SkillChoiceEffectSpec.cs`, `SkillExecutionSnapshot.cs`, `InGameSkillDefinitionMapper.cs`, `SkillExecutionUtility.cs`, and `SkillMultiEffectExecutor.cs` show `ShieldAmountMultiplier` is parsed into choice specs, applied to snapshots, and consumed by shield amount resolution.
- `SkillEffectDefinition.cs`, `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `StatusEffectRuntime.cs`, and `SkillMultiEffectExecutor.cs` show `condition_status_source_skill_id` is parsed, built, and checked through `MatchesConditionStatus(..., requiredSourceSkillId)`.
- `SkillTriggerRuntime.cs` resolves triggered effects from both active and passive effect arrays, so J-owned passive effects referenced by `triggered_effect_id` are discoverable.
- `InGamePassiveEffectRuntime.cs` still routes learned passive effects through `SkillMultiEffectExecutor.Execute(...)`; the moved J post-E effect rows are `enabled_by_default=false` with no effect-level choice gate, so they do not run from the passive refresh path and instead run through J-owned triggers.
- CSV acceptance check returned `eActiveShieldRows=1`, `eDisabledShieldVariants=3`, `shieldAmountNodes=4`, `oldEJRows=0`, `jTriggerRows=2`, and `jShieldSource=ariel-e-shield-base`.
- CSV field-count check returned no bad rows for `monster_skill_choices.csv`, `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- Runtime and editor `dotnet build` commands passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP console logs showed CSV runtime sync from `Assets/CSVdata/runtime`, runtime catalog load, and `InGame skill data validation passed with 0 warning(s)`; Unity-MCP warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested Code Builder to implement remaining Phase 2-5 work and then perform Code Reviewer work.

## Task: 2026-06-19 Ariel Target Runtime Structure Review

### Task title

Review whether Ariel's current skill and execution runtime are fully arranged into the target MonsterUnitActor / RuntimeModel / UnitSkillController / SkillExecutionPlan / handler / combat-service structure.

### Goals

- Check Ariel's current execution architecture against the target responsibility split.
- Identify target-structure gaps based on inspected runtime code and validation output.
- Avoid implementation changes during Code Reviewer work.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not implement fixes.
- Review conclusions must be backed by inspected code or command/Unity-MCP output.

### Role Owner

Code Reviewer

### Status

Fix required. Ariel is closer to the target structure, but the current runtime is not fully in the final target structure.

### Next Actions

- Move actor-owned revive/combat reset state changes out of `MonsterUnitActor`.
- Convert legacy effect/trigger execution into real plan action handler dispatch, instead of carrying raw `SkillEffectDefinition` / `SkillTriggerDefinition` payloads through the plan.
- Remove Ariel-only special routing once the generic plan action path owns the same behavior.

### Evidence

- `MonsterUnitActor.cs` exposes display/animation/debug methods, but `ReviveForNextDay(...)` still mutates `Model.AutoAttackEnabled`, `Model.AutoSkillEnabled`, `Model.Statuses`, `Model.Resources`, `Model.SkillRuntime`, and health resources.
- `BaseUnitRuntimeModel.cs` owns `Identity`, `Stats`, `Resources`, `SkillRuntime`, `Statuses`, `AutoAttackEnabled`, and `AutoSkillEnabled`; `MonsterUnitRuntimeModel.cs` inherits this runtime state model.
- `UnitSkillController.cs` ticks `skillRuntime`, routes auto/manual execution, and creates `SkillExecutionRequest`; `SkillExecutionRequest.cs` carries runtime entry, owner, roster, combat manager, delta time, manual target, aim point, log flag, and animation callback.
- `SkillExecutionPlan.cs` compiles normalized plan nodes and now carries `Actions`, `Effects`, and `Triggers`, but `Effects` / `Triggers` are still raw legacy definitions.
- `SkillPlanActionDispatcher.cs` resolves effects/triggers from plan payloads, but `SkillMultiEffectExecutor.cs` and `SkillTriggerRuntime.cs` still directly execute damage/status/zone/visual/cooldown/reload behavior.
- `SkillExecutionSnapshot.cs` and `InGameSkillDefinitionMapper.cs` still contain Ariel-only choice routing (`IsArielChoice(...)`) to avoid old normalized choice folding.
- `InGameCombatManager.cs` owns roster/skill system coordination and applies damage/status, matching the shared combat coordination target at a broad level.
- Ariel CSV scan returned `arielChoices=50`, `arielEffects=36`, `arielTriggers=6`, `arielNodes=40`, `arielParams=44`, and `arielWideNonDefault=0`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and existing `MSB3277` warnings; parallel build produced one transient `MSB3026` copy retry warning.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and existing `MSB3277` warnings.
- Unity-MCP `Pakuri/InGame/Validate Skill Data` logged `InGame skill data validation passed with 0 warning(s)`.
- Unity console warning/error read returned one unrelated Animator Controller error: `[Worker4] The Animator Controller (lightning-scout_1) you have used is not valid. Animations will not play`.

### History

- 2026-06-19: User requested Code Reviewer to verify whether Ariel's full skill structure and execution structure are configured into the target architecture.

## Task: 2026-06-19 Target Structure Re-Review After Builder Fix

### Task title

Re-review whether the current runtime reaches the target architecture and whether Ariel needs more isolated refactor before moving to other monsters.

### Goals

- Check the target responsibility split after Code Builder's structure fixes.
- Decide whether Ariel is fully final or whether the current structure is good enough to proceed with other monsters.
- Ground the decision in inspected code and validation output.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer does not implement fixes.
- Findings must be based on inspected files and command/Unity-MCP output.

### Role Owner

Code Reviewer

### Status

Passed with residual architecture caveat. The target structure is now good enough to move to other monsters before doing final handler-class polish. Ariel does not need to remain blocked for more isolated refactor.

### Next Actions

- Proceed with Eve/Vega/Sein/Rin migration using the generic node-backed choice and plan action path.
- After several monsters share the path, finish the remaining handler split by moving `ExecuteDamageEffectAction`, `ExecuteStatusEffectAction`, zone/visual spawn, and trigger cooldown/reload routines out of the legacy executor/runtime files into dedicated action handler classes.
- User performs Play Mode verification for Ariel because current review did not run gameplay.

### Evidence

- `MonsterUnitActor.cs` now contains presentation/animation/damage-popup/debug/collider-facing methods only; search found no `AutoAttackEnabled`, `AutoSkillEnabled`, `Statuses`, `Resources`, `SkillRuntime`, or `CurrentHealth` mutation in that file.
- `MonsterUnitRuntimeStateService.cs` owns next-day runtime reset for auto flags, statuses, shields, health, and active skill runtime state.
- `BaseUnitRuntimeModel.cs` owns identity, stats, resources, statuses, and skill runtime state; `MonsterUnitRuntimeModel.cs` inherits that runtime model.
- `UnitSkillController.cs` still owns skill runtime tick, auto/manual routing, and `SkillExecutionRequest` creation.
- `SkillExecutionPlan.cs` exposes `EffectActions` and `TriggerActions` instead of public raw `Plan.Effects` / `Plan.Triggers`.
- Search under `Pakuri/Assets/Scripts2/InGame/Skills` found no remaining `IsArielChoice`, `ApplyArielChoiceDefinition`, `Plan.Effects`, or `Plan.Triggers`.
- `SkillPlanActionDispatcher.cs` now owns effect-kind dispatch and trigger-action dispatch.
- `SkillMultiEffectExecutor.cs` still owns internal methods named `ExecuteDamageEffectAction`, `ExecuteStatusEffectAction`, persistent zone spawn, and visual spawn; `SkillTriggerRuntime.cs` still owns internal cooldown/reload and triggered attack action methods.
- Ariel active CSV scan returned `arielChoices=50`, `arielEffects=36`, `arielTriggers=6`, and `arielNodes=40`; Ariel still uses explicit trigger/effect rows for event-bound behavior.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- After clearing the Unity console, Unity-MCP `Pakuri/InGame/Validate Skill Data` logged `InGame skill data validation passed with 0 warning(s)`, and warning/error console read returned 0 entries.

### History

- 2026-06-19: User asked Code Reviewer whether the current structure reached the target and whether Ariel should be fully finished before moving to other skills.
