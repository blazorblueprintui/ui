using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Groups consecutive bubbles from the same sender.
/// </summary>
public partial class BbBubbleGroup : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for the bubble group.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets grouped bubble content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the group container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn("flex min-w-0 flex-col gap-2", Class);
}
