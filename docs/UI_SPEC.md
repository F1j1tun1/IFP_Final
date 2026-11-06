# Interface specification for Part 2

The interface is an English-language mechanical fortress game, not an admin
dashboard. The central object is the battlefield. Make actions and enemy movement
easy to read before adding decoration.

## Visual direction

- Dark navy background (`#0b1220`), steel panels (`#172337`), soft light text
  (`#edf2f7`) and muted secondary text (`#a8b8cd`).
- Cyan for Pulse, amber for Mortar, pale blue for Frost. Use shapes/icons as well
  as color. Red/orange identifies enemy danger and core damage.
- A bright energy core at the path endpoint, a visible entrance at its start,
  engraved mechanical lanes, circular build sockets and restrained glow effects.
- Original SVG/CSS vector artwork is the agreed style. Use free local assets only
  when they fit; record actual licenses and source URLs in ASSETS.md.
- Use a local/system font stack. Keep high contrast and readable numeric values.
- Brief visual feedback for firing, construction, upgrade and damage. Audio is
  optional and muted until a player gesture enables it. No paid asset dependency.

## Screens

1. **Main menu:** title, one-sentence premise, Play, How to play and local record.
2. **Game:** HUD, battlefield, build/selected-tower panel, start-wave/pause/speed
   controls, help access and restart confirmation.
3. **How to play / codex:** explain fixed pads, gold, upgrades, armor, frost,
   starting each wave, victory/defeat, and the three tower roles.
4. **Results:** Victory/Defeat, points, waves cleared, kills, remaining core health,
   best score and Play again. A defeat never offers Continue or Retry wave.

Use the root route and component/dialog state for these screens where practical.
This keeps GitHub Pages navigation simple.

## Desktop composition

HUD across the top: core health, gold, wave and score. A large 16:9 battlefield
occupies the main area; a compact tower panel sits on its right or directly below
when space is limited. Keep the start/pause/speed controls in a consistent place.

Click an empty pad to reveal the three build choices, costs and effects. Click an
occupied pad to see its name, level, damage, range, upgrade cost, sell refund and
target-priority choice. Show range while selected and a clear selection outline.
The panel should explain why a build/upgrade is unavailable. A maximum-level
tower displays "Max level" instead of a nonfunctional upgrade action.

The planning screen displays the upcoming wave and an explicit Start wave button.
During combat it displays active plus pending enemy counts. Pause shows a clear
overlay or state label and Resume. Start next wave is unavailable during combat.

## Phone behavior

- Verify at 390x844 and a small landscape viewport, as well as a laptop viewport.
- Stack the HUD, battlefield and bottom action panel; avoid page-wide horizontal
  overflow. Preserve the board aspect ratio and all world coordinates.
- Build by tap-selecting a pad, then tapping a tower choice. Do not require drag,
  hover, right-click, a keyboard or precision clicking.
- Use touch targets of approximately 44 CSS pixels. Transparent hit areas may be
  larger than the visible pad while retaining a clear selected state.
- Keep Start wave, Pause/Resume, x1/x2 and the selected-tower controls reachable.
- Use an SVG viewBox or accurately scaled overlays; do not use raw client pixels
  as world coordinates.

## Accessibility and lifecycle

Provide semantic buttons, descriptive labels, visible focus, keyboard activation,
and labels on icon-only controls. Do not encode enemy types or state only in color.
Respect prefers-reduced-motion. Avoid announcing changing health every frame in
an aria-live region; announce meaningful events/errors instead.

Opening help during combat should pause. Hiding the browser tab should pause;
returning to it should require manual Resume. Restart during an active run needs
a confirmation because it discards that run. Result-screen Play again can restart
directly. Menu navigation must stop/dispose the game clock or leave the game paused.

## Definition of visual completion

The final game has no starter page, debug commands, simulator controls, fake scores
or developer instructions in its player interface. Prices and health bars come
from the real engine. The local record survives reload. Verify both full game flows
and the published site; record any real limitation instead of claiming perfection.
