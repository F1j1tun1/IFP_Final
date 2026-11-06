param([string]$BasePath = '/IFP_Final/')

$ErrorActionPreference = 'Stop'
if ($BasePath -notmatch '^/(?:[A-Za-z0-9_-]+/)?$') {
    throw 'BasePath must be / or a repository path such as /IFP_Final/.'
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot 'artifacts/pages'
Push-Location $repoRoot
try {
    dotnet publish src/ChronoBastion.Web -m:1 -c Release -o $publishDir
    if ($LASTEXITCODE -ne 0) { throw 'Web publish failed.' }

    $siteDir = Join-Path $publishDir 'wwwroot'
    $indexPath = Join-Path $siteDir 'index.html'
    $html = [IO.File]::ReadAllText($indexPath)
    if (-not $html.Contains('<base href="/" />')) {
        throw 'The published index does not contain the expected root base element.'
    }
    $html = $html.Replace('<base href="/" />', ('<base href="' + $BasePath + '" />'))
    [IO.File]::WriteAllText($indexPath, $html, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText((Join-Path $siteDir '.nojekyll'), '')
    Copy-Item -LiteralPath $indexPath -Destination (Join-Path $siteDir '404.html') -Force
    Write-Output ('Pages files are ready in ' + $siteDir)
} finally {
    Pop-Location
}
