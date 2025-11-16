#!/bin/bash

# Release script for BlazorUI.Components
# Usage: ./scripts/release-components.sh <version>
# Example: ./scripts/release-components.sh 1.0.0-beta.4

set -e  # Exit on error

PACKAGE_NAME="components"
PROJECT_PATH="src/BlazorUI.Components"
COLOR_GREEN='\033[0;32m'
COLOR_RED='\033[0;31m'
COLOR_YELLOW='\033[1;33m'
COLOR_RESET='\033[0m'

# Check if version argument is provided
if [ -z "$1" ]; then
    echo -e "${COLOR_RED}Error: Version argument required${COLOR_RESET}"
    echo "Usage: ./scripts/release-components.sh <version>"
    echo "Example: ./scripts/release-components.sh 1.0.0-beta.4"
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

# Rebuild Tailwind CSS to ensure it's up-to-date
echo ""
echo -e "${COLOR_YELLOW}Building Tailwind CSS...${COLOR_RESET}"
cd "$PROJECT_PATH"

# Detect OS and use appropriate Tailwind CLI
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    # Windows (Git Bash/MSYS)
    ../../tools/tailwindcss.exe -i wwwroot/css/blazorui-input.css -o wwwroot/blazorui.css --minify
else
    # Linux/macOS
    npx --yes @tailwindcss/cli@4.1.16 -i wwwroot/css/blazorui-input.css -o wwwroot/blazorui.css --minify
fi

cd - > /dev/null

# Check if CSS was updated (indicates it was out-of-date)
if ! git diff --quiet -- "$PROJECT_PATH/wwwroot/blazorui.css"; then
    echo ""
    echo -e "${COLOR_RED}═══════════════════════════════════════════════════${COLOR_RESET}"
    echo -e "${COLOR_RED}Error: blazorui.css was out-of-date!${COLOR_RESET}"
    echo -e "${COLOR_RED}═══════════════════════════════════════════════════${COLOR_RESET}"
    echo ""
    echo "The CSS has been rebuilt and differs from what's committed."
    echo "Please review the changes, commit them, and run the release script again."
    echo ""
    echo -e "${COLOR_YELLOW}Changes detected:${COLOR_RESET}"
    git diff "$PROJECT_PATH/wwwroot/blazorui.css" | head -30
    echo ""
    echo "To commit the updated CSS:"
    echo "  git add $PROJECT_PATH/wwwroot/blazorui.css"
    echo "  git commit -m 'chore: rebuild blazorui.css for v${VERSION}'"
    echo ""
    exit 1
fi

echo -e "${COLOR_GREEN}✓ CSS is up-to-date${COLOR_RESET}"
echo ""

# Show what we're about to do
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo -e "${COLOR_YELLOW}Release Summary${COLOR_RESET}"
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo -e "Package:  ${COLOR_GREEN}BlazorUI.Components${COLOR_RESET}"
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
git tag -a "$TAG_NAME" -m "Release BlazorUI.Components v${VERSION}"

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
