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
        "relative flex shrink-0 overflow-hidden rounded-full",
        Size switch
        {
            AvatarSize.Small => "h-8 w-8 text-xs",
            AvatarSize.Default => "h-10 w-10 text-sm",
            AvatarSize.Large => "h-12 w-12 text-base",
            AvatarSize.ExtraLarge => "h-16 w-16 text-lg",
            _ => "h-10 w-10 text-sm"
        },
        AvatarGroupContext != null ? "border-2 border-background" : null,
        Class
    );

    private string DotSizeClass => Size switch
    {
        AvatarSize.Small => "h-2 w-2",
        AvatarSize.Default => "h-2.5 w-2.5",
        AvatarSize.Large => "h-3 w-3",
        AvatarSize.ExtraLarge => "h-3.5 w-3.5",
        _ => "h-2.5 w-2.5"
    };

    private string DotCssClass => ClassNames.cn(
        "absolute bottom-0 right-0 block rounded-full ring-2 ring-background",
        DotSizeClass,
        DotClass ?? "bg-primary"
    );
}
