# PROJECTILE_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

## Task: 2026-05-06 Sein Projectile Active Skills

### Task title

Record Sein projectile runtime additions for active skills A and B.

### Goals

- Route Sein A through a dedicated manual fire projectile path.
- Implement Sein B as a separate click-triggered volley projectile skill.
- Keep no-hit selected-Monster projectiles on the existing X-boundary cleanup rule.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein A and B projectile travel, collision, and cleanup in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` routes selected Sein A fire to `FireManualSeinScorchingArrow(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` calls `TrackSeinProjectileHit(...)` after selected-Monster projectile damage is applied.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` creates Sein A/B projectiles with fire damage, selected projectile sprite fallback, and long lifetime so no-hit cleanup remains X-boundary based.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.

### History

- 2026-05-06: Sein A-E active implementation added dedicated projectile behavior for Scorching Arrow and Blazing Volley.

## Task: 2026-05-05 Monster Projectile No-Range Cleanup Rule

### Task title

Record current monster projectile cleanup rule as X-boundary based instead of range/lifetime based.

### Goals

- Keep monster projectile skills from using gameplay range as the projectile deletion condition.
- Delete magazine/projectile skill objects when their X position reaches the predefined battlefield X coordinate.
- Keep non-projectile skill range decisions recorded in the monster board while this board tracks projectile cleanup behavior.

### Constraints

- Role Owner is Designer.
- This is a design rule and implementation handoff note, not a completed code change.
- User performs Play Mode gameplay verification.
- Code Reviewer execution requires explicit user permission.

### Role Owner

Designer

### Status

Design rule recorded; Code Builder handoff needed if every current projectile path must be normalized.

### Next Actions

- Code Builder should audit all selected-Monster projectile creation paths and make sure `RemainingLifetime` cannot remove player monster projectiles before the battlefield X boundary when no hit occurs.
- Code Builder should preserve enemy projectile lifetime behavior separately unless the user also changes enemy projectile rules.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` moves projectiles each update and decrements `RemainingLifetime`, but no-hit player projectiles are currently cleaned up through `HasPlayerProjectileReachedBattlefieldXEdge(projectile)`.
- `HasPlayerProjectileReachedBattlefieldXEdge(projectile)` compares the projectile X coordinate against `0f` and `fieldSize.x`, using projectile direction to decide the relevant edge.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` still sets Rin A `RemainingLifetime` from `skill.Range / RinShatteringFistProjectileSpeed`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` still sets Ariel A `RemainingLifetime` from configured lifetime and `skill.Range / speed`.

### History

- 2026-05-05: User clarified that current monster projectile skills should disappear at a predefined X coordinate, not because of skill range.

## Task: 2026-05-04 Selected-Monster Projectile X-Edge Cleanup

### Task title

Make selected-Monster projectiles disappear at the battlefield X boundary instead of lifetime expiry.

### Goals

- Stop Rin A and other selected-Monster projectiles from being cleaned up by short configured/computed lifetime while still traveling.
- Use the common projectile rule requested by the user: delete the projectile when its X coordinate reaches the map end.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly request it for this change.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies projectile cleanup positions in Play Mode.
- If a future skill intentionally fires with no horizontal direction, revisit whether that skill needs a separate cleanup rule.

### Evidence

- Before this change, `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` decremented player projectile `RemainingLifetime` and cleaned up no-hit selected-Monster projectiles when `RemainingLifetime <= 0f`.
- `CombatRuntimeProjectiles.cs` now calls `HasPlayerProjectileReachedBattlefieldXEdge(projectile)` and only cleans up selected-Monster projectiles at the battlefield X edge.
- The status label now reports that the projectile disappeared after reaching the battlefield X boundary.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP assembly conflict warnings.
- Unity console error query returned only an MCP-FOR-UNITY client-handler exit log.

### History

- 2026-05-04: User reported Rin A disappearing again after about 0.5 seconds and requested the monster-wide projectile deletion rule be based on reaching the map-end X coordinate.
- 2026-05-04: Builder changed selected-Monster no-hit projectile cleanup from lifetime expiry to X-edge cleanup.

## Task: 2026-05-03 Damage Application Popup Trigger Wiring

### Task title

Trigger shared floating damage popups from both enemy-hit and selected-Monster-hit damage application paths.

### Goals

- Spawn damage popups when enemies lose shield/HP from projectiles or skill damage.
- Spawn damage popups when the selected Monster loses shield/HP from enemy hits.
- Keep the existing damage return values intact while adding visual trigger data.

### Constraints

- Role Owner is Code Builder.
- Ground the change in actual projectile/damage runtime code.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly request it for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies that direct hits, branch hits, and incoming enemy hits all raise one damage number over the damaged target.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:285-307` now tracks `totalAppliedDamage` across shield absorption plus HP loss in `ApplyDamageToEnemy(...)` and calls `SpawnDamagePopupForEnemy(...)` when the total is positive.
- `CombatRuntimeProjectiles.cs:313-337` now does the same for `ApplyDamageToSelectedMonster(...)`, calling `SpawnDamagePopupForSelectedMonster(...)`.
- `CombatRuntimeProjectiles.cs:49`, `:147`, and `:241` still route the shared enemy-hit, selected-Monster-hit, and Eve branch-damage paths through those two centralized damage application functions, so one popup trigger path covers the existing runtime entry points.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeDamagePopups.cs` currently remains an intentionally empty partial stub because the generated Unity C# project still references that file path while the real popup implementation was moved into the already-included `CombatRuntimeScene.cs` partial; keeping the stub avoids the previously observed `CS2001` missing-source failure.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both completed with 0 errors after the stub/file-path cleanup.

### History

- 2026-05-03: User requested visible damage numbers for both sides of combat.
- 2026-05-03: Code Builder wired popup spawning into the two centralized damage application helpers and kept an empty partial stub file in place to satisfy the current generated project reference.

## Task: 2026-05-03 Ariel Automatic Skill Trigger Cadence Follow-up

### Task title

Gate Ariel automatic support-skill retries to actual firing windows.

### Goals

- Stop held input from retrying Ariel support skills every frame while Ariel A cannot fire.
- Remove the reported occasional Ariel C barrage symptom without changing Ariel A projectile fire itself.
- Keep the shared held-input projectile path intact for the other Monsters.

### Constraints

- Role Owner is Code Builder.
- Ground the fix in actual projectile/combat runtime code.
- User performs Play Mode gameplay verification.
- Code Reviewer was not rerun because the user did not explicitly request another review.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies that holding attack no longer causes Ariel C to repeatedly trigger while Ariel A is on reload or shot cooldown.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeHud.cs` still keeps `fireRequestedThisFrame` true while left mouse or touch is held.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:332-345` now adds `ShouldTrySelectedMonsterAutomaticSkillsThisFrame()` and keeps the Ariel-specific rule there.
- `CombatRuntimeProjectiles.cs:349-356` now only calls `TryTriggerSelectedMonsterAutomaticSkills()` for Ariel when `reloadRemaining <= 0f`, `shotCooldown <= 0f`, and `currentShotsRemaining > 0`, instead of retrying on every held-input frame.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the existing Unity/MCP warnings.
- Unity refresh completed with `resulting_state: idle`; Unity console error query returned MCP-FOR-UNITY handler exit logs only.

### History

- 2026-05-03: User reported that Ariel C sometimes behaves like a barrage during held-input combat.
- 2026-05-03: Code Builder traced the retry path to `UpdateSelectedMonsterCombat()` and added Ariel-only firing-window gating for automatic support-skill checks.

## Task: Ariel A Projectile Lifetime Follow-up

### Task title

Prevent Ariel Judgement Light from expiring too soon.

### Goals

- Keep Ariel A using its documented projectile speed and skill range.
- Avoid immediate visual cleanup caused by range/speed producing a very short lifetime.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the latest request did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel A projectile travel/cleanup timing in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` line with `var lifetime` now uses `Mathf.Max(projectileLifetimeConfigured, range / Mathf.Max(0.1f, speed))`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.
- Unity console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: User reported Ariel A is deleted shortly after firing.
- 2026-04-30: Code Builder changed Ariel A projectile lifetime to respect the configured projectile lifetime minimum.
- 2026-04-30: User reported `White Judgement` was not applying. Code Builder changed `TryTriggerArielJudgementLightExplosion()` to return a fired flag and made `UpdateProjectiles()` trigger/cleanup the pending Ariel explosion immediately when the marked projectile hits an enemy, while keeping lifetime-expiry fallback.

## Task: Ariel Judgement Light Projectile Runtime

### Task title

Implement Ariel A as a held/click direction Holy projectile with enhancement effects.

### Goals

- Keep Ariel A in the existing primary fire path, respecting shot cooldown, magazine, reload, and held input behavior.
- Implement pierce, magazine/reload traits, Holy damage bonuses, final-shot explosion, and Holy Exposure master behavior.
- Reuse shared projectile collision and damage calculation paths.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run without explicit user permission.

### Role Owner

Code Builder

### Status

Implemented and locally validated. Code Reviewer returned FAIL for Ariel A projectile mismatches, and Code Builder has applied the requested correction pass.

### Next Actions

- User verifies Ariel A held fire, magazine/reload, pierce, explosion, and Holy Exposure behavior in Play Mode.

### Evidence

- `CombatRuntimeProjectiles.FirePrimarySkill()` routes Ariel to `FireManualArielJudgementLight(direction)`.
- `CombatRuntimeArielSkills.cs` creates `JudgementLight_*` projectiles with `SkillId = "ariel-a"`, `DamageAttribute.Holy`, Ariel A skill damage/range, projectile speed `17`, computed lifetime, hit radius, and selected pierce.
- Projectile hit resolution applies Ariel final damage, flat Holy defense reduction, critical chance, and critical damage bonuses.
- `ariel-a-master-2` applies Holy Exposure on projectile hit.
- `ariel-a-master-1` triggers two area Holy explosions on the last shot.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Follow-up fix stores `ariel-a-master-1` explosion data on the last projectile and triggers it from `CombatRuntimeProjectiles.cs` when that projectile is cleaned up after hit or lifetime expiry.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.
- Follow-up Unity refresh completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: Code Builder implemented Ariel Judgement Light as a real projectile path based on `a-judgement-light.md`.
- 2026-04-30: User instructed Builder to fix Code Reviewer findings; Builder corrected Ariel A skill data usage and last-shot explosion timing.

## Task: Hold Input Primary Skill Fire

### Task title

Allow primary projectiles to repeat while pointer input is held.

### Goals

- Make left mouse hold and touch hold keep requesting primary projectile fire.
- Preserve existing projectile spawn, movement, hit detection, and cleanup behavior.
- Preserve existing shot interval, magazine, and reload behavior.

### Constraints

- Role Owner is Code Builder.
- Ground claims in actual code and command output.
- User performs Play Mode verification.
- Code Reviewer was not run without explicit permission.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies that held input creates repeated A-skill projectiles at the configured interval and that release stops new firing.

### Evidence

- `CombatRuntimeHud.cs` now keeps `fireRequestedThisFrame` true while mouse left button or touch is held.
- `CombatRuntimeProjectiles.cs` uses that fire request in `UpdateSelectedMonsterCombat()` and still blocks firing during reload or shot cooldown.
- `FirePrimarySkill()` remains the shared path for non-Eve Monster A projectiles, while Eve still routes to `FireManualEveArcBolt(direction)`.
- `git diff --check -- Pakuri\Assets\Scripts\Combat\CombatRuntimeHud.cs` returned exit code 0 with only CRLF warning output.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity console error query returned MCP-FOR-UNITY handler logs only, not project compile errors.

### History

- 2026-04-30: User requested that holding left mouse click or mobile touch continuously fires A skills toward the current click/touch position.
- 2026-04-30: Code Builder changed the input request generation and left projectile runtime behavior unchanged.

## Task: Enemy Projectile And Overhead HP Display

### Task title

Ranged enemies use projectiles and enemies show simple overhead name/HP labels.

### Goals

- Ranged enemies should no longer damage the Monster or Nexus immediately at attack time.
- Enemy projectiles should apply HP damage only after touching the Monster or Nexus target.
- Enemies should show a simple overhead name and HP text.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- All claims must be grounded in actual files and command output.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that Archer/Rogue/Hero Karin style ranged attackers spawn visible enemy projectiles.
- User verifies Monster/Nexus HP changes only when enemy projectiles reach the target.
- User verifies enemy overhead labels remain readable enough and update HP after taking damage/healing.

### Evidence

- `EveVerticalSliceController.cs` `ProjectileRuntime` now has enemy projectile fields: source enemy, target transform, and Monster/Nexus target flag.
- `TryUseStageOneEnemySkill()` now routes `EnemyAttackType.Ranged` and `EnemyAttackType.MeleeAndRanged` default attacks through `FireEnemyProjectile()`.
- `UpdateProjectiles()` now branches enemy projectiles into `TryHitEnemyProjectileTarget()`, which applies Monster or Nexus damage only on collision.
- `SpawnEnemy()` now creates an overhead `TextMesh` through `CreateEnemyLabel()`, and `UpdateEnemyLabel()` writes enemy name and current/max HP.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query returned MCP-FOR-UNITY transport/client handler entries only, not project script compile errors.

### History

- 2026-04-27: User requested ranged enemy projectiles with collision-based HP damage and simple overhead enemy name/HP display.
- 2026-04-27: Implemented enemy projectile runtime path and overhead TextMesh labels in `EveVerticalSliceController.cs`.

## Task: Eve Projectile Click Hold Compliance Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/dungeon-squad-combat-player-controls.md`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`, `wasPressedThisFrame`, `GetMouseButtonDown(0)`.
- Legacy non-English note retained these code references: `Pakuri/reference/eve-projectile-click-hold-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-combat-player-controls.md`, `a-arc-bolt.md`, `combat-attribute-and-damage-system.md`, `EveVerticalSliceController.cs`, `eve-combat-implementation-report.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/eve-projectile-click-hold-plan.html`.

## Task: Eve Projectile Click Implementation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Completed without Code Review. External reviewer commands timed out again, so only Builder-side validation was performed.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`, `ProjectileRuntime`, `projectileRoot`, `UpdateProjectiles()`, `TryHitEnemy()`, `HandlePointerInput()`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scenes/SampleScene.unity`, `ProjectileRoot`.
- Legacy non-English note retained these code references: `manage_scene save`, `Assets/Scenes/SampleScene.unity`.
- Legacy non-English note retained these code references: `find_gameobjects by_name ProjectileRoot`, `ProjectileRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
  - Legacy non-English note retained these code references: `projectileCount = 1`.
  - Legacy non-English note retained these code references: `projectileCount = 0`.
  - Legacy non-English note retained these code references: `enemyHealth = 37.95`.
  - Legacy non-English note retained these code references: `currentShotsRemaining = 0`, `reloadRemaining = 4.0`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Screenshots/eve-projectile-click-runtime.png`.
- Legacy non-English note retained these code references: `validate_script`.
- Legacy non-English note retained these code references: `read_console`, `FindObjectOfType<Camera>()`, `FindFirstObjectByType<Camera>()`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
  - `codex review --uncommitted` timeout
  - Legacy non-English note retained these code references: `codex exec`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `eve-projectile-click-hold-plan.html`, `a-arc-bolt.md`, `dungeon-squad-combat-player-controls.md`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `ProjectileRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `FireArcBolt()`.
- Legacy non-English note retained these code references: `FindFirstObjectByType<Camera>()`.
- Legacy non-English note retained these code references: `Pakuri/reference/eve-projectile-click-implementation-report.html`.
- Legacy non-English note retained these code references: `codex review --uncommitted`, `codex exec`.

## Task: Eve Arc Branch And DebugScene Skill Toggle Runtime

### Task title

Narrow Eve Arc Bolt extra projectile spread, implement immediate lightning branch damage, and add DebugScene skill-toggle testing UI.

### Goals

- Reduce the extra projectile spread angle for Eve A Arc Bolt.
- Change Eve A lightning branch semantics from status chance to immediate branch damage on hit.
- Draw a thin straight rectangular lightning line from the hit enemy to each branch target.
- Add a DebugScene-only controller under `Assets/Scenes/DebugScene.unity` that can test the 5 monster assets and toggle skills A-J plus enhancement/master effects.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Do not run external Code Reviewer unless the user explicitly grants permission.
- The current data model has `SkillSlot` A-J only; there is no K enum value. DebugScene shows K as a disabled no-data toggle rather than inventing runtime data.
- Preserve unrelated existing worktree changes from previous Eve runtime and report tasks.

### Role Owner

Code Builder

### Status

Builder correction pass completed for the prior `eve-a-master-1` findings, DebugScene UI flow was reworked to match the newer user request, and a follow-up SkillDebugPanel visibility fix was applied. A later Builder pass changed `DebugSceneController` toward scene-bound editable UI and saved static skill/choice toggle slots into `DebugScene.unity`. User then instructed Builder to restore `DebugSetupPanel` and setup controls; those scene paths were restored and build/console validation passed. The later root-scale finding was fixed by serializing the `DebugSceneController` root `RectTransform` scale as `{1,1,1}` and by guarding only the zero-scale case in `EnsureCanvasShell()`, while external Code Reviewer execution is deferred until user permission.

### Next Actions

- User Play Mode verifies `DebugScene` because Codex does not run Unity-MCP Play Mode gameplay verification.
- Run external Code Reviewer only after explicit user permission.

### Evidence

- `Pakuri/Assets/Scripts/Data/SkillDefinition.cs` defines `SkillSlot` A through J only; no K slot exists.
- `Pakuri/Assets/Data/GameData/Monsters/eve.asset` has `eve-a-trait-5` with branch chance text and `eve-a-master-1` with branch circuit text.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` now uses `EveArcExtraProjectileAngleStep = 3f` instead of the previous 4-degree spread step.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now applies branch damage immediately after an Eve projectile hit, selects nearby targets within branch radius, and creates line visuals through `CreateEveArcBranchLine`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` was added to create a DebugScene-only uGUI panel with 5 monster buttons, skill toggles, enhancement/master toggles, and immediate `RunSession` restart into `CombatRuntimeController.BeginConfiguredDay(...)`.
- `Pakuri/Assets/Scenes/DebugScene.unity` now contains a `DebugSceneController` object wired to `Assets/Data/GameData/GameDataCatalog.asset` and `CombatRoot`.
- In `Pakuri/Assets/Scenes/DebugScene.unity`, the duplicated `RunSceneBootstrap` and `RunCombatUiController` components are serialized disabled to keep DebugScene separate from the RunScene flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding 1: `eve-a-master-1` branch damage is implemented as 100% in `CombatRuntimeEveSkills.cs`, but `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` says branch damage is 60%.
- Reviewer finding 2: `eve-a-master-1` does not add the documented magazine +2; `GetEveArcMagazineCapacity()` currently only applies `eve-a-trait-1` +4.
- User approved the Reviewer findings. `CombatRuntimeEveSkills.cs` now sets `eve-a-master-1` branch damage multiplier to `0.60f`, and `GetEveArcMagazineCapacity()` now adds `+2` for `eve-a-master-1`.
- User clarified DebugScene flow: starting DebugScene must not spawn enemies; user selects monster, opens skill debug, toggles A-J, chooses enhancement effects in a separate closable UI, then presses Start to spawn enemies.
- `CombatRuntimeController.cs` now exposes `ApplyDebugSelection(...)` so DebugScene can update selected monster/skill state without calling `BeginPrototypeDay(...)` or spawning enemies.
- `DebugSceneController.cs` now keeps monster selection and skill/enhancement configuration separate from `StartCombat()`, uses `BeginConfiguredDay(...)` only when Start is pressed, and uses `ApplyDebugSelection(...)` for pre-start or mid-combat debug changes.
- `DebugSceneController.cs` now disables passive F unless active A is checked; unchecking A also clears F.
- `DebugSceneController.cs` now opens an enhancement modal when a skill/passive is checked, and the modal has a close button.
- `DebugSceneController.cs` ignores prisoner reward UI entirely; after combat resolves, the existing `CombatRuntimeController` stays resolved until the DebugScene Start button is pressed again.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY transport/client handler logs, not project compile errors.
- User reported the SkillDebugPanel skill list was not visible.
- `DebugSceneController.OpenSkillWindow()` now activates `SkillDebugPanel` before rebuilding its toggles.
- `DebugSceneController.RebuildSkillToggles()` and `OpenEnhancementModal()` now call `RefreshToggleContentHeight(...)` after creating toggles so ScrollRect content has a concrete height.
- `DebugSceneController.EnsureToggle(...)` now assigns fixed toggle anchors/pivot plus `LayoutElement` min/preferred height, and `EnsureScrollContent(...)` assigns fixed scroll viewport `LayoutElement` height.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested after the SkillDebugPanel fix; follow-up refresh returned `resulting_state=idle`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer command was attempted for the latest DebugScene changes, but Codex CLI exited with usage-limit errors and did not return a review verdict.
- User reported the SkillDebugPanel skill list still did not appear and requested editable scene UI instead of UI generated during game execution.
- `DebugSceneController.cs` was changed so `Awake()` calls `BindSceneUi()` instead of `BuildUi()`, and the controller now binds buttons/toggles from scene object paths instead of creating panels/toggles at runtime.
- Unity Edit Mode code saved static toggle objects in `Pakuri/Assets/Scenes/DebugScene.unity`: `Active_A` through `Active_E`, `Passive_F` through `Passive_J`, and `Choice_01` through `Choice_08`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed with editor state `ready_for_tools=true`; Unity console error query showed only MCP-FOR-UNITY client handler logs, not project compile errors.
- Required external Code Reviewer returned `REVIEW_RESULT: FAIL`.
- Reviewer finding 1: `DebugSceneController.cs` line 156 requires direct child `DebugSetupPanel`, but `DebugScene.unity` line 8213 shows only `SkillDebugPanel` and `EnhancementModal` under `DebugSceneController`; `Select-String` found no `m_Name: DebugSetupPanel`.
- Reviewer finding 2: `DebugSceneController.cs` line 166 expects `Title`, `Status`, `CombatText`, `MonsterButtons`, `SkillWindowButton`, and `StartButton` under `DebugSetupPanel`, but scene search found those setup paths absent.
- User instructed Code Builder to restore `DebugSetupPanel` and setup controls into the scene and re-run build validation.
- Unity Edit Mode code restored and saved scene objects for `DebugSetupPanel`, `DebugSetupPanel/Title`, `DebugSetupPanel/Status`, `DebugSetupPanel/MonsterButtons`, `DebugSetupPanel/SkillWindowButton`, `DebugSetupPanel/StartButton`, and `DebugSetupPanel/CombatText`.
- The same scene save pass also ensured `SkillDebugPanel/SkillScroll/Viewport/Content`, `EnhancementModal/ChoiceScroll/Viewport/Content`, A-J skill toggles, and `Choice_01` through `Choice_08` exist as editable scene objects.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the previous `DebugSceneController requires DebugSetupPanel...` project error.
- Required external Code Reviewer verified the requested scene paths now exist, but returned `REVIEW_RESULT: FAIL`.
- Latest Reviewer finding: `DebugScene.unity` line 10104 stores `DebugSceneController` root `RectTransform` with `m_LocalScale: {x: 0, y: 0, z: 0}`; since child UI is parented under this root, the UI can remain visually collapsed/non-interactive.
- User later instructed that Code Reviewer must be run only with user permission.
- `Pakuri/Assets/Scenes/DebugScene.unity` now stores the `DebugSceneController` root `RectTransform` with `m_LocalScale: {x: 1, y: 1, z: 1}`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` now restores `transform.localScale` only when it is exactly `Vector3.zero`, preserving non-zero user-edited UI scale and position.
- `Select-String` confirmed `DebugSetupPanel`, `SkillDebugPanel`, `EnhancementModal`, `Active_A` through `Active_E`, `Passive_F` through `Passive_J`, and `Choice_01` / `Choice_08` are present in `Pakuri/Assets/Scenes/DebugScene.unity`.
- Unity read-only `execute_code` confirmed `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content` has children `Active_A,Active_B,Active_C,Active_D,Active_E,Passive_F,Passive_G,Passive_H,Passive_I,Passive_J`.
- Unity `mcpforunity://scene/gameobject/65632` showed the loaded `DebugSceneController` transform scale and lossyScale are `{1,1,1}`.
- User reported that `Content` child skill toggles were clickable but their descriptions and checkmark were invisible.
- `DebugSceneController.ConfigureToggle(...)` now calls `ConfigureToggleVisuals(...)` to rebuild each scene-bound toggle slot's `Background`, `Checkmark/Glyph`, and `Label` visuals every time the slot is bound.
- `DebugSceneController.ConfigureToggleVisuals(...)` uses a separate `Checkmark/Glyph` child `Text` as the Toggle graphic, because the existing `Checkmark` object already has an `Image` graphic and Unity did not add a second `Text` graphic to the same GameObject in the runtime inspection.
- Legacy non-English note retained these ASCII code references: `execute_code`, `Active_A`, `toggle.graphic=Text:Checkmark/Glyph`, `labelAlpha=1`.
- Runtime Unity missing-script inspection returned `missingTotal=0`; the visible console still contained older `The referenced script (Unknown) on this Behaviour is missing!` entries with no file/line.
- User reported the Label skill text and checkbox were still not visible. Builder replaced the Text-glyph checkmark approach with Unity built-in `UISprite` and `Checkmark` sprites in `DebugSceneController.ConfigureToggleVisuals(...)`.
- Legacy non-English note retained these ASCII code references: `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content`, `Active_A`, `Passive_J`, `DebugScene.unity`, `labelAlpha=1`, `bgSprite=UISprite`, `checkSprite=Checkmark`, `toggleGraphic=Checkmark`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed with editor state `ready_for_tools=true`; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the previous `DebugSceneController requires DebugSetupPanel...` project error.
- User reported `Failed to find UI/Skin/UISprite.psd` from `DebugSceneController.ConfigureToggleVisuals(...)`.
- `Select-String` confirmed the old `UI/Skin` and `GetBuiltinResource<Sprite>` calls were removed from `Pakuri/Assets/Scripts/Run/DebugSceneController.cs`; the only sprite load is now `Resources.Load<Sprite>("DebugUiSolid")`.
- `Pakuri/Assets/Resources/DebugUiSolid.png` was created as a project-owned 1x1 Sprite resource, avoiding Unity built-in UI skin paths.
- Unity Edit Mode scene save updated the actual `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content` slots so `Active_A` through `Passive_J` remain editable scene objects and their `Background` / `Background/Checkmark` images use `DebugUiSolid`.
- Legacy non-English note retained these ASCII code references: `execute_code`, `resourceSprite=DebugUiSolid`, `contentCount=10`, `labelAlpha=1`, `bgSprite=DebugUiSolid`, `checkSprite=DebugUiSolid`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- Unity refresh/compile completed with `resulting_state=idle`; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the `Failed to find UI/Skin/UISprite.psd` project error.
- User requested the same visible/editable rebuild for `EnhancementModal` children.
- Unity read-only `execute_code` first confirmed `EnhancementModal/ChoiceScroll/Viewport/Content` had 8 choices but `Choice_01` had `bgSprite=null` and `checkSprite=null`.
- Unity Edit Mode code deleted all existing children under `DebugSceneController/EnhancementModal`, recreated `Title`, `Summary`, `CloseButton`, `ChoiceScroll/Viewport/Content`, and `Choice_01` through `Choice_08`, and saved `Assets/Scenes/DebugScene.unity`.
- Unity read-only `execute_code` then confirmed `modalActive=False`, `title=Enhancements`, `closeButton=True`, `count=8`, `choice01Label=Choice Slot 01`, `labelAlpha=1`, `bgSprite=DebugUiSolid`, and `checkSprite=DebugUiSolid`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings after the scene rebuild.
- Unity refresh completed with `resulting_state=idle`; Unity console error query showed only MCP-FOR-UNITY client handler logs.

### History

- 2026-04-29: User requested narrower extra projectile angle, immediate lightning branch damage/visuals, and a separate DebugScene for testing monster skills and enhancements through toggles.
- 2026-04-29: Code Builder inspected actual combat scripts, scene files, monster assets, and the SkillSlot enum before editing.
- 2026-04-29: Code Builder implemented branch projectile fields, immediate branch damage, line visuals, DebugScene controller script, and DebugScene scene wiring.
- 2026-04-29: External Code Reviewer found two `eve-a-master-1` spec mismatches and returned `NEEDS_CHANGES`; Builder paused per AGENTS.md.
- 2026-04-30: User approved the prior Reviewer findings and then clarified the desired DebugScene UI flow.
- 2026-04-30: Code Builder fixed `eve-a-master-1` branch damage and magazine size, added `CombatRuntimeController.ApplyDebugSelection(...)`, and rewrote `DebugSceneController` so enemies spawn only from the DebugScene Start button.
- 2026-04-30: User reported the SkillDebugPanel skill list was not visible; Code Builder updated the panel activation order and ScrollRect/LayoutElement sizing, then rebuilt and checked Unity console.
- 2026-04-30: Code Builder attempted the required external Code Reviewer pass, but Codex CLI reported a usage limit and no verdict was produced.
- 2026-04-30: User reported the SkillDebugPanel issue persisted and requested editable scene UI rather than game-run generated UI. Code Builder changed the controller toward scene-bound UI and saved static toggle slots, but the required external Code Reviewer found missing `DebugSetupPanel` setup controls and returned `FAIL`; Builder paused per AGENTS.md.
- 2026-04-30: User instructed Builder to restore `DebugSetupPanel` and setup controls. Builder restored those scene objects and validated build/console, then external Reviewer found the root scale `{0,0,0}` scene issue and returned `FAIL`; Builder paused per AGENTS.md.
- 2026-04-30: User instructed Builder to fix the actual Content skill-list visibility and preserve user-edited UI Scale/Position, and also instructed that Code Reviewer execution now requires user permission. Builder fixed the serialized root scale, kept only a zero-scale runtime guard, rebuilt, refreshed Unity, checked the console, and did not run Code Reviewer.
- 2026-04-30: User reported the `Content` child skill descriptions and checkmarks were still invisible while clicks worked. Builder changed toggle visual binding to normalize scene-bound Label/Background/Checkmark/Glyph elements, applied the same normalization to the current runtime instance, rebuilt, checked Unity console/missing-script state, and did not run Code Reviewer.
- 2026-04-30: User reported the Label skill text and checkbox were still invisible. Builder switched checkboxes to built-in Unity UI sprites, saved the actual scene slots in Edit Mode, rebuilt, refreshed Unity, checked console, and did not run Code Reviewer.
- 2026-04-30: User reported `Failed to find UI/Skin/UISprite.psd` and asked to rebuild `SkillDebugPanel` as visible editable scene UI. Builder removed built-in UI skin sprite usage, created project-owned `Assets/Resources/DebugUiSolid.png`, saved the scene toggle visuals against that sprite, rebuilt, refreshed Unity, checked console, and did not run Code Reviewer because user permission was not granted.
- 2026-04-30: User requested the same rebuild for `EnhancementModal` children. Builder deleted and recreated the modal children as editable scene uGUI objects using `DebugUiSolid`, verified the first choice label/checkmark state, rebuilt, refreshed Unity, checked console, and did not run Code Reviewer because user permission was not granted.

# Task: 2026-05-04 Rin A Projectile And Extra Elemental Damage

## Task title

Add Rin Shattering Fist projectile behavior and elemental extra-damage hooks.

## Goals

- Route Rin A through a Rin-specific physical projectile path.
- Apply Rin A trait/master projectile effects, including pierce, magazine/reload/interval changes, critical bonuses, and Thunder Gauntlet extra lightning.
- Base elemental extra damage on the source physical final damage as clarified by the user.

## Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

## Role Owner

Code Builder

## Status

Implemented, Reviewer findings fixed, and locally validated. Code Reviewer has not been rerun because the user did not request another review.

## Next Actions

- User verifies Rin A projectile and Thunder Gauntlet behavior in Play Mode.
- Run another Code Reviewer pass only if the user explicitly requests it.

## Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:52` calls Rin projectile-hit handling with the actual `appliedDamage` returned by `ApplyDamageToEnemy(...)`.
- `CombatRuntimeProjectiles.cs:198` multiplies Rin final-damage modifiers into shared projectile damage resolution, and `:204`/`:205` add Rin critical chance/multiplier bonuses.
- `CombatRuntimeProjectiles.cs:443` routes selected Rin A fire to `FireManualRinShatteringFist`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:77` creates Rin A projectiles, `:445` handles Rin A master 2 hit behavior, and `:507` applies extra elemental damage from the source physical damage actually applied to the enemy.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only existing Unity/MCP warnings.

## History

- 2026-05-04: Code Builder added Rin A projectile routing and Rin elemental extra-damage handling.
- 2026-05-04: Code Builder fixed the Reviewer-reported Rin A follow-up issue by passing applied projectile damage into Thunder Gauntlet and chain handling.

## Task: 2026-05-06 Sein Passive Projectile Hooks

### Task title

Add Sein F-J passive projectile modifiers and Flame Barrage proc routing.

### Goals

- Apply Sein passive fire damage, critical chance, critical multiplier, and fire-defense reduction in projectile damage resolution.
- Route Sein G auto Blazing Volley passive procs from fire projectile hits with an internal cooldown.
- Keep projectile behavior tied to selected-Monster skill ownership and actual hit events.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Sein G procs only from fire damage hits and respects its cooldown.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now includes Sein final-damage, critical chance, critical multiplier, flat fire-defense reduction, and projectile-hit tracking hooks in shared projectile resolution.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` implements `TryTriggerSeinFlameBarrageProc(...)`, `FireSeinFlameBarrageProc(...)`, and passive helper checks for `sein-f` and `sein-g`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` defines the G traits used by the proc chance, proc power, and Scorching Arrow reload reduction logic.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-06: Code Builder added projectile-path support for Sein F fire/crit modifiers and Sein G auto Blazing Volley procs while implementing Sein passive skills F-J.

## Task: 2026-05-07 Sein Active Projectile Corrections

### Task title

Correct Sein active projectile and line targeting behavior.

### Goals

- Support locked-target player projectiles for Sein C.
- Keep A explosion damage from excluding the original projectile target.
- Change B volley output from angled fan spread to same-direction magazine fire with separate ammo/reload state.
- Change E line damage from beam area checks to one target per sky-origin line.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein projectile visuals and target-only E behavior in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` added `LockedEnemyTarget`, `SeinExplodesOnLockedTarget`, `SeinExplosionRadius`, and `SeinExplosionDamageMultiplier` to `ProjectileRuntime`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now calls `UpdateSeinLockedTargetProjectile(...)` and resolves locked projectiles only against their assigned enemy.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now launches C through `FireSeinFlameTrajectoryProjectile(...)`, creates E sky-origin lines through `CreateSeinDoomsdayTargetLine(...)`, and includes the A explosion target.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- `git diff --check` completed with no whitespace errors; it reported only CRLF conversion warnings.

### History

- 2026-05-07: User clarified that Sein C should feel like a target-locked projectile and E should start from the sky and damage only target enemies.

## Task: 2026-05-07 Sein C Projectile Follow-up Correction

### Task title

Track Sein C projectile contact delay and moving path segment behavior.

### Goals

- Route Sein C projectile contact into a delayed explosion effect instead of immediate damage/explosion.
- Create `Piercing Trajectory` path segments as the projectile moves from previous position to current position.
- Avoid full-path line damage at cast time.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies C projectile travel, contact delay, and moving path trail in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:25` stores projectile previous position before movement.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:29` calls `CreateSeinFlameTrajectoryPathSegment(...)` after movement.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:52` calls `TryHandleSeinFlameTrajectoryImpact(...)` before normal projectile damage application.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:484` implements moving path segment visuals/damage for `sein-c-master-2`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:529` implements delayed impact handling for C contact.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: User clarified C should explode after contact delay and leave path while flying, not draw/apply the path immediately.
- 2026-05-07: Code Builder implemented travel-time C path segments and delayed impact handling.

## Task: 2026-05-07 Vega Three-Sword Projectile Runtime

### Task title

Track Vega A three-sword projectile and mark application behavior.

### Goals

- Queue three Vega A sword projectiles per manual magazine shot.
- Apply Vega `이름표식` from projectile hits.
- Route Vega projectile final damage modifiers through shared projectile damage resolution.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega A projectile cadence, piercing behavior, and mark stacks in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs` defines queued Vega pending shots, `FireManualVegaThreeSwordFlurry(...)`, `SpawnVegaSwordProjectile(...)`, and `HandleVegaProjectileHit(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` calls Vega projectile hit tracking and includes Vega damage/critical hooks in shared projectile damage resolution.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: Code Builder implemented Vega A projectile runtime as part of Vega A-E active skill implementation.

## Task: 2026-05-07 Vega Projectile Sprite Runtime Source Fix

### Task title

Record Vega projectile visual source correction for the selected-Monster projectile path.

### Goals

- Ensure Vega A projectile rendering uses the intended selected projectile sprite.
- Record that the projectile path itself was correct and the defect was in runtime data source alignment.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega projectile sprite appearance in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs:163` assigns `selectedProjectileSprite = monster.ProjectileSprite`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:396` renders Vega projectile sprites from `selectedProjectileSprite` before falling back to `GetSharedSprite()`.
- `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.Build.cs:27` builds runtime `MonsterDefinition.ProjectileSprite` from `sourceMonster.ProjectileSpritePath`.
- Unity-MCP inspection showed the runtime `ProjectileSpritePath` was old before the fix and `Assets/Image/Monster/Vega/Vega_Shoot2.png` after `monsters.csv` was corrected and synced.

### History

- 2026-05-07: User reported Vega projectile visuals still showed the old sprite despite the SO projectile sprite assignment.
# Task: 2026-05-08 Manifested A Projectile Runtime

### Task title

Route Manifested projectile skills through projectile objects instead of line effects.

### Goals

- Use each Manifested monster's `ProjectileSprite` for `MagazineProjectile` / `CooldownProjectile` skills.
- Keep Manifested projectile movement and X-boundary cleanup compatible with the existing player projectile update loop.
- Avoid applying selected 1P monster projectile labels and passive modifiers to Manifested projectile hits.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Do not run Unity Play Mode from Codex.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested A projectile visuals and cleanup in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:124` adds `IsManifestedProjectile` and Manifested source label fields to `ProjectileRuntime`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:50` branches Manifested projectiles before the selected-monster `TryHitEnemy(...)` path.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:465` creates Manifested projectile GameObjects with `runtime.Monster.ProjectileSprite`.
- `CombatRuntimeParty.cs:552` resolves Manifested projectile hit damage through a dedicated method and `ApplyDamageToEnemy(...)`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported Manifested monsters looked like they were firing abnormal thin beam skills instead of A projectiles.
- 2026-05-08: Code Builder added a Manifested projectile branch to the shared projectile update loop.

# Task: 2026-05-08 Manifested Vega A Projectile Burst

### Task title

Add Manifested Vega A three-projectile burst behavior.

### Goals

- Make Manifested Vega A fire three sword projectiles per magazine shot instead of one generic projectile.
- Preserve projectile object movement, hit detection, and X-boundary cleanup through the shared projectile loop.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Vega A projectile cadence and hit behavior in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:728` identifies `vega-a` as the Manifested three-sword skill.
- `CombatRuntimeParty.cs:733` queues three projectiles for Manifested Vega A.
- `CombatRuntimeParty.cs:769` fires queued projectiles through the Manifested projectile object path with infinite pierce and per-shot damage multiplier.
- `CombatRuntimeParty.cs:774` uses `VegaThreeSwordBulletInterval` for the internal projectile spacing.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported Manifested Vega A was not using the registered three-projectile basic attack behavior.
- 2026-05-08: Code Builder added the Manifested Vega A burst queue.
