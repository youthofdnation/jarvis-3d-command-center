#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DIST_DIR="${ROOT_DIR}/webgl-dist"
OUT_DIR="${ROOT_DIR}/dist"
OUT_FILE="${OUT_DIR}/jarvis3d-webgl-dist.tar.gz"

if [[ ! -f "${DIST_DIR}/index.html" ]]; then
  echo "[Jarvis3D] Prebuilt dist not found: ${DIST_DIR}/index.html"
  echo "[Jarvis3D] Generate it with: bash ./export_webgl_dist.sh"
  exit 1
fi

mkdir -p "${OUT_DIR}"
tar -czf "${OUT_FILE}" -C "${DIST_DIR}" .

echo "[Jarvis3D] Packaged prebuilt dist: ${OUT_FILE}"
echo "[Jarvis3D] Upload this archive to any static server and extract under web root."

