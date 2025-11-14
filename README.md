# BlazorUI

A comprehensive UI component library for Blazor based on [shadcn/ui](https://ui.shadcn.com/).

## Overview

BlazorUI brings the beautiful design system of shadcn/ui to Blazor applications. This library provides plug-and-play UI components with full shadcn/ui compatibility, featuring both styled components and headless primitives that work across all Blazor hosting models (Server, WebAssembly, and Hybrid).

## Project Structure

```
BlazorUI/
├── src/
│   ├── BlazorUI.Components/   # Styled components (shadcn/ui design)
│   ├── BlazorUI.Primitives/   # Headless UI primitives
│   └── BlazorUI.Icons/        # Lucide icon integration
├── demo/
│   └── BlazorUI.Demo/         # Demo Blazor Server app
└── .devflow/                  # DevFlow documentation
```

## Technology Stack

- **.NET 8 (LTS)**
- **Blazor** (Server, WebAssembly, Hybrid)
- **Tailwind CSS** (standalone CLI, no Node.js required)
- **CSS Variables** for theming
- **Lucide Icons** (1000+ beautiful icons)

## Getting Started

### Prerequisites

- .NET 8 SDK
- Tailwind CSS standalone CLI (included in demo app)

### Building the Solution

```bash
dotnet build
```

### Running the Demo

```bash
cd demo/BlazorUI.Demo
dotnet watch run
```

The demo app will be available at `https://localhost:5001`

## Components

BlazorUI includes **24 styled components** with full shadcn/ui design compatibility:

### Form Components
- **Button** - Multiple variants (default, destructive, outline, secondary, ghost, link) with icon support
- **Checkbox** - Accessible checkbox with indeterminate state
- **Input** - Text input with multiple types and validation support
- **Input Group** - Enhanced inputs with icons, buttons, and addons
- **Label** - Accessible form labels
- **RadioGroup** - Radio button groups with keyboard navigation
- **Select** - Dropdown select with search and keyboard navigation
- **Switch** - Toggle switch component
- **Combobox** - Searchable autocomplete dropdown

### Layout & Navigation
- **Accordion** - Collapsible content sections
- **Collapsible** - Expandable/collapsible panels
- **Separator** - Visual dividers
- **Sidebar** - Responsive sidebar with collapsible icon mode, variants (default, floating, inset), and mobile sheet integration
- **Tabs** - Tabbed interfaces with controlled/uncontrolled modes

### Overlay Components
- **Dialog** - Modal dialogs
- **Sheet** - Slide-out panels (top, right, bottom, left)
- **Popover** - Floating content containers
- **Tooltip** - Contextual hover tooltips
- **HoverCard** - Rich hover previews
- **DropdownMenu** - Context menus with nested submenus
- **Command** - Command palette with keyboard navigation

### Display Components
- **Avatar** - User avatars with fallback support
- **Badge** - Status badges and labels
- **Skeleton** - Loading placeholders

### Icons
- **Lucide Icons** - 1000+ beautiful icons with search and browse interface

## Primitives

BlazorUI also includes **15 headless primitive components** for building custom UI:

- Accordion Primitive
- Checkbox Primitive
- Collapsible Primitive
- Combobox Primitive
- Dialog Primitive
- Dropdown Menu Primitive
- Hover Card Primitive
- Label Primitive
- Popover Primitive
- Radio Group Primitive
- Select Primitive
- Sheet Primitive
- Switch Primitive
- Tabs Primitive
- Tooltip Primitive

All primitives are fully accessible, keyboard-navigable, and provide complete control over styling.

## Features

- **Full shadcn/ui Compatibility** - Drop-in Blazor equivalents of shadcn/ui components
- **Dark Mode Support** - Built-in light/dark theme switching with CSS variables
- **Responsive Design** - Mobile-first components that adapt to all screen sizes
- **Accessibility First** - WCAG 2.1 AA compliant with keyboard navigation and ARIA attributes
- **Keyboard Shortcuts** - Native keyboard navigation support (e.g., Ctrl/Cmd+B for sidebar toggle)
- **State Persistence** - Cookie-based state management for user preferences
- **TypeScript-Inspired API** - Familiar API design for developers coming from React/shadcn/ui
- **No JavaScript Dependencies** - Pure Blazor implementation with no Node.js required
- **Lucide Icons Integration** - Built-in icon component with 1000+ searchable icons
- **Form Validation Ready** - Works seamlessly with Blazor's form validation

## Architecture

BlazorUI uses a **two-layer architecture**:

### Styled Components Layer (`BlazorUI.Components`)
- Pre-styled components matching shadcn/ui design system
- Built on top of primitives for consistency
- Ready to use out of the box
- Full theme support via CSS variables

### Primitives Layer (`BlazorUI.Primitives`)
- Headless, unstyled components
- Complete accessibility implementation
- Keyboard navigation and ARIA support
- Maximum flexibility for custom styling

### Key Principles
- **Feature-based organization** - Each component in its own folder with all related files
- **Code-behind pattern** - Clean separation of markup (`.razor`) and logic (`.razor.cs`)
- **CSS Variables theming** - Runtime theme switching with light/dark mode support
- **Accessibility first** - WCAG 2.1 AA compliance with comprehensive keyboard navigation
- **Composition over inheritance** - Components designed to be composed together
- **Progressive enhancement** - Works without JavaScript where possible

For detailed architecture documentation, see `.devflow/architecture.md`

## Development

This project uses [DevFlow](https://github.com/mathewtaylor/devflow) for structured feature development.

### Quality Standards

- **Naming Conventions**
  - PascalCase for public members
  - camelCase for private fields (no underscore prefix)
  - Component files: `ComponentName.razor` + `ComponentName.razor.cs`

- **Documentation**
  - XML documentation for all public APIs
  - Inline comments for complex logic
  - README files for each major feature

- **Testing**
  - Manual testing across all Blazor hosting models (Server, WASM, Hybrid)
  - Cross-browser compatibility testing (Chrome, Firefox, Edge, Safari)
  - Accessibility validation with screen readers
  - Keyboard navigation verification

- **Code Quality**
  - Follow Blazor best practices
  - Avoid unnecessary re-renders
  - Proper disposal of resources
  - Null-safety throughout

## Publishing Releases

BlazorUI publishes three independent NuGet packages with automated CI/CD:

- **BlazorUI.Primitives** - Headless UI primitives
- **BlazorUI.Components** - Styled components
- **BlazorUI.Icons** - Lucide icon library

### Release Process

Each package can be released independently with its own version number using the provided release scripts:

```bash
# Release Primitives
./scripts/release-primitives.sh 1.0.0-beta.4

# Release Components
./scripts/release-components.sh 1.1.0-beta.2

# Release Icons
./scripts/release-icons.sh 1.0.3
```

The scripts will:
1. Validate the version format (semantic versioning)
2. Check for uncommitted changes
3. Create and push a git tag (e.g., `primitives/v1.0.0-beta.4`)
4. Trigger automated GitHub Actions workflow

### Automated Publishing

When a tag is pushed, GitHub Actions will automatically:
1. Build the project
2. Run tests (if any)
3. Pack the NuGet package
4. Publish to NuGet.org

Monitor the release workflow at: https://github.com/blazorui-net/ui/actions

### Versioning Strategy

- Each package maintains its own independent version
- Versions are determined by git tags (e.g., `primitives/v1.0.0-beta.4`)
- MinVer automatically calculates package versions from git tags
- Pre-release versions use `-beta.X` suffix for beta releases

### Prerequisites for Publishing

To publish packages, you need:
1. Write access to the GitHub repository
2. NUGET_API_KEY secret configured in GitHub repository settings
3. Git tag push permissions

## License

TBD

## Acknowledgments

- [shadcn/ui](https://ui.shadcn.com/) - Original design system and inspiration
- [Radix UI](https://www.radix-ui.com/) - Primitive component patterns and accessibility standards
- [Tailwind CSS](https://tailwindcss.com/) - Utility-first CSS framework
- [Lucide Icons](https://lucide.dev/) - Beautiful icon library
