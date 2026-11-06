$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    dotnet restore ChronoBastion.sln
    if ($LASTEXITCODE -ne 0) { throw 'Dependency restore failed.' }

    dotnet build ChronoBastion.sln -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }

    dotnet test ChronoBastion.sln -c Release --no-build --collect:'XPlat Code Coverage' --settings coverage.runsettings
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }

    dotnet run --project tools/ChronoBastion.Simulator -c Release --no-build -- all
    if ($LASTEXITCODE -ne 0) { throw 'A complete-game scenario failed.' }
} finally {
    Pop-Location
}
