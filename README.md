# EvuMinimap

Client-side Valheim mod that resizes the minimap and moves the point it grows from. Buff icons slide out of the way when the map covers them, and stay where Valheim put them when it does not.

## Features

- Scale from 0.5x to 5x vanilla. The default anchor is the top-right corner, so the map grows down and left.
- Move that anchor from its vanilla position. At 1x and zero offset, every anchor still matches the vanilla map.
- Alt+Numpad plus and minus change the size by one step. Either Alt key works.
- Buff icons move only when the minimap overlaps them.
- Reset restores the map and the buff behavior. Hotkeys stay as they are.

## Installation

Install with a mod manager, or copy `EvuMinimap.dll` and `EvuMinimap.Core.dll` into `BepInEx/plugins`. Both files have to sit in the same folder.

BepInExPack Valheim is required to play. Hexium assumes that pack, so this package does not list it as a dependency.

Start Valheim once. Settings are written to `BepInEx/config/evu.evuminimap.cfg`.

## Configuration

A configuration manager is optional. The mod uses BepInEx's config file, so Configuration Manager and the usual forks can edit it in game.

| Setting | Default |
| --- | --- |
| Scale | 1 (vanilla), from 0.5 to 5 |
| Anchor | Top-right |
| Offset | 0, 0 |
| Buff icons | Move only when the minimap overlaps them |
| Larger | Alt + Numpad + |
| Smaller | Alt + Numpad - |

One press changes the size by 0.25. Changing the anchor at 1x does not move the map. Later size changes grow away from the new anchor. An offset moves that anchor, in HUD units, from the place it has on the vanilla map.

The full list is in [docs/CONFIGURATION.md](docs/CONFIGURATION.md).

## Compatibility

Client only. The dedicated server loads the plugin and does nothing. The large map is unchanged.

If Minimal Status Effects or StatusQuo is installed, buff icons are left alone.

## Build

See [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md). `make verify` builds the plugin and runs the layout tests.

## License

[MIT](LICENSE)
