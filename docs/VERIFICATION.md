# Verification evidence

This file distinguishes the implemented foundation from the unfinished graphical
game. Part 2 must add its own actual results; unchecked items are not claims of success.

## Part 1 local verification

Date: 6 November 2026. Environment: Windows x64, .NET SDK 10.0.401.

| Check | Result |
|---|---|
| Release build of all five projects | Passed, zero compiler warnings/errors |
| xUnit tests | 70 passed, zero failed/skipped |
| Core line / branch coverage | 96.23% / 90.05% |
| Application line / branch coverage | 98.14% / 96.42% |
| Legal defended full run | Won wave 10, 20 health, 151 kills, 12090 points |
| Undefended full run | Lost during wave 3, zero kills, 300 points |
| Blazor release publish | Passed |
| Pages package | `/IFP_Final/` base, `.nojekyll`, and 404 fallback produced |
| Published shell startup in browser | Passed through a local static server at `/IFP_Final/`; Blazor rendered the title and subtitle |

Coverage is collected with coverage.runsettings. It includes Core/Application,
excludes generated code under obj and code marked GeneratedCode, and does not claim
coverage of a final graphical UI. Reports are generated under
tests/ChronoBastion.Tests/TestResults and are not committed.

The simulator uses ordinary commands and earned resources. It is not a UI
autoplayer and grants no extra money. Its example winning run takes 4220 fixed
ticks (211 seconds of active game time at x1); planning time is additional.

## Reproduce

```powershell
pwsh -File scripts/Test.ps1
pwsh -File scripts/Publish-Pages.ps1
```

Individual commands are in README. Core tests cover command errors and transaction
atomicity, pricing/refunds, upgrades, targeting, armor, splash, frost expiry,
cooldowns, rewards, path positions, schedules, pause, speed, bounded catch-up,
restart and corrupt record handling.

## Automated remote checks

The Build and test workflow is configured for Windows and Linux, with an additional
Pages release build on Linux. Check the workflow run for the exact commit being
handed off; a configured workflow alone does not prove that a run succeeded.

## Remaining Part 2 checks

- [ ] Real browser startup and gameplay with no console/network errors.
- [ ] Component tests for action/state/feedback boundaries.
- [ ] Browser tests for build, upgrade, selling, wave start, pause, speed and results.
- [ ] A complete browser win and a complete browser loss/restart.
- [ ] The record survives reload and never saves an unfinished run.
- [ ] Malformed, unsupported and blocked browser storage do not break play.
- [ ] Hidden-tab pause, return/resume and repeated game/menu transitions.
- [ ] Laptop and phone-sized layouts with usable keyboard/touch controls.
- [ ] Physical phone test, if a phone is available; state explicitly if only emulated.
- [ ] At least two available browser engines, with exact versions recorded.
- [ ] Release build and live Pages site work under `/IFP_Final/`, including refresh.
- [ ] Final presentation and real screenshots match the implementation.

No graphical gameplay, physical-device or live-deployment result is claimed by
the Part 1 checks above. These remain meaningful work for the second participant.
