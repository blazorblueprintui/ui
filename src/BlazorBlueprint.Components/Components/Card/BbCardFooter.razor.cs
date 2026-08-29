using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

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
public partial class BbCardFooter : ComponentBase
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
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

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
