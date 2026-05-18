param(
    [string]$ProjectRoot = "C:\TowerDefence_Pakuri\Test",
    [string]$OutputPath = "Pakuri\reference\Report\2026-05-18-csharp-script-role-map.html"
)

$repoRoot = (Resolve-Path $ProjectRoot).Path
$assetsRoot = Join-Path $repoRoot "Pakuri\Assets"
$outputFullPath = Join-Path $repoRoot $OutputPath

$textAssetExtensions = @(
    ".unity", ".prefab", ".asset", ".mat", ".anim", ".controller",
    ".overrideController", ".playable", ".renderTexture", ".inputactions", ".asmdef"
)

$scriptFiles = Get-ChildItem -Path $assetsRoot -Recurse -Filter *.cs | Sort-Object FullName
$assetFiles = Get-ChildItem -Path $assetsRoot -Recurse -File |
    Where-Object { $textAssetExtensions -contains $_.Extension } |
    Sort-Object FullName

$scriptTexts = @{}
foreach ($file in $scriptFiles)
{
    $scriptTexts[$file.FullName] = Get-Content -Raw -LiteralPath $file.FullName
}

function Escape-Html([string]$value)
{
    if ($null -eq $value) { return "" }
    return [System.Net.WebUtility]::HtmlEncode($value)
}

function Get-RelativeRepoPath([string]$fullPath)
{
    return $fullPath.Substring($repoRoot.Length + 1).Replace("/", "\")
}

function Get-FolderKey([string]$relativePath)
{
    $parts = $relativePath.Split("\")
    if ($parts.Length -le 2) { return "(root)" }
    return ($parts[2..($parts.Length - 2)] -join "\")
}

function Get-TypeMatches([string]$text)
{
    $pattern = '(?m)^\s*(?:\[.*\]\s*)*(?:public|internal|private|protected)?\s*(?:sealed\s+|abstract\s+|static\s+|partial\s+)*\b(class|struct|interface|enum)\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?:\:\s*([^{\r\n]+))?'
    return [regex]::Matches($text, $pattern)
}

function Get-PrimaryTypeInfo([string]$text, [string]$fallbackName)
{
    $matches = Get-TypeMatches $text
    if ($matches.Count -eq 0)
    {
        return [pscustomobject]@{
            Kind = "unknown"
            Name = $fallbackName
            Bases = @()
            AllTypes = @($fallbackName)
        }
    }

    $allTypes = @()
    foreach ($match in $matches)
    {
        $allTypes += $match.Groups[2].Value
    }

    $bases = @()
    if ($matches[0].Groups[3].Success)
    {
        $bases = $matches[0].Groups[3].Value.Split(",") |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ }
    }

    return [pscustomobject]@{
        Kind = $matches[0].Groups[1].Value
        Name = $matches[0].Groups[2].Value
        Bases = $bases
        AllTypes = $allTypes
    }
}

function Get-MethodNames([string]$text)
{
    $pattern = '(?m)^\s*(?:public|internal|private|protected)\s+(?:static\s+|virtual\s+|override\s+|sealed\s+|abstract\s+|partial\s+|async\s+)*[A-Za-z0-9_<>\[\],\.\?]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\('
    $blocked = @("if", "for", "foreach", "while", "switch", "catch", "lock", "using", "return")
    $names = New-Object System.Collections.Generic.List[string]

    foreach ($match in [regex]::Matches($text, $pattern))
    {
        $name = $match.Groups[1].Value
        if ($blocked -contains $name) { continue }
        if (-not $names.Contains($name)) { $names.Add($name) }
    }

    return $names.ToArray()
}

function Get-CodeReferenceCount([string]$typeName, [string]$selfPath)
{
    if ([string]::IsNullOrWhiteSpace($typeName)) { return 0 }

    $pattern = "\b$([regex]::Escape($typeName))\b"
    $count = 0
    foreach ($entry in $scriptTexts.GetEnumerator())
    {
        if ($entry.Key -eq $selfPath) { continue }
        if ([regex]::IsMatch($entry.Value, $pattern))
        {
            $count++
        }
    }

    return $count
}

function Get-AssetReferenceCount([string]$scriptPath)
{
    $metaPath = "$scriptPath.meta"
    if (-not (Test-Path -LiteralPath $metaPath)) { return 0 }

    $metaText = Get-Content -Raw -LiteralPath $metaPath
    $guidMatch = [regex]::Match($metaText, '(?m)^guid:\s*([a-f0-9]+)\s*$')
    if (-not $guidMatch.Success) { return 0 }

    $guid = $guidMatch.Groups[1].Value
    $count = 0
    foreach ($asset in $assetFiles)
    {
        if (Select-String -LiteralPath $asset.FullName -Pattern $guid -Quiet)
        {
            $count++
        }
    }

    return $count
}

function Get-Role(
    [string]$relativePath,
    [string]$typeName,
    [string[]]$bases
)
{
    $baseText = ($bases -join ", ")
    $folder = Get-FolderKey $relativePath

    if ($baseText -match '\bMonoBehaviour\b')
    {
        if ($relativePath -match '\\UI\\')
        {
            if ($typeName -match 'Manager$') { return "Scene-bound runtime manager" }
            return "Scene-bound UI component"
        }

        if ($typeName -match 'Actor')
        {
            return "Scene or prefab actor component"
        }

        if ($typeName -match 'Manager$')
        {
            return "Scene-bound runtime manager"
        }

        return "Scene-bound MonoBehaviour component"
    }

    if ($baseText -match '\bScriptableObject\b')
    {
        return "ScriptableObject definition or catalog"
    }

    if ($relativePath -match '\\Editor\\' -or $baseText -match '\bEditor\b|\bAssetPostprocessor\b')
    {
        return "Unity Editor automation or tooling"
    }

    if ($folder -match 'InGame\\Combat')
    {
        if ($typeName -match 'Calculator') { return "Combat calculation service" }
        return "Combat stat or calculation model"
    }

    if ($folder -match 'InGame\\Run')
    {
        if ($typeName -match 'Session') { return "Run session state" }
        return "Run state model"
    }

    if ($folder -match 'InGame\\Skills\\Execution')
    {
        if ($typeName -match 'Context|Result|Snapshot|Record|Spec') { return "Skill execution data contract" }
        if ($typeName -match 'Resolver') { return "Skill choice or result resolver" }
        if ($typeName -match 'System|Executor|Registry|Actor|Button') { return "Skill execution logic" }
        return "Skill execution helper"
    }

    if ($folder -match 'InGame\\Skills\\Runtime')
    {
        return "Per-unit skill runtime state"
    }

    if ($folder -match 'InGame\\Skills\\Data')
    {
        if ($typeName -match 'Validator') { return "Data validation service" }
        return "Skill data definition"
    }

    if ($folder -match 'InGame\\Units')
    {
        if ($typeName -match 'Factory') { return "Unit creation service" }
        if ($typeName -match 'Service') { return "Unit registration or lookup service" }
        if ($typeName -match 'ActorView') { return "Unit presentation or binding layer" }
        return "Unit runtime state model"
    }

    if ($folder -match 'InGame\\Core')
    {
        if ($typeName -match 'Context|Result') { return "Core context or result slot" }
        if ($typeName -match 'Cooldown|Targeting') { return "Core rules helper" }
        if ($typeName -match 'System|Executor|Service') { return "Core runtime service" }
        return "Core support type"
    }

    if ($folder -match 'InGame\\Data\\Runtime\\Csv')
    {
        return "CSV runtime data layer"
    }

    if ($folder -match 'InGame\\Data\\Runtime')
    {
        return "Runtime data catalog"
    }

    if ($folder -match 'InGame\\Data\\Definition')
    {
        return "Definition or catalog support type"
    }

    return "General support code"
}

function Get-Importance(
    [int]$assetRefs,
    [int]$codeRefs,
    [int]$lineCount,
    [string]$role,
    [string[]]$bases
)
{
    $baseText = ($bases -join ", ")

    if ($assetRefs -ge 5 -or $codeRefs -ge 40)
    {
        return "Critical"
    }

    if ($baseText -match '\bMonoBehaviour\b|\bScriptableObject\b')
    {
        if ($assetRefs -ge 1 -or $codeRefs -ge 8 -or $lineCount -ge 220)
        {
            return "High"
        }
    }

    if ($role -match 'Editor' -and $lineCount -ge 250)
    {
        return "High"
    }

    if ($codeRefs -ge 15 -or $lineCount -ge 250)
    {
        return "High"
    }

    if ($codeRefs -ge 5 -or $lineCount -ge 90)
    {
        return "Medium"
    }

    return "Low"
}

function Get-Integration(
    [int]$assetRefs,
    [int]$codeRefs,
    [int]$lineCount,
    [string]$relativePath,
    [string]$typeName,
    [string[]]$bases
)
{
    $baseText = ($bases -join ", ")

    if ($assetRefs -gt 0 -or $baseText -match '\bMonoBehaviour\b|\bScriptableObject\b')
    {
        return "Low"
    }

    if ($relativePath -match 'PakuriCsvRuntimeData\..*\.cs$')
    {
        return "Medium"
    }

    if ($typeName -match 'Context|Result|Identity|Stats|Resource|Model|Resolver|Registry|Parser|Record|Spec|Cooldown|Targeting|Factory|Service|Data$')
    {
        if ($codeRefs -le 2 -and $lineCount -le 220)
        {
            return "High"
        }
    }

    if ($codeRefs -le 2 -and $lineCount -le 120)
    {
        return "High"
    }

    if ($codeRefs -le 12 -and $lineCount -le 260)
    {
        return "Medium"
    }

    return "Low"
}

function Get-MediumPlan(
    [string]$relativePath,
    [string]$folderKey
)
{
    $map = @{
        "Pakuri\Assets\Scripts2\InGame\Combat\DamageCalculator.cs" = "Keep this as the anchor file; the former CombatStatModels.cs support types are already nested inside it."
        "Pakuri\Assets\Scripts2\InGame\Core\EnemyTargeting.cs" = "Fold into EnemyCombatSystem.cs as a targeting helper."
        "Pakuri\Assets\Scripts2\InGame\Skills\Runtime\SkillRuntimeFactory.cs" = "Bundle with UnitSkillRuntimeSet and SkillRuntimeInstance under a smaller SkillRuntime.cs family."
        "Pakuri\Assets\Scripts2\InGame\Skills\Runtime\SkillRuntimeInstance.cs" = "Move next to SkillRuntimeFactory.cs in a runtime bundle, leaving only the public factory surface separate."
        "Pakuri\Assets\Scripts2\InGame\Skills\Runtime\UnitSkillRuntimeSet.cs" = "Merge into SkillRuntimeFactory.cs or a SkillRuntime.Types.cs companion."
        "Pakuri\Assets\Scripts2\InGame\Units\UnitDefenseRuntime.cs" = "Move into BaseUnitRuntimeModel.cs support types or a UnitRuntime.Types.cs companion."
        "Pakuri\Assets\Scripts2\InGame\Units\UnitStateBucket.cs" = "Move into the same runtime-types bundle as BaseUnitRuntimeModel.cs."
        "Pakuri\Assets\Scripts2\InGame\Units\UnitFactory.cs" = "Keep as the anchor file and continue pulling small unit-creation helpers into it."
        "Pakuri\Assets\Scripts2\InGame\Units\UnitRosterService.cs" = "Fold into InGameCombatManager.cs if manager-centric ownership is preferred, or keep it as the anchor for roster-specific helpers."
        "Pakuri\Assets\Scripts2\InGame\Data\Definition\SkillDefinition.cs" = "Move definition-only DTOs next to SkillData.cs or InGameSkillCatalog.cs, depending on who owns the schema long-term."
        "Pakuri\Assets\Scripts2\InGame\Data\Editor\PakuriCsvRuntimeCatalogPostprocessor.cs" = "Fold into the CSV runtime catalog editor sync entrypoint or an Editor utility bundle."
        "Pakuri\Assets\Scripts2\InGame\Data\Editor\PakuriSkillEffectPrefabCsvExporter.cs" = "Either split MonsterDefinitionEditor out cleanly or keep this as the editor-side export hub."
    }

    if ($map.ContainsKey($relativePath))
    {
        return $map[$relativePath]
    }

    if ($relativePath -match 'PakuriCsvRuntimeData\..*\.cs$')
    {
        return "Consolidate small partials by dataset concern to reduce the PakuriCsvRuntimeData.* file count."
    }

    if ($folderKey -match 'InGame\\Skills\\Execution')
    {
        return "Use SkillExecutionSystem.cs as the anchor and move DTO-only types into an Execution.Types.cs companion if needed."
    }

    if ($folderKey -match 'InGame\\Skills\\Data')
    {
        return "Use SkillData.cs as the anchor and keep only large validator or editor hooks split out."
    }

    if ($folderKey -match 'InGame\\Units')
    {
        return "Bundle model-only types into BaseUnitRuntimeModel.cs or a UnitRuntime.Types.cs companion."
    }

    if ($folderKey -match 'InGame\\Core')
    {
        return "Merge into the nearest core owner file or a focused Core.Types.cs helper file."
    }

    return "Merge into the strongest owner in the same domain, or consolidate into a .Types.cs companion instead of a monolith."
}

$rows = New-Object System.Collections.Generic.List[object]
foreach ($file in $scriptFiles)
{
    $text = $scriptTexts[$file.FullName]
    $relativePath = Get-RelativeRepoPath $file.FullName
    $folderKey = Get-FolderKey $relativePath
    $lineCount = ($text -split "`r?`n").Count
    $typeInfo = Get-PrimaryTypeInfo $text ([IO.Path]::GetFileNameWithoutExtension($file.Name))
    $methodNames = Get-MethodNames $text
    $codeRefs = Get-CodeReferenceCount $typeInfo.Name $file.FullName
    $assetRefs = Get-AssetReferenceCount $file.FullName
    $role = Get-Role $relativePath $typeInfo.Name $typeInfo.Bases
    $importance = Get-Importance $assetRefs $codeRefs $lineCount $role $typeInfo.Bases
    $integration = Get-Integration $assetRefs $codeRefs $lineCount $relativePath $typeInfo.Name $typeInfo.Bases
    $mediumPlan = if ($integration -eq "Medium") { Get-MediumPlan $relativePath $folderKey } else { "" }

    $methodPreview = if ($methodNames.Count -gt 0) { ($methodNames | Select-Object -First 4) -join ", " } else { "no public or internal methods detected" }
    $typesPreview = if ($typeInfo.AllTypes.Count -gt 0) { ($typeInfo.AllTypes | Select-Object -First 4) -join ", " } else { $typeInfo.Name }
    $basesPreview = if ($typeInfo.Bases.Count -gt 0) { ($typeInfo.Bases | Select-Object -First 3) -join ", " } else { "" }
    $evidence = "types: $typesPreview"
    if ($basesPreview) { $evidence += " | bases: $basesPreview" }
    $evidence += " | methods: $methodPreview | refs: code $codeRefs / assets $assetRefs"

    $rows.Add([pscustomobject]@{
        RelativePath = $relativePath
        FolderKey = $folderKey
        LineCount = $lineCount
        PrimaryType = $typeInfo.Name
        Role = $role
        Importance = $importance
        Integration = $integration
        Evidence = $evidence
        MediumPlan = $mediumPlan
        CodeRefs = $codeRefs
        AssetRefs = $assetRefs
    })
}

$importanceCounts = @{
    Critical = ($rows | Where-Object Importance -eq "Critical").Count
    High = ($rows | Where-Object Importance -eq "High").Count
    Medium = ($rows | Where-Object Importance -eq "Medium").Count
    Low = ($rows | Where-Object Importance -eq "Low").Count
}

$integrationCounts = @{
    Low = ($rows | Where-Object Integration -eq "Low").Count
    Medium = ($rows | Where-Object Integration -eq "Medium").Count
    High = ($rows | Where-Object Integration -eq "High").Count
}

$completedHighIntegrations = @(
    [pscustomobject]@{ Removed = "RunDayModel.cs"; Owner = "RunSession.cs"; Note = "RunSession now owns the run-day value type directly." }
    [pscustomobject]@{ Removed = "UnitIdentity.cs / UnitStatsRuntime.cs / UnitResourceRuntime.cs / UnitRuntimeModel.cs"; Owner = "BaseUnitRuntimeModel.cs"; Note = "Identity, stats, resource state, and the derived runtime alias were bundled into the runtime base file." }
    [pscustomobject]@{ Removed = "StageOneEnemyPassiveStatApplier.cs"; Owner = "UnitFactory.cs"; Note = "The CreateEnemy-only helper moved into UnitFactory private static methods." }
    [pscustomobject]@{ Removed = "CombatStatModels.cs"; Owner = "DamageCalculator.cs"; Note = "AttributeDefenseSet, CombatStatBlock, and DefenseBreakdown were absorbed into DamageCalculator.cs as nested support types." }
    [pscustomobject]@{ Removed = "BuffSkillData.cs / SkillBlueprintSpecs.cs"; Owner = "SkillData.cs"; Note = "Skill schema and concrete data leaf types are now in one bundle." }
    [pscustomobject]@{ Removed = "StatusEffectData.cs"; Owner = "StatusEffectKind.cs"; Note = "Status-effect kind and transient status-effect data are now co-located." }
    [pscustomobject]@{ Removed = "SkillChoiceModifierCsvParser.cs"; Owner = "SkillChoiceModifierLibrary.cs"; Note = "The CSV loader moved into the modifier library file." }
    [pscustomobject]@{ Removed = "SkillChoiceResolver.cs / SkillExecutorRegistry.cs"; Owner = "SkillExecutionSystem.cs"; Note = "Execution-system-only helpers were absorbed into the execution-system file." }
    [pscustomobject]@{ Removed = "InGameSkillDataValidationMenu.cs"; Owner = "InGameSkillDataValidator.cs"; Note = "Validator logic and the editor menu entry now live together." }
    [pscustomobject]@{ Removed = "StartContext.cs / InGameContextManager.cs / InGameResultManager.cs"; Owner = "SceneEntryManager.cs"; Note = "The scene-entry handoff and empty placeholder files were pulled into the scene-entry owner." }
    [pscustomobject]@{ Removed = "UnitResourceMutationService.cs"; Owner = "InGameCombatManager.cs"; Note = "Resource mutation helpers now live next to the combat-manager public API." }
    [pscustomobject]@{ Removed = "EnemySkillRuntime.cs / EnemySkillCooldown.cs"; Owner = "EnemyCombatSystem.cs"; Note = "Enemy skill resolved-contract types and cooldown-rule helpers now live inside EnemyCombatSystem.cs." }
    [pscustomobject]@{ Removed = "EnemySkillExecutor.cs / EnemyCombatSimulationSystem.cs"; Owner = "EnemyCombatSystem.cs"; Note = "Enemy combat execution logic and the enemy combat loop were consolidated into the shorter EnemyCombatSystem.cs owner." }
    [pscustomobject]@{ Removed = "OfferingUI.cs / MenifestUI.cs"; Owner = "InGameUIManager.cs"; Note = "Run-scene UI flow helpers now live next to the owning UI manager." }
)

$rowsByFolder = $rows | Group-Object FolderKey | Sort-Object Name
$mediumRows = $rows | Where-Object Integration -eq "Medium" | Sort-Object RelativePath

$html = New-Object System.Text.StringBuilder
[void]$html.AppendLine('<!DOCTYPE html>')
[void]$html.AppendLine('<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">')
[void]$html.AppendLine('<title>Pakuri C# Script Role Map</title>')
[void]$html.AppendLine("<style>:root{--bg:#f4efe6;--panel:#fffaf2;--ink:#1e1b18;--muted:#6d6255;--line:#d5c7b3;--critical:#7f1d1d;--high:#b45309;--medium:#166534;--low:#475569;--int-low:#7c2d12;--int-medium:#1d4ed8;--int-high:#065f46}*{box-sizing:border-box}body{margin:0;font-family:'Segoe UI',system-ui,sans-serif;background:linear-gradient(180deg,#efe6d7 0%,#f8f5ef 45%,#ece7de 100%);color:var(--ink);line-height:1.45}main{max-width:1680px;margin:0 auto;padding:32px 24px 64px}h1{font-size:32px;margin:0 0 8px}h2{margin:0 0 10px;font-size:22px}p{margin:8px 0}.hero,.panel,details{background:rgba(255,250,242,.92);border:1px solid var(--line);border-radius:12px;box-shadow:0 12px 30px rgba(30,27,24,.08)}.hero{padding:24px}.panel{padding:18px;margin-top:18px}.stats{display:grid;grid-template-columns:repeat(auto-fit,minmax(180px,1fr));gap:12px;margin-top:18px}.stat{padding:14px;border:1px solid var(--line);border-radius:10px;background:#fffdf8}.stat strong{display:block;font-size:24px}.legend{display:flex;flex-wrap:wrap;gap:10px;margin-top:14px}.pill{display:inline-block;padding:4px 10px;border-radius:999px;font-size:12px;font-weight:700}.critical{background:#fee2e2;color:var(--critical)}.high{background:#ffedd5;color:var(--high)}.medium{background:#dcfce7;color:var(--medium)}.low{background:#e2e8f0;color:var(--low)}.intlow{background:#ffedd5;color:var(--int-low)}.intmedium{background:#dbeafe;color:var(--int-medium)}.inthigh{background:#d1fae5;color:var(--int-high)}nav{display:flex;flex-wrap:wrap;gap:8px;margin:20px 0}nav a{text-decoration:none;color:var(--ink);padding:8px 12px;border-radius:999px;border:1px solid var(--line);background:#fffdf8}table{width:100%;border-collapse:collapse;font-size:13px}th,td{padding:10px 12px;vertical-align:top;border-bottom:1px solid var(--line);text-align:left}th{position:sticky;top:0;background:#f9f3e8;z-index:1}.table-wrap{overflow:auto}details{margin-top:18px;overflow:hidden}summary{cursor:pointer;list-style:none;padding:16px 18px;font-weight:700;background:#f8f1e4}summary::-webkit-details-marker{display:none}code{font-family:Consolas,monospace;font-size:12px}.muted{color:var(--muted)}ul{margin:8px 0 0 18px;padding:0}.footer{margin-top:24px;font-size:13px;color:var(--muted)}</style></head><body><main>")
[void]$html.AppendLine('<section class="hero">')
[void]$html.AppendLine('<h1>Pakuri C# Script Role Map</h1>')
[void]$html.AppendLine("<p>Evidence scope: all current C# scripts under <code>Pakuri/Assets</code> ($($rows.Count) files). Each row is based on the actual file contents: declared types, representative methods, code reference count, and Unity asset reference count.</p>")
[void]$html.AppendLine('<p class="muted">The previous Integration = High candidates were merged in this Code Builder pass. The remaining Integration = Medium candidates now include an explicit merge direction in the report.</p>')
[void]$html.AppendLine('<div class="stats">')
[void]$html.AppendLine("<div class='stat'><span class='muted'>Total scripts</span><strong>$($rows.Count)</strong></div>")
[void]$html.AppendLine("<div class='stat'><span class='muted'>Critical</span><strong>$($importanceCounts.Critical)</strong></div>")
[void]$html.AppendLine("<div class='stat'><span class='muted'>High</span><strong>$($importanceCounts.High)</strong></div>")
[void]$html.AppendLine("<div class='stat'><span class='muted'>Medium</span><strong>$($importanceCounts.Medium)</strong></div>")
[void]$html.AppendLine("<div class='stat'><span class='muted'>Low</span><strong>$($importanceCounts.Low)</strong></div>")
[void]$html.AppendLine("<div class='stat'><span class='muted'>Integration Low</span><strong>$($integrationCounts.Low)</strong></div>")
[void]$html.AppendLine("<div class='stat'><span class='muted'>Integration Medium</span><strong>$($integrationCounts.Medium)</strong></div>")
[void]$html.AppendLine("<div class='stat'><span class='muted'>Integration High</span><strong>$($integrationCounts.High)</strong></div>")
[void]$html.AppendLine('</div>')
[void]$html.AppendLine('<div class="legend"><span class="pill critical">Critical</span><span class="pill high">High</span><span class="pill medium">Medium</span><span class="pill low">Low</span><span class="pill intlow">Integration Low</span><span class="pill intmedium">Integration Medium</span><span class="pill inthigh">Integration High</span></div>')
[void]$html.AppendLine('</section>')

[void]$html.AppendLine('<section class="panel"><h2>Completed High Integrations</h2><div class="table-wrap"><table><thead><tr><th>Removed Files</th><th>Integrated Into</th><th>Result</th></tr></thead><tbody>')
foreach ($entry in $completedHighIntegrations)
{
    [void]$html.AppendLine("<tr><td><code>$(Escape-Html $entry.Removed)</code></td><td><code>$(Escape-Html $entry.Owner)</code></td><td>$(Escape-Html $entry.Note)</td></tr>")
}
[void]$html.AppendLine('</tbody></table></div></section>')

[void]$html.AppendLine('<section class="panel"><h2>Remaining Medium Integration Plan</h2><div class="table-wrap"><table><thead><tr><th>Script</th><th>Role</th><th>Evidence</th><th>Proposed Integration</th></tr></thead><tbody>')
foreach ($row in $mediumRows)
{
    [void]$html.AppendLine("<tr><td><code>$(Escape-Html $row.RelativePath)</code></td><td>$(Escape-Html $row.Role)</td><td>$(Escape-Html $row.Evidence)</td><td>$(Escape-Html $row.MediumPlan)</td></tr>")
}
[void]$html.AppendLine('</tbody></table></div></section>')

[void]$html.AppendLine('<nav>')
foreach ($group in $rowsByFolder)
{
    $anchor = ($group.Name -replace '[^A-Za-z0-9]+', '-')
    [void]$html.AppendLine("<a href='#$anchor'>$(Escape-Html $group.Name)</a>")
}
[void]$html.AppendLine('</nav>')

foreach ($group in $rowsByFolder)
{
    $anchor = ($group.Name -replace '[^A-Za-z0-9]+', '-')
    [void]$html.AppendLine("<details open id='$anchor'><summary>$(Escape-Html $group.Name) <span class='muted'>($($group.Count) files)</span></summary><div class='table-wrap'><table><thead><tr><th>Script</th><th>Primary Type</th><th>Role</th><th>Importance</th><th>Integration</th><th>Evidence</th><th>Medium Plan</th></tr></thead><tbody>")
    foreach ($row in ($group.Group | Sort-Object RelativePath))
    {
        $importanceClass = $row.Importance.ToLowerInvariant()
        $integrationClass = switch ($row.Integration)
        {
            "Low" { "intlow" }
            "Medium" { "intmedium" }
            default { "inthigh" }
        }

        $mediumCell = if ($row.Integration -eq "Medium") { Escape-Html $row.MediumPlan } else { "<span class='muted'>-</span>" }
        $scriptCell = "<code>$(Escape-Html $row.RelativePath)</code><br><span class='muted'>$($row.LineCount) lines</span>"
        [void]$html.AppendLine("<tr><td>$scriptCell</td><td><strong>$(Escape-Html $row.PrimaryType)</strong></td><td>$(Escape-Html $row.Role)</td><td><span class='pill $importanceClass'>$(Escape-Html $row.Importance)</span></td><td><span class='pill $integrationClass'>$(Escape-Html $row.Integration)</span></td><td>$(Escape-Html $row.Evidence)</td><td>$mediumCell</td></tr>")
    }
    [void]$html.AppendLine('</tbody></table></div></details>')
}

[void]$html.AppendLine('<section class="panel footer">')
[void]$html.AppendLine('<p>Verification notes:</p>')
[void]$html.AppendLine('<ul>')
[void]$html.AppendLine('<li><code>Pakuri/Assembly-CSharp.csproj</code> and <code>Pakuri/Assembly-CSharp-Editor.csproj</code> both built successfully after the integrations.</li>')
[void]$html.AppendLine('<li>The Unity console no longer showed C# compile errors at inspection time. The remaining console entries were a <code>UnityEditor.Graphs.Edge.WakeUp()</code> NullReferenceException and MCP client exit logs.</li>')
[void]$html.AppendLine('</ul>')
[void]$html.AppendLine('</section>')
[void]$html.AppendLine('</main></body></html>')

$parent = Split-Path -Parent $outputFullPath
if (-not (Test-Path -LiteralPath $parent))
{
    New-Item -ItemType Directory -Path $parent | Out-Null
}

[System.IO.File]::WriteAllText($outputFullPath, $html.ToString(), [System.Text.Encoding]::UTF8)

[pscustomobject]@{
    OutputPath = $outputFullPath
    ScriptCount = $rows.Count
    MediumIntegrationCount = $integrationCounts.Medium
} | Format-List
