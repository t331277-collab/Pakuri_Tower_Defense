# DATA_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

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

## Task: CSV Data Role And Loading Review

### Task title

`Pakuri/data` CSV ??븷 ?뚯븙 諛?寃뚯엫 濡쒕뵫 諛⑹떇 寃??

### Goals

- `Pakuri/data` ?꾨옒 CSV?ㅼ쓽 ?ㅼ젣 ??븷???뚯씪 援ъ“? ?섑뵆 ??湲곗??쇰줈 遺꾨쪟?쒕떎.
- ?꾩옱 ?꾨줈?앺듃 肄붾뱶媛 ??CSV?ㅼ쓣 ?ㅼ젣濡??쎄퀬 ?덈뒗吏 ?뺤씤?쒕떎.
- 寃뚯엫?먯꽌 ???곗씠?곕? ?몄젣, ?대뼡 諛⑹떇?쇰줈 遺덈윭?ㅻ뒗 寃껋씠 留욌뒗吏 ?ㅺ퀎 ?먮떒???④릿??

### Constraints

- ?ㅼ젣 CSV ?댁슜, ?ㅼ젣 ?꾩옱 ?ㅽ겕由쏀듃, ?ㅼ젣 ?대뜑 ?꾩튂瑜?洹쇨굅濡쒕쭔 ?먮떒?쒕떎.
- ?꾩쭅 ?녿뒗 CSV 濡쒕뜑???곗씠???뚯씠?꾨씪?몄쓣 ?대? ?덈떎怨?留먰븯吏 ?딅뒗??
- ???묒뾽? Designer 遺꾩꽍?대ŉ, CSV 濡쒕뜑 援ы쁽? ?ы븿?섏? ?딅뒗??

### Role Owner

Designer

### Status

Completed

### Next Actions

- ?ъ슜?먭? ?먰븯硫???遺꾩꽍??湲곗??쇰줈 Code Builder handoff瑜??묒꽦??CSV importer ?먮뒗 ScriptableObject ?앹꽦 ?뚯씠?꾨씪??援ы쁽 踰붿쐞瑜?怨좎젙?쒕떎.
- 異붿쿇 諛⑺뼢? `Pakuri/data`瑜?????먮낯?쇰줈 ?좎??섍퀬, 鍮뚮뱶???고????곗씠?곕뒗 `Assets` ?꾨옒 ?앹꽦 ?먯궛?쇰줈 蹂?섑븯??諛⑹떇?대떎.

### Evidence

- `Pakuri/data` ?꾨옒 CSV??珥?22媛쒖씠硫?珥??ш린????28.22KB??
- `ally_units.csv`, `ally_runtime.csv`, `enemies.csv`, `enemy_runtime.csv`???뺤쟻 ?ㅽ꺈怨??고????꾪닾 ?뚮씪誘명꽣媛 遺꾨━??援ъ“??
- `skills.csv`, `skill_runtime.csv`, `skill_branches.csv`, `levelup_choices.csv`, `levelup_rules.csv`???ㅽ궗 / 遺꾧린 / ?덈꺼???좏깮吏 ?곗씠?곕? 媛吏꾨떎.
- `waves_chapter1.csv`, `waves_chapter2.csv`, `waves_chapter3.csv`, `waves_runtime.csv`, `boss_patterns.csv`???⑥씠釉?/ 蹂댁뒪 ?⑦꽩 / ?꾪닾 吏꾪뻾 ?곗씠?곕? 媛吏꾨떎.
- `items.csv`, `status_effects.csv`, `formations.csv`, `balance_targets.csv`???λ퉬 / ?곹깭?댁긽 / 諛곗튂 / 諛몃윴??紐⑺몴 ?곗씠?곕? 媛吏꾨떎.
- `spawn_points.csv`??2踰덉㎏ 以꾩뿉 `???ㅽ룿 醫뚰몴??CSV媛 ?꾨땲??肄붾뱶?먯꽌 泥섎━?쒕떎.`怨??곹? ?덉뼱 ?꾩옱 鍮꾪솢???곗씠?곕떎.
- `towers.csv`, `tower_skills.csv`??`TOWER_001` 以묒떖??援ы삎 ?⑥씪 ????꾨줈?좏????곗씠?곕떎.
- `ally_units.csv`??`ALLY_*` 泥닿퀎?몃뜲 `skills.csv`??`TOWER_001` ?뚯쑀 ?ㅽ궗留?媛吏怨??덉뼱 ?곗씠??紐⑤뜽???쇱옱?섏뼱 ?덈떎.
- ?ㅼ젣 臾닿껐???뺤씤 寃곌낵 `ally_units.csv`, `levelup_choices.csv`, `skill_branches.csv`媛 李몄“?섎뒗 `SKILL_004` ?댁긽 ?ㅼ닔媛 `skills.csv`???녿떎.
- `Pakuri/data`??`Assets` 諛붽묑???덉쑝硫? ?꾩옱 `Assets/Resources`, `Assets/StreamingAssets` ?붾젆?곕━??議댁옱?섏? ?딅뒗??
- `Pakuri/Assets/Scripts`? ?꾨줈?앺듃 ?띿뒪???뚯씪 寃??寃곌낵 CSV 濡쒕뜑??`TextAsset`, `Resources.Load`, `StreamingAssets` ?ъ슜 ?붿쟻? ?뺤씤?섏? ?딆븯??

### History

- 2026-04-26: `AGENTS.md`, `BLACKBOARD.md`瑜??ㅼ떆 ?쎄퀬 `Pakuri/data` ?꾩껜 CSV 紐⑸줉, ?ㅻ뜑, 泥????섑뵆???뺤씤?덈떎.
- 2026-04-26: ?ㅽ궗 李몄“ 臾닿껐?깆쓣 ?먭???`ALLY_*` 湲곕컲 ?곗씠?곗? `TOWER_*` 湲곕컲 ?곗씠?곌? ?쇱옱?섏뼱 ?덇퀬, ?쇰? ?ㅽ궗 李몄“媛 鍮꾩뼱 ?덉쓬???뺤씤?덈떎.
- 2026-04-26: ?꾩옱 CSV??鍮뚮뱶 ?ы븿 ?꾩튂???덉? ?딄퀬 濡쒕뜑???놁쑝誘濡? ?고???吏곸젒 CSV ?뚯떛蹂대떎 鍮뚮뱶 ??蹂???먯궛 諛⑹떇?????덉쟾?섎떎怨??뺣━?덈떎.
- 2026-04-26: ???먮떒??`Pakuri/reference/save-and-load-plan.html` 蹂몃Ц?먮룄 諛섏쁺??SaveAndLoad? ?뺤쟻 ?곗씠??濡쒕뵫 寃쎄퀎瑜??④퍡 臾몄꽌?뷀뻽??

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

