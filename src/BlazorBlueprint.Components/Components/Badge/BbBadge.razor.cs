using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

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
public partial class BbBadge : ComponentBase
{
    /// <summary>
    /// Gets or sets the visual style variant of the badge.
    /// </summary>
    [Parameter]
    public BadgeVariant Variant { get; set; } = BadgeVariant.Default;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the badge.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the badge.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether to show a notification dot on the badge.
    /// </summary>
    [Parameter]
    public bool ShowDot { get; set; }

    /// <summary>
    /// Gets or sets the position of the notification dot.
    /// </summary>
    [Parameter]
    public BadgeDotPosition DotPosition { get; set; } = BadgeDotPosition.TopRight;

    /// <summary>
    /// Gets or sets custom CSS classes for the dot indicator.
    /// Defaults to the variant's primary color when not specified.
    /// </summary>
    [Parameter]
    public string? DotClass { get; set; }

    private string CssClass => ClassNames.cn(
        "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold",
        "transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2",
        ShowDot ? "relative" : null,
        Variant switch
        {
            BadgeVariant.Default => "border-transparent bg-primary text-primary-foreground hover:bg-primary/80",
            BadgeVariant.Secondary => "border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80",
            BadgeVariant.Destructive => "border-transparent bg-destructive text-destructive-foreground hover:bg-destructive/80",
            BadgeVariant.Outline => "text-foreground",
            _ => "border-transparent bg-primary text-primary-foreground hover:bg-primary/80"
        },
        Class
    );

    private string DotPositionClass => DotPosition switch
    {
        BadgeDotPosition.TopRight => "-top-1 -right-1",
        BadgeDotPosition.TopLeft => "-top-1 -left-1",
        BadgeDotPosition.BottomRight => "-bottom-1 -right-1",
        BadgeDotPosition.BottomLeft => "-bottom-1 -left-1",
        _ => "-top-1 -right-1"
    };

    private string DotCssClass => ClassNames.cn(
        "absolute block h-2 w-2 rounded-full ring-2 ring-background",
        DotPositionClass,
        DotClass ?? "bg-primary"
    );
}
