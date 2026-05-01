# PROJECTILE_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

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
- 2026-04-30: User reported `백색 심판` was not applying. Code Builder changed `TryTriggerArielJudgementLightExplosion()` to return a fired flag and made `UpdateProjectiles()` trigger/cleanup the pending Ariel explosion immediately when the marked projectile hits an enemy, while keeping lifetime-expiry fallback.

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

臾몄꽌 以?섑삎 ?꾪겕 蹂쇳듃 ?ъ궗泥??낅젰/?곸쨷 援ъ“ ?섏젙 怨꾪쉷 HTML ?묒꽦

### Goals

- ?꾩옱 ?대툕 ?꾪닾 ?꾨줈?좏??낆쓣 湲곗??쇰줈, ?꾪겕 蹂쇳듃瑜?臾몄꽌 ?뺤쓽????留욌뒗 `?ъ궗泥?/ ?꾩갹?? 援ъ“濡?諛붽씀???묒뾽 怨꾪쉷???뺣━?쒕떎.
- ?ъ슜?먭? ?붿껌??`?쇱そ ?대┃ ?좎? ???곗냽 諛쒖궗`, `?ъ궗泥??곸쨷 ???쇳빐` ?붽뎄瑜??ㅼ젣 肄붾뱶? reference 臾몄꽌 李⑥씠 湲곗??쇰줈 ?ㅻ챸?쒕떎.
- Code Builder媛 諛붾줈 援ы쁽???ㅼ뼱媛????덈룄濡??섏젙 踰붿쐞, ?뚯씪蹂?蹂寃?怨꾪쉷, 寃利?泥댄겕由ъ뒪?몃? HTML ???μ쑝濡??④릿??

### Constraints

- ?ㅼ젣 reference 臾몄꽌? ?ㅼ젣 ?꾩옱 肄붾뱶??洹쇨굅?댁꽌留??곷뒗??
- ?꾩쭅 ?녿뒗 援ы쁽??援ы쁽??寃껋쿂???곸? ?딅뒗??
- ???묒뾽? ?ㅺ퀎 臾몄꽌 ?묒꽦?대ŉ, 肄붾뱶 ?섏젙 ?먯껜???ы븿?섏? ?딅뒗??

### Role Owner

Designer

### Status

Completed

### Next Actions

- ?ъ슜?먭? ?먰븯硫???臾몄꽌瑜?湲곗??쇰줈 Code Builder ?④퀎濡??꾪솚???ㅼ젣 ?ъ궗泥댄삎 諛쒖궗 濡쒖쭅??援ы쁽?쒕떎.
- 援ы쁽 ??`EveVerticalSliceController.cs`??利됱떆 ?쇳빐 援ъ“瑜??ъ궗泥??곸쨷 援ъ“濡?諛붽씀怨? hold ?낅젰 寃利앷낵 reviewer 猷⑦봽瑜??ㅼ떆 ?섑뻾?쒕떎.

### Evidence

- `Pakuri/reference/dungeon-squad-combat-player-controls.md`???꾪닾 以??뚮젅?댁뼱 ?낅젰??`怨듦꺽 吏??吏???쇰줈 ?뺤쓽?쒕떎.
- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`???꾪겕 蹂쇳듃瑜?`?ъ궗泥?/ ?꾩갹???쇰줈 ?뺤쓽?섍퀬, ?ъ궗泥??띾룄 `15.0`, ?꾩갹 `6`, ?ъ옣??`4珥?, 諛쒖궗 媛꾧꺽 `0.35珥?, 媛먯쟾 `15%`瑜?紐낆떆?쒕떎.
- `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`??媛숈? ?띿꽦 諛⑹뼱??李몄“? 諛⑹뼱??諛섏쁺 ??移섎챸? ?곸슜 洹쒖튃???뺤쓽?쒕떎.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` ?꾩옱 援ы쁽? `wasPressedThisFrame` / `GetMouseButtonDown(0)` ?낅젰怨?利됱떆 ?쇳빐 援ъ“瑜??ъ슜?쒕떎.
- ???ㅺ퀎 臾몄꽌 `Pakuri/reference/eve-projectile-click-hold-plan.html`瑜?異붽??덈떎.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-combat-player-controls.md`, `a-arc-bolt.md`, `combat-attribute-and-damage-system.md`, `EveVerticalSliceController.cs`, `eve-combat-implementation-report.html`瑜??ㅼ떆 ?쎌뿀??
- 2026-04-24: ?꾩옱 肄붾뱶媛 ?⑤컻 ?대┃ ?낅젰怨?利됱떆 ?쇳빐 援ъ“?꾩쓣 ?뺤씤?덈떎.
- 2026-04-24: hold ?낅젰 湲곕컲 ?곗냽 諛쒖궗? ?ъ궗泥??곸쨷 湲곕컲 ?쇳빐 泥섎━濡???린???ㅺ퀎 HTML??`Pakuri/reference/eve-projectile-click-hold-plan.html`??異붽??덈떎.

## Task: Eve Projectile Click Implementation

### Task title

?대툕 ?꾪겕 蹂쇳듃瑜??대┃???ъ궗泥??곸쨷 援ъ“濡??섏젙?섍퀬 ?꾨즺 蹂닿퀬 HTML ?묒꽦

### Goals

- 湲곗〈 利됱떆 ?쇳빐 援ъ“瑜??쒓굅?섍퀬, ?쇱そ ?대┃ ?쒖뿉留??꾪겕 蹂쇳듃 ?ъ궗泥?1諛쒖씠 ?앹꽦?섍쾶 ?쒕떎.
- ?ъ궗泥닿? ?ㅼ젣濡??대룞?섍퀬 ?곴낵 ?우쓣 ?뚮쭔 ?쇳빐瑜??곸슜?섍쾶 ?쒕떎.
- ?섏젙 ??媛앹껜 ??븷, ?숈옉 諛⑹떇, ?묒뾽 以?臾몄젣, ??꾩뒪?ы봽 ?묒뾽 濡쒓렇瑜??ы븿???꾨즺 蹂닿퀬 HTML???④릿??

### Constraints

- ?ㅼ젣 ?꾩옱 肄붾뱶? ?ㅼ젣 Unity ?고???寃利앹쓣 洹쇨굅濡??묒뾽?쒕떎.
- ???ㅽ룿 異? 移대찓?? ?꾩옣 醫뚰몴??湲곗〈 媛믪쓣 ?좎??쒕떎.
- 濡쒖쭅 ?섏젙 ??reviewer 媛뺤젣 ?먮쫫???ㅼ떆 ?쒕룄?섍퀬, ?ㅽ뙣 ??洹?洹쇨굅瑜??④릿??

### Role Owner

Code Builder

### Status

Completed without Code Review. External reviewer commands timed out again, so only Builder-side validation was performed.

### Next Actions

- ?ъ슜?먭? ?먰븯硫??ㅼ쓬 ?④퀎濡??ㅼ젣 ?대┃ ?낅젰 湲곕컲 ?뺤떇 ?뚮젅???뚯뒪?? ?띿꽦蹂?諛⑹뼱???곗씠??紐⑤뜽, Collider 湲곕컲 異⑸룎濡??뺤옣?쒕떎.
- reviewer ?몃? 媛뺤젣 ?먮쫫 timeout ?먯씤??蹂꾨룄 遺꾨━?댁꽌 ?닿껐?댁빞 ?쒕떎.
- ?꾩옱 ?곹깭??Code Review 誘몄닔???곹깭?대?濡? ?댄썑 由щ럭媛 ?꾩슂?섎㈃ 蹂꾨룄 reviewer ?④퀎瑜??ㅼ떆 ?ㅽ뻾?댁빞 ?쒕떎.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`??`ProjectileRuntime`, `projectileRoot`, `UpdateProjectiles()`, `TryHitEnemy()`, ?대┃ 湲곕컲 `HandlePointerInput()`瑜??ы븿?섎룄濡??섏젙?먮떎.
- `Pakuri/Assets/Scenes/SampleScene.unity`??`ProjectileRoot`瑜??ы븿???꾩옱 ?꾩옣 援ъ“濡??ㅼ떆 ??λ릱??
- `manage_scene save`媛 `Assets/Scenes/SampleScene.unity` ????깃났??諛섑솚?덈떎.
- `find_gameobjects by_name ProjectileRoot`???ъ뿉??`ProjectileRoot`瑜?李얠븯??
- ?뚮젅??紐⑤뱶 ?듭젣 寃利앹뿉??
  - 諛쒖궗 吏곹썑 `projectileCount = 1`
  - 1珥???`projectileCount = 0`
  - 媛숈? 寃利앹뿉??`enemyHealth = 37.95`
  - 理쒖쥌 ?ш?利앹뿉??`currentShotsRemaining = 0`, `reloadRemaining = 4.0`
- 寃利?罹≪쿂 `Pakuri/Assets/Screenshots/eve-projectile-click-runtime.png`瑜??앹꽦?덈떎.
- `validate_script`???대쾲?먮룄 duplicate signature false positive瑜??덈떎.
- `read_console`?먯꽌??`FindObjectOfType<Camera>()` obsolete warning???섏솕怨??댄썑 `FindFirstObjectByType<Camera>()`濡??섏젙?덈떎.
- ?몃? reviewer ?쒕룄:
  - `codex review --uncommitted` timeout
  - reviewer ?꾩슜 `codex exec` timeout

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `eve-projectile-click-hold-plan.html`, `a-arc-bolt.md`, `dungeon-squad-combat-player-controls.md`, ?꾩옱 `EveVerticalSliceController.cs`瑜??ㅼ떆 ?쎌뿀??
- 2026-04-24: 利됱떆 ?쇳빐 援ъ“瑜??쒓굅?섍퀬 ?대┃???ъ궗泥??앹꽦/?대룞/?곸쨷 援ъ“濡?`EveVerticalSliceController.cs`瑜?援먯껜?덈떎.
- 2026-04-24: `ProjectileRoot` ?앹꽦怨?hierarchy 諛섏쁺???뺤씤?덈떎.
- 2026-04-24: ?뚮젅??紐⑤뱶 ?듭젣 寃利앹쑝濡??ъ궗泥??곸쨷 ???쇳빐 ?곸슜???뺤씤?덈떎.
- 2026-04-24: ?섎룞 line review?먯꽌 留덉?留????댄썑 ?먮룞 ?ъ옣??吏??臾몄젣瑜?李얠븘 `FireArcBolt()`?먯꽌 利됱떆 ?ъ옣???쒖옉?쇰줈 ?섏젙?덈떎.
- 2026-04-24: obsolete camera ?먯깋 寃쎄퀬瑜?`FindFirstObjectByType<Camera>()`濡??섏젙?덈떎.
- 2026-04-24: ?묒뾽 ?꾨즺 蹂닿퀬??`Pakuri/reference/eve-projectile-click-implementation-report.html`瑜?異붽??덈떎.
- 2026-04-24: ?몃? reviewer濡?`codex review --uncommitted`, reviewer ?꾩슜 `codex exec`瑜??ㅼ떆 ?쒕룄?덉쑝??紐⑤몢 timeout ?먮떎.

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
- Runtime Unity `execute_code` normalized 10 current skill toggle visuals and confirmed `Active_A` has `toggle.graphic=Text:Checkmark/Glyph`, `labelText=A: ?꾪겕 蹂쇳듃`, `labelAlpha=1`, and `glyphText=??.
- Runtime Unity missing-script inspection returned `missingTotal=0`; the visible console still contained older `The referenced script (Unknown) on this Behaviour is missing!` entries with no file/line.
- User reported the Label skill text and checkbox were still not visible. Builder replaced the Text-glyph checkmark approach with Unity built-in `UISprite` and `Checkmark` sprites in `DebugSceneController.ConfigureToggleVisuals(...)`.
- Unity Edit Mode scene save normalized the actual `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content` slots `Active_A` through `Passive_J` and saved `DebugScene.unity`; `Active_A` inspection returned `label=A: ?꾪겕 蹂쇳듃`, `labelAlpha=1`, `bgSprite=UISprite`, `checkSprite=Checkmark`, and `toggleGraphic=Checkmark`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed with editor state `ready_for_tools=true`; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the previous `DebugSceneController requires DebugSetupPanel...` project error.
- User reported `Failed to find UI/Skin/UISprite.psd` from `DebugSceneController.ConfigureToggleVisuals(...)`.
- `Select-String` confirmed the old `UI/Skin` and `GetBuiltinResource<Sprite>` calls were removed from `Pakuri/Assets/Scripts/Run/DebugSceneController.cs`; the only sprite load is now `Resources.Load<Sprite>("DebugUiSolid")`.
- `Pakuri/Assets/Resources/DebugUiSolid.png` was created as a project-owned 1x1 Sprite resource, avoiding Unity built-in UI skin paths.
- Unity Edit Mode scene save updated the actual `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content` slots so `Active_A` through `Passive_J` remain editable scene objects and their `Background` / `Background/Checkmark` images use `DebugUiSolid`.
- Unity read-only `execute_code` confirmed `resourceSprite=DebugUiSolid`, `contentCount=10`, `label=A: ?꾪겕 蹂쇳듃`, `labelAlpha=1`, `bgSprite=DebugUiSolid`, and `checkSprite=DebugUiSolid`.
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

