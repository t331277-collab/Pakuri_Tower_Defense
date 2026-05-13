# REFACTORING

This board records broad refactoring plans that cut across combat, monster runtime, data flow, UI, and reports.

When doing related work, follow `MDTREE.md` routing and update this file together with the affected domain boards.

## Task: 2026-05-13 Phase 3 Projectile Effect Drone Split Work Breakdown

### Task title

Define Phase 3-A through Phase 3-H before implementation.

### Goals

- Split Phase 3 `Projectile / Effect / Drone Simulation Split` into reviewable implementation slices.
- Preserve current projectile hit order, effect tick behavior, drone cadence, cleanup timing, and `CombatRuntimeController.Update()` order.
- Avoid starting Phase 6 skill reuse or Phase 7 common target / temporary effect migration during Phase 3.
- Provide Code Builder with a clear sequence before any runtime C# edit starts.

### Constraints

- Role Owner is Designer.
- Do not change runtime C# behavior in this planning task.
- Phase 3 must keep damage application APIs stable until Phase 7.
- Do not move selected-unit combat, enemy simulation, or common target ownership in Phase 3.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission.

### Role Owner

Designer

### Status

Completed as a pre-implementation work breakdown. Phase 3 should be split into 3-A through 3-H.

### Next Actions

- If implementation starts, begin with Phase 3-A only.
- After each slice, Code Builder should run runtime/editor builds, `git diff --check`, Unity-MCP refresh/console evidence where relevant, and update affected boards.
- Do not start Phase 3-D or later until Phase 3-A through 3-C have preserved projectile behavior.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:496` through `:506` still calls `UpdateProjectiles()`, `UpdateMonsterSkillRuntimeEffects()`, `UpdateManifestedMonsterPartyCombat()`, and `UpdateSelectedMonsterCombat()` in a fixed order.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` through `:154` currently owns projectile ticking, movement, selected/manifested/enemy hit routing, status application, pierce, edge cleanup, and projectile cleanup calls in one loop.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:649` through `:660` owns `CleanupProjectile(int index)`.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1230` owns `UpdateEveSkillEffects()` and the shared `skillEffects` lifetime/tick loop even though later checks dispatch to Sein, Vega, and manifested effect damage.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1233` through `:1283` owns `TickSkillEffect(...)`, including Eve, Sein, Vega, and manifested effect damage routing.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1286` through `:1342` owns selected Eve `drones` ticking and drone projectile fire.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:8` through `:115` owns a separate `ManifestedDroneRuntime` list and lifecycle, not the same `DroneRuntime` list used by selected Eve.
- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:7` through `:79` already provides Phase 1 registration facade methods for projectile, skill effect, and drone additions while the underlying lists still live on `CombatRuntimeController`.

### Phase 3 Slices

| Slice | Name | Scope | Keep out of scope | Acceptance criteria |
| --- | --- | --- | --- | --- |
| 3-A | Projectile Simulation Boundary Shell | Add a narrow projectile simulation boundary and route the existing `UpdateProjectiles()` entry through it while keeping current helper calls and update order. | Do not change damage formulas, hit logic, pierce logic, or projectile creation sites. | `CombatRuntimeController.Update()` still reaches projectile ticking at the same point, and behavior is a wrapper-level move only. |
| 3-B | Projectile Cleanup / Lifetime Ownership | Move projectile cleanup and lifetime/edge-removal responsibility behind the projectile boundary. | Do not split selected/manifested/enemy damage resolution yet. | Cleanup remains reverse-iteration safe; destroyed GameObjects and `projectiles.RemoveAt(i)` behavior are preserved. |
| 3-C | Projectile Hit Routing Helpers | Split the projectile loop into source-specific handlers for enemy projectile, manifested projectile, and selected/player projectile paths. | Do not move `ApplyDamageToEnemy`, `ApplyDamageToSelectedMonster`, `ApplyDamageToManifestedUnit`, or common target state. | Hit order, status labels, pierce consumption, Ariel explosion trigger, Sein/Vega/Rin hooks, and status application remain in the same observable order. |
| 3-D | Skill Effect Simulation Boundary Shell | Move the `skillEffects` lifetime/tick loop behind an effect simulation boundary while keeping existing damage/status helper callbacks. | Do not introduce `TemporaryEffectInstance`; that is Phase 7. | Effect duration, tick interval, `HitThisTick.Clear()`, expiry handling, and removal timing are preserved. |
| 3-E | Skill Effect Hit / Expiry Routing | Separate effect shape checks, hit routing, and expiry-spawn routing enough that Eve/Sein/Vega/Manifested effect handlers are readable boundaries. | Do not migrate shield/status modifiers into common temporary effects. | Eve B/C, Sein residual spawn, Vega line/area effects, and manifested effect damage still dispatch through existing formulas. |
| 3-F | Selected Drone Simulation Boundary | Move selected Eve `drones` ticking and `FireDroneProjectile(...)` behind a drone simulation boundary. | Do not merge manifested drones yet and do not redesign drone skills. | Drone duration, attack period, nearest-target lookup, projectile creation, and cleanup remain unchanged. |
| 3-G | Manifested Drone Simulation Alignment | Give `ManifestedDroneRuntime` ticking a comparable simulation boundary or adapter while preserving its party-source runtime and list ownership. | Do not force `DroneRuntime` and `ManifestedDroneRuntime` into one class unless implementation evidence shows it is a safe no-behavior slice. | Manifested Eve drone deployment, cooldown, target lookup, projectile fire, and cleanup stay behaviorally identical. |
| 3-H | Phase 3 Closeout / Ownership Verification | Verify projectile/effect/drone lifecycle owners are readable, board state is updated, and Phase 4 Enemy Simulation can start. | Do not perform Phase 4 enemy split in the same slice. | Builds pass, `git diff --check` passes, Unity-MCP console has no C# compile errors, and boards identify Phase 4 as the next default phase. |

### History

- 2026-05-13: User explicitly asked Designer to decide how far to split Phase 3 into `3-[A-Z]` before work starts and record it on the boards.

## Task: 2026-05-13 Roadmap Amendment For Shared Target, Temporary Effect, And Skill Reuse

### Task title

Add explicit timing for shared target, reusable temporary effects, same-type skill reuse, and Monster / Enemy actor commonization.

### Goals

- Reconcile the 2026-05-10 shared combat target / temporary effect proposal with the post-Phase-2-E roadmap.
- State that same-type skill reuse is a Phase 6 decision after Phase 3 lifecycle and adapter boundaries.
- State that common target / temporary effect migration belongs in Phase 7.
- State that Monster / Enemy inheritance or prefab-based common authoring belongs after Phase 7 stabilization, primarily as a Phase 8 view/component decision.

### Constraints

- Role Owner is Designer.
- Do not change runtime C# behavior.
- Preserve the current Phase 3 next-step recommendation.
- Keep prefab authoring separate from combat-state ownership until target/effect APIs are proven.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Start implementation with Phase 3 `Projectile / Effect / Drone Simulation Split` unless the user asks for another design-only pass.
- During Phase 6, decide the exact same-type grouping scope with the user: projectile, beam / line / area / field, summon / drone, and any exceptions.
- During Phase 7, introduce read adapters before moving state ownership into `CombatTargetModel`.
- During Phase 8, evaluate prefab/view commonization only after Phase 7 target/effect behavior is stable.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` with section `7. 2026-05-10 공통 대상 / 임시효과 제안 반영 여부`.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:401` through `:402` says selected ally, summoned ally, and enemy combat state must first be readable from one layer before common temporary effects can be safely reused.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:53`, `:68`, `:83`, `:98`, and `:113` still route monster action-speed reads through controller methods, so broad skill reuse should wait for adapter narrowing.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` defines `EnemyRuntime` as private nested class, while `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:8` is a `MonoBehaviour`; this supports adapter/model-first commonization before base-class inheritance.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:354`, `:419`, and `:517` show enemy scene object composition with `AddComponent`, so prefab authoring should be evaluated as a view/component phase rather than as the first combat model step.

### History

- 2026-05-13: User requested an amendment explaining whether the 2026-05-10 shared combat target / temporary effect proposal is included in the roadmap and where to place same-type skill reuse, common parent/prefab creation, and temporary-effect reuse.

## Task: 2026-05-13 Phase 2 Manifested Party Closeout Verification

### Task title

Verify whether Phase 2 `Manifested Party` should close after Phase 2-E.

### Goals

- Inspect the remaining Phase 2 candidate code in `CombatRuntimeParty.cs`.
- Decide whether a small independent Phase 2-F formula or field-effect slice exists.
- Confirm the current code still compiles after the Phase 2-E state.
- Record whether the next default implementation phase should be Phase 3.

### Constraints

- Role Owner is Code Builder.
- Do not change runtime C# behavior for this verification task.
- Preserve current player-facing combat behavior.
- Do not run Unity Play Mode; user owns gameplay verification.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Completed. Phase 2 is closable; no small independent Phase 2-F slice was identified from inspected code.

### Next Actions

- Start Phase 3 `Projectile / Effect / Drone Simulation Split` as the next default implementation phase.
- Keep Rin shockwave, generic/persistent field routing, Eve frost field, and Vega queued projectile call-site behavior in place until Phase 3 or Phase 6 provides a better owner boundary.
- Run Code Reviewer only if the user explicitly requests another review pass.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:351` through `:443` contains `TryFireManifestedRinShockwave(...)`, which combines Rin-specific choice checks, line effect creation, direct enemy iteration, Rin damage hooks, knockback, slow, and reload reduction; it is not a small generic helper slice.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:469` through `:508` contains generic field routing and `TryFireManifestedPersistentSkill(...)`, which remains directly called by `CombatRuntimeManifestedPartyDamage.cs:22`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:512` through `:564` contains `CreateManifestedEveFrostField(...)`, which combines Eve-specific radius/duration/tick/chill/freeze choices and effect registration; it should wait for Phase 3 effect simulation or later skill-adapter work rather than becoming a standalone Phase 2-F slice.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:701` through `:744` contains Vega queued projectile setup/ticking, and `CombatRuntimeManifestedPartySkills.cs:82` and `:115` still call `UpdateManifestedQueuedProjectiles(...)` and `QueueManifestedVegaThreeSwordFlurry(...)`; this is tied to `CombatSkillRuntime` magazine ticking rather than an independent closeout slice.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs:9` through `:22` already owns the generic manifested skill fire entry point and delegates the remaining special cases before generic damage.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing `System.Net.Http` / `System.IO.Compression` assembly reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings.
- Unity-MCP `refresh_unity` returned `success=true`, `refresh_triggered=false`, `resulting_state=idle`.
- Unity-MCP console warning/error read returned only MCP-FOR-UNITY client handler disposed/exited logs, not C# compile errors.

### History

- 2026-05-13: User explicitly assigned Code Builder and asked to perform the Phase 2 closeout verification.

## Task: 2026-05-13 Combat Runtime Refactor Roadmap After Phase 2-E

### Task title

Create the post-Phase-2-E combat runtime refactor roadmap.

### Goals

- Confirm Phase 2 closeout criteria after the Phase 2-E manifested party damage/projectile helper split.
- Record the remaining full roadmap from Phase 3 through Phase 7 and the later common actor/base-class decision point.
- Place skill reuse refactoring at the correct timing relative to projectile/effect/drone simulation and monster runtime adapter narrowing.
- Place common combat target and Monster/Enemy common base proposals at the correct timing relative to `ICombatTarget`, `TemporaryEffect`, and state-owner stabilization.

### Constraints

- Role Owner is Designer for this roadmap/report task.
- Preserve the existing implementation order unless inspected code provides a stronger reason to change it.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode.
- Code Reviewer execution requires explicit user permission and is not part of this report task.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Treat Phase 2 as closable unless Code Builder identifies a small independent remaining manifested formula/field-effect slice in inspected code.
- Prepare Phase 3 `Projectile / Effect / Drone Simulation Split` as the next default implementation phase.
- Delay broad skill-executor reuse until Phase 6 adapter narrowing, using Phase 3 to prepare projectile/effect/drone lifecycle boundaries.
- Delay common combat target migration until Phase 7 and delay common base-class inheritance until after `ICombatTarget` / temporary-effect behavior is verified.

### Evidence

- Added `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html`.
- `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html:319` through `:321` says Phase 2 should only continue if a smaller formula/field-effect slice is identified.
- `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html:324` through `:345` records Phase 3 projectile/effect/drone, Phase 4 enemy, Phase 5 selected-unit, Phase 6 adapter narrowing, and Phase 7 combat target / temporary-effect migration.
- `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html:546` through `:550` proposes Manifested Party, Projectile / Effect / Drone, Enemy Simulation, Selected Unit Combat, then monster runtime adapter narrowing.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:401` through `:402` says common target and reusable temporary effects are needed, but first selected ally, summoned ally, and enemy combat state must be readable from one layer.
- `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html:451` through `:453` says common base-class inheritance should come after `ICombatTarget` / adapter and effect-layer stabilization.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:307` through `:310` still owns battlefield lists and `:498` through `:503` still owns top-level update calls.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` still owns `UpdateProjectiles()` and `:516` still owns selected-unit combat.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:306`, `:706`, and `:945` still own enemy spawn/update/target priority.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:29` still exposes a full `CombatRuntimeController` reference.
- Code search confirmed `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:8` is a `MonoBehaviour` and `:193` still calls back through `Owner.TickManifestedUnitSkill(...)`.

### History

- 2026-05-13: User requested a detailed evidence-based HTML roadmap from Phase 2 closure verification through the remaining refactor phases, including the timing for skill reuse and common combat model proposals.

## Task: 2026-05-13 CombatRuntimeController Structure Split Plan

### Task title

Plan the `CombatRuntimeController` structure split refactor from the 2026-05-10 proposal.

### Goals

- Turn the 2026-05-10 `CombatRuntimeController` structure proposal into an executable work order.
- Reduce the God Class / shared-state pressure around `CombatRuntimeController`.
- Split work in a sequence that preserves current combat behavior.
- Make later skill reuse, common target modeling, temporary effects, and Monster/Enemy objectification possible without creating two sources of truth.
- Define which board files must be updated as each implementation slice lands.

### Constraints

- Role Owner is Designer for this planning task.
- Code Builder must not start implementation from this board alone; each slice still needs explicit implementation work.
- Preserve current player-facing combat behavior unless a later task explicitly approves behavior change.
- Do not run Unity Play Mode as Codex verification. User owns gameplay verification.
- Code Reviewer execution requires explicit user permission.
- Do not introduce runtime combat singletons for battle state. The inspected proposal says singletons are valid for global definitions/context, not mutable battle state.

### Role Owner

Designer

### Status

Phase 1 battlefield facade boundary implemented. Phase 2 manifested party runtime, view binder, skill dispatcher, drone lifecycle, skill visual helper, and damage/projectile helper boundaries are implemented. User-requested Phase 2 Code Reviewer completed with `REVIEW_RESULT: PASS`.

### Next Actions

- Continue Phase 2 only if a smaller remaining manifested party formula or field-effect slice is identified by inspected code; otherwise prepare Phase 3 projectile/effect/drone simulation split.
- Do not start by extracting every monster skill executor or by moving all HP/shield/status state into `CombatTargetModel`.
- After each implementation slice, run runtime/editor builds and update this board plus the affected combat/projectile/status/monster/report board files.

### Evidence

- `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html:543` through `:550` proposes the order: state ownership document, Manifested Party, Projectile / Effect / Drone, Enemy Simulation, Selected Unit Combat, then narrower monster runtime adapter interfaces.
- `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html:734` states that state ownership separation is more important than file splitting.
- `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html:736` states the first boundaries to handle are `Manifested Party`, `Projectile / Effect / Drone`, and `Enemy Simulation`.
- Current inspected `CombatRuntimeController` partial files total 14 files, 14,022 lines, and 668,782 characters.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:307` through `:310` owns `enemies`, `projectiles`, `skillEffects`, and `drones`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:357` and `:369` own selected-monster and selected-unit runtime state.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:481` through `:505` runs the main update orchestration.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:557` owns the current manifested party combat update entry point.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:14` owns projectile update and `:516` owns selected monster combat update in the current partial layout.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeEnemies.cs:706` owns enemy update in the current partial layout.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatMonsterSkillRuntime.cs:29` shows monster skill runtime adapters still hold a full `CombatRuntimeController` reference.
- `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html` adds the later validation that common base-class inheritance for Monster/Enemy should come after `ICombatTarget` / adapter and effect-layer stabilization.
- `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md` records the Phase 0 state ownership table and declares proposed owners for Phase 1 through Phase 7.
- `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs:7` through `:79` adds `CombatBattlefieldState` and facade add methods for enemies, projectiles, skill effects, and drones.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:8` through `:12` adds the `manifestedParty` service and routes existing `manifestedMonsters`, `manifestedDrones`, and `manifestedMonsterSlots` accessors through it.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:42` through `:60` owns the manifested party combat tick loop and separates unit skill sync, combat ticking, and view refresh calls.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:553` through `:583` now delegates `UpdateManifestedMonsterPartyCombat()` to `manifestedParty.TickCombat(...)` and splits unit validity, skill sync, combat tick, and view refresh into separate helper methods.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs:7` through `:19` owns `ManifestedMonsterStatusViews`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs:23` through `:52` resolves scene-authored manifested name, HP, HP-bar, and shield-bar children.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs:55` through `:141` owns fallback and live HP/shield bar repair helpers.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs:256` through `:302` owns manifested label and HP/shield bar refresh.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:194`, `:197`, `:224`, `:300`, `:334`, and `:1851` now call the view binder helpers from the party/runtime flow.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs:5` keeps the existing `CombatUnitRuntime` callback name as a compatibility wrapper.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs:10` through `:71` owns manifested unit skill dispatch order, preserving Eve, Rin, Sein, Vega, Ariel, then generic fallback.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs:74` through `:139` owns fallback manifested skill ticking, queued projectile ticking, reload ticking, and magazine fire dispatch.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs:64` through `:71` routes per-skill dispatch through `ManifestedPartyRuntime.TickUnitSkill(...)`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:362` now starts at `FireManifestedMonsterSkill(...)`, so the dispatcher block was moved out while damage/formula and scene-object firing methods remain in the existing party partial.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:193` still calls `Owner.TickManifestedUnitSkill(...)`, preserving the current public callback path for this slice.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:8` through `:16` now owns `ManifestedDroneRuntime`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:19` through `:48` now owns manifested Eve drone deployment and registration into `manifestedDrones`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:51` through `:92` now owns manifested drone ticking, target lookup, and projectile firing.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:95` through `:115` now owns manifested drone cleanup.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:16` now starts at `ManifestedMonsterSlotNames`, so the drone runtime type moved out of the party partial.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:1586` still calls `RemoveManifestedDroneAt(i)` from party clear, preserving cleanup behavior.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyVisuals.cs:9` now owns manifested non-drone skill visual dispatch.
- `CombatRuntimeManifestedPartyVisuals.cs:60` now owns manifested skill visual duration resolution while preserving existing monster-specific skill ID durations.
- `CombatRuntimeManifestedPartyVisuals.cs:120`, `:132`, and `:154` now own manifested circle visual creation, line visual creation, and shared visual configuration.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:371`, `:401`, and `:757` remain call sites for the moved visual helpers, preserving skill-fire behavior while reducing the party partial surface.
- `Pakuri/Assembly-CSharp.csproj:85` includes `Assets\Scripts\Combat\Manager\CombatRuntimeManifestedPartyVisuals.cs`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs:9` now owns generic manifested non-projectile skill fire.
- `CombatRuntimeManifestedPartyDamage.cs:63`, `:81`, `:112`, and `:124` now own manifested projectile fire entry points, pierce resolution, and projectile object/runtime creation.
- `CombatRuntimeManifestedPartyDamage.cs:184`, `:236`, `:258`, and `:289` now own manifested projectile hit resolution, source follow-up effects, area follow-up damage, and projectile status application.
- `CombatRuntimeManifestedPartyDamage.cs:311`, `:316`, `:335`, `:368`, `:451`, `:458`, `:465`, `:476`, and `:483` now own generic manifested skill damage, effect damage, base damage, damage multiplier, projectile speed, projectile lifetime, projectile hit radius, and status chance helpers.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:351`, `:490`, `:512`, and `:739` now retain only the monster-specific Rin shockwave, persistent field, Eve frost field, and queued Vega projectile call sites in the party partial.
- `Pakuri/Assembly-CSharp.csproj:81` includes `Assets\Scripts\Combat\Manager\CombatRuntimeManifestedPartyDamage.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-E damage/projectile helper slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-E damage/projectile helper slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs` as a `UnityEditor.MonoScript` with guid `87b85aa0eb1d47849e4ae88329a740ef`; after forced script refresh, editor state returned `ready_for_tools=true`, and console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `git diff --check` completed with no whitespace errors and LF-to-CRLF warnings only after the Phase 2-E damage/projectile helper slice.
- External Phase 2 Code Reviewer output was saved to `codex_loop_logs\phase2_manifested_party_reviewer_20260513.md` and ended with `REVIEW_RESULT: PASS`.
- Reviewer evidence found no missing referenced helper, duplicate method definition, new null-risk regression, or behavior-order regression in the moved Phase 2 code.
- `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html` records the Phase 2-E alignment check against section 9 of `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- The Phase 2-E alignment report concludes that Phase 2 follows the proposal direction for state ownership, Manifested Party first, and no runtime-state singleton migration, while Phase 3 `Projectile / Effect / Drone Simulation Split`, Phase 4 `Enemy Simulation Split`, Phase 5 `Selected Unit Combat Split`, Phase 6 adapter narrowing, and Phase 7 common target/effect migration remain.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-D visual helper slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-D visual helper slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyVisuals.cs` as a `UnityEditor.MonoScript` with guid `6f30f996b52c4bb4ea62492d3b619c4c`; after forced script refresh, console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-C drone lifecycle slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-C drone lifecycle slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs` as a `UnityEditor.MonoScript`; after forced script refresh, editor state returned `ready_for_tools=true`, and console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-B skill dispatcher slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-B skill dispatcher slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs` as a `UnityEditor.MonoScript`; after forced script refresh, editor state returned `ready_for_tools=true`, and console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2-A view binder slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2-A view binder slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs` as a `UnityEditor.MonoScript`; after forced script refresh, console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after the Phase 2 service slice.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing assembly reference warnings after the Phase 2 service slice.
- Unity-MCP imported `Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs` as a `UnityEditor.MonoScript`; after forced script refresh, console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.
- `Select-String` after implementation found 52 `AddBattlefield*` call sites and no remaining raw `enemies.Add`, `projectiles.Add`, `skillEffects.Add`, or `drones.Add` calls for battlefield lists; remaining `.Add` matches were internal hit sets and `manifestedDrones`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and 2 existing assembly reference warnings after a first parallel run hit an `obj\Debug\Assembly-CSharp.dll` file lock.
- Unity-MCP imported `Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs` as a `UnityEditor.MonoScript`; after forced script refresh, console warning/error read returned only MCP-FOR-UNITY client handler logs, not C# compile errors.

### History

- 2026-05-13: User asked to write how the `CombatRuntimeController` structure split refactor should proceed, based on `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`, into `boards/REFACTORING/REFACTORING.md`.
- 2026-05-13: User asked to start Phase 0, `State Ownership Map`; Designer created `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md` without runtime C# changes.
- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 1 `Battlefield Facade Boundary`; Builder added the battlefield facade partial under `Assets/Scripts/Combat/Battlefield` and routed battlefield list registration through facade methods.
- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 2 `Manifested Party Runtime Split`; Builder added a manifested party runtime service boundary while preserving existing selected/manifested combat behavior paths.
- 2026-05-13: User explicitly assigned Code Builder and asked to perform Phase 2-A `Manifested Party View Binder`; Builder moved manifested party status view binding and label/bar refresh helpers into a new partial script.
- 2026-05-13: User explicitly assigned Code Builder and asked to perform Phase 2-B `Manifested Party Skill Dispatcher`; Builder moved manifested skill dispatch and fallback cooldown/magazine ticking into a new partial script behind the manifested party runtime service.
- 2026-05-13: User explicitly assigned Code Builder and asked to start the remaining Phase 2 work; Builder moved manifested Eve drone runtime, deployment, ticking, and cleanup into a new partial script as Phase 2-C.
- 2026-05-13: User explicitly assigned Code Builder and asked to start Phase 2-D `Manifested Party Skill Visual / Scene Object Helper`; Builder moved manifested non-drone skill visual duration and visual object helper methods into a new partial script.
- 2026-05-13: User explicitly assigned Code Builder and asked to perform Phase 2-E `Manifested Party Damage / Projectile Fire Helper` and run Code Reviewer for Phase 2; Builder moved generic manifested damage/projectile helper methods into a new partial script before Reviewer execution.
- 2026-05-13: External Code Reviewer returned `REVIEW_RESULT: PASS` for the Phase 2 manifested party refactor.
- 2026-05-13: User asked for a Phase 2-E status report comparing the current refactor with the 2026-05-10 proposal's section 9 recommendations; Designer added `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html`.

## Current Refactor Order

### Phase 0. State Ownership Map

Status:

- Completed as `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md`.

Purpose:

- Document which system owns each mutable combat state before moving code.
- Prevent implementation from only moving methods while keeping hidden shared state.

Work:

- Map controller-owned battlefield lists: `enemies`, `projectiles`, `skillEffects`, `drones`.
- Map selected unit state: selected monster definition, selected unit runtime, HP/shield, magazine/reload, projectile config, status UI state.
- Map manifested party state: party slots, `CombatUnitRuntime` list, HP/shield bars, learned active skills, manifested skill runtimes.
- Map enemy simulation state: spawn state, movement, targeting, enemy HP/shield/status/buff fields.
- Map skill executor dependencies: monster skill files that directly access battlefield lists, enemy state, selected unit state, and manifested party state.

Acceptance criteria:

- A state ownership table exists before code extraction starts.
- Every field moved in later phases has a declared old owner and new owner.
- No behavior changes are made in this phase.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/REPORT/REPORT_BLACKBOARD.md`

### Phase 1. Battlefield Facade Boundary

Status:

- Implemented as `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs`.
- Battlefield registration writes now route through `AddBattlefieldEnemy`, `AddBattlefieldProjectile`, `AddBattlefieldSkillEffect`, and `AddBattlefieldDrone`.
- Update loops, cleanup loops, target queries, runtime nested classes, and list storage remain in their existing files for behavior preservation.

Purpose:

- Create the first safe boundary around shared battlefield lists before splitting larger systems.
- Make skill executors request battlefield actions rather than directly mutating lists.

Work:

- Introduce a small battlefield facade or internal service around `enemies`, `projectiles`, `skillEffects`, and `drones`.
- Start with one narrow path, such as manifested visual/projectile spawn or one Eve projectile/effect spawn path.
- Keep the underlying lists in place during the first slice; change access path, not behavior.
- Add request-style methods for spawn/find operations before moving the full simulation loop.

Acceptance criteria:

- One selected spawn path no longer calls the raw list directly.
- Existing update order in `CombatRuntimeController.Update()` remains unchanged.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/COMBAT/PROJECTILE_BLACKBOARD.md`
- Relevant monster board if a monster skill file changes.

### Phase 2. Manifested Party Runtime Split

Status:

- Started as `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs`.
- The first slice moves manifested party list/slot storage and top-level party tick orchestration behind a service boundary.
- Phase 2-A moves manifested party status view binding and label/bar refresh helpers into `CombatRuntimeManifestedPartyView.cs`.
- Phase 2-B moves manifested party skill dispatch and fallback cooldown/magazine ticking into `CombatRuntimeManifestedPartySkills.cs`.
- Phase 2-C moves manifested Eve drone lifecycle into `CombatRuntimeManifestedPartyDrones.cs`.
- Phase 2-D moves manifested non-drone skill visual and scene-object helper methods into `CombatRuntimeManifestedPartyVisuals.cs`.
- Phase 2-E moves generic manifested damage and projectile-fire helper methods into `CombatRuntimeManifestedPartyDamage.cs`.
- Monster-specific special formulas such as Rin shockwave and Eve frost field remain in existing controller partials for behavior preservation.

Purpose:

- Split the largest manager partial boundary after the battlefield access path is available.
- Remove party combat/state/UI responsibility from the controller partial surface.

Work:

- Separate manifested party state from manifested party view updates.
- Extract party skill ticking and unit dispatch into a party runtime service.
- Keep scene slot binding and Unity object references stable.
- Keep existing selected/manifested behavior parity until Play Mode verification by the user.

Acceptance criteria:

- `CombatRuntimeController` delegates manifested party tick to a dedicated object or narrowed service.
- Party label/HP/shield view update is isolated from damage/cooldown logic.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/MON/MON_BLACKBOARD.md`
- Relevant `boards/MON/{NAME}_MONSTER.md` files if monster-specific unit dispatch changes.
- `boards/UI/RUNSCENE_UI.md` if party UI display behavior changes.

### Phase 3. Projectile / Effect / Drone Simulation Split

Purpose:

- Move battlefield object lifecycle out of controller partials after facade access is established.
- Make projectile/effect/drone simulation independently readable and testable.

Work:

- Extract projectile ticking, collision, lifetime, and cleanup into a simulation service.
- Extract skill effect and drone ticking into the same battlefield simulation layer or separate narrow services.
- Keep damage application API stable until common target/effect migration starts.

Acceptance criteria:

- `CombatRuntimeController.Update()` calls a narrowed simulation tick method.
- Projectile/effect/drone lists are owned by the simulation boundary or facade, not directly scattered through skill files.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/COMBAT/PROJECTILE_BLACKBOARD.md`
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md` if status application changes.

### Phase 4. Enemy Simulation Split

Purpose:

- Separate spawning, movement, targeting, enemy active skills, and enemy status/buff timers from selected-unit and controller logic.

Work:

- Move enemy update and spawn policy into an enemy simulation boundary.
- Prepare `EnemyRuntime` to stop being only a private nested implementation detail.
- Keep enemy target priority and damage behavior unchanged.

Acceptance criteria:

- Enemy update can be read without loading selected-unit combat and manifested party implementation details.
- Enemy simulation exposes only narrow query/application APIs to the battlefield and skill layers.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/COMBAT/ENEMY_BLACKBOARD.md`
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`

### Phase 5. Selected Unit Combat Split

Purpose:

- Move 1P reload, automatic skill trigger, shield mirror, and selected-unit combat state out of the God Class center after shared battlefield/enemy boundaries exist.

Work:

- Extract selected unit combat tick from `CombatRuntimeProjectiles.cs` / controller partial surface.
- Keep selected unit state synchronized with existing `CombatUnitRuntime` until `CombatTargetModel` migration.
- Avoid changing current skill behavior in the same slice.

Acceptance criteria:

- Selected unit tick is owned by a selected-unit combat boundary.
- Existing `Update()` order still produces the same combat phases.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- Relevant monster board if selected monster skill behavior changes.

### Phase 6. Monster Runtime Adapter Interface Narrowing

Purpose:

- Stop monster skill runtime adapters from seeing the whole `CombatRuntimeController`.
- Enable skill reuse by making skills request target lookup, projectile/effect spawn, damage, and temporary effects through narrow contracts.

Work:

- Define small interfaces for target query, damage application, battlefield spawn, and temporary effect application.
- Migrate `CombatMonsterSkillRuntime` and monster-specific runtime adapters to those interfaces incrementally.
- Keep monster-specific formulas in existing files until common effect/target layers are stable.

Acceptance criteria:

- At least one monster runtime path no longer needs full controller access.
- Skill executor dependencies are narrower than `CombatRuntimeController`.
- Runtime and editor builds pass.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/MON/MON_BLACKBOARD.md`
- Relevant `boards/MON/{NAME}_MONSTER.md`

### Phase 7. CombatTarget / TemporaryEffect Migration

Purpose:

- Satisfy the shared target and temporary effect design after structural boundaries reduce duplication risk.
- Move toward reusable skill effects and common Monster/Enemy object handling.

Work:

- Introduce `ICombatTarget` / adapter first for selected unit, manifested unit, and enemy.
- Add `CombatTargetModel` only after read adapters prove stable.
- Introduce `TemporaryEffectInstance` for action speed, shield, then status effects.
- Decide whether a common `CombatActorBase` or pure target model base class is worthwhile after adapter/effect behavior stabilizes.

Acceptance criteria:

- Selected unit, manifested units, and enemies can be read through a common target contract.
- At least one temporary effect applies through a common effect API to more than one target type.
- No direct common base-class inheritance is introduced before adapter behavior is verified.

Related boards:

- `boards/REFACTORING/REFACTORING.md`
- `boards/COMBAT/COMBAT_BLACKBOARD.md`
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/MON/MON_BLACKBOARD.md`
- Relevant monster files.

## Implementation Gate

Each implementation phase must provide:

- Changed file list.
- Build evidence: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`.
- Build evidence: `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`.
- `git diff --check` evidence.
- Unity-MCP refresh / console evidence where relevant.
- Explicit note that Unity Play Mode gameplay verification remains user-owned.
- Board updates for every affected domain.
