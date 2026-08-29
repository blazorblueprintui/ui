using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Displays the primary text/title of an Item.
/// </summary>
/// <remarks>
/// ItemTitle uses font-medium styling with gap spacing for inline elements.
/// </remarks>
public partial class BbItemTitle : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the title.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered as the title.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the title element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        "flex items-center gap-2 font-medium leading-none",
        Class
    );
}
