# Thornwatch

A single-player fantasy tower defence game in C# and Blazor WebAssembly.
Defend the Whispering Wood, Ashfall Village and the Ironthorn Keep through
12, 12 and 15 waves. The War Chief leads the final siege.

Yerkingali Saruar and Ainabek Chingiskhan, IT-2502.

The current integration is a draft. See [team handoff](TEAM_HANDOFF.md) for the
agreed interface/engine ownership and the remaining ability work.

## Run

Install the .NET 10 SDK, then run from the project folder:

```powershell
dotnet restore
dotnet run --project src/ChronoBastion.Web --launch-profile http
```

Open http://localhost:5042. Keep the terminal open while playing; Ctrl+C stops it.
The same commands work on Windows, Linux and macOS.

## Play

- Choose **New journey**, select a numbered clearing and build a defender.
- **Archer** fires quickly at one enemy. **Catapult** hits groups. **Frost mage**
  slows enemies. Each has three upgrades with a different appearance.
- Start every wave manually. Build, upgrade, sell for 70% of investment, or
  select First / Strongest / Nearest targeting, including while paused.
- **Pause / Resume** stops combat. **x1 / x2** changes battle speed.
- After the forest and village, **Travel to next region** carries your remaining
  health and gold, sells the old towers at their normal refund and adds 150 gold.
  Each new map has ten fixed clearings. Build its defences from scratch.
- Clear all 39 waves and the War Chief to win. If the gate falls anywhere,
  the entire attempt ends. A new journey starts in the forest with 300 gold
  and 20 gate health. There is no retry-wave or chapter-select shortcut.

On phones, tap the board or the large numbered buttons below it. On keyboards,
Tab moves focus and Enter/Space selects a clearing. The field manual explains
all fourteen enemies. Wolves and runners are fast; armored enemies resist damage;
the War Chief is the final siege enemy. Special abilities are assigned to the
engine teammate in TEAM_HANDOFF.md and are not yet enabled in this draft. All enemies,
including wraiths, follow the road and can be hit by all tower types.

**Sound on/off** enables original synthesized bow, stone, ice and event cues.
It starts off and only activates after a click. There are no external audio files.

## Saving

**Save & menu** pauses and retains the current attempt. **Continue journey**
restores its towers, enemies, resources and wave progress; combat resumes only
when you choose Resume. Opening the manual or hiding the tab also pauses combat.

A versioned snapshot is kept ready in memory after each update. Browser storage
is written after player actions, every two seconds, and on visibility/page-hide
signals, not every frame. An abrupt browser/device crash can lose up to two seconds.
Defeat or victory replaces the attempt with an ended marker; only the best
finished-run record remains. Confirming a new journey replaces the old attempt.

Data stays in this browser and origin. Corrupt or unavailable storage produces a
warning without stopping gameplay. The save codec validates phases, map progress,
wave schedules, entities, finite values and size before loading. It is a local
convenience feature, not a tamper-proof leaderboard. Keep one active game tab.
Old Chrono Bastion records use a separate key and are not imported into this campaign.

## Verify

```powershell
pwsh -File scripts/Test.ps1
pwsh -File scripts/Publish-Pages.ps1
```

Browser tests require Node.js 24:

```powershell
pwsh -File scripts/Test-Browser.ps1
```

The full browser suite plays all 39 waves through ordinary controls. It also
checks saves/reloads between maps, a live battle reload, defeat/restart, corrupt
and blocked storage, keyboard/touch controls and responsive layouts. Allow about
15–20 minutes. Screenshots and reports go to `artifacts/`; phone viewports are
emulated. CI runs Windows/Linux .NET checks and Chromium plus Firefox on Linux.

## Publish

`Publish-Pages.ps1` generates static files in `artifacts/pages/wwwroot`, with the
`/IFP_Final/` base path. Select **Settings > Pages > Source > GitHub Actions** as
the repository owner, then run **Deploy Pages** from main after green checks.
The intended URL is https://f1j1tun1.github.io/IFP_Final/ . It is not considered
verified until deployment and live-site tests succeed.

## Source and assets

- `src/ChronoBastion.Core`: pure rules, immutable state, three maps and catalogs.
- `src/ChronoBastion.Application`: the single session, timing, record and save codecs.
- `src/ChronoBastion.Web`: reusable components, sprite rendering, browser storage/audio.
- `tests/`: engine, component, persistence and browser tests.
- `tools/ChronoBastion.Simulator`: complete legal winning and losing scenarios.

Existing project/namespace names are retained for compatibility; the game is
branded Thornwatch. The original functional engine was extended for the agreed
campaign. Combat still runs in C#, never JavaScript.

The two sprite atlases in `wwwroot/art` are original project artwork based on the
approved character sheets. Maps and interface use original SVG/CSS and system
fonts. Sound cues are synthesized in `browser.js`. No third-party game assets,
paid services, accounts, database or API keys are required.
Course reference: Alex Yagur, *Functional Programming with C#* (Packt, 2024).
