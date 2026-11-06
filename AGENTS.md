# Chrono Bastion working agreement

Read `README.md`, `docs/ARCHITECTURE.md`, `docs/API_CONTRACT.md`, and
`docs/PART_2_HANDOFF.md` before changing the application.

This project has exactly two sequential ownership parts:

1. Yerkingali Saruar: architecture, Core, Application, simulation, domain tests,
   and the handoff. This is the completed foundation.
2. Ainabek Chingiskhan: the Blazor game interface, browser integration, component
   and browser tests, presentation, and public deployment.

Both students are in IT-2502. All project text is English. Their current IDE is
VS Code on Windows. Keep instructions beginner-friendly and code straightforward.

## Product boundaries

- Single-player real-time tower defence on .NET 10 / standalone Blazor WebAssembly.
- One map, ten waves, fixed build pads, Pulse/Mortar/Frost, three levels per tower.
- Grunt/Runner/Armored enemies and one boss in wave ten.
- Losing ends the whole run. Restart always returns to wave one.
- No rewind, checkpoint, saved run, accounts, backend, paid service or multiplayer.
- Only a local high score persists. Never serialize GameState as a saved game.
- Laptop-first layout, playable touch controls on phones.
- Free GitHub Pages deployment after Part 2 is complete.

## Implementation boundaries

- Preserve the public Core/Application contracts. Fix a demonstrated core bug with
  a regression test if necessary; do not rewrite the engine for the interface.
- The UI dispatches GameCommand values through GameSession. It never edits health,
  gold, score, enemy positions, cooldowns or wave schedules itself.
- GameSession must be called serially on the Blazor synchronization context.
- JavaScript may handle browser capabilities; combat remains in C#.
- Use the catalogs and map data instead of duplicating prices or coordinates.
- Add comments only for decisions that are not apparent from names and structure.
- Do not add packages, infrastructure or features without a concrete need.

## Verification

Run `pwsh -File scripts/Test.ps1` and `pwsh -File scripts/Publish-Pages.ps1`.
Part 2 must additionally verify the actual browser game, mobile layouts, storage
failure handling, a complete win/loss/restart flow, and the deployed URL.
Record what was actually checked in `docs/VERIFICATION.md`. Do not report unrun
browser/device checks as passed. Do not publish the foundation-only shell as the
finished game. The Pages workflow is deliberately manual.
