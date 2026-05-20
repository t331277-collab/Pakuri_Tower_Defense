## Archived History

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- Completed or older Reviewer task blocks were archived to `boards/ARCHIVE/OPS_REVIEWER_ARCHIVE_2026-05-19.md` on 2026-05-19.

## Task: 2026-05-14 InGame Phase2-A Base Unit Model Reviewer

### Task title

Run Code Reviewer after the Phase2-A base unit runtime model split.

### Goals

- Execute one Code Reviewer pass after Code Builder added `BaseUnitRuntimeModel`, `MonsterUnitRuntimeModel`, and `EnemyUnitRuntimeModel`.
- Review the uncommitted changed set for compile risks, missing helpers, null risks, and side effects.
- Record whether the Reviewer returned a pass decision or a fix request.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer must not edit files.
- Do not run Unity Play Mode; user owns gameplay verification.

### Role Owner

Code Reviewer

### Status

Reviewer executed and returned a fix request, not a pass decision.

### Next Actions

- Code Builder should not run another Reviewer pass for this task unless the user explicitly asks.
- The reported issue is in the existing uncommitted Phase1-D skill validator area, not in the Phase2-A unit model split files.
- Decide separately whether to fix `InGameSkillDataValidator` so Eve-E `MagazineProjectile` without `ShotIntervalSeconds` is accepted or remapped.

### Evidence

- Initial Reviewer command using `openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe review --uncommitted` failed because that executable path no longer exists.
- `Get-ChildItem C:\Users\t3312\.vscode\extensions -Directory -Filter 'openai.chatgpt-*'` found `openai.chatgpt-26.506.31421-win32-x64`.
- Reviewer command using the current executable first failed with socket/network error `os error 10013`.
- Escalated Reviewer command using `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.506.31421-win32-x64\bin\windows-x86_64\codex.exe review --uncommitted` completed and reported `[P2] Do not require shot intervals for every magazine skill`.
- Reviewer cited `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDataValidator.cs:363-365` and said the current catalog has Eve-E as `MagazineProjectile` with `ShotIntervalSeconds: 0`, while existing runtime `TryCastEveDroneBeacon()` uses `EveDroneAttackPeriod`.

### History

- 2026-05-14: Code Builder attempted the required Reviewer transition after the Phase2-A model split; Reviewer did not pass because it found a validator issue in the broader uncommitted set.

## Task: 2026-05-14 InGame Rename Reviewer Attempt

### Task title

Run Code Reviewer after the CombatV2-to-InGame rename.

### Goals

- Execute one Code Reviewer pass after Code Builder renamed the `Assets/Scripts2` runtime tree to `InGame`.
- Review script/class/path rename consistency, `.csproj` references, HTML report updates, and board updates.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer must not edit files.
- Do not run Unity Play Mode; user owns gameplay verification.

### Role Owner

Code Reviewer

### Status

Reviewer execution attempted but did not complete.

### Next Actions

- Re-run Reviewer later when Codex CLI network/socket access is stable, or provide an approved external wrapper run that can wait longer than 180 seconds.

### Evidence

- First Reviewer command used the old extension path `openai.chatgpt-26.417.40842-win32-x64` and failed because `codex.exe` was not found.
- `Get-ChildItem C:\Users\t3312\.vscode\extensions -Directory -Filter 'openai.chatgpt-*'` found `openai.chatgpt-26.506.31421-win32-x64`.
- Reviewer command using `openai.chatgpt-26.506.31421-win32-x64\bin\windows-x86_64\codex.exe review --uncommitted` started but failed with socket/network error `os error 10013`.
- Escalated Reviewer command with the same executable timed out after 180 seconds before returning a review result.

### History

- 2026-05-14: Code Builder attempted the required Reviewer transition after the InGame rename, but no `PASS` or `FAIL` review result was produced.
