# Architecture

> **Last Updated:** 2025-11-11

## Overview

BlazorUI is a UI component library for Blazor that replicates the design and functionality of shadcn/ui (https://ui.shadcn.com/). The library provides plug-and-play components that work across all Blazor hosting models (Server, WebAssembly, and Hybrid).

## System Architecture

### Component Library Structure

```
BlazorUI/
├── Components/                 # UI components (feature-based organization)
│   ├── Button/
│   │   ├── Button.razor
│   │   ├── Button.razor.cs
│   │   └── ButtonVariant.cs
│   ├── Input/
│   │   ├── Input.razor
│   │   └── Input.razor.cs
│   ├── Dialog/
│   │   ├── Dialog.razor
│   │   └── DialogService.cs
│   ├── Card/
│   ├── Badge/
│   ├── Alert/
│   ├── Dropdown/
│   ├── Tabs/
│   ├── Checkbox/
│   ├── Radio/
│   ├── Select/
│   ├── Textarea/
│   ├── Label/
│   ├── Separator/
│   └── ... (more components)
│
├── Shared/                     # Shared utilities and base classes
│   ├── Base/
│   │   └── ShadcnComponentBase.cs  # Common component logic
│   ├── Enums/
│   │   ├── Size.cs            # sm, md, lg
│   │   ├── Variant.cs         # default, outline, ghost, etc.
│   │   └── ColorScheme.cs     # primary, secondary, destructive, etc.
│   └── Extensions/
│       └── CssClassBuilder.cs # Helper for building CSS classes
│
├── Themes/                     # CSS and theming
│   ├── shadcn-base.css        # Base shadcn styles
│   ├── variables.css          # CSS custom properties
│   └── components.css         # Component-specific styles
│
├── wwwroot/                    # Static assets
│   ├── styles/
│   │   └── shadcnblazor.css   # Bundled CSS output
│   └── scripts/
│       └── interop.js         # JS interop if needed
│
└── BlazorUI.csproj        # Project file

Demo/                           # Demo application (separate project)
├── Pages/
│   ├── Index.razor
│   ├── ButtonDemo.razor
│   ├── InputDemo.razor
│   └── ... (component demos)
└── Demo.csproj
```

### Component Categories (Based on shadcn)

Following shadcn's component organization:

**1. Form Components**
- Button, Input, Textarea, Select, Checkbox, Radio, Label, Switch

**2. Data Display**
- Card, Badge, Alert, Avatar, Separator, Table

**3. Overlays**
- Dialog, Popover, Tooltip, Sheet, DropdownMenu

**4. Navigation**
- Tabs, NavigationMenu, Breadcrumb, Pagination

**5. Feedback**
- Toast, Progress, Skeleton, Spinner

**6. Layout**
- Accordion, Collapsible, AspectRatio, ScrollArea

### Primitive Components (BlazorUI.Primitives)

The library includes a separate Primitives package for headless, unstyled components that provide behavior without styling. These follow the Radix UI pattern (component wrappers around HTML elements).

**Available Primitives:**

1. **Table Primitive** (`BlazorUI.Primitives.Table`)
   - Headless table with sorting, pagination, and selection
   - Components: `Table<TData>`, `TableHeader`, `TableHeaderCell<TData>`, `TableBody`, `TableRow<TData>`, `TableCell`, `TablePagination<TData>`
   - Features:
     - Controlled/uncontrolled state management with `@bind-State`
     - Three-state column sorting (none → ascending → descending → none)
     - Pagination with page navigation and configurable page sizes
     - Row selection (single/multi-select modes)
     - Full keyboard navigation (Tab, Enter, Space for sorting and selection)
     - ARIA attributes for screen reader support
   - Usage Pattern:
     - Developer provides sorted/filtered data to `Data` parameter
     - Primitive handles pagination and selection state
     - Wrap HTML elements (`<table>`, `<thead>`, `<tbody>`, etc.) with primitive components
     - Style using Tailwind or custom CSS via `Class` parameter
   - State Management: Supports both internal state (uncontrolled) and external state (controlled with `@bind-State`)

## Key Design Patterns

### 1. CSS Variables for Theming

Following shadcn's approach with CSS custom properties:

```css
:root {
  --background: 0 0% 100%;
  --foreground: 222.2 84% 4.9%;

  --primary: 222.2 47.4% 11.2%;
  --primary-foreground: 210 40% 98%;

  --secondary: 210 40% 96.1%;
  --secondary-foreground: 222.2 47.4% 11.2%;

  --destructive: 0 84.2% 60.2%;
  --destructive-foreground: 210 40% 98%;

  --muted: 210 40% 96.1%;
  --muted-foreground: 215.4 16.3% 46.9%;

  --accent: 210 40% 96.1%;
  --accent-foreground: 222.2 47.4% 11.2%;

  --border: 214.3 31.8% 91.4%;
  --input: 214.3 31.8% 91.4%;
  --ring: 222.2 84% 4.9%;

  --radius: 0.5rem;
}

.dark {
  --background: 222.2 84% 4.9%;
  --foreground: 210 40% 98%;
  /* ... dark mode overrides ... */
}
```

### 2. Component Parameter Pattern

All components follow a consistent parameter structure:

```csharp
// Button.razor.cs
public partial class Button : ShadcnComponentBase
{
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Default;
    [Parameter] public Size Size { get; set; } = Size.Medium;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Class { get; set; }  // Additional CSS classes
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string CssClass => new CssClassBuilder()
        .Add("btn")
        .Add($"btn-{Variant.ToClassName()}")
        .Add($"btn-{Size.ToClassName()}")
        .Add("btn-disabled", Disabled)
        .Add(Class)
        .Build();
}
```

### 3. Tailwind Integration

Components use Tailwind utility classes, matching shadcn's approach:

```razor
@* Button.razor *@
<button
    class="@CssClass inline-flex items-center justify-center rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-50 disabled:pointer-events-none"
    disabled="@Disabled"
    @onclick="HandleClick">
    @ChildContent
</button>
```

### 4. Variant System

Each component supports multiple variants (matching shadcn):

```csharp
public enum ButtonVariant
{
    Default,    // bg-primary text-primary-foreground hover:bg-primary/90
    Destructive,// bg-destructive text-destructive-foreground hover:bg-destructive/90
    Outline,    // border border-input hover:bg-accent hover:text-accent-foreground
    Secondary,  // bg-secondary text-secondary-foreground hover:bg-secondary/80
    Ghost,      // hover:bg-accent hover:text-accent-foreground
    Link        // underline-offset-4 hover:underline text-primary
}
```

### 5. Accessibility First

All components implement WCAG 2.1 AA standards:

- **Keyboard Navigation:** Tab, Enter, Space, Arrow keys
- **ARIA Attributes:** `aria-label`, `aria-describedby`, `aria-expanded`, etc.
- **Focus Management:** Visible focus indicators, focus trap for modals
- **Screen Reader Support:** Semantic HTML, proper labeling

Example:
```razor
<button
    role="button"
    aria-label="@AriaLabel"
    aria-disabled="@Disabled"
    tabindex="@(Disabled ? -1 : 0)"
    @onkeydown="HandleKeyDown">
    @ChildContent
</button>
```

## Technology Integration

### Blazor Hosting Models

The library supports all three Blazor hosting models:

1. **Blazor Server**
   - Components render on server, UI updates via SignalR
   - JS interop works as expected

2. **Blazor WebAssembly**
   - Components run in browser via WebAssembly
   - Bundle size considerations (lazy loading, tree shaking)

3. **Blazor Hybrid (MAUI)**
   - Components run in native desktop/mobile apps
   - Native interop capabilities

**Compatibility Strategy:**
- Use platform-agnostic APIs (avoid server-specific or WASM-specific code)
- Minimal JS interop (use Blazor native features where possible)
- Test across all three hosting models

### Tailwind CSS Setup

Developers using this library will need to:

1. Install Tailwind CSS in their project
2. Configure `tailwind.config.js` to include BlazorUI classes:
```javascript
module.exports = {
  content: [
    './Components/**/*.{razor,html,cshtml}',
    './node_modules/shadcnblazor/**/*.razor'  // Include library components
  ],
  theme: {
    extend: {
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        // ... more color tokens
      },
    },
  },
}
```
3. Import the library's CSS variables in their main CSS file

## Component Development Workflow

### Adding a New Component

1. **Reference shadcn docs:** https://ui.shadcn.com/docs/components/[component-name]
2. **Create component folder:** `/Components/[ComponentName]/`
3. **Implement Razor component:** `[ComponentName].razor`
4. **Add code-behind (if needed):** `[ComponentName].razor.cs`
5. **Define variants and parameters:** Match shadcn's API
6. **Implement accessibility:** ARIA, keyboard navigation
7. **Test across hosting models:** Server, WASM, Hybrid
8. **Create demo page:** `/Demo/Pages/[ComponentName]Demo.razor`

### Component Checklist

- [ ] Visual parity with shadcn component
- [ ] All variants implemented (default, outline, ghost, etc.)
- [ ] Size options (sm, md, lg) if applicable
- [ ] Disabled state
- [ ] Keyboard navigation
- [ ] ARIA attributes
- [ ] Dark mode support
- [ ] Responsive behavior
- [ ] Works in all Blazor hosting models
- [ ] XML documentation comments
- [ ] Demo page with examples

## Future Enhancements

### Phase 1: Core Components
- Button, Input, Label, Textarea
- Card, Badge, Alert
- Dialog, Popover, Tooltip

### Phase 2: Forms & Validation
- Form, Select, Checkbox, Radio, Switch
- Integration with Blazor's `EditForm` and `FluentValidation`

### Phase 3: Complex Components
- ✅ **Table Primitive** (Completed 2025-11-11) - Headless table with sorting, pagination, selection, and filtering support
  - Radix UI pattern implementation
  - Developer-controlled sorting and filtering
  - Built-in pagination and selection state management
  - Full accessibility with ARIA attributes and keyboard navigation
- Calendar, DatePicker
- Command palette
- Data table with virtualization (advanced features on top of Table Primitive)

### Phase 4: Advanced Features
- Animation system (Framer Motion equivalent)
- Form builder
- Theming UI (runtime theme editor)
- CLI tool for scaffolding components

## Dependencies

**Required:**
- .NET 8 SDK
- Blazor (built-in with .NET 8)

**Consumer Requirements:**
- Tailwind CSS (installed in consuming project)
- Node.js (for Tailwind build process)

**Optional:**
- bUnit (for future automated testing)
- Playwright (for future E2E testing)

## Performance Considerations

1. **Minimal Re-renders:** Use `ShouldRender()` to avoid unnecessary updates
2. **Code Splitting:** Each component is independently loadable
3. **CSS Optimization:** Tailwind purges unused styles
4. **Lazy Loading:** Heavy components (Dialog, Popover) load on demand
5. **Virtualization:** Large lists use `Virtualize` component

## Security Considerations

1. **XSS Prevention:** Always use `@` syntax for HTML encoding
2. **Input Validation:** Form components validate user input
3. **Event Handler Safety:** Use Blazor's event system (no inline JS)
4. **CSP Compliance:** Avoid inline styles/scripts
5. **Dependency Scanning:** Regular updates for security patches

## Deployment

**NuGet Package:**
- Package ID: `BlazorUI`
- Target: `net8.0`
- Include: Components, CSS, JS (if needed)

**Distribution:**
- Publish to NuGet.org
- Semantic versioning (SemVer)
- Changelog for each release
- GitHub releases with release notes

---

**This architecture document should be updated as the project evolves.**
