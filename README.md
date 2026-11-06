# Chrono Bastion

A single-player tower defence project for a functional programming course.
Defend an energy core against ten waves using Pulse, Mortar and Frost towers.
Losing ends the run; only the best completed-run score is saved in the browser.

**Team:** Yerkingali Saruar and Ainabek Chingiskhan, IT-2502.

## Current status

**Part 1 is implemented:** game rules, immutable state, commands, functional
helpers, session timing, high-score format, automated tests and a console simulator.
The Blazor project builds, but its home page is only an application shell.
**The playable graphical interface and live deployment belong to Part 2.**

## Start here

- **Chingiskhan:** follow [Part 2 handoff](docs/PART_2_HANDOFF.md).
- **His GPT:** use [the complete continuation prompt](docs/PART_2_GPT_PROMPT.md).
- [Architecture and state flow](docs/ARCHITECTURE.md)
- [Public API and integration examples](docs/API_CONTRACT.md)
- [Game rules and balance](docs/GAME_DESIGN.md)
- [Visual and mobile specification](docs/UI_SPEC.md)
- [Testing and verification evidence](docs/VERIFICATION.md)
- [Deployment instructions](docs/DEPLOYMENT.md)
- [Functional concepts and defense notes](docs/DEFENSE.md)
- [Assessment coverage](docs/REQUIREMENTS_MATRIX.md)

## Run on Windows

Install the **.NET 10 SDK**, Git, and VS Code with C# Dev Kit. Open a terminal in
the repository root, the folder containing `ChronoBastion.sln`.

```powershell
git clone https://github.com/F1j1tun1/IFP_Final.git
cd IFP_Final
dotnet --list-sdks
dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build
dotnet run --project tools/ChronoBastion.Simulator -c Release --no-build -- all
dotnet run --project src/ChronoBastion.Web --launch-profile http
```

Open the HTTP address printed by the last command (currently
`http://localhost:5042`). A title-only home page is expected until Part 2 is done.
The simulator runs both a legal winning strategy and an undefended losing run.
Its `victory` and `defeat` arguments run either scenario separately.

The same dotnet commands work on macOS/Linux with the .NET 10 SDK. For the helper
scripts there, install PowerShell 7 and use `pwsh`. No database or API key is needed.

## Project layout

```text
src/ChronoBastion.Core          Pure game rules and immutable data
src/ChronoBastion.Application   Session timing and high-score serialization
src/ChronoBastion.Web           Blazor interface and browser adapters (Part 2)
tests/ChronoBastion.Tests       Core and application tests
tools/ChronoBastion.Simulator   Headless full-game examples
docs/                         Design, contracts, handoff and defense
scripts/                      Repeatable verification and Pages packaging
```

## Verification and release

```powershell
pwsh -File scripts/Test.ps1
pwsh -File scripts/Publish-Pages.ps1
```

CI builds and tests on Windows and Linux. Publishing produces
`artifacts/pages/wwwroot`; it does not publish anything by itself.
The manual **Deploy Pages** workflow is for the finished Part 2 game.
The intended URL is `https://f1j1tun1.github.io/IFP_Final/`; consult
[deployment status](docs/DEPLOYMENT.md) before treating it as live.

## References

The supplied assessment document defines the grading criteria. Alex Yagur's
*Functional Programming with C#* (Packt, 2024) supplies the course concepts and
tower-defence exercise context. The engine here is an original implementation.
The supplied book and assessment files are not redistributed in the repository.
See [asset credits](docs/ASSETS.md) for the project's visual/audio asset policy.
