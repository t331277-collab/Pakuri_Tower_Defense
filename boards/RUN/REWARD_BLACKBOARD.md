# REWARD_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

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
- `Pakuri/reference/4.run/prisoner-choice-system.md` defines 怨듭뼇 as spending a prisoner on an existing monster to show up to 3 skill or enhancement choices and choose 1.
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
- 2026-04-28: User requested prisoner use through 怨듭뼇 and a skill choice pool triggered by prisoner reward buttons; Code Builder implemented the `PrisonerPanel` choice flow using the current monster skill and reward-choice data.

- 2026-04-28: User reported Play Mode verification completed for the prisoner offering choice flow.

- 2026-04-28: User reported no notable Play Mode issues and chose to defer the Code Reviewer run until later.

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

## Task: Monster Select Run UI Expansion Plan

### Task title

紐ъ뒪???좏깮 UI, Run ?쒖옉, ?꾪닾 ???ㅽ궗 媛뺥솕 ?먮쫫 ?뺤옣 ?ㅺ퀎 HTML ?묒꽦

### Goals

- ?꾩옱 援ы쁽???대툕 ?⑤룆 ?꾪닾 ?꾨줈?좏??낆쓣 湲곗??쇰줈, 紐ъ뒪???좏깮 UI? Run ?쒖옉 ?먮쫫???대뼸寃??쇰컲?뷀븷吏 ?뺣━?쒕떎.
- `2.Monster` 臾몄꽌援곌낵 `skill-choice-pool-rule.md`, `combat-reward-system.md`瑜?洹쇨굅濡?紐ъ뒪?곕퀎 ?쒖옉 ?ㅽ궗 A, 理쒕? ?≫떚釉?3媛? 理쒕? ?⑥떆釉?3媛? ?꾪닾 ??媛뺥솕 ?좏깮 ?먮쫫???ㅺ퀎?쒕떎.
- 援ы쁽 ?꾩뿉 ?꾩슂??怨듯넻 ?쒖뒪?? UI ?⑤꼸 援ъ“, ?대┛ 吏덈Ц??HTML 臾몄꽌濡??④릿??

### Constraints

- ?ㅼ젣 ?꾩옱 肄붾뱶, ?ㅼ젣 ???곹깭, ?ㅼ젣 reference 臾몄꽌??洹쇨굅?댁꽌留??곷뒗??
- 援ы쁽?섏? ?딆? UI/???쒖뒪?쒖쓣 ?대? ?덈뒗 寃껋쿂???곸? ?딅뒗??
- ???묒뾽? Designer ?ㅺ퀎 臾몄꽌 ?묒꽦?대ŉ, ?ㅼ젣 肄붾뱶 援ы쁽? ?ы븿?섏? ?딅뒗??

### Role Owner

Designer

### Status

Completed

### Next Actions

- ?ъ슜?먭? ?먰븯硫????ㅺ퀎 臾몄꽌瑜?湲곗??쇰줈 Designer handoff瑜??묒꽦??Code Builder 援ы쁽 踰붿쐞瑜?怨좎젙?쒕떎.
- ?ъ슜?먭? 紐낆떆?곸쑝濡?援ы쁽??吏?쒗븯硫? 癒쇱? UI 堉덈?? RunSession 遺꾨━遺???ㅼ뼱媛??寃껋씠 ?덉쟾?섎떎.
- 1李?援ы쁽 踰붿쐞??臾몄꽌媛 ?꾨퉬??`?꾨━??, `?대툕`, `?몄씤`, `踰좉?` 4紐ъ뒪???곗꽑?쇰줈 ?↔퀬, `由?? ?붾? ?곹깭濡??붾떎.
- 由곗쓽 `g~j` ?⑥떆釉?臾몄꽌媛 ?ㅼ젣 ??μ냼???놁쑝誘濡? 由곗쓣 ?뚮젅??媛????곸쑝濡??щ━???묒뾽? ?꾩냽 臾몄꽌 蹂닿컯 ?댄썑濡?誘몃，??

### Evidence

- `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`留??꾩옱 寃뚯엫 ?꾩슜 ?ㅽ겕由쏀듃濡?議댁옱?쒕떎.
- ?꾩옱 ?쒖꽦 ?ъ? `Assets/Scenes/SampleScene.unity`?대ŉ 猷⑦듃 ?ㅻ툕?앺듃??`Main Camera`, `Global Light 2D`, `CombatRoot`??
- `CombatRoot` ?섏쐞?먮뒗 `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`, `ProjectileRoot`媛 ?덈떎.
- `Pakuri/Assets` ?꾨옒?먯꽌??`NO_UI_TOOLKIT_ASSETS`, `NO_UI_NAMED_ASSETS`媛 ?뺤씤??蹂꾨룄 UI ?먯궛???놁쓬???ы솗?명뻽??
- `Pakuri/reference/2.Monster/monster-basic-rule.md`??紐ъ뒪?곌? ?≫떚釉?A瑜?湲곕낯 ?듬뱷 ?곹깭濡??쒖옉?섍퀬, ??以??≫떚釉?理쒕? 3媛? ?⑥떆釉?理쒕? 3媛쒕? 媛吏꾨떎怨??뺤쓽?쒕떎.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`???좉퇋 ?≫떚釉? ?좉퇋 ?⑥떆釉? ?≫떚釉??뱀꽦, 留덉뒪???ㅽ궗???섎굹???좏깮吏 ?濡??⑹퀜 3媛쒕? ?쒖떆?섎뒗 洹쒖튃???뺤쓽?쒕떎.
- `Pakuri/reference/4.run/combat-reward-system.md`???쇰컲 ?꾪닾/以묎컙蹂댁뒪/蹂댁뒪 ?꾪닾蹂??щ줈, ?좊Ъ, 怨⑤뱶, ?대몺???붿쟻 蹂댁긽 洹쒖튃???뺤쓽?쒕떎.
- `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `eve/eve-tower.md`, `rin/rin-tower.md`, `sein/sein-tower.md`, `vega/vega-tower.md`濡??꾩옱 援ы쁽 ???紐ъ뒪??5醫낆쓣 ?뺤씤?덈떎.
- ?ъ슜???묐떟?쇰줈 紐⑤뱺 紐ъ뒪?곕뒗 ?⑥떆釉??щ’ `F~J` 珥?5媛쒕? 媛吏硫? ??以??ㅼ젣濡??좏깮 媛?ν븳 ?⑥떆釉뚮뒗 理쒕? 3媛쒕씪???ㅺ퀎 湲곗????뺤젙?덈떎.
- ?ъ슜???묐떟?쇰줈 ?대쾲 踰붿쐞???щ줈 蹂댁긽? `?쒖떆留??섎뒗 ?뺣낫`濡?泥섎━?섍퀬, ?곸엯 ?쒖뒪?쒖? ?섏쨷??遺숈씠湲곕줈 ?뺤젙?덈떎.
- ?ъ슜???묐떟?쇰줈 1李?援ы쁽? 臾몄꽌媛 ?꾨퉬??4紐ъ뒪??`?꾨━??, `?대툕`, `?몄씤`, `踰좉?`)遺??吏꾪뻾?섍퀬, `由?? ?붾? ?곹깭濡??먭린濡??뺤젙?덈떎.
- ?ㅼ젣 ??μ냼 ?뺤씤 寃곌낵 ?꾨━?? ?대툕, ?몄씤, 踰좉???`f~j` ?⑥떆釉?臾몄꽌媛 紐⑤몢 議댁옱?섏?留? 由곗? `f-ambidextrous.md`留??덇퀬 `g~j` ?⑥떆釉?臾몄꽌???꾩쭅 ?녿떎.
- ???ㅺ퀎 臾몄꽌 `Pakuri/reference/monster-select-run-ui-expansion-plan.html`瑜?異붽??덈떎.

### History

- 2026-04-25: `AGENTS.md`, `BLACKBOARD.md`瑜??ㅼ떆 ?쎄퀬 ?꾩옱 ?묒뾽 洹쒖튃怨?湲곗〈 ?묒뾽 釉붾줉???ы솗?명뻽??
- 2026-04-25: `2.Monster` ?대뜑 ?꾩껜, `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `combat-reward-system.md`, `dungeon-squad-run-structure.md`, 媛?紐ъ뒪?????臾몄꽌瑜??쎌뿀??
- 2026-04-25: ?꾩옱 肄붾뱶? ???곹깭瑜??ㅼ떆 ?뺤씤???꾩옱 援ы쁽???대툕 ?⑤룆 ?꾪닾 ?꾨줈?좏??낃낵 ?꾩떆 HUD ?섏??꾩쓣 ?ы솗?명뻽??
- 2026-04-25: UI ?먯궛 遺?? 蹂댁긽 ? 誘멸뎄?? ?띿꽦/?곹깭 怨듯넻 ?쒖뒪??遺議깆쓣 ?꾩옱 ?뺤옣 ?묒뾽???듭떖 媛?쑝濡??뺣━?덈떎.
- 2026-04-25: 紐ъ뒪???좏깮 UI, Run ?쒖옉, ?꾪닾 ??蹂댁긽/?ㅽ궗 ?좏깮 ?먮쫫???뺣━???ㅺ퀎 HTML `Pakuri/reference/monster-select-run-ui-expansion-plan.html`瑜?異붽??덈떎.
- 2026-04-25: ?ъ슜???듬???諛섏쁺???⑥떆釉뚮뒗 ?щ’ `F~J` 珥?5媛? ??以?理쒕? 3媛??듬뱷?쇰줈 ?ㅺ퀎瑜?怨좎젙?덇퀬, ?щ줈 蹂댁긽? ?곗꽑 ?쒖떆 ?꾩슜 ?뺣낫濡?泥섎━?섍린濡?湲곕줉?덈떎.
- 2026-04-25: ?ㅼ젣 ??μ냼?먯꽌 由곗쓽 `g~j` ?⑥떆釉?臾몄꽌媛 ?놁쓬???ㅼ떆 ?뺤씤?? 臾몄꽌 湲곕컲 ?꾩껜 紐ъ뒪??援ы쁽 ?꾩뿉 ?⑥? ?먮즺 媛?쑝濡?湲곕줉?덈떎.
- 2026-04-25: ?ъ슜???듬???諛섏쁺??1李?援ы쁽 踰붿쐞瑜?`?꾨━??, `?대툕`, `?몄씤`, `踰좉?` 4紐ъ뒪???곗꽑?쇰줈 怨좎젙?섍퀬, `由?? ?붾? ?곹깭濡??④린湲곕줈 湲곕줉?덈떎.

## Task: Run Flow UICanvas Prototype Implementation

### Task title

`run-systems-integration-summary-report.html` 湲곗? 泥?援ы쁽 ?щ씪?댁뒪 李⑹닔

### Goals

- 5紐ъ뒪???좏깮, `RunSession`, `RunFlowController`, `UICanvas` 湲곕컲 ?먮쫫??泥?援ы쁽 ?щ씪?댁뒪瑜?留뚮뱺??
- ?뺤쟻 ?곗씠?곕뒗 CSV ?고???濡쒕뱶 ???Unity ?꾨줈?앺듃 ?대? ?먯궛?쇰줈 留뚮뱺??
- ?꾩옱 `EveVerticalSliceController`瑜??좏깮 紐ъ뒪??湲곕컲 怨듯넻 A ?ㅽ궗 ?꾨줈?좏????꾪닾? A/F 理쒖냼 蹂댁긽 猷⑦봽媛 媛?ν븳 援ъ“濡??곕떎.

### Constraints

- ?ъ슜?먯쓽 ?붿껌?濡??좊땲???뚮젅???ㅽ뻾 寃利앹? ?ъ슜?먯뿉寃?留↔린怨? ???肄붾뱶/???먯궛 以鍮꾩? ?먮뵒???곹깭 ?뺤씤源뚯?留??쒕떎.
- UI??`UICanvas` 湲곗??쇰줈 ?ъ뿉 吏곸젒 諛곗튂?쒕떎.
- ?꾩옱 ?ъ슜?먯쓽 吏?쒕줈 ?몃? Reviewer ?④퀎???좎떆 以묒??섍퀬, Builder 醫낅즺 ?꾩뿉??媛꾨떒???먯껜 ?먭?留??섑뻾?쒕떎.
- 援ы쁽?섏? ?딆? B~E, G~J, ?좊Ъ 3??, ?꾩껜 ?쇳빀 蹂댁긽 ?? ?대쾲 ?щ씪?댁뒪 踰붿쐞???ｌ? ?딅뒗??

### Role Owner

Code Builder

### Status

Builder changes applied. ?몃? Reviewer 1??寃곌낵 諛섏쁺源뚯????꾨즺?먭퀬, ?댄썑 Reviewer ?④퀎???ъ슜??吏?쒕줈 ?좎떆 以묒??덈떎. `LegacyRuntime.ttf` 援먯껜? Unity ?ъ뺨?뚯씪源뚯? 留덉낀怨? ?꾩옱???ъ슜???뚮젅??寃利??湲??곹깭??

### Next Actions

- ?ъ슜?먭? Unity?먯꽌 ?뚮젅??紐⑤뱶濡?`RunUICanvas` ?숈옉, 5紐ъ뒪???좏깮, ?꾪닾 吏꾩엯, 理쒖냼 蹂댁긽 ?좏깮, ?ㅼ쓬 ?쇱감 吏꾪뻾??寃利앺븳??
- 寃利?以?UI 諛곗튂 臾몄젣???낅젰 臾몄젣, ?꾪닾 ?먮쫫 臾몄젣瑜??뺤씤?섎㈃ 洹?洹쇨굅瑜?諛쏆븘 ?ㅼ쓬 Builder ?섏젙?쇰줈 ?댁뼱媛꾨떎.
- ?댄썑 ?뺤옣? `?좊Ъ 3??`, `?좉퇋 ?≫떚釉??⑥떆釉??뱀꽦/留덉뒪???꾩껜 ?`, `B/G, C/H, D/I, E/J` ?쒖쑝濡?媛꾨떎.

### Evidence

- ???고????곗씠???ㅽ겕由쏀듃 `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`瑜?異붽??덈떎.
- ?먮뵒???쒕뱶 ?ㅽ겕由쏀듃 `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`瑜?異붽??덇퀬, Unity 硫붾돱 `Pakuri/Seed Default Game Data` ?ㅽ뻾?쇰줈 `Assets/Data/GameData/GameDataCatalog.asset`? 5媛?紐ъ뒪???먯궛???앹꽦?덈떎.
- ?????먮쫫 ?ㅽ겕由쏀듃 `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`瑜?異붽??덈떎.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`瑜??좏깮 紐ъ뒪??湲곕컲 怨듯넻 A ?ㅽ궗 ?꾨줈?좏????꾪닾? 理쒖냼 蹂댁긽 猷⑦봽瑜?泥섎━?섎룄濡??ш쾶 ?섏젙?덈떎.
- Unity ??`Assets/Scenes/SampleScene.unity`??猷⑦듃 `RunUICanvas`? `EventSystem`??吏곸젒 ?앹꽦?섍퀬 ??ν뻽??
- Unity asset search 寃곌낵 `Assets/Data/GameData/GameDataCatalog.asset`? `Assets/Data/GameData/Monsters/*.asset` 5媛쒓? ?ㅼ젣濡??앹꽦?먮떎.
- Unity root hierarchy ?ы솗??寃곌낵 `RunUICanvas`?먮뒗 `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`媛 遺숈뿀怨? `EventSystem`?먮뒗 `EventSystem`, `InputSystemUIInputModule`媛 遺숈뿀??
- ?몃? Reviewer 1??寃곌낵????媛吏 ?댁뒋瑜?吏?곹뻽?? 蹂댁긽 ?④낵媛 ?ㅼ쓬 ?쇱감???좎??섏? ?딅뒗 臾몄젣, ?ㅽ뀒?댁? 諛곗쑉???꾪닾/蹂댁긽??諛섏쁺?섏? ?딅뒗 臾몄젣, ?뚮젅??以?踰꾪듉 ?ъ깮?????뚮㈇ ?꾪뿕.
- 洹?吏?곸쓣 諛섏쁺??`RunSession`???꾩쟻 蹂댁긽 ?섏튂瑜?異붽??섍퀬, `EveVerticalSliceController.BeginConfiguredDay(...)`媛 ?몄뀡 ?꾩쟻 蹂댁긽???ㅼ떆 ?곸슜?섎룄濡??섏젙?덈떎.
- 媛숈? ?섏젙?먯꽌 `EveVerticalSliceController`??`stageIndex` 湲곕컲 ??泥대젰 諛곗쑉怨??대몺???붿쟻 吏湲?諛곗쑉??諛섏쁺?섎룄濡??섏젙?덈떎.
- `RunFlowController.ClearButtons(...)`???뚮젅??以??ъ깮??踰꾪듉??媛숈? ?대쫫?쇰줈 ?ъ궗?⑸릺吏 ?딅룄濡?`QueuedForDestroy` ?대쫫 蹂寃????쒓굅?섎룄濡??섏젙?덈떎.
- 2026-04-26 ?ъ슜???뚮젅??寃利앹뿉??`RunFlowController.ResolveReferences()`??`Arial.ttf` 李몄“媛 Unity ?댁옣 ?고듃 ?뺤콉怨?留욎? ?딆븘 `ArgumentException`??諛쒖깮?덇퀬, ?대? `LegacyRuntime.ttf`濡?援먯껜?덈떎.
- `LegacyRuntime.ttf` 援먯껜 ??Unity ?ㅽ겕由쏀듃 ?ъ뺨?뚯씪???붿껌?덇퀬, 理쒓렐 Unity console 20媛?濡쒓렇 ?ы솗?몄뿉?쒕뒗 ?숈씪??`Arial.ttf` ?덉쇅媛 ?ㅼ떆 蹂댁씠吏 ?딆븯??
- ?몃? Reviewer ?ъ떎?됱? 10遺???꾩븘???덉뿉 ?앸굹吏 ?딆븯怨? ?댄썑 Reviewer ?④퀎???ъ슜??吏?쒕줈 ?좎떆 以묒??덈떎.

### History

- 2026-04-26: Designer 湲곗??쇰줈 ?꾩옱 HTML怨??ㅼ젣 肄붾뱶/???곹깭瑜??ㅼ떆 ?쎄퀬 泥?Builder ?щ씪?댁뒪 踰붿쐞瑜?`?뺤쟻 ?곗씠???먯궛 + RunSession/RunFlowController + UICanvas + A/F 理쒖냼 蹂댁긽 猷⑦봽`濡?怨좎젙?덈떎.
- 2026-04-26: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`瑜??덈줈 異붽??덈떎.
- 2026-04-26: `Pakuri/Seed Default Game Data`瑜??ㅽ뻾??5紐ъ뒪??湲곕낯 ?먯궛怨?`GameDataCatalog.asset`瑜??앹꽦?덈떎.
- 2026-04-26: `RunUICanvas`, `EventSystem`???ъ뿉 異붽??섍퀬 ??ν뻽??
- 2026-04-26: ?몃? Reviewer 1?뚭? 蹂댁긽 ?좎?, ?ㅽ뀒?댁? 諛곗쑉, 踰꾪듉 ?ъ깮??臾몄젣瑜?吏?곹뻽怨? Builder媛 媛숈? ?댁뿉?????댁뒋瑜??섏젙?덈떎.
- 2026-04-26: ?섏젙 ??Unity console?먯꽌????而댄뙆???ㅻ쪟媛 蹂댁씠吏 ?딆븯怨? ?몃? Reviewer ?ъ떎?됱? ?쒓컙 珥덇낵濡?醫낅즺?먮떎.
- 2026-04-26: ?ъ슜???뚮젅??寃利앹뿉??`Resources.GetBuiltinResource<Font>("Arial.ttf")` ?덉쇅媛 蹂닿퀬?먭퀬, `RunFlowController`??湲곕낯 ?고듃瑜?`LegacyRuntime.ttf`濡?援먯껜?덈떎. 媛숈? ?쒖젏???ъ슜???붿껌?쇰줈 ?몃? Reviewer ?④퀎???좎떆 以묒??섍퀬 ?먯껜 ?먭?留??좎??섍린濡??덈떎.
- 2026-04-26: `LegacyRuntime.ttf` 援먯껜 ??Unity ?ъ뺨?뚯씪怨?理쒓렐 肄섏넄 濡쒓렇瑜??ㅼ떆 ?뺤씤?덇퀬, ?숈씪???고듃 ?덉쇅???ы쁽?섏? ?딆븯??

