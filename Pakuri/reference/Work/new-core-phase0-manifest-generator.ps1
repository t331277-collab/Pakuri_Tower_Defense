param(
    [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
)

$ErrorActionPreference = 'Stop'

function Get-ProjectPath
{
    param([string]$AbsolutePath)

    return $AbsolutePath.Substring($WorkspaceRoot.Length + 1).Replace('\', '/')
}

function Get-SerializedDocuments
{
    param([string]$RawText)

    return [regex]::Matches(
        $RawText,
        '(?ms)^--- !u!(?<type>\d+) &(?<id>-?\d+)\r?\n(?<body>.*?)(?=^--- !u!|\z)')
}

function Get-Sha256
{
    param([string]$Text)

    $bytes = [Text.Encoding]::UTF8.GetBytes($Text)
    $hash = [Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
    return ([BitConverter]::ToString($hash)).Replace('-', '').ToLowerInvariant()
}

function Write-Manifest
{
    param(
        [object[]]$Rows,
        [string]$FileName
    )

    $path = Join-Path $PSScriptRoot $FileName
    $Rows | Export-Csv -LiteralPath $path -NoTypeInformation -Encoding UTF8
}

$assetsRoot = Join-Path $WorkspaceRoot 'Pakuri\Assets'
$scriptsRoot = Join-Path $assetsRoot 'Scripts'
$csvRoot = Join-Path $assetsRoot 'CSVdata'

$guidToAssetPath = @{}
Get-ChildItem -LiteralPath $assetsRoot -Recurse -Filter '*.meta' -File | ForEach-Object {
    $raw = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8
    $match = [regex]::Match($raw, '(?m)^guid:\s*([0-9a-f]{32})\s*$')
    if ($match.Success)
    {
        $assetPath = $_.FullName.Substring(0, $_.FullName.Length - '.meta'.Length)
        $guidToAssetPath[$match.Groups[1].Value] = Get-ProjectPath $assetPath
    }
}

$scriptGuidToPath = @{}
Get-ChildItem -LiteralPath $scriptsRoot -Recurse -Filter '*.cs.meta' -File | ForEach-Object {
    $raw = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8
    $match = [regex]::Match($raw, '(?m)^guid:\s*([0-9a-f]{32})\s*$')
    if ($match.Success)
    {
        $scriptGuidToPath[$match.Groups[1].Value] =
            (Get-ProjectPath $_.FullName).Substring(0, (Get-ProjectPath $_.FullName).Length - '.meta'.Length)
    }
}

$serializedExtensions = @(
    '.unity',
    '.prefab',
    '.asset',
    '.controller',
    '.overrideController',
    '.anim',
    '.playable',
    '.mat',
    '.scenetemplate'
)

$serializedFiles = @(
    Get-ChildItem -LiteralPath $assetsRoot -Recurse -File |
        Where-Object { $serializedExtensions -contains $_.Extension }
)

$scriptReferenceRows = [Collections.Generic.List[object]]::new()
$inspectorSnapshotRows = [Collections.Generic.List[object]]::new()

foreach ($file in $serializedFiles)
{
    $assetPath = Get-ProjectPath $file.FullName
    $raw = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    $documents = Get-SerializedDocuments $raw
    $gameObjectNames = @{}

    foreach ($document in $documents)
    {
        if ($document.Groups['type'].Value -ne '1')
        {
            continue
        }

        $nameMatch = [regex]::Match(
            $document.Groups['body'].Value,
            '(?m)^\s*m_Name:\s*(.*?)\s*$')
        if ($nameMatch.Success)
        {
            $gameObjectNames[$document.Groups['id'].Value] = $nameMatch.Groups[1].Value
        }
    }

    foreach ($document in $documents)
    {
        $scriptMatch = [regex]::Match(
            $document.Value,
            'm_Script:\s*\{fileID:\s*11500000,\s*guid:\s*([0-9a-f]{32}),\s*type:\s*3\}')
        if (!$scriptMatch.Success)
        {
            continue
        }

        $scriptGuid = $scriptMatch.Groups[1].Value
        if (!$scriptGuidToPath.ContainsKey($scriptGuid))
        {
            continue
        }

        $gameObjectMatch = [regex]::Match(
            $document.Groups['body'].Value,
            '(?m)^\s*m_GameObject:\s*\{fileID:\s*(-?\d+)\}\s*$')
        $gameObjectFileId = if ($gameObjectMatch.Success) {
            $gameObjectMatch.Groups[1].Value
        } else {
            ''
        }
        $gameObjectName = if (
            $gameObjectFileId -and
            $gameObjectNames.ContainsKey($gameObjectFileId)
        ) {
            $gameObjectNames[$gameObjectFileId]
        } else {
            ''
        }
        $pathScope = if ($assetPath.StartsWith('Pakuri/Assets/Legacy/')) {
            'LegacyPath'
        } else {
            'NonLegacyPath'
        }

        $scriptReferenceRows.Add([pscustomobject][ordered]@{
            AssetPath = $assetPath
            AssetKind = $file.Extension.TrimStart('.')
            PathScope = $pathScope
            ScriptPath = $scriptGuidToPath[$scriptGuid]
            ScriptGuid = $scriptGuid
            ObjectTypeId = $document.Groups['type'].Value
            ComponentFileId = $document.Groups['id'].Value
            GameObjectFileId = $gameObjectFileId
            GameObjectName = $gameObjectName
        })

        $payload = $document.Value.TrimEnd("`r", "`n")
        $inspectorSnapshotRows.Add([pscustomobject][ordered]@{
            AssetPath = $assetPath
            PathScope = $pathScope
            ScriptPath = $scriptGuidToPath[$scriptGuid]
            ScriptGuid = $scriptGuid
            ComponentFileId = $document.Groups['id'].Value
            GameObjectName = $gameObjectName
            PayloadSha256 = Get-Sha256 $payload
            PayloadBase64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($payload))
        })
    }
}

$scriptReferenceRows = @(
    $scriptReferenceRows |
        Sort-Object AssetPath, ComponentFileId, ScriptGuid
)
$inspectorSnapshotRows = @(
    $inspectorSnapshotRows |
        Sort-Object AssetPath, ComponentFileId, ScriptGuid
)

$csvContractRows = @(
    foreach ($file in Get-ChildItem -LiteralPath $csvRoot -Recurse -Filter '*.csv' -File | Sort-Object FullName)
    {
        $lineCount = (Get-Content -LiteralPath $file.FullName -Encoding UTF8).Count
        [pscustomobject][ordered]@{
            CsvPath = Get-ProjectPath $file.FullName
            SizeBytes = $file.Length
            Sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            LinesAfterHeader = [Math]::Max(0, $lineCount - 1)
        }
    }
)

$resourceRows = [Collections.Generic.List[object]]::new()
$resourceQueue = [Collections.Generic.Queue[string]]::new()
$queuedResources = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$visitedSerializedResources = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$resourceEdgeKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

function Add-ResourceQueue
{
    param([string]$ProjectPath)

    if (
        $ProjectPath -and
        (Test-Path -LiteralPath (Join-Path $WorkspaceRoot $ProjectPath)) -and
        $queuedResources.Add($ProjectPath)
    )
    {
        $resourceQueue.Enqueue($ProjectPath)
    }
}

foreach ($assetPath in @(
    $scriptReferenceRows |
        Where-Object { $_.PathScope -eq 'NonLegacyPath' } |
        Select-Object -ExpandProperty AssetPath -Unique
))
{
    $resourceRows.Add([pscustomobject][ordered]@{
        SourceKind = 'MigrationRoot'
        SourcePath = $assetPath
        SourceProperty = ''
        ReferenceValue = ''
        RetainedAssetPath = $assetPath
        AssetKind = [IO.Path]::GetExtension($assetPath).TrimStart('.')
        Exists = $true
    })
    Add-ResourceQueue $assetPath
}

foreach ($csvFile in Get-ChildItem -LiteralPath $csvRoot -Recurse -Filter '*.csv' -File | Sort-Object FullName)
{
    $csvPath = Get-ProjectPath $csvFile.FullName
    $rows = @(Import-Csv -LiteralPath $csvFile.FullName -Encoding UTF8)
    $dataRowIndex = 0
    foreach ($row in @($rows | Select-Object -Skip 1))
    {
        $dataRowIndex++
        foreach ($property in $row.PSObject.Properties)
        {
            if (
                $property.Name -notmatch '(?i)_path$' -or
                [string]::IsNullOrWhiteSpace([string]$property.Value)
            )
            {
                continue
            }

            $rawPath = ([string]$property.Value).Trim()
            $projectPath = if ($rawPath.StartsWith('Pakuri/Assets/')) {
                $rawPath
            } elseif ($rawPath.StartsWith('Assets/')) {
                "Pakuri/$rawPath"
            } else {
                ''
            }
            $exists = $projectPath -and (Test-Path -LiteralPath (Join-Path $WorkspaceRoot $projectPath))

            $resourceRows.Add([pscustomobject][ordered]@{
                SourceKind = 'CsvPath'
                SourcePath = "$csvPath#data-row-$dataRowIndex"
                SourceProperty = $property.Name
                ReferenceValue = $rawPath
                RetainedAssetPath = $projectPath
                AssetKind = if ($projectPath) {
                    [IO.Path]::GetExtension($projectPath).TrimStart('.')
                } else {
                    ''
                }
                Exists = [bool]$exists
            })

            if ($exists)
            {
                Add-ResourceQueue $projectPath
            }
        }
    }
}

while ($resourceQueue.Count -gt 0)
{
    $sourcePath = $resourceQueue.Dequeue()
    $sourceExtension = [IO.Path]::GetExtension($sourcePath)
    if (
        $serializedExtensions -notcontains $sourceExtension -or
        !$visitedSerializedResources.Add($sourcePath)
    )
    {
        continue
    }

    $absoluteSourcePath = Join-Path $WorkspaceRoot $sourcePath
    $raw = Get-Content -LiteralPath $absoluteSourcePath -Raw -Encoding UTF8
    foreach ($match in [regex]::Matches($raw, 'guid:\s*([0-9a-f]{32})'))
    {
        $guid = $match.Groups[1].Value
        if (!$guidToAssetPath.ContainsKey($guid))
        {
            continue
        }

        $targetPath = $guidToAssetPath[$guid]
        $targetExtension = [IO.Path]::GetExtension($targetPath)
        if ($targetExtension -in @('.cs', '.asmdef', '.dll'))
        {
            continue
        }

        $edgeKey = "$sourcePath|$guid|$targetPath"
        if (!$resourceEdgeKeys.Add($edgeKey))
        {
            continue
        }

        $resourceRows.Add([pscustomobject][ordered]@{
            SourceKind = 'SerializedGuid'
            SourcePath = $sourcePath
            SourceProperty = 'guid'
            ReferenceValue = $guid
            RetainedAssetPath = $targetPath
            AssetKind = $targetExtension.TrimStart('.')
            Exists = $true
        })
        Add-ResourceQueue $targetPath
    }
}

$resourceRows = @(
    $resourceRows |
        Sort-Object SourceKind, SourcePath, SourceProperty, ReferenceValue, RetainedAssetPath
)

Write-Manifest $csvContractRows 'new-core-phase0-csv-contract-manifest.csv'
Write-Manifest $scriptReferenceRows 'new-core-phase0-script-reference-manifest.csv'
Write-Manifest $resourceRows 'new-core-phase0-retained-resource-manifest.csv'
Write-Manifest $inspectorSnapshotRows 'new-core-phase0-inspector-snapshot.csv'

[pscustomobject][ordered]@{
    SerializedFiles = $serializedFiles.Count
    CsvContracts = $csvContractRows.Count
    ScriptReferences = $scriptReferenceRows.Count
    UniqueScriptGuids = @($scriptReferenceRows.ScriptGuid | Sort-Object -Unique).Count
    UniqueScriptReferenceAssets = @($scriptReferenceRows.AssetPath | Sort-Object -Unique).Count
    ResourceRows = $resourceRows.Count
    UniqueRetainedResources = @(
        $resourceRows.RetainedAssetPath |
            Where-Object { $_ } |
            Sort-Object -Unique
    ).Count
    MissingCsvPathResources = @(
        $resourceRows |
            Where-Object { $_.SourceKind -eq 'CsvPath' -and $_.Exists -ne $true }
    ).Count
    InspectorSnapshots = $inspectorSnapshotRows.Count
} | Format-List
