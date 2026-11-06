# Verification evidence

Date: 6 November 2026. Baseline: `65122e3c075b58689bc2a75229446e34c3a042c4`.
The graphical interface uses the original Core/Application API without changes.

## Baseline reproduced before integration

Windows x64, local .NET SDK 10.0.403:

- 70 existing tests passed, none failed or skipped.
- Defended simulator: Won, wave 10, 20 health, 151 kills, 12,090 points, 4,220 ticks.
- Undefended simulator: Lost in wave 3, zero kills, 300 points, 1,936 ticks.
- Baseline Release build succeeded with zero warnings/errors.

## Part 2 local checks

| Check | Actual result |
|---|---|
| `pwsh -File scripts/Test.ps1` | Passed: Release build, 83 tests (70 existing + 13 UI/storage), both simulations |
| `pwsh -File scripts/Publish-Pages.ps1` | Passed: trimmed static package, `/IFP_Final/` base, `.nojekyll`, 404 fallback |
| Component boundaries | HUD updates, pad selection, build/upgrade/sell/target callbacks, occupied-pad and insufficient-gold feedback, maximum level, victory/defeat callbacks |
| Rendering regression | Pads and enemies with the same numeric ID render without duplicate-key failures |
| Storage adapter | Read once; unfinished run does not save; malformed/unsupported record; blocked storage; best record retained in memory |
| Chromium packaged release | Passed: 151.0.7922.34, served locally at `/IFP_Final/`; no console/network errors |
| Browser victory | All ten waves through ordinary UI actions, 151 kills and 12,090 points; reload retained the winning record |
| Browser defeat and restart | Undefended wave-three defeat, 300 points; normal, corrupted and blocked storage scenarios; new run resets pads/gold/wave/speed |
| Controls | Build, upgrade, maximum level, sale refund, priority selection, insufficient gold, pause/resume and speed |
| Visibility | Real adapter called through a browser `visibilitychange` event with `document.hidden` emulated; manual Resume required |
| Repeated transitions | Three game/menu cycles; subsequent movement remained within the expected one-clock interval; unfinished run not saved |
| Responsive layouts | 1440px laptop, 390×844 portrait and 844×390 landscape; no horizontal page overflow; every pad selectable by touch emulation |
| Screenshots | Actual captures in `docs/screenshots`, visually inspected |
| Presentation | Ten-slide English PPTX, speaker notes, editable text/tables/architecture; all slide renders inspected; package/layout validation passed |

The final local Chromium run passed at 22:30 (Asia/Oral). Its report is saved in `docs/browser-results.json`. The browser runner writes its exact success/failure report to
`artifacts/browser-results.json`. It fails on page exceptions, error-level console
messages, failed requests and HTTP errors. `BROWSERS=chromium` selects the local
single-engine run; the default also requires Firefox. There is no accelerated
simulation, injected GameState, extra gold or public test harness.

## Environment limitations

- A physical phone was not available. Phone results above are emulated viewports
  and touch events, not a physical-device claim.
- Firefox 153.0 crashed before opening a page in the restricted local Windows
  process environment (exit 3221225477). No local Firefox pass is claimed.
- Windows MSBuild task-host IPC was unavailable in the restricted environment.
  Local package-cache task declarations for WebAssembly/ILLink were run in-process
  to permit verification. This workaround is outside the repository; normal
  Windows/Linux CI uses unmodified SDK packages. Scripts use one MSBuild worker.
- No native PowerPoint playback or physical macOS/Linux device check is claimed.
  The presentation was rendered and structurally validated.

## CI and publication

[GitHub Actions run 42262998402](https://github.com/F1j1tun1/IFP_Final/actions/runs/42262998402)
passed for implementation commit `085d277` on 6 November 2026:

- Windows and Ubuntu: Release build, all 83 tests and both simulator scenarios.
- Ubuntu: trimmed GitHub Pages package built with the unmodified .NET SDK.
- Chromium 151.0.7922.34: controls, responsive layouts, ten-wave victory,
  persistence, defeat/restart, corrupted/blocked storage and repeated transitions.
- Firefox 153.0: complete defeat, record persistence and fresh restart.
- Both browsers: no console errors, failed requests or HTTP errors.

The downloaded CI browser report is saved in `docs/browser-results-ci.json`.
[Part 2 pull request](https://github.com/F1j1tun1/IFP_Final/pull/1) contains the
reviewed implementation and checks. Core/Application source is unchanged.

GitHub CLI is authenticated as Chingiskhan07 and the branch is pushed. Publication
still requires the owner to select **Settings > Pages > Build and deployment >
Source > GitHub Actions**: the collaborator has push permission but no admin
permission, and the Pages API returned HTTP 404. Once enabled, run **Deploy Pages**
from `main` after its checks pass, then verify the public URL. No live-site pass is
claimed while this prerequisite remains outstanding. This is a hosting setting,
not an additional development stage.

## Reproduce

```powershell
pwsh -File scripts/Test.ps1
pwsh -File scripts/Publish-Pages.ps1
pwsh -File scripts/Test-Browser.ps1
```

Node.js 24 is required only for browser tests. The game itself needs no Node.js,
server, account or database. Domain coverage settings deliberately include Core
and Application; they do not represent total interface coverage.
