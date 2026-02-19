# BlazorBlueprint.Components

Pre-styled Blazor components with shadcn/ui design. Beautiful defaults with zero configuration - no Tailwind setup required!

## Features

- **Zero Configuration**: Pre-built CSS included - no Tailwind setup required
- **shadcn/ui Design**: Beautiful, modern design language inspired by shadcn/ui
- **Pre-Styled Components**: Production-ready components with pre-built styling
- **Dark Mode**: Built-in dark mode support using CSS variables
- **shadcn/ui Theme Compatible**: Use any theme from shadcn/ui or tweakcn.com
- **Fully Customizable**: Override styles with custom CSS or Tailwind classes
- **Built with Accessibility in Mind**: Includes ARIA attributes and keyboard support via BlazorBlueprint.Primitives
- **Composable**: Flexible component composition patterns
- **Type-Safe**: Full C# type safety with IntelliSense support
- **.NET 8**: Built for the latest .NET platform

## Installation

```bash
dotnet add package BlazorBlueprint.Components
```

This package automatically includes:
- `BlazorBlueprint.Primitives` - Headless primitives providing behavior and accessibility
- `BlazorBlueprint.Icons.Lucide` - Lucide icon set
- Pre-built CSS - No Tailwind setup required!

## Quick Start

### 1. Register services in `Program.cs`:

```csharp
builder.Services.AddBlazorBlueprintComponents();
```

This registers all required services including portal management, focus trapping, positioning, toast notifications, and programmatic dialogs.

### 2. Add to your `_Imports.razor`:

```razor
@using BlazorBlueprint.Components
@using BlazorBlueprint.Primitives
```

That's it — two imports give you access to all components and their enums (`ButtonVariant`, `InputType`, `AccordionType`, etc.).

### 3. Add CSS to your `App.razor`:

BlazorBlueprint Components come with pre-built CSS - no Tailwind setup required!

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />

    <!-- Optional: Your custom theme (defines CSS variables) -->
    <link rel="stylesheet" href="styles/theme.css" />

    <!-- Pre-built BlazorBlueprint styles (included in NuGet package) -->
    <link rel="stylesheet" href="_content/BlazorBlueprint.Components/blazorblueprint.css" />

    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

### 4. Add the portal host to your root layout (`MainLayout.razor`):

```razor
<BbPortalHost />
```

This is required for overlay components (Dialog, Sheet, Popover, Tooltip, etc.) to render correctly.

### 5. Start using components:

```razor
<BbButton Variant="ButtonVariant.Default">Click me</BbButton>

<BbDialog>
    <BbDialogTrigger AsChild>
        <BbButton>Open Dialog</BbButton>
    </BbDialogTrigger>
    <BbDialogContent>
        <BbDialogHeader>
            <BbDialogTitle>Welcome to BlazorBlueprint</BbDialogTitle>
            <BbDialogDescription>
                Beautiful Blazor components with zero configuration
            </BbDialogDescription>
        </BbDialogHeader>
        <BbDialogFooter>
            <BbDialogClose AsChild>
                <BbButton Variant="ButtonVariant.Outline">Close</BbButton>
            </BbDialogClose>
        </BbDialogFooter>
    </BbDialogContent>
</BbDialog>
```

That's it! No Tailwind installation, no build configuration needed.

## Available Components

### General

| Component | Description |
|-----------|-------------|
| **Accordion** | Collapsible content sections with smooth animations |
| **Alert** | Contextual feedback messages with variant support |
| **Alert Dialog** | Modal confirmation dialogs requiring user action |
| **Aspect Ratio** | Maintain consistent width-to-height ratios |
| **Avatar** | User profile images with fallback initials and group support |
| **Badge** | Labels for status, categories, and metadata |
| **Breadcrumb** | Navigation breadcrumb trail with separator support |
| **Button** | Interactive buttons with multiple variants and sizes |
| **Button Group** | Grouped button controls with shared styling |
| **Calendar** | Date selection calendar |
| **Card** | Content container with header, content, and footer sections |
| **Carousel** | Scrollable content carousel with navigation controls |
| **Chart** | Data visualization with multiple series types (Bar, Line, Area, Pie, Radar, Radial) |
| **Collapsible** | Expandable content area with trigger control |
| **Empty** | Empty state placeholder for no-content scenarios |
| **Item** | List item container for menus and lists |
| **Kbd** | Keyboard shortcut display |
| **Pagination** | Page navigation controls |
| **Progress** | Progress indicator bar |
| **Resizable** | Resizable panel layout with drag handles |
| **Scroll Area** | Custom scrollable area with styled scrollbars |
| **Separator** | Visual dividers for content sections |
| **Skeleton** | Loading placeholders for content and images |
| **Spinner** | Loading spinner indicator |
| **Split Button** | Button with dropdown action split |
| **Timeline** | Chronological event display |
| **Toggle** | Toggle button control |
| **Typography** | Typography components for consistent text styling |

### Overlays & Navigation

| Component | Description |
|-----------|-------------|
| **Command** | Command palette for quick actions and navigation |
| **Context Menu** | Right-click context menus with items, labels, and shortcuts |
| **Dialog** | Modal dialogs with backdrop and focus management |
| **Drawer** | Slide-out drawer panels with header, footer, and items |
| **Dropdown Menu** | Context menus with items, separators, and shortcuts |
| **Hover Card** | Rich preview cards on hover with delay control |
| **Menubar** | Horizontal menu bar with dropdown menus |
| **Navigation Menu** | Responsive navigation menu with submenus |
| **Popover** | Floating panels for additional content and actions |
| **Responsive Nav** | Mobile-responsive navigation |
| **Sheet** | Side panels that slide in from viewport edges |
| **Sidebar** | Responsive navigation sidebar with collapsible menus |
| **Tabs** | Tabbed interface for organizing related content |
| **Toast** | Toast notification system with action support |
| **Tooltip** | Brief informational popups on hover or focus |

### Form Controls

| Component | Description |
|-----------|-------------|
| **Checkbox** | Binary selection control with indeterminate state |
| **Checkbox Group** | Group of checkboxes with shared state management |
| **Color Picker** | Color selection input |
| **Combobox** | Autocomplete input with searchable dropdown |
| **Currency Input** | Currency-formatted number input |
| **Date Picker** | Date selection input with calendar popup |
| **Date Range Picker** | Date range selection input |
| **Field** | Form field wrapper with label, description, and error states |
| **File Upload** | File upload with drag-and-drop support |
| **Input** | Text input fields with multiple types and sizes |
| **Input Field** | Integrated input with field label and description |
| **Input Group** | Grouped input controls with addons and buttons |
| **Input OTP** | One-time password input with segmented fields |
| **Label** | Accessible labels for form controls |
| **Masked Input** | Input with mask pattern enforcement |
| **Multi Select** | Multi-select dropdown with tag support |
| **Native Select** | Native HTML select element with styling |
| **Numeric Input** | Number input with formatting and validation |
| **Radio Group** | Mutually exclusive options with keyboard navigation |
| **Range Slider** | Dual-handle range slider input |
| **Rating** | Star/icon rating input |
| **Select** | Dropdown selection with groups and labels |
| **Slider** | Single-handle slider input |
| **Switch** | Toggle control for on/off states |
| **Textarea** | Multi-line text input field |
| **Time Picker** | Time selection input |

### Editors

| Component | Description |
|-----------|-------------|
| **Markdown Editor** | Markdown editor with toolbar and live preview |
| **Rich Text Editor** | Rich text editor with formatting toolbar |

### Pre-Built Form Fields

Convenience wrappers that combine a form control with `BbField` for label, description, and error handling:

| Component | Description |
|-----------|-------------|
| **FormFieldCheckbox** | Checkbox with integrated field wrapper |
| **FormFieldCombobox** | Combobox with integrated field wrapper |
| **FormFieldInput** | Input with integrated field wrapper |
| **FormFieldMultiSelect** | MultiSelect with integrated field wrapper |
| **FormFieldRadioGroup** | RadioGroup with integrated field wrapper |
| **FormFieldSelect** | Select with integrated field wrapper |
| **FormFieldSwitch** | Switch with integrated field wrapper |

### Services

| Service | Description |
|---------|-------------|
| `ToastService` | Toast notification state management |
| `DialogService` | Programmatic dialog/confirm control |
| `IPortalService` | Portal management for overlays (from Primitives) |
| `IFocusManager` | Focus trapping and restoration (from Primitives) |
| `IPositioningService` | Floating element positioning (from Primitives) |
| `IKeyboardShortcutService` | Global keyboard shortcut registration (from Primitives) |
| `DropdownManagerService` | Coordinates dropdown mutual exclusivity (from Primitives) |

## Component API Reference

### Button

```razor
<BbButton
    Variant="ButtonVariant.Default"
    Size="ButtonSize.Default"
    Type="ButtonType.Button"
    IconPosition="IconPosition.Start"
    Disabled="false">
    Click me
</BbButton>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Variant` | `ButtonVariant` | `Default` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `Size` | `ButtonSize` | `Default` | `Small`, `Default`, `Large`, `Icon`, `IconSmall`, `IconLarge` |
| `Type` | `ButtonType` | `Button` | `Button`, `Submit`, `Reset` |
| `IconPosition` | `IconPosition` | `Start` | `Start`, `End` |

### Input

```razor
<BbInput
    Type="InputType.Email"
    Placeholder="name@example.com"
    Disabled="false" />
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Type` | `InputType` | `Text` | `Text`, `Email`, `Password`, `Number`, `Tel`, `Url`, `Search`, `Date`, `Time`, `File` |

### Avatar

```razor
<BbAvatar Size="AvatarSize.Default">
    <BbAvatarImage Src="user.jpg" Alt="User" />
    <BbAvatarFallback>JD</BbAvatarFallback>
</BbAvatar>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Size` | `AvatarSize` | `Default` | `Small`, `Default`, `Large`, `ExtraLarge` |

### Badge

```razor
<BbBadge Variant="BadgeVariant.Default">New</BbBadge>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Variant` | `BadgeVariant` | `Default` | `Default`, `Secondary`, `Destructive`, `Outline` |

### Accordion

```razor
<BbAccordion Type="AccordionType.Single" Collapsible="true">
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

### Tabs

```razor
<BbTabs
    DefaultValue="tab1"
    Orientation="TabsOrientation.Horizontal"
    ActivationMode="TabsActivationMode.Automatic">
    <BbTabsList>
        <BbTabsTrigger Value="tab1">Tab 1</BbTabsTrigger>
        <BbTabsTrigger Value="tab2">Tab 2</BbTabsTrigger>
    </BbTabsList>
    <BbTabsContent Value="tab1">Content 1</BbTabsContent>
    <BbTabsContent Value="tab2">Content 2</BbTabsContent>
</BbTabs>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Orientation` | `TabsOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `ActivationMode` | `TabsActivationMode` | `Automatic` | `Automatic` (on focus), `Manual` (on click) |

### Sheet

```razor
<BbSheet>
    <BbSheetTrigger AsChild>
        <BbButton>Open Sheet</BbButton>
    </BbSheetTrigger>
    <BbSheetContent Side="SheetSide.Right">
        <BbSheetHeader>
            <BbSheetTitle>Sheet Title</BbSheetTitle>
            <BbSheetDescription>Sheet description</BbSheetDescription>
        </BbSheetHeader>
        <!-- Content -->
    </BbSheetContent>
</BbSheet>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Side` | `SheetSide` | `Right` | `Top`, `Right`, `Bottom`, `Left` |

### Select

```razor
<BbSelect TValue="string" @bind-Value="selectedValue">
    <BbSelectTrigger>
        <BbSelectValue Placeholder="Select an option" />
    </BbSelectTrigger>
    <BbSelectContent>
        <BbSelectItem Value="@("option1")" Text="Option 1" />
        <BbSelectItem Value="@("option2")" Text="Option 2" />
    </BbSelectContent>
</BbSelect>
```

Select is a generic component. Specify `TValue` for type safety.

### Separator

```razor
<BbSeparator Orientation="SeparatorOrientation.Horizontal" />
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Orientation` | `SeparatorOrientation` | `Horizontal` | `Horizontal`, `Vertical` |

### Skeleton

```razor
<BbSkeleton Shape="SkeletonShape.Rectangular" Class="w-full h-4" />
<BbSkeleton Shape="SkeletonShape.Circular" Class="w-12 h-12" />
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `Shape` | `SkeletonShape` | `Rectangular` | `Rectangular`, `Circular` |

### DataTable

```razor
<BbDataTable TItem="User" Items="users" SelectionMode="DataTableSelectionMode.Multiple">
    <BbDataTableColumn TItem="User" Field="x => x.Name" Header="Name" />
    <BbDataTableColumn TItem="User" Field="x => x.Email" Header="Email" />
</BbDataTable>
```

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `SelectionMode` | `DataTableSelectionMode` | `None` | `None`, `Single`, `Multiple` |

## Theming

BlazorBlueprint is **100% compatible with shadcn/ui themes**. Customize your application's appearance using CSS variables.

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

Reference it in your `App.razor` **before** the BlazorBlueprint CSS:

```razor
<link rel="stylesheet" href="styles/theme.css" />
<link rel="stylesheet" href="_content/BlazorBlueprint.Components/blazorblueprint.css" />
```

### Dark Mode

Dark mode automatically activates when you add the `.dark` class to the `<html>` element. All components will switch to dark mode colors.

## Usage Example

```razor
<BbDialog>
    <BbDialogTrigger AsChild>
        <BbButton>Open Dialog</BbButton>
    </BbDialogTrigger>
    <BbDialogContent>
        <BbDialogHeader>
            <BbDialogTitle>Confirm Action</BbDialogTitle>
            <BbDialogDescription>
                Are you sure you want to proceed?
            </BbDialogDescription>
        </BbDialogHeader>
        <p>This action cannot be undone.</p>
        <BbDialogFooter>
            <BbDialogClose AsChild>
                <BbButton Variant="ButtonVariant.Outline">Cancel</BbButton>
            </BbDialogClose>
            <BbButton Variant="ButtonVariant.Default">Confirm</BbButton>
        </BbDialogFooter>
    </BbDialogContent>
</BbDialog>
```

### AsChild Pattern

Use `AsChild` on trigger components to use your own styled elements instead of the default button:

```razor
<BbDropdownMenu>
    <BbDropdownMenuTrigger AsChild>
        <BbButton Variant="ButtonVariant.Outline">
            Actions
            <BbLucideIcon Name="chevron-down" Size="16" />
        </BbButton>
    </BbDropdownMenuTrigger>
    <BbDropdownMenuContent>
        <BbDropdownMenuItem>Edit</BbDropdownMenuItem>
        <BbDropdownMenuItem>Delete</BbDropdownMenuItem>
    </BbDropdownMenuContent>
</BbDropdownMenu>
```

This is the industry-standard pattern from Radix UI/shadcn/ui. When `AsChild` is true, the child component (e.g., BbButton) automatically receives trigger behavior via `TriggerContext`.

## Form Example

```razor
<div class="space-y-4">
    <div>
        <BbLabel For="email">Email</BbLabel>
        <BbInput Id="email" Type="InputType.Email" Placeholder="name@example.com" />
    </div>

    <div class="flex items-center space-x-2">
        <BbCheckbox Id="terms" @bind-Checked="agreedToTerms" />
        <BbLabel For="terms">I agree to the terms and conditions</BbLabel>
    </div>

    <BbButton Disabled="@(!agreedToTerms)">Submit</BbButton>
</div>

@code {
    private bool agreedToTerms = false;
}
```

## Customizing Components

### Override Default Styles

Use the `Class` parameter to add custom CSS classes or Tailwind classes (if you have Tailwind set up):

```razor
<BbButton Class="bg-purple-600 hover:bg-purple-700">
    Custom Button
</BbButton>

<BbCard Class="border-2 border-purple-500 shadow-xl">
    Custom Card Styling
</BbCard>
```

**Note:** BlazorBlueprint Components include pre-built CSS and don't require Tailwind. However, you can still use Tailwind classes for customization if you've set up Tailwind in your project.

### Component Composition

Build complex UIs by composing components:

```razor
<BbCard>
    <BbCardHeader>
        <BbCardTitle>Settings</BbCardTitle>
        <BbCardDescription>Manage your account settings</BbCardDescription>
    </BbCardHeader>
    <BbCardContent class="space-y-4">
        <div>
            <BbLabel>Email Notifications</BbLabel>
            <BbSwitch @bind-Checked="emailNotifications" />
        </div>
        <BbSeparator />
        <div>
            <BbLabel>Push Notifications</BbLabel>
            <BbSwitch @bind-Checked="pushNotifications" />
        </div>
    </BbCardContent>
    <BbCardFooter>
        <BbButton>Save Changes</BbButton>
    </BbCardFooter>
</BbCard>
```

## Design Philosophy

BlazorBlueprint.Components follows the shadcn/ui philosophy with zero-configuration deployment:

1. **Zero Configuration**: Pre-built CSS included - just install and use
2. **shadcn/ui Compatible**: Uses the same design tokens and CSS variables
3. **Built on Primitives**: All behavior comes from BlazorBlueprint.Primitives
4. **Theme Tokens**: Fully themeable using CSS variables
5. **Built with Accessibility in Mind**: Includes ARIA attributes and keyboard support
6. **Customizable**: Override with custom CSS or add Tailwind if needed

## When to Use

**Use BlazorBlueprint.Components when:**
- Want beautiful defaults with shadcn/ui design
- Need zero-configuration setup (no build tools required)
- Want to ship quickly without building components from scratch
- Need dark mode and theming support out of the box
- Want shadcn/ui theme compatibility

**Consider [BlazorBlueprint.Primitives](https://www.nuget.org/packages/BlazorBlueprint.Primitives) when:**
- Building a completely custom design system
- Want zero opinions about styling
- Need to match a specific brand or design language
- Prefer full control over all CSS

## Documentation

For full documentation, examples, and API reference, visit:
- [Documentation Site](https://blazorblueprintui.com)
- [GitHub Repository](https://github.com/blazorblueprintui/ui)

## Dependencies

- [BlazorBlueprint.Primitives](https://www.nuget.org/packages/BlazorBlueprint.Primitives) - Headless component primitives (auto-installed)
- [BlazorBlueprint.Icons.Lucide](https://www.nuget.org/packages/BlazorBlueprint.Icons.Lucide) - Lucide icon set (auto-installed)
- Pre-built CSS (included in package)
- No external dependencies required!

**Optional:**
- Tailwind CSS (if you want to use Tailwind classes for customization)

## License

Apache License 2.0 - see [LICENSE](https://github.com/blazorblueprintui/ui/blob/main/LICENSE) for details.

## Contributing

Contributions are welcome! Please see our [Contributing Guide](https://github.com/blazorblueprintui/ui/blob/main/CONTRIBUTING.md).
