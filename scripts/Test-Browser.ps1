$ErrorActionPreference = 'Stop'
Push-Location (Split-Path -Parent $PSScriptRoot)
try {
    npm ci
    if ($LASTEXITCODE -ne 0) { throw 'Browser dependency installation failed.' }
    npx playwright install chromium firefox
    if ($LASTEXITCODE -ne 0) { throw 'Browser installation failed.' }
    & "$PSScriptRoot/Publish-Pages.ps1"
    npm run test:browser
    if ($LASTEXITCODE -ne 0) { throw 'Browser tests failed.' }
} finally { Pop-Location }
