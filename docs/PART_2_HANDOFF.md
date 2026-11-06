# Part 2 integration

The project has exactly two sequential ownership parts:

1. Yerkingali Saruar: architecture, Core, Application, simulator, domain tests and
   the public integration contract. Foundation commit: `65122e3`.
2. Ainabek Chingiskhan: Blazor interface, browser storage and visibility adapters,
   component/browser tests, final documentation, presentation and publication.

Both students are in IT-2502. No further development stage is assigned to Part 1.

## Implemented integration

`Home.razor` owns one injected `GameSession`, one `PeriodicTimer`, selection and
screen state. It subscribes once to `Updated`, dispatches commands and advances
elapsed time on Blazor's synchronization context. Disposal cancels and awaits the
clock, removes the subscription and disposes JavaScript listeners/references.

`GameBoard`, `GameHud`, `TowerPanel`, `MainMenu`, `HelpDialog` and `ResultDialog`
communicate through parameters and EventCallbacks. The board uses `BastionMap`
coordinates and catalog data. Shot/death effects expire and are capped; they never
apply combat damage. No Core/Application public APIs were changed.

`BrowserHighScoreStore` reads once, uses the existing codec, and saves only on a
finished run. Errors remain nonfatal. The visibility adapter pauses combat and
requires manual Resume. Menu transitions discard an unfinished run only after
confirmation; a new run always dispatches Restart.

## Running and reviewing

Use README for local setup and player instructions. Run `scripts/Test.ps1`,
`scripts/Publish-Pages.ps1` and `scripts/Test-Browser.ps1`. The browser suite uses
ordinary UI controls and the simulator's documented strategy with real resources.

Actual results, environment details and remaining limitations are recorded in
`docs/VERIFICATION.md`. Publication follows `docs/DEPLOYMENT.md` after review and
green CI. Both teammates should rehearse the explanations in `docs/DEFENSE.md`.
