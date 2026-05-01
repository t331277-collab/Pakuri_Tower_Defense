# AUTOMATION_GUIDE

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Task: Hierarchical Board Migration And Routing Rule Update

### Task title

Move persistent-state routing from always reading `BLACKBOARD.md` to `AGENTS.md` + `MDTREE.md` + domain boards.

### Goals

- Reduce token use by routing to relevant board files instead of reading the full root board.
- Preserve the previous full `BLACKBOARD.md` history in an archive.
- Require simultaneous updates to every related board file when a task crosses domains.
- Record that Code Reviewer execution needs user permission and Unity-MCP Play Mode gameplay verification is user-owned.

### Constraints

- Role Owner is Code Builder for this migration.
- Preserve old detailed task history.
- Do not run Code Reviewer without user permission.
- Do not run Unity-MCP Play Mode gameplay verification.

### Role Owner

Code Builder

### Status

Implemented pending validation.

### Next Actions

- Validate file existence and routing references.
- Use this rule set for future task routing.

### Evidence

- `AGENTS.md` now says to read `AGENTS.md` and `MDTREE.md` first.
- `MDTREE.md` defines MON, COMBAT, RUN, UI, DATA, OPS, and REPORT routing.
- `BLACKBOARD.md` is now a root index.
- The previous full root board was archived at `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`.

### History

- 2026-04-30: User requested hierarchical board routing and simultaneous related board updates to avoid drift.

## Migrated Task Blocks

## Task: Token Optimized Board Routing Report

### Task title

Document the current token-optimized board routing workflow.

### Goals

- Record that the routing/report explanation was created for the AGENTS/MDTREE/boards workflow.
- Keep automation guidance aligned with the new method: read `AGENTS.md`, route through `MDTREE.md`, then read only relevant boards.
- Preserve the rule that Code Reviewer execution requires explicit user permission.

### Constraints

- Role Owner is Code Builder for this saved report task.
- Do not claim gameplay validation.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Completed pending user review.

### Next Actions

- Continue using `MDTREE.md` as the routing entry point for future automation and documentation tasks.

### Evidence

- `AGENTS.md` now defines `BLACKBOARD.md` as a root index and sends detailed state to `boards/`.
- `MDTREE.md` provides the routing table for domain board reads.
- `BLACKBOARD.md` points to `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`.
- Added report: `Pakuri/reference/Report/2026-04-30-token-optimized-board-routing.html`.

### History

- 2026-04-30: User requested a saved HTML explanation of token optimization changes to `AGENTS.md`, `BLACKBOARD.md`, and the work method.

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

## Task: Token Efficient Reviewer Wrapper

### Task title

Reduce unnecessary token use in the external Builder -> Reviewer wrapper while preserving evidence-based review.

### Goals

- Stop wrapper prompts from encouraging full `BLACKBOARD.md` dumps.
- Keep `AGENTS.md` full-read behavior and preserve related `BLACKBOARD.md` block checks.
- Provide Reviewer with direct changed-file evidence so it can review changed lines without broad repeated exploration.
- Create an HTML report explaining the before/after problem and solution.

### Constraints

- Role Owner is Code Builder.
- All claims must be grounded in actual files and command output.
- Because this modifies the external reviewer wrapper logic, Code Reviewer review is required after Builder implementation.

### Role Owner

Code Builder

### Status

Builder implementation, local validation, Reviewer feedback fixes, and external Code Reviewer PASS completed.

### Next Actions

- On the next actual wrapper run, compare new `*.console.txt` `tokens used` values against the prior 59k-83k token smoke-test logs.

### Evidence

- `codex_builder_reviewer.ps1` now adds `Get-BlackboardIndexText`, `Limit-Text`, `Get-ChangedPathList`, `Get-GitDiffText`, and `Get-AddedFileEvidenceText`.
- The wrapper now writes `blackboard_index.txt`, `loop_XX_git_diff.patch`, and `loop_XX_changed_file_evidence.txt` for each loop.
- `loop_XX_git_diff.patch` is git diff evidence for tracked changes; `loop_XX_changed_file_evidence.txt` is the fallback content evidence for existing changed files including untracked additions.
- Builder and Reviewer prompts now instruct agents to read `AGENTS.md` in full but use `BLACKBOARD.md` through the generated index and related task blocks instead of printing the full file.
- Reviewer prompts now include git diff evidence and changed file content evidence excerpts.
- Added `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- PowerShell parser validation for `codex_builder_reviewer.ps1` returned `PARSE_OK`.
- `git status --short` after Builder implementation showed `M codex_builder_reviewer.ps1`, `M BLACKBOARD.md`, and untracked `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- External Code Reviewer final rerun returned `REVIEW_RESULT: PASS` in `codex_loop_logs/token_wrapper_reviewer_20260428_rerun2.md`.
- `AGENTS.md` now says Reviewer runs once only, then reports issues to the user instead of continuing an automatic fix loop.
- `AGENTS.md` now says Codex does not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification, while Codex records build/compile/console/editor-state evidence only.

### History

- 2026-04-28: User asked to change the workflow so token use is reduced without weakening evidence-based hallucination prevention, and to create an HTML before/after report.
- 2026-04-28: Code Builder changed the wrapper to create targeted BLACKBOARD and changed-file evidence, then created the HTML report.
- 2026-04-28: External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES` because the HTML report overstated `loop_XX_git_diff.patch` as full changed diff evidence for untracked added files.
- 2026-04-28: Code Builder corrected the HTML report and BLACKBOARD wording to distinguish tracked git diff evidence from changed file content evidence.
- 2026-04-28: External Code Reviewer rerun still found one remaining HTML sentence that overstated full diff patch evidence; Code Builder corrected that sentence.
- 2026-04-28: External Code Reviewer final rerun returned `REVIEW_RESULT: PASS`.
- 2026-04-28: User requested a simple `AGENTS.md` policy update for one Reviewer run only and user-owned Unity-MCP Play Mode verification; Code Builder added the wording to `AGENTS.md` and the HTML report.

