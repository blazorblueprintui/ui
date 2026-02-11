#!/bin/bash

# BlazorBlueprint Issue Tester Runner
# Usage: ./run-ui-tests.sh
#
# Launches the Issue Tester Blazor Server app for reproducing
# and testing user-reported issues against BlazorBlueprint components.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ISSUE_TESTER="$PROJECT_ROOT/tests/BlazorBlueprint.IssueTester/BlazorBlueprint.IssueTester.csproj"

if [ ! -f "$ISSUE_TESTER" ]; then
    echo "Error: Issue Tester project not found at $ISSUE_TESTER"
    exit 1
fi

echo ""
echo "BlazorBlueprint Issue Tester"
echo "============================"
echo ""
echo "Project: $ISSUE_TESTER"
echo ""
echo "Press Ctrl+C to stop the server"
echo "----------------------------------------"
echo ""

dotnet run --project "$ISSUE_TESTER"
