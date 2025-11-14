# BlazorUI.Components

Pre-styled Blazor components built with shadcn/ui design and Tailwind CSS. Beautiful defaults that you can customize to match your brand.

## Features

- **shadcn/ui Design**: Beautiful, modern design language inspired by shadcn/ui
- **Pre-Styled Components**: Ready-to-use components with Tailwind CSS styling
- **Dark Mode**: Built-in dark mode support using CSS variables
- **Fully Customizable**: Override styles with Tailwind classes or custom CSS
- **Accessible**: Built on BlazorUI.Primitives with WCAG 2.1 AA compliance
- **Composable**: Flexible component composition patterns
- **Type-Safe**: Full C# type safety with IntelliSense support
- **.NET 8**: Built for the latest .NET platform

## Installation

```bash
dotnet add package BlazorUI.Components
```

This package automatically includes:
- `BlazorUI.Primitives` - Headless primitives providing behavior and accessibility
- `BlazorUI.Icons` - Lucide icon library

### Tailwind CSS Setup

BlazorUI Components requires Tailwind CSS. Add to your project:

1. Install Tailwind CSS:
```bash
npm install -D tailwindcss
npx tailwindcss init
```

2. Configure `tailwind.config.js`:
```js
module.exports = {
  content: [
    './**/*.{razor,html}',
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
```

3. Add Tailwind directives to your CSS file:
```css
@tailwind base;
@tailwind components;
@tailwind utilities;
```

For complete setup instructions, see our [documentation](https://github.com/blazorui-net/ui).

## Available Components

- **Accordion**: Collapsible content sections with smooth animations
- **Avatar**: User profile images with fallback initials and icons
- **Badge**: Labels for status, categories, and metadata
- **Button**: Interactive buttons with multiple variants and sizes
- **Checkbox**: Binary selection control with indeterminate state
- **Collapsible**: Expandable content area with trigger control
- **Combobox**: Autocomplete input with searchable dropdown
- **Command**: Command palette for quick actions and navigation
- **Dialog**: Modal dialogs with backdrop and focus management
- **Dropdown Menu**: Context menus with items, separators, and shortcuts
- **Hover Card**: Rich preview cards on hover with delay control
- **Input**: Text input fields with multiple types and sizes
- **Label**: Accessible labels for form controls
- **Popover**: Floating panels for additional content and actions
- **Radio Group**: Mutually exclusive options with keyboard navigation
- **Select**: Dropdown selection with groups and labels
- **Separator**: Visual dividers for content sections
- **Sheet**: Side panels that slide in from viewport edges
- **Sidebar**: Responsive navigation sidebar with collapsible menus
- **Skeleton**: Loading placeholders for content and images
- **Switch**: Toggle control for on/off states
- **Tabs**: Tabbed interface for organizing related content
- **Tooltip**: Brief informational popups on hover or focus

## Usage Example

```razor
@using BlazorUI.Components.Dialog

<Dialog>
    <DialogTrigger>Open Dialog</DialogTrigger>
    <DialogContent>
        <DialogHeader>
            <DialogTitle>Confirm Action</DialogTitle>
            <DialogDescription>
                Are you sure you want to proceed?
            </DialogDescription>
        </DialogHeader>
        <p>This action cannot be undone.</p>
        <DialogFooter>
            <DialogClose>Cancel</DialogClose>
            <Button Variant="default">Confirm</Button>
        </DialogFooter>
    </DialogContent>
</Dialog>
```

## Button Variants

```razor
@using BlazorUI.Components.Button

<Button Variant="default">Default</Button>
<Button Variant="destructive">Destructive</Button>
<Button Variant="outline">Outline</Button>
<Button Variant="secondary">Secondary</Button>
<Button Variant="ghost">Ghost</Button>
<Button Variant="link">Link</Button>

<Button Size="sm">Small</Button>
<Button Size="default">Default</Button>
<Button Size="lg">Large</Button>
<Button Size="icon">
    <IconPlus />
</Button>
```

## Form Example

```razor
@using BlazorUI.Components.Label
@using BlazorUI.Components.Input
@using BlazorUI.Components.Checkbox
@using BlazorUI.Components.Button

<div class="space-y-4">
    <div>
        <Label For="email">Email</Label>
        <Input Id="email" Type="email" Placeholder="name@example.com" />
    </div>

    <div class="flex items-center space-x-2">
        <Checkbox Id="terms" @bind-Checked="agreedToTerms" />
        <Label For="terms">I agree to the terms and conditions</Label>
    </div>

    <Button Disabled="@(!agreedToTerms)">Submit</Button>
</div>

@code {
    private bool agreedToTerms = false;
}
```

## Customizing Components

### Override Default Styles

Use the `Class` parameter to add custom Tailwind classes:

```razor
<Button Class="bg-purple-600 hover:bg-purple-700">
    Custom Button
</Button>

<Card Class="border-2 border-purple-500 shadow-xl">
    Custom Card Styling
</Card>
```

### Dark Mode

Components automatically support dark mode using CSS variables:

```razor
@* Automatically adapts to dark mode *@
<Button Variant="default">Themed Button</Button>
<Card>Themed Card</Card>
```

### Component Composition

Build complex UIs by composing components:

```razor
<Card>
    <CardHeader>
        <CardTitle>Settings</CardTitle>
        <CardDescription>Manage your account settings</CardDescription>
    </CardHeader>
    <CardContent class="space-y-4">
        <div>
            <Label>Email Notifications</Label>
            <Switch @bind-Checked="emailNotifications" />
        </div>
        <Separator />
        <div>
            <Label>Push Notifications</Label>
            <Switch @bind-Checked="pushNotifications" />
        </div>
    </CardContent>
    <CardFooter>
        <Button>Save Changes</Button>
    </CardFooter>
</Card>
```

## Design Philosophy

BlazorUI.Components follows the shadcn/ui philosophy:

1. **Copy-Paste Friendly**: Components are simple and can be customized in your project
2. **Tailwind First**: Uses Tailwind CSS for all styling
3. **Built on Primitives**: All behavior comes from BlazorUI.Primitives
4. **Design Tokens**: Uses CSS variables for theming
5. **Accessible by Default**: WCAG 2.1 AA compliance built-in

## When to Use

**Use BlazorUI.Components when:**
- Want beautiful defaults with shadcn/ui design
- Using or planning to use Tailwind CSS
- Need to ship quickly without building components from scratch
- Want dark mode support out of the box

**Consider [BlazorUI.Primitives](https://www.nuget.org/packages/BlazorUI.Primitives) when:**
- Building a completely custom design system
- Want zero opinions about styling
- Need to match a specific brand or design language

## Documentation

For full documentation, examples, and API reference, visit:
- [Documentation Site](https://github.com/blazorui-net/ui)
- [Component Demos](https://github.com/blazorui-net/ui)
- [GitHub Repository](https://github.com/blazorui-net/ui)

## Dependencies

- [BlazorUI.Primitives](https://www.nuget.org/packages/BlazorUI.Primitives) - Headless component primitives
- [BlazorUI.Icons](https://www.nuget.org/packages/BlazorUI.Icons) - Lucide icon library
- Tailwind CSS (not included, must be installed separately)

## License

MIT License - see [LICENSE](https://github.com/blazorui-net/ui/blob/main/LICENSE) for details.

## Contributing

Contributions are welcome! Please see our [Contributing Guide](https://github.com/blazorui-net/ui/blob/main/CONTRIBUTING.md).

---

Made with ❤️ by the BlazorUI team
