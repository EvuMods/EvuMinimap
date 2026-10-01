# Configuration

The file is `BepInEx/config/evu.evuminimap.cfg`. Changes apply on the next frame. Editors that understand BepInEx ranges, enum lists, and keyboard shortcuts can show the same fields. The attribute class in the plugin is the ConfigurationManager template, matched by type name, so the official manager and the usual forks can show order, the advanced flag, and the reset button without a hard dependency.

The plugin soft-depends on `com.bepis.bepinex.configurationmanager` so that manager loads first when it is installed. The mod still runs without it.

## Minimap

| Key | Default | Range | Meaning |
| --- | --- | --- | --- |
| Scale | 1 | 0.5 to 5 | Size relative to vanilla. Applied as `localScale` on the small-map root, so the circle, pins, wind marker, and biome name scale together. |
| Anchor | TopRight | nine-point list | Which point of the minimap stays put as the size changes. |
| OffsetX | 0 | -4000 to 4000 | Horizontal shift of that anchor, in HUD units, from its vanilla position. |
| OffsetY | 0 | -4000 to 4000 | Vertical shift of that anchor, in HUD units, from its vanilla position. |
| RepositionBuffIcons | true | bool | When the minimap overlaps the buff strip, slide the strip to the side with the most free space. Below the map wins when it fits. When there is no overlap, the strip stays at the vanilla position. |
| ScaleStep | 0.25 | 0.05 to 1 | Hotkey step. Marked advanced in Configuration Manager. |

Anchor names: `TopLeft`, `Top`, `TopRight`, `Left`, `Center`, `Right`, `BottomLeft`, `Bottom`, `BottomRight`. Configuration Manager shows them as a dropdown from the enum. BepInEx's `AcceptableValueList` cannot wrap this enum on the .NET Framework build, so the list is the enum itself.

At scale 1 and offset 0, every anchor describes the same vanilla rectangle. After you scale up, the chosen point stays on its vanilla spot (plus the offset) and the rest of the map grows away from it.

## Hotkeys

| Key | Default |
| --- | --- |
| IncreaseSize | LeftAlt + KeypadPlus |
| DecreaseSize | LeftAlt + KeypadMinus |

The binding is stored with Left Alt. The press check treats Right Alt as the same modifier, so either Alt key works. One press moves one step. Holding the key does not repeat. Extra modifiers such as Ctrl suppress the hotkey.

## Actions

| Key | Meaning |
| --- | --- |
| ResetToVanilla | Restores scale, anchor, both offsets, and buff reposition. Does not change hotkeys or the scale step. |

Configuration Manager draws this as a **Reset to vanilla** button. A manager that ignores custom drawers can set `ResetToVanilla = true`. The plugin resets and writes the flag back to false.

## Buff layout mods

If `randyknapp.mods.minimalstatuseffects` or `redseiko.valheim.statusquo` is loaded, buff reposition is skipped and a line is written to the BepInEx log. Those mods own the status-effect list.
