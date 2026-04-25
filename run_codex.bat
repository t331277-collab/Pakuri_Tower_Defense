@echo off
chcp 65001 >nul
setlocal EnableExtensions DisableDelayedExpansion

cd /d "%~dp0"
set "CODEX_ROOT=%CD%"
set "PROMPT_FILE=%CODEX_ROOT%\codex_prompt.txt"
set "LAUNCHER_SCRIPT=%CODEX_ROOT%\run_codex_prompt_launcher.ps1"
set "CODEX_CMD="

echo [run_codex] Root: %CODEX_ROOT%
echo [run_codex] Prompt: %PROMPT_FILE%
echo [run_codex] Launcher: %LAUNCHER_SCRIPT%

if exist "%PROMPT_FILE%" goto CHECK_CODEX
echo.
echo [run_codex] ERROR: codex_prompt.txt was not found.
echo [run_codex] Path: %PROMPT_FILE%
pause
exit /b 1

:CHECK_CODEX
if exist "%LAUNCHER_SCRIPT%" goto RESOLVE_CODEX
echo.
echo [run_codex] ERROR: run_codex_prompt_launcher.ps1 was not found.
echo [run_codex] Path: %LAUNCHER_SCRIPT%
pause
exit /b 1

:RESOLVE_CODEX
for /f "usebackq delims=" %%I in (`powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; try { $command = Get-Command codex -ErrorAction Stop; if ($command.CommandType -eq 'Application' -and (Test-Path -LiteralPath $command.Source)) { & $command.Source --version *> $null; if ($LASTEXITCODE -eq 0) { [Console]::WriteLine($command.Source); exit 0 } } } catch {}; $extensions = Join-Path $env:USERPROFILE '.vscode\extensions'; if (Test-Path -LiteralPath $extensions) { $candidate = Get-ChildItem -LiteralPath $extensions -Directory -Filter 'openai.chatgpt-*' | ForEach-Object { Join-Path $_.FullName 'bin\windows-x86_64\codex.exe' } | Where-Object { Test-Path -LiteralPath $_ } | Sort-Object FullName -Descending | Select-Object -First 1; if ($candidate) { [Console]::WriteLine($candidate); exit 0 } }; $wrapper = Join-Path $env:APPDATA 'npm\codex.cmd'; try { if (Test-Path -LiteralPath $wrapper) { & $wrapper --version *> $null; if ($LASTEXITCODE -eq 0) { [Console]::WriteLine($wrapper); exit 0 } } } catch {}; exit 1"`) do set "CODEX_CMD=%%I"

if defined CODEX_CMD goto RUN_CODEX
echo.
echo [run_codex] ERROR: Codex CLI was not found.
echo [run_codex] Checked Get-Command codex, %%USERPROFILE%%\.vscode\extensions\openai.chatgpt-*\bin\windows-x86_64\codex.exe, and %%APPDATA%%\npm\codex.cmd.
pause
exit /b 1

:RUN_CODEX
echo [run_codex] Codex CLI: %CODEX_CMD%
echo.
echo [run_codex] Reading codex_prompt.txt as UTF-8 and starting Codex.
powershell -NoProfile -ExecutionPolicy Bypass -File "%LAUNCHER_SCRIPT%" -CodexCommand "%CODEX_CMD%" -CodeXRoot "%CODEX_ROOT%" -PromptFile "%PROMPT_FILE%"
set "CODEX_EXIT_CODE=%ERRORLEVEL%"

echo.
echo [run_codex] Codex exit code: %CODEX_EXIT_CODE%
pause
exit /b %CODEX_EXIT_CODE%
