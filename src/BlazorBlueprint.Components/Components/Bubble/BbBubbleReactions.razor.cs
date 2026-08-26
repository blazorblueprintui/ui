using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Displays reaction chips anchored to a bubble surface.
/// </summary>
public partial class BbBubbleReactions : ComponentBase
{
    /// <summary>
    /// Gets or sets which side of the bubble to anchor to.
    /// </summary>
    [Parameter]
    public BubbleReactionsSide Side { get; set; } = BubbleReactionsSide.Bottom;

    /// <summary>
    /// Gets or sets horizontal alignment along the bubble edge.
    /// </summary>
    [Parameter]
    public BubbleReactionsAlign Align { get; set; } = BubbleReactionsAlign.End;

    /// <summary>
    /// Gets or sets custom classes for the reactions row.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets reaction content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the reactions container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "absolute z-10 flex w-fit shrink-0 items-center justify-center gap-1 rounded-full bg-muted px-1.5 py-0.5 text-sm ring-3 ring-card has-[button]:p-0",
        Side == BubbleReactionsSide.Top ? "top-0 -translate-y-3/4" : "bottom-0 translate-y-3/4",
        Align == BubbleReactionsAlign.Start ? "left-3" : "right-3",
        Class
    );
}
