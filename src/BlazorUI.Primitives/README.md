# BlazorUI.Primitives

Headless, unstyled Blazor primitive components with full accessibility support. Build your own component library using these composable primitives.

## Features

- **Headless & Unstyled**: Complete control over styling - primitives provide behavior, accessibility, and state management without imposing any visual design
- **Accessibility First**: Built-in ARIA attributes, keyboard navigation, and screen reader support following WCAG 2.1 AA standards
- **Composition-Based**: Flexible component composition patterns for building complex UIs
- **Type-Safe**: Full C# type safety with IntelliSense support
- **State Management**: Built-in controlled and uncontrolled state patterns
- **Keyboard Navigation**: Full keyboard support for all interactive components
- **.NET 8**: Built for the latest .NET platform

## Installation

```bash
dotnet add package BlazorUI.Primitives
```

## Available Primitives

- **Accordion**: Collapsible content sections with single or multiple item expansion
- **Checkbox**: Binary selection control with indeterminate state
- **Collapsible**: Expandable content area with trigger control
- **Combobox**: Autocomplete input with searchable dropdown
- **Dialog**: Modal dialogs with backdrop and focus management
- **Dropdown Menu**: Context menus with items, separators, and keyboard shortcuts
- **Hover Card**: Rich preview cards on hover with delay control
- **Label**: Accessible labels for form controls with automatic association
- **Popover**: Floating panels for additional content with positioning
- **Radio Group**: Mutually exclusive options with keyboard navigation
- **Select**: Dropdown selection with groups and virtualization support
- **Sheet**: Side panels that slide in from viewport edges
- **Switch**: Toggle control for on/off states
- **Tabs**: Tabbed interface with keyboard navigation
- **Tooltip**: Brief informational popups with hover/focus triggers

## Usage Example

```razor
@using BlazorUI.Primitives.Dialog

<DialogPrimitive>
    <DialogTrigger class="my-custom-button-class">
        Open Dialog
    </DialogTrigger>
    <DialogPortal>
        <DialogOverlay class="my-overlay-styles" />
        <DialogContent class="my-custom-dialog-styles">
            <DialogTitle class="my-title-styles">
                Custom Styled Dialog
            </DialogTitle>
            <DialogDescription class="my-description-styles">
                This is a fully customizable dialog.
            </DialogDescription>
            <p class="my-content-styles">Your content here</p>
            <DialogClose class="my-close-button">Close</DialogClose>
        </DialogContent>
    </DialogPortal>
</DialogPrimitive>
```

## Controlled vs Uncontrolled

All stateful primitives support both controlled and uncontrolled modes:

### Uncontrolled (Component manages its own state)

```razor
<DialogPrimitive>
    <DialogTrigger>Open</DialogTrigger>
    <DialogContent>
        <!-- Component handles open/close internally -->
    </DialogContent>
</DialogPrimitive>
```

### Controlled (Parent component manages state)

```razor
<DialogPrimitive @bind-Open="isDialogOpen">
    <DialogTrigger>Open</DialogTrigger>
    <DialogContent>
        <button @onclick="() => isDialogOpen = false">Close</button>
    </DialogContent>
</DialogPrimitive>

@code {
    private bool isDialogOpen = false;
}
```

## Design Philosophy

BlazorUI.Primitives follows the "headless component" pattern popularized by Radix UI and Headless UI:

1. **Separation of Concerns**: Primitives handle behavior and accessibility; you handle the design
2. **Composability**: Build complex components by composing simple primitives
3. **No Style Opinions**: Zero CSS included - bring your own design system
4. **Accessibility by Default**: ARIA attributes and keyboard navigation built-in

## When to Use

**Use BlazorUI.Primitives when:**
- Building a custom design system from scratch
- Need complete control over component styling
- Want to match a specific brand or design language
- Integrating with existing CSS frameworks or design tokens

**Consider [BlazorUI.Components](https://www.nuget.org/packages/BlazorUI.Components) when:**
- Want beautiful defaults with shadcn/ui design
- Prefer pre-styled components with Tailwind CSS
- Need to ship quickly without custom styling

## Documentation

For full documentation, examples, and API reference, visit:
- [Documentation Site](https://github.com/blazorui-net/ui)
- [GitHub Repository](https://github.com/blazorui-net/ui)

## License

MIT License - see [LICENSE](https://github.com/blazorui-net/ui/blob/main/LICENSE) for details.

## Contributing

Contributions are welcome! Please see our [Contributing Guide](https://github.com/blazorui-net/ui/blob/main/CONTRIBUTING.md).
