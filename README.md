# BlazorUI

[![Website](https://img.shields.io/badge/Website-blazorui.net-blue)](https://blazorui.net)
[![NuGet](https://img.shields.io/nuget/v/BlazorUI.Components)](https://www.nuget.org/packages/BlazorUI.Components)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue)](LICENSE)

Beautiful, accessible UI components for Blazor. Inspired by [shadcn/ui](https://ui.shadcn.com/).

<p align="center">
  <a href="https://blazorui.net">
    <img src=".github/assets/hero.png" alt="BlazorUI - Beautiful Components for Blazor" />
  </a>
</p>

<p align="center">
  <a href="https://blazorui.net"><strong>Documentation</strong></a> ·
  <a href="https://blazorui.net/components"><strong>Components</strong></a> ·
  <a href="https://blazorui.net/primitives"><strong>Primitives</strong></a>
</p>

## Overview

BlazorUI brings the elegant design system of shadcn/ui to Blazor applications. Build modern, responsive interfaces with **60+ styled components** and **17 headless primitives** that work across all Blazor hosting models—Server, WebAssembly, and Hybrid.

### Why BlazorUI?

- **Zero Configuration** — Pre-built CSS included. No Tailwind setup, no Node.js, no build tools required.
- **Full shadcn/ui Compatibility** — Use themes from [shadcn/ui](https://ui.shadcn.com/themes) or [tweakcn](https://tweakcn.com) directly.
- **Accessibility First** — WCAG 2.1 AA compliant with keyboard navigation and screen reader support.
- **Dark Mode Built-in** — Light and dark themes with CSS variables, ready out of the box.
- **Pure Blazor** — No JavaScript dependencies. Just C# and Razor components.

## Getting Started

### Installation

Install BlazorUI packages from NuGet:

```bash
# Headless primitives for custom styling
dotnet add package BlazorUI.Primitives

# Styled components with shadcn/ui design
dotnet add package BlazorUI.Components

# Icon libraries (choose one or more)
dotnet add package BlazorUI.Icons.Lucide      # 1,640 icons - stroke-based, consistent
dotnet add package BlazorUI.Icons.Heroicons   # 1,288 icons - 4 variants (outline, solid, mini, micro)
dotnet add package BlazorUI.Icons.Feather     # 286 icons - minimalist, stroke-based
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
        <DialogFooter>
            <DialogClose AsChild>
                <Button Variant="ButtonVariant.Outline">Close</Button>
            </DialogClose>
        </DialogFooter>
    </DialogContent>
</Dialog>
```

**AsChild Pattern:** Use `AsChild` on trigger components to use your own styled elements (like Button) instead of the default button. This is the industry-standard pattern from Radix UI/shadcn/ui.

### Learn More

- **Documentation & Demos**: Visit [blazorui.net](https://blazorui.net) for full documentation and interactive examples
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

BlazorUI includes **60+ styled components** with full shadcn/ui design compatibility:

### Form Components
- **Button** - Multiple variants (default, destructive, outline, secondary, ghost, link) with icon support
- **Button Group** - Visually group related buttons with connected styling
- **Checkbox** - Accessible checkbox with indeterminate state
- **Combobox** - Searchable autocomplete dropdown
- **Field** - Combine labels, controls, and help text for accessible forms
- **Input** - Text input with multiple types and validation support
- **Input Group** - Enhanced inputs with icons, buttons, and addons
- **Input OTP** - One-time password input with individual digit fields
- **Label** - Accessible form labels
- **MultiSelect** - Searchable multi-selection with tags and checkboxes
- **Native Select** - Browser-native select dropdown with consistent styling
- **RadioGroup** - Radio button groups with keyboard navigation
- **Select** - Dropdown select with search and keyboard navigation
- **Switch** - Toggle switch component
- **Textarea** - Multi-line text input with automatic sizing
- **Calendar** - Interactive calendar for date selection with constraints
- **Date Picker** - Date picker with popover calendar and formatting options
- **Slider** - Range input for selecting numeric values with drag support
- **Toggle** - Two-state button for toggleable options
- **Toggle Group** - Group of toggles with single or multiple selection

### Layout & Navigation
- **Accordion** - Collapsible content sections
- **Aspect Ratio** - Maintain width/height ratio for responsive content
- **Breadcrumb** - Navigation trail showing hierarchical location
- **Card** - Container for grouped content with header and footer
- **Carousel** - Slideshow component for cycling through content
- **Collapsible** - Expandable/collapsible panels
- **Item** - Flexible list items with media, content, and actions
- **Pagination** - Page navigation with previous/next controls
- **Resizable** - Resizable panels with drag handles
- **Scroll Area** - Custom scrollable area with styled scrollbars
- **Separator** - Visual dividers
- **Sidebar** - Responsive sidebar with collapsible icon mode, variants (default, floating, inset), and mobile sheet integration
- **Tabs** - Tabbed interfaces with controlled/uncontrolled modes
- **Navigation Menu** - Horizontal navigation with dropdown menus

### Overlay Components
- **Command** - Command palette with keyboard navigation and filtering
- **Context Menu** - Right-click menu with customizable items
- **Dialog** - Modal dialogs
- **Drawer** - Mobile-friendly panel sliding from screen edge
- **DropdownMenu** - Context menus with nested submenus
- **Menubar** - Desktop application-style menu bar
- **HoverCard** - Rich hover previews
- **Popover** - Floating content containers
- **Sheet** - Slide-out panels (top, right, bottom, left)
- **Tooltip** - Contextual hover tooltips
- **Toast** - Temporary notification messages with multiple positions

### Data & Content
- **DataTable** - Powerful tables with sorting, filtering, pagination, and selection
- **MarkdownEditor** - Rich text editor with toolbar formatting and live preview
- **RichTextEditor** - WYSIWYG editor with formatting toolbar and HTML output

### Display Components
- **Alert** - Callout messages with variants for important notifications
- **Alert Dialog** - Modal requiring user acknowledgement
- **Avatar** - User avatars with fallback support
- **Badge** - Status badges and labels
- **Empty** - Empty state placeholder with icon, title, and description
- **Kbd** - Keyboard shortcut display component
- **Progress** - Progress bar indicator
- **Skeleton** - Loading placeholders
- **Spinner** - Loading spinner with size variants
- **Typography** - Consistent text styling (headings, paragraphs, lists, etc.)

### Icons

BlazorUI offers **three icon library packages** to suit different design preferences:

- **Lucide Icons** (`BlazorUI.Icons.Lucide`) - 1,640 beautiful, consistent stroke-based icons
  - ISC licensed
  - 24x24 viewBox, 2px stroke width
  - Perfect for: Modern, clean interfaces

- **Heroicons** (`BlazorUI.Icons.Heroicons`) - 1,288 icons across 4 variants
  - MIT licensed by Tailwind Labs
  - Variants: Outline (24x24), Solid (24x24), Mini (20x20), Micro (16x16)
  - Perfect for: Tailwind-based designs, flexible sizing needs

- **Feather Icons** (`BlazorUI.Icons.Feather`) - 286 minimalist stroke-based icons
  - MIT licensed
  - 24x24 viewBox, 2px stroke width
  - Perfect for: Simple, lightweight projects

## Primitives

BlazorUI also includes **17 headless primitive components** for building custom UI:

- Accordion Primitive
- Checkbox Primitive
- Collapsible Primitive
- Combobox Primitive
- Dialog Primitive
- Dropdown Menu Primitive
- Hover Card Primitive
- Label Primitive
- MultiSelect Primitive
- Popover Primitive
- Radio Group Primitive
- Select Primitive
- Sheet Primitive
- Switch Primitive
- Table Primitive
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
- **Icon Library Options** - 3 separate icon packages (Lucide, Heroicons, Feather) with 3,200+ total icons
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

BlazorUI is open source software licensed under the [Apache License 2.0](LICENSE).

If you create derivative works, you must include the contents of the [NOTICE](NOTICE) file in your distribution, as required by the Apache License 2.0.


## Acknowledgments

BlazorUI is inspired by [shadcn/ui](https://ui.shadcn.com/) and based on the design principles of [Radix UI](https://www.radix-ui.com/).

While BlazorUI is a complete reimplementation for Blazor/C# and contains no code from these projects, we are grateful for their excellent work which inspired this library.

- shadcn/ui: MIT License - Copyright (c) 2023 shadcn
- Radix UI: MIT License - Copyright (c) 2022-present WorkOS

BlazorUI is an independent project and is not affiliated with or endorsed by shadcn or Radix UI.


- [Tailwind CSS](https://tailwindcss.com/) - Utility-first CSS framework
- [Lucide Icons](https://lucide.dev/) - Beautiful stroke-based icon library (ISC License)
- [Heroicons](https://heroicons.com/) - Icon library by Tailwind Labs (MIT License)
- [Feather Icons](https://feathericons.com/) - Minimalist icon library (MIT License)