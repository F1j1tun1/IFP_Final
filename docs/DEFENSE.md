# Defense and presentation notes

Team: **Yerkingali Saruar** and **Ainabek Chingiskhan**, **IT-2502**.
Topic: **Chrono Bastion — a functional tower defence game in C# and Blazor**.
The assessment describes a 15–20 minute group defense. Either student may be
asked about any part. Both should understand the following explanations.

## Presentation outline

Prepare an English slide deck after the graphical game is complete. Use actual
screenshots and measured results. This outline is not a finished slide deck.

| Slide | Content | Approximate time |
|---|---|---|
| 1 | Title, full names, group, division into two parts | 30 seconds |
| 2 | Aim: build a playable browser game using functional concepts; concrete objectives | 1 minute |
| 3 | Technologies considered and why C# / Blazor WebAssembly fits the course | 1 minute |
| 4 | Architecture and the direction of dependencies | 1 minute |
| 5 | Functional concepts with two short real code examples | 2 minutes |
| 6 | UI structure, reusable components, state flow and desktop/phone design | 1 minute |
| 7 | Live demo: build, upgrade, start, pause, speed, record and fresh restart | 3 minutes |
| 8 | Testing evidence and error handling, including a losing scenario | 1 minute |
| 9 | Contributions, limitations, practical value and improvements | 1 minute |
| 10 | Working URL/repository and questions | Remaining time |

## Aim, objectives and practical significance

Aim: demonstrate that functional programming can express and test the rules of
an interactive real-time game while Blazor supplies a reusable web interface.

Objectives: model immutable state; implement the battle rules; make invalid actions
explicit; compose small functions; build a responsive interface; verify the rules
and user flows; publish a game that runs without an application backend.

Practical value: reproducible simulations make debugging and balancing easier.
The same engine runs in a console, automated tests and a browser. Separating the
rules also makes the two-person handoff concrete and reduces integration conflicts.
The final UI provides an accessible demonstration of otherwise abstract FP topics.

## Technology comparison

This is a project-fit comparison, not a performance benchmark.

| Approach | Fit for this project | Decision |
|---|---|---|
| Console C# | Excellent for logic/testing; weak fit for the required visual and responsive experience | Keep as verification tool |
| Desktop UI | Can produce a desktop game, but adds another delivery path for phones | Not selected |
| JavaScript game | Fits browser graphics, but shifts the main rules away from the C#/FP course focus | Keep JS limited to browser capabilities |
| Standalone Blazor WebAssembly | Runs C# in the browser and supports static deployment and reusable components | Selected |

The book uses .NET 8; the project uses the installed .NET 10 SDK while applying
the same FP concepts. No server-side application is needed for a local single-player
game. This is a design choice, not a claim that Blazor is the best engine for every game.

## Functional concepts in the actual code

| Concept | Code to open | Explanation |
|---|---|---|
| Pure functions | Core/Rules/CombatRules.cs | Damage and movement depend on input values and return new values |
| Immutable state | Core/Model/GameState.cs and GameEngine.cs | Records and immutable arrays preserve previous snapshots |
| Explicit absence | Core/Functional/Option.cs; GameQueries.TowerAt | An empty pad is normal, represented by None |
| Explicit failure | Core/Functional/Result.cs; GameEngine.Execute | Invalid player actions return a typed error |
| Map and Bind | GameEngine.Upgrade | Find a tower, validate it, then transform state; failure stops later steps |
| Higher-order functions | CombatRules.SelectTarget / RankBy | Target selection receives a ranking function |
| Partial application | CombatRules.DamageFor | Fix tower stats and return a function that accepts an enemy |
| Recursion | WavePlanner.CountEnemies / Flatten | Waves contain groups of packs and nested reinforcements |
| Composition and LINQ | WavePlanner.Schedule; GameQueries | Small transformations build the final result |
| Isolated side effects | Application/GameSession and Web adapters | State replacement, time, storage and rendering are outside pure rules |

We use the concepts where they support the game. We do not claim to implement every
exercise or every topic in the book. In particular, this project does not need an
expression-tree query engine or an artificial SQL backend.

## Questions to rehearse

**What makes a function pure?** Its result depends only on its inputs and it does
not change observable external state. Calculate an armored hit twice: it returns
the same health, and the input Enemy still has its original health.

**Why are local lists allowed inside Tick?** They are private working buffers. No
input collection is changed or exposed. The returned GameState uses immutable arrays.

**What is the difference between Map and Bind?** Map transforms a successful value
with a normal function. Bind chains a function that itself can return failure.
Both leave a failure unchanged and skip the next operation.

**What is a monad here?** Result is a context for success/failure with a Bind
operation. The upgrade pipeline chains operations in that context. Explain the
actual example before trying to recite terminology.

**Why not use exceptions for insufficient gold?** It is a normal player outcome.
A typed error makes it predictable and lets the UI show a useful message. We still
throw for programming mistakes, such as requesting a nonexistent catalog level.

**Why does the game have a fixed tick?** Gameplay should not become stronger or
weaker with a different display refresh rate. The session converts elapsed time
into 50 ms steps; speed 2 performs twice as many steps.

**Where is state stored?** One GameSession owns the latest in-memory GameState.
The UI renders it and dispatches commands. Only the finished-run record goes to
localStorage, which belongs to that browser and origin.

**Why is there no rewind despite the title?** The agreed rules use the time-themed
visual setting but deliberately require a completely fresh run after losing.

**What happens when the tab is hidden?** The final UI pauses it and requires manual
resume. The session also bounds catch-up work so a delayed callback cannot simulate
an arbitrarily long unseen battle. Part 2 must verify the visibility adapter.

**Can the record be cheated?** It is a local convenience, not a competitive online
ranking. A user controls their own browser storage. No anti-cheat claim is made.

**What has been tested?** Cite VERIFICATION.md. Distinguish core/session unit tests,
full headless simulations, component/browser tests and physical device checks.
Do not call an emulated mobile viewport a physical-phone test.

**What did each person contribute?** Explain the two ownership parts and show the
real commits. Each student should also explain the other part's public boundaries.

## Final preparation

Have a local development server ready as a fallback for network trouble. Rehearse
a short build/upgrade/pause demonstration. Use the documented winning strategy to
prepare a final-wave demo if needed; do not add a public cheat menu. Bring the actual
test output, diagram, source file examples and verified deployed URL.
