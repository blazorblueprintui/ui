namespace BlazorUI.Components.Button;

/// <summary>
/// Defines the visual style variant for a Button component.
/// </summary>
/// <remarks>
/// Button variants follow the shadcn/ui design system and use CSS custom properties
/// for theming. Each variant applies different background, text, and hover styles
/// while maintaining accessibility standards (WCAG 2.1 AA).
/// </remarks>
public enum ButtonVariant
{
    /// <summary>
    /// Default primary button style with solid background.
    /// Uses --primary and --primary-foreground CSS variables.
    /// Recommended for primary actions (submit, save, confirm).
    /// </summary>
    Default,

    /// <summary>
    /// Destructive action button style (delete, remove, cancel).
    /// Uses --destructive and --destructive-foreground CSS variables.
    /// Indicates potentially dangerous or irreversible actions.
    /// </summary>
    Destructive,

    /// <summary>
    /// Outlined button style with transparent background and border.
    /// Uses --border and --input CSS variables.
    /// Suitable for secondary actions or form controls.
    /// </summary>
    Outline,

    /// <summary>
    /// Secondary button style with muted background.
    /// Uses --secondary and --secondary-foreground CSS variables.
    /// For alternative actions or less prominent CTAs.
    /// </summary>
    Secondary,

    /// <summary>
    /// Ghost button style with no background or border (minimal).
    /// Only shows background on hover using --accent CSS variable.
    /// Ideal for tertiary actions, toolbars, or navigation.
    /// </summary>
    Ghost,

    /// <summary>
    /// Link-styled button that appears as underlined text.
    /// Uses --primary CSS variable for text color.
    /// Suitable for inline actions or navigation that should look like links.
    /// </summary>
    Link
}
