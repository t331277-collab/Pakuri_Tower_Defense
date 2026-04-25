[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$CodexCommand,
    [Parameter(Mandatory = $true)][string]$CodeXRoot,
    [Parameter(Mandatory = $true)][string]$PromptFile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)

function Assert-PathExists {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Label was not found: $Path"
    }
}

function Resolve-NativeCodexExecutable {
    param([Parameter(Mandatory = $true)][string]$PreferredPath)

    if ((Test-Path -LiteralPath $PreferredPath) -and ([System.IO.Path]::GetExtension($PreferredPath)).Equals('.exe', [System.StringComparison]::OrdinalIgnoreCase)) {
        return $PreferredPath
    }

    try {
        $command = Get-Command codex -ErrorAction Stop
        if ($command.CommandType -eq 'Application' -and (Test-Path -LiteralPath $command.Source)) {
            return $command.Source
        }
    } catch {
        # Fall through to the VS Code extension scan below.
    }

    $extensions = Join-Path $env:USERPROFILE '.vscode\extensions'
    if (Test-Path -LiteralPath $extensions) {
        $candidate = Get-ChildItem -LiteralPath $extensions -Directory -Filter 'openai.chatgpt-*' |
            ForEach-Object { Join-Path $_.FullName 'bin\windows-x86_64\codex.exe' } |
            Where-Object { Test-Path -LiteralPath $_ } |
            Sort-Object FullName -Descending |
            Select-Object -First 1

        if ($candidate) {
            return $candidate
        }
    }

    throw "A native codex.exe launch target was not found. Preferred path: $PreferredPath"
}

function ConvertTo-NativeArgument {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Value)

    if ($Value.Length -eq 0) {
        return '""'
    }

    $needsQuotes = $Value.IndexOfAny([char[]]@(' ', "`t", "`n", "`v", '"')) -ge 0
    if (-not $needsQuotes) {
        return $Value
    }

    $builder = New-Object System.Text.StringBuilder
    [void]$builder.Append('"')
    $slashCount = 0

    foreach ($char in $Value.ToCharArray()) {
        if ($char -eq '\') {
            $slashCount++
            continue
        }

        if ($char -eq '"') {
            if ($slashCount -gt 0) {
                [void]$builder.Append(('\' * ($slashCount * 2)))
            }
            [void]$builder.Append('\"')
            $slashCount = 0
            continue
        }

        if ($slashCount -gt 0) {
            [void]$builder.Append(('\' * $slashCount))
            $slashCount = 0
        }

        [void]$builder.Append($char)
    }

    if ($slashCount -gt 0) {
        [void]$builder.Append(('\' * ($slashCount * 2)))
    }

    [void]$builder.Append('"')
    return $builder.ToString()
}

Assert-PathExists -Path $CodeXRoot -Label 'CodeXRoot'
Assert-PathExists -Path $PromptFile -Label 'PromptFile'
Assert-PathExists -Path $CodexCommand -Label 'CodexCommand'

$launchTarget = Resolve-NativeCodexExecutable -PreferredPath $CodexCommand
$prompt = [System.IO.File]::ReadAllText($PromptFile, [System.Text.Encoding]::UTF8)
$argumentLine = [string]::Join(' ', (@('--cd', $CodeXRoot, $prompt) | ForEach-Object { ConvertTo-NativeArgument $_ }))

$processInfo = New-Object System.Diagnostics.ProcessStartInfo
$processInfo.FileName = $launchTarget
$processInfo.WorkingDirectory = $CodeXRoot
$processInfo.UseShellExecute = $false
$processInfo.Arguments = $argumentLine

$process = [System.Diagnostics.Process]::Start($processInfo)
if ($null -eq $process) {
    throw "Codex process failed to start: $launchTarget"
}
$process.WaitForExit()

exit $process.ExitCode
