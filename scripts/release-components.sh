#!/bin/bash

# Release script for BlazorBlueprint.Components
# Usage: ./scripts/release-components.sh [version]
# If no version is provided, the script will prompt you to pick a bump type.

set -e  # Exit on error

PACKAGE_NAME="components"
PROJECT_PATH="src/BlazorBlueprint.Components"
COLOR_GREEN='\033[0;32m'
COLOR_RED='\033[0;31m'
COLOR_YELLOW='\033[1;33m'
COLOR_CYAN='\033[0;36m'
COLOR_RESET='\033[0m'
CSPROJ="$PROJECT_PATH/BlazorBlueprint.Components.csproj"

COMMITS_MADE=0

# Get latest Primitives version from NuGet
get_latest_primitives_version() {
  local URL="https://api.nuget.org/v3-flatcontainer/BlazorBlueprint.Primitives/index.json"
  curl -s "$URL" | grep -o '"[0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*"' | tail -1 | tr -d '"'
}

# Get current Primitives version from Components.csproj
get_current_primitives_version() {
  grep 'Include="BlazorBlueprint.Primitives"' "$CSPROJ" | grep -o 'Version="[^"]*"' | cut -d'"' -f2
}

# Update Primitives version in Components.csproj
update_primitives_version() {
  local NEW_VERSION=$1
  sed -i "s/Include=\"BlazorBlueprint.Primitives\" Version=\"[^\"]*\"/Include=\"BlazorBlueprint.Primitives\" Version=\"$NEW_VERSION\"/" "$CSPROJ"
}

# Get current version from the latest components git tag
get_current_version() {
  git tag --sort=-v:refname | grep "^${PACKAGE_NAME}/v" | head -1 | sed "s|^${PACKAGE_NAME}/v||"
}

# Bump a semantic version component
# Usage: bump_version <current_version> <major|minor|patch>
bump_version() {
  local version=$1
  local bump_type=$2
  local major minor patch
  major=$(echo "$version" | cut -d. -f1)
  minor=$(echo "$version" | cut -d. -f2)
  patch=$(echo "$version" | cut -d. -f3 | sed 's/-.*//')  # strip any prerelease suffix

  case $bump_type in
    major) echo "$((major + 1)).0.0" ;;
    minor) echo "${major}.$((minor + 1)).0" ;;
    patch) echo "${major}.${minor}.$((patch + 1))" ;;
  esac
}

if [ -n "$1" ]; then
    # Version provided as argument (legacy usage)
    VERSION=$1
    if ! [[ $VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.-]+)?$ ]]; then
        echo -e "${COLOR_RED}Error: Invalid version format${COLOR_RESET}"
        echo "Version must follow semantic versioning (e.g., 1.0.0 or 1.0.0-beta.4)"
        exit 1
    fi
else
    # Interactive version selection
    CURRENT_VERSION=$(get_current_version)
    if [ -z "$CURRENT_VERSION" ]; then
        echo -e "${COLOR_RED}Error: No existing components tags found${COLOR_RESET}"
        echo "Usage: ./scripts/release-components.sh <version>"
        exit 1
    fi

    CURRENT_TAG="${PACKAGE_NAME}/v${CURRENT_VERSION}"
    NEXT_MAJOR=$(bump_version "$CURRENT_VERSION" major)
    NEXT_MINOR=$(bump_version "$CURRENT_VERSION" minor)
    NEXT_PATCH=$(bump_version "$CURRENT_VERSION" patch)

    echo ""
    echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
    echo -e "${COLOR_YELLOW}BlazorBlueprint.Components Release${COLOR_RESET}"
    echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
    echo -e "Current version: ${COLOR_GREEN}${CURRENT_VERSION}${COLOR_RESET}"
    echo ""

    # Show changes since last tag
    CHANGES=$(git log "${CURRENT_TAG}..HEAD" --oneline 2>/dev/null)
    if [ -n "$CHANGES" ]; then
        echo -e "${COLOR_CYAN}Changes since v${CURRENT_VERSION}:${COLOR_RESET}"
        echo "$CHANGES" | sed 's/^/  /'
    else
        echo -e "${COLOR_YELLOW}No commits since v${CURRENT_VERSION}${COLOR_RESET}"
    fi

    echo ""
    echo -e "${COLOR_CYAN}Select new version:${COLOR_RESET}"
    echo -e "  1) ${COLOR_GREEN}${NEXT_MAJOR}${COLOR_RESET} (Major)"
    echo -e "  2) ${COLOR_GREEN}${NEXT_MINOR}${COLOR_RESET} (Minor)"
    echo -e "  3) ${COLOR_GREEN}${NEXT_PATCH}${COLOR_RESET} (Patch)"
    echo ""
    read -p "Enter choice (1-3): " version_choice

    case $version_choice in
        1) VERSION=$NEXT_MAJOR ;;
        2) VERSION=$NEXT_MINOR ;;
        3) VERSION=$NEXT_PATCH ;;
        *)
            echo -e "${COLOR_RED}Invalid choice${COLOR_RESET}"
            exit 1
            ;;
    esac
fi

TAG_NAME="${PACKAGE_NAME}/v${VERSION}"
RELEASE_BRANCH="${PACKAGE_NAME}/release/v${VERSION}"

# Check if we're in the right directory
if [ ! -d "$PROJECT_PATH" ]; then
    echo -e "${COLOR_RED}Error: Project directory not found: $PROJECT_PATH${COLOR_RESET}"
    echo "Make sure you're running this script from the repository root"
    exit 1
fi

# Check for uncommitted changes (excluding blazorblueprint.css which we handle automatically)
CSS_FILE="$PROJECT_PATH/wwwroot/blazorblueprint.css"
OTHER_CHANGES=$(git diff-index --name-only HEAD -- | grep -v "^$CSS_FILE$" || true)

if [ -n "$OTHER_CHANGES" ]; then
    echo -e "${COLOR_RED}Error: You have uncommitted changes${COLOR_RESET}"
    echo "Please commit or stash your changes before creating a release"
    echo ""
    echo "Uncommitted files (excluding blazorblueprint.css which is handled automatically):"
    echo "$OTHER_CHANGES" | sed 's/^/  /'
    exit 1
fi

# If CSS has uncommitted changes, note it (will be handled during release)
if ! git diff --quiet -- "$CSS_FILE" 2>/dev/null; then
    echo -e "${COLOR_YELLOW}Note: blazorblueprint.css has uncommitted changes - will be rebuilt and committed during release${COLOR_RESET}"
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
echo "Checking BlazorBlueprint.Primitives version..."
LATEST_PRIMITIVES=$(get_latest_primitives_version)
CURRENT_PRIMITIVES=$(get_current_primitives_version)

echo ""
echo -e "${COLOR_YELLOW}BlazorBlueprint.Primitives Dependency${COLOR_RESET}"
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
echo -e "Package:     ${COLOR_GREEN}BlazorBlueprint.Components${COLOR_RESET}"
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
    echo -e "${COLOR_CYAN}Updating BlazorBlueprint.Primitives to $LATEST_PRIMITIVES...${COLOR_RESET}"
    update_primitives_version "$LATEST_PRIMITIVES"
    git add "$CSPROJ"
    git commit -m "chore: bump BlazorBlueprint.Primitives to $LATEST_PRIMITIVES"
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
    ../../tools/tailwindcss.exe -i wwwroot/css/blazorblueprint-input.css -o wwwroot/blazorblueprint.css --minify
else
    # Linux/macOS
    npx --yes @tailwindcss/cli@4.1.16 -i wwwroot/css/blazorblueprint-input.css -o wwwroot/blazorblueprint.css --minify
fi

cd - > /dev/null

# Check if CSS has any changes (content or line endings) and commit if needed
# Use git status instead of git diff to catch line-ending changes
CSS_STATUS=$(git status --porcelain -- "$PROJECT_PATH/wwwroot/blazorblueprint.css")
if [ -n "$CSS_STATUS" ]; then
    echo ""
    echo -e "${COLOR_YELLOW}CSS needs updating, committing...${COLOR_RESET}"
    git add "$PROJECT_PATH/wwwroot/blazorblueprint.css"
    git commit -m "chore: rebuild blazorblueprint.css for v${VERSION}"
    COMMITS_MADE=$((COMMITS_MADE + 1))
    echo -e "${COLOR_GREEN}✓ CSS rebuilt and committed${COLOR_RESET}"
else
    echo -e "${COLOR_GREEN}✓ CSS is up-to-date${COLOR_RESET}"
fi

# Create and push tag
echo ""
echo -e "${COLOR_GREEN}Creating tag: $TAG_NAME${COLOR_RESET}"
git tag -a "$TAG_NAME" -m "Release BlazorBlueprint.Components v${VERSION}"

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

This PR merges changes made during the release of **BlazorBlueprint.Components v${VERSION}** back to main.

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
echo "  https://github.com/blazorblueprintui/ui/actions"
echo ""
if [ $COMMITS_MADE -gt 0 ]; then
    echo -e "${COLOR_YELLOW}Remember to merge the PR to bring release changes back to main!${COLOR_RESET}"
    echo ""
fi
