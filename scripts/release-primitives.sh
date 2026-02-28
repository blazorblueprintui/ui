#!/bin/bash

# Release script for BlazorBlueprint.Primitives
# Usage: ./scripts/release-primitives.sh [version]
# If no version is provided, the script will prompt you to pick a bump type.

set -e  # Exit on error

PACKAGE_NAME="primitives"
PROJECT_PATH="src/BlazorBlueprint.Primitives"
COLOR_GREEN='\033[0;32m'
COLOR_RED='\033[0;31m'
COLOR_YELLOW='\033[1;33m'
COLOR_CYAN='\033[0;36m'
COLOR_RESET='\033[0m'

COMMITS_MADE=0

# Get current version from the latest primitives git tag
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
        echo -e "${COLOR_RED}Error: No existing primitives tags found${COLOR_RESET}"
        echo "Usage: ./scripts/release-primitives.sh <version>"
        exit 1
    fi

    CURRENT_TAG="${PACKAGE_NAME}/v${CURRENT_VERSION}"
    NEXT_MAJOR=$(bump_version "$CURRENT_VERSION" major)
    NEXT_MINOR=$(bump_version "$CURRENT_VERSION" minor)
    NEXT_PATCH=$(bump_version "$CURRENT_VERSION" patch)

    echo ""
    echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
    echo -e "${COLOR_YELLOW}BlazorBlueprint.Primitives Release${COLOR_RESET}"
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

# Ensure we're on main and up to date with remote
CURRENT_BRANCH=$(git branch --show-current)
echo "Fetching from remote..."
git fetch origin main --quiet
LOCAL=$(git rev-parse main 2>/dev/null || echo "")
REMOTE=$(git rev-parse origin/main)

if [ "$CURRENT_BRANCH" != "main" ] || [ "$LOCAL" != "$REMOTE" ]; then
    if [ "$CURRENT_BRANCH" != "main" ]; then
        echo -e "${COLOR_YELLOW}Current branch: ${CURRENT_BRANCH}${COLOR_RESET}"
    else
        echo -e "${COLOR_YELLOW}Local main is behind origin/main${COLOR_RESET}"
    fi
    read -p "Switch to main and pull latest? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        git checkout main
        git pull
    else
        echo -e "${COLOR_YELLOW}Release cancelled${COLOR_RESET}"
        exit 0
    fi
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

# Check if release branch already exists
if git rev-parse "$RELEASE_BRANCH" >/dev/null 2>&1; then
    echo -e "${COLOR_RED}Error: Release branch $RELEASE_BRANCH already exists${COLOR_RESET}"
    echo "Delete the existing branch first: git branch -D $RELEASE_BRANCH"
    exit 1
fi

# Show what we're about to do
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo -e "${COLOR_YELLOW}Release Summary${COLOR_RESET}"
echo -e "${COLOR_YELLOW}═══════════════════════════════════════════════════${COLOR_RESET}"
echo -e "Package:  ${COLOR_GREEN}BlazorBlueprint.Primitives${COLOR_RESET}"
echo -e "Version:  ${COLOR_GREEN}${VERSION}${COLOR_RESET}"
echo -e "Branch:   ${COLOR_CYAN}${RELEASE_BRANCH}${COLOR_RESET}"
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

# Create release branch
echo ""
echo -e "${COLOR_CYAN}Creating release branch: $RELEASE_BRANCH${COLOR_RESET}"
git checkout -b "$RELEASE_BRANCH"

# No additional checks needed for primitives currently
# Future: Add any pre-release checks here

# Create and push tag
echo ""
echo -e "${COLOR_GREEN}Creating tag: $TAG_NAME${COLOR_RESET}"
git tag -a "$TAG_NAME" -m "Release BlazorBlueprint.Primitives v${VERSION}"

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

This PR merges changes made during the release of **BlazorBlueprint.Primitives v${VERSION}** back to main.

**Commits made during release:** $COMMITS_MADE

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
