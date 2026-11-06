# Chrono Bastion game rules

Defend a mechanical energy core on one winding path. A run starts with 300 gold,
20 core health, no towers and no enemies. The player starts each wave explicitly.
The world is 960 by 540 units. All coordinates come from `BastionMap`.

## Scope

Ten waves, ten fixed build pads, three tower types with three levels each, three
regular enemy types and one final boss. Mouse/keyboard and touch controls are
required. Pausing and speed 1/2 are supported. A loss ends the run completely.
Restart resets health, gold, towers, enemies, wave, score, speed and pause state.
Only the best finished-run record survives a reload.

## Towers

Prices for levels 2/3 are additional upgrade prices. Damage is before enemy armor.
Intervals below are game seconds, independent of the display frame rate.

| Tower | Level | Price | Damage | Range | Interval | Effect |
|---|---:|---:|---:|---:|---:|---|
| Pulse | 1 | 90 | 12 | 135 | 0.40 | Single target |
| Pulse | 2 | 75 | 20 | 150 | 0.40 | Single target |
| Pulse | 3 | 120 | 32 | 165 | 0.35 | Single target |
| Mortar | 1 | 140 | 28 | 160 | 1.40 | Splash radius 65 |
| Mortar | 2 | 110 | 46 | 175 | 1.30 | Splash radius 75 |
| Mortar | 3 | 160 | 72 | 190 | 1.20 | Splash radius 85 |
| Frost | 1 | 100 | 5 | 140 | 0.80 | Speed x0.6 for 1.75 seconds |
| Frost | 2 | 80 | 8 | 155 | 0.70 | Speed x0.5 for 2.00 seconds |
| Frost | 3 | 130 | 12 | 170 | 0.60 | Speed x0.4 for 2.25 seconds |

Sell refund is floor(70% of the build price plus all paid upgrades). Selling never
awards score. Upgrading preserves the current attack cooldown. Starting a new wave
resets cooldowns so all towers are ready.

Target choices: **First** selects the enemy closest to the core, **Strongest** the
highest current health, and **Nearest** the smallest physical distance to the tower.
All choices are restricted to living enemies in range. Ties use the lower enemy ID.
Mortar splash can hit other enemies outside the tower's range if they are inside
the impact radius. Every affected enemy receives the listed damage after armor.

Frost effects do not multiply. Repeated hits keep the strongest active slow and
the longer remaining/new duration. A weaker frost hit can therefore refresh a
stronger active slow. Slow expiry restores normal speed.

## Enemies

| Type | Base health | Speed | Armor | Gold | Points | Core damage |
|---|---:|---:|---:|---:|---:|---:|
| Grunt | 35 | 60 | 0% | 12 | 20 | 1 |
| Runner | 25 | 95 | 0% | 10 | 25 | 1 |
| Armored | 95 | 40 | 35% | 22 | 40 | 2 |
| The Overseer | 1300 | 25 | 25% | 250 | 1000 | 20 |

Health multiplier is `1 + (wave - 1) * 0.1`. Speed is world units per game second.
Damage is `max(1, tower damage * (1 - armor))`. Escaped enemies award no kill gold
or kill points. An escaped boss destroys even a full-health core.

## Wave schedule

| Wave | Grunts | Runners | Armored | Boss |
|---|---:|---:|---:|---:|
| 1 | 5 | 0 | 0 | 0 |
| 2 | 6 | 2 | 0 | 0 |
| 3 | 7 | 2 | 0 | 0 |
| 4 | 8 | 3 | 2 | 0 |
| 5 | 9 | 3 | 2 | 0 |
| 6 | 10 | 4 | 3 | 0 |
| 7 | 11 | 4 | 3 | 0 |
| 8 | 12 | 5 | 3 | 0 |
| 9 | 13 | 5 | 4 | 0 |
| 10 | 14 | 6 | 4 | 1 |

Grunts spawn first, followed by reinforcements. Grunt spacing is `24 - wave` ticks
(minimum 12), runner spacing 14, armored spacing 24. One tick is 0.05 seconds.
The next group starts one spacing interval after the previous group's last enemy.

Completing wave N awards `30 + 5*N` gold and `100*N` points. Completing wave ten
also awards 1000 victory points plus 50 for every remaining core-health point.
Scores increase only through kills and these completion bonuses. Both a loss and
a win are eligible to replace the local record. Equal scores prefer more cleared waves.

## Tested strategy

The console example buys towers between waves using this order:
2 Pulse, 5 Mortar, 6 Frost, 1 Pulse, 9 Pulse, 8 Mortar, 4 Pulse, 7 Frost,
10 Pulse, 3 Mortar. It waits when the next tower is unaffordable. Once all pads
are occupied, it upgrades towers in that same order using earned gold.

This is a reproducible test strategy, not an automatic player in the final UI.
It wins with normal resources; an undefended run loses during wave three.
The graphical game still needs human playtesting for clarity and enjoyment.
