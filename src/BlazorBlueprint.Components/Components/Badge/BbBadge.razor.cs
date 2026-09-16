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
        "bb:inline-flex bb:items-center bb:rounded-full bb:border bb:px-2.5 bb:py-0.5 bb:text-xs bb:font-semibold",
        "bb:transition-colors bb:focus:outline-none bb:focus:ring-2 bb:focus:ring-ring bb:focus:ring-offset-2",
        ShowDot ? "bb:relative" : null,
        Variant switch
        {
            BadgeVariant.Default => "bb:border-transparent bb:bg-primary bb:text-primary-foreground bb:hover:bg-primary/80",
            BadgeVariant.Secondary => "bb:border-transparent bb:bg-secondary bb:text-secondary-foreground bb:hover:bg-secondary/80",
            BadgeVariant.Destructive => "bb:border-transparent bb:bg-destructive bb:text-destructive-foreground bb:hover:bg-destructive/80",
            BadgeVariant.Outline => "bb:text-foreground",
            _ => "bb:border-transparent bb:bg-primary bb:text-primary-foreground bb:hover:bg-primary/80"
        },
        Class
    );

    private string DotPositionClass => DotPosition switch
    {
        BadgeDotPosition.TopRight => "bb:-top-1 bb:-right-1",
        BadgeDotPosition.TopLeft => "bb:-top-1 bb:-left-1",
        BadgeDotPosition.BottomRight => "bb:-bottom-1 bb:-right-1",
        BadgeDotPosition.BottomLeft => "bb:-bottom-1 bb:-left-1",
        _ => "bb:-top-1 bb:-right-1"
    };

    private string DotCssClass => ClassNames.cn(
        "bb:absolute bb:block bb:h-2 bb:w-2 bb:rounded-full bb:ring-2 bb:ring-background",
        DotPositionClass,
        DotClass ?? "bb:bg-primary"
    );
}
