#!/bin/bash

# Release script for BlazorUI.Icons
# Usage: ./scripts/release-icons.sh <version>
# Example: ./scripts/release-icons.sh 1.0.0-beta.4

set -e  # Exit on error

PACKAGE_NAME="icons"
PROJECT_PATH="src/BlazorUI.Icons"
COLOR_GREEN='\033[0;32m'
COLOR_RED='\033[0;31m'
COLOR_YELLOW='\033[1;33m'
COLOR_RESET='\033[0m'

# Check if version argument is provided
if [ -z "$1" ]; then
    echo -e "${COLOR_RED}Error: Version argument required${COLOR_RESET}"
    echo "Usage: ./scripts/release-icons.sh <version>"
    echo "Example: ./scripts/release-icons.sh 1.0.0-beta.4"
    exit 1
fi

VERSION=$1
TAG_NAME="${PACKAGE_NAME}/v${VERSION}"

# Validate semantic version format (basic check)
if ! [[ $VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.-]+)?$ ]]; then
    echo -e "${COLOR_RED}Error: Invalid version format${COLOR_RESET}"
    echo "Version must follow semantic versioning (e.g., 1.0.0 or 1.0.0-beta.4)"
    exit 1
fi

# Check if we're in the right directory
if [ ! -d "$PROJECT_PATH" ]; then
    echo -e "${COLOR_RED}Error: Project directory not found: $PROJECT_PATH${COLOR_RESET}"
    echo "Make sure you're running this script from the repository root"
    exit 1
fi

# Check for uncommitted changes
if ! git diff-index --quiet HEAD --; then
    echo -e "${COLOR_RED}Error: You have uncommitted changes${COLOR_RESET}"
    echo "Please commit or stash your changes before creating a release"
    git status --short
    exit 1
fi

# Check if tag already exists
if git rev-parse "$TAG_NAME" >/dev/null 2>&1; then
    echo -e "${COLOR_RED}Error: Tag $TAG_NAME already exists${COLOR_RESET}"
    echo "Use a different version number or delete the existing tag first"
    exit 1
fi

# Show what we're about to do
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo -e "${COLOR_YELLOW}Release Summary${COLOR_RESET}"
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo -e "Package:  ${COLOR_GREEN}BlazorUI.Icons${COLOR_RESET}"
echo -e "Version:  ${COLOR_GREEN}${VERSION}${COLOR_RESET}"
echo -e "Tag:      ${COLOR_GREEN}${TAG_NAME}${COLOR_RESET}"
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo ""

# Confirm with user
read -p "Proceed with release? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${COLOR_YELLOW}Release cancelled${COLOR_RESET}"
    exit 0
fi

# Create and push tag
echo ""
echo -e "${COLOR_GREEN}Creating tag: $TAG_NAME${COLOR_RESET}"
git tag -a "$TAG_NAME" -m "Release BlazorUI.Icons v${VERSION}"

echo -e "${COLOR_GREEN}Pushing tag to GitHub...${COLOR_RESET}"
git push origin "$TAG_NAME"

echo ""
echo -e "${COLOR_GREEN}═══════════════════════════════════════════════════${COLOR_RESET}"
echo -e "${COLOR_GREEN}✓ Release initiated successfully!${COLOR_RESET}"
echo -e "${COLOR_GREEN}═══════════════════════════════════════════════════${COLOR_RESET}"
echo ""
echo "GitHub Actions will now:"
echo "  1. Build the project"
echo "  2. Pack the NuGet package"
echo "  3. Publish to NuGet.org"
echo ""
echo "Monitor the workflow at:"
echo "  https://github.com/blazorui-net/ui/actions"
echo ""
