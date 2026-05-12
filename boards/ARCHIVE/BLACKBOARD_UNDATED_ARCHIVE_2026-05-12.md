# BLACKBOARD_UNDATED_ARCHIVE_2026-05-12

- Created: 2026-05-12
- Purpose: 7-day grouped archive for moved `boards/**/*BLACKBOARD.md` task blocks.
- Source task blocks are grouped by their original source file path.

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Ariel A Lifetime And Shield Bar Split Visual

### Task title

Fix Ariel A early projectile cleanup and change shield HP bars to fixed-width split segments.

### Goals

- Ariel A projectiles should not disappear after roughly half a second because of range/speed lifetime calculation.
- Shield visuals should share one fixed HP bar width with red HP and white shield segments.
- Preserve existing HP and shield numeric state.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the latest request did not explicitly permit it.
- Codex did not control Play Mode; Unity editor state showed Play Mode active.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel A projectile lifetime and shield bar ratio behavior in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` now keeps Ariel A lifetime at least `projectileLifetimeConfigured`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now updates HP and shield as adjacent bar segments with `health + shield` as the visual total while shield is present.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` now applies the split bar helper to the selected Monster status bar.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same warnings.
- Unity console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-04-30: User requested fixing Ariel A early deletion and changing all Monster shield-granting skills to a League-style single HP/shield bar visual.
- 2026-04-30: Code Builder implemented the projectile lifetime and shared shield bar visual changes.
- 2026-04-30: User requested HP Bar background black and reported Ariel A master `White Judgement` was not visibly applying. Code Builder changed shared HP bar backgrounds to `Color.black`, changed pending Ariel judgement explosions to trigger immediately on enemy hit, and made the explosion use a longer, higher-sorting circle-sprite visual.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Ariel Combat Runtime Skill Integration

### Task title

Integrate Ariel active/passive runtime behavior into the shared combat controller.

### Goals

- Add Ariel-specific skill runtime without changing Eve-specific skill behavior intentionally.
- Keep shared primary fire, cooldown, reload, projectile, shield, and status display paths working for both Eve and Ariel.
- Treat document "all allies" effects as selected-Monster effects until the runtime has an actual ally collection.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run without explicit user permission.

### Role Owner

Code Builder

### Status

Implemented and locally validated. Code Reviewer returned FAIL for Ariel behavior mismatches, and Code Builder has applied the requested correction pass.

### Next Actions

- User verifies Ariel skill behavior in Play Mode.
- If a future party/allied unit runtime is introduced, expand Ariel party-wide shield/blessing logic to that runtime collection.

### Evidence

- `CombatRuntimeArielSkills.cs` adds Ariel cooldown timers, A manual projectile, B/E shield application, C area damage/blessing, D Holy Exposure, E battlefield-wide damage, and F-J passive helpers.
- `CombatRuntimeProjectiles.cs` now calls selected-monster generic cooldown/action-speed/magazine helpers and routes Eve/Ariel A skills separately.
- `CombatRuntimeProjectiles.cs` now applies Ariel incoming damage reduction from E master 1 before selected Monster shield/HP damage.
- `CombatRuntimeEnemies.cs` now decrements and resolves Holy Exposure expiry and includes Holy Exposure in enemy status labels.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity refresh completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.
- Follow-up fixes addressed Reviewer findings: Ariel A uses skill A damage/range and projectile speed `17`; Ariel A master explosion is tied to projectile cleanup; Radiant Shield reflection receives the source enemy; and Holy damage bonuses are only applied in final damage calculation.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.
- Follow-up Unity refresh completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: Ariel A-E/F-J runtime integration was implemented after reading the actual Ariel skill documents and current combat partial scripts.
- 2026-04-30: User instructed Builder to fix Code Reviewer findings; Builder applied the Ariel correction pass and did not rerun Code Reviewer because a new review was not explicitly requested.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Hold Input Primary Skill Fire

### Task title

Use held pointer input for primary Monster combat firing.

### Goals

- Convert combat pointer input from press-once to held-input behavior.
- Keep existing combat cooldown, magazine, reload, and projectile collision behavior unchanged.
- Support both mouse and touch input paths.

### Constraints

- Role Owner is Code Builder.
- Ground claims in actual files and command output.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because the user did not explicitly request it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies held mouse/touch behavior in RunScene or DebugScene Play Mode.

### Evidence

- `CombatRuntimeHud.cs` changed from `Mouse.current.leftButton.wasPressedThisFrame` and `Input.GetMouseButtonDown(0)` to held input checks.
- `CombatRuntimeHud.cs` added Input System touchscreen primary-touch support and legacy `Input.touchCount` / `Input.GetTouch(0)` support.
- `CombatRuntimeProjectiles.cs` still controls repeated firing through `shotCooldown`, `currentShotsRemaining`, `reloadRemaining`, and `FirePrimarySkill()`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing 2 Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity editor state returned `ready_for_tools=true`; console errors were MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: User requested hold-to-fire behavior for the current 5 Monster A skills and their left-button/touch active trigger behavior.
- 2026-04-30: Code Builder implemented the change in the shared combat input path.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Combat Runtime Controller Split

### Task title

Rename and split `EveVerticalSliceController` into role-based combat runtime scripts.

### Goals

- Rename `EveVerticalSliceController` to a role-accurate `CombatRuntimeController`.
- Preserve the existing RunScene component connection by moving the original `.meta` to `CombatRuntimeController.cs.meta`.
- Split the large combat controller into partial scripts by responsibility without intentionally changing gameplay behavior.
- Keep current RunScene combat, reward, enemy, projectile, and HUD flows compiling.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run Code Reviewer once only after implementation.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Completed. Builder implementation, local validation, one external Code Reviewer run, user confirmation for intentional scene marker position, and user Play Mode verification are done.

### Next Actions

- Continue with the next implementation task selected by the user.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` was 87,832 bytes before the split.
- `EveVerticalSliceController.cs` was replaced by `CombatRuntimeController.cs` plus role-based partial files: `CombatRuntimeScene.cs`, `CombatRuntimeEnemies.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeRewards.cs`, and `CombatRuntimeHud.cs`.
- `CombatRuntimeController.cs.meta` uses the original script guid `e1c1fbd89ef220a499bf601ceaf19ced`, preserving the existing Unity MonoScript asset identity for the renamed controller.
- `RunCombatUiController.cs`, `RunFlowController.cs`, and `RunSceneBootstrap.cs` now reference `CombatRuntimeController`.
- `RunScene.unity` now records `Assembly-CSharp::Pakuri.Combat.CombatRuntimeController` in the controller component `m_EditorClassIdentifier`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity script refresh completed; console error query returned only MCP-FOR-UNITY client handler exit logs, not project script compile errors.
- `git diff --check` for the changed runtime files returned exit code 0 with CRLF warnings only.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES` in `codex_loop_logs/combat_runtime_split_reviewer_20260428.md`.
- Reviewer findings 1-2 point at stage-basic spawn-rule changes that were already reviewed as `PASS` in `codex_loop_logs/stage_basic_spawn_reviewer_20260428.md`.
- `Select-String` confirmed current `RunScene.unity` stores `EnemySpawnPoint` local position at `{x: 34.39, y: 8, z: 0}`.
- User confirmed the `EnemySpawnPoint` position was manually adjusted and should not be treated as a required fix.
- User reported Play Mode worked without notable problems after the rename/split.

### History

- 2026-04-28: User requested doing roadmap step 1 first, renaming `EveVerticalSliceController` according to its purpose and splitting scripts by role.
- 2026-04-28: Code Builder renamed the controller to `CombatRuntimeController`, split the large file into role-based partial scripts, updated runtime references, and completed local validation.
- 2026-04-28: External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES`; Code Builder is waiting for user instruction instead of auto-fixing.
- 2026-04-28: User confirmed the `EnemySpawnPoint` position was manually adjusted, so the scene marker position finding is accepted as intentional.
- 2026-04-28: User reported Play Mode worked without notable problems; task marked completed.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Combat Visual Sprite Assignment

### Task title

Allow monster/enemy ScriptableObjects and RunScene battlefield background to use editable sprites.

### Goals

- Add editable unit/projectile sprite references to monster and enemy ScriptableObjects under `Assets/Data/GameData`.
- Use assigned monster sprites for the selected monster and its projectiles at runtime.
- Use assigned enemy sprites for enemy bodies and enemy projectiles at runtime.
- Let `RunScene` use an editable battlefield background sprite without forcing the user's manual `BattlefieldBackground` scale.
- Keep unit body `SpriteRenderer.color` values white so assigned unit sprites are not tinted.
- Keep projectile, HP bar, marker, camera background, and battlefield background sprite colors white.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation and local build/Unity console validation completed. User reported Play Mode verification completed. Unit, projectile, HP bar, marker, camera background, and battlefield background sprite color preservation was added. External Code Reviewer run was attempted but interrupted by the user and is not completed.

### Next Actions

- User assigns `UnitSprite` and `ProjectileSprite` on monster/enemy assets as needed.
- User assigns `BattlefieldBackgroundSprite` on `CombatRuntimeController` and adjusts `BattlefieldBackground` Transform Scale manually; keep `Auto Fit Battlefield Background To Field` off when manual scale should be preserved.
- Run Code Reviewer later if the user wants this visual-support change reviewed.

### Evidence

- `MonsterDefinition.cs` now exposes `UnitSprite` and `ProjectileSprite`.
- `EnemyDefinition.cs` now exposes `UnitSprite` and `ProjectileSprite`, and `CloneRuntimeCopy()` preserves both references.
- `CombatRuntimeScene.cs` now reads `MonsterDefinition.UnitSprite` and `MonsterDefinition.ProjectileSprite` into runtime selected sprite fields.
- `CombatRuntimeEnemies.cs` now uses `EnemyDefinition.UnitSprite` for enemy bodies and `EnemyDefinition.ProjectileSprite` for enemy projectiles, falling back to the generated shared sprite when no sprite is assigned.
- `CombatRuntimeProjectiles.cs` now uses the selected monster projectile sprite, falling back to the generated shared sprite when no sprite is assigned.
- `CombatRuntimeController.cs` now exposes `BattlefieldBackgroundAnchor`, `BattlefieldBackgroundSprite`, `BattlefieldBackgroundColor`, and `AutoFitBattlefieldBackgroundToField`.
- `CombatRuntimeScene.cs` now only rewrites `BattlefieldBackground.localScale` when `autoFitBattlefieldBackgroundToField` is true, so manual scale is preserved by default.
- `CombatRuntimeScene.cs` now applies `Color.white` to the selected monster body renderer.
- `CombatRuntimeEnemies.cs` now keeps enemy body renderer colors white in `UpdateEnemyColor()`.
- `CombatRuntimeProjectiles.cs` now applies `Color.white` to selected monster projectiles.
- `CombatRuntimeEnemies.cs` now applies `Color.white` to enemy projectiles and enemy HP bar background/fill sprites.
- `CombatRuntimeController.cs` now initializes marker and battlefield background color fields as `Color.white`.
- `CombatRuntimeScene.cs` now applies `Color.white` to the camera background and battlefield background renderer.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing 2 MCPForUnity/Unity reference warnings.
- Unity script refresh/compile was requested; console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.
- User reported Play Mode verification completed before the manual background scale fix.

### History

- 2026-04-28: User requested editable projectile images and monster images on `Assets/Data/GameData` enemy/monster SOs, plus an editable RunScene background image.
- 2026-04-28: Code Builder added sprite fields to monster/enemy definitions and wired runtime monster/enemy/projectile renderers to use them.
- 2026-04-28: User reported Play Mode verification completed but found `BattlefieldBackground` scale was forced on game start.
- 2026-04-28: Code Builder changed background auto-fit scaling to an opt-in serialized bool so manual `BattlefieldBackground` scale is preserved by default.
- 2026-04-28: User requested unit sprite colors stay white; Code Builder changed selected monster and enemy body renderers to keep `SpriteRenderer.color` white.
- 2026-04-29: User requested projectile, HP bar, marker, and background colors stay white; Code Builder changed those runtime color assignments to `Color.white`.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Stage Basic Enemy Spawn Rule Reset

### Task title

Reset RunScene enemy spawn positions to `stage-basic-rules.md`.

### Goals

- Treat the current RunScene battlefield as bottom-left `(0,0)` and top-right `(31,17)`.
- Treat `EnemySpawnPoint` X as `33`.
- Spawn normal enemies from X `33` with random Y in `0~17`.
- Spawn boss enemies from `(33,8)`.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run Code Reviewer once only after implementation.

### Role Owner

Code Builder

### Status

Builder implementation, local validation, and one Code Reviewer PASS completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Unity Play Mode that normal enemies spawn along Y `0~17` from X `33`, and bosses spawn near `(33,8)`.

### Evidence

- `Pakuri/reference/5.enemy/stage-basic-rules.md` says screen coordinates are `(0,0)` to `(31,17)`, default spawn X is `33`, normal monster Y is random `0~17`, and boss default point is `(33,8)`.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` previously serialized `enemySpawnYRange = new Vector2(6f, 10f)`.
- `SpawnEnemy()` previously used `enemySpawnAnchor.position` and applied the random Y range as an offset from `DefaultEnemySpawnPosition.y`.
- `EveVerticalSliceController.cs` now serializes `enemySpawnYRange = new Vector2(0f, 17f)`.
- `EveVerticalSliceController.cs` now defines `EnemySpawnX = 33f`, `BossSpawnY = 8f`, and `DefaultEnemySpawnPosition = new Vector3(EnemySpawnX, BossSpawnY, 0f)`.
- `ResolveEnemySpawnPosition(bool isBoss)` now forces X to `33`, uses Y `8` for bosses, and uses random Y from `enemySpawnYRange` for normal enemies.
- `Pakuri/Assets/Scenes/RunScene.unity` now stores `EnemySpawnPoint` at `{x: 33, y: 8, z: 0}` and `enemySpawnYRange: {x: 0, y: 17}`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity console error query returned only an MCP-FOR-UNITY client handler exit log, not a project script compile error.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: PASS` in `codex_loop_logs/stage_basic_spawn_reviewer_20260428.md`.

### History

- 2026-04-28: User requested enemy spawn rules reset based on `Pakuri/reference/5.enemy/stage-basic-rules.md`, treating the RunScene field as `(0,0)` to `(31,17)` and `EnemySpawnPoint` X as `33`.
- 2026-04-28: Code Builder updated `EveVerticalSliceController.cs` and `RunScene.unity` to match the document rules.
- 2026-04-28: External Code Reviewer one-shot review returned `REVIEW_RESULT: PASS`; Play Mode gameplay verification remains user-owned.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: EnemySpawnPoint Editable Position

### Task title

Allow scene-edited `CombatRoot/EnemySpawnPoint` position to persist when starting the game.

### Goals

- Stop runtime scene reference resolution from resetting an existing `EnemySpawnPoint` to the hardcoded default `(29, 8, 0)`.
- Keep default creation behavior for missing anchors.
- Make enemy spawn placement use the edited `EnemySpawnPoint` transform position, including vertical movement.

### Constraints

- Role Owner is Code Builder after Designer handoff.
- User explicitly requested no Code Reviewer stage for this task; proceed with self-review only.
- All claims must be grounded in actual code and command output.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User moves `CombatRoot/EnemySpawnPoint` in `RunScene`, starts Play Mode, and verifies the marker no longer returns to `(29, 8, 0)`.
- User verifies spawned enemies appear around the edited `EnemySpawnPoint` position.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` previously called `EnsureChild(enemySpawnAnchor, "EnemySpawnPoint", new Vector3(29f, 8f, 0f))`.
- `EnsureChild()` previously assigned `current.position = worldPosition` and `existing.position = worldPosition`, which reset existing anchors.
- Added `DefaultEnemySpawnPosition` and changed `EnsureChild()` so existing `current` or found children are returned without overwriting their position.
- `SpawnEnemy()` now starts from `enemySpawnAnchor.position` and applies the configured Y random range as an offset from the default spawn Y, so edited spawn point Y also affects spawn placement.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- External Code Reviewer command was started but the user interrupted it and instructed to proceed with self-review only.

### History

- 2026-04-28: User reported that editing `Combat Root/EnemySpawnPoint` in the scene is reverted when starting the game.
- 2026-04-28: Confirmed reset cause in `EveVerticalSliceController.ResolveSceneReferences()` and `EnsureChild()`.
- 2026-04-28: Changed existing anchor handling to preserve scene-authored positions and adjusted enemy spawn placement to use the anchor transform as the base position.
- 2026-04-28: Per user instruction, skipped Code Reviewer and kept only Builder self-review plus build verification.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Enemy Target Priority Monster First

### Task title

Enemy combat flow targets the selected Monster before the Nexus.

### Goals

- Enemies should move toward and attack the Monster before attacking the tower/Nexus.
- If the Monster HP reaches 0, enemies should fall back to the existing Nexus target and Nexus defeat flow.
- Keep the change grounded in the existing `EveVerticalSliceController` combat flow.

### Constraints

- Role Owner is Code Builder.
- User will run Play Mode verification.
- Do not claim gameplay verification; only build, Unity refresh, console check, and self-review are performed here.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that Stage 1 enemies approach the selected Monster first.
- User verifies that Monster HP decreases before Nexus HP, and Nexus starts taking damage only after Monster HP reaches 0.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` now calls `GetEnemyPriorityTarget()` in `UpdateEnemies()` before moving or attacking.
- `GetEnemyPriorityTarget()` returns `eveAnchor` while `unitCurrentHealth > 0f`, then falls back to `nexusAnchor`.
- Enemy damage skills now call `ApplyEnemyDamageToPriorityTarget()`, which subtracts from `unitCurrentHealth` first and from `nexusCurrentHealth` only after Monster HP is depleted.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query showed MCP-FOR-UNITY transport/client handler entries only; no project script compile error was returned.

### History

- 2026-04-27: User requested enemies attack Monsters before hitting the tower.
- 2026-04-27: Confirmed the existing `UpdateEnemies()` flow targeted only `nexusAnchor` and the existing damage function subtracted only `nexusCurrentHealth`.
- 2026-04-27: Changed enemy movement and damage target selection to prefer the Monster while alive, with Nexus fallback after Monster HP reaches 0.
- 2026-04-27: Follow-up self-review fixes applied. Enemy attacks now resolve through `EnemyAttackResolver`, Monster defenses are cloned into runtime target defenses, enemy critical passive bonuses are copied from enemy stats into runtime, and fallback Stage 1 enemy ScriptableObjects are cached with `HideFlags.DontSave`.
- 2026-04-27: Ranged and melee/ranged enemies now fire enemy projectiles. HP damage is resolved only when those projectiles collide with the Monster or Nexus. Enemies now create a simple overhead `TextMesh` label showing name and HP.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Combat Script Self-Review Fixes

### Task title

Fix self-review findings for Monster defense, enemy critical passives, God Script pressure, and fallback enemy allocation.

### Goals

- Apply Monster attribute defenses when enemies damage the Monster.
- Make enemy critical chance/damage passive fields participate in damage resolution.
- Reduce `EveVerticalSliceController` responsibility by moving enemy attack damage resolution into a helper.
- Avoid creating new fallback Stage 1 enemy ScriptableObjects on every combat initialization.
- Skip text-encoding changes because user confirmed current text is not an issue.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- All claims must be grounded in actual files and command output.

### Role Owner

Code Builder

### Status

Builder fixes and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that enemy hits against Monster now use Monster defense and that archer/rogue critical passives can affect damage.
- Future cleanup should continue splitting `EveVerticalSliceController`; current change only extracts enemy attack damage resolution.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/EnemyAttackResolver.cs`.
- `EveVerticalSliceController.cs` now stores `selectedMonsterDefenses`, clones `monster.Defenses`, and passes them to `EnemyAttackResolver.ResolveAgainstMonster`.
- Enemy runtime now copies `CriticalChanceBonus` and `CriticalMultiplierBonus` from `CombatStatBlock` deltas and existing Stage 1 passives add onto those fields.
- Fallback Stage 1 enemy creation now uses static `fallbackStageOneEnemyCache` and `HideFlags.DontSave`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity initially reported missing `EnemyAttackResolver`; `manage_asset import` for `Assets/Scripts/Combat/EnemyAttackResolver.cs` generated/imported the MonoScript asset, and the later console error query showed only MCP-FOR-UNITY client handler entries.

### History

- 2026-04-27: User asked to fix self-review findings in order, excluding the text-encoding item.
- 2026-04-27: Implemented enemy attack damage helper, Monster defense application, enemy critical passive participation, and fallback enemy cache.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Combat Monster Enemy Implementation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `combat-monster-enemy-implementation-plan.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Debug.Log`.

### Constraints

- Role Owner??Code Builder??
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `[CombatDamage]`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/CombatStatModels.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `FormulaLog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/SkillDefinition.cs`, `Pakuri/Assets/Scripts/Data/EnemyDefinition.cs`.
- Legacy non-English note retained these code references: `GameDataCatalog.cs`, `StageOneEnemies`, `MonsterDefinition.cs`, `PrimaryAttribute`, `BaseStats`, `Defenses`, `ActiveSkills`, `PassiveSkills`.
- Legacy non-English note retained these code references: `RunFlowController.cs`, `RunSceneBootstrap.cs`, `GameDataCatalog`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Debug.Log("[CombatDamage] ...")`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `Pakuri/Assets/Data/GameData/Enemies`, `GameDataCatalog.asset`, `StageOneEnemies`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Data/GameData/Monsters/eve.asset`, `PrimaryAttribute`, `ActiveSkills`, `PassiveSkills`, `ImplementationState`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `0f`.
- Legacy non-English note retained these code references: `PakuriGameDataSeeder.cs`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

---

## Source File: `boards/COMBAT/COMBAT_BLACKBOARD.md`

## Task: Combat Monster Enemy Implementation Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `Pakuri/reference/3.combat`, `Pakuri/reference/5.enemy`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `buff-debuff.md`, `realtime-damage-meter.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/5.enemy/stage-basic-rules.md`, `enemy-stage-index.md`, `stage-1-enemies.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/monster-basic-rule.md`, `monster-skill-patterns.md`, `skill-choice-pool-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/data/enemies.csv`, `enemy_runtime.csv`, `skills.csv`, `skill_runtime.csv`, `ally_units.csv`, `ally_runtime.csv`, `status_effects.csv`, `levelup_choices.csv`, `skill_branches.csv`, `levelup_rules.csv`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `EveVerticalSliceController.cs`, `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`, `Pakuri/Assets/Scripts/Run/RunSession.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

---

## Source File: `boards/COMBAT/ENEMY_BLACKBOARD.md`

## Task: Stage Basic Enemy Spawn Rule Reset

### Task title

Reset RunScene enemy spawn positions to `stage-basic-rules.md`.

### Goals

- Treat the current RunScene battlefield as bottom-left `(0,0)` and top-right `(31,17)`.
- Treat `EnemySpawnPoint` X as `33`.
- Spawn normal enemies from X `33` with random Y in `0~17`.
- Spawn boss enemies from `(33,8)`.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run Code Reviewer once only after implementation.

### Role Owner

Code Builder

### Status

Builder implementation, local validation, and one Code Reviewer PASS completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Unity Play Mode that normal enemies spawn along Y `0~17` from X `33`, and bosses spawn near `(33,8)`.

### Evidence

- `Pakuri/reference/5.enemy/stage-basic-rules.md` says screen coordinates are `(0,0)` to `(31,17)`, default spawn X is `33`, normal monster Y is random `0~17`, and boss default point is `(33,8)`.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` previously serialized `enemySpawnYRange = new Vector2(6f, 10f)`.
- `SpawnEnemy()` previously used `enemySpawnAnchor.position` and applied the random Y range as an offset from `DefaultEnemySpawnPosition.y`.
- `EveVerticalSliceController.cs` now serializes `enemySpawnYRange = new Vector2(0f, 17f)`.
- `EveVerticalSliceController.cs` now defines `EnemySpawnX = 33f`, `BossSpawnY = 8f`, and `DefaultEnemySpawnPosition = new Vector3(EnemySpawnX, BossSpawnY, 0f)`.
- `ResolveEnemySpawnPosition(bool isBoss)` now forces X to `33`, uses Y `8` for bosses, and uses random Y from `enemySpawnYRange` for normal enemies.
- `Pakuri/Assets/Scenes/RunScene.unity` now stores `EnemySpawnPoint` at `{x: 33, y: 8, z: 0}` and `enemySpawnYRange: {x: 0, y: 17}`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity console error query returned only an MCP-FOR-UNITY client handler exit log, not a project script compile error.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: PASS` in `codex_loop_logs/stage_basic_spawn_reviewer_20260428.md`.

### History

- 2026-04-28: User requested enemy spawn rules reset based on `Pakuri/reference/5.enemy/stage-basic-rules.md`, treating the RunScene field as `(0,0)` to `(31,17)` and `EnemySpawnPoint` X as `33`.
- 2026-04-28: Code Builder updated `EveVerticalSliceController.cs` and `RunScene.unity` to match the document rules.
- 2026-04-28: External Code Reviewer one-shot review returned `REVIEW_RESULT: PASS`; Play Mode gameplay verification remains user-owned.

---

## Source File: `boards/COMBAT/ENEMY_BLACKBOARD.md`

## Task: EnemySpawnPoint Editable Position

### Task title

Allow scene-edited `CombatRoot/EnemySpawnPoint` position to persist when starting the game.

### Goals

- Stop runtime scene reference resolution from resetting an existing `EnemySpawnPoint` to the hardcoded default `(29, 8, 0)`.
- Keep default creation behavior for missing anchors.
- Make enemy spawn placement use the edited `EnemySpawnPoint` transform position, including vertical movement.

### Constraints

- Role Owner is Code Builder after Designer handoff.
- User explicitly requested no Code Reviewer stage for this task; proceed with self-review only.
- All claims must be grounded in actual code and command output.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User moves `CombatRoot/EnemySpawnPoint` in `RunScene`, starts Play Mode, and verifies the marker no longer returns to `(29, 8, 0)`.
- User verifies spawned enemies appear around the edited `EnemySpawnPoint` position.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` previously called `EnsureChild(enemySpawnAnchor, "EnemySpawnPoint", new Vector3(29f, 8f, 0f))`.
- `EnsureChild()` previously assigned `current.position = worldPosition` and `existing.position = worldPosition`, which reset existing anchors.
- Added `DefaultEnemySpawnPosition` and changed `EnsureChild()` so existing `current` or found children are returned without overwriting their position.
- `SpawnEnemy()` now starts from `enemySpawnAnchor.position` and applies the configured Y random range as an offset from the default spawn Y, so edited spawn point Y also affects spawn placement.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- External Code Reviewer command was started but the user interrupted it and instructed to proceed with self-review only.

### History

- 2026-04-28: User reported that editing `Combat Root/EnemySpawnPoint` in the scene is reverted when starting the game.
- 2026-04-28: Confirmed reset cause in `EveVerticalSliceController.ResolveSceneReferences()` and `EnsureChild()`.
- 2026-04-28: Changed existing anchor handling to preserve scene-authored positions and adjusted enemy spawn placement to use the anchor transform as the base position.
- 2026-04-28: Per user instruction, skipped Code Reviewer and kept only Builder self-review plus build verification.

---

## Source File: `boards/COMBAT/ENEMY_BLACKBOARD.md`

## Task: Enemy Target Priority Monster First

### Task title

Enemy combat flow targets the selected Monster before the Nexus.

### Goals

- Enemies should move toward and attack the Monster before attacking the tower/Nexus.
- If the Monster HP reaches 0, enemies should fall back to the existing Nexus target and Nexus defeat flow.
- Keep the change grounded in the existing `EveVerticalSliceController` combat flow.

### Constraints

- Role Owner is Code Builder.
- User will run Play Mode verification.
- Do not claim gameplay verification; only build, Unity refresh, console check, and self-review are performed here.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that Stage 1 enemies approach the selected Monster first.
- User verifies that Monster HP decreases before Nexus HP, and Nexus starts taking damage only after Monster HP reaches 0.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` now calls `GetEnemyPriorityTarget()` in `UpdateEnemies()` before moving or attacking.
- `GetEnemyPriorityTarget()` returns `eveAnchor` while `unitCurrentHealth > 0f`, then falls back to `nexusAnchor`.
- Enemy damage skills now call `ApplyEnemyDamageToPriorityTarget()`, which subtracts from `unitCurrentHealth` first and from `nexusCurrentHealth` only after Monster HP is depleted.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query showed MCP-FOR-UNITY transport/client handler entries only; no project script compile error was returned.

### History

- 2026-04-27: User requested enemies attack Monsters before hitting the tower.
- 2026-04-27: Confirmed the existing `UpdateEnemies()` flow targeted only `nexusAnchor` and the existing damage function subtracted only `nexusCurrentHealth`.
- 2026-04-27: Changed enemy movement and damage target selection to prefer the Monster while alive, with Nexus fallback after Monster HP reaches 0.
- 2026-04-27: Follow-up self-review fixes applied. Enemy attacks now resolve through `EnemyAttackResolver`, Monster defenses are cloned into runtime target defenses, enemy critical passive bonuses are copied from enemy stats into runtime, and fallback Stage 1 enemy ScriptableObjects are cached with `HideFlags.DontSave`.
- 2026-04-27: Ranged and melee/ranged enemies now fire enemy projectiles. HP damage is resolved only when those projectiles collide with the Monster or Nexus. Enemies now create a simple overhead `TextMesh` label showing name and HP.

---

## Source File: `boards/COMBAT/ENEMY_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/ENEMY_BLACKBOARD.md`

## Task: Combat Monster Enemy Implementation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `combat-monster-enemy-implementation-plan.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Debug.Log`.

### Constraints

- Role Owner??Code Builder??
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `[CombatDamage]`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/CombatStatModels.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `FormulaLog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/SkillDefinition.cs`, `Pakuri/Assets/Scripts/Data/EnemyDefinition.cs`.
- Legacy non-English note retained these code references: `GameDataCatalog.cs`, `StageOneEnemies`, `MonsterDefinition.cs`, `PrimaryAttribute`, `BaseStats`, `Defenses`, `ActiveSkills`, `PassiveSkills`.
- Legacy non-English note retained these code references: `RunFlowController.cs`, `RunSceneBootstrap.cs`, `GameDataCatalog`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Debug.Log("[CombatDamage] ...")`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `Pakuri/Assets/Data/GameData/Enemies`, `GameDataCatalog.asset`, `StageOneEnemies`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Data/GameData/Monsters/eve.asset`, `PrimaryAttribute`, `ActiveSkills`, `PassiveSkills`, `ImplementationState`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `0f`.
- Legacy non-English note retained these code references: `PakuriGameDataSeeder.cs`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

---

## Source File: `boards/COMBAT/ENEMY_BLACKBOARD.md`

## Task: Combat Monster Enemy Implementation Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `Pakuri/reference/3.combat`, `Pakuri/reference/5.enemy`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `buff-debuff.md`, `realtime-damage-meter.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/5.enemy/stage-basic-rules.md`, `enemy-stage-index.md`, `stage-1-enemies.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/monster-basic-rule.md`, `monster-skill-patterns.md`, `skill-choice-pool-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/data/enemies.csv`, `enemy_runtime.csv`, `skills.csv`, `skill_runtime.csv`, `ally_units.csv`, `ally_runtime.csv`, `status_effects.csv`, `levelup_choices.csv`, `skill_branches.csv`, `levelup_rules.csv`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `EveVerticalSliceController.cs`, `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`, `Pakuri/Assets/Scripts/Run/RunSession.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `rg`, `Get-ChildItem`, `Get-Content`.
- Legacy non-English note retained these code references: `Pakuri/reference/run-systems-integration-summary-report.html`, `Pakuri/reference/Report/run-systems-integration-summary-report.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

---

## Source File: `boards/COMBAT/PROJECTILE_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/PROJECTILE_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/PROJECTILE_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/PROJECTILE_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/PROJECTILE_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/PROJECTILE_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/PROJECTILE_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`

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

---

## Source File: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`

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

---

## Source File: `boards/DATA/CSV_BLACKBOARD.md`

## Task: CSV Data Role And Loading Review

### Task title

Legacy non-English note retained these code references: `Pakuri/data`.

### Goals

- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
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
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these code references: `ally_units.csv`, `ally_runtime.csv`, `enemies.csv`, `enemy_runtime.csv`.
- Legacy non-English note retained these code references: `skills.csv`, `skill_runtime.csv`, `skill_branches.csv`, `levelup_choices.csv`, `levelup_rules.csv`.
- Legacy non-English note retained these code references: `waves_chapter1.csv`, `waves_chapter2.csv`, `waves_chapter3.csv`, `waves_runtime.csv`, `boss_patterns.csv`.
- Legacy non-English note retained these code references: `items.csv`, `status_effects.csv`, `formations.csv`, `balance_targets.csv`.
- Legacy non-English note retained these ASCII code references: `spawn_points.csv`.
- Legacy non-English note retained these code references: `towers.csv`, `tower_skills.csv`, `TOWER_001`.
- Legacy non-English note retained these code references: `ally_units.csv`, `ALLY_*`, `skills.csv`, `TOWER_001`.
- Legacy non-English note retained these code references: `ally_units.csv`, `levelup_choices.csv`, `skill_branches.csv`, `SKILL_004`, `skills.csv`.
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`, `Assets/Resources`, `Assets/StreamingAssets`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts`, `TextAsset`, `Resources.Load`, `StreamingAssets`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `Pakuri/data`.
- Legacy non-English note retained these code references: `ALLY_*`, `TOWER_*`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`.

---

## Source File: `boards/DATA/CSV_BLACKBOARD.md`

## Task: SaveAndLoad Direction Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `reference/4.run`, `reference/6.meta`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
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

- Legacy non-English note retained these code references: `RunSession`, `MetaSaveData`, `RunSnapshot`, `SaveLoadService`.
- Legacy non-English note retained these code references: `GameDataCatalog`, `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/4.run/dungeon-squad-run-structure.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/shop-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/event-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-index.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/active-skill-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/dark-trace-currency-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`, `Assets/Resources`, `Assets/StreamingAssets`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/save-and-load-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `monster-select-run-ui-expansion-plan.html`, `monster-select-run-ui-builder-handoff.html`, `reference/4.run`, `reference/6.meta`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `MetaSaveData`, `RunSnapshot`, `EphemeralRuntime`, `Pakuri/reference/save-and-load-plan.html`.
- Legacy non-English note retained these code references: `Pakuri/data`, `save-and-load-plan.html`.

---

## Source File: `boards/DATA/DATA_BLACKBOARD.md`

## Task: Monster A-J Skill Data Cleanup

### Task title

Prepare the 5 monster A-J skill data cleanup from reference documents.

### Goals

- Use `Pakuri/reference/Report/2026-04-28-reference-implementation-roadmap.html` step 5 as the implementation direction.
- Compare the 5 monster A-J skill documents under `Pakuri/reference/2.Monster` against current `Assets/Data/GameData/Monsters/*.asset`.
- Represent A as the default active skill, B-E as selectable actives, F as a selectable base passive, and G-J as passives unlocked by their matching active skills.
- Keep this pass focused on data/selection/unlock structure before full runtime effects.

### Constraints

- Role Owner is Designer until explicit Builder handoff.
- Ground all claims in actual files and command output.
- Current `SkillDefinition`/`PassiveDefinition` can store base skill/passive fields but has no structured fields for active enhancements, passive enhancements, or master skill branches.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation completed, and the user reported Play Mode verification completed. The required one-shot external Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; the user chose not to fix that reviewer finding for now. The finding is limited to trailing whitespace in `Pakuri/Assets/Data/GameData/Monsters/eve.asset`.

### Next Actions

- Continue to the next requested design or implementation task.
- If the user later wants the reviewer finding cleaned, remove the trailing whitespace in `eve.asset`, rerun `git diff --check`, rebuild, and update this block.

### Evidence

- Roadmap report step 5 says to organize monster A-J skill data first, completing selection/unlock structure before all complex effects.
- `Pakuri/reference/2.Monster` contains `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `monster-skill-patterns.md`, 5 monster tower documents, and 50 A-J skill documents.
- `SkillDefinition.cs` currently contains `SkillId`, `DisplayName`, `Slot`, `RuntimeKind`, `ImplementationState`, damage/range/cooldown/magazine fields, `StatusEffectId`, and `Summary`.
- `PassiveDefinition` currently contains `PassiveId`, `DisplayName`, `Slot`, `RequiredActiveSlot`, `ImplementationState`, and `Summary`.
- `MonsterDefinition.cs` currently stores `InitialRewardChoices`, `ActiveSkills`, and `PassiveSkills`, but no active-enhancement, passive-enhancement, or master-skill structured data.
- Current monster assets already contain A-E active entries and F-J passive entries; all A entries are `RuntimeImplemented`, B-E and F-J are `DataOnly`.
- `monster-basic-rule.md` states each monster starts with active A learned, starts with no passives learned, F is selectable without a specific active unlock, and G-J unlock after the matching B-E active is learned.
- `skill-choice-pool-rule.md` defines active enhancements, passive enhancements, and master skill candidates, but the current SO model has no dedicated structures for these candidates.
- `SkillDefinition.cs` now adds `SkillChoiceDefinition`, `SkillIcon`, `SkillEffectPrefab`, `DescriptionText`, active `EnhancementChoices`, active `MasterSkillChoices`, passive `EnhancementChoices`, `IsDefaultLearned`, and `IsAvailableWithoutActiveRequirement`.
- `PakuriGameDataSeeder.cs` now reads `Pakuri/reference/2.Monster/{monster}/skill/*.md` and populates A-E active and F-J passive data from those documents.
- `RunCombatUiController.cs` now adds structured active enhancements, passive enhancements, and master skill choices to the prisoner offering pool; it bypasses the active requirement only when `PassiveDefinition.IsAvailableWithoutActiveRequirement` is true.
- After running `Pakuri/Seed Default Game Data`, each monster asset has 5 `SkillId` entries, 5 `PassiveId` entries, 10 `EnhancementChoices` blocks, and 5 `MasterSkillChoices` blocks.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing 2 Unity/MCP reference warnings.
- Unity console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; verified with `git diff --check -- Pakuri\Assets\Data\GameData\Monsters\eve.asset`, which reports trailing whitespace at lines 225, 238, 288, 301, 352, and 365.
- Added `Pakuri/reference/Report/2026-04-29-roadmap-implementation-result.html` comparing today's implementation result against `2026-04-28-reference-implementation-roadmap.html`.
- Added `Pakuri/reference/Report/2026-04-29-token-optimization-savings.html` estimating token savings from document parsing/token reduction based on measured file sizes.

### History

- 2026-04-29: User requested starting roadmap step 5, monster A-J skill data cleanup, and asked for questions if needed.
- 2026-04-29: User selected the data-structure expansion path, requested per-skill icon/effect/description fields, confirmed reference documents are the conflict source of truth, and confirmed F passive should be selectable from prisoner offering instead of default-granted.
- 2026-04-29: Code Builder expanded skill data structures, connected structured choices to prisoner offering, seeded monster A-J data from reference documents, and ran build/Unity validation.
- 2026-04-29: External Code Reviewer one-shot review returned `NEEDS_CHANGES` for trailing whitespace in `eve.asset`; Builder paused for user instruction per AGENTS.md.
- 2026-04-29: User reported Play Mode verification completed and chose not to fix the reviewer-raised whitespace issue for now.
- 2026-04-29: Designer added roadmap comparison and token optimization savings HTML reports under `Pakuri/reference/Report`.

---

## Source File: `boards/DATA/DATA_BLACKBOARD.md`

## Task: CSV Data Role And Loading Review

### Task title

Legacy non-English note retained these code references: `Pakuri/data`.

### Goals

- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
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
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these code references: `ally_units.csv`, `ally_runtime.csv`, `enemies.csv`, `enemy_runtime.csv`.
- Legacy non-English note retained these code references: `skills.csv`, `skill_runtime.csv`, `skill_branches.csv`, `levelup_choices.csv`, `levelup_rules.csv`.
- Legacy non-English note retained these code references: `waves_chapter1.csv`, `waves_chapter2.csv`, `waves_chapter3.csv`, `waves_runtime.csv`, `boss_patterns.csv`.
- Legacy non-English note retained these code references: `items.csv`, `status_effects.csv`, `formations.csv`, `balance_targets.csv`.
- Legacy non-English note retained these ASCII code references: `spawn_points.csv`.
- Legacy non-English note retained these code references: `towers.csv`, `tower_skills.csv`, `TOWER_001`.
- Legacy non-English note retained these code references: `ally_units.csv`, `ALLY_*`, `skills.csv`, `TOWER_001`.
- Legacy non-English note retained these code references: `ally_units.csv`, `levelup_choices.csv`, `skill_branches.csv`, `SKILL_004`, `skills.csv`.
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`, `Assets/Resources`, `Assets/StreamingAssets`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts`, `TextAsset`, `Resources.Load`, `StreamingAssets`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `Pakuri/data`.
- Legacy non-English note retained these code references: `ALLY_*`, `TOWER_*`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`.

---

## Source File: `boards/DATA/DATA_BLACKBOARD.md`

## Task: Run Systems Integration Summary Report

### Task title

Legacy non-English note retained these code references: `monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan`.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.

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
- Legacy non-English note retained these code references: `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`, `MetaSaveData`, `RunSnapshot`, `GameDataCatalog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `Scenes`, `Screenshots`, `Scripts`, `Settings`, `Resources`, `StreamingAssets`, `DataGenerated`.
- Legacy non-English note retained these code references: `.uxml`, `.uss`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/rin/rin-tower.md`, `rin/skill/g~j`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/4.run/combat-reward-system.md`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `RunSession`, `run-systems-integration-summary-report.html`.

---

## Source File: `boards/DATA/DATA_BLACKBOARD.md`

## Task: Run Flow UICanvas Prototype Implementation

### Task title

Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.

### Goals

- Legacy non-English note retained these code references: `RunSession`, `RunFlowController`, `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

### Next Actions

- Legacy non-English note retained these code references: `RunUICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `B/G, C/H, D/I, E/J`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Seed Default Game Data`, `Assets/Data/GameData/GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `RunUICanvas`, `EventSystem`.
- Legacy non-English note retained these code references: `Assets/Data/GameData/GameDataCatalog.asset`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`, `EventSystem`, `InputSystemUIInputModule`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `stageIndex`.
- Legacy non-English note retained these code references: `RunFlowController.ClearButtons(...)`, `QueuedForDestroy`.
- Legacy non-English note retained these code references: `RunFlowController.ResolveReferences()`, `Arial.ttf`, `ArgumentException`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`, `Arial.ttf`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `EventSystem`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Resources.GetBuiltinResource<Font>("Arial.ttf")`, `RunFlowController`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

---

## Source File: `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`

## Task: Ariel Runtime Implementation State

### Task title

Mark Ariel A-E and F-J skill data as runtime implemented.

### Goals

- Keep Ariel `MonsterDefinition` data aligned with the newly implemented runtime code.
- Ensure future data seeding preserves Ariel runtime implementation states.

### Constraints

- Role Owner is Code Builder.
- Ground claims in actual asset and seeder code.
- Do not run Play Mode verification from Codex.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User can run Play Mode verification using DebugScene or RunScene.
- If Unity regenerates C# project files, confirm `CombatRuntimeArielSkills.cs` remains included after refresh.

### Evidence

- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` now has `ImplementationState: 2` for `ariel-a` through `ariel-e` and `ariel-f` through `ariel-j`.
- `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs` now uses `IsRuntimeImplementedActive(...)` and `IsRuntimeImplementedPassive(...)`.
- Seeder helper `IsRuntimeImplementedMonster(...)` returns true for `eve` and `ariel`, so future seeding keeps Eve/Ariel A-E and F-J runtime implemented.
- `Select-String` confirmed all Ariel A-E/F-J `ImplementationState` values are `2`.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-04-30: Code Builder updated Ariel asset state and seeder behavior during Ariel skill runtime implementation.

---

## Source File: `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`

## Task: Monster A-J Skill Data Cleanup

### Task title

Prepare the 5 monster A-J skill data cleanup from reference documents.

### Goals

- Use `Pakuri/reference/Report/2026-04-28-reference-implementation-roadmap.html` step 5 as the implementation direction.
- Compare the 5 monster A-J skill documents under `Pakuri/reference/2.Monster` against current `Assets/Data/GameData/Monsters/*.asset`.
- Represent A as the default active skill, B-E as selectable actives, F as a selectable base passive, and G-J as passives unlocked by their matching active skills.
- Keep this pass focused on data/selection/unlock structure before full runtime effects.

### Constraints

- Role Owner is Designer until explicit Builder handoff.
- Ground all claims in actual files and command output.
- Current `SkillDefinition`/`PassiveDefinition` can store base skill/passive fields but has no structured fields for active enhancements, passive enhancements, or master skill branches.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation completed, and the user reported Play Mode verification completed. The required one-shot external Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; the user chose not to fix that reviewer finding for now. The finding is limited to trailing whitespace in `Pakuri/Assets/Data/GameData/Monsters/eve.asset`.

### Next Actions

- Continue to the next requested design or implementation task.
- If the user later wants the reviewer finding cleaned, remove the trailing whitespace in `eve.asset`, rerun `git diff --check`, rebuild, and update this block.

### Evidence

- Roadmap report step 5 says to organize monster A-J skill data first, completing selection/unlock structure before all complex effects.
- `Pakuri/reference/2.Monster` contains `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `monster-skill-patterns.md`, 5 monster tower documents, and 50 A-J skill documents.
- `SkillDefinition.cs` currently contains `SkillId`, `DisplayName`, `Slot`, `RuntimeKind`, `ImplementationState`, damage/range/cooldown/magazine fields, `StatusEffectId`, and `Summary`.
- `PassiveDefinition` currently contains `PassiveId`, `DisplayName`, `Slot`, `RequiredActiveSlot`, `ImplementationState`, and `Summary`.
- `MonsterDefinition.cs` currently stores `InitialRewardChoices`, `ActiveSkills`, and `PassiveSkills`, but no active-enhancement, passive-enhancement, or master-skill structured data.
- Current monster assets already contain A-E active entries and F-J passive entries; all A entries are `RuntimeImplemented`, B-E and F-J are `DataOnly`.
- `monster-basic-rule.md` states each monster starts with active A learned, starts with no passives learned, F is selectable without a specific active unlock, and G-J unlock after the matching B-E active is learned.
- `skill-choice-pool-rule.md` defines active enhancements, passive enhancements, and master skill candidates, but the current SO model has no dedicated structures for these candidates.
- `SkillDefinition.cs` now adds `SkillChoiceDefinition`, `SkillIcon`, `SkillEffectPrefab`, `DescriptionText`, active `EnhancementChoices`, active `MasterSkillChoices`, passive `EnhancementChoices`, `IsDefaultLearned`, and `IsAvailableWithoutActiveRequirement`.
- `PakuriGameDataSeeder.cs` now reads `Pakuri/reference/2.Monster/{monster}/skill/*.md` and populates A-E active and F-J passive data from those documents.
- `RunCombatUiController.cs` now adds structured active enhancements, passive enhancements, and master skill choices to the prisoner offering pool; it bypasses the active requirement only when `PassiveDefinition.IsAvailableWithoutActiveRequirement` is true.
- After running `Pakuri/Seed Default Game Data`, each monster asset has 5 `SkillId` entries, 5 `PassiveId` entries, 10 `EnhancementChoices` blocks, and 5 `MasterSkillChoices` blocks.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing 2 Unity/MCP reference warnings.
- Unity console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; verified with `git diff --check -- Pakuri\Assets\Data\GameData\Monsters\eve.asset`, which reports trailing whitespace at lines 225, 238, 288, 301, 352, and 365.
- Added `Pakuri/reference/Report/2026-04-29-roadmap-implementation-result.html` comparing today's implementation result against `2026-04-28-reference-implementation-roadmap.html`.
- Added `Pakuri/reference/Report/2026-04-29-token-optimization-savings.html` estimating token savings from document parsing/token reduction based on measured file sizes.

### History

- 2026-04-29: User requested starting roadmap step 5, monster A-J skill data cleanup, and asked for questions if needed.
- 2026-04-29: User selected the data-structure expansion path, requested per-skill icon/effect/description fields, confirmed reference documents are the conflict source of truth, and confirmed F passive should be selectable from prisoner offering instead of default-granted.
- 2026-04-29: Code Builder expanded skill data structures, connected structured choices to prisoner offering, seeded monster A-J data from reference documents, and ran build/Unity validation.
- 2026-04-29: External Code Reviewer one-shot review returned `NEEDS_CHANGES` for trailing whitespace in `eve.asset`; Builder paused for user instruction per AGENTS.md.
- 2026-04-29: User reported Play Mode verification completed and chose not to fix the reviewer-raised whitespace issue for now.
- 2026-04-29: Designer added roadmap comparison and token optimization savings HTML reports under `Pakuri/reference/Report`.

---

## Source File: `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`

## Task: Run Flow UICanvas Prototype Implementation

### Task title

Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.

### Goals

- Legacy non-English note retained these code references: `RunSession`, `RunFlowController`, `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

### Next Actions

- Legacy non-English note retained these code references: `RunUICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `B/G, C/H, D/I, E/J`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Seed Default Game Data`, `Assets/Data/GameData/GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `RunUICanvas`, `EventSystem`.
- Legacy non-English note retained these code references: `Assets/Data/GameData/GameDataCatalog.asset`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`, `EventSystem`, `InputSystemUIInputModule`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `stageIndex`.
- Legacy non-English note retained these code references: `RunFlowController.ClearButtons(...)`, `QueuedForDestroy`.
- Legacy non-English note retained these code references: `RunFlowController.ResolveReferences()`, `Arial.ttf`, `ArgumentException`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`, `Arial.ttf`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `EventSystem`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Resources.GetBuiltinResource<Font>("Arial.ttf")`, `RunFlowController`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

---

## Source File: `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`

## Task: Run Systems Integration Summary Report

### Task title

Legacy non-English note retained these code references: `monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan`.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.

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
- Legacy non-English note retained these code references: `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`, `MetaSaveData`, `RunSnapshot`, `GameDataCatalog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `Scenes`, `Screenshots`, `Scripts`, `Settings`, `Resources`, `StreamingAssets`, `DataGenerated`.
- Legacy non-English note retained these code references: `.uxml`, `.uss`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/rin/rin-tower.md`, `rin/skill/g~j`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/4.run/combat-reward-system.md`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `RunSession`, `run-systems-integration-summary-report.html`.

# Task: 2026-05-04 Rin A-E Runtime Implementation State

## Task title

Mark Rin active skills A-E as runtime implemented in the Rin monster data asset.

## Goals

- Keep Rin data asset implementation-state flags aligned with the newly added combat runtime.
- Do not edit reference planning documents.

## Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

## Role Owner

Code Builder

## Status

Implemented and locally validated.

## Next Actions

- User verifies Rin A-E runtime behavior in Play Mode.

## Evidence

- `Pakuri/Assets/Data/GameData/Monsters/rin.asset:88`, `:155`, `:222`, `:287`, and `:354` now show `ImplementationState: 2` for `rin-a` through `rin-e`.
- `Pakuri/reference/2.Monster/rin/skill/*.md` files were read as source references but were not edited.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP warnings.
- Unity-MCP script refresh reached idle and console error query returned only MCP-FOR-UNITY client handler logs.

## History

- 2026-05-04: Code Builder updated Rin A-E implementation-state flags after adding combat runtime support.

---

## Source File: `boards/MON/MON_BLACKBOARD.md`

## Task: Monster Shield Bar Split Visual

### Task title

Display player Monster shields as one fixed-width HP/shield split bar.

### Goals

- Keep Monster HP and shield numeric values unchanged.
- When shield is present, draw one bar with red HP and white shield sharing the fixed visual width by ratio.
- Apply the shared visual path to the selected Monster status bar.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the latest request did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies shielded Monster HP bar visuals in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now has `UpdateHpShieldBarFill()` using `health + shield` as the visual total while shield is present, so HP 10 and shield 1 are drawn as adjacent red/white segments within the same root bar width.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` now calls `UpdateHpShieldBarFill()` for `selectedMonsterHpBarFill` and `selectedMonsterShieldBarFill`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.

### History

- 2026-04-30: User requested League-style shield visuals for all Monster shield-granting skills.
- 2026-04-30: Code Builder changed the shared Monster HP/shield bar update path.
- 2026-04-30: User requested HP Bar `Background` color to differ from white shield; Code Builder changed shared HP bar background renderers to `Color.black`.

---

## Source File: `boards/MON/MON_BLACKBOARD.md`

## Task: Ariel A-E Active And F-J Enhancement Runtime

### Task title

Implement Ariel skill documents A-E and their F-J enhancement/passive effects.

### Goals

- Read actual Ariel skill markdown under `Pakuri/reference/2.Monster/ariel`.
- Implement Ariel active skills A-E in the combat runtime.
- Implement Ariel passive/enhancement effects F-J where the current selected-Monster runtime has corresponding state.
- Keep the implementation grounded in the existing single selected Monster combat model.

### Constraints

- Role Owner is Code Builder.
- User performs Unity Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it for this implementation.
- Current combat runtime has one selected allied Monster, not a collection of allied units.

### Role Owner

Code Builder

### Status

Implemented and locally validated. Code Reviewer returned FAIL for Ariel behavior mismatches, and Code Builder has applied the requested correction pass.

### Next Actions

- User verifies Ariel A-E and F-J selected effects in DebugScene or RunScene Play Mode.
- If exact multi-ally party behavior is added later, revisit Ariel "all allies" effects and expand them from selected Monster to the full ally collection.

### Evidence

- `boards/MON/ARIEL_MONSTER.md` contains the Ariel-specific skill slot and runtime evidence.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` was added for Ariel-specific runtime behavior.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now routes Ariel A to `FireManualArielJudgementLight(direction)` and combines Ariel damage/defense/critical modifiers with projectile hit resolution.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now tracks and displays Ariel Holy Exposure on enemies.
- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` marks Ariel A-E and F-J as runtime implemented.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity editor state returned `ready_for_tools=true`; console errors were MCP-FOR-UNITY handler logs only.
- 2026-04-30 follow-up fixes addressed Reviewer findings: Ariel A now uses `ariel-a` skill damage/range with projectile speed `17`, last-shot explosion happens from projectile cleanup position, Radiant Shield reflection receives the source attacker, and Holy damage bonuses are not pre-applied before final damage calculation.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.
- Follow-up Unity refresh completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: User requested Ariel A-E skill implementation plus enhancement effects after pointing to `Pakuri/reference/2.Monster/ariel`.
- 2026-04-30: Code Builder implemented Ariel runtime behavior and updated related data/state files.
- 2026-04-30: User instructed Builder to fix Code Reviewer findings; Builder applied the Ariel correction pass and did not rerun Code Reviewer because a new review was not explicitly requested.

---

## Source File: `boards/MON/MON_BLACKBOARD.md`

## Task: Hold Input Primary Skill Fire

### Task title

Allow all 5 Monster A skills to keep firing while left mouse or touch input is held.

### Goals

- Change the current one-click A skill trigger into a held-input trigger.
- Preserve existing shot interval, magazine, reload, and active-skill trigger behavior.
- Support mouse left-button hold and mobile touch hold.
- Keep the change in the shared combat input path so all 5 player Monsters use the same behavior.

### Constraints

- Role Owner is Code Builder.
- Ground claims in actual files and command output.
- Do not run Unity Play Mode gameplay verification; user verifies gameplay.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that holding left mouse or touch continuously fires A skill toward the held pointer position for each Monster.
- User verifies left-button/touch-triggered active skill effects still respect their cooldowns while held.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeHud.cs` now uses held input checks: `Mouse.current.leftButton.isPressed`, `Touchscreen.current.primaryTouch.press.isPressed`, `Input.GetMouseButton(0)`, and `Input.touchCount`.
- `CombatRuntimeHud.cs` still sets `fireRequestedThisFrame = true` after converting the current pointer/touch screen position into the clamped world attack point.
- `CombatRuntimeProjectiles.cs` already gates primary fire through `shotCooldown`, `currentShotsRemaining`, and `reloadRemaining`, so held input repeats through the existing fire interval and reload rules.
- `CombatRuntimeProjectiles.cs` calls `TryTriggerEveAutomaticSkills()` whenever the shared fire request is active, preserving left-button active trigger behavior through existing cooldown logic.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing 2 Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity editor state returned `ready_for_tools=true`; Unity console error query returned MCP-FOR-UNITY client handler logs only, not project compile errors.

### History

- 2026-04-30: User requested that holding left mouse click, or mobile touch, continuously fires the 5 Monsters' A skill toward the held pointer position and keeps the same active-skill trigger behavior.
- 2026-04-30: Code Builder changed the shared combat pointer input to treat held mouse/touch as a fire request and validated compilation.

---

## Source File: `boards/MON/MON_BLACKBOARD.md`

## Task: Monster A-J Skill Data Cleanup

### Task title

Prepare the 5 monster A-J skill data cleanup from reference documents.

### Goals

- Use `Pakuri/reference/Report/2026-04-28-reference-implementation-roadmap.html` step 5 as the implementation direction.
- Compare the 5 monster A-J skill documents under `Pakuri/reference/2.Monster` against current `Assets/Data/GameData/Monsters/*.asset`.
- Represent A as the default active skill, B-E as selectable actives, F as a selectable base passive, and G-J as passives unlocked by their matching active skills.
- Keep this pass focused on data/selection/unlock structure before full runtime effects.

### Constraints

- Role Owner is Designer until explicit Builder handoff.
- Ground all claims in actual files and command output.
- Current `SkillDefinition`/`PassiveDefinition` can store base skill/passive fields but has no structured fields for active enhancements, passive enhancements, or master skill branches.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation completed, and the user reported Play Mode verification completed. The required one-shot external Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; the user chose not to fix that reviewer finding for now. The finding is limited to trailing whitespace in `Pakuri/Assets/Data/GameData/Monsters/eve.asset`.

### Next Actions

- Continue to the next requested design or implementation task.
- If the user later wants the reviewer finding cleaned, remove the trailing whitespace in `eve.asset`, rerun `git diff --check`, rebuild, and update this block.

### Evidence

- Roadmap report step 5 says to organize monster A-J skill data first, completing selection/unlock structure before all complex effects.
- `Pakuri/reference/2.Monster` contains `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `monster-skill-patterns.md`, 5 monster tower documents, and 50 A-J skill documents.
- `SkillDefinition.cs` currently contains `SkillId`, `DisplayName`, `Slot`, `RuntimeKind`, `ImplementationState`, damage/range/cooldown/magazine fields, `StatusEffectId`, and `Summary`.
- `PassiveDefinition` currently contains `PassiveId`, `DisplayName`, `Slot`, `RequiredActiveSlot`, `ImplementationState`, and `Summary`.
- `MonsterDefinition.cs` currently stores `InitialRewardChoices`, `ActiveSkills`, and `PassiveSkills`, but no active-enhancement, passive-enhancement, or master-skill structured data.
- Current monster assets already contain A-E active entries and F-J passive entries; all A entries are `RuntimeImplemented`, B-E and F-J are `DataOnly`.
- `monster-basic-rule.md` states each monster starts with active A learned, starts with no passives learned, F is selectable without a specific active unlock, and G-J unlock after the matching B-E active is learned.
- `skill-choice-pool-rule.md` defines active enhancements, passive enhancements, and master skill candidates, but the current SO model has no dedicated structures for these candidates.
- `SkillDefinition.cs` now adds `SkillChoiceDefinition`, `SkillIcon`, `SkillEffectPrefab`, `DescriptionText`, active `EnhancementChoices`, active `MasterSkillChoices`, passive `EnhancementChoices`, `IsDefaultLearned`, and `IsAvailableWithoutActiveRequirement`.
- `PakuriGameDataSeeder.cs` now reads `Pakuri/reference/2.Monster/{monster}/skill/*.md` and populates A-E active and F-J passive data from those documents.
- `RunCombatUiController.cs` now adds structured active enhancements, passive enhancements, and master skill choices to the prisoner offering pool; it bypasses the active requirement only when `PassiveDefinition.IsAvailableWithoutActiveRequirement` is true.
- After running `Pakuri/Seed Default Game Data`, each monster asset has 5 `SkillId` entries, 5 `PassiveId` entries, 10 `EnhancementChoices` blocks, and 5 `MasterSkillChoices` blocks.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing 2 Unity/MCP reference warnings.
- Unity console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; verified with `git diff --check -- Pakuri\Assets\Data\GameData\Monsters\eve.asset`, which reports trailing whitespace at lines 225, 238, 288, 301, 352, and 365.
- Added `Pakuri/reference/Report/2026-04-29-roadmap-implementation-result.html` comparing today's implementation result against `2026-04-28-reference-implementation-roadmap.html`.
- Added `Pakuri/reference/Report/2026-04-29-token-optimization-savings.html` estimating token savings from document parsing/token reduction based on measured file sizes.

### History

- 2026-04-29: User requested starting roadmap step 5, monster A-J skill data cleanup, and asked for questions if needed.
- 2026-04-29: User selected the data-structure expansion path, requested per-skill icon/effect/description fields, confirmed reference documents are the conflict source of truth, and confirmed F passive should be selectable from prisoner offering instead of default-granted.
- 2026-04-29: Code Builder expanded skill data structures, connected structured choices to prisoner offering, seeded monster A-J data from reference documents, and ran build/Unity validation.
- 2026-04-29: External Code Reviewer one-shot review returned `NEEDS_CHANGES` for trailing whitespace in `eve.asset`; Builder paused for user instruction per AGENTS.md.
- 2026-04-29: User reported Play Mode verification completed and chose not to fix the reviewer-raised whitespace issue for now.
- 2026-04-29: Designer added roadmap comparison and token optimization savings HTML reports under `Pakuri/reference/Report`.

---

## Source File: `boards/MON/MON_BLACKBOARD.md`

## Task: Combat Monster Enemy Implementation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `combat-monster-enemy-implementation-plan.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Debug.Log`.

### Constraints

- Role Owner??Code Builder??
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `[CombatDamage]`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/CombatStatModels.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `FormulaLog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/SkillDefinition.cs`, `Pakuri/Assets/Scripts/Data/EnemyDefinition.cs`.
- Legacy non-English note retained these code references: `GameDataCatalog.cs`, `StageOneEnemies`, `MonsterDefinition.cs`, `PrimaryAttribute`, `BaseStats`, `Defenses`, `ActiveSkills`, `PassiveSkills`.
- Legacy non-English note retained these code references: `RunFlowController.cs`, `RunSceneBootstrap.cs`, `GameDataCatalog`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Debug.Log("[CombatDamage] ...")`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `Pakuri/Assets/Data/GameData/Enemies`, `GameDataCatalog.asset`, `StageOneEnemies`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Data/GameData/Monsters/eve.asset`, `PrimaryAttribute`, `ActiveSkills`, `PassiveSkills`, `ImplementationState`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `0f`.
- Legacy non-English note retained these code references: `PakuriGameDataSeeder.cs`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

---

## Source File: `boards/MON/MON_BLACKBOARD.md`

## Task: Combat Monster Enemy Implementation Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `Pakuri/reference/3.combat`, `Pakuri/reference/5.enemy`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `buff-debuff.md`, `realtime-damage-meter.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/5.enemy/stage-basic-rules.md`, `enemy-stage-index.md`, `stage-1-enemies.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/monster-basic-rule.md`, `monster-skill-patterns.md`, `skill-choice-pool-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/data/enemies.csv`, `enemy_runtime.csv`, `skills.csv`, `skill_runtime.csv`, `ally_units.csv`, `ally_runtime.csv`, `status_effects.csv`, `levelup_choices.csv`, `skill_branches.csv`, `levelup_rules.csv`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `EveVerticalSliceController.cs`, `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`, `Pakuri/Assets/Scripts/Run/RunSession.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `rg`, `Get-ChildItem`, `Get-Content`.
- Legacy non-English note retained these code references: `Pakuri/reference/run-systems-integration-summary-report.html`, `Pakuri/reference/Report/run-systems-integration-summary-report.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

---

## Source File: `boards/MON/MON_BLACKBOARD.md`

## Task: Monster Select Run UI Expansion Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `2.Monster`, `skill-choice-pool-rule.md`, `combat-reward-system.md`.
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
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `g~j`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `Main Camera`, `Global Light 2D`, `CombatRoot`.
- Legacy non-English note retained these code references: `CombatRoot`, `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`, `ProjectileRoot`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `NO_UI_TOOLKIT_ASSETS`, `NO_UI_NAMED_ASSETS`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/monster-basic-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `eve/eve-tower.md`, `rin/rin-tower.md`, `sein/sein-tower.md`, `vega/vega-tower.md`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `f~j`, `f-ambidextrous.md`, `g~j`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `2.Monster`, `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `combat-reward-system.md`, `dungeon-squad-run-structure.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note retained these code references: `g~j`.
- Legacy non-English note retained these ASCII code references: `, `.

---

## Source File: `boards/MON/MON_BLACKBOARD.md`

## Task: Run Flow UICanvas Prototype Implementation

### Task title

Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.

### Goals

- Legacy non-English note retained these code references: `RunSession`, `RunFlowController`, `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

### Next Actions

- Legacy non-English note retained these code references: `RunUICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `B/G, C/H, D/I, E/J`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Seed Default Game Data`, `Assets/Data/GameData/GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `RunUICanvas`, `EventSystem`.
- Legacy non-English note retained these code references: `Assets/Data/GameData/GameDataCatalog.asset`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`, `EventSystem`, `InputSystemUIInputModule`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `stageIndex`.
- Legacy non-English note retained these code references: `RunFlowController.ClearButtons(...)`, `QueuedForDestroy`.
- Legacy non-English note retained these code references: `RunFlowController.ResolveReferences()`, `Arial.ttf`, `ArgumentException`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`, `Arial.ttf`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `EventSystem`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Resources.GetBuiltinResource<Font>("Arial.ttf")`, `RunFlowController`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

---

## Source File: `boards/OPS/CODEX_CLI_BLACKBOARD.md`

## Task: Codex CLI Bootstrap

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `run_codex.bat`.
- Legacy non-English note retained these code references: `codex_prompt.txt`.
- Legacy non-English note retained these code references: `AGENTS.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `bat`, `txt`, `md`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Completed for bootstrap file creation, path correction, and Codex CLI path resolver hardening. No downstream Builder task has been run through the loop yet.

### Next Actions

- Legacy non-English note retained these code references: `run_codex.bat`.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `codex_loop_logs`, `BLACKBOARD.md`.

### Evidence

- Legacy non-English note retained these code references: `Get-Location`, `C:\TowerDefence_Pakuri\Test`.
- Legacy non-English note retained these code references: `Get-ChildItem -Force`, `.git`, `.gitignore`.
- Legacy non-English note retained these code references: `run_codex.bat`, `codex_prompt.txt`, `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `Get-Command codex`, `c:\Users\t3312\.vscode\extensions\openai.chatgpt-26.415.20818-win32-x64\bin\windows-x86_64\codex.exe`.
- Legacy non-English note retained these code references: `codex --version`, `codex-cli 0.122.0-alpha.1`.
- Legacy non-English note retained these code references: `Test-Path (Join-Path $env:APPDATA 'npm\codex.cmd')`, `False`.
- Legacy non-English note retained these code references: `Join-Path $env:APPDATA 'npm\codex.cmd'`, `C:\Users\t3312\AppData\Roaming\npm\codex.cmd`.
- Legacy non-English note retained these code references: `codex --help`, `exec`, `review`, `login`, `logout`, `mcp`, `marketplace`, `mcp-server`, `app-server`, `completion`, `sandbox`, `debug`.
- Legacy non-English note retained these code references: `codex --help`, `codex review --help`, `codex exec --help`, `codex debug --help`, `codex mcp --help`.
- Legacy non-English note retained these code references: `codex review --help`, `--uncommitted`, `--base`, `--commit`.
- Legacy non-English note retained these code references: `codex exec --help`, `--skip-git-repo-check`, `-C`, `--full-auto`, `-o`.
- Legacy non-English note retained these code references: `git rev-parse --is-inside-work-tree`, `true`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note retained these code references: `Test-Path (Join-Path $env:APPDATA 'npm\codex.cmd')`, `True`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`, `codex.exe`.
- Legacy non-English note retained these code references: `& (Join-Path $env:APPDATA 'npm\codex.cmd') --version`, `codex-cli 0.122.0-alpha.1`.
- Legacy non-English note retained these code references: `cmd /d /c "call run_codex.bat < NUL"`, `codex.cmd`, `Required default path: C:\Users\t3312\AppData\Roaming\npm\codex.cmd`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`.
- Legacy non-English note retained these code references: `C:\Users\t3312\AppData\Roaming\npm\codex.cmd`, `openai.chatgpt-26.415.20818-win32-x64\bin\windows-x86_64\codex.exe`.
- Legacy non-English note retained these code references: `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`, `codex-cli 0.122.0-alpha.13`.
- Legacy non-English note retained these code references: `run_codex.bat`, `%APPDATA%\npm\codex.cmd`, `codex.exe`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`, `Resolve-CodexCommand`.
- Legacy non-English note retained these code references: `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`, `codex-cli 0.122.0-alpha.13`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`, `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`, `codex-cli 0.122.0-alpha.13`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`.
- Legacy non-English note retained these code references: `codex_loop_logs\manual_reviewer_20260423_212033.md`, `REVIEW_RESULT: PASS`.
- Legacy non-English note retained these ASCII code references: `codex exec`.
- Legacy non-English note retained these code references: `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.422.30944-win32-x64\bin\windows-x86_64\codex.exe`, `REVIEW_RESULT: PASS`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`, `Invoke-CodexExec`, `$builderExit`.
- Legacy non-English note retained these code references: `Invoke-CodexExec`, `*.console.txt`.
- Legacy non-English note retained these code references: `$ErrorActionPreference = 'Stop'`, `NativeCommandError`, `Invoke-CodexExec`, `Continue`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`, `PARSE_OK`.
- Legacy non-English note retained these code references: `Reviewer PASS at loop 1.`, `codex_loop_logs\20260425_213006\loop_01_reviewer.md`, `REVIEW_RESULT: PASS`.
- Legacy non-English note retained these code references: `codex_loop_logs\reviewer_restore_fix_review.md`, `run_codex.bat`, `BLACKBOARD.md`, `REVIEW_RESULT: NEEDS_CHANGES`.
- Legacy non-English note retained these code references: `run_codex.bat`, `codex_prompt.txt`, `.Replace([string][char]34, [string][char]0x201D)`.
- Legacy non-English note retained these code references: `Add-BlackboardHistory`, `Codex CLI Bootstrap`, `Builder Reviewer Loop`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Reviewer PASS at loop 1.`, `codex_loop_logs\20260425_213901\loop_01_reviewer.md`, `REVIEW_RESULT: PASS`.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `exec`, `review`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `run_codex.bat`, `codex_prompt.txt`, `AGENTS.md`, `BLACKBOARD.md`, `codex_builder_reviewer.ps1`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`, `--version`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note retained these code references: `run_codex.bat`, `codex_builder_reviewer.ps1`, `codex.exe`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note retained these code references: `codex_loop_logs\manual_reviewer_20260423_212033.md`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`.
- Legacy non-English note retained these code references: `codex_loop_logs\20260425_213006\loop_01_reviewer.md`, `REVIEW_RESULT: PASS`.
- Legacy non-English note retained these code references: `run_codex.bat`, `BLACKBOARD.md`, `codex_loop_logs\20260425_213901\loop_01_reviewer.md`, `REVIEW_RESULT: PASS`.

- 2026-04-25 21:39:01 +09:00: Builder -> Reviewer loop started. Run directory: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901
- 2026-04-25 21:39:27 +09:00: Loop 1 Builder started. Output: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901\loop_01_builder.md
- 2026-04-25 21:41:53 +09:00: Loop 1 Builder finished with exit code 0.
- 2026-04-25 21:42:22 +09:00: Loop 1 Reviewer started. Output: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901\loop_01_reviewer.md
- 2026-04-25 21:44:07 +09:00: Loop 1 Reviewer finished with exit code 0.
- 2026-04-25 21:44:07 +09:00: Loop 1 Reviewer decision: PASS. Builder -> Reviewer loop completed.
### Builder Reviewer Loop

- Enforcement method: External wrapper script
- Wrapper file: `codex_builder_reviewer.ps1`
- Git dependency: Not required
- Max loops: 3
- Current loop count: 1 in latest smoke test
- Last reviewer decision: PASS for wrapper log `codex_loop_logs\20260425_213901\loop_01_reviewer.md`
- Last log directory: `codex_loop_logs\20260425_213901`

---

## Source File: `boards/OPS/CODEX_CLI_BLACKBOARD.md`

## Task: Token Efficient Reviewer Wrapper

### Task title

Reduce unnecessary token use in the external Builder -> Reviewer wrapper while preserving evidence-based review.

### Goals

- Stop wrapper prompts from encouraging full `BLACKBOARD.md` dumps.
- Keep `AGENTS.md` full-read behavior and preserve related `BLACKBOARD.md` block checks.
- Provide Reviewer with direct changed-file evidence so it can review changed lines without broad repeated exploration.
- Create an HTML report explaining the before/after problem and solution.

### Constraints

- Role Owner is Code Builder.
- All claims must be grounded in actual files and command output.
- Because this modifies the external reviewer wrapper logic, Code Reviewer review is required after Builder implementation.

### Role Owner

Code Builder

### Status

Builder implementation, local validation, Reviewer feedback fixes, and external Code Reviewer PASS completed.

### Next Actions

- On the next actual wrapper run, compare new `*.console.txt` `tokens used` values against the prior 59k-83k token smoke-test logs.

### Evidence

- `codex_builder_reviewer.ps1` now adds `Get-BlackboardIndexText`, `Limit-Text`, `Get-ChangedPathList`, `Get-GitDiffText`, and `Get-AddedFileEvidenceText`.
- The wrapper now writes `blackboard_index.txt`, `loop_XX_git_diff.patch`, and `loop_XX_changed_file_evidence.txt` for each loop.
- `loop_XX_git_diff.patch` is git diff evidence for tracked changes; `loop_XX_changed_file_evidence.txt` is the fallback content evidence for existing changed files including untracked additions.
- Builder and Reviewer prompts now instruct agents to read `AGENTS.md` in full but use `BLACKBOARD.md` through the generated index and related task blocks instead of printing the full file.
- Reviewer prompts now include git diff evidence and changed file content evidence excerpts.
- Added `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- PowerShell parser validation for `codex_builder_reviewer.ps1` returned `PARSE_OK`.
- `git status --short` after Builder implementation showed `M codex_builder_reviewer.ps1`, `M BLACKBOARD.md`, and untracked `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- External Code Reviewer final rerun returned `REVIEW_RESULT: PASS` in `codex_loop_logs/token_wrapper_reviewer_20260428_rerun2.md`.
- `AGENTS.md` now says Reviewer runs once only, then reports issues to the user instead of continuing an automatic fix loop.
- `AGENTS.md` now says Codex does not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification, while Codex records build/compile/console/editor-state evidence only.

### History

- 2026-04-28: User asked to change the workflow so token use is reduced without weakening evidence-based hallucination prevention, and to create an HTML before/after report.
- 2026-04-28: Code Builder changed the wrapper to create targeted BLACKBOARD and changed-file evidence, then created the HTML report.
- 2026-04-28: External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES` because the HTML report overstated `loop_XX_git_diff.patch` as full changed diff evidence for untracked added files.
- 2026-04-28: Code Builder corrected the HTML report and BLACKBOARD wording to distinguish tracked git diff evidence from changed file content evidence.
- 2026-04-28: External Code Reviewer rerun still found one remaining HTML sentence that overstated full diff patch evidence; Code Builder corrected that sentence.
- 2026-04-28: External Code Reviewer final rerun returned `REVIEW_RESULT: PASS`.
- 2026-04-28: User requested a simple `AGENTS.md` policy update for one Reviewer run only and user-owned Unity-MCP Play Mode verification; Code Builder added the wording to `AGENTS.md` and the HTML report.

---

## Source File: `boards/OPS/REVIEWER_BLACKBOARD.md`

## Task: Token Efficient Reviewer Wrapper

### Task title

Reduce unnecessary token use in the external Builder -> Reviewer wrapper while preserving evidence-based review.

### Goals

- Stop wrapper prompts from encouraging full `BLACKBOARD.md` dumps.
- Keep `AGENTS.md` full-read behavior and preserve related `BLACKBOARD.md` block checks.
- Provide Reviewer with direct changed-file evidence so it can review changed lines without broad repeated exploration.
- Create an HTML report explaining the before/after problem and solution.

### Constraints

- Role Owner is Code Builder.
- All claims must be grounded in actual files and command output.
- Because this modifies the external reviewer wrapper logic, Code Reviewer review is required after Builder implementation.

### Role Owner

Code Builder

### Status

Builder implementation, local validation, Reviewer feedback fixes, and external Code Reviewer PASS completed.

### Next Actions

- On the next actual wrapper run, compare new `*.console.txt` `tokens used` values against the prior 59k-83k token smoke-test logs.

### Evidence

- `codex_builder_reviewer.ps1` now adds `Get-BlackboardIndexText`, `Limit-Text`, `Get-ChangedPathList`, `Get-GitDiffText`, and `Get-AddedFileEvidenceText`.
- The wrapper now writes `blackboard_index.txt`, `loop_XX_git_diff.patch`, and `loop_XX_changed_file_evidence.txt` for each loop.
- `loop_XX_git_diff.patch` is git diff evidence for tracked changes; `loop_XX_changed_file_evidence.txt` is the fallback content evidence for existing changed files including untracked additions.
- Builder and Reviewer prompts now instruct agents to read `AGENTS.md` in full but use `BLACKBOARD.md` through the generated index and related task blocks instead of printing the full file.
- Reviewer prompts now include git diff evidence and changed file content evidence excerpts.
- Added `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- PowerShell parser validation for `codex_builder_reviewer.ps1` returned `PARSE_OK`.
- `git status --short` after Builder implementation showed `M codex_builder_reviewer.ps1`, `M BLACKBOARD.md`, and untracked `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- External Code Reviewer final rerun returned `REVIEW_RESULT: PASS` in `codex_loop_logs/token_wrapper_reviewer_20260428_rerun2.md`.
- `AGENTS.md` now says Reviewer runs once only, then reports issues to the user instead of continuing an automatic fix loop.
- `AGENTS.md` now says Codex does not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification, while Codex records build/compile/console/editor-state evidence only.

### History

- 2026-04-28: User asked to change the workflow so token use is reduced without weakening evidence-based hallucination prevention, and to create an HTML before/after report.
- 2026-04-28: Code Builder changed the wrapper to create targeted BLACKBOARD and changed-file evidence, then created the HTML report.
- 2026-04-28: External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES` because the HTML report overstated `loop_XX_git_diff.patch` as full changed diff evidence for untracked added files.
- 2026-04-28: Code Builder corrected the HTML report and BLACKBOARD wording to distinguish tracked git diff evidence from changed file content evidence.
- 2026-04-28: External Code Reviewer rerun still found one remaining HTML sentence that overstated full diff patch evidence; Code Builder corrected that sentence.
- 2026-04-28: External Code Reviewer final rerun returned `REVIEW_RESULT: PASS`.
- 2026-04-28: User requested a simple `AGENTS.md` policy update for one Reviewer run only and user-owned Unity-MCP Play Mode verification; Code Builder added the wording to `AGENTS.md` and the HTML report.

---

## Source File: `boards/OPS/REVIEWER_BLACKBOARD.md`

## Task: Codex CLI Bootstrap

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `run_codex.bat`.
- Legacy non-English note retained these code references: `codex_prompt.txt`.
- Legacy non-English note retained these code references: `AGENTS.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `bat`, `txt`, `md`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Completed for bootstrap file creation, path correction, and Codex CLI path resolver hardening. No downstream Builder task has been run through the loop yet.

### Next Actions

- Legacy non-English note retained these code references: `run_codex.bat`.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `codex_loop_logs`, `BLACKBOARD.md`.

### Evidence

- Legacy non-English note retained these code references: `Get-Location`, `C:\TowerDefence_Pakuri\Test`.
- Legacy non-English note retained these code references: `Get-ChildItem -Force`, `.git`, `.gitignore`.
- Legacy non-English note retained these code references: `run_codex.bat`, `codex_prompt.txt`, `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `Get-Command codex`, `c:\Users\t3312\.vscode\extensions\openai.chatgpt-26.415.20818-win32-x64\bin\windows-x86_64\codex.exe`.
- Legacy non-English note retained these code references: `codex --version`, `codex-cli 0.122.0-alpha.1`.
- Legacy non-English note retained these code references: `Test-Path (Join-Path $env:APPDATA 'npm\codex.cmd')`, `False`.
- Legacy non-English note retained these code references: `Join-Path $env:APPDATA 'npm\codex.cmd'`, `C:\Users\t3312\AppData\Roaming\npm\codex.cmd`.
- Legacy non-English note retained these code references: `codex --help`, `exec`, `review`, `login`, `logout`, `mcp`, `marketplace`, `mcp-server`, `app-server`, `completion`, `sandbox`, `debug`.
- Legacy non-English note retained these code references: `codex --help`, `codex review --help`, `codex exec --help`, `codex debug --help`, `codex mcp --help`.
- Legacy non-English note retained these code references: `codex review --help`, `--uncommitted`, `--base`, `--commit`.
- Legacy non-English note retained these code references: `codex exec --help`, `--skip-git-repo-check`, `-C`, `--full-auto`, `-o`.
- Legacy non-English note retained these code references: `git rev-parse --is-inside-work-tree`, `true`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note retained these code references: `Test-Path (Join-Path $env:APPDATA 'npm\codex.cmd')`, `True`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`, `codex.exe`.
- Legacy non-English note retained these code references: `& (Join-Path $env:APPDATA 'npm\codex.cmd') --version`, `codex-cli 0.122.0-alpha.1`.
- Legacy non-English note retained these code references: `cmd /d /c "call run_codex.bat < NUL"`, `codex.cmd`, `Required default path: C:\Users\t3312\AppData\Roaming\npm\codex.cmd`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`.
- Legacy non-English note retained these code references: `C:\Users\t3312\AppData\Roaming\npm\codex.cmd`, `openai.chatgpt-26.415.20818-win32-x64\bin\windows-x86_64\codex.exe`.
- Legacy non-English note retained these code references: `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`, `codex-cli 0.122.0-alpha.13`.
- Legacy non-English note retained these code references: `run_codex.bat`, `%APPDATA%\npm\codex.cmd`, `codex.exe`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`, `Resolve-CodexCommand`.
- Legacy non-English note retained these code references: `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`, `codex-cli 0.122.0-alpha.13`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`, `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`, `codex-cli 0.122.0-alpha.13`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`.
- Legacy non-English note retained these code references: `codex_loop_logs\manual_reviewer_20260423_212033.md`, `REVIEW_RESULT: PASS`.
- Legacy non-English note retained these ASCII code references: `codex exec`.
- Legacy non-English note retained these code references: `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.422.30944-win32-x64\bin\windows-x86_64\codex.exe`, `REVIEW_RESULT: PASS`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`, `Invoke-CodexExec`, `$builderExit`.
- Legacy non-English note retained these code references: `Invoke-CodexExec`, `*.console.txt`.
- Legacy non-English note retained these code references: `$ErrorActionPreference = 'Stop'`, `NativeCommandError`, `Invoke-CodexExec`, `Continue`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`, `PARSE_OK`.
- Legacy non-English note retained these code references: `Reviewer PASS at loop 1.`, `codex_loop_logs\20260425_213006\loop_01_reviewer.md`, `REVIEW_RESULT: PASS`.
- Legacy non-English note retained these code references: `codex_loop_logs\reviewer_restore_fix_review.md`, `run_codex.bat`, `BLACKBOARD.md`, `REVIEW_RESULT: NEEDS_CHANGES`.
- Legacy non-English note retained these code references: `run_codex.bat`, `codex_prompt.txt`, `.Replace([string][char]34, [string][char]0x201D)`.
- Legacy non-English note retained these code references: `Add-BlackboardHistory`, `Codex CLI Bootstrap`, `Builder Reviewer Loop`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Reviewer PASS at loop 1.`, `codex_loop_logs\20260425_213901\loop_01_reviewer.md`, `REVIEW_RESULT: PASS`.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `exec`, `review`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `run_codex.bat`, `codex_prompt.txt`, `AGENTS.md`, `BLACKBOARD.md`, `codex_builder_reviewer.ps1`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`, `--version`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note retained these code references: `run_codex.bat`, `codex_builder_reviewer.ps1`, `codex.exe`.
- Legacy non-English note retained these code references: `%APPDATA%\npm\codex.cmd`.
- Legacy non-English note retained these code references: `codex_loop_logs\manual_reviewer_20260423_212033.md`.
- Legacy non-English note retained these code references: `codex_builder_reviewer.ps1`.
- Legacy non-English note retained these code references: `codex_loop_logs\20260425_213006\loop_01_reviewer.md`, `REVIEW_RESULT: PASS`.
- Legacy non-English note retained these code references: `run_codex.bat`, `BLACKBOARD.md`, `codex_loop_logs\20260425_213901\loop_01_reviewer.md`, `REVIEW_RESULT: PASS`.

- 2026-04-25 21:39:01 +09:00: Builder -> Reviewer loop started. Run directory: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901
- 2026-04-25 21:39:27 +09:00: Loop 1 Builder started. Output: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901\loop_01_builder.md
- 2026-04-25 21:41:53 +09:00: Loop 1 Builder finished with exit code 0.
- 2026-04-25 21:42:22 +09:00: Loop 1 Reviewer started. Output: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901\loop_01_reviewer.md
- 2026-04-25 21:44:07 +09:00: Loop 1 Reviewer finished with exit code 0.
- 2026-04-25 21:44:07 +09:00: Loop 1 Reviewer decision: PASS. Builder -> Reviewer loop completed.
### Builder Reviewer Loop

- Enforcement method: External wrapper script
- Wrapper file: `codex_builder_reviewer.ps1`
- Git dependency: Not required
- Max loops: 3
- Current loop count: 1 in latest smoke test
- Last reviewer decision: PASS for wrapper log `codex_loop_logs\20260425_213901\loop_01_reviewer.md`
- Last log directory: `codex_loop_logs\20260425_213901`

---

## Source File: `boards/OPS/UNITY_MCP_BLACKBOARD.md`

## Task: Unity MCP Bridge Connection

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `Pakuri`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Completed. Unity Editor-side MCP For Unity bridge is connected to the current Codex MCP server.

### Next Actions

- Legacy non-English note retained these code references: `Stdio`, `Session Active`, `manage_scene get_active`.
- Legacy non-English note retained these code references: `run_tests EditMode`, `get_test_job`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/ProjectSettings/ProjectVersion.txt`, `m_EditorVersion: 6000.3.4f1`.
- Legacy non-English note retained these code references: `Pakuri/ProjectSettings/ProjectVersion.txt`, `m_EditorVersion: 6000.3.14f1`.
- Legacy non-English note retained these code references: `Pakuri/ProjectSettings/ProjectVersion.txt`, `m_EditorVersionWithRevision: 6000.3.14f1 (d68c3f99a318)`.
- Legacy non-English note retained these code references: `Pakuri/Packages/manifest.json`, `com.coplaydev.unity-mcp`.
- Legacy non-English note retained these code references: `codex mcp get unityMCP`, `enabled: true`, `transport: stdio`, `command: uvx`, `args: --from mcpforunityserver mcp-for-unity --transport stdio`.
- Legacy non-English note retained these code references: `debug_request_context`, `9.6.6`, `active_instance: null`, `all_keys_in_store: []`.
- Legacy non-English note retained these code references: `manage_scene get_active`, `No Unity Editor instances found. Please ensure Unity is running with MCP for Unity bridge.`.
- Legacy non-English note retained these code references: `%USERPROFILE%\.unity-mcp`.
- Legacy non-English note retained these code references: `Test-NetConnection 127.0.0.1:6400`.
- Legacy non-English note retained these code references: `StdioBridgeHost.cs`, `[InitializeOnLoad]`, `StartAutoConnect()`, `WriteHeartbeat()`, `%USERPROFILE%\.unity-mcp\unity-mcp-status-<hash>.json`.
- Legacy non-English note retained these code references: `McpCiBoot.cs`, `EditorPrefs.SetBool(EditorPrefKeys.UseHttpTransport, false)`, `StdioBridgeHost.StartAutoConnect()`.
- Legacy non-English note retained these code references: `README.md`, `Window > MCP for Unity`, `Auto-Setup`, `Start Bridge`.
- Legacy non-English note retained these code references: `%USERPROFILE%\.unity-mcp\unity-mcp-status-c88ab184.json`, `unity_port: 6400`, `reason: ready`, `project_name: Pakuri`, `unity_version: 6000.3.4f1`.
- Legacy non-English note retained these code references: `debug_request_context`, `active_instance: Pakuri@c88ab184`.
- Legacy non-English note retained these code references: `manage_scene get_active`, `SampleScene`, `Assets/Scenes/SampleScene.unity`, `rootCount: 2`.
- Legacy non-English note retained these code references: `read_console`, `Transport changed to: Stdio`, `StdioBridgeHost started on port 6400. (OS=WindowsEditor, server=9.6.6)`, `SkillSync complete: Added: 3, Updated: 0, Deleted: 0 (C:\Users\t3312\.codex\skills\unity-mcp-skill)`.
- Legacy non-English note retained these code references: `manage_asset search`, `Assets`.
- Legacy non-English note retained these code references: `manage_scene get_hierarchy`, `Main Camera`, `Global Light 2D`.
- Legacy non-English note retained these code references: `run_tests EditMode`, `bee66234eeec4e67b238bafff3d63dc9`, `get_test_job`, `status: succeeded`, `resultState: Passed`, `total: 0`, `passed: 0`, `failed: 0`, `skipped: 0`.
- Legacy non-English note retained these code references: `debug_request_context`, `active_instance: Pakuri@0c8eeeb5`.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Stdio`, `Session Active`, `Configuration`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/ProjectSettings/ProjectVersion.txt`, `6000.3.14f1`, `debug_request_context`, `Pakuri@0c8eeeb5`.

---

## Source File: `boards/OPS/UNITY_MCP_BLACKBOARD.md`

## Task: Combat Automation Responsibility Guide

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `reference/current-architecture-plan.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
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
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/current-architecture-plan.html`.
- Legacy non-English note retained these code references: `manage_asset search`, `Assets`, `Scenes`, `Settings`, `Assets/Scripts`.
- Legacy non-English note retained these code references: `Get-ChildItem Pakuri\\Assets`, `Scenes`, `Settings`.
- Legacy non-English note retained these code references: `manage_scene get_hierarchy`, `SampleScene`, `Main Camera`, `Global Light 2D`.
- Legacy non-English note retained these code references: `debug_request_context`, `Pakuri@c88ab184`.
- Legacy non-English note retained these code references: `manage_scene get_active`, `manage_scene get_hierarchy`, `run_tests EditMode`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `reference/current-architecture-plan.html`.
- Legacy non-English note retained these code references: `manage_asset search`, `Get-ChildItem Pakuri\\Assets`, `manage_scene get_hierarchy`.
- Legacy non-English note retained these code references: `Pakuri/reference`.

---

## Source File: `boards/REPORT/REPORT_BLACKBOARD.md`

## Task: Hierarchical Board Migration And Routing Rule Update

### Task title

Document the board hierarchy migration and routing rule update.

### Goals

- Keep report/documentation task history aligned with the root board migration.
- Note that `MDTREE.md` is now the routing entry point for detailed board reads.
- Preserve the old full `BLACKBOARD.md` in `boards/ARCHIVE`.

### Constraints

- Role Owner is Code Builder for the file migration.
- Markdown-only task; no Unity build is required unless code files change.

### Role Owner

Code Builder

### Status

Implemented pending validation.

### Next Actions

- Use `MDTREE.md` for future documentation/report routing.

### Evidence

- Added `MDTREE.md`.
- Replaced root `BLACKBOARD.md` with a compact index.
- Added domain board files under `boards/`.
- Preserved the old root board in `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`.

### History

- 2026-04-30: User requested hierarchical board files and simultaneous related board updates.

## Migrated Task Blocks

---

## Source File: `boards/REPORT/REPORT_BLACKBOARD.md`

## Task: Token Optimized Board Routing Report

### Task title

Create an HTML report explaining the token optimization board-routing change.

### Goals

- Document how `AGENTS.md` was changed from always reading `BLACKBOARD.md` to reading `AGENTS.md` + `MDTREE.md` first.
- Explain how the old root `BLACKBOARD.md` state was split into `boards/` domain files.
- Explain the new work method for routing, reading, and updating board files.
- Save the explanation as an HTML report under `Pakuri/reference/Report`.

### Constraints

- Role Owner is Code Builder because the user explicitly asked to create and save a file.
- Ground every claim in actual files and command output.
- Do not run Unity Play Mode gameplay verification.
- Do not run Code Reviewer unless the user explicitly asks.

### Role Owner

Code Builder

### Status

Completed pending user review.

### Next Actions

- Use `Pakuri/reference/Report/2026-04-30-token-optimized-board-routing.html` when explaining the current board-routing workflow.

### Evidence

- `AGENTS.md` says to read `AGENTS.md` and `MDTREE.md` before normal work.
- `MDTREE.md` defines routing for MON, COMBAT, RUN, UI, DATA, OPS, and REPORT work.
- `BLACKBOARD.md` now describes itself as the root persistent-state index.
- `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md` exists as the pre-hierarchy archive.
- `Get-ChildItem -Recurse -File boards -Filter *.md` confirmed the domain board files exist.
- Added `Pakuri/reference/Report/2026-04-30-token-optimized-board-routing.html`.

### History

- 2026-04-30: User requested an HTML report explaining how `AGENTS.md`, `BLACKBOARD.md`, and work methods changed for token optimization.
- 2026-04-30: Code Builder created the HTML report and recorded this task in the report board.

---

## Source File: `boards/REPORT/REPORT_BLACKBOARD.md`

## Task: DebugScene UI Canvas Retrospective Report

### Task title

DebugScene UI Canvas initial approach, user corrections, and fix history HTML report.

### Goals

- Analyze the recent DebugScene UI Canvas work log.
- Summarize the initial runtime-generated UI approach, user correction requests, reviewer findings, and final scene-bound UI solution.
- Write the result as an HTML report under `Pakuri/reference/Report`.

### Constraints

- Role Owner is Designer.
- Ground all claims in actual files, code, and command output.
- Do not implement runtime gameplay changes for this report.
- Preserve the repository rule that Play Mode gameplay verification is user-owned.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Use `Pakuri/reference/Report/2026-04-30-debugscene-ui-canvas-retrospective.html` as the current written summary if the DebugScene UI flow needs to be discussed again.

### Evidence

- `Get-Content -LiteralPath AGENTS.md` and `Get-Content -LiteralPath BLACKBOARD.md` were run before the response.
- `rg` was not available in this PowerShell environment, so `Select-String` was used.
- `Select-String` confirmed `DebugSceneController.cs` contains `EnsureCanvasShell`, `BindSceneUi`, `ConfigureToggleVisuals`, and `Resources.Load<Sprite>("DebugUiSolid")`.
- `Select-String` confirmed `DebugScene.unity` contains `DebugSceneController`, `DebugSetupPanel`, `SkillDebugPanel`, `EnhancementModal`, `Active_A`, `Passive_J`, `Choice_01`, and `Choice_08`.
- `Get-ChildItem -LiteralPath Pakuri\Assets\Resources` confirmed `DebugUiSolid.png` and `DebugUiSolid.png.meta` exist.
- Added `Pakuri/reference/Report/2026-04-30-debugscene-ui-canvas-retrospective.html`.

### History

- 2026-04-30: User requested an HTML summary of the initial DebugScene UI canvas creation method, user correction points, and how the problems were solved.
- 2026-04-30: Designer reviewed BLACKBOARD task history and current DebugScene code/scene evidence, then added the retrospective HTML report.

---

## Source File: `boards/REPORT/REPORT_BLACKBOARD.md`

## Task: Next Roadmap Work Plan Report

### Task title

Create an HTML summary of the next implementation tasks from the 2026-04-28 roadmap and 2026-04-29 result report.

### Goals

- Read `Pakuri/reference/Report/2026-04-28-reference-implementation-roadmap.html`.
- Read `Pakuri/reference/Report/2026-04-29-roadmap-implementation-result.html`.
- Summarize the next work items into a new HTML report grounded in those files and current `BLACKBOARD.md`.
- Keep this as a Designer report, not a Code Builder implementation.

### Constraints

- Role Owner is Designer.
- Ground all claims in actual files and command output.
- Do not implement gameplay/code changes in this task.
- Preserve the existing user-deferred reviewer finding in `Pakuri/Assets/Data/GameData/Monsters/eve.asset`.

### Role Owner

Designer

### Status

Completed. Added `Pakuri/reference/Report/2026-04-29-next-work-plan.html`.

### Next Actions

- If the user wants implementation next, create a focused Code Builder handoff. The most recommended first slice is Eve B/G or another small state-effect runtime slice that connects selected skill/passive data to real combat effects.

### Evidence

- `2026-04-28-reference-implementation-roadmap.html` says the roadmap after steps 1~5 continues with status effects, stage 2~4 enemies, elite/event, shop/artifact, formation, meta save, and auxiliary UI.
- `2026-04-29-roadmap-implementation-result.html` records roadmap steps 1~5 as complete and identifies step 6, status-effect expansion, as the next large stage.
- Current `BLACKBOARD.md` records Eve active skill runtime as completed with external Reviewer `PASS`.
- Current `BLACKBOARD.md` records Monster A-J Skill Data Cleanup as implemented, with the `eve.asset` trailing whitespace reviewer finding intentionally deferred by the user.
- `Pakuri/reference/Report/2026-04-29-next-work-plan.html` now lists the immediate queue, later queue, Builder handoff candidates, excluded work, and evidence.

### History

- 2026-04-29: Designer read `AGENTS.md`, `BLACKBOARD.md`, `2026-04-28-reference-implementation-roadmap.html`, and `2026-04-29-roadmap-implementation-result.html`.
- 2026-04-29: Designer created the next-work HTML report and recorded this completed task block.

---

## Source File: `boards/REPORT/REPORT_BLACKBOARD.md`

## Task: Reference Implementation Roadmap Report

### Task title

Create an HTML report summarizing current implementation status and next implementation order from `reference` Markdown documents.

### Goals

- Read current `AGENTS.md` and relevant `BLACKBOARD.md` state before work.
- Inspect `Pakuri/reference` Markdown files while treating `dungeon-squad*.md` files as reference-only, not implementation targets.
- Compare reference documents against actual `Assets` scripts, scenes, and data assets.
- Create an HTML report under `Pakuri/reference/Report`.

### Constraints

- Role Owner is Designer.
- Ground all claims in actual files and command output.
- Do not claim implementation for systems that have no actual script, scene, or asset evidence.
- This is a design/status report, not gameplay logic implementation.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- If implementation continues, recommended first Builder handoff is combat reward actualization: prisoner count/probability, boss prisoner guarantee display, gold/dark trace accumulation, and `RunSession` persistence within the current run.

### Evidence

- `Get-ChildItem Pakuri\reference -Recurse -Filter *.md` found 105 Markdown files.
- File count command classified 9 `dungeon-squad*.md` files as reference-only and 96 non-`dungeon-squad*.md` files as implementation reference documents.
- `Get-ChildItem Pakuri\Assets\Scripts -Recurse -File` confirmed current script folders: `Combat`, `Data`, and `Run`.
- `Get-ChildItem Pakuri\Assets\Scenes -File` confirmed `MainMenuScene.unity` and `RunScene.unity`.
- `Get-ChildItem Pakuri\Assets\Data -Recurse -File` confirmed `GameDataCatalog.asset`, 5 monster assets, and 8 stage1 enemy assets.
- `Select-String` checks found no dedicated runtime script or asset evidence for full `Formation`, `Artifact`, `Shop`, `Meta`, `Guidebook`, `Training`, or `Market` systems beyond existing `.meta` files and unrelated Unity/EventSystem references.
- Created `Pakuri/reference/Report/2026-04-28-reference-implementation-roadmap.html`.

### History

- 2026-04-28: User requested an HTML summary of current implementation status and future implementation order based on `reference` Markdown files, while treating `dungeon-squad*.md` as reference-only.
- 2026-04-28: Designer inspected current references, scripts, scenes, and data assets, then created the implementation roadmap HTML report.

---

## Source File: `boards/REPORT/REPORT_BLACKBOARD.md`

## Task: Run Systems Integration Summary Report

### Task title

Legacy non-English note retained these code references: `monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan`.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.

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
- Legacy non-English note retained these code references: `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`, `MetaSaveData`, `RunSnapshot`, `GameDataCatalog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `Scenes`, `Screenshots`, `Scripts`, `Settings`, `Resources`, `StreamingAssets`, `DataGenerated`.
- Legacy non-English note retained these code references: `.uxml`, `.uss`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/rin/rin-tower.md`, `rin/skill/g~j`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/4.run/combat-reward-system.md`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `RunSession`, `run-systems-integration-summary-report.html`.

# Task: 2026-05-07 Character Skill Effect Pipeline Review

### Task title

Character creation, skill, and effect script structure review report

### Goals

- Inspect the current character creation, skill execution, and effect-related scripts from actual repository files.
- Check whether related managers/factories exist and whether logic is concentrated in a few controllers.
- Explain the current structure and document improvement directions in HTML.

### Constraints

- Follow `AGENTS.md`: all claims must be based on inspected code or command output.
- Role Owner is Designer, so no gameplay code implementation was performed.
- Do not run Unity Play Mode; use file inspection and Unity-MCP editor checks only.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User can open `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.
- If implementation is requested later, start with ID-based `RunSession` learned skill state and `CombatEffectFactory` extraction.

### Evidence

- Created `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.
- Read `AGENTS.md`, `MDTREE.md`, and routed through report/monster/combat/data/run board context.
- Inspected `Pakuri/Assets/Scripts/Data/PakuriDataManager.cs`, `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.cs`, `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.Build.cs`, `Pakuri/Assets/Scripts/Run/RunSession.cs`, `Pakuri/Assets/Scripts/Run/RunStartContext.cs`, `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs`, `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs`, and `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs`.
- `Get-ChildItem Pakuri\Assets\Scripts -Recurse -Filter *.cs` line-count evidence showed large files including `RunCombatUiController.cs` 1372 lines, `DebugSceneController.cs` 1181 lines, `CombatRuntimeSeinSkills.cs` 1125 lines, `CombatRuntimeEveSkills.cs` 1057 lines, `CombatRuntimeEnemies.cs` 1025 lines, `CombatRuntimeVegaSkills.cs` 883 lines, and `CombatRuntimeController.cs` 841 lines.
- `Select-String` evidence found `PakuriDataManager` and `RunSceneBootstrap` as manager/bootstrap classes, but no `Factory`, `Service`, `Dispatcher`, or skill-specific interface class names under `Pakuri/Assets/Scripts`.
- Unity-MCP `execute_code` result: `catalogNull=False, managerSame=True, monsters=5, enemies=8, firstMonster=ariel, firstActive=5, firstPassive=5`.
- Unity-MCP console warning/error check returned two missing script warnings and MCP client handler logs.
- Test search command `Get-ChildItem Pakuri\Assets -Recurse -Include *Tests*.cs,*Test*.cs` returned no files.

### History

- 2026-05-07: User requested an evidence-based structure review of current character creation, skills, effects, related managers, logic concentration, and pipeline operation, with improvements written as HTML if needed.
- 2026-05-07: Inspected the actual scripts and Unity-MCP catalog state, then generated the HTML review report.
# Task: 2026-05-07 Report Priority 1 Refactor Follow-up

### Task title

Implement first-priority recommendation from character / skill / effect pipeline report.

### Goals

- Record that the report's first recommendation, ID-based `RunSession` learned skill state, has been implemented.

### Constraints

- Role Owner is Code Builder for implementation follow-up.
- Evidence must come from changed files and verification output.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue future report recommendations from `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`, starting with `CombatEffectFactory` extraction if requested.

### Evidence

- The HTML report `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html` listed ID-based `RunSession` learned state as priority 1.
- Implemented ID-based learned state in `RunSession.cs`, `RunCombatUiController.cs`, `DebugSceneController.cs`, `RunFlowController.cs`, `CombatRuntimeController.cs`, `CombatRuntimeScene.cs`, and monster combat skill partials.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity-MCP `execute_code` result: `monster=ariel, activeSkillId=ariel-a, firstLearnedActive=ariel-a, hasSkillId=True, hasDisplayName=False`.

### History

- 2026-05-07: User requested implementation of the first-priority refactor from the generated structure report.
# Task: 2026-05-07 Remove Completed Priority From Pipeline Report

### Task title

Remove completed RunSession ID refactor recommendation from the HTML structure report.

### Goals

- Update `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html` so it no longer lists the completed RunSession ID refactor as an open recommendation or structural problem.

### Constraints

- Role Owner is Designer because this is report/document maintenance.
- Evidence must come from the actual HTML file and search output.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User can reopen `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.
- Remaining first recommendation in the report is now `CombatEffectFactory` / `CombatEffectService` extraction.

### Evidence

- Removed the stale evidence-table row that said `RunSession.LearnedActives` / `LearnedPassives` were display-name based.
- Removed the stale structural problem block about learned skill state depending on display names.
- Removed the completed improvement block titled "런 세션을 ID 기반으로 바꾸기".
- Renumbered remaining improvement directions so `CombatEffectFactory` / `CombatEffectService` is now 1순위.
- Verification search on the HTML found only remaining priority labels 1순위 through 4순위 and no matches for `LearnedActives`, `HasLearnedActive(skill.DisplayName)`, `학습 스킬 상태`, or `RunSession</code>의 ID`.

### History

- 2026-05-07: User said the first-priority RunSession ID refactor was fixed and asked to delete that content from the HTML report.

# Task: 2026-05-07 Combat Effect Factory Refactor Follow-up

### Task title

Implement the remaining first-priority `CombatEffectFactory` recommendation from the character / skill / effect pipeline report.

### Goals

- Record that the report's remaining first recommendation, `CombatEffectFactory` / `CombatEffectService` extraction, has an initial implementation.
- Keep the report board aligned with combat runtime evidence.

### Constraints

- Role Owner is Code Builder for implementation follow-up.
- Evidence must come from changed files and verification output.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- If the HTML report is refreshed later, mark `CombatEffectFactory` extraction as initially implemented and leave pooling/lifetime-service expansion as future work.
- Continue future report recommendations with monster skill runtime module separation after effect visual parity is confirmed.

### Evidence

- The HTML report `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html` listed `CombatEffectFactory` / `CombatEffectService` as the remaining first recommendation.
- Added `Pakuri/Assets/Scripts/Combat/CombatEffectFactory.cs` and Unity generated/imported `CombatEffectFactory.cs.meta`.
- `CombatRuntimeEveSkills.cs` now wraps `CombatEffectFactory.CreateLine(...)` and `CreateCircle(...)` inside the existing `SkillEffectRuntime` return path.
- Direct active-skill effect calls in Ariel, Eve, Rin, Sein, and Vega skill partials now pass `skill.SkillEffectPrefab` when the current skill definition is available.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP console check after importing the new script returned only MCP client handler logs, not project compile errors.

### History

- 2026-05-07: User requested proceeding with the `CombatEffectFactory` / `CombatEffectService` direction described in the pipeline review report.

# Task: 2026-05-08 Manifested Unit Runtime Refactor Design Document

### Task title

Create the step 1 design document for manifested unit runtime ownership.

### Goals

- Record the division between unit component ownership and battlefield controller services.
- Explicitly exclude 1P/EveUnit migration until the user proceeds with step 6.
- Hand off steps 2-5 to Code Builder.

### Constraints

- Role Owner starts as Designer and hands implementation to Code Builder.
- Evidence must come from inspected combat/runtime files.
- Do not run Unity Play Mode.

### Role Owner

Designer -> Code Builder

### Status

Implemented and locally validated with the code pass.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-08-manifested-unit-runtime-refactor-design.md` as the reference before step 6.

### Evidence

- Added `Pakuri/reference/Report/2026-05-08-manifested-unit-runtime-refactor-design.md`.
- The document cites `CombatRuntimeParty.cs`, `CombatRuntimeScene.cs`, `RunSession.ManifestedMonsterIds`, `MonsterDefinition`, `SkillDefinition`, and `RunSession.RunMonsterState` as the basis for the refactor.
- Code Builder then added `CombatUnitRuntime.cs`, `CombatSkillRuntime.cs`, and updated `CombatRuntimeParty.cs`.
- Runtime and Editor builds completed with 0 errors; Unity-MCP console error query returned only MCP client-handler logs after importing the new scripts.

### History

- 2026-05-08: User requested steps 1-5 of the object-oriented manifested runtime refactor before step 6.

# Task: 2026-05-10 Monster OOP Runtime Risk Review HTML

### Task title

Create a current-state HTML review for monster OOP runtime risks and remaining work priority.

### Goals

- Reassess the old `2026-05-08-monster-oop-refactor-manifested-work-status.html` against the current code.
- Explain whether remaining structural work must be done now.
- Evaluate risks for balance tuning, effect hookup, meta global upgrades, and monster placement changes.
- State whether the current structure is fully object-oriented.

### Constraints

- Role Owner is Designer.
- Evidence must come from inspected files and command output.
- This report does not change gameplay code.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User can open `Pakuri/reference/Report/2026-05-10-monster-oop-runtime-risk-and-next-work-review.html`.
- If meta global upgrades or formation/placement changes are implemented next, use this report's risk sections as the design starting point.

### Evidence

- Added `Pakuri/reference/Report/2026-05-10-monster-oop-runtime-risk-and-next-work-review.html`.
- The report cites `CombatUnitRuntime.cs`, `CombatSkillRuntime.cs`, `CombatRuntimeParty.cs`, `RunSession.cs`, `CombatEffectFactory.cs`, `boards/MON/MON_BLACKBOARD.md`, and `boards/COMBAT/COMBAT_BLACKBOARD.md`.
- File check confirmed `Pakuri/reference/Report/2026-05-10-monster-oop-runtime-risk-and-next-work-review.html` exists and has length 16834 bytes.
- Search confirmed the report contains sections for `지금 꼭 해야 하는가`, `완전한 OOP인가`, `meta 전역 강화`, and `몬스터 배치 변경`.

### History

- 2026-05-10: User asked for an HTML report based on the 2026-05-08 manifested OOP work-status report, considering current progress, remaining work priority, structural risks, OOP completeness, balance, effects, meta upgrades, and placement changes.

---

## Source File: `boards/RUN/REWARD_BLACKBOARD.md`

## Task: Run Day Combat Type And Material Rewards

### Task title

Implement run day combat type model, actual prisoner/gold/dark trace rewards, and prisoner offering choices.

### Goals

- Add a run day model for day index and combat type.
- Implement document-based rewards for prisoner, gold, and dark trace.
- Do not implement artifact effects yet.
- Show reward buttons by cloning editable templates under `RewardPanel/RewardButtons`.
- Show prisoner reward types and open the pre-made `PrisonerPanel` for offering choices when a prisoner reward is clicked.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run Code Reviewer once only after implementation.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation and local validation for editable templates, click-to-claim material rewards, always-available ContinueButton, and prisoner offering choice panel completed. User Play Mode verification is complete. User chose to defer the external Code Reviewer run for later.

### Next Actions

- User will run or request the deferred external Code Reviewer review later if needed.

### Evidence

- `Pakuri/reference/4.run/combat-reward-system.md` defines prisoner count chance, boss prisoner guarantee, gold, and dark trace rewards.
- `Pakuri/reference/4.run/dungeon-squad-run-structure.md` defines day-based combat types for normal, midboss, and boss days.
- `RunSession.cs` currently stores stage/day/gold/dark trace/prisoner count but has no explicit combat type model.
- `RunCombatUiController.cs` currently uses fixed `RewardButton_0` to `RewardButton_2` slots under `RewardButtons`.
- Added `Pakuri/Assets/Scripts/Run/RunDayModel.cs` with `RunCombatType` and day-based combat type resolution.
- `RunSession.cs` now tracks `CurrentDayModel`, `CurrentCombatType`, and collected prisoner names.
- `CombatRuntimeController` now builds reward items for prisoners, gold, and dark trace only; artifact rewards and prisoner offering are not implemented.
- `RunCombatUiController.cs` now clones editable `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` templates for prisoner, artifact, and material/other reward display categories.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the 2 existing Unity/MCPForUnity reference warnings.
- Unity refresh requested script compilation; console error query returned only MCP-FOR-UNITY client handler exit logs, not project script compile errors.
- `git diff --check` for changed Run/Combat files returned exit code 0 with CRLF warnings only.
- Unity generated `Pakuri/Assets/Scripts/Run/RunDayModel.cs.meta`.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES` in `codex_loop_logs/run_day_rewards_reviewer_20260428.md`.
- Reviewer finding: `CombatRuntimeRewards.cs` can duplicate prisoner rewards because `BuildRewardPrisoners()` adds guaranteed boss prisoners and then samples `currentNormalEnemyPool`, which can include the same normal enemy used as `currentNormalBossDefinition`.
- User accepted the duplicate prisoner finding as acceptable for now and reported Play Mode test completed.
- `CombatRuntimeController.RewardChoiceView` now carries `PrisonerName`, `GoldAmount`, `DarkTraceAmount`, and `Claimed`.
- `CombatRuntimeRewards.ApplyRewardChoice()` now marks one reward option as claimed and keeps `IsWaitingForRewardChoice` true until all reward options are claimed.
- `RunSession.cs` now exposes `ClaimMaterialReward()` and `ClaimPrisonerReward()` for click-to-claim updates.
- `RunCombatUiController.cs` no longer calls `ApplyPostCombatSummary()` when entering the reward panel; it applies prisoner/material rewards only from clicked reward buttons.
- `RunCombatUiController.cs` now resolves editable templates named `Prisoner`, `Artifact`, and `Material`.
- Unity editor check on loaded `RunScene` found `RewardButtons` children: `RewardPreviewButton`, `Prisoner`, `Artifact`, and `Material`; missing component scan returned `missing=0`.
- Saved `RunScene` after template rename.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the 2 existing Unity/MCPForUnity reference warnings.
- Unity console was cleared and rechecked; error query returned 0 entries.
- User reported Play Mode verification completed for the click-to-claim reward flow and clarified that `ContinueButton` staying active before all rewards are selected is intentional.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/prisoner-choice-system.md`.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md` defines the skill choice pool as unlearned active skills, unlearned passive skills, learned active enhancements, and master skills when conditions exist; candidates under 3 are shown only by remaining count.
- `Pakuri/reference/2.Monster/monster-basic-rule.md` defines run-time acquisition limits as active skills 3 and passive skills 3.
- `RunScene.unity` contains a pre-made inactive `PrisonerPanel` with `Choice1`, `Choice2`, and `Choice3`.
- `MonsterDefinition.cs` contains current data fields available for this prototype: `ActiveSkills`, `PassiveSkills`, and `InitialRewardChoices`; no separate master-skill data model exists yet.
- `RunSession.cs` now records offering choices and learned active/passive skills through `RecordOfferingChoice()`, `HasLearnedActive()`, and `HasLearnedPassive()`.
- `RunCombatUiController.cs` now caches `PrisonerPanel`, opens it from prisoner reward buttons, builds up to 3 shuffled offering choices from actual monster data while respecting the current active/passive acquisition limits, hides unused choice buttons, and returns to `RewardPanel` after a choice.
- `RunCombatUiController.cs` now keeps `ContinueButton` active in reward state so rewards can be skipped.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the 2 existing Unity/MCPForUnity reference warnings after the prisoner offering implementation.
- Unity script refresh completed; console error query returned only MCP-FOR-UNITY client handler exit logs, not project script compile errors.
- User reported Play Mode verification completed for the prisoner offering choice flow.
- User reported no notable Play Mode issues and chose not to run Code Reviewer now; user may run Code Reviewer later.

### History

- 2026-04-28: User requested roadmap steps 2 and 3 together, excluding artifact implementation, and requested reward buttons cloned from one editable template per reward category.
- 2026-04-28: Code Builder implemented the run day combat type model, material reward construction, prisoner display reward items, and template-cloned reward buttons.
- 2026-04-28: External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES`; Code Builder is waiting for user instruction instead of auto-fixing.
- 2026-04-28: User accepted the duplicate prisoner finding, reported Play Mode test completed, and requested editable `Prisoner`, `Material`, `Artifact` templates plus click-to-claim material rewards.
- 2026-04-28: Code Builder changed reward acquisition from reward-panel entry to clicked reward buttons, kept artifact as an editable template only, and saved `RunScene` with editable template names.
- 2026-04-28: User reported Play Mode verification completed and clarified that ContinueButton should remain active even when rewards remain unselected.
- Legacy non-English note retained these code references: `PrisonerPanel`.

- 2026-04-28: User reported Play Mode verification completed for the prisoner offering choice flow.

- 2026-04-28: User reported no notable Play Mode issues and chose to defer the Code Reviewer run until later.

---

## Source File: `boards/RUN/REWARD_BLACKBOARD.md`

## Task: RunScene Reward Button Visibility Fix

### Task title

RunScene stage-clear reward buttons are fixed editable slots and visible when rewards exist

### Goals

- Fix the RunScene issue where stage-clear reward buttons did not appear.
- Keep reward UI objects editable in Edit Mode instead of relying on delete/recreate behavior.
- Preserve authored button labels where possible, while runtime reward labels are still assigned from actual reward data.

### Constraints

- No external reviewer for this task; perform simple self-review only.
- Do not run Unity Play Mode; user performs gameplay verification.
- All claims must be based on actual files, scene state, or command output.

### Role Owner

Code Builder

### Status

Builder fix applied and self-reviewed. Waiting for user Play Mode verification.

### Next Actions

- User verifies RunScene stage clear: reward panel appears with reward buttons, selecting a reward enables the continue flow.
- If reward panel appears but a button is blocked or misplaced, inspect the saved RectTransform values of `RewardPanel`, `RewardButtons`, and `RewardButton_0..2`.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now uses fixed `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` slots under `RewardButtons`.
- `RebuildRewardButtons()` clears only the tracked button list, calls `EnsureRewardButtonSlots(false)`, then activates slots based on `combatController.GetRewardChoiceCount()`.
- `EnsureRewardButtonSlots()` repairs zero-height `RewardButtons`, ensures the three named button slots, and hides non-slot legacy buttons such as `RewardPreviewButton`.
- Existing nonzero reward button slot RectTransforms keep their authored positions/sizes; default positions are applied only when a slot is newly created or has a broken zero size.
- `EnsureButton()` now preserves existing non-empty labels unless an overwrite is explicitly requested or a label is newly created/empty.
- Unity MCP RunScene inspection after `OnEnable` reported `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` active in Edit Mode, and `RewardPreviewButton` inactive.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity console error check after clearing showed MCP-FOR-UNITY client handler exit logs only, not project script compile errors.

### History

- 2026-04-26: User reported RunScene reward buttons do not appear.
- 2026-04-26: Scene inspection found `RewardButtons` previously had zero height and fixed reward slots were missing, while monster assets contained reward choice data.
- 2026-04-26: Added persistent reward slots, repaired reward root sizing, hid legacy preview buttons, and made existing RunScene reward UI visible in Edit Mode.

---

## Source File: `boards/RUN/REWARD_BLACKBOARD.md`

## Task: Monster Select Run UI Expansion Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `2.Monster`, `skill-choice-pool-rule.md`, `combat-reward-system.md`.
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
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `g~j`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `Main Camera`, `Global Light 2D`, `CombatRoot`.
- Legacy non-English note retained these code references: `CombatRoot`, `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`, `ProjectileRoot`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `NO_UI_TOOLKIT_ASSETS`, `NO_UI_NAMED_ASSETS`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/monster-basic-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `eve/eve-tower.md`, `rin/rin-tower.md`, `sein/sein-tower.md`, `vega/vega-tower.md`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `f~j`, `f-ambidextrous.md`, `g~j`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `2.Monster`, `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `combat-reward-system.md`, `dungeon-squad-run-structure.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note retained these code references: `g~j`.
- Legacy non-English note retained these ASCII code references: `, `.

---

## Source File: `boards/RUN/REWARD_BLACKBOARD.md`

## Task: Run Flow UICanvas Prototype Implementation

### Task title

Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.

### Goals

- Legacy non-English note retained these code references: `RunSession`, `RunFlowController`, `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

### Next Actions

- Legacy non-English note retained these code references: `RunUICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `B/G, C/H, D/I, E/J`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Seed Default Game Data`, `Assets/Data/GameData/GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `RunUICanvas`, `EventSystem`.
- Legacy non-English note retained these code references: `Assets/Data/GameData/GameDataCatalog.asset`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`, `EventSystem`, `InputSystemUIInputModule`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `stageIndex`.
- Legacy non-English note retained these code references: `RunFlowController.ClearButtons(...)`, `QueuedForDestroy`.
- Legacy non-English note retained these code references: `RunFlowController.ResolveReferences()`, `Arial.ttf`, `ArgumentException`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`, `Arial.ttf`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `EventSystem`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Resources.GetBuiltinResource<Font>("Arial.ttf")`, `RunFlowController`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

# Task: 2026-05-08 Prisoner Choice Reward Flow

### Task title

Route prisoner rewards through choice UI before Offering or Manifest.

### Goals

- Make prisoner reward selection open `PrisonerChoicePanel` first.
- Add Manifest, Assimilate, Offering, and Torture/Corrupt choice buttons.
- Preserve existing Offering behavior through `PrisonerPanel`.
- Make Assimilate and Torture/Corrupt clickable but non-functional for now.
- Make Manifest show result data and return to the normal reward-continue flow.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected files and build output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that each prisoner choice button activates the expected panel or placeholder behavior.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` changed prisoner reward handling from direct `OpenPrisonerPanel(...)` to `OpenPrisonerChoicePanel(...)`.
- `RunCombatUiController.cs` now creates `PrisonerChoicePanel` with Manifest, Assimilate, Offering, and Torture/Corrupt buttons.
- `RunCombatUiController.cs` keeps Offering routed to the existing `OpenPrisonerPanel(...)` path.
- `RunCombatUiController.cs` creates `PrisonerSummonerPanel` and displays monster image, name/title, A skill description, and basic stats on Manifest candidate/result.
- `RunCombatUiController.cs` adds a Manifest result `ContinueButton` so success/failure returns to `RewardPanel` and the existing continue flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested prisoner rewards no longer jump directly into Offering and instead open a prisoner choice UI with Manifest/Assimilate/Offering/Torture-Corrupt options.

# Task: 2026-05-08 Reward Panel Runtime Visibility Gate

### Task title

Keep reward and prisoner panels hidden until reward logic activates them.

### Goals

- Hide Reward/Prisoner/Manifest/Offering/Defeat UI on RunScene runtime entry.
- Preserve reward victory flow as the only path that activates `RewardPanel`.
- Preserve prisoner reward flow as the only path that activates prisoner choice, Manifest, and Offering panels.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code and build output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that reward/prisoner panels do not appear before victory and reward interaction.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:438` through `:447` hides reward, prisoner, prisoner choice, prisoner summoner, defeat, and `PrisonerOfferingPanel` on runtime HUD-only state.
- `RunCombatUiController.cs:453` activates `RewardPanel` only in `EnterRewardState()`.
- `RunCombatUiController.cs:590`, `:624`, and `:823` activate prisoner choice, Manifest, and Offering panels only from prisoner reward interactions.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User asked to check that all non-HUD/Monster UI is hidden at RunScene entry and only opens according to game logic.

# Task: 2026-05-08 Prisoner Reward Click Opens Choice Panel

### Task title

Fix prisoner reward click showing only claimed state without opening prisoner choice UI.

### Goals

- Ensure a prisoner reward click opens `PrisonerChoicePanel`.
- Avoid rebuilding the reward list into the claimed/completed visual before the prisoner choice panel opens.
- Make prisoner reward detection robust if one reward view field is inconsistent.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code, build output, and Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that clicking a prisoner reward opens `PrisonerChoicePanel` immediately instead of leaving only the claimed label on `RewardPanel`.

### Evidence

- Before the fix, `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` called `RebuildRewardButtons()` before checking whether the selected reward was prisoner, so the visible reward list could be rebuilt into the claimed state before panel transition.
- `CombatRuntimeRewards.cs` creates prisoner reward options with `RewardId = "prisoner:..."`, `RewardKind = "Prisoner"`, and `PrisonerName`.
- `RunCombatUiController.cs` now checks `IsPrisonerReward(rewardView, rewardId)` before rebuilding reward buttons.
- `RunCombatUiController.cs` now treats a reward as prisoner when `RewardKind == "Prisoner"` or when `PrisonerName` is present and `rewardId` starts with `prisoner:`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh completed to idle, and console error query returned only MCP client-handler logs.

### History

- 2026-05-08: User reported that clicking a prisoner reward in `RewardPanel` only showed the acquired/completed state and did not open any prisoner choice window.

# Task: 2026-05-08 PrisonerChoicePanel Per-Frame Hide Fix

### Task title

Keep prisoner choice/reward modals open after a prisoner reward click.

### Goals

- Fix the remaining prisoner reward click bug where `PrisonerChoicePanel` was opened and then immediately hidden.
- Keep non-prisoner reward clicks on the existing claimed-list rebuild path.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code, build output, and Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify prisoner reward click, Manifest panel open, Offering panel open, and return-to-reward behavior.

### Evidence

- `CombatRuntimeRewards.cs:177` through `:181` creates prisoner rewards with `RewardId = "prisoner:..."`, `RewardKind = "Prisoner"`, and `PrisonerName`.
- `CombatRuntimeController.cs:178` through `:204` exposes `RewardChoiceView.RewardKind` and `RewardChoiceView.PrisonerName`.
- `RunCombatUiController.cs:566` through `:568` checks prisoner reward status before reward-button rebuild and opens `OpenPrisonerChoicePanel(...)`.
- `RunCombatUiController.cs:599` through `:603` activates `PrisonerChoicePanel`.
- `RunCombatUiController.cs:458` through `:480` shows why the bug persisted: `EnterRewardState()` hides prisoner panels.
- `RunCombatUiController.cs:157` through `:164` now skips `EnterRewardState()` while a prisoner reward modal is active.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing Unity/MCPForUnity reference warnings.
- Unity-MCP console read after script refresh showed existing missing-script/MCP entries, not C# compile errors.

### History

- 2026-05-08: User reported that the reward button still did not show `PrisonerChoicePanel`.
- 2026-05-08: Builder identified the first fix was incomplete because the victory `Update()` loop re-entered reward state every frame and hid the newly opened panel.

# Task: 2026-05-08 Prisoner Offering And Reward Used State Follow-up

### Task title

Route Offering to `PrisonerOfferingPanel` and rebuild reward buttons after prisoner modal returns.

### Goals

- Use the scene-authored `PrisonerOfferingPanel` as the actual Offering UI.
- Prevent the legacy/generated `PrisonerPanel` from appearing when Offering is clicked.
- Show the prisoner reward button as used/claimed after returning from Manifest or Offering.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected scene hierarchy, changed code, and build/Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify Offering and Manifest return behavior from the reward panel.

### Evidence

- Unity-MCP scene inspection found `RunCombatCanvas/PrisonerOfferingPanel` with `Choice1`, `Choice2`, `Choice3`, and `Title`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now binds offering title/buttons from `PrisonerOfferingPanel` first, falling back to `PrisonerPanel` only if needed.
- `RunCombatUiController.cs` now hides `PrisonerPanel` and activates `PrisonerOfferingPanel` in the Offering flow.
- `RunCombatUiController.cs` resets `rewardPanelEntered = false` after Manifest result close and after committed Offering, so `EnterRewardState()` rebuilds reward buttons and reflects claimed state.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported Offering opened `PrisonerPanel`, and returning from Manifest did not visibly mark the prisoner reward button as used.
- 2026-05-08: Code Builder routed Offering to `PrisonerOfferingPanel` and forced reward-button refresh after prisoner modal flows.

# Task: 2026-05-08 Offering Rewards Target Party Members

### Task title

Make Offering rewards apply to selected and Manifested monster states.

### Goals

- Generate Offering choices for every current run party member, including Manifested monsters.
- Track chosen rewards and learned skills per monster ID so one monster's Offering state does not block or overwrite another's.
- Preserve selected-monster legacy learned lists for existing 1P combat code while adding party-member scoped state.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected Run reward/UI code and build output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that Offering can give a Manifested monster a new skill/modifier and that the monster uses that learned state in later combat.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:884` builds Offering choices from `ResolveOfferingTargetMonsters()`.
- `RunCombatUiController.cs:943`, `:977`, `:1016`, `:1054`, `:1074`, and `:1094` take `RunSession.RunMonsterState` for active, passive, enhancement, and master Offering choices.
- `RunCombatUiController.cs:968`, `:1007`, `:1040`, and `:1127` store `MonsterId = memberState.MonsterId` on generated choices.
- `RunCombatUiController.cs:1206` records the selected Offering choice against `choice.MonsterId`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs:166` records monster-ID scoped Offering choices.
- `RunSession.cs:218` and `:229` check learned active/passive skills by monster ID.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing Unity reference warnings.
- `git diff --check` on the changed Run/Combat scripts completed with no whitespace errors, aside from Git LF-to-CRLF normalization warnings.

### History

- 2026-05-08: User clarified Manifested monsters must also grow through Offering after joining the run.
- 2026-05-08: Code Builder changed Offering generation and commit paths to carry a party-member `MonsterId`.

# Task: 2026-05-08 Summoner Return Without Manifest

### Task title

Add a `PrisonerSummonerPanel` return button.

### Goals

- Let the player leave `PrisonerSummonerPanel` and return to `RewardPanel` without attempting Manifest.
- Keep the existing result `ContinueButton` for success/failure result close.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected UI code, saved scene YAML, build output, and Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that `Back to Reward` leaves the summoner panel without adding a Manifested monster.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:64` declares `prisonerSummonerBackButton`.
- `RunCombatUiController.cs:390` creates/binds `BackButton` with label `Back to Reward`.
- `RunCombatUiController.cs:731` clears the pending Manifest candidate and returns to the reward panel.
- `Pakuri/Assets/Scenes/RunScene.unity:5233` contains `m_Name: BackButton`.
- `Pakuri/Assets/Scenes/RunScene.unity:8429` contains `m_Text: Back to Reward`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested a button on `PrisonerSummonerPanel` that returns to `RewardPanel` without summoning.
- 2026-05-08: Code Builder added `BackButton` and wired it to the no-Manifest return path.

# Task: 2026-05-08 Manifested Offering Skill Refresh Follow-up

### Task title

Refresh Manifested party state after Manifest and Offering results.

### Goals

- Ensure the first successful Manifest result is visible to the run/combat party state immediately.
- Ensure Offering choices that target Manifested monsters update the Manifested skill runtime snapshot.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that a Manifested monster receiving an Offering skill uses that learned skill in later combat.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:702` refreshes Manifested runtime after Manifest success.
- `RunCombatUiController.cs:1246` refreshes Manifested runtime after `CommitOfferingChoice(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:149` reconfigures Manifested party members from `RunSession`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User asked to check whether Offering-acquired skills on Manifested monsters actually fire.
- 2026-05-08: Code Builder verified the monster-ID Offering path and added immediate Manifested party refresh after Offering.
# Task: 2026-05-08 Offering-Acquired Manifested Skill Visual Follow-up

### Task title

Record Offering-acquired Manifested skills using skill-kind combat visuals.

### Goals

- Keep Offering target identity and learned-skill commit behavior unchanged.
- Fix the combat-side visual result of Offering-acquired Manifested skills.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Offering-acquired Manifested active skills in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:1206` records Offering choices against `choice.MonsterId`.
- `RunCombatUiController.cs:1246` refreshes the Manifested combat runtime after Offering commit.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:896` now creates Manifested non-projectile visuals from `SkillRuntimeKind` and `SkillEffectPrefab`.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported the reward/Offering side worked but the resulting Manifested skill visual was still wrong.

# Task: 2026-05-08 Offering-Acquired Manifested Sustained Duration Follow-up

### Task title

Record that Offering-acquired Manifested sustained skills now use longer visual durations.

### Goals

- Preserve the Offering path that grants skills to Manifested monster state.
- Keep the combat-side sustained visual duration fix tied to Offering-acquired skills.

### Constraints

- Role Owner is Code Builder.
- No reward UI code changed in this pass.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated through combat runtime changes.

### Next Actions

- User verifies Offering-acquired Manifested sustained skills in later combat.

### Evidence

- Existing `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:1246` refreshes Manifested combat state after Offering commit.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now uses `ResolveManifestedSkillVisualDuration(...)` for sustained learned skill visuals.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported Offering acquisition and cooldown worked, then reported sustained effects were too short.

# Task: 2026-05-08 Manifested Offering Runtime Ownership Follow-up

### Task title

Keep Offering-acquired Manifested skills on the Manifested unit component runtime.

### Goals

- Preserve monster-ID scoped Offering commit behavior.
- Ensure manifested combat reads learned skills from each party member state into that unit's own runtime list.
- Keep reward UI code unchanged in this pass.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated through combat runtime changes.

### Next Actions

- User verifies that an Offering choice targeting a Manifested monster still upgrades that manifested unit's later combat skill behavior.

### Evidence

- Existing `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:1206` records Offering choices against `choice.MonsterId`.
- Existing `RunCombatUiController.cs:1246` refreshes Manifested combat state after Offering commit.
- `Pakuri/Assets/Scripts/Combat/CombatUnitRuntime.cs` now owns the learned skill runtime list for a manifested unit.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` still syncs `RunSession.RunMonsterState.LearnedActives` into manifested learned skill runtimes, now on `CombatUnitRuntime`.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User asked to perform object-oriented manifested runtime refactor steps 1-5 before later deciding step 6.

# Task: 2026-05-08 Manifest Button Roll And Failure Popup

### Task title

Move prisoner Manifest roll to the choice button and display failure popup.

### Goals

- Make `ManifestButton` perform the success/failure roll directly.
- Keep successful Manifest using the existing reward-return flow.
- Show a distinct failure popup when Manifest fails.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user performs gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies prisoner reward -> Manifest success/failure UI in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:367` binds `PrisonerChoicePanel/ManifestButton` to `TryManifestPrisonerMonster`.
- `RunCombatUiController.cs:391` binds `PrisonerSummonerPanel/SummonButton` to result close instead of the roll method.
- `RunCombatUiController.cs:396` through `:400` creates `PrisonerManifestFailurePopup`.
- `RunCombatUiController.cs:700` through `:722` implements failure popup display and close behavior.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested the Manifest chance to happen on `ManifestButton` rather than `SummonButton`, with a popup on failure.

---

## Source File: `boards/RUN/RUN_BLACKBOARD.md`

## Task: Run Day Combat Type And Material Rewards

### Task title

Implement run day combat type model, actual prisoner/gold/dark trace rewards, and prisoner offering choices.

### Goals

- Add a run day model for day index and combat type.
- Implement document-based rewards for prisoner, gold, and dark trace.
- Do not implement artifact effects yet.
- Show reward buttons by cloning editable templates under `RewardPanel/RewardButtons`.
- Show prisoner reward types and open the pre-made `PrisonerPanel` for offering choices when a prisoner reward is clicked.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run Code Reviewer once only after implementation.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation and local validation for editable templates, click-to-claim material rewards, always-available ContinueButton, and prisoner offering choice panel completed. User Play Mode verification is complete. User chose to defer the external Code Reviewer run for later.

### Next Actions

- User will run or request the deferred external Code Reviewer review later if needed.

### Evidence

- `Pakuri/reference/4.run/combat-reward-system.md` defines prisoner count chance, boss prisoner guarantee, gold, and dark trace rewards.
- `Pakuri/reference/4.run/dungeon-squad-run-structure.md` defines day-based combat types for normal, midboss, and boss days.
- `RunSession.cs` currently stores stage/day/gold/dark trace/prisoner count but has no explicit combat type model.
- `RunCombatUiController.cs` currently uses fixed `RewardButton_0` to `RewardButton_2` slots under `RewardButtons`.
- Added `Pakuri/Assets/Scripts/Run/RunDayModel.cs` with `RunCombatType` and day-based combat type resolution.
- `RunSession.cs` now tracks `CurrentDayModel`, `CurrentCombatType`, and collected prisoner names.
- `CombatRuntimeController` now builds reward items for prisoners, gold, and dark trace only; artifact rewards and prisoner offering are not implemented.
- `RunCombatUiController.cs` now clones editable `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` templates for prisoner, artifact, and material/other reward display categories.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the 2 existing Unity/MCPForUnity reference warnings.
- Unity refresh requested script compilation; console error query returned only MCP-FOR-UNITY client handler exit logs, not project script compile errors.
- `git diff --check` for changed Run/Combat files returned exit code 0 with CRLF warnings only.
- Unity generated `Pakuri/Assets/Scripts/Run/RunDayModel.cs.meta`.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES` in `codex_loop_logs/run_day_rewards_reviewer_20260428.md`.
- Reviewer finding: `CombatRuntimeRewards.cs` can duplicate prisoner rewards because `BuildRewardPrisoners()` adds guaranteed boss prisoners and then samples `currentNormalEnemyPool`, which can include the same normal enemy used as `currentNormalBossDefinition`.
- User accepted the duplicate prisoner finding as acceptable for now and reported Play Mode test completed.
- `CombatRuntimeController.RewardChoiceView` now carries `PrisonerName`, `GoldAmount`, `DarkTraceAmount`, and `Claimed`.
- `CombatRuntimeRewards.ApplyRewardChoice()` now marks one reward option as claimed and keeps `IsWaitingForRewardChoice` true until all reward options are claimed.
- `RunSession.cs` now exposes `ClaimMaterialReward()` and `ClaimPrisonerReward()` for click-to-claim updates.
- `RunCombatUiController.cs` no longer calls `ApplyPostCombatSummary()` when entering the reward panel; it applies prisoner/material rewards only from clicked reward buttons.
- `RunCombatUiController.cs` now resolves editable templates named `Prisoner`, `Artifact`, and `Material`.
- Unity editor check on loaded `RunScene` found `RewardButtons` children: `RewardPreviewButton`, `Prisoner`, `Artifact`, and `Material`; missing component scan returned `missing=0`.
- Saved `RunScene` after template rename.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the 2 existing Unity/MCPForUnity reference warnings.
- Unity console was cleared and rechecked; error query returned 0 entries.
- User reported Play Mode verification completed for the click-to-claim reward flow and clarified that `ContinueButton` staying active before all rewards are selected is intentional.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/prisoner-choice-system.md`.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md` defines the skill choice pool as unlearned active skills, unlearned passive skills, learned active enhancements, and master skills when conditions exist; candidates under 3 are shown only by remaining count.
- `Pakuri/reference/2.Monster/monster-basic-rule.md` defines run-time acquisition limits as active skills 3 and passive skills 3.
- `RunScene.unity` contains a pre-made inactive `PrisonerPanel` with `Choice1`, `Choice2`, and `Choice3`.
- `MonsterDefinition.cs` contains current data fields available for this prototype: `ActiveSkills`, `PassiveSkills`, and `InitialRewardChoices`; no separate master-skill data model exists yet.
- `RunSession.cs` now records offering choices and learned active/passive skills through `RecordOfferingChoice()`, `HasLearnedActive()`, and `HasLearnedPassive()`.
- `RunCombatUiController.cs` now caches `PrisonerPanel`, opens it from prisoner reward buttons, builds up to 3 shuffled offering choices from actual monster data while respecting the current active/passive acquisition limits, hides unused choice buttons, and returns to `RewardPanel` after a choice.
- `RunCombatUiController.cs` now keeps `ContinueButton` active in reward state so rewards can be skipped.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the 2 existing Unity/MCPForUnity reference warnings after the prisoner offering implementation.
- Unity script refresh completed; console error query returned only MCP-FOR-UNITY client handler exit logs, not project script compile errors.
- User reported Play Mode verification completed for the prisoner offering choice flow.
- User reported no notable Play Mode issues and chose not to run Code Reviewer now; user may run Code Reviewer later.

### History

- 2026-04-28: User requested roadmap steps 2 and 3 together, excluding artifact implementation, and requested reward buttons cloned from one editable template per reward category.
- 2026-04-28: Code Builder implemented the run day combat type model, material reward construction, prisoner display reward items, and template-cloned reward buttons.
- 2026-04-28: External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES`; Code Builder is waiting for user instruction instead of auto-fixing.
- 2026-04-28: User accepted the duplicate prisoner finding, reported Play Mode test completed, and requested editable `Prisoner`, `Material`, `Artifact` templates plus click-to-claim material rewards.
- 2026-04-28: Code Builder changed reward acquisition from reward-panel entry to clicked reward buttons, kept artifact as an editable template only, and saved `RunScene` with editable template names.
- 2026-04-28: User reported Play Mode verification completed and clarified that ContinueButton should remain active even when rewards remain unselected.
- Legacy non-English note retained these code references: `PrisonerPanel`.

- 2026-04-28: User reported Play Mode verification completed for the prisoner offering choice flow.

- 2026-04-28: User reported no notable Play Mode issues and chose to defer the Code Reviewer run until later.

---

## Source File: `boards/RUN/RUN_BLACKBOARD.md`

## Task: Main Menu To RunScene Flow Separation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `RunScene`, `MainMenuScene`.
- Legacy non-English note retained these ASCII code references: `MainMenuScene`.
- Legacy non-English note retained these code references: `RunScene`, `RunSession`.
- Legacy non-English note retained these code references: `DontDestroyOnLoad`, `RunStartContext`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Next Actions

- Legacy non-English note retained these ASCII code references: `MainMenuScene`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunStartContext.cs`, `RunSession`, `DontDestroyOnLoad`.
- Legacy non-English note retained these ASCII code references: `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`, `Touch To Start`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs`, `RunScene`, `RunStartContext`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `using System;`, `Serializable`, `StringComparison`, `Math`.
- Legacy non-English note retained these code references: `RunScene`, `RunUICanvas`, `RunSceneBootstrap`.
- Legacy non-English note retained these code references: `MainMenuScene`, `MainMenuCanvas`, `MainMenuFlowController`, `EventSystem`.
- Legacy non-English note retained these code references: `Pakuri/ProjectSettings/EditorBuildSettings.asset`, `Assets/Scenes/MainMenuScene.unity`, `Assets/Scenes/RunScene.unity`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `SampleScene.unity`, `MainMenuScene.unity`, `RunScene.unity`.
- Legacy non-English note retained these code references: `RunScene.unity`, `RunUICanvas`, `RunFlowController`.
- Legacy non-English note retained these code references: `RunStartContext`, `MainMenuFlowController`, `RunSceneBootstrap`, `RunScene`, `MainMenuScene`.
- Legacy non-English note retained these code references: `dotnet build`.

---

## Source File: `boards/RUN/RUN_BLACKBOARD.md`

## Task: Monster Select Run UI Expansion Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `2.Monster`, `skill-choice-pool-rule.md`, `combat-reward-system.md`.
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
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `g~j`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `Main Camera`, `Global Light 2D`, `CombatRoot`.
- Legacy non-English note retained these code references: `CombatRoot`, `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`, `ProjectileRoot`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `NO_UI_TOOLKIT_ASSETS`, `NO_UI_NAMED_ASSETS`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/monster-basic-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `eve/eve-tower.md`, `rin/rin-tower.md`, `sein/sein-tower.md`, `vega/vega-tower.md`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `f~j`, `f-ambidextrous.md`, `g~j`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `2.Monster`, `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `combat-reward-system.md`, `dungeon-squad-run-structure.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note retained these code references: `g~j`.
- Legacy non-English note retained these ASCII code references: `, `.

---

## Source File: `boards/RUN/RUN_BLACKBOARD.md`

## Task: SaveAndLoad Direction Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `reference/4.run`, `reference/6.meta`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
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

- Legacy non-English note retained these code references: `RunSession`, `MetaSaveData`, `RunSnapshot`, `SaveLoadService`.
- Legacy non-English note retained these code references: `GameDataCatalog`, `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/4.run/dungeon-squad-run-structure.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/shop-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/event-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-index.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/active-skill-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/dark-trace-currency-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`, `Assets/Resources`, `Assets/StreamingAssets`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/save-and-load-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `monster-select-run-ui-expansion-plan.html`, `monster-select-run-ui-builder-handoff.html`, `reference/4.run`, `reference/6.meta`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `MetaSaveData`, `RunSnapshot`, `EphemeralRuntime`, `Pakuri/reference/save-and-load-plan.html`.
- Legacy non-English note retained these code references: `Pakuri/data`, `save-and-load-plan.html`.

---

## Source File: `boards/RUN/RUN_BLACKBOARD.md`

## Task: Run Systems Integration Summary Report

### Task title

Legacy non-English note retained these code references: `monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan`.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.

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
- Legacy non-English note retained these code references: `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`, `MetaSaveData`, `RunSnapshot`, `GameDataCatalog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `Scenes`, `Screenshots`, `Scripts`, `Settings`, `Resources`, `StreamingAssets`, `DataGenerated`.
- Legacy non-English note retained these code references: `.uxml`, `.uss`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/rin/rin-tower.md`, `rin/skill/g~j`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/4.run/combat-reward-system.md`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `RunSession`, `run-systems-integration-summary-report.html`.

---

## Source File: `boards/RUN/RUN_BLACKBOARD.md`

## Task: Run Flow UICanvas Prototype Implementation

### Task title

Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.

### Goals

- Legacy non-English note retained these code references: `RunSession`, `RunFlowController`, `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

### Next Actions

- Legacy non-English note retained these code references: `RunUICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `B/G, C/H, D/I, E/J`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Seed Default Game Data`, `Assets/Data/GameData/GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `RunUICanvas`, `EventSystem`.
- Legacy non-English note retained these code references: `Assets/Data/GameData/GameDataCatalog.asset`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`, `EventSystem`, `InputSystemUIInputModule`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `stageIndex`.
- Legacy non-English note retained these code references: `RunFlowController.ClearButtons(...)`, `QueuedForDestroy`.
- Legacy non-English note retained these code references: `RunFlowController.ResolveReferences()`, `Arial.ttf`, `ArgumentException`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`, `Arial.ttf`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `EventSystem`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Resources.GetBuiltinResource<Font>("Arial.ttf")`, `RunFlowController`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

# Task: 2026-05-07 Character Skill Effect Pipeline Review

### Task title

Run character selection and session structure review summary

### Goals

- Preserve run-side conclusions from the structure review.

### Constraints

- Evidence must come from inspected scripts and Unity-MCP output.
- Designer review only; no run code implementation was performed.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- See `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.
- Future run work should move learned active/passive state from display-name strings to stable skill/passive IDs.

### Evidence

- `MainMenuFlowController.StartRun` calls `RunStartContext.Ensure().PrepareNewRun(selectedMonster)`.
- `RunStartContext.cs` stores `SelectedMonster` and `RunSession`, and keeps the context with `DontDestroyOnLoad`.
- `RunSceneBootstrap.cs` starts combat from pending context or fallback monster.
- `RunSession.cs` stores `LearnedActives` and `LearnedPassives` as `List<string>`, and `RunCombatUiController.cs` checks learned actives with `skill.DisplayName`.
- Report created at `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.

### History

- 2026-05-07: User requested current character creation, skill, and effect pipeline review. Designer documented the run selection/session flow and the display-name learned-skill risk.
# Task: 2026-05-07 RunSession Learned Skill ID Refactor

### Task title

Refactor RunSession learned active/passive state to store stable skill IDs.

### Goals

- Store learned active skills as `SkillDefinition.SkillId` values in `RunSession.LearnedActives`.
- Store learned passive skills as `PassiveDefinition.PassiveId` values in `RunSession.LearnedPassives`.
- Keep display text sourced from definitions such as `SkillDefinition.DisplayName` instead of using display names for learned-state logic.

### Constraints

- Role Owner is Code Builder because the user explicitly requested refactoring implementation.
- Evidence must come from inspected code, build output, and Unity-MCP output.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should verify in Play Mode that newly offered active/passive choices unlock and enhance the expected skills.
- Code Reviewer execution remains deferred until explicit user permission.

### Evidence

- Changed `Pakuri/Assets/Scripts/Run/RunSession.cs` to add `ActiveSkillId`/`PassiveSkillId`, resolve default active ID from `IsDefaultLearned` or slot A, and use `AddLearnedActive`/`AddLearnedPassive` with IDs.
- Changed `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` so offering choices store `ActiveSkillId`/`PassiveSkillId`, and `HasLearnedActive`/`HasLearnedPassive` checks use `skill.SkillId`/`passive.PassiveId`.
- Changed `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` so debug sessions add selected active/passive IDs instead of display names.
- Changed `Pakuri/Assets/Scripts/Run/RunFlowController.cs` so passive reward unlock passes `SelectedMonsterPassiveId`.
- Search evidence after edits found no remaining `HasLearnedActive(skill.DisplayName)`, `HasLearnedPassive(passive.DisplayName)`, `session.LearnedActives.Add(skill.DisplayName)`, or `session.LearnedPassives.Add(passive.DisplayName)` matches under `Pakuri/Assets/Scripts`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity-MCP `execute_code` result: `monster=ariel, activeSkillId=ariel-a, firstLearnedActive=ariel-a, hasSkillId=True, hasDisplayName=False`.
- Unity-MCP console warning/error check after compile returned only MCP client handler logs.

### History

- 2026-05-07: User asked to begin refactoring from the report's first priority: make `RunSession.LearnedActives` and `LearnedPassives` ID based.
- 2026-05-07: Code Builder implemented the ID-based learned-state path and validated build/editor behavior without Play Mode.

# Task: 2026-05-08 RunScene Prisoner Manifest Party Implementation

### Task title

Record prisoner Manifest results in `RunSession` and feed the next combat party.

### Goals

- Store Manifested monster IDs in the active run session.
- Keep 1P as the selected monster and add Manifested monsters from 2P onward on the next combat start.
- Keep initial Manifest combat participation limited to automatic A/basic attack behavior.

### Constraints

- Role Owner is Code Builder because the user explicitly requested implementation.
- Evidence must come from inspected files and build/Unity-MCP output.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify prisoner reward -> choice -> Manifest success/failure -> next combat party behavior.
- Code Reviewer execution remains deferred until explicit user permission.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunSession.cs` now stores `ManifestedMonsterIds`, `HasManifestedMonster(...)`, and `RecordManifestedMonster(...)`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` records successful Manifest results through `currentSession.RecordManifestedMonster(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` calls `ConfigureManifestedMonsterParty(session)` when a configured day begins.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` resolves `RunSession.ManifestedMonsterIds` through `PakuriDataManager`, skips the selected monster, and exposes party panel data with 1P at index 0 and Manifested monsters from index 1.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP imported `Assets/Scripts/Combat/CombatRuntimeParty.cs`; Unity console error query returned only MCP client-handler logs, not project compile errors.

### History

- 2026-05-08: User requested the recommended RunScene order: Rin CSV/SO cleanup, Manifest result storage, next-combat party read, 2P+ display, and limited A/basic auto-combat.

# Task: 2026-05-08 RunScene Runtime UI State Gate

### Task title

Start RunScene runtime with combat HUD only and let game logic own later panel transitions.

### Goals

- Prevent editor-visible panels from leaking into the initial RunScene runtime state.
- Keep `HudPanel` and `MonsterPanel` available during combat.
- Keep reward/prisoner/Manifest/defeat panels controlled by victory/reward/prisoner/defeat logic.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code, scene hierarchy, build output, and Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that the initial combat state shows only HUD/Monster UI and that victory/defeat/reward actions still reveal the correct panels.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:111` calls `ShowRuntimeHudOnly()` from Play-mode `OnEnable`, so runtime state is applied before the existing `Start()` call at `:138`.
- `RunCombatUiController.cs:438` through `:447` hides reward, prisoner, prisoner choice, prisoner summoner, defeat, and legacy `PrisonerOfferingPanel`, while keeping `HudPanel` and `MonsterPanel` active.
- `RunCombatUiController.cs:453`, `:590`, `:624`, `:823`, `:1170`, and `:1192` remain the game-logic activation points for reward, prisoner choice, Manifest, Offering, continue, and defeat states.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP console error query after script refresh returned only MCP client-handler logs.

### History

- 2026-05-08: User requested checking and enforcing the Play 기준 where all UI may be active before RunScene entry but only `HudPanel` and `MonsterPanel` remain active on entry.
# Task: 2026-05-08 Prisoner Reward Transition Bugfix

### Task title

Keep prisoner reward selection on the prisoner-choice transition path.

### Goals

- Make prisoner reward selection leave the reward list and enter prisoner choice UI.
- Preserve normal reward-list rebuild behavior for non-prisoner rewards.

### Constraints

- Role Owner is Code Builder.
- Detailed reward/UI evidence is recorded in `boards/RUN/REWARD_BLACKBOARD.md` and `boards/UI/RUNSCENE_UI.md`.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify the prisoner reward transition from victory reward state.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now evaluates prisoner rewards with `IsPrisonerReward(rewardView, rewardId)` before reward-button rebuild.
- Non-prisoner rewards still call `RebuildRewardButtons()` and then `EnterRewardState()`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported the prisoner reward click did not open a panel and only changed the button to acquired.

---

## Source File: `boards/RUN/SAVELOAD_BLACKBOARD.md`

## Task: SaveAndLoad Direction Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `reference/4.run`, `reference/6.meta`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
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

- Legacy non-English note retained these code references: `RunSession`, `MetaSaveData`, `RunSnapshot`, `SaveLoadService`.
- Legacy non-English note retained these code references: `GameDataCatalog`, `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/4.run/dungeon-squad-run-structure.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/shop-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/event-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-index.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/meta-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/active-skill-growth-node-list.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/6.meta/dark-trace-currency-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/data`, `Assets`, `Assets/Resources`, `Assets/StreamingAssets`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/save-and-load-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `monster-select-run-ui-expansion-plan.html`, `monster-select-run-ui-builder-handoff.html`, `reference/4.run`, `reference/6.meta`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `MetaSaveData`, `RunSnapshot`, `EphemeralRuntime`, `Pakuri/reference/save-and-load-plan.html`.
- Legacy non-English note retained these code references: `Pakuri/data`, `save-and-load-plan.html`.

---

## Source File: `boards/RUN/SAVELOAD_BLACKBOARD.md`

## Task: Run Systems Integration Summary Report

### Task title

Legacy non-English note retained these code references: `monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan`.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.

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
- Legacy non-English note retained these code references: `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`, `MetaSaveData`, `RunSnapshot`, `GameDataCatalog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `Scenes`, `Screenshots`, `Scripts`, `Settings`, `Resources`, `StreamingAssets`, `DataGenerated`.
- Legacy non-English note retained these code references: `.uxml`, `.uss`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/rin/rin-tower.md`, `rin/skill/g~j`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/4.run/combat-reward-system.md`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `RunSession`, `run-systems-integration-summary-report.html`.

---

## Source File: `boards/UI/UI_BLACKBOARD.md`

## Task: DebugScene UI Canvas Retrospective Report

### Task title

DebugScene UI Canvas initial approach, user corrections, and fix history HTML report.

### Goals

- Analyze the recent DebugScene UI Canvas work log.
- Summarize the initial runtime-generated UI approach, user correction requests, reviewer findings, and final scene-bound UI solution.
- Write the result as an HTML report under `Pakuri/reference/Report`.

### Constraints

- Role Owner is Designer.
- Ground all claims in actual files, code, and command output.
- Do not implement runtime gameplay changes for this report.
- Preserve the repository rule that Play Mode gameplay verification is user-owned.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Use `Pakuri/reference/Report/2026-04-30-debugscene-ui-canvas-retrospective.html` as the current written summary if the DebugScene UI flow needs to be discussed again.

### Evidence

- `Get-Content -LiteralPath AGENTS.md` and `Get-Content -LiteralPath BLACKBOARD.md` were run before the response.
- `rg` was not available in this PowerShell environment, so `Select-String` was used.
- `Select-String` confirmed `DebugSceneController.cs` contains `EnsureCanvasShell`, `BindSceneUi`, `ConfigureToggleVisuals`, and `Resources.Load<Sprite>("DebugUiSolid")`.
- `Select-String` confirmed `DebugScene.unity` contains `DebugSceneController`, `DebugSetupPanel`, `SkillDebugPanel`, `EnhancementModal`, `Active_A`, `Passive_J`, `Choice_01`, and `Choice_08`.
- `Get-ChildItem -LiteralPath Pakuri\Assets\Resources` confirmed `DebugUiSolid.png` and `DebugUiSolid.png.meta` exist.
- Added `Pakuri/reference/Report/2026-04-30-debugscene-ui-canvas-retrospective.html`.

### History

- 2026-04-30: User requested an HTML summary of the initial DebugScene UI canvas creation method, user correction points, and how the problems were solved.
- 2026-04-30: Designer reviewed BLACKBOARD task history and current DebugScene code/scene evidence, then added the retrospective HTML report.

---

## Source File: `boards/UI/UI_BLACKBOARD.md`

## Task: Combat Visual Sprite Assignment

### Task title

Allow monster/enemy ScriptableObjects and RunScene battlefield background to use editable sprites.

### Goals

- Add editable unit/projectile sprite references to monster and enemy ScriptableObjects under `Assets/Data/GameData`.
- Use assigned monster sprites for the selected monster and its projectiles at runtime.
- Use assigned enemy sprites for enemy bodies and enemy projectiles at runtime.
- Let `RunScene` use an editable battlefield background sprite without forcing the user's manual `BattlefieldBackground` scale.
- Keep unit body `SpriteRenderer.color` values white so assigned unit sprites are not tinted.
- Keep projectile, HP bar, marker, camera background, and battlefield background sprite colors white.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation and local build/Unity console validation completed. User reported Play Mode verification completed. Unit, projectile, HP bar, marker, camera background, and battlefield background sprite color preservation was added. External Code Reviewer run was attempted but interrupted by the user and is not completed.

### Next Actions

- User assigns `UnitSprite` and `ProjectileSprite` on monster/enemy assets as needed.
- User assigns `BattlefieldBackgroundSprite` on `CombatRuntimeController` and adjusts `BattlefieldBackground` Transform Scale manually; keep `Auto Fit Battlefield Background To Field` off when manual scale should be preserved.
- Run Code Reviewer later if the user wants this visual-support change reviewed.

### Evidence

- `MonsterDefinition.cs` now exposes `UnitSprite` and `ProjectileSprite`.
- `EnemyDefinition.cs` now exposes `UnitSprite` and `ProjectileSprite`, and `CloneRuntimeCopy()` preserves both references.
- `CombatRuntimeScene.cs` now reads `MonsterDefinition.UnitSprite` and `MonsterDefinition.ProjectileSprite` into runtime selected sprite fields.
- `CombatRuntimeEnemies.cs` now uses `EnemyDefinition.UnitSprite` for enemy bodies and `EnemyDefinition.ProjectileSprite` for enemy projectiles, falling back to the generated shared sprite when no sprite is assigned.
- `CombatRuntimeProjectiles.cs` now uses the selected monster projectile sprite, falling back to the generated shared sprite when no sprite is assigned.
- `CombatRuntimeController.cs` now exposes `BattlefieldBackgroundAnchor`, `BattlefieldBackgroundSprite`, `BattlefieldBackgroundColor`, and `AutoFitBattlefieldBackgroundToField`.
- `CombatRuntimeScene.cs` now only rewrites `BattlefieldBackground.localScale` when `autoFitBattlefieldBackgroundToField` is true, so manual scale is preserved by default.
- `CombatRuntimeScene.cs` now applies `Color.white` to the selected monster body renderer.
- `CombatRuntimeEnemies.cs` now keeps enemy body renderer colors white in `UpdateEnemyColor()`.
- `CombatRuntimeProjectiles.cs` now applies `Color.white` to selected monster projectiles.
- `CombatRuntimeEnemies.cs` now applies `Color.white` to enemy projectiles and enemy HP bar background/fill sprites.
- `CombatRuntimeController.cs` now initializes marker and battlefield background color fields as `Color.white`.
- `CombatRuntimeScene.cs` now applies `Color.white` to the camera background and battlefield background renderer.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing 2 MCPForUnity/Unity reference warnings.
- Unity script refresh/compile was requested; console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.
- User reported Play Mode verification completed before the manual background scale fix.

### History

- 2026-04-28: User requested editable projectile images and monster images on `Assets/Data/GameData` enemy/monster SOs, plus an editable RunScene background image.
- 2026-04-28: Code Builder added sprite fields to monster/enemy definitions and wired runtime monster/enemy/projectile renderers to use them.
- 2026-04-28: User reported Play Mode verification completed but found `BattlefieldBackground` scale was forced on game start.
- 2026-04-28: Code Builder changed background auto-fit scaling to an opt-in serialized bool so manual `BattlefieldBackground` scale is preserved by default.
- 2026-04-28: User requested unit sprite colors stay white; Code Builder changed selected monster and enemy body renderers to keep `SpriteRenderer.color` white.
- 2026-04-29: User requested projectile, HP bar, marker, and background colors stay white; Code Builder changed those runtime color assignments to `Color.white`.

---

## Source File: `boards/UI/UI_BLACKBOARD.md`

## Task: RunScene Reward Button Visibility Fix

### Task title

RunScene stage-clear reward buttons are fixed editable slots and visible when rewards exist

### Goals

- Fix the RunScene issue where stage-clear reward buttons did not appear.
- Keep reward UI objects editable in Edit Mode instead of relying on delete/recreate behavior.
- Preserve authored button labels where possible, while runtime reward labels are still assigned from actual reward data.

### Constraints

- No external reviewer for this task; perform simple self-review only.
- Do not run Unity Play Mode; user performs gameplay verification.
- All claims must be based on actual files, scene state, or command output.

### Role Owner

Code Builder

### Status

Builder fix applied and self-reviewed. Waiting for user Play Mode verification.

### Next Actions

- User verifies RunScene stage clear: reward panel appears with reward buttons, selecting a reward enables the continue flow.
- If reward panel appears but a button is blocked or misplaced, inspect the saved RectTransform values of `RewardPanel`, `RewardButtons`, and `RewardButton_0..2`.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now uses fixed `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` slots under `RewardButtons`.
- `RebuildRewardButtons()` clears only the tracked button list, calls `EnsureRewardButtonSlots(false)`, then activates slots based on `combatController.GetRewardChoiceCount()`.
- `EnsureRewardButtonSlots()` repairs zero-height `RewardButtons`, ensures the three named button slots, and hides non-slot legacy buttons such as `RewardPreviewButton`.
- Existing nonzero reward button slot RectTransforms keep their authored positions/sizes; default positions are applied only when a slot is newly created or has a broken zero size.
- `EnsureButton()` now preserves existing non-empty labels unless an overwrite is explicitly requested or a label is newly created/empty.
- Unity MCP RunScene inspection after `OnEnable` reported `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` active in Edit Mode, and `RewardPreviewButton` inactive.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity console error check after clearing showed MCP-FOR-UNITY client handler exit logs only, not project script compile errors.

### History

- 2026-04-26: User reported RunScene reward buttons do not appear.
- 2026-04-26: Scene inspection found `RewardButtons` previously had zero height and fixed reward slots were missing, while monster assets contained reward choice data.
- 2026-04-26: Added persistent reward slots, repaired reward root sizing, hid legacy preview buttons, and made existing RunScene reward UI visible in Edit Mode.

---

## Source File: `boards/UI/UI_BLACKBOARD.md`

## Task: MainMenu Persistent Editable Panels

### Task title

MainMenuScene stage-transition UI panels are persistent scene objects

### Goals

- MainMenuScene UI transitions must not create/delete runtime screen UI.
- Touch To Start, Run menu, and Character Select must all exist in the scene so the user can edit them together in Edit Mode.
- Future UI direction: authored scene UI is the source of truth; scripts bind callbacks, toggle visibility, and only create missing named anchors.

### Constraints

- No external reviewer for this task; perform simple self-review only.
- Do not run Unity Play Mode; user performs gameplay verification.
- All claims must be based on actual files, scene state, or command output.

### Role Owner

Code Builder

### Status

Builder changes applied and self-reviewed. Waiting for user Play Mode verification.

### Next Actions

- User verifies MainMenuScene flow: Touch To Start -> Run -> Character Select -> RunScene.
- If user edits any of `TouchToStartPanel`, `RunMenuPanel`, `CharacterSelectPanel`, or their child labels/buttons, verify those edits persist after entering Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs` now has separate persistent fields for `touchToStartPanel`, `runMenuPanel`, `characterSelectPanel`, and `monsterButtonRoot`.
- `MainMenuFlowController.OnEnable()` calls `ShowAllPanelsForEditing()` only when `Application.isPlaying` is false, so all panels are visible in Edit Mode.
- Runtime methods `ShowTouchToStart()`, `ShowRunMenu()`, and `ShowCharacterSelect()` call `SetPanelVisibility(...)` and no longer call `Destroy`, `DestroyImmediate`, or `ClearButtons`.
- `EnsureText()` and `EnsureButton()` set default text/style only when a component is newly created, preserving existing authored UI text and styling.
- Unity MCP scene check reported `MainMenuCanvas` child count 3 after cleanup.
- Unity MCP code execution reported `TouchToStartPanel active=True children=3`, `RunMenuPanel active=True children=3`, and `CharacterSelectPanel active=True children=4`.
- Unity MCP code execution reported five persistent character buttons under `MonsterButtons`: `MonsterButton_ariel`, `MonsterButton_eve`, `MonsterButton_rin`, `MonsterButton_sein`, and `MonsterButton_vega`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity console error check showed only MCP-FOR-UNITY client handler exit log entries, not project script compile errors.
- `Pakuri/Packages/manifest.json` search found `com.unity.ugui` and no `com.unity.textmeshpro` line.
- Asset search under `Pakuri/Assets` found no `TextMeshPro`, `TMP_Text`, `TMPro`, or `LiberationSans` usage/assets.
- Generated `Pakuri/Assembly-CSharp.csproj` contains `Unity.TextMeshPro` references, but current project UI scripts and scene-generated UI are still based on `UnityEngine.UI.Text`.

### History

- 2026-04-26: User requested MainMenuScene click-transition screens to be editable at once instead of created/deleted at runtime, and asked why UI text is not TextMeshPro text.
- 2026-04-26: Replaced the single dynamic `MainMenuPanel` flow with persistent `TouchToStartPanel`, `RunMenuPanel`, and `CharacterSelectPanel` scene objects.
- 2026-04-26: Removed the obsolete generated `MainMenuPanel` from `MainMenuScene` and saved the scene.
- 2026-04-26: Verified build, scene hierarchy, persistent character buttons, and console state.
- 2026-04-26: User reported UI Pos X / Pos Y could not be edited. Actual code and scene checks found `VerticalLayoutGroup` and `ContentSizeFitter` on generated UI containers.
- 2026-04-26: Updated `MainMenuFlowController` and `RunCombatUiController` so generated UI containers remove `VerticalLayoutGroup` / `ContentSizeFitter` instead of adding them.
- 2026-04-26: Verified MainMenuScene `TouchToStartPanel`, `RunMenuPanel`, `CharacterSelectPanel`, and `MonsterButtons` report `VLG=False, CSF=False`; also removed and saved those components from RunScene reward/defeat UI containers.

---

## Source File: `boards/UI/UI_BLACKBOARD.md`

## Task: Preserve Authored UI Layouts

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `MainMenuFlowController`, `RunCombatUiController`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Next Actions

- Legacy non-English note retained these code references: `MainMenuScene`, `RunScene`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`, `MainMenuPanel`, `BuildUiScaffold()`, `CacheUiReferences()`, `Title`, `Summary`, `Buttons`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`, `HudPanel`, `RewardPanel`, `DefeatPanel`, `BuildUiScaffold()`, `CacheUiReferences()`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `BuildUiScaffold()`, `EnsurePanel()`, `EnsureText()`, `EnsureButton()`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

---

## Source File: `boards/UI/UI_BLACKBOARD.md`

## Task: RunScene Combat UI Restoration And Edit Mode Visibility

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `RunScene`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `MainMenuScene`, `RunScene`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Next Actions

- Legacy non-English note retained these code references: `MainMenuScene -> RunScene`.
- Legacy non-English note retained these code references: `MainMenuCanvas`, `RunCombatCanvas`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`, `[ExecuteAlways]`, `Touch To Start`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`, `RunScene`.
- Legacy non-English note retained these code references: `RunCombatUiController`.
- Legacy non-English note retained these code references: `RunCombatUiController`, `EveVerticalSliceController`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs`, `ActiveMonster`, `ActiveSession`, `FallbackMonsterId`.
- Legacy non-English note retained these code references: `RunScene`, `RunCombatCanvas`, `RunCombatUiController`, `CombatRoot`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `MainMenuScene`, `MainMenuCanvas`.
- Legacy non-English note retained these code references: `RunScene`, `RunCombatCanvas`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note retained these code references: `RunScene`.
- Legacy non-English note retained these code references: `RunScene`, `RunFlowController`.
- Legacy non-English note retained these code references: `RunCombatUiController`, `RunScene`, `RunCombatCanvas`.
- Legacy non-English note retained these code references: `MainMenuFlowController`, `RunCombatUiController`, `[ExecuteAlways]`.

---

## Source File: `boards/UI/UI_BLACKBOARD.md`

## Task: Main Menu To RunScene Flow Separation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `RunScene`, `MainMenuScene`.
- Legacy non-English note retained these ASCII code references: `MainMenuScene`.
- Legacy non-English note retained these code references: `RunScene`, `RunSession`.
- Legacy non-English note retained these code references: `DontDestroyOnLoad`, `RunStartContext`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Next Actions

- Legacy non-English note retained these ASCII code references: `MainMenuScene`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunStartContext.cs`, `RunSession`, `DontDestroyOnLoad`.
- Legacy non-English note retained these ASCII code references: `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`, `Touch To Start`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs`, `RunScene`, `RunStartContext`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `using System;`, `Serializable`, `StringComparison`, `Math`.
- Legacy non-English note retained these code references: `RunScene`, `RunUICanvas`, `RunSceneBootstrap`.
- Legacy non-English note retained these code references: `MainMenuScene`, `MainMenuCanvas`, `MainMenuFlowController`, `EventSystem`.
- Legacy non-English note retained these code references: `Pakuri/ProjectSettings/EditorBuildSettings.asset`, `Assets/Scenes/MainMenuScene.unity`, `Assets/Scenes/RunScene.unity`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `SampleScene.unity`, `MainMenuScene.unity`, `RunScene.unity`.
- Legacy non-English note retained these code references: `RunScene.unity`, `RunUICanvas`, `RunFlowController`.
- Legacy non-English note retained these code references: `RunStartContext`, `MainMenuFlowController`, `RunSceneBootstrap`, `RunScene`, `MainMenuScene`.
- Legacy non-English note retained these code references: `dotnet build`.

---

## Source File: `boards/UI/UI_BLACKBOARD.md`

## Task: Run Flow UICanvas Prototype Implementation

### Task title

Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.

### Goals

- Legacy non-English note retained these code references: `RunSession`, `RunFlowController`, `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

### Next Actions

- Legacy non-English note retained these code references: `RunUICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `B/G, C/H, D/I, E/J`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Seed Default Game Data`, `Assets/Data/GameData/GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `RunUICanvas`, `EventSystem`.
- Legacy non-English note retained these code references: `Assets/Data/GameData/GameDataCatalog.asset`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`, `EventSystem`, `InputSystemUIInputModule`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `stageIndex`.
- Legacy non-English note retained these code references: `RunFlowController.ClearButtons(...)`, `QueuedForDestroy`.
- Legacy non-English note retained these code references: `RunFlowController.ResolveReferences()`, `Arial.ttf`, `ArgumentException`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`, `Arial.ttf`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `EventSystem`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Resources.GetBuiltinResource<Font>("Arial.ttf")`, `RunFlowController`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

---
