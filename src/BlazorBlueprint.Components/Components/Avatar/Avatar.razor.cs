using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Avatar;

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
public partial class Avatar : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to render inside the avatar.
    /// </summary>
    /// <remarks>
    /// Typically contains AvatarImage and AvatarFallback components.
    /// The first successfully loaded content will be displayed.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the size variant of the avatar.
    /// </summary>
    /// <remarks>
    /// Controls the dimensions and font-size of the avatar.
    /// Default value is <see cref="AvatarSize.Default"/>.
    /// </remarks>
    [Parameter]
    public AvatarSize Size { get; set; } = AvatarSize.Default;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the avatar container.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the avatar container.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base avatar styles (rounded-full, overflow-hidden, etc.)
    /// - Size-specific classes (dimensions, font-size)
    /// - Custom classes from the Class parameter
    /// </remarks>
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
        Class
    );
}
