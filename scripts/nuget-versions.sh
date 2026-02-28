#!/bin/bash

# Shows the latest published version and release date for BlazorBlueprint packages on NuGet.
# Usage: ./scripts/nuget-versions.sh

COLOR_GREEN='\033[0;32m'
COLOR_CYAN='\033[0;36m'
COLOR_YELLOW='\033[1;33m'
COLOR_RESET='\033[0m'

get_latest_version() {
  local id=$1
  curl -s "https://azuresearch-usnc.nuget.org/query?q=packageid:${id}&prerelease=false&take=1" \
    | grep -o '"version":"[^"]*"' | head -1 | cut -d'"' -f4
}

get_published_date() {
  local id=$1
  local version=$2
  local lower_id
  lower_id=$(echo "$id" | tr '[:upper:]' '[:lower:]')
  curl -s "https://api.nuget.org/v3/registration5-semver1/${lower_id}/${version}.json" \
    | grep -o '"published":"[^"]*"' | head -1 | cut -d'"' -f4 | cut -dT -f1
}

echo ""
echo -e "${COLOR_YELLOW}BlazorBlueprint NuGet Package Versions${COLOR_RESET}"
echo -e "${COLOR_YELLOW}═══════════════════════════════════════${COLOR_RESET}"
echo ""

for PACKAGE in "BlazorBlueprint.Primitives" "BlazorBlueprint.Components"; do
  VERSION=$(get_latest_version "$PACKAGE")
  if [ -z "$VERSION" ]; then
    echo -e "  ${COLOR_CYAN}${PACKAGE}${COLOR_RESET}: not found on NuGet"
  else
    DATE=$(get_published_date "$PACKAGE" "$VERSION")
    echo -e "  ${COLOR_CYAN}${PACKAGE}${COLOR_RESET}"
    echo -e "  Version:  ${COLOR_GREEN}${VERSION}${COLOR_RESET}"
    echo -e "  Released: ${COLOR_GREEN}${DATE}${COLOR_RESET}"
    echo ""
  fi
done
