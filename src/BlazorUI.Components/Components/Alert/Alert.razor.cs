using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Alert;

/// <summary>
/// A callout component that displays a prominent message to users.
/// </summary>
/// <remarks>
/// <para>
/// The Alert component provides a way to display important messages that attract
/// user attention. It follows accessibility guidelines with proper ARIA attributes.
/// </para>
/// <para>
/// Features:
/// - 2 visual variants (Default, Destructive)
/// - Optional icon support
/// - Semantic HTML with role="alert"
/// - Dark mode compatible via CSS variables
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Alert Variant="AlertVariant.Default"&gt;
///     &lt;AlertTitle&gt;Heads up!&lt;/AlertTitle&gt;
///     &lt;AlertDescription&gt;You can add components to your app.&lt;/AlertDescription&gt;
/// &lt;/Alert&gt;
/// </code>
/// </example>
public partial class Alert : ComponentBase
{
    /// <summary>
    /// Gets or sets the visual style variant of the alert.
    /// </summary>
    /// <remarks>
    /// Controls the color scheme and visual appearance using CSS custom properties.
    /// Default value is <see cref="AlertVariant.Default"/>.
    /// </remarks>
    [Parameter]
    public AlertVariant Variant { get; set; } = AlertVariant.Default;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the alert.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the alert.
    /// </summary>
    /// <remarks>
    /// Typically contains AlertTitle and AlertDescription components.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets an optional icon to display in the alert.
    /// </summary>
    /// <remarks>
    /// Can be any RenderFragment (SVG, icon font, image).
    /// Positioned absolutely in the top-left corner.
    /// </remarks>
    [Parameter]
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the alert element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base alert styles
        "relative w-full rounded-lg border p-4",
        Icon != null ? "[&>svg+div]:translate-y-[-3px] [&>svg]:absolute [&>svg]:left-4 [&>svg]:top-4 [&:has(svg)]:pl-11" : null,
        // Variant-specific styles
        Variant switch
        {
            AlertVariant.Default => "bg-background text-foreground [&>svg]:text-foreground",
            AlertVariant.Destructive => "border-destructive/50 text-destructive dark:border-destructive [&>svg]:text-destructive",
            _ => "bg-background text-foreground [&>svg]:text-foreground"
        },
        // Custom classes (if provided)
        Class
    );
}
