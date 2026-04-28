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
    $content = Read-Utf8File -Path $BlackboardPath
    $markers = @("`r`n### Builder Reviewer Loop", "`n### Builder Reviewer Loop")
    foreach ($marker in $markers) {
        $index = $content.IndexOf($marker, [System.StringComparison]::Ordinal)
        if ($index -ge 0) {
            Write-Utf8File -Path $BlackboardPath -Text ($content.Insert($index, $entry))
            return
        }
    }

    Write-Utf8File -Path $BlackboardPath -Text ($content + $entry)
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

function Get-BlackboardIndexText {
    $lines = Get-Content -LiteralPath $BlackboardPath -Encoding UTF8
    $selected = New-Object System.Collections.Generic.List[string]
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '^## Task:' -or
            $line -match '^### (Status|Next Actions|Evidence|History)$' -or
            $line -match 'Reviewer|reviewer|token|usage|timeout|codex_builder_reviewer|Codex CLI Bootstrap') {
            $selected.Add(('{0}: {1}' -f ($i + 1), $line))
        }
    }

    if ($selected.Count -eq 0) {
        return 'No BLACKBOARD.md task index lines matched.'
    }

    return ($selected -join "`r`n")
}

function Limit-Text {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [int]$MaxChars = 30000
    )

    if ($Text.Length -le $MaxChars) {
        return $Text
    }

    return $Text.Substring(0, $MaxChars) + "`r`n... [truncated by codex_builder_reviewer.ps1 after $MaxChars chars]"
}

function Get-ChangedPathList {
    param([Parameter(Mandatory = $true)]$ChangedFiles)

    $paths = New-Object System.Collections.Generic.List[string]
    foreach ($change in $ChangedFiles) {
        if (-not $change) {
            continue
        }

        $parts = $change -split ' ', 2
        if ($parts.Count -eq 2 -and $parts[1]) {
            $paths.Add($parts[1])
        }
    }

    return $paths
}

function Get-GitDiffText {
    param([Parameter(Mandatory = $true)]$ChangedFiles)

    $git = Get-Command git -ErrorAction SilentlyContinue
    if ($null -eq $git) {
        return 'Git command not found; git diff evidence is unavailable.'
    }

    $inside = & git -C $Root rev-parse --is-inside-work-tree 2>$null
    if ($LASTEXITCODE -ne 0 -or (($inside | Out-String).Trim()) -ne 'true') {
        return 'Current root is not a Git work tree; git diff evidence is unavailable.'
    }

    $paths = @(Get-ChangedPathList -ChangedFiles $ChangedFiles | Where-Object {
        $_ -and
        ($_ -notmatch '^codex_loop_logs[\\/]')
    })

    if ($paths.Count -eq 0) {
        return 'No changed paths available for git diff.'
    }

    $arguments = @('-C', $Root, 'diff', '--unified=30', '--') + $paths
    $diff = & git @arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        return "git diff failed with exit code $LASTEXITCODE.`r`n$($diff -join "`r`n")"
    }

    if ($null -eq $diff -or @($diff).Count -eq 0) {
        return 'git diff returned no text for changed paths.'
    }

    return ($diff -join "`r`n")
}

function Get-AddedFileEvidenceText {
    param([Parameter(Mandatory = $true)]$ChangedFiles)

    $paths = @(Get-ChangedPathList -ChangedFiles $ChangedFiles | Where-Object {
        $_ -and
        ($_ -notmatch '^codex_loop_logs[\\/]') -and
        (Test-Path -LiteralPath (Join-Path $Root $_))
    })

    if ($paths.Count -eq 0) {
        return 'No existing changed files available for added-file evidence.'
    }

    $sections = New-Object System.Collections.Generic.List[string]
    foreach ($path in $paths) {
        $fullPath = Join-Path $Root $path
        $resolvedPath = (Resolve-Path -LiteralPath $fullPath).Path
        $relativePath = Get-RelativePath -FullName $resolvedPath
        $fileInfo = Get-Item -LiteralPath $resolvedPath
        if ($fileInfo.Length -gt 200000) {
            $sections.Add("### $relativePath`r`nSkipped content evidence because file is larger than 200000 bytes.")
            continue
        }

        if ($fileInfo.Extension -notin @('.ps1', '.cs', '.md', '.html', '.txt', '.json', '.xml', '.asset', '.unity', '.uss', '.uxml', '.bat', '.cmd')) {
            $sections.Add("### $relativePath`r`nSkipped content evidence because extension '$($fileInfo.Extension)' is not in the text evidence allowlist.")
            continue
        }

        $text = Read-Utf8File -Path $fullPath
        $sections.Add("### $relativePath`r`n" + (Limit-Text -Text $text -MaxChars 12000))
    }

    return ($sections -join "`r`n`r`n")
}

function Invoke-CodexExec {
    param(
        [Parameter(Mandatory = $true)][string]$Prompt,
        [Parameter(Mandatory = $true)][string]$OutputPath
    )

    $consoleOutputPath = "$OutputPath.console.txt"
    $exitCode = $null
    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $Prompt | & $ResolvedCodexCmd exec --full-auto --skip-git-repo-check -C $Root -o $OutputPath - *> $consoleOutputPath
        $exitCode = $global:LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($null -eq $exitCode) {
        return 0
    }
    return [int]$exitCode
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
$blackboardIndexPath = Join-Path $runDir 'blackboard_index.txt'
Write-Utf8File -Path $blackboardIndexPath -Text (Get-BlackboardIndexText)

Add-BlackboardHistory "Builder -> Reviewer loop started. Run directory: $runDir"

$reviewFeedback = ''
for ($loop = 1; $loop -le $MaxLoops; $loop++) {
    $before = @(Get-FileSnapshot)
    $builderOutput = Join-Path $runDir ("loop_{0:00}_builder.md" -f $loop)
    $reviewerOutput = Join-Path $runDir ("loop_{0:00}_reviewer.md" -f $loop)

    $builderPrompt = @"
Role: Code Builder

Read AGENTS.md in full before doing any substantive work, then follow it.
For BLACKBOARD.md, avoid printing or dumping the full file. First read this generated task index:
$blackboardIndexPath
Then read only the related BLACKBOARD.md task block(s) needed for the user task. If no related block exists, state that.
Current loop: $loop / $MaxLoops

User task:
$taskText

Previous Reviewer feedback:
$reviewFeedback

Requirements:
- Base all claims on actual files and command output.
- Implement directly when implementation is needed.
- Record changed files and verification results in BLACKBOARD.md or the log.
- When checking whether something exists, use targeted file reads/searches and cite the command result.
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
    $gitDiffText = Get-GitDiffText -ChangedFiles $changedFiles
    $gitDiffForPrompt = Limit-Text -Text $gitDiffText -MaxChars 30000
    $addedFileEvidenceText = Get-AddedFileEvidenceText -ChangedFiles $changedFiles
    $addedFileEvidenceForPrompt = Limit-Text -Text $addedFileEvidenceText -MaxChars 30000

    Write-Utf8File -Path (Join-Path $runDir ("loop_{0:00}_changed_files.txt" -f $loop)) -Text $changedText
    Write-Utf8File -Path (Join-Path $runDir ("loop_{0:00}_git_status.txt" -f $loop)) -Text $gitStatus
    $gitDiffPath = Join-Path $runDir ("loop_{0:00}_git_diff.patch" -f $loop)
    Write-Utf8File -Path $gitDiffPath -Text $gitDiffText
    $addedFileEvidencePath = Join-Path $runDir ("loop_{0:00}_changed_file_evidence.txt" -f $loop)
    Write-Utf8File -Path $addedFileEvidencePath -Text $addedFileEvidenceText

    $reviewerPrompt = @"
Role: Code Reviewer

Read AGENTS.md in full before doing any substantive work, then follow it.
For BLACKBOARD.md, avoid printing or dumping the full file. First read this generated task index:
$blackboardIndexPath
Then read only the related BLACKBOARD.md task block(s) needed for the user task. If no related block exists, state that.
Do not implement. Review the changed files and actual file contents line-by-line.

Current loop: $loop / $MaxLoops

User task:
$taskText

Changed files from snapshot comparison:
$changedText

Git status information:
$gitStatus

Git diff evidence path:
$gitDiffPath

Git diff evidence excerpt:
$gitDiffForPrompt

Changed file content evidence path:
$addedFileEvidencePath

Changed file content evidence excerpt:
$addedFileEvidenceForPrompt

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
