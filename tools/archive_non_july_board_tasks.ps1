param(
    [string]$KeepMonth = "2026-07",
    [string]$ArchiveDate = "2026-07-18",
    [switch]$Apply
)

$ErrorActionPreference = "Stop"

$repositoryRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$boardDirectories = @(
    "boards/COMBAT",
    "boards/DATA",
    "boards/MON",
    "boards/OPS",
    "boards/RUN",
    "boards/UI"
)
$archiveRelativePath = "boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_$ArchiveDate.md"
$archivePath = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $archiveRelativePath))
$damageMeterRelativePath = "boards/UI/DAMAGE_METER_UI_HANDOFF.md"
$damageMeterPath = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $damageMeterRelativePath))
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$requiredTaskSections = @(
    "### Task title",
    "### Goals",
    "### Constraints",
    "### Role Owner",
    "### Status",
    "### Next Actions",
    "### Evidence",
    "### History"
)

function Assert-WorkspacePath {
    param([string]$Path)

    $resolved = [System.IO.Path]::GetFullPath($Path)
    if (-not $resolved.StartsWith($repositoryRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path escapes repository root: $resolved"
    }
}

function Read-Utf8Text {
    param([string]$Path)

    return [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
}

function Write-Utf8Text {
    param(
        [string]$Path,
        [string]$Text
    )

    Assert-WorkspacePath $Path
    [System.IO.File]::WriteAllText($Path, $Text, $utf8NoBom)
}

function Normalize-Newlines {
    param([string]$Text)

    return (($Text -replace "`r`n", "`n") -replace "`r", "`n")
}

function Get-LevelTwoSections {
    param([string]$Text)

    $matches = @([regex]::Matches($Text, '(?m)^## (?!#)[^\r\n]*$'))
    $sections = New-Object System.Collections.Generic.List[object]
    for ($index = 0; $index -lt $matches.Count; $index++) {
        $start = $matches[$index].Index
        $end = if ($index + 1 -lt $matches.Count) { $matches[$index + 1].Index } else { $Text.Length }
        $sections.Add([pscustomobject]@{
            Heading = $matches[$index].Value.Trim()
            Start = $start
            End = $end
            Text = $Text.Substring($start, $end - $start).TrimEnd()
        })
    }

    return $sections
}

function Test-IsTaskHeading {
    param([string]$Heading)

    return $Heading -match '^## (?:Recent )?Task:'
}

function Test-IsKeptMonthTask {
    param([string]$Heading)

    $dateMatch = [regex]::Match($Heading, '2026-\d{2}-\d{2}')
    return $dateMatch.Success -and $dateMatch.Value.StartsWith("$KeepMonth-", [System.StringComparison]::Ordinal)
}

function Complete-TaskFormat {
    param([string]$TaskText)

    $normalized = Normalize-Newlines $TaskText
    if ($normalized.Contains("### Task title")) {
        return $normalized.TrimEnd()
    }

    $firstNewline = $normalized.IndexOf("`n")
    $heading = if ($firstNewline -ge 0) { $normalized.Substring(0, $firstNewline) } else { $normalized }
    $body = if ($firstNewline -ge 0) { $normalized.Substring($firstNewline + 1).TrimStart("`n") } else { "" }
    $title = ($heading -replace '^## (?:Recent )?Task:\s*', '').Trim()
    return "$heading`n`n### Task title`n`n$title`n`n$body".TrimEnd()
}

function Assert-TaskFormat {
    param(
        [string]$TaskText,
        [string]$Context
    )

    foreach ($requiredSection in $requiredTaskSections) {
        if (-not $TaskText.Contains($requiredSection)) {
            throw "Task block is missing '$requiredSection': $Context"
        }
    }
}

function Add-ArchiveHistoryLink {
    param(
        [string]$Text,
        [string]$SourceRelativePath
    )

    $normalized = (Normalize-Newlines $Text).TrimEnd()
    if ($normalized.Contains($archiveRelativePath)) {
        return "$normalized`n"
    }

    $line = "- Non-July task blocks from ``$SourceRelativePath`` were moved to ``$archiveRelativePath`` on $ArchiveDate."
    $archiveHeading = [regex]::Match($normalized, '(?m)^## Archived History$')
    if ($archiveHeading.Success) {
        $followingHeadings = @([regex]::Matches($normalized.Substring($archiveHeading.Index + $archiveHeading.Length), '(?m)^## (?!#)[^\r\n]*$'))
        $insertAt = if ($followingHeadings.Count -gt 0) {
            $archiveHeading.Index + $archiveHeading.Length + $followingHeadings[0].Index
        } else {
            $normalized.Length
        }
        $before = $normalized.Substring(0, $insertAt).TrimEnd()
        $after = $normalized.Substring($insertAt).TrimStart("`n")
        $combined = "$before`n`n$line"
        if (-not [string]::IsNullOrWhiteSpace($after)) {
            $combined += "`n`n$after"
        }
        return "$combined`n"
    }

    $firstHeading = [regex]::Match($normalized, '(?m)^## (?!#)[^\r\n]*$')
    $insertAt = if ($firstHeading.Success) { $firstHeading.Index } else { $normalized.Length }
    $before = $normalized.Substring(0, $insertAt).TrimEnd()
    $after = $normalized.Substring($insertAt).TrimStart("`n")
    $section = "## Archived History`n`n$line"
    $combined = if ([string]::IsNullOrWhiteSpace($before)) { $section } else { "$before`n`n$section" }
    if (-not [string]::IsNullOrWhiteSpace($after)) {
        $combined += "`n`n$after"
    }
    return "$combined`n"
}

function Convert-DamageMeterHandoff {
    param([string]$Text)

    $normalized = (Normalize-Newlines $Text).Trim()
    $lines = @($normalized -split "`n")
    if ($lines.Count -eq 0 -or $lines[0] -ne '# Damage Meter UI Code Builder Handoff') {
        throw "Unexpected damage meter handoff heading."
    }

    $body = ($lines[1..($lines.Count - 1)] -join "`n").TrimStart()
    $body = [regex]::Replace($body, '(?m)^## ', '### ')
    $body = $body.Replace('### Inspected evidence', '### Evidence')
    if (-not $body.Contains('### Next Actions')) {
        $historyIndex = $body.IndexOf('### History', [System.StringComparison]::Ordinal)
        if ($historyIndex -lt 0) {
            throw "Damage meter handoff has no History section."
        }
        $nextActions = "### Next Actions`n`n- No active action remains in this archived handoff. The implementation result is recorded in ``boards/UI/RUNSCENE_UI.md`` under the 2026-05-29 damage meter task.`n`n"
        $body = $body.Insert($historyIndex, $nextActions)
    }

    $task = "## Task: Undated Damage Meter UI Code Builder Handoff`n`n$body"
    Assert-TaskFormat $task $damageMeterRelativePath
    return $task.TrimEnd()
}

Assert-WorkspacePath $archivePath
Assert-WorkspacePath $damageMeterPath
if (Test-Path -LiteralPath $archivePath) {
    throw "Target archive already exists: $archiveRelativePath"
}

$sourceFiles = New-Object System.Collections.Generic.List[System.IO.FileInfo]
foreach ($boardDirectory in $boardDirectories) {
    $absoluteDirectory = Join-Path $repositoryRoot $boardDirectory
    foreach ($file in Get-ChildItem -LiteralPath $absoluteDirectory -File -Filter '*.md' | Sort-Object Name) {
        if ($file.FullName -ne $damageMeterPath) {
            $sourceFiles.Add($file)
        }
    }
}

$updatedFiles = @{}
$originalFiles = @{}
$archivedBySource = [ordered]@{}
$keptJulyTaskCount = 0
$archivedTaskCount = 0

foreach ($file in $sourceFiles) {
    $sourceRelativePath = $file.FullName.Substring($repositoryRoot.Length + 1).Replace('/', '\')
    $original = Normalize-Newlines (Read-Utf8Text $file.FullName)
    $originalFiles[$file.FullName] = $original
    $sections = @(Get-LevelTwoSections $original)
    if ($sections.Count -eq 0) {
        continue
    }

    $preamble = $original.Substring(0, $sections[0].Start).TrimEnd()
    $keptSections = New-Object System.Collections.Generic.List[string]
    $archivedSections = New-Object System.Collections.Generic.List[string]
    foreach ($section in $sections) {
        if (-not (Test-IsTaskHeading $section.Heading)) {
            $keptSections.Add($section.Text)
            continue
        }

        if (Test-IsKeptMonthTask $section.Heading) {
            $keptJulyTaskCount++
            $keptSections.Add($section.Text)
            continue
        }

        $completedTask = Complete-TaskFormat $section.Text
        Assert-TaskFormat $completedTask "$sourceRelativePath | $($section.Heading)"
        $archivedTaskCount++
        $archivedSections.Add($completedTask)
    }

    if ($archivedSections.Count -eq 0) {
        continue
    }

    $activeParts = New-Object System.Collections.Generic.List[string]
    if (-not [string]::IsNullOrWhiteSpace($preamble)) {
        $activeParts.Add($preamble)
    }
    foreach ($keptSection in $keptSections) {
        $activeParts.Add($keptSection)
    }
    $activeText = (($activeParts -join "`n`n").TrimEnd() + "`n")
    $activeText = Add-ArchiveHistoryLink $activeText $sourceRelativePath
    $updatedFiles[$file.FullName] = $activeText
    $archivedBySource[$sourceRelativePath] = @($archivedSections)
}

if (-not (Test-Path -LiteralPath $damageMeterPath)) {
    throw "Expected standalone damage meter handoff does not exist: $damageMeterRelativePath"
}
$damageMeterOriginal = Normalize-Newlines (Read-Utf8Text $damageMeterPath)
$originalFiles[$damageMeterPath] = $damageMeterOriginal
$damageMeterTask = Convert-DamageMeterHandoff $damageMeterOriginal
$archivedBySource[$damageMeterRelativePath.Replace('/', '\')] = @($damageMeterTask)
$archivedTaskCount++

$archiveHeader = @"
# Board Cleanup Archive $ArchiveDate

Moved from active boards under COMBAT, DATA, MON, OPS, RUN, and UI during the $ArchiveDate cleanup.
Criteria: keep only task blocks explicitly dated in $KeepMonth in active boards. Archive every earlier dated task and every undated task.
The standalone undated damage meter handoff was normalized into the required task-block format and archived from its former UI path.

- Kept active task blocks: $keptJulyTaskCount
- Archived task blocks: $archivedTaskCount
- Source board files with archived task blocks: $($archivedBySource.Count)
"@.Trim()

$archiveParts = New-Object System.Collections.Generic.List[string]
$archiveParts.Add($archiveHeader)
foreach ($entry in $archivedBySource.GetEnumerator()) {
    $archiveParts.Add("## Source: $($entry.Key)")
    foreach ($task in $entry.Value) {
        $archiveParts.Add($task)
    }
}
$archiveText = (($archiveParts -join "`n`n").TrimEnd() + "`n")

$archiveTasks = @([regex]::Matches($archiveText, '(?m)^## (?:Recent )?Task:[^\r\n]*$'))
if ($archiveTasks.Count -ne $archivedTaskCount) {
    throw "Archive task count mismatch: expected $archivedTaskCount, found $($archiveTasks.Count)."
}

foreach ($match in $archiveTasks) {
    $start = $match.Index
    $nextLevelTwo = [regex]::Match($archiveText.Substring($start + $match.Length), '(?m)^## (?!#)[^\r\n]*$')
    $end = if ($nextLevelTwo.Success) { $start + $match.Length + $nextLevelTwo.Index } else { $archiveText.Length }
    $taskText = $archiveText.Substring($start, $end - $start)
    Assert-TaskFormat $taskText $match.Value
}

"keepMonth=$KeepMonth"
"keptJulyTasks=$keptJulyTaskCount"
"archivedTasks=$archivedTaskCount"
"updatedActiveBoards=$($updatedFiles.Count)"
"archiveSources=$($archivedBySource.Count)"
"standaloneHandoff=$damageMeterRelativePath"
"mode=$(if ($Apply) { 'apply' } else { 'dry-run' })"

if (-not $Apply) {
    exit 0
}

try {
    Write-Utf8Text $archivePath $archiveText
    foreach ($entry in $updatedFiles.GetEnumerator()) {
        Write-Utf8Text $entry.Key $entry.Value
    }
    Remove-Item -LiteralPath $damageMeterPath
} catch {
    foreach ($entry in $originalFiles.GetEnumerator()) {
        Write-Utf8Text $entry.Key $entry.Value
    }
    if (Test-Path -LiteralPath $archivePath) {
        Remove-Item -LiteralPath $archivePath
    }
    throw
}

"archiveWritten=$archiveRelativePath"
"damageMeterHandoffRemoved=$damageMeterRelativePath"
