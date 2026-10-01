#!/usr/bin/env bash
# Download BepInEx and Valheim dedicated-server assemblies into .refs/.
# Game binaries are not committed. VALHEIM_INSTALL overrides the Steam download.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
REFS="$ROOT/.refs"
CACHE="$REFS/cache"
BEPINEX_VERSION="$(tr -d '[:space:]' < "$ROOT/deps/bepinex-pack.version")"
APP_ID="896660"
mkdir -p "$REFS/BepInEx" "$REFS/Valheim" "$CACHE"

copy_if_present() {
  local src="$1" dest="$2"
  shift 2
  local name
  for name in "$@"; do
    if [[ -f "$src/$name" ]]; then
      cp -f "$src/$name" "$dest/$name"
    fi
  done
}

require_file() {
  if [[ ! -f "$1" ]]; then
    echo "fetch-refs: missing $1" >&2
    exit 1
  fi
}

fetch_bepinex() {
  if [[ -n "${BEPINEX_CORE:-}" && -f "${BEPINEX_CORE}/BepInEx.dll" ]]; then
    cp -f "${BEPINEX_CORE}/BepInEx.dll" "${BEPINEX_CORE}/0Harmony.dll" "$REFS/BepInEx/"
    echo "$BEPINEX_VERSION" > "$REFS/bepinex.version"
    return
  fi

  if [[ -f "$REFS/BepInEx/BepInEx.dll" && "$(cat "$REFS/bepinex.version" 2>/dev/null || true)" == "$BEPINEX_VERSION" ]]; then
    return
  fi

  local zip="$CACHE/BepInExPack_Valheim-${BEPINEX_VERSION}.zip"
  if [[ ! -f "$zip" ]]; then
    curl -fL --retry 3 -A "EvuMinimap" -o "$zip" \
      "https://thunderstore.io/package/download/denikson/BepInExPack_Valheim/${BEPINEX_VERSION}/"
  fi
  rm -rf "$CACHE/bepinex-pack"
  unzip -q "$zip" -d "$CACHE/bepinex-pack"
  local core
  core="$(find "$CACHE/bepinex-pack" -type d -name core | head -1)"
  require_file "$core/BepInEx.dll"
  require_file "$core/0Harmony.dll"
  cp -f "$core/BepInEx.dll" "$core/0Harmony.dll" "$REFS/BepInEx/"
  echo "$BEPINEX_VERSION" > "$REFS/bepinex.version"
}

python_bin() {
  if command -v python3 >/dev/null 2>&1; then
    echo python3
  else
    echo python
  fi
}

current_buildid() {
  curl -fsSL "https://api.steamcmd.net/v1/info/${APP_ID}" \
    | "$(python_bin)" -c "import json,sys; d=json.load(sys.stdin); print(d['data']['${APP_ID}']['depots']['branches']['public']['buildid'])"
}

find_managed() {
  local root="$1"
  local candidate
  for candidate in \
    "$root/valheim_Data/Managed" \
    "$root/Valheim_Data/Managed" \
    "$root/valheim_server_Data/Managed" \
    "$root/Valheim_server_Data/Managed"
  do
    if [[ -f "$candidate/assembly_valheim.dll" ]]; then
      echo "$candidate"
      return 0
    fi
  done
  local found
  found="$(find "$root" -type f -name assembly_valheim.dll 2>/dev/null | head -1 || true)"
  if [[ -n "$found" ]]; then
    dirname "$found"
    return 0
  fi
  return 1
}

copy_valheim() {
  local managed="$1"
  local buildid="$2"
  copy_if_present "$managed" "$REFS/Valheim" \
    assembly_valheim.dll \
    UnityEngine.dll \
    UnityEngine.CoreModule.dll \
    UnityEngine.UIModule.dll \
    UnityEngine.UI.dll \
    UnityEngine.IMGUIModule.dll \
    UnityEngine.InputLegacyModule.dll \
    UnityEngine.TextRenderingModule.dll \
    UnityEngine.TextCoreModule.dll \
    UnityEngine.PhysicsModule.dll \
    UnityEngine.Physics2DModule.dll
  require_file "$REFS/Valheim/assembly_valheim.dll"
  require_file "$REFS/Valheim/UnityEngine.CoreModule.dll"
  require_file "$REFS/Valheim/UnityEngine.IMGUIModule.dll"
  require_file "$REFS/Valheim/UnityEngine.InputLegacyModule.dll"
  echo "$buildid" > "$REFS/valheim.buildid"
}

fetch_valheim() {
  if [[ -n "${VALHEIM_INSTALL:-}" ]]; then
    local managed
    managed="$(find_managed "$VALHEIM_INSTALL")"
    local buildid="local"
    if [[ -f "$ROOT/.valheim-buildid" ]]; then
      buildid="$(tr -d '[:space:]' < "$ROOT/.valheim-buildid")"
    fi
    copy_valheim "$managed" "$buildid"
    return
  fi

  local buildid
  buildid="$(current_buildid)"
  if [[ -f "$REFS/Valheim/assembly_valheim.dll" && "$(cat "$REFS/valheim.buildid" 2>/dev/null || true)" == "$buildid" ]]; then
    return
  fi

  local install="$CACHE/valheim-server"
  mkdir -p "$install"
  local steamcmd="steamcmd"
  if ! command -v steamcmd >/dev/null 2>&1; then
    if [[ ! -x "$CACHE/steamcmd/steamcmd.sh" ]]; then
      mkdir -p "$CACHE/steamcmd"
      curl -fsSL "https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz" \
        | tar -xz -C "$CACHE/steamcmd"
    fi
    steamcmd="$CACHE/steamcmd/steamcmd.sh"
  fi

  set +e
  "$steamcmd" +force_install_dir "$install" +login anonymous +app_update "$APP_ID" validate +quit
  local status=$?
  set -e

  local managed
  if ! managed="$(find_managed "$install")"; then
    echo "fetch-refs: steamcmd exit $status and assembly_valheim.dll was not found under $install" >&2
    exit 1
  fi
  copy_valheim "$managed" "$buildid"
}

fetch_bepinex
fetch_valheim
echo "fetch-refs: BepInEx $BEPINEX_VERSION, Valheim build $(cat "$REFS/valheim.buildid")"
