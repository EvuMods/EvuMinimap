# EvuMinimap

Client-side Valheim mod that resizes the minimap and moves the point it grows from. Buff icons slide out of the way when the map covers them, and stay where Valheim put them when it does not.

Only your game needs the mod. The large map is unchanged.

## Requirements

- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) 5.4.2351 or a compatible 5.4 pack
- Valheim, client only

A configuration manager is optional. The mod uses BepInEx's config file, so [Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager) and the usual forks can edit it in game. Ranges, the anchor dropdown, hotkeys, and the reset button are tagged for those editors.

## Install

1. Install BepInExPack Valheim.
2. Copy `EvuMinimap.dll` into `BepInEx/plugins`.
3. Start Valheim once. Settings are written to `BepInEx/config/evu.evuminimap.cfg`.

## Defaults

| Setting | Default |
| --- | --- |
| Scale | 1 (vanilla), from 0.5 to 5 |
| Anchor | Top-right. The map grows down and left. |
| Offset | 0, 0. The anchor sits where it does in vanilla. |
| Buff icons | Move only when the minimap overlaps them |
| Larger | Alt + Numpad + |
| Smaller | Alt + Numpad - |

Either Alt key works. One press changes the size by 0.25. Reset restores the map and the buff behavior. It does not clear the hotkeys.

Changing the anchor at 1x does not move the map. Later size changes grow away from the new anchor. An offset moves that anchor, in HUD units, from the place it has on the vanilla map.

## Build

See [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md). `make verify` builds the plugin and runs the layout tests.

## License

[MIT](LICENSE)
