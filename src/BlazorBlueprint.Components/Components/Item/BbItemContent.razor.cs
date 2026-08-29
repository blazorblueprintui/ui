using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A flex container for managing internal spacing and layout of item information.
/// </summary>
/// <remarks>
/// ItemContent provides proper spacing and flex layout for title, description,
/// and other content within an Item.
/// </remarks>
public partial class BbItemContent : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the content container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the container.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the content element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        "flex flex-1 flex-col gap-1",
        Class
    );
}
