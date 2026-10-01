# Development

## Layout

- `src/EvuMinimap.Core` solves rectangles. It targets `netstandard2.0` and does not reference Unity.
- `src/EvuMinimap` is the BepInEx plugin (`net48`). It captures the vanilla small-map rect once, then writes the solved anchors, pivot, size, and `localScale` while the small map is on screen.
- Buff clearance uses the same core. The plugin converts the map and the status-effect list into one canvas space, asks `BuffPlacement` for a spot, and writes the delta back as `anchoredPosition`. When the map is hidden, the large map is open, or the world has no map, the buff list is put back.
- Shape mask `None` does not add a clip. Oval and rectangle add a mask. The UI mask clones the terrain material, so each frame the live map properties are copied back onto that clone and the stencil values are kept. Aspect only changes the mask window. Icon alpha is a `CanvasGroup` on the small-map root, plus `_Color` on custom pin shaders. The terrain shader does not read UI opacity.

The plugin does not Harmony-patch the HUD. `LateUpdate` is enough while the game writes those rects earlier in the frame. If a game update starts overwriting them afterwards, add a postfix on the method that writes them and keep the solver as it is.

## Prerequisites

- .NET SDK 8. `make` uses `~/.dotnet` when that install exists, including when `/usr/bin/dotnet` is only a runtime.
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

`make verify` is the gate: fetch references if needed, build, and test. `make package` writes `dist/EvuMinimap-<version>.zip` in the Hexium layout: `manifest.json`, `icon.png`, `README.md`, `CHANGELOG.md`, `EvuMinimap.dll`, and `EvuMinimap.Core.dll` all at the zip root. `version_number` in the packaged manifest is taken from `version.txt`. BepInExPack is not a manifest dependency; Hexium assumes it and strips that entry on upload.

The release workflow attaches that zip and the two raw DLLs to the GitHub release. The Thunderstore workflow is manual. Run it from Actions, leave the tag empty to package the selected branch, or set a release tag such as `v0.1.0`. It publishes team `EvuMods` to the Valheim community with categories Mods, AI Generated, Client-side, Tweaks, and the update slug from the `game_category` input (`deep-north-update` today). NSFW is off. The service account token belongs in the `TCLI_AUTH_TOKEN` repository secret, not in the repo.

## Version

`version.txt` is the version. `Directory.Build.props` reads it, and the plugin's `BepInPlugin` version is generated from that property. release-please updates `version.txt` and `CHANGELOG.md`.

## Game updates

`.valheim-buildid` is the dedicated-server build this tree was last verified against. The Valheim update workflow compares it with the public Steam build. A green `make verify` opens a pull request whose message is `fix: rebuild against Valheim <buildid>`. A failed build opens a GitHub issue and does not release.

A green compile means the referenced members and the layout tests still hold. It does not play the game. Check the minimap in a client after a game update that you care about.

For that chain to publish on its own, the release-please workflow dispatches itself after merging a release pull request that contains only those rebuild commits. GitHub does not start a new workflow from `GITHUB_TOKEN` alone, so the dispatch is explicit. See `.github/workflows/`.

## In-game check

There is no automated playtest here. After a HUD change, confirm:

- Turning Enabled off restores the vanilla minimap, shape, icon alpha, and buff strip. Hotkeys do nothing until it is on again.
- Scale 1, top-right, zero offset matches vanilla.
- Raising the scale grows away from the chosen anchor.
- Alt+Numpad plus and minus change one step, and either Alt key works.
- Buff icons move to the left of a larger top-right map, and stay in the vanilla corner if the map is moved to the other side of the screen.
- Alt+Numpad multiply and divide cycle profiles 1 through 5, and the top-left toast says which one is active.
- Shape mask None matches the vanilla minimap. Oval and rectangle crop the terrain as well as the frame and pins. Lowering icon alpha fades the frame, pins, and markers. The terrain stays opaque.
- Reset returns the active profile. Hotkeys and the other profiles stay bound.
- The large map still opens and closes.
