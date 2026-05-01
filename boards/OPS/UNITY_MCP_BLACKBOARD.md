# UNITY_MCP_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Migrated Task Blocks

## Task: Unity MCP Bridge Connection

### Task title

Unity MCP bridge ?곌껐 諛??깅줉 ?뺤씤

### Goals

- ?꾩옱 ?뚰겕?ㅽ럹?댁뒪??Unity ?꾨줈?앺듃 `Pakuri`?먯꽌 Unity MCP bridge瑜?Codex MCP ?쒕쾭? ?곌껐?쒕떎.
- Codex CLI 履?MCP ?깅줉 ?곹깭? Unity Editor 履?bridge ?ㅽ뻾 ?곹깭瑜??ㅼ젣 紐낅졊 異쒕젰?쇰줈 援щ텇?쒕떎.
- ?ъ슜?먭? Unity Editor ??MCP For Unity ?ㅼ젙??吏곸젒 議곗옉?댁빞 ?섎뒗 寃쎌슦, ?꾩슂????ぉ??紐낇솗??吏덈Ц?쒕떎.

### Constraints

- 紐⑤뱺 ?먮떒? ?ㅼ젣 ?뚯씪, ?⑦궎吏 肄붾뱶, 紐낅졊 異쒕젰??洹쇨굅?쒕떎.
- Unity ?꾨줈?앺듃 ?뚯씪? ?ъ슜???붿껌 ?놁씠 ?섏젙?섏? ?딅뒗??
- Unity Editor ?대? bridge ?쒖옉? ?ㅼ젣 ?곌껐 ?뺤씤 ?꾧퉴吏 ?꾨즺??寃껋쑝濡?留먰븯吏 ?딅뒗??

### Role Owner

Code Builder

### Status

Completed. Unity Editor-side MCP For Unity bridge is connected to the current Codex MCP server.

### Next Actions

- ?댄썑 Unity MCP媛 ?딄린硫?Unity Editor?먯꽌 Transport瑜?`Stdio`濡??먭퀬 `Session Active`瑜??ㅼ떆 耳???`manage_scene get_active`濡??ш?利앺븳??
- Unity Test Runner ?뺤씤? `run_tests EditMode` ??`get_test_job`?쇰줈 寃곌낵瑜??뺤씤?쒕떎.

### Evidence

- `Pakuri/ProjectSettings/ProjectVersion.txt` 異쒕젰: `m_EditorVersion: 6000.3.4f1`
- 2026-04-25 ?ы솗??`Pakuri/ProjectSettings/ProjectVersion.txt` 異쒕젰: `m_EditorVersion: 6000.3.14f1`
- 2026-04-25 ?ы솗??`Pakuri/ProjectSettings/ProjectVersion.txt` 異쒕젰: `m_EditorVersionWithRevision: 6000.3.14f1 (d68c3f99a318)`
- `Pakuri/Packages/manifest.json`?먮뒗 `com.coplaydev.unity-mcp` ?섏〈?깆씠 ?덈떎.
- `codex mcp get unityMCP` 異쒕젰: `enabled: true`, `transport: stdio`, `command: uvx`, `args: --from mcpforunityserver mcp-for-unity --transport stdio`
- Unity MCP ?쒕쾭 `debug_request_context` 異쒕젰: server version `9.6.6`, `active_instance: null`, `all_keys_in_store: []`
- `manage_scene get_active` 異쒕젰: `No Unity Editor instances found. Please ensure Unity is running with MCP for Unity bridge.`
- `%USERPROFILE%\.unity-mcp` status directory??議댁옱?섏? ?딆븯??
- `Test-NetConnection 127.0.0.1:6400`? TCP ?곌껐 ?ㅽ뙣濡?timeout ?먮떎.
- `StdioBridgeHost.cs`?먮뒗 `[InitializeOnLoad]`, `StartAutoConnect()`, `WriteHeartbeat()`, `%USERPROFILE%\.unity-mcp\unity-mcp-status-<hash>.json` ?묒꽦 肄붾뱶媛 ?덈떎.
- `McpCiBoot.cs`??`EditorPrefs.SetBool(EditorPrefKeys.UseHttpTransport, false)` ??`StdioBridgeHost.StartAutoConnect()`瑜??몄텧?쒕떎.
- `README.md` Quick start??`Window > MCP for Unity`, `Auto-Setup`, ?꾩슂 ??`Start Bridge`瑜??덈궡?쒕떎.
- ?ъ슜??議곗옉 ??`%USERPROFILE%\.unity-mcp\unity-mcp-status-c88ab184.json`???앹꽦?먭퀬 ?댁슜? `unity_port: 6400`, `reason: ready`, `project_name: Pakuri`, `unity_version: 6000.3.4f1`???
- ?ъ슜??議곗옉 ??Unity MCP ?쒕쾭 `debug_request_context` 異쒕젰? `active_instance: Pakuri@c88ab184`???
- ?ъ슜??議곗옉 ??`manage_scene get_active` 異쒕젰? `SampleScene`, `Assets/Scenes/SampleScene.unity`, `rootCount: 2`???
- `read_console` 異쒕젰?먮뒗 `Transport changed to: Stdio`, `StdioBridgeHost started on port 6400. (OS=WindowsEditor, server=9.6.6)`, `SkillSync complete: Added: 3, Updated: 0, Deleted: 0 (C:\Users\t3312\.codex\skills\unity-mcp-skill)`媛 ?덉뿀??
- `manage_asset search`??`Assets`?먯꽌 珥?11媛??먯뀑??李얠븯??
- `manage_scene get_hierarchy`??猷⑦듃 ?ㅻ툕?앺듃 `Main Camera`, `Global Light 2D`瑜?諛섑솚?덈떎.
- `run_tests EditMode`??job `bee66234eeec4e67b238bafff3d63dc9`瑜??쒖옉?덇퀬 `get_test_job` 寃곌낵??`status: succeeded`, `resultState: Passed`, `total: 0`, `passed: 0`, `failed: 0`, `skipped: 0`???
- 2026-04-25 ?ы솗??Unity MCP ?쒕쾭 `debug_request_context` 異쒕젰? `active_instance: Pakuri@0c8eeeb5`???

### History

- 2026-04-23: Unity ?꾨줈?앺듃 援ъ“, MCP ?⑦궎吏 ?ㅼ튂, Codex CLI MCP ?깅줉 ?곹깭瑜??뺤씤?덈떎.
- 2026-04-23: Unity MCP ?쒕쾭???ㅽ뻾 以묒씠??Unity Editor bridge ?몄뒪?댁뒪媛 ?깅줉?섏? ?딆븯?뚯쓣 ?뺤씤?덈떎.
- 2026-04-23: Unity Editor ?대? MCP For Unity ?ㅼ젙/bridge ?쒖옉???꾩슂?섎떎怨??먮떒?덈떎.
- 2026-04-23: ?ъ슜?먭? Unity Editor?먯꽌 Transport瑜?`Stdio`濡?諛붽씀怨?`Session Active`, Codex client `Configuration`???섑뻾?덈떎.
- 2026-04-23: Unity MCP bridge ?곌껐, scene/asset/console/hierarchy ?묎렐, EditMode Test Runner ?ㅽ뻾??寃利앺뻽??
- 2026-04-25: ?ъ슜???덈궡 ??`Pakuri/ProjectSettings/ProjectVersion.txt`瑜??ㅼ떆 ?뺤씤??Unity 踰꾩쟾??`6000.3.14f1`濡??щ씪媛?寃껋쓣 湲곕줉?덇퀬, `debug_request_context`濡??꾩옱 MCP ?쒖꽦 ?몄뒪?댁뒪媛 `Pakuri@0c8eeeb5`???먯쓣 ?ы솗?명뻽??

## Task: Combat Automation Responsibility Guide

### Task title

湲곗큹 ?꾪닾 ?쒖뒪??援ы쁽 ???먮룞??媛??踰붿쐞? ?ъ슜???섎룞 ?묒뾽 踰붿쐞 ?뺣━ HTML ?묒꽦

### Goals

- `reference/current-architecture-plan.html` 湲곗??쇰줈 湲곗큹 ?꾪닾 ?쒖뒪??援ы쁽 李⑹닔 ????븷 遺꾨떞???뺣━?쒕떎.
- ?꾩옱 Unity ?꾨줈?앺듃 援ъ“? MCP ?곌껐 ?곹깭瑜?洹쇨굅濡??대뜑 ?앹꽦, ?ㅽ겕由쏀듃 ?앹꽦, ??諛곗튂 ?먮룞??媛??踰붿쐞瑜?援щ텇?쒕떎.
- ?ъ슜?먭? 吏곸젒 ?댁빞 ?섎뒗 ?묒뾽怨??쒓? ?먮룞?쇰줈 ?????덈뒗 ?묒뾽??HTML 臾몄꽌 ???μ쑝濡??뺣━?쒕떎.

### Constraints

- ?ㅼ젣 ?뚯씪, ?ㅼ젣 ???곹깭, ?ㅼ젣 MCP ?몄텧 寃곌낵??洹쇨굅???뺣━?쒕떎.
- 援ы쁽?섏? ?딆? ?먮룞???λ젰??援ы쁽??寃껋쿂???곸? ?딅뒗??
- ???묒뾽? ?ㅺ퀎 臾몄꽌 ?묒꽦?대ŉ, ?꾪닾 ?쒖뒪??肄붾뱶 援ы쁽 ?먯껜???ы븿?섏? ?딅뒗??

### Role Owner

Designer

### Status

Completed

### Next Actions

- ?ъ슜?먭? ?먰븯硫???臾몄꽌瑜?湲곗??쇰줈 Designer handoff瑜??묒꽦?쒕떎.
- ?ъ슜?먭? 紐낆떆?곸쑝濡?援ы쁽??吏?쒗븯硫?Code Builder ?④퀎濡??꾪솚???대뜑, ?ㅽ겕由쏀듃, ???ㅻ툕?앺듃 ?앹꽦???ㅼ젣濡??섑뻾?쒕떎.

### Evidence

- `Pakuri/reference/current-architecture-plan.html` ?뚯씪??議댁옱?섎ŉ ?꾪닾 ?쒖뒪???쒖옉 援ъ“瑜??ㅻ챸?쒕떎.
- `manage_asset search` 寃곌낵 `Assets`?먮뒗 `Scenes`, `Settings`? 湲곕낯 URP/InputSystem ?먯궛留??덇퀬 `Assets/Scripts` ?대뜑???녿떎.
- `Get-ChildItem Pakuri\\Assets` 異쒕젰?먮룄 `Scenes`, `Settings` ??寃뚯엫 ?꾩슜 ?대뜑媛 ?녿떎.
- `manage_scene get_hierarchy` 寃곌낵 ?꾩옱 `SampleScene` 猷⑦듃 ?ㅻ툕?앺듃??`Main Camera`, `Global Light 2D`肉먯씠??
- Unity MCP `debug_request_context` 寃곌낵 ?쒖꽦 ?몄뒪?댁뒪??`Pakuri@c88ab184`??
- 媛숈? ?몄뀡?먯꽌 `manage_scene get_active`, `manage_scene get_hierarchy`, `run_tests EditMode`媛 ?깃났???꾩옱 ?먮룞???곌껐???댁븘 ?덉쓬???뺤씤?덈떎.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `reference/current-architecture-plan.html`瑜??ㅼ떆 ?쎌뿀??
- 2026-04-24: `manage_asset search`, `Get-ChildItem Pakuri\\Assets`, `manage_scene get_hierarchy`濡??꾩옱 ?꾨줈?앺듃 援ъ“? ???곹깭瑜??ы솗?명뻽??
- 2026-04-24: ?먮룞??媛??踰붿쐞? ?ъ슜???섎룞 ?묒뾽 踰붿쐞瑜??뺣━??HTML 臾몄꽌瑜?`Pakuri/reference`??異붽??덈떎.

