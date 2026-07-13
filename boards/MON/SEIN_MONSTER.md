## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-09 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/SEIN_MONSTER.md`.

# SEIN_MONSTER

## Scope

Sein dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Sein file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Active Sein task history is recorded below.

## Task: 2026-07-13 Sein Skill Runtime Visual Migration Design

### Task title

Design the safe migration of Sein prefab-backed skill visuals to runtime-created sprite, animator, and collider objects.

### Goals

- Create `boards/MON/SEIN_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md` using the inspected Rin plan format as structural reference.
- Classify every active Sein prefab visual by easy conversion, schema/runtime extension, retained fallback, or no-target status.
- Preserve current projectile, delayed-impact, zone-collider, Trigger, and Sein-E multi-deployment behavior.

### Constraints

- Role Owner is Designer; no code, CSV, scene, or prefab implementation is part of this task.
- Every conclusion uses inspected current code, active runtime CSV rows, prefab serialization, and Unity-MCP asset/hierarchy output.
- Active skill authoring is currently under `Pakuri/Assets/CSVdata/runtime/`; older `source/` board paths are not treated as current authority.
- Do not add runtime object/collider offset columns or graph params; user fixed both offsets at `(0,0)`.
- Remaining shared runtime/common-logic extensions require explicit user approval before Skill Builder / Code Builder implementation.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Designer

### Status

Design completed. Implementation has not started.

### Next Actions

- User reviews and approves or narrows the proposed Sein-E runtime multi-deployment and optional Sein-C impact-runtime extensions.
- After approval, assign Skill Builder / Code Builder to implement the phased migration from `boards/MON/SEIN_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`.
- Update DATA/asset boards only when implementation actually changes CSV schema/rows, runtime catalog wiring, or scene `EffectManager` mappings.

### Evidence

- Unity-MCP found nine Sein skill prefabs and reported each as a single-root hierarchy with one `SpriteRenderer` and one `BoxCollider2D`; seven also have one `Animator`.
- Serialized prefab inspection resolved exact sprite/controller paths, root scales, collider sizes, and current legacy offsets for all targets.
- User set runtime object/collider offsets to `(0,0)`; the corrected plan removes all proposed offset CSV/graph additions and preserves collider sizes only.
- `RuntimeSkillVisualFactory.cs` already supports one runtime sprite/animator/box; absent offset authoring leaves `RuntimeSkillHitboxSpec.Offset` at `(0,0)`.
- `InGameSkillDefinitionMapper.cs` and `SingleAttackSkillExecutor.cs` currently tie Sein-E multi-deployment/line-style behavior to the prefab path.
- `InGameProjectileActor.cs` currently accepts a prefab-only impact visual, so Sein C's separate projectile and delayed-impact roles cannot both use the one base skill `RuntimeVisual`.
- The created plan records per-target decisions, exact runtime values, required shared changes, migration order, risks, acceptance criteria, and Builder verification.

### History

- 2026-07-13: User asked Designer to create a Sein runtime skill migration document modeled on the Rin runtime visual migration plan.
- 2026-07-13: Designer inspected current Sein prefab assets/hierarchies, runtime CSV references, scene mappings, runtime visual factory, mappers, executors, actor collision paths, and normalized graph visual support, then created the migration plan without implementation changes.
- 2026-07-13: User clarified runtime object and collider offsets are fixed at `(0,0)` and must not receive new authoring columns. Designer removed the offset schema proposal and reclassified current non-zero prefab offsets as intentionally normalized legacy data.

## Task: 2026-07-13 Sein-D Prefab Collider Hit Detection

### Task title

Make Sein-D damage ticks use the authored prefab collider instead of implicit battlefield-wide `Field` targeting.

### Goals

- Let Sein-D use the same `InGameZoneSkillActor` prefab-collider path as Sein-C master 1 and Sein-D master 2.
- Preserve radius fallback when a zone instance has no collider.
- Preserve explicit all-target behavior authored through hit-target-count semantics.

### Constraints

- Role Owner is Code Builder.
- User-authored collider changes in `Sein_C_Master_1.prefab`, `Sein_D.prefab`, and `Sein_D_Master_2.prefab` are preserved without Builder edits.
- No CSV, scene, prefab, public API, or serialized field is changed by the runtime fix.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor-validated; Play Mode collider-boundary verification remains.

### Next Actions

- Verify in Play Mode that Sein-D damages and applies its tick status only to enemies overlapping `Sein_D.prefab`'s collider.
- Verify enemies outside the collider no longer receive Sein-D ticks.
- Reconfirm Sein-C master 1 and Sein-D master 2 continue using their prefab colliders.

### Evidence

- Current area-skill data contains one `Field` skill: `sein-d`, with prefab path `Assets/Prefab/Skill/Sein/Sein_D.prefab`.
- `InGameSkillDefinitionMapper.cs` no longer maps `SkillRuntimeKind.Field` to `Targeting.CoverAll` or `ZoneSkillData.Area.CoverAll`; explicit `hitAllTargets` remains the zone-wide override.
- `InGameZoneSkillActor.Initialize(...)` selects prefab hitbox evaluation when `coverAll` is false and an instantiated collider exists, otherwise retaining its radius fallback.
- Unity-MCP prefab hierarchy inspection found an active root `BoxCollider2D` on `Sein_C_Master_1`, `Sein_D`, and `Sein_D_Master_2`.
- Runtime and Editor C# builds completed with 0 errors; only the existing two MSB3277 assembly-conflict warnings remained on the final sequential build.
- Unity-MCP refresh/compile completed and the cleared console contained 0 errors.

### History

- 2026-07-13: User required Sein-D to follow prefab collider boundaries and added colliders to the three Sein zone prefabs.
- 2026-07-13: Code Builder removed the implicit `Field => CoverAll` mapping, preserved explicit all-target routing, and validated the existing collider-first/fallback zone actor path.

## Task: 2026-07-13 Sein A-J Node Migration

### Task title

Implement the approved Sein A-J migration from wide Choice and legacy Effect authoring to positional skill graph nodes.

### Goals

- Preserve `boards/MON/SEIN_NODE_MIGRATION_PROPOSAL.md` as the implementation contract and result record.
- Map Sein A-J to existing node/runtime functions wherever current code supports the behavior.
- Separate graph exposure/composer extensions from Trigger rows that must remain event envelopes.
- Remove migrated wide Choice behavior and legacy Effect rows after graph parity is authored.

### Constraints

- Role Owner is Code Builder for the implementation phase.
- Every conclusion is based on inspected current runtime CSV, node definitions, materializer, mapper, Effect composer, Trigger runtime, Executor code, and Sein reference files.
- Preserve current gameplay, IDs, prefab/collider contracts, and current authored values during migration.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and source/build validation completed. Prefabs and scenes were not changed. User Play Mode parity verification remains.

### Next Actions

- Verify Sein A-J combinations in Play Mode, especially B consecutive-hit scaling, C delayed/contact follow-ups, D/E persistent status ticks, E multi-deployment, G proc/reload gates, and J target-specific refunds.
- Keep the 17 Trigger rows as event envelopes unless a separate trigger-runtime migration is explicitly designed.

### Evidence

- `boards/MON/SEIN_NODE_MIGRATION_PROPOSAL.md` records the complete A-J mapping, migration sequence, risks, and acceptance criteria.
- Post-migration inspection counted Sein Choice 51, positional graph 121, legacy Effect 0, Trigger 17, and legacy direct node/param 0 rows.
- All 51 Sein Choice rows contain routing/metadata only; graph-migrated wide behavior values remaining outside routing columns count 0.
- `skill_node_definitions.csv` and `skill_node_definition_params.csv` expose `DamageDelayMultiplier`, `ConsecutiveHitDamageBonus`, and `AttachStatusPayload`, plus the existing runtime-consumed `EffectDamage` attack coefficient/tick interval parameters.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` proves the 21-column graph materialization, exact-one-operation Effect rule, monster-level direct-node mixing guard, and passive-owner generated Effect gate inference.
- `PakuriCsvRuntimeData.Build.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSnapshot.cs` materialize the new graph operations into the already-consumed runtime fields.
- Static validation found 0 CSV shape, unknown-node, required-argument, duplicate-order, and Effect-operation-count errors across the inspected runtime skill CSV set.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` each build with 0 errors and 2 pre-existing assembly-conflict warnings.
- Unity-MCP `Pakuri/Validate CSV Source Data` loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies without validation errors; `Pakuri/Sync CSV Runtime Catalog Assets` completed successfully.
- `git diff --name-only` for `Pakuri/Assets/Prefab/Skill/Sein` and `NewRunScene.unity` is empty.

### History

- 2026-07-13: User asked Designer to create a Sein node-migration proposal using the Rin proposal format and existing functions as much as possible.
- 2026-07-13: Designer inspected current Sein data/runtime support, identified two wide-to-graph exposure nodes and one hybrid damage/status Effect composer gap, then created the proposal without implementation changes.
- 2026-07-13: User explicitly assigned Code Builder. Code Builder added the shared node/composer exposure, authored 121 Sein graph rows, removed 19 migrated legacy Effects and all migrated Choice behavior values, retained 17 Trigger rows, and completed build plus Unity source validation.

## Task: 2026-05-28 Sein-I Base And Sein-G Trait-3 Shared Runtime Completion

### Task title

Extend the shared passive and trigger runtime so Sein-I base and Sein-G trait-3 can be fully implemented.

### Goals

- Add a shared passive-base modifier path that can modify an existing active skill while the passive is learned.
- Add a shared triggered-skill cast marker path so a passive can react only to an auto-triggered child skill cast.
- Finish the remaining Sein-I base and Sein-G trait-3 CSV implementation on top of those shared paths.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The work stays inside the Sein board, the DATA board, the passive-stat blueprint authority, the current shared runtime scripts, and the routed Sein CSV files.
- No Play Mode gameplay verification is performed by Codex.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and Unity editor-validated.

### Next Actions

- User verifies in Play Mode that learned `sein-i` speeds up `sein-d` tick cadence by 20%.
- User verifies that only Sein-G auto-triggered `sein-b` casts reduce `sein-a` reload, while manual `sein-b` casts do not.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now define and build shared `PassiveBase` modifier rows through `PassiveDefinition.BaseModifierChoices`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now applies learned passive base modifiers to active skill snapshots before chosen enhancement choices are added.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now forward a trigger-source skill marker through triggered skill cast execution, and triggered skills now dispatch shared `OnSkillCast` trigger events.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains `sein-i-base-shot-interval` and marks `sein-g-trait-3` as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `sein-g-auto-barrage-reload-trait3`, which uses the shared triggered-cast origin marker plus `condition_status_source_skill_id=sein-g` to gate the reload reduction to Sein-G-origin casts only.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed after one transient initial file-lock failure on `obj\Debug\Assembly-CSharp.dll`; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary, and `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-28: Code Builder added a shared passive-base modifier group and a shared triggered-skill cast origin marker path, then authored the remaining Sein-I base and Sein-G trait-3 rows on top of that runtime support.

## Task: 2026-05-27 Sein Passive CSV-Only F/H/I/J Authoring

### Task title

Implement the Sein passive portions that are already expressible through current CSV/runtime paths, without adding shared runtime logic.

### Goals

- Author `sein-f`, `sein-h`, the status portion of `sein-i`, and `sein-j` through existing status/effect/trigger CSV fields.
- Keep `sein-i` base tick-speed modification and `sein-g-trait-3` trigger-origin behavior outside this pass because their required shared paths are not present in the inspected runtime.
- Sync and validate the authored source CSV data through the Unity editor menus.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Routed edit set is limited to `monster_skill_effects.csv`, `monster_skill_choices.csv`, and `monster_skill_triger.csv`; Unity sync updates the generated runtime catalog asset.
- No shared runtime/common-logic extension is authored in this pass.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and editor-validated for the CSV-only scope. `sein-i` base tick-speed `+20%` and exact `sein-g-trait-3` remain pending shared runtime work.

### Next Actions

- Implement the approved shared passive-base to active-skill numeric modifier path for `sein-i` base tick speed.
- Implement a marker or composite trigger path so only `sein-g`-auto-triggered `sein-b` can reduce `sein-a` reload for `sein-g-trait-3`.
- Run gameplay verification in Unity Play Mode after the shared-runtime remainder is implemented.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` rows `sein-f-fire-damage` through `sein-e-passive-j-fire-resist-down-trait3` add 12 current-runtime status effects: F ally fire/crit effects, H C-hit fire-resist/exposure effects, I D-hit exposure effects, and J E-hit exposure/resist effects.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` rows `sein-f-trait-1` through `sein-j-trait-3` now route active-skill traits to `sein-a`, `sein-c`, `sein-d`, or `sein-e` where required and record the CSV-authored effects as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` rows `sein-j-kill-sein-a-reload` through `sein-j-kill-sein-e-cooldown-trait2` add 12 existing-action triggers for Sein-J: base 20% and trait-2 additional 10% cooldown/reload refunds after `sein-e` kills.
- `TextFieldParser` validation after authoring returned `FIELD_COUNT_OK` for the three routed CSV files: effects `columns=66 lines=110`, trigger `columns=44 lines=42`, choices `columns=89 lines=252`.
- Unity menu `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog ... with 5 monsters and 8 stage-one enemies.` and `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.
- The only exception entries emitted during the isolated Unity menu run were MCP transport connection-exit logs sourced from `Library/PackageCache/com.coplaydev.unity-mcp.../StdioBridgeHost.cs`, not CSV validation failures.

### History

- 2026-05-27: Skill Builder / Code Builder edited only CSV-solvable Sein passive behavior, field-validated the routed CSV set, then used Unity validation and catalog sync menus. Shared-runtime-only I base and G trait-3 behavior remained excluded.

## Task: 2026-05-27 Designer Review Of Sein Passive F-J Blockers

### Task title

Re-check whether Sein passive F-J requires new shared runtime work, or whether the existing CSV and runtime paths already cover the requested behavior.

### Goals

- Verify the reported F-J blockers against inspected CSV and runtime code instead of assuming the prior Skill Builder conclusion is complete.
- Separate behavior already expressible through current CSV paths from exact behavior that still lacks an execution path.
- Keep the review within the active Sein board, the selected passive blueprint, the routed F-J CSV rows, and the runtime files named or directly reached by the blocker.

### Constraints

- Role Owner is Designer; this task records design verification only and does not implement CSV or runtime changes.
- The inspected active CSV set was limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `status_effects.csv`.
- No reference markdown, archive markdown, unrelated monster board, UI board, RUN board, or DATA board was opened for this review.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Designer

### Status

Reviewed. The previous blocker list is over-broad: new passive-specific status kinds are not required for exact F/H/J status effects or the status portion of I. Two exact behaviors still require user-approved shared runtime work.

### Next Actions

- Return F/H/J and the status portion of I to Skill Builder as CSV-only work using existing `passive-buff`, `fire-resist-down`, and `fire-exposure` status ids plus existing `requires_passive_skill_id` / trigger paths.
- Before authoring trait-gated active effects, set the relevant passive choice `target_skill_id` to the affected active skill so the active skill snapshot can see that chosen passive enhancement.
- Ask the user whether to approve a shared runtime path for:
  - `sein-i` base applying `Sein-D` tick-speed `+20%` while the passive is learned;
  - `sein-g-trait-3` reducing `Sein-A` reload only after a `Sein-G`-triggered `Sein-B` execution.

### Evidence

- `boards/SkillBluePrint/passive-stat-blueprint.md` treats always-on skill-specific numeric modifiers and status-rule numeric modifiers as ordinary passive-stat behavior, while event-driven follow-ups remain stop-and-ask behavior.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` has no retained `sein-f`, `sein-h`, `sein-i`, or `sein-j` execution rows; their `monster_skills.csv` rows are marked `RuntimeImplemented` but currently only carry summary text for these effects.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` sets status `SourceSkillId` from each `effect_id`, defaults merge policy to `SameSourceRefresh`, supports `requires_passive_skill_id`, and copies flat fire-resist and element-damage-taken numeric payloads into runtime statuses.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` keeps source-aware statuses of the same `StatusEffectKind` separate by source id, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` sums their flat element resistance and element damage-taken values. Existing `fire-resist-down`, `fire-exposure`, and `passive-buff` ids therefore cover distinct additive F/H/I/J status effects without new enum ids.
- Existing CSV rows `ariel-e-passive-j-action-speed`, `rin-j-physical-defense-down`, and their trait variants already demonstrate active-skill effects gated by `requires_passive_skill_id` and passive enhancements routed to the affected active skill through `target_skill_id`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` already supports passive-owner `OnKill`, `CooldownRefund`, and `ReloadReduce` actions, so Sein-J kill-based cooldown refund can be authored as existing trigger rows, including separate target rows when multiple Sein active skills must be refunded.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` only adds choice modifiers to an active skill snapshot from chosen choices whose target resolves to that active skill. No inspected path applies a learned passive base row directly to `sein-d` tick interval, so `sein-i` base tick-speed `+20%` remains a real shared-runtime gap.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` executes one trigger action per trigger row and forwards a triggered skill as `sein-b`; no inspected CSV-visible marker identifies that B cast as specifically produced by `sein-g`. Therefore exact `sein-g-trait-3` remains blocked without a shared marker or composite-action path.

### History

- 2026-05-27: Designer inspected the passive-stat blueprint, routed Sein F-J CSV rows, status merge/runtime code, passive multi-effect gating, trigger actions, and active snapshot choice application. The inspection disproved the proposed new-status-kind blocker for F/H/I/J status payloads and retained only the I base tick-speed and G trait-3 trigger-origin gaps as shared-runtime decisions.

## Task: 2026-05-27 Sein Passive F-J Skill Builder Blocker Review

### Task title

Attempt Sein passive F-J Skill Builder authoring from the passive-stat blueprint, keep only validator-safe progress, and record the shared-runtime blockers that prevent exact completion.

### Goals

- Implement Sein passive `f-heated-aim`, `g-flame-barrage`, `h-burning-trajectory`, `i-thermal-spread`, and `j-doomsday-omen` through the routed passive skill CSV set when the existing shared runtime permits it.
- Keep repository data validator-clean while testing whether the current passive/stat/trigger paths are sufficient.
- Stop before widening into a new shared runtime/common-logic extension that was not yet user-approved for this passive task.

### Constraints

- Role Owner is Skill Builder.
- Routed CSV read/edit set was limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `status_effects.csv`.
- Reference inputs came only from `Pakuri/reference/2.Monster/sein/skill/f-heated-aim.md` through `j-doomsday-omen.md` plus `boards/SkillBluePrint/passive-stat-blueprint.md`.
- New shared runtime/common-logic extensions discovered during authoring must stop and be reported instead of being silently invented.

### Role Owner

Skill Builder

### Status

Partially advanced, then stopped at shared-runtime blockers. Repository restored to validator-clean state.

### Next Actions

- Ask the user whether to widen scope into shared runtime work for:
  - new supported runtime status kinds or a string-backed passive status path so F/H/I/J can apply distinct additive passive debuffs/buffs without enum rejection;
  - a passive-base skill-modifier path so Sein-I base can grant `Sein-D` tick-speed `+20%` without relying on a selectable enhancement row;
  - a trigger marker path so `Sein-G trait-3` can detect “auto-triggered Sein-B only” before reducing `Sein-A` reload.
- If the user approves that wider shared work, resume from this blocker list instead of rediscovering it.

### Evidence

- `boards/SkillBluePrint/passive-stat-blueprint.md` explicitly limits the common path to always-on stat/skill/status modifiers and says event-driven passives should stop and ask rather than be guessed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` shows only chosen passive-enhancement rows can modify an active skill snapshot via `SkillChoiceResolver`; there is no current passive-base path that directly applies `Sein-I` base `Sein-D` tick-speed `+20%`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` accepts only the enum-backed ids through `StatusEffectUtility.TryParse(...)`; attempted new ids for distinct Sein passive statuses were rejected by the CSV validator.
- Unity editor validation, after reverting the unsupported new passive status/effect rows, logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Valid Sein-G progress was retained:
  - `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `sein-g-auto-barrage-base`, `sein-g-auto-barrage-trait1`, and `sein-g-auto-barrage-trait2`.
  - `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `sein-g-trait-1` and `sein-g-trait-2` as `RuntimeImplemented`, while `sein-g-trait-3` remains `DataOnlyUnsupported`.

### History

- 2026-05-27: Skill Builder first attempted full F-J authoring with new passive-specific status ids and trigger/effect rows, then proved through Unity validation and direct runtime reflection that those ids are rejected because they are not supported by `StatusEffectKind`.
- 2026-05-27: Builder restored the invalid F/H/I/J passive status/effect rows, kept only validator-safe Sein-G trigger authoring, and recorded the remaining shared-runtime blockers for a later explicit user decision.

## Task: 2026-05-27 Sein-G Triggered Skill Damage Override Support

### Task title

Add the minimum shared runtime path needed for Sein-G so a passive trigger can launch a triggered active skill with a trigger-scoped damage multiplier.

### Goals

- Let passive or trigger runtime launch a triggered active skill with a damage multiplier that applies only to that triggered execution.
- Keep the change shared and minimal so existing triggered-skill callers remain behavior-compatible when they do not pass an override.
- Avoid adding broader trigger snapshot or custom Sein-only branches before Sein-G authoring begins.

### Constraints

- Role Owner is Code Builder.
- Only the shared triggered-skill execution path is extended in this step; no Sein-G CSV authoring is part of this change.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Skill Builder can now author Sein-G trigger rows that call `sein-b` with a trigger-row `damage_multiplier`, knowing that the multiplier will be applied only to that triggered B execution.
- User still needs Play Mode verification after Sein-G rows are authored.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now accepts an optional `triggeredDamageMultiplier` when forwarding triggered-skill execution to the skill system.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now accepts that override on the triggered execution path and applies it through `SkillExecutionSnapshot.ApplyDynamicDamageMultiplier(...)` before executor dispatch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now forwards trigger-row `damage_multiplier` into triggered-skill execution instead of dropping it for `TriggerActionKind.TriggeredSkill`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after the change; only the existing `MSB3277` warnings remained.

### History

- 2026-05-27: Designer review concluded that Sein-G could already express the ally-fire trigger, proc chance, cooldown gate, and reload reduction, but lacked a shared path for “triggered B only deals 60% damage.”

## Task: 2026-05-27 Sein-E Manual Parallel Deployment Visual Fix

### Task title

Fix Sein-E manual casting so multi-deployment lines no longer collapse onto one center and so the prefab visual is scaled/oriented like the old line attack presentation.

### Goals

- Make manual Sein-E cast multiple deployments appear simultaneously instead of overlapping at one center.
- Align manual Sein-E deployment centers in parallel to the clicked direction.
- Restore large line-like visual scaling for `Sein_E.prefab` under the new `SingleAttack` multi-deployment runtime.

### Constraints

- Role Owner is Code Builder.
- The fix stays in shared `SingleAttackSkillExecutor` runtime code; no CSV or prefab content rewrite is part of this step.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that manual Sein-E now shows 3 simultaneous parallel lines in the clicked direction.
- User verifies the line visual length/width now matches the intended old `LineAttack` presentation more closely instead of spawning as a very small effect.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now routes both auto and manual Sein-E multi-deployment casts through the same shared target-anchored center allocation path instead of keeping a manual-only parallel-center branch.
- The same executor still rotates and rescales multi-deployment prefabs using the old line-style presentation rule (`length=31`, width from resolved skill radius), but its facing now follows the resolved deployment center direction just like auto casting.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillDeploymentCenterUtility.cs` no longer disables nearest-distinct target-center allocation just because the cast came from manual aim input.
- `Pakuri/Assets/Prefab/Skill/Sein/Sein_E.prefab` still has local scale `0.22610311` and a `BoxCollider2D`, which explained why the unadjusted `SingleAttack` presentation looked much smaller than the previous line actor presentation.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 code errors after the fix; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 code errors when rerun sequentially; only the existing `MSB3277` warnings remained.

### History

- 2026-05-27: User reported that Sein-E only showed one small line when cast manually, which inspection traced to manual center duplication in the new multi-deployment path and to the loss of the old line actor's visual scaling rule.
- 2026-05-27: User then reported that manual Sein-E still behaved differently from auto; Builder removed the manual-only parallel deployment branch so manual casts now use the same nearest-distinct target-center allocation as auto casts.

## Task: 2026-05-27 Sein-E Presence Zone CSV Validation Fix

### Task title

Allow Sein-E / Sein-D status-only persistent zones to pass shared CSV validation without inventing fake damage values.

### Goals

- Keep `sein-d-zone-presence` and `sein-e-master2-zone-presence` authored as zero-damage status-refresh zones.
- Fix the shared CSV validator so these rows do not require fake positive `base_damage`.
- Verify the original Unity error no longer appears when `Pakuri/Validate CSV Source Data` runs.

### Constraints

- Role Owner is Code Builder.
- The fix must stay on shared validation logic instead of mutating Sein data into unintended damage rows.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- User verifies in Play Mode that Sein-D and Sein-E master-2 presence zones still refresh `sein-d-superheated-presence` without adding unintended damage.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now allows `SkillMultiEffectKind.Damage` rows with `base_damage <= 0`, zero coefficients, persistent-zone timing, and status payloads to pass validation as status-only persistent zones.
- The authored rows remain `sein-d-zone-presence` and `sein-e-master2-zone-presence` in `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv`; no fake damage numbers were added.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun sequentially after one file-lock failure caused by parallel compilation.
- Unity menu `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and did not log the earlier `requires positive base_damage` errors for `sein-d-zone-presence` or `sein-e-master2-zone-presence`.

### History

- 2026-05-27: User reported Unity startup validation errors because status-only presence zones were authored as `Damage` effects with `base_damage=0`, which the shared validator previously rejected unconditionally.

## Task: 2026-05-27 Sein-E Multi-Deployment SingleAttack And Master Zone Follow-Up

### Task title

Extend shared `SingleAttack` runtime for Sein-E multi-deployment casting and implement Sein-E enhancement/master rows on that shared path.

### Goals

- Convert `sein-e` from `LineAttack` authoring to `SingleAttack` authoring without losing the intended multi-line cast feel.
- Let one Sein-E cast execute multiple independent prefab-hitbox `SingleAttack` deployments using nearest-distinct target allocation with repeat fallback.
- Implement Sein-E trait-3, trait-4, trait-5, and master-2 through shared multi-effect/status/runtime paths instead of a Sein-only branch.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Selected blueprint is `boards/SkillBluePrint/single-attack-blueprint.md`.
- Routed CSV authority was limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_modifier_skill_choice.csv`, and `status_effects.csv`.
- User explicitly approved widening scope to shared runtime/common-logic extension for the required multi-deployment behavior.
- `Sein-E` must use `Assets/Prefab/Skill/Sein/Sein_E.prefab`.
- `Sein-E master-2` must reuse Sein-D-style zone behavior/effects instead of a separate bespoke prefab family.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that base `sein-e` fires 3 independent hitbox deployments at once, preferring distinct nearby enemies and only overlapping targets when fewer than 3 enemies exist.
- User verifies that `sein-e-trait-4` raises deployment count to 4 while reducing damage to 85%.
- User verifies that `sein-e-master-2` spawns a Sein-D-style zone at each Sein-E deployment center and that `sein-e-trait-5` only gains bonus damage against enemies currently inside those superheated zones.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillDeploymentCenterUtility.cs` now provides shared target-anchored deployment-center allocation with nearest-distinct selection plus repeat fallback.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs`, `InGameSkillDefinitionMapper.cs`, and `Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now support `SingleAttack` multi-deployment prefab-hitbox execution, using `hit_target_count` as base deployment count when the skill is authored as prefab-hitbox `SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` and `Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now add shared `OnDeploymentCast` timing so each Sein-E deployment center can run follow-up effects.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ZoneSkillExecutor.cs` now reuses the same deployment-center utility for existing multi-zone target anchoring and runs cast-timed multi-effects at each spawned zone center.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `sein-e` as `runtime_kind=SingleAttack`, `hit_target_count=3`, base `fire-resist-down` status application for `5s` with flat reduction `10`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `sein-e-trait-1` through `sein-e-master-2` as `RuntimeImplemented`, sets `sein-e-trait-4.hit_target_count_bonus=1` with `damage_multiplier=0.85`, and gates `sein-e-trait-5` on `conditional_target_status_id=sein-d-superheated-presence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now adds `sein-e-trait3-fire-resist-bonus` as a shared `OnHit` status effect, plus `sein-e-master2-zone-damage` and `sein-e-master2-zone-presence` as shared `OnDeploymentCast` persistent-zone follow-ups that reuse Sein-D values and visuals.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains `sein-d-superheated-presence`, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now recognizes that status id in shared runtime parsing/definition paths.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.

### History

- 2026-05-27: User clarified that Sein-E should not be a capped-3-target attack, but a simultaneous multi-cast skill whose independent hitboxes can each hit unlimited enemies.
- 2026-05-27: Builder reused the distinct-target-allocation concept from existing zone runtime behavior, promoted it into a shared deployment-center utility, and added a shared `OnDeploymentCast` follow-up timing so Sein-E master-2 could spawn Sein-D-style zones per deployment center.

## Task: 2026-05-27 Sein-B Manual Burst And Projectile Hold Input Fix

### Task title

Fix Sein-B manual burst continuation and add projectile-only manual hold firing in `NewRunScene`.

### Goals

- Make `sein-b` complete its full burst sequence from one manual click even when player auto-skill is off.
- Allow manual projectile skills to keep firing while the mouse button is held, using the current cursor direction at each shot.
- Keep beam, zone, and single-attack skills on their existing one-click manual behavior.

### Constraints

- Role Owner is Code Builder.
- No CSV, prefab, or scene serialization change is part of this fix; the issue is resolved in runtime input ownership only.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that `sein-b` fires all 5 burst shots from one click in manual mode.
- User verifies that holding the mouse continues firing projectile skills toward the current cursor direction, while non-projectile active skills still react only to click-start.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors `sein-b` with `magazine_capacity=4`, `shot_interval_seconds=0.18`, and `projectile_burst_count=5`; no Sein-B CSV row change was needed.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now stores latched manual projectile input, continues manual execution while a projectile runtime is bursting, and limits hold-repeat behavior to `ProjectileSkillData`.
- The same combat manager now refreshes projectile manual aim from the current cursor position while the mouse button is held, but leaves non-projectile manual skills on `wasPressedThisFrame` behavior.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock on `Assembly-CSharp.dll`.

### History

- 2026-05-27: User reported that Sein-B fired only one manual projectile after DebugUI learn, even though the skill CSV authored a 5-shot burst.
- 2026-05-27: Code Builder confirmed the burst CSV/runtime data were already correct and fixed the manual-input ownership path so projectile bursts can continue without enabling full auto-skill mode.

## Task: 2026-05-27 Sein-C And Sein-D Enhancement/Master Runtime Completion

### Task title

Implement Sein-C and Sein-D enhancement/master behavior on shared projectile, multi-effect, status, and zone runtime paths.

### Goals

- Convert `sein-c` from area-attack authoring to delayed-impact projectile authoring using the shared projectile runtime.
- Implement Sein-C trait/master rows and Sein-D trait/master rows through current shared choice/effect/status paths where possible.
- Reuse shared persistent-zone spawning for Sein-C master-1 and Sein-D master-2 instead of adding Sein-only runtime branches.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprints are `boards/SkillBluePrint/projectile-blueprint.md` for Sein-C and `boards/SkillBluePrint/area-attack-blueprint.md` for Sein-D.
- User explicitly approved widening scope to shared runtime/common-logic extension and CSV schema expansion where needed.
- The following values are user-provided or inferred from the nearest inspected authority and should stay explicit until the user replaces them:
  - `sein-c.projectile_speed=20` is inferred from the requested reuse of `Assets/Prefab/Skill/Sein/Sein_B.prefab`.
  - `sein-a-hit-mark` duration `5s` is inferred for Sein-C trait-5 gating.
  - `sein-c-master-1` residual zone radius `1.2` and tick interval `0.5s` are inferred; the user only specified `25%` damage for `1.5s`.
  - `sein-d-master-2` residual zone radius `3.2` and tick interval `0.5s` reuse inspected base Sein-D values as the nearest available authority.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-C now fires the requested projectile visual, stops on first contact, then explodes after the delay.
- User verifies Sein-C master-2 contact damage and visual, Sein-C master-1 residual flame-zone spawn, and Sein-D master-2 residual ember-zone spawn.
- If the inferred Sein-C/Sein-D zone radius or tick values should change, update the authored effect rows rather than adding new runtime branches.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/c-flame-trajectory.md` and `Pakuri/reference/2.Monster/sein/skill/d-superheated-zone.md` were the inspected skill references for the requested Sein-C and Sein-D behavior bundle.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now sets `sein-c.runtime_kind=CooldownProjectile`, `sein-c.projectile_speed=20`, `sein-c.damage_delay_seconds=0.8`, and `sein-c.skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_C.prefab`; it also sets `sein-d.skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_D.prefab`, `sein-d.active_duration_seconds=4`, `sein-d.shot_interval_seconds=0.5`, and `sein-d.status_effect_id=sein-d-heat-stack`.
- The same skill CSV now marks `sein-a` with `status_effect_id=sein-a-hit-mark`, `status_chance=1`, `status_duration_seconds=5`, `status_max_stacks=1`, and `status_stack_amount=1` so Sein-C trait-5 can stay on a shared conditional-status path.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `damage_delay_multiplier`, updates `sein-c-trait-4` to `0.6`, gates `sein-c-trait-5` on `sein-a-hit-mark`, and authors `sein-c-master-1`, `sein-c-master-2`, and `sein-d-master-2` as `SharedExtension`; it also updates `sein-d-trait-1`, `sein-d-trait-2`, `sein-d-trait-5`, and `sein-d-master-1` with the shared duration / interval / conditional-damage fields.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now includes `active_duration_seconds` and `tick_interval_seconds` columns plus `sein-c-master2-contact`, `sein-c-master1-zone`, and `sein-d-master2-zone` effect rows using `Assets/Prefab/Skill/Sein/Sein_C_Master-2.prefab`, `Assets/Prefab/Skill/Sein/Sein_C_Master_1.prefab`, and `Assets/Prefab/Skill/Sein/Sein_D_Master_2.prefab`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains `sein-a-hit-mark` and `sein-d-heat-stack`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps monster `sein` skill `sein-c` through `EffectManager` to the requested flying-arrow prefab `Assets/Prefab/Skill/Sein/Sein_B.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs` now supports projectile contact-stop, delayed impact, on-hit follow-up effects, on-expire follow-up effects, and delayed area resolution.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now resolves shared `OnHit` / `OnExpire` multi-effects for projectiles, uses scene `EffectManager` projectile visuals before effect prefabs, and creates a projectile actor even when only delayed-impact behavior is present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now supports `EventTarget` damage/status targeting and shared persistent damage-zone spawning from effect rows.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now recognizes `sein-a-hit-mark` and `sein-d-heat-stack` as shared runtime statuses.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.
- Unity console filtering after `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` showed `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` and did not show a `Pakuri` CSV failure in the retrieved entries.

### History

- 2026-05-27: User requested Code Builder / Skill Builder implementation for Sein-C and Sein-D enhancement/master behavior with explicit prefab paths for projectile, explosion, and zone visuals.
- 2026-05-27: User approved shared projectile delayed-impact expansion and shared residual-zone reuse instead of a helper active-skill row approach.

## Task: 2026-05-26 Sein-B Enhancement And Master Runtime Completion

### Task title

Implement Sein-B enhancement choices and master effects through the shared burst projectile and shared consecutive-hit extension paths.

### Goals

- Mark Sein-B trait 1-4 and master 1-2 as implemented through existing shared projectile choice modifiers.
- Add a reusable shared consecutive-hit damage extension for projectile skills.
- Implement Sein-B trait 5 on that shared consecutive-hit path with +8% same-target consecutive-hit damage up to +40%.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprint is `boards/SkillBluePrint/projectile-blueprint.md`.
- User explicitly approved widening scope to a new shared runtime/common-logic extension and new CSV columns for projectile consecutive-hit behavior.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-B trait 1-4 and master 1-2 modify burst count, damage, reload speed, shot interval, and crit chance as expected.
- User verifies in Play Mode that Sein-B trait 5 deals no bonus on the first hit to a target, then gains +8% per same-target consecutive hit up to +40%, and resets when the hit target changes.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md` defines Sein-B trait 1 burst count +2, trait 2 damage +25%, trait 3 reload speed +30%, trait 4 shot interval -25%, trait 5 same-target consecutive hit damage +8% up to +40%, master 1 burst count +4 with damage -20%, and master 2 burst count -2 with damage +90% and crit chance +20%.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `consecutive_hit_bonus_rate` and `consecutive_hit_max` columns, marks `sein-b-trait-1` through `sein-b-trait-5` and `sein-b-master-1` through `sein-b-master-2` as `RuntimeImplemented`, sets `sein-b-trait-5` to `consecutive_hit_bonus_rate=0.08` and `consecutive_hit_max=0.4`, and sets `sein-b-master-2` to `crit_chance_bonus=0.2`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now carry the new consecutive-hit choice fields through the CSV runtime definition/build path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carry those fields through the shared choice-to-snapshot path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now tracks the last projectile hit target and repeat count and resolves a shared same-target consecutive-hit damage multiplier from choice snapshot data, with fallback to `ProjectileSkillData` fields when present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs` and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now apply that shared consecutive-hit multiplier on both prefab projectile hits and direct-hit fallback projectile damage.
- CSV field-count validation returned `monster_skill_choices.csv HEADER_WIDTH=88 FIELD_COUNT_OK`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after a first parallel file-lock retry; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity editor validation logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity runtime catalog inspection returned `sein-b-trait-5|state=RuntimeImplemented|dmg=False:1|burst=0|crit=0|consec=0.08:0.4` and `sein-b-master-2|state=RuntimeImplemented|dmg=True:1.9|burst=-2|crit=0.2|consec=0:0`.

### History

- 2026-05-26: User asked Skill Builder to implement Sein-B enhancement and master effects using `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md`.
- 2026-05-26: Builder confirmed master 2 crit chance already fit the shared choice path, but trait 5 same-target consecutive-hit damage required a new shared projectile runtime extension and new choice CSV columns.
- 2026-05-26: User approved widening scope to a reusable shared consecutive-hit extension and new CSV columns, and Builder implemented the shared path plus Sein-B choice wiring.

## Task: 2026-05-26 Sein-A Enhancement And Master Runtime Completion

### Task title

Implement Sein-A enhancement choices and master effects on the shared projectile and hit-trigger SingleAttack paths.

### Goals

- Mark Sein-A trait 1-5 and master 1 as implemented through existing shared projectile choice modifier fields.
- Implement Sein-A master 2 as an OnOutgoingDamage hit trigger that runs a shared SingleAttack explosion.
- Use `Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab` for the master-2 small explosion effect.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprint is `boards/SkillBluePrint/projectile-blueprint.md`.
- User pointed out the existing hit-trigger SingleAttack common runtime, so master-2 stays on that shared trigger path instead of adding Sein-only logic.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-A trait 1-5 and master 1 modify damage, magazine, reload speed, pierce, and shot interval as expected.
- User verifies in Play Mode that Sein-A master 2 spawns `Sein_A_Master-2.prefab` and deals 50% Fire explosion damage on Sein-A hits.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md` defines Sein-A as a projectile / magazine basic attack with trait 1 damage +25%, trait 2 magazine +4, trait 3 reload speed +30%, trait 4 pierce +1 and damage +10%, trait 5 shot interval -20% and damage -10%, master 1 damage +55% and pierce +1, and master 2 50% Fire small explosion on hit.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `sein-a-trait-1` through `sein-a-trait-5`, `sein-a-master-1`, and `sein-a-master-2` as `RuntimeImplemented`.
- The same choice rows use existing shared projectile choice fields: `damage_multiplier`, `magazine_bonus`, `reload_time_multiplier`, `pierce_bonus`, and `shot_interval_multiplier`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now adds `sein-a-master2-hit-explosion` with `trigger_event=OnOutgoingDamage`, `requires_active_choice_id=sein-a-master-2`, `event_skill_id=sein-a`, `trigger_action=SingleAttack`, `damage_source=EventAppliedDamage`, `damage_source_multiplier=0.5`, `attribute=Fire`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab`.
- `Pakuri/Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab` exists and contains a `BoxCollider2D`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now dispatches source-owned `OnOutgoingDamage` triggers before the existing passive-owner trigger scan, enabling active-skill choice-gated hit triggers without Sein-only branches.
- CSV field-count validation returned `FIELD_COUNT_OK` for `monster_skill_choices.csv` 86 columns / 252 rows, `monster_skill_triger.csv` 44 columns / 27 rows, and `monster_skills.csv` 57 columns / 52 rows.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity editor code inspection returned all Sein-A trait/master rows as `RuntimeImplemented` and returned `trigger=sein-a-master2-hit-explosion|event=OnOutgoingDamage|action=SingleAttack|source=sein-a|choice=sein-a-master-2|eventSkill=sein-a|damage=EventAppliedDamage:0.5|prefab=Sein_A_Master-2`.

### History

- 2026-05-26: User asked Skill Builder to implement Sein-A enhancement and master effects using `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md`, with master-2 using `Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab`.
- 2026-05-26: Initial blueprint pass stopped on projectile impact explosion, then user pointed to the existing hit-trigger SingleAttack common logic; Builder reused that shared path and added a small source-owned `OnOutgoingDamage` dispatch extension.

## Task: 2026-05-20 Sein-B Shared Burst Projectile Implementation

### Task title

Implement Sein-B through the shared projectile burst extension.

### Goals

- Add a shared sequential burst count path instead of a Sein-only projectile branch.
- Make `sein-b` fire 5 projectiles per cycle at `shot_interval_seconds`, repeat that cycle `magazine_capacity` times, then wait on cooldown/reload.
- Wire `sein-b` to the requested `Assets/Prefab/Skill/Sein/Sein_A.prefab` visual through `EffectManager`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Unity Play Mode gameplay verification remains user-owned.
- Keep the implementation reusable for future projectile skills such as Vega.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and non-gameplay verified.

### Next Actions

- User verifies in Play Mode that Sein-B emits 5 sequential projectiles per cycle and repeats for 4 magazine cycles before the 6 second recovery.
- If Sein-B crit-chance master behavior is required, implement that as a separate choice-modifier extension because the current shared choice path still lacks crit chance modifiers.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md` defines `?꾪솚 ??5`, `?꾩갹 ??4`, `?ъ옣???쒓컙 6.0珥?, `諛쒖궗 媛꾧꺽 0.18珥?, base fire damage `14`, attack coefficient `0.65`, and projectile speed `20.0`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `projectile_burst_count`; the `sein-b` row maps to `projectile_burst_count=5`, `magazine_capacity=4`, `shot_interval_seconds=0.18`, `cooldown_seconds=6`, `reload_seconds=6`, `projectile_speed=20`, and `pierce_count=0`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now tracks queued burst shots and starts recovery only after the queued burst completes and the magazine is exhausted.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` keeps `AdditionalProjectileBonus` as simultaneous fan-spread only when `BurstProjectileCount <= 1`; burst skills use that bonus in runtime burst count instead.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `sein-b` to prefab GUID `256552cb82ec9c2499fc2e0e01d20dd2`, the existing `Assets/Prefab/Skill/Sein/Sein_A.prefab`.
- `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` followed by runtime mapping inspection returned `sein-b:burst=5;mag=4;interval=0.18;cooldown=6;reload=6;speed=20`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained. A first parallel runtime build hit only an `Assembly-CSharp.dll` file lock and passed when rerun alone.
- Unity-MCP console after refresh still contained MCP client-exit and `UnityEditor.Graphs` exceptions, but no `Pakuri` skill/CSV error was reported in the retrieved entries.

### History

- 2026-05-20: User approved an exact shared implementation for the Sein-B 5-shot burst cycle instead of the approximate existing magazine projectile behavior.

## Task: 2026-06-07 Sein Animation Clip Controller And Prefab Wiring

### Task title

Create Sein's shared Rin-contract animation assets and wire the monster prefab animator.

### Goals

- Create Sein's six animation clips: attack 1, attack 2, attack 3, idle, hit, and death.
- Create `Sein_Animation_Cont.controller` with the same parameter contract as Rin: `Attack`, `AttackIndex`, `Hit`, and `Death`.
- Add Animator and `Animation_Controller` components to `Sein_Unit.prefab` and connect `MonsterUnitActor.animationController`.

### Constraints

- Role Owner is Code Builder.
- The controller contract follows inspected `Rin_Animation_Cont.controller`.
- Unity Editor import and Play Mode animation verification were not available in this session.

### Role Owner

Code Builder

### Status

Implemented and locally YAML/build-verified.

### Next Actions

- User lets Unity import the new `.anim` and `.controller` assets.
- User verifies in Play Mode that Sein plays idle, attack 1-3, hit, and death through the shared animation parameter contract.

### Evidence

- `Pakuri/Assets/Image/Monster/Sein/Animation/Animation_Sein_Sprite` now contains 6 `Anim_Sein_*.anim` files, 6 matching `.anim.meta` files, `Sein_Animation_Cont.controller`, and `Sein_Animation_Cont.controller.meta`.
- `Select-String` confirmed `Sein_Animation_Cont.controller` contains `Attack`, `AttackIndex`, `Hit`, `Death`, and the states `Anim_Sein_Attack_1`, `Anim_Sein_Attack_2`, `Anim_Sein_Attack_3`, `Anim_Sein_Hit`, `Anim_Sein_Idle`, and `Anim_Sein_Dead_1`.
- `Pakuri/Assets/Prefab/Monster/Sein_Unit.prefab` now has `animationController: {fileID: 900300000000002}`, an `Animator` with controller GUID `ea44a003bbf345bbbccbfb750101f1ea`, and an `Animation_Controller` with `idleState: Anim_Sein_Idle`, `deadState: Anim_Sein_Dead_1`, and `attackStateCount: 3`.
- The controller meta GUID check returned `Sein controllerGuid=ea44a003bbf345bbbccbfb750101f1ea linked=True`.
- The generated idle clip check returned `Sein idleName=Anim_Sein_Idle spriteRefs=16`.
- 2026-06-07 follow-up correction verified `Sein root=4596420534878418281 rootRefs=true animatorOwner=4596420534878418281 controllerOwner=4596420534878418281 ok=true` after fixing the generated Animator and `Animation_Controller` component owner fileIDs to the root `Sein_Unit` GameObject.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing `MSB3277` warnings remained.

### History

- 2026-06-07: User asked Code Builder to create each monster's six animation clips, create controllers with Rin's parameter contract, and wire each monster prefab Animator controller.
- 2026-06-07: User reported the non-Rin monster prefabs still did not show assigned Animator / `Animation_Controller`; Code Builder found the generated component blocks were owned by the wrong GameObject fileID and corrected them to the root Unit GameObject.

## Task: 2026-07-13 Sein Skill Runtime Visual Refactor

### Task title

Move Sein A-E skill visuals from prefab-first execution to runtime-composed visual/hitbox execution.

### Goals

- Runtime-compose all currently prefab-backed Sein A-E skill visual targets identified in `SEIN_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`.
- Keep Sein C projectile and impact visuals separate.
- Keep Sein E multi-deployment and line-style collider behavior.
- Use centered runtime roots and colliders with no Sein offset authoring.

### Constraints

- Role Owner is Code Builder; no Skill Blueprint is used.
- Prefab and scene references stay as fallback until user Play Mode verification.
- Play Mode remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and non-Play-Mode verified.

### Next Actions

- User verifies A-E runtime visuals, animation, damage boundaries, C delayed impact, and E deployment count/orientation in Play Mode.
- After parity confirmation, remove retained fallback prefab paths and A/B/C scene mappings in a separate cleanup.

### Evidence

- Base CSV rows now define runtime visuals/hitboxes for `sein-a`, `sein-b`, `sein-c`, `sein-d`, and `sein-e` using inspected prefab values.
- Trigger/choice graph rows now define runtime visuals for A master 2, C master 1/master 2, D master 2, and E master 2.
- Sein C uses `ImpactRuntimeVisual` for its delayed B-1 impact while its projectile uses `Sein_Shoot.png`.
- Runtime visual selection precedes prefab fallback in `ProjectileSkillExecutor`, `InGameProjectileActor.ResolveImpact`, `SkillMultiEffectExecutor`, and `SingleAttackSkillExecutor`.
- All 7 edited CSV files passed row/header column-count checks; both C# projects built with 0 errors; Unity-MCP sync and post-sync validation produced 0 error logs.

### History

- 2026-07-13: User authorized Code Builder to start the refactor and fixed the zero-offset contract.
- 2026-07-13: Runtime composition code/data/catalog wiring was implemented; manual gameplay parity was intentionally left to the user.
