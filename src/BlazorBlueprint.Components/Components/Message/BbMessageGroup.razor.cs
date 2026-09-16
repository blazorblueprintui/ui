using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Groups consecutive message rows from the same sender.
/// </summary>
public partial class BbMessageGroup : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for the message group.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets grouped message content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the group container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn("bb:flex bb:min-w-0 bb:flex-col bb:gap-2", Class);
}
