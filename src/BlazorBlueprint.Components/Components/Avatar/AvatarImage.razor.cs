using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Avatar;

/// <summary>
/// Displays an image within an Avatar component with automatic error handling.
/// </summary>
/// <remarks>
/// <para>
/// AvatarImage renders a user's profile picture or avatar image. If the image
/// fails to load, the component automatically hides itself and allows the
/// AvatarFallback sibling to display instead.
/// </para>
/// <para>
/// Features:
/// - Automatic error handling and fallback
/// - Accessible with alt text support
/// - Optimized image rendering (aspect-ratio, object-fit)
/// - Seamless integration with Avatar container
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Avatar&gt;
///     &lt;AvatarImage Source="https://example.com/user.jpg" Alt="John Doe" /&gt;
///     &lt;AvatarFallback&gt;JD&lt;/AvatarFallback&gt;
/// &lt;/Avatar&gt;
/// </code>
/// </example>
public partial class AvatarImage : ComponentBase
{
    /// <summary>
    /// Gets or sets the URL of the image to display.
    /// </summary>
    /// <remarks>
    /// Should be a valid image URL. If the image fails to load,
    /// the component will hide and defer to AvatarFallback.
    /// </remarks>
    [Parameter]
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the alternative text for the image.
    /// </summary>
    /// <remarks>
    /// Essential for accessibility. Screen readers use this to describe
    /// the image to visually impaired users. Should describe who the
    /// avatar represents (e.g., "John Doe", "User avatar").
    /// </remarks>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the image.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the image element.
    /// </summary>
    /// <remarks>
    /// Applies styles for proper sizing, positioning, and rendering
    /// within the circular avatar container.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        "absolute inset-0 aspect-square h-full w-full object-cover",
        Class
    );

    private bool isLoaded = true;
    private bool hasError;

    /// <summary>
    /// Handles image loading errors by hiding the image.
    /// </summary>
    /// <remarks>
    /// When an image fails to load (404, CORS error, etc.),
    /// this method sets hasError=true, causing the image to not render
    /// and allowing the AvatarFallback to display instead.
    /// </remarks>
    private void HandleError()
    {
        hasError = true;
        StateHasChanged();
    }

    /// <summary>
    /// Resets error state when Source parameter changes.
    /// </summary>
    protected override void OnParametersSet()
    {
        // Reset error state when source changes
        hasError = false;
        isLoaded = !string.IsNullOrWhiteSpace(Source);
    }
}
