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
        "bb:absolute bb:z-10 bb:flex bb:w-fit bb:shrink-0 bb:items-center bb:justify-center bb:gap-1 bb:rounded-full bb:bg-muted bb:px-1.5 bb:py-0.5 bb:text-sm bb:ring-3 bb:ring-card bb:has-[button]:p-0",
        Side == BubbleReactionsSide.Top ? "bb:top-0 bb:-translate-y-3/4" : "bb:bottom-0 bb:translate-y-3/4",
        Align == BubbleReactionsAlign.Start ? "bb:left-3" : "bb:right-3",
        Class
    );
}
