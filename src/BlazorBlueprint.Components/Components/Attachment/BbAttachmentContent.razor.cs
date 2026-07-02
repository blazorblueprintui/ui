using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Content wrapper for attachment title and description.
/// </summary>
public partial class BbAttachmentContent : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for content wrapper.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets content for attachment metadata.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for content wrapper.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn("max-w-full min-w-0 flex-1 leading-tight group-data-[orientation=vertical]/attachment:px-1", Class);
}
