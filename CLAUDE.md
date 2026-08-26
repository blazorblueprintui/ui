# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Important Rules

- Never commit to git unless explicitly instructed to
- This application does not support hot-reload — rebuild to see changes
- Do NOT add `Co-Authored-By` lines to commit messages
- Do NOT add "Generated with Claude" / AI-attribution footers to PR bodies or GitHub issue/PR comments
- When adding/changing public component API (new params, components, or features), add a demo example in `demos/BlazorBlueprint.Demo.Shared` (live example + `CodeExamples/.../*.txt` snippet + API Reference entry) — don't just temp-add an example to test and then revert it
- Always create a new branch from `develop` when starting work, unless explicitly told otherwise

---

## Build & Test Commands

```bash
# Accept new API surface snapshots after reviewing .received.txt files
# (copy .received.txt → .verified.txt in tests/BlazorBlueprint.Tests/ApiSurface/)
./scripts/run-tests.sh --accept

# Demo apps — each host is pinned to its own port
dotnet run --project demos/BlazorBlueprint.Demo.Server    # port 7172
dotnet run --project demos/BlazorBlueprint.Demo.Wasm      # port 7173
dotnet run --project demos/BlazorBlueprint.Demo.Auto       # port 7174
```

**Tailwind CSS**: Built automatically during `dotnet build` via standalone CLI (`tools/tailwindcss.exe`). Input: `src/BlazorBlueprint.Components/wwwroot/css/blazorblueprint-input.css` → Output: `wwwroot/blazorblueprint.css`.

---

## Architecture

**Two-layer component library** inspired by shadcn/ui and Radix UI, targeting .NET 8 Blazor (Server, WASM, and Auto render modes): `Primitives` are headless and unstyled, `Components` are styled and built on top of them.

### Dependency flow
`Components` → `Primitives` + `Icons.Lucide` (ProjectReference locally, PackageReference when packing for NuGet).

### Component structure
Components inherit directly from `ComponentBase` (no custom base class). Text inputs (`Input`, `Textarea`, `InputGroupInput`, `InputGroupTextarea`) implement their own `Value`/`ValueChanged` pattern rather than `InputBase<T>`.

### Overlay pattern
Overlay components (Dialog, Sheet, Popover, Tooltip, etc.) render through `<PortalHost />` which must be placed in the root layout. Uses `IPortalService` + `FloatingPortal` + `IPositioningService` for positioning.

---

## API Surface Tests

Tests use **Verify** (snapshot testing) to detect unintended public API changes. When you add/change/remove public API:

1. Run tests — they will fail with a `.received.txt` diff
2. Review the diff in `tests/BlazorBlueprint.Tests/ApiSurface/`
3. Accept: copy `.received.txt` → `.verified.txt` (or run `./scripts/run-tests.sh --accept`)

---

## Code Style

Style is enforced mechanically by `.editorconfig` + `TreatWarningsAsErrors` — the build is the source of truth, so it isn't restated here. The one convention the config does *not* enforce:

- **Private fields**: `camelCase` with **no underscore prefix** (deliberately unlike the common C# `_camelCase` default)
