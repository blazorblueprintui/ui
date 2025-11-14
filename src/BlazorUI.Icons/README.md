# BlazorUI.Icons

A comprehensive Lucide icon library for Blazor applications, providing 1,600+ beautiful, consistent SVG icons.

## Features

- **1,640+ Icons**: Complete Lucide icon set
- **React-Style API**: Familiar component-based API for React developers
- **Fully Accessible**: ARIA-compliant with customizable labels
- **Tree-Shakeable**: Blazor assembly trimming removes unused icons at publish time
- **Type-Safe**: Full XML documentation and IntelliSense support
- **Themeable**: Icons inherit color from parent by default, supports CSS variables
- **Lightweight**: Optimized SVG rendering with minimal overhead

## Installation

```bash
dotnet add package BlazorUI.Icons
```

## Basic Usage

### Import the Namespace

Add to `_Imports.razor`:

```razor
@using BlazorUI.Icons.Components
```

### Render an Icon

```razor
<LucideIcon Name="camera" />
```

### Customize Size and Color

```razor
<LucideIcon Name="heart" Size="32" Color="red" />
```

### Use with CSS Variables (Theming)

```razor
<LucideIcon Name="sun" Color="var(--primary)" />
```

### Icon-Only Button (with Accessibility)

```razor
<button aria-label="Take Photo">
    <LucideIcon Name="camera" />
</button>
```

### Integration with BlazorUI Button Component

```razor
<Button>
    <Icon>
        <LucideIcon Name="camera" Size="20" />
    </Icon>
    Take Photo
</Button>
```

## Component API

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Name` | `string` | **(Required)** | Icon name (e.g., "camera", "home", "user"). Case-insensitive. |
| `Size` | `int` | `24` | Icon size in pixels |
| `Color` | `string` | `"currentColor"` | Icon color (CSS color value, inherits from parent by default) |
| `StrokeWidth` | `double` | `2.0` | SVG stroke width (Lucide icons designed with 2px strokes) |
| `Class` | `string?` | `null` | Additional CSS classes |
| `AriaLabel` | `string?` | `null` | Accessibility label for screen readers |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Any additional SVG attributes |

### Examples

**Basic Icon:**
```razor
<LucideIcon Name="home" />
```

**Large Icon with Custom Color:**
```razor
<LucideIcon Name="star" Size="48" Color="#FFD700" />
```

**Icon with Custom Stroke Width:**
```razor
<LucideIcon Name="circle" StrokeWidth="1.5" />
```

**Icon with Custom CSS Classes:**
```razor
<LucideIcon Name="alert-circle" Class="text-destructive hover:text-destructive/80" />
```

**Accessible Icon-Only Button:**
```razor
<button>
    <LucideIcon Name="trash" AriaLabel="Delete item" />
</button>
```

**Icon with Data Attributes:**
```razor
<LucideIcon Name="settings" data-testid="settings-icon" />
```

## Icon Names

All 1,640+ Lucide icons are available. Icon names use kebab-case and match the official Lucide icon names:

- `camera`
- `home`
- `user`
- `settings`
- `heart`
- `star`
- `check`
- `x`
- `chevron-down`
- `chevron-up`
- `alert-circle`
- `info`
- ... and 1,600+ more

**Browse all icons:** [lucide.dev/icons](https://lucide.dev/icons/)

## Styling

### Default Behavior

Icons inherit `color` from their parent element by default:

```razor
<div style="color: blue;">
    <LucideIcon Name="home" /> <!-- Will be blue -->
</div>
```

### Explicit Color

Override the inherited color:

```razor
<LucideIcon Name="home" Color="red" />
```

### CSS Variables (Theming)

Perfect for theme systems:

```razor
<LucideIcon Name="sun" Color="var(--primary)" />
<LucideIcon Name="moon" Color="var(--foreground)" />
```

### Tailwind CSS

Use Tailwind utility classes:

```razor
<LucideIcon Name="check" Class="text-green-500 dark:text-green-400" />
```

## Accessibility

### Decorative Icons (Next to Text)

Icons next to text are decorative and don't need labels:

```razor
<button>
    <LucideIcon Name="camera" />
    <span>Take Photo</span>
</button>
```

### Semantic Icons (Icon-Only)

Icon-only elements require `AriaLabel`:

```razor
<button>
    <LucideIcon Name="camera" AriaLabel="Take Photo" />
</button>
```

## Performance

- **Bundle Size**: ~300 KB for complete icon set (before compression)
- **Brotli Compression**: Reduces size by ~70% in production
- **Assembly Trimming**: Unused icons automatically removed at publish time
- **Static Dictionary**: O(1) icon lookup with minimal memory overhead

## Browser Support

Works in all modern browsers that support:
- Blazor Server / WebAssembly / Hybrid
- SVG rendering
- CSS `currentColor`

## License

This package is MIT licensed.

Lucide icons are ISC licensed by [Lucide Contributors](https://github.com/lucide-icons/lucide).

## Links

- **Lucide Icons**: [lucide.dev](https://lucide.dev/)
- **BlazorUI**: [GitHub Repository](https://github.com/blazorui-net/ui)
- **Issues**: [Report a Bug](https://github.com/blazorui-net/ui/issues)

## Contributing

Contributions are welcome! Please open an issue or pull request on GitHub.

---

Made with ❤️ by the BlazorUI team
