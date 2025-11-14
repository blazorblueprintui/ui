using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Card;

/// <summary>
/// The footer section of a card component.
/// </summary>
/// <remarks>
/// <para>
/// CardFooter provides a distinct bottom section for card content, typically containing
/// action buttons, links, or additional information. Uses flexbox for flexible layouts.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;CardFooter&gt;
///     &lt;Button&gt;Submit&lt;/Button&gt;
///     &lt;Button Variant="ButtonVariant.Outline"&gt;Cancel&lt;/Button&gt;
/// &lt;/CardFooter&gt;
/// </code>
/// </example>
public partial class CardFooter : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered inside the card footer.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the card footer.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the card footer element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base footer styles (from shadcn/ui)
        "flex items-center p-6 pt-0",
        // Custom classes (if provided)
        Class
    );
}
