namespace BlazorUI.Components.Alert;

/// <summary>
/// Defines the visual style variant for an Alert component.
/// </summary>
/// <remarks>
/// Alert variants use CSS custom properties for theming.
/// Each variant conveys different meaning or urgency levels.
/// </remarks>
public enum AlertVariant
{
    /// <summary>
    /// Default alert style with standard background and border.
    /// Uses --background and --foreground CSS variables.
    /// Suitable for general informational messages.
    /// </summary>
    Default,

    /// <summary>
    /// Destructive alert style for warnings or errors.
    /// Uses --destructive and --destructive-foreground CSS variables.
    /// Indicates critical status or requires user attention.
    /// </summary>
    Destructive
}
