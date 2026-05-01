# RUNSCENE_UI

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

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

## Task: Preserve Authored UI Layouts

### Task title

?ъ슜???몄쭛 UI媛 ?뚮젅???쒖옉 ??肄붾뱶 湲곕낯媛믪쑝濡??섎룎?꾧???臾몄젣 ?섏젙

### Goals

- ?먮뵒?곗뿉???ъ슜?먭? ?섏젙??UI ?꾩튂, ?ш린, ?? ?고듃 ?ㅼ젙??寃뚯엫 ?쒖옉 ???좎??섍쾶 ?쒕떎.
- `MainMenuFlowController`, `RunCombatUiController`媛 湲곗〈 UI 怨꾩링??諛쒓껄?섎㈃ ?ъ깮??湲곕낯媛??ъ쟻?????李몄“留?罹먯떛?섍쾶 ?쒕떎.
- ??UI媛 ?놁쓣 ?뚮쭔 湲곕낯 UI瑜??앹꽦?쒕떎.

### Constraints

- ?몃? Code Reviewer???몄텧?섏? ?딄퀬 ?먯껜 肄붾뱶 由щ럭留??섑뻾?쒕떎.
- Codex媛 Unity ?뚮젅??紐⑤뱶瑜??ㅽ뻾??寃利앺븯吏 ?딄퀬, ?ㅼ젣 ?뚮젅??寃利앹? ?ъ슜?먯뿉寃?留↔릿??
- ?먮떒怨??ㅻ챸? ?ㅼ젣 ?뚯씪, ?ㅼ젣 ?? ?ㅼ젣 紐낅졊 異쒕젰 洹쇨굅瑜?湲곗??쇰줈 ?쒕떎.

### Role Owner

Code Builder

### Status

Builder changes applied. ?먯껜 鍮뚮뱶/肄섏넄 ?뺤씤源뚯? ?꾨즺?덇퀬, ?ъ슜???뚮젅??寃利??湲??곹깭??

### Next Actions

- ?ъ슜?먭? `MainMenuScene` ?먮뒗 `RunScene`?먯꽌 UI瑜??섏젙?????뚮젅?대? ?쒖옉???꾩튂/?ш린/?????몄쭛媛믪씠 ?좎??섎뒗吏 寃利앺븳??
- 留뚯빟 ?뱀젙 踰꾪듉???④퀎 ?꾪솚 以??덈줈 ?앹꽦?섏뼱 ?ㅽ??쇱씠 ?щ씪吏??寃쎌슦, ?대떦 踰꾪듉 ?대쫫怨??ъ쓣 洹쇨굅濡?諛쏆븘 怨좎젙 UI ?⑤꼸 諛⑹떇?쇰줈 ??遺꾨━?쒕떎.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`??湲곗〈 `MainMenuPanel`???덉쑝硫?`BuildUiScaffold()`瑜??ㅼ떆 ?ㅽ뻾?섏? ?딄퀬 `CacheUiReferences()`濡?湲곗〈 `Title`, `Summary`, `Buttons` 李몄“留??〓뒗??
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`??湲곗〈 `HudPanel`, `RewardPanel`, `DefeatPanel`???덉쑝硫?`BuildUiScaffold()`瑜??ㅼ떆 ?ㅽ뻾?섏? ?딄퀬 `CacheUiReferences()`濡?湲곗〈 李몄“留??〓뒗??
- ??而⑦듃濡ㅻ윭 紐⑤몢 ???ㅻ툕?앺듃/而댄룷?뚰듃媛 ?앹꽦??寃쎌슦?먮쭔 RectTransform ?ш린, Image ?? Text ?고듃/?뺣젹 媛숈? 湲곕낯 ?ㅽ??쇱쓣 ?곸슜?섎룄濡?蹂寃쏀뻽??
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`???ㅻ쪟 0媛쒕줈 ?듦낵?덈떎. ?⑥? 寃쎄퀬??Unity/MCPForUnity 李몄“??`System.Net.Http`, `System.IO.Compression` 踰꾩쟾 異⑸룎 寃쎄퀬 2媛쒕떎.
- Unity 肄섏넄 error 議고쉶?먯꽌?????ㅽ겕由쏀듃 而댄뙆???ㅻ쪟媛 蹂댁씠吏 ?딆븯怨? MCP client 醫낅즺 濡쒓렇留??뺤씤?먮떎.

### History

- 2026-04-26: ?ъ슜??寃利앹뿉??UI瑜??섏젙?대룄 寃뚯엫 ?쒖옉 ??肄붾뱶 湲곕낯媛믪쑝濡??섎룎?꾧???臾몄젣媛 蹂닿퀬?먮떎.
- 2026-04-26: ?ㅼ젣 肄붾뱶 ?뺤씤 寃곌낵 `BuildUiScaffold()`, `EnsurePanel()`, `EnsureText()`, `EnsureButton()`??湲곗〈 UI?먮룄 湲곕낯 RectTransform/???띿뒪???ㅽ??쇱쓣 諛섎났 ?곸슜?섍퀬 ?덉쓬???뺤씤?덈떎.
- 2026-04-26: 湲곗〈 UI媛 ?덉쑝硫?罹먯떛留??섑뻾?섍퀬, 湲곕낯 ?ㅽ??쇱? ?덈줈 ?앹꽦??UI?먮쭔 ?곸슜?섎룄濡??섏젙?덈떎.

## Task: RunScene Combat UI Restoration And Edit Mode Visibility

### Task title

RunScene ?꾪닾 HUD / 蹂댁긽 UI 蹂듦뎄? ?먮뵒??鍮꾩떎??UI ?쒖떆

### Goals

- `RunScene`?먯꽌 ?ㅽ뀒?댁? ?대━????蹂댁긽李쎌씠 ?ㅼ떆 ?④쾶 ?쒕떎.
- ?꾪닾 以????HP, 罹먮┃??HP, ?꾩갹, 由щ줈???⑥? 珥? ?ы솕 ?곹깭 HUD媛 ?ㅼ떆 蹂댁씠寃??쒕떎.
- `MainMenuScene`怨?`RunScene`??UI媛 ?뚮젅???ㅽ뻾 ???먮뵒???곹깭?먯꽌???앹꽦?섏뼱 吏곸젒 ?몄쭛 媛?ν븯寃??쒕떎.

### Constraints

- ?몃? Code Reviewer???몄텧?섏? ?딄퀬 ?먯껜 肄붾뱶 由щ럭留??섑뻾?쒕떎.
- Codex媛 Unity ?뚮젅??紐⑤뱶瑜??ㅽ뻾??寃利앺븯吏 ?딄퀬, ?ㅼ젣 ?뚮젅??寃利앹? ?ъ슜?먯뿉寃?留↔릿??
- ?먮떒怨??ㅻ챸? ?ㅼ젣 ?뚯씪, ?ㅼ젣 ?? ?ㅼ젣 紐낅졊 異쒕젰 洹쇨굅瑜?湲곗??쇰줈 ?쒕떎.

### Role Owner

Code Builder

### Status

Builder changes applied. ?먯껜 鍮뚮뱶/肄섏넄/??怨꾩링 ?뺤씤源뚯? ?꾨즺?덇퀬, ?ъ슜???뚮젅??寃利??湲??곹깭??

### Next Actions

- ?ъ슜?먭? Unity?먯꽌 `MainMenuScene -> RunScene` ?먮쫫???ㅽ뻾???꾪닾 HUD? ?대━????蹂댁긽李??쒖떆瑜?寃利앺븳??
- ?먮뵒??鍮꾩떎???곹깭?먯꽌 `MainMenuCanvas`, `RunCombatCanvas` ?섏쐞 UI ?ㅻ툕?앺듃瑜?吏곸젒 ?좏깮/?몄쭛?????덈뒗吏 ?뺤씤?쒕떎.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`??`[ExecuteAlways]`瑜?異붽??섍퀬, 鍮꾩떎???곹깭?먯꽌??`Touch To Start` UI瑜??앹꽦?섍쾶 ?덈떎.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`瑜?異붽???`RunScene` ?꾪닾 HUD, 蹂댁긽 ?⑤꼸, ?⑤같 ?⑤꼸???대떦?섍쾶 ?덈떎.
- `RunCombatUiController`??HUD?????HP, 罹먮┃??HP, ?꾩갹, ?ъ옣???⑥? ?쒓컙, 怨⑤뱶, ?붿쟻???쒖떆?쒕떎.
- `RunCombatUiController`???꾪닾 ?밸━ ??`EveVerticalSliceController`??蹂댁긽 ?꾨낫瑜??쎌뼱 蹂댁긽 踰꾪듉??留뚮뱾怨? 蹂댁긽 ?좏깮 ???ㅼ쓬 ?쇱감濡?吏꾪뻾?쒕떎.
- `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs`??`ActiveMonster`, `ActiveSession`, `FallbackMonsterId`瑜?怨듦컻???꾪닾 UI媛 ?꾩옱 ???몄뀡???쎌쓣 ???덇쾶 ?덈떎.
- Unity MCP ???묒뾽?쇰줈 `RunScene`??`RunCombatCanvas`? `RunCombatUiController`瑜?異붽??덇퀬, `CombatRoot` / `GameDataCatalog.asset` 李몄“瑜??곌껐?덈떎.
- Unity MCP 怨꾩링 ?뺤씤 寃곌낵 `MainMenuScene`??`MainMenuCanvas`?먮뒗 ?먯떇 UI 1媛쒓? ?앹꽦?먮떎.
- Unity MCP 怨꾩링 ?뺤씤 寃곌낵 `RunScene`??`RunCombatCanvas`?먮뒗 ?먯떇 UI 3媛쒓? ?앹꽦?먮떎.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`???ㅻ쪟 0媛쒕줈 ?듦낵?덈떎. ?⑥? 寃쎄퀬??Unity/MCPForUnity 李몄“??`System.Net.Http`, `System.IO.Compression` 踰꾩쟾 異⑸룎 寃쎄퀬 2媛쒕떎.
- Unity 肄섏넄 error 議고쉶?먯꽌?????ㅽ겕由쏀듃 而댄뙆???ㅻ쪟媛 蹂댁씠吏 ?딆븯怨? MCP client 醫낅즺 濡쒓렇留??뺤씤?먮떎.

### History

- 2026-04-26: ?ъ슜???뚮젅??寃利?寃곌낵 `RunScene`??HUD? ?대━????蹂댁긽 UI媛 ?쒖떆?섏? ?딅뒗 臾몄젣媛 蹂닿퀬?먮떎.
- 2026-04-26: ?먯씤? `RunScene` 遺꾨━ 怨쇱젙?먯꽌 湲곗〈 `RunFlowController`媛 ?쒓굅?섎ŉ ?꾪닾 HUD/蹂댁긽 UI ?대떦?먭? ?щ씪吏?寃껋쑝濡??먮떒?덈떎.
- 2026-04-26: `RunCombatUiController`瑜??덈줈 異붽??섍퀬 `RunScene`??`RunCombatCanvas`瑜?諛곗튂?덈떎.
- 2026-04-26: `MainMenuFlowController`? `RunCombatUiController`媛 ?먮뵒??鍮꾩떎???곹깭?먯꽌??UI ?먯떇??留뚮뱾?꾨줉 `[ExecuteAlways]` 湲곕컲?쇰줈 蹂댁젙?덈떎.

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

