# CSV_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

## Task: 2026-05-02 Typed CSV Source Runtime Pipeline

### Task title

Introduce a typed CSV source-of-truth pipeline under `Pakuri/Assets/CSVdata/source`.

### Goals

- Add CSV tables that can represent the current monster, reward-choice, skill, passive-choice, and stage-one enemy runtime data.
- Preserve the user's rule that legacy `Pakuri/data/*.csv` files are not rewritten in-place.
- Add parser-side and built-catalog validation so invalid source data becomes a fatal startup error.

### Constraints

- Role Owner is Code Builder.
- Use the actual runtime model shape, not the incomplete legacy CSV headers.
- Only row edits should be needed for normal content iteration inside the new typed source files.
- Do not claim Play Mode verification.

### Role Owner

Code Builder

### Status

Builder follow-up implemented and locally revalidated after the Reviewer findings. A later builder pass also split the `PakuriCsvRuntimeData` monolith into partial files without changing the typed CSV contract. The original Reviewer verdict remains FAIL until the user explicitly asks for another review.

### Next Actions

- Keep content edits inside `Pakuri/Assets/CSVdata/source/*.csv`.
- If a new content type needs new fields, extend the typed source schema and the runtime catalog sync together.
- If later requested, add a separate editor report that lists missing asset references and invalid enum values before startup.

### Evidence

- `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.cs` defines the typed CSV source contract and now loads source text through `PakuriCsvRuntimeSourceCatalog` instead of direct filesystem reads.
- The runtime loader is now physically split across `PakuriCsvRuntimeData.cs`, `PakuriCsvRuntimeData.Loader.cs`, `PakuriCsvRuntimeData.Validation.cs`, `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.Editor.cs`, and `PakuriCsvRuntimeData.Types.cs`, so the CSV pipeline is no longer concentrated in one 2000+ line file.
- The parser still expects a header row and a required second-row type declaration; `CsvTable.Load(...)` throws when a CSV has fewer than 2 rows.
- The active imported source files are:
- `Pakuri/Assets/CSVdata/source/catalog_monsters.csv`
- `Pakuri/Assets/CSVdata/source/catalog_stage_one_enemies.csv`
- `Pakuri/Assets/CSVdata/source/monsters.csv`
- `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv`
- `Pakuri/Assets/CSVdata/source/monster_skills.csv`
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`
- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` serializes references to those 7 imported CSV TextAssets.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` serializes the runtime-safe sprite/prefab dependency map that corresponds to the current CSV rows.
- `PakuriDataManager.Instance.GetData<T>(id)` now exists in `Pakuri/Assets/Scripts/Data/PakuriDataManager.cs` and is used by `RunFlowController.cs`, `RunCombatUiController.cs`, and `RunSceneBootstrap.cs` for monster lookup.
- The split kept the singleton boundary narrow: `PakuriDataManager` remains the only singleton-style query registry, while `PakuriCsvRuntimeData` stays a static bootstrap/service entry point instead of turning scene/runtime controllers into global singletons.
- `ValidateSourceModelOrThrow(...)` now checks duplicate ids, missing catalog references, missing monster references, active/passive slot rules, skill-choice linkage, and runtime asset-catalog coverage.
- `ValidateRuntimeCatalogOrThrow(...)` now checks the built in-memory catalog plus non-null bound assets for non-empty sprite/prefab paths.
- `Pakuri/Validate CSV Source Data` previously logged a successful 5-monster / 8-enemy load from resource source `Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog`.
- `Get-Content -Encoding UTF8 Pakuri/Assets/CSVdata/source/monsters.csv` confirmed the second-row type schema and readable Korean payload text.
- After the split, a full Unity asset refresh generated `.meta` files for each new `PakuriCsvRuntimeData.*.cs` file, and a later console read showed no C# compile errors before `Pakuri/Validate CSV Source Data` was re-run successfully.

### History

- 2026-05-02: User approved a typed CSV direction instead of forcing the runtime shape into the legacy CSV headers.
- 2026-05-02: Initial builder pass implemented parser, validator, exporter, UTF-8 writing, and validation menu support, but still used `Pakuri/data/source` and lacked a unified query contract.
- 2026-05-02: User imported the typed CSV into `Pakuri/Assets/CSVdata` and asked Builder to address the Reviewer findings.
- 2026-05-02: Builder switched the active source root to `Assets/CSVdata/source`, added runtime source/asset catalog assets under `Assets/Resources/Pakuri/CSVRuntime`, and added `PakuriDataManager`.
- 2026-05-02: Added `Pakuri/reference/Report/2026-05-02-data-structure-refactor-implementation-report.html` to summarize the current CSV runtime pipeline against the original review direction.
- 2026-05-02: Builder split the CSV runtime code into runtime-entry, loader, validation, build, editor, and type-support partial files, then revalidated compile/import and the CSV startup path through the Unity validation menu.

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

