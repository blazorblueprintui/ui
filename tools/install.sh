#!/bin/bash
# Tailwind CLI Downloader for Linux/macOS (Multi-Arch)

# Pinned for reproducible builds; override with TAILWIND_VERSION=vX.Y.Z
VERSION="${TAILWIND_VERSION:-v4.3.5}"

ARCH=$(uname -m)
OS=$(uname -s)

if [ "$OS" == "Linux" ]; then
    TARGET="tailwindcss-linux"
    if [ "$ARCH" == "x86_64" ]; then
        PLATFORM="linux-x64"
    elif [[ "$ARCH" == "aarch64" || "$ARCH" == "arm64" ]]; then
        PLATFORM="linux-arm64"
    else
        echo "Unsupported Linux Architecture: $ARCH"
        exit 1
    fi
elif [ "$OS" == "Darwin" ]; then
    TARGET="tailwindcss-macos"
    if [ "$ARCH" == "arm64" ]; then
        PLATFORM="macos-arm64"
    else
        PLATFORM="macos-x64"
    fi
else
    echo "Unsupported OS: $OS"
    exit 1
fi

URL="https://github.com/tailwindlabs/tailwindcss/releases/download/$VERSION/tailwindcss-$PLATFORM"

echo "Downloading $URL..."
curl -L "$URL" -o "./$TARGET"
chmod +x "./$TARGET"
echo "Done! Binary saved as tools/$TARGET"
