using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Card;

/// <summary>
/// The main content area of a card component.
/// </summary>
/// <remarks>
/// <para>
/// CardContent provides the primary content area within a card, positioned between
/// the header and footer. It uses consistent padding for content alignment.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;CardContent&gt;
///     &lt;p&gt;Your card content goes here&lt;/p&gt;
/// &lt;/CardContent&gt;
/// </code>
/// </example>
public partial class CardContent : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered inside the card content area.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the card content.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the card content element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base content styles (from shadcn/ui)
        "p-6 pt-0",
        // Custom classes (if provided)
        Class
    );
}
