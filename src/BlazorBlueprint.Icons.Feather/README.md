# BlazorBlueprint.Icons.Feather

A comprehensive Feather icon library for Blazor applications, providing 286 beautiful, minimalist SVG icons.

## Features

- **286 Icons**: Complete Feather icon set
- **Minimalist Design**: Simple, beautiful stroke-based icons
- **React-Style API**: Familiar component-based API for React developers
- **Includes ARIA Attributes**: Customizable aria-label and aria-hidden support
- **Tree-Shakeable**: Blazor assembly trimming removes unused icons at publish time
- **Type-Safe**: Full XML documentation and IntelliSense support
- **Themeable**: Icons inherit color from parent by default, supports CSS variables
- **Lightweight**: Optimized SVG rendering with minimal overhead

## Installation

```bash
dotnet add package BlazorBlueprint.Icons.Feather
```

## Basic Usage

### Import the Namespace

Add to `_Imports.razor`:

```razor
@using BlazorBlueprint.Icons.Feather.Components
```

### Render an Icon

```razor
<FeatherIcon Name="camera" />
```

### Customize Size and Color

```razor
<FeatherIcon Name="heart" Size="32" Color="red" />
```

### Use with CSS Variables (Theming)

```razor
<FeatherIcon Name="sun" Color="var(--primary)" />
```

### Icon-Only Button (with Accessibility)

```razor
<button aria-label="Take Photo">
    <FeatherIcon Name="camera" />
</button>
```

### Integration with BlazorBlueprint Button Component

```razor
<Button>
    <Icon>
        <FeatherIcon Name="camera" Size="20" />
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
| `StrokeWidth` | `double` | `2.0` | SVG stroke width (Feather icons designed with 2px strokes) |
| `Class` | `string?` | `null` | Additional CSS classes |
| `AriaLabel` | `string?` | `null` | Accessibility label for screen readers |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Any additional SVG attributes |

### Examples

**Basic Icon:**
```razor
<FeatherIcon Name="home" />
```

**Large Icon with Custom Color:**
```razor
<FeatherIcon Name="star" Size="48" Color="#FFD700" />
```

**Icon with Custom Stroke Width:**
```razor
<FeatherIcon Name="circle" StrokeWidth="1.5" />
```

**Icon with Custom CSS Classes:**
```razor
<FeatherIcon Name="alert-circle" Class="text-destructive hover:text-destructive/80" />
```

**Accessible Icon-Only Button:**
```razor
<button>
    <FeatherIcon Name="trash" AriaLabel="Delete item" />
</button>
```

**Icon with Data Attributes:**
```razor
<FeatherIcon Name="settings" data-testid="settings-icon" />
```

## Icon Names

All 286 Feather icons are available. Icon names use kebab-case and match the official Feather icon names:

- `activity`
- `airplay`
- `alert-circle`
- `alert-triangle`
- `archive`
- `bell`
- `bookmark`
- `calendar`
- `camera`
- `check`
- `check-circle`
- `chevron-down`
- `chevron-up`
- `circle`
- `heart`
- `home`
- `mail`
- `settings`
- `star`
- `trash`
- `user`
- `x`
- ... and 260+ more

**Browse all icons:** [feathericons.com](https://feathericons.com/)

## Styling

### Default Behavior

Icons inherit `color` from their parent element by default:

```razor
<div style="color: blue;">
    <FeatherIcon Name="home" /> <!-- Will be blue -->
</div>
```

### Explicit Color

Override the inherited color:

```razor
<FeatherIcon Name="home" Color="red" />
```

### CSS Variables (Theming)

Perfect for theme systems:

```razor
<FeatherIcon Name="sun" Color="var(--primary)" />
<FeatherIcon Name="moon" Color="var(--foreground)" />
```

### Tailwind CSS

Use Tailwind utility classes:

```razor
<FeatherIcon Name="check" Class="text-green-500 dark:text-green-400" />
```

## Accessibility

### Decorative Icons (Next to Text)

Icons next to text are decorative and don't need labels:

```razor
<button>
    <FeatherIcon Name="camera" />
    <span>Take Photo</span>
</button>
```

### Semantic Icons (Icon-Only)

Icon-only elements require `AriaLabel`:

```razor
<button>
    <FeatherIcon Name="camera" AriaLabel="Take Photo" />
</button>
```

## Performance

- **Bundle Size**: ~70 KB for complete icon set (before compression)
- **Brotli Compression**: Reduces size by ~70% in production
- **Assembly Trimming**: Unused icons automatically removed at publish time
- **Static Dictionary**: O(1) icon lookup with minimal memory overhead

## Browser Support

Works in all modern browsers that support:
- Blazor Server / WebAssembly / Hybrid
- SVG rendering
- CSS `currentColor`

## License

The C# wrapper code is MIT licensed; see the included `LICENSE` file. The package also includes the upstream icon license in `THIRD-PARTY-NOTICES.txt`.

Feather icons are MIT licensed by [Cole Bemis](https://github.com/feathericons/feather).

## Links

- **Feather Icons**: [feathericons.com](https://feathericons.com/)
- **BlazorBlueprint**: [GitHub Repository](https://github.com/blazorblueprintui/ui)
- **Issues**: [Report a Bug](https://github.com/blazorblueprintui/ui/issues)

## Contributing

Contributions are welcome! Please open an issue or pull request on GitHub.

---

Made with ❤️ by the BlazorBlueprint team
