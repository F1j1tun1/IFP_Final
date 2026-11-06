# Assessment coverage

The supplied requirements describe 20 equally weighted criteria. This matrix maps
them to evidence and ownership; it does not guarantee a grade. The final presentation
must also include names, topic, aim, goals, plan, technology comparison, architecture,
practical significance and methodology/mockups.

| Criterion | Part 1 evidence | Part 2 completion evidence |
|---|---|---|
| 1. Code structure and design | Core/Application/Web dependency boundaries; pure rules | Maintain boundaries in components/services |
| 2. Component reusability | Stable commands, state and catalogs | Reusable board, HUD, tower panel and dialogs with parameters/callbacks |
| 3. Cross-platform compatibility | .NET projects; Windows/Linux CI | Actual browser/device checks with results recorded |
| 4. State management | GameSession with immutable snapshots and typed commands | One session, correct lifecycle and render subscriptions |
| 5. UI design | UI_SPEC and game world data | Finished coherent vector design and screenshots |
| 6. UX | Explicit rules and actionable error messages | Clear flow, help, feedback, keyboard/touch controls |
| 7. Responsiveness | Fixed world coordinates independent of screen pixels | Laptop/mobile layouts and viewport checks |
| 8. Core functionality | Entire battle, upgrades, waves, results and restart | Real UI connected to every required action |
| 9. Advanced features | Deterministic simulation; composable targeting; splash/slow interaction | Expose these features clearly; optional polished effects |
| 10. Error handling and resilience | Typed errors, validated record codec, bounded timing | Storage failure handling, visible errors, safe lifecycle |
| 11. Efficiency | Fixed ticks, cached immutable content, bounded world | Profile rendering and loading; avoid redundant updates |
| 12. Scalability | Content separated from rules; independent client sessions | Static delivery; explain scope honestly, no unsupported load claims |
| 13. Resource utilization | No per-frame I/O, limited towers/waves | Dispose loops/events, cap effects, test repeated runs |
| 14. Testing coverage | Unit/session tests and full-game simulator | Component and browser end-to-end tests |
| 15. Quality assurance | Deterministic win/loss checks and regression cases | Manual cross-browser/device checks and fixes |
| 16. Documentation | Architecture, API, rules, handoff, run/deploy instructions | Player guide, screenshots and final verification evidence |
| 17. Innovation | Mechanical fortress interpretation; functional simulation design | Distinct visual execution and clear tactical feedback |
| 18. Creative FP/C#/Blazor use | Option, Result, Map/Bind, partial application, recursive waves | Component composition, session binding and browser adapters |
| 19. Professionalism | Consistent build, tests and ownership boundary | Finished UI, credits, clean repo and verified live deployment |
| 20. Presentation | DEFENSE outline and code explanations | Final English slide deck, real demo and rehearsal |

The document's examples of SignalR/cloud services/role security are examples, not
mandatory technologies. A single-player local game does not need those services.
The chosen advanced mechanics still need to be demonstrated and explained well.
The final assessment remains the instructor's decision.
