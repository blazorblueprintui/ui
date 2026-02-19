#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "What library do you want to build?"
echo ""
echo "0. All"
echo "---"
echo "1. Lucide"
echo "2. Heroicons"
echo "3. Feather"
echo ""
read -rp "Select an option: " choice

build_lucide() {
  echo ""
  echo "Building Lucide icons..."
  node "$SCRIPT_DIR/generate-lucide.js"
}

build_heroicons() {
  echo ""
  echo "Building Heroicons..."
  node "$SCRIPT_DIR/generate-heroicons.js"
}

build_feather() {
  echo ""
  echo "Building Feather icons..."
  node "$SCRIPT_DIR/generate-feather.js"
}

case "$choice" in
  0)
    build_lucide
    build_heroicons
    build_feather
    ;;
  1)
    build_lucide
    ;;
  2)
    build_heroicons
    ;;
  3)
    build_feather
    ;;
  *)
    echo "Invalid option: $choice"
    exit 1
    ;;
esac

echo ""
echo "Done!"
