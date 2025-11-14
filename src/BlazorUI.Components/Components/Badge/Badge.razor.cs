using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Badge;

/// <summary>
/// A badge component that displays a small count or label.
/// </summary>
/// <remarks>
/// <para>
/// The Badge component provides a compact way to display status, notifications counts,
/// or labels. It follows the shadcn/ui design system with multiple visual variants.
/// </para>
/// <para>
/// Features:
/// - 4 visual variants (Default, Secondary, Destructive, Outline)
/// - Compact, inline-friendly design
/// - Accessible with semantic HTML
/// - RTL (Right-to-Left) support
/// - Dark mode compatible via CSS variables
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Badge Variant="BadgeVariant.Default"&gt;New&lt;/Badge&gt;
///
/// &lt;Badge Variant="BadgeVariant.Destructive"&gt;5&lt;/Badge&gt;
/// </code>
/// </example>
public partial class Badge : ComponentBase
{
    /// <summary>
    /// Gets or sets the visual style variant of the badge.
    /// </summary>
    /// <remarks>
    /// Controls the color scheme and visual appearance using CSS custom properties.
    /// Default value is <see cref="BadgeVariant.Default"/>.
    /// </remarks>
    [Parameter]
    public BadgeVariant Variant { get; set; } = BadgeVariant.Default;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the badge.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the badge.
    /// </summary>
    /// <remarks>
    /// Typically contains short text (1-2 words) or a small number.
    /// For accessibility, ensure the content is meaningful.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the badge element.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base badge styles (inline-flex, rounded, border, font)
    /// - Variant-specific classes (colors, backgrounds)
    /// - Custom classes from the Class parameter
    /// Uses the cn() utility for intelligent class merging and Tailwind conflict resolution.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base badge styles (from shadcn/ui)
        "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold",
        "transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2",
        // Variant-specific styles
        Variant switch
        {
            BadgeVariant.Default => "border-transparent bg-primary text-primary-foreground hover:bg-primary/80",
            BadgeVariant.Secondary => "border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80",
            BadgeVariant.Destructive => "border-transparent bg-destructive text-destructive-foreground hover:bg-destructive/80",
            BadgeVariant.Outline => "text-foreground",
            _ => "border-transparent bg-primary text-primary-foreground hover:bg-primary/80"
        },
        // Custom classes (if provided)
        Class
    );
}
