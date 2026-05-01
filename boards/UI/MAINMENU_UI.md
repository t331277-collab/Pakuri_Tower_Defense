# MAINMENU_UI

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

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

