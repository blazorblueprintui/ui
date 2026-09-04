using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A horizontal divider element for separating items in a list.
/// </summary>
/// <remarks>
/// ItemSeparator uses the Separator component with specific margin utilities
/// to create visual separation between items.
/// </remarks>
public partial class BbItemSeparator : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the separator.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the separator element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        "my-1",
        Class
    );
}
