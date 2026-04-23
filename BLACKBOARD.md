# BLACKBOARD.md

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

### Builder Reviewer Loop

- Enforcement method: External wrapper script
- Wrapper file: `codex_builder_reviewer.ps1`
- Git dependency: Not required
- Max loops: 3
- Current loop count: 0
- Last reviewer decision: PASS for manual reviewer log `codex_loop_logs\manual_reviewer_20260423_212033.md`
- Last log directory: `codex_loop_logs`

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

### History

- 2026-04-23: Unity 프로젝트 구조, MCP 패키지 설치, Codex CLI MCP 등록 상태를 확인했다.
- 2026-04-23: Unity MCP 서버는 실행 중이나 Unity Editor bridge 인스턴스가 등록되지 않았음을 확인했다.
- 2026-04-23: Unity Editor 내부 MCP For Unity 설정/bridge 시작이 필요하다고 판단했다.
- 2026-04-23: 사용자가 Unity Editor에서 Transport를 `Stdio`로 바꾸고 `Session Active`, Codex client `Configuration`을 수행했다.
- 2026-04-23: Unity MCP bridge 연결, scene/asset/console/hierarchy 접근, EditMode Test Runner 실행을 검증했다.
