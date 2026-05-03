# Autominion

Autominion is a Dalamud plugin for FFXIV that dismisses your minion when you step onto a housing plot and summons it again when you leave that plot.

## Current behavior

- Detects transitions onto and off of housing plots.
- Dismisses the currently summoned minion when you enter a plot.
- Keeps minions dismissed when you move from a plot into a house/apartment interior.
- Summons your selected owned minion, or Minion Roulette if configured, when you leave a plot.
- Uses delayed post-transition action checks to avoid firing during bad game states.

## Commands

- `/autominion` opens the main window.
- `/autominion config` opens settings.
- `/autominion rescan` forces an immediate housing-state recheck.

## Requirements

- Dalamud API 15
- .NET 10
- FFXIV Patch 7.5 validated

## Build

1. Open `Autominion.sln` in Visual Studio or Rider.
2. Build the solution.
3. The dev output is `Autominion/bin/x64/Debug/Autominion.dll`.

## In-game testing

Recommended checks:
- Stand in a housing ward street, then step onto a plot and confirm dismissal.
- Enter the house interior from the plot and confirm it stays dismissed (no resummon).
- Step back off the plot and confirm summon.
- Pick a specific owned minion in settings and verify that minion is the one summoned after plot exit.
- Enable roulette fallback and confirm it only applies when no specific minion is selected or the specific summon path fails.

## Notes

- The minion selector only shows owned minions.
- The plugin is built around plot-entry and plot-exit transitions, not general housing-zone entry.

