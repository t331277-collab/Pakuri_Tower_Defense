@echo off
chcp 65001 >nul
setlocal EnableExtensions DisableDelayedExpansion

cd /d "%~dp0"
set "CODEX_ROOT=%CD%"
set "PROMPT_FILE=%CODEX_ROOT%\codex_prompt.txt"
set "CODEX_CMD=%APPDATA%\npm\codex.cmd"

echo [run_codex] Root: %CODEX_ROOT%
echo [run_codex] Prompt: %PROMPT_FILE%
echo [run_codex] Codex CLI: %CODEX_CMD%

if exist "%PROMPT_FILE%" goto CHECK_CODEX
echo.
echo [run_codex] ERROR: codex_prompt.txt was not found.
echo [run_codex] Path: %PROMPT_FILE%
pause
exit /b 1

:CHECK_CODEX
if exist "%CODEX_CMD%" goto RUN_CODEX
echo.
echo [run_codex] ERROR: Codex CLI was not found.
echo [run_codex] Required default path: %CODEX_CMD%
echo [run_codex] This script intentionally uses %%APPDATA%%\npm\codex.cmd.
echo [run_codex] Install codex.cmd at that path or create a wrapper there, then run again.
pause
exit /b 1

:RUN_CODEX
echo.
echo [run_codex] Reading codex_prompt.txt as UTF-8 and starting Codex.
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; $prompt = [System.IO.File]::ReadAllText($env:PROMPT_FILE, [System.Text.Encoding]::UTF8); & $env:CODEX_CMD --cd $env:CODEX_ROOT $prompt; if ($null -eq $global:LASTEXITCODE) { exit 0 } else { exit $global:LASTEXITCODE }"
set "CODEX_EXIT_CODE=%ERRORLEVEL%"

echo.
echo [run_codex] Codex exit code: %CODEX_EXIT_CODE%
pause
exit /b %CODEX_EXIT_CODE%
