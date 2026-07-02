namespace BlazorBlueprint.Components;

/// <summary>
/// The base (gray scale) color palette used for backgrounds, borders, and neutral UI elements.
/// Each value maps to a distinct set of OKLCH gray-scale CSS custom properties.
/// </summary>
public enum BaseColor
{
    /// <summary>Cool gray with a subtle blue undertone. Default for shadcn/ui.</summary>
    Zinc,

    /// <summary>Cool blue-gray, slightly warmer than Zinc.</summary>
    Slate,

    /// <summary>Pure, balanced gray with no color tint.</summary>
    Gray,

    /// <summary>True neutral gray — identical lightness steps with zero chroma.</summary>
    Neutral,

    /// <summary>Warm gray with a slight yellow/brown undertone.</summary>
    Stone
}
