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
        "w-fit max-w-full min-w-0 overflow-hidden rounded-3xl border border-transparent px-3 py-2.5 text-sm leading-relaxed wrap-break-word group-data-[align=end]/bubble:self-end [button]:text-left [button,a]:transition-colors [button,a]:outline-none [button,a]:focus-visible:border-ring [button,a]:focus-visible:ring-3 [button,a]:focus-visible:ring-ring/30 group-data-[variant=ghost]/bubble:border-0",
        Class
    );
}
