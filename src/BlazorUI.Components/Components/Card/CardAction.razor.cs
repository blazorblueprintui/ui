using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Card;

/// <summary>
/// An action area within the card header for buttons, links, or other interactive elements.
/// </summary>
/// <remarks>
/// <para>
/// CardAction provides a dedicated space for action elements like buttons or links
/// within the card header, typically positioned to the right of the title/description.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;CardAction&gt;
///     &lt;Button Size="ButtonSize.Small"&gt;Action&lt;/Button&gt;
/// &lt;/CardAction&gt;
/// </code>
/// </example>
public partial class CardAction : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered in the card action area.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the card action area.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the card action element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base action styles
        "flex items-center gap-2",
        // Custom classes (if provided)
        Class
    );
}
