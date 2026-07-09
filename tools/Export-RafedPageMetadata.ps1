param(
    [string]$Username = $env:RAFED_USERNAME,

    [securestring]$Password,

    [string]$BaseUrl = "https://www.sarh.org.sa/rafed/",

    [string]$LinksPath = "rafed-links.json",

    [string]$OutputPath = "rafed-page-metadata.json",

    [int]$Limit = 0
)

$ErrorActionPreference = "Stop"

# Read-only structural audit only. After authentication this script only issues GET
# requests and writes page metadata, never form values, records, cookies, or HTML.

function Get-CleanText {
    param([string]$Html)

    if ([string]::IsNullOrWhiteSpace($Html)) {
        return ""
    }

    $withoutTags = [regex]::Replace($Html, "<[^>]+>", " ")
    $decoded = [System.Net.WebUtility]::HtmlDecode($withoutTags)
    return [regex]::Replace($decoded, "\s+", " ").Trim()
}

function Get-Token {
    param([string]$Html)

    $match = [regex]::Match($Html, 'name=["'']?token["'']?\s+value=["'']([^"'']+)["'']', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (!$match.Success) {
        $match = [regex]::Match($Html, 'name=token value="([^"]+)"', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    }

    return $match.Groups[1].Value
}

function ConvertFrom-SecureStringToPlainText {
    param([securestring]$Value)

    $bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try {
        return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    }
    finally {
        if ($bstr -ne [IntPtr]::Zero) {
            [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
        }
    }
}

function Resolve-RafedUrl {
    param(
        [string]$Base,
        [string]$Href
    )

    if ([string]::IsNullOrWhiteSpace($Href)) {
        return $Base
    }

    return ([System.Uri]::new([System.Uri]::new($Base), $Href)).AbsoluteUri
}

function Get-AttributeValue {
    param(
        [string]$Html,
        [string]$Name
    )

    $match = [regex]::Match($Html, "\b$Name\s*=\s*[""']([^""']+)[""']", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($match.Success) {
        return $match.Groups[1].Value
    }

    return $null
}

function Get-PageMetadata {
    param(
        [Microsoft.PowerShell.Commands.WebRequestSession]$Session,
        [string]$Url,
        [string]$Label
    )

    $response = Invoke-WebRequest -Uri $Url -Method Get -WebSession $Session -UseBasicParsing
    $html = $response.Content

    $title = [regex]::Match($html, "<title>(.*?)</title>", [System.Text.RegularExpressions.RegexOptions]::Singleline).Groups[1].Value
    $formMatches = [regex]::Matches($html, "<form\b[^>]*>", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $inputMatches = [regex]::Matches($html, "<(?:input|select|textarea)\b[^>]*>", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $forms = $formMatches |
        Select-Object -First 20 |
        ForEach-Object {
            [pscustomobject]@{
                method = (Get-AttributeValue $_.Value "method")
                action = (Get-AttributeValue $_.Value "action")
            }
        }
    $inputs = $inputMatches |
        Select-Object -First 80 |
        ForEach-Object {
            [pscustomobject]@{
                name = (Get-AttributeValue $_.Value "name")
                type = (Get-AttributeValue $_.Value "type")
                tag = [regex]::Match($_.Value, "^<([a-z]+)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase).Groups[1].Value.ToLowerInvariant()
            }
        }
    $buttons = [regex]::Matches($html, '<(?:button|a|input)[^>]*(?:class=["''][^"'']*\bbtn\b[^"'']*["'']|type=["'']submit["''])[^>]*>(.*?)</(?:button|a)>|<input[^>]*type=["'']submit["''][^>]*value=["'']([^"'']+)["'']', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline) |
        ForEach-Object {
            $text = if ($_.Groups[1].Success) { $_.Groups[1].Value } else { $_.Groups[2].Value }
            Get-CleanText $text
        } |
        Where-Object { ![string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 20

    [pscustomobject]@{
        url = $Url
        label = $Label
        status = [int]$response.StatusCode
        title = Get-CleanText $title
        forms = @($forms)
        formCount = $formMatches.Count
        inputs = @($inputs)
        inputCount = $inputMatches.Count
        tables = [regex]::Matches($html, "<table\b", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
        buttons = @($buttons)
    }
}

if ([string]::IsNullOrWhiteSpace($Username)) {
    $Username = Read-Host "Rafed username"
}

if ($null -eq $Password) {
    if (![string]::IsNullOrWhiteSpace($env:RAFED_PASSWORD)) {
        $Password = ConvertTo-SecureString $env:RAFED_PASSWORD -AsPlainText -Force
    }
    else {
        $Password = Read-Host "Rafed password" -AsSecureString
    }
}

$plainPassword = ConvertFrom-SecureStringToPlainText $Password

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$loginPage = Invoke-WebRequest -Uri $BaseUrl -Method Get -WebSession $session -UseBasicParsing
$token = Get-Token $loginPage.Content

if ([string]::IsNullOrWhiteSpace($token)) {
    throw "Could not find Rafed login token."
}

$loginBody = @{
    token = $token
    login_username = $Username
    login_password = $plainPassword
    remember = "remember"
}

$loginResponse = Invoke-WebRequest -Uri $BaseUrl -Method Post -Body $loginBody -WebSession $session -UseBasicParsing
$plainPassword = $null
if ($loginResponse.Content -notmatch 'dashboard body logged') {
    throw "Login did not reach a logged-in Rafed dashboard."
}

$links = Get-Content -Raw $LinksPath | ConvertFrom-Json
if ($Limit -gt 0) {
    $links = $links | Select-Object -First $Limit
}

$metadata = foreach ($link in $links) {
    $url = Resolve-RafedUrl -Base $BaseUrl -Href $link.href
    Get-PageMetadata -Session $session -Url $url -Label $link.text
}

$payload = [pscustomobject]@{
    capturedAt = (Get-Date).ToUniversalTime().ToString("o")
    count = @($metadata).Count
    pages = @($metadata)
}

$payload | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 $OutputPath
Write-Host "Wrote $(@($metadata).Count) read-only page metadata records to $OutputPath"
