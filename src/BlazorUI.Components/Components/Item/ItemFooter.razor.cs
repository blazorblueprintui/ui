using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Item;

/// <summary>
/// Layout container for full-width footer section within an Item.
/// </summary>
/// <remarks>
/// ItemFooter uses flex basis-full for full-width layout with
/// space-between justification.
/// </remarks>
public partial class ItemFooter : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the footer.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered in the footer.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the footer element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        "flex basis-full items-center justify-between",
        Class
    );
}
