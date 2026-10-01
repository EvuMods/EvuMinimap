# Agent Instructions

These instructions apply to the EvuMinimap repository.

## Read Order

1. Read [README.md](README.md) for what the mod does.
2. Read [ROADMAP.md](ROADMAP.md) before adding a feature that is listed as later work.
3. Read [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for the toolchain, project split, and verification.
4. Read [docs/CONFIGURATION.md](docs/CONFIGURATION.md) before changing settings, defaults, or hotkeys.

## Product Boundary

EvuMinimap is a client-side BepInEx plugin. It changes the small minimap's size, anchor, offset, shape mask, and icon alpha, and it can slide the vanilla buff strip off that map. A global Enabled setting, on by default, leaves that HUD vanilla when off.

It does not change the large map, sync anything to a server, or depend on Jotunn or ServerSync. Dedicated server processes load the plugin and return without touching the HUD.

## Project Split

- `src/EvuMinimap.Core` is `netstandard2.0` and has no Unity types. Layout, scale limits, and buff clearance live here so they can be tested without the game.
- `src/EvuMinimap` is the `net48` BepInEx plugin. It reads config, captures the vanilla rect once, and writes the solved layout in `LateUpdate`.
- `tests/EvuMinimap.Core.Tests` covers the core. In-game checks are manual.

`MinimapProfile` is the saved unit. Five profiles are stored. Profile 1 keeps the original `Minimap` keys, and `Profiles` / `Active` selects which one is shown. The solver and the applier take one profile.

Shape mask swaps the small-map mask from the applier. Icon alpha is a canvas group on that root and does not fade the terrain.

## Verification

Run `make verify` after code changes. That fetches reference assemblies when needed, builds the solution, and runs the tests.

Do not commit `.refs/`, `bin/`, `obj/`, or `dist/`. Valheim and BepInEx binaries stay out of git.

## Commits

Use [Conventional Commits](CONTRIBUTING.md). release-please opens release pull requests from those messages. A `fix: rebuild against Valheim <buildid>` commit is reserved for the game-update workflow.
