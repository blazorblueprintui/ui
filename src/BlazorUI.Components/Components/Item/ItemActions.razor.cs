using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Item;

/// <summary>
/// Container for action buttons or interactive elements positioned on the right.
/// </summary>
/// <remarks>
/// ItemActions provides flex layout and alignment for buttons, icons,
/// or other interactive elements within an Item.
/// </remarks>
public partial class ItemActions : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the actions container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the actions container.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the actions element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        "flex items-center gap-2",
        Class
    );
}
