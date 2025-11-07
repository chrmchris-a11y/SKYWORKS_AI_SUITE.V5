#!/bin/bash

# ================================================================
# SKYWORKS UI Packaging Script (bash)
# Creates dist/skyworks_static_ui.zip with all 12 pages + assets
# ================================================================

set -euo pipefail

echo ""
echo "================================================================"
echo "  SKYWORKS UI Packaging (bash)"
echo "================================================================"
echo ""

# Paths
SRC="WebPlatform/wwwroot/app/Pages/ui"
DST="dist/skyworks_static_ui.zip"

# Verify source exists
if [ ! -d "$SRC" ]; then
    echo "❌ ERROR: Source directory not found: $SRC"
    exit 1
fi

# Create dist directory
mkdir -p dist
echo "✅ Ensured dist/ directory exists"

# Remove existing ZIP
if [ -f "$DST" ]; then
    rm -f "$DST"
    echo "✅ Removed existing ZIP"
fi

# Count files
FILE_COUNT=$(find "$SRC" -type f | wc -l)
echo "📦 Packing $FILE_COUNT files from $SRC..."

# Create ZIP
(cd "$SRC" && zip -r "../../../../../dist/skyworks_static_ui.zip" . -q)
echo "✅ ZIP created: $DST"

# Verify ZIP contents
ZIP_SIZE=$(du -h "$DST" | cut -f1)
echo "📊 ZIP size: $ZIP_SIZE"

# List ZIP contents
echo ""
echo "📋 ZIP Contents (first 20 files):"
unzip -l "$DST" | head -n 25 | tail -n 20

echo ""
echo "================================================================"
echo "  ✅ SUCCESS! Package ready for distribution"
echo "================================================================"
echo ""
echo "📦 Package: $DST"
echo "📊 Files: $FILE_COUNT"
echo "💾 Size: $ZIP_SIZE"
echo ""
