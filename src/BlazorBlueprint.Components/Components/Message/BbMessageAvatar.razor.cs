using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Avatar slot for a message row.
/// </summary>
public partial class BbMessageAvatar : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for the avatar slot.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets avatar content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the avatar container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass =>
        ClassNames.cn(
            "bb:flex bb:w-fit bb:min-w-8 bb:shrink-0 bb:items-center bb:justify-center bb:self-end bb:overflow-hidden bb:rounded-full bb:bg-muted bb:group-has-data-[slot=message-footer]/message:-translate-y-8",
            Class);
}
