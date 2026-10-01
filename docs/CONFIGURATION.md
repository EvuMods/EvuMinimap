# Configuration

The file is `BepInEx/config/evu.evuminimap.cfg`. Changes apply on the next frame. Editors that understand BepInEx ranges, enum lists, and keyboard shortcuts can show the same fields. The attribute class in the plugin is the ConfigurationManager template, matched by type name, so the official manager and the usual forks can show order, the advanced flag, and the reset button without a hard dependency.

The plugin soft-depends on `com.bepis.bepinex.configurationmanager` so that manager loads first when it is installed. The mod still runs without it.

## General

| Key | Default | Meaning |
| --- | --- | --- |
| Enabled | true | When off, the small minimap and buff strip stay vanilla. Saved profiles and hotkeys remain in the file and apply again when this is on. |

## Minimap

| Key | Default | Range | Meaning |
| --- | --- | --- | --- |
| Scale | 1 | 0.5 to 5 | Size relative to vanilla. Applied as `localScale` on the small-map root, so the circle, pins, wind marker, and biome name scale together. |
| Anchor | TopRight | nine-point list | Which point of the minimap stays put as the size changes. |
| OffsetX | 0 | -4000 to 4000 | Horizontal shift of that anchor, in HUD units, from its vanilla position. |
| OffsetY | 0 | -4000 to 4000 | Vertical shift of that anchor, in HUD units, from its vanilla position. |
| RepositionBuffIcons | true | bool | When the minimap overlaps the buff strip, slide the strip to the left of the map when that fits. Otherwise use the side with the most free space. When there is no overlap, the strip stays at the vanilla position. |
| ShapeMask | None | None, Oval, Rectangle | Extra clip. None keeps Valheim's shape, including changes from other mods. Shown as "Shape mask". |
| IconAlpha | 1 | 0 to 1 | Opacity of the frame, pins, and markers. Shown as "Icon alpha". Does not fade the terrain. |
| Aspect | 1 | 0.5 to 2 | Width/height of the shape mask. The map and icons stay 1:1. Above 1 crops the top and bottom. Below 1 crops the sides. Locked while shape mask is None. |
| CornerRadius | 0 | 0 to 1 | Corner roundness of the rectangle mask. 0 is sharp. 1 is a capsule. Locked for None and oval. |
| ScaleStep | 0.25 | 0.05 to 1 | Hotkey step for the active profile. Marked advanced in Configuration Manager. |

Anchor names: `TopLeft`, `Top`, `TopRight`, `Left`, `Center`, `Right`, `BottomLeft`, `Bottom`, `BottomRight`. Configuration Manager shows them as a dropdown from the enum. BepInEx's `AcceptableValueList` cannot wrap this enum on the .NET Framework build, so the list is the enum itself.

At scale 1 and offset 0, every anchor describes the same vanilla rectangle. After you scale up, the chosen point stays on its vanilla spot (plus the offset) and the rest of the map grows away from it.

## Profiles

Five profiles. Profile 1 is the `Minimap` section above, so an older config file still loads as profile 1. Profiles 2 through 5 are sections `Minimap 2` through `Minimap 5`, with the same keys, each starting at the vanilla map.

| Section | Key | Default | Meaning |
| --- | --- | --- | --- |
| Profiles | Active | 1 | Which profile is on screen. 1 to 5. |

Size hotkeys and reset change the active profile only. Changing the active profile writes `Switched to Minimap Profile N` to the BepInEx log and shows that text as a fading top-left Valheim message.

## Hotkeys

| Key | Default |
| --- | --- |
| IncreaseSize | LeftAlt + KeypadPlus |
| DecreaseSize | LeftAlt + KeypadMinus |
| NextProfile | LeftAlt + KeypadMultiply |
| PreviousProfile | LeftAlt + KeypadDivide |

The binding is stored with Left Alt. The press check treats Right Alt as the same modifier, so either Alt key works. One press moves one step. Holding the key does not repeat. Extra modifiers such as Ctrl suppress the hotkey.

## Actions

| Key | Meaning |
| --- | --- |
| ResetToVanilla | Restores the active profile: scale, anchor, both offsets, buff reposition, shape mask, and icon alpha. Does not change hotkeys, the scale step, or the other profiles. |

Configuration Manager draws this as a **Reset to vanilla** button. A manager that ignores custom drawers can set `ResetToVanilla = true`. The plugin resets and writes the flag back to false.

## Buff layout mods

If `randyknapp.mods.minimalstatuseffects` or `redseiko.valheim.statusquo` is loaded, buff reposition is skipped and a line is written to the BepInEx log. Those mods own the status-effect list.
