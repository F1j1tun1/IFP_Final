# Thornwatch

A single-player fantasy tower defence game in C# and Blazor WebAssembly.
Defend the Whispering Wood, Ashfall Village and the Ironthorn Keep through
12, 12 and 15 waves. The War Chief leads the final siege.

Yerkingali Saruar and Ainabek Chingiskhan, IT-2502.

See [project notes](PROJECT_NOTES.md) for team contributions, game rules and
balance results.

## Run

On Windows, install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0),
download this repository with **Code > Download ZIP**, and extract the whole ZIP.
Open **Start Game.cmd** in the extracted folder. Keep `Start-Game.ps1` and the
project folders beside it. The launcher checks the SDK, builds the game and opens
your browser. The first launch needs internet to download packages and may take
a few minutes. Installing only the .NET Runtime is not enough.

Keep the launcher window open while playing; Ctrl+C stops the game. If the
browser does not open automatically, visit http://localhost:5042. Close another
copy using that address before launching again.

For a manual start from the project folder:

```powershell
dotnet restore
dotnet run --project src/ChronoBastion.Web --launch-profile http
```

The manual commands also work on Linux and macOS.

## Play

- Choose **New journey**, select a numbered clearing and build a defender.
- **Archer** fires quickly at one enemy. **Catapult** hits groups. **Frost mage**
  slows enemies. Each has three levels with a different appearance.
- Start waves manually or enable **Auto waves** (off by default). After clearing
  a wave, it counts down five real seconds. Pause freezes the countdown; switching
  Auto waves off cancels it. x2 only accelerates combat. A new region always waits
  for a manual start. Enabling the option does not start an idle wave immediately.
- Build, upgrade, sell for 50% of investment, or
  select First / Strongest / Nearest targeting, including while paused.
- **Pause / Resume** stops combat. **x1 / x2** changes battle speed.
- After the forest and village, **Travel to next region** carries your remaining
  health and gold, refunds the full investment in old towers and adds 75 gold.
  Each new map has ten fixed clearings. Build its defences from scratch.
- Clear all 39 waves and the War Chief to win. If the gate falls anywhere,
  the entire attempt ends. A new journey starts in the forest with 300 gold
  and 20 gate health. There is no retry-wave or chapter-select shortcut.

On phones, tap the board or the large numbered buttons below it. On keyboards,
Tab moves focus and Enter/Space selects a clearing. The field manual explains
all fourteen enemies. Wolves and runners are fast; armored enemies resist damage;
the War Chief summons three goblins every six battle seconds. Shamans heal up to
three wounded allies within 110 world units every four battle seconds, restoring
12% of each target's maximum health. They cannot heal themselves, dead units or
the War Chief. All enemies, including wraiths, follow the road and can be hit by
all tower types.

**Sound on/off** enables original synthesized bow, stone, ice and event cues.
It starts off and only activates after a click. There are no external audio files.

## Saving

**Save & menu** pauses and retains the current attempt. **Continue journey**
restores its towers, enemies, resources and wave progress; combat resumes only
when you choose Resume. Opening the manual or hiding the tab also pauses combat.
Between waves, those actions cancel the countdown; choose Start wave on return.
Loading a save never starts a wave automatically. Ability timers are preserved.

A versioned snapshot is kept ready in memory after each update. Browser storage
is written after player actions, every two seconds, and on visibility/page-hide
signals, not every frame. An abrupt browser/device crash can lose up to two seconds.
Defeat or victory replaces the attempt with an ended marker; only the best
finished-run record remains. Confirming a new journey replaces the old attempt.

Data stays in this browser and origin. Corrupt or unavailable storage produces a
warning without stopping gameplay. The save codec validates phases, map progress,
wave schedules, entities, finite values and size before loading. It is a local
convenience feature, not a tamper-proof leaderboard.
Only one tab can own journey storage. A second tab cannot start or overwrite an
attempt; close the first tab and reload the second to continue. Unexpected changes
from another tab stop gameplay and saving until reload. Browsers without Web Locks
can play without saving; use HTTPS or localhost for persistence.
Campaign snapshots use version 3. Version 2 saves are migrated to the illustrated
maps: enemies retain their percentage of road travelled, and towers retain their
pad numbers and upgrades. Version 1 attempts cannot be resumed; best records remain.
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

Run `dotnet run --project tools/ChronoBastion.Simulator -- balance` to compare mixed,
archer-only, catapult-heavy, poor-placement, fixed three-tower and undefended runs.
All spending uses ordinary game commands. `artifacts/balance.csv` records income,
spending, travel refunds/bonus, reserve gold, tower levels and health after every wave.
Poor placement fills pads with the least road coverage first, using the same tower
mix; the fixed defence never adds to or upgrades its initial three towers.

## Publish

`Publish-Pages.ps1` generates static files in `artifacts/pages/wwwroot`, with the
`/IFP_Final/` base path. Select **Settings > Pages > Source > GitHub Actions** as
the repository owner, then run **Deploy Pages** from main after green checks.
The Pages URL is https://f1j1tun1.github.io/IFP_Final/. Deployment to this address
has not been verified.

## Source and assets

- `src/ChronoBastion.Core`: pure rules, immutable state, three maps and catalogs.
- `src/ChronoBastion.Application`: the single session, timing, record and save codecs.
- `src/ChronoBastion.Web`: reusable components, sprite rendering, browser storage/audio.
- `tests/`: engine, component, persistence and browser tests.
- `tools/ChronoBastion.Simulator`: complete legal winning and losing scenarios.

The game is called Thornwatch; project and namespace names still use
`ChronoBastion`. Combat rules run in C#.

Tower and enemy sprites are in `wwwroot/art`; the three map images are in
`wwwroot/art/maps`. The interface uses SVG, CSS and system fonts. Sound cues are
synthesized in `browser.js`. The game needs no paid services, accounts, database
or API keys.
Course reference: Alex Yagur, *Functional Programming with C#* (Packt, 2024).
