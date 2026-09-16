namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the visual style variant for a Badge component.
/// </summary>
/// <remarks>
/// Badge variants follow the shadcn/ui design system and use CSS custom properties
/// for theming. Each variant conveys different meaning or urgency levels.
/// </remarks>
public enum BadgeVariant
{
    /// <summary>
    /// Default primary badge style with solid background.
    /// Uses --primary and --primary-foreground CSS variables.
    /// Suitable for highlighting important items or new content.
    /// </summary>
    Default,

    /// <summary>
    /// Secondary badge style with muted background.
    /// Uses --secondary and --secondary-foreground CSS variables.
    /// For alternative or less prominent labels.
    /// </summary>
    Secondary,

    /// <summary>
    /// Destructive badge style for warnings or errors.
    /// Uses --destructive and --destructive-foreground CSS variables.
    /// Indicates critical status or requires user attention.
    /// </summary>
    Destructive,

    /// <summary>
    /// Outlined badge style with transparent background and border.
    /// Uses --foreground CSS variable for text.
    /// Minimal style for subtle categorization or tags.
    /// </summary>
    Outline,
    /// <summary>Solid positive status.</summary>
    Success,
    /// <summary>Solid caution status.</summary>
    Warning,
    /// <summary>Solid informational status.</summary>
    Info,
    /// <summary>Soft primary palette.</summary>
    Soft,
    /// <summary>Soft error palette.</summary>
    SoftDestructive,
    /// <summary>Soft positive palette.</summary>
    SoftSuccess,
    /// <summary>Soft caution palette.</summary>
    SoftWarning,
    /// <summary>Soft informational palette.</summary>
    SoftInfo
}
