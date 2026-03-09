#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="${1:-$ROOT_DIR/HaloUI/Iconography/Packs/Material}"

dotnet run --project "$ROOT_DIR/tools/HaloUI.IconCatalog.Tool/HaloUI.IconCatalog.Tool.csproj" -- \
  material-icons \
  --output "$OUTPUT_DIR"
