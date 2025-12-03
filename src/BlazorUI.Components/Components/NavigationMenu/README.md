# NavigationMenu Component

Horizontal navigation bar with dropdown panels for app and marketing site navigation.

## Features

- **Dropdown Panels**: Triggers open content panels on hover/click
- **Keyboard Navigation**: Arrow keys, Escape support
- **ARIA Support**: Proper menu roles and states
- **Flexible Layout**: Supports various content layouts

## Usage

### Basic Navigation Menu

```razor
@using BlazorUI.Components.NavigationMenu

<NavigationMenu>
    <NavigationMenuList>
        <NavigationMenuItem Value="products">
            <NavigationMenuTrigger>Products</NavigationMenuTrigger>
            <NavigationMenuContent>
                <ul class="grid gap-3 p-4 md:w-[400px] lg:w-[500px] lg:grid-cols-[.75fr_1fr]">
                    <li>
                        <NavigationMenuLink Href="/products/overview">
                            <div class="text-sm font-medium leading-none">Overview</div>
                            <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                                Learn about our products
                            </p>
                        </NavigationMenuLink>
                    </li>
                    <li>
                        <NavigationMenuLink Href="/products/features">
                            <div class="text-sm font-medium leading-none">Features</div>
                            <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                                Explore all features
                            </p>
                        </NavigationMenuLink>
                    </li>
                </ul>
            </NavigationMenuContent>
        </NavigationMenuItem>

        <NavigationMenuItem Value="resources">
            <NavigationMenuTrigger>Resources</NavigationMenuTrigger>
            <NavigationMenuContent>
                <ul class="grid w-[400px] gap-3 p-4 md:grid-cols-2">
                    <li>
                        <NavigationMenuLink Href="/docs">Documentation</NavigationMenuLink>
                    </li>
                    <li>
                        <NavigationMenuLink Href="/blog">Blog</NavigationMenuLink>
                    </li>
                    <li>
                        <NavigationMenuLink Href="/tutorials">Tutorials</NavigationMenuLink>
                    </li>
                </ul>
            </NavigationMenuContent>
        </NavigationMenuItem>

        <NavigationMenuItem>
            <NavigationMenuLink Href="/pricing" Class="h-10 px-4 py-2">
                Pricing
            </NavigationMenuLink>
        </NavigationMenuItem>
    </NavigationMenuList>
</NavigationMenu>
```

### With Featured Item

```razor
<NavigationMenu>
    <NavigationMenuList>
        <NavigationMenuItem Value="getting-started">
            <NavigationMenuTrigger>Getting Started</NavigationMenuTrigger>
            <NavigationMenuContent>
                <ul class="grid gap-3 p-6 md:w-[400px] lg:w-[500px] lg:grid-cols-[.75fr_1fr]">
                    <li class="row-span-3">
                        <NavigationMenuLink Href="/" Class="flex h-full w-full select-none flex-col justify-end rounded-md bg-gradient-to-b from-muted/50 to-muted p-6 no-underline outline-none focus:shadow-md">
                            <div class="mb-2 mt-4 text-lg font-medium">
                                shadcn/ui
                            </div>
                            <p class="text-sm leading-tight text-muted-foreground">
                                Beautifully designed components for Blazor.
                            </p>
                        </NavigationMenuLink>
                    </li>
                    <li>
                        <NavigationMenuLink Href="/docs">
                            <div class="text-sm font-medium leading-none">Introduction</div>
                            <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                                Get started with the basics
                            </p>
                        </NavigationMenuLink>
                    </li>
                    <li>
                        <NavigationMenuLink Href="/docs/installation">
                            <div class="text-sm font-medium leading-none">Installation</div>
                            <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                                How to install and configure
                            </p>
                        </NavigationMenuLink>
                    </li>
                </ul>
            </NavigationMenuContent>
        </NavigationMenuItem>
    </NavigationMenuList>
</NavigationMenu>
```

## API Reference

### NavigationMenu Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The menu content |
| `Value` | `string?` | `null` | Active item value (controlled) |
| `ValueChanged` | `EventCallback<string>` | - | Called when active item changes |
| `Orientation` | `NavigationMenuOrientation` | `Horizontal` | Menu orientation |
| `AriaLabel` | `string` | `"Main"` | Accessible label |
| `Class` | `string?` | `null` | Additional CSS classes |

### NavigationMenuItem Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `string` | Auto-generated | Unique item identifier |
| `ChildContent` | `RenderFragment?` | `null` | Item content |

### NavigationMenuTrigger Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Trigger content |
| `Class` | `string?` | `null` | Additional CSS classes |

### NavigationMenuContent Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Panel content |
| `Class` | `string?` | `null` | Additional CSS classes |

### NavigationMenuLink Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Href` | `string` | `"#"` | Link URL |
| `Active` | `bool` | `false` | Whether link is active |
| `ChildContent` | `RenderFragment?` | `null` | Link content |
| `OnClick` | `EventCallback` | - | Click callback |
| `Class` | `string?` | `null` | Additional CSS classes |

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `ArrowRight` | Next item (horizontal) |
| `ArrowLeft` | Previous item (horizontal) |
| `ArrowDown` | Open menu / Next item (vertical) |
| `ArrowUp` | Previous item (vertical) |
| `Enter/Space` | Toggle menu |
| `Escape` | Close menu |
