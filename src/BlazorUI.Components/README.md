# BlazorUI.Components

Pre-styled Blazor components with shadcn/ui design. Beautiful defaults with zero configuration - no Tailwind setup required!

## Features

- **Zero Configuration**: Pre-built CSS included - no Tailwind setup required
- **shadcn/ui Design**: Beautiful, modern design language inspired by shadcn/ui
- **Pre-Styled Components**: Production-ready components with pre-built styling
- **Dark Mode**: Built-in dark mode support using CSS variables
- **shadcn/ui Theme Compatible**: Use any theme from shadcn/ui or tweakcn.com
- **Fully Customizable**: Override styles with custom CSS or Tailwind classes
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
- Pre-built CSS - No Tailwind setup required!

## Quick Start

### 1. Add to your `_Imports.razor`:

```razor
@using BlazorUI.Components
@using BlazorUI.Components.Button
@using BlazorUI.Components.Input
@using BlazorUI.Components.Dialog
@using BlazorUI.Components.Sheet
@using BlazorUI.Components.Accordion
@using BlazorUI.Components.Tabs
@using BlazorUI.Components.Select
@using BlazorUI.Components.Avatar
@using BlazorUI.Components.Badge
```

Add imports for each component namespace you use. This gives access to component-specific enums like `ButtonVariant`, `InputType`, etc.

### 2. Add CSS to your `App.razor`:

BlazorUI Components come with pre-built CSS - no Tailwind setup required!

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />

    <!-- Optional: Your custom theme (defines CSS variables) -->
    <link rel="stylesheet" href="styles/theme.css" />

    <!-- Pre-built BlazorUI styles (included in NuGet package) -->
    <link rel="stylesheet" href="_content/BlazorUI.Components/blazorui.css" />

    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

### 3. Start using components:

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
                Beautiful Blazor components with zero configuration
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

That's it! No Tailwind installation, no build configuration needed.

## Available Components

- **Accordion**: Collapsible content sections with smooth animations
- **Avatar**: User profile images with fallback initials and icons
- **Badge**: Labels for status, categories, and metadata
- **Button**: Interactive buttons with multiple variants and sizes
- **ButtonGroup**: Grouped button controls with shared styling
- **Card**: Content container with header, content, and footer sections
- **Checkbox**: Binary selection control with indeterminate state
- **Collapsible**: Expandable content area with trigger control
- **Combobox**: Autocomplete input with searchable dropdown
- **Command**: Command palette for quick actions and navigation
- **DataTable**: Data table with sorting, pagination, and toolbar
- **Dialog**: Modal dialogs with backdrop and focus management
- **Dropdown Menu**: Context menus with items, separators, and shortcuts
- **Field**: Form field wrapper with label, description, and error states
- **Hover Card**: Rich preview cards on hover with delay control
- **Input**: Text input fields with multiple types and sizes
- **InputGroup**: Grouped input controls with addons
- **Item**: List item container for menus and lists
- **Label**: Accessible labels for form controls
- **MarkdownEditor**: Markdown editor with toolbar and live preview
- **MultiSelect**: Multi-select dropdown with tag support
- **Popover**: Floating panels for additional content and actions
- **Radio Group**: Mutually exclusive options with keyboard navigation
- **RichTextEditor**: Rich text editor with formatting toolbar
- **Select**: Dropdown selection with groups and labels
- **Separator**: Visual dividers for content sections
- **Sheet**: Side panels that slide in from viewport edges
- **Sidebar**: Responsive navigation sidebar with collapsible menus
- **Skeleton**: Loading placeholders for content and images
- **Switch**: Toggle control for on/off states
- **Tabs**: Tabbed interface for organizing related content
- **Textarea**: Multi-line text input field
- **Tooltip**: Brief informational popups on hover or focus

## Component API Reference

### Button

```razor
<Button
    Variant="ButtonVariant.Default"
    Size="ButtonSize.Default"
    Type="ButtonType.Button"
    IconPosition="IconPosition.Start"
    Disabled="false">
    Click me
</Button>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Variant` | `ButtonVariant` | `Default` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `Size` | `ButtonSize` | `Default` | `Small`, `Default`, `Large`, `Icon`, `IconSmall`, `IconLarge` |
| `Type` | `ButtonType` | `Button` | `Button`, `Submit`, `Reset` |
| `IconPosition` | `IconPosition` | `Start` | `Start`, `End` |

### Input

```razor
<Input
    Type="InputType.Email"
    Placeholder="name@example.com"
    Disabled="false" />
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Type` | `InputType` | `Text` | `Text`, `Email`, `Password`, `Number`, `Tel`, `Url`, `Search`, `Date`, `Time`, `File` |

### Avatar

```razor
<Avatar Size="AvatarSize.Default">
    <AvatarImage Src="user.jpg" Alt="User" />
    <AvatarFallback>JD</AvatarFallback>
</Avatar>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Size` | `AvatarSize` | `Default` | `Small`, `Default`, `Large`, `ExtraLarge` |

### Badge

```razor
<Badge Variant="BadgeVariant.Default">New</Badge>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Variant` | `BadgeVariant` | `Default` | `Default`, `Secondary`, `Destructive`, `Outline` |

### Accordion

```razor
<Accordion Type="AccordionType.Single" Collapsible="true">
    <AccordionItem Value="item-1">
        <AccordionTrigger>Section 1</AccordionTrigger>
        <AccordionContent>Content 1</AccordionContent>
    </AccordionItem>
</Accordion>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Type` | `AccordionType` | `Single` | `Single` (one item open) or `Multiple` (many items open) |
| `Collapsible` | `bool` | `false` | When `Single`, allows closing all items |

### Tabs

```razor
<Tabs
    DefaultValue="tab1"
    Orientation="TabsOrientation.Horizontal"
    ActivationMode="TabsActivationMode.Automatic">
    <TabsList>
        <TabsTrigger Value="tab1">Tab 1</TabsTrigger>
        <TabsTrigger Value="tab2">Tab 2</TabsTrigger>
    </TabsList>
    <TabsContent Value="tab1">Content 1</TabsContent>
    <TabsContent Value="tab2">Content 2</TabsContent>
</Tabs>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Orientation` | `TabsOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `ActivationMode` | `TabsActivationMode` | `Automatic` | `Automatic` (on focus), `Manual` (on click) |

### Sheet

```razor
<Sheet>
    <SheetTrigger AsChild>
        <Button>Open Sheet</Button>
    </SheetTrigger>
    <SheetContent Side="SheetSide.Right">
        <SheetHeader>
            <SheetTitle>Sheet Title</SheetTitle>
            <SheetDescription>Sheet description</SheetDescription>
        </SheetHeader>
        <!-- Content -->
    </SheetContent>
</Sheet>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Side` | `SheetSide` | `Right` | `Top`, `Right`, `Bottom`, `Left` |

### Select

```razor
<Select TValue="string" @bind-Value="selectedValue">
    <SelectTrigger>
        <SelectValue Placeholder="Select an option" />
    </SelectTrigger>
    <SelectContent>
        <SelectItem Value="@("option1")">Option 1</SelectItem>
        <SelectItem Value="@("option2")">Option 2</SelectItem>
    </SelectContent>
</Select>
```

Select is a generic component. Specify `TValue` for type safety.

### Separator

```razor
<Separator Orientation="SeparatorOrientation.Horizontal" />
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Orientation` | `SeparatorOrientation` | `Horizontal` | `Horizontal`, `Vertical` |

### Skeleton

```razor
<Skeleton Shape="SkeletonShape.Rectangular" Class="w-full h-4" />
<Skeleton Shape="SkeletonShape.Circular" Class="w-12 h-12" />
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Shape` | `SkeletonShape` | `Rectangular` | `Rectangular`, `Circular` |

### DataTable

```razor
<DataTable TItem="User" Items="users" SelectionMode="DataTableSelectionMode.Multiple">
    <DataTableColumn TItem="User" Field="x => x.Name" Header="Name" />
    <DataTableColumn TItem="User" Field="x => x.Email" Header="Email" />
</DataTable>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `SelectionMode` | `DataTableSelectionMode` | `None` | `None`, `Single`, `Multiple` |

## Theming

BlazorUI is **100% compatible with shadcn/ui themes**. Customize your application's appearance using CSS variables.

### Using Themes from shadcn/ui and tweakcn

You can use any theme from:
- **[shadcn/ui themes](https://ui.shadcn.com/themes)** - Official shadcn/ui theme gallery
- **[tweakcn.com](https://tweakcn.com)** - Advanced theme customization tool

Simply copy the CSS variables and paste them into your `wwwroot/styles/theme.css` file.

### Example Theme

Create `wwwroot/styles/theme.css`:

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

Reference it in your `App.razor` **before** the BlazorUI CSS:

```razor
<link rel="stylesheet" href="styles/theme.css" />
<link rel="stylesheet" href="_content/BlazorUI.Components/blazorui.css" />
```

### Dark Mode

Dark mode automatically activates when you add the `.dark` class to the `<html>` element. All components will switch to dark mode colors.

## Usage Example

```razor
@using BlazorUI.Components.Dialog

<Dialog>
    <DialogTrigger AsChild>
        <Button>Open Dialog</Button>
    </DialogTrigger>
    <DialogContent>
        <DialogHeader>
            <DialogTitle>Confirm Action</DialogTitle>
            <DialogDescription>
                Are you sure you want to proceed?
            </DialogDescription>
        </DialogHeader>
        <p>This action cannot be undone.</p>
        <DialogFooter>
            <DialogClose AsChild>
                <Button Variant="ButtonVariant.Outline">Cancel</Button>
            </DialogClose>
            <Button Variant="ButtonVariant.Default">Confirm</Button>
        </DialogFooter>
    </DialogContent>
</Dialog>
```

### AsChild Pattern

Use `AsChild` on trigger components to use your own styled elements instead of the default button:

```razor
<DropdownMenu>
    <DropdownMenuTrigger AsChild>
        <Button Variant="ButtonVariant.Outline">
            Actions
            <LucideIcon Name="chevron-down" Size="16" />
        </Button>
    </DropdownMenuTrigger>
    <DropdownMenuContent>
        <DropdownMenuItem>Edit</DropdownMenuItem>
        <DropdownMenuItem>Delete</DropdownMenuItem>
    </DropdownMenuContent>
</DropdownMenu>
```

This is the industry-standard pattern from Radix UI/shadcn/ui. When `AsChild` is true, the child component (e.g., Button) automatically receives trigger behavior via `TriggerContext`.

## Form Example

```razor
@using BlazorUI.Components.Label
@using BlazorUI.Components.Input
@using BlazorUI.Components.Checkbox
@using BlazorUI.Components.Button

<div class="space-y-4">
    <div>
        <Label For="email">Email</Label>
        <Input Id="email" Type="InputType.Email" Placeholder="name@example.com" />
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

Use the `Class` parameter to add custom CSS classes or Tailwind classes (if you have Tailwind set up):

```razor
<Button Class="bg-purple-600 hover:bg-purple-700">
    Custom Button
</Button>

<Card Class="border-2 border-purple-500 shadow-xl">
    Custom Card Styling
</Card>
```

**Note:** BlazorUI Components include pre-built CSS and don't require Tailwind. However, you can still use Tailwind classes for customization if you've set up Tailwind in your project.

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

BlazorUI.Components follows the shadcn/ui philosophy with zero-configuration deployment:

1. **Zero Configuration**: Pre-built CSS included - just install and use
2. **shadcn/ui Compatible**: Uses the same design tokens and CSS variables
3. **Built on Primitives**: All behavior comes from BlazorUI.Primitives
4. **Theme Tokens**: Fully themeable using CSS variables
5. **Accessible by Default**: WCAG 2.1 AA compliance built-in
6. **Customizable**: Override with custom CSS or add Tailwind if needed

## When to Use

**Use BlazorUI.Components when:**
- Want beautiful defaults with shadcn/ui design
- Need zero-configuration setup (no build tools required)
- Want to ship quickly without building components from scratch
- Need dark mode and theming support out of the box
- Want shadcn/ui theme compatibility

**Consider [BlazorUI.Primitives](https://www.nuget.org/packages/BlazorUI.Primitives) when:**
- Building a completely custom design system
- Want zero opinions about styling
- Need to match a specific brand or design language
- Prefer full control over all CSS

## Documentation

For full documentation, examples, and API reference, visit:
- [Documentation Site](https://github.com/blazorui-net/ui)
- [Component Demos](https://github.com/blazorui-net/ui)
- [GitHub Repository](https://github.com/blazorui-net/ui)

## Dependencies

- [BlazorUI.Primitives](https://www.nuget.org/packages/BlazorUI.Primitives) - Headless component primitives (auto-installed)
- Pre-built CSS (included in package)
- No external dependencies required!

**Optional:**
- Tailwind CSS (if you want to use Tailwind classes for customization)

## License

Apache License 2.0 - see [LICENSE](https://github.com/blazorui-net/ui/blob/main/LICENSE) for details.

## Contributing

Contributions are welcome! Please see our [Contributing Guide](https://github.com/blazorui-net/ui/blob/main/CONTRIBUTING.md).

---

Made with ❤️ by the BlazorUI team
