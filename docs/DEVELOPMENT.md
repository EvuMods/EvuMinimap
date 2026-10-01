# Development

## Layout

- `src/EvuMinimap.Core` solves rectangles. It targets `netstandard2.0` and does not reference Unity.
- `src/EvuMinimap` is the BepInEx plugin (`net48`). It captures the vanilla small-map rect once, then writes the solved anchors, pivot, size, and `localScale` while the small map is on screen.
- Buff clearance uses the same core. The plugin converts the map and the status-effect list into one canvas space, asks `BuffPlacement` for a spot, and writes the delta back as `anchoredPosition`. When the map is hidden, the large map is open, or the world has no map, the buff list is put back.

The plugin does not Harmony-patch the HUD. `LateUpdate` is enough while the game writes those rects earlier in the frame. If a game update starts overwriting them afterwards, add a postfix on the method that writes them and keep the solver as it is.

## Prerequisites

- .NET SDK 8
- `make`, `curl`, `unzip`, and `python3`
- Either a local Valheim install (`VALHEIM_INSTALL` pointing at the game directory) or SteamCMD, which `scripts/fetch-refs.sh` downloads for you

The script also downloads the BepInEx pack version in `deps/bepinex-pack.version`. Assemblies land in `.refs/`, which is gitignored. `BepInEx.AssemblyPublicizer.MSBuild` publicizes `assembly_valheim.dll` during the plugin build so private game fields can be read.

Reference assemblies come from the Valheim dedicated server (Steam app 896660). That build exposes the same minimap and HUD types the client uses. `VALHEIM_INSTALL` copies from a local client instead.

## Commands

```sh
make fetch-refs
make build
make test
make verify
make package
```

`make verify` is the gate: fetch references if needed, build, and test. `make package` writes `dist/EvuMinimap-<version>.zip` with the plugin dll, README, license, and changelog.

## Version

`version.txt` is the version. `Directory.Build.props` reads it, and the plugin's `BepInPlugin` version is generated from that property. release-please updates `version.txt` and `CHANGELOG.md`.

## Game updates

`.valheim-buildid` is the dedicated-server build this tree was last verified against. The Valheim update workflow compares it with the public Steam build. A green `make verify` opens a pull request whose message is `fix: rebuild against Valheim <buildid>`. A failed build opens a GitHub issue and does not release.

A green compile means the referenced members and the layout tests still hold. It does not play the game. Check the minimap in a client after a game update that you care about.

For that chain to publish on its own, the release-please workflow dispatches itself after merging a release pull request that contains only those rebuild commits. GitHub does not start a new workflow from `GITHUB_TOKEN` alone, so the dispatch is explicit. See `.github/workflows/`.

## In-game check

There is no automated playtest here. After a HUD change, confirm:

- Scale 1, top-right, zero offset matches vanilla.
- Raising the scale grows away from the chosen anchor.
- Alt+Numpad plus and minus change one step, and either Alt key works.
- Buff icons drop below a larger top-right map, and stay in the vanilla corner if the map is moved to the other side of the screen.
- Reset returns the map and the buff list. Hotkeys stay bound.
- The large map still opens and closes.
