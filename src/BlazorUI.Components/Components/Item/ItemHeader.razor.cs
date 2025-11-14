using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Item;

/// <summary>
/// Layout container for full-width header section within an Item.
/// </summary>
/// <remarks>
/// ItemHeader uses flex basis-full for full-width layout with
/// space-between justification.
/// </remarks>
public partial class ItemHeader : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the header.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

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
