using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Card;

/// <summary>
/// The title heading of a card component.
/// </summary>
/// <remarks>
/// <para>
/// CardTitle displays the main heading within a card header using semantic HTML (h3).
/// It follows typography hierarchy and provides appropriate text sizing and weight.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;CardTitle&gt;Card Title&lt;/CardTitle&gt;
/// </code>
/// </example>
public partial class CardTitle : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered as the card title.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the card title.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the card title element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base title styles (from shadcn/ui)
        "text-2xl font-semibold leading-none tracking-tight",
        // Custom classes (if provided)
        Class
    );
}
