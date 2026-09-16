# BlazorBlueprint.Icons.Heroicons

A comprehensive Heroicons icon library for Blazor applications, providing 1,288 beautiful icons across 4 variants (outline, solid, mini, micro).

## Features

- **1,288 Icons**: Complete Heroicons icon set across 4 variants
- **4 Variants**: Outline (24x24), Solid (24x24), Mini (20x20), Micro (16x16)
- **React-Style API**: Familiar component-based API for React developers
- **Includes ARIA Attributes**: Customizable aria-label and aria-hidden support
- **Tree-Shakeable**: Blazor assembly trimming removes unused icons at publish time
- **Type-Safe**: Full XML documentation and IntelliSense support
- **Themeable**: Icons inherit color from parent by default, supports CSS variables
- **Lightweight**: Optimized SVG rendering with minimal overhead

## Installation

```bash
dotnet add package BlazorBlueprint.Icons.Heroicons
```

## Basic Usage

### Import the Namespace

Add to `_Imports.razor`:

```razor
@using BlazorBlueprint.Icons.Heroicons.Components
```

### Render an Icon

```razor
@* Default variant (Outline) *@
<HeroIcon Name="camera" />
```

### Use Different Variants

```razor
@* Outline variant (24x24) - default *@
<HeroIcon Name="camera" Variant="HeroIconVariant.Outline" />

@* Solid variant (24x24) *@
<HeroIcon Name="camera" Variant="HeroIconVariant.Solid" />

@* Mini variant (20x20) *@
<HeroIcon Name="camera" Variant="HeroIconVariant.Mini" />

@* Micro variant (16x16) *@
<HeroIcon Name="camera" Variant="HeroIconVariant.Micro" />
```

### Customize Size and Color

```razor
<HeroIcon Name="heart" Size="32" Color="red" Variant="HeroIconVariant.Solid" />
```

### Use with CSS Variables (Theming)

```razor
<HeroIcon Name="sun" Color="var(--primary)" />
```

### Icon-Only Button (with Accessibility)

```razor
<button aria-label="Take Photo">
    <HeroIcon Name="camera" />
</button>
```

### Integration with BlazorBlueprint Button Component

```razor
<Button>
    <Icon>
        <HeroIcon Name="camera" Size="20" Variant="HeroIconVariant.Solid" />
    </Icon>
    Take Photo
</Button>
```

## Component API

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Name` | `string` | **(Required)** | Icon name (e.g., "camera", "home", "user"). Case-insensitive. |
| `Variant` | `HeroIconVariant` | `Outline` | Icon variant: Outline (24x24), Solid (24x24), Mini (20x20), or Micro (16x16) |
| `Size` | `int?` | `null` | Icon size in pixels (overrides variant default size) |
| `Color` | `string` | `"currentColor"` | Icon color (CSS color value, inherits from parent by default) |
| `StrokeWidth` | `double?` | `null` | SVG stroke width (for outline variant only) |
| `Class` | `string?` | `null` | Additional CSS classes |
| `AriaLabel` | `string?` | `null` | Accessibility label for screen readers |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Any additional SVG attributes |

### Icon Variants

```csharp
public enum HeroIconVariant
{
    Outline,  // 24x24, stroke-based (default)
    Solid,    // 24x24, filled
    Mini,     // 20x20, filled
    Micro     // 16x16, filled
}
```

### Examples

**Basic Icon (Outline):**
```razor
<HeroIcon Name="home" />
```

**Solid Icon:**
```razor
<HeroIcon Name="star" Variant="HeroIconVariant.Solid" />
```

**Mini Icon with Custom Color:**
```razor
<HeroIcon Name="heart" Variant="HeroIconVariant.Mini" Color="#EF4444" />
```

**Micro Icon with Custom Size:**
```razor
<HeroIcon Name="check" Variant="HeroIconVariant.Micro" Size="20" />
```

**Icon with Custom CSS Classes:**
```razor
<HeroIcon Name="exclamation-triangle" Class="text-destructive hover:text-destructive/80" />
```

**Accessible Icon-Only Button:**
```razor
<button>
    <HeroIcon Name="trash" Variant="HeroIconVariant.Solid" AriaLabel="Delete item" />
</button>
```

**Icon with Data Attributes:**
```razor
<HeroIcon Name="cog-6-tooth" data-testid="settings-icon" />
```

## Icon Names

All 316 unique Heroicons are available across 4 variants (1,288 total). Icon names use kebab-case and match the official Heroicons naming:

- `academic-cap`
- `arrow-down`
- `arrow-up`
- `bell`
- `bookmark`
- `calendar`
- `camera`
- `check`
- `chevron-down`
- `chevron-up`
- `heart`
- `home`
- `user`
- `x-mark`
- ... and 300+ more

**Browse all icons:** [heroicons.com](https://heroicons.com/)

## Variant Guidelines

### Outline (Default)
- **Size**: 24x24
- **Style**: Stroke-based, 1.5px stroke
- **Use case**: General UI, navigation, buttons

### Solid
- **Size**: 24x24
- **Style**: Filled paths
- **Use case**: Active states, emphasis, decorative

### Mini
- **Size**: 20x20
- **Style**: Filled paths
- **Use case**: Compact UI, inline with text, badges

### Micro
- **Size**: 16x16
- **Style**: Filled paths, simplified details
- **Use case**: Very small UI elements, dense layouts

## Styling

### Default Behavior

Icons inherit `color` from their parent element by default:

```razor
<div style="color: blue;">
    <HeroIcon Name="home" /> <!-- Will be blue -->
</div>
```

### Explicit Color

Override the inherited color:

```razor
<HeroIcon Name="home" Color="red" />
```

### CSS Variables (Theming)

Perfect for theme systems:

```razor
<HeroIcon Name="sun" Color="var(--primary)" />
<HeroIcon Name="moon" Color="var(--foreground)" />
```

### Tailwind CSS

Use Tailwind utility classes (Heroicons are designed for Tailwind):

```razor
<HeroIcon Name="check-circle" Class="text-green-500 dark:text-green-400" Variant="HeroIconVariant.Solid" />
```

## Accessibility

### Decorative Icons (Next to Text)

Icons next to text are decorative and don't need labels:

```razor
<button>
    <HeroIcon Name="camera" />
    <span>Take Photo</span>
</button>
```

### Semantic Icons (Icon-Only)

Icon-only elements require `AriaLabel`:

```razor
<button>
    <HeroIcon Name="camera" AriaLabel="Take Photo" />
</button>
```

## Performance

- **Bundle Size**: ~450 KB for complete icon set with all variants (before compression)
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

Heroicons are MIT licensed by [Tailwind Labs](https://github.com/tailwindlabs/heroicons).

## Links

- **Heroicons**: [heroicons.com](https://heroicons.com/)
- **BlazorBlueprint**: [GitHub Repository](https://github.com/blazorblueprintui/ui)
- **Issues**: [Report a Bug](https://github.com/blazorblueprintui/ui/issues)

## Contributing

Contributions are welcome! Please open an issue or pull request on GitHub.

---

Made with ❤️ by the BlazorBlueprint team
