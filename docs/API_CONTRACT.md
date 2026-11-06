# Core and interface contract

These are implemented C# APIs. Part 2 should integrate them, not create alternative
combat logic. Namespaces used below are `ChronoBastion.Core`, `.Model`, `.Content`,
`.Rules`, `.Functional`, and `ChronoBastion.Application`.

## Main entry points

```csharp
GameState GameEngine.NewGame();
Result<GameTransition, GameError> GameEngine.Execute(GameState state, GameCommand command);
GameTransition GameEngine.Tick(GameState state);

GameState GameSession.State { get; }
Result<GameTransition, GameError> GameSession.Dispatch(GameCommand command);
void GameSession.Advance(TimeSpan elapsed);
event Action<GameTransition>? GameSession.Updated;
```

The static engine API is useful for tests and the simulator. The UI should use one
injected GameSession. It is already registered in Web/Program.cs. Each browser
application instance gets its own in-memory session.

## Commands

| C# command | Allowed phase | Result |
|---|---|---|
| `new GameCommand.BuildTower(padId, kind)` | Planning/Combat | Charges level-one price, creates tower |
| `new GameCommand.UpgradeTower(padId)` | Planning/Combat | Charges next-level price, increments level |
| `new GameCommand.SellTower(padId)` | Planning/Combat | Removes tower, refunds 70% of total investment |
| `new GameCommand.SetTargetPriority(padId, priority)` | Planning/Combat | Changes target choice without cost |
| `new GameCommand.StartWave()` | Planning | Starts the next wave |
| `new GameCommand.SetPaused(true/false)` | Combat | Pauses/resumes the current battle |
| `new GameCommand.SetSpeed(1/2)` | Planning/Combat | Changes simulated ticks per elapsed second |
| `new GameCommand.Restart()` | Any | Returns a completely fresh run |

Expected player mistakes return `Result<GameTransition, GameError>.Failure`.
Examples include insufficient gold, an occupied pad and a missing tower. Failures
do not mutate state and do not publish Updated. Restart never restores a checkpoint.

## Minimal UI wiring

```csharp
[Inject] private GameSession Session { get; set; } = default!;
private string? error;

private void Build(int padId, TowerKind kind)
{
    var result = Session.Dispatch(new GameCommand.BuildTower(padId, kind));
    error = result.Match(_ => (string?)null, failure => failure.Message);
}
```

Subscribe to Updated once in the component that owns the game screen. In its
handler, consume effect events and request a render. Use InvokeAsync when entering
from a timer or JavaScript. Clear selection and old effects when GameRestarted is
received. Unsubscribe on disposal. Do not subscribe on every render.

## Timer contract

Use one timer only. A simple C# adapter can use PeriodicTimer and Stopwatch:

```csharp
private readonly CancellationTokenSource stop = new();
private Task? loop;

private async Task RunClockAsync()
{
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(33));
    var clock = Stopwatch.StartNew();
    var previous = clock.Elapsed;
    try
    {
        while (await timer.WaitForNextTickAsync(stop.Token))
        {
            await InvokeAsync(() =>
            {
                var now = clock.Elapsed;
                Session.Advance(now - previous);
                previous = now;
            });
        }
    }
    catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
}
```

Start this once (for example from OnAfterRenderAsync on firstRender), keep its Task,
and implement IAsyncDisposable to cancel, await the task, unsubscribe, and dispose
the CancellationTokenSource. Dispose JavaScript object references/listeners too.
This example is a pattern, not a complete component: add the lifecycle methods,
event handler and imports in the actual implementation.

Do not use Task.Run, a second timer inside GameBoard, or `Advance(elapsed * speed)`.
Application already applies speed. Handle `document.visibilitychange`: when the
page becomes hidden during combat, dispatch SetPaused(true). On return, keep it
paused until the player chooses Resume. This avoids an unseen battle on a phone.

## State to display

| Property | Meaning |
|---|---|
| Phase | Planning / Combat / Won / Lost |
| WaveNumber | Zero initially; current or last started wave afterwards |
| WavesCleared | Count completed successfully |
| Gold, CoreHealth, Score | Authoritative HUD values |
| Kills, Escaped | Run statistics |
| WaveTick | Number of fixed ticks performed in the current wave |
| IsPaused, SpeedMultiplier | Pause and x1/x2 controls |
| Towers | Immutable list, sorted by PadId |
| Enemies | Immutable list of living active enemies |
| PendingEnemies | Scheduled enemies that have not spawned yet |
| IsFinished | True for Won/Lost |

Enemy.Health / MaxHealth is the health-bar ratio. Enemy.Distance is distance along
the path, not X/Y. Use `BastionMap.PositionAt(enemy.Distance)` for coordinates.
Enemy.SlowTicks > 0 indicates an active frost effect. Tower identity is PadId.
Enemy IDs are unique within a run and restart at 1 in a new run.

## Map, catalogs and queries

```csharp
BastionMap.Width;                         // 960
BastionMap.Height;                        // 540
BastionMap.Path;                          // ImmutableArray<Position>
BastionMap.Pads;                          // ImmutableArray<BuildPad>, IDs 1..10
BastionMap.PositionAt(enemy.Distance);    // Position(X, Y)
BastionMap.FindPad(padId);                // Option<BuildPad>
TowerCatalog.All;                         // all three definitions
TowerCatalog.Get(tower.Kind).AtLevel(tower.Level);
EnemyCatalog.All;
GameQueries.TowerAt(Session.State, padId); // Option<Tower>
GameQueries.UpgradePrice(tower);          // None at level three
GameQueries.Describe(enemy);              // description for the codex
GameRules.SellRefund(tower);
WavePlanner.Describe(nextWave);           // recursive wave description
WavePlanner.Schedule(nextWave);           // exact schedule
```

Use SVG `viewBox="0 0 960 540"` for the board. Its CSS dimensions may change, but
the world coordinates never do. A selected tower's range is a circle centered on
its pad with the radius returned by its current TowerStats.Range. Avoid hardcoded
duplicated catalogs in Razor/JavaScript. Only request waves 1..10 and levels 1..3.

## Events and animations

GameTransition contains State and an ImmutableArray of effects generated by that
one operation. Consume each update once. They are not a persistent event log.

| Event | UI use |
|---|---|
| TowerBuilt / TowerUpgraded / TowerSold | Build/upgrade feedback, clear sold selection |
| EnemySpawned | Optional short spawn effect |
| ShotFired(PadId, Kind, Target) | Draw a brief beam/shell/frost effect toward Target |
| EnemyKilled(EnemyId, Position, Gold, Points) | Death particles, optional small reward label |
| EnemyEscaped(EnemyId, CoreDamage) | Core hit feedback |
| WaveStarted(Number) | Clear old planning hint, update header |
| WaveCleared(Number, Gold, Points) | Inter-wave summary |
| GameEnded(Won, Score) | Show results and consider updating the record |
| GameRestarted | Clear effects, selected pad, dialogs and transient errors |

Cap effects (for example 100) and remove them after their animation ends. Cosmetic
animation may interpolate; it must not change authoritative enemy coordinates.
Reduced-motion mode may shorten/disable animations without altering the game.

## High-score storage

```csharp
HighScores.StorageKey;                  // chrono-bastion.high-score.v1
HighScores.Parse(string? json);         // Result<Option<HighScore>, string>
HighScores.FromFinishedGame(state);     // None unless Won/Lost
HighScores.Best(previous, candidate);   // compares score, then cleared waves
HighScores.Serialize(record);           // JSON, source-generated serializer
```

Schema: `Version`, `Score`, `WavesCleared`, `Won`. The version is 1. Use the codec
instead of a second JSON model. Storage I/O belongs in BrowserHighScoreStore.

On first render: read localStorage inside a try/catch, parse, and retain the Option
in memory. On GameEnded: get the candidate, compare it with the existing record,
update the in-memory record, and attempt to write the selected record. Missing data
is normal. Corrupt/unavailable storage should produce a nonblocking message such
as "Your record could not be saved on this device." Never break a run or save
GameState. Saving should not be triggered by every tick or every render.

Keep browser operations async. If persistence is unavailable, the in-memory record
can still work for the current visit. There is no server or cross-device sync.

## Stable behavior

Use callbacks/parameters to compose Blazor components. A board should report a pad
selection; its parent chooses a command. Child components should not each own a
new GameSession. Presentation state such as selected pad, menu visibility, mute
and effect lifetimes belongs in Web. Domain state belongs in Application/Core.
