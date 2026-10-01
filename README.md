# EvuMinimap

Client-side Valheim mod that resizes the minimap and moves the point it grows from. Buff icons slide out of the way when the map covers them, and stay where Valheim put them when it does not.

## Features

- A global toggle, on by default. Off leaves the vanilla minimap, buff strip, and ship wind panel alone. Saved profiles stay in the config.
- Scale from 0.5x to 5x vanilla. The default anchor is the top-right corner, so the map grows down and left.
- Move that anchor from its vanilla position. At 1x and zero offset, every anchor still matches the vanilla map.
- Alt+Numpad plus and minus change the size by one step. Either Alt key works.
- Five saved profiles. Alt+Numpad multiply and divide cycle them, and Valheim shows a short toast.
- Optional shape mask: none, oval, or rectangle. Aspect crops that mask. The map and icons stay 1:1. Icon alpha fades the frame, pins, and markers.
- Buff icons move to the left of the minimap when it overlaps them, and stay put when it does not.
- The boat wind panel moves below the minimap when it overlaps, and stays put when it does not.
- Reset restores the active profile. Hotkeys and the other profiles stay as they are.

## Installation

Install [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/), then install [EvuMinimap from Thunderstore](https://thunderstore.io/c/valheim/p/EvuMods/EvuMinimap/), or copy `EvuMinimap.dll` and `EvuMinimap.Core.dll` into `BepInEx/plugins`. Both files have to sit in the same folder.

Start Valheim once. Settings are written to `BepInEx/config/evu.evuminimap.cfg`.

## Configuration

A configuration manager is optional. The mod uses BepInEx's config file, so Configuration Manager and the usual forks can edit it in game.

| Setting | Default |
| --- | --- |
| Enabled | On |
| Scale | 1 (vanilla), from 0.5 to 5 |
| Anchor | Top-right |
| Offset | 0, 0 |
| Shape mask | None |
| Icon alpha | 1 |
| Buff icons | Move only when the minimap overlaps them |
| Ship wind | Move below the map only when it overlaps |
| Larger | Alt + Numpad + |
| Smaller | Alt + Numpad - |
| Next profile | Alt + Numpad * |
| Previous profile | Alt + Numpad / |

One press changes the size by 0.25. Changing the anchor at 1x does not move the map. Later size changes grow away from the new anchor. An offset moves that anchor, in HUD units, from the place it has on the vanilla map.

The full list is in [docs/CONFIGURATION.md](docs/CONFIGURATION.md).

## Compatibility

Client only. The dedicated server loads the plugin and does nothing. The large map is unchanged.

If Minimal Status Effects or StatusQuo is installed, buff icons are left alone.

## Build

See [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md). `make verify` builds the plugin and runs the layout tests.

## License

[MIT](LICENSE)
