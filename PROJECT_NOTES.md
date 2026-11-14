# Thornwatch project notes

## Team contributions

- **Ainabek Chingiskhan / Chingiskhan07:** cartoon sprites, maps, menu, HUD,
  tower controls, responsive interaction and the campaign integration foundation.
- **Yerkingali Saruar / F1j1tun1:** enemy abilities, economy, automatic waves,
  save consistency and their engine/browser tests, followed by the map
  and interface revision.

Three illustrated maps now have curved roads and aligned build sites. Region
colours carry through the menu and dialogs; a new journey returns to the forest.
Gate health is red in the HUD and above the gate. Currency amounts use coin icons.
Save version 3 migrates version 2 enemies by their fraction of the road travelled;
tower pad IDs, upgrades, resources and ability timers are retained.

## Implemented mechanics

The War Chief summons three ordinary goblins every six combat seconds, behind its
current position on the active map. They receive unique IDs, normal wave-scaled
health and ordinary rewards. Summoning is capped at 120 living enemies. The final
wave continues until all enemies, including summons, are killed or escape.

Every four combat seconds, a shaman restores 12% of maximum health to up to three
wounded allies within 110 world units. The most injured health fraction goes
first, with enemy ID breaking ties. A shaman cannot heal itself, dead units or the
War Chief. Healing cannot exceed maximum health. Other shamans are valid allies.

Tick order: scheduled spawns, tower attacks, surviving casters' abilities,
movement/death rewards, defeat, then wave completion. A caster killed by a tower
cannot act that tick. All time is measured in simulation ticks and the incoming
immutable state is preserved. Saved runs store and validate ability timers.

Auto waves defaults to Off. If enabled when a wave ends, the existing clock runs
a five-second countdown. Pause freezes it; x2 affects combat only. Disabling the
option cancels the countdown. The menu, manual, hidden tab and save restoration
cancel pending automatic starts. Combat returns paused; planning requires Start
wave. New regions and new journeys always start manually. There is no wave retry.

## Economy

| Rule | Previous draft | Current |
| --- | --- | --- |
| Starting gold / health | 300 / 20 | 300 / 20 |
| Archer build / upgrades | 90 / 75 / 120 | 90 / 110 / 180 |
| Catapult build / upgrades | 140 / 110 / 160 | 140 / 160 / 260 |
| Frost mage build / upgrades | 100 / 80 / 130 | 100 / 120 / 190 |
| Wave reward | 30 + 5 × global wave | 10 + wave within region |
| Voluntary sale | 70% | 50% |
| Travel refund | 70% | 100% |
| Region bonus | 150 | 75 |
| Health growth per global wave | 10% of base | 12% of base |
| War Chief base health | 1100 | 900 |

Kill gold is lower: goblin 4, runner/wolf 3, spider 2, orc/wraith 6,
armored orc 7, shaman/armored skeleton 8, ent 10, skeleton 4, zombie 5,
troll 15 and War Chief 80. Scores and tower attack stats are unchanged.
More expensive upgrades compete with extra coverage. Full travel refunds let the
player rebuild for a new map without losing a fraction of the entire army twice.

The simulator produces these results using normal game commands and resources:

| Strategy | Result | Gate health | Gold left |
| --- | --- | ---: | ---: |
| Mixed defence | Win, wave 39 | 20 | 1592 |
| Catapult-heavy | Win, wave 39 | 20 | 698 |
| Archers only | Lose, wave 39 | 0 | 1999 |
| Least road coverage first | Lose, wave 39 | 0 | 1463 |
| Three initial towers, no upgrades | Lose, wave 13 | 0 | 1097 |
| No defence | Lose, wave 3 | 0 | 323 |

These fixed strategies do not measure how difficult players will find the game.
The opening is forgiving; further balance changes need human playtesting.
Run the simulator's `balance` mode for per-wave resource and tower-level data.

## Save ownership

One tab holds a Web Lock for `thornwatch.run.v2`. Other tabs cannot start a game or
write that attempt. Close the owning tab and reload to transfer control. A storage
change from another writer stops the current tab before its cached state can be
saved again. An ended marker cannot be overwritten by a stale participating tab.
Unsupported/unavailable storage is reported and allows play without saving.

Version 1 attempts are not migrated because prices and combat rules changed.
The existing best-record key is retained. Saves are local, not tamper-proof, and
crashes can lose up to the two-second autosave interval. There is no cloud sync.

## Verification and deployment

Run `scripts/Test.ps1`, `scripts/Publish-Pages.ps1` and `scripts/Test-Browser.ps1`.
The browser suite uses the published `/IFP_Final/` build, ordinary player controls,
all 39 waves, loss/restart, countdown interruptions, two-tab conflicts and storage
failure. Reports and screenshots stay in ignored `artifacts/`; viewport emulation
does not constitute testing on a physical phone.

Publishing requires Pages to use GitHub Actions and the Deploy Pages workflow
to run from a tested main commit. See [README](README.md#publish) for setup.
