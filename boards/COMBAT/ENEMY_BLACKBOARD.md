# ENEMY_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

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

