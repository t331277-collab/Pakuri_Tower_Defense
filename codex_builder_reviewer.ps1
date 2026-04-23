[CmdletBinding()]
param(
    [string]$Task,
    [string]$TaskFile,
    [string]$CodexCmd,
    [int]$MaxLoops = 3
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)

$Root = $PSScriptRoot
$BlackboardPath = Join-Path $Root 'BLACKBOARD.md'
$AgentsPath = Join-Path $Root 'AGENTS.md'
$LogRoot = Join-Path $Root 'codex_loop_logs'

function Resolve-CodexCommand {
    if ($CodexCmd) {
        if (-not (Test-Path -LiteralPath $CodexCmd)) {
            throw "Codex CLI was not found: $CodexCmd"
        }
        return $CodexCmd
    }

    $wrapper = Join-Path $env:APPDATA 'npm\codex.cmd'
    try {
        if (Test-Path -LiteralPath $wrapper) {
            & $wrapper --version *> $null
            if ($LASTEXITCODE -eq 0) {
                return $wrapper
            }
        }
    } catch {
        # Fall through to the VS Code extension scan below.
    }

    $extensions = Join-Path $env:USERPROFILE '.vscode\extensions'
    if (Test-Path -LiteralPath $extensions) {
        $candidate = Get-ChildItem -LiteralPath $extensions -Directory -Filter 'openai.chatgpt-*' |
            ForEach-Object { Join-Path $_.FullName 'bin\windows-x86_64\codex.exe' } |
            Where-Object { Test-Path -LiteralPath $_ } |
            Sort-Object -Descending |
            Select-Object -First 1

        if ($candidate) {
            return $candidate
        }
    }

    throw "Codex CLI was not found. Checked $wrapper and $extensions\openai.chatgpt-*\bin\windows-x86_64\codex.exe."
}

$ResolvedCodexCmd = Resolve-CodexCommand

function Read-Utf8File {
    param([Parameter(Mandatory = $true)][string]$Path)
    return [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
}

function Write-Utf8File {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )
    [System.IO.File]::WriteAllText($Path, $Text, [System.Text.UTF8Encoding]::new($false))
}

function Add-BlackboardHistory {
    param([Parameter(Mandatory = $true)][string]$Text)
    $stamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss K'
    $entry = "`r`n- ${stamp}: $Text"
    [System.IO.File]::AppendAllText($BlackboardPath, $entry, [System.Text.UTF8Encoding]::new($false))
}

function Assert-RequiredFiles {
    if (-not (Test-Path -LiteralPath $AgentsPath)) {
        throw "AGENTS.md was not found: $AgentsPath"
    }
    if (-not (Test-Path -LiteralPath $BlackboardPath)) {
        throw "BLACKBOARD.md was not found: $BlackboardPath"
    }
    if (-not (Test-Path -LiteralPath $ResolvedCodexCmd)) {
        throw "Codex CLI was not found: $ResolvedCodexCmd"
    }
}

function Get-RelativePath {
    param([Parameter(Mandatory = $true)][string]$FullName)
    return $FullName.Substring($Root.Length).TrimStart('\', '/')
}

function Test-InIgnoredPath {
    param([Parameter(Mandatory = $true)][string]$FullName)
    $gitPath = (Join-Path $Root '.git') + [System.IO.Path]::DirectorySeparatorChar
    $logPath = (Join-Path $Root 'codex_loop_logs') + [System.IO.Path]::DirectorySeparatorChar
    return $FullName.StartsWith($gitPath, [System.StringComparison]::OrdinalIgnoreCase) -or
        $FullName.StartsWith($logPath, [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-FileSnapshot {
    Get-ChildItem -LiteralPath $Root -Recurse -Force -File |
        Where-Object { -not (Test-InIgnoredPath -FullName $_.FullName) } |
        ForEach-Object {
            [pscustomobject]@{
                Path = Get-RelativePath -FullName $_.FullName
                Length = $_.Length
                LastWriteTicks = $_.LastWriteTimeUtc.Ticks
            }
        }
}

function Compare-FileSnapshots {
    param(
        [Parameter(Mandatory = $true)]$Before,
        [Parameter(Mandatory = $true)]$After
    )

    $beforeMap = @{}
    foreach ($item in $Before) {
        $beforeMap[$item.Path] = $item
    }

    $afterMap = @{}
    foreach ($item in $After) {
        $afterMap[$item.Path] = $item
    }

    $changes = New-Object System.Collections.Generic.List[string]
    foreach ($item in $After) {
        if (-not $beforeMap.ContainsKey($item.Path)) {
            $changes.Add("ADDED $($item.Path)")
            continue
        }
        $old = $beforeMap[$item.Path]
        if ($old.Length -ne $item.Length -or $old.LastWriteTicks -ne $item.LastWriteTicks) {
            $changes.Add("MODIFIED $($item.Path)")
        }
    }

    foreach ($item in $Before) {
        if (-not $afterMap.ContainsKey($item.Path)) {
            $changes.Add("DELETED $($item.Path)")
        }
    }

    return $changes
}

function Get-GitStatusText {
    $git = Get-Command git -ErrorAction SilentlyContinue
    if ($null -eq $git) {
        return 'Git command not found.'
    }

    $inside = & git -C $Root rev-parse --is-inside-work-tree 2>$null
    if ($LASTEXITCODE -ne 0 -or (($inside | Out-String).Trim()) -ne 'true') {
        return 'Current root is not a Git work tree.'
    }

    $lines = & git -C $Root status --short
    $filtered = @($lines | Where-Object {
        $_ -and
        ($_ -notmatch 'codex_loop_logs/') -and
        ($_ -notmatch 'codex_loop_logs\\')
    })

    if ($filtered.Count -eq 0) {
        return 'Git status has no changes outside codex_loop_logs.'
    }

    return ($filtered -join "`r`n")
}

function Invoke-CodexExec {
    param(
        [Parameter(Mandatory = $true)][string]$Prompt,
        [Parameter(Mandatory = $true)][string]$OutputPath
    )

    $Prompt | & $ResolvedCodexCmd exec --full-auto --skip-git-repo-check -C $Root -o $OutputPath -
    if ($null -eq $global:LASTEXITCODE) {
        return 0
    }
    return $global:LASTEXITCODE
}

Assert-RequiredFiles

if (-not (Test-Path -LiteralPath $LogRoot)) {
    New-Item -ItemType Directory -Path $LogRoot | Out-Null
}

$taskTextParts = New-Object System.Collections.Generic.List[string]
if ($Task) {
    $taskTextParts.Add($Task)
}
if ($TaskFile) {
    if (-not (Test-Path -LiteralPath $TaskFile)) {
        throw "TaskFile was not found: $TaskFile"
    }
    $taskTextParts.Add((Read-Utf8File -Path $TaskFile))
}
if ($taskTextParts.Count -eq 0) {
    $entered = Read-Host 'Enter the task for Code Builder'
    if (-not $entered) {
        throw 'Task text is empty.'
    }
    $taskTextParts.Add($entered)
}

$taskText = $taskTextParts -join "`r`n`r`n"
$runId = Get-Date -Format 'yyyyMMdd_HHmmss'
$runDir = Join-Path $LogRoot $runId
New-Item -ItemType Directory -Path $runDir | Out-Null
Write-Utf8File -Path (Join-Path $runDir 'task.txt') -Text $taskText

Add-BlackboardHistory "Builder -> Reviewer loop started. Run directory: $runDir"

$reviewFeedback = ''
for ($loop = 1; $loop -le $MaxLoops; $loop++) {
    $before = @(Get-FileSnapshot)
    $builderOutput = Join-Path $runDir ("loop_{0:00}_builder.md" -f $loop)
    $reviewerOutput = Join-Path $runDir ("loop_{0:00}_reviewer.md" -f $loop)

    $builderPrompt = @"
Role: Code Builder

Read AGENTS.md and BLACKBOARD.md before doing any substantive work, then follow them.
Current loop: $loop / $MaxLoops

User task:
$taskText

Previous Reviewer feedback:
$reviewFeedback

Requirements:
- Base all claims on actual files and command output.
- Implement directly when implementation is needed.
- Record changed files and verification results in BLACKBOARD.md or the log.
- Do not stop after the Builder response; this external wrapper will run the Reviewer phase next.
"@

    Add-BlackboardHistory "Loop $loop Builder started. Output: $builderOutput"
    $builderExit = Invoke-CodexExec -Prompt $builderPrompt -OutputPath $builderOutput
    Add-BlackboardHistory "Loop $loop Builder finished with exit code $builderExit."
    if ($builderExit -ne 0) {
        throw "Builder phase failed. Loop=$loop ExitCode=$builderExit Output=$builderOutput"
    }

    $after = @(Get-FileSnapshot)
    $changedFiles = @(Compare-FileSnapshots -Before $before -After $after)
    $changedText = if ($changedFiles.Count -gt 0) { $changedFiles -join "`r`n" } else { 'No file changes detected by snapshot comparison.' }
    $gitStatus = Get-GitStatusText

    Write-Utf8File -Path (Join-Path $runDir ("loop_{0:00}_changed_files.txt" -f $loop)) -Text $changedText
    Write-Utf8File -Path (Join-Path $runDir ("loop_{0:00}_git_status.txt" -f $loop)) -Text $gitStatus

    $reviewerPrompt = @"
Role: Code Reviewer

Read AGENTS.md and BLACKBOARD.md before doing any substantive work, then follow them.
Do not implement. Review the changed files and actual file contents line-by-line.

Current loop: $loop / $MaxLoops

User task:
$taskText

Changed files from snapshot comparison:
$changedText

Git status information:
$gitStatus

Review scope:
- Review changed lines line-by-line.
- Confirm that used functions/helpers actually exist.
- Check null/None risks.
- Check additional issues and side effects.

Decision format:
- If there are no issues, end with exactly REVIEW_RESULT: PASS.
- If there are issues, end with exactly REVIEW_RESULT: NEEDS_CHANGES and include concrete files, lines, and evidence for Builder.
"@

    Add-BlackboardHistory "Loop $loop Reviewer started. Output: $reviewerOutput"
    $reviewerExit = Invoke-CodexExec -Prompt $reviewerPrompt -OutputPath $reviewerOutput
    Add-BlackboardHistory "Loop $loop Reviewer finished with exit code $reviewerExit."
    if ($reviewerExit -ne 0) {
        throw "Reviewer phase failed. Loop=$loop ExitCode=$reviewerExit Output=$reviewerOutput"
    }

    $reviewText = Read-Utf8File -Path $reviewerOutput
    if ($reviewText -match 'REVIEW_RESULT:\s*PASS') {
        Add-BlackboardHistory "Loop $loop Reviewer decision: PASS. Builder -> Reviewer loop completed."
        Write-Host "Reviewer PASS at loop $loop."
        exit 0
    }

    if ($reviewText -match 'REVIEW_RESULT:\s*NEEDS_CHANGES') {
        Add-BlackboardHistory "Loop $loop Reviewer decision: NEEDS_CHANGES. Builder will rerun if loop budget remains."
        $reviewFeedback = $reviewText
        continue
    }

    Add-BlackboardHistory "Loop $loop Reviewer decision marker missing. Treating as NEEDS_CHANGES."
    $reviewFeedback = "Reviewer output did not include a required REVIEW_RESULT marker. Treat as NEEDS_CHANGES.`r`n`r`n$reviewText"
}

Add-BlackboardHistory "Builder -> Reviewer loop stopped after reaching max loop count $MaxLoops without PASS."
Write-Error "Reviewer PASS was not reached within max loop count $MaxLoops. Log: $runDir"
exit 2
