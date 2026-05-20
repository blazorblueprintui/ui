# BlazorBlueprint.Icons.FontAwesome

Font Awesome Free icon library for Blazor applications, providing icons across 3 variants (Solid, Regular, Brands).

> Only Font Awesome **Free** is supported. Font Awesome Pro requires a commercial license and cannot be redistributed via NuGet.

## Features

- **Three Variants**: Solid, Regular (outline), Brands (third-party logos)
- **Aspect-Ratio Aware**: Per-icon `viewBox` is baked in, so non-square Brands icons (e.g. `github`) render at their correct proportions
- **Includes ARIA Attributes**: Customizable `aria-label`
- **Tree-Shakeable**: Blazor assembly trimming removes unused icons at publish time
- **Type-Safe**: Full XML documentation and IntelliSense support
- **Themeable**: Icons inherit color from parent by default, supports CSS variables

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
<FontAwesomeIcon Name="user" Variant="FontAwesomeIconVariant.Solid" />
<FontAwesomeIcon Name="user" Variant="FontAwesomeIconVariant.Regular" />
<FontAwesomeIcon Name="github" Variant="FontAwesomeIconVariant.Brands" />
```

### Customize Size and Color

```razor
<FontAwesomeIcon Name="heart" Size="32" Color="red" Variant="FontAwesomeIconVariant.Solid" />
```

## Regenerating Icon Data

The `Data/FontAwesomeIconData.cs` file is auto-generated. To refresh it:

1. Download the latest Iconify JSON sets for Font Awesome 6 Free:
   - `fa6-solid.json`
   - `fa6-regular.json`
   - `fa6-brands.json`
   (from the `@iconify-json/fa6-*` npm packages, or
   `https://github.com/iconify/icon-sets/tree/master/json`)
2. Place them in `tools/icon-generation/data/`.
3. Run:

```powershell
./GenerateIconData.ps1
```

## License

The C# wrapper code is MIT licensed. Font Awesome Free icon artwork is licensed under the [Font Awesome Free License](https://fontawesome.com/license/free) (CC BY 4.0 for icons, SIL OFL 1.1 for fonts, MIT for code).
