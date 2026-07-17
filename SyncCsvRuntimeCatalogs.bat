@echo off
setlocal EnableExtensions

set "REPO_DIR=%~dp0"
set "PROJECT_DIR=%REPO_DIR%Pakuri"
set "LOG_FILE=%REPO_DIR%PakuriCsvRuntimeSync.log"

if not exist "%PROJECT_DIR%\Assets\CSVdata\authoring\monster\monsters.csv" (
    echo Missing Unity project or CSV authoring source under "%PROJECT_DIR%".
    exit /b 1
)

if not defined UNITY_EXE (
    for /f "tokens=2" %%V in ('findstr /b /c:"m_EditorVersion:" "%PROJECT_DIR%\ProjectSettings\ProjectVersion.txt"') do (
        if exist "C:\Program Files\Unity\Hub\Editor\%%V\Editor\Unity.exe" (
            set "UNITY_EXE=C:\Program Files\Unity\Hub\Editor\%%V\Editor\Unity.exe"
        )
    )
)

if not defined UNITY_EXE (
    for /d %%D in ("C:\Program Files\Unity\Hub\Editor\*") do (
        if exist "%%~fD\Editor\Unity.exe" (
            set "UNITY_EXE=%%~fD\Editor\Unity.exe"
        )
    )
)

if not defined UNITY_EXE (
    if exist "C:\Program Files\Unity\Editor\Unity.exe" (
        set "UNITY_EXE=C:\Program Files\Unity\Editor\Unity.exe"
    )
)

if not exist "%UNITY_EXE%" (
    echo Unity.exe was not found.
    echo Set UNITY_EXE to the full Unity.exe path, then run this file again.
    echo Example:
    echo   set "UNITY_EXE=C:\Program Files\Unity\Hub\Editor\6000.0.38f1\Editor\Unity.exe"
    exit /b 1
)

echo Syncing and validating Pakuri CSV runtime catalogs...
echo Project: "%PROJECT_DIR%"
echo Unity: "%UNITY_EXE%"
echo Log: "%LOG_FILE%"

"%UNITY_EXE%" -batchmode -quit -projectPath "%PROJECT_DIR%" -executeMethod Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor -logFile "%LOG_FILE%"
set "EXIT_CODE=%ERRORLEVEL%"

if not "%EXIT_CODE%"=="0" (
    echo CSV runtime catalog sync failed with exit code %EXIT_CODE%.
    echo Check "%LOG_FILE%" for Unity batchmode details.
    exit /b %EXIT_CODE%
)

echo CSV runtime catalog sync completed.
exit /b 0
