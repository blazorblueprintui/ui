# BlazorUI

A comprehensive UI component library for Blazor inspired by [shadcn/ui](https://ui.shadcn.com/).

## Overview

BlazorUI brings the beautiful design system of shadcn/ui to Blazor applications. This library provides **zero-config, plug-and-play UI components** with full shadcn/ui compatibility, featuring pre-built CSS, styled components, and headless primitives that work across all Blazor hosting models (Server, WebAssembly, and Hybrid).

**No Tailwind CSS setup required** - just install the NuGet package and start building!

## Getting Started

### Installation

Install BlazorUI packages from NuGet:

```bash
# Headless primitives for custom styling
dotnet add package BlazorUI.Primitives

# Styled components with shadcn/ui design
dotnet add package BlazorUI.Components

# Lucide icon library
dotnet add package BlazorUI.Icons
```

### Quick Start

1. **Add to your `_Imports.razor`:**

```razor
@using BlazorUI.Components
```

2. **Add PortalHost to your layout:**

   For overlay components (Dialog, Sheet, Popover, etc.) to work correctly, add `<PortalHost />` to your root layout:

```razor
@inherits LayoutComponentBase

<div class="min-h-screen bg-background">
    <!-- Your layout content -->
    @Body
</div>

<PortalHost />
```

3. **Add CSS to your `App.razor`:**

   BlazorUI Components come with pre-built CSS - no Tailwind setup required!

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <!-- Your theme CSS variables -->
    <link rel="stylesheet" href="styles/theme.css" />
    <!-- Pre-built BlazorUI styles -->
    <link rel="stylesheet" href="_content/BlazorUI.Components/blazorui.css" />
    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

4. **Start using components:**

```razor
<Button Variant="ButtonVariant.Default">Click me</Button>

<Dialog>
    <DialogTrigger AsChild>
        <Button>Open Dialog</Button>
    </DialogTrigger>
    <DialogContent>
        <DialogHeader>
            <DialogTitle>Welcome to BlazorUI</DialogTitle>
            <DialogDescription>
                Beautiful Blazor components inspired by shadcn/ui
            </DialogDescription>
        </DialogHeader>
    </DialogContent>
</Dialog>
```

### Learn More

- **Contributing**: See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup and guidelines

## Theming

BlazorUI is **100% compatible with shadcn/ui themes**, making it easy to customize your application's appearance.

### Using Themes from shadcn/ui and tweakcn

You can use any theme from:
- **[shadcn/ui themes](https://ui.shadcn.com/themes)** - Official shadcn/ui theme gallery
- **[tweakcn.com](https://tweakcn.com)** - Advanced theme customization tool with live preview

Simply copy the CSS variables from these tools and paste them into your `wwwroot/styles/theme.css` file.

### Customizing Your Theme

1. **Create `wwwroot/styles/theme.css`** in your Blazor project

2. **Add your theme variables** inside the `:root` (light mode) and `.dark` (dark mode) selectors:

```css
@layer base {
  :root {
    --background: oklch(1 0 0);
    --foreground: oklch(0.1450 0 0);
    --primary: oklch(0.2050 0 0);
    --primary-foreground: oklch(0.9850 0 0);
    /* ... other variables */
  }

  .dark {
    --background: oklch(0.1450 0 0);
    --foreground: oklch(0.9850 0 0);
    --primary: oklch(0.9220 0 0);
    --primary-foreground: oklch(0.2050 0 0);
    /* ... other variables */
  }
}
```

3. **Reference it in your `App.razor`** before the BlazorUI CSS:

```razor
<link rel="stylesheet" href="styles/theme.css" />
<link rel="stylesheet" href="_content/BlazorUI.Components/blazorui.css" />
```

That's it! BlazorUI will automatically use your theme variables.

### Available Theme Variables

BlazorUI supports all standard shadcn/ui CSS variables:
- Colors: `--background`, `--foreground`, `--primary`, `--secondary`, `--accent`, `--destructive`, `--muted`, etc.
- Typography: `--font-sans`, `--font-serif`, `--font-mono`
- Layout: `--radius` (border radius), `--shadow-*` (shadows)
- Charts: `--chart-1` through `--chart-5`
- Sidebar: `--sidebar`, `--sidebar-primary`, `--sidebar-accent`, etc.

### Dark Mode

BlazorUI automatically supports dark mode by applying the `.dark` class to the `<html>` element. All components will automatically switch to dark mode colors when this class is present.

## Styling

### BlazorUI.Components (Pre-styled)

**No Tailwind CSS setup required!** BlazorUI Components include pre-built, production-ready CSS that ships with the NuGet package.

Simply add two CSS files to your `App.razor`:

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />

    <!-- 1. Your custom theme (defines CSS variables) -->
    <link rel="stylesheet" href="styles/theme.css" />

    <!-- 2. Pre-built BlazorUI styles (included in NuGet package) -->
    <link rel="stylesheet" href="_content/BlazorUI.Components/blazorui.css" />

    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

**Important:** Load your theme CSS **before** `blazorui.css` so the CSS variables are defined when BlazorUI references them.

**Note:** The pre-built CSS is already minified and optimized. You don't need to install Tailwind CSS, configure build processes, or set up any additional tooling.

### BlazorUI.Primitives (Headless)

Primitives are completely **headless** - they provide behavior and accessibility without any styling. You have complete freedom to style them however you want:

**Option 1: Tailwind CSS** (requires your own Tailwind setup)
```razor
<BlazorUI.Primitives.Accordion.Accordion class="space-y-4">
    <BlazorUI.Primitives.Accordion.AccordionItem class="border rounded-lg">
        <!-- Your custom Tailwind classes -->
    </BlazorUI.Primitives.Accordion.AccordionItem>
</BlazorUI.Primitives.Accordion.Accordion>
```

**Option 2: CSS Modules / Vanilla CSS**
```razor
<BlazorUI.Primitives.Accordion.Accordion class="my-accordion">
    <!-- Style with your own CSS -->
</BlazorUI.Primitives.Accordion.Accordion>
```

**Option 3: Inline Styles**
```razor
<BlazorUI.Primitives.Accordion.Accordion style="margin: 1rem;">
    <!-- Direct inline styling -->
</BlazorUI.Primitives.Accordion.Accordion>
```

Primitives give you complete control over styling while handling all the complex behavior, accessibility, and keyboard navigation for you. Unlike `BlazorUI.Components`, primitives don't include any CSS - you bring your own styling approach.

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
- **Zero Configuration** - Pre-built CSS included, no Tailwind setup required
- **Dark Mode Support** - Built-in light/dark theme switching with CSS variables
- **Responsive Design** - Mobile-first components that adapt to all screen sizes
- **Accessibility First** - WCAG 2.1 AA compliant with keyboard navigation and ARIA attributes
- **Keyboard Shortcuts** - Native keyboard navigation support (e.g., Ctrl/Cmd+B for sidebar toggle)
- **State Persistence** - Cookie-based state management for user preferences
- **TypeScript-Inspired API** - Familiar API design for developers coming from React/shadcn/ui
- **Pure Blazor** - No JavaScript dependencies, no Node.js required
- **Lucide Icons Integration** - Built-in icon component with 1000+ searchable icons
- **Form Validation Ready** - Works seamlessly with Blazor's form validation

## Architecture

BlazorUI uses a **two-layer architecture**:

### Styled Components Layer (`BlazorUI.Components`)
- Pre-styled components matching shadcn/ui design system
- **Pre-built CSS included** - no Tailwind configuration needed
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

## License

BlazorUI is open source software licensed under the [MIT License](LICENSE).


## Acknowledgments

BlazorUI is inspired by [shadcn/ui](https://ui.shadcn.com/) and based on the design principles of [Radix UI](https://www.radix-ui.com/).

While BlazorUI is a complete reimplementation for Blazor/C# and contains no code from these projects, we are grateful for their excellent work which inspired this library.

- shadcn/ui: MIT License - Copyright (c) 2023 shadcn
- Radix UI: MIT License - Copyright (c) 2022-present WorkOS

BlazorUI is an independent project and is not affiliated with or endorsed by shadcn or Radix UI.


- [Tailwind CSS](https://tailwindcss.com/) - Utility-first CSS framework
- [Lucide Icons](https://lucide.dev/) - Beautiful icon library