# BlazorBlueprint.Icons.FontAwesome

A comprehensive Font Awesome Free icon library for Blazor applications, providing 2,066 icons across 3 variants (Solid, Regular, Brands).

> Only Font Awesome **Free** is supported. Font Awesome Pro requires a commercial license and cannot be redistributed via NuGet.

## Features

- **2,066 Icons**: Full Font Awesome 6 Free icon set across 3 variants
- **3 Variants**: Solid (filled), Regular (outline), Brands (third-party logos)
- **Aspect-Ratio Aware**: Per-icon `viewBox` is preserved, so non-square Brands icons (e.g. `github`, `twitter`) render at their correct proportions
- **React-Style API**: Familiar component-based API
- **Includes ARIA Attributes**: Customizable `aria-label` for accessibility
- **Tree-Shakeable**: Blazor assembly trimming removes unused icons at publish time
- **Type-Safe**: Full XML documentation and IntelliSense support
- **Themeable**: Icons inherit color from parent by default, supports CSS variables
- **Lightweight**: Static dictionary lookup with minimal overhead

## Installation

```bash
dotnet add package BlazorBlueprint.Icons.FontAwesome
```

## Basic Usage

### Import the Namespace

Add to `_Imports.razor`:

```razor
@using BlazorBlueprint.Icons.FontAwesome.Components
@using BlazorBlueprint.Icons.FontAwesome.Data
```

### Render an Icon

```razor
@* Default variant (Solid) *@
<FontAwesomeIcon Name="camera" />
```

### Use Different Variants

```razor
@* Solid variant — the largest set, filled glyphs (default) *@
<FontAwesomeIcon Name="house" Variant="FontAwesomeIconVariant.Solid" />

@* Regular variant — outline alternative (small curated subset in the Free tier) *@
<FontAwesomeIcon Name="heart" Variant="FontAwesomeIconVariant.Regular" />

@* Brands variant — third-party logos *@
<FontAwesomeIcon Name="github" Variant="FontAwesomeIconVariant.Brands" />
```

### Customize Size and Color

```razor
<FontAwesomeIcon Name="heart" Size="32" Color="red" Variant="FontAwesomeIconVariant.Solid" />
```

### Use with CSS Variables (Theming)

```razor
<FontAwesomeIcon Name="sun" Color="var(--primary)" />
```

### Icon-Only Button (with Accessibility)

```razor
<button aria-label="Take Photo">
    <FontAwesomeIcon Name="camera" />
</button>
```

### Integration with BlazorBlueprint Button Component

```razor
<BbButton>
    <Icon>
        <FontAwesomeIcon Name="download" Variant="FontAwesomeIconVariant.Solid" Size="16" />
    </Icon>
    <ChildContent>Download</ChildContent>
</BbButton>
```

## Component API

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Name` | `string` | **(Required)** | Icon name (e.g., "camera", "house", "github"). Case-insensitive, kebab-case. |
| `Variant` | `FontAwesomeIconVariant` | `Solid` | Icon variant: Solid, Regular, or Brands |
| `Size` | `int?` | `16` | Icon width in pixels. Height is scaled proportionally to preserve aspect ratio. |
| `Color` | `string` | `"currentColor"` | Icon color (any CSS color value, inherits from parent by default) |
| `Class` | `string?` | `null` | Additional CSS classes |
| `AriaLabel` | `string?` | `null` | Accessibility label for screen readers |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Any additional SVG attributes |

### Icon Variants

```csharp
public enum FontAwesomeIconVariant
{
    Solid,    // Filled glyphs — the largest set and the default
    Regular,  // Outline glyphs — small curated subset in the Free tier
    Brands    // Third-party logos (GitHub, Microsoft, Apple, etc.)
}
```

### Examples

**Basic Icon (Solid):**
```razor
<FontAwesomeIcon Name="house" />
```

**Brands Icon:**
```razor
<FontAwesomeIcon Name="github" Variant="FontAwesomeIconVariant.Brands" />
```

**Regular Icon with Custom Color:**
```razor
<FontAwesomeIcon Name="heart" Variant="FontAwesomeIconVariant.Regular" Color="#EF4444" />
```

**Custom Size:**
```razor
<FontAwesomeIcon Name="star" Size="32" />
```

**Icon with Custom CSS Classes:**
```razor
<FontAwesomeIcon Name="triangle-exclamation" Class="text-destructive hover:text-destructive/80" />
```

**Accessible Icon-Only Button:**
```razor
<button>
    <FontAwesomeIcon Name="trash" AriaLabel="Delete item" />
</button>
```

**Icon with Data Attributes:**
```razor
<FontAwesomeIcon Name="gear" data-testid="settings-icon" />
```

## Icon Names

All Font Awesome Free icons are available, with names matching the official Font Awesome naming (kebab-case). Common examples:

- `house`, `user`, `gear`, `magnifying-glass`
- `arrow-left`, `arrow-right`, `arrow-up`, `arrow-down`
- `circle-check`, `circle-xmark`, `circle-exclamation`
- `heart`, `star`, `bell`, `bookmark`
- `github`, `microsoft`, `apple`, `google` (Brands)
- ... and 2,000+ more

**Browse all icons:** [fontawesome.com/icons](https://fontawesome.com/icons)

## Variant Guidelines

### Solid (Default)
- **Style**: Filled paths
- **Coverage**: 1,400+ icons — the largest set in Free
- **Use case**: Primary UI, navigation, emphasis, the default for most applications

### Regular
- **Style**: Outline / stroke-style paths
- **Coverage**: 160+ icons — a small curated subset; the Free tier ships far fewer Regular icons than Solid
- **Use case**: When you want a lighter visual weight than Solid

### Brands
- **Style**: Filled logos at the artist-specified aspect ratio
- **Coverage**: 480+ third-party brand and product logos
- **Use case**: Social links, technology logos, payment provider icons
- **Note**: Brands icons are **not all square** — width and height vary per icon. The component preserves each icon's intrinsic viewBox and scales height accordingly.

## Styling

### Default Behavior

Icons inherit `color` from their parent element by default:

```razor
<div style="color: blue;">
    <FontAwesomeIcon Name="house" /> <!-- Will be blue -->
</div>
```

### Explicit Color

Override the inherited color:

```razor
<FontAwesomeIcon Name="house" Color="red" />
```

### CSS Variables (Theming)

Perfect for theme systems:

```razor
<FontAwesomeIcon Name="sun" Color="var(--primary)" />
<FontAwesomeIcon Name="moon" Color="var(--foreground)" />
```

### Tailwind CSS

Use Tailwind utility classes:

```razor
<FontAwesomeIcon Name="circle-check" Class="text-green-500 dark:text-green-400" />
```

## Accessibility

### Decorative Icons (Next to Text)

Icons next to text are decorative and don't need labels:

```razor
<button>
    <FontAwesomeIcon Name="camera" />
    <span>Take Photo</span>
</button>
```

### Semantic Icons (Icon-Only)

Icon-only elements require `AriaLabel`:

```razor
<button>
    <FontAwesomeIcon Name="camera" AriaLabel="Take Photo" />
</button>
```

## Performance

- **Bundle Size**: ~580 KB for the complete icon set across all 3 variants (before compression)
- **Brotli Compression**: Reduces size by ~70% in production
- **Assembly Trimming**: Unused icons automatically removed at publish time
- **Static Dictionary**: O(1) icon lookup with minimal memory overhead

## Browser Support

Works in all modern browsers that support:
- Blazor Server / WebAssembly / Hybrid
- SVG rendering
- CSS `currentColor`

## Regenerating Icon Data

The `Data/FontAwesomeIconData.cs` file is auto-generated from the Iconify JSON sets for Font Awesome 6 Free. To refresh:

1. Download the latest sets from the `@iconify-json/fa6-*` npm packages, or [iconify/icon-sets](https://github.com/iconify/icon-sets/tree/master/json):
   - `fa6-solid.json`
   - `fa6-regular.json`
   - `fa6-brands.json`
2. Place them in `tools/icon-generation/data/`.
3. Run:

```powershell
./GenerateIconData.ps1
```

## License

The C# wrapper code is MIT licensed.

Font Awesome Free icon artwork is licensed under the [Font Awesome Free License](https://fontawesome.com/license/free):
- Icons: CC BY 4.0
- Fonts: SIL OFL 1.1
- Code: MIT

## Links

- **Font Awesome**: [fontawesome.com](https://fontawesome.com/)
- **Icon Browser**: [fontawesome.com/icons](https://fontawesome.com/icons)
- **BlazorBlueprint**: [GitHub Repository](https://github.com/blazorblueprintui/ui)
- **Issues**: [Report a Bug](https://github.com/blazorblueprintui/ui/issues)

## Contributing

Contributions are welcome! Please open an issue or pull request on GitHub.

---

Made with ❤️ by the BlazorBlueprint team
