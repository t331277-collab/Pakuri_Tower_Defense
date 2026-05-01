# REPORT_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

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

## Task: 2026-04-27 Combat Implementation Status Reports

### Task title

Create HTML reports comparing today's combat / monster / enemy implementation with the implementation plan, and separately summarizing code-review-resolved work.

### Goals

- Compare today's implemented skill, damage calculation, Stage 1 enemy, Monster, projectile, and HP bar work against `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.
- Generate one HTML report for implementation status.
- Generate a separate HTML report for work found and resolved through self-review / reviewer-related review flow.
- Keep external Reviewer status accurate and do not claim a PASS verdict where the reviewer command did not complete.

### Constraints

- Role Owner is Designer.
- All claims must be grounded in actual files, BLACKBOARD history, and command output.
- Do not claim Unity Play Mode verification.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User can open `Pakuri/reference/Report/2026-04-27-combat-monster-enemy-implementation-status.html`.
- User can open `Pakuri/reference/Report/2026-04-27-code-review-resolved-work.html`.

### Evidence

- Created `Pakuri/reference/Report/2026-04-27-combat-monster-enemy-implementation-status.html`.
- Created `Pakuri/reference/Report/2026-04-27-code-review-resolved-work.html`.
- Read `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.
- Confirmed today's modified scripts with `Get-ChildItem Pakuri\Assets\Scripts -Recurse`.
- Confirmed actual code symbols with `Select-String` in `CombatStatModels.cs`, `DamageCalculator.cs`, `EnemyDefinition.cs`, `SkillDefinition.cs`, `MonsterDefinition.cs`, `GameDataCatalog.cs`, `PakuriGameDataSeeder.cs`, `EveVerticalSliceController.cs`, and `EnemyAttackResolver.cs`.
- Confirmed Stage 1 enemy assets exist under `Pakuri/Assets/Data/GameData/Enemies`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.

### History

- 2026-04-27: User requested two HTML reports: one comparing today's implementation with the combat-monster-enemy implementation plan, and another for code-review-resolved work.
- 2026-04-27: Generated both reports and verified their file presence and key headings.

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

