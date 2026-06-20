#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SRC_DIR="${ROOT_DIR}/Builds/WebGL"
DIST_DIR="${ROOT_DIR}/webgl-dist"

if [[ ! -f "${SRC_DIR}/index.html" ]]; then
  echo "[Jarvis3D] Build output not found: ${SRC_DIR}/index.html"
  echo "[Jarvis3D] Build first from Unity menu: Jarvis3D/WebGL/Build (Development or Release)"
  exit 1
fi

rm -rf "${DIST_DIR}"
mkdir -p "${DIST_DIR}"
cp -R "${SRC_DIR}/." "${DIST_DIR}/"
touch "${DIST_DIR}/.nojekyll"

echo "[Jarvis3D] Exported prebuilt files to ${DIST_DIR}"
echo "[Jarvis3D] You can now run workflow: WebGL Deploy Prebuilt (No Unity Build)"

