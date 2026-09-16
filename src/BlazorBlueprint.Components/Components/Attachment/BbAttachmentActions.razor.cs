using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Container for one or more attachment action controls.
/// </summary>
public partial class BbAttachmentActions : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for actions container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets action controls.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for actions container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:relative bb:z-20 bb:flex bb:shrink-0 bb:items-center bb:group-data-[orientation=vertical]/attachment:absolute bb:group-data-[orientation=vertical]/attachment:top-3 bb:group-data-[orientation=vertical]/attachment:right-3 bb:group-data-[orientation=vertical]/attachment:gap-1",
        Class);
}
