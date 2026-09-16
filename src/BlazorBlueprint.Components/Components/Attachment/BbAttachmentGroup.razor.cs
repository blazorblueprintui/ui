using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Lays out attachments in a horizontally scrollable snapping row.
/// </summary>
public partial class BbAttachmentGroup : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for the group container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets attachment items.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the group container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:flex bb:min-w-0 bb:scroll-fade-x bb:snap-x bb:snap-mandatory bb:scroll-px-1 bb:scrollbar-none bb:gap-3 bb:overflow-x-auto bb:overscroll-x-contain bb:py-1 bb:*:data-[slot=attachment]:flex-none bb:*:data-[slot=attachment]:snap-start",
        Class
    );
}
