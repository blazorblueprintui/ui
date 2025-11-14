using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.InputGroup;

/// <summary>
/// A text container for displaying static content within InputGroup addons.
/// </summary>
/// <remarks>
/// <para>
/// InputGroupText provides consistent styling for text content displayed alongside
/// inputs, such as currency symbols, URL prefixes, domain suffixes, or character counters.
/// </para>
/// <para>
/// Features:
/// - Muted text color for subtle appearance
/// - Proper spacing for icons and text
/// - Small text size for better proportions
/// - Flexible content support (text, icons, or both)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- Currency prefix --&gt;
/// &lt;InputGroup&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.InlineStart"&gt;
///         &lt;InputGroupText&gt;$&lt;/InputGroupText&gt;
///     &lt;/InputGroupAddon&gt;
///     &lt;InputGroupInput Type="InputType.Number" Placeholder="0.00" /&gt;
/// &lt;/InputGroup&gt;
///
/// &lt;!-- Character counter --&gt;
/// &lt;InputGroup&gt;
///     &lt;InputGroupTextarea Placeholder="Write a comment..." /&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.BlockEnd"&gt;
///         &lt;InputGroupText&gt;0 / 280&lt;/InputGroupText&gt;
///     &lt;/InputGroupAddon&gt;
/// &lt;/InputGroup&gt;
///
/// &lt;!-- URL prefix with icon --&gt;
/// &lt;InputGroup&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.InlineStart"&gt;
///         &lt;InputGroupText&gt;
///             &lt;GlobeIcon /&gt;
///             https://
///         &lt;/InputGroupText&gt;
///     &lt;/InputGroupAddon&gt;
///     &lt;InputGroupInput Placeholder="example.com" /&gt;
/// &lt;/InputGroup&gt;
/// </code>
/// </example>
public partial class InputGroupText : ComponentBase
{
    /// <summary>
    /// Gets or sets the child content (text or icons).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the text container.
    /// </summary>
    /// <remarks>
    /// Provides subtle, muted styling with proper spacing for icons.
    /// Uses smaller text size to complement input text.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base text styles
        "text-sm text-muted-foreground",
        "flex items-center gap-0.5", // Reduced gap for tighter spacing
        // Adjust icon sizing
        "[&>svg]:size-4",
        // Custom classes
        Class
    );
}
