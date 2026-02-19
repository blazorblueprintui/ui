# BlazorBlueprint.Primitives

Headless, unstyled Blazor primitive components with ARIA attributes and keyboard support. Build your own component library using these composable primitives.

## Features

- **Headless & Unstyled**: Complete control over styling — primitives provide behavior, accessibility, and state management without imposing any visual design
- **Built with Accessibility in Mind**: Includes ARIA attributes and keyboard interaction support
- **Composition-Based**: Flexible component composition patterns for building complex UIs
- **Type-Safe**: Full C# type safety with IntelliSense support
- **State Management**: Built-in controlled and uncontrolled state patterns
- **Keyboard Support**: Keyboard interaction support for interactive components
- **Two-Layer Portal Architecture**: Category-scoped portals (Container and Overlay) for efficient rendering
- **.NET 8**: Built for the latest .NET platform

## Installation

```bash
dotnet add package BlazorBlueprint.Primitives
```

### Setup

Register services in `Program.cs`:

```csharp
builder.Services.AddBlazorBlueprintPrimitives();
```

Add the portal host to your root layout (`MainLayout.razor`):

```razor
<BbPortalHost />
```

Add a single import to `_Imports.razor`:

```razor
@using BlazorBlueprint.Primitives
```

## Available Primitives

| Primitive | Description |
|-----------|-------------|
| **Accordion** | Collapsible content sections with single or multiple item expansion |
| **Checkbox** | Binary selection control with indeterminate state and `BbCheckboxIndicator` sub-component |
| **Collapsible** | Expandable content area with trigger control |
| **Dialog** | Modal dialogs with backdrop, focus management, and portal rendering |
| **Dropdown Menu** | Context menus with items, checkbox items, separators, and keyboard shortcuts |
| **Floating Portal** | Unified floating content infrastructure with ForceMount and positioning |
| **Hover Card** | Rich preview cards on hover with delay control |
| **Label** | Accessible labels for form controls with automatic association |
| **Popover** | Floating panels for additional content with positioning |
| **Radio Group** | Mutually exclusive options with keyboard navigation |
| **Select** | Dropdown selection with cascading type inference and display text resolution |
| **Sheet** | Side panels that slide in from viewport edges |
| **Switch** | Toggle control with `BbSwitchThumb` sub-component for automatic `data-state` sync |
| **Table** | Data table with header, body, rows, cells, and pagination |
| **Tabs** | Tabbed interface with keyboard navigation |
| **Tooltip** | Brief informational popups with hover/focus triggers |

### Services

| Service | Description |
|---------|-------------|
| `IPortalService` | Two-layer portal management with Container and Overlay categories |
| `IFocusManager` | Focus trapping and restoration for overlays |
| `IPositioningService` | Floating UI positioning with auto-update |
| `IKeyboardShortcutService` | Global keyboard shortcut registration and management |
| `DropdownManagerService` | Coordinates open/close state across multiple dropdowns |

## API Reference

### Accordion

```razor
<BbAccordion Type="AccordionType.Single" Collapsible="true" DefaultValue="item-1">
    <BbAccordionItem Value="item-1">
        <BbAccordionTrigger>Section 1</BbAccordionTrigger>
        <BbAccordionContent>Content 1</BbAccordionContent>
    </BbAccordionItem>
</BbAccordion>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Type` | `AccordionType` | `Single` | `Single` (one item open) or `Multiple` (many items open) |
| `Collapsible` | `bool` | `false` | When `Single`, allows closing all items |

### Checkbox

```razor
<BbCheckbox @bind-Checked="isChecked" Indeterminate="@isIndeterminate">
    <BbCheckboxIndicator />
</BbCheckbox>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Checked` | `bool` | `false` | Checked state |
| `Indeterminate` | `bool` | `false` | Shows partial/mixed state |

**BbCheckboxIndicator** renders the appropriate check or indeterminate SVG icon automatically based on parent state:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Custom content instead of default icons |
| `Size` | `int` | `14` | SVG icon size in pixels |
| `StrokeWidth` | `int` | `3` | SVG stroke width |

### Select

```razor
<BbSelect TValue="string" @bind-Value="selected" @bind-Open="isOpen">
    <BbSelectTrigger>
        <BbSelectValue Placeholder="Choose..." />
    </BbSelectTrigger>
    <BbSelectContent>
        <BbSelectItem Value="@("a")" Text="Option A" />
        <BbSelectItem Value="@("b")" Text="Option B" />
    </BbSelectContent>
</BbSelect>
```

Select uses `[CascadingTypeParameter]` — child components infer `TValue` from the parent. Supports `ItemClass` for parent-level item styling.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `TValue?` | — | Selected value (two-way bindable) |
| `Open` | `bool` | `false` | Open state (two-way bindable) |
| `ItemClass` | `string?` | `null` | CSS classes cascaded to all `BbSelectItem` children |

### Dialog

```razor
<BbDialog @bind-Open="isOpen">
    <BbDialogTrigger>Open</BbDialogTrigger>
    <BbDialogPortal>
        <BbDialogOverlay />
        <BbDialogContent>
            <BbDialogTitle>Title</BbDialogTitle>
            <BbDialogDescription>Description</BbDialogDescription>
            <BbDialogClose>Close</BbDialogClose>
        </BbDialogContent>
    </BbDialogPortal>
</BbDialog>
```

### Sheet

```razor
<BbSheet>
    <BbSheetTrigger>Open</BbSheetTrigger>
    <BbSheetPortal>
        <BbSheetOverlay />
        <BbSheetContent Side="SheetSide.Right">
            <BbSheetTitle>Title</BbSheetTitle>
            <BbSheetDescription>Description</BbSheetDescription>
            <BbSheetClose>Close</BbSheetClose>
        </BbSheetContent>
    </BbSheetPortal>
</BbSheet>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Side` | `SheetSide` | `Right` | `Top`, `Right`, `Bottom`, `Left` |

### Popover

```razor
<BbPopover>
    <BbPopoverTrigger>Open</BbPopoverTrigger>
    <BbPopoverContent Side="PopoverSide.Bottom" Align="PopoverAlign.Center">
        Content here
    </BbPopoverContent>
</BbPopover>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Side` | `PopoverSide` | `Bottom` | `Top`, `Right`, `Bottom`, `Left` |
| `Align` | `PopoverAlign` | `Center` | `Start`, `Center`, `End` |
| `CloseOnEscape` | `bool` | `true` | Close when Escape key pressed |
| `CloseOnClickOutside` | `bool` | `true` | Close when clicking outside |

### Tooltip

```razor
<BbTooltip DelayDuration="700" HideDelay="0">
    <BbTooltipTrigger>Hover me</BbTooltipTrigger>
    <BbTooltipContent>Tooltip text</BbTooltipContent>
</BbTooltip>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `DelayDuration` | `int` | `700` | Milliseconds before showing |
| `HideDelay` | `int` | `0` | Milliseconds before hiding |

### HoverCard

```razor
<BbHoverCard OpenDelay="700" CloseDelay="300">
    <BbHoverCardTrigger>Hover for preview</BbHoverCardTrigger>
    <BbHoverCardContent>Rich preview content</BbHoverCardContent>
</BbHoverCard>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `OpenDelay` | `int` | `700` | Milliseconds before showing |
| `CloseDelay` | `int` | `300` | Milliseconds before hiding |

### Dropdown Menu

```razor
<BbDropdownMenu ItemClass="px-2 py-1.5 cursor-pointer rounded hover:bg-accent">
    <BbDropdownMenuTrigger>Menu</BbDropdownMenuTrigger>
    <BbDropdownMenuContent>
        <BbDropdownMenuItem>Cut</BbDropdownMenuItem>
        <BbDropdownMenuItem>Copy</BbDropdownMenuItem>
        <BbDropdownMenuItem Href="https://example.com" Target="_blank">Visit Site</BbDropdownMenuItem>
        <BbDropdownMenuCheckboxItem @bind-Checked="isEnabled">Enable</BbDropdownMenuCheckboxItem>
    </BbDropdownMenuContent>
</BbDropdownMenu>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ItemClass` | `string?` | `null` | CSS classes cascaded to all menu items |

**BbDropdownMenuItem** supports `Href` and `Target` for link items — renders as `<a>` when `Href` is set.

### Switch

```razor
<BbSwitch @bind-Checked="isEnabled" class="relative h-6 w-11 rounded-full bg-input">
    <BbSwitchThumb class="pointer-events-none block h-5 w-5 rounded-full bg-background shadow-lg" />
</BbSwitch>
```

`BbSwitchThumb` automatically syncs `data-state` (`"checked"` / `"unchecked"`) from the parent via cascading parameter.

### Radio Group

```razor
<BbRadioGroup TValue="string" @bind-Value="selected" ItemClass="flex items-center gap-2">
    <BbRadioGroupItem Value="@("a")">Option A</BbRadioGroupItem>
    <BbRadioGroupItem Value="@("b")">Option B</BbRadioGroupItem>
</BbRadioGroup>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ItemClass` | `string?` | `null` | CSS classes cascaded to all radio items |

### Tabs

```razor
<BbTabs DefaultValue="tab1" Orientation="TabsOrientation.Horizontal"
        ActivationMode="TabsActivationMode.Automatic">
    <BbTabsList>
        <BbTabsTrigger Value="tab1">Tab 1</BbTabsTrigger>
    </BbTabsList>
    <BbTabsContent Value="tab1">Content</BbTabsContent>
</BbTabs>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Orientation` | `TabsOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `ActivationMode` | `TabsActivationMode` | `Automatic` | `Automatic` (on focus), `Manual` (on click) |

### Table

```razor
<BbTable TData="Person">
    <BbTableHeader>
        <BbTableRow>
            <BbTableHeaderCell>Name</BbTableHeaderCell>
            <BbTableHeaderCell>Email</BbTableHeaderCell>
        </BbTableRow>
    </BbTableHeader>
    <BbTableBody>
        @foreach (var person in people)
        {
            <BbTableRow>
                <BbTableCell>@person.Name</BbTableCell>
                <BbTableCell>@person.Email</BbTableCell>
            </BbTableRow>
        }
    </BbTableBody>
</BbTable>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SelectionMode` | `SelectionMode` | `None` | `None`, `Single`, `Multiple` |
| `SortDirection` | `SortDirection` | `None` | `None`, `Ascending`, `Descending` |

## Portal Architecture

Primitives use a two-layer portal system for rendering overlay content:

- **Container portals** (`PortalCategory.Container`): Dialog, Sheet — full-screen overlays
- **Overlay portals** (`PortalCategory.Overlay`): Popover, Select, Dropdown, Tooltip, HoverCard — positioned floating content

Each category has its own host (`BbContainerPortalHost`, `BbOverlayPortalHost`), so opening a tooltip doesn't cause Dialog portals to re-render. `BbPortalHost` is a convenience wrapper that renders both.

`BbFloatingPortal` keeps content mounted in the DOM when closed (`ForceMount` defaults to `true`), hidden via CSS. A `data-state` attribute (`"open"` / `"closed"`) on the portal content enables CSS animations.

## Controlled vs Uncontrolled

All stateful primitives support both controlled and uncontrolled modes:

### Uncontrolled (Component manages its own state)

```razor
<BbDialog>
    <BbDialogTrigger>Open</BbDialogTrigger>
    <BbDialogPortal>
        <BbDialogOverlay />
        <BbDialogContent>Content</BbDialogContent>
    </BbDialogPortal>
</BbDialog>
```

### Controlled (Parent component manages state)

```razor
<BbDialog @bind-Open="isDialogOpen">
    <BbDialogTrigger>Open</BbDialogTrigger>
    <BbDialogPortal>
        <BbDialogOverlay />
        <BbDialogContent>
            <button @onclick="() => isDialogOpen = false">Close</button>
        </BbDialogContent>
    </BbDialogPortal>
</BbDialog>

@code {
    private bool isDialogOpen = false;
}
```

## Design Philosophy

BlazorBlueprint.Primitives follows the "headless component" pattern popularized by Radix UI and Headless UI:

1. **Separation of Concerns**: Primitives handle behavior and accessibility; you handle the design
2. **Composability**: Build complex components by composing simple primitives
3. **No Style Opinions**: Zero CSS included — bring your own design system
4. **Accessibility by Default**: ARIA attributes and keyboard navigation built-in

## When to Use

**Use BlazorBlueprint.Primitives when:**
- Building a custom design system from scratch
- Need complete control over component styling
- Want to match a specific brand or design language
- Integrating with existing CSS frameworks or design tokens

**Consider [BlazorBlueprint.Components](https://www.nuget.org/packages/BlazorBlueprint.Components) when:**
- Want beautiful defaults with shadcn/ui design
- Prefer zero-configuration setup with pre-built CSS
- Need to ship quickly without custom styling

## Documentation

For full documentation, examples, and API reference, visit:
- [Documentation Site](https://blazorblueprintui.com)
- [GitHub Repository](https://github.com/blazorblueprintui/ui)

## License

Apache License 2.0 - see [LICENSE](https://github.com/blazorblueprintui/ui/blob/main/LICENSE) for details.

## Contributing

Contributions are welcome! Please see our [Contributing Guide](https://github.com/blazorblueprintui/ui/blob/main/CONTRIBUTING.md).
