#!/bin/bash

# Release script for BlazorBlueprint.Components
# Usage: ./scripts/release-components.sh [version] [flags]
#
# Arguments:
#   version                     Semantic version (e.g., 1.0.0 or 1.0.0-beta.4)
#                               If omitted, prompts for interactive version selection.
#
# Flags:
#   --yes                       Skip the "Proceed with release?" confirmation prompt
#   --update-primitives         Auto-update Primitives to latest NuGet version (no prompt)
#   --release-notes <path>      Path to release notes file; committed on release branch
#                               and used as the annotated tag message
#
# Examples:
#   ./scripts/release-components.sh                          # Fully interactive
#   ./scripts/release-components.sh 2.2.0                    # Specify version, interactive prompts
#   ./scripts/release-components.sh 2.2.0 --yes              # Non-interactive, skip Primitives update
#   ./scripts/release-components.sh 2.2.0 --yes --update-primitives --release-notes notes.md

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
AUTO_CONFIRM=false
AUTO_UPDATE_PRIMITIVES=false
RELEASE_NOTES_PATH=""

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

# Parse arguments
VERSION=""
while [[ $# -gt 0 ]]; do
    case $1 in
        --yes)
            AUTO_CONFIRM=true
            shift
            ;;
        --update-primitives)
            AUTO_UPDATE_PRIMITIVES=true
            shift
            ;;
        --release-notes)
            if [[ -z "$2" || "$2" == --* ]]; then
                echo -e "${COLOR_RED}Error: --release-notes requires a file path argument${COLOR_RESET}"
                exit 1
            fi
            RELEASE_NOTES_PATH="$2"
            if [ ! -f "$RELEASE_NOTES_PATH" ]; then
                echo -e "${COLOR_RED}Error: Release notes file not found: $RELEASE_NOTES_PATH${COLOR_RESET}"
                exit 1
            fi
            shift 2
            ;;
        -*)
            echo -e "${COLOR_RED}Error: Unknown flag: $1${COLOR_RESET}"
            exit 1
            ;;
        *)
            if [ -n "$VERSION" ]; then
                echo -e "${COLOR_RED}Error: Multiple version arguments provided${COLOR_RESET}"
                exit 1
            fi
            VERSION="$1"
            shift
            ;;
    esac
done

if [ -n "$VERSION" ]; then
    # Version provided as argument
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

# Check for uncommitted changes
UNCOMMITTED_CHANGES=$(git diff-index --name-only HEAD -- || true)

if [ -n "$UNCOMMITTED_CHANGES" ]; then
    echo -e "${COLOR_RED}Error: You have uncommitted changes${COLOR_RESET}"
    echo "Please commit or stash your changes before creating a release"
    echo ""
    echo "Uncommitted files:"
    echo "$UNCOMMITTED_CHANGES" | sed 's/^/  /'
    exit 1
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
    if [ "$AUTO_UPDATE_PRIMITIVES" = true ]; then
        # Flag passed: auto-update
        WILL_UPDATE_PRIMITIVES=true
        echo -e "${COLOR_CYAN}Will auto-update Primitives to $LATEST_PRIMITIVES (--update-primitives)${COLOR_RESET}"
    elif [ "$AUTO_CONFIRM" = true ]; then
        # Non-interactive mode without --update-primitives: skip silently
        echo -e "${COLOR_YELLOW}Skipping Primitives update (pass --update-primitives to include)${COLOR_RESET}"
    else
        # Interactive mode: prompt
        echo -e "${COLOR_YELLOW}⚠️  Newer version available on NuGet${COLOR_RESET}"
        read -p "Update to $LATEST_PRIMITIVES? (y/N) " update_confirm
        if [[ "$update_confirm" =~ ^[Yy]$ ]]; then
            WILL_UPDATE_PRIMITIVES=true
        fi
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
if [ -n "$RELEASE_NOTES_PATH" ]; then
  echo -e "Notes:       ${COLOR_GREEN}${RELEASE_NOTES_PATH}${COLOR_RESET}"
fi
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo ""

# Confirm with user
if [ "$AUTO_CONFIRM" = true ]; then
    echo -e "${COLOR_CYAN}Auto-confirmed (--yes)${COLOR_RESET}"
else
    read -p "Proceed with release? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${COLOR_YELLOW}Release cancelled${COLOR_RESET}"
        exit 0
    fi
fi

# Create release branch
echo ""
echo -e "${COLOR_CYAN}Creating release branch: $RELEASE_BRANCH${COLOR_RESET}"
git checkout -b "$RELEASE_BRANCH"

# Commit release notes if provided
if [ -n "$RELEASE_NOTES_PATH" ]; then
    RELEASE_NOTES_DEST="$PROJECT_PATH/RELEASE_NOTES.md"
    if [ "$RELEASE_NOTES_PATH" != "$RELEASE_NOTES_DEST" ]; then
        cp "$RELEASE_NOTES_PATH" "$RELEASE_NOTES_DEST"
    fi
    echo ""
    echo -e "${COLOR_CYAN}Committing release notes...${COLOR_RESET}"
    git add "$RELEASE_NOTES_DEST"
    git commit -m "docs: add release notes for v${VERSION}"
    COMMITS_MADE=$((COMMITS_MADE + 1))
    echo -e "${COLOR_GREEN}✓ Committed release notes${COLOR_RESET}"
fi

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

# Note: blazorblueprint.css is no longer tracked in git.
# It is rebuilt automatically by the MSBuild target during dotnet build/pack in CI.

# Create and push tag
echo ""
echo -e "${COLOR_GREEN}Creating tag: $TAG_NAME${COLOR_RESET}"
if [ -n "$RELEASE_NOTES_PATH" ]; then
    RELEASE_NOTES_DEST="$PROJECT_PATH/RELEASE_NOTES.md"
    git tag -a "$TAG_NAME" -F "$RELEASE_NOTES_DEST"
else
    git tag -a "$TAG_NAME" -m "Release BlazorBlueprint.Components v${VERSION}"
fi

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
