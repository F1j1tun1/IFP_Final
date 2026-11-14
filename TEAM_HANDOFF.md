# Thornwatch team handoff

Agreed division, 14 November 2026. This is the ongoing project revision, not a
third planned development stage. Keep the pull request in draft until both parts
are reviewed together. Do not publish the incomplete ability implementation.

## Ainabek Chingiskhan / Chingiskhan07

Owns the interface and graphics: the approved cartoon sprites, three map views,
menus, HUD, tower controls, help/results, responsive touch/keyboard interaction,
sound feedback, browser persistence and browser/component tests.

The branch also includes the integration foundation needed to exercise that UI:
three map definitions, 12/12/15 wave schedules, fourteen enemy catalog entries,
region transitions with resource carryover, and a validated save codec. These are
already implemented; they are not unfinished tasks assigned to the teammate.

## Yerkingali Saruar / F1j1tun1

Owns the enemy and boss abilities in the C# engine, their balance and domain tests.
The prototype abilities were deliberately removed from the active branch after
this division was agreed. No claim is made that they are implemented here.

Implement these agreed mechanics:

1. **War Chief:** summon three goblins periodically (starting proposal: once every
   six battle seconds). Spawn them near or behind the boss on the existing path,
   with unique IDs from NextEnemyId. Give each ordinary catalog health/rewards.
   Keep the total number of living enemies bounded. No spawning after boss death.
2. **Orc shaman:** periodically heal nearby living allies (starting proposal:
   restore 4% of max health within 100 world units every three battle seconds).
   Cap health at MaxHealth. Do not resurrect dead enemies or heal the War Chief.
3. Tune the ability intervals, enemy health, wave composition and rewards using
   complete legal simulations. All 39 waves must remain winnable without injected
   gold, health or game-state shortcuts.

These numerical proposals are starting points for your implementation and
balancing, not a requirement to reproduce an earlier prototype verbatim. Do not
add unagreed mechanics such as extra maps, multiplayer or retry-wave checkpoints.

## Integration points

- `src/ChronoBastion.Core/GameEngine.cs`: Tick is the pure simulation boundary.
  Insert ability processing in a deliberate order around spawning/combat/movement;
  explain that order with focused tests. Return new state and events.
- `src/ChronoBastion.Core/Content/EnemyCatalog.cs` and `WavePlanner.cs`: definitions,
  balance and recursive wave composition. Use Campaign.Get(state.StageNumber).Map
  for positions, ranges and path length; never assume the first map.
- `GameEvent.Summoned(Position)` already exists. The interface consumes it for
  the boss announcement and sound; emit it once per summon burst.
- `src/ChronoBastion.Application/SavedRun.cs`: snapshots include the current state.
  If you add cooldown/ability fields, serialize and validate them and add a reload
  equivalence test. Use simulation ticks, not wall-clock time, Random, JavaScript
  timers or another GameSession. Reload must not reset a boss ability for an exploit.
- `GameQueries.Describe`: update English enemy descriptions when abilities work.
  Preserve the UI parameters, dispatch commands and existing event contracts.

## Acceptance checks for the engine work

- A boss summons at the intended interval; spawned IDs are unique and rewards
  are paid once. Summons cannot appear beyond the gate or after the run ends.
- Pause freezes ability timers. Speed x2 changes elapsed progression, not damage
  per tick. Saving/reloading continues deterministically and resumes paused.
- Shaman healing obeys range, maximum health, death and boss exclusions.
- Enemy positions, splash hits and ability ranges use each region's actual map.
- Victory requires the boss and all surviving summons to be resolved. Defeat
  takes priority and still deletes the active saved attempt through GameEnded.
- Existing tests remain green; add focused tests demonstrating your new behavior.
- Run Test.ps1, Publish-Pages.ps1 and Test-Browser.ps1; inspect the final boss fight.

## Working together

The integration draft is PR #2, branch `feat/thornwatch`. Fetch it and create your
own branch from it so you receive the new graphics and contracts:

```powershell
git fetch origin
git switch -c feat/enemy-abilities origin/feat/thornwatch
```

Commit your actual changes under your own Git identity with short neutral titles,
for example `Add boss reinforcements` or `Balance enemy abilities`. Open your PR
against `feat/thornwatch` for integration review. Do not force-push main or replace
another person's work. After integration, run the complete checks again before
merging PR #2 into main.

Publication also requires the owner to set Settings > Pages > Build and deployment
> Source to GitHub Actions, then run Deploy Pages from the final tested main commit.
