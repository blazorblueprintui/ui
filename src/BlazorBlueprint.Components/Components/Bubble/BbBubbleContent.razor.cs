using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Content surface wrapper for <see cref="BbBubble"/>.
/// </summary>
public partial class BbBubbleContent : ComponentBase
{
    /// <summary>
    /// Gets or sets the element type to render. Defaults to Div, but automatically switches to Anchor when <see cref="Href"/> is provided.
    /// </summary>
    [Parameter]
    public BubbleContentElement AsChild { get; set; } = BubbleContentElement.Div;

    /// <summary>
    /// Gets or sets href when rendering an anchor element.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets custom classes for the bubble content.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets content inside the bubble surface.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the content element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private BubbleContentElement ResolvedElement =>
        AsChild == BubbleContentElement.Div && !string.IsNullOrEmpty(Href) ? BubbleContentElement.Anchor : AsChild;

    private string CssClass => ClassNames.cn(
        "bb:w-fit bb:max-w-full bb:min-w-0 bb:overflow-hidden bb:rounded-3xl bb:border bb:border-transparent bb:px-3 bb:py-2.5 bb:text-sm bb:leading-relaxed bb:wrap-break-word bb:group-data-[align=end]/bubble:self-end bb:[button]:text-left bb:[button,a]:transition-colors bb:[button,a]:outline-none bb:[button,a]:focus-visible:border-ring bb:[button,a]:focus-visible:ring-3 bb:[button,a]:focus-visible:ring-ring/30 bb:group-data-[variant=ghost]/bubble:border-0",
        Class
    );
}
