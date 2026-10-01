# Roadmap

Shipped behavior is in [README.md](README.md). This list is the work that the current split is meant to absorb without a rewrite.

## Profiles

Save several `MinimapProfile` values, numbered 1 through 5. A hotkey switches the active profile. The solver and the applier already take one profile. The config layer grows extra sections and an active index. Existing keys stay the first profile so old config files still load.

## Shape

Square, circle, rectangle, and an optional corner radius. The vanilla minimap is circular. A later change replaces the mask on the small-map root from the applier. Layout math stays in HUD rectangles.

## Alpha

Fade the small map with a canvas group on its root. Pins and the frame fade together until a more specific split is worth doing.

## Buff icons

Version 1 moves the vanilla strip only when the minimap overlaps it, preferring a slot below the map. A later mode can pin the strip to the map even when the map has moved across the screen.

Other mods that already own that strip (Minimal Status Effects, StatusQuo) are left alone.
