using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Layout container for full-width header section within an Item.
/// </summary>
/// <remarks>
/// ItemHeader uses flex basis-full for full-width layout with
/// space-between justification.
/// </remarks>
public partial class BbItemHeader : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the header.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered in the header.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the header element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        "flex basis-full items-center justify-between",
        Class
    );
}
