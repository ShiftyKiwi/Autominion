# Autominion

Autominion is a Dalamud plugin for FFXIV that reacts to local housing state changes.

Current behavior:
- Summons a minion when you are on a housing exterior plot or yard.
- Dismisses a minion when you enter a housing interior.
- Dismisses a minion when you enter an apartment.
- Waits for a safe post-zone window before acting.

## Commands

- `/autominion` opens the main window.
- `/autominion config` opens settings.
- `/autominion rescan` forces an immediate housing-state recheck.

## Current scope

This version is built for:
- Dalamud API 14
- .NET 10
- FFXIV patch 7.45 validation target

The housing detection path is implemented with `FFXIVClientStructs` housing manager data. Summon timing is modeled after MinionRoulette-style delayed execution after zone and state changes.

## Build

1. Open [Autominion.sln](c:\Users\Nick\Documents\FFXIVPlugins\Repos\Autominion\Autominion.sln) in Visual Studio or Rider.
2. Build the solution.
3. The dev output is [Autominion.dll](c:\Users\Nick\Documents\FFXIVPlugins\Repos\Autominion\Autominion\bin\x64\Debug\Autominion.dll).

## In-game testing

Recommended test cases:
- Enter a housing exterior from a non-housing zone.
- Enter a private or FC house interior.
- Enter an apartment.
- Move around within the same housing area and confirm it does not spam actions.

## Known focus area

The housing transition detection is the most certain part of the implementation. The dismiss path should be verified in-game on 7.45, since that action flow was not directly available from the comparison repos.
