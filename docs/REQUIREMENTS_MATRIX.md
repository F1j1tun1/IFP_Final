# Assessment coverage

The assessment has twenty criteria. These are evidence locations, not a predicted
grade. The final deck includes team names, topic, aim, objectives, plan, technology
comparison, architecture, practical significance, methodology and real screenshots.

| Criterion | Evidence |
|---|---|
| 1. Structure/design | Core → Application → Web dependency boundary; ARCHITECTURE.md |
| 2. Reusability | GameBoard, GameHud, TowerPanel and dialogs use parameters/EventCallbacks |
| 3. Cross-platform | Standalone WASM; Windows/Linux CI; exact browser/device limits in VERIFICATION.md |
| 4. State management | One injected GameSession; Home owns serial Dispatch/Advance and subscription |
| 5. UI design | Original SVG fortress, tower/enemy shapes, range and health indicators; screenshots |
| 6. UX | Menu, field manual, action feedback, confirmations, finished-run results |
| 7. Responsiveness | Fixed SVG world coordinates, stacked phone layout, large pad buttons; browser viewport tests |
| 8. Functionality | All three towers/levels, target modes, ten waves, boss, win/loss and fresh restart |
| 9. Advanced features | Deterministic combat, armor, splash, slowing, composed target ranking |
| 10. Resilience | Result errors, validated record codec, blocked/corrupt storage tests, hidden-tab pause |
| 11. Efficiency | One 33ms UI clock, fixed engine ticks, no per-frame I/O; bounded effects |
| 12. Scalability | Static delivery, independent local sessions; no unmeasured load or multiplayer claim |
| 13. Resources | Timer cancellation, event unsubscribe, JS listener/reference disposal; menu-cycle tests |
| 14. Test coverage | 70 Core/Application + 13 component/storage tests; complete browser runs |
| 15. QA | Release browser suite, error/network checks, screenshot inspection; VERIFICATION.md |
| 16. Documentation | README player/setup guide, API contract, design, release instructions and evidence |
| 17. Innovation | Mechanical fortress artwork and tactical feedback around the functional simulation |
| 18. FP/C#/Blazor | Option, Result, Map/Bind, recursion, partial application, immutable records, component composition |
| 19. Professionalism | Reviewed changes, asset credits, reproducible scripts; deployment status in VERIFICATION.md |
| 20. Presentation | presentation/Chrono-Bastion.pptx; ten English slides, speaker notes and real captures |

Part 1: Yerkingali Saruar (engine, session, architecture, simulator, domain tests).
Part 2: Ainabek Chingiskhan (interface, browser integration/tests, final materials
and publication). Both are IT-2502. There is no third planned development stage.

The assessment's SignalR/cloud/security items are examples, not requirements for
this single-player game. No unnecessary backend or account system was added.
