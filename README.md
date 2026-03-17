# Blazor Blueprint

[![Website](https://img.shields.io/badge/Website-blazorblueprintui.com-blue)](https://blazorblueprintui.com)
[![NuGet](https://img.shields.io/nuget/v/BlazorBlueprint.Components)](https://www.nuget.org/packages/BlazorBlueprint.Components)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue)](LICENSE)

Beautiful UI components for Blazor, built with accessibility in mind. Inspired by [shadcn/ui](https://ui.shadcn.com/).

<p align="center">
  <a href="https://blazorblueprintui.com">
    <img src=".github/assets/hero.png" alt="Blazor Blueprint - Beautiful Components for Blazor" />
  </a>
</p>

<p align="center">
  <a href="https://blazorblueprintui.com"><strong>Documentation</strong></a> ·
  <a href="https://blazorblueprintui.com/components"><strong>Components</strong></a> ·
  <a href="https://blazorblueprintui.com/primitives"><strong>Primitives</strong></a>
</p>

<p align="center">
  <strong>100+ Components</strong> · <strong>11 Chart Types</strong> · <strong>3,200+ Icons</strong>
</p>

## Table of Contents

- [Why Blazor Blueprint?](#why-blazor-blueprint)
- [AI Integration](#ai-integration)
- [Getting Started](#getting-started)
- [Components](#components)
- [Primitives](#primitives)
- [Icons](#icons)
- [Theming](#theming)
- [Localization](#localization)
- [Architecture](#architecture)
- [Demo Applications](#demo-applications)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgments](#acknowledgments)

## Why Blazor Blueprint?

Blazor developers lack a modern, design-system-first UI library equivalent to what React developers have with shadcn/ui. Blazor Blueprint fills that gap — pre-built components and headless primitives that integrate directly with Tailwind and shadcn themes, targeting .NET 8 across Server, WebAssembly, and Auto render modes.

- **Zero Configuration** — Pre-built CSS included. No Tailwind setup, no Node.js, no build tools required.
- **Full shadcn/ui Theme Compatibility** — Use themes from [shadcn/ui](https://ui.shadcn.com/themes) or [tweakcn](https://tweakcn.com) directly.
- **Built with Accessibility in Mind** — Includes ARIA attributes, keyboard support, and semantic HTML structure.
- **Dark Mode Built-in** — Light and dark themes with CSS variables, ready out of the box.
- **Two-Layer Architecture** — Use pre-styled components for speed, or headless primitives for full control.

## AI Integration

Blazor Blueprint ships with a built-in MCP server and llms.txt — so Claude, Cursor, Copilot, and Windsurf generate correct component code on the first try.

- **MCP Server** — 11 tools give your AI structured access to every component, pattern, and API.
- **llms.txt** — 100+ machine-optimized docs so any LLM can understand the library without hallucinating.
- **Works Everywhere** — Claude Code, Cursor, GitHub Copilot, Windsurf — any MCP-compatible AI tool.

```bash
npx blazorblueprint add mcp-server
```

Learn more at [blazorblueprintui.com](https://blazorblueprintui.com).

## Getting Started

### Installation

```bash
# Styled components (includes Primitives)
dotnet add package BlazorBlueprint.Components

# Or just headless primitives for custom styling
dotnet add package BlazorBlueprint.Primitives
```

Optionally add an icon library:

```bash
dotnet add package BlazorBlueprint.Icons.Lucide      # 1,640+ icons
dotnet add package BlazorBlueprint.Icons.Heroicons    # 1,288 icons (4 variants)
dotnet add package BlazorBlueprint.Icons.Feather      # 286 icons
```

### Project Template

The fastest way to start a new project:

```bash
dotnet new install BlazorBlueprint.Templates
dotnet new blazorblueprint -n MyApp
```

### Setup

**1. Register services** in `Program.cs`:

```csharp
builder.Services.AddBlazorBlueprintComponents();
```

**2. Add imports** to `_Imports.razor`:

```razor
@using BlazorBlueprint.Components
```

**3. Add CSS** to your `App.razor` `<head>`:

```html
<!-- Your theme variables (optional) -->
<link rel="stylesheet" href="styles/theme.css" />
<!-- Blazor Blueprint styles -->
<link rel="stylesheet" href="_content/BlazorBlueprint.Components/blazorblueprint.css" />
```

**4. Add BbPortalHost** to your root layout (required for overlays like Dialog, Sheet, Popover):

```razor
@inherits LayoutComponentBase

<div class="min-h-screen bg-background">
    @Body
</div>

<BbPortalHost />
```

**5. Use components:**

```razor
<BbButton Variant="ButtonVariant.Default">Click me</BbButton>

<BbDialog>
    <BbDialogTrigger>
        <BbButton>Open Dialog</BbButton>
    </BbDialogTrigger>
    <BbDialogContent>
        <BbDialogHeader>
            <BbDialogTitle>Welcome</BbDialogTitle>
            <BbDialogDescription>
                Beautiful Blazor components inspired by shadcn/ui.
            </BbDialogDescription>
        </BbDialogHeader>
        <BbDialogFooter>
            <BbDialogClose>
                <BbButton Variant="ButtonVariant.Outline">Close</BbButton>
            </BbDialogClose>
        </BbDialogFooter>
    </BbDialogContent>
</BbDialog>
```

## Components

Blazor Blueprint includes **99 styled components** organized into the following categories.

### Enterprise Components

Production-ready components for complex data-driven applications:

| Component | Description |
|-----------|-------------|
| **Dashboard Grid** | Drag-and-drop, resizable widget layout for composing dashboards. Built on CSS Grid with responsive breakpoints, state persistence, keyboard accessibility, and loading/empty states. |
| **DataGrid** | Full-featured data grid with multi-column sorting, per-column filtering, row grouping with aggregates, hierarchical tree data, row selection, expandable rows, virtualization, context menus, pinned columns, column reordering/resizing/visibility, and state persistence. Supports `IQueryable`, `IEnumerable`, and `ItemsProvider` data sources. |
| **Dynamic Form** | Schema-driven form rendering — define fields, validation rules, and layout in a schema object, and the component generates the complete form with appropriate inputs, conditional visibility, and error display. |
| **Filter Builder** | Visual query builder for constructing complex filter expressions with AND/OR logic, nested condition groups, and type-aware operators. Pairs with DataGrid for interactive data exploration. |
| **Form Wizard** | Multi-step form wizard with progress indicators, per-step validation, optional/skippable steps, and navigation controls. |
| **Chart** | 11 chart types (Area, Bar, Candlestick, Funnel, Gauge, Heatmap, Line, Pie, Radar, Radial Bar, Scatter) built on Apache ECharts with a declarative composition API and automatic theme integration. |
| **Rich Text Editor** | WYSIWYG editor with formatting toolbar and HTML output. |
| **Markdown Editor** | Toolbar formatting with split-pane live preview. |

### Form & Input

| Component | Description |
|-----------|-------------|
| **Button** | Multiple variants (default, destructive, outline, secondary, ghost, link) with loading state and icon support |
| **Button Group** | Visually group related buttons with connected styling |
| **Calendar** | Interactive calendar with date constraints and range selection |
| **Checkbox** | Checkbox with indeterminate state and ARIA attributes |
| **Checkbox Group** | Group of checkboxes with select-all support |
| **Color Picker** | Color selection with swatches and custom input |
| **Combobox** | Searchable autocomplete dropdown |
| **Currency Input** | Currency-formatted numeric input with locale support |
| **Date Picker** | Date picker with popover calendar and formatting options |
| **Date Range Picker** | Dual-calendar range selection |
| **Dynamic Form** | Schema-driven form rendering — generates complete forms from a definition with automatic input selection, validation, conditional visibility, and layout customization |
| **Field** | Combines label, control, description, and error for structured forms |
| **Filter Builder** | Visual query builder for data filter expressions with AND/OR logic, condition groups, and two-way binding |
| **File Upload** | Drag-and-drop file upload with preview |
| **Form Field Checkbox** | Pre-configured checkbox field with built-in label, description, and validation |
| **Form Field Checkbox Group** | Pre-configured checkbox group field with built-in label, description, and manual validation |
| **Form Field Combobox** | Pre-configured combobox field with built-in label, description, and validation |
| **Form Field Currency Input** | Pre-configured currency input field with built-in label, description, and validation |
| **Form Field Date Picker** | Pre-configured date picker field with built-in label, description, and validation |
| **Form Field Date Range Picker** | Pre-configured date range picker field with built-in label, description, and manual validation |
| **Form Field File Upload** | Pre-configured file upload field with built-in label, description, and manual validation |
| **Form Field Input** | Pre-configured input field with built-in label, description, and validation |
| **Form Field Input OTP** | Pre-configured OTP input field with built-in label, description, and validation |
| **Form Field Masked Input** | Pre-configured masked input field with built-in label, description, and validation |
| **Form Field MultiSelect** | Pre-configured multi-select field with built-in label, description, and validation |
| **Form Field Native Select** | Pre-configured native select field with built-in label, description, and validation |
| **Form Field Numeric Input** | Pre-configured numeric input field with built-in label, description, and validation |
| **Form Field RadioGroup** | Pre-configured radio group field with built-in label, description, and validation |
| **Form Field Select** | Pre-configured select field with built-in label, description, and validation |
| **Form Field Switch** | Pre-configured switch field with built-in label, description, and validation |
| **Form Field Tag Input** | Pre-configured tag input field with built-in label, description, and validation |
| **Form Field Textarea** | Pre-configured textarea field with built-in label, description, and validation |
| **Form Field Time Picker** | Pre-configured time picker field with built-in label, description, and validation |
| **Form Wizard** | Multi-step form wizard with step navigation, progress indication, per-step validation, and optional/skippable steps |
| **Input** | Text input with multiple types and validation |
| **Input Field** | Typed input with automatic conversion, formatting, and validation for 15+ types |
| **Input Group** | Enhanced inputs with icons, buttons, and addons |
| **Input OTP** | One-time password input with individual digit fields |
| **Label** | Form labels with control association |
| **Masked Input** | Input with format masks (phone, SSN, etc.) |
| **MultiSelect** | Searchable multi-selection with tags and checkboxes |
| **Native Select** | Browser-native select with consistent styling |
| **Numeric Input** | Numeric input with increment/decrement controls |
| **Radio Group** | Radio buttons with keyboard navigation |
| **Range Slider** | Dual-handle slider for selecting value ranges |
| **Rating** | Star/icon rating input |
| **Select** | Dropdown select with search and keyboard navigation |
| **Slider** | Range input with drag support |
| **Sortable** | Drag-and-drop sortable lists and grids powered by SortableJS, with connected multi-list support and Kanban-style boards |
| **Split Button** | Primary action with dropdown for secondary actions |
| **Switch** | Toggle switch with customizable thumb |
| **Tag Input** | Inline tag/chip input for managing string lists with suggestions, validation, and customizable triggers |
| **Textarea** | Multi-line text input with auto-sizing and character count |
| **Time Picker** | Time selection with hour/minute controls |
| **Toggle** | Two-state toggle button |
| **Toggle Group** | Single or multi-select toggle group |

### Layout & Navigation

| Component | Description |
|-----------|-------------|
| **Accordion** | Collapsible content sections (single or multiple) |
| **Aspect Ratio** | Maintain width/height ratio for responsive content |
| **Breadcrumb** | Navigation trail with hierarchical location |
| **Card** | Container with header, content, footer, and action areas |
| **Carousel** | Slideshow for cycling through content |
| **Collapsible** | Expandable/collapsible panels |
| **Item** | Flexible list items with media, content, and actions |
| **Navigation Menu** | Horizontal navigation with dropdown menus |
| **Pagination** | Page navigation with first/previous/next/last controls and page size selection |
| **Resizable** | Resizable panels with drag handles |
| **Responsive Nav** | Adaptive navigation that switches between desktop and mobile layouts |
| **Scroll Area** | Custom scrollable area with styled scrollbars |
| **Separator** | Visual dividers (horizontal and vertical) |
| **Sidebar** | Responsive sidebar with collapsible icon mode, variants (default, floating, inset), and mobile sheet integration |
| **Tabs** | Tabbed interfaces with controlled/uncontrolled modes |
| **Timeline** | Vertical timeline with alignment, connector styles, loading states, and collapsible items |

### Overlay

| Component | Description |
|-----------|-------------|
| **Alert Dialog** | Modal requiring user acknowledgement |
| **Command** | Command palette with keyboard navigation, filtering, and dialog mode |
| **Context Menu** | Right-click menu with customizable items |
| **Dialog** | Modal dialogs with programmatic `DialogService` supporting alert, prompt, and custom component dialogs |
| **Drawer** | Mobile-friendly panel sliding from screen edge |
| **Dropdown Menu** | Menus with checkbox items and keyboard navigation |
| **Hover Card** | Rich hover previews |
| **Menubar** | Desktop application-style menu bar |
| **Popover** | Floating content containers |
| **Sheet** | Slide-out panels (top, right, bottom, left) |
| **Toast** | Notification messages with multiple positions via `ToastService` |
| **Tooltip** | Contextual hover tooltips |

### Data & Content

| Component            | Description                                                                                                        |
|----------------------|--------------------------------------------------------------------------------------------------------------------|
| **Chart**            | 11 chart types (Area, Bar, Candlestick, Funnel, Gauge, Heatmap, Line, Pie, Radar, Radial Bar, Scatter) with theme integration |
| **Dashboard Grid**   | Drag-and-drop, resizable widget layout for dashboards with responsive breakpoints, state persistence, and keyboard accessibility |
| **DataGrid**         | Enterprise data grid with sorting, per-column filtering, row grouping with aggregates, hierarchical tree data, selection, expandable rows, row virtualization, context menu, pinned columns, column reordering/resizing/visibility, and state persistence |
| **DataTable**        | Tables with sorting, filtering, pagination, and row selection                                                      |
| **DataView**         | Displays data using templates in a grid or list layout with sorting, filtering, pagination, and infinite scrolling |
| **Markdown Editor**  | Toolbar formatting with live preview                                                                               |
| **Rich Text Editor** | WYSIWYG editor with formatting toolbar and HTML output                                                             |
| **Tree View**        | Hierarchical data display with selection, checkboxes, lazy loading, drag-and-drop, search filtering, and data-driven or declarative modes |

### Display

| Component | Description |
|-----------|-------------|
| **Alert** | Callout messages with dismissible variants |
| **Avatar** | User avatars with fallback and group support |
| **Badge** | Status badges and labels |
| **Empty** | Empty state placeholder with icon, title, and description |
| **Kbd** | Keyboard shortcut display |
| **Progress** | Progress bar indicator |
| **Skeleton** | Loading placeholders |
| **Spinner** | Loading spinner with size variants |
| **Typography** | Consistent text styling (H1–H4, paragraph, lead, muted, blockquote, inline code, etc.) |

## Primitives

Blazor Blueprint's **26 headless primitives** provide behavior, ARIA attributes, and keyboard support without any styling. They handle all the complex interaction logic — focus trapping, ARIA attributes, keyboard shortcuts, portal rendering — while giving you complete control over appearance.

Use primitives when you need full design freedom or are building a custom design system.

| Primitive | What it handles |
|-----------|----------------|
| **Accordion** | Expand/collapse logic, single/multiple mode, keyboard navigation |
| **Alert Dialog** | Modal requiring explicit acknowledgement, no dismiss via overlay or Escape |
| **Checkbox** | Checked/unchecked/indeterminate state, ARIA attributes |
| **Collapsible** | Open/close state, animated transitions |
| **Context Menu** | Right-click menu with keyboard navigation and positioning |
| **Dashboard Grid** | Widget layout state, drag-and-drop coordination, resize handling, responsive breakpoints |
| **DataGrid** | Headless data grid with sorting, filtering, pagination, selection, expansion, row grouping, and state management |
| **Dialog** | Focus trapping, escape to close, scroll locking, portal rendering |
| **Dropdown Menu** | Open/close, keyboard navigation, click-outside dismissal |
| **Hover Card** | Hover intent, delay timing, portal positioning |
| **Label** | Label-control association |
| **Popover** | Floating positioning, portal rendering, click-outside |
| **Progress** | Accessible progress bar with determinate and indeterminate states |
| **Radio Group** | Single selection, arrow key navigation, ARIA roles |
| **Scroll Area** | Custom scrollbar with accessible ARIA scrollbar role and drag support |
| **Select** | Dropdown behavior, typeahead, keyboard navigation |
| **Separator** | Semantic or decorative divider with orientation support |
| **Sheet** | Side panel, focus trapping, scroll locking |
| **Slider** | Range input with keyboard navigation and pointer drag support |
| **Sortable** | Drag-and-drop sortable lists with SortableJS interop, ARIA live announcements, and connected multi-list support |
| **Switch** | Toggle state, keyboard support, ARIA switch role |
| **Table** | Sorting, pagination, row selection, keyboard row navigation |
| **Tabs** | Tab selection, arrow key navigation, ARIA tab roles |
| **Toggle** | Pressed/active state with aria-pressed support |
| **Tooltip** | Hover/focus triggers, delay, portal positioning |
| **Tree View** | Hierarchical expand/collapse, selection, checkbox state management |

Primitives are completely unstyled — bring your own CSS, Tailwind classes, or inline styles:

```razor
<BbAccordion class="my-accordion">
    <BbAccordionItem Value="item-1">
        <BbAccordionTrigger class="my-trigger">Section One</BbAccordionTrigger>
        <BbAccordionContent class="my-content">Content here.</BbAccordionContent>
    </BbAccordionItem>
</BbAccordion>
```

## Icons

Three icon library packages with **3,200+ total icons**:

| Package | Icons | Style | License |
|---------|-------|-------|---------|
| `BlazorBlueprint.Icons.Lucide` | 1,640+ | Stroke-based, consistent 24x24 | ISC |
| `BlazorBlueprint.Icons.Heroicons` | 1,288 | 4 variants (Outline, Solid, Mini, Micro) | MIT |
| `BlazorBlueprint.Icons.Feather` | 286 | Minimalist, stroke-based 24x24 | MIT |

## Theming

Blazor Blueprint is **100% compatible with shadcn/ui themes**. Use any theme from [shadcn/ui](https://ui.shadcn.com/themes) or [tweakcn](https://tweakcn.com) — copy the CSS variables into your `theme.css`:

```css
@layer base {
  :root {
    --background: oklch(1 0 0);
    --foreground: oklch(0.1450 0 0);
    --primary: oklch(0.2050 0 0);
    --primary-foreground: oklch(0.9850 0 0);
    /* ... */
  }

  .dark {
    --background: oklch(0.1450 0 0);
    --foreground: oklch(0.9850 0 0);
    --primary: oklch(0.9220 0 0);
    --primary-foreground: oklch(0.2050 0 0);
    /* ... */
  }
}
```

Load your theme **before** `blazorblueprint.css` so the variables are defined when referenced.

### Supported Variables

- **Colors** — `--background`, `--foreground`, `--primary`, `--secondary`, `--accent`, `--destructive`, `--muted`, and their foreground variants
- **Typography** — `--font-sans`, `--font-serif`, `--font-mono`
- **Layout** — `--radius`, `--shadow-*`
- **Charts** — `--chart-1` through `--chart-5`
- **Sidebar** — `--sidebar`, `--sidebar-primary`, `--sidebar-accent`, and variants

### Dark Mode

Apply the `.dark` class to your `<html>` element. All components automatically switch to dark mode colors.

## Localization

All component chrome strings (button labels, placeholders, ARIA labels, status messages) are localizable via the `IBbLocalizer` interface. The built-in `DefaultBbLocalizer` provides English defaults for all 189 strings.

### Quick Start

Subclass `DefaultBbLocalizer` to integrate with your localization strategy:

```csharp
public class AppLocalizer : DefaultBbLocalizer
{
    private readonly IStringLocalizer<SharedResources> localizer;

    public AppLocalizer(IStringLocalizer<SharedResources> localizer)
    {
        this.localizer = localizer;
    }

    public override string this[string key] => localizer[key] ?? base[key];
}

// Register in Program.cs
builder.Services.AddSingleton<IBbLocalizer, AppLocalizer>();
```

Components use string-key lookup (e.g., `Localizer["DataGrid.Loading"]`) with `string.Format` for parameterized strings. Calendar, DatePicker, DateRangePicker, and NumericInput automatically adapt to `CultureInfo.CurrentCulture` for date/number formatting.

## Architecture

Blazor Blueprint uses a **two-layer architecture** inspired by [Radix UI](https://www.radix-ui.com/):

```
BlazorBlueprint.Components     ← Pre-styled, ready to use
    ↓ builds on
BlazorBlueprint.Primitives     ← Headless, includes ARIA attributes and keyboard support
```

**Components** ship pre-built CSS matching the shadcn/ui design system. No Tailwind setup required — just reference the stylesheet and optionally provide theme variables.

**Primitives** are completely unstyled. They include ARIA attributes, focus management, and keyboard support for complex interaction patterns, giving you full control over appearance.

### Services

Services are registered via dependency injection:

- `AddBlazorBlueprintComponents()` — registers everything (Components + Primitives)
- `AddBlazorBlueprintPrimitives()` — registers only Primitives services

Key services include `IPortalService` (overlay rendering), `IFocusManager` (focus trapping), `IPositioningService` (floating element positioning), `IKeyboardShortcutService` (global shortcuts), `DialogService` (programmatic dialogs), `ToastService` (notifications), and `IBbLocalizer` (localization).

## Demo Applications

Demo applications are included for all three Blazor hosting models:

```bash
dotnet run --project demos/BlazorBlueprint.Demo.Server
dotnet run --project demos/BlazorBlueprint.Demo.Wasm
dotnet run --project demos/BlazorBlueprint.Demo.Auto
```

The demos share a common Razor Class Library (`BlazorBlueprint.Demo.Shared`) with thin hosting projects per render mode, demonstrating that components work identically across Server, WebAssembly, and Auto.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup and guidelines.

## License

Blazor Blueprint is open source software licensed under the [Apache License 2.0](LICENSE).

If you create derivative works, you must include the contents of the [NOTICE](NOTICE) file in your distribution.

## Acknowledgments

Blazor Blueprint is inspired by [shadcn/ui](https://ui.shadcn.com/) and the design principles of [Radix UI](https://www.radix-ui.com/). Blazor Blueprint is a complete reimplementation for Blazor/C# and contains no code from these projects.

- [shadcn/ui](https://ui.shadcn.com/) — MIT License, Copyright (c) 2023 shadcn
- [Radix UI](https://www.radix-ui.com/) — MIT License, Copyright (c) 2022-present WorkOS
- [Tailwind CSS](https://tailwindcss.com/) — Utility-first CSS framework
- [Lucide Icons](https://lucide.dev/) — ISC License
- [Heroicons](https://heroicons.com/) — MIT License, Tailwind Labs
- [Feather Icons](https://feathericons.com/) — MIT License
- [Apache ECharts](https://echarts.apache.org/) — Apache License 2.0
- [SortableJS](https://sortablejs.github.io/Sortable/) — MIT License
