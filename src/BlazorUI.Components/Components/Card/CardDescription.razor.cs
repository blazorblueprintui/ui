using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Card;

/// <summary>
/// The description text of a card component.
/// </summary>
/// <remarks>
/// <para>
/// CardDescription displays supplementary text within a card header, typically
/// providing context or additional information about the card content.
/// Uses muted text color to create visual hierarchy.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;CardDescription&gt;Additional information about the card&lt;/CardDescription&gt;
/// </code>
/// </example>
public partial class CardDescription : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered as the card description.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the card description.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the card description element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base description styles (from shadcn/ui)
        "text-sm text-muted-foreground",
        // Custom classes (if provided)
        Class
    );
}
