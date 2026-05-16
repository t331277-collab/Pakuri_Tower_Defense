## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-10` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.

## Task: 2026-05-17 Status Effect Enum Centralization And Label Display

### Task title

Centralize InGame status effects under `StatusEffectKind` and show active statuses on unit name labels.

### Goals

- Replace the runtime status key with a shared enum so skills and combat APIs do not own ad hoc string status logic.
- Keep existing CSV/string boundary fields compatible by parsing them into the enum.
- Show active status display names on each unit's `MonsterNameLabel`, for example `검사[감전]` and `검사[감전/취약]`.

### Constraints

- Role Owner is Code Builder.
- Existing string fields remain at CSV/serialized boundaries for compatibility.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies in NewRunScene Play Mode that Eve-A shock displays as `[감전]` and combined statuses display in slash-separated order.
- Later status additions should add enum definitions in `StatusEffectKind.cs` instead of adding new hardcoded runtime strings.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs:5` defines the shared `StatusEffectKind` enum, and `:89` centralizes id/display/duration/stack defaults through `StatusEffectUtility.GetDefinition(...)`.
- `StatusEffectKind.cs:120` builds the label suffix from active statuses.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:37` applies statuses by `StatusEffectKind`, `:70` returns whether ticking removed statuses, and `:159` stores `UnitStatusRuntime.Kind`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:159`, `:193`, and `:209` refresh unit actors after status apply/remove, and `:390` refreshes when status ticking changes the active set.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs:53` and `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs:53` append active status display suffixes to the unit name label.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle; console warning/error read showed only MCP client handler logs.
- `git diff --check` passed for the status-enum touched files; full worktree `git diff --check` is still blocked by unrelated trailing whitespace in `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:5269`.

### History

- 2026-05-17: User requested Code Builder to centralize status effects as an enum and append active statuses to `MonsterNameLabel`.
- 2026-05-17: User requested active status labels to include stack counts; `StatusEffectUtility.BuildDisplaySuffix(...)` now formats each active status as `DisplayName +Stacks`.

## Task: 2026-05-17 Eve-A Projectile Shock Application

### Task title

Apply Eve-A shock through the shared InGame projectile hit path.

### Goals

- Use the status runtime foundation from step 1 for the first visible Eve-A status application.
- Normalize the Eve-A shock tag from CSV/reference values into the shared `shock` runtime tag.
- Keep shock as a projectile hit effect rather than a separate Eve-only enemy state.

### Constraints

- Role Owner is Code Builder.
- This slice applies/refreshes status state only; shock damage/passive damage amplification hooks remain future work.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Implement Eve D/F/I passive and skill damage calculations against `HasStatus(..., "shock")` and `GetStatusStacks(..., "shock")` in later slices.
- User verifies in Play Mode that Eve-A hits can create shock stacks before those stacks are consumed by later skills.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:158` resolves projectile hit status specs and normalizes `shock`, `감전`, and the current mojibake CSV value into `shock`.
- `SkillExecutors.cs:177` sets Eve-A shock chance to 15% when the normalized tag is `shock`.
- `SkillExecutors.cs:194` builds `ProjectileStatusHitSpec` with stack, duration, max-stack, permanent, and refresh-duration fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs:164` through `:182` applies the status through `InGameCombatManager.ApplyStatus(...)`.
- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md:29` through `:30` define shock chance 15% and 1 stack.
- Runtime/editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-17: Step 2 connected Eve-A's first status application after the shared status runtime foundation was implemented.

## Task: 2026-05-17 InGame Unit Status Runtime Foundation

### Task title

Add the first shared InGame unit status runtime store.

### Goals

- Give `BaseUnitRuntimeModel` a shared status container for Eve shock, chill, freeze, vulnerable, slow, and later conditional passive checks.
- Expose combat-manager APIs that later skill executors can call to apply, query, stack, tick, and remove statuses.
- Keep this slice as a foundation only; do not connect Eve-A projectile hits or Eve B-E effect executors yet.

### Constraints

- Role Owner is Code Builder.
- No Unity Play Mode verification was run by Codex.
- This slice does not implement resistance modifiers, damage modifiers, visual status labels, Eve-A shock application, Eve B-E execution, or F-J passive hooks.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Connect Eve-A projectile hits to `InGameCombatManager.ApplyStatus(...)` for `shock` after status duration/chance ownership is confirmed.
- Extend damage/resistance calculation to query `HasStatus(...)` and `GetStatusStacks(...)` before implementing Eve F-J passive bonuses.
- User verifies Play Mode behavior only after the status store is connected to a visible skill effect.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` now has `UnitStatusRuntimeSet Statuses`.
- `BaseUnitRuntimeModel.cs` now defines `UnitStatusRuntimeSet` with `Apply(...)`, `Tick(...)`, `Has(...)`, `GetStacks(...)`, `Remove(...)`, and `Clear()`.
- `BaseUnitRuntimeModel.cs` now defines `UnitStatusRuntime` with normalized `Tag`, `Stacks`, `DurationRemaining`, `Permanent`, stack capping, duration setting, and expiry ticking.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now exposes `ApplyStatus(...)`, `HasStatus(...)`, `GetStatusStacks(...)`, and `RemoveStatus(...)`.
- `InGameCombatManager.Update()` now calls `TickUnitStatuses(Time.deltaTime)` after skill and enemy simulation ticks.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- The first parallel editor build hit the known `obj\Debug\Assembly-CSharp.dll` file-lock case; standalone `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP refresh returned idle; console warning/error read returned only MCP client handler logs.
- `git diff --check` on the changed scripts passed with only LF-to-CRLF normalization warnings.

### History

- 2026-05-17: User approved starting Eve A-J implementation from step 1, the common status runtime foundation.

## Task: 2026-05-15 InGame Rounded Resource Mutation Follow-up

### Task title

Round InGame HP/Shield mutation values and stabilize HP Fill positioning.

### Goals

- Ensure InGame damage results are whole-number HP/Shield values instead of fractional values.
- Make HP `Fill` shrink from left to right during actor refresh after HP changes without leaving the authored `Background` sprite bounds.
- Show the final rounded damage through the prefab `Damage` TextMesh in `N(Damage)` format.
- Preserve the current shield grant API path while rounding shield resources too.

### Constraints

- Role Owner is Code Builder.
- No timed shield/status expiry was implemented in this follow-up.
- No Unity Play Mode verification was run by Codex.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified by builds and inspection.

### Next Actions

- User verifies NewRunScene Play Mode HP/Shield display behavior after projectile hits and shield grants.
- Timed shield/status expiry remains a later InGame status-effect slice.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/UnitResourceMutationService.cs` now rounds defense-adjusted damage with `Mathf.Round(...)`.
- `UnitResourceMutationService.cs` now stores rounded current HP and shield values through `RoundResource(...)` in damage, grant shield, and set shield paths.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` now use left-anchored HP `Fill` positioning so HP decreases from left to right visually.
- `MonsterUnitActor.cs` and `EnemyUnitActor.cs` now calculate HP/Shield segment placement from actual local rendered sprite width, `sprite.bounds.size.x * localScale.x`, and convert the desired rendered segment width back into each target sprite scale.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now shows the rounded applied damage through Actor `ShowDamage(...)` calls after resource mutation.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` contains `InGameDamageTextPopup`, which displays damage as `N(Damage)`, animates the prefab `Damage` TextMesh up by `1f` local Y over `0.9f` seconds, and fades it out.
- Runtime/editor builds passed with 0 errors and existing assembly reference warnings; the first parallel runtime build failure was a known file-lock retry case and the standalone runtime build passed.
- Follow-up runtime/editor builds passed with 0 errors after the Damage Text popup integration; Unity-MCP console warning/error read showed no remaining C# compile errors.

### History

- 2026-05-15: User requested Code Builder to fix HP decrease as rounded values and correct HPBar `Fill` coordinate drift after damage.
- 2026-05-15: User clarified HP should shrink like a left-to-right slide and requested animated Damage Text feedback; Code Builder updated the Actor fill math and damage popup path.
- 2026-05-15: User reported the `Fill` still left the `BG` and requested `N(Damage)` text format; Code Builder changed segment math to use actual SpriteRenderer rendered width and changed popup text formatting.

## Task: 2026-05-15 Phase4-C-0 Shield Grant Visual Without Timed Expiry

### Task title

Record Ariel-B minimum shield grant and the remaining shield/status gap.

### Goals

- Connect Ariel-B to `InGameCombatManager.GrantShield(...)` through the new shield executor path.
- Show a temporary attached shield visual through `InGameAttachedSkillEffectActor`.
- Keep the current InGame HP/Shield bar resource refresh path as the visible shield presentation owner.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- Timed shield resource expiry is still not implemented in the InGame Phase4-C-0 path.
- Eve-A shock/status application is not implemented in this slice, even though Eve-A data carries status-related fields.

### Role Owner

Code Builder

### Status

Builder implementation completed for minimum shield grant/visual; timed status behavior remains pending.

### Next Actions

- Add a common timed resource/status effect layer before declaring Ariel-B shield duration complete.
- Connect Eve-A shock through a reusable status application path in a later Phase4-C subtask.
- User verifies visual shield grant in Play Mode when Ariel-B is learned and cast.

### Evidence

- `SkillExecutors.cs` now makes `ShieldSkillExecutor` call `context.CombatManager.GrantShield(...)`.
- `SkillExecutors.cs` also instantiates `ariel-b` shield visual prefabs and initializes `InGameAttachedSkillEffectActor`.
- `InGameAttachedSkillEffectActor.cs` destroys only the attached visual after its duration; it does not remove shield resources.
- `InGameCombatManager.cs` already exposes the `GrantShield(...)` resource mutation API used by the executor.
- Runtime/editor builds passed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-15: Phase4-C-0 connected Ariel-B minimum shield grant and visual effect while explicitly leaving timed shield/status expiration for a later slice.

## Task: 2026-05-15 InGame HP Shield Bar Segment Rule

### Task title

Record and implement the InGame same-bar HP and Shield representation rule.

### Goals

- Show current HP and current Shield as adjacent segments inside one `MonsterHpBar` background.
- Preserve the authored background scale, such as X scale `20`, as the total visible bar width.
- When HP is `100` and Shield is `100`, set HP fill width to `10` and Shield width to `10`.
- Keep Shield hidden when current Shield is `0`.

### Constraints

- Role Owner is Code Builder.
- This slice changes actor display math only; it does not implement shield skills, timed shield expiry, or skill-driven shield grants.
- Runtime skill tuning and Play Mode gameplay verification remain later tasks.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified by build and code inspection.

### Next Actions

- Later shield skill code should call the InGame resource mutation API, then rely on changed-unit actor refresh.
- User verifies the visual ratio in Play Mode after a skill or debug path grants Shield.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now calls `SetResourceFillSegments(currentHealth, currentShield, maxHealth)`.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` now calls the same segment-style method.
- Segment denominator is `Mathf.Max(maxHealth, currentHealth + currentShield)`.
- Segment widths use the authored `Background.localScale.x`; HP starts at the background left edge and Shield starts after the HP segment.
- Existing prefab inspection found `Background` X scale `20`, `Fill` X scale `20`, and `Shield` X scale `0` under both monster and enemy HP bars before the code change.
- Runtime and editor builds passed with 0 errors and existing warnings.

### History

- 2026-05-15: User specified that HP `100` plus Shield `100` should fill one HP bar background by showing HP fill scale `10` and Shield scale `10` when the background X scale is `20`.

## Task: 2026-05-15 InGame Skill Coordinate Tuning Rule

### Task title

Record the NewRunScene visible-map coordinate baseline for future skill radius and area tuning.

### Goals

- Use the current visible camera/map design baseline as X `0~31` and Y `0~17`.
- Treat the camera right edge as X `31` and the camera top edge as Y `17`.
- Require future skill numeric values from CSV, such as radius, area size, projectile distance, range, and targeting width, to be tuned in this actual map-coordinate scale.
- Keep real scene object positions from `Transform` as actual Unity/world coordinates and do not remap them through CSV scaling.

### Constraints

- Role Owner is Designer.
- No code, CSV, scene, prefab, or Play Mode changes in this task.
- This is a future implementation constraint for skill execution and tuning; it does not validate any current skill radius visually.
- Object positions read from actual `Transform` values remain authoritative for spawned units, spawn points, Nexus, and authored scene objects.

### Role Owner

Designer

### Status

Recorded as a future skill implementation constraint.

### Next Actions

- Future Code Builder skill implementation must interpret CSV numeric tuning values against the X `0~31`, Y `0~17` visible-map baseline unless a value is explicitly authored as a raw Transform/world coordinate.
- When implementing projectile, zone, shield, buff, or enemy attack ranges, document which CSV fields are map-scale tuning values and which values come from actual scene `Transform` reads.
- User should verify final perceived range/radius in Play Mode after skill implementation.

### Evidence

- User stated on 2026-05-15 that the current visible camera basis should be treated as X `0~31` and Y `0~17`.
- User stated that the camera far right should be considered `31`, and the bottom-to-top visible height should be considered `17`.
- User stated that actual object `Transform` reads are excluded from this conversion rule.
- User stated that CSV numeric values such as skill radius should be tuned to match actual map coordinates during skill implementation.

### History

- 2026-05-15: User provided the visible-map coordinate baseline and asked to record it on the board for future skill implementation.

## Task: 2026-05-13 Phase 3-H Status Effect Boundary Closeout

### Task title

Verify skill-effect lifecycle and status routing after Phase 3.

### Goals

- Confirm persistent skill-effect lifecycle ownership is behind `CombatRuntimeSkillEffectSimulation.cs`.
- Confirm status/effect hit dispatch remains explicit for Eve, Sein, Vega, and manifested sources.
- Keep common temporary effect migration deferred.

### Constraints

- Role Owner is Code Builder.
- Do not introduce `TemporaryEffectInstance`.
- Do not change shield/status formulas in this closeout.
- User performs Play Mode status/effect verification.

### Role Owner

Code Builder

### Status

Completed and locally validated.

### Next Actions

- User verifies Eve/Sein/Vega/manifested persistent effects in Play Mode if needed.
- Keep shared temporary-effect migration for the later planned phase.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:22` through `:25` routes persistent effects through `SkillEffectSimulationBoundary`.
- `CombatRuntimeSkillEffectSimulation.cs:51` through `:56` preserves beam versus radius shape checks.
- `CombatRuntimeSkillEffectSimulation.cs:58` through `:79` preserves Sein, Vega, manifested, then Eve fallback dispatch order.
- `CombatRuntimeSkillEffectSimulation.cs:81` through `:97` preserves Eve B slow and Eve C chill/freeze handling.
- `CombatRuntimeSkillEffectSimulation.cs:99` through `:101` routes expiry handling to `TryHandleSeinSkillEffectExpired(...)`.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder verified that Phase 3 status/effect ownership is complete without changing status formulas in Phase 3-H.

## Task: 2026-05-13 Phase 3-E Skill Effect Hit And Expiry Routing

### Task title

Separate skill-effect shape, damage dispatch, and expiry dispatch helpers.

### Goals

- Preserve status/effect outcomes while splitting the shared effect hit path into named helpers.
- Keep Eve B slow, Eve C chill/freeze, Sein residual spawn, Vega marks, and manifested effect damage formulas unchanged.
- Leave common reusable temporary effects for Phase 7.

### Constraints

- Role Owner is Code Builder.
- Do not introduce `TemporaryEffectInstance`.
- Do not migrate shield/status modifiers into a common layer.
- User performs Play Mode status/effect verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve/Sein/Vega/manifested effect behavior in Play Mode if needed.
- Continue Phase 3-F only after accepting this slice.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:51` through `:56` preserves beam versus radius shape checks.
- `CombatRuntimeSkillEffectSimulation.cs:58` through `:79` preserves effect dispatch order.
- `CombatRuntimeSkillEffectSimulation.cs:81` through `:97` preserves Eve B slow and Eve C chill/freeze status handling.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeSeinSkills.cs:1031` through `:1073` still owns Sein effect damage/status formulas.
- `CombatRuntimeSeinSkills.cs:1075` through `:1114` still owns residual effect spawn on expiry.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:772` through `:783` still owns Vega effect hit damage and name-mark behavior.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder implemented Phase 3-E skill-effect hit and expiry routing split without changing status/effect formulas.

## Task: 2026-05-13 Phase 3-D Skill Effect Lifecycle Boundary

### Task title

Move skill-effect duration and tick lifecycle behind a boundary.

### Goals

- Preserve status/effect behavior while moving the shared skill-effect lifecycle loop out of the Eve skill file.
- Keep status and damage helper callbacks unchanged.
- Leave common reusable temporary effects for Phase 7.

### Constraints

- Role Owner is Code Builder.
- Do not introduce `TemporaryEffectInstance`.
- Do not migrate shield/status modifiers into a common layer.
- User performs Play Mode status/effect verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Phase 3-E may separate effect shape checks, hit routing, and expiry routing.
- User verifies Eve/Sein/Vega/manifested effect behavior in Play Mode if needed.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:47` through `:53` preserves duration ticking, tick remaining decrement, `HitThisTick.Clear()`, effect tick callback, and tick interval reset.
- `CombatRuntimeSkillEffectSimulation.cs:61` through `:63` preserves expiry callback, object destruction, and list removal order.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1222`, `:1228`, and `:1234` still route effect damage through existing Sein, Vega, and manifested callbacks.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeSeinSkills.cs:1075` still owns skill-effect expiry handling.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder implemented Phase 3-D skill-effect lifecycle boundary without changing status/effect formulas.

## Task: 2026-05-13 Phase 3 Skill Effect Slice Plan

### Task title

Define skill-effect Phase 3 slices before implementation.

### Goals

- Move skill-effect lifecycle ownership behind a simulation boundary after projectile slices.
- Preserve current effect tick interval, expiry, residual spawn, shape hit checks, and damage routing.
- Keep reusable temporary-effect migration reserved for Phase 7.

### Constraints

- Role Owner is Designer.
- Do not change runtime C# behavior.
- Do not introduce `TemporaryEffectInstance` in Phase 3.
- Do not migrate shield/status modifiers into a common layer during this phase.

### Role Owner

Designer

### Status

Completed. Skill-effect work should occupy Phase 3-D and Phase 3-E.

### Next Actions

- Phase 3-D: move the shared skill-effect lifetime/tick loop behind a simulation boundary.
- Phase 3-E: separate shape checks, effect damage dispatch, and expiry-spawn routing while preserving Eve, Sein, Vega, and manifested behavior.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1230` owns `UpdateEveSkillEffects()` and `UpdatePersistentSkillEffects()` over the shared `skillEffects` list.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1233` through `:1283` owns `TickSkillEffect(...)`, which dispatches to Sein, Vega, manifested, and Eve effect damage/status behavior.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1428` owns beam shape checks for effects.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeSeinSkills.cs:1026` through `:1114` owns Sein effect detection, damage, and residual effect spawning on expiry.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:772` through `:813` owns Vega effect damage/name-mark behavior, and `:1523` through `:1533` owns Vega effect classification helpers.
- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:32` through `:35` already routes skill-effect registration through `AddBattlefieldSkillEffect(...)`.

### History

- 2026-05-13: Designer split Phase 3 skill-effect lifecycle work into 3-D and 3-E before Code Builder implementation.

## Task: 2026-05-13 Temporary Effect Reuse Roadmap Timing

### Task title

Record the timing for moving status, shield, and modifier effects into a common temporary-effect layer.

### Goals

- Clarify that temporary-effect reuse is a Phase 7 migration, not a Phase 3 implementation task.
- Start common temporary effects with a simple modifier such as action speed.
- Move shield next, then move/damage/status-chance modifiers, and leave complex statuses for later.

### Constraints

- Role Owner is Designer.
- Do not change runtime C# behavior.
- Preserve existing shield and status behavior until Code Builder receives a specific implementation task.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Phase 7-C: verify one common modifier effect.
- Phase 7-D: split shield grant / absorb APIs.
- Phase 7-E/F: migrate broader modifiers and complex statuses only after the simple effect path is proven.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` to order temporary-effect migration as action speed, shield, general modifiers, then complex status effects.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:330` through `:346` proposes `ApplyTemporaryEffect(...)`, `GrantShield(...)`, and shield subsystem separation.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:145` through `:186` currently ticks manifested shield and timed buffs locally.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:738` through `:748` currently ticks enemy move buff, freeze, slow, shock, and chill state directly.

### History

- 2026-05-13: User asked to amend the roadmap with temporary-effect reuse timing, including reusable shield/effect direction from the 2026-05-10 proposal.

## Task: 2026-05-13 Battlefield Facade Skill Effect Registration

### Task title

Route skill-effect registration through the Phase 1 battlefield facade.

### Goals

- Replace direct battlefield `skillEffects.Add(...)` registration writes with facade calls.
- Preserve existing effect timers, status application, visual duration, and tick behavior.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build and Unity-MCP console checks.

### Next Actions

- Future Phase 3 can move effect ticking/lifetime ownership behind the facade.
- Future Phase 7 can migrate transferable status/shield effects into common temporary-effect APIs.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:32` through `:35` adds `AddBattlefieldSkillEffect(...)`.
- `Select-String` after implementation found skill-effect registration calls routed through `AddBattlefieldSkillEffect(...)` in party and monster skill files.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.
- Unity-MCP console warning/error read after script import/refresh returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-13: Code Builder implemented Phase 1 battlefield facade boundary and routed skill-effect registration writes through it.

## Task: 2026-05-10 Ariel Selected Shield Expiry Status Fix

### Task title

Make selected-unit Ariel shield status expire outside selected-Ariel-only runtime.

### Goals

- Ensure shields on selected 1P from Manifested Ariel B/E are cleared when their timer reaches zero.
- Keep Archangel shield state and selected unit mirror fields synchronized with the cleared shield.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode status verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies selected 1P shield text/bar disappears after Manifested Ariel shield duration ends.

### Evidence

- Before the fix, `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:83` through `:88` tied `unitShieldTimer` decay to `UpdateArielSkillCooldowns()`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:518` now ticks selected-unit shield state from common selected combat.
- `CombatRuntimeArielSkills.cs:86` clears `unitShieldValue`, `arielArchangelShieldValue`, `arielArchangelShieldTimer`, and mirrored `selectedUnitRuntime` shield/Ariel fields on expiry.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:35` adds `ShieldAppliedFrame`; `:160` through `:163` skip shield timer decay on the frame the shield was applied.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:28` adds `unitShieldAppliedFrame`; `:95` through `:98` apply the same first-frame skip to selected 1P shields.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: User reported selected 1P kept Ariel shield after the Manifested Ariel shield duration should have ended.
- 2026-05-10: User then reported selected 1P shield duration appeared shorter than 2P-5P; Builder aligned first-frame timer decay semantics for selected and manifested shield statuses.

## Task: 2026-05-10 Ariel Unit Shield And Holy Exposure Runtime

### Task title

Carry Ariel shield, sanctuary, Archangel, and Holy Exposure behavior through unit source logic.

### Goals

- Store Ariel shield and timed buff state per `CombatUnitRuntime`.
- Make Ariel B/E shields protect selected plus manifested party units.
- Keep Ariel Holy Exposure and shield-dependent passive bonuses source-aware for manifested Ariel.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode status verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies selected Ariel shield skills protect teammates and Manifested Ariel applies Holy Exposure/status interactions in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:42` stores per-unit shield, shield source, blessing, sanctuary, Archangel shield, burst, and reflect state.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:808` applies Ariel team shields to selected and manifested units.
- `CombatRuntimeArielSkills.cs:869` handles manifested shield absorption, Archangel shield share reduction, reflect, and burst.
- `CombatRuntimeArielSkills.cs:1300` applies Ariel A master Holy Exposure from manifested projectile hits.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:464` through `:473` lets manifested units absorb shield before HP damage.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: Ariel unit executor migration corrected selected-only shield storage so Ariel shields can protect 2P-5P teammates.

## Task: 2026-05-10 Vega Unit Status And Passive Runtime

### Task title

Carry Manifested Vega mark, silence, vulnerability, and passive state through unit source logic.

### Goals

- Make Manifested Vega B apply silence/name marks from the source unit choices/passives.
- Make Manifested Vega C maintain unit-owned Extermination Permit buff state.
- Make Manifested Vega D/E apply I/J vulnerability and cooldown-charge behavior from the source unit.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode status verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP checks.

### Next Actions

- User verifies Manifested Vega name marks, silence, area vulnerability, survivor vulnerability, and cooldown-charge behavior in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:36` stores Manifested Vega C buff and D cooldown-charge state on the unit.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:445` implements unit B rectangle slash with silence/name-mark state.
- `CombatRuntimeVegaSkills.cs:507` implements unit C Extermination Permit timer and action/attack buff state.
- `CombatRuntimeVegaSkills.cs:548` and `:616` implement unit D/E status interactions.
- `CombatRuntimeVegaSkills.cs:1634` and `:1651` implement unit I/J vulnerability application.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-10: Vega unit executor migration moved the remaining Manifested Vega status/passive behavior from generic manifested approximations into Vega unit-owned logic.

## Task: 2026-05-10 Shield Status Timer And Eve F Application

### Task title

Centralize selected shield timer ownership and apply Eve F to lightning allies.

### Goals

- Keep selected-unit shield duration decremented by the shared shield timer path only.
- Preserve first-frame shield duration by using `ShieldAppliedFrame`.
- Apply Eve F battle-start shield to manifested allies that have lightning skills.

### Constraints

- Role Owner is Code Builder.
- This is status/runtime validation only; Play Mode verification remains user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies that selected Eve's Eve F shield lasts the intended 12 seconds and that manifested lightning allies receive it.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:518` calls `UpdateSelectedUnitShieldTimer(Time.deltaTime)` once per combat update for selected-unit shield timers.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:93` through `:108` no longer decrements `unitShieldTimer` inside Eve cooldown ticking.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1682` through `:1732` stamps `unitShieldAppliedFrame` / `ShieldAppliedFrame` and applies Eve F to manifested lightning-skill allies.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:160` through `:168` skips timer decrement on the frame a shield was applied.
- Runtime and Editor builds completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console error read returned only MCP client handler logs.

### History

- 2026-05-10: User reported selected Eve shield duration seemed too short and asked for a broader shield skill review.
