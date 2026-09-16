using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// An avatar component that displays a user image with fallback support.
/// </summary>
/// <remarks>
/// <para>
/// The Avatar component provides a circular container for user images that follows
/// the shadcn/ui design system. It supports automatic fallback to initials or icons
/// when images fail to load or are unavailable.
/// </para>
/// <para>
/// Features:
/// - Multiple size variants (Small, Default, Large, ExtraLarge)
/// - Automatic image fallback handling
/// - Accessible with proper ARIA labels
/// - Supports initials, images, and custom content
/// - Dark mode compatible via CSS variables
/// - RTL (Right-to-Left) support
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Avatar&gt;
///     &lt;AvatarImage Source="https://example.com/avatar.jpg" Alt="User Name" /&gt;
///     &lt;AvatarFallback&gt;UN&lt;/AvatarFallback&gt;
/// &lt;/Avatar&gt;
///
/// &lt;Avatar Size="AvatarSize.Large"&gt;
///     &lt;AvatarFallback&gt;JD&lt;/AvatarFallback&gt;
/// &lt;/Avatar&gt;
/// </code>
/// </example>
public partial class BbAvatar : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to render inside the avatar.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the size variant of the avatar.
    /// </summary>
    [Parameter]
    public AvatarSize Size { get; set; } = AvatarSize.Default;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the avatar container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets whether to show a dot indicator on the avatar.
    /// </summary>
    [Parameter]
    public bool ShowDot { get; set; }

    /// <summary>
    /// Gets or sets custom CSS classes for the dot indicator (e.g., "bg-green-500" for online status).
    /// Defaults to "bg-primary" when not specified.
    /// </summary>
    [Parameter]
    public string? DotClass { get; set; }

    [CascadingParameter(Name = "AvatarGroup")]
    internal BbAvatarGroup? AvatarGroupContext { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:relative bb:flex bb:shrink-0 bb:overflow-hidden bb:rounded-full",
        Size switch
        {
            AvatarSize.Small => "bb:h-8 bb:w-8 bb:text-xs",
            AvatarSize.Default => "bb:h-10 bb:w-10 bb:text-sm",
            AvatarSize.Large => "bb:h-12 bb:w-12 bb:text-base",
            AvatarSize.ExtraLarge => "bb:h-16 bb:w-16 bb:text-lg",
            _ => "bb:h-10 bb:w-10 bb:text-sm"
        },
        AvatarGroupContext != null ? "bb:border-2 bb:border-background" : null,
        Class
    );

    private string DotSizeClass => Size switch
    {
        AvatarSize.Small => "bb:h-2 bb:w-2",
        AvatarSize.Default => "bb:h-2.5 bb:w-2.5",
        AvatarSize.Large => "bb:h-3 bb:w-3",
        AvatarSize.ExtraLarge => "bb:h-3.5 bb:w-3.5",
        _ => "bb:h-2.5 bb:w-2.5"
    };

    private string DotCssClass => ClassNames.cn(
        "bb:absolute bb:bottom-0 bb:right-0 bb:block bb:rounded-full bb:ring-2 bb:ring-background",
        DotSizeClass,
        DotClass ?? "bb:bg-primary"
    );
}
