#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_DIR="${ROOT_DIR}/Builds/WebGL"
PORT="${1:-8080}"

if [[ ! -f "${BUILD_DIR}/index.html" ]]; then
  echo "[Jarvis3D] WebGL build output not found: ${BUILD_DIR}/index.html"
  echo "[Jarvis3D] Build first from Unity menu: Jarvis3D/WebGL/Build (Development or Release)"
  exit 1
fi

echo "[Jarvis3D] Serving WebGL build on http://localhost:${PORT}"
echo "[Jarvis3D] Press Ctrl+C to stop."
if command -v python3 >/dev/null 2>&1; then
  python3 -m http.server "${PORT}" --directory "${BUILD_DIR}"
else
  python -m http.server "${PORT}" --directory "${BUILD_DIR}"
fi

