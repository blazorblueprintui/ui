#!/bin/bash

# Release script for BlazorUI.Components
# Usage: ./scripts/release-components.sh <version>
# Example: ./scripts/release-components.sh 1.7.0

set -e  # Exit on error

PACKAGE_NAME="components"
PROJECT_PATH="src/BlazorUI.Components"
COLOR_GREEN='\033[0;32m'
COLOR_RED='\033[0;31m'
COLOR_YELLOW='\033[1;33m'
COLOR_CYAN='\033[0;36m'
COLOR_RESET='\033[0m'
CSPROJ="$PROJECT_PATH/BlazorUI.Components.csproj"

COMMITS_MADE=0

# Get latest Primitives version from NuGet
get_latest_primitives_version() {
  local URL="https://api.nuget.org/v3-flatcontainer/blazorui.primitives/index.json"
  curl -s "$URL" | grep -o '"[0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*"' | tail -1 | tr -d '"'
}

# Get current Primitives version from Components.csproj
get_current_primitives_version() {
  grep 'Include="BlazorUI.Primitives"' "$CSPROJ" | grep -o 'Version="[^"]*"' | cut -d'"' -f2
}

# Update Primitives version in Components.csproj
update_primitives_version() {
  local NEW_VERSION=$1
  sed -i "s/Include=\"BlazorUI.Primitives\" Version=\"[^\"]*\"/Include=\"BlazorUI.Primitives\" Version=\"$NEW_VERSION\"/" "$CSPROJ"
}

# Check if version argument is provided
if [ -z "$1" ]; then
    echo -e "${COLOR_RED}Error: Version argument required${COLOR_RESET}"
    echo "Usage: ./scripts/release-components.sh <version>"
    echo "Example: ./scripts/release-components.sh 1.7.0"
    exit 1
fi

VERSION=$1
TAG_NAME="${PACKAGE_NAME}/v${VERSION}"
RELEASE_BRANCH="${PACKAGE_NAME}/release/v${VERSION}"

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

# Check for uncommitted changes (excluding blazorui.css which we handle automatically)
CSS_FILE="$PROJECT_PATH/wwwroot/blazorui.css"
OTHER_CHANGES=$(git diff-index --name-only HEAD -- | grep -v "^$CSS_FILE$" || true)

if [ -n "$OTHER_CHANGES" ]; then
    echo -e "${COLOR_RED}Error: You have uncommitted changes${COLOR_RESET}"
    echo "Please commit or stash your changes before creating a release"
    echo ""
    echo "Uncommitted files (excluding blazorui.css which is handled automatically):"
    echo "$OTHER_CHANGES" | sed 's/^/  /'
    exit 1
fi

# If CSS has uncommitted changes, note it (will be handled during release)
if ! git diff --quiet -- "$CSS_FILE" 2>/dev/null; then
    echo -e "${COLOR_YELLOW}Note: blazorui.css has uncommitted changes - will be rebuilt and committed during release${COLOR_RESET}"
fi

# Check if tag already exists
if git rev-parse "$TAG_NAME" >/dev/null 2>&1; then
    echo -e "${COLOR_RED}Error: Tag $TAG_NAME already exists${COLOR_RESET}"
    echo "Use a different version number or delete the existing tag first"
    exit 1
fi

# Check if release branch already exists
if git rev-parse "$RELEASE_BRANCH" >/dev/null 2>&1; then
    echo -e "${COLOR_RED}Error: Release branch $RELEASE_BRANCH already exists${COLOR_RESET}"
    echo "Delete the existing branch first: git branch -D $RELEASE_BRANCH"
    exit 1
fi

# Check Primitives version
echo ""
echo "Checking BlazorUI.Primitives version..."
LATEST_PRIMITIVES=$(get_latest_primitives_version)
CURRENT_PRIMITIVES=$(get_current_primitives_version)

echo ""
echo -e "${COLOR_YELLOW}BlazorUI.Primitives Dependency${COLOR_RESET}"
echo "   Current (in csproj): $CURRENT_PRIMITIVES"
echo "   Latest (on NuGet):   $LATEST_PRIMITIVES"
echo ""

WILL_UPDATE_PRIMITIVES=false
if [ "$LATEST_PRIMITIVES" != "$CURRENT_PRIMITIVES" ]; then
  echo -e "${COLOR_YELLOW}⚠️  Newer version available on NuGet${COLOR_RESET}"
  read -p "Update to $LATEST_PRIMITIVES? (y/N) " update_confirm
  if [[ "$update_confirm" =~ ^[Yy]$ ]]; then
    WILL_UPDATE_PRIMITIVES=true
  fi
fi

# Show what we're about to do
echo ""
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo -e "${COLOR_YELLOW}Release Summary${COLOR_RESET}"
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo -e "Package:     ${COLOR_GREEN}BlazorUI.Components${COLOR_RESET}"
echo -e "Version:     ${COLOR_GREEN}${VERSION}${COLOR_RESET}"
echo -e "Branch:      ${COLOR_CYAN}${RELEASE_BRANCH}${COLOR_RESET}"
echo -e "Tag:         ${COLOR_GREEN}${TAG_NAME}${COLOR_RESET}"
echo -e "Primitives:  ${COLOR_GREEN}${CURRENT_PRIMITIVES}${COLOR_RESET}"
if [ "$WILL_UPDATE_PRIMITIVES" = true ]; then
  echo -e "             ${COLOR_CYAN}→ will update to ${LATEST_PRIMITIVES}${COLOR_RESET}"
fi
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo ""

# Confirm with user
read -p "Proceed with release? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${COLOR_YELLOW}Release cancelled${COLOR_RESET}"
    exit 0
fi

# Create release branch
echo ""
echo -e "${COLOR_CYAN}Creating release branch: $RELEASE_BRANCH${COLOR_RESET}"
git checkout -b "$RELEASE_BRANCH"

# Update Primitives version if requested
if [ "$WILL_UPDATE_PRIMITIVES" = true ]; then
    echo ""
    echo -e "${COLOR_CYAN}Updating BlazorUI.Primitives to $LATEST_PRIMITIVES...${COLOR_RESET}"
    update_primitives_version "$LATEST_PRIMITIVES"
    git add "$CSPROJ"
    git commit -m "chore: bump BlazorUI.Primitives to $LATEST_PRIMITIVES"
    COMMITS_MADE=$((COMMITS_MADE + 1))
    echo -e "${COLOR_GREEN}✓ Updated and committed Primitives version${COLOR_RESET}"
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

# Check if CSS differs from HEAD and commit if needed
if ! git diff HEAD --quiet -- "$PROJECT_PATH/wwwroot/blazorui.css"; then
    echo ""
    echo -e "${COLOR_YELLOW}CSS was out-of-date, committing updated version...${COLOR_RESET}"
    git add "$PROJECT_PATH/wwwroot/blazorui.css"
    git commit -m "chore: rebuild blazorui.css for v${VERSION}"
    COMMITS_MADE=$((COMMITS_MADE + 1))
    echo -e "${COLOR_GREEN}✓ CSS rebuilt and committed${COLOR_RESET}"
else
    echo -e "${COLOR_GREEN}✓ CSS is up-to-date${COLOR_RESET}"
fi

# Create and push tag
echo ""
echo -e "${COLOR_GREEN}Creating tag: $TAG_NAME${COLOR_RESET}"
git tag -a "$TAG_NAME" -m "Release BlazorUI.Components v${VERSION}"

echo -e "${COLOR_GREEN}Pushing release branch and tag to GitHub...${COLOR_RESET}"
git push origin "$RELEASE_BRANCH"
git push origin "$TAG_NAME"

# Create PR if commits were made
if [ $COMMITS_MADE -gt 0 ]; then
    echo ""
    echo -e "${COLOR_CYAN}Creating PR to merge release changes back to main...${COLOR_RESET}"
    PR_URL=$(gh pr create \
        --base main \
        --head "$RELEASE_BRANCH" \
        --title "chore: merge release changes from $TAG_NAME" \
        --body "$(cat <<EOF
## Release Changes

This PR merges changes made during the release of **BlazorUI.Components v${VERSION}** back to main.

**Commits made during release:** $COMMITS_MADE

### Changes included:
$(git log main..$RELEASE_BRANCH --oneline | sed 's/^/- /')

---
*Auto-generated by release script*
EOF
)")
    echo -e "${COLOR_GREEN}PR created: $PR_URL${COLOR_RESET}"
else
    echo ""
    echo -e "${COLOR_CYAN}No additional commits made during release - no PR needed${COLOR_RESET}"
    # Clean up release branch since no changes
    git checkout main
    git branch -D "$RELEASE_BRANCH"
    git push origin --delete "$RELEASE_BRANCH" 2>/dev/null || true
fi

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
if [ $COMMITS_MADE -gt 0 ]; then
    echo -e "${COLOR_YELLOW}Remember to merge the PR to bring release changes back to main!${COLOR_RESET}"
    echo ""
fi
