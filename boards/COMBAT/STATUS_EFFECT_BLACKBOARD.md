# STATUS_EFFECT_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

## Task: 2026-05-08 Eve Unit Caster Status Effects

### Task title

Track Eve B-E status behavior through shared unit caster execution.

### Goals

- Keep status-board evidence for Eve Frost Field and Static Override moving to caster-based runtime.
- Ensure manifested Eve status effects remain tied to the manifested unit source.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies chill/freeze and shock behavior from selected and manifested Eve in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` creates caster-based Eve Frost Field effects and sets `SkillEffectRuntime.ManifestedSource` only for non-selected units.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` applies Static Override damage through `ApplyEveUnitSkillDamage(...)`, which separates selected and manifested damage paths.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: Eve C status parity was first fixed for manifested units, then Eve B-E execution moved to shared unit caster functions.

## Task: 2026-05-08 Manifested Eve Frost Field Chill

### Task title

Apply chill/freeze status from manifested Eve Frost Field persistent ticks.

### Goals

- Record that manifested Eve C now applies status through the persistent field tick path.
- Keep status application tied to the manifested source unit rather than selected-Eve-only checks.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Eve C chill/freeze behavior in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` added `SkillEffectRuntime.ManifestedSource`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` checks `effect.ManifestedSource != null` inside `TickSkillEffect(...)` and routes those ticks to manifested effect handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` applies `ApplyChill(target, Mathf.Max(1, effect.StatusStacks), 2.5f)` and `target.FreezeTimer` for manifested `eve-c` effects.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported that manifested Eve Frost Field was missing chill status and ongoing damage.

## Task: 2026-05-06 Sein Fire Resistance Reduction And Heat State

### Task title

Track Sein fire-specific enemy state for active skill interactions.

### Goals

- Record temporary Sein heat state from Scorching Arrow and Superheated Zone.
- Record temporary fire-defense reduction from Doomsday Line.
- Ensure fire-defense reduction is applied to Sein fire damage resolution.

### Constraints

- Role Owner is Code Builder.
- This pass adds internal runtime state only; user-facing status labels can be expanded later if requested.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein C trait 5, D repeated tick behavior, and E fire-resistance reduction in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` adds `SeinScorchingArrowTimer`, `SeinSuperheatedZoneTimer`, `SeinSuperheatedTickCount`, `SeinFireDefenseReductionTimer`, and `SeinFireDefenseReduction` to `EnemyRuntime`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` updates those timers, applies D tick count state, and applies E fire-defense reduction.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` includes `GetSeinFlatDefenseReduction(...)` in selected-Monster projectile fire-damage resolution.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.

### History

- 2026-05-06: Sein A-E active implementation introduced internal heat/fire-defense-reduction state for C/D/E interactions.

## Task: 2026-05-05 Rin Collapse Aftermath Physical Defense Reduction

### Task title

Track Rin J physical-defense reduction as a combat status.

### Goals

- Record that Rin J `Collapse Aftermath` now applies a temporary physical-defense reduction to Collapse Strike hit targets.
- Keep the status board aware of the new enemy status label and timer field.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Collapse Strike hit targets receive the intended physical-defense reduction behavior in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Rin-specific details are recorded in `boards/MON/RIN_MONSTER.md`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` adds `RinPhysicalDefenseReductionTimer` and `RinPhysicalDefenseReduction` to `EnemyRuntime`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` decrements the timer, clears the reduction when expired, and displays `물방감소` while active.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` applies the J reduction from Collapse Strike and exposes it to `DamageCalculator.Resolve(...)` through `GetRinPercentDefenseReductions(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` includes Rin physical-defense reduction in selected-Monster projectile damage resolution.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-05: User requested Rin F-J passive implementation; Rin J required adding a temporary physical-defense-reduction status path.

## Task: 2026-05-03 Ariel Sanctuary Proclamation State Correction

### Task title

Correct Ariel J timed buff and shield-dependent holy bonus state handling.

### Goals

- Keep blessing-derived buffs on `arielBlessingTimer` only.
- Keep E master 1 `Heavenly Sanctuary` damage reduction on its own timer.
- Add a distinct timed state for J `Sanctuary Proclamation` and tie its holy-damage bonus to remaining Archangel shield state.

### Constraints

- Role Owner is Code Builder.
- Current runtime still has one selected allied Monster and one pooled shield value.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Builder follow-up implemented and locally validated. Code Reviewer has not been rerun because the user did not request another review.

### Next Actions

- User verifies that Ariel J holy-damage bonus ends as soon as the active pooled shield is no longer owned by E.
- User verifies that Ariel E battlefield effect appears and that Ariel C no longer repeatedly retries during held-input blocked windows.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/j-sanctuary-proclamation.md:18-19` defines J as a timed post-E action-speed buff plus a shield-remaining holy-damage bonus.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs:136-143` now updates `arielSanctuaryProclamationTimer` and `arielArchangelShieldTimer` independently from `arielBlessingTimer`.
- `CombatRuntimeArielSkills.cs:429-451` now starts the J proclamation timer on E cast, delegates Archangel shield ownership to the shared shield helper, and spawns the missing E battlefield effect.
- `CombatRuntimeArielSkills.cs:554-580` now tracks Archangel shield ownership only when the E shield actually becomes the pooled shield owner, and clears it if a stronger non-E shield replaces that owner.
- `CombatRuntimeArielSkills.cs:592-600` and `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:315-319` now reduce the tracked E shield share when shield absorption happens.
- `CombatRuntimeArielSkills.cs:771`, `852`, and `898-900` now keep J holy-damage bonus and action-speed bonus bound to dedicated Archangel/proclamation state.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:332-356` now prevents Ariel support-status retries from running every held-input frame while Ariel A is not in a firing window, which addresses the reported occasional C barrage symptom at the trigger level.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing warnings.
- External Code Reviewer executed once and found one remaining issue in the prior pass: `CombatRuntimeArielSkills.cs:429-431` could still mark an unapplied E shield as active if a larger non-E shield already occupied the pooled selected-Monster shield state; Builder follow-up corrected that ownership path and did not rerun review afterward.

### History

- 2026-05-03: User requested Ariel passive F-J implementation.
- 2026-05-03: Code Builder corrected the status-model mismatch in J by separating proclamation timing from blessing timing and by tracking the E shield share explicitly.
- 2026-05-03: User explicitly requested Code Reviewer execution; Reviewer returned NEEDS_CHANGES for the remaining E-shield source leak.
- 2026-05-03: User then requested fixing that reviewer finding and also reported missing Ariel E effect plus occasional Ariel C barrage behavior; Code Builder applied the follow-up and revalidated with build and Unity refresh evidence.

## Task: Shield HP Bar Ratio Visual

### Task title

Display shields as a white segment in the same HP bar.

### Goals

- Preserve actual HP and shield values.
- Show shield as white bar space adjacent to red HP inside the same fixed-width bar.
- Apply the visual to selected Monster shields and enemy shield bars that use the shared helper.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the latest request did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies shield visuals for Ariel/Eve shield-granting effects in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now uses `UpdateHpShieldBarFill()` and `UpdateBarSegment()` to split the root HP bar into red HP and white shield segments when shield is greater than 0.
- Existing shield values remain stored separately as `unitShieldValue` and `enemy.ShieldValue`; the visual calculation only changes `SpriteRenderer` segment scale and position.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-04-30: User requested League-style shield visualization where HP is unchanged and shield adds white visual space within one fixed bar.
- 2026-04-30: Code Builder changed the shared status bar visual update logic.
- 2026-04-30: User requested HP Bar `Background` color to be black instead of the same white as shield. Code Builder changed `CreateHpBar()` to pass `Color.black` for the `Background` part while leaving shield as white.

## Task: Ariel Holy Exposure And Blessing Runtime

### Task title

Add Ariel Holy Exposure, shield, blessing, and sanctuary status runtime.

### Goals

- Add enemy Holy Exposure state for Ariel A/D/I/E interactions.
- Add selected-Monster shield/buff timers for Ariel B/C/E/G/H/J interactions.
- Keep statuses within the current combat runtime data model.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Current runtime has one selected allied Monster, so party buffs apply to that unit only.
- Code Reviewer was not run without explicit user permission.

### Role Owner

Code Builder

### Status

Implemented and locally validated. Code Reviewer returned FAIL for Radiant Shield reflection and Holy multiplier duplication, and Code Builder has applied the requested correction pass.

### Next Actions

- User verifies Holy Exposure labels and Ariel shield/blessing/sanctuary behavior in Play Mode.

### Evidence

- `EnemyRuntime` now stores `HolyExposureTimer`, `HolyExposureStacks`, damage taken bonus, Holy flat defense reduction, critical damage taken bonus, detonation multiplier, and accumulated Holy damage.
- `CombatRuntimeEnemies.UpdateEnemies()` decrements Holy Exposure and resolves expiry detonation.
- `BuildEnemyStatusText()` displays `Holy Exposure{stacks}` while active.
- `CombatRuntimeArielSkills.cs` applies Holy Exposure from Ariel A master 2 and Ariel D, and uses Ariel I passive/traits for target damage and Holy resistance reduction.
- `CombatRuntimeArielSkills.cs` manages Ariel shield, blessing, sanctuary, action speed, cooldown charge speed, and Holy damage buff timers.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Follow-up fix passes the source enemy into selected-Monster damage, so `ariel-b-master-2` reflects absorbed Radiant Shield damage back to the attacker instead of nearest enemy.
- Follow-up fix removes pre-application of the shared Ariel Holy damage multiplier from Ariel A/C/D/E cast paths; final Holy bonus application remains in the shared final damage calculation.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.
- Follow-up Unity refresh completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: Code Builder added Ariel-specific status state and runtime helpers while implementing Ariel A-E/F-J.
- 2026-04-30: User instructed Builder to fix Code Reviewer findings; Builder corrected Radiant Shield attacker reflection and Holy damage multiplier duplication.

## Task: Eve Active Skill Status Runtime

### Task title

Implement Eve active skill A-E runtime status effects before roadmap step 6.

### Goals

- Make Eve learned active skills A-E cast on player click with automatic nearest-enemy targeting.
- Keep skills from auto-casting without a click.
- Implement Eve-related combat statuses first: shock, chill/freeze blue tint, slow, vulnerability, and shield bar visuals.
- Apply selected Eve active trait choices to actual runtime behavior.
- Use Eve's implementation shape as the later framework for other monsters.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run one Code Reviewer after Builder logic changes.
- Preserve the existing user-deferred reviewer finding in `Pakuri/Assets/Data/GameData/Monsters/eve.asset` without fixing it unless requested.

### Role Owner

Code Builder

### Status

Builder implemented the user-approved correction pass for Eve A manual firing, B-E click-triggered automatic targeting, infinite skill target range, the prior reviewer findings, the mojibake status message fix, and RunScene manual transform preservation for EveUnit status visuals. Build, Unity console validation, and the required one-shot external Code Reviewer pass completed with `REVIEW_RESULT: PASS`.

### Next Actions

- User can Play Mode verify Eve A/B-E behavior and RunScene manual transform preservation.
- Continue to the next requested design or implementation task.

### Evidence

- User clarified that learned active skills should be cast by player click, auto-targeting the nearest enemy in range, but should not auto-cast by themselves.
- User clarified selected trait enhancement effects should actually apply.
- User accepted targeting recommendation for Eve D: target the nearest shocked enemy in range, and do not cast if none exists.
- User clarified chill and freeze can both use the same blue-tint visual for now and should be documented later in HTML.
- `CombatRuntimeEveSkills.cs` was added to implement Eve A-E click-cast behavior, beam/field/drone runtime objects, status application helpers, and trait checks by `eve-*-trait-*` reward ids.
- `CombatRuntimeProjectiles.cs` now supports player projectile pierce, per-projectile hit tracking, Eve drone vulnerability application, and delegates Eve click casting before legacy click-to-point firing.
- `CombatRuntimeEnemies.cs` now tracks shock/chill/freeze/slow/vulnerability timers/stacks, applies blue tint for shock/chill/freeze, and updates a white shield bar overlay.
- Enemy and selected monster HP bars are now red, while the shield bar is white.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding 1: `eve-a-trait-5` applies power +25% but not the documented lightning/status chance +35%; reviewer cited `CombatRuntimeEveSkills.cs` around line 172, `CombatRuntimeProjectiles.cs` around lines 58-60, and `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` line 52.
- Reviewer finding 2: `FreezeTimer` is declared/consumed but no code path sets it; reviewer cited `CombatRuntimeController.cs` around line 62, `CombatRuntimeEnemies.cs` around lines 643 and 671, `CombatRuntimeEveSkills.cs` around line 360, and `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md` line 44.
- User clarified the correction: Eve A must be manual firing toward the clicked direction, not automatic casting or automatic targeting; that same click is the trigger for the other Eve skills.
- User clarified B-E should conditionally auto-cast and auto-target once the click trigger fires.
- User clarified skill range should be infinite; if the trigger works, the skill should execute on the nearest enemy or the skill-specific priority target.
- `CombatRuntimeProjectiles.UpdateSelectedMonsterCombat()` now calls `TryTriggerEveAutomaticSkills()` on click without consuming the primary A firing path.
- `CombatRuntimeProjectiles.FirePrimarySkill()` now routes Eve A to `FireManualEveArcBolt(direction)` after deriving the clicked direction from `currentAttackPoint`.
- `CombatRuntimeEveSkills.TryTriggerEveAutomaticSkills()` now triggers only B-E, not A.
- `CombatRuntimeEveSkills.FireManualEveArcBolt()` now applies Eve A trait projectile count, pierce, damage, fire interval, reload, and trait 5 status chance modifiers while preserving clicked-direction firing.
- `ProjectileRuntime.StatusChance` and projectile hit handling now allow Eve A trait 5 to add +35% status chance without changing the global configured chance for other projectiles.
- Eve B, C, D, and drone E targeting now use `float.PositiveInfinity` range; D still keeps its shocked-target predicate as the skill-specific priority.
- `SkillEffectRuntime.FreezeDuration` is now set by `eve-c-trait-5`, and Frost Field ticks apply `enemy.FreezeTimer` when that trait is selected.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer one-shot review for the correction pass returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Latest reviewer finding: `CombatRuntimeEveSkills.cs` contains mojibake user-facing `statusLabel` messages at and around lines 87, 106, 171, 288, 353, 425, and 489. Reviewer verified the core logic requirements as satisfied but flagged the visible broken text.
- `CombatRuntimeEveSkills.cs` statusLabel messages at lines 87, 106, 171, 288, 354, 425, and 489 were changed to readable ASCII English text to resolve the mojibake finding.
- `CombatRuntimeScene.EnsureStatusLabel()` now preserves existing `MonsterHpLabel` local position and scale, assigning defaults only when the label object is newly created.
- `CombatRuntimeEnemies.CreateHpBar()` now preserves existing `MonsterHpBar` root position and scale and preserves existing Background/Fill transforms, assigning defaults only when those objects are newly created.
- `CombatRuntimeEnemies.CreateShieldBarFill()` now preserves an existing Shield transform and only assigns default shield transform values when newly created.
- `CombatRuntimeScene.EnsureSpriteRenderer()` no longer overwrites existing anchors with SpriteRenderers; in the current `RunScene`, `EveUnit` already has a SpriteRenderer, so its scene-authored scale is preserved.
- `CombatRuntimeScene.EnsureBattlefieldBackgroundVisual()` no longer forces `BattlefieldBackground` position; scale is still only changed when `autoFitBattlefieldBackgroundToField` is true. `RunScene.unity` currently has `autoFitBattlefieldBackgroundToField: 0`.
- `Pakuri/Assets/Scenes/RunScene.unity` contains actual scene-authored `EveUnit`, `MonsterHpLabel`, `MonsterHpBar`, and `BattlefieldBackground` objects.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer one-shot review for the latest changes returned `REVIEW_RESULT: PASS`.
- Added `Pakuri/reference/Report/2026-04-29-eve-active-skill-runtime-implementation.html` documenting the Eve A-E runtime implementation, the user clarification process that reduced implementation ambiguity, status/effect wiring, manual transform preservation, and verification results.

### History

- 2026-04-29: User requested Eve Monster active skill A-E status/effect runtime before roadmap step 6 and provided detailed semantics for pierce, extra projectiles, beams, area instant skills, drones, blue status tint, red HP bar, and white shield bar.
- 2026-04-29: Designer asked five implementation interpretation questions; user clarified click-cast auto-targeting, actual trait application, D shocked-target behavior, and blue tint for both ice states.
- 2026-04-29: Code Builder implemented Eve A-E runtime behavior and completed local build/Unity console validation.
- 2026-04-29: External Code Reviewer found two missing trait/status behavior issues; Builder paused per AGENTS.md.
- 2026-04-29: User instructed Builder to prioritize restoring A as manual clicked-direction firing, make B-E click-triggered automatic infinite-range skills, and fix the two reviewer findings.
- 2026-04-29: Code Builder implemented the correction pass and completed local build/Unity console validation; required external Reviewer pass remains pending.
- 2026-04-29: External Code Reviewer verified the correction logic but returned `NEEDS_CHANGES` for mojibake status messages in `CombatRuntimeEveSkills.cs`; Builder paused per AGENTS.md.
- 2026-04-29: User instructed Builder to fix the reviewer finding and preserve manually edited RunScene `EveUnit` child HP Label/HPBar position and scale, plus other scene-authored transforms where applicable.
- 2026-04-29: Code Builder fixed Eve status messages, preserved existing status visual transforms and scene-authored anchor transforms, completed build/Unity validation, and external Code Reviewer returned `PASS`.
- 2026-04-29: Code Builder added an HTML implementation report for the Eve active skill runtime work under `Pakuri/reference/Report`.

## Task: Eve Passive Runtime Implementation

### Task title

Implement Eve passive runtime effects for the Eve skill documents under `Pakuri/reference/2.Monster/eve`.

### Goals

- Implement Eve passive effects from the existing Eve passive documents `f-voltage-calibration.md`, `g-weakness-analysis.md`, `h-particle-separation.md`, `i-cooling-algorithm.md`, and `j-overcurrent-circuit.md`.
- Connect selected passive and passive-trait reward ids to runtime combat behavior.
- Add a white shield HP bar overlay to the selected monster HP bar while keeping the full HP bar length unchanged.
- Apply behavior speed, cooldown, duration, firing interval, and damage-area adjustments according to `Pakuri/reference/3.combat/combat-stat-system.md`.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run one Code Reviewer after Builder logic changes.
- The user mentioned `k`, but the actual Eve skill folder contains `f` through `j` and no `k` file; this pass treated the existing `h-particle-separation.md` / slot H document as the missing fifth passive.
- Preserve unrelated existing worktree changes, including the prior next-work HTML report and the user-deferred `eve.asset` trailing whitespace finding.

### Role Owner

Code Builder

### Status

Builder implementation and reviewer correction pass completed. Local build/Unity console validation completed, and the follow-up external Code Reviewer returned `REVIEW_RESULT: PASS`.

### Next Actions

- User can Play Mode verify Eve passive effects, including Voltage Calibration shield/reload acceleration, Particle Separation Prism Ray proc, Cooling Algorithm freeze interactions, Overcurrent Circuit lightning bonuses, and Weakness Analysis vulnerable-target bonuses.
- Continue to the next requested design or implementation task.

### Evidence

- Actual Eve passive files present under `Pakuri/reference/2.Monster/eve/skill`: `f-voltage-calibration.md`, `g-weakness-analysis.md`, `h-particle-separation.md`, `i-cooling-algorithm.md`, and `j-overcurrent-circuit.md`; no `k` file exists.
- `combat-stat-system.md` says action speed accelerates projectile firing interval and active skill cooldown charging, while duration and firing interval are separate stats.
- `CombatRuntimeController.cs` now has learned passive state and selected monster shield runtime fields.
- `CombatRuntimeScene.cs` now creates and updates a white selected monster shield bar overlay on `MonsterHpBar`.
- `CombatRuntimeProjectiles.cs` now applies Eve passive damage/defense/status chance modifiers and selected monster shield absorption.
- `CombatRuntimeEnemies.cs` now applies selected monster shield absorption to direct enemy attacks and triggers Eve H trait 3 freeze-release damage.
- `CombatRuntimeEveSkills.cs` now implements Eve F/G/H/I/J passive checks, shield, action speed helper, passive damage multipliers, resistance reductions, status chance bonus, and particle-separation Prism Ray proc.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- Initial parallel Editor build failed with a file lock on `obj\Debug\Assembly-CSharp.dll`; rerunning `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` sequentially completed with 0 errors and the same warnings.
- Unity script refresh/compile was requested; follow-up refresh returned `resulting_state=idle`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding 1: `CombatRuntimeProjectiles.cs` line 250 decrements Arc Bolt reload with raw `Time.deltaTime`, so `eve-f-trait-3` action speed does not affect reload while shielded.
- Reviewer finding 2: current uncommitted changes include the prior unrelated `Next Roadmap Work Plan Report` block in `BLACKBOARD.md` and untracked `Pakuri/reference/Report/2026-04-29-next-work-plan.html`, which are outside the Eve passive runtime implementation scope unless explicitly justified or separated.
- Reviewer finding 1 was corrected by applying `GetEveActionSpeedMultiplier()` to the Arc Bolt reload countdown in `CombatRuntimeProjectiles.UpdateSelectedMonsterCombat()`.
- Reviewer finding 2 is explicitly justified here: `Pakuri/reference/Report/2026-04-29-next-work-plan.html` and the `Next Roadmap Work Plan Report` BLACKBOARD block were created in the immediately preceding user-requested Designer task, are preserved as completed task evidence, and are not part of the Eve passive runtime implementation logic.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- Follow-up parallel Editor build hit a transient write lock on `obj\Debug\Assembly-CSharp.dll`; rerunning `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` sequentially completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; follow-up refresh returned `resulting_state=idle`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- Follow-up external Code Reviewer confirmed prior finding 1 fixed, accepted the explicit separation/justification for prior finding 2, and returned `REVIEW_RESULT: PASS`.

### History

- 2026-04-29: User requested implementation of Eve passive effects for active skills A-E, shield HP bar overlay, and timing/range handling based on `combat-stat-system.md`.
- 2026-04-29: Code Builder confirmed actual Eve passive documents are F-J and no K document exists; implementation treated H as the missing fifth passive.
- 2026-04-29: Code Builder implemented the runtime pass and completed local build/Unity console validation.
- 2026-04-29: External Code Reviewer returned `NEEDS_CHANGES`; Builder paused per AGENTS.md.
- 2026-04-29: User instructed Builder to fix the reviewer findings; Builder applied the Arc Bolt reload action-speed correction and documented the prior next-work report as a separate completed user-requested task.
- 2026-04-29: Code Builder rebuilt, rechecked Unity console, and follow-up external Code Reviewer returned `PASS`.

## Task: Monster And Enemy Hp Slider Bars

### Task title

Add overhead HP text and HP slider bars for Stage 1 enemies and the selected Player Monster.

### Goals

- Add a simple HP slider-style bar above enemies using existing/basic Unity-rendered assets.
- Add the same kind of name, HP text, and HP bar above the selected Player Monster.
- Keep HP text/bar updates tied to the current runtime health values.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- All claims must be grounded in actual files and command output.
- Do not import new visual assets for this request; use the existing generated 1x1 shared sprite path in `EveVerticalSliceController`.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. External Reviewer execution was attempted but could not complete because the Codex CLI reported a usage limit. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that enemies show name, HP text, and HP bar above their heads.
- User verifies in Play Mode that the selected Player Monster shows name, HP text, and HP bar above the Monster.
- User verifies the bars shrink as HP decreases for both enemies and the selected Player Monster.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` `EnemyRuntime` now stores `HpBarFill`.
- `EveVerticalSliceController.cs` now stores `selectedMonsterLabel` and `selectedMonsterHpBarFill`.
- `EnsureSelectedMonsterStatusVisuals()` creates/reuses `MonsterHpLabel` and `MonsterHpBar` under `eveAnchor`.
- `SpawnEnemy()` creates `EnemyHpBar` under each spawned enemy, and `UpdateEnemyLabel()` updates both text and bar fill.
- `CreateHpBar()`, `EnsureHpBarPart()`, and `UpdateHpBarFill()` implement the shared world-space HP bar with `SpriteRenderer` and the existing shared 1x1 sprite.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query returned MCP-FOR-UNITY client handler entries only, not project script compile errors.
- External Reviewer command was attempted with `codex.exe exec --skip-git-repo-check`; it failed with a Codex usage-limit message and did not produce a review verdict.

### History

- 2026-04-27: User requested HP Slider Bar using basic assets and the same name/HP display for Player Monster as enemies.
- 2026-04-27: Implemented world-space SpriteRenderer HP bars for enemies and selected Player Monster in `EveVerticalSliceController.cs`.
- 2026-04-27: Attempted external Code Reviewer execution. The command exited before review due to Codex usage limit, so only local Builder self-review, build, Unity refresh, and console checks are available for this turn.

# Task: 2026-05-04 Rin Shockwave And Collapse Slow Effects

## Task title

Use existing combat slow and knockback state for Rin active skill effects.

## Goals

- Implement Rin C knockback and master 2 slow.
- Implement Rin E master 2 slow.
- Reuse existing `EnemyRuntime.SlowTimer` and `SlowMultiplier` so enemy movement already respects the effect.

## Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

## Role Owner

Code Builder

## Status

Implemented and locally validated.

## Next Actions

- User verifies Rin C knockback and Rin C/E slow effects in Play Mode.

## Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:266` applies Rin C master 1 lightning extra damage, while `:272` applies Rin C master 2 slow.
- `CombatRuntimeRinSkills.cs:420` applies Rin E master 2 slow and dark extra damage.
- `CombatRuntimeRinSkills.cs:661` moves hit enemies for Rin C knockback and clamps them inside the battlefield bounds.
- `CombatRuntimeRinSkills.cs:676` writes slow state through the existing `EnemyRuntime.SlowMultiplier` and `SlowTimer`.
- Existing `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` already decrements `SlowTimer`, resets `SlowMultiplier`, and multiplies enemy movement by `SlowMultiplier`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only existing Unity/MCP warnings.

## History

- 2026-05-04: Code Builder implemented Rin C/E slow and C knockback using current combat status-effect fields.

## Task: 2026-05-06 Sein Passive Debuff State

### Task title

Track Sein H/I/J passive target debuffs and fire resistance changes.

### Goals

- Add enemy runtime timers for Sein H Burning Trajectory, I Thermal Spread, and J Doomsday Omen damage-taken bonuses.
- Keep Sein fire-resistance reduction and fire damage-taken effects inside existing combat damage resolution.
- Preserve status-effect evidence for future continuation and review.

### Constraints

- Role Owner is Code Builder.
- This pass adds internal runtime state only; user-facing status label expansion can be done later if requested.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that C, D, and E hits apply the expected follow-up fire damage increases and J kill cooldown charge.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now adds `SeinBurningTrajectoryTimer`, `SeinBurningTrajectoryDamageTakenBonus`, `SeinThermalSpreadTimer`, `SeinThermalSpreadDamageTakenBonus`, `SeinDoomsdayOmenTimer`, and `SeinDoomsdayOmenDamageTakenBonus` to `EnemyRuntime`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now decrements and clears the Sein H/I/J timers, applies H/I/J debuffs from the matching active skill paths, and includes those bonuses in Sein fire damage resolution.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now extends D tick speed/radius and E cooldown charge behavior from the I/J passive data.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity console error query after refresh returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-06: Code Builder added internal enemy state for Sein H/I/J passive debuffs during the F-J implementation pass.

## Task: 2026-05-07 Sein C/E Residual Zone Follow-up

### Task title

Track Sein C falling residual and E ash superheated zone placement.

### Goals

- Record that C `Falling Trajectory` now creates a residual fire zone after delayed explosion expiry.
- Record that E `Ashen Sky` zones are created from actual hit target positions instead of one initial target center.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies C residual zone and E ash zone placement in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:624` creates `SeinFallingTrajectoryResidual` for `sein-c` when `SeinSpawnResidualOnExpire` is set.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:762` creates E `SeinAshSuperheatedZone` effects from actual target positions, up to 3 zones.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: User reported C `Falling Trajectory` fire zone not spawning and E ash zones grouping around the first target.
- 2026-05-07: Code Builder added C residual expiry handling and changed E ash-zone creation to actual hit targets.

## Task: 2026-05-07 Vega Name Mark And Silence State

### Task title

Track Vega `이름표식` and `침묵` combat state added for active skills A-E.

### Goals

- Add enemy runtime state for Vega name marks and silence.
- Display Vega mark/silence state in enemy labels.
- Prevent silenced enemies from using their active skill while silence remains.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega mark stacks and B silence behavior in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` adds `VegaNameMarkStacks` and `VegaSilenceTimer` to enemy runtime state.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` decrements `VegaSilenceTimer`, displays `이름표식`/`침묵`, and blocks active skill use while silenced.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs` applies name marks and silence from Vega A/B/D/E active skill paths.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: Code Builder added Vega status state during Vega A-E active skill implementation.

## Task: 2026-05-07 Vega Passive Vulnerability State

### Task title

Track Vega I/J passive target vulnerability state.

### Goals

- Add enemy runtime timers for Vega I `연쇄 참결` area-damage vulnerability.
- Add enemy runtime timers for Vega J `사형 집행인` survivor damage vulnerability.
- Display the new temporary vulnerability states in enemy status labels.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Vega D-hit targets show area vulnerability and Final Sentence survivors show survivor vulnerability.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:88` through `:91` add `VegaBlackLedgerAreaVulnerability*` and `VegaFinalSentenceVulnerability*` fields to `EnemyRuntime`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:83` through `:92` decrement and clear the new Vega vulnerability timers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:922` applies Vega I area vulnerability after D area hits.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:939` applies Vega J survivor vulnerability after Final Sentence does not kill.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs:909` and `:914` add `참결취약` and `선고취약` labels.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: Code Builder added Vega I/J passive target-vulnerability state during Vega F-J passive implementation.

## Task: 2026-05-07 Vega B Silence Rectangle Source

### Task title

Track Vega B silence application after the target-centered rectangle correction.

### Goals

- Record that Vega B silence still applies after changing B from line damage to target-centered rectangle damage.
- Keep status-effect history aligned with the Vega combat correction.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that enemies inside Vega B's 3 by 1 target rectangle receive silence as before.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:422` through `:441` now applies `ApplyVegaSilence(...)` in the target-centered rectangle hit loop.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` no longer needs the old delayed `SkillEffectRuntime.VegaSilenceDuration` field after B was made immediate.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: User requested Vega B to apply immediate rectangular area damage on the enemy instead of line damage from Vega.

## Task: 2026-05-08 Manifested Sustained Effect Visual Duration

### Task title

Track sustained Manifested field/buff visual duration correction.

### Goals

- Record that this pass corrected visual lifetime for sustained Manifested skill effects.
- Avoid claiming selected-monster status-effect parity for Manifested skills beyond the inspected change.

### Constraints

- Role Owner is Code Builder.
- This pass changed visual/drone lifetime, not selected-monster status-effect timer semantics.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies sustained Manifested visual duration in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now maps `eve-c` to `EveFrostFieldDuration`, `sein-d` to `SeinSuperheatedZoneDuration`, `vega-c` to `VegaExterminationPermitDuration`, `ariel-b` to `ArielRadiantShieldDuration`, and `ariel-c` to `ArielBlessingDuration`.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported sustained Manifested skill effects were much shorter than the original skills.
