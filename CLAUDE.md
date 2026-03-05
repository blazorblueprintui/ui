# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Important Rules

- Never commit to git unless explicitly instructed to
- This application does not support hot-reload — rebuild to see changes
- Do NOT add `Co-Authored-By` lines to commit messages
- Always create a new branch from `develop` when starting work, unless explicitly told otherwise

---

## Build & Test Commands

```bash
# Build the Components project (most common during development)
dotnet build src/BlazorBlueprint.Components/BlazorBlueprint.Components.csproj --nologo --verbosity quiet

# Build entire solution
dotnet build

# Run tests (xunit + Verify snapshot tests)
dotnet test tests/BlazorBlueprint.Tests/BlazorBlueprint.Tests.csproj --verbosity normal

# Accept new API surface snapshots after reviewing .received.txt files
# (copy .received.txt → .verified.txt in tests/BlazorBlueprint.Tests/ApiSurface/)
./scripts/run-tests.sh --accept

# Run demo apps
dotnet run --project demos/BlazorBlueprint.Demo.Server    # port 7172
dotnet run --project demos/BlazorBlueprint.Demo.Wasm      # port 7173
dotnet run --project demos/BlazorBlueprint.Demo.Auto       # port 7174
```

**Tailwind CSS**: Built automatically during `dotnet build` via standalone CLI (`tools/tailwindcss.exe`). Input: `src/BlazorBlueprint.Components/wwwroot/css/blazorblueprint-input.css` → Output: `wwwroot/blazorblueprint.css`.

---

## Architecture

**Two-layer component library** inspired by shadcn/ui and Radix UI, targeting .NET 8 Blazor (Server, WASM, and Auto render modes):

### Projects

| Project | Purpose |
|---------|---------|
| `BlazorBlueprint.Primitives` | Headless, unstyled components with accessibility/keyboard/ARIA. No CSS. |
| `BlazorBlueprint.Components` | Styled components built on Primitives. Ships pre-built Tailwind CSS. |
| `BlazorBlueprint.Icons.*` | Three icon packages: Lucide, Heroicons, Feather |
| `BlazorBlueprint.Demo.Shared` | Shared demo pages/layouts (Razor Class Library) |
| `BlazorBlueprint.Demo.Server/Wasm/Auto` | Thin host projects per render mode |
| `BlazorBlueprint.Tests` | API surface snapshot tests (Verify + xunit) |

### Dependency flow
`Components` → `Primitives` + `Icons.Lucide` (ProjectReference locally, PackageReference when packing for NuGet).

### Component structure
Each component lives in its own folder under `Components/` or `Primitives/` with:
- `ComponentName.razor` — markup
- `ComponentName.razor.cs` — code-behind (partial class)
- Enum/type files (e.g., `ButtonVariant.cs`, `ButtonSize.cs`)

Components inherit directly from `ComponentBase` (no custom base class). Text inputs (`Input`, `Textarea`, `InputGroupInput`, `InputGroupTextarea`) implement their own `Value`/`ValueChanged` pattern rather than `InputBase<T>`.

### JS Interop
- Primitives JS: `src/BlazorBlueprint.Primitives/wwwroot/js/primitives/` (focus, positioning, portal, scroll-lock, keyboard shortcuts, resize, dismiss)
- Components JS: `src/BlazorBlueprint.Components/wwwroot/js/` (file-upload, slider, resizable, sidebar, etc.)

### Services (registered via DI)
- `AddBlazorBlueprintComponents()` — registers everything (Components + Primitives)
- `AddBlazorBlueprintPrimitives()` — registers only Primitives services
- Key services: `IPortalService`, `IFocusManager`, `IPositioningService`, `DropdownManagerService`, `IKeyboardShortcutService`, `ToastService`

### Overlay pattern
Overlay components (Dialog, Sheet, Popover, Tooltip, etc.) render through `<PortalHost />` which must be placed in the root layout. Uses `IPortalService` + `FloatingPortal` + `IPositioningService` for positioning.

---

## API Surface Tests

Tests use **Verify** (snapshot testing) to detect unintended public API changes. When you add/change/remove public API:

1. Run tests — they will fail with a `.received.txt` diff
2. Review the diff in `tests/BlazorBlueprint.Tests/ApiSurface/`
3. Accept: copy `.received.txt` → `.verified.txt` (or run `./scripts/run-tests.sh --accept`)

---

## Code Style (enforced at build — TreatWarningsAsErrors)

- **Braces required** on all control flow (IDE0011 enforced as error)
- **Private fields**: `camelCase` (no underscore prefix)
- **Public members/types**: `PascalCase`
- **Constants**: `PascalCase`
- **Interfaces**: `I` prefix
- **Nullable reference types** enabled with null diagnostics as errors (CS8600-CS8625)
- Prefer `var` when type is apparent
- Allman-style braces (new line before `{`)
- Indent: 4 spaces for `.cs`/`.razor`, 2 spaces for `.csproj`/`.json`/`.js`/`.css`/`.yml`

---

## Versioning

Uses **MinVer** with git tags. Tag prefixes:
- Components: `components/v` (e.g., `components/v3.0.0`)
- Primitives: `primitives/v` (e.g., `primitives/v2.3.2`)

---

## Current Branch: v3

Active development branch for v3 with breaking changes. See `V3-MIGRATION-GUIDE.md` for details. Key v3 changes: namespace flattening, Combobox/MultiSelect API redesign, trigger `AsChild` defaults, and new usability parameters across components.
