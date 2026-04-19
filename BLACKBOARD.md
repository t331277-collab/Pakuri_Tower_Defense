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

Completed for bootstrap file creation and path correction. No downstream Builder task has been run through the loop yet.

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

### History

- 2026-04-19: 작업 폴더와 대상 파일 존재 여부를 확인했다.
- 2026-04-19: Codex CLI 실제 경로, 버전, `exec`, `review` 도움말을 확인했다.
- 2026-04-19: `%APPDATA%\npm\codex.cmd`가 현재 존재하지 않는다는 점을 확인했다.
- 2026-04-19: 네이티브 hook/event가 도움말 출력에서 확인되지 않아 외부 PowerShell 래퍼 방식으로 설계했다.
- 2026-04-19: `run_codex.bat`, `codex_prompt.txt`, `AGENTS.md`, `BLACKBOARD.md`, `codex_builder_reviewer.ps1`를 생성했다.
- 2026-04-19: 승인 후 `%APPDATA%\npm\codex.cmd` 래퍼를 생성하고 `--version` 실행으로 검증했다.

### Builder Reviewer Loop

- Enforcement method: External wrapper script
- Wrapper file: `codex_builder_reviewer.ps1`
- Git dependency: Not required
- Max loops: 3
- Current loop count: 0
- Last reviewer decision: Not run yet for a downstream Builder task
- Last log directory: Not created yet by an actual Builder -> Reviewer run
