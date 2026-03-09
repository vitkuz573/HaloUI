#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="${1:-$ROOT_DIR/HaloUI.IconPacks.Material/Iconography/Packs/Material}"
C_SHARP_OUTPUT="$ROOT_DIR/HaloUI.IconPacks.Material/Iconography/Packs/Material/HaloMaterialIcons.g.cs"

dotnet run --project "$ROOT_DIR/tools/HaloUI.IconCatalog.Tool/HaloUI.IconCatalog.Tool.csproj" -- \
  material-icons \
  --output "$OUTPUT_DIR" \
  --csharp-output "$C_SHARP_OUTPUT"
