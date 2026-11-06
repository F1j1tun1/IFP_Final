# Part 2 handoff to Ainabek Chingiskhan

You are completing **the second and final ownership part** of Chrono Bastion.
Yerkingali Saruar's first part supplies the game engine, architecture and tests.
Your part is the graphical Blazor game, browser integration, interface tests,
presentation and publication. Both students are in IT-2502.

The project must be finished quickly. Keep the agreed scope; implement a complete,
readable game before optional audio or extra decoration. You are not expected to
invent combat rules or learn a second framework before starting.

## What already works

- A complete ten-wave battle on one fixed map with ten build pads.
- Three tower types, three upgrade levels, splash damage, slowing and armor.
- Building, upgrading, selling, target selection, gold, points and core health.
- Wave start, pause, speed 1/2, victory, defeat and a full restart.
- Immutable state, Result/Option, pure functions, recursion and composition.
- GameSession, which owns state and converts elapsed time to simulation ticks.
- A validated high-score format and comparison rules; browser storage is yours.
- Automated tests and a headless winning/losing demonstration.
- A buildable Blazor shell with GameSession registered.
- Windows/Linux CI and a manual GitHub Pages release workflow.

The current web home page only displays the title. That is the intended handoff
point, not a broken game. Do not spend time trying to find a missing renderer.

## First setup on your Windows computer

Install Git, the .NET 10 **SDK** (not only the runtime), VS Code and C# Dev Kit.
Open VS Code's terminal using PowerShell. You can run dotnet commands directly;
PowerShell 7 (`pwsh`) is useful for the supplied helper scripts.

If you have not cloned the repository:

```powershell
git clone https://github.com/F1j1tun1/IFP_Final.git
cd IFP_Final
```

If you already have a clone, open its folder and first run `git status`. Preserve
your own uncommitted work before switching branches. On a clean working tree:

```powershell
git switch main
git pull --ff-only origin main
git switch -c feat/part-2-interface
dotnet --list-sdks
dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build
dotnet run --project tools/ChronoBastion.Simulator -c Release --no-build -- all
dotnet run --project src/ChronoBastion.Web --launch-profile http
```

If the part-two branch already exists, switch to it instead of creating it again.
The simulator must report Won for the defended run and Lost for the undefended
run. Initial tests must pass before you change code. Use your own GitHub account
and commit identity; do not copy Yerkingali's Git configuration.

## Give your GPT the context

Open the repository as its workspace. Paste the entire text from
`docs/PART_2_GPT_PROMPT.md` into your GPT/Codex conversation. It must actually read
the repository documents, not work from this sentence or a screenshot alone.
Start with README, ARCHITECTURE, API_CONTRACT, GAME_DESIGN and UI_SPEC.

## Work sequence inside your part

### Connect one real screen

Replace Home.razor with the application screen. Inject the existing GameSession.
Add a single owner component for the timer and event subscription. Render the
map path from BastionMap.Path and pads from BastionMap.Pads using an SVG viewBox.
Show the real gold, health, wave and score values.

Build the smallest genuine flow first: select a pad, choose Pulse, dispatch
BuildTower, start a wave, draw enemies at PositionAt(enemy.Distance), and observe
damage/kills through the actual session. This establishes integration before polish.
Do not create a parallel JavaScript game or fill the board with simulated UI data.

### Complete controls and screens

Implement MainMenu, GameBoard, GameHud, TowerPanel, Help/Codex and ResultDialog
(names can vary). Use component parameters and EventCallback boundaries.
The parent manages selected pad and screen state; child components render data
and report actions. Reuse tower cards/stat rows instead of duplicating markup.

For empty pads show all tower prices and roles. For occupied pads show level,
damage, range, upgrade price, sell refund and target-priority selection. Use
GameQueries/TowerCatalog. Display returned GameError messages near the action.
Provide Start wave, Pause/Resume, x1/x2 and restart confirmation. Show clear Won/Lost
results with Play again. A restart always returns to a new run.

### Add browser behavior

Implement BrowserHighScoreStore using localStorage and the supplied HighScores
codec. Read once after the first render, compare/write on GameEnded, and handle
denied storage and malformed JSON without interrupting gameplay. A failed write
still permits an in-memory record for the visit. Never save GameState.

Use one timer as described in API_CONTRACT. Invoke Advance/Dispatch serially through
Blazor's context. Dispose timers, subscriptions and JS references. Pause on a hidden
tab and require the user to resume. Do not multiply elapsed time by speed twice.

### Finish the visual design and phone controls

Follow UI_SPEC: dark mechanical fortress, original vector artwork, readable HUD,
cyan Pulse, amber Mortar and icy Frost. Tower placement and enemy types need
distinct shapes. Animate ShotFired/EnemyKilled as short-lived effects; the damage
already happened in Core. Cap effects and honor reduced motion.

On a phone, stack the board and controls. Use tap-to-select plus explicit build
buttons, large touch targets, no hover-only actions and no horizontal page overflow.
Keep all ten pads reachable. Optional sound must follow a player gesture and have
a mute control. Add exact credits for any imported free assets.

### Test the real interface

Keep the existing tests. Add component tests for HUD updates, build/upgrade errors,
selected-pad actions and result/restart behavior. Add a browser suite for the real
page: menu, build, wave start, pause/resume, x2, loss, restart, record persistence,
and mobile controls. Choose a .NET-compatible component test tool and browser tool
with current supported versions; only add packages you actually use.

Use a test-only harness if needed to speed full-game browser checks. Do not ship
cheat controls or fake state on the public page. The console winning strategy can
guide a real browser win. Verify that a discarded/restarted run does not save its
unfinished score. Check tab hiding, malformed/blocked storage, max upgrades,
insufficient gold, rapid double clicks and repeated new games.

Run all tests, the simulator and the release packaging script. Check the browser
console/network and test a published build, not only the development server.
Update VERIFICATION.md with actual results, browsers and viewports.

### Finish the submission and publish

Prepare the final English README/user instructions, real screenshots, completed
asset credits and presentation. DEFENSE.md supplies the talk structure and core
explanations. Use REQUIREMENTS_MATRIX.md to check that each assessment category
has concrete evidence. Do not claim features or tests you did not implement/run.

Commit your actual changes in the part-two branch, push it, open a pull request,
review its diff and merge it after CI is green. Do not force-push main. Then follow
DEPLOYMENT.md to enable Pages, run Deploy Pages from main, and verify the live URL.
An admin-only repository setting may need F1j1tun1, but coding your part should not
wait for that setting. The end result is a playable public game and a complete repo.

## Final acceptance checklist

- [ ] The game loads in a laptop browser and a phone-sized viewport.
- [ ] The UI uses the supplied engine/session, with one active timer.
- [ ] All three towers build only on marked pads; prices come from the catalog.
- [ ] Upgrades stop at level three; sale refunds are correct; errors are visible.
- [ ] Target-priority options affect real combat.
- [ ] All ten waves, armor, splash and slow effects behave correctly.
- [ ] Pause, speed and hidden-tab behavior do not create extra simulation loops.
- [ ] Defeat starts no next wave; Play again resets the whole game.
- [ ] Victory is reachable; the final result reflects actual engine state.
- [ ] Only the record persists, survives reload and tolerates storage errors.
- [ ] Keyboard focus and touch targets are usable; no required hover controls.
- [ ] Tests, the two full simulations and release publishing succeed.
- [ ] No startup placeholders, debug controls, broken assets or console errors remain.
- [ ] English documentation, actual verification evidence, credits and presentation are ready.
- [ ] The live GitHub Pages URL works after refresh on the repository subpath.

## What to send back

Send the final commit/PR, verified public URL, test summary and presentation location.
Both teammates should read DEFENSE.md and walk through each other's code once.
The instructor can ask either student about any part of the project.
