# Prompt for Chingiskhan's GPT

Copy the following prompt into a new coding-assistant conversation opened in this
repository. Everything needed is in the repository; the earlier chat is not required.

---

You are completing Part 2 of Chrono Bastion in
https://github.com/F1j1tun1/IFP_Final . I am Ainabek Chingiskhan (GitHub:
Chingiskhan07). My teammate is Yerkingali Saruar (GitHub: F1j1tun1). We are both
in IT-2502 and use VS Code on Windows. We are beginners. Explain essential run
steps plainly, but take care of routine implementation decisions yourself.

The team has exactly two sequential parts. Part 1 is already implemented by my
teammate: full architecture, pure C# game engine, application session, high-score
format, automated tests, a headless simulator and a Blazor starter project.
My Part 2 finishes the graphical game, browser integration, UI/browser testing,
documentation/presentation and free publication. Complete this part; do not just
give a plan, rebuild the engine, or send substantial unfinished work back to Part 1.

Before editing, inspect git status and read AGENTS.md, README.md,
docs/PART_2_HANDOFF.md, docs/ARCHITECTURE.md, docs/API_CONTRACT.md,
docs/GAME_DESIGN.md, docs/UI_SPEC.md, docs/DEPLOYMENT.md,
docs/REQUIREMENTS_MATRIX.md and docs/DEFENSE.md. Run the existing tests and simulator
to establish the baseline. Preserve any existing user changes. Work in
feat/part-2-interface, creating it from current main if it does not already exist.

Build this agreed product:

- English-language standalone Blazor WebAssembly game on .NET 10.
- One 960x540 world, ten fixed build pads, ten waves.
- Pulse, Mortar and Frost towers with three levels each.
- Grunt, Runner, Armored enemies and a boss in the final wave.
- Real-time combat, manual wave start, pause/resume, x1/x2 speed, building,
  upgrading, selling, target-priority selection, victory and defeat.
- Defeat ends the whole run. Play again starts completely fresh. No rewind,
  retry-wave/checkpoint system or saved ongoing game.
- Save only a local record using the supplied HighScores contract.
- No backend, authentication, database, multiplayer, paid service or extra maps.
- Laptop-first presentation with genuinely usable phone touch controls.
- Dark mechanical fortress and energy core; clean original vector graphics,
  restrained effects and readable controls. Free assets only, with real credits.

Use one injected ChronoBastion.Application.GameSession, already registered in
Web/Program.cs. Send GameCommand values through Dispatch. Drive the session with
one elapsed-time loop calling Advance(TimeSpan). The session applies x2 itself.
Subscribe once to Updated, process its events once, and clean up on disposal.
All state changes stay in C# Core/Application. Do not directly edit GameState,
health, gold or positions from the UI. Preserve the public contract. If you find
a real engine bug, demonstrate it with a regression test and fix it minimally.

Render the map and pads from BastionMap, positions through PositionAt, and all
prices/stats from the catalogs. ShotFired is cosmetic feedback for damage already
applied; do not apply damage again at animation completion. Keep effects bounded.
Implement menu, game board/HUD, tower panel, help/codex and result screen with
reusable Blazor components and clear parameter/event boundaries.

Handle missing, corrupted and unavailable localStorage gracefully. Use the
source-generated HighScores codec. Save only a finished-run result, after comparing
with the existing record; do not write every frame. Pause on hidden tabs and require
manual resume. Confirm restart while a run is active. Use relative asset paths for
GitHub Pages. Keep UI copy in English and technical explanations out of player flows.

Use readable code and short methods. Add comments only for non-obvious decisions.
Complete the actual implementation, then run the existing tests, add meaningful
component/browser tests, verify a real win/loss/restart and saved record, check a
laptop and mobile viewport, and inspect console/network errors. Test release output
as well as development. Record actual evidence in docs/VERIFICATION.md; never mark
unrun checks as passed or claim a device was tested if it was only emulated.

Finish the English README, player instructions, screenshots, asset credits and
presentation, using the existing defense outline and requirements matrix. Both
students must understand the whole application, so retain a clear explanation of
the functional concepts and how the interface consumes the core.

Run scripts/Test.ps1 and scripts/Publish-Pages.ps1. Commit and push my actual work
under my configured identity, open/review a PR, and merge only after checks pass.
Do not force-push. Follow docs/DEPLOYMENT.md to enable GitHub Pages and run the
manual Deploy Pages workflow from main. The intended URL is
https://f1j1tun1.github.io/IFP_Final/ . The user has authorized free publication of
the finished game. If an admin-only Pages setting requires the repository owner,
finish everything else and identify the exact setting needed.

Do not publish the starter shell as the finished game. When done, report the
verified live URL, commit/PR, test results, presentation location and any genuine
remaining limitation. Start implementation after the baseline inspection.
