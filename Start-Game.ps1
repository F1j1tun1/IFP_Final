param([switch]$NoBrowser)

$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$gameUrl = 'http://localhost:5042'
$sdkUrl = 'https://dotnet.microsoft.com/en-us/download/dotnet/10.0'
$browserJob = $null

Write-Host 'Starting Thornwatch...'
Write-Host "Game address: $gameUrl"
Write-Host 'Keep this window open while playing. Press Ctrl+C to stop.'
Write-Host 'The first launch downloads packages and can take a few minutes.'
Write-Host ''

try {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        throw "Install the .NET 10 SDK, then reopen this launcher. Download: $sdkUrl"
    }

    $sdks = & $dotnet.Source --list-sdks
    if ($LASTEXITCODE -ne 0 -or -not ($sdks -match '^10\.0\.\d+\s+\[')) {
        throw "The .NET 10 SDK is required. The Runtime alone is not enough. Download: $sdkUrl"
    }

    if (-not $NoBrowser) {
        $browserJob = Start-Job -ArgumentList $gameUrl -ScriptBlock {
            param($url)
            for ($attempt = 0; $attempt -lt 300; $attempt++) {
                try {
                    $page = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2
                    if ($page.StatusCode -eq 200 -and $page.Content.Contains('<title>Thornwatch</title>')) {
                        Start-Process $url
                        return
                    }
                } catch { }
                Start-Sleep -Seconds 1
            }
        }
    }

    & $dotnet.Source run --project 'src/ChronoBastion.Web' -c Release --launch-profile http
    if ($LASTEXITCODE -ne 0) {
        throw 'The game could not start. Check the error above. If port 5042 is in use, close the other game launcher and try again.'
    }
} catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
} finally {
    if ($null -ne $browserJob) {
        Stop-Job $browserJob -ErrorAction SilentlyContinue
        Remove-Job $browserJob -Force -ErrorAction SilentlyContinue
    }
}
