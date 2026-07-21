## Archived History

- Non-July task blocks from `boards\COMBAT\STATUS_EFFECT_BLACKBOARD.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/STATUS_EFFECT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/status history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current shared status runtime baseline and the resource-display rule still relevant to active work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

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

Implemented and compile-verified. User Play Mode verification remains.

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
