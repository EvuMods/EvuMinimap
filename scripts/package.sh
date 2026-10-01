#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VERSION="$(tr -d '[:space:]' < "$ROOT/version.txt")"

python_bin() {
  if command -v python3 >/dev/null 2>&1; then
    echo python3
  else
    echo python
  fi
}
DLL="$ROOT/src/EvuMinimap/bin/Release/EvuMinimap.dll"
STAGE="$ROOT/dist/EvuMinimap"
ZIP="$ROOT/dist/EvuMinimap-${VERSION}.zip"

if [[ ! -f "$DLL" ]]; then
  echo "package: missing $DLL. Run make build first." >&2
  exit 1
fi

rm -rf "$STAGE" "$ZIP"
mkdir -p "$STAGE"
cp "$DLL" "$ROOT/README.md" "$ROOT/LICENSE" "$ROOT/CHANGELOG.md" "$STAGE/"
"$(python_bin)" - "$ROOT/dist" "EvuMinimap-${VERSION}.zip" "EvuMinimap" <<'PY'
import sys
import zipfile
from pathlib import Path

dist = Path(sys.argv[1])
archive = dist / sys.argv[2]
folder = dist / sys.argv[3]
with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED) as zf:
    for path in sorted(folder.rglob("*")):
        if path.is_file():
            zf.write(path, path.relative_to(dist).as_posix())
PY
echo "package: $ZIP"
