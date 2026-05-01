# COMBAT_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

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
- 2026-04-30: User requested HP Bar background black and reported Ariel A master `백색 심판` was not visibly applying. Code Builder changed shared HP bar backgrounds to `Color.black`, changed pending Ariel judgement explosions to trigger immediately on enemy hit, and made the explosion use a longer, higher-sorting circle-sprite visual.

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

## Task: Combat Monster Enemy Implementation

### Task title

?꾪닾 湲곕낯 洹쒖튃 湲곕컲 Stage 1 ??/ Monster ?곗씠??/ ?쇳빐 怨꾩궛 濡쒓렇 援ы쁽

### Goals

- `combat-monster-enemy-implementation-plan.html`??諛⑺뼢?濡?怨듯넻 ?꾪닾 紐⑤뜽, ?띿꽦蹂?諛⑹뼱??怨꾩궛, Stage 1 ???곗씠?곗? ?고????④낵瑜?援ы쁽?쒕떎.
- Monster 5紐낆쓽 ?≫떚釉?A~E, ?⑥떆釉?F~J ?곗씠???щ’??留뚮뱺??
- Monster媛 ?곸뿉寃??쇳빐瑜??낇옄 ??Unity Console `Debug.Log`濡?怨꾩궛?앷낵 ?곸슜 ?쇳빐瑜?媛꾨떒??異쒕젰?쒕떎.

### Constraints

- Role Owner??Code Builder??
- ?ъ슜?먭? ?뚮젅???ㅽ뻾 寃利앹? 吏곸젒 ?섑뻾?쒕떎怨??덉쑝誘濡?Codex??Play Mode瑜??ㅽ뻾?섏? ?딅뒗??
- ?ъ슜?먭? ?먯껜 由щ럭源뚯?留??붿껌?덉쑝誘濡??몃? Reviewer???몄텧?섏? ?딄퀬 Builder ?먯껜 由щ럭? 鍮뚮뱶/肄섏넄 ?뺤씤源뚯?留??섑뻾?덈떎.
- ?먮떒? ?ㅼ젣 肄붾뱶, asset, 紐낅졊 異쒕젰??洹쇨굅?쒕떎.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- ?ъ슜?먭? Unity Play Mode?먯꽌 MainMenuScene ?먮뒗 RunScene ?먮쫫???ㅽ뻾??Stage 1 ???ㅽ룿, ???≫떚釉??⑥떆釉? 紐ъ뒪???쇳빐 怨꾩궛 濡쒓렇瑜??뺤씤?쒕떎.
- Unity Console?먯꽌 `[CombatDamage]` 濡쒓렇媛 怨듦꺽?? ?ㅽ궗, ??? ?띿꽦 諛⑹뼱??怨듭떇, 理쒖쥌 ?곸슜 ?쇳빐瑜?異쒕젰?섎뒗吏 ?뺤씤?쒕떎.

### Evidence

- 異붽???怨듯넻 ?꾪닾 ??? `Pakuri/Assets/Scripts/Combat/CombatStatModels.cs`.
- ?뺤옣???쇳빐 怨꾩궛: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`媛 ?띿꽦蹂?諛⑹뼱?? 怨좎젙/?쇱꽱??諛⑹뼱 蹂댁젙, 移섎챸? ??? 理쒖쥌 諛곗쑉, `FormulaLog`瑜?泥섎━?쒕떎.
- 異붽????곗씠????? `Pakuri/Assets/Scripts/Data/SkillDefinition.cs`, `Pakuri/Assets/Scripts/Data/EnemyDefinition.cs`.
- ?뺤옣??移댄깉濡쒓렇/紐ъ뒪???곗씠?? `GameDataCatalog.cs`??`StageOneEnemies`, `MonsterDefinition.cs`??`PrimaryAttribute`, `BaseStats`, `Defenses`, `ActiveSkills`, `PassiveSkills`瑜?異붽??덈떎.
- ?꾪닾 ?곌껐: `RunFlowController.cs`, `RunSceneBootstrap.cs`媛 `GameDataCatalog`瑜?`EveVerticalSliceController.BeginConfiguredDay(...)`???섍릿??
- ?꾪닾 ?고??? `EveVerticalSliceController.cs`媛 Stage 1 ??????ъ슜?섍퀬, 寃??諛⑺뙣蹂?沅곸닔/?꾩쟻/?ъ젣/?섑샇???怨듦꺽????⑹궗 移대┛???≫떚釉??⑥떆釉??고????④낵瑜?泥섎━?쒕떎.
- 11?쇱감??Stage 1 洹쒖튃?濡??섑샇??? 怨듦꺽??? ?⑹궗 移대┛??紐⑤몢 蹂댁뒪 ?ㅽ룿 ??곸쑝濡?泥섎━?섎룄濡??섏젙?덈떎.
- 紐ъ뒪?곌? ?곸뿉寃??쇳빐瑜?以???`Debug.Log("[CombatDamage] ...")`濡??띿꽦 諛⑹뼱??怨듭떇, 理쒖쥌 ?쇳빐, ?ㅼ젣 ?곸슜 ?쇳빐, ?⑥? 蹂댄샇留?HP瑜?異쒕젰?쒕떎.
- `Pakuri/Seed Default Game Data` 硫붾돱 ?ㅽ뻾 ??`Pakuri/Assets/Data/GameData/Enemies` ?꾨옒 Stage 1 ??8醫?asset???앹꽦?먭퀬, `GameDataCatalog.asset`??`StageOneEnemies` 李몄“媛 湲곕줉?먮떎.
- `Pakuri/Assets/Data/GameData/Monsters/eve.asset` ?뺤씤 寃곌낵 `PrimaryAttribute`, `ActiveSkills`, `PassiveSkills`, `ImplementationState`媛 湲곕줉?먮떎.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`???ㅻ쪟 0媛쒕줈 ?듦낵?덈떎. ?⑥? 寃쎄퀬??湲곗〈 Unity/MCPForUnity `System.Net.Http`, `System.IO.Compression` 踰꾩쟾 異⑸룎 寃쎄퀬 2媛쒕떎.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`???ㅻ쪟 0媛쒕줈 ?듦낵?덈떎. ?⑥? 寃쎄퀬???숈씪??湲곗〈 李몄“ 寃쎄퀬 2媛쒕떎.
- Unity console error 議고쉶??MCP-FOR-UNITY client handler exit 濡쒓렇留?諛섑솚?덇퀬, ???꾨줈?앺듃 而댄뙆???ㅻ쪟???뺤씤?섏? ?딆븯??

### History

- 2026-04-27: ?ъ슜??吏?쒕줈 Designer ?ㅺ퀎 HTML 湲곗? 援ы쁽??李⑹닔?덈떎.
- 2026-04-27: `AGENTS.md`, `BLACKBOARD.md`, Unity MCP skill 吏移⑥쓣 癒쇱? ?뺤씤?덈떎.
- 2026-04-27: 湲곗〈 `EveVerticalSliceController`媛 ??諛⑹뼱?μ쓣 `0f`濡??섍린??援ъ“?꾩쓣 ?뺤씤?섍퀬 ?띿꽦蹂?諛⑹뼱??怨꾩궛??異붽??덈떎.
- 2026-04-27: Stage 1 ???곗씠?곗? Monster 5紐??ㅽ궗/?⑥떆釉??곗씠???먯궛 ?앹꽦???꾪빐 `PakuriGameDataSeeder.cs`瑜??뺤옣?섍퀬 硫붾돱瑜??ㅽ뻾?덈떎.
- 2026-04-27: ?먯껜 由щ럭 以?11?쇱감 ?ㅼ쨷 蹂댁뒪 洹쒖튃 ?꾨씫??諛쒓껄???섑샇??? 怨듦꺽??? ?⑹궗 移대┛??紐⑤몢 ?ㅽ룿?섎룄濡??섏젙?덈떎.
- 2026-04-27: ?고????먮뵒??鍮뚮뱶? Unity 肄섏넄 error ?뺤씤源뚯? ?꾨즺?덈떎.

## Task: Combat Monster Enemy Implementation Plan

### Task title

?꾪닾 湲곕낯 洹쒖튃, Monster ?ㅽ궗, Stage 1 ??援ы쁽 諛⑹떇 HTML ?ㅺ퀎

### Goals

- `Pakuri/reference/3.combat` ?꾪닾 湲곕낯 湲고쉷?쒖? `Pakuri/reference/5.enemy` ??湲고쉷?쒕? ?ㅼ젣 ?뚯씪 湲곗??쇰줈 ?쎄퀬 援ы쁽 諛⑺뼢???뺣━?쒕떎.
- ?꾩슂??寃쎌슦 `Pakuri/data` CSV????븷???뺤씤?섎릺, ?ㅼ젣 臾몄꽌? 異⑸룎?섎뒗 媛믪? 洹몃?濡??ъ슜?섏? ?딅뒗??
- Monster???띿꽦蹂?諛⑹뼱?? ?≫떚釉??ㅽ궗, 湲곕낯 ?λ젰移? ?⑥떆釉뚯? Stage 1 ??援ы쁽 諛⑹떇??HTML 臾몄꽌濡??뺣━?쒕떎.

### Constraints

- Role Owner??Designer?대ŉ ?ㅼ젣 C# 援ы쁽? ?섏? ?딅뒗??
- 紐⑤뱺 ?먮떒? ?ㅼ젣 臾몄꽌, CSV, ?꾩옱 C# 肄붾뱶 ?댁슜??洹쇨굅?쒕떎.
- ?꾩옱 ?꾨줈?앺듃?먮뒗 CSV ?고???濡쒕뜑媛 ?뺤씤?섏? ?딆븯?쇰?濡?CSV 吏곸젒 濡쒕뵫??援ы쁽??寃껋쿂???곗? ?딅뒗??

### Role Owner

Designer

### Status

Completed.

### Next Actions

- ?ъ슜?먭? 援ы쁽???먰븯硫???HTML??湲곗??쇰줈 Code Builder?먭쾶 handoff?쒕떎.
- Builder ?④퀎?먯꽌??怨듯넻 ?꾪닾 ?곗씠??紐⑤뜽, ?띿꽦蹂?諛⑹뼱??怨꾩궛, Stage 1 ???먯궛, ?ㅽ궗 ?ㅽ뻾湲??쒖꽌濡??ㅼ뼱媛꾨떎.

### Evidence

- ?쎌? ?꾪닾 臾몄꽌: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `buff-debuff.md`, `realtime-damage-meter.md`.
- ?쎌? ??臾몄꽌: `Pakuri/reference/5.enemy/stage-basic-rules.md`, `enemy-stage-index.md`, `stage-1-enemies.md`.
- ?쎌? Monster 臾몄꽌: `Pakuri/reference/2.Monster/monster-basic-rule.md`, `monster-skill-patterns.md`, `skill-choice-pool-rule.md`, 媛?Monster tower 臾몄꽌? ?ㅽ궗 臾몄꽌 紐⑸줉.
- ?뺤씤??CSV: `Pakuri/data/enemies.csv`, `enemy_runtime.csv`, `skills.csv`, `skill_runtime.csv`, `ally_units.csv`, `ally_runtime.csv`, `status_effects.csv`, `levelup_choices.csv`, `skill_branches.csv`, `levelup_rules.csv`.
- ?뺤씤???꾩옱 肄붾뱶: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `EveVerticalSliceController.cs`, `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`, `Pakuri/Assets/Scripts/Run/RunSession.cs`.
- ?앹꽦??臾몄꽌: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

### History

- 2026-04-27: AGENTS.md? BLACKBOARD.md瑜?癒쇱? ?쎌뿀??
- 2026-04-27: `rg`媛 ?ㅼ튂?섏뼱 ?덉? ?딆븘 PowerShell `Get-ChildItem`怨?`Get-Content`濡??ㅼ젣 ?뚯씪 紐⑸줉怨??댁슜???뺤씤?덈떎.
- 2026-04-27: `Pakuri/reference/run-systems-integration-summary-report.html`??BLACKBOARD 湲곕줉怨??щ━ ?대떦 寃쎈줈???녾퀬, ?ㅼ젣 ?뚯씪? `Pakuri/reference/Report/run-systems-integration-summary-report.html`???덉쓬???뺤씤?덈떎.
- 2026-04-27: Stage 1 ??臾몄꽌? CSV???꾩옱 ???곗씠?곌? 吏곸젒 ?쇱튂?섏? ?딆쑝誘濡?Stage 1 ?섏튂??臾몄꽌 ?곗꽑, CSV???ㅽ궎留?李멸퀬濡??뺣━?덈떎.
- 2026-04-27: Designer ?ㅺ퀎 HTML `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`瑜?異붽??덈떎.

