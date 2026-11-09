# Chrono Bastion

A tower defence game built with C# and Blazor WebAssembly. Protect the core
through ten waves using Pulse, Mortar and Frost towers.

Yerkingali Saruar and Ainabek Chingiskhan, IT-2502.

## Run

Install the .NET 10 SDK, then run from the project folder:

```powershell
dotnet restore
dotnet run --project src/ChronoBastion.Web --launch-profile http
```

Open http://localhost:5042. Keep the terminal open while playing and press
Ctrl+C to stop. The same commands work on Windows, Linux and macOS.

## How to play

- Select **Deploy defences**, choose a numbered pad and build a tower.
- Pulse fires quickly, Mortar damages groups, and Frost slows enemies.
- Select a tower to upgrade, sell or change its targeting priority.
- Start each wave when ready. You can build during combat or pause to plan.
- Clear all ten waves to win. Losing or restarting begins a new run.

**Pause / Resume** stops combat. **x1 / x2** changes speed. Opening the field
manual or hiding the tab pauses the game until you resume it.

On phones, use the numbered pad buttons below the board. Keyboard controls use
Tab to move focus and Enter or Space to select a pad. The field manual explains
the towers, enemies and controls.

Only the best completed-run score is saved in the browser. No account, database
or API key is required.

## Tests

Run the build, .NET tests and complete-game simulations with PowerShell 7:

```powershell
pwsh -File scripts/Test.ps1
```

Browser tests also require Node.js 24:

```powershell
pwsh -File scripts/Test-Browser.ps1
```

Browser reports and screenshots are written to `artifacts/`.

## Publish

```powershell
pwsh -File scripts/Publish-Pages.ps1
```

The static files are generated in `artifacts/pages/wwwroot` for the `/IFP_Final/`
base path. To publish on GitHub Pages, select **GitHub Actions** under the
repository's Pages settings and run the **Deploy Pages** workflow from `main`.

## Source

- `src/ChronoBastion.Core`: game rules, immutable state and tower/enemy definitions.
- `src/ChronoBastion.Application`: game session, timing and record format.
- `src/ChronoBastion.Web`: interface, rendering and browser storage.
- `tests/`: engine, component and browser tests.
- `tools/ChronoBastion.Simulator`: console checks for complete games.

The interface uses SVG, CSS and system fonts. No external game assets are needed.
Course reference: Alex Yagur, *Functional Programming with C#* (Packt, 2024).
