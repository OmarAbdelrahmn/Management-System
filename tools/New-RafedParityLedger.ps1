[CmdletBinding()]
param(
    [string]$SeedPath = "Application/Service/SystemCatalog/RafedCatalogSeed.cs",

    [string]$LinksPath = "rafed-links.json",

    [string]$MetadataPath = "rafed-sample-pages.json",

    [string]$OutputPath = "docs/rafed-service-parity-ledger.csv"
)

$ErrorActionPreference = "Stop"

function Get-CompactText {
    param([object]$Value)

    if ($null -eq $Value) {
        return ""
    }

    return ([string]$Value -replace "\s+", " ").Trim()
}

function Get-RafedFileName {
    param([string]$Url)

    if ([string]::IsNullOrWhiteSpace($Url)) {
        return ""
    }

    try {
        return [System.IO.Path]::GetFileName(([uri]$Url).AbsolutePath)
    }
    catch {
        return [System.IO.Path]::GetFileName($Url)
    }
}

function Get-PageMetadataIndex {
    param([string]$Path)

    $index = @{}
    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Output -NoEnumerate $index
        return
    }

    $payload = Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
    $pages = if ($payload -is [pscustomobject] -and $null -ne $payload.PSObject.Properties['pages']) { @($payload.pages) } else { @($payload) }
    foreach ($page in $pages) {
        $file = if ($page.file) { [string]$page.file } else { Get-RafedFileName ([string]$page.url) }
        if (-not [string]::IsNullOrWhiteSpace($file)) {
            $index[$file] = $page
        }
    }

    Write-Output -NoEnumerate $index
}

function Get-ObservedInputs {
    param([object]$Page)

    if ($null -eq $Page -or $null -eq $Page.inputs) {
        return "Not captured"
    }

    $inputs = @($Page.inputs | ForEach-Object {
        $name = Get-CompactText $_.name
        if ([string]::IsNullOrWhiteSpace($name)) { $name = Get-CompactText $_.id }
        if ([string]::IsNullOrWhiteSpace($name)) { $name = Get-CompactText $_.label }
        if ([string]::IsNullOrWhiteSpace($name)) { $name = Get-CompactText $_.placeholder }
        $type = Get-CompactText $_.type
        if ([string]::IsNullOrWhiteSpace($name)) { return }
        if ([string]::IsNullOrWhiteSpace($type)) { return $name }
        return "$name [$type]"
    } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)

    if ($inputs.Count) {
        return $inputs -join "; "
    }

    return "No inputs observed"
}

function Get-ObservedActions {
    param([object]$Page)

    if ($null -eq $Page) {
        return "Not captured"
    }

    $actions = @()
    if ($Page.forms) {
        $actions += @($Page.forms | ForEach-Object {
            $method = Get-CompactText $_.method
            $action = Get-CompactText $_.action
            if ([string]::IsNullOrWhiteSpace($method) -and [string]::IsNullOrWhiteSpace($action)) { return }
            return "form $method $action".Trim()
        })
    }
    if ($Page.buttons) {
        $actions += @($Page.buttons | ForEach-Object {
            $text = if ($_ -is [string]) { $_ } else { $_.text }
            $text = Get-CompactText $text
            if (-not [string]::IsNullOrWhiteSpace($text)) { return "button: $text" }
        })
    }

    $actions = @($actions | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)
    if ($actions.Count) {
        return $actions -join "; "
    }

    return "Not captured"
}

function Get-TableObservation {
    param([object]$Page)

    if ($null -eq $Page -or $null -eq $Page.tables) {
        return "Not captured"
    }

    $count = if ($Page.tables -is [System.Collections.IEnumerable] -and $Page.tables -isnot [string]) { @($Page.tables).Count } else { [int]$Page.tables }
    return "$count table(s) observed"
}

foreach ($path in @($SeedPath, $LinksPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Required input was not found: $path"
    }
}

$seed = Get-Content -Raw -LiteralPath $SeedPath
$moduleNames = @{}
$modulePattern = 'new\(@"(?<key>[^"]+)",\s*@"(?<name>[^"]+)",\s*@"(?<english>[^"]+)",\s*@"(?<description>[^"]*)",\s*@"menu-icon'
foreach ($match in [regex]::Matches($seed, $modulePattern)) {
    $moduleNames[$match.Groups['key'].Value] = $match.Groups['name'].Value
}

$pagePattern = 'new\(@"(?<key>[^"]+)",\s*@"(?<name>[^"]+)",\s*@"(?<route>[^"]+)",\s*@"(?<permission>[^"]+)",\s*@"(?<service>[^"]+)",\s*@"(?<servicePlan>[^"]*)",\s*@"(?<uiPlan>[^"]*)",\s*@"(?<original>[^"]+)",\s*(?:@"(?<icon>[^"]*)"|null),\s*SystemPageStatus\.(?<status>\w+),\s*(?<sort>\d+)\)'
$pageMatches = [regex]::Matches($seed, $pagePattern)
if ($pageMatches.Count -ne 688) {
    throw "Expected 688 services in the catalog seed but found $($pageMatches.Count). Update the parser before generating the ledger."
}

$links = Get-Content -Raw -LiteralPath $LinksPath | ConvertFrom-Json
$linkByFile = @{}
foreach ($link in $links) {
    $file = Get-RafedFileName ([string]$link.href)
    if (-not [string]::IsNullOrWhiteSpace($file) -and -not $linkByFile.ContainsKey($file)) {
        $linkByFile[$file] = [string]$link.href
    }
}

$metadataByFile = Get-PageMetadataIndex $MetadataPath
$capturedAt = if (Test-Path -LiteralPath $MetadataPath) { (Get-Item -LiteralPath $MetadataPath).LastWriteTimeUtc.ToString('yyyy-MM-ddTHH:mm:ssZ') } else { "Not captured" }
$rows = foreach ($match in $pageMatches) {
    $key = $match.Groups['key'].Value
    $moduleKey = $key.Split('.', 2)[0]
    $originalHref = $match.Groups['original'].Value
    $page = $metadataByFile[$originalHref]
    $originalUrl = if ($linkByFile.ContainsKey($originalHref)) { $linkByFile[$originalHref] } else { "https://www.sarh.org.sa/rafed/$originalHref" }
    $evidence = if ($null -ne $page) { "$(Split-Path -Leaf $MetadataPath): captured structural metadata" } else { "rafed-links.json: sidebar inventory only" }

    [pscustomobject][ordered]@{
        ServiceKey = $key
        ModuleKey = $moduleKey
        ModuleNameAr = if ($moduleNames.ContainsKey($moduleKey)) { $moduleNames[$moduleKey] } else { "Unmapped module name" }
        ServiceNameAr = $match.Groups['name'].Value
        OriginalHref = $originalHref
        OriginalUrl = $originalUrl
        OriginalFields = Get-ObservedInputs $page
        OriginalActions = Get-ObservedActions $page
        OriginalStates = "Not captured"
        OriginalValidations = "Not captured"
        OriginalTables = Get-TableObservation $page
        OriginalFilters = "Not captured"
        OriginalExports = "Not captured"
        OriginalPermissions = "Not captured"
        LocalRoute = $match.Groups['route'].Value
        LocalApi = "Not mapped"
        LocalEntity = "Not mapped"
        Migration = "Not assessed"
        LocalService = $match.Groups['service'].Value
        CatalogStatus = $match.Groups['status'].Value
        ParityStatus = "Partial"
        EvidenceSource = $evidence
        EvidenceCapturedAt = $capturedAt
        TestCoverage = "Not assessed"
        AcceptanceChecklist = "Not started"
    }
}

if (($rows.ServiceKey | Select-Object -Unique).Count -ne 688) {
    throw "The generated ledger contains duplicate service keys."
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$rows | Export-Csv -LiteralPath $OutputPath -NoTypeInformation -Encoding UTF8
$sampleCount = @($rows | Where-Object { $_.EvidenceSource -like '*captured structural metadata*' }).Count
Write-Host "Wrote $($rows.Count) services to $OutputPath ($sampleCount with captured structural metadata)."
