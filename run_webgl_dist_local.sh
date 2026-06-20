#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DIST_DIR="${ROOT_DIR}/webgl-dist"
PORT="${1:-8080}"

if [[ ! -f "${DIST_DIR}/index.html" ]]; then
  echo "[Jarvis3D] Prebuilt dist not found: ${DIST_DIR}/index.html"
  echo "[Jarvis3D] Generate it with: bash ./export_webgl_dist.sh"
  exit 1
fi

echo "[Jarvis3D] Serving prebuilt dist on http://localhost:${PORT}"
echo "[Jarvis3D] Press Ctrl+C to stop."
if command -v python3 >/dev/null 2>&1; then
  python3 -m http.server "${PORT}" --directory "${DIST_DIR}"
else
  python -m http.server "${PORT}" --directory "${DIST_DIR}"
fi

