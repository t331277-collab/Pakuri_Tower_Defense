# BLACKBOARD.md

## Task: Reviewer Wrapper Smoke Test 2026-04-25 21:40

### Task title

Smoke test after reviewer wrapper fix

### Goals

- Confirm Code Builder can inspect `AGENTS.md` and `BLACKBOARD.md`.
- Confirm no project code changes are needed for this smoke test.
- Leave loop history/evidence for the external Reviewer phase.

### Constraints

- Do not modify project files except wrapper-managed logs and `BLACKBOARD.md` loop history.
- Base claims on actual files and command output.
- External wrapper will run Code Reviewer next.

### Role Owner

Code Builder

### Status

Builder phase completed. No project code changes were needed.

### Next Actions

- External wrapper should run Code Reviewer phase.
- Code Reviewer should verify this Builder result and end with `REVIEW_RESULT: PASS` if no issue is found.

### Evidence

- 2026-04-25 21:40:30 +09:00 `Get-Location` output: `C:\TowerDefence_Pakuri\Test`.
- `AGENTS.md` was read with `Get-Content -Raw -LiteralPath AGENTS.md`.
- `BLACKBOARD.md` was read with `Get-Content -Raw -LiteralPath BLACKBOARD.md`.
- `git rev-parse --is-inside-work-tree` output: `true`.
- `git status --short` output before this entry included existing changes: `M BLACKBOARD.md`, `M codex_builder_reviewer.ps1`, `M run_codex.bat`, and untracked `codex_loop_logs/...` entries.
- Latest wrapper log directory inspection found `codex_loop_logs\20260425_213901` containing `task.txt` and `loop_01_builder.md.console.txt`.
- No Unity/project source, scene, asset, reference, or wrapper script file was modified by this Builder phase.

### History

- 2026-04-25 21:40:30 +09:00: Builder inspected required files and command outputs, determined the smoke test requires no code changes, and recorded this loop history for Reviewer verification.

## 운영 규칙

이 파일은 프롬프트 초기화, 세션 재시작, 재부팅 후에도 작업을 이어가기 위한 지속 상태 파일이다.

새 작업이 시작되면 관련 작업 블록을 먼저 읽고 이어서 작업한다. 작업 블록은 작업이 완료되었거나 사용자가 명시적으로 삭제를 요청했을 때만 제거한다.

각 작업 블록에는 최소한 다음 항목을 유지한다.
- Task title
- Goals
- Constraints
- Role Owner
- Status
- Next Actions
- Evidence
- History

별도 저장소가 더 효율적이라고 판단되면 바로 바꾸지 말고 대안, 트레이드오프, 판단 기준을 먼저 보고한다.

## Task: Codex CLI Bootstrap

### Task title

Codex CLI 부트스트랩 및 Builder -> Reviewer 외부 강제 흐름 구성

### Goals

- `run_codex.bat`가 파일 위치를 루트로 잡고 UTF-8 콘솔에서 Codex CLI를 시작하게 한다.
- `codex_prompt.txt`를 UTF-8로 읽어 시작 프롬프트로 전달하게 한다.
- `AGENTS.md`에 근거 기반 작업 규칙과 Designer, Code Builder, Code Reviewer 롤을 정의한다.
- Builder 단계 직후 Reviewer 단계가 자동 실행되는 실제 외부 강제 흐름을 제공한다.
- 프롬프트 초기화나 재부팅 뒤에도 작업 상태를 이어갈 수 있게 한다.

### Constraints

- 모든 설명과 작업 판단은 실제 파일, 코드, 명령 출력 근거를 기준으로 한다.
- 구현되지 않은 것을 구현된 것처럼 말하지 않는다.
- 저장소에 없는 파일이나 구조는 먼저 확인하고, 없으면 없다고 말한다.
- `bat`, `txt`, `md` 파일은 UTF-8로 저장한다.
- Codex CLI 기본 실행 경로는 `%APPDATA%\npm\codex.cmd`다.
- Builder -> Reviewer 루프는 최대 3회만 허용한다.
- Git 저장소가 아닐 수 있으므로 Git 의존 흐름을 기본 전제로 삼지 않는다.

### Role Owner

Code Builder

### Status

Completed for bootstrap file creation, path correction, and Codex CLI path resolver hardening. No downstream Builder task has been run through the loop yet.

### Next Actions

- 일반 대화형 시작은 `run_codex.bat`를 실행한다.
- Builder -> Reviewer 강제 루프가 필요한 작업은 `powershell -NoProfile -ExecutionPolicy Bypass -File .\codex_builder_reviewer.ps1 -Task "작업 내용"` 형식으로 실행한다.
- 실제 Builder 작업을 래퍼로 실행하면 `codex_loop_logs`와 `BLACKBOARD.md`의 loop 기록을 확인한다.

### Evidence

- `Get-Location` 출력: `C:\TowerDefence_Pakuri\Test`
- 최초 `Get-ChildItem -Force` 출력에는 `.git`, `.gitignore`만 있었다.
- `run_codex.bat`, `codex_prompt.txt`, `AGENTS.md`, `BLACKBOARD.md`는 최초 확인 시 존재하지 않았다.
- `Get-Command codex` 출력의 실제 경로: `c:\Users\t3312\.vscode\extensions\openai.chatgpt-26.415.20818-win32-x64\bin\windows-x86_64\codex.exe`
- `codex --version` 출력: `codex-cli 0.122.0-alpha.1`
- `Test-Path (Join-Path $env:APPDATA 'npm\codex.cmd')` 출력: `False`
- `Join-Path $env:APPDATA 'npm\codex.cmd'` 출력: `C:\Users\t3312\AppData\Roaming\npm\codex.cmd`
- `codex --help` 출력에는 `exec`, `review`, `login`, `logout`, `mcp`, `marketplace`, `mcp-server`, `app-server`, `completion`, `sandbox`, `debug`, `apply`, `resume`, `fork`, `cloud`, `exec-server`, `features`, `help` 명령이 있었다.
- `codex --help`, `codex review --help`, `codex exec --help`, `codex debug --help`, `codex mcp --help` 출력에서 Claude Hooks와 같은 hook/event 명령은 확인되지 않았다.
- `codex review --help` 출력에는 `--uncommitted`, `--base`, `--commit` 옵션이 있었다.
- `codex exec --help` 출력에는 `--skip-git-repo-check`, `-C`, `--full-auto`, `-o` 옵션이 있었다.
- `git rev-parse --is-inside-work-tree` 출력: `true`
- 승인 후 `%APPDATA%\npm\codex.cmd` 래퍼를 생성했다.
- 승인된 검증에서 `Test-Path (Join-Path $env:APPDATA 'npm\codex.cmd')` 출력: `True`
- 승인된 검증에서 `%APPDATA%\npm\codex.cmd` 내용은 감지된 `codex.exe`를 호출했다.
- 승인된 검증에서 `& (Join-Path $env:APPDATA 'npm\codex.cmd') --version` 출력: `codex-cli 0.122.0-alpha.1`
- `cmd /d /c "call run_codex.bat < NUL"`은 `codex.cmd` 생성 전 오류 경로를 검증했고, `Required default path: C:\Users\t3312\AppData\Roaming\npm\codex.cmd`를 출력했다.
- `codex_builder_reviewer.ps1`는 PowerShell syntax check를 통과했다.
- 2026-04-23 `C:\Users\t3312\AppData\Roaming\npm\codex.cmd` 내용은 삭제된 VS Code 확장 경로 `openai.chatgpt-26.415.20818-win32-x64\bin\windows-x86_64\codex.exe`를 가리키고 있었다.
- 2026-04-23 실제 존재하는 Codex CLI 경로는 `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`였고 `codex-cli 0.122.0-alpha.13`을 출력했다.
- 2026-04-23 `run_codex.bat`는 `%APPDATA%\npm\codex.cmd`가 실행 가능하지 않으면 VS Code 확장 폴더의 최신 `codex.exe`를 탐색하도록 수정했다.
- 2026-04-23 `codex_builder_reviewer.ps1`도 동일하게 Codex CLI 경로를 해석하도록 `Resolve-CodexCommand`를 추가했다.
- 2026-04-23 수정 후 Codex CLI 경로 탐색은 `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`를 찾았고 `codex-cli 0.122.0-alpha.13`을 출력했다.
- 2026-04-23 승인 후 `%APPDATA%\npm\codex.cmd` 래퍼를 현재 존재하는 `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe` 경로로 갱신했고 `codex-cli 0.122.0-alpha.13`을 출력했다.
- 2026-04-23 수정 후 `codex_builder_reviewer.ps1`는 PowerShell parser syntax check를 통과했다.
- 2026-04-23 Code Reviewer 외부 검토 로그 `codex_loop_logs\manual_reviewer_20260423_212033.md`는 `REVIEW_RESULT: PASS`를 반환했다.
- 2026-04-25 sandbox 내부 직접 `codex exec` smoke test는 `액세스가 거부되었습니다. (os error 5)`로 실패했다.
- 2026-04-25 승인된 외부 실행으로 최신 Codex CLI `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.422.30944-win32-x64\bin\windows-x86_64\codex.exe` reviewer smoke test가 `REVIEW_RESULT: PASS`를 반환했다.
- 2026-04-25 `codex_builder_reviewer.ps1`의 `Invoke-CodexExec`가 Codex 콘솔 출력을 반환값으로 섞어 `$builderExit`를 문자열로 만드는 문제를 확인했다.
- 2026-04-25 `Invoke-CodexExec`가 콘솔 출력을 `*.console.txt`로 저장하고 정수 종료 코드만 반환하도록 수정했다.
- 2026-04-25 Codex CLI stderr 배너가 `$ErrorActionPreference = 'Stop'`에서 `NativeCommandError`를 일으켜, `Invoke-CodexExec` 내부에서만 native stderr 처리를 `Continue`로 완화했다.
- 2026-04-25 수정 후 `codex_builder_reviewer.ps1`는 PowerShell parser syntax check에서 `PARSE_OK`를 반환했다.
- 2026-04-25 수정 후 smoke test 래퍼 실행은 `Reviewer PASS at loop 1.`을 반환했고, `codex_loop_logs\20260425_213006\loop_01_reviewer.md`는 `REVIEW_RESULT: PASS`를 포함한다.
- 2026-04-25 Code Reviewer 직접 검토 `codex_loop_logs\reviewer_restore_fix_review.md`는 `run_codex.bat`의 프롬프트 quote 변형, `BLACKBOARD.md`의 잘못된 history 위치, pre-fix 손상 exit code 기록을 지적하며 `REVIEW_RESULT: NEEDS_CHANGES`를 반환했다.
- 2026-04-25 `run_codex.bat`는 `codex_prompt.txt` UTF-8 내용을 변형 없이 전달하도록 `.Replace([string][char]34, [string][char]0x201D)`를 제거했다.
- 2026-04-25 `Add-BlackboardHistory`는 루프 기록을 파일 끝이 아니라 `Codex CLI Bootstrap` 작업의 `Builder Reviewer Loop` 섹션 앞에 삽입하도록 수정했다.
- 2026-04-25 잘못 붙었던 Eve 작업 하단의 wrapper smoke-test history 기록을 제거했다.
- 2026-04-25 최종 smoke test 래퍼 실행은 `Reviewer PASS at loop 1.`을 반환했고, `codex_loop_logs\20260425_213901\loop_01_reviewer.md`는 `REVIEW_RESULT: PASS`를 포함한다.

### History

- 2026-04-19: 작업 폴더와 대상 파일 존재 여부를 확인했다.
- 2026-04-19: Codex CLI 실제 경로, 버전, `exec`, `review` 도움말을 확인했다.
- 2026-04-19: `%APPDATA%\npm\codex.cmd`가 현재 존재하지 않는다는 점을 확인했다.
- 2026-04-19: 네이티브 hook/event가 도움말 출력에서 확인되지 않아 외부 PowerShell 래퍼 방식으로 설계했다.
- 2026-04-19: `run_codex.bat`, `codex_prompt.txt`, `AGENTS.md`, `BLACKBOARD.md`, `codex_builder_reviewer.ps1`를 생성했다.
- 2026-04-19: 승인 후 `%APPDATA%\npm\codex.cmd` 래퍼를 생성하고 `--version` 실행으로 검증했다.
- 2026-04-23: VS Code 확장 업데이트로 `%APPDATA%\npm\codex.cmd`가 가리키는 고정 버전 경로가 깨진 문제를 확인했다.
- 2026-04-23: `run_codex.bat`와 `codex_builder_reviewer.ps1`를 고정 래퍼 의존에서 실행 가능한 래퍼 우선, 실패 시 최신 VS Code 확장 `codex.exe` 탐색 방식으로 수정했다.
- 2026-04-23: 승인 후 `%APPDATA%\npm\codex.cmd` 외부 래퍼 자체도 현재 존재하는 Codex CLI 실행 파일로 갱신했다.
- 2026-04-23: `codex_loop_logs\manual_reviewer_20260423_212033.md`에 Code Reviewer 통과 판정을 기록했다.
- 2026-04-25: Code Reviewer 강제 흐름 중단 원인이 Codex CLI 실행 실패와 래퍼의 종료 코드 반환 처리 오류임을 확인하고 `codex_builder_reviewer.ps1`를 수정했다.
- 2026-04-25: 수정 후 Builder -> Reviewer smoke test를 실행해 `codex_loop_logs\20260425_213006\loop_01_reviewer.md`에서 `REVIEW_RESULT: PASS`를 확인했다.
- 2026-04-25: Code Reviewer가 지적한 `run_codex.bat` 프롬프트 변형과 `BLACKBOARD.md` 기록 위치 문제를 수정한 뒤 `codex_loop_logs\20260425_213901\loop_01_reviewer.md`에서 `REVIEW_RESULT: PASS`를 확인했다.

- 2026-04-25 21:39:01 +09:00: Builder -> Reviewer loop started. Run directory: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901
- 2026-04-25 21:39:27 +09:00: Loop 1 Builder started. Output: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901\loop_01_builder.md
- 2026-04-25 21:41:53 +09:00: Loop 1 Builder finished with exit code 0.
- 2026-04-25 21:42:22 +09:00: Loop 1 Reviewer started. Output: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901\loop_01_reviewer.md
- 2026-04-25 21:44:07 +09:00: Loop 1 Reviewer finished with exit code 0.
- 2026-04-25 21:44:07 +09:00: Loop 1 Reviewer decision: PASS. Builder -> Reviewer loop completed.
### Builder Reviewer Loop

- Enforcement method: External wrapper script
- Wrapper file: `codex_builder_reviewer.ps1`
- Git dependency: Not required
- Max loops: 3
- Current loop count: 1 in latest smoke test
- Last reviewer decision: PASS for wrapper log `codex_loop_logs\20260425_213901\loop_01_reviewer.md`
- Last log directory: `codex_loop_logs\20260425_213901`

## Task: Unity MCP Bridge Connection

### Task title

Unity MCP bridge 연결 및 등록 확인

### Goals

- 현재 워크스페이스의 Unity 프로젝트 `Pakuri`에서 Unity MCP bridge를 Codex MCP 서버와 연결한다.
- Codex CLI 쪽 MCP 등록 상태와 Unity Editor 쪽 bridge 실행 상태를 실제 명령 출력으로 구분한다.
- 사용자가 Unity Editor 내 MCP For Unity 설정을 직접 조작해야 하는 경우, 필요한 항목을 명확히 질문한다.

### Constraints

- 모든 판단은 실제 파일, 패키지 코드, 명령 출력에 근거한다.
- Unity 프로젝트 파일은 사용자 요청 없이 수정하지 않는다.
- Unity Editor 내부 bridge 시작은 실제 연결 확인 전까지 완료된 것으로 말하지 않는다.

### Role Owner

Code Builder

### Status

Completed. Unity Editor-side MCP For Unity bridge is connected to the current Codex MCP server.

### Next Actions

- 이후 Unity MCP가 끊기면 Unity Editor에서 Transport를 `Stdio`로 두고 `Session Active`를 다시 켠 뒤 `manage_scene get_active`로 재검증한다.
- Unity Test Runner 확인은 `run_tests EditMode` 후 `get_test_job`으로 결과를 확인한다.

### Evidence

- `Pakuri/ProjectSettings/ProjectVersion.txt` 출력: `m_EditorVersion: 6000.3.4f1`
- 2026-04-25 재확인 `Pakuri/ProjectSettings/ProjectVersion.txt` 출력: `m_EditorVersion: 6000.3.14f1`
- 2026-04-25 재확인 `Pakuri/ProjectSettings/ProjectVersion.txt` 출력: `m_EditorVersionWithRevision: 6000.3.14f1 (d68c3f99a318)`
- `Pakuri/Packages/manifest.json`에는 `com.coplaydev.unity-mcp` 의존성이 있다.
- `codex mcp get unityMCP` 출력: `enabled: true`, `transport: stdio`, `command: uvx`, `args: --from mcpforunityserver mcp-for-unity --transport stdio`
- Unity MCP 서버 `debug_request_context` 출력: server version `9.6.6`, `active_instance: null`, `all_keys_in_store: []`
- `manage_scene get_active` 출력: `No Unity Editor instances found. Please ensure Unity is running with MCP for Unity bridge.`
- `%USERPROFILE%\.unity-mcp` status directory는 존재하지 않았다.
- `Test-NetConnection 127.0.0.1:6400`은 TCP 연결 실패로 timeout 됐다.
- `StdioBridgeHost.cs`에는 `[InitializeOnLoad]`, `StartAutoConnect()`, `WriteHeartbeat()`, `%USERPROFILE%\.unity-mcp\unity-mcp-status-<hash>.json` 작성 코드가 있다.
- `McpCiBoot.cs`는 `EditorPrefs.SetBool(EditorPrefKeys.UseHttpTransport, false)` 후 `StdioBridgeHost.StartAutoConnect()`를 호출한다.
- `README.md` Quick start는 `Window > MCP for Unity`, `Auto-Setup`, 필요 시 `Start Bridge`를 안내한다.
- 사용자 조작 후 `%USERPROFILE%\.unity-mcp\unity-mcp-status-c88ab184.json`이 생성됐고 내용은 `unity_port: 6400`, `reason: ready`, `project_name: Pakuri`, `unity_version: 6000.3.4f1`였다.
- 사용자 조작 후 Unity MCP 서버 `debug_request_context` 출력은 `active_instance: Pakuri@c88ab184`였다.
- 사용자 조작 후 `manage_scene get_active` 출력은 `SampleScene`, `Assets/Scenes/SampleScene.unity`, `rootCount: 2`였다.
- `read_console` 출력에는 `Transport changed to: Stdio`, `StdioBridgeHost started on port 6400. (OS=WindowsEditor, server=9.6.6)`, `SkillSync complete: Added: 3, Updated: 0, Deleted: 0 (C:\Users\t3312\.codex\skills\unity-mcp-skill)`가 있었다.
- `manage_asset search`는 `Assets`에서 총 11개 에셋을 찾았다.
- `manage_scene get_hierarchy`는 루트 오브젝트 `Main Camera`, `Global Light 2D`를 반환했다.
- `run_tests EditMode`는 job `bee66234eeec4e67b238bafff3d63dc9`를 시작했고 `get_test_job` 결과는 `status: succeeded`, `resultState: Passed`, `total: 0`, `passed: 0`, `failed: 0`, `skipped: 0`였다.
- 2026-04-25 재확인 Unity MCP 서버 `debug_request_context` 출력은 `active_instance: Pakuri@0c8eeeb5`였다.

### History

- 2026-04-23: Unity 프로젝트 구조, MCP 패키지 설치, Codex CLI MCP 등록 상태를 확인했다.
- 2026-04-23: Unity MCP 서버는 실행 중이나 Unity Editor bridge 인스턴스가 등록되지 않았음을 확인했다.
- 2026-04-23: Unity Editor 내부 MCP For Unity 설정/bridge 시작이 필요하다고 판단했다.
- 2026-04-23: 사용자가 Unity Editor에서 Transport를 `Stdio`로 바꾸고 `Session Active`, Codex client `Configuration`을 수행했다.
- 2026-04-23: Unity MCP bridge 연결, scene/asset/console/hierarchy 접근, EditMode Test Runner 실행을 검증했다.
- 2026-04-25: 사용자 안내 후 `Pakuri/ProjectSettings/ProjectVersion.txt`를 다시 확인해 Unity 버전이 `6000.3.14f1`로 올라간 것을 기록했고, `debug_request_context`로 현재 MCP 활성 인스턴스가 `Pakuri@0c8eeeb5`인 점을 재확인했다.

## Task: Combat Automation Responsibility Guide

### Task title

기초 전투 시스템 구현 시 자동화 가능 범위와 사용자 수동 작업 범위 정리 HTML 작성

### Goals

- `reference/current-architecture-plan.html` 기준으로 기초 전투 시스템 구현 착수 시 역할 분담을 정리한다.
- 현재 Unity 프로젝트 구조와 MCP 연결 상태를 근거로 폴더 생성, 스크립트 생성, 씬 배치 자동화 가능 범위를 구분한다.
- 사용자가 직접 해야 하는 작업과 제가 자동으로 할 수 있는 작업을 HTML 문서 한 장으로 정리한다.

### Constraints

- 실제 파일, 실제 씬 상태, 실제 MCP 호출 결과에 근거해 정리한다.
- 구현되지 않은 자동화 능력을 구현된 것처럼 적지 않는다.
- 이 작업은 설계 문서 작성이며, 전투 시스템 코드 구현 자체는 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 문서를 기준으로 Designer handoff를 작성한다.
- 사용자가 명시적으로 구현을 지시하면 Code Builder 단계로 전환해 폴더, 스크립트, 씬 오브젝트 생성을 실제로 수행한다.

### Evidence

- `Pakuri/reference/current-architecture-plan.html` 파일이 존재하며 전투 시스템 시작 구조를 설명한다.
- `manage_asset search` 결과 `Assets`에는 `Scenes`, `Settings`와 기본 URP/InputSystem 자산만 있고 `Assets/Scripts` 폴더는 없다.
- `Get-ChildItem Pakuri\\Assets` 출력에도 `Scenes`, `Settings` 외 게임 전용 폴더가 없다.
- `manage_scene get_hierarchy` 결과 현재 `SampleScene` 루트 오브젝트는 `Main Camera`, `Global Light 2D`뿐이다.
- Unity MCP `debug_request_context` 결과 활성 인스턴스는 `Pakuri@c88ab184`다.
- 같은 세션에서 `manage_scene get_active`, `manage_scene get_hierarchy`, `run_tests EditMode`가 성공해 현재 자동화 연결이 살아 있음을 확인했다.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `reference/current-architecture-plan.html`를 다시 읽었다.
- 2026-04-24: `manage_asset search`, `Get-ChildItem Pakuri\\Assets`, `manage_scene get_hierarchy`로 현재 프로젝트 구조와 씬 상태를 재확인했다.
- 2026-04-24: 자동화 가능 범위와 사용자 수동 작업 범위를 정리한 HTML 문서를 `Pakuri/reference`에 추가했다.

## Task: Eve Initial Combat Preview

### Task title

`dungeon-squad-run-structure.md` 기준 이브 단독 초기 전투 완성 모습 HTML 작성

### Goals

- `reference/4.run/dungeon-squad-run-structure.md`를 기준으로 초기 전투 로직을 어떻게 이해했는지 시각적으로 검증 가능한 HTML 문서를 만든다.
- 앞서 제안한 vertical slice 방향을 유지한 채, 이브만 구현했을 때의 초기 완성 상태를 정리한다.
- 문서 기반 확정 사항과 초기 구현용 제안을 분리해서 표시한다.

### Constraints

- 실제 reference 문서에 있는 내용만 확정으로 적고, 제안은 제안으로 명확히 구분한다.
- 현재 Unity 프로젝트와 씬 상태를 근거로 “아직 없는 것”과 “구현 후 기대 모습”을 구분한다.
- 이 작업은 설계 검증용 HTML 작성이며, 전투 시스템 코드 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 확인 후 방향이 맞다고 판단하면, Designer handoff 문서로 구체적인 구현 순서를 내릴 수 있다.
- 사용자가 명시적으로 구현을 지시하면 Code Builder가 이 HTML의 구조를 기준으로 실제 폴더, 스크립트, 씬 오브젝트를 생성한다.

### Evidence

- `Pakuri/reference/4.run/dungeon-squad-run-structure.md`는 1일차 고정 전투, 전투 후 보상 확인, 포로 기반 선택, 다음 일차 이동 흐름을 정의한다.
- `Pakuri/reference/2.Monster/eve/eve-tower.md`는 이브를 번개/얼음 엔진형 보조 딜러로 정의하고, 첫 액티브로 `A. 아크 볼트`를 둔다.
- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`는 아크 볼트의 탄창 수 6, 재장전 4초, 발사 간격 0.35초, 번개 피해 계산식 `24 + 주문력 * 0.95`, 감전 15%를 정의한다.
- `Pakuri/reference/Scene/combat-scene-layout.md`는 테스트 전장 32x18, 넥서스 `(2,8)`, 적 우측 진입, 아군 배치 영역 `(4~10, 3~15)`를 정의한다.
- `Pakuri/reference/dungeon-squad-combat-player-controls.md`는 전투 중 플레이어 조작을 “공격 지점 지정”으로 정의한다.
- `Pakuri/reference/4.run/combat-reward-system.md`는 일반 전투 보상으로 포로 1~3명, 골드 10, 어둠의 흔적 10, 보스 포로 확정 포함을 정의한다.
- `Pakuri/reference/5.enemy/stage-1-enemies.md`는 1스테이지 일반몹 5종과 일반 전투 보스 강화 규칙을 정의한다.
- 현재 `manage_scene get_active` 결과는 `Assets/Scenes/SampleScene.unity`이며, `manage_scene get_hierarchy` 결과 씬 루트는 `Main Camera`, `Global Light 2D`뿐이다.
- 현재 `manage_asset search` 결과 `Assets`에는 기본 `Scenes`, `Settings`, URP/InputSystem 자산만 있고 게임 전용 스크립트 폴더는 없다.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-run-structure.md`, `eve-tower.md`, `current-architecture-plan.html`를 다시 읽었다.
- 2026-04-24: `a-arc-bolt.md`, `combat-scene-layout.md`, `combat-reward-system.md`, `dungeon-squad-combat-player-controls.md`, `combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `stage-1-enemies.md`를 추가로 읽었다.
- 2026-04-24: 현재 Unity 씬과 에셋 상태를 다시 조회한 뒤, 이브 단독 초기 전투 완성 모습을 설명하는 HTML 문서를 `Pakuri/reference`에 추가했다.

## Task: Eve Combat Vertical Slice Implementation

### Task title

이브 단독 초기 전투 vertical slice 실제 구현 및 작업 설명 HTML 작성

### Goals

- `eve-initial-combat-vertical-slice-preview.html` 기반으로 Unity 프로젝트에 실제 전투 프로토타입을 만든다.
- 현재 씬의 메인 카메라를 전장 기준으로 맞추고 `CombatRoot` 및 앵커 오브젝트를 생성한다.
- 적 스폰 X는 고정하고 Y는 랜덤으로 생성되게 한다.
- 구현 후 실제 검증 근거와 작업 설명을 HTML로 남긴다.

### Constraints

- 실제 reference 문서와 실제 Unity 씬 상태를 기준으로 구현한다.
- 현재 프로젝트에 없는 아트 자산은 추측하지 않고 placeholder 비주얼로 처리한다.
- 로직 작업 후 reviewer 검수를 시도하고, 외부 reviewer 실행이 실패하면 그 실패 근거를 남긴다.

### Role Owner

Code Builder

### Status

Completed with manual reviewer pass in-session. External Codex reviewer commands timed out and did not produce a new review artifact.

### Next Actions

- 사용자가 원하면 이 프로토타입 위에 실제 아트 자산, 정식 UI, 추가 적 타입, 보상 데이터 구조를 붙인다.
- reviewer 외부 강제 흐름을 이 작업에도 안정적으로 연결하려면 `codex review`/`codex exec` 타임아웃 원인을 별도 확인한다.

### Evidence

- `Assets/Scripts/Combat/DamageCalculator.cs`를 생성했다.
- `Assets/Scripts/Combat/EveVerticalSliceController.cs`를 생성했다.
- `manage_asset search path=Assets/Scripts` 결과 `Combat`, `DamageCalculator.cs`, `EveVerticalSliceController.cs`가 존재한다.
- `SampleScene.unity`에는 `CombatRoot`와 `Pakuri.Combat.EveVerticalSliceController` 컴포넌트가 저장됐다.
- `manage_scene get_hierarchy include_transform=true` 결과:
  - `Main Camera` 위치 `15.5, 8.5, -10`
  - `Nexus` 위치 `2, 8, 0`
  - `EveUnit` 위치 `6, 8, 0`
  - `EnemySpawnPoint` 위치 `29, 8, 0`
  - `InputTarget` 위치 `16, 8, 0`
- `SampleScene.unity` 텍스트 확인 결과 `orthographic: 1`, `orthographic size: 10`, `CombatRoot`, `EveVerticalSliceController`, 각 좌표가 저장되어 있다.
- 플레이 모드 런타임 검사 `execute_code` 결과:
  - 적 스폰 런타임 오브젝트 `Enemy_Normal_01`, `Enemy_Boss_01`가 생성됐다.
  - 이후 `battleResolved=True`, `victory=True`, `waitingForRewardChoice=True` 상태를 확인했다.
- 게임 화면 캡처 파일:
  - `Assets/Screenshots/screenshot-20260424-165841.png`
  - `Assets/Screenshots/screenshot-20260424-165958.png`
- `validate_script`는 `DamageCalculator.cs`에 대해 성공했고, `EveVerticalSliceController.cs`는 실제 파일 내용 중복이 없는데도 duplicate signature 오탐을 반환했다.
- `codex review --uncommitted`는 실행 경로 문제 후 실제 실행에서 timeout 됐다.
- reviewer 전용 `codex exec`도 300초 timeout으로 끝났고 새 review 로그 파일을 남기지 못했다.
- 현재 세션에서 `DamageCalculator.cs`, `EveVerticalSliceController.cs`, `SampleScene.unity`를 line-by-line 확인했고 추가 blocking issue는 찾지 못했다.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `eve-initial-combat-vertical-slice-preview.html`, 관련 reference 문서를 다시 읽었다.
- 2026-04-24: `Assets/Scripts`, `Assets/Scripts/Combat` 폴더를 생성했다.
- 2026-04-24: `DamageCalculator.cs`, `EveVerticalSliceController.cs`를 추가했다.
- 2026-04-24: `CombatRoot`를 만들고 `EveVerticalSliceController`를 붙였다.
- 2026-04-24: `Main Camera`를 전장 기준 위치와 orthographic 설정으로 맞췄다.
- 2026-04-24: `ExecuteAlways` 기반으로 `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`가 씬에 생성되도록 했다.
- 2026-04-24: 플레이 모드에서 적 스폰, 승리 상태, 보상 대기 상태를 확인했다.
- 2026-04-24: 외부 reviewer로 `codex review --uncommitted`, reviewer 전용 `codex exec`를 시도했으나 모두 timeout 됐다.
- 2026-04-24: 현재 세션에서 manual reviewer 검토를 수행하고 작업 설명 HTML을 추가했다.

## Task: Eve Projectile Click Hold Compliance Plan

### Task title

문서 준수형 아크 볼트 투사체 입력/적중 구조 수정 계획 HTML 작성

### Goals

- 현재 이브 전투 프로토타입을 기준으로, 아크 볼트를 문서 정의에 더 맞는 `투사체 / 탄창형` 구조로 바꾸는 작업 계획을 정리한다.
- 사용자가 요청한 `왼쪽 클릭 유지 시 연속 발사`, `투사체 적중 시 피해` 요구를 실제 코드와 reference 문서 차이 기준으로 설명한다.
- Code Builder가 바로 구현에 들어갈 수 있도록 수정 범위, 파일별 변경 계획, 검증 체크리스트를 HTML 한 장으로 남긴다.

### Constraints

- 실제 reference 문서와 실제 현재 코드에 근거해서만 적는다.
- 아직 없는 구현을 구현된 것처럼 적지 않는다.
- 이 작업은 설계 문서 작성이며, 코드 수정 자체는 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 문서를 기준으로 Code Builder 단계로 전환해 실제 투사체형 발사 로직을 구현한다.
- 구현 시 `EveVerticalSliceController.cs`의 즉시 피해 구조를 투사체 적중 구조로 바꾸고, hold 입력 검증과 reviewer 루프를 다시 수행한다.

### Evidence

- `Pakuri/reference/dungeon-squad-combat-player-controls.md`는 전투 중 플레이어 입력을 `공격 지점 지정`으로 정의한다.
- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`는 아크 볼트를 `투사체 / 탄창형`으로 정의하고, 투사체 속도 `15.0`, 탄창 `6`, 재장전 `4초`, 발사 간격 `0.35초`, 감전 `15%`를 명시한다.
- `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`는 같은 속성 방어력 참조와 방어력 반영 후 치명타 적용 규칙을 정의한다.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` 현재 구현은 `wasPressedThisFrame` / `GetMouseButtonDown(0)` 입력과 즉시 피해 구조를 사용한다.
- 새 설계 문서 `Pakuri/reference/eve-projectile-click-hold-plan.html`를 추가했다.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-combat-player-controls.md`, `a-arc-bolt.md`, `combat-attribute-and-damage-system.md`, `EveVerticalSliceController.cs`, `eve-combat-implementation-report.html`를 다시 읽었다.
- 2026-04-24: 현재 코드가 단발 클릭 입력과 즉시 피해 구조임을 확인했다.
- 2026-04-24: hold 입력 기반 연속 발사와 투사체 적중 기반 피해 처리로 옮기는 설계 HTML을 `Pakuri/reference/eve-projectile-click-hold-plan.html`에 추가했다.

## Task: Eve Projectile Click Implementation

### Task title

이브 아크 볼트를 클릭형 투사체 적중 구조로 수정하고 완료 보고 HTML 작성

### Goals

- 기존 즉시 피해 구조를 제거하고, 왼쪽 클릭 시에만 아크 볼트 투사체 1발이 생성되게 한다.
- 투사체가 실제로 이동하고 적과 닿을 때만 피해를 적용하게 한다.
- 수정 후 객체 역할, 동작 방식, 작업 중 문제, 타임스탬프 작업 로그를 포함한 완료 보고 HTML을 남긴다.

### Constraints

- 실제 현재 코드와 실제 Unity 런타임 검증을 근거로 작업한다.
- 적 스폰 축, 카메라, 전장 좌표는 기존 값을 유지한다.
- 로직 수정 후 reviewer 강제 흐름을 다시 시도하고, 실패 시 그 근거를 남긴다.

### Role Owner

Code Builder

### Status

Completed without Code Review. External reviewer commands timed out again, so only Builder-side validation was performed.

### Next Actions

- 사용자가 원하면 다음 단계로 실제 클릭 입력 기반 정식 플레이 테스트, 속성별 방어력 데이터 모델, Collider 기반 충돌로 확장한다.
- reviewer 외부 강제 흐름 timeout 원인을 별도 분리해서 해결해야 한다.
- 현재 상태는 Code Review 미수행 상태이므로, 이후 리뷰가 필요하면 별도 reviewer 단계를 다시 실행해야 한다.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`는 `ProjectileRuntime`, `projectileRoot`, `UpdateProjectiles()`, `TryHitEnemy()`, 클릭 기반 `HandlePointerInput()`를 포함하도록 수정됐다.
- `Pakuri/Assets/Scenes/SampleScene.unity`는 `ProjectileRoot`를 포함한 현재 전장 구조로 다시 저장됐다.
- `manage_scene save`가 `Assets/Scenes/SampleScene.unity` 저장 성공을 반환했다.
- `find_gameobjects by_name ProjectileRoot`는 씬에서 `ProjectileRoot`를 찾았다.
- 플레이 모드 통제 검증에서:
  - 발사 직후 `projectileCount = 1`
  - 1초 뒤 `projectileCount = 0`
  - 같은 검증에서 `enemyHealth = 37.95`
  - 최종 재검증에서 `currentShotsRemaining = 0`, `reloadRemaining = 4.0`
- 검증 캡처 `Pakuri/Assets/Screenshots/eve-projectile-click-runtime.png`를 생성했다.
- `validate_script`는 이번에도 duplicate signature false positive를 냈다.
- `read_console`에서는 `FindObjectOfType<Camera>()` obsolete warning이 나왔고 이후 `FindFirstObjectByType<Camera>()`로 수정했다.
- 외부 reviewer 시도:
  - `codex review --uncommitted` timeout
  - reviewer 전용 `codex exec` timeout

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `eve-projectile-click-hold-plan.html`, `a-arc-bolt.md`, `dungeon-squad-combat-player-controls.md`, 현재 `EveVerticalSliceController.cs`를 다시 읽었다.
- 2026-04-24: 즉시 피해 구조를 제거하고 클릭형 투사체 생성/이동/적중 구조로 `EveVerticalSliceController.cs`를 교체했다.
- 2026-04-24: `ProjectileRoot` 생성과 hierarchy 반영을 확인했다.
- 2026-04-24: 플레이 모드 통제 검증으로 투사체 적중 시 피해 적용을 확인했다.
- 2026-04-24: 수동 line review에서 마지막 탄 이후 자동 재장전 지연 문제를 찾아 `FireArcBolt()`에서 즉시 재장전 시작으로 수정했다.
- 2026-04-24: obsolete camera 탐색 경고를 `FindFirstObjectByType<Camera>()`로 수정했다.
- 2026-04-24: 작업 완료 보고서 `Pakuri/reference/eve-projectile-click-implementation-report.html`를 추가했다.
- 2026-04-24: 외부 reviewer로 `codex review --uncommitted`, reviewer 전용 `codex exec`를 다시 시도했으나 모두 timeout 됐다.

## Task: Monster Select Run UI Expansion Plan

### Task title

몬스터 선택 UI, Run 시작, 전투 후 스킬 강화 흐름 확장 설계 HTML 작성

### Goals

- 현재 구현된 이브 단독 전투 프로토타입을 기준으로, 몬스터 선택 UI와 Run 시작 흐름을 어떻게 일반화할지 정리한다.
- `2.Monster` 문서군과 `skill-choice-pool-rule.md`, `combat-reward-system.md`를 근거로 몬스터별 시작 스킬 A, 최대 액티브 3개, 최대 패시브 3개, 전투 후 강화 선택 흐름을 설계한다.
- 구현 전에 필요한 공통 시스템, UI 패널 구조, 열린 질문을 HTML 문서로 남긴다.

### Constraints

- 실제 현재 코드, 실제 씬 상태, 실제 reference 문서에 근거해서만 적는다.
- 구현되지 않은 UI/런 시스템을 이미 있는 것처럼 적지 않는다.
- 이 작업은 Designer 설계 문서 작성이며, 실제 코드 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 설계 문서를 기준으로 Designer handoff를 작성해 Code Builder 구현 범위를 고정한다.
- 사용자가 명시적으로 구현을 지시하면, 먼저 UI 뼈대와 RunSession 분리부터 들어가는 것이 안전하다.
- 1차 구현 범위는 문서가 완비된 `아리엘`, `이브`, `세인`, `베가` 4몬스터 우선으로 잡고, `린`은 더미 상태로 둔다.
- 린의 `g~j` 패시브 문서가 실제 저장소에 없으므로, 린을 플레이 가능 대상으로 올리는 작업은 후속 문서 보강 이후로 미룬다.

### Evidence

- `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`만 현재 게임 전용 스크립트로 존재한다.
- 현재 활성 씬은 `Assets/Scenes/SampleScene.unity`이며 루트 오브젝트는 `Main Camera`, `Global Light 2D`, `CombatRoot`다.
- `CombatRoot` 하위에는 `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`, `ProjectileRoot`가 있다.
- `Pakuri/Assets` 아래에서는 `NO_UI_TOOLKIT_ASSETS`, `NO_UI_NAMED_ASSETS`가 확인돼 별도 UI 자산이 없음을 재확인했다.
- `Pakuri/reference/2.Monster/monster-basic-rule.md`는 몬스터가 액티브 A를 기본 습득 상태로 시작하고, 런 중 액티브 최대 3개, 패시브 최대 3개를 가진다고 정의한다.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`는 신규 액티브, 신규 패시브, 액티브 특성, 마스터 스킬을 하나의 선택지 풀로 합쳐 3개를 제시하는 규칙을 정의한다.
- `Pakuri/reference/4.run/combat-reward-system.md`는 일반 전투/중간보스/보스 전투별 포로, 유물, 골드, 어둠의 흔적 보상 규칙을 정의한다.
- `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `eve/eve-tower.md`, `rin/rin-tower.md`, `sein/sein-tower.md`, `vega/vega-tower.md`로 현재 구현 대상 몬스터 5종을 확인했다.
- 사용자 응답으로 모든 몬스터는 패시브 슬롯 `F~J` 총 5개를 가지며, 런 중 실제로 선택 가능한 패시브는 최대 3개라는 설계 기준을 확정했다.
- 사용자 응답으로 이번 범위의 포로 보상은 `표시만 하는 정보`로 처리하고, 영입 시스템은 나중에 붙이기로 확정했다.
- 사용자 응답으로 1차 구현은 문서가 완비된 4몬스터(`아리엘`, `이브`, `세인`, `베가`)부터 진행하고, `린`은 더미 상태로 두기로 확정했다.
- 실제 저장소 확인 결과 아리엘, 이브, 세인, 베가는 `f~j` 패시브 문서가 모두 존재하지만, 린은 `f-ambidextrous.md`만 있고 `g~j` 패시브 문서는 아직 없다.
- 새 설계 문서 `Pakuri/reference/monster-select-run-ui-expansion-plan.html`를 추가했다.

### History

- 2026-04-25: `AGENTS.md`, `BLACKBOARD.md`를 다시 읽고 현재 작업 규칙과 기존 작업 블록을 재확인했다.
- 2026-04-25: `2.Monster` 폴더 전체, `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `combat-reward-system.md`, `dungeon-squad-run-structure.md`, 각 몬스터 타워 문서를 읽었다.
- 2026-04-25: 현재 코드와 씬 상태를 다시 확인해 현재 구현이 이브 단독 전투 프로토타입과 임시 HUD 수준임을 재확인했다.
- 2026-04-25: UI 자산 부재, 보상 풀 미구현, 속성/상태 공통 시스템 부족을 현재 확장 작업의 핵심 갭으로 정리했다.
- 2026-04-25: 몬스터 선택 UI, Run 시작, 전투 후 보상/스킬 선택 흐름을 정리한 설계 HTML `Pakuri/reference/monster-select-run-ui-expansion-plan.html`를 추가했다.
- 2026-04-25: 사용자 답변을 반영해 패시브는 슬롯 `F~J` 총 5개, 런 중 최대 3개 습득으로 설계를 고정했고, 포로 보상은 우선 표시 전용 정보로 처리하기로 기록했다.
- 2026-04-25: 실제 저장소에서 린의 `g~j` 패시브 문서가 없음을 다시 확인해, 문서 기반 전체 몬스터 구현 전에 남은 자료 갭으로 기록했다.
- 2026-04-25: 사용자 답변을 반영해 1차 구현 범위를 `아리엘`, `이브`, `세인`, `베가` 4몬스터 우선으로 고정하고, `린`은 더미 상태로 남기기로 기록했다.

## Task: SaveAndLoad Direction Plan

### Task title

Run / Meta 저장 경계와 SaveAndLoad 구조 설계 HTML 작성

### Goals

- 현재 Run 확장 설계와 `reference/4.run`, `reference/6.meta` 문서를 근거로 저장 / 불러오기 방향을 정리한다.
- 런 내부 저장과 메타 영구 저장의 경계를 분리한다.
- v1에서 저장할 것, 나중에 미룰 것, 저장하지 않을 런타임 상태를 HTML 문서 한 장으로 정리한다.

### Constraints

- 실제 문서와 실제 현재 코드 구조를 근거로만 적는다.
- 아직 미작성인 메타 해금 문서를 구현된 것처럼 적지 않는다.
- 이 작업은 Designer 설계 문서 작성이며, 실제 SaveLoad 코드 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 문서를 기준으로 Code Builder handoff를 작성해 `RunSession`, `MetaSaveData`, `RunSnapshot`, `SaveLoadService` 구현 순서를 고정한다.
- 실제 구현은 `GameDataCatalog` 부팅 로드 구조와 `RunSession` 분리 후 체크포인트 저장부터 시작하는 것이 맞다.

### Evidence

- `Pakuri/reference/4.run/dungeon-squad-run-structure.md`는 11일 단위 스테이지, 일반 진행일 선택지, 전투 후 보상, 다음 일차 이동 흐름을 정의한다.
- `Pakuri/reference/4.run/combat-reward-system.md`는 골드가 런 내부 재화이며 런 종료 시 사라지고, 어둠의 흔적이 런 외부 재화라고 정의한다.
- `Pakuri/reference/4.run/shop-system.md`는 상점이 스테이지당 1회, 6~9일 중 하루만 등장한다고 정의한다.
- `Pakuri/reference/4.run/event-system.md`는 일반 / 정예 전투 진입 직후 20% 확률 이벤트와 전투 복귀 흐름을 정의한다.
- `Pakuri/reference/6.meta/meta-growth-index.md`는 메타 성장에서 현재 확정된 범위와 미작성 범위를 구분한다.
- `Pakuri/reference/6.meta/meta-growth-node-list.md`는 캐릭터별 공통 스탯 강화와 초기화 규칙을 정의한다.
- `Pakuri/reference/6.meta/active-skill-growth-node-list.md`는 캐릭터별 액티브 메타 강화 규칙을 정의한다.
- `Pakuri/reference/6.meta/dark-trace-currency-system.md`는 어둠 계열 재화 티어, 승급, 사용처, 메타 초기화 규칙을 정의한다.
- `Pakuri/reference/monster-select-run-ui-expansion-plan.html`은 `RunSession` 분리와 Run 세션 데이터 제안을 포함한다.
- `Pakuri/reference/monster-select-run-ui-builder-handoff.html`은 고정 구현 순서에서 `RunSession` / `RunFlowController` 분리를 먼저 요구한다.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`는 현재 전투, 일차 진행, 보상, UI를 한 클래스에 함께 들고 있다.
- `Pakuri/data` CSV는 `Assets` 바깥에 있고, 현재 `Assets/Resources`, `Assets/StreamingAssets`, CSV 로더 흔적이 없다.
- `Pakuri/reference/save-and-load-plan.html`은 이제 저장 구조뿐 아니라 `CSV 저작 원본 -> 런타임 생성 자산 -> 게임 시작 시 1회 로드` 방향까지 포함한다.

### History

- 2026-04-26: `AGENTS.md`, `BLACKBOARD.md`, `monster-select-run-ui-expansion-plan.html`, `monster-select-run-ui-builder-handoff.html`, `reference/4.run`, `reference/6.meta`, 현재 `EveVerticalSliceController.cs`를 다시 읽었다.
- 2026-04-26: SaveAndLoad를 `MetaSaveData`, `RunSnapshot`, `EphemeralRuntime` 3층으로 나누고, v1은 일차 경계 체크포인트 저장만 지원하는 방향으로 정리한 HTML을 `Pakuri/reference/save-and-load-plan.html`에 추가했다.
- 2026-04-26: `Pakuri/data` CSV 검토 결과를 반영해 `save-and-load-plan.html`에 정적 게임 데이터 로딩 방향, importer 기반 생성 자산 구조, 부팅 시 1회 로드 방식을 추가했다.

## Task: CSV Data Role And Loading Review

### Task title

`Pakuri/data` CSV 역할 파악 및 게임 로딩 방식 검토

### Goals

- `Pakuri/data` 아래 CSV들의 실제 역할을 파일 구조와 샘플 행 기준으로 분류한다.
- 현재 프로젝트 코드가 이 CSV들을 실제로 읽고 있는지 확인한다.
- 게임에서 이 데이터를 언제, 어떤 방식으로 불러오는 것이 맞는지 설계 판단을 남긴다.

### Constraints

- 실제 CSV 내용, 실제 현재 스크립트, 실제 폴더 위치를 근거로만 판단한다.
- 아직 없는 CSV 로더나 데이터 파이프라인을 이미 있다고 말하지 않는다.
- 이 작업은 Designer 분석이며, CSV 로더 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 분석을 기준으로 Code Builder handoff를 작성해 CSV importer 또는 ScriptableObject 생성 파이프라인 구현 범위를 고정한다.
- 추천 방향은 `Pakuri/data`를 저작 원본으로 유지하고, 빌드용 런타임 데이터는 `Assets` 아래 생성 자산으로 변환하는 방식이다.

### Evidence

- `Pakuri/data` 아래 CSV는 총 22개이며 총 크기는 약 28.22KB다.
- `ally_units.csv`, `ally_runtime.csv`, `enemies.csv`, `enemy_runtime.csv`는 정적 스탯과 런타임 전투 파라미터가 분리된 구조다.
- `skills.csv`, `skill_runtime.csv`, `skill_branches.csv`, `levelup_choices.csv`, `levelup_rules.csv`는 스킬 / 분기 / 레벨업 선택지 데이터를 가진다.
- `waves_chapter1.csv`, `waves_chapter2.csv`, `waves_chapter3.csv`, `waves_runtime.csv`, `boss_patterns.csv`는 웨이브 / 보스 패턴 / 전투 진행 데이터를 가진다.
- `items.csv`, `status_effects.csv`, `formations.csv`, `balance_targets.csv`는 장비 / 상태이상 / 배치 / 밸런스 목표 데이터를 가진다.
- `spawn_points.csv`는 2번째 줄에 `적 스폰 좌표는 CSV가 아니라 코드에서 처리한다.`고 적혀 있어 현재 비활성 데이터다.
- `towers.csv`, `tower_skills.csv`는 `TOWER_001` 중심의 구형 단일 타워 프로토타입 데이터다.
- `ally_units.csv`는 `ALLY_*` 체계인데 `skills.csv`는 `TOWER_001` 소유 스킬만 가지고 있어 데이터 모델이 혼재되어 있다.
- 실제 무결성 확인 결과 `ally_units.csv`, `levelup_choices.csv`, `skill_branches.csv`가 참조하는 `SKILL_004` 이상 다수가 `skills.csv`에 없다.
- `Pakuri/data`는 `Assets` 바깥에 있으며, 현재 `Assets/Resources`, `Assets/StreamingAssets` 디렉터리는 존재하지 않는다.
- `Pakuri/Assets/Scripts`와 프로젝트 텍스트 파일 검색 결과 CSV 로더나 `TextAsset`, `Resources.Load`, `StreamingAssets` 사용 흔적은 확인되지 않았다.

### History

- 2026-04-26: `AGENTS.md`, `BLACKBOARD.md`를 다시 읽고 `Pakuri/data` 전체 CSV 목록, 헤더, 첫 행 샘플을 확인했다.
- 2026-04-26: 스킬 참조 무결성을 점검해 `ALLY_*` 기반 데이터와 `TOWER_*` 기반 데이터가 혼재되어 있고, 일부 스킬 참조가 비어 있음을 확인했다.
- 2026-04-26: 현재 CSV는 빌드 포함 위치에 있지 않고 로더도 없으므로, 런타임 직접 CSV 파싱보다 빌드 전 변환 자산 방식이 더 안전하다고 정리했다.
- 2026-04-26: 위 판단을 `Pakuri/reference/save-and-load-plan.html` 본문에도 반영해 SaveAndLoad와 정적 데이터 로딩 경계를 함께 문서화했다.

## Task: Run Systems Integration Summary Report

### Task title

`monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan` 통합 보고서 HTML 작성

### Goals

- 기존 3개 설계 HTML의 공통 결론을 한 장으로 합쳐 현재 프로젝트가 어떤 구조로 작업될지 빠르게 보여준다.
- 현재 실제 코드 상태와 문서 기준 구조를 함께 정리해, 구현 예정 범위와 아직 이른 범위를 분리한다.
- 기획서가 아직 부족한 부분과 현재 적용하기 이른 데이터 파이프라인을 명시적으로 `추후 구현 예정`으로 기록한다.

### Constraints

- 실제 존재하는 3개 HTML, 실제 현재 코드, 실제 문서 상태를 근거로만 적는다.
- 아직 구현되지 않은 UI, 저장, 데이터 importer를 구현된 것처럼 적지 않는다.
- 이 작업은 Designer 보고서 작성이며, 실제 코드 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 통합 보고서를 기준으로 Designer가 Code Builder handoff 문서를 더 짧게 다시 정리할 수 있다.
- 실제 구현은 보고서에 적은 순서대로 `RunSession` 분리, UI 흐름 분리, 정적 데이터 자산, A/F 최소 보상 / 스킬선택, 체크포인트 저장 순으로 들어가는 것이 안전하다.

### Evidence

- `Pakuri/reference/monster-select-run-ui-builder-handoff.html`는 `RunSession`, `RunFlowController` 또는 동등 구조를 먼저 세우는 고정 구현 순서를 제안한다.
- `Pakuri/reference/monster-select-run-ui-expansion-plan.html`는 몬스터 선택 UI, Run 시작, 전투 후 보상/선택 흐름과 `RunSession` 중심 구조를 설명한다.
- `Pakuri/reference/save-and-load-plan.html`는 `MetaSaveData`, `RunSnapshot`, `GameDataCatalog` 분리와 부팅 시 1회 데이터 로드를 정의한다.
- 현재 프로젝트의 게임 전용 스크립트는 `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`만 확인된다.
- 현재 `Pakuri/Assets` 아래에는 `Scenes`, `Screenshots`, `Scripts`, `Settings`만 있고, `Resources`, `StreamingAssets`, `DataGenerated`는 없다.
- 현재 프로젝트에는 `.uxml`, `.uss` UI Toolkit 자산이 없다.
- 실제 CSV 원본은 `Pakuri/data`에 있지만 현재 로더와 생성 자산 파이프라인은 없다.
- 새 통합 문서 `Pakuri/reference/run-systems-integration-summary-report.html`를 추가했고, 문서 안에 현재 구조, 작업 순서, 저장/데이터 방향, `추후 구현 예정` 항목을 함께 정리했다.
- 2026-04-26 재확인 결과 `Pakuri/reference/2.Monster/rin/rin-tower.md`와 `rin/skill/g~j` 문서가 존재해, 린의 패시브 문서 부족 전제는 더 이상 유효하지 않다.
- 2026-04-26 재확인 결과 `Pakuri/Assets` 재귀 검색에서 `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset` 관련 정적 데이터 로더 / 자산 정의는 확인되지 않았다.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`는 현재 보상 패널에서 이브 전용 고정 선택지 3개만 직접 생성한다.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`와 `Pakuri/reference/4.run/combat-reward-system.md`는 전체 보상 / 스킬선택 규칙을 정의하지만, 현재 구현은 그 전체 범위에 아직 도달하지 않았다.

### History

- 2026-04-26: `AGENTS.md`, `BLACKBOARD.md`, 기존 3개 설계 HTML을 다시 읽고 서로 겹치는 구조와 고정 결론을 추렸다.
- 2026-04-26: 현재 실제 코드와 자산 상태를 다시 확인해, 아직 없는 UI Toolkit 자산과 데이터 생성 파이프라인을 보고서에 명시적으로 비구현 상태로 적었다.
- 2026-04-26: `Pakuri/reference/run-systems-integration-summary-report.html`를 추가해 현재 구조, 권장 구현 순서, 데이터/저장 경계, 기획 부족 영역과 이른 데이터 적용 범위를 `추후 구현 예정`으로 분리했다.
- 2026-04-26: 린 문서 갱신과 데이터 방향 변경을 반영해 `run-systems-integration-summary-report.html`를 수정했고, 린을 5몬스터 범위에 포함시키고 정적 데이터는 CSV importer 전제가 아니라 Unity 프로젝트 내부 정적 자산 기준으로 정리했다.
- 2026-04-26: 보상 / 스킬선택은 완전히 나중으로 미루지 않고, `RunSession` / UI / 공통 전투 코어 다음 마일스톤에서 A/F 최소 범위를 같이 붙이는 방향으로 `run-systems-integration-summary-report.html`를 다시 수정했다.
