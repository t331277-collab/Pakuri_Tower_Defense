# RUN_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

## Task: 2026-05-02 Run Skill And Reward Query Expansion

### Task title

Expand run-scene gameplay queries so monster skill and reward sub-data also flow through `PakuriDataManager`.

### Goals

- Stop run UI/debug consumers from reading `monster.ActiveSkills`, `monster.PassiveSkills`, and `monster.InitialRewardChoices` directly as their primary query path.
- Reuse the new collection-level query helpers for offering, debug toggles, and fallback skill lookups.
- Keep current run UI behavior unchanged while moving the lookup contract.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual script edits and actual Unity/editor output.
- Do not run Unity Play Mode verification.
- Code Reviewer has not run yet for this follow-up phase.

### Role Owner

Code Builder

### Status

Implemented, locally validated, and later reviewed with no discrete actionable bug reported.

### Next Actions

- User can verify in Play Mode that prisoner offerings, debug skill toggles, and fallback monster skill resolution still behave the same with CSV-backed data.
- If later requested, move `RunSession` learned-state checks away from `DisplayName` strings to stable ids.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now resolves active skills, passive skills, and initial reward choices through local helpers that call `PakuriDataManager.Instance.GetActiveSkills(...)`, `GetPassiveSkills(...)`, and `GetRewardChoices(...)`.
- `RunCombatUiController.cs` still resolves the fallback monster through `PakuriDataManager.Instance.ResolveMonster(...)`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` now rebuilds active/passive toggle lists through local helpers that call `PakuriDataManager.Instance.GetActiveSkills(...)` and `GetPassiveSkills(...)`.
- `MainMenuFlowController.cs`, `RunFlowController.cs`, and `RunSceneBootstrap.cs` still use the previously unified `PakuriDataManager` roster/fallback contract.
- `Select-String` over `Pakuri/Assets/Scripts/Run/*.cs` found the new `GetActiveSkills`, `GetPassiveSkills`, and `GetRewardChoices` helper calls in `RunCombatUiController.cs` and `DebugSceneController.cs`.
- After one compile-fix pass, Unity refresh completed without C# compile errors, and the CSV validation menu still loaded the 5-monster / 8-enemy runtime catalog.
- External `codex review --uncommitted` later covered the modified run-side files (`DebugSceneController`, `RunCombatUiController`) and reported no discrete actionable bug introduced by this patch.

### History

- 2026-05-02: User asked to finish the still-partial query-contract expansion after the earlier roster-level unification.
- 2026-05-02: Builder moved run combat offering/debug sub-data lookup onto `PakuriDataManager` collection queries and revalidated the scripts in Unity.
- 2026-05-02: The later reviewer pass inspected the modified run-side query-expansion files and did not raise an actionable follow-up bug.

## Task: 2026-05-02 Run Query Contract Unification

### Task title

Route run-entry and run-UI monster queries through `PakuriDataManager`.

### Goals

- Remove direct `GameDataCatalog.Monsters` reads from MainMenu, RunFlow, and DebugScene gameplay/UI paths.
- Keep fallback monster resolution in run-scene entry/UI behind one manager contract.
- Update missing-data messaging to the current CSV runtime source instead of the legacy seeder flow.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual script edits and actual Unity/editor output.
- Do not run Unity Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User can verify in Play Mode that MainMenu, RunScene fallback entry, and DebugScene still show/select monsters correctly with CSV-backed data.
- If later requested, move more run-scene setup to stable-id/context contracts so scene scripts no longer carry serialized catalog references at all.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs:165` now builds character-select buttons from `PakuriDataManager.Instance.GetMonsters(gameDataCatalog)`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs:70` and `:217` now read the monster roster through `PakuriDataManager` and no longer iterate `gameDataCatalog.Monsters` directly.
- `Pakuri/Assets/Scripts/Run/RunFlowController.cs:188` now resolves the monster list through `PakuriDataManager`, and `RunFlowController.cs:192` changed the missing-data hint to `Assets/CSVdata/source` and `Pakuri/CSVRuntime`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs:898` now resolves its fallback monster through `PakuriDataManager.Instance.ResolveMonster(...)`.
- `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs:62` now resolves its fallback monster through the same `ResolveMonster(...)` contract.
- After the change, the script-tree `Select-String` query for `gameDataCatalog.Monsters|gameDataCatalog.StageOneEnemies|fallbackCatalog.Monsters` no longer found run-scene consumer usage.
- Unity `read_console` after script refresh showed the runtime catalog load log and existing missing-script warnings, but no C# compile error entries.

### History

- 2026-05-02: User requested implementation of the previously identified query-contract unification.
- 2026-05-02: Builder replaced remaining run-scene monster roster and fallback-monster reads with `PakuriDataManager` helpers.
- 2026-05-02: Builder also updated the RunFlow missing-data text so it no longer points to the legacy `Pakuri/Seed Default Game Data` menu.

## Task: 2026-05-02 Run Catalog Source Resolution To CSV Runtime Data

### Task title

Switch run-scene catalog resolution to the new CSV runtime loader.

### Goals

- Make run-entry and run-UI scripts consume the typed CSV runtime catalog on startup.
- Keep serialized `GameDataCatalog` fields only as scene references, not as the hidden runtime source after CSV failure.
- Use the new query contract for monster-id lookup in run flow entry points.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual script edits and actual build/console output.
- Do not run Unity Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally revalidated after the CSV Reviewer follow-up.

### Next Actions

- User can verify in Play Mode that MainMenu -> RunScene and DebugScene still enter combat with CSV-backed data.
- If later requested, replace display-name based learned-skill checks in `RunSession` with stable ids from the CSV source.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`, `RunFlowController.cs`, `DebugSceneController.cs`, `RunCombatUiController.cs`, and `RunSceneBootstrap.cs` still resolve their catalog through `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)`.
- `Pakuri/Assets/Scripts/Run/RunFlowController.cs` now uses `PakuriDataManager.Instance.GetData<MonsterDefinition>(currentSession.SelectedMonsterId)` when starting or retrying combat.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now resolves the fallback monster through `PakuriDataManager.Instance.GetData<MonsterDefinition>(RunSceneBootstrap.FallbackMonsterId)`.
- `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs` now resolves its fallback monster through `PakuriDataManager.Instance.GetData<MonsterDefinition>(fallbackMonsterId)`.
- `PakuriCsvRuntimeData` initializes from `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`, and on failure now returns `null` instead of silently falling back to the serialized scene catalog.
- Unity refresh after the follow-up created the new data-script `.meta` files and later console reads showed no C# compile errors.
- `Pakuri/Validate CSV Source Data` previously logged a successful in-memory load from resource source `Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog`.

### History

- 2026-05-02: Initial builder pass updated run-scene consumers to prefer the runtime CSV catalog while preserving their serialized field.
- 2026-05-02: Reviewer follow-up request led Builder to add `PakuriDataManager` monster lookup and to block serialized fallback use after CSV initialization failure.
- 2026-05-02: Unity refresh after the follow-up completed without C# compile errors.

## Task: 2026-05-01 Run Structure Expansion Risk Review

### Task title

Review `RunSession` and run flow scripts for future content expansion risks.

### Goals

- Inspect the actual run-state and run-UI scripts under `Pakuri/Assets/Scripts/Run`.
- Identify structural issues for adding elite/shop branches, richer reward types, or persistent progression.

### Constraints

- Role Owner is Designer.
- Base all findings on actual script content and command output.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- If the user requests follow-up design, separate the work into `RunSession` identity cleanup, branchable day-flow design, and UI prefab/template strategy.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunSession.cs` stores learned actives/passives by display name strings, not stable IDs, and `RunCombatUiController.cs` / `DebugSceneController.cs` also check learned state by `skill.DisplayName` / `passive.DisplayName`.
- `Pakuri/Assets/Scripts/Run/RunDayModel.cs` clamps stages to `1..4` and days to `1..11`; `RunCombatType.Shop` exists in the enum, but `RunDayModel.Resolve(...)` never returns `Shop` or `Elite`.
- `Select-String` over `Pakuri/Assets/Scripts/**/*.cs` found `HasEliteOption` / `HasShopOption` only in `RunDayModel.cs`, so those branch flags are currently unused.
- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs` moves state to `RunScene` through `RunStartContext` and `DontDestroyOnLoad`; there is no disk-backed save or reload path in `Pakuri/Assets/Scripts`.
- `Pakuri/Assets/Scripts/Run/RunFlowController.cs` and `RunCombatUiController.cs` both implement reward-state UI and button generation against `CombatRuntimeController`, creating duplicate flow logic.
- `RunFlowController.cs` explicitly describes the current scope as `5몬스터 A 스킬 전투와 A/F 최소 보상 루프`, confirming the run loop is still prototype-limited.
- `Get-ChildItem Pakuri/Assets -Recurse -Filter *.prefab`, `*.uxml`, and `*.uss` returned `0`, so run UI is still fully code-built rather than asset-templated.

### History

- 2026-05-01: Reviewed `RunSession.cs`, `RunDayModel.cs`, `RunStartContext.cs`, `MainMenuFlowController.cs`, `RunFlowController.cs`, `RunCombatUiController.cs`, and `DebugSceneController.cs` for content expansion risk.

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

## Task: 2026-04-26 Run UI Implementation Status Report

### Task title

HTML report for completed and incomplete Run / UI implementation work on 2026-04-26

### Goals

- Compare today's implementation against `run-systems-integration-summary-report.html` and `monster-select-run-ui-expansion-plan.html`.
- Document completed work, incomplete work, UI editability issues, and chosen UI editing direction.

### Constraints

- All claims must be based on actual files, scene state, command output, or `BLACKBOARD.md` history.
- Do not include work-time estimates in the report.
- Reflect the user's decision that game data is made inside Unity and consumed from Unity assets, not from runtime CSV loading.
- Reflect the user's decision that UI will use editable scene UI: Codex may create a base UI, and user-authored UI should be modified/bound rather than replaced.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User can open `Pakuri/reference/2026-04-26-run-ui-implementation-status-report.html` to review the report.

### Evidence

- Created `Pakuri/reference/2026-04-26-run-ui-implementation-status-report.html`.
- The report references actual implementation files including `MainMenuFlowController.cs`, `RunCombatUiController.cs`, `RunSceneBootstrap.cs`, `RunStartContext.cs`, `RunSession.cs`, `MonsterDefinition.cs`, and `GameDataCatalog.cs`.
- File timestamp check confirmed the report exists under `Pakuri/reference`.
- Updated the report to remove work-time content, UI Toolkit incomplete-scope content, and user Play Mode verification from the incomplete-scope table.
- Updated the report to state that CSV is not the runtime data path; Unity-created assets such as `MonsterDefinition` and `GameDataCatalog` are the chosen data source.

### History

- 2026-04-26: User requested an HTML work report based on `run-systems-integration-summary-report.html` and `monster-select-run-ui-expansion-plan.html`.
- 2026-04-26: Read both source HTML files, implementation file lists, data asset lists, scene file timestamps, manifest TextMeshPro evidence, and generated the report.
- 2026-04-26: User requested removal of Play Mode verification, work-time content, and UI Toolkit incomplete-scope content; user also fixed the direction to Unity-created data assets and editable scene Canvas UI. Updated the report accordingly.

## Task: Main Menu To RunScene Flow Separation

### Task title

MainMenuScene ?④퀎 ?꾪솚怨?RunScene ?꾪닾 ?꾩슜 吏꾩엯 遺꾨━

### Goals

- `RunScene`???ㅼ뼱 ?덈뜕 罹먮┃???좏깮 UI ?먮쫫??`MainMenuScene`?쇰줈 遺꾨━?쒕떎.
- `MainMenuScene`? `Touch To Start -> ??踰꾪듉 -> 罹먮┃???좏깮 -> RunScene ?낆옣` ?④퀎 ?꾪솚???대떦?쒕떎.
- `RunScene`? ?좏깮??罹먮┃?곗? `RunSession`??諛쏆븘 ?꾪닾留??쒖옉?쒕떎.
- ??媛??꾨떖? ?뺤옣?깆쓣 怨좊젮??`DontDestroyOnLoad` 湲곕컲 `RunStartContext`濡?泥섎━?쒕떎.

### Constraints

- ?몃? Code Reviewer???몄텧?섏? ?딄퀬 ?먯껜 肄붾뱶 由щ럭留??섑뻾?쒕떎.
- Codex媛 Unity ?뚮젅??紐⑤뱶瑜??ㅽ뻾??寃利앺븯吏 ?딄퀬, ?ㅼ젣 ?뚮젅??寃利앹? ?ъ슜?먯뿉寃?留↔릿??
- ?먮떒怨??ㅻ챸? ?ㅼ젣 ?뚯씪, ?ㅼ젣 ?? ?ㅼ젣 紐낅졊 異쒕젰 洹쇨굅瑜?湲곗??쇰줈 ?쒕떎.

### Role Owner

Code Builder

### Status

Builder changes applied. ?먯껜 肄붾뱶 由щ럭? 鍮뚮뱶 ?뺤씤源뚯? ?꾨즺?덇퀬, ?ъ슜???뚮젅??寃利??湲??곹깭??

### Next Actions

- ?ъ슜?먭? Unity?먯꽌 `MainMenuScene`???ㅽ뻾??`Touch To Start -> ??-> 罹먮┃???좏깮 -> RunScene ?꾪닾 吏꾩엯` ?먮쫫??寃利앺븳??
- 寃利?以????꾪솚, ?낅젰, ?꾪닾 珥덇린??臾몄젣媛 ?덉쑝硫?洹?洹쇨굅瑜?諛쏆븘 ?ㅼ쓬 Builder ?섏젙?쇰줈 ?댁뼱媛꾨떎.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunStartContext.cs`瑜?異붽????좏깮 紐ъ뒪?곗? `RunSession`??`DontDestroyOnLoad` 而⑦뀓?ㅽ듃濡??꾨떖?섍쾶 ?덈떎.
- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`瑜?異붽???`Touch To Start`, `??, 罹먮┃???좏깮 ?④퀎瑜?媛숈? `MainMenuScene` Canvas ?덉뿉???꾪솚?섍쾶 ?덈떎.
- `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs`瑜?異붽???`RunScene`?먯꽌 `RunStartContext`瑜??쎄퀬 `EveVerticalSliceController.BeginConfiguredDay(...)`瑜??몄텧?섍쾶 ?덈떎.
- `Pakuri/Assets/Scripts/Run/RunSession.cs`?먮뒗 ?꾨씫?섏뼱 ?덈뜕 `using System;`留??뺣━??`Serializable`, `StringComparison`, `Math` ?ъ슜 洹쇨굅瑜?紐낆떆?덈떎.
- Unity MCP ???묒뾽?쇰줈 `RunScene`?먯꽌 `RunUICanvas`媛 ?쒓굅?먭퀬, `RunSceneBootstrap` 猷⑦듃 ?ㅻ툕?앺듃媛 異붽??먮떎.
- Unity MCP ???묒뾽?쇰줈 `MainMenuScene`?먮뒗 `MainMenuCanvas`? `MainMenuFlowController`, `EventSystem`??異붽??먮떎.
- `Pakuri/ProjectSettings/EditorBuildSettings.asset`??`Assets/Scenes/MainMenuScene.unity`, `Assets/Scenes/RunScene.unity` ?쒖꽌濡?媛깆떊?먮떎.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`???ㅻ쪟 0媛쒕줈 ?듦낵?덈떎. ?⑥? 寃쎄퀬??Unity/MCPForUnity 李몄“??`System.Net.Http`, `System.IO.Compression` 踰꾩쟾 異⑸룎 寃쎄퀬 2媛쒕떎.
- Unity 肄섏넄 error 議고쉶?먯꽌?????ㅽ겕由쏀듃 而댄뙆???ㅻ쪟媛 蹂댁씠吏 ?딆븯怨? MCP client 醫낅즺 濡쒓렇留??뺤씤?먮떎.

### History

- 2026-04-26: ?ъ슜??吏?쒕줈 ?몃? Reviewer ?몄텧 ?놁씠 ?먯껜 由щ럭留??섑뻾?섍퀬, ?ㅼ젣 ?뚮젅??寃利앹? ?ъ슜?먯뿉寃?留↔린湲곕줈 ?뺤젙?덈떎.
- 2026-04-26: ?꾩옱 ?ㅼ젣 ???뚯씪??`SampleScene.unity`媛 ?꾨땲??`MainMenuScene.unity`, `RunScene.unity`?꾩쓣 ?뺤씤?덈떎.
- 2026-04-26: `RunScene.unity`??`RunUICanvas`? `RunFlowController`媛 ?⑥븘 ?덉뼱 罹먮┃???좏깮???꾪닾 ???덉뿉 臾띠뿬 ?덉쓬???뺤씤?덈떎.
- 2026-04-26: `RunStartContext`, `MainMenuFlowController`, `RunSceneBootstrap`瑜?異붽??섍퀬 `RunScene` / `MainMenuScene` / Build Settings瑜?媛깆떊?덈떎.
- 2026-04-26: ?먯껜 寃利앹쑝濡?`dotnet build`? Unity 肄섏넄 ?뺤씤???섑뻾?덈떎.

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

## Task: SaveAndLoad Direction Plan

### Task title

Run / Meta ???寃쎄퀎? SaveAndLoad 援ъ“ ?ㅺ퀎 HTML ?묒꽦

### Goals

- ?꾩옱 Run ?뺤옣 ?ㅺ퀎? `reference/4.run`, `reference/6.meta` 臾몄꽌瑜?洹쇨굅濡????/ 遺덈윭?ㅺ린 諛⑺뼢???뺣━?쒕떎.
- ???대? ??κ낵 硫뷀? ?곴뎄 ??μ쓽 寃쎄퀎瑜?遺꾨━?쒕떎.
- v1?먯꽌 ??ν븷 寃? ?섏쨷??誘몃０ 寃? ??ν븯吏 ?딆쓣 ?고????곹깭瑜?HTML 臾몄꽌 ???μ쑝濡??뺣━?쒕떎.

### Constraints

- ?ㅼ젣 臾몄꽌? ?ㅼ젣 ?꾩옱 肄붾뱶 援ъ“瑜?洹쇨굅濡쒕쭔 ?곷뒗??
- ?꾩쭅 誘몄옉?깆씤 硫뷀? ?닿툑 臾몄꽌瑜?援ы쁽??寃껋쿂???곸? ?딅뒗??
- ???묒뾽? Designer ?ㅺ퀎 臾몄꽌 ?묒꽦?대ŉ, ?ㅼ젣 SaveLoad 肄붾뱶 援ы쁽? ?ы븿?섏? ?딅뒗??

### Role Owner

Designer

### Status

Completed

### Next Actions

- ?ъ슜?먭? ?먰븯硫???臾몄꽌瑜?湲곗??쇰줈 Code Builder handoff瑜??묒꽦??`RunSession`, `MetaSaveData`, `RunSnapshot`, `SaveLoadService` 援ы쁽 ?쒖꽌瑜?怨좎젙?쒕떎.
- ?ㅼ젣 援ы쁽? `GameDataCatalog` 遺??濡쒕뱶 援ъ“? `RunSession` 遺꾨━ ??泥댄겕?ъ씤????λ????쒖옉?섎뒗 寃껋씠 留욌떎.

### Evidence

- `Pakuri/reference/4.run/dungeon-squad-run-structure.md`??11???⑥쐞 ?ㅽ뀒?댁?, ?쇰컲 吏꾪뻾???좏깮吏, ?꾪닾 ??蹂댁긽, ?ㅼ쓬 ?쇱감 ?대룞 ?먮쫫???뺤쓽?쒕떎.
- `Pakuri/reference/4.run/combat-reward-system.md`??怨⑤뱶媛 ???대? ?ы솕?대ŉ ??醫낅즺 ???щ씪吏怨? ?대몺???붿쟻?????몃? ?ы솕?쇨퀬 ?뺤쓽?쒕떎.
- `Pakuri/reference/4.run/shop-system.md`???곸젏???ㅽ뀒?댁???1?? 6~9??以??섎（留??깆옣?쒕떎怨??뺤쓽?쒕떎.
- `Pakuri/reference/4.run/event-system.md`???쇰컲 / ?뺤삁 ?꾪닾 吏꾩엯 吏곹썑 20% ?뺣쪧 ?대깽?몄? ?꾪닾 蹂듦? ?먮쫫???뺤쓽?쒕떎.
- `Pakuri/reference/6.meta/meta-growth-index.md`??硫뷀? ?깆옣?먯꽌 ?꾩옱 ?뺤젙??踰붿쐞? 誘몄옉??踰붿쐞瑜?援щ텇?쒕떎.
- `Pakuri/reference/6.meta/meta-growth-node-list.md`??罹먮┃?곕퀎 怨듯넻 ?ㅽ꺈 媛뺥솕? 珥덇린??洹쒖튃???뺤쓽?쒕떎.
- `Pakuri/reference/6.meta/active-skill-growth-node-list.md`??罹먮┃?곕퀎 ?≫떚釉?硫뷀? 媛뺥솕 洹쒖튃???뺤쓽?쒕떎.
- `Pakuri/reference/6.meta/dark-trace-currency-system.md`???대몺 怨꾩뿴 ?ы솕 ?곗뼱, ?밴툒, ?ъ슜泥? 硫뷀? 珥덇린??洹쒖튃???뺤쓽?쒕떎.
- `Pakuri/reference/monster-select-run-ui-expansion-plan.html`? `RunSession` 遺꾨━? Run ?몄뀡 ?곗씠???쒖븞???ы븿?쒕떎.
- `Pakuri/reference/monster-select-run-ui-builder-handoff.html`? 怨좎젙 援ы쁽 ?쒖꽌?먯꽌 `RunSession` / `RunFlowController` 遺꾨━瑜?癒쇱? ?붽뎄?쒕떎.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`???꾩옱 ?꾪닾, ?쇱감 吏꾪뻾, 蹂댁긽, UI瑜????대옒?ㅼ뿉 ?④퍡 ?ㅺ퀬 ?덈떎.
- `Pakuri/data` CSV??`Assets` 諛붽묑???덇퀬, ?꾩옱 `Assets/Resources`, `Assets/StreamingAssets`, CSV 濡쒕뜑 ?붿쟻???녿떎.
- `Pakuri/reference/save-and-load-plan.html`? ?댁젣 ???援ъ“肉??꾨땲??`CSV ????먮낯 -> ?고????앹꽦 ?먯궛 -> 寃뚯엫 ?쒖옉 ??1??濡쒕뱶` 諛⑺뼢源뚯? ?ы븿?쒕떎.

### History

- 2026-04-26: `AGENTS.md`, `BLACKBOARD.md`, `monster-select-run-ui-expansion-plan.html`, `monster-select-run-ui-builder-handoff.html`, `reference/4.run`, `reference/6.meta`, ?꾩옱 `EveVerticalSliceController.cs`瑜??ㅼ떆 ?쎌뿀??
- 2026-04-26: SaveAndLoad瑜?`MetaSaveData`, `RunSnapshot`, `EphemeralRuntime` 3痢듭쑝濡??섎늻怨? v1? ?쇱감 寃쎄퀎 泥댄겕?ъ씤????λ쭔 吏?먰븯??諛⑺뼢?쇰줈 ?뺣━??HTML??`Pakuri/reference/save-and-load-plan.html`??異붽??덈떎.
- 2026-04-26: `Pakuri/data` CSV 寃??寃곌낵瑜?諛섏쁺??`save-and-load-plan.html`???뺤쟻 寃뚯엫 ?곗씠??濡쒕뵫 諛⑺뼢, importer 湲곕컲 ?앹꽦 ?먯궛 援ъ“, 遺????1??濡쒕뱶 諛⑹떇??異붽??덈떎.

## Task: Run Systems Integration Summary Report

### Task title

`monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan` ?듯빀 蹂닿퀬??HTML ?묒꽦

### Goals

- 湲곗〈 3媛??ㅺ퀎 HTML??怨듯넻 寃곕줎?????μ쑝濡??⑹퀜 ?꾩옱 ?꾨줈?앺듃媛 ?대뼡 援ъ“濡??묒뾽?좎? 鍮좊Ⅴ寃?蹂댁뿬以??
- ?꾩옱 ?ㅼ젣 肄붾뱶 ?곹깭? 臾몄꽌 湲곗? 援ъ“瑜??④퍡 ?뺣━?? 援ы쁽 ?덉젙 踰붿쐞? ?꾩쭅 ?대Ⅸ 踰붿쐞瑜?遺꾨━?쒕떎.
- 湲고쉷?쒓? ?꾩쭅 遺議깊븳 遺遺꾧낵 ?꾩옱 ?곸슜?섍린 ?대Ⅸ ?곗씠???뚯씠?꾨씪?몄쓣 紐낆떆?곸쑝濡?`異뷀썑 援ы쁽 ?덉젙`?쇰줈 湲곕줉?쒕떎.

### Constraints

- ?ㅼ젣 議댁옱?섎뒗 3媛?HTML, ?ㅼ젣 ?꾩옱 肄붾뱶, ?ㅼ젣 臾몄꽌 ?곹깭瑜?洹쇨굅濡쒕쭔 ?곷뒗??
- ?꾩쭅 援ы쁽?섏? ?딆? UI, ??? ?곗씠??importer瑜?援ы쁽??寃껋쿂???곸? ?딅뒗??
- ???묒뾽? Designer 蹂닿퀬???묒꽦?대ŉ, ?ㅼ젣 肄붾뱶 援ы쁽? ?ы븿?섏? ?딅뒗??

### Role Owner

Designer

### Status

Completed

### Next Actions

- ?ъ슜?먭? ?먰븯硫????듯빀 蹂닿퀬?쒕? 湲곗??쇰줈 Designer媛 Code Builder handoff 臾몄꽌瑜???吏㏐쾶 ?ㅼ떆 ?뺣━?????덈떎.
- ?ㅼ젣 援ы쁽? 蹂닿퀬?쒖뿉 ?곸? ?쒖꽌?濡?`RunSession` 遺꾨━, UI ?먮쫫 遺꾨━, ?뺤쟻 ?곗씠???먯궛, A/F 理쒖냼 蹂댁긽 / ?ㅽ궗?좏깮, 泥댄겕?ъ씤??????쒖쑝濡??ㅼ뼱媛??寃껋씠 ?덉쟾?섎떎.

### Evidence

- `Pakuri/reference/monster-select-run-ui-builder-handoff.html`??`RunSession`, `RunFlowController` ?먮뒗 ?숇벑 援ъ“瑜?癒쇱? ?몄슦??怨좎젙 援ы쁽 ?쒖꽌瑜??쒖븞?쒕떎.
- `Pakuri/reference/monster-select-run-ui-expansion-plan.html`??紐ъ뒪???좏깮 UI, Run ?쒖옉, ?꾪닾 ??蹂댁긽/?좏깮 ?먮쫫怨?`RunSession` 以묒떖 援ъ“瑜??ㅻ챸?쒕떎.
- `Pakuri/reference/save-and-load-plan.html`??`MetaSaveData`, `RunSnapshot`, `GameDataCatalog` 遺꾨━? 遺????1???곗씠??濡쒕뱶瑜??뺤쓽?쒕떎.
- ?꾩옱 ?꾨줈?앺듃??寃뚯엫 ?꾩슜 ?ㅽ겕由쏀듃??`Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`留??뺤씤?쒕떎.
- ?꾩옱 `Pakuri/Assets` ?꾨옒?먮뒗 `Scenes`, `Screenshots`, `Scripts`, `Settings`留??덇퀬, `Resources`, `StreamingAssets`, `DataGenerated`???녿떎.
- ?꾩옱 ?꾨줈?앺듃?먮뒗 `.uxml`, `.uss` UI Toolkit ?먯궛???녿떎.
- ?ㅼ젣 CSV ?먮낯? `Pakuri/data`???덉?留??꾩옱 濡쒕뜑? ?앹꽦 ?먯궛 ?뚯씠?꾨씪?몄? ?녿떎.
- ???듯빀 臾몄꽌 `Pakuri/reference/run-systems-integration-summary-report.html`瑜?異붽??덇퀬, 臾몄꽌 ?덉뿉 ?꾩옱 援ъ“, ?묒뾽 ?쒖꽌, ????곗씠??諛⑺뼢, `異뷀썑 援ы쁽 ?덉젙` ??ぉ???④퍡 ?뺣━?덈떎.
- 2026-04-26 ?ы솗??寃곌낵 `Pakuri/reference/2.Monster/rin/rin-tower.md`? `rin/skill/g~j` 臾몄꽌媛 議댁옱?? 由곗쓽 ?⑥떆釉?臾몄꽌 遺議??꾩젣?????댁긽 ?좏슚?섏? ?딅떎.
- 2026-04-26 ?ы솗??寃곌낵 `Pakuri/Assets` ?ш? 寃?됱뿉??`ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset` 愿???뺤쟻 ?곗씠??濡쒕뜑 / ?먯궛 ?뺤쓽???뺤씤?섏? ?딆븯??
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`???꾩옱 蹂댁긽 ?⑤꼸?먯꽌 ?대툕 ?꾩슜 怨좎젙 ?좏깮吏 3媛쒕쭔 吏곸젒 ?앹꽦?쒕떎.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`? `Pakuri/reference/4.run/combat-reward-system.md`???꾩껜 蹂댁긽 / ?ㅽ궗?좏깮 洹쒖튃???뺤쓽?섏?留? ?꾩옱 援ы쁽? 洹??꾩껜 踰붿쐞???꾩쭅 ?꾨떖?섏? ?딆븯??

### History

- 2026-04-26: `AGENTS.md`, `BLACKBOARD.md`, 湲곗〈 3媛??ㅺ퀎 HTML???ㅼ떆 ?쎄퀬 ?쒕줈 寃뱀튂??援ъ“? 怨좎젙 寃곕줎??異붾졇??
- 2026-04-26: ?꾩옱 ?ㅼ젣 肄붾뱶? ?먯궛 ?곹깭瑜??ㅼ떆 ?뺤씤?? ?꾩쭅 ?녿뒗 UI Toolkit ?먯궛怨??곗씠???앹꽦 ?뚯씠?꾨씪?몄쓣 蹂닿퀬?쒖뿉 紐낆떆?곸쑝濡?鍮꾧뎄???곹깭濡??곸뿀??
- 2026-04-26: `Pakuri/reference/run-systems-integration-summary-report.html`瑜?異붽????꾩옱 援ъ“, 沅뚯옣 援ы쁽 ?쒖꽌, ?곗씠?????寃쎄퀎, 湲고쉷 遺議??곸뿭怨??대Ⅸ ?곗씠???곸슜 踰붿쐞瑜?`異뷀썑 援ы쁽 ?덉젙`?쇰줈 遺꾨━?덈떎.
- 2026-04-26: 由?臾몄꽌 媛깆떊怨??곗씠??諛⑺뼢 蹂寃쎌쓣 諛섏쁺??`run-systems-integration-summary-report.html`瑜??섏젙?덇퀬, 由곗쓣 5紐ъ뒪??踰붿쐞???ы븿?쒗궎怨??뺤쟻 ?곗씠?곕뒗 CSV importer ?꾩젣媛 ?꾨땲??Unity ?꾨줈?앺듃 ?대? ?뺤쟻 ?먯궛 湲곗??쇰줈 ?뺣━?덈떎.
- 2026-04-26: 蹂댁긽 / ?ㅽ궗?좏깮? ?꾩쟾???섏쨷?쇰줈 誘몃（吏 ?딄퀬, `RunSession` / UI / 怨듯넻 ?꾪닾 肄붿뼱 ?ㅼ쓬 留덉씪?ㅽ넠?먯꽌 A/F 理쒖냼 踰붿쐞瑜?媛숈씠 遺숈씠??諛⑺뼢?쇰줈 `run-systems-integration-summary-report.html`瑜??ㅼ떆 ?섏젙?덈떎.

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

