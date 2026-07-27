## Archived History

- Non-July task blocks from `boards\COMBAT\STATUS_EFFECT_BLACKBOARD.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/STATUS_EFFECT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/status history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current shared status runtime baseline and the resource-display rule still relevant to active work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-27 DamageAttribute Type Extraction

### Task title

Extract the shared `DamageAttribute` enum from `DamageCalculator.cs`.

### Goals

- Give the shared combat attribute type its own source file.
- Preserve every existing type reference and runtime meaning.

### Constraints

- Role Owner is Code Builder.
- Preserve `Pakuri.Combat.DamageAttribute`, all six member names, their order, and the existing public API.
- Do not change damage, status, skill, CSV, prefab, scene, or runtime behavior.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Keep future attributes in `DamageAttribute.cs`; no Play Mode verification is required for this behavior-preserving source move.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Damage/DamageAttribute.cs` is now the sole `DamageAttribute` declaration and keeps `namespace Pakuri.Combat`.
- `DamageCalculator.cs` retains its existing `DamageAttribute` parameter and no consuming script required a namespace or call-site change.
- Unity imported the new script and generated `DamageAttribute.cs.meta`; regenerated `Assembly-CSharp.csproj` includes the new source file.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and its Editor counterpart completed with 0 errors. Existing `MSB3277` warning groups and Editor `CS2008` empty-source warning remain.

### History

- 2026-07-27: User requested Code Builder to separate the shared enum and preserve consuming scripts. The enum was mechanically moved without changing its full type name.

## Task: 2026-07-25 Target-Attached Effect Visual Unification

### Task title

Unify status and timed target visuals through target-child attachment.

### Goals

- Create target-bound visuals through one `EffectManager.CreateTargetVisual` path.
- Attach status, following-skill, buff, shield, and support visuals as children of their targets.
- Remove `BuffSkillActor` per-frame position copying while preserving each effect's existing lifetime owner.
- Keep target-attached effects reachable by `EffectManager.ClearEffects`.

### Constraints

- Role Owner is Code Builder.
- Preserve status-instance refresh/removal, timed-duration removal, hitbox policy, authored visual/prefab selection, and object names.
- Do not change CSV, prefabs, scenes, status rules, damage, targeting, or duration values.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and Unity Editor validated. User Play Mode verification remains.

### Next Actions

- User verifies status and timed target visuals move with a moving target and stay under that target in the runtime hierarchy.
- User verifies status visuals disappear on status removal, timed visuals disappear at duration end, and combat reset clears both kinds.

### Evidence

- `EffectManager.CreateTargetVisual` creates through the general `CreateEffect`, parents the result with `Transform.SetParent(target, true)`, centers it on the target, and registers it for global cleanup.
- `ShowOrRefreshStatusEffect`, `ShowFollowingSkillEffects`, and the three direct Buff/Shield/Support creation paths now call `CreateTargetVisual`.
- Status visuals still use `statusEffectVisuals` and `includeHitbox: false`; following and Buff/Shield/Support visuals still use `BuffSkillActor` duration cleanup.
- `BuffSkillActor` no longer stores a target or offset and no longer copies target position in `Update`; it now manages duration only.
- `EffectManager.ClearEffects` removes registered target children as well as remaining runtime-root children. Destroyed target-child references are pruned when another target visual is created.
- `git diff --check` passed for the three changed scripts with line-ending notices only.
- Runtime and Editor C# builds completed with 0 errors. Existing `MSB3277` assembly-conflict warning groups and Editor `CS2008` empty-source warning remain.
- Unity 6000.3.14f1 completed script compilation and domain reload. All three scripts validated with 0 errors; `BuffSkillActor` reported one advisory to null-check `GetComponent`, and Unity Console returned 0 error entries.

### History

- 2026-07-25: Designer compared status and following-effect logic and identified shared creation/attachment with separate lifetime ownership.
- 2026-07-25: User selected Code Builder and requested target following be unified through target-child attachment.
- 2026-07-25: Code Builder added the shared path, migrated five callers, reduced `BuffSkillActor` to lifetime management, preserved global cleanup, and completed local and Unity Editor verification.

## Task: 2026-07-25 Combat Skills Implementation Comments

### Task title

Add concise implementation-responsibility comments across the current `Combat/Skills` runtime.

### Goals

- Mark the actual code points that store compiled SkillNode operations, assemble execution snapshots, route Definitions, resolve targets, and execute each skill family.
- Keep comments short and use the requested “what this implements” style.

### Constraints

- Role Owner is Code Builder.
- Change comments only; preserve gameplay behavior, APIs, CSV, prefabs, scenes, and existing uncommitted work.
- Unity Play Mode gameplay verification remains user-owned and is not required for comment-only changes.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- Keep these comments aligned when runtime ownership or skill-family execution boundaries change.

### Evidence

- Added 25 concise implementation comments across 22 scripts under `Pakuri/Assets/Scripts/Combat/Skills`.
- Covered `SkillNode`, Definition fields, learned-ID ownership, execution snapshot assembly, runtime rule evaluation, targeting, family Executors, Actors, Passive, and Trigger ownership.
- Existing uncommitted edits in 11 overlapping Skill files were inspected and preserved.
- `git diff --check -- Pakuri/Assets/Scripts/Combat/Skills` passed with line-ending notices only.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 `MSB3277` warning groups.
- Unity 6000.3.14f1 accepted a script compilation request; the Console error query returned 0 entries.

### History

- 2026-07-25: User switched to Code Builder and requested short implementation comments based on the preceding CSV-to-runtime skill explanation.

## Task: 2026-07-25 Remove Skill Inspector Limit Attributes

### Task title

Remove manually authored `Min` and `Range` Inspector limits while retaining CSV validation.

### Goals

- Remove all `[Min(...)]` and `[Range(...)]` attributes from scripts under `Pakuri/Assets/Scripts`.
- Preserve field declarations, default values, runtime guards, and `CsvDataValidator`.

### Constraints

- Role Owner is Code Builder.
- Do not change CSV schema, parsing, validation rules, runtime clamps, or gameplay behavior.
- Preserve existing uncommitted changes in `SkillDefinition.cs` and the wider worktree.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- Keep numeric authoring constraints in `CsvDataValidator` rather than reintroducing Inspector-only `Min` or `Range` attributes.

### Evidence

- Repository search initially found 38 `[Min(...)]` or `[Range(...)]` occurrences, all in `Pakuri/Assets/Scripts/Combat/Skills/Definitions/SkillDefinition.cs`.
- All 38 attributes were removed while their field declarations and default values were retained; a full Scripts search now returns 0 matching attributes.
- `Pakuri/Assets/Scripts/Data/CsvDataValidator.cs` remained unchanged and still checks negative cooldown values, non-positive active cooldowns, and status chances outside 0..1.
- `git diff --check` passed with line-ending notices only.
- `dotnet build Pakuri/Pakuri.sln --no-restore` passed with 0 errors and the existing 2 `MSB3277` warning groups.
- Unity 6000.3.14f1 completed script compilation and returned to idle; the `error CS` Console query returned 0 entries. One unrelated MCP transport `NetworkStream` disposal error remains in the Console.

### History

- 2026-07-25: User selected Code Builder and requested removal of manually authored `Min` and `Range` limits while retaining Validator behavior.
- 2026-07-25: Code Builder removed the 38 Inspector attributes, confirmed Validator preservation, and completed local and Unity compilation checks.

## Task: 2026-07-25 Timed Skill Effect Creation Overload

### Task title

Unify timed additional-effect creation under the `EffectManager.CreateEffect` API.

### Goals

- Remove the separate `ShowTimedSkillEffect` API.
- Preserve additional-effect visual selection, object naming, one-second display, and `SingleSkillActor` cleanup.
- Keep target-following effect creation separate.

### Constraints

- Role Owner is Code Builder.
- Do not change skill CSV, prefabs, scenes, damage, status application, hitbox behavior, or effect duration values.
- Preserve the user's existing `EffectManager` comment changes.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and Unity Editor validated.

### Next Actions

- User verifies Sein C Master 2 and Vega A Master 2 timed visuals still appear at the resolved effect center and disappear after one second.
- User verifies center-anchored status-effect paths remain visually unchanged when an authored visual exists.

### Evidence

- `EffectManager.CreateEffect(SkillEffectDefinition, Vector3, float)` now owns the former timed-effect object naming, runtime-visual/prefab selection, `SingleSkillActor.InitializeTimed(...)`, and created-object return.
- `ZoneSkillExecutor` and `StatusRules` contain the four migrated `CreateEffect(effect, center, 1f)` calls.
- Repository search found zero remaining `ShowTimedSkillEffect` references and four migrated timed-effect calls.
- `ShowFollowingSkillEffects` remains unchanged and continues using `BuffSkillActor` for target-following visuals.
- `git diff --check` passed for the three changed code files with line-ending notices only.
- Runtime and Editor C# builds passed with 0 errors; the existing two `MSB3277` warning groups and Editor empty-source `CS2008` warning remained.
- Unity 6000.3.14f1 completed a forced script compilation and domain reload; all three changed scripts validated with 0 warnings and 0 errors, and the Unity Console returned 0 error entries.

### History

- 2026-07-25: Designer inspected the timed and following effect responsibilities and proposed the dedicated `CreateEffect` overload.
- 2026-07-25: Code Builder moved the timed creation behavior, replaced four callers, preserved following-effect ownership, and completed local and Unity Editor verification.

## Task: 2026-07-23 Remove Duplicate Choice And BuffShield Status Fields

### Task title

Keep normalized Choice nodes and `ShieldStatus` as the status-condition and shield-refresh authorities.

### Goals

- Remove the flat `RequiredSourceStatus*` Choice round trip.
- Keep `RequiredSourceStatus` conversion and runtime checks through normalized nodes.
- Remove duplicate or unimplemented BuffShield refresh and reflection fields.

### Constraints

- Role Owner is Code Builder.
- Status CSV, status definitions, active graph rows, and status runtime behavior remain unchanged.
- No new status kind, handler, or gameplay behavior is added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, Unity-console verified, and Code Reviewer passed.

### Next Actions

- User verifies Vega source-status-gated Choices and shield refresh behavior in Unity Play Mode.

### Evidence

- `SkillChoiceDefinition`, `SkillChoiceRow`, catalog building, compilation, and the flat `SkillRequirement.MeetsSourceStatus` overload no longer contain the flat `RequiredSourceStatus*` path.
- `SkillDefinitionCompiler` still maps the `RequiredSourceStatus` handler to `SourceStatusRequirementOp`; `SkillExecutionRuleResolver` evaluates that operation from `NormalizedPlanNodes`.
- `SkillGraphParser.ValidateSkillNodeParamValue` continues validating `StatusId` parameters against `model.StatusEffects`.
- Active Choice graph CSV rows for Vega still author `RequiredSourceStatus` nodes; no CSV row changed.
- `BuffShieldSkillDefinition` retains `ShieldStatus`; refresh policy remains in the shared status runtime, while duplicate `RefreshRule` and unimplemented reflection fields and compiler assignments were removed.
- Solution build completed with 0 errors, and Unity console returned 0 error entries after forced script refresh.

### History

- 2026-07-23: Code Builder consolidated Choice source-status checks and BuffShield refresh ownership.
- 2026-07-23: Code Reviewer returned PASS with no status-runtime fix request.

## Task: 2026-07-17 OpeningCharge Buff Classification And Contact Freeze

### Task title

Route OpeningCharge as a Buff-owned active charge while preserving its contact freeze.

### Goals

- Treat the caster's charge movement increase as Buff authoring rather than SingleAttack authoring.
- Preserve the existing `freeze` status application for 5 seconds on the contacted target.
- Prevent the Trigger kind check from rejecting the specialized charge runtime data.

### Constraints

- Role Owner is Code Builder.
- Do not add a new status type or status CSV schema.
- Keep movement execution in `ChargeSkillExecutor`; the shared status runtime remains responsible only for the authored contact freeze.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Corrected implementation complete and compile-verified. User Play Mode verification remains.

### Next Actions

- Verify the caster movement increase begins from the CombatStart Buff Trigger.
- Verify only the contacted hostile receives the existing 5-second freeze application.

### Evidence

- `skills_buff.csv` owns OpeningCharge with `move_speed_multiplier=2.5`, `status_effect_id=freeze`, and `status_duration=5`.
- `SkillTriggerRuntime.MatchesRuntimeKind` now accepts `ChargeSkillData` under `SkillRuntimeKind.Buff`, allowing the specialized charge executor to start.
- `ChargeSkillExecutor` still calls the shared status applier after the target-max-health contact damage and then clears the active charge.
- No status schema, status definition, or new status runtime branch was added.
- Runtime and Editor C# builds passed with 0 errors; only the pre-existing 2 `MSB3277` warnings remained.

### History

- 2026-07-17: Code Builder applied the Buff/Status unification blueprint boundary: Buff owns the charge classification and movement authoring, while the existing shared status path owns freeze.

## Task: 2026-07-15 Next-Day Skill Effect Cleanup

### Task title

Clear completed-day runtime skill actors and applied combat effects without deleting skill metadata.

### Goals

- Treat `EffectManager.runtimeSkillRoot` as the ownership boundary for field-resident skill objects.
- Disable and destroy all root children before the next day begins.
- Clear unit statuses and transient shields, then refresh actor resource views.
- Reset full monster active-skill runtime state, not cooldown alone.

### Constraints

- Role Owner is Code Builder.
- Runtime cleanup does not alter skill definitions, learned active/passive IDs, choice records, or `RunSession` state.
- Existing effect prefab lookup tables remain intact.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies no old projectile, zone, beam, impact, status visual, or delayed skill hit survives a day transition.
- User verifies magazine, reload, queued burst, hit count, cast, active duration, tick timer, and cooldown begin from fresh runtime state.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` now exposes `ClearRuntimeSkillObjects()` and clears all children of the same root used by `InstantiateSkillPrefab()` and `CreateRuntimeSkillObject()`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` calls `StopAllCoroutines()`; inspected `StartCoroutine` call sites on this manager are skill projectile, repeated deployment, delayed hit, multi-effect, and trigger paths.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` `ResetRuntimeState()` clears cooldown, cast, active duration, tick, reload, magazine, queued burst shots, projectile count, hit count, and consecutive-hit state.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitRuntimeStateService.cs` invokes that full runtime reset while leaving model metadata untouched.
- Runtime C# build passed with 0 errors, Unity console reported 0 errors, and `NewRunScene` validation reported no missing scripts or broken prefabs.

### History

- 2026-07-15: Code Builder traced all runtime skill object creation to `EffectManager.runtimeSkillRoot` and added centralized cleanup.
- 2026-07-15: Code Builder added status, shield, passive-trigger, delayed-action, and full monster skill runtime reset at the next-day boundary.

## Task: 2026-07-13 Vega Effect Graph Composer Migration

### Task title

Preserve Vega status gates, runtime-kind filters, and conditional modifiers in positional Effect graphs.

### Goals

- Move Vega legacy status Effects to Skill/Choice/Trigger-owned positional graphs.
- Preserve H owner source-status gates, I incoming `Area` filters, and G conditional critical-resistance behavior.
- Reuse current status runtime meanings without adding Vega-only combat branches.

### Constraints

- Role Owner is Code Builder.
- Blueprint, prefab, scene, object/collider offset, and new gameplay behavior are outside scope.
- Existing status runtime fields remain authoritative; new node types only expose those fields through positional authoring.

### Role Owner

Code Builder

### Status

Implemented and Unity CSV/InGame validation passed; Play Mode status parity verification remains.

### Next Actions

- Verify Vega-H effects start and stop with live `slaughter-permit` ownership in Play Mode.
- Verify Vega-I debuffs apply only to incoming `Area` runtime kinds.
- Verify Vega-G trait 3 requires both `silence` and `name-mark` and applies critical resistance `-0.10`.

### Evidence

- `PakuriCsvRuntimeData.Build.cs` composes `RequiredSourceStatus`, `StatusRuntimeKindFilter`, and `StatusCriticalResistanceBonus` into existing `SkillEffectDefinition` fields.
- `ConditionStatusExpression` maps to the existing `ConditionStatus` handler and preserves `silence&name-mark` without a new condition evaluator.
- Vega Effect authoring is now 109 positional rows across 23 Effect graphs; legacy Vega Effect rows are 0.
- Vega Trigger graph references total 11 and resolve to existing Trigger-owned Effect graphs.
- Unity-MCP `Pakuri/Validate CSV Source Data` loaded the runtime catalog successfully; `Pakuri/InGame/Validate Skill Data` passed with 0 warnings.
- Runtime and Editor C# projects build with 0 errors.

### History

- 2026-07-13: Designer proposal identified missing positional exposure for source gates, runtime-kind filters, and critical resistance.
- 2026-07-13: Code Builder added shared schema/composer mapping, migrated Vega Effects, and removed the 23 legacy rows after automated validation.

## Task: 2026-07-13 Sein Hybrid Effect Graph Migration

### Task title

Preserve Sein damage-plus-status and persistent-zone payloads while removing legacy Effect rows.

### Goals

- Let one generated damage Effect retain its status payload without inventing a second gameplay Effect.
- Preserve existing persistent tick, status duration/stack, merge-policy, and passive ownership gates.

### Constraints

- Role Owner is Code Builder.
- `AttachStatusPayload` composes status fields onto the Effect's existing operation kind; it does not create a separate trigger skill.
- No new status gameplay meaning or prefab/scene behavior is introduced.

### Role Owner

Code Builder

### Status

Implemented and source/build validated; Play Mode persistent-status verification remains.

### Next Actions

- Verify Sein-C follow-up Effects and Sein-D/E persistent damage/status refresh behavior in Play Mode.

### Evidence

- `AttachStatusPayload` is registered as an Effect-only positional node with required `status_id` and optional chance/label/duration/stack/merge arguments.
- `PakuriCsvRuntimeData.Build.ApplyEffectOwnedSkillEffectNode(...)` applies the status payload while preserving the current Effect operation kind.
- `EffectDamage` positional params now expose the already-consumed attack-power coefficient and tick interval.
- Sein legacy Effect rows count 0 after 19 equivalent Effect graphs were authored; normalized graph validation reports 0 errors.
- Unity-MCP source validation loaded the runtime catalog without validation errors.

### History

- 2026-07-13: Code Builder added the hybrid status-payload composer path and migrated Sein-C/D/E/F legacy Effects to positional graphs.

## Task: 2026-07-12 Rin Status Effect Graph Exposure Design

### Task title

Preserve Rin status Effect behavior while migrating legacy Effect rows to graphs.

### Goals

- Expose existing move-speed, critical-damage, resistance-reduction, outgoing-additional-damage, health-ratio, and hit-count Effect meanings to positional graphs.
- Preserve passive-to-active Effect gates for Rin-J effects attached to Rin-E.

### Constraints

- Role Owner is Code Builder for the approved implementation phase.
- No new status gameplay meaning was introduced.
- Existing Trigger event envelopes remain intact; Play Mode verification is still required.

### Role Owner

Code Builder

### Status

Approved status Effect graph exposure and passive gate inference implemented; source/build validation completed.

### Next Actions

- Verify Rin-B/C/E/G/I/J status application, expiry, resistance reduction, outgoing additional damage, and passive gates in Play Mode.

### Evidence

- Current Rin legacy Effects use status move speed, attack power, critical chance/damage, physical damage bonus, physical resistance reduction, health-ratio, and OnHitCount fields already consumed by shared runtime.
- The current graph materializer leaves generated `RequiresPassiveSkillId` blank; the proposal requires passive Skill/Choice ownership to preserve this gate when targeting `rin-e`.
- Effect composition now maps the approved health-ratio, hit-count, move-speed, attack-power, critical-damage, resistance-reduction, and outgoing-additional-damage graph operations to existing runtime fields.
- Generated passive Skill/Choice Effects infer `RequiresPassiveSkillId`; Rin-I kill Effects use explicit Trigger-owned Effect graph references.
- Rin legacy Effect rows are zero after migration, and Unity source validation completed without validation errors.

### History

- 2026-07-12: Designer recorded the status-specific graph exposure and gate requirements in the Rin node proposal.
- 2026-07-12: Code Builder implemented the approved Effect composer operations and passive gate preservation, then validated the generated source catalog.

## Task: 2026-07-12 Eve Status Graph Migration

### Task title

Move Eve status-dependent skill behavior from wide/legacy rows into shared graph nodes.

### Goals

- Preserve shock, chill/freeze, vulnerable, status-duration, stack, resistance, and damage-taken behavior through common nodes.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- No monster-only status runtime branch is introduced.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and source-validated. Eve-B/E base runtime visuals no longer leak into status visuals; Play Mode verification remains.

### Next Actions

- User verifies Eve-D stack damage, Eve-E vulnerable stacks, and Eve F-J passive status modifiers in Play Mode.
- User verifies Eve-B slow and Eve-E vulnerable apply without spawning an extra `RuntimeStatusVisual` or status-owned collider.

### Evidence

- Eve-D uses `StatusFilteredDeployment`, `TargetStatusStackDamage`, and additive stack-rate nodes without consuming shock.
- Eve-E uses graph-authored vulnerable stack amount/max-stack/critical-damage-taken modifiers.
- Eve F-J legacy status effects were replaced by shared Effect graphs; `StatusElementDamageTakenBonus` now accepts an optional element attribute so Eve-I remains Lightning-specific.
- Unity CSV validation passed and both C# projects build with 0 errors.
- `StatusEffectRuntime.CreateStatusData(...)` now accepts a source runtime visual only when `RuntimeSkillVisualAnchor.StatusTarget` is explicit, instead of copying every base skill visual.
- Status-attached runtime visual creation passes `includeHitbox: false`, so status decorations cannot create gameplay colliders.
- `skills_single_attack.csv` opts only Ariel-D into `StatusTarget`; Eve-B/E keep the default `Skill` ownership.

### History

- 2026-07-12: Completed Eve status graph migration and verification handoff.
- 2026-07-12: Fixed Eve-B/E `RuntimeStatusVisual` leakage through shared status visual ownership and collider guards.

## Task: 2026-07-11 Shared DamageCalculator Final-Damage Routing

### Task title

Route all `InGameCombatManager` final-damage calculations through `DamageCalculator`.

### Goals

- Remove the duplicated non-critical defense formula from `InGameCombatManager`.
- Use `DamageCalculator` for both critical-enabled and critical-disabled damage.
- Preserve existing defense, status multiplier, rounding, shield-consumption, health, and combat-trigger behavior.

### Constraints

- Role Owner is Code Builder.
- The existing public attribute-based `DamageCalculator.Resolve(...)` signature remains available and keeps its critical-enabled behavior.
- Critical-disabled damage must not consume `UnityEngine.Random.value`.
- `InGameCombatManager` remains responsible for gathering runtime status inputs and applying the result to status shields, direct shields, health, views, and triggers.
- No MSW-MCP was used; Unity checks use Unity-MCP only.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Unity-MCP script validation was unavailable because no Unity Editor instance was connected.

### Next Actions

- User verifies in Play Mode that critical-disabled damage, failed critical rolls, successful critical rolls, defense reduction, incoming-damage modifiers, and shield absorption retain their previous results.
- If Unity Editor-side evidence is required, connect the project through Unity-MCP and validate the two changed scripts plus console state.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Combat/DamageCalculator.cs` now has a `criticalAllowed` overload; the existing overload delegates to it with criticals enabled.
- `DamageCalculator` gates its random roll with `criticalAllowed && UnityEngine.Random.value < criticalChance`, so disabled criticals do not consume combat RNG.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` removed `ResolveDamageAfterDefense(...)`; `ResolveFinalDamage(...)` now always calls `DamageCalculator.Resolve(...)` and passes defense reductions plus incoming-damage multipliers.
- Search for `ResolveDamageAfterDefense|DamageCalculator.Resolve(` across the two changed files returned only the unified `InGameCombatManager` call.
- `git diff --check` passed for both code files with only existing LF-to-CRLF normalization warnings.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and 2 existing `MSB3277` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and 2 existing `MSB3277` warnings.
- Unity-MCP `validate_script` for both changed scripts returned `No Unity Editor instances found`; no Unity-side validation result was available.

### History

- 2026-07-11: User requested Code Builder to make `InGameCombatManager` call `DamageCalculator` for actual damage regardless of whether criticals are allowed.
- 2026-07-11: Code Builder implemented the single calculation route, preserved the existing public overload and non-critical RNG behavior, and completed local build verification.

## Task: 2026-07-21 Damage Calculator File Unification

### Task title

Merge raw skill-value calculation and final target-damage calculation into `DamageCalculator.cs`.

### Goals

- Keep one damage calculation script under `Pakuri/Assets/Scripts/Combat/Damage`.
- Preserve the existing calculation order from caster power through target defense and incoming-damage modifiers.
- Remove the separate `SkillValueCalculator` type and all of its call sites.

### Constraints

- Role Owner is Code Builder.
- Do not add fallback helpers or ternary expressions while moving the calculation code.
- Preserve cast-time raw calculation and hit-time target/final calculation as separate methods.
- Preserve public damage APIs, serialized defense data, player-facing formulas, and unrelated user changes.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated. User Play Mode verification remains.

### Next Actions

- User verifies representative projectile, line, zone, single, trigger, shield, and healing skills in Play Mode.
- User verifies critical, defense reduction, outgoing-damage, conditional-target, and incoming-damage modifiers retain their previous results.

### Evidence

- `DamageCalculator.cs` now owns `ResolveDamage`, `ResolvePowerValue`, `ResolveShield`, `ResolveDamageAgainstTarget`, `ResolveStat`, and the existing final-damage methods.
- `SkillValueCalculator.cs` and its `.meta` were deleted; its former GUID had no asset reference outside its own `.meta`.
- Nine skill execution files now call `DamageCalculator`; repository search reports zero remaining `SkillValueCalculator` references.
- `Pakuri/Assets/Scripts/Combat/Damage` now contains only `DamageCalculator.cs` and its `.meta`.
- `DamageCalculator.cs` contains no question-mark expression; the moved stat lookup uses an explicit `if` branch.
- `git diff --check` passed with line-ending notices only.
- Unity script refresh regenerated the project source list, `DamageCalculator.cs` validation returned 0 warnings and 0 errors, and Unity Console error query returned 0 entries.
- Runtime and Editor `dotnet build --no-restore /p:UseSharedCompilation=false` each passed with 0 errors and the existing 2 `MSB3277` assembly-version warnings.

### History

- 2026-07-21: User switched to Code Builder, requested the two damage scripts be unified, and prohibited new fallback helpers and question-mark expressions in code changes.
- 2026-07-21: Code Builder moved raw-value methods into `DamageCalculator`, changed all callers, deleted the obsolete script and meta, refreshed Unity, and completed local verification.

## Task: 2026-07-22 Skill Effect Responsibility Split

### Task title

Delete the ambiguous `SkillEffect.cs` script and keep `SkillNode` as execution blueprint data.

### Goals

- Remove `SkillEffect.cs` without losing the effect behavior used by skill Executors and Actors.
- Keep `SkillNode.cs` limited to node kinds, stored values, and explicit node factories.
- Place shared status-data composition with the existing combat status rules.

### Constraints

- Role Owner is Code Builder.
- Do not add fallback helpers or question-mark conditional expressions.
- Preserve the existing skill effect results while changing ownership.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. User Play Mode verification remains.

### Next Actions

- Verify representative projectile, line, zone, single, buff, passive, and Trigger effects in Play Mode.
- Verify status chance, stack, duration, threshold, and hit-count effects retain their previous results.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillEffect.cs` and its meta were deleted; `Test-Path` returned `False`.
- `SkillNodeEffectExecutor.cs`, `SkillEffect.cs`, and their meta files are deleted; repository search found no remaining `SkillNodeEffectExecutor`, `SkillHitExecutor`, or old effect-script reference.
- Projectile, Line, Zone, Single, and Buff Executors now select the node effect timing and dispatch the effect kind themselves; Passive and Trigger own their corresponding effect dispatch.
- Projectile, Line, Zone, and Single Actors report hit or expiry events to their matching family Executor instead of a shared effect Executor.
- `UnitSkillData.CollectEffects(...)` reads the base effects and the unit's selected `SkillNode` effects; `SkillNode.cs` remains definition and named factory data only.
- Status composition and application live in `Combat/Status/StatusRules.cs`; effect target rules live in `SkillTargeting.cs`; effect visual creation lives in `EffectManager.cs`.
- Repository search found no added question-mark conditional expression in the changed scripts.
- `dotnet build Pakuri/Assembly-CSharp.csproj -v:minimal` passed with 0 errors and the existing 2 `MSB3277` warnings.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore -v:minimal` passed with 0 errors; it retained the 2 `MSB3277` warnings and the empty Editor source warning.
- `git diff --check` passed with line-ending notices only.

### History

- 2026-07-22: Initial Code Builder pass incorrectly replaced `SkillEffect.cs` with another central `SkillNodeEffectExecutor.cs`.
- 2026-07-22: Corrective Code Builder pass deleted the central executor, moved timing and hit enhancement execution to family Executors, and kept only target, status, and visual rules in their owning systems.

## Task: 2026-07-25 Remove Unused Effect Visual Branches And Hitbox Offset

### Task title

Remove the approved unused `EffectVisualBuilder` branches and the centered hitbox offset contract.

### Goals

- Simplify runtime effect component creation, scaling, and animation-duration inspection.
- Remove the unused hitbox offset from runtime definitions and collider construction.
- Preserve sprite, Animator, hitbox size, local-scale, visual-anchor, and impact-visual behavior.

### Constraints

- Role Owner is Code Builder.
- Do not add a Skill Graph scale Validator.
- Preserve unrelated user changes already present in the working tree.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated. User Play Mode verification remains.

### Next Actions

- User verifies representative sprite-only, Animator, line-width, area-radius, and hitbox effects in Play Mode.
- User verifies effect objects are newly created without preattached `SpriteRenderer`, `Animator`, or `BoxCollider2D` components.

### Evidence

- `EffectVisualBuilder.cs` now adds `SpriteRenderer`, `Animator`, and `BoxCollider2D` directly, always multiplies the resolved prefab scale, and no longer preserves negative scale signs.
- `EffectVisualBuilder.cs` no longer replaces non-positive visual scale with `1`, scans legacy `Animation` components, or assigns a collider offset.
- `RuntimeSkillHitboxSpec` now stores only `Size`; repository search found zero remaining `RuntimeHitboxOffset`, `runtime_hitbox_offset`, `hitbox.Offset`, or matching `Vector2` offset construction references.
- `CsvDataValidator.cs` has no working-tree change, and no Skill Graph scale Validator was added.
- `dotnet build Pakuri/Pakuri.sln --no-restore` passed with 0 errors and the existing 2 `MSB3277` assembly-version warnings.
- Unity 6000.3.14f1 completed a forced asset refresh and script compilation; Unity Console contained 0 errors.
- `Pakuri/Validate CSV Source Data` completed without `CsvFatalException` and loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.

### History

- 2026-07-25: User approved deletion of every previously identified candidate and explicitly prohibited adding a Graph Validator.
- 2026-07-25: Code Builder removed the branches and centered-hitbox offset contract, then completed static, solution-build, Unity compile, and CSV source validation.
